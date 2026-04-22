using System.Collections.Generic;
using System.Text;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Source citation metadata used by planetary-generation help surfaces and tests.
/// </summary>
public sealed class PlanetaryScienceSource
{
    public string Id { get; }
    public string Citation { get; }
    public string Url { get; }

    public PlanetaryScienceSource(string id, string citation, string url)
    {
        Id = id;
        Citation = citation;
        Url = url;
    }
}

/// <summary>
/// Parameter-specific planetary-science help text and source linkage.
/// </summary>
public sealed class PlanetaryScienceParameterReference
{
    public string ParameterId { get; }
    public string TooltipSummary { get; }
    public IReadOnlyList<string> SourceIds { get; }

    public PlanetaryScienceParameterReference(string parameterId, string tooltipSummary, IReadOnlyList<string> sourceIds)
    {
        ParameterId = parameterId;
        TooltipSummary = tooltipSummary;
        SourceIds = sourceIds;
    }
}

/// <summary>
/// Canonical science notes and source registry for planetary-generation controls.
/// </summary>
public static class PlanetaryScienceReferenceCatalog
{
    private static readonly Dictionary<string, PlanetaryScienceSource> Sources = new()
    {
        ["chenkipping2017"] = new PlanetaryScienceSource(
            "chenkipping2017",
            "Chen and Kipping (2017), probabilistic mass-radius regimes for planets.",
            "https://arxiv.org/abs/1603.08614"),
        ["otegi2020"] = new PlanetaryScienceSource(
            "otegi2020",
            "Otegi, Bouchy, and Helled (2020), separate rocky and volatile-rich mass-radius relations below 120 Earth masses.",
            "https://arxiv.org/abs/1911.04745"),
        ["fulton2017"] = new PlanetaryScienceSource(
            "fulton2017",
            "Fulton et al. (2017), the small-planet radius gap.",
            "https://ui.adsabs.harvard.edu/abs/2017AJ....154..109F/abstract"),
        ["kasting1993"] = new PlanetaryScienceSource(
            "kasting1993",
            "Kasting, Whitmire, and Reynolds (1993), classical conservative habitable-zone climate limits.",
            "https://doi.org/10.1006/icar.1993.1010"),
        ["kopparapu2013"] = new PlanetaryScienceSource(
            "kopparapu2013",
            "Kopparapu et al. (2013), updated main-sequence habitable-zone limits with improved H2O and CO2 absorption.",
            "https://arxiv.org/abs/1301.6674"),
        ["kopparapu2014"] = new PlanetaryScienceSource(
            "kopparapu2014",
            "Kopparapu et al. (2014), habitable-zone dependence on planetary mass.",
            "https://arxiv.org/abs/1404.5292"),
        ["owenwu2017"] = new PlanetaryScienceSource(
            "owenwu2017",
            "Owen and Wu (2017), photoevaporation and the evaporation valley.",
            "https://arxiv.org/abs/1705.10810"),
        ["ginzburg2018"] = new PlanetaryScienceSource(
            "ginzburg2018",
            "Ginzburg, Schlichting, and Sari (2018), core-powered mass loss.",
            "https://arxiv.org/abs/1708.01621"),
        ["mordasini2007"] = new PlanetaryScienceSource(
            "mordasini2007",
            "Mordasini, Alibert, Benz, and Naef (2007), giant-planet formation by core accretion.",
            "https://arxiv.org/abs/0710.5667"),
        ["lambrechtsjohansen2012"] = new PlanetaryScienceSource(
            "lambrechtsjohansen2012",
            "Lambrechts and Johansen (2012), rapid giant-core growth by pebble accretion.",
            "https://arxiv.org/abs/1205.3030"),
        ["fischervalenti2005"] = new PlanetaryScienceSource(
            "fischervalenti2005",
            "Fischer and Valenti (2005), planet-metallicity correlation.",
            "https://www.astro.ucla.edu/~aes/AST278/reading/Fischer_2005_ApJ_622_1102.pdf"),
        ["canupward2006"] = new PlanetaryScienceSource(
            "canupward2006",
            "Canup and Ward (2006), regular satellite formation in circumplanetary disks.",
            "https://www.nature.com/articles/nature04860"),
        ["jewitthaghighipour2007"] = new PlanetaryScienceSource(
            "jewitthaghighipour2007",
            "Jewitt and Haghighipour (2007), irregular and captured satellites.",
            "https://arxiv.org/abs/astro-ph/0703059"),
        ["demeocarry2013"] = new PlanetaryScienceSource(
            "demeocarry2013",
            "DeMeo and Carry (2013), asteroid compositional structure.",
            "https://arxiv.org/abs/1307.2424"),
        ["lamy2004"] = new PlanetaryScienceSource(
            "lamy2004",
            "Lamy, Toth, Fernandez, and Weaver (2004), comet nucleus sizes, albedos, and colors.",
            "https://physics.ucf.edu/~yfernandez/papers/comets2chapter/comets2reprint.pdf"),
        ["mroz2020"] = new PlanetaryScienceSource(
            "mroz2020",
            "Mroz et al. (2020), free-floating or wide-orbit planet constraints.",
            "https://arxiv.org/abs/2009.12377"),
    };

    private static readonly List<PlanetaryScienceParameterReference> ParameterReferences = new()
    {
        new PlanetaryScienceParameterReference(
            "planet_mass_radius_model",
            "This picks the mass-radius curve used to turn planet mass into planet size.\nChen-Kipping is the broad all-planet default.\nOtegi splits smaller planets into rocky and volatile-rich branches.\nChanging this mainly shifts radius, density, gravity, and atmosphere retention.",
            new[] { "chenkipping2017", "otegi2020" }),
        new PlanetaryScienceParameterReference(
            "planet_envelope_loss_model",
            "This picks the gas-loss model for hot close-in planets.\nPhotoevaporation uses high-energy starlight as the main stripping engine.\nCore-powered uses the young planet's own cooling heat.\nAuto blends the two ideas as the default.\nChanging this shifts how often hot planets stay puffy or end up as stripped cores.",
            new[] { "fulton2017", "owenwu2017", "ginzburg2018" }),
        new PlanetaryScienceParameterReference(
            "planet_habitable_zone_model",
            "This picks the circumstellar habitable-zone reference band used when the generator scores orbit placement and downstream environment context.\nKasting 1993 keeps the older conservative moist-greenhouse and maximum-greenhouse limits.\nKopparapu 2013 Conservative uses the updated cloud-free climate coefficients for the inner and outer classical habitable zone.\nKopparapu 2013 Optimistic widens the band to the empirical Recent Venus and Early Mars limits.\nChanging this shifts which orbital slots count as more temperate or life-friendly without making habitability a simple yes or no switch.",
            new[] { "kasting1993", "kopparapu2013", "kopparapu2014" }),
        new PlanetaryScienceParameterReference(
            "planet_gas_giant_formation_model",
            "This picks the giant-planet growth model used for system weighting.\nCore Accretion follows the standard heavy-core-first picture.\nPebble-assisted follows the faster pebble-growth picture.\nMixed stays between the two.\nChanging this mainly shifts how easily systems grow Neptune-to-Jupiter scale planets.",
            new[] { "mordasini2007", "lambrechtsjohansen2012" }),
        new PlanetaryScienceParameterReference(
            "planet_metallicity_coupling_strength",
            "Metallicity means how rich the system is in heavy elements like iron, silicon, and oxygen.\nWeak keeps the metal-rich giant-planet effect mild.\nObserved Default uses the baseline empirical trend.\nStrong makes metallicity matter more than that baseline.\nChanging this mainly shifts giant-planet odds and outer-system architecture.",
            new[] { "fischervalenti2005" }),
        new PlanetaryScienceParameterReference(
            "planet_rogue_planet_allowance",
            "Rogue planets are planets that got kicked out or never stayed in a stable orbit.\nOff favors tidier bound architectures.\nRare uses a low free-floating baseline.\nStandard allows more disturbed low-mass outcomes and ejection pressure.\nThis is an upstream disruption setting, not a full rogue-planet population simulator by itself.",
            new[] { "mroz2020" }),
        new PlanetaryScienceParameterReference(
            "planet_moon_formation_bias",
            "Regular moons form in a disk around a giant planet.\nCaptured moons are outsiders that got trapped later.\nRegular-disk favored leans toward orderly moon families.\nCaptured-rich leans toward irregular outer moons.\nChanging this mainly affects moon counts, spacing, and how tidy large moon systems look.",
            new[] { "canupward2006", "jewitthaghighipour2007" }),
        new PlanetaryScienceParameterReference(
            "planet_minor_body_outer_system_bias",
            "This biases cold outer leftovers between rockier belts and icier reservoirs.\nAsteroid-leaning favors drier rocky debris.\nBalanced keeps neither branch dominant.\nComet-leaning favors icy primitive reservoirs.\nChanging this affects volatile delivery, comet supply, and outer-belt composition.",
            new[] { "demeocarry2013", "lamy2004" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "chenkipping2017",
        "otegi2020",
        "fulton2017",
        "kasting1993",
        "kopparapu2013",
        "kopparapu2014",
        "owenwu2017",
        "ginzburg2018",
        "mordasini2007",
        "lambrechtsjohansen2012",
        "fischervalenti2005",
        "canupward2006",
        "jewitthaghighipour2007",
        "demeocarry2013",
        "lamy2004",
        "mroz2020",
    };

    public static string GetTooltipSummary(string parameterId)
    {
        PlanetaryScienceParameterReference? reference = FindParameterReference(parameterId);
        return reference?.TooltipSummary ?? string.Empty;
    }

    public static IReadOnlyList<PlanetaryScienceParameterReference> GetParameterReferences()
    {
        return ParameterReferences;
    }

    /// <summary>
    /// Returns the source identifiers backing a planetary parameter tooltip.
    /// </summary>
    public static IReadOnlyList<string> GetParameterSourceIds(string parameterId)
    {
        PlanetaryScienceParameterReference? reference = FindParameterReference(parameterId);
        if (reference == null)
        {
            return System.Array.Empty<string>();
        }

        return reference.SourceIds;
    }

    /// <summary>
    /// Returns the ordered source identifiers surfaced in the help panel.
    /// </summary>
    public static IReadOnlyList<string> GetHelpPanelSourceIds()
    {
        return PanelSourceIds;
    }

    public static PlanetaryScienceSource? GetSource(string sourceId)
    {
        if (Sources.ContainsKey(sourceId))
        {
            return Sources[sourceId];
        }

        return null;
    }

    public static string BuildHelpPanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b][color=#f0c46a]Planetary formation controls[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]These settings shape the whole planet population in a galaxy or system. They do not directly hand-build one planet. Instead, they change the odds of rocky worlds, water-rich worlds, puffier mini-Neptunes, gas giants, stripped cores, moon styles, and outer small-body leftovers.[/color]");
        builder.AppendLine();

        AppendGuideSection(
            builder,
            "Mass-Radius Model",
            "This is the rule that connects a planet's broad class to its size and density.",
            "Chen-Kipping is the broad empirical default across rocky planets, Neptunes, and giants. Otegi adds a separate rocky branch and a separate volatile-rich branch for smaller planets, so transition worlds split more clearly. In practice, this changes planet radius, density, gravity, and how easily a planet keeps gas.");

        AppendGuideSection(
            builder,
            "Envelope Loss",
            "Some close-in planets start with more gas than they can keep.",
            "Photoevaporation makes harsh high-energy starlight do most of the stripping. Core-powered lets the hot young planet itself drive some gas away as it cools. Auto blends both ideas. In practice, this changes whether hot close-in worlds stay puffy or end up as bare or thin-atmosphere cores.");

        AppendGuideSection(
            builder,
            "Habitable Zone Model",
            "This is the academic climate-model family StarGen uses for the classical circumstellar liquid-water reference band.",
            "Kasting 1993 keeps the older conservative band. Kopparapu 2013 Conservative updates the inner and outer limits with newer absorption data. Kopparapu 2013 Optimistic widens the band to the empirical Recent Venus and Early Mars markers. In practice, this shifts which orbits get higher temperate or biosphere-friendly weighting, but it still remains one input among many.");

        AppendGuideSection(
            builder,
            "Gas-Giant Formation",
            "This is the growth style the generator assumes for giant planets.",
            "Core Accretion follows the standard heavy-core-first picture. Pebble-assisted uses fast pebble growth to help giant cores form sooner. Mixed sits between them. In practice, this changes how often systems cross the line from Neptune-scale planets into true giants.");

        AppendGuideSection(
            builder,
            "Metallicity Coupling",
            "Metallicity is how rich the star-forming material is in heavy elements that help build dust, rock, and cores.",
            "Observed Default uses the baseline giant-planet metallicity trend. Weak softens that trend. Strong sharpens it. In practice, this changes giant-planet odds and how much metal-rich systems build heavier outer architectures.");

        AppendGuideSection(
            builder,
            "Rogue Allowance",
            "Rogue planets are worlds that do not stay in a normal stable orbit around a star.",
            "Off favors tidy bound systems. Rare keeps a low ejection baseline close to current free-floating constraints. Standard assumes more scattering and ejection pressure. In practice, this changes how much the generator favors orderly systems versus disturbed low-mass outcomes. It is not a full stand-alone rogue-population simulator.");

        AppendGuideSection(
            builder,
            "Moon Bias",
            "Regular moons grow with the planet in a disk. Captured moons are outsiders that get trapped later.",
            "Regular-disk favored leans toward tidy moon families with shared origins. Captured-rich leans toward more irregular outer moons. Mixed stays between them. In practice, this changes moon counts, spacing, and whether the system looks orderly or capture-heavy.");

        AppendGuideSection(
            builder,
            "Outer Small-Body Bias",
            "Cold leftovers can lean more rocky like asteroids or more icy like comet reservoirs.",
            "Asteroid-leaning favors drier rocky debris. Comet-leaning favors icy primitive reservoirs. Balanced stays in the middle. In practice, this changes volatile delivery, comet supply, and the composition of cold belts.");

        builder.AppendLine("[b][color=#f0c46a]What StarGen actually does with this[/color][/b]");
        builder.AppendLine("1. It builds one aggregate system state from the stars plus these formation assumptions.");
        builder.AppendLine("2. It uses that state to shift broad planet-class odds before it resolves each planet's detailed properties.");
        builder.AppendLine("3. It resolves planet size and density from the selected mass-radius model unless the user directly overrides them.");
        builder.AppendLine("4. It keeps the current generation spine intact instead of replacing it with a full formation simulation.");
        builder.AppendLine();

        builder.AppendLine("[b][color=#f0c46a]Sources[/color][/b]");
        foreach (string sourceId in PanelSourceIds)
        {
            PlanetaryScienceSource source = Sources[sourceId];
            builder.Append("- ");
            if (!string.IsNullOrWhiteSpace(source.Url))
            {
                builder.Append("[url=");
                builder.Append(source.Url);
                builder.Append("]");
                builder.Append(source.Citation);
                builder.Append("[/url]");
            }
            else
            {
                builder.Append(source.Citation);
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendGuideSection(StringBuilder builder, string title, string meaning, string effect)
    {
        builder.Append("[b]");
        builder.Append(title);
        builder.AppendLine("[/b]");
        builder.Append("[color=#9cc4ff]What it means:[/color] ");
        builder.AppendLine(meaning);
        builder.Append("[color=#9cc4ff]What changing it does:[/color] ");
        builder.AppendLine(effect);
        builder.AppendLine();
    }

    private static PlanetaryScienceParameterReference? FindParameterReference(string parameterId)
    {
        foreach (PlanetaryScienceParameterReference reference in ParameterReferences)
        {
            if (reference.ParameterId == parameterId)
            {
                return reference;
            }
        }

        return null;
    }
}
