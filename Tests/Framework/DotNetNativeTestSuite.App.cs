#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.GalaxyViewer;
using StarGen.App.Rendering;
using StarGen.App.SystemViewer;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Math;

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
        PackedScene? scene = ResourceLoader.Load<PackedScene>("res://src/app/galaxy_viewer/GalaxyViewerCSharp.tscn");
        AssertNotNull(scene, "galaxy viewer scene should load for inspector testing");

        GalaxyViewer? viewer = scene!.Instantiate() as GalaxyViewer;
        AssertNotNull(viewer, "galaxy viewer scene should instantiate for inspector testing");

        try
        {
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

            Window? localSpaceDialog = viewer.GetNodeOrNull<Window>("BuildLocalSpaceDialog");
            AssertNotNull(localSpaceDialog, "galaxy viewer should expose the build-local-space dialog");

            bool builtLocalSpace = viewer.BuildLocalSpaceSynchronouslyForTesting(new Vector3I(1, 1, 1));
            AssertTrue(builtLocalSpace, "galaxy viewer should be able to build a local-space cache in subsector view");
            GalaxyLocalSpaceCache? localSpaceCache = viewer.GetLocalSpaceCache();
            AssertNotNull(localSpaceCache, "local-space build should retain a cache profile");
            AssertTrue(localSpaceCache!.Region.GetSystemCount() > 0, "local-space cache should contain generated systems");
        }
        finally
        {
            viewer?.QueueFree();
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
