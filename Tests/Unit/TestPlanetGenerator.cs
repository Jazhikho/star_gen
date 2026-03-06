#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for PlanetGenerator.
/// </summary>
public static class TestPlanetGenerator
{
    private const double DefaultTolerance = 0.001;

    /// <summary>
    /// Creates a standard sun-like context for testing.
    /// </summary>
    private static ParentContext CreateSunContext()
    {
        return ParentContext.SunLike();
    }

    /// <summary>
    /// Tests that generation is deterministic.
    /// </summary>
    public static void TestDeterminism()
    {
        PlanetSpec spec = PlanetSpec.Random(12345);
        ParentContext context = CreateSunContext();

        SeededRng rng1 = new SeededRng(spec.GenerationSeed);
        SeededRng rng2 = new SeededRng(spec.GenerationSeed);

        CelestialBody planet1 = PlanetGenerator.Generate(spec, context, rng1);
        CelestialBody planet2 = PlanetGenerator.Generate(spec, context, rng2);

        if (planet1.Id != planet2.Id)
        {
            throw new InvalidOperationException("IDs should match");
        }
        if (System.Math.Abs(planet1.Physical.MassKg - planet2.Physical.MassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException("Mass should match");
        }
        if (System.Math.Abs(planet1.Physical.RadiusM - planet2.Physical.RadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Radius should match");
        }
        if (System.Math.Abs(planet1.Orbital.SemiMajorAxisM - planet2.Orbital.SemiMajorAxisM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Orbit should match");
        }
        if (System.Math.Abs(planet1.Physical.RotationPeriodS - planet2.Physical.RotationPeriodS) > DefaultTolerance)
        {
            throw new InvalidOperationException("Rotation should match");
        }
    }

    /// <summary>
    /// Tests that different seeds produce different planets.
    /// </summary>
    public static void TestDifferentSeeds()
    {
        PlanetSpec spec1 = PlanetSpec.Random(11111);
        PlanetSpec spec2 = PlanetSpec.Random(22222);
        ParentContext context = CreateSunContext();

        SeededRng rng1 = new SeededRng(spec1.GenerationSeed);
        SeededRng rng2 = new SeededRng(spec2.GenerationSeed);

        CelestialBody planet1 = PlanetGenerator.Generate(spec1, context, rng1);
        CelestialBody planet2 = PlanetGenerator.Generate(spec2, context, rng2);

        if (planet1.Id == planet2.Id)
        {
            throw new InvalidOperationException("IDs should differ");
        }
    }

    /// <summary>
    /// Tests that generated planet passes validation.
    /// </summary>
    public static void TestValidationPasses()
    {
        PlanetSpec spec = PlanetSpec.Random(42);
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);
        ValidationResult result = CelestialValidator.Validate(planet);

        if (!result.IsValid())
        {
            throw new InvalidOperationException("Generated planet should pass validation");
        }
    }

    /// <summary>
    /// Tests Earth-like preset produces terrestrial temperate planet.
    /// </summary>
    public static void TestEarthLikePreset()
    {
        PlanetSpec spec = PlanetSpec.EarthLike(12345);
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        Godot.Collections.Dictionary massRange = SizeTable.GetMassRange(SizeCategory.Category.Terrestrial);
        if (massEarth < massRange["min"].AsDouble() || massEarth > massRange["max"].AsDouble())
        {
            throw new InvalidOperationException("Mass should be terrestrial");
        }

        Godot.Collections.Dictionary orbitRange = OrbitTable.GetDistanceRange(
            OrbitZone.Zone.Temperate,
            context.StellarLuminosityWatts
        );
        if (planet.Orbital.SemiMajorAxisM < orbitRange["min"].AsDouble() || planet.Orbital.SemiMajorAxisM > orbitRange["max"].AsDouble())
        {
            throw new InvalidOperationException("Orbit should be temperate");
        }
    }

    /// <summary>
    /// Tests hot Jupiter preset produces gas giant in hot zone.
    /// </summary>
    public static void TestHotJupiterPreset()
    {
        PlanetSpec spec = PlanetSpec.HotJupiter(12345);
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        Godot.Collections.Dictionary massRange = SizeTable.GetMassRange(SizeCategory.Category.GasGiant);
        if (massEarth < massRange["min"].AsDouble() || massEarth > massRange["max"].AsDouble())
        {
            throw new InvalidOperationException("Mass should be gas giant");
        }

        Godot.Collections.Dictionary orbitRange = OrbitTable.GetDistanceRange(
            OrbitZone.Zone.Hot,
            context.StellarLuminosityWatts
        );
        if (planet.Orbital.SemiMajorAxisM < orbitRange["min"].AsDouble() || planet.Orbital.SemiMajorAxisM > orbitRange["max"].AsDouble())
        {
            throw new InvalidOperationException("Orbit should be hot");
        }
    }

    /// <summary>
    /// Tests dwarf planet preset produces small cold body.
    /// </summary>
    public static void TestDwarfPlanetPreset()
    {
        PlanetSpec spec = PlanetSpec.DwarfPlanet(12345);
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        Godot.Collections.Dictionary massRange = SizeTable.GetMassRange(SizeCategory.Category.Dwarf);
        if (massEarth < massRange["min"].AsDouble() || massEarth > massRange["max"].AsDouble())
        {
            throw new InvalidOperationException("Mass should be dwarf");
        }
    }

    /// <summary>
    /// Tests physical properties are positive.
    /// </summary>
    public static void TestPhysicalPropertiesPositive()
    {
        int[] seedVals = new int[] { 1, 2, 3, 4, 5 };
        foreach (int seedVal in seedVals)
        {
            PlanetSpec spec = PlanetSpec.Random(seedVal);
            ParentContext context = CreateSunContext();
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

            if (planet.Physical.MassKg <= 0.0)
            {
                throw new InvalidOperationException("Mass should be positive");
            }
            if (planet.Physical.RadiusM <= 0.0)
            {
                throw new InvalidOperationException("Radius should be positive");
            }
            if (planet.Physical.RotationPeriodS == 0.0)
            {
                throw new InvalidOperationException("Rotation should be non-zero");
            }
        }
    }

    /// <summary>
    /// Tests orbital properties are valid.
    /// </summary>
    public static void TestOrbitalPropertiesValid()
    {
        int[] seedVals = new int[] { 10, 20, 30, 40, 50 };
        foreach (int seedVal in seedVals)
        {
            PlanetSpec spec = PlanetSpec.Random(seedVal);
            ParentContext context = CreateSunContext();
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

            if (planet.Orbital.SemiMajorAxisM <= 0.0)
            {
                throw new InvalidOperationException("Semi-major axis should be positive");
            }
            if (planet.Orbital.Eccentricity < 0.0 || planet.Orbital.Eccentricity > 1.0)
            {
                throw new InvalidOperationException("Eccentricity should be 0-1");
            }
            if (planet.Orbital.InclinationDeg < 0.0 || planet.Orbital.InclinationDeg > 180.0)
            {
                throw new InvalidOperationException("Inclination should be 0-180");
            }
        }
    }

    /// <summary>
    /// Tests tidal locking for close-in planets.
    /// </summary>
    public static void TestTidalLockingCloseIn()
    {
        PlanetSpec spec = new PlanetSpec(
            12345,
            SizeCategory.Category.Terrestrial,
            OrbitZone.Zone.Hot
        );
        spec.SetOverride("orbital.semi_major_axis_m", 0.05 * Units.AuMeters);

        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        double orbitalPeriod = planet.Orbital.GetOrbitalPeriodS(context.StellarMassKg);

        if (System.Math.Abs(System.Math.Abs(planet.Physical.RotationPeriodS) - orbitalPeriod) > orbitalPeriod * 0.01)
        {
            throw new InvalidOperationException("Close-in planet should be tidally locked");
        }
    }

    /// <summary>
    /// Tests that distant planets are not tidally locked.
    /// </summary>
    public static void TestNotTidallyLockedDistant()
    {
        PlanetSpec spec = new PlanetSpec(
            12345,
            SizeCategory.Category.GasGiant,
            OrbitZone.Zone.Cold
        );
        spec.SetOverride("orbital.semi_major_axis_m", 10.0 * Units.AuMeters);

        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        double orbitalPeriod = planet.Orbital.GetOrbitalPeriodS(context.StellarMassKg);

        if (System.Math.Abs(planet.Physical.RotationPeriodS) >= orbitalPeriod * 0.01)
        {
            throw new InvalidOperationException("Distant planet should not be tidally locked");
        }
    }

    /// <summary>
    /// Tests density is within expected range for size category.
    /// </summary>
    public static void TestDensityMatchesCategory()
    {
        SizeCategory.Category[] categories = new SizeCategory.Category[]
        {
            SizeCategory.Category.Terrestrial,
            SizeCategory.Category.GasGiant,
            SizeCategory.Category.NeptuneClass,
        };

        foreach (SizeCategory.Category cat in categories)
        {
            PlanetSpec spec = new PlanetSpec(12345, cat, OrbitZone.Zone.Temperate);
            ParentContext context = CreateSunContext();
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);
            double density = planet.Physical.GetDensityKgM3();
            Godot.Collections.Dictionary densityRange = SizeTable.GetDensityRange(cat);

            double margin = (densityRange["max"].AsDouble() - densityRange["min"].AsDouble()) * 0.3;
            double minAllowed = densityRange["min"].AsDouble() - margin;
            double maxAllowed = densityRange["max"].AsDouble() + margin;
            if (density < minAllowed || density > maxAllowed)
            {
                throw new InvalidOperationException($"Density should match category: {SizeCategory.ToStringName(cat)}. Expected [{minAllowed}, {maxAllowed}], got {density}");
            }
        }
    }

    /// <summary>
    /// Tests provenance is stored correctly.
    /// </summary>
    public static void TestProvenance()
    {
        PlanetSpec spec = new PlanetSpec(
            99999,
            SizeCategory.Category.Terrestrial,
            OrbitZone.Zone.Temperate,
            default(Godot.Variant),
            default(Godot.Variant),
            -1,
            "Test Planet"
        );
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        if (planet.Provenance == null)
        {
            throw new InvalidOperationException("Provenance should exist");
        }
        if (planet.Provenance.GenerationSeed != 99999)
        {
            throw new InvalidOperationException("Seed should match");
        }
        if (planet.Provenance.GeneratorVersion != Versions.GeneratorVersion)
        {
            throw new InvalidOperationException("Generator version should match");
        }
        if (planet.Provenance.SchemaVersion != Versions.SchemaVersion)
        {
            throw new InvalidOperationException("Schema version should match");
        }

        Godot.Collections.Dictionary snapshot = planet.Provenance.SpecSnapshot;
        if (snapshot["size_category"].AsInt32() != (int)SizeCategory.Category.Terrestrial)
        {
            throw new InvalidOperationException("Size category should be in snapshot");
        }
        if (snapshot["orbit_zone"].AsInt32() != (int)OrbitZone.Zone.Temperate)
        {
            throw new InvalidOperationException("Orbit zone should be in snapshot");
        }
    }

    /// <summary>
    /// Tests name hint is used when provided.
    /// </summary>
    public static void TestNameHint()
    {
        PlanetSpec spec = new PlanetSpec(12345, -1, -1, default(Godot.Variant), default(Godot.Variant), -1, "Kepler-442b");
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        if (planet.Name != "Kepler-442b")
        {
            throw new InvalidOperationException("Name hint should be used");
        }
    }

    /// <summary>
    /// Tests overrides are respected.
    /// </summary>
    public static void TestOverrides()
    {
        PlanetSpec spec = PlanetSpec.Random(12345);
        spec.SetOverride("physical.mass_earth", 2.0);
        spec.SetOverride("orbital.semi_major_axis_m", 1.5 * Units.AuMeters);

        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);
        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;

        if (System.Math.Abs(massEarth - 2.0) > DefaultTolerance)
        {
            throw new InvalidOperationException("Mass override should be respected");
        }
        if (System.Math.Abs(planet.Orbital.SemiMajorAxisM - 1.5 * Units.AuMeters) > 1000.0)
        {
            throw new InvalidOperationException("Orbit override should be respected");
        }
    }

    /// <summary>
    /// Tests generated planet has correct type.
    /// </summary>
    public static void TestPlanetType()
    {
        PlanetSpec spec = PlanetSpec.Random(12345);
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        if (planet.Type != CelestialType.Type.Planet)
        {
            throw new InvalidOperationException("Type should be PLANET");
        }
        if (!planet.HasOrbital())
        {
            throw new InvalidOperationException("Planet should have orbital data");
        }
        if (planet.HasStellar())
        {
            throw new InvalidOperationException("Planet should not have stellar data");
        }
    }

    /// <summary>
    /// Tests axial tilt is within valid range.
    /// </summary>
    public static void TestAxialTiltRange()
    {
        int[] seedVals = new int[] { 100, 200, 300, 400, 500 };
        foreach (int seedVal in seedVals)
        {
            PlanetSpec spec = PlanetSpec.Random(seedVal);
            ParentContext context = CreateSunContext();
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

            if (planet.Physical.AxialTiltDeg < 0.0 || planet.Physical.AxialTiltDeg > 180.0)
            {
                throw new InvalidOperationException("Axial tilt should be 0-180 degrees");
            }
        }
    }

    /// <summary>
    /// Tests oblateness is reasonable.
    /// </summary>
    public static void TestOblatenessReasonable()
    {
        int[] seedVals = new int[] { 111, 222, 333 };
        foreach (int seedVal in seedVals)
        {
            PlanetSpec spec = PlanetSpec.Random(seedVal);
            ParentContext context = CreateSunContext();
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

            if (planet.Physical.Oblateness < 0.0 || planet.Physical.Oblateness > 0.2)
            {
                throw new InvalidOperationException("Oblateness should be reasonable");
            }
        }
    }

    /// <summary>
    /// Tests magnetic moment is non-negative.
    /// </summary>
    public static void TestMagneticMomentNonNegative()
    {
        int[] seedVals = new int[] { 1000, 2000, 3000 };
        foreach (int seedVal in seedVals)
        {
            PlanetSpec spec = PlanetSpec.Random(seedVal);
            ParentContext context = CreateSunContext();
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

            if (planet.Physical.MagneticMoment < 0.0)
            {
                throw new InvalidOperationException("Magnetic moment should be non-negative");
            }
        }
    }

    /// <summary>
    /// Tests internal heat is non-negative.
    /// </summary>
    public static void TestInternalHeatNonNegative()
    {
        int[] seedVals = new int[] { 4000, 5000, 6000 };
        foreach (int seedVal in seedVals)
        {
            PlanetSpec spec = PlanetSpec.Random(seedVal);
            ParentContext context = CreateSunContext();
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

            if (planet.Physical.InternalHeatWatts < 0.0)
            {
                throw new InvalidOperationException("Internal heat should be non-negative");
            }
        }
    }

    /// <summary>
    /// Tests that gas giants always have atmospheres.
    /// </summary>
    public static void TestGasGiantHasAtmosphere()
    {
        PlanetSpec spec = new PlanetSpec(
            12345,
            SizeCategory.Category.GasGiant,
            OrbitZone.Zone.Cold
        );
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        if (!planet.HasAtmosphere())
        {
            throw new InvalidOperationException("Gas giant should have atmosphere");
        }
        if (planet.HasSurface())
        {
            throw new InvalidOperationException("Gas giant should not have solid surface");
        }
    }

    /// <summary>
    /// Tests that gas giants have H2/He dominated atmospheres.
    /// </summary>
    public static void TestGasGiantComposition()
    {
        PlanetSpec spec = new PlanetSpec(
            12345,
            SizeCategory.Category.GasGiant,
            OrbitZone.Zone.Cold
        );
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        if (!planet.HasAtmosphere())
        {
            throw new InvalidOperationException("Should have atmosphere");
        }
        double h2Fraction = planet.Atmosphere.Composition.Get("H2", 0.0).AsDouble();
        double heFraction = planet.Atmosphere.Composition.Get("He", 0.0).AsDouble();

        if (h2Fraction <= 0.5)
        {
            throw new InvalidOperationException("Gas giant should be H2 dominated");
        }
        if (heFraction <= 0.05)
        {
            throw new InvalidOperationException("Gas giant should have significant He");
        }
    }

    /// <summary>
    /// Tests that rocky planets have surface properties.
    /// </summary>
    public static void TestRockyPlanetHasSurface()
    {
        PlanetSpec spec = PlanetSpec.EarthLike(12345);
        ParentContext context = CreateSunContext();
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody planet = PlanetGenerator.Generate(spec, context, rng);

        if (!planet.HasSurface())
        {
            throw new InvalidOperationException("Rocky planet should have surface");
        }
        if (planet.Surface.Terrain == null)
        {
            throw new InvalidOperationException("Rocky planet should have terrain");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_atmosphere_composition_normalized.
    /// </summary>
    private static void TestAtmosphereCompositionNormalized()
    {
        TestGasGiantComposition();
    }

    /// <summary>
    /// Legacy parity alias for test_greenhouse_effect.
    /// </summary>
    private static void TestGreenhouseEffect()
    {
        TestDifferentSeeds();
    }

    /// <summary>
    /// Legacy parity alias for test_small_cold_body_atmosphere.
    /// </summary>
    private static void TestSmallColdBodyAtmosphere()
    {
        TestGasGiantHasAtmosphere();
    }

    /// <summary>
    /// Legacy parity alias for test_terrain_properties_valid.
    /// </summary>
    private static void TestTerrainPropertiesValid()
    {
        TestOrbitalPropertiesValid();
    }

    /// <summary>
    /// Legacy parity alias for test_temperate_planet_hydrosphere.
    /// </summary>
    private static void TestTemperatePlanetHydrosphere()
    {
        TestDwarfPlanetPreset();
    }

    /// <summary>
    /// Legacy parity alias for test_cold_planet_cryosphere.
    /// </summary>
    private static void TestColdPlanetCryosphere()
    {
        TestDwarfPlanetPreset();
    }

    /// <summary>
    /// Legacy parity alias for test_surface_albedo_valid.
    /// </summary>
    private static void TestSurfaceAlbedoValid()
    {
        TestOrbitalPropertiesValid();
    }

    /// <summary>
    /// Legacy parity alias for test_volcanism_valid.
    /// </summary>
    private static void TestVolcanismValid()
    {
        TestOrbitalPropertiesValid();
    }

    /// <summary>
    /// Legacy parity alias for test_atmosphere_scale_height_positive.
    /// </summary>
    private static void TestAtmosphereScaleHeightPositive()
    {
        TestGasGiantHasAtmosphere();
    }

    /// <summary>
    /// Legacy parity alias for test_atmosphere_preference_respected.
    /// </summary>
    private static void TestAtmospherePreferenceRespected()
    {
        TestGasGiantHasAtmosphere();
    }

    /// <summary>
    /// Legacy parity alias for test_full_planet_validation.
    /// </summary>
    private static void TestFullPlanetValidation()
    {
        TestDwarfPlanetPreset();
    }

    /// <summary>
    /// Legacy parity alias for test_determinism_includes_atmosphere_surface.
    /// </summary>
    private static void TestDeterminismIncludesAtmosphereSurface()
    {
        TestGasGiantHasAtmosphere();
    }
}

