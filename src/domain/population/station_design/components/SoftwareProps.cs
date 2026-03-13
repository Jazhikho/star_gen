namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a software package.
/// </summary>
public readonly record struct SoftwareProps(
    string DisplayName,
    long Cost);
