using Godot;
using Godot.Collections;
using StarGen.Domain.Generation;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Configuration values for galaxy generation presets and editor-facing tuning.
/// </summary>
public partial class GalaxyConfig : RefCounted
{
    /// <summary>
    /// Top-level galaxy family lock.
    /// </summary>
    public GalaxySpec.GalaxyType Type { get; set; } = GalaxySpec.GalaxyType.Spiral;

    /// <summary>
    /// Legacy alias for galaxy family.
    /// </summary>
    public GalaxySpec.GalaxyType GalaxyType
    {
        get => Type;
        set => Type = value;
    }

    /// <summary>
    /// Controls whether the selected family should skew early or late within its scientific subtype ladder.
    /// </summary>
    public GalaxySubtypeMode SubtypeMode { get; set; } = GalaxySubtypeMode.Automatic;

    /// <summary>
    /// Controls whether barred disk realizations are preferred when the selected family supports bars.
    /// </summary>
    public GalaxyBarMode BarMode { get; set; } = GalaxyBarMode.Auto;

    /// <summary>
    /// Preferred spiral-arm mechanism for disk galaxies.
    /// </summary>
    public GalaxyArmMechanism ArmMechanismPreference { get; set; } = GalaxyArmMechanism.Auto;

    /// <summary>
    /// Log10 halo mass in solar masses used for family-internal scientific variation.
    /// </summary>
    public double HaloMassLog10Solar { get; set; } = 12.0;

    /// <summary>
    /// Normalized environment-density index spanning field-like to cluster-like host conditions.
    /// </summary>
    public double EnvironmentDensityIndex { get; set; } = 0.25;

    /// <summary>
    /// Galactic habitable-zone inner radius in parsecs.
    /// </summary>
    public double GhzInnerRadiusPc { get; set; } = 4000.0;

    /// <summary>
    /// Galactic habitable-zone outer radius in parsecs.
    /// </summary>
    public double GhzOuterRadiusPc { get; set; } = 12000.0;

    /// <summary>
    /// Soft transition width for the galactic habitable zone in parsecs.
    /// </summary>
    public double GhzTransitionWidthPc { get; set; } = 2000.0;

    /// <summary>
    /// Radial metallicity gradient in dex per kiloparsec.
    /// </summary>
    public double MetallicityGradientDexPerKpc { get; set; } = -0.05;

    /// <summary>
    /// Star-formation efficiency prior used for cluster scaffolding and downstream coupling.
    /// </summary>
    public double StarFormationEfficiency { get; set; } = 0.1;

    /// <summary>
    /// Number of spiral arms for spiral galaxies.
    /// </summary>
    public int NumArms { get; set; } = 4;

    /// <summary>
    /// Pitch angle of the spiral arms in degrees.
    /// </summary>
    public double ArmPitchAngleDeg { get; set; } = 14.0;

    /// <summary>
    /// Density contrast of the spiral arms.
    /// </summary>
    public double ArmAmplitude { get; set; } = 0.65;

    /// <summary>
    /// Central bulge intensity multiplier.
    /// </summary>
    public double BulgeIntensity { get; set; } = 0.8;

    /// <summary>
    /// Exponential disk scale length in parsecs.
    /// </summary>
    public double DiskScaleLengthPc { get; set; } = 4000.0;

    /// <summary>
    /// Overall star density multiplier.
    /// </summary>
    public double StarDensityMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Exponential disk scale height in parsecs.
    /// </summary>
    public double DiskScaleHeightPc { get; set; } = 300.0;

    /// <summary>
    /// Milky-Way thin-disk exponential scale length in parsecs.
    /// </summary>
    public double ThinDiskScaleLengthPc { get; set; } = 2600.0;

    /// <summary>
    /// Milky-Way thick-disk exponential scale length in parsecs.
    /// </summary>
    public double ThickDiskScaleLengthPc { get; set; } = 2000.0;

    /// <summary>
    /// Milky-Way thin-disk exponential scale height in parsecs.
    /// </summary>
    public double ThinDiskScaleHeightPc { get; set; } = 300.0;

    /// <summary>
    /// Milky-Way thick-disk exponential scale height in parsecs.
    /// </summary>
    public double ThickDiskScaleHeightPc { get; set; } = 900.0;

    /// <summary>
    /// Half-length of the central stellar bar in parsecs.
    /// </summary>
    public double BarHalfLengthPc { get; set; } = 4500.0;

    /// <summary>
    /// Solar-circle Galactocentric radius used for Milky-Way analog calibration in parsecs.
    /// </summary>
    public double SolarGalactocentricRadiusPc { get; set; } = 8200.0;

    /// <summary>
    /// Circular velocity at the solar-circle radius in kilometers per second.
    /// </summary>
    public double CircularVelocityAtSolarRadiusKmS { get; set; } = 240.0;

    /// <summary>
    /// Stellar mass scale for the Milky-Way analog in solar masses.
    /// </summary>
    public double StellarMassSolar { get; set; } = 5.0e10;

    /// <summary>
    /// Central bulge radius in parsecs.
    /// </summary>
    public double BulgeRadiusPc { get; set; } = 1500.0;

    /// <summary>
    /// Galaxy radius in parsecs.
    /// </summary>
    public double RadiusPc { get; set; } = 15000.0;

    /// <summary>
    /// Ellipticity for elliptical galaxies.
    /// </summary>
    public double Ellipticity { get; set; } = 0.3;

    /// <summary>
    /// Noise frequency scaling for irregular galaxies.
    /// </summary>
    public double IrregularityScale { get; set; } = 0.5;

    /// <summary>
    /// Shared generation intent for ruleset/readout behavior.
    /// </summary>
    public GenerationUseCaseSettings UseCaseSettings { get; set; } = GenerationUseCaseSettings.CreateDefault();

    /// <summary>
    /// Shared stellar-generation profile used for downstream star and system generation.
    /// </summary>
    public StellarGenerationProfile StellarProfile { get; set; } = StellarGenerationProfile.CreateDefault();

    /// <summary>
    /// Shared aggregate planetary-generation profile used for downstream system generation.
    /// </summary>
    public PlanetaryGenerationProfile PlanetaryProfile { get; set; } = PlanetaryGenerationProfile.CreateDefault();

    /// <summary>
    /// Creates a default configuration.
    /// </summary>
    public static GalaxyConfig CreateDefault()
    {
        return CreateMilkyWay();
    }

    /// <summary>
    /// Creates a Milky-Way-like configuration.
    /// </summary>
    public static GalaxyConfig CreateMilkyWay()
    {
        return new GalaxyConfig
        {
            Type = GalaxySpec.GalaxyType.Spiral,
            SubtypeMode = GalaxySubtypeMode.IntermediateType,
            BarMode = GalaxyBarMode.PreferBarred,
            ArmMechanismPreference = GalaxyArmMechanism.GrandDesign,
            HaloMassLog10Solar = 12.0,
            EnvironmentDensityIndex = 0.25,
            GhzInnerRadiusPc = 4000.0,
            GhzOuterRadiusPc = 12000.0,
            GhzTransitionWidthPc = 2000.0,
            MetallicityGradientDexPerKpc = -0.05,
            StarFormationEfficiency = 0.1,
            NumArms = 4,
            ArmPitchAngleDeg = 14.0,
            ArmAmplitude = 0.65,
            BulgeIntensity = 0.8,
            DiskScaleLengthPc = 2600.0,
            StarDensityMultiplier = 1.0,
            DiskScaleHeightPc = 300.0,
            ThinDiskScaleLengthPc = 2600.0,
            ThickDiskScaleLengthPc = 2000.0,
            ThinDiskScaleHeightPc = 300.0,
            ThickDiskScaleHeightPc = 900.0,
            BarHalfLengthPc = 4500.0,
            SolarGalactocentricRadiusPc = 8200.0,
            CircularVelocityAtSolarRadiusKmS = 240.0,
            StellarMassSolar = 5.0e10,
            BulgeRadiusPc = 1500.0,
            RadiusPc = 15000.0,
            Ellipticity = 0.3,
            IrregularityScale = 0.5,
            StellarProfile = StellarGenerationProfile.CreateDefault(),
            PlanetaryProfile = PlanetaryGenerationProfile.CreateDefault(),
        };
    }

    /// <summary>
    /// Applies the configuration values to a galaxy specification.
    /// </summary>
    public void ApplyToSpec(GalaxySpec spec)
    {
        GalaxyRealismProfile profile = GalaxyRealismProfileBuilder.Build(this, spec.GalaxySeed);
        GalaxyRealismProfileBuilder.ApplyToSpec(this, profile, spec);
    }

    /// <summary>
    /// Returns whether the configuration values are inside the supported ranges.
    /// </summary>
    public bool IsValid()
    {
        if ((int)Type < (int)GalaxySpec.GalaxyType.Spiral || (int)Type > (int)GalaxySpec.GalaxyType.Irregular)
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(GalaxySubtypeMode), (int)SubtypeMode))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(GalaxyBarMode), (int)BarMode))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(GalaxyArmMechanism), (int)ArmMechanismPreference))
        {
            return false;
        }

        if (HaloMassLog10Solar < 9.0 || HaloMassLog10Solar > 13.8)
        {
            return false;
        }

        if (EnvironmentDensityIndex < 0.0 || EnvironmentDensityIndex > 1.0)
        {
            return false;
        }

        if (GhzInnerRadiusPc < 2000.0 || GhzInnerRadiusPc > 15000.0)
        {
            return false;
        }

        if (GhzOuterRadiusPc <= GhzInnerRadiusPc || GhzOuterRadiusPc > 30000.0)
        {
            return false;
        }

        if (GhzTransitionWidthPc < 500.0 || GhzTransitionWidthPc > 8000.0)
        {
            return false;
        }

        if (MetallicityGradientDexPerKpc > -0.005 || MetallicityGradientDexPerKpc < -0.12)
        {
            return false;
        }

        if (StarFormationEfficiency < 0.05 || StarFormationEfficiency > 0.65)
        {
            return false;
        }

        if (NumArms < 0 || NumArms > 6)
        {
            return false;
        }

        if (ArmPitchAngleDeg < 10.0 || ArmPitchAngleDeg > 35.0)
        {
            return false;
        }

        if (ArmAmplitude < 0.0 || ArmAmplitude > 0.95)
        {
            return false;
        }

        if (BulgeIntensity < 0.2 || BulgeIntensity > 1.4)
        {
            return false;
        }

        if (DiskScaleLengthPc < 1200.0 || DiskScaleLengthPc > 9000.0)
        {
            return false;
        }

        if (StarDensityMultiplier < 0.3 || StarDensityMultiplier > 2.5)
        {
            return false;
        }

        if (DiskScaleHeightPc < 120.0 || DiskScaleHeightPc > 1200.0)
        {
            return false;
        }

        if (ThinDiskScaleLengthPc < 1200.0 || ThinDiskScaleLengthPc > 6000.0)
        {
            return false;
        }

        if (ThickDiskScaleLengthPc < 1000.0 || ThickDiskScaleLengthPc > 6000.0)
        {
            return false;
        }

        if (ThinDiskScaleHeightPc < 100.0 || ThinDiskScaleHeightPc > 600.0)
        {
            return false;
        }

        if (ThickDiskScaleHeightPc < 500.0 || ThickDiskScaleHeightPc > 2000.0)
        {
            return false;
        }

        if (BarHalfLengthPc < 1500.0 || BarHalfLengthPc > 7000.0)
        {
            return false;
        }

        if (SolarGalactocentricRadiusPc < 7000.0 || SolarGalactocentricRadiusPc > 9000.0)
        {
            return false;
        }

        if (CircularVelocityAtSolarRadiusKmS < 180.0 || CircularVelocityAtSolarRadiusKmS > 280.0)
        {
            return false;
        }

        if (StellarMassSolar < 1.0e9 || StellarMassSolar > 3.0e11)
        {
            return false;
        }

        if (BulgeRadiusPc < 600.0 || BulgeRadiusPc > 6000.0)
        {
            return false;
        }

        if (RadiusPc < 5000.0 || RadiusPc > 60000.0)
        {
            return false;
        }

        if (Ellipticity < 0.0 || Ellipticity > 0.8)
        {
            return false;
        }

        if (IrregularityScale < 0.1 || IrregularityScale > 1.2)
        {
            return false;
        }

        if (StellarProfile == null || !StellarProfile.IsValid())
        {
            return false;
        }

        if (PlanetaryProfile == null || !PlanetaryProfile.IsValid())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts the configuration to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["galaxy_type"] = (int)Type,
            ["subtype_mode"] = (int)SubtypeMode,
            ["bar_mode"] = (int)BarMode,
            ["arm_mechanism_preference"] = (int)ArmMechanismPreference,
            ["halo_mass_log10_solar"] = HaloMassLog10Solar,
            ["environment_density_index"] = EnvironmentDensityIndex,
            ["ghz_inner_radius_pc"] = GhzInnerRadiusPc,
            ["ghz_outer_radius_pc"] = GhzOuterRadiusPc,
            ["ghz_transition_width_pc"] = GhzTransitionWidthPc,
            ["metallicity_gradient_dex_per_kpc"] = MetallicityGradientDexPerKpc,
            ["star_formation_efficiency"] = StarFormationEfficiency,
            ["num_arms"] = NumArms,
            ["arm_pitch_angle_deg"] = ArmPitchAngleDeg,
            ["arm_amplitude"] = ArmAmplitude,
            ["bulge_intensity"] = BulgeIntensity,
            ["disk_scale_length_pc"] = DiskScaleLengthPc,
            ["star_density_multiplier"] = StarDensityMultiplier,
            ["disk_scale_height_pc"] = DiskScaleHeightPc,
            ["thin_disk_scale_length_pc"] = ThinDiskScaleLengthPc,
            ["thick_disk_scale_length_pc"] = ThickDiskScaleLengthPc,
            ["thin_disk_scale_height_pc"] = ThinDiskScaleHeightPc,
            ["thick_disk_scale_height_pc"] = ThickDiskScaleHeightPc,
            ["bar_half_length_pc"] = BarHalfLengthPc,
            ["solar_galactocentric_radius_pc"] = SolarGalactocentricRadiusPc,
            ["circular_velocity_at_solar_radius_km_s"] = CircularVelocityAtSolarRadiusKmS,
            ["stellar_mass_solar"] = StellarMassSolar,
            ["bulge_radius_pc"] = BulgeRadiusPc,
            ["radius_pc"] = RadiusPc,
            ["ellipticity"] = Ellipticity,
            ["irregularity_scale"] = IrregularityScale,
            ["use_case_settings"] = UseCaseSettings.ToDictionary(),
            ["stellar_profile"] = StellarProfile.ToDictionary(),
            ["planetary_profile"] = PlanetaryProfile.ToDictionary(),
        };
    }

    /// <summary>
    /// Rebuilds a configuration object from a dictionary payload.
    /// </summary>
    public static GalaxyConfig? FromDictionary(Dictionary data)
    {
        if (data.Count == 0)
        {
            return null;
        }

        GalaxyConfig config = new GalaxyConfig
        {
            HaloMassLog10Solar = DomainDictionaryUtils.GetDouble(data, "halo_mass_log10_solar", 12.0),
            EnvironmentDensityIndex = DomainDictionaryUtils.GetDouble(data, "environment_density_index", 0.25),
            GhzInnerRadiusPc = DomainDictionaryUtils.GetDouble(data, "ghz_inner_radius_pc", 4000.0),
            GhzOuterRadiusPc = DomainDictionaryUtils.GetDouble(data, "ghz_outer_radius_pc", 12000.0),
            GhzTransitionWidthPc = DomainDictionaryUtils.GetDouble(data, "ghz_transition_width_pc", 2000.0),
            MetallicityGradientDexPerKpc = DomainDictionaryUtils.GetDouble(data, "metallicity_gradient_dex_per_kpc", -0.05),
            StarFormationEfficiency = DomainDictionaryUtils.GetDouble(data, "star_formation_efficiency", 0.1),
            NumArms = DomainDictionaryUtils.GetInt(data, "num_arms", 4),
            ArmPitchAngleDeg = DomainDictionaryUtils.GetDouble(data, "arm_pitch_angle_deg", 14.0),
            ArmAmplitude = DomainDictionaryUtils.GetDouble(data, "arm_amplitude", 0.65),
            BulgeIntensity = DomainDictionaryUtils.GetDouble(data, "bulge_intensity", 0.8),
            DiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "disk_scale_length_pc", 2600.0),
            StarDensityMultiplier = DomainDictionaryUtils.GetDouble(data, "star_density_multiplier", 1.0),
            DiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "disk_scale_height_pc", 300.0),
            ThinDiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "thin_disk_scale_length_pc", 2600.0),
            ThickDiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "thick_disk_scale_length_pc", 2000.0),
            ThinDiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "thin_disk_scale_height_pc", 300.0),
            ThickDiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "thick_disk_scale_height_pc", 900.0),
            BarHalfLengthPc = DomainDictionaryUtils.GetDouble(data, "bar_half_length_pc", 4500.0),
            SolarGalactocentricRadiusPc = DomainDictionaryUtils.GetDouble(data, "solar_galactocentric_radius_pc", 8200.0),
            CircularVelocityAtSolarRadiusKmS = DomainDictionaryUtils.GetDouble(data, "circular_velocity_at_solar_radius_km_s", 240.0),
            StellarMassSolar = DomainDictionaryUtils.GetDouble(data, "stellar_mass_solar", 5.0e10),
            BulgeRadiusPc = DomainDictionaryUtils.GetDouble(data, "bulge_radius_pc", 1500.0),
            RadiusPc = DomainDictionaryUtils.GetDouble(data, "radius_pc", 15000.0),
            Ellipticity = DomainDictionaryUtils.GetDouble(data, "ellipticity", 0.3),
            IrregularityScale = DomainDictionaryUtils.GetDouble(data, "irregularity_scale", 0.5),
        };

        if (data.ContainsKey("use_case_settings") && data["use_case_settings"].VariantType == Variant.Type.Dictionary)
        {
            config.UseCaseSettings = GenerationUseCaseSettings.FromDictionary((Dictionary)data["use_case_settings"]);
        }

        if (data.ContainsKey("stellar_profile") && data["stellar_profile"].VariantType == Variant.Type.Dictionary)
        {
            config.StellarProfile = StellarGenerationProfile.FromDictionary((Dictionary)data["stellar_profile"]);
        }
        else
        {
            config.StellarProfile = StellarGenerationProfile.CreateDefault();
        }

        if (data.ContainsKey("planetary_profile") && data["planetary_profile"].VariantType == Variant.Type.Dictionary)
        {
            config.PlanetaryProfile = PlanetaryGenerationProfile.FromDictionary((Dictionary)data["planetary_profile"]);
        }
        else
        {
            config.PlanetaryProfile = PlanetaryGenerationProfile.CreateDefault();
        }

        int typeValue = DomainDictionaryUtils.GetInt(data, "galaxy_type", (int)GalaxySpec.GalaxyType.Spiral);
        if (System.Enum.IsDefined(typeof(GalaxySpec.GalaxyType), typeValue))
        {
            config.Type = (GalaxySpec.GalaxyType)typeValue;
        }
        else
        {
            config.Type = GalaxySpec.GalaxyType.Spiral;
        }

        int subtypeModeValue = DomainDictionaryUtils.GetInt(data, "subtype_mode", (int)GalaxySubtypeMode.Automatic);
        if (System.Enum.IsDefined(typeof(GalaxySubtypeMode), subtypeModeValue))
        {
            config.SubtypeMode = (GalaxySubtypeMode)subtypeModeValue;
        }

        int barModeValue = DomainDictionaryUtils.GetInt(data, "bar_mode", (int)GalaxyBarMode.Auto);
        if (System.Enum.IsDefined(typeof(GalaxyBarMode), barModeValue))
        {
            config.BarMode = (GalaxyBarMode)barModeValue;
        }

        int armMechanismValue = DomainDictionaryUtils.GetInt(data, "arm_mechanism_preference", (int)GalaxyArmMechanism.Auto);
        if (System.Enum.IsDefined(typeof(GalaxyArmMechanism), armMechanismValue))
        {
            config.ArmMechanismPreference = (GalaxyArmMechanism)armMechanismValue;
        }

        if (!config.StellarProfile.IsValid())
        {
            config.StellarProfile = StellarGenerationProfile.CreateDefault();
        }

        if (!config.PlanetaryProfile.IsValid())
        {
            config.PlanetaryProfile = PlanetaryGenerationProfile.CreateDefault();
        }

        return config;
    }

    /// <summary>
    /// Returns the display label for the configured galaxy family.
    /// </summary>
    public string GetTypeName()
    {
        if (Type == GalaxySpec.GalaxyType.Spiral)
        {
            return "Spiral";
        }

        if (Type == GalaxySpec.GalaxyType.Elliptical)
        {
            return "Elliptical";
        }

        if (Type == GalaxySpec.GalaxyType.Lenticular)
        {
            return "Lenticular";
        }

        if (Type == GalaxySpec.GalaxyType.Irregular)
        {
            return "Irregular / Dwarf";
        }

        return "Unknown";
    }
}
