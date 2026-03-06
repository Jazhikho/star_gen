using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators.Moon;

/// <summary>
/// Generates physical properties for moons.
/// </summary>
public static class MoonPhysicalGenerator
{
    /// <summary>
    /// Generates physical properties for a moon.
    /// </summary>
    public static PhysicalProps GeneratePhysicalProps(
        MoonSpec spec,
        ParentContext context,
        SizeCategory.Category sizeCategory,
        OrbitalProps orbital,
        SeededRng rng)
    {
        double densityKgM3 = spec.GetOverrideFloat("physical.density_kg_m3", -1.0);
        if (densityKgM3 < 0.0)
        {
            densityKgM3 = SizeTable.RandomDensity(sizeCategory, rng);
        }

        double massKg = spec.GetOverrideFloat("physical.mass_kg", -1.0);
        if (massKg < 0.0)
        {
            double massEarth = spec.GetOverrideFloat("physical.mass_earth", -1.0);
            if (massEarth < 0.0)
            {
                massEarth = SizeTable.RandomMassEarth(sizeCategory, rng);
                double maxMoonMassKg = context.ParentBodyMassKg * 0.1;
                double maxMoonMassEarth = maxMoonMassKg / Units.EarthMassKg;
                massEarth = System.Math.Min(massEarth, maxMoonMassEarth);
            }

            massKg = massEarth * Units.EarthMassKg;
        }

        double radiusM = spec.GetOverrideFloat("physical.radius_m", -1.0);
        if (radiusM < 0.0)
        {
            double radiusEarth = spec.GetOverrideFloat("physical.radius_earth", -1.0);
            if (radiusEarth < 0.0)
            {
                radiusM = SizeTable.RadiusFromMassDensity(massKg, densityKgM3);
            }
            else
            {
                radiusM = radiusEarth * Units.EarthRadiusMeters;
            }
        }

        bool isLocked = IsTidallyLockedToParent(
            orbital.SemiMajorAxisM,
            massKg,
            radiusM,
            context.ParentBodyMassKg,
            context.StellarAgeYears);

        double rotationPeriodS = spec.GetOverrideFloat("physical.rotation_period_s", -1.0);
        if (rotationPeriodS < 0.0)
        {
            rotationPeriodS = CalculateRotationPeriod(
                orbital,
                context.ParentBodyMassKg,
                isLocked,
                rng);
        }

        double axialTiltDeg = spec.GetOverrideFloat("physical.axial_tilt_deg", -1.0);
        if (axialTiltDeg < 0.0)
        {
            axialTiltDeg = CalculateAxialTilt(isLocked, rng);
        }

        double oblateness = spec.GetOverrideFloat("physical.oblateness", -1.0);
        if (oblateness < 0.0)
        {
            if (isLocked)
            {
                oblateness = rng.RandfRange(0.0f, 0.005f);
            }
            else
            {
                oblateness = rng.RandfRange(0.0f, 0.02f);
            }
        }

        double magneticMoment = spec.GetOverrideFloat("physical.magnetic_moment", -1.0);
        if (magneticMoment < 0.0)
        {
            magneticMoment = CalculateMagneticMoment(massKg, radiusM, rotationPeriodS, rng);
        }

        double internalHeatWatts = spec.GetOverrideFloat("physical.internal_heat_watts", -1.0);
        if (internalHeatWatts < 0.0)
        {
            internalHeatWatts = CalculateInternalHeat(massKg, context.StellarAgeYears, rng);
        }

        return new PhysicalProps(
            massKg,
            radiusM,
            rotationPeriodS,
            axialTiltDeg,
            oblateness,
            magneticMoment,
            internalHeatWatts);
    }

    /// <summary>
    /// Calculates tidal heating for a moon.
    /// </summary>
    public static double CalculateTidalHeating(
        PhysicalProps physical,
        OrbitalProps orbital,
        ParentContext context)
    {
        double eccentricity = orbital.Eccentricity;
        if (eccentricity < 0.001)
        {
            return 0.0;
        }

        double distanceM = orbital.SemiMajorAxisM;
        double radiusM = physical.RadiusM;
        double parentMassKg = context.ParentBodyMassKg;
        if (distanceM <= 0.0 || radiusM <= 0.0)
        {
            return 0.0;
        }

        const double ioReferenceHeat = 1.0e14;
        const double ioEccentricity = 0.004;
        const double ioDistanceM = 4.218e8;
        const double ioRadiusM = 1.8216e6;
        const double jupiterMassKg = 1.898e27;

        double massRatio = System.Math.Pow(parentMassKg / jupiterMassKg, 2.0);
        double radiusRatio = System.Math.Pow(radiusM / ioRadiusM, 5.0);
        double eccentricityRatio = System.Math.Pow(eccentricity / ioEccentricity, 2.0);
        double distanceRatio = System.Math.Pow(ioDistanceM / distanceM, 6.0);
        double tidalHeat = ioReferenceHeat * massRatio * radiusRatio * eccentricityRatio * distanceRatio;
        return System.Math.Min(tidalHeat, 1.0e16);
    }

    /// <summary>Returns whether the moon is tidally locked to its parent.</summary>
    private static bool IsTidallyLockedToParent(
        double orbitalDistanceM,
        double massKg,
        double radiusM,
        double parentMassKg,
        double systemAgeYears)
    {
        if (radiusM <= 0.0 || parentMassKg <= 0.0)
        {
            return false;
        }

        double orbitalDistanceKm = orbitalDistanceM / 1000.0;
        double radiusKm = radiusM / 1000.0;
        double tau = 1.0e8
            * System.Math.Pow(orbitalDistanceKm / 384400.0, 6.0)
            * (massKg / 7.35e22)
            / (System.Math.Pow(parentMassKg / 5.97e24, 2.0) * System.Math.Pow(radiusKm / 1737.0, 3.0));
        return systemAgeYears > tau;
    }

    /// <summary>Computes rotation period; locked moons match orbital period.</summary>
    private static double CalculateRotationPeriod(
        OrbitalProps orbital,
        double parentMassKg,
        bool isLocked,
        SeededRng rng)
    {
        if (isLocked)
        {
            return orbital.GetOrbitalPeriodS(parentMassKg);
        }

        return rng.RandfRange(5.0f, 50.0f) * 3600.0;
    }

    /// <summary>Computes axial tilt; locked bodies remain near-zero.</summary>
    private static double CalculateAxialTilt(bool isLocked, SeededRng rng)
    {
        if (isLocked)
        {
            return rng.RandfRange(0.0f, 5.0f);
        }

        return rng.RandfRange(0.0f, 30.0f);
    }

    /// <summary>Estimates magnetic moment for the moon.</summary>
    private static double CalculateMagneticMoment(
        double massKg,
        double radiusM,
        double rotationPeriodS,
        SeededRng rng)
    {
        _ = radiusM;
        double massEarth = massKg / Units.EarthMassKg;
        double rotationHours = System.Math.Abs(rotationPeriodS) / 3600.0;

        if (massEarth < 0.01 || rotationHours > 100.0)
        {
            return 0.0;
        }

        if (rng.Randf() < 0.9f)
        {
            return 0.0;
        }

        const double earthMoment = 8.0e22;
        double baseMoment = earthMoment
            * System.Math.Pow(massEarth, 0.5)
            * (24.0 / System.Math.Max(rotationHours, 1.0))
            * 0.1;
        return baseMoment * rng.RandfRange(0.01f, 0.5f);
    }

    /// <summary>Estimates internal heat from mass and age.</summary>
    private static double CalculateInternalHeat(double massKg, double ageYears, SeededRng rng)
    {
        const double earthHeat = 4.7e13;
        double massEarth = massKg / Units.EarthMassKg;
        double ageFactor = 1.0;
        if (ageYears > 0.0)
        {
            ageFactor = System.Math.Pow(0.5, ageYears / 2.0e9);
            ageFactor = System.Math.Max(ageFactor, 0.1);
        }

        double sizeFactor = System.Math.Pow(massEarth, 0.8);
        double baseHeat = earthHeat * sizeFactor * ageFactor * 0.5;
        return baseHeat * rng.RandfRange(0.3f, 1.5f);
    }
}
