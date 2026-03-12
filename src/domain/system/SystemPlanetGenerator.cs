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
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        PlanetGenerationResult result = new()
        {
            Slots = CloneSlots(slots),
        };
        System.Collections.Generic.Dictionary<string, OrbitHost> hostMap = BuildHostMap(orbitHosts);

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

            CelestialBody? planet = GeneratePlanetForSlot(slot, hostMap[slot.OrbitHostId], stars, rng, enablePopulation, useCaseSettings);
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
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        PlanetGenerationResult result = new()
        {
            Slots = CloneSlots(slots),
        };
        System.Collections.Generic.Dictionary<string, OrbitHost> hostMap = BuildHostMap(orbitHosts);
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

            CelestialBody? planet = GeneratePlanetForSlot(slot, hostMap[slot.OrbitHostId], stars, rng, enablePopulation, useCaseSettings);
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
        GenerationUseCaseSettings? useCaseSettings)
    {
        SizeCategory.Category sizeCategory = DetermineSizeCategory(slot.Zone, rng);
        int planetSeed = unchecked((int)rng.Randi());
        PlanetSpec spec = new(
            planetSeed,
            (int)sizeCategory,
            (int)slot.Zone,
            useCaseSettings: useCaseSettings);
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
    private static SizeCategory.Category DetermineSizeCategory(OrbitZone.Zone zone, SeededRng rng)
    {
        System.Collections.Generic.Dictionary<SizeCategory.Category, float> weights = zone switch
        {
            OrbitZone.Zone.Hot => HotZoneWeights,
            OrbitZone.Zone.Cold => ColdZoneWeights,
            _ => TemperateZoneWeights,
        };

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
