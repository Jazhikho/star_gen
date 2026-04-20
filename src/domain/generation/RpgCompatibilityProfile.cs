namespace StarGen.Domain.Generation;

/// <summary>
/// Resolved compatibility-profile defaults and generation pressures for RPG override modes.
/// </summary>
public sealed class RpgCompatibilityProfile
{
    /// <summary>
    /// Mainworld hydrosphere leaning applied when a compatibility profile is choosing a focal world.
    /// </summary>
    public enum MainworldHydrosphereBiasType
    {
        Auto = 0,
        DryLeaning = 1,
        Mixed = 2,
        Oceanic = 3,
    }

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
    /// Multiplier applied to harsh-world colony probability when life support or shielding is required.
    /// </summary>
    public double HarshColonyProbabilityMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to general civilian settlement colony types.
    /// </summary>
    public double SettlementColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to agricultural colony types.
    /// </summary>
    public double AgriculturalColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to industrial colony types.
    /// </summary>
    public double IndustrialColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to corporate colony types.
    /// </summary>
    public double CorporateColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to scientific colony types.
    /// </summary>
    public double ScientificColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to military colony types.
    /// </summary>
    public double MilitaryColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to refugee colony types.
    /// </summary>
    public double RefugeeColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Multiplier applied to separatist colony types.
    /// </summary>
    public double SeparatistColonyTypeMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Preferred mainworld hydrosphere leaning for the profile.
    /// </summary>
    public MainworldHydrosphereBiasType MainworldHydrosphereBias { get; init; } = MainworldHydrosphereBiasType.Auto;

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
                HarshColonyProbabilityMultiplier = 0.95,
                SettlementColonyTypeMultiplier = 1.15,
                AgriculturalColonyTypeMultiplier = 1.10,
                IndustrialColonyTypeMultiplier = 1.05,
                MainworldHydrosphereBias = MainworldHydrosphereBiasType.Mixed,
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
                HarshColonyProbabilityMultiplier = 0.75,
                SettlementColonyTypeMultiplier = 1.35,
                AgriculturalColonyTypeMultiplier = 1.25,
                IndustrialColonyTypeMultiplier = 1.20,
                CorporateColonyTypeMultiplier = 1.05,
                MilitaryColonyTypeMultiplier = 1.05,
                ScientificColonyTypeMultiplier = 0.95,
                RefugeeColonyTypeMultiplier = 0.90,
                SeparatistColonyTypeMultiplier = 0.95,
                MainworldHydrosphereBias = MainworldHydrosphereBiasType.Mixed,
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
                HarshColonyProbabilityMultiplier = 1.50,
                SettlementColonyTypeMultiplier = 0.95,
                AgriculturalColonyTypeMultiplier = 0.85,
                IndustrialColonyTypeMultiplier = 1.30,
                CorporateColonyTypeMultiplier = 1.35,
                ScientificColonyTypeMultiplier = 1.40,
                MilitaryColonyTypeMultiplier = 1.10,
                MainworldHydrosphereBias = MainworldHydrosphereBiasType.Oceanic,
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
                HarshColonyProbabilityMultiplier = 1.60,
                SettlementColonyTypeMultiplier = 0.80,
                AgriculturalColonyTypeMultiplier = 0.70,
                IndustrialColonyTypeMultiplier = 0.85,
                CorporateColonyTypeMultiplier = 0.85,
                ScientificColonyTypeMultiplier = 1.25,
                MilitaryColonyTypeMultiplier = 1.20,
                RefugeeColonyTypeMultiplier = 1.25,
                SeparatistColonyTypeMultiplier = 1.30,
                MainworldHydrosphereBias = MainworldHydrosphereBiasType.DryLeaning,
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
