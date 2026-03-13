using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Classification;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestClassificationEvaluator
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestClassificationEvaluator::test_waystation_minimal", TestWaystationMinimal);
        runner.RunNativeTest("TestClassificationEvaluator::test_waystation_missing_one_req", TestWaystationMissingOneReq);
        runner.RunNativeTest("TestClassificationEvaluator::test_naval_requires_military_command", TestNavalRequiresMilitaryCommand);
        runner.RunNativeTest("TestClassificationEvaluator::test_defense_platform_full_design", TestDefensePlatformFullDesign);
        runner.RunNativeTest("TestClassificationEvaluator::test_research_requires_computer_15", TestResearchRequiresComputer15);
    }

    public static void TestWaystationMinimal()
    {
        ClassificationContext context = MakeContext();
        context.Facilities[FacilityKind.FuelDepot] = 2;
        context.Docking[DockingBerthKind.StandardBerth] = 2;
        ClassificationReport report = ClassificationEvaluator.Evaluate(context);
        DotNetNativeTestSuite.AssertTrue(report.Results[ClassificationId.Waystation].Earned, "Should earn Waystation");
    }

    public static void TestWaystationMissingOneReq()
    {
        ClassificationContext context = MakeContext();
        context.Facilities[FacilityKind.FuelDepot] = 1;
        context.Docking[DockingBerthKind.StandardBerth] = 2;
        ClassificationReport report = ClassificationEvaluator.Evaluate(context);
        DotNetNativeTestSuite.AssertTrue(!report.Results[ClassificationId.Waystation].Earned, "Should not earn with one depot");
        DotNetNativeTestSuite.AssertEqual(1, report.Results[ClassificationId.Waystation].MetCount);
    }

    public static void TestNavalRequiresMilitaryCommand()
    {
        ClassificationContext context = MakeContext();
        context.CommandCenter = CommandCenterKind.Standard;
        context.Sensors = SensorGrade.Advanced;
        context.Facilities[FacilityKind.CombatInfoCenter] = 1;
        context.Facilities[FacilityKind.Armory] = 1;
        context.Facilities[FacilityKind.Training] = 1;
        context.Hardpoints = 100;
        context.HardpointsUsed = 25;
        context.Docking[DockingBerthKind.StandardBerth] = 6;
        ClassificationReport report = ClassificationEvaluator.Evaluate(context);
        DotNetNativeTestSuite.AssertTrue(!report.Results[ClassificationId.NavalBase].Earned, "Standard command should fail naval base");
    }

    public static void TestDefensePlatformFullDesign()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.Defense,
            HullTons = 20_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        DesignResult result = DesignCalculator.Calculate(spec);
        ClassificationReport report = ClassificationEvaluator.Evaluate(result);
        DotNetNativeTestSuite.AssertTrue(report.Results[ClassificationId.DefensePlatform].Earned, "Defense template should earn defense platform");
    }

    public static void TestResearchRequiresComputer15()
    {
        ClassificationContext context = MakeContext();
        context.ComputerRating = 10;
        context.Facilities[FacilityKind.Laboratory] = 2;
        context.Facilities[FacilityKind.CommsArray] = 1;
        ClassificationReport report = ClassificationEvaluator.Evaluate(context);
        DotNetNativeTestSuite.AssertTrue(!report.Results[ClassificationId.ResearchStation].Earned, "Computer/10 should not satisfy Research Station");

        context.ComputerRating = 15;
        report = ClassificationEvaluator.Evaluate(context);
        DotNetNativeTestSuite.AssertTrue(report.Results[ClassificationId.ResearchStation].Earned, "Computer/15 should satisfy Research Station");
    }

    private static ClassificationContext MakeContext()
    {
        return new ClassificationContext
        {
            Facilities = new ComponentCounts<FacilityKind>(),
            Docking = new ComponentCounts<DockingBerthKind>(),
            Screens = new ComponentCounts<DefensiveScreen>(),
            Sensors = SensorGrade.Basic,
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 10,
            Hardpoints = 10,
            HardpointsUsed = 0,
        };
    }
}
