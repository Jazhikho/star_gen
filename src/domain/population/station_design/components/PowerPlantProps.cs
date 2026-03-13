namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a power plant type.
/// </summary>
public readonly record struct PowerPlantProps(
    string DisplayName,
    double TonsPerPowerPoint,
    long CostPerTon,
    double FuelPerPointPerMonth);
