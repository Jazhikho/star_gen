#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for galaxy-to-system propagation.
/// </summary>
public static class TestGalaxySystemGenerator
{
    private static GalaxyStar MakeTestStar(Vector3 position, int seed, GalaxySpec? spec = null)
    {
        GalaxySpec effectiveSpec = spec ?? GalaxySpec.CreateMilkyWay(42);
        return GalaxyStar.CreateWithDerivedProperties(position, seed, effectiveSpec);
    }

    /// <summary>
    /// Tests that a galaxy star still generates a valid solar system.
    /// </summary>
    public static void TestGenerateSystemFromStar()
    {
        GalaxyStar star = MakeTestStar(new Vector3(8000.0f, 0.0f, 0.0f), 12345);
        SolarSystem? system = GalaxySystemGenerator.GenerateSystem(star);

        DotNetNativeTestSuite.AssertNotNull(system, "system generation should succeed");
        DotNetNativeTestSuite.AssertTrue(system!.GetStarCount() > 0, "generated system should contain at least one star");
    }

    /// <summary>
    /// Tests that system generation remains deterministic for the same galaxy star.
    /// </summary>
    public static void TestGenerateSystemDeterministic()
    {
        GalaxyStar starA = MakeTestStar(new Vector3(5000.0f, 0.0f, 0.0f), 99999);
        GalaxyStar starB = MakeTestStar(new Vector3(5000.0f, 0.0f, 0.0f), 99999);

        SolarSystem? systemA = GalaxySystemGenerator.GenerateSystem(starA);
        SolarSystem? systemB = GalaxySystemGenerator.GenerateSystem(starB);

        DotNetNativeTestSuite.AssertNotNull(systemA, "first system should generate");
        DotNetNativeTestSuite.AssertNotNull(systemB, "second system should generate");
        DotNetNativeTestSuite.AssertEqual(systemA!.GetStarCount(), systemB!.GetStarCount(), "same star seed should keep the stellar count deterministic");
        DotNetNativeTestSuite.AssertEqual(systemA.GetPlanetCount(), systemB.GetPlanetCount(), "same star seed should keep planet count deterministic");
    }

    /// <summary>
    /// Tests that different galaxy-star seeds still diverge.
    /// </summary>
    public static void TestGenerateSystemDifferentSeedsDifferentResults()
    {
        GalaxyStar starA = MakeTestStar(new Vector3(5000.0f, 0.0f, 0.0f), 111);
        GalaxyStar starB = MakeTestStar(new Vector3(5000.0f, 0.0f, 0.0f), 222);

        SolarSystem? systemA = GalaxySystemGenerator.GenerateSystem(starA);
        SolarSystem? systemB = GalaxySystemGenerator.GenerateSystem(starB);

        DotNetNativeTestSuite.AssertNotNull(systemA, "first system should generate");
        DotNetNativeTestSuite.AssertNotNull(systemB, "second system should generate");
        bool anyDifferent = systemA!.GetPlanetCount() != systemB!.GetPlanetCount() || systemA.Id != systemB.Id;
        DotNetNativeTestSuite.AssertTrue(anyDifferent, "different star seeds should still diverge");
    }

    /// <summary>
    /// Tests null stars still return null.
    /// </summary>
    public static void TestGenerateSystemNullStarReturnsNull()
    {
        SolarSystem? system = GalaxySystemGenerator.GenerateSystem(null);
        DotNetNativeTestSuite.AssertNull(system, "null star should return null");
    }

    /// <summary>
    /// Tests the no-asteroid path remains supported.
    /// </summary>
    public static void TestGenerateSystemWithoutAsteroids()
    {
        GalaxyStar star = MakeTestStar(new Vector3(8000.0f, 0.0f, 0.0f), 54321);
        SolarSystem? system = GalaxySystemGenerator.GenerateSystem(star, false);

        DotNetNativeTestSuite.AssertNotNull(system, "system should generate without asteroids");
        DotNetNativeTestSuite.AssertEqual(0, system!.AsteroidBelts.Count, "asteroid belts should remain disabled");
    }

    /// <summary>
    /// Tests that the system spec snapshot carries galaxy context.
    /// </summary>
    public static void TestSystemSpecSnapshotCarriesGalaxyContext()
    {
        GalaxyStar star = MakeTestStar(new Vector3(2500.0f, 0.0f, 0.0f), 11111);
        SolarSystem? system = GalaxySystemGenerator.GenerateSystem(star);

        DotNetNativeTestSuite.AssertNotNull(system, "system should generate");
        Dictionary snapshot = system!.Provenance.SpecSnapshot;
        DotNetNativeTestSuite.AssertTrue(snapshot.ContainsKey("galaxy_context"), "spec snapshot should contain galaxy context");
        Dictionary context = (Dictionary)snapshot["galaxy_context"];
        DotNetNativeTestSuite.AssertTrue(context.ContainsKey("metallicity_prior"), "serialized galaxy context should include metallicity prior");
        DotNetNativeTestSuite.AssertTrue(context.ContainsKey("age_mean_gyr"), "serialized galaxy context should include age context");
    }

    /// <summary>
    /// Tests that metal-rich and metal-poor galaxy regions propagate into stellar outputs.
    /// </summary>
    public static void TestGalaxyMetallicityPropagatesIntoGeneratedStars()
    {
        GalaxyStar innerStar = MakeTestStar(new Vector3(1500.0f, 0.0f, 0.0f), 60001);
        GalaxyStar outerStar = MakeTestStar(new Vector3(14500.0f, 0.0f, 0.0f), 60002);

        SolarSystem? innerSystem = GalaxySystemGenerator.GenerateSystem(innerStar, includeAsteroids: false, enablePopulation: false);
        SolarSystem? outerSystem = GalaxySystemGenerator.GenerateSystem(outerStar, includeAsteroids: false, enablePopulation: false);

        DotNetNativeTestSuite.AssertNotNull(innerSystem, "inner system should generate");
        DotNetNativeTestSuite.AssertNotNull(outerSystem, "outer system should generate");

        CelestialBody innerGeneratedStar = innerSystem!.GetStars()[0];
        CelestialBody outerGeneratedStar = outerSystem!.GetStars()[0];
        DotNetNativeTestSuite.AssertTrue(innerGeneratedStar.Stellar!.Metallicity >= outerGeneratedStar.Stellar!.Metallicity, "inner galaxy metallicity should not be lower than outer-galaxy metallicity");
    }

    /// <summary>
    /// Tests that galaxy-aged contexts propagate into the generated system star age.
    /// </summary>
    public static void TestGalaxyAgeContextPropagatesIntoGeneratedStars()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(777);
        GalaxyStar bulgeStar = MakeTestStar(new Vector3(200.0f, 0.0f, 0.0f), 77771, spec);
        GalaxyStar outerDiskStar = MakeTestStar(new Vector3(12000.0f, 0.0f, 0.0f), 77772, spec);

        SolarSystem? bulgeSystem = GalaxySystemGenerator.GenerateSystem(bulgeStar, includeAsteroids: false, enablePopulation: false);
        SolarSystem? outerDiskSystem = GalaxySystemGenerator.GenerateSystem(outerDiskStar, includeAsteroids: false, enablePopulation: false);

        DotNetNativeTestSuite.AssertNotNull(bulgeSystem, "bulge system should generate");
        DotNetNativeTestSuite.AssertNotNull(outerDiskSystem, "outer-disk system should generate");
        DotNetNativeTestSuite.AssertTrue(bulgeSystem!.GetStars()[0].Stellar!.AgeYears >= outerDiskSystem!.GetStars()[0].Stellar!.AgeYears, "older galaxy contexts should not produce younger stellar ages than the outer disk for the same family");
    }
}
