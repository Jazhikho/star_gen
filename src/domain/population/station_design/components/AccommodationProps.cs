namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for an accommodation type.
/// </summary>
public readonly record struct AccommodationProps(
    string DisplayName,
    double Tonnage,
    long Cost,
    int Occupancy,
    string Description);
