using StarGen.Domain.Population;
using StarGen.Domain.Population.StationDesign;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestDesignMapping
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestDesignMapping::test_trade_class_mapping", TestTradeClassMapping);
        runner.RunNativeTest("TestDesignMapping::test_hull_tonnage_clamped_and_rounded", TestHullTonnageClampedAndRounded);
        runner.RunNativeTest("TestDesignMapping::test_hull_tonnage_uses_explicit_small_class", TestHullTonnageUsesExplicitSmallClass);
        runner.RunNativeTest("TestDesignMapping::test_hull_configuration_rules", TestHullConfigurationRules);
    }

    public static void TestTradeClassMapping()
    {
        DotNetNativeTestSuite.AssertEqual(DesignTemplate.HighportD, DesignMapping.DeriveTemplate(StationPurpose.Purpose.Trade, StationClass.Class.O));
        DotNetNativeTestSuite.AssertEqual(DesignTemplate.HighportC, DesignMapping.DeriveTemplate(StationPurpose.Purpose.Trade, StationClass.Class.B));
        DotNetNativeTestSuite.AssertEqual(DesignTemplate.HighportB, DesignMapping.DeriveTemplate(StationPurpose.Purpose.Trade, StationClass.Class.A));
        DotNetNativeTestSuite.AssertEqual(DesignTemplate.HighportA, DesignMapping.DeriveTemplate(StationPurpose.Purpose.Trade, StationClass.Class.S));
    }

    public static void TestHullTonnageClampedAndRounded()
    {
        int hull = DesignMapping.DeriveHullTonnage(DesignTemplate.HighportB, StationClass.Class.B, 50_000);
        DotNetNativeTestSuite.AssertTrue(hull % 100 == 0, "Hull tonnage should round to the next 100 tons");
        DotNetNativeTestSuite.AssertTrue(hull >= ComponentCatalog.Templates[DesignTemplate.HighportB].MinHull);
        DotNetNativeTestSuite.AssertTrue(hull <= ComponentCatalog.Templates[DesignTemplate.HighportB].MaxHull);
    }

    public static void TestHullTonnageUsesExplicitSmallClass()
    {
        int utilityHull = DesignMapping.DeriveHullTonnage(DesignTemplate.Defense, StationClass.Class.U, 5_000);
        int outpostHull = DesignMapping.DeriveHullTonnage(DesignTemplate.Defense, StationClass.Class.O, 5_000);

        DotNetNativeTestSuite.AssertTrue(utilityHull < outpostHull,
            "Utility-sized templates should use the U span instead of collapsing to the O span");
    }

    public static void TestHullConfigurationRules()
    {
        DotNetNativeTestSuite.AssertEqual(HullConfiguration.Asteroid, DesignMapping.DeriveHullConfiguration(StationType.Type.AsteroidBelt, DesignTemplate.Mining));
        DotNetNativeTestSuite.AssertEqual(HullConfiguration.Platform, DesignMapping.DeriveHullConfiguration(StationType.Type.Lagrange, DesignTemplate.Trade));
        DotNetNativeTestSuite.AssertEqual(HullConfiguration.Platform, DesignMapping.DeriveHullConfiguration(StationType.Type.DeepSpace, DesignTemplate.Defense));
        DotNetNativeTestSuite.AssertEqual(HullConfiguration.Ring, DesignMapping.DeriveHullConfiguration(StationType.Type.Orbital, DesignTemplate.HighportB));
        DotNetNativeTestSuite.AssertEqual(HullConfiguration.Cylinder, DesignMapping.DeriveHullConfiguration(StationType.Type.Orbital, DesignTemplate.Research));
    }
}
