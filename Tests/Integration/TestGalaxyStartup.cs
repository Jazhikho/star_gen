#nullable enable annotations
#nullable disable warnings
using StarGen.App;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestGalaxyStartup
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestGalaxyStartup::test_after_ready_galaxy_viewer_is_null_until_started",
            TestAfterReadyGalaxyViewerIsNullUntilStarted);
        runner.RunNativeTest(
            "TestGalaxyStartup::test_start_galaxy_with_defaults_creates_viewer",
            TestStartGalaxyWithDefaultsCreatesViewer);
        runner.RunNativeTest(
            "TestGalaxyStartup::test_start_galaxy_with_defaults_sets_positive_seed",
            TestStartGalaxyWithDefaultsSetsPositiveSeed);
    }

    private static void TestAfterReadyGalaxyViewerIsNullUntilStarted()
    {
        MainApp app = IntegrationTestUtils.CreateMainAppReady();
        try
        {
            DotNetNativeTestSuite.AssertNull(app.get_galaxy_viewer(), "Galaxy viewer should be null before start");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestStartGalaxyWithDefaultsCreatesViewer()
    {
        MainApp app = IntegrationTestUtils.CreateMainAppReady();
        try
        {
            app.start_galaxy_with_defaults();
            DotNetNativeTestSuite.AssertNotNull(app.get_galaxy_viewer(), "Galaxy viewer should be created");
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Active viewer should be galaxy");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestStartGalaxyWithDefaultsSetsPositiveSeed()
    {
        MainApp app = IntegrationTestUtils.CreateMainAppReady();
        try
        {
            app.start_galaxy_with_defaults();
            DotNetNativeTestSuite.AssertGreaterThan(app.get_galaxy_seed(), 0, "Galaxy seed should be positive");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }
}
