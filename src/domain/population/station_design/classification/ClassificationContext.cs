namespace StarGen.Domain.Population.StationDesign.Classification;

/// <summary>
/// Slice of a design result used by classification rules.
/// </summary>
public sealed class ClassificationContext
{
    public ComponentCounts<FacilityKind> Facilities = new();
    public ComponentCounts<DockingBerthKind> Docking = new();
    public ComponentCounts<DefensiveScreen> Screens = new();
    public SensorGrade Sensors;
    public CommandCenterKind CommandCenter;
    public int ComputerRating;
    public int Hardpoints;
    public int HardpointsUsed;

    /// <summary>
    /// Builds a context from a completed design result.
    /// </summary>
    public static ClassificationContext FromResult(DesignResult result)
    {
        return new ClassificationContext
        {
            Facilities = result.Selection.Facilities,
            Docking = result.Selection.Docking,
            Screens = result.Selection.Screens,
            Sensors = result.Selection.Sensors,
            CommandCenter = result.Selection.CommandCenter,
            ComputerRating = result.Selection.ComputerRating,
            Hardpoints = result.Hardpoints,
            HardpointsUsed = result.HardpointsUsed,
        };
    }
}
