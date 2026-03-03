using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Utils;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators.Planet;

/// <summary>
/// Generates atmosphere properties for planets.
/// </summary>
public static class PlanetAtmosphereGenerator
{
    /// <summary>
    /// Earth's atmospheric pressure in Pascals.
    /// </summary>
    public const double EarthAtmospherePa = 101325.0;

    private const double BoltzmannK = 1.380649e-23;
    private const double HydrogenMassKg = 1.6735575e-27;

    /// <summary>
    /// Generates atmosphere properties for a planet.
    /// </summary>
    public static AtmosphereProps GenerateAtmosphere(
        PlanetSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        OrbitZone.Zone zone,
        double equilibriumTempK,
        SeededRng rng)
    {
        double surfacePressurePa = CalculateSurfacePressure(spec, sizeCategory, rng);
        Dictionary composition = GenerateAtmosphereComposition(sizeCategory, zone, equilibriumTempK, rng);

        double averageMolecularMass = AtmosphereUtils.GetAverageMolecularMass(composition);
        double gravity = physical.GetSurfaceGravityMS2();
        double scaleHeightM = 0.0;
        if (gravity > 0.0 && averageMolecularMass > 0.0)
        {
            scaleHeightM = BoltzmannK * equilibriumTempK / (averageMolecularMass * gravity);
        }

        double greenhouseFactor = CalculateGreenhouseFactor(composition, surfacePressurePa, rng);
        return new AtmosphereProps(surfacePressurePa, scaleHeightM, composition, greenhouseFactor);
    }

    /// <summary>
    /// Returns whether the planet should have an atmosphere.
    /// </summary>
    public static bool ShouldHaveAtmosphere(
        PlanetSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        ParentContext context,
        SeededRng rng)
    {
        if (spec.HasAtmospherePreference())
        {
            return (bool)spec.HasAtmosphere;
        }

        if (SizeCategory.IsGaseous(sizeCategory))
        {
            return true;
        }

        if (!CanRetainAtmosphere(physical, sizeCategory, context))
        {
            return false;
        }

        return rng.Randf() < GetAtmosphereProbability(sizeCategory);
    }

    private static bool CanRetainAtmosphere(
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        ParentContext context)
    {
        double escapeVelocity = physical.GetEscapeVelocityMS();
        double equilibriumTemp = context.GetEquilibriumTemperatureK(0.3);
        double thermalVelocity = System.Math.Sqrt(3.0 * BoltzmannK * equilibriumTemp / HydrogenMassKg);
        double jeansParameter = escapeVelocity / thermalVelocity;

        if (sizeCategory == SizeCategory.Category.Dwarf)
        {
            return jeansParameter > 10.0;
        }

        if (SizeCategory.IsRocky(sizeCategory))
        {
            return jeansParameter > 4.0;
        }

        return true;
    }

    private static double GetAtmosphereProbability(SizeCategory.Category sizeCategory)
    {
        return sizeCategory switch
        {
            SizeCategory.Category.Dwarf => 0.1,
            SizeCategory.Category.SubTerrestrial => 0.4,
            SizeCategory.Category.Terrestrial => 0.8,
            SizeCategory.Category.SuperEarth => 0.95,
            _ => 1.0,
        };
    }

    private static double CalculateSurfacePressure(
        PlanetSpec spec,
        SizeCategory.Category sizeCategory,
        SeededRng rng)
    {
        double overridePressure = spec.GetOverrideFloat("atmosphere.surface_pressure_pa", -1.0);
        if (overridePressure >= 0.0)
        {
            return overridePressure;
        }

        return sizeCategory switch
        {
            SizeCategory.Category.Dwarf => rng.RandfRange(0.1f, 100.0f),
            SizeCategory.Category.SubTerrestrial => rng.RandfRange(100.0f, 10000.0f),
            SizeCategory.Category.Terrestrial => System.Math.Pow(10.0, rng.RandfRange(3.0f, 7.0f)),
            SizeCategory.Category.SuperEarth => System.Math.Pow(10.0, rng.RandfRange(4.0f, 8.0f)),
            SizeCategory.Category.MiniNeptune or SizeCategory.Category.NeptuneClass or SizeCategory.Category.GasGiant => rng.RandfRange(0.5e5f, 2.0e5f),
            _ => EarthAtmospherePa,
        };
    }

    private static Dictionary GenerateAtmosphereComposition(
        SizeCategory.Category sizeCategory,
        OrbitZone.Zone zone,
        double equilibriumTempK,
        SeededRng rng)
    {
        Dictionary composition = SizeCategory.IsGaseous(sizeCategory)
            ? GenerateGasGiantComposition(sizeCategory, rng)
            : GenerateRockyAtmosphereComposition(zone, equilibriumTempK, rng);

        double total = 0.0;
        foreach (Godot.Variant fraction in composition.Values)
        {
            total += (double)fraction;
        }

        if (total > 0.0)
        {
            foreach (Godot.Variant gas in composition.Keys)
            {
                composition[gas] = (double)composition[gas] / total;
            }
        }

        return composition;
    }

    private static Dictionary GenerateGasGiantComposition(SizeCategory.Category sizeCategory, SeededRng rng)
    {
        Dictionary composition = new();
        if (sizeCategory == SizeCategory.Category.GasGiant)
        {
            composition["H2"] = rng.RandfRange(0.82f, 0.92f);
            composition["He"] = rng.RandfRange(0.06f, 0.12f);
            composition["CH4"] = rng.RandfRange(0.001f, 0.005f);
            composition["NH3"] = rng.RandfRange(0.0001f, 0.001f);
        }
        else
        {
            composition["H2"] = rng.RandfRange(0.70f, 0.85f);
            composition["He"] = rng.RandfRange(0.10f, 0.20f);
            composition["CH4"] = rng.RandfRange(0.01f, 0.05f);
            composition["H2O"] = rng.RandfRange(0.001f, 0.01f);
        }

        return composition;
    }

    private static Dictionary GenerateRockyAtmosphereComposition(
        OrbitZone.Zone zone,
        double equilibriumTempK,
        SeededRng rng)
    {
        Dictionary composition = new();
        double roll = rng.Randf();

        if (zone == OrbitZone.Zone.Hot || equilibriumTempK > 500.0)
        {
            composition["CO2"] = rng.RandfRange(0.80f, 0.98f);
            composition["N2"] = rng.RandfRange(0.01f, 0.15f);
            composition["SO2"] = rng.RandfRange(0.001f, 0.05f);
        }
        else if (zone == OrbitZone.Zone.Temperate)
        {
            if (roll < 0.3)
            {
                composition["N2"] = rng.RandfRange(0.70f, 0.80f);
                composition["O2"] = rng.RandfRange(0.15f, 0.25f);
                composition["Ar"] = rng.RandfRange(0.005f, 0.02f);
                composition["CO2"] = rng.RandfRange(0.0001f, 0.001f);
                composition["H2O"] = rng.RandfRange(0.001f, 0.04f);
            }
            else if (roll < 0.7)
            {
                composition["CO2"] = rng.RandfRange(0.90f, 0.98f);
                composition["N2"] = rng.RandfRange(0.02f, 0.08f);
                composition["SO2"] = rng.RandfRange(0.0001f, 0.001f);
            }
            else
            {
                composition["CO2"] = rng.RandfRange(0.90f, 0.97f);
                composition["N2"] = rng.RandfRange(0.02f, 0.05f);
                composition["Ar"] = rng.RandfRange(0.01f, 0.03f);
            }
        }
        else
        {
            if (roll < 0.4)
            {
                composition["N2"] = rng.RandfRange(0.90f, 0.98f);
                composition["CH4"] = rng.RandfRange(0.01f, 0.06f);
            }
            else
            {
                composition["CO2"] = rng.RandfRange(0.85f, 0.95f);
                composition["N2"] = rng.RandfRange(0.03f, 0.10f);
            }
        }

        return composition;
    }

    private static double CalculateGreenhouseFactor(Dictionary composition, double surfacePressurePa, SeededRng rng)
    {
        double co2Fraction = composition.ContainsKey("CO2") ? (double)composition["CO2"] : 0.0;
        double ch4Fraction = composition.ContainsKey("CH4") ? (double)composition["CH4"] : 0.0;
        double h2oFraction = composition.ContainsKey("H2O") ? (double)composition["H2O"] : 0.0;

        double pressureRatio = System.Math.Max(surfacePressurePa / EarthAtmospherePa, 0.001);
        double pressureFactor = System.Math.Log(pressureRatio) / System.Math.Log(10.0);
        pressureFactor = System.Math.Clamp(pressureFactor, -2.0, 3.0);

        double greenhouse = 1.0;
        if (co2Fraction > 0.0)
        {
            greenhouse += 0.1
                * (System.Math.Log(co2Fraction * 1e6 + 1.0) / System.Math.Log(10.0))
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
        return System.Math.Clamp(greenhouse, 1.0, 3.0);
    }
}
