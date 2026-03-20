using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Pipeline;
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
        PlanetPopulationData data = new PlanetPopulationData
        {
            BodyId = body.Id,
            GenerationSeed = generationSeed,
            Profile = profile,
            Suitability = suitability,
        };

        PlanetEnvironmentProfile environmentProfile = PlanetEnvironmentProfile.FromPlanetProfile(
            profile,
            generationSeed,
            body.Name,
            body.GetTypeString());
        ConceptDependencyChainGenerator.PopulatePreSocietyStates(
            environmentProfile,
            out EcologyState ecologyState,
            out SpeciesEvolutionState speciesEvolutionState,
            out SentienceAssessment sentienceAssessment);
        data.EnvironmentProfile = environmentProfile;
        data.EcologyState = ecologyState;
        data.SpeciesEvolution = speciesEvolutionState;
        data.SentienceAssessment = sentienceAssessment;
        return data;
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
        int currentYear = DefaultCurrentYear,
        GenerationUseCaseSettings? useCaseSettings = null)
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
            data.Suitability,
            useCaseSettings);
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
        ColonySuitability? existingSuitability = null,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        ColonySuitability suitability = existingSuitability ?? SuitabilityCalculator.Calculate(profile);
        PlanetPopulationData data = new()
        {
            BodyId = profile.BodyId,
            GenerationSeed = generationSeed,
            Profile = profile,
            Suitability = suitability,
        };

        PlanetEnvironmentProfile environmentProfile = PlanetEnvironmentProfile.FromPlanetProfile(
            profile,
            generationSeed,
            profile.BodyId,
            "Planet");
        ConceptDependencyChainGenerator.PopulatePreSocietyStates(
            environmentProfile,
            out EcologyState ecologyState,
            out SpeciesEvolutionState speciesEvolutionState,
            out SentienceAssessment sentienceAssessment);
        data.EnvironmentProfile = environmentProfile;
        data.EcologyState = ecologyState;
        data.SpeciesEvolution = speciesEvolutionState;
        data.SentienceAssessment = sentienceAssessment;

        if (!generateNatives)
        {
            MarkNativeLifeAbsent(data);
        }

        SeededRng rng = new(generationSeed);
        bool allowNativeSentients = generateNatives
            && data.SentienceAssessment != null
            && data.SentienceAssessment.Status == ConceptRunStatus.Generated
            && data.SentienceAssessment.HasSentientLife;
        if (allowNativeSentients)
        {
            data.NativePopulations = GenerateNatives(profile, currentYear, rng, true);
        }

        if (generateColonies)
        {
            data.Colonies = GenerateColonies(
                profile,
                suitability,
                data.NativePopulations,
                currentYear,
                rng,
                useCaseSettings);
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
        CelestialBody? parentBody = null,
        GenerationUseCaseSettings? useCaseSettings = null)
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
            generateNatives = data.Profile != null
                && PopulationLikelihood.ShouldGenerateNatives(data.Profile, populationSeed, useCaseSettings);
            generateColony = data.Profile != null
                && data.Suitability != null
                && PopulationLikelihood.ShouldGenerateColony(data.Profile, data.Suitability, populationSeed, useCaseSettings);
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
            data.Suitability,
            useCaseSettings);
        generated.EnvironmentProfile = data.EnvironmentProfile;
        return generated;
    }

    private static Array<NativePopulation> GenerateNatives(
        PlanetProfile profile,
        int currentYear,
        SeededRng rng,
        bool forcePopulation)
    {
        SeededRng nativeRng = rng.Fork();
        return NativePopulationGenerator.Generate(
            profile,
            nativeRng,
            currentYear,
            DefaultMaxNativePopulations,
            forcePopulation,
            DefaultNativeMinHistoryYears,
            DefaultNativeMaxHistoryYears);
    }

    private static Array<Colony> GenerateColonies(
        PlanetProfile profile,
        ColonySuitability suitability,
        Array<NativePopulation> existingNatives,
        int currentYear,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings)
    {
        Array<Colony> colonies = new();
        if (!suitability.IsColonizable())
        {
            return colonies;
        }

        SeededRng colonyRng = rng.Fork();
        int colonyCount = DetermineAutoColonyCount(profile, suitability, colonyRng, useCaseSettings);
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

    private static int DetermineAutoColonyCount(
        PlanetProfile profile,
        ColonySuitability suitability,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings)
    {
        double permissiveness = GenerationUseCaseSettings.NeutralPermissiveness;
        if (useCaseSettings != null)
        {
            permissiveness = useCaseSettings.PopulationPermissiveness;
        }

        int count = 0;
        double adjustedChance = PopulationProbability.CalculateColonyProbability(profile, suitability, permissiveness);

        if (rng.Randf() < adjustedChance)
        {
            count = 1;
            int maxColonies = 1 + (int)System.Math.Round(3.0 * permissiveness);
            double additionalChance = adjustedChance * Lerp(0.12, 0.40, permissiveness);
            while (count < maxColonies && rng.Randf() < additionalChance)
            {
                count += 1;
                additionalChance *= Lerp(0.18, 0.45, permissiveness);
            }
        }

        return count;
    }

    private static double Lerp(double minValue, double maxValue, double factor)
    {
        return minValue + ((maxValue - minValue) * factor);
    }

    private static void MarkNativeLifeAbsent(PlanetPopulationData data)
    {
        string reason = "Native life did not emerge for this world under the current generation assumptions.";

        EcologyState ecologyState = new EcologyState();
        ecologyState.Status = ConceptRunStatus.NotApplicable;
        ecologyState.StatusReason = reason;
        if (data.EcologyState != null)
        {
            ecologyState.Provenance = data.EcologyState.Provenance;
        }
        data.EcologyState = ecologyState;

        SpeciesEvolutionState speciesEvolutionState = new SpeciesEvolutionState();
        speciesEvolutionState.Status = ConceptRunStatus.NotApplicable;
        speciesEvolutionState.StatusReason = reason;
        if (data.SpeciesEvolution != null)
        {
            speciesEvolutionState.Provenance = data.SpeciesEvolution.Provenance;
        }
        data.SpeciesEvolution = speciesEvolutionState;

        SentienceAssessment sentienceAssessment = new SentienceAssessment();
        sentienceAssessment.Status = ConceptRunStatus.NotApplicable;
        sentienceAssessment.StatusReason = reason;
        if (data.SentienceAssessment != null)
        {
            sentienceAssessment.Provenance = data.SentienceAssessment.Provenance;
        }
        data.SentienceAssessment = sentienceAssessment;
    }
}
