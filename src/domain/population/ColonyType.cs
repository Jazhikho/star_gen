namespace StarGen.Domain.Population;

/// <summary>
/// Colony type classification based on founding purpose.
/// </summary>
public static class ColonyType
{
    /// <summary>
    /// Colony founding purposes.
    /// </summary>
    public enum Type
    {
        Settlement,
        Corporate,
        Military,
        Scientific,
        Penal,
        Religious,
        Agricultural,
        Industrial,
        Refugee,
        Separatist,
    }

    /// <summary>
    /// Converts a colony type to a display string.
    /// </summary>
    public static string ToStringName(Type type)
    {
        return type switch
        {
            Type.Settlement => "Settlement",
            Type.Corporate => "Corporate",
            Type.Military => "Military",
            Type.Scientific => "Scientific",
            Type.Penal => "Penal",
            Type.Religious => "Religious",
            Type.Agricultural => "Agricultural",
            Type.Industrial => "Industrial",
            Type.Refugee => "Refugee",
            Type.Separatist => "Separatist",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a colony type from a string.
    /// </summary>
    public static Type FromString(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "settlement" => Type.Settlement,
            "corporate" => Type.Corporate,
            "military" => Type.Military,
            "scientific" => Type.Scientific,
            "penal" => Type.Penal,
            "religious" => Type.Religious,
            "agricultural" => Type.Agricultural,
            "industrial" => Type.Industrial,
            "refugee" => Type.Refugee,
            "separatist" => Type.Separatist,
            _ => Type.Settlement,
        };
    }

    /// <summary>
    /// Returns the typical starting regime for a colony type.
    /// </summary>
    public static GovernmentType.Regime TypicalStartingRegime(Type type)
    {
        return type switch
        {
            Type.Settlement => GovernmentType.Regime.Constitutional,
            Type.Corporate => GovernmentType.Regime.Corporate,
            Type.Military => GovernmentType.Regime.MilitaryJunta,
            Type.Scientific => GovernmentType.Regime.Technocracy,
            Type.Penal => GovernmentType.Regime.MilitaryJunta,
            Type.Religious => GovernmentType.Regime.Theocracy,
            Type.Agricultural => GovernmentType.Regime.Constitutional,
            Type.Industrial => GovernmentType.Regime.Corporate,
            Type.Refugee => GovernmentType.Regime.Tribal,
            Type.Separatist => GovernmentType.Regime.EliteRepublic,
            _ => GovernmentType.Regime.Constitutional,
        };
    }

    /// <summary>
    /// Returns the typical initial population for a colony type.
    /// </summary>
    public static int TypicalInitialPopulation(Type type)
    {
        return type switch
        {
            Type.Settlement => 10000,
            Type.Corporate => 5000,
            Type.Military => 2000,
            Type.Scientific => 500,
            Type.Penal => 20000,
            Type.Religious => 3000,
            Type.Agricultural => 8000,
            Type.Industrial => 15000,
            Type.Refugee => 50000,
            Type.Separatist => 25000,
            _ => 10000,
        };
    }

    /// <summary>
    /// Returns the growth-rate modifier for a colony type.
    /// </summary>
    public static double GrowthRateModifier(Type type)
    {
        return type switch
        {
            Type.Settlement => 1.0,
            Type.Corporate => 0.7,
            Type.Military => 0.5,
            Type.Scientific => 0.4,
            Type.Penal => 0.8,
            Type.Religious => 1.3,
            Type.Agricultural => 1.2,
            Type.Industrial => 0.9,
            Type.Refugee => 1.1,
            Type.Separatist => 1.0,
            _ => 1.0,
        };
    }

    /// <summary>
    /// Returns whether this colony type tends toward native conflict.
    /// </summary>
    public static bool TendsTowardNativeConflict(Type type)
    {
        return type == Type.Corporate
            || type == Type.Military
            || type == Type.Penal
            || type == Type.Industrial;
    }

    /// <summary>
    /// Returns the number of colony types.
    /// </summary>
    public static int Count()
    {
        return 10;
    }
}
