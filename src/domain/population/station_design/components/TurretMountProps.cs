namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a turret or barbette mount.
/// </summary>
public readonly record struct TurretMountProps(
    string DisplayName,
    int Tonnage,
    long Cost,
    int Power,
    int HardpointsConsumed);
