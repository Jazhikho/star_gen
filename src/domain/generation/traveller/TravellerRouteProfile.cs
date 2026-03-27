using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation.Traveller;

/// <summary>
/// Route-relevant Traveller data derived from the authoritative mainworld.
/// </summary>
public partial class TravellerRouteProfile : RefCounted
{
    /// <summary>
    /// Estimated mainworld population count used by route tools.
    /// </summary>
    public int EstimatedPopulation { get; set; }

    /// <summary>
    /// Traveller population code for the mainworld.
    /// </summary>
    public int PopulationCode { get; set; }

    /// <summary>
    /// Traveller starport code.
    /// </summary>
    public string StarportCode { get; set; } = "X";

    /// <summary>
    /// Traveller tech-level code.
    /// </summary>
    public int TechLevelCode { get; set; }

    /// <summary>
    /// Traveller-style route importance metric.
    /// </summary>
    public int Importance { get; set; }

    /// <summary>
    /// Maximum jump number this world can reasonably sustain for route building.
    /// </summary>
    public int MaxJumpNumber { get; set; }

    /// <summary>
    /// Aggregate commercial weight used to sort candidate routes.
    /// </summary>
    public int RouteWeight { get; set; }

    /// <summary>
    /// Returns whether the world can participate in Traveller route building.
    /// </summary>
    public bool IsRouteEligible()
    {
        return EstimatedPopulation > 0
            && MaxJumpNumber > 0
            && !string.Equals(StarportCode, "X", System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts the route profile to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["estimated_population"] = EstimatedPopulation,
            ["population_code"] = PopulationCode,
            ["starport_code"] = StarportCode,
            ["tech_level_code"] = TechLevelCode,
            ["importance"] = Importance,
            ["max_jump_number"] = MaxJumpNumber,
            ["route_weight"] = RouteWeight,
        };
    }

    /// <summary>
    /// Rebuilds a route profile from a dictionary payload.
    /// </summary>
    public static TravellerRouteProfile FromDictionary(Dictionary data)
    {
        TravellerRouteProfile profile = new();
        if (data.Count == 0)
        {
            return profile;
        }

        profile.EstimatedPopulation = GetInt(data, "estimated_population", 0);
        profile.PopulationCode = GetInt(data, "population_code", 0);
        profile.StarportCode = GetString(data, "starport_code", "X");
        profile.TechLevelCode = GetInt(data, "tech_level_code", 0);
        profile.Importance = GetInt(data, "importance", 0);
        profile.MaxJumpNumber = GetInt(data, "max_jump_number", 0);
        profile.RouteWeight = GetInt(data, "route_weight", 0);
        return profile;
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
