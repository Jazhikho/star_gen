using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Systems;

/// <summary>
/// Generates planets for a solar system by filling orbit slots.
/// </summary>
public static class SystemPlanetGenerator
{
    // Petigura et al. (2013), Fulton et al. (2017), and Fernandes et al. (2019) support
    // different size-class prevalence across hot, temperate, and cold orbital regimes.
    // Tuning: the zone tables below are StarGen deterministic weights shaped by that
    // observational framework, not literal occurrence-rate tables.
    private static readonly System.Collections.Generic.Dictionary<SizeCategory.Category, float> HotZoneWeights = new()
    {
        [SizeCategory.Category.Dwarf] = 5.0f,
        [SizeCategory.Category.SubTerrestrial] = 15.0f,
        [SizeCategory.Category.Terrestrial] = 25.0f,
        [SizeCategory.Category.SuperEarth] = 30.0f,
        [SizeCategory.Category.MiniNeptune] = 15.0f,
        [SizeCategory.Category.NeptuneClass] = 5.0f,
        [SizeCategory.Category.GasGiant] = 5.0f,
    };

    private static readonly System.Collections.Generic.Dictionary<SizeCategory.Category, float> TemperateZoneWeights = new()
    {
        [SizeCategory.Category.Dwarf] = 8.0f,
        [SizeCategory.Category.SubTerrestrial] = 18.0f,
        [SizeCategory.Category.Terrestrial] = 25.0f,
        [SizeCategory.Category.SuperEarth] = 20.0f,
        [SizeCategory.Category.MiniNeptune] = 12.0f,
        [SizeCategory.Category.NeptuneClass] = 10.0f,
        [SizeCategory.Category.GasGiant] = 7.0f,
    };

    private static readonly System.Collections.Generic.Dictionary<SizeCategory.Category, float> ColdZoneWeights = new()
    {
        [SizeCategory.Category.Dwarf] = 10.0f,
        [SizeCategory.Category.SubTerrestrial] = 8.0f,
        [SizeCategory.Category.Terrestrial] = 5.0f,
        [SizeCategory.Category.SuperEarth] = 7.0f,
        [SizeCategory.Category.MiniNeptune] = 15.0f,
        [SizeCategory.Category.NeptuneClass] = 25.0f,
        [SizeCategory.Category.GasGiant] = 30.0f,
    };

    /// <summary>
    /// Generates planets for a set of orbit slots.
    /// </summary>
    public static PlanetGenerationResult Generate(
        Array<OrbitSlot> slots,
        Array<OrbitHost> orbitHosts,
        Array<CelestialBody> stars,
        SeededRng rng,
        bool enablePopulation = false,
        GenerationUseCaseSettings? useCaseSettings = null,
        SolarSystemSpec? systemSpec = null)
    {
        PlanetGenerationResult result = new()
        {
            Slots = CloneSlots(slots),
        };
        System.Collections.Generic.Dictionary<string, OrbitHost> hostMap = BuildHostMap(orbitHosts);
        PlanetarySystemState planetaryState = PlanetarySystemState.Build(systemSpec, stars);
        RpgCompatibilityProfile compatibilityProfile = useCaseSettings?.GetCompatibilityProfile()
            ?? RpgCompatibilityProfile.Resolve(GenerationUseCaseSettings.RulesetModeType.Default);
        string preferredMainworldSlotId = SelectPreferredMainworldSlotId(result.Slots, planetaryState, useCaseSettings, compatibilityProfile);

        foreach (OrbitSlot slot in result.Slots)
        {
            if (!slot.IsAvailable())
            {
                continue;
            }

            bool targetMainworldSlot = slot.Id == preferredMainworldSlotId;
            if (!ShouldFillSlot(slot, rng, planetaryState, compatibilityProfile, targetMainworldSlot))
            {
                continue;
            }

            if (!hostMap.ContainsKey(slot.OrbitHostId))
            {
                continue;
            }

            CelestialBody? planet = GeneratePlanetForSlot(
                slot,
                hostMap[slot.OrbitHostId],
                stars,
                rng,
                enablePopulation,
                useCaseSettings,
                planetaryState,
                compatibilityProfile,
                targetMainworldSlot);
            if (planet != null)
            {
                result.Planets.Add(planet);
                slot.FillWithPlanet(planet.Id);
            }
        }

        result.Success = true;
        return result;
    }

    /// <summary>
    /// Generates planets until a target count is reached or no slots remain.
    /// </summary>
    public static PlanetGenerationResult GenerateTargeted(
        Array<OrbitSlot> slots,
        Array<OrbitHost> orbitHosts,
        Array<CelestialBody> stars,
        int targetCount,
        SeededRng rng,
        bool enablePopulation = false,
        GenerationUseCaseSettings? useCaseSettings = null,
        SolarSystemSpec? systemSpec = null)
    {
        PlanetGenerationResult result = new()
        {
            Slots = CloneSlots(slots),
        };
        System.Collections.Generic.Dictionary<string, OrbitHost> hostMap = BuildHostMap(orbitHosts);
        PlanetarySystemState planetaryState = PlanetarySystemState.Build(systemSpec, stars);
        RpgCompatibilityProfile compatibilityProfile = useCaseSettings?.GetCompatibilityProfile()
            ?? RpgCompatibilityProfile.Resolve(GenerationUseCaseSettings.RulesetModeType.Default);
        Array<OrbitSlot> availableSlots = new();
        System.Collections.Generic.Dictionary<OrbitSlot, double> slotScores = new();
        string preferredMainworldSlotId = SelectPreferredMainworldSlotId(result.Slots, planetaryState, useCaseSettings, compatibilityProfile);

        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.IsAvailable())
            {
                availableSlots.Add(slot);
                slotScores[slot] = GetSlotOrderingScore(slot, planetaryState, compatibilityProfile, slot.Id == preferredMainworldSlotId) + (rng.Randf() * 0.3);
            }
        }

        SortSlotsByScore(availableSlots, slotScores);

        int planetIndex = 0;
        foreach (OrbitSlot slot in availableSlots)
        {
            if (planetIndex >= targetCount)
            {
                break;
            }

            if (!hostMap.ContainsKey(slot.OrbitHostId))
            {
                continue;
            }

            bool targetMainworldSlot = slot.Id == preferredMainworldSlotId;
            CelestialBody? planet = GeneratePlanetForSlot(
                slot,
                hostMap[slot.OrbitHostId],
                stars,
                rng,
                enablePopulation,
                useCaseSettings,
                planetaryState,
                compatibilityProfile,
                targetMainworldSlot);
            if (planet != null)
            {
                result.Planets.Add(planet);
                slot.FillWithPlanet(planet.Id);
                planetIndex += 1;
            }
        }

        result.Success = true;
        return result;
    }

    /// <summary>
    /// Returns statistics about a set of generated planets.
    /// </summary>
    public static Dictionary GetStatistics(Array<CelestialBody> planets)
    {
        Dictionary stats = new()
        {
            ["total"] = planets.Count,
            ["rocky"] = 0,
            ["gaseous"] = 0,
            ["has_atmosphere"] = 0,
            ["has_rings"] = 0,
            ["min_mass_earth"] = 0.0,
            ["max_mass_earth"] = 0.0,
            ["avg_mass_earth"] = 0.0,
        };

        if (planets.Count == 0)
        {
            return stats;
        }

        int rockyCount = 0;
        int gaseousCount = 0;
        int atmosphereCount = 0;
        int ringCount = 0;
        double massSumEarth = 0.0;
        double minMassKg = planets[0].Physical.MassKg;
        double maxMassKg = planets[0].Physical.MassKg;

        foreach (CelestialBody planet in planets)
        {
            double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
            massSumEarth += massEarth;
            minMassKg = System.Math.Min(minMassKg, planet.Physical.MassKg);
            maxMassKg = System.Math.Max(maxMassKg, planet.Physical.MassKg);

            if (massEarth < 10.0)
            {
                rockyCount += 1;
            }
            else
            {
                gaseousCount += 1;
            }

            if (planet.HasAtmosphere())
            {
                atmosphereCount += 1;
            }

            if (planet.HasRingSystem())
            {
                ringCount += 1;
            }
        }

        stats["rocky"] = rockyCount;
        stats["gaseous"] = gaseousCount;
        stats["has_atmosphere"] = atmosphereCount;
        stats["has_rings"] = ringCount;
        stats["min_mass_earth"] = minMassKg / Units.EarthMassKg;
        stats["max_mass_earth"] = maxMassKg / Units.EarthMassKg;
        stats["avg_mass_earth"] = massSumEarth / planets.Count;
        return stats;
    }

    /// <summary>
    /// Returns planets in a specified zone.
    /// </summary>
    public static Array<CelestialBody> FilterByZone(
        Array<CelestialBody> planets,
        Array<OrbitSlot> slots,
        OrbitZone.Zone zone)
    {
        Array<CelestialBody> result = new();
        System.Collections.Generic.Dictionary<string, OrbitSlot> planetToSlot = new();
        foreach (OrbitSlot slot in slots)
        {
            if (slot.IsFilled && !string.IsNullOrEmpty(slot.PlanetId))
            {
                planetToSlot[slot.PlanetId] = slot;
            }
        }

        foreach (CelestialBody planet in planets)
        {
            if (planetToSlot.ContainsKey(planet.Id) && planetToSlot[planet.Id].Zone == zone)
            {
                result.Add(planet);
            }
        }

        return result;
    }

    /// <summary>
    /// Sorts planets by orbital distance.
    /// </summary>
    public static void SortByDistance(Array<CelestialBody> planets)
    {
        List<CelestialBody> sorted = new();
        foreach (CelestialBody planet in planets)
        {
            sorted.Add(planet);
        }

        sorted.Sort((left, right) =>
        {
            bool leftHas = left.HasOrbital();
            bool rightHas = right.HasOrbital();
            if (leftHas && rightHas)
            {
                return left.Orbital!.SemiMajorAxisM.CompareTo(right.Orbital!.SemiMajorAxisM);
            }

            if (leftHas && !rightHas)
            {
                return -1;
            }

            if (!leftHas && rightHas)
            {
                return 1;
            }

            return 0;
        });

        planets.Clear();
        foreach (CelestialBody planet in sorted)
        {
            planets.Add(planet);
        }
    }

    /// <summary>
    /// Sorts planets by mass descending.
    /// </summary>
    public static void SortByMass(Array<CelestialBody> planets)
    {
        List<CelestialBody> sorted = new();
        foreach (CelestialBody planet in planets)
        {
            sorted.Add(planet);
        }

        sorted.Sort((left, right) => right.Physical.MassKg.CompareTo(left.Physical.MassKg));
        planets.Clear();
        foreach (CelestialBody planet in sorted)
        {
            planets.Add(planet);
        }
    }

    /// <summary>
    /// Returns planets that can plausibly host moons.
    /// </summary>
    public static Array<CelestialBody> GetMoonCandidates(Array<CelestialBody> planets)
    {
        Array<CelestialBody> result = new();
        foreach (CelestialBody planet in planets)
        {
            double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
            // Ronnet and Johansen (2020), Sasaki et al. (2010), and Szulagyi et al. (2018)
            // support treating large-planet moon systems as architecture-dependent outcomes
            // rather than equally likely around arbitrarily tiny primaries. Tuning: the
            // `0.1 Earth mass` cutoff below is StarGen's lightweight moon-host floor.
            if (massEarth >= 0.1)
            {
                result.Add(planet);
            }
        }

        return result;
    }

    /// <summary>
    /// Assigns Roman numeral names to planets in order.
    /// </summary>
    public static void AssignRomanNumeralNames(Array<CelestialBody> planets, string systemName = "")
    {
        string[] numerals =
        {
            "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X",
            "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX",
        };

        for (int index = 0; index < planets.Count; index += 1)
        {
            string numeral;
            if (index < numerals.Length)
            {
                numeral = numerals[index];
            }
            else
            {
                numeral = (index + 1).ToString();
            }
            if (string.IsNullOrEmpty(systemName))
            {
                planets[index].Name = $"Planet {numeral}";
            }
            else
            {
                planets[index].Name = $"{systemName} {numeral}";
            }
        }
    }

    /// <summary>
    /// Estimates the number of planets likely to be generated from slots.
    /// </summary>
    public static int EstimatePlanetCount(Array<OrbitSlot> slots)
    {
        double expected = 0.0;
        foreach (OrbitSlot slot in slots)
        {
            if (slot.IsAvailable())
            {
                expected += slot.FillProbability;
            }
        }

        return (int)System.Math.Round(expected);
    }

    /// <summary>
    /// Validates that planets still match their assigned slots.
    /// </summary>
    public static bool ValidatePlanetSlotConsistency(Array<CelestialBody> planets, Array<OrbitSlot> slots)
    {
        System.Collections.Generic.Dictionary<string, OrbitSlot> planetToSlot = new();
        foreach (OrbitSlot slot in slots)
        {
            if (slot.IsFilled && !string.IsNullOrEmpty(slot.PlanetId))
            {
                planetToSlot[slot.PlanetId] = slot;
            }
        }

        foreach (CelestialBody planet in planets)
        {
            if (!planetToSlot.ContainsKey(planet.Id))
            {
                return false;
            }

            if (!planet.HasOrbital())
            {
                return false;
            }

            OrbitSlot slot = planetToSlot[planet.Id];
            double distanceDiff = System.Math.Abs(planet.Orbital!.SemiMajorAxisM - slot.SemiMajorAxisM);
            if (distanceDiff > 1000.0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns whether a slot should be filled.
    /// </summary>
    private static bool ShouldFillSlot(OrbitSlot slot, SeededRng rng)
    {
        return rng.Randf() < slot.FillProbability;
    }

    /// <summary>
    /// Generates a planet for a specific slot.
    /// </summary>
    private static CelestialBody? GeneratePlanetForSlot(
        OrbitSlot slot,
        OrbitHost host,
        Array<CelestialBody> stars,
        SeededRng rng,
        bool enablePopulation,
        GenerationUseCaseSettings? useCaseSettings,
        PlanetarySystemState planetaryState,
        RpgCompatibilityProfile compatibilityProfile,
        bool targetMainworldSlot)
    {
        SizeCategory.Category sizeCategory = DetermineSizeCategory(slot, planetaryState, rng, compatibilityProfile, targetMainworldSlot);
        int planetSeed = unchecked((int)rng.Randi());
        PlanetSpec spec = new(
            planetSeed,
            (int)sizeCategory,
            (int)slot.Zone,
            useCaseSettings: useCaseSettings);
        ApplyAggregatePlanetaryContext(spec, slot, planetaryState, rng, compatibilityProfile, targetMainworldSlot);
        spec.SetOverride("orbital.semi_major_axis_m", slot.SemiMajorAxisM);
        if (slot.SuggestedEccentricity > 0.0)
        {
            spec.SetOverride("orbital.eccentricity", slot.SuggestedEccentricity);
        }

        ParentContext context = CreateParentContext(host, stars, slot.SemiMajorAxisM, planetaryState);
        SeededRng planetRng = new(planetSeed);
        CelestialBody planet = PlanetGenerator.Generate(spec, context, planetRng, enablePopulation);
        planet.Id = $"planet_{slot.Id}";
        if (string.IsNullOrEmpty(planet.Name))
        {
            planet.Name = GeneratePlanetName(slot);
        }

        if (planet.HasOrbital())
        {
            planet.Orbital!.ParentId = host.NodeId;
        }

        return planet;
    }

    /// <summary>
    /// Selects a size category based on orbit zone.
    /// </summary>
    private static SizeCategory.Category DetermineSizeCategory(
        OrbitSlot slot,
        PlanetarySystemState planetaryState,
        SeededRng rng,
        RpgCompatibilityProfile compatibilityProfile,
        bool targetMainworldSlot)
    {
        OrbitZone.Zone zone = slot.Zone;
        System.Collections.Generic.Dictionary<SizeCategory.Category, float> weights = zone switch
        {
            OrbitZone.Zone.Hot => CloneWeights(HotZoneWeights),
            OrbitZone.Zone.Cold => CloneWeights(ColdZoneWeights),
            _ => CloneWeights(TemperateZoneWeights),
        };

        double orbitAu = slot.GetSemiMajorAxisAu();
        bool beyondSnowLine = orbitAu >= planetaryState.SnowLineAu;
        double fluxEarth = planetaryState.GetFluxEarth(orbitAu);
        double habitableAlignment = planetaryState.GetHabitableZoneAlignment(orbitAu);
        double snowLineGiantWeight = planetaryState.GetSnowLineGiantFormationWeight(orbitAu);
        double compactInnerWeight = planetaryState.GetCompactInnerArchitectureWeight(orbitAu);
        double compactRegionEdgeAu = System.Math.Max(1.6, planetaryState.SnowLineAu * 0.70);
        bool inCompactInnerRegion = orbitAu <= compactRegionEdgeAu;
        bool insideLossRegime = fluxEarth >= 1.8 || orbitAu <= (planetaryState.HabitableZoneInnerAu * 0.85);

        // Izidoro et al. (2017), Raymond and Izidoro (2017), Fernandes et al. (2019), Fulton
        // et al. (2017), Owen and Wu (2017), Ginzburg et al. (2018), and MrÃ³z et al. (2020)
        // support the architecture trends being summarized here: compact inner systems,
        // snow-line giant enhancement, loss-regime stripping, volatile delivery coupling, and
        // bounded rogue-world effects. Tuning: the scalar multipliers below are StarGen
        // generator weights inside that framework rather than literature coefficients.
        weights[SizeCategory.Category.GasGiant] = (float)(
            weights[SizeCategory.Category.GasGiant]
            * planetaryState.GasGiantWeight
            * snowLineGiantWeight);
        if (!beyondSnowLine)
        {
            weights[SizeCategory.Category.GasGiant] *= 0.72f;
        }

        weights[SizeCategory.Category.NeptuneClass] = (float)(
            weights[SizeCategory.Category.NeptuneClass]
            * planetaryState.GasBudgetScalar
            * (0.70 + (0.50 * snowLineGiantWeight)));
        weights[SizeCategory.Category.MiniNeptune] = (float)(
            weights[SizeCategory.Category.MiniNeptune]
            * (0.82 + (planetaryState.GasBudgetScalar * 0.22)));
        weights[SizeCategory.Category.Terrestrial] = (float)(weights[SizeCategory.Category.Terrestrial] * (0.85 + (planetaryState.SolidBudgetScalar * 0.45)));
        weights[SizeCategory.Category.SuperEarth] = (float)(
            weights[SizeCategory.Category.SuperEarth]
            * (0.72 + (planetaryState.SolidBudgetScalar * 0.30)));
        weights[SizeCategory.Category.Dwarf] = (float)(weights[SizeCategory.Category.Dwarf] * (0.85 + (planetaryState.ImpactStirring * 0.18)));
        weights[SizeCategory.Category.SubTerrestrial] = (float)(weights[SizeCategory.Category.SubTerrestrial] * (0.88 + (planetaryState.ImpactStirring * 0.15)));

        if (inCompactInnerRegion)
        {
            weights[SizeCategory.Category.SuperEarth] *= (float)(0.88 + (compactInnerWeight * 0.28));
            weights[SizeCategory.Category.MiniNeptune] *= (float)(0.92 + (compactInnerWeight * 0.18));
            weights[SizeCategory.Category.GasGiant] *= 0.92f;
        }

        if (orbitAu > planetaryState.SnowLineAu * 3.50)
        {
            weights[SizeCategory.Category.GasGiant] *= 0.82f;
            weights[SizeCategory.Category.NeptuneClass] *= 0.92f;
        }

        if (insideLossRegime)
        {
            weights[SizeCategory.Category.MiniNeptune] *= planetaryState.Profile.EnvelopeLossModel switch
            {
                PlanetEnvelopeLossModel.Photoevaporation => 0.45f,
                PlanetEnvelopeLossModel.CorePowered => 0.65f,
                _ => 0.55f,
            };
            weights[SizeCategory.Category.NeptuneClass] *= 0.75f;
            weights[SizeCategory.Category.SuperEarth] *= 1.18f;
            weights[SizeCategory.Category.SubTerrestrial] *= 1.10f;
        }

        if (!beyondSnowLine && habitableAlignment > 0.60 && planetaryState.VolatileDeliveryScalar > 1.0)
        {
            weights[SizeCategory.Category.Terrestrial] *= 1.15f;
            weights[SizeCategory.Category.SuperEarth] *= 1.10f;
            weights[SizeCategory.Category.MiniNeptune] *= 0.90f;
        }

        if (beyondSnowLine && planetaryState.OuterReservoirScalar > 1.0)
        {
            weights[SizeCategory.Category.NeptuneClass] *= (float)(0.95 + (0.15 * snowLineGiantWeight));
            weights[SizeCategory.Category.GasGiant] *= (float)(0.94 + (0.18 * snowLineGiantWeight));
            weights[SizeCategory.Category.Terrestrial] *= 0.88f;
        }

        ApplyCompatibilityWeights(weights, zone, habitableAlignment, beyondSnowLine, compatibilityProfile, targetMainworldSlot);

        if (planetaryState.Profile.RoguePlanetAllowance == PlanetRoguePlanetAllowance.Standard)
        {
            weights[SizeCategory.Category.Dwarf] *= 1.05f;
            weights[SizeCategory.Category.SubTerrestrial] *= 1.08f;
            weights[SizeCategory.Category.GasGiant] *= 0.92f;
        }
        else if (planetaryState.Profile.RoguePlanetAllowance == PlanetRoguePlanetAllowance.Off)
        {
            weights[SizeCategory.Category.GasGiant] *= 1.05f;
        }

        List<SizeCategory.Category> categories = new();
        List<float> weightArray = new();
        foreach (KeyValuePair<SizeCategory.Category, float> entry in weights)
        {
            categories.Add(entry.Key);
            weightArray.Add(entry.Value);
        }

        SizeCategory.Category? selected = rng.WeightedChoice(categories, weightArray);
        return selected ?? SizeCategory.Category.Terrestrial;
    }

    private static void ApplyAggregatePlanetaryContext(
        PlanetSpec spec,
        OrbitSlot slot,
        PlanetarySystemState state,
        SeededRng rng,
        RpgCompatibilityProfile compatibilityProfile,
        bool targetMainworldSlot)
    {
        double orbitAu = slot.GetSemiMajorAxisAu();
        bool beyondSnowLine = orbitAu >= state.SnowLineAu;
        double fluxEarth = state.GetFluxEarth(orbitAu);
        double habitableAlignment = state.GetHabitableZoneAlignment(orbitAu);
        double snowLineGiantWeight = state.GetSnowLineGiantFormationWeight(orbitAu);
        bool insideLossRegime = fluxEarth >= 1.8 || orbitAu <= (state.HabitableZoneInnerAu * 0.85);
        double localVolatilePositionFactor = 0.85;
        if (beyondSnowLine)
        {
            localVolatilePositionFactor = 1.10;
        }
        else if (habitableAlignment > 0.35)
        {
            localVolatilePositionFactor = 1.0;
        }

        // Raymond and Izidoro (2017) support giant-planet growth and scattering as a volatile-
        // delivery channel into inner rocky systems. Tuning: the local volatile-position and
        // giant-delivery blends below are deterministic StarGen transport weights.
        double innerGiantDeliveryBoost = 1.0;
        if (!beyondSnowLine)
        {
            innerGiantDeliveryBoost = 0.82 + (0.18 * state.GiantScatteringScalar);
            if (habitableAlignment > 0.35)
            {
                innerGiantDeliveryBoost += 0.08 * state.GiantScatteringScalar;
            }
        }

        double localVolatileDelivery = state.VolatileDeliveryScalar
            * localVolatilePositionFactor
            * innerGiantDeliveryBoost;
        double localBombardment = state.BombardmentScalar * (beyondSnowLine ? 1.05 : 0.95);

        spec.FormationTrace["system_state"] = state.ToDictionary();
        spec.FormationTrace["slot_au"] = orbitAu;
        spec.FormationTrace["snow_line_au"] = state.SnowLineAu;
        spec.FormationTrace["beyond_snow_line"] = beyondSnowLine;
        spec.FormationTrace["stellar_flux_earth"] = fluxEarth;
        spec.FormationTrace["habitable_zone_inner_au"] = state.HabitableZoneInnerAu;
        spec.FormationTrace["habitable_zone_outer_au"] = state.HabitableZoneOuterAu;
        spec.FormationTrace["habitable_zone_alignment"] = habitableAlignment;
        spec.FormationTrace["inside_radius_valley_regime"] = insideLossRegime;
        spec.FormationTrace["xuv_activity_scalar"] = state.XuvActivityScalar;
        spec.FormationTrace["outer_reservoir_scalar"] = state.OuterReservoirScalar;
        spec.FormationTrace["snow_line_giant_weight"] = snowLineGiantWeight;
        spec.FormationTrace["giant_scattering_scalar"] = state.GiantScatteringScalar;
        spec.FormationTrace["volatile_delivery_scalar"] = localVolatileDelivery;
        spec.FormationTrace["bombardment_scalar"] = localBombardment;

        if (state.Profile.MassRadiusModel == PlanetMassRadiusModel.Otegi)
        {
            spec.FormationTrace["requested_mass_radius_model"] = "otegi_2020";
        }
        else
        {
            spec.FormationTrace["requested_mass_radius_model"] = "chen_kipping";
        }

        spec.FormationTrace["envelope_loss_model"] = state.Profile.EnvelopeLossModel.ToString();
        spec.FormationTrace["gas_giant_formation_model"] = state.Profile.GasGiantFormationModel.ToString();
        spec.FormationTrace["moon_formation_bias"] = state.Profile.MoonFormationBias.ToString();
        spec.FormationTrace["rogue_planet_allowance"] = state.Profile.RoguePlanetAllowance.ToString();

        if (insideLossRegime && state.Profile.EnvelopeLossModel != PlanetEnvelopeLossModel.Auto)
        {
            spec.EnvelopeOverride = state.Profile.EnvelopeLossModel == PlanetEnvelopeLossModel.Photoevaporation
                ? PlanetEnvelopeOverride.Stripped
                : PlanetEnvelopeOverride.Thin;
        }

        if (insideLossRegime && spec.ClassBias == PlanetClassBias.Auto && rng.Randf() < 0.35f)
        {
            spec.ClassBias = state.Profile.EnvelopeLossModel switch
            {
                PlanetEnvelopeLossModel.Photoevaporation => PlanetClassBias.StrippedCore,
                PlanetEnvelopeLossModel.CorePowered => PlanetClassBias.Rocky,
                _ => PlanetClassBias.StrippedCore,
            };
        }
        else if (beyondSnowLine && spec.ClassBias == PlanetClassBias.Auto)
        {
            double gasGiantChance = 0.18 + (0.22 * snowLineGiantWeight);
            if ((state.GasBudgetScalar * snowLineGiantWeight) >= 1.10 && rng.Randf() < gasGiantChance)
            {
                spec.ClassBias = PlanetClassBias.GasGiant;
            }
            else
            {
                spec.ClassBias = PlanetClassBias.WaterRich;
            }
        }
        else if (slot.Zone == OrbitZone.Zone.Hot && state.EscapePressureProxy > 1.2 && rng.Randf() < 0.28f)
        {
            spec.ClassBias = PlanetClassBias.StrippedCore;
        }
        else if (state.GasBudgetScalar > 1.05 && rng.Randf() < 0.22f)
        {
            spec.ClassBias = PlanetClassBias.SubNeptune;
        }

        if (state.SolidBudgetScalar > 1.1 && spec.CompositionBias == PlanetCompositionBias.Auto && spec.ClassBias != PlanetClassBias.GasGiant)
        {
            spec.CompositionBias = beyondSnowLine ? PlanetCompositionBias.IcyWaterRich : PlanetCompositionBias.Rocky;
        }

        if (spec.VolatileRichness == PlanetVolatileRichness.Auto)
        {
            if (beyondSnowLine || localVolatileDelivery >= 1.35)
            {
                spec.VolatileRichness = PlanetVolatileRichness.Rich;
            }
            else if (insideLossRegime || localVolatileDelivery <= 0.75)
            {
                spec.VolatileRichness = PlanetVolatileRichness.Poor;
            }
            else if (habitableAlignment > 0.50)
            {
                spec.VolatileRichness = PlanetVolatileRichness.Moderate;
            }
        }

        if (spec.HydrosphereTendency == PlanetHydrosphereTendency.Auto && spec.ClassBias != PlanetClassBias.GasGiant)
        {
            if (!beyondSnowLine && habitableAlignment > 0.55 && localVolatileDelivery >= 1.10)
            {
                spec.HydrosphereTendency = localVolatileDelivery >= 1.35
                    ? PlanetHydrosphereTendency.Oceanic
                    : PlanetHydrosphereTendency.Mixed;
            }
            else if (insideLossRegime || localVolatileDelivery <= 0.70)
            {
                spec.HydrosphereTendency = PlanetHydrosphereTendency.Dry;
            }
        }

        if (state.Profile.MoonFormationBias == PlanetMoonFormationBias.CapturedRich)
        {
            spec.FormationTrace["captured_moon_bias"] = true;
        }

        if (targetMainworldSlot)
        {
            spec.FormationTrace["override_mainworld_candidate_bias"] = compatibilityProfile.Label;
            if (spec.ClassBias == PlanetClassBias.Auto && habitableAlignment > 0.45 && !insideLossRegime)
            {
                spec.ClassBias = PlanetClassBias.Rocky;
            }

            if (spec.CompositionBias == PlanetCompositionBias.Auto && !beyondSnowLine)
            {
                spec.CompositionBias = PlanetCompositionBias.Rocky;
            }

            if (spec.VolatileRichness == PlanetVolatileRichness.Auto)
            {
                spec.VolatileRichness = PlanetVolatileRichness.Moderate;
            }

            if (spec.HydrosphereTendency == PlanetHydrosphereTendency.Auto && habitableAlignment > 0.50)
            {
                if (compatibilityProfile.MainworldHydrosphereBias == RpgCompatibilityProfile.MainworldHydrosphereBiasType.Oceanic)
                {
                    spec.HydrosphereTendency = PlanetHydrosphereTendency.Oceanic;
                }
                else if (compatibilityProfile.MainworldHydrosphereBias == RpgCompatibilityProfile.MainworldHydrosphereBiasType.DryLeaning)
                {
                    if (localVolatileDelivery >= 1.15 && habitableAlignment >= 0.75)
                    {
                        spec.HydrosphereTendency = PlanetHydrosphereTendency.Mixed;
                    }
                    else
                    {
                        spec.HydrosphereTendency = PlanetHydrosphereTendency.Dry;
                    }
                }
                else if (compatibilityProfile.MainworldHydrosphereBias == RpgCompatibilityProfile.MainworldHydrosphereBiasType.Mixed)
                {
                    spec.HydrosphereTendency = PlanetHydrosphereTendency.Mixed;
                }
                else
                {
                    spec.HydrosphereTendency = PlanetHydrosphereTendency.Mixed;
                }
            }
        }
    }

    private static void ApplyCompatibilityWeights(
        System.Collections.Generic.Dictionary<SizeCategory.Category, float> weights,
        OrbitZone.Zone zone,
        double habitableAlignment,
        bool beyondSnowLine,
        RpgCompatibilityProfile compatibilityProfile,
        bool targetMainworldSlot)
    {
        if (!compatibilityProfile.IsActive)
        {
            return;
        }

        if (habitableAlignment > 0.45 && !beyondSnowLine)
        {
            double terrestrialBoost = compatibilityProfile.TerrestrialWorldWeightMultiplier;
            weights[SizeCategory.Category.Terrestrial] = (float)(weights[SizeCategory.Category.Terrestrial] * terrestrialBoost);
            weights[SizeCategory.Category.SuperEarth] = (float)(weights[SizeCategory.Category.SuperEarth] * (1.0 + ((terrestrialBoost - 1.0) * 0.7)));
            weights[SizeCategory.Category.GasGiant] = (float)(weights[SizeCategory.Category.GasGiant] * 0.75);
            weights[SizeCategory.Category.NeptuneClass] = (float)(weights[SizeCategory.Category.NeptuneClass] * 0.82);
        }

        if (zone == OrbitZone.Zone.Temperate)
        {
            float temperateMultiplier = (float)compatibilityProfile.TemperateSlotFillMultiplier;
            weights[SizeCategory.Category.Terrestrial] *= temperateMultiplier;
            weights[SizeCategory.Category.SuperEarth] *= temperateMultiplier;
        }
        else
        {
            float harshMultiplier = (float)compatibilityProfile.HarshSlotFillMultiplier;
            weights[SizeCategory.Category.Dwarf] *= harshMultiplier;
            weights[SizeCategory.Category.SubTerrestrial] *= harshMultiplier;
            weights[SizeCategory.Category.GasGiant] *= harshMultiplier;
        }

        if (targetMainworldSlot)
        {
            weights[SizeCategory.Category.Terrestrial] *= 1.65f;
            weights[SizeCategory.Category.SuperEarth] *= 1.45f;
            weights[SizeCategory.Category.MiniNeptune] *= 0.60f;
            weights[SizeCategory.Category.NeptuneClass] *= 0.40f;
            weights[SizeCategory.Category.GasGiant] *= 0.25f;
            weights[SizeCategory.Category.Dwarf] *= 0.45f;
            weights[SizeCategory.Category.SubTerrestrial] *= 0.55f;
        }
    }

    private static bool ShouldFillSlot(
        OrbitSlot slot,
        SeededRng rng,
        PlanetarySystemState planetaryState,
        RpgCompatibilityProfile compatibilityProfile,
        bool targetMainworldSlot)
    {
        double fillProbability = slot.FillProbability;
        double orbitAu = slot.GetSemiMajorAxisAu();
        double compactInnerWeight = planetaryState.GetCompactInnerArchitectureWeight(orbitAu);
        if (compatibilityProfile.IsActive)
        {
            double habitableAlignment = planetaryState.GetHabitableZoneAlignment(orbitAu);
            if (slot.Zone == OrbitZone.Zone.Temperate && habitableAlignment > 0.40)
            {
                fillProbability *= compatibilityProfile.TemperateSlotFillMultiplier;
            }
            else
            {
                fillProbability *= compatibilityProfile.HarshSlotFillMultiplier;
            }

            if (targetMainworldSlot)
            {
                if (compatibilityProfile.RecommendedMainworldPolicy == GenerationUseCaseSettings.MainworldPolicyType.Require)
                {
                    return true;
                }

                fillProbability = System.Math.Max(fillProbability, 0.90);
            }
        }

        double compactRegionEdgeAu = System.Math.Max(1.6, planetaryState.SnowLineAu * 0.70);
        if (slot.Zone != OrbitZone.Zone.Cold && orbitAu <= compactRegionEdgeAu)
        {
            fillProbability *= 0.92 + (0.16 * compactInnerWeight) + (0.05 * planetaryState.SolidBudgetScalar);
        }

        if (orbitAu > planetaryState.SnowLineAu * 4.0
            && planetaryState.GetSnowLineGiantFormationWeight(orbitAu) < 0.70)
        {
            fillProbability *= 0.92;
        }

        return rng.Randf() < System.Math.Clamp(fillProbability, 0.0, 1.0);
    }

    private static string SelectPreferredMainworldSlotId(
        Array<OrbitSlot> slots,
        PlanetarySystemState planetaryState,
        GenerationUseCaseSettings? useCaseSettings,
        RpgCompatibilityProfile compatibilityProfile)
    {
        if (!compatibilityProfile.IsActive || useCaseSettings == null || useCaseSettings.MainworldPolicy == GenerationUseCaseSettings.MainworldPolicyType.None)
        {
            return string.Empty;
        }

        OrbitSlot? bestSlot = null;
        double bestScore = double.MinValue;
        foreach (OrbitSlot slot in slots)
        {
            if (!slot.IsAvailable())
            {
                continue;
            }

            double score = GetSlotOrderingScore(slot, planetaryState, compatibilityProfile, true);
            if (score > bestScore)
            {
                bestScore = score;
                bestSlot = slot;
            }
        }

        if (bestSlot == null)
        {
            return string.Empty;
        }

        return bestSlot.Id;
    }

    private static double GetSlotOrderingScore(
        OrbitSlot slot,
        PlanetarySystemState planetaryState,
        RpgCompatibilityProfile compatibilityProfile,
        bool targetMainworldSlot)
    {
        double orbitAu = slot.GetSemiMajorAxisAu();
        double habitableAlignment = planetaryState.GetHabitableZoneAlignment(orbitAu);
        double fluxEarth = planetaryState.GetFluxEarth(orbitAu);
        // Kopparapu et al. (2013, 2014) support prioritizing temperate orbits and Earth-like
        // flux windows when scoring potentially habitable slots. Tuning: the `4.5`, `1.2`,
        // `-0.5`, and flux-window bonus below are StarGen prioritization weights.
        double score = slot.FillProbability;
        score += habitableAlignment * 4.5;

        if (slot.Zone == OrbitZone.Zone.Temperate)
        {
            score += 1.2 * compatibilityProfile.TemperateSlotFillMultiplier;
        }
        else
        {
            score -= 0.5 * compatibilityProfile.HarshSlotFillMultiplier;
        }

        if (fluxEarth >= 0.55 && fluxEarth <= 1.65)
        {
            score += 1.2;
        }

        if (targetMainworldSlot)
        {
            score += compatibilityProfile.TerrestrialWorldWeightMultiplier;
        }

        return score;
    }

    private static System.Collections.Generic.Dictionary<SizeCategory.Category, float> CloneWeights(System.Collections.Generic.Dictionary<SizeCategory.Category, float> source)
    {
        System.Collections.Generic.Dictionary<SizeCategory.Category, float> clone = new();
        foreach (KeyValuePair<SizeCategory.Category, float> entry in source)
        {
            clone[entry.Key] = entry.Value;
        }

        return clone;
    }

    /// <summary>
    /// Creates a parent context from an orbit host.
    /// </summary>
    private static ParentContext CreateParentContext(
        OrbitHost host,
        Array<CelestialBody> stars,
        double orbitalDistanceM,
        PlanetarySystemState planetaryState)
    {
        double systemAge = 4.6e9;
        foreach (CelestialBody star in stars)
        {
            if (star.HasStellar())
            {
                systemAge = star.Stellar!.AgeYears;
                break;
            }
        }

        return ParentContext.ForPlanet(
            host.CombinedMassKg,
            host.CombinedLuminosityWatts,
            host.EffectiveTemperatureK,
            systemAge,
            orbitalDistanceM,
            planetaryState.Profile.HabitableZoneModel);
    }

    /// <summary>
    /// Generates a default planet name for a slot.
    /// </summary>
    private static string GeneratePlanetName(OrbitSlot slot)
    {
        return $"{slot.GetZoneString()} Planet ({slot.GetSemiMajorAxisAu():0.0} AU)";
    }

    /// <summary>
    /// Builds a host lookup map.
    /// </summary>
    private static System.Collections.Generic.Dictionary<string, OrbitHost> BuildHostMap(Array<OrbitHost> orbitHosts)
    {
        System.Collections.Generic.Dictionary<string, OrbitHost> hostMap = new();
        foreach (OrbitHost host in orbitHosts)
        {
            hostMap[host.NodeId] = host;
        }

        return hostMap;
    }

    /// <summary>
    /// Clones a slot array shallowly.
    /// </summary>
    private static Array<OrbitSlot> CloneSlots(Array<OrbitSlot> source)
    {
        Array<OrbitSlot> clone = new();
        foreach (OrbitSlot slot in source)
        {
            clone.Add(slot);
        }

        return clone;
    }

    /// <summary>
    /// Sorts slots by a precomputed score descending.
    /// </summary>
    private static void SortSlotsByScore(Array<OrbitSlot> slots, System.Collections.Generic.Dictionary<OrbitSlot, double> slotScores)
    {
        List<OrbitSlot> sorted = new();
        foreach (OrbitSlot slot in slots)
        {
            sorted.Add(slot);
        }

        sorted.Sort((left, right) =>
        {
            double leftScore;
            if (slotScores.ContainsKey(left))
            {
                leftScore = slotScores[left];
            }
            else
            {
                leftScore = 0.0;
            }

            double rightScore;
            if (slotScores.ContainsKey(right))
            {
                rightScore = slotScores[right];
            }
            else
            {
                rightScore = 0.0;
            }
            int scoreComparison = rightScore.CompareTo(leftScore);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            return string.CompareOrdinal(left.Id, right.Id);
        });

        slots.Clear();
        foreach (OrbitSlot slot in sorted)
        {
            slots.Add(slot);
        }
    }
}
