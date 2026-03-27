namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic context describing native-population pressure around a colony target.
/// </summary>
public sealed class ColonyPressureContext
{
    /// <summary>
    /// Native-pressure signal from worlds inside the same system.
    /// </summary>
    public double LocalNativePressure;

    /// <summary>
    /// Native-pressure signal from nearby star systems.
    /// </summary>
    public double NearbySystemNativePressure;

    /// <summary>
    /// Count of native-inhabited worlds contributing from the same system.
    /// </summary>
    public int LocalNativeWorldCount;

    /// <summary>
    /// Count of native-inhabited worlds contributing from nearby systems.
    /// </summary>
    public int NearbyNativeWorldCount;

    /// <summary>
    /// Returns a combined pressure score in the inclusive range [0, 1].
    /// </summary>
    public double GetCombinedPressure()
    {
        double combined = (LocalNativePressure * 0.65) + (NearbySystemNativePressure * 0.35);
        return System.Math.Clamp(combined, 0.0, 1.0);
    }

    /// <summary>
    /// Returns a combined native-world count for diagnostics and scoring.
    /// </summary>
    public int GetCombinedNativeWorldCount()
    {
        return LocalNativeWorldCount + NearbyNativeWorldCount;
    }
}
