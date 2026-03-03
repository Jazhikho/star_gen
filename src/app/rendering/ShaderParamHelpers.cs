namespace StarGen.App.Rendering;

/// <summary>
/// Shared helpers for shader parameter derivation.
/// </summary>
public static class ShaderParamHelpers
{
    /// <summary>
    /// Calculates a visually useful rotation speed for shader animation.
    /// </summary>
    public static float CalculateVisualRotationSpeed(float rotationPeriodS)
    {
        float periodDays = rotationPeriodS / 86400.0f;
        if (periodDays < 1.0f)
        {
            return 0.15f;
        }

        if (periodDays < 30.0f)
        {
            return 0.05f + ((30.0f - periodDays) / 30.0f * 0.1f);
        }

        return 0.03f;
    }
}
