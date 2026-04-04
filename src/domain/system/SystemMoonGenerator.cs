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
        bool enablePopulation = false,
        GenerationUseCaseSettings? useCaseSettings = null,
        SolarSystemSpec? systemSpec = null)
    {
        _ = orbitHosts;

        MoonGenerationResult result = new();
        PlanetarySystemState planetaryState = PlanetarySystemState.Build(systemSpec, stars);
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
                planetaryState,
                rng,
                enablePopulation,
                useCaseSettings);

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
            string letter;
            if (index < letters.Length)
            {
                letter = letters[index];
            }
            else
            {
                letter = (index + 1).ToString();
            }

            string name;
            if (string.IsNullOrEmpty(planetName))
            {
                name = letter;
            }
            else
            {
                name = $"{planetName} {letter}";
            }

            moons[index].Name = name;
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
        PlanetarySystemState planetaryState,
        SeededRng rng,
        bool enablePopulation,
        GenerationUseCaseSettings? useCaseSettings)
    {
        Array<CelestialBody> moons = new();
        double planetOrbitalDistanceM;
        if (planet.HasOrbital())
        {
            planetOrbitalDistanceM = planet.Orbital!.SemiMajorAxisM;
        }
        else
        {
            planetOrbitalDistanceM = Units.AuMeters;
        }
        int moonCount = DetermineMoonCount(planet, planetaryState, rng);
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
            double hillFraction;
            if (hillRadiusM > 0.0)
            {
                hillFraction = moonDistance / hillRadiusM;
            }
            else
            {
                hillFraction = 0.0;
            }
            bool isCaptured = hillFraction > 0.25 && rng.Randf() < CalculateCaptureProbability(planet, planetaryState, hillFraction);
            if (isCaptured)
            {
                moonDistance = System.Math.Max(moonDistance, hillRadiusM * rng.RandfRange(0.32f, 0.60f));
            }
            CelestialBody? moon = GenerateSingleMoon(
                planet,
                moonDistance,
                isCaptured,
                stellarMassKg,
                stellarLuminosityWatts,
                stellarTemperatureK,
                stellarAgeYears,
                planetOrbitalDistanceM,
                planetaryState,
                index,
                rng,
                enablePopulation,
                useCaseSettings);

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
    private static int DetermineMoonCount(CelestialBody planet, PlanetarySystemState planetaryState, SeededRng rng)
    {
        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        double orbitAu = planet.HasOrbital() ? planet.Orbital!.SemiMajorAxisM / Units.AuMeters : planetaryState.SnowLineAu;
        bool beyondSnowLine = orbitAu >= planetaryState.SnowLineAu;
        double regularDiskBonus = planetaryState.Profile.MoonFormationBias switch
        {
            PlanetMoonFormationBias.RegularDiskFavored => 1.20,
            PlanetMoonFormationBias.CapturedRich => 0.92,
            _ => 1.0,
        };
        double outerSystemBonus = beyondSnowLine ? 1.20 : 0.90;
        int minMoons;
        int maxMoons;
        double probability;

        if (massEarth >= 50.0)
        {
            minMoons = beyondSnowLine ? 3 : 2;
            maxMoons = beyondSnowLine ? 12 : 8;
            probability = 0.94 * regularDiskBonus * outerSystemBonus;
        }
        else if (massEarth >= 10.0)
        {
            minMoons = 1;
            maxMoons = beyondSnowLine ? 6 : 4;
            probability = 0.82 * regularDiskBonus * outerSystemBonus;
        }
        else if (massEarth >= 2.0)
        {
            minMoons = 0;
            maxMoons = 1;
            probability = 0.24 + (planetaryState.ImpactStirring * 0.08);
        }
        else if (massEarth >= 0.3)
        {
            minMoons = 0;
            maxMoons = 1;
            probability = 0.18 + (planetaryState.ImpactStirring * 0.06);
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

        probability = System.Math.Clamp(probability, 0.02, 0.99);

        if (rng.Randf() > probability)
        {
            return 0;
        }

        if (minMoons >= maxMoons)
        {
            return minMoons;
        }

        double raw = rng.Randf();
            double biasExponent = massEarth >= 10.0 ? 0.60 : 0.85;
            double biased = System.Math.Pow(raw, biasExponent);
            return (int)(minMoons + ((maxMoons + 0.99 - minMoons) * biased));
    }

    private static double CalculateCaptureProbability(
        CelestialBody planet,
        PlanetarySystemState planetaryState,
        double hillFraction)
    {
        double orbitAu = planet.HasOrbital() ? planet.Orbital!.SemiMajorAxisM / Units.AuMeters : planetaryState.SnowLineAu;
        bool beyondSnowLine = orbitAu >= planetaryState.SnowLineAu;
        double baseProbability = beyondSnowLine ? 0.12 : 0.04;
        baseProbability *= planetaryState.Profile.MoonFormationBias switch
        {
            PlanetMoonFormationBias.CapturedRich => 1.85,
            PlanetMoonFormationBias.RegularDiskFavored => 0.55,
            _ => 1.0,
        };
        baseProbability *= 0.85 + (planetaryState.ImpactStirring * 0.20);
        baseProbability *= System.Math.Clamp(0.50 + (hillFraction * 1.40), 0.40, 1.35);
        return System.Math.Clamp(baseProbability, 0.02, 0.60);
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
                    double spacingRatio;
                    if (existing < distance)
                    {
                        spacingRatio = distance / existing;
                    }
                    else
                    {
                        spacingRatio = existing / distance;
                    }
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
        PlanetarySystemState planetaryState,
        int moonIndex,
        SeededRng rng,
        bool enablePopulation,
        GenerationUseCaseSettings? useCaseSettings)
    {
        int moonSeed = unchecked((int)rng.Randi());
        MoonSpec spec = new(moonSeed, -1, isCaptured, useCaseSettings: useCaseSettings);
        double planetMassEarth = planet.Physical.MassKg / Units.EarthMassKg;
        SizeCategory.Category sizeCategory;

        if (isCaptured)
        {
            sizeCategory = rng.WeightedChoice(MoonSizeCategories, CapturedWeights);
        }
        else if (planetMassEarth >= 50.0)
        {
            float[] weights = (float[])GasGiantWeights.Clone();
            if (planetaryState.Profile.MoonFormationBias == PlanetMoonFormationBias.RegularDiskFavored)
            {
                weights[2] += 8.0f;
                weights[3] += 4.0f;
            }

            sizeCategory = rng.WeightedChoice(MoonSizeCategories, weights);
        }
        else if (planetMassEarth >= 10.0)
        {
            float[] weights = (float[])IceGiantWeights.Clone();
            if (planetaryState.Profile.MoonFormationBias == PlanetMoonFormationBias.RegularDiskFavored)
            {
                weights[2] += 4.0f;
            }

            sizeCategory = rng.WeightedChoice(MoonSizeCategories, weights);
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
        string numeral;
        if (moonIndex < numerals.Length)
        {
            numeral = numerals[moonIndex];
        }
        else
        {
            numeral = (moonIndex + 1).ToString();
        }

        string prefix;
        if (!string.IsNullOrEmpty(planet.Name))
        {
            prefix = planet.Name;
        }
        else
        {
            prefix = planet.Id;
        }

        moon.Id = $"moon_{planet.Id}_{moonIndex}";
        string moonName;
        if (isCaptured)
        {
            moonName = $"{prefix} {numeral} (captured)";
        }
        else
        {
            moonName = $"{prefix} {numeral}";
        }

        moon.Name = moonName;
        if (moon.HasOrbital())
        {
            moon.Orbital!.ParentId = planet.Id;
        }

        return moon;
    }
}
