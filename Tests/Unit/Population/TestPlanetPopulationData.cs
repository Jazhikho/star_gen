#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for PlanetPopulationData container.
/// </summary>
public static class TestPlanetPopulationData
{
    /// <summary>
    /// Creates a test profile.
    /// </summary>
    private static PlanetProfile CreateTestProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "planet_001";
        profile.HabitabilityScore = 8;
        return profile;
    }

    /// <summary>
    /// Creates a test suitability.
    /// </summary>
    private static ColonySuitability CreateTestSuitability()
    {
        ColonySuitability suitability = new();
        suitability.OverallScore = 75;
        return suitability;
    }

    /// <summary>
    /// Creates a test native population.
    /// </summary>
    private static NativePopulation CreateTestNative(string id, int pop, bool extant = true)
    {
        NativePopulation native = new();
        native.Id = id;
        native.Name = "Native " + id;
        native.Population = pop;
        native.IsExtant = extant;
        native.TechLevel = TechnologyLevel.Level.Medieval;
        return native;
    }

    /// <summary>
    /// Creates a test colony.
    /// </summary>
    private static Colony CreateTestColony(string id, int pop, bool active = true)
    {
        Colony colony = new();
        colony.Id = id;
        colony.Name = "Colony " + id;
        colony.Population = pop;
        colony.IsActive = active;
        colony.TechLevel = TechnologyLevel.Level.Interstellar;
        return colony;
    }

    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertEqual("", data.BodyId, "Default BodyId should be empty");
        DotNetNativeTestSuite.AssertNull(data.Profile, "Default Profile should be null");
        DotNetNativeTestSuite.AssertNull(data.Suitability, "Default Suitability should be null");
        DotNetNativeTestSuite.AssertEqual(0, data.NativePopulations.Count, "Default should have no natives");
        DotNetNativeTestSuite.AssertEqual(0, data.Colonies.Count, "Default should have no colonies");
        DotNetNativeTestSuite.AssertEqual(0, data.GeneratedTimestamp, "Default timestamp should be 0");
    }

    /// <summary>
    /// Tests get_total_population with no populations.
    /// </summary>
    public static void TestGetTotalPopulationEmpty()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertEqual(0, data.GetTotalPopulation(), "Empty data should have 0 population");
    }

    /// <summary>
    /// Tests get_total_population with natives only.
    /// </summary>
    public static void TestGetTotalPopulationNativesOnly()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        data.NativePopulations.Add(CreateTestNative("n2", 500000));

        DotNetNativeTestSuite.AssertEqual(1500000, data.GetTotalPopulation(), "Should sum native populations");
    }

    /// <summary>
    /// Tests get_total_population with colonies only.
    /// </summary>
    public static void TestGetTotalPopulationColoniesOnly()
    {
        PlanetPopulationData data = new();
        data.Colonies.Add(CreateTestColony("c1", 200000));
        data.Colonies.Add(CreateTestColony("c2", 100000));

        DotNetNativeTestSuite.AssertEqual(300000, data.GetTotalPopulation(), "Should sum colony populations");
    }

    /// <summary>
    /// Tests get_total_population with mixed populations.
    /// </summary>
    public static void TestGetTotalPopulationMixed()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        data.Colonies.Add(CreateTestColony("c1", 200000));

        DotNetNativeTestSuite.AssertEqual(1200000, data.GetTotalPopulation(), "Should sum all populations");
    }

    /// <summary>
    /// Tests get_total_population excludes extinct/abandoned.
    /// </summary>
    public static void TestGetTotalPopulationExcludesInactive()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000, true));
        data.NativePopulations.Add(CreateTestNative("n2", 500000, false));
        data.Colonies.Add(CreateTestColony("c1", 200000, true));
        data.Colonies.Add(CreateTestColony("c2", 100000, false));

        DotNetNativeTestSuite.AssertEqual(1200000, data.GetTotalPopulation(), "Should exclude extinct and abandoned");
    }

    /// <summary>
    /// Tests get_native_population.
    /// </summary>
    public static void TestGetNativePopulation()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        data.Colonies.Add(CreateTestColony("c1", 200000));

        DotNetNativeTestSuite.AssertEqual(1000000, data.GetNativePopulation(), "Should return native population");
    }

    /// <summary>
    /// Tests get_colony_population.
    /// </summary>
    public static void TestGetColonyPopulation()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        data.Colonies.Add(CreateTestColony("c1", 200000));

        DotNetNativeTestSuite.AssertEqual(200000, data.GetColonyPopulation(), "Should return colony population");
    }

    /// <summary>
    /// Tests get_dominant_population returns largest.
    /// </summary>
    public static void TestGetDominantPopulation()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        data.Colonies.Add(CreateTestColony("c1", 2000000));

        Variant dominant = data.GetDominantPopulation();
        DotNetNativeTestSuite.AssertNotNull(dominant, "Should have dominant");
        DotNetNativeTestSuite.AssertTrue(dominant.Obj is Colony, "Dominant should be Colony");
        DotNetNativeTestSuite.AssertEqual("c1", ((Colony)dominant.Obj).Id, "Dominant ID should be c1");
    }

    /// <summary>
    /// Tests get_dominant_population with no populations.
    /// </summary>
    public static void TestGetDominantPopulationEmpty()
    {
        PlanetPopulationData data = new();
        Variant dominant = data.GetDominantPopulation();
        DotNetNativeTestSuite.AssertTrue(dominant.VariantType == Variant.Type.Nil, "Should be null");
    }

    /// <summary>
    /// Tests get_dominant_population_name.
    /// </summary>
    public static void TestGetDominantPopulationName()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertEqual("Uninhabited", data.GetDominantPopulationName(), "Should be Uninhabited");

        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        DotNetNativeTestSuite.AssertEqual("Native n1", data.GetDominantPopulationName(), "Should be Native n1");
    }

    /// <summary>
    /// Tests is_inhabited.
    /// </summary>
    public static void TestIsInhabited()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertFalse(data.IsInhabited(), "Should not be inhabited");

        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        DotNetNativeTestSuite.AssertTrue(data.IsInhabited(), "Should be inhabited");
    }

    /// <summary>
    /// Tests has_natives.
    /// </summary>
    public static void TestHasNatives()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertFalse(data.HasNatives(), "Should have no natives");

        data.NativePopulations.Add(CreateTestNative("n1", 1000000, false));
        DotNetNativeTestSuite.AssertTrue(data.HasNatives(), "Should have natives");
    }

    /// <summary>
    /// Tests has_extant_natives.
    /// </summary>
    public static void TestHasExtantNatives()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertFalse(data.HasExtantNatives(), "Should have no extant natives");

        data.NativePopulations.Add(CreateTestNative("n1", 1000000, false));
        DotNetNativeTestSuite.AssertFalse(data.HasExtantNatives(), "Extinct native should not count");

        data.NativePopulations.Add(CreateTestNative("n2", 500000, true));
        DotNetNativeTestSuite.AssertTrue(data.HasExtantNatives(), "Should have extant natives");
    }

    /// <summary>
    /// Tests has_colonies.
    /// </summary>
    public static void TestHasColonies()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertFalse(data.HasColonies(), "Should have no colonies");

        data.Colonies.Add(CreateTestColony("c1", 100000, false));
        DotNetNativeTestSuite.AssertTrue(data.HasColonies(), "Should have colonies");
    }

    /// <summary>
    /// Tests has_active_colonies.
    /// </summary>
    public static void TestHasActiveColonies()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertFalse(data.HasActiveColonies(), "Should have no active colonies");

        data.Colonies.Add(CreateTestColony("c1", 100000, false));
        DotNetNativeTestSuite.AssertFalse(data.HasActiveColonies(), "Abandoned colony should not count");

        data.Colonies.Add(CreateTestColony("c2", 200000, true));
        DotNetNativeTestSuite.AssertTrue(data.HasActiveColonies(), "Should have active colonies");
    }

    /// <summary>
    /// Tests get_extant_native_count.
    /// </summary>
    public static void TestGetExtantNativeCount()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000, true));
        data.NativePopulations.Add(CreateTestNative("n2", 500000, false));
        data.NativePopulations.Add(CreateTestNative("n3", 300000, true));

        DotNetNativeTestSuite.AssertEqual(2, data.GetExtantNativeCount(), "Should count extant natives");
    }

    /// <summary>
    /// Tests get_active_colony_count.
    /// </summary>
    public static void TestGetActiveColonyCount()
    {
        PlanetPopulationData data = new();
        data.Colonies.Add(CreateTestColony("c1", 200000, true));
        data.Colonies.Add(CreateTestColony("c2", 100000, false));
        data.Colonies.Add(CreateTestColony("c3", 50000, true));

        DotNetNativeTestSuite.AssertEqual(2, data.GetActiveColonyCount(), "Should count active colonies");
    }

    /// <summary>
    /// Tests get_extant_natives.
    /// </summary>
    public static void TestGetExtantNatives()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000, true));
        data.NativePopulations.Add(CreateTestNative("n2", 500000, false));

        Array<NativePopulation> extant = data.GetExtantNatives();
        DotNetNativeTestSuite.AssertEqual(1, extant.Count, "Should have 1 extant native");
        DotNetNativeTestSuite.AssertEqual("n1", extant[0].Id, "Extant native should be n1");
    }

    /// <summary>
    /// Tests get_active_colonies.
    /// </summary>
    public static void TestGetActiveColonies()
    {
        PlanetPopulationData data = new();
        data.Colonies.Add(CreateTestColony("c1", 200000, true));
        data.Colonies.Add(CreateTestColony("c2", 100000, false));

        Array<Colony> active = data.GetActiveColonies();
        DotNetNativeTestSuite.AssertEqual(1, active.Count, "Should have 1 active colony");
        DotNetNativeTestSuite.AssertEqual("c1", active[0].Id, "Active colony should be c1");
    }

    /// <summary>
    /// Tests has_native_colony_conflict.
    /// </summary>
    public static void TestHasNativeColonyConflict()
    {
        PlanetPopulationData data = new();

        Colony colony = CreateTestColony("c1", 200000);
        NativeRelation hostileRelation = NativeRelation.CreateFirstContact("n1", -100, -80);
        hostileRelation.CurrentStatus = NativeRelation.Status.Hostile;
        colony.SetNativeRelation(hostileRelation);

        data.Colonies.Add(colony);

        DotNetNativeTestSuite.AssertTrue(data.HasNativeColonyConflict(), "Should have conflict");
    }

    /// <summary>
    /// Tests get_political_situation.
    /// </summary>
    public static void TestGetPoliticalSituation()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertEqual("uninhabited", data.GetPoliticalSituation(), "Should be uninhabited");

        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        DotNetNativeTestSuite.AssertEqual("native_only", data.GetPoliticalSituation(), "Should be native_only");

        data.NativePopulations.Clear();
        data.Colonies.Add(CreateTestColony("c1", 200000));
        DotNetNativeTestSuite.AssertEqual("colony_only", data.GetPoliticalSituation(), "Should be colony_only");

        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        DotNetNativeTestSuite.AssertEqual("coexisting", data.GetPoliticalSituation(), "Should be coexisting");
    }

    /// <summary>
    /// Tests get_political_situation with conflict.
    /// </summary>
    public static void TestGetPoliticalSituationConflict()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));

        Colony colony = CreateTestColony("c1", 200000);
        NativeRelation hostileRelation = NativeRelation.CreateFirstContact("n1", -100, -80);
        hostileRelation.CurrentStatus = NativeRelation.Status.Hostile;
        colony.SetNativeRelation(hostileRelation);
        data.Colonies.Add(colony);

        DotNetNativeTestSuite.AssertEqual("conflict", data.GetPoliticalSituation(), "Should be conflict");
    }

    /// <summary>
    /// Tests get_highest_tech_level.
    /// </summary>
    public static void TestGetHighestTechLevel()
    {
        PlanetPopulationData data = new();
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, data.GetHighestTechLevel(), "Empty should be StoneAge");

        NativePopulation native = CreateTestNative("n1", 1000000);
        native.TechLevel = TechnologyLevel.Level.Medieval;
        data.NativePopulations.Add(native);
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Medieval, data.GetHighestTechLevel(), "Should be Medieval");

        Colony colony = CreateTestColony("c1", 200000);
        colony.TechLevel = TechnologyLevel.Level.Interstellar;
        data.Colonies.Add(colony);
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Interstellar, data.GetHighestTechLevel(), "Should be Interstellar");
    }

    /// <summary>
    /// Tests get_native_by_id.
    /// </summary>
    public static void TestGetNativeById()
    {
        PlanetPopulationData data = new();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        data.NativePopulations.Add(CreateTestNative("n2", 500000));

        NativePopulation? found = data.GetNativeById("n2");
        DotNetNativeTestSuite.AssertNotNull(found, "Should find n2");
        DotNetNativeTestSuite.AssertEqual("n2", found.Id, "ID should be n2");

        DotNetNativeTestSuite.AssertNull(data.GetNativeById("nonexistent"), "Should not find nonexistent");
    }

    /// <summary>
    /// Tests get_colony_by_id.
    /// </summary>
    public static void TestGetColonyById()
    {
        PlanetPopulationData data = new();
        data.Colonies.Add(CreateTestColony("c1", 200000));
        data.Colonies.Add(CreateTestColony("c2", 100000));

        Colony? found = data.GetColonyById("c2");
        DotNetNativeTestSuite.AssertNotNull(found, "Should find c2");
        DotNetNativeTestSuite.AssertEqual("c2", found.Id, "ID should be c2");

        DotNetNativeTestSuite.AssertNull(data.GetColonyById("nonexistent"), "Should not find nonexistent");
    }

    /// <summary>
    /// Tests get_summary.
    /// </summary>
    public static void TestGetSummary()
    {
        PlanetPopulationData data = new();
        data.BodyId = "planet_001";
        data.Profile = CreateTestProfile();
        data.Suitability = CreateTestSuitability();
        data.NativePopulations.Add(CreateTestNative("n1", 1000000));
        data.Colonies.Add(CreateTestColony("c1", 200000));

        Godot.Collections.Dictionary summary = data.GetSummary();

        DotNetNativeTestSuite.AssertEqual("planet_001", summary["body_id"].AsString(), "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(1200000, summary["total_population"].AsInt32(), "Total population should match");
        DotNetNativeTestSuite.AssertEqual(1, summary["extant_native_count"].AsInt32(), "Extant native count should match");
        DotNetNativeTestSuite.AssertEqual(1, summary["active_colony_count"].AsInt32(), "Active colony count should match");
        DotNetNativeTestSuite.AssertEqual("coexisting", summary["political_situation"].AsString(), "Political situation should match");
        DotNetNativeTestSuite.AssertEqual(8, summary["habitability_score"].AsInt32(), "Habitability score should match");
        DotNetNativeTestSuite.AssertEqual(75, summary["suitability_score"].AsInt32(), "Suitability score should match");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        PlanetPopulationData original = new();
        original.BodyId = "planet_001";
        original.GenerationSeed = 12345;
        original.Profile = CreateTestProfile();
        original.Suitability = CreateTestSuitability();
        original.NativePopulations.Add(CreateTestNative("n1", 1000000));
        original.Colonies.Add(CreateTestColony("c1", 200000));

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetPopulationData restored = PlanetPopulationData.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.BodyId, restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(original.GenerationSeed, restored.GenerationSeed, "GenerationSeed should match");
        DotNetNativeTestSuite.AssertNotNull(restored.Profile, "Profile should not be null");
        DotNetNativeTestSuite.AssertEqual(original.Profile.HabitabilityScore, restored.Profile.HabitabilityScore, "Habitability should match");
        DotNetNativeTestSuite.AssertNotNull(restored.Suitability, "Suitability should not be null");
        DotNetNativeTestSuite.AssertEqual(original.Suitability.OverallScore, restored.Suitability.OverallScore, "Suitability score should match");
        DotNetNativeTestSuite.AssertEqual(1, restored.NativePopulations.Count, "Should have 1 native");
        DotNetNativeTestSuite.AssertEqual(1, restored.Colonies.Count, "Should have 1 colony");
    }

    /// <summary>
    /// Tests serialization with no populations.
    /// </summary>
    public static void TestSerializationEmpty()
    {
        PlanetPopulationData original = new();
        original.BodyId = "empty_planet";

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetPopulationData restored = PlanetPopulationData.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual("empty_planet", restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(0, restored.NativePopulations.Count, "Should have no natives");
        DotNetNativeTestSuite.AssertEqual(0, restored.Colonies.Count, "Should have no colonies");
    }
}
