using StarGen.Domain.Population.StationDesign;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestHullCalculator
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestHullCalculator::test_cost_per_ton_tier_boundaries", TestCostPerTonTierBoundaries);
        runner.RunNativeTest("TestHullCalculator::test_hull_cost_applies_config_multiplier", TestHullCostAppliesConfigMultiplier);
        runner.RunNativeTest("TestHullCalculator::test_structure_hit_points", TestStructureHitPoints);
        runner.RunNativeTest("TestHullCalculator::test_hardpoints", TestHardpoints);
        runner.RunNativeTest("TestHullCalculator::test_armor_tonnage_none_is_zero", TestArmorTonnageNoneIsZero);
        runner.RunNativeTest("TestHullCalculator::test_command_center_minimum_20_tons", TestCommandCenterMinimum20Tons);
        runner.RunNativeTest("TestHullCalculator::test_effective_armor_config_modifier", TestEffectiveArmorConfigModifier);
    }

    public static void TestCostPerTonTierBoundaries()
    {
        DotNetNativeTestSuite.AssertEqual(50_000L, HullCalculator.CostPerTon(2_000));
        DotNetNativeTestSuite.AssertEqual(40_000L, HullCalculator.CostPerTon(2_001));
        DotNetNativeTestSuite.AssertEqual(40_000L, HullCalculator.CostPerTon(10_000));
        DotNetNativeTestSuite.AssertEqual(30_000L, HullCalculator.CostPerTon(10_001));
        DotNetNativeTestSuite.AssertEqual(30_000L, HullCalculator.CostPerTon(100_000));
        DotNetNativeTestSuite.AssertEqual(25_000L, HullCalculator.CostPerTon(100_001));
    }

    public static void TestHullCostAppliesConfigMultiplier()
    {
        long sphereCost = HullCalculator.HullCost(10_000, HullConfiguration.Sphere);
        long asteroidCost = HullCalculator.HullCost(10_000, HullConfiguration.Asteroid);
        DotNetNativeTestSuite.AssertTrue(asteroidCost < sphereCost, "Asteroid should be cheaper than sphere");
        DotNetNativeTestSuite.AssertEqual(400_000_000L, sphereCost);
    }

    public static void TestStructureHitPoints()
    {
        DotNetNativeTestSuite.AssertEqual(1, HullCalculator.StructureHitPoints(50));
        DotNetNativeTestSuite.AssertEqual(2, HullCalculator.StructureHitPoints(100));
        DotNetNativeTestSuite.AssertEqual(1000, HullCalculator.StructureHitPoints(50_000));
    }

    public static void TestHardpoints()
    {
        DotNetNativeTestSuite.AssertEqual(1, HullCalculator.Hardpoints(100));
        DotNetNativeTestSuite.AssertEqual(500, HullCalculator.Hardpoints(50_000));
        DotNetNativeTestSuite.AssertEqual(0, HullCalculator.Hardpoints(99));
    }

    public static void TestArmorTonnageNoneIsZero()
    {
        int tons = HullCalculator.ArmorTonnage(50_000, ArmorMaterial.None, 5, HullConfiguration.Sphere);
        DotNetNativeTestSuite.AssertEqual(0, tons);
    }

    public static void TestCommandCenterMinimum20Tons()
    {
        int tons = HullCalculator.CommandTonnage(100, CommandCenterKind.Standard);
        DotNetNativeTestSuite.AssertEqual(20, tons, "Tiny hull should still get 20 tons of command space");
    }

    public static void TestEffectiveArmorConfigModifier()
    {
        int spherePoints = HullCalculator.EffectiveArmorPoints(ArmorMaterial.Crystaliron, 5, HullConfiguration.Sphere);
        int asteroidPoints = HullCalculator.EffectiveArmorPoints(ArmorMaterial.Crystaliron, 5, HullConfiguration.Asteroid);
        DotNetNativeTestSuite.AssertEqual(5, spherePoints);
        DotNetNativeTestSuite.AssertEqual(7, asteroidPoints, "Asteroid armor multiplier should increase effective armor");
    }
}
