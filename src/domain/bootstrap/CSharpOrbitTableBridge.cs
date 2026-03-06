using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Tables;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for pure orbit-table helpers.
/// </summary>
[GlobalClass]
public partial class CSharpOrbitTableBridge : RefCounted
{
    /// <summary>
    /// Returns the orbital-distance range for a zone as a dictionary with "min" and "max" keys.
    /// </summary>
    public Dictionary GetDistanceRange(int zone, double stellarLuminosityWatts)
    {
        return OrbitTable.GetDistanceRange((OrbitZone.Zone)zone, stellarLuminosityWatts);
    }

    /// <summary>
    /// Returns the eccentricity range for a zone as a dictionary with "min" and "max" keys.
    /// </summary>
    public Dictionary GetEccentricityRange(int zone)
    {
        return OrbitTable.GetEccentricityRange((OrbitZone.Zone)zone);
    }

    /// <summary>
    /// Estimates the tidal-locking timescale in years.
    /// </summary>
    public double TidalLockingTimescaleYears(
        double orbitalDistanceM,
        double bodyMassKg,
        double bodyRadiusM,
        double stellarMassKg)
    {
        return OrbitTable.TidalLockingTimescaleYears(orbitalDistanceM, bodyMassKg, bodyRadiusM, stellarMassKg);
    }

    /// <summary>
    /// Returns whether a body should be tidally locked.
    /// </summary>
    public bool IsTidallyLocked(
        double orbitalDistanceM,
        double bodyMassKg,
        double bodyRadiusM,
        double stellarMassKg,
        double systemAgeYears)
    {
        return OrbitTable.IsTidallyLocked(orbitalDistanceM, bodyMassKg, bodyRadiusM, stellarMassKg, systemAgeYears);
    }
}
