using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Generation;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

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
        CelestialBody? parentBody = null,
        GenerationUseCaseSettings? useCaseSettings = null)
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
            out SentienceAssessment sentienceAssessment,
            useCaseSettings);
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
        bool generateColonies = false,
        CelestialBody? parentBody = null,
        int currentYear = DefaultCurrentYear,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        PlanetPopulationData data = BuildProfileOnlyData(body, context, generationSeed, parentBody, useCaseSettings);
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
        bool generateColonies = false,
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
            out SentienceAssessment sentienceAssessment,
            useCaseSettings);
        data.EnvironmentProfile = environmentProfile;
        data.EcologyState = ecologyState;
        data.SpeciesEvolution = speciesEvolutionState;
        data.SentienceAssessment = sentienceAssessment;

        if (!generateNatives)
        {
            MarkNativeLifeAbsent(data);
        }

        SeededRng rng = new(generationSeed);
        bool allowNativePopulations = generateNatives
            && data.EcologyState != null
            && data.EcologyState.Status == ConceptRunStatus.Generated;
        if (allowNativePopulations)
        {
            data.NativePopulations = GenerateNatives(profile, currentYear, rng, true);
            HarmonizeSentienceAssessmentWithNativePopulations(data);
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

        int populationSeed = unchecked((int)PopulationSeeding.GeneratePopulationSeed(body.Id, baseSeed));
        PlanetPopulationData data = BuildProfileOnlyData(body, context, populationSeed, parentBody, useCaseSettings);

        bool generateNatives;
        if (populationOverride == (int)PopulationLikelihood.Override.ForceNatives)
        {
            generateNatives = true;
        }
        else if (populationOverride == (int)PopulationLikelihood.Override.ForceColony)
        {
            generateNatives = false;
        }
        else
        {
            generateNatives = data.Profile != null
                && PopulationLikelihood.ShouldGenerateNatives(data.Profile, populationSeed, useCaseSettings);
        }

        if (data.Profile == null || data.Suitability == null)
        {
            return data;
        }

        PlanetPopulationData generated = GenerateFromProfile(
            data.Profile,
            populationSeed,
            generateNatives,
            false,
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
        GenerationUseCaseSettings? useCaseSettings,
        ColonyPressureContext? pressureContext = null)
    {
        Array<Colony> colonies = new();
        if (!suitability.IsColonizable())
        {
            return colonies;
        }

        SeededRng colonyRng = rng.Fork();
        int colonyCount = DetermineAutoColonyCount(profile, suitability, colonyRng, useCaseSettings, pressureContext);
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
        GenerationUseCaseSettings? useCaseSettings,
        ColonyPressureContext? pressureContext = null)
    {
        double permissiveness = GenerationUseCaseSettings.NeutralPermissiveness;

        int count = 1;
        double adjustedChance = PopulationProbability.CalculateColonyProbability(profile, suitability, permissiveness, pressureContext);
        int maxColonies = 1 + (int)System.Math.Round(3.0 * permissiveness);
        double additionalChance = adjustedChance * Lerp(0.12, 0.40, permissiveness);
        while (count < maxColonies && rng.Randf() < additionalChance)
        {
            count += 1;
            additionalChance *= Lerp(0.18, 0.45, permissiveness);
        }

        return count;
    }

    /// <summary>
    /// Rebuilds colony generation for every populated body in a completed solar system.
    /// </summary>
    public static void RebuildColoniesForSystem(
        SolarSystem? system,
        GenerationUseCaseSettings? useCaseSettings = null,
        NativeSystemPressureSummary? nearbySystemSummary = null,
        int currentYear = DefaultCurrentYear)
    {
        if (system == null)
        {
            return;
        }

        NativeSystemPressureSummary externalSummary = nearbySystemSummary ?? NativeSystemPressureSummary.Empty;
        List<CelestialBody> nativeSourceBodies = GetNativeSourceBodies(system);
        List<CelestialBody> targetBodies = GetColonyTargetBodies(system);

        foreach (CelestialBody body in targetBodies)
        {
            PlanetPopulationData data = body.PopulationData!;
            if (data.Profile == null || data.Suitability == null)
            {
                continue;
            }

            ColonyPressureContext pressureContext = BuildColonyPressureContext(system, body, nativeSourceBodies, externalSummary);
            bool shouldGenerateColony = PopulationLikelihood.ShouldGenerateColony(
                data.Profile,
                data.Suitability,
                data.GenerationSeed,
                useCaseSettings,
                pressureContext);

            data.Colonies.Clear();
            if (shouldGenerateColony)
            {
                SeededRng rng = new(data.GenerationSeed);
                data.Colonies = GenerateColonies(
                    data.Profile,
                    data.Suitability,
                    data.NativePopulations,
                    currentYear,
                    rng,
                    useCaseSettings,
                    pressureContext);
            }

            data.Population = data.GetTotalPopulation();
            data.IsActive = data.GetTotalPopulation() > 0;
        }
    }

    private static double Lerp(double minValue, double maxValue, double factor)
    {
        return minValue + ((maxValue - minValue) * factor);
    }

    private static List<CelestialBody> GetNativeSourceBodies(SolarSystem system)
    {
        List<CelestialBody> nativeBodies = new();
        foreach (CelestialBody body in system.Bodies.Values)
        {
            if (!body.HasPopulationData() || body.PopulationData == null || !body.PopulationData.HasExtantNatives())
            {
                continue;
            }

            nativeBodies.Add(body);
        }

        nativeBodies.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
        return nativeBodies;
    }

    private static List<CelestialBody> GetColonyTargetBodies(SolarSystem system)
    {
        List<CelestialBody> targetBodies = new();
        foreach (CelestialBody body in system.Bodies.Values)
        {
            if (!body.HasPopulationData() || body.PopulationData == null)
            {
                continue;
            }

            targetBodies.Add(body);
        }

        targetBodies.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
        return targetBodies;
    }

    private static ColonyPressureContext BuildColonyPressureContext(
        SolarSystem system,
        CelestialBody targetBody,
        List<CelestialBody> nativeSourceBodies,
        NativeSystemPressureSummary nearbySystemSummary)
    {
        List<(double DistanceAu, string BodyId, CelestialBody Body)> orderedSources = new();
        foreach (CelestialBody sourceBody in nativeSourceBodies)
        {
            double distanceAu = CalculateNativeDistanceAu(system, targetBody, sourceBody);
            orderedSources.Add((distanceAu, sourceBody.Id, sourceBody));
        }

        orderedSources.Sort((left, right) =>
        {
            int distanceComparison = left.DistanceAu.CompareTo(right.DistanceAu);
            if (distanceComparison != 0)
            {
                return distanceComparison;
            }

            return string.CompareOrdinal(left.BodyId, right.BodyId);
        });

        int localNativeWorldCount = 0;
        double rawLocalPressure = 0.0;
        foreach ((double distanceAu, string _, CelestialBody sourceBody) in orderedSources)
        {
            double distanceFactor = CalculateDistanceFactor(system, targetBody, sourceBody, distanceAu);
            if (distanceFactor <= 0.0)
            {
                continue;
            }

            localNativeWorldCount += 1;
            int nativePopulation = sourceBody.PopulationData!.GetNativePopulation();
            double populationFactor = NormalizePopulation(nativePopulation);
            rawLocalPressure += distanceFactor * populationFactor;
        }

        return new ColonyPressureContext
        {
            LocalNativeWorldCount = localNativeWorldCount,
            LocalNativePressure = 1.0 - System.Math.Exp(-rawLocalPressure * 0.85),
            NearbyNativeWorldCount = nearbySystemSummary.NativeWorldCount,
            NearbySystemNativePressure = System.Math.Clamp(nearbySystemSummary.PressureSignal, 0.0, 1.0),
        };
    }

    private static double CalculateNativeDistanceAu(SolarSystem system, CelestialBody targetBody, CelestialBody sourceBody)
    {
        if (targetBody.Id == sourceBody.Id)
        {
            return 0.0;
        }

        double targetAnchorAu = ResolveStarAnchorDistanceAu(system, targetBody);
        double sourceAnchorAu = ResolveStarAnchorDistanceAu(system, sourceBody);
        return System.Math.Abs(targetAnchorAu - sourceAnchorAu);
    }

    private static double ResolveStarAnchorDistanceAu(SolarSystem system, CelestialBody body)
    {
        if (!body.HasOrbital() || body.Orbital == null)
        {
            return 0.0;
        }

        if (body.Type == CelestialType.Type.Moon && !string.IsNullOrWhiteSpace(body.Orbital.ParentId))
        {
            CelestialBody? parentBody = system.GetBody(body.Orbital.ParentId);
            if (parentBody != null && parentBody.HasOrbital() && parentBody.Orbital != null)
            {
                return parentBody.Orbital.SemiMajorAxisM / 149597870700.0;
            }
        }

        return body.Orbital.SemiMajorAxisM / 149597870700.0;
    }

    private static double CalculateDistanceFactor(
        SolarSystem system,
        CelestialBody targetBody,
        CelestialBody sourceBody,
        double distanceAu)
    {
        if (targetBody.Id == sourceBody.Id)
        {
            return 1.0;
        }

        double distanceFactor = 1.0 - System.Math.Clamp(distanceAu / 12.0, 0.0, 1.0);
        if (SharesSameParentBody(targetBody, sourceBody))
        {
            distanceFactor = System.Math.Max(distanceFactor, 0.90);
        }
        else if (IsParentChildPair(targetBody, sourceBody))
        {
            distanceFactor = System.Math.Max(distanceFactor, 0.85);
        }
        else if (OrbitsDifferentParents(system, targetBody, sourceBody))
        {
            distanceFactor *= 0.75;
        }

        return System.Math.Clamp(distanceFactor, 0.0, 1.0);
    }

    private static bool SharesSameParentBody(CelestialBody left, CelestialBody right)
    {
        if (!left.HasOrbital() || left.Orbital == null || !right.HasOrbital() || right.Orbital == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(left.Orbital.ParentId) || string.IsNullOrWhiteSpace(right.Orbital.ParentId))
        {
            return false;
        }

        return left.Orbital.ParentId == right.Orbital.ParentId;
    }

    private static bool IsParentChildPair(CelestialBody left, CelestialBody right)
    {
        if (!left.HasOrbital() || left.Orbital == null || !right.HasOrbital() || right.Orbital == null)
        {
            return false;
        }

        return left.Orbital.ParentId == right.Id || right.Orbital.ParentId == left.Id;
    }

    private static bool OrbitsDifferentParents(SolarSystem system, CelestialBody left, CelestialBody right)
    {
        double leftAnchorAu = ResolveStarAnchorDistanceAu(system, left);
        double rightAnchorAu = ResolveStarAnchorDistanceAu(system, right);
        return System.Math.Abs(leftAnchorAu - rightAnchorAu) > 0.0;
    }

    private static double NormalizePopulation(int nativePopulation)
    {
        if (nativePopulation <= 0)
        {
            return 0.35;
        }

        return 0.35 + (0.65 * System.Math.Clamp(System.Math.Log10(nativePopulation + 1.0) / 9.0, 0.0, 1.0));
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

    private static void HarmonizeSentienceAssessmentWithNativePopulations(PlanetPopulationData data)
    {
        if (data.NativePopulations.Count <= 0)
        {
            return;
        }

        if (data.SentienceAssessment == null)
        {
            data.SentienceAssessment = new SentienceAssessment();
        }

        data.SentienceAssessment.Status = ConceptRunStatus.Generated;
        data.SentienceAssessment.HasSentientLife = true;
        if (string.IsNullOrWhiteSpace(data.SentienceAssessment.CandidateSpeciesName))
        {
            data.SentienceAssessment.CandidateSpeciesName = data.NativePopulations[0].Name;
        }

        data.SentienceAssessment.StatusReason = "Native population generation confirmed a sentient lineage on this world.";
    }
}
