using Godot.Collections;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic history generation for populations.
/// </summary>
public static class HistoryGenerator
{
    private const int MinEventSpacing = 10;
    private const int BaseEventInterval = 25;

    /// <summary>
    /// Base event weights before profile modifiers.
    /// </summary>
    public static readonly Dictionary BaseWeights = new()
    {
        [(int)HistoryEvent.EventType.NaturalDisaster] = 1.0,
        [(int)HistoryEvent.EventType.Plague] = 0.8,
        [(int)HistoryEvent.EventType.Famine] = 0.7,
        [(int)HistoryEvent.EventType.War] = 1.0,
        [(int)HistoryEvent.EventType.CivilWar] = 0.6,
        [(int)HistoryEvent.EventType.TechAdvancement] = 1.2,
        [(int)HistoryEvent.EventType.Expansion] = 1.0,
        [(int)HistoryEvent.EventType.PoliticalChange] = 0.9,
        [(int)HistoryEvent.EventType.Migration] = 0.7,
        [(int)HistoryEvent.EventType.Collapse] = 0.2,
        [(int)HistoryEvent.EventType.GoldenAge] = 0.5,
        [(int)HistoryEvent.EventType.CulturalShift] = 0.6,
        [(int)HistoryEvent.EventType.Treaty] = 0.4,
        [(int)HistoryEvent.EventType.Discovery] = 0.8,
        [(int)HistoryEvent.EventType.Construction] = 0.7,
        [(int)HistoryEvent.EventType.Leader] = 0.9,
    };

    /// <summary>
    /// Generates a history across a time span.
    /// </summary>
    public static PopulationHistory GenerateHistory(
        PlanetProfile profile,
        int startYear,
        int endYear,
        SeededRng rng,
        string foundingTitle = "Founding")
    {
        PopulationHistory history = new();
        if (startYear >= endYear)
        {
            return history;
        }

        HistoryEvent founding = new(
            HistoryEvent.EventType.Founding,
            startYear,
            foundingTitle,
            "The beginning of recorded history.",
            0.5);
        history.AddEvent(founding);

        Dictionary weights = CalculateEventWeights(profile);
        int currentYear = startYear + CalculateNextInterval(profile, rng);
        Dictionary lastEventYears = new();

        while (currentYear < endYear)
        {
            HistoryEvent.EventType eventType = PickEventType(weights, lastEventYears, currentYear, rng);
            HistoryEvent historyEvent = GenerateEvent(eventType, currentYear, profile, rng);
            history.AddEvent(historyEvent);
            lastEventYears[(int)eventType] = currentYear;
            currentYear += CalculateNextInterval(profile, rng);
        }

        return history;
    }

    private static Dictionary CalculateEventWeights(PlanetProfile profile)
    {
        Dictionary weights = new();
        foreach (int typeKey in BaseWeights.Keys)
        {
            weights[typeKey] = (double)BaseWeights[typeKey];
        }

        double disasterModifier = 1.0 + profile.VolcanismLevel + profile.TectonicActivity;
        weights[(int)HistoryEvent.EventType.NaturalDisaster] =
            (double)weights[(int)HistoryEvent.EventType.NaturalDisaster] * disasterModifier;

        if (profile.WeatherSeverity > 0.5)
        {
            weights[(int)HistoryEvent.EventType.NaturalDisaster] =
                (double)weights[(int)HistoryEvent.EventType.NaturalDisaster] * (1.0 + profile.WeatherSeverity);
            weights[(int)HistoryEvent.EventType.Famine] =
                (double)weights[(int)HistoryEvent.EventType.Famine] * (1.0 + (profile.WeatherSeverity * 0.5));
        }

        if (profile.HabitabilityScore < 5)
        {
            double hardshipModifier = 1.5 - (profile.HabitabilityScore * 0.1);
            weights[(int)HistoryEvent.EventType.Famine] = (double)weights[(int)HistoryEvent.EventType.Famine] * hardshipModifier;
            weights[(int)HistoryEvent.EventType.Plague] = (double)weights[(int)HistoryEvent.EventType.Plague] * hardshipModifier;
            weights[(int)HistoryEvent.EventType.Collapse] = (double)weights[(int)HistoryEvent.EventType.Collapse] * hardshipModifier;
            weights[(int)HistoryEvent.EventType.Expansion] = (double)weights[(int)HistoryEvent.EventType.Expansion] * 0.5;
            weights[(int)HistoryEvent.EventType.GoldenAge] = (double)weights[(int)HistoryEvent.EventType.GoldenAge] * 0.3;
        }

        if (profile.HabitabilityScore >= 7)
        {
            double prosperityModifier = 1.0 + ((profile.HabitabilityScore - 7) * 0.15);
            weights[(int)HistoryEvent.EventType.GoldenAge] = (double)weights[(int)HistoryEvent.EventType.GoldenAge] * prosperityModifier;
            weights[(int)HistoryEvent.EventType.Expansion] = (double)weights[(int)HistoryEvent.EventType.Expansion] * prosperityModifier;
            weights[(int)HistoryEvent.EventType.TechAdvancement] =
                (double)weights[(int)HistoryEvent.EventType.TechAdvancement] * prosperityModifier;
        }

        if (!profile.HasLiquidWater)
        {
            weights[(int)HistoryEvent.EventType.Famine] = (double)weights[(int)HistoryEvent.EventType.Famine] * 1.5;
        }

        if (profile.RadiationLevel > 0.5)
        {
            weights[(int)HistoryEvent.EventType.Plague] =
                (double)weights[(int)HistoryEvent.EventType.Plague] * (1.0 + profile.RadiationLevel);
        }

        int resourceCount = profile.Resources.Count;
        if (resourceCount >= 5)
        {
            weights[(int)HistoryEvent.EventType.War] = (double)weights[(int)HistoryEvent.EventType.War] * 1.2;
            weights[(int)HistoryEvent.EventType.TechAdvancement] =
                (double)weights[(int)HistoryEvent.EventType.TechAdvancement] * 1.2;
        }
        else if (resourceCount <= 2)
        {
            weights[(int)HistoryEvent.EventType.Famine] = (double)weights[(int)HistoryEvent.EventType.Famine] * 1.3;
            weights[(int)HistoryEvent.EventType.Migration] = (double)weights[(int)HistoryEvent.EventType.Migration] * 1.5;
        }

        if (profile.ContinentCount >= 3)
        {
            weights[(int)HistoryEvent.EventType.War] = (double)weights[(int)HistoryEvent.EventType.War] * 1.2;
            weights[(int)HistoryEvent.EventType.Treaty] = (double)weights[(int)HistoryEvent.EventType.Treaty] * 1.3;
            weights[(int)HistoryEvent.EventType.CulturalShift] =
                (double)weights[(int)HistoryEvent.EventType.CulturalShift] * 1.2;
        }

        return weights;
    }

    private static int CalculateNextInterval(PlanetProfile profile, SeededRng rng)
    {
        double intervalBase = BaseEventInterval;
        if (profile.HabitabilityScore < 5)
        {
            intervalBase *= 0.7;
        }
        else if (profile.HabitabilityScore >= 8)
        {
            intervalBase *= 1.3;
        }

        double interval = intervalBase * rng.RandfRange(0.5f, 1.5f);
        return System.Math.Max(5, (int)System.Math.Round(interval));
    }

    private static HistoryEvent.EventType PickEventType(
        Dictionary weights,
        Dictionary lastYears,
        int currentYear,
        SeededRng rng)
    {
        Array<int> eligibleTypes = new();
        Array<float> eligibleWeights = new();

        foreach (int typeKey in weights.Keys)
        {
            double weight = (double)weights[typeKey];
            if (lastYears.ContainsKey(typeKey))
            {
                int yearsSince = currentYear - (int)lastYears[typeKey];
                if (yearsSince < MinEventSpacing)
                {
                    weight *= 0.1;
                }
                else if (yearsSince < MinEventSpacing * 2)
                {
                    weight *= 0.5;
                }
            }

            if (weight > 0.01)
            {
                eligibleTypes.Add(typeKey);
                eligibleWeights.Add((float)weight);
            }
        }

        if (eligibleTypes.Count == 0)
        {
            return HistoryEvent.EventType.PoliticalChange;
        }

        int? result = rng.WeightedChoice(eligibleTypes, eligibleWeights);
        return result.HasValue ? (HistoryEvent.EventType)result.Value : HistoryEvent.EventType.PoliticalChange;
    }

    private static HistoryEvent GenerateEvent(
        HistoryEvent.EventType type,
        int year,
        PlanetProfile profile,
        SeededRng rng)
    {
        string title = GenerateEventTitle(type, profile, rng);
        string description = GenerateEventDescription(type);
        double magnitude = GenerateEventMagnitude(type, rng);

        HistoryEvent historyEvent = new(type, year, title, description, magnitude)
        {
            PopulationDelta = EstimatePopulationDelta(type, magnitude, rng),
        };

        return historyEvent;
    }

    private static string GenerateEventTitle(
        HistoryEvent.EventType type,
        PlanetProfile profile,
        SeededRng rng)
    {
        return type switch
        {
            HistoryEvent.EventType.NaturalDisaster => PickString(
                new Array<string>
                {
                    "Great Earthquake",
                    "Volcanic Eruption",
                    "Massive Flood",
                    "Terrible Storm",
                    "Meteor Strike",
                    profile.VolcanismLevel > 0.5 ? "Volcanic Winter" : string.Empty,
                    profile.TectonicActivity > 0.5 ? "Tectonic Upheaval" : string.Empty,
                },
                rng),
            HistoryEvent.EventType.Plague => PickString(
                new Array<string> { "The Great Plague", "Red Death", "Wasting Sickness", "Silent Fever", "The Blight" },
                rng),
            HistoryEvent.EventType.Famine => PickString(
                new Array<string> { "The Great Famine", "Years of Want", "The Hungry Time", "Crop Failure", "The Withering" },
                rng),
            HistoryEvent.EventType.War => PickString(
                new Array<string> { "The Great War", "War of Succession", "Border Conflict", "The Conquest", "War of Independence" },
                rng),
            HistoryEvent.EventType.CivilWar => PickString(
                new Array<string> { "The Civil War", "The Rebellion", "The Uprising", "War of Brothers", "The Revolution" },
                rng),
            HistoryEvent.EventType.TechAdvancement => PickString(
                new Array<string> { "Age of Innovation", "The Discovery", "Technical Revolution", "Scientific Breakthrough", "New Era" },
                rng),
            HistoryEvent.EventType.Expansion => PickString(
                new Array<string> { "The Expansion", "New Territories", "The Settling", "Frontier Era", "Colonial Period" },
                rng),
            HistoryEvent.EventType.PoliticalChange => PickString(
                new Array<string> { "The Reform", "Change of Power", "New Order", "The Transition", "Political Upheaval" },
                rng),
            HistoryEvent.EventType.Migration => PickString(
                new Array<string> { "The Great Migration", "The Exodus", "Mass Movement", "The Resettlement", "Diaspora" },
                rng),
            HistoryEvent.EventType.Collapse => PickString(
                new Array<string> { "The Collapse", "The Fall", "Dark Age Begins", "The Decline", "End of an Era" },
                rng),
            HistoryEvent.EventType.GoldenAge => PickString(
                new Array<string> { "The Golden Age", "Era of Prosperity", "The Renaissance", "Age of Plenty", "The Flowering" },
                rng),
            HistoryEvent.EventType.CulturalShift => PickString(
                new Array<string> { "Cultural Revolution", "The Awakening", "New Beliefs", "The Reformation", "Age of Reason" },
                rng),
            HistoryEvent.EventType.Treaty => PickString(
                new Array<string> { "The Grand Treaty", "Peace Accord", "The Alliance", "Trade Agreement", "The Pact" },
                rng),
            HistoryEvent.EventType.Discovery => PickString(
                new Array<string> { "The Great Discovery", "New Horizons", "Revelation", "The Finding", "Breakthrough" },
                rng),
            HistoryEvent.EventType.Construction => PickString(
                new Array<string> { "The Great Work", "Monument Rising", "Infrastructure Boom", "The Building", "Grand Project" },
                rng),
            HistoryEvent.EventType.Leader => PickString(
                new Array<string> { "Rise of a Leader", "The Ruler", "New Dynasty", "The Reformer", "The Tyrant" },
                rng),
            _ => "Historical Event",
        };
    }

    private static string GenerateEventDescription(HistoryEvent.EventType type)
    {
        return type switch
        {
            HistoryEvent.EventType.NaturalDisaster => "A devastating natural disaster struck the population.",
            HistoryEvent.EventType.Plague => "A deadly disease spread through the population.",
            HistoryEvent.EventType.Famine => "Food shortages led to widespread hunger and hardship.",
            HistoryEvent.EventType.War => "Armed conflict erupted, changing the political landscape.",
            HistoryEvent.EventType.CivilWar => "Internal divisions led to violent conflict within the population.",
            HistoryEvent.EventType.TechAdvancement => "Significant technological progress transformed society.",
            HistoryEvent.EventType.Expansion => "The population expanded into new territories.",
            HistoryEvent.EventType.PoliticalChange => "The political structure underwent significant change.",
            HistoryEvent.EventType.Migration => "Large numbers of people moved to new regions.",
            HistoryEvent.EventType.Collapse => "Existing social structures collapsed under pressure.",
            HistoryEvent.EventType.GoldenAge => "A period of unprecedented prosperity and cultural achievement began.",
            HistoryEvent.EventType.CulturalShift => "Major changes in beliefs, values, or customs swept through society.",
            HistoryEvent.EventType.Treaty => "A significant agreement was reached between parties.",
            HistoryEvent.EventType.Discovery => "An important discovery changed understanding of the world.",
            HistoryEvent.EventType.Construction => "A major construction project was completed.",
            HistoryEvent.EventType.Leader => "A notable leader rose to prominence.",
            _ => "A significant event occurred.",
        };
    }

    private static double GenerateEventMagnitude(HistoryEvent.EventType type, SeededRng rng)
    {
        double baseMagnitude = 0.0;
        const double variance = 0.3;

        if (HistoryEvent.IsTypicallyHarmful(type))
        {
            baseMagnitude = -0.5;
        }
        else if (HistoryEvent.IsTypicallyBeneficial(type))
        {
            baseMagnitude = 0.5;
        }

        double magnitude = baseMagnitude + rng.RandfRange((float)-variance, (float)variance);
        return System.Math.Clamp(magnitude, -1.0, 1.0);
    }

    private static int EstimatePopulationDelta(
        HistoryEvent.EventType type,
        double magnitude,
        SeededRng rng)
    {
        int baseDelta = type switch
        {
            HistoryEvent.EventType.NaturalDisaster => rng.RandiRange(-10000, -1000),
            HistoryEvent.EventType.Plague => rng.RandiRange(-50000, -5000),
            HistoryEvent.EventType.Famine => rng.RandiRange(-20000, -2000),
            HistoryEvent.EventType.War => rng.RandiRange(-30000, -3000),
            HistoryEvent.EventType.CivilWar => rng.RandiRange(-20000, -2000),
            HistoryEvent.EventType.Collapse => rng.RandiRange(-100000, -10000),
            HistoryEvent.EventType.Expansion => rng.RandiRange(5000, 50000),
            HistoryEvent.EventType.GoldenAge => rng.RandiRange(10000, 100000),
            HistoryEvent.EventType.Migration => rng.RandiRange(-10000, 10000),
            _ => 0,
        };

        double scaled = baseDelta * System.Math.Abs(magnitude);
        return (int)System.Math.Round(scaled);
    }

    private static string PickString(Array<string> values, SeededRng rng)
    {
        Array<string> filtered = new();
        foreach (string value in values)
        {
            if (!string.IsNullOrEmpty(value))
            {
                filtered.Add(value);
            }
        }

        return filtered[rng.RandiRange(0, filtered.Count - 1)];
    }
}
