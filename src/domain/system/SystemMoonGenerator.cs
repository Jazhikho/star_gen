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

namespace StarGen.Domain.Systems;

/// <summary>
/// Generates moons for planets in a solar system.
/// </summary>
public static class SystemMoonGenerator
{
    private static readonly SizeCategory.Category[] MoonSizeCategories =
    {
        SizeCategory.Category.Dwarf,
        SizeCategory.Category.SubTerrestrial,
        SizeCategory.Category.Terrestrial,
        SizeCategory.Category.SuperEarth,
    };

    private static readonly float[] CapturedWeights = { 80.0f, 18.0f, 2.0f, 0.0f };
    private static readonly float[] GasGiantWeights = { 30.0f, 45.0f, 20.0f, 5.0f };
    private static readonly float[] IceGiantWeights = { 40.0f, 45.0f, 14.0f, 1.0f };
    private static readonly float[] TerrestrialWeights = { 50.0f, 45.0f, 5.0f, 0.0f };

    private const double MaxHillFractionRegular = 0.40;
    private const double CaptureProbability = 0.30;

    /// <summary>
    /// Generates moons for all planets in a system.
    /// </summary>
    public static MoonGenerationResult Generate(
        Array<CelestialBody> planets,
        Array<OrbitHost> orbitHosts,
        Array<CelestialBody> stars,
        SeededRng rng,
        bool enablePopulation = false)
    {
        _ = orbitHosts;

        MoonGenerationResult result = new();
        double stellarMassKg = Units.SolarMassKg;
        double stellarLuminosityWatts = StellarProps.SolarLuminosityWatts;
        double stellarTemperatureK = 5778.0;
        double stellarAgeYears = 4.6e9;

        if (stars.Count > 0)
        {
            CelestialBody primaryStar = stars[0];
            stellarMassKg = primaryStar.Physical.MassKg;
            if (primaryStar.HasStellar())
            {
                stellarLuminosityWatts = primaryStar.Stellar!.LuminosityWatts;
                stellarTemperatureK = primaryStar.Stellar.EffectiveTemperatureK;
                stellarAgeYears = primaryStar.Stellar.AgeYears;
            }
        }

        foreach (CelestialBody planet in planets)
        {
            Array<CelestialBody> planetMoons = GenerateMoonsForPlanet(
                planet,
                stellarMassKg,
                stellarLuminosityWatts,
                stellarTemperatureK,
                stellarAgeYears,
                rng,
                enablePopulation);

            if (planetMoons.Count == 0)
            {
                continue;
            }

            Array<string> moonIds = new();
            foreach (CelestialBody moon in planetMoons)
            {
                result.Moons.Add(moon);
                moonIds.Add(moon.Id);
            }

            result.PlanetMoonMap[planet.Id] = moonIds;
        }

        result.Success = true;
        return result;
    }

    /// <summary>
    /// Assigns Greek-letter names to moons in distance order.
    /// </summary>
    public static void AssignGreekLetterNames(Array<CelestialBody> moons, string planetName = "")
    {
        string[] letters =
        {
            "Alpha", "Beta", "Gamma", "Delta", "Epsilon",
            "Zeta", "Eta", "Theta", "Iota", "Kappa",
        };

        for (int index = 0; index < moons.Count; index += 1)
        {
            string letter = index < letters.Length ? letters[index] : (index + 1).ToString();
            moons[index].Name = string.IsNullOrEmpty(planetName) ? letter : $"{planetName} {letter}";
        }
    }

    /// <summary>
    /// Returns the moons orbiting a specific planet.
    /// </summary>
    public static Array<CelestialBody> GetMoonsForPlanet(Array<CelestialBody> moons, string planetId)
    {
        Array<CelestialBody> result = new();
        foreach (CelestialBody moon in moons)
        {
            if (moon.HasOrbital() && moon.Orbital!.ParentId == planetId)
            {
                result.Add(moon);
            }
        }

        return result;
    }

    /// <summary>
    /// Sorts moons by orbital distance.
    /// </summary>
    public static void SortByDistance(Array<CelestialBody> moons)
    {
        List<CelestialBody> sorted = new();
        foreach (CelestialBody moon in moons)
        {
            sorted.Add(moon);
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

        moons.Clear();
        foreach (CelestialBody moon in sorted)
        {
            moons.Add(moon);
        }
    }

    /// <summary>
    /// Calculates summary statistics for generated moons.
    /// </summary>
    public static Dictionary GetStatistics(Array<CelestialBody> moons)
    {
        Dictionary stats = new()
        {
            ["total"] = moons.Count,
            ["captured"] = 0,
            ["regular"] = 0,
            ["has_atmosphere"] = 0,
            ["has_subsurface_ocean"] = 0,
            ["min_mass_earth"] = 0.0,
            ["max_mass_earth"] = 0.0,
            ["avg_mass_earth"] = 0.0,
        };

        if (moons.Count == 0)
        {
            return stats;
        }

        int capturedCount = 0;
        int regularCount = 0;
        int atmosphereCount = 0;
        int subsurfaceOceanCount = 0;
        double massSumEarth = 0.0;
        double minMassKg = moons[0].Physical.MassKg;
        double maxMassKg = moons[0].Physical.MassKg;

        foreach (CelestialBody moon in moons)
        {
            double massEarth = moon.Physical.MassKg / Units.EarthMassKg;
            massSumEarth += massEarth;
            minMassKg = System.Math.Min(minMassKg, moon.Physical.MassKg);
            maxMassKg = System.Math.Max(maxMassKg, moon.Physical.MassKg);

            if (moon.Name.Contains("captured"))
            {
                capturedCount += 1;
            }
            else
            {
                regularCount += 1;
            }

            if (moon.HasAtmosphere())
            {
                atmosphereCount += 1;
            }

            if (moon.HasSurface() && moon.Surface!.HasCryosphere() && moon.Surface.Cryosphere!.HasSubsurfaceOcean)
            {
                subsurfaceOceanCount += 1;
            }
        }

        stats["captured"] = capturedCount;
        stats["regular"] = regularCount;
        stats["has_atmosphere"] = atmosphereCount;
        stats["has_subsurface_ocean"] = subsurfaceOceanCount;
        stats["min_mass_earth"] = minMassKg / Units.EarthMassKg;
        stats["max_mass_earth"] = maxMassKg / Units.EarthMassKg;
        stats["avg_mass_earth"] = massSumEarth / moons.Count;
        return stats;
    }

    /// <summary>
    /// Validates that generated moons reference existing parent planets.
    /// </summary>
    public static bool ValidateMoonPlanetConsistency(Array<CelestialBody> moons, Array<CelestialBody> planets)
    {
        HashSet<string> planetIds = new();
        foreach (CelestialBody planet in planets)
        {
            planetIds.Add(planet.Id);
        }

        foreach (CelestialBody moon in moons)
        {
            if (!moon.HasOrbital())
            {
                return false;
            }

            if (string.IsNullOrEmpty(moon.Orbital!.ParentId))
            {
                return false;
            }

            if (!planetIds.Contains(moon.Orbital.ParentId))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Generates moons for a single planet.
    /// </summary>
    private static Array<CelestialBody> GenerateMoonsForPlanet(
        CelestialBody planet,
        double stellarMassKg,
        double stellarLuminosityWatts,
        double stellarTemperatureK,
        double stellarAgeYears,
        SeededRng rng,
        bool enablePopulation)
    {
        Array<CelestialBody> moons = new();
        double planetOrbitalDistanceM = planet.HasOrbital() ? planet.Orbital!.SemiMajorAxisM : Units.AuMeters;
        int moonCount = DetermineMoonCount(planet, rng);
        if (moonCount <= 0)
        {
            return moons;
        }

        double hillRadiusM = OrbitalMechanics.CalculateHillSphere(
            planet.Physical.MassKg,
            stellarMassKg,
            planetOrbitalDistanceM);
        if (hillRadiusM <= planet.Physical.RadiusM * 3.0)
        {
            return moons;
        }

        Array<double> moonDistances = GenerateMoonDistances(planet, hillRadiusM, moonCount, rng);
        for (int index = 0; index < moonDistances.Count; index += 1)
        {
            double moonDistance = moonDistances[index];
            double hillFraction = hillRadiusM > 0.0 ? moonDistance / hillRadiusM : 0.0;
            bool isCaptured = hillFraction > 0.25 && rng.Randf() < CaptureProbability;
            CelestialBody? moon = GenerateSingleMoon(
                planet,
                moonDistance,
                isCaptured,
                stellarMassKg,
                stellarLuminosityWatts,
                stellarTemperatureK,
                stellarAgeYears,
                planetOrbitalDistanceM,
                index,
                rng,
                enablePopulation);

            if (moon != null)
            {
                moons.Add(moon);
            }
        }

        return moons;
    }

    /// <summary>
    /// Determines the moon count for a planet based on its mass.
    /// </summary>
    private static int DetermineMoonCount(CelestialBody planet, SeededRng rng)
    {
        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        int minMoons;
        int maxMoons;
        double probability;

        if (massEarth >= 50.0)
        {
            minMoons = 2;
            maxMoons = 8;
            probability = 0.95;
        }
        else if (massEarth >= 10.0)
        {
            minMoons = 1;
            maxMoons = 5;
            probability = 0.90;
        }
        else if (massEarth >= 2.0)
        {
            minMoons = 0;
            maxMoons = 2;
            probability = 0.40;
        }
        else if (massEarth >= 0.3)
        {
            minMoons = 0;
            maxMoons = 2;
            probability = 0.30;
        }
        else if (massEarth >= 0.01)
        {
            minMoons = 0;
            maxMoons = 1;
            probability = 0.15;
        }
        else
        {
            minMoons = 0;
            maxMoons = 1;
            probability = 0.05;
        }

        if (rng.Randf() > probability)
        {
            return 0;
        }

        if (minMoons >= maxMoons)
        {
            return minMoons;
        }

        double raw = rng.Randf();
        double biased = System.Math.Pow(raw, 0.7);
        return (int)(minMoons + ((maxMoons + 0.99 - minMoons) * biased));
    }

    /// <summary>
    /// Generates moon orbital distances within a Hill sphere.
    /// </summary>
    private static Array<double> GenerateMoonDistances(CelestialBody planet, double hillRadiusM, int count, SeededRng rng)
    {
        Array<double> distances = new();
        double minDistance = planet.Physical.RadiusM * 3.0;
        double maxDistance = hillRadiusM * MaxHillFractionRegular;
        if (minDistance >= maxDistance)
        {
            return distances;
        }

        double logMin = System.Math.Log(minDistance);
        double logMax = System.Math.Log(maxDistance);
        double logRange = logMax - logMin;

        for (int index = 0; index < count; index += 1)
        {
            int attempts = 0;
            double distance = 0.0;

            while (attempts < 10)
            {
                double baseFraction = (index + 0.5) / count;
                double jitter = rng.RandfRange(-0.3f, 0.3f) / count;
                double fraction = System.Math.Clamp(baseFraction + jitter, 0.05, 0.95);
                double logDistance = logMin + (fraction * logRange);
                distance = System.Math.Exp(logDistance);

                bool valid = true;
                foreach (double existing in distances)
                {
                    double spacingRatio = existing < distance ? distance / existing : existing / distance;
                    if (spacingRatio < 1.3)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    break;
                }

                attempts += 1;
            }

            if (distance > minDistance)
            {
                distances.Add(distance);
            }
        }

        distances.Sort();
        return distances;
    }

    /// <summary>
    /// Generates a single moon for a parent planet.
    /// </summary>
    private static CelestialBody? GenerateSingleMoon(
        CelestialBody planet,
        double moonDistance,
        bool isCaptured,
        double stellarMassKg,
        double stellarLuminosityWatts,
        double stellarTemperatureK,
        double stellarAgeYears,
        double planetOrbitalDistanceM,
        int moonIndex,
        SeededRng rng,
        bool enablePopulation)
    {
        int moonSeed = unchecked((int)rng.Randi());
        MoonSpec spec = new(moonSeed, -1, isCaptured);
        double planetMassEarth = planet.Physical.MassKg / Units.EarthMassKg;
        SizeCategory.Category sizeCategory;

        if (isCaptured)
        {
            sizeCategory = rng.WeightedChoice(MoonSizeCategories, CapturedWeights);
        }
        else if (planetMassEarth >= 50.0)
        {
            sizeCategory = rng.WeightedChoice(MoonSizeCategories, GasGiantWeights);
        }
        else if (planetMassEarth >= 10.0)
        {
            sizeCategory = rng.WeightedChoice(MoonSizeCategories, IceGiantWeights);
        }
        else if (planetMassEarth >= 0.5)
        {
            sizeCategory = rng.WeightedChoice(MoonSizeCategories, TerrestrialWeights);
        }
        else
        {
            sizeCategory = SizeCategory.Category.Dwarf;
        }

        spec.SizeCategory = (int)sizeCategory;
        spec.SetOverride("orbital.semi_major_axis_m", moonDistance);

        ParentContext context = ParentContext.ForMoon(
            stellarMassKg,
            stellarLuminosityWatts,
            stellarTemperatureK,
            stellarAgeYears,
            planetOrbitalDistanceM,
            planet.Physical.MassKg,
            planet.Physical.RadiusM,
            moonDistance);

        SeededRng moonRng = new(moonSeed);
        CelestialBody? moon = MoonGenerator.Generate(spec, context, moonRng, enablePopulation, planet);
        if (moon == null)
        {
            return null;
        }

        string[] numerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
        string numeral = moonIndex < numerals.Length ? numerals[moonIndex] : (moonIndex + 1).ToString();
        string prefix = !string.IsNullOrEmpty(planet.Name) ? planet.Name : planet.Id;

        moon.Id = $"moon_{planet.Id}_{moonIndex}";
        moon.Name = isCaptured ? $"{prefix} {numeral} (captured)" : $"{prefix} {numeral}";
        if (moon.HasOrbital())
        {
            moon.Orbital!.ParentId = planet.Id;
        }

        return moon;
    }
}
