using StarGen.Domain.Generation.Tables;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Deterministic stellar-property approximations inspired by supported isochrone families.
/// </summary>
public static class StellarIsochroneApproximator
{
    /// <summary>
    /// Resolves luminosity, radius, and temperature from stellar mass, age, metallicity, and model choice.
    /// </summary>
    public static StellarModelResult Resolve(
        StellarGenerationProfile profile,
        double massSolar,
        double ageYears,
        double metallicitySolar)
    {
        double lifetimeYears = EstimateMainSequenceLifetimeYears(massSolar);
        double ageFraction = 0.0;
        if (lifetimeYears > 0.0)
        {
            ageFraction = System.Math.Clamp(ageYears / lifetimeYears, 0.0, 0.95);
        }

        double metallicityOffset = 0.0;
        if (metallicitySolar > 0.0)
        {
            metallicityOffset = System.Math.Log10(metallicitySolar);
        }

        double luminositySolar = StarTable.LuminosityFromMass(massSolar);
        double radiusSolar = StarTable.RadiusFromMass(massSolar);

        double luminosityMultiplier = 1.0;
        double radiusMultiplier = 1.0;
        double temperatureMultiplier = 1.0;

        if (profile.IsochroneModel == StellarIsochroneModel.Parsec)
        {
            luminosityMultiplier += (ageFraction * 0.14) + (metallicityOffset * 0.03);
            radiusMultiplier += (ageFraction * 0.11) + (metallicityOffset * 0.04);
            temperatureMultiplier -= (ageFraction * 0.030) + (metallicityOffset * 0.015);
        }
        else
        {
            luminosityMultiplier += (ageFraction * 0.18) + (metallicityOffset * 0.05);
            radiusMultiplier += (ageFraction * 0.09) + (metallicityOffset * 0.03);
            temperatureMultiplier -= (ageFraction * 0.025) + (metallicityOffset * 0.010);
        }

        luminositySolar *= System.Math.Clamp(luminosityMultiplier, 0.05, 1000.0);
        radiusSolar *= System.Math.Clamp(radiusMultiplier, 0.1, 100.0);
        double temperatureK = StarTable.TemperatureFromLuminosityRadius(luminositySolar, radiusSolar);
        temperatureK *= System.Math.Clamp(temperatureMultiplier, 0.65, 1.25);

        return new StellarModelResult(luminositySolar, radiusSolar, temperatureK, ageFraction, lifetimeYears);
    }

    /// <summary>
    /// Estimates a main-sequence lifetime in years from stellar mass.
    /// </summary>
    public static double EstimateMainSequenceLifetimeYears(double massSolar)
    {
        if (massSolar <= 0.0)
        {
            return 1.0e10;
        }

        double exponent = 2.5;
        if (massSolar < 0.45)
        {
            exponent = 2.1;
        }
        else if (massSolar > 8.0)
        {
            exponent = 2.9;
        }

        return 1.0e10 / System.Math.Pow(massSolar, exponent);
    }
}

/// <summary>
/// Resolved stellar model outputs.
/// </summary>
public readonly struct StellarModelResult
{
    /// <summary>
    /// Luminosity in solar units.
    /// </summary>
    public double LuminositySolar { get; }

    /// <summary>
    /// Radius in solar units.
    /// </summary>
    public double RadiusSolar { get; }

    /// <summary>
    /// Effective temperature in Kelvin.
    /// </summary>
    public double TemperatureK { get; }

    /// <summary>
    /// Age fraction along the approximated main-sequence lifetime.
    /// </summary>
    public double AgeFraction { get; }

    /// <summary>
    /// Estimated main-sequence lifetime in years.
    /// </summary>
    public double LifetimeYears { get; }

    /// <summary>
    /// Creates a result record.
    /// </summary>
    public StellarModelResult(double luminositySolar, double radiusSolar, double temperatureK, double ageFraction, double lifetimeYears)
    {
        LuminositySolar = luminositySolar;
        RadiusSolar = radiusSolar;
        TemperatureK = temperatureK;
        AgeFraction = ageFraction;
        LifetimeYears = lifetimeYears;
    }
}
