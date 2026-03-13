namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Design template that drives auto-populated component presets.
/// Orthogonal to StationType (location) and StationClass (population size).
/// </summary>
public enum DesignTemplate
{
    HighportA,
    HighportB,
    HighportC,
    HighportD,
    Naval,
    Scout,
    Research,
    Mining,
    Trade,
    Waystation,
    Defense,
    Freeport,
    Custom,
}
