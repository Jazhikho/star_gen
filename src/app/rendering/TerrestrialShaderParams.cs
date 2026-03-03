using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.App.Rendering;

/// <summary>
/// Derives terrestrial planet shader uniforms from body components.
/// </summary>
public static class TerrestrialShaderParams
{
    /// <summary>
    /// Returns shader parameters for the spatial terrestrial surface shader.
    /// </summary>
    public static Dictionary GetParams(CelestialBody body)
    {
        Dictionary parameters = new();
        float seedValue = body.Provenance == null ? 0.0f : (body.Provenance.GenerationSeed % 1000L) / 10.0f;
        parameters["u_seed"] = seedValue;

        Merge(parameters, TerrestrialShaderParamProfiles.BuildTerrainParams(body));
        Merge(parameters, TerrestrialShaderParamProfiles.BuildSurfaceColorParams(body));
        Merge(parameters, TerrestrialShaderParamProfiles.BuildOceanParams(body));
        Merge(parameters, TerrestrialShaderParamProfiles.BuildIceParams(body));
        Merge(parameters, TerrestrialShaderParamProfiles.BuildAtmosphereParams(body));
        Merge(parameters, TerrestrialShaderParamProfiles.BuildCloudParams(body));
        Merge(parameters, TerrestrialShaderParamProfiles.BuildLightingParams(body));
        Merge(parameters, TerrestrialShaderParamProfiles.BuildAnimationParams(body));

        parameters["u_cityLightIntensity"] = 0.0f;
        parameters["u_cityLightColor"] = new Color(1.0f, 0.85f, 0.5f);
        return parameters;
    }

    /// <summary>
    /// Checks whether a body should use the terrestrial shader path.
    /// </summary>
    public static bool IsTerrestrialSuitable(CelestialBody? body)
    {
        if (body == null || !body.HasSurface() || body.Surface == null)
        {
            return false;
        }

        string surfaceType = body.Surface.SurfaceType?.ToLowerInvariant() ?? string.Empty;
        return surfaceType != "gaseous" && surfaceType != "gas_giant" && surfaceType != "ice_giant";
    }

    /// <summary>
    /// Returns shader parameters for the legacy canvas-item terrestrial shader.
    /// </summary>
    public static Dictionary GetTerrestrialShaderParams(CelestialBody body)
    {
        Dictionary parameters = new();
        float seedValue = body.Provenance == null ? 0.0f : (body.Provenance.GenerationSeed % 1000L) / 10.0f;
        float rotationPeriod = Mathf.Abs((float)body.Physical.RotationPeriodS);
        if (rotationPeriod < 1.0f)
        {
            rotationPeriod = 86400.0f;
        }

        parameters["u_seed"] = seedValue;
        parameters["u_axialTilt"] = Mathf.DegToRad((float)body.Physical.AxialTiltDeg);
        parameters["u_rotSpeed"] = ShaderParamHelpers.CalculateVisualRotationSpeed(rotationPeriod);

        Dictionary terrain = TerrestrialShaderParamProfiles.BuildTerrainParams(body);
        parameters["u_terrainScale"] = terrain["u_terrainScale"];
        parameters["u_terrainHeight"] = terrain["u_terrainHeight"];
        parameters["u_roughness"] = terrain["u_roughness"];
        parameters["u_continentSize"] = terrain["u_continentSize"];
        parameters["u_octaves"] = terrain["u_octaves"];

        Dictionary ocean = TerrestrialShaderParamProfiles.BuildOceanParams(body);
        parameters["u_seaLevel"] = ocean["u_seaLevel"];
        parameters["u_seaSpecular"] = ocean["u_seaSpecular"];
        parameters["u_fresnelStrength"] = ocean["u_fresnelStrength"];
        parameters["u_colSeaShallow"] = ColorToVector((Color)ocean["u_colSeaShallow"]);
        parameters["u_colSeaDeep"] = ColorToVector((Color)ocean["u_colSeaDeep"]);

        Dictionary ice = TerrestrialShaderParamProfiles.BuildIceParams(body);
        parameters["u_iceCap"] = ice["u_iceCap"];
        parameters["u_colIce"] = ColorToVector((Color)ice["u_colIce"]);

        Dictionary colors = TerrestrialShaderParamProfiles.GetSurfaceColors(body);
        parameters["u_colLow"] = ColorToVector((Color)colors["low"]);
        parameters["u_colMid"] = ColorToVector((Color)colors["mid"]);
        parameters["u_colHigh"] = ColorToVector((Color)colors["high"]);
        parameters["u_colPeak"] = ColorToVector((Color)colors["peak"]);

        Dictionary atmosphere = TerrestrialShaderParamProfiles.BuildAtmosphereParams(body);
        parameters["u_atmoDensity"] = atmosphere["u_atmoDensity"];
        parameters["u_atmoFalloff"] = atmosphere["u_atmoFalloff"];
        parameters["u_scatterStrength"] = atmosphere["u_scatterStrength"];
        parameters["u_atmoColor"] = ColorToVector((Color)atmosphere["u_atmoColor"]);

        Dictionary clouds = TerrestrialShaderParamProfiles.BuildCloudParams(body);
        parameters["u_cloudCoverage"] = clouds["u_cloudCoverage"];
        parameters["u_cloudScale"] = clouds["u_cloudScale"];
        parameters["u_cloudShadow"] = clouds["u_cloudShadow"];
        parameters["u_cloudColor"] = ColorToVector((Color)clouds["u_cloudColor"]);
        parameters["u_cloudDrift"] = ((float)parameters["u_rotSpeed"]) * 0.5f;

        Dictionary lighting = TerrestrialShaderParamProfiles.BuildLightingParams(body);
        parameters["u_lightX"] = 0.7f;
        parameters["u_lightY"] = 0.4f;
        parameters["u_ambient"] = lighting["u_ambient"];
        parameters["u_limbDark"] = lighting["u_limbDark"];
        parameters["u_terminatorSharp"] = lighting["u_terminatorSharp"];

        if (body.HasRingSystem() && body.RingSystem != null)
        {
            Merge(parameters, RingShaderParams.GetRingShaderParams(body.RingSystem, (float)body.Physical.RadiusM));
        }
        else
        {
            parameters["u_ringType"] = 0;
        }

        return parameters;
    }

    /// <summary>
    /// Merges one dictionary into another.
    /// </summary>
    private static void Merge(Dictionary target, Dictionary source)
    {
        foreach (Variant key in source.Keys)
        {
            target[key] = source[key];
        }
    }

    /// <summary>
    /// Converts a color to the legacy vector-based payload shape.
    /// </summary>
    private static Vector3 ColorToVector(Color color)
    {
        return new Vector3(color.R, color.G, color.B);
    }
}
