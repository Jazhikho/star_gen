using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Celestial.Validation;

/// <summary>
/// Validates celestial bodies and their components.
/// </summary>
public static class CelestialValidator
{
    /// <summary>
    /// Validates a complete celestial body.
    /// </summary>
    public static ValidationResult Validate(CelestialBody body)
    {
        ValidationResult result = new();

        ValidateIdentity(body, result);
        ValidatePhysical(body.Physical, result);

        if (body.HasStellar())
        {
            ValidateStellar(body.Stellar!, body.Type, result);
        }

        if (body.HasOrbital())
        {
            ValidateOrbital(body.Orbital!, result);
        }

        if (body.HasSurface())
        {
            ValidateSurface(body.Surface!, result);
        }

        if (body.HasAtmosphere())
        {
            ValidateAtmosphere(body.Atmosphere!, result);
        }

        if (body.HasRingSystem())
        {
            ValidateRingSystem(body.RingSystem!, body.Physical, result);
        }

        ValidateTypeConsistency(body, result);
        return result;
    }

    private static void ValidateIdentity(CelestialBody body, ValidationResult result)
    {
        if (string.IsNullOrEmpty(body.Id))
        {
            result.AddError("id", "ID cannot be empty");
        }

        if (string.IsNullOrEmpty(body.Name))
        {
            result.AddWarning("name", "Name is empty");
        }
    }

    private static void ValidatePhysical(PhysicalProps? physical, ValidationResult result)
    {
        if (physical == null)
        {
            result.AddError("physical", "Physical properties are required");
            return;
        }

        if (physical.MassKg <= 0.0)
        {
            result.AddError("physical.mass_kg", "Mass must be greater than zero");
        }

        if (physical.RadiusM <= 0.0)
        {
            result.AddError("physical.radius_m", "Radius must be greater than zero");
        }

        if (physical.AxialTiltDeg < 0.0 || physical.AxialTiltDeg > 180.0)
        {
            result.AddWarning("physical.axial_tilt_deg", "Axial tilt should be between 0 and 180 degrees");
        }

        if (physical.Oblateness < 0.0 || physical.Oblateness >= 1.0)
        {
            result.AddError("physical.oblateness", "Oblateness must be between 0 and 1");
        }

        if (physical.InternalHeatWatts < 0.0)
        {
            result.AddError("physical.internal_heat_watts", "Internal heat cannot be negative");
        }
    }

    private static void ValidateStellar(StellarProps stellar, CelestialType.Type bodyType, ValidationResult result)
    {
        if (bodyType != CelestialType.Type.Star)
        {
            result.AddWarning("stellar", "Non-star body has stellar properties");
        }

        if (stellar.LuminosityWatts < 0.0)
        {
            result.AddError("stellar.luminosity_watts", "Luminosity cannot be negative");
        }

        if (stellar.EffectiveTemperatureK < 0.0)
        {
            result.AddError("stellar.effective_temperature_k", "Effective temperature cannot be negative");
        }

        if (stellar.Metallicity < 0.0)
        {
            result.AddError("stellar.metallicity", "Metallicity cannot be negative");
        }

        if (stellar.AgeYears < 0.0)
        {
            result.AddError("stellar.age_years", "Age cannot be negative");
        }
    }

    private static void ValidateOrbital(OrbitalProps orbital, ValidationResult result)
    {
        if (orbital.SemiMajorAxisM <= 0.0)
        {
            result.AddError("orbital.semi_major_axis_m", "Semi-major axis must be greater than zero");
        }

        if (orbital.Eccentricity < 0.0)
        {
            result.AddError("orbital.eccentricity", "Eccentricity cannot be negative");
        }

        if (orbital.Eccentricity >= 1.0)
        {
            result.AddWarning("orbital.eccentricity", "Eccentricity >= 1.0 indicates unbound orbit");
        }

        if (orbital.InclinationDeg < 0.0 || orbital.InclinationDeg > 180.0)
        {
            result.AddWarning("orbital.inclination_deg", "Inclination should be between 0 and 180 degrees");
        }
    }

    private static void ValidateSurface(SurfaceProps surface, ValidationResult result)
    {
        if (surface.TemperatureK < 0.0)
        {
            result.AddError("surface.temperature_k", "Temperature cannot be negative");
        }

        if (surface.Albedo < 0.0 || surface.Albedo > 1.0)
        {
            result.AddError("surface.albedo", "Albedo must be between 0 and 1");
        }

        if (surface.VolcanismLevel < 0.0 || surface.VolcanismLevel > 1.0)
        {
            result.AddError("surface.volcanism_level", "Volcanism level must be between 0 and 1");
        }

        if (surface.HasTerrain())
        {
            ValidateTerrain(surface.Terrain!, result);
        }

        if (surface.HasHydrosphere())
        {
            ValidateHydrosphere(surface.Hydrosphere!, result);
        }

        if (surface.HasCryosphere())
        {
            ValidateCryosphere(surface.Cryosphere!, result);
        }
    }

    private static void ValidateTerrain(TerrainProps terrain, ValidationResult result)
    {
        if (terrain.ElevationRangeM < 0.0)
        {
            result.AddError("surface.terrain.elevation_range_m", "Elevation range cannot be negative");
        }

        if (terrain.Roughness < 0.0 || terrain.Roughness > 1.0)
        {
            result.AddError("surface.terrain.roughness", "Roughness must be between 0 and 1");
        }

        if (terrain.CraterDensity < 0.0 || terrain.CraterDensity > 1.0)
        {
            result.AddError("surface.terrain.crater_density", "Crater density must be between 0 and 1");
        }

        if (terrain.TectonicActivity < 0.0 || terrain.TectonicActivity > 1.0)
        {
            result.AddError("surface.terrain.tectonic_activity", "Tectonic activity must be between 0 and 1");
        }

        if (terrain.ErosionLevel < 0.0 || terrain.ErosionLevel > 1.0)
        {
            result.AddError("surface.terrain.erosion_level", "Erosion level must be between 0 and 1");
        }
    }

    private static void ValidateHydrosphere(HydrosphereProps hydrosphere, ValidationResult result)
    {
        if (hydrosphere.OceanCoverage < 0.0 || hydrosphere.OceanCoverage > 1.0)
        {
            result.AddError("surface.hydrosphere.ocean_coverage", "Ocean coverage must be between 0 and 1");
        }

        if (hydrosphere.OceanDepthM < 0.0)
        {
            result.AddError("surface.hydrosphere.ocean_depth_m", "Ocean depth cannot be negative");
        }

        if (hydrosphere.IceCoverage < 0.0 || hydrosphere.IceCoverage > 1.0)
        {
            result.AddError("surface.hydrosphere.ice_coverage", "Ice coverage must be between 0 and 1");
        }

        if (hydrosphere.SalinityPpt < 0.0)
        {
            result.AddError("surface.hydrosphere.salinity_ppt", "Salinity cannot be negative");
        }
    }

    private static void ValidateCryosphere(CryosphereProps cryosphere, ValidationResult result)
    {
        if (cryosphere.PolarCapCoverage < 0.0 || cryosphere.PolarCapCoverage > 1.0)
        {
            result.AddError("surface.cryosphere.polar_cap_coverage", "Polar cap coverage must be between 0 and 1");
        }

        if (cryosphere.PermafrostDepthM < 0.0)
        {
            result.AddError("surface.cryosphere.permafrost_depth_m", "Permafrost depth cannot be negative");
        }

        if (cryosphere.SubsurfaceOceanDepthM < 0.0)
        {
            result.AddError("surface.cryosphere.subsurface_ocean_depth_m", "Subsurface ocean depth cannot be negative");
        }

        if (cryosphere.CryovolcanismLevel < 0.0 || cryosphere.CryovolcanismLevel > 1.0)
        {
            result.AddError("surface.cryosphere.cryovolcanism_level", "Cryovolcanism level must be between 0 and 1");
        }
    }

    private static void ValidateAtmosphere(AtmosphereProps atmosphere, ValidationResult result)
    {
        if (atmosphere.SurfacePressurePa < 0.0)
        {
            result.AddError("atmosphere.surface_pressure_pa", "Surface pressure cannot be negative");
        }

        if (atmosphere.ScaleHeightM < 0.0)
        {
            result.AddError("atmosphere.scale_height_m", "Scale height cannot be negative");
        }

        if (atmosphere.GreenhouseFactor < 0.0)
        {
            result.AddError("atmosphere.greenhouse_factor", "Greenhouse factor cannot be negative");
        }

        double compositionSum = atmosphere.GetCompositionSum();
        if (atmosphere.Composition.Count > 0 && (compositionSum < 0.99 || compositionSum > 1.01))
        {
            result.AddWarning(
                "atmosphere.composition",
                $"Composition fractions should sum to 1.0 (got {compositionSum:0.000})");
        }
    }

    private static void ValidateRingSystem(RingSystemProps ringSystem, PhysicalProps physical, ValidationResult result)
    {
        if (ringSystem.TotalMassKg < 0.0)
        {
            result.AddError("ring_system.total_mass_kg", "Total mass cannot be negative");
        }

        if (ringSystem.Bands.Count == 0)
        {
            result.AddWarning("ring_system.bands", "Ring system has no bands");
            return;
        }

        double previousOuter = 0.0;
        for (int index = 0; index < ringSystem.Bands.Count; index += 1)
        {
            RingBand band = ringSystem.Bands[index];
            string prefix = $"ring_system.bands[{index}]";

            if (band.InnerRadiusM <= 0.0)
            {
                result.AddError($"{prefix}.inner_radius_m", "Inner radius must be greater than zero");
            }

            if (band.OuterRadiusM <= 0.0)
            {
                result.AddError($"{prefix}.outer_radius_m", "Outer radius must be greater than zero");
            }

            if (band.InnerRadiusM >= band.OuterRadiusM)
            {
                result.AddError(prefix, "Inner radius must be less than outer radius");
            }

            if (band.OpticalDepth < 0.0)
            {
                result.AddError($"{prefix}.optical_depth", "Optical depth cannot be negative");
            }

            if (band.ParticleSizeM <= 0.0)
            {
                result.AddError($"{prefix}.particle_size_m", "Particle size must be greater than zero");
            }

            if (physical.RadiusM > 0.0 && band.InnerRadiusM < physical.RadiusM)
            {
                result.AddError($"{prefix}.inner_radius_m", "Ring inner radius cannot be less than body radius");
            }

            if (index > 0 && band.InnerRadiusM < previousOuter)
            {
                result.AddWarning(prefix, "Ring bands overlap");
            }

            previousOuter = band.OuterRadiusM;
        }
    }

    private static void ValidateTypeConsistency(CelestialBody body, ValidationResult result)
    {
        switch (body.Type)
        {
            case CelestialType.Type.Star:
                if (body.HasSurface())
                {
                    result.AddWarning("surface", "Stars typically don't have surface properties");
                }

                if (!body.HasStellar())
                {
                    result.AddWarning("stellar", "Star should have stellar properties");
                }
                break;

            case CelestialType.Type.Asteroid:
                if (body.HasAtmosphere())
                {
                    result.AddWarning("atmosphere", "Asteroids rarely have atmospheres");
                }
                break;

            case CelestialType.Type.Planet:
                if (body.HasStellar())
                {
                    result.AddWarning("stellar", "Planets should not have stellar properties");
                }
                break;

            case CelestialType.Type.Moon:
                if (body.HasStellar())
                {
                    result.AddWarning("stellar", "Moons should not have stellar properties");
                }
                break;
        }
    }
}
