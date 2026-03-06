#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for CelestialValidator.
/// </summary>
public static class TestCelestialValidator
{
    /// <summary>
    /// Creates a valid Earth-like planet for testing.
    /// </summary>
    private static CelestialBody CreateValidPlanet()
    {
        PhysicalProps physical = new PhysicalProps(
            5.972e24,
            6.371e6,
            86400.0,
            23.5
        );
        Provenance provenance = Provenance.CreateCurrent(12345);
        CelestialBody body = new CelestialBody(
            "earth_001",
            "Earth-like",
            CelestialType.Type.Planet,
            physical,
            provenance
        );
        return body;
    }

    /// <summary>
    /// Tests validation passes for valid body.
    /// </summary>
    public static void TestValidBodyPasses()
    {
        CelestialBody body = CreateValidPlanet();
        ValidationResult result = CelestialValidator.Validate(body);
        if (!result.IsValid())
        {
            throw new InvalidOperationException("Valid body should pass validation");
        }
    }

    /// <summary>
    /// Tests empty ID fails validation.
    /// </summary>
    public static void TestEmptyIdFails()
    {
        CelestialBody body = CreateValidPlanet();
        body.Id = "";
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Empty ID should fail validation");
        }
        if (result.GetErrorCount() <= 0)
        {
            throw new InvalidOperationException($"Expected error count > 0, got {result.GetErrorCount()}");
        }
    }

    /// <summary>
    /// Tests zero mass fails validation.
    /// </summary>
    public static void TestZeroMassFails()
    {
        CelestialBody body = CreateValidPlanet();
        body.Physical.MassKg = 0.0;
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Zero mass should fail validation");
        }
    }

    /// <summary>
    /// Tests negative mass fails validation.
    /// </summary>
    public static void TestNegativeMassFails()
    {
        CelestialBody body = CreateValidPlanet();
        body.Physical.MassKg = -1.0;
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Negative mass should fail validation");
        }
    }

    /// <summary>
    /// Tests zero radius fails validation.
    /// </summary>
    public static void TestZeroRadiusFails()
    {
        CelestialBody body = CreateValidPlanet();
        body.Physical.RadiusM = 0.0;
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Zero radius should fail validation");
        }
    }

    /// <summary>
    /// Tests invalid orbital eccentricity fails validation.
    /// </summary>
    public static void TestNegativeEccentricityFails()
    {
        CelestialBody body = CreateValidPlanet();
        body.Orbital = new OrbitalProps(1.5e11, -0.1);
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Negative eccentricity should fail validation");
        }
    }

    /// <summary>
    /// Tests unbound orbit eccentricity generates warning.
    /// </summary>
    public static void TestHighEccentricityWarns()
    {
        CelestialBody body = CreateValidPlanet();
        body.Orbital = new OrbitalProps(1.5e11, 1.5);
        ValidationResult result = CelestialValidator.Validate(body);
        if (!result.IsValid())
        {
            throw new InvalidOperationException("High eccentricity should be valid with warning");
        }
        if (result.GetWarningCount() <= 0)
        {
            throw new InvalidOperationException($"Expected warning count > 0, got {result.GetWarningCount()}");
        }
    }

    /// <summary>
    /// Tests invalid albedo fails validation.
    /// </summary>
    public static void TestInvalidAlbedoFails()
    {
        CelestialBody body = CreateValidPlanet();
        body.Surface = new SurfaceProps(288.0, 1.5);
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Invalid albedo should fail validation");
        }
    }

    /// <summary>
    /// Tests negative temperature fails validation.
    /// </summary>
    public static void TestNegativeTemperatureFails()
    {
        CelestialBody body = CreateValidPlanet();
        body.Surface = new SurfaceProps(-10.0, 0.3);
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Negative temperature should fail validation");
        }
    }

    /// <summary>
    /// Tests atmosphere composition warning.
    /// </summary>
    public static void TestAtmosphereCompositionWarning()
    {
        CelestialBody body = CreateValidPlanet();
        body.Atmosphere = new AtmosphereProps(101325.0, 8500.0, new Dictionary { ["N2"] = 0.5 });
        ValidationResult result = CelestialValidator.Validate(body);
        if (!result.IsValid())
        {
            throw new InvalidOperationException("Partial composition should be valid with warning");
        }
        if (result.GetWarningCount() <= 0)
        {
            throw new InvalidOperationException($"Expected warning count > 0, got {result.GetWarningCount()}");
        }
    }

    /// <summary>
    /// Tests ring band inner radius less than outer radius.
    /// </summary>
    public static void TestRingBandRadiusOrder()
    {
        CelestialBody body = CreateValidPlanet();
        RingBand badBand = new RingBand(2.0e8, 1.0e8);
        List<RingBand> bands = new List<RingBand> { badBand };
        body.RingSystem = new RingSystemProps(bands);
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Ring band with inner > outer should fail validation");
        }
    }

    /// <summary>
    /// Tests ring band inside body fails.
    /// </summary>
    public static void TestRingBandInsideBodyFails()
    {
        CelestialBody body = CreateValidPlanet();
        RingBand badBand = new RingBand(1.0e6, 2.0e8);
        List<RingBand> bands = new List<RingBand> { badBand };
        body.RingSystem = new RingSystemProps(bands);
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.IsValid())
        {
            throw new InvalidOperationException("Ring band inside body should fail validation");
        }
    }

    /// <summary>
    /// Tests valid ring system passes.
    /// </summary>
    public static void TestValidRingSystemPasses()
    {
        CelestialBody body = CreateValidPlanet();
        RingBand band = new RingBand(1.0e8, 2.0e8, 0.5, new Dictionary { ["ice"] = 1.0 }, 1.0, "Main");
        List<RingBand> bands = new List<RingBand> { band };
        body.RingSystem = new RingSystemProps(bands, 1.0e18);
        ValidationResult result = CelestialValidator.Validate(body);
        if (!result.IsValid())
        {
            throw new InvalidOperationException("Valid ring system should pass validation");
        }
    }

    /// <summary>
    /// Tests stellar validation on star.
    /// </summary>
    public static void TestStarWithStellarPasses()
    {
        PhysicalProps physical = new PhysicalProps(1.989e30, 6.957e8);
        CelestialBody body = new CelestialBody(
            "sun_001", "Sun", CelestialType.Type.Star, physical
        );
        body.Stellar = new StellarProps(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9);
        ValidationResult result = CelestialValidator.Validate(body);
        if (!result.IsValid())
        {
            throw new InvalidOperationException("Star with stellar properties should pass validation");
        }
    }

    /// <summary>
    /// Tests star without stellar properties warns.
    /// </summary>
    public static void TestStarWithoutStellarWarns()
    {
        PhysicalProps physical = new PhysicalProps(1.989e30, 6.957e8);
        CelestialBody body = new CelestialBody(
            "sun_001", "Sun", CelestialType.Type.Star, physical
        );
        ValidationResult result = CelestialValidator.Validate(body);
        if (result.GetWarningCount() <= 0)
        {
            throw new InvalidOperationException($"Expected warning count > 0, got {result.GetWarningCount()}");
        }
    }

    /// <summary>
    /// Tests star with surface property generates warning.
    /// </summary>
    public static void TestStarWithSurfaceWarns()
    {
        PhysicalProps physical = new PhysicalProps(1.989e30, 6.957e8);
        CelestialBody body = new CelestialBody(
            "sun_001", "Sun", CelestialType.Type.Star, physical
        );
        body.Surface = new SurfaceProps(5778.0, 0.0);
        ValidationResult result = CelestialValidator.Validate(body);
        if (!result.IsValid())
        {
            throw new InvalidOperationException("Star with surface should be valid with warning");
        }
        if (result.GetWarningCount() <= 0)
        {
            throw new InvalidOperationException($"Expected warning count > 0, got {result.GetWarningCount()}");
        }
    }
}
