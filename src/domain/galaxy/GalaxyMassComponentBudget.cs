using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Diagnostic-only mass component budget for galaxy-level science audits and future dynamics work.
/// </summary>
public partial class GalaxyMassComponentBudget : RefCounted
{
    /// <summary>
    /// Total halo mass proxy in solar masses.
    /// </summary>
    public double TotalHaloMassSolar { get; set; } = 1.0e12;

    /// <summary>
    /// Dark-matter halo mass proxy in solar masses after diagnostic baryonic components are reserved.
    /// </summary>
    public double DarkMatterHaloMassSolar { get; set; } = 9.25e11;

    /// <summary>
    /// Total baryonic component mass proxy in solar masses.
    /// </summary>
    public double BaryonicMassSolar { get; set; } = 7.5e10;

    /// <summary>
    /// Stellar disk mass proxy in solar masses.
    /// </summary>
    public double StellarDiskMassSolar { get; set; } = 3.4e10;

    /// <summary>
    /// Stellar spheroid mass proxy in solar masses, including bulge or elliptical main body.
    /// </summary>
    public double StellarSpheroidMassSolar { get; set; } = 1.5e10;

    /// <summary>
    /// Diffuse stellar halo mass proxy in solar masses.
    /// </summary>
    public double StellarHaloMassSolar { get; set; } = 9.5e8;

    /// <summary>
    /// Nuclear stellar component mass proxy in solar masses.
    /// </summary>
    public double NuclearStellarMassSolar { get; set; } = 3.5e7;

    /// <summary>
    /// Cold gas mass proxy in solar masses.
    /// </summary>
    public double ColdGasMassSolar { get; set; } = 6.0e9;

    /// <summary>
    /// Hot gas or circumgalactic gas mass proxy in solar masses.
    /// </summary>
    public double HotGasMassSolar { get; set; } = 1.9e10;

    /// <summary>
    /// Baryonic mass divided by total halo mass.
    /// </summary>
    public double BaryonFraction { get; set; } = 0.075;

    /// <summary>
    /// Cold gas plus hot gas divided by baryonic mass.
    /// </summary>
    public double GasFraction { get; set; } = 0.33;

    /// <summary>
    /// Local solar-neighborhood stellar mass density proxy in solar masses per cubic parsec.
    /// </summary>
    public double LocalStellarMassDensitySolarPerPc3 { get; set; } = 0.04;

    /// <summary>
    /// Source identifiers backing the diagnostic budget surface.
    /// </summary>
    public string SourceIds { get; set; } = "BlandHawthornGerhard2016;Bovy2017;Kennicutt1998;HuntVasiliev2025";

    /// <summary>
    /// Implementation status for this budget surface.
    /// </summary>
    public string SourceStatus { get; set; } = "diagnostic_proxy";

    /// <summary>
    /// Audit note explaining how consumers should treat the budget.
    /// </summary>
    public string Notes { get; set; } = "Diagnostic mass components only; not a rotation curve, potential model, or star-placement driver.";

    /// <summary>
    /// Creates a detached copy of the budget.
    /// </summary>
    public GalaxyMassComponentBudget Clone()
    {
        return new GalaxyMassComponentBudget
        {
            TotalHaloMassSolar = TotalHaloMassSolar,
            DarkMatterHaloMassSolar = DarkMatterHaloMassSolar,
            BaryonicMassSolar = BaryonicMassSolar,
            StellarDiskMassSolar = StellarDiskMassSolar,
            StellarSpheroidMassSolar = StellarSpheroidMassSolar,
            StellarHaloMassSolar = StellarHaloMassSolar,
            NuclearStellarMassSolar = NuclearStellarMassSolar,
            ColdGasMassSolar = ColdGasMassSolar,
            HotGasMassSolar = HotGasMassSolar,
            BaryonFraction = BaryonFraction,
            GasFraction = GasFraction,
            LocalStellarMassDensitySolarPerPc3 = LocalStellarMassDensitySolarPerPc3,
            SourceIds = SourceIds,
            SourceStatus = SourceStatus,
            Notes = Notes,
        };
    }

    /// <summary>
    /// Creates a diagnostic budget from the resolved galaxy profile without affecting generation behavior.
    /// </summary>
    public static GalaxyMassComponentBudget CreateDiagnostic(GalaxyRealismProfile profile)
    {
        double totalHaloMass = System.Math.Pow(10.0, profile.HaloMassLog10Solar);
        double stellarMass = System.Math.Max(profile.StellarMassSolar, 0.0);
        double nuclearShare = ResolveNuclearShare(profile);
        double stellarHaloShare = ResolveStellarHaloShare(profile);
        double nuclearMass = stellarMass * nuclearShare;
        double stellarHaloMass = stellarMass * stellarHaloShare;
        double structuralStellarMass = System.Math.Max(0.0, stellarMass - nuclearMass - stellarHaloMass);
        double diskShare = ResolveDiskShare(profile);
        double stellarDiskMass = structuralStellarMass * diskShare;
        double stellarSpheroidMass = structuralStellarMass - stellarDiskMass;
        double coldGasMass = stellarMass * ResolveColdGasFraction(profile);
        double hotGasMass = System.Math.Min(totalHaloMass * ResolveHotGasFraction(profile), stellarMass * 4.0);
        double baryonicMass = stellarMass + coldGasMass + hotGasMass;
        double darkMatterMass = System.Math.Max(0.0, totalHaloMass - baryonicMass);
        double baryonFraction = 0.0;
        if (totalHaloMass > 0.0)
        {
            baryonFraction = baryonicMass / totalHaloMass;
        }

        double gasFraction = 0.0;
        if (baryonicMass > 0.0)
        {
            gasFraction = (coldGasMass + hotGasMass) / baryonicMass;
        }

        return new GalaxyMassComponentBudget
        {
            TotalHaloMassSolar = totalHaloMass,
            DarkMatterHaloMassSolar = darkMatterMass,
            BaryonicMassSolar = baryonicMass,
            StellarDiskMassSolar = stellarDiskMass,
            StellarSpheroidMassSolar = stellarSpheroidMass,
            StellarHaloMassSolar = stellarHaloMass,
            NuclearStellarMassSolar = nuclearMass,
            ColdGasMassSolar = coldGasMass,
            HotGasMassSolar = hotGasMass,
            BaryonFraction = baryonFraction,
            GasFraction = gasFraction,
            LocalStellarMassDensitySolarPerPc3 = ResolveLocalStellarMassDensity(profile),
            SourceIds = "BlandHawthornGerhard2016;Bovy2017;Kennicutt1998;HuntVasiliev2025",
            SourceStatus = "diagnostic_proxy",
            Notes = "Diagnostic mass components only; not a rotation curve, potential model, or star-placement driver.",
        };
    }

    /// <summary>
    /// Creates a dictionary payload for persistence and provenance.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["total_halo_mass_solar"] = TotalHaloMassSolar,
            ["dark_matter_halo_mass_solar"] = DarkMatterHaloMassSolar,
            ["baryonic_mass_solar"] = BaryonicMassSolar,
            ["stellar_disk_mass_solar"] = StellarDiskMassSolar,
            ["stellar_spheroid_mass_solar"] = StellarSpheroidMassSolar,
            ["stellar_halo_mass_solar"] = StellarHaloMassSolar,
            ["nuclear_stellar_mass_solar"] = NuclearStellarMassSolar,
            ["cold_gas_mass_solar"] = ColdGasMassSolar,
            ["hot_gas_mass_solar"] = HotGasMassSolar,
            ["baryon_fraction"] = BaryonFraction,
            ["gas_fraction"] = GasFraction,
            ["local_stellar_mass_density_solar_per_pc3"] = LocalStellarMassDensitySolarPerPc3,
            ["source_ids"] = SourceIds,
            ["source_status"] = SourceStatus,
            ["notes"] = Notes,
        };
    }

    /// <summary>
    /// Rebuilds a mass component budget from a serialized dictionary payload.
    /// </summary>
    public static GalaxyMassComponentBudget FromDictionary(Dictionary data)
    {
        GalaxyMassComponentBudget budget = new GalaxyMassComponentBudget
        {
            TotalHaloMassSolar = DomainDictionaryUtils.GetDouble(data, "total_halo_mass_solar", 1.0e12),
            DarkMatterHaloMassSolar = DomainDictionaryUtils.GetDouble(data, "dark_matter_halo_mass_solar", 9.25e11),
            BaryonicMassSolar = DomainDictionaryUtils.GetDouble(data, "baryonic_mass_solar", 7.5e10),
            StellarDiskMassSolar = DomainDictionaryUtils.GetDouble(data, "stellar_disk_mass_solar", 3.4e10),
            StellarSpheroidMassSolar = DomainDictionaryUtils.GetDouble(data, "stellar_spheroid_mass_solar", 1.5e10),
            StellarHaloMassSolar = DomainDictionaryUtils.GetDouble(data, "stellar_halo_mass_solar", 9.5e8),
            NuclearStellarMassSolar = DomainDictionaryUtils.GetDouble(data, "nuclear_stellar_mass_solar", 3.5e7),
            ColdGasMassSolar = DomainDictionaryUtils.GetDouble(data, "cold_gas_mass_solar", 6.0e9),
            HotGasMassSolar = DomainDictionaryUtils.GetDouble(data, "hot_gas_mass_solar", 1.9e10),
            BaryonFraction = DomainDictionaryUtils.GetDouble(data, "baryon_fraction", 0.075),
            GasFraction = DomainDictionaryUtils.GetDouble(data, "gas_fraction", 0.33),
            LocalStellarMassDensitySolarPerPc3 = DomainDictionaryUtils.GetDouble(data, "local_stellar_mass_density_solar_per_pc3", 0.04),
            SourceIds = DomainDictionaryUtils.GetString(data, "source_ids", "BlandHawthornGerhard2016;Bovy2017;Kennicutt1998;HuntVasiliev2025"),
            SourceStatus = DomainDictionaryUtils.GetString(data, "source_status", "diagnostic_proxy"),
            Notes = DomainDictionaryUtils.GetString(data, "notes", "Diagnostic mass components only; not a rotation curve, potential model, or star-placement driver."),
        };

        return budget;
    }

    private static double ResolveNuclearShare(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return 0.0015;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return 0.0002;
        }

        return 0.0007;
    }

    private static double ResolveStellarHaloShare(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return 0.08;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return 0.015;
        }

        return 0.02;
    }

    private static double ResolveDiskShare(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return 0.0;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return 0.75;
        }

        return System.Math.Clamp(1.0 - profile.BulgeToTotal, 0.0, 1.0);
    }

    private static double ResolveColdGasFraction(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return 0.01;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Lenticular)
        {
            return 0.03;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return 0.25;
        }

        double lateDiskPressure = System.Math.Clamp(1.0 - profile.BulgeToTotal, 0.0, 1.0);
        return 0.06 + (lateDiskPressure * 0.08);
    }

    private static double ResolveHotGasFraction(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return 0.04;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return 0.006;
        }

        return 0.018 + (profile.EnvironmentDensityIndex * 0.012);
    }

    private static double ResolveLocalStellarMassDensity(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return 0.025;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return 0.012;
        }

        return 0.040;
    }
}
