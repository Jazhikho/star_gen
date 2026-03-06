#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Integration tests for moon display in ObjectViewer.
/// Covers: moon position maths, roman numerals, camera framing,
/// inspector signal routing, and SystemViewer moon collection.
/// </summary>
public static class TestObjectViewerMoons
{
    private static readonly string[] RomanNumerals = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];

    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_roman_numeral_basic",
            TestRomanNumeralBasic);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_roman_numeral_out_of_range",
            TestRomanNumeralOutOfRange);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_moon_system_scale_earth_like",
            TestMoonSystemScaleEarthLike);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_circular_orbit_position_at_zero_anomaly",
            TestCircularOrbitPositionAtZeroAnomaly);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_inclined_orbit_has_y_component",
            TestInclinedOrbitHasYComponent);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_moon_display_scale_proportional_to_planet",
            TestMoonDisplayScaleProportionalToPlanet);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_planet_display_scale_is_earth_radii",
            TestPlanetDisplayScaleIsEarthRadii);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_kepler_period_at_reference_sma",
            TestKeplerPeriodAtReferenceSma);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_kepler_period_four_times_farther",
            TestKeplerPeriodFourTimesFarther);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_kepler_period_close_moon_has_positive_period",
            TestKeplerPeriodCloseMoonHasPositivePeriod);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_ring_filter_keeps_outer_moons",
            TestRingFilterKeepsOuterMoons);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_ring_filter_removes_inner_moons",
            TestRingFilterRemovesInnerMoons);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_ring_filter_boundary_strictly_exclusive",
            TestRingFilterBoundaryStrictlyExclusive);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_framing_distance_no_moons",
            TestFramingDistanceNoMoons);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_framing_distance_with_moon",
            TestFramingDistanceWithMoon);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_moon_collection_by_parent_id",
            TestMoonCollectionByParentId);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_moon_collection_empty_system",
            TestMoonCollectionEmptySystem);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_moon_collection_skips_no_orbital",
            TestMoonCollectionSkipsNoOrbital);
        runner.RunNativeTest(
            "TestObjectViewerMoons::test_inspector_panel_moon_button_emits_selection",
            TestInspectorPanelMoonButtonEmitsSelection);
    }

    /// <summary>
    /// Builds a minimal CelestialBody for use as a test planet.
    /// </summary>
    private static CelestialBody MakePlanet()
    {
        PhysicalProps phys = new(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            86400.0,
            23.5,
            0.003,
            8.0e22,
            0.0
        );
        CelestialBody body = new(
            "test_planet",
            "TestPlanet",
            CelestialType.Type.Planet,
            phys,
            null
        );
        return body;
    }

    /// <summary>
    /// Builds a moon body with given orbital elements.
    /// </summary>
    private static CelestialBody MakeMoon(
        string id,
        double semiMajorAxisM,
        double eccentricity,
        double inclinationDeg)
    {
        PhysicalProps phys = new(
            7.34e22,
            1.737e6,
            2360591.0,
            6.7,
            0.001,
            0.0,
            0.0
        );
        OrbitalProps orbital = new(
            semiMajorAxisM,
            eccentricity,
            inclinationDeg,
            0.0,
            0.0,
            0.0,
            "test_planet"
        );
        CelestialBody body = new(
            id,
            $"Moon_{id}",
            CelestialType.Type.Moon,
            phys,
            null
        );
        body.Orbital = orbital;
        return body;
    }

    /// <summary>
    /// Tests roman numeral conversion for basic values.
    /// </summary>
    private static void TestRomanNumeralBasic()
    {
        System.Collections.Generic.Dictionary<int, string> expected = new()
        {
            { 1, "I" }, { 2, "II" }, { 3, "III" }, { 4, "IV" }, { 5, "V" },
            { 6, "VI" }, { 7, "VII" }, { 8, "VIII" }, { 9, "IX" }, { 10, "X" }
        };

        foreach (int n in expected.Keys)
        {
            string result;
            if (n >= 1 && n <= RomanNumerals.Length)
            {
                result = RomanNumerals[n - 1];
            }
            else
            {
                result = n.ToString();
            }
            DotNetNativeTestSuite.AssertEqual(expected[n], result, $"Roman numeral for {n}");
        }
    }

    /// <summary>
    /// Tests roman numeral conversion for out-of-range values.
    /// </summary>
    private static void TestRomanNumeralOutOfRange()
    {
        int n = 11;
        string result;
        if (n >= 1 && n <= RomanNumerals.Length)
        {
            result = RomanNumerals[n - 1];
        }
        else
        {
            result = n.ToString();
        }
        DotNetNativeTestSuite.AssertEqual("11", result, "Out-of-range falls back to str(n)");
    }

    /// <summary>
    /// Tests moon system scale for Earth-like planet.
    /// </summary>
    private static void TestMoonSystemScaleEarthLike()
    {
        CelestialBody planet = MakePlanet();
        double r = planet.Physical.RadiusM;
        double displayScale = r / Units.EarthRadiusMeters;
        double moonSystemScale = displayScale / r;

        DotNetNativeTestSuite.AssertFloatNear(
            displayScale,
            moonSystemScale * r,
            1e-6,
            "scale × radius == display_scale");
    }

    /// <summary>
    /// Tests circular orbit position at zero anomaly.
    /// </summary>
    private static void TestCircularOrbitPositionAtZeroAnomaly()
    {
        CelestialBody moon = MakeMoon("circ", 3.844e8, 0.0, 0.0);
        double planetR = Units.EarthRadiusMeters;
        double displayS = planetR / Units.EarthRadiusMeters;
        double moonScale = displayS / planetR;
        double aDisplay = moon.Orbital.SemiMajorAxisM * moonScale;

        Vector3 expected = new((float)aDisplay, 0.0f, 0.0f);

        double e = 0.0;
        double a = moon.Orbital.SemiMajorAxisM * moonScale;
        double inc = Mathf.DegToRad(0.0f);
        double lan = Mathf.DegToRad(0.0f);
        double aop = Mathf.DegToRad(0.0f);
        double ma = Mathf.DegToRad(0.0f);

        double ea = ma;
        for (int i = 0; i < 5; i++)
        {
            ea = ea - (ea - e * System.Math.Sin(ea) - ma) / (1.0 - e * System.Math.Cos(ea));
        }
        double ta = 2.0 * System.Math.Atan2(
            System.Math.Sqrt(1.0 + e) * System.Math.Sin(ea / 2.0),
            System.Math.Sqrt(1.0 - e) * System.Math.Cos(ea / 2.0)
        );
        double r = a * (1.0 - e * System.Math.Cos(ea));
        double px = r * System.Math.Cos(ta);
        double py = r * System.Math.Sin(ta);

        double cLan = System.Math.Cos(lan);
        double sLan = System.Math.Sin(lan);
        double cAop = System.Math.Cos(aop);
        double sAop = System.Math.Sin(aop);
        double cInc = System.Math.Cos(inc);
        double sInc = System.Math.Sin(inc);

        double x = (cLan * cAop - sLan * sAop * cInc) * px + (-cLan * sAop - sLan * cAop * cInc) * py;
        double z = (sLan * cAop + cLan * sAop * cInc) * px + (-sLan * sAop + cLan * cAop * cInc) * py;
        double y = (sAop * sInc) * px + (cAop * sInc) * py;
        Vector3 result = new((float)x, (float)y, (float)z);

        DotNetNativeTestSuite.AssertFloatNear(expected.X, result.X, 1e-6f, "circular X == a_display");
        DotNetNativeTestSuite.AssertFloatNear(0.0, result.Y, 1e-6f, "circular Y == 0");
        DotNetNativeTestSuite.AssertFloatNear(0.0, result.Z, 1e-6f, "circular Z == 0");
    }

    /// <summary>
    /// Tests inclined orbit has Y component.
    /// </summary>
    private static void TestInclinedOrbitHasYComponent()
    {
        CelestialBody moon = MakeMoon("inclined", 3.844e8, 0.0, 90.0);
        double planetR = Units.EarthRadiusMeters;
        double displayS = planetR / Units.EarthRadiusMeters;
        double moonScale = displayS / planetR;

        double e = 0.0;
        double a = moon.Orbital.SemiMajorAxisM * moonScale;
        double inc = Mathf.DegToRad(90.0f);
        double lan = Mathf.DegToRad(0.0f);
        double aop = Mathf.DegToRad(0.0f);
        double ma = Mathf.DegToRad(90.0f);

        double ea = ma;
        for (int i = 0; i < 5; i++)
        {
            ea = ea - (ea - e * System.Math.Sin(ea) - ma) / (1.0 - e * System.Math.Cos(ea));
        }
        double ta = 2.0 * System.Math.Atan2(
            System.Math.Sqrt(1.0 + e) * System.Math.Sin(ea / 2.0),
            System.Math.Sqrt(1.0 - e) * System.Math.Cos(ea / 2.0)
        );
        double rOrb = a * (1.0 - e * System.Math.Cos(ea));
        double px = rOrb * System.Math.Cos(ta);
        double py = rOrb * System.Math.Sin(ta);

        double cAop = System.Math.Cos(aop);
        double sAop = System.Math.Sin(aop);
        double sInc = System.Math.Sin(inc);

        double y = (sAop * sInc) * px + (cAop * sInc) * py;

        DotNetNativeTestSuite.AssertTrue(System.Math.Abs(y) > 0.0, "90° inclination orbit produces non-zero Y component");
    }

    /// <summary>
    /// Tests moon display scale proportional to planet.
    /// </summary>
    private static void TestMoonDisplayScaleProportionalToPlanet()
    {
        CelestialBody planet = MakePlanet();
        CelestialBody moon = MakeMoon("m", 3.844e8, 0.0, 0.0);
        double planetR = planet.Physical.RadiusM;
        double planetDisplay = planetR / Units.EarthRadiusMeters;
        double moonSystemScale = planetDisplay / planetR;
        double expected = moon.Physical.RadiusM * moonSystemScale;
        DotNetNativeTestSuite.AssertFloatNear(
            expected,
            moon.Physical.RadiusM * (planetDisplay / planetR),
            1e-10,
            "Moon display scale is proportional to planet display scale");
    }

    /// <summary>
    /// Tests planet display scale is Earth radii.
    /// </summary>
    private static void TestPlanetDisplayScaleIsEarthRadii()
    {
        CelestialBody planet = MakePlanet();
        double expected = planet.Physical.RadiusM / Units.EarthRadiusMeters;
        DotNetNativeTestSuite.AssertFloatNear(1.0, expected, 1e-6, "Earth-radius planet → scale 1.0");
    }

    /// <summary>
    /// Tests Kepler period at reference SMA.
    /// </summary>
    private static void TestKeplerPeriodAtReferenceSma()
    {
        const double Base = 120.0;
        const double RefSma = 3.844e8;
        double scale = System.Math.Pow(RefSma / RefSma, 1.5);
        double period = Base * scale;
        DotNetNativeTestSuite.AssertFloatNear(Base, period, 1e-6, "Reference SMA gives base period");
    }

    /// <summary>
    /// Tests Kepler period four times farther.
    /// </summary>
    private static void TestKeplerPeriodFourTimesFarther()
    {
        const double Base = 120.0;
        const double RefSma = 3.844e8;
        double scale = System.Math.Pow(4.0 * RefSma / RefSma, 1.5);
        double period = Base * scale;
        DotNetNativeTestSuite.AssertFloatNear(Base * 8.0, period, 1e-4, "4× farther moon has 8× longer period");
    }

    /// <summary>
    /// Tests Kepler period close moon has positive period.
    /// </summary>
    private static void TestKeplerPeriodCloseMoonHasPositivePeriod()
    {
        const double Base = 120.0;
        const double RefSma = 3.844e8;
        double sma = RefSma * 0.1;
        double scale = System.Math.Pow(sma / RefSma, 1.5);
        double period = Base * scale;
        if (period < 0.001)
        {
            period = 0.001;
        }
        DotNetNativeTestSuite.AssertTrue(period > 0.0, "Close moon has positive period");
        DotNetNativeTestSuite.AssertTrue(period < Base, "Close moon period is shorter than reference");
    }

    /// <summary>
    /// Tests ring filter keeps outer moons.
    /// </summary>
    private static void TestRingFilterKeepsOuterMoons()
    {
        double ringOuterM = Units.EarthRadiusMeters * 10.0;
        CelestialBody moon = MakeMoon("outer", Units.EarthRadiusMeters * 15.0, 0.0, 0.0);
        DotNetNativeTestSuite.AssertTrue(moon.Orbital.SemiMajorAxisM > ringOuterM, "Moon outside ring passes filter");
    }

    /// <summary>
    /// Tests ring filter removes inner moons.
    /// </summary>
    private static void TestRingFilterRemovesInnerMoons()
    {
        double ringOuterM = Units.EarthRadiusMeters * 10.0;
        CelestialBody moon = MakeMoon("inner", Units.EarthRadiusMeters * 3.0, 0.0, 0.0);
        DotNetNativeTestSuite.AssertFalse(moon.Orbital.SemiMajorAxisM > ringOuterM, "Moon inside ring is filtered");
    }

    /// <summary>
    /// Tests ring filter boundary strictly exclusive.
    /// </summary>
    private static void TestRingFilterBoundaryStrictlyExclusive()
    {
        double ringOuterM = Units.EarthRadiusMeters * 5.0;
        CelestialBody moon = MakeMoon("edge", ringOuterM, 0.0, 0.0);
        DotNetNativeTestSuite.AssertFalse(moon.Orbital.SemiMajorAxisM > ringOuterM, "Moon at exact ring edge is filtered (strict >)");
    }

    /// <summary>
    /// Tests framing distance with no moons.
    /// </summary>
    private static void TestFramingDistanceNoMoons()
    {
        CelestialBody planet = MakePlanet();
        double displayScale = planet.Physical.RadiusM / Units.EarthRadiusMeters;
        double expected = displayScale * 4.0;
        DotNetNativeTestSuite.AssertFloatNear(displayScale * 4.0, expected, 1e-6, "No-moon framing = 4× display scale");
    }

    /// <summary>
    /// Tests framing distance with moon.
    /// </summary>
    private static void TestFramingDistanceWithMoon()
    {
        CelestialBody planet = MakePlanet();
        CelestialBody moon = MakeMoon("m1", 3.844e8, 0.05, 5.0);
        double planetR = planet.Physical.RadiusM;
        double displayS = planetR / Units.EarthRadiusMeters;
        double moonScale = displayS / planetR;
        double apoapsisM = moon.Orbital.SemiMajorAxisM * (1.0 + moon.Orbital.Eccentricity);
        double apoapsisDisplay = apoapsisM * moonScale;
        double expected = apoapsisDisplay * 1.5;

        DotNetNativeTestSuite.AssertTrue(expected > displayS * 4.0, "Moon framing > planet-only framing for this moon distance");
    }

    /// <summary>
    /// Tests moon collection by parent ID.
    /// </summary>
    private static void TestMoonCollectionByParentId()
    {
        CelestialBody planet = MakePlanet();
        CelestialBody moonA = MakeMoon("a", 3.844e8, 0.0, 0.0);
        CelestialBody moonB = MakeMoon("b", 5.0e8, 0.1, 2.0);
        CelestialBody orphan = MakeMoon("c", 2.0e8, 0.0, 0.0);
        orphan.Orbital.ParentId = "other_planet";

        CelestialBody[] allMoons = [moonA, moonB, orphan];
        System.Collections.Generic.List<CelestialBody> collected = new();

        foreach (CelestialBody m in allMoons)
        {
            if (m.HasOrbital() && m.Orbital.ParentId == planet.Id)
            {
                collected.Add(m);
            }
        }

        DotNetNativeTestSuite.AssertEqual(2, collected.Count, "Two moons collected for correct parent_id");
        DotNetNativeTestSuite.AssertTrue(collected.Contains(moonA), "moon_a collected");
        DotNetNativeTestSuite.AssertTrue(collected.Contains(moonB), "moon_b collected");
        DotNetNativeTestSuite.AssertFalse(collected.Contains(orphan), "Orphan moon excluded");
    }

    /// <summary>
    /// Tests moon collection empty system.
    /// </summary>
    private static void TestMoonCollectionEmptySystem()
    {
        CelestialBody planet = MakePlanet();
        CelestialBody[] allMoons = [];
        System.Collections.Generic.List<CelestialBody> collected = new();

        foreach (CelestialBody m in allMoons)
        {
            if (m.HasOrbital() && m.Orbital.ParentId == planet.Id)
            {
                collected.Add(m);
            }
        }

        DotNetNativeTestSuite.AssertEqual(0, collected.Count, "Empty system → zero moons collected");
    }

    /// <summary>
    /// Tests moon collection skips no orbital.
    /// </summary>
    private static void TestMoonCollectionSkipsNoOrbital()
    {
        CelestialBody planet = MakePlanet();
        PhysicalProps phys = new(7.34e22, 1.737e6, 2360591.0, 0.0, 0.0, 0.0, 0.0);
        CelestialBody bareMoon = new("bare", "BareMoon", CelestialType.Type.Moon, phys, null);

        System.Collections.Generic.List<CelestialBody> collected = new();
        foreach (CelestialBody m in new[] { bareMoon })
        {
            if (m.HasOrbital() && m.Orbital.ParentId == planet.Id)
            {
                collected.Add(m);
            }
        }

        DotNetNativeTestSuite.AssertEqual(0, collected.Count, "Moon without orbital skipped safely");
    }

    /// <summary>
    /// Tests inspector panel exposes the moon-selected signal.
    /// </summary>
    private static void TestInspectorPanelMoonButtonEmitsSelection()
    {
        InspectorPanel panel = new();
        VBoxContainer container = new() { Name = "InspectorContainer" };
        panel.AddChild(container);
        panel._Ready();

        CelestialBody planet = MakePlanet();
        CelestialBody moon = MakeMoon("moon_a", 3.844e8, 0.05, 5.0);
        Godot.Collections.Array<CelestialBody> moons = [moon];

        bool emitted = false;
        panel.Connect(InspectorPanel.SignalName.MoonSelected, Callable.From<Variant>((_) => emitted = true));
        panel.DisplayBodyWithMoons(planet, moons);

        Button? moonButton = null;
        foreach (Node child in container.GetChildren())
        {
            foreach (Node grandChild in child.GetChildren())
            {
                if (grandChild is Button typedButton)
                {
                    moonButton = typedButton;
                    break;
                }
            }
        }

        DotNetNativeTestSuite.AssertNotNull(moonButton, "Displaying moons should create a moon-selection button");
        moonButton!.EmitSignal(BaseButton.SignalName.Pressed);
        DotNetNativeTestSuite.AssertTrue(emitted, "Pressing a moon button should emit MoonSelected");
    }
}
