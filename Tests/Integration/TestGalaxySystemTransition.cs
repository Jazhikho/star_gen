#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestGalaxySystemTransition
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestGalaxySystemTransition::test_galaxy_viewer_starts_at_subsector", TestGalaxyViewerStartsAtSubsector);
        runner.RunNativeTest("TestGalaxySystemTransition::test_open_system_saves_galaxy_state", TestOpenSystemSavesGalaxyState);
        runner.RunNativeTest("TestGalaxySystemTransition::test_back_to_galaxy_restores_state", TestBackToGalaxyRestoresState);
        runner.RunNativeTest("TestGalaxySystemTransition::test_system_is_cached_after_generation", TestSystemIsCachedAfterGeneration);
        runner.RunNativeTest("TestGalaxySystemTransition::test_returning_to_same_system_uses_cache", TestReturningToSameSystemUsesCache);
        runner.RunNativeTest("TestGalaxySystemTransition::test_different_stars_have_different_systems", TestDifferentStarsHaveDifferentSystems);
        runner.RunNativeTest("TestGalaxySystemTransition::test_current_star_seed_tracked", TestCurrentStarSeedTracked);
        runner.RunNativeTest("TestGalaxySystemTransition::test_full_round_trip_galaxy_system_object_system_galaxy", TestFullRoundTripGalaxySystemObjectSystemGalaxy);
        runner.RunNativeTest("TestGalaxySystemTransition::test_zoom_out_then_open_system_then_return", TestZoomOutThenOpenSystemThenReturn);
        runner.RunNativeTest("TestGalaxySystemTransition::test_clear_saved_state", TestClearSavedState);
    }

    private static MainApp CreateStartedApp()
    {
        return IntegrationTestUtils.CreateMainAppReadyAndStarted();
    }

    private static void TestGalaxyViewerStartsAtSubsector()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Subsector, viewer!.get_zoom_level(), "Should start at subsector");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestOpenSystemSavesGalaxyState()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            DotNetNativeTestSuite.AssertFalse(viewer!.has_saved_state(), "Saved state should start empty");
            app._on_open_system_requested(12345, new Vector3(8000.0f, 20.0f, 0.0f));
            DotNetNativeTestSuite.AssertTrue(viewer.has_saved_state(), "Opening system should save state");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestBackToGalaxyRestoresState()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            int initialZoom = viewer!.get_zoom_level();
            app._on_open_system_requested(12345, new Vector3(8000.0f, 20.0f, 0.0f));
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Should switch to system");
            app._on_back_to_galaxy();
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Should return to galaxy");
            DotNetNativeTestSuite.AssertEqual(initialZoom, viewer.get_zoom_level(), "Zoom should restore");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSystemIsCachedAfterGeneration()
    {
        MainApp app = CreateStartedApp();
        try
        {
            int starSeed = 99999;
            app._on_open_system_requested(starSeed, Vector3.Zero);
            DotNetNativeTestSuite.AssertTrue(app.get_system_cache().HasSystem(starSeed), "System should be cached");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestReturningToSameSystemUsesCache()
    {
        MainApp app = CreateStartedApp();
        try
        {
            int starSeed = 77777;
            app._on_open_system_requested(starSeed, Vector3.Zero);
            SolarSystem? firstSystem = app.get_system_cache().GetSystem(starSeed);
            DotNetNativeTestSuite.AssertNotNull(firstSystem, "Cached system should exist");
            app._on_back_to_galaxy();
            app._on_open_system_requested(starSeed, Vector3.Zero);
            SolarSystem? secondSystem = app.get_system_cache().GetSystem(starSeed);
            DotNetNativeTestSuite.AssertTrue(ReferenceEquals(firstSystem, secondSystem), "Should reuse cached instance");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestDifferentStarsHaveDifferentSystems()
    {
        MainApp app = CreateStartedApp();
        try
        {
            int seedA = 11111;
            int seedB = 22222;
            app._on_open_system_requested(seedA, Vector3.Zero);
            app._on_back_to_galaxy();
            app._on_open_system_requested(seedB, Vector3.Zero);
            SolarSystem? systemA = app.get_system_cache().GetSystem(seedA);
            SolarSystem? systemB = app.get_system_cache().GetSystem(seedB);
            DotNetNativeTestSuite.AssertNotNull(systemA, "First system should exist");
            DotNetNativeTestSuite.AssertNotNull(systemB, "Second system should exist");
            DotNetNativeTestSuite.AssertFalse(ReferenceEquals(systemA, systemB), "Different seeds should produce different systems");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestCurrentStarSeedTracked()
    {
        MainApp app = CreateStartedApp();
        try
        {
            DotNetNativeTestSuite.AssertEqual(0, app.get_current_star_seed(), "No star should be active initially");
            int starSeed = 55555;
            app._on_open_system_requested(starSeed, Vector3.Zero);
            DotNetNativeTestSuite.AssertEqual(starSeed, app.get_current_star_seed(), "Current star seed should be tracked");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestFullRoundTripGalaxySystemObjectSystemGalaxy()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            int initialZoom = viewer!.get_zoom_level();

            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Start at galaxy");
            app._on_open_system_requested(12345, new Vector3(8000.0f, 20.0f, 0.0f));
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "At system");

            CelestialBody body = IntegrationTestUtils.CreateTestBody(type: CelestialType.Type.Star, massKg: 1.989e30, radiusM: 6.96e8);
            app._on_open_in_object_viewer(body);
            DotNetNativeTestSuite.AssertEqual("object", app.get_active_viewer(), "At object");

            app._on_back_to_system();
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Back at system");

            app._on_back_to_galaxy();
            DotNetNativeTestSuite.AssertEqual("galaxy", app.get_active_viewer(), "Back at galaxy");
            DotNetNativeTestSuite.AssertEqual(initialZoom, viewer.get_zoom_level(), "Zoom should restore");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestZoomOutThenOpenSystemThenReturn()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            InputEventKey zoomOut = new()
            {
                Keycode = Key.Bracketleft,
                Pressed = true,
            };
            viewer!._handle_key_input(zoomOut);
            int zoomedOutLevel = viewer.get_zoom_level();
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Sector, zoomedOutLevel, "Should zoom out to sector");

            app._on_open_system_requested(33333, Vector3.Zero);
            app._on_back_to_galaxy();
            DotNetNativeTestSuite.AssertEqual(zoomedOutLevel, viewer.get_zoom_level(), "Should restore zoom level");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestClearSavedState()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            app._on_open_system_requested(12345, Vector3.Zero);
            DotNetNativeTestSuite.AssertTrue(viewer!.has_saved_state(), "Should have saved state");
            viewer.clear_saved_state();
            DotNetNativeTestSuite.AssertFalse(viewer.has_saved_state(), "Saved state should clear");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }
}
