using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using System.Globalization;

namespace StarGen.Domain.Systems;

/// <summary>
/// Generates asteroid belts and representative major asteroids for a system.
/// </summary>
public static class SystemAsteroidGenerator
{
    private const int MaxMajorAsteroids = 10;
    private const double BeltProbabilityHot = 0.05;
    private const double BeltProbabilityTemperate = 0.12;
    private const double BeltProbabilityCold = 0.25;
    private const int MaxBeltsPerHost = 2;
    private const double MinBeltWidthFraction = 0.1;
    private const double MaxBeltWidthFraction = 0.4;
    private const double InnerBeltProbability = 0.60;
    private const double OuterBeltProbability = 0.50;
    private const double InnerBeltMassMinKg = 1.0e20;
    private const double InnerBeltMassMaxKg = 1.0e22;
    private const double OuterBeltMassMinKg = 1.0e20;
    private const double OuterBeltMassMaxKg = 1.0e23;
    private const double LargeObjectDiameterThresholdKm = 500.0;
    private const double InnerLargeObjectMaxDiameterKm = 1100.0;
    private const double OuterLargeObjectMaxDiameterKm = 2400.0;
    private const string InnerReservoirSourceIds = "DeMeoCarry2014;RaymondIzidoro2017";
    private const string OuterReservoirSourceIds = "KavelaarsEtAl2023;BernardinelliEtAl2022;BauerEtAl2017;RaymondIzidoro2017";
    private const string CompositionSourceIds = "DeMeoCarry2014";
    private const string InnerSizeSourceIds = "DeMeoCarry2014";
    private const string OuterSizeSourceIds = "KavelaarsEtAl2023;BernardinelliEtAl2022";
    private const string OuterSubfamilySourceIds = "KavelaarsEtAl2023;BernardinelliEtAl2022";

    /// <summary>
    /// Generates belts and their major asteroids for a system.
    /// </summary>
    public static BeltGenerationResult Generate(
        Array<OrbitHost> orbitHosts,
        Array<OrbitSlot> filledSlots,
        Array<CelestialBody> stars,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings = null,
        SolarSystemSpec? systemSpec = null)
    {
        BeltGenerationResult result = new();
        PlanetarySystemState planetaryState = PlanetarySystemState.Build(systemSpec, stars);

        foreach (OrbitHost host in orbitHosts)
        {
            Array<AsteroidBelt> hostBelts = GenerateBeltsForHost(host, filledSlots, planetaryState, rng);
            foreach (AsteroidBelt belt in hostBelts)
            {
                result.Belts.Add(belt);
                Array<CelestialBody> beltAsteroids = GenerateMajorAsteroids(belt, host, stars, planetaryState, rng, useCaseSettings);
                Array<string> asteroidIds = new();

                foreach (CelestialBody asteroid in beltAsteroids)
                {
                    result.Asteroids.Add(asteroid);
                    asteroidIds.Add(asteroid.Id);
                }

                belt.MajorAsteroidIds = asteroidIds;
                result.BeltAsteroidMap[belt.Id] = asteroidIds;
                AddReservoirRecordsForBelt(result.Reservoirs, belt);
            }
        }

        result.Success = true;
        return result;
    }

    /// <summary>
    /// Generates major asteroids for preselected belts.
    /// </summary>
    public static BeltGenerationResult GenerateFromPredefinedBelts(
        Array<AsteroidBelt> belts,
        Array<OrbitHost> orbitHosts,
        Array<CelestialBody> stars,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings = null,
        SolarSystemSpec? systemSpec = null)
    {
        BeltGenerationResult result = new();
        PlanetarySystemState planetaryState = PlanetarySystemState.Build(systemSpec, stars);
        System.Collections.Generic.Dictionary<string, OrbitHost> hostsById = new();
        foreach (OrbitHost host in orbitHosts)
        {
            hostsById[host.NodeId] = host;
        }

        foreach (AsteroidBelt belt in belts)
        {
            result.Belts.Add(belt);
            if (!hostsById.ContainsKey(belt.OrbitHostId))
            {
                continue;
            }

            Array<CelestialBody> beltAsteroids = GenerateMajorAsteroids(belt, hostsById[belt.OrbitHostId], stars, planetaryState, rng, useCaseSettings);
            Array<string> asteroidIds = new();
            foreach (CelestialBody asteroid in beltAsteroids)
            {
                result.Asteroids.Add(asteroid);
                asteroidIds.Add(asteroid.Id);
            }

            belt.MajorAsteroidIds = asteroidIds;
            result.BeltAsteroidMap[belt.Id] = asteroidIds;
            AddReservoirRecordsForBelt(result.Reservoirs, belt);
        }

        result.Success = true;
        return result;
    }

    /// <summary>
    /// Reserves orbit slots for belts before planet placement.
    /// </summary>
    public static BeltReservationResult ReserveBeltSlots(
        Array<OrbitHost> orbitHosts,
        Array<OrbitSlot> allSlots,
        Array<CelestialBody> stars,
        SeededRng rng,
        SolarSystemSpec? systemSpec = null)
    {
        PlanetarySystemState planetaryState = PlanetarySystemState.Build(systemSpec, stars);

        BeltReservationResult result = new();
        foreach (OrbitHost host in orbitHosts)
        {
            int beltCount = 0;
            List<OrbitSlot> hostSlots = new();
            foreach (OrbitSlot slot in allSlots)
            {
                if (slot.OrbitHostId == host.NodeId && slot.IsAvailable())
                {
                    hostSlots.Add(slot);
                }
            }

            hostSlots.Sort((left, right) => left.SemiMajorAxisM.CompareTo(right.SemiMajorAxisM));
            foreach (OrbitSlot slot in hostSlots)
            {
                if (beltCount >= MaxBeltsPerHost)
                {
                    break;
                }

                double probability = slot.Zone switch
                {
                    OrbitZone.Zone.Hot => BeltProbabilityHot,
                    OrbitZone.Zone.Cold => BeltProbabilityCold,
                    _ => BeltProbabilityTemperate,
                };
                if (rng.Randf() >= probability)
                {
                    continue;
                }

                AsteroidBelt? belt = CreateBeltAtSlot(slot, host, planetaryState, rng);
                if (belt == null)
                {
                    continue;
                }

                result.Belts.Add(belt);
                result.ReservedSlotIds.Add(slot.Id);
                beltCount += 1;
            }
        }

        return result;
    }

    /// <summary>
    /// Marks reserved slots as filled so planet generation skips them.
    /// </summary>
    public static void MarkReservedSlots(Array<OrbitSlot> slots, Array<string> reservedSlotIds)
    {
        foreach (OrbitSlot slot in slots)
        {
            if (reservedSlotIds.Contains(slot.Id))
            {
                slot.IsFilled = true;
                slot.PlanetId = $"__belt_reserved__{slot.Id}";
            }
        }
    }

    /// <summary>
    /// Clears temporary belt-reservation markers.
    /// </summary>
    public static void ClearReservedSlotMarks(Array<OrbitSlot> slots)
    {
        foreach (OrbitSlot slot in slots)
        {
            if (slot.PlanetId.StartsWith("__belt_reserved__", System.StringComparison.Ordinal))
            {
                slot.IsFilled = false;
                slot.PlanetId = string.Empty;
            }
        }
    }

    /// <summary>
    /// Returns the asteroids associated with a specific belt.
    /// </summary>
    public static Array<CelestialBody> GetAsteroidsForBelt(Array<CelestialBody> asteroids, AsteroidBelt belt)
    {
        Array<CelestialBody> result = new();
        foreach (CelestialBody asteroid in asteroids)
        {
            if (belt.MajorAsteroidIds.Contains(asteroid.Id))
            {
                result.Add(asteroid);
            }
        }

        return result;
    }

    /// <summary>
    /// Sorts asteroids by mass descending.
    /// </summary>
    public static void SortByMass(Array<CelestialBody> asteroids)
    {
        List<CelestialBody> sorted = new();
        foreach (CelestialBody asteroid in asteroids)
        {
            sorted.Add(asteroid);
        }

        sorted.Sort((left, right) => right.Physical.MassKg.CompareTo(left.Physical.MassKg));
        asteroids.Clear();
        foreach (CelestialBody asteroid in sorted)
        {
            asteroids.Add(asteroid);
        }
    }

    /// <summary>
    /// Calculates summary statistics for generated belts.
    /// </summary>
    public static Dictionary GetStatistics(Array<AsteroidBelt> belts, Array<CelestialBody> asteroids)
    {
        int innerBelts = 0;
        int outerBelts = 0;
        int rockyBelts = 0;
        int icyBelts = 0;
        int mixedBelts = 0;
        int metallicBelts = 0;
        double totalBeltMassKg = 0.0;

        foreach (AsteroidBelt belt in belts)
        {
            totalBeltMassKg += belt.TotalMassKg;
            if (belt.Name.Contains("Inner"))
            {
                innerBelts += 1;
            }
            else if (belt.Name.Contains("Outer"))
            {
                outerBelts += 1;
            }

            switch (belt.PrimaryComposition)
            {
                case AsteroidBelt.Composition.Rocky:
                    rockyBelts += 1;
                    break;
                case AsteroidBelt.Composition.Icy:
                    icyBelts += 1;
                    break;
                case AsteroidBelt.Composition.Mixed:
                    mixedBelts += 1;
                    break;
                case AsteroidBelt.Composition.Metallic:
                    metallicBelts += 1;
                    break;
            }
        }

        double avgPerBelt;
        if (belts.Count > 0)
        {
            avgPerBelt = (double)asteroids.Count / belts.Count;
        }
        else
        {
            avgPerBelt = 0.0;
        }

        return new Dictionary
        {
            ["total_belts"] = belts.Count,
            ["total_asteroids"] = asteroids.Count,
            ["inner_belts"] = innerBelts,
            ["outer_belts"] = outerBelts,
            ["rocky_belts"] = rockyBelts,
            ["icy_belts"] = icyBelts,
            ["mixed_belts"] = mixedBelts,
            ["metallic_belts"] = metallicBelts,
            ["total_belt_mass_kg"] = totalBeltMassKg,
            ["avg_asteroids_per_belt"] = avgPerBelt,
        };
    }

    /// <summary>
    /// Validates that belts do not overlap filled planetary slots.
    /// </summary>
    public static bool ValidateBeltPlacement(Array<AsteroidBelt> belts, Array<OrbitSlot> filledSlots)
    {
        foreach (AsteroidBelt belt in belts)
        {
            foreach (OrbitSlot slot in filledSlots)
            {
                if (slot.OrbitHostId != belt.OrbitHostId || !slot.IsFilled)
                {
                    continue;
                }

                if (slot.SemiMajorAxisM >= belt.InnerRadiusM && slot.SemiMajorAxisM <= belt.OuterRadiusM)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Generates belts for a single orbit host.
    /// </summary>
    private static Array<AsteroidBelt> GenerateBeltsForHost(OrbitHost host, Array<OrbitSlot> filledSlots, PlanetarySystemState planetaryState, SeededRng rng)
    {
        Array<double> planetDistances = new();
        foreach (OrbitSlot slot in filledSlots)
        {
            if (slot.OrbitHostId == host.NodeId && slot.IsFilled)
            {
                planetDistances.Add(slot.SemiMajorAxisM);
            }
        }

        planetDistances.Sort();
        Array<AsteroidBelt> belts = new();
        if (!host.HasValidZone())
        {
            return belts;
        }

        double innerBeltProbability = System.Math.Clamp(
            InnerBeltProbability
            * (0.85 + (0.20 * planetaryState.SolidBudgetScalar))
            * GetInnerBeltBiasFactor(planetaryState.Profile.MinorBodyOuterSystemBias),
            0.03,
            0.90);
        if (rng.Randf() < innerBeltProbability)
        {
            double targetCenter = host.FrostLineM * rng.RandfRange(0.7f, 1.1f);
            if (targetCenter >= host.InnerStabilityM && targetCenter <= host.OuterStabilityM)
            {
                double bestGapInner;
                double bestGapOuter;
                double bestScore;

                if (planetDistances.Count == 0)
                {
                    double widthFraction = rng.RandfRange((float)MinBeltWidthFraction, (float)MaxBeltWidthFraction);
                    double halfWidth = targetCenter * widthFraction * 0.5;
                    bestGapInner = System.Math.Max(host.InnerStabilityM, targetCenter - halfWidth);
                    bestGapOuter = System.Math.Min(host.OuterStabilityM, targetCenter + halfWidth);
                    bestScore = 1.0;
                }
                else
                {
                    bestGapInner = host.InnerStabilityM;
                    bestGapOuter = planetDistances[0] * 0.8;
                    double width = bestGapOuter - bestGapInner;
                    double center = (bestGapInner + bestGapOuter) / 2.0;
                    if (width > 0.0)
                    {
                        bestScore = width / (1.0 + (System.Math.Abs(center - targetCenter) / targetCenter));
                    }
                    else
                    {
                        bestScore = -1.0;
                    }

                    for (int index = 0; index < planetDistances.Count - 1; index += 1)
                    {
                        double gapInner = planetDistances[index] * 1.2;
                        double gapOuter = planetDistances[index + 1] * 0.8;
                        if (gapOuter <= gapInner)
                        {
                            continue;
                        }

                        width = gapOuter - gapInner;
                        center = (gapInner + gapOuter) / 2.0;
                        double score = width / (1.0 + (System.Math.Abs(center - targetCenter) / targetCenter));
                        if (score > bestScore)
                        {
                            bestGapInner = gapInner;
                            bestGapOuter = gapOuter;
                            bestScore = score;
                        }
                    }

                    double lastPlanet = planetDistances[planetDistances.Count - 1];
                    double finalGapInner = lastPlanet * 1.2;
                    double finalGapOuter = host.OuterStabilityM;
                    if (finalGapOuter > finalGapInner)
                    {
                        width = finalGapOuter - finalGapInner;
                        center = (finalGapInner + finalGapOuter) / 2.0;
                        double score = width / (1.0 + (System.Math.Abs(center - targetCenter) / targetCenter));
                        if (score > bestScore)
                        {
                            bestGapInner = finalGapInner;
                            bestGapOuter = finalGapOuter;
                            bestScore = score;
                        }
                    }
                }

                double gapWidth = bestGapOuter - bestGapInner;
                double minGapWidth = bestGapInner * MinBeltWidthFraction;
                if (gapWidth >= minGapWidth)
                {
                    double beltCenter = System.Math.Clamp(targetCenter, bestGapInner, bestGapOuter);
                    double widthFraction = rng.RandfRange((float)MinBeltWidthFraction, (float)MaxBeltWidthFraction);
                    double halfWidth = beltCenter * widthFraction * 0.5;
                    AsteroidBelt innerBelt = new($"belt_{host.NodeId}_inner", "Inner Asteroid Belt")
                    {
                        OrbitHostId = host.NodeId,
                        InnerRadiusM = System.Math.Max(bestGapInner, beltCenter - halfWidth),
                        OuterRadiusM = System.Math.Min(bestGapOuter, beltCenter + halfWidth),
                    };

                    double compositionRoll = rng.Randf();
                    if (compositionRoll < 0.50)
                    {
                        innerBelt.PrimaryComposition = AsteroidBelt.Composition.Rocky;
                    }
                    else if (compositionRoll < 0.80)
                    {
                        innerBelt.PrimaryComposition = AsteroidBelt.Composition.Mixed;
                    }
                    else
                    {
                        innerBelt.PrimaryComposition = AsteroidBelt.Composition.Metallic;
                    }

                    if (planetaryState.Profile.MinorBodyOuterSystemBias == PlanetMinorBodyOuterSystemBias.AsteroidLeaning && compositionRoll > 0.65)
                    {
                        innerBelt.PrimaryComposition = AsteroidBelt.Composition.Metallic;
                    }

                    double widthFactor = (innerBelt.OuterRadiusM - innerBelt.InnerRadiusM) / innerBelt.InnerRadiusM;
                    double scale = System.Math.Clamp(widthFactor * 2.0, 0.5, 2.0) * (0.85 + (0.25 * planetaryState.SolidBudgetScalar));
                    double logMin = System.Math.Log(InnerBeltMassMinKg * scale);
                    double logMax = System.Math.Log(InnerBeltMassMaxKg * scale);
                    innerBelt.TotalMassKg = System.Math.Exp(rng.RandfRange((float)logMin, (float)logMax));
                    AnnotateBelt(innerBelt, "main_asteroid_belt", InnerReservoirSourceIds, CompositionSourceIds, InnerSizeSourceIds, "demeo_carry_inner_belt_proxy");
                    belts.Add(innerBelt);
                }
            }
        }

        double outerBeltProbability = System.Math.Clamp(
            OuterBeltProbability
            * (0.80 + (0.25 * planetaryState.OuterReservoirScalar))
            * GetOuterBeltBiasFactor(planetaryState.Profile.MinorBodyOuterSystemBias),
            0.05,
            0.96);
        if (rng.Randf() < outerBeltProbability)
        {
            double minDistance = System.Math.Max(host.FrostLineM * 10.0, host.HabitableZoneOuterM * 18.0);
            if (minDistance <= 0.0)
            {
                minDistance = host.FrostLineM * 8.0;
            }

            double maxDistance = host.OuterStabilityM * 0.85;
            if (minDistance < maxDistance)
            {
                double outermostPlanet;
                if (planetDistances.Count > 0)
                {
                    outermostPlanet = planetDistances[planetDistances.Count - 1];
                }
                else
                {
                    outermostPlanet = 0.0;
                }
                double innerRadius = System.Math.Max(minDistance, outermostPlanet * 1.5);
                if (innerRadius < maxDistance)
                {
                    double widthFraction = rng.RandfRange(0.18f, 0.45f);
                    double outerRadius = System.Math.Min(innerRadius * (1.0 + widthFraction), maxDistance);
                    if (outerRadius > innerRadius)
                    {
                        AsteroidBelt.Composition outerComposition;
                        double outerRoll = rng.Randf();
                        if (outerRoll < 0.70)
                        {
                            outerComposition = AsteroidBelt.Composition.Icy;
                        }
                        else
                        {
                            outerComposition = AsteroidBelt.Composition.Mixed;
                        }

                        if (planetaryState.Profile.MinorBodyOuterSystemBias == PlanetMinorBodyOuterSystemBias.CometLeaning && outerRoll < 0.88)
                        {
                            outerComposition = AsteroidBelt.Composition.Icy;
                        }

                        AsteroidBelt outerBelt = new($"belt_{host.NodeId}_outer", "Outer Asteroid Belt")
                        {
                            OrbitHostId = host.NodeId,
                            InnerRadiusM = innerRadius,
                            OuterRadiusM = outerRadius,
                            PrimaryComposition = outerComposition,
                        };

                        double widthFactor = (outerBelt.OuterRadiusM - outerBelt.InnerRadiusM) / outerBelt.InnerRadiusM;
                        double scale = System.Math.Clamp(widthFactor * 2.0, 0.5, 2.0) * (0.85 + (0.30 * planetaryState.OuterReservoirScalar));
                        double logMin = System.Math.Log(OuterBeltMassMinKg * scale);
                        double logMax = System.Math.Log(OuterBeltMassMaxKg * scale);
                        outerBelt.TotalMassKg = System.Math.Exp(rng.RandfRange((float)logMin, (float)logMax));
                        AnnotateBelt(outerBelt, "trans_neptunian_reservoir", OuterReservoirSourceIds, CompositionSourceIds, OuterSizeSourceIds, "kavelaars_bernardinelli_large_tno_proxy");
                        belts.Add(outerBelt);
                    }
                }
            }
        }

        return belts;
    }

    /// <summary>
    /// Creates a belt centered on a specific orbit slot.
    /// </summary>
    private static AsteroidBelt? CreateBeltAtSlot(OrbitSlot slot, OrbitHost host, PlanetarySystemState planetaryState, SeededRng rng)
    {
        double centerM = slot.SemiMajorAxisM;
        if (centerM <= 0.0)
        {
            return null;
        }

        double widthFraction = rng.RandfRange((float)MinBeltWidthFraction, (float)MaxBeltWidthFraction);
        double halfWidth = centerM * widthFraction * 0.5;
        AsteroidBelt belt = new($"belt_{host.NodeId}_{slot.Id}", slot.Zone switch
        {
            OrbitZone.Zone.Hot => "Inner Debris Belt",
            OrbitZone.Zone.Cold => "Outer Asteroid Belt",
            _ => "Asteroid Belt",
        })
        {
            OrbitHostId = host.NodeId,
            InnerRadiusM = System.Math.Max(0.0, centerM - halfWidth),
            OuterRadiusM = centerM + halfWidth,
        };

        double roll = rng.Randf();
        AsteroidBelt.Composition composition;
        switch (slot.Zone)
        {
            case OrbitZone.Zone.Hot:
                if (roll < 0.50)
                {
                    composition = AsteroidBelt.Composition.Rocky;
                }
                else if (roll < 0.80)
                {
                    composition = AsteroidBelt.Composition.Mixed;
                }
                else
                {
                    composition = AsteroidBelt.Composition.Metallic;
                }

                break;
            case OrbitZone.Zone.Cold:
                double coldIcyThreshold = planetaryState.Profile.MinorBodyOuterSystemBias == PlanetMinorBodyOuterSystemBias.CometLeaning ? 0.88 : 0.70;
                if (roll < coldIcyThreshold)
                {
                    composition = AsteroidBelt.Composition.Icy;
                }
                else
                {
                    composition = AsteroidBelt.Composition.Mixed;
                }

                break;
            default:
                if (roll < 0.40)
                {
                    composition = AsteroidBelt.Composition.Rocky;
                }
                else if (roll < 0.75)
                {
                    composition = AsteroidBelt.Composition.Mixed;
                }
                else
                {
                    composition = AsteroidBelt.Composition.Metallic;
                }

                break;
        }

        belt.PrimaryComposition = composition;
        if (slot.Zone == OrbitZone.Zone.Cold)
        {
            AnnotateBelt(belt, "trans_neptunian_reservoir", OuterReservoirSourceIds, CompositionSourceIds, OuterSizeSourceIds, "kavelaars_bernardinelli_large_tno_proxy");
        }
        else
        {
            AnnotateBelt(belt, "main_asteroid_belt", InnerReservoirSourceIds, CompositionSourceIds, InnerSizeSourceIds, "demeo_carry_inner_belt_proxy");
        }

        bool isOuter = slot.Zone == OrbitZone.Zone.Cold;
        double minMass;
        double maxMass;
        if (isOuter)
        {
            minMass = OuterBeltMassMinKg;
            maxMass = OuterBeltMassMaxKg;
        }
        else
        {
            minMass = InnerBeltMassMinKg;
            maxMass = InnerBeltMassMaxKg;
        }
        double widthFactor = (belt.OuterRadiusM - belt.InnerRadiusM) / System.Math.Max(belt.InnerRadiusM, 1.0);
        double budgetScalar = isOuter ? planetaryState.OuterReservoirScalar : planetaryState.SolidBudgetScalar;
        double scale = System.Math.Clamp(widthFactor * 2.0, 0.5, 2.0) * (0.85 + (0.20 * budgetScalar));
        double logMinMass = System.Math.Log(minMass * scale);
        double logMaxMass = System.Math.Log(maxMass * scale);
        belt.TotalMassKg = System.Math.Exp(rng.RandfRange((float)logMinMass, (float)logMaxMass));
        return belt;
    }

    private static double GetInnerBeltBiasFactor(PlanetMinorBodyOuterSystemBias bias)
    {
        return bias switch
        {
            PlanetMinorBodyOuterSystemBias.AsteroidLeaning => 1.18,
            PlanetMinorBodyOuterSystemBias.CometLeaning => 0.82,
            _ => 1.0,
        };
    }

    private static double GetOuterBeltBiasFactor(PlanetMinorBodyOuterSystemBias bias)
    {
        return bias switch
        {
            PlanetMinorBodyOuterSystemBias.AsteroidLeaning => 0.84,
            PlanetMinorBodyOuterSystemBias.CometLeaning => 1.24,
            _ => 1.0,
        };
    }

    private static void AnnotateBelt(
        AsteroidBelt belt,
        string reservoirKind,
        string reservoirSourceIds,
        string compositionSourceIds,
        string sizeDistributionSourceIds,
        string populationModel)
    {
        belt.ReservoirKind = reservoirKind;
        belt.ReservoirSourceIds = reservoirSourceIds;
        belt.CompositionSourceIds = compositionSourceIds;
        belt.SizeDistributionSourceIds = sizeDistributionSourceIds;
        belt.PopulationModel = populationModel;
        if (string.Equals(reservoirKind, "trans_neptunian_reservoir", System.StringComparison.Ordinal))
        {
            ApplyTransNeptunianReservoirSubfamilies(belt);
        }
        else
        {
            belt.ReservoirSubfamily = "main_belt_proxy";
            belt.ReservoirSubfamilyMix = "main_belt_proxy:1.00";
            belt.ReservoirSubfamilySourceIds = reservoirSourceIds;
        }
    }

    private static void ApplyTransNeptunianReservoirSubfamilies(AsteroidBelt belt)
    {
        double innerAu = belt.InnerRadiusM / Units.AuMeters;
        double outerAu = belt.OuterRadiusM / Units.AuMeters;
        double centerAu = belt.GetCenterAu();
        double widthAu = belt.GetWidthAu();
        TnoSubfamilyWeights weights = ResolveTransNeptunianSubfamilyWeights(innerAu, outerAu, centerAu, widthAu);

        belt.ReservoirSubfamily = SelectDominantTransNeptunianSubfamily(weights);
        belt.ReservoirSubfamilyMix = FormatTransNeptunianSubfamilyMix(weights);
        belt.ReservoirSubfamilySourceIds = OuterSubfamilySourceIds;
    }

    private static TnoSubfamilyWeights ResolveTransNeptunianSubfamilyWeights(double innerAu, double outerAu, double centerAu, double widthAu)
    {
        TnoSubfamilyWeights weights = new();
        if (centerAu < 35.0)
        {
            weights.ColdClassical = 0.04;
            weights.HotClassical = 0.18;
            weights.Resonant = 0.28;
            weights.Scattered = 0.06;
            weights.Centaur = 0.22;
            weights.CometFeeding = 0.22;
            return weights;
        }

        if (innerAu >= 41.0 && outerAu <= 49.5 && widthAu <= 12.0)
        {
            weights.ColdClassical = 0.32;
            weights.HotClassical = 0.30;
            weights.Resonant = 0.20;
            weights.Scattered = 0.08;
            weights.Centaur = 0.03;
            weights.CometFeeding = 0.07;
            return weights;
        }

        if (centerAu <= 50.0)
        {
            weights.ColdClassical = 0.14;
            weights.HotClassical = 0.38;
            weights.Resonant = 0.26;
            weights.Scattered = 0.12;
            weights.Centaur = 0.04;
            weights.CometFeeding = 0.06;
            return weights;
        }

        weights.ColdClassical = 0.04;
        weights.HotClassical = 0.16;
        weights.Resonant = 0.14;
        weights.Scattered = 0.38;
        weights.Centaur = 0.10;
        weights.CometFeeding = 0.18;
        return weights;
    }

    private static string SelectDominantTransNeptunianSubfamily(TnoSubfamilyWeights weights)
    {
        string dominant = "cold_classical_tno_proxy";
        double value = weights.ColdClassical;
        if (weights.HotClassical > value)
        {
            dominant = "hot_classical_tno_proxy";
            value = weights.HotClassical;
        }

        if (weights.Resonant > value)
        {
            dominant = "resonant_tno_proxy";
            value = weights.Resonant;
        }

        if (weights.Scattered > value)
        {
            dominant = "scattered_tno_proxy";
            value = weights.Scattered;
        }

        if (weights.Centaur > value)
        {
            dominant = "centaur_proxy";
            value = weights.Centaur;
        }

        if (weights.CometFeeding > value)
        {
            dominant = "comet_feeding_proxy";
        }

        return dominant;
    }

    private static string FormatTransNeptunianSubfamilyMix(TnoSubfamilyWeights weights)
    {
        return string.Join(
            ";",
            "cold_classical:" + weights.ColdClassical.ToString("0.00", CultureInfo.InvariantCulture),
            "hot_classical:" + weights.HotClassical.ToString("0.00", CultureInfo.InvariantCulture),
            "resonant:" + weights.Resonant.ToString("0.00", CultureInfo.InvariantCulture),
            "scattered:" + weights.Scattered.ToString("0.00", CultureInfo.InvariantCulture),
            "centaur:" + weights.Centaur.ToString("0.00", CultureInfo.InvariantCulture),
            "comet_feeding:" + weights.CometFeeding.ToString("0.00", CultureInfo.InvariantCulture));
    }

    private static void AddReservoirRecordsForBelt(Array<SmallBodyReservoir> reservoirs, AsteroidBelt belt)
    {
        if (string.Equals(belt.ReservoirKind, "trans_neptunian_reservoir", System.StringComparison.Ordinal))
        {
            AddTransNeptunianReservoirRecords(reservoirs, belt);
            return;
        }

        reservoirs.Add(CreateReservoirRecord(
            belt,
            "main_belt",
            "Main Belt",
            1.0,
            belt.ReservoirSourceIds,
            belt.PopulationModel));
    }

    private static void AddTransNeptunianReservoirRecords(Array<SmallBodyReservoir> reservoirs, AsteroidBelt belt)
    {
        AddReservoirRecordFromMix(reservoirs, belt, "cold_classical", "Cold Classical TNO");
        AddReservoirRecordFromMix(reservoirs, belt, "hot_classical", "Hot Classical TNO");
        AddReservoirRecordFromMix(reservoirs, belt, "resonant", "Resonant TNO");
        AddReservoirRecordFromMix(reservoirs, belt, "scattered", "Scattered TNO");
        AddReservoirRecordFromMix(reservoirs, belt, "centaur", "Centaur");
        AddReservoirRecordFromMix(reservoirs, belt, "comet_feeding", "Comet-Feeding");
    }

    private static void AddReservoirRecordFromMix(Array<SmallBodyReservoir> reservoirs, AsteroidBelt belt, string family, string displayFamily)
    {
        double weight = ResolveSubfamilyWeight(belt.ReservoirSubfamilyMix, family);
        if (weight <= 0.0)
        {
            return;
        }

        reservoirs.Add(CreateReservoirRecord(
            belt,
            family,
            displayFamily,
            weight,
            belt.ReservoirSubfamilySourceIds,
            "tno_subfamily_diagnostic_proxy"));
    }

    private static SmallBodyReservoir CreateReservoirRecord(
        AsteroidBelt belt,
        string family,
        string displayFamily,
        double relativeWeight,
        string sourceIds,
        string populationModel)
    {
        SmallBodyReservoir reservoir = new($"reservoir_{belt.Id}_{family}", $"{displayFamily} Reservoir")
        {
            OrbitHostId = belt.OrbitHostId,
            AnchorBeltId = belt.Id,
            ReservoirKind = belt.ReservoirKind,
            ReservoirFamily = family,
            RelativeWeight = relativeWeight,
            InnerRadiusM = belt.InnerRadiusM,
            OuterRadiusM = belt.OuterRadiusM,
            SourceIds = sourceIds,
            PopulationModel = populationModel,
            RepresentationStatus = "diagnostic_proxy",
        };

        return reservoir;
    }

    private static double ResolveSubfamilyWeight(string mix, string family)
    {
        string[] entries = mix.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(':', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            if (!string.Equals(parts[0], family, System.StringComparison.Ordinal))
            {
                continue;
            }

            if (double.TryParse(parts[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
            {
                return parsed;
            }
        }

        return 0.0;
    }

    /// <summary>
    /// Generates the major asteroids tracked for a belt.
    /// </summary>
    private static Array<CelestialBody> GenerateMajorAsteroids(
        AsteroidBelt belt,
        OrbitHost host,
        Array<CelestialBody> stars,
        PlanetarySystemState planetaryState,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings)
    {
        Array<CelestialBody> asteroids = new();
        int count = rng.RandiRange(3, MaxMajorAsteroids);
        double stellarMassKg = host.CombinedMassKg;
        double stellarLuminosityWatts = host.CombinedLuminosityWatts;
        double stellarTemperatureK = host.EffectiveTemperatureK;
        double stellarAgeYears = 4.6e9;

        if (stars.Count > 0 && stars[0].HasStellar())
        {
            stellarAgeYears = stars[0].Stellar!.AgeYears;
        }

        List<double> diametersKm = new();
        double alpha = ResolveMajorBodyPowerLawAlpha(belt, planetaryState);
        for (int index = 0; index < count; index += 1)
        {
            double maxDiameterKm = ResolveLargeObjectMaxDiameterKm(belt);
            double lower = System.Math.Pow(LargeObjectDiameterThresholdKm, 1.0 - alpha);
            double upper = System.Math.Pow(maxDiameterKm, 1.0 - alpha);
            double u = rng.Randf();
            double diameterKm = System.Math.Pow(
                lower + (u * (upper - lower)),
                1.0 / (1.0 - alpha));
            diametersKm.Add(diameterKm);
        }

        diametersKm.Sort((left, right) => right.CompareTo(left));
        for (int index = 0; index < diametersKm.Count; index += 1)
        {
            CelestialBody? asteroid = GenerateSingleMajorAsteroid(
                belt,
                host,
                stellarMassKg,
                stellarLuminosityWatts,
                stellarTemperatureK,
                stellarAgeYears,
                diametersKm[index],
                planetaryState,
                index,
                rng,
                useCaseSettings);
            if (asteroid != null)
            {
                asteroids.Add(asteroid);
            }
        }

        return asteroids;
    }

    private static double ResolveLargeObjectMaxDiameterKm(AsteroidBelt belt)
    {
        if (string.Equals(belt.ReservoirKind, "trans_neptunian_reservoir", System.StringComparison.Ordinal))
        {
            return OuterLargeObjectMaxDiameterKm;
        }

        return InnerLargeObjectMaxDiameterKm;
    }

    /// <summary>
    /// Generates a single representative major asteroid.
    /// </summary>
    private static CelestialBody? GenerateSingleMajorAsteroid(
        AsteroidBelt belt,
        OrbitHost host,
        double stellarMassKg,
        double stellarLuminosityWatts,
        double stellarTemperatureK,
        double stellarAgeYears,
        double diameterKm,
        PlanetarySystemState planetaryState,
        int asteroidIndex,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings)
    {
        double distanceFraction = rng.RandfRange(0.1f, 0.9f);
        double orbitalDistance = belt.InnerRadiusM + ((belt.OuterRadiusM - belt.InnerRadiusM) * distanceFraction);

        int asteroidType = SelectAsteroidTypeForBelt(belt, host, orbitalDistance, planetaryState, rng);

        int asteroidSeed = unchecked((int)rng.Randi());
        AsteroidSpec spec;
        if (asteroidIndex == 0)
        {
            spec = AsteroidSpec.CeresLike(asteroidSeed);
            spec.AsteroidType = asteroidType;
            if (useCaseSettings != null)
            {
                spec.UseCaseSettings = useCaseSettings.Clone();
            }
            else
            {
                spec.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            }
        }
        else
        {
            spec = new AsteroidSpec(asteroidSeed, asteroidType, useCaseSettings: useCaseSettings);
            spec.IsLarge = true;
        }

        double radiusM = diameterKm * 500.0;
        double densityKgM3 = ResolveRepresentativeDensityKgM3((AsteroidType.Type)asteroidType);
        double volumeM3 = (4.0 / 3.0) * System.Math.PI * radiusM * radiusM * radiusM;
        double massKg = volumeM3 * densityKgM3;

        spec.SetOverride("physical.radius_m", radiusM);
        spec.SetOverride("physical.density_kg_m3", densityKgM3);
        spec.SetOverride("physical.mass_kg", massKg);
        spec.SetOverride("orbital.semi_major_axis_m", orbitalDistance);

        ParentContext context = ParentContext.ForPlanet(
            stellarMassKg,
            stellarLuminosityWatts,
            stellarTemperatureK,
            stellarAgeYears,
            orbitalDistance,
            planetaryState.Profile.HabitableZoneModel);

        SeededRng asteroidRng = new(asteroidSeed);
        CelestialBody asteroid = AsteroidGenerator.Generate(spec, context, asteroidRng);
        asteroid.Id = $"asteroid_{belt.Id}_{asteroidIndex}";
        if (belt.Name.Contains("Inner"))
        {
            asteroid.Name = $"{asteroidIndex + 1} {belt.Name.Replace("Inner Asteroid Belt", "Ceres-family")}";
        }
        else if (belt.Name.Contains("Outer"))
        {
            asteroid.Name = $"{asteroidIndex + 1} {belt.Name.Replace("Outer Asteroid Belt", "TNO")}";
        }
        else
        {
            asteroid.Name = $"{asteroidIndex + 1} {belt.Id}";
        }

        if (asteroid.HasOrbital())
        {
            asteroid.Orbital!.ParentId = host.NodeId;
        }

        asteroid.SetMeta("small_body_reservoir_kind", belt.ReservoirKind);
        asteroid.SetMeta("small_body_source_ids", planetaryState.SmallBodySourceIds);
        asteroid.SetMeta("belt_reservoir_source_ids", belt.ReservoirSourceIds);
        asteroid.SetMeta("belt_reservoir_subfamily", belt.ReservoirSubfamily);
        asteroid.SetMeta("belt_reservoir_subfamily_mix", belt.ReservoirSubfamilyMix);
        asteroid.SetMeta("belt_reservoir_subfamily_source_ids", belt.ReservoirSubfamilySourceIds);
        asteroid.SetMeta("belt_composition_source_ids", belt.CompositionSourceIds);
        asteroid.SetMeta("belt_size_distribution_source_ids", belt.SizeDistributionSourceIds);
        asteroid.SetMeta("minor_body_population_slope", ResolveMajorBodyPowerLawAlpha(belt, planetaryState));
        asteroid.SetMeta("belt_id", belt.Id);
        asteroid.SetMeta("major_body_diameter_km", diameterKm);
        asteroid.SetMeta("major_body_size_semantics", "diameter_km");
        asteroid.SetMeta("belt_population_readiness", "native_life_absent_station_or_habitat_settlement_followup");
        return asteroid;
    }

    private static double ResolveRepresentativeDensityKgM3(AsteroidType.Type asteroidType)
    {
        switch (asteroidType)
        {
            case AsteroidType.Type.CType:
                return 1600.0;
            case AsteroidType.Type.SType:
                return 2700.0;
            case AsteroidType.Type.MType:
                return 5200.0;
            case AsteroidType.Type.DType:
                return 1000.0;
            case AsteroidType.Type.VType:
                return 3000.0;
            default:
                return 2000.0;
        }
    }

    private static double ResolveMajorBodyPowerLawAlpha(AsteroidBelt belt, PlanetarySystemState planetaryState)
    {
        double slope = System.Math.Clamp(planetaryState.Profile.MinorBodyPopulationSlope, 1.0, 5.0);
        if (string.Equals(belt.ReservoirKind, "trans_neptunian_reservoir", System.StringComparison.Ordinal))
        {
            return System.Math.Clamp(2.15 + (0.22 * slope), 1.8, 3.4);
        }

        return System.Math.Clamp(2.0 + (0.25 * slope), 1.8, 3.6);
    }

    private static int SelectAsteroidTypeForBelt(
        AsteroidBelt belt,
        OrbitHost host,
        double orbitalDistanceM,
        PlanetarySystemState planetaryState,
        SeededRng rng)
    {
        double compositionRoll = rng.Randf();
        double snowLineM = System.Math.Max(host.FrostLineM, 1.0);
        double snowLineRatio = orbitalDistanceM / snowLineM;

        if (string.Equals(belt.ReservoirKind, "trans_neptunian_reservoir", System.StringComparison.Ordinal))
        {
            double dTypeThreshold = planetaryState.Profile.MinorBodyOuterSystemBias == PlanetMinorBodyOuterSystemBias.CometLeaning ? 0.34 : 0.18;
            if (compositionRoll < dTypeThreshold)
            {
                return (int)AsteroidType.Type.DType;
            }

            return (int)AsteroidType.Type.CType;
        }

        if (snowLineRatio < 0.92)
        {
            if (compositionRoll < 0.74)
            {
                return (int)AsteroidType.Type.SType;
            }

            if (compositionRoll < 0.88)
            {
                return (int)AsteroidType.Type.CType;
            }

            return compositionRoll > 0.97 ? (int)AsteroidType.Type.VType : (int)AsteroidType.Type.MType;
        }

        if (snowLineRatio < 1.35)
        {
            if (compositionRoll < 0.46)
            {
                return (int)AsteroidType.Type.CType;
            }

            if (compositionRoll < 0.78)
            {
                return (int)AsteroidType.Type.SType;
            }

            return compositionRoll > 0.94 ? (int)AsteroidType.Type.VType : (int)AsteroidType.Type.MType;
        }

        if (compositionRoll < 0.68)
        {
            return (int)AsteroidType.Type.CType;
        }

        if (compositionRoll < 0.86)
        {
            return (int)AsteroidType.Type.DType;
        }

        return (int)AsteroidType.Type.SType;
    }

    private sealed class TnoSubfamilyWeights
    {
        public double ColdClassical;
        public double HotClassical;
        public double Resonant;
        public double Scattered;
        public double Centaur;
        public double CometFeeding;
    }
}
