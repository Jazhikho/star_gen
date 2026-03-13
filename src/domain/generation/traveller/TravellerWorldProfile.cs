using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation.Traveller;

/// <summary>
/// Deterministic Traveller-style world profile for a generated planet.
/// </summary>
public partial class TravellerWorldProfile : RefCounted
{
    /// <summary>
    /// Starport quality code as a single UWP character.
    /// </summary>
    public string StarportCode { get; set; } = "X";

    /// <summary>
    /// World size code.
    /// </summary>
    public int SizeCode { get; set; }

    /// <summary>
    /// Atmosphere code.
    /// </summary>
    public int AtmosphereCode { get; set; }

    /// <summary>
    /// Hydrographics code.
    /// </summary>
    public int HydrographicsCode { get; set; }

    /// <summary>
    /// Population code.
    /// </summary>
    public int PopulationCode { get; set; }

    /// <summary>
    /// Government code.
    /// </summary>
    public int GovernmentCode { get; set; }

    /// <summary>
    /// Law level code.
    /// </summary>
    public int LawCode { get; set; }

    /// <summary>
    /// Technology level code.
    /// </summary>
    public int TechLevelCode { get; set; }

    /// <summary>
    /// Returns the formatted UWP string.
    /// </summary>
    public string ToUwpString()
    {
        return
            NormalizeStarportCode(StarportCode)
            + ToHexDigit(SizeCode)
            + ToHexDigit(AtmosphereCode)
            + ToHexDigit(HydrographicsCode)
            + ToHexDigit(PopulationCode)
            + ToHexDigit(GovernmentCode)
            + ToHexDigit(LawCode)
            + "-"
            + ToHexDigit(TechLevelCode);
    }

    /// <summary>
    /// Converts the profile to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["starport_code"] = NormalizeStarportCode(StarportCode),
            ["size_code"] = SizeCode,
            ["atmosphere_code"] = AtmosphereCode,
            ["hydrographics_code"] = HydrographicsCode,
            ["population_code"] = PopulationCode,
            ["government_code"] = GovernmentCode,
            ["law_code"] = LawCode,
            ["tech_level_code"] = TechLevelCode,
            ["uwp"] = ToUwpString(),
        };
    }

    /// <summary>
    /// Rebuilds a profile from a dictionary payload.
    /// </summary>
    public static TravellerWorldProfile FromDictionary(Dictionary data)
    {
        TravellerWorldProfile profile = new TravellerWorldProfile();
        if (data.Count == 0)
        {
            return profile;
        }

        profile.StarportCode = NormalizeStarportCode(GetString(data, "starport_code", "X"));
        profile.SizeCode = GetInt(data, "size_code", 0);
        profile.AtmosphereCode = GetInt(data, "atmosphere_code", 0);
        profile.HydrographicsCode = GetInt(data, "hydrographics_code", 0);
        profile.PopulationCode = GetInt(data, "population_code", 0);
        profile.GovernmentCode = GetInt(data, "government_code", 0);
        profile.LawCode = GetInt(data, "law_code", 0);
        profile.TechLevelCode = GetInt(data, "tech_level_code", 0);
        return profile;
    }

    /// <summary>
    /// Returns a UWP hex digit for the supplied code.
    /// </summary>
    public static string ToHexDigit(int value)
    {
        int clamped = System.Math.Clamp(value, 0, 15);
        if (clamped < 10)
        {
            return clamped.ToString();
        }

        return ((char)('A' + (clamped - 10))).ToString();
    }

    /// <summary>
    /// Parses a UWP hex digit into an integer code.
    /// </summary>
    public static int ParseHexDigit(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return 0;
        }

        string normalized = token.Trim().ToUpperInvariant();
        if (int.TryParse(normalized, out int numeric))
        {
            return System.Math.Clamp(numeric, 0, 15);
        }

        char digit = normalized[0];
        if (digit >= 'A' && digit <= 'F')
        {
            return 10 + (digit - 'A');
        }

        return 0;
    }

    /// <summary>
    /// Returns a stable display label for the profile.
    /// </summary>
    public string GetSummary()
    {
        return $"UWP {ToUwpString()}";
    }

    private static string NormalizeStarportCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "X";
        }

        string normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length > 1)
        {
            normalized = normalized.Substring(0, 1);
        }

        return normalized;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
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
            return ParseHexDigit((string)value);
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.String)
        {
            return (string)data[key];
        }

        return fallback;
    }
}
