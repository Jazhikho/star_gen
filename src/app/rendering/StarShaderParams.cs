using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.App.Rendering;

/// <summary>
/// Derives star shader uniforms from celestial-body data.
/// </summary>
public static class StarShaderParams
{
    /// <summary>
    /// Returns shader parameters for star rendering.
    /// </summary>
    public static Dictionary GetStarShaderParams(CelestialBody body)
    {
        Dictionary parameters = new();

        float temperatureK = 5778.0f;
        float luminositySolar = 1.0f;
        float ageYears = 4.6e9f;
        float rotationPeriodS = 2.16e6f;

        if (body.HasStellar() && body.Stellar != null)
        {
            temperatureK = (float)body.Stellar.EffectiveTemperatureK;
            luminositySolar = (float)(body.Stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts);
            ageYears = (float)body.Stellar.AgeYears;
        }

        rotationPeriodS = Mathf.Abs((float)body.Physical.RotationPeriodS);
        if (rotationPeriodS < 1.0f)
        {
            rotationPeriodS = 2.16e6f;
        }

        float seedValue;
        if (body.Provenance == null)
        {
            seedValue = 0.0f;
        }
        else
        {
            seedValue = (body.Provenance.GenerationSeed % 1000L) / 10.0f;
        }

        parameters["u_temperature"] = temperatureK;
        parameters["u_star_color"] = ColorUtils.TemperatureToBlackbodyColor(temperatureK);
        parameters["u_luminosity"] = Mathf.Clamp(Mathf.Sqrt(luminositySolar), 0.3f, 3.0f);
        parameters["u_limbDark"] = CalculateLimbDarkening(temperatureK);

        Dictionary granulation = CalculateGranulationParams(temperatureK);
        parameters["u_granScale"] = granulation["scale"];
        parameters["u_granContrast"] = granulation["contrast"];
        parameters["u_granTurb"] = granulation["turbulence"];
        parameters["u_granFlow"] = granulation["flow"];
        parameters["u_superGranScale"] = granulation["super_scale"];
        parameters["u_superGranStr"] = granulation["super_strength"];

        Dictionary spots = CalculateSpotParams(temperatureK, ageYears, rotationPeriodS);
        parameters["u_spotCount"] = spots["count"];
        parameters["u_spotSize"] = spots["size"];
        parameters["u_penumbra"] = spots["penumbra"];
        parameters["u_spotDark"] = spots["darkness"];

        Dictionary chromosphere = CalculateChromosphereParams(temperatureK);
        parameters["u_chromoThick"] = chromosphere["thickness"];
        parameters["u_chromoIntensity"] = chromosphere["intensity"];
        parameters["u_chromoShift"] = chromosphere["shift"];

        Dictionary corona = CalculateCoronaParams(temperatureK, luminositySolar);
        parameters["u_coronaExtent"] = corona["extent"];
        parameters["u_coronaBright"] = corona["brightness"];
        parameters["u_coronaStreams"] = corona["streams"];
        parameters["u_coronaLength"] = corona["length"];
        parameters["u_coronaAsym"] = corona["asymmetry"];

        Dictionary prominence = CalculateProminenceParams(temperatureK, ageYears);
        parameters["u_promCount"] = prominence["count"];
        parameters["u_promHeight"] = prominence["height"];
        parameters["u_promGlow"] = prominence["glow"];
        parameters["u_flareIntensity"] = prominence["flare_intensity"];

        parameters["u_rotSpeed"] = ShaderParamHelpers.CalculateVisualRotationSpeed(rotationPeriodS);
        parameters["u_bloomRadius"] = 0.15f;
        parameters["u_bloomIntensity"] = Mathf.Clamp(0.4f + (luminositySolar * 0.2f), 0.4f, 1.5f);
        parameters["u_spikeCount"] = 4.0f;
        parameters["u_spikeLength"] = 0.3f;
        parameters["u_spikeBright"] = 0.25f;
        parameters["u_seed"] = seedValue;

        return parameters;
    }

    /// <summary>
    /// Calculates limb darkening from stellar temperature.
    /// </summary>
    private static float CalculateLimbDarkening(float temperatureK)
    {
        if (temperatureK > 30000.0f) return 0.2f;
        if (temperatureK > 10000.0f) return 0.3f;
        if (temperatureK > 7500.0f) return 0.45f;
        if (temperatureK > 6000.0f) return 0.55f;
        if (temperatureK > 5200.0f) return 0.6f;
        if (temperatureK > 3700.0f) return 0.68f;
        return 0.8f;
    }

    /// <summary>
    /// Calculates granulation shader parameters from stellar temperature.
    /// </summary>
    private static Dictionary CalculateGranulationParams(float temperatureK)
    {
        Dictionary parameters = new();
        if (temperatureK > 10000.0f)
        {
            parameters["scale"] = 10.0f;
            parameters["contrast"] = 0.05f;
            parameters["turbulence"] = 0.15f;
            parameters["flow"] = 0.02f;
            parameters["super_scale"] = 3.0f;
            parameters["super_strength"] = 0.05f;
        }
        else if (temperatureK > 7500.0f)
        {
            parameters["scale"] = 20.0f;
            parameters["contrast"] = 0.15f;
            parameters["turbulence"] = 0.25f;
            parameters["flow"] = 0.04f;
            parameters["super_scale"] = 4.0f;
            parameters["super_strength"] = 0.08f;
        }
        else if (temperatureK > 6000.0f)
        {
            parameters["scale"] = 26.0f;
            parameters["contrast"] = 0.28f;
            parameters["turbulence"] = 0.35f;
            parameters["flow"] = 0.06f;
            parameters["super_scale"] = 5.0f;
            parameters["super_strength"] = 0.12f;
        }
        else if (temperatureK > 5200.0f)
        {
            parameters["scale"] = 30.0f;
            parameters["contrast"] = 0.35f;
            parameters["turbulence"] = 0.4f;
            parameters["flow"] = 0.08f;
            parameters["super_scale"] = 6.0f;
            parameters["super_strength"] = 0.15f;
        }
        else if (temperatureK > 3700.0f)
        {
            parameters["scale"] = 28.0f;
            parameters["contrast"] = 0.42f;
            parameters["turbulence"] = 0.45f;
            parameters["flow"] = 0.1f;
            parameters["super_scale"] = 5.0f;
            parameters["super_strength"] = 0.12f;
        }
        else
        {
            parameters["scale"] = 20.0f;
            parameters["contrast"] = 0.55f;
            parameters["turbulence"] = 0.55f;
            parameters["flow"] = 0.12f;
            parameters["super_scale"] = 4.0f;
            parameters["super_strength"] = 0.1f;
        }

        return parameters;
    }

    /// <summary>
    /// Calculates sunspot shader parameters from temperature, age, and rotation.
    /// </summary>
    private static Dictionary CalculateSpotParams(float temperatureK, float ageYears, float rotationPeriodS)
    {
        Dictionary parameters = new();
        float ageFactor = Mathf.Clamp(1.0f - (ageYears / 10.0e9f), 0.2f, 1.0f);
        const float solarRotation = 2.16e6f;
        float rotationFactor = Mathf.Clamp(solarRotation / Mathf.Max(rotationPeriodS, 1.0f), 0.5f, 2.0f);
        float activity = ageFactor * rotationFactor;

        if (temperatureK > 8000.0f)
        {
            parameters["count"] = 0.0f;
            parameters["size"] = 0.0f;
            parameters["penumbra"] = 2.0f;
            parameters["darkness"] = 0.35f;
        }
        else if (temperatureK > 6000.0f)
        {
            parameters["count"] = Mathf.Floor(activity * 3.0f);
            parameters["size"] = 0.04f + (activity * 0.02f);
            parameters["penumbra"] = 2.0f;
            parameters["darkness"] = 0.4f;
        }
        else if (temperatureK > 5200.0f)
        {
            parameters["count"] = Mathf.Floor(activity * 5.0f);
            parameters["size"] = 0.05f + (activity * 0.02f);
            parameters["penumbra"] = 2.0f;
            parameters["darkness"] = 0.35f;
        }
        else if (temperatureK > 3700.0f)
        {
            parameters["count"] = Mathf.Floor(activity * 7.0f);
            parameters["size"] = 0.06f + (activity * 0.02f);
            parameters["penumbra"] = 1.8f;
            parameters["darkness"] = 0.35f;
        }
        else
        {
            parameters["count"] = Mathf.Floor(activity * 12.0f);
            parameters["size"] = 0.08f + (activity * 0.04f);
            parameters["penumbra"] = 1.6f;
            parameters["darkness"] = 0.3f;
        }

        return parameters;
    }

    /// <summary>
    /// Calculates chromosphere shader parameters from temperature.
    /// </summary>
    private static Dictionary CalculateChromosphereParams(float temperatureK)
    {
        Dictionary parameters = new();
        if (temperatureK > 10000.0f)
        {
            parameters["thickness"] = 0.008f;
            parameters["intensity"] = 0.3f;
            parameters["shift"] = 0.2f;
        }
        else if (temperatureK > 6000.0f)
        {
            parameters["thickness"] = 0.012f;
            parameters["intensity"] = 0.6f;
            parameters["shift"] = 0.4f;
        }
        else if (temperatureK > 5200.0f)
        {
            parameters["thickness"] = 0.015f;
            parameters["intensity"] = 0.8f;
            parameters["shift"] = 0.5f;
        }
        else if (temperatureK > 3700.0f)
        {
            parameters["thickness"] = 0.018f;
            parameters["intensity"] = 0.9f;
            parameters["shift"] = 0.55f;
        }
        else
        {
            parameters["thickness"] = 0.02f;
            parameters["intensity"] = 1.2f;
            parameters["shift"] = 0.6f;
        }

        return parameters;
    }

    /// <summary>
    /// Calculates corona shader parameters from temperature and luminosity.
    /// </summary>
    private static Dictionary CalculateCoronaParams(float temperatureK, float luminositySolar)
    {
        Dictionary parameters = new();
        float lumFactor = Mathf.Clamp(Mathf.Sqrt(luminositySolar), 0.5f, 2.0f);
        if (temperatureK > 10000.0f)
        {
            parameters["extent"] = 0.5f * lumFactor;
            parameters["brightness"] = 0.8f;
            parameters["streams"] = 12.0f;
            parameters["length"] = 0.6f;
            parameters["asymmetry"] = 0.2f;
        }
        else if (temperatureK > 6000.0f)
        {
            parameters["extent"] = 0.35f * lumFactor;
            parameters["brightness"] = 0.55f;
            parameters["streams"] = 10.0f;
            parameters["length"] = 0.5f;
            parameters["asymmetry"] = 0.25f;
        }
        else if (temperatureK > 5200.0f)
        {
            parameters["extent"] = 0.3f * lumFactor;
            parameters["brightness"] = 0.5f;
            parameters["streams"] = 8.0f;
            parameters["length"] = 0.5f;
            parameters["asymmetry"] = 0.3f;
        }
        else if (temperatureK > 3700.0f)
        {
            parameters["extent"] = 0.25f * lumFactor;
            parameters["brightness"] = 0.4f;
            parameters["streams"] = 6.0f;
            parameters["length"] = 0.4f;
            parameters["asymmetry"] = 0.35f;
        }
        else
        {
            parameters["extent"] = 0.2f * lumFactor;
            parameters["brightness"] = 0.3f;
            parameters["streams"] = 4.0f;
            parameters["length"] = 0.35f;
            parameters["asymmetry"] = 0.4f;
        }

        return parameters;
    }

    /// <summary>
    /// Calculates prominence and flare parameters from temperature and age.
    /// </summary>
    private static Dictionary CalculateProminenceParams(float temperatureK, float ageYears)
    {
        Dictionary parameters = new();
        float ageFactor = Mathf.Clamp(1.0f - (ageYears / 10.0e9f), 0.3f, 1.0f);
        if (temperatureK > 10000.0f)
        {
            parameters["count"] = 1.0f;
            parameters["height"] = 0.08f;
            parameters["glow"] = 0.5f;
            parameters["flare_intensity"] = 0.1f * ageFactor;
        }
        else if (temperatureK > 6000.0f)
        {
            parameters["count"] = 2.0f;
            parameters["height"] = 0.1f;
            parameters["glow"] = 0.7f;
            parameters["flare_intensity"] = 0.15f * ageFactor;
        }
        else if (temperatureK > 5200.0f)
        {
            parameters["count"] = 3.0f;
            parameters["height"] = 0.12f;
            parameters["glow"] = 0.8f;
            parameters["flare_intensity"] = 0.2f * ageFactor;
        }
        else if (temperatureK > 3700.0f)
        {
            parameters["count"] = 3.0f;
            parameters["height"] = 0.15f;
            parameters["glow"] = 0.9f;
            parameters["flare_intensity"] = 0.3f * ageFactor;
        }
        else
        {
            parameters["count"] = 2.0f;
            parameters["height"] = 0.1f;
            parameters["glow"] = 0.7f;
            parameters["flare_intensity"] = 0.5f * ageFactor;
        }

        return parameters;
    }
}
