using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Domain.Generation.Science;

/// <summary>
/// Produces diagnostic-only planet-level habitable-zone records.
/// </summary>
public static class PlanetHabitableZoneDiagnosticEngine
{
    /// <summary>
    /// Evaluates a Kopparapu-2014-style mass-corrected HZ diagnostic for a generated planet.
    /// </summary>
    public static PlanetHabitableZoneDiagnostic EvaluateMassCorrectedKopparapu2014(
        PhysicalProps physical,
        OrbitalProps orbital,
        ParentContext context)
    {
        double massEarth = physical.MassKg / Units.EarthMassKg;
        double boundedMassEarth = System.Math.Clamp(massEarth, 0.10, 5.0);
        double massLog5 = System.Math.Log(boundedMassEarth) / System.Math.Log(5.0);
        double boundedMassLog5 = System.Math.Clamp(massLog5, -1.45, 1.0);
        double innerFluxCorrection = System.Math.Clamp(1.0 + (0.08 * boundedMassLog5), 0.90, 1.08);
        double outerFluxCorrection = System.Math.Clamp(1.0 + (0.02 * boundedMassLog5), 0.96, 1.03);

        double baseInnerM = OrbitalMechanics.CalculateHabitableZoneInner(
            context.StellarLuminosityWatts,
            context.StellarTemperatureK,
            PlanetHabitableZoneModel.Kopparapu2013Conservative);
        double baseOuterM = OrbitalMechanics.CalculateHabitableZoneOuter(
            context.StellarLuminosityWatts,
            context.StellarTemperatureK,
            PlanetHabitableZoneModel.Kopparapu2013Conservative);

        double innerAu = (baseInnerM / Units.AuMeters) / System.Math.Sqrt(innerFluxCorrection);
        double outerAu = (baseOuterM / Units.AuMeters) / System.Math.Sqrt(outerFluxCorrection);
        double orbitAu = orbital.SemiMajorAxisM / Units.AuMeters;

        return new PlanetHabitableZoneDiagnostic
        {
            PlanetMassEarth = massEarth,
            InnerAu = innerAu,
            OuterAu = outerAu,
            OrbitAu = orbitAu,
            Alignment = CalculateAlignment(orbitAu, innerAu, outerAu),
            InnerFluxCorrectionScalar = innerFluxCorrection,
        };
    }

    private static double CalculateAlignment(double orbitAu, double innerAu, double outerAu)
    {
        if (orbitAu <= 0.0 || innerAu <= 0.0 || outerAu <= innerAu)
        {
            return 0.0;
        }

        if (orbitAu >= innerAu && orbitAu <= outerAu)
        {
            return 1.0;
        }

        double innerDecayFloor = innerAu * 0.45;
        double outerDecayCeiling = outerAu * 1.80;
        if (orbitAu < innerAu)
        {
            return System.Math.Clamp((orbitAu - innerDecayFloor) / System.Math.Max(innerAu - innerDecayFloor, 0.01), 0.0, 1.0);
        }

        return System.Math.Clamp((outerDecayCeiling - orbitAu) / System.Math.Max(outerDecayCeiling - outerAu, 0.01), 0.0, 1.0);
    }
}
