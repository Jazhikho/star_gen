#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for GalaxySystemGenerator class.
/// </summary>
public static class TestGalaxySystemGenerator
{
    /// <summary>
    /// Creates a test star for generator tests.
    /// </summary>
    private static GalaxyStar MakeTestStar(int starSeed)
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        return GalaxyStar.CreateWithDerivedProperties(new Vector3(8000.0f, 0.0f, 0.0f), starSeed, spec);
    }

    /// <summary>
    /// Tests generate system from star.
    /// </summary>
    public static void TestGenerateSystemFromStar()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(8000.0f, 0.0f, 0.0f), 12345, spec
        );

        SolarSystem system = GalaxySystemGenerator.GenerateSystem(star);
        DotNetNativeTestSuite.AssertNotNull(system, "Should generate system");
        if (system.GetStarCount() <= 0)
        {
            throw new InvalidOperationException("System should have at least one star");
        }
    }

    /// <summary>
    /// Tests generate system deterministic.
    /// </summary>
    public static void TestGenerateSystemDeterministic()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar starA = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(5000.0f, 0.0f, 0.0f), 99999, spec
        );
        GalaxyStar starB = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(5000.0f, 0.0f, 0.0f), 99999, spec
        );

        SolarSystem systemA = GalaxySystemGenerator.GenerateSystem(starA);
        SolarSystem systemB = GalaxySystemGenerator.GenerateSystem(starB);

        DotNetNativeTestSuite.AssertEqual(systemA.GetStarCount(), systemB.GetStarCount(), "Same seed should give same star count");
        DotNetNativeTestSuite.AssertEqual(systemA.GetPlanetCount(), systemB.GetPlanetCount(), "Same seed should give same planet count");
        DotNetNativeTestSuite.AssertEqual(systemA.GetMoonCount(), systemB.GetMoonCount(), "Same seed should give same moon count");

        if (systemA.GetStarCount() > 0 && systemB.GetStarCount() > 0)
        {
            CelestialBody starABody = systemA.GetStars()[0];
            CelestialBody starBBody = systemB.GetStars()[0];
            DotNetNativeTestSuite.AssertEqual(starABody.Physical.MassKg, starBBody.Physical.MassKg, "Same seed should give same star mass");
        }
    }

    /// <summary>
    /// Tests generate system different seeds different results.
    /// </summary>
    public static void TestGenerateSystemDifferentSeedsDifferentResults()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar starA = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(5000.0f, 0.0f, 0.0f), 111, spec
        );
        GalaxyStar starB = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(5000.0f, 0.0f, 0.0f), 222, spec
        );

        SolarSystem systemA = GalaxySystemGenerator.GenerateSystem(starA);
        SolarSystem systemB = GalaxySystemGenerator.GenerateSystem(starB);

        bool anyDifferent = (
            systemA.GetStarCount() != systemB.GetStarCount() ||
            systemA.GetPlanetCount() != systemB.GetPlanetCount() ||
            systemA.Id != systemB.Id
        );
        if (!anyDifferent)
        {
            throw new InvalidOperationException("Different seeds should produce different systems");
        }
    }

    /// <summary>
    /// Tests generate system null star returns null.
    /// </summary>
    public static void TestGenerateSystemNullStarReturnsNull()
    {
        SolarSystem system = GalaxySystemGenerator.GenerateSystem(null);
        DotNetNativeTestSuite.AssertNull(system, "Null star should return null system");
    }

    /// <summary>
    /// Tests generate system without asteroids.
    /// </summary>
    public static void TestGenerateSystemWithoutAsteroids()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(8000.0f, 0.0f, 0.0f), 54321, spec
        );

        SolarSystem system = GalaxySystemGenerator.GenerateSystem(star, false);
        DotNetNativeTestSuite.AssertNotNull(system, "Should generate system without asteroids");
        DotNetNativeTestSuite.AssertEqual(0, system.AsteroidBelts.Count, "Should have no asteroid belts");
    }

    /// <summary>
    /// Tests metallicity applied to spec.
    /// </summary>
    public static void TestMetallicityAppliedToSpec()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(500.0f, 0.0f, 0.0f), 11111, spec
        );

        SolarSystem system = GalaxySystemGenerator.GenerateSystem(star);
        DotNetNativeTestSuite.AssertNotNull(system, "Should generate system with metallicity context");
        DotNetNativeTestSuite.AssertNotNull(system.Provenance, "System should have provenance");
        DotNetNativeTestSuite.AssertNotNull(system.Provenance.SpecSnapshot, "Provenance should have spec snapshot");
        if (!system.Provenance.SpecSnapshot.ContainsKey("system_metallicity"))
        {
            throw new InvalidOperationException("Spec snapshot should contain metallicity");
        }
    }

    /// <summary>
    /// Tests generate system has valid hierarchy.
    /// </summary>
    public static void TestGenerateSystemHasValidHierarchy()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(8000.0f, 0.0f, 0.0f), 77777, spec
        );

        SolarSystem system = GalaxySystemGenerator.GenerateSystem(star);
        DotNetNativeTestSuite.AssertNotNull(system, "Should generate system");
        DotNetNativeTestSuite.AssertNotNull(system.Hierarchy, "System should have hierarchy");
        if (!system.Hierarchy.IsValid())
        {
            throw new InvalidOperationException("Hierarchy should be valid");
        }
    }

    /// <summary>
    /// Tests generate system planets have parent ids.
    /// </summary>
    public static void TestGenerateSystemPlanetsHaveParentIds()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(8000.0f, 0.0f, 0.0f), 88888, spec
        );

        SolarSystem system = GalaxySystemGenerator.GenerateSystem(star);
        DotNetNativeTestSuite.AssertNotNull(system, "Should generate system");

        Array<CelestialBody> planets = system.GetPlanets();
        foreach (CelestialBody planet in planets)
        {
            if (!planet.HasOrbital())
            {
                throw new InvalidOperationException("Planet should have orbital properties");
            }
            if (string.IsNullOrEmpty(planet.Orbital.ParentId))
            {
                throw new InvalidOperationException("Planet should have parent_id set");
            }
            OrbitHost host = system.GetOrbitHost(planet.Orbital.ParentId);
            DotNetNativeTestSuite.AssertNotNull(host, "Planet parent_id should reference valid orbit host");
        }
    }

    /// <summary>
    /// Tests generate system provenance has spec snapshot.
    /// </summary>
    public static void TestGenerateSystemProvenanceHasSpecSnapshot()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(8000.0f, 0.0f, 0.0f), 55555, spec
        );

        SolarSystem system = GalaxySystemGenerator.GenerateSystem(star);
        DotNetNativeTestSuite.AssertNotNull(system, "Should generate system");
        DotNetNativeTestSuite.AssertNotNull(system.Provenance, "System should have provenance");
        if (system.Provenance.SpecSnapshot.Count == 0)
        {
            throw new InvalidOperationException("Provenance should have spec snapshot");
        }
        if (!system.Provenance.SpecSnapshot.ContainsKey("generation_seed"))
        {
            throw new InvalidOperationException("Spec snapshot should have generation_seed");
        }
        DotNetNativeTestSuite.AssertEqual(star.StarSeed, system.Provenance.SpecSnapshot["generation_seed"].AsInt32(), "Spec snapshot seed should match star seed");
    }

    /// <summary>
    /// Tests generate system without overrides unchanged.
    /// </summary>
    public static void TestGenerateSystemWithoutOverridesUnchanged()
    {
        GalaxyStar star = MakeTestStar(777);
        SolarSystem sysA = GalaxySystemGenerator.GenerateSystem(star, false, false, null);
        SolarSystem sysB = GalaxySystemGenerator.GenerateSystem(star, false, false);
        DotNetNativeTestSuite.AssertNotNull(sysA, "Should generate system A");
        DotNetNativeTestSuite.AssertNotNull(sysB, "Should generate system B");
        Array<CelestialBody> planetsA = sysA.GetPlanets();
        Array<CelestialBody> planetsB = sysB.GetPlanets();
        DotNetNativeTestSuite.AssertEqual(planetsA.Count, planetsB.Count, "null overrides must not change body count");
    }

    /// <summary>
    /// Tests generate system applies planet override.
    /// </summary>
    public static void TestGenerateSystemAppliesPlanetOverride()
    {
        GalaxyStar star = MakeTestStar(12345);
        SolarSystem baseline = GalaxySystemGenerator.GenerateSystem(star, false, false);
        DotNetNativeTestSuite.AssertNotNull(baseline, "Should generate baseline system");
        Array<CelestialBody> planets = baseline.GetPlanets();
        if (planets.Count == 0)
        {
            return;
        }
        CelestialBody targetPlanet = planets[0];
        string targetId = targetPlanet.Id;

        double editedMass = 7.777e24;
        GalaxyBodyOverrides ov = new GalaxyBodyOverrides();
        CelestialBody edited = CelestialSerializer.FromDictionary(CelestialSerializer.ToDict(targetPlanet));
        edited.Physical.MassKg = editedMass;
        edited.Name = "Edited-By-Test";
        ov.SetOverride(star.StarSeed, edited);

        SolarSystem patched = GalaxySystemGenerator.GenerateSystem(star, false, false, ov);
        DotNetNativeTestSuite.AssertNotNull(patched, "Should generate patched system");
        CelestialBody patchedPlanet = patched.GetBody(targetId);
        DotNetNativeTestSuite.AssertNotNull(patchedPlanet, "override body id must still resolve");
        DotNetNativeTestSuite.AssertFloatNear(editedMass, patchedPlanet.Physical.MassKg, 1.0, "override mass must replace deterministic value");
        DotNetNativeTestSuite.AssertEqual("Edited-By-Test", patchedPlanet.Name, "override name must survive");
    }

    /// <summary>
    /// Tests generate system ignores overrides for other seeds.
    /// </summary>
    public static void TestGenerateSystemIgnoresOverridesForOtherSeeds()
    {
        GalaxyStar star = MakeTestStar(500);
        SolarSystem baseline = GalaxySystemGenerator.GenerateSystem(star, false, false);
        DotNetNativeTestSuite.AssertNotNull(baseline, "Should generate baseline system");

        GalaxyBodyOverrides ov = new GalaxyBodyOverrides();
        PhysicalProps dummyPhys = new PhysicalProps(1.0, 1.0);
        CelestialBody dummy = new CelestialBody("wont_match", "Nope", CelestialType.Type.Planet, dummyPhys, null);
        ov.SetOverride(999999, dummy);

        SolarSystem patched = GalaxySystemGenerator.GenerateSystem(star, false, false, ov);
        DotNetNativeTestSuite.AssertEqual(baseline.GetPlanets().Count, patched.GetPlanets().Count, "wrong-seed override must not change structure");
        if (baseline.GetPlanets().Count > 0)
        {
            CelestialBody a = baseline.GetPlanets()[0];
            CelestialBody b = patched.GetPlanets()[0];
            DotNetNativeTestSuite.AssertFloatNear(a.Physical.MassKg, b.Physical.MassKg, 0.0, "wrong-seed override must not affect generation");
        }
    }
}
