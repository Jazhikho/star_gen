using System.Collections.Generic;

namespace StarGen.Domain.Population.StationDesign.Presets;

/// <summary>
/// Complete design preset for a station template. Drives auto-population.
/// </summary>
public sealed class DesignPreset
{
    /// <summary>
    /// Command center type.
    /// </summary>
    public CommandCenterKind CommandCenter { get; init; }

    /// <summary>
    /// Computer rating.
    /// </summary>
    public int ComputerRating { get; init; }

    /// <summary>
    /// Sensor grade.
    /// </summary>
    public SensorGrade Sensors { get; init; }

    /// <summary>
    /// Power plant type.
    /// </summary>
    public PowerPlantKind PowerPlant { get; init; }

    /// <summary>
    /// Power margin multiplier (e.g. 1.2 = 20% surplus).
    /// </summary>
    public double PowerMargin { get; init; }

    /// <summary>
    /// Fuel reserve in months.
    /// </summary>
    public int FuelMonths { get; init; }

    /// <summary>
    /// Armor material.
    /// </summary>
    public ArmorMaterial ArmorMaterial { get; init; }

    /// <summary>
    /// Armor scaling rule.
    /// </summary>
    public ArmorScalingRule ArmorRule { get; init; }

    /// <summary>
    /// Default software loadout.
    /// </summary>
    public IReadOnlyList<SoftwarePackage> Software { get; init; } = System.Array.Empty<SoftwarePackage>();

    /// <summary>
    /// Facility scaling rules keyed by facility type.
    /// </summary>
    public IReadOnlyDictionary<FacilityKind, ScalingRule> Facilities { get; init; } =
        new Dictionary<FacilityKind, ScalingRule>();

    /// <summary>
    /// Docking berth scaling rules.
    /// </summary>
    public IReadOnlyDictionary<DockingBerthKind, ScalingRule> Docking { get; init; } =
        new Dictionary<DockingBerthKind, ScalingRule>();

    /// <summary>
    /// Weapon allocation.
    /// </summary>
    public WeaponAllocation Weapons { get; init; } = new WeaponAllocation();

    /// <summary>
    /// Screen scaling rules.
    /// </summary>
    public IReadOnlyDictionary<DefensiveScreen, ScalingRule> Screens { get; init; } =
        new Dictionary<DefensiveScreen, ScalingRule>();

    /// <summary>
    /// Officer ratio for accommodation auto-population.
    /// </summary>
    public double OfficerRatio { get; init; } = 0.2;
}
