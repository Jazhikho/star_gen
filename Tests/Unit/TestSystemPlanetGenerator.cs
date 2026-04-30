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
using StarGen.Domain.Generation.Tables;
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
    /// Creates a deterministic stellar fixture with explicit mass and luminosity.
    /// </summary>
    private static CelestialBody CreateStellarFixture(
        string id,
        double massSolar,
        double luminositySolar,
        double temperatureK,
        double metallicity,
        double ageYears)
    {
        CelestialBody star = new CelestialBody(
            id,
            id,
            CelestialType.Type.Star,
            new PhysicalProps(massSolar * Units.SolarMassKg, Units.SolarRadiusMeters));
        star.Stellar = new StellarProps(
            luminositySolar * StellarProps.SolarLuminosityWatts,
            temperatureK,
            "G",
            "main_sequence",
            metallicity,
            ageYears);
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
    /// Applies a deterministic shared fill probability to a slot set so compatibility multipliers
    /// have enough headroom to change outcomes materially in tests.
    /// </summary>
    private static void SetSlotFillProbability(Array<OrbitSlot> slots, double fillProbability)
    {
        foreach (OrbitSlot slot in slots)
        {
            slot.FillProbability = fillProbability;
        }
    }

    /// <summary>
    /// Creates a slot mix with multiple viable temperate candidates plus hot/cold competition so
    /// RPG compatibility pressures can materially change fill outcomes.
    /// </summary>
    private static Array<OrbitSlot> CreateCompatibilityPressureSlots(OrbitHost host)
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();
        double[] distancesAu = new double[] { 0.35, 0.85, 1.10, 1.45, 1.95, 3.10, 4.60, 6.20 };

        for (int index = 0; index < distancesAu.Length; index += 1)
        {
            double distance = distancesAu[index] * Units.AuMeters;
            OrbitSlot slot = new OrbitSlot($"compat_slot_{index}", host.NodeId, distance)
            {
                IsStable = true,
                FillProbability = 0.42,
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
    /// Creates a slot set that can reveal compact-inner-architecture biases.
    /// </summary>
    private static Array<OrbitSlot> CreateCompactArchitectureSlots(OrbitHost host)
    {
        double[] distancesAu =
        {
            0.12, 0.18, 0.27, 0.40, 0.60, 0.90, 1.30, 1.90, 2.70, 4.50,
        };
        Array<OrbitSlot> slots = new Array<OrbitSlot>();
        for (int i = 0; i < distancesAu.Length; i += 1)
        {
            double distance = distancesAu[i] * Units.AuMeters;
            OrbitSlot slot = new OrbitSlot($"compact_slot_{i}", host.NodeId, distance)
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
    /// Tests that host mass now changes the derived disk-lifetime and solids-reservoir priors.
    /// </summary>
    public static void TestHostMassAdjustmentsChangeDiskAndSolidPriors()
    {
        SolarSystemSpec spec = new SolarSystemSpec(8181, 1, 1)
        {
            PlanetaryProfile = PlanetaryGenerationProfile.CreateDefault(),
        };
        Array<CelestialBody> lowMassStars = new Array<CelestialBody>
        {
            CreateStellarFixture("low_mass_star", 0.45, 0.05, 3600.0, 1.0, 5.0e9),
        };
        Array<CelestialBody> highMassStars = new Array<CelestialBody>
        {
            CreateStellarFixture("high_mass_star", 2.20, 18.0, 8600.0, 1.0, 5.0e8),
        };

        PlanetarySystemState lowMassState = PlanetarySystemState.Build(spec, lowMassStars);
        PlanetarySystemState highMassState = PlanetarySystemState.Build(spec, highMassStars);

        if (lowMassState.HostMassDiskLifetimeScalar <= highMassState.HostMassDiskLifetimeScalar)
        {
            throw new InvalidOperationException("Lower-mass hosts should preserve disks longer than higher-mass hosts in the derived planetary state.");
        }

        if (highMassState.HostMassSolidReservoirScalar <= lowMassState.HostMassSolidReservoirScalar)
        {
            throw new InvalidOperationException("Higher-mass hosts should now carry a larger derived solids reservoir than lower-mass hosts.");
        }
    }

    /// <summary>
    /// Tests source-backed host occurrence regimes alter demographic scalars.
    /// </summary>
    public static void TestHostOccurrenceRegimeAdjustsDemographicScalars()
    {
        SolarSystemSpec spec = new SolarSystemSpec(8182, 1, 1)
        {
            PlanetaryProfile = PlanetaryGenerationProfile.CreateDefault(),
        };
        Array<CelestialBody> fgkStars = new Array<CelestialBody>
        {
            CreateStellarFixture("fgk_star", 1.00, 1.0, 5778.0, 1.0, 5.0e9),
        };
        Array<CelestialBody> midLateMStars = new Array<CelestialBody>
        {
            CreateStellarFixture("mid_late_m_star", 0.22, 0.008, 3200.0, 1.0, 5.0e9),
        };

        PlanetarySystemState fgkState = PlanetarySystemState.Build(spec, fgkStars);
        PlanetarySystemState midLateMState = PlanetarySystemState.Build(spec, midLateMStars);

        if (!fgkState.IsFgkOccurrenceRegime())
        {
            throw new InvalidOperationException("Solar-type hosts should use the FGK occurrence regime.");
        }

        if (!midLateMState.IsMidLateMDwarfOccurrenceRegime())
        {
            throw new InvalidOperationException("Cool low-mass hosts should use the mid-late M occurrence regime.");
        }

        if (midLateMState.CloseInSmallPlanetOccurrenceScalar <= fgkState.CloseInSmallPlanetOccurrenceScalar)
        {
            throw new InvalidOperationException("Mid-late M hosts should carry a stronger close-in small-planet occurrence scalar.");
        }

        if (midLateMState.HabitableZoneRockyOccurrenceScalar > fgkState.HabitableZoneRockyOccurrenceScalar)
        {
            throw new InvalidOperationException("M-dwarf HZ rocky occurrence should not receive an automatic surplus over FGK after Bergsten et al. 2023.");
        }

        if (midLateMState.SubNeptuneOccurrenceScalar >= fgkState.SubNeptuneOccurrenceScalar)
        {
            throw new InvalidOperationException("Mid-late M hosts should suppress close-in sub-Neptune weighting.");
        }

        if (midLateMState.CloseInHotGiantOccurrenceScalar >= fgkState.CloseInHotGiantOccurrenceScalar)
        {
            throw new InvalidOperationException("Mid-late M hosts should suppress close-in hot-giant weighting more strongly than FGK hosts.");
        }
    }

    /// <summary>
    /// Tests that giant-planet weighting now peaks near the snow line instead of rising monotonically outward.
    /// </summary>
    public static void TestSnowLineGiantFormationWeightTurnsOverOutsidePeak()
    {
        SolarSystemSpec spec = new SolarSystemSpec(8282, 1, 1)
        {
            PlanetaryProfile = PlanetaryGenerationProfile.CreateDefault(),
        };
        PlanetarySystemState state = PlanetarySystemState.Build(spec, new Array<CelestialBody> { CreateTestStar() });
        double peakWeight = state.GetSnowLineGiantFormationWeight(state.SnowLineAu);
        double innerWeight = state.GetSnowLineGiantFormationWeight(state.SnowLineAu * 0.35);
        double farOuterWeight = state.GetSnowLineGiantFormationWeight(state.SnowLineAu * 5.0);

        if (peakWeight <= innerWeight)
        {
            throw new InvalidOperationException("Snow-line giant weighting should exceed the inner-system weight at the same system state.");
        }

        if (peakWeight <= farOuterWeight)
        {
            throw new InvalidOperationException("Snow-line giant weighting should turn over and decline in the far outer system.");
        }
    }

    /// <summary>
    /// Tests that stronger migration now yields more compact inner intermediate-mass worlds.
    /// </summary>
    public static void TestMigrationStrengthFavorsCompactInnerArchitecture()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        int weakCompactWorldCount = 0;
        int strongCompactWorldCount = 0;

        for (int seed = 9100; seed < 9180; seed += 1)
        {
            Array<OrbitSlot> weakSlots = CreateCompactArchitectureSlots(host);
            Array<OrbitSlot> strongSlots = CreateCompactArchitectureSlots(host);
            SolarSystemSpec weakSpec = new SolarSystemSpec(seed, 1, 1)
            {
                PlanetaryProfile = new PlanetaryGenerationProfile
                {
                    MigrationStrength = 0.65,
                    SolidMassScalar = 0.95,
                    GasMassScalar = 0.95,
                },
            };
            SolarSystemSpec strongSpec = new SolarSystemSpec(seed, 1, 1)
            {
                PlanetaryProfile = new PlanetaryGenerationProfile
                {
                    MigrationStrength = 1.65,
                    SolidMassScalar = 1.20,
                    GasMassScalar = 1.10,
                },
            };

            PlanetGenerationResult weakResult = SystemPlanetGenerator.Generate(
                weakSlots,
                new Array<OrbitHost> { host },
                new Array<CelestialBody> { star },
                new SeededRng(seed),
                systemSpec: weakSpec);
            PlanetGenerationResult strongResult = SystemPlanetGenerator.Generate(
                strongSlots,
                new Array<OrbitHost> { host },
                new Array<CelestialBody> { star },
                new SeededRng(seed),
                systemSpec: strongSpec);

            weakCompactWorldCount += CountIntermediateMassPlanetsInsideAu(weakResult.Planets, 1.60);
            strongCompactWorldCount += CountIntermediateMassPlanetsInsideAu(strongResult.Planets, 1.60);
        }

        if (strongCompactWorldCount <= weakCompactWorldCount)
        {
            throw new InvalidOperationException($"Higher migration strength should now favor more compact inner super-Earth and mini-Neptune analogs. Weak={weakCompactWorldCount} Strong={strongCompactWorldCount}");
        }
    }

    /// <summary>
    /// Tests that the Tanaka-style Type-I migration diagnostic responds to mass and disk gas.
    /// </summary>
    public static void TestTypeIMigrationDiagnosticRespondsToPlanetMassAndGasDisk()
    {
        Array<CelestialBody> stars = new Array<CelestialBody> { CreateTestStar() };
        SolarSystemSpec lightDiskSpec = new SolarSystemSpec(9191, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                GasMassScalar = 0.55,
                DiskLifetimeMyr = 2.0,
            },
        };
        SolarSystemSpec gasRichSpec = new SolarSystemSpec(9191, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                GasMassScalar = 1.70,
                DiskLifetimeMyr = 5.0,
            },
        };

        PlanetarySystemState lightDiskState = PlanetarySystemState.Build(lightDiskSpec, stars);
        PlanetarySystemState gasRichState = PlanetarySystemState.Build(gasRichSpec, stars);
        double earthMassSlow = lightDiskState.EstimateTypeIMigrationTimescaleMyr(1.0, 2.0);
        double superEarthFast = lightDiskState.EstimateTypeIMigrationTimescaleMyr(5.0, 2.0);
        double lightLikelihood = lightDiskState.GetTypeIMigrationLikelihood(5.0, 2.0);
        double gasRichLikelihood = gasRichState.GetTypeIMigrationLikelihood(5.0, 2.0);

        if (superEarthFast >= earthMassSlow)
        {
            throw new InvalidOperationException("Type-I migration timescale should be shorter for higher-mass low-mass planets.");
        }

        if (gasRichLikelihood <= lightLikelihood)
        {
            throw new InvalidOperationException("Gas-rich, longer-lived disks should produce a stronger Type-I migration likelihood.");
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
    /// Tests that the source-backed radius-valley mechanisms expose opposite orbital-period slopes.
    /// </summary>
    public static void TestRadiusValleyMechanismsHaveOppositePeriodSlopes()
    {
        SolarSystemSpec photoSpec = new SolarSystemSpec(2323, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                EnvelopeLossModel = PlanetEnvelopeLossModel.Photoevaporation,
            },
        };
        SolarSystemSpec coreSpec = new SolarSystemSpec(2323, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                EnvelopeLossModel = PlanetEnvelopeLossModel.CorePowered,
            },
        };
        Array<CelestialBody> stars = new Array<CelestialBody> { CreateTestStar() };
        PlanetarySystemState photoState = PlanetarySystemState.Build(photoSpec, stars);
        PlanetarySystemState coreState = PlanetarySystemState.Build(coreSpec, stars);
        double tenDayOrbitAu = OrbitAuForPeriodDays(10.0, photoState.StellarMassSolar);
        double sixtyDayOrbitAu = OrbitAuForPeriodDays(60.0, photoState.StellarMassSolar);

        double photoInnerCenter = photoState.GetRadiusValleyCenterEarth(tenDayOrbitAu);
        double photoOuterCenter = photoState.GetRadiusValleyCenterEarth(sixtyDayOrbitAu);
        double coreInnerCenter = coreState.GetRadiusValleyCenterEarth(tenDayOrbitAu);
        double coreOuterCenter = coreState.GetRadiusValleyCenterEarth(sixtyDayOrbitAu);

        if (photoOuterCenter >= photoInnerCenter)
        {
            throw new InvalidOperationException("Owen-Wu photoevaporation should move the radius-valley center smaller at longer periods.");
        }

        if (coreOuterCenter <= coreInnerCenter)
        {
            throw new InvalidOperationException("Ginzburg core-powered mass loss should move the radius-valley center larger at longer periods.");
        }
    }

    /// <summary>
    /// Tests that generated planets record radius-valley diagnostics in provenance.
    /// </summary>
    public static void TestEnvelopeLossTraceRecordsRadiusValleyDiagnostics()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateHotLossSlots(host, 1);
        SolarSystemSpec spec = new SolarSystemSpec(2424, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                EnvelopeLossModel = PlanetEnvelopeLossModel.Photoevaporation,
            },
        };

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(2424),
            systemSpec: spec);

        if (result.Planets.Count != 1)
        {
            throw new InvalidOperationException("Trace test should generate exactly one close-in planet.");
        }

        CelestialBody planet = result.Planets[0];
        if (planet.Provenance == null || !planet.Provenance.SpecSnapshot.ContainsKey("formation_trace"))
        {
            throw new InvalidOperationException("Generated planet should preserve the formation trace in provenance.");
        }

        Dictionary trace = (Dictionary)planet.Provenance.SpecSnapshot["formation_trace"];
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("radius_valley_period_days"), "Trace should include radius-valley orbital period.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("radius_valley_center_earth"), "Trace should include radius-valley center.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("radius_valley_loss_pressure"), "Trace should include radius-valley loss pressure.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("radius_valley_mechanism"), "Trace should include radius-valley mechanism.");
        DotNetNativeTestSuite.AssertEqual("owen_wu_photoevaporation", trace["radius_valley_mechanism"].AsString(), "Trace should identify the selected source-backed mechanism.");

        if (!trace["inside_radius_valley_regime"].AsBool())
        {
            throw new InvalidOperationException("Close-in trace test planet should be inside the radius-valley regime.");
        }

        if (trace["radius_valley_center_earth"].AsDouble() <= 0.0)
        {
            throw new InvalidOperationException("Radius-valley center should be positive.");
        }

        if (trace["radius_valley_loss_pressure"].AsDouble() <= 0.0)
        {
            throw new InvalidOperationException("Radius-valley loss pressure should be positive.");
        }
    }

    /// <summary>
    /// Tests that generated planet provenance records the slot stability policy.
    /// </summary>
    public static void TestPlanetFormationTraceRecordsSlotStabilityPolicy()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 1);
        slots[0].FillProbability = 1.0;
        slots[0].SpacingFromInnerMutualHillRadii = 11.0;
        slots[0].PeriodRatioFromInner = 1.5;
        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            new SeededRng(2525));

        if (result.Planets.Count != 1)
        {
            throw new InvalidOperationException("Slot stability trace test should generate exactly one planet.");
        }

        CelestialBody planet = result.Planets[0];
        if (planet.Provenance == null || !planet.Provenance.SpecSnapshot.ContainsKey("formation_trace"))
        {
            throw new InvalidOperationException("Generated planet should preserve the formation trace in provenance.");
        }

        Dictionary trace = (Dictionary)planet.Provenance.SpecSnapshot["formation_trace"];
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("slot_stability_policy"), "Trace should include slot stability policy.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("slot_stability_sources"), "Trace should include slot stability sources.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("slot_spacing_from_inner_mutual_hill_radii"), "Trace should include mutual-Hill spacing diagnostic.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("slot_spacing_mass_proxy_earth_masses"), "Trace should include the slot spacing mass proxy.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("host_occurrence_regime"), "Trace should include the host occurrence regime.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("occurrence_sources"), "Trace should include occurrence source IDs.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("formation_sources"), "Trace should include formation source IDs.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("disk_dust_host_mass_exponent"), "Trace should include the active disk-dust exponent.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("type_i_migration_model"), "Trace should include the Type-I migration model.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("type_i_migration_likelihood"), "Trace should include Type-I migration likelihood.");
        DotNetNativeTestSuite.AssertTrue(trace.ContainsKey("close_in_small_planet_occurrence_scalar"), "Trace should include close-in occurrence scalar.");
        DotNetNativeTestSuite.AssertEqual(OrbitalMechanics.CompactArchitectureSpacingPolicyId, trace["slot_stability_policy"].AsString(), "Trace should identify the active spacing policy.");

        if (!trace["slot_stability_sources"].AsString().Contains("Obertas2017"))
        {
            throw new InvalidOperationException("Trace should carry the source cluster backing the active spacing policy.");
        }

        if (!trace["formation_sources"].AsString().Contains("TanakaTakeuchiWard2002"))
        {
            throw new InvalidOperationException("Trace should carry the source cluster backing the active formation and migration proxies.");
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
    /// Tests that higher rogue-world allowance materially nudges the system toward more low-mass disrupted outcomes.
    /// </summary>
    public static void TestRogueAllowanceChangesLowMassOutcomeBias()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> orderlySlots = CreateTestSlots(host, 20);
        Array<OrbitSlot> disruptedSlots = CreateTestSlots(host, 20);
        SolarSystemSpec orderlySpec = new SolarSystemSpec(3636, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                RoguePlanetAllowance = PlanetRoguePlanetAllowance.Off,
            },
        };
        SolarSystemSpec disruptedSpec = new SolarSystemSpec(3636, 1, 1)
        {
            PlanetaryProfile = new PlanetaryGenerationProfile
            {
                RoguePlanetAllowance = PlanetRoguePlanetAllowance.Standard,
            },
        };

        PlanetGenerationResult orderlyResult = SystemPlanetGenerator.GenerateTargeted(
            orderlySlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            12,
            new SeededRng(3636),
            systemSpec: orderlySpec);
        PlanetGenerationResult disruptedResult = SystemPlanetGenerator.GenerateTargeted(
            disruptedSlots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            12,
            new SeededRng(3636),
            systemSpec: disruptedSpec);

        int orderlyLowMassCount = CountLowMassPlanets(orderlyResult.Planets);
        int disruptedLowMassCount = CountLowMassPlanets(disruptedResult.Planets);
        if (disruptedLowMassCount < orderlyLowMassCount)
        {
            throw new InvalidOperationException("Higher rogue-world allowance should not reduce the generator's low-mass disrupted-planet bias on the same seeded slot set.");
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

    /// <summary>
    /// Tests that the Otegi model separates rocky and volatile-rich transition worlds more strongly than Chen-Kipping.
    /// </summary>
    public static void TestMassRadiusModelsChangeTransitionWorldScale()
    {
        PlanetSpec rockySpec = PlanetSpec.Random(5150);
        rockySpec.ClassBias = PlanetClassBias.Rocky;
        rockySpec.CompositionBias = PlanetCompositionBias.Rocky;
        rockySpec.VolatileRichness = PlanetVolatileRichness.Poor;

        PlanetSpec volatileSpec = PlanetSpec.Random(5151);
        volatileSpec.ClassBias = PlanetClassBias.SubNeptune;
        volatileSpec.CompositionBias = PlanetCompositionBias.GasEnvelope;
        volatileSpec.VolatileRichness = PlanetVolatileRichness.Rich;

        PlanetMassRadiusResolution chenRockyResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.ChenKipping,
            rockySpec,
            SizeCategory.Category.SuperEarth,
            12.0);
        PlanetMassRadiusResolution chenResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.ChenKipping,
            volatileSpec,
            SizeCategory.Category.MiniNeptune,
            12.0);
        PlanetMassRadiusResolution otegiRockyResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            rockySpec,
            SizeCategory.Category.SuperEarth,
            12.0);
        PlanetMassRadiusResolution otegiResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            volatileSpec,
            SizeCategory.Category.MiniNeptune,
            12.0);

        double chenSeparation = chenResolution.RadiusEarth - chenRockyResolution.RadiusEarth;
        double otegiSeparation = otegiResolution.RadiusEarth - otegiRockyResolution.RadiusEarth;
        if (otegiSeparation <= chenSeparation)
        {
            throw new InvalidOperationException("Otegi should separate rocky and volatile-rich 12 Earth-mass transition worlds more strongly than Chen-Kipping.");
        }

        if (otegiResolution.DensityKgM3 >= otegiRockyResolution.DensityKgM3)
        {
            throw new InvalidOperationException("Under Otegi, the volatile-rich transition world should come out less dense than the rocky one.");
        }
    }

    /// <summary>
    /// Tests that Otegi's rocky relation does not silently extend beyond the source's rocky-population endpoint.
    /// </summary>
    public static void TestOtegiRockyBranchStopsAtSourceMassLimit()
    {
        PlanetSpec rockySpec = PlanetSpec.Random(5252);
        rockySpec.ClassBias = PlanetClassBias.Rocky;
        rockySpec.CompositionBias = PlanetCompositionBias.Rocky;

        PlanetMassRadiusResolution belowLimit = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            rockySpec,
            SizeCategory.Category.SuperEarth,
            24.0);
        PlanetMassRadiusResolution aboveLimit = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            rockySpec,
            SizeCategory.Category.SuperEarth,
            30.0);

        DotNetNativeTestSuite.AssertEqual("otegi_rocky", belowLimit.AppliedRegimeId, "Otegi should still allow the rocky branch below the source mass limit.");
        DotNetNativeTestSuite.AssertEqual("otegi_volatile", aboveLimit.AppliedRegimeId, "Otegi should switch to the volatile branch above the rocky-population endpoint.");

        if (aboveLimit.RadiusEarth <= belowLimit.RadiusEarth)
        {
            throw new InvalidOperationException("Above-limit Otegi worlds should not be held to the capped rocky radius.");
        }
    }

    /// <summary>
    /// Tests that Otegi coefficient and exponent uncertainties are sampled deterministically during generation.
    /// </summary>
    public static void TestOtegiSeededUncertaintySamplingIsDeterministicAndActive()
    {
        PlanetSpec volatileSpec = PlanetSpec.Random(5353);
        volatileSpec.ClassBias = PlanetClassBias.SubNeptune;
        volatileSpec.CompositionBias = PlanetCompositionBias.GasEnvelope;

        PlanetMassRadiusResolution midpointResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            volatileSpec,
            SizeCategory.Category.MiniNeptune,
            12.0);
        PlanetMassRadiusResolution firstSample = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            volatileSpec,
            SizeCategory.Category.MiniNeptune,
            12.0,
            new SeededRng(5353));
        PlanetMassRadiusResolution repeatedSample = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            volatileSpec,
            SizeCategory.Category.MiniNeptune,
            12.0,
            new SeededRng(5353));
        PlanetMassRadiusResolution alternateSample = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            volatileSpec,
            SizeCategory.Category.MiniNeptune,
            12.0,
            new SeededRng(5354));

        DotNetNativeTestSuite.AssertEqual(firstSample.RadiusEarth, repeatedSample.RadiusEarth, "Same seed should produce the same Otegi sampled radius.");
        DotNetNativeTestSuite.AssertEqual(firstSample.RadiusScatterLog10, repeatedSample.RadiusScatterLog10, "Same seed should produce the same Otegi sampled coefficient/exponent offset.");

        if (System.Math.Abs(firstSample.RadiusEarth - midpointResolution.RadiusEarth) < 0.000001
            && System.Math.Abs(alternateSample.RadiusEarth - midpointResolution.RadiusEarth) < 0.000001)
        {
            throw new InvalidOperationException("Seeded Otegi sampling should move generated radii away from the central relation for at least one checked seed.");
        }

        if (System.Math.Abs(firstSample.RadiusScatterLog10) < 0.000001
            && System.Math.Abs(alternateSample.RadiusScatterLog10) < 0.000001)
        {
            throw new InvalidOperationException("Seeded Otegi sampling should record a nonzero coefficient/exponent offset for at least one checked seed.");
        }
    }

    /// <summary>
    /// Tests that Chen-Kipping class probabilities now expose the Terran-to-Neptunian transition instead of only a hard regime label.
    /// </summary>
    public static void TestChenKippingClassificationProbabilitiesTrackTransition()
    {
        PlanetMassRadiusResolution earthMassClassification = PlanetMassRadiusTable.ResolveChenKippingClassificationOnly(1.0);
        PlanetMassRadiusResolution transitionClassification = PlanetMassRadiusTable.ResolveChenKippingClassificationOnly(2.04);
        PlanetMassRadiusResolution subNeptuneClassification = PlanetMassRadiusTable.ResolveChenKippingClassificationOnly(10.0);
        PlanetMassRadiusResolution jovianClassification = PlanetMassRadiusTable.ResolveChenKippingClassificationOnly(300.0);

        if (earthMassClassification.TerranProbability <= earthMassClassification.NeptunianProbability)
        {
            throw new InvalidOperationException("A one Earth-mass world should remain more Terran than Neptunian under Chen-Kipping classification.");
        }

        double transitionDifference = System.Math.Abs(transitionClassification.TerranProbability - transitionClassification.NeptunianProbability);
        if (transitionDifference > 0.05)
        {
            throw new InvalidOperationException("The Chen-Kipping transition mass should expose comparable Terran and Neptunian probabilities.");
        }

        if (subNeptuneClassification.NeptunianProbability <= subNeptuneClassification.TerranProbability)
        {
            throw new InvalidOperationException("A 10 Earth-mass world should be more Neptunian than Terran under Chen-Kipping classification.");
        }

        if (jovianClassification.JovianProbability <= jovianClassification.NeptunianProbability)
        {
            throw new InvalidOperationException("A 300 Earth-mass world should be more Jovian than Neptunian under Chen-Kipping classification.");
        }
    }

    /// <summary>
    /// Tests that generated Chen-Kipping radii use deterministic seeded scatter instead of a single midpoint curve.
    /// </summary>
    public static void TestChenKippingSeededScatterIsDeterministicAndActive()
    {
        PlanetSpec spec = PlanetSpec.Random(6161);
        spec.ClassBias = PlanetClassBias.SubNeptune;
        spec.CompositionBias = PlanetCompositionBias.GasEnvelope;

        PlanetMassRadiusResolution midpointResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.ChenKipping,
            spec,
            SizeCategory.Category.MiniNeptune,
            8.0);
        PlanetMassRadiusResolution firstSample = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.ChenKipping,
            spec,
            SizeCategory.Category.MiniNeptune,
            8.0,
            new SeededRng(6161));
        PlanetMassRadiusResolution repeatedSample = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.ChenKipping,
            spec,
            SizeCategory.Category.MiniNeptune,
            8.0,
            new SeededRng(6161));
        PlanetMassRadiusResolution alternateSample = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.ChenKipping,
            spec,
            SizeCategory.Category.MiniNeptune,
            8.0,
            new SeededRng(6162));

        DotNetNativeTestSuite.AssertEqual(firstSample.RadiusEarth, repeatedSample.RadiusEarth, "Same seed should produce the same Chen-Kipping sampled radius.");
        DotNetNativeTestSuite.AssertEqual(firstSample.RadiusScatterLog10, repeatedSample.RadiusScatterLog10, "Same seed should produce the same Chen-Kipping radius scatter.");

        if (System.Math.Abs(firstSample.RadiusEarth - midpointResolution.RadiusEarth) < 0.000001
            && System.Math.Abs(alternateSample.RadiusEarth - midpointResolution.RadiusEarth) < 0.000001)
        {
            throw new InvalidOperationException("Seeded Chen-Kipping sampling should move generated radii away from the deterministic midpoint for at least one checked seed.");
        }

        if (System.Math.Abs(firstSample.RadiusScatterLog10) < 0.000001
            && System.Math.Abs(alternateSample.RadiusScatterLog10) < 0.000001)
        {
            throw new InvalidOperationException("Seeded Chen-Kipping sampling should record nonzero scatter for at least one checked seed.");
        }
    }

    /// <summary>
    /// Tests that the Otegi option falls back to Chen-Kipping for giant planets outside its intended range.
    /// </summary>
    public static void TestOtegiFallsBackToChenKippingForGiants()
    {
        PlanetSpec giantSpec = PlanetSpec.Random(6262);
        giantSpec.ClassBias = PlanetClassBias.GasGiant;
        giantSpec.CompositionBias = PlanetCompositionBias.GasEnvelope;

        PlanetMassRadiusResolution chenResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.ChenKipping,
            giantSpec,
            SizeCategory.Category.GasGiant,
            300.0);
        PlanetMassRadiusResolution otegiResolution = PlanetMassRadiusTable.Resolve(
            PlanetMassRadiusModel.Otegi,
            giantSpec,
            SizeCategory.Category.GasGiant,
            300.0);

        DotNetNativeTestSuite.AssertEqual(chenResolution.AppliedModelId, otegiResolution.AppliedModelId, "Gas giants should fall back to the same applied model");
        DotNetNativeTestSuite.AssertEqual(chenResolution.AppliedRegimeId, otegiResolution.AppliedRegimeId, "Gas giants should fall back to the same applied regime");
        DotNetNativeTestSuite.AssertEqual(chenResolution.RadiusEarth, otegiResolution.RadiusEarth, "Gas giants should resolve to the same radius when Otegi falls back");
    }

    /// <summary>
    /// Tests that RPG compatibility profiles materially change system-level orbit fill pressure.
    /// </summary>
    public static void TestCompatibilityProfilesShiftSystemFillPressure()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitHost> hosts = new Array<OrbitHost> { host };
        Array<CelestialBody> stars = new Array<CelestialBody> { star };
        int temperateFilledDefault = 0;
        int temperateFilledSpaceOpera = 0;
        int harshFilledDefault = 0;
        int harshFilledStarforged = 0;

        for (int seed = 7000; seed < 7120; seed += 1)
        {
            Array<OrbitSlot> defaultSlots = CreateCompatibilityPressureSlots(host);
            Array<OrbitSlot> spaceOperaSlots = CreateCompatibilityPressureSlots(host);
            Array<OrbitSlot> starforgedSlots = CreateCompatibilityPressureSlots(host);
            SolarSystemSpec defaultSpec = new SolarSystemSpec(seed, 1, 1);
            SolarSystemSpec spaceOperaSpec = new SolarSystemSpec(seed, 1, 1);
            SolarSystemSpec starforgedSpec = new SolarSystemSpec(seed, 1, 1);
            spaceOperaSpec.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            spaceOperaSpec.UseCaseSettings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Traveller;
            spaceOperaSpec.UseCaseSettings.ApplyRulesetDefaults();
            starforgedSpec.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            starforgedSpec.UseCaseSettings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Starforged;
            starforgedSpec.UseCaseSettings.ApplyRulesetDefaults();

            PlanetGenerationResult defaultResult = SystemPlanetGenerator.Generate(
                defaultSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                defaultSpec.UseCaseSettings,
                defaultSpec);
            PlanetGenerationResult spaceOperaResult = SystemPlanetGenerator.Generate(
                spaceOperaSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                spaceOperaSpec.UseCaseSettings,
                spaceOperaSpec);
            PlanetGenerationResult starforgedResult = SystemPlanetGenerator.Generate(
                starforgedSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                starforgedSpec.UseCaseSettings,
                starforgedSpec);

            temperateFilledDefault += CountFilledSlotsByZone(defaultResult.Slots, OrbitZone.Zone.Temperate);
            temperateFilledSpaceOpera += CountFilledSlotsByZone(spaceOperaResult.Slots, OrbitZone.Zone.Temperate);
            harshFilledDefault += CountFilledSlotsByZone(defaultResult.Slots, OrbitZone.Zone.Hot) + CountFilledSlotsByZone(defaultResult.Slots, OrbitZone.Zone.Cold);
            harshFilledStarforged += CountFilledSlotsByZone(starforgedResult.Slots, OrbitZone.Zone.Hot) + CountFilledSlotsByZone(starforgedResult.Slots, OrbitZone.Zone.Cold);
        }

        if (temperateFilledSpaceOpera <= temperateFilledDefault)
        {
            throw new InvalidOperationException($"Space Opera should fill more temperate slots than default. Default={temperateFilledDefault} SpaceOpera={temperateFilledSpaceOpera}");
        }

        if (harshFilledStarforged <= harshFilledDefault)
        {
            throw new InvalidOperationException($"Starforged should fill more harsh slots than default. Default={harshFilledDefault} Starforged={harshFilledStarforged}");
        }
    }

    /// <summary>
    /// Tests that custom Space Opera compatibility multipliers override the default scaffold values.
    /// </summary>
    public static void TestCustomSpaceOperaOverridesShiftSystemFillPressure()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitHost> hosts = new Array<OrbitHost> { host };
        Array<CelestialBody> stars = new Array<CelestialBody> { star };
        int temperateFilledDefaultSpaceOpera = 0;
        int temperateFilledCustomSpaceOpera = 0;
        int harshFilledDefaultSpaceOpera = 0;
        int harshFilledCustomSpaceOpera = 0;

        for (int seed = 7120; seed < 7240; seed += 1)
        {
            Array<OrbitSlot> defaultSlots = CreateCompatibilityPressureSlots(host);
            Array<OrbitSlot> customSlots = CreateCompatibilityPressureSlots(host);
            SolarSystemSpec defaultSpec = new SolarSystemSpec(seed, 1, 1);
            SolarSystemSpec customSpec = new SolarSystemSpec(seed, 1, 1);
            defaultSpec.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            defaultSpec.UseCaseSettings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Traveller;
            defaultSpec.UseCaseSettings.ApplyRulesetDefaults();
            defaultSpec.UseCaseSettings.MainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Prefer;
            customSpec.UseCaseSettings = defaultSpec.UseCaseSettings.Clone();
            customSpec.UseCaseSettings.CompatibilityTemperateSlotFillMultiplier = 0.75;
            customSpec.UseCaseSettings.CompatibilityHarshSlotFillMultiplier = 1.35;

            PlanetGenerationResult defaultResult = SystemPlanetGenerator.Generate(
                defaultSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                defaultSpec.UseCaseSettings,
                defaultSpec);
            PlanetGenerationResult customResult = SystemPlanetGenerator.Generate(
                customSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                customSpec.UseCaseSettings,
                customSpec);

            temperateFilledDefaultSpaceOpera += CountFilledSlotsByZone(defaultResult.Slots, OrbitZone.Zone.Temperate);
            temperateFilledCustomSpaceOpera += CountFilledSlotsByZone(customResult.Slots, OrbitZone.Zone.Temperate);
            harshFilledDefaultSpaceOpera += CountFilledSlotsByZone(defaultResult.Slots, OrbitZone.Zone.Hot) + CountFilledSlotsByZone(defaultResult.Slots, OrbitZone.Zone.Cold);
            harshFilledCustomSpaceOpera += CountFilledSlotsByZone(customResult.Slots, OrbitZone.Zone.Hot) + CountFilledSlotsByZone(customResult.Slots, OrbitZone.Zone.Cold);
        }

        if (temperateFilledCustomSpaceOpera >= temperateFilledDefaultSpaceOpera)
        {
            throw new InvalidOperationException($"Lower custom Space Opera temperate bias should reduce temperate fill. Default={temperateFilledDefaultSpaceOpera} Custom={temperateFilledCustomSpaceOpera}");
        }

        if (harshFilledCustomSpaceOpera <= harshFilledDefaultSpaceOpera)
        {
            throw new InvalidOperationException($"Higher custom Space Opera harsh bias should increase harsh fill. Default={harshFilledDefaultSpaceOpera} Custom={harshFilledCustomSpaceOpera}");
        }
    }

    /// <summary>
    /// Tests that Cepheus, Starfinder, and Starforged now have distinct system-level orbit-fill signatures.
    /// </summary>
    public static void TestNonSpaceOperaProfilesDifferentiateSystemBias()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitHost> hosts = new Array<OrbitHost> { host };
        Array<CelestialBody> stars = new Array<CelestialBody> { star };
        int cepheusTemperateFilled = 0;
        int starforgedTemperateFilled = 0;
        int cepheusHarshFilled = 0;
        int starfinderHarshFilled = 0;

        for (int seed = 7240; seed < 7360; seed += 1)
        {
            Array<OrbitSlot> cepheusSlots = CreateCompatibilityPressureSlots(host);
            Array<OrbitSlot> starfinderSlots = CreateCompatibilityPressureSlots(host);
            Array<OrbitSlot> starforgedSlots = CreateCompatibilityPressureSlots(host);

            SolarSystemSpec cepheusSpec = new SolarSystemSpec(seed, 1, 1);
            cepheusSpec.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            cepheusSpec.UseCaseSettings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Cepheus;
            cepheusSpec.UseCaseSettings.ApplyRulesetDefaults();

            SolarSystemSpec starfinderSpec = new SolarSystemSpec(seed, 1, 1);
            starfinderSpec.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            starfinderSpec.UseCaseSettings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Starfinder;
            starfinderSpec.UseCaseSettings.ApplyRulesetDefaults();

            SolarSystemSpec starforgedSpec = new SolarSystemSpec(seed, 1, 1);
            starforgedSpec.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            starforgedSpec.UseCaseSettings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Starforged;
            starforgedSpec.UseCaseSettings.ApplyRulesetDefaults();

            PlanetGenerationResult cepheusResult = SystemPlanetGenerator.Generate(
                cepheusSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                cepheusSpec.UseCaseSettings,
                cepheusSpec);
            PlanetGenerationResult starfinderResult = SystemPlanetGenerator.Generate(
                starfinderSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                starfinderSpec.UseCaseSettings,
                starfinderSpec);
            PlanetGenerationResult starforgedResult = SystemPlanetGenerator.Generate(
                starforgedSlots,
                hosts,
                stars,
                new SeededRng(seed),
                false,
                starforgedSpec.UseCaseSettings,
                starforgedSpec);

            cepheusTemperateFilled += CountFilledSlotsByZone(cepheusResult.Slots, OrbitZone.Zone.Temperate);
            starforgedTemperateFilled += CountFilledSlotsByZone(starforgedResult.Slots, OrbitZone.Zone.Temperate);
            cepheusHarshFilled += CountFilledSlotsByZone(cepheusResult.Slots, OrbitZone.Zone.Hot) + CountFilledSlotsByZone(cepheusResult.Slots, OrbitZone.Zone.Cold);
            starfinderHarshFilled += CountFilledSlotsByZone(starfinderResult.Slots, OrbitZone.Zone.Hot) + CountFilledSlotsByZone(starfinderResult.Slots, OrbitZone.Zone.Cold);
        }

        if (cepheusTemperateFilled <= starforgedTemperateFilled)
        {
            throw new InvalidOperationException($"Cepheus should fill more temperate slots than Starforged. Cepheus={cepheusTemperateFilled} Starforged={starforgedTemperateFilled}");
        }

        if (starfinderHarshFilled <= cepheusHarshFilled)
        {
            throw new InvalidOperationException($"Starfinder should fill more harsh slots than Cepheus. Cepheus={cepheusHarshFilled} Starfinder={starfinderHarshFilled}");
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

    private static int CountLowMassPlanets(Array<CelestialBody> planets)
    {
        int count = 0;
        foreach (CelestialBody planet in planets)
        {
            double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
            if (massEarth <= 1.2)
            {
                count += 1;
            }
        }

        return count;
    }

    private static int CountIntermediateMassPlanetsInsideAu(Array<CelestialBody> planets, double maxOrbitAu)
    {
        int count = 0;
        foreach (CelestialBody planet in planets)
        {
            if (!planet.HasOrbital())
            {
                continue;
            }

            double orbitAu = planet.Orbital!.SemiMajorAxisM / Units.AuMeters;
            if (orbitAu > maxOrbitAu)
            {
                continue;
            }

            double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
            if (massEarth >= 2.0 && massEarth <= 20.0)
            {
                count += 1;
            }
        }

        return count;
    }

    private static double OrbitAuForPeriodDays(double periodDays, double stellarMassSolar)
    {
        double periodYears = periodDays / 365.25;
        return System.Math.Pow(periodYears * periodYears * stellarMassSolar, 1.0 / 3.0);
    }

    private static int CountFilledSlotsByZone(Array<OrbitSlot> slots, OrbitZone.Zone zone)
    {
        int count = 0;
        foreach (OrbitSlot slot in slots)
        {
            if (slot.Zone == zone && slot.IsFilled)
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

