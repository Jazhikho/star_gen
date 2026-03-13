namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for a sensor package grade.
/// </summary>
public readonly record struct SensorGradeProps(
    string DisplayName,
    int Tonnage,
    long Cost,
    int Power,
    int DiceModifier);
