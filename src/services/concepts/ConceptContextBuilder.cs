using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;

namespace StarGen.Services.Concepts;

/// <summary>
/// Builds cross-layer concept context snapshots from existing StarGen data.
/// </summary>
public static class ConceptContextBuilder
{
    /// <summary>
    /// Returns a manual default snapshot.
    /// </summary>
    public static ConceptContextSnapshot CreateDefault(int seed)
    {
        return new ConceptContextSnapshot
        {
            Seed = seed,
            HabitabilityScore = 5,
            AvgTemperatureK = 288.15,
            WaterAvailability = 0.55,
            OxygenLevel = 0.21,
            GravityG = 1.0,
            RadiationLevel = 0.1,
            Population = 2500000,
            DominantBiome = "Temperate",
            SourceLabel = "Manual concept sandbox",
            TechnologyLevel = Domain.Population.TechnologyLevel.Level.Information,
            Regime = GovernmentType.Regime.Constitutional,
        };
    }

    /// <summary>
    /// Builds a snapshot from a body and optional system.
    /// </summary>
    public static ConceptContextSnapshot FromBody(CelestialBody body, SolarSystem? system = null, int galaxySeed = 0)
    {
        ConceptContextSnapshot snapshot = CreateDefault(GetSeedFromBody(body));
        snapshot.GalaxySeed = galaxySeed;
        if (system != null)
        {
            snapshot.SystemName = system.Name;
        }
        else
        {
            snapshot.SystemName = string.Empty;
        }
        snapshot.BodyId = body.Id;
        snapshot.BodyName = body.Name;
        snapshot.BodyType = body.GetTypeString();
        if (!string.IsNullOrEmpty(snapshot.SystemName))
        {
            snapshot.SourceLabel = snapshot.SystemName + " / " + snapshot.BodyName;
        }
        else
        {
            snapshot.SourceLabel = snapshot.BodyName;
        }
        snapshot.PersistedResults = body.ConceptResults.Clone();
        snapshot.EnvironmentProfile = CloneEnvironmentProfile(body.EnvironmentProfile);
        snapshot.EcologyState = CloneEcologyState(body.Ecology);
        snapshot.SpeciesEvolution = CloneSpeciesEvolutionState(body.SpeciesEvolution);
        snapshot.SentienceAssessment = CloneSentienceAssessment(body.Sentience);
        snapshot.DiseaseState = CloneDiseaseState(body.Disease);

        if (body.HasPopulationData() && body.PopulationData != null)
        {
            ApplyPopulationData(snapshot, body.PopulationData);
            snapshot.PersistedResults.MergeFrom(body.PopulationData.ConceptResults);
        }

        return snapshot;
    }

    /// <summary>
    /// Builds a snapshot from a system.
    /// </summary>
    public static ConceptContextSnapshot FromSystem(SolarSystem system, int seed)
    {
        ConceptContextSnapshot snapshot = CreateDefault(seed);
        snapshot.SystemName = system.Name;
        snapshot.SourceLabel = system.Name;
        snapshot.Population = system.GetTotalPopulation();
        snapshot.PersistedResults = system.ConceptResults.Clone();
        return snapshot;
    }

    /// <summary>
    /// Builds a snapshot from a galaxy selection.
    /// </summary>
    public static ConceptContextSnapshot FromGalaxy(GalaxyConfig? config, int galaxySeed, int starSeed)
    {
        int snapshotSeed = galaxySeed;
        if (starSeed != 0)
        {
            snapshotSeed = starSeed;
        }

        ConceptContextSnapshot snapshot = CreateDefault(snapshotSeed);
        snapshot.GalaxySeed = galaxySeed;
        snapshot.SourceLabel = $"Galaxy seed {galaxySeed}";
        if (config != null)
        {
            snapshot.SystemName = config.Type.ToString();
        }

        return snapshot;
    }

    /// <summary>
    /// Builds a snapshot from a native population on a body.
    /// </summary>
    public static ConceptContextSnapshot FromNativePopulation(
        CelestialBody body,
        NativePopulation nativePopulation,
        SolarSystem? system = null,
        int galaxySeed = 0)
    {
        ConceptContextSnapshot snapshot = FromBody(body, system, galaxySeed);
        snapshot.Seed = StableStringHash(nativePopulation.Id) ^ GetSeedFromBody(body);
        snapshot.BodyName = nativePopulation.Name;
        snapshot.Population = nativePopulation.Population;
        if (nativePopulation.PrimaryBiome != string.Empty)
        {
            snapshot.DominantBiome = nativePopulation.PrimaryBiome;
        }
        snapshot.Regime = nativePopulation.GetRegime();
        snapshot.TechnologyLevel = nativePopulation.TechLevel;
        snapshot.SourceLabel = BuildPopulationSourceLabel(system, body, nativePopulation.Name);
        snapshot.PersistedResults = nativePopulation.ConceptResults.Clone();
        snapshot.SocietyState = CloneSocietyState(nativePopulation.SocietyState);
        snapshot.ReligionState = CloneReligionState(nativePopulation.ReligionState);
        snapshot.LanguageState = CloneLanguageState(nativePopulation.LanguageState);
        snapshot.DiseaseState = CloneDiseaseState(nativePopulation.DiseaseState);
        return snapshot;
    }

    /// <summary>
    /// Builds a snapshot from a colony on a body.
    /// </summary>
    public static ConceptContextSnapshot FromColony(
        CelestialBody body,
        Colony colony,
        SolarSystem? system = null,
        int galaxySeed = 0)
    {
        ConceptContextSnapshot snapshot = FromBody(body, system, galaxySeed);
        snapshot.Seed = StableStringHash(colony.Id) ^ GetSeedFromBody(body);
        snapshot.BodyName = colony.Name;
        snapshot.Population = colony.Population;
        snapshot.Regime = colony.GetRegime();
        snapshot.TechnologyLevel = colony.TechLevel;
        snapshot.SourceLabel = BuildPopulationSourceLabel(system, body, colony.Name);
        snapshot.PersistedResults = colony.ConceptResults.Clone();
        snapshot.SocietyState = CloneSocietyState(colony.SocietyState);
        snapshot.ReligionState = CloneReligionState(colony.ReligionState);
        snapshot.LanguageState = CloneLanguageState(colony.LanguageState);
        snapshot.DiseaseState = CloneDiseaseState(colony.DiseaseState);
        return snapshot;
    }

    private static int GetSeedFromBody(CelestialBody body)
    {
        if (body.Provenance != null)
        {
            return (int)body.Provenance.GenerationSeed;
        }

        if (body.PopulationData != null && body.PopulationData.GenerationSeed != 0)
        {
            return body.PopulationData.GenerationSeed;
        }

        return StableStringHash(body.Id);
    }

    private static void ApplyPopulationData(ConceptContextSnapshot snapshot, PlanetPopulationData data)
    {
        snapshot.Population = data.GetTotalPopulation();
        if (data.EnvironmentProfile != null)
        {
            snapshot.EnvironmentProfile = PlanetEnvironmentProfile.FromDictionary(data.EnvironmentProfile.ToDictionary());
        }

        if (data.EcologyState != null)
        {
            snapshot.EcologyState = global::StarGen.Domain.Concepts.Pipeline.EcologyState.FromDictionary(data.EcologyState.ToDictionary());
        }

        if (data.SpeciesEvolution != null)
        {
            snapshot.SpeciesEvolution = SpeciesEvolutionState.FromDictionary(data.SpeciesEvolution.ToDictionary());
        }

        if (data.SentienceAssessment != null)
        {
            snapshot.SentienceAssessment = SentienceAssessment.FromDictionary(data.SentienceAssessment.ToDictionary());
        }

        if (data.DiseaseState != null)
        {
            snapshot.DiseaseState = DiseaseState.FromDictionary(data.DiseaseState.ToDictionary());
        }

        if (data.Profile != null)
        {
            PlanetProfile profile = data.Profile;
            snapshot.HabitabilityScore = profile.HabitabilityScore;
            snapshot.AvgTemperatureK = profile.AvgTemperatureK;
            snapshot.GravityG = profile.GravityG;
            snapshot.RadiationLevel = profile.RadiationLevel;
            snapshot.WaterAvailability = profile.OceanCoverage;
            if (profile.HasBreathableAtmosphere)
            {
                snapshot.OxygenLevel = 0.21;
            }
            else
            {
                snapshot.OxygenLevel = 0.02;
            }
            snapshot.DominantBiome = BiomeType.ToStringName(profile.GetDominantBiome());
        }

        Colony? colony = null;
        foreach (Colony activeColony in data.GetActiveColonies())
        {
            colony = activeColony;
            break;
        }

        if (colony != null)
        {
            snapshot.Regime = colony.GetRegime();
            snapshot.TechnologyLevel = colony.TechLevel;
            return;
        }

        foreach (NativePopulation nativePopulation in data.GetExtantNatives())
        {
            snapshot.Regime = nativePopulation.GetRegime();
            snapshot.TechnologyLevel = nativePopulation.TechLevel;
            if (!string.IsNullOrEmpty(nativePopulation.PrimaryBiome))
            {
                snapshot.DominantBiome = nativePopulation.PrimaryBiome;
            }

            return;
        }
    }

    private static string BuildPopulationSourceLabel(SolarSystem? system, CelestialBody body, string populationName)
    {
        if (system != null && !string.IsNullOrEmpty(system.Name))
        {
            return system.Name + " / " + body.Name + " / " + populationName;
        }

        return body.Name + " / " + populationName;
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

    private static PlanetEnvironmentProfile? CloneEnvironmentProfile(PlanetEnvironmentProfile? profile)
    {
        if (profile == null)
        {
            return null;
        }

        return PlanetEnvironmentProfile.FromDictionary(profile.ToDictionary());
    }

    private static EcologyState? CloneEcologyState(EcologyState? state)
    {
        if (state == null)
        {
            return null;
        }

        return EcologyState.FromDictionary(state.ToDictionary());
    }

    private static SpeciesEvolutionState? CloneSpeciesEvolutionState(SpeciesEvolutionState? state)
    {
        if (state == null)
        {
            return null;
        }

        return SpeciesEvolutionState.FromDictionary(state.ToDictionary());
    }

    private static SentienceAssessment? CloneSentienceAssessment(SentienceAssessment? state)
    {
        if (state == null)
        {
            return null;
        }

        return SentienceAssessment.FromDictionary(state.ToDictionary());
    }

    private static SocietyState? CloneSocietyState(SocietyState? state)
    {
        if (state == null)
        {
            return null;
        }

        return SocietyState.FromDictionary(state.ToDictionary());
    }

    private static ReligionState? CloneReligionState(ReligionState? state)
    {
        if (state == null)
        {
            return null;
        }

        return ReligionState.FromDictionary(state.ToDictionary());
    }

    private static LanguageState? CloneLanguageState(LanguageState? state)
    {
        if (state == null)
        {
            return null;
        }

        return LanguageState.FromDictionary(state.ToDictionary());
    }

    private static DiseaseState? CloneDiseaseState(DiseaseState? state)
    {
        if (state == null)
        {
            return null;
        }

        return DiseaseState.FromDictionary(state.ToDictionary());
    }
}
