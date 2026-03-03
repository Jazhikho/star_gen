using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Sparse map of edited celestial bodies inside a galaxy.
/// </summary>
public partial class GalaxyBodyOverrides : RefCounted
{
    private readonly System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, Dictionary>> _overrides = new();

    /// <summary>
    /// Stores a serialized override for a body.
    /// </summary>
    public void SetOverride(int starSeed, CelestialBody body)
    {
        if (body == null || string.IsNullOrEmpty(body.Id))
        {
            return;
        }

        SetOverrideDict(starSeed, body.Id, CelestialSerializer.ToDictionary(body));
    }

    /// <summary>
    /// Stores a pre-serialized override dictionary for a body.
    /// </summary>
    public void SetOverrideDict(int starSeed, string bodyId, Dictionary bodyDict)
    {
        if (string.IsNullOrEmpty(bodyId) || bodyDict.Count == 0)
        {
            return;
        }

        if (!_overrides.ContainsKey(starSeed))
        {
            _overrides[starSeed] = new System.Collections.Generic.Dictionary<string, Dictionary>();
        }

        _overrides[starSeed][bodyId] = CloneDictionary(bodyDict);
    }

    /// <summary>
    /// Removes an override if present.
    /// </summary>
    public void ClearOverride(int starSeed, string bodyId)
    {
        if (!_overrides.ContainsKey(starSeed))
        {
            return;
        }

        System.Collections.Generic.Dictionary<string, Dictionary> bucket = _overrides[starSeed];
        bucket.Remove(bodyId);
        if (bucket.Count == 0)
        {
            _overrides.Remove(starSeed);
        }
    }

    /// <summary>
    /// Returns the serialized override for a body, or an empty dictionary.
    /// </summary>
    public Dictionary GetOverrideDict(int starSeed, string bodyId)
    {
        if (!_overrides.ContainsKey(starSeed))
        {
            return new Dictionary();
        }

        System.Collections.Generic.Dictionary<string, Dictionary> bucket = _overrides[starSeed];
        return bucket.ContainsKey(bodyId) ? CloneDictionary(bucket[bodyId]) : new Dictionary();
    }

    /// <summary>
    /// Returns the deserialized override body, or null.
    /// </summary>
    public CelestialBody? GetOverrideBody(int starSeed, string bodyId)
    {
        Dictionary data = GetOverrideDict(starSeed, bodyId);
        return data.Count == 0 ? null : CelestialSerializer.FromDictionary(data);
    }

    /// <summary>
    /// Returns whether any override exists for a star system.
    /// </summary>
    public bool HasAnyFor(int starSeed)
    {
        return _overrides.ContainsKey(starSeed);
    }

    /// <summary>
    /// Returns all overridden body identifiers for a star system.
    /// </summary>
    public Array<string> GetOverriddenIds(int starSeed)
    {
        Array<string> ids = new();
        if (!_overrides.ContainsKey(starSeed))
        {
            return ids;
        }

        foreach (string bodyId in _overrides[starSeed].Keys)
        {
            ids.Add(bodyId);
        }

        return ids;
    }

    /// <summary>
    /// Returns whether the override set is empty.
    /// </summary>
    public bool IsEmpty()
    {
        return _overrides.Count == 0;
    }

    /// <summary>
    /// Returns the total number of overridden bodies across all systems.
    /// </summary>
    public int TotalCount()
    {
        int count = 0;
        foreach (KeyValuePair<int, System.Collections.Generic.Dictionary<string, Dictionary>> entry in _overrides)
        {
            count += entry.Value.Count;
        }

        return count;
    }

    /// <summary>
    /// Applies overrides to a body array in place.
    /// </summary>
    public int ApplyToBodies(int starSeed, Array<CelestialBody> bodies)
    {
        if (!_overrides.ContainsKey(starSeed))
        {
            return 0;
        }

        System.Collections.Generic.Dictionary<string, Dictionary> bucket = _overrides[starSeed];
        int replaced = 0;
        for (int index = 0; index < bodies.Count; index += 1)
        {
            CelestialBody body = bodies[index];
            if (body == null || !bucket.ContainsKey(body.Id))
            {
                continue;
            }

            CelestialBody? patched = CelestialSerializer.FromDictionary(bucket[body.Id]);
            if (patched != null)
            {
                bodies[index] = patched;
                replaced += 1;
            }
        }

        return replaced;
    }

    /// <summary>
    /// Serializes the override set to a dictionary for persistence.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary output = new();
        foreach (KeyValuePair<int, System.Collections.Generic.Dictionary<string, Dictionary>> entry in _overrides)
        {
            Dictionary bucket = new();
            foreach (KeyValuePair<string, Dictionary> bodyEntry in entry.Value)
            {
                bucket[bodyEntry.Key] = CloneDictionary(bodyEntry.Value);
            }

            output[entry.Key.ToString()] = bucket;
        }

        return output;
    }

    /// <summary>
    /// Rebuilds an override set from a dictionary payload.
    /// </summary>
    public static GalaxyBodyOverrides FromDictionary(Dictionary data)
    {
        GalaxyBodyOverrides result = new();
        foreach (Variant seedKey in data.Keys)
        {
            if (seedKey.VariantType != Variant.Type.String)
            {
                continue;
            }

            string seedString = (string)seedKey;
            if (!int.TryParse(seedString, out int seedInt))
            {
                continue;
            }

            if (data[seedKey].VariantType != Variant.Type.Dictionary)
            {
                continue;
            }

            Dictionary bucketIn = (Dictionary)data[seedKey];
            foreach (Variant bodyId in bucketIn.Keys)
            {
                if (bodyId.VariantType != Variant.Type.String || bucketIn[bodyId].VariantType != Variant.Type.Dictionary)
                {
                    continue;
                }

                result.SetOverrideDict(seedInt, (string)bodyId, (Dictionary)bucketIn[bodyId]);
            }
        }

        return result;
    }

    /// <summary>
    /// Clones a dictionary payload.
    /// </summary>
    private static Dictionary CloneDictionary(Dictionary source)
    {
        Dictionary clone = new();
        foreach (Variant key in source.Keys)
        {
            Variant value = source[key];
            clone[key] = value.VariantType == Variant.Type.Dictionary ? CloneDictionary((Dictionary)value) : value;
        }

        return clone;
    }
}
