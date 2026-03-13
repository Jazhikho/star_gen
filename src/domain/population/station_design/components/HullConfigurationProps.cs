namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a hull configuration.
/// </summary>
public readonly record struct HullConfigurationProps(
    string DisplayName,
    double CostMultiplier,
    double ArmorMultiplier,
    string Description);
