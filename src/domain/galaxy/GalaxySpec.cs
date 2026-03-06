using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Specification for a procedurally generated galaxy.
/// </summary>
public partial class GalaxySpec : RefCounted
{
    /// <summary>
    /// Supported galaxy morphologies.
    /// </summary>
    public enum GalaxyType
    {
        Spiral,
        Elliptical,
        Irregular,
    }

    /// <summary>
    /// Master seed for the galaxy.
    /// </summary>
    public int GalaxySeed { get; set; }

    /// <summary>
    /// Morphological type.
    /// </summary>
    public GalaxyType Type { get; set; } = GalaxyType.Spiral;

    /// <summary>
    /// Radius of the galactic disk in parsecs.
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
    /// Ellipticity for elliptical galaxies.
    /// </summary>
    public double Ellipticity { get; set; } = 0.3;

    /// <summary>
    /// Irregularity scale for irregular galaxies.
    /// </summary>
    public double IrregularityScale { get; set; } = 0.5;

    /// <summary>
    /// Creates a Milky-Way-like spiral galaxy specification.
    /// </summary>
    public static GalaxySpec CreateMilkyWay(int galaxySeed)
    {
        return new GalaxySpec
        {
            GalaxySeed = galaxySeed,
            Type = GalaxyType.Spiral,
            RadiusPc = 15000.0,
            HeightPc = 1000.0,
            NumArms = 4,
            ArmPitchAngleDeg = 14.0,
            ArmWidth = 0.4,
            ArmAmplitude = 0.65,
            BulgeRadiusPc = 1500.0,
            BulgeHeightPc = 800.0,
            BulgeIntensity = 0.8,
            DiskScaleLengthPc = 4000.0,
            DiskScaleHeightPc = 300.0,
        };
    }

    /// <summary>
    /// Creates a galaxy specification from a configuration object and seed.
    /// Derived quantities:
    /// <list type="bullet">
    ///   <item>
    ///     <term>HeightPc</term>
    ///     <description>
    ///       Set to RadiusPc / 15. The Milky Way disk has a radius-to-half-height ratio of ~15:1
    ///       for the total stellar distribution (Bland-Hawthorn &amp; Gerhard 2016, ARA&amp;A 54).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>BulgeHeightPc</term>
    ///     <description>
    ///       Set to BulgeRadiusPc × 0.53. Milky Way bar/bulge observations give a vertical-to-planar
    ///       half-axis ratio in the range 0.4–0.6 (Wegg, Gerhard &amp; Portail 2015, MNRAS 450; Portail
    ///       et al. 2017, MNRAS 465). A factor of 0.53 represents a moderately flattened prolate bulge.
    ///     </description>
    ///   </item>
    /// </list>
    /// </summary>
    public static GalaxySpec CreateFromConfig(GalaxyConfig? config, int galaxySeed)
    {
        GalaxyConfig effectiveConfig = config ?? GalaxyConfig.CreateDefault();
        return new GalaxySpec
        {
            GalaxySeed = galaxySeed,
            Type = effectiveConfig.Type,
            RadiusPc = effectiveConfig.RadiusPc,
            HeightPc = effectiveConfig.RadiusPc / 15.0,
            NumArms = effectiveConfig.NumArms,
            ArmPitchAngleDeg = effectiveConfig.ArmPitchAngleDeg,
            ArmWidth = 0.4,
            ArmAmplitude = effectiveConfig.ArmAmplitude,
            BulgeRadiusPc = effectiveConfig.BulgeRadiusPc,
            BulgeHeightPc = effectiveConfig.BulgeRadiusPc * 0.53,
            BulgeIntensity = effectiveConfig.BulgeIntensity,
            DiskScaleLengthPc = effectiveConfig.DiskScaleLengthPc,
            DiskScaleHeightPc = effectiveConfig.DiskScaleHeightPc,
            Ellipticity = effectiveConfig.Ellipticity,
            IrregularityScale = effectiveConfig.IrregularityScale,
        };
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
            ["ellipticity"] = Ellipticity,
            ["irregularity_scale"] = IrregularityScale,
        };
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static GalaxySpec FromDictionary(Dictionary data)
    {
        GalaxySpec spec = new()
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
            DiskScaleLengthPc = DomainDictionaryUtils.GetDouble(data, "disk_scale_length_pc", 4000.0),
            DiskScaleHeightPc = DomainDictionaryUtils.GetDouble(data, "disk_scale_height_pc", 300.0),
            Ellipticity = DomainDictionaryUtils.GetDouble(data, "ellipticity", 0.3),
            IrregularityScale = DomainDictionaryUtils.GetDouble(data, "irregularity_scale", 0.5),
        };

        int typeValue = DomainDictionaryUtils.GetInt(data, "galaxy_type", (int)GalaxyType.Spiral);
        if (System.Enum.IsDefined(typeof(GalaxyType), typeValue))
        {
            spec.Type = (GalaxyType)typeValue;
        }
        else
        {
            spec.Type = GalaxyType.Spiral;
        }
        return spec;
    }

}
