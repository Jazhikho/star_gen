using Godot;
using Godot.Collections;
using StarGen.Domain.Math;

namespace StarGen.Domain.Systems;

/// <summary>
/// Asteroid-belt region within a solar system.
/// </summary>
public partial class AsteroidBelt : RefCounted
{
    /// <summary>
    /// Belt composition categories.
    /// </summary>
    public enum Composition
    {
        Rocky,
        Icy,
        Mixed,
        Metallic,
    }

    /// <summary>
    /// Compatibility enum alias for legacy tests/API.
    /// </summary>
    public enum CompositionType
    {
        Rocky,
        Icy,
        Mixed,
        Metallic,
    }

    /// <summary>
    /// Unique belt identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Display name for the belt.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Identifier of the orbit host this belt orbits.
    /// </summary>
    public string OrbitHostId = string.Empty;

    /// <summary>
    /// Inner edge of the belt in meters.
    /// </summary>
    public double InnerRadiusM;

    /// <summary>
    /// Outer edge of the belt in meters.
    /// </summary>
    public double OuterRadiusM;

    /// <summary>
    /// Total estimated belt mass in kilograms.
    /// </summary>
    public double TotalMassKg;

    /// <summary>
    /// Primary belt composition.
    /// </summary>
    public Composition PrimaryComposition = Composition.Rocky;

    /// <summary>
    /// Identifiers of the largest asteroids associated with the belt.
    /// </summary>
    public Array<string> MajorAsteroidIds = new();

    /// <summary>
    /// Creates a new asteroid belt.
    /// </summary>
    public AsteroidBelt(string id = "", string name = "")
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Returns the belt width in meters.
    /// </summary>
    public double GetWidthM()
    {
        return System.Math.Max(0.0, OuterRadiusM - InnerRadiusM);
    }

    /// <summary>
    /// Returns the belt width in astronomical units.
    /// </summary>
    public double GetWidthAu()
    {
        return GetWidthM() / Units.AuMeters;
    }

    /// <summary>
    /// Returns the belt center in meters.
    /// </summary>
    public double GetCenterM()
    {
        return (InnerRadiusM + OuterRadiusM) / 2.0;
    }

    /// <summary>
    /// Returns the belt center in astronomical units.
    /// </summary>
    public double GetCenterAu()
    {
        return GetCenterM() / Units.AuMeters;
    }

    /// <summary>
    /// Returns the display label for the current composition.
    /// </summary>
    public string GetCompositionString()
    {
        return PrimaryComposition switch
        {
            Composition.Rocky => "Rocky",
            Composition.Icy => "Icy",
            Composition.Mixed => "Mixed",
            Composition.Metallic => "Metallic",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Returns the number of tracked major asteroids.
    /// </summary>
    public int GetMajorAsteroidCount()
    {
        return MajorAsteroidIds.Count;
    }

    /// <summary>
    /// Converts a composition enum to its serialized token.
    /// </summary>
    public static string CompositionToString(Composition composition)
    {
        return composition switch
        {
            Composition.Rocky => "rocky",
            Composition.Icy => "icy",
            Composition.Mixed => "mixed",
            Composition.Metallic => "metallic",
            _ => "rocky",
        };
    }

    /// <summary>
    /// Compatibility overload for legacy enum alias.
    /// </summary>
    public static string CompositionToString(CompositionType composition)
    {
        return CompositionToString((Composition)composition);
    }

    /// <summary>
    /// Parses a serialized composition token.
    /// </summary>
    public static CompositionType StringToComposition(string compositionName)
    {
        return compositionName.ToLowerInvariant() switch
        {
            "rocky" => CompositionType.Rocky,
            "icy" => CompositionType.Icy,
            "mixed" => CompositionType.Mixed,
            "metallic" => CompositionType.Metallic,
            _ => CompositionType.Rocky,
        };
    }

    /// <summary>
    /// Parser returning the primary enum type used internally.
    /// </summary>
    public static Composition StringToPrimaryComposition(string compositionName)
    {
        return (Composition)StringToComposition(compositionName);
    }

    /// <summary>
    /// Converts the belt to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<string> asteroidIds = new();
        foreach (string asteroidId in MajorAsteroidIds)
        {
            asteroidIds.Add(asteroidId);
        }

        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["orbit_host_id"] = OrbitHostId,
            ["inner_radius_m"] = InnerRadiusM,
            ["outer_radius_m"] = OuterRadiusM,
            ["total_mass_kg"] = TotalMassKg,
            ["composition"] = CompositionToString(PrimaryComposition),
            ["major_asteroid_ids"] = asteroidIds,
        };
    }

    /// <summary>
    /// Creates a belt from a dictionary payload.
    /// </summary>
    public static AsteroidBelt FromDictionary(Dictionary data)
    {
        AsteroidBelt belt = new(
            GetString(data, "id", string.Empty),
            GetString(data, "name", string.Empty));
        belt.OrbitHostId = GetString(data, "orbit_host_id", string.Empty);
        belt.InnerRadiusM = GetDouble(data, "inner_radius_m", 0.0);
        belt.OuterRadiusM = GetDouble(data, "outer_radius_m", 0.0);
        belt.TotalMassKg = GetDouble(data, "total_mass_kg", 0.0);
        belt.PrimaryComposition = (Composition)StringToComposition(GetString(data, "composition", "rocky"));

        if (data.ContainsKey("major_asteroid_ids") && data["major_asteroid_ids"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["major_asteroid_ids"])
            {
                if (value.VariantType == Variant.Type.String)
                {
                    belt.MajorAsteroidIds.Add((string)value);
                }
            }
        }

        return belt;
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
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
            Variant.Type.String => TryParseDouble((string)value, fallback),
            _ => fallback,
        };
    }

    private static double TryParseDouble(string s, double fallback)
    {
        if (double.TryParse(s, out double parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
