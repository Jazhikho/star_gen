using System.Collections.Generic;
using System.Globalization;
using StarGen.Concepts.ReligionGenerator;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Religion;
using StarGen.Domain.Population;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the religion concept fold-in.
/// </summary>
public sealed class ReligionAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the religion presenter.
    /// </summary>
    public ReligionAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Religion,
            DisplayName = "Religion",
            Summary = "Generate belief systems, authority structures, ritual emphasis, and religious landscapes seeded from culture-adjacent context.",
            AcceptedContext = "Population, government, settlement, or manual cultural profile",
        };
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return ConceptResultFactory.Run(request);
    }

    private static ReligionParams BuildParams(ConceptContextSnapshot context)
    {
        ReligionParams parameters = new()
        {
            Seed = context.Seed,
            Subsistence = MapSubsistence(context),
            SocialOrg = MapSocialOrganization(context),
            Settlement = MapSettlement(context),
            Environment = MapEnvironment(context),
            ExternalThreat = MapThreat(context),
            Isolation = MapIsolation(context),
            PoliticalPower = MapPoliticalPower(context),
            WritingSystem = MapWriting(context),
            PriorTraditions = context.Population > 10000000 ? "syncretic" : "indigenous_only",
            GenderSystem = MapGenderSystem(context),
            KinshipStructure = context.Population > 5000000 ? "lineage_corporate" : "extended_clan",
        };
        return parameters;
    }

    private static ReligionConceptSnapshot BuildSnapshot(ReligionResult result)
    {
        return new ReligionConceptSnapshot
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
    }

    private static List<ConceptMetric> BuildMetrics(ReligionResult result)
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

    private static List<ConceptSection> BuildSections(ReligionConceptSnapshot snapshot, ReligionResult result)
    {
        List<string> landscape = new();
        foreach (LandscapeEntry rival in result.Landscape.Rivals)
        {
            landscape.Add($"{rival.Name}: {rival.Desc}");
        }

        foreach (LandscapeEntry dynamicEntry in result.Landscape.Dynamics)
        {
            landscape.Add($"{dynamicEntry.Name}: {dynamicEntry.Desc}");
        }

        List<string> doctrinalNotes = new()
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

    private static string MapSubsistence(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Desert", System.StringComparison.OrdinalIgnoreCase))
        {
            return "pastoral";
        }

        if (context.DominantBiome.Equals("Oceanic", System.StringComparison.OrdinalIgnoreCase))
        {
            return "maritime";
        }

        if (context.Population > 8000000)
        {
            return "urban_trade";
        }

        return "agricultural";
    }

    private static string MapSocialOrganization(ConceptContextSnapshot context)
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

    private static string MapSettlement(ConceptContextSnapshot context)
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

    private static string MapEnvironment(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Desert", System.StringComparison.OrdinalIgnoreCase))
        {
            return "arid_scarce";
        }

        if (context.DominantBiome.Equals("Oceanic", System.StringComparison.OrdinalIgnoreCase))
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

    private static string MapThreat(ConceptContextSnapshot context)
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

    private static string MapIsolation(ConceptContextSnapshot context)
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

    private static string MapPoliticalPower(ConceptContextSnapshot context)
    {
        if (!context.Regime.HasValue)
        {
            return "intertwined";
        }

        return context.Regime.Value switch
        {
            GovernmentType.Regime.Theocracy => "theocratic",
            GovernmentType.Regime.MassDemocracy => "distributed",
            GovernmentType.Regime.Constitutional => "separate",
            _ => "intertwined",
        };
    }

    private static string MapWriting(ConceptContextSnapshot context)
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

    private static string MapGenderSystem(ConceptContextSnapshot context)
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

    private static List<string> MergeLists(List<string> left, List<string> right)
    {
        List<string> merged = new();
        merged.AddRange(left);
        merged.AddRange(right);
        return merged;
    }
}
