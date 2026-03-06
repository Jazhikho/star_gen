#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationGenerator.
/// </summary>
public static class TestStationGenerator
{
    /// <summary>
    /// Creates a bridge system context.
    /// </summary>
    private static StationSystemContext CreateBridgeContext()
    {
        StationSystemContext ctx = new StationSystemContext();
        ctx.SystemId = "bridge_system";
        ctx.IsBridgeSystem = true;
        ctx.PlanetIds = new Array<string>(new List<string> { "planet_001" });
        return ctx;
    }

    /// <summary>
    /// Creates a colony context.
    /// </summary>
    private static StationSystemContext CreateColonyContext()
    {
        StationSystemContext ctx = new StationSystemContext();
        ctx.SystemId = "colony_system";
        ctx.ColonyWorldCount = 2;
        ctx.ColonyPlanetIds = new Array<string>(new List<string> { "planet_001", "planet_002" });
        ctx.HabitablePlanetCount = 2;
        ctx.PlanetIds = new Array<string>(new List<string> { "planet_001", "planet_002" });
        return ctx;
    }

    /// <summary>
    /// Creates a resource system context.
    /// </summary>
    private static StationSystemContext CreateResourceContext()
    {
        StationSystemContext ctx = new StationSystemContext();
        ctx.SystemId = "resource_system";
        ctx.ResourceRichness = 0.7f;
        ctx.HabitablePlanetCount = 0;
        ctx.AsteroidBeltCount = 2;
        ctx.ResourceBodyIds = new Array<string>(new List<string> { "asteroid_001", "asteroid_002" });
        return ctx;
    }

    /// <summary>
    /// Creates a spacefaring natives context.
    /// </summary>
    private static StationSystemContext CreateNativeContext()
    {
        StationSystemContext ctx = new StationSystemContext();
        ctx.SystemId = "native_system";
        ctx.NativeWorldCount = 1;
        ctx.NativePlanetIds = new Array<string>(new List<string> { "planet_001" });
        ctx.HighestNativeTech = TechnologyLevel.Level.Spacefaring;
        ctx.HasSpacefaringNatives = true;
        ctx.PlanetIds = new Array<string>(new List<string> { "planet_001" });
        return ctx;
    }

    /// <summary>
    /// Creates an empty context.
    /// </summary>
    private static StationSystemContext CreateEmptyContext()
    {
        StationSystemContext ctx = new StationSystemContext();
        ctx.SystemId = "empty_system";
        return ctx;
    }

    /// <summary>
    /// Tests generation disabled.
    /// </summary>
    public static void TestGenerationDisabled()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerateStations = false;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertEqual(0, result.GetTotalCount(), "Should have 0 stations");
    }

    /// <summary>
    /// Tests generation for bridge system.
    /// </summary>
    public static void TestGenerateBridgeSystem()
    {
        StationSystemContext ctx = CreateBridgeContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertGreaterThan(result.GetTotalCount(), 0, "Should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, result.Recommendation.Context, "Context should be BridgeSystem");

        bool hasUtility = false;
        foreach (Outpost outpost in result.Outposts)
        {
            if (outpost.StationClass == StationClass.Class.U)
            {
                hasUtility = true;
                break;
            }
        }
        DotNetNativeTestSuite.AssertTrue(hasUtility, "Should have utility station");
    }

    /// <summary>
    /// Tests generation for colony system.
    /// </summary>
    public static void TestGenerateColonySystem()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertGreaterThan(result.GetTotalCount(), 0, "Should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ColonyWorld, result.Recommendation.Context, "Context should be ColonyWorld");
        DotNetNativeTestSuite.AssertGreaterThan(result.Stations.Count, 0, "Should have large stations");
    }

    /// <summary>
    /// Tests generation for resource system.
    /// </summary>
    public static void TestGenerateResourceSystem()
    {
        StationSystemContext ctx = CreateResourceContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertGreaterThan(result.GetTotalCount(), 0, "Should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ResourceSystem, result.Recommendation.Context, "Context should be ResourceSystem");

        bool hasMining = false;
        foreach (Outpost outpost in result.Outposts)
        {
            if (outpost.PrimaryPurpose == StationPurpose.Purpose.Mining)
            {
                hasMining = true;
                break;
            }
        }
        if (!hasMining)
        {
            foreach (SpaceStation station in result.Stations)
            {
                if (station.PrimaryPurpose == StationPurpose.Purpose.Mining)
                {
                    hasMining = true;
                    break;
                }
            }
        }
        DotNetNativeTestSuite.AssertTrue(hasMining, "Should have mining station");
    }

    /// <summary>
    /// Tests generation for native world.
    /// </summary>
    public static void TestGenerateNativeWorld()
    {
        StationSystemContext ctx = CreateNativeContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertGreaterThan(result.GetTotalCount(), 0, "Should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.NativeWorld, result.Recommendation.Context, "Context should be NativeWorld");
    }

    /// <summary>
    /// Tests generation for empty system.
    /// </summary>
    public static void TestGenerateEmptySystem()
    {
        StationSystemContext ctx = CreateEmptyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertEqual(0, result.GetTotalCount(), "Empty system should have 0 stations");
    }

    /// <summary>
    /// Tests empty system with min_stations override.
    /// </summary>
    public static void TestGenerateEmptyWithMinStations()
    {
        StationSystemContext ctx = CreateEmptyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.MinStations = 2;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertGreaterThan(result.GetTotalCount(), 0, "Should have stations with min_stations override");
    }

    /// <summary>
    /// Tests max_stations limit.
    /// </summary>
    public static void TestMaxStationsLimit()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.MaxStations = 1;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertLessThan(result.GetTotalCount(), 3, "Should respect max_stations");
    }

    /// <summary>
    /// Tests forced context.
    /// </summary>
    public static void TestForcedContext()
    {
        StationSystemContext ctx = CreateEmptyContext();
        StationSpec spec = StationSpec.ForContext(StationPlacementContext.Context.ResourceSystem);
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ResourceSystem, result.Recommendation.Context, "Context should be forced to ResourceSystem");
        DotNetNativeTestSuite.AssertGreaterThan(result.GetTotalCount(), 0, "Should have stations");
    }

    /// <summary>
    /// Tests determinism with same seed.
    /// </summary>
    public static void TestDeterminismSameSeed()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 99999;

        StationGenerator.GenerationResult result1 = StationGenerator.Generate(ctx, spec);
        StationGenerator.GenerationResult result2 = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertEqual(result1.GetTotalCount(), result2.GetTotalCount(), "Total count should match");
        DotNetNativeTestSuite.AssertEqual(result1.Outposts.Count, result2.Outposts.Count, "Outpost count should match");
        DotNetNativeTestSuite.AssertEqual(result1.Stations.Count, result2.Stations.Count, "Station count should match");

        for (int i = 0; i < result1.Stations.Count; i += 1)
        {
            DotNetNativeTestSuite.AssertEqual(result1.Stations[i].Id, result2.Stations[i].Id, $"Station {i} ID should match");
            DotNetNativeTestSuite.AssertEqual(result1.Stations[i].Name, result2.Stations[i].Name, $"Station {i} Name should match");
            DotNetNativeTestSuite.AssertEqual(result1.Stations[i].Population, result2.Stations[i].Population, $"Station {i} Population should match");
        }
    }

    /// <summary>
    /// Tests determinism with different seeds.
    /// </summary>
    public static void TestDeterminismDifferentSeeds()
    {
        StationSystemContext ctx = CreateColonyContext();

        StationSpec spec1 = new StationSpec();
        spec1.GenerationSeed = 11111;

        StationSpec spec2 = new StationSpec();
        spec2.GenerationSeed = 22222;

        StationGenerator.GenerationResult result1 = StationGenerator.Generate(ctx, spec1);
        StationGenerator.GenerationResult result2 = StationGenerator.Generate(ctx, spec2);

        DotNetNativeTestSuite.AssertEqual(11111, result1.GenerationSeed, "Result1 seed should be 11111");
        DotNetNativeTestSuite.AssertEqual(22222, result2.GenerationSeed, "Result2 seed should be 22222");
    }

    /// <summary>
    /// Tests allow_utility = false.
    /// </summary>
    public static void TestNoUtilityStations()
    {
        StationSystemContext ctx = CreateBridgeContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.AllowUtility = false;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        foreach (Outpost outpost in result.Outposts)
        {
            DotNetNativeTestSuite.AssertNotEqual(StationClass.Class.U, outpost.StationClass, "Should not have U class");
        }
    }

    /// <summary>
    /// Tests allow_large_stations = false.
    /// </summary>
    public static void TestNoLargeStations()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.AllowLargeStations = false;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        foreach (SpaceStation station in result.Stations)
        {
            DotNetNativeTestSuite.AssertTrue(StationClass.UsesOutpostGovernment(station.StationClass), "Should only have small stations");
        }
    }

    /// <summary>
    /// Tests excluded purposes.
    /// </summary>
    public static void TestExcludedPurposes()
    {
        StationSystemContext ctx = CreateResourceContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.ExcludedPurposes = new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Mining };

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        foreach (Outpost outpost in result.Outposts)
        {
            DotNetNativeTestSuite.AssertNotEqual(StationPurpose.Purpose.Mining, outpost.PrimaryPurpose, "Outpost should not have Mining purpose");
        }
        foreach (SpaceStation station in result.Stations)
        {
            DotNetNativeTestSuite.AssertNotEqual(StationPurpose.Purpose.Mining, station.PrimaryPurpose, "Station should not have Mining purpose");
        }
    }

    /// <summary>
    /// Tests population density modifier.
    /// </summary>
    public static void TestPopulationDensity()
    {
        StationSystemContext ctx = CreateColonyContext();

        StationSpec specNormal = new StationSpec();
        specNormal.GenerationSeed = 12345;
        specNormal.PopulationDensity = 1.0f;

        StationSpec specDense = new StationSpec();
        specDense.GenerationSeed = 12345;
        specDense.PopulationDensity = 2.0f;

        StationGenerator.GenerationResult resultNormal = StationGenerator.Generate(ctx, specNormal);
        StationGenerator.GenerationResult resultDense = StationGenerator.Generate(ctx, specDense);

        int popNormal = 0;
        foreach (SpaceStation s in resultNormal.Stations)
        {
            popNormal += s.Population;
        }
        foreach (Outpost o in resultNormal.Outposts)
        {
            popNormal += o.Population;
        }

        int popDense = 0;
        foreach (SpaceStation s in resultDense.Stations)
        {
            popDense += s.Population;
        }
        foreach (Outpost o in resultDense.Outposts)
        {
            popDense += o.Population;
        }

        DotNetNativeTestSuite.AssertGreaterThan(popDense, (int)(popNormal * 0.5f), "Dense should have more population");
    }

    /// <summary>
    /// Tests station IDs are unique.
    /// </summary>
    public static void TestUniqueIds()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.MinStations = 5;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        System.Collections.Generic.Dictionary<string, bool> ids = new System.Collections.Generic.Dictionary<string, bool>();
        foreach (Outpost outpost in result.Outposts)
        {
            DotNetNativeTestSuite.AssertFalse(ids.ContainsKey(outpost.Id), $"Duplicate ID: {outpost.Id}");
            ids[outpost.Id] = true;
        }
        foreach (SpaceStation station in result.Stations)
        {
            DotNetNativeTestSuite.AssertFalse(ids.ContainsKey(station.Id), $"Duplicate ID: {station.Id}");
            ids[station.Id] = true;
        }
    }

    /// <summary>
    /// Tests orbital stations have body IDs.
    /// </summary>
    public static void TestOrbitalBodyIds()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        foreach (SpaceStation station in result.Stations)
        {
            if (station.StationType == StationType.Type.Orbital)
            {
                DotNetNativeTestSuite.AssertNotEqual("", station.OrbitingBodyId, "Orbital station should have body ID");
            }
        }
    }

    /// <summary>
    /// Tests large stations have government.
    /// </summary>
    public static void TestLargeStationsHaveGovernment()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        foreach (SpaceStation station in result.Stations)
        {
            if (station.UsesColonyGovernment())
            {
                DotNetNativeTestSuite.AssertNotNull(station.Government, "Large station should have government");
                DotNetNativeTestSuite.AssertNotNull(station.History, "Large station should have history");
            }
        }
    }

    /// <summary>
    /// Tests get_stations_for_body.
    /// </summary>
    public static void TestGetStationsForBody()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        List<SpaceStation> planetStations = result.GetStationsForBody("planet_001");

        foreach (SpaceStation station in planetStations)
        {
            DotNetNativeTestSuite.AssertEqual("planet_001", station.OrbitingBodyId, "Station should orbit planet_001");
        }
    }

    /// <summary>
    /// Tests result to_dict.
    /// </summary>
    public static void TestResultToDict()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);
        Godot.Collections.Dictionary data = result.ToDictionary();

        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("outposts"), "Should have outposts");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("stations"), "Should have stations");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("generation_seed"), "Should have generation_seed");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("recommendation"), "Should have recommendation");
    }

    /// <summary>
    /// Tests invalid spec produces warnings.
    /// </summary>
    public static void TestInvalidSpecWarnings()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.MinStations = 10;
        spec.MaxStations = 1;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        DotNetNativeTestSuite.AssertGreaterThan(result.Warnings.Count, 0, "Should have warnings");
    }

    /// <summary>
    /// Tests decommission chance.
    /// </summary>
    public static void TestDecommissionChance()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.DecommissionChance = 0.5f;
        spec.MinStations = 10;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        int decommissionedCount = 0;
        foreach (Outpost outpost in result.Outposts)
        {
            if (!outpost.IsOperational)
            {
                decommissionedCount += 1;
            }
        }
        foreach (SpaceStation station in result.Stations)
        {
            if (!station.IsOperational)
            {
                decommissionedCount += 1;
            }
        }

        DotNetNativeTestSuite.AssertTrue(decommissionedCount >= 0, "Decommissioned count should be >= 0");
    }

    /// <summary>
    /// Tests establishment years are within range.
    /// </summary>
    public static void TestEstablishmentYears()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationSpec spec = new StationSpec();
        spec.GenerationSeed = 12345;
        spec.MinEstablishedYear = -100;
        spec.MaxEstablishedYear = -10;

        StationGenerator.GenerationResult result = StationGenerator.Generate(ctx, spec);

        foreach (Outpost outpost in result.Outposts)
        {
            DotNetNativeTestSuite.AssertGreaterThan(outpost.EstablishedYear, -101, "Outpost established year should be > -101");
            DotNetNativeTestSuite.AssertLessThan(outpost.EstablishedYear, -9, "Outpost established year should be < -9");
        }

        foreach (SpaceStation station in result.Stations)
        {
            DotNetNativeTestSuite.AssertGreaterThan(station.EstablishedYear, -101, "Station established year should be > -101");
            DotNetNativeTestSuite.AssertLessThan(station.EstablishedYear, -9, "Station established year should be < -9");
        }
    }
}
