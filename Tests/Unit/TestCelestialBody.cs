#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for CelestialBody data model.
/// </summary>
public static class TestCelestialBody
{
    /// <summary>
    /// Tests creation with minimal values.
    /// </summary>
    public static void TestMinimalCreation()
    {
        CelestialBody body = new CelestialBody("test_001", "Test Planet");
        if (body.Id != "test_001")
        {
            throw new InvalidOperationException($"Expected id 'test_001', got '{body.Id}'");
        }
        if (body.Name != "Test Planet")
        {
            throw new InvalidOperationException($"Expected name 'Test Planet', got '{body.Name}'");
        }
        if (body.Type != CelestialType.Type.Planet)
        {
            throw new InvalidOperationException($"Expected type Planet, got {body.Type}");
        }
        if (body.Physical == null)
        {
            throw new InvalidOperationException("Expected non-null physical");
        }
    }

    /// <summary>
    /// Tests creation with all parameters.
    /// </summary>
    public static void TestFullCreation()
    {
        PhysicalProps physical = new PhysicalProps(1.0e24, 6.0e6);
        Provenance provenance = Provenance.CreateCurrent(12345);
        CelestialBody body = new CelestialBody(
            "star_001",
            "Test Star",
            CelestialType.Type.Star,
            physical,
            provenance
        );

        if (body.Id != "star_001")
        {
            throw new InvalidOperationException($"Expected id 'star_001', got '{body.Id}'");
        }
        if (body.Name != "Test Star")
        {
            throw new InvalidOperationException($"Expected name 'Test Star', got '{body.Name}'");
        }
        if (body.Type != CelestialType.Type.Star)
        {
            throw new InvalidOperationException($"Expected type Star, got {body.Type}");
        }
        if (body.Physical.MassKg != 1.0e24)
        {
            throw new InvalidOperationException($"Expected mass 1.0e24, got {body.Physical.MassKg}");
        }
        if (body.Provenance == null)
        {
            throw new InvalidOperationException("Expected non-null provenance");
        }
        if (body.Provenance.GenerationSeed != 12345)
        {
            throw new InvalidOperationException($"Expected generation_seed 12345, got {body.Provenance.GenerationSeed}");
        }
    }

    /// <summary>
    /// Tests optional component flags.
    /// </summary>
    public static void TestHasComponentFlags()
    {
        CelestialBody body = new CelestialBody("test_001", "Test");

        if (body.HasOrbital())
        {
            throw new InvalidOperationException("Expected no orbital initially");
        }
        if (body.HasStellar())
        {
            throw new InvalidOperationException("Expected no stellar initially");
        }
        if (body.HasSurface())
        {
            throw new InvalidOperationException("Expected no surface initially");
        }
        if (body.HasAtmosphere())
        {
            throw new InvalidOperationException("Expected no atmosphere initially");
        }
        if (body.HasRingSystem())
        {
            throw new InvalidOperationException("Expected no ring system initially");
        }

        body.Orbital = new OrbitalProps();
        if (!body.HasOrbital())
        {
            throw new InvalidOperationException("Expected orbital after setting");
        }

        body.Stellar = new StellarProps();
        if (!body.HasStellar())
        {
            throw new InvalidOperationException("Expected stellar after setting");
        }

        body.Surface = new SurfaceProps();
        if (!body.HasSurface())
        {
            throw new InvalidOperationException("Expected surface after setting");
        }

        body.Atmosphere = new AtmosphereProps();
        if (!body.HasAtmosphere())
        {
            throw new InvalidOperationException("Expected atmosphere after setting");
        }

        RingBand band = new RingBand(1.0e8, 2.0e8);
        Godot.Collections.Array<RingBand> bands = new Godot.Collections.Array<RingBand> { band };
        body.RingSystem = new RingSystemProps(bands);
        if (!body.HasRingSystem())
        {
            throw new InvalidOperationException("Expected ring system after setting");
        }
    }

    /// <summary>
    /// Tests type string conversion.
    /// </summary>
    public static void TestTypeString()
    {
        CelestialBody body = new CelestialBody("test", "Test", CelestialType.Type.Moon);
        if (body.GetTypeString() != "Moon")
        {
            throw new InvalidOperationException($"Expected type string 'Moon', got '{body.GetTypeString()}'");
        }
    }

    /// <summary>
    /// Tests all celestial types.
    /// </summary>
    public static void TestAllTypes()
    {
        CelestialType.Type[] types = new CelestialType.Type[]
        {
            CelestialType.Type.Star,
            CelestialType.Type.Planet,
            CelestialType.Type.Moon,
            CelestialType.Type.Asteroid,
        };

        foreach (CelestialType.Type t in types)
        {
            CelestialBody body = new CelestialBody("test", "Test", t);
            if (body.Type != t)
            {
                throw new InvalidOperationException($"Expected type {t}, got {body.Type}");
            }
            if (body.GetTypeString().Length == 0)
            {
                throw new InvalidOperationException($"Expected non-empty type string for type {t}");
            }
        }
    }
}
