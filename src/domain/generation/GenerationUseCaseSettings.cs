using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation;

/// <summary>
/// Shared generation-intent settings that tune worldbuilding assumptions and optional Traveller readouts.
/// </summary>
public partial class GenerationUseCaseSettings : RefCounted
{
    /// <summary>
    /// Neutral midpoint for permissiveness sliders.
    /// </summary>
    public const double NeutralPermissiveness = 0.5;

    /// <summary>
    /// Traveller-leaning life permissiveness baseline.
    /// </summary>
    public const double TravellerLifePermissiveness = 0.55;

    /// <summary>
    /// Supported ruleset modes.
    /// </summary>
    public enum RulesetModeType
    {
        Default = 0,
        Traveller = 1,
        Cepheus = 2,
        Starfinder = 3,
        Starforged = 4,
    }

    /// <summary>
    /// Legacy life-potential model families retained for save compatibility.
    /// </summary>
    public enum LifePotentialModelType
    {
        EarthAnchoredComposite = 0,
        EarthHistory = 0,
        RapidBiospheres = 1,
        EnvironmentalWindows = 2,
        RareComplexLife = 3,
    }

    /// <summary>
    /// Top-level life framework presets.
    /// </summary>
    public enum LifeFrameworkType
    {
        EarthAnchoredComposite = 0,
        RapidBiospheres = 1,
        EnvironmentalWindows = 2,
        RareComplexLife = 3,
    }

    /// <summary>
    /// Source-aligned abiogenesis assumptions.
    /// </summary>
    public enum AbiogenesisModelType
    {
        FollowFramework = 0,
        RapidStart = 1,
        Conservative = 2,
    }

    /// <summary>
    /// Source-aligned complex-life assumptions.
    /// </summary>
    public enum ComplexLifeModelType
    {
        FollowFramework = 0,
        EarthAnchoredComposite = 1,
        EnvironmentalWindows = 2,
        RareEarthFilters = 3,
    }

    /// <summary>
    /// Source-aligned civilization assumptions.
    /// </summary>
    public enum CivilizationModelType
    {
        FollowFramework = 0,
        EarthAnchoredComposite = 1,
        RareCivilizations = 2,
        TechnosphereOxygenBottleneck = 3,
    }

    /// <summary>
    /// Weighting strength for long stable environmental windows.
    /// </summary>
    public enum EnvironmentalWindowWeightType
    {
        FollowFramework = 0,
        Low = 1,
        Moderate = 2,
        High = 3,
    }

    /// <summary>
    /// Mainworld policy strength for system and galaxy flows.
    /// </summary>
    public enum MainworldPolicyType
    {
        None = 0,
        Prefer = 1,
        Require = 2,
    }

    /// <summary>
    /// Active ruleset mode.
    /// </summary>
    public RulesetModeType RulesetMode { get; set; } = RulesetModeType.Default;

    /// <summary>
    /// Whether Traveller/UWP readouts should be visible in the UI.
    /// </summary>
    public bool ShowTravellerReadouts { get; set; }

    /// <summary>
    /// User-adjustable life permissiveness in the inclusive range [0, 1].
    /// </summary>
    public double LifePermissiveness { get; set; } = NeutralPermissiveness;

    /// <summary>
    /// Selected top-level life framework used by model-aware generation flows.
    /// </summary>
    public LifeFrameworkType LifeFramework { get; set; } = LifeFrameworkType.EarthAnchoredComposite;

    /// <summary>
    /// Abiogenesis assumption used by model-aware generation flows.
    /// </summary>
    public AbiogenesisModelType AbiogenesisModel { get; set; } = AbiogenesisModelType.FollowFramework;

    /// <summary>
    /// Complex-life assumption used by model-aware generation flows.
    /// </summary>
    public ComplexLifeModelType ComplexLifeModel { get; set; } = ComplexLifeModelType.FollowFramework;

    /// <summary>
    /// Civilization assumption used by model-aware generation flows.
    /// </summary>
    public CivilizationModelType CivilizationModel { get; set; } = CivilizationModelType.FollowFramework;

    /// <summary>
    /// Stable-window weighting assumption used by model-aware generation flows.
    /// </summary>
    public EnvironmentalWindowWeightType EnvironmentalWindowWeight { get; set; } = EnvironmentalWindowWeightType.FollowFramework;

    /// <summary>
    /// When true, supportable worlds are forced to keep native life instead of rolling it stochastically.
    /// </summary>
    public bool ForceLifeOnSupportableWorlds { get; set; }

    /// <summary>
    /// Legacy compatibility property that maps to the new life framework.
    /// </summary>
    public LifePotentialModelType LifePotentialModel
    {
        get => MapFrameworkToLegacyLifeModel(LifeFramework);
        set => LifeFramework = MapLegacyLifeModelToFramework(value);
    }

    /// <summary>
    /// Desired mainworld policy for system and galaxy flows.
    /// </summary>
    public MainworldPolicyType MainworldPolicy { get; set; } = MainworldPolicyType.None;

    /// <summary>
    /// Returns a new default settings instance.
    /// </summary>
    public static GenerationUseCaseSettings CreateDefault()
    {
        return new GenerationUseCaseSettings();
    }

    /// <summary>
    /// Returns whether Traveller ruleset mode is active.
    /// </summary>
    public bool IsTravellerMode()
    {
        return RulesetMode == RulesetModeType.Traveller;
    }

    /// <summary>
    /// Returns whether any RPG compatibility override is active.
    /// </summary>
    public bool IsRpgOverrideMode()
    {
        return RulesetMode != RulesetModeType.Default;
    }

    /// <summary>
    /// Resolves the active RPG compatibility profile.
    /// </summary>
    public RpgCompatibilityProfile GetCompatibilityProfile()
    {
        return RpgCompatibilityProfile.Resolve(RulesetMode);
    }

    /// <summary>
    /// Returns whether the active ruleset uses UWP-like readouts.
    /// </summary>
    public bool UsesUwpLikeReadouts()
    {
        if (ShowTravellerReadouts)
        {
            return true;
        }

        return GetCompatibilityProfile().UsesUwpLikeReadouts;
    }

    /// <summary>
    /// Applies Traveller-oriented defaults while preserving explicit slider values.
    /// </summary>
    public void ApplyTravellerDefaults()
    {
        RulesetMode = RulesetModeType.Traveller;
        ApplyRulesetDefaults();
    }

    /// <summary>
    /// Applies the defaults for the currently selected ruleset mode.
    /// </summary>
    public void ApplyRulesetDefaults()
    {
        RpgCompatibilityProfile profile = GetCompatibilityProfile();
        if (!profile.IsActive)
        {
            return;
        }

        ShowTravellerReadouts = profile.UsesUwpLikeReadouts;
        MainworldPolicy = profile.RecommendedMainworldPolicy;
        LifeFramework = profile.RecommendedLifeFramework;
        AbiogenesisModel = AbiogenesisModelType.FollowFramework;
        ComplexLifeModel = ComplexLifeModelType.FollowFramework;
        CivilizationModel = CivilizationModelType.FollowFramework;
        EnvironmentalWindowWeight = EnvironmentalWindowWeightType.FollowFramework;
        LifePermissiveness = profile.RecommendedLifePermissiveness;
    }

    /// <summary>
    /// Returns whether the current permissiveness values are still neutral.
    /// </summary>
    public bool HasNeutralPermissiveness()
    {
        if (!IsApproximatelyNeutral(LifePermissiveness))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Clones the current settings.
    /// </summary>
    public GenerationUseCaseSettings Clone()
    {
        return new GenerationUseCaseSettings
        {
            RulesetMode = RulesetMode,
            ShowTravellerReadouts = ShowTravellerReadouts,
            LifePermissiveness = LifePermissiveness,
            LifeFramework = LifeFramework,
            AbiogenesisModel = AbiogenesisModel,
            ComplexLifeModel = ComplexLifeModel,
            CivilizationModel = CivilizationModel,
            EnvironmentalWindowWeight = EnvironmentalWindowWeight,
            ForceLifeOnSupportableWorlds = ForceLifeOnSupportableWorlds,
            MainworldPolicy = MainworldPolicy,
        };
    }

    /// <summary>
    /// Converts the settings to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["ruleset_mode"] = (int)RulesetMode,
            ["show_traveller_readouts"] = ShowTravellerReadouts,
            ["life_permissiveness"] = System.Math.Clamp(LifePermissiveness, 0.0, 1.0),
            ["life_potential_model"] = (int)LifePotentialModel,
            ["life_framework"] = (int)LifeFramework,
            ["abiogenesis_model"] = (int)AbiogenesisModel,
            ["complex_life_model"] = (int)ComplexLifeModel,
            ["civilization_model"] = (int)CivilizationModel,
            ["environmental_window_weight"] = (int)EnvironmentalWindowWeight,
            ["force_life_on_supportable_worlds"] = ForceLifeOnSupportableWorlds,
            ["mainworld_policy"] = (int)MainworldPolicy,
        };
    }

    /// <summary>
    /// Rebuilds settings from a dictionary payload.
    /// </summary>
    public static GenerationUseCaseSettings FromDictionary(Dictionary data)
    {
        GenerationUseCaseSettings settings = new GenerationUseCaseSettings();
        if (data.Count == 0)
        {
            return settings;
        }

        int rulesetModeValue = GetInt(data, "ruleset_mode", (int)RulesetModeType.Default);
        if (System.Enum.IsDefined(typeof(RulesetModeType), rulesetModeValue))
        {
            settings.RulesetMode = (RulesetModeType)rulesetModeValue;
        }

        settings.ShowTravellerReadouts = GetBool(data, "show_traveller_readouts", false);
        settings.LifePermissiveness = System.Math.Clamp(GetDouble(data, "life_permissiveness", NeutralPermissiveness), 0.0, 1.0);
        int lifeFrameworkValue = GetInt(data, "life_framework", -1);
        if (System.Enum.IsDefined(typeof(LifeFrameworkType), lifeFrameworkValue))
        {
            settings.LifeFramework = (LifeFrameworkType)lifeFrameworkValue;
        }
        else
        {
            int lifePotentialModelValue = GetInt(data, "life_potential_model", -1);
            if (System.Enum.IsDefined(typeof(LifePotentialModelType), lifePotentialModelValue))
            {
                settings.LifeFramework = MapLegacyLifeModelToFramework((LifePotentialModelType)lifePotentialModelValue);
            }
            else
            {
                settings.LifeFramework = MapPermissivenessToFramework(settings.LifePermissiveness);
            }
        }

        int abiogenesisModelValue = GetInt(data, "abiogenesis_model", (int)AbiogenesisModelType.FollowFramework);
        if (System.Enum.IsDefined(typeof(AbiogenesisModelType), abiogenesisModelValue))
        {
            settings.AbiogenesisModel = (AbiogenesisModelType)abiogenesisModelValue;
        }

        int complexLifeModelValue = GetInt(data, "complex_life_model", (int)ComplexLifeModelType.FollowFramework);
        if (System.Enum.IsDefined(typeof(ComplexLifeModelType), complexLifeModelValue))
        {
            settings.ComplexLifeModel = (ComplexLifeModelType)complexLifeModelValue;
        }

        int civilizationModelValue = GetInt(data, "civilization_model", (int)CivilizationModelType.FollowFramework);
        if (System.Enum.IsDefined(typeof(CivilizationModelType), civilizationModelValue))
        {
            settings.CivilizationModel = (CivilizationModelType)civilizationModelValue;
        }

        int environmentalWindowWeightValue = GetInt(data, "environmental_window_weight", (int)EnvironmentalWindowWeightType.FollowFramework);
        if (System.Enum.IsDefined(typeof(EnvironmentalWindowWeightType), environmentalWindowWeightValue))
        {
            settings.EnvironmentalWindowWeight = (EnvironmentalWindowWeightType)environmentalWindowWeightValue;
        }

        settings.ForceLifeOnSupportableWorlds = GetBool(data, "force_life_on_supportable_worlds", false);

        int mainworldPolicyValue = GetInt(data, "mainworld_policy", (int)MainworldPolicyType.None);
        if (System.Enum.IsDefined(typeof(MainworldPolicyType), mainworldPolicyValue))
        {
            settings.MainworldPolicy = (MainworldPolicyType)mainworldPolicyValue;
        }

        return settings;
    }

    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool)
        {
            return (bool)data[key];
        }

        return fallback;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            _ => fallback,
        };
    }

    private static bool IsApproximatelyNeutral(double value)
    {
        return System.Math.Abs(value - NeutralPermissiveness) < 0.001;
    }

    /// <summary>
    /// Returns the baseline permissiveness value associated with a life-potential model.
    /// </summary>
    public static double GetRecommendedLifePermissiveness(LifePotentialModelType model)
    {
        return GetRecommendedLifePermissiveness(MapLegacyLifeModelToFramework(model));
    }

    /// <summary>
    /// Returns the baseline permissiveness value associated with a life framework.
    /// </summary>
    public static double GetRecommendedLifePermissiveness(LifeFrameworkType framework)
    {
        return framework switch
        {
            LifeFrameworkType.EarthAnchoredComposite => 0.50,
            LifeFrameworkType.RapidBiospheres => 0.68,
            LifeFrameworkType.EnvironmentalWindows => 0.56,
            LifeFrameworkType.RareComplexLife => 0.34,
            _ => NeutralPermissiveness,
        };
    }

    /// <summary>
    /// Infers the closest life framework for a raw permissiveness value.
    /// </summary>
    public static LifeFrameworkType InferLifeFrameworkFromPermissiveness(double permissiveness)
    {
        return MapPermissivenessToFramework(permissiveness);
    }

    /// <summary>
    /// Returns whether the stored permissiveness differs from the selected model baseline.
    /// </summary>
    public bool HasLifePermissivenessOverride()
    {
        return System.Math.Abs(LifePermissiveness - GetRecommendedLifePermissiveness(LifeFramework)) > 0.001;
    }

    private static LifeFrameworkType MapPermissivenessToFramework(double permissiveness)
    {
        if (permissiveness <= 0.40)
        {
            return LifeFrameworkType.RareComplexLife;
        }

        if (permissiveness >= 0.62)
        {
            return LifeFrameworkType.RapidBiospheres;
        }

        if (permissiveness >= 0.53)
        {
            return LifeFrameworkType.EnvironmentalWindows;
        }

        return LifeFrameworkType.EarthAnchoredComposite;
    }

    private static LifeFrameworkType MapLegacyLifeModelToFramework(LifePotentialModelType model)
    {
        return model switch
        {
            LifePotentialModelType.RapidBiospheres => LifeFrameworkType.RapidBiospheres,
            LifePotentialModelType.EnvironmentalWindows => LifeFrameworkType.EnvironmentalWindows,
            LifePotentialModelType.RareComplexLife => LifeFrameworkType.RareComplexLife,
            _ => LifeFrameworkType.EarthAnchoredComposite,
        };
    }

    private static LifePotentialModelType MapFrameworkToLegacyLifeModel(LifeFrameworkType framework)
    {
        return framework switch
        {
            LifeFrameworkType.RapidBiospheres => LifePotentialModelType.RapidBiospheres,
            LifeFrameworkType.EnvironmentalWindows => LifePotentialModelType.EnvironmentalWindows,
            LifeFrameworkType.RareComplexLife => LifePotentialModelType.RareComplexLife,
            _ => LifePotentialModelType.EarthAnchoredComposite,
        };
    }
}
