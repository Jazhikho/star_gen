using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Diagnostic-only galactic dynamics and local mass-budget surface for audits and future behavior changes.
/// </summary>
public partial class GalaxyDynamicsDiagnostic : RefCounted
{
    /// <summary>
    /// Diagnostic bar pattern speed in kilometers per second per kiloparsec.
    /// </summary>
    public double BarPatternSpeedKmSPerKpc { get; set; }

    /// <summary>
    /// Diagnostic bar corotation radius in parsecs.
    /// </summary>
    public double CorotationRadiusPc { get; set; }

    /// <summary>
    /// Corotation radius divided by bar half-length.
    /// </summary>
    public double CorotationToBarLengthRatio { get; set; }

    /// <summary>
    /// Local total mass-density proxy in solar masses per cubic parsec.
    /// </summary>
    public double LocalTotalMassDensitySolarPerPc3 { get; set; } = 0.10;

    /// <summary>
    /// Local baryonic mass-density proxy in solar masses per cubic parsec.
    /// </summary>
    public double LocalBaryonicMassDensitySolarPerPc3 { get; set; } = 0.09;

    /// <summary>
    /// Local dark-matter mass-density proxy in solar masses per cubic parsec.
    /// </summary>
    public double LocalDarkMatterDensitySolarPerPc3 { get; set; } = 0.01;

    /// <summary>
    /// Local surface-density proxy near the solar neighborhood in solar masses per square parsec.
    /// </summary>
    public double LocalSurfaceDensitySolarPerPc2 { get; set; } = 70.0;

    /// <summary>
    /// Calibration scope for interpreting the galaxy diagnostics.
    /// </summary>
    public string AnalogCalibrationMode { get; set; } = "milky_way_analog";

    /// <summary>
    /// Status of non-Milky-Way comparison behavior.
    /// </summary>
    public string NonMilkyWayComparisonStatus { get; set; } = "milky_way_anchor_only";

    /// <summary>
    /// Source identifiers backing this diagnostic surface.
    /// </summary>
    public string SourceIds { get; set; } = "BlandHawthornGerhard2016;Bovy2017;KhoperskovEtAl2024;HuntVasiliev2025";

    /// <summary>
    /// Implementation status for this dynamics surface.
    /// </summary>
    public string SourceStatus { get; set; } = "diagnostic_proxy";

    /// <summary>
    /// Audit note explaining how consumers should treat the diagnostic.
    /// </summary>
    public string Notes { get; set; } = "Diagnostic dynamics only; not a gravitational potential, orbit integrator, placement driver, or calibrated non-Milky-Way analog.";

    /// <summary>
    /// Creates a detached copy of the diagnostic.
    /// </summary>
    public GalaxyDynamicsDiagnostic Clone()
    {
        return new GalaxyDynamicsDiagnostic
        {
            BarPatternSpeedKmSPerKpc = BarPatternSpeedKmSPerKpc,
            CorotationRadiusPc = CorotationRadiusPc,
            CorotationToBarLengthRatio = CorotationToBarLengthRatio,
            LocalTotalMassDensitySolarPerPc3 = LocalTotalMassDensitySolarPerPc3,
            LocalBaryonicMassDensitySolarPerPc3 = LocalBaryonicMassDensitySolarPerPc3,
            LocalDarkMatterDensitySolarPerPc3 = LocalDarkMatterDensitySolarPerPc3,
            LocalSurfaceDensitySolarPerPc2 = LocalSurfaceDensitySolarPerPc2,
            AnalogCalibrationMode = AnalogCalibrationMode,
            NonMilkyWayComparisonStatus = NonMilkyWayComparisonStatus,
            SourceIds = SourceIds,
            SourceStatus = SourceStatus,
            Notes = Notes,
        };
    }

    /// <summary>
    /// Creates a diagnostic dynamics surface from resolved galaxy fields.
    /// </summary>
    public static GalaxyDynamicsDiagnostic CreateDiagnostic(
        GalaxyRealismProfile profile,
        GalaxyMassComponentBudget budget,
        GalaxyRotationCurveDiagnostic rotationCurve)
    {
        double corotationRadius = ResolveCorotationRadius(profile);
        double patternSpeed = ResolvePatternSpeed(profile, corotationRadius, rotationCurve);
        double ratio = ResolveCorotationRatio(profile, corotationRadius);
        double localStellarDensity = budget.LocalStellarMassDensitySolarPerPc3;
        double localGasDensity = ResolveLocalGasDensity(profile, budget);
        double localBaryonicDensity = localStellarDensity + localGasDensity;
        double localDarkDensity = ResolveLocalDarkDensity(profile, budget);

        return new GalaxyDynamicsDiagnostic
        {
            BarPatternSpeedKmSPerKpc = patternSpeed,
            CorotationRadiusPc = corotationRadius,
            CorotationToBarLengthRatio = ratio,
            LocalTotalMassDensitySolarPerPc3 = localBaryonicDensity + localDarkDensity,
            LocalBaryonicMassDensitySolarPerPc3 = localBaryonicDensity,
            LocalDarkMatterDensitySolarPerPc3 = localDarkDensity,
            LocalSurfaceDensitySolarPerPc2 = ResolveLocalSurfaceDensity(profile, localBaryonicDensity, localDarkDensity),
            AnalogCalibrationMode = ResolveAnalogCalibrationMode(profile),
            NonMilkyWayComparisonStatus = ResolveNonMilkyWayComparisonStatus(profile),
            SourceIds = "BlandHawthornGerhard2016;Bovy2017;KhoperskovEtAl2024;HuntVasiliev2025",
            SourceStatus = "diagnostic_proxy",
            Notes = "Diagnostic dynamics only; not a gravitational potential, orbit integrator, placement driver, or calibrated non-Milky-Way analog.",
        };
    }

    /// <summary>
    /// Creates a dictionary payload for persistence and provenance.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["bar_pattern_speed_km_s_per_kpc"] = BarPatternSpeedKmSPerKpc,
            ["corotation_radius_pc"] = CorotationRadiusPc,
            ["corotation_to_bar_length_ratio"] = CorotationToBarLengthRatio,
            ["local_total_mass_density_solar_per_pc3"] = LocalTotalMassDensitySolarPerPc3,
            ["local_baryonic_mass_density_solar_per_pc3"] = LocalBaryonicMassDensitySolarPerPc3,
            ["local_dark_matter_density_solar_per_pc3"] = LocalDarkMatterDensitySolarPerPc3,
            ["local_surface_density_solar_per_pc2"] = LocalSurfaceDensitySolarPerPc2,
            ["analog_calibration_mode"] = AnalogCalibrationMode,
            ["non_milky_way_comparison_status"] = NonMilkyWayComparisonStatus,
            ["source_ids"] = SourceIds,
            ["source_status"] = SourceStatus,
            ["notes"] = Notes,
        };
    }

    /// <summary>
    /// Rebuilds a dynamics diagnostic from a serialized dictionary payload.
    /// </summary>
    public static GalaxyDynamicsDiagnostic FromDictionary(Dictionary data)
    {
        return new GalaxyDynamicsDiagnostic
        {
            BarPatternSpeedKmSPerKpc = DomainDictionaryUtils.GetDouble(data, "bar_pattern_speed_km_s_per_kpc", 0.0),
            CorotationRadiusPc = DomainDictionaryUtils.GetDouble(data, "corotation_radius_pc", 0.0),
            CorotationToBarLengthRatio = DomainDictionaryUtils.GetDouble(data, "corotation_to_bar_length_ratio", 0.0),
            LocalTotalMassDensitySolarPerPc3 = DomainDictionaryUtils.GetDouble(data, "local_total_mass_density_solar_per_pc3", 0.10),
            LocalBaryonicMassDensitySolarPerPc3 = DomainDictionaryUtils.GetDouble(data, "local_baryonic_mass_density_solar_per_pc3", 0.09),
            LocalDarkMatterDensitySolarPerPc3 = DomainDictionaryUtils.GetDouble(data, "local_dark_matter_density_solar_per_pc3", 0.01),
            LocalSurfaceDensitySolarPerPc2 = DomainDictionaryUtils.GetDouble(data, "local_surface_density_solar_per_pc2", 70.0),
            AnalogCalibrationMode = DomainDictionaryUtils.GetString(data, "analog_calibration_mode", "milky_way_analog"),
            NonMilkyWayComparisonStatus = DomainDictionaryUtils.GetString(data, "non_milky_way_comparison_status", "milky_way_anchor_only"),
            SourceIds = DomainDictionaryUtils.GetString(data, "source_ids", "BlandHawthornGerhard2016;Bovy2017;KhoperskovEtAl2024;HuntVasiliev2025"),
            SourceStatus = DomainDictionaryUtils.GetString(data, "source_status", "diagnostic_proxy"),
            Notes = DomainDictionaryUtils.GetString(data, "notes", "Diagnostic dynamics only; not a gravitational potential, orbit integrator, placement driver, or calibrated non-Milky-Way analog."),
        };
    }

    private static double ResolveCorotationRadius(GalaxyRealismProfile profile)
    {
        if (!profile.IsBarred || profile.BarHalfLengthPc <= 0.0)
        {
            return 0.0;
        }

        double ratio = 1.18 + (profile.EnvironmentDensityIndex * 0.14);
        return profile.BarHalfLengthPc * ratio;
    }

    private static double ResolvePatternSpeed(
        GalaxyRealismProfile profile,
        double corotationRadiusPc,
        GalaxyRotationCurveDiagnostic rotationCurve)
    {
        if (!profile.IsBarred || corotationRadiusPc <= 0.0)
        {
            return 0.0;
        }

        double corotationRadiusKpc = corotationRadiusPc / 1000.0;
        double rawPatternSpeed = rotationCurve.ReferenceVelocityKmS / corotationRadiusKpc;
        return System.Math.Clamp(rawPatternSpeed, 25.0, 65.0);
    }

    private static double ResolveCorotationRatio(GalaxyRealismProfile profile, double corotationRadiusPc)
    {
        if (!profile.IsBarred || profile.BarHalfLengthPc <= 0.0)
        {
            return 0.0;
        }

        return corotationRadiusPc / profile.BarHalfLengthPc;
    }

    private static double ResolveLocalGasDensity(GalaxyRealismProfile profile, GalaxyMassComponentBudget budget)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return 0.002;
        }

        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            return 0.008;
        }

        double gasShare = budget.GasFraction;
        return System.Math.Clamp(gasShare * 0.035, 0.003, 0.018);
    }

    private static double ResolveLocalDarkDensity(GalaxyRealismProfile profile, GalaxyMassComponentBudget budget)
    {
        double darkFraction = 0.0;
        if (budget.TotalHaloMassSolar > 0.0)
        {
            darkFraction = budget.DarkMatterHaloMassSolar / budget.TotalHaloMassSolar;
        }

        double density = 0.008 + (darkFraction * 0.004);
        if (profile.Family == GalaxySpec.GalaxyType.Irregular)
        {
            density += 0.006;
        }

        return System.Math.Clamp(density, 0.004, 0.030);
    }

    private static double ResolveLocalSurfaceDensity(
        GalaxyRealismProfile profile,
        double localBaryonicDensity,
        double localDarkDensity)
    {
        double verticalScalePc = System.Math.Max(profile.ThinDiskScaleHeightPc, 100.0);
        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            verticalScalePc = System.Math.Max(profile.EffectiveRadiusPc * 0.12, 300.0);
        }

        double surfaceDensity = (localBaryonicDensity + (localDarkDensity * 0.5)) * verticalScalePc * 2.0;
        return System.Math.Clamp(surfaceDensity, 10.0, 180.0);
    }

    private static string ResolveAnalogCalibrationMode(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Spiral && profile.ResolvedSubtype == GalaxyResolvedSubtype.SpiralSb)
        {
            return "milky_way_analog";
        }

        if (profile.Family == GalaxySpec.GalaxyType.Spiral)
        {
            return "spiral_family_comparison_preset";
        }

        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return "elliptical_family_comparison_preset";
        }

        if (profile.Family == GalaxySpec.GalaxyType.Lenticular)
        {
            return "lenticular_family_comparison_preset";
        }

        return "irregular_family_comparison_preset";
    }

    private static string ResolveNonMilkyWayComparisonStatus(GalaxyRealismProfile profile)
    {
        if (profile.Family == GalaxySpec.GalaxyType.Spiral && profile.ResolvedSubtype == GalaxyResolvedSubtype.SpiralSb)
        {
            return "milky_way_anchor_only";
        }

        if (profile.Family == GalaxySpec.GalaxyType.Spiral)
        {
            return "non_milky_way_spiral_comparison_sources";
        }

        if (profile.Family == GalaxySpec.GalaxyType.Elliptical)
        {
            return "elliptical_comparison_sources";
        }

        if (profile.Family == GalaxySpec.GalaxyType.Lenticular)
        {
            return "lenticular_comparison_sources";
        }

        return "irregular_comparison_sources";
    }
}
