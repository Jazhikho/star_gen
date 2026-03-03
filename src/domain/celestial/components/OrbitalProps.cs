using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Orbital parameters of a celestial body.
/// </summary>
public partial class OrbitalProps : RefCounted
{
    private const double G = 6.674e-11;

    /// <summary>
    /// Semi-major axis in meters.
    /// </summary>
    public double SemiMajorAxisM;

    /// <summary>
    /// Orbital eccentricity.
    /// </summary>
    public double Eccentricity;

    /// <summary>
    /// Orbital inclination in degrees.
    /// </summary>
    public double InclinationDeg;

    /// <summary>
    /// Longitude of ascending node in degrees.
    /// </summary>
    public double LongitudeOfAscendingNodeDeg;

    /// <summary>
    /// Argument of periapsis in degrees.
    /// </summary>
    public double ArgumentOfPeriapsisDeg;

    /// <summary>
    /// Mean anomaly at epoch in degrees.
    /// </summary>
    public double MeanAnomalyDeg;

    /// <summary>
    /// ID of the parent body this object orbits.
    /// </summary>
    public string ParentId;

    /// <summary>
    /// Creates a new orbital-properties component.
    /// </summary>
    public OrbitalProps(
        double semiMajorAxisM = 0.0,
        double eccentricity = 0.0,
        double inclinationDeg = 0.0,
        double longitudeOfAscendingNodeDeg = 0.0,
        double argumentOfPeriapsisDeg = 0.0,
        double meanAnomalyDeg = 0.0,
        string parentId = "")
    {
        SemiMajorAxisM = semiMajorAxisM;
        Eccentricity = eccentricity;
        InclinationDeg = inclinationDeg;
        LongitudeOfAscendingNodeDeg = longitudeOfAscendingNodeDeg;
        ArgumentOfPeriapsisDeg = argumentOfPeriapsisDeg;
        MeanAnomalyDeg = meanAnomalyDeg;
        ParentId = parentId;
    }

    /// <summary>
    /// Calculates the periapsis distance in meters.
    /// </summary>
    public double GetPeriapsisM() => SemiMajorAxisM * (1.0 - Eccentricity);

    /// <summary>
    /// Calculates the apoapsis distance in meters.
    /// </summary>
    public double GetApoapsisM() => SemiMajorAxisM * (1.0 + Eccentricity);

    /// <summary>
    /// Calculates the orbital period in seconds for the supplied parent mass.
    /// </summary>
    public double GetOrbitalPeriodS(double parentMassKg)
    {
        if (SemiMajorAxisM <= 0.0 || parentMassKg <= 0.0)
        {
            return 0.0;
        }

        return 2.0 * Mathf.Pi * System.Math.Sqrt(System.Math.Pow(SemiMajorAxisM, 3.0) / (G * parentMassKg));
    }

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["semi_major_axis_m"] = SemiMajorAxisM,
            ["eccentricity"] = Eccentricity,
            ["inclination_deg"] = InclinationDeg,
            ["longitude_of_ascending_node_deg"] = LongitudeOfAscendingNodeDeg,
            ["argument_of_periapsis_deg"] = ArgumentOfPeriapsisDeg,
            ["mean_anomaly_deg"] = MeanAnomalyDeg,
            ["parent_id"] = ParentId,
        };
    }

    /// <summary>
    /// Creates an orbital-properties component from a dictionary.
    /// </summary>
    public static OrbitalProps FromDictionary(Dictionary data)
    {
        return new OrbitalProps(
            GetDouble(data, "semi_major_axis_m", 0.0),
            GetDouble(data, "eccentricity", 0.0),
            GetDouble(data, "inclination_deg", 0.0),
            GetDouble(data, "longitude_of_ascending_node_deg", 0.0),
            GetDouble(data, "argument_of_periapsis_deg", 0.0),
            GetDouble(data, "mean_anomaly_deg", 0.0),
            GetString(data, "parent_id", ""));
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        return data.ContainsKey(key) ? (double)data[key] : fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        return data.ContainsKey(key) ? (string)data[key] : fallback;
    }
}
