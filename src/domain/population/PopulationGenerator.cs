using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic population generator that mirrors the GDScript population flow.
/// </summary>
public static class PopulationGenerator
{
    private const int DefaultCurrentYear = 0;
    private const int DefaultMaxNativePopulations = 3;
    private const int DefaultNativeMinHistoryYears = 1000;
    private const int DefaultNativeMaxHistoryYears = 50000;
    private const int DefaultMaxAutoColonies = 2;
    private const double DefaultColonyChance = 0.3;
    private const int DefaultColonyMinHistoryYears = 50;
    private const int DefaultColonyMaxHistoryYears = 500;

    /// <summary>
    /// Builds profile-only population data.
    /// </summary>
    public static PlanetPopulationData BuildProfileOnlyData(
        CelestialBody body,
        ParentContext context,
        int generationSeed = 0,
        CelestialBody? parentBody = null)
    {
        PlanetProfile profile = ProfileGenerator.Generate(body, context, parentBody);
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        return new PlanetPopulationData
        {
            BodyId = body.Id,
            GenerationSeed = generationSeed,
            Profile = profile,
            Suitability = suitability,
        };
    }

    /// <summary>
    /// Generates full population data for a body.
    /// </summary>
    public static PlanetPopulationData Generate(
        CelestialBody body,
        ParentContext context,
        int generationSeed = 0,
        bool generateNatives = true,
        bool generateColonies = true,
        CelestialBody? parentBody = null,
        int currentYear = DefaultCurrentYear)
    {
        PlanetPopulationData data = BuildProfileOnlyData(body, context, generationSeed, parentBody);
        if (data.Profile == null || data.Suitability == null)
        {
            return data;
        }

        return GenerateFromProfile(
            data.Profile,
            generationSeed,
            generateNatives,
            generateColonies,
            currentYear,
            data.Suitability);
    }

    /// <summary>
    /// Generates full population data from an existing profile.
    /// </summary>
    public static PlanetPopulationData GenerateFromProfile(
        PlanetProfile profile,
        int generationSeed = 0,
        bool generateNatives = true,
        bool generateColonies = true,
        int currentYear = DefaultCurrentYear,
        ColonySuitability? existingSuitability = null)
    {
        ColonySuitability suitability = existingSuitability ?? SuitabilityCalculator.Calculate(profile);
        PlanetPopulationData data = new()
        {
            BodyId = profile.BodyId,
            GenerationSeed = generationSeed,
            Profile = profile,
            Suitability = suitability,
        };

        SeededRng rng = new(generationSeed);
        if (generateNatives)
        {
            data.NativePopulations = GenerateNatives(profile, currentYear, rng);
        }

        if (generateColonies)
        {
            data.Colonies = GenerateColonies(
                profile,
                suitability,
                data.NativePopulations,
                currentYear,
                rng);
        }

        return data;
    }

    /// <summary>
    /// Generates full serialized population payload data using the same top-level decision flow as GDScript.
    /// </summary>
    public static PlanetPopulationData? GenerateAuto(
        CelestialBody body,
        ParentContext context,
        int baseSeed,
        int populationOverride = 0,
        CelestialBody? parentBody = null)
    {
        if (populationOverride == (int)PopulationLikelihood.Override.None)
        {
            return null;
        }

        long populationSeed = PopulationSeeding.GeneratePopulationSeed(body.Id, baseSeed);
        PlanetPopulationData data = BuildProfileOnlyData(body, context, (int)populationSeed, parentBody);

        bool generateNatives;
        bool generateColony;
        if (populationOverride == (int)PopulationLikelihood.Override.ForceNatives)
        {
            generateNatives = true;
            generateColony = false;
        }
        else if (populationOverride == (int)PopulationLikelihood.Override.ForceColony)
        {
            generateNatives = false;
            generateColony = data.Suitability != null && data.Suitability.IsColonizable();
        }
        else
        {
            generateNatives = data.Profile != null && PopulationLikelihood.ShouldGenerateNatives(data.Profile, populationSeed);
            generateColony = data.Profile != null
                && data.Suitability != null
                && PopulationLikelihood.ShouldGenerateColony(data.Profile, data.Suitability, populationSeed);
        }

        if (data.Profile == null || data.Suitability == null)
        {
            return data;
        }

        PlanetPopulationData generated = GenerateFromProfile(
            data.Profile,
            (int)populationSeed,
            generateNatives,
            generateColony,
            DefaultCurrentYear,
            data.Suitability);
        return generated;
    }

    private static Array<NativePopulation> GenerateNatives(
        PlanetProfile profile,
        int currentYear,
        SeededRng rng)
    {
        SeededRng nativeRng = rng.Fork();
        return NativePopulationGenerator.Generate(
            profile,
            nativeRng,
            currentYear,
            DefaultMaxNativePopulations,
            false,
            DefaultNativeMinHistoryYears,
            DefaultNativeMaxHistoryYears);
    }

    private static Array<Colony> GenerateColonies(
        PlanetProfile profile,
        ColonySuitability suitability,
        Array<NativePopulation> existingNatives,
        int currentYear,
        SeededRng rng)
    {
        Array<Colony> colonies = new();
        if (!suitability.IsColonizable())
        {
            return colonies;
        }

        SeededRng colonyRng = rng.Fork();
        int colonyCount = DetermineAutoColonyCount(suitability, colonyRng);
        for (int index = 0; index < colonyCount; index += 1)
        {
            Colony? colony = ColonyGenerator.Generate(
                profile,
                suitability,
                existingNatives,
                colonyRng.Fork(),
                currentYear,
                DefaultColonyMinHistoryYears,
                DefaultColonyMaxHistoryYears,
                TechnologyLevel.Level.Interstellar,
                $"civ_auto_{index}",
                "Unknown Civilization");
            if (colony != null)
            {
                colonies.Add(colony);
            }
        }

        return colonies;
    }

    private static int DetermineAutoColonyCount(ColonySuitability suitability, SeededRng rng)
    {
        int count = 0;
        double adjustedChance = DefaultColonyChance * (suitability.OverallScore / 50.0);
        adjustedChance = System.Math.Clamp(adjustedChance, 0.0, 0.9);

        if (rng.Randf() < adjustedChance)
        {
            count = 1;
            double additionalChance = adjustedChance * 0.3;
            while (count < DefaultMaxAutoColonies && rng.Randf() < additionalChance)
            {
                count += 1;
                additionalChance *= 0.3;
            }
        }

        return count;
    }
}
