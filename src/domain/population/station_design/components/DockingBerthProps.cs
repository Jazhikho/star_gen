namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a docking berth type.
/// </summary>
public readonly record struct DockingBerthProps(
    string DisplayName,
    int Tonnage,
    long Cost,
    string Description);
