using Godot;
using Godot.Collections;

namespace StarGen.Domain.Utils;

/// <summary>
/// Shared Godot-Collection dictionary read helpers.
/// All methods accept a fallback value that is returned when the key is absent
/// or the stored variant type does not match the expected type.
/// </summary>
public static class DomainDictionaryUtils
{
    /// <summary>
    /// Reads a string value from a Godot dictionary.
    /// </summary>
    /// <param name="data">Source dictionary.</param>
    /// <param name="key">Key to look up.</param>
    /// <param name="fallback">Value returned when the key is missing or the type is wrong.</param>
    /// <returns>The stored string, or <paramref name="fallback"/>.</returns>
    public static string GetString(Dictionary data, string key, string fallback)
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

    /// <summary>
    /// Reads an integer value from a Godot dictionary.
    /// Handles Int and Float variant types; also tries to parse from a String variant.
    /// </summary>
    /// <param name="data">Source dictionary.</param>
    /// <param name="key">Key to look up.</param>
    /// <param name="fallback">Value returned when the key is missing or the type is unrecognised.</param>
    /// <returns>The integer value, or <paramref name="fallback"/>.</returns>
    public static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
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

        if (value.VariantType == Variant.Type.String)
        {
            return TryParseInt((string)value, fallback);
        }

        GD.PushError(
            $"[DomainDictionaryUtils] GetInt: unexpected Variant type '{value.VariantType}' for key '{key}' — using fallback {fallback}.");
        return fallback;
    }

    /// <summary>
    /// Reads a 64-bit integer value from a Godot dictionary.
    /// Handles Int and Float variant types.
    /// </summary>
    /// <param name="data">Source dictionary.</param>
    /// <param name="key">Key to look up.</param>
    /// <param name="fallback">Value returned when the key is missing or the type is wrong.</param>
    /// <returns>The long value, or <paramref name="fallback"/>.</returns>
    public static long GetLong(Dictionary data, string key, long fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (long)(int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (long)(double)value;
        }

        return fallback;
    }

    /// <summary>
    /// Reads a double-precision floating-point value from a Godot dictionary.
    /// Handles Float and Int variant types; also tries to parse from a String variant.
    /// </summary>
    /// <param name="data">Source dictionary.</param>
    /// <param name="key">Key to look up.</param>
    /// <param name="fallback">Value returned when the key is missing or the type is unrecognised.</param>
    /// <returns>The double value, or <paramref name="fallback"/>.</returns>
    public static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Float)
        {
            return (double)value;
        }

        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.String)
        {
            return TryParseDouble((string)value, fallback);
        }

        return fallback;
    }

    /// <summary>
    /// Reads a boolean value from a Godot dictionary.
    /// </summary>
    /// <param name="data">Source dictionary.</param>
    /// <param name="key">Key to look up.</param>
    /// <param name="fallback">Value returned when the key is missing or the type is wrong.</param>
    /// <returns>The boolean value, or <paramref name="fallback"/>.</returns>
    public static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Bool)
        {
            return (bool)value;
        }

        return fallback;
    }

    /// <summary>
    /// Reads a nested dictionary from a Godot dictionary.
    /// Returns an empty dictionary when the key is missing or the value is not a dictionary.
    /// </summary>
    /// <param name="data">Source dictionary.</param>
    /// <param name="key">Key to look up.</param>
    /// <returns>The nested dictionary, or an empty one.</returns>
    public static Dictionary GetDictionary(Dictionary data, string key)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Dictionary)
        {
            return (Dictionary)data[key];
        }

        return new Dictionary();
    }

    /// <summary>
    /// Attempts to read a Godot array from a dictionary payload.
    /// </summary>
    /// <param name="data">Source dictionary.</param>
    /// <param name="key">Key to look up.</param>
    /// <param name="array">Receives the array on success, or an empty array on failure.</param>
    /// <returns>True when the key exists and contains an array; false otherwise.</returns>
    public static bool TryGetArray(Dictionary data, string key, out Array array)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Array)
        {
            array = (Array)data[key];
            return true;
        }

        array = new Array();
        return false;
    }

    /// <summary>
    /// Coerces a numeric Variant (Int or Float) to a double.
    /// Returns 0.0 for non-numeric types.
    /// </summary>
    /// <param name="value">Variant to coerce.</param>
    /// <returns>Numeric value as a double, or 0.0.</returns>
    public static double GetNumeric(Variant value)
    {
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (double)value;
        }

        return 0.0;
    }

    /// <summary>
    /// Parses an integer from a string, returning the fallback on failure.
    /// </summary>
    /// <param name="s">String to parse.</param>
    /// <param name="fallback">Value returned when parsing fails.</param>
    /// <returns>Parsed integer, or <paramref name="fallback"/>.</returns>
    public static int TryParseInt(string s, int fallback)
    {
        if (int.TryParse(s, out int parsed))
        {
            return parsed;
        }

        return fallback;
    }

    /// <summary>
    /// Parses a double from a string, returning the fallback on failure.
    /// </summary>
    /// <param name="s">String to parse.</param>
    /// <param name="fallback">Value returned when parsing fails.</param>
    /// <returns>Parsed double, or <paramref name="fallback"/>.</returns>
    public static double TryParseDouble(string s, double fallback)
    {
        if (double.TryParse(s, out double parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
