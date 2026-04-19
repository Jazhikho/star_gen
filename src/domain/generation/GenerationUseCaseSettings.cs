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
    }

    /// <summary>
    /// Supported life-potential model families.
    /// </summary>
    public enum LifePotentialModelType
    {
        EarthHistory = 0,
        RapidBiospheres = 1,
        EnvironmentalWindows = 2,
        RareComplexLife = 3,
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
    /// Selected life-potential model used by model-aware generation flows.
    /// </summary>
    public LifePotentialModelType LifePotentialModel { get; set; } = LifePotentialModelType.EarthHistory;

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
    /// Applies Traveller-oriented defaults while preserving explicit slider values.
    /// </summary>
    public void ApplyTravellerDefaults()
    {
        RulesetMode = RulesetModeType.Traveller;
        ShowTravellerReadouts = true;
        MainworldPolicy = MainworldPolicyType.Require;
        if (IsApproximatelyNeutral(LifePermissiveness))
        {
            LifePotentialModel = LifePotentialModelType.RapidBiospheres;
            LifePermissiveness = TravellerLifePermissiveness;
        }

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
            LifePotentialModel = LifePotentialModel,
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
        int lifePotentialModelValue = GetInt(data, "life_potential_model", -1);
        if (System.Enum.IsDefined(typeof(LifePotentialModelType), lifePotentialModelValue))
        {
            settings.LifePotentialModel = (LifePotentialModelType)lifePotentialModelValue;
        }
        else
        {
            settings.LifePotentialModel = MapPermissivenessToLifeModel(settings.LifePermissiveness);
        }

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
        return model switch
        {
            LifePotentialModelType.EarthHistory => 0.50,
            LifePotentialModelType.RapidBiospheres => 0.68,
            LifePotentialModelType.EnvironmentalWindows => 0.56,
            LifePotentialModelType.RareComplexLife => 0.34,
            _ => NeutralPermissiveness,
        };
    }

    /// <summary>
    /// Returns whether the stored permissiveness differs from the selected model baseline.
    /// </summary>
    public bool HasLifePermissivenessOverride()
    {
        return System.Math.Abs(LifePermissiveness - GetRecommendedLifePermissiveness(LifePotentialModel)) > 0.001;
    }

    private static LifePotentialModelType MapPermissivenessToLifeModel(double permissiveness)
    {
        if (permissiveness <= 0.40)
        {
            return LifePotentialModelType.RareComplexLife;
        }

        if (permissiveness >= 0.62)
        {
            return LifePotentialModelType.RapidBiospheres;
        }

        if (permissiveness >= 0.53)
        {
            return LifePotentialModelType.EnvironmentalWindows;
        }

        return LifePotentialModelType.EarthHistory;
    }
}
