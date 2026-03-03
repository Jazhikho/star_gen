using Godot;
using Godot.Collections;
using StarGen.Domain.Constants;

namespace StarGen.Domain.Systems;

/// <summary>
/// Handles solar-system serialization and deserialization.
/// </summary>
public static class SystemSerializer
{
    /// <summary>
    /// Serializes a solar system to a dictionary payload.
    /// </summary>
    public static Dictionary ToDictionary(SolarSystem system)
    {
        Dictionary data = system.ToDictionary();
        if (!data.ContainsKey("schema_version"))
        {
            data["schema_version"] = Versions.SchemaVersion;
        }

        if (!data.ContainsKey("generator_version"))
        {
            data["generator_version"] = Versions.GeneratorVersion;
        }

        if (!data.ContainsKey("type"))
        {
            data["type"] = "solar_system";
        }

        return data;
    }

    /// <summary>
    /// Deserializes a dictionary payload to a solar system.
    /// </summary>
    public static SolarSystem? FromDictionary(Dictionary data)
    {
        if (data.Count == 0)
        {
            return null;
        }

        string dataType = GetString(data, "type", string.Empty);
        if (dataType != "solar_system" && dataType != string.Empty)
        {
            return null;
        }

        return SolarSystem.FromDictionary(data);
    }

    /// <summary>
    /// Serializes a solar system to JSON.
    /// </summary>
    public static string ToJson(SolarSystem system, bool pretty = true)
    {
        Dictionary data = ToDictionary(system);
        return pretty ? Json.Stringify(data, "\t") : Json.Stringify(data);
    }

    /// <summary>
    /// Deserializes a solar system from JSON.
    /// </summary>
    public static SolarSystem? FromJson(string jsonString)
    {
        Variant parsed = Json.ParseString(jsonString);
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return FromDictionary((Dictionary)parsed);
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
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }
}
