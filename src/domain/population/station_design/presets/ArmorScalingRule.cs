using System;

namespace StarGen.Domain.Population.StationDesign.Presets;

/// <summary>
/// Derives armor point count from hull tonnage within [MinPoints, MaxPoints].
/// </summary>
public readonly record struct ArmorScalingRule(int Divisor, int MinPoints, int MaxPoints)
{
    /// <summary>
    /// Applies the rule to a hull tonnage.
    /// </summary>
    public int Apply(int hullTons)
    {
        if (Divisor <= 0)
        {
            return MinPoints;
        }

        int raw = hullTons / Divisor;
        return System.Math.Clamp(raw, MinPoints, MaxPoints);
    }
}
