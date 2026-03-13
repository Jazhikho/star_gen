using Godot.Collections;
using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Classification;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestClassificationReportSerialization
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestClassificationReportSerialization::test_round_trip", TestRoundTrip);
    }

    public static void TestRoundTrip()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.Waystation,
            HullTons = 5_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        ClassificationReport original = ClassificationEvaluator.Evaluate(DesignCalculator.Calculate(spec));
        Dictionary data = original.ToDictionary();
        ClassificationReport restored = ClassificationReport.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Earned.Count, restored.Earned.Count);
        DotNetNativeTestSuite.AssertTrue(restored.IsEarned(ClassificationId.Waystation));
        DotNetNativeTestSuite.AssertEqual(
            original.Results[ClassificationId.Waystation].MetCount,
            restored.Results[ClassificationId.Waystation].MetCount);
    }
}
