using StarGen.Domain.Population.StationDesign;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestBudgetFitter
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestBudgetFitter::test_under_budget_no_scaling", TestUnderBudgetNoScaling);
        runner.RunNativeTest("TestBudgetFitter::test_over_budget_scales_down", TestOverBudgetScalesDown);
        runner.RunNativeTest("TestBudgetFitter::test_manual_sections_untouched", TestManualSectionsUntouched);
        runner.RunNativeTest("TestBudgetFitter::test_scale_respects_floor", TestScaleRespectsFloor);
    }

    public static void TestUnderBudgetNoScaling()
    {
        ComponentSelection selection = MakeSelectionWithFacilities(1);
        double scale = BudgetFitter.Fit(selection, 50_000, AutoPopulateFlags.AllAuto, 1000, 100, 100, 100);
        DotNetNativeTestSuite.AssertEqual(1.0, scale);
        DotNetNativeTestSuite.AssertEqual(1, selection.Facilities[FacilityKind.Warehouse]);
    }

    public static void TestOverBudgetScalesDown()
    {
        ComponentSelection selection = MakeSelectionWithFacilities(50);
        double scale = BudgetFitter.Fit(selection, 10_000, AutoPopulateFlags.AllAuto, 500, 100, 100, 100);
        DotNetNativeTestSuite.AssertTrue(scale < 1.0, "Over budget should scale");
        DotNetNativeTestSuite.AssertTrue(selection.Facilities[FacilityKind.Warehouse] < 50, "Facility count should drop");
    }

    public static void TestManualSectionsUntouched()
    {
        ComponentSelection selection = MakeSelectionWithFacilities(50);
        AutoPopulateFlags flags = AutoPopulateFlags.Engineering
            | AutoPopulateFlags.Command
            | AutoPopulateFlags.Defenses
            | AutoPopulateFlags.Docking
            | AutoPopulateFlags.Quarters;

        BudgetFitter.Fit(selection, 10_000, flags, 500, 100, 100, 100);
        DotNetNativeTestSuite.AssertEqual(50, selection.Facilities[FacilityKind.Warehouse], "Manual facilities should not scale");
    }

    public static void TestScaleRespectsFloor()
    {
        ComponentSelection selection = MakeSelectionWithFacilities(1000);
        double scale = BudgetFitter.Fit(selection, 1_000, AutoPopulateFlags.AllAuto, 50, 20, 10, 10);
        DotNetNativeTestSuite.AssertTrue(scale >= BudgetFitter.MinScaleFactor, "Scale should respect minimum floor");
    }

    private static ComponentSelection MakeSelectionWithFacilities(int warehouseCount)
    {
        ComponentSelection selection = new();
        selection.Facilities[FacilityKind.Warehouse] = warehouseCount;
        return selection;
    }
}
