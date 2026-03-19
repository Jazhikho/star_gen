using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
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
        snapshot.SystemName = system?.Name ?? string.Empty;
        snapshot.BodyId = body.Id;
        snapshot.BodyName = body.Name;
        snapshot.BodyType = body.GetTypeString();
        snapshot.SourceLabel = !string.IsNullOrEmpty(snapshot.SystemName)
            ? $"{snapshot.SystemName} / {snapshot.BodyName}"
            : snapshot.BodyName;
        snapshot.PersistedResults = body.ConceptResults.Clone();

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
        ConceptContextSnapshot snapshot = CreateDefault(starSeed != 0 ? starSeed : galaxySeed);
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
        snapshot.DominantBiome = nativePopulation.PrimaryBiome != string.Empty ? nativePopulation.PrimaryBiome : snapshot.DominantBiome;
        snapshot.Regime = nativePopulation.GetRegime();
        snapshot.TechnologyLevel = nativePopulation.TechLevel;
        snapshot.SourceLabel = BuildPopulationSourceLabel(system, body, nativePopulation.Name);
        snapshot.PersistedResults = nativePopulation.ConceptResults.Clone();
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
        if (data.Profile != null)
        {
            PlanetProfile profile = data.Profile;
            snapshot.HabitabilityScore = profile.HabitabilityScore;
            snapshot.AvgTemperatureK = profile.AvgTemperatureK;
            snapshot.GravityG = profile.GravityG;
            snapshot.RadiationLevel = profile.RadiationLevel;
            snapshot.WaterAvailability = profile.OceanCoverage;
            snapshot.OxygenLevel = profile.HasBreathableAtmosphere ? 0.21 : 0.02;
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
}
