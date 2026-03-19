using StarGen.Domain.Population;

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
        return new ConceptContextSnapshot
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
    }
}
