namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a station facility module.
/// </summary>
public readonly record struct FacilityProps(
    string DisplayName,
    int Tonnage,
    long Cost,
    int Power,
    string Description);
