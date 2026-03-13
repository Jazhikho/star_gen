namespace StarGen.Domain.Population.StationDesign.Components;

/// <summary>
/// Properties for an armor material.
/// </summary>
public readonly record struct ArmorMaterialProps(
    string DisplayName,
    double TonsPerPointPerHullTon,
    long CostPerTon);
