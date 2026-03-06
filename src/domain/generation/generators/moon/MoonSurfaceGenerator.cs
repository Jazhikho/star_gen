using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators.Moon;

/// <summary>
/// Generates surface properties for moons.
/// </summary>
public static class MoonSurfaceGenerator
{
    private const double WaterFreezeK = 273.15;

    /// <summary>
    /// Generates surface properties for a moon.
    /// </summary>
    public static SurfaceProps GenerateSurface(
        MoonSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        double surfaceTempK,
        double tidalHeatWatts,
        ParentContext context,
        SeededRng rng)
    {
        _ = context;
        double albedo = CalculateAlbedo(surfaceTempK, rng);
        string surfaceType = DetermineSurfaceType(surfaceTempK, tidalHeatWatts, rng);
        double volcanismLevel = CalculateVolcanism(physical, tidalHeatWatts, rng);
        Dictionary surfaceComposition = GenerateSurfaceComposition(surfaceTempK, rng);

        SurfaceProps surface = new(
            surfaceTempK,
            albedo,
            surfaceType,
            volcanismLevel,
            surfaceComposition);

        surface.Terrain = GenerateTerrain(physical, sizeCategory, volcanismLevel, rng);
        if (surfaceTempK < WaterFreezeK)
        {
            surface.Cryosphere = GenerateCryosphere(
                spec,
                surfaceTempK,
                physical,
                tidalHeatWatts,
                rng);
        }

        return surface;
    }

    /// <summary>Computes surface albedo from temperature and RNG.</summary>
    private static double CalculateAlbedo(double surfaceTempK, SeededRng rng)
    {
        if (surfaceTempK < 150.0)
        {
            return rng.RandfRange(0.4f, 0.95f);
        }

        if (surfaceTempK < WaterFreezeK)
        {
            return rng.RandfRange(0.2f, 0.6f);
        }

        return rng.RandfRange(0.05f, 0.3f);
    }

    /// <summary>Picks surface type string from temperature and tidal heating.</summary>
    private static string DetermineSurfaceType(
        double surfaceTempK,
        double tidalHeatWatts,
        SeededRng rng)
    {
        if (tidalHeatWatts > 1.0e13)
        {
            return "volcanic";
        }

        if (surfaceTempK < 100.0)
        {
            double roll = rng.Randf();
            if (roll < 0.6)
            {
                return "icy";
            }

            if (roll < 0.8)
            {
                return "icy_cratered";
            }

            return "icy_smooth";
        }

        if (surfaceTempK < WaterFreezeK)
        {
            if (rng.Randf() < 0.5f)
            {
                return "icy_rocky";
            }

            return "rocky_cold";
        }

        return "rocky";
    }

    /// <summary>Computes volcanism from internal and tidal heat.</summary>
    private static double CalculateVolcanism(
        PhysicalProps physical,
        double tidalHeatWatts,
        SeededRng rng)
    {
        double totalHeat = physical.InternalHeatWatts + tidalHeatWatts;
        double baseVolcanism = System.Math.Clamp(totalHeat / 1.0e14, 0.0, 1.0);
        double variation = rng.RandfRange(0.7f, 1.3f);
        return System.Math.Clamp(baseVolcanism * variation, 0.0, 1.0);
    }

    /// <summary>Generates normalized surface composition from temperature.</summary>
    private static Dictionary GenerateSurfaceComposition(double surfaceTempK, SeededRng rng)
    {
        Dictionary composition = new();

        if (surfaceTempK < 100.0)
        {
            composition["water_ice"] = rng.RandfRange(0.5f, 0.8f);
            composition["silicates"] = rng.RandfRange(0.1f, 0.3f);
            if (rng.Randf() < 0.5f)
            {
                composition["nitrogen_ice"] = rng.RandfRange(0.01f, 0.1f);
            }

            if (rng.Randf() < 0.3f)
            {
                composition["methane_ice"] = rng.RandfRange(0.01f, 0.05f);
            }
        }
        else if (surfaceTempK < WaterFreezeK)
        {
            composition["water_ice"] = rng.RandfRange(0.3f, 0.6f);
            composition["silicates"] = rng.RandfRange(0.3f, 0.5f);
            composition["carbon_compounds"] = rng.RandfRange(0.05f, 0.15f);
        }
        else
        {
            composition["silicates"] = rng.RandfRange(0.5f, 0.7f);
            composition["iron_oxides"] = rng.RandfRange(0.1f, 0.3f);
            composition["sulfur_compounds"] = rng.RandfRange(0.05f, 0.15f);
        }

        double total = 0.0;
        foreach (Godot.Variant fraction in composition.Values)
        {
            total += (double)fraction;
        }

        if (total > 0.0)
        {
            foreach (Godot.Variant material in composition.Keys)
            {
                composition[material] = (double)composition[material] / total;
            }
        }

        return composition;
    }

    /// <summary>Builds terrain props from gravity, size, and volcanism.</summary>
    private static TerrainProps GenerateTerrain(
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        double volcanismLevel,
        SeededRng rng)
    {
        double gravity = physical.GetSurfaceGravityMS2();
        double gravityFactor = 9.81 / System.Math.Max(gravity, 0.01);
        double elevationRangeM = 10000.0 * gravityFactor * rng.RandfRange(0.2f, 1.0f);
        elevationRangeM = System.Math.Clamp(elevationRangeM, 500.0, 50000.0);

        double roughness = rng.RandfRange(0.3f, 0.9f);
        double craterDensity;
        if (volcanismLevel > 0.5)
        {
            craterDensity = rng.RandfRange(0.0f, 0.2f);
        }
        else if (sizeCategory == SizeCategory.Category.Dwarf)
        {
            craterDensity = rng.RandfRange(0.7f, 0.95f);
        }
        else
        {
            craterDensity = rng.RandfRange(0.4f, 0.8f);
        }

        double tectonicActivity = volcanismLevel * rng.RandfRange(0.5f, 1.0f);
        double erosionLevel = rng.RandfRange(0.0f, 0.1f);
        string terrainType;
        if (volcanismLevel > 0.5)
        {
            terrainType = "volcanic";
        }
        else if (craterDensity > 0.6)
        {
            terrainType = "cratered";
        }
        else
        {
            terrainType = "plains";
        }

        return new TerrainProps(
            elevationRangeM,
            roughness,
            craterDensity,
            tectonicActivity,
            erosionLevel,
            terrainType);
    }

    /// <summary>Generates cryosphere (polar caps, subsurface ocean) for moons.</summary>
    private static CryosphereProps GenerateCryosphere(
        MoonSpec spec,
        double surfaceTempK,
        PhysicalProps physical,
        double tidalHeatWatts,
        SeededRng rng)
    {
        double polarCapCoverage;
        if (surfaceTempK < 150.0)
        {
            polarCapCoverage = rng.RandfRange(0.8f, 1.0f);
        }
        else
        {
            polarCapCoverage = rng.RandfRange(0.3f, 0.8f);
        }
        double permafrostDepthM = rng.RandfRange(1000.0f, 50000.0f);

        bool hasSubsurfaceOcean;
        if (spec.HasOceanPreference())
        {
            hasSubsurfaceOcean = (bool)spec.HasSubsurfaceOcean;
        }
        else
        {
            double totalHeat = physical.InternalHeatWatts + tidalHeatWatts;
            if (totalHeat > 5.0e11)
            {
                hasSubsurfaceOcean = rng.Randf() < 0.7f;
            }
            else if (totalHeat > 1.0e11)
            {
                hasSubsurfaceOcean = rng.Randf() < 0.3f;
            }
            else
            {
                hasSubsurfaceOcean = false;
            }
        }

        double subsurfaceOceanDepthM;
        if (hasSubsurfaceOcean)
        {
            subsurfaceOceanDepthM = rng.RandfRange(5000.0f, 150000.0f);
        }
        else
        {
            subsurfaceOceanDepthM = 0.0;
        }

        double cryovolcanismLevel;
        if (hasSubsurfaceOcean && tidalHeatWatts > 1.0e11)
        {
            cryovolcanismLevel = rng.RandfRange(0.1f, 0.8f);
        }
        else
        {
            cryovolcanismLevel = 0.0;
        }

        string iceType = "water_ice";
        if (surfaceTempK < 50.0)
        {
            double roll = rng.Randf();
            if (roll < 0.2)
            {
                iceType = "nitrogen_ice";
            }
            else if (roll < 0.3)
            {
                iceType = "methane_ice";
            }
        }

        return new CryosphereProps(
            polarCapCoverage,
            permafrostDepthM,
            hasSubsurfaceOcean,
            subsurfaceOceanDepthM,
            cryovolcanismLevel,
            iceType);
    }
}
