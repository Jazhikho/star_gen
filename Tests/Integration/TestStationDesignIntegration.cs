#nullable enable annotations
#nullable disable warnings
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Classification;
using StarGen.Services.Export;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Integration tests for detailed station design generation, persistence, and export.
/// </summary>
public static class TestStationDesignIntegration
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestStationDesignIntegration::test_station_generator_creates_detailed_design", TestStationGeneratorCreatesDetailedDesign);
        runner.RunNativeTest("TestStationDesignIntegration::test_space_station_full_round_trip", TestSpaceStationFullRoundTrip);
        runner.RunNativeTest("TestStationDesignIntegration::test_space_station_legacy_compact_round_trip", TestSpaceStationLegacyCompactRoundTrip);
        runner.RunNativeTest("TestStationDesignIntegration::test_space_station_scalar_legacy_round_trip_preserves_spec", TestSpaceStationScalarLegacyRoundTripPreservesSpec);
        runner.RunNativeTest("TestStationDesignIntegration::test_station_stat_block_export", TestStationStatBlockExport);
    }

    public static void TestStationGeneratorCreatesDetailedDesign()
    {
        StationSystemContext context = CreateColonyContext();
        StationSpec spec = new();
        spec.GenerationSeed = 12345;
        spec.GenerateDetailedDesign = true;

        StationGenerator.GenerationResult result = StationGenerator.Generate(context, spec);
        DotNetNativeTestSuite.AssertGreaterThan(result.Stations.Count, 0, "Should generate at least one large station");

        SpaceStation station = result.Stations[0];
        DotNetNativeTestSuite.AssertNotNull(station.DetailedDesign, "Detailed design should be generated");
        DotNetNativeTestSuite.AssertNotNull(station.ClassificationReport, "Classification report should be generated");
    }

    public static void TestSpaceStationFullRoundTrip()
    {
        SpaceStation station = CreateDesignedStation();
        Dictionary data = station.ToDictionary();
        SpaceStation restored = SpaceStation.FromDictionary(data);

        DotNetNativeTestSuite.AssertNotNull(restored.DetailedDesign, "Detailed design should round-trip");
        DotNetNativeTestSuite.AssertNotNull(restored.ClassificationReport, "Classification report should round-trip");
        DotNetNativeTestSuite.AssertEqual(station.DetailedDesign!.Cost.Total, restored.DetailedDesign!.Cost.Total);
        DotNetNativeTestSuite.AssertEqual(station.DetailedDesign.CatalogVersion, restored.DetailedDesign.CatalogVersion);
        DotNetNativeTestSuite.AssertTrue(restored.ClassificationReport!.IsEarned(ClassificationId.StarportB));
    }

    public static void TestSpaceStationLegacyCompactRoundTrip()
    {
        SpaceStation station = CreateDesignedStation();
        Dictionary data = station.ToDictionary();
        data.Remove("detailed_design");
        data.Remove("classification_report");

        SpaceStation restored = SpaceStation.FromDictionary(data);
        DotNetNativeTestSuite.AssertNotNull(restored.DetailedDesign, "Detailed design should regenerate from compact fields");
        DotNetNativeTestSuite.AssertNotNull(restored.ClassificationReport, "Classification report should regenerate from compact fields");
        DotNetNativeTestSuite.AssertEqual(station.DetailedDesign!.Tonnage.Used, restored.DetailedDesign!.Tonnage.Used);
    }

    public static void TestSpaceStationScalarLegacyRoundTripPreservesSpec()
    {
        SpaceStation station = CreateManualDesignedStation();
        Dictionary data = station.ToDictionary();
        data.Remove("detailed_design");
        data.Remove("classification_report");
        data.Remove("detailed_design_compact");

        SpaceStation restored = SpaceStation.FromDictionary(data);
        DotNetNativeTestSuite.AssertNotNull(restored.DetailedDesign, "Detailed design should regenerate from scalar compact fields");
        DotNetNativeTestSuite.AssertEqual(station.DetailedDesign!.Spec.AutoFlags, restored.DetailedDesign!.Spec.AutoFlags);
        DotNetNativeTestSuite.AssertEqual(station.DetailedDesign.Spec.OfficerRatio, restored.DetailedDesign.Spec.OfficerRatio);
    }

    public static void TestStationStatBlockExport()
    {
        SpaceStation station = CreateDesignedStation();
        string statBlock = StationStatBlockExporter.Export(station.Name, station.DetailedDesign!, station.ClassificationReport);

        DotNetNativeTestSuite.AssertTrue(statBlock.Contains(station.Name), "Stat block should contain the station name");
        DotNetNativeTestSuite.AssertTrue(statBlock.Contains("Classifications:"), "Stat block should contain classifications");
        DotNetNativeTestSuite.AssertTrue(statBlock.Contains("TOTAL COST:"), "Stat block should contain total cost");
    }

    private static StationSystemContext CreateColonyContext()
    {
        StationSystemContext context = new();
        context.SystemId = "colony_system";
        context.ColonyWorldCount = 2;
        context.HabitablePlanetCount = 2;
        context.PlanetIds = new Array<string> { "planet_001", "planet_002" };
        context.ColonyPlanetIds = new Array<string> { "planet_001", "planet_002" };
        return context;
    }

    private static SpaceStation CreateDesignedStation()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.HighportB,
            HullTons = 50_000,
            Configuration = HullConfiguration.Ring,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        SpaceStation station = new();
        station.Id = "station_001";
        station.Name = "Test Highport";
        station.Population = 50_000;
        station.StationClass = StationClass.Class.B;
        station.StationType = StationType.Type.Orbital;
        station.PrimaryPurpose = StationPurpose.Purpose.Trade;
        station.HullTonnage = spec.HullTons;
        station.DetailedDesign = DesignCalculator.Calculate(spec);
        station.ClassificationReport = ClassificationEvaluator.Evaluate(station.DetailedDesign);
        return station;
    }

    private static SpaceStation CreateManualDesignedStation()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.HighportB,
            HullTons = 50_000,
            Configuration = HullConfiguration.Ring,
            AutoFlags = AutoPopulateFlags.Command | AutoPopulateFlags.Engineering,
            OfficerRatio = 0.35,
        };

        SpaceStation station = new();
        station.Id = "station_002";
        station.Name = "Manual Highport";
        station.Population = 50_000;
        station.StationClass = StationClass.Class.B;
        station.StationType = StationType.Type.Orbital;
        station.PrimaryPurpose = StationPurpose.Purpose.Trade;
        station.HullTonnage = spec.HullTons;
        station.DetailedDesign = DesignCalculator.Calculate(spec);
        station.ClassificationReport = ClassificationEvaluator.Evaluate(station.DetailedDesign);
        return station;
    }
}
