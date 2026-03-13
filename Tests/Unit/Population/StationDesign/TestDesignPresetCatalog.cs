using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Presets;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population.StationDesign;

public static class TestDesignPresetCatalog
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestDesignPresetCatalog::test_every_template_has_preset", TestEveryTemplateHasPreset);
        runner.RunNativeTest("TestDesignPresetCatalog::test_catalog_version", TestCatalogVersion);
        runner.RunNativeTest("TestDesignPresetCatalog::test_get_unknown_template_throws", TestGetUnknownTemplateThrows);
    }

    public static void TestEveryTemplateHasPreset()
    {
        foreach (DesignTemplate template in System.Enum.GetValues<DesignTemplate>())
        {
            DesignPreset preset = DesignPresetCatalog.Get(template);
            DotNetNativeTestSuite.AssertTrue(preset.PowerMargin >= 1.0, $"{template} should have power margin >= 1.0");
            DotNetNativeTestSuite.AssertTrue(preset.OfficerRatio > 0.0, $"{template} should have an officer ratio");
        }
    }

    public static void TestCatalogVersion()
    {
        DotNetNativeTestSuite.AssertEqual("1.1.0", DesignPresetCatalog.CatalogVersion);
    }

    public static void TestGetUnknownTemplateThrows()
    {
        bool threw = false;
        try
        {
            DesignPresetCatalog.Get((DesignTemplate)999);
        }
        catch (System.ArgumentException)
        {
            threw = true;
        }

        DotNetNativeTestSuite.AssertTrue(threw, "Unknown template should throw");
    }
}
