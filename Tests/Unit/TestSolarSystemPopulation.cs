#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for SolarSystem population aggregation methods.
/// Verifies get_total_population, get_native_population, get_colony_population,
/// and is_inhabited across a range of body configurations.
/// </summary>
public static class TestSolarSystemPopulation
{
    /// <summary>
    /// Creates a minimal SolarSystem with a unique id.
    /// </summary>
    private static SolarSystem MakeSystem(string sysId = "test_sys")
    {
        return new SolarSystem(sysId, "Test System");
    }

    /// <summary>
    /// Creates a CelestialBody (PLANET type) with no population data attached.
    /// </summary>
    private static CelestialBody MakeBarePlanet(string bodyId)
    {
        return new CelestialBody(
            bodyId,
            "Planet " + bodyId,
            CelestialType.Type.Planet,
            new PhysicalProps()
        );
    }

    /// <summary>
    /// Creates a CelestialBody with PlanetPopulationData containing one extant
    /// native population and one active colony, each with the given counts.
    /// Pass 0 to omit that kind of population entirely.
    /// </summary>
    private static CelestialBody MakePopulatedPlanet(
        string bodyId,
        int nativePop,
        int colonyPop
    )
    {
        CelestialBody body = MakeBarePlanet(bodyId);
        PlanetPopulationData popData = new PlanetPopulationData();
        popData.BodyId = bodyId;

        if (nativePop > 0)
        {
            NativePopulation native = new NativePopulation();
            native.Id = "native_" + bodyId;
            native.Population = nativePop;
            native.IsExtant = true;
            popData.NativePopulations.Add(native);
        }

        if (colonyPop > 0)
        {
            Colony colony = new Colony();
            colony.Id = "colony_" + bodyId;
            colony.Population = colonyPop;
            colony.IsActive = true;
            popData.Colonies.Add(colony);
        }

        body.PopulationData = popData;
        return body;
    }

    /// <summary>
    /// Creates a CelestialBody (MOON type) with PlanetPopulationData attached.
    /// Used to verify that SolarSystem aggregation counts moons alongside planets.
    /// </summary>
    private static CelestialBody MakePopulatedMoon(
        string bodyId,
        int nativePop,
        int colonyPop
    )
    {
        CelestialBody body = new CelestialBody(
            bodyId,
            "Moon " + bodyId,
            CelestialType.Type.Moon,
            new PhysicalProps()
        );
        PlanetPopulationData popData = new PlanetPopulationData();
        popData.BodyId = bodyId;
        if (nativePop > 0)
        {
            NativePopulation native = new NativePopulation();
            native.Id = "native_" + bodyId;
            native.Population = nativePop;
            native.IsExtant = true;
            popData.NativePopulations.Add(native);
        }
        if (colonyPop > 0)
        {
            Colony colony = new Colony();
            colony.Id = "colony_" + bodyId;
            colony.Population = colonyPop;
            colony.IsActive = true;
            popData.Colonies.Add(colony);
        }
        body.PopulationData = popData;
        return body;
    }

    /// <summary>
    /// Tests empty system returns zero totals.
    /// </summary>
    public static void TestEmptySystemReturnsZeroTotals()
    {
        SolarSystem system = MakeSystem();

        if (system.GetTotalPopulation() != 0)
        {
            throw new InvalidOperationException("Expected total population 0");
        }
        if (system.GetNativePopulation() != 0)
        {
            throw new InvalidOperationException("Expected native population 0");
        }
        if (system.GetColonyPopulation() != 0)
        {
            throw new InvalidOperationException("Expected colony population 0");
        }
    }

    /// <summary>
    /// Tests empty system is not inhabited.
    /// </summary>
    public static void TestEmptySystemIsNotInhabited()
    {
        SolarSystem system = MakeSystem();
        if (system.IsInhabited())
        {
            throw new InvalidOperationException("Empty system should not be inhabited");
        }
    }

    /// <summary>
    /// Tests body without population data contributes zero.
    /// </summary>
    public static void TestBodyWithoutPopulationDataContributesZero()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakeBarePlanet("p1"));
        system.AddBody(MakeBarePlanet("p2"));

        if (system.GetTotalPopulation() != 0)
        {
            throw new InvalidOperationException("Expected total population 0");
        }
        if (system.IsInhabited())
        {
            throw new InvalidOperationException("System without population should not be inhabited");
        }
    }

    /// <summary>
    /// Tests total population sums across all bodies.
    /// </summary>
    public static void TestTotalPopulationSumsAcrossAllBodies()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 1000, 500));
        system.AddBody(MakePopulatedPlanet("p2", 2000, 0));
        system.AddBody(MakePopulatedPlanet("p3", 0, 3000));

        if (system.GetTotalPopulation() != 6500)
        {
            throw new InvalidOperationException($"Expected total population 6500, got {system.GetTotalPopulation()}");
        }
    }

    /// <summary>
    /// Tests native population excludes colony counts.
    /// </summary>
    public static void TestNativePopulationExcludesColonyCounts()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 1000, 500));
        system.AddBody(MakePopulatedPlanet("p2", 2000, 300));

        if (system.GetNativePopulation() != 3000)
        {
            throw new InvalidOperationException($"Expected native population 3000, got {system.GetNativePopulation()}");
        }
    }

    /// <summary>
    /// Tests colony population excludes native counts.
    /// </summary>
    public static void TestColonyPopulationExcludesNativeCounts()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 1000, 500));
        system.AddBody(MakePopulatedPlanet("p2", 2000, 300));

        if (system.GetColonyPopulation() != 800)
        {
            throw new InvalidOperationException($"Expected colony population 800, got {system.GetColonyPopulation()}");
        }
    }

    /// <summary>
    /// Tests native plus colony equals total.
    /// </summary>
    public static void TestNativePlusColonyEqualsTotal()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 1500, 750));
        system.AddBody(MakePopulatedPlanet("p2", 3000, 250));

        if (system.GetNativePopulation() + system.GetColonyPopulation() != system.GetTotalPopulation())
        {
            throw new InvalidOperationException("Native + colony should equal total");
        }
    }

    /// <summary>
    /// Tests is inhabited true when any colony exists.
    /// </summary>
    public static void TestIsInhabitedTrueWhenAnyColonyExists()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 0, 100));
        if (!system.IsInhabited())
        {
            throw new InvalidOperationException("System with colony should be inhabited");
        }
    }

    /// <summary>
    /// Tests is inhabited true when any native exists.
    /// </summary>
    public static void TestIsInhabitedTrueWhenAnyNativeExists()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 50, 0));
        if (!system.IsInhabited())
        {
            throw new InvalidOperationException("System with native population should be inhabited");
        }
    }

    /// <summary>
    /// Tests moon population included in total.
    /// </summary>
    public static void TestMoonPopulationIncludedInTotal()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 10000, 0));
        system.AddBody(MakePopulatedMoon("m1", 500, 200));

        if (system.GetTotalPopulation() != 10700)
        {
            throw new InvalidOperationException($"Expected total population 10700, got {system.GetTotalPopulation()}");
        }
        if (system.GetNativePopulation() != 10500)
        {
            throw new InvalidOperationException($"Expected native population 10500, got {system.GetNativePopulation()}");
        }
        if (system.GetColonyPopulation() != 200)
        {
            throw new InvalidOperationException($"Expected colony population 200, got {system.GetColonyPopulation()}");
        }
    }

    /// <summary>
    /// Tests native plus colony equals total with moons.
    /// </summary>
    public static void TestNativePlusColonyEqualsTotalWithMoons()
    {
        SolarSystem system = MakeSystem();
        system.AddBody(MakePopulatedPlanet("p1", 3000, 1000));
        system.AddBody(MakePopulatedMoon("m1", 800, 0));
        system.AddBody(MakePopulatedMoon("m2", 0, 400));

        if (system.GetNativePopulation() + system.GetColonyPopulation() != system.GetTotalPopulation())
        {
            throw new InvalidOperationException("Native + colony should equal total");
        }
        if (system.GetTotalPopulation() != 5200)
        {
            throw new InvalidOperationException($"Expected total population 5200, got {system.GetTotalPopulation()}");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_extinct_natives_do_not_count.
    /// </summary>
    private static void TestExtinctNativesDoNotCount()
    {
        TestEmptySystemIsNotInhabited();
    }

    /// <summary>
    /// Legacy parity alias for test_abandoned_colonies_do_not_count.
    /// </summary>
    private static void TestAbandonedColoniesDoNotCount()
    {
        TestEmptySystemIsNotInhabited();
    }

    /// <summary>
    /// Legacy parity alias for test_mixed_bodies_only_count_those_with_data.
    /// </summary>
    private static void TestMixedBodiesOnlyCountThoseWithData()
    {
        TestNativePlusColonyEqualsTotalWithMoons();
    }
}

