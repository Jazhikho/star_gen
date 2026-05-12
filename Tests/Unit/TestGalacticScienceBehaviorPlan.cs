#nullable enable annotations
#nullable disable warnings
using System;
using System.IO;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Regression tests for the galactic science behavior-plan baseline.
/// </summary>
public static class TestGalacticScienceBehaviorPlan
{
    /// <summary>
    /// P2P: every current diagnostic field round-trips through spec and realism-profile serialization.
    /// </summary>
    public static void TestP2PDiagnosticFieldsRoundTripEveryCurrentField()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(18101);
        GalaxyMassComponentBudget budget = BuildSentinelBudget();
        GalaxyRotationCurveDiagnostic rotation = BuildSentinelRotation();
        GalaxyDynamicsDiagnostic dynamics = BuildSentinelDynamics();

        spec.MassComponentBudget = budget;
        spec.RotationCurveDiagnostic = rotation;
        spec.DynamicsDiagnostic = dynamics;
        spec.RealismProfile.MassComponentBudget = budget.Clone();
        spec.RealismProfile.RotationCurveDiagnostic = rotation.Clone();
        spec.RealismProfile.DynamicsDiagnostic = dynamics.Clone();

        GalaxySpec restored = GalaxySpec.FromDictionary(spec.ToDictionary());

        AssertBudgetEquals(budget, restored.MassComponentBudget, "spec");
        AssertRotationEquals(rotation, restored.RotationCurveDiagnostic, "spec");
        AssertDynamicsEquals(dynamics, restored.DynamicsDiagnostic, "spec");
        AssertBudgetEquals(budget, restored.RealismProfile.MassComponentBudget, "profile");
        AssertRotationEquals(rotation, restored.RealismProfile.RotationCurveDiagnostic, "profile");
        AssertDynamicsEquals(dynamics, restored.RealismProfile.DynamicsDiagnostic, "profile");
    }

    /// <summary>
    /// P2P: diagnostic-only fields must not affect density sampling or rendered star positions.
    /// </summary>
    public static void TestP2PDiagnosticOnlyFieldsDoNotChangeDensitySampling()
    {
        GalaxySpec baseline = GalaxySpec.CreateMilkyWay(18102);
        GalaxySpec diagnosticMutated = GalaxySpec.FromDictionary(baseline.ToDictionary());
        ReplaceDiagnosticsWithExtremeNonBehavioralValues(diagnosticMutated);

        GalaxySample baselineSample = DensitySampler.SampleGalaxy(baseline, 256, new SeededRng(18202));
        GalaxySample mutatedSample = DensitySampler.SampleGalaxy(diagnosticMutated, 256, new SeededRng(18202));

        AssertSampleEquals(baselineSample.BulgePoints, mutatedSample.BulgePoints, "bulge");
        AssertSampleEquals(baselineSample.DiskPoints, mutatedSample.DiskPoints, "disk");
    }

    /// <summary>
    /// P2P: diagnostic-only fields must not alter local origin context before behavior mode is added.
    /// </summary>
    public static void TestP2PDiagnosticOnlyFieldsDoNotChangeOriginContext()
    {
        GalaxySpec baseline = GalaxySpec.CreateMilkyWay(18103);
        GalaxySpec diagnosticMutated = GalaxySpec.FromDictionary(baseline.ToDictionary());
        ReplaceDiagnosticsWithExtremeNonBehavioralValues(diagnosticMutated);

        Vector3[] samples = new Vector3[]
        {
            new Vector3(3000.0f, 0.0f, 0.0f),
            new Vector3(7200.0f, 0.0f, 2200.0f),
            new Vector3(11600.0f, 180.0f, -700.0f),
        };

        for (int index = 0; index < samples.Length; index += 1)
        {
            GalaxyOriginContext baselineContext = GalaxyScientificFieldEvaluator.Evaluate(samples[index], baseline);
            GalaxyOriginContext mutatedContext = GalaxyScientificFieldEvaluator.Evaluate(samples[index], diagnosticMutated);
            AssertContextEquals(baselineContext, mutatedContext, $"sample {index}");
        }
    }

    /// <summary>
    /// P2P: every non-Sb family currently exposes comparison-source caveats before active behavior.
    /// </summary>
    public static void TestP2PNonMilkyWayFamiliesCarryComparisonCaveats()
    {
        GalaxyConfig[] configs = new GalaxyConfig[]
        {
            BuildSpiralLateConfig(),
            BuildFamilyConfig(GalaxySpec.GalaxyType.Elliptical),
            BuildFamilyConfig(GalaxySpec.GalaxyType.Lenticular),
            BuildFamilyConfig(GalaxySpec.GalaxyType.Irregular),
        };

        for (int index = 0; index < configs.Length; index += 1)
        {
            GalaxySpec spec = GalaxySpec.CreateFromConfig(configs[index], 18104 + index);
            DotNetNativeTestSuite.AssertTrue(spec.ResolvedSubtype != GalaxyResolvedSubtype.SpiralSb, $"test config {index} should exercise a non-Sb subtype");
            DotNetNativeTestSuite.AssertEqual("family_proxy_pending_comparison", spec.DynamicsDiagnostic.AnalogCalibrationMode, $"non-Sb config {index} should not claim Milky Way analog calibration");
            DotNetNativeTestSuite.AssertEqual("needs_non_milky_way_comparison_sources", spec.DynamicsDiagnostic.NonMilkyWayComparisonStatus, $"non-Sb config {index} should carry source caveat");
        }
    }

    /// <summary>
    /// F2P: the behavior plan must keep Studio, Viewer, and behavior-gate requirements explicit.
    /// </summary>
    public static void TestF2PBehaviorPlanCoversStudioViewerAndDomainGates()
    {
        string plan = ReadBehaviorPlan();
        string[] requiredFragments = new string[]
        {
            "P2P",
            "F2P",
            "Resolved Galactic Diagnostics",
            "diagnostic-only fields clearly as inactive for generation behavior",
            "`Dynamics Mode`: `Diagnostics Only`",
            "`Affect Region Context`",
            "`Affect Placement`",
            "`Science Diagnostics` inspector section",
            "viewer overlays first, not generation drivers",
            "A future `GalaxyDynamicsBehaviorMode` should gate all behavioral effects",
            "Stage 1: enrich `GalaxyOriginContext`",
            "Stage 2: adjust hazard, age cohort, cluster probability, and local stellar-profile context",
            "Stage 3: optionally alter star placement/density sampling",
            "source-backed non-Milky-Way family comparison presets",
        };

        AssertContainsAll(plan, requiredFragments);
    }

    /// <summary>
    /// F2P: the behavior plan must state the test surfaces required before future activation.
    /// </summary>
    public static void TestF2PBehaviorPlanCoversFutureTestRequirements()
    {
        string plan = ReadBehaviorPlan();
        string[] requiredFragments = new string[]
        {
            "Serialization round-trip tests for every diagnostic field and future behavior flag",
            "Galaxy Studio tests proving diagnostics are visible but controls remain disabled",
            "Galaxy Viewer tests proving readouts surface diagnostic fields without mutating generation",
            "Regression tests proving default galaxy seeds produce the same star positions",
            "Behavior-mode tests, when added, proving each active mode changes only the documented downstream surface",
        };

        AssertContainsAll(plan, requiredFragments);
    }

    private static GalaxyMassComponentBudget BuildSentinelBudget()
    {
        return new GalaxyMassComponentBudget
        {
            TotalHaloMassSolar = 9.91e11,
            DarkMatterHaloMassSolar = 8.71e11,
            BaryonicMassSolar = 1.20e11,
            StellarDiskMassSolar = 4.11e10,
            StellarSpheroidMassSolar = 2.22e10,
            StellarHaloMassSolar = 1.33e9,
            NuclearStellarMassSolar = 4.44e7,
            ColdGasMassSolar = 7.55e9,
            HotGasMassSolar = 2.66e10,
            BaryonFraction = 0.1211,
            GasFraction = 0.2844,
            LocalStellarMassDensitySolarPerPc3 = 0.052,
            SourceIds = "sentinel_budget_source_a;sentinel_budget_source_b",
            SourceStatus = "sentinel_budget_status",
            Notes = "sentinel budget notes",
        };
    }

    private static GalaxyRotationCurveDiagnostic BuildSentinelRotation()
    {
        return new GalaxyRotationCurveDiagnostic
        {
            ReferenceRadiusPc = 8123.0,
            InnerVelocityKmS = 198.0,
            ReferenceVelocityKmS = 231.0,
            OuterVelocityKmS = 227.0,
            DiskContributionKmS = 144.0,
            SpheroidContributionKmS = 76.0,
            GasContributionKmS = 41.0,
            DarkMatterContributionKmS = 169.0,
            CurveShape = "sentinel_curve_shape",
            SourceIds = "sentinel_rotation_source_a;sentinel_rotation_source_b",
            SourceStatus = "sentinel_rotation_status",
            Notes = "sentinel rotation notes",
        };
    }

    private static GalaxyDynamicsDiagnostic BuildSentinelDynamics()
    {
        return new GalaxyDynamicsDiagnostic
        {
            BarPatternSpeedKmSPerKpc = 44.0,
            CorotationRadiusPc = 5310.0,
            CorotationToBarLengthRatio = 1.18,
            LocalTotalMassDensitySolarPerPc3 = 0.104,
            LocalBaryonicMassDensitySolarPerPc3 = 0.092,
            LocalDarkMatterDensitySolarPerPc3 = 0.012,
            LocalSurfaceDensitySolarPerPc2 = 68.0,
            AnalogCalibrationMode = "sentinel_analog_mode",
            NonMilkyWayComparisonStatus = "sentinel_comparison_status",
            SourceIds = "sentinel_dynamics_source_a;sentinel_dynamics_source_b",
            SourceStatus = "sentinel_dynamics_status",
            Notes = "sentinel dynamics notes",
        };
    }

    private static void ReplaceDiagnosticsWithExtremeNonBehavioralValues(GalaxySpec spec)
    {
        GalaxyMassComponentBudget budget = BuildSentinelBudget();
        budget.TotalHaloMassSolar = 3.0e13;
        budget.DarkMatterHaloMassSolar = 2.9e13;
        budget.LocalStellarMassDensitySolarPerPc3 = 0.001;

        GalaxyRotationCurveDiagnostic rotation = BuildSentinelRotation();
        rotation.ReferenceVelocityKmS = 55.0;
        rotation.InnerVelocityKmS = 25.0;
        rotation.OuterVelocityKmS = 360.0;

        GalaxyDynamicsDiagnostic dynamics = BuildSentinelDynamics();
        dynamics.BarPatternSpeedKmSPerKpc = 5.0;
        dynamics.CorotationRadiusPc = 25000.0;
        dynamics.LocalTotalMassDensitySolarPerPc3 = 9.5;
        dynamics.AnalogCalibrationMode = "extreme_nonbehavioral_sentinel";

        spec.MassComponentBudget = budget;
        spec.RotationCurveDiagnostic = rotation;
        spec.DynamicsDiagnostic = dynamics;
        spec.RealismProfile.MassComponentBudget = budget.Clone();
        spec.RealismProfile.RotationCurveDiagnostic = rotation.Clone();
        spec.RealismProfile.DynamicsDiagnostic = dynamics.Clone();
    }

    private static GalaxyConfig BuildSpiralLateConfig()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.Type = GalaxySpec.GalaxyType.Spiral;
        config.SubtypeMode = GalaxySubtypeMode.LateType;
        return config;
    }

    private static GalaxyConfig BuildFamilyConfig(GalaxySpec.GalaxyType family)
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.Type = family;
        config.SubtypeMode = GalaxySubtypeMode.LateType;
        return config;
    }

    private static string ReadBehaviorPlan()
    {
        string path = Path.Combine("Docs", "GalacticScienceBehaviorPlan.md");
        DotNetNativeTestSuite.AssertTrue(File.Exists(path), "galactic science behavior plan should exist");
        return File.ReadAllText(path);
    }

    private static void AssertContainsAll(string text, string[] fragments)
    {
        for (int index = 0; index < fragments.Length; index += 1)
        {
            DotNetNativeTestSuite.AssertTrue(text.Contains(fragments[index]), $"plan should contain '{fragments[index]}'");
        }
    }

    private static void AssertBudgetEquals(GalaxyMassComponentBudget expected, GalaxyMassComponentBudget actual, string context)
    {
        DotNetNativeTestSuite.AssertEqual(expected.TotalHaloMassSolar, actual.TotalHaloMassSolar, $"{context} budget total halo mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.DarkMatterHaloMassSolar, actual.DarkMatterHaloMassSolar, $"{context} budget dark halo mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.BaryonicMassSolar, actual.BaryonicMassSolar, $"{context} budget baryonic mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.StellarDiskMassSolar, actual.StellarDiskMassSolar, $"{context} budget stellar disk mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.StellarSpheroidMassSolar, actual.StellarSpheroidMassSolar, $"{context} budget spheroid mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.StellarHaloMassSolar, actual.StellarHaloMassSolar, $"{context} budget stellar halo mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.NuclearStellarMassSolar, actual.NuclearStellarMassSolar, $"{context} budget nuclear mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.ColdGasMassSolar, actual.ColdGasMassSolar, $"{context} budget cold gas should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.HotGasMassSolar, actual.HotGasMassSolar, $"{context} budget hot gas should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.BaryonFraction, actual.BaryonFraction, $"{context} budget baryon fraction should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.GasFraction, actual.GasFraction, $"{context} budget gas fraction should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.LocalStellarMassDensitySolarPerPc3, actual.LocalStellarMassDensitySolarPerPc3, $"{context} budget local stellar density should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.SourceIds, actual.SourceIds, $"{context} budget sources should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.SourceStatus, actual.SourceStatus, $"{context} budget status should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.Notes, actual.Notes, $"{context} budget notes should round-trip");
    }

    private static void AssertRotationEquals(GalaxyRotationCurveDiagnostic expected, GalaxyRotationCurveDiagnostic actual, string context)
    {
        DotNetNativeTestSuite.AssertEqual(expected.ReferenceRadiusPc, actual.ReferenceRadiusPc, $"{context} rotation reference radius should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.InnerVelocityKmS, actual.InnerVelocityKmS, $"{context} rotation inner velocity should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.ReferenceVelocityKmS, actual.ReferenceVelocityKmS, $"{context} rotation reference velocity should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.OuterVelocityKmS, actual.OuterVelocityKmS, $"{context} rotation outer velocity should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.DiskContributionKmS, actual.DiskContributionKmS, $"{context} rotation disk contribution should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.SpheroidContributionKmS, actual.SpheroidContributionKmS, $"{context} rotation spheroid contribution should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.GasContributionKmS, actual.GasContributionKmS, $"{context} rotation gas contribution should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.DarkMatterContributionKmS, actual.DarkMatterContributionKmS, $"{context} rotation dark contribution should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.CurveShape, actual.CurveShape, $"{context} rotation curve shape should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.SourceIds, actual.SourceIds, $"{context} rotation sources should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.SourceStatus, actual.SourceStatus, $"{context} rotation status should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.Notes, actual.Notes, $"{context} rotation notes should round-trip");
    }

    private static void AssertDynamicsEquals(GalaxyDynamicsDiagnostic expected, GalaxyDynamicsDiagnostic actual, string context)
    {
        DotNetNativeTestSuite.AssertEqual(expected.BarPatternSpeedKmSPerKpc, actual.BarPatternSpeedKmSPerKpc, $"{context} dynamics pattern speed should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.CorotationRadiusPc, actual.CorotationRadiusPc, $"{context} dynamics corotation should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.CorotationToBarLengthRatio, actual.CorotationToBarLengthRatio, $"{context} dynamics corotation ratio should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.LocalTotalMassDensitySolarPerPc3, actual.LocalTotalMassDensitySolarPerPc3, $"{context} dynamics total density should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.LocalBaryonicMassDensitySolarPerPc3, actual.LocalBaryonicMassDensitySolarPerPc3, $"{context} dynamics baryonic density should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.LocalDarkMatterDensitySolarPerPc3, actual.LocalDarkMatterDensitySolarPerPc3, $"{context} dynamics dark density should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.LocalSurfaceDensitySolarPerPc2, actual.LocalSurfaceDensitySolarPerPc2, $"{context} dynamics surface density should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.AnalogCalibrationMode, actual.AnalogCalibrationMode, $"{context} dynamics analog mode should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.NonMilkyWayComparisonStatus, actual.NonMilkyWayComparisonStatus, $"{context} dynamics comparison status should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.SourceIds, actual.SourceIds, $"{context} dynamics sources should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.SourceStatus, actual.SourceStatus, $"{context} dynamics status should round-trip");
        DotNetNativeTestSuite.AssertEqual(expected.Notes, actual.Notes, $"{context} dynamics notes should round-trip");
    }

    private static void AssertSampleEquals(Vector3[] expected, Vector3[] actual, string population)
    {
        DotNetNativeTestSuite.AssertEqual(expected.Length, actual.Length, $"{population} sample count should remain unchanged");
        for (int index = 0; index < expected.Length; index += 1)
        {
            DotNetNativeTestSuite.AssertTrue(expected[index].IsEqualApprox(actual[index]), $"{population} sample {index} should remain unchanged");
        }
    }

    private static void AssertContextEquals(GalaxyOriginContext expected, GalaxyOriginContext actual, string context)
    {
        DotNetNativeTestSuite.AssertEqual((int)expected.ResolvedSubtype, (int)actual.ResolvedSubtype, $"{context} subtype should remain unchanged");
        DotNetNativeTestSuite.AssertEqual((int)expected.RegionKind, (int)actual.RegionKind, $"{context} region should remain unchanged");
        DotNetNativeTestSuite.AssertEqual((int)expected.AgeCohort, (int)actual.AgeCohort, $"{context} age cohort should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.MetallicityPrior, actual.MetallicityPrior, $"{context} metallicity should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.AgeBias, actual.AgeBias, $"{context} age bias should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.AgeMeanGyr, actual.AgeMeanGyr, $"{context} mean age should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.GhzWeight, actual.GhzWeight, $"{context} GHZ weight should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.HazardWeight, actual.HazardWeight, $"{context} hazard weight should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.ClusterProbability, actual.ClusterProbability, $"{context} cluster probability should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.IsBarInfluenced, actual.IsBarInfluenced, $"{context} bar influence should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.IsArmInfluenced, actual.IsArmInfluenced, $"{context} arm influence should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.LocalDensityRatio, actual.LocalDensityRatio, $"{context} density ratio should remain unchanged");
        DotNetNativeTestSuite.AssertEqual(expected.LocalStarFormationEfficiency, actual.LocalStarFormationEfficiency, $"{context} local star-formation efficiency should remain unchanged");
    }
}
