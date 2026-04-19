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

        foreach (OrbitSlot slot in result.Slots)
        {
            if (!slot.IsAvailable())
            {
                continue;
            }

            if (!ShouldFillSlot(slot, rng))
            {
                continue;
            }

            if (!hostMap.ContainsKey(slot.OrbitHostId))
            {
                continue;
            }

            CelestialBody? planet = GeneratePlanetForSlot(slot, hostMap[slot.OrbitHostId], stars, rng, enablePopulation, useCaseSettings, planetaryState);
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
        Array<OrbitSlot> availableSlots = new();
        System.Collections.Generic.Dictionary<OrbitSlot, double> slotScores = new();

        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.IsAvailable())
            {
                availableSlots.Add(slot);
                slotScores[slot] = slot.FillProbability + (rng.Randf() * 0.3);
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

            CelestialBody? planet = GeneratePlanetForSlot(slot, hostMap[slot.OrbitHostId], stars, rng, enablePopulation, useCaseSettings, planetaryState);
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
        PlanetarySystemState planetaryState)
    {
        SizeCategory.Category sizeCategory = DetermineSizeCategory(slot, planetaryState, rng);
        int planetSeed = unchecked((int)rng.Randi());
        PlanetSpec spec = new(
            planetSeed,
            (int)sizeCategory,
            (int)slot.Zone,
            useCaseSettings: useCaseSettings);
        ApplyAggregatePlanetaryContext(spec, slot, planetaryState, rng);
        spec.SetOverride("orbital.semi_major_axis_m", slot.SemiMajorAxisM);
        if (slot.SuggestedEccentricity > 0.0)
        {
            spec.SetOverride("orbital.eccentricity", slot.SuggestedEccentricity);
        }

        ParentContext context = CreateParentContext(host, stars, slot.SemiMajorAxisM);
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
    private static SizeCategory.Category DetermineSizeCategory(OrbitSlot slot, PlanetarySystemState planetaryState, SeededRng rng)
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
        bool insideLossRegime = fluxEarth >= 1.8 || orbitAu <= (planetaryState.HabitableZoneInnerAu * 0.85);

        weights[SizeCategory.Category.GasGiant] = (float)(weights[SizeCategory.Category.GasGiant] * planetaryState.GasGiantWeight * (beyondSnowLine ? 1.35 : 0.55));
        weights[SizeCategory.Category.NeptuneClass] = (float)(weights[SizeCategory.Category.NeptuneClass] * (planetaryState.GasBudgetScalar * (beyondSnowLine ? 1.2 : 0.85)));
        weights[SizeCategory.Category.MiniNeptune] = (float)(weights[SizeCategory.Category.MiniNeptune] * (0.85 + (planetaryState.GasBudgetScalar * 0.35)));
        weights[SizeCategory.Category.Terrestrial] = (float)(weights[SizeCategory.Category.Terrestrial] * (0.85 + (planetaryState.SolidBudgetScalar * 0.45)));
        weights[SizeCategory.Category.SuperEarth] = (float)(weights[SizeCategory.Category.SuperEarth] * (0.75 + (planetaryState.SolidBudgetScalar * 0.35) + (planetaryState.MigrationStrength * 0.12)));
        weights[SizeCategory.Category.Dwarf] = (float)(weights[SizeCategory.Category.Dwarf] * (0.85 + (planetaryState.ImpactStirring * 0.18)));
        weights[SizeCategory.Category.SubTerrestrial] = (float)(weights[SizeCategory.Category.SubTerrestrial] * (0.88 + (planetaryState.ImpactStirring * 0.15)));

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
            weights[SizeCategory.Category.NeptuneClass] *= 1.10f;
            weights[SizeCategory.Category.GasGiant] *= 1.08f;
            weights[SizeCategory.Category.Terrestrial] *= 0.88f;
        }

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

    private static void ApplyAggregatePlanetaryContext(PlanetSpec spec, OrbitSlot slot, PlanetarySystemState state, SeededRng rng)
    {
        double orbitAu = slot.GetSemiMajorAxisAu();
        bool beyondSnowLine = orbitAu >= state.SnowLineAu;
        double fluxEarth = state.GetFluxEarth(orbitAu);
        double habitableAlignment = state.GetHabitableZoneAlignment(orbitAu);
        bool insideLossRegime = fluxEarth >= 1.8 || orbitAu <= (state.HabitableZoneInnerAu * 0.85);
        double localVolatileDelivery = state.VolatileDeliveryScalar
            * (beyondSnowLine ? 1.15 : (habitableAlignment > 0.35 ? 1.0 : 0.85));
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
            if (state.GasBudgetScalar >= 1.15 && rng.Randf() < 0.45f)
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
    private static ParentContext CreateParentContext(OrbitHost host, Array<CelestialBody> stars, double orbitalDistanceM)
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
            orbitalDistanceM);
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
