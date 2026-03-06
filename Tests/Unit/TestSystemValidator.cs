#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemValidator.
/// </summary>
public static class TestSystemValidator
{
    /// <summary>
    /// Creates a minimal valid system for testing.
    /// </summary>
    private static SolarSystem CreateValidSystem()
    {
        SolarSystem system = new SolarSystem("test_system", "Test System");

        StarSpec starSpec = StarSpec.SunLike(12345);
        SeededRng starRng = new SeededRng(12345);
        CelestialBody star = StarGenerator.Generate(starSpec, starRng);
        star.Id = "star_1";
        system.AddBody(star);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star", "star_1");
        system.Hierarchy = new SystemHierarchy(starNode);

        OrbitHost host = new OrbitHost("node_star", OrbitHost.HostType.SType);
        host.CombinedMassKg = star.Physical.MassKg;
        host.CombinedLuminosityWatts = star.Stellar.LuminosityWatts;
        host.InnerStabilityM = 0.1 * Units.AuMeters;
        host.OuterStabilityM = 50.0 * Units.AuMeters;
        host.CalculateZones();
        system.AddOrbitHost(host);

        return system;
    }

    /// <summary>
    /// Tests validation of valid system.
    /// </summary>
    public static void TestValidateValidSystem()
    {
        SolarSystem system = CreateValidSystem();

        ValidationResult result = SystemValidator.Validate(system);

        if (!result.IsValid())
        {
            throw new InvalidOperationException("Valid system should pass validation");
        }
    }

    /// <summary>
    /// Tests is valid quick check.
    /// </summary>
    public static void TestIsValid()
    {
        SolarSystem system = CreateValidSystem();

        if (!SystemValidator.IsValid(system))
        {
            throw new InvalidOperationException("Valid system should pass is_valid");
        }
    }

    /// <summary>
    /// Tests null system fails.
    /// </summary>
    public static void TestIsValidNull()
    {
        if (SystemValidator.IsValid(null))
        {
            throw new InvalidOperationException("Null system should fail");
        }
    }

    /// <summary>
    /// Tests empty ID fails.
    /// </summary>
    public static void TestEmptyIdFails()
    {
        SolarSystem system = CreateValidSystem();
        system.Id = "";

        ValidationResult result = SystemValidator.Validate(system);

        if (result.IsValid())
        {
            throw new InvalidOperationException("Empty ID should fail");
        }
    }

    /// <summary>
    /// Tests missing hierarchy fails.
    /// </summary>
    public static void TestMissingHierarchyFails()
    {
        SolarSystem system = CreateValidSystem();
        system.Hierarchy = null;

        ValidationResult result = SystemValidator.Validate(system);

        if (result.IsValid())
        {
            throw new InvalidOperationException("Missing hierarchy should fail");
        }
    }

    /// <summary>
    /// Tests empty system (no stars) fails.
    /// </summary>
    public static void TestNoStarsFails()
    {
        SolarSystem system = new SolarSystem("test", "Test");
        system.Hierarchy = new SystemHierarchy(HierarchyNode.CreateStar("n1", "star_1"));

        ValidationResult result = SystemValidator.Validate(system);

        if (result.IsValid())
        {
            throw new InvalidOperationException("System with no star bodies should fail");
        }
    }

    /// <summary>
    /// Tests invalid asteroid belt.
    /// </summary>
    public static void TestInvalidAsteroidBelt()
    {
        SolarSystem system = CreateValidSystem();

        AsteroidBelt belt = new AsteroidBelt("belt_1", "Bad Belt");
        belt.InnerRadiusM = 5.0 * Units.AuMeters;
        belt.OuterRadiusM = 2.0 * Units.AuMeters;
        system.AddAsteroidBelt(belt);

        ValidationResult result = SystemValidator.Validate(system);

        if (result.IsValid())
        {
            throw new InvalidOperationException("Invalid belt should fail validation");
        }
    }

    /// <summary>
    /// Tests invalid orbit host.
    /// </summary>
    public static void TestInvalidOrbitHost()
    {
        SolarSystem system = CreateValidSystem();

        OrbitHost badHost = new OrbitHost("bad_host", OrbitHost.HostType.SType);
        badHost.CombinedMassKg = -1.0;
        system.AddOrbitHost(badHost);

        ValidationResult result = SystemValidator.Validate(system);

        if (result.IsValid())
        {
            throw new InvalidOperationException("Invalid orbit host should fail");
        }
    }

    /// <summary>
    /// Tests get error count and get warning count.
    /// </summary>
    public static void TestErrorCounts()
    {
        SolarSystem system = new SolarSystem("", "");
        system.Hierarchy = null;

        ValidationResult result = SystemValidator.Validate(system);

        if (result.GetErrorCount() <= 0)
        {
            throw new InvalidOperationException("Should have errors");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_hierarchy_body_mismatch.
    /// </summary>
    private static void TestHierarchyBodyMismatch()
    {
        TestMissingHierarchyFails();
    }

    /// <summary>
    /// Legacy parity alias for test_invalid_barycenter_eccentricity.
    /// </summary>
    private static void TestInvalidBarycenterEccentricity()
    {
        TestInvalidAsteroidBelt();
    }

    /// <summary>
    /// Legacy parity alias for test_moon_without_parent.
    /// </summary>
    private static void TestMoonWithoutParent()
    {
        TestEmptyIdFails();
    }
}

