using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Generation;

/// <summary>
/// Supported mass-radius lookup families for planet generation.
/// </summary>
public enum PlanetMassRadiusModel
{
    ChenKipping = 1,
    Otegi = 2,
}

/// <summary>
/// Supported close-in envelope-loss models.
/// </summary>
public enum PlanetEnvelopeLossModel
{
    Auto = 0,
    Photoevaporation = 1,
    CorePowered = 2,
}

/// <summary>
/// Supported gas-giant formation emphases.
/// </summary>
public enum GasGiantFormationModel
{
    CoreAccretion = 0,
    PebbleAssisted = 1,
    Mixed = 2,
}

/// <summary>
/// Strength of metallicity coupling for giant-planet outcomes.
/// </summary>
public enum PlanetMetallicityCouplingStrength
{
    Weak = 0,
    ObservedDefault = 1,
    Strong = 2,
}

/// <summary>
/// Bias for how moons are formed around large planets.
/// </summary>
public enum PlanetMoonFormationBias
{
    RegularDiskFavored = 0,
    CapturedRich = 1,
    Mixed = 2,
}

/// <summary>
/// Policy for how much ejection pressure the generator should assume.
/// </summary>
public enum PlanetRoguePlanetAllowance
{
    Off = 0,
    Rare = 1,
    Standard = 2,
}

/// <summary>
/// Bias for whether outer-system leftovers lean more asteroid-like or comet-like.
/// </summary>
public enum PlanetMinorBodyOuterSystemBias
{
    AsteroidLeaning = 0,
    Balanced = 1,
    CometLeaning = 2,
}

/// <summary>
/// Shared aggregate planetary-generation priors used by galaxy and system generation.
/// </summary>
public partial class PlanetaryGenerationProfile : RefCounted
{
    /// <summary>
    /// Mass-radius family used when resolving broad class outcomes into physical sizes.
    /// </summary>
    public PlanetMassRadiusModel MassRadiusModel { get; set; } = PlanetMassRadiusModel.ChenKipping;

    /// <summary>
    /// Envelope-loss family used for close-in volatile-rich worlds.
    /// </summary>
    public PlanetEnvelopeLossModel EnvelopeLossModel { get; set; } = PlanetEnvelopeLossModel.Auto;

    /// <summary>
    /// Gas-giant formation emphasis used for aggregate weighting.
    /// </summary>
    public GasGiantFormationModel GasGiantFormationModel { get; set; } = GasGiantFormationModel.Mixed;

    /// <summary>
    /// Strength of metallicity coupling for gas-giant outcomes.
    /// </summary>
    public PlanetMetallicityCouplingStrength MetallicityCouplingStrength { get; set; } = PlanetMetallicityCouplingStrength.ObservedDefault;

    /// <summary>
    /// Bias for regular versus captured moon outcomes around large planets.
    /// </summary>
    public PlanetMoonFormationBias MoonFormationBias { get; set; } = PlanetMoonFormationBias.Mixed;

    /// <summary>
    /// Ejection-pressure policy used to suppress or allow bound planets.
    /// </summary>
    public PlanetRoguePlanetAllowance RoguePlanetAllowance { get; set; } = PlanetRoguePlanetAllowance.Rare;

    /// <summary>
    /// Bias for how much outer-system leftover material behaves like comet reservoirs.
    /// </summary>
    public PlanetMinorBodyOuterSystemBias MinorBodyOuterSystemBias { get; set; } = PlanetMinorBodyOuterSystemBias.Balanced;

    /// <summary>
    /// Aggregate solids budget scalar used to bias rocky and ice-rich outcomes.
    /// </summary>
    public double SolidMassScalar { get; set; } = 1.0;

    /// <summary>
    /// Aggregate gas budget scalar used to bias volatile-rich and giant outcomes.
    /// </summary>
    public double GasMassScalar { get; set; } = 1.0;

    /// <summary>
    /// Protoplanetary disk lifetime surrogate in Myr.
    /// </summary>
    public double DiskLifetimeMyr { get; set; } = 3.5;

    /// <summary>
    /// Scalar applied to the snow-line distance.
    /// </summary>
    public double SnowLineScalar { get; set; } = 1.0;

    /// <summary>
    /// Scalar used to shift dry versus oxidized or water-friendly planet mixes.
    /// </summary>
    public double OxidationScalar { get; set; } = 1.0;

    /// <summary>
    /// Migration-strength surrogate used to reshape broad planet class frequencies.
    /// </summary>
    public double MigrationStrength { get; set; } = 1.0;

    /// <summary>
    /// Impact-stirring surrogate used to increase stripping and capture outcomes.
    /// </summary>
    public double ImpactStirring { get; set; } = 1.0;

    /// <summary>
    /// Creates the default aggregate planetary-generation profile.
    /// </summary>
    public static PlanetaryGenerationProfile CreateDefault()
    {
        return new PlanetaryGenerationProfile();
    }

    /// <summary>
    /// Creates a detached copy of the profile.
    /// </summary>
    public PlanetaryGenerationProfile Clone()
    {
        return new PlanetaryGenerationProfile
        {
            MassRadiusModel = MassRadiusModel,
            EnvelopeLossModel = EnvelopeLossModel,
            GasGiantFormationModel = GasGiantFormationModel,
            MetallicityCouplingStrength = MetallicityCouplingStrength,
            MoonFormationBias = MoonFormationBias,
            RoguePlanetAllowance = RoguePlanetAllowance,
            MinorBodyOuterSystemBias = MinorBodyOuterSystemBias,
            SolidMassScalar = SolidMassScalar,
            GasMassScalar = GasMassScalar,
            DiskLifetimeMyr = DiskLifetimeMyr,
            SnowLineScalar = SnowLineScalar,
            OxidationScalar = OxidationScalar,
            MigrationStrength = MigrationStrength,
            ImpactStirring = ImpactStirring,
        };
    }

    /// <summary>
    /// Returns whether the profile values stay inside supported ranges.
    /// </summary>
    public bool IsValid()
    {
        if (!System.Enum.IsDefined(typeof(PlanetMassRadiusModel), (int)MassRadiusModel))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(PlanetEnvelopeLossModel), (int)EnvelopeLossModel))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(GasGiantFormationModel), (int)GasGiantFormationModel))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(PlanetMetallicityCouplingStrength), (int)MetallicityCouplingStrength))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(PlanetMoonFormationBias), (int)MoonFormationBias))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(PlanetRoguePlanetAllowance), (int)RoguePlanetAllowance))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(PlanetMinorBodyOuterSystemBias), (int)MinorBodyOuterSystemBias))
        {
            return false;
        }

        if (SolidMassScalar < 0.35 || SolidMassScalar > 2.5)
        {
            return false;
        }

        if (GasMassScalar < 0.2 || GasMassScalar > 3.0)
        {
            return false;
        }

        if (DiskLifetimeMyr < 0.5 || DiskLifetimeMyr > 12.0)
        {
            return false;
        }

        if (SnowLineScalar < 0.5 || SnowLineScalar > 2.0)
        {
            return false;
        }

        if (OxidationScalar < 0.5 || OxidationScalar > 1.5)
        {
            return false;
        }

        if (MigrationStrength < 0.2 || MigrationStrength > 2.5)
        {
            return false;
        }

        if (ImpactStirring < 0.2 || ImpactStirring > 2.5)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts the profile to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["mass_radius_model"] = (int)MassRadiusModel,
            ["envelope_loss_model"] = (int)EnvelopeLossModel,
            ["gas_giant_formation_model"] = (int)GasGiantFormationModel,
            ["metallicity_coupling_strength"] = (int)MetallicityCouplingStrength,
            ["moon_formation_bias"] = (int)MoonFormationBias,
            ["rogue_planet_allowance"] = (int)RoguePlanetAllowance,
            ["minor_body_outer_system_bias"] = (int)MinorBodyOuterSystemBias,
            ["solid_mass_scalar"] = SolidMassScalar,
            ["gas_mass_scalar"] = GasMassScalar,
            ["disk_lifetime_myr"] = DiskLifetimeMyr,
            ["snow_line_scalar"] = SnowLineScalar,
            ["oxidation_scalar"] = OxidationScalar,
            ["migration_strength"] = MigrationStrength,
            ["impact_stirring"] = ImpactStirring,
        };
    }

    /// <summary>
    /// Rebuilds the profile from a dictionary payload.
    /// </summary>
    public static PlanetaryGenerationProfile FromDictionary(Dictionary data)
    {
        PlanetaryGenerationProfile profile = new PlanetaryGenerationProfile();

        int massRadiusValue = DomainDictionaryUtils.GetInt(data, "mass_radius_model", (int)PlanetMassRadiusModel.ChenKipping);
        if (massRadiusValue == 0)
        {
            massRadiusValue = (int)PlanetMassRadiusModel.ChenKipping;
        }

        if (System.Enum.IsDefined(typeof(PlanetMassRadiusModel), massRadiusValue))
        {
            profile.MassRadiusModel = (PlanetMassRadiusModel)massRadiusValue;
        }

        int envelopeLossValue = DomainDictionaryUtils.GetInt(data, "envelope_loss_model", (int)PlanetEnvelopeLossModel.Auto);
        if (System.Enum.IsDefined(typeof(PlanetEnvelopeLossModel), envelopeLossValue))
        {
            profile.EnvelopeLossModel = (PlanetEnvelopeLossModel)envelopeLossValue;
        }

        int gasGiantValue = DomainDictionaryUtils.GetInt(data, "gas_giant_formation_model", (int)GasGiantFormationModel.Mixed);
        if (System.Enum.IsDefined(typeof(GasGiantFormationModel), gasGiantValue))
        {
            profile.GasGiantFormationModel = (GasGiantFormationModel)gasGiantValue;
        }

        int metallicityValue = DomainDictionaryUtils.GetInt(data, "metallicity_coupling_strength", (int)PlanetMetallicityCouplingStrength.ObservedDefault);
        if (System.Enum.IsDefined(typeof(PlanetMetallicityCouplingStrength), metallicityValue))
        {
            profile.MetallicityCouplingStrength = (PlanetMetallicityCouplingStrength)metallicityValue;
        }

        int moonBiasValue = DomainDictionaryUtils.GetInt(data, "moon_formation_bias", (int)PlanetMoonFormationBias.Mixed);
        if (System.Enum.IsDefined(typeof(PlanetMoonFormationBias), moonBiasValue))
        {
            profile.MoonFormationBias = (PlanetMoonFormationBias)moonBiasValue;
        }

        int rogueValue = DomainDictionaryUtils.GetInt(data, "rogue_planet_allowance", (int)PlanetRoguePlanetAllowance.Rare);
        if (System.Enum.IsDefined(typeof(PlanetRoguePlanetAllowance), rogueValue))
        {
            profile.RoguePlanetAllowance = (PlanetRoguePlanetAllowance)rogueValue;
        }

        int outerBiasValue = DomainDictionaryUtils.GetInt(data, "minor_body_outer_system_bias", (int)PlanetMinorBodyOuterSystemBias.Balanced);
        if (System.Enum.IsDefined(typeof(PlanetMinorBodyOuterSystemBias), outerBiasValue))
        {
            profile.MinorBodyOuterSystemBias = (PlanetMinorBodyOuterSystemBias)outerBiasValue;
        }

        profile.SolidMassScalar = DomainDictionaryUtils.GetDouble(data, "solid_mass_scalar", 1.0);
        profile.GasMassScalar = DomainDictionaryUtils.GetDouble(data, "gas_mass_scalar", 1.0);
        profile.DiskLifetimeMyr = DomainDictionaryUtils.GetDouble(data, "disk_lifetime_myr", 3.5);
        profile.SnowLineScalar = DomainDictionaryUtils.GetDouble(data, "snow_line_scalar", 1.0);
        profile.OxidationScalar = DomainDictionaryUtils.GetDouble(data, "oxidation_scalar", 1.0);
        profile.MigrationStrength = DomainDictionaryUtils.GetDouble(data, "migration_strength", 1.0);
        profile.ImpactStirring = DomainDictionaryUtils.GetDouble(data, "impact_stirring", 1.0);
        return profile;
    }
}
