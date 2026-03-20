using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Concepts;

/// <summary>
/// Shared helpers for concept-domain serialization.
/// </summary>
public static class ConceptSerializationUtils
{
    /// <summary>
    /// Reads a string value or throws when the stored type is invalid.
    /// </summary>
    public static string ReadString(Dictionary data, string key)
    {
        if (!data.ContainsKey(key))
        {
            throw new InvalidOperationException("Missing required concept key '" + key + "'.");
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.String)
        {
            throw new InvalidOperationException("Expected string for concept key '" + key + "'.");
        }

        return (string)value;
    }

    /// <summary>
    /// Reads an integer value or throws when the stored type is invalid.
    /// </summary>
    public static int ReadInt(Dictionary data, string key)
    {
        if (!data.ContainsKey(key))
        {
            throw new InvalidOperationException("Missing required concept key '" + key + "'.");
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (int)(double)value;
        }

        throw new InvalidOperationException("Expected numeric value for concept key '" + key + "'.");
    }

    /// <summary>
    /// Reads a long value or throws when the stored type is invalid.
    /// </summary>
    public static long ReadLong(Dictionary data, string key)
    {
        if (!data.ContainsKey(key))
        {
            throw new InvalidOperationException("Missing required concept key '" + key + "'.");
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (long)(double)value;
        }

        throw new InvalidOperationException("Expected numeric value for concept key '" + key + "'.");
    }

    /// <summary>
    /// Reads a double value or throws when the stored type is invalid.
    /// </summary>
    public static double ReadDouble(Dictionary data, string key)
    {
        if (!data.ContainsKey(key))
        {
            throw new InvalidOperationException("Missing required concept key '" + key + "'.");
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (double)value;
        }

        throw new InvalidOperationException("Expected numeric value for concept key '" + key + "'.");
    }

    /// <summary>
    /// Reads a boolean value or throws when the stored type is invalid.
    /// </summary>
    public static bool ReadBool(Dictionary data, string key)
    {
        if (!data.ContainsKey(key))
        {
            throw new InvalidOperationException("Missing required concept key '" + key + "'.");
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Bool)
        {
            throw new InvalidOperationException("Expected boolean value for concept key '" + key + "'.");
        }

        return (bool)value;
    }

    /// <summary>
    /// Reads a dictionary payload or null when missing.
    /// </summary>
    public static Dictionary? ReadDictionary(Dictionary data, string key)
    {
        if (!data.ContainsKey(key))
        {
            return null;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Dictionary)
        {
            throw new InvalidOperationException("Expected dictionary value for concept key '" + key + "'.");
        }

        return (Dictionary)value;
    }

    /// <summary>
    /// Reads an optional string value or returns the provided fallback when the key is absent.
    /// </summary>
    public static string ReadOptionalString(Dictionary data, string key, string fallback = "")
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        return ReadString(data, key);
    }

    /// <summary>
    /// Reads an optional integer value or returns the provided fallback when the key is absent.
    /// </summary>
    public static int ReadOptionalInt(Dictionary data, string key, int fallback = 0)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        return ReadInt(data, key);
    }

    /// <summary>
    /// Reads an optional double value or returns the provided fallback when the key is absent.
    /// </summary>
    public static double ReadOptionalDouble(Dictionary data, string key, double fallback = 0.0)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        return ReadDouble(data, key);
    }

    /// <summary>
    /// Converts a list of strings into a Godot array.
    /// </summary>
    public static Array<string> ToArray(IReadOnlyList<string> values)
    {
        Array<string> array = new();
        foreach (string value in values)
        {
            array.Add(value);
        }

        return array;
    }

    /// <summary>
    /// Reads a string list from a Godot array value.
    /// </summary>
    public static List<string> ReadStringList(Dictionary data, string key)
    {
        List<string> result = new();
        if (!data.ContainsKey(key))
        {
            return result;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Array)
        {
            throw new InvalidOperationException("Expected array value for concept key '" + key + "'.");
        }

        foreach (Variant item in (Godot.Collections.Array)value)
        {
            if (item.VariantType != Variant.Type.String)
            {
                throw new InvalidOperationException("Expected string item in concept array '" + key + "'.");
            }

            result.Add((string)item);
        }

        return result;
    }

    /// <summary>
    /// Converts a string dictionary to a Godot dictionary.
    /// </summary>
    public static Dictionary ToDictionary(IReadOnlyDictionary<string, string> values)
    {
        Dictionary dictionary = new();
        foreach (KeyValuePair<string, string> entry in values)
        {
            dictionary[entry.Key] = entry.Value;
        }

        return dictionary;
    }

    /// <summary>
    /// Converts an integer dictionary to a Godot dictionary.
    /// </summary>
    public static Dictionary ToDictionary(IReadOnlyDictionary<string, int> values)
    {
        Dictionary dictionary = new();
        foreach (KeyValuePair<string, int> entry in values)
        {
            dictionary[entry.Key] = entry.Value;
        }

        return dictionary;
    }

    /// <summary>
    /// Reads a string dictionary from a Godot dictionary.
    /// </summary>
    public static System.Collections.Generic.Dictionary<string, string> ReadStringDictionary(Dictionary data, string key)
    {
        System.Collections.Generic.Dictionary<string, string> result = new();
        Dictionary? nested = ReadDictionary(data, key);
        if (nested == null)
        {
            return result;
        }

        foreach (Variant nestedKey in nested.Keys)
        {
            if (nestedKey.VariantType != Variant.Type.String)
            {
                throw new InvalidOperationException("Expected string key in concept dictionary '" + key + "'.");
            }

            Variant value = nested[nestedKey];
            if (value.VariantType != Variant.Type.String)
            {
                throw new InvalidOperationException("Expected string value in concept dictionary '" + key + "'.");
            }

            result[(string)nestedKey] = (string)value;
        }

        return result;
    }

    /// <summary>
    /// Reads an integer dictionary from a Godot dictionary.
    /// </summary>
    public static System.Collections.Generic.Dictionary<string, int> ReadIntDictionary(Dictionary data, string key)
    {
        System.Collections.Generic.Dictionary<string, int> result = new();
        Dictionary? nested = ReadDictionary(data, key);
        if (nested == null)
        {
            return result;
        }

        foreach (Variant nestedKey in nested.Keys)
        {
            if (nestedKey.VariantType != Variant.Type.String)
            {
                throw new InvalidOperationException("Expected string key in concept dictionary '" + key + "'.");
            }

            Variant value = nested[nestedKey];
            if (value.VariantType == Variant.Type.Int)
            {
                result[(string)nestedKey] = (int)value;
                continue;
            }

            if (value.VariantType == Variant.Type.Float)
            {
                result[(string)nestedKey] = (int)(double)value;
                continue;
            }

            throw new InvalidOperationException("Expected numeric value in concept dictionary '" + key + "'.");
        }

        return result;
    }
}
