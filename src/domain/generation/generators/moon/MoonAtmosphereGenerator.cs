using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Utils;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators.Moon;

/// <summary>
/// Generates atmosphere properties for moons.
/// </summary>
public static class MoonAtmosphereGenerator
{
    private const double BoltzmannK = 1.380649e-23;
    private const double NitrogenMassKg = 4.6518e-26;

    /// <summary>
    /// Generates atmosphere properties for a moon.
    /// </summary>
    public static AtmosphereProps GenerateAtmosphere(
        MoonSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        double equilibriumTempK,
        SeededRng rng)
    {
        double surfacePressurePa = spec.GetOverrideFloat("atmosphere.surface_pressure_pa", -1.0);
        if (surfacePressurePa < 0.0)
        {
            surfacePressurePa = sizeCategory switch
            {
                SizeCategory.Category.SubTerrestrial => rng.RandfRange(1000.0f, 200000.0f),
                SizeCategory.Category.Terrestrial => rng.RandfRange(10000.0f, 500000.0f),
                SizeCategory.Category.SuperEarth => rng.RandfRange(50000.0f, 1000000.0f),
                _ => rng.RandfRange(100.0f, 10000.0f),
            };
        }

        Dictionary composition = new();
        if (equilibriumTempK < 200.0)
        {
            composition["N2"] = rng.RandfRange(0.90f, 0.98f);
            composition["CH4"] = rng.RandfRange(0.01f, 0.06f);
            composition["Ar"] = rng.RandfRange(0.001f, 0.01f);
        }
        else
        {
            double roll = rng.Randf();
            if (roll < 0.5)
            {
                composition["N2"] = rng.RandfRange(0.70f, 0.90f);
                composition["CO2"] = rng.RandfRange(0.05f, 0.20f);
            }
            else
            {
                composition["CO2"] = rng.RandfRange(0.85f, 0.95f);
                composition["N2"] = rng.RandfRange(0.03f, 0.10f);
            }
        }

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

        double averageMolecularMass = AtmosphereUtils.GetAverageMolecularMass(composition);
        double gravity = physical.GetSurfaceGravityMS2();
        double scaleHeightM = 0.0;
        if (gravity > 0.0 && averageMolecularMass > 0.0)
        {
            scaleHeightM = BoltzmannK * equilibriumTempK / (averageMolecularMass * gravity);
        }

        double greenhouseFactor = AtmosphereUtils.CalculateGreenhouseFactor(composition, surfacePressurePa, rng);
        return new AtmosphereProps(
            surfacePressurePa,
            scaleHeightM,
            composition,
            greenhouseFactor);
    }

    /// <summary>
    /// Returns whether the moon should have an atmosphere.
    /// </summary>
    public static bool ShouldHaveAtmosphere(
        MoonSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        ParentContext context,
        SeededRng rng)
    {
        if (spec.HasAtmospherePreference())
        {
            return (bool)spec.HasAtmosphere;
        }

        double escapeVelocity = physical.GetEscapeVelocityMS();
        double equilibriumTemp = context.GetEquilibriumTemperatureK(0.3);
        double thermalVelocity = System.Math.Sqrt(
            3.0 * BoltzmannK * equilibriumTemp / NitrogenMassKg);
        double jeansParameter = escapeVelocity / thermalVelocity;

        if (jeansParameter < 6.0 || sizeCategory == SizeCategory.Category.Dwarf)
        {
            return false;
        }

        double probability = sizeCategory switch
        {
            SizeCategory.Category.SubTerrestrial => 0.1,
            SizeCategory.Category.Terrestrial => 0.3,
            SizeCategory.Category.SuperEarth => 0.5,
            _ => 0.0,
        };

        return rng.Randf() < probability;
    }
}
