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
        };
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
        return state;
    }
}
