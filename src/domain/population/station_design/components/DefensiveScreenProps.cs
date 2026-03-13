namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a defensive screen system.
/// </summary>
public readonly record struct DefensiveScreenProps(
    string DisplayName,
    int Tonnage,
    long Cost,
    int Power);
