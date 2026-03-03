using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems.AsteroidFields;

/// <summary>
/// Specification for generating or rendering a dense asteroid-belt field.
/// </summary>
public partial class BeltFieldSpec : RefCounted
{
    public double InnerRadiusAu = 2.0;
    public double OuterRadiusAu = 3.5;
    public int AsteroidCount = 1000;
    public double MaxInclinationDeg = 20.0;
    public double MaxEccentricity = 0.25;
    public double MinBodyRadiusKm = 0.5;
    public double MaxBodyRadiusKm = 500.0;
    public double SizePowerLawExponent = 2.5;
    public double RadialConcentration = 2.0;
    public Array<double> GapCentersAu = new();
    public Array<double> GapHalfWidthsAu = new();
    public int ClusterCount;
    public Array<double> ClusterLongitudesRad = new();
    public double ClusterConcentration = 3.0;
    public double ClusterFraction = 0.3;
    public Array<BeltMajorAsteroidInput> MajorAsteroidInputs = new();

    /// <summary>
    /// Converts the specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> majorInputs = new();
        foreach (BeltMajorAsteroidInput input in MajorAsteroidInputs)
        {
            majorInputs.Add(input.ToDictionary());
        }

        return new Dictionary
        {
            ["inner_radius_au"] = InnerRadiusAu,
            ["outer_radius_au"] = OuterRadiusAu,
            ["asteroid_count"] = AsteroidCount,
            ["max_inclination_deg"] = MaxInclinationDeg,
            ["max_eccentricity"] = MaxEccentricity,
            ["min_body_radius_km"] = MinBodyRadiusKm,
            ["max_body_radius_km"] = MaxBodyRadiusKm,
            ["size_power_law_exponent"] = SizePowerLawExponent,
            ["radial_concentration"] = RadialConcentration,
            ["gap_centers_au"] = CloneDoubleArray(GapCentersAu),
            ["gap_half_widths_au"] = CloneDoubleArray(GapHalfWidthsAu),
            ["cluster_count"] = ClusterCount,
            ["cluster_longitudes_rad"] = CloneDoubleArray(ClusterLongitudesRad),
            ["cluster_concentration"] = ClusterConcentration,
            ["cluster_fraction"] = ClusterFraction,
            ["major_asteroid_inputs"] = majorInputs,
        };
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static BeltFieldSpec FromDictionary(Dictionary data)
    {
        BeltFieldSpec spec = new()
        {
            InnerRadiusAu = GetDouble(data, "inner_radius_au", 2.0),
            OuterRadiusAu = GetDouble(data, "outer_radius_au", 3.5),
            AsteroidCount = GetInt(data, "asteroid_count", 1000),
            MaxInclinationDeg = GetDouble(data, "max_inclination_deg", 20.0),
            MaxEccentricity = GetDouble(data, "max_eccentricity", 0.25),
            MinBodyRadiusKm = GetDouble(data, "min_body_radius_km", 0.5),
            MaxBodyRadiusKm = GetDouble(data, "max_body_radius_km", 500.0),
            SizePowerLawExponent = GetDouble(data, "size_power_law_exponent", 2.5),
            RadialConcentration = GetDouble(data, "radial_concentration", 2.0),
            ClusterCount = GetInt(data, "cluster_count", 0),
            ClusterConcentration = GetDouble(data, "cluster_concentration", 3.0),
            ClusterFraction = GetDouble(data, "cluster_fraction", 0.3),
        };

        PopulateDoubleArray(data, "gap_centers_au", spec.GapCentersAu);
        PopulateDoubleArray(data, "gap_half_widths_au", spec.GapHalfWidthsAu);
        PopulateDoubleArray(data, "cluster_longitudes_rad", spec.ClusterLongitudesRad);

        if (data.ContainsKey("major_asteroid_inputs") && data["major_asteroid_inputs"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["major_asteroid_inputs"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    spec.MajorAsteroidInputs.Add(BeltMajorAsteroidInput.FromDictionary((Dictionary)value));
                }
            }
        }

        return spec;
    }

    private static Array<double> CloneDoubleArray(Array<double> source)
    {
        Array<double> clone = new();
        foreach (double value in source)
        {
            clone.Add(value);
        }

        return clone;
    }

    private static void PopulateDoubleArray(Dictionary data, string key, Array<double> target)
    {
        if (!data.ContainsKey(key) || data[key].VariantType != Variant.Type.Array)
        {
            return;
        }

        foreach (Variant value in (Array)data[key])
        {
            switch (value.VariantType)
            {
                case Variant.Type.Float:
                    target.Add((double)value);
                    break;
                case Variant.Type.Int:
                    target.Add((int)value);
                    break;
            }
        }
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
