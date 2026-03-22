#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.App;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
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
        runner.RunNativeTest("TestMainAppNavigation::test_main_menu_galaxy_generation_opens_studio", TestMainMenuGalaxyGenerationOpensStudio);
        runner.RunNativeTest("TestMainAppNavigation::test_main_menu_system_generation_opens_studio", TestMainMenuSystemGenerationOpensStudio);
        runner.RunNativeTest("TestMainAppNavigation::test_main_menu_object_generation_opens_studio", TestMainMenuObjectGenerationOpensStudio);
        runner.RunNativeTest("TestMainAppNavigation::test_main_menu_station_generation_opens_studio", TestMainMenuStationGenerationOpensStudio);
        runner.RunNativeTest("TestMainAppNavigation::test_main_menu_concept_atlas_opens_screen", TestMainMenuConceptAtlasOpensScreen);
        runner.RunNativeTest("TestMainAppNavigation::test_concept_atlas_return_from_menu_goes_to_main_menu", TestConceptAtlasReturnFromMenuGoesToMainMenu);
        runner.RunNativeTest("TestMainAppNavigation::test_object_viewer_can_open_concept_atlas", TestObjectViewerCanOpenConceptAtlas);
        runner.RunNativeTest("TestMainAppNavigation::test_system_studio_launch_generates_before_viewer", TestSystemStudioLaunchGeneratesBeforeViewer);
        runner.RunNativeTest("TestMainAppNavigation::test_object_studio_launch_generates_before_viewer", TestObjectStudioLaunchGeneratesBeforeViewer);
        runner.RunNativeTest("TestMainAppNavigation::test_system_studio_viewer_has_no_back_navigation", TestSystemStudioViewerHasNoBackNavigation);
        runner.RunNativeTest("TestMainAppNavigation::test_object_studio_viewer_has_no_back_navigation", TestObjectStudioViewerHasNoBackNavigation);
        runner.RunNativeTest("TestMainAppNavigation::test_system_studio_object_view_returns_to_system", TestSystemStudioObjectViewReturnsToSystem);
        runner.RunNativeTest("TestMainAppNavigation::test_object_edit_persists_back_to_standalone_system", TestObjectEditPersistsBackToStandaloneSystem);
        runner.RunNativeTest("TestMainAppNavigation::test_galaxy_viewer_can_return_to_main_menu", TestGalaxyViewerCanReturnToMainMenu);
        runner.RunNativeTest("TestMainAppNavigation::test_system_viewer_file_menu_exposes_new_system_and_main_menu", TestSystemViewerFileMenuExposesNewSystemAndMainMenu);
        runner.RunNativeTest("TestMainAppNavigation::test_object_viewer_file_menu_exposes_new_object_and_main_menu", TestObjectViewerFileMenuExposesNewObjectAndMainMenu);
        runner.RunNativeTest("TestMainAppNavigation::test_open_system_respects_realistic_population_settings", TestOpenSystemRespectsRealisticPopulationSettings);
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
            StarGen.App.SystemViewer.SystemViewer? viewer = app.get_system_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "System viewer should exist");
            Button? backButton = viewer!.GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
            Control? generationSection = viewer.GetNodeOrNull<Control>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection");
            DotNetNativeTestSuite.AssertNotNull(backButton, "System viewer should expose a back button");
            DotNetNativeTestSuite.AssertTrue(backButton!.Visible, "Galaxy-opened systems should show a back button");
            DotNetNativeTestSuite.AssertNotNull(generationSection, "System viewer should expose the generation section node");
            DotNetNativeTestSuite.AssertFalse(generationSection!.Visible, "Galaxy-opened systems should hide regeneration controls");
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
            StarGen.App.Viewer.ObjectViewer? viewer = app.get_object_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Object viewer should exist");
            Button? backButton = viewer!.GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
            Control? generationSection = viewer.GetNodeOrNull<Control>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection");
            DotNetNativeTestSuite.AssertNotNull(backButton, "Object viewer should expose a back button");
            DotNetNativeTestSuite.AssertTrue(backButton!.Visible, "System-opened objects should show a back button");
            DotNetNativeTestSuite.AssertNotNull(generationSection, "Object viewer should expose the generation section node");
            DotNetNativeTestSuite.AssertFalse(generationSection!.Visible, "System-opened objects should hide regeneration controls");
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

    private static void TestMainMenuGalaxyGenerationOpensStudio()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_galaxy_generation_requested();

            DotNetNativeTestSuite.AssertEqual("galaxystudio", app.get_active_viewer(), "Main-menu galaxy generation should open the galaxy studio");
            GalaxyGenerationScreen? screen = app.get_galaxy_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Galaxy studio should exist");
            Button? startButton = screen!.GetNodeOrNull<Button>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel/MarginContainer/SummaryVBox/Buttons/StartButton");
            DotNetNativeTestSuite.AssertNotNull(startButton, "Galaxy studio should expose a launch button");
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
            DotNetNativeTestSuite.AssertNotNull(
                screen.GetNodeOrNull<Control>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel"),
                "System studio should expose a rules panel");
            DotNetNativeTestSuite.AssertNotNull(
                screen.GetNodeOrNull<Control>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel"),
                "System studio should expose a summary panel");
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
            DotNetNativeTestSuite.AssertNotNull(
                screen.GetNodeOrNull<Control>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel"),
                "Object studio should expose a rules panel");
            DotNetNativeTestSuite.AssertNotNull(
                screen.GetNodeOrNull<Control>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel"),
                "Object studio should expose a summary panel");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestMainMenuStationGenerationOpensStudio()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_station_generation_requested();

            DotNetNativeTestSuite.AssertEqual("stationstudio", app.get_active_viewer(), "Main-menu station generation should open the station studio");
            StationStudioScreen? screen = app.get_station_studio_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Station studio should exist");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestMainMenuConceptAtlasOpensScreen()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_concept_atlas_requested();

            DotNetNativeTestSuite.AssertEqual("conceptatlas", app.get_active_viewer(), "Main-menu concept atlas should open the concept atlas");
            StarGen.App.Concepts.ConceptAtlasScreen? screen = app.get_concept_atlas_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Concept atlas screen should exist");
            ItemList? moduleList = screen!.FindChild("ModuleList", recursive: true, owned: false) as ItemList;
            DotNetNativeTestSuite.AssertNotNull(moduleList, "Concept atlas should build the module list");
            DotNetNativeTestSuite.AssertGreaterThan(moduleList!.ItemCount, 0, "Concept atlas should list concept modules");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestConceptAtlasReturnFromMenuGoesToMainMenu()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_galaxy_viewer_main_menu_requested();
            app._on_main_menu_concept_atlas_requested();
            StarGen.App.Concepts.ConceptAtlasScreen? screen = app.get_concept_atlas_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Concept atlas should exist");

            screen!.EmitSignal(StarGen.App.Concepts.ConceptAtlasScreen.SignalName.BackRequested);
            DotNetNativeTestSuite.AssertEqual("menu", app.get_active_viewer(), "Returning from main-menu atlas should restore the menu");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestObjectViewerCanOpenConceptAtlas()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(12345, Vector3.Zero);
            CelestialBody body = IntegrationTestUtils.CreateTestBody(name: "Mira", type: CelestialType.Type.Planet);
            app._on_open_in_object_viewer(body);
            app._on_object_concept_atlas_requested(body, 12345);

            DotNetNativeTestSuite.AssertEqual("conceptatlas", app.get_active_viewer(), "Object viewer atlas request should open the concept atlas");
            StarGen.App.Concepts.ConceptAtlasScreen? screen = app.get_concept_atlas_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Concept atlas should exist");
            DotNetNativeTestSuite.AssertEqual("Mira", screen!.GetContextSnapshot().BodyName, "Object atlas context should preserve the current body");

            screen.EmitSignal(StarGen.App.Concepts.ConceptAtlasScreen.SignalName.BackRequested);
            DotNetNativeTestSuite.AssertEqual("object", app.get_active_viewer(), "Returning from object atlas should restore the object viewer");
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
            Button? backButton = viewer.GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
            Control? generationSection = viewer.GetNodeOrNull<Control>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection");
            DotNetNativeTestSuite.AssertNotNull(backButton, "System viewer should expose a back button");
            DotNetNativeTestSuite.AssertFalse(backButton!.Visible, "System studio viewer should not expose a back button");
            DotNetNativeTestSuite.AssertNotNull(generationSection, "System viewer should expose the generation section node");
            DotNetNativeTestSuite.AssertFalse(generationSection!.Visible, "System studio viewer should hide regeneration controls");
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
            Button? backButton = viewer.GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
            Control? generationSection = viewer.GetNodeOrNull<Control>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection");
            DotNetNativeTestSuite.AssertNotNull(backButton, "Object viewer should expose a back button");
            DotNetNativeTestSuite.AssertFalse(backButton!.Visible, "Object studio viewer should not expose a back button");
            DotNetNativeTestSuite.AssertNotNull(generationSection, "Object viewer should expose the generation section node");
            DotNetNativeTestSuite.AssertFalse(generationSection!.Visible, "Object studio viewer should hide regeneration controls");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSystemStudioViewerHasNoBackNavigation()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_system_generation_requested();
            SystemGenerationScreen? screen = app.get_system_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "System studio should exist");

            screen!.EmitSignal("start_system_generation", screen.GetCurrentSpec());
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "System studio launch should open the system viewer");
            StarGen.App.SystemViewer.SystemViewer? viewer = app.get_system_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "System viewer should exist");
            Button? backButton = viewer!.GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
            PopupMenu? fileMenu = FindViewerFileMenu(viewer);
            DotNetNativeTestSuite.AssertNotNull(backButton, "System viewer should expose a back button node");
            DotNetNativeTestSuite.AssertFalse(backButton!.Visible, "System studio viewer should hide the top-bar back button");
            DotNetNativeTestSuite.AssertNotNull(fileMenu, "System viewer should expose the file menu");
            fileMenu!.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "New System"), "System viewer file menu should expose a new-system action");
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "Return to Main Menu"), "System viewer file menu should expose a main-menu action");
            DotNetNativeTestSuite.AssertFalse(PopupContainsText(fileMenu, "Back to"), "System studio viewer should not offer file-menu back navigation");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestObjectStudioViewerHasNoBackNavigation()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_object_generation_requested();
            ObjectGenerationScreen? screen = app.get_object_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "Object studio should exist");

            screen!.EmitSignal("start_object_generation", screen.GetCurrentRequest());
            DotNetNativeTestSuite.AssertEqual("object", app.get_active_viewer(), "Object studio launch should open the object viewer");
            StarGen.App.Viewer.ObjectViewer? viewer = app.get_object_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Object viewer should exist");
            Button? backButton = viewer!.GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
            PopupMenu? fileMenu = FindViewerFileMenu(viewer);
            DotNetNativeTestSuite.AssertNotNull(backButton, "Object viewer should expose a back button node");
            DotNetNativeTestSuite.AssertFalse(backButton!.Visible, "Object studio viewer should hide the top-bar back button");
            DotNetNativeTestSuite.AssertNotNull(fileMenu, "Object viewer should expose the file menu");
            fileMenu!.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "New Object"), "Object viewer file menu should expose a new-object action");
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "Return to Main Menu"), "Object viewer file menu should expose a main-menu action");
            DotNetNativeTestSuite.AssertFalse(PopupContainsText(fileMenu, "Back to"), "Object studio viewer should not offer file-menu back navigation");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSystemStudioObjectViewReturnsToSystem()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_system_generation_requested();
            SystemGenerationScreen? screen = app.get_system_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "System studio should exist");
            screen!.EmitSignal("start_system_generation", screen.GetCurrentSpec());

            StarGen.App.SystemViewer.SystemViewer? systemViewer = app.get_system_viewer();
            DotNetNativeTestSuite.AssertNotNull(systemViewer, "System viewer should exist");
            SolarSystem? system = systemViewer!.GetCurrentSystem();
            DotNetNativeTestSuite.AssertNotNull(system, "System viewer should have a system");
            CelestialBody body = GetPreferredSystemBody(system!);

            app._on_open_in_object_viewer(body);
            StarGen.App.Viewer.ObjectViewer? objectViewer = app.get_object_viewer();
            DotNetNativeTestSuite.AssertNotNull(objectViewer, "Object viewer should exist");
            Button? backButton = objectViewer!.GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
            DotNetNativeTestSuite.AssertNotNull(backButton, "Object viewer should expose a back button node");
            DotNetNativeTestSuite.AssertTrue(backButton!.Visible, "System-opened object view should show a back button");

            app._on_back_to_system();
            DotNetNativeTestSuite.AssertEqual("system", app.get_active_viewer(), "Object view should return to the system viewer in system studio");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestObjectEditPersistsBackToStandaloneSystem()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_main_menu_system_generation_requested();
            SystemGenerationScreen? screen = app.get_system_generation_screen();
            DotNetNativeTestSuite.AssertNotNull(screen, "System studio should exist");
            screen!.EmitSignal("start_system_generation", screen.GetCurrentSpec());

            StarGen.App.SystemViewer.SystemViewer? systemViewer = app.get_system_viewer();
            DotNetNativeTestSuite.AssertNotNull(systemViewer, "System viewer should exist");
            SolarSystem? system = systemViewer!.GetCurrentSystem();
            DotNetNativeTestSuite.AssertNotNull(system, "System viewer should have a system");
            CelestialBody originalBody = GetPreferredSystemBody(system!);

            app._on_open_in_object_viewer(originalBody);
            StarGen.App.Viewer.ObjectViewer? objectViewer = app.get_object_viewer();
            DotNetNativeTestSuite.AssertNotNull(objectViewer, "Object viewer should exist");

            CelestialBody? editedBody = CelestialSerializer.FromDictionary(CelestialSerializer.ToDictionary(originalBody));
            DotNetNativeTestSuite.AssertNotNull(editedBody, "Edited body clone should deserialize");
            editedBody!.Name = "Edited " + originalBody.Name;
            objectViewer!.EmitSignal(StarGen.App.Viewer.ObjectViewer.SignalName.BodyEdited, editedBody, 0);

            app._on_back_to_system();
            SolarSystem? updatedSystem = systemViewer.GetCurrentSystem();
            DotNetNativeTestSuite.AssertNotNull(updatedSystem, "System should remain available after returning from object view");
            CelestialBody? persistedBody = updatedSystem!.GetBody(originalBody.Id);
            DotNetNativeTestSuite.AssertNotNull(persistedBody, "Edited body should still exist in the system");
            DotNetNativeTestSuite.AssertEqual(editedBody.Name, persistedBody!.Name, "Standalone system edits should persist back into the system viewer");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestGalaxyViewerCanReturnToMainMenu()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_galaxy_viewer_main_menu_requested();
            DotNetNativeTestSuite.AssertEqual("menu", app.get_active_viewer(), "Galaxy viewer main-menu return should switch back to the menu");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSystemViewerFileMenuExposesNewSystemAndMainMenu()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(12345, Vector3.Zero);
            StarGen.App.SystemViewer.SystemViewer? viewer = app.get_system_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "System viewer should exist");
            PopupMenu? fileMenu = FindViewerFileMenu(viewer!);
            DotNetNativeTestSuite.AssertNotNull(fileMenu, "System viewer should expose a file menu");
            fileMenu!.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "New System"), "System viewer should expose a new-system file menu action");
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "Return to Main Menu"), "System viewer should expose a main-menu file menu action");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestObjectViewerFileMenuExposesNewObjectAndMainMenu()
    {
        MainApp app = CreateStartedApp();
        try
        {
            app._on_open_system_requested(12345, Vector3.Zero);
            CelestialBody body = IntegrationTestUtils.CreateTestBody(type: CelestialType.Type.Planet);
            app._on_open_in_object_viewer(body);
            StarGen.App.Viewer.ObjectViewer? viewer = app.get_object_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Object viewer should exist");
            PopupMenu? fileMenu = FindViewerFileMenu(viewer!);
            DotNetNativeTestSuite.AssertNotNull(fileMenu, "Object viewer should expose a file menu");
            fileMenu!.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "New Object"), "Object viewer should expose a new-object file menu action");
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "Return to Main Menu"), "Object viewer should expose a main-menu file menu action");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestOpenSystemRespectsRealisticPopulationSettings()
    {
        MainApp app = CreateStartedApp();
        try
        {
            StarGen.App.GalaxyViewer.GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");

            GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
            settings.LifePermissiveness = 1.0;

            GalaxyConfig config = GalaxyConfig.CreateDefault();
            config.UseCaseSettings = settings.Clone();
            viewer!.SetGalaxyConfig(config);

            int populatedSeed = 0;
            Vector3 worldPosition = new Vector3(8000.0f, 0.0f, 0.0f);
            for (int seedValue = 1; seedValue <= 400; seedValue += 1)
            {
                StarSystemPreviewData? preview = StarSystemPreview.Generate(
                    seedValue,
                    worldPosition,
                    viewer.get_spec()!,
                    settings);
                if (preview != null && preview.TotalPopulation > 0)
                {
                    populatedSeed = seedValue;
                    break;
                }
            }

            DotNetNativeTestSuite.AssertGreaterThan(
                populatedSeed,
                0,
                "A deterministic populated realistic system seed should exist in the scanned range");

            app._on_open_system_requested(populatedSeed, worldPosition);
            StarGen.App.SystemViewer.SystemViewer? systemViewer = app.get_system_viewer();
            DotNetNativeTestSuite.AssertNotNull(systemViewer, "System viewer should exist after opening a galaxy system");
            SolarSystem? system = systemViewer!.GetCurrentSystem();
            DotNetNativeTestSuite.AssertNotNull(system, "Galaxy-opened system should be generated");
            DotNetNativeTestSuite.AssertGreaterThan(
                system!.GetTotalPopulation(),
                0,
                "Galaxy-opened realistic systems should preserve populated worlds when permissiveness is high");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static CelestialBody GetPreferredSystemBody(SolarSystem system)
    {
        if (system.GetPlanets().Count > 0)
        {
            return system.GetPlanets()[0];
        }

        if (system.GetStars().Count > 0)
        {
            return system.GetStars()[0];
        }

        throw new InvalidOperationException("Expected the generated system to contain at least one body.");
    }

    private static PopupMenu? FindViewerFileMenu(Node viewer)
    {
        HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
        if (menuRow == null)
        {
            return null;
        }

        foreach (Node child in menuRow.GetChildren())
        {
            if (child is MenuButton menuButton && menuButton.Text == "File")
            {
                return menuButton.GetPopup();
            }
        }

        return null;
    }

    private static bool PopupContainsText(PopupMenu popupMenu, string text)
    {
        for (int index = 0; index < popupMenu.ItemCount; index += 1)
        {
            if (popupMenu.GetItemText(index).Contains(text))
            {
                return true;
            }
        }

        return false;
    }
}
