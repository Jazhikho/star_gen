using Godot.Collections;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic colony generation.
/// </summary>
public static class ColonyGenerator
{
    /// <summary>
    /// Generates a single colony for a profile, or null when colonization is not possible.
    /// </summary>
    public static Colony? Generate(
        PlanetProfile profile,
        ColonySuitability suitability,
        Array<NativePopulation> existingNatives,
        SeededRng rng,
        int currentYear = 0,
        int minHistoryYears = 50,
        int maxHistoryYears = 500,
        TechnologyLevel.Level foundingTechLevel = TechnologyLevel.Level.Interstellar,
        string foundingCivilizationId = "civ_unknown",
        string foundingCivilizationName = "Unknown Civilization")
    {
        if (!suitability.IsColonizable())
        {
            return null;
        }

        Colony colony = new();
        colony.Id = $"colony_{profile.BodyId}_{rng.Randi()}";
        colony.Type = DetermineColonyType(profile, suitability, rng);
        colony.BodyId = profile.BodyId;
        colony.FoundingCivilizationId = foundingCivilizationId;
        colony.FoundingCivilizationName = foundingCivilizationName;
        colony.TechLevel = foundingTechLevel;

        colony.Name = GenerateColonyName(colony.Type, rng);

        int historyYears = rng.RandiRange(minHistoryYears, maxHistoryYears);
        colony.FoundingYear = currentYear - historyYears;
        colony.Government = GenerateGovernment(colony.Type, rng);
        colony.Government.RegimeEstablishedYear = colony.FoundingYear;

        int initialPopulation = CalculateInitialPopulation(colony.Type, rng);
        int yearsElapsed = currentYear - colony.FoundingYear;
        colony.Population = ProjectColonyPopulation(initialPopulation, yearsElapsed, suitability, colony.Type, rng);
        colony.PeakPopulation = (int)System.Math.Round(colony.Population * rng.RandfRange(1.0f, 1.3f));
        colony.PeakPopulationYear = currentYear - rng.RandiRange(0, System.Math.Max(1, (int)(yearsElapsed / 4.0)));
        colony.TerritorialControl = CalculateTerritorialControl(suitability, existingNatives, yearsElapsed, rng);
        colony.PrimaryIndustry = DeterminePrimaryIndustry(profile, colony.Type, rng);
        colony.SelfSufficiency = CalculateSelfSufficiency(profile, suitability, yearsElapsed, rng);

        if (existingNatives.Count > 0)
        {
            EstablishNativeRelations(colony, existingNatives, currentYear, rng);
        }

        colony.History = HistoryGenerator.GenerateHistory(
            profile,
            colony.FoundingYear,
            currentYear,
            rng,
            colony.Name + " Founded");
        AddNativeEventsToHistory(colony);

        if (rng.Randf() < 0.05 && yearsElapsed > 100)
        {
            int abandonmentYear = currentYear - rng.RandiRange(10, (int)(yearsElapsed / 2.0));
            Array<string> reasons = new()
            {
                "resource depletion",
                "hostile conditions",
                "native conflict",
                "economic collapse",
                "unknown",
            };
            colony.RecordAbandonment(abandonmentYear, reasons[rng.RandiRange(0, reasons.Count - 1)]);
        }

        if (!colony.IsIndependent && yearsElapsed > 200 && rng.Randf() < 0.2)
        {
            int independenceYear = currentYear - rng.RandiRange(0, 100);
            colony.RecordIndependence(independenceYear);
        }

        return colony;
    }

    private static ColonyType.Type DetermineColonyType(
        PlanetProfile profile,
        ColonySuitability suitability,
        SeededRng rng)
    {
        Dictionary weights = new();
        for (int typeValue = 0; typeValue < ColonyType.Count(); typeValue += 1)
        {
            weights[typeValue] = 1.0;
        }

        if (suitability.OverallScore >= 70)
        {
            MultiplyWeight(weights, ColonyType.Type.Settlement, 2.0);
            MultiplyWeight(weights, ColonyType.Type.Agricultural, 1.5);
        }

        int resourceScore = suitability.GetFactorScore(ColonySuitability.FactorType.Resources);
        if (resourceScore >= 60)
        {
            MultiplyWeight(weights, ColonyType.Type.Corporate, 1.5);
            MultiplyWeight(weights, ColonyType.Type.Industrial, 1.5);
        }

        if (suitability.OverallScore < 40)
        {
            MultiplyWeight(weights, ColonyType.Type.Military, 2.0);
            MultiplyWeight(weights, ColonyType.Type.Scientific, 2.0);
            MultiplyWeight(weights, ColonyType.Type.Settlement, 0.3);
        }

        if (profile.HasLiquidWater && profile.OceanCoverage < 0.9)
        {
            MultiplyWeight(weights, ColonyType.Type.Agricultural, 1.5);
        }

        Array<int> types = new();
        Array<float> weightValues = new();
        foreach (int typeKey in weights.Keys)
        {
            types.Add(typeKey);
            weightValues.Add((float)(double)weights[typeKey]);
        }

        int? selected = rng.WeightedChoice(types, weightValues);
        return selected.HasValue ? (ColonyType.Type)selected.Value : ColonyType.Type.Settlement;
    }

    private static string GenerateColonyName(ColonyType.Type type, SeededRng rng)
    {
        Array<string> prefixes = new()
        {
            "New", "Nova", "Port", "Fort", "Camp", "Station", "Haven", "Point", "Landing", "Base",
        };
        Array<string> roots = new()
        {
            "Terra", "Sol", "Vega", "Hope", "Dawn", "Unity", "Prospect", "Fortune", "Pioneer", "Frontier",
            "Avalon", "Elysium", "Arcadia", "Horizon",
        };
        Array<string> suffixes = new()
        {
            string.Empty, " Prime", " Alpha", " Colony", " Station", " Settlement", " Outpost", " Base",
        };

        if (type == ColonyType.Type.Military || type == ColonyType.Type.Scientific)
        {
            prefixes = new Array<string> { "Fort", "Station", "Base", "Outpost", "Camp" };
            suffixes = new Array<string> { " Base", " Station", " Outpost", " Alpha", " Prime" };
        }

        string prefix = prefixes[rng.RandiRange(0, prefixes.Count - 1)];
        string root = roots[rng.RandiRange(0, roots.Count - 1)];
        string suffix = suffixes[rng.RandiRange(0, suffixes.Count - 1)];

        if (rng.Randf() < 0.3)
        {
            return root + suffix;
        }

        return prefix + " " + root + suffix;
    }

    private static Government GenerateGovernment(ColonyType.Type type, SeededRng rng)
    {
        Government government = Government.CreateColonyDefault(rng, ColonyType.ToStringName(type));
        government.Regime = ColonyType.TypicalStartingRegime(type);
        return government;
    }

    private static int CalculateInitialPopulation(ColonyType.Type type, SeededRng rng)
    {
        int basePopulation = ColonyType.TypicalInitialPopulation(type);
        double variance = rng.RandfRange(0.7f, 1.3f);
        return (int)System.Math.Round(basePopulation * variance);
    }

    private static int ProjectColonyPopulation(
        int initialPopulation,
        int years,
        ColonySuitability suitability,
        ColonyType.Type type,
        SeededRng rng)
    {
        double growthRate = suitability.BaseGrowthRate * ColonyType.GrowthRateModifier(type);
        growthRate *= rng.RandfRange(0.8f, 1.2f);
        return SuitabilityCalculator.ProjectPopulation(initialPopulation, years, growthRate, suitability.CarryingCapacity);
    }

    private static double CalculateTerritorialControl(
        ColonySuitability suitability,
        Array<NativePopulation> existingNatives,
        int years,
        SeededRng rng)
    {
        double baseControl = 0.05 + ((double)suitability.OverallScore / 100.0 * 0.1);
        double timeFactor = System.Math.Min(years / 200.0, 1.0);
        baseControl *= 1.0 + timeFactor;

        double nativeControl = 0.0;
        foreach (NativePopulation native in existingNatives)
        {
            if (native.IsExtant)
            {
                nativeControl += native.TerritorialControl;
            }
        }

        double available = System.Math.Max(0.0, 1.0 - (nativeControl * 0.8));
        double control = System.Math.Min(baseControl, available);
        control *= rng.RandfRange(0.7f, 1.3f);
        return System.Math.Clamp(control, 0.01, 0.95);
    }

    private static string DeterminePrimaryIndustry(
        PlanetProfile profile,
        ColonyType.Type type,
        SeededRng rng)
    {
        switch (type)
        {
            case ColonyType.Type.Corporate:
            case ColonyType.Type.Industrial:
            {
                Array<string> industries = new() { "mining", "manufacturing", "processing" };
                if (profile.Resources.ContainsKey((int)ResourceType.Type.RareElements))
                {
                    industries.Add("rare element extraction");
                }

                if (profile.Resources.ContainsKey((int)ResourceType.Type.Hydrocarbons))
                {
                    industries.Add("petrochemical");
                }

                return industries[rng.RandiRange(0, industries.Count - 1)];
            }
            case ColonyType.Type.Agricultural:
                return "agriculture";
            case ColonyType.Type.Military:
                return "defense";
            case ColonyType.Type.Scientific:
            {
                Array<string> focuses = new() { "research", "exploration", "xenobiology", "terraforming study" };
                return focuses[rng.RandiRange(0, focuses.Count - 1)];
            }
            case ColonyType.Type.Religious:
                return "spiritual community";
            default:
            {
                Array<string> general = new() { "mixed economy", "services", "trade" };
                return general[rng.RandiRange(0, general.Count - 1)];
            }
        }
    }

    private static double CalculateSelfSufficiency(
        PlanetProfile profile,
        ColonySuitability suitability,
        int years,
        SeededRng rng)
    {
        double baseValue = suitability.OverallScore / 200.0;
        double timeFactor = System.Math.Min(years / 300.0, 0.4);
        double resourceFactor = suitability.GetFactorScore(ColonySuitability.FactorType.Resources) / 200.0;
        double sufficiency = baseValue + timeFactor + resourceFactor;

        if (profile.HasBreathableAtmosphere)
        {
            sufficiency += 0.15;
        }

        if (profile.HasLiquidWater)
        {
            sufficiency += 0.1;
        }

        sufficiency *= rng.RandfRange(0.9f, 1.1f);
        return System.Math.Clamp(sufficiency, 0.1, 1.0);
    }

    private static void EstablishNativeRelations(
        Colony colony,
        Array<NativePopulation> natives,
        int currentYear,
        SeededRng rng)
    {
        int colonyAge = currentYear - colony.FoundingYear;
        foreach (NativePopulation native in natives)
        {
            if (!native.IsExtant)
            {
                if (native.ExtinctionYear > colony.FoundingYear)
                {
                    NativeRelation historical = NativeRelation.CreateFirstContact(
                        native.Id,
                        colony.FoundingYear + rng.RandiRange(1, 50),
                        rng.RandiRange(-30, 30));
                    historical.RecordExtinction(native.ExtinctionYear, native.ExtinctionCause);
                    colony.SetNativeRelation(historical);
                }

                continue;
            }

            int maxOffset = System.Math.Min(100, System.Math.Max(0, colonyAge));
            int contactYear = colony.FoundingYear + rng.RandiRange(0, maxOffset);
            int disposition = rng.RandiRange(-20, 20);
            if (ColonyType.TendsTowardNativeConflict(colony.Type))
            {
                disposition -= 30;
            }

            if (colony.Type == ColonyType.Type.Scientific)
            {
                disposition += 20;
            }

            NativeRelation relation = NativeRelation.CreateFirstContact(native.Id, contactYear, disposition);
            EvolveNativeRelation(relation, currentYear, rng);
            colony.SetNativeRelation(relation);
        }
    }

    private static void EvolveNativeRelation(
        NativeRelation relation,
        int currentYear,
        SeededRng rng)
    {
        int yearsOfContact = currentYear - relation.FirstContactYear;
        if (yearsOfContact <= 0)
        {
            return;
        }

        int eventsCount = (int)(yearsOfContact / 50.0);
        for (int index = 0; index < eventsCount; index += 1)
        {
            int eventType = rng.RandiRange(0, 10);
            switch (eventType)
            {
                case 0:
                case 1:
                {
                    double intensity = rng.RandfRange(0.2f, 0.8f);
                    int eventYear = relation.FirstContactYear + rng.RandiRange(10, yearsOfContact);
                    relation.RecordConflict(eventYear, "Territorial dispute", intensity);
                    relation.TerritoryTaken += rng.RandfRange(0.05f, 0.15f);
                    break;
                }
                case 2:
                case 3:
                    relation.TradeLevel = System.Math.Min(relation.TradeLevel + rng.RandfRange(0.1f, 0.3f), 1.0);
                    relation.RelationScore = System.Math.Min(relation.RelationScore + 10, 100);
                    break;
                case 4:
                    if (relation.RelationScore > -30)
                    {
                        int treatyYear = relation.FirstContactYear + rng.RandiRange(20, yearsOfContact);
                        relation.RecordTreaty(treatyYear, "Peace and trade agreement");
                    }

                    break;
                case 5:
                case 6:
                    relation.CulturalExchange = System.Math.Min(relation.CulturalExchange + rng.RandfRange(0.1f, 0.2f), 1.0);
                    relation.RelationScore = System.Math.Min(relation.RelationScore + 5, 100);
                    break;
                default:
                    relation.RelationScore = System.Math.Clamp(relation.RelationScore + rng.RandiRange(-10, 10), -100, 100);
                    break;
            }
        }

        relation.TerritoryTaken = System.Math.Clamp(relation.TerritoryTaken, 0.0, 0.9);
        relation.UpdateStatus();
    }

    private static void AddNativeEventsToHistory(Colony colony)
    {
        foreach (NativeRelation relation in colony.GetAllNativeRelations())
        {
            if (relation.FirstContactYear > 0)
            {
                colony.History.AddNewEvent(
                    HistoryEvent.EventType.Contact,
                    relation.FirstContactYear,
                    "First Contact",
                    "Contact established with native population.",
                    0.0);
            }

            if (relation.HasTreaty)
            {
                colony.History.AddNewEvent(
                    HistoryEvent.EventType.Treaty,
                    relation.TreatyYear,
                    "Native Treaty",
                    "Treaty signed with native population.",
                    0.3);
            }
        }
    }

    private static void MultiplyWeight(Dictionary weights, ColonyType.Type type, double factor)
    {
        int key = (int)type;
        weights[key] = (double)weights[key] * factor;
    }
}
