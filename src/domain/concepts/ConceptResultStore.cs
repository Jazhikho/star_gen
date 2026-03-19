using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Concepts;

/// <summary>
/// Persisted collection of concept run results keyed by concept kind.
/// </summary>
public sealed class ConceptResultStore
{
    private readonly System.Collections.Generic.Dictionary<ConceptKind, ConceptRunResult> _results = new();

    /// <summary>
    /// Returns whether the store contains no results.
    /// </summary>
    public bool IsEmpty()
    {
        return _results.Count == 0;
    }

    /// <summary>
    /// Returns whether a given concept result exists.
    /// </summary>
    public bool Has(ConceptKind kind)
    {
        return _results.ContainsKey(kind);
    }

    /// <summary>
    /// Stores a concept result.
    /// </summary>
    public void Set(ConceptKind kind, ConceptRunResult result)
    {
        _results[kind] = ConceptRunResultSerialization.Clone(result);
    }

    /// <summary>
    /// Returns a cloned concept result when present.
    /// </summary>
    public ConceptRunResult? Get(ConceptKind kind)
    {
        if (!_results.TryGetValue(kind, out ConceptRunResult? result))
        {
            return null;
        }

        return ConceptRunResultSerialization.Clone(result);
    }

    /// <summary>
    /// Merges all values from another store.
    /// </summary>
    public void MergeFrom(ConceptResultStore? other)
    {
        if (other == null || other.IsEmpty())
        {
            return;
        }

        foreach (KeyValuePair<ConceptKind, ConceptRunResult> entry in other._results)
        {
            _results[entry.Key] = ConceptRunResultSerialization.Clone(entry.Value);
        }
    }

    /// <summary>
    /// Returns all stored results as a read-only view.
    /// </summary>
    public IReadOnlyDictionary<ConceptKind, ConceptRunResult> GetAll()
    {
        return _results;
    }

    /// <summary>
    /// Returns a deep clone of this store.
    /// </summary>
    public ConceptResultStore Clone()
    {
        ConceptResultStore clone = new();
        clone.MergeFrom(this);
        return clone;
    }

    /// <summary>
    /// Converts the store into a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new();
        foreach (KeyValuePair<ConceptKind, ConceptRunResult> entry in _results)
        {
            data[entry.Key.ToString().ToLowerInvariant()] = ConceptRunResultSerialization.ToDictionary(entry.Value);
        }

        return data;
    }

    /// <summary>
    /// Creates a store from a dictionary payload.
    /// </summary>
    public static ConceptResultStore FromDictionary(Dictionary data)
    {
        ConceptResultStore store = new();
        foreach (Variant key in data.Keys)
        {
            if (key.VariantType != Godot.Variant.Type.String)
            {
                continue;
            }

            string keyText = ((string)key).Trim();
            if (!System.Enum.TryParse(keyText, ignoreCase: true, out ConceptKind kind))
            {
                continue;
            }

            Variant value = data[key];
            if (value.VariantType != Godot.Variant.Type.Dictionary)
            {
                continue;
            }

            store.Set(kind, ConceptRunResultSerialization.FromDictionary((Dictionary)value));
        }

        return store;
    }
}
