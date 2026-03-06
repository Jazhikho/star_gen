using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.App.Rendering;

/// <summary>
/// Internal helper methods for terrestrial shader parameter derivation.
/// </summary>
internal static class TerrestrialShaderParamProfiles
{
    /// <summary>
    /// Builds terrain parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildTerrainParams(CelestialBody body)
    {
        Dictionary parameters = new()
        {
            ["u_terrainScale"] = 4.0f,
            ["u_terrainHeight"] = 0.5f,
            ["u_roughness"] = 0.55f,
            ["u_continentSize"] = 1.5f,
            ["u_landCoherence"] = 0.6f,
            ["u_coastalDetail"] = 0.5f,
            ["u_octaves"] = 6,
        };

        if (!body.HasSurface() || body.Surface == null)
        {
            return parameters;
        }

        SurfaceProps surface = body.Surface;
        string surfaceType = surface.SurfaceType?.ToLowerInvariant() ?? string.Empty;

        if (surface.HasTerrain() && surface.Terrain != null)
        {
            TerrainProps terrain = surface.Terrain;
            parameters["u_roughness"] = Mathf.Clamp((float)terrain.Roughness, 0.3f, 0.8f);

            float elevationNormalized = Mathf.Clamp((float)(terrain.ElevationRangeM / 25000.0), 0.1f, 1.0f);
            parameters["u_terrainHeight"] = elevationNormalized;

            float tectonicFactor = (float)terrain.TectonicActivity;
            parameters["u_continentSize"] = 0.8f + (tectonicFactor * 2.7f);

            float fragmentation = Mathf.Max((float)terrain.ErosionLevel, (float)terrain.CraterDensity);
            parameters["u_landCoherence"] = Mathf.Clamp(0.85f - (fragmentation * 0.65f), 0.2f, 0.9f);
            parameters["u_coastalDetail"] = Mathf.Clamp(0.3f + ((float)terrain.ErosionLevel * 0.5f), 0.2f, 0.9f);

            if (terrain.TectonicActivity > 0.5)
            {
                parameters["u_terrainScale"] = 5.0f + ((float)terrain.TectonicActivity * 3.0f);
                parameters["u_octaves"] = 7;
            }
            else if (terrain.TectonicActivity < 0.2)
            {
                parameters["u_terrainScale"] = 3.0f;
                parameters["u_octaves"] = 5;
            }
        }

        if (!surface.HasTerrain())
        {
            ApplySurfaceTypeTerrainFallback(parameters, surfaceType, (float)surface.TemperatureK);
        }

        float tempK = (float)surface.TemperatureK;
        if (tempK < 250.0f)
        {
            parameters["u_landCoherence"] = Mathf.Min((float)parameters["u_landCoherence"] + 0.1f, 0.9f);
        }
        else if (tempK > 320.0f)
        {
            parameters["u_landCoherence"] = Mathf.Max((float)parameters["u_landCoherence"] - 0.1f, 0.2f);
        }

        return parameters;
    }

    /// <summary>
    /// Builds surface color parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildSurfaceColorParams(CelestialBody body)
    {
        Dictionary colors = GetSurfaceColors(body);
        Dictionary parameters = new()
        {
            ["u_colLow"] = (Color)colors["low"],
            ["u_colMid"] = (Color)colors["mid"],
            ["u_colHigh"] = (Color)colors["high"],
            ["u_colPeak"] = (Color)colors["peak"],
        };

        EnsureMinimumBrightness(parameters, "u_colLow");
        EnsureMinimumBrightness(parameters, "u_colMid");
        EnsureMinimumBrightness(parameters, "u_colHigh");
        EnsureMinimumBrightness(parameters, "u_colPeak");

        if (body.HasSurface() && body.Surface != null)
        {
            SurfaceProps surface = body.Surface;
            Dictionary composition = surface.SurfaceComposition;
            if (composition.ContainsKey("iron_oxides"))
            {
                float iron = GetFloat(composition, "iron_oxides", 0.0f);
                Color rustTint = new(0.7f, 0.4f, 0.3f);
                parameters["u_colLow"] = ((Color)parameters["u_colLow"]).Lerp(rustTint, iron * 0.4f);
                parameters["u_colMid"] = ((Color)parameters["u_colMid"]).Lerp(rustTint, iron * 0.3f);
            }

            float tempK = (float)surface.TemperatureK;
            if (tempK > 320.0f)
            {
                parameters["u_colLow"] = ((Color)parameters["u_colLow"]).Lerp(new Color(0.6f, 0.5f, 0.35f), 0.3f);
                parameters["u_colMid"] = ((Color)parameters["u_colMid"]).Lerp(new Color(0.7f, 0.6f, 0.4f), 0.3f);
            }
            else if (tempK < 260.0f)
            {
                parameters["u_colLow"] = ((Color)parameters["u_colLow"]).Lerp(new Color(0.4f, 0.45f, 0.4f), 0.3f);
                parameters["u_colMid"] = ((Color)parameters["u_colMid"]).Lerp(new Color(0.5f, 0.55f, 0.5f), 0.3f);
            }
        }

        return parameters;
    }

    /// <summary>
    /// Builds ocean parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildOceanParams(CelestialBody body)
    {
        Dictionary parameters = new()
        {
            ["u_seaLevel"] = 0.4f,
            ["u_seaSpecular"] = 0.8f,
            ["u_fresnelStrength"] = 1.0f,
            ["u_colSeaShallow"] = new Color(0.133f, 0.533f, 0.733f),
            ["u_colSeaDeep"] = new Color(0.039f, 0.133f, 0.267f),
        };

        if (body.HasSurface() && body.Surface != null && body.Surface.HasHydrosphere() && body.Surface.Hydrosphere != null)
        {
            HydrosphereProps hydrosphere = body.Surface.Hydrosphere;
            parameters["u_seaLevel"] = Mathf.Clamp(0.3f + ((float)hydrosphere.OceanCoverage * 0.4f), 0.0f, 0.8f);

            Dictionary oceanColors = GetOceanColors(hydrosphere);
            parameters["u_colSeaShallow"] = oceanColors["shallow"];
            parameters["u_colSeaDeep"] = oceanColors["deep"];

            string waterType = hydrosphere.WaterType.ToLowerInvariant();
            if (waterType == "methane")
            {
                parameters["u_seaSpecular"] = 0.5f;
            }
            else if (waterType == "ammonia")
            {
                parameters["u_seaSpecular"] = 0.6f;
            }
            else if (waterType == "hydrocarbon")
            {
                parameters["u_seaSpecular"] = 0.4f;
            }

            if (hydrosphere.IceCoverage > 0.5)
            {
                parameters["u_seaSpecular"] = (float)parameters["u_seaSpecular"] * (1.0f - ((float)hydrosphere.IceCoverage * 0.5f));
            }

            // Keep desert/arid worlds mostly dry even when a minimal hydrosphere payload exists.
            string surfaceType = body.Surface.SurfaceType.ToLowerInvariant();
            if (surfaceType is "desert" or "arid")
            {
                parameters["u_seaLevel"] = Mathf.Min((float)parameters["u_seaLevel"], Mathf.Clamp((float)hydrosphere.OceanCoverage * 2.0f, 0.0f, 0.2f));
            }

            return parameters;
        }

        if (body.HasSurface() && body.Surface != null)
        {
            string surfaceType = body.Surface.SurfaceType.ToLowerInvariant();
            string[] dryTypes =
            {
                "desert", "rocky", "rocky_cold", "volcanic", "molten",
                "tundra", "arid", "barren", "cratered",
                "frozen", "icy", "icy_rocky", "icy_cratered",
            };

            foreach (string dryType in dryTypes)
            {
                if (surfaceType == dryType)
                {
                    parameters["u_seaLevel"] = 0.05f;
                    break;
                }
            }
        }

        return parameters;
    }

    /// <summary>
    /// Builds ice parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildIceParams(CelestialBody body)
    {
        Dictionary parameters = new()
        {
            ["u_iceCap"] = 0.3f,
            ["u_colIce"] = new Color(0.867f, 0.933f, 1.0f),
        };

        if (body.HasSurface() && body.Surface != null && body.Surface.HasCryosphere() && body.Surface.Cryosphere != null)
        {
            CryosphereProps cryosphere = body.Surface.Cryosphere;
            parameters["u_iceCap"] = Mathf.Clamp((float)cryosphere.PolarCapCoverage, 0.0f, 1.0f);
            parameters["u_colIce"] = cryosphere.IceType.ToLowerInvariant() switch
            {
                "co2_ice" or "dry_ice" => new Color(0.95f, 0.95f, 0.95f),
                "nitrogen_ice" => new Color(0.85f, 0.88f, 0.95f),
                "methane_ice" => new Color(0.9f, 0.85f, 0.8f),
                _ => new Color(0.867f, 0.933f, 1.0f),
            };

            return parameters;
        }

        if (body.HasSurface() && body.Surface != null)
        {
            float tempK = (float)body.Surface.TemperatureK;
            if (tempK < 200.0f)
            {
                parameters["u_iceCap"] = Mathf.Clamp((250.0f - tempK) / 100.0f, 0.3f, 0.9f);
            }
            else if (tempK > 300.0f)
            {
                parameters["u_iceCap"] = 0.0f;
            }
        }

        return parameters;
    }

    /// <summary>
    /// Builds atmosphere parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildAtmosphereParams(CelestialBody body)
    {
        Dictionary parameters = new()
        {
            ["u_atmoDensity"] = 0.6f,
            ["u_atmoFalloff"] = 2.5f,
            ["u_scatterStrength"] = 0.8f,
            ["u_atmoColor"] = new Color(0.267f, 0.533f, 0.8f),
        };

        if (!body.HasAtmosphere() || body.Atmosphere == null)
        {
            parameters["u_atmoDensity"] = 0.0f;
            parameters["u_scatterStrength"] = 0.0f;
            return parameters;
        }

        AtmosphereProps atmosphere = body.Atmosphere;
        float pressureRatio = (float)(atmosphere.SurfacePressurePa / 101325.0);
        parameters["u_atmoDensity"] = Mathf.Clamp(pressureRatio, 0.0f, 2.0f);

        if (pressureRatio < 0.1f)
        {
            parameters["u_atmoFalloff"] = 4.0f;
        }
        else if (pressureRatio > 2.0f)
        {
            parameters["u_atmoFalloff"] = 1.5f;
        }

        parameters["u_atmoColor"] = ColorUtils.AtmosphereToSkyColor(atmosphere.Composition);
        if (atmosphere.GreenhouseFactor > 1.2)
        {
            float greenhouseStrength = Mathf.Clamp(((float)atmosphere.GreenhouseFactor - 1.0f) / 2.0f, 0.0f, 1.0f);
            parameters["u_atmoColor"] = ((Color)parameters["u_atmoColor"]).Lerp(new Color(0.9f, 0.7f, 0.4f), greenhouseStrength * 0.3f);
        }

        parameters["u_scatterStrength"] = Mathf.Clamp(0.5f + (pressureRatio * 0.3f), 0.3f, 1.2f);
        return parameters;
    }

    /// <summary>
    /// Builds cloud parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildCloudParams(CelestialBody body)
    {
        Dictionary parameters = new()
        {
            ["u_cloudCoverage"] = 0.4f,
            ["u_cloudScale"] = 3.5f,
            ["u_cloudShadow"] = 0.3f,
            ["u_cloudColor"] = new Color(1.0f, 1.0f, 1.0f),
        };

        if (!body.HasAtmosphere() || body.Atmosphere == null)
        {
            parameters["u_cloudCoverage"] = 0.0f;
            return parameters;
        }

        AtmosphereProps atmosphere = body.Atmosphere;
        Dictionary composition = atmosphere.Composition;
        bool hasWater = composition.ContainsKey("H2O");
        if (!hasWater
            && body.HasSurface()
            && body.Surface != null
            && body.Surface.HasHydrosphere()
            && body.Surface.Hydrosphere != null
            && body.Surface.Hydrosphere.OceanCoverage > 0.0)
        {
            hasWater = true;
        }

        float pressureRatio = (float)(atmosphere.SurfacePressurePa / 101325.0);

        if (hasWater)
        {
            float waterFraction = GetFloat(composition, "H2O", 0.0f);
            if (waterFraction <= 0.0f
                && body.HasSurface()
                && body.Surface != null
                && body.Surface.HasHydrosphere()
                && body.Surface.Hydrosphere != null)
            {
                waterFraction = Mathf.Clamp((float)body.Surface.Hydrosphere.OceanCoverage, 0.0f, 1.0f);
            }

            parameters["u_cloudCoverage"] = Mathf.Clamp((waterFraction * 10.0f) + (pressureRatio * 0.2f), 0.0f, 0.9f);
        }
        else if (composition.ContainsKey("SO2") || composition.ContainsKey("H2SO4"))
        {
            parameters["u_cloudCoverage"] = Mathf.Clamp(pressureRatio * 0.5f, 0.2f, 0.95f);
            parameters["u_cloudColor"] = new Color(0.9f, 0.85f, 0.7f);
        }
        else if (composition.ContainsKey("CO2") && pressureRatio > 0.5f)
        {
            parameters["u_cloudCoverage"] = Mathf.Clamp(pressureRatio * 0.1f, 0.05f, 0.2f);
            parameters["u_cloudColor"] = new Color(0.95f, 0.9f, 0.85f);
        }
        else
        {
            parameters["u_cloudCoverage"] = 0.1f;
        }

        if (pressureRatio > 2.0f)
        {
            parameters["u_cloudShadow"] = 0.5f;
        }

        if (atmosphere.GreenhouseFactor > 2.0)
        {
            parameters["u_cloudCoverage"] = Mathf.Max((float)parameters["u_cloudCoverage"], 0.7f);
            parameters["u_cloudColor"] = new Color(0.95f, 0.9f, 0.8f);
        }

        return parameters;
    }

    /// <summary>
    /// Builds lighting parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildLightingParams(CelestialBody body)
    {
        Dictionary parameters = new()
        {
            ["u_limbDark"] = 0.8f,
            ["u_terminatorSharp"] = 0.12f,
            ["u_ambient"] = 0.04f,
        };

        if (body.HasAtmosphere() && body.Atmosphere != null)
        {
            float pressureRatio = (float)(body.Atmosphere.SurfacePressurePa / 101325.0);
            if (pressureRatio > 1.5f)
            {
                parameters["u_terminatorSharp"] = 0.2f;
                parameters["u_ambient"] = 0.06f;
            }
            else if (pressureRatio < 0.1f)
            {
                parameters["u_terminatorSharp"] = 0.08f;
                parameters["u_ambient"] = 0.03f;
            }
        }

        return parameters;
    }

    /// <summary>
    /// Builds animation parameters for the spatial terrestrial shader.
    /// </summary>
    internal static Dictionary BuildAnimationParams(CelestialBody body)
    {
        float axialTilt = (float)body.Physical.AxialTiltDeg;
        float rotationPeriod = (float)body.Physical.RotationPeriodS;
        float rotationSpeed = ShaderParamHelpers.CalculateVisualRotationSpeed(rotationPeriod);
        return new Dictionary
        {
            ["u_rotSpeed"] = rotationSpeed,
            ["u_cloudDrift"] = rotationSpeed * 0.5f,
            ["u_axialTilt"] = Mathf.Clamp(axialTilt, 0.0f, 90.0f),
        };
    }

    /// <summary>
    /// Returns the base surface color profile for a terrestrial body.
    /// </summary>
    internal static Dictionary GetSurfaceColors(CelestialBody body)
    {
        Dictionary colors = new()
        {
            ["low"] = new Color(0.165f, 0.29f, 0.102f),
            ["mid"] = new Color(0.353f, 0.541f, 0.227f),
            ["high"] = new Color(0.541f, 0.478f, 0.353f),
            ["peak"] = new Color(0.8f, 0.733f, 0.667f),
        };

        if (!body.HasSurface() || body.Surface == null)
        {
            return colors;
        }

        SurfaceProps surface = body.Surface;
        string surfaceType = surface.SurfaceType?.ToLowerInvariant() ?? string.Empty;
        switch (surfaceType)
        {
            case "rocky":
            case "rocky_cold":
            case "cratered":
                colors["low"] = new Color(0.35f, 0.32f, 0.28f);
                colors["mid"] = new Color(0.45f, 0.42f, 0.38f);
                colors["high"] = new Color(0.55f, 0.52f, 0.48f);
                colors["peak"] = new Color(0.7f, 0.68f, 0.65f);
                break;
            case "desert":
                colors["low"] = new Color(0.6f, 0.45f, 0.3f);
                colors["mid"] = new Color(0.7f, 0.55f, 0.35f);
                colors["high"] = new Color(0.75f, 0.6f, 0.4f);
                colors["peak"] = new Color(0.85f, 0.75f, 0.6f);
                break;
            case "volcanic":
            case "molten":
                colors["low"] = new Color(0.15f, 0.12f, 0.1f);
                colors["mid"] = new Color(0.25f, 0.2f, 0.15f);
                colors["high"] = new Color(0.4f, 0.25f, 0.15f);
                colors["peak"] = new Color(0.6f, 0.35f, 0.2f);
                break;
            case "icy":
            case "frozen":
            case "icy_rocky":
                colors["low"] = new Color(0.7f, 0.75f, 0.8f);
                colors["mid"] = new Color(0.8f, 0.85f, 0.9f);
                colors["high"] = new Color(0.85f, 0.88f, 0.92f);
                colors["peak"] = new Color(0.95f, 0.97f, 1.0f);
                break;
            case "oceanic":
            case "continental":
            case "temperate":
            case "earthlike":
            case "habitable":
                colors["low"] = new Color(0.2f, 0.35f, 0.15f);
                colors["mid"] = new Color(0.25f, 0.45f, 0.2f);
                colors["high"] = new Color(0.4f, 0.45f, 0.35f);
                colors["peak"] = new Color(0.6f, 0.58f, 0.55f);
                break;
            case "tundra":
            case "cold":
            case "frozen_continental":
            case "subarctic":
            case "arctic":
                colors["low"] = new Color(0.40f, 0.45f, 0.38f);
                colors["mid"] = new Color(0.50f, 0.55f, 0.48f);
                colors["high"] = new Color(0.62f, 0.66f, 0.62f);
                colors["peak"] = new Color(0.82f, 0.86f, 0.90f);
                break;
            case "arid":
                colors["low"] = new Color(0.55f, 0.42f, 0.28f);
                colors["mid"] = new Color(0.65f, 0.50f, 0.32f);
                colors["high"] = new Color(0.72f, 0.58f, 0.38f);
                colors["peak"] = new Color(0.82f, 0.72f, 0.55f);
                break;
            case "barren":
                colors["low"] = new Color(0.32f, 0.30f, 0.28f);
                colors["mid"] = new Color(0.42f, 0.40f, 0.37f);
                colors["high"] = new Color(0.52f, 0.50f, 0.47f);
                colors["peak"] = new Color(0.65f, 0.63f, 0.60f);
                break;
            default:
                ApplyTemperatureDerivedColors(colors, (float)surface.TemperatureK);
                break;
        }

        float tempK = (float)surface.TemperatureK;
        if (tempK > 400.0f)
        {
            float heatFactor = Mathf.Clamp((tempK - 400.0f) / 300.0f, 0.0f, 1.0f);
            colors["low"] = ((Color)colors["low"]).Lerp(new Color(0.5f, 0.35f, 0.2f), heatFactor);
            colors["mid"] = ((Color)colors["mid"]).Lerp(new Color(0.6f, 0.45f, 0.25f), heatFactor);
        }
        else if (tempK < 250.0f)
        {
            float coldFactor = Mathf.Clamp((250.0f - tempK) / 100.0f, 0.0f, 1.0f);
            colors["low"] = ((Color)colors["low"]).Lerp(new Color(0.6f, 0.65f, 0.7f), coldFactor);
            colors["mid"] = ((Color)colors["mid"]).Lerp(new Color(0.7f, 0.75f, 0.8f), coldFactor);
        }

        return colors;
    }

    /// <summary>
    /// Returns ocean colors for a hydrosphere profile.
    /// </summary>
    internal static Dictionary GetOceanColors(HydrosphereProps hydrosphere)
    {
        Dictionary colors = hydrosphere.WaterType.ToLowerInvariant() switch
        {
            "ammonia" => new Dictionary
            {
                ["shallow"] = new Color(0.5f, 0.55f, 0.6f),
                ["deep"] = new Color(0.2f, 0.25f, 0.35f),
            },
            "methane" => new Dictionary
            {
                ["shallow"] = new Color(0.4f, 0.35f, 0.2f),
                ["deep"] = new Color(0.2f, 0.15f, 0.1f),
            },
            "hydrocarbon" => new Dictionary
            {
                ["shallow"] = new Color(0.4f, 0.35f, 0.25f),
                ["deep"] = new Color(0.15f, 0.12f, 0.08f),
            },
            _ => new Dictionary
            {
                ["shallow"] = new Color(0.133f, 0.533f, 0.733f),
                ["deep"] = new Color(0.039f, 0.133f, 0.267f),
            },
        };

        if (hydrosphere.SalinityPpt > 50.0)
        {
            float saltFactor = Mathf.Clamp((float)((hydrosphere.SalinityPpt - 50.0) / 100.0), 0.0f, 0.3f);
            Color shallow = (Color)colors["shallow"];
            colors["shallow"] = new Color(
                shallow.R + (saltFactor * 0.1f),
                shallow.G - (saltFactor * 0.05f),
                shallow.B - (saltFactor * 0.1f));
        }

        return colors;
    }

    /// <summary>
    /// Applies fallback terrain-style shaping from surface-type hints.
    /// </summary>
    private static void ApplySurfaceTypeTerrainFallback(Dictionary parameters, string surfaceType, float tempK)
    {
        switch (surfaceType)
        {
            case "oceanic":
                parameters["u_continentSize"] = 3.0f;
                parameters["u_landCoherence"] = 0.3f;
                break;
            case "continental":
            case "temperate":
            case "earthlike":
            case "habitable":
                parameters["u_continentSize"] = 1.5f;
                parameters["u_landCoherence"] = 0.6f;
                break;
            case "desert":
            case "arid":
                parameters["u_continentSize"] = 1.2f;
                parameters["u_landCoherence"] = 0.75f;
                break;
            case "tundra":
            case "cold":
            case "frozen_continental":
            case "subarctic":
            case "arctic":
                parameters["u_continentSize"] = 1.8f;
                parameters["u_landCoherence"] = 0.55f;
                break;
            case "barren":
            case "rocky":
            case "rocky_cold":
            case "cratered":
                parameters["u_continentSize"] = 2.0f;
                parameters["u_landCoherence"] = 0.5f;
                break;
            case "icy":
            case "frozen":
            case "icy_rocky":
            case "icy_cratered":
            case "ice":
            case "glacial":
                parameters["u_continentSize"] = 2.5f;
                parameters["u_landCoherence"] = 0.4f;
                break;
            default:
                if (tempK < 200.0f)
                {
                    parameters["u_continentSize"] = 2.5f;
                    parameters["u_landCoherence"] = 0.4f;
                }
                else if (tempK < 260.0f)
                {
                    parameters["u_continentSize"] = 1.8f;
                    parameters["u_landCoherence"] = 0.55f;
                }
                else if (tempK < 320.0f)
                {
                    parameters["u_continentSize"] = 1.5f;
                    parameters["u_landCoherence"] = 0.6f;
                }
                else
                {
                    parameters["u_continentSize"] = 1.2f;
                    parameters["u_landCoherence"] = 0.7f;
                }
                break;
        }
    }

    /// <summary>
    /// Applies temperature-derived fallback terrain colors for unclassified surfaces.
    /// </summary>
    private static void ApplyTemperatureDerivedColors(Dictionary colors, float tempK)
    {
        float warmth = Mathf.Clamp((tempK - 150.0f) / 500.0f, 0.0f, 1.0f);
        colors["low"] = new Color(0.35f, 0.38f, 0.42f).Lerp(new Color(0.50f, 0.38f, 0.25f), warmth);
        colors["mid"] = new Color(0.45f, 0.48f, 0.52f).Lerp(new Color(0.60f, 0.48f, 0.32f), warmth);
        colors["high"] = new Color(0.55f, 0.58f, 0.60f).Lerp(new Color(0.68f, 0.56f, 0.40f), warmth);
        colors["peak"] = new Color(0.70f, 0.72f, 0.75f).Lerp(new Color(0.80f, 0.70f, 0.55f), warmth);
    }

    /// <summary>
    /// Ensures a color parameter is not too dark to read visually.
    /// </summary>
    private static void EnsureMinimumBrightness(Dictionary parameters, string key)
    {
        const float minBrightness = 0.15f;
        Color color = (Color)parameters[key];
        float brightness = Mathf.Max(color.R, Mathf.Max(color.G, color.B));
        if (brightness < minBrightness)
        {
            float boost = minBrightness / Mathf.Max(brightness, 0.01f);
            parameters[key] = new Color(
                (color.R * boost) + 0.05f,
                (color.G * boost) + 0.05f,
                (color.B * boost) + 0.05f);
        }
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

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (float)(double)value,
            Variant.Type.Int => (int)value,
            _ => fallback,
        };
    }
}
