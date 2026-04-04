using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Generation;

/// <summary>
/// Derived aggregate planetary state built once per system from stellar context plus the shared profile.
/// </summary>
public partial class PlanetarySystemState : RefCounted
{
    /// <summary>
    /// Aggregate profile that produced this derived state.
    /// </summary>
    public PlanetaryGenerationProfile Profile { get; set; } = PlanetaryGenerationProfile.CreateDefault();

    /// <summary>
    /// Combined stellar mass in solar masses.
    /// </summary>
    public double StellarMassSolar { get; set; } = 1.0;

    /// <summary>
    /// Combined stellar luminosity in solar luminosities.
    /// </summary>
    public double StellarLuminositySolar { get; set; } = 1.0;

    /// <summary>
    /// Aggregate system metallicity used for planet weighting.
    /// </summary>
    public double SystemMetallicity { get; set; } = 1.0;

    /// <summary>
    /// Snow-line distance in AU.
    /// </summary>
    public double SnowLineAu { get; set; } = 2.7;

    /// <summary>
    /// Effective solids budget scalar after metallicity coupling.
    /// </summary>
    public double SolidBudgetScalar { get; set; } = 1.0;

    /// <summary>
    /// Effective gas budget scalar after lifetime and metallicity adjustments.
    /// </summary>
    public double GasBudgetScalar { get; set; } = 1.0;

    /// <summary>
    /// Escape or XUV stripping pressure proxy for close-in worlds.
    /// </summary>
    public double EscapePressureProxy { get; set; } = 1.0;

    /// <summary>
    /// Effective migration-strength surrogate.
    /// </summary>
    public double MigrationStrength { get; set; } = 1.0;

    /// <summary>
    /// Effective impact-stirring surrogate.
    /// </summary>
    public double ImpactStirring { get; set; } = 1.0;

    /// <summary>
    /// Effective metallicity enrichment scalar applied to gas-giant weighting.
    /// </summary>
    public double MetallicityEnrichment { get; set; } = 1.0;

    /// <summary>
    /// Effective gas-giant formation weight after aggregate model choices.
    /// </summary>
    public double GasGiantWeight { get; set; } = 1.0;

    /// <summary>
    /// Inner edge of the habitable zone in AU.
    /// </summary>
    public double HabitableZoneInnerAu { get; set; } = 0.95;

    /// <summary>
    /// Outer edge of the habitable zone in AU.
    /// </summary>
    public double HabitableZoneOuterAu { get; set; } = 1.37;

    /// <summary>
    /// Relative stellar high-energy activity proxy used for close-in atmosphere stripping.
    /// </summary>
    public double XuvActivityScalar { get; set; } = 1.0;

    /// <summary>
    /// How massive and comet-rich the outer volatile reservoir is likely to be.
    /// </summary>
    public double OuterReservoirScalar { get; set; } = 1.0;

    /// <summary>
    /// System-wide volatile-delivery proxy used for oceans, icy worlds, and atmosphere retention.
    /// </summary>
    public double VolatileDeliveryScalar { get; set; } = 1.0;

    /// <summary>
    /// Late bombardment and stirring proxy used for stripping and delivery events.
    /// </summary>
    public double BombardmentScalar { get; set; } = 1.0;

    /// <summary>
    /// Creates a default derived state.
    /// </summary>
    public static PlanetarySystemState CreateDefault()
    {
        return new PlanetarySystemState();
    }

    /// <summary>
    /// Builds a derived planetary-system state from the current stellar context and profile.
    /// </summary>
    public static PlanetarySystemState Build(SolarSystemSpec? spec, Array<CelestialBody>? stars)
    {
        PlanetaryGenerationProfile profile = spec?.PlanetaryProfile?.Clone() ?? PlanetaryGenerationProfile.CreateDefault();
        double massSolar = 0.0;
        double luminositySolar = 0.0;
        double metallicity = spec?.SystemMetallicity ?? -1.0;
        double ageYears = spec?.SystemAgeYears ?? -1.0;
        double effectiveTempK = 5778.0;

        if (stars != null)
        {
            foreach (CelestialBody star in stars)
            {
                massSolar += star.Physical.MassKg / Units.SolarMassKg;
                if (star.HasStellar())
                {
                    luminositySolar += star.Stellar!.LuminosityWatts / StellarProps.SolarLuminosityWatts;
                    if (metallicity <= 0.0)
                    {
                        metallicity = star.Stellar.Metallicity;
                    }

                    if (effectiveTempK <= 0.0 || effectiveTempK == 5778.0)
                    {
                        effectiveTempK = star.Stellar.EffectiveTemperatureK;
                    }

                    if (ageYears <= 0.0)
                    {
                        ageYears = star.Stellar.AgeYears;
                    }
                }
            }
        }

        if (massSolar <= 0.0)
        {
            massSolar = 1.0;
        }

        if (luminositySolar <= 0.0)
        {
            luminositySolar = 1.0;
        }

        if (metallicity <= 0.0)
        {
            metallicity = spec?.GalaxyContext?.MetallicityPrior ?? 1.0;
        }

        double metallicityEnrichment = System.Math.Clamp(metallicity, 0.35, 2.5);
        double snowLineAu = 2.7 * System.Math.Sqrt(System.Math.Max(0.05, luminositySolar)) * profile.SnowLineScalar;
        double diskLifetimeFactor = System.Math.Clamp(profile.DiskLifetimeMyr / 3.5, 0.4, 2.0);
        double solidBudget = profile.SolidMassScalar * (0.72 + (0.28 * metallicityEnrichment)) * System.Math.Pow(profile.OxidationScalar, 0.35);
        double gasBudget = profile.GasMassScalar * (0.80 + (0.12 * metallicityEnrichment)) * System.Math.Sqrt(diskLifetimeFactor);
        double escapeProxy = System.Math.Clamp((luminositySolar * 0.55) + (profile.MigrationStrength * 0.25) + (profile.ImpactStirring * 0.15), 0.3, 3.5);
        double gasGiantWeight = 1.0;
        if (profile.GasGiantFormationModel == GasGiantFormationModel.CoreAccretion)
        {
            gasGiantWeight *= 0.95;
        }
        else if (profile.GasGiantFormationModel == GasGiantFormationModel.PebbleAssisted)
        {
            gasGiantWeight *= 1.15;
        }
        else
        {
            gasGiantWeight *= 1.05;
        }

        gasGiantWeight *= profile.MetallicityCouplingStrength switch
        {
            PlanetMetallicityCouplingStrength.Weak => 0.9 + (0.12 * metallicityEnrichment),
            PlanetMetallicityCouplingStrength.Strong => 0.72 + (0.45 * metallicityEnrichment),
            _ => 0.82 + (0.28 * metallicityEnrichment),
        };

        if (ageYears > 0.0 && ageYears < 1.0e9)
        {
            gasBudget *= 1.05;
        }
        else if (ageYears > 8.0e9)
        {
            solidBudget *= 1.05;
            escapeProxy *= 0.92;
        }

        double ageActivityFactor = 1.0;
        if (ageYears > 0.0)
        {
            ageActivityFactor = System.Math.Clamp(System.Math.Pow(1.8e9 / ageYears, 0.18), 0.65, 1.8);
        }

        double lowMassActivityFactor;
        if (massSolar < 0.85)
        {
            lowMassActivityFactor = System.Math.Clamp(1.42 - (massSolar * 0.40), 1.0, 1.35);
        }
        else
        {
            lowMassActivityFactor = System.Math.Clamp(1.02 - ((massSolar - 0.85) * 0.10), 0.82, 1.02);
        }

        double xuvActivity = System.Math.Clamp(ageActivityFactor * lowMassActivityFactor * (0.92 + (0.08 * luminositySolar)), 0.45, 1.85);
        double outerBias = profile.MinorBodyOuterSystemBias switch
        {
            PlanetMinorBodyOuterSystemBias.AsteroidLeaning => 0.82,
            PlanetMinorBodyOuterSystemBias.CometLeaning => 1.24,
            _ => 1.0,
        };
        double outerReservoir = System.Math.Clamp(
            outerBias * (0.78 + (0.30 * gasBudget) + (0.22 * solidBudget)),
            0.45,
            2.60);
        double volatileDelivery = System.Math.Clamp(
            outerReservoir * (0.72 + (0.18 * profile.MigrationStrength) + (0.15 * profile.ImpactStirring)),
            0.35,
            2.75);
        double bombardment = System.Math.Clamp(
            profile.ImpactStirring * (0.75 + (0.35 * outerReservoir)),
            0.30,
            3.00);
        double habitableZoneInnerAu = OrbitalMechanics.CalculateHabitableZoneInner(
            luminositySolar * StellarProps.SolarLuminosityWatts,
            effectiveTempK) / Units.AuMeters;
        double habitableZoneOuterAu = OrbitalMechanics.CalculateHabitableZoneOuter(
            luminositySolar * StellarProps.SolarLuminosityWatts,
            effectiveTempK) / Units.AuMeters;

        return new PlanetarySystemState
        {
            Profile = profile,
            StellarMassSolar = massSolar,
            StellarLuminositySolar = luminositySolar,
            SystemMetallicity = metallicity,
            SnowLineAu = snowLineAu,
            SolidBudgetScalar = solidBudget,
            GasBudgetScalar = gasBudget,
            EscapePressureProxy = escapeProxy,
            MigrationStrength = profile.MigrationStrength,
            ImpactStirring = profile.ImpactStirring,
            MetallicityEnrichment = metallicityEnrichment,
            GasGiantWeight = gasGiantWeight,
            HabitableZoneInnerAu = habitableZoneInnerAu,
            HabitableZoneOuterAu = habitableZoneOuterAu,
            XuvActivityScalar = xuvActivity,
            OuterReservoirScalar = outerReservoir,
            VolatileDeliveryScalar = volatileDelivery,
            BombardmentScalar = bombardment,
        };
    }

    /// <summary>
    /// Returns the stellar flux at the supplied orbit in Earth-equivalent units.
    /// </summary>
    public double GetFluxEarth(double orbitAu)
    {
        if (orbitAu <= 0.0)
        {
            return 0.0;
        }

        return StellarLuminositySolar / System.Math.Max(orbitAu * orbitAu, 0.01);
    }

    /// <summary>
    /// Returns how well the supplied orbit aligns with the classical habitable zone.
    /// </summary>
    public double GetHabitableZoneAlignment(double orbitAu)
    {
        if (orbitAu <= 0.0 || HabitableZoneInnerAu <= 0.0 || HabitableZoneOuterAu <= HabitableZoneInnerAu)
        {
            return 0.0;
        }

        if (orbitAu >= HabitableZoneInnerAu && orbitAu <= HabitableZoneOuterAu)
        {
            return 1.0;
        }

        double innerDecayFloor = HabitableZoneInnerAu * 0.45;
        double outerDecayCeiling = HabitableZoneOuterAu * 1.80;
        if (orbitAu < HabitableZoneInnerAu)
        {
            return System.Math.Clamp((orbitAu - innerDecayFloor) / System.Math.Max(HabitableZoneInnerAu - innerDecayFloor, 0.01), 0.0, 1.0);
        }

        return System.Math.Clamp((outerDecayCeiling - orbitAu) / System.Math.Max(outerDecayCeiling - HabitableZoneOuterAu, 0.01), 0.0, 1.0);
    }

    /// <summary>
    /// Creates a detached copy of the state.
    /// </summary>
    public PlanetarySystemState Clone()
    {
        return new PlanetarySystemState
        {
            Profile = Profile.Clone(),
            StellarMassSolar = StellarMassSolar,
            StellarLuminositySolar = StellarLuminositySolar,
            SystemMetallicity = SystemMetallicity,
            SnowLineAu = SnowLineAu,
            SolidBudgetScalar = SolidBudgetScalar,
            GasBudgetScalar = GasBudgetScalar,
            EscapePressureProxy = EscapePressureProxy,
            MigrationStrength = MigrationStrength,
            ImpactStirring = ImpactStirring,
            MetallicityEnrichment = MetallicityEnrichment,
            GasGiantWeight = GasGiantWeight,
            HabitableZoneInnerAu = HabitableZoneInnerAu,
            HabitableZoneOuterAu = HabitableZoneOuterAu,
            XuvActivityScalar = XuvActivityScalar,
            OuterReservoirScalar = OuterReservoirScalar,
            VolatileDeliveryScalar = VolatileDeliveryScalar,
            BombardmentScalar = BombardmentScalar,
        };
    }

    /// <summary>
    /// Converts the state to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["profile"] = Profile.ToDictionary(),
            ["stellar_mass_solar"] = StellarMassSolar,
            ["stellar_luminosity_solar"] = StellarLuminositySolar,
            ["system_metallicity"] = SystemMetallicity,
            ["snow_line_au"] = SnowLineAu,
            ["solid_budget_scalar"] = SolidBudgetScalar,
            ["gas_budget_scalar"] = GasBudgetScalar,
            ["escape_pressure_proxy"] = EscapePressureProxy,
            ["migration_strength"] = MigrationStrength,
            ["impact_stirring"] = ImpactStirring,
            ["metallicity_enrichment"] = MetallicityEnrichment,
            ["gas_giant_weight"] = GasGiantWeight,
            ["habitable_zone_inner_au"] = HabitableZoneInnerAu,
            ["habitable_zone_outer_au"] = HabitableZoneOuterAu,
            ["xuv_activity_scalar"] = XuvActivityScalar,
            ["outer_reservoir_scalar"] = OuterReservoirScalar,
            ["volatile_delivery_scalar"] = VolatileDeliveryScalar,
            ["bombardment_scalar"] = BombardmentScalar,
        };
    }

    /// <summary>
    /// Rebuilds a derived state from a dictionary payload.
    /// </summary>
    public static PlanetarySystemState FromDictionary(Dictionary data)
    {
        PlanetarySystemState state = new PlanetarySystemState();
        if (data.ContainsKey("profile") && data["profile"].VariantType == Variant.Type.Dictionary)
        {
            state.Profile = PlanetaryGenerationProfile.FromDictionary((Dictionary)data["profile"]);
        }

        state.StellarMassSolar = DomainDictionaryUtils.GetDouble(data, "stellar_mass_solar", 1.0);
        state.StellarLuminositySolar = DomainDictionaryUtils.GetDouble(data, "stellar_luminosity_solar", 1.0);
        state.SystemMetallicity = DomainDictionaryUtils.GetDouble(data, "system_metallicity", 1.0);
        state.SnowLineAu = DomainDictionaryUtils.GetDouble(data, "snow_line_au", 2.7);
        state.SolidBudgetScalar = DomainDictionaryUtils.GetDouble(data, "solid_budget_scalar", 1.0);
        state.GasBudgetScalar = DomainDictionaryUtils.GetDouble(data, "gas_budget_scalar", 1.0);
        state.EscapePressureProxy = DomainDictionaryUtils.GetDouble(data, "escape_pressure_proxy", 1.0);
        state.MigrationStrength = DomainDictionaryUtils.GetDouble(data, "migration_strength", 1.0);
        state.ImpactStirring = DomainDictionaryUtils.GetDouble(data, "impact_stirring", 1.0);
        state.MetallicityEnrichment = DomainDictionaryUtils.GetDouble(data, "metallicity_enrichment", 1.0);
        state.GasGiantWeight = DomainDictionaryUtils.GetDouble(data, "gas_giant_weight", 1.0);
        state.HabitableZoneInnerAu = DomainDictionaryUtils.GetDouble(data, "habitable_zone_inner_au", 0.95);
        state.HabitableZoneOuterAu = DomainDictionaryUtils.GetDouble(data, "habitable_zone_outer_au", 1.37);
        state.XuvActivityScalar = DomainDictionaryUtils.GetDouble(data, "xuv_activity_scalar", 1.0);
        state.OuterReservoirScalar = DomainDictionaryUtils.GetDouble(data, "outer_reservoir_scalar", 1.0);
        state.VolatileDeliveryScalar = DomainDictionaryUtils.GetDouble(data, "volatile_delivery_scalar", 1.0);
        state.BombardmentScalar = DomainDictionaryUtils.GetDouble(data, "bombardment_scalar", 1.0);
        return state;
    }
}
