using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StarGen.Domain.Concepts.Ecology;
using StarGen.Domain.Concepts.Evolution;
using StarGen.Domain.Ecology;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Generates deterministic upstream concept states from a planet environment profile.
/// </summary>
public static class ConceptDependencyChainGenerator
{
    private const string EcologyGeneratorVersion = "pipeline-ecology-v2";
    private const string EvolutionGeneratorVersion = "pipeline-evolution-v2";
    private const string SentienceGeneratorVersion = "pipeline-sentience-v2";

    /// <summary>
    /// Builds the full pre-society chain from a planet profile.
    /// </summary>
    public static void PopulatePreSocietyStates(
        PlanetEnvironmentProfile environment,
        out EcologyState ecologyState,
        out SpeciesEvolutionState speciesEvolutionState,
        out SentienceAssessment sentienceAssessment)
    {
        ecologyState = GenerateEcology(environment);
        speciesEvolutionState = GenerateSpecies(environment, ecologyState);
        sentienceAssessment = GenerateSentience(environment, speciesEvolutionState);
    }

    private static EcologyState GenerateEcology(PlanetEnvironmentProfile environment)
    {
        EcologyState state = new EcologyState();
        state.Provenance = BuildProvenance(
            ConceptKind.Ecology,
            environment.Seed,
            EcologyGeneratorVersion,
            environment.BodyName,
            new List<string> { "environment:" + environment.BodyId });

        if (!environment.SupportsBiology())
        {
            state.Status = ConceptRunStatus.NotApplicable;
            state.StatusReason = "Environment does not support a stable biosphere.";
            return state;
        }

        EnvironmentSpec spec = BuildEcologySpec(environment);
        EcologyWeb web = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
        EcologyConceptSnapshot snapshot = new EcologyConceptSnapshot();
        snapshot.SlotCount = web.Slots.Count;
        snapshot.ConnectionCount = web.Connections.Count;
        snapshot.Productivity = web.TotalProductivity;
        snapshot.Biomass = web.GetTotalBiomass();
        snapshot.Complexity = web.ComplexityScore;
        snapshot.Stability = web.StabilityScore;
        snapshot.MaxChainLength = web.GetMaxChainLength();

        foreach (TrophicLevel level in Enum.GetValues(typeof(TrophicLevel)))
        {
            snapshot.LevelCounts[FormatForDisplay(level.ToString())] = web.GetSlotsByLevel(level).Count;
        }

        snapshot.HighlightedNiches = web.Slots
            .OrderByDescending(slot => slot.BiomassCapacity)
            .Take(6)
            .Select(slot => FormatForDisplay(slot.Level.ToString()) + ": " + slot.Description + " (" + slot.BiomassCapacity.ToString("0.0", CultureInfo.InvariantCulture) + " biomass)")
            .ToList();

        state.Status = ConceptRunStatus.Generated;
        state.Snapshot = snapshot;
        return state;
    }

    private static SpeciesEvolutionState GenerateSpecies(PlanetEnvironmentProfile environment, EcologyState ecologyState)
    {
        SpeciesEvolutionState state = new SpeciesEvolutionState();
        state.Provenance = BuildProvenance(
            ConceptKind.Evolution,
            environment.Seed,
            EvolutionGeneratorVersion,
            environment.BodyName,
            new List<string> { "environment:" + environment.BodyId, "ecology:" + environment.BodyId });

        if (ecologyState.Status != ConceptRunStatus.Generated)
        {
            state.Status = ConceptRunStatus.NotApplicable;
            state.StatusReason = "Species evolution requires an ecological baseline.";
            return state;
        }

        ConceptContextSnapshot snapshotContext = BuildSnapshotAdapter(environment);
        EvolutionConceptSnapshot snapshot = EvolutionConceptGenerator.Generate(snapshotContext);
        double socialComplexity = ResolveSocialComplexity(snapshot);
        double communicationScore = ResolveCommunicationScore(snapshot);
        double manipulationScore = ResolveManipulationScore(snapshot);
        state.Status = ConceptRunStatus.Generated;
        state.Snapshot = snapshot;
        state.HasSentientCandidate = snapshot.UnlockedNodes.Contains("brain")
            && snapshot.CognitionScore >= 0.48
            && socialComplexity >= 0.38
            && communicationScore >= 0.30
            && manipulationScore >= 0.30;
        return state;
    }

    private static SentienceAssessment GenerateSentience(
        PlanetEnvironmentProfile environment,
        SpeciesEvolutionState speciesEvolutionState)
    {
        SentienceAssessment assessment = new SentienceAssessment();
        assessment.Provenance = BuildProvenance(
            ConceptKind.Civilization,
            environment.Seed,
            SentienceGeneratorVersion,
            environment.BodyName,
            new List<string> { "environment:" + environment.BodyId, "evolution:" + environment.BodyId });

        if (speciesEvolutionState.Status != ConceptRunStatus.Generated)
        {
            assessment.Status = ConceptRunStatus.NotApplicable;
            assessment.StatusReason = "Sentience requires a species lineage.";
            return assessment;
        }

        EvolutionConceptSnapshot species = speciesEvolutionState.Snapshot;
        assessment.CandidateSpeciesName = species.SpeciesName;
        assessment.CognitionScore = species.CognitionScore;
        assessment.SocialComplexity = ResolveSocialComplexity(species);
        assessment.CommunicationScore = ResolveCommunicationScore(species);
        assessment.ManipulationScore = ResolveManipulationScore(species);
        bool supportsSentientCivilization = environment.HabitabilityScore >= 5;
        if (supportsSentientCivilization)
        {
            assessment.HasSentientLife = speciesEvolutionState.HasSentientCandidate
                && assessment.CognitionScore >= 0.48
                && assessment.SocialComplexity >= 0.40
                && assessment.CommunicationScore >= 0.32
                && assessment.ManipulationScore >= 0.32;
        }
        else
        {
            assessment.HasSentientLife = false;
        }

        assessment.Status = ConceptRunStatus.Generated;
        if (assessment.HasSentientLife)
        {
            assessment.StatusReason = "Cognition, communication, and manipulation thresholds support sentient populations.";
        }
        else if (!supportsSentientCivilization)
        {
            assessment.StatusReason = "Complex life may exist, but the current environment is too marginal to support a sentient civilization.";
        }
        else
        {
            assessment.StatusReason = "The biosphere supports complex life, but not a sentient lineage at the current seed and pressures.";
        }

        return assessment;
    }

    private static EnvironmentSpec BuildEcologySpec(PlanetEnvironmentProfile environment)
    {
        float averageTemperature = (float)environment.AvgTemperatureK;
        float temperatureRange = 8.0f + ((10 - environment.HabitabilityScore) * 1.5f);
        float temperatureMin = averageTemperature - temperatureRange;
        float temperatureMax = averageTemperature + temperatureRange;
        if (temperatureMin < 1.0f)
        {
            temperatureMin = 1.0f;
        }

        if (temperatureMax < temperatureMin + 1.0f)
        {
            temperatureMax = temperatureMin + 1.0f;
        }

        EnvironmentSpec spec = new EnvironmentSpec();
        spec.Seed = unchecked((ulong)environment.Seed);
        spec.TemperatureMin = temperatureMin;
        spec.TemperatureMax = temperatureMax;
        spec.WaterAvailability = System.Math.Clamp((float)environment.OceanCoverage, 0.0f, 1.0f);
        spec.LightLevel = System.Math.Clamp(0.55f + (environment.HabitabilityScore / 20.0f), 0.0f, 1.0f);
        spec.NutrientLevel = System.Math.Clamp(0.20f + (float)(environment.TectonicActivity * 0.22) + (float)(environment.VolcanismLevel * 0.12), 0.0f, 1.0f);
        spec.Gravity = (float)System.Math.Clamp(environment.GravityG, 0.2, 3.0);
        spec.RadiationLevel = System.Math.Clamp((float)environment.RadiationLevel, 0.0f, 1.0f);
        if (environment.HasBreathableAtmosphere)
        {
            spec.OxygenLevel = 0.21f;
        }
        else
        {
            spec.OxygenLevel = 0.03f;
        }
        spec.SeasonalVariation = System.Math.Clamp((float)(0.12 + (environment.WeatherSeverity * 0.30)), 0.0f, 1.0f);
        spec.Biome = ResolveEcologyBiome(environment);
        spec.GeneratorVersion = EcologyGeneratorVersion;
        return spec;
    }

    private static ConceptContextSnapshot BuildSnapshotAdapter(PlanetEnvironmentProfile environment)
    {
        ConceptContextSnapshot snapshot = new ConceptContextSnapshot();
        snapshot.Seed = environment.Seed;
        snapshot.BodyId = environment.BodyId;
        snapshot.BodyName = environment.BodyName;
        snapshot.BodyType = environment.BodyType;
        snapshot.HabitabilityScore = environment.HabitabilityScore;
        snapshot.AvgTemperatureK = environment.AvgTemperatureK;
        snapshot.WaterAvailability = environment.OceanCoverage;
        if (environment.HasBreathableAtmosphere)
        {
            snapshot.OxygenLevel = 0.21;
        }
        else
        {
            snapshot.OxygenLevel = 0.03;
        }
        snapshot.GravityG = environment.GravityG;
        snapshot.RadiationLevel = environment.RadiationLevel;
        snapshot.DominantBiome = environment.DominantBiome;
        snapshot.SourceLabel = environment.BodyName;
        snapshot.EnvironmentProfile = PlanetEnvironmentProfile.FromDictionary(environment.ToDictionary());
        return snapshot;
    }

    private static double ResolveSocialComplexity(EvolutionConceptSnapshot snapshot)
    {
        double score = 0.18;
        if (snapshot.UnlockedNodes.Contains("social_living"))
        {
            score += 0.22;
        }

        if (snapshot.UnlockedNodes.Contains("culture"))
        {
            score += 0.22;
        }

        if (snapshot.UnlockedNodes.Contains("cooperative_hunt"))
        {
            score += 0.12;
        }

        score += snapshot.CognitionScore * 0.22;
        return System.Math.Clamp(score, 0.0, 1.0);
    }

    private static double ResolveCommunicationScore(EvolutionConceptSnapshot snapshot)
    {
        double score = 0.14;
        if (snapshot.UnlockedNodes.Contains("comm_complex"))
        {
            score += 0.26;
        }

        if (snapshot.UnlockedNodes.Contains("language"))
        {
            score += 0.26;
        }

        if (snapshot.UnlockedNodes.Contains("color_vision") || snapshot.UnlockedNodes.Contains("electroreception") || snapshot.UnlockedNodes.Contains("echolocation"))
        {
            score += 0.12;
        }

        score += snapshot.CognitionScore * 0.18;
        return System.Math.Clamp(score, 0.0, 1.0);
    }

    private static double ResolveManipulationScore(EvolutionConceptSnapshot snapshot)
    {
        double score = 0.12;
        if (snapshot.UnlockedNodes.Contains("limbs"))
        {
            score += 0.22;
        }

        if (snapshot.UnlockedNodes.Contains("tool_use"))
        {
            score += 0.28;
        }

        if (snapshot.UnlockedNodes.Contains("climbing") || snapshot.UnlockedNodes.Contains("walking"))
        {
            score += 0.10;
        }

        score += snapshot.CognitionScore * 0.12;
        return System.Math.Clamp(score, 0.0, 1.0);
    }

    private static StarGen.Domain.Ecology.BiomeType ResolveEcologyBiome(PlanetEnvironmentProfile environment)
    {
        string normalized = environment.DominantBiome.Trim().ToLowerInvariant();
        if (normalized == "barren")
        {
            if (environment.HasLiquidWater)
            {
                return StarGen.Domain.Ecology.BiomeType.Subterranean;
            }

            throw new InvalidOperationException("Ecology cannot be mapped from barren biome context.");
        }

        return MapEcologyBiome(environment.DominantBiome);
    }

    private static StarGen.Domain.Ecology.BiomeType MapEcologyBiome(string biomeName)
    {
        string normalized = biomeName.Trim().ToLowerInvariant();
        if (normalized == "temperate")
        {
            return StarGen.Domain.Ecology.BiomeType.Grassland;
        }

        if (normalized == "oceanic")
        {
            return StarGen.Domain.Ecology.BiomeType.Aquatic;
        }

        if (normalized == "ocean")
        {
            return StarGen.Domain.Ecology.BiomeType.Aquatic;
        }

        if (normalized == "desert")
        {
            return StarGen.Domain.Ecology.BiomeType.Desert;
        }

        if (normalized == "forest")
        {
            return StarGen.Domain.Ecology.BiomeType.Forest;
        }

        if (normalized == "taiga")
        {
            return StarGen.Domain.Ecology.BiomeType.Forest;
        }

        if (normalized == "jungle")
        {
            return StarGen.Domain.Ecology.BiomeType.Forest;
        }

        if (normalized == "grassland")
        {
            return StarGen.Domain.Ecology.BiomeType.Grassland;
        }

        if (normalized == "savanna")
        {
            return StarGen.Domain.Ecology.BiomeType.Grassland;
        }

        if (normalized == "mountain")
        {
            return StarGen.Domain.Ecology.BiomeType.Grassland;
        }

        if (normalized == "tundra")
        {
            return StarGen.Domain.Ecology.BiomeType.Tundra;
        }

        if (normalized == "ice sheet")
        {
            return StarGen.Domain.Ecology.BiomeType.Tundra;
        }

        if (normalized == "volcanic")
        {
            return StarGen.Domain.Ecology.BiomeType.Volcanic;
        }

        if (normalized == "subterranean")
        {
            return StarGen.Domain.Ecology.BiomeType.Subterranean;
        }

        if (normalized == "subsurface")
        {
            return StarGen.Domain.Ecology.BiomeType.Subterranean;
        }

        if (normalized == "wetland")
        {
            return StarGen.Domain.Ecology.BiomeType.Wetland;
        }

        if (normalized == "reef")
        {
            return StarGen.Domain.Ecology.BiomeType.Reef;
        }

        throw new InvalidOperationException("Unsupported ecology biome '" + biomeName + "'.");
    }

    private static ConceptProvenance BuildProvenance(
        ConceptKind kind,
        int seed,
        string generatorVersion,
        string sourceContext,
        List<string> upstreamDependencies)
    {
        ConceptProvenance provenance = new ConceptProvenance();
        provenance.ConceptId = kind.ToString();
        provenance.Seed = seed;
        provenance.GeneratorVersion = generatorVersion;
        provenance.SourceContext = sourceContext;
        provenance.InputSignature = seed.ToString(CultureInfo.InvariantCulture);
        provenance.UpstreamDependencies = upstreamDependencies;
        return provenance;
    }

    private static string FormatForDisplay(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.Trim().Replace('_', ' ').Replace('-', ' ');
        if (normalized.Length == 1)
        {
            return normalized.ToUpperInvariant();
        }

        return char.ToUpperInvariant(normalized[0]) + normalized.Substring(1).ToLowerInvariant();
    }
}
