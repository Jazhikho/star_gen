using StarGen.Domain.Population;
using StarGen.Domain.Concepts.Pipeline;

namespace StarGen.Domain.Concepts;

/// <summary>
/// Minimal cross-layer context snapshot used by concept tools.
/// </summary>
public sealed class ConceptContextSnapshot
{
    /// <summary>
    /// Persisted concept results associated with the source context.
    /// </summary>
    public ConceptResultStore PersistedResults { get; set; } = new ConceptResultStore();

    /// <summary>
    /// Resolved environment profile when the context comes from persisted world state.
    /// </summary>
    public PlanetEnvironmentProfile? EnvironmentProfile { get; set; }

    /// <summary>
    /// Resolved ecology state when available.
    /// </summary>
    public EcologyState? EcologyState { get; set; }

    /// <summary>
    /// Resolved species and evolution state when available.
    /// </summary>
    public SpeciesEvolutionState? SpeciesEvolution { get; set; }

    /// <summary>
    /// Resolved sentience assessment when available.
    /// </summary>
    public SentienceAssessment? SentienceAssessment { get; set; }

    /// <summary>
    /// Resolved society state when available.
    /// </summary>
    public SocietyState? SocietyState { get; set; }

    /// <summary>
    /// Resolved religion state when available.
    /// </summary>
    public ReligionState? ReligionState { get; set; }

    /// <summary>
    /// Resolved language state when available.
    /// </summary>
    public LanguageState? LanguageState { get; set; }

    /// <summary>
    /// Resolved disease state when available.
    /// </summary>
    public DiseaseState? DiseaseState { get; set; }

    /// <summary>
    /// Seed to use when no stronger context exists.
    /// </summary>
    public int Seed { get; set; }

    /// <summary>
    /// Galaxy seed if available.
    /// </summary>
    public int GalaxySeed { get; set; }

    /// <summary>
    /// System name if available.
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Body identifier if available.
    /// </summary>
    public string BodyId { get; set; } = string.Empty;

    /// <summary>
    /// Body display name if available.
    /// </summary>
    public string BodyName { get; set; } = string.Empty;

    /// <summary>
    /// Body type if available.
    /// </summary>
    public string BodyType { get; set; } = string.Empty;

    /// <summary>
    /// Human-habitability score.
    /// </summary>
    public int HabitabilityScore { get; set; }

    /// <summary>
    /// Average surface temperature in Kelvin.
    /// </summary>
    public double AvgTemperatureK { get; set; } = 288.15;

    /// <summary>
    /// Normalized water availability.
    /// </summary>
    public double WaterAvailability { get; set; } = 0.5;

    /// <summary>
    /// Normalized oxygen level.
    /// </summary>
    public double OxygenLevel { get; set; } = 0.21;

    /// <summary>
    /// Surface gravity in G.
    /// </summary>
    public double GravityG { get; set; } = 1.0;

    /// <summary>
    /// Radiation level.
    /// </summary>
    public double RadiationLevel { get; set; } = 0.1;

    /// <summary>
    /// Total population if present.
    /// </summary>
    public int Population { get; set; }

    /// <summary>
    /// Dominant biome string.
    /// </summary>
    public string DominantBiome { get; set; } = "Barren";

    /// <summary>
    /// Government regime if available.
    /// </summary>
    public GovernmentType.Regime? Regime { get; set; }

    /// <summary>
    /// Technology level if available.
    /// </summary>
    public TechnologyLevel.Level? TechnologyLevel { get; set; }

    /// <summary>
    /// Human-readable context source.
    /// </summary>
    public string SourceLabel { get; set; } = "Manual concept sandbox";

    /// <summary>
    /// Returns a shallow clone.
    /// </summary>
    public ConceptContextSnapshot Clone()
    {
        ConceptContextSnapshot clone = new ConceptContextSnapshot
        {
            PersistedResults = PersistedResults.Clone(),
            Seed = Seed,
            GalaxySeed = GalaxySeed,
            SystemName = SystemName,
            BodyId = BodyId,
            BodyName = BodyName,
            BodyType = BodyType,
            HabitabilityScore = HabitabilityScore,
            AvgTemperatureK = AvgTemperatureK,
            WaterAvailability = WaterAvailability,
            OxygenLevel = OxygenLevel,
            GravityG = GravityG,
            RadiationLevel = RadiationLevel,
            Population = Population,
            DominantBiome = DominantBiome,
            Regime = Regime,
            TechnologyLevel = TechnologyLevel,
            SourceLabel = SourceLabel,
        };

        clone.EnvironmentProfile = CloneEnvironmentProfile(EnvironmentProfile);
        clone.EcologyState = CloneEcologyState(EcologyState);
        clone.SpeciesEvolution = CloneSpeciesEvolutionState(SpeciesEvolution);
        clone.SentienceAssessment = CloneSentienceAssessment(SentienceAssessment);
        clone.SocietyState = CloneSocietyState(SocietyState);
        clone.ReligionState = CloneReligionState(ReligionState);
        clone.LanguageState = CloneLanguageState(LanguageState);
        clone.DiseaseState = CloneDiseaseState(DiseaseState);
        return clone;
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

        return global::StarGen.Domain.Concepts.Pipeline.EcologyState.FromDictionary(state.ToDictionary());
    }

    private static SpeciesEvolutionState? CloneSpeciesEvolutionState(SpeciesEvolutionState? state)
    {
        if (state == null)
        {
            return null;
        }

        return global::StarGen.Domain.Concepts.Pipeline.SpeciesEvolutionState.FromDictionary(state.ToDictionary());
    }

    private static SentienceAssessment? CloneSentienceAssessment(SentienceAssessment? assessment)
    {
        if (assessment == null)
        {
            return null;
        }

        return global::StarGen.Domain.Concepts.Pipeline.SentienceAssessment.FromDictionary(assessment.ToDictionary());
    }

    private static SocietyState? CloneSocietyState(SocietyState? state)
    {
        if (state == null)
        {
            return null;
        }

        return global::StarGen.Domain.Concepts.Pipeline.SocietyState.FromDictionary(state.ToDictionary());
    }

    private static ReligionState? CloneReligionState(ReligionState? state)
    {
        if (state == null)
        {
            return null;
        }

        return global::StarGen.Domain.Concepts.Pipeline.ReligionState.FromDictionary(state.ToDictionary());
    }

    private static LanguageState? CloneLanguageState(LanguageState? state)
    {
        if (state == null)
        {
            return null;
        }

        return global::StarGen.Domain.Concepts.Pipeline.LanguageState.FromDictionary(state.ToDictionary());
    }

    private static DiseaseState? CloneDiseaseState(DiseaseState? state)
    {
        if (state == null)
        {
            return null;
        }

        return global::StarGen.Domain.Concepts.Pipeline.DiseaseState.FromDictionary(state.ToDictionary());
    }
}
