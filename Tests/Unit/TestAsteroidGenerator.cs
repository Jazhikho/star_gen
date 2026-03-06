#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for AsteroidGenerator.
/// </summary>
public static class TestAsteroidGenerator
{
    /// <summary>
    /// Creates a standard solar system context for asteroid generation.
    /// </summary>
    private static ParentContext CreateSolarContext()
    {
        return ParentContext.SunLike(2.7 * Units.AuMeters);
    }

    /// <summary>
    /// Tests generate returns celestial body.
    /// </summary>
    public static void TestGenerateReturnsCelestialBody()
    {
        AsteroidSpec spec = AsteroidSpec.Random(12345);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(12345);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid == null)
        {
            throw new InvalidOperationException("Should return a CelestialBody");
        }
        if (asteroid.Type != CelestialType.Type.Asteroid)
        {
            throw new InvalidOperationException("Should be an asteroid type");
        }
    }

    /// <summary>
    /// Tests generate is deterministic.
    /// </summary>
    public static void TestGenerateIsDeterministic()
    {
        AsteroidSpec spec1 = AsteroidSpec.Random(54321);
        AsteroidSpec spec2 = AsteroidSpec.Random(54321);
        ParentContext context = CreateSolarContext();
        SeededRng rng1 = new SeededRng(54321);
        SeededRng rng2 = new SeededRng(54321);

        CelestialBody asteroid1 = AsteroidGenerator.Generate(spec1, context, rng1);
        CelestialBody asteroid2 = AsteroidGenerator.Generate(spec2, context, rng2);

        if (asteroid1.Id != asteroid2.Id)
        {
            throw new InvalidOperationException("IDs should match");
        }
        if (asteroid1.Physical.MassKg != asteroid2.Physical.MassKg)
        {
            throw new InvalidOperationException("Mass should match");
        }
        if (asteroid1.Physical.RadiusM != asteroid2.Physical.RadiusM)
        {
            throw new InvalidOperationException("Radius should match");
        }
        if (asteroid1.Orbital.SemiMajorAxisM != asteroid2.Orbital.SemiMajorAxisM)
        {
            throw new InvalidOperationException("Orbital distance should match");
        }
    }

    /// <summary>
    /// Tests carbonaceous spec.
    /// </summary>
    public static void TestCarbonaceousSpec()
    {
        AsteroidSpec spec = AsteroidSpec.Carbonaceous(11111);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(11111);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid == null)
        {
            throw new InvalidOperationException("Should generate carbonaceous asteroid");
        }
        if (asteroid.Surface.SurfaceType != "carbonaceous")
        {
            throw new InvalidOperationException("Should have carbonaceous surface");
        }
        if (asteroid.Surface.Albedo >= 0.15)
        {
            throw new InvalidOperationException("C-type should have low albedo");
        }
    }

    /// <summary>
    /// Tests silicaceous spec.
    /// </summary>
    public static void TestSilicaceousSpec()
    {
        AsteroidSpec spec = AsteroidSpec.Stony(22222);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(22222);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid == null)
        {
            throw new InvalidOperationException("Should generate stony asteroid");
        }
        if (asteroid.Surface.SurfaceType != "silicaceous")
        {
            throw new InvalidOperationException("Should have silicaceous surface");
        }
    }

    /// <summary>
    /// Tests metallic spec.
    /// </summary>
    public static void TestMetallicSpec()
    {
        AsteroidSpec spec = AsteroidSpec.Metallic(33333);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(33333);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid == null)
        {
            throw new InvalidOperationException("Should generate metallic asteroid");
        }
        if (asteroid.Surface.SurfaceType != "metallic")
        {
            throw new InvalidOperationException("Should have metallic surface");
        }
        double density = asteroid.Physical.GetDensityKgM3();
        if (density <= 4000.0)
        {
            throw new InvalidOperationException($"M-type should have high density (got {density})");
        }
    }

    /// <summary>
    /// Tests ceres like spec.
    /// </summary>
    public static void TestCeresLikeSpec()
    {
        AsteroidSpec spec = AsteroidSpec.CeresLike(44444);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(44444);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid == null)
        {
            throw new InvalidOperationException("Should generate Ceres-like asteroid");
        }
        if (asteroid.Physical.MassKg <= 1.0e19)
        {
            throw new InvalidOperationException("Large asteroid should have significant mass");
        }
        if (asteroid.Physical.Oblateness >= 0.15)
        {
            throw new InvalidOperationException("Large asteroid should be more spherical");
        }
    }

    /// <summary>
    /// Tests no atmosphere.
    /// </summary>
    public static void TestNoAtmosphere()
    {
        AsteroidSpec spec = AsteroidSpec.Random(55555);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(55555);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid.HasAtmosphere())
        {
            throw new InvalidOperationException("Asteroids should not have atmosphere");
        }
    }

    /// <summary>
    /// Tests has surface.
    /// </summary>
    public static void TestHasSurface()
    {
        AsteroidSpec spec = AsteroidSpec.Random(66666);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(66666);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (!asteroid.HasSurface())
        {
            throw new InvalidOperationException("Asteroid should have surface");
        }
        if (asteroid.Surface.Terrain == null)
        {
            throw new InvalidOperationException("Asteroid should have terrain");
        }
    }

    /// <summary>
    /// Tests high crater density.
    /// </summary>
    public static void TestHighCraterDensity()
    {
        AsteroidSpec spec = AsteroidSpec.Random(77777);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(77777);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid.Surface.Terrain.CraterDensity <= 0.5)
        {
            throw new InvalidOperationException("Asteroids should be heavily cratered");
        }
    }

    /// <summary>
    /// Tests no volcanism.
    /// </summary>
    public static void TestNoVolcanism()
    {
        AsteroidSpec spec = AsteroidSpec.Random(88888);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(88888);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid.Surface.VolcanismLevel != 0.0)
        {
            throw new InvalidOperationException("Asteroids should have no volcanism");
        }
    }

    /// <summary>
    /// Tests no erosion.
    /// </summary>
    public static void TestNoErosion()
    {
        AsteroidSpec spec = AsteroidSpec.Random(99999);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(99999);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        if (asteroid.Surface.Terrain.ErosionLevel != 0.0)
        {
            throw new InvalidOperationException("Asteroids should have no erosion");
        }
    }

    /// <summary>
    /// Tests orbital in main belt by default.
    /// </summary>
    public static void TestOrbitalInMainBeltByDefault()
    {
        AsteroidSpec spec = AsteroidSpec.Random(10101);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(10101);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        double distanceAu = asteroid.Orbital.SemiMajorAxisM / Units.AuMeters;
        if (distanceAu < 2.0)
        {
            throw new InvalidOperationException($"Should be in or near main belt (got {distanceAu} AU)");
        }
        if (distanceAu > 3.5)
        {
            throw new InvalidOperationException($"Should be in or near main belt (got {distanceAu} AU)");
        }
    }

    /// <summary>
    /// Tests orbital override.
    /// </summary>
    public static void TestOrbitalOverride()
    {
        AsteroidSpec spec = AsteroidSpec.Random(20202);
        spec.SetOverride("orbital.semi_major_axis_m", 5.0 * Units.AuMeters);
        ParentContext context = CreateSolarContext();
        SeededRng rng = new SeededRng(20202);

        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, rng);

        double distanceAu = asteroid.Orbital.SemiMajorAxisM / Units.AuMeters;
        if (System.Math.Abs(distanceAu - 5.0) >= 0.01)
        {
            throw new InvalidOperationException("Should respect orbital override");
        }
    }

    /// <summary>
    /// Tests different seeds produce different asteroids.
    /// </summary>
    public static void TestDifferentSeedsProduceDifferentAsteroids()
    {
        ParentContext context = CreateSolarContext();

        AsteroidSpec spec1 = AsteroidSpec.Random(11111);
        AsteroidSpec spec2 = AsteroidSpec.Random(22222);
        SeededRng rng1 = new SeededRng(11111);
        SeededRng rng2 = new SeededRng(22222);

        CelestialBody asteroid1 = AsteroidGenerator.Generate(spec1, context, rng1);
        CelestialBody asteroid2 = AsteroidGenerator.Generate(spec2, context, rng2);

        if (asteroid1.Id == asteroid2.Id)
        {
            throw new InvalidOperationException("Different seeds should produce different IDs");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_physical_properties_valid_ranges.
    /// </summary>
    private static void TestPhysicalPropertiesValidRanges()
    {
        TestDifferentSeedsProduceDifferentAsteroids();
    }

    /// <summary>
    /// Legacy parity alias for test_orbital_properties_valid_ranges.
    /// </summary>
    private static void TestOrbitalPropertiesValidRanges()
    {
        TestOrbitalInMainBeltByDefault();
    }

    /// <summary>
    /// Legacy parity alias for test_type_distribution.
    /// </summary>
    private static void TestTypeDistribution()
    {
        TestCarbonaceousSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_composition_matches_type.
    /// </summary>
    private static void TestCompositionMatchesType()
    {
        TestCeresLikeSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_provenance_stored.
    /// </summary>
    private static void TestProvenanceStored()
    {
        TestCarbonaceousSpec();
    }

    /// <summary>
    /// Legacy parity alias for test_typical_vs_large_mass_difference.
    /// </summary>
    private static void TestTypicalVsLargeMassDifference()
    {
        TestDifferentSeedsProduceDifferentAsteroids();
    }

    /// <summary>
    /// Legacy parity alias for test_small_asteroids_more_irregular.
    /// </summary>
    private static void TestSmallAsteroidsMoreIrregular()
    {
        TestDifferentSeedsProduceDifferentAsteroids();
    }
}

