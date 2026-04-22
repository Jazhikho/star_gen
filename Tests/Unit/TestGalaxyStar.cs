#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for galaxy-star origin context and field derivation.
/// </summary>
public static class TestGalaxyStar
{
    /// <summary>
    /// Tests that derived stars receive structured galaxy-origin context.
    /// </summary>
    public static void TestDerivedStarsReceiveGalaxyOriginContext()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(new Vector3(7000.0f, 0.0f, 0.0f), 99999, spec);

        DotNetNativeTestSuite.AssertTrue(star.OriginContext.MetallicityPrior > 0.0, "metallicity prior should be derived");
        DotNetNativeTestSuite.AssertTrue(star.OriginContext.AgeMeanGyr > 0.0, "age mean should be derived");
        DotNetNativeTestSuite.AssertTrue(star.OriginContext.GhzWeight >= 0.0 && star.OriginContext.GhzWeight <= 1.0, "GHZ weight should be normalized");
    }

    /// <summary>
    /// Tests that inner and outer stars preserve the metallicity gradient.
    /// </summary>
    public static void TestMetallicityGradientFallsWithRadius()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar innerStar = GalaxyStar.CreateWithDerivedProperties(new Vector3(2500.0f, 0.0f, 0.0f), 1, spec);
        GalaxyStar outerStar = GalaxyStar.CreateWithDerivedProperties(new Vector3(14000.0f, 0.0f, 0.0f), 2, spec);

        DotNetNativeTestSuite.AssertTrue(innerStar.Metallicity > outerStar.Metallicity, "inner disk should be more metal rich than the outer disk");
    }

    /// <summary>
    /// Tests that bulge stars skew older than arm stars.
    /// </summary>
    public static void TestBulgeStarsSkewOlderThanArmStars()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar bulgeStar = GalaxyStar.CreateWithDerivedProperties(new Vector3(300.0f, 0.0f, 0.0f), 11, spec);
        GalaxyStar armStar = GalaxyStar.CreateWithDerivedProperties(new Vector3(8000.0f, 0.0f, 0.0f), 12, spec);

        DotNetNativeTestSuite.AssertTrue(bulgeStar.OriginContext.AgeMeanGyr >= armStar.OriginContext.AgeMeanGyr, "bulge context should be older than disk or arm context");
        DotNetNativeTestSuite.AssertTrue(bulgeStar.AgeBias >= armStar.AgeBias, "legacy age bias should mirror the older bulge context");
    }

    /// <summary>
    /// Tests that cluster probability responds to star-forming regions.
    /// </summary>
    public static void TestStarFormingRegionsIncreaseClusterProbability()
    {
        GalaxyConfig irregularConfig = GalaxyConfig.CreateMilkyWay();
        irregularConfig.Type = GalaxySpec.GalaxyType.Irregular;
        irregularConfig.HaloMassLog10Solar = 10.2;
        irregularConfig.StarFormationEfficiency = 0.22;
        GalaxySpec irregularSpec = GalaxySpec.CreateFromConfig(irregularConfig, 88);

        GalaxyStar bodyStar = GalaxyStar.CreateWithDerivedProperties(new Vector3(1200.0f, 0.0f, 900.0f), 51, irregularSpec);
        GalaxyStar envelopeStar = GalaxyStar.CreateWithDerivedProperties(new Vector3(12000.0f, 0.0f, 0.0f), 52, irregularSpec);

        DotNetNativeTestSuite.AssertTrue(bodyStar.OriginContext.ClusterProbability >= envelopeStar.OriginContext.ClusterProbability, "star-forming irregular bodies should be more cluster-prone than their envelopes");
    }

    /// <summary>
    /// Tests that serialization preserves the structured origin context.
    /// </summary>
    public static void TestSerializationPreservesOriginContext()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(123);
        GalaxyStar original = GalaxyStar.CreateWithDerivedProperties(new Vector3(6200.0f, 120.0f, -800.0f), 444, spec);

        Godot.Collections.Dictionary data = original.ToDictionary();
        GalaxyStar? restored = GalaxyStar.FromDictionary(data);

        DotNetNativeTestSuite.AssertNotNull(restored, "galaxy star should deserialize");
        DotNetNativeTestSuite.AssertEqual(original.StarSeed, restored!.StarSeed, "seed should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.OriginContext.RegionKind, restored.OriginContext.RegionKind, "region should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.OriginContext.AgeCohort, restored.OriginContext.AgeCohort, "age cohort should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.OriginContext.MetallicityPrior, restored.OriginContext.MetallicityPrior, "metallicity prior should round-trip");
    }
}
