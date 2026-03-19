using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Concepts;
using StarGen.Domain.Constants;
using StarGen.Domain.Population;

namespace StarGen.Domain.Celestial.Serialization;

/// <summary>
/// Handles serialization and deserialization of celestial bodies.
/// </summary>
public static class CelestialSerializer
{
    /// <summary>
    /// Legacy alias for converted tests.
    /// </summary>
    public static Dictionary ToDict(CelestialBody body) => ToDictionary(body);

    /// <summary>
    /// Legacy alias for converted tests.
    /// </summary>
    public static CelestialBody? FromDict(Dictionary data) => FromDictionary(data);

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

        if (body.HasPopulationData() && body.PopulationData != null)
        {
            data["population_data"] = body.PopulationData.ToDictionary();
        }

        if (body.HasConceptResults())
        {
            data["concept_results"] = body.ConceptResults.ToDictionary();
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
            Variant typeVariant = data["type"];
            if (typeVariant.VariantType != Variant.Type.String)
            {
                GD.PushError($"CelestialSerializer.FromDictionary: 'type' value has unexpected Variant type '{typeVariant.VariantType}' — defaulting to Planet.");
            }
            else
            {
                string typeName = (string)typeVariant;
                if (!CelestialType.TryParse(typeName, out CelestialType.Type parsedType))
                {
                    GD.PushError($"CelestialSerializer.FromDictionary: unrecognized body type '{typeName}' — defaulting to Planet.");
                }
                else
                {
                    type = parsedType;
                }
            }
        }
        else
        {
            GD.PushError("CelestialSerializer.FromDictionary: 'type' key missing from body data — defaulting to Planet.");
        }

        Dictionary physicalData;
        if (data.ContainsKey("physical") && data["physical"].VariantType == Variant.Type.Dictionary)
        {
            physicalData = (Dictionary)data["physical"];
        }
        else
        {
            GD.PushError($"CelestialSerializer.FromDictionary: 'physical' key missing or wrong type for body '{GetString(data, "id", "<unknown>")}' — constructing empty PhysicalProps.");
            physicalData = new Dictionary();
        }

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

        if (data.ContainsKey("concept_results") && data["concept_results"].VariantType == Variant.Type.Dictionary)
        {
            body.ConceptResults = ConceptResultStore.FromDictionary((Dictionary)data["concept_results"]);
        }

        return body;
    }

    /// <summary>
    /// Serializes a celestial body to JSON.
    /// </summary>
    public static string ToJson(CelestialBody body, bool pretty = true)
    {
        Dictionary data = ToDictionary(body);
        if (pretty)
        {
            return Json.Stringify(data, "\t");
        }

        return Json.Stringify(data);
    }

    /// <summary>
    /// Deserializes a JSON string to a celestial body.
    /// </summary>
    public static CelestialBody? FromJson(string jsonString)
    {
        Json parser = new();
        Error parseError = parser.Parse(jsonString);
        if (parseError != Error.Ok)
        {
            return null;
        }

        Variant parsed = parser.Data;
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return FromDictionary((Dictionary)parsed);
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key))
        {
            return (string)data[key];
        }

        return fallback;
    }

    /// <summary>
    /// Deserializes population data to PlanetPopulationData (handles minimal or full payloads).
    /// </summary>
    private static PlanetPopulationData DeserializePopulationData(Dictionary data)
    {
        return PlanetPopulationData.FromDictionary(data);
    }
}

