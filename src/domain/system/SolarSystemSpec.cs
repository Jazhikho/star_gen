using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;

namespace StarGen.Domain.Systems;

/// <summary>
/// Specification for solar-system generation.
/// </summary>
public partial class SolarSystemSpec : RefCounted
{
    /// <summary>
    /// Deterministic seed for system generation.
    /// </summary>
    public int GenerationSeed;

    /// <summary>
    /// Optional system-name hint.
    /// </summary>
    public string NameHint = string.Empty;

    /// <summary>
    /// Minimum number of stars to generate.
    /// </summary>
    public int StarCountMin;

    /// <summary>
    /// Maximum number of stars to generate.
    /// </summary>
    public int StarCountMax;

    /// <summary>
    /// Optional ordered spectral-class hints.
    /// </summary>
    public Array<int> SpectralClassHints = new();

    /// <summary>
    /// Shared system age in years, or -1 for per-star randomization.
    /// </summary>
    public double SystemAgeYears = -1.0;

    /// <summary>
    /// Shared system metallicity, or -1 for per-star randomization.
    /// </summary>
    public double SystemMetallicity = -1.0;

    /// <summary>
    /// Whether asteroid belts should be generated.
    /// </summary>
    public bool IncludeAsteroidBelts = true;

    /// <summary>
    /// Whether planet and moon population data should be generated.
    /// </summary>
    public bool GeneratePopulation;

    /// <summary>
    /// Field overrides keyed by path.
    /// </summary>
    public Dictionary Overrides = new();

    /// <summary>
    /// Creates a new solar-system spec.
    /// </summary>
    public SolarSystemSpec(int generationSeed = 0, int starCountMin = 1, int starCountMax = 1)
    {
        GenerationSeed = generationSeed;
        StarCountMin = System.Math.Clamp(starCountMin, 1, 10);
        StarCountMax = System.Math.Clamp(starCountMax, StarCountMin, 10);
    }

    /// <summary>
    /// Creates a single-star specification.
    /// </summary>
    public static SolarSystemSpec SingleStar(int seedValue)
    {
        return new SolarSystemSpec(seedValue, 1, 1);
    }

    /// <summary>
    /// Creates a binary-star specification.
    /// </summary>
    public static SolarSystemSpec Binary(int seedValue)
    {
        return new SolarSystemSpec(seedValue, 2, 2);
    }

    /// <summary>
    /// Creates a random small multi-star specification.
    /// </summary>
    public static SolarSystemSpec RandomSmall(int seedValue)
    {
        return new SolarSystemSpec(seedValue, 1, 3);
    }

    /// <summary>
    /// Creates a random one-to-ten-star specification.
    /// </summary>
    public static SolarSystemSpec Random(int seedValue)
    {
        return new SolarSystemSpec(seedValue, 1, 10);
    }

    /// <summary>
    /// Creates a Sun-like specification.
    /// </summary>
    public static SolarSystemSpec SunLike(int seedValue)
    {
        SolarSystemSpec spec = new(seedValue, 1, 1);
        spec.SpectralClassHints.Add((int)StarClass.SpectralClass.G);
        return spec;
    }

    /// <summary>
    /// Creates an Alpha-Centauri-like triple-star specification.
    /// </summary>
    public static SolarSystemSpec AlphaCentauriLike(int seedValue)
    {
        SolarSystemSpec spec = new(seedValue, 3, 3);
        spec.SpectralClassHints.Add((int)StarClass.SpectralClass.G);
        spec.SpectralClassHints.Add((int)StarClass.SpectralClass.K);
        spec.SpectralClassHints.Add((int)StarClass.SpectralClass.M);
        return spec;
    }

    /// <summary>
    /// Returns whether a field override exists.
    /// </summary>
    public bool HasOverride(string fieldPath)
    {
        return Overrides.ContainsKey(fieldPath);
    }

    /// <summary>
    /// Returns an override value or a fallback when none is present.
    /// </summary>
    public Variant GetOverride(string fieldPath, Variant defaultValue)
    {
        if (Overrides.ContainsKey(fieldPath))
        {
            return Overrides[fieldPath];
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets an override value.
    /// </summary>
    public void SetOverride(string fieldPath, Variant value)
    {
        Overrides[fieldPath] = value;
    }

    /// <summary>
    /// Converts the spec to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<int> hints = new();
        foreach (int hint in SpectralClassHints)
        {
            hints.Add(hint);
        }

        return new Dictionary
        {
            ["generation_seed"] = GenerationSeed,
            ["name_hint"] = NameHint,
            ["star_count_min"] = StarCountMin,
            ["star_count_max"] = StarCountMax,
            ["spectral_class_hints"] = hints,
            ["system_age_years"] = SystemAgeYears,
            ["system_metallicity"] = SystemMetallicity,
            ["include_asteroid_belts"] = IncludeAsteroidBelts,
            ["generate_population"] = GeneratePopulation,
            ["overrides"] = CloneDictionary(Overrides),
        };
    }

    /// <summary>
    /// Creates a spec from a dictionary payload.
    /// </summary>
    public static SolarSystemSpec FromDictionary(Dictionary data)
    {
        SolarSystemSpec spec = new(
            GetInt(data, "generation_seed", 0),
            GetInt(data, "star_count_min", 1),
            GetInt(data, "star_count_max", 1));
        spec.NameHint = GetString(data, "name_hint", string.Empty);
        spec.SystemAgeYears = GetDouble(data, "system_age_years", -1.0);
        spec.SystemMetallicity = GetDouble(data, "system_metallicity", -1.0);
        spec.IncludeAsteroidBelts = GetBool(data, "include_asteroid_belts", true);
        spec.GeneratePopulation = GetBool(data, "generate_population", false);

        if (data.ContainsKey("overrides") && data["overrides"].VariantType == Variant.Type.Dictionary)
        {
            spec.Overrides = CloneDictionary((Dictionary)data["overrides"]);
        }

        if (data.ContainsKey("spectral_class_hints") && data["spectral_class_hints"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["spectral_class_hints"])
            {
                if (value.VariantType == Variant.Type.Int)
                {
                    spec.SpectralClassHints.Add((int)value);
                }
            }
        }

        return spec;
    }

    /// <summary>
    /// Clones a dictionary so callers keep ownership of their payload.
    /// </summary>
    private static Dictionary CloneDictionary(Dictionary source)
    {
        Dictionary clone = new();
        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
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
            Variant.Type.Float => (int)(double)value,
            Variant.Type.String => TryParseInt((string)value, fallback),
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            Variant.Type.String => TryParseDouble((string)value, fallback),
            _ => fallback,
        };
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

    /// <summary>
    /// Reads a boolean value from a dictionary.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool)
        {
            return (bool)data[key];
        }

        return fallback;
    }

    private static int TryParseInt(string s, int fallback)
    {
        if (int.TryParse(s, out int parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private static double TryParseDouble(string s, double fallback)
    {
        if (double.TryParse(s, out double parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
