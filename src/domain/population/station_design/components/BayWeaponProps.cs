namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a bay weapon system.
/// </summary>
public readonly record struct BayWeaponProps(
    string DisplayName,
    int Tonnage,
    long Cost,
    int Power);
