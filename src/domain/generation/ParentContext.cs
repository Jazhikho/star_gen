using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;

namespace StarGen.Domain.Generation;

/// <summary>
/// Provides parent-body context for generation without object references.
/// </summary>
public partial class ParentContext : Godot.RefCounted
{
    private const double StefanBoltzmann = 5.67e-8;

    /// <summary>
    /// Mass of the parent star in kilograms.
    /// </summary>
    public double StellarMassKg;

    /// <summary>
    /// Luminosity of the parent star in watts.
    /// </summary>
    public double StellarLuminosityWatts;

    /// <summary>
    /// Effective temperature of the parent star in Kelvin.
    /// </summary>
    public double StellarTemperatureK;

    /// <summary>
    /// Age of the parent star in years.
    /// </summary>
    public double StellarAgeYears;

    /// <summary>
    /// Distance from the star in meters.
    /// </summary>
    public double OrbitalDistanceFromStarM;

    /// <summary>
    /// Mass of the parent body in kilograms.
    /// </summary>
    public double ParentBodyMassKg;

    /// <summary>
    /// Radius of the parent body in meters.
    /// </summary>
    public double ParentBodyRadiusM;

    /// <summary>
    /// Distance from the parent body in meters.
    /// </summary>
    public double OrbitalDistanceFromParentM;

    /// <summary>
    /// Creates a new parent-context instance.
    /// </summary>
    public ParentContext(
        double stellarMassKg = 0.0,
        double stellarLuminosityWatts = 0.0,
        double stellarTemperatureK = 0.0,
        double stellarAgeYears = 0.0,
        double orbitalDistanceFromStarM = 0.0,
        double parentBodyMassKg = 0.0,
        double parentBodyRadiusM = 0.0,
        double orbitalDistanceFromParentM = 0.0)
    {
        StellarMassKg = stellarMassKg;
        StellarLuminosityWatts = stellarLuminosityWatts;
        StellarTemperatureK = stellarTemperatureK;
        StellarAgeYears = stellarAgeYears;
        OrbitalDistanceFromStarM = orbitalDistanceFromStarM;
        ParentBodyMassKg = parentBodyMassKg;
        ParentBodyRadiusM = parentBodyRadiusM;
        OrbitalDistanceFromParentM = orbitalDistanceFromParentM;
    }

    /// <summary>
    /// Creates a context for a planet orbiting a star.
    /// </summary>
    public static ParentContext ForPlanet(
        double stellarMassKg,
        double stellarLuminosityWatts,
        double stellarTemperatureK,
        double stellarAgeYears,
        double orbitalDistanceM)
    {
        return new ParentContext(
            stellarMassKg,
            stellarLuminosityWatts,
            stellarTemperatureK,
            stellarAgeYears,
            orbitalDistanceM);
    }

    /// <summary>
    /// Creates a context for a moon orbiting a planet.
    /// </summary>
    public static ParentContext ForMoon(
        double stellarMassKg,
        double stellarLuminosityWatts,
        double stellarTemperatureK,
        double stellarAgeYears,
        double planetOrbitalDistanceM,
        double planetMassKg,
        double planetRadiusM,
        double moonOrbitalDistanceM)
    {
        return new ParentContext(
            stellarMassKg,
            stellarLuminosityWatts,
            stellarTemperatureK,
            stellarAgeYears,
            planetOrbitalDistanceM,
            planetMassKg,
            planetRadiusM,
            moonOrbitalDistanceM);
    }

    /// <summary>
    /// Creates a Sun-like default context.
    /// </summary>
    public static ParentContext SunLike(double orbitalDistanceM = Units.AuMeters)
    {
        return ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            orbitalDistanceM);
    }

    /// <summary>
    /// Returns whether this context has a parent body.
    /// </summary>
    public bool HasParentBody() => ParentBodyMassKg > 0.0;

    /// <summary>
    /// Calculates the Hill-sphere radius in meters.
    /// </summary>
    public double GetHillSphereRadiusM()
    {
        if (ParentBodyMassKg <= 0.0 || StellarMassKg <= 0.0 || OrbitalDistanceFromStarM <= 0.0)
        {
            return 0.0;
        }

        double massRatio = ParentBodyMassKg / (3.0 * StellarMassKg);
        return OrbitalDistanceFromStarM * System.Math.Pow(massRatio, 1.0 / 3.0);
    }

    /// <summary>
    /// Calculates the Roche limit in meters for the supplied satellite density.
    /// </summary>
    public double GetRocheLimitM(double satelliteDensityKgM3)
    {
        if (ParentBodyRadiusM <= 0.0 || ParentBodyMassKg <= 0.0 || satelliteDensityKgM3 <= 0.0)
        {
            return 0.0;
        }

        double parentDensity = ParentBodyMassKg / ((4.0 / 3.0) * System.Math.PI * System.Math.Pow(ParentBodyRadiusM, 3.0));
        return 2.44 * ParentBodyRadiusM * System.Math.Pow(parentDensity / satelliteDensityKgM3, 1.0 / 3.0);
    }

    /// <summary>
    /// Calculates equilibrium temperature in Kelvin at the orbital distance.
    /// </summary>
    public double GetEquilibriumTemperatureK(double albedo = 0.3)
    {
        if (StellarLuminosityWatts <= 0.0 || OrbitalDistanceFromStarM <= 0.0)
        {
            return 0.0;
        }

        double absorbed = StellarLuminosityWatts * (1.0 - albedo);
        double distanceFactor = 4.0 * System.Math.PI * System.Math.Pow(OrbitalDistanceFromStarM, 2.0);
        return System.Math.Pow(absorbed / (4.0 * distanceFactor * StefanBoltzmann), 0.25);
    }

    /// <summary>
    /// Converts this context to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["stellar_mass_kg"] = StellarMassKg,
            ["stellar_luminosity_watts"] = StellarLuminosityWatts,
            ["stellar_temperature_k"] = StellarTemperatureK,
            ["stellar_age_years"] = StellarAgeYears,
            ["orbital_distance_from_star_m"] = OrbitalDistanceFromStarM,
            ["parent_body_mass_kg"] = ParentBodyMassKg,
            ["parent_body_radius_m"] = ParentBodyRadiusM,
            ["orbital_distance_from_parent_m"] = OrbitalDistanceFromParentM,
        };
    }

    /// <summary>
    /// Creates a context from a dictionary payload.
    /// </summary>
    public static ParentContext FromDictionary(Dictionary data)
    {
        return new ParentContext(
            GetDouble(data, "stellar_mass_kg", 0.0),
            GetDouble(data, "stellar_luminosity_watts", 0.0),
            GetDouble(data, "stellar_temperature_k", 0.0),
            GetDouble(data, "stellar_age_years", 0.0),
            GetDouble(data, "orbital_distance_from_star_m", 0.0),
            GetDouble(data, "parent_body_mass_kg", 0.0),
            GetDouble(data, "parent_body_radius_m", 0.0),
            GetDouble(data, "orbital_distance_from_parent_m", 0.0));
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (data.ContainsKey(key))
        {
            return (double)data[key];
        }

        return fallback;
    }
}
