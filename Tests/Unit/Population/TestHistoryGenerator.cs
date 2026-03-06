#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for HistoryGenerator profile-driven event generation.
/// </summary>
public static class TestHistoryGenerator
{
    /// <summary>
    /// Creates a stable Earth-like profile.
    /// </summary>
    private static PlanetProfile CreateEarthLikeProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "earth_like";
        profile.HabitabilityScore = 10;
        profile.VolcanismLevel = 0.2;
        profile.TectonicActivity = 0.5;
        profile.WeatherSeverity = 0.3;
        profile.RadiationLevel = 0.1;
        profile.HasLiquidWater = true;
        profile.ContinentCount = 7;
        profile.Resources = new Dictionary
        {
            { (int)ResourceType.Type.Water, 0.9 },
            { (int)ResourceType.Type.Metals, 0.5 },
            { (int)ResourceType.Type.Silicates, 0.8 },
            { (int)ResourceType.Type.Organics, 0.7 },
            { (int)ResourceType.Type.RareElements, 0.3 },
        };
        return profile;
    }

    /// <summary>
    /// Creates a harsh volcanic profile.
    /// </summary>
    private static PlanetProfile CreateVolcanicProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "volcanic";
        profile.HabitabilityScore = 3;
        profile.VolcanismLevel = 0.9;
        profile.TectonicActivity = 0.8;
        profile.WeatherSeverity = 0.6;
        profile.RadiationLevel = 0.4;
        profile.HasLiquidWater = false;
        profile.ContinentCount = 1;
        profile.Resources = new Dictionary
        {
            { (int)ResourceType.Type.Silicates, 0.7 },
            { (int)ResourceType.Type.Metals, 0.6 },
        };
        return profile;
    }

    /// <summary>
    /// Tests event weights calculation for Earth-like world.
    /// </summary>
    public static void TestCalculateEventWeightsEarthLike()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        Godot.Collections.Dictionary weights = HistoryGenerator.CalculateEventWeights(profile);

        DotNetNativeTestSuite.AssertGreaterThan(weights.Count, 10, "Should have many event types");

        double goldenAgeWeight = weights.Get((int)HistoryEvent.EventType.GoldenAge, 0.0).AsDouble();
        DotNetNativeTestSuite.AssertGreaterThan(goldenAgeWeight, 0.5, "Earth-like should boost golden age events");
    }

    /// <summary>
    /// Tests event weights calculation for volcanic world.
    /// </summary>
    public static void TestCalculateEventWeightsVolcanic()
    {
        PlanetProfile profile = CreateVolcanicProfile();
        Godot.Collections.Dictionary weights = HistoryGenerator.CalculateEventWeights(profile);

        double disasterWeight = weights.Get((int)HistoryEvent.EventType.NaturalDisaster, 0.0).AsDouble();

        PlanetProfile earthProfile = CreateEarthLikeProfile();
        Godot.Collections.Dictionary earthWeights = HistoryGenerator.CalculateEventWeights(earthProfile);
        double earthDisaster = earthWeights.Get((int)HistoryEvent.EventType.NaturalDisaster, 0.0).AsDouble();

        DotNetNativeTestSuite.AssertGreaterThan(disasterWeight, earthDisaster, "Volcanic world should have higher disaster weight");
    }

    /// <summary>
    /// Tests event weights for low habitability.
    /// </summary>
    public static void TestCalculateEventWeightsLowHabitability()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 2;
        profile.VolcanismLevel = 0.1;
        profile.TectonicActivity = 0.1;
        profile.HasLiquidWater = false;

        Godot.Collections.Dictionary weights = HistoryGenerator.CalculateEventWeights(profile);

        double expansionWeight = weights.Get((int)HistoryEvent.EventType.Expansion, 0.0).AsDouble();
        double goldenAgeWeight = weights.Get((int)HistoryEvent.EventType.GoldenAge, 0.0).AsDouble();

        DotNetNativeTestSuite.AssertLessThan(expansionWeight, 1.0, "Low habitability should reduce expansion weight");
        DotNetNativeTestSuite.AssertLessThan(goldenAgeWeight, 0.5, "Low habitability should reduce golden age weight");
    }

    /// <summary>
    /// Tests history generation produces events.
    /// </summary>
    public static void TestGenerateHistoryProducesEvents()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory history = HistoryGenerator.GenerateHistory(
            profile, -1000, 0, rng, "Test Founding"
        );

        DotNetNativeTestSuite.AssertGreaterThan(history.Size(), 0, "Should generate at least one event");
    }

    /// <summary>
    /// Tests history generation includes founding event.
    /// </summary>
    public static void TestGenerateHistoryHasFounding()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory history = HistoryGenerator.GenerateHistory(
            profile, -1000, 0, rng, "The Beginning"
        );

        HistoryEvent founding = history.GetFoundingEvent();
        DotNetNativeTestSuite.AssertNotNull(founding, "Should have founding event");
        DotNetNativeTestSuite.AssertEqual(-1000, founding.Year, "Founding year should be -1000");
        DotNetNativeTestSuite.AssertEqual("The Beginning", founding.Title, "Founding title should match");
    }

    /// <summary>
    /// Tests history generation respects year range.
    /// </summary>
    public static void TestGenerateHistoryYearRange()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory history = HistoryGenerator.GenerateHistory(
            profile, -500, 100, rng
        );

        foreach (HistoryEvent historyEvent in history.GetAllEvents())
        {
            DotNetNativeTestSuite.AssertInRange(historyEvent.Year, -500, 100, "Event year should be in range");
        }
    }

    /// <summary>
    /// Tests history generation with invalid range.
    /// </summary>
    public static void TestGenerateHistoryInvalidRange()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory history = HistoryGenerator.GenerateHistory(
            profile, 100, -100, rng
        );

        DotNetNativeTestSuite.AssertTrue(history.IsEmpty(), "Invalid range should produce empty history");
    }

    /// <summary>
    /// Tests determinism - same seed produces same history.
    /// </summary>
    public static void TestDeterminism()
    {
        PlanetProfile profile = CreateEarthLikeProfile();

        SeededRng rng1 = new(42);
        PopulationHistory history1 = HistoryGenerator.GenerateHistory(
            profile, -1000, 0, rng1
        );

        SeededRng rng2 = new(42);
        PopulationHistory history2 = HistoryGenerator.GenerateHistory(
            profile, -1000, 0, rng2
        );

        DotNetNativeTestSuite.AssertEqual(history1.Size(), history2.Size(), "Same seed should produce same event count");

        Array<HistoryEvent> events1 = history1.GetAllEvents();
        Array<HistoryEvent> events2 = history2.GetAllEvents();

        for (int i = 0; i < events1.Count; i += 1)
        {
            DotNetNativeTestSuite.AssertEqual(events1[i].Year, events2[i].Year, $"Event {i} year should match");
            DotNetNativeTestSuite.AssertEqual(events1[i].Type, events2[i].Type, $"Event {i} type should match");
        }
    }

    /// <summary>
    /// Tests different seeds produce different histories.
    /// </summary>
    public static void TestDifferentSeeds()
    {
        PlanetProfile profile = CreateEarthLikeProfile();

        SeededRng rng1 = new(1);
        PopulationHistory history1 = HistoryGenerator.GenerateHistory(
            profile, -1000, 0, rng1
        );

        SeededRng rng2 = new(999);
        PopulationHistory history2 = HistoryGenerator.GenerateHistory(
            profile, -1000, 0, rng2
        );

        if (history1.Size() > 3 && history2.Size() > 3)
        {
            Array<HistoryEvent> events1 = history1.GetAllEvents();
            Array<HistoryEvent> events2 = history2.GetAllEvents();

            bool hasDifference = false;
            for (int i = 0; i < Math.Min(events1.Count, events2.Count); i += 1)
            {
                if (events1[i].Type != events2[i].Type)
                {
                    hasDifference = true;
                    break;
                }
            }

            DotNetNativeTestSuite.AssertTrue(
                history1.Size() != history2.Size() || hasDifference,
                "Different seeds should generally produce different histories"
            );
        }
    }

    /// <summary>
    /// Tests generated events have valid data.
    /// </summary>
    public static void TestGeneratedEventValidity()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory history = HistoryGenerator.GenerateHistory(
            profile, -1000, 0, rng
        );

        foreach (HistoryEvent historyEvent in history.GetAllEvents())
        {
            DotNetNativeTestSuite.AssertTrue(historyEvent.Title != "", "Event should have a title");
            DotNetNativeTestSuite.AssertInRange(historyEvent.Magnitude, -1.0, 1.0, "Magnitude should be in range");
        }
    }

    /// <summary>
    /// Tests profile affects event distribution.
    /// </summary>
    public static void TestProfileAffectsDistribution()
    {
        PlanetProfile volcanic = CreateVolcanicProfile();
        PlanetProfile earth = CreateEarthLikeProfile();

        int volcanicDisasters = 0;
        int earthDisasters = 0;
        int iterations = 10;

        for (int i = 0; i < iterations; i += 1)
        {
            SeededRng rngV = new(i * 100);
            PopulationHistory historyV = HistoryGenerator.GenerateHistory(
                volcanic, -1000, 0, rngV
            );
            volcanicDisasters += historyV.GetEventsByType(HistoryEvent.EventType.NaturalDisaster).Count;

            SeededRng rngE = new(i * 100);
            PopulationHistory historyE = HistoryGenerator.GenerateHistory(
                earth, -1000, 0, rngE
            );
            earthDisasters += historyE.GetEventsByType(HistoryEvent.EventType.NaturalDisaster).Count;
        }

        DotNetNativeTestSuite.AssertTrue(
            volcanicDisasters >= earthDisasters * 0.5,
            "Volcanic world should not have drastically fewer disasters than Earth-like"
        );
    }

    /// <summary>
    /// Tests short time span produces fewer events.
    /// </summary>
    public static void TestShortTimeSpan()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory shortHistory = HistoryGenerator.GenerateHistory(
            profile, -50, 0, rng
        );

        DotNetNativeTestSuite.AssertLessThan(shortHistory.Size(), 10, "Short time span should produce few events");
    }

    /// <summary>
    /// Tests long time span produces more events.
    /// </summary>
    public static void TestLongTimeSpan()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory longHistory = HistoryGenerator.GenerateHistory(
            profile, -5000, 0, rng
        );

        DotNetNativeTestSuite.AssertGreaterThan(longHistory.Size(), 20, "Long time span should produce many events");
    }

    /// <summary>
    /// Tests events have appropriate magnitudes for their types.
    /// </summary>
    public static void TestEventMagnitudeAppropriateness()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        SeededRng rng = new(12345);

        PopulationHistory history = HistoryGenerator.GenerateHistory(
            profile, -2000, 0, rng
        );

        foreach (HistoryEvent historyEvent in history.GetAllEvents())
        {
            if (HistoryEvent.IsTypicallyHarmful(historyEvent.Type))
            {
                DotNetNativeTestSuite.AssertLessThan(
                    historyEvent.Magnitude,
                    0.5,
                    "Harmful event type should not have strongly positive magnitude"
                );
            }
            else if (HistoryEvent.IsTypicallyBeneficial(historyEvent.Type))
            {
                DotNetNativeTestSuite.AssertGreaterThan(
                    historyEvent.Magnitude,
                    -0.5,
                    "Beneficial event type should not have strongly negative magnitude"
                );
            }
        }
    }

    /// <summary>
    /// Tests resource-poor worlds have more famine.
    /// </summary>
    public static void TestResourcePoorFamine()
    {
        PlanetProfile poor = new();
        poor.HabitabilityScore = 4;
        poor.HasLiquidWater = false;
        poor.Resources = new Godot.Collections.Dictionary { { (int)ResourceType.Type.Silicates, 0.5 } };

        Godot.Collections.Dictionary weights = HistoryGenerator.CalculateEventWeights(poor);
        double famineWeight = weights.Get((int)HistoryEvent.EventType.Famine, 0.0).AsDouble();

        DotNetNativeTestSuite.AssertGreaterThan(famineWeight, 1.0, "Resource-poor world should have elevated famine weight");
    }
}
