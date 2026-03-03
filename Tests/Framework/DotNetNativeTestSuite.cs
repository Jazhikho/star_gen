using System;
using System.IO;
using StarGen.App.GalaxyViewer;
using Godot;
using Godot.Collections;
using StarGen.App.Rendering;
using StarGen.App.SystemViewer;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Services.Persistence;

namespace StarGen.Tests.Framework;

/// <summary>
/// Native C# tests that exercise migrated code paths directly without the GDScript bridge.
/// </summary>
public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Runs the current native C# test tranche through the shared .NET runner.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_stable_hash_seed_chain_consistency",
            TestStableHashSeedChainConsistency);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_color_utils_spectral_and_blackbody_behavior",
            TestColorUtilsSpectralAndBlackbodyBehavior);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_property_formatter_outputs_expected_display_strings",
            TestPropertyFormatterOutputsExpectedDisplayStrings);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_material_factory_creates_expected_materials_and_caches",
            TestMaterialFactoryCreatesExpectedMaterialsAndCaches);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_system_scale_manager_distance_and_orbit_math",
            TestSystemScaleManagerDistanceAndOrbitMath);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_system_display_layout_calculates_expected_sizing_and_belts",
            TestSystemDisplayLayoutCalculatesExpectedSizingAndBelts);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_zoom_state_machine_transitions_and_signal",
            TestZoomStateMachineTransitionsAndSignal);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_quadrant_selector_picks_nearest_occupied_quadrant",
            TestQuadrantSelectorPicksNearestOccupiedQuadrant);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_selection_indicator_show_hide_wrappers",
            TestSelectionIndicatorShowHideWrappers);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_config_round_trip_and_validation",
            TestGalaxyConfigRoundTripAndValidation);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_coordinates_hierarchy_and_centers",
            TestGalaxyCoordinatesHierarchyAndCenters);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_raycast_utils_hits_and_misses_aabb",
            TestRaycastUtilsHitsAndMissesAabb);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_grid_cursor_navigation_snaps_to_nearest_targets",
            TestGridCursorNavigationSnapsToNearestTargets);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_star_round_trip_and_derived_properties",
            TestGalaxyStarRoundTripAndDerivedProperties);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_spiral_density_model_center_exceeds_outer_disk_density",
            TestSpiralDensityModelCenterExceedsOuterDiskDensity);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_density_sampler_is_deterministic_for_fixed_seed",
            TestDensitySamplerIsDeterministicForFixedSeed);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_star_picker_selects_nearest_star_to_ray",
            TestStarPickerSelectsNearestStarToRay);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_subsector_generator_is_deterministic_for_fixed_inputs",
            TestSubsectorGeneratorIsDeterministicForFixedInputs);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_subsector_neighborhood_is_deterministic_for_fixed_inputs",
            TestSubsectorNeighborhoodIsDeterministicForFixedInputs);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_star_system_preview_generates_consistent_preview",
            TestStarSystemPreviewGeneratesConsistentPreview);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_generation_realism_profile_slider_and_presets",
            TestGenerationRealismProfileSliderAndPresets);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_orbital_mechanics_period_axis_round_trip",
            TestOrbitalMechanicsPeriodAxisRoundTrip);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_orbit_host_zone_calculation_and_round_trip",
            TestOrbitHostZoneCalculationAndRoundTrip);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_solar_system_spec_presets_and_round_trip",
            TestSolarSystemSpecPresetsAndRoundTrip);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_hierarchy_node_round_trip_and_tree_queries",
            TestHierarchyNodeRoundTripAndTreeQueries);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_system_hierarchy_round_trip_and_node_filters",
            TestSystemHierarchyRoundTripAndNodeFilters);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_solar_system_add_body_updates_indexes",
            TestSolarSystemAddBodyUpdatesIndexes);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_sector_generation_is_stable_for_repeated_access",
            TestSectorGenerationIsStableForRepeatedAccess);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_caches_sectors_and_systems",
            TestGalaxyCachesSectorsAndSystems);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_home_position_stays_within_default_galaxy_bounds",
            TestHomePositionStaysWithinDefaultGalaxyBounds);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_save_data_round_trip_preserves_payload",
            TestGalaxySaveDataRoundTripPreservesPayload);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_body_overrides_round_trip_and_apply",
            TestGalaxyBodyOverridesRoundTripAndApply);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_system_cache_put_get_and_evict",
            TestSystemCachePutGetAndEvict);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_viewer_save_load_clears_saved_state",
            TestGalaxyViewerSaveLoadClearsSavedState);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_system_viewer_save_load_round_trips_json_path",
            TestSystemViewerSaveLoadRoundTripsJsonPath);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_star_generator_deterministic_same_seed",
            TestStarGeneratorDeterministicSameSeed);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_planet_generator_deterministic_same_seed",
            TestPlanetGeneratorDeterministicSameSeed);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_moon_generator_deterministic_same_seed",
            TestMoonGeneratorDeterministicSameSeed);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_celestial_serializer_round_trip_preserves_star_payload",
            TestCelestialSerializerRoundTripPreservesStarPayload);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_ring_system_generator_deterministic_same_seed",
            TestRingSystemGeneratorDeterministicSameSeed);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_asteroid_generator_deterministic_same_seed",
            TestAsteroidGeneratorDeterministicSameSeed);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_system_serializer_round_trip_preserves_system_payload",
            TestSystemSerializerRoundTripPreservesSystemPayload);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_galaxy_system_generator_deterministic_same_star_seed",
            TestGalaxySystemGeneratorDeterministicSameStarSeed);
    }

    /// <summary>
    /// Verifies that hierarchical seed derivation stays internally consistent.
    /// </summary>
    private static void TestStableHashSeedChainConsistency()
    {
        long galaxySeed = 42_4242;
        Vector3I quadrantCoords = new(1, 2, 3);
        Vector3I sectorCoords = new(4, 5, 6);
        Vector3I subsectorCoords = new(7, 8, 9);

        long quadrantSeed = SeedDeriver.DeriveQuadrantSeed(galaxySeed, quadrantCoords);
        long sectorSeed = SeedDeriver.DeriveSectorSeed(quadrantSeed, sectorCoords);
        long subsectorSeed = SeedDeriver.DeriveSubsectorSeed(sectorSeed, subsectorCoords);

        AssertEqual(
            SeedDeriver.DeriveSectorSeedFull(galaxySeed, quadrantCoords, sectorCoords),
            sectorSeed,
            "full sector derivation should match the stepwise chain");
        AssertEqual(
            SeedDeriver.DeriveSubsectorSeedFull(galaxySeed, quadrantCoords, sectorCoords, subsectorCoords),
            subsectorSeed,
            "full subsector derivation should match the stepwise chain");
        AssertNotEqual(quadrantSeed, sectorSeed, "derived child seeds should change across hierarchy levels");
    }

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

        CelestialBody star = CreateFixtureMaterialFactoryStarBody();
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

        SolarSystem system = CreateFixtureDisplayLayoutSystemWithBelt();
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
        Array<Vector3I> occupied = [new Vector3I(0, 0, 0), new Vector3I(1, 0, 0)];

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

    /// <summary>
    /// Verifies galaxy-configuration validation, display naming, and round-trip behavior.
    /// </summary>
    private static void TestGalaxyConfigRoundTripAndValidation()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        AssertTrue(config.IsValid(), "Milky-Way config should be valid");
        AssertEqual("Spiral", config.GetTypeName(), "Milky-Way config should report the spiral type name");

        Dictionary data = config.ToDictionary();
        GalaxyConfig? rebuilt = GalaxyConfig.FromDictionary(data);
        AssertNotNull(rebuilt, "galaxy config should rebuild from its dictionary payload");
        AssertVariantDeepEqual(data, rebuilt!.ToDictionary(), "galaxy config should round-trip semantically unchanged");

        rebuilt.NumArms = 1;
        AssertTrue(!rebuilt.IsValid(), "invalid arm-count changes should fail validation");
    }

    /// <summary>
    /// Verifies key hierarchy conversions and center calculations for galaxy coordinates.
    /// </summary>
    private static void TestGalaxyCoordinatesHierarchyAndCenters()
    {
        Vector3 position = new(1234.5f, 67.8f, -987.6f);
        Vector3I quadrant = GalaxyCoordinates.ParsecToQuadrant(position);
        AssertEqual(new Vector3I(1, 0, -1), quadrant, "parsec-to-quadrant conversion should floor into the expected cell");

        HierarchyCoords hierarchy = GalaxyCoordinates.ParsecToHierarchy(position);
        AssertEqual(quadrant, hierarchy.QuadrantCoords, "hierarchy conversion should preserve quadrant coordinates");
        AssertTrue(
            hierarchy.SectorLocalCoords.X >= 0 && hierarchy.SectorLocalCoords.X <= 9,
            "sector local x should stay in the expected 0-9 range");
        AssertTrue(
            hierarchy.SubsectorLocalCoords.Z >= 0 && hierarchy.SubsectorLocalCoords.Z <= 9,
            "subsector local z should stay in the expected 0-9 range");

        Vector3 quadrantCenter = GalaxyCoordinates.QuadrantToParsecCenter(Vector3I.Zero);
        AssertFloatNear(500.0, quadrantCenter.X, 1.0e-6, "quadrant center should be offset by half a quadrant");

        Vector3 subsectorCenter = GalaxyCoordinates.GetSubsectorWorldCenter(position);
        AssertTrue(
            subsectorCenter.DistanceTo(GalaxyCoordinates.GetSubsectorWorldOrigin(position)) > 0.0f,
            "subsector center should differ from its origin by a positive offset");
    }

    /// <summary>
    /// Verifies slab-based AABB ray tests for the migrated raycast helper.
    /// </summary>
    private static void TestRaycastUtilsHitsAndMissesAabb()
    {
        float hit = RaycastUtils.RayIntersectsAabb(
            new Vector3(-5.0f, 0.5f, 0.5f),
            Vector3.Right,
            Vector3.Zero,
            Vector3.One);
        AssertTrue(hit >= 0.0f, "ray aimed through the box should report a hit");
        AssertFloatNear(5.0, hit, 1.0e-6, "hit distance should match the first slab intersection");

        float miss = RaycastUtils.RayIntersectsAabb(
            new Vector3(-5.0f, 5.0f, 0.5f),
            Vector3.Right,
            Vector3.Zero,
            Vector3.One);
        AssertEqual(RaycastUtils.NoHit, miss, "ray outside the slabs should miss the box");
    }

    /// <summary>
    /// Verifies nearest-target snapping for the migrated grid cursor.
    /// </summary>
    private static void TestGridCursorNavigationSnapsToNearestTargets()
    {
        GridCursor cursor = new()
        {
            Position = Vector3I.Zero,
        };
        Vector3I[] occupied =
        [
            new Vector3I(3, 0, 0),
            new Vector3I(1, 0, 0),
            new Vector3I(0, 4, 0),
            new Vector3I(-2, 0, 0),
        ];

        Vector3I? right = cursor.MoveInDirection(Vector3I.Right, occupied);
        AssertTrue(right.HasValue, "cursor should find a target to the right");
        AssertEqual(new Vector3I(1, 0, 0), right!.Value, "cursor should choose the nearest matching target in direction");

        cursor.Position = new Vector3I(1, 2, 0);
        Vector3I? nearest = cursor.SnapToNearest(occupied);
        AssertTrue(nearest.HasValue, "cursor should snap to the nearest occupied point");
        AssertEqual(new Vector3I(1, 0, 0), nearest!.Value, "snap should choose the closest occupied point");
    }

    /// <summary>
    /// Verifies galaxy-star derivation and dictionary round-trip behavior.
    /// </summary>
    private static void TestGalaxyStarRoundTripAndDerivedProperties()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(123_321);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(new Vector3(1200.0f, 15.0f, -800.0f), 111_111, spec);

        AssertTrue(star.Metallicity >= 0.1 && star.Metallicity <= 3.0, "derived metallicity should stay within the supported range");
        AssertTrue(star.AgeBias >= 0.5 && star.AgeBias <= 2.0, "derived age bias should stay within the supported range");

        Dictionary data = star.ToDictionary();
        GalaxyStar? rebuilt = GalaxyStar.FromDictionary(data);
        AssertNotNull(rebuilt, "galaxy star should rebuild from its serialized payload");
        AssertVariantDeepEqual(data, rebuilt!.ToDictionary(), "galaxy star should round-trip semantically unchanged");
    }

    /// <summary>
    /// Verifies that spiral density peaks near the core and falls off in the outer disk.
    /// </summary>
    private static void TestSpiralDensityModelCenterExceedsOuterDiskDensity()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(234_567);
        SpiralDensityModel model = new(spec);

        float coreDensity = model.GetDensity(Vector3.Zero);
        float outerDensity = model.GetDensity(new Vector3((float)spec.RadiusPc, 0.0f, 0.0f));

        AssertTrue(coreDensity > outerDensity, "spiral density should be stronger near the galactic core");
        AssertTrue(coreDensity > 0.0f, "spiral core density should be positive");
        AssertTrue(model.GetPeakDensity() >= coreDensity, "peak density should bound the sampled core density");
    }

    /// <summary>
    /// Verifies deterministic point sampling for a fixed galaxy spec and RNG seed.
    /// </summary>
    private static void TestDensitySamplerIsDeterministicForFixedSeed()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(345_678);
        GalaxySample first = DensitySampler.SampleGalaxy(spec, 128, new SeededRng(456_789));
        GalaxySample second = DensitySampler.SampleGalaxy(spec, 128, new SeededRng(456_789));

        AssertEqual(first.GetTotalCount(), second.GetTotalCount(), "sampled point count should be deterministic");
        AssertEqual(first.BulgePoints.Length, second.BulgePoints.Length, "bulge sample count should be deterministic");
        AssertEqual(first.DiskPoints.Length, second.DiskPoints.Length, "disk sample count should be deterministic");

        if (first.BulgePoints.Length > 0)
        {
            AssertTrue(first.BulgePoints[0] == second.BulgePoints[0], "bulge point positions should match for identical seeds");
        }

        if (first.DiskPoints.Length > 0)
        {
            AssertTrue(first.DiskPoints[0] == second.DiskPoints[0], "disk point positions should match for identical seeds");
        }
    }

    /// <summary>
    /// Verifies nearest-to-ray picking for the migrated star-picker helper.
    /// </summary>
    private static void TestStarPickerSelectsNearestStarToRay()
    {
        Vector3[] positions =
        [
            new Vector3(10.0f, 0.1f, 0.0f),
            new Vector3(12.0f, 2.0f, 0.0f),
            new Vector3(15.0f, 0.05f, 0.0f),
        ];
        long[] starSeeds = [101L, 202L, 303L];

        StarPickResult? result = StarPicker.PickNearestToRay(
            Vector3.Zero,
            Vector3.Right,
            positions,
            starSeeds,
            0.2f);

        AssertNotNull(result, "star picker should find a visible star near the ray");
        AssertEqual(2, result!.StarIndex, "star picker should choose the star with the smallest lateral distance");
        AssertEqual(303L, result.StarSeed, "star picker should preserve the matching seed");
    }

    /// <summary>
    /// Verifies deterministic subsector generation for fixed seeds and density inputs.
    /// </summary>
    private static void TestSubsectorGeneratorIsDeterministicForFixedInputs()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(567_890);
        SpiralDensityModel densityModel = new(spec);
        float referenceDensity = densityModel.GetDensity(HomePosition.GetDefaultPosition());

        SectorStarData first = SubSectorGenerator.GenerateSingleSubsector(
            678_901,
            HomePosition.GetHomeSubsectorCenter() - (Vector3.One * (float)(GalaxyCoordinates.SubsectorSizePc * 0.5)),
            densityModel,
            referenceDensity);
        SectorStarData second = SubSectorGenerator.GenerateSingleSubsector(
            678_901,
            HomePosition.GetHomeSubsectorCenter() - (Vector3.One * (float)(GalaxyCoordinates.SubsectorSizePc * 0.5)),
            densityModel,
            referenceDensity);

        AssertEqual(first.GetCount(), second.GetCount(), "subsector star count should be deterministic");
        AssertEqual(first.StarSeeds.Length, second.StarSeeds.Length, "subsector seed count should be deterministic");
        if (first.GetCount() > 0)
        {
            AssertTrue(first.Positions[0] == second.Positions[0], "subsector positions should match for identical inputs");
            AssertEqual(first.StarSeeds[0], second.StarSeeds[0], "subsector star seeds should match for identical inputs");
        }
    }

    /// <summary>
    /// Verifies deterministic neighborhood generation around a fixed camera position.
    /// </summary>
    private static void TestSubsectorNeighborhoodIsDeterministicForFixedInputs()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(789_012);
        SpiralDensityModel densityModel = new(spec);
        Vector3 cameraPosition = HomePosition.GetHomeSubsectorCenter();
        float referenceDensity = densityModel.GetDensity(cameraPosition);

        SubSectorNeighborhoodData first = SubSectorNeighborhood.Build(cameraPosition, 890_123, densityModel, referenceDensity);
        SubSectorNeighborhoodData second = SubSectorNeighborhood.Build(cameraPosition, 890_123, densityModel, referenceDensity);

        AssertEqual(first.SubsectorOrigins.Length, second.SubsectorOrigins.Length, "subsector origin counts should be deterministic");
        AssertEqual(first.StarPositions.Length, second.StarPositions.Length, "neighborhood star counts should be deterministic");
        AssertTrue(first.CenterOrigin == second.CenterOrigin, "center origin should be deterministic");
        if (first.StarPositions.Length > 0)
        {
            AssertTrue(first.StarPositions[0] == second.StarPositions[0], "neighborhood star positions should match");
            AssertEqual(first.StarSeeds[0], second.StarSeeds[0], "neighborhood star seeds should match");
        }
    }

    /// <summary>
    /// Verifies consistent system-preview generation for a fixed star seed and position.
    /// </summary>
    private static void TestStarSystemPreviewGeneratesConsistentPreview()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(901_234);
        Vector3 worldPosition = HomePosition.GetDefaultPosition();

        StarSystemPreviewData? first = StarSystemPreview.Generate(246_810, worldPosition, spec);
        StarSystemPreviewData? second = StarSystemPreview.Generate(246_810, worldPosition, spec);

        AssertNotNull(first, "preview generation should produce a result for a valid star seed");
        AssertNotNull(second, "preview generation should be repeatable for a valid star seed");
        AssertEqual(first!.StarSeed, second!.StarSeed, "preview seed should remain stable");
        AssertEqual(first.StarCount, second.StarCount, "preview star count should be deterministic");
        AssertEqual(first.PlanetCount, second.PlanetCount, "preview planet count should be deterministic");
        AssertEqual(first.IsInhabited, second.IsInhabited, "preview inhabited state should be deterministic");
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

    /// <summary>
    /// Verifies the key Kepler conversions in the migrated orbital-mechanics helper.
    /// </summary>
    private static void TestOrbitalMechanicsPeriodAxisRoundTrip()
    {
        double originalAxis = Units.AuMeters;
        double period = OrbitalMechanics.CalculateOrbitalPeriod(originalAxis, Units.SolarMassKg);
        double rebuiltAxis = OrbitalMechanics.CalculateSemiMajorAxis(period, Units.SolarMassKg);

        AssertTrue(period > 0.0, "orbital period should be positive for valid inputs");
        AssertFloatNear(originalAxis, rebuiltAxis, Units.AuMeters * 1.0e-9, "period/axis conversions should round-trip");
        AssertTrue(
            OrbitalMechanics.CalculateOrbitalVelocity(originalAxis, Units.SolarMassKg) > 0.0,
            "orbital velocity should be positive for valid inputs");
    }

    /// <summary>
    /// Verifies orbit-host zone calculations and dictionary round-tripping.
    /// </summary>
    private static void TestOrbitHostZoneCalculationAndRoundTrip()
    {
        OrbitHost host = new("primary", OrbitHost.HostType.SType)
        {
            CombinedMassKg = Units.SolarMassKg,
            CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts,
            EffectiveTemperatureK = 5778.0,
            InnerStabilityM = 0.5 * Units.AuMeters,
            OuterStabilityM = 5.0 * Units.AuMeters,
        };
        host.CalculateZones();

        AssertTrue(host.HasValidZone(), "fixture host should expose a valid stable zone");
        AssertEqual("S-type", host.GetTypeString(), "host type string should match the stored type");
        AssertTrue(host.IsDistanceStable(Units.AuMeters), "1 AU should lie inside the fixture stable zone");
        AssertTrue(host.IsDistanceHabitable(Units.AuMeters), "1 AU should lie inside the solar-luminosity habitable zone");
        AssertTrue(host.IsBeyondFrostLine(3.0 * Units.AuMeters), "3 AU should lie beyond the nominal frost line");

        Dictionary data = host.ToDictionary();
        OrbitHost rebuilt = OrbitHost.FromDictionary(data);
        AssertVariantDeepEqual(data, rebuilt.ToDictionary(), "orbit host should round-trip semantically unchanged");
    }

    /// <summary>
    /// Verifies solar-system spec presets and dictionary round-tripping.
    /// </summary>
    private static void TestSolarSystemSpecPresetsAndRoundTrip()
    {
        SolarSystemSpec spec = SolarSystemSpec.AlphaCentauriLike(135_791);
        spec.NameHint = "Alpha";
        spec.GeneratePopulation = true;
        spec.IncludeAsteroidBelts = false;
        spec.SetOverride("viewer.force_binary", true);

        AssertEqual(3, spec.StarCountMin, "Alpha-Centauri preset should request three stars");
        AssertEqual(3, spec.StarCountMax, "Alpha-Centauri preset should cap at three stars");
        AssertTrue(spec.HasOverride("viewer.force_binary"), "set overrides should be addressable");

        Dictionary data = spec.ToDictionary();
        SolarSystemSpec rebuilt = SolarSystemSpec.FromDictionary(data);
        AssertVariantDeepEqual(data, rebuilt.ToDictionary(), "solar-system spec should round-trip semantically unchanged");

        SolarSystemSpec sunLike = SolarSystemSpec.SunLike(246_802);
        AssertEqual(1, sunLike.StarCountMin, "Sun-like preset should request one star");
        AssertEqual(1, sunLike.SpectralClassHints.Count, "Sun-like preset should seed one spectral hint");
    }

    /// <summary>
    /// Verifies hierarchy-node traversal and dictionary round-tripping.
    /// </summary>
    private static void TestHierarchyNodeRoundTripAndTreeQueries()
    {
        HierarchyNode left = HierarchyNode.CreateStar("node_a", "star_a");
        HierarchyNode right = HierarchyNode.CreateStar("node_b", "star_b");
        HierarchyNode root = HierarchyNode.CreateBarycenter("root", left, right, Units.AuMeters * 10.0, 0.1);

        AssertTrue(root.IsBarycenter(), "root should be a barycenter");
        AssertEqual(2, root.GetStarCount(), "barycenter subtree should contain both stars");
        AssertEqual(2, root.GetDepth(), "binary hierarchy depth should be two");
        AssertEqual(2, root.GetAllStarIds().Count, "tree should expose both star ids");
        AssertNotNull(root.FindNode("node_b"), "tree queries should find nested nodes");

        Dictionary data = root.ToDictionary();
        HierarchyNode? rebuilt = HierarchyNode.FromDictionary(data);
        AssertNotNull(rebuilt, "hierarchy node should rebuild from dictionary payload");
        AssertVariantDeepEqual(data, rebuilt!.ToDictionary(), "hierarchy node should round-trip semantically unchanged");
    }

    /// <summary>
    /// Verifies system-hierarchy traversal and dictionary round-tripping.
    /// </summary>
    private static void TestSystemHierarchyRoundTripAndNodeFilters()
    {
        HierarchyNode root = HierarchyNode.CreateBarycenter(
            "root",
            HierarchyNode.CreateStar("node_a", "star_a"),
            HierarchyNode.CreateStar("node_b", "star_b"),
            Units.AuMeters * 20.0);
        SystemHierarchy hierarchy = new(root);

        AssertTrue(hierarchy.IsValid(), "hierarchy with a root should be valid");
        AssertEqual(2, hierarchy.GetStarCount(), "hierarchy should count both stars");
        AssertEqual(3, hierarchy.GetAllNodes().Count, "hierarchy should flatten root plus children");
        AssertEqual(1, hierarchy.GetAllBarycenters().Count, "hierarchy should identify the barycenter");
        AssertEqual(2, hierarchy.GetAllStarNodes().Count, "hierarchy should identify the leaf stars");

        Dictionary data = hierarchy.ToDictionary();
        SystemHierarchy rebuilt = SystemHierarchy.FromDictionary(data);
        AssertVariantDeepEqual(data, rebuilt.ToDictionary(), "system hierarchy should round-trip semantically unchanged");
    }

    /// <summary>
    /// Verifies solar-system indexes update as bodies are added.
    /// </summary>
    private static void TestSolarSystemAddBodyUpdatesIndexes()
    {
        SolarSystem system = new("sys_1", "Fixture");
        CelestialBody star = StarGenerator.Generate(StarSpec.SunLike(111_222), new SeededRng(222_333));
        CelestialBody planet = PlanetGenerator.Generate(
            PlanetSpec.EarthLike(333_444),
            CreateFixturePlanetContext(),
            new SeededRng(444_555),
            enablePopulation: false);

        system.AddBody(star);
        system.AddBody(planet);

        AssertEqual(2, system.GetBodyCount(), "solar system should store both added bodies");
        AssertEqual(1, system.GetStarCount(), "solar system should index star bodies");
        AssertEqual(1, system.GetPlanetCount(), "solar system should index planet bodies");
        AssertEqual(1, system.GetStars().Count, "star lookup should return the added star");
        AssertEqual(1, system.GetPlanets().Count, "planet lookup should return the added planet");
        AssertNotNull(system.GetBody(star.Id), "body lookup should find the stored star");
    }

    /// <summary>
    /// Verifies sector generation remains stable across repeated access.
    /// </summary>
    private static void TestSectorGenerationIsStableForRepeatedAccess()
    {
        Galaxy galaxy = Galaxy.CreateDefault(555_666);
        HierarchyCoords home = HomePosition.GetHomeHierarchy();
        Sector sector = galaxy.GetSector(home.QuadrantCoords, home.SectorLocalCoords);

        int firstCount = sector.GetStarCount();
        int secondCount = sector.GetStarCount();
        AssertEqual(firstCount, secondCount, "repeated sector access should keep a stable star count");

        Godot.Collections.Array<GalaxyStar> starsA = sector.GetStars();
        Godot.Collections.Array<GalaxyStar> starsB = sector.GetStars();
        AssertEqual(starsA.Count, starsB.Count, "repeated star enumeration should keep a stable count");
        AssertTrue(sector.IsGenerated(), "sector should report generated after access");
    }

    /// <summary>
    /// Verifies top-level galaxy caches sectors and systems consistently.
    /// </summary>
    private static void TestGalaxyCachesSectorsAndSystems()
    {
        Galaxy galaxy = Galaxy.CreateDefault(777_888);
        Vector3 homePosition = HomePosition.GetDefaultPosition();
        Sector byPosition = galaxy.GetSectorAtPosition(homePosition);
        HierarchyCoords home = HomePosition.GetHomeHierarchy();
        Sector byCoords = galaxy.GetSector(home.QuadrantCoords, home.SectorLocalCoords);

        AssertTrue(ReferenceEquals(byPosition, byCoords), "sector lookups by position and by coordinates should reuse the same cache entry");
        AssertEqual(1, galaxy.GetCachedSectorCount(), "reused sector lookups should only cache one sector");

        SolarSystem? system = GalaxySystemGenerator.GenerateSystem(CreateFixtureGalaxyStar(), includeAsteroids: false, enablePopulation: false);
        AssertNotNull(system, "fixture galaxy star should generate a system for cache testing");
        galaxy.CacheSystem(123456, system!);
        AssertTrue(galaxy.HasCachedSystem(123456), "cached systems should be discoverable by seed");
        AssertEqual(1, galaxy.GetCachedSystemCount(), "system cache count should track cached entries");
        AssertTrue(ReferenceEquals(system, galaxy.GetCachedSystem(123456)), "cached system lookup should return the stored instance");
    }

    /// <summary>
    /// Verifies the default home position remains inside the nominal Milky Way bounds.
    /// </summary>
    private static void TestHomePositionStaysWithinDefaultGalaxyBounds()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(919_191);
        Vector3 home = HomePosition.GetDefaultPosition();
        HierarchyCoords hierarchy = HomePosition.GetHomeHierarchy();

        AssertTrue(HomePosition.IsWithinGalaxy(home, spec), "default home position should lie inside the galaxy bounds");
        AssertEqual(
            GalaxyCoordinates.ParsecToQuadrant(home),
            HomePosition.GetHomeQuadrant(),
            "home quadrant helper should match direct hierarchy conversion");
        AssertEqual(
            hierarchy.QuadrantCoords,
            HomePosition.GetHomeQuadrant(),
            "home hierarchy should agree with the home quadrant helper");
    }

    /// <summary>
    /// Verifies galaxy save data round-trips without semantic drift.
    /// </summary>
    private static void TestGalaxySaveDataRoundTripPreservesPayload()
    {
        GalaxySaveData original = GalaxySaveData.Create(1_777_777_777);
        original.GalaxySeed = 246810;
        original.ZoomLevel = (int)GalaxyCoordinates.ZoomLevel.Sector;
        original.SelectedQuadrant = new Vector3I(2, 3, 4);
        original.SelectedSector = new Vector3I(5, 6, 7);
        original.CameraPosition = new Vector3(10.0f, 20.0f, 30.0f);
        original.CameraRotation = new Vector3(0.1f, 0.2f, 0.3f);
        original.HasStarSelection = true;
        original.SelectedStarSeed = 13579;
        original.SelectedStarPosition = new Vector3(-1.0f, 2.5f, -3.5f);
        original.CachedSystemCount = 12;
        original.SetConfig(GalaxyConfig.CreateMilkyWay());
        original.SetBodyOverrides(CreateFixtureOverrides());

        Dictionary originalData = original.ToDictionary();
        GalaxySaveData? rebuilt = GalaxySaveData.FromDictionary(originalData);
        AssertNotNull(rebuilt, "galaxy save data should rebuild from its serialized dictionary");

        Dictionary rebuiltData = rebuilt!.ToDictionary();
        AssertVariantDeepEqual(
            originalData,
            rebuiltData,
            "galaxy save data should round-trip semantically unchanged");
        AssertTrue(rebuilt.HasConfig(), "rebuilt save data should preserve galaxy config");
        AssertTrue(rebuilt.HasBodyOverrides(), "rebuilt save data should preserve body overrides");
    }

    /// <summary>
    /// Verifies body-override serialization and in-place application.
    /// </summary>
    private static void TestGalaxyBodyOverridesRoundTripAndApply()
    {
        GalaxyBodyOverrides original = CreateFixtureOverrides();
        Dictionary serialized = original.ToDictionary();
        GalaxyBodyOverrides rebuilt = GalaxyBodyOverrides.FromDictionary(serialized);

        AssertEqual(original.TotalCount(), rebuilt.TotalCount(), "override count should survive round-trip serialization");
        AssertTrue(rebuilt.HasAnyFor(CreateFixtureGalaxyStar().StarSeed), "rebuilt overrides should preserve the star bucket");

        int starSeed = CreateFixtureGalaxyStar().StarSeed;
        CelestialBody baseline = StarGenerator.Generate(StarSpec.RedDwarf(777_777), new SeededRng(888_888));
        baseline.Name = "Original";
        Array<CelestialBody> bodies = [baseline];

        int replaced = rebuilt.ApplyToBodies(starSeed, bodies);
        AssertEqual(1, replaced, "override application should patch the matching body");
        AssertEqual("Edited Star", bodies[0].Name, "override application should replace the body payload");
    }

    /// <summary>
    /// Verifies basic cache put/get/evict semantics.
    /// </summary>
    private static void TestSystemCachePutGetAndEvict()
    {
        SystemCache cache = new();
        int seed = 112233;
        SolarSystem? system = GalaxySystemGenerator.GenerateSystem(CreateFixtureGalaxyStar(), includeAsteroids: false, enablePopulation: false);
        AssertNotNull(system, "fixture galaxy star should generate a system for cache testing");

        AssertTrue(!cache.HasSystem(seed), "fresh cache should start empty");
        cache.PutSystem(seed, system!);
        AssertTrue(cache.HasSystem(seed), "stored systems should become addressable by seed");
        AssertEqual(1, cache.GetCacheSize(), "cache size should reflect inserted systems");
        AssertTrue(ReferenceEquals(system, cache.GetSystem(seed)), "cache should return the same stored instance");

        cache.Evict(seed);
        AssertTrue(!cache.HasSystem(seed), "evict should remove the cached system");
        AssertEqual(0, cache.GetCacheSize(), "cache size should shrink after eviction");
    }

    /// <summary>
    /// Verifies save-state clearing for the migrated galaxy-viewer save/load helper.
    /// </summary>
    private static void TestGalaxyViewerSaveLoadClearsSavedState()
    {
        GalaxyViewerSaveLoad saveLoad = new();
        MockGalaxyViewerStateNode viewer = new();

        AssertTrue(saveLoad.HasSavedState(viewer), "fixture viewer should start with saved state");
        saveLoad.ClearSavedState(viewer);

        AssertTrue(!saveLoad.HasSavedState(viewer), "clearing saved state should reset the saved zoom level");
        AssertEqual(Variant.Type.Nil, viewer.get_saved_quadrant().VariantType, "saved quadrant should be cleared");
        AssertEqual(Variant.Type.Nil, viewer.get_saved_sector().VariantType, "saved sector should be cleared");
        AssertEqual(0, viewer.get_saved_star_seed(), "saved star seed should be cleared");
        AssertFloatNear(0.0, viewer.get_saved_star_position().Length(), 1.0e-6, "saved star position should be reset");
    }

    /// <summary>
    /// Verifies JSON save/load round-trip behavior for the migrated system-viewer helper.
    /// </summary>
    private static void TestSystemViewerSaveLoadRoundTripsJsonPath()
    {
        SystemViewerSaveLoad saveLoad = new();
        SolarSystem? original = GalaxySystemGenerator.GenerateSystem(CreateFixtureGalaxyStar(), includeAsteroids: false, enablePopulation: false);
        AssertNotNull(original, "fixture galaxy star should generate a system for save/load testing");

        string path = Path.Combine(
            ProjectSettings.GlobalizePath("user://"),
            $"dotnet_native_system_{Guid.NewGuid():N}.json");
        MockSystemViewerNode viewer = new(original!);

        try
        {
            Error saveError = saveLoad.SaveToPath(viewer, path, compress: false);
            AssertEqual(Error.Ok, saveError, "system-viewer save helper should save JSON files successfully");

            SystemPersistenceLoadResult result = saveLoad.LoadFromPath(path);
            AssertTrue(result.Success, "system-viewer load helper should load the saved file");
            AssertNotNull(result.System, "system-viewer load helper should rebuild a system");

            Dictionary originalData = SystemSerializer.ToDictionary(original!);
            Dictionary rebuiltData = SystemSerializer.ToDictionary(result.System!);
            NormalizeTransientFields(originalData);
            NormalizeTransientFields(rebuiltData);

            AssertVariantDeepEqual(
                originalData,
                rebuiltData,
                "system-viewer save/load helper should preserve system payloads");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// Verifies that star generation is deterministic for the same spec and RNG seed.
    /// </summary>
    private static void TestStarGeneratorDeterministicSameSeed()
    {
        StarSpec spec = StarSpec.SunLike(101_001);
        CelestialBody first = StarGenerator.Generate(spec, new SeededRng(202_002));
        CelestialBody second = StarGenerator.Generate(spec, new SeededRng(202_002));

        string firstJson = CelestialSerializer.ToJson(first, pretty: false);
        string secondJson = CelestialSerializer.ToJson(second, pretty: false);

        AssertEqual(firstJson, secondJson, "same star spec and seed should serialize identically");
    }

    /// <summary>
    /// Verifies that planet generation is deterministic for the same spec and RNG seed.
    /// </summary>
    private static void TestPlanetGeneratorDeterministicSameSeed()
    {
        PlanetSpec spec = PlanetSpec.EarthLike(212_121);
        ParentContext context = CreateFixturePlanetContext();

        CelestialBody first = PlanetGenerator.Generate(spec, context, new SeededRng(313_131), enablePopulation: false);
        CelestialBody second = PlanetGenerator.Generate(spec, context, new SeededRng(313_131), enablePopulation: false);

        Dictionary firstData = CelestialSerializer.ToDictionary(first);
        Dictionary secondData = CelestialSerializer.ToDictionary(second);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same planet spec and seed should remain semantically identical");
    }

    /// <summary>
    /// Verifies that moon generation is deterministic for the same spec and RNG seed.
    /// </summary>
    private static void TestMoonGeneratorDeterministicSameSeed()
    {
        MoonSpec spec = MoonSpec.TitanLike(414_141);
        ParentContext context = CreateFixtureMoonContext();

        CelestialBody? first = MoonGenerator.Generate(spec, context, new SeededRng(515_151), enablePopulation: false);
        CelestialBody? second = MoonGenerator.Generate(spec, context, new SeededRng(515_151), enablePopulation: false);

        AssertNotNull(first, "fixture moon spec should generate a moon");
        AssertNotNull(second, "fixture moon spec should generate a moon");

        Dictionary firstData = CelestialSerializer.ToDictionary(first!);
        Dictionary secondData = CelestialSerializer.ToDictionary(second!);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same moon spec and seed should remain semantically identical");
    }

    /// <summary>
    /// Verifies that C# celestial serialization round-trips a generated star without drift.
    /// </summary>
    private static void TestCelestialSerializerRoundTripPreservesStarPayload()
    {
        StarSpec spec = StarSpec.RedDwarf(303_003);
        CelestialBody original = StarGenerator.Generate(spec, new SeededRng(404_004));
        Dictionary originalData = CelestialSerializer.ToDictionary(original);
        string originalJson = Json.Stringify(originalData);

        CelestialBody? rebuilt = CelestialSerializer.FromJson(originalJson);
        AssertNotNull(rebuilt, "round-trip should rebuild a celestial body");

        Dictionary rebuiltData = CelestialSerializer.ToDictionary(rebuilt!);
        AssertVariantDeepEqual(
            originalData,
            rebuiltData,
            "round-tripped star payload should remain semantically unchanged");
    }

    /// <summary>
    /// Verifies that ring generation is deterministic for the same inputs and RNG seed.
    /// </summary>
    private static void TestRingSystemGeneratorDeterministicSameSeed()
    {
        RingSystemSpec spec = RingSystemSpec.Complex(505_005);
        PhysicalProps planetPhysical = new(
            Units.EarthMassKg * 120.0,
            Units.JupiterRadiusMeters,
            10.0 * 3600.0,
            3.0,
            0.05,
            1.0e25,
            0.0);
        ParentContext context = ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters * 5.2);

        RingSystemProps? first = RingSystemGenerator.Generate(spec, planetPhysical, context, new SeededRng(606_006));
        RingSystemProps? second = RingSystemGenerator.Generate(spec, planetPhysical, context, new SeededRng(606_006));

        AssertNotNull(first, "ring generation should produce a system for the fixture inputs");
        AssertNotNull(second, "ring generation should produce a system for the fixture inputs");

        string firstJson = Json.Stringify(first!.ToDictionary());
        string secondJson = Json.Stringify(second!.ToDictionary());
        AssertEqual(firstJson, secondJson, "same ring inputs and seed should serialize identically");
    }

    /// <summary>
    /// Verifies that asteroid generation is deterministic for the same inputs and RNG seed.
    /// </summary>
    private static void TestAsteroidGeneratorDeterministicSameSeed()
    {
        AsteroidSpec spec = AsteroidSpec.CeresLike(707_007);
        ParentContext context = ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters * 2.7);

        CelestialBody first = AsteroidGenerator.Generate(spec, context, new SeededRng(808_008));
        CelestialBody second = AsteroidGenerator.Generate(spec, context, new SeededRng(808_008));

        Dictionary firstData = CelestialSerializer.ToDictionary(first);
        Dictionary secondData = CelestialSerializer.ToDictionary(second);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same asteroid inputs and seed should remain semantically identical");
    }

    /// <summary>
    /// Verifies that a generated system can round-trip through the C# system serializer.
    /// </summary>
    private static void TestSystemSerializerRoundTripPreservesSystemPayload()
    {
        GalaxyStar star = CreateFixtureGalaxyStar();
        SolarSystem? original = GalaxySystemGenerator.GenerateSystem(star, includeAsteroids: true, enablePopulation: false);
        AssertNotNull(original, "fixture galaxy star should generate a system");

        Dictionary originalData = SystemSerializer.ToDictionary(original!);
        NormalizeTransientFields(originalData);
        string originalJson = Json.Stringify(originalData);

        SolarSystem? rebuilt = SystemSerializer.FromJson(originalJson);
        AssertNotNull(rebuilt, "system round-trip should rebuild a solar system");

        Dictionary rebuiltData = SystemSerializer.ToDictionary(rebuilt!);
        NormalizeTransientFields(rebuiltData);

        AssertVariantDeepEqual(
            originalData,
            rebuiltData,
            "round-tripped system payload should remain semantically unchanged");
    }

    /// <summary>
    /// Verifies that galaxy-to-system generation remains deterministic for a fixed galaxy star.
    /// </summary>
    private static void TestGalaxySystemGeneratorDeterministicSameStarSeed()
    {
        GalaxyStar firstStar = CreateFixtureGalaxyStar();
        GalaxyStar secondStar = CreateFixtureGalaxyStar();

        SolarSystem? first = GalaxySystemGenerator.GenerateSystem(firstStar, includeAsteroids: true, enablePopulation: false);
        SolarSystem? second = GalaxySystemGenerator.GenerateSystem(secondStar, includeAsteroids: true, enablePopulation: false);

        AssertNotNull(first, "fixture galaxy star should generate a system");
        AssertNotNull(second, "fixture galaxy star should generate a system");

        Dictionary firstData = SystemSerializer.ToDictionary(first!);
        Dictionary secondData = SystemSerializer.ToDictionary(second!);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same galaxy star seed should generate the same system payload");
    }

    /// <summary>
    /// Throws when two values differ.
    /// </summary>
    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message}. Expected '{expected}', got '{actual}'.");
        }
    }

    /// <summary>
    /// Throws when two values are equal.
    /// </summary>
    private static void AssertNotEqual<T>(T left, T right, string message)
    {
        if (Equals(left, right))
        {
            throw new InvalidOperationException($"{message}. Both values were '{left}'.");
        }
    }

    /// <summary>
    /// Throws when a reference value is null.
    /// </summary>
    private static void AssertNotNull(object? value, string message)
    {
        if (value == null)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Throws when a condition is false.
    /// </summary>
    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Throws when floating-point values differ beyond a supplied tolerance.
    /// </summary>
    private static void AssertFloatNear(double expected, double actual, double tolerance, string message)
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException($"{message}. Expected '{expected}', got '{actual}'.");
        }
    }

    /// <summary>
    /// Throws when two Godot variants differ after tolerant deep comparison.
    /// </summary>
    private static void AssertVariantDeepEqual(Dictionary expected, Dictionary actual, string message)
    {
        if (TryFindDifference(expected, actual, "$", out string difference))
        {
            throw new InvalidOperationException($"{message}. {difference}");
        }
    }

    /// <summary>
    /// Returns the first semantic difference between two variants, if any.
    /// </summary>
    private static bool TryFindDifference(Variant expected, Variant actual, string path, out string difference)
    {
        if (IsNumericVariant(expected) && IsNumericVariant(actual))
        {
            if (NumbersEqual(ToDouble(expected), ToDouble(actual)))
            {
                difference = string.Empty;
                return false;
            }

            difference = $"{path}: numeric mismatch ({ToDouble(expected)} != {ToDouble(actual)})";
            return true;
        }

        if (IsStringLikeVariant(expected) && IsStringLikeVariant(actual))
        {
            string expectedText = expected.ToString();
            string actualText = actual.ToString();
            if (expectedText == actualText)
            {
                difference = string.Empty;
                return false;
            }

            difference = $"{path}: string mismatch ('{expectedText}' != '{actualText}')";
            return true;
        }

        if (expected.VariantType != actual.VariantType)
        {
            difference = $"{path}: type mismatch ({expected.VariantType} != {actual.VariantType})";
            return true;
        }

        if (expected.VariantType == Variant.Type.Dictionary)
        {
            Dictionary expectedDictionary = expected.AsGodotDictionary();
            Dictionary actualDictionary = actual.AsGodotDictionary();
            if (expectedDictionary.Count != actualDictionary.Count)
            {
                difference = $"{path}: dictionary size mismatch ({expectedDictionary.Count} != {actualDictionary.Count})";
                return true;
            }

            foreach (Variant key in expectedDictionary.Keys)
            {
                if (!TryGetDictionaryValue(actualDictionary, key, out Variant actualKey, out Variant actualValue))
                {
                    difference = $"{path}: missing key '{key}'";
                    return true;
                }

                string childPath = $"{path}.{actualKey}";
                if (TryFindDifference(expectedDictionary[key], actualValue, childPath, out difference))
                {
                    return true;
                }
            }

            difference = string.Empty;
            return false;
        }

        if (expected.VariantType == Variant.Type.Array)
        {
            Godot.Collections.Array expectedArray = expected.AsGodotArray();
            Godot.Collections.Array actualArray = actual.AsGodotArray();
            if (expectedArray.Count != actualArray.Count)
            {
                difference = $"{path}: array size mismatch ({expectedArray.Count} != {actualArray.Count})";
                return true;
            }

            for (int index = 0; index < expectedArray.Count; index += 1)
            {
                if (TryFindDifference(expectedArray[index], actualArray[index], $"{path}[{index}]", out difference))
                {
                    return true;
                }
            }

            difference = string.Empty;
            return false;
        }

        if (expected.Equals(actual))
        {
            difference = string.Empty;
            return false;
        }

        difference = $"{path}: value mismatch ('{expected}' != '{actual}')";
        return true;
    }

    /// <summary>
    /// Returns whether a variant is any numeric type that may be normalized through JSON.
    /// </summary>
    private static bool IsNumericVariant(Variant value)
    {
        return value.VariantType == Variant.Type.Int
            || value.VariantType == Variant.Type.Float;
    }

    /// <summary>
    /// Returns whether a variant is a string-like key/value that can normalize through JSON.
    /// </summary>
    private static bool IsStringLikeVariant(Variant value)
    {
        return value.VariantType == Variant.Type.String
            || value.VariantType == Variant.Type.StringName
            || value.VariantType == Variant.Type.NodePath;
    }

    /// <summary>
    /// Converts a numeric variant to double.
    /// </summary>
    private static double ToDouble(Variant value)
    {
        return value.VariantType == Variant.Type.Int
            ? value.AsInt64()
            : value.AsDouble();
    }

    /// <summary>
    /// Compares two floating-point values with relative tolerance.
    /// </summary>
    private static bool NumbersEqual(double expected, double actual)
    {
        double difference = Math.Abs(expected - actual);
        if (difference <= 1.0e-6)
        {
            return true;
        }

        double scale = Math.Max(Math.Abs(expected), Math.Abs(actual));
        if (scale <= 1.0)
        {
            return difference <= 1.0e-6;
        }

        return difference / scale <= 1.0e-6;
    }

    /// <summary>
    /// Removes known transient fields from a payload tree before semantic comparison.
    /// </summary>
    private static void NormalizeTransientFields(Dictionary data)
    {
        RemoveTransientFields(data);
    }

    /// <summary>
    /// Walks a payload recursively and removes transient keys like timestamps.
    /// </summary>
    private static void RemoveTransientFields(Variant value)
    {
        if (value.VariantType == Variant.Type.Dictionary)
        {
            Dictionary dictionary = value.AsGodotDictionary();
            dictionary.Remove("created_timestamp");
            foreach (Variant key in dictionary.Keys)
            {
                RemoveTransientFields(dictionary[key]);
            }

            return;
        }

        if (value.VariantType == Variant.Type.Array)
        {
            Godot.Collections.Array array = value.AsGodotArray();
            foreach (Variant item in array)
            {
                RemoveTransientFields(item);
            }
        }
    }

    /// <summary>
    /// Finds a matching dictionary value while allowing string-key normalization.
    /// </summary>
    private static bool TryGetDictionaryValue(
        Dictionary dictionary,
        Variant expectedKey,
        out Variant actualKey,
        out Variant actualValue)
    {
        if (dictionary.ContainsKey(expectedKey))
        {
            actualKey = expectedKey;
            actualValue = dictionary[expectedKey];
            return true;
        }

        if (IsStringLikeVariant(expectedKey))
        {
            string expectedText = expectedKey.ToString();
            foreach (Variant candidateKey in dictionary.Keys)
            {
                if (candidateKey.ToString() == expectedText)
                {
                    actualKey = candidateKey;
                    actualValue = dictionary[candidateKey];
                    return true;
                }
            }
        }

        actualKey = default;
        actualValue = default;
        return false;
    }

    /// <summary>
    /// Creates a deterministic galaxy-star fixture for system-generation tests.
    /// </summary>
    private static GalaxyStar CreateFixtureGalaxyStar()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(909_009);
        return GalaxyStar.CreateWithDerivedProperties(new Vector3(1200.0f, 15.0f, -800.0f), 111_111, spec);
    }

    /// <summary>
    /// Creates a deterministic planet-generation context.
    /// </summary>
    private static ParentContext CreateFixturePlanetContext()
    {
        return ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters);
    }

    /// <summary>
    /// Creates a deterministic moon-generation context for a gas-giant host.
    /// </summary>
    private static ParentContext CreateFixtureMoonContext()
    {
        return ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters * 9.5,
            Units.JupiterMassKg,
            Units.JupiterRadiusMeters,
            Units.JupiterRadiusMeters * 20.0);
    }

    /// <summary>
    /// Creates a deterministic edited-body override payload for a fixture star.
    /// </summary>
    private static GalaxyBodyOverrides CreateFixtureOverrides()
    {
        GalaxyStar star = CreateFixtureGalaxyStar();
        CelestialBody body = StarGenerator.Generate(StarSpec.RedDwarf(star.StarSeed), new SeededRng(888_888));
        body.Name = "Edited Star";

        GalaxyBodyOverrides overrides = new();
        overrides.SetOverride(star.StarSeed, body);
        return overrides;
    }

    /// <summary>
    /// Creates a simple rocky planet used by the material-factory tests.
    /// </summary>
    private static CelestialBody CreateFixtureMaterialFactoryRockyBody()
    {
        CelestialBody body = new(
            "test_rocky",
            "Test Rocky",
            CelestialType.Type.Planet,
            new PhysicalProps(
                Units.EarthMassKg,
                Units.EarthRadiusMeters,
                86_400.0,
                23.5,
                0.0033,
                7.8e22,
                4.4e13),
            new Provenance(24_680, "1.0.0", 0, 0, new Dictionary()));
        body.Surface = new SurfaceProps(
            288.0,
            0.3,
            "continental",
            0.1,
            new Dictionary
            {
                ["iron"] = 0.2,
            });
        return body;
    }

    /// <summary>
    /// Creates a simple solar-type star used by the material-factory tests.
    /// </summary>
    private static CelestialBody CreateFixtureMaterialFactoryStarBody()
    {
        CelestialBody body = new(
            "test_star",
            "Test Star",
            CelestialType.Type.Star,
            new PhysicalProps(
                1.989e30,
                6.9634e8,
                2.16e6,
                7.25,
                0.0,
                0.0,
                0.0),
            new Provenance(13_579, "1.0.0", 0, 0, new Dictionary()));
        body.Stellar = new StellarProps(
            StellarProps.SolarLuminosityWatts,
            5778.0,
            "G2V",
            "main_sequence",
            1.0,
            4.6e9);
        return body;
    }

    /// <summary>
    /// Creates a deterministic single-star fixture with one planet and one belt for layout tests.
    /// </summary>
    private static SolarSystem CreateFixtureDisplayLayoutSystemWithBelt()
    {
        SolarSystem system = new("test_layout", "Layout System");

        CelestialBody star = new(
            "star_0",
            "Test Star",
            CelestialType.Type.Star,
            new PhysicalProps(Units.SolarMassKg, Units.SolarRadiusMeters),
            new Provenance(11_111, "1.0.0", 0, 0, new Dictionary()));
        CelestialBody planet = CreateFixtureMaterialFactoryRockyBody();
        planet.Id = "planet_0";
        planet.Name = "Test Planet";
        planet.Orbital = new OrbitalProps
        {
            ParentId = "node_star_0",
            SemiMajorAxisM = Units.AuMeters,
            MeanAnomalyDeg = 0.0,
        };

        AsteroidBelt belt = new("belt_0", "Test Belt")
        {
            OrbitHostId = "node_star_0",
            InnerRadiusM = 2.0 * Units.AuMeters,
            OuterRadiusM = 3.0 * Units.AuMeters,
            TotalMassKg = 1.0e21,
        };

        system.AddBody(star);
        system.AddBody(planet);
        system.AddAsteroidBelt(belt);
        system.Hierarchy = new SystemHierarchy(HierarchyNode.CreateStar("node_star_0", "star_0"));
        return system;
    }

    /// <summary>
    /// Minimal mixed-runtime save-state surface for the galaxy-viewer save/load helper.
    /// </summary>
    private sealed partial class MockGalaxyViewerStateNode : Node
    {
        private int _savedZoomLevel = (int)GalaxyCoordinates.ZoomLevel.Subsector;
        private Variant _savedQuadrant = Variant.CreateFrom(new Vector3I(1, 2, 3));
        private Variant _savedSector = Variant.CreateFrom(new Vector3I(4, 5, 6));
        private Vector3 _savedStarCameraPosition = new(7.0f, 8.0f, 9.0f);
        private Vector3 _savedStarCameraRotation = new(0.1f, 0.2f, 0.3f);
        private int _savedStarSeed = 123456;
        private Vector3 _savedStarPosition = new(10.0f, 11.0f, 12.0f);

        public void set_saved_zoom_level(int value) => _savedZoomLevel = value;

        public int get_saved_zoom_level() => _savedZoomLevel;

        public void set_saved_quadrant(Variant value) => _savedQuadrant = value;

        public Variant get_saved_quadrant() => _savedQuadrant;

        public void set_saved_sector(Variant value) => _savedSector = value;

        public Variant get_saved_sector() => _savedSector;

        public void set_saved_star_camera_position(Vector3 value) => _savedStarCameraPosition = value;

        public Vector3 get_saved_star_camera_position() => _savedStarCameraPosition;

        public void set_saved_star_camera_rotation(Vector3 value) => _savedStarCameraRotation = value;

        public Vector3 get_saved_star_camera_rotation() => _savedStarCameraRotation;

        public void set_saved_star_seed(int value) => _savedStarSeed = value;

        public int get_saved_star_seed() => _savedStarSeed;

        public void set_saved_star_position(Vector3 value) => _savedStarPosition = value;

        public Vector3 get_saved_star_position() => _savedStarPosition;
    }

    /// <summary>
    /// Minimal mixed-runtime viewer surface for the system-viewer save/load helper.
    /// </summary>
    private sealed partial class MockSystemViewerNode : Node
    {
        private readonly SolarSystem _currentSystem;

        public MockSystemViewerNode(SolarSystem currentSystem)
        {
            _currentSystem = currentSystem;
        }

        public SolarSystem get_current_system() => _currentSystem;
    }
}
