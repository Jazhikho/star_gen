#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for SpiralDensityModel — density distribution correctness.
/// </summary>
public static class TestSpiralDensityModel
{
    private static GalaxySpec _spec;
    private static SpiralDensityModel _model;

    private static void BeforeEach()
    {
        _spec = GalaxySpec.CreateMilkyWay(1);
        _model = new SpiralDensityModel(_spec);
    }

    public static void TestCenterHasHighestDensity()
    {
        BeforeEach();
        float centerD = _model.GetDensity(Vector3.Zero);
        float edgeD = _model.GetDensity(new Vector3(10000.0f, 0.0f, 0.0f));
        DotNetNativeTestSuite.AssertGreaterThan(centerD, edgeD, "Center should be denser than edge");
    }

    public static void TestDensityIsNonNegative()
    {
        BeforeEach();
        Vector3[] positions =
        [
            Vector3.Zero,
            new Vector3(5000.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 500.0f, 0.0f),
            new Vector3(15000.0f, 1000.0f, 15000.0f),
            new Vector3(-3000.0f, -200.0f, 7000.0f),
        ];
        foreach (Vector3 pos in positions)
        {
            float d = _model.GetDensity(pos);
            DotNetNativeTestSuite.AssertTrue(d >= 0.0f, $"Density must be non-negative at {pos}");
        }
    }

    public static void TestDensityFallsWithHeight()
    {
        BeforeEach();
        float dPlane = _model.GetDensity(new Vector3(3000.0f, 0.0f, 0.0f));
        float dAbove = _model.GetDensity(new Vector3(3000.0f, 800.0f, 0.0f));
        DotNetNativeTestSuite.AssertGreaterThan(dPlane, dAbove, "Density should fall above the disk plane");
    }

    public static void TestDensityFallsWithRadius()
    {
        BeforeEach();
        float dInner = _model.GetDensity(new Vector3(2000.0f, 0.0f, 0.0f));
        float dOuter = _model.GetDensity(new Vector3(12000.0f, 0.0f, 0.0f));
        DotNetNativeTestSuite.AssertGreaterThan(dInner, dOuter, "Density should fall with galactic radius");
    }

    public static void TestArmFactorPeaksOnArm()
    {
        BeforeEach();
        float r = 5000.0f;
        float bestFactor = 0.0f;
        float worstFactor = 999.0f;

        for (int step = 0; step < 360; step += 1)
        {
            float theta = step * Mathf.Tau / 360.0f;
            float x = r * Mathf.Cos(theta);
            float z = r * Mathf.Sin(theta);
            float af = _model.GetArmFactor(r, x, z);
            bestFactor = Mathf.Max(bestFactor, af);
            worstFactor = Mathf.Min(worstFactor, af);
        }

        DotNetNativeTestSuite.AssertFloatNear(1.0, bestFactor, 0.01, "Arm peak factor should be ~1.0");

        float midpointDelta = (float)(Mathf.Pi / _spec.NumArms);
        float overlapProximity = (float)Mathf.Exp(
            -midpointDelta * midpointDelta / (2.0f * _spec.ArmWidth * _spec.ArmWidth));
        float expectedMin = (1.0f - (float)_spec.ArmAmplitude) + (float)_spec.ArmAmplitude * overlapProximity;
        DotNetNativeTestSuite.AssertFloatNear(
            (double)expectedMin, worstFactor, 0.05,
            "Inter-arm factor should match expected overlap minimum");
    }

    public static void TestArmFactorNearCenterIsOne()
    {
        BeforeEach();
        float af = _model.GetArmFactor(0.5f, 0.5f, 0.0f);
        DotNetNativeTestSuite.AssertFloatNear(1.0, af, 0.001, "Near center, arm factor should be 1.0");
    }

    public static void TestDeterminism()
    {
        BeforeEach();
        float d1 = _model.GetDensity(new Vector3(4000.0f, 100.0f, 3000.0f));
        float d2 = _model.GetDensity(new Vector3(4000.0f, 100.0f, 3000.0f));
        DotNetNativeTestSuite.AssertEqual(d1, d2, "Same input must give same density (pure function)");
    }

    public static void TestDifferentSpecGivesDifferentDensity()
    {
        BeforeEach();
        GalaxySpec spec2 = GalaxySpec.CreateMilkyWay(1);
        spec2.NumArms = 2;
        spec2.ArmPitchAngleDeg = 25.0;
        SpiralDensityModel model2 = new SpiralDensityModel(spec2);

        Vector3 pos = new Vector3(5000.0f, 0.0f, 3000.0f);
        float d1 = _model.GetDensity(pos);
        float d2 = model2.GetDensity(pos);
        DotNetNativeTestSuite.AssertNotEqual(d1, d2, "Different spec should give different density");
    }
}
