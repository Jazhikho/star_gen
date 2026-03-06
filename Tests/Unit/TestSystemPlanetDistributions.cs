#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Statistical tests for system-scale planet distributions.
/// </summary>
public static class TestSystemPlanetDistributions
{
    /// <summary>
    /// Ensures close-in gas giants remain uncommon across generated systems.
    /// </summary>
    public static void TestHotJupiterFractionRare()
    {
        GenerationStatsHarness.PlanetDistributionStats stats = GenerationStatsHarness.SampleSystemPlanetStats(50000, 200);
        DotNetNativeTestSuite.AssertGreaterThan(stats.TotalPlanets, 0, "Ensemble sampling must produce some planets");

        double fraction = (double)stats.HotJupiters / (double)stats.TotalPlanets;
        DotNetNativeTestSuite.AssertLessThan(
            fraction,
            ScientificBenchmarks.HotJupiterFractionMax,
            "Hot Jupiters should remain below the research benchmark maximum");
    }

    /// <summary>
    /// Ensures outer orbits preferentially contain large planets compared with inner orbits.
    /// </summary>
    public static void TestColdZonePrefersLargePlanets()
    {
        GenerationStatsHarness.PlanetDistributionStats stats = GenerationStatsHarness.SampleSystemPlanetStats(60000, 150);
        DotNetNativeTestSuite.AssertGreaterThan(stats.InnerTotal, 0, "Inner region should contain planets");
        DotNetNativeTestSuite.AssertGreaterThan(stats.OuterTotal, 0, "Outer region should contain planets");

        double innerLargeFraction = (double)stats.InnerLarge / (double)stats.InnerTotal;
        double outerLargeFraction = (double)stats.OuterLarge / (double)stats.OuterTotal;

        DotNetNativeTestSuite.AssertGreaterThan(
            outerLargeFraction,
            innerLargeFraction,
            "Large planets should be more common in outer orbits");
    }
}

