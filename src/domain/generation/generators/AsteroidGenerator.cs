using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Generates asteroid celestial bodies from asteroid specifications.
/// </summary>
public static class AsteroidGenerator
{
    private static readonly float[] TypeWeights =
    {
        75.0f,
        17.0f,
        8.0f,
    };

    private static readonly AsteroidType.Type[] AsteroidTypes =
    {
        AsteroidType.Type.CType,
        AsteroidType.Type.SType,
        AsteroidType.Type.MType,
    };

    private const double TypicalMassMinKg = 1.0e10;
    private const double TypicalMassMaxKg = 1.0e18;
    private const double LargeMassMinKg = 1.0e19;
    private const double LargeMassMaxKg = 1.0e21;
    private const double MainBeltInnerAu = 2.1;
    private const double MainBeltOuterAu = 3.3;
    private const double BeltEccentricityMax = 0.3;
    private const double BeltInclinationMaxDeg = 30.0;

    /// <summary>
    /// Generates an asteroid from a specification and parent context.
    /// </summary>
    public static CelestialBody Generate(AsteroidSpec spec, ParentContext context, SeededRng rng)
    {
        AsteroidType.Type asteroidType = DetermineAsteroidType(spec, rng);
        PhysicalProps physical = GeneratePhysicalProps(spec, asteroidType, rng);
        OrbitalProps orbital = GenerateOrbitalProps(spec, rng);
        double equilibriumTempK = context.GetEquilibriumTemperatureK(AsteroidType.GetTypicalAlbedo(asteroidType));
        SurfaceProps surface = GenerateSurface(spec, physical, asteroidType, equilibriumTempK, rng);
        string bodyId = GenerateId(spec, rng);
        Provenance provenance = CreateProvenance(spec, context);

        CelestialBody body = new(bodyId, spec.NameHint, CelestialType.Type.Asteroid, physical, provenance)
        {
            Orbital = orbital,
            Surface = surface,
            Atmosphere = null,
        };
        return body;
    }

    /// <summary>Picks asteroid type from spec override or weighted RNG.</summary>
    private static AsteroidType.Type DetermineAsteroidType(AsteroidSpec spec, SeededRng rng)
    {
        if (spec.HasAsteroidType())
        {
            return (AsteroidType.Type)spec.AsteroidType;
        }

        AsteroidType.Type? selected = rng.WeightedChoice(AsteroidTypes, TypeWeights);
        if (selected == null)
        {
            GD.PushError("AsteroidGenerator.DetermineAsteroidType: WeightedChoice returned null — type weight table may be empty or invalid.");
            throw new InvalidOperationException("WeightedChoice returned null for AsteroidType.");
        }

        return selected.Value;
    }

    /// <summary>Builds physical properties from type, density/mass ranges, and overrides.</summary>
    private static PhysicalProps GeneratePhysicalProps(
        AsteroidSpec spec,
        AsteroidType.Type asteroidType,
        SeededRng rng)
    {
        Dictionary densityRange = GetDensityRange(asteroidType);
        double densityKgM3 = spec.GetOverrideFloat("physical.density_kg_m3", -1.0);
        if (densityKgM3 < 0.0)
        {
            densityKgM3 = rng.RandfRange((float)(double)densityRange["min"], (float)(double)densityRange["max"]);
        }

        double massKg = spec.GetOverrideFloat("physical.mass_kg", -1.0);
        if (massKg < 0.0)
        {
            double massMinKg;
            double massMaxKg;
            if (spec.IsLarge)
            {
                massMinKg = LargeMassMinKg;
                massMaxKg = LargeMassMaxKg;
            }
            else
            {
                massMinKg = TypicalMassMinKg;
                massMaxKg = TypicalMassMaxKg;
            }

            double logMin = System.Math.Log(massMinKg);
            double logMax = System.Math.Log(massMaxKg);
            massKg = System.Math.Exp(rng.RandfRange((float)logMin, (float)logMax));
        }

        double radiusM = spec.GetOverrideFloat("physical.radius_m", -1.0);
        if (radiusM < 0.0)
        {
            double volumeM3 = massKg / densityKgM3;
            radiusM = System.Math.Pow(volumeM3 * 3.0 / (4.0 * System.Math.PI), 1.0 / 3.0);
        }

        double rotationPeriodS = spec.GetOverrideFloat("physical.rotation_period_s", -1.0);
        if (rotationPeriodS < 0.0)
        {
            rotationPeriodS = GenerateRotationPeriod(radiusM, rng);
        }

        double axialTiltDeg = spec.GetOverrideFloat("physical.axial_tilt_deg", -1.0);
        if (axialTiltDeg < 0.0)
        {
            axialTiltDeg = rng.RandfRange(0.0f, 180.0f);
        }

        double oblateness = spec.GetOverrideFloat("physical.oblateness", -1.0);
        if (oblateness < 0.0)
        {
            if (spec.IsLarge)
            {
                oblateness = rng.RandfRange(0.0f, 0.1f);
            }
            else
            {
                oblateness = rng.RandfRange(0.0f, 0.4f);
            }
        }

        double magneticMoment = spec.GetOverrideFloat("physical.magnetic_moment", 0.0);

        double internalHeatWatts = spec.GetOverrideFloat("physical.internal_heat_watts", -1.0);
        if (internalHeatWatts < 0.0)
        {
            if (spec.IsLarge)
            {
                internalHeatWatts = rng.RandfRange(1.0e6f, 1.0e10f);
            }
            else
            {
                internalHeatWatts = rng.RandfRange(0.0f, 1.0e6f);
            }
        }

        return new PhysicalProps(
            massKg,
            radiusM,
            rotationPeriodS,
            axialTiltDeg,
            oblateness,
            magneticMoment,
            internalHeatWatts);
    }

    /// <summary>Generates rotation period in seconds from radius and RNG.</summary>
    private static double GenerateRotationPeriod(double radiusM, SeededRng rng)
    {
        double radiusKm = radiusM / 1000.0;
        double minHours;
        double maxHours;

        if (radiusKm < 1.0)
        {
            minHours = 0.1;
            maxHours = 100.0;
        }
        else if (radiusKm < 10.0)
        {
            minHours = 2.0;
            maxHours = 24.0;
        }
        else if (radiusKm < 100.0)
        {
            minHours = 4.0;
            maxHours = 30.0;
        }
        else
        {
            minHours = 5.0;
            maxHours = 20.0;
        }

        double periodHours = System.Math.Exp(rng.RandfRange(
            (float)System.Math.Log(minHours),
            (float)System.Math.Log(maxHours)));

        if (rng.Randf() < 0.3f)
        {
            periodHours = -periodHours;
        }

        return periodHours * 3600.0;
    }

    /// <summary>Builds orbital properties for main-belt style orbit with overrides.</summary>
    private static OrbitalProps GenerateOrbitalProps(AsteroidSpec spec, SeededRng rng)
    {
        double semiMajorAxisM = spec.GetOverrideFloat("orbital.semi_major_axis_m", -1.0);
        if (semiMajorAxisM < 0.0)
        {
            double innerM = MainBeltInnerAu * Units.AuMeters;
            double outerM = MainBeltOuterAu * Units.AuMeters;
            semiMajorAxisM = System.Math.Exp(rng.RandfRange(
                (float)System.Math.Log(innerM),
                (float)System.Math.Log(outerM)));
        }

        double eccentricity = spec.GetOverrideFloat("orbital.eccentricity", -1.0);
        if (eccentricity < 0.0)
        {
            double raw = rng.Randf();
            eccentricity = raw * raw * BeltEccentricityMax;
        }

        double inclinationDeg = spec.GetOverrideFloat("orbital.inclination_deg", -1.0);
        if (inclinationDeg < 0.0)
        {
            double raw = rng.Randf();
            inclinationDeg = raw * raw * BeltInclinationMaxDeg;
        }

        double longitudeOfAscendingNodeDeg = spec.GetOverrideFloat(
            "orbital.longitude_of_ascending_node_deg",
            rng.RandfRange(0.0f, 360.0f));
        double argumentOfPeriapsisDeg = spec.GetOverrideFloat(
            "orbital.argument_of_periapsis_deg",
            rng.RandfRange(0.0f, 360.0f));
        double meanAnomalyDeg = spec.GetOverrideFloat(
            "orbital.mean_anomaly_deg",
            rng.RandfRange(0.0f, 360.0f));

        return new OrbitalProps(
            semiMajorAxisM,
            eccentricity,
            inclinationDeg,
            longitudeOfAscendingNodeDeg,
            argumentOfPeriapsisDeg,
            meanAnomalyDeg,
            string.Empty);
    }

    /// <summary>Builds surface props (albedo, type, composition, terrain) from type and RNG.</summary>
    private static SurfaceProps GenerateSurface(
        AsteroidSpec spec,
        PhysicalProps physical,
        AsteroidType.Type asteroidType,
        double equilibriumTempK,
        SeededRng rng)
    {
        Dictionary albedoRange = GetAlbedoRange(asteroidType);
        double albedo = spec.GetOverrideFloat("surface.albedo", -1.0);
        if (albedo < 0.0)
        {
            albedo = rng.RandfRange((float)(double)albedoRange["min"], (float)(double)albedoRange["max"]);
        }

        SurfaceProps surface = new(
            equilibriumTempK,
            albedo,
            GetSurfaceType(asteroidType),
            0.0,
            GenerateSurfaceComposition(asteroidType, rng));

        surface.Terrain = GenerateTerrain(physical, spec.IsLarge, rng);
        return surface;
    }

    /// <summary>Returns min/max density range for the given asteroid type.</summary>
    private static Dictionary GetDensityRange(AsteroidType.Type asteroidType)
    {
        return asteroidType switch
        {
            AsteroidType.Type.CType => BuildRange(1100.0, 2500.0),
            AsteroidType.Type.SType => BuildRange(2200.0, 3500.0),
            AsteroidType.Type.MType => BuildRange(4500.0, 7500.0),
            _ => throw new InvalidOperationException($"AsteroidGenerator.GetDensityRange: unrecognized asteroid type '{asteroidType}'."),
        };
    }

    /// <summary>Returns min/max albedo range for the given asteroid type.</summary>
    private static Dictionary GetAlbedoRange(AsteroidType.Type asteroidType)
    {
        return asteroidType switch
        {
            AsteroidType.Type.CType => BuildRange(0.03, 0.10),
            AsteroidType.Type.SType => BuildRange(0.10, 0.30),
            AsteroidType.Type.MType => BuildRange(0.10, 0.25),
            _ => throw new InvalidOperationException($"AsteroidGenerator.GetAlbedoRange: unrecognized asteroid type '{asteroidType}'."),
        };
    }

    /// <summary>Maps asteroid type to surface type string.</summary>
    private static string GetSurfaceType(AsteroidType.Type asteroidType)
    {
        return asteroidType switch
        {
            AsteroidType.Type.CType => "carbonaceous",
            AsteroidType.Type.SType => "silicaceous",
            AsteroidType.Type.MType => "metallic",
            _ => throw new InvalidOperationException($"AsteroidGenerator.GetSurfaceType: unrecognized asteroid type '{asteroidType}'."),
        };
    }

    /// <summary>Generates normalized surface composition dict for the asteroid type.</summary>
    private static Dictionary GenerateSurfaceComposition(AsteroidType.Type asteroidType, SeededRng rng)
    {
        Dictionary composition = new();

        switch (asteroidType)
        {
            case AsteroidType.Type.CType:
                composition["carbon_compounds"] = rng.RandfRange(0.15f, 0.30f);
                composition["hydrated_silicates"] = rng.RandfRange(0.30f, 0.50f);
                composition["organics"] = rng.RandfRange(0.05f, 0.15f);
                composition["water_ice"] = rng.RandfRange(0.05f, 0.20f);
                composition["magnetite"] = rng.RandfRange(0.05f, 0.15f);
                break;
            case AsteroidType.Type.SType:
                composition["silicates"] = rng.RandfRange(0.40f, 0.60f);
                composition["pyroxene"] = rng.RandfRange(0.15f, 0.25f);
                composition["olivine"] = rng.RandfRange(0.10f, 0.20f);
                composition["nickel_iron"] = rng.RandfRange(0.05f, 0.15f);
                break;
            case AsteroidType.Type.MType:
                composition["iron"] = rng.RandfRange(0.70f, 0.85f);
                composition["nickel"] = rng.RandfRange(0.10f, 0.20f);
                composition["cobalt"] = rng.RandfRange(0.01f, 0.05f);
                composition["silicates"] = rng.RandfRange(0.02f, 0.10f);
                break;
            default:
                throw new InvalidOperationException($"AsteroidGenerator.GenerateSurfaceComposition: unrecognized asteroid type '{asteroidType}'.");
        }

        return NormalizeComposition(composition);
    }

    /// <summary>Builds terrain props (elevation, roughness) for the asteroid.</summary>
    private static TerrainProps GenerateTerrain(PhysicalProps physical, bool isLarge, SeededRng rng)
    {
        double elevationFraction;
        if (isLarge)
        {
            elevationFraction = rng.RandfRange(0.01f, 0.05f);
        }
        else
        {
            elevationFraction = rng.RandfRange(0.05f, 0.30f);
        }
        double elevationRangeM = System.Math.Max(physical.RadiusM * 2.0 * elevationFraction, 10.0);

        return new TerrainProps(
            elevationRangeM,
            rng.RandfRange(0.5f, 1.0f),
            rng.RandfRange(0.6f, 0.95f),
            0.0,
            0.0,
            "cratered");
    }

    /// <summary>Creates provenance from spec and context.</summary>
    private static Provenance CreateProvenance(AsteroidSpec spec, ParentContext context)
    {
        Dictionary specSnapshot = spec.ToDictionary();
        specSnapshot["context"] = context.ToDictionary();
        return Provenance.CreateCurrent(spec.GenerationSeed, specSnapshot);
    }

    /// <summary>Produces unique asteroid id from spec override or RNG.</summary>
    private static string GenerateId(AsteroidSpec spec, SeededRng rng)
    {
        Variant overrideId = spec.GetOverride("id", default);
        if (overrideId.VariantType == Variant.Type.String)
        {
            string id = (string)overrideId;
            if (!string.IsNullOrEmpty(id))
            {
                return id;
            }
        }

        int randomPart = (int)(rng.Randi() % 1_000_000u);
        return GeneratorUtils.GenerateIdFromRandomPart("asteroid", randomPart);
    }

    /// <summary>Normalizes composition fractions so they sum to 1.</summary>
    private static Dictionary NormalizeComposition(Dictionary composition)
    {
        double total = 0.0;
        foreach (Variant fraction in composition.Values)
        {
            total += (double)fraction;
        }

        if (total <= 0.0)
        {
            GD.PushError("AsteroidGenerator.NormalizeComposition: composition total is zero or negative — input was empty or contained no positive fractions.");
            return composition;
        }

        foreach (Variant key in composition.Keys)
        {
            composition[key] = (double)composition[key] / total;
        }

        return composition;
    }

    /// <summary>Builds a min/max range dictionary.</summary>
    private static Dictionary BuildRange(double min, double max)
    {
        return new Dictionary
        {
            ["min"] = min,
            ["max"] = max,
        };
    }
}
