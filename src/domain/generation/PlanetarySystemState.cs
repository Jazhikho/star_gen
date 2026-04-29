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
    /// Host-mass adjustment applied to the nominal disk lifetime prior.
    /// </summary>
    public double HostMassDiskLifetimeScalar { get; set; } = 1.0;

    /// <summary>
    /// Host-mass adjustment applied to the solids reservoir prior.
    /// </summary>
    public double HostMassSolidReservoirScalar { get; set; } = 1.0;

    /// <summary>
    /// How strongly giant-planet formation should peak around the snow line.
    /// </summary>
    public double SnowLineGiantFormationScalar { get; set; } = 1.0;

    /// <summary>
    /// How much volatile delivery is being boosted by giant-driven scattering channels.
    /// </summary>
    public double GiantScatteringScalar { get; set; } = 1.0;

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
        // Pascucci et al. (2016) and Ribas et al. (2015) support host-mass dependence in disk
        // solids and disk lifetimes. Tuning: the `0.65`, `0.25`, `3.5`, and clamp bands below
        // are StarGen surrogates for propagating those trends into one deterministic system
        // state rather than direct disk-population fit coefficients.
        double hostMassDiskLifetimeScalar = ComputeHostMassDiskLifetimeScalar(massSolar);
        double hostMassSolidReservoirScalar = System.Math.Clamp(System.Math.Pow(System.Math.Max(0.2, massSolar), 0.65), 0.45, 1.85);
        double hostMassGasReservoirScalar = System.Math.Clamp(System.Math.Pow(System.Math.Max(0.2, massSolar), 0.25), 0.70, 1.35);
        double adjustedDiskLifetimeMyr = profile.DiskLifetimeMyr * hostMassDiskLifetimeScalar;
        double diskLifetimeFactor = System.Math.Clamp(adjustedDiskLifetimeMyr / 3.5, 0.35, 2.10);
        double diskRadiusFactor = System.Math.Clamp(profile.DiskRadiusScale, 0.35, 3.0);
        double dustToGasFactor = System.Math.Clamp(profile.DustToGasScale, 0.35, 3.0);
        double fragmentationFactor = profile.FragmentationVelocityModel switch
        {
            PlanetFragmentationVelocityModel.LowFragmentationVelocity => 0.86,
            PlanetFragmentationVelocityModel.HighFragmentationVelocity => 1.14,
            _ => 1.0,
        };
        double giantOriginBandFactor = profile.GiantOriginBandModel switch
        {
            PlanetGiantOriginBandModel.FiveToTwentyFiveAu => 1.12,
            PlanetGiantOriginBandModel.SnowLineAdjacent => 1.08,
            _ => 1.0,
        };
        // Fischer and Valenti (2005) support stronger giant-planet and solid-reservoir outcomes
        // around metal-rich hosts. Tuning: the metallicity blends below are StarGen enrichment
        // weights layered onto that framework instead of literature coefficients.
        double solidBudget = profile.SolidMassScalar
            * hostMassSolidReservoirScalar
            * dustToGasFactor
            * (0.72 + (0.28 * metallicityEnrichment))
            * System.Math.Pow(profile.OxidationScalar, 0.35);
        double gasBudget = profile.GasMassScalar
            * hostMassGasReservoirScalar
            * System.Math.Pow(diskRadiusFactor, 0.18)
            * (0.80 + (0.12 * metallicityEnrichment))
            * System.Math.Sqrt(diskLifetimeFactor);
        // Tanaka, Takeuchi, and Ward (2002) provide the canonical isothermal Type-I migration
        // framework for low-mass planets embedded in gaseous discs. Tuning: the weighted
        // surrogate below compresses that framework into a deterministic StarGen aggregate
        // proxy, so the `0.78 / 0.18 / 0.10 / 0.18` coefficients are calibration choices rather
        // than literature constants, and remain pending human verification in the audit pass.
        double effectiveMigrationStrength = System.Math.Clamp(
            profile.MigrationStrength
            * (0.78 + (0.18 * solidBudget) + (0.10 * gasBudget))
            * System.Math.Pow(diskLifetimeFactor, 0.18),
            0.45,
            2.40);
        double escapeProxy = System.Math.Clamp(
            (luminositySolar * 0.55) + (effectiveMigrationStrength * 0.22) + (profile.ImpactStirring * 0.15),
            0.3,
            3.5);
        // Mordasini et al. (2007) and Lambrechts and Johansen (2012) motivate distinct giant-
        // formation branches for core accretion and pebble-assisted growth. Tuning: the
        // `0.95 / 1.15 / 1.05` multipliers below are StarGen branch weights, not published
        // occurrence ratios.
        double gasGiantWeight = 1.0;
        if (profile.GasGiantFormationModel == GasGiantFormationModel.CoreAccretion)
        {
            gasGiantWeight *= 0.95;
        }
        else if (profile.GasGiantFormationModel == GasGiantFormationModel.PebbleAssisted)
        {
            gasGiantWeight *= 1.15 * fragmentationFactor;
        }
        else
        {
            gasGiantWeight *= 1.05 * System.Math.Sqrt(fragmentationFactor);
        }

        gasGiantWeight *= giantOriginBandFactor;

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

        // Fernandes et al. (2019) support a giant-planet occurrence peak near the snow line,
        // and Izidoro et al. (2017) support migration-linked scattering as an architecture
        // shaper. Tuning: the `0.72 + 0.20 * gas + 0.16 * solids` and
        // `0.62 + 0.20 * stirring + 0.12 * migration` composites below are StarGen weights.
        double snowLineGiantFormation = System.Math.Clamp(
            gasGiantWeight
            * (0.72 + (0.20 * gasBudget) + (0.16 * solidBudget))
            * System.Math.Pow(diskLifetimeFactor, 0.20),
            0.25,
            3.00);
        if (profile.GiantOriginBandModel == PlanetGiantOriginBandModel.FiveToTwentyFiveAu)
        {
            double sourceBandAlignment = System.Math.Clamp((snowLineAu - 2.0) / 23.0, 0.0, 1.0);
            snowLineGiantFormation *= 0.94 + (0.18 * sourceBandAlignment);
        }
        else if (profile.GiantOriginBandModel == PlanetGiantOriginBandModel.SnowLineAdjacent)
        {
            snowLineGiantFormation *= 1.08;
        }

        snowLineGiantFormation = System.Math.Clamp(snowLineGiantFormation, 0.25, 3.00);
        double giantScattering = System.Math.Clamp(
            snowLineGiantFormation
            * (0.62 + (0.20 * profile.ImpactStirring) + (0.12 * effectiveMigrationStrength)),
            0.25,
            3.25);
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

        // Luger and Barnes (2015) support stronger long-lived high-energy activity around low-
        // mass hosts, while Ribas et al. (2015) anchors the broad age dependence of young
        // systems. Tuning: the final luminosity blend and clamp remain StarGen summary weights.
        double xuvActivity = System.Math.Clamp(ageActivityFactor * lowMassActivityFactor * (0.92 + (0.08 * luminositySolar)), 0.45, 1.85);
        double outerBias = profile.MinorBodyOuterSystemBias switch
        {
            PlanetMinorBodyOuterSystemBias.AsteroidLeaning => 0.82,
            PlanetMinorBodyOuterSystemBias.CometLeaning => 1.24,
            _ => 1.0,
        };
        // DeMeo and Carry (2014), Lamy et al. (2004), and Raymond and Izidoro (2017) support
        // treating outer small-body reservoirs and inward volatile delivery as linked but not
        // identical channels. Tuning: the `0.74 / 0.26 / 0.18 / 0.08` and
        // `0.56 / 0.22 / 0.10` blends below are StarGen transport weights.
        double outerReservoir = System.Math.Clamp(
            outerBias * (0.74 + (0.26 * gasBudget) + (0.18 * solidBudget) + (0.08 * snowLineGiantFormation)),
            0.45,
            2.75);
        double volatileDelivery = System.Math.Clamp(
            outerReservoir * (0.56 + (0.22 * giantScattering) + (0.10 * profile.ImpactStirring)),
            0.30,
            3.00);
        double bombardment = System.Math.Clamp(
            profile.ImpactStirring * (0.62 + (0.22 * outerReservoir) + (0.26 * giantScattering)),
            0.30,
            3.00);
        double habitableZoneInnerAu = OrbitalMechanics.CalculateHabitableZoneInner(
            luminositySolar * StellarProps.SolarLuminosityWatts,
            effectiveTempK,
            profile.HabitableZoneModel) / Units.AuMeters;
        double habitableZoneOuterAu = OrbitalMechanics.CalculateHabitableZoneOuter(
            luminositySolar * StellarProps.SolarLuminosityWatts,
            effectiveTempK,
            profile.HabitableZoneModel) / Units.AuMeters;

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
            MigrationStrength = effectiveMigrationStrength,
            ImpactStirring = profile.ImpactStirring,
            MetallicityEnrichment = metallicityEnrichment,
            GasGiantWeight = gasGiantWeight,
            HostMassDiskLifetimeScalar = hostMassDiskLifetimeScalar,
            HostMassSolidReservoirScalar = hostMassSolidReservoirScalar,
            SnowLineGiantFormationScalar = snowLineGiantFormation,
            GiantScatteringScalar = giantScattering,
            HabitableZoneInnerAu = habitableZoneInnerAu,
            HabitableZoneOuterAu = habitableZoneOuterAu,
            XuvActivityScalar = xuvActivity,
            OuterReservoirScalar = outerReservoir,
            VolatileDeliveryScalar = volatileDelivery,
            BombardmentScalar = bombardment,
        };
    }

    /// <summary>
    /// Returns how strongly giant-planet outcomes should be favored at a given orbit.
    /// </summary>
    public double GetSnowLineGiantFormationWeight(double orbitAu)
    {
        if (orbitAu <= 0.0)
        {
            return 0.0;
        }

        if (SnowLineAu <= 0.0)
        {
            return System.Math.Clamp(SnowLineGiantFormationScalar, 0.25, 1.70);
        }

        double ratio = orbitAu / System.Math.Max(SnowLineAu, 0.01);
        double logDistance = System.Math.Abs(System.Math.Log10(System.Math.Max(0.05, ratio)));
        double weight = 1.20 - (1.05 * logDistance);
        if (ratio > 2.5)
        {
            weight *= 0.88;
        }

        if (ratio > 5.0)
        {
            weight *= 0.68;
        }

        weight *= 0.74 + (0.26 * SnowLineGiantFormationScalar);
        return System.Math.Clamp(weight, 0.25, 1.70);
    }

    /// <summary>
    /// Returns how strongly compact inner architectures should be favored at a given orbit.
    /// </summary>
    public double GetCompactInnerArchitectureWeight(double orbitAu)
    {
        if (orbitAu <= 0.0)
        {
            return 0.0;
        }

        double compactEdgeAu = SnowLineAu * 0.65;
        if (HabitableZoneOuterAu > compactEdgeAu)
        {
            compactEdgeAu = HabitableZoneOuterAu;
        }

        if (compactEdgeAu <= 0.0)
        {
            return System.Math.Clamp(MigrationStrength, 0.35, 2.50);
        }

        double normalizedDistance = 1.0
            - System.Math.Clamp((orbitAu - 0.10) / System.Math.Max(compactEdgeAu - 0.10, 0.10), 0.0, 1.0);
        double weight = 0.70 + (normalizedDistance * (MigrationStrength - 0.70));
        if (orbitAu > compactEdgeAu * 1.35)
        {
            weight *= 0.82;
        }

        return System.Math.Clamp(weight, 0.35, 2.50);
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
            HostMassDiskLifetimeScalar = HostMassDiskLifetimeScalar,
            HostMassSolidReservoirScalar = HostMassSolidReservoirScalar,
            SnowLineGiantFormationScalar = SnowLineGiantFormationScalar,
            GiantScatteringScalar = GiantScatteringScalar,
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
            ["host_mass_disk_lifetime_scalar"] = HostMassDiskLifetimeScalar,
            ["host_mass_solid_reservoir_scalar"] = HostMassSolidReservoirScalar,
            ["snow_line_giant_formation_scalar"] = SnowLineGiantFormationScalar,
            ["giant_scattering_scalar"] = GiantScatteringScalar,
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
        state.HostMassDiskLifetimeScalar = DomainDictionaryUtils.GetDouble(data, "host_mass_disk_lifetime_scalar", 1.0);
        state.HostMassSolidReservoirScalar = DomainDictionaryUtils.GetDouble(data, "host_mass_solid_reservoir_scalar", 1.0);
        state.SnowLineGiantFormationScalar = DomainDictionaryUtils.GetDouble(data, "snow_line_giant_formation_scalar", 1.0);
        state.GiantScatteringScalar = DomainDictionaryUtils.GetDouble(data, "giant_scattering_scalar", 1.0);
        state.HabitableZoneInnerAu = DomainDictionaryUtils.GetDouble(data, "habitable_zone_inner_au", 0.95);
        state.HabitableZoneOuterAu = DomainDictionaryUtils.GetDouble(data, "habitable_zone_outer_au", 1.37);
        state.XuvActivityScalar = DomainDictionaryUtils.GetDouble(data, "xuv_activity_scalar", 1.0);
        state.OuterReservoirScalar = DomainDictionaryUtils.GetDouble(data, "outer_reservoir_scalar", 1.0);
        state.VolatileDeliveryScalar = DomainDictionaryUtils.GetDouble(data, "volatile_delivery_scalar", 1.0);
        state.BombardmentScalar = DomainDictionaryUtils.GetDouble(data, "bombardment_scalar", 1.0);
        return state;
    }

    private static double ComputeHostMassDiskLifetimeScalar(double stellarMassSolar)
    {
        if (stellarMassSolar <= 0.0)
        {
            return 1.0;
        }

        if (stellarMassSolar >= 2.0)
        {
            return System.Math.Clamp(0.74 - ((stellarMassSolar - 2.0) * 0.08), 0.55, 0.74);
        }

        if (stellarMassSolar >= 1.5)
        {
            return System.Math.Clamp(0.90 - ((stellarMassSolar - 1.5) * 0.32), 0.74, 0.90);
        }

        if (stellarMassSolar <= 0.6)
        {
            return System.Math.Clamp(1.14 + ((0.6 - stellarMassSolar) * 0.25), 1.0, 1.28);
        }

        return 1.0;
    }
}
