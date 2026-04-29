using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Population;

/// <summary>
/// Resolves the active life-model settings into concrete biology, sentience, and civilization tuning.
/// </summary>
public static class LifePotentialModeling
{
    /// <summary>
    /// Concrete tuning bundle for biosphere, sentience, and civilization gating.
    /// </summary>
    public readonly struct Tuning
    {
        public Tuning(
            GenerationUseCaseSettings.LifeFrameworkType framework,
            GenerationUseCaseSettings.AbiogenesisModelType abiogenesisModel,
            GenerationUseCaseSettings.ComplexLifeModelType complexLifeModel,
            GenerationUseCaseSettings.CivilizationModelType civilizationModel,
            GenerationUseCaseSettings.EnvironmentalWindowWeightType environmentalWindowWeight,
            double permissiveness,
            double supportThresholdOffset,
            double abiogenesisMultiplier,
            double complexLifeMultiplier,
            double sentienceMultiplier,
            double civilizationMultiplier,
            double environmentalWindowMultiplier,
            GenerationUseCaseSettings.SubsurfaceHabitabilityModelType subsurfaceHabitabilityModel,
            double darkBiosphereEnergyScale,
            bool requiresBreathableAtmosphereForComplexLife,
            bool requiresOxygenRichAtmosphereForCivilization)
        {
            Framework = framework;
            AbiogenesisModel = abiogenesisModel;
            ComplexLifeModel = complexLifeModel;
            CivilizationModel = civilizationModel;
            EnvironmentalWindowWeight = environmentalWindowWeight;
            Permissiveness = permissiveness;
            SupportThresholdOffset = supportThresholdOffset;
            AbiogenesisMultiplier = abiogenesisMultiplier;
            ComplexLifeMultiplier = complexLifeMultiplier;
            SentienceMultiplier = sentienceMultiplier;
            CivilizationMultiplier = civilizationMultiplier;
            EnvironmentalWindowMultiplier = environmentalWindowMultiplier;
            SubsurfaceHabitabilityModel = subsurfaceHabitabilityModel;
            DarkBiosphereEnergyScale = darkBiosphereEnergyScale;
            RequiresBreathableAtmosphereForComplexLife = requiresBreathableAtmosphereForComplexLife;
            RequiresOxygenRichAtmosphereForCivilization = requiresOxygenRichAtmosphereForCivilization;
        }

        public GenerationUseCaseSettings.LifeFrameworkType Framework { get; }

        public GenerationUseCaseSettings.AbiogenesisModelType AbiogenesisModel { get; }

        public GenerationUseCaseSettings.ComplexLifeModelType ComplexLifeModel { get; }

        public GenerationUseCaseSettings.CivilizationModelType CivilizationModel { get; }

        public GenerationUseCaseSettings.EnvironmentalWindowWeightType EnvironmentalWindowWeight { get; }

        public double Permissiveness { get; }

        public double SupportThresholdOffset { get; }

        public double AbiogenesisMultiplier { get; }

        public double ComplexLifeMultiplier { get; }

        public double SentienceMultiplier { get; }

        public double CivilizationMultiplier { get; }

        public double EnvironmentalWindowMultiplier { get; }

        public GenerationUseCaseSettings.SubsurfaceHabitabilityModelType SubsurfaceHabitabilityModel { get; }

        public double DarkBiosphereEnergyScale { get; }

        public bool RequiresBreathableAtmosphereForComplexLife { get; }

        public bool RequiresOxygenRichAtmosphereForCivilization { get; }
    }

    /// <summary>
    /// Resolves the active life settings and effective tuning for one environment.
    /// </summary>
    public static Tuning Resolve(PlanetEnvironmentProfile environment, GenerationUseCaseSettings? settings)
    {
        GenerationUseCaseSettings.LifeFrameworkType framework = settings?.LifeFramework
            ?? GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite;
        if (settings != null
            && settings.HasLifePermissivenessOverride()
            && framework == GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite
            && settings.AbiogenesisModel == GenerationUseCaseSettings.AbiogenesisModelType.FollowFramework
            && settings.ComplexLifeModel == GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework
            && settings.CivilizationModel == GenerationUseCaseSettings.CivilizationModelType.FollowFramework
            && settings.EnvironmentalWindowWeight == GenerationUseCaseSettings.EnvironmentalWindowWeightType.FollowFramework)
        {
            framework = GenerationUseCaseSettings.InferLifeFrameworkFromPermissiveness(settings.LifePermissiveness);
        }

        GenerationUseCaseSettings.AbiogenesisModelType abiogenesisModel = ResolveAbiogenesisModel(settings, framework);
        GenerationUseCaseSettings.ComplexLifeModelType complexLifeModel = ResolveComplexLifeModel(settings, framework);
        GenerationUseCaseSettings.CivilizationModelType civilizationModel = ResolveCivilizationModel(settings, framework);
        GenerationUseCaseSettings.EnvironmentalWindowWeightType environmentalWindowWeight = ResolveEnvironmentalWindowWeight(settings, framework);
        double permissiveness = ResolvePermissiveness(settings, framework);
        double stabilityWindow = CalculateStabilityWindow(environment);
        double supportThresholdOffset = ResolveSupportThresholdOffset(framework);
        double abiogenesisMultiplier = ResolveAbiogenesisMultiplier(abiogenesisModel, stabilityWindow);
        double complexLifeMultiplier = ResolveComplexLifeMultiplier(complexLifeModel, stabilityWindow);
        double sentienceMultiplier = ResolveSentienceMultiplier(complexLifeModel, stabilityWindow);
        double civilizationMultiplier = ResolveCivilizationMultiplier(civilizationModel, stabilityWindow);
        double environmentalWindowMultiplier = ResolveEnvironmentalWindowMultiplier(environmentalWindowWeight, stabilityWindow);
        bool requiresBreathableAtmosphereForComplexLife = complexLifeModel != GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework
            && complexLifeModel != GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters;
        if (complexLifeModel == GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters)
        {
            requiresBreathableAtmosphereForComplexLife = true;
        }

        bool requiresOxygenRichAtmosphereForCivilization = civilizationModel == GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck
            || civilizationModel == GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite;

        return new Tuning(
            framework,
            abiogenesisModel,
            complexLifeModel,
            civilizationModel,
            environmentalWindowWeight,
            permissiveness,
            supportThresholdOffset,
            abiogenesisMultiplier,
            complexLifeMultiplier,
            sentienceMultiplier,
            civilizationMultiplier,
            environmentalWindowMultiplier,
            settings?.SubsurfaceHabitabilityModel ?? GenerationUseCaseSettings.SubsurfaceHabitabilityModelType.ProtectedOceanProxy,
            System.Math.Clamp(settings?.DarkBiosphereEnergyScale ?? 1.0, 0.25, 3.0),
            requiresBreathableAtmosphereForComplexLife,
            requiresOxygenRichAtmosphereForCivilization);
    }

    /// <summary>
    /// Returns whether complex ecosystems should require breathable conditions for the active model.
    /// </summary>
    public static bool RequiresBreathableAtmosphereForComplexLife(Tuning tuning)
    {
        return tuning.RequiresBreathableAtmosphereForComplexLife;
    }

    /// <summary>
    /// Returns whether the active model rewards long stable windows.
    /// </summary>
    public static bool RewardsStableWindows(Tuning tuning)
    {
        return tuning.EnvironmentalWindowWeight != GenerationUseCaseSettings.EnvironmentalWindowWeightType.Low;
    }

    private static GenerationUseCaseSettings.AbiogenesisModelType ResolveAbiogenesisModel(
        GenerationUseCaseSettings? settings,
        GenerationUseCaseSettings.LifeFrameworkType framework)
    {
        GenerationUseCaseSettings.AbiogenesisModelType model = settings?.AbiogenesisModel
            ?? GenerationUseCaseSettings.AbiogenesisModelType.FollowFramework;
        if (model != GenerationUseCaseSettings.AbiogenesisModelType.FollowFramework)
        {
            return model;
        }

        if (framework == GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres)
        {
            return GenerationUseCaseSettings.AbiogenesisModelType.RapidStart;
        }

        return GenerationUseCaseSettings.AbiogenesisModelType.Conservative;
    }

    private static GenerationUseCaseSettings.ComplexLifeModelType ResolveComplexLifeModel(
        GenerationUseCaseSettings? settings,
        GenerationUseCaseSettings.LifeFrameworkType framework)
    {
        GenerationUseCaseSettings.ComplexLifeModelType model = settings?.ComplexLifeModel
            ?? GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework;
        if (model != GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework)
        {
            return model;
        }

        return framework switch
        {
            GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows => GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows,
            GenerationUseCaseSettings.LifeFrameworkType.RareComplexLife => GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters,
            _ => GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite,
        };
    }

    private static GenerationUseCaseSettings.CivilizationModelType ResolveCivilizationModel(
        GenerationUseCaseSettings? settings,
        GenerationUseCaseSettings.LifeFrameworkType framework)
    {
        GenerationUseCaseSettings.CivilizationModelType model = settings?.CivilizationModel
            ?? GenerationUseCaseSettings.CivilizationModelType.FollowFramework;
        if (model != GenerationUseCaseSettings.CivilizationModelType.FollowFramework)
        {
            return model;
        }

        return framework switch
        {
            GenerationUseCaseSettings.LifeFrameworkType.RareComplexLife => GenerationUseCaseSettings.CivilizationModelType.RareCivilizations,
            _ => GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite,
        };
    }

    private static GenerationUseCaseSettings.EnvironmentalWindowWeightType ResolveEnvironmentalWindowWeight(
        GenerationUseCaseSettings? settings,
        GenerationUseCaseSettings.LifeFrameworkType framework)
    {
        GenerationUseCaseSettings.EnvironmentalWindowWeightType weight = settings?.EnvironmentalWindowWeight
            ?? GenerationUseCaseSettings.EnvironmentalWindowWeightType.FollowFramework;
        if (weight != GenerationUseCaseSettings.EnvironmentalWindowWeightType.FollowFramework)
        {
            return weight;
        }

        return framework switch
        {
            GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres => GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate,
            GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows => GenerationUseCaseSettings.EnvironmentalWindowWeightType.High,
            GenerationUseCaseSettings.LifeFrameworkType.RareComplexLife => GenerationUseCaseSettings.EnvironmentalWindowWeightType.High,
            _ => GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate,
        };
    }

    private static double ResolvePermissiveness(
        GenerationUseCaseSettings? settings,
        GenerationUseCaseSettings.LifeFrameworkType framework)
    {
        if (settings == null)
        {
            return GenerationUseCaseSettings.NeutralPermissiveness;
        }

        if (settings.HasLifePermissivenessOverride())
        {
            return settings.LifePermissiveness;
        }

        return GenerationUseCaseSettings.GetRecommendedLifePermissiveness(framework);
    }

    private static double ResolveSupportThresholdOffset(GenerationUseCaseSettings.LifeFrameworkType framework)
    {
        return framework switch
        {
            GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres => -0.02,
            GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows => -0.01,
            GenerationUseCaseSettings.LifeFrameworkType.RareComplexLife => 0.04,
            _ => 0.0,
        };
    }

    private static double ResolveAbiogenesisMultiplier(
        GenerationUseCaseSettings.AbiogenesisModelType model,
        double stabilityWindow)
    {
        return model switch
        {
            GenerationUseCaseSettings.AbiogenesisModelType.RapidStart => 1.30 + (0.12 * stabilityWindow),
            GenerationUseCaseSettings.AbiogenesisModelType.Conservative => 0.88 + (0.08 * stabilityWindow),
            _ => 1.0,
        };
    }

    private static double ResolveComplexLifeMultiplier(
        GenerationUseCaseSettings.ComplexLifeModelType model,
        double stabilityWindow)
    {
        return model switch
        {
            GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows => 0.70 + (0.65 * stabilityWindow),
            GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters => 0.34 + (0.24 * stabilityWindow),
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite => 0.84 + (0.28 * stabilityWindow),
            _ => 1.0,
        };
    }

    private static double ResolveSentienceMultiplier(
        GenerationUseCaseSettings.ComplexLifeModelType complexLifeModel,
        double stabilityWindow)
    {
        return complexLifeModel switch
        {
            GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows => 0.78 + (0.44 * stabilityWindow),
            GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters => 0.28 + (0.22 * stabilityWindow),
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite => 0.86 + (0.18 * stabilityWindow),
            _ => 1.0,
        };
    }

    private static double ResolveCivilizationMultiplier(
        GenerationUseCaseSettings.CivilizationModelType model,
        double stabilityWindow)
    {
        return model switch
        {
            GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck => 0.42 + (0.18 * stabilityWindow),
            GenerationUseCaseSettings.CivilizationModelType.RareCivilizations => 0.26 + (0.14 * stabilityWindow),
            GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite => 0.62 + (0.18 * stabilityWindow),
            _ => 1.0,
        };
    }

    private static double ResolveEnvironmentalWindowMultiplier(
        GenerationUseCaseSettings.EnvironmentalWindowWeightType weight,
        double stabilityWindow)
    {
        return weight switch
        {
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Low => 0.92 + (0.12 * stabilityWindow),
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.High => 0.70 + (0.70 * stabilityWindow),
            _ => 0.82 + (0.38 * stabilityWindow),
        };
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
