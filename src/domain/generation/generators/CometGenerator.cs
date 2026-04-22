using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Utils;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Generates comet celestial bodies from comet specifications.
/// </summary>
public static class CometGenerator
{
    /// <summary>
    /// Supported comet families.
    /// </summary>
    public enum FamilyType
    {
        JupiterFamily = 0,
        LongPeriod = 1,
    }

    /// <summary>
    /// Supported comet activity states.
    /// </summary>
    public enum ActivityType
    {
        Active = 0,
        Dormant = 1,
        Extinct = 2,
    }

    /// <summary>
    /// Generates a comet from a specification and parent context.
    /// </summary>
    public static CelestialBody Generate(CometSpec spec, ParentContext context, SeededRng rng)
    {
        FamilyType family = DetermineFamily(spec, rng);
        ActivityType activity = DetermineActivity(spec, rng);
        PhysicalProps physical = GeneratePhysicalProps(spec, rng);
        OrbitalProps orbital = GenerateOrbitalProps(spec, family, rng);
        SurfaceProps surface = GenerateSurface(spec, activity, context, rng);
        string bodyId = GenerateId(spec, rng);
        Provenance provenance = CreateProvenance(spec, context);

        CelestialBody body = new(bodyId, spec.NameHint, CelestialType.Type.Comet, physical, provenance)
        {
            Orbital = orbital,
            Surface = surface,
            Atmosphere = null,
        };

        body.SetMeta("comet_family", FamilyToString(family));
        body.SetMeta("comet_activity", ActivityToString(activity));
        return body;
    }

    /// <summary>
    /// Returns the display label for a comet family.
    /// </summary>
    public static string FamilyToString(FamilyType family)
    {
        if (family == FamilyType.JupiterFamily)
        {
            return "Jupiter-family";
        }

        return "Long-period";
    }

    /// <summary>
    /// Returns the display label for a comet activity state.
    /// </summary>
    public static string ActivityToString(ActivityType activity)
    {
        if (activity == ActivityType.Active)
        {
            return "Active";
        }

        if (activity == ActivityType.Dormant)
        {
            return "Dormant";
        }

        return "Extinct";
    }

    private static FamilyType DetermineFamily(CometSpec spec, SeededRng rng)
    {
        if (spec.HasFamily())
        {
            return (FamilyType)spec.Family;
        }

        if (rng.Randf() < 0.65f)
        {
            return FamilyType.JupiterFamily;
        }

        return FamilyType.LongPeriod;
    }

    private static ActivityType DetermineActivity(CometSpec spec, SeededRng rng)
    {
        if (spec.HasActivityState())
        {
            return (ActivityType)spec.ActivityState;
        }

        double roll = rng.Randf();
        if (roll < 0.55)
        {
            return ActivityType.Active;
        }

        if (roll < 0.85)
        {
            return ActivityType.Dormant;
        }

        return ActivityType.Extinct;
    }

    private static PhysicalProps GeneratePhysicalProps(CometSpec spec, SeededRng rng)
    {
        double densityKgM3 = spec.GetOverrideFloat("physical.density_kg_m3", -1.0);
        if (densityKgM3 < 0.0)
        {
            if (spec.IsLarge)
            {
                densityKgM3 = rng.RandfRange(450.0f, 700.0f);
            }
            else
            {
                densityKgM3 = rng.RandfRange(300.0f, 600.0f);
            }
        }

        double radiusM = spec.GetOverrideFloat("physical.radius_m", -1.0);
        if (radiusM < 0.0)
        {
            if (spec.IsLarge)
            {
                radiusM = rng.RandfRange(7000.0f, 30000.0f);
            }
            else
            {
                radiusM = rng.RandfRange(1000.0f, 12000.0f);
            }
        }

        double volumeM3 = (4.0 / 3.0) * System.Math.PI * System.Math.Pow(radiusM, 3.0);
        double massKg = spec.GetOverrideFloat("physical.mass_kg", -1.0);
        if (massKg < 0.0)
        {
            massKg = volumeM3 * densityKgM3;
        }

        double rotationPeriodS = spec.GetOverrideFloat("physical.rotation_period_s", -1.0);
        if (rotationPeriodS < 0.0)
        {
            rotationPeriodS = rng.RandfRange(4.0f, 72.0f) * 3600.0;
        }

        double axialTiltDeg = spec.GetOverrideFloat("physical.axial_tilt_deg", -1.0);
        if (axialTiltDeg < 0.0)
        {
            axialTiltDeg = rng.RandfRange(0.0f, 180.0f);
        }

        double internalHeatWatts = spec.GetOverrideFloat("physical.internal_heat_watts", -1.0);
        if (internalHeatWatts < 0.0)
        {
            internalHeatWatts = rng.RandfRange(0.0f, 5.0e7f);
        }

        return new PhysicalProps(
            massKg,
            radiusM,
            rotationPeriodS,
            axialTiltDeg,
            rng.RandfRange(0.05f, 0.35f),
            0.0,
            internalHeatWatts);
    }

    private static OrbitalProps GenerateOrbitalProps(CometSpec spec, FamilyType family, SeededRng rng)
    {
        double semiMajorAxisM = spec.GetOverrideFloat("orbital.semi_major_axis_m", -1.0);
        if (semiMajorAxisM < 0.0)
        {
            if (family == FamilyType.JupiterFamily)
            {
                semiMajorAxisM = rng.RandfRange(3.0f, 8.0f) * Units.AuMeters;
            }
            else
            {
                semiMajorAxisM = rng.RandfRange(20.0f, 200.0f) * Units.AuMeters;
            }
        }

        double eccentricity = spec.GetOverrideFloat("orbital.eccentricity", -1.0);
        if (eccentricity < 0.0)
        {
            if (family == FamilyType.JupiterFamily)
            {
                eccentricity = rng.RandfRange(0.35f, 0.75f);
            }
            else
            {
                eccentricity = rng.RandfRange(0.85f, 0.995f);
            }
        }

        double inclinationDeg = spec.GetOverrideFloat("orbital.inclination_deg", -1.0);
        if (inclinationDeg < 0.0)
        {
            if (family == FamilyType.JupiterFamily)
            {
                inclinationDeg = rng.RandfRange(0.0f, 35.0f);
            }
            else
            {
                inclinationDeg = rng.RandfRange(0.0f, 160.0f);
            }
        }

        return new OrbitalProps(
            semiMajorAxisM,
            eccentricity,
            inclinationDeg,
            rng.RandfRange(0.0f, 360.0f),
            rng.RandfRange(0.0f, 360.0f),
            rng.RandfRange(0.0f, 360.0f),
            string.Empty);
    }

    private static SurfaceProps GenerateSurface(CometSpec spec, ActivityType activity, ParentContext context, SeededRng rng)
    {
        double albedo = spec.GetOverrideFloat("surface.albedo", -1.0);
        if (albedo < 0.0)
        {
            if (activity == ActivityType.Extinct)
            {
                albedo = rng.RandfRange(0.05f, 0.12f);
            }
            else
            {
                albedo = rng.RandfRange(0.02f, 0.08f);
            }
        }

        double temperatureK = context.GetEquilibriumTemperatureK(albedo);
        double volcanism = spec.GetOverrideFloat("surface.volcanism_level", -1.0);
        if (volcanism < 0.0)
        {
            volcanism = 0.0;
        }

        Dictionary composition = new Dictionary
        {
            ["water_ice"] = 0.45,
            ["carbon_compounds"] = 0.18,
            ["silicates"] = 0.22,
            ["organics"] = 0.10,
            ["co2_ice"] = 0.05,
        };

        if (activity == ActivityType.Extinct)
        {
            composition["dust"] = 0.18;
            composition["water_ice"] = 0.25;
        }

        SurfaceProps surface = new(
            temperatureK,
            albedo,
            "cometary",
            volcanism,
            NormalizeComposition(composition));
        surface.Terrain = new TerrainProps(
            rng.RandfRange(100.0f, 3000.0f),
            rng.RandfRange(0.65f, 1.0f),
            rng.RandfRange(0.3f, 0.8f),
            0.0,
            0.0,
            "fractured");
        return surface;
    }

    private static Provenance CreateProvenance(CometSpec spec, ParentContext context)
    {
        Dictionary specSnapshot = spec.ToDictionary();
        specSnapshot["context"] = context.ToDictionary();
        return Provenance.CreateCurrent(spec.GenerationSeed, specSnapshot);
    }

    private static string GenerateId(CometSpec spec, SeededRng rng)
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
        return GeneratorUtils.GenerateIdFromRandomPart("comet", randomPart);
    }

    private static Dictionary NormalizeComposition(Dictionary composition)
    {
        double total = 0.0;
        foreach (Variant value in composition.Values)
        {
            total += (double)value;
        }

        foreach (Variant key in composition.Keys)
        {
            composition[key] = (double)composition[key] / total;
        }

        return composition;
    }
}
