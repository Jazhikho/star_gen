using System;

namespace StarGen.Domain.Population.StationDesign.Presets;

/// <summary>
/// Scales a facility/docking count from hull tonnage: max(Minimum, floor(hullTons / Divisor)).
/// </summary>
public readonly record struct ScalingRule(int Divisor, int Minimum)
{
    /// <summary>
    /// Applies the rule to a hull tonnage.
    /// </summary>
    public int Apply(int hullTons)
    {
        if (Divisor <= 0)
        {
            return Minimum;
        }

        return System.Math.Max(Minimum, hullTons / Divisor);
    }
}
