using System;
using System.Collections.Generic;
using Godot;
using StarGen.Concepts.ReligionGenerator;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Civilization;
using StarGen.Domain.Concepts.Disease;
using StarGen.Domain.Concepts.Language;
using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Concepts.Religion;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;

namespace StarGen.Services.Concepts;

/// <summary>
/// Populates persisted concept state on generated or loaded StarGen world objects.
/// </summary>
public static class ConceptWorldStateGenerator
{
    /// <summary>
    /// Ensures persisted concept results exist for the full system and its bodies.
    /// </summary>
    public static void EnsureSystemConcepts(SolarSystem? system, int galaxySeed = 0)
    {
        if (system == null)
        {
            return;
        }

        foreach (CelestialBody body in system.Bodies.Values)
        {
            EnsureBodyConcepts(body, system, galaxySeed);
        }

        CelestialBody? aggregateBody = SelectAggregateBody(system);
        if (aggregateBody == null)
        {
            return;
        }

        if (aggregateBody.HasConceptResults())
        {
            CopyMissingResults(system.ConceptResults, aggregateBody.ConceptResults);
        }

        if (aggregateBody.PopulationData != null && aggregateBody.PopulationData.HasConceptResults())
        {
            CopyMissingResults(system.ConceptResults, aggregateBody.PopulationData.ConceptResults);
        }

        ConceptContextSnapshot systemSnapshot = ConceptContextBuilder.FromSystem(system, ResolveSystemSeed(system));
        if (systemSnapshot.Population > 0)
        {
            EnsureConcept(system.ConceptResults, ConceptKind.Civilization, systemSnapshot);
            EnsureConcept(system.ConceptResults, ConceptKind.Language, systemSnapshot);
            EnsureConcept(system.ConceptResults, ConceptKind.Religion, systemSnapshot);
            EnsureConcept(system.ConceptResults, ConceptKind.Disease, systemSnapshot);
        }
    }

    /// <summary>
    /// Ensures persisted concept results exist for a body and any attached populations.
    /// </summary>
    public static void EnsureBodyConcepts(CelestialBody? body, SolarSystem? system = null, int galaxySeed = 0)
    {
        if (body == null)
        {
            return;
        }

        EnsureBodyPipelineState(body);

        ConceptContextSnapshot bodySnapshot = ConceptContextBuilder.FromBody(body, system, galaxySeed);
        EnsureConcept(body.ConceptResults, ConceptKind.Ecology, bodySnapshot);
        EnsureConcept(body.ConceptResults, ConceptKind.Evolution, bodySnapshot);

        PlanetPopulationData? populationData = body.PopulationData;
        if (populationData == null || !populationData.IsInhabited())
        {
            return;
        }

        EnsurePopulationConcepts(body, populationData, system, galaxySeed);
        CopyMissingResults(body.ConceptResults, populationData.ConceptResults);
    }

    private static void EnsurePopulationConcepts(
        CelestialBody body,
        PlanetPopulationData populationData,
        SolarSystem? system,
        int galaxySeed)
    {
        foreach (NativePopulation nativePopulation in populationData.NativePopulations)
        {
            if (!nativePopulation.IsExtant)
            {
                continue;
            }

            EnsureNativeStates(body, nativePopulation, system, galaxySeed);
            ConceptContextSnapshot nativeSnapshot = ConceptContextBuilder.FromNativePopulation(body, nativePopulation, system, galaxySeed);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Civilization, nativeSnapshot);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Religion, nativeSnapshot);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Language, nativeSnapshot);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Disease, nativeSnapshot);
            EnsureHistory(nativePopulation.History, nativePopulation.ConceptResults);
        }

        foreach (Colony colony in populationData.Colonies)
        {
            if (!colony.IsActive)
            {
                continue;
            }

            EnsureColonyStates(body, colony, system, galaxySeed);
            ConceptContextSnapshot colonySnapshot = ConceptContextBuilder.FromColony(body, colony, system, galaxySeed);
            EnsureConcept(colony.ConceptResults, ConceptKind.Civilization, colonySnapshot);
            EnsureConcept(colony.ConceptResults, ConceptKind.Religion, colonySnapshot);
            EnsureConcept(colony.ConceptResults, ConceptKind.Language, colonySnapshot);
            EnsureConcept(colony.ConceptResults, ConceptKind.Disease, colonySnapshot);
            EnsureHistory(colony.History, colony.ConceptResults);
        }

        Variant dominant = populationData.GetDominantPopulation();
        if (dominant.VariantType == Variant.Type.Nil)
        {
            return;
        }

        if (dominant.Obj is NativePopulation dominantNative)
        {
            CopyMissingResults(populationData.ConceptResults, dominantNative.ConceptResults);
        }
        else if (dominant.Obj is Colony dominantColony)
        {
            CopyMissingResults(populationData.ConceptResults, dominantColony.ConceptResults);
        }

        ConceptContextSnapshot bodySnapshot = ConceptContextBuilder.FromBody(body, system, galaxySeed);
        if (populationData.DiseaseState == null)
        {
            populationData.DiseaseState = GenerateDiseaseState(bodySnapshot, true);
        }
        EnsureConcept(populationData.ConceptResults, ConceptKind.Disease, bodySnapshot);
    }

    private static void EnsureConcept(ConceptResultStore target, ConceptKind kind, ConceptContextSnapshot snapshot)
    {
        if (target.Has(kind))
        {
            return;
        }

        ConceptRunResult result = ConceptResultFactory.Run(new ConceptRunRequest
        {
            Kind = kind,
            Context = snapshot,
        });
        target.Set(kind, result);
    }

    private static void CopyMissingResults(ConceptResultStore destination, ConceptResultStore source)
    {
        IReadOnlyDictionary<ConceptKind, ConceptRunResult> values = source.GetAll();
        foreach (KeyValuePair<ConceptKind, ConceptRunResult> entry in values)
        {
            if (!destination.Has(entry.Key))
            {
                destination.Set(entry.Key, entry.Value);
            }
        }
    }

    private static void EnsureBodyPipelineState(CelestialBody body)
    {
        if (body.PopulationData == null)
        {
            return;
        }

        if (body.EnvironmentProfile == null)
        {
            body.EnvironmentProfile = body.PopulationData.EnvironmentProfile;
        }

        if (body.Ecology == null)
        {
            body.Ecology = body.PopulationData.EcologyState;
        }

        if (body.SpeciesEvolution == null)
        {
            body.SpeciesEvolution = body.PopulationData.SpeciesEvolution;
        }

        if (body.Sentience == null)
        {
            body.Sentience = body.PopulationData.SentienceAssessment;
        }

        if (body.Disease == null)
        {
            body.Disease = body.PopulationData.DiseaseState;
        }
    }

    private static void EnsureNativeStates(
        CelestialBody body,
        NativePopulation nativePopulation,
        SolarSystem? system,
        int galaxySeed)
    {
        ConceptContextSnapshot snapshot = ConceptContextBuilder.FromNativePopulation(body, nativePopulation, system, galaxySeed);
        if (nativePopulation.SocietyState == null)
        {
            nativePopulation.SocietyState = GenerateSocietyState(snapshot);
        }

        if (nativePopulation.ReligionState == null)
        {
            nativePopulation.ReligionState = GenerateReligionState(snapshot);
        }

        if (nativePopulation.LanguageState == null)
        {
            nativePopulation.LanguageState = GenerateLanguageState(snapshot);
        }

        if (nativePopulation.DiseaseState == null)
        {
            nativePopulation.DiseaseState = GenerateDiseaseState(snapshot, true);
        }
    }

    private static void EnsureColonyStates(
        CelestialBody body,
        Colony colony,
        SolarSystem? system,
        int galaxySeed)
    {
        ConceptContextSnapshot snapshot = ConceptContextBuilder.FromColony(body, colony, system, galaxySeed);
        if (colony.SocietyState == null)
        {
            colony.SocietyState = GenerateSocietyState(snapshot);
        }

        if (colony.ReligionState == null)
        {
            colony.ReligionState = GenerateReligionState(snapshot);
        }

        if (colony.LanguageState == null)
        {
            colony.LanguageState = GenerateLanguageState(snapshot);
        }

        if (colony.DiseaseState == null)
        {
            colony.DiseaseState = GenerateDiseaseState(snapshot, true);
        }
    }

    private static SocietyState GenerateSocietyState(ConceptContextSnapshot snapshot)
    {
        SocietyState state = new SocietyState();
        if (snapshot.Population <= 0)
        {
            state.Status = ConceptRunStatus.NotApplicable;
            state.StatusReason = "Civilisation requires a sentient population.";
            state.Provenance = BuildProvenance(ConceptKind.Civilization, snapshot.Seed, "pipeline-civilization-v2", snapshot.SourceLabel);
            return state;
        }

        state.Status = ConceptRunStatus.Generated;
        state.Snapshot = CivilizationConceptGenerator.Generate(snapshot);
        state.Provenance = BuildProvenance(ConceptKind.Civilization, snapshot.Seed, "pipeline-civilization-v2", snapshot.SourceLabel);
        return state;
    }

    private static ReligionState GenerateReligionState(ConceptContextSnapshot snapshot)
    {
        ReligionState state = new ReligionState();
        if (snapshot.Population <= 0)
        {
            state.Status = ConceptRunStatus.NotApplicable;
            state.StatusReason = "Religion requires an extant sentient population.";
            state.Provenance = BuildProvenance(ConceptKind.Religion, snapshot.Seed, ReligionGenerator.GeneratorVersion, snapshot.SourceLabel);
            return state;
        }

        ReligionParams parameters = BuildReligionParams(snapshot);
        ReligionResult result = ReligionGenerator.Generate(parameters);
        ReligionConceptSnapshot religionSnapshot = new ReligionConceptSnapshot();
        religionSnapshot.Deity = result.Deity.Name;
        religionSnapshot.Cosmology = result.Cosmology.Desc;
        religionSnapshot.Authority = result.Authority.Name;
        religionSnapshot.Specialist = result.Specialist.Name;
        religionSnapshot.Rituals = new List<string>(result.Rituals);
        religionSnapshot.Ethics = new List<string>(result.Ethics);
        religionSnapshot.Landscape = new List<string> { result.Landscape.HegemonyDesc };

        state.Status = ConceptRunStatus.Generated;
        state.Snapshot = religionSnapshot;
        state.Provenance = BuildProvenance(ConceptKind.Religion, snapshot.Seed, ReligionGenerator.GeneratorVersion, snapshot.SourceLabel);
        return state;
    }

    private static LanguageState GenerateLanguageState(ConceptContextSnapshot snapshot)
    {
        LanguageState state = new LanguageState();
        if (snapshot.Population <= 0)
        {
            state.Status = ConceptRunStatus.NotApplicable;
            state.StatusReason = "Language generation is reserved for sentient populations or manual atlas runs.";
            state.Provenance = BuildProvenance(ConceptKind.Language, snapshot.Seed, "pipeline-language-v2", snapshot.SourceLabel);
            return state;
        }

        state.Status = ConceptRunStatus.Generated;
        state.Snapshot = LanguageConceptGenerator.Generate(snapshot);
        state.Provenance = BuildProvenance(ConceptKind.Language, snapshot.Seed, "pipeline-language-v2", snapshot.SourceLabel);
        return state;
    }

    private static DiseaseState GenerateDiseaseState(ConceptContextSnapshot snapshot, bool allowPopulationDiseases)
    {
        DiseaseState state = new DiseaseState();
        bool ecologicalHost = snapshot.EcologyState != null && snapshot.EcologyState.Status == ConceptRunStatus.Generated;
        bool populatedHost = allowPopulationDiseases && snapshot.Population > 0;
        if (!ecologicalHost && !populatedHost)
        {
            state.Status = ConceptRunStatus.NotApplicable;
            state.StatusReason = "Disease requires a biological host ecology or an inhabited population.";
            state.Provenance = BuildProvenance(ConceptKind.Disease, snapshot.Seed, "pipeline-disease-v2", snapshot.SourceLabel);
            return state;
        }

        state.Status = ConceptRunStatus.Generated;
        state.Snapshot = DiseaseConceptGenerator.Generate(snapshot);
        state.Provenance = BuildProvenance(ConceptKind.Disease, snapshot.Seed, "pipeline-disease-v2", snapshot.SourceLabel);
        return state;
    }

    private static ReligionParams BuildReligionParams(ConceptContextSnapshot context)
    {
        ReligionParams parameters = new ReligionParams();
        parameters.Seed = context.Seed;
        parameters.Subsistence = MapReligionSubsistence(context);
        parameters.SocialOrg = MapReligionSocialOrganization(context);
        parameters.Settlement = MapReligionSettlement(context);
        parameters.Environment = MapReligionEnvironment(context);
        parameters.ExternalThreat = MapReligionThreat(context);
        parameters.Isolation = MapReligionIsolation(context);
        parameters.PoliticalPower = MapReligionPoliticalPower(context);
        parameters.WritingSystem = MapReligionWriting(context);
        if (context.Population > 10000000)
        {
            parameters.PriorTraditions = "syncretic";
        }
        else
        {
            parameters.PriorTraditions = "indigenous_only";
        }
        parameters.GenderSystem = MapReligionGenderSystem(context);
        if (context.Population > 5000000)
        {
            parameters.KinshipStructure = "lineage_corporate";
        }
        else
        {
            parameters.KinshipStructure = "extended_clan";
        }
        return parameters;
    }

    private static CelestialBody? SelectAggregateBody(SolarSystem system)
    {
        CelestialBody? bestBody = null;
        int bestPopulation = -1;
        int bestHabitability = -1;

        foreach (CelestialBody body in system.Bodies.Values)
        {
            int population = 0;
            int habitability = -1;
            if (body.PopulationData != null)
            {
                population = body.PopulationData.GetTotalPopulation();
                if (body.PopulationData.Profile != null)
                {
                    habitability = body.PopulationData.Profile.HabitabilityScore;
                }
            }

            if (population > bestPopulation)
            {
                bestPopulation = population;
                bestHabitability = habitability;
                bestBody = body;
                continue;
            }

            if (population == bestPopulation && habitability > bestHabitability)
            {
                bestHabitability = habitability;
                bestBody = body;
            }
        }

        if (bestBody != null)
        {
            return bestBody;
        }

        foreach (CelestialBody planet in system.GetPlanets())
        {
            return planet;
        }

        foreach (CelestialBody moon in system.GetMoons())
        {
            return moon;
        }

        foreach (CelestialBody star in system.GetStars())
        {
            return star;
        }

        return null;
    }

    private static int ResolveSystemSeed(SolarSystem system)
    {
        if (system.Provenance != null)
        {
            return (int)system.Provenance.GenerationSeed;
        }

        return StableStringHash(system.Id);
    }

    private static void EnsureHistory(PopulationHistory history, ConceptResultStore results)
    {
        EnsureHistoryEvent(history, results, ConceptKind.Civilization, HistoryEvent.EventType.PoliticalChange, 0, 0.18);
        EnsureHistoryEvent(history, results, ConceptKind.Religion, HistoryEvent.EventType.CulturalShift, 1, 0.08);
        EnsureHistoryEvent(history, results, ConceptKind.Language, HistoryEvent.EventType.CulturalShift, 2, 0.05);
        EnsureHistoryEvent(history, results, ConceptKind.Disease, HistoryEvent.EventType.Plague, 3, -0.35);
    }

    private static void EnsureHistoryEvent(
        PopulationHistory history,
        ConceptResultStore results,
        ConceptKind kind,
        HistoryEvent.EventType eventType,
        int yearOffset,
        double magnitude)
    {
        ConceptRunResult? result = results.Get(kind);
        if (result == null)
        {
            return;
        }

        foreach (HistoryEvent existing in history.GetAllEvents())
        {
            if (existing.Metadata.ContainsKey("concept_kind")
                && existing.Metadata["concept_kind"].VariantType == Variant.Type.String
                && string.Equals((string)existing.Metadata["concept_kind"], kind.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        HistoryEvent historyEvent = history.AddNewEvent(
            eventType,
            yearOffset,
            result.Title,
            result.Summary,
            magnitude);
        historyEvent.Metadata["concept_kind"] = kind.ToString();
        historyEvent.Metadata["concept_subtitle"] = result.Subtitle;
        historyEvent.Metadata["concept_seed"] = result.Provenance.Seed;
    }

    private static int StableStringHash(string value)
    {
        const uint fnvOffset = 2166136261;
        const uint fnvPrime = 16777619;
        uint hash = fnvOffset;

        foreach (char character in value)
        {
            hash ^= character;
            hash *= fnvPrime;
        }

        return unchecked((int)hash);
    }

    private static ConceptProvenance BuildProvenance(
        ConceptKind kind,
        int seed,
        string generatorVersion,
        string sourceContext)
    {
        ConceptProvenance provenance = new ConceptProvenance();
        provenance.ConceptId = kind.ToString();
        provenance.Seed = seed;
        provenance.GeneratorVersion = generatorVersion;
        provenance.SourceContext = sourceContext;
        provenance.InputSignature = seed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return provenance;
    }

    private static string MapReligionSubsistence(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            return "pastoral";
        }

        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            return "maritime";
        }

        if (context.Population > 8000000)
        {
            return "urban_trade";
        }

        return "agricultural";
    }

    private static string MapReligionSocialOrganization(ConceptContextSnapshot context)
    {
        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Interstellar)
        {
            return "empire";
        }

        if (context.Population > 10000000)
        {
            return "state";
        }

        if (context.Population > 500000)
        {
            return "chiefdom";
        }

        return "tribe";
    }

    private static string MapReligionSettlement(ConceptContextSnapshot context)
    {
        if (context.Population > 5000000)
        {
            return "urban_centers";
        }

        if (context.Population > 50000)
        {
            return "permanent_village";
        }

        return "semi_nomadic";
    }

    private static string MapReligionEnvironment(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            return "arid_scarce";
        }

        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            return "coastal_riverine";
        }

        if (context.RadiationLevel > 0.55)
        {
            return "harsh_extreme";
        }

        if (context.HabitabilityScore <= 3)
        {
            return "unpredictable";
        }

        return "temperate_fertile";
    }

    private static string MapReligionThreat(ConceptContextSnapshot context)
    {
        if (context.RadiationLevel > 0.7 || context.HabitabilityScore <= 2)
        {
            return "existential";
        }

        if (context.RadiationLevel > 0.35 || context.HabitabilityScore <= 4)
        {
            return "high";
        }

        return "moderate";
    }

    private static string MapReligionIsolation(ConceptContextSnapshot context)
    {
        if (context.Population > 12000000)
        {
            return "cosmopolitan";
        }

        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Spacefaring)
        {
            return "cultural_exchange";
        }

        return "trade_contact";
    }

    private static string MapReligionPoliticalPower(ConceptContextSnapshot context)
    {
        if (!context.Regime.HasValue)
        {
            return "intertwined";
        }

        if (context.Regime.Value == GovernmentType.Regime.Theocracy)
        {
            return "theocratic";
        }

        if (context.Regime.Value == GovernmentType.Regime.MassDemocracy)
        {
            return "distributed";
        }

        if (context.Regime.Value == GovernmentType.Regime.Constitutional)
        {
            return "separate";
        }

        return "intertwined";
    }

    private static string MapReligionWriting(ConceptContextSnapshot context)
    {
        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Information)
        {
            return "digital_literate";
        }

        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Renaissance)
        {
            return "literate";
        }

        return "oral";
    }

    private static string MapReligionGenderSystem(ConceptContextSnapshot context)
    {
        if (context.Regime.HasValue && context.Regime.Value == GovernmentType.Regime.Theocracy)
        {
            return "ritually_differentiated";
        }

        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Information)
        {
            return "egalitarian";
        }

        return "customary";
    }
}
