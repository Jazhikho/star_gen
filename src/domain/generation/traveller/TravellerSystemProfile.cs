using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation.Traveller;

/// <summary>
/// Typed Traveller profile for a generated system and its selected mainworld.
/// </summary>
public partial class TravellerSystemProfile : RefCounted
{
    /// <summary>
    /// Identifier of the selected mainworld body.
    /// </summary>
    public string MainworldBodyId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the selected mainworld.
    /// </summary>
    public string MainworldName { get; set; } = string.Empty;

    /// <summary>
    /// Stable explanation of why the mainworld was chosen.
    /// </summary>
    public string SelectionReason { get; set; } = string.Empty;

    /// <summary>
    /// Authoritative Traveller UWP profile for the selected mainworld.
    /// </summary>
    public TravellerWorldProfile WorldProfile { get; set; } = new();

    /// <summary>
    /// Registered trade codes for the selected mainworld.
    /// </summary>
    public TravellerTradeCodeSet TradeCodes { get; set; } = new();

    /// <summary>
    /// Travel zone label when applicable.
    /// </summary>
    public string TravelZone { get; set; } = string.Empty;

    /// <summary>
    /// Route-relevant Traveller metrics derived from the mainworld.
    /// </summary>
    public TravellerRouteProfile RouteProfile { get; set; } = new();

    /// <summary>
    /// Returns the authoritative UWP string.
    /// </summary>
    public string GetUwp()
    {
        return WorldProfile.ToUwpString();
    }

    /// <summary>
    /// Converts the profile to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["mainworld_body_id"] = MainworldBodyId,
            ["mainworld_name"] = MainworldName,
            ["selection_reason"] = SelectionReason,
            ["world_profile"] = WorldProfile.ToDictionary(),
            ["trade_codes"] = TradeCodes.ToDictionary(),
            ["travel_zone"] = TravelZone,
            ["route_profile"] = RouteProfile.ToDictionary(),
            ["uwp"] = GetUwp(),
        };
    }

    /// <summary>
    /// Rebuilds a system profile from a dictionary payload.
    /// </summary>
    public static TravellerSystemProfile FromDictionary(Dictionary data)
    {
        TravellerSystemProfile profile = new();
        if (data.Count == 0)
        {
            return profile;
        }

        profile.MainworldBodyId = GetString(data, "mainworld_body_id", string.Empty);
        profile.MainworldName = GetString(data, "mainworld_name", string.Empty);
        profile.SelectionReason = GetString(data, "selection_reason", string.Empty);
        profile.TravelZone = GetString(data, "travel_zone", string.Empty);

        if (data.ContainsKey("world_profile") && data["world_profile"].VariantType == Variant.Type.Dictionary)
        {
            profile.WorldProfile = TravellerWorldProfile.FromDictionary((Dictionary)data["world_profile"]);
        }

        if (data.ContainsKey("trade_codes") && data["trade_codes"].VariantType == Variant.Type.Dictionary)
        {
            profile.TradeCodes = TravellerTradeCodeSet.FromDictionary((Dictionary)data["trade_codes"]);
        }

        if (data.ContainsKey("route_profile") && data["route_profile"].VariantType == Variant.Type.Dictionary)
        {
            profile.RouteProfile = TravellerRouteProfile.FromDictionary((Dictionary)data["route_profile"]);
        }

        return profile;
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
