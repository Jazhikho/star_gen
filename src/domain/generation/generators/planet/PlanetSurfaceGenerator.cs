using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators.Planet;

/// <summary>
/// Generates surface properties for rocky planets.
/// </summary>
public static class PlanetSurfaceGenerator
{
    private const double WaterFreezeK = 273.15;
    private const double WaterBoilK = 373.15;

    /// <summary>
    /// Generates surface properties for a rocky planet.
    /// </summary>
    public static SurfaceProps GenerateSurface(
        PlanetSpec spec,
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        OrbitZone.Zone zone,
        double surfaceTempK,
        ParentContext context,
        SeededRng rng)
    {
        double albedo = CalculateAlbedo(spec, zone, surfaceTempK, rng);
        string surfaceType = DetermineSurfaceType(sizeCategory, zone, surfaceTempK, rng);
        double volcanismLevel = CalculateVolcanism(spec, physical, context.StellarAgeYears, rng);
        Dictionary surfaceComposition = GenerateSurfaceComposition(sizeCategory, zone, surfaceTempK, rng);

        SurfaceProps surface = new(
            surfaceTempK,
            albedo,
            surfaceType,
            volcanismLevel,
            surfaceComposition);

        surface.Terrain = GenerateTerrain(physical, sizeCategory, volcanismLevel, rng);

        if (HasHydrosphereOverride(spec) || CanHaveLiquidWater(surfaceTempK, physical))
        {
            surface.Hydrosphere = GenerateHydrosphere(spec, surfaceTempK, sizeCategory, volcanismLevel, rng);
        }

        if (surfaceTempK < WaterFreezeK || zone == OrbitZone.Zone.Cold)
        {
            surface.Cryosphere = GenerateCryosphere(surfaceTempK, physical, rng);
        }

        return surface;
    }

    /// <summary>Computes surface albedo from zone, temperature, and overrides.</summary>
    private static double CalculateAlbedo(
        PlanetSpec spec,
        OrbitZone.Zone zone,
        double surfaceTempK,
        SeededRng rng)
    {
        double overrideAlbedo = spec.GetOverrideFloat("surface.albedo", -1.0);
        if (overrideAlbedo >= 0.0)
        {
            return System.Math.Clamp(overrideAlbedo, 0.0, 1.0);
        }

        if (surfaceTempK < WaterFreezeK)
        {
            return rng.RandfRange(0.5f, 0.8f);
        }

        if (zone == OrbitZone.Zone.Hot)
        {
            return rng.RandfRange(0.05f, 0.2f);
        }

        return rng.RandfRange(0.1f, 0.5f);
    }

    /// <summary>Picks surface type string from size, zone, temperature and RNG.</summary>
    private static string DetermineSurfaceType(
        SizeCategory.Category sizeCategory,
        OrbitZone.Zone zone,
        double surfaceTempK,
        SeededRng rng)
    {
        if (surfaceTempK >= 1000.0)
        {
            // Basalt melting point ~1000–1200 K; below this, rocky surfaces can form.
            return "molten";
        }

        if (surfaceTempK > 500.0)
        {
            return "volcanic";
        }

        if (surfaceTempK < 100.0)
        {
            return "frozen";
        }

        if (surfaceTempK < WaterFreezeK)
        {
            if (rng.Randf() < 0.5f)
            {
                return "icy";
            }

            return "rocky_cold";
        }

        if (zone == OrbitZone.Zone.Temperate)
        {
            double roll = rng.Randf();
            if (roll < 0.2)
            {
                return "oceanic";
            }

            if (roll < 0.35)
            {
                return "continental";
            }

            if (roll < 0.50)
            {
                return "tundra";
            }

            if (roll < 0.65)
            {
                return "desert";
            }

            if (roll < 0.80)
            {
                return "arid";
            }

            return "rocky";
        }

        if (sizeCategory == SizeCategory.Category.Dwarf)
        {
            return "cratered";
        }

        double coldRoll = rng.Randf();
        if (coldRoll < 0.3)
        {
            return "barren";
        }

        if (coldRoll < 0.5)
        {
            return "rocky_cold";
        }

        return "rocky";
    }

    /// <summary>Computes volcanism level from internal heat and age.</summary>
    private static double CalculateVolcanism(
        PlanetSpec spec,
        PhysicalProps physical,
        double ageYears,
        SeededRng rng)
    {
        double overrideVolcanism = spec.GetOverrideFloat("surface.volcanism_level", -1.0);
        if (overrideVolcanism >= 0.0)
        {
            return System.Math.Clamp(overrideVolcanism, 0.0, 1.0);
        }

        const double earthHeat = 4.7e13;
        double heatRatio = physical.InternalHeatWatts / earthHeat;
        double baseVolcanism = System.Math.Clamp(heatRatio * 0.5, 0.0, 1.0);

        if (ageYears > 0.0)
        {
            baseVolcanism *= System.Math.Pow(0.5, ageYears / 5.0e9);
        }

        return System.Math.Clamp(baseVolcanism * rng.RandfRange(0.5f, 1.5f), 0.0, 1.0);
    }

    /// <summary>Generates normalized surface composition dict from zone and temperature.</summary>
    private static Dictionary GenerateSurfaceComposition(
        SizeCategory.Category sizeCategory,
        OrbitZone.Zone zone,
        double surfaceTempK,
        SeededRng rng)
    {
        Dictionary composition = new();

        if (surfaceTempK < WaterFreezeK)
        {
            composition["water_ice"] = rng.RandfRange(0.2f, 0.6f);
            composition["silicates"] = rng.RandfRange(0.3f, 0.6f);
            composition["carbon_compounds"] = rng.RandfRange(0.05f, 0.2f);
        }
        else if (zone == OrbitZone.Zone.Hot)
        {
            composition["silicates"] = rng.RandfRange(0.5f, 0.7f);
            composition["iron_oxides"] = rng.RandfRange(0.1f, 0.3f);
            composition["sulfur_compounds"] = rng.RandfRange(0.05f, 0.15f);
        }
        else
        {
            composition["silicates"] = rng.RandfRange(0.4f, 0.6f);
            composition["iron_oxides"] = rng.RandfRange(0.1f, 0.2f);
            composition["carbonates"] = rng.RandfRange(0.05f, 0.15f);
            if ((int)sizeCategory >= (int)SizeCategory.Category.Terrestrial)
            {
                composition["water"] = rng.RandfRange(0.01f, 0.1f);
            }
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

    /// <summary>Generates terrain properties from physical characteristics and volcanism.</summary>
    /// <param name="physical">Physical properties of the body.</param>
    /// <param name="sizeCategory">Body size category used for crater density scaling.</param>
    /// <param name="volcanismLevel">Current volcanism level [0–1].</param>
    /// <param name="rng">Seeded RNG for reproducible output.</param>
    /// <returns>A <see cref="TerrainProps"/> describing elevation, roughness, craters, and type.</returns>
    private static TerrainProps GenerateTerrain(
        PhysicalProps physical,
        SizeCategory.Category sizeCategory,
        double volcanismLevel,
        SeededRng rng)
    {
        double gravity = physical.GetSurfaceGravityMS2();
        double gravityFactor = 9.81 / System.Math.Max(gravity, 0.1);
        double elevationRangeM = 20000.0 * gravityFactor * rng.RandfRange(0.3f, 1.5f);
        elevationRangeM = System.Math.Clamp(elevationRangeM, 1000.0, 100000.0);

        double roughness = rng.RandfRange(0.2f, 0.8f);
        double craterDensity;
        if (sizeCategory == SizeCategory.Category.Dwarf)
        {
            craterDensity = rng.RandfRange(0.6f, 0.95f);
        }
        else if (volcanismLevel > 0.3)
        {
            craterDensity = rng.RandfRange(0.0f, 0.3f);
        }
        else
        {
            craterDensity = rng.RandfRange(0.2f, 0.7f);
        }

        double tectonicActivity = System.Math.Clamp(volcanismLevel * rng.RandfRange(0.8f, 1.2f), 0.0, 1.0);
        double erosionLevel = rng.RandfRange(0.1f, 0.5f);

        string terrainType;
        if (volcanismLevel > 0.5)
        {
            terrainType = "volcanic";
        }
        else if (craterDensity > 0.6)
        {
            terrainType = "cratered";
        }
        else if (tectonicActivity > 0.4)
        {
            terrainType = "tectonic";
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

    /// <summary>Returns whether the body can retain liquid surface water.</summary>
    private static bool CanHaveLiquidWater(double surfaceTempK, PhysicalProps physical)
    {
        if (surfaceTempK < WaterFreezeK)
        {
            return false;
        }

        if (surfaceTempK > WaterBoilK * 1.5)
        {
            return false;
        }

        return physical.GetEscapeVelocityMS() > 3000.0;
    }

    /// <summary>Generates hydrosphere properties for a body that can retain liquid water.</summary>
    /// <param name="surfaceTempK">Surface temperature in Kelvin.</param>
    /// <param name="sizeCategory">Body size category — larger bodies support deeper oceans.</param>
    /// <param name="volcanismLevel">Volcanism level [0–1]; high values may acidify water.</param>
    /// <param name="rng">Seeded RNG for reproducible output.</param>
    /// <returns>A <see cref="HydrosphereProps"/> describing ocean and ice coverage.</returns>
    private static HydrosphereProps GenerateHydrosphere(
        PlanetSpec spec,
        double surfaceTempK,
        SizeCategory.Category sizeCategory,
        double volcanismLevel,
        SeededRng rng)
    {
        double overrideOceanCoverage = spec.GetOverrideFloat("surface.hydrosphere.ocean_coverage", -1.0);
        double oceanCoverage;
        if (overrideOceanCoverage >= 0.0)
        {
            oceanCoverage = System.Math.Clamp(overrideOceanCoverage, 0.0, 1.0);
        }
        else if ((int)sizeCategory >= (int)SizeCategory.Category.Terrestrial)
        {
            oceanCoverage = rng.RandfRange(0.1f, 0.95f);
        }
        else
        {
            oceanCoverage = rng.RandfRange(0.0f, 0.3f);
        }

        double oceanDepthM = oceanCoverage * rng.RandfRange(1000.0f, 10000.0f);
        double overrideIceCoverage = spec.GetOverrideFloat("surface.hydrosphere.ice_coverage", -1.0);
        double iceCoverage = 0.0;

        if (overrideIceCoverage >= 0.0)
        {
            iceCoverage = System.Math.Clamp(overrideIceCoverage, 0.0, 1.0);
        }
        else if (surfaceTempK < WaterFreezeK + 20.0)
        {
            iceCoverage = rng.RandfRange(0.3f, 0.8f);
        }
        else if (surfaceTempK < WaterFreezeK + 50.0)
        {
            iceCoverage = rng.RandfRange(0.05f, 0.3f);
        }

        string waterType;
        if (volcanismLevel > 0.5 && rng.Randf() < 0.3f)
        {
            waterType = "acidic_water";
        }
        else
        {
            waterType = "water";
        }
        return new HydrosphereProps(
            oceanCoverage,
            oceanDepthM,
            iceCoverage,
            rng.RandfRange(10.0f, 50.0f),
            waterType);
    }

    private static bool HasHydrosphereOverride(PlanetSpec spec)
    {
        return spec.HasOverride("surface.hydrosphere.ocean_coverage")
            || spec.HasOverride("surface.hydrosphere.ice_coverage");
    }

    /// <summary>Generates cryosphere (polar caps, ice) from surface temperature.</summary>
    private static CryosphereProps GenerateCryosphere(
        double surfaceTempK,
        PhysicalProps physical,
        SeededRng rng)
    {
        double polarCapCoverage = surfaceTempK switch
        {
            < WaterFreezeK => rng.RandfRange(0.3f, 1.0f),
            < WaterFreezeK + 30.0 => rng.RandfRange(0.05f, 0.4f),
            _ => rng.RandfRange(0.0f, 0.1f),
        };

        double permafrostDepthM = 0.0;
        if (surfaceTempK < WaterFreezeK)
        {
            permafrostDepthM = rng.RandfRange(100.0f, 5000.0f);
        }
        else if (polarCapCoverage > 0.1)
        {
            permafrostDepthM = rng.RandfRange(10.0f, 500.0f);
        }

        bool hasSubsurfaceOcean = false;
        double subsurfaceOceanDepthM = 0.0;
        if (surfaceTempK < WaterFreezeK && physical.InternalHeatWatts > 1.0e12 && rng.Randf() < 0.4f)
        {
            hasSubsurfaceOcean = true;
            subsurfaceOceanDepthM = rng.RandfRange(10000.0f, 100000.0f);
        }

        double cryovolcanismLevel = 0.0;
        if (hasSubsurfaceOcean && physical.InternalHeatWatts > 5.0e12)
        {
            cryovolcanismLevel = rng.RandfRange(0.1f, 0.6f);
        }

        string iceType = "water_ice";
        if (surfaceTempK < 100.0)
        {
            double roll = rng.Randf();
            if (roll < 0.3)
            {
                iceType = "nitrogen_ice";
            }
            else if (roll < 0.5)
            {
                iceType = "methane_ice";
            }
            else if (roll < 0.7)
            {
                iceType = "co2_ice";
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
