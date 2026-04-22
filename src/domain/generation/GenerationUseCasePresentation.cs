namespace StarGen.Domain.Generation;

/// <summary>
/// Shared user-facing terminology for ruleset and permissiveness presentation.
/// </summary>
public static class GenerationUseCasePresentation
{
    /// <summary>
    /// User-facing label for the default scientific pipeline.
    /// </summary>
    public const string RealisticRulesetLabel = "Realistic";

    /// <summary>
    /// User-facing label for Space Opera mode.
    /// </summary>
    public const string TravellerRulesetLabel = "Space Opera";

    /// <summary>
    /// User-facing label for Cepheus compatibility mode.
    /// </summary>
    public const string CepheusRulesetLabel = "Cepheus";

    /// <summary>
    /// User-facing label for Starfinder compatibility mode.
    /// </summary>
    public const string StarfinderRulesetLabel = "Starfinder";

    /// <summary>
    /// User-facing label for Starforged compatibility mode.
    /// </summary>
    public const string StarforgedRulesetLabel = "Starforged";

    /// <summary>
    /// Returns the user-facing ruleset label.
    /// </summary>
    public static string GetRulesetLabel(GenerationUseCaseSettings.RulesetModeType rulesetMode)
    {
        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Traveller)
        {
            return TravellerRulesetLabel;
        }

        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Cepheus)
        {
            return CepheusRulesetLabel;
        }

        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Starfinder)
        {
            return StarfinderRulesetLabel;
        }

        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Starforged)
        {
            return StarforgedRulesetLabel;
        }

        return RealisticRulesetLabel;
    }

    /// <summary>
    /// Populates an option button with the supported ruleset labels.
    /// </summary>
    public static void PopulateRulesetOptions(Godot.OptionButton optionButton)
    {
        optionButton.Clear();
        optionButton.AddItem(RealisticRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Default);
        optionButton.AddItem(TravellerRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        optionButton.AddItem(CepheusRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Cepheus);
        optionButton.AddItem(StarfinderRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Starfinder);
        optionButton.AddItem(StarforgedRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Starforged);
    }
}
