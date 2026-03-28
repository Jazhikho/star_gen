#nullable enable annotations
#nullable disable warnings
using StarGen.Tests.Integration;

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Runs the headless-safe integration tests.
    /// </summary>
    public static void RunHeadlessIntegrationTests(DotNetTestRunner runner)
    {
        TestPopulationGoldenMasters.RunAll(runner);
        TestPopulationIntegration.RunAll(runner);
        TestGenerationParameters.RunAll(runner);
        TestWindowSettingsService.RunAll(runner);
        TestObjectViewerMoons.RunAll(runner);
        TestStationDesignIntegration.RunAll(runner);
    }

    /// <summary>
    /// Runs the interactive-only integration tests.
    /// </summary>
    public static void RunSceneOnlyIntegrationTests(DotNetTestRunner runner)
    {
    }
}
