namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Crew requirement breakdown by department.
/// </summary>
public sealed class CrewBreakdown
{
    public int Command;
    public int Engineering;
    public int Gunnery;
    public int Docking;
    public int Maintenance;
    public int Medical;
    public int Security;
    public int Facilities;
    public int Admin;
    public int Total;
}

/// <summary>
/// Tonnage allocation breakdown by system.
/// </summary>
public sealed class TonnageBreakdown
{
    public int Armor;
    public int Command;
    public int Sensors;
    public int PowerPlant;
    public int Fuel;
    public int Weapons;
    public int Screens;
    public int Docking;
    public int Quarters;
    public int Facilities;
    public int Used;
    public int Cargo;
}

/// <summary>
/// Credit cost breakdown by system.
/// </summary>
public sealed class CostBreakdown
{
    public long Hull;
    public long Armor;
    public long Command;
    public long Computer;
    public long Sensors;
    public long PowerPlant;
    public long Weapons;
    public long Screens;
    public long Docking;
    public long Quarters;
    public long Facilities;
    public long Software;
    public long Total;
}

/// <summary>
/// Power production and consumption summary.
/// </summary>
public sealed class PowerBudget
{
    public int Demand;
    public int Output;
    public int Surplus;
}
