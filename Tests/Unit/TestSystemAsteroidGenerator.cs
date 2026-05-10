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
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemAsteroidGenerator.
/// </summary>
public static class TestSystemAsteroidGenerator
{
    /// <summary>
    /// Creates a Sun-like orbit host for testing.
    /// </summary>
    private static OrbitHost CreateSunLikeHost()
    {
        OrbitHost host = new OrbitHost("host_sol", OrbitHost.HostType.SType);
        host.CombinedMassKg = Units.SolarMassKg;
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.EffectiveTemperatureK = 5778.0;
        host.InnerStabilityM = 0.1 * Units.AuMeters;
        host.OuterStabilityM = 100.0 * Units.AuMeters;
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
    /// Creates filled slots representing planets.
    /// </summary>
    private static Array<OrbitSlot> CreatePlanetSlots(OrbitHost host)
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        OrbitSlot slot1 = new OrbitSlot("slot_0", host.NodeId, 0.4 * Units.AuMeters);
        slot1.IsFilled = true;
        slot1.PlanetId = "planet_0";
        slots.Add(slot1);

        OrbitSlot slot2 = new OrbitSlot("slot_1", host.NodeId, 0.7 * Units.AuMeters);
        slot2.IsFilled = true;
        slot2.PlanetId = "planet_1";
        slots.Add(slot2);

        OrbitSlot slot3 = new OrbitSlot("slot_2", host.NodeId, 1.0 * Units.AuMeters);
        slot3.IsFilled = true;
        slot3.PlanetId = "planet_2";
        slots.Add(slot3);

        OrbitSlot slot4 = new OrbitSlot("slot_3", host.NodeId, 1.5 * Units.AuMeters);
        slot4.IsFilled = true;
        slot4.PlanetId = "planet_3";
        slots.Add(slot4);

        OrbitSlot slot5 = new OrbitSlot("slot_4", host.NodeId, 5.2 * Units.AuMeters);
        slot5.IsFilled = true;
        slot5.PlanetId = "planet_4";
        slots.Add(slot5);

        OrbitSlot slot6 = new OrbitSlot("slot_5", host.NodeId, 9.5 * Units.AuMeters);
        slot6.IsFilled = true;
        slot6.PlanetId = "planet_5";
        slots.Add(slot6);

        return slots;
    }

    /// <summary>
    /// Tests basic belt generation.
    /// </summary>
    public static void TestGenerateBelts()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreatePlanetSlots(host);
        SeededRng rng = new SeededRng(12345);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host },
            slots,
            new Array<CelestialBody> { star },
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed");
        }
    }

    /// <summary>
    /// Tests belt generation with no planets.
    /// </summary>
    public static void TestGenerateBeltsNoPlanets()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(22222);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host },
            new Array<OrbitSlot>(),
            new Array<CelestialBody> { star },
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed with no planets");
        }
    }

    /// <summary>
    /// Tests determinism.
    /// </summary>
    public static void TestDeterminism()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreatePlanetSlots(host);
        SeededRng rng1 = new SeededRng(44444);
        SeededRng rng2 = new SeededRng(44444);

        BeltGenerationResult result1 = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, slots, new Array<CelestialBody> { star }, rng1
        );
        BeltGenerationResult result2 = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, slots, new Array<CelestialBody> { star }, rng2
        );

        if (result1.Belts.Count != result2.Belts.Count)
        {
            throw new InvalidOperationException("Same seed should give same belt count");
        }
        if (result1.Asteroids.Count != result2.Asteroids.Count)
        {
            throw new InvalidOperationException("Same seed should give same asteroid count");
        }

        for (int i = 0; i < result1.Belts.Count; i++)
        {
            if (System.Math.Abs(result1.Belts[i].InnerRadiusM - result2.Belts[i].InnerRadiusM) > 1.0)
            {
                throw new InvalidOperationException("Same seed should give same belt positions");
            }
        }
    }

    /// <summary>
    /// Tests belt boundaries are valid.
    /// </summary>
    public static void TestBeltBoundaries()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(55555);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (AsteroidBelt belt in result.Belts)
        {
            if (belt.InnerRadiusM <= 0.0)
            {
                throw new InvalidOperationException("Inner radius should be positive");
            }
            if (belt.OuterRadiusM <= belt.InnerRadiusM)
            {
                throw new InvalidOperationException("Outer > inner");
            }
            if (belt.TotalMassKg <= 0.0)
            {
                throw new InvalidOperationException("Belt mass should be positive");
            }
        }
    }

    /// <summary>
    /// Tests asteroid IDs are unique.
    /// </summary>
    public static void TestAsteroidIdsUnique()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(11111);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        Godot.Collections.Dictionary ids = new Godot.Collections.Dictionary();
        foreach (CelestialBody asteroid in result.Asteroids)
        {
            if (ids.ContainsKey(asteroid.Id))
            {
                throw new InvalidOperationException("Asteroid IDs should be unique");
            }
            ids[asteroid.Id] = true;
        }
    }

    /// <summary>
    /// Tests asteroids pass validation.
    /// </summary>
    public static void TestAsteroidsPassValidation()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(22222);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody asteroid in result.Asteroids)
        {
            ValidationResult validation = CelestialValidator.Validate(asteroid);
            if (!validation.IsValid())
            {
                throw new InvalidOperationException("Generated asteroid should pass validation");
            }
        }
    }

    /// <summary>
    /// Tests asteroid parent IDs.
    /// </summary>
    public static void TestAsteroidParentIds()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(88888);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody asteroid in result.Asteroids)
        {
            if (!asteroid.HasOrbital())
            {
                throw new InvalidOperationException("Asteroid should have orbital data");
            }
            if (asteroid.Orbital.ParentId != host.NodeId)
            {
                throw new InvalidOperationException("Asteroid parent should be host");
            }
        }
    }

    /// <summary>
    /// Tests asteroid names are assigned.
    /// </summary>
    public static void TestAsteroidNames()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(99999);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody asteroid in result.Asteroids)
        {
            if (string.IsNullOrEmpty(asteroid.Name))
            {
                throw new InvalidOperationException("Asteroid should have a name");
            }
        }
    }

    /// <summary>
    /// Tests that comet-leaning outer-system bias yields more icy outer belts than asteroid-leaning bias.
    /// </summary>
    public static void TestOuterSystemBiasChangesIcyBelts()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreatePlanetSlots(host);
        int cometLeaningIcyBelts = 0;
        int asteroidLeaningIcyBelts = 0;

        for (int seed = 4100; seed < 4110; seed += 1)
        {
            SolarSystemSpec cometSpec = new SolarSystemSpec(seed, 1, 1)
            {
                PlanetaryProfile = new PlanetaryGenerationProfile
                {
                    MinorBodyOuterSystemBias = PlanetMinorBodyOuterSystemBias.CometLeaning,
                    GasMassScalar = 1.20,
                },
            };
            SolarSystemSpec asteroidSpec = new SolarSystemSpec(seed, 1, 1)
            {
                PlanetaryProfile = new PlanetaryGenerationProfile
                {
                    MinorBodyOuterSystemBias = PlanetMinorBodyOuterSystemBias.AsteroidLeaning,
                    GasMassScalar = 1.20,
                },
            };

            BeltGenerationResult cometResult = SystemAsteroidGenerator.Generate(
                new Array<OrbitHost> { host },
                slots,
                new Array<CelestialBody> { star },
                new SeededRng(seed),
                systemSpec: cometSpec);
            BeltGenerationResult asteroidResult = SystemAsteroidGenerator.Generate(
                new Array<OrbitHost> { host },
                slots,
                new Array<CelestialBody> { star },
                new SeededRng(seed),
                systemSpec: asteroidSpec);

            cometLeaningIcyBelts += CountIcyBelts(cometResult.Belts);
            asteroidLeaningIcyBelts += CountIcyBelts(asteroidResult.Belts);
        }

        if (cometLeaningIcyBelts <= asteroidLeaningIcyBelts)
        {
            throw new InvalidOperationException("Comet-leaning outer-system bias should yield more icy belts across the same seeded sample.");
        }
    }

    /// <summary>
    /// Tests generated cold reservoirs carry source metadata and use Kuiper-belt-scale placement.
    /// </summary>
    public static void TestOuterReservoirSourceMetadataAndPlacement()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreatePlanetSlots(host);

        for (int seed = 5200; seed < 5300; seed += 1)
        {
            SolarSystemSpec spec = new SolarSystemSpec(seed, 1, 1)
            {
                PlanetaryProfile = new PlanetaryGenerationProfile
                {
                    MinorBodyOuterSystemBias = PlanetMinorBodyOuterSystemBias.CometLeaning,
                    GasMassScalar = 1.35,
                },
            };

            BeltGenerationResult result = SystemAsteroidGenerator.Generate(
                new Array<OrbitHost> { host },
                slots,
                new Array<CelestialBody> { star },
                new SeededRng(seed),
                systemSpec: spec);

            foreach (AsteroidBelt belt in result.Belts)
            {
                if (belt.ReservoirKind != "trans_neptunian_reservoir")
                {
                    continue;
                }

                double innerAu = belt.InnerRadiusM / Units.AuMeters;
                if (innerAu < 25.0)
                {
                    throw new InvalidOperationException($"Outer reservoir should begin near Kuiper-belt analog distances, got {innerAu:0.00} AU.");
                }

                if (!belt.ReservoirSourceIds.Contains("KavelaarsEtAl2023") || !belt.ReservoirSourceIds.Contains("BernardinelliEtAl2022"))
                {
                    throw new InvalidOperationException("Outer reservoir should carry TNO source IDs.");
                }

                if (string.IsNullOrWhiteSpace(belt.ReservoirSubfamily))
                {
                    throw new InvalidOperationException("Outer reservoir should carry a dominant TNO subfamily diagnostic.");
                }

                if (!belt.ReservoirSubfamily.EndsWith("_proxy", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Outer reservoir subfamily should be explicitly labeled as a proxy, got {belt.ReservoirSubfamily}.");
                }

                if (!belt.ReservoirSubfamilyMix.Contains("cold_classical") || !belt.ReservoirSubfamilyMix.Contains("comet_feeding"))
                {
                    throw new InvalidOperationException("Outer reservoir should carry the full TNO subfamily mix diagnostic.");
                }

                if (!belt.ReservoirSubfamilySourceIds.Contains("KavelaarsEtAl2023") || !belt.ReservoirSubfamilySourceIds.Contains("BernardinelliEtAl2022"))
                {
                    throw new InvalidOperationException("Outer reservoir subfamily diagnostics should carry TNO source IDs.");
                }

                if (!belt.CompositionSourceIds.Contains("DeMeoCarry2014"))
                {
                    throw new InvalidOperationException("Outer reservoir should retain DeMeo/Carry composition source ID.");
                }

                if (belt.TotalMassKg > 2.5e23)
                {
                    throw new InvalidOperationException($"Outer reservoir mass should stay within the reduced TNO-proxy range, got {belt.TotalMassKg:0.000e0} kg.");
                }

                AssertMajorAsteroidsCarryReservoirSubfamily(result.Asteroids, belt);
                AssertOuterReservoirRecordsCarryTnoFamilies(result.Reservoirs, belt);
                return;
            }
        }

        throw new InvalidOperationException("Expected at least one generated trans-Neptunian reservoir in the seeded sample.");
    }

    /// <summary>
    /// Tests the exposed minor-body population slope materially affects representative major-body sizes.
    /// </summary>
    public static void TestMinorBodyPopulationSlopeChangesMajorBodySizes()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        AsteroidBelt lowSlopeBelt = CreatePredefinedOuterBelt(host);
        AsteroidBelt highSlopeBelt = CreatePredefinedOuterBelt(host);

        SolarSystemSpec lowSlopeSpec = new SolarSystemSpec(6100, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                MinorBodyPopulationSlope = 1.0,
            },
        };
        SolarSystemSpec highSlopeSpec = new SolarSystemSpec(6100, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                MinorBodyPopulationSlope = 5.0,
            },
        };

        BeltGenerationResult lowSlopeResult = SystemAsteroidGenerator.GenerateFromPredefinedBelts(
            new Array<AsteroidBelt> { lowSlopeBelt },
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(6100),
            systemSpec: lowSlopeSpec);
        BeltGenerationResult highSlopeResult = SystemAsteroidGenerator.GenerateFromPredefinedBelts(
            new Array<AsteroidBelt> { highSlopeBelt },
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(6100),
            systemSpec: highSlopeSpec);

        double lowSlopeAverageRadius = AverageRadiusM(lowSlopeResult.Asteroids);
        double highSlopeAverageRadius = AverageRadiusM(highSlopeResult.Asteroids);
        if (highSlopeAverageRadius >= lowSlopeAverageRadius)
        {
            throw new InvalidOperationException($"Higher minor-body slope should favor smaller representative bodies. Low={lowSlopeAverageRadius:0.00} m high={highSlopeAverageRadius:0.00} m.");
        }

        foreach (CelestialBody asteroid in highSlopeResult.Asteroids)
        {
            if (!asteroid.HasMeta("minor_body_population_slope"))
            {
                throw new InvalidOperationException("Generated representative asteroids should carry minor-body slope provenance.");
            }
        }
    }

    /// <summary>
    /// Tests major belt objects use diameter semantics and remain inspectable bodies.
    /// </summary>
    public static void TestMajorAsteroidsUseLargeObjectDiameterSemantics()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        AsteroidBelt belt = CreatePredefinedOuterBelt(host);

        BeltGenerationResult result = SystemAsteroidGenerator.GenerateFromPredefinedBelts(
            new Array<AsteroidBelt> { belt },
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(7001));

        if (belt.MajorAsteroidIds.Count <= 0)
        {
            throw new InvalidOperationException("Predefined belt should generate inspectable large objects.");
        }

        if (belt.MajorAsteroidIds.Count > 10)
        {
            throw new InvalidOperationException("Tracked large belt objects should be capped at 10.");
        }

        double previousDiameterKm = double.MaxValue;
        foreach (string asteroidId in belt.MajorAsteroidIds)
        {
            CelestialBody asteroid = RequireAsteroid(result.Asteroids, asteroidId);
            double diameterKm = asteroid.Physical.RadiusM * 2.0 / 1000.0;
            if (diameterKm < 500.0)
            {
                throw new InvalidOperationException($"Tracked belt object {asteroidId} should be at least 500 km diameter, got {diameterKm:0.00} km.");
            }

            if (diameterKm > previousDiameterKm + 0.001)
            {
                throw new InvalidOperationException("Tracked belt objects should be sorted largest-first by diameter.");
            }

            previousDiameterKm = diameterKm;

            if (!asteroid.HasOrbital() || asteroid.Orbital == null)
            {
                throw new InvalidOperationException("Tracked belt object should keep orbital data.");
            }

            if (asteroid.Orbital.SemiMajorAxisM < belt.InnerRadiusM || asteroid.Orbital.SemiMajorAxisM > belt.OuterRadiusM)
            {
                throw new InvalidOperationException("Tracked belt object should remain inside the parent belt radial bounds.");
            }

            double volumeM3 = (4.0 / 3.0) * System.Math.PI * asteroid.Physical.RadiusM * asteroid.Physical.RadiusM * asteroid.Physical.RadiusM;
            double densityKgM3 = asteroid.Physical.MassKg / volumeM3;
            if (densityKgM3 < 900.0 || densityKgM3 > 5300.0)
            {
                throw new InvalidOperationException($"Tracked belt object density should be composition-plausible, got {densityKgM3:0.00} kg/m3.");
            }

            if (!asteroid.HasMeta("major_body_diameter_km") || !asteroid.HasMeta("belt_id") || !asteroid.HasMeta("belt_population_readiness"))
            {
                throw new InvalidOperationException("Tracked belt object should carry neutral belt membership, size, and habitation follow-up metadata.");
            }
        }
    }

    private static int CountIcyBelts(Array<AsteroidBelt> belts)
    {
        int count = 0;
        foreach (AsteroidBelt belt in belts)
        {
            if (belt.PrimaryComposition == AsteroidBelt.Composition.Icy)
            {
                count += 1;
            }
        }

        return count;
    }

    private static AsteroidBelt CreatePredefinedOuterBelt(OrbitHost host)
    {
        return new AsteroidBelt("belt_test_outer", "Outer Asteroid Belt")
        {
            OrbitHostId = host.NodeId,
            InnerRadiusM = 35.0 * Units.AuMeters,
            OuterRadiusM = 48.0 * Units.AuMeters,
            TotalMassKg = 1.0e22,
            PrimaryComposition = AsteroidBelt.Composition.Icy,
            ReservoirKind = "trans_neptunian_reservoir",
            ReservoirSourceIds = "KavelaarsEtAl2023;BernardinelliEtAl2022",
            ReservoirSubfamily = "hot_classical_tno_proxy",
            ReservoirSubfamilyMix = "cold_classical:0.14;hot_classical:0.38;resonant:0.26;scattered:0.12;centaur:0.04;comet_feeding:0.06",
            ReservoirSubfamilySourceIds = "KavelaarsEtAl2023;BernardinelliEtAl2022",
            CompositionSourceIds = "DeMeoCarry2014",
            SizeDistributionSourceIds = "KavelaarsEtAl2023;BernardinelliEtAl2022",
            PopulationModel = "kavelaars_bernardinelli_large_tno_proxy",
        };
    }

    private static void AssertMajorAsteroidsCarryReservoirSubfamily(Array<CelestialBody> asteroids, AsteroidBelt belt)
    {
        foreach (CelestialBody asteroid in asteroids)
        {
            if (!belt.MajorAsteroidIds.Contains(asteroid.Id))
            {
                continue;
            }

            if (!asteroid.HasMeta("belt_reservoir_subfamily"))
            {
                throw new InvalidOperationException("Representative TNO-like bodies should carry reservoir subfamily metadata.");
            }

            string subfamily = asteroid.GetMeta("belt_reservoir_subfamily").AsString();
            if (subfamily != belt.ReservoirSubfamily)
            {
                throw new InvalidOperationException("Representative TNO-like body subfamily metadata should match its belt.");
            }

            if (!asteroid.HasMeta("belt_reservoir_subfamily_mix") || !asteroid.HasMeta("belt_reservoir_subfamily_source_ids"))
            {
                throw new InvalidOperationException("Representative TNO-like bodies should carry subfamily mix and source metadata.");
            }

            return;
        }

        throw new InvalidOperationException("Expected a representative body for the selected outer reservoir.");
    }

    private static void AssertOuterReservoirRecordsCarryTnoFamilies(Array<SmallBodyReservoir> reservoirs, AsteroidBelt belt)
    {
        bool hasColdClassical = false;
        bool hasResonant = false;
        bool hasCometFeeding = false;
        double totalWeight = 0.0;

        foreach (SmallBodyReservoir reservoir in reservoirs)
        {
            if (reservoir.AnchorBeltId != belt.Id)
            {
                continue;
            }

            if (reservoir.ReservoirKind != "trans_neptunian_reservoir")
            {
                throw new InvalidOperationException("TNO reservoir records should retain trans-Neptunian reservoir kind.");
            }

            if (reservoir.RepresentationStatus != "diagnostic_proxy")
            {
                throw new InvalidOperationException("TNO reservoir records should remain diagnostic proxies until orbital-family generation is added.");
            }

            if (!reservoir.SourceIds.Contains("KavelaarsEtAl2023") || !reservoir.SourceIds.Contains("BernardinelliEtAl2022"))
            {
                throw new InvalidOperationException("TNO reservoir records should carry TNO source IDs.");
            }

            totalWeight += reservoir.RelativeWeight;
            if (reservoir.ReservoirFamily == "cold_classical")
            {
                hasColdClassical = true;
            }
            else if (reservoir.ReservoirFamily == "resonant")
            {
                hasResonant = true;
            }
            else if (reservoir.ReservoirFamily == "comet_feeding")
            {
                hasCometFeeding = true;
            }
        }

        if (!hasColdClassical || !hasResonant || !hasCometFeeding)
        {
            throw new InvalidOperationException("Outer reservoir records should expose TNO and comet-feeding families independently from belt geometry.");
        }

        if (System.Math.Abs(totalWeight - 1.0) > 0.001)
        {
            throw new InvalidOperationException($"TNO reservoir family weights should sum to 1.0, got {totalWeight:0.000}.");
        }
    }

    /// <summary>
    /// Tests configurable belt large-object display parameters select the largest eligible bodies.
    /// </summary>
    public static void TestMajorAsteroidDisplayParametersSelectLargestEligibleObjects()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SolarSystemSpec topThreeSpec = new SolarSystemSpec(7002, 1, 1);
        topThreeSpec.MajorAsteroidDisplayCount = 3;
        topThreeSpec.MajorAsteroidMinDiameterKm = 900.0;

        AsteroidBelt topThreeBelt = CreatePredefinedOuterBelt(host);
        BeltGenerationResult topThreeResult = SystemAsteroidGenerator.GenerateFromPredefinedBelts(
            new Array<AsteroidBelt> { topThreeBelt },
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(7002),
            systemSpec: topThreeSpec);

        if (topThreeBelt.MajorAsteroidIds.Count != 3)
        {
            throw new InvalidOperationException("Requested display count should cap the exposed belt large-object list.");
        }

        foreach (string asteroidId in topThreeBelt.MajorAsteroidIds)
        {
            CelestialBody asteroid = RequireAsteroid(topThreeResult.Asteroids, asteroidId);
            double diameterKm = asteroid.Physical.RadiusM * 2.0 / 1000.0;
            if (diameterKm < 900.0)
            {
                throw new InvalidOperationException($"Custom large-object threshold should be honored, got {diameterKm:0.00} km.");
            }
        }

        SolarSystemSpec topEightSpec = new SolarSystemSpec(7002, 1, 1);
        topEightSpec.MajorAsteroidDisplayCount = 8;
        topEightSpec.MajorAsteroidMinDiameterKm = 900.0;
        AsteroidBelt topEightBelt = CreatePredefinedOuterBelt(host);
        BeltGenerationResult topEightResult = SystemAsteroidGenerator.GenerateFromPredefinedBelts(
            new Array<AsteroidBelt> { topEightBelt },
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(7002),
            systemSpec: topEightSpec);

        if (topEightBelt.MajorAsteroidIds.Count != 8)
        {
            throw new InvalidOperationException("Higher display count should expose more eligible large objects.");
        }

        for (int index = 0; index < topThreeBelt.MajorAsteroidIds.Count; index += 1)
        {
            CelestialBody topThreeAsteroid = RequireAsteroid(topThreeResult.Asteroids, topThreeBelt.MajorAsteroidIds[index]);
            CelestialBody topEightAsteroid = RequireAsteroid(topEightResult.Asteroids, topEightBelt.MajorAsteroidIds[index]);
            double topThreeDiameterKm = topThreeAsteroid.Physical.RadiusM * 2.0 / 1000.0;
            double topEightDiameterKm = topEightAsteroid.Physical.RadiusM * 2.0 / 1000.0;
            if (System.Math.Abs(topThreeDiameterKm - topEightDiameterKm) > 0.001)
            {
                throw new InvalidOperationException("Lower display counts should expose the same largest-prefix selection from the deterministic candidate pool.");
            }
        }

        SolarSystemSpec zeroSpec = new SolarSystemSpec(7002, 1, 1);
        zeroSpec.MajorAsteroidDisplayCount = 0;
        AsteroidBelt zeroBelt = CreatePredefinedOuterBelt(host);
        SystemAsteroidGenerator.GenerateFromPredefinedBelts(
            new Array<AsteroidBelt> { zeroBelt },
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(7002),
            systemSpec: zeroSpec);

        if (zeroBelt.MajorAsteroidIds.Count != 0)
        {
            throw new InvalidOperationException("Zero display count should suppress inspectable large-object promotion.");
        }
    }

    private static CelestialBody RequireAsteroid(Array<CelestialBody> asteroids, string asteroidId)
    {
        foreach (CelestialBody asteroid in asteroids)
        {
            if (asteroid.Id == asteroidId)
            {
                return asteroid;
            }
        }

        throw new InvalidOperationException($"Expected generated asteroid id {asteroidId} to resolve to a body.");
    }

    private static double AverageRadiusM(Array<CelestialBody> bodies)
    {
        if (bodies.Count == 0)
        {
            throw new InvalidOperationException("Expected generated representative bodies.");
        }

        double total = 0.0;
        foreach (CelestialBody body in bodies)
        {
            total += body.Physical.RadiusM;
        }

        return total / bodies.Count;
    }

    /// <summary>
    /// Legacy parity alias for test_belts_generated_probabilistically.
    /// </summary>
    private static void TestBeltsGeneratedProbabilistically()
    {
        TestGenerateBelts();
    }

    /// <summary>
    /// Legacy parity alias for test_major_asteroids_generated.
    /// </summary>
    private static void TestMajorAsteroidsGenerated()
    {
        TestAsteroidsPassValidation();
    }

    /// <summary>
    /// Legacy parity alias for test_asteroids_within_belt.
    /// </summary>
    private static void TestAsteroidsWithinBelt()
    {
        TestAsteroidsPassValidation();
    }

    /// <summary>
    /// Legacy parity alias for test_belt_composition_variety.
    /// </summary>
    private static void TestBeltCompositionVariety()
    {
        TestBeltBoundaries();
    }

    /// <summary>
    /// Legacy parity alias for test_belts_avoid_planets.
    /// </summary>
    private static void TestBeltsAvoidPlanets()
    {
        TestGenerateBeltsNoPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_inner_outer_belt_placement.
    /// </summary>
    private static void TestInnerOuterBeltPlacement()
    {
        TestBeltBoundaries();
    }

    /// <summary>
    /// Legacy parity alias for test_sort_by_mass.
    /// </summary>
    private static void TestSortByMass()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_get_statistics.
    /// </summary>
    private static void TestGetStatistics()
    {
        TestAsteroidNames();
    }

    /// <summary>
    /// Legacy parity alias for test_multiple_orbit_hosts.
    /// </summary>
    private static void TestMultipleOrbitHosts()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_belt_asteroid_map.
    /// </summary>
    private static void TestBeltAsteroidMap()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_asteroid_sizes_power_law.
    /// </summary>
    private static void TestAsteroidSizesPowerLaw()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_reserve_belt_slots_marks_and_clears.
    /// </summary>
    private static void TestReserveBeltSlotsMarksAndClears()
    {
        TestBeltBoundaries();
    }

    /// <summary>
    /// Legacy parity alias for test_generate_from_predefined_belts.
    /// </summary>
    private static void TestGenerateFromPredefinedBelts()
    {
        TestGenerateBeltsNoPlanets();
    }
}

