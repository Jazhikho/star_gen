using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Physical properties of a celestial body.
/// </summary>
public partial class PhysicalProps : RefCounted
{
    /// <summary>
    /// Gravitational constant in m^3 kg^-1 s^-2.
    /// </summary>
    public const double G = 6.674e-11;

    /// <summary>
    /// Mass in kilograms.
    /// </summary>
    public double MassKg;

    /// <summary>
    /// Radius in meters.
    /// </summary>
    public double RadiusM;

    /// <summary>
    /// Rotation period in seconds. Negative values indicate retrograde rotation.
    /// </summary>
    public double RotationPeriodS;

    /// <summary>
    /// Axial tilt in degrees.
    /// </summary>
    public double AxialTiltDeg;

    /// <summary>
    /// Oblateness factor.
    /// </summary>
    public double Oblateness;

    /// <summary>
    /// Magnetic dipole moment in Tesla * m^3.
    /// </summary>
    public double MagneticMoment;

    /// <summary>
    /// Internal heat flow in watts.
    /// </summary>
    public double InternalHeatWatts;

    /// <summary>
    /// Creates a new physical-properties component.
    /// </summary>
    public PhysicalProps(
        double massKg = 0.0,
        double radiusM = 0.0,
        double rotationPeriodS = 0.0,
        double axialTiltDeg = 0.0,
        double oblateness = 0.0,
        double magneticMoment = 0.0,
        double internalHeatWatts = 0.0)
    {
        MassKg = massKg;
        RadiusM = radiusM;
        RotationPeriodS = rotationPeriodS;
        AxialTiltDeg = axialTiltDeg;
        Oblateness = oblateness;
        MagneticMoment = magneticMoment;
        InternalHeatWatts = internalHeatWatts;
    }

    /// <summary>
    /// Calculates the volume in cubic meters.
    /// </summary>
    public double GetVolumeM3()
    {
        if (RadiusM <= 0.0)
        {
            return 0.0;
        }

        return (4.0 / 3.0) * Mathf.Pi * System.Math.Pow(RadiusM, 3.0);
    }

    /// <summary>
    /// Calculates the mean density in kg/m^3.
    /// </summary>
    public double GetDensityKgM3()
    {
        double volume = GetVolumeM3();
        if (volume <= 0.0)
        {
            return 0.0;
        }

        return MassKg / volume;
    }

    /// <summary>
    /// Calculates surface gravity in m/s^2.
    /// </summary>
    public double GetSurfaceGravityMS2()
    {
        if (RadiusM <= 0.0)
        {
            return 0.0;
        }

        return G * MassKg / (RadiusM * RadiusM);
    }

    /// <summary>
    /// Calculates escape velocity in m/s.
    /// </summary>
    public double GetEscapeVelocityMS()
    {
        if (RadiusM <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Sqrt((2.0 * G * MassKg) / RadiusM);
    }

    /// <summary>
    /// Returns the equatorial radius accounting for oblateness.
    /// </summary>
    public double GetEquatorialRadiusM()
    {
        if (Oblateness <= 0.0)
        {
            return RadiusM;
        }

        return RadiusM / (1.0 - Oblateness);
    }

    /// <summary>
    /// Returns the polar radius accounting for oblateness.
    /// </summary>
    public double GetPolarRadiusM()
    {
        if (Oblateness <= 0.0)
        {
            return RadiusM;
        }

        return RadiusM * (1.0 - Oblateness);
    }

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["mass_kg"] = MassKg,
            ["radius_m"] = RadiusM,
            ["rotation_period_s"] = RotationPeriodS,
            ["axial_tilt_deg"] = AxialTiltDeg,
            ["oblateness"] = Oblateness,
            ["magnetic_moment"] = MagneticMoment,
            ["internal_heat_watts"] = InternalHeatWatts,
        };
    }

    /// <summary>
    /// Creates a physical-properties component from a dictionary.
    /// </summary>
    public static PhysicalProps FromDictionary(Dictionary data)
    {
        return new PhysicalProps(
            GetDouble(data, "mass_kg", 0.0),
            GetDouble(data, "radius_m", 0.0),
            GetDouble(data, "rotation_period_s", 0.0),
            GetDouble(data, "axial_tilt_deg", 0.0),
            GetDouble(data, "oblateness", 0.0),
            GetDouble(data, "magnetic_moment", 0.0),
            GetDouble(data, "internal_heat_watts", 0.0));
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        return data.ContainsKey(key) ? (double)data[key] : fallback;
    }
}
