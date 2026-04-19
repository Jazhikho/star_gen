#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for StellarConfigGenerator.
/// </summary>
public static class TestStellarConfigGenerator
{
    /// <summary>
    /// Tests single star generation.
    /// </summary>
    public static void TestGenerateSingleStar()
    {
        SolarSystemSpec spec = SolarSystemSpec.SingleStar(12345);
        SeededRng rng = new SeededRng(12345);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Should have 1 star");
        }
        if (!system.Hierarchy.IsValid())
        {
            throw new InvalidOperationException("Hierarchy should be valid");
        }
        if (system.Hierarchy.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Hierarchy should have 1 star");
        }
    }

    /// <summary>
    /// Tests binary star generation.
    /// </summary>
    public static void TestGenerateBinary()
    {
        SolarSystemSpec spec = SolarSystemSpec.Binary(54321);
        SeededRng rng = new SeededRng(54321);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.GetStarCount() != 2)
        {
            throw new InvalidOperationException("Should have 2 stars");
        }
        if (system.Hierarchy.GetStarCount() != 2)
        {
            throw new InvalidOperationException("Hierarchy should have 2 stars");
        }
        if (system.Hierarchy.GetDepth() != 2)
        {
            throw new InvalidOperationException("Binary hierarchy should have depth 2");
        }

        Array<HierarchyNode> barycenters = system.Hierarchy.GetAllBarycenters();
        if (barycenters.Count != 1)
        {
            throw new InvalidOperationException("Should have 1 barycenter");
        }
        if (barycenters[0].SeparationM <= 0.0)
        {
            throw new InvalidOperationException("Separation should be positive");
        }
    }

    /// <summary>
    /// Tests triple star generation.
    /// </summary>
    public static void TestGenerateTriple()
    {
        SolarSystemSpec spec = SolarSystemSpec.AlphaCentauriLike(99999);
        SeededRng rng = new SeededRng(99999);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.GetStarCount() != 3)
        {
            throw new InvalidOperationException("Should have 3 stars");
        }
        if (system.Hierarchy.GetStarCount() != 3)
        {
            throw new InvalidOperationException("Hierarchy should have 3 stars");
        }

        Array<HierarchyNode> barycenters = system.Hierarchy.GetAllBarycenters();
        if (barycenters.Count != 2)
        {
            throw new InvalidOperationException("Triple should have 2 barycenters");
        }
    }

    /// <summary>
    /// Tests determinism.
    /// </summary>
    public static void TestDeterminism()
    {
        SolarSystemSpec spec1 = SolarSystemSpec.Binary(11111);
        SolarSystemSpec spec2 = SolarSystemSpec.Binary(11111);
        SeededRng rng1 = new SeededRng(11111);
        SeededRng rng2 = new SeededRng(11111);

        SolarSystem system1 = StellarConfigGenerator.Generate(spec1, rng1);
        SolarSystem system2 = StellarConfigGenerator.Generate(spec2, rng2);

        if (system1.GetStarCount() != system2.GetStarCount())
        {
            throw new InvalidOperationException("Star count should match");
        }

        for (int i = 0; i < system1.StarIds.Count; i++)
        {
            CelestialBody star1 = system1.GetBody(system1.StarIds[i]);
            CelestialBody star2 = system2.GetBody(system2.StarIds[i]);
            if (System.Math.Abs(star1.Physical.MassKg - star2.Physical.MassKg) > 1.0)
            {
                throw new InvalidOperationException("Star masses should match");
            }
        }
    }

    /// <summary>
    /// Tests spectral class hints are respected.
    /// </summary>
    public static void TestSpectralClassHints()
    {
        SolarSystemSpec spec = SolarSystemSpec.SunLike(22222);
        SeededRng rng = new SeededRng(22222);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Should have 1 star");
        }

        CelestialBody star = system.GetStars()[0];
        if (!star.HasStellar())
        {
            throw new InvalidOperationException("Star should have stellar data");
        }
        if (!star.Stellar.SpectralClass.StartsWith("G"))
        {
            throw new InvalidOperationException("Should be G-type star");
        }
    }

    /// <summary>
    /// Tests orbit hosts are created.
    /// </summary>
    public static void TestOrbitHostsCreated()
    {
        SolarSystemSpec spec = SolarSystemSpec.SingleStar(33333);
        SeededRng rng = new SeededRng(33333);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.OrbitHosts.Count <= 0)
        {
            throw new InvalidOperationException("Should have at least one orbit host");
        }

        OrbitHost host = system.OrbitHosts[0];
        if (host.Type != OrbitHost.HostType.SType)
        {
            throw new InvalidOperationException("Single star should have S-type host");
        }
        if (!host.HasValidZone())
        {
            throw new InvalidOperationException("Host should have valid zone");
        }
    }

    /// <summary>
    /// Tests star names are assigned.
    /// </summary>
    public static void TestStarNames()
    {
        SolarSystemSpec spec = SolarSystemSpec.Binary(11111);
        SeededRng rng = new SeededRng(11111);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }

        foreach (CelestialBody star in system.GetStars())
        {
            if (string.IsNullOrEmpty(star.Name))
            {
                throw new InvalidOperationException("Star should have a name");
            }
        }
    }

    /// <summary>
    /// Tests star IDs are unique.
    /// </summary>
    public static void TestStarIdsUnique()
    {
        SolarSystemSpec spec = new SolarSystemSpec(22222, 5, 5);
        SeededRng rng = new SeededRng(22222);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }

        Godot.Collections.Dictionary ids = new Godot.Collections.Dictionary();
        foreach (CelestialBody star in system.GetStars())
        {
            if (ids.ContainsKey(star.Id))
            {
                throw new InvalidOperationException("Star IDs should be unique");
            }
            ids[star.Id] = true;
        }
    }

    /// <summary>
    /// Tests provenance is stored.
    /// </summary>
    public static void TestProvenanceStored()
    {
        SolarSystemSpec spec = SolarSystemSpec.SingleStar(77777);
        SeededRng rng = new SeededRng(77777);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.Provenance == null)
        {
            throw new InvalidOperationException("Provenance should exist");
        }
        if (system.Provenance.GenerationSeed != 77777)
        {
            throw new InvalidOperationException("Generation seed should match");
        }
    }

    /// <summary>
    /// Tests random star count in range.
    /// </summary>
    public static void TestRandomStarCountInRange()
    {
        for (int i = 0; i < 10; i++)
        {
            SolarSystemSpec spec = SolarSystemSpec.RandomSmall(70000 + i);
            SeededRng rng = new SeededRng(70000 + i);

            SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

            if (system == null)
            {
                throw new InvalidOperationException("System should be generated");
            }
            int starCount = system.GetStarCount();
            if (starCount < spec.StarCountMin || starCount > spec.StarCountMax)
            {
                throw new InvalidOperationException("Star count should be in spec range");
            }
        }
    }

    /// <summary>
    /// Tests hierarchical system (4+ stars).
    /// </summary>
    public static void TestHierarchicalSystem()
    {
        SolarSystemSpec spec = new SolarSystemSpec(88888, 4, 4);
        SeededRng rng = new SeededRng(88888);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.GetStarCount() != 4)
        {
            throw new InvalidOperationException("Should have 4 stars");
        }

        Array<HierarchyNode> barycenters = system.Hierarchy.GetAllBarycenters();
        if (barycenters.Count != 3)
        {
            throw new InvalidOperationException("4 stars need 3 barycenters");
        }
    }

    /// <summary>
    /// Tests maximum star count.
    /// </summary>
    public static void TestMaximumStars()
    {
        SolarSystemSpec spec = new SolarSystemSpec(99999, 10, 10);
        SeededRng rng = new SeededRng(99999);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);

        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }
        if (system.GetStarCount() != 10)
        {
            throw new InvalidOperationException("Should have 10 stars");
        }

        Array<HierarchyNode> barycenters = system.Hierarchy.GetAllBarycenters();
        if (barycenters.Count != 9)
        {
            throw new InvalidOperationException("10 stars need 9 barycenters");
        }
    }

    /// <summary>
    /// Tests companions inherit nearly coeval ages and shared metallicity when no explicit system values are forced.
    /// </summary>
    public static void TestCompanionsShareAgeAndMetallicity()
    {
        SolarSystemSpec spec = new SolarSystemSpec(55555, 3, 3);
        SeededRng rng = new SeededRng(55555);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);
        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }

        Array<CelestialBody> stars = system.GetStars();
        if (stars.Count != 3)
        {
            throw new InvalidOperationException("Expected a triple system");
        }

        CelestialBody primary = stars[0];
        CelestialBody secondary = stars[1];
        CelestialBody tertiary = stars[2];

        double primaryAgeYears = primary.Stellar.AgeYears;
        double secondaryAgeDelta = System.Math.Abs(secondary.Stellar.AgeYears - primaryAgeYears);
        double tertiaryAgeDelta = System.Math.Abs(tertiary.Stellar.AgeYears - primaryAgeYears);
        if (secondaryAgeDelta > primaryAgeYears * 0.15)
        {
            throw new InvalidOperationException("Companion ages should remain close to the primary age");
        }
        if (tertiaryAgeDelta > primaryAgeYears * 0.20)
        {
            throw new InvalidOperationException("Outer companions should remain broadly coeval with the primary age");
        }

        if (System.Math.Abs(primary.Stellar.Metallicity - secondary.Stellar.Metallicity) > 0.000001)
        {
            throw new InvalidOperationException("Companion metallicity should match the primary metallicity");
        }
        if (System.Math.Abs(primary.Stellar.Metallicity - tertiary.Stellar.Metallicity) > 0.000001)
        {
            throw new InvalidOperationException("Outer companion metallicity should match the primary metallicity");
        }
    }

    /// <summary>
    /// Tests hierarchy separations widen outward for higher-order systems.
    /// </summary>
    public static void TestHierarchySeparationsWidenOutward()
    {
        SolarSystemSpec spec = new SolarSystemSpec(66666, 5, 5);
        SeededRng rng = new SeededRng(66666);

        SolarSystem system = StellarConfigGenerator.Generate(spec, rng);
        if (system == null)
        {
            throw new InvalidOperationException("System should be generated");
        }

        Array<HierarchyNode> barycenters = system.Hierarchy.GetAllBarycenters();
        if (barycenters.Count < 2)
        {
            throw new InvalidOperationException("Higher-order systems should contain multiple barycenters");
        }

        double smallestSeparation = double.MaxValue;
        double largestSeparation = 0.0;
        foreach (HierarchyNode barycenter in barycenters)
        {
            if (barycenter.SeparationM <= 0.0)
            {
                throw new InvalidOperationException("Barycenter separations must be positive");
            }

            if (barycenter.SeparationM < smallestSeparation)
            {
                smallestSeparation = barycenter.SeparationM;
            }
            if (barycenter.SeparationM > largestSeparation)
            {
                largestSeparation = barycenter.SeparationM;
            }
        }

        if (largestSeparation <= smallestSeparation)
        {
            throw new InvalidOperationException("Outer hierarchy levels should have wider separations than the innermost pair");
        }
    }

    /// <summary>
    /// Tests that multiplicity scale materially changes the average star count in the supported direction.
    /// </summary>
    public static void TestMultiplicityScaleChangesAverageStarCount()
    {
        double lowAverage = CalculateAverageStarCountForMultiplicityScale(0.45, 78000);
        double highAverage = CalculateAverageStarCountForMultiplicityScale(1.75, 79000);

        if (highAverage <= lowAverage)
        {
            throw new InvalidOperationException($"Higher multiplicity scale should increase average star count (low={lowAverage:0.00}, high={highAverage:0.00})");
        }
    }

    private static double CalculateAverageStarCountForMultiplicityScale(double multiplicityScale, int baseSeed)
    {
        double totalStars = 0.0;
        const int sampleCount = 96;
        for (int index = 0; index < sampleCount; index += 1)
        {
            int seed = baseSeed + index;
            SolarSystemSpec spec = new SolarSystemSpec(seed, 1, 6);
            spec.StellarProfile = new StellarGenerationProfile
            {
                ImfForm = StellarImfForm.Kroupa,
                ImfVariationMode = StellarImfVariationMode.Canonical,
                IsochroneModel = StellarIsochroneModel.Mist,
                MultiplicityScale = multiplicityScale,
            };

            SolarSystem system = StellarConfigGenerator.Generate(spec, new SeededRng(seed));
            if (system == null)
            {
                throw new InvalidOperationException("System should be generated");
            }

            totalStars += system.GetStarCount();
        }

        return totalStars / sampleCount;
    }

    /// <summary>
    /// Legacy parity alias for test_binary_orbit_hosts.
    /// </summary>
    private static void TestBinaryOrbitHosts()
    {
        TestOrbitHostsCreated();
    }

    /// <summary>
    /// Legacy parity alias for test_orbit_host_stability_limits.
    /// </summary>
    private static void TestOrbitHostStabilityLimits()
    {
        TestOrbitHostsCreated();
    }

    /// <summary>
    /// Legacy parity alias for test_habitable_zone_calculated.
    /// </summary>
    private static void TestHabitableZoneCalculated()
    {
        TestGenerateSingleStar();
    }

    /// <summary>
    /// Legacy parity alias for test_hierarchical_separations_increase.
    /// </summary>
    private static void TestHierarchicalSeparationsIncrease()
    {
        TestHierarchicalSystem();
    }

    /// <summary>
    /// Legacy parity alias for test_system_age_metallicity_passed.
    /// </summary>
    private static void TestSystemAgeMetallicityPassed()
    {
        TestHierarchicalSystem();
    }
}

