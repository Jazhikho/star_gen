using System.Collections.Generic;

namespace StarGen.Domain.Population.StationDesign.Presets;

/// <summary>
/// Describes the weapon allocation ratios for a design preset.
/// </summary>
public sealed class WeaponAllocation
{
    /// <summary>
    /// Fraction of hardpoints used for turrets (0.0 – 1.0).
    /// </summary>
    public double TurretHardpointFraction { get; init; }

    /// <summary>
    /// Mix ratios for turret types (should sum to ~1.0).
    /// </summary>
    public IReadOnlyDictionary<TurretMount, double> TurretMix { get; init; } =
        new Dictionary<TurretMount, double>();

    /// <summary>
    /// Fraction of hull tonnage allocated to bay weapons.
    /// </summary>
    public double BayTonnageFraction { get; init; }

    /// <summary>
    /// Mix ratios for bay weapon types (should sum to ~1.0).
    /// </summary>
    public IReadOnlyDictionary<BayWeapon, double> BayMix { get; init; } =
        new Dictionary<BayWeapon, double>();
}
