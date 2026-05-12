using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Diagnostic-only rotation-curve decomposition for galaxy-level audits and future dynamics work.
/// </summary>
public partial class GalaxyRotationCurveDiagnostic : RefCounted
{
    /// <summary>
    /// Reference radius for the decomposition in parsecs.
    /// </summary>
    public double ReferenceRadiusPc { get; set; } = 8200.0;

    /// <summary>
    /// Inner diagnostic circular velocity in kilometers per second.
    /// </summary>
    public double InnerVelocityKmS { get; set; } = 205.0;

    /// <summary>
    /// Diagnostic circular velocity at the reference radius in kilometers per second.
    /// </summary>
    public double ReferenceVelocityKmS { get; set; } = 240.0;

    /// <summary>
    /// Outer diagnostic circular velocity in kilometers per second.
    /// </summary>
    public double OuterVelocityKmS { get; set; } = 232.0;

    /// <summary>
    /// Disk contribution to the reference velocity in kilometers per second.
    /// </summary>
    public double DiskContributionKmS { get; set; } = 155.0;

    /// <summary>
    /// Spheroid contribution to the reference velocity in kilometers per second.
    /// </summary>
    public double SpheroidContributionKmS { get; set; } = 85.0;

    /// <summary>
    /// Gas contribution to the reference velocity in kilometers per second.
    /// </summary>
    public double GasContributionKmS { get; set; } = 35.0;

    /// <summary>
    /// Dark-matter contribution to the reference velocity in kilometers per second.
    /// </summary>
    public double DarkMatterContributionKmS { get; set; } = 160.0;

    /// <summary>
    /// Qualitative diagnostic curve shape.
    /// </summary>
    public string CurveShape { get; set; } = "flat_disk_halo_supported";

    /// <summary>
    /// Source identifiers backing the diagnostic rotation surface.
    /// </summary>
    public string SourceIds { get; set; } = "BlandHawthornGerhard2016;HuntVasiliev2025;Bovy2017";

    /// <summary>
    /// Implementation status for this rotation surface.
    /// </summary>
    public string SourceStatus { get; set; } = "diagnostic_proxy";

    /// <summary>
    /// Audit note explaining how consumers should treat the rotation diagnostic.
    /// </summary>
    public string Notes { get; set; } = "Diagnostic rotation curve only; not a gravitational potential, orbit integrator, or placement driver.";

    /// <summary>
    /// Creates a detached copy of the diagnostic.
    /// </summary>
    public GalaxyRotationCurveDiagnostic Clone()
    {
        return new GalaxyRotationCurveDiagnostic
        {
            ReferenceRadiusPc = ReferenceRadiusPc,
            InnerVelocityKmS = InnerVelocityKmS,
            ReferenceVelocityKmS = ReferenceVelocityKmS,
            OuterVelocityKmS = OuterVelocityKmS,
            DiskContributionKmS = DiskContributionKmS,
            SpheroidContributionKmS = SpheroidContributionKmS,
            GasContributionKmS = GasContributionKmS,
            DarkMatterContributionKmS = DarkMatterContributionKmS,
            CurveShape = CurveShape,
            SourceIds = SourceIds,
            SourceStatus = SourceStatus,
            Notes = Notes,
        };
    }

    /// <summary>
    /// Creates a diagnostic rotation-curve surface from the resolved profile and component budget.
    /// </summary>
    public static GalaxyRotationCurveDiagnostic CreateDiagnostic(
        GalaxyRealismProfile profile,
        GalaxyMassComponentBudget budget)
    {
        double referenceVelocity = System.Math.Clamp(profile.CircularVelocityAtSolarRadiusKmS, 120.0, 360.0);
        double diskWeight = System.Math.Max(budget.StellarDiskMassSolar, 0.0) * 1.15;
        double spheroidWeight = System.Math.Max(
            budget.StellarSpheroidMassSolar + budget.NuclearStellarMassSolar + budget.StellarHaloMassSolar,
            0.0);
        double gasWeight = System.Math.Max(budget.ColdGasMassSolar + budget.HotGasMassSolar, 0.0) * 0.55;
        double darkWeight = System.Math.Max(budget.DarkMatterHaloMassSolar, 0.0) * 0.18;
        double totalWeight = diskWeight + spheroidWeight + gasWeight + darkWeight;
        if (totalWeight <= 0.0)
        {
            totalWeight = 1.0;
            darkWeight = 1.0;
        }

        double diskContribution = ResolveVelocityContribution(referenceVelocity, diskWeight, totalWeight);
        double spheroidContribution = ResolveVelocityContribution(referenceVelocity, spheroidWeight, totalWeight);
        double gasContribution = ResolveVelocityContribution(referenceVelocity, gasWeight, totalWeight);
        double darkContribution = ResolveVelocityContribution(referenceVelocity, darkWeight, totalWeight);
        double diskPressure = diskWeight / totalWeight;
        double spheroidPressure = spheroidWeight / totalWeight;
        double darkPressure = darkWeight / totalWeight;

        return new GalaxyRotationCurveDiagnostic
        {
            ReferenceRadiusPc = profile.SolarGalactocentricRadiusPc,
            InnerVelocityKmS = ResolveInnerVelocity(referenceVelocity, diskPressure, spheroidPressure),
            ReferenceVelocityKmS = referenceVelocity,
            OuterVelocityKmS = ResolveOuterVelocity(referenceVelocity, darkPressure),
            DiskContributionKmS = diskContribution,
            SpheroidContributionKmS = spheroidContribution,
            GasContributionKmS = gasContribution,
            DarkMatterContributionKmS = darkContribution,
            CurveShape = ResolveCurveShape(profile, darkPressure, diskPressure),
            SourceIds = "BlandHawthornGerhard2016;HuntVasiliev2025;Bovy2017",
            SourceStatus = "diagnostic_proxy",
            Notes = "Diagnostic rotation curve only; not a gravitational potential, orbit integrator, or placement driver.",
        };
    }

    /// <summary>
    /// Creates a dictionary payload for persistence and provenance.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["reference_radius_pc"] = ReferenceRadiusPc,
            ["inner_velocity_km_s"] = InnerVelocityKmS,
            ["reference_velocity_km_s"] = ReferenceVelocityKmS,
            ["outer_velocity_km_s"] = OuterVelocityKmS,
            ["disk_contribution_km_s"] = DiskContributionKmS,
            ["spheroid_contribution_km_s"] = SpheroidContributionKmS,
            ["gas_contribution_km_s"] = GasContributionKmS,
            ["dark_matter_contribution_km_s"] = DarkMatterContributionKmS,
            ["curve_shape"] = CurveShape,
            ["source_ids"] = SourceIds,
            ["source_status"] = SourceStatus,
            ["notes"] = Notes,
        };
    }

    /// <summary>
    /// Rebuilds a rotation diagnostic from a serialized dictionary payload.
    /// </summary>
    public static GalaxyRotationCurveDiagnostic FromDictionary(Dictionary data)
    {
        return new GalaxyRotationCurveDiagnostic
        {
            ReferenceRadiusPc = DomainDictionaryUtils.GetDouble(data, "reference_radius_pc", 8200.0),
            InnerVelocityKmS = DomainDictionaryUtils.GetDouble(data, "inner_velocity_km_s", 205.0),
            ReferenceVelocityKmS = DomainDictionaryUtils.GetDouble(data, "reference_velocity_km_s", 240.0),
            OuterVelocityKmS = DomainDictionaryUtils.GetDouble(data, "outer_velocity_km_s", 232.0),
            DiskContributionKmS = DomainDictionaryUtils.GetDouble(data, "disk_contribution_km_s", 155.0),
            SpheroidContributionKmS = DomainDictionaryUtils.GetDouble(data, "spheroid_contribution_km_s", 85.0),
            GasContributionKmS = DomainDictionaryUtils.GetDouble(data, "gas_contribution_km_s", 35.0),
            DarkMatterContributionKmS = DomainDictionaryUtils.GetDouble(data, "dark_matter_contribution_km_s", 160.0),
            CurveShape = DomainDictionaryUtils.GetString(data, "curve_shape", "flat_disk_halo_supported"),
            SourceIds = DomainDictionaryUtils.GetString(data, "source_ids", "BlandHawthornGerhard2016;HuntVasiliev2025;Bovy2017"),
            SourceStatus = DomainDictionaryUtils.GetString(data, "source_status", "diagnostic_proxy"),
            Notes = DomainDictionaryUtils.GetString(data, "notes", "Diagnostic rotation curve only; not a gravitational potential, orbit integrator, or placement driver."),
        };
    }

    private static double ResolveVelocityContribution(double referenceVelocity, double componentWeight, double totalWeight)
    {
        double share = System.Math.Clamp(componentWeight / totalWeight, 0.0, 1.0);
        return referenceVelocity * System.Math.Sqrt(share);
    }

    private static double ResolveInnerVelocity(double referenceVelocity, double diskPressure, double spheroidPressure)
    {
        double innerScalar = 0.72 + (spheroidPressure * 0.34) + (diskPressure * 0.08);
        return System.Math.Clamp(referenceVelocity * innerScalar, referenceVelocity * 0.45, referenceVelocity * 1.20);
    }

    private static double ResolveOuterVelocity(double referenceVelocity, double darkPressure)
    {
        double outerScalar = 0.82 + (darkPressure * 0.28);
        return System.Math.Clamp(referenceVelocity * outerScalar, referenceVelocity * 0.55, referenceVelocity * 1.15);
    }

    private static string ResolveCurveShape(GalaxyRealismProfile profile, double darkPressure, double diskPressure)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return "spheroid_supported_diagnostic";
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return "rising_dwarf_proxy";
        }

        if (darkPressure >= 0.60 && diskPressure >= 0.15)
        {
            return "flat_disk_halo_supported";
        }

        if (darkPressure >= 0.60)
        {
            return "halo_supported_diagnostic";
        }

        return "baryon_weighted_proxy";
    }
}
