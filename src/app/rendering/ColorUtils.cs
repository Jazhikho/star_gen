using Godot;
using Godot.Collections;

namespace StarGen.App.Rendering;

/// <summary>
/// Utility functions for shader-facing color calculations.
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// Calculates blackbody color from temperature using a Planck-law approximation.
    /// </summary>
    public static Color TemperatureToBlackbodyColor(float temperatureK)
    {
        float temp = Mathf.Clamp(temperatureK, 1000.0f, 40000.0f) / 100.0f;
        float red;
        float green;
        float blue;

        if (temp <= 66.0f)
        {
            red = 255.0f;
        }
        else
        {
            red = temp - 60.0f;
            red = 329.698727446f * Mathf.Pow(red, -0.1332047592f);
            red = Mathf.Clamp(red, 0.0f, 255.0f);
        }

        if (temp <= 66.0f)
        {
            green = 99.4708025861f * Mathf.Log(temp) - 161.1195681661f;
            green = Mathf.Clamp(green, 0.0f, 255.0f);
        }
        else
        {
            green = temp - 60.0f;
            green = 288.1221695283f * Mathf.Pow(green, -0.0755148492f);
            green = Mathf.Clamp(green, 0.0f, 255.0f);
        }

        if (temp >= 66.0f)
        {
            blue = 255.0f;
        }
        else if (temp <= 19.0f)
        {
            blue = 0.0f;
        }
        else
        {
            blue = temp - 10.0f;
            blue = 138.5177312231f * Mathf.Log(blue) - 305.0447927307f;
            blue = Mathf.Clamp(blue, 0.0f, 255.0f);
        }

        return new Color(red / 255.0f, green / 255.0f, blue / 255.0f);
    }

    /// <summary>
    /// Returns a representative color for a stellar spectral class.
    /// </summary>
    public static Color SpectralClassToColor(string spectralType)
    {
        if (string.IsNullOrEmpty(spectralType))
        {
            return Colors.White;
        }

        string firstChar = spectralType[..1].ToUpperInvariant();
        return firstChar switch
        {
            "O" => new Color(0.6f, 0.7f, 1.0f),
            "B" => new Color(0.7f, 0.8f, 1.0f),
            "A" => new Color(0.9f, 0.9f, 1.0f),
            "F" => new Color(1.0f, 1.0f, 0.9f),
            "G" => new Color(1.0f, 1.0f, 0.8f),
            "K" => new Color(1.0f, 0.85f, 0.6f),
            "M" => new Color(1.0f, 0.6f, 0.4f),
            _ => Colors.White,
        };
    }

    /// <summary>
    /// Calculates a sky color from atmospheric composition.
    /// </summary>
    public static Color AtmosphereToSkyColor(Dictionary composition)
    {
        if (composition.Count == 0)
        {
            return new Color(0.5f, 0.6f, 0.8f);
        }

        Color color = new(0.0f, 0.0f, 0.0f);
        float totalWeight = 0.0f;
        Dictionary gasColors = new()
        {
            ["N2"] = new Color(0.4f, 0.5f, 0.9f),
            ["O2"] = new Color(0.5f, 0.6f, 0.9f),
            ["CO2"] = new Color(0.9f, 0.7f, 0.5f),
            ["CH4"] = new Color(0.4f, 0.6f, 0.8f),
            ["H2"] = new Color(0.7f, 0.7f, 0.8f),
            ["He"] = new Color(0.8f, 0.8f, 0.8f),
            ["NH3"] = new Color(0.8f, 0.7f, 0.6f),
            ["H2O"] = new Color(0.6f, 0.7f, 0.9f),
            ["SO2"] = new Color(0.9f, 0.8f, 0.5f),
            ["Ar"] = new Color(0.5f, 0.5f, 0.6f),
        };

        foreach (Variant gasVariant in composition.Keys)
        {
            string gas = (string)gasVariant;
            float fraction = GetFloat(composition, gas, 0.0f);
            if (fraction < 0.01f)
            {
                continue;
            }

            Color gasColor = gasColors.ContainsKey(gas) ? (Color)gasColors[gas] : new Color(0.5f, 0.5f, 0.5f);
            color += gasColor * fraction;
            totalWeight += fraction;
        }

        return totalWeight > 0.0f ? color / totalWeight : new Color(0.5f, 0.6f, 0.8f);
    }

    /// <summary>
    /// Calculates a base surface color from surface type and composition.
    /// </summary>
    public static Color SurfaceToColor(string surfaceType, Dictionary composition, float albedo)
    {
        string normalized = surfaceType.ToLowerInvariant();
        Color baseColor = normalized switch
        {
            "molten" => new Color(1.0f, 0.3f, 0.1f),
            "volcanic" => new Color(0.3f, 0.25f, 0.2f),
            "frozen" or "icy" or "icy_smooth" => new Color(0.85f, 0.9f, 0.95f),
            "icy_cratered" or "icy_rocky" => new Color(0.7f, 0.75f, 0.8f),
            "rocky" or "rocky_cold" => new Color(0.5f, 0.45f, 0.4f),
            "oceanic" => new Color(0.2f, 0.4f, 0.6f),
            "continental" => new Color(0.4f, 0.5f, 0.3f),
            "desert" => new Color(0.8f, 0.7f, 0.5f),
            "tundra" => new Color(0.55f, 0.58f, 0.50f),
            "arid" => new Color(0.65f, 0.50f, 0.32f),
            "barren" => new Color(0.42f, 0.40f, 0.37f),
            "cratered" => new Color(0.40f, 0.40f, 0.40f),
            _ => new Color(0.5f, 0.5f, 0.5f),
        };

        baseColor = baseColor.Lerp(Colors.White, albedo * 0.3f);
        return composition.Count == 0 ? baseColor : ModifyColorByComposition(baseColor, composition);
    }

    /// <summary>
    /// Calculates an asteroid color from type and composition.
    /// </summary>
    public static Color AsteroidToColor(string surfaceType, Dictionary composition)
    {
        string normalized = surfaceType.ToLowerInvariant();
        Color baseColor = normalized switch
        {
            "carbonaceous" => new Color(0.15f, 0.12f, 0.1f),
            "silicaceous" => new Color(0.5f, 0.45f, 0.4f),
            "metallic" => new Color(0.6f, 0.6f, 0.55f),
            _ => new Color(0.4f, 0.4f, 0.4f),
        };

        if (composition.Count > 0)
        {
            if (composition.ContainsKey("iron"))
            {
                baseColor = baseColor.Lerp(new Color(0.55f, 0.5f, 0.45f), GetFloat(composition, "iron", 0.0f) * 0.3f);
            }

            if (composition.ContainsKey("nickel"))
            {
                baseColor = baseColor.Lerp(new Color(0.6f, 0.58f, 0.5f), GetFloat(composition, "nickel", 0.0f) * 0.2f);
            }
        }

        return baseColor;
    }

    /// <summary>
    /// Calculates a ring color with opacity based on optical depth.
    /// </summary>
    public static Color RingToColor(Dictionary composition, float opticalDepth)
    {
        Color baseColor = new(0.8f, 0.8f, 0.8f);

        if (composition.ContainsKey("water_ice"))
        {
            baseColor = baseColor.Lerp(new Color(0.95f, 0.97f, 1.0f), GetFloat(composition, "water_ice", 0.0f));
        }

        if (composition.ContainsKey("silicates"))
        {
            baseColor = baseColor.Lerp(new Color(0.6f, 0.55f, 0.5f), GetFloat(composition, "silicates", 0.0f));
        }

        if (composition.ContainsKey("iron_oxides"))
        {
            baseColor = baseColor.Lerp(new Color(0.7f, 0.5f, 0.4f), GetFloat(composition, "iron_oxides", 0.0f) * 0.5f);
        }

        baseColor.A = Mathf.Clamp(opticalDepth, 0.1f, 0.95f);
        return baseColor;
    }

    /// <summary>
    /// Returns a display string for greenhouse intensity.
    /// </summary>
    public static string GetGreenhouseDescription(float greenhouseFactor)
    {
        if (greenhouseFactor < 1.05f)
        {
            return "None";
        }

        if (greenhouseFactor < 1.2f)
        {
            return "Mild";
        }

        if (greenhouseFactor < 1.5f)
        {
            return "Moderate";
        }

        if (greenhouseFactor < 2.0f)
        {
            return "Strong";
        }

        return "Extreme";
    }

    /// <summary>
    /// Returns a color hint for greenhouse intensity.
    /// </summary>
    public static Color GetGreenhouseColor(float greenhouseFactor)
    {
        float intensity = Mathf.Clamp((greenhouseFactor - 1.0f) / 2.0f, 0.0f, 1.0f);
        return new Color(1.0f, 1.0f - intensity * 0.5f, 1.0f - intensity * 0.8f);
    }

    /// <summary>
    /// Applies composition-driven tinting to a base color.
    /// </summary>
    private static Color ModifyColorByComposition(Color baseColor, Dictionary composition)
    {
        Color color = baseColor;

        if (composition.ContainsKey("iron_oxides"))
        {
            color = color.Lerp(new Color(0.7f, 0.3f, 0.2f), GetFloat(composition, "iron_oxides", 0.0f) * 0.5f);
        }

        if (composition.ContainsKey("water_ice"))
        {
            color = color.Lerp(new Color(0.9f, 0.95f, 1.0f), GetFloat(composition, "water_ice", 0.0f) * 0.4f);
        }

        if (composition.ContainsKey("silicates"))
        {
            color = color.Lerp(new Color(0.5f, 0.5f, 0.5f), GetFloat(composition, "silicates", 0.0f) * 0.2f);
        }

        if (composition.ContainsKey("carbon_compounds"))
        {
            color = color.Lerp(new Color(0.2f, 0.2f, 0.2f), GetFloat(composition, "carbon_compounds", 0.0f) * 0.3f);
        }

        return color;
    }

    /// <summary>
    /// Reads a float payload from a dictionary.
    /// </summary>
    private static float GetFloat(Dictionary data, string key, float fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        return GetNumeric(data[key], fallback);
    }

    /// <summary>
    /// Converts a numeric variant to float.
    /// </summary>
    private static float GetNumeric(Variant value, float fallback)
    {
        return value.VariantType switch
        {
            Variant.Type.Float => (float)(double)value,
            Variant.Type.Int => (int)value,
            _ => fallback,
        };
    }
}
