#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemSerializer.
/// </summary>
public static class TestSystemSerializer
{
    private const double DefaultTolerance = 1.0;

    /// <summary>
    /// Creates a test system with various components.
    /// </summary>
    private static SolarSystem CreateTestSystem()
    {
        SolarSystem system = new SolarSystem("test_sys", "Test System");

        StarSpec starSpec = StarSpec.SunLike(12345);
        SeededRng starRng = new SeededRng(12345);
        CelestialBody star = StarGenerator.Generate(starSpec, starRng);
        star.Id = "star_1";
        system.AddBody(star);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star", "star_1");
        system.Hierarchy = new SystemHierarchy(starNode);

        CelestialBody planet = new CelestialBody("planet_1", "Test Planet", CelestialType.Type.Planet);
        planet.Physical = new PhysicalProps(Units.EarthMassKg, Units.EarthRadiusMeters, 86400.0, 23.5);
        planet.Orbital = new OrbitalProps(Units.AuMeters, 0.017, 0.0, 0.0, 0.0, 0.0, "node_star");
        system.AddBody(planet);

        CelestialBody moon = new CelestialBody("moon_1", "Test Moon", CelestialType.Type.Moon);
        moon.Physical = new PhysicalProps(7.34e22, 1.74e6, 2360592.0);
        moon.Orbital = new OrbitalProps(3.84e8, 0.05, 5.0, 0.0, 0.0, 0.0, "planet_1");
        system.AddBody(moon);

        OrbitHost host = new OrbitHost("node_star", OrbitHost.HostType.SType);
        host.CombinedMassKg = star.Physical.MassKg;
        host.InnerStabilityM = 0.1 * Units.AuMeters;
        host.OuterStabilityM = 50.0 * Units.AuMeters;
        system.AddOrbitHost(host);

        AsteroidBelt belt = new AsteroidBelt("belt_1", "Main Belt");
        belt.OrbitHostId = "node_star";
        belt.InnerRadiusM = 2.2 * Units.AuMeters;
        belt.OuterRadiusM = 3.2 * Units.AuMeters;
        belt.TotalMassKg = 3.0e21;
        belt.PrimaryComposition = AsteroidBelt.Composition.Rocky;
        system.AddAsteroidBelt(belt);

        system.Provenance = new Provenance(12345, "0.4.1.1", 1, 1234567890, new Godot.Collections.Dictionary { { "test", true } });

        return system;
    }

    /// <summary>
    /// Tests serialization round-trip via dictionary.
    /// </summary>
    public static void TestRoundTripDict()
    {
        SolarSystem original = CreateTestSystem();

        Godot.Collections.Dictionary data = SystemSerializer.ToDictionary(original);
        SolarSystem restored = SystemSerializer.FromDictionary(data);

        if (restored == null)
        {
            throw new InvalidOperationException("Should deserialize successfully");
        }
        if (restored.Id != original.Id)
        {
            throw new InvalidOperationException("ID should match");
        }
        if (restored.Name != original.Name)
        {
            throw new InvalidOperationException("Name should match");
        }
        if (restored.StarIds.Count != original.StarIds.Count)
        {
            throw new InvalidOperationException("Star IDs count should match");
        }
        if (restored.PlanetIds.Count != original.PlanetIds.Count)
        {
            throw new InvalidOperationException("Planet IDs count should match");
        }
        if (restored.MoonIds.Count != original.MoonIds.Count)
        {
            throw new InvalidOperationException("Moon IDs count should match");
        }
        if (restored.AsteroidBelts.Count != original.AsteroidBelts.Count)
        {
            throw new InvalidOperationException("Asteroid belts count should match");
        }
        if (restored.OrbitHosts.Count != original.OrbitHosts.Count)
        {
            throw new InvalidOperationException("Orbit hosts count should match");
        }
    }

    /// <summary>
    /// Tests serialization round-trip via JSON.
    /// </summary>
    public static void TestRoundTripJson()
    {
        SolarSystem original = CreateTestSystem();

        string jsonStr = SystemSerializer.ToJson(original);
        SolarSystem restored = SystemSerializer.FromJson(jsonStr);

        if (restored == null)
        {
            throw new InvalidOperationException("Should deserialize from JSON");
        }
        if (restored.Id != original.Id)
        {
            throw new InvalidOperationException("ID should match");
        }
    }

    /// <summary>
    /// Tests hierarchy is preserved.
    /// </summary>
    public static void TestHierarchyPreserved()
    {
        SolarSystem original = CreateTestSystem();

        Godot.Collections.Dictionary data = SystemSerializer.ToDictionary(original);
        SolarSystem restored = SystemSerializer.FromDictionary(data);

        if (restored.Hierarchy == null)
        {
            throw new InvalidOperationException("Hierarchy should be preserved");
        }
        if (!restored.Hierarchy.IsValid())
        {
            throw new InvalidOperationException("Hierarchy should be valid");
        }
        if (restored.Hierarchy.GetStarCount() != original.Hierarchy.GetStarCount())
        {
            throw new InvalidOperationException("Star count should match");
        }
    }

    /// <summary>
    /// Tests bodies are preserved.
    /// </summary>
    public static void TestBodiesPreserved()
    {
        SolarSystem original = CreateTestSystem();

        Godot.Collections.Dictionary data = SystemSerializer.ToDictionary(original);
        SolarSystem restored = SystemSerializer.FromDictionary(data);

        foreach (string bodyId in original.Bodies.Keys)
        {
            if (!restored.Bodies.ContainsKey(bodyId))
            {
                throw new InvalidOperationException($"Body {bodyId} should be preserved");
            }

            CelestialBody origBody = (CelestialBody)original.Bodies[bodyId];
            CelestialBody restBody = (CelestialBody)restored.Bodies[bodyId];

            if (restBody.Type != origBody.Type)
            {
                throw new InvalidOperationException("Body type should match");
            }
            if (System.Math.Abs(restBody.Physical.MassKg - origBody.Physical.MassKg) > DefaultTolerance)
            {
                throw new InvalidOperationException("Body mass should match");
            }
        }
    }

    /// <summary>
    /// Tests orbit hosts are preserved.
    /// </summary>
    public static void TestOrbitHostsPreserved()
    {
        SolarSystem original = CreateTestSystem();

        Godot.Collections.Dictionary data = SystemSerializer.ToDictionary(original);
        SolarSystem restored = SystemSerializer.FromDictionary(data);

        if (restored.OrbitHosts.Count != original.OrbitHosts.Count)
        {
            throw new InvalidOperationException("Orbit hosts count should match");
        }

        for (int i = 0; i < original.OrbitHosts.Count; i++)
        {
            OrbitHost origHost = original.OrbitHosts[i];
            OrbitHost restHost = restored.OrbitHosts[i];

            if (restHost.NodeId != origHost.NodeId)
            {
                throw new InvalidOperationException("Node ID should match");
            }
            if (System.Math.Abs(restHost.CombinedMassKg - origHost.CombinedMassKg) > DefaultTolerance)
            {
                throw new InvalidOperationException("Combined mass should match");
            }
        }
    }

    /// <summary>
    /// Tests asteroid belts are preserved.
    /// </summary>
    public static void TestAsteroidBeltsPreserved()
    {
        SolarSystem original = CreateTestSystem();

        Godot.Collections.Dictionary data = SystemSerializer.ToDictionary(original);
        SolarSystem restored = SystemSerializer.FromDictionary(data);

        if (restored.AsteroidBelts.Count != original.AsteroidBelts.Count)
        {
            throw new InvalidOperationException("Asteroid belts count should match");
        }

        AsteroidBelt origBelt = original.AsteroidBelts[0];
        AsteroidBelt restBelt = restored.AsteroidBelts[0];

        if (restBelt.Id != origBelt.Id)
        {
            throw new InvalidOperationException("Belt ID should match");
        }
        if (restBelt.PrimaryComposition != origBelt.PrimaryComposition)
        {
            throw new InvalidOperationException("Composition should match");
        }
        if (System.Math.Abs(restBelt.InnerRadiusM - origBelt.InnerRadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Inner radius should match");
        }
    }

    /// <summary>
    /// Tests provenance is preserved.
    /// </summary>
    public static void TestProvenancePreserved()
    {
        SolarSystem original = CreateTestSystem();

        Godot.Collections.Dictionary data = SystemSerializer.ToDictionary(original);
        SolarSystem restored = SystemSerializer.FromDictionary(data);

        if (restored.Provenance == null)
        {
            throw new InvalidOperationException("Provenance should be preserved");
        }
        if (restored.Provenance.GenerationSeed != original.Provenance.GenerationSeed)
        {
            throw new InvalidOperationException("Generation seed should match");
        }
        if (restored.Provenance.GeneratorVersion != original.Provenance.GeneratorVersion)
        {
            throw new InvalidOperationException("Generator version should match");
        }
    }

    /// <summary>
    /// Tests deserialization of invalid data returns null.
    /// </summary>
    public static void TestInvalidDataReturnsNull()
    {
        SolarSystem result = SystemSerializer.FromDictionary(new Godot.Collections.Dictionary());
        if (result != null)
        {
            throw new InvalidOperationException("Empty dict should return null");
        }
    }

    /// <summary>
    /// Tests invalid JSON returns null.
    /// </summary>
    public static void TestInvalidJsonReturnsNull()
    {
        SolarSystem result = SystemSerializer.FromJson("not valid json");
        if (result != null)
        {
            throw new InvalidOperationException("Invalid JSON should return null");
        }
    }

    /// <summary>
    /// Tests schema version is included.
    /// </summary>
    public static void TestSchemaVersionIncluded()
    {
        SolarSystem system = CreateTestSystem();
        Godot.Collections.Dictionary data = SystemSerializer.ToDictionary(system);

        if (!data.ContainsKey("schema_version"))
        {
            throw new InvalidOperationException("Should include schema version");
        }
        if (!data.ContainsKey("generator_version"))
        {
            throw new InvalidOperationException("Should include generator version");
        }
        if (!data.ContainsKey("type"))
        {
            throw new InvalidOperationException("Should include type");
        }
        if (data["type"].AsString() != "solar_system")
        {
            throw new InvalidOperationException("Type should be solar_system");
        }
    }
}
