using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Pure deterministic calculations for colony suitability assessment.
/// </summary>
public static class SuitabilityCalculator
{
    private const double IdealTempMinK = 273.0;
    private const double IdealTempMaxK = 303.0;
    private const double SurvivableTempMinK = 200.0;
    private const double SurvivableTempMaxK = 350.0;
    private const double IdealPressureMinAtm = 0.5;
    private const double IdealPressureMaxAtm = 2.0;
    private const double SurvivablePressureMinAtm = 0.001;
    private const double SurvivablePressureMaxAtm = 10.0;
    private const double IdealGravityMinG = 0.7;
    private const double IdealGravityMaxG = 1.3;
    private const double SurvivableGravityMinG = 0.1;
    private const double SurvivableGravityMaxG = 3.0;
    private const double IdealDayMinHours = 18.0;
    private const double IdealDayMaxHours = 30.0;
    private const double SurvivableDayMinHours = 1.0;
    private const double SurvivableDayMaxHours = 720.0;
    private const double BaseDensityPerKm2 = 50.0;
    private const double EarthSurfaceKm2 = 510.1e6;
    private const double MaxGrowthRate = 0.03;
    private const double MinGrowthRate = 0.001;

    /// <summary>
    /// Calculates a complete suitability assessment from a planet profile.
    /// </summary>
    public static ColonySuitability Calculate(PlanetProfile profile)
    {
        ColonySuitability suitability = new()
        {
            BodyId = profile.BodyId,
        };

        suitability.FactorScores[(int)ColonySuitability.FactorType.Temperature] = CalculateTemperatureScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Pressure] = CalculatePressureScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Gravity] = CalculateGravityScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Atmosphere] = CalculateAtmosphereScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Water] = CalculateWaterScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Radiation] = CalculateRadiationScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Resources] = CalculateResourcesScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Terrain] = CalculateTerrainScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.Weather] = CalculateWeatherScore(profile);
        suitability.FactorScores[(int)ColonySuitability.FactorType.DayLength] = CalculateDayLengthScore(profile);

        suitability.OverallScore = CalculateOverallScore(suitability.FactorScores);
        suitability.LimitingFactors = IdentifyLimitingFactors(suitability.FactorScores);
        suitability.Advantages = IdentifyAdvantages(suitability.FactorScores);
        suitability.CarryingCapacity = CalculateCarryingCapacity(profile, suitability.OverallScore);
        suitability.BaseGrowthRate = CalculateGrowthRate(profile, suitability.OverallScore);
        suitability.InfrastructureDifficulty = CalculateInfrastructureDifficulty(profile, suitability.FactorScores);
        suitability.RequiresLifeSupport = RequiresLifeSupport(suitability.FactorScores);
        suitability.RequiresPressureSuit = RequiresPressureSuit(profile);
        suitability.RequiresRadiationShielding = RequiresRadiationShielding(profile);

        return suitability;
    }

    /// <summary>
    /// Calculates carrying capacity from profile and suitability score.
    /// </summary>
    public static int CalculateCarryingCapacity(PlanetProfile profile, int suitabilityScore)
    {
        if (suitabilityScore < 10)
        {
            return 0;
        }

        double habitableFraction = profile.GetHabitableSurface();
        if (habitableFraction < 0.05)
        {
            if (profile.OceanCoverage > 0.3)
            {
                habitableFraction = profile.OceanCoverage * 0.1;
            }
            else if (profile.IceCoverage > 0.3)
            {
                habitableFraction = profile.IceCoverage * 0.05;
            }
        }

        double surfaceMultiplier = System.Math.Max(profile.GravityG, 0.5);
        double estimatedSurfaceKm2 = EarthSurfaceKm2 * surfaceMultiplier;
        double habitableAreaKm2 = estimatedSurfaceKm2 * habitableFraction;
        double densityModifier = suitabilityScore / 100.0;
        double effectiveDensity = BaseDensityPerKm2 * densityModifier;

        double waterAbundance = profile.Resources.ContainsKey((int)ResourceType.Type.Water)
            ? (double)profile.Resources[(int)ResourceType.Type.Water]
            : 0.0;
        double waterMultiplier = 0.5 + (waterAbundance * 0.5);
        double resourceMultiplier = 0.7 + System.Math.Min(profile.Resources.Count / 10.0, 0.3);

        double capacity = habitableAreaKm2 * effectiveDensity * waterMultiplier * resourceMultiplier;
        if (capacity < 100.0 && suitabilityScore >= 10)
        {
            capacity = 100.0;
        }

        capacity = System.Math.Min(capacity, 10e9);
        return (int)System.Math.Round(capacity);
    }

    /// <summary>
    /// Calculates sustainable annual growth rate.
    /// </summary>
    public static double CalculateGrowthRate(PlanetProfile profile, int suitabilityScore)
    {
        if (suitabilityScore < 10)
        {
            return 0.0;
        }

        double baseRate = MinGrowthRate + ((MaxGrowthRate - MinGrowthRate) * (suitabilityScore / 100.0));
        if (profile.HasBreathableAtmosphere)
        {
            baseRate *= 1.2;
        }

        if (profile.RadiationLevel > 0.5)
        {
            baseRate *= 0.8;
        }

        if (profile.HasLiquidWater)
        {
            baseRate *= 1.1;
        }

        return System.Math.Clamp(baseRate, MinGrowthRate, MaxGrowthRate);
    }

    /// <summary>
    /// Projects population with logistic growth.
    /// </summary>
    public static int ProjectPopulation(
        int initialPopulation,
        int years,
        double growthRate,
        int carryingCapacity)
    {
        if (initialPopulation <= 0 || carryingCapacity <= 0 || growthRate <= 0.0)
        {
            return initialPopulation;
        }

        if (years <= 0)
        {
            return initialPopulation;
        }

        double k = carryingCapacity;
        double p0 = initialPopulation;
        if (p0 >= k)
        {
            return carryingCapacity;
        }

        double ratio = (k - p0) / p0;
        double population = k / (1.0 + (ratio * System.Math.Exp(-growthRate * years)));
        population = System.Math.Clamp(population, initialPopulation, carryingCapacity);
        return (int)System.Math.Round(population);
    }

    private static int CalculateTemperatureScore(PlanetProfile profile)
    {
        double tempK = profile.AvgTemperatureK;
        if (tempK < SurvivableTempMinK || tempK > SurvivableTempMaxK)
        {
            return 0;
        }

        if (tempK >= IdealTempMinK && tempK <= IdealTempMaxK)
        {
            return 100;
        }

        if (tempK < IdealTempMinK)
        {
            double ratio = (tempK - SurvivableTempMinK) / (IdealTempMinK - SurvivableTempMinK);
            return System.Math.Clamp((int)System.Math.Round(ratio * 100.0), 0, 99);
        }

        double highRatio = (SurvivableTempMaxK - tempK) / (SurvivableTempMaxK - IdealTempMaxK);
        return System.Math.Clamp((int)System.Math.Round(highRatio * 100.0), 0, 99);
    }

    private static int CalculatePressureScore(PlanetProfile profile)
    {
        double pressure = profile.PressureAtm;
        if (pressure < SurvivablePressureMinAtm)
        {
            return profile.HasAtmosphere ? 0 : 10;
        }

        if (pressure > SurvivablePressureMaxAtm)
        {
            return 0;
        }

        if (pressure >= IdealPressureMinAtm && pressure <= IdealPressureMaxAtm)
        {
            return 100;
        }

        if (pressure < IdealPressureMinAtm)
        {
            double ratio = (pressure - SurvivablePressureMinAtm) / (IdealPressureMinAtm - SurvivablePressureMinAtm);
            return System.Math.Clamp((int)System.Math.Round(10.0 + (ratio * 90.0)), 10, 99);
        }

        double highRatio = (pressure - IdealPressureMaxAtm) / (SurvivablePressureMaxAtm - IdealPressureMaxAtm);
        return System.Math.Clamp((int)System.Math.Round(100.0 - (highRatio * 80.0)), 20, 99);
    }

    private static int CalculateGravityScore(PlanetProfile profile)
    {
        double gravity = profile.GravityG;
        if (gravity < SurvivableGravityMinG || gravity > SurvivableGravityMaxG)
        {
            return 0;
        }

        if (gravity >= IdealGravityMinG && gravity <= IdealGravityMaxG)
        {
            return 100;
        }

        if (gravity < IdealGravityMinG)
        {
            double ratio = (gravity - SurvivableGravityMinG) / (IdealGravityMinG - SurvivableGravityMinG);
            return System.Math.Clamp((int)System.Math.Round(ratio * 100.0), 0, 99);
        }

        double highRatio = (SurvivableGravityMaxG - gravity) / (SurvivableGravityMaxG - IdealGravityMaxG);
        return System.Math.Clamp((int)System.Math.Round(highRatio * 100.0), 0, 99);
    }

    private static int CalculateAtmosphereScore(PlanetProfile profile)
    {
        if (profile.HasBreathableAtmosphere)
        {
            return 100;
        }

        if (!profile.HasAtmosphere)
        {
            return 30;
        }

        double hostility = profile.RadiationLevel;
        return System.Math.Clamp((int)System.Math.Round(60.0 - (hostility * 40.0)), 20, 60);
    }

    private static int CalculateWaterScore(PlanetProfile profile)
    {
        double waterAbundance = profile.Resources.ContainsKey((int)ResourceType.Type.Water)
            ? (double)profile.Resources[(int)ResourceType.Type.Water]
            : 0.0;

        if (profile.HasLiquidWater)
        {
            double baseScore = 70.0 + (profile.OceanCoverage * 30.0);
            return System.Math.Clamp((int)System.Math.Round(baseScore), 70, 100);
        }

        if (profile.IceCoverage > 0.0)
        {
            double score = 40.0 + (profile.IceCoverage * 40.0);
            return System.Math.Clamp((int)System.Math.Round(score), 40, 80);
        }

        if (waterAbundance > 0.0)
        {
            return System.Math.Clamp((int)System.Math.Round(waterAbundance * 60.0), 10, 60);
        }

        return 5;
    }

    private static int CalculateRadiationScore(PlanetProfile profile)
    {
        double protection = 1.0 - profile.RadiationLevel;
        if (profile.HasMagneticField)
        {
            protection = System.Math.Min(protection + 0.2, 1.0);
        }

        if (profile.HasAtmosphere && profile.PressureAtm > 0.1)
        {
            protection = System.Math.Min(protection + 0.1, 1.0);
        }

        return System.Math.Clamp((int)System.Math.Round(protection * 100.0), 0, 100);
    }

    private static int CalculateResourcesScore(PlanetProfile profile)
    {
        Dictionary weights = new()
        {
            [(int)ResourceType.Type.Water] = 2.0,
            [(int)ResourceType.Type.Metals] = 1.5,
            [(int)ResourceType.Type.Silicates] = 1.0,
            [(int)ResourceType.Type.RareElements] = 1.2,
            [(int)ResourceType.Type.Organics] = 1.3,
            [(int)ResourceType.Type.Hydrocarbons] = 1.0,
            [(int)ResourceType.Type.Volatiles] = 0.8,
            [(int)ResourceType.Type.Radioactives] = 0.7,
        };

        double score = 0.0;
        double totalWeight = 0.0;
        int resourceCount = 0;

        foreach (Variant resourceKey in profile.Resources.Keys)
        {
            double abundance = (double)profile.Resources[resourceKey];
            double weight = weights.ContainsKey(resourceKey) ? (double)weights[resourceKey] : 0.5;
            score += abundance * weight * 100.0;
            totalWeight += weight;
            resourceCount += 1;
        }

        if (totalWeight > 0.0)
        {
            score /= totalWeight;
        }

        if (resourceCount >= 5)
        {
            score += 10.0;
        }
        else if (resourceCount >= 3)
        {
            score += 5.0;
        }

        return System.Math.Clamp((int)System.Math.Round(score), 0, 100);
    }

    private static int CalculateTerrainScore(PlanetProfile profile)
    {
        double habitable = profile.GetHabitableSurface();
        if (habitable < 0.01)
        {
            if (profile.OceanCoverage > 0.5)
            {
                return 30;
            }

            if (profile.IceCoverage > 0.5)
            {
                return 25;
            }

            return 10;
        }

        double landScore = System.Math.Min(habitable / 0.3, 1.0) * 60.0;
        double tectonicPenalty = profile.TectonicActivity * 20.0;
        double volcanicPenalty = profile.VolcanismLevel * 15.0;
        double continentBonus = System.Math.Min(profile.ContinentCount / 5.0, 1.0) * 15.0;
        double score = 40.0 + landScore - tectonicPenalty - volcanicPenalty + continentBonus;

        return System.Math.Clamp((int)System.Math.Round(score), 10, 100);
    }

    private static int CalculateWeatherScore(PlanetProfile profile)
    {
        if (!profile.HasAtmosphere)
        {
            return 70;
        }

        double safety = 1.0 - profile.WeatherSeverity;
        return System.Math.Clamp((int)System.Math.Round(safety * 100.0), 0, 100);
    }

    private static int CalculateDayLengthScore(PlanetProfile profile)
    {
        double hours = profile.DayLengthHours;
        if (profile.IsTidallyLocked)
        {
            return 40;
        }

        if (hours < SurvivableDayMinHours || hours > SurvivableDayMaxHours)
        {
            return 20;
        }

        if (hours >= IdealDayMinHours && hours <= IdealDayMaxHours)
        {
            return 100;
        }

        if (hours < IdealDayMinHours)
        {
            double ratio = (hours - SurvivableDayMinHours) / (IdealDayMinHours - SurvivableDayMinHours);
            return System.Math.Clamp((int)System.Math.Round(40.0 + (ratio * 60.0)), 40, 99);
        }

        double highRatio = (SurvivableDayMaxHours - hours) / (SurvivableDayMaxHours - IdealDayMaxHours);
        return System.Math.Clamp((int)System.Math.Round(40.0 + (highRatio * 60.0)), 40, 99);
    }

    private static int CalculateOverallScore(Dictionary factorScores)
    {
        if (factorScores.Count == 0)
        {
            return 0;
        }

        Dictionary weights = new()
        {
            [(int)ColonySuitability.FactorType.Temperature] = 1.5,
            [(int)ColonySuitability.FactorType.Pressure] = 1.2,
            [(int)ColonySuitability.FactorType.Gravity] = 1.3,
            [(int)ColonySuitability.FactorType.Atmosphere] = 1.4,
            [(int)ColonySuitability.FactorType.Water] = 1.5,
            [(int)ColonySuitability.FactorType.Radiation] = 1.3,
            [(int)ColonySuitability.FactorType.Resources] = 1.0,
            [(int)ColonySuitability.FactorType.Terrain] = 0.8,
            [(int)ColonySuitability.FactorType.Weather] = 0.7,
            [(int)ColonySuitability.FactorType.DayLength] = 0.6,
        };

        double weightedSum = 0.0;
        double totalWeight = 0.0;
        int minScore = 100;

        foreach (Variant factorKey in factorScores.Keys)
        {
            int score = (int)factorScores[factorKey];
            double weight = weights.ContainsKey(factorKey) ? (double)weights[factorKey] : 1.0;
            weightedSum += score * weight;
            totalWeight += weight;
            minScore = System.Math.Min(minScore, score);
        }

        double average = totalWeight > 0.0 ? weightedSum / totalWeight : 0.0;
        if (minScore < 20)
        {
            double penalty = (20.0 - minScore) * 1.5;
            average = System.Math.Max(average - penalty, minScore * 0.5);
        }

        return System.Math.Clamp((int)System.Math.Round(average), 0, 100);
    }

    private static Array<int> IdentifyLimitingFactors(Dictionary factorScores)
    {
        List<(int Factor, int Score)> limiting = new();
        foreach (Variant factorKey in factorScores.Keys)
        {
            int score = (int)factorScores[factorKey];
            if (score < 50)
            {
                limiting.Add(((int)factorKey, score));
            }
        }

        limiting.Sort((left, right) => left.Score.CompareTo(right.Score));
        Array<int> result = new();
        foreach ((int factor, _) in limiting)
        {
            result.Add(factor);
        }

        return result;
    }

    private static Array<int> IdentifyAdvantages(Dictionary factorScores)
    {
        List<(int Factor, int Score)> advantages = new();
        foreach (Variant factorKey in factorScores.Keys)
        {
            int score = (int)factorScores[factorKey];
            if (score >= 70)
            {
                advantages.Add(((int)factorKey, score));
            }
        }

        advantages.Sort((left, right) => right.Score.CompareTo(left.Score));
        Array<int> result = new();
        foreach ((int factor, _) in advantages)
        {
            result.Add(factor);
        }

        return result;
    }

    private static double CalculateInfrastructureDifficulty(PlanetProfile profile, Dictionary factorScores)
    {
        double difficulty = 1.0;
        if (!profile.HasBreathableAtmosphere)
        {
            difficulty *= 1.5;
        }

        int tempScore = factorScores.ContainsKey((int)ColonySuitability.FactorType.Temperature)
            ? (int)factorScores[(int)ColonySuitability.FactorType.Temperature]
            : 50;
        if (tempScore < 50)
        {
            difficulty *= 1.0 + ((50.0 - tempScore) / 100.0);
        }

        if (profile.RadiationLevel > 0.3)
        {
            difficulty *= 1.0 + (profile.RadiationLevel * 0.5);
        }

        if (profile.GravityG < 0.5)
        {
            difficulty *= 1.2;
        }
        else if (profile.GravityG > 1.5)
        {
            difficulty *= 1.0 + ((profile.GravityG - 1.5) * 0.3);
        }

        if (profile.TectonicActivity > 0.5)
        {
            difficulty *= 1.1;
        }

        if (profile.WeatherSeverity > 0.6)
        {
            difficulty *= 1.1;
        }

        return System.Math.Clamp(difficulty, 0.5, 5.0);
    }

    private static bool RequiresLifeSupport(Dictionary factorScores)
    {
        int atmosphereScore = factorScores.ContainsKey((int)ColonySuitability.FactorType.Atmosphere)
            ? (int)factorScores[(int)ColonySuitability.FactorType.Atmosphere]
            : 0;
        int temperatureScore = factorScores.ContainsKey((int)ColonySuitability.FactorType.Temperature)
            ? (int)factorScores[(int)ColonySuitability.FactorType.Temperature]
            : 0;
        return atmosphereScore < 80 || temperatureScore < 60;
    }

    private static bool RequiresPressureSuit(PlanetProfile profile)
    {
        return profile.PressureAtm < 0.5 || profile.PressureAtm > 2.0;
    }

    private static bool RequiresRadiationShielding(PlanetProfile profile)
    {
        return profile.RadiationLevel > 0.5;
    }
}
