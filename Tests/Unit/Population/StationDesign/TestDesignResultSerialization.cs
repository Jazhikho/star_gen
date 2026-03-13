using Godot;
using Godot.Collections;
using StarGen.Domain.Population.StationDesign;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestDesignResultSerialization
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestDesignResultSerialization::test_full_round_trip", TestFullRoundTrip);
        runner.RunNativeTest("TestDesignResultSerialization::test_compact_regeneration", TestCompactRegeneration);
        runner.RunNativeTest("TestDesignResultSerialization::test_compact_regeneration_preserves_non_default_spec", TestCompactRegenerationPreservesNonDefaultSpec);
    }

    public static void TestFullRoundTrip()
    {
        DesignResult original = CreateResult();
        Dictionary data = original.ToDictionary();
        DesignResult restored = DesignResult.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.CatalogVersion, restored.CatalogVersion);
        DotNetNativeTestSuite.AssertEqual(original.Spec.Template, restored.Spec.Template);
        DotNetNativeTestSuite.AssertEqual(original.Selection.PowerPlant, restored.Selection.PowerPlant);
        DotNetNativeTestSuite.AssertEqual(original.Crew.Total, restored.Crew.Total);
        DotNetNativeTestSuite.AssertEqual(original.Cost.Total, restored.Cost.Total);
        DotNetNativeTestSuite.AssertEqual(original.Tonnage.Used, restored.Tonnage.Used);
        DotNetNativeTestSuite.AssertEqual(original.Warnings.Count, restored.Warnings.Count);
    }

    public static void TestCompactRegeneration()
    {
        DesignResult original = CreateResult();
        Dictionary compact = original.ToCompactDictionary();
        DesignResult restored = DesignResult.FromCompactDictionary(compact);

        DotNetNativeTestSuite.AssertEqual(original.Spec.Template, restored.Spec.Template);
        DotNetNativeTestSuite.AssertEqual(original.Spec.HullTons, restored.Spec.HullTons);
        DotNetNativeTestSuite.AssertEqual(original.Spec.Configuration, restored.Spec.Configuration);
        DotNetNativeTestSuite.AssertEqual(original.Tonnage.Used, restored.Tonnage.Used);
        DotNetNativeTestSuite.AssertEqual(original.Cost.Total, restored.Cost.Total);
        DotNetNativeTestSuite.AssertEqual(original.Crew.Total, restored.Crew.Total);
    }

    public static void TestCompactRegenerationPreservesNonDefaultSpec()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.HighportB,
            HullTons = 50_000,
            Configuration = HullConfiguration.Ring,
            AutoFlags = AutoPopulateFlags.Command | AutoPopulateFlags.Engineering,
            OfficerRatio = 0.35,
        };

        DesignResult original = DesignCalculator.Calculate(spec);
        Dictionary compact = original.ToCompactDictionary();
        DesignResult restored = DesignResult.FromCompactDictionary(compact);

        DotNetNativeTestSuite.AssertEqual(original.Spec.AutoFlags, restored.Spec.AutoFlags);
        DotNetNativeTestSuite.AssertEqual(original.Spec.OfficerRatio, restored.Spec.OfficerRatio);
        DotNetNativeTestSuite.AssertEqual(original.Spec.Configuration, restored.Spec.Configuration);
    }

    private static DesignResult CreateResult()
    {
        DesignSpec spec = new()
        {
            Template = DesignTemplate.HighportB,
            HullTons = 50_000,
            Configuration = HullConfiguration.Sphere,
            AutoFlags = AutoPopulateFlags.AllAuto,
        };

        return DesignCalculator.Calculate(spec);
    }
}
