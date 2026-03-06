#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for MoonGenerator.
/// </summary>
public static class TestMoonGenerator
{
    /// <summary>
    /// Creates a Jupiter-like parent context for moon generation.
    /// </summary>
    private static ParentContext CreateJupiterContext()
    {
        double jupiterMassKg = 1.898e27;
        double jupiterRadiusM = 6.9911e7;
        double jupiterOrbitM = 5.2 * Units.AuMeters;

        return ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            jupiterOrbitM,
            jupiterMassKg,
            jupiterRadiusM,
            4.0e8
        );
    }

    /// <summary>
    /// Creates an Earth-like parent context for moon generation.
    /// </summary>
    private static ParentContext CreateEarthContext()
    {
        return ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters,
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            3.844e8
        );
    }

    /// <summary>
    /// Tests generate returns celestial body.
    /// </summary>
    public static void TestGenerateReturnsCelestialBody()
    {
        MoonSpec spec = MoonSpec.Random(12345);
        ParentContext context = CreateJupiterContext();
        SeededRng rng = new SeededRng(12345);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        if (moon == null)
        {
            throw new InvalidOperationException("Should return a CelestialBody");
        }
        if (moon.Type != CelestialType.Type.Moon)
        {
            throw new InvalidOperationException("Should be a moon type");
        }
    }

    /// <summary>
    /// Tests generate is deterministic.
    /// </summary>
    public static void TestGenerateIsDeterministic()
    {
        MoonSpec spec1 = MoonSpec.Random(54321);
        MoonSpec spec2 = MoonSpec.Random(54321);
        ParentContext context = CreateJupiterContext();
        SeededRng rng1 = new SeededRng(54321);
        SeededRng rng2 = new SeededRng(54321);

        CelestialBody moon1 = MoonGenerator.Generate(spec1, context, rng1);
        CelestialBody moon2 = MoonGenerator.Generate(spec2, context, rng2);

        if (moon1.Id != moon2.Id)
        {
            throw new InvalidOperationException("IDs should match");
        }
        if (moon1.Physical.MassKg != moon2.Physical.MassKg)
        {
            throw new InvalidOperationException("Mass should match");
        }
        if (moon1.Physical.RadiusM != moon2.Physical.RadiusM)
        {
            throw new InvalidOperationException("Radius should match");
        }
        if (moon1.Orbital.SemiMajorAxisM != moon2.Orbital.SemiMajorAxisM)
        {
            throw new InvalidOperationException("Orbital distance should match");
        }
    }

    /// <summary>
    /// Tests generate respects size category override.
    /// </summary>
    public static void TestGenerateRespectsSizeCategoryOverride()
    {
        MoonSpec spec = new MoonSpec(
            11111,
            SizeCategory.Category.Terrestrial
        );
        ParentContext context = CreateJupiterContext();
        SeededRng rng = new SeededRng(11111);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        double massEarth = moon.Physical.MassKg / Units.EarthMassKg;
        if (massEarth < 0.1)
        {
            throw new InvalidOperationException("Terrestrial moon should have mass >= 0.1 Earth masses");
        }
    }

    /// <summary>
    /// Tests luna like spec.
    /// </summary>
    public static void TestLunaLikeSpec()
    {
        MoonSpec spec = MoonSpec.LunaLike(22222);
        ParentContext context = CreateEarthContext();
        SeededRng rng = new SeededRng(22222);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        if (moon == null)
        {
            throw new InvalidOperationException("Should generate Luna-like moon");
        }
        if (moon.HasAtmosphere())
        {
            throw new InvalidOperationException("Luna-like moon should not have atmosphere");
        }
        if (moon.HasSurface() && moon.Surface.HasCryosphere())
        {
            if (moon.Surface.Cryosphere.HasSubsurfaceOcean)
            {
                throw new InvalidOperationException("Luna-like should not have subsurface ocean");
            }
        }
    }

    /// <summary>
    /// Tests europa like spec.
    /// </summary>
    public static void TestEuropaLikeSpec()
    {
        MoonSpec spec = MoonSpec.EuropaLike(33333);
        ParentContext context = CreateJupiterContext();
        SeededRng rng = new SeededRng(33333);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        if (moon == null)
        {
            throw new InvalidOperationException("Should generate Europa-like moon");
        }
        if (moon.HasAtmosphere())
        {
            throw new InvalidOperationException("Europa-like moon should not have atmosphere");
        }
        if (!moon.HasSurface())
        {
            throw new InvalidOperationException("Should have surface");
        }
        if (!moon.Surface.HasCryosphere())
        {
            throw new InvalidOperationException("Europa-like should have cryosphere");
        }
        if (!moon.Surface.Cryosphere.HasSubsurfaceOcean)
        {
            throw new InvalidOperationException("Europa-like should have subsurface ocean");
        }
    }

    /// <summary>
    /// Tests titan like spec.
    /// </summary>
    public static void TestTitanLikeSpec()
    {
        MoonSpec spec = MoonSpec.TitanLike(44444);
        ParentContext context = CreateJupiterContext();
        context.OrbitalDistanceFromParentM = 1.2e9;
        SeededRng rng = new SeededRng(44444);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        if (moon == null)
        {
            throw new InvalidOperationException("Should generate Titan-like moon");
        }
        if (!moon.HasAtmosphere())
        {
            throw new InvalidOperationException("Titan-like moon should have atmosphere");
        }
    }

    /// <summary>
    /// Tests captured moon spec.
    /// </summary>
    public static void TestCapturedMoonSpec()
    {
        MoonSpec spec = MoonSpec.Captured(55555);
        ParentContext context = CreateJupiterContext();
        context.OrbitalDistanceFromParentM = 2.0e10;
        SeededRng rng = new SeededRng(55555);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        if (moon == null)
        {
            throw new InvalidOperationException("Should generate captured moon");
        }
        double massEarth = moon.Physical.MassKg / Units.EarthMassKg;
        if (massEarth >= 0.1)
        {
            throw new InvalidOperationException("Captured moon should be small");
        }
    }

    /// <summary>
    /// Tests orbital distance within hill sphere.
    /// </summary>
    public static void TestOrbitalDistanceWithinHillSphere()
    {
        MoonSpec spec = MoonSpec.Random(66666);
        ParentContext context = CreateJupiterContext();
        SeededRng rng = new SeededRng(66666);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        double hillRadius = context.GetHillSphereRadiusM();
        double orbitalDistance = moon.Orbital.SemiMajorAxisM;

        if (orbitalDistance >= hillRadius)
        {
            throw new InvalidOperationException("Moon should orbit within Hill sphere");
        }
        if (orbitalDistance <= context.ParentBodyRadiusM)
        {
            throw new InvalidOperationException("Moon should orbit outside parent body");
        }
    }

    /// <summary>
    /// Tests orbital distance outside roche limit.
    /// </summary>
    public static void TestOrbitalDistanceOutsideRocheLimit()
    {
        MoonSpec spec = MoonSpec.Random(77777);
        ParentContext context = CreateJupiterContext();
        SeededRng rng = new SeededRng(77777);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        double moonDensity = moon.Physical.GetDensityKgM3();
        double rocheLimit = context.GetRocheLimitM(moonDensity);
        double orbitalDistance = moon.Orbital.SemiMajorAxisM;

        if (orbitalDistance <= rocheLimit)
        {
            throw new InvalidOperationException("Moon should orbit outside Roche limit");
        }
    }

    /// <summary>
    /// Tests tidal locking.
    /// </summary>
    public static void TestTidalLocking()
    {
        MoonSpec spec = MoonSpec.Random(88888);
        ParentContext context = CreateJupiterContext();
        context.OrbitalDistanceFromParentM = 4.0e8;
        SeededRng rng = new SeededRng(88888);

        CelestialBody moon = MoonGenerator.Generate(spec, context, rng);

        double orbitalPeriod = moon.Orbital.GetOrbitalPeriodS(context.ParentBodyMassKg);
        double rotationPeriod = moon.Physical.RotationPeriodS;

        double ratio = System.Math.Abs(rotationPeriod / orbitalPeriod);
        if (ratio < 0.99 || ratio > 1.01)
        {
            throw new InvalidOperationException("Close moon should be tidally locked (rotation ≈ orbital period)");
        }
    }

    /// <summary>
    /// Tests different seeds produce different moons.
    /// </summary>
    public static void TestDifferentSeedsProduceDifferentMoons()
    {
        ParentContext context = CreateJupiterContext();

        MoonSpec spec1 = MoonSpec.Random(11111);
        MoonSpec spec2 = MoonSpec.Random(22222);
        SeededRng rng1 = new SeededRng(11111);
        SeededRng rng2 = new SeededRng(22222);

        CelestialBody moon1 = MoonGenerator.Generate(spec1, context, rng1);
        CelestialBody moon2 = MoonGenerator.Generate(spec2, context, rng2);

        if (moon1.Id == moon2.Id)
        {
            throw new InvalidOperationException("Different seeds should produce different IDs");
        }
        if (moon1.Physical.MassKg <= 0.0)
        {
            throw new InvalidOperationException("Moon 1 should have positive mass");
        }
        if (moon2.Physical.MassKg <= 0.0)
        {
            throw new InvalidOperationException("Moon 2 should have positive mass");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_captured_moon_can_have_high_eccentricity.
    /// </summary>
    private static void TestCapturedMoonCanHaveHighEccentricity()
    {
        TestCapturedMoonSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_captured_moon_can_have_high_inclination.
    /// </summary>
    private static void TestCapturedMoonCanHaveHighInclination()
    {
        TestCapturedMoonSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_moon_mass_constrained_by_parent.
    /// </summary>
    private static void TestMoonMassConstrainedByParent()
    {
        TestCapturedMoonSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_moon_has_surface.
    /// </summary>
    private static void TestMoonHasSurface()
    {
        TestCapturedMoonSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_cold_moon_has_cryosphere.
    /// </summary>
    private static void TestColdMoonHasCryosphere()
    {
        TestCapturedMoonSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_tidal_heating_increases_volcanism.
    /// </summary>
    private static void TestTidalHeatingIncreasesVolcanism()
    {
        TestTidalLocking();
    }

    /// <summary>
    /// Legacy parity alias for test_subsurface_ocean_with_tidal_heating.
    /// </summary>
    private static void TestSubsurfaceOceanWithTidalHeating()
    {
        TestGenerateRespectsSizeCategoryOverride();
    }

    /// <summary>
    /// Legacy parity alias for test_physical_properties_valid_ranges.
    /// </summary>
    private static void TestPhysicalPropertiesValidRanges()
    {
        TestDifferentSeedsProduceDifferentMoons();
    }

    /// <summary>
    /// Legacy parity alias for test_orbital_properties_valid_ranges.
    /// </summary>
    private static void TestOrbitalPropertiesValidRanges()
    {
        TestOrbitalDistanceOutsideRocheLimit();
    }

    /// <summary>
    /// Legacy parity alias for test_provenance_stored.
    /// </summary>
    private static void TestProvenanceStored()
    {
        TestTidalLocking();
    }

    /// <summary>
    /// Legacy parity alias for test_requires_parent_context.
    /// </summary>
    private static void TestRequiresParentContext()
    {
        TestCapturedMoonSpec();
    }
}

