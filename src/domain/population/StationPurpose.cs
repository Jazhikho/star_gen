using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Station primary-purpose classification.
/// </summary>
public static class StationPurpose
{
    /// <summary>
    /// Station purposes.
    /// </summary>
    public enum Purpose
    {
        Utility,
        Trade,
        Military,
        Science,
        Mining,
        Residential,
        Administrative,
        Industrial,
        Medical,
        Communications,
    }

    /// <summary>
    /// Converts a purpose to a display string.
    /// </summary>
    public static string ToStringName(Purpose purpose)
    {
        return purpose switch
        {
            Purpose.Utility => "Utility",
            Purpose.Trade => "Trade",
            Purpose.Military => "Military",
            Purpose.Science => "Science",
            Purpose.Mining => "Mining",
            Purpose.Residential => "Residential",
            Purpose.Administrative => "Administrative",
            Purpose.Industrial => "Industrial",
            Purpose.Medical => "Medical",
            Purpose.Communications => "Communications",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a purpose from a string.
    /// </summary>
    public static Purpose FromString(string name)
    {
        return name.ToLowerInvariant().Trim() switch
        {
            "utility" => Purpose.Utility,
            "trade" => Purpose.Trade,
            "military" => Purpose.Military,
            "science" => Purpose.Science,
            "mining" => Purpose.Mining,
            "residential" => Purpose.Residential,
            "administrative" => Purpose.Administrative,
            "industrial" => Purpose.Industrial,
            "medical" => Purpose.Medical,
            "communications" => Purpose.Communications,
            _ => Purpose.Utility,
        };
    }

    /// <summary>
    /// Returns typical utility-station purposes.
    /// </summary>
    public static Array<Purpose> TypicalUtilityPurposes()
    {
        return new Array<Purpose> { Purpose.Utility, Purpose.Trade, Purpose.Communications };
    }

    /// <summary>
    /// Returns typical outpost purposes.
    /// </summary>
    public static Array<Purpose> TypicalOutpostPurposes()
    {
        return new Array<Purpose> { Purpose.Military, Purpose.Science, Purpose.Mining, Purpose.Communications };
    }

    /// <summary>
    /// Returns typical settlement-station purposes.
    /// </summary>
    public static Array<Purpose> TypicalSettlementPurposes()
    {
        return new Array<Purpose> { Purpose.Trade, Purpose.Residential, Purpose.Administrative, Purpose.Industrial };
    }

    /// <summary>
    /// Returns whether a purpose is typical for small stations.
    /// </summary>
    public static bool IsSmallStationPurpose(Purpose purpose)
    {
        return purpose == Purpose.Utility
            || purpose == Purpose.Mining
            || purpose == Purpose.Science
            || purpose == Purpose.Military
            || purpose == Purpose.Communications;
    }

    /// <summary>
    /// Returns the number of purposes.
    /// </summary>
    public static int Count()
    {
        return 10;
    }
}
