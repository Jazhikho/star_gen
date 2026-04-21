#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.GalaxyViewer;
using StarGen.App.Rendering;
using StarGen.App.SystemViewer;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Math;
using StarGen.Services.Persistence;

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Verifies stable color behavior for migrated rendering helpers.
    /// </summary>
    private static void TestColorUtilsSpectralAndBlackbodyBehavior()
    {
        Color gClass = ColorUtils.SpectralClassToColor("G2V");
        AssertFloatNear(1.0f, gClass.R, 1.0e-6f, "G-class red channel should match the expected palette");
        AssertFloatNear(1.0f, gClass.G, 1.0e-6f, "G-class green channel should match the expected palette");
        AssertFloatNear(0.8f, gClass.B, 1.0e-6f, "G-class blue channel should match the expected palette");

        Color cool = ColorUtils.TemperatureToBlackbodyColor(3000.0f);
        Color hot = ColorUtils.TemperatureToBlackbodyColor(12000.0f);
        AssertTrue(cool.R >= hot.R, "cooler stars should not be bluer in the red channel");
        AssertTrue(cool.B <= hot.B, "hotter stars should push more blue light");
    }

    /// <summary>
    /// Verifies stable display formatting for migrated UI helpers.
    /// </summary>
    private static void TestPropertyFormatterOutputsExpectedDisplayStrings()
    {
        string population = PropertyFormatter.FormatPopulation(1_234_567);
        AssertEqual("1.23M", population, "population formatting should use the M suffix");

        string habitability = PropertyFormatter.FormatHabitability(8);
        AssertEqual("8/10 (Comfortable)", habitability, "habitability formatting should include score and category");

        string distance = PropertyFormatter.FormatDistance(Units.AuMeters);
        AssertEqual("1.0000 AU", distance, "1 AU should format in astronomical units");
    }

    /// <summary>
    /// Verifies core material creation paths and caching for the migrated material factory.
    /// </summary>
    private static void TestMaterialFactoryCreatesExpectedMaterialsAndCaches()
    {
        MaterialFactory.ClearCache();

        Material defaultMaterial = MaterialFactory.CreateBodyMaterial(null);
        AssertTrue(defaultMaterial is StandardMaterial3D, "null bodies should use the default standard material");

        var star = CreateFixtureMaterialFactoryStarBody();
        Material firstMaterial = MaterialFactory.CreateBodyMaterial(star);
        Material secondMaterial = MaterialFactory.CreateBodyMaterial(star);

        AssertEqual(firstMaterial, secondMaterial, "repeated requests should reuse the cached material");
        AssertTrue(firstMaterial is ShaderMaterial, "star bodies should use the shader-material path");

        ShaderMaterial shaderMaterial = (ShaderMaterial)firstMaterial;
        AssertNotNull(shaderMaterial.Shader, "star materials should have a shader assigned");
        Variant temperatureVariant = shaderMaterial.GetShaderParameter("u_temperature");
        AssertTrue(IsNumericVariant(temperatureVariant), "star shader should expose a numeric temperature parameter");
        AssertFloatNear(5778.0, ToDouble(temperatureVariant), 0.01, "star shader should receive the stellar temperature");

        MaterialFactory.ClearCache();
    }

    /// <summary>
    /// Verifies stable scaling and orbit geometry for the system-viewer scale helper.
    /// </summary>
    private static void TestSystemScaleManagerDistanceAndOrbitMath()
    {
        SystemScaleManager scaleManager = new(Units.AuMeters);
        double units = scaleManager.DistanceToUnits(Units.AuMeters * 2.5);
        AssertFloatNear(2.5, units, 1.0e-9, "distance scaling should convert meters to viewport units");
        AssertFloatNear(Units.AuMeters * 2.5, scaleManager.UnitsToDistance(units), 1.0, "distance scaling should round-trip");

        Vector3[] points = scaleManager.GenerateOrbitPoints(
            Units.AuMeters,
            0.1,
            5.0,
            15.0,
            30.0,
            32);
        AssertEqual(33, points.Length, "orbit-point generation should include a closing point");
        AssertTrue(points[0].DistanceTo(points[^1]) < 1.0e-4f, "generated orbit should close cleanly");
    }

    /// <summary>
    /// Verifies sizing helpers and belt-layout generation for the migrated system layout engine.
    /// </summary>
    private static void TestSystemDisplayLayoutCalculatesExpectedSizingAndBelts()
    {
        AssertFloatNear(
            3.0,
            SystemDisplayLayout.CalculateStarDisplayRadius(Units.SolarRadiusMeters),
            0.01,
            "a solar-radius star should keep the expected display radius");
        AssertFloatNear(
            9.0,
            SystemDisplayLayout.CalculateFirstOrbitRadiusForStar(3.0f, 2.0f, 0.0f),
            0.01,
            "the first orbit radius should preserve the current surface-gap formula");

        var system = CreateFixtureDisplayLayoutSystemWithBelt();
        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);
        BeltLayout? beltLayout = layout.GetBeltLayout("belt_0");

        AssertNotNull(beltLayout, "belt layouts should be generated for asteroid belts");
        AssertTrue(beltLayout!.CenterDisplayRadius > 0.0f, "belt display center radius should be positive");
        AssertTrue(beltLayout.OuterDisplayRadius > beltLayout.InnerDisplayRadius, "belt display radii should expand outward");
        AssertFloatNear(2.0, beltLayout.InnerAu, 0.001, "belt layout should preserve the inner AU metadata");
        AssertFloatNear(3.0, beltLayout.OuterAu, 0.001, "belt layout should preserve the outer AU metadata");
    }

    /// <summary>
    /// Verifies zoom transitions and signal emission for the migrated galaxy-viewer state machine.
    /// </summary>
    private static void TestZoomStateMachineTransitionsAndSignal()
    {
        ZoomStateMachine zoomMachine = new();
        int signalCount = 0;
        int lastOldLevel = -1;
        int lastNewLevel = -1;
        zoomMachine.LevelChanged += (oldLevel, newLevel) =>
        {
            signalCount += 1;
            lastOldLevel = oldLevel;
            lastNewLevel = newLevel;
        };

        AssertEqual((int)GalaxyCoordinates.ZoomLevel.Galaxy, zoomMachine.GetCurrentLevel(), "zoom should start at galaxy level");
        AssertTrue(zoomMachine.CanZoomIn(), "fresh zoom machine should allow zooming in");
        AssertTrue(!zoomMachine.CanZoomOut(), "fresh zoom machine should not allow zooming out");

        zoomMachine.ZoomIn();
        AssertEqual((int)GalaxyCoordinates.ZoomLevel.Quadrant, zoomMachine.GetCurrentLevel(), "zoom in should advance one level");
        AssertEqual(1, signalCount, "zoom in should emit one level-change signal");
        AssertEqual((int)GalaxyCoordinates.ZoomLevel.Galaxy, lastOldLevel, "signal should include the previous level");
        AssertEqual((int)GalaxyCoordinates.ZoomLevel.Quadrant, lastNewLevel, "signal should include the new level");

        zoomMachine.SetLevel(999);
        AssertEqual((int)GalaxyCoordinates.ZoomLevel.Quadrant, zoomMachine.GetCurrentLevel(), "out-of-range levels should be ignored");

        zoomMachine.TransitionTo((int)GalaxyCoordinates.ZoomLevel.Subsector);
        AssertEqual((int)GalaxyCoordinates.ZoomLevel.Subsector, zoomMachine.GetCurrentLevel(), "explicit transitions should set the requested level");
        AssertTrue(!zoomMachine.CanZoomIn(), "subsector is the deepest zoom level");
        AssertTrue(zoomMachine.CanZoomOut(), "subsector should still allow zooming out");
    }

    /// <summary>
    /// Verifies nearest-hit quadrant picking for the migrated quadrant selector.
    /// </summary>
    private static void TestQuadrantSelectorPicksNearestOccupiedQuadrant()
    {
        QuadrantSelector selector = new();
        float quadrantSize = (float)GalaxyCoordinates.QuadrantSizePc;
        Godot.Collections.Array<Vector3I> occupied = [new Vector3I(0, 0, 0), new Vector3I(1, 0, 0)];

        Variant picked = selector.PickFromRay(
            new Vector3(-10.0f, quadrantSize * 0.5f, quadrantSize * 0.5f),
            Vector3.Right,
            occupied);
        AssertEqual(Variant.Type.Vector3I, picked.VariantType, "ray pick should return occupied quadrant coordinates");
        AssertEqual(new Vector3I(0, 0, 0), picked.AsVector3I(), "ray pick should choose the nearest intersected quadrant");

        selector.SetSelection(picked);
        AssertTrue(selector.HasSelection(), "explicit selection should be tracked");
        AssertEqual(new Vector3I(0, 0, 0), selector.SelectedCoords.AsVector3I(), "stored selection should match the chosen quadrant");

        selector.ClearSelection();
        AssertTrue(!selector.HasSelection(), "clearing selection should reset the selector");
    }

    /// <summary>
    /// Verifies the migrated selection-indicator wrappers preserve visible state.
    /// </summary>
    private static void TestSelectionIndicatorShowHideWrappers()
    {
        SelectionIndicator indicator = new();
        try
        {
            indicator._Ready();
            AssertTrue(!indicator.IsShown(), "selection indicator should start hidden");

            Vector3 firstPosition = new(1.0f, 2.0f, 3.0f);
            indicator.ShowAt(firstPosition);
            AssertTrue(indicator.IsShown(), "ShowAt should make the indicator visible");
            AssertFloatNear(firstPosition.X, indicator.Position.X, 1.0e-6, "ShowAt should update the x coordinate");

            Vector3 secondPosition = new(-4.0f, 5.0f, -6.0f);
            indicator.show_at(secondPosition);
            AssertTrue(indicator.is_shown(), "snake_case wrapper should also make the indicator visible");
            AssertFloatNear(secondPosition.Z, indicator.Position.Z, 1.0e-6, "snake_case wrapper should update the position");

            indicator.HideIndicator();
            AssertTrue(!indicator.IsShown(), "HideIndicator should hide the indicator");
            indicator.hide_indicator();
            AssertTrue(!indicator.is_shown(), "snake_case hide wrapper should leave the indicator hidden");
        }
        finally
        {
            indicator.Free();
        }
    }

    /// <summary>
    /// Verifies the galaxy viewer inspector hides the legacy profile and overview sections.
    /// </summary>
    private static void TestGalaxyInspectorPanelHidesLegacySections()
    {
        StudioUiPreferencesService.StudioUiPreferences originalPreferences = StudioUiPreferencesService.LoadOrDefault();
        PackedScene? scene = ResourceLoader.Load<PackedScene>("res://src/app/galaxy_viewer/GalaxyViewerCSharp.tscn");
        AssertNotNull(scene, "galaxy viewer scene should load for inspector testing");

        GalaxyViewer? viewer = scene!.Instantiate() as GalaxyViewer;
        AssertNotNull(viewer, "galaxy viewer scene should instantiate for inspector testing");

        try
        {
            StudioUiPreferencesService.Save(StudioUiPreferencesService.CreateDefault());
            viewer!._Ready();
            GalaxyInspectorPanel? panel = viewer.GetInspectorPanel();
            AssertNotNull(panel, "galaxy viewer should expose the typed inspector panel");
            AssertTrue(!panel!.IsConfigSectionVisible(), "galaxy viewer inspector should hide the legacy active-profile section");
            AssertTrue(!panel.IsOverviewSectionVisible(), "galaxy viewer inspector should hide the legacy overview section");
            AssertTrue(!panel.IsColonizationSectionVisible(), "galaxy viewer inspector should hide the jump-route tools section");

            SpinBox? seedInput = viewer.GetNodeOrNull<SpinBox>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput");
            AssertTrue(seedInput == null, "galaxy viewer should not expose the old top-level seed input");

            CheckBox? showCompassCheck = viewer.GetNodeOrNull<CheckBox>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection/ShowCompassCheck");
            AssertTrue(showCompassCheck == null, "galaxy viewer should not expose the old show-compass checkbox");

            Node? compass = viewer.GetNodeOrNull<Node>("UI/Compass");
            AssertTrue(compass == null, "galaxy viewer should not mount the unused compass viewport");

            Label? overviewTitle = viewer.GetNodeOrNull<Label>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel/SelectionSection/TitleLabel");
            AssertNotNull(overviewTitle, "galaxy viewer should still expose the overview title label");
            AssertEqual("Overview", overviewTitle!.Text, "galaxy viewer inspector should label the live location block as Overview");

            HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/UIRoot/TopBar/MarginContainer/TopBarVBox/MenuRow");
            AssertNotNull(menuRow, "galaxy viewer should expose the top menu row");
            AssertEqual(4, menuRow!.GetChildCount(), "galaxy viewer should expose File, Tools, Options, and Help in the menu row");
            AssertEqual("File", ((Button)menuRow.GetChild(0)).Text, "first top-level viewer menu should remain File");
            AssertEqual("Tools", ((Button)menuRow.GetChild(1)).Text, "second top-level viewer menu should be Tools");
            AssertEqual("Options", ((Button)menuRow.GetChild(2)).Text, "third top-level viewer menu should be Options");
            AssertEqual("Help", ((Button)menuRow.GetChild(3)).Text, "fourth top-level viewer menu should remain Help");

            Window? optionsDialog = viewer.GetNodeOrNull<Window>("OptionsDialog");
            AssertNotNull(optionsDialog, "galaxy viewer should expose the shared options dialog");
            CheckBox? showSeedControlsCheck = viewer.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/ShowSeedControlsCheck");
            CheckBox? skipIntroCheck = viewer.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/SkipIntroCheck");
            Button? applyButton = viewer.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/ApplyOptionsButton");
            Button? closeButton = viewer.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/CloseButton");
            AssertNotNull(showSeedControlsCheck, "galaxy viewer options should expose the studio-seed preference toggle");
            AssertNotNull(skipIntroCheck, "galaxy viewer options should expose the skip-intro checkbox");
            AssertNotNull(applyButton, "galaxy viewer options should expose the apply button");
            AssertNotNull(closeButton, "galaxy viewer options should expose the close button");
            AssertEqual("Show all studio seeds", showSeedControlsCheck!.Text, "galaxy viewer should label the seed toggle as showing all studio seeds");

            Label? optionsStatusLabel = viewer.GetNodeOrNull<Label>("OptionsDialog/MarginContainer/OptionsVBox/OptionsStatusLabel");
            AssertNotNull(optionsStatusLabel, "galaxy viewer options should expose the options status label");
            AssertTrue(optionsStatusLabel!.Text.Contains("All studio seeds"), "galaxy viewer options status should describe the all-studio-seeds preference");

            Button? optionsButton = menuRow.GetChild(2) as Button;
            AssertNotNull(optionsButton, "galaxy viewer should expose an Options menu action button");
            optionsButton!.EmitSignal(BaseButton.SignalName.Pressed);
            AssertTrue(optionsDialog!.Visible, "pressing Options should show the viewer options dialog");
            showSeedControlsCheck.ButtonPressed = true;
            skipIntroCheck!.ButtonPressed = true;
            applyButton!.EmitSignal(BaseButton.SignalName.Pressed);
            StudioUiPreferencesService.StudioUiPreferences updatedPreferences = StudioUiPreferencesService.LoadOrDefault();
            AssertTrue(updatedPreferences.ShowSeedControls, "galaxy viewer apply should persist the all-studio-seeds preference");
            AssertTrue(updatedPreferences.SkipIntro, "galaxy viewer apply should persist the skip-intro preference");
            AssertFalse(optionsDialog.Visible, "galaxy viewer apply should close the viewer options dialog");
            optionsButton.EmitSignal(BaseButton.SignalName.Pressed);
            closeButton!.EmitSignal(BaseButton.SignalName.Pressed);
            AssertFalse(optionsDialog.Visible, "galaxy viewer close button should close the viewer options dialog");
            optionsButton.EmitSignal(BaseButton.SignalName.Pressed);
            optionsDialog.EmitSignal(Window.SignalName.CloseRequested);
            AssertTrue(!optionsDialog.Visible, "the viewer options dialog should close when the window close signal is emitted");

            Window? localSpaceDialog = viewer.GetNodeOrNull<Window>("BuildLocalSpaceDialog");
            AssertNotNull(localSpaceDialog, "galaxy viewer should expose the build-local-space dialog");
            Control? cameraPanel = viewer.GetNodeOrNull<Control>("UI/UIRoot/CameraPanel");
            Button? cameraHeaderButton = viewer.GetNodeOrNull<Button>("UI/UIRoot/CameraPanel/CameraPanelVBox/CameraPanelHeaderButton");
            Control? cameraPanelContent = viewer.GetNodeOrNull<Control>("UI/UIRoot/CameraPanel/CameraPanelVBox/CameraPanelContent");
            AssertNotNull(cameraPanel, "galaxy viewer should expose the compact camera panel");
            AssertNotNull(cameraHeaderButton, "galaxy viewer camera panel should expose the collapse toggle");
            AssertNotNull(cameraPanelContent, "galaxy viewer camera panel should expose collapsible content");
            AssertEqual("> Camera", cameraHeaderButton!.Text, "galaxy viewer camera panel should start collapsed");
            AssertFalse(cameraPanelContent!.Visible, "galaxy viewer camera panel should start collapsed");

            bool builtLocalSpace = viewer.BuildLocalSpaceSynchronouslyForTesting(new Vector3I(1, 1, 1));
            AssertTrue(builtLocalSpace, "galaxy viewer should be able to build a local-space cache in subsector view");
            GalaxyLocalSpaceCache? localSpaceCache = viewer.GetLocalSpaceCache();
            AssertNotNull(localSpaceCache, "local-space build should retain a cache profile");
            AssertTrue(localSpaceCache!.Region.GetSystemCount() > 0, "local-space cache should contain generated systems");
        }
        finally
        {
            viewer?.QueueFree();
            StudioUiPreferencesService.Save(originalPreferences);
        }
    }

    /// <summary>
    /// Verifies local-space cache coverage persists across movement and subsequent builds append systems.
    /// </summary>
    private static void TestGalaxyViewerLocalSpaceCachePersistsAndAppends()
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>("res://src/app/galaxy_viewer/GalaxyViewerCSharp.tscn");
        AssertNotNull(scene, "galaxy viewer scene should load for local-space cache testing");

        GalaxyViewer? viewer = scene!.Instantiate() as GalaxyViewer;
        AssertNotNull(viewer, "galaxy viewer scene should instantiate for local-space cache testing");

        try
        {
            viewer!._Ready();
            Vector3I testExtent = new Vector3I(1, 1, 1);
            bool initialBuild = viewer.BuildLocalSpaceSynchronouslyForTesting(testExtent);
            AssertTrue(initialBuild, "initial local-space build should succeed");

            GalaxyLocalSpaceCache? initialCache = viewer.GetLocalSpaceCache();
            AssertNotNull(initialCache, "initial local-space build should create a cache");

            int initialSystemCount = initialCache!.Region.GetSystemCount();
            AssertEqual(1, initialCache.CoverageAreaCount, "initial local-space cache should track one coverage area");
            AssertTrue(!string.IsNullOrEmpty(viewer.GetVisibleJumpRouteRegionId()), "current position should be covered immediately after the initial build");

            StarViewCamera? starCamera = viewer.GetStarCamera();
            AssertNotNull(starCamera, "galaxy viewer should expose the star camera for movement testing");

            Vector3 initialPosition = starCamera!.GetCurrentPosition();
            Vector3 shiftedPosition = initialPosition + new Vector3((float)GalaxyCoordinates.SubsectorSizePc * 4.0f, 0.0f, 0.0f);
            starCamera.Configure(shiftedPosition);

            GalaxyLocalSpaceCache? cacheAfterMove = viewer.GetLocalSpaceCache();
            AssertNotNull(cacheAfterMove, "moving outside cached coverage should not delete the cache");
            AssertEqual(initialSystemCount, cacheAfterMove!.Region.GetSystemCount(), "moving should not change cached system count");
            AssertTrue(cacheAfterMove.ContainsPosition(initialPosition), "cache should still cover the original built position after movement");
            AssertTrue(!cacheAfterMove.ContainsPosition(shiftedPosition), "shifted position should start outside the original coverage");
            AssertTrue(string.IsNullOrEmpty(viewer.GetVisibleJumpRouteRegionId()), "jump-route region should be unavailable until the new position is built into the cache");

            bool appendedBuild = viewer.BuildLocalSpaceSynchronouslyForTesting(testExtent);
            AssertTrue(appendedBuild, "second local-space build should succeed after movement");

            GalaxyLocalSpaceCache? appendedCache = viewer.GetLocalSpaceCache();
            AssertNotNull(appendedCache, "cache should still exist after appending a new area");
            AssertEqual(2, appendedCache!.CoverageAreaCount, "second build at a new location should append a second coverage area");
            AssertTrue(appendedCache.Region.GetSystemCount() > initialSystemCount, "appending a second local-space build should grow the cached system set");
            AssertTrue(appendedCache.ContainsPosition(initialPosition), "expanded cache should still cover the original built position");
            AssertTrue(appendedCache.ContainsPosition(shiftedPosition), "expanded cache should also cover the newly built position");
            AssertTrue(!string.IsNullOrEmpty(viewer.GetVisibleJumpRouteRegionId()), "jump-route region should become available again once the new position is cached");
        }
        finally
        {
            viewer?.QueueFree();
        }
    }

    /// <summary>
    /// Verifies the main-menu options dialog applies persisted preferences and closes from both affordances.
    /// </summary>
    private static void TestMainMenuOptionsDialogAppliesAndCloses()
    {
        StudioUiPreferencesService.StudioUiPreferences originalPreferences = StudioUiPreferencesService.LoadOrDefault();
        try
        {
            StudioUiPreferencesService.Save(StudioUiPreferencesService.CreateDefault());

            PackedScene? scene = ResourceLoader.Load<PackedScene>("res://src/app/MainMenuScreen.tscn");
            AssertNotNull(scene, "main menu scene should load for options testing");

            MainMenuScreen? screen = scene!.Instantiate() as MainMenuScreen;
            AssertNotNull(screen, "main menu scene should instantiate for options testing");

            try
            {
                screen!._Ready();
                Button? optionsButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/OptionsButton");
                Window? optionsDialog = screen.GetNodeOrNull<Window>("OptionsDialog");
                CheckBox? showSeedsCheck = screen.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/ShowSeedControlsCheck");
                CheckBox? skipIntroCheck = screen.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/SkipIntroCheck");
                Button? applyButton = screen.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ApplyOptionsButton");
                Button? closeButton = screen.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/CloseButton");

                AssertNotNull(optionsButton, "main menu should expose the options button");
                AssertNotNull(optionsDialog, "main menu should expose the options dialog");
                AssertNotNull(showSeedsCheck, "main menu options should expose the show-seeds toggle");
                AssertNotNull(skipIntroCheck, "main menu options should expose the skip-intro toggle");
                AssertNotNull(applyButton, "main menu options should expose the apply button");
                AssertNotNull(closeButton, "main menu options should expose the close button");
                AssertEqual("Show all studio seeds", showSeedsCheck!.Text, "main menu should use the all-studio-seeds wording");

                optionsButton!.EmitSignal(BaseButton.SignalName.Pressed);
                AssertTrue(optionsDialog!.Visible, "main menu options should open from the options button");

                showSeedsCheck.ButtonPressed = true;
                skipIntroCheck!.ButtonPressed = true;
                applyButton!.EmitSignal(BaseButton.SignalName.Pressed);

                StudioUiPreferencesService.StudioUiPreferences updatedPreferences = StudioUiPreferencesService.LoadOrDefault();
                AssertTrue(updatedPreferences.ShowSeedControls, "main menu apply should persist the show-seeds preference");
                AssertTrue(updatedPreferences.SkipIntro, "main menu apply should persist the skip-intro preference");
                AssertFalse(optionsDialog.Visible, "main menu apply should close the options dialog after saving");

                optionsButton.EmitSignal(BaseButton.SignalName.Pressed);
                closeButton!.EmitSignal(BaseButton.SignalName.Pressed);
                AssertTrue(!optionsDialog.Visible, "main menu options should close from the explicit close button");

                optionsButton.EmitSignal(BaseButton.SignalName.Pressed);
                optionsDialog.EmitSignal(Window.SignalName.CloseRequested);
                AssertTrue(!optionsDialog.Visible, "main menu options should close from the titlebar close affordance");
            }
            finally
            {
                screen?.QueueFree();
            }
        }
        finally
        {
            StudioUiPreferencesService.Save(originalPreferences);
        }
    }

    /// <summary>
    /// Verifies the system viewer uses the shared viewer menu standard and options dialog.
    /// </summary>
    private static void TestSystemViewerMenuMatchesGalaxyViewerStandard()
    {
        StudioUiPreferencesService.StudioUiPreferences originalPreferences = StudioUiPreferencesService.LoadOrDefault();
        SystemViewer? viewer = null;
        try
        {
            StudioUiPreferencesService.Save(StudioUiPreferencesService.CreateDefault());

            PackedScene? scene = ResourceLoader.Load<PackedScene>("res://src/app/system_viewer/SystemViewer.tscn");
            AssertNotNull(scene, "system viewer scene should load for menu testing");

            viewer = scene!.Instantiate() as SystemViewer;
            AssertNotNull(viewer, "system viewer scene should instantiate for menu testing");

            viewer!._Ready();
            HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
            AssertNotNull(menuRow, "system viewer should expose the top menu row");
            AssertEqual(4, menuRow!.GetChildCount(), "system viewer should expose File, Tools, Options, and Help");
            AssertEqual("File", ((Button)menuRow.GetChild(0)).Text, "system viewer first menu should be File");
            AssertEqual("Tools", ((Button)menuRow.GetChild(1)).Text, "system viewer second menu should be Tools");
            AssertEqual("Options", ((Button)menuRow.GetChild(2)).Text, "system viewer third menu should be Options");
            AssertEqual("Help", ((Button)menuRow.GetChild(3)).Text, "system viewer fourth menu should be Help");

            Window? optionsDialog = viewer.GetNodeOrNull<Window>("OptionsDialog");
            CheckBox? showSeedsCheck = viewer.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/ShowSeedControlsCheck");
            Button? optionsButton = menuRow.GetChild(2) as Button;
            Button? closeButton = viewer.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/CloseButton");
            Button? applyButton = viewer.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/ApplyOptionsButton");
            CheckBox? skipIntroCheck = viewer.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/SkipIntroCheck");
            Control? cameraPanel = viewer.GetNodeOrNull<Control>("UI/CameraPanel");
            Button? cameraHeaderButton = viewer.GetNodeOrNull<Button>("UI/CameraPanel/CameraPanelVBox/CameraPanelHeaderButton");
            Control? cameraPanelContent = viewer.GetNodeOrNull<Control>("UI/CameraPanel/CameraPanelVBox/CameraPanelContent");

            AssertNotNull(optionsDialog, "system viewer should expose an options dialog");
            AssertNotNull(showSeedsCheck, "system viewer options should expose the all-studio-seeds toggle");
            AssertNotNull(closeButton, "system viewer options should expose the close button");
            AssertNotNull(applyButton, "system viewer options should expose the apply button");
            AssertNotNull(skipIntroCheck, "system viewer options should expose the skip-intro checkbox");
            AssertEqual("Show all studio seeds", showSeedsCheck!.Text, "system viewer should use the all-studio-seeds wording");
            AssertNotNull(cameraPanel, "system viewer should expose the compact camera panel");
            AssertNotNull(cameraHeaderButton, "system viewer camera panel should expose a collapse toggle");
            AssertNotNull(cameraPanelContent, "system viewer camera panel should expose collapsible content");
            AssertEqual("> Camera", cameraHeaderButton!.Text, "system viewer camera panel should start collapsed");
            AssertFalse(cameraPanelContent!.Visible, "system viewer camera panel should start collapsed");

            optionsButton!.EmitSignal(BaseButton.SignalName.Pressed);
            AssertTrue(optionsDialog!.Visible, "system viewer options should open from the options action");
            showSeedsCheck.ButtonPressed = true;
            skipIntroCheck!.ButtonPressed = true;
            applyButton!.EmitSignal(BaseButton.SignalName.Pressed);
            StudioUiPreferencesService.StudioUiPreferences preferences = StudioUiPreferencesService.LoadOrDefault();
            AssertTrue(preferences.ShowSeedControls, "system viewer apply should persist the all-studio-seeds preference");
            AssertTrue(preferences.SkipIntro, "system viewer apply should persist the skip-intro preference");
            AssertFalse(optionsDialog.Visible, "system viewer apply should close the dialog after saving");
            optionsButton.EmitSignal(BaseButton.SignalName.Pressed);
            closeButton!.EmitSignal(BaseButton.SignalName.Pressed);
            AssertTrue(!optionsDialog.Visible, "system viewer options should close from the explicit close button");
        }
        finally
        {
            viewer?.QueueFree();
            StudioUiPreferencesService.Save(originalPreferences);
        }
    }

    /// <summary>
    /// Verifies the object viewer uses the shared viewer menu standard and options dialog.
    /// </summary>
    private static void TestObjectViewerMenuMatchesGalaxyViewerStandard()
    {
        StudioUiPreferencesService.StudioUiPreferences originalPreferences = StudioUiPreferencesService.LoadOrDefault();
        ObjectViewer? viewer = null;
        try
        {
            StudioUiPreferencesService.Save(StudioUiPreferencesService.CreateDefault());

            PackedScene? scene = ResourceLoader.Load<PackedScene>("res://src/app/viewer/ObjectViewer.tscn");
            AssertNotNull(scene, "object viewer scene should load for menu testing");

            viewer = scene!.Instantiate() as ObjectViewer;
            AssertNotNull(viewer, "object viewer scene should instantiate for menu testing");

            viewer!._Ready();
            HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
            AssertNotNull(menuRow, "object viewer should expose the top menu row");
            AssertEqual(4, menuRow!.GetChildCount(), "object viewer should expose File, Tools, Options, and Help");
            AssertEqual("File", ((Button)menuRow.GetChild(0)).Text, "object viewer first menu should be File");
            AssertEqual("Tools", ((Button)menuRow.GetChild(1)).Text, "object viewer second menu should be Tools");
            AssertEqual("Options", ((Button)menuRow.GetChild(2)).Text, "object viewer third menu should be Options");
            AssertEqual("Help", ((Button)menuRow.GetChild(3)).Text, "object viewer fourth menu should be Help");

            Window? optionsDialog = viewer.GetNodeOrNull<Window>("OptionsDialog");
            CheckBox? showSeedsCheck = viewer.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/ShowSeedControlsCheck");
            Button? optionsButton = menuRow.GetChild(2) as Button;
            Button? closeButton = viewer.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/CloseButton");
            Button? applyButton = viewer.GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/ApplyOptionsButton");
            CheckBox? skipIntroCheck = viewer.GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/SkipIntroCheck");
            Control? cameraPanel = viewer.GetNodeOrNull<Control>("UI/CameraPanel");
            Button? cameraHeaderButton = viewer.GetNodeOrNull<Button>("UI/CameraPanel/CameraPanelVBox/CameraPanelHeaderButton");
            Control? cameraPanelContent = viewer.GetNodeOrNull<Control>("UI/CameraPanel/CameraPanelVBox/CameraPanelContent");

            AssertNotNull(optionsDialog, "object viewer should expose an options dialog");
            AssertNotNull(showSeedsCheck, "object viewer options should expose the all-studio-seeds toggle");
            AssertNotNull(closeButton, "object viewer options should expose the close button");
            AssertNotNull(applyButton, "object viewer options should expose the apply button");
            AssertNotNull(skipIntroCheck, "object viewer options should expose the skip-intro checkbox");
            AssertEqual("Show all studio seeds", showSeedsCheck!.Text, "object viewer should use the all-studio-seeds wording");
            AssertNotNull(cameraPanel, "object viewer should expose the compact camera panel");
            AssertNotNull(cameraHeaderButton, "object viewer camera panel should expose a collapse toggle");
            AssertNotNull(cameraPanelContent, "object viewer camera panel should expose collapsible content");
            AssertEqual("> Camera", cameraHeaderButton!.Text, "object viewer camera panel should start collapsed");
            AssertFalse(cameraPanelContent!.Visible, "object viewer camera panel should start collapsed");

            optionsButton!.EmitSignal(BaseButton.SignalName.Pressed);
            AssertTrue(optionsDialog!.Visible, "object viewer options should open from the options action");
            showSeedsCheck.ButtonPressed = true;
            skipIntroCheck!.ButtonPressed = true;
            applyButton!.EmitSignal(BaseButton.SignalName.Pressed);
            StudioUiPreferencesService.StudioUiPreferences preferences = StudioUiPreferencesService.LoadOrDefault();
            AssertTrue(preferences.ShowSeedControls, "object viewer apply should persist the all-studio-seeds preference");
            AssertTrue(preferences.SkipIntro, "object viewer apply should persist the skip-intro preference");
            AssertFalse(optionsDialog.Visible, "object viewer apply should close the dialog after saving");
            optionsButton.EmitSignal(BaseButton.SignalName.Pressed);
            closeButton!.EmitSignal(BaseButton.SignalName.Pressed);
            AssertTrue(!optionsDialog.Visible, "object viewer options should close from the explicit close button");
        }
        finally
        {
            viewer?.QueueFree();
            StudioUiPreferencesService.Save(originalPreferences);
        }
    }

    /// <summary>
    /// Verifies realism-profile slider mapping and preset constructors.
    /// </summary>
    private static void TestGenerationRealismProfileSliderAndPresets()
    {
        GenerationRealismProfile stylized = GenerationRealismProfile.FromSlider(0.1);
        GenerationRealismProfile balanced = GenerationRealismProfile.FromSlider(0.5);
        GenerationRealismProfile calibrated = GenerationRealismProfile.FromSlider(0.9);

        AssertEqual(GenerationRealismProfile.ModeType.Stylized, stylized.Mode, "low slider values should map to stylized mode");
        AssertEqual(GenerationRealismProfile.ModeType.Balanced, balanced.Mode, "mid slider values should map to balanced mode");
        AssertEqual(GenerationRealismProfile.ModeType.Calibrated, calibrated.Mode, "high slider values should map to calibrated mode");

        AssertFloatNear(0.0, GenerationRealismProfile.Stylized().RealismSlider, 1.0e-9, "stylized preset should pin the slider to 0");
        AssertFloatNear(0.5, GenerationRealismProfile.Balanced().RealismSlider, 1.0e-9, "balanced preset should pin the slider to 0.5");
        AssertFloatNear(1.0, GenerationRealismProfile.Calibrated().RealismSlider, 1.0e-9, "calibrated preset should pin the slider to 1");
    }
}
