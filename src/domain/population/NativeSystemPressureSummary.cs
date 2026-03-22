namespace StarGen.Domain.Population;

/// <summary>
/// Cached summary of native-world pressure produced by a star system.
/// </summary>
public sealed class NativeSystemPressureSummary
{
    /// <summary>
    /// Shared empty summary instance.
    /// </summary>
    public static readonly NativeSystemPressureSummary Empty = new();

    /// <summary>
    /// Number of extant native-inhabited worlds in the system.
    /// </summary>
    public int NativeWorldCount;

    /// <summary>
    /// Aggregated pressure signal in the inclusive range [0, 1].
    /// </summary>
    public double PressureSignal;

    /// <summary>
    /// Returns a detached copy.
    /// </summary>
    public NativeSystemPressureSummary Clone()
    {
        return new NativeSystemPressureSummary
        {
            NativeWorldCount = NativeWorldCount,
            PressureSignal = PressureSignal,
        };
    }
}
