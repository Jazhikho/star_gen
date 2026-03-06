#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Throws when two values differ.
    /// </summary>
    public static void AssertEqual<T>(T expected, T actual, string message = "")
    {
        if (!ValuesEqual(expected, actual))
        {
            string msg = string.IsNullOrEmpty(message) ? "Values should be equal" : message;
            throw new InvalidOperationException($"{msg}. Expected '{expected}', got '{actual}'.");
        }
    }

    /// <summary>
    /// Non-generic equality helper that allows mixed nullable/reference call sites.
    /// </summary>
    public static void AssertEqual(object? expected, object? actual, string message = "")
    {
        if (!ValuesEqual(expected, actual))
        {
            string msg = string.IsNullOrEmpty(message) ? "Values should be equal" : message;
            throw new InvalidOperationException($"{msg}. Expected '{expected}', got '{actual}'.");
        }
    }

    /// <summary>
    /// Throws when two values differ (overload without message).
    /// </summary>
    public static void AssertEqual<T>(T expected, T actual)
    {
        AssertEqual(expected, actual, "Values should be equal");
    }

    /// <summary>
    /// Throws when two values are equal.
    /// </summary>
    public static void AssertNotEqual<T>(T left, T right, string message)
    {
        if (ValuesEqual(left, right))
        {
            throw new InvalidOperationException($"{message}. Both values were '{left}'.");
        }
    }

    /// <summary>
    /// Compares values with normalization for common Godot/.NET interop mismatches.
    /// </summary>
    private static bool ValuesEqual(object? expected, object? actual)
    {
        if (expected is null && actual is null)
        {
            return true;
        }

        if (expected is null || actual is null)
        {
            return false;
        }

        if (TryToDouble(expected, out double expectedNumber) && TryToDouble(actual, out double actualNumber))
        {
            if (double.IsNaN(expectedNumber) || double.IsNaN(actualNumber))
            {
                return false;
            }

            if (expectedNumber == actualNumber)
            {
                return true;
            }

            double difference = Math.Abs(expectedNumber - actualNumber);
            double scale = Math.Max(Math.Abs(expectedNumber), Math.Abs(actualNumber));
            double tolerance = Math.Max(1e-9, scale * 1e-9);
            return difference <= tolerance;
        }

        if (IsStringLike(expected) && IsStringLike(actual))
        {
            return expected.ToString() == actual.ToString();
        }

        return Equals(expected, actual);
    }

    private static bool IsStringLike(object value)
    {
        return value is string
            || value is StringName
            || value is NodePath;
    }

    private static bool TryToDouble(object value, out double number)
    {
        switch (value)
        {
            case byte b:
                number = b;
                return true;
            case sbyte sb:
                number = sb;
                return true;
            case short s:
                number = s;
                return true;
            case ushort us:
                number = us;
                return true;
            case int i:
                number = i;
                return true;
            case uint ui:
                number = ui;
                return true;
            case long l:
                number = l;
                return true;
            case ulong ul:
                number = ul;
                return true;
            case float f:
                number = f;
                return true;
            case double d:
                number = d;
                return true;
            case decimal dec:
                number = (double)dec;
                return true;
            case System.Enum e:
                number = Convert.ToDouble(e);
                return true;
            default:
                number = 0.0;
                return false;
        }
    }

    /// <summary>
    /// Throws when a reference value is null.
    /// </summary>
    public static void AssertNotNull(object? value, string message = "Value should not be null")
    {
        if (value == null)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Throws when a condition is false.
    /// </summary>
    public static void AssertTrue(bool condition, string message = "Condition should be true")
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Throws when a condition is true.
    /// </summary>
    public static void AssertFalse(bool condition, string message = "Condition should be false")
    {
        if (condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Throws when a reference value is not null.
    /// </summary>
    public static void AssertNull(object? value, string message = "Value should be null")
    {
        if (value != null)
        {
            throw new InvalidOperationException($"{message}. Expected null, got '{value}'.");
        }
    }

    /// <summary>
    /// Throws when actual is not greater than expected.
    /// </summary>
    public static void AssertGreaterThan<T>(T actual, T expected, string message) where T : IComparable<T>
    {
        if (actual.CompareTo(expected) <= 0)
        {
            throw new InvalidOperationException($"{message}. Expected '{actual}' > '{expected}'.");
        }
    }

    /// <summary>
    /// Numeric overload that tolerates mixed float/double literals in converted tests.
    /// </summary>
    public static void AssertGreaterThan(double actual, double expected, string message)
    {
        if (actual <= expected)
        {
            throw new InvalidOperationException($"{message}. Expected '{actual}' > '{expected}'.");
        }
    }

    /// <summary>
    /// Throws when actual is not less than expected.
    /// </summary>
    public static void AssertLessThan<T>(T actual, T expected, string message) where T : IComparable<T>
    {
        if (actual.CompareTo(expected) >= 0)
        {
            throw new InvalidOperationException($"{message}. Expected '{actual}' < '{expected}'.");
        }
    }

    /// <summary>
    /// Numeric overload that tolerates mixed float/double literals in converted tests.
    /// </summary>
    public static void AssertLessThan(double actual, double expected, string message)
    {
        if (actual >= expected)
        {
            throw new InvalidOperationException($"{message}. Expected '{actual}' < '{expected}'.");
        }
    }

    /// <summary>
    /// Throws when value is not in the specified range [min, max].
    /// </summary>
    public static void AssertInRange<T>(T value, T min, T max, string message) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new InvalidOperationException($"{message}. Expected '{value}' in range [{min}, {max}].");
        }
    }

    /// <summary>
    /// Throws when floating-point values differ beyond a supplied tolerance.
    /// </summary>
    public static void AssertFloatNear(double expected, double actual, double tolerance, string message = "")
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            string msg = string.IsNullOrEmpty(message) ? "Float values should be near" : message;
            throw new InvalidOperationException($"{msg}. Expected '{expected}', got '{actual}'.");
        }
    }

    /// <summary>
    /// Throws when floating-point values differ beyond a supplied tolerance (float overload).
    /// </summary>
    public static void AssertFloatNear(float expected, float actual, float tolerance, string message = "")
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            string msg = string.IsNullOrEmpty(message) ? "Float values should be near" : message;
            throw new InvalidOperationException($"{msg}. Expected '{expected}', got '{actual}'.");
        }
    }

    /// <summary>
    /// Legacy alias for float comparisons in converted tests.
    /// </summary>
    public static void AssertFloatEqual(float expected, float actual, float tolerance, string message = "")
    {
        AssertFloatNear(expected, actual, tolerance, message);
    }

    /// <summary>
    /// Legacy alias for double comparisons in converted tests.
    /// </summary>
    public static void AssertFloatEqual(double expected, double actual, double tolerance, string message = "")
    {
        AssertFloatNear(expected, actual, tolerance, message);
    }

    /// <summary>
    /// Throws when two Godot variants differ after tolerant deep comparison.
    /// </summary>
    private static void AssertVariantDeepEqual(Godot.Collections.Dictionary expected, Godot.Collections.Dictionary actual, string message)
    {
        if (TryFindDifference(expected, actual, "$", out string difference))
        {
            throw new InvalidOperationException($"{message}. {difference}");
        }
    }

    /// <summary>
    /// Returns the first semantic difference between two variants, if any.
    /// </summary>
    private static bool TryFindDifference(Variant expected, Variant actual, string path, out string difference)
    {
        if (IsNumericVariant(expected) && IsNumericVariant(actual))
        {
            if (NumbersEqual(ToDouble(expected), ToDouble(actual)))
            {
                difference = string.Empty;
                return false;
            }

            difference = $"{path}: numeric mismatch ({ToDouble(expected)} != {ToDouble(actual)})";
            return true;
        }

        if (IsStringLikeVariant(expected) && IsStringLikeVariant(actual))
        {
            string expectedText = expected.ToString();
            string actualText = actual.ToString();
            if (expectedText == actualText)
            {
                difference = string.Empty;
                return false;
            }

            difference = $"{path}: string mismatch ('{expectedText}' != '{actualText}')";
            return true;
        }

        if (expected.VariantType != actual.VariantType)
        {
            difference = $"{path}: type mismatch ({expected.VariantType} != {actual.VariantType})";
            return true;
        }

        if (expected.VariantType == Variant.Type.Dictionary)
        {
            Godot.Collections.Dictionary expectedDictionary = expected.AsGodotDictionary();
            Godot.Collections.Dictionary actualDictionary = actual.AsGodotDictionary();
            if (expectedDictionary.Count != actualDictionary.Count)
            {
                difference = $"{path}: dictionary size mismatch ({expectedDictionary.Count} != {actualDictionary.Count})";
                return true;
            }

            foreach (Variant key in expectedDictionary.Keys)
            {
                if (!TryGetDictionaryValue(actualDictionary, key, out Variant actualKey, out Variant actualValue))
                {
                    difference = $"{path}: missing key '{key}'";
                    return true;
                }

                string childPath = $"{path}.{actualKey}";
                if (TryFindDifference(expectedDictionary[key], actualValue, childPath, out difference))
                {
                    return true;
                }
            }

            difference = string.Empty;
            return false;
        }

        if (expected.VariantType == Variant.Type.Array)
        {
            Godot.Collections.Array expectedArray = expected.AsGodotArray();
            Godot.Collections.Array actualArray = actual.AsGodotArray();
            if (expectedArray.Count != actualArray.Count)
            {
                difference = $"{path}: array size mismatch ({expectedArray.Count} != {actualArray.Count})";
                return true;
            }

            for (int index = 0; index < expectedArray.Count; index += 1)
            {
                if (TryFindDifference(expectedArray[index], actualArray[index], $"{path}[{index}]", out difference))
                {
                    return true;
                }
            }

            difference = string.Empty;
            return false;
        }

        if (expected.Equals(actual))
        {
            difference = string.Empty;
            return false;
        }

        difference = $"{path}: value mismatch ('{expected}' != '{actual}')";
        return true;
    }

    /// <summary>
    /// Returns whether a variant is any numeric type that may be normalized through JSON.
    /// </summary>
    private static bool IsNumericVariant(Variant value)
    {
        return value.VariantType == Variant.Type.Int
            || value.VariantType == Variant.Type.Float;
    }

    /// <summary>
    /// Returns whether a variant is a string-like key/value that can normalize through JSON.
    /// </summary>
    private static bool IsStringLikeVariant(Variant value)
    {
        return value.VariantType == Variant.Type.String
            || value.VariantType == Variant.Type.StringName
            || value.VariantType == Variant.Type.NodePath;
    }

    /// <summary>
    /// Converts a numeric variant to double.
    /// </summary>
    private static double ToDouble(Variant value)
    {
        if (value.VariantType == Variant.Type.Int)
        {
            return value.AsInt64();
        }

        return value.AsDouble();
    }

    /// <summary>
    /// Compares two floating-point values with relative tolerance.
    /// </summary>
    private static bool NumbersEqual(double expected, double actual)
    {
        double difference = Math.Abs(expected - actual);
        if (difference <= 1.0e-6)
        {
            return true;
        }

        double scale = Math.Max(Math.Abs(expected), Math.Abs(actual));
        if (scale <= 1.0)
        {
            return difference <= 1.0e-6;
        }

        return difference / scale <= 1.0e-6;
    }

    /// <summary>
    /// Removes known transient fields from a payload tree before semantic comparison.
    /// </summary>
    private static void NormalizeTransientFields(Godot.Collections.Dictionary data)
    {
        RemoveTransientFields(data);
    }

    /// <summary>
    /// Walks a payload recursively and removes transient keys like timestamps.
    /// </summary>
    private static void RemoveTransientFields(Variant value)
    {
        if (value.VariantType == Variant.Type.Dictionary)
        {
            Godot.Collections.Dictionary dictionary = value.AsGodotDictionary();
            dictionary.Remove("created_timestamp");
            foreach (Variant key in dictionary.Keys)
            {
                RemoveTransientFields(dictionary[key]);
            }

            return;
        }

        if (value.VariantType == Variant.Type.Array)
        {
            Godot.Collections.Array array = value.AsGodotArray();
            foreach (Variant item in array)
            {
                RemoveTransientFields(item);
            }
        }
    }

    /// <summary>
    /// Finds a matching dictionary value while allowing string-key normalization.
    /// </summary>
    private static bool TryGetDictionaryValue(
        Godot.Collections.Dictionary dictionary,
        Variant expectedKey,
        out Variant actualKey,
        out Variant actualValue)
    {
        if (dictionary.ContainsKey(expectedKey))
        {
            actualKey = expectedKey;
            actualValue = dictionary[expectedKey];
            return true;
        }

        if (IsStringLikeVariant(expectedKey))
        {
            string expectedText = expectedKey.ToString();
            foreach (Variant candidateKey in dictionary.Keys)
            {
                if (candidateKey.ToString() == expectedText)
                {
                    actualKey = candidateKey;
                    actualValue = dictionary[candidateKey];
                    return true;
                }
            }
        }

        actualKey = default;
        actualValue = default;
        return false;
    }

    /// <summary>
    /// Creates a deterministic galaxy-star fixture for system-generation tests.
    /// </summary>
    private static GalaxyStar CreateFixtureGalaxyStar()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(909_009);
        return GalaxyStar.CreateWithDerivedProperties(new Vector3(1200.0f, 15.0f, -800.0f), 111_111, spec);
    }

    /// <summary>
    /// Creates a deterministic planet-generation context.
    /// </summary>
    private static ParentContext CreateFixturePlanetContext()
    {
        return ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters);
    }

    /// <summary>
    /// Creates a deterministic moon-generation context for a gas-giant host.
    /// </summary>
    private static ParentContext CreateFixtureMoonContext()
    {
        return ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters * 9.5,
            Units.JupiterMassKg,
            Units.JupiterRadiusMeters,
            Units.JupiterRadiusMeters * 20.0);
    }

    /// <summary>
    /// Creates a deterministic edited-body override payload for a fixture star.
    /// </summary>
    private static GalaxyBodyOverrides CreateFixtureOverrides()
    {
        GalaxyStar star = CreateFixtureGalaxyStar();
        CelestialBody body = StarGenerator.Generate(StarSpec.RedDwarf(star.StarSeed), new SeededRng(888_888));
        body.Name = "Edited Star";

        GalaxyBodyOverrides overrides = new();
        overrides.SetOverride(star.StarSeed, body);
        return overrides;
    }

    /// <summary>
    /// Creates a simple rocky planet used by the material-factory tests.
    /// </summary>
    private static CelestialBody CreateFixtureMaterialFactoryRockyBody()
    {
        CelestialBody body = new(
            "test_rocky",
            "Test Rocky",
            CelestialType.Type.Planet,
            new PhysicalProps(
                Units.EarthMassKg,
                Units.EarthRadiusMeters,
                86_400.0,
                23.5,
                0.0033,
                7.8e22,
                4.4e13),
            new Provenance(24_680, "1.0.0", 0, 0, new Godot.Collections.Dictionary()));
        body.Surface = new SurfaceProps(
            288.0,
            0.3,
            "continental",
            0.1,
            new Godot.Collections.Dictionary
            {
                ["iron"] = 0.2,
            });
        return body;
    }

    /// <summary>
    /// Creates a simple solar-type star used by the material-factory tests.
    /// </summary>
    private static CelestialBody CreateFixtureMaterialFactoryStarBody()
    {
        CelestialBody body = new(
            "test_star",
            "Test Star",
            CelestialType.Type.Star,
            new PhysicalProps(
                1.989e30,
                6.9634e8,
                2.16e6,
                7.25,
                0.0,
                0.0,
                0.0),
            new Provenance(13_579, "1.0.0", 0, 0, new Godot.Collections.Dictionary()));
        body.Stellar = new StellarProps(
            StellarProps.SolarLuminosityWatts,
            5778.0,
            "G2V",
            "main_sequence",
            1.0,
            4.6e9);
        return body;
    }

    /// <summary>
    /// Creates a deterministic single-star fixture with one planet and one belt for layout tests.
    /// </summary>
    private static SolarSystem CreateFixtureDisplayLayoutSystemWithBelt()
    {
        SolarSystem system = new("test_layout", "Layout System");

        CelestialBody star = new(
            "star_0",
            "Test Star",
            CelestialType.Type.Star,
            new PhysicalProps(Units.SolarMassKg, Units.SolarRadiusMeters),
            new Provenance(11_111, "1.0.0", 0, 0, new Godot.Collections.Dictionary()));
        CelestialBody planet = CreateFixtureMaterialFactoryRockyBody();
        planet.Id = "planet_0";
        planet.Name = "Test Planet";
        planet.Orbital = new OrbitalProps
        {
            ParentId = "node_star_0",
            SemiMajorAxisM = Units.AuMeters,
            MeanAnomalyDeg = 0.0,
        };

        AsteroidBelt belt = new("belt_0", "Test Belt")
        {
            OrbitHostId = "node_star_0",
            InnerRadiusM = 2.0 * Units.AuMeters,
            OuterRadiusM = 3.0 * Units.AuMeters,
            TotalMassKg = 1.0e21,
        };

        system.AddBody(star);
        system.AddBody(planet);
        system.AddAsteroidBelt(belt);
        system.Hierarchy = new SystemHierarchy(HierarchyNode.CreateStar("node_star_0", "star_0"));
        return system;
    }

    /// <summary>
    /// Creates a Saturn-like physical props for ring system tests.
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
            8.0e16);
    }

    /// <summary>
    /// Creates a Saturn-like parent context (at 9.5 AU from a solar-type star).
    /// </summary>
    private static ParentContext CreateSaturnContext()
    {
        return ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            9.5 * Units.AuMeters);
    }

    /// <summary>
    /// Creates an inner-zone parent context (at 1.5 AU from a solar-type star).
    /// </summary>
    private static ParentContext CreateInnerContext()
    {
        return ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            1.5 * Units.AuMeters);
    }
}
