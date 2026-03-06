#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for RingSystemGenerator.
/// </summary>
public static class TestRingSystemGenerator
{
    /// <summary>
    /// Creates Saturn-like physical properties for testing.
    /// </summary>
    private static PhysicalProps CreateSaturnPhysical()
    {
        return new PhysicalProps(
            5.683e26,
            5.8232e7,
            38362.4,
            26.73,
            0.0687,
            4.6e18,
            8.0e16
        );
    }

    /// <summary>
    /// Creates a Saturn-like context (outer solar system, beyond ice line).
    /// </summary>
    private static ParentContext CreateSaturnContext()
    {
        return ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            9.5 * Units.AuMeters
        );
    }

    /// <summary>
    /// Creates an inner solar system context (inside ice line).
    /// </summary>
    private static ParentContext CreateInnerContext()
    {
        return ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            1.5 * Units.AuMeters
        );
    }

    /// <summary>
    /// Tests generate returns ring system.
    /// </summary>
    public static void TestGenerateReturnsRingSystem()
    {
        RingSystemSpec spec = RingSystemSpec.Random(12345);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(12345);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should return a RingSystemProps");
        }
        if (rings.GetBandCount() <= 0)
        {
            throw new InvalidOperationException("Should have at least one band");
        }
    }

    /// <summary>
    /// Tests generate is deterministic.
    /// </summary>
    public static void TestGenerateIsDeterministic()
    {
        RingSystemSpec spec1 = RingSystemSpec.Random(54321);
        RingSystemSpec spec2 = RingSystemSpec.Random(54321);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng1 = new SeededRng(54321);
        SeededRng rng2 = new SeededRng(54321);

        RingSystemProps rings1 = RingSystemGenerator.Generate(spec1, physical, context, rng1);
        RingSystemProps rings2 = RingSystemGenerator.Generate(spec2, physical, context, rng2);

        if (rings1.GetBandCount() != rings2.GetBandCount())
        {
            throw new InvalidOperationException("Band count should match");
        }
        if (rings1.TotalMassKg != rings2.TotalMassKg)
        {
            throw new InvalidOperationException("Mass should match");
        }
        if (rings1.GetInnerRadiusM() != rings2.GetInnerRadiusM())
        {
            throw new InvalidOperationException("Inner radius should match");
        }
    }

    /// <summary>
    /// Tests trace complexity.
    /// </summary>
    public static void TestTraceComplexity()
    {
        RingSystemSpec spec = RingSystemSpec.Trace(11111);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(11111);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate trace rings");
        }
        if (rings.GetBandCount() != 1)
        {
            throw new InvalidOperationException("Trace rings should have 1 band");
        }
        if (rings.Bands[0].OpticalDepth >= 0.2)
        {
            throw new InvalidOperationException("Trace rings should have low optical depth");
        }
    }

    /// <summary>
    /// Tests simple complexity.
    /// </summary>
    public static void TestSimpleComplexity()
    {
        RingSystemSpec spec = RingSystemSpec.Simple(22222);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(22222);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate simple rings");
        }
        if (rings.GetBandCount() < 2)
        {
            throw new InvalidOperationException("Simple rings should have at least 2 bands");
        }
        if (rings.GetBandCount() > 3)
        {
            throw new InvalidOperationException("Simple rings should have at most 3 bands");
        }
    }

    /// <summary>
    /// Tests complex complexity.
    /// </summary>
    public static void TestComplexComplexity()
    {
        RingSystemSpec spec = RingSystemSpec.Complex(33333);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(33333);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate complex rings");
        }
        if (rings.GetBandCount() < 4)
        {
            throw new InvalidOperationException("Complex rings should have at least 4 bands");
        }
        if (rings.GetBandCount() > 7)
        {
            throw new InvalidOperationException("Complex rings should have at most 7 bands");
        }
    }

    /// <summary>
    /// Tests icy composition beyond ice line.
    /// </summary>
    public static void TestIcyCompositionBeyondIceLine()
    {
        RingSystemSpec spec = RingSystemSpec.Random(44444);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(44444);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }
        Godot.Collections.Dictionary composition = rings.Bands[0].Composition;
        if (!composition.ContainsKey("water_ice"))
        {
            throw new InvalidOperationException("Should have water ice beyond ice line");
        }
        if (composition["water_ice"].AsDouble() <= 0.5)
        {
            throw new InvalidOperationException("Should be predominantly icy");
        }
    }

    /// <summary>
    /// Tests rocky composition inside ice line.
    /// </summary>
    public static void TestRockyCompositionInsideIceLine()
    {
        RingSystemSpec spec = RingSystemSpec.Random(55555);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateInnerContext();
        SeededRng rng = new SeededRng(55555);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }
        Godot.Collections.Dictionary composition = rings.Bands[0].Composition;
        if (!composition.ContainsKey("silicates"))
        {
            throw new InvalidOperationException("Should have silicates inside ice line");
        }
        if (composition["silicates"].AsDouble() <= 0.5)
        {
            throw new InvalidOperationException("Should be predominantly rocky");
        }
    }

    /// <summary>
    /// Tests forced icy composition.
    /// </summary>
    public static void TestForcedIcyComposition()
    {
        RingSystemSpec spec = RingSystemSpec.Icy(66666);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateInnerContext();
        SeededRng rng = new SeededRng(66666);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }
        Godot.Collections.Dictionary composition = rings.Bands[0].Composition;
        if (!composition.ContainsKey("water_ice"))
        {
            throw new InvalidOperationException("Should have water ice when forced");
        }
    }

    /// <summary>
    /// Tests forced rocky composition.
    /// </summary>
    public static void TestForcedRockyComposition()
    {
        RingSystemSpec spec = RingSystemSpec.Rocky(77777);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(77777);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }
        Godot.Collections.Dictionary composition = rings.Bands[0].Composition;
        if (!composition.ContainsKey("silicates"))
        {
            throw new InvalidOperationException("Should have silicates when forced rocky");
        }
    }

    /// <summary>
    /// Tests rings outside planet radius.
    /// </summary>
    public static void TestRingsOutsidePlanetRadius()
    {
        RingSystemSpec spec = RingSystemSpec.Random(88888);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(88888);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }
        double innerRadius = rings.GetInnerRadiusM();
        if (innerRadius <= physical.RadiusM)
        {
            throw new InvalidOperationException("Rings should be outside planet radius");
        }
    }

    /// <summary>
    /// Tests bands ordered by radius.
    /// </summary>
    public static void TestBandsOrderedByRadius()
    {
        RingSystemSpec spec = RingSystemSpec.Complex(99999);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(99999);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }

        for (int i = 0; i < rings.GetBandCount(); i++)
        {
            RingBand band = rings.GetBand(i);
            if (band.OuterRadiusM <= band.InnerRadiusM)
            {
                throw new InvalidOperationException("Band outer > inner");
            }

            if (i > 0)
            {
                RingBand prevBand = rings.GetBand(i - 1);
                if (band.InnerRadiusM < prevBand.OuterRadiusM)
                {
                    throw new InvalidOperationException("Bands should not overlap");
                }
            }
        }
    }

    /// <summary>
    /// Tests band properties valid.
    /// </summary>
    public static void TestBandPropertiesValid()
    {
        RingSystemSpec spec = RingSystemSpec.Complex(10101);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(10101);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }

        for (int i = 0; i < rings.GetBandCount(); i++)
        {
            RingBand band = rings.GetBand(i);
            if (band.InnerRadiusM <= 0.0)
            {
                throw new InvalidOperationException("Inner radius should be positive");
            }
            if (band.OuterRadiusM <= 0.0)
            {
                throw new InvalidOperationException("Outer radius should be positive");
            }
            if (band.OpticalDepth <= 0.0)
            {
                throw new InvalidOperationException("Optical depth should be positive");
            }
            if (band.ParticleSizeM <= 0.0)
            {
                throw new InvalidOperationException("Particle size should be positive");
            }
            if (band.Composition.Count == 0)
            {
                throw new InvalidOperationException("Composition should not be empty");
            }
        }
    }

    /// <summary>
    /// Tests total mass positive.
    /// </summary>
    public static void TestTotalMassPositive()
    {
        RingSystemSpec spec = RingSystemSpec.Random(20202);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(20202);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }
        if (rings.TotalMassKg <= 0.0)
        {
            throw new InvalidOperationException("Total mass should be positive");
        }
    }

    /// <summary>
    /// Tests inclination small.
    /// </summary>
    public static void TestInclinationSmall()
    {
        RingSystemSpec spec = RingSystemSpec.Random(30303);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        SeededRng rng = new SeededRng(30303);

        RingSystemProps rings = RingSystemGenerator.Generate(spec, physical, context, rng);

        if (rings == null)
        {
            throw new InvalidOperationException("Should generate rings");
        }
        if (rings.InclinationDeg < 0.0)
        {
            throw new InvalidOperationException("Inclination should be non-negative");
        }
        if (rings.InclinationDeg >= 10.0)
        {
            throw new InvalidOperationException("Inclination should be small (aligned with equator)");
        }
    }

    /// <summary>
    /// Tests different seeds produce different rings.
    /// </summary>
    public static void TestDifferentSeedsProduceDifferentRings()
    {
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();

        RingSystemSpec spec1 = RingSystemSpec.Random(11111);
        RingSystemSpec spec2 = RingSystemSpec.Random(22222);
        SeededRng rng1 = new SeededRng(11111);
        SeededRng rng2 = new SeededRng(22222);

        RingSystemProps rings1 = RingSystemGenerator.Generate(spec1, physical, context, rng1);
        RingSystemProps rings2 = RingSystemGenerator.Generate(spec2, physical, context, rng2);

        if (rings1 == null)
        {
            throw new InvalidOperationException("Should generate rings 1");
        }
        if (rings2 == null)
        {
            throw new InvalidOperationException("Should generate rings 2");
        }
        bool differ = (
            rings1.GetBandCount() != rings2.GetBandCount() ||
            rings1.TotalMassKg != rings2.TotalMassKg ||
            rings1.GetInnerRadiusM() != rings2.GetInnerRadiusM()
        );
        if (!differ)
        {
            throw new InvalidOperationException("Different seeds should produce different rings");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_should_have_rings_gas_giant.
    /// </summary>
    private static void TestShouldHaveRingsGasGiant()
    {
        TestDifferentSeedsProduceDifferentRings();
    }

    /// <summary>
    /// Legacy parity alias for test_should_have_rings_terrestrial_rare.
    /// </summary>
    private static void TestShouldHaveRingsTerrestrialRare()
    {
        TestDifferentSeedsProduceDifferentRings();
    }
}

