using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Population;

/// <summary>
/// Resolves the active life-potential model into concrete biology and civilization tuning.
/// </summary>
public static class LifePotentialModeling
{
    /// <summary>
    /// Concrete tuning bundle for biosphere and civilization gating.
    /// </summary>
    public readonly struct Tuning
    {
        public Tuning(
            GenerationUseCaseSettings.LifePotentialModelType model,
            double permissiveness,
            double supportThresholdOffset,
            double abiogenesisMultiplier,
            double complexLifeMultiplier,
            double sentienceMultiplier)
        {
            Model = model;
            Permissiveness = permissiveness;
            SupportThresholdOffset = supportThresholdOffset;
            AbiogenesisMultiplier = abiogenesisMultiplier;
            ComplexLifeMultiplier = complexLifeMultiplier;
            SentienceMultiplier = sentienceMultiplier;
        }

        public GenerationUseCaseSettings.LifePotentialModelType Model { get; }

        public double Permissiveness { get; }

        public double SupportThresholdOffset { get; }

        public double AbiogenesisMultiplier { get; }

        public double ComplexLifeMultiplier { get; }

        public double SentienceMultiplier { get; }
    }

    /// <summary>
    /// Resolves the active life-potential model and its effective tuning for one environment.
    /// </summary>
    public static Tuning Resolve(PlanetEnvironmentProfile environment, GenerationUseCaseSettings? settings)
    {
        GenerationUseCaseSettings.LifePotentialModelType model = settings?.LifePotentialModel
            ?? GenerationUseCaseSettings.LifePotentialModelType.EarthHistory;
        double permissiveness = ResolvePermissiveness(settings);
        double stabilityWindow = CalculateStabilityWindow(environment);

        return model switch
        {
            GenerationUseCaseSettings.LifePotentialModelType.RapidBiospheres => new Tuning(
                model,
                permissiveness,
                -0.02,
                1.30 + (0.12 * stabilityWindow),
                0.90 + (0.10 * stabilityWindow),
                0.85 + (0.08 * stabilityWindow)),
            GenerationUseCaseSettings.LifePotentialModelType.EnvironmentalWindows => new Tuning(
                model,
                permissiveness,
                -0.01,
                0.95 + (0.15 * stabilityWindow),
                0.70 + (0.65 * stabilityWindow),
                0.55 + (0.85 * stabilityWindow)),
            GenerationUseCaseSettings.LifePotentialModelType.RareComplexLife => new Tuning(
                model,
                permissiveness,
                0.04,
                0.72 + (0.08 * stabilityWindow),
                0.34 + (0.24 * stabilityWindow),
                0.18 + (0.22 * stabilityWindow)),
            _ => new Tuning(
                GenerationUseCaseSettings.LifePotentialModelType.EarthHistory,
                permissiveness,
                0.0,
                1.0,
                1.0,
                1.0),
        };
    }

    /// <summary>
    /// Returns whether complex ecosystems should require breathable, oxygen-rich conditions for the active model.
    /// </summary>
    public static bool RequiresBreathableAtmosphereForComplexLife(GenerationUseCaseSettings.LifePotentialModelType model)
    {
        return model == GenerationUseCaseSettings.LifePotentialModelType.EarthHistory
            || model == GenerationUseCaseSettings.LifePotentialModelType.EnvironmentalWindows
            || model == GenerationUseCaseSettings.LifePotentialModelType.RareComplexLife;
    }

    /// <summary>
    /// Returns whether the active model strongly rewards long stable habitable windows.
    /// </summary>
    public static bool RewardsStableWindows(GenerationUseCaseSettings.LifePotentialModelType model)
    {
        return model == GenerationUseCaseSettings.LifePotentialModelType.EnvironmentalWindows
            || model == GenerationUseCaseSettings.LifePotentialModelType.RareComplexLife;
    }

    private static double ResolvePermissiveness(GenerationUseCaseSettings? settings)
    {
        if (settings == null)
        {
            return GenerationUseCaseSettings.NeutralPermissiveness;
        }

        if (settings.HasLifePermissivenessOverride())
        {
            return settings.LifePermissiveness;
        }

        return GenerationUseCaseSettings.GetRecommendedLifePermissiveness(settings.LifePotentialModel);
    }

    private static double CalculateStabilityWindow(PlanetEnvironmentProfile environment)
    {
        double score = 0.0;
        score += Clamp01(environment.HabitableZoneAlignment) * 0.24;
        score += Clamp01(environment.OceanCoverage + environment.LandCoverage) * 0.14;
        score += Clamp01(environment.LandCoverage * 1.2) * 0.12;
        score += Clamp01(1.0 - environment.RadiationLevel) * 0.16;
        score += Clamp01(1.0 - environment.XuvExposure) * 0.10;
        score += Clamp01(1.0 - (environment.WeatherSeverity * 0.75)) * 0.10;
        score += Clamp01(1.0 - (environment.VolcanismLevel * 0.65)) * 0.06;
        score += Clamp01(environment.HasBreathableAtmosphere ? 1.0 : 0.0) * 0.08;
        return Clamp01(score);
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
