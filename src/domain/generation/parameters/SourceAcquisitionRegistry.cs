using System.Collections.Generic;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Acquisition state for a cited source used by science-facing generation controls.
/// </summary>
public enum SourceAcquisitionStatus
{
    LocalSourceNote = 0,
    PendingReacquisition = 1,
    AcquisitionBlocked = 2,
    BackgroundCatalogOnly = 3,
}

/// <summary>
/// Machine-readable acquisition metadata for a cited source.
/// </summary>
public sealed class SourceAcquisitionRecord
{
    public SourceAcquisitionRecord(
        string sourceId,
        SourceAcquisitionStatus status,
        string textNoteResPath,
        string auditNote)
    {
        SourceId = sourceId;
        Status = status;
        TextNoteResPath = textNoteResPath;
        AuditNote = auditNote;
    }

    public string SourceId { get; }

    public SourceAcquisitionStatus Status { get; }

    public string TextNoteResPath { get; }

    public string AuditNote { get; }
}

/// <summary>
/// Tracks whether cited sources are locally acquired, pending reacquisition, or intentionally blocked.
/// </summary>
public static class SourceAcquisitionRegistry
{
    private static readonly Dictionary<string, SourceAcquisitionRecord> Records = new()
    {
        ["park2007"] = Create(
            "park2007",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/ParkEtAl2007.txt",
            "Local source note exists for morphology-density environment context."),
        ["tanaka2004"] = Create(
            "tanaka2004",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/TanakaEtAl2004.txt",
            "Local source note exists for galaxy environment-density context."),
        ["behroozi2019"] = Create(
            "behroozi2019",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Behroozi2019.txt",
            "Local source note exists for halo-mass and galaxy population context."),
        ["bland2016"] = Create(
            "bland2016",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/BlandHawthornGerhard2016.txt",
            "Local reviewed source note exists for Milky-Way structural and kinematic parameter grounding."),
        ["oohama2009"] = Create(
            "oohama2009",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/OohamaEtAl2009.txt",
            "Local source note exists for lenticular bulge/disk structure context."),
        ["laurikainen2010"] = Create(
            "laurikainen2010",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/LaurikainenEtAl2010.txt",
            "Local source note exists for S0 and disk/bulge structure context."),
        ["diazgarcia2016"] = Create(
            "diazgarcia2016",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/DiazGarcia2016.txt",
            "Local source note exists for bar-fraction and bar-strength context."),
        ["hart2017"] = Create(
            "hart2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Hart2017.txt",
            "Local source note exists for spiral arm count and pitch-angle context."),
        ["lingard2021"] = Create(
            "lingard2021",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Lingard2021.txt",
            "Local source note exists for transient spiral winding context."),
        ["rodriguezpadilla2013"] = Create(
            "rodriguezpadilla2013",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/RodriguezPadilla2013.txt",
            "Local source note exists for galaxy shape and ellipticity context."),
        ["kennicutt1998"] = Create(
            "kennicutt1998",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kennicutt1998.txt",
            "Local source note exists for star-formation scaling context."),
        ["forgan2017"] = Create(
            "forgan2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/ForgnEtAl2017.txt",
            "Local source note exists under the current retained filename; rename remains a source-ID normalization follow-up."),
        ["spitoni2017"] = Create(
            "spitoni2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/SpitoniEtAl2017.txt",
            "Local source note exists for Galactic chemical evolution and GHZ context."),
        ["apogee2024"] = Create(
            "apogee2024",
            SourceAcquisitionStatus.AcquisitionBlocked,
            string.Empty,
            "Catalog citation exists for metallicity-gradient context, but no retained local source note exists yet."),
        ["cmetall2024"] = Create(
            "cmetall2024",
            SourceAcquisitionStatus.AcquisitionBlocked,
            string.Empty,
            "Catalog citation exists for metallicity-gradient context, but no retained local source note exists yet."),
        ["kroupa2001"] = Create(
            "kroupa2001",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kroupa2001.txt",
            "Local source note exists for IMF grounding."),
        ["chabrier2003"] = Create(
            "chabrier2003",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Chabrier2003.txt",
            "Local source note exists for IMF grounding."),
        ["li2023"] = Create(
            "li2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Li2023.txt",
            "Local source note exists for IMF variation context."),
        ["choi2016"] = Create(
            "choi2016",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Choi2016.txt",
            "Local source note exists for MIST isochrone context."),
        ["bressan2012"] = Create(
            "bressan2012",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/BressanEtAl2012.txt",
            "Local source note exists for PARSEC isochrone context."),
        ["duchene2013"] = Create(
            "duchene2013",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/DucheneKraus2013.txt",
            "Local source note exists for stellar multiplicity context."),
        ["raghavan2010"] = Create(
            "raghavan2010",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Raghavan2010.txt",
            "Local source note exists for solar-type binary statistics."),
        ["tokovinin2014"] = Create(
            "tokovinin2014",
            SourceAcquisitionStatus.AcquisitionBlocked,
            string.Empty,
            "Catalog citation exists for hierarchy context, but no retained local source note exists yet."),
        ["tokovinin2021"] = Create(
            "tokovinin2021",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Tokovinin2021.txt",
            "Local source note exists for hierarchical stellar systems."),
        ["cummings2018"] = Create(
            "cummings2018",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Cummings2018.txt",
            "Local source note exists for white-dwarf initial-final mass relation context."),
        ["kirkpatrick2000"] = Create(
            "kirkpatrick2000",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kirkpatrick2000.txt",
            "Local source note exists for L-dwarf temperature context."),
        ["kirkpatrick2011"] = Create(
            "kirkpatrick2011",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kirkpatrick2011.txt",
            "Local source note exists for WISE brown-dwarf census context."),
        ["kirkpatrick2024"] = Create(
            "kirkpatrick2024",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kirkpatrick2024.txt",
            "Local source note exists for nearby stellar and brown-dwarf census context."),
        ["moedistefano2017"] = Create(
            "moedistefano2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/MoeDiStefano2017.txt",
            "Local source note exists for stellar multiplicity distributions."),
        ["hurley2000"] = Create(
            "hurley2000",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Hurley2000.txt",
            "Local source note exists for analytic stellar-evolution approximation context."),
        ["chenkipping2017"] = Create(
            "chenkipping2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/ChenKipping2017.txt",
            "Local source note exists for mass-radius model grounding."),
        ["otegi2020"] = Create(
            "otegi2020",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Otegi2020.txt",
            "Local source note exists for rocky and volatile-rich mass-radius relations."),
        ["fulton2017"] = Create(
            "fulton2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Fulton2017.txt",
            "Local source note exists for radius-gap context."),
        ["kasting1993"] = Create(
            "kasting1993",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kasting1993.txt",
            "Local source note exists for classical habitable-zone context."),
        ["kopparapu2013"] = Create(
            "kopparapu2013",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kopparapu2013.txt",
            "Local source note exists for updated habitable-zone limits."),
        ["kopparapu2014"] = Create(
            "kopparapu2014",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Kopparapu2014.txt",
            "Local source note exists for planet-mass dependence in HZ limits."),
        ["owenwu2017"] = Create(
            "owenwu2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/OwenWu2017.txt",
            "Local source note exists for photoevaporation context."),
        ["ginzburg2018"] = Create(
            "ginzburg2018",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Ginzburg2018.txt",
            "Local source note exists for core-powered mass-loss context."),
        ["mordasini2007"] = Create(
            "mordasini2007",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Mordasini2007.txt",
            "Local source note exists for core-accretion giant-formation context."),
        ["ronnet2020"] = Create(
            "ronnet2020",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Ronnet2020.txt",
            "Local source note exists for giant-planet moon-system context."),
        ["sasaki2010"] = Create(
            "sasaki2010",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Sasaki2010.txt",
            "Local source note exists for regular moon-system architecture context."),
        ["szulagyi2018"] = Create(
            "szulagyi2018",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Szulagyi2018.txt",
            "Local source note exists for ice-giant moon formation context."),
        ["jewitthaghighipour2007"] = Create(
            "jewitthaghighipour2007",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/JewittHaghighipour2007.txt",
            "Local source note exists for irregular and captured satellites."),
        ["demeocarry2014"] = Create(
            "demeocarry2014",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/DeMeoCarry2014.txt",
            "Local source note exists for asteroid compositional gradients; human verification remains required because the PDF extract metadata is corrupted."),
        ["kavelaarsetal2023"] = Create(
            "kavelaarsetal2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/KavelaarsEtAl2023.txt",
            "Local source note exists for Kuiper-belt/TNO population structure; retained as partly implemented with human-verification caveats."),
        ["bernardinellietal2022"] = Create(
            "bernardinellietal2022",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/BernardinelliEtAl2022.txt",
            "Local source note exists for DES TNO demographics; retained as partly implemented with survey-bias caveats."),
        ["napieretal2023"] = Create(
            "napieretal2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/NapierEtAl2023.txt",
            "Local source note exists, but the mechanics translation remains human-verification pending and is not an active default source in this pass."),
        ["lamy2004"] = Create(
            "lamy2004",
            SourceAcquisitionStatus.AcquisitionBlocked,
            string.Empty,
            "Previously cited comet source is no longer retained as active support after source access and replacement review."),
        ["mroz2020"] = Create(
            "mroz2020",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Mroz2020.txt",
            "Local source note exists for free-floating planet constraints."),
        ["petigura2013"] = Create(
            "petigura2013",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Petigura2013.txt",
            "Local source note exists for small-planet occurrence context."),
        ["bryson2021"] = Create(
            "bryson2021",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Bryson2021.txt",
            "Local source note exists for reliability-corrected habitable-zone rocky occurrence context."),
        ["bergstenetal2023"] = Create(
            "bergstenetal2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/BergstenEtAl2023.txt",
            "Local source note exists for M-dwarf habitable-zone occurrence caution."),
        ["mentcharbonneau2023"] = Create(
            "mentcharbonneau2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/MentCharbonneau2023.txt",
            "Local source note exists for close-in mid-to-late M dwarf occurrence context."),
        ["cuietal2026"] = Create(
            "cuietal2026",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/CuiEtAl2026.txt",
            "Local source note exists for close-in FGK TESS-era occurrence context; human metadata verification remains required."),
        ["gillisetal2026"] = Create(
            "gillisetal2026",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/GillisEtAl2026.txt",
            "Local source note exists for mid-to-late M dwarf close-in occurrence context; human metadata verification remains required."),
        ["raymondizidoro2017"] = Create(
            "raymondizidoro2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/RaymondIzidoro2017.txt",
            "Local source note exists for volatile-delivery and migration context."),
        ["ribas2015"] = Create(
            "ribas2015",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Ribas2015.txt",
            "Local source note exists for stellar-mass disk lifetime and XUV activity context."),
        ["izidoro2017"] = Create(
            "izidoro2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Izidoro2017.txt",
            "Local source note exists for migration-chain and post-instability compact-system context."),
        ["tanakatakeuchiward2002"] = Create(
            "tanakatakeuchiward2002",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/TanakaTakeuchiWard2002.txt",
            "Local source note exists for Type-I migration diagnostics; non-isothermal migration remains follow-up."),
        ["baueretal2017"] = Create(
            "baueretal2017",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/BauerEtAl2017.txt",
            "Local source note exists and anchors the v1.0 comet retune; human verification remains required before release claims are final."),
        ["savvidouetal2023"] = Create(
            "savvidouetal2023",
            SourceAcquisitionStatus.PendingReacquisition,
            "res://Sources/Texts/SavvidouEtAl2023.txt",
            "Local note is based on HTML because the acquired PDF did not match the article; keep source-backed controls human-audit-required until the correct PDF is reacquired."),
        ["pascucci2016"] = Create(
            "pascucci2016",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Pascucci2016.txt",
            "Local source note exists from earlier planetary grounding work."),
        ["fischervalenti2005"] = Create(
            "fischervalenti2005",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/FischerValenti2005.txt",
            "Local source note exists from earlier planetary grounding work."),
        ["lambrechtsjohansen2012"] = Create(
            "lambrechtsjohansen2012",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/LambrechtsJohansen2012.txt",
            "Local source note exists from earlier planetary grounding work."),
        ["fernandes2019"] = Create(
            "fernandes2019",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Fernandes2019.txt",
            "Local source note exists from earlier planetary grounding work."),
        ["escuderoetal2023"] = Create(
            "escuderoetal2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/EscuderoEtAl2023.txt",
            "Local source note exists for dark-biosphere proxy work; biological interpretation remains human-audit-required for release claims."),
        ["hellerbarnes2013"] = Create(
            "hellerbarnes2013",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/HellerBarnes2013.txt",
            "Local source note exists from earlier moon habitability grounding work."),
        ["lineweaverdavis2002"] = Create(
            "lineweaverdavis2002",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/LineweaverDavis2002.txt",
            "Local source note exists for rapid-biogenesis framing."),
        ["spiegelturner2012"] = Create(
            "spiegelturner2012",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/SpiegelTurner2012.txt",
            "Local source note exists for conservative abiogenesis framing."),
        ["forganrice2010"] = Create(
            "forganrice2010",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/ForganRice2010.txt",
            "Local source note exists for Rare Earth and intelligent-life filters."),
        ["mills2024"] = Create(
            "mills2024",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Mills2024.txt",
            "Local source note exists for environmental-window life framing."),
        ["balbi2023"] = Create(
            "balbi2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Balbi2023.txt",
            "Local source note exists for oxygen and technosphere bottleneck framing."),
        ["hamiltonetal2020"] = Create(
            "hamiltonetal2020",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/HamiltonEtAl2020.txt",
            "Local source note exists; sentient-world implications remain human-audit-required."),
        ["bettencourtetal2007"] = Create(
            "bettencourtetal2007",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/BettencourtEtAl2007.txt",
            "Local source note exists; urban-scaling implications remain human-audit-required before release claims."),
        ["arvidssonetal2023"] = Create(
            "arvidssonetal2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/ArvidssonEtAl2023.txt",
            "Local source note exists; technology-access inequality implications remain human-audit-required."),
        ["knez2023"] = Create(
            "knez2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Knez2023.txt",
            "Local source note exists; technology-diffusion implications remain human-audit-required."),
        ["stokey2020"] = Create(
            "stokey2020",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Stokey2020.txt",
            "Local source note exists; technology-diffusion implications remain human-audit-required and off-topic status must stay visible until human review."),
        ["comin2013"] = Create(
            "comin2013",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Comin2013.txt",
            "Local source note exists; use as a technology-diffusion support source pending human verification."),
        ["cominmestieri2013"] = Create(
            "cominmestieri2013",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/CominMestieri2013.txt",
            "Local source note exists; use as a technology-diffusion support source pending human verification."),
        ["chowdhury2022"] = Create(
            "chowdhury2022",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/Chowdhury2022.txt",
            "Local source note exists; state-capacity/regulation implications remain human-audit-required and source-note metadata needs cleanup."),
        ["chacuaetal2024"] = Create(
            "chacuaetal2024",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/ChacuaEtAl2024.txt",
            "Local source note exists; economic-complexity implications remain human-audit-required."),
        ["ballandetal2022"] = Create(
            "ballandetal2022",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/BallandEtAl2022.txt",
            "Local source note exists; economic-complexity implications remain human-audit-required."),
        ["vankleefetal2023"] = Create(
            "vankleefetal2023",
            SourceAcquisitionStatus.LocalSourceNote,
            "res://Sources/Texts/VanKleefEtAl2023.txt",
            "Local source note exists; legitimacy and norm implications remain human-audit-required."),
        ["canupward2006"] = Create(
            "canupward2006",
            SourceAcquisitionStatus.AcquisitionBlocked,
            string.Empty,
            "Previously cited source was removed from active support after source access was withdrawn; retained here only to document why missing local files are not treated as omissions."),
    };

    /// <summary>
    /// Returns the acquisition record for a source ID, if one has been registered.
    /// </summary>
    public static SourceAcquisitionRecord? GetRecord(string sourceId)
    {
        if (Records.TryGetValue(sourceId, out SourceAcquisitionRecord? record))
        {
            return record;
        }

        return null;
    }

    /// <summary>
    /// Returns every registered source-acquisition record.
    /// </summary>
    public static IReadOnlyCollection<SourceAcquisitionRecord> GetRecords()
    {
        return Records.Values;
    }

    private static SourceAcquisitionRecord Create(
        string sourceId,
        SourceAcquisitionStatus status,
        string textNoteResPath,
        string auditNote)
    {
        return new SourceAcquisitionRecord(sourceId, status, textNoteResPath, auditNote);
    }
}
