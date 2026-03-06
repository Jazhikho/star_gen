#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for StarSystemPreview.
/// </summary>
public static class TestStarSystemPreview
{
    /// <summary>
    /// Tests generate returns null for zero seed.
    /// </summary>
    public static void TestGenerateReturnsNullForZeroSeed()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            0, new Vector3(100.0f, 0.0f, 0.0f), galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNull(result, "Zero seed should return null");
    }

    /// <summary>
    /// Tests generate returns null for null spec.
    /// </summary>
    public static void TestGenerateReturnsNullForNullSpec()
    {
        StarSystemPreviewData result = StarSystemPreview.Generate(
            12345, new Vector3(100.0f, 0.0f, 0.0f), null
        );
        DotNetNativeTestSuite.AssertNull(result, "Null spec should return null");
    }

    /// <summary>
    /// Tests generate returns preview data.
    /// </summary>
    public static void TestGenerateReturnsPreviewData()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            99999, new Vector3(8000.0f, 0.0f, 0.0f), galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNotNull(result, "Valid inputs should produce PreviewData");
    }

    /// <summary>
    /// Tests generate seeds match.
    /// </summary>
    public static void TestGenerateSeedsMatch()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        int seedValue = 77777;
        Vector3 pos = new Vector3(8000.0f, 0.0f, 0.0f);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            seedValue, pos, galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNotNull(result, "Should produce data");
        DotNetNativeTestSuite.AssertEqual(seedValue, result.StarSeed, "star_seed should match input");
    }

    /// <summary>
    /// Tests generate caches system.
    /// </summary>
    public static void TestGenerateCachesSystem()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            55555, new Vector3(8000.0f, 0.0f, 0.0f), galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNotNull(result, "Should produce data");
        DotNetNativeTestSuite.AssertNotNull(result.System, "Cached system should not be null");
        if (!result.System.IsValid())
        {
            throw new InvalidOperationException("Cached system should be valid");
        }
    }

    /// <summary>
    /// Tests generate star count at least one.
    /// </summary>
    public static void TestGenerateStarCountAtLeastOne()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            11111, new Vector3(8000.0f, 0.0f, 0.0f), galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNotNull(result, "Should produce data");
        if (result.StarCount < 1)
        {
            throw new InvalidOperationException("System must have at least one star");
        }
    }

    /// <summary>
    /// Tests generate spectral classes match star count.
    /// </summary>
    public static void TestGenerateSpectralClassesMatchStarCount()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            22222, new Vector3(8000.0f, 0.0f, 0.0f), galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNotNull(result, "Should produce data");
        DotNetNativeTestSuite.AssertEqual(
            result.StarCount,
            result.SpectralClasses.Length,
            "Spectral classes array length should equal star_count"
        );
    }

    /// <summary>
    /// Tests generate temperatures match star count.
    /// </summary>
    public static void TestGenerateTemperaturesMatchStarCount()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            33333, new Vector3(8000.0f, 0.0f, 0.0f), galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNotNull(result, "Should produce data");
        DotNetNativeTestSuite.AssertEqual(
            result.StarCount,
            result.StarTemperatures.Length,
            "Temperatures array length should equal star_count"
        );
    }

    /// <summary>
    /// Tests generate is deterministic.
    /// </summary>
    public static void TestGenerateIsDeterministic()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        Vector3 pos = new Vector3(8000.0f, 50.0f, 200.0f);
        int seedValue = 44444;

        StarSystemPreviewData resultA = StarSystemPreview.Generate(
            seedValue, pos, galaxy.Spec
        );
        StarSystemPreviewData resultB = StarSystemPreview.Generate(
            seedValue, pos, galaxy.Spec
        );

        DotNetNativeTestSuite.AssertNotNull(resultA, "First call should produce data");
        DotNetNativeTestSuite.AssertNotNull(resultB, "Second call should produce data");
        DotNetNativeTestSuite.AssertEqual(resultA.StarCount, resultB.StarCount, "Star count must be deterministic");
        DotNetNativeTestSuite.AssertEqual(resultA.PlanetCount, resultB.PlanetCount, "Planet count must be deterministic");
        DotNetNativeTestSuite.AssertEqual(resultA.BeltCount, resultB.BeltCount, "Belt count must be deterministic");
    }

    /// <summary>
    /// Tests generate metallicity positive.
    /// </summary>
    public static void TestGenerateMetallicityPositive()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        Galaxy galaxy = new Galaxy(config, 42);
        StarSystemPreviewData result = StarSystemPreview.Generate(
            66666, new Vector3(8000.0f, 0.0f, 0.0f), galaxy.Spec
        );
        DotNetNativeTestSuite.AssertNotNull(result, "Should produce data");
        if (result.Metallicity <= 0.0)
        {
            throw new InvalidOperationException("Metallicity must be positive");
        }
    }
}
