using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;

namespace StarGen.App.Rendering;

/// <summary>
/// Derives gas-giant shader uniforms from body components.
/// </summary>
public static class GasGiantShaderParams
{
    /// <summary>
    /// Returns shader parameters for gas-giant rendering.
    /// </summary>
    public static Dictionary GetParams(CelestialBody body)
    {
        Dictionary parameters = new();

        int seedInt = body.Provenance == null ? 0 : unchecked((int)body.Provenance.GenerationSeed);
        float seedValue = (seedInt % 1000) / 10.0f;
        parameters["u_seed"] = seedValue;

        RandomNumberGenerator rng = new()
        {
            Seed = unchecked((ulong)seedInt),
        };

        GasGiantArchetype archetype = ClassifyArchetype(body);
        GasGiantArchetypePreset preset = GasGiantShaderParamProfiles.GetPreset(archetype);

        Merge(parameters, GetShapeParams(body));
        Merge(parameters, GetBandParams(body, preset, rng));
        Merge(parameters, GetStormParams(body, preset, rng));
        Merge(parameters, GetDetailParams(preset, rng));
        Merge(parameters, GetColorParams(body, preset, rng));
        Merge(parameters, GetAtmosphereParams(preset, rng));
        Merge(parameters, GetLightingParams(preset));
        Merge(parameters, GetAnimationParams(body, preset));

        if (body.HasRingSystem() && body.RingSystem != null)
        {
            Dictionary ringParameters = RingShaderParams.GetRingShaderParams(body.RingSystem, (float)body.Physical.RadiusM);
            foreach (Variant key in ringParameters.Keys)
            {
                parameters[key] = ringParameters[key];
            }
        }
        else
        {
            parameters["u_ringType"] = 0;
        }

        return parameters;
    }

    /// <summary>
    /// Returns compatibility parameters for the legacy gas-giant shader path.
    /// </summary>
    public static Dictionary GetGasGiantShaderParams(CelestialBody body)
    {
        Dictionary parameters = GetParams(body);
        Dictionary legacy = new()
        {
            ["u_seed"] = GetFloat(parameters, "u_seed", 0.0f),
            ["u_gOblateness"] = GetFloat(parameters, "u_oblateness", 0.0f),
            ["u_axialTilt"] = GetFloat(parameters, "u_axialTilt", 0.0f),
            ["u_rotSpeed"] = GetFloat(parameters, "u_rotSpeed", 0.12f),
            ["u_gFlowSpeed"] = GetFloat(parameters, "u_flowSpeed", 0.3f),
            ["u_gColBandLight"] = GasGiantShaderParamProfiles.ColorToVector(GetColor(parameters, "u_colBandLight", Colors.White)),
            ["u_gColBandDark"] = GasGiantShaderParamProfiles.ColorToVector(GetColor(parameters, "u_colBandDark", Colors.Gray)),
            ["u_gColStorm"] = GasGiantShaderParamProfiles.ColorToVector(GetColor(parameters, "u_colStorm", Colors.Orange)),
            ["u_gColPolar"] = GasGiantShaderParamProfiles.ColorToVector(GetColor(parameters, "u_colPolar", Colors.CornflowerBlue)),
            ["u_atmoColor"] = GasGiantShaderParamProfiles.ColorToVector(GetColor(parameters, "u_atmoColor", Colors.White)),
            ["u_gBandCount"] = GetFloat(parameters, "u_bandCount", 14.0f),
            ["u_gBandContrast"] = GetFloat(parameters, "u_bandContrast", 0.5f),
            ["u_gBandTurb"] = GetFloat(parameters, "u_bandTurbulence", 0.6f),
            ["u_gFlowDetail"] = GetInt(parameters, "u_flowDetail", 5),
            ["u_gStormIntensity"] = GetFloat(parameters, "u_stormIntensity", 0.5f),
            ["u_gStormScale"] = GetFloat(parameters, "u_stormScale", 2.0f),
            ["u_gVortex"] = GetFloat(parameters, "u_vortexStrength", 0.7f),
            ["u_atmoDensity"] = GetFloat(parameters, "u_atmoDensity", 1.2f),
            ["u_atmoFalloff"] = GetFloat(parameters, "u_atmoFalloff", 2.0f),
            ["u_scatterStrength"] = GetFloat(parameters, "u_scatterStrength", 0.7f),
            ["u_limbDark"] = GetFloat(parameters, "u_limbDark", 0.9f),
            ["u_terminatorSharp"] = GetFloat(parameters, "u_terminatorSharp", 0.15f),
            ["u_lightX"] = GetFloat(parameters, "u_lightX", 0.7f),
            ["u_lightY"] = GetFloat(parameters, "u_lightY", 0.3f),
            ["u_ambient"] = GetFloat(parameters, "u_ambient", 0.04f),
        };

        if (body.HasRingSystem() && body.RingSystem != null)
        {
            Dictionary ringParameters = RingShaderParams.GetRingShaderParams(body.RingSystem, (float)body.Physical.RadiusM);
            foreach (Variant key in ringParameters.Keys)
            {
                legacy[key] = ringParameters[key];
            }
        }
        else
        {
            legacy["u_ringType"] = 0;
        }

        return legacy;
    }

    /// <summary>
    /// Returns whether the body should use gas-giant rendering.
    /// </summary>
    public static bool IsGasGiant(CelestialBody body)
    {
        if (body.HasSurface() && body.Surface != null && body.Surface.HasTerrain())
        {
            return false;
        }

        double massEarth = body.Physical.MassKg / StarGen.Domain.Math.Units.EarthMassKg;
        if (!body.HasSurface() && massEarth >= 10.0)
        {
            return true;
        }

        if (massEarth >= 15.0)
        {
            return true;
        }

        if (massEarth < 10.0)
        {
            return false;
        }

        if (body.HasSurface() && body.Surface != null)
        {
            string surfaceType = body.Surface.SurfaceType.ToLowerInvariant();
            return surfaceType is "gaseous" or "gas_giant" or "ice_giant";
        }

        return false;
    }

    private static void Merge(Dictionary destination, Dictionary source)
    {
        foreach (Variant key in source.Keys)
        {
            destination[key] = source[key];
        }
    }

    private static GasGiantArchetype ClassifyArchetype(CelestialBody body)
    {
        double massEarth = body.Physical.MassKg / StarGen.Domain.Math.Units.EarthMassKg;
        float temperatureK = GetTemperatureK(body);
        float rotationH = Mathf.Abs((float)body.Physical.RotationPeriodS) / 3600.0f;
        if (rotationH < 1.0f)
        {
            rotationH = 10.0f;
        }

        if (massEarth < 25.0)
        {
            return GasGiantArchetype.MiniNeptune;
        }

        if (massEarth > 500.0)
        {
            return GasGiantArchetype.SuperJupiter;
        }

        if (temperatureK > 700.0f)
        {
            return GasGiantArchetype.HotJupiter;
        }

        if (temperatureK < 150.0f)
        {
            return rotationH > 16.0f
                ? GasGiantArchetype.UranusClass
                : GasGiantArchetype.NeptuneClass;
        }

        float methane = GetCompositionFraction(body, "CH4");
        if (rotationH < 12.0f && massEarth < 200.0 && methane < 0.01f)
        {
            return body.Physical.Oblateness > 0.08
                ? GasGiantArchetype.SaturnClass
                : GasGiantArchetype.JupiterClass;
        }

        return GasGiantArchetype.JupiterClass;
    }

    private static Dictionary GetShapeParams(CelestialBody body)
    {
        return new Dictionary
        {
            ["u_oblateness"] = 0.0f,
            ["u_axialTilt"] = Mathf.DegToRad((float)body.Physical.AxialTiltDeg),
        };
    }

    private static Dictionary GetBandParams(CelestialBody body, GasGiantArchetypePreset preset, RandomNumberGenerator rng)
    {
        Dictionary parameters = new();
        float rotationPeriodS = Mathf.Abs((float)body.Physical.RotationPeriodS);
        if (rotationPeriodS < 1.0f)
        {
            rotationPeriodS = 36000.0f;
        }

        float rotationH = Mathf.Clamp(rotationPeriodS / 3600.0f, 8.0f, 30.0f);
        float rotationT = Mathf.Clamp(1.0f - ((rotationH - 8.0f) / 22.0f), 0.0f, 1.0f);
        float heatT = Mathf.Clamp((float)body.Physical.InternalHeatWatts / 2.0e17f, 0.0f, 1.0f);

        float bandNudge = (rotationT * 3.0f) + rng.RandfRange(-2.5f, 2.5f);
        parameters["u_bandCount"] = Mathf.Clamp(preset.BandCount + bandNudge, 4.0f, 28.0f);
        parameters["u_bandContrast"] = Mathf.Clamp(
            preset.BandContrast + (heatT * 0.12f) + rng.RandfRange(-0.08f, 0.08f),
            0.05f,
            0.95f);
        parameters["u_bandTurbulence"] = Mathf.Clamp(
            (preset.BandTurbulence * 0.7f) + (heatT * 0.10f) + (rotationT * 0.08f) + rng.RandfRange(-0.06f, 0.06f),
            0.05f,
            0.95f);
        parameters["u_bandSharpness"] = Mathf.Clamp(preset.BandSharpness + rng.RandfRange(-0.15f, 0.15f), 0.0f, 1.0f);
        parameters["u_bandWarp"] = Mathf.Clamp(preset.BandWarp + rng.RandfRange(-0.12f, 0.12f), 0.0f, 1.0f);
        parameters["u_chevronStrength"] = Mathf.Clamp(preset.Chevron + rng.RandfRange(-0.08f, 0.08f), 0.0f, 0.6f);
        parameters["u_flowDetail"] = Mathf.Clamp(preset.FlowDetail + rng.RandiRange(-1, 1), 2, 8);
        parameters["u_jetStrength"] = Mathf.Clamp(preset.JetStrength + rng.RandfRange(-0.2f, 0.2f), 0.3f, 2.0f);
        return parameters;
    }

    private static Dictionary GetStormParams(CelestialBody body, GasGiantArchetypePreset preset, RandomNumberGenerator rng)
    {
        Dictionary parameters = new();
        float heatT = Mathf.Clamp((float)body.Physical.InternalHeatWatts / 2.0e17f, 0.0f, 1.0f);
        parameters["u_stormIntensity"] = Mathf.Clamp(
            (preset.StormIntensity * 0.85f) + (heatT * 0.12f) + rng.RandfRange(-0.08f, 0.08f),
            0.0f,
            0.95f);
        parameters["u_stormScale"] = Mathf.Clamp(
            preset.StormScale + (heatT * 0.50f) + rng.RandfRange(-0.30f, 0.30f),
            0.80f,
            4.50f);
        parameters["u_vortexStrength"] = Mathf.Clamp(
            (preset.Vortex * 0.9f) + rng.RandfRange(-0.12f, 0.12f),
            0.05f,
            1.40f);
        parameters["u_stormCount"] = rng.RandiRange(preset.StormCountLo, preset.StormCountHi);
        parameters["u_darkSpotRatio"] = Mathf.Clamp(
            preset.DarkSpotRatio + rng.RandfRange(-0.15f, 0.15f),
            0.0f,
            1.0f);
        return parameters;
    }

    private static Dictionary GetDetailParams(GasGiantArchetypePreset preset, RandomNumberGenerator rng)
    {
        return new Dictionary
        {
            ["u_detailLevel"] = Mathf.Clamp(preset.DetailLevel + rng.RandfRange(-0.10f, 0.10f), 0.05f, 1.0f),
            ["u_hazeDensity"] = Mathf.Clamp(preset.HazeDensity + rng.RandfRange(-0.10f, 0.10f), 0.0f, 0.90f),
            ["u_streakIntensity"] = Mathf.Clamp(preset.Streak + rng.RandfRange(-0.08f, 0.08f), 0.0f, 0.60f),
        };
    }

    private static Dictionary GetColorParams(CelestialBody body, GasGiantArchetypePreset preset, RandomNumberGenerator rng)
    {
        Dictionary parameters = new();
        float hueShift = rng.RandfRange(-0.033f, 0.033f);
        const float jitter = 0.04f;

        parameters["u_colBandLight"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.ZoneEq, hueShift, rng, jitter);
        parameters["u_colZoneMid"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.ZoneMid, hueShift, rng, jitter);
        parameters["u_colBandDark"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.BeltEq, hueShift * 0.8f, rng, jitter);
        parameters["u_colBeltMid"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.BeltMid, hueShift * 0.8f, rng, jitter);
        parameters["u_colBeltPolar"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.BeltPolar, hueShift * 0.7f, rng, jitter);
        parameters["u_colStorm"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.Storm, hueShift * 0.5f, rng, jitter);
        parameters["u_colPolar"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.Polar, hueShift * 0.6f, rng, jitter);
        parameters["u_hazeColor"] = GasGiantShaderParamProfiles.FinalizeColor(preset.Palette.Haze, hueShift * 0.6f, rng, jitter * 0.5f);

        Color atmosphereColor = GasGiantShaderParamProfiles.ShiftHue(preset.Palette.Atmosphere, hueShift * 0.5f);
        if (body.HasAtmosphere() && body.Atmosphere != null)
        {
            Color skyColor = ColorUtils.AtmosphereToSkyColor(body.Atmosphere.Composition);
            atmosphereColor = atmosphereColor.Lerp(skyColor, 0.30f);
        }

        parameters["u_atmoColor"] = GasGiantShaderParamProfiles.ClampColor(atmosphereColor);
        return parameters;
    }

    private static Dictionary GetAtmosphereParams(GasGiantArchetypePreset preset, RandomNumberGenerator rng)
    {
        return new Dictionary
        {
            ["u_atmoDensity"] = Mathf.Clamp(preset.AtmoDensity + rng.RandfRange(-0.10f, 0.10f), 0.40f, 1.80f),
            ["u_atmoFalloff"] = Mathf.Clamp(preset.AtmoFalloff + rng.RandfRange(-0.15f, 0.15f), 1.20f, 4.50f),
            ["u_scatterStrength"] = Mathf.Clamp(preset.Scatter + rng.RandfRange(-0.08f, 0.08f), 0.30f, 1.40f),
        };
    }

    private static Dictionary GetLightingParams(GasGiantArchetypePreset preset)
    {
        return new Dictionary
        {
            ["u_limbDark"] = preset.LimbDark,
            ["u_terminatorSharp"] = preset.TerminatorSharp,
            ["u_lightX"] = 0.7f,
            ["u_lightY"] = 0.3f,
            ["u_ambient"] = preset.Ambient,
        };
    }

    private static Dictionary GetAnimationParams(CelestialBody body, GasGiantArchetypePreset preset)
    {
        float rotationPeriodS = Mathf.Abs((float)body.Physical.RotationPeriodS);
        if (rotationPeriodS < 1.0f)
        {
            rotationPeriodS = 36000.0f;
        }

        float physicsRotation = ShaderParamHelpers.CalculateVisualRotationSpeed(rotationPeriodS) * 2.0f;
        float presetRotation = preset.BandCount * 0.006f;
        float rotation = Mathf.Clamp(Mathf.Lerp(physicsRotation, presetRotation, 0.4f), 0.04f, 0.22f);

        float rotationH = Mathf.Clamp(rotationPeriodS / 3600.0f, 8.0f, 30.0f);
        float rotationT = Mathf.Clamp(1.0f - ((rotationH - 8.0f) / 22.0f), 0.0f, 1.0f);
        float flowSpeed = Mathf.Clamp(preset.FlowSpeed + (rotationT * 0.08f), 0.05f, 0.70f);

        return new Dictionary
        {
            ["u_rotSpeed"] = rotation,
            ["u_flowSpeed"] = flowSpeed,
        };
    }

    private static float GetTemperatureK(CelestialBody body)
    {
        if (body.HasSurface() && body.Surface != null)
        {
            return (float)body.Surface.TemperatureK;
        }

        if (body.HasAtmosphere() && body.Atmosphere != null)
        {
            return 150.0f + ((float)body.Atmosphere.GreenhouseFactor - 1.0f) * 200.0f;
        }

        return 150.0f;
    }

    private static float GetCompositionFraction(CelestialBody body, string gas)
    {
        if (!body.HasAtmosphere() || body.Atmosphere == null || !body.Atmosphere.Composition.ContainsKey(gas))
        {
            return 0.0f;
        }

        Variant value = body.Atmosphere.Composition[gas];
        return value.VariantType switch
        {
            Variant.Type.Float => (float)(double)value,
            Variant.Type.Int => (long)value,
            _ => 0.0f,
        };
    }

    private static float GetFloat(Dictionary dictionary, string key, float fallback)
    {
        if (!dictionary.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = dictionary[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (float)(double)value,
            Variant.Type.Int => (long)value,
            _ => fallback,
        };
    }

    private static int GetInt(Dictionary dictionary, string key, int fallback)
    {
        if (!dictionary.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = dictionary[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)(long)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    private static Color GetColor(Dictionary dictionary, string key, Color fallback)
    {
        if (!dictionary.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = dictionary[key];
        return value.VariantType == Variant.Type.Color ? (Color)value : fallback;
    }
}
