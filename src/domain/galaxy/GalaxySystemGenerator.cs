using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Generates solar systems on demand from galaxy-star data.
/// </summary>
public static class GalaxySystemGenerator
{
    /// <summary>
    /// Generates a complete solar system from a galaxy-star entry.
    /// </summary>
    public static SolarSystem? GenerateSystem(
        GalaxyStar? star,
        bool includeAsteroids = true,
        bool enablePopulation = false,
        GalaxyBodyOverrides? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null,
        Galaxy? galaxy = null)
    {
        if (star == null)
        {
            return null;
        }

        SolarSystemSpec spec = CreateSpecFromStar(star, includeAsteroids, useCaseSettings, galaxy);
        if (useCaseSettings != null && useCaseSettings.GetCompatibilityProfile().ForcePopulationGeneration)
        {
            spec.GeneratePopulation = true;
        }
        bool generatePopulation = enablePopulation || spec.GeneratePopulation;
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

        PlanetGenerationResult planetResult = SystemPlanetGenerator.Generate(
            allSlots,
            hosts,
            stars,
            rng,
            generatePopulation,
            spec.UseCaseSettings,
            spec);
        foreach (CelestialBody planet in planetResult.Planets)
        {
            system.AddBody(planet);
        }

        MoonGenerationResult moonResult = SystemMoonGenerator.Generate(
            planetResult.Planets,
            hosts,
            stars,
            rng,
            generatePopulation,
            spec.UseCaseSettings,
            spec);
        foreach (CelestialBody moon in moonResult.Moons)
        {
            system.AddBody(moon);
        }

        if (includeAsteroids && spec.IncludeAsteroidBelts)
        {
            BeltGenerationResult beltResult = SystemAsteroidGenerator.Generate(
                hosts,
                planetResult.Slots,
                stars,
                rng,
                spec.UseCaseSettings,
                spec);

            foreach (AsteroidBelt belt in beltResult.Belts)
            {
                system.AddAsteroidBelt(belt);
            }

            foreach (SmallBodyReservoir reservoir in beltResult.Reservoirs)
            {
                system.AddSmallBodyReservoir(reservoir);
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

        if (overrides != null && overrides.HasAnyFor(star.StarSeed))
        {
            ApplyOverridesToSystem(system, star.StarSeed, overrides);
        }

        TravellerSystemGenerator.ApplyTravellerMainworld(system);

        return system;
    }

    /// <summary>
    /// Swaps generated bodies for edited versions where identifiers match.
    /// </summary>
    private static void ApplyOverridesToSystem(SolarSystem system, int starSeed, GalaxyBodyOverrides overrides)
    {
        Array<CelestialBody> allBodies = new();
        foreach (CelestialBody star in system.GetStars())
        {
            allBodies.Add(star);
        }

        foreach (CelestialBody planet in system.GetPlanets())
        {
            allBodies.Add(planet);
        }

        foreach (CelestialBody moon in system.GetMoons())
        {
            allBodies.Add(moon);
        }

        foreach (CelestialBody asteroid in system.GetAsteroids())
        {
            allBodies.Add(asteroid);
        }

        int replaced = overrides.ApplyToBodies(starSeed, allBodies);
        if (replaced == 0)
        {
            return;
        }

        foreach (CelestialBody body in allBodies)
        {
            if (body == null)
            {
                continue;
            }

            if (overrides.GetOverrideDict(starSeed, body.Id).Count == 0)
            {
                continue;
            }

            system.AddBody(body);
        }
    }

    /// <summary>
    /// Creates a system specification from a galaxy-star entry.
    /// </summary>
    private static SolarSystemSpec CreateSpecFromStar(GalaxyStar star, bool includeAsteroids, GenerationUseCaseSettings? useCaseSettings, Galaxy? galaxy)
    {
        SolarSystemSpec spec = SolarSystemSpec.RandomSmall(star.StarSeed);
        spec.SystemMetallicity = star.Metallicity;
        spec.SystemAgeYears = star.OriginContext.AgeMeanGyr * 1.0e9;
        spec.GalaxyContext = star.OriginContext.Clone();
        spec.StellarProfile = star.OriginContext.StellarProfile.Clone();
        spec.PlanetaryProfile = galaxy?.Config.PlanetaryProfile?.Clone() ?? PlanetaryGenerationProfile.CreateDefault();
        spec.IncludeAsteroidBelts = includeAsteroids;
        if (useCaseSettings != null)
        {
            spec.UseCaseSettings = useCaseSettings.Clone();
        }
        return spec;
    }
}
