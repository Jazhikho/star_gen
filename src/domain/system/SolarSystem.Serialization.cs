using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Concepts;

namespace StarGen.Domain.Systems;

/// <summary>
/// Dictionary serialization and deserialization for SolarSystem.
/// </summary>
public partial class SolarSystem
{
    /// <summary>
    /// Converts the system to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary hierarchyData;
        if (Hierarchy != null)
        {
            hierarchyData = Hierarchy.ToDictionary();
        }
        else
        {
            hierarchyData = new Dictionary();
        }

        Dictionary data = new()
        {
            ["id"] = Id,
            ["name"] = Name,
            ["hierarchy"] = hierarchyData,
            ["star_ids"] = CloneArray(StarIds),
            ["planet_ids"] = CloneArray(PlanetIds),
            ["moon_ids"] = CloneArray(MoonIds),
            ["asteroid_ids"] = CloneArray(AsteroidIds),
        };

        Dictionary bodiesData = new();
        foreach (string bodyId in Bodies.Keys)
        {
            bodiesData[bodyId] = CelestialSerializer.ToDictionary(Bodies[bodyId]);
        }

        data["bodies"] = bodiesData;

        Array<Dictionary> beltsData = new();
        foreach (AsteroidBelt belt in AsteroidBelts)
        {
            beltsData.Add(belt.ToDictionary());
        }

        data["asteroid_belts"] = beltsData;

        Array<Dictionary> hostsData = new();
        foreach (OrbitHost host in OrbitHosts)
        {
            hostsData.Add(host.ToDictionary());
        }

        data["orbit_hosts"] = hostsData;

        if (Provenance != null)
        {
            data["provenance"] = Provenance.ToDictionary();
        }

        if (HasConceptResults())
        {
            data["concept_results"] = ConceptResults.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Creates a system from a dictionary payload.
    /// </summary>
    public static SolarSystem FromDictionary(Dictionary data)
    {
        SolarSystem system = new(
            GetString(data, "id", string.Empty),
            GetString(data, "name", string.Empty));

        if (data.ContainsKey("hierarchy") && data["hierarchy"].VariantType == Variant.Type.Dictionary)
        {
            system.Hierarchy = SystemHierarchy.FromDictionary((Dictionary)data["hierarchy"]);
        }

        if (data.ContainsKey("bodies") && data["bodies"].VariantType == Variant.Type.Dictionary)
        {
            Dictionary bodiesData = (Dictionary)data["bodies"];
            foreach (Variant bodyKey in bodiesData.Keys)
            {
                if (bodyKey.VariantType != Variant.Type.String)
                {
                    continue;
                }

                Variant bodyValue = bodiesData[bodyKey];
                if (bodyValue.VariantType != Variant.Type.Dictionary)
                {
                    continue;
                }

                CelestialBody? body = CelestialSerializer.FromDictionary((Dictionary)bodyValue);
                if (body != null)
                {
                    system.Bodies[(string)bodyKey] = body;
                }
            }
        }

        PopulateStringArray(data, "star_ids", system.StarIds);
        PopulateStringArray(data, "planet_ids", system.PlanetIds);
        PopulateStringArray(data, "moon_ids", system.MoonIds);
        PopulateStringArray(data, "asteroid_ids", system.AsteroidIds);

        if (data.ContainsKey("asteroid_belts") && data["asteroid_belts"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["asteroid_belts"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    system.AsteroidBelts.Add(AsteroidBelt.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("orbit_hosts") && data["orbit_hosts"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["orbit_hosts"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    system.OrbitHosts.Add(OrbitHost.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("provenance") && data["provenance"].VariantType == Variant.Type.Dictionary)
        {
            system.Provenance = Provenance.FromDictionary((Dictionary)data["provenance"]);
        }

        if (data.ContainsKey("concept_results") && data["concept_results"].VariantType == Variant.Type.Dictionary)
        {
            system.ConceptResults = ConceptResultStore.FromDictionary((Dictionary)data["concept_results"]);
        }

        return system;
    }

    /// <summary>
    /// Clones a string array for payload serialization.
    /// </summary>
    private static Array<string> CloneArray(Array<string> source)
    {
        Array<string> clone = new();
        foreach (string value in source)
        {
            clone.Add(value);
        }

        return clone;
    }

    /// <summary>
    /// Populates a typed string array from a payload key.
    /// </summary>
    private static void PopulateStringArray(Dictionary data, string key, Array<string> target)
    {
        if (!data.ContainsKey(key) || data[key].VariantType != Variant.Type.Array)
        {
            return;
        }

        foreach (Variant value in (Array)data[key])
        {
            if (value.VariantType == Variant.Type.String)
            {
                target.Add((string)value);
            }
        }
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
}
