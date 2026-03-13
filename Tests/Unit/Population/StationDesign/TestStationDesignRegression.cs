using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Classification;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestStationDesignRegression
{
    private const string FixtureDirectory = "res://Tests/Fixtures/StationDesign";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestStationDesignRegression::test_fixture_regressions", TestFixtureRegressions);
    }

    public static void TestFixtureRegressions()
    {
        foreach (string fixtureName in GetFixtureNames())
        {
            RegressionFixture fixture = LoadFixture(fixtureName);
            DesignSpec spec = new()
            {
                Template = Enum.Parse<DesignTemplate>(fixture.Template),
                HullTons = fixture.HullTons,
                Configuration = Enum.Parse<HullConfiguration>(fixture.Configuration),
                AutoFlags = AutoPopulateFlags.AllAuto,
            };

            DesignResult first = DesignCalculator.Calculate(spec);
            DesignResult second = DesignCalculator.Calculate(spec);
            ClassificationReport firstReport = ClassificationEvaluator.Evaluate(first);
            ClassificationReport secondReport = ClassificationEvaluator.Evaluate(second);

            AssertMatchesFixture(fixture, first, firstReport);
            AssertMatchesFixture(fixture, second, secondReport);
            AssertDeterministic(first, second, firstReport, secondReport, fixture.Name);
        }
    }

    private static void AssertMatchesFixture(
        RegressionFixture fixture,
        DesignResult result,
        ClassificationReport report)
    {
        DotNetNativeTestSuite.AssertEqual(fixture.UsedTonnage, result.Tonnage.Used, fixture.Name + " used tonnage");
        DotNetNativeTestSuite.AssertEqual(fixture.CargoTonnage, result.Tonnage.Cargo, fixture.Name + " cargo tonnage");
        DotNetNativeTestSuite.AssertEqual(fixture.TotalCost, result.Cost.Total, fixture.Name + " total cost");
        DotNetNativeTestSuite.AssertEqual(fixture.TotalCrew, result.Crew.Total, fixture.Name + " total crew");
        DotNetNativeTestSuite.AssertEqual(fixture.PowerDemand, result.Power.Demand, fixture.Name + " power demand");
        DotNetNativeTestSuite.AssertEqual(fixture.PowerOutput, result.Power.Output, fixture.Name + " power output");
        DotNetNativeTestSuite.AssertEqual(fixture.HardpointsUsed, result.HardpointsUsed, fixture.Name + " hardpoints used");
        DotNetNativeTestSuite.AssertEqual(fixture.WarningCount, result.Warnings.Count, fixture.Name + " warning count");

        List<ClassificationId> actual = new(report.Earned);
        List<ClassificationId> expected = new();
        List<string>? expectedNames = fixture.EarnedClassifications;
        if (expectedNames == null)
        {
            throw new InvalidOperationException("Fixture missing earned classifications: " + fixture.Name);
        }

        foreach (string expectedName in expectedNames)
        {
            expected.Add(Enum.Parse<ClassificationId>(expectedName));
        }

        DotNetNativeTestSuite.AssertEqual(expected.Count, actual.Count, fixture.Name + " earned count");
        for (int index = 0; index < expected.Count; index++)
        {
            DotNetNativeTestSuite.AssertEqual(expected[index], actual[index], fixture.Name + " earned[" + index + "]");
        }
    }

    private static void AssertDeterministic(
        DesignResult first,
        DesignResult second,
        ClassificationReport firstReport,
        ClassificationReport secondReport,
        string fixtureName)
    {
        DotNetNativeTestSuite.AssertEqual(first.Tonnage.Used, second.Tonnage.Used, fixtureName + " deterministic used tonnage");
        DotNetNativeTestSuite.AssertEqual(first.Tonnage.Cargo, second.Tonnage.Cargo, fixtureName + " deterministic cargo tonnage");
        DotNetNativeTestSuite.AssertEqual(first.Cost.Total, second.Cost.Total, fixtureName + " deterministic cost");
        DotNetNativeTestSuite.AssertEqual(first.Crew.Total, second.Crew.Total, fixtureName + " deterministic crew");
        DotNetNativeTestSuite.AssertEqual(first.Power.Demand, second.Power.Demand, fixtureName + " deterministic demand");
        DotNetNativeTestSuite.AssertEqual(first.Power.Output, second.Power.Output, fixtureName + " deterministic output");
        DotNetNativeTestSuite.AssertEqual(first.HardpointsUsed, second.HardpointsUsed, fixtureName + " deterministic hardpoints");
        DotNetNativeTestSuite.AssertEqual(firstReport.Earned.Count, secondReport.Earned.Count, fixtureName + " deterministic classifications");

        for (int index = 0; index < firstReport.Earned.Count; index++)
        {
            DotNetNativeTestSuite.AssertEqual(firstReport.Earned[index], secondReport.Earned[index], fixtureName + " deterministic classification[" + index + "]");
        }
    }

    private static IReadOnlyList<string> GetFixtureNames()
    {
        return new[]
        {
            "HighportB-50000.json",
            "HighportA-200000.json",
            "Defense-20000.json",
            "Waystation-5000.json",
        };
    }

    private static RegressionFixture LoadFixture(string fixtureName)
    {
        string path = ProjectSettings.GlobalizePath(FixtureDirectory + "/" + fixtureName);
        string json = File.ReadAllText(path);
        RegressionFixture? fixture = JsonSerializer.Deserialize<RegressionFixture>(json);

        if (fixture == null)
        {
            throw new InvalidOperationException("Failed to deserialize fixture: " + fixtureName);
        }

        if (fixture.EarnedClassifications == null)
        {
            throw new InvalidOperationException("Fixture missing earned classifications: " + fixtureName);
        }

        return fixture;
    }

    private sealed class RegressionFixture
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("template")]
        public string Template { get; set; } = string.Empty;

        [JsonPropertyName("hull_tons")]
        public int HullTons { get; set; }

        [JsonPropertyName("configuration")]
        public string Configuration { get; set; } = string.Empty;

        [JsonPropertyName("used_tonnage")]
        public int UsedTonnage { get; set; }

        [JsonPropertyName("cargo_tonnage")]
        public int CargoTonnage { get; set; }

        [JsonPropertyName("total_cost")]
        public long TotalCost { get; set; }

        [JsonPropertyName("total_crew")]
        public int TotalCrew { get; set; }

        [JsonPropertyName("power_demand")]
        public int PowerDemand { get; set; }

        [JsonPropertyName("power_output")]
        public int PowerOutput { get; set; }

        [JsonPropertyName("hardpoints_used")]
        public int HardpointsUsed { get; set; }

        [JsonPropertyName("warning_count")]
        public int WarningCount { get; set; }

        [JsonPropertyName("earned_classifications")]
        public List<string>? EarnedClassifications { get; set; }
    }
}
