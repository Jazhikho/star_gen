using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StdMath = System.Math;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Generic integer count bag keyed by an enum type.
/// Returns zero for unset keys instead of throwing.
/// </summary>
public sealed class ComponentCounts<T> : IReadOnlyDictionary<T, int> where T : struct, Enum
{
    private readonly System.Collections.Generic.Dictionary<T, int> _counts = new();

    /// <summary>
    /// Gets or sets the count for a component key.
    /// Getting a missing key returns zero. Setting to zero removes the entry.
    /// </summary>
    public int this[T key]
    {
        get
        {
            if (_counts.TryGetValue(key, out int count))
            {
                return count;
            }

            return 0;
        }
        set
        {
            if (value < 0)
            {
                throw new ArgumentException($"Count cannot be negative: {value} for {key}");
            }

            if (value == 0)
            {
                _counts.Remove(key);
            }
            else
            {
                _counts[key] = value;
            }
        }
    }

    /// <summary>
    /// Returns the sum of all counts.
    /// </summary>
    public int Sum()
    {
        int total = 0;
        foreach (KeyValuePair<T, int> entry in _counts)
        {
            total += entry.Value;
        }

        return total;
    }

    /// <summary>
    /// Returns whether all counts are zero or no entries exist.
    /// </summary>
    public bool IsEmpty()
    {
        return _counts.Count == 0;
    }

    /// <summary>
    /// Returns all non-zero entries for iteration.
    /// </summary>
    public IReadOnlyDictionary<T, int> Entries => _counts;

    /// <summary>
    /// Returns a new instance with each count multiplied by factor and floored.
    /// Counts that floor to zero are dropped.
    /// </summary>
    public ComponentCounts<T> Scale(double factor)
    {
        ComponentCounts<T> scaled = new();
        foreach (KeyValuePair<T, int> entry in _counts)
        {
            int newCount = (int)StdMath.Floor(entry.Value * factor);
            if (newCount > 0)
            {
                scaled._counts[entry.Key] = newCount;
            }
        }

        return scaled;
    }

    /// <summary>
    /// Returns a deep copy.
    /// </summary>
    public ComponentCounts<T> Clone()
    {
        ComponentCounts<T> copy = new();
        foreach (KeyValuePair<T, int> entry in _counts)
        {
            copy._counts[entry.Key] = entry.Value;
        }

        return copy;
    }

    /// <summary>
    /// Computes a weighted sum: for each entry, multiplies count by the
    /// value returned from the selector, and sums the results.
    /// </summary>
    public int SumBy(Func<T, int> selector)
    {
        int total = 0;
        foreach (KeyValuePair<T, int> entry in _counts)
        {
            total += selector(entry.Key) * entry.Value;
        }

        return total;
    }

    /// <summary>
    /// Computes a weighted sum returning long, for cost calculations.
    /// </summary>
    public long SumByLong(Func<T, long> selector)
    {
        long total = 0;
        foreach (KeyValuePair<T, int> entry in _counts)
        {
            total += selector(entry.Key) * entry.Value;
        }

        return total;
    }

    /// <summary>
    /// Serializes the count bag to a Godot dictionary using enum integral values as keys.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new();
        foreach (KeyValuePair<T, int> entry in _counts)
        {
            data[Convert.ToInt32(entry.Key)] = entry.Value;
        }

        return data;
    }

    /// <summary>
    /// Rebuilds a count bag from a Godot dictionary keyed by enum integral values.
    /// </summary>
    public static ComponentCounts<T> FromDictionary(Dictionary? data)
    {
        ComponentCounts<T> counts = new();
        if (data == null)
        {
            return counts;
        }

        foreach (Variant rawKey in data.Keys)
        {
            if (rawKey.VariantType != Variant.Type.Int)
            {
                continue;
            }

            Variant rawValue = data[rawKey];
            if (rawValue.VariantType != Variant.Type.Int)
            {
                continue;
            }

            int value = (int)rawValue;
            if (value <= 0)
            {
                continue;
            }

            int intKey = (int)rawKey;
            T key = (T)Enum.ToObject(typeof(T), intKey);
            counts[key] = value;
        }

        return counts;
    }

    /// <summary>
    /// Returns the present enum keys.
    /// </summary>
    public IEnumerable<T> Keys => _counts.Keys;

    /// <summary>
    /// Returns the present counts.
    /// </summary>
    public IEnumerable<int> Values => _counts.Values;

    /// <summary>
    /// Returns the number of stored non-zero entries.
    /// </summary>
    public int Count => _counts.Count;

    /// <summary>
    /// Returns whether the bag contains the given key.
    /// </summary>
    public bool ContainsKey(T key)
    {
        return _counts.ContainsKey(key);
    }

    /// <summary>
    /// Tries to get the stored count for a key.
    /// </summary>
    public bool TryGetValue(T key, out int value)
    {
        return _counts.TryGetValue(key, out value);
    }

    /// <summary>
    /// Returns an enumerator over the non-zero entries.
    /// </summary>
    public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
    {
        return _counts.GetEnumerator();
    }

    /// <summary>
    /// Returns a non-generic enumerator over the non-zero entries.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
