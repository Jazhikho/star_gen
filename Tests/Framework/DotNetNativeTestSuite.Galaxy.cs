#nullable enable annotations
#nullable disable warnings
using Godot;
using Godot.Collections;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
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
    /// Verifies galaxy-configuration validation, display naming, and round-trip behavior.
    /// </summary>
    private static void TestGalaxyConfigRoundTripAndValidation()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        AssertTrue(config.IsValid(), "Milky-Way config should be valid");
        AssertEqual("Spiral", config.GetTypeName(), "Milky-Way config should report the spiral type name");

        Godot.Collections.Dictionary data = config.ToDictionary();
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

        Godot.Collections.Dictionary data = star.ToDictionary();
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
        original.ZoomLevel = GalaxyCoordinates.ZoomLevel.Sector;
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

        Godot.Collections.Dictionary originalData = original.ToDictionary();
        GalaxySaveData? rebuilt = GalaxySaveData.FromDictionary(originalData);
        AssertNotNull(rebuilt, "galaxy save data should rebuild from its serialized dictionary");

        Godot.Collections.Dictionary rebuiltData = rebuilt!.ToDictionary();
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
        Godot.Collections.Dictionary serialized = original.ToDictionary();
        GalaxyBodyOverrides rebuilt = GalaxyBodyOverrides.FromDictionary(serialized);

        AssertEqual(original.TotalCount(), rebuilt.TotalCount(), "override count should survive round-trip serialization");
        AssertTrue(rebuilt.HasAnyFor(CreateFixtureGalaxyStar().StarSeed), "rebuilt overrides should preserve the star bucket");

        int starSeed = CreateFixtureGalaxyStar().StarSeed;
        CelestialBody baseline = StarGenerator.Generate(StarSpec.RedDwarf(777_777), new SeededRng(888_888));
        baseline.Name = "Original";
        Godot.Collections.Array<CelestialBody> bodies = [baseline];

        int replaced = rebuilt.ApplyToBodies(starSeed, bodies);
        AssertEqual(1, replaced, "override application should patch the matching body");
        AssertEqual("Edited Star", bodies[0].Name, "override application should replace the body payload");
    }

    /// <summary>
    /// Verifies save-state clearing for the migrated galaxy-viewer save/load helper.
    /// </summary>
    private static void TestGalaxyViewerSaveLoadClearsSavedState()
    {
        GalaxyViewerSaveLoad saveLoad = new();
        MockGalaxyViewerStateNode viewer = new();
        try
        {
            AssertTrue(saveLoad.HasSavedState(viewer), "fixture viewer should start with saved state");
            saveLoad.ClearSavedState(viewer);

            AssertTrue(!saveLoad.HasSavedState(viewer), "clearing saved state should reset the saved zoom level");
            AssertTrue(!viewer.GetSavedQuadrant().HasValue, "saved quadrant should be cleared");
            AssertTrue(!viewer.GetSavedSector().HasValue, "saved sector should be cleared");
            AssertEqual(0, viewer.GetSavedStarSeed(), "saved star seed should be cleared");
            AssertFloatNear(0.0, viewer.GetSavedStarPosition().Length(), 1.0e-6, "saved star position should be reset");
        }
        finally
        {
            viewer.Free();
        }
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

        Godot.Collections.Dictionary firstData = SystemSerializer.ToDictionary(first!);
        Godot.Collections.Dictionary secondData = SystemSerializer.ToDictionary(second!);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same galaxy star seed should generate the same system payload");
    }
}
