using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Rng;

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
            double mass;
            if (masses.ContainsKey(gas))
            {
                mass = (double)masses[gas];
            }
            else
            {
                mass = DefaultNitrogenMassKg;
            }
            averageMass += fraction * mass;
            totalFraction += fraction;
        }

        if (totalFraction > 0.0)
        {
            return averageMass / totalFraction;
        }

        return DefaultNitrogenMassKg;
    }

    /// <summary>
    /// Calculates surface temperature from equilibrium temperature and atmosphere.
    /// </summary>
    public static double CalculateSurfaceTemperature(double equilibriumTempK, AtmosphereProps? atmosphere)
    {
        if (atmosphere != null)
        {
            return equilibriumTempK * atmosphere.GreenhouseFactor;
        }

        return equilibriumTempK;
    }

    /// <summary>
    /// Computes the greenhouse warming factor from atmospheric composition and pressure.
    /// Based on logarithmic CO2 radiative forcing, CH4 potency, and H2O feedback,
    /// following parameterisations in Pierrehumbert (2010) and Kopparapu et al. (2013).
    /// The factor multiplies the radiative equilibrium temperature to give surface temperature.
    /// A small random variation (±10%) represents unmodelled cloud and albedo feedbacks.
    /// Cap of 3.5 allows Venus-like worlds (~3.2 for Venus).
    /// </summary>
    /// <param name="composition">Mole-fraction dictionary keyed by gas species (e.g. "CO2", "CH4", "H2O").</param>
    /// <param name="surfacePressurePa">Surface pressure in Pascals.</param>
    /// <param name="rng">Seeded RNG for the ±10 % variation term.</param>
    /// <returns>Greenhouse factor ≥ 1.0, clamped to [1.0, 3.5].</returns>
    public static double CalculateGreenhouseFactor(Dictionary composition, double surfacePressurePa, SeededRng rng)
    {
        double co2Fraction = 0.0;
        if (composition.ContainsKey("CO2"))
        {
            co2Fraction = (double)composition["CO2"];
        }

        double ch4Fraction = 0.0;
        if (composition.ContainsKey("CH4"))
        {
            ch4Fraction = (double)composition["CH4"];
        }

        double h2oFraction = 0.0;
        if (composition.ContainsKey("H2O"))
        {
            h2oFraction = (double)composition["H2O"];
        }

        double pressureRatio = System.Math.Max(surfacePressurePa / EarthAtmospherePa, 0.001);
        double pressureFactor = System.Math.Log10(pressureRatio);
        pressureFactor = System.Math.Clamp(pressureFactor, -2.0, 3.0);

        double greenhouse = 1.0;
        if (co2Fraction > 0.0)
        {
            greenhouse += 0.1
                * System.Math.Log10(co2Fraction * 1e6 + 1.0)
                * (1.0 + pressureFactor * 0.3);
        }

        if (ch4Fraction > 0.0)
        {
            greenhouse += ch4Fraction * 25.0;
        }

        if (h2oFraction > 0.0)
        {
            greenhouse += h2oFraction * 2.0;
        }

        greenhouse *= rng.RandfRange(0.9f, 1.1f);
        return System.Math.Clamp(greenhouse, 1.0, 3.5);
    }

    /// <summary>
    /// Earth's surface pressure in Pascals; used as the reference for greenhouse calculations.
    /// </summary>
    public const double EarthAtmospherePa = 101325.0;
}
