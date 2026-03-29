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
        ["fulton2017"] = new PlanetaryScienceSource(
            "fulton2017",
            "Fulton et al. (2017), the small-planet radius gap.",
            "https://ui.adsabs.harvard.edu/abs/2017AJ....154..109F/abstract"),
        ["cumming2008"] = new PlanetaryScienceSource(
            "cumming2008",
            "Cumming et al. (2008), giant-planet occurrence and orbital distribution.",
            "https://arxiv.org/abs/0803.3357"),
        ["wright2012"] = new PlanetaryScienceSource(
            "wright2012",
            "Wright et al. (2012), hot-Jupiter occurrence.",
            "https://arxiv.org/abs/1205.2273"),
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
        ["mroz2020"] = new PlanetaryScienceSource(
            "mroz2020",
            "Mroz et al. (2020), free-floating or wide-orbit planet constraints.",
            "https://arxiv.org/abs/2009.12377"),
        ["starplanetsmd"] = new PlanetaryScienceSource(
            "starplanetsmd",
            "StarGen planets.md retrofit notes, deterministic surrogate formation model.",
            string.Empty),
    };

    private static readonly List<PlanetaryScienceParameterReference> ParameterReferences = new()
    {
        new PlanetaryScienceParameterReference(
            "planet_mass_radius_model",
            "This chooses the rule used to turn a planet class into a size and density.\nLegacy stays close to older StarGen behavior.\nChen-Kipping leans harder on observed planet size bands.",
            new[] { "chenkipping2017", "starplanetsmd" }),
        new PlanetaryScienceParameterReference(
            "planet_envelope_loss_model",
            "This controls how close-in planets lose thick gas envelopes.\nPhotoevaporation means harsh starlight strips gas away.\nCore-powered means a hot young planet can blow off some of its own gas.\nChanging this shifts how often close-in worlds stay puffy versus ending up as denser stripped cores.",
            new[] { "fulton2017", "starplanetsmd" }),
        new PlanetaryScienceParameterReference(
            "planet_gas_giant_formation_model",
            "This decides how easily a system grows giant planets.\nCore accretion grows them more slowly from heavy cores.\nPebble-assisted lets small solids build giants faster.\nMixed stays between those two ideas.",
            new[] { "cumming2008", "wright2012", "starplanetsmd" }),
        new PlanetaryScienceParameterReference(
            "planet_metallicity_coupling_strength",
            "Metallicity means how rich the system is in heavy elements like iron, silicon, and oxygen.\nStronger coupling means metal-rich systems make giant planets more easily.\nWeaker coupling keeps metallicity important, but less dominant.",
            new[] { "fischervalenti2005", "starplanetsmd" }),
        new PlanetaryScienceParameterReference(
            "planet_rogue_planet_allowance",
            "Rogue planets are planets that got kicked out or never stayed in a normal orbit.\nHigher allowance means the generator assumes more ejection pressure, so tidy bound systems become a little less common and stripped or disturbed outcomes become a little more common.",
            new[] { "mroz2020", "starplanetsmd" }),
        new PlanetaryScienceParameterReference(
            "planet_moon_formation_bias",
            "Regular moons form in a disk around a giant planet.\nCaptured moons are outsiders that got trapped later.\nChanging this shifts whether big planets tend to have orderly moon families or more irregular captured companions.",
            new[] { "canupward2006", "jewitthaghighipour2007", "starplanetsmd" }),
        new PlanetaryScienceParameterReference(
            "planet_minor_body_outer_system_bias",
            "This controls whether leftover outer-system debris leans more rocky like asteroids or more icy like comet reservoirs.\nChanging it shifts the small-body feel of the colder system outskirts.",
            new[] { "demeocarry2013", "starplanetsmd" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "chenkipping2017",
        "fulton2017",
        "cumming2008",
        "wright2012",
        "fischervalenti2005",
        "canupward2006",
        "jewitthaghighipour2007",
        "demeocarry2013",
        "mroz2020",
        "starplanetsmd",
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
            "Legacy stays closer to older StarGen outputs. Chen-Kipping pulls the sizes closer to observed planet-size bands, especially around the sub-Neptune and giant-planet ranges.");

        AppendGuideSection(
            builder,
            "Envelope Loss",
            "Some close-in planets start with more gas than they can keep.",
            "Photoevaporation makes harsh starlight do most of the stripping. Core-powered lets the hot planet itself drive some gas away. Changing this mostly affects whether close-in volatile-rich planets stay puffy or end up as denser stripped worlds.");

        AppendGuideSection(
            builder,
            "Gas-Giant Formation",
            "This is the growth style the generator assumes for giant planets.",
            "Core accretion slows giant growth and usually makes giants a bit less common. Pebble-assisted makes it easier for systems with enough material to grow bigger planets sooner. Mixed sits in the middle.");

        AppendGuideSection(
            builder,
            "Metallicity Coupling",
            "Metallicity is how rich the star-forming material is in heavy elements that help build dust, rock, and cores.",
            "A stronger coupling makes metal-rich systems much better at growing giant planets. A weaker coupling still allows the effect, but it changes outcomes less sharply.");

        AppendGuideSection(
            builder,
            "Rogue Allowance",
            "Rogue planets are worlds that do not stay in a normal stable orbit around a star.",
            "Higher allowance means the generator assumes more ejection and disturbance. That does not fill your system with free-floaters, but it does make orderly bound layouts a little less favored.");

        AppendGuideSection(
            builder,
            "Moon Bias",
            "Regular moons grow with the planet in a disk. Captured moons are outsiders that get trapped later.",
            "Changing this shifts whether giant planets lean toward tidy moon families or more irregular captured companions.");

        AppendGuideSection(
            builder,
            "Outer Small-Body Bias",
            "Cold leftovers can lean more rocky like asteroids or more icy like comet reservoirs.",
            "Changing this alters the small-body flavor of outer systems without rewriting the whole planet generator.");

        builder.AppendLine("[b][color=#f0c46a]What StarGen actually does with this[/color][/b]");
        builder.AppendLine("1. It builds one aggregate system state from the stars plus these formation assumptions.");
        builder.AppendLine("2. It uses that state to shift broad planet-class odds before it resolves each planet's detailed properties.");
        builder.AppendLine("3. It keeps the current generation spine intact instead of replacing it with a full formation simulation.");
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
