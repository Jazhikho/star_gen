using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Systems.Fixtures;

/// <summary>
/// Generates deterministic solar-system fixtures for regression coverage.
/// </summary>
public static class SystemFixtureGenerator
{
    private const int BaseSeed = 600000;

    /// <summary>
    /// Generates all predefined system fixtures.
    /// </summary>
    public static Array<Dictionary> GenerateAllFixtures()
    {
        Array<Dictionary> fixtures = new();
        List<FixtureConfig> configs = GetFixtureConfigs();
        foreach (FixtureConfig config in configs)
        {
            Dictionary fixture = GenerateFixture(config);
            if (fixture.Count > 0)
            {
                fixtures.Add(fixture);
            }
        }

        return fixtures;
    }

    /// <summary>
    /// Generates a complete solar system from a specification.
    /// </summary>
    public static SolarSystem? GenerateSystem(SolarSystemSpec spec, bool? enablePopulation = null)
    {
        bool generatePopulation = enablePopulation ?? spec.GeneratePopulation;
        SeededRng rng = new(spec.GenerationSeed);
        SolarSystem? system = StellarConfigGenerator.Generate(spec, rng);
        if (system == null)
        {
            return null;
        }

        Array<CelestialBody> stars = system.GetStars();
        Array<OrbitHost> hosts = system.OrbitHosts;
        Godot.Collections.Dictionary<string, Array<OrbitSlot>> allSlotsDict =
            OrbitSlotGenerator.GenerateAllSlots(hosts, stars, system.Hierarchy, rng);

        Array<OrbitSlot> allSlots = new();
        foreach (string hostId in allSlotsDict.Keys)
        {
            foreach (OrbitSlot slot in allSlotsDict[hostId])
            {
                allSlots.Add(slot);
            }
        }

        BeltReservationResult? beltReservation = null;
        if (spec.IncludeAsteroidBelts)
        {
            beltReservation = SystemAsteroidGenerator.ReserveBeltSlots(hosts, allSlots, stars, rng);
            SystemAsteroidGenerator.MarkReservedSlots(allSlots, beltReservation.ReservedSlotIds);
        }

        PlanetGenerationResult planetResult = SystemPlanetGenerator.Generate(
            allSlots,
            hosts,
            stars,
            rng,
            generatePopulation);
        foreach (CelestialBody planet in planetResult.Planets)
        {
            system.AddBody(planet);
        }

        if (spec.IncludeAsteroidBelts)
        {
            SystemAsteroidGenerator.ClearReservedSlotMarks(planetResult.Slots);
        }

        MoonGenerationResult moonResult = SystemMoonGenerator.Generate(
            planetResult.Planets,
            hosts,
            stars,
            rng,
            generatePopulation);
        foreach (CelestialBody moon in moonResult.Moons)
        {
            system.AddBody(moon);
        }

        if (spec.IncludeAsteroidBelts && beltReservation != null)
        {
            BeltGenerationResult beltResult = SystemAsteroidGenerator.GenerateFromPredefinedBelts(
                beltReservation.Belts,
                hosts,
                stars,
                rng);

            foreach (AsteroidBelt belt in beltResult.Belts)
            {
                system.AddAsteroidBelt(belt);
            }

            foreach (CelestialBody asteroid in beltResult.Asteroids)
            {
                system.AddBody(asteroid);
            }
        }

        if (system.Provenance != null)
        {
            system.Provenance.SpecSnapshot = spec.ToDictionary();
        }

        return system;
    }

    /// <summary>
    /// Exports all fixtures to JSON strings keyed by fixture name.
    /// </summary>
    public static Dictionary ExportAllToJson(bool pretty = true)
    {
        Array<Dictionary> fixtures = GenerateAllFixtures();
        Dictionary result = new();

        foreach (Dictionary fixture in fixtures)
        {
            if (!fixture.ContainsKey("name") || fixture["name"].VariantType != Variant.Type.String)
            {
                continue;
            }

            string fixtureName = (string)fixture["name"];
            result[fixtureName] = pretty ? Json.Stringify(fixture, "\t") : Json.Stringify(fixture);
        }

        return result;
    }

    /// <summary>
    /// Returns the predefined fixture configurations.
    /// </summary>
    private static List<FixtureConfig> GetFixtureConfigs()
    {
        return new List<FixtureConfig>
        {
            new FixtureConfig("system_single_sun_like", BaseSeed + 1, 1, 1, new Array<int> { (int)StarClass.SpectralClass.G }, true),
            new FixtureConfig("system_single_red_dwarf", BaseSeed + 2, 1, 1, new Array<int> { (int)StarClass.SpectralClass.M }, true),
            new FixtureConfig("system_single_hot_blue", BaseSeed + 3, 1, 1, new Array<int> { (int)StarClass.SpectralClass.B }, false),
            new FixtureConfig("system_binary_equal", BaseSeed + 10, 2, 2, new Array<int> { (int)StarClass.SpectralClass.G, (int)StarClass.SpectralClass.G }, true),
            new FixtureConfig("system_binary_unequal", BaseSeed + 11, 2, 2, new Array<int> { (int)StarClass.SpectralClass.G, (int)StarClass.SpectralClass.M }, true),
            new FixtureConfig("system_triple_hierarchical", BaseSeed + 20, 3, 3, new Array<int> { (int)StarClass.SpectralClass.G, (int)StarClass.SpectralClass.K, (int)StarClass.SpectralClass.M }, true),
            new FixtureConfig("system_quadruple", BaseSeed + 30, 4, 4, new Array<int>(), true),
            new FixtureConfig("system_random_small", BaseSeed + 40, 1, 3, new Array<int>(), true),
            new FixtureConfig("system_max_stars", BaseSeed + 50, 10, 10, new Array<int>(), false),
            new FixtureConfig("system_minimal", BaseSeed + 60, 1, 1, new Array<int> { (int)StarClass.SpectralClass.K }, false),
        };
    }

    /// <summary>
    /// Generates a single fixture payload from a configuration.
    /// </summary>
    private static Dictionary GenerateFixture(FixtureConfig config)
    {
        SolarSystemSpec spec = new(config.SeedValue, config.StarCountMin, config.StarCountMax)
        {
            IncludeAsteroidBelts = config.IncludeBelts,
            NameHint = config.Name,
        };
        spec.SpectralClassHints = CloneHints(config.SpectralHints);

        SolarSystem? system = GenerateSystem(spec);
        if (system == null)
        {
            return new Dictionary();
        }

        return new Dictionary
        {
            ["name"] = config.Name,
            ["seed"] = config.SeedValue,
            ["spec"] = spec.ToDictionary(),
            ["system"] = SystemSerializer.ToDictionary(system),
        };
    }

    /// <summary>
    /// Clones spectral-class hints for spec ownership.
    /// </summary>
    private static Array<int> CloneHints(Array<int> source)
    {
        Array<int> clone = new();
        foreach (int hint in source)
        {
            clone.Add(hint);
        }

        return clone;
    }

    private sealed class FixtureConfig
    {
        public string Name;
        public int SeedValue;
        public int StarCountMin;
        public int StarCountMax;
        public Array<int> SpectralHints;
        public bool IncludeBelts;

        public FixtureConfig(
            string name,
            int seedValue,
            int starCountMin,
            int starCountMax,
            Array<int> spectralHints,
            bool includeBelts)
        {
            Name = name;
            SeedValue = seedValue;
            StarCountMin = starCountMin;
            StarCountMax = starCountMax;
            SpectralHints = spectralHints;
            IncludeBelts = includeBelts;
        }
    }
}
