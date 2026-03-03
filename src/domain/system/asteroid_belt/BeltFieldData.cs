using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems.AsteroidFields;

/// <summary>
/// Generated asteroid belt field data including background and major entries.
/// </summary>
public partial class BeltFieldData : RefCounted
{
    public Array<BeltAsteroidData> Asteroids = new();
    public BeltFieldSpec? Spec;
    public int GenerationSeed;
    public string GeneratorVersion = "belt-field-1.0";

    /// <summary>
    /// Returns the major asteroids from the field.
    /// </summary>
    public Array<BeltAsteroidData> GetMajorAsteroids()
    {
        Array<BeltAsteroidData> result = new();
        foreach (BeltAsteroidData asteroid in Asteroids)
        {
            if (asteroid.IsMajor)
            {
                result.Add(asteroid);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the background asteroid samples from the field.
    /// </summary>
    public Array<BeltAsteroidData> GetBackgroundAsteroids()
    {
        Array<BeltAsteroidData> result = new();
        foreach (BeltAsteroidData asteroid in Asteroids)
        {
            if (!asteroid.IsMajor)
            {
                result.Add(asteroid);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts the field data to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> asteroids = new();
        foreach (BeltAsteroidData asteroid in Asteroids)
        {
            asteroids.Add(asteroid.ToDictionary());
        }

        return new Dictionary
        {
            ["asteroids"] = asteroids,
            ["spec"] = Spec != null ? Spec.ToDictionary() : new Dictionary(),
            ["generation_seed"] = GenerationSeed,
            ["generator_version"] = GeneratorVersion,
        };
    }

    /// <summary>
    /// Rebuilds field data from a dictionary payload.
    /// </summary>
    public static BeltFieldData FromDictionary(Dictionary data)
    {
        BeltFieldData field = new()
        {
            GenerationSeed = GetInt(data, "generation_seed", 0),
            GeneratorVersion = GetString(data, "generator_version", "belt-field-1.0"),
        };

        if (data.ContainsKey("spec") && data["spec"].VariantType == Variant.Type.Dictionary)
        {
            field.Spec = BeltFieldSpec.FromDictionary((Dictionary)data["spec"]);
        }

        if (data.ContainsKey("asteroids") && data["asteroids"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["asteroids"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    field.Asteroids.Add(BeltAsteroidData.FromDictionary((Dictionary)value));
                }
            }
        }

        return field;
    }

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
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }
}
