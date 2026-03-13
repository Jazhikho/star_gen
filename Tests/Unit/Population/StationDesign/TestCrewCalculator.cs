using StarGen.Domain.Population.StationDesign;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestCrewCalculator
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestCrewCalculator::test_command_crew_minimum_two", TestCommandCrewMinimumTwo);
        runner.RunNativeTest("TestCrewCalculator::test_engineering_scales_with_plant", TestEngineeringScalesWithPlant);
        runner.RunNativeTest("TestCrewCalculator::test_gunnery_crew_bays_need_four", TestGunneryCrewBaysNeedFour);
        runner.RunNativeTest("TestCrewCalculator::test_docking_standard_needs_two_crew", TestDockingStandardNeedsTwoCrew);
        runner.RunNativeTest("TestCrewCalculator::test_facilities_crew_scales", TestFacilitiesCrewScales);
        runner.RunNativeTest("TestCrewCalculator::test_admin_is_one_twentieth", TestAdminIsOneTwentieth);
    }

    public static void TestCommandCrewMinimumTwo()
    {
        DotNetNativeTestSuite.AssertEqual(2, CrewCalculator.CommandCrew(100));
    }

    public static void TestEngineeringScalesWithPlant()
    {
        DotNetNativeTestSuite.AssertEqual(1, CrewCalculator.EngineeringCrew(500, 0));
        DotNetNativeTestSuite.AssertEqual(2, CrewCalculator.EngineeringCrew(1_500, 0));
        DotNetNativeTestSuite.AssertEqual(4, CrewCalculator.EngineeringCrew(1_500, 6_000));
    }

    public static void TestGunneryCrewBaysNeedFour()
    {
        ComponentCounts<BayWeapon> bays = new();
        bays[BayWeapon.Missile50] = 3;
        DotNetNativeTestSuite.AssertEqual(12, CrewCalculator.GunneryCrew(new ComponentCounts<TurretMount>(), bays));
    }

    public static void TestDockingStandardNeedsTwoCrew()
    {
        ComponentCounts<DockingBerthKind> docking = new();
        docking[DockingBerthKind.StandardBerth] = 3;
        DotNetNativeTestSuite.AssertEqual(6, CrewCalculator.DockingCrew(docking));
    }

    public static void TestFacilitiesCrewScales()
    {
        ComponentCounts<FacilityKind> facilities = new();
        facilities[FacilityKind.ShipyardMedium] = 2;
        facilities[FacilityKind.Laboratory] = 1;
        DotNetNativeTestSuite.AssertEqual(23, CrewCalculator.FacilitiesCrew(facilities));
    }

    public static void TestAdminIsOneTwentieth()
    {
        DotNetNativeTestSuite.AssertEqual(5, CrewCalculator.AdminCrew(100));
    }
}
