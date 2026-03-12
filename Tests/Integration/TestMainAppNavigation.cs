#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.Domain.Celestial;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestMainAppNavigation
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestMainAppNavigation::test_starts_with_galaxy_viewer", TestStartsWithGalaxyViewer);
        runner.RunNativeTest("TestMainAppNavigation::test_galaxy_seed_is_set", TestGalaxySeedIsSet);
        runner.RunNativeTest("TestMainAppNavigation::test_system_cache_exists", TestSystemCacheExists);
        runner.RunNativeTest("TestMainAppNavigation::test_open_system_transitions_to_system_viewer", TestOpenSystemTransitionsToSystemViewer);
        runner.RunNativeTest("TestMainAppNavigation::test_open_system_caches_generated_system", TestOpenSystemCachesGeneratedSystem);
        runner.RunNativeTest("TestMainAppNavigation::test_open_same_system_uses_cache", TestOpenSameSystemUsesCache);
        runner.RunNativeTest("TestMainAppNavigation::test_back_to_galaxy_from_system", TestBackToGalaxyFromSystem);
        runner.RunNativeTest("TestMainAppNavigation::test_system_to_object_navigation", TestSystemToObjectNavigation);
        runner.RunNativeTest("TestMainAppNavigation::test_back_to_system_from_object", TestBackToSystemFromObject);
        runner.RunNativeTest("TestMainAppNavigation::test_full_navigation_cycle", TestFullNavigationCycle);
        runner.RunNativeTest("TestMainAppNavigation::test_generated_system_is_deterministic", TestGeneratedSystemIsDeterministic);
        runner.RunNativeTest("TestMainAppNavigation::test_zero_star_seed_ignored", TestZeroStarSeedIgnored);
        runner.RunNativeTest("TestMainAppNavigation::test_main_menu_system_generation_opens_studio", TestMainMenuSystemGenerationOpensStudio);
        runner.RunNativeTest("TestMainAppNavigation::test_main_menu_object_generation_opens_studio", TestMainMenuObjectGenerationOpensStudio);
        runner.RunNativeTest("TestMainAppNavigation::test_system_studio_launch_generates_before_viewer", TestSystemStudioLaunchGeneratesBeforeViewer);
        runner.RunNativeTest("TestMainAppNavigation::test_object_studio_launch_generates_before_viewer", TestObjectStudioLaunchGeneratesBeforeViewer);
    }

    private static MainApp CreateStartedApp()
    {
        return IntegrationTestUtils.CreateMainAppReadyAndStarted();
    }

    private static void TestStartsWithGalaxyViewer()
    {
        MainApp app = CreateStartedApp();
        try
        {
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Should start in galaxy view");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestGalaxySeedIsSet()
    {
        MainApp app = CreateStartedApp();
        try
        {
            DotNetNativeTestSuite.AssertGreaterThan(app.get_galaxy_seed(), 0, "Galaxy seed should be positive");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSystemCacheExists()
    {
        MainApp app = CreateStartedApp();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(app.get_system_cache(), "System cache should exist");
            DotNetNativeTestSuite.AssertEqual(0, app.get_system_cache().GetCacheSize(), "Cache should start empty");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestOpenSystemTransitionsToSystemViewer()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(12345, Vector3.Zero);
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Should transition to system view");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestOpenSystemCachesGeneratedSystem()
    {
        MainApp app = CreateStartedApp();
        try
        {
            int starSeed = 12345;
            app._on_open_system_requested(starSeed, Vector3.Zero);
            DotNetNativeTestSuite.AssertTrue(app.get_system_cache().HasSystem(starSeed), "Generated system should be cached");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestOpenSameSystemUsesCache()
    {
        MainApp app = CreateStartedApp();
        try
        {
            int starSeed = 12345;
            app._on_open_system_requested(starSeed, Vector3.Zero);
            object? firstSystem = app.get_system_cache().GetSystem(starSeed);

            app._on_back_to_galaxy();
            app._on_open_system_requested(starSeed, Vector3.Zero);
            object? secondSystem = app.get_system_cache().GetSystem(starSeed);

            DotNetNativeTestSuite.AssertTrue(ReferenceEquals(firstSystem, secondSystem), "Same seed should use cache");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestBackToGalaxyFromSystem()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(12345, Vector3.Zero);
            app._on_back_to_galaxy();
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Should return to galaxy view");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSystemToObjectNavigation()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(12345, Vector3.Zero);
            CelestialBody body = IntegrationTestUtils.CreateTestBody(type: CelestialType.Type.Planet);
            app._on_open_in_object_viewer(body);
            DotNetNativeTestSuite.AssertEqual("object", app.get_active_viewer(), "Should transition to object view");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestBackToSystemFromObject()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(12345, Vector3.Zero);
            CelestialBody body = IntegrationTestUtils.CreateTestBody(type: CelestialType.Type.Planet);
            app._on_open_in_object_viewer(body);
            app._on_back_to_system();
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Should return to system view");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestFullNavigationCycle()
    {
        MainApp app = CreateStartedApp();
        try
        {
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Should start at galaxy");
            app._on_open_system_requested(12345, Vector3.Zero);
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Should be at system");
            CelestialBody body = IntegrationTestUtils.CreateTestBody(type: CelestialType.Type.Star);
            app._on_open_in_object_viewer(body);
            DotNetNativeTestSuite.AssertEqual("object", app.get_active_viewer(), "Should be at object");
            app._on_back_to_system();
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Should return to system");
            app._on_back_to_galaxy();
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Should return to galaxy");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestGeneratedSystemIsDeterministic()
    {
        MainApp app = CreateStartedApp();
        try
        {
            int starSeed = 99999;
            SolarSystem? systemOne = app._generate_system_from_seed(starSeed);
            SolarSystem? systemTwo = app._generate_system_from_seed(starSeed);
            DotNetNativeTestSuite.AssertNotNull(systemOne, "First system should generate");
            DotNetNativeTestSuite.AssertNotNull(systemTwo, "Second system should generate");
            DotNetNativeTestSuite.AssertEqual(systemOne!.GetStars().Count, systemTwo!.GetStars().Count, "Star count should match");
            DotNetNativeTestSuite.AssertEqual(systemOne.GetPlanets().Count, systemTwo.GetPlanets().Count, "Planet count should match");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestZeroStarSeedIgnored()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(0, Vector3.Zero);
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Zero seed should be ignored");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestMainMenuSystemGenerationOpensStudio()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_system_generation_requested();

            DotNetNativeTestSuite.AssertEqual("systemstudio", app.get_active_viewer(), "Main-menu system generation should open the system studio");
            SystemGenerationScreen? screen = app.get_system_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "System studio should exist");
            Button? startButton = screen!.GetNodeOrNull<Button>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel/MarginContainer/SummaryVBox/Buttons/StartButton");
            DotNetNativeTestSuite.AssertNotNull(startButton, "System studio should expose a launch button");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestMainMenuObjectGenerationOpensStudio()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_object_generation_requested();

            DotNetNativeTestSuite.AssertEqual("objectstudio", app.get_active_viewer(), "Main-menu object generation should open the object studio");
            ObjectGenerationScreen? screen = app.get_object_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Object studio should exist");
            Button? startButton = screen!.GetNodeOrNull<Button>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel/MarginContainer/SummaryVBox/Buttons/StartButton");
            DotNetNativeTestSuite.AssertNotNull(startButton, "Object studio should expose a launch button");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSystemStudioLaunchGeneratesBeforeViewer()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_system_generation_requested();
            SystemGenerationScreen? screen = app.get_system_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "System studio should exist");

            screen!.EmitSignal("start_system_generation", screen.GetCurrentSpec());

            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Launching from the system studio should open the system viewer");
            StarGen.App.SystemViewer.SystemViewer? viewer = app.get_system_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "System viewer should exist");
            DotNetNativeTestSuite.AssertNotNull(viewer!.GetCurrentSystem(), "System studio launch should generate a system immediately");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestObjectStudioLaunchGeneratesBeforeViewer()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_object_generation_requested();
            ObjectGenerationScreen? screen = app.get_object_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Object studio should exist");

            screen!.EmitSignal("start_object_generation", screen.GetCurrentRequest());

            DotNetNativeTestSuite.AssertEqual("object", app.get_active_viewer(), "Launching from the object studio should open the object viewer");
            StarGen.App.Viewer.ObjectViewer? viewer = app.get_object_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Object viewer should exist");
            DotNetNativeTestSuite.AssertNotNull(viewer!.current_body, "Object studio launch should generate a body immediately");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }
}
