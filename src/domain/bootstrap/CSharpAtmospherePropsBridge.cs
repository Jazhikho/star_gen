using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for atmosphere-property calculations.
/// </summary>
[GlobalClass]
public partial class CSharpAtmospherePropsBridge : RefCounted
{
    /// <summary>
    /// Calculates the sum of composition fractions.
    /// </summary>
    public double GetCompositionSum(Dictionary composition) => new AtmosphereProps(composition: composition).GetCompositionSum();

    /// <summary>
    /// Returns the dominant gas in the supplied composition.
    /// </summary>
    public string GetDominantGas(Dictionary composition) => new AtmosphereProps(composition: composition).GetDominantGas();
}
