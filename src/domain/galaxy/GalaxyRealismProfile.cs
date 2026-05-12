using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Stores the resolved scientific galaxy profile derived from the current family lock and realism tunables.
/// </summary>
public partial class GalaxyRealismProfile : RefCounted
{
    /// <summary>
    /// Top-level galaxy family selected by the user.
    /// </summary>
    public GalaxySpec.GalaxyType Family { get; set; } = GalaxySpec.GalaxyType.Spiral;

    /// <summary>
    /// Scientifically resolved subtype inside the selected family.
    /// </summary>
    public GalaxyResolvedSubtype ResolvedSubtype { get; set; } = GalaxyResolvedSubtype.SpiralSb;

    /// <summary>
    /// Log10 halo mass in solar masses used as the morphology prior.
    /// </summary>
    public double HaloMassLog10Solar { get; set; } = 12.0;

    /// <summary>
    /// Normalized environment-density index spanning field-like to cluster-like conditions.
    /// </summary>
    public double EnvironmentDensityIndex { get; set; } = 0.25;

    /// <summary>
    /// Resolved arm mechanism after any auto-selection has been applied.
    /// </summary>
    public GalaxyArmMechanism ArmMechanism { get; set; } = GalaxyArmMechanism.GrandDesign;

    /// <summary>
    /// Whether the resolved disk realization includes a central bar.
    /// </summary>
    public bool IsBarred { get; set; }

    /// <summary>
    /// Relative bar strength on a normalized 0-1 scale.
    /// </summary>
    public double BarStrength { get; set; } = 0.0;

    /// <summary>
    /// Half-length of the central stellar bar in parsecs.
    /// </summary>
    public double BarHalfLengthPc { get; set; } = 4500.0;

    /// <summary>
    /// Sérsic index used for bulge or spheroid structure.
    /// </summary>
    public double SersicIndex { get; set; } = 2.5;

    /// <summary>
    /// Effective radius of the dominant spheroid in parsecs.
    /// </summary>
    public double EffectiveRadiusPc { get; set; } = 2500.0;

    /// <summary>
    /// Bulge-to-total stellar-light proxy used for subtype structure.
    /// </summary>
    public double BulgeToTotal { get; set; } = 0.2;

    /// <summary>
    /// Inner galactic habitable-zone radius in parsecs.
    /// </summary>
    public double GhzInnerRadiusPc { get; set; } = 4000.0;

    /// <summary>
    /// Outer galactic habitable-zone radius in parsecs.
    /// </summary>
    public double GhzOuterRadiusPc { get; set; } = 12000.0;

    /// <summary>
    /// Soft-transition width for the galactic habitable zone in parsecs.
    /// </summary>
    public double GhzTransitionWidthPc { get; set; } = 2000.0;

    /// <summary>
    /// Radial metallicity gradient in dex per kiloparsec.
    /// </summary>
    public double MetallicityGradientDexPerKpc { get; set; } = -0.05;

    /// <summary>
    /// Star-formation efficiency prior used for galaxy-origin context.
    /// </summary>
    public double StarFormationEfficiency { get; set; } = 0.1;

    /// <summary>
    /// Embedded-cluster mass-function slope used for cluster scaffolding.
    /// </summary>
    public double ClusterMassFunctionSlope { get; set; } = 2.0;

    /// <summary>
    /// Characteristic dissolution timescale for unbound clusters in megayears.
    /// </summary>
    public double ClusterDissolutionTimescaleMyr { get; set; } = 10.0;

    /// <summary>
    /// Characteristic stellar-population age in gigayears.
    /// </summary>
    public double CharacteristicAgeGyr { get; set; } = 6.0;

    /// <summary>
    /// Thin-disk exponential scale length in parsecs.
    /// </summary>
    public double ThinDiskScaleLengthPc { get; set; } = 2600.0;

    /// <summary>
    /// Thick-disk exponential scale length in parsecs.
    /// </summary>
    public double ThickDiskScaleLengthPc { get; set; } = 2000.0;

    /// <summary>
    /// Thin-disk exponential scale height in parsecs.
    /// </summary>
    public double ThinDiskScaleHeightPc { get; set; } = 300.0;

    /// <summary>
    /// Thick-disk exponential scale height in parsecs.
    /// </summary>
    public double ThickDiskScaleHeightPc { get; set; } = 900.0;

    /// <summary>
    /// Solar-circle Galactocentric radius in parsecs.
    /// </summary>
    public double SolarGalactocentricRadiusPc { get; set; } = 8200.0;

    /// <summary>
    /// Circular velocity at the solar-circle radius in kilometers per second.
    /// </summary>
    public double CircularVelocityAtSolarRadiusKmS { get; set; } = 240.0;

    /// <summary>
    /// Stellar mass scale in solar masses.
    /// </summary>
    public double StellarMassSolar { get; set; } = 5.0e10;

    /// <summary>
    /// Source-use status for the Milky-Way structural schema.
    /// </summary>
    public string MilkyWayStructureSourceStatus { get; set; } = "partly implemented";

    /// <summary>
    /// Diagnostic-only mass component budget used by science audits and future dynamics work.
    /// </summary>
    public GalaxyMassComponentBudget MassComponentBudget { get; set; } = new GalaxyMassComponentBudget();

    /// <summary>
    /// Creates a dictionary payload for persistence and provenance.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["family"] = (int)Family,
            ["resolved_subtype"] = (int)ResolvedSubtype,
            ["halo_mass_log10_solar"] = HaloMassLog10Solar,
            ["environment_density_index"] = EnvironmentDensityIndex,
            ["arm_mechanism"] = (int)ArmMechanism,
            ["is_barred"] = IsBarred,
            ["bar_strength"] = BarStrength,
            ["bar_half_length_pc"] = BarHalfLengthPc,
            ["sersic_index"] = SersicIndex,
            ["effective_radius_pc"] = EffectiveRadiusPc,
            ["bulge_to_total"] = BulgeToTotal,
            ["ghz_inner_radius_pc"] = GhzInnerRadiusPc,
            ["ghz_outer_radius_pc"] = GhzOuterRadiusPc,
            ["ghz_transition_width_pc"] = GhzTransitionWidthPc,
            ["metallicity_gradient_dex_per_kpc"] = MetallicityGradientDexPerKpc,
            ["star_formation_efficiency"] = StarFormationEfficiency,
            ["cluster_mass_function_slope"] = ClusterMassFunctionSlope,
            ["cluster_dissolution_timescale_myr"] = ClusterDissolutionTimescaleMyr,
            ["characteristic_age_gyr"] = CharacteristicAgeGyr,
            ["thin_disk_scale_length_pc"] = ThinDiskScaleLengthPc,
            ["thick_disk_scale_length_pc"] = ThickDiskScaleLengthPc,
            ["thin_disk_scale_height_pc"] = ThinDiskScaleHeightPc,
            ["thick_disk_scale_height_pc"] = ThickDiskScaleHeightPc,
            ["solar_galactocentric_radius_pc"] = SolarGalactocentricRadiusPc,
            ["circular_velocity_at_solar_radius_km_s"] = CircularVelocityAtSolarRadiusKmS,
            ["stellar_mass_solar"] = StellarMassSolar,
            ["milky_way_structure_source_status"] = MilkyWayStructureSourceStatus,
            ["mass_component_budget"] = MassComponentBudget.ToDictionary(),
        };
    }

    /// <summary>
    /// Rebuilds a profile from a serialized dictionary payload.
    /// </summary>
    public static GalaxyRealismProfile FromDictionary(Dictionary data)
    {
        GalaxyRealismProfile profile = new GalaxyRealismProfile();
        int familyValue = DomainDictionaryUtils.GetInt(data, "family", (int)GalaxySpec.GalaxyType.Spiral);
        if (System.Enum.IsDefined(typeof(GalaxySpec.GalaxyType), familyValue))
        {
            profile.Family = (GalaxySpec.GalaxyType)familyValue;
        }

        int subtypeValue = DomainDictionaryUtils.GetInt(data, "resolved_subtype", (int)GalaxyResolvedSubtype.SpiralSb);
        if (System.Enum.IsDefined(typeof(GalaxyResolvedSubtype), subtypeValue))
        {
            profile.ResolvedSubtype = (GalaxyResolvedSubtype)subtypeValue;
        }

        int armMechanismValue = DomainDictionaryUtils.GetInt(data, "arm_mechanism", (int)GalaxyArmMechanism.GrandDesign);
        if (System.Enum.IsDefined(typeof(GalaxyArmMechanism), armMechanismValue))
        {
            profile.ArmMechanism = (GalaxyArmMechanism)armMechanismValue;
        }

        profile.HaloMassLog10Solar = DomainDictionaryUtils.GetDouble(data, "halo_mass_log10_solar", 12.0);
        profile.EnvironmentDensityIndex = DomainDictionaryUtils.GetDouble(data, "environment_density_index", 0.25);
        profile.IsBarred = DomainDictionaryUtils.GetBool(data, "is_barred", false);
        profile.BarStrength = DomainDictionaryUtils.GetDouble(data, "bar_strength", 0.0);
        profile.BarHalfLengthPc = DomainDictionaryUtils.GetDouble(data, "bar_half_length_pc", 4500.0);
        profile.SersicIndex = DomainDictionaryUtils.GetDouble(data, "sersic_index", 2.5);
        profile.EffectiveRadiusPc = DomainDictionaryUtils.GetDouble(data, "effective_radius_pc", 2500.0);
        profile.BulgeToTotal = DomainDictionaryUtils.GetDouble(data, "bulge_to_total", 0.2);
        profile.GhzInnerRadiusPc = DomainDictionaryUtils.GetDouble(data, "ghz_inner_radius_pc", 4000.0);
        profile.GhzOuterRadiusPc = DomainDictionaryUtils.GetDouble(data, "ghz_outer_radius_pc", 12000.0);
        profile.GhzTransitionWidthPc = DomainDictionaryUtils.GetDouble(data, "ghz_transition_width_pc", 2000.0);
        profile.MetallicityGradientDexPerKpc = DomainDictionaryUtils.GetDouble(data, "metallicity_gradient_dex_per_kpc", -0.05);
        profile.StarFormationEfficiency = DomainDictionaryUtils.GetDouble(data, "star_formation_efficiency", 0.1);
        profile.ClusterMassFunctionSlope = DomainDictionaryUtils.GetDouble(data, "cluster_mass_function_slope", 2.0);
        profile.ClusterDissolutionTimescaleMyr = DomainDictionaryUtils.GetDouble(data, "cluster_dissolution_timescale_myr", 10.0);
        profile.CharacteristicAgeGyr = DomainDictionaryUtils.GetDouble(data, "characteristic_age_gyr", 6.0);
        profile.ThinDiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "thin_disk_scale_length_pc", 2600.0);
        profile.ThickDiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "thick_disk_scale_length_pc", 2000.0);
        profile.ThinDiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "thin_disk_scale_height_pc", 300.0);
        profile.ThickDiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "thick_disk_scale_height_pc", 900.0);
        profile.SolarGalactocentricRadiusPc = DomainDictionaryUtils.GetDouble(data, "solar_galactocentric_radius_pc", 8200.0);
        profile.CircularVelocityAtSolarRadiusKmS = DomainDictionaryUtils.GetDouble(data, "circular_velocity_at_solar_radius_km_s", 240.0);
        profile.StellarMassSolar = DomainDictionaryUtils.GetDouble(data, "stellar_mass_solar", 5.0e10);
        profile.MilkyWayStructureSourceStatus = DomainDictionaryUtils.GetString(data, "milky_way_structure_source_status", "partly implemented");
        if (data.ContainsKey("mass_component_budget") && data["mass_component_budget"].VariantType == Variant.Type.Dictionary)
        {
            profile.MassComponentBudget = GalaxyMassComponentBudget.FromDictionary((Dictionary)data["mass_component_budget"]);
        }
        else
        {
            profile.MassComponentBudget = GalaxyMassComponentBudget.CreateDiagnostic(profile);
        }

        return profile;
    }
}
