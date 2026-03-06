#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.Rendering;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for ColorUtils.
/// </summary>
public static class TestColorUtils
{
    private const double DefaultTolerance = 0.01;

    /// <summary>
    /// Tests blackbody hot star is blue.
    /// </summary>
    public static void TestBlackbodyHotStarIsBlue()
    {
        Color color = ColorUtils.TemperatureToBlackbodyColor(30000.0);

        if (color.B <= color.R)
        {
            throw new InvalidOperationException("Hot star should have more blue than red");
        }
        if (color.B <= 0.8)
        {
            throw new InvalidOperationException("Hot star should have high blue component");
        }
    }

    /// <summary>
    /// Tests blackbody solar is yellow white.
    /// </summary>
    public static void TestBlackbodySolarIsYellowWhite()
    {
        Color color = ColorUtils.TemperatureToBlackbodyColor(5778.0);

        if (color.R <= 0.9)
        {
            throw new InvalidOperationException("Solar temperature should have high red");
        }
        if (color.G <= 0.9)
        {
            throw new InvalidOperationException("Solar temperature should have high green");
        }
        if (color.B <= 0.7)
        {
            throw new InvalidOperationException("Solar temperature should have moderate blue");
        }
    }

    /// <summary>
    /// Tests blackbody cool star is red.
    /// </summary>
    public static void TestBlackbodyCoolStarIsRed()
    {
        Color color = ColorUtils.TemperatureToBlackbodyColor(3000.0);

        if (color.R <= color.B)
        {
            throw new InvalidOperationException("Cool star should have more red than blue");
        }
        if (color.R <= 0.9)
        {
            throw new InvalidOperationException("Cool star should have high red component");
        }
    }

    /// <summary>
    /// Tests blackbody temperature gradient.
    /// </summary>
    public static void TestBlackbodyTemperatureGradient()
    {
        Color color3000 = ColorUtils.TemperatureToBlackbodyColor(3000.0);
        Color color6000 = ColorUtils.TemperatureToBlackbodyColor(6000.0);
        Color color10000 = ColorUtils.TemperatureToBlackbodyColor(10000.0);
        Color color20000 = ColorUtils.TemperatureToBlackbodyColor(20000.0);

        if (color6000.B <= color3000.B)
        {
            throw new InvalidOperationException("6000K should be bluer than 3000K");
        }
        if (color10000.B <= color6000.B)
        {
            throw new InvalidOperationException("10000K should be bluer than 6000K");
        }
        if (color20000.B < color10000.B)
        {
            throw new InvalidOperationException("20000K should be at least as blue as 10000K");
        }
    }

    /// <summary>
    /// Tests spectral class colors.
    /// </summary>
    public static void TestSpectralClassColors()
    {
        Color oColor = ColorUtils.SpectralClassToColor("O5V");
        Color gColor = ColorUtils.SpectralClassToColor("G2V");
        Color mColor = ColorUtils.SpectralClassToColor("M5V");

        if (oColor.B <= gColor.B)
        {
            throw new InvalidOperationException("O-class should be bluer than G-class");
        }

        if (mColor.R <= mColor.B)
        {
            throw new InvalidOperationException("M-class should be redder than blue");
        }
        if (mColor.R <= gColor.R * 0.9)
        {
            throw new InvalidOperationException("M-class should have high red");
        }
    }

    /// <summary>
    /// Tests atmosphere nitrogen is blue.
    /// </summary>
    public static void TestAtmosphereNitrogenIsBlue()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "N2", 0.78 }, { "O2", 0.21 }, { "Ar", 0.01 } };
        Color color = ColorUtils.AtmosphereToSkyColor(composition);

        if (color.B <= color.R)
        {
            throw new InvalidOperationException("N2/O2 atmosphere should be bluish");
        }
    }

    /// <summary>
    /// Tests atmosphere co2 is orange.
    /// </summary>
    public static void TestAtmosphereCo2IsOrange()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "CO2", 0.96 }, { "N2", 0.03 } };
        Color color = ColorUtils.AtmosphereToSkyColor(composition);

        if (color.R <= color.B)
        {
            throw new InvalidOperationException("CO2 atmosphere should be more orange/red");
        }
    }

    /// <summary>
    /// Tests atmosphere methane is cyan.
    /// </summary>
    public static void TestAtmosphereMethaneisCyan()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "N2", 0.95 }, { "CH4", 0.05 } };
        Color color = ColorUtils.AtmosphereToSkyColor(composition);

        if (color.B <= 0.5)
        {
            throw new InvalidOperationException("Methane-containing atmosphere should have blue");
        }
    }

    /// <summary>
    /// Tests surface molten is red orange.
    /// </summary>
    public static void TestSurfaceMoltenIsRedOrange()
    {
        Color color = ColorUtils.SurfaceToColor("molten", new Godot.Collections.Dictionary(), 0.1);

        if (color.R <= color.B)
        {
            throw new InvalidOperationException("Molten surface should be reddish");
        }
        if (color.R <= 0.7)
        {
            throw new InvalidOperationException("Molten surface should have high red");
        }
    }

    /// <summary>
    /// Tests surface icy is white blue.
    /// </summary>
    public static void TestSurfaceIcyIsWhiteBlue()
    {
        Color color = ColorUtils.SurfaceToColor("icy", new Dictionary { { "water_ice", 0.9 } }, 0.8);

        if (color.B <= 0.7)
        {
            throw new InvalidOperationException("Icy surface should have high blue");
        }
        if (color.R <= 0.7)
        {
            throw new InvalidOperationException("Icy surface should have high red (white)");
        }
    }

    /// <summary>
    /// Tests surface rocky is gray brown.
    /// </summary>
    public static void TestSurfaceRockyIsGrayBrown()
    {
        Color color = ColorUtils.SurfaceToColor("rocky", new Dictionary { { "silicates", 0.7 } }, 0.3);

        double maxDiff = System.Math.Max(System.Math.Abs(color.R - color.G), System.Math.Abs(color.G - color.B));
        if (maxDiff >= 0.3)
        {
            throw new InvalidOperationException("Rocky surface should be fairly neutral in color");
        }
    }

    /// <summary>
    /// Tests asteroid carbonaceous is dark.
    /// </summary>
    public static void TestAsteroidCarbonaceousIsDark()
    {
        Color color = ColorUtils.AsteroidToColor("carbonaceous", new Dictionary { { "carbon_compounds", 0.3 } });

        if (color.R >= 0.3)
        {
            throw new InvalidOperationException("Carbonaceous asteroid should be dark");
        }
        if (color.G >= 0.3)
        {
            throw new InvalidOperationException("Carbonaceous asteroid should be dark");
        }
        if (color.B >= 0.3)
        {
            throw new InvalidOperationException("Carbonaceous asteroid should be dark");
        }
    }

    /// <summary>
    /// Tests asteroid metallic is gray.
    /// </summary>
    public static void TestAsteroidMetallicIsGray()
    {
        Color color = ColorUtils.AsteroidToColor("metallic", new Dictionary { { "iron", 0.8 }, { "nickel", 0.15 } });

        if (color.R <= 0.4)
        {
            throw new InvalidOperationException("Metallic asteroid should be lighter than carbonaceous");
        }
    }

    /// <summary>
    /// Tests ring icy is bright.
    /// </summary>
    public static void TestRingIcyIsBright()
    {
        Color color = ColorUtils.RingToColor(new Dictionary { { "water_ice", 0.95 } }, 0.5);

        if (color.R <= 0.8)
        {
            throw new InvalidOperationException("Icy ring should be bright");
        }
        if (color.B <= 0.8)
        {
            throw new InvalidOperationException("Icy ring should be bright with blue tint");
        }
        if (System.Math.Abs(color.A - 0.5) > DefaultTolerance)
        {
            throw new InvalidOperationException("Alpha should match optical depth");
        }
    }

    /// <summary>
    /// Tests ring optical depth affects alpha.
    /// </summary>
    public static void TestRingOpticalDepthAffectsAlpha()
    {
        Color thinRing = ColorUtils.RingToColor(new Dictionary { { "water_ice", 0.9 } }, 0.1);
        Color thickRing = ColorUtils.RingToColor(new Dictionary { { "water_ice", 0.9 } }, 0.8);

        if (thickRing.A <= thinRing.A)
        {
            throw new InvalidOperationException("Thicker ring should have higher alpha");
        }
    }
}
