#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.Rendering;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for MaterialFactory.
/// </summary>
public static class TestMaterialFactory
{
    /// <summary>
    /// Creates a basic rocky body suitable for material tests.
    /// </summary>
    private static CelestialBody CreateRockyBody()
    {
        PhysicalProps physical = new PhysicalProps(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            86400.0,
            23.5,
            0.0033,
            7.8e22,
            4.4e13
        );
        CelestialBody body = new CelestialBody(
            "test_rocky",
            "Test Rocky",
            CelestialType.Type.Planet,
            physical,
            new Provenance(24680, "1.0.0", 0, 0, new Dictionary())
        );
        body.Surface = new SurfaceProps(288.0, 0.3, "continental", 0.1, new Dictionary { { "iron", 0.2 } });
        return body;
    }

    /// <summary>
    /// Creates a basic stellar body suitable for material tests.
    /// </summary>
    private static CelestialBody CreateStarBody()
    {
        PhysicalProps physical = new PhysicalProps(
            1.989e30,
            6.9634e8,
            2.16e6,
            7.25,
            0.0,
            0.0,
            0.0
        );
        CelestialBody body = new CelestialBody(
            "test_star",
            "Test Star",
            CelestialType.Type.Star,
            physical,
            new Provenance(13579, "1.0.0", 0, 0, new Dictionary())
        );
        body.Stellar = new StellarProps(
            StellarProps.SolarLuminosityWatts,
            5778.0,
            "G2V",
            "main_sequence",
            1.0,
            4.6e9
        );
        return body;
    }

    /// <summary>
    /// Ensures null bodies fall back to the default material.
    /// </summary>
    public static void TestNullBodyReturnsDefaultMaterial()
    {
        MaterialFactory.ClearCache();
        Material material = MaterialFactory.CreateBodyMaterial(null);

        DotNetNativeTestSuite.AssertNotNull(material, "Null bodies should still return a material");
        if (!(material is StandardMaterial3D))
        {
            throw new InvalidOperationException("Null bodies should use the default standard material");
        }
        MaterialFactory.ClearCache();
    }

    /// <summary>
    /// Ensures repeated material requests for the same body reuse the cache.
    /// </summary>
    public static void TestSameBodyReusesCachedMaterial()
    {
        MaterialFactory.ClearCache();
        CelestialBody body = CreateRockyBody();
        Material firstMaterial = MaterialFactory.CreateBodyMaterial(body);
        Material secondMaterial = MaterialFactory.CreateBodyMaterial(body);

        DotNetNativeTestSuite.AssertNotNull(firstMaterial, "First material should be created");
        DotNetNativeTestSuite.AssertEqual(firstMaterial, secondMaterial, "Same body should reuse the cached material");
        MaterialFactory.ClearCache();
    }

    /// <summary>
    /// Ensures star bodies use the star shader material path.
    /// </summary>
    public static void TestStarBodyUsesStarShaderMaterial()
    {
        MaterialFactory.ClearCache();
        CelestialBody body = CreateStarBody();
        Material material = MaterialFactory.CreateBodyMaterial(body);
        ShaderMaterial shaderMaterial = material as ShaderMaterial;

        if (!(material is ShaderMaterial))
        {
            throw new InvalidOperationException("Stars should use a shader material");
        }
        DotNetNativeTestSuite.AssertNotNull(shaderMaterial, "Star material should cast to ShaderMaterial");
        DotNetNativeTestSuite.AssertNotNull(shaderMaterial.Shader, "Star material should have a shader assigned");
        DotNetNativeTestSuite.AssertFloatNear(
            5778.0,
            (double)shaderMaterial.GetShaderParameter("u_temperature"),
            0.01,
            "Star shader should receive the stellar temperature"
        );
        MaterialFactory.ClearCache();
    }
}
