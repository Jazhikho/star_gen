using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Presets;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestPresetApplicator
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestPresetApplicator::test_highport_b_facilities_50k", TestHighportBFacilities50k);
        runner.RunNativeTest("TestPresetApplicator::test_highport_b_docking_50k", TestHighportBDocking50k);
        runner.RunNativeTest("TestPresetApplicator::test_turret_mix_total_matches_fraction", TestTurretMixTotalMatchesFraction);
        runner.RunNativeTest("TestPresetApplicator::test_bays_respect_tonnage_budget", TestBaysRespectTonnageBudget);
        runner.RunNativeTest("TestPresetApplicator::test_accommodations_cover_crew", TestAccommodationsCoverCrew);
        runner.RunNativeTest("TestPresetApplicator::test_determinism", TestDeterminism);
    }

    public static void TestHighportBFacilities50k()
    {
        DesignPreset preset = DesignPresetCatalog.Get(DesignTemplate.HighportB);
        ComponentCounts<FacilityKind> facilities = PresetApplicator.ApplyFacilities(preset, 50_000);
        DotNetNativeTestSuite.AssertEqual(1, facilities[FacilityKind.ShipyardSmall], "50000/30000 = 1, min 1");
        DotNetNativeTestSuite.AssertEqual(2, facilities[FacilityKind.FuelRefinery], "50000/18000 = 2, min 1");
        DotNetNativeTestSuite.AssertEqual(5, facilities[FacilityKind.Commercial], "50000/10000 = 5, min 2");
        DotNetNativeTestSuite.AssertEqual(1, facilities[FacilityKind.CommsArray], "Zero divisor should use minimum");
    }

    public static void TestHighportBDocking50k()
    {
        DesignPreset preset = DesignPresetCatalog.Get(DesignTemplate.HighportB);
        ComponentCounts<DockingBerthKind> docking = PresetApplicator.ApplyDocking(preset, 50_000);
        DotNetNativeTestSuite.AssertEqual(7, docking[DockingBerthKind.SmallCraftBay]);
        DotNetNativeTestSuite.AssertEqual(12, docking[DockingBerthKind.StandardBerth]);
        DotNetNativeTestSuite.AssertEqual(2, docking[DockingBerthKind.LargeBerth]);
    }

    public static void TestTurretMixTotalMatchesFraction()
    {
        DesignPreset preset = DesignPresetCatalog.Get(DesignTemplate.HighportB);
        ComponentCounts<TurretMount> turrets = PresetApplicator.ApplyTurrets(preset, 50_000);
        int expected = (int)System.Math.Floor((50_000 / 100.0) * preset.Weapons.TurretHardpointFraction);
        DotNetNativeTestSuite.AssertEqual(expected, turrets.Sum(), "Total turrets should match hardpoint fraction");
    }

    public static void TestBaysRespectTonnageBudget()
    {
        DesignPreset preset = DesignPresetCatalog.Get(DesignTemplate.Naval);
        ComponentCounts<BayWeapon> bays = PresetApplicator.ApplyBays(preset, 200_000);
        int bayTonnage = bays.SumBy(HullCalculator.BayWeaponTonnage);
        int budget = (int)System.Math.Floor(200_000 * preset.Weapons.BayTonnageFraction);
        DotNetNativeTestSuite.AssertTrue(bayTonnage <= budget, "Bay tonnage should not exceed its budget");
    }

    public static void TestAccommodationsCoverCrew()
    {
        ComponentCounts<AccommodationKind> accommodations = PresetApplicator.ApplyAccommodations(100, 0.2);
        int berths = 0;
        foreach (System.Collections.Generic.KeyValuePair<AccommodationKind, int> entry in accommodations)
        {
            berths += HullCalculator.AccommodationOccupancy(entry.Key) * entry.Value;
        }

        DotNetNativeTestSuite.AssertTrue(berths >= 100, $"Berths {berths} should cover 100 crew");
    }

    public static void TestDeterminism()
    {
        DesignPreset preset = DesignPresetCatalog.Get(DesignTemplate.Naval);
        ComponentCounts<FacilityKind> first = PresetApplicator.ApplyFacilities(preset, 200_000);
        ComponentCounts<FacilityKind> second = PresetApplicator.ApplyFacilities(preset, 200_000);
        foreach (FacilityKind kind in System.Enum.GetValues<FacilityKind>())
        {
            DotNetNativeTestSuite.AssertEqual(first[kind], second[kind], $"Determinism failed for {kind}");
        }
    }
}
