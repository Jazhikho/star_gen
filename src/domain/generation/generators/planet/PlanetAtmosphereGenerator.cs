using System;
using Godot;
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
    /// Aliased from <see cref="AtmosphereUtils.EarthAtmospherePa"/> for convenience.
    /// </summary>
    public const double EarthAtmospherePa = AtmosphereUtils.EarthAtmospherePa;

    private const double BoltzmannK = 1.380649e-23;

    /// <summary>
    /// Mass of one H2 molecule in kg; used for gas-giant Jeans criterion.
    /// </summary>
    private const double HydrogenMassKg = 1.6735575e-27;

    /// <summary>
    /// Mass of one N2 molecule in kg (~2 × 14 Da); used for rocky-body atmosphere
    /// retention. Rocky planets rarely retain H2 — N2 (the dominant terrestrial
    /// atmospheric species) is the appropriate test molecule.
    /// 1 Da = 1.66053906660e-27 kg; N2 ≈ 28 Da.
    /// </summary>
    private const double NitrogenMassKg = 28.0 * 1.66053906660e-27;

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
        surfacePressurePa = ApplyFormationPressureModifiers(spec, sizeCategory, surfacePressurePa);
        Dictionary composition = GenerateAtmosphereComposition(spec, sizeCategory, zone, equilibriumTempK, rng);

        double averageMolecularMass = AtmosphereUtils.GetAverageMolecularMass(composition);
        double gravity = physical.GetSurfaceGravityMS2();
        double scaleHeightM = 0.0;
        if (gravity > 0.0 && averageMolecularMass > 0.0)
        {
            scaleHeightM = BoltzmannK * equilibriumTempK / (averageMolecularMass * gravity);
        }

        double greenhouseFactor = AtmosphereUtils.CalculateGreenhouseFactor(composition, surfacePressurePa, rng);
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

        double probability = GetAtmosphereProbability(sizeCategory);
        probability = ApplyFormationAtmosphereProbabilityModifiers(spec, probability, context);
        return rng.Randf() < System.Math.Clamp(probability, 0.0, 1.0);
    }

    /// <summary>
    /// Returns whether the body can retain an atmosphere via the Jeans criterion.
    /// Rocky bodies are tested against N2 (the dominant terrestrial atmospheric
    /// molecule, ~28 Da) because they cannot retain H2 — using H2 as the test
    /// molecule would incorrectly prevent atmospheres on Earth-mass rocky worlds.
    /// Dwarf bodies use N2 as well but require a higher Jeans parameter.
    /// Gas giants trivially retain atmospheres and skip the check.
    /// Jeans parameter λ = v_esc / v_thermal; atmosphere is retained when λ > threshold.
    /// Thresholds after Pierrehumbert (2010) and Catling &amp; Kasting (2017).
    /// </summary>
    private static bool CanRetainAtmosphere(
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        ParentContext context)
    {
        double escapeVelocity = physical.GetEscapeVelocityMS();
        double equilibriumTemp = context.GetEquilibriumTemperatureK(0.3);

        if (sizeCategory == SizeCategory.Category.Dwarf)
        {
            double thermalVelocity = System.Math.Sqrt(3.0 * BoltzmannK * equilibriumTemp / NitrogenMassKg);
            return (escapeVelocity / thermalVelocity) > 10.0;
        }

        if (SizeCategory.IsRocky(sizeCategory))
        {
            double thermalVelocity = System.Math.Sqrt(3.0 * BoltzmannK * equilibriumTemp / NitrogenMassKg);
            return (escapeVelocity / thermalVelocity) > 4.0;
        }

        return true;
    }

    /// <summary>Returns probability of having an atmosphere by size category.</summary>
    private static double GetAtmosphereProbability(SizeCategory.Category sizeCategory)
    {
        return sizeCategory switch
        {
            SizeCategory.Category.Dwarf => 0.1,
            SizeCategory.Category.SubTerrestrial => 0.4,
            SizeCategory.Category.Terrestrial => 0.8,
            SizeCategory.Category.SuperEarth => 0.95,
            _ => throw new InvalidOperationException($"PlanetAtmosphereGenerator.GetAtmosphereProbability: unrecognized size category '{sizeCategory}'."),
        };
    }

    /// <summary>Computes surface pressure in Pa from size and overrides.</summary>
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
            _ => throw new InvalidOperationException($"PlanetAtmosphereGenerator.CalculateSurfacePressure: unrecognized size category '{sizeCategory}'."),
        };
    }

    private static double ApplyFormationPressureModifiers(
        PlanetSpec spec,
        SizeCategory.Category sizeCategory,
        double surfacePressurePa)
    {
        if (SizeCategory.IsGaseous(sizeCategory))
        {
            return surfacePressurePa;
        }

        double fluxEarth = ReadFormationDouble(spec, "stellar_flux_earth", 1.0);
        double xuvActivity = ReadFormationDouble(spec, "xuv_activity_scalar", 1.0);
        double volatileDelivery = ReadFormationDouble(spec, "volatile_delivery_scalar", 1.0);
        double habitableAlignment = ReadFormationDouble(spec, "habitable_zone_alignment", 0.0);
        bool insideLossRegime = ReadFormationBool(spec, "inside_radius_valley_regime", false);
        double modifier = 1.0;

        if (spec.ClassBias == PlanetClassBias.StrippedCore || spec.EnvelopeOverride == PlanetEnvelopeOverride.Stripped)
        {
            modifier *= 0.05;
        }
        else if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Thin)
        {
            modifier *= 0.45;
        }
        else if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Retained)
        {
            modifier *= 1.35;
        }

        if (insideLossRegime || (fluxEarth * xuvActivity) > 2.2)
        {
            modifier *= 0.55;
        }

        if (volatileDelivery >= 1.15)
        {
            modifier *= 1.20;
        }

        if (habitableAlignment >= 0.55)
        {
            modifier *= 1.10;
        }

        return surfacePressurePa * System.Math.Clamp(modifier, 0.03, 3.0);
    }

    /// <summary>Builds normalized gas composition for gas giants or rocky atmospheres.</summary>
    private static Dictionary GenerateAtmosphereComposition(
        PlanetSpec spec,
        SizeCategory.Category sizeCategory,
        OrbitZone.Zone zone,
        double equilibriumTempK,
        SeededRng rng)
    {
        Dictionary composition;
        if (SizeCategory.IsGaseous(sizeCategory))
        {
            composition = GenerateGasGiantComposition(sizeCategory, rng);
        }
        else
        {
            composition = GenerateRockyAtmosphereComposition(spec, zone, equilibriumTempK, rng);
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

        return composition;
    }

    /// <summary>Generates H2/He-dominated composition for gas giants.</summary>
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

    /// <summary>Generates composition for rocky planet atmospheres by zone.</summary>
    private static Dictionary GenerateRockyAtmosphereComposition(
        PlanetSpec spec,
        OrbitZone.Zone zone,
        double equilibriumTempK,
        SeededRng rng)
    {
        Dictionary composition = new();
        double roll = rng.Randf();
        double fluxEarth = ReadFormationDouble(spec, "stellar_flux_earth", zone == OrbitZone.Zone.Hot ? 4.0 : 1.0);
        double volatileDelivery = ReadFormationDouble(spec, "volatile_delivery_scalar", 1.0);
        double habitableAlignment = ReadFormationDouble(spec, "habitable_zone_alignment", 0.0);
        bool stripped = spec.ClassBias == PlanetClassBias.StrippedCore
            || spec.EnvelopeOverride == PlanetEnvelopeOverride.Stripped
            || ReadFormationBool(spec, "inside_radius_valley_regime", false);

        if (stripped || fluxEarth > 3.5)
        {
            composition["CO2"] = rng.RandfRange(0.70f, 0.94f);
            composition["N2"] = rng.RandfRange(0.04f, 0.22f);
            composition["SO2"] = rng.RandfRange(0.001f, 0.04f);
            return composition;
        }

        if (zone == OrbitZone.Zone.Hot || equilibriumTempK > 500.0)
        {
            composition["CO2"] = rng.RandfRange(0.80f, 0.98f);
            composition["N2"] = rng.RandfRange(0.01f, 0.15f);
            composition["SO2"] = rng.RandfRange(0.001f, 0.05f);
        }
        else if (habitableAlignment > 0.55 && volatileDelivery >= 1.0)
        {
            composition["N2"] = rng.RandfRange(0.62f, 0.80f);
            composition["H2O"] = rng.RandfRange(0.01f, 0.07f);
            if (roll < 0.20)
            {
                composition["O2"] = rng.RandfRange(0.12f, 0.24f);
                composition["CO2"] = rng.RandfRange(0.0002f, 0.01f);
            }
            else if (roll < 0.65)
            {
                composition["CO2"] = rng.RandfRange(0.01f, 0.12f);
                composition["Ar"] = rng.RandfRange(0.005f, 0.03f);
            }
            else
            {
                composition["CO2"] = rng.RandfRange(0.12f, 0.30f);
                composition["Ar"] = rng.RandfRange(0.005f, 0.02f);
            }
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
            if (volatileDelivery > 1.05 && roll < 0.60)
            {
                composition["N2"] = rng.RandfRange(0.70f, 0.88f);
                composition["CH4"] = rng.RandfRange(0.04f, 0.14f);
                composition["CO2"] = rng.RandfRange(0.02f, 0.10f);
            }
            else if (roll < 0.4)
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

    private static double ApplyFormationAtmosphereProbabilityModifiers(
        PlanetSpec spec,
        double probability,
        ParentContext context)
    {
        double fluxEarth = ReadFormationDouble(spec, "stellar_flux_earth", 1.0);
        double xuvActivity = ReadFormationDouble(spec, "xuv_activity_scalar", 1.0);
        double volatileDelivery = ReadFormationDouble(spec, "volatile_delivery_scalar", 1.0);
        double habitableAlignment = ReadFormationDouble(spec, "habitable_zone_alignment", 0.0);
        bool insideLossRegime = ReadFormationBool(spec, "inside_radius_valley_regime", false);
        double modifier = 1.0;

        if (spec.ClassBias == PlanetClassBias.StrippedCore)
        {
            modifier *= 0.28;
        }

        if (insideLossRegime || (fluxEarth * xuvActivity) > 2.2)
        {
            modifier *= 0.55;
        }

        if (volatileDelivery >= 1.15)
        {
            modifier *= 1.18;
        }

        if (habitableAlignment >= 0.55)
        {
            modifier *= 1.08;
        }

        if (context.StellarAgeYears > 0.0 && context.StellarAgeYears < 7.5e8)
        {
            modifier *= 0.92;
        }

        return probability * modifier;
    }

    private static double ReadFormationDouble(PlanetSpec spec, string key, double fallback)
    {
        if (spec.FormationTrace.ContainsKey(key))
        {
            Variant value = spec.FormationTrace[key];
            if (value.VariantType == Variant.Type.Float)
            {
                return (double)value;
            }

            if (value.VariantType == Variant.Type.Int)
            {
                return (int)value;
            }
        }

        return fallback;
    }

    private static bool ReadFormationBool(PlanetSpec spec, string key, bool fallback)
    {
        if (spec.FormationTrace.ContainsKey(key) && spec.FormationTrace[key].VariantType == Variant.Type.Bool)
        {
            return (bool)spec.FormationTrace[key];
        }

        return fallback;
    }

}
