using Godot;
using Godot.Collections;
using StarGen.Domain.Generation;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Specification for a procedurally generated galaxy.
/// </summary>
public partial class GalaxySpec : RefCounted
{
    /// <summary>
    /// Supported galaxy families.
    /// </summary>
    public enum GalaxyType
    {
        Spiral = 0,
        Elliptical = 1,
        Lenticular = 2,
        Irregular = 3,
    }

    /// <summary>
    /// Master seed for the galaxy.
    /// </summary>
    public int GalaxySeed { get; set; }

    /// <summary>
    /// Top-level galaxy family.
    /// </summary>
    public GalaxyType Type { get; set; } = GalaxyType.Spiral;

    /// <summary>
    /// User-selected subtype bias mode.
    /// </summary>
    public GalaxySubtypeMode SubtypeMode { get; set; } = GalaxySubtypeMode.Automatic;

    /// <summary>
    /// User-selected bar preference.
    /// </summary>
    public GalaxyBarMode BarMode { get; set; } = GalaxyBarMode.Auto;

    /// <summary>
    /// User-selected spiral-arm preference.
    /// </summary>
    public GalaxyArmMechanism ArmMechanismPreference { get; set; } = GalaxyArmMechanism.Auto;

    /// <summary>
    /// Scientifically resolved subtype.
    /// </summary>
    public GalaxyResolvedSubtype ResolvedSubtype { get; set; } = GalaxyResolvedSubtype.SpiralSb;

    /// <summary>
    /// Radius of the galactic footprint in parsecs.
    /// </summary>
    public double RadiusPc { get; set; } = 15000.0;

    /// <summary>
    /// Half-height of the full extent in parsecs.
    /// </summary>
    public double HeightPc { get; set; } = 1000.0;

    /// <summary>
    /// Number of spiral arms.
    /// </summary>
    public int NumArms { get; set; } = 4;

    /// <summary>
    /// Pitch angle of the logarithmic spiral arms in degrees.
    /// </summary>
    public double ArmPitchAngleDeg { get; set; } = 14.0;

    /// <summary>
    /// Angular half-width of each arm in radians.
    /// </summary>
    public double ArmWidth { get; set; } = 0.4;

    /// <summary>
    /// Arm density contrast.
    /// </summary>
    public double ArmAmplitude { get; set; } = 0.65;

    /// <summary>
    /// Radius of the central bulge in parsecs.
    /// </summary>
    public double BulgeRadiusPc { get; set; } = 1500.0;

    /// <summary>
    /// Half-height of the central bulge in parsecs.
    /// </summary>
    public double BulgeHeightPc { get; set; } = 800.0;

    /// <summary>
    /// Peak intensity of the bulge relative to disk normalization.
    /// </summary>
    public double BulgeIntensity { get; set; } = 0.8;

    /// <summary>
    /// Exponential scale length of the disk in parsecs.
    /// </summary>
    public double DiskScaleLengthPc { get; set; } = 4000.0;

    /// <summary>
    /// Exponential scale height of the disk in parsecs.
    /// </summary>
    public double DiskScaleHeightPc { get; set; } = 300.0;

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
    /// Ellipticity for spheroidal galaxies.
    /// </summary>
    public double Ellipticity { get; set; } = 0.3;

    /// <summary>
    /// Irregularity scale for irregular and dwarf galaxies.
    /// </summary>
    public double IrregularityScale { get; set; } = 0.5;

    /// <summary>
    /// Halo-mass proxy in log10 solar masses.
    /// </summary>
    public double HaloMassLog10Solar { get; set; } = 12.0;

    /// <summary>
    /// Normalized environment-density index spanning field-like to cluster-like host conditions.
    /// </summary>
    public double EnvironmentDensityIndex { get; set; } = 0.25;

    /// <summary>
    /// Resolved arm mechanism after scientific variation is applied.
    /// </summary>
    public GalaxyArmMechanism ArmMechanism { get; set; } = GalaxyArmMechanism.GrandDesign;

    /// <summary>
    /// Whether the resolved galaxy includes a central bar.
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
    /// Sérsic index of the dominant spheroid.
    /// </summary>
    public double SersicIndex { get; set; } = 2.5;

    /// <summary>
    /// Effective radius of the dominant spheroid in parsecs.
    /// </summary>
    public double EffectiveRadiusPc { get; set; } = 2500.0;

    /// <summary>
    /// Bulge-to-total stellar-light proxy.
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
    /// Soft galactic habitable-zone transition width in parsecs.
    /// </summary>
    public double GhzTransitionWidthPc { get; set; } = 2000.0;

    /// <summary>
    /// Radial metallicity gradient in dex per kiloparsec.
    /// </summary>
    public double MetallicityGradientDexPerKpc { get; set; } = -0.05;

    /// <summary>
    /// Star-formation-efficiency prior used for cluster scaffolding and downstream coupling.
    /// </summary>
    public double StarFormationEfficiency { get; set; } = 0.1;

    /// <summary>
    /// Embedded-cluster mass-function slope.
    /// </summary>
    public double ClusterMassFunctionSlope { get; set; } = 2.0;

    /// <summary>
    /// Characteristic dissolution timescale for unbound clusters in megayears.
    /// </summary>
    public double ClusterDissolutionTimescaleMyr { get; set; } = 10.0;

    /// <summary>
    /// Resolved realism profile backing the galaxy specification.
    /// </summary>
    public GalaxyRealismProfile RealismProfile { get; set; } = new GalaxyRealismProfile();

    /// <summary>
    /// Shared stellar-generation profile inherited by systems spawned from this galaxy.
    /// </summary>
    public StellarGenerationProfile StellarProfile { get; set; } = StellarGenerationProfile.CreateDefault();

    /// <summary>
    /// Creates a Milky-Way-like spiral galaxy specification.
    /// </summary>
    public static GalaxySpec CreateMilkyWay(int galaxySeed)
    {
        return CreateFromConfig(GalaxyConfig.CreateMilkyWay(), galaxySeed);
    }

    /// <summary>
    /// Creates a galaxy specification from a configuration object and seed.
    /// </summary>
    public static GalaxySpec CreateFromConfig(GalaxyConfig? config, int galaxySeed)
    {
        GalaxyConfig effectiveConfig = config ?? GalaxyConfig.CreateDefault();
        GalaxySpec spec = new GalaxySpec
        {
            GalaxySeed = galaxySeed,
        };
        GalaxyRealismProfile profile = GalaxyRealismProfileBuilder.Build(effectiveConfig, galaxySeed);
        GalaxyRealismProfileBuilder.ApplyToSpec(effectiveConfig, profile, spec);
        return spec;
    }

    /// <summary>
    /// Converts the specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["galaxy_seed"] = GalaxySeed,
            ["galaxy_type"] = (int)Type,
            ["subtype_mode"] = (int)SubtypeMode,
            ["bar_mode"] = (int)BarMode,
            ["arm_mechanism_preference"] = (int)ArmMechanismPreference,
            ["resolved_subtype"] = (int)ResolvedSubtype,
            ["radius_pc"] = RadiusPc,
            ["height_pc"] = HeightPc,
            ["num_arms"] = NumArms,
            ["arm_pitch_angle_deg"] = ArmPitchAngleDeg,
            ["arm_width"] = ArmWidth,
            ["arm_amplitude"] = ArmAmplitude,
            ["bulge_radius_pc"] = BulgeRadiusPc,
            ["bulge_height_pc"] = BulgeHeightPc,
            ["bulge_intensity"] = BulgeIntensity,
            ["disk_scale_length_pc"] = DiskScaleLengthPc,
            ["disk_scale_height_pc"] = DiskScaleHeightPc,
            ["thin_disk_scale_length_pc"] = ThinDiskScaleLengthPc,
            ["thick_disk_scale_length_pc"] = ThickDiskScaleLengthPc,
            ["thin_disk_scale_height_pc"] = ThinDiskScaleHeightPc,
            ["thick_disk_scale_height_pc"] = ThickDiskScaleHeightPc,
            ["ellipticity"] = Ellipticity,
            ["irregularity_scale"] = IrregularityScale,
            ["halo_mass_log10_solar"] = HaloMassLog10Solar,
            ["environment_density_index"] = EnvironmentDensityIndex,
            ["arm_mechanism"] = (int)ArmMechanism,
            ["is_barred"] = IsBarred,
            ["bar_strength"] = BarStrength,
            ["bar_half_length_pc"] = BarHalfLengthPc,
            ["solar_galactocentric_radius_pc"] = SolarGalactocentricRadiusPc,
            ["circular_velocity_at_solar_radius_km_s"] = CircularVelocityAtSolarRadiusKmS,
            ["stellar_mass_solar"] = StellarMassSolar,
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
            ["realism_profile"] = RealismProfile.ToDictionary(),
            ["stellar_profile"] = StellarProfile.ToDictionary(),
        };
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static GalaxySpec FromDictionary(Dictionary data)
    {
        GalaxySpec spec = new GalaxySpec
        {
            GalaxySeed = DomainDictionaryUtils.GetInt(data, "galaxy_seed", 0),
            RadiusPc = DomainDictionaryUtils.GetDouble(data, "radius_pc", 15000.0),
            HeightPc = DomainDictionaryUtils.GetDouble(data, "height_pc", 1000.0),
            NumArms = DomainDictionaryUtils.GetInt(data, "num_arms", 4),
            ArmPitchAngleDeg = DomainDictionaryUtils.GetDouble(data, "arm_pitch_angle_deg", 14.0),
            ArmWidth = DomainDictionaryUtils.GetDouble(data, "arm_width", 0.4),
            ArmAmplitude = DomainDictionaryUtils.GetDouble(data, "arm_amplitude", 0.65),
            BulgeRadiusPc = DomainDictionaryUtils.GetDouble(data, "bulge_radius_pc", 1500.0),
            BulgeHeightPc = DomainDictionaryUtils.GetDouble(data, "bulge_height_pc", 800.0),
            BulgeIntensity = DomainDictionaryUtils.GetDouble(data, "bulge_intensity", 0.8),
            DiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "disk_scale_length_pc", 2600.0),
            DiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "disk_scale_height_pc", 300.0),
            ThinDiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "thin_disk_scale_length_pc", 2600.0),
            ThickDiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "thick_disk_scale_length_pc", 2000.0),
            ThinDiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "thin_disk_scale_height_pc", 300.0),
            ThickDiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "thick_disk_scale_height_pc", 900.0),
            Ellipticity = DomainDictionaryUtils.GetDouble(data, "ellipticity", 0.3),
            IrregularityScale = DomainDictionaryUtils.GetDouble(data, "irregularity_scale", 0.5),
            HaloMassLog10Solar = DomainDictionaryUtils.GetDouble(data, "halo_mass_log10_solar", 12.0),
            EnvironmentDensityIndex = DomainDictionaryUtils.GetDouble(data, "environment_density_index", 0.25),
            IsBarred = DomainDictionaryUtils.GetBool(data, "is_barred", false),
            BarStrength = DomainDictionaryUtils.GetDouble(data, "bar_strength", 0.0),
            BarHalfLengthPc = DomainDictionaryUtils.GetDouble(data, "bar_half_length_pc", 4500.0),
            SolarGalactocentricRadiusPc = DomainDictionaryUtils.GetDouble(data, "solar_galactocentric_radius_pc", 8200.0),
            CircularVelocityAtSolarRadiusKmS = DomainDictionaryUtils.GetDouble(data, "circular_velocity_at_solar_radius_km_s", 240.0),
            StellarMassSolar = DomainDictionaryUtils.GetDouble(data, "stellar_mass_solar", 5.0e10),
            SersicIndex = DomainDictionaryUtils.GetDouble(data, "sersic_index", 2.5),
            EffectiveRadiusPc = DomainDictionaryUtils.GetDouble(data, "effective_radius_pc", 2500.0),
            BulgeToTotal = DomainDictionaryUtils.GetDouble(data, "bulge_to_total", 0.2),
            GhzInnerRadiusPc = DomainDictionaryUtils.GetDouble(data, "ghz_inner_radius_pc", 4000.0),
            GhzOuterRadiusPc = DomainDictionaryUtils.GetDouble(data, "ghz_outer_radius_pc", 12000.0),
            GhzTransitionWidthPc = DomainDictionaryUtils.GetDouble(data, "ghz_transition_width_pc", 2000.0),
            MetallicityGradientDexPerKpc = DomainDictionaryUtils.GetDouble(data, "metallicity_gradient_dex_per_kpc", -0.05),
            StarFormationEfficiency = DomainDictionaryUtils.GetDouble(data, "star_formation_efficiency", 0.1),
            ClusterMassFunctionSlope = DomainDictionaryUtils.GetDouble(data, "cluster_mass_function_slope", 2.0),
            ClusterDissolutionTimescaleMyr = DomainDictionaryUtils.GetDouble(data, "cluster_dissolution_timescale_myr", 10.0),
        };

        int typeValue = DomainDictionaryUtils.GetInt(data, "galaxy_type", (int)GalaxyType.Spiral);
        if (System.Enum.IsDefined(typeof(GalaxyType), typeValue))
        {
            spec.Type = (GalaxyType)typeValue;
        }

        int subtypeModeValue = DomainDictionaryUtils.GetInt(data, "subtype_mode", (int)GalaxySubtypeMode.Automatic);
        if (System.Enum.IsDefined(typeof(GalaxySubtypeMode), subtypeModeValue))
        {
            spec.SubtypeMode = (GalaxySubtypeMode)subtypeModeValue;
        }

        int barModeValue = DomainDictionaryUtils.GetInt(data, "bar_mode", (int)GalaxyBarMode.Auto);
        if (System.Enum.IsDefined(typeof(GalaxyBarMode), barModeValue))
        {
            spec.BarMode = (GalaxyBarMode)barModeValue;
        }

        int armPreferenceValue = DomainDictionaryUtils.GetInt(data, "arm_mechanism_preference", (int)GalaxyArmMechanism.Auto);
        if (System.Enum.IsDefined(typeof(GalaxyArmMechanism), armPreferenceValue))
        {
            spec.ArmMechanismPreference = (GalaxyArmMechanism)armPreferenceValue;
        }

        int resolvedSubtypeValue = DomainDictionaryUtils.GetInt(data, "resolved_subtype", (int)GalaxyResolvedSubtype.SpiralSb);
        if (System.Enum.IsDefined(typeof(GalaxyResolvedSubtype), resolvedSubtypeValue))
        {
            spec.ResolvedSubtype = (GalaxyResolvedSubtype)resolvedSubtypeValue;
        }

        int armMechanismValue = DomainDictionaryUtils.GetInt(data, "arm_mechanism", (int)GalaxyArmMechanism.GrandDesign);
        if (System.Enum.IsDefined(typeof(GalaxyArmMechanism), armMechanismValue))
        {
            spec.ArmMechanism = (GalaxyArmMechanism)armMechanismValue;
        }

        if (data.ContainsKey("realism_profile") && data["realism_profile"].VariantType == Variant.Type.Dictionary)
        {
            spec.RealismProfile = GalaxyRealismProfile.FromDictionary((Dictionary)data["realism_profile"]);
        }
        else
        {
            spec.RealismProfile = GalaxyRealismProfileBuilder.Build(GalaxyConfig.CreateMilkyWay(), spec.GalaxySeed);
        }

        if (data.ContainsKey("stellar_profile") && data["stellar_profile"].VariantType == Variant.Type.Dictionary)
        {
            spec.StellarProfile = StellarGenerationProfile.FromDictionary((Dictionary)data["stellar_profile"]);
        }
        else
        {
            spec.StellarProfile = StellarGenerationProfile.CreateDefault();
        }

        return spec;
    }
}
