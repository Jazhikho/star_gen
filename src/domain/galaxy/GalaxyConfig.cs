using Godot;
using Godot.Collections;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Configuration values for galaxy generation presets and editor-facing tuning.
/// </summary>
public partial class GalaxyConfig : RefCounted
{
    /// <summary>
    /// Galaxy morphological type.
    /// </summary>
    public GalaxySpec.GalaxyType Type = GalaxySpec.GalaxyType.Spiral;

    /// <summary>
    /// Number of spiral arms for spiral galaxies.
    /// </summary>
    public int NumArms = 4;

    /// <summary>
    /// Pitch angle of the spiral arms in degrees.
    /// </summary>
    public double ArmPitchAngleDeg = 14.0;

    /// <summary>
    /// Density contrast of the spiral arms.
    /// </summary>
    public double ArmAmplitude = 0.65;

    /// <summary>
    /// Central bulge intensity multiplier.
    /// </summary>
    public double BulgeIntensity = 0.8;

    /// <summary>
    /// Exponential disk scale length in parsecs.
    /// </summary>
    public double DiskScaleLengthPc = 4000.0;

    /// <summary>
    /// Overall star density multiplier.
    /// </summary>
    public double StarDensityMultiplier = 1.0;

    /// <summary>
    /// Exponential disk scale height in parsecs.
    /// </summary>
    public double DiskScaleHeightPc = 300.0;

    /// <summary>
    /// Central bulge radius in parsecs.
    /// </summary>
    public double BulgeRadiusPc = 1500.0;

    /// <summary>
    /// Galaxy radius in parsecs.
    /// </summary>
    public double RadiusPc = 15000.0;

    /// <summary>
    /// Ellipticity for elliptical galaxies.
    /// </summary>
    public double Ellipticity = 0.3;

    /// <summary>
    /// Noise frequency scaling for irregular galaxies.
    /// </summary>
    public double IrregularityScale = 0.5;

    /// <summary>
    /// Creates a default configuration.
    /// </summary>
    public static GalaxyConfig CreateDefault()
    {
        return new GalaxyConfig();
    }

    /// <summary>
    /// Creates a Milky-Way-like configuration.
    /// </summary>
    public static GalaxyConfig CreateMilkyWay()
    {
        return new GalaxyConfig
        {
            Type = GalaxySpec.GalaxyType.Spiral,
            NumArms = 4,
            ArmPitchAngleDeg = 14.0,
            ArmAmplitude = 0.65,
            BulgeIntensity = 0.8,
            DiskScaleLengthPc = 4000.0,
            StarDensityMultiplier = 1.0,
            DiskScaleHeightPc = 300.0,
            BulgeRadiusPc = 1500.0,
            RadiusPc = 15000.0,
        };
    }

    /// <summary>
    /// Applies the configuration values to a galaxy specification.
    /// </summary>
    public void ApplyToSpec(GalaxySpec spec)
    {
        spec.Type = Type;
        spec.NumArms = NumArms;
        spec.ArmPitchAngleDeg = ArmPitchAngleDeg;
        spec.ArmAmplitude = ArmAmplitude;
        spec.BulgeIntensity = BulgeIntensity;
        spec.DiskScaleLengthPc = DiskScaleLengthPc;
        spec.DiskScaleHeightPc = DiskScaleHeightPc;
        spec.BulgeRadiusPc = BulgeRadiusPc;
        spec.RadiusPc = RadiusPc;
        spec.Ellipticity = Ellipticity;
        spec.IrregularityScale = IrregularityScale;
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

        if (NumArms < 2 || NumArms > 6)
        {
            return false;
        }

        if (ArmPitchAngleDeg < 10.0 || ArmPitchAngleDeg > 30.0)
        {
            return false;
        }

        if (ArmAmplitude < 0.3 || ArmAmplitude > 0.9)
        {
            return false;
        }

        if (BulgeIntensity < 0.3 || BulgeIntensity > 1.2)
        {
            return false;
        }

        if (DiskScaleLengthPc < 2000.0 || DiskScaleLengthPc > 6000.0)
        {
            return false;
        }

        if (StarDensityMultiplier < 0.5 || StarDensityMultiplier > 2.0)
        {
            return false;
        }

        if (DiskScaleHeightPc < 200.0 || DiskScaleHeightPc > 500.0)
        {
            return false;
        }

        if (BulgeRadiusPc < 1000.0 || BulgeRadiusPc > 2500.0)
        {
            return false;
        }

        if (RadiusPc < 10000.0 || RadiusPc > 25000.0)
        {
            return false;
        }

        if (Ellipticity < 0.0 || Ellipticity > 0.7)
        {
            return false;
        }

        if (IrregularityScale < 0.1 || IrregularityScale > 1.0)
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
            ["num_arms"] = NumArms,
            ["arm_pitch_angle_deg"] = ArmPitchAngleDeg,
            ["arm_amplitude"] = ArmAmplitude,
            ["bulge_intensity"] = BulgeIntensity,
            ["disk_scale_length_pc"] = DiskScaleLengthPc,
            ["star_density_multiplier"] = StarDensityMultiplier,
            ["disk_scale_height_pc"] = DiskScaleHeightPc,
            ["bulge_radius_pc"] = BulgeRadiusPc,
            ["radius_pc"] = RadiusPc,
            ["ellipticity"] = Ellipticity,
            ["irregularity_scale"] = IrregularityScale,
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

        GalaxyConfig config = new()
        {
            NumArms = GetInt(data, "num_arms", 4),
            ArmPitchAngleDeg = GetDouble(data, "arm_pitch_angle_deg", 14.0),
            ArmAmplitude = GetDouble(data, "arm_amplitude", 0.65),
            BulgeIntensity = GetDouble(data, "bulge_intensity", 0.8),
            DiskScaleLengthPc = GetDouble(data, "disk_scale_length_pc", 4000.0),
            StarDensityMultiplier = GetDouble(data, "star_density_multiplier", 1.0),
            DiskScaleHeightPc = GetDouble(data, "disk_scale_height_pc", 300.0),
            BulgeRadiusPc = GetDouble(data, "bulge_radius_pc", 1500.0),
            RadiusPc = GetDouble(data, "radius_pc", 15000.0),
            Ellipticity = GetDouble(data, "ellipticity", 0.3),
            IrregularityScale = GetDouble(data, "irregularity_scale", 0.5),
        };

        int typeValue = GetInt(data, "galaxy_type", (int)GalaxySpec.GalaxyType.Spiral);
        config.Type = System.Enum.IsDefined(typeof(GalaxySpec.GalaxyType), typeValue)
            ? (GalaxySpec.GalaxyType)typeValue
            : GalaxySpec.GalaxyType.Spiral;
        return config;
    }

    /// <summary>
    /// Returns the display label for the configured galaxy type.
    /// </summary>
    public string GetTypeName()
    {
        return Type switch
        {
            GalaxySpec.GalaxyType.Spiral => "Spiral",
            GalaxySpec.GalaxyType.Elliptical => "Elliptical",
            GalaxySpec.GalaxyType.Irregular => "Irregular",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            _ => fallback,
        };
    }
}
