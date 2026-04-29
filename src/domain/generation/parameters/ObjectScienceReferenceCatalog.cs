using System.Collections.Generic;
using System.Text;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Reviewed-source registry and help content for Object Studio science notes.
/// </summary>
public sealed class ObjectScienceSource
{
    public string Id { get; }
    public string Citation { get; }
    public string Url { get; }

    public ObjectScienceSource(string id, string citation, string url)
    {
        Id = id;
        Citation = citation;
        Url = url;
    }
}

/// <summary>
/// Source-backed issue note shown when direct object settings pull against each other.
/// </summary>
public sealed class ObjectScienceIssueNote
{
    public string Text { get; }
    public string Tooltip { get; }

    public ObjectScienceIssueNote(string text, string tooltip)
    {
        Text = text;
        Tooltip = tooltip;
    }
}

/// <summary>
/// Object-studio help and direct-setting conflict notes.
/// </summary>
public static class ObjectScienceReferenceCatalog
{
    private static readonly Dictionary<string, ObjectScienceSource> Sources = new()
    {
        ["wordsworthkreidberg2022"] = new ObjectScienceSource(
            "wordsworthkreidberg2022",
            "Wordsworth and Kreidberg (2022), rocky planet atmospheres vary with escape, stellar flux, and volatile history, and airless rocky worlds are a real end state.",
            "https://arxiv.org/abs/2112.04663"),
        ["kopparapu2014"] = new ObjectScienceSource(
            "kopparapu2014",
            "Kopparapu et al. (2014), stable surface habitability depends on stellar flux and planetary mass rather than one universal Earth-like band.",
            "https://arxiv.org/abs/1404.5292"),
        ["owenwu2017"] = new ObjectScienceSource(
            "owenwu2017",
            "Owen and Wu (2017), strong irradiation can strip close-in planets down to denser bare or thin-envelope cores.",
            "https://arxiv.org/abs/1705.10810"),
        ["balbi2023"] = new ObjectScienceSource(
            "balbi2023",
            "Balbi and Frank (2023), oxygen-rich conditions may support technological civilizations when the atmospheric context makes biological oxygen plausible.",
            "https://arxiv.org/abs/2308.01160"),
        ["ronnet2020"] = new ObjectScienceSource(
            "ronnet2020",
            "Ronnet and Johansen (2020), giant-planet moon systems can form through circumplanetary-disk capture, ablation, and pebble-accretion pathways rather than arbitrary counts.",
            "https://doi.org/10.1051/0004-6361/201936804"),
        ["sasaki2010"] = new ObjectScienceSource(
            "sasaki2010",
            "Sasaki, Stewart, and Ida (2010), Jovian and Saturnian regular-satellite systems can have different architectures.",
            "https://doi.org/10.1088/0004-637X/714/2/1052"),
        ["szulagyi2018"] = new ObjectScienceSource(
            "szulagyi2018",
            "Szulagyi, Cilibrasi, and Mayer (2018), icy moons can form in circumplanetary disks around Uranus- and Neptune-like planets.",
            "https://doi.org/10.3847/2041-8213/aaeed6"),
        ["hellerbarnes2013"] = new ObjectScienceSource(
            "hellerbarnes2013",
            "Heller and Barnes (2013), moon habitability depends on illumination and tidal-heating limits rather than moon count alone.",
            "https://arxiv.org/abs/1209.5323"),
        ["demeocarry2014"] = new ObjectScienceSource(
            "demeocarry2014",
            "DeMeo and Carry (2014), asteroid composition changes with heliocentric distance and preserves different inner versus outer-system materials.",
            "https://arxiv.org/abs/1310.3846"),
        ["lamy2004"] = new ObjectScienceSource(
            "lamy2004",
            "Lamy et al. (2004), comet nuclei are dark volatile-rich bodies distinct from ordinary asteroids.",
            "https://ui.adsabs.harvard.edu/abs/2004come.book..223L/abstract"),
        ["baueretal2017"] = new ObjectScienceSource(
            "baueretal2017",
            "Bauer et al. (2017), NEOWISE comet nucleus sizes and activity fractions support smaller Jupiter-family comet nuclei than StarGen's old wide range.",
            "Sources/Texts/BauerEtAl2017.txt"),
        ["escuderoetal2023"] = new ObjectScienceSource(
            "escuderoetal2023",
            "Escudero et al. (2023), protected subsurface habitability depends on water plus chemical energy and rock-fluid interfaces.",
            "Sources/Texts/EscuderoEtAl2023.txt"),
    };

    public static ObjectScienceSource? GetSource(string sourceId)
    {
        if (Sources.TryGetValue(sourceId, out ObjectScienceSource? source))
        {
            return source;
        }

        return null;
    }

    public static string BuildHelpPanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b][color=#f0c46a]Object Studio[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]Object Studio is the direct-authoring surface. It lets you lock in one specific star, planet, asteroid, or comet instead of only setting aggregate galaxy or system priors.[/color]");
        builder.AppendLine();
        builder.AppendLine("[b]What this means in practice[/b]");
        builder.AppendLine("[color=#9cc4ff]Direct locks win:[/color] If you force a setting here, StarGen will honor that direct object target even when the combination would be unusual in a normal downstream roll.");
        builder.AppendLine("[color=#9cc4ff]Conflict notes matter:[/color] When two direct settings pull against each other, the summary notes call that out and explain why the combination is scientifically tense.");
        builder.AppendLine("[color=#9cc4ff]Life controls are local:[/color] The life controls here apply only to this one planet. They do not replace the aggregate life assumptions used by Galaxy Studio or System Studio.");
        builder.AppendLine();
        builder.AppendLine("[b]Important direct-setting connections[/b]");
        builder.AppendLine("- [color=#9cc4ff]Airless or stripped planets versus oceans:[/color] Surface oceans and dense weather work best with retained atmospheres and the right stellar flux. If you force an airless or stripped planet and still ask for ocean-heavy conditions, the lock wins, but the combination becomes physically tense. [Wordsworth & Kreidberg 2022; Kopparapu et al. 2014]");
        builder.AppendLine("- [color=#9cc4ff]Stripped worlds versus thick pressure:[/color] Strong atmospheric loss and dense retained surface pressure push in opposite directions. If you ask for both, StarGen still builds the target, but the note warns that the choice is fighting the modeled loss physics. [Owen & Wu 2017; Wordsworth & Kreidberg 2022]");
        builder.AppendLine("- [color=#9cc4ff]Moon targets versus host class:[/color] The moon target count is not guaranteed. StarGen still caps the final result by host size because small rocky worlds do not support giant-planet moon systems. [Ronnet & Johansen 2020; Sasaki et al. 2010; Szulagyi et al. 2018; Heller & Barnes 2013]");
        builder.AppendLine("- [color=#9cc4ff]Civilization bottlenecks versus airless worlds:[/color] If you choose the Technosphere Oxygen Bottleneck model, airless, heavily stripped, or abiotic-oxygen-risk worlds become poorer civilization candidates even if other life settings stay permissive. [Balbi & Frank 2023]");
        builder.AppendLine();
        builder.AppendLine("[b]Direct object families[/b]");
        builder.AppendLine("- [color=#9cc4ff]Asteroids:[/color] Orbit band, density, and albedo help you target inner rocky fragments, main-belt bodies, or darker outer primitive material. [DeMeo & Carry 2014]");
        builder.AppendLine("- [color=#9cc4ff]Comets:[/color] Family, activity, and nucleus model let you target short-period Jupiter-family comets, distant long-period icy bodies, or explicit legacy wide-size behavior without pretending all comets are asteroids with a different label. [Bauer et al. 2017; Lamy et al. 2004]");
        builder.AppendLine("- [color=#9cc4ff]Subsurface oceans:[/color] Protected oceans can support a separate dark-biosphere proxy, but the stronger model also asks for chemical energy and rock-fluid interface support. [Escudero et al. 2023; Heller & Barnes 2013]");
        builder.AppendLine();
        builder.AppendLine("[b][color=#f0c46a]Sources[/color][/b]");
        foreach (ObjectScienceSource source in Sources.Values)
        {
            builder.Append("- [url=");
            builder.Append(source.Url);
            builder.Append("]");
            builder.Append(source.Citation);
            builder.AppendLine("[/url]");
        }

        return builder.ToString().TrimEnd();
    }

    public static IReadOnlyList<ObjectScienceIssueNote> BuildPlanetIssueNotes(
        bool atmosphereForcedOff,
        PlanetEnvelopeOverride envelopeOverride,
        PlanetHydrosphereTendency hydrosphereTendency,
        int oceanCoverageId,
        int surfacePressureId,
        PlanetClassBias classBias,
        GenerationUseCaseSettings.CivilizationModelType civilizationModel)
    {
        List<ObjectScienceIssueNote> notes = new();

        bool wantsSurfaceWater = hydrosphereTendency == PlanetHydrosphereTendency.Mixed
            || hydrosphereTendency == PlanetHydrosphereTendency.Oceanic
            || oceanCoverageId >= 1;
        bool wantsDenseAir = surfacePressureId >= 1;

        if (atmosphereForcedOff && (wantsSurfaceWater || wantsDenseAir))
        {
            notes.Add(new ObjectScienceIssueNote(
                "Airless planets are a poor match for dense surface pressure or open-water targets, so this lock set is scientifically tense.",
                "You forced the planet toward no atmosphere while also asking for surface pressure or open-water behavior. StarGen will still honor the direct lock, but stable surface oceans and dense weather are not a natural fit for airless rocky worlds. Supported by Wordsworth and Kreidberg (2022) plus Kopparapu et al. (2014)."));
        }

        if (envelopeOverride == PlanetEnvelopeOverride.Stripped && (wantsSurfaceWater || wantsDenseAir))
        {
            notes.Add(new ObjectScienceIssueNote(
                "A stripped-envelope target fights against thick air or ocean-heavy surface settings.",
                "Stripping means the planet has lost much of its atmosphere. Asking for dense pressure or ocean-heavy conditions at the same time makes the direct settings pull in opposite directions. Supported by Owen and Wu (2017) plus Wordsworth and Kreidberg (2022)."));
        }

        if (classBias == PlanetClassBias.GasGiant && envelopeOverride == PlanetEnvelopeOverride.Stripped)
        {
            notes.Add(new ObjectScienceIssueNote(
                "Gas Giant and Stripped Envelope push in opposite directions, so the result stops behaving like a classic gas giant.",
                "Gas-giant bias asks for a deep gas-rich world, while a stripped-envelope override asks for strong atmospheric loss. StarGen will build a compromise because Object Studio honors direct locks, but the combination is physically self-opposing."));
        }

        if (civilizationModel == GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck
            && (atmosphereForcedOff || envelopeOverride == PlanetEnvelopeOverride.Stripped))
        {
            notes.Add(new ObjectScienceIssueNote(
                "Technosphere Oxygen Bottleneck makes technological civilizations much less likely on an airless or heavily stripped world.",
                "The selected civilization model treats oxygen-rich atmospheres as an important late bottleneck for technological civilizations. An airless or heavily stripped world therefore works against that life setting even if simpler life remains possible. Supported by Balbi and Frank (2023)."));
        }

        return notes;
    }
}
