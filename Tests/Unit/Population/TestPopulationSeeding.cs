#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for PopulationSeeding deterministic seed generation.
/// </summary>
public static class TestPopulationSeeding
{
    /// <summary>
    /// Tests that the same body_id + base_seed always produces the same seed.
    /// </summary>
    public static void TestDeterminismSameInputsSameOutput()
    {
        long seed1 = PopulationSeeding.GeneratePopulationSeed("planet_abc", 42);
        long seed2 = PopulationSeeding.GeneratePopulationSeed("planet_abc", 42);
        DotNetNativeTestSuite.AssertEqual(seed1, seed2, "Same inputs should produce same seed");
    }

    /// <summary>
    /// Tests that different body_ids produce different seeds.
    /// </summary>
    public static void TestDifferentBodyIdsDifferentSeeds()
    {
        long seed1 = PopulationSeeding.GeneratePopulationSeed("planet_abc", 42);
        long seed2 = PopulationSeeding.GeneratePopulationSeed("planet_def", 42);
        DotNetNativeTestSuite.AssertNotEqual(seed1, seed2, "Different body_ids should produce different seeds");
    }

    /// <summary>
    /// Tests that different base seeds produce different seeds.
    /// </summary>
    public static void TestDifferentBaseSeedsDifferentSeeds()
    {
        long seed1 = PopulationSeeding.GeneratePopulationSeed("planet_abc", 42);
        long seed2 = PopulationSeeding.GeneratePopulationSeed("planet_abc", 43);
        DotNetNativeTestSuite.AssertNotEqual(seed1, seed2, "Different base seeds should produce different seeds");
    }

    /// <summary>
    /// Tests that seeds are positive (non-negative).
    /// </summary>
    public static void TestSeedsArePositive()
    {
        for (int i = 0; i < 20; i++)
        {
            string bodyId = $"body_{i}";
            long seedVal = PopulationSeeding.GeneratePopulationSeed(bodyId, i * 100);
            DotNetNativeTestSuite.AssertTrue(seedVal >= 0, $"Seed should be non-negative for body '{bodyId}'");
        }
    }

    /// <summary>
    /// Tests order independence: generating seeds in different order gives same results.
    /// </summary>
    public static void TestOrderIndependence()
    {
        string[] ids = { "planet_01", "planet_02", "planet_03", "moon_01" };
        long baseValue = 12345;

        System.Collections.Generic.Dictionary<string, long> forwardSeeds = new();
        for (int i = 0; i < ids.Length; i++)
        {
            forwardSeeds[ids[i]] = PopulationSeeding.GeneratePopulationSeed(ids[i], baseValue);
        }

        System.Collections.Generic.Dictionary<string, long> reverseSeeds = new();
        for (int i = ids.Length - 1; i >= 0; i--)
        {
            reverseSeeds[ids[i]] = PopulationSeeding.GeneratePopulationSeed(ids[i], baseValue);
        }

        for (int i = 0; i < ids.Length; i++)
        {
            DotNetNativeTestSuite.AssertEqual(forwardSeeds[ids[i]], reverseSeeds[ids[i]], $"Seed for '{ids[i]}' should be same regardless of generation order");
        }
    }

    /// <summary>
    /// Tests that native sub-seeds are deterministic.
    /// </summary>
    public static void TestNativeSeedDeterminism()
    {
        long popSeed = 99999;
        long seed1 = PopulationSeeding.GenerateNativeSeed(popSeed, 0);
        long seed2 = PopulationSeeding.GenerateNativeSeed(popSeed, 0);
        DotNetNativeTestSuite.AssertEqual(seed1, seed2, "Same population seed + index should give same native seed");
    }

    /// <summary>
    /// Tests that different native indices produce different seeds.
    /// </summary>
    public static void TestNativeSeedsDifferByIndex()
    {
        long popSeed = 99999;
        long seed0 = PopulationSeeding.GenerateNativeSeed(popSeed, 0);
        long seed1 = PopulationSeeding.GenerateNativeSeed(popSeed, 1);
        DotNetNativeTestSuite.AssertNotEqual(seed0, seed1, "Different native indices should produce different seeds");
    }

    /// <summary>
    /// Tests that colony sub-seeds differ from native sub-seeds.
    /// </summary>
    public static void TestColonySeedsDifferFromNativeSeeds()
    {
        long popSeed = 99999;
        long nativeSeed = PopulationSeeding.GenerateNativeSeed(popSeed, 0);
        long colonySeed = PopulationSeeding.GenerateColonySeed(popSeed, 0);
        DotNetNativeTestSuite.AssertNotEqual(nativeSeed, colonySeed, "Colony and native seeds for same index should differ");
    }
}
