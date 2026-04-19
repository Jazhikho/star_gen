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
    /// Returns the user-facing ruleset label.
    /// </summary>
    public static string GetRulesetLabel(GenerationUseCaseSettings.RulesetModeType rulesetMode)
    {
        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Traveller)
        {
            return TravellerRulesetLabel;
        }

        return RealisticRulesetLabel;
    }
}
