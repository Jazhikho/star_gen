using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StarGen.Concepts.ReligionGenerator;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Ecology;
using StarGen.Domain.Concepts.Religion;
using StarGen.Domain.Ecology;
using StarGen.Domain.Population;

namespace StarGen.Services.Concepts;

/// <summary>
/// Ecology and religion concept-result builders shared between atlas and persisted state.
/// </summary>
public static partial class ConceptResultFactory
{
    private const string EcologyGeneratorVersion = "atlas-ecology-1.0.0";

    private static ConceptRunResult BuildEcologyResult(ConceptContextSnapshot context)
    {
        EnvironmentSpec spec = BuildEcologySpec(context);
        EcologyWeb web = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
        EcologyConceptSnapshot snapshot = BuildEcologySnapshot(web);

        return new ConceptRunResult
        {
            Title = string.IsNullOrEmpty(context.BodyName)
                ? "Ecology Sandbox"
                : context.BodyName + " Ecology",
            Subtitle = $"Biome: {spec.Biome} | Context: {context.SourceLabel}",
            Summary = BuildEcologySummary(spec, snapshot),
            Metrics = BuildEcologyMetrics(snapshot),
            Sections = BuildEcologySections(spec, snapshot),
            Provenance = new ConceptProvenance
            {
                ConceptId = ConceptKind.Ecology.ToString(),
                Seed = (int)spec.Seed,
                GeneratorVersion = EcologyGeneratorVersion,
                SourceContext = context.SourceLabel,
            },
        };
    }

    private static EnvironmentSpec BuildEcologySpec(ConceptContextSnapshot context)
    {
        float averageTemperature = (float)context.AvgTemperatureK;
        float temperatureRange = 8.0f + ((10 - context.HabitabilityScore) * 1.5f);
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

        return new EnvironmentSpec
        {
            Seed = unchecked((ulong)context.Seed),
            TemperatureMin = temperatureMin,
            TemperatureMax = temperatureMax,
            WaterAvailability = Math.Clamp((float)context.WaterAvailability, 0.0f, 1.0f),
            LightLevel = Math.Clamp(0.55f + (context.HabitabilityScore / 20.0f), 0.0f, 1.0f),
            NutrientLevel = Math.Clamp(0.25f + (context.HabitabilityScore / 14.0f), 0.0f, 1.0f),
            Gravity = (float)Math.Clamp(context.GravityG, 0.2, 3.0),
            RadiationLevel = Math.Clamp((float)context.RadiationLevel, 0.0f, 1.0f),
            OxygenLevel = Math.Clamp((float)context.OxygenLevel, 0.0f, 1.0f),
            SeasonalVariation = Math.Clamp(0.2f + ((10 - context.HabitabilityScore) / 20.0f), 0.0f, 1.0f),
            Biome = MapEcologyBiome(context.DominantBiome),
            GeneratorVersion = EcologyGeneratorVersion,
        };
    }

    private static EcologyConceptSnapshot BuildEcologySnapshot(EcologyWeb web)
    {
        EcologyConceptSnapshot snapshot = new EcologyConceptSnapshot
        {
            SlotCount = web.Slots.Count,
            ConnectionCount = web.Connections.Count,
            Productivity = web.TotalProductivity,
            Biomass = web.GetTotalBiomass(),
            Complexity = web.ComplexityScore,
            Stability = web.StabilityScore,
            MaxChainLength = web.GetMaxChainLength(),
        };

        foreach (TrophicLevel level in Enum.GetValues(typeof(TrophicLevel)))
        {
            snapshot.LevelCounts[FormatIdentifierForDisplay(level.ToString())] = web.GetSlotsByLevel(level).Count;
        }

        snapshot.HighlightedNiches = web.Slots
            .OrderByDescending(slot => slot.BiomassCapacity)
            .Take(6)
            .Select(slot => $"{FormatIdentifierForDisplay(slot.Level.ToString())}: {slot.Description} ({slot.BiomassCapacity.ToString("0.0", CultureInfo.InvariantCulture)} biomass)")
            .ToList();

        return snapshot;
    }

    private static string BuildEcologySummary(EnvironmentSpec spec, EcologyConceptSnapshot snapshot)
    {
        return
            $"Generated a {spec.Biome.ToString().ToLowerInvariant()} ecology with {snapshot.SlotCount} trophic slots, " +
            $"{snapshot.ConnectionCount} feeding links, and a maximum chain length of {snapshot.MaxChainLength}. " +
            $"The current context supports {snapshot.Biomass.ToString("0.0", CultureInfo.InvariantCulture)} total biomass " +
            $"at {snapshot.Productivity.ToString("0.00", CultureInfo.InvariantCulture)} productivity.";
    }

    private static List<ConceptMetric> BuildEcologyMetrics(EcologyConceptSnapshot snapshot)
    {
        return new List<ConceptMetric>
        {
            new ConceptMetric { Label = "Productivity", Value = snapshot.Productivity, MaxValue = 1.0, DisplayText = snapshot.Productivity.ToString("0.00", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Biomass", Value = snapshot.Biomass, MaxValue = Math.Max(1000.0, snapshot.Biomass), DisplayText = snapshot.Biomass.ToString("0.0", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Complexity", Value = snapshot.Complexity, MaxValue = 1.0, DisplayText = snapshot.Complexity.ToString("0.00", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Stability", Value = snapshot.Stability, MaxValue = 1.0, DisplayText = snapshot.Stability.ToString("0.00", CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Food-chain length", Value = snapshot.MaxChainLength, MaxValue = 8.0, DisplayText = snapshot.MaxChainLength.ToString(CultureInfo.InvariantCulture) },
        };
    }

    private static List<ConceptSection> BuildEcologySections(EnvironmentSpec spec, EcologyConceptSnapshot snapshot)
    {
        return new List<ConceptSection>
        {
            new ConceptSection
            {
                Title = "Environment",
                Items = new List<string>
                {
                    $"Biome: {FormatIdentifierForDisplay(spec.Biome.ToString())}",
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

    private static StarGen.Domain.Ecology.BiomeType MapEcologyBiome(string biomeName)
    {
        string normalized = biomeName.Trim().ToLowerInvariant();
        return normalized switch
        {
            "oceanic" => StarGen.Domain.Ecology.BiomeType.Aquatic,
            "desert" => StarGen.Domain.Ecology.BiomeType.Desert,
            "forest" => StarGen.Domain.Ecology.BiomeType.Forest,
            "grassland" => StarGen.Domain.Ecology.BiomeType.Grassland,
            "tundra" => StarGen.Domain.Ecology.BiomeType.Tundra,
            "volcanic" => StarGen.Domain.Ecology.BiomeType.Volcanic,
            "subterranean" => StarGen.Domain.Ecology.BiomeType.Subterranean,
            "wetland" => StarGen.Domain.Ecology.BiomeType.Wetland,
            "reef" => StarGen.Domain.Ecology.BiomeType.Reef,
            _ => StarGen.Domain.Ecology.BiomeType.Grassland,
        };
    }

    private static ConceptRunResult BuildReligionResult(ConceptContextSnapshot context)
    {
        ReligionParams parameters = BuildReligionParams(context);
        ReligionResult result = ReligionGenerator.Generate(parameters);
        ReligionConceptSnapshot snapshot = new ReligionConceptSnapshot
        {
            Deity = result.Deity.Name,
            Cosmology = result.Cosmology.Desc,
            Authority = result.Authority.Name,
            Specialist = result.Specialist.Name,
            Rituals = new List<string>(result.Rituals),
            Ethics = new List<string>(result.Ethics),
            Landscape = new List<string>
            {
                result.Landscape.HegemonyDesc,
            },
        };

        return new ConceptRunResult
        {
            Title = string.IsNullOrEmpty(context.BodyName)
                ? "Religion Sandbox"
                : context.BodyName + " Belief System",
            Subtitle = $"{FormatIdentifierForDisplay(parameters.SocialOrg)} | {FormatIdentifierForDisplay(parameters.Settlement)} | {FormatIdentifierForDisplay(parameters.Environment)}",
            Summary = $"{snapshot.Deity} religion with {snapshot.Specialist.ToLowerInvariant()} leadership, {snapshot.Authority.ToLowerInvariant()} authority, and a {result.Landscape.HegemonyPct.ToString(CultureInfo.InvariantCulture)}% hegemony estimate.",
            Metrics = BuildReligionMetrics(result),
            Sections = BuildReligionSections(snapshot, result),
            Provenance = new ConceptProvenance
            {
                ConceptId = ConceptKind.Religion.ToString(),
                Seed = parameters.Seed,
                GeneratorVersion = ReligionGenerator.GeneratorVersion,
                SourceContext = context.SourceLabel,
            },
        };
    }

    private static ReligionParams BuildReligionParams(ConceptContextSnapshot context)
    {
        return new ReligionParams
        {
            Seed = context.Seed,
            Subsistence = MapReligionSubsistence(context),
            SocialOrg = MapReligionSocialOrganization(context),
            Settlement = MapReligionSettlement(context),
            Environment = MapReligionEnvironment(context),
            ExternalThreat = MapReligionThreat(context),
            Isolation = MapReligionIsolation(context),
            PoliticalPower = MapReligionPoliticalPower(context),
            WritingSystem = MapReligionWriting(context),
            PriorTraditions = context.Population > 10000000 ? "syncretic" : "indigenous_only",
            GenderSystem = MapReligionGenderSystem(context),
            KinshipStructure = context.Population > 5000000 ? "lineage_corporate" : "extended_clan",
        };
    }

    private static List<ConceptMetric> BuildReligionMetrics(ReligionResult result)
    {
        return new List<ConceptMetric>
        {
            new ConceptMetric { Label = "Hegemony", Value = result.Landscape.HegemonyPct, MaxValue = 100.0, DisplayText = result.Landscape.HegemonyPct + "%" },
            new ConceptMetric { Label = "Non-belief", Value = result.Landscape.NonBeliefPct, MaxValue = 100.0, DisplayText = result.Landscape.NonBeliefPct + "%" },
            new ConceptMetric { Label = "Ritual breadth", Value = result.Rituals.Count, MaxValue = 8.0, DisplayText = result.Rituals.Count.ToString(CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Ethical emphases", Value = result.Ethics.Count, MaxValue = 8.0, DisplayText = result.Ethics.Count.ToString(CultureInfo.InvariantCulture) },
            new ConceptMetric { Label = "Rival traditions", Value = result.Landscape.Rivals.Count, MaxValue = 5.0, DisplayText = result.Landscape.Rivals.Count.ToString(CultureInfo.InvariantCulture) },
        };
    }

    private static List<ConceptSection> BuildReligionSections(ReligionConceptSnapshot snapshot, ReligionResult result)
    {
        List<string> landscape = new List<string>();
        foreach (LandscapeEntry rival in result.Landscape.Rivals)
        {
            landscape.Add($"{rival.Name}: {rival.Desc}");
        }

        foreach (LandscapeEntry dynamicEntry in result.Landscape.Dynamics)
        {
            landscape.Add($"{dynamicEntry.Name}: {dynamicEntry.Desc}");
        }

        List<string> doctrinalNotes = new List<string>
        {
            $"Cosmology: {snapshot.Cosmology}",
            $"Afterlife: {result.Afterlife.Name} - {result.Afterlife.Desc}",
            $"Misfortune: {result.Misfortune.Name} - {result.Misfortune.Desc}",
            $"Gender role: {result.GenderRole.Name}",
        };

        return new List<ConceptSection>
        {
            new ConceptSection
            {
                Title = "Structure",
                Items = new List<string>
                {
                    $"Deity frame: {snapshot.Deity}",
                    $"Specialists: {snapshot.Specialist}",
                    $"Authority: {snapshot.Authority}",
                },
            },
            new ConceptSection
            {
                Title = "Doctrine and worldview",
                Items = doctrinalNotes,
            },
            new ConceptSection
            {
                Title = "Rituals and ethics",
                Items = MergeLists(snapshot.Rituals, snapshot.Ethics),
            },
            new ConceptSection
            {
                Title = "Landscape",
                Items = landscape,
            },
        };
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
        if (!context.TechnologyLevel.HasValue)
        {
            return "none";
        }

        if (context.TechnologyLevel.Value >= TechnologyLevel.Level.Industrial)
        {
            return "widespread";
        }

        if (context.TechnologyLevel.Value >= TechnologyLevel.Level.Classical)
        {
            return "limited";
        }

        return "none";
    }

    private static string MapReligionGenderSystem(ConceptContextSnapshot context)
    {
        if (context.Regime.HasValue && context.Regime.Value == GovernmentType.Regime.Theocracy)
        {
            return "patrilineal";
        }

        if (context.Population > 8000000)
        {
            return "bilateral";
        }

        return "dualistic";
    }
}
