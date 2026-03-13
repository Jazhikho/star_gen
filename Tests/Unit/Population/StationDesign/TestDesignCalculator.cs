using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Classification;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestDesignCalculator
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestDesignCalculator::test_highport_b_50k_determinism", TestHighportB50kDeterminism);
        runner.RunNativeTest("TestDesignCalculator::test_highport_b_50k_basic_invariants", TestHighportB50kBasicInvariants);
        runner.RunNativeTest("TestDesignCalculator::test_naval_base_has_correct_presets", TestNavalBaseHasCorrectPresets);
        runner.RunNativeTest("TestDesignCalculator::test_custom_template_minimal", TestCustomTemplateMinimal);
        runner.RunNativeTest("TestDesignCalculator::test_defense_platform_high_weapons", TestDefensePlatformHighWeapons);
        runner.RunNativeTest("TestDesignCalculator::test_waystation_classification", TestWaystationClassification);
        runner.RunNativeTest("TestDesignCalculator::test_highport_a_classification", TestHighportAClassification);
    }

    public static void TestHighportB50kDeterminism()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.HighportB,
            HullTons = 50_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        DesignResult first = DesignCalculator.Calculate(spec);
        DesignResult second = DesignCalculator.Calculate(spec);

        DotNetNativeTestSuite.AssertEqual(first.Tonnage.Used, second.Tonnage.Used, "Tonnage should be identical");
        DotNetNativeTestSuite.AssertEqual(first.Cost.Total, second.Cost.Total, "Cost should be identical");
        DotNetNativeTestSuite.AssertEqual(first.Crew.Total, second.Crew.Total, "Crew should be identical");
        DotNetNativeTestSuite.AssertEqual(first.Power.Demand, second.Power.Demand, "Power demand should be identical");
        DotNetNativeTestSuite.AssertEqual(first.CatalogVersion, second.CatalogVersion, "Catalog version should be identical");
    }

    public static void TestHighportB50kBasicInvariants()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.HighportB,
            HullTons = 50_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        DesignResult result = DesignCalculator.Calculate(spec);

        DotNetNativeTestSuite.AssertTrue(result.Tonnage.Used <= 50_000, "Should not exceed hull tonnage");
        DotNetNativeTestSuite.AssertTrue(result.Tonnage.Cargo >= 0, "Cargo should be non-negative");
        DotNetNativeTestSuite.AssertTrue(result.Power.Surplus >= 0, "Power should not be deficit");
        DotNetNativeTestSuite.AssertTrue(result.Crew.Total > 0, "Should have crew");
        DotNetNativeTestSuite.AssertTrue(result.BerthsAvailable >= result.Crew.Total, "Berths should cover crew");
        DotNetNativeTestSuite.AssertEqual(1000, result.StructureHitPoints);
        DotNetNativeTestSuite.AssertEqual(500, result.Hardpoints);
        DotNetNativeTestSuite.AssertEqual("1.1.0", result.CatalogVersion);
    }

    public static void TestNavalBaseHasCorrectPresets()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.Naval,
            HullTons = 200_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        DesignResult result = DesignCalculator.Calculate(spec);
        DotNetNativeTestSuite.AssertEqual(CommandCenterKind.Military, result.Selection.CommandCenter);
        DotNetNativeTestSuite.AssertEqual(SensorGrade.Advanced, result.Selection.Sensors);
        DotNetNativeTestSuite.AssertEqual(35, result.Selection.ComputerRating);
        DotNetNativeTestSuite.AssertEqual(ArmorMaterial.BondedSuperdense, result.Selection.ArmorMaterial);
    }

    public static void TestCustomTemplateMinimal()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.Custom,
            HullTons = 1_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        DesignResult result = DesignCalculator.Calculate(spec);
        DotNetNativeTestSuite.AssertEqual(ArmorMaterial.None, result.Selection.ArmorMaterial);
        DotNetNativeTestSuite.AssertEqual(0, result.EffectiveArmorPoints);
        DotNetNativeTestSuite.AssertTrue(result.Tonnage.Used <= 1_000, "Should fit in hull");
    }

    public static void TestDefensePlatformHighWeapons()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.Defense,
            HullTons = 20_000,
            Configuration = HullConfiguration.Platform,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        DesignResult result = DesignCalculator.Calculate(spec);
        DotNetNativeTestSuite.AssertTrue(result.HardpointsUsed > 0, "Should have turrets");
        DotNetNativeTestSuite.AssertTrue(result.Selection.Screens[DefensiveScreen.NuclearDamper] >= 1, "Defense should have dampers");
        ClassificationReport report = ClassificationEvaluator.Evaluate(result);
        DotNetNativeTestSuite.AssertTrue(report.Results[ClassificationId.DefensePlatform].Earned, "Defense template should earn its classification");
    }

    public static void TestWaystationClassification()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.Waystation,
            HullTons = 5_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        ClassificationReport report = ClassificationEvaluator.Evaluate(DesignCalculator.Calculate(spec));
        DotNetNativeTestSuite.AssertTrue(report.Results[ClassificationId.Waystation].Earned, "Waystation template should earn Waystation");
    }

    public static void TestHighportAClassification()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.HighportA,
            HullTons = 200_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        ClassificationReport report = ClassificationEvaluator.Evaluate(DesignCalculator.Calculate(spec));
        DotNetNativeTestSuite.AssertTrue(report.Results[ClassificationId.StarportA].Earned, "Highport A should earn Class A Starport");
        DotNetNativeTestSuite.AssertTrue(report.Results[ClassificationId.StarportB].Earned, "Class A should also satisfy Class B");
    }
}
