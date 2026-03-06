#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for Galaxy class.
/// </summary>
public static class TestGalaxy
{
    /// <summary>
    /// Tests create default.
    /// </summary>
    public static void TestCreateDefault()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        DotNetNativeTestSuite.AssertNotNull(galaxy, "Should create galaxy");
        DotNetNativeTestSuite.AssertEqual(42, galaxy.GalaxySeed, "Seed should match");
        DotNetNativeTestSuite.AssertNotNull(galaxy.Spec, "Should have spec");
        DotNetNativeTestSuite.AssertNotNull(galaxy.Config, "Should have config");
        DotNetNativeTestSuite.AssertNotNull(galaxy.DensityModel, "Should have density model");
        if (galaxy.ReferenceDensity <= 0.0)
        {
            throw new InvalidOperationException("Reference density should be positive");
        }
    }

    /// <summary>
    /// Tests create with config.
    /// </summary>
    public static void TestCreateWithConfig()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.NumArms = 2;
        Galaxy galaxy = new Galaxy(config, 123);
        DotNetNativeTestSuite.AssertEqual(123, galaxy.GalaxySeed, "Seed should match");
        DotNetNativeTestSuite.AssertEqual(2, galaxy.Config.NumArms, "Config should be applied");
    }

    /// <summary>
    /// Tests get sector creates sector.
    /// </summary>
    public static void TestGetSectorCreatesSector()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = galaxy.GetSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        DotNetNativeTestSuite.AssertNotNull(sector, "Should create sector");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 0, 0), sector.QuadrantCoords, "Quadrant coords should match");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(5, 5, 5), sector.SectorLocalCoords, "Sector coords should match");
    }

    /// <summary>
    /// Tests get sector caches sector.
    /// </summary>
    public static void TestGetSectorCachesSector()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sectorA = galaxy.GetSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        Sector sectorB = galaxy.GetSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        DotNetNativeTestSuite.AssertEqual(sectorA, sectorB, "Should return same cached sector");
        DotNetNativeTestSuite.AssertEqual(1, galaxy.GetCachedSectorCount(), "Should have one cached sector");
    }

    /// <summary>
    /// Tests get sector different coords different sectors.
    /// </summary>
    public static void TestGetSectorDifferentCoordsDifferentSectors()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sectorA = galaxy.GetSector(new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));
        Sector sectorB = galaxy.GetSector(new Vector3I(0, 0, 0), new Vector3I(1, 0, 0));
        if (sectorA == sectorB)
        {
            throw new InvalidOperationException("Different coords should give different sectors");
        }
        DotNetNativeTestSuite.AssertEqual(2, galaxy.GetCachedSectorCount(), "Should have two cached sectors");
    }

    /// <summary>
    /// Tests get sector at position.
    /// </summary>
    public static void TestGetSectorAtPosition()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Vector3 pos = new Vector3(550.0f, 550.0f, 550.0f);
        Sector sector = galaxy.GetSectorAtPosition(pos);
        DotNetNativeTestSuite.AssertNotNull(sector, "Should find sector");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 0, 0), sector.QuadrantCoords, "Should be in quadrant 0");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(5, 5, 5), sector.SectorLocalCoords, "Should be in sector 5,5,5");
    }

    /// <summary>
    /// Tests get stars in sector.
    /// </summary>
    public static void TestGetStarsInSector()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Array<GalaxyStar> stars = galaxy.GetStarsInSector(new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));
        if (stars.Count <= 0)
        {
            throw new InvalidOperationException("Should have stars near center");
        }
    }

    /// <summary>
    /// Tests get stars in sector deterministic.
    /// </summary>
    public static void TestGetStarsInSectorDeterministic()
    {
        Galaxy galaxyA = Galaxy.CreateDefault(42);
        Galaxy galaxyB = Galaxy.CreateDefault(42);
        Array<GalaxyStar> starsA = galaxyA.GetStarsInSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        Array<GalaxyStar> starsB = galaxyB.GetStarsInSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        DotNetNativeTestSuite.AssertEqual(starsA.Count, starsB.Count, "Same inputs should give same star count");
        if (starsA.Count > 0)
        {
            DotNetNativeTestSuite.AssertEqual(starsA[0].StarSeed, starsB[0].StarSeed, "First star seed should match");
            if (!starsA[0].Position.IsEqualApprox(starsB[0].Position))
            {
                throw new InvalidOperationException("Same seed should give same positions");
            }
        }
    }

    /// <summary>
    /// Tests get stars in subsector.
    /// </summary>
    public static void TestGetStarsInSubsector()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Array<GalaxyStar> stars = galaxy.GetStarsInSubsector(
            new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), new Vector3I(5, 5, 5)
        );
        DotNetNativeTestSuite.AssertNotNull(stars, "Should return array (possibly empty)");
    }

    /// <summary>
    /// Tests cache system.
    /// </summary>
    public static void TestCacheSystem()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        SolarSystem mockSystem = new SolarSystem("test_id", "Test System");
        galaxy.CacheSystem(12345, mockSystem);
        if (!galaxy.HasCachedSystem(12345))
        {
            throw new InvalidOperationException("Should have cached system");
        }
        DotNetNativeTestSuite.AssertEqual(mockSystem, galaxy.GetCachedSystem(12345), "Should return cached system");
        DotNetNativeTestSuite.AssertEqual(1, galaxy.GetCachedSystemCount(), "Should have one cached system");
    }

    /// <summary>
    /// Tests get cached system returns null for missing.
    /// </summary>
    public static void TestGetCachedSystemReturnsNullForMissing()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        DotNetNativeTestSuite.AssertNull(galaxy.GetCachedSystem(99999), "Should return null for uncached seed");
        if (galaxy.HasCachedSystem(99999))
        {
            throw new InvalidOperationException("Should report not cached");
        }
    }

    /// <summary>
    /// Tests clear cache.
    /// </summary>
    public static void TestClearCache()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = galaxy.GetSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        SolarSystem mockSystem = new SolarSystem("test_id", "Test System");
        galaxy.CacheSystem(12345, mockSystem);
        DotNetNativeTestSuite.AssertEqual(1, galaxy.GetCachedSectorCount(), "Should have cached sector");
        DotNetNativeTestSuite.AssertEqual(1, galaxy.GetCachedSystemCount(), "Should have cached system");

        galaxy.ClearCache();
        DotNetNativeTestSuite.AssertEqual(0, galaxy.GetCachedSectorCount(), "Should clear sectors");
        DotNetNativeTestSuite.AssertEqual(0, galaxy.GetCachedSystemCount(), "Should clear systems");
    }

    /// <summary>
    /// Tests to dict and from dict.
    /// </summary>
    public static void TestToDictAndFromDict()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.NumArms = 3;
        Galaxy galaxy = new Galaxy(config, 999);

        Godot.Collections.Dictionary dict = galaxy.ToDictionary();
        DotNetNativeTestSuite.AssertEqual(999, dict["seed"].AsInt32(), "Dict should contain seed");
        if (!dict.ContainsKey("config"))
        {
            throw new InvalidOperationException("Dict should contain config");
        }

        Galaxy restored = Galaxy.FromDictionary(dict);
        DotNetNativeTestSuite.AssertEqual(999, restored.GalaxySeed, "Restored seed should match");
        DotNetNativeTestSuite.AssertEqual(3, restored.Config.NumArms, "Restored config should match");
    }

    /// <summary>
    /// Tests get stars in radius.
    /// </summary>
    public static void TestGetStarsInRadius()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Vector3 center = new Vector3(50.0f, 50.0f, 50.0f);
        float radiusPc = 30.0f;
        Array<GalaxyStar> stars = galaxy.GetStarsInRadius(center, radiusPc);
        foreach (GalaxyStar star in stars)
        {
            float dist = star.Position.DistanceTo(center);
            if (dist >= radiusPc + 0.1)
            {
                throw new InvalidOperationException("Star should be within radius");
            }
        }
    }

    /// <summary>
    /// Tests different galaxy seeds different stars.
    /// </summary>
    public static void TestDifferentGalaxySeedsDifferentStars()
    {
        Galaxy galaxyA = Galaxy.CreateDefault(42);
        Galaxy galaxyB = Galaxy.CreateDefault(999);
        Array<GalaxyStar> starsA = galaxyA.GetStarsInSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        Array<GalaxyStar> starsB = galaxyB.GetStarsInSector(new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));

        bool anyDifferent = starsA.Count != starsB.Count;
        if (!anyDifferent && starsA.Count > 0)
        {
            anyDifferent = starsA[0].StarSeed != starsB[0].StarSeed;
        }
        if (!anyDifferent)
        {
            throw new InvalidOperationException("Different galaxy seeds should give different stars");
        }
    }

    /// <summary>
    /// Tests stars have metallicity.
    /// </summary>
    public static void TestStarsHaveMetallicity()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Array<GalaxyStar> stars = galaxy.GetStarsInSector(new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));
        if (stars.Count > 0)
        {
            GalaxyStar star = stars[0];
            if (star.Metallicity <= 0.0)
            {
                throw new InvalidOperationException("Metallicity should be positive");
            }
            if (star.Metallicity >= 10.0)
            {
                throw new InvalidOperationException("Metallicity should be reasonable");
            }
            if (star.AgeBias <= 0.0)
            {
                throw new InvalidOperationException("Age bias should be positive");
            }
        }
    }
}
