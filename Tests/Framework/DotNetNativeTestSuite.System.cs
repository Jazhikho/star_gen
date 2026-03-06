#nullable enable annotations
#nullable disable warnings
using System.IO;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Services.Persistence;

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Verifies the key Kepler conversions in the migrated orbital-mechanics helper.
    /// </summary>
    private static void TestOrbitalMechanicsPeriodAxisRoundTrip()
    {
        double originalAxis = Units.AuMeters;
        double period = OrbitalMechanics.CalculateOrbitalPeriod(originalAxis, Units.SolarMassKg);
        double rebuiltAxis = OrbitalMechanics.CalculateSemiMajorAxis(period, Units.SolarMassKg);

        AssertTrue(period > 0.0, "orbital period should be positive for valid inputs");
        AssertFloatNear(originalAxis, rebuiltAxis, Units.AuMeters * 1.0e-9, "period/axis conversions should round-trip");
        AssertTrue(
            OrbitalMechanics.CalculateOrbitalVelocity(originalAxis, Units.SolarMassKg) > 0.0,
            "orbital velocity should be positive for valid inputs");
    }

    /// <summary>
    /// Verifies orbit-host zone calculations and dictionary round-tripping.
    /// </summary>
    private static void TestOrbitHostZoneCalculationAndRoundTrip()
    {
        OrbitHost host = new("primary", OrbitHost.HostType.SType)
        {
            CombinedMassKg = Units.SolarMassKg,
            CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts,
            EffectiveTemperatureK = 5778.0,
            InnerStabilityM = 0.5 * Units.AuMeters,
            OuterStabilityM = 5.0 * Units.AuMeters,
        };
        host.CalculateZones();

        AssertTrue(host.HasValidZone(), "fixture host should expose a valid stable zone");
        AssertEqual("S-type", host.GetTypeString(), "host type string should match the stored type");
        AssertTrue(host.IsDistanceStable(Units.AuMeters), "1 AU should lie inside the fixture stable zone");
        AssertTrue(host.IsDistanceHabitable(Units.AuMeters), "1 AU should lie inside the solar-luminosity habitable zone");
        AssertTrue(host.IsBeyondFrostLine(3.0 * Units.AuMeters), "3 AU should lie beyond the nominal frost line");

        Godot.Collections.Dictionary data = host.ToDictionary();
        OrbitHost rebuilt = OrbitHost.FromDictionary(data);
        AssertVariantDeepEqual(data, rebuilt.ToDictionary(), "orbit host should round-trip semantically unchanged");
    }

    /// <summary>
    /// Verifies solar-system spec presets and dictionary round-tripping.
    /// </summary>
    private static void TestSolarSystemSpecPresetsAndRoundTrip()
    {
        SolarSystemSpec spec = SolarSystemSpec.AlphaCentauriLike(135_791);
        spec.NameHint = "Alpha";
        spec.GeneratePopulation = true;
        spec.IncludeAsteroidBelts = false;
        spec.SetOverride("viewer.force_binary", true);

        AssertEqual(3, spec.StarCountMin, "Alpha-Centauri preset should request three stars");
        AssertEqual(3, spec.StarCountMax, "Alpha-Centauri preset should cap at three stars");
        AssertTrue(spec.HasOverride("viewer.force_binary"), "set overrides should be addressable");

        Godot.Collections.Dictionary data = spec.ToDictionary();
        SolarSystemSpec rebuilt = SolarSystemSpec.FromDictionary(data);
        AssertVariantDeepEqual(data, rebuilt.ToDictionary(), "solar-system spec should round-trip semantically unchanged");

        SolarSystemSpec sunLike = SolarSystemSpec.SunLike(246_802);
        AssertEqual(1, sunLike.StarCountMin, "Sun-like preset should request one star");
        AssertEqual(1, sunLike.SpectralClassHints.Count, "Sun-like preset should seed one spectral hint");
    }

    /// <summary>
    /// Verifies hierarchy-node traversal and dictionary round-tripping.
    /// </summary>
    private static void TestHierarchyNodeRoundTripAndTreeQueries()
    {
        HierarchyNode left = HierarchyNode.CreateStar("node_a", "star_a");
        HierarchyNode right = HierarchyNode.CreateStar("node_b", "star_b");
        HierarchyNode root = HierarchyNode.CreateBarycenter("root", left, right, Units.AuMeters * 10.0, 0.1);

        AssertTrue(root.IsBarycenter(), "root should be a barycenter");
        AssertEqual(2, root.GetStarCount(), "barycenter subtree should contain both stars");
        AssertEqual(2, root.GetDepth(), "binary hierarchy depth should be two");
        AssertEqual(2, root.GetAllStarIds().Count, "tree should expose both star ids");
        AssertNotNull(root.FindNode("node_b"), "tree queries should find nested nodes");

        Godot.Collections.Dictionary data = root.ToDictionary();
        HierarchyNode? rebuilt = HierarchyNode.FromDictionary(data);
        AssertNotNull(rebuilt, "hierarchy node should rebuild from dictionary payload");
        AssertVariantDeepEqual(data, rebuilt!.ToDictionary(), "hierarchy node should round-trip semantically unchanged");
    }

    /// <summary>
    /// Verifies system-hierarchy traversal and dictionary round-tripping.
    /// </summary>
    private static void TestSystemHierarchyRoundTripAndNodeFilters()
    {
        HierarchyNode root = HierarchyNode.CreateBarycenter(
            "root",
            HierarchyNode.CreateStar("node_a", "star_a"),
            HierarchyNode.CreateStar("node_b", "star_b"),
            Units.AuMeters * 20.0);
        SystemHierarchy hierarchy = new(root);

        AssertTrue(hierarchy.IsValid(), "hierarchy with a root should be valid");
        AssertEqual(2, hierarchy.GetStarCount(), "hierarchy should count both stars");
        AssertEqual(3, hierarchy.GetAllNodes().Count, "hierarchy should flatten root plus children");
        AssertEqual(1, hierarchy.GetAllBarycenters().Count, "hierarchy should identify the barycenter");
        AssertEqual(2, hierarchy.GetAllStarNodes().Count, "hierarchy should identify the leaf stars");

        Godot.Collections.Dictionary data = hierarchy.ToDictionary();
        SystemHierarchy rebuilt = SystemHierarchy.FromDictionary(data);
        AssertVariantDeepEqual(data, rebuilt.ToDictionary(), "system hierarchy should round-trip semantically unchanged");
    }

    /// <summary>
    /// Verifies solar-system indexes update as bodies are added.
    /// </summary>
    private static void TestSolarSystemAddBodyUpdatesIndexes()
    {
        SolarSystem system = new("sys_1", "Fixture");
        CelestialBody star = StarGenerator.Generate(StarSpec.SunLike(111_222), new SeededRng(222_333));
        CelestialBody planet = PlanetGenerator.Generate(
            PlanetSpec.EarthLike(333_444),
            CreateFixturePlanetContext(),
            new SeededRng(444_555),
            enablePopulation: false);

        system.AddBody(star);
        system.AddBody(planet);

        AssertEqual(2, system.GetBodyCount(), "solar system should store both added bodies");
        AssertEqual(1, system.GetStarCount(), "solar system should index star bodies");
        AssertEqual(1, system.GetPlanetCount(), "solar system should index planet bodies");
        AssertEqual(1, system.GetStars().Count, "star lookup should return the added star");
        AssertEqual(1, system.GetPlanets().Count, "planet lookup should return the added planet");
        AssertNotNull(system.GetBody(star.Id), "body lookup should find the stored star");
    }

    /// <summary>
    /// Verifies basic cache put/get/evict semantics.
    /// </summary>
    private static void TestSystemCachePutGetAndEvict()
    {
        SystemCache cache = new();
        int seed = 112233;
        SolarSystem? system = GalaxySystemGenerator.GenerateSystem(CreateFixtureGalaxyStar(), includeAsteroids: false, enablePopulation: false);
        AssertNotNull(system, "fixture galaxy star should generate a system for cache testing");

        AssertTrue(!cache.HasSystem(seed), "fresh cache should start empty");
        cache.PutSystem(seed, system!);
        AssertTrue(cache.HasSystem(seed), "stored systems should become addressable by seed");
        AssertEqual(1, cache.GetCacheSize(), "cache size should reflect inserted systems");
        AssertTrue(ReferenceEquals(system, cache.GetSystem(seed)), "cache should return the same stored instance");

        cache.Evict(seed);
        AssertTrue(!cache.HasSystem(seed), "evict should remove the cached system");
        AssertEqual(0, cache.GetCacheSize(), "cache size should shrink after eviction");
    }

    /// <summary>
    /// Verifies JSON save/load round-trip behavior for the migrated system-viewer helper.
    /// </summary>
    private static void TestSystemViewerSaveLoadRoundTripsJsonPath()
    {
        StarGen.App.SystemViewer.SystemViewerSaveLoad saveLoad = new();
        SolarSystem? original = GalaxySystemGenerator.GenerateSystem(CreateFixtureGalaxyStar(), includeAsteroids: false, enablePopulation: false);
        AssertNotNull(original, "fixture galaxy star should generate a system for save/load testing");

        string path = Path.Combine(
            Godot.ProjectSettings.GlobalizePath("user://"),
            $"dotnet_native_system_{System.Guid.NewGuid():N}.json");
        MockSystemViewerNode viewer = new(original!);

        try
        {
            Error saveError = saveLoad.SaveToPath(viewer, path, compress: false);
            AssertEqual(Error.Ok, saveError, "system-viewer save helper should save JSON files successfully");

            SystemPersistenceLoadResult result = saveLoad.LoadFromPath(path);
            AssertTrue(result.Success, "system-viewer load helper should load the saved file");
            AssertNotNull(result.System, "system-viewer load helper should rebuild a system");

            Godot.Collections.Dictionary originalData = SystemSerializer.ToDictionary(original!);
            Godot.Collections.Dictionary rebuiltData = SystemSerializer.ToDictionary(result.System!);
            NormalizeTransientFields(originalData);
            NormalizeTransientFields(rebuiltData);

            AssertVariantDeepEqual(
                originalData,
                rebuiltData,
                "system-viewer save/load helper should preserve system payloads");
        }
        finally
        {
            viewer.Free();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
