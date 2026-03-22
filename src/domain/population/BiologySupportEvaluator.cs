using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Population;

/// <summary>
/// Shared biology-support rules used by both native-life likelihood and the ecology pipeline.
/// </summary>
public static class BiologySupportEvaluator
{
    /// <summary>
    /// Deterministic support-evaluation result used for diagnostics and downstream gating.
    /// </summary>
    public sealed class Assessment
    {
        /// <summary>
        /// Whether the evaluated world can support biology.
        /// </summary>
        public bool IsSupported { get; init; }

        /// <summary>
        /// Failure reason when biology is not supported.
        /// </summary>
        public FailureReason Reason { get; init; }
    }

    /// <summary>
    /// Canonical reasons a world fails the biology-support gate.
    /// </summary>
    public enum FailureReason
    {
        None = 0,
        NoLiquidWater = 1,
        LowHabitability = 2,
        HighRadiation = 3,
        TooCold = 4,
        TooHot = 5,
    }

    /// <summary>
    /// Returns whether a planet profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static bool SupportsBiology(
        PlanetProfile profile,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        Assessment assessment = Evaluate(profile, useCaseSettings);
        return assessment.IsSupported;
    }

    /// <summary>
    /// Evaluates whether a planet profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static Assessment Evaluate(
        PlanetProfile profile,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        PlanetEnvironmentProfile environment = PlanetEnvironmentProfile.FromPlanetProfile(
            profile,
            0,
            profile.BodyId,
            "Planet");
        return Evaluate(environment, useCaseSettings);
    }

    /// <summary>
    /// Returns whether an environment profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static bool SupportsBiology(
        PlanetEnvironmentProfile environment,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        Assessment assessment = Evaluate(environment, useCaseSettings);
        return assessment.IsSupported;
    }

    /// <summary>
    /// Evaluates whether an environment profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static Assessment Evaluate(
        PlanetEnvironmentProfile environment,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        if (!environment.HasLiquidWater)
        {
            return CreateFailure(FailureReason.NoLiquidWater);
        }

        double permissiveness = ResolveLifePermissiveness(useCaseSettings);
        if (IsGuaranteedPrimeWorld(environment, permissiveness))
        {
            return CreateSuccess();
        }

        double minimumHabitability = Lerp(8.0, 5.0, permissiveness);
        if (environment.HabitabilityScore < minimumHabitability)
        {
            return CreateFailure(FailureReason.LowHabitability);
        }

        double radiationLimit = Lerp(0.35, 0.98, permissiveness);
        if (environment.RadiationLevel >= radiationLimit)
        {
            return CreateFailure(FailureReason.HighRadiation);
        }

        double minimumTemperature = Lerp(255.0, 110.0, permissiveness);
        double maximumTemperature = Lerp(320.0, 480.0, permissiveness);
        if (!environment.HasAtmosphere)
        {
            minimumTemperature -= Lerp(0.0, 40.0, permissiveness);
            maximumTemperature += Lerp(0.0, 40.0, permissiveness);
        }

        if (environment.IsMoon)
        {
            minimumTemperature -= Lerp(10.0, 20.0, permissiveness);
            maximumTemperature += Lerp(10.0, 20.0, permissiveness);
        }

        if (!environment.HasBreathableAtmosphere)
        {
            if (permissiveness < 0.35)
            {
                return CreateFailure(FailureReason.LowHabitability);
            }

            if (!environment.HasAtmosphere && permissiveness < 0.75)
            {
                return CreateFailure(FailureReason.LowHabitability);
            }
        }

        if (environment.AvgTemperatureK < minimumTemperature)
        {
            return CreateFailure(FailureReason.TooCold);
        }

        if (environment.AvgTemperatureK > maximumTemperature)
        {
            return CreateFailure(FailureReason.TooHot);
        }

        return CreateSuccess();
    }

    private static bool IsGuaranteedPrimeWorld(PlanetEnvironmentProfile environment, double permissiveness)
    {
        if (environment.HabitabilityScore < 8)
        {
            return false;
        }

        if (!environment.HasBreathableAtmosphere && permissiveness < 0.5)
        {
            return false;
        }

        if (environment.RadiationLevel >= 0.95)
        {
            return false;
        }

        if (permissiveness <= 0.25)
        {
            return true;
        }

        return environment.HabitabilityScore >= 9;
    }

    private static double ResolveLifePermissiveness(GenerationUseCaseSettings? useCaseSettings)
    {
        if (useCaseSettings == null)
        {
            return GenerationUseCaseSettings.NeutralPermissiveness;
        }

        return useCaseSettings.LifePermissiveness;
    }

    private static double Lerp(double minValue, double maxValue, double factor)
    {
        return minValue + ((maxValue - minValue) * factor);
    }

    private static Assessment CreateSuccess()
    {
        return new Assessment
        {
            IsSupported = true,
            Reason = FailureReason.None,
        };
    }

    private static Assessment CreateFailure(FailureReason reason)
    {
        return new Assessment
        {
            IsSupported = false,
            Reason = reason,
        };
    }
}
