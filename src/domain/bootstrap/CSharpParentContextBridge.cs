using Godot;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for parent-context calculations.
/// </summary>
[GlobalClass]
public partial class CSharpParentContextBridge : RefCounted
{
    /// <summary>
    /// Calculates the Hill-sphere radius in meters.
    /// </summary>
    public double GetHillSphereRadiusM(
        double stellarMassKg,
        double orbitalDistanceFromStarM,
        double parentBodyMassKg)
    {
        ParentContext context = new(
            stellarMassKg: stellarMassKg,
            orbitalDistanceFromStarM: orbitalDistanceFromStarM,
            parentBodyMassKg: parentBodyMassKg);
        return context.GetHillSphereRadiusM();
    }

    /// <summary>
    /// Calculates the Roche limit in meters.
    /// </summary>
    public double GetRocheLimitM(
        double parentBodyMassKg,
        double parentBodyRadiusM,
        double satelliteDensityKgM3)
    {
        ParentContext context = new(
            parentBodyMassKg: parentBodyMassKg,
            parentBodyRadiusM: parentBodyRadiusM);
        return context.GetRocheLimitM(satelliteDensityKgM3);
    }

    /// <summary>
    /// Calculates equilibrium temperature in Kelvin.
    /// </summary>
    public double GetEquilibriumTemperatureK(
        double stellarLuminosityWatts,
        double orbitalDistanceFromStarM,
        double albedo = 0.3)
    {
        ParentContext context = new(
            stellarLuminosityWatts: stellarLuminosityWatts,
            orbitalDistanceFromStarM: orbitalDistanceFromStarM);
        return context.GetEquilibriumTemperatureK(albedo);
    }
}
