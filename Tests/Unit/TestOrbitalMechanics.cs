#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for OrbitalMechanics utility class.
/// </summary>
public static class TestOrbitalMechanics
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests orbital period calculation for Earth.
    /// </summary>
    public static void TestCalculateOrbitalPeriodEarth()
    {
        double periodS = OrbitalMechanics.CalculateOrbitalPeriod(
            Units.AuMeters,
            Units.SolarMassKg
        );

        double periodYears = periodS / (365.25 * 24.0 * 3600.0);
        if (System.Math.Abs(periodYears - 1.0) > 0.01)
        {
            throw new InvalidOperationException("Earth orbital period should be ~1 year");
        }
    }

    /// <summary>
    /// Tests orbital period calculation for Jupiter.
    /// </summary>
    public static void TestCalculateOrbitalPeriodJupiter()
    {
        double periodS = OrbitalMechanics.CalculateOrbitalPeriod(
            5.2 * Units.AuMeters,
            Units.SolarMassKg
        );

        double periodYears = periodS / (365.25 * 24.0 * 3600.0);
        if (System.Math.Abs(periodYears - 11.86) > 0.5)
        {
            throw new InvalidOperationException("Jupiter orbital period should be ~11.86 years");
        }
    }

    /// <summary>
    /// Tests semi-major axis calculation (inverse of period).
    /// </summary>
    public static void TestCalculateSemiMajorAxis()
    {
        double oneYearS = 365.25 * 24.0 * 3600.0;
        double a = OrbitalMechanics.CalculateSemiMajorAxis(oneYearS, Units.SolarMassKg);

        if (System.Math.Abs(a - Units.AuMeters) > Units.AuMeters * 0.01)
        {
            throw new InvalidOperationException("1 year period should give 1 AU");
        }
    }

    /// <summary>
    /// Tests semi-major axis edge cases return zero.
    /// </summary>
    public static void TestCalculateSemiMajorAxisEdgeCases()
    {
        if (OrbitalMechanics.CalculateSemiMajorAxis(0.0, Units.SolarMassKg) != 0.0)
        {
            throw new InvalidOperationException("Zero period should return 0");
        }
        if (OrbitalMechanics.CalculateSemiMajorAxis(365.25 * 24.0 * 3600.0, 0.0) != 0.0)
        {
            throw new InvalidOperationException("Zero mass should return 0");
        }
        if (OrbitalMechanics.CalculateSemiMajorAxis(-1.0, Units.SolarMassKg) != 0.0)
        {
            throw new InvalidOperationException("Negative period should return 0");
        }
    }

    /// <summary>
    /// Tests orbital velocity at Earth's distance.
    /// </summary>
    public static void TestCalculateOrbitalVelocityEarth()
    {
        double velocity = OrbitalMechanics.CalculateOrbitalVelocity(
            Units.AuMeters,
            Units.SolarMassKg
        );

        double velocityKmS = velocity / 1000.0;
        if (velocityKmS < 29.5 || velocityKmS > 30.0)
        {
            throw new InvalidOperationException("Earth orbital velocity should be ~29.78 km/s");
        }
    }

    /// <summary>
    /// Tests escape velocity from Earth.
    /// </summary>
    public static void TestCalculateEscapeVelocityEarth()
    {
        double vEsc = OrbitalMechanics.CalculateEscapeVelocity(
            Units.EarthMassKg,
            Units.EarthRadiusMeters
        );

        double vEscKmS = vEsc / 1000.0;
        if (vEscKmS < 11.0 || vEscKmS > 11.4)
        {
            throw new InvalidOperationException("Earth escape velocity should be ~11.2 km/s");
        }
    }

    /// <summary>
    /// Tests mean motion calculation.
    /// </summary>
    public static void TestCalculateMeanMotion()
    {
        double n = OrbitalMechanics.CalculateMeanMotion(
            Units.AuMeters,
            Units.SolarMassKg
        );

        if (System.Math.Abs(n - 1.99e-7) > 1e-8)
        {
            throw new InvalidOperationException("Earth mean motion");
        }
    }

    /// <summary>
    /// Tests edge cases return zero.
    /// </summary>
    public static void TestOrbitalPeriodEdgeCases()
    {
        if (OrbitalMechanics.CalculateOrbitalPeriod(0.0, Units.SolarMassKg) != 0.0)
        {
            throw new InvalidOperationException("Zero distance should return 0");
        }
        if (OrbitalMechanics.CalculateOrbitalPeriod(Units.AuMeters, 0.0) != 0.0)
        {
            throw new InvalidOperationException("Zero mass should return 0");
        }
        if (OrbitalMechanics.CalculateOrbitalPeriod(-1.0 * Units.AuMeters, Units.SolarMassKg) != 0.0)
        {
            throw new InvalidOperationException("Negative distance should return 0");
        }
    }

    /// <summary>
    /// Tests Hill sphere for Earth around Sun.
    /// </summary>
    public static void TestCalculateHillSphereEarth()
    {
        double hillRadius = OrbitalMechanics.CalculateHillSphere(
            Units.EarthMassKg,
            Units.SolarMassKg,
            Units.AuMeters
        );

        double hillKm = hillRadius / 1000.0;
        if (hillKm < 1.4e6 || hillKm > 1.6e6)
        {
            throw new InvalidOperationException("Earth Hill sphere should be ~1.5 million km");
        }
    }

    /// <summary>
    /// Tests habitable zone for Sun-like star.
    /// </summary>
    public static void TestCalculateHabitableZoneSolar()
    {
        const double SolarEffectiveTempK = 5780.0;
        double hzInner = OrbitalMechanics.CalculateHabitableZoneInner(
            StellarProps.SolarLuminosityWatts,
            SolarEffectiveTempK
        );
        double hzOuter = OrbitalMechanics.CalculateHabitableZoneOuter(
            StellarProps.SolarLuminosityWatts,
            SolarEffectiveTempK
        );

        double hzInnerAu = hzInner / Units.AuMeters;
        double hzOuterAu = hzOuter / Units.AuMeters;

        if (hzInnerAu < 0.9 || hzInnerAu > 1.0)
        {
            throw new InvalidOperationException("Solar HZ inner edge ~0.95 AU");
        }
        if (hzOuterAu < 1.3 || hzOuterAu > 1.5)
        {
            throw new InvalidOperationException("Solar HZ outer edge ~1.37 AU");
        }
    }

    /// <summary>
    /// Tests frost line for Sun.
    /// </summary>
    public static void TestCalculateFrostLineSolar()
    {
        double frost = OrbitalMechanics.CalculateFrostLine(StellarProps.SolarLuminosityWatts);
        double frostAu = frost / Units.AuMeters;

        if (frostAu < 2.5 || frostAu > 3.0)
        {
            throw new InvalidOperationException("Solar frost line ~2.7 AU");
        }
    }

    /// <summary>
    /// Tests orbital zone classification.
    /// </summary>
    public static void TestGetOrbitalZone()
    {
        double lum = StellarProps.SolarLuminosityWatts;

        OrbitZone.Zone zoneHot = OrbitZone.FromOrbitalDistance(
            0.5 * Units.AuMeters, lum
        );
        if (zoneHot != OrbitZone.Zone.Hot)
        {
            throw new InvalidOperationException("0.5 AU should be HOT");
        }

        OrbitZone.Zone zoneTemp = OrbitZone.FromOrbitalDistance(
            1.0 * Units.AuMeters, lum
        );
        if (zoneTemp != OrbitZone.Zone.Temperate)
        {
            throw new InvalidOperationException("1.0 AU should be TEMPERATE");
        }

        OrbitZone.Zone zoneCold = OrbitZone.FromOrbitalDistance(
            5.0 * Units.AuMeters, lum
        );
        if (zoneCold != OrbitZone.Zone.Cold)
        {
            throw new InvalidOperationException("5.0 AU should be COLD");
        }
    }

    /// <summary>
    /// Tests orbital-zone classification rejects invalid boundaries.
    /// </summary>
    public static void TestGetOrbitalZoneInvalidInputs()
    {
        string invalidDistance = OrbitalMechanics.GetOrbitalZone(
            0.0,
            0.95 * Units.AuMeters,
            1.37 * Units.AuMeters,
            2.7 * Units.AuMeters);
        if (invalidDistance != "Unknown")
        {
            throw new InvalidOperationException("Zero orbital distance should classify as Unknown");
        }

        string invalidInnerBoundary = OrbitalMechanics.GetOrbitalZone(
            Units.AuMeters,
            0.0,
            1.37 * Units.AuMeters,
            2.7 * Units.AuMeters);
        if (invalidInnerBoundary != "Unknown")
        {
            throw new InvalidOperationException("Missing habitable-zone inner edge should classify as Unknown");
        }

        string invalidFrostLine = OrbitalMechanics.GetOrbitalZone(
            Units.AuMeters,
            0.95 * Units.AuMeters,
            1.37 * Units.AuMeters,
            0.0);
        if (invalidFrostLine != "Unknown")
        {
            throw new InvalidOperationException("Missing frost line should classify as Unknown");
        }
    }

    // ========== Additional tests (converted from GDScript) ==========

    /// <summary>Tests Hill sphere for Jupiter around Sun.</summary>
    public static void TestCalculateHillSphereJupiter()
    {
        double jupiterMassKg = 1.898e27;
        double jupiterDistanceM = 5.2 * Units.AuMeters;
        double hillRadius = OrbitalMechanics.CalculateHillSphere(
            jupiterMassKg,
            Units.SolarMassKg,
            jupiterDistanceM
        );
        double hillAu = hillRadius / Units.AuMeters;
        if (hillAu < 0.33 || hillAu > 0.37)
        {
            throw new InvalidOperationException("Jupiter Hill sphere should be ~0.35 AU");
        }
    }

    /// <summary>Tests Roche limit for Earth-density satellite around Earth.</summary>
    public static void TestCalculateRocheLimitEarth()
    {
        double earthDensity = 5515.0;
        double satelliteDensity = 3000.0;
        double roche = OrbitalMechanics.CalculateRocheLimit(
            Units.EarthRadiusMeters,
            earthDensity,
            satelliteDensity
        );
        double rocheKm = roche / 1000.0;
        if (rocheKm < 17000.0 || rocheKm > 21000.0)
        {
            throw new InvalidOperationException("Roche limit for rocky satellite ~18-20k km");
        }
    }

    /// <summary>Tests Roche limit using mass-based function.</summary>
    public static void TestCalculateRocheLimitFromMass()
    {
        double roche = OrbitalMechanics.CalculateRocheLimitFromMass(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            3000.0
        );
        double rocheKm = roche / 1000.0;
        if (rocheKm < 17000.0 || rocheKm > 21000.0)
        {
            throw new InvalidOperationException("Roche limit from mass should match");
        }
    }

    /// <summary>Tests sphere of influence.</summary>
    public static void TestCalculateSphereOfInfluence()
    {
        double soi = OrbitalMechanics.CalculateSphereOfInfluence(
            Units.EarthMassKg,
            Units.SolarMassKg,
            Units.AuMeters
        );
        double soiKm = soi / 1000.0;
        if (soiKm < 900000.0 || soiKm > 1000000.0)
        {
            throw new InvalidOperationException("Earth SOI ~925,000 km");
        }
    }

    /// <summary>Tests barycenter for Earth-Moon system.</summary>
    public static void TestCalculateBarycenterEarthMoon()
    {
        double moonMassKg = 7.342e22;
        double earthMoonDistanceM = 3.844e8;
        double barycenterDist = OrbitalMechanics.CalculateBarycenterFromA(
            Units.EarthMassKg,
            moonMassKg,
            earthMoonDistanceM
        );
        double distKm = barycenterDist / 1000.0;
        if (distKm < 4500.0 || distKm > 4800.0)
        {
            throw new InvalidOperationException("Earth-Moon barycenter should be ~4,670 km from Earth");
        }
    }

    /// <summary>Tests barycenter for equal masses.</summary>
    public static void TestCalculateBarycenterEqualMasses()
    {
        double mass = 1e30;
        double separation = 1e11;
        double barycenterDist = OrbitalMechanics.CalculateBarycenterFromA(mass, mass, separation);
        if (System.Math.Abs(barycenterDist - separation / 2.0) > separation * 0.001)
        {
            throw new InvalidOperationException("Equal masses barycenter at midpoint");
        }
    }

    /// <summary>Tests barycenter for Sun-Jupiter system.</summary>
    public static void TestCalculateBarycenterSunJupiter()
    {
        double jupiterMass = 1.898e27;
        double jupiterDistance = 5.2 * Units.AuMeters;
        double distanceFromSun = OrbitalMechanics.CalculateBarycenterFromA(
            Units.SolarMassKg,
            jupiterMass,
            jupiterDistance
        );
        double distanceSolarRadii = distanceFromSun / Units.SolarRadiusMeters;
        if (distanceSolarRadii < 1.0 || distanceSolarRadii > 1.2)
        {
            throw new InvalidOperationException("Sun-Jupiter barycenter ~1.07 solar radii");
        }
    }

    /// <summary>Tests S-type stability limit for circular binary.</summary>
    public static void TestCalculateStypeStabilityLimitCircular()
    {
        double binarySep = 10.0 * Units.AuMeters;
        double massRatio = 1.0;
        double ecc = 0.0;
        double limit = OrbitalMechanics.CalculateStypeStabilityLimit(binarySep, massRatio, ecc);
        double limitAu = limit / Units.AuMeters;
        if (limitAu < 2.0 || limitAu > 3.0)
        {
            throw new InvalidOperationException("S-type limit should be ~24-27% of separation for equal masses");
        }
    }

    /// <summary>Tests S-type stability limit for eccentric binary.</summary>
    public static void TestCalculateStypeStabilityLimitEccentric()
    {
        double binarySep = 10.0 * Units.AuMeters;
        double massRatio = 0.5;
        double ecc = 0.5;
        double limit = OrbitalMechanics.CalculateStypeStabilityLimit(binarySep, massRatio, ecc);
        double limitAu = limit / Units.AuMeters;
        if (limitAu >= 3.0)
        {
            throw new InvalidOperationException("Eccentric binary should reduce S-type limit");
        }
    }

    /// <summary>Tests S-type stability for Alpha Centauri-like binary.</summary>
    public static void TestCalculateStypeStabilityAlphaCen()
    {
        double separationM = 24.0 * Units.AuMeters;
        double eccentricity = 0.52;
        double massRatio = 0.9 / 1.1;
        double limit = OrbitalMechanics.CalculateStypeStabilityLimit(separationM, massRatio, eccentricity);
        double limitAu = limit / Units.AuMeters;
        if (limitAu < 2.0 || limitAu > 5.0)
        {
            throw new InvalidOperationException("S-type limit for Alpha Cen A should be ~3-4 AU");
        }
    }

    /// <summary>Tests P-type stability limit for circular binary.</summary>
    public static void TestCalculatePtypeStabilityLimitCircular()
    {
        double binarySep = 1.0 * Units.AuMeters;
        double massRatio = 1.0;
        double ecc = 0.0;
        double limit = OrbitalMechanics.CalculatePtypeStabilityLimit(binarySep, massRatio, ecc);
        double limitAu = limit / Units.AuMeters;
        if (limitAu < 2.0 || limitAu > 4.0)
        {
            throw new InvalidOperationException("P-type limit should be ~2-3x separation");
        }
    }

    /// <summary>Tests P-type stability limit increases with eccentricity.</summary>
    public static void TestCalculatePtypeStabilityLimitEccentric()
    {
        double binarySep = 1.0 * Units.AuMeters;
        double massRatio = 0.5;
        double limitCirc = OrbitalMechanics.CalculatePtypeStabilityLimit(binarySep, massRatio, 0.0);
        double limitEcc = OrbitalMechanics.CalculatePtypeStabilityLimit(binarySep, massRatio, 0.5);
        if (limitEcc <= limitCirc)
        {
            throw new InvalidOperationException("Eccentric binary should increase P-type limit");
        }
    }

    /// <summary>Tests P-type stability for close binary.</summary>
    public static void TestCalculatePtypeStabilityCloseBinary()
    {
        double separationM = 0.2 * Units.AuMeters;
        double eccentricity = 0.1;
        double massRatio = 0.5;
        double limit = OrbitalMechanics.CalculatePtypeStabilityLimit(separationM, massRatio, eccentricity);
        double limitAu = limit / Units.AuMeters;
        if (limitAu < 0.4 || limitAu > 1.0)
        {
            throw new InvalidOperationException("P-type limit for close binary ~0.5-0.8 AU");
        }
    }

    /// <summary>Jacobi radius scales as M_star^(1/3).</summary>
    public static void TestCalculateJacobiRadiusScaling()
    {
        double r1sunM = OrbitalMechanics.CalculateJacobiRadiusM(Units.SolarMassKg);
        double r1sunAu = r1sunM / Units.AuMeters;
        if (r1sunAu < 2.5e5 || r1sunAu > 3.0e5)
        {
            throw new InvalidOperationException("Jacobi at 1 M_sun ~ 2.78e5 AU");
        }
        double m01 = 0.1 * Units.SolarMassKg;
        double r01M = OrbitalMechanics.CalculateJacobiRadiusM(m01);
        double r01Au = r01M / Units.AuMeters;
        if (r01Au < 1.1e5 || r01Au > 1.4e5)
        {
            throw new InvalidOperationException("Jacobi at 0.1 M_sun ~ 1.29e5 AU");
        }
        double m2 = 2.0 * Units.SolarMassKg;
        double r2M = OrbitalMechanics.CalculateJacobiRadiusM(m2);
        if (r2M <= r1sunM)
        {
            throw new InvalidOperationException("Jacobi increases with stellar mass");
        }
    }

    /// <summary>Formation outer limit scales as M_star^0.6.</summary>
    public static void TestCalculateFormationOuterLimit()
    {
        double r1sunM = OrbitalMechanics.CalculateFormationOuterLimitM(Units.SolarMassKg, 100.0);
        double r1sunAu = r1sunM / Units.AuMeters;
        if (System.Math.Abs(r1sunAu - 100.0) > 1.0)
        {
            throw new InvalidOperationException("Formation at 1 M_sun with base 100 AU");
        }
        double m01 = 0.1 * Units.SolarMassKg;
        double r01M = OrbitalMechanics.CalculateFormationOuterLimitM(m01, 100.0);
        double r01Au = r01M / Units.AuMeters;
        if (r01Au < 18.0 || r01Au > 28.0)
        {
            throw new InvalidOperationException("Formation at 0.1 M_sun ~ 25 AU");
        }
    }

    /// <summary>Combined outer limit is min(formation, Jacobi).</summary>
    public static void TestCalculateOuterStabilityLimitM()
    {
        double limit1sunM = OrbitalMechanics.CalculateOuterStabilityLimitM(Units.SolarMassKg, 100.0);
        double limit1sunAu = limit1sunM / Units.AuMeters;
        if (System.Math.Abs(limit1sunAu - 100.0) > 1.0)
        {
            throw new InvalidOperationException("At 1 M_sun formation is smaller than Jacobi");
        }
        double formationM = OrbitalMechanics.CalculateFormationOuterLimitM(Units.SolarMassKg, 100.0);
        double jacobiM = OrbitalMechanics.CalculateJacobiRadiusM(Units.SolarMassKg);
        if (limit1sunM >= jacobiM)
        {
            throw new InvalidOperationException("Outer limit should be formation-limited for 1 M_sun");
        }
        if (System.Math.Abs(limit1sunM - formationM) > 1.0)
        {
            throw new InvalidOperationException("Limit should equal formation when formation < Jacobi");
        }
    }

    /// <summary>Tests binary period calculation.</summary>
    public static void TestCalculateBinaryPeriod()
    {
        double periodS = OrbitalMechanics.CalculateBinaryPeriod(
            Units.AuMeters,
            Units.SolarMassKg,
            Units.SolarMassKg
        );
        double periodYears = periodS / (365.25 * 24.0 * 3600.0);
        if (periodYears < 0.68 || periodYears > 0.73)
        {
            throw new InvalidOperationException("Binary period for 2 solar masses at 1 AU");
        }
    }

    /// <summary>Tests habitable zone scales with luminosity.</summary>
    public static void TestCalculateHabitableZoneScaling()
    {
        double luminosity = 4.0 * StellarProps.SolarLuminosityWatts;
        const double SolarEffectiveTempK = 5780.0;
        double hzInner = OrbitalMechanics.CalculateHabitableZoneInner(luminosity, SolarEffectiveTempK);
        double hzInnerAu = hzInner / Units.AuMeters;
        if (hzInnerAu < 1.8 || hzInnerAu > 2.0)
        {
            throw new InvalidOperationException("HZ inner for 4x solar should be ~1.9 AU");
        }
    }

    /// <summary>Tests resonance spacing 2:1 ratio.</summary>
    public static void TestCalculateResonanceSpacing2_1()
    {
        double innerOrbit = 1.0 * Units.AuMeters;
        SeededRng rng = new SeededRng(12345);
        double outerOrbit = OrbitalMechanics.CalculateResonanceSpacing(innerOrbit, 2.0, 0.0, rng);
        double outerAu = outerOrbit / Units.AuMeters;
        if (System.Math.Abs(outerAu - 1.587) > 0.01)
        {
            throw new InvalidOperationException("2:1 resonance spacing");
        }
    }

    /// <summary>Tests resonance spacing 3:2 ratio.</summary>
    public static void TestCalculateResonanceSpacing3_2()
    {
        double innerOrbit = 1.0 * Units.AuMeters;
        SeededRng rng = new SeededRng(12345);
        double outerOrbit = OrbitalMechanics.CalculateResonanceSpacing(innerOrbit, 1.5, 0.0, rng);
        double outerAu = outerOrbit / Units.AuMeters;
        if (System.Math.Abs(outerAu - 1.310) > 0.01)
        {
            throw new InvalidOperationException("3:2 resonance spacing");
        }
    }

    /// <summary>Tests resonance spacing with variation.</summary>
    public static void TestCalculateResonanceSpacingWithVariation()
    {
        double innerOrbit = 1.0 * Units.AuMeters;
        SeededRng rng = new SeededRng(12345);
        double outerOrbit = OrbitalMechanics.CalculateResonanceSpacing(innerOrbit, 2.0, 0.2, rng);
        double outerAu = outerOrbit / Units.AuMeters;
        if (outerAu < 1.27 || outerAu > 1.90)
        {
            throw new InvalidOperationException("2:1 resonance with 20% variation");
        }
    }

    /// <summary>Tests get_common_resonance_ratios.</summary>
    public static void TestGetCommonResonanceRatios()
    {
        Array<double> ratios = OrbitalMechanics.GetCommonResonanceRatios();
        if (ratios.Count <= 5)
        {
            throw new InvalidOperationException("Should have multiple resonance ratios");
        }
        bool has2 = false;
        bool has15 = false;
        foreach (double r in ratios)
        {
            if (System.Math.Abs(r - 2.0) < 0.001) has2 = true;
            if (System.Math.Abs(r - 1.5) < 0.001) has15 = true;
        }
        if (!has2 || !has15)
        {
            throw new InvalidOperationException("Should include 2:1 and 3:2");
        }
    }

    /// <summary>Tests period to distance ratio conversion.</summary>
    public static void TestPeriodRatioToDistanceRatio()
    {
        double distRatio = OrbitalMechanics.PeriodRatioToDistanceRatio(2.0);
        if (distRatio < 1.58 || distRatio > 1.60)
        {
            throw new InvalidOperationException("2:1 period ratio -> ~1.587 distance ratio");
        }
    }

    /// <summary>Tests distance to period ratio conversion.</summary>
    public static void TestDistanceRatioToPeriodRatio()
    {
        double periodRatio = OrbitalMechanics.DistanceRatioToPeriodRatio(2.0);
        if (periodRatio < 2.8 || periodRatio > 2.85)
        {
            throw new InvalidOperationException("2x distance -> ~2.83 period ratio");
        }
    }

    /// <summary>Tests minimum planet spacing.</summary>
    public static void TestCalculateMinimumPlanetSpacing()
    {
        double spacing = OrbitalMechanics.CalculateMinimumPlanetSpacing(
            Units.EarthMassKg,
            Units.EarthMassKg,
            Units.SolarMassKg,
            1.0 * Units.AuMeters
        );
        double spacingAu = spacing / Units.AuMeters;
        if (spacingAu <= 0.05)
        {
            throw new InvalidOperationException("Minimum spacing should be significant");
        }
        if (spacingAu >= 0.2)
        {
            throw new InvalidOperationException("Minimum spacing should be reasonable");
        }
    }

    /// <summary>Tests is_orbit_stable with no companions.</summary>
    public static void TestIsOrbitStableNoCompanions()
    {
        Array<double> emptyM = new Array<double>();
        Array<double> emptyP = new Array<double>();
        bool stable = OrbitalMechanics.IsOrbitStable(
            1.0 * Units.AuMeters,
            Units.SolarMassKg,
            0.0,
            emptyM,
            emptyP
        );
        if (!stable)
        {
            throw new InvalidOperationException("Orbit with no companions should be stable");
        }
    }

    /// <summary>Tests is_orbit_stable too close to companion.</summary>
    public static void TestIsOrbitStableTooCloseToCompanion()
    {
        Array<double> masses = new Array<double> { Units.SolarMassKg };
        Array<double> positions = new Array<double> { 10.0 * Units.AuMeters };
        bool stable = OrbitalMechanics.IsOrbitStable(
            8.0 * Units.AuMeters,
            Units.SolarMassKg,
            0.0,
            masses,
            positions
        );
        if (stable)
        {
            throw new InvalidOperationException("Orbit too close to companion should be unstable");
        }
    }

    /// <summary>Tests is_orbit_stable far from companion.</summary>
    public static void TestIsOrbitStableFarFromCompanion()
    {
        Array<double> masses = new Array<double> { Units.SolarMassKg * 0.5 };
        Array<double> positions = new Array<double> { 50.0 * Units.AuMeters };
        bool stable = OrbitalMechanics.IsOrbitStable(
            1.0 * Units.AuMeters,
            Units.SolarMassKg,
            0.0,
            masses,
            positions
        );
        if (!stable)
        {
            throw new InvalidOperationException("Orbit far from companion should be stable");
        }
    }

    /// <summary>Tests perturbation strength calculation.</summary>
    public static void TestCalculatePerturbationStrength()
    {
        double jupiterMassKg = 1.898e27;
        double jupiterDistanceM = 5.2 * Units.AuMeters;
        double marsOrbit = 1.5 * Units.AuMeters;
        double strengthMars = OrbitalMechanics.CalculatePerturbationStrength(
            marsOrbit,
            jupiterDistanceM,
            jupiterMassKg,
            Units.SolarMassKg
        );
        double beltOrbit = 3.0 * Units.AuMeters;
        double strengthBelt = OrbitalMechanics.CalculatePerturbationStrength(
            beltOrbit,
            jupiterDistanceM,
            jupiterMassKg,
            Units.SolarMassKg
        );
        if (strengthMars >= strengthBelt)
        {
            throw new InvalidOperationException("Mars perturbation < asteroid belt perturbation");
        }
        double closeOrbit = 4.0 * Units.AuMeters;
        double strengthClose = OrbitalMechanics.CalculatePerturbationStrength(
            closeOrbit,
            jupiterDistanceM,
            jupiterMassKg,
            Units.SolarMassKg
        );
        if (strengthClose <= strengthMars)
        {
            throw new InvalidOperationException("Closer orbit should have higher perturbation");
        }
    }

    /// <summary>Tests perturbation strength for orbit inside companion.</summary>
    public static void TestCalculatePerturbationStrengthInside()
    {
        double strength = OrbitalMechanics.CalculatePerturbationStrength(
            1.0 * Units.AuMeters,
            5.0 * Units.AuMeters,
            Units.SolarMassKg,
            Units.SolarMassKg
        );
        if (strength <= 0.0)
        {
            throw new InvalidOperationException("Perturbation strength should be positive");
        }
        if (strength >= 0.1)
        {
            throw new InvalidOperationException("Perturbation for distant companion should be small");
        }
    }

    /// <summary>Tests do_orbits_overlap for non-overlapping circular orbits.</summary>
    public static void TestDoOrbitsOverlapNoOverlap()
    {
        bool overlap = OrbitalMechanics.DoOrbitsOverlap(
            1.0 * Units.AuMeters,
            0.0,
            2.0 * Units.AuMeters,
            0.0
        );
        if (overlap)
        {
            throw new InvalidOperationException("Non-overlapping circular orbits");
        }
    }

    /// <summary>Tests do_orbits_overlap for overlapping eccentric orbits.</summary>
    public static void TestDoOrbitsOverlapEccentric()
    {
        bool overlap = OrbitalMechanics.DoOrbitsOverlap(
            1.0 * Units.AuMeters,
            0.5,
            1.2 * Units.AuMeters,
            0.3
        );
        if (!overlap)
        {
            throw new InvalidOperationException("Eccentric orbits with overlapping ranges");
        }
    }

    /// <summary>Tests do_orbits_overlap for just-touching orbits.</summary>
    public static void TestDoOrbitsOverlapTouching()
    {
        bool overlap = OrbitalMechanics.DoOrbitsOverlap(
            1.0 * Units.AuMeters,
            0.5,
            1.5 * Units.AuMeters,
            0.0
        );
        if (!overlap)
        {
            throw new InvalidOperationException("Orbits that touch should overlap");
        }
    }

    /// <summary>Tests synodic period calculation.</summary>
    public static void TestCalculateSynodicPeriod()
    {
        double earthPeriod = 365.25 * 24.0 * 3600.0;
        double marsPeriod = 687.0 * 24.0 * 3600.0;
        double synodic = OrbitalMechanics.CalculateSynodicPeriod(earthPeriod, marsPeriod);
        double synodicDays = synodic / (24.0 * 3600.0);
        if (System.Math.Abs(synodicDays - 780.0) > 10.0)
        {
            throw new InvalidOperationException("Earth-Mars synodic period ~780 days");
        }
    }

    /// <summary>Tests synodic period for nearly identical periods.</summary>
    public static void TestCalculateSynodicPeriodNearIdentical()
    {
        double period1 = 365.25 * 24.0 * 3600.0;
        double period2 = period1 * 1.0000000001;
        double synodic = OrbitalMechanics.CalculateSynodicPeriod(period1, period2);
        if (synodic <= 1.0e15)
        {
            throw new InvalidOperationException("Near-identical periods should give very large synodic period");
        }
    }

    /// <summary>Tests edge case: zero inputs.</summary>
    public static void TestEdgeCaseZeroInputs()
    {
        if (OrbitalMechanics.CalculateOrbitalPeriod(0.0, Units.SolarMassKg) != 0.0) throw new InvalidOperationException("period 0");
        if (OrbitalMechanics.CalculateOrbitalPeriod(Units.AuMeters, 0.0) != 0.0) throw new InvalidOperationException("period 0 mass");
        if (OrbitalMechanics.CalculateHillSphere(0.0, Units.SolarMassKg, Units.AuMeters) != 0.0) throw new InvalidOperationException("hill 0");
        if (OrbitalMechanics.CalculateBarycenterFromA(0.0, Units.SolarMassKg, 1e11) != 0.0) throw new InvalidOperationException("bary 0");
        if (OrbitalMechanics.CalculateRocheLimit(0.0, 5000.0, 3000.0) != 0.0) throw new InvalidOperationException("roche 0");
        if (OrbitalMechanics.CalculateHabitableZoneInner(0.0, 5780.0) != 0.0) throw new InvalidOperationException("hz inner 0");
        if (OrbitalMechanics.CalculateFrostLine(0.0) != 0.0) throw new InvalidOperationException("frost 0");
    }

    /// <summary>Tests edge case: negative inputs.</summary>
    public static void TestEdgeCaseNegativeInputs()
    {
        if (OrbitalMechanics.CalculateOrbitalPeriod(-1.0 * Units.AuMeters, Units.SolarMassKg) != 0.0)
        {
            throw new InvalidOperationException("Negative distance should return 0");
        }
        if (OrbitalMechanics.CalculateHillSphere(-1.0, Units.SolarMassKg, Units.AuMeters) != 0.0)
        {
            throw new InvalidOperationException("Negative mass should return 0");
        }
    }

    /// <summary>Tests edge case: invalid resonance spacing.</summary>
    public static void TestResonanceSpacingEdgeCases()
    {
        SeededRng rng = new SeededRng(12345);
        double result = OrbitalMechanics.CalculateResonanceSpacing(Units.AuMeters, 0.5, 0.0, rng);
        if (result != Units.AuMeters)
        {
            throw new InvalidOperationException("Invalid ratio should return inner orbit");
        }
        double result2 = OrbitalMechanics.CalculateResonanceSpacing(0.0, 2.0, 0.0, rng);
        if (result2 != 0.0)
        {
            throw new InvalidOperationException("Zero inner orbit should return zero");
        }
    }
}
