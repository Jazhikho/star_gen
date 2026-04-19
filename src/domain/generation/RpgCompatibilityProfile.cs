namespace StarGen.Domain.Generation;

/// <summary>
/// Resolved compatibility-profile defaults and generation pressures for RPG override modes.
/// </summary>
public sealed class RpgCompatibilityProfile
{
    /// <summary>
    /// Active ruleset mode represented by this profile.
    /// </summary>
    public GenerationUseCaseSettings.RulesetModeType RulesetMode { get; init; }

    /// <summary>
    /// User-facing label for the profile.
    /// </summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Whether this profile is an RPG override rather than the default realistic mode.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Whether the profile uses UWP-like world-profile readouts.
    /// </summary>
    public bool UsesUwpLikeReadouts { get; init; }

    /// <summary>
    /// Whether the profile should automatically keep the population pipeline on.
    /// </summary>
    public bool ForcePopulationGeneration { get; init; }

    /// <summary>
    /// Recommended mainworld policy for the profile.
    /// </summary>
    public GenerationUseCaseSettings.MainworldPolicyType RecommendedMainworldPolicy { get; init; }

    /// <summary>
    /// Recommended life framework for the profile.
    /// </summary>
    public GenerationUseCaseSettings.LifeFrameworkType RecommendedLifeFramework { get; init; }

    /// <summary>
    /// Recommended life permissiveness for the profile.
    /// </summary>
    public double RecommendedLifePermissiveness { get; init; }

    /// <summary>
    /// Multiplier applied to temperate-slot fill pressure.
    /// </summary>
    public double TemperateSlotFillMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to harsh hot/cold slot fill pressure.
    /// </summary>
    public double HarshSlotFillMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to terrestrial and super-Earth world weights near good mainworld slots.
    /// </summary>
    public double TerrestrialWorldWeightMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to native-life probability.
    /// </summary>
    public double NativeLifeProbabilityMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to colony probability.
    /// </summary>
    public double ColonyProbabilityMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Returns the resolved profile for a ruleset mode.
    /// </summary>
    public static RpgCompatibilityProfile Resolve(GenerationUseCaseSettings.RulesetModeType rulesetMode)
    {
        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Traveller)
        {
            return new RpgCompatibilityProfile
            {
                RulesetMode = rulesetMode,
                Label = "Space Opera",
                IsActive = true,
                UsesUwpLikeReadouts = true,
                ForcePopulationGeneration = true,
                RecommendedMainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Require,
                RecommendedLifeFramework = GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres,
                RecommendedLifePermissiveness = 0.58,
                TemperateSlotFillMultiplier = 1.35,
                HarshSlotFillMultiplier = 0.85,
                TerrestrialWorldWeightMultiplier = 1.25,
                NativeLifeProbabilityMultiplier = 1.15,
                ColonyProbabilityMultiplier = 1.35,
            };
        }

        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Cepheus)
        {
            return new RpgCompatibilityProfile
            {
                RulesetMode = rulesetMode,
                Label = "Cepheus",
                IsActive = true,
                UsesUwpLikeReadouts = true,
                ForcePopulationGeneration = true,
                RecommendedMainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Require,
                RecommendedLifeFramework = GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite,
                RecommendedLifePermissiveness = 0.50,
                TemperateSlotFillMultiplier = 1.18,
                HarshSlotFillMultiplier = 0.92,
                TerrestrialWorldWeightMultiplier = 1.15,
                NativeLifeProbabilityMultiplier = 1.00,
                ColonyProbabilityMultiplier = 1.15,
            };
        }

        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Starfinder)
        {
            return new RpgCompatibilityProfile
            {
                RulesetMode = rulesetMode,
                Label = "Starfinder",
                IsActive = true,
                UsesUwpLikeReadouts = false,
                ForcePopulationGeneration = true,
                RecommendedMainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Prefer,
                RecommendedLifeFramework = GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows,
                RecommendedLifePermissiveness = 0.60,
                TemperateSlotFillMultiplier = 1.25,
                HarshSlotFillMultiplier = 1.05,
                TerrestrialWorldWeightMultiplier = 1.18,
                NativeLifeProbabilityMultiplier = 1.08,
                ColonyProbabilityMultiplier = 1.45,
            };
        }

        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Starforged)
        {
            return new RpgCompatibilityProfile
            {
                RulesetMode = rulesetMode,
                Label = "Starforged",
                IsActive = true,
                UsesUwpLikeReadouts = false,
                ForcePopulationGeneration = true,
                RecommendedMainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Prefer,
                RecommendedLifeFramework = GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite,
                RecommendedLifePermissiveness = 0.45,
                TemperateSlotFillMultiplier = 0.95,
                HarshSlotFillMultiplier = 1.15,
                TerrestrialWorldWeightMultiplier = 0.95,
                NativeLifeProbabilityMultiplier = 0.92,
                ColonyProbabilityMultiplier = 0.85,
            };
        }

        return new RpgCompatibilityProfile
        {
            RulesetMode = GenerationUseCaseSettings.RulesetModeType.Default,
            Label = GenerationUseCasePresentation.RealisticRulesetLabel,
            IsActive = false,
            UsesUwpLikeReadouts = false,
            ForcePopulationGeneration = false,
            RecommendedMainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.None,
            RecommendedLifeFramework = GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite,
            RecommendedLifePermissiveness = GenerationUseCaseSettings.NeutralPermissiveness,
        };
    }
}
