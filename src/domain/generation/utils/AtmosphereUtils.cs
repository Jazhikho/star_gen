using Godot.Collections;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Generation.Utils;

/// <summary>
/// Shared utility functions for atmosphere-related calculations.
/// </summary>
public static class AtmosphereUtils
{
    private const double AtomicMassUnitKg = 1.6605e-27;
    private const double DefaultNitrogenMassKg = 28.0 * AtomicMassUnitKg;

    /// <summary>
    /// Returns the average molecular mass for a gas mixture in kilograms.
    /// </summary>
    public static double GetAverageMolecularMass(Dictionary composition)
    {
        Dictionary masses = new()
        {
            ["H2"] = 2.016 * AtomicMassUnitKg,
            ["He"] = 4.003 * AtomicMassUnitKg,
            ["CH4"] = 16.04 * AtomicMassUnitKg,
            ["NH3"] = 17.03 * AtomicMassUnitKg,
            ["H2O"] = 18.02 * AtomicMassUnitKg,
            ["N2"] = 28.01 * AtomicMassUnitKg,
            ["O2"] = 32.00 * AtomicMassUnitKg,
            ["Ar"] = 39.95 * AtomicMassUnitKg,
            ["CO2"] = 44.01 * AtomicMassUnitKg,
            ["SO2"] = 64.07 * AtomicMassUnitKg,
        };

        double averageMass = 0.0;
        double totalFraction = 0.0;

        foreach (Godot.Variant gas in composition.Keys)
        {
            double fraction = (double)composition[gas];
            double mass = masses.ContainsKey(gas) ? (double)masses[gas] : DefaultNitrogenMassKg;
            averageMass += fraction * mass;
            totalFraction += fraction;
        }

        return totalFraction > 0.0 ? averageMass / totalFraction : DefaultNitrogenMassKg;
    }

    /// <summary>
    /// Calculates surface temperature from equilibrium temperature and atmosphere.
    /// </summary>
    public static double CalculateSurfaceTemperature(double equilibriumTempK, AtmosphereProps? atmosphere)
    {
        return atmosphere != null ? equilibriumTempK * atmosphere.GreenhouseFactor : equilibriumTempK;
    }
}
