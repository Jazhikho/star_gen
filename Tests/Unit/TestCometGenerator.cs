#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for comet generation.
/// </summary>
public static class TestCometGenerator
{
    /// <summary>
    /// Tests comet generation returns a comet body.
    /// </summary>
    public static void TestGenerateReturnsCometBody()
    {
        CometSpec spec = CometSpec.JupiterFamily(12345);
        ParentContext context = ParentContext.SunLike(5.0 * Units.AuMeters);
        SeededRng rng = new SeededRng(12345);

        CelestialBody comet = CometGenerator.Generate(spec, context, rng);
        if (comet.Type != CelestialType.Type.Comet)
        {
            throw new InvalidOperationException("Expected comet body type.");
        }

        if (!comet.HasSurface() || comet.Surface == null || comet.Surface.SurfaceType != "cometary")
        {
            throw new InvalidOperationException("Expected cometary surface properties.");
        }
    }

    /// <summary>
    /// Tests comet generation is deterministic for the same seed.
    /// </summary>
    public static void TestGenerateIsDeterministic()
    {
        CometSpec specA = CometSpec.LongPeriod(24680);
        CometSpec specB = CometSpec.LongPeriod(24680);
        ParentContext context = ParentContext.SunLike(5.0 * Units.AuMeters);
        SeededRng rngA = new SeededRng(24680);
        SeededRng rngB = new SeededRng(24680);

        CelestialBody cometA = CometGenerator.Generate(specA, context, rngA);
        CelestialBody cometB = CometGenerator.Generate(specB, context, rngB);

        if (cometA.Id != cometB.Id)
        {
            throw new InvalidOperationException("Expected deterministic comet IDs.");
        }

        if (cometA.Physical.MassKg != cometB.Physical.MassKg)
        {
            throw new InvalidOperationException("Expected deterministic comet mass.");
        }

        if (cometA.Orbital == null || cometB.Orbital == null)
        {
            throw new InvalidOperationException("Expected orbital data on generated comets.");
        }

        if (cometA.Orbital.SemiMajorAxisM != cometB.Orbital.SemiMajorAxisM)
        {
            throw new InvalidOperationException("Expected deterministic comet orbit.");
        }
    }

    /// <summary>
    /// Tests long-period comets bias toward stretched outer-system orbits.
    /// </summary>
    public static void TestLongPeriodPresetBiasesOuterOrbit()
    {
        CometSpec spec = CometSpec.LongPeriod(31415);
        ParentContext context = ParentContext.SunLike(5.0 * Units.AuMeters);
        SeededRng rng = new SeededRng(31415);

        CelestialBody comet = CometGenerator.Generate(spec, context, rng);
        if (comet.Orbital == null)
        {
            throw new InvalidOperationException("Expected orbital data on long-period comet.");
        }

        double semiMajorAxisAu = comet.Orbital.SemiMajorAxisM / Units.AuMeters;
        if (semiMajorAxisAu < 20.0)
        {
            throw new InvalidOperationException($"Expected long-period comet semi-major axis >= 20 AU, got {semiMajorAxisAu:0.00} AU.");
        }
    }
}
