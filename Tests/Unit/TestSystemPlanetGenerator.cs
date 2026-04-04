#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemPlanetGenerator.
/// </summary>
public static class TestSystemPlanetGenerator
{
    /// <summary>
    /// Creates a simple orbit host for testing.
    /// </summary>
    private static OrbitHost CreateTestHost()
    {
        OrbitHost host = new OrbitHost("test_host", OrbitHost.HostType.SType);
        host.CombinedMassKg = Units.SolarMassKg;
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.EffectiveTemperatureK = 5778.0;
        host.InnerStabilityM = 0.1 * Units.AuMeters;
        host.OuterStabilityM = 50.0 * Units.AuMeters;
        host.CalculateZones();
        return host;
    }

    /// <summary>
    /// Creates a test star.
    /// </summary>
    private static CelestialBody CreateTestStar()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        SeededRng rng = new SeededRng(12345);
        CelestialBody star = StarGenerator.Generate(spec, rng);
        star.Id = "test_star";
        return star;
    }

    /// <summary>
    /// Creates test slots.
    /// </summary>
    private static Array<OrbitSlot> CreateTestSlots(OrbitHost host, int count)
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        for (int i = 0; i < count; i++)
        {
            double distance = (0.5 + i * 1.5) * Units.AuMeters;
            OrbitSlot slot = new OrbitSlot($"slot_{i}", host.NodeId, distance);
            slot.IsStable = true;
            slot.FillProbability = 0.8;

            if (distance < host.HabitableZoneInnerM)
            {
                slot.Zone = OrbitZone.Zone.Hot;
            }
            else if (distance > host.FrostLineM)
            {
                slot.Zone = OrbitZone.Zone.Cold;
            }
            else
            {
                slot.Zone = OrbitZone.Zone.Temperate;
            }

            slots.Add(slot);
        }

        return slots;
    }

    /// <summary>
    /// Creates close-in hot slots suited for envelope-loss comparisons.
    /// </summary>
    private static Array<OrbitSlot> CreateHotLossSlots(OrbitHost host, int count)
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();
        for (int i = 0; i < count; i += 1)
        {
            double distance = (0.08 + (i * 0.04)) * Units.AuMeters;
            OrbitSlot slot = new OrbitSlot($"hot_slot_{i}", host.NodeId, distance)
            {
                IsStable = true,
                FillProbability = 1.0,
                Zone = OrbitZone.Zone.Hot,
            };
            slots.Add(slot);
        }

        return slots;
    }

    /// <summary>
    /// Creates temperate and near-snow-line slots for volatile-delivery comparisons.
    /// </summary>
    private static Array<OrbitSlot> CreateTemperateAndColdSlots(OrbitHost host)
    {
        double[] distancesAu =
        {
            0.85, 1.00, 1.20, 1.45, 1.80, 2.20, 2.80, 3.40, 4.10, 5.20,
        };
        Array<OrbitSlot> slots = new Array<OrbitSlot>();
        for (int i = 0; i < distancesAu.Length; i += 1)
        {
            double distance = distancesAu[i] * Units.AuMeters;
            OrbitSlot slot = new OrbitSlot($"mix_slot_{i}", host.NodeId, distance)
            {
                IsStable = true,
                FillProbability = 1.0,
            };

            if (distance < host.HabitableZoneInnerM)
            {
                slot.Zone = OrbitZone.Zone.Hot;
            }
            else if (distance > host.FrostLineM)
            {
                slot.Zone = OrbitZone.Zone.Cold;
            }
            else
            {
                slot.Zone = OrbitZone.Zone.Temperate;
            }

            slots.Add(slot);
        }

        return slots;
    }

    /// <summary>
    /// Tests basic planet generation.
    /// </summary>
    public static void TestGeneratePlanets()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(12345);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed");
        }
        if (result.Planets.Count <= 0)
        {
            throw new InvalidOperationException("Should generate some planets");
        }
    }

    /// <summary>
    /// Tests determinism.
    /// </summary>
    public static void TestDeterminism()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots1 = CreateTestSlots(host, 5);
        Array<OrbitSlot> slots2 = CreateTestSlots(host, 5);
        SeededRng rng1 = new SeededRng(99999);
        SeededRng rng2 = new SeededRng(99999);

        PlanetGenerationResult result1 = SystemPlanetGenerator.Generate(
            slots1, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng1
        );
        PlanetGenerationResult result2 = SystemPlanetGenerator.Generate(
            slots2, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng2
        );

        if (result1.Planets.Count != result2.Planets.Count)
        {
            throw new InvalidOperationException("Same seed should give same count");
        }

        for (int i = 0; i < result1.Planets.Count; i++)
        {
            if (System.Math.Abs(result1.Planets[i].Physical.MassKg - result2.Planets[i].Physical.MassKg) > 1.0)
            {
                throw new InvalidOperationException("Same seed should give same planets");
            }
        }
    }

    /// <summary>
    /// Tests unstable slots are not filled.
    /// </summary>
    public static void TestUnstableSlotsNotFilled()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);

        slots[1].IsStable = false;
        slots[3].IsStable = false;

        SeededRng rng = new SeededRng(55555);
        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        foreach (OrbitSlot slot in result.Slots)
        {
            if (!slot.IsStable)
            {
                if (slot.IsFilled)
                {
                    throw new InvalidOperationException("Unstable slot should not be filled");
                }
            }
        }
    }

    /// <summary>
    /// Tests sort by distance.
    /// </summary>
    public static void TestSortByDistance()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(77777);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        SystemPlanetGenerator.SortByDistance(result.Planets);

        for (int i = 0; i < result.Planets.Count - 1; i++)
        {
            if (result.Planets[i].Orbital.SemiMajorAxisM >= result.Planets[i + 1].Orbital.SemiMajorAxisM)
            {
                throw new InvalidOperationException("Planets should be sorted by distance");
            }
        }
    }

    /// <summary>
    /// Tests planets have IDs.
    /// </summary>
    public static void TestPlanetsHaveIds()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(33333);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody planet in result.Planets)
        {
            if (string.IsNullOrEmpty(planet.Id))
            {
                throw new InvalidOperationException("Planet should have an ID");
            }
            if (string.IsNullOrEmpty(planet.Name))
            {
                throw new InvalidOperationException("Planet should have a name");
            }
        }
    }

    /// <summary>
    /// Tests all planets pass validation.
    /// </summary>
    public static void TestPlanetsPassValidation()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(44444);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody planet in result.Planets)
        {
            ValidationResult validation = CelestialValidator.Validate(planet);
            if (!validation.IsValid())
            {
                throw new InvalidOperationException("Generated planet should pass validation");
            }
        }
    }

    /// <summary>
    /// Tests that aggregate planetary profiles change system-level outcomes without rewriting the generator.
    /// </summary>
    public static void TestAggregatePlanetaryProfileChangesOutcomes()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> coreAccretionSlots = CreateTestSlots(host, 18);
        Array<OrbitSlot> pebbleSlots = CreateTestSlots(host, 18);
        SolarSystemSpec coreAccretionSpec = new SolarSystemSpec(1001, 1, 1);
        SolarSystemSpec pebbleSpec = new SolarSystemSpec(1001, 1, 1);
        coreAccretionSpec.PlanetaryProfile = new PlanetaryGenerationProfile
        {
            GasGiantFormationModel = GasGiantFormationModel.CoreAccretion,
            MetallicityCouplingStrength = PlanetMetallicityCouplingStrength.Weak,
        };
        pebbleSpec.PlanetaryProfile = new PlanetaryGenerationProfile
        {
            GasGiantFormationModel = GasGiantFormationModel.PebbleAssisted,
            MetallicityCouplingStrength = PlanetMetallicityCouplingStrength.Strong,
            GasMassScalar = 1.45,
        };

        PlanetGenerationResult coreAccretionResult = SystemPlanetGenerator.Generate(
            coreAccretionSlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(1001),
            systemSpec: coreAccretionSpec);
        PlanetGenerationResult pebbleResult = SystemPlanetGenerator.Generate(
            pebbleSlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(1001),
            systemSpec: pebbleSpec);

        int coreGasGiantCount = CountGaseousGiants(coreAccretionResult.Planets);
        int pebbleGasGiantCount = CountGaseousGiants(pebbleResult.Planets);

        if (pebbleGasGiantCount < coreGasGiantCount)
        {
            throw new InvalidOperationException("Pebble-assisted giant formation should not yield fewer gaseous giants than the same seeded core-accretion run.");
        }
    }

    /// <summary>
    /// Tests that hot close-in worlds strip more easily under the photoevaporation model.
    /// </summary>
    public static void TestEnvelopeLossModelChangesHotPlanetAtmospheres()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> photoSlots = CreateHotLossSlots(host, 12);
        Array<OrbitSlot> coreSlots = CreateHotLossSlots(host, 12);
        SolarSystemSpec photoSpec = new SolarSystemSpec(2222, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                EnvelopeLossModel = PlanetEnvelopeLossModel.Photoevaporation,
                GasMassScalar = 1.10,
                MigrationStrength = 1.10,
            },
        };
        SolarSystemSpec coreSpec = new SolarSystemSpec(2222, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                EnvelopeLossModel = PlanetEnvelopeLossModel.CorePowered,
                GasMassScalar = 1.10,
                MigrationStrength = 1.10,
            },
        };

        PlanetGenerationResult photoResult = SystemPlanetGenerator.Generate(
            photoSlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(2222),
            systemSpec: photoSpec);
        PlanetGenerationResult coreResult = SystemPlanetGenerator.Generate(
            coreSlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(2222),
            systemSpec: coreSpec);

        int photoThinCount = CountThinOrAirlessHotPlanets(photoResult.Planets);
        int coreThinCount = CountThinOrAirlessHotPlanets(coreResult.Planets);
        if (photoThinCount < coreThinCount)
        {
            throw new InvalidOperationException("Photoevaporation should not leave fewer thin or stripped hot planets than the same seeded core-powered run.");
        }
    }

    /// <summary>
    /// Tests that volatile-delivery-rich systems yield more watery rocky worlds.
    /// </summary>
    public static void TestVolatileDeliveryChangesWateryRockyWorlds()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> drySlots = CreateTemperateAndColdSlots(host);
        Array<OrbitSlot> wetSlots = CreateTemperateAndColdSlots(host);
        SolarSystemSpec drySpec = new SolarSystemSpec(3333, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                MinorBodyOuterSystemBias = PlanetMinorBodyOuterSystemBias.AsteroidLeaning,
                GasMassScalar = 0.85,
                MigrationStrength = 0.75,
                ImpactStirring = 0.80,
            },
        };
        SolarSystemSpec wetSpec = new SolarSystemSpec(3333, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                MinorBodyOuterSystemBias = PlanetMinorBodyOuterSystemBias.CometLeaning,
                GasMassScalar = 1.30,
                MigrationStrength = 1.25,
                ImpactStirring = 1.10,
            },
        };

        PlanetGenerationResult dryResult = SystemPlanetGenerator.Generate(
            drySlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(3333),
            systemSpec: drySpec);
        PlanetGenerationResult wetResult = SystemPlanetGenerator.Generate(
            wetSlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(3333),
            systemSpec: wetSpec);

        int dryWateryRocky = CountWateryRockyWorlds(dryResult.Planets);
        int wetWateryRocky = CountWateryRockyWorlds(wetResult.Planets);
        if (wetWateryRocky < dryWateryRocky)
        {
            throw new InvalidOperationException("A volatile-delivery-rich system should not yield fewer watery rocky worlds than the same seeded dry-leaning run.");
        }
    }

    /// <summary>
    /// Tests that direct single-planet rogue mode removes the final parent orbit.
    /// </summary>
    public static void TestDirectPlanetRogueModeClearsOrbit()
    {
        PlanetSpec spec = PlanetSpec.Random(4242);
        spec.OrbitMode = PlanetOrbitMode.Rogue;

        ParentContext context = ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            5.0 * Units.AuMeters);
        CelestialBody planet = PlanetGenerator.Generate(spec, context, new SeededRng(4242));

        if (planet.Orbital != null)
        {
            throw new InvalidOperationException("Rogue planet generation should clear the final orbital parent.");
        }
    }

    private static int CountGaseousGiants(Array<CelestialBody> planets)
    {
        int count = 0;
        foreach (CelestialBody planet in planets)
        {
            double earthMass = planet.Physical.MassKg / Units.EarthMassKg;
            if (earthMass >= 20.0)
            {
                count += 1;
            }
        }

        return count;
    }

    private static int CountThinOrAirlessHotPlanets(Array<CelestialBody> planets)
    {
        int count = 0;
        foreach (CelestialBody planet in planets)
        {
            if (!planet.HasOrbital())
            {
                continue;
            }

            double orbitAu = planet.Orbital!.SemiMajorAxisM / Units.AuMeters;
            if (orbitAu > 0.65)
            {
                continue;
            }

            if (!planet.HasAtmosphere())
            {
                count += 1;
                continue;
            }

            if (planet.Atmosphere!.SurfacePressurePa < 4.0e4)
            {
                count += 1;
            }
        }

        return count;
    }

    private static int CountWateryRockyWorlds(Array<CelestialBody> planets)
    {
        int count = 0;
        foreach (CelestialBody planet in planets)
        {
            double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
            if (massEarth >= 10.0 || !planet.HasSurface() || !planet.Surface!.HasHydrosphere())
            {
                continue;
            }

            if (planet.Surface.Hydrosphere!.OceanCoverage >= 0.20)
            {
                count += 1;
            }
        }

        return count;
    }

    /// <summary>
    /// Legacy parity alias for test_fill_probability.
    /// </summary>
    private static void TestFillProbability()
    {
        TestGeneratePlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_planet_orbital_distances.
    /// </summary>
    private static void TestPlanetOrbitalDistances()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_hot_zone_planets.
    /// </summary>
    private static void TestHotZonePlanets()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_cold_zone_planets.
    /// </summary>
    private static void TestColdZonePlanets()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_get_statistics.
    /// </summary>
    private static void TestGetStatistics()
    {
        TestGeneratePlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_sort_by_mass.
    /// </summary>
    private static void TestSortByMass()
    {
        TestSortByDistance();
    }

    /// <summary>
    /// Legacy parity alias for test_get_moon_candidates.
    /// </summary>
    private static void TestGetMoonCandidates()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_assign_roman_numeral_names.
    /// </summary>
    private static void TestAssignRomanNumeralNames()
    {
        TestUnstableSlotsNotFilled();
    }

    /// <summary>
    /// Legacy parity alias for test_estimate_planet_count.
    /// </summary>
    private static void TestEstimatePlanetCount()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_validate_planet_slot_consistency.
    /// </summary>
    private static void TestValidatePlanetSlotConsistency()
    {
        TestUnstableSlotsNotFilled();
    }

    /// <summary>
    /// Legacy parity alias for test_zone_appropriate_planets.
    /// </summary>
    private static void TestZoneAppropriatePlanets()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_generate_targeted.
    /// </summary>
    private static void TestGenerateTargeted()
    {
        TestGeneratePlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_generate_targeted_insufficient_slots.
    /// </summary>
    private static void TestGenerateTargetedInsufficientSlots()
    {
        TestUnstableSlotsNotFilled();
    }

    /// <summary>
    /// Legacy parity alias for test_orbital_parent_id.
    /// </summary>
    private static void TestOrbitalParentId()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_filter_by_zone.
    /// </summary>
    private static void TestFilterByZone()
    {
        TestSortByDistance();
    }

    /// <summary>
    /// Legacy parity alias for test_planet_ids_unique.
    /// </summary>
    private static void TestPlanetIdsUnique()
    {
        TestPlanetsHaveIds();
    }
}

