using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Colonization;

/// <summary>
/// User-facing settings for explicit colonization simulation runs.
/// </summary>
public partial class ColonizationSimulationSettings : RefCounted
{
    /// <summary>
    /// Expansion permissiveness in the inclusive range [0, 1].
    /// </summary>
    public double ExpansionPermissiveness { get; set; } = Generation.GenerationUseCaseSettings.NeutralPermissiveness;

    /// <summary>
    /// Maximum colonization epochs to simulate.
    /// </summary>
    public int MaxEpochs { get; set; } = 4;

    /// <summary>
    /// Lowest technology level that can export interstellar colonists.
    /// </summary>
    public int MinimumInterstellarTechLevel { get; set; } = (int)TechnologyLevel.Level.Interstellar;

    /// <summary>
    /// Creates a default settings instance.
    /// </summary>
    public static ColonizationSimulationSettings CreateDefault()
    {
        return new ColonizationSimulationSettings();
    }

    /// <summary>
    /// Creates a deep copy of the settings.
    /// </summary>
    public ColonizationSimulationSettings Clone()
    {
        return new ColonizationSimulationSettings
        {
            ExpansionPermissiveness = ExpansionPermissiveness,
            MaxEpochs = MaxEpochs,
            MinimumInterstellarTechLevel = MinimumInterstellarTechLevel,
        };
    }

    /// <summary>
    /// Converts the settings to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["expansion_permissiveness"] = System.Math.Clamp(ExpansionPermissiveness, 0.0, 1.0),
            ["max_epochs"] = MaxEpochs,
            ["minimum_interstellar_tech_level"] = MinimumInterstellarTechLevel,
        };
    }

    /// <summary>
    /// Rebuilds settings from a dictionary payload.
    /// </summary>
    public static ColonizationSimulationSettings FromDictionary(Dictionary data)
    {
        ColonizationSimulationSettings settings = new ColonizationSimulationSettings();
        settings.ExpansionPermissiveness = System.Math.Clamp(
            DomainDictionaryUtils.GetDouble(data, "expansion_permissiveness", Generation.GenerationUseCaseSettings.NeutralPermissiveness),
            0.0,
            1.0);
        settings.MaxEpochs = System.Math.Max(1, DomainDictionaryUtils.GetInt(data, "max_epochs", 4));
        settings.MinimumInterstellarTechLevel = DomainDictionaryUtils.GetInt(
            data,
            "minimum_interstellar_tech_level",
            (int)TechnologyLevel.Level.Interstellar);
        return settings;
    }
}
