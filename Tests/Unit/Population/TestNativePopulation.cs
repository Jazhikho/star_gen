#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for NativePopulation data model.
/// </summary>
public static class TestNativePopulation
{
    /// <summary>
    /// Creates a test population.
    /// </summary>
    private static NativePopulation CreateTestPopulation()
    {
        NativePopulation pop = new();
        pop.Id = "test_native_001";
        pop.Name = "Testani";
        pop.BodyId = "planet_001";
        pop.OriginYear = -10000;
        pop.Population = 1000000;
        pop.PeakPopulation = 1200000;
        pop.PeakPopulationYear = -500;
        pop.TechLevel = TechnologyLevel.Level.Industrial;
        pop.IsExtant = true;
        pop.PrimaryBiome = "Forest";
        pop.TerritorialControl = 0.6;
        pop.CulturalTraits = new Godot.Collections.Array<string> { "seafaring", "mercantile" };

        pop.Government.Regime = GovernmentType.Regime.Constitutional;
        pop.Government.Legitimacy = 0.7;

        pop.History.AddNewEvent(
            HistoryEvent.EventType.Founding,
            -10000,
            "Emergence",
            "The beginning"
        );

        return pop;
    }

    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        NativePopulation pop = new();
        DotNetNativeTestSuite.AssertEqual("", pop.Id, "Default ID should be empty");
        DotNetNativeTestSuite.AssertEqual(0, pop.Population, "Default population should be 0");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, pop.TechLevel, "Default tech level should be StoneAge");
        DotNetNativeTestSuite.AssertTrue(pop.IsExtant, "Default should be extant");
        DotNetNativeTestSuite.AssertNotNull(pop.Government, "Government should not be null");
        DotNetNativeTestSuite.AssertNotNull(pop.History, "History should not be null");
    }

    /// <summary>
    /// Tests get_age for extant population.
    /// </summary>
    public static void TestGetAgeExtant()
    {
        NativePopulation pop = CreateTestPopulation();
        pop.OriginYear = -5000;

        int age = pop.GetAge(0);
        DotNetNativeTestSuite.AssertEqual(5000, age, "Age should be 5000 years");
    }

    /// <summary>
    /// Tests get_age for extinct population.
    /// </summary>
    public static void TestGetAgeExtinct()
    {
        NativePopulation pop = CreateTestPopulation();
        pop.OriginYear = -5000;
        pop.IsExtant = false;
        pop.ExtinctionYear = -1000;

        int age = pop.GetAge(0);
        DotNetNativeTestSuite.AssertEqual(4000, age, "Age should be 4000 years (origin to extinction)");
    }

    /// <summary>
    /// Tests get_growth_state.
    /// </summary>
    public static void TestGetGrowthState()
    {
        NativePopulation pop = CreateTestPopulation();

        pop.Population = 1200000;
        pop.PeakPopulation = 1200000;
        DotNetNativeTestSuite.AssertEqual("growing", pop.GetGrowthState(), "Should be growing at peak");

        pop.Population = 900000;
        DotNetNativeTestSuite.AssertEqual("stable", pop.GetGrowthState(), "Should be stable");

        pop.Population = 400000;
        DotNetNativeTestSuite.AssertEqual("declining", pop.GetGrowthState(), "Should be declining");

        pop.IsExtant = false;
        DotNetNativeTestSuite.AssertEqual("extinct", pop.GetGrowthState(), "Should be extinct");
    }

    /// <summary>
    /// Tests can_spaceflight.
    /// </summary>
    public static void TestCanSpaceflight()
    {
        NativePopulation pop = new();

        pop.TechLevel = TechnologyLevel.Level.Industrial;
        DotNetNativeTestSuite.AssertFalse(pop.CanSpaceflight(), "Industrial should not have spaceflight");

        pop.TechLevel = TechnologyLevel.Level.Spacefaring;
        DotNetNativeTestSuite.AssertTrue(pop.CanSpaceflight(), "Spacefaring should have spaceflight");
    }

    /// <summary>
    /// Tests can_colonize.
    /// </summary>
    public static void TestCanColonize()
    {
        NativePopulation pop = new();

        pop.TechLevel = TechnologyLevel.Level.Spacefaring;
        DotNetNativeTestSuite.AssertFalse(pop.CanColonize(), "Spacefaring should not colonize");

        pop.TechLevel = TechnologyLevel.Level.Interstellar;
        DotNetNativeTestSuite.AssertTrue(pop.CanColonize(), "Interstellar should colonize");
    }

    /// <summary>
    /// Tests get_regime.
    /// </summary>
    public static void TestGetRegime()
    {
        NativePopulation pop = CreateTestPopulation();
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Constitutional, pop.GetRegime(), "Regime should match");
    }

    /// <summary>
    /// Tests is_politically_stable.
    /// </summary>
    public static void TestIsPoliticallyStable()
    {
        NativePopulation pop = CreateTestPopulation();
        pop.Government.Legitimacy = 0.7;
        pop.Government.Regime = GovernmentType.Regime.Constitutional;

        DotNetNativeTestSuite.AssertTrue(pop.IsPoliticallyStable(), "Should be stable with high legitimacy");

        pop.Government.Legitimacy = 0.1;
        DotNetNativeTestSuite.AssertFalse(pop.IsPoliticallyStable(), "Should not be stable with low legitimacy");
    }

    /// <summary>
    /// Tests record_extinction.
    /// </summary>
    public static void TestRecordExtinction()
    {
        NativePopulation pop = CreateTestPopulation();
        DotNetNativeTestSuite.AssertTrue(pop.IsExtant, "Should start extant");

        pop.RecordExtinction(-100, "asteroid impact");

        DotNetNativeTestSuite.AssertFalse(pop.IsExtant, "Should be extinct");
        DotNetNativeTestSuite.AssertEqual(-100, pop.ExtinctionYear, "Extinction year should match");
        DotNetNativeTestSuite.AssertEqual("asteroid impact", pop.ExtinctionCause, "Extinction cause should match");
        DotNetNativeTestSuite.AssertEqual(0, pop.Population, "Population should be 0");
    }

    /// <summary>
    /// Tests update_peak_population.
    /// </summary>
    public static void TestUpdatePeakPopulation()
    {
        NativePopulation pop = CreateTestPopulation();
        pop.Population = 500000;
        pop.PeakPopulation = 400000;

        pop.UpdatePeakPopulation(-50);

        DotNetNativeTestSuite.AssertEqual(500000, pop.PeakPopulation, "Peak should update");
        DotNetNativeTestSuite.AssertEqual(-50, pop.PeakPopulationYear, "Peak year should update");
    }

    /// <summary>
    /// Tests update_peak_population does not update if lower.
    /// </summary>
    public static void TestUpdatePeakPopulationNoUpdate()
    {
        NativePopulation pop = CreateTestPopulation();
        pop.Population = 300000;
        pop.PeakPopulation = 400000;
        pop.PeakPopulationYear = -100;

        pop.UpdatePeakPopulation(0);

        DotNetNativeTestSuite.AssertEqual(400000, pop.PeakPopulation, "Peak should not update");
        DotNetNativeTestSuite.AssertEqual(-100, pop.PeakPopulationYear, "Peak year should not update");
    }

    /// <summary>
    /// Tests get_summary.
    /// </summary>
    public static void TestGetSummary()
    {
        NativePopulation pop = CreateTestPopulation();
        Godot.Collections.Dictionary summary = pop.GetSummary();

        DotNetNativeTestSuite.AssertEqual("test_native_001", summary["id"].AsString(), "ID should match");
        DotNetNativeTestSuite.AssertEqual("Testani", summary["name"].AsString(), "Name should match");
        DotNetNativeTestSuite.AssertEqual(1000000, summary["population"].AsInt32(), "Population should match");
        DotNetNativeTestSuite.AssertEqual("Industrial", summary["tech_level"].AsString(), "Tech level should match");
        DotNetNativeTestSuite.AssertEqual("Constitutional Government", summary["regime"].AsString(), "Regime should match");
        DotNetNativeTestSuite.AssertTrue(summary["is_extant"].AsBool(), "Should be extant");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        NativePopulation original = CreateTestPopulation();

        Godot.Collections.Dictionary data = original.ToDictionary();
        NativePopulation restored = NativePopulation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Id, restored.Id, "ID should match");
        DotNetNativeTestSuite.AssertEqual(original.Name, restored.Name, "Name should match");
        DotNetNativeTestSuite.AssertEqual(original.BodyId, restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(original.OriginYear, restored.OriginYear, "OriginYear should match");
        DotNetNativeTestSuite.AssertEqual(original.Population, restored.Population, "Population should match");
        DotNetNativeTestSuite.AssertEqual(original.PeakPopulation, restored.PeakPopulation, "PeakPopulation should match");
        DotNetNativeTestSuite.AssertEqual(original.TechLevel, restored.TechLevel, "TechLevel should match");
        DotNetNativeTestSuite.AssertEqual(original.IsExtant, restored.IsExtant, "IsExtant should match");
        DotNetNativeTestSuite.AssertEqual(original.PrimaryBiome, restored.PrimaryBiome, "PrimaryBiome should match");
        DotNetNativeTestSuite.AssertFloatNear(original.TerritorialControl, restored.TerritorialControl, 0.001, "TerritorialControl should match");
    }

    /// <summary>
    /// Tests cultural_traits serialization.
    /// </summary>
    public static void TestCulturalTraitsSerialization()
    {
        NativePopulation original = CreateTestPopulation();

        Godot.Collections.Dictionary data = original.ToDictionary();
        NativePopulation restored = NativePopulation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.CulturalTraits.Count, restored.CulturalTraits.Count, "Cultural traits count should match");
        foreach (string trait in original.CulturalTraits)
        {
            DotNetNativeTestSuite.AssertTrue(restored.CulturalTraits.Contains(trait), $"Should contain trait {trait}");
        }
    }

    /// <summary>
    /// Tests government serialization.
    /// </summary>
    public static void TestGovernmentSerialization()
    {
        NativePopulation original = CreateTestPopulation();

        Godot.Collections.Dictionary data = original.ToDictionary();
        NativePopulation restored = NativePopulation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Government.Regime, restored.Government.Regime, "Regime should match");
        DotNetNativeTestSuite.AssertFloatNear(original.Government.Legitimacy, restored.Government.Legitimacy, 0.001, "Legitimacy should match");
    }

    /// <summary>
    /// Tests history serialization.
    /// </summary>
    public static void TestHistorySerialization()
    {
        NativePopulation original = CreateTestPopulation();

        Godot.Collections.Dictionary data = original.ToDictionary();
        NativePopulation restored = NativePopulation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.History.Size(), restored.History.Size(), "History size should match");
    }

    /// <summary>
    /// Tests extinct population serialization.
    /// </summary>
    public static void TestExtinctPopulationSerialization()
    {
        NativePopulation original = CreateTestPopulation();
        original.RecordExtinction(-100, "plague");

        Godot.Collections.Dictionary data = original.ToDictionary();
        NativePopulation restored = NativePopulation.FromDictionary(data);

        DotNetNativeTestSuite.AssertFalse(restored.IsExtant, "Should be extinct");
        DotNetNativeTestSuite.AssertEqual(-100, restored.ExtinctionYear, "Extinction year should match");
        DotNetNativeTestSuite.AssertEqual("plague", restored.ExtinctionCause, "Extinction cause should match");
    }
}
