using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;

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
    private const double OuterBeltMassMinKg = 1.0e21;
    private const double OuterBeltMassMaxKg = 1.0e24;
    private const double MajorAsteroidThresholdKm = 100.0;
    private const double PowerLawAlpha = 2.5;

    /// <summary>
    /// Generates belts and their major asteroids for a system.
    /// </summary>
    public static BeltGenerationResult Generate(
        Array<OrbitHost> orbitHosts,
        Array<OrbitSlot> filledSlots,
        Array<CelestialBody> stars,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        BeltGenerationResult result = new();

        foreach (OrbitHost host in orbitHosts)
        {
            Array<AsteroidBelt> hostBelts = GenerateBeltsForHost(host, filledSlots, rng);
            foreach (AsteroidBelt belt in hostBelts)
            {
                result.Belts.Add(belt);
                Array<CelestialBody> beltAsteroids = GenerateMajorAsteroids(belt, host, stars, rng, useCaseSettings);
                Array<string> asteroidIds = new();

                foreach (CelestialBody asteroid in beltAsteroids)
                {
                    result.Asteroids.Add(asteroid);
                    asteroidIds.Add(asteroid.Id);
                }

                belt.MajorAsteroidIds = asteroidIds;
                result.BeltAsteroidMap[belt.Id] = asteroidIds;
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
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        BeltGenerationResult result = new();
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

            Array<CelestialBody> beltAsteroids = GenerateMajorAsteroids(belt, hostsById[belt.OrbitHostId], stars, rng, useCaseSettings);
            Array<string> asteroidIds = new();
            foreach (CelestialBody asteroid in beltAsteroids)
            {
                result.Asteroids.Add(asteroid);
                asteroidIds.Add(asteroid.Id);
            }

            belt.MajorAsteroidIds = asteroidIds;
            result.BeltAsteroidMap[belt.Id] = asteroidIds;
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
        SeededRng rng)
    {
        _ = stars;

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

                AsteroidBelt? belt = CreateBeltAtSlot(slot, host, rng);
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
    private static Array<AsteroidBelt> GenerateBeltsForHost(OrbitHost host, Array<OrbitSlot> filledSlots, SeededRng rng)
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

        if (rng.Randf() < InnerBeltProbability)
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

                    double widthFactor = (innerBelt.OuterRadiusM - innerBelt.InnerRadiusM) / innerBelt.InnerRadiusM;
                    double scale = System.Math.Clamp(widthFactor * 2.0, 0.5, 2.0);
                    double logMin = System.Math.Log(InnerBeltMassMinKg * scale);
                    double logMax = System.Math.Log(InnerBeltMassMaxKg * scale);
                    innerBelt.TotalMassKg = System.Math.Exp(rng.RandfRange((float)logMin, (float)logMax));
                    belts.Add(innerBelt);
                }
            }
        }

        if (rng.Randf() < OuterBeltProbability)
        {
            double minDistance = host.FrostLineM * 5.0;
            double maxDistance = host.OuterStabilityM * 0.8;
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
                    double widthFraction = rng.RandfRange((float)MinBeltWidthFraction, (float)MaxBeltWidthFraction);
                    double outerRadius = System.Math.Min(innerRadius * (1.0 + widthFraction), maxDistance);
                    if (outerRadius > innerRadius)
                    {
                        AsteroidBelt.Composition outerComposition;
                        if (rng.Randf() < 0.70)
                        {
                            outerComposition = AsteroidBelt.Composition.Icy;
                        }
                        else
                        {
                            outerComposition = AsteroidBelt.Composition.Mixed;
                        }

                        AsteroidBelt outerBelt = new($"belt_{host.NodeId}_outer", "Outer Asteroid Belt")
                        {
                            OrbitHostId = host.NodeId,
                            InnerRadiusM = innerRadius,
                            OuterRadiusM = outerRadius,
                            PrimaryComposition = outerComposition,
                        };

                        double widthFactor = (outerBelt.OuterRadiusM - outerBelt.InnerRadiusM) / outerBelt.InnerRadiusM;
                        double scale = System.Math.Clamp(widthFactor * 2.0, 0.5, 2.0);
                        double logMin = System.Math.Log(OuterBeltMassMinKg * scale);
                        double logMax = System.Math.Log(OuterBeltMassMaxKg * scale);
                        outerBelt.TotalMassKg = System.Math.Exp(rng.RandfRange((float)logMin, (float)logMax));
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
    private static AsteroidBelt? CreateBeltAtSlot(OrbitSlot slot, OrbitHost host, SeededRng rng)
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
                if (roll < 0.70)
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
        double scale = System.Math.Clamp(widthFactor * 2.0, 0.5, 2.0);
        double logMinMass = System.Math.Log(minMass * scale);
        double logMaxMass = System.Math.Log(maxMass * scale);
        belt.TotalMassKg = System.Math.Exp(rng.RandfRange((float)logMinMass, (float)logMaxMass));
        return belt;
    }

    /// <summary>
    /// Generates the major asteroids tracked for a belt.
    /// </summary>
    private static Array<CelestialBody> GenerateMajorAsteroids(
        AsteroidBelt belt,
        OrbitHost host,
        Array<CelestialBody> stars,
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

        List<double> sizesKm = new();
        for (int index = 0; index < count; index += 1)
        {
            double maxSizeKm = 1000.0;
            double minSizeKm = MajorAsteroidThresholdKm;
            double lower = System.Math.Pow(minSizeKm, 1.0 - PowerLawAlpha);
            double upper = System.Math.Pow(maxSizeKm, 1.0 - PowerLawAlpha);
            double u = rng.Randf();
            double sizeKm = System.Math.Pow(
                lower + (u * (upper - lower)),
                1.0 / (1.0 - PowerLawAlpha));
            sizesKm.Add(sizeKm);
        }

        sizesKm.Sort((left, right) => right.CompareTo(left));
        for (int index = 0; index < sizesKm.Count; index += 1)
        {
            CelestialBody? asteroid = GenerateSingleMajorAsteroid(
                belt,
                host,
                stellarMassKg,
                stellarLuminosityWatts,
                stellarTemperatureK,
                stellarAgeYears,
                sizesKm[index],
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
        double sizeKm,
        int asteroidIndex,
        SeededRng rng,
        GenerationUseCaseSettings? useCaseSettings)
    {
        double distanceFraction = rng.RandfRange(0.1f, 0.9f);
        double orbitalDistance = belt.InnerRadiusM + ((belt.OuterRadiusM - belt.InnerRadiusM) * distanceFraction);

        double compositionRoll = rng.Randf();
        int asteroidType;
        switch (belt.PrimaryComposition)
        {
            case AsteroidBelt.Composition.Rocky:
                if (compositionRoll < 0.75)
                {
                    asteroidType = (int)AsteroidType.Type.SType;
                }
                else
                {
                    asteroidType = (int)AsteroidType.Type.CType;
                }

                break;
            case AsteroidBelt.Composition.Icy:
                asteroidType = (int)AsteroidType.Type.CType;
                break;
            case AsteroidBelt.Composition.Metallic:
                if (compositionRoll < 0.60)
                {
                    asteroidType = (int)AsteroidType.Type.MType;
                }
                else
                {
                    asteroidType = (int)AsteroidType.Type.SType;
                }

                break;
            case AsteroidBelt.Composition.Mixed:
                if (compositionRoll < 0.50)
                {
                    asteroidType = (int)AsteroidType.Type.CType;
                }
                else if (compositionRoll < 0.85)
                {
                    asteroidType = (int)AsteroidType.Type.SType;
                }
                else
                {
                    asteroidType = (int)AsteroidType.Type.MType;
                }

                break;
            default:
                asteroidType = (int)AsteroidType.Type.CType;
                break;
        }

        int asteroidSeed = unchecked((int)rng.Randi());
        AsteroidSpec spec;
        if (asteroidIndex == 0 && sizeKm >= 400.0)
        {
            spec = AsteroidSpec.CeresLike(asteroidSeed);
            spec.AsteroidType = asteroidType;
            spec.UseCaseSettings = useCaseSettings?.Clone() ?? GenerationUseCaseSettings.CreateDefault();
        }
        else
        {
            spec = new AsteroidSpec(asteroidSeed, asteroidType, useCaseSettings: useCaseSettings);
            spec.IsLarge = sizeKm >= 400.0;
        }

        spec.SetOverride("physical.radius_m", sizeKm * 1000.0);
        spec.SetOverride("orbital.semi_major_axis_m", orbitalDistance);

        ParentContext context = ParentContext.ForPlanet(
            stellarMassKg,
            stellarLuminosityWatts,
            stellarTemperatureK,
            stellarAgeYears,
            orbitalDistance);

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

        return asteroid;
    }
}
