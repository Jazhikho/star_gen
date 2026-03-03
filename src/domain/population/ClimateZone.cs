namespace StarGen.Domain.Population;

/// <summary>
/// Climate-zone classifications for planetary surfaces.
/// </summary>
public static class ClimateZone
{
    /// <summary>
    /// Supported climate-zone types.
    /// </summary>
    public enum Zone
    {
        Polar,
        Subpolar,
        Temperate,
        Subtropical,
        Tropical,
        Arid,
        Extreme,
    }

    /// <summary>
    /// Converts a zone to a display string.
    /// </summary>
    public static string ToStringName(Zone zone)
    {
        return zone switch
        {
            Zone.Polar => "Polar",
            Zone.Subpolar => "Subpolar",
            Zone.Temperate => "Temperate",
            Zone.Subtropical => "Subtropical",
            Zone.Tropical => "Tropical",
            Zone.Arid => "Arid",
            Zone.Extreme => "Extreme",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a zone from a string.
    /// </summary>
    public static Zone FromString(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "polar" => Zone.Polar,
            "subpolar" => Zone.Subpolar,
            "temperate" => Zone.Temperate,
            "subtropical" => Zone.Subtropical,
            "tropical" => Zone.Tropical,
            "arid" => Zone.Arid,
            "extreme" => Zone.Extreme,
            _ => Zone.Extreme,
        };
    }

    /// <summary>
    /// Returns the number of climate-zone types.
    /// </summary>
    public static int Count() => 7;
}
