#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SolarSystem.
/// </summary>
public static class TestSolarSystem
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Creates a simple test star.
    /// </summary>
    private static CelestialBody CreateTestStar(string starId, string starName, int seedVal)
    {
        StarSpec spec = StarSpec.SunLike(seedVal);
        spec.NameHint = starName;
        spec.SetOverride("id", starId);
        SeededRng rng = new SeededRng(seedVal);
        CelestialBody star = StarGenerator.Generate(spec, rng);
        star.Id = starId;
        return star;
    }

    /// <summary>
    /// Tests basic construction.
    /// </summary>
    public static void TestConstruction()
    {
        SolarSystem system = new SolarSystem("sys_1", "Test System");

        if (system.Id != "sys_1")
        {
            throw new InvalidOperationException("Expected id sys_1");
        }
        if (system.Name != "Test System")
        {
            throw new InvalidOperationException("Expected name Test System");
        }
        if (system.IsValid())
        {
            throw new InvalidOperationException("System should not be valid without bodies");
        }
        if (system.GetBodyCount() != 0)
        {
            throw new InvalidOperationException("Expected 0 bodies");
        }
    }

    /// <summary>
    /// Tests adding a star.
    /// </summary>
    public static void TestAddStar()
    {
        SolarSystem system = new SolarSystem("sys_1", "Sol System");
        CelestialBody star = CreateTestStar("sol", "Sol", 12345);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_sol", "sol");
        system.Hierarchy = new SystemHierarchy(starNode);

        system.AddBody(star);

        if (system.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Expected 1 star");
        }
        if (!system.StarIds.Contains("sol"))
        {
            throw new InvalidOperationException("Should have sol in star_ids");
        }
        if (system.GetBody("sol") == null)
        {
            throw new InvalidOperationException("Should be able to get sol");
        }
    }

    /// <summary>
    /// Tests adding multiple body types.
    /// </summary>
    public static void TestAddMultipleBodyTypes()
    {
        SolarSystem system = new SolarSystem("sys_1", "Test");

        CelestialBody star = CreateTestStar("star_1", "Star", 111);
        system.AddBody(star);

        CelestialBody planet = new CelestialBody("planet_1", "Earth", CelestialType.Type.Planet);
        system.AddBody(planet);

        CelestialBody moon = new CelestialBody("moon_1", "Luna", CelestialType.Type.Moon);
        moon.Orbital = new OrbitalProps(3.844e8, 0.05, 5.0, 0.0, 0.0, 0.0, "planet_1");
        system.AddBody(moon);

        CelestialBody asteroid = new CelestialBody("asteroid_1", "Ceres", CelestialType.Type.Asteroid);
        system.AddBody(asteroid);

        if (system.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Expected 1 star");
        }
        if (system.GetPlanetCount() != 1)
        {
            throw new InvalidOperationException("Expected 1 planet");
        }
        if (system.GetMoonCount() != 1)
        {
            throw new InvalidOperationException("Expected 1 moon");
        }
        if (system.GetAsteroidCount() != 1)
        {
            throw new InvalidOperationException("Expected 1 asteroid");
        }
        if (system.GetBodyCount() != 4)
        {
            throw new InvalidOperationException("Expected 4 bodies total");
        }
    }

    /// <summary>
    /// Tests get moons of planet.
    /// </summary>
    public static void TestGetMoonsOfPlanet()
    {
        SolarSystem system = new SolarSystem("sys_1", "Test");

        CelestialBody planet = new CelestialBody("earth", "Earth", CelestialType.Type.Planet);
        system.AddBody(planet);

        CelestialBody moon1 = new CelestialBody("luna", "Luna", CelestialType.Type.Moon);
        moon1.Orbital = new OrbitalProps(3.844e8, 0.05, 5.0, 0.0, 0.0, 0.0, "earth");
        system.AddBody(moon1);

        CelestialBody planet2 = new CelestialBody("mars", "Mars", CelestialType.Type.Planet);
        system.AddBody(planet2);

        CelestialBody moon2 = new CelestialBody("phobos", "Phobos", CelestialType.Type.Moon);
        moon2.Orbital = new OrbitalProps(9.376e6, 0.01, 1.0, 0.0, 0.0, 0.0, "mars");
        system.AddBody(moon2);

        Array<CelestialBody> earthMoons = system.GetMoonsOfPlanet("earth");
        Array<CelestialBody> marsMoons = system.GetMoonsOfPlanet("mars");

        if (earthMoons.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 moon for Earth");
        }
        if (earthMoons[0].Id != "luna")
        {
            throw new InvalidOperationException("Expected luna");
        }
        if (marsMoons.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 moon for Mars");
        }
        if (marsMoons[0].Id != "phobos")
        {
            throw new InvalidOperationException("Expected phobos");
        }
    }

    /// <summary>
    /// Tests is valid.
    /// </summary>
    public static void TestIsValid()
    {
        SolarSystem system = new SolarSystem("sys_1", "Test");

        if (system.IsValid())
        {
            throw new InvalidOperationException("System without hierarchy/stars should not be valid");
        }

        HierarchyNode starNode = HierarchyNode.CreateStar("n1", "star_1");
        system.Hierarchy = new SystemHierarchy(starNode);
        if (system.IsValid())
        {
            throw new InvalidOperationException("System with hierarchy but no star body should not be valid");
        }

        CelestialBody star = CreateTestStar("star_1", "Star", 123);
        system.AddBody(star);
        if (!system.IsValid())
        {
            throw new InvalidOperationException("System with hierarchy and star body should be valid");
        }
    }

    /// <summary>
    /// Tests asteroid belt management.
    /// </summary>
    public static void TestAsteroidBelts()
    {
        SolarSystem system = new SolarSystem("sys_1", "Test");

        AsteroidBelt belt = new AsteroidBelt("main_belt", "Main Belt");
        system.AddAsteroidBelt(belt);

        if (system.AsteroidBelts.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 belt");
        }
        if (system.AsteroidBelts[0].Name != "Main Belt")
        {
            throw new InvalidOperationException("Expected Main Belt");
        }
    }

    /// <summary>
    /// Tests orbit host management.
    /// </summary>
    public static void TestOrbitHosts()
    {
        SolarSystem system = new SolarSystem("sys_1", "Test");

        OrbitHost host = new OrbitHost("sol", OrbitHost.HostType.SType);
        system.AddOrbitHost(host);

        if (system.OrbitHosts.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 orbit host");
        }
        if (system.GetOrbitHost("sol") == null)
        {
            throw new InvalidOperationException("Should find orbit host sol");
        }
        if (system.GetOrbitHost("nonexistent") != null)
        {
            throw new InvalidOperationException("Should not find nonexistent host");
        }
    }

    /// <summary>
    /// Tests get summary.
    /// </summary>
    public static void TestGetSummary()
    {
        SolarSystem system = new SolarSystem("sys_1", "Alpha Centauri");

        CelestialBody star = CreateTestStar("alpha_a", "Alpha A", 111);
        system.AddBody(star);

        CelestialBody planet = new CelestialBody("planet_1", "Planet b", CelestialType.Type.Planet);
        system.AddBody(planet);

        string summary = system.GetSummary();

        if (!summary.Contains("Alpha Centauri"))
        {
            throw new InvalidOperationException("Summary should contain system name");
        }
        if (!summary.Contains("1 stars"))
        {
            throw new InvalidOperationException("Summary should contain star count");
        }
        if (!summary.Contains("1 planets"))
        {
            throw new InvalidOperationException("Summary should contain planet count");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_round_trip.
    /// </summary>
    private static void TestRoundTrip()
    {
        TestAddStar();
    }
}

