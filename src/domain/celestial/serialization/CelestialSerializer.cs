using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Constants;
using StarGen.Domain.Population;

namespace StarGen.Domain.Celestial.Serialization;

/// <summary>
/// Handles serialization and deserialization of celestial bodies.
/// </summary>
public static class CelestialSerializer
{
    /// <summary>
    /// Serializes a celestial body to a dictionary.
    /// </summary>
    public static Dictionary ToDictionary(CelestialBody body)
    {
        Dictionary data = new()
        {
            ["schema_version"] = Versions.SchemaVersion,
            ["id"] = body.Id,
            ["name"] = body.Name,
            ["type"] = body.GetTypeString(),
            ["physical"] = body.Physical.ToDictionary(),
        };

        if (body.HasOrbital())
        {
            data["orbital"] = body.Orbital!.ToDictionary();
        }

        if (body.HasStellar())
        {
            data["stellar"] = body.Stellar!.ToDictionary();
        }

        if (body.HasSurface())
        {
            data["surface"] = body.Surface!.ToDictionary();
        }

        if (body.HasAtmosphere())
        {
            data["atmosphere"] = body.Atmosphere!.ToDictionary();
        }

        if (body.HasRingSystem())
        {
            data["ring_system"] = body.RingSystem!.ToDictionary();
        }

        if (body.HasPopulationData())
        {
            if (body.PopulationData is SerializedPopulationData serializedPopulationData)
            {
                data["population_data"] = serializedPopulationData.ToDictionary();
            }
            else if (body.PopulationData!.HasMethod("to_dict"))
            {
                data["population_data"] = body.PopulationData.Call("to_dict");
            }
        }

        if (body.Provenance != null)
        {
            data["provenance"] = body.Provenance.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Deserializes a dictionary to a celestial body.
    /// </summary>
    public static CelestialBody? FromDictionary(Dictionary data)
    {
        if (data.Count == 0)
        {
            return null;
        }

        CelestialType.Type type = CelestialType.Type.Planet;
        if (data.ContainsKey("type"))
        {
            string typeName = (string)data["type"];
            if (CelestialType.TryParse(typeName, out CelestialType.Type parsedType))
            {
                type = parsedType;
            }
        }

        Dictionary physicalData = data.ContainsKey("physical") ? (Dictionary)data["physical"] : new Dictionary();
        PhysicalProps physical = PhysicalProps.FromDictionary(physicalData);

        Provenance? provenance = null;
        if (data.ContainsKey("provenance"))
        {
            provenance = Provenance.FromDictionary((Dictionary)data["provenance"]);
        }

        CelestialBody body = new(
            GetString(data, "id", ""),
            GetString(data, "name", ""),
            type,
            physical,
            provenance);

        if (data.ContainsKey("orbital"))
        {
            body.Orbital = OrbitalProps.FromDictionary((Dictionary)data["orbital"]);
        }

        if (data.ContainsKey("stellar"))
        {
            body.Stellar = StellarProps.FromDictionary((Dictionary)data["stellar"]);
        }

        if (data.ContainsKey("surface"))
        {
            body.Surface = SurfaceProps.FromDictionary((Dictionary)data["surface"]);
        }

        if (data.ContainsKey("atmosphere"))
        {
            body.Atmosphere = AtmosphereProps.FromDictionary((Dictionary)data["atmosphere"]);
        }

        if (data.ContainsKey("ring_system"))
        {
            body.RingSystem = RingSystemProps.FromDictionary((Dictionary)data["ring_system"]);
        }

        if (data.ContainsKey("population_data"))
        {
            body.PopulationData = DeserializePopulationData((Dictionary)data["population_data"]);
        }

        return body;
    }

    /// <summary>
    /// Serializes a celestial body to JSON.
    /// </summary>
    public static string ToJson(CelestialBody body, bool pretty = true)
    {
        Dictionary data = ToDictionary(body);
        return pretty ? Json.Stringify(data, "\t") : Json.Stringify(data);
    }

    /// <summary>
    /// Deserializes a JSON string to a celestial body.
    /// </summary>
    public static CelestialBody? FromJson(string jsonString)
    {
        Variant parsed = Json.ParseString(jsonString);
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return FromDictionary((Dictionary)parsed);
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        return data.ContainsKey(key) ? (string)data[key] : fallback;
    }

    /// <summary>
    /// Deserializes population data to the richest currently available C# type.
    /// </summary>
    private static RefCounted DeserializePopulationData(Dictionary data)
    {
        if (LooksLikePlanetPopulationData(data))
        {
            return PlanetPopulationData.FromDictionary(data);
        }

        return new SerializedPopulationData(data);
    }

    /// <summary>
    /// Returns whether a dictionary matches the known planet-population payload shape.
    /// </summary>
    private static bool LooksLikePlanetPopulationData(Dictionary data)
    {
        return data.ContainsKey("body_id")
            || data.ContainsKey("profile")
            || data.ContainsKey("suitability")
            || data.ContainsKey("native_populations")
            || data.ContainsKey("colonies");
    }
}
