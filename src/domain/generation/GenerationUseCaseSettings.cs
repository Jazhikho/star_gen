using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation;

/// <summary>
/// Shared generation-intent settings that tune worldbuilding assumptions and optional Traveller readouts.
/// </summary>
public partial class GenerationUseCaseSettings : RefCounted
{
    /// <summary>
    /// Supported ruleset modes.
    /// </summary>
    public enum RulesetModeType
    {
        Default = 0,
        Traveller = 1,
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
    public double LifePermissiveness { get; set; } = 0.5;

    /// <summary>
    /// User-adjustable population permissiveness in the inclusive range [0, 1].
    /// </summary>
    public double PopulationPermissiveness { get; set; } = 0.5;

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
            PopulationPermissiveness = PopulationPermissiveness,
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
            ["population_permissiveness"] = System.Math.Clamp(PopulationPermissiveness, 0.0, 1.0),
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
        settings.LifePermissiveness = System.Math.Clamp(GetDouble(data, "life_permissiveness", 0.5), 0.0, 1.0);
        settings.PopulationPermissiveness = System.Math.Clamp(GetDouble(data, "population_permissiveness", 0.5), 0.0, 1.0);

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
}
