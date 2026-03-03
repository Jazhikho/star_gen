using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.App.Rendering;

/// <summary>
/// Derives atmosphere shader uniforms from atmospheric body data.
/// </summary>
public static class AtmosphereShaderParams
{
    /// <summary>
    /// Returns shader parameters for atmosphere rendering.
    /// </summary>
    public static Dictionary GetParams(CelestialBody body)
    {
        if (!body.HasAtmosphere() || body.Atmosphere == null)
        {
            return GetDefaultParams();
        }

        AtmosphereProps atmosphere = body.Atmosphere;
        Dictionary parameters = new();

        Color skyColor = ColorUtils.AtmosphereToSkyColor(atmosphere.Composition);
        parameters["u_atmosphereColor"] = new Color(skyColor.R, skyColor.G, skyColor.B, 1.0f);

        float pressureRatio = (float)(atmosphere.SurfacePressurePa / 101325.0);
        parameters["u_density"] = Mathf.Clamp(pressureRatio * 0.8f, 0.05f, 2.0f);
        parameters["u_falloff"] = CalculateFalloff(body, atmosphere);
        parameters["u_scatterStrength"] = CalculateScatterStrength(atmosphere);

        float greenhouseIntensity = Mathf.Clamp(((float)atmosphere.GreenhouseFactor - 1.0f) * 0.5f, 0.0f, 1.0f);
        parameters["u_greenhouseIntensity"] = greenhouseIntensity;
        parameters["u_greenhouseColor"] = GetGreenhouseColor(atmosphere);

        parameters["u_lightDir"] = new Vector3(0.7f, 0.4f, 0.6f);
        parameters["u_sunGlowStrength"] = CalculateSunGlow(atmosphere);
        parameters["u_terminatorSoftness"] = CalculateTerminatorSoftness(pressureRatio);

        return parameters;
    }

    /// <summary>
    /// Returns whether atmosphere rendering should be visible for a body.
    /// </summary>
    public static bool ShouldRenderAtmosphere(CelestialBody body)
    {
        return body.HasAtmosphere()
            && body.Atmosphere != null
            && body.Atmosphere.SurfacePressurePa >= 100.0;
    }

    /// <summary>
    /// Returns the default shader parameters used when no atmosphere is available.
    /// </summary>
    private static Dictionary GetDefaultParams()
    {
        return new Dictionary
        {
            ["u_atmosphereColor"] = new Color(0.4f, 0.6f, 0.9f, 1.0f),
            ["u_density"] = 0.6f,
            ["u_falloff"] = 3.0f,
            ["u_scatterStrength"] = 0.8f,
            ["u_greenhouseIntensity"] = 0.0f,
            ["u_greenhouseColor"] = new Color(1.0f, 0.6f, 0.3f),
            ["u_lightDir"] = new Vector3(0.7f, 0.4f, 0.6f),
            ["u_sunGlowStrength"] = 0.3f,
            ["u_terminatorSoftness"] = 0.15f,
        };
    }

    /// <summary>
    /// Calculates atmospheric falloff from scale height.
    /// </summary>
    private static float CalculateFalloff(CelestialBody body, AtmosphereProps atmosphere)
    {
        if (body.Physical == null || body.Physical.RadiusM <= 0.0 || atmosphere.ScaleHeightM <= 0.0)
        {
            return 3.0f;
        }

        float falloff = (float)(85000.0 / atmosphere.ScaleHeightM);
        return Mathf.Clamp(falloff, 1.0f, 10.0f);
    }

    /// <summary>
    /// Calculates scattering strength from composition and pressure.
    /// </summary>
    private static float CalculateScatterStrength(AtmosphereProps atmosphere)
    {
        Dictionary composition = atmosphere.Composition;
        float h2Fraction = GetFraction(composition, "H2") + GetFraction(composition, "He");
        float earthlikeFraction = GetFraction(composition, "N2") + GetFraction(composition, "O2");
        float co2Fraction = GetFraction(composition, "CO2");

        float scatter = 0.5f;
        if (earthlikeFraction > 0.5f)
        {
            scatter = 0.8f + (earthlikeFraction * 0.2f);
        }
        else if (h2Fraction > 0.5f)
        {
            scatter = 0.5f + (h2Fraction * 0.2f);
        }
        else if (co2Fraction > 0.5f)
        {
            scatter = 0.4f + (co2Fraction * 0.3f);
        }

        float pressureFactor = Mathf.Clamp((float)(atmosphere.SurfacePressurePa / 101325.0), 0.1f, 2.0f);
        scatter *= Mathf.Sqrt(pressureFactor);
        return Mathf.Clamp(scatter, 0.3f, 1.5f);
    }

    /// <summary>
    /// Calculates greenhouse glow color from composition.
    /// </summary>
    private static Color GetGreenhouseColor(AtmosphereProps atmosphere)
    {
        Dictionary composition = atmosphere.Composition;
        if (GetFraction(composition, "CO2") > 0.5f)
        {
            return new Color(1.0f, 0.5f, 0.25f);
        }

        if (GetFraction(composition, "CH4") > 0.01f)
        {
            return new Color(0.9f, 0.8f, 0.4f);
        }

        if (GetFraction(composition, "SO2") > 0.001f || GetFraction(composition, "H2S") > 0.001f)
        {
            return new Color(1.0f, 0.85f, 0.4f);
        }

        return new Color(1.0f, 0.6f, 0.3f);
    }

    /// <summary>
    /// Calculates sun-glow strength from atmospheric pressure.
    /// </summary>
    private static float CalculateSunGlow(AtmosphereProps atmosphere)
    {
        float pressureRatio = (float)(atmosphere.SurfacePressurePa / 101325.0);
        if (pressureRatio > 2.0f)
        {
            return 0.5f;
        }

        if (pressureRatio > 0.5f)
        {
            return 0.3f;
        }

        return 0.15f;
    }

    /// <summary>
    /// Calculates terminator softness from relative pressure.
    /// </summary>
    private static float CalculateTerminatorSoftness(float pressureRatio)
    {
        if (pressureRatio > 5.0f) return 0.3f;
        if (pressureRatio > 1.0f) return 0.2f;
        if (pressureRatio > 0.1f) return 0.15f;
        return 0.08f;
    }

    /// <summary>
    /// Reads a gas fraction from a composition dictionary.
    /// </summary>
    private static float GetFraction(Dictionary composition, string key)
    {
        if (!composition.ContainsKey(key))
        {
            return 0.0f;
        }

        Variant value = composition[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (float)(double)value,
            Variant.Type.Int => (int)value,
            _ => 0.0f,
        };
    }
}
