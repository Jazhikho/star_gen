namespace StarGen.Domain.Population;

/// <summary>
/// Context describing why and where a station is placed.
/// </summary>
public static class StationPlacementContext
{
    /// <summary>
    /// Placement-context types.
    /// </summary>
    public enum Context
    {
        BridgeSystem,
        ColonyWorld,
        NativeWorld,
        ResourceSystem,
        Strategic,
        Scientific,
        Other,
    }

    /// <summary>
    /// Converts a context to a display string.
    /// </summary>
    public static string ToStringName(Context context)
    {
        return context switch
        {
            Context.BridgeSystem => "Bridge System",
            Context.ColonyWorld => "Colony World",
            Context.NativeWorld => "Native World",
            Context.ResourceSystem => "Resource System",
            Context.Strategic => "Strategic",
            Context.Scientific => "Scientific",
            Context.Other => "Other",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a context from a string.
    /// </summary>
    public static Context FromString(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" ", "_").Trim();
        return normalized switch
        {
            "bridge_system" => Context.BridgeSystem,
            "bridge" => Context.BridgeSystem,
            "colony_world" => Context.ColonyWorld,
            "colony" => Context.ColonyWorld,
            "native_world" => Context.NativeWorld,
            "native" => Context.NativeWorld,
            "resource_system" => Context.ResourceSystem,
            "resource" => Context.ResourceSystem,
            "strategic" => Context.Strategic,
            "scientific" => Context.Scientific,
            "science" => Context.Scientific,
            "other" => Context.Other,
            _ => Context.Other,
        };
    }

    /// <summary>
    /// Returns whether this context favors utility stations.
    /// </summary>
    public static bool FavorsUtilityStations(Context context)
    {
        return context == Context.BridgeSystem;
    }

    /// <summary>
    /// Returns whether this context can support large stations.
    /// </summary>
    public static bool CanSupportLargeStations(Context context)
    {
        return context == Context.ColonyWorld
            || context == Context.NativeWorld
            || context == Context.ResourceSystem;
    }

    /// <summary>
    /// Returns whether this context requires spacefaring natives.
    /// </summary>
    public static bool RequiresSpacefaringNatives(Context context)
    {
        return context == Context.NativeWorld;
    }

    /// <summary>
    /// Returns the number of placement contexts.
    /// </summary>
    public static int Count()
    {
        return 7;
    }
}
