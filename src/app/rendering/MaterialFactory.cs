using System.Collections.Generic;
using Godot;
using GDict = Godot.Collections.Dictionary;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.App.Rendering;

/// <summary>
/// Creates render materials for celestial bodies using the existing shader-param helpers.
/// </summary>
public static class MaterialFactory
{
    private static readonly Shader StarSurfaceShader = GD.Load<Shader>("res://src/app/rendering/shaders/star_surface.gdshader");
    private static readonly Shader StarAtmosphereShader = GD.Load<Shader>("res://src/app/rendering/shaders/star_atmosphere.gdshader");
    private static readonly Shader TerrestrialShader = GD.Load<Shader>("res://src/app/rendering/shaders/planet_terrestrial_surface.gdshader");
    private static readonly Shader GasGiantSurfaceShader = GD.Load<Shader>("res://src/app/rendering/shaders/planet_gas_giant_surface.gdshader");
    private static readonly Shader RingSystemShader = GD.Load<Shader>("res://src/app/rendering/shaders/ring_system.gdshader");
    private static readonly Shader AtmosphereRimShader = GD.Load<Shader>("res://src/app/rendering/shaders/atmosphere_rim.gdshader");

    private static readonly Dictionary<string, Material> MaterialCache = new();
    private static readonly Dictionary<ulong, string> CacheKeyHistory = new();

    /// <summary>
    /// Creates a material for a celestial body, reusing cached instances when possible.
    /// </summary>
    public static Material CreateBodyMaterial(CelestialBody? body)
    {
        if (body == null)
        {
            return CreateDefaultMaterial();
        }

        string cacheKey = GenerateCacheKey(body);
        if (MaterialCache.TryGetValue(cacheKey, out Material? cachedMaterial))
        {
            return cachedMaterial;
        }

        Material material = body.Type switch
        {
            CelestialType.Type.Star => CreateStarMaterial(body),
            CelestialType.Type.Planet => CreatePlanetMaterial(body),
            CelestialType.Type.Moon => CreateMoonMaterial(body),
            CelestialType.Type.Asteroid => CreateAsteroidMaterial(body),
            _ => CreateDefaultMaterial(),
        };

        MaterialCache[cacheKey] = material;
        CacheKeyHistory[body.GetInstanceId()] = cacheKey;
        return material;
    }

    /// <summary>
    /// Creates the secondary star atmosphere/corona material.
    /// </summary>
    public static ShaderMaterial? CreateStarAtmosphereMaterial(CelestialBody? body)
    {
        if (body == null || body.Type != CelestialType.Type.Star)
        {
            return null;
        }

        ShaderMaterial material = new()
        {
            Shader = StarAtmosphereShader,
        };

        GDict parameters = StarShaderParams.GetStarShaderParams(body);
        ApplyShaderParameters(material, parameters);
        material.SetShaderParameter("u_star_radius_ratio", 0.8f);
        return material;
    }

    /// <summary>
    /// Creates an atmosphere rim material when the body has a visible atmosphere.
    /// </summary>
    public static ShaderMaterial? CreateAtmosphereMaterial(CelestialBody body)
    {
        if (!AtmosphereShaderParams.ShouldRenderAtmosphere(body))
        {
            return null;
        }

        ShaderMaterial material = new()
        {
            Shader = AtmosphereRimShader,
        };

        ApplyShaderParameters(material, AtmosphereShaderParams.GetParams(body));
        return material;
    }

    /// <summary>
    /// Creates a full ring-system shader material.
    /// </summary>
    public static ShaderMaterial CreateRingSystemMaterial(RingSystemProps ringSystem, CelestialBody? body = null)
    {
        ShaderMaterial material = new()
        {
            Shader = RingSystemShader,
        };

        ApplyShaderParameters(material, RingShaderParams.GetParams(ringSystem, body));
        return material;
    }

    /// <summary>
    /// Creates a legacy single-band ring material.
    /// </summary>
    public static StandardMaterial3D CreateRingMaterial(RingBand band)
    {
        StandardMaterial3D material = new();
        Color ringColor = ColorUtils.RingToColor(band.Composition, (float)band.OpticalDepth);

        material.AlbedoColor = new Color(ringColor.R, ringColor.G, ringColor.B, ringColor.A);
        material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        material.Roughness = 0.9f;
        material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
        return material;
    }

    /// <summary>
    /// Clears the reusable material cache.
    /// </summary>
    public static void ClearCache()
    {
        MaterialCache.Clear();
        CacheKeyHistory.Clear();
    }

    private static string GenerateCacheKey(CelestialBody body)
    {
        List<string> keyParts = new()
        {
            CelestialType.TypeToString(body.Type),
        };

        switch (body.Type)
        {
            case CelestialType.Type.Star:
                if (body.HasStellar() && body.Stellar != null)
                {
                    int luminosityRounded = (int)(body.Stellar.LuminosityWatts / 1.0e24);
                    int ageBin = (int)(body.Stellar.AgeYears / 1.0e9);
                    int seedValue = body.Provenance == null ? 0 : (int)(body.Provenance.GenerationSeed % 1000L);
                    keyParts.Add($"temp_{(int)body.Stellar.EffectiveTemperatureK}_lum_{luminosityRounded}_age_{ageBin}_seed_{seedValue}");
                }
                else
                {
                    keyParts.Add("default");
                }
                break;
            case CelestialType.Type.Planet:
            case CelestialType.Type.Moon:
                if (body.HasSurface() && body.Surface != null)
                {
                    string surfaceType = string.IsNullOrEmpty(body.Surface.SurfaceType) ? "unknown" : body.Surface.SurfaceType;
                    float albedo = (float)body.Surface.Albedo;
                    int seedValue = body.Provenance == null ? 0 : (int)(body.Provenance.GenerationSeed % 1000L);
                    keyParts.Add($"{surfaceType}_albedo_{albedo:0.00}_seed_{seedValue}");
                    keyParts.Add($"temp_{(int)(body.Surface.TemperatureK / 50.0)}");
                }
                else
                {
                    keyParts.Add("default");
                }

                if (GasGiantShaderParams.IsGasGiant(body))
                {
                    double massEarth = body.Physical.MassKg / StarGen.Domain.Math.Units.EarthMassKg;
                    int tempK = (int)GetBodyTemperatureK(body);
                    int rotationBin = (int)(Mathf.Abs((float)body.Physical.RotationPeriodS) / 3600.0f);
                    string dominantGas = body.HasAtmosphere() && body.Atmosphere != null
                        ? body.Atmosphere.GetDominantGas()
                        : "none";
                    int seedValue = body.Provenance == null ? 0 : (int)(body.Provenance.GenerationSeed % 1000L);
                    keyParts.Add($"gas_m{(int)massEarth}_t{tempK}_r{rotationBin}_{dominantGas}_s{seedValue}");
                }
                else if (TerrestrialShaderParams.IsTerrestrialSuitable(body))
                {
                    keyParts.Add("terrestrial");
                }
                break;
            case CelestialType.Type.Asteroid:
                if (body.HasSurface() && body.Surface != null)
                {
                    keyParts.Add(body.Surface.SurfaceType);
                }
                else
                {
                    keyParts.Add("default");
                }
                break;
        }

        return string.Join("_", keyParts);
    }

    private static ShaderMaterial CreateStarMaterial(CelestialBody body)
    {
        ShaderMaterial material = new()
        {
            Shader = StarSurfaceShader,
        };

        ApplyShaderParameters(material, StarShaderParams.GetStarShaderParams(body));
        return material;
    }

    private static Material CreatePlanetMaterial(CelestialBody body)
    {
        if (GasGiantShaderParams.IsGasGiant(body))
        {
            return CreateGasGiantMaterial(body);
        }

        if (TerrestrialShaderParams.IsTerrestrialSuitable(body))
        {
            return CreateTerrestrialMaterial(body);
        }

        return CreateRockyMaterial(body);
    }

    private static Material CreateMoonMaterial(CelestialBody body)
    {
        if (TerrestrialShaderParams.IsTerrestrialSuitable(body))
        {
            return CreateTerrestrialMaterial(body);
        }

        if (body.HasSurface()
            && body.Surface != null
            && body.Surface.HasCryosphere()
            && body.Surface.Cryosphere != null
            && body.Surface.Cryosphere.PolarCapCoverage > 0.5)
        {
            return CreateIcyMaterial(body);
        }

        return CreateRockyMaterial(body);
    }

    private static ShaderMaterial CreateTerrestrialMaterial(CelestialBody body)
    {
        ShaderMaterial material = new()
        {
            Shader = TerrestrialShader,
        };

        ApplyShaderParameters(material, TerrestrialShaderParams.GetParams(body));
        return material;
    }

    private static StandardMaterial3D CreateAsteroidMaterial(CelestialBody body)
    {
        StandardMaterial3D material = new();
        if (body.HasSurface() && body.Surface != null)
        {
            Color asteroidColor = ColorUtils.AsteroidToColor(body.Surface.SurfaceType, body.Surface.SurfaceComposition);
            float albedo = (float)body.Surface.Albedo;

            material.AlbedoColor = asteroidColor * (albedo * 2.0f);
            material.Roughness = 0.95f;
            material.Metallic = 0.1f;

            if (body.Surface.SurfaceType == "metallic")
            {
                material.Metallic = 0.4f;
            }
        }
        else
        {
            material.AlbedoColor = new Color(0.3f, 0.3f, 0.3f);
            material.Roughness = 0.9f;
        }

        return material;
    }

    private static ShaderMaterial CreateGasGiantMaterial(CelestialBody body)
    {
        ShaderMaterial material = new()
        {
            Shader = GasGiantSurfaceShader,
        };

        ApplyShaderParameters(material, GasGiantShaderParams.GetParams(body));
        return material;
    }

    private static StandardMaterial3D CreateRockyMaterial(CelestialBody body)
    {
        StandardMaterial3D material = new();
        if (body.HasSurface() && body.Surface != null)
        {
            Color surfaceColor = ColorUtils.SurfaceToColor(
                body.Surface.SurfaceType,
                body.Surface.SurfaceComposition,
                (float)body.Surface.Albedo);
            material.AlbedoColor = surfaceColor;

            if (body.Surface.HasTerrain() && body.Surface.Terrain != null)
            {
                material.Roughness = Mathf.Clamp(0.5f + ((float)body.Surface.Terrain.Roughness * 0.4f), 0.3f, 0.95f);
            }
            else
            {
                material.Roughness = 0.7f;
            }

            material.Metallic = 0.0f;
            if (body.Surface.SurfaceComposition.ContainsKey("iron"))
            {
                float ironAmount = GetFloat(body.Surface.SurfaceComposition, "iron", 0.0f);
                material.Metallic = Mathf.Clamp(ironAmount * 0.5f, 0.0f, 0.3f);
            }

            if (body.Surface.TemperatureK > 700.0)
            {
                material.EmissionEnabled = true;
                material.Emission = ColorUtils.TemperatureToBlackbodyColor((float)body.Surface.TemperatureK) * 0.5f;
                material.EmissionEnergyMultiplier = (float)((body.Surface.TemperatureK - 700.0) / 1000.0);
            }
        }
        else
        {
            material.AlbedoColor = new Color(0.5f, 0.45f, 0.4f);
            material.Roughness = 0.8f;
        }

        return material;
    }

    private static StandardMaterial3D CreateIcyMaterial(CelestialBody body)
    {
        StandardMaterial3D material = new()
        {
            AlbedoColor = new Color(0.9f, 0.95f, 1.0f),
            Metallic = 0.1f,
        };

        if (body.HasSurface() && body.Surface != null)
        {
            material.AlbedoColor *= (float)body.Surface.Albedo * 1.5f;
            material.Roughness = 0.2f;
        }
        else
        {
            material.Roughness = 0.3f;
        }

        return material;
    }

    private static StandardMaterial3D CreateDefaultMaterial()
    {
        return new StandardMaterial3D
        {
            AlbedoColor = new Color(0.5f, 0.5f, 0.5f),
            Roughness = 0.5f,
        };
    }

    private static void ApplyShaderParameters(ShaderMaterial material, GDict parameters)
    {
        foreach (Variant key in parameters.Keys)
        {
            material.SetShaderParameter((string)key, parameters[key]);
        }
    }

    private static float GetBodyTemperatureK(CelestialBody body)
    {
        if (body.HasSurface() && body.Surface != null)
        {
            return (float)body.Surface.TemperatureK;
        }

        if (body.HasAtmosphere() && body.Atmosphere != null)
        {
            return 150.0f + (((float)body.Atmosphere.GreenhouseFactor - 1.0f) * 200.0f);
        }

        return 150.0f;
    }

    private static float GetFloat(GDict dictionary, string key, float fallback)
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
}
