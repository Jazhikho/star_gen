using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems.AsteroidFields;

/// <summary>
/// Input orbital and physical data for a major asteroid in a rendered belt field.
/// </summary>
public partial class BeltMajorAsteroidInput : RefCounted
{
    public string BodyId = string.Empty;
    public double SemiMajorAxisM;
    public double Eccentricity;
    public double InclinationDeg;
    public double LongitudeAscendingNodeDeg;
    public double ArgumentPeriapsisDeg;
    public double MeanAnomalyDeg;
    public double BodyRadiusKm = 100.0;
    public int AsteroidType = -1;

    /// <summary>
    /// Converts this input to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["body_id"] = BodyId,
            ["semi_major_axis_m"] = SemiMajorAxisM,
            ["eccentricity"] = Eccentricity,
            ["inclination_deg"] = InclinationDeg,
            ["longitude_ascending_node_deg"] = LongitudeAscendingNodeDeg,
            ["argument_periapsis_deg"] = ArgumentPeriapsisDeg,
            ["mean_anomaly_deg"] = MeanAnomalyDeg,
            ["body_radius_km"] = BodyRadiusKm,
            ["asteroid_type"] = AsteroidType,
        };
    }

    /// <summary>
    /// Rebuilds an input from a dictionary payload.
    /// </summary>
    public static BeltMajorAsteroidInput FromDictionary(Dictionary data)
    {
        return new BeltMajorAsteroidInput
        {
            BodyId = GetString(data, "body_id", string.Empty),
            SemiMajorAxisM = GetDouble(data, "semi_major_axis_m", 0.0),
            Eccentricity = GetDouble(data, "eccentricity", 0.0),
            InclinationDeg = GetDouble(data, "inclination_deg", 0.0),
            LongitudeAscendingNodeDeg = GetDouble(data, "longitude_ascending_node_deg", 0.0),
            ArgumentPeriapsisDeg = GetDouble(data, "argument_periapsis_deg", 0.0),
            MeanAnomalyDeg = GetDouble(data, "mean_anomaly_deg", 0.0),
            BodyRadiusKm = GetDouble(data, "body_radius_km", 100.0),
            AsteroidType = GetInt(data, "asteroid_type", -1),
        };
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.String)
        {
            return (string)value;
        }

        return fallback;
    }

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
