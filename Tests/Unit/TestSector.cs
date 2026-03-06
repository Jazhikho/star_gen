#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for Sector class.
/// </summary>
public static class TestSector
{
    /// <summary>
    /// Tests sector creation.
    /// </summary>
    public static void TestSectorCreation()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = new Sector(galaxy, new Vector3I(1, 0, 2), new Vector3I(5, 5, 5));

        DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 2), sector.QuadrantCoords, "Quadrant coords should match");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(5, 5, 5), sector.SectorLocalCoords, "Sector coords should match");
        if (sector.IsGenerated())
        {
            throw new InvalidOperationException("Should not be generated initially");
        }
    }

    /// <summary>
    /// Tests sector world origin.
    /// </summary>
    public static void TestSectorWorldOrigin()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = new Sector(galaxy, new Vector3I(0, 0, 0), new Vector3I(3, 4, 5));

        Vector3 expected = GalaxyCoordinates.SectorWorldOrigin(new Vector3I(0, 0, 0), new Vector3I(3, 4, 5));
        if (!sector.WorldOrigin.IsEqualApprox(expected))
        {
            throw new InvalidOperationException("World origin should match");
        }
    }

    /// <summary>
    /// Tests get stars triggers generation.
    /// </summary>
    public static void TestGetStarsTriggersGeneration()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = new Sector(galaxy, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));

        if (sector.IsGenerated())
        {
            throw new InvalidOperationException("Should not be generated before get_stars");
        }
        Array<GalaxyStar> stars = sector.GetStars();
        if (!sector.IsGenerated())
        {
            throw new InvalidOperationException("Should be generated after get_stars");
        }
    }

    /// <summary>
    /// Tests get stars deterministic.
    /// </summary>
    public static void TestGetStarsDeterministic()
    {
        Galaxy galaxyA = Galaxy.CreateDefault(42);
        Galaxy galaxyB = Galaxy.CreateDefault(42);

        Sector sectorA = new Sector(galaxyA, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        Sector sectorB = new Sector(galaxyB, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));

        Array<GalaxyStar> starsA = sectorA.GetStars();
        Array<GalaxyStar> starsB = sectorB.GetStars();

        DotNetNativeTestSuite.AssertEqual(starsA.Count, starsB.Count, "Same inputs should give same star count");
        if (starsA.Count > 0)
        {
            DotNetNativeTestSuite.AssertEqual(starsA[0].StarSeed, starsB[0].StarSeed, "First star seed should match");
        }
    }

    /// <summary>
    /// Tests get star count.
    /// </summary>
    public static void TestGetStarCount()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = new Sector(galaxy, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));

        Array<GalaxyStar> stars = sector.GetStars();
        DotNetNativeTestSuite.AssertEqual(stars.Count, sector.GetStarCount(), "Star count should match array size");
    }

    /// <summary>
    /// Tests get stars in subsector.
    /// </summary>
    public static void TestGetStarsInSubsector()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = new Sector(galaxy, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));

        Array<GalaxyStar> allStars = sector.GetStars();

        int subsectorTotal = 0;
        for (int ssx = 0; ssx < 10; ssx++)
        {
            for (int ssy = 0; ssy < 10; ssy++)
            {
                for (int ssz = 0; ssz < 10; ssz++)
                {
                    Array<GalaxyStar> ssStars = sector.GetStarsInSubsector(new Vector3I(ssx, ssy, ssz));
                    subsectorTotal += ssStars.Count;
                }
            }
        }

        DotNetNativeTestSuite.AssertEqual(allStars.Count, subsectorTotal, "Sum of subsector stars should equal total");
    }

    /// <summary>
    /// Tests stars have correct subsector coords.
    /// </summary>
    public static void TestStarsHaveCorrectSubsectorCoords()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = new Sector(galaxy, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));

        Array<GalaxyStar> stars = sector.GetStarsInSubsector(new Vector3I(3, 3, 3));
        foreach (GalaxyStar star in stars)
        {
            DotNetNativeTestSuite.AssertEqual(new Vector3I(3, 3, 3), star.SubsectorCoords, "Star subsector coords should match");
        }
    }

    /// <summary>
    /// Tests regenerate clears and regenerates.
    /// </summary>
    public static void TestRegenerateClearsAndRegenerates()
    {
        Galaxy galaxy = Galaxy.CreateDefault(42);
        Sector sector = new Sector(galaxy, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));

        Array<GalaxyStar> starsBefore = sector.GetStars();
        int countBefore = starsBefore.Count;

        sector.Regenerate();

        Array<GalaxyStar> starsAfter = sector.GetStars();
        DotNetNativeTestSuite.AssertEqual(countBefore, starsAfter.Count, "Regeneration should produce same count");
    }

    /// <summary>
    /// Tests sector seed is deterministic.
    /// </summary>
    public static void TestSectorSeedIsDeterministic()
    {
        Galaxy galaxyA = Galaxy.CreateDefault(42);
        Galaxy galaxyB = Galaxy.CreateDefault(42);

        Sector sectorA = new Sector(galaxyA, new Vector3I(1, 2, 3), new Vector3I(4, 5, 6));
        Sector sectorB = new Sector(galaxyB, new Vector3I(1, 2, 3), new Vector3I(4, 5, 6));

        DotNetNativeTestSuite.AssertEqual(sectorA.SectorSeed, sectorB.SectorSeed, "Same inputs should give same sector seed");

        Sector sectorC = new Sector(galaxyA, new Vector3I(1, 2, 3), new Vector3I(7, 8, 9));
        if (sectorA.SectorSeed == sectorC.SectorSeed)
        {
            throw new InvalidOperationException("Different coords should give different seed");
        }
    }
}
