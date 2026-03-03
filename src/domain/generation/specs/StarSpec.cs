using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Specification for star generation.
/// </summary>
public partial class StarSpec : BaseSpec
{
    /// <summary>
    /// Target spectral class, or -1 for random.
    /// </summary>
    public int SpectralClass;

    /// <summary>
    /// Target subclass, or -1 for random.
    /// </summary>
    public int Subclass;

    /// <summary>
    /// Metallicity relative to solar, or -1 for random.
    /// </summary>
    public double Metallicity;

    /// <summary>
    /// Age hint in years, or -1 for random.
    /// </summary>
    public double AgeYears;

    /// <summary>
    /// Creates a new star specification.
    /// </summary>
    public StarSpec(
        int generationSeed = 0,
        int spectralClass = -1,
        int subclass = -1,
        double metallicity = -1.0,
        double ageYears = -1.0,
        string nameHint = "",
        Dictionary? overrides = null)
        : base(generationSeed, nameHint, overrides)
    {
        SpectralClass = spectralClass;
        Subclass = subclass;
        Metallicity = metallicity;
        AgeYears = ageYears;
    }

    /// <summary>
    /// Creates a fully random star specification.
    /// </summary>
    public static StarSpec Random(int generationSeed) => new(generationSeed);

    /// <summary>
    /// Creates a Sun-like star specification.
    /// </summary>
    public static StarSpec SunLike(int generationSeed)
    {
        return new StarSpec(generationSeed, (int)StarClass.SpectralClass.G, 2, 1.0, -1.0);
    }

    /// <summary>
    /// Creates a red-dwarf specification.
    /// </summary>
    public static StarSpec RedDwarf(int generationSeed)
    {
        return new StarSpec(generationSeed, (int)StarClass.SpectralClass.M, -1, -1.0, -1.0);
    }

    /// <summary>
    /// Creates a hot blue star specification.
    /// </summary>
    public static StarSpec HotBlue(int generationSeed)
    {
        return new StarSpec(generationSeed, (int)StarClass.SpectralClass.B, -1, -1.0, -1.0);
    }

    /// <summary>
    /// Returns whether a spectral class was specified.
    /// </summary>
    public bool HasSpectralClass() => SpectralClass >= 0;

    /// <summary>
    /// Returns whether a subclass was specified.
    /// </summary>
    public bool HasSubclass() => Subclass >= 0;

    /// <summary>
    /// Returns whether a metallicity was specified.
    /// </summary>
    public bool HasMetallicity() => Metallicity >= 0.0;

    /// <summary>
    /// Returns whether an age was specified.
    /// </summary>
    public bool HasAge() => AgeYears >= 0.0;

    /// <summary>
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "star";
        data["spectral_class"] = SpectralClass;
        data["subclass"] = Subclass;
        data["metallicity"] = Metallicity;
        data["age_years"] = AgeYears;
        return data;
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static StarSpec FromDictionary(Dictionary data)
    {
        return new StarSpec(
            data.ContainsKey("generation_seed") ? (int)data["generation_seed"] : 0,
            data.ContainsKey("spectral_class") ? (int)data["spectral_class"] : -1,
            data.ContainsKey("subclass") ? (int)data["subclass"] : -1,
            GetDouble(data, "metallicity", -1.0),
            GetDouble(data, "age_years", -1.0),
            data.ContainsKey("name_hint") ? (string)data["name_hint"] : string.Empty,
            data.ContainsKey("overrides") ? (Dictionary)data["overrides"] : null);
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        return data.ContainsKey(key) ? (double)data[key] : fallback;
    }
}
