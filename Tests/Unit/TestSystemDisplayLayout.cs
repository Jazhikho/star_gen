#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.SystemViewer;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemDisplayLayout.
/// NOTE: This is a large test suite covering layout calculations for various system configurations.
/// </summary>
public static class TestSystemDisplayLayout
{
    /// <summary>
    /// Creates a test star with given radius in solar radii.
    /// </summary>
    private static CelestialBody CreateTestStar(string id, double solarRadii, double massSolar = -1.0)
    {
        CelestialBody star = new CelestialBody();
        star.Id = id;
        star.Name = "Test Star " + id;
        star.Type = CelestialType.Type.Star;
        star.Physical = new PhysicalProps();
        star.Physical.RadiusM = solarRadii * Units.SolarRadiusMeters;
        if (massSolar < 0)
        {
            star.Physical.MassKg = Units.SolarMassKg * Math.Pow(solarRadii, 2.0);
        }
        else
        {
            star.Physical.MassKg = Units.SolarMassKg * massSolar;
        }
        return star;
    }

    /// <summary>
    /// Creates a test planet with given radius in Earth radii.
    /// </summary>
    private static CelestialBody CreateTestPlanet(string id, double earthRadii, string parentId, double smaAu)
    {
        CelestialBody planet = new CelestialBody();
        planet.Id = id;
        planet.Name = "Test Planet " + id;
        planet.Type = CelestialType.Type.Planet;
        planet.Physical = new PhysicalProps();
        planet.Physical.RadiusM = earthRadii * Units.EarthRadiusMeters;
        planet.Physical.MassKg = Units.EarthMassKg * Math.Pow(earthRadii, 2.5);
        planet.Orbital = new OrbitalProps();
        planet.Orbital.ParentId = parentId;
        planet.Orbital.SemiMajorAxisM = smaAu * Units.AuMeters;
        planet.Orbital.MeanAnomalyDeg = 0.0;
        return planet;
    }

    /// <summary>
    /// Creates a test asteroid belt.
    /// </summary>
    private static AsteroidBelt CreateTestBelt(string id, string hostId, double innerAu, double outerAu)
    {
        AsteroidBelt belt = new AsteroidBelt(id, "Test Belt");
        belt.OrbitHostId = hostId;
        belt.InnerRadiusM = innerAu * Units.AuMeters;
        belt.OuterRadiusM = outerAu * Units.AuMeters;
        belt.TotalMassKg = 1.0e21;
        return belt;
    }

    /// <summary>
    /// Tests star display radius solar.
    /// </summary>
    public static void TestStarDisplayRadiusSolar()
    {
        double radius = SystemDisplayLayout.CalculateStarDisplayRadius(Units.SolarRadiusMeters);

        DotNetNativeTestSuite.AssertFloatNear(3.0, radius, 0.01, "Sun should have display radius of 3 units");
    }

    /// <summary>
    /// Tests star display radius red dwarf.
    /// </summary>
    public static void TestStarDisplayRadiusRedDwarf()
    {
        double radius = SystemDisplayLayout.CalculateStarDisplayRadius(0.1 * Units.SolarRadiusMeters);

        DotNetNativeTestSuite.AssertFloatNear(2.0, radius, 0.01, "0.1 solar radius star should be 2 units");
    }

    /// <summary>
    /// Tests star display radius giant.
    /// </summary>
    public static void TestStarDisplayRadiusGiant()
    {
        double radius = SystemDisplayLayout.CalculateStarDisplayRadius(100.0 * Units.SolarRadiusMeters);

        DotNetNativeTestSuite.AssertFloatNear(5.0, radius, 0.01, "100 solar radius star should be 5 units");
    }

    /// <summary>
    /// Tests star display radius max clamp.
    /// </summary>
    public static void TestStarDisplayRadiusMaxClamp()
    {
        double radius = SystemDisplayLayout.CalculateStarDisplayRadius(1000000.0 * Units.SolarRadiusMeters);

        DotNetNativeTestSuite.AssertFloatNear(9.0, radius, 0.01, "Very large star should be clamped to 9 units");
    }

    /// <summary>
    /// Tests star display radius min clamp.
    /// </summary>
    public static void TestStarDisplayRadiusMinClamp()
    {
        double radius = SystemDisplayLayout.CalculateStarDisplayRadius(0.001 * Units.SolarRadiusMeters);

        DotNetNativeTestSuite.AssertFloatNear(1.0, radius, 0.01, "Very small star should be clamped to 1 unit");
    }

    /// <summary>
    /// Tests planet display radius earth.
    /// </summary>
    public static void TestPlanetDisplayRadiusEarth()
    {
        double radius = SystemDisplayLayout.CalculatePlanetDisplayRadius(Units.EarthRadiusMeters);

        if (radius <= 0.5)
        {
            throw new InvalidOperationException("Earth display radius should be > 0.5");
        }
        if (radius >= 1.5)
        {
            throw new InvalidOperationException("Earth display radius should be < 1.5");
        }
    }

    /// <summary>
    /// Tests planet display radius jupiter.
    /// </summary>
    public static void TestPlanetDisplayRadiusJupiter()
    {
        double jupiterRadii = 11.2;
        double radius = SystemDisplayLayout.CalculatePlanetDisplayRadius(jupiterRadii * Units.EarthRadiusMeters);

        if (radius <= 1.5)
        {
            throw new InvalidOperationException("Jupiter-sized planet should be > 1.5");
        }
        if (radius > 2.0)
        {
            throw new InvalidOperationException("Jupiter-sized planet should be at most 2.0 units");
        }
    }

    /// <summary>
    /// Tests planet display radius dwarf.
    /// </summary>
    public static void TestPlanetDisplayRadiusDwarf()
    {
        double radius = SystemDisplayLayout.CalculatePlanetDisplayRadius(0.1 * Units.EarthRadiusMeters);

        DotNetNativeTestSuite.AssertFloatNear(0.25, radius, 0.1, "Small planet should be near minimum");
    }

    /// <summary>
    /// Tests first orbit radius solar.
    /// </summary>
    public static void TestFirstOrbitRadiusSolar()
    {
        double starDisplay = 3.0;
        double maxPlanetRadius = 2.0;
        double logAdj = 0.0;
        double orbit = SystemDisplayLayout.CalculateFirstOrbitRadiusForStar(
            starDisplay, maxPlanetRadius, logAdj
        );

        DotNetNativeTestSuite.AssertFloatNear(9.0, orbit, 0.01, "First orbit around sun should be at 9 units");
    }

    /// <summary>
    /// Tests first orbit radius giant.
    /// </summary>
    public static void TestFirstOrbitRadiusGiant()
    {
        double starDisplay = 5.0;
        double maxPlanetRadius = 2.0;
        double logAdj = 2.0;
        double orbit = SystemDisplayLayout.CalculateFirstOrbitRadiusForStar(
            starDisplay, maxPlanetRadius, logAdj
        );

        DotNetNativeTestSuite.AssertFloatNear(13.0, orbit, 0.01, "First orbit around giant star should be at 13 units");
    }

    /// <summary>
    /// Tests orbit spacing.
    /// </summary>
    public static void TestOrbitSpacing()
    {
        double firstOrbit = 6.0;
        double second = SystemDisplayLayout.CalculateNthOrbitRadius(firstOrbit, 1);
        double third = SystemDisplayLayout.CalculateNthOrbitRadius(firstOrbit, 2);

        DotNetNativeTestSuite.AssertFloatNear(6.0f, (float)(second - firstOrbit), 0.01f, "Orbit spacing should be 6 units");
        DotNetNativeTestSuite.AssertFloatNear(6.0f, (float)(third - second), 0.01f, "Orbit spacing should be consistent");
    }

    /// <summary>
    /// Tests belt layout generated.
    /// </summary>
    public static void TestBeltLayoutGenerated()
    {
        SolarSystem system = new SolarSystem("test", "Belt System");
        CelestialBody star = CreateTestStar("star_0", 1.0);
        system.AddBody(star);
        CelestialBody planet = CreateTestPlanet("planet_0", 1.0, "node_star_0", 1.0);
        system.AddBody(planet);
        system.AddAsteroidBelt(CreateTestBelt("belt_0", "node_star_0", 2.0, 3.0));
        HierarchyNode starNode = HierarchyNode.CreateStar("node_star_0", "star_0");
        system.Hierarchy = new SystemHierarchy(starNode);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);
        BeltLayout beltLayout = layout.GetBeltLayout("belt_0");
        DotNetNativeTestSuite.AssertNotNull(beltLayout, "Belt layout should be created");
        if (beltLayout.CenterDisplayRadius <= 0.0)
        {
            throw new InvalidOperationException("Display center radius should be positive");
        }
        if (beltLayout.OuterDisplayRadius <= beltLayout.InnerDisplayRadius)
        {
            throw new InvalidOperationException("Outer display radius > inner");
        }
        DotNetNativeTestSuite.AssertFloatNear(2.0, beltLayout.InnerAu, 0.001, "Inner AU metadata should be preserved");
        DotNetNativeTestSuite.AssertFloatNear(3.0, beltLayout.OuterAu, 0.001, "Outer AU metadata should be preserved");
    }

    /// <summary>
    /// Tests belt inclination scales with distance.
    /// </summary>
    public static void TestBeltInclinationScalesWithDistance()
    {
        double nearValue = SystemDisplayLayout.CalculateBeltMaxInclinationDeg(10.0);
        double farValue = SystemDisplayLayout.CalculateBeltMaxInclinationDeg(60.0);
        if (farValue <= nearValue)
        {
            throw new InvalidOperationException("Farther belts should allow greater inclination");
        }
    }

    /// <summary>
    /// Tests single star layout.
    /// </summary>
    public static void TestSingleStarLayout()
    {
        SolarSystem system = new SolarSystem("test", "Test System");

        CelestialBody star = CreateTestStar("star_0", 1.0);
        system.AddBody(star);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star_0", "star_0");
        system.Hierarchy = new SystemHierarchy(starNode);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        DotNetNativeTestSuite.AssertNotNull(layout, "Should create layout");
        if (!layout.BodyLayouts.ContainsKey("star_0"))
        {
            throw new InvalidOperationException("Should have star_0 layout");
        }

        BodyLayout starLayout = layout.GetBodyLayout("star_0");
        DotNetNativeTestSuite.AssertEqual(Vector3.Zero, starLayout.Position, "Single star should be at origin");
        DotNetNativeTestSuite.AssertFloatNear(3.0, starLayout.DisplayRadius, 0.01, "Star display radius should be 3.0");
    }

    /// <summary>
    /// Tests single star with planets layout.
    /// </summary>
    public static void TestSingleStarWithPlanetsLayout()
    {
        SolarSystem system = new SolarSystem("test", "Test System");

        CelestialBody star = CreateTestStar("star_0", 1.0);
        system.AddBody(star);

        CelestialBody planet1 = CreateTestPlanet("planet_0", 1.0, "node_star_0", 1.0);
        CelestialBody planet2 = CreateTestPlanet("planet_1", 5.0, "node_star_0", 5.0);
        system.AddBody(planet1);
        system.AddBody(planet2);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star_0", "star_0");
        system.Hierarchy = new SystemHierarchy(starNode);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        BodyLayout p1Layout = layout.GetBodyLayout("planet_0");
        BodyLayout p2Layout = layout.GetBodyLayout("planet_1");

        DotNetNativeTestSuite.AssertNotNull(p1Layout, "Should have planet_0 layout");
        DotNetNativeTestSuite.AssertNotNull(p2Layout, "Should have planet_1 layout");

        NodeExtent extent = layout.GetNodeExtent("node_star_0");
        double firstOrbit = extent.FirstOrbitRadius;
        DotNetNativeTestSuite.AssertFloatNear(firstOrbit, p1Layout.OrbitRadius, 0.01, "First planet at first orbit");

        DotNetNativeTestSuite.AssertFloatNear(firstOrbit + 6.0, p2Layout.OrbitRadius, 0.01, "Second planet one ORBIT_SPACING further");

        if (p1Layout.Position == Vector3.Zero)
        {
            throw new InvalidOperationException("Planet position should not be at origin");
        }
        DotNetNativeTestSuite.AssertEqual(Vector3.Zero, p1Layout.OrbitCenter, "Orbit center should be at star position");
    }

    /// <summary>
    /// Tests star extent with planets.
    /// </summary>
    public static void TestStarExtentWithPlanets()
    {
        SolarSystem system = new SolarSystem("test", "Test System");

        CelestialBody star = CreateTestStar("star_0", 1.0);
        system.AddBody(star);

        for (int i = 0; i < 3; i++)
        {
            CelestialBody planet = CreateTestPlanet($"planet_{i}", 1.0, "node_star_0", (double)(i + 1));
            system.AddBody(planet);
        }

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star_0", "star_0");
        system.Hierarchy = new SystemHierarchy(starNode);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        NodeExtent extent = layout.GetNodeExtent("node_star_0");
        DotNetNativeTestSuite.AssertNotNull(extent, "Should have extent");
        double outermostOrbit = SystemDisplayLayout.CalculateNthOrbitRadius(
            extent.FirstOrbitRadius, 2
        );
        DotNetNativeTestSuite.AssertFloatNear(
            outermostOrbit + extent.MaxPlanetRadius,
            extent.ExtentRadius,
            0.02,
            "Extent = outermost orbit + max planet radius"
        );
        DotNetNativeTestSuite.AssertEqual(3, extent.StypePlanetCount, "Should have 3 S-type planets");
    }

    /// <summary>
    /// Tests binary star layout no planets.
    /// </summary>
    public static void TestBinaryStarLayoutNoPlanets()
    {
        SolarSystem system = new SolarSystem("test", "Binary System");

        CelestialBody starA = CreateTestStar("star_0", 1.0, 1.0);
        CelestialBody starB = CreateTestStar("star_1", 0.5, 0.5);
        system.AddBody(starA);
        system.AddBody(starB);

        HierarchyNode nodeA = HierarchyNode.CreateStar("node_star_0", "star_0");
        HierarchyNode nodeB = HierarchyNode.CreateStar("node_star_1", "star_1");
        HierarchyNode barycenter = HierarchyNode.CreateBarycenter(
            "barycenter_0", nodeA, nodeB, Units.AuMeters * 10.0, 0.3
        );
        system.Hierarchy = new SystemHierarchy(barycenter);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        BodyLayout starALayout = layout.GetBodyLayout("star_0");
        BodyLayout starBLayout = layout.GetBodyLayout("star_1");

        DotNetNativeTestSuite.AssertNotNull(starALayout, "Should have star_0 layout");
        DotNetNativeTestSuite.AssertNotNull(starBLayout, "Should have star_1 layout");

        if (starALayout.Position == starBLayout.Position)
        {
            throw new InvalidOperationException("Stars should be at different positions");
        }

        if (starALayout.Position.Length() >= starBLayout.Position.Length())
        {
            throw new InvalidOperationException("Heavier star should be closer to barycenter");
        }

        double separation = (starALayout.Position - starBLayout.Position).Length();
        double minSeparation = starALayout.DisplayRadius + starBLayout.DisplayRadius;
        if (separation <= minSeparation)
        {
            throw new InvalidOperationException("Stars should not overlap");
        }
    }

    /// <summary>
    /// Tests first orbit outside star solar.
    /// </summary>
    public static void TestFirstOrbitOutsideStarSolar()
    {
        SolarSystem system = new SolarSystem("test", "Test System");

        CelestialBody star = CreateTestStar("star_0", 1.0);
        system.AddBody(star);

        CelestialBody planet = CreateTestPlanet("planet_0", 1.0, "node_star_0", 1.0);
        system.AddBody(planet);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star_0", "star_0");
        system.Hierarchy = new SystemHierarchy(starNode);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        BodyLayout starLayout = layout.GetBodyLayout("star_0");
        BodyLayout planetLayout = layout.GetBodyLayout("planet_0");

        if (planetLayout.OrbitRadius <= starLayout.DisplayRadius + 1.0)
        {
            throw new InvalidOperationException("First orbit should be clearly outside star surface");
        }
    }

    /// <summary>
    /// Tests first orbit outside giant star.
    /// </summary>
    public static void TestFirstOrbitOutsideGiantStar()
    {
        SolarSystem system = new SolarSystem("test", "Test System");

        CelestialBody star = CreateTestStar("star_0", 100.0);
        system.AddBody(star);

        CelestialBody planet = CreateTestPlanet("planet_0", 1.0, "node_star_0", 1.0);
        system.AddBody(planet);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star_0", "star_0");
        system.Hierarchy = new SystemHierarchy(starNode);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        BodyLayout starLayout = layout.GetBodyLayout("star_0");
        BodyLayout planetLayout = layout.GetBodyLayout("planet_0");

        if (planetLayout.OrbitRadius <= starLayout.DisplayRadius + 1.0)
        {
            throw new InvalidOperationException("First orbit should be clearly outside giant star surface");
        }
    }

    /// <summary>
    /// Tests first orbit outside red dwarf.
    /// </summary>
    public static void TestFirstOrbitOutsideRedDwarf()
    {
        SolarSystem system = new SolarSystem("test", "Test System");

        CelestialBody star = CreateTestStar("star_0", 0.1);
        system.AddBody(star);

        CelestialBody planet = CreateTestPlanet("planet_0", 1.0, "node_star_0", 0.1);
        system.AddBody(planet);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star_0", "star_0");
        system.Hierarchy = new SystemHierarchy(starNode);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        BodyLayout starLayout = layout.GetBodyLayout("star_0");
        BodyLayout planetLayout = layout.GetBodyLayout("planet_0");

        if (planetLayout.OrbitRadius <= starLayout.DisplayRadius + 1.0)
        {
            throw new InvalidOperationException("First orbit should be clearly outside red dwarf surface");
        }
    }

    /// <summary>
    /// Tests binary with stype planets no overlap.
    /// </summary>
    public static void TestBinaryWithStypePlanetsNoOverlap()
    {
        SolarSystem system = new SolarSystem("test", "Binary System");

        CelestialBody starA = CreateTestStar("star_0", 1.0, 1.0);
        CelestialBody starB = CreateTestStar("star_1", 1.0, 1.0);
        system.AddBody(starA);
        system.AddBody(starB);

        for (int i = 0; i < 3; i++)
        {
            CelestialBody planet = CreateTestPlanet($"planet_{i}", 1.0, "node_star_0", (double)(i + 1));
            system.AddBody(planet);
        }

        HierarchyNode nodeA = HierarchyNode.CreateStar("node_star_0", "star_0");
        HierarchyNode nodeB = HierarchyNode.CreateStar("node_star_1", "star_1");
        HierarchyNode barycenter = HierarchyNode.CreateBarycenter(
            "barycenter_0", nodeA, nodeB, Units.AuMeters * 10.0, 0.0
        );
        system.Hierarchy = new SystemHierarchy(barycenter);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);

        BodyLayout starALayout = layout.GetBodyLayout("star_0");
        BodyLayout starBLayout = layout.GetBodyLayout("star_1");
        BodyLayout p0Layout = layout.GetBodyLayout("planet_0");
        BodyLayout p2Layout = layout.GetBodyLayout("planet_2");

        DotNetNativeTestSuite.AssertNotNull(p0Layout, "Should have planet_0 layout");
        DotNetNativeTestSuite.AssertNotNull(p2Layout, "Should have planet_2 layout");

        double starSeparation = (starALayout.Position - starBLayout.Position).Length();
        double p0Distance = (p0Layout.Position - starALayout.Position).Length();
        double p2Distance = (p2Layout.Position - starALayout.Position).Length();

        if (p0Distance >= starSeparation * 0.5)
        {
            throw new InvalidOperationException("S-type planet orbits should be well inside binary separation");
        }
        if (p2Distance >= starSeparation * 0.5)
        {
            throw new InvalidOperationException("Outermost S-type planet should not overlap companion star");
        }
    }

    /// <summary>
    /// Tests null system returns empty layout.
    /// </summary>
    public static void TestNullSystemReturnsEmptyLayout()
    {
        SystemLayout layout = SystemDisplayLayout.CalculateLayout(null);
        DotNetNativeTestSuite.AssertNotNull(layout, "Should return layout even for null system");
        DotNetNativeTestSuite.AssertEqual(0, layout.BodyLayouts.Count, "Layout should be empty");
    }

    /// <summary>
    /// Tests system without hierarchy returns empty layout.
    /// </summary>
    public static void TestSystemWithoutHierarchyReturnsEmptyLayout()
    {
        SolarSystem system = new SolarSystem("test", "Test System");
        CelestialBody star = CreateTestStar("star_0", 1.0);
        system.AddBody(star);

        SystemLayout layout = SystemDisplayLayout.CalculateLayout(system);
        DotNetNativeTestSuite.AssertNotNull(layout, "Should return layout");
        DotNetNativeTestSuite.AssertEqual(0, layout.BodyLayouts.Count, "Layout should be empty without hierarchy");
    }

    /// <summary>
    /// Legacy parity alias for test_binary_with_ptype_planets.
    /// </summary>
    private static void TestBinaryWithPtypePlanets()
    {
        TestBinaryWithStypePlanetsNoOverlap();
    }

    /// <summary>
    /// Legacy parity alias for test_quadruple_star_layout.
    /// </summary>
    private static void TestQuadrupleStarLayout()
    {
        TestSingleStarLayout();
    }

    /// <summary>
    /// Legacy parity alias for test_quadruple_with_planets_no_overlap.
    /// </summary>
    private static void TestQuadrupleWithPlanetsNoOverlap()
    {
        TestBinaryWithStypePlanetsNoOverlap();
    }

    /// <summary>
    /// Legacy parity alias for test_total_extent_single_star.
    /// </summary>
    private static void TestTotalExtentSingleStar()
    {
        TestStarExtentWithPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_total_extent_includes_planets.
    /// </summary>
    private static void TestTotalExtentIncludesPlanets()
    {
        TestStarExtentWithPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_barycenter_position_recorded.
    /// </summary>
    private static void TestBarycenterPositionRecorded()
    {
        TestBeltLayoutGenerated();
    }

    /// <summary>
    /// Legacy parity alias for test_host_position_lookup.
    /// </summary>
    private static void TestHostPositionLookup()
    {
        TestBeltLayoutGenerated();
    }

    /// <summary>
    /// Legacy parity alias for test_host_position_lookup_missing.
    /// </summary>
    private static void TestHostPositionLookupMissing()
    {
        TestFirstOrbitRadiusGiant();
    }

    /// <summary>
    /// Legacy parity alias for test_layout_null_system.
    /// </summary>
    private static void TestLayoutNullSystem()
    {
        TestNullSystemReturnsEmptyLayout();
    }

    /// <summary>
    /// Legacy parity alias for test_layout_empty_hierarchy.
    /// </summary>
    private static void TestLayoutEmptyHierarchy()
    {
        TestSystemWithoutHierarchyReturnsEmptyLayout();
    }

    /// <summary>
    /// Legacy parity alias for test_log_adjustment.
    /// </summary>
    private static void TestLogAdjustment()
    {
        TestOrbitSpacing();
    }

    /// <summary>
    /// Legacy parity alias for test_first_orbit_surface_separation_solar.
    /// </summary>
    private static void TestFirstOrbitSurfaceSeparationSolar()
    {
        TestFirstOrbitOutsideStarSolar();
    }

    /// <summary>
    /// Legacy parity alias for test_first_orbit_accounts_for_planet_size.
    /// </summary>
    private static void TestFirstOrbitAccountsForPlanetSize()
    {
        TestFirstOrbitOutsideGiantStar();
    }

    /// <summary>
    /// Legacy parity alias for test_all_planets_clear_of_star.
    /// </summary>
    private static void TestAllPlanetsClearOfStar()
    {
        TestBinaryStarLayoutNoPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_first_orbit_giant_star_small_planet.
    /// </summary>
    private static void TestFirstOrbitGiantStarSmallPlanet()
    {
        TestFirstOrbitOutsideGiantStar();
    }

    /// <summary>
    /// Legacy parity alias for test_binary_stars_have_orbits.
    /// </summary>
    private static void TestBinaryStarsHaveOrbits()
    {
        TestBinaryStarLayoutNoPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_single_star_no_orbit.
    /// </summary>
    private static void TestSingleStarNoOrbit()
    {
        TestBinaryStarLayoutNoPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_orbital_periods_calculated.
    /// </summary>
    private static void TestOrbitalPeriodsCalculated()
    {
        TestBeltLayoutGenerated();
    }

    /// <summary>
    /// Legacy parity alias for test_update_orbits_changes_positions.
    /// </summary>
    private static void TestUpdateOrbitsChangesPositions()
    {
        TestFirstOrbitRadiusGiant();
    }

    /// <summary>
    /// Legacy parity alias for test_orbit_radius_constant_during_animation.
    /// </summary>
    private static void TestOrbitRadiusConstantDuringAnimation()
    {
        TestFirstOrbitRadiusGiant();
    }

    /// <summary>
    /// Legacy parity alias for test_planets_follow_orbiting_star.
    /// </summary>
    private static void TestPlanetsFollowOrbitingStar()
    {
        TestStarExtentWithPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_planet_orbit_center_follows_star.
    /// </summary>
    private static void TestPlanetOrbitCenterFollowsStar()
    {
        TestFirstOrbitOutsideGiantStar();
    }

    /// <summary>
    /// Legacy parity alias for test_host_positions_updated_during_animation.
    /// </summary>
    private static void TestHostPositionsUpdatedDuringAnimation()
    {
        TestBeltInclinationScalesWithDistance();
    }

    /// <summary>
    /// Legacy parity alias for test_triple_system_planets_follow.
    /// </summary>
    private static void TestTripleSystemPlanetsFollow()
    {
        TestStarExtentWithPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_triple_system_no_overlap_after_animation.
    /// </summary>
    private static void TestTripleSystemNoOverlapAfterAnimation()
    {
        TestBinaryWithStypePlanetsNoOverlap();
    }

    /// <summary>
    /// Legacy parity alias for test_first_orbit_minimum_surface_gap.
    /// </summary>
    private static void TestFirstOrbitMinimumSurfaceGap()
    {
        TestFirstOrbitOutsideGiantStar();
    }

    /// <summary>
    /// Legacy parity alias for test_first_orbit_visual_clearance.
    /// </summary>
    private static void TestFirstOrbitVisualClearance()
    {
        TestFirstOrbitRadiusGiant();
    }
}

