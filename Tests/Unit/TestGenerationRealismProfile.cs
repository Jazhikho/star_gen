#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Generation;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for GenerationRealismProfile: slider mapping, mode enum, determinism of profile choice.
/// </summary>
public static class TestGenerationRealismProfile
{
    /// <summary>
    /// Tests from slider zero is stylized.
    /// </summary>
    public static void TestFromSliderZeroIsStylized()
    {
        GenerationRealismProfile profile = GenerationRealismProfile.FromSlider(0.0);
        if (profile.Mode != GenerationRealismProfile.RealismMode.Stylized)
        {
            throw new InvalidOperationException("Slider 0 should be STYLIZED");
        }
        if (System.Math.Abs(profile.RealismSlider - 0.0) > 0.001)
        {
            throw new InvalidOperationException("Slider value should be 0");
        }
    }

    /// <summary>
    /// Tests from slider one is calibrated.
    /// </summary>
    public static void TestFromSliderOneIsCalibrated()
    {
        GenerationRealismProfile profile = GenerationRealismProfile.FromSlider(1.0);
        if (profile.Mode != GenerationRealismProfile.RealismMode.Calibrated)
        {
            throw new InvalidOperationException("Slider 1 should be CALIBRATED");
        }
        if (System.Math.Abs(profile.RealismSlider - 1.0) > 0.001)
        {
            throw new InvalidOperationException("Slider value should be 1");
        }
    }

    /// <summary>
    /// Tests from slider mid is balanced.
    /// </summary>
    public static void TestFromSliderMidIsBalanced()
    {
        GenerationRealismProfile profile = GenerationRealismProfile.FromSlider(0.5);
        if (profile.Mode != GenerationRealismProfile.RealismMode.Balanced)
        {
            throw new InvalidOperationException("Slider 0.5 should be BALANCED");
        }
    }

    /// <summary>
    /// Tests from slider clamped.
    /// </summary>
    public static void TestFromSliderClamped()
    {
        GenerationRealismProfile low = GenerationRealismProfile.FromSlider(-0.1);
        GenerationRealismProfile high = GenerationRealismProfile.FromSlider(1.5);
        if (System.Math.Abs(low.RealismSlider - 0.0) > 0.001)
        {
            throw new InvalidOperationException("Negative slider should clamp to 0");
        }
        if (System.Math.Abs(high.RealismSlider - 1.0) > 0.001)
        {
            throw new InvalidOperationException("Slider > 1 should clamp to 1");
        }
    }

    /// <summary>
    /// Tests static factories.
    /// </summary>
    public static void TestStaticFactories()
    {
        GenerationRealismProfile cal = GenerationRealismProfile.Calibrated();
        GenerationRealismProfile bal = GenerationRealismProfile.Balanced();
        GenerationRealismProfile sty = GenerationRealismProfile.Stylized();
        if (cal.Mode != GenerationRealismProfile.RealismMode.Calibrated)
        {
            throw new InvalidOperationException("calibrated() should return CALIBRATED");
        }
        if (bal.Mode != GenerationRealismProfile.RealismMode.Balanced)
        {
            throw new InvalidOperationException("balanced() should return BALANCED");
        }
        if (sty.Mode != GenerationRealismProfile.RealismMode.Stylized)
        {
            throw new InvalidOperationException("stylized() should return STYLIZED");
        }
    }

    /// <summary>
    /// Tests same slider same mode.
    /// </summary>
    public static void TestSameSliderSameMode()
    {
        GenerationRealismProfile a = GenerationRealismProfile.FromSlider(0.2);
        GenerationRealismProfile b = GenerationRealismProfile.FromSlider(0.2);
        if (a.Mode != b.Mode)
        {
            throw new InvalidOperationException("Same slider should yield same mode (determinism)");
        }
        if (System.Math.Abs(a.RealismSlider - b.RealismSlider) > 0.001)
        {
            throw new InvalidOperationException("Same slider should yield same value");
        }
    }
}
