using Godot;
using Godot.Collections;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic native-population generation.
/// </summary>
public static class NativePopulationGenerator
{
    /// <summary>
    /// Minimum habitability score for sentient life.
    /// </summary>
    public const int MinHabitabilityForSentience = 4;

    /// <summary>
    /// Base chance for sentient life to emerge.
    /// </summary>
    public const double BaseSentienceChance = 0.3;

    /// <summary>
    /// Generates native populations from a profile.
    /// </summary>
    public static Array<NativePopulation> Generate(
        PlanetProfile profile,
        SeededRng rng,
        int currentYear = 0,
        int maxPopulations = 3,
        bool forcePopulation = false,
        int minHistoryYears = 1000,
        int maxHistoryYears = 100000,
        double lifeChanceModifier = 1.0)
    {
        Array<NativePopulation> populations = new();
        if (!profile.CanSupportNativeLife())
        {
            return populations;
        }

        int count = DeterminePopulationCount(profile, rng, maxPopulations, forcePopulation, lifeChanceModifier);
        if (count == 0)
        {
            return populations;
        }

        for (int index = 0; index < count; index += 1)
        {
            NativePopulation population = GenerateSinglePopulation(
                profile,
                rng,
                currentYear,
                minHistoryYears,
                maxHistoryYears,
                index,
                count);
            populations.Add(population);
        }

        return populations;
    }

    private static int DeterminePopulationCount(
        PlanetProfile profile,
        SeededRng rng,
        int maxPopulations,
        bool forcePopulation,
        double lifeChanceModifier)
    {
        double lifeChance = CalculateLifeChance(profile) * lifeChanceModifier;
        if (lifeChance <= 0.0)
        {
            return 0;
        }

        if (forcePopulation)
        {
            int forcedCount = 1;
            double extraChance = CalculateMultiPopulationChance(profile);
            while (forcedCount < maxPopulations && rng.Randf() < extraChance)
            {
                forcedCount += 1;
                extraChance *= 0.5;
            }

            return forcedCount;
        }

        if (rng.Randf() > lifeChance)
        {
            return 0;
        }

        int count = 1;
        double multiChance = CalculateMultiPopulationChance(profile);
        while (count < maxPopulations && rng.Randf() < multiChance)
        {
            count += 1;
            multiChance *= 0.5;
        }

        return count;
    }

    private static double CalculateMultiPopulationChance(PlanetProfile profile)
    {
        double chance = 0.0;
        if (profile.ContinentCount >= 3)
        {
            chance += 0.2;
        }

        if (profile.ContinentCount >= 5)
        {
            chance += 0.1;
        }

        if (profile.HabitabilityScore >= 7)
        {
            chance += 0.2;
        }

        if (profile.Biomes.Count >= 4)
        {
            chance += 0.1;
        }

        return chance;
    }

    private static double CalculateLifeChance(PlanetProfile profile)
    {
        if (profile.HabitabilityScore < MinHabitabilityForSentience)
        {
            return 0.0;
        }

        double chance = BaseSentienceChance;
        chance *= (profile.HabitabilityScore - MinHabitabilityForSentience + 1) / 7.0;

        if (profile.HasLiquidWater)
        {
            chance *= 1.5;
        }

        if (profile.Biomes.Count >= 4)
        {
            chance *= 1.2;
        }

        if (profile.VolcanismLevel < 0.3 && profile.TectonicActivity < 0.5)
        {
            chance *= 1.2;
        }

        return System.Math.Clamp(chance, 0.0, 0.95);
    }

    private static NativePopulation GenerateSinglePopulation(
        PlanetProfile profile,
        SeededRng rng,
        int currentYear,
        int minHistoryYears,
        int maxHistoryYears,
        int index,
        int total)
    {
        NativePopulation population = new();
        population.Id = $"native_{profile.BodyId}_{index}";
        population.Name = GeneratePopulationName(rng);
        population.BodyId = profile.BodyId;

        int historyYears = rng.RandiRange(minHistoryYears, maxHistoryYears);
        population.OriginYear = currentYear - historyYears;
        population.TechLevel = DetermineTechLevel(historyYears, profile, rng);
        population.Government = GenerateGovernment(population.TechLevel, rng);
        population.Government.RegimeEstablishedYear = currentYear - rng.RandiRange(10, 500);
        population.PrimaryBiome = SelectPrimaryBiome(profile, rng);

        if (total == 1)
        {
            population.TerritorialControl = rng.RandfRange(0.3f, 0.9f);
        }
        else
        {
            double maxControl = 0.8 / total;
            population.TerritorialControl = rng.RandfRange(0.1f, (float)maxControl);
        }

        population.Population = CalculatePopulation(profile, population, rng);
        population.PeakPopulation = (int)System.Math.Round(population.Population * rng.RandfRange(1.0f, 1.5f));
        population.PeakPopulationYear = currentYear - rng.RandiRange(0, (int)(historyYears / 4.0));
        population.CulturalTraits = GenerateCulturalTraits(profile, population.TechLevel, rng);
        population.History = HistoryGenerator.GenerateHistory(
            profile,
            population.OriginYear,
            currentYear,
            rng,
            population.Name + " Emergence");

        if (rng.Randf() < 0.1 && historyYears > 10000)
        {
            int extinctionYear = currentYear - rng.RandiRange(100, (int)(historyYears / 2.0));
            Array<string> causes = new() { "climate change", "asteroid impact", "plague", "war", "unknown" };
            population.RecordExtinction(extinctionYear, causes[rng.RandiRange(0, causes.Count - 1)]);
        }

        return population;
    }

    private static string GeneratePopulationName(SeededRng rng)
    {
        Array<string> prefixes = new()
        {
            "Ak", "El", "Vor", "Zan", "Kir", "Tor", "Mar", "Sol", "Vel", "Nor",
            "Ar", "Eth", "Om", "Ur", "Ix", "Yl", "Qu", "Thal", "Krath", "Ven",
        };
        Array<string> suffixes = new()
        {
            "ani", "ari", "oni", "eni", "uri", "ian", "ean", "aan", "iin", "oon",
            "ax", "ex", "ix", "ox", "ux", "al", "el", "il", "ol", "ul",
        };

        string prefix = prefixes[rng.RandiRange(0, prefixes.Count - 1)];
        string suffix = suffixes[rng.RandiRange(0, suffixes.Count - 1)];
        return prefix + suffix;
    }

    private static TechnologyLevel.Level DetermineTechLevel(int ageYears, PlanetProfile profile, SeededRng rng)
    {
        TechnologyLevel.Level expected = TechnologyLevel.Level.StoneAge;
        for (int index = 0; index < TechnologyLevel.Count(); index += 1)
        {
            TechnologyLevel.Level level = (TechnologyLevel.Level)index;
            if (ageYears >= TechnologyLevel.TypicalYearsToReach(level))
            {
                expected = level;
            }
        }

        int modifier = 0;
        if (profile.HabitabilityScore < 6)
        {
            modifier -= 1;
        }

        if (profile.Resources.Count >= 5)
        {
            modifier += 1;
        }

        modifier += rng.RandiRange(-1, 1);
        int finalLevel = System.Math.Clamp((int)expected + modifier, 0, TechnologyLevel.Count() - 1);
        return (TechnologyLevel.Level)finalLevel;
    }

    private static Government GenerateGovernment(TechnologyLevel.Level techLevel, SeededRng rng)
    {
        Government government = new();
        switch (techLevel)
        {
            case TechnologyLevel.Level.StoneAge:
                government.Regime = GovernmentType.Regime.Tribal;
                break;
            case TechnologyLevel.Level.BronzeAge:
            case TechnologyLevel.Level.IronAge:
            {
                Array<GovernmentType.Regime> options = new()
                {
                    GovernmentType.Regime.Chiefdom,
                    GovernmentType.Regime.CityState,
                    GovernmentType.Regime.PatrimonialKingdom,
                };
                government.Regime = options[rng.RandiRange(0, options.Count - 1)];
                break;
            }
            case TechnologyLevel.Level.Classical:
            case TechnologyLevel.Level.Medieval:
            {
                Array<GovernmentType.Regime> options = new()
                {
                    GovernmentType.Regime.Feudal,
                    GovernmentType.Regime.PatrimonialKingdom,
                    GovernmentType.Regime.BureaucraticEmpire,
                };
                government.Regime = options[rng.RandiRange(0, options.Count - 1)];
                break;
            }
            case TechnologyLevel.Level.Renaissance:
            case TechnologyLevel.Level.Industrial:
            {
                Array<GovernmentType.Regime> options = new()
                {
                    GovernmentType.Regime.AbsoluteMonarchy,
                    GovernmentType.Regime.Constitutional,
                    GovernmentType.Regime.EliteRepublic,
                };
                government.Regime = options[rng.RandiRange(0, options.Count - 1)];
                break;
            }
            default:
            {
                Array<GovernmentType.Regime> options = new()
                {
                    GovernmentType.Regime.MassDemocracy,
                    GovernmentType.Regime.Constitutional,
                    GovernmentType.Regime.OnePartyState,
                    GovernmentType.Regime.EliteRepublic,
                };
                government.Regime = options[rng.RandiRange(0, options.Count - 1)];
                break;
            }
        }

        double techFactor = (int)techLevel / (double)TechnologyLevel.Count();
        government.CoercionCentralization = System.Math.Clamp(rng.RandfRange(0.1f, 0.4f) + (techFactor * 0.5), 0.0, 1.0);
        government.AdministrativeCapacity = System.Math.Clamp(rng.RandfRange(0.1f, 0.3f) + (techFactor * 0.6), 0.0, 1.0);

        if (GovernmentType.IsParticipatory(government.Regime))
        {
            government.PoliticalInclusiveness = rng.RandfRange(0.3f, 0.8f);
        }
        else
        {
            government.PoliticalInclusiveness = rng.RandfRange(0.05f, 0.3f);
        }

        government.Legitimacy = rng.RandfRange(0.4f, 0.9f);
        return government;
    }

    private static string SelectPrimaryBiome(PlanetProfile profile, SeededRng rng)
    {
        Array<int> candidates = new();
        Array<float> weights = new();

        foreach (Variant biomeKey in profile.Biomes.Keys)
        {
            BiomeType.Type biome = (BiomeType.Type)(int)biomeKey;
            if (BiomeType.CanSupportLife(biome))
            {
                candidates.Add((int)biomeKey);
                weights.Add((float)(double)profile.Biomes[biomeKey]);
            }
        }

        if (candidates.Count == 0)
        {
            return "Unknown";
        }

        int? result = rng.WeightedChoice(candidates, weights);
        int selected = result ?? candidates[candidates.Count - 1];
        return BiomeType.ToStringName((BiomeType.Type)selected);
    }

    private static int CalculatePopulation(PlanetProfile profile, NativePopulation population, SeededRng rng)
    {
        double baseDensity = population.TechLevel switch
        {
            TechnologyLevel.Level.StoneAge => 0.1,
            TechnologyLevel.Level.BronzeAge => 1.0,
            TechnologyLevel.Level.IronAge => 1.0,
            TechnologyLevel.Level.Classical => 5.0,
            TechnologyLevel.Level.Medieval => 5.0,
            TechnologyLevel.Level.Renaissance => 15.0,
            TechnologyLevel.Level.Industrial => 50.0,
            _ => 200.0,
        };

        baseDensity *= profile.HabitabilityScore / 10.0;
        double habitableSurface = profile.GetHabitableSurface();
        double surfaceAreaKm2 = 510.1e6 * System.Math.Max(profile.GravityG, 0.5);
        double controlledArea = surfaceAreaKm2 * habitableSurface * population.TerritorialControl;
        double count = controlledArea * baseDensity * rng.RandfRange(0.5f, 1.5f);
        return System.Math.Max(100, (int)System.Math.Round(count));
    }

    private static Array<string> GenerateCulturalTraits(
        PlanetProfile profile,
        TechnologyLevel.Level techLevel,
        SeededRng rng)
    {
        Array<string> traits = new();

        MaybeAddTrait(traits, "seafaring", profile.OceanCoverage > 0.5, 0.7, rng);
        MaybeAddTrait(traits, "cold-adapted", profile.IceCoverage > 0.2, 0.6, rng);
        MaybeAddTrait(traits, "volcanic-reverent", profile.VolcanismLevel > 0.5, 0.5, rng);
        MaybeAddTrait(traits, "weather-wise", profile.WeatherSeverity > 0.6, 0.5, rng);
        MaybeAddTrait(traits, "industrious", techLevel >= TechnologyLevel.Level.Industrial, 0.6, rng);
        MaybeAddTrait(traits, "expansionist", techLevel >= TechnologyLevel.Level.Spacefaring, 0.7, rng);

        Array<string> randomTraits = new()
        {
            "warlike", "peaceful", "mercantile", "artistic", "spiritual",
            "pragmatic", "traditional", "innovative", "isolationist", "diplomatic",
        };

        int count = rng.RandiRange(1, 3);
        for (int index = 0; index < count; index += 1)
        {
            string trait = randomTraits[rng.RandiRange(0, randomTraits.Count - 1)];
            if (!traits.Contains(trait))
            {
                traits.Add(trait);
            }
        }

        return traits;
    }

    private static void MaybeAddTrait(
        Array<string> traits,
        string trait,
        bool condition,
        double chance,
        SeededRng rng)
    {
        if (condition && rng.Randf() < chance)
        {
            traits.Add(trait);
        }
    }
}
