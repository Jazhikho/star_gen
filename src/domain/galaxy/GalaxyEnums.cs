namespace StarGen.Domain.Galaxy;

/// <summary>
/// Controls how strongly the selected galaxy family should be biased toward earlier or later subtypes.
/// </summary>
public enum GalaxySubtypeMode
{
    Automatic = 0,
    EarlyType = 1,
    IntermediateType = 2,
    LateType = 3,
}

/// <summary>
/// Controls whether disk galaxies should prefer barred or unbarred realizations.
/// </summary>
public enum GalaxyBarMode
{
    Auto = 0,
    PreferBarred = 1,
    PreferUnbarred = 2,
}

/// <summary>
/// Controls the preferred spiral-arm mechanism for disk galaxies.
/// </summary>
public enum GalaxyArmMechanism
{
    Auto = 0,
    GrandDesign = 1,
    MultiArmed = 2,
    Flocculent = 3,
}

/// <summary>
/// Controls whether galaxy dynamics diagnostics are readout-only or allowed to influence downstream behavior.
/// </summary>
public enum GalaxyDynamicsBehaviorMode
{
    DiagnosticsOnly = 0,
    AffectRegionContext = 1,
    AffectPlacement = 2,
}

/// <summary>
/// Persistence helpers for galaxy dynamics behavior modes.
/// </summary>
public static class GalaxyDynamicsBehaviorModePersistence
{
    /// <summary>
    /// Converts a mode to its stable save-string value.
    /// </summary>
    public static string ToPersistenceString(GalaxyDynamicsBehaviorMode mode)
    {
        if (mode == GalaxyDynamicsBehaviorMode.AffectRegionContext)
        {
            return "affect_region_context";
        }

        if (mode == GalaxyDynamicsBehaviorMode.AffectPlacement)
        {
            return "affect_placement";
        }

        return "diagnostics_only";
    }

    /// <summary>
    /// Parses a stable save-string value, defaulting to diagnostic-only for old saves.
    /// </summary>
    public static GalaxyDynamicsBehaviorMode FromPersistenceString(string value)
    {
        if (value == "affect_region_context")
        {
            return GalaxyDynamicsBehaviorMode.AffectRegionContext;
        }

        if (value == "affect_placement")
        {
            return GalaxyDynamicsBehaviorMode.AffectPlacement;
        }

        return GalaxyDynamicsBehaviorMode.DiagnosticsOnly;
    }
}

/// <summary>
/// Resolved scientific subtype chosen inside the current top-level family lock.
/// </summary>
public enum GalaxyResolvedSubtype
{
    SpiralSa = 0,
    SpiralSb = 1,
    SpiralSc = 2,
    SpiralSd = 3,
    EllipticalDwarf = 4,
    EllipticalIntermediate = 5,
    EllipticalGiant = 6,
    LenticularS0 = 7,
    LenticularS0a = 8,
    IrregularMagellanic = 9,
    DwarfIrregular = 10,
    DwarfSpheroidal = 11,
}

/// <summary>
/// Broad age cohort derived from the host galaxy environment.
/// </summary>
public enum GalaxyAgeCohort
{
    Young = 0,
    Mature = 1,
    Old = 2,
    Ancient = 3,
}

/// <summary>
/// Region classification used when feeding galaxy-derived context into downstream generation.
/// </summary>
public enum GalaxyRegionKind
{
    Core = 0,
    Bulge = 1,
    Bar = 2,
    InnerDisk = 3,
    SpiralArm = 4,
    OuterDisk = 5,
    Halo = 6,
    LenticularDisk = 7,
    IrregularBody = 8,
    DwarfEnvelope = 9,
}
