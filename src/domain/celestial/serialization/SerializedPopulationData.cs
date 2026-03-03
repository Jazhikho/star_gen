using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Serialization;

/// <summary>
/// Raw dictionary-backed placeholder for population data until the C# population model is ported.
/// </summary>
public partial class SerializedPopulationData : RefCounted
{
    private readonly Dictionary _data;

    /// <summary>
    /// Creates a new raw population-data payload.
    /// </summary>
    public SerializedPopulationData(Dictionary sourceData)
    {
        _data = CloneDictionary(sourceData);
    }

    /// <summary>
    /// Returns a copy of the stored population-data payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return CloneDictionary(_data);
    }

    private static Dictionary CloneDictionary(Dictionary source)
    {
        Dictionary clone = new();
        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
    }
}
