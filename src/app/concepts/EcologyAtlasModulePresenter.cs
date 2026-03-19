using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Ecology;
using StarGen.Domain.Ecology;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the ecology concept fold-in.
/// </summary>
public sealed class EcologyAtlasModulePresenter : IConceptModulePresenter
{
    private const string GeneratorVersion = "atlas-ecology-1.0.0";

    /// <summary>
    /// Creates the ecology presenter.
    /// </summary>
    public EcologyAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Ecology,
            DisplayName = "Ecology",
            Summary = "Explore trophic structure, biosphere productivity, and ecosystem resilience from planetary context or manual sandbox inputs.",
            AcceptedContext = "Planet context or manual environment profile",
        };
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return ConceptResultFactory.Run(request);
    }

    private static EnvironmentSpec BuildSpec(ConceptContextSnapshot context)
    {
        float averageTemperature = (float)context.AvgTemperatureK;
        float tempRange = 8.0f + ((10 - context.HabitabilityScore) * 1.5f);
        return new EnvironmentSpec
        {
            Seed = unchecked((ulong)context.Seed),
            TemperatureMin = averageTemperature - tempRange,
            TemperatureMax = averageTemperature + tempRange,
            WaterAvailability = Clamp01((float)context.WaterAvailability),
            LightLevel = Clamp01(0.55f + (context.HabitabilityScore / 20.0f)),
            NutrientLevel = Clamp01(0.25f + (context.HabitabilityScore / 14.0f)),
            Gravity = (float)System.Math.Clamp(context.GravityG, 0.2, 3.0),
            RadiationLevel = Clamp01((float)context.RadiationLevel),
            OxygenLevel = Clamp01((float)context.OxygenLevel),
            SeasonalVariation = Clamp01(0.2f + ((10 - context.HabitabilityScore) / 20.0f)),
            Biome = MapBiome(context.DominantBiome),
            GeneratorVersion = GeneratorVersion,
        };
    }

    private static EcologyConceptSnapshot BuildSnapshot(EcologyWeb web)
    {
        EcologyConceptSnapshot snapshot = new()
        {
            SlotCount = web.Slots.Count,
            ConnectionCount = web.Connections.Count,
            Productivity = web.TotalProductivity,
            Biomass = web.GetTotalBiomass(),
            Complexity = web.ComplexityScore,
            Stability = web.StabilityScore,
            MaxChainLength = web.GetMaxChainLength(),
        };

        foreach (TrophicLevel level in System.Enum.GetValues(typeof(TrophicLevel)))
        {
            snapshot.LevelCounts[level.ToString()] = web.GetSlotsByLevel(level).Count;
        }

        snapshot.HighlightedNiches = web.Slots
            .OrderByDescending(slot => slot.BiomassCapacity)
            .Take(6)
            .Select(slot => $"{slot.Level}: {slot.Description} ({slot.BiomassCapacity.ToString("0.0", CultureInfo.InvariantCulture)} biomass)")
            .ToList();

        return snapshot;
    }

    private static string BuildSummary(EnvironmentSpec spec, EcologyConceptSnapshot snapshot)
    {
        return
            $"Generated a {spec.Biome.ToString().ToLowerInvariant()} ecology with {snapshot.SlotCount} trophic slots, " +
            $"{snapshot.ConnectionCount} feeding links, and a maximum chain length of {snapshot.MaxChainLength}. " +
            $"The current context supports {snapshot.Biomass.ToString("0.0", CultureInfo.InvariantCulture)} total biomass " +
            $"at {snapshot.Productivity.ToString("0.00", CultureInfo.InvariantCulture)} productivity.";
    }

    private static List<ConceptMetric> BuildMetrics(EcologyConceptSnapshot snapshot)
    {
        return new List<ConceptMetric>
        {
            new ConceptMetric { Label = "Productivity", Value = snapshot.Productivity, MaxValue = 1.0, DisplayText = snapshot.Productivity.ToString("0.00", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Biomass", Value = snapshot.Biomass, MaxValue = System.Math.Max(1000.0, snapshot.Biomass), DisplayText = snapshot.Biomass.ToString("0.0", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Complexity", Value = snapshot.Complexity, MaxValue = 1.0, DisplayText = snapshot.Complexity.ToString("0.00", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Stability", Value = snapshot.Stability, MaxValue = 1.0, DisplayText = snapshot.Stability.ToString("0.00", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Food-chain length", Value = snapshot.MaxChainLength, MaxValue = 8.0, DisplayText = snapshot.MaxChainLength.ToString(CultureInfo.InvariantCulture) },
        };
    }

    private static List<ConceptSection> BuildSections(EnvironmentSpec spec, EcologyConceptSnapshot snapshot)
    {
        return new List<ConceptSection>
        {
            new ConceptSection
            {
                Title = "Environment",
                Items = new List<string>
                {
                    $"Biome: {spec.Biome}",
                    $"Temperature band: {spec.TemperatureMin.ToString("0", CultureInfo.InvariantCulture)} K to {spec.TemperatureMax.ToString("0", CultureInfo.InvariantCulture)} K",
                    $"Water {spec.WaterAvailability.ToString("0.00", CultureInfo.InvariantCulture)}, oxygen {spec.OxygenLevel.ToString("0.00", CultureInfo.InvariantCulture)}, radiation {spec.RadiationLevel.ToString("0.00", CultureInfo.InvariantCulture)}",
                },
            },
            new ConceptSection
            {
                Title = "Trophic profile",
                Items = snapshot.LevelCounts.Select(entry => $"{entry.Key}: {entry.Value}").ToList(),
            },
            new ConceptSection
            {
                Title = "Dominant niches",
                Items = snapshot.HighlightedNiches,
            },
        };
    }

    private static BiomeType MapBiome(string biomeName)
    {
        string normalized = biomeName.Trim().ToLowerInvariant();
        return normalized switch
        {
            "oceanic" => BiomeType.Aquatic,
            "desert" => BiomeType.Desert,
            "forest" => BiomeType.Forest,
            "grassland" => BiomeType.Grassland,
            "tundra" => BiomeType.Tundra,
            "volcanic" => BiomeType.Volcanic,
            "subterranean" => BiomeType.Subterranean,
            "wetland" => BiomeType.Wetland,
            "reef" => BiomeType.Reef,
            _ => BiomeType.Grassland,
        };
    }

    private static float Clamp01(float value)
    {
        return System.Math.Clamp(value, 0.0f, 1.0f);
    }
}
