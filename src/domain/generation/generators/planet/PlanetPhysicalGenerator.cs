using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators.Planet;

/// <summary>
/// Generates physical properties for planets.
/// </summary>
public static class PlanetPhysicalGenerator
{
    /// <summary>
    /// Generates physical properties for a planet.
    /// </summary>
    public static PhysicalProps GeneratePhysicalProps(
        PlanetSpec spec,
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
        double massEarth;
        if (massKg < 0.0)
        {
            massEarth = spec.GetOverrideFloat("physical.mass_earth", -1.0);
            if (massEarth < 0.0)
            {
                massEarth = SizeTable.RandomMassEarth(sizeCategory, rng);
            }

            massKg = massEarth * Units.EarthMassKg;
        }
        else
        {
            massEarth = massKg / Units.EarthMassKg;
        }

        double radiusM = spec.GetOverrideFloat("physical.radius_m", -1.0);
        if (radiusM < 0.0)
        {
            double radiusEarth = spec.GetOverrideFloat("physical.radius_earth", -1.0);
            radiusM = radiusEarth < 0.0
                ? SizeTable.RadiusFromMassDensity(massKg, densityKgM3)
                : radiusEarth * Units.EarthRadiusMeters;
        }

        bool isLocked = OrbitTable.IsTidallyLocked(
            orbital.SemiMajorAxisM,
            massKg,
            radiusM,
            context.StellarMassKg,
            context.StellarAgeYears);

        double rotationPeriodS = spec.GetOverrideFloat("physical.rotation_period_s", -1.0);
        if (rotationPeriodS < 0.0)
        {
            rotationPeriodS = CalculateRotationPeriod(massKg, orbital, context.StellarMassKg, isLocked, rng);
        }

        double axialTiltDeg = spec.GetOverrideFloat("physical.axial_tilt_deg", -1.0);
        if (axialTiltDeg < 0.0)
        {
            axialTiltDeg = CalculateAxialTilt(isLocked, rng);
        }

        double oblateness = spec.GetOverrideFloat("physical.oblateness", -1.0);
        if (oblateness < 0.0)
        {
            oblateness = CalculateOblateness(massKg, radiusM, rotationPeriodS, sizeCategory);
        }

        double magneticMoment = spec.GetOverrideFloat("physical.magnetic_moment", -1.0);
        if (magneticMoment < 0.0)
        {
            magneticMoment = CalculateMagneticMoment(massKg, radiusM, rotationPeriodS, sizeCategory, rng);
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

    private static double CalculateRotationPeriod(
        double massKg,
        OrbitalProps orbital,
        double stellarMassKg,
        bool isLocked,
        SeededRng rng)
    {
        if (isLocked)
        {
            return orbital.GetOrbitalPeriodS(stellarMassKg);
        }

        double massEarth = massKg / Units.EarthMassKg;
        double baseHours = massEarth switch
        {
            < 1.0 => rng.RandfRange(15.0f, 50.0f),
            < 10.0 => rng.RandfRange(10.0f, 30.0f),
            < 100.0 => rng.RandfRange(8.0f, 20.0f),
            _ => rng.RandfRange(8.0f, 15.0f),
        };

        double periodHours = baseHours * rng.RandfRange(0.8f, 1.2f);
        if (rng.Randf() < 0.05f)
        {
            periodHours = -periodHours;
        }

        return periodHours * 3600.0;
    }

    private static double CalculateAxialTilt(bool isLocked, SeededRng rng)
    {
        if (isLocked)
        {
            return rng.RandfRange(0.0f, 10.0f);
        }

        double roll = rng.Randf();
        if (roll < 0.6)
        {
            return rng.RandfRange(0.0f, 30.0f);
        }

        if (roll < 0.9)
        {
            return rng.RandfRange(30.0f, 60.0f);
        }

        if (roll < 0.98)
        {
            return rng.RandfRange(60.0f, 90.0f);
        }

        return rng.RandfRange(90.0f, 180.0f);
    }

    private static double CalculateOblateness(
        double massKg,
        double radiusM,
        double rotationPeriodS,
        SizeCategory.Category sizeCategory)
    {
        if (rotationPeriodS == 0.0 || radiusM <= 0.0)
        {
            return 0.0;
        }

        double omega = 2.0 * System.Math.PI / System.Math.Abs(rotationPeriodS);
        double fluidOblateness = (5.0 / 4.0) * omega * omega * System.Math.Pow(radiusM, 3.0) / (PhysicalProps.G * massKg);
        double rigidityFactor = SizeCategory.IsRocky(sizeCategory) ? 0.3 : 0.8;
        return System.Math.Clamp(fluidOblateness * rigidityFactor, 0.0, 0.15);
    }

    private static double CalculateMagneticMoment(
        double massKg,
        double radiusM,
        double rotationPeriodS,
        SizeCategory.Category sizeCategory,
        SeededRng rng)
    {
        const double earthMoment = 8.0e22;
        double rotationHours = System.Math.Abs(rotationPeriodS) / 3600.0;
        if (rotationHours > 100.0 && SizeCategory.IsRocky(sizeCategory))
        {
            return 0.0;
        }

        double massEarth = massKg / Units.EarthMassKg;
        double radiusEarth = radiusM / Units.EarthRadiusMeters;
        double baseMoment;

        if (SizeCategory.IsGaseous(sizeCategory))
        {
            baseMoment = earthMoment
                * System.Math.Pow(massEarth, 0.8)
                * (24.0 / System.Math.Max(rotationHours, 1.0));
        }
        else
        {
            if (massEarth < 0.1)
            {
                return 0.0;
            }

            baseMoment = earthMoment
                * System.Math.Pow(massEarth, 0.5)
                * radiusEarth
                * (24.0 / System.Math.Max(rotationHours, 1.0));
        }

        double variation = rng.RandfRange(0.3f, 3.0f);
        if (rng.Randf() < 0.15f)
        {
            return 0.0;
        }

        return baseMoment * variation;
    }

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

        double baseHeat = earthHeat * System.Math.Pow(massEarth, 0.9) * ageFactor;
        double variation = rng.RandfRange(0.5f, 2.0f);
        return baseHeat * variation;
    }
}
