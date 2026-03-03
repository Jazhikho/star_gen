using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// Region of systems used for jump-lane calculations.
/// </summary>
public partial class JumpLaneRegion : RefCounted
{
    /// <summary>
    /// Region scope categories.
    /// </summary>
    public enum RegionScope
    {
        Subsector,
        Sector,
    }

    /// <summary>
    /// Region scope.
    /// </summary>
    public RegionScope Scope = RegionScope.Subsector;

    /// <summary>
    /// Systems inside the region.
    /// </summary>
    public Array<JumpLaneSystem> Systems = new();

    /// <summary>
    /// Optional region identifier.
    /// </summary>
    public string RegionId = string.Empty;

    /// <summary>
    /// Creates a new jump-lane region.
    /// </summary>
    public JumpLaneRegion(RegionScope scope = RegionScope.Subsector, string regionId = "")
    {
        Scope = scope;
        RegionId = regionId;
    }

    /// <summary>
    /// Adds a system to the region.
    /// </summary>
    public void AddSystem(JumpLaneSystem system)
    {
        Systems.Add(system);
    }

    /// <summary>
    /// Removes a system by identifier.
    /// </summary>
    public bool RemoveSystem(string systemId)
    {
        for (int index = Systems.Count - 1; index >= 0; index -= 1)
        {
            if (Systems[index].Id == systemId)
            {
                Systems.RemoveAt(index);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns a system by identifier, or null when not found.
    /// </summary>
    public JumpLaneSystem? GetSystem(string systemId)
    {
        foreach (JumpLaneSystem system in Systems)
        {
            if (system.Id == systemId)
            {
                return system;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns populated systems only.
    /// </summary>
    public Array<JumpLaneSystem> GetPopulatedSystems()
    {
        Array<JumpLaneSystem> populated = new();
        foreach (JumpLaneSystem system in Systems)
        {
            if (system.IsPopulated())
            {
                populated.Add(system);
            }
        }

        return populated;
    }

    /// <summary>
    /// Returns unpopulated systems only.
    /// </summary>
    public Array<JumpLaneSystem> GetUnpopulatedSystems()
    {
        Array<JumpLaneSystem> unpopulated = new();
        foreach (JumpLaneSystem system in Systems)
        {
            if (!system.IsPopulated())
            {
                unpopulated.Add(system);
            }
        }

        return unpopulated;
    }

    /// <summary>
    /// Returns populated systems sorted by effective population ascending.
    /// </summary>
    public Array<JumpLaneSystem> GetSystemsSortedByPopulation()
    {
        List<JumpLaneSystem> sorted = new();
        foreach (JumpLaneSystem system in GetPopulatedSystems())
        {
            sorted.Add(system);
        }

        sorted.Sort((left, right) => left.GetEffectivePopulation().CompareTo(right.GetEffectivePopulation()));
        Array<JumpLaneSystem> result = new();
        foreach (JumpLaneSystem system in sorted)
        {
            result.Add(system);
        }

        return result;
    }

    /// <summary>
    /// Returns the total system count.
    /// </summary>
    public int GetSystemCount()
    {
        return Systems.Count;
    }

    /// <summary>
    /// Returns the populated-system count.
    /// </summary>
    public int GetPopulatedCount()
    {
        return GetPopulatedSystems().Count;
    }

    /// <summary>
    /// Clears all systems from the region.
    /// </summary>
    public void Clear()
    {
        Systems.Clear();
    }

    /// <summary>
    /// Converts the region to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> systems = new();
        foreach (JumpLaneSystem system in Systems)
        {
            systems.Add(system.ToDictionary());
        }

        return new Dictionary
        {
            ["scope"] = (int)Scope,
            ["region_id"] = RegionId,
            ["systems"] = systems,
        };
    }

    /// <summary>
    /// Creates a region from a dictionary payload.
    /// </summary>
    public static JumpLaneRegion FromDictionary(Dictionary data)
    {
        JumpLaneRegion region = new(
            (RegionScope)GetInt(data, "scope", (int)RegionScope.Subsector),
            GetString(data, "region_id", string.Empty));
        if (data.ContainsKey("systems") && data["systems"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["systems"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    region.Systems.Add(JumpLaneSystem.FromDictionary((Dictionary)value));
                }
            }
        }

        return region;
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

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
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
            Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : fallback,
            _ => fallback,
        };
    }
}
