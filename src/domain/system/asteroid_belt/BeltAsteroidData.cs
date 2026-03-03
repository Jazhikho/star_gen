using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems.AsteroidFields;

/// <summary>
/// Generated sample data for one asteroid in a belt field.
/// </summary>
public partial class BeltAsteroidData : RefCounted
{
    public bool IsMajor;
    public string BodyId = string.Empty;
    public int AsteroidType = -1;
    public Vector3 PositionAu = Vector3.Zero;
    public double SemiMajorAxisAu;
    public double Eccentricity;
    public double InclinationRad;
    public double LongitudeAscendingNodeRad;
    public double ArgumentPeriapsisRad;
    public double TrueAnomalyRad;
    public double BodyRadiusKm = 1.0;

    /// <summary>
    /// Converts the sampled asteroid to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["is_major"] = IsMajor,
            ["body_id"] = BodyId,
            ["asteroid_type"] = AsteroidType,
            ["position_au"] = PositionAu,
            ["semi_major_axis_au"] = SemiMajorAxisAu,
            ["eccentricity"] = Eccentricity,
            ["inclination_rad"] = InclinationRad,
            ["longitude_ascending_node_rad"] = LongitudeAscendingNodeRad,
            ["argument_periapsis_rad"] = ArgumentPeriapsisRad,
            ["true_anomaly_rad"] = TrueAnomalyRad,
            ["body_radius_km"] = BodyRadiusKm,
        };
    }

    /// <summary>
    /// Rebuilds sampled asteroid data from a dictionary payload.
    /// </summary>
    public static BeltAsteroidData FromDictionary(Dictionary data)
    {
        return new BeltAsteroidData
        {
            IsMajor = GetBool(data, "is_major", false),
            BodyId = GetString(data, "body_id", string.Empty),
            AsteroidType = GetInt(data, "asteroid_type", -1),
            PositionAu = GetVector3(data, "position_au", Vector3.Zero),
            SemiMajorAxisAu = GetDouble(data, "semi_major_axis_au", 0.0),
            Eccentricity = GetDouble(data, "eccentricity", 0.0),
            InclinationRad = GetDouble(data, "inclination_rad", 0.0),
            LongitudeAscendingNodeRad = GetDouble(data, "longitude_ascending_node_rad", 0.0),
            ArgumentPeriapsisRad = GetDouble(data, "argument_periapsis_rad", 0.0),
            TrueAnomalyRad = GetDouble(data, "true_anomaly_rad", 0.0),
            BodyRadiusKm = GetDouble(data, "body_radius_km", 1.0),
        };
    }

    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool ? (bool)data[key] : fallback;
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

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    private static Vector3 GetVector3(Dictionary data, string key, Vector3 fallback)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Vector3 ? (Vector3)data[key] : fallback;
    }
}
