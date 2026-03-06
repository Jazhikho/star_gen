namespace StarGen.Domain.Population;

/// <summary>
/// Pure deterministic calculations for colony suitability assessment.
/// Physical-environment factor scoring in SuitabilityCalculator.Scoring.cs.
/// </summary>
public static partial class SuitabilityCalculator
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

        double waterAbundance = 0.0;
        if (profile.Resources.ContainsKey((int)ResourceType.Type.Water))
        {
            waterAbundance = (double)profile.Resources[(int)ResourceType.Type.Water];
        }
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
            if (profile.HasAtmosphere)
            {
                return 0;
            }

            return 10;
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
}
