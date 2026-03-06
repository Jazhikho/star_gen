#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Math;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for unit conversion functions.
/// </summary>
public static class TestUnits
{
    private const double DefaultTolerance = 0.00001;
    private const double LooseTolerance = 0.0001;

    /// <summary>
    /// Tests solar mass conversion round-trip.
    /// </summary>
    public static void TestSolarMassRoundTrip()
    {
        double original = 1.5;
        double kg = Units.SolarMassesToKg(original);
        double back = Units.KgToSolarMasses(kg);
        if (System.Math.Abs(back - original) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests Earth mass conversion round-trip.
    /// </summary>
    public static void TestEarthMassRoundTrip()
    {
        double original = 317.8;
        double kg = Units.EarthMassesToKg(original);
        double back = Units.KgToEarthMasses(kg);
        if (System.Math.Abs(back - original) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests Jupiter mass conversion round-trip.
    /// </summary>
    public static void TestJupiterMassRoundTrip()
    {
        double original = 2.5;
        double kg = Units.JupiterMassesToKg(original);
        double back = Units.KgToJupiterMasses(kg);
        if (System.Math.Abs(back - original) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests AU conversion round-trip.
    /// </summary>
    public static void TestAuRoundTrip()
    {
        double original = 1.0;
        double meters = Units.AuToMeters(original);
        double back = Units.MetersToAu(meters);
        if (System.Math.Abs(back - original) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests light year conversion round-trip.
    /// </summary>
    public static void TestLightYearRoundTrip()
    {
        double original = 4.24;
        double meters = Units.LightYearsToMeters(original);
        double back = Units.MetersToLightYears(meters);
        if (System.Math.Abs(back - original) > LooseTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests parsec conversion round-trip.
    /// </summary>
    public static void TestParsecRoundTrip()
    {
        double original = 1.3;
        double meters = Units.ParsecsToMeters(original);
        double back = Units.MetersToParsecs(meters);
        if (System.Math.Abs(back - original) > LooseTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests solar radius conversion round-trip.
    /// </summary>
    public static void TestSolarRadiusRoundTrip()
    {
        double original = 0.1;
        double meters = Units.SolarRadiiToMeters(original);
        double back = Units.MetersToSolarRadii(meters);
        if (System.Math.Abs(back - original) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests Earth radius conversion round-trip.
    /// </summary>
    public static void TestEarthRadiusRoundTrip()
    {
        double original = 11.2;
        double meters = Units.EarthRadiiToMeters(original);
        double back = Units.MetersToEarthRadii(meters);
        if (System.Math.Abs(back - original) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected {original}, got {back}");
        }
    }

    /// <summary>
    /// Tests Celsius to Kelvin conversion.
    /// </summary>
    public static void TestCelsiusToKelvin()
    {
        double result1 = Units.CelsiusToKelvin(0.0);
        if (System.Math.Abs(result1 - 273.15) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 273.15, got {result1}");
        }

        double result2 = Units.CelsiusToKelvin(100.0);
        if (System.Math.Abs(result2 - 373.15) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 373.15, got {result2}");
        }

        double result3 = Units.CelsiusToKelvin(-273.15);
        if (System.Math.Abs(result3 - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result3}");
        }
    }

    /// <summary>
    /// Tests Kelvin to Celsius conversion.
    /// </summary>
    public static void TestKelvinToCelsius()
    {
        double result1 = Units.KelvinToCelsius(273.15);
        if (System.Math.Abs(result1 - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result1}");
        }

        double result2 = Units.KelvinToCelsius(373.15);
        if (System.Math.Abs(result2 - 100.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 100.0, got {result2}");
        }

        double result3 = Units.KelvinToCelsius(0.0);
        if (System.Math.Abs(result3 - (-273.15)) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected -273.15, got {result3}");
        }
    }

    /// <summary>
    /// Tests known physical relationship: 1 Jupiter mass ≈ 317.8 Earth masses.
    /// </summary>
    public static void TestJupiterEarthMassRelationship()
    {
        double jupiterKg = Units.JupiterMassesToKg(1.0);
        double earthMasses = Units.KgToEarthMasses(jupiterKg);
        if (earthMasses < 317.0 || earthMasses > 319.0)
        {
            throw new InvalidOperationException($"Expected Earth masses in range [317.0, 319.0], got {earthMasses}");
        }
    }
}
