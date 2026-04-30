using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Utils;
using StarGen.Domain.Math;
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
    private const string AtmosphereActiveSources = "WordsworthKreidberg2022;ChatterjeeEtAl2026;LugerBarnes2015;Kopparapu2013;Balbi2023";
    private const string AtmosphereContextSources = "VissapragadaEtAl2022;BiassoniEtAl2023";
    private const string AtmosphereUnderutilizedSources = "Kopparapu2014";

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
        RecordGeneratedAtmosphereTrace(spec, sizeCategory, surfacePressurePa, composition, greenhouseFactor);
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
            bool forcedResult = (bool)spec.HasAtmosphere;
            string forcedOutcome = "forced_airless";
            double forcedProbability = 0.0;
            if (forcedResult)
            {
                forcedOutcome = "forced_retained";
                forcedProbability = 1.0;
            }

            RecordRetentionTrace(
                spec,
                physical,
                sizeCategory,
                context,
                forcedProbability,
                forcedResult,
                -1.0,
                forcedOutcome);
            return forcedResult;
        }

        if (SizeCategory.IsGaseous(sizeCategory))
        {
            RecordRetentionTrace(
                spec,
                physical,
                sizeCategory,
                context,
                1.0,
                true,
                -1.0,
                "gaseous_h_he_envelope");
            return true;
        }

        if (!CanRetainAtmosphere(physical, sizeCategory, context))
        {
            RecordRetentionTrace(
                spec,
                physical,
                sizeCategory,
                context,
                0.0,
                false,
                -1.0,
                "jeans_escape_failure");
            return false;
        }

        double probability = GetAtmosphereProbability(sizeCategory);
        probability = ApplyFormationAtmosphereProbabilityModifiers(spec, probability, context);
        double clampedProbability = System.Math.Clamp(probability, 0.0, 1.0);
        double roll = rng.Randf();
        bool retained = roll < clampedProbability;
        string rollOutcome = "probability_roll_failed";
        if (retained)
        {
            rollOutcome = "retained_after_probability_roll";
        }

        RecordRetentionTrace(
            spec,
            physical,
            sizeCategory,
            context,
            clampedProbability,
            retained,
            roll,
            rollOutcome);
        return retained;
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

    private static void RecordRetentionTrace(
        PlanetSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        ParentContext context,
        double retentionProbability,
        bool retained,
        double retentionRoll,
        string outcome)
    {
        double escapePressure = CalculateSecondaryAtmosphereEscapePressure(spec, physical, sizeCategory, context);
        double retentionScalar = CalculateSecondaryAtmosphereRetentionScalar(spec, physical, escapePressure);
        double preMainSequenceRisk = CalculatePreMainSequenceXuvRisk(spec, context);

        spec.FormationTrace["atmosphere_retention_model"] = "stargen_secondary_atmosphere_retention_v1";
        spec.FormationTrace["atmosphere_active_sources"] = AtmosphereActiveSources;
        spec.FormationTrace["atmosphere_context_sources"] = AtmosphereContextSources;
        spec.FormationTrace["atmosphere_underutilized_sources"] = AtmosphereUnderutilizedSources;
        spec.FormationTrace["atmosphere_context_source_caveat"] = "VissapragadaEtAl2022 and BiassoniEtAl2023 are observational He-escape context pending human verification, not active default models.";
        spec.FormationTrace["atmosphere_retention_probability"] = retentionProbability;
        spec.FormationTrace["atmosphere_retention_roll"] = retentionRoll;
        spec.FormationTrace["atmosphere_retained"] = retained;
        spec.FormationTrace["atmosphere_retention_outcome"] = outcome;
        spec.FormationTrace["secondary_atmosphere_escape_pressure"] = escapePressure;
        spec.FormationTrace["secondary_atmosphere_retention_scalar"] = retentionScalar;
        spec.FormationTrace["pre_main_sequence_xuv_risk"] = preMainSequenceRisk;
        spec.FormationTrace["secondary_atmosphere_model_status"] = "heuristic_trace; full hydrodynamic escape and volcanic-revival model remains follow-up";

        if (!retained)
        {
            spec.FormationTrace["atmosphere_regime"] = "airless_or_trace_only";
            spec.FormationTrace["atmosphere_composition_family"] = "none";
        }
    }

    private static void RecordGeneratedAtmosphereTrace(
        PlanetSpec spec,
        SizeCategory.Category sizeCategory,
        double surfacePressurePa,
        Dictionary composition,
        double greenhouseFactor)
    {
        string compositionFamily = ClassifyCompositionFamily(composition);
        string regime = ClassifyAtmosphereRegime(spec, sizeCategory, surfacePressurePa, compositionFamily, composition);
        double oxygenFraction = ReadCompositionFraction(composition, "O2");
        double oxygenPartialPressureAtm = oxygenFraction * (surfacePressurePa / EarthAtmospherePa);
        string oxygenContext = ClassifyOxygenContext(spec, oxygenFraction, oxygenPartialPressureAtm);

        spec.FormationTrace["atmosphere_regime"] = regime;
        spec.FormationTrace["atmosphere_composition_family"] = compositionFamily;
        spec.FormationTrace["atmosphere_surface_pressure_pa"] = surfacePressurePa;
        spec.FormationTrace["atmosphere_greenhouse_factor"] = greenhouseFactor;
        spec.FormationTrace["oxygen_fraction"] = oxygenFraction;
        spec.FormationTrace["oxygen_partial_pressure_atm"] = oxygenPartialPressureAtm;
        spec.FormationTrace["oxygen_context"] = oxygenContext;
    }

    private static string ClassifyCompositionFamily(Dictionary composition)
    {
        double h2 = ReadCompositionFraction(composition, "H2");
        double he = ReadCompositionFraction(composition, "He");
        double co2 = ReadCompositionFraction(composition, "CO2");
        double n2 = ReadCompositionFraction(composition, "N2");
        double o2 = ReadCompositionFraction(composition, "O2");
        double h2o = ReadCompositionFraction(composition, "H2O");
        double ch4 = ReadCompositionFraction(composition, "CH4");
        double so2 = ReadCompositionFraction(composition, "SO2");

        if ((h2 + he) >= 0.60)
        {
            return "h_he_dominated";
        }

        if (co2 >= 0.65 && so2 >= 0.001)
        {
            return "co2_silicate_outgassing";
        }

        if (co2 >= 0.65)
        {
            return "co2_dominated_secondary";
        }

        if (n2 >= 0.50 && o2 >= 0.10)
        {
            return "n2_o2_secondary";
        }

        if (n2 >= 0.50 && h2o >= 0.01)
        {
            return "n2_h2o_secondary";
        }

        if (n2 >= 0.50 && ch4 >= 0.01)
        {
            return "cold_n2_ch4_secondary";
        }

        return "mixed_secondary";
    }

    private static string ClassifyAtmosphereRegime(
        PlanetSpec spec,
        SizeCategory.Category sizeCategory,
        double surfacePressurePa,
        string compositionFamily,
        Dictionary composition)
    {
        if (SizeCategory.IsGaseous(sizeCategory))
        {
            return "primordial_h_he_envelope";
        }

        if (surfacePressurePa < 1000.0)
        {
            return "tenuous_secondary_atmosphere";
        }

        double escapePressure = ReadFormationDouble(spec, "secondary_atmosphere_escape_pressure", 0.0);
        if (escapePressure >= 1.0 && surfacePressurePa < 25000.0)
        {
            return "secondary_atmosphere_vulnerable_or_rebuilt";
        }

        if (compositionFamily == "co2_silicate_outgassing")
        {
            return "hot_or_stripped_co2_secondary";
        }

        double oxygenFraction = ReadCompositionFraction(composition, "O2");
        double habitableAlignment = ReadFormationDouble(spec, "habitable_zone_alignment", 0.0);
        if (oxygenFraction >= 0.12 && habitableAlignment >= 0.55)
        {
            return "oxygenated_secondary_biological_candidate";
        }

        if (habitableAlignment >= 0.55)
        {
            return "secondary_n2_co2_hz_candidate";
        }

        if (compositionFamily == "cold_n2_ch4_secondary")
        {
            return "cold_volatile_secondary";
        }

        return "secondary_atmosphere";
    }

    private static string ClassifyOxygenContext(
        PlanetSpec spec,
        double oxygenFraction,
        double oxygenPartialPressureAtm)
    {
        if (oxygenFraction <= 0.001)
        {
            return "not_oxygenated";
        }

        double preMainSequenceRisk = ReadFormationDouble(spec, "pre_main_sequence_xuv_risk", 0.0);
        double volatileDelivery = ReadFormationDouble(spec, "volatile_delivery_scalar", 1.0);
        double escapePressure = ReadFormationDouble(spec, "secondary_atmosphere_escape_pressure", 0.0);
        if (preMainSequenceRisk >= 0.55 || escapePressure >= 1.15 || volatileDelivery < 0.70)
        {
            return "abiotic_o2_false_positive_risk";
        }

        if (oxygenPartialPressureAtm >= 0.18)
        {
            return "balbi_technosphere_o2_candidate";
        }

        if (oxygenPartialPressureAtm >= 0.005)
        {
            return "low_o2_biosphere_candidate";
        }

        return "trace_o2";
    }

    private static double CalculateSecondaryAtmosphereEscapePressure(
        PlanetSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        ParentContext context)
    {
        if (SizeCategory.IsGaseous(sizeCategory))
        {
            return 0.0;
        }

        double fluxEarth = ReadFormationDouble(spec, "stellar_flux_earth", EstimateFluxEarth(context));
        double xuvActivity = ReadFormationDouble(spec, "xuv_activity_scalar", 1.0);
        double gravityG = physical.GetSurfaceGravityMS2() / 9.80665;
        double massEarth = physical.MassKg / Units.EarthMassKg;
        double volatileDelivery = ReadFormationDouble(spec, "volatile_delivery_scalar", 1.0);
        bool insideRadiusValley = ReadFormationBool(spec, "inside_radius_valley_regime", false);
        double bindingScalar = System.Math.Max(0.25, System.Math.Sqrt(System.Math.Max(massEarth, 0.05)) * System.Math.Max(gravityG, 0.10));
        double pressure = (fluxEarth * xuvActivity) / bindingScalar;

        double preMainSequenceRisk = CalculatePreMainSequenceXuvRisk(spec, context);
        pressure *= 1.0 + (preMainSequenceRisk * 0.70);

        if (insideRadiusValley)
        {
            pressure *= 1.20;
        }

        if (volatileDelivery >= 1.15)
        {
            pressure *= 0.90;
        }

        if (context.StellarAgeYears > 0.0 && context.StellarAgeYears < 7.5e8)
        {
            pressure *= 1.15;
        }

        return System.Math.Clamp(pressure, 0.0, 6.0);
    }

    private static double CalculateSecondaryAtmosphereRetentionScalar(
        PlanetSpec spec,
        PhysicalProps physical,
        double escapePressure)
    {
        double volatileDelivery = ReadFormationDouble(spec, "volatile_delivery_scalar", 1.0);
        double gravityG = physical.GetSurfaceGravityMS2() / 9.80665;
        double retention = 1.0 - (escapePressure / 3.0);
        retention += (System.Math.Clamp(volatileDelivery, 0.0, 2.0) - 1.0) * 0.18;
        retention += (System.Math.Clamp(gravityG, 0.0, 2.0) - 1.0) * 0.12;
        return System.Math.Clamp(retention, 0.0, 1.0);
    }

    private static double CalculatePreMainSequenceXuvRisk(PlanetSpec spec, ParentContext context)
    {
        double temperatureK = context.StellarTemperatureK;
        double habitableAlignment = ReadFormationDouble(spec, "habitable_zone_alignment", 0.0);
        double fluxEarth = ReadFormationDouble(spec, "stellar_flux_earth", EstimateFluxEarth(context));
        double risk = 0.0;

        if (temperatureK > 0.0 && temperatureK <= 3900.0)
        {
            risk = 0.35 + ((3900.0 - temperatureK) / 1400.0) * 0.45;
            if (habitableAlignment >= 0.55)
            {
                risk += 0.20;
            }

            if (fluxEarth >= 1.0)
            {
                risk += 0.10;
            }

            if (temperatureK <= 3300.0)
            {
                risk += 0.15;
            }
        }
        else if (temperatureK > 3900.0 && temperatureK <= 4700.0 && habitableAlignment >= 0.55)
        {
            risk = 0.10;
        }

        if (context.StellarAgeYears > 0.0 && context.StellarAgeYears < 1.0e9)
        {
            risk += 0.10;
        }

        return System.Math.Clamp(risk, 0.0, 1.0);
    }

    private static double EstimateFluxEarth(ParentContext context)
    {
        if (context.StellarLuminosityWatts <= 0.0 || context.OrbitalDistanceFromStarM <= 0.0)
        {
            return 1.0;
        }

        double luminositySolar = context.StellarLuminosityWatts / StellarProps.SolarLuminosityWatts;
        double distanceAu = context.OrbitalDistanceFromStarM / Units.AuMeters;
        return luminositySolar / System.Math.Max(distanceAu * distanceAu, 0.0001);
    }

    private static double ReadCompositionFraction(Dictionary composition, string gas)
    {
        if (!composition.ContainsKey(gas))
        {
            return 0.0;
        }

        Variant value = composition[gas];
        if (value.VariantType == Variant.Type.Float)
        {
            return (double)value;
        }

        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        return 0.0;
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
