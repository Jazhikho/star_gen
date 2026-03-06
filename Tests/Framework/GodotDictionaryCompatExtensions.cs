#nullable enable annotations
#nullable disable warnings
using Godot;

namespace StarGen.Tests.Framework;

/// <summary>
/// Compatibility helpers for GDScript-style dictionary access used in converted tests.
/// </summary>
public static class GodotDictionaryCompatExtensions
{
    public static Variant Get(this Godot.Collections.Dictionary dictionary, Variant key, Variant defaultValue)
    {
        if (dictionary.ContainsKey(key))
        {
            return dictionary[key];
        }

        return defaultValue;
    }

    public static Variant GetValueOrDefault(this Godot.Collections.Dictionary dictionary, Variant key, Variant defaultValue)
    {
        return dictionary.Get(key, defaultValue);
    }
}
