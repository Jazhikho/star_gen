#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationPlacementRules.
/// </summary>
public static class TestStationPlacementRules
{
    /// <summary>
    /// Creates a basic empty system context.
    /// </summary>
    private static StationSystemContext CreateEmptyContext()
    {
        StationSystemContext ctx = new StationSystemContext();
        ctx.SystemId = "test_system";
        return ctx;
    }

    /// <summary>
    /// Creates a bridge system context.
    /// </summary>
    private static StationSystemContext CreateBridgeContext()
    {
        StationSystemContext ctx = CreateEmptyContext();
        ctx.IsBridgeSystem = true;
        return ctx;
    }

    /// <summary>
    /// Creates a context with spacefaring natives.
    /// </summary>
    private static StationSystemContext CreateNativeSpacefaringContext()
    {
        StationSystemContext ctx = CreateEmptyContext();
        ctx.NativeWorldCount = 1;
        ctx.NativePlanetIds = new Array<string>(new List<string> { "planet_001" });
        ctx.HighestNativeTech = TechnologyLevel.Level.Spacefaring;
        ctx.HasSpacefaringNatives = true;
        ctx.PlanetIds = new Array<string>(new List<string> { "planet_001" });
        return ctx;
    }

    /// <summary>
    /// Creates a context with non-spacefaring natives.
    /// </summary>
    private static StationSystemContext CreateNativePrimitiveContext()
    {
        StationSystemContext ctx = CreateEmptyContext();
        ctx.NativeWorldCount = 1;
        ctx.NativePlanetIds = new Array<string>(new List<string> { "planet_001" });
        ctx.HighestNativeTech = TechnologyLevel.Level.Industrial;
        ctx.HasSpacefaringNatives = false;
        ctx.PlanetIds = new Array<string>(new List<string> { "planet_001" });
        return ctx;
    }

    /// <summary>
    /// Creates a context with a colony.
    /// </summary>
    private static StationSystemContext CreateColonyContext()
    {
        StationSystemContext ctx = CreateEmptyContext();
        ctx.ColonyWorldCount = 1;
        ctx.ColonyPlanetIds = new Array<string>(new List<string> { "planet_001" });
        ctx.HabitablePlanetCount = 1;
        ctx.PlanetIds = new Array<string>(new List<string> { "planet_001" });
        return ctx;
    }

    /// <summary>
    /// Creates a resource-rich system context.
    /// </summary>
    private static StationSystemContext CreateResourceContext()
    {
        StationSystemContext ctx = CreateEmptyContext();
        ctx.ResourceRichness = 0.7f;
        ctx.HabitablePlanetCount = 0;
        ctx.AsteroidBeltCount = 2;
        ctx.ResourceBodyIds = new Array<string>(new List<string> { "asteroid_001", "asteroid_002", "moon_001" });
        return ctx;
    }

    /// <summary>
    /// Tests empty system evaluation.
    /// </summary>
    public static void TestEvaluateEmptySystem()
    {
        StationSystemContext ctx = CreateEmptyContext();
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertFalse(rec.ShouldHaveStations, "Empty system should not have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Other, rec.Context, "Context should be Other");
    }

    /// <summary>
    /// Tests bridge system evaluation.
    /// </summary>
    public static void TestEvaluateBridgeSystem()
    {
        StationSystemContext ctx = CreateBridgeContext();
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertTrue(rec.ShouldHaveStations, "Bridge system should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, rec.Context, "Context should be BridgeSystem");
        DotNetNativeTestSuite.AssertGreaterThan(rec.UtilityStationCount, 0, "Should have utility stations");
        DotNetNativeTestSuite.AssertTrue(rec.AllowDeepSpace, "Should allow deep space");
        DotNetNativeTestSuite.AssertTrue(rec.RecommendedPurposes.Contains(StationPurpose.Purpose.Utility), "Should recommend Utility");
    }

    /// <summary>
    /// Tests native spacefaring world evaluation.
    /// </summary>
    public static void TestEvaluateNativeSpacefaring()
    {
        StationSystemContext ctx = CreateNativeSpacefaringContext();
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertTrue(rec.ShouldHaveStations, "Native spacefaring should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.NativeWorld, rec.Context, "Context should be NativeWorld");
        DotNetNativeTestSuite.AssertGreaterThan(rec.LargeStationCount, 0, "Should have large stations");
        DotNetNativeTestSuite.AssertTrue(rec.OrbitalCandidates.Contains("planet_001"), "Should have planet_001 as candidate");
        DotNetNativeTestSuite.AssertTrue(rec.RecommendedPurposes.Contains(StationPurpose.Purpose.Trade), "Should recommend Trade");
    }

    /// <summary>
    /// Tests native primitive world evaluation (scientific interest).
    /// </summary>
    public static void TestEvaluateNativePrimitive()
    {
        StationSystemContext ctx = CreateNativePrimitiveContext();
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertTrue(rec.ShouldHaveStations, "Native primitive should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Scientific, rec.Context, "Context should be Scientific");
        DotNetNativeTestSuite.AssertGreaterThan(rec.OutpostCount, 0, "Should have outposts");
        DotNetNativeTestSuite.AssertTrue(rec.RecommendedPurposes.Contains(StationPurpose.Purpose.Science), "Should recommend Science");
    }

    /// <summary>
    /// Tests colony world evaluation.
    /// </summary>
    public static void TestEvaluateColonyWorld()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertTrue(rec.ShouldHaveStations, "Colony world should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ColonyWorld, rec.Context, "Context should be ColonyWorld");
        DotNetNativeTestSuite.AssertGreaterThan(rec.LargeStationCount, 0, "Should have large stations");
        DotNetNativeTestSuite.AssertTrue(rec.OrbitalCandidates.Contains("planet_001"), "Should have planet_001 as candidate");
    }

    /// <summary>
    /// Tests resource-rich system evaluation.
    /// </summary>
    public static void TestEvaluateResourceSystem()
    {
        StationSystemContext ctx = CreateResourceContext();
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertTrue(rec.ShouldHaveStations, "Resource system should have stations");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ResourceSystem, rec.Context, "Context should be ResourceSystem");
        DotNetNativeTestSuite.AssertGreaterThan(rec.OutpostCount, 0, "Should have outposts");
        DotNetNativeTestSuite.AssertTrue(rec.AllowBeltStations, "Should allow belt stations");
        DotNetNativeTestSuite.AssertTrue(rec.RecommendedPurposes.Contains(StationPurpose.Purpose.Mining), "Should recommend Mining");
    }

    /// <summary>
    /// Tests high-resource system gets large station.
    /// </summary>
    public static void TestHighResourceGetsLargeStation()
    {
        StationSystemContext ctx = CreateResourceContext();
        ctx.ResourceRichness = 0.8f;
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertGreaterThan(rec.LargeStationCount, 0, "High resource should have large stations");
        DotNetNativeTestSuite.AssertTrue(rec.RecommendedPurposes.Contains(StationPurpose.Purpose.Residential), "Should recommend Residential");
    }

    /// <summary>
    /// Tests colony + bridge system gets both utilities and orbital.
    /// </summary>
    public static void TestBridgeWithColony()
    {
        StationSystemContext ctx = CreateBridgeContext();
        ctx.ColonyWorldCount = 1;
        ctx.ColonyPlanetIds = new Array<string>(new List<string> { "planet_001" });
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, rec.Context, "Context should be BridgeSystem");
        DotNetNativeTestSuite.AssertGreaterThan(rec.UtilityStationCount, 0, "Should have utility stations");
        DotNetNativeTestSuite.AssertGreaterThan(rec.LargeStationCount, 0, "Should have large stations");
    }

    /// <summary>
    /// Tests should_have_orbital_stations with colony.
    /// </summary>
    public static void TestShouldHaveOrbitalColony()
    {
        bool result = StationPlacementRules.ShouldHaveOrbitalStations(false, null, true);
        DotNetNativeTestSuite.AssertTrue(result, "Colony should have orbital stations");
    }

    /// <summary>
    /// Tests should_have_orbital_stations with spacefaring natives.
    /// </summary>
    public static void TestShouldHaveOrbitalSpacefaringNatives()
    {
        bool result = StationPlacementRules.ShouldHaveOrbitalStations(true, TechnologyLevel.Level.Spacefaring, false);
        DotNetNativeTestSuite.AssertTrue(result, "Spacefaring natives should have orbital stations");
    }

    /// <summary>
    /// Tests should_have_orbital_stations with primitive natives.
    /// </summary>
    public static void TestShouldHaveOrbitalPrimitiveNatives()
    {
        bool result = StationPlacementRules.ShouldHaveOrbitalStations(true, TechnologyLevel.Level.Industrial, false);
        DotNetNativeTestSuite.AssertFalse(result, "Primitive natives should not have orbital stations");
    }

    /// <summary>
    /// Tests should_have_orbital_stations with no population.
    /// </summary>
    public static void TestShouldHaveOrbitalNone()
    {
        bool result = StationPlacementRules.ShouldHaveOrbitalStations(false, null, false);
        DotNetNativeTestSuite.AssertFalse(result, "No population should not have orbital stations");
    }

    /// <summary>
    /// Tests estimate_orbital_station_count.
    /// </summary>
    public static void TestEstimateOrbitalStationCount()
    {
        int count = StationPlacementRules.EstimateOrbitalStationCount(10_000_000, TechnologyLevel.Level.Industrial);
        DotNetNativeTestSuite.AssertEqual(0, count, "Non-spacefaring should get 0");

        count = StationPlacementRules.EstimateOrbitalStationCount(1_000_000, TechnologyLevel.Level.Spacefaring);
        DotNetNativeTestSuite.AssertEqual(1, count, "Small spacefaring should get 1");

        count = StationPlacementRules.EstimateOrbitalStationCount(50_000_000, TechnologyLevel.Level.Spacefaring);
        DotNetNativeTestSuite.AssertEqual(5, count, "Large population should get 5");

        int interstellarCount = StationPlacementRules.EstimateOrbitalStationCount(50_000_000, TechnologyLevel.Level.Interstellar);
        DotNetNativeTestSuite.AssertGreaterThan(interstellarCount, 5, "Interstellar should get bonus");
    }

    /// <summary>
    /// Tests recommend_station_class for bridge system.
    /// </summary>
    public static void TestRecommendClassBridge()
    {
        StationClass.Class cls = StationPlacementRules.RecommendStationClass(StationPlacementContext.Context.BridgeSystem, false);
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, cls, "Bridge should recommend U");
    }

    /// <summary>
    /// Tests recommend_station_class for scientific outpost.
    /// </summary>
    public static void TestRecommendClassScientific()
    {
        StationClass.Class cls = StationPlacementRules.RecommendStationClass(StationPlacementContext.Context.Scientific, false);
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, cls, "Scientific should recommend O");
    }

    /// <summary>
    /// Tests recommend_station_class for colony with large pop.
    /// </summary>
    public static void TestRecommendClassColonyLarge()
    {
        StationClass.Class cls = StationPlacementRules.RecommendStationClass(StationPlacementContext.Context.ColonyWorld, true);
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, cls, "Colony with large pop should recommend A");
    }

    /// <summary>
    /// Tests recommend_station_class for resource system without pop.
    /// </summary>
    public static void TestRecommendClassResourceSmall()
    {
        StationClass.Class cls = StationPlacementRules.RecommendStationClass(StationPlacementContext.Context.ResourceSystem, false);
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, cls, "Resource without pop should recommend O");
    }

    /// <summary>
    /// Tests recommend_purposes for utility station.
    /// </summary>
    public static void TestRecommendPurposesUtility()
    {
        Array<StationPurpose.Purpose> purposes = StationPlacementRules.RecommendPurposes(StationPlacementContext.Context.BridgeSystem, true);
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Utility), "Should recommend Utility");
    }

    /// <summary>
    /// Tests recommend_purposes for colony.
    /// </summary>
    public static void TestRecommendPurposesColony()
    {
        Array<StationPurpose.Purpose> purposes = StationPlacementRules.RecommendPurposes(StationPlacementContext.Context.ColonyWorld, false);
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Trade), "Should recommend Trade");
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Residential), "Should recommend Residential");
    }

    /// <summary>
    /// Tests recommend_purposes for mining.
    /// </summary>
    public static void TestRecommendPurposesResource()
    {
        Array<StationPurpose.Purpose> purposes = StationPlacementRules.RecommendPurposes(StationPlacementContext.Context.ResourceSystem, false);
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Mining), "Should recommend Mining");
    }

    /// <summary>
    /// Tests calculate_resource_richness with empty resources.
    /// </summary>
    public static void TestResourceRichnessEmpty()
    {
        float richness = (float)StationPlacementRules.CalculateResourceRichness(new Godot.Collections.Dictionary());
        DotNetNativeTestSuite.AssertFloatEqual(0.0f, richness, 0.001f, "Empty resources should be 0");
    }

    /// <summary>
    /// Tests calculate_resource_richness with common resources.
    /// </summary>
    public static void TestResourceRichnessCommon()
    {
        Godot.Collections.Dictionary resources = new Godot.Collections.Dictionary
        {
            { (int)ResourceType.Type.Silicates, 0.5f },
            { (int)ResourceType.Type.Metals, 0.3f }
        };
        float richness = (float)StationPlacementRules.CalculateResourceRichness(resources);
        DotNetNativeTestSuite.AssertGreaterThan(richness, 0.0f, "Common resources should be > 0");
        DotNetNativeTestSuite.AssertLessThan(richness, 1.0f, "Common resources should be < 1");
    }

    /// <summary>
    /// Tests calculate_resource_richness with rare resources.
    /// </summary>
    public static void TestResourceRichnessRare()
    {
        Godot.Collections.Dictionary common = new Godot.Collections.Dictionary
        {
            { (int)ResourceType.Type.Silicates, 0.5f }
        };
        Godot.Collections.Dictionary rare = new Godot.Collections.Dictionary
        {
            { (int)ResourceType.Type.RareElements, 0.5f }
        };
        float commonRichness = (float)StationPlacementRules.CalculateResourceRichness(common);
        float rareRichness = (float)StationPlacementRules.CalculateResourceRichness(rare);

        DotNetNativeTestSuite.AssertGreaterThan(rareRichness, commonRichness, "Rare resources should score higher");
    }

    /// <summary>
    /// Tests create_system_context helper.
    /// </summary>
    public static void TestCreateSystemContext()
    {
        Array<Godot.Collections.Dictionary> nativeData = new Array<Godot.Collections.Dictionary>
        {
            new Godot.Collections.Dictionary { { "body_id", "planet_001" }, { "tech_level", (int)TechnologyLevel.Level.Spacefaring } }
        };
        Array<string> colonyIds = new Array<string> { "planet_002" };
        Array<string> planetIds = new Array<string> { "planet_001", "planet_002" };
        Array<string> resourceIds = new Array<string> { "asteroid_001" };

        StationSystemContext ctx = StationPlacementRules.CreateSystemContext(
            "sys_001",
            planetIds,
            2,
            nativeData,
            colonyIds,
            0.5f,
            1,
            resourceIds,
            true
        );

        DotNetNativeTestSuite.AssertEqual("sys_001", ctx.SystemId, "SystemId should match");
        DotNetNativeTestSuite.AssertTrue(ctx.IsBridgeSystem, "Should be bridge system");
        DotNetNativeTestSuite.AssertEqual(2, ctx.HabitablePlanetCount, "HabitablePlanetCount should be 2");
        DotNetNativeTestSuite.AssertEqual(1, ctx.NativeWorldCount, "NativeWorldCount should be 1");
        DotNetNativeTestSuite.AssertEqual(1, ctx.ColonyWorldCount, "ColonyWorldCount should be 1");
        DotNetNativeTestSuite.AssertTrue(ctx.HasSpacefaringNatives, "Should have spacefaring natives");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Spacefaring, ctx.HighestNativeTech, "HighestNativeTech should be Spacefaring");
        DotNetNativeTestSuite.AssertFloatEqual(0.5f, ctx.ResourceRichness, 0.001f, "ResourceRichness should be 0.5");
        DotNetNativeTestSuite.AssertEqual(1, ctx.AsteroidBeltCount, "AsteroidBeltCount should be 1");
    }

    /// <summary>
    /// Tests context priority: bridge takes precedence.
    /// </summary>
    public static void TestContextPriorityBridge()
    {
        StationSystemContext ctx = CreateColonyContext();
        ctx.IsBridgeSystem = true;
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, rec.Context, "Bridge should take precedence");
    }

    /// <summary>
    /// Tests context priority: spacefaring natives over colony.
    /// </summary>
    public static void TestContextPrioritySpacefaringNatives()
    {
        StationSystemContext ctx = CreateNativeSpacefaringContext();
        ctx.ColonyWorldCount = 1;
        ctx.ColonyPlanetIds = new Array<string>(new List<string> { "planet_002" });
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.NativeWorld, rec.Context, "Spacefaring natives should take precedence");
    }

    /// <summary>
    /// Tests context priority: colony over resources.
    /// </summary>
    public static void TestContextPriorityColonyOverResources()
    {
        StationSystemContext ctx = CreateColonyContext();
        ctx.ResourceRichness = 0.8f;
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ColonyWorld, rec.Context, "Colony should take precedence over resources");
    }

    /// <summary>
    /// Tests strategic context for habitable but uncolonized.
    /// </summary>
    public static void TestStrategicContext()
    {
        StationSystemContext ctx = CreateEmptyContext();
        ctx.HabitablePlanetCount = 1;
        ctx.PlanetIds = new Array<string>(new List<string> { "planet_001" });
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Strategic, rec.Context, "Should be Strategic");
        DotNetNativeTestSuite.AssertTrue(rec.RecommendedPurposes.Contains(StationPurpose.Purpose.Military), "Should recommend Military");
    }

    /// <summary>
    /// Tests recommendation to_dict.
    /// </summary>
    public static void TestRecommendationToDict()
    {
        StationSystemContext ctx = CreateColonyContext();
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        Godot.Collections.Dictionary data = rec.ToDictionary();
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("context"), "Should have context");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("should_have_stations"), "Should have should_have_stations");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("utility_station_count"), "Should have utility_station_count");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("outpost_count"), "Should have outpost_count");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("large_station_count"), "Should have large_station_count");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("recommended_purposes"), "Should have recommended_purposes");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("reasoning"), "Should have reasoning");
    }

    /// <summary>
    /// Tests minor resources trigger small outpost.
    /// </summary>
    public static void TestMinorResourcesOutpost()
    {
        StationSystemContext ctx = CreateEmptyContext();
        ctx.ResourceRichness = 0.25f;
        StationPlacementRecommendation rec = StationPlacementRules.EvaluateSystem(ctx);

        DotNetNativeTestSuite.AssertTrue(rec.ShouldHaveStations, "Minor resources should have stations");
        DotNetNativeTestSuite.AssertGreaterThan(rec.OutpostCount, 0, "Should have outposts");
    }
}
