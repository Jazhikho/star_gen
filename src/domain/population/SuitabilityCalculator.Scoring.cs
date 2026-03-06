using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Environment factor scoring, overall-score aggregation, and lifecycle analysis for SuitabilityCalculator.
/// </summary>
public static partial class SuitabilityCalculator
{
    private static int CalculateWaterScore(PlanetProfile profile)
    {
        double waterAbundance = 0.0;
        if (profile.Resources.ContainsKey((int)ResourceType.Type.Water))
        {
            waterAbundance = (double)profile.Resources[(int)ResourceType.Type.Water];
        }

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

    private static readonly Dictionary _resourceWeights = new()
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

    private static readonly Dictionary _factorWeights = new()
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

    private static int CalculateResourcesScore(PlanetProfile profile)
    {
        double score = 0.0;
        double totalWeight = 0.0;
        int resourceCount = 0;

        foreach (Variant resourceKey in profile.Resources.Keys)
        {
            double abundance = (double)profile.Resources[resourceKey];
            double weight;
            if (_resourceWeights.ContainsKey(resourceKey))
            {
                weight = (double)_resourceWeights[resourceKey];
            }
            else
            {
                weight = 0.5;
            }
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

        double weightedSum = 0.0;
        double totalWeight = 0.0;
        int minScore = 100;

        foreach (Variant factorKey in factorScores.Keys)
        {
            int score = (int)factorScores[factorKey];
            double weight;
            if (_factorWeights.ContainsKey(factorKey))
            {
                weight = (double)_factorWeights[factorKey];
            }
            else
            {
                weight = 1.0;
            }
            weightedSum += score * weight;
            totalWeight += weight;
            minScore = System.Math.Min(minScore, score);
        }

        double average;
        if (totalWeight > 0.0)
        {
            average = weightedSum / totalWeight;
        }
        else
        {
            average = 0.0;
        }
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

        int tempScore = 50;
        if (factorScores.ContainsKey((int)ColonySuitability.FactorType.Temperature))
        {
            tempScore = (int)factorScores[(int)ColonySuitability.FactorType.Temperature];
        }
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
        int atmosphereScore = 0;
        if (factorScores.ContainsKey((int)ColonySuitability.FactorType.Atmosphere))
        {
            atmosphereScore = (int)factorScores[(int)ColonySuitability.FactorType.Atmosphere];
        }

        int temperatureScore = 0;
        if (factorScores.ContainsKey((int)ColonySuitability.FactorType.Temperature))
        {
            temperatureScore = (int)factorScores[(int)ColonySuitability.FactorType.Temperature];
        }
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
