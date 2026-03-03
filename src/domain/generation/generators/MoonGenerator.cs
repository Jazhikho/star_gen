using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators.Moon;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Generation.Utils;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Generates moon celestial bodies from moon specifications.
/// </summary>
public static class MoonGenerator
{
    private static readonly float[] SizeCategoryWeightsRegular =
    {
        30.0f,
        50.0f,
        15.0f,
        5.0f,
    };

    private static readonly float[] SizeCategoryWeightsCaptured =
    {
        80.0f,
        18.0f,
        2.0f,
        0.0f,
    };

    private static readonly SizeCategory.Category[] MoonSizes =
    {
        SizeCategory.Category.Dwarf,
        SizeCategory.Category.SubTerrestrial,
        SizeCategory.Category.Terrestrial,
        SizeCategory.Category.SuperEarth,
    };

    private const double MaxHillFractionPrograde = 0.5;
    private const double MaxHillFractionRetrograde = 0.7;

    /// <summary>
    /// Generates a moon from a specification and parent context.
    /// </summary>
    public static CelestialBody? Generate(
        MoonSpec spec,
        ParentContext context,
        SeededRng rng,
        bool enablePopulation = false,
        CelestialBody? parentBody = null,
        int populationOverride = 0)
    {
        if (!context.HasParentBody())
        {
            return null;
        }

        SizeCategory.Category sizeCategory = DetermineSizeCategory(spec, rng);
        OrbitalProps orbital = GenerateOrbitalProps(spec, context, sizeCategory, rng);
        PhysicalProps physical = MoonPhysicalGenerator.GeneratePhysicalProps(spec, context, sizeCategory, orbital, rng);
        double equilibriumTempK = context.GetEquilibriumTemperatureK(0.3);
        double tidalHeatWatts = MoonPhysicalGenerator.CalculateTidalHeating(physical, orbital, context);

        physical = new PhysicalProps(
            physical.MassKg,
            physical.RadiusM,
            physical.RotationPeriodS,
            physical.AxialTiltDeg,
            physical.Oblateness,
            physical.MagneticMoment,
            physical.InternalHeatWatts + tidalHeatWatts);

        AtmosphereProps? atmosphere = null;
        if (MoonAtmosphereGenerator.ShouldHaveAtmosphere(spec, physical, sizeCategory, context, rng))
        {
            atmosphere = MoonAtmosphereGenerator.GenerateAtmosphere(
                spec,
                physical,
                sizeCategory,
                equilibriumTempK,
                rng);
        }

        double surfaceTempK = AtmosphereUtils.CalculateSurfaceTemperature(equilibriumTempK, atmosphere);
        SurfaceProps surface = MoonSurfaceGenerator.GenerateSurface(
            spec,
            physical,
            sizeCategory,
            surfaceTempK,
            tidalHeatWatts,
            context,
            rng);

        CelestialBody body = new(
            GenerateId(spec, rng),
            spec.NameHint,
            CelestialType.Type.Moon,
            physical,
            CreateProvenance(spec, context))
        {
            Orbital = orbital,
            Atmosphere = atmosphere,
            Surface = surface,
        };

        if (enablePopulation)
        {
            body.PopulationData = PopulationGenerator.GenerateAuto(
                body,
                context,
                spec.GenerationSeed,
                populationOverride,
                parentBody);
        }

        return body;
    }

    private static SizeCategory.Category DetermineSizeCategory(MoonSpec spec, SeededRng rng)
    {
        if (spec.HasSizeCategory())
        {
            return (SizeCategory.Category)spec.SizeCategory;
        }

        float[] weights = spec.IsCaptured ? SizeCategoryWeightsCaptured : SizeCategoryWeightsRegular;
        SizeCategory.Category? selected = rng.WeightedChoice(MoonSizes, weights);
        return selected ?? SizeCategory.Category.SubTerrestrial;
    }

    private static OrbitalProps GenerateOrbitalProps(
        MoonSpec spec,
        ParentContext context,
        SizeCategory.Category sizeCategory,
        SeededRng rng)
    {
        double hillRadiusM = context.GetHillSphereRadiusM();
        Dictionary densityRange = SizeTable.GetDensityRange(sizeCategory);
        double estimatedDensity = ((double)densityRange["min"] + (double)densityRange["max"]) / 2.0;
        double rocheLimitM = context.GetRocheLimitM(estimatedDensity);

        double minDistanceM = System.Math.Max(rocheLimitM * 1.5, context.ParentBodyRadiusM * 2.0);
        double overrideMin = spec.GetOverrideFloat("orbital.min_semi_major_axis_m", -1.0);
        if (overrideMin > 0.0)
        {
            minDistanceM = System.Math.Max(minDistanceM, overrideMin);
        }

        double maxFraction = spec.IsCaptured ? MaxHillFractionRetrograde : MaxHillFractionPrograde;
        double maxDistanceM = hillRadiusM * maxFraction;
        if (minDistanceM >= maxDistanceM)
        {
            minDistanceM = context.ParentBodyRadiusM * 3.0;
            maxDistanceM = context.ParentBodyRadiusM * 100.0;
        }

        double semiMajorAxisM = spec.GetOverrideFloat("orbital.semi_major_axis_m", -1.0);
        if (semiMajorAxisM < 0.0)
        {
            semiMajorAxisM = System.Math.Exp(rng.RandfRange(
                (float)System.Math.Log(minDistanceM),
                (float)System.Math.Log(maxDistanceM)));
        }

        double eccentricity = spec.GetOverrideFloat("orbital.eccentricity", -1.0);
        if (eccentricity < 0.0)
        {
            if (spec.IsCaptured)
            {
                eccentricity = rng.RandfRange(0.1f, 0.5f);
            }
            else
            {
                double raw = rng.Randf();
                eccentricity = raw * raw * 0.1;
            }
        }

        double inclinationDeg = spec.GetOverrideFloat("orbital.inclination_deg", -1.0);
        if (inclinationDeg < 0.0)
        {
            inclinationDeg = spec.IsCaptured
                ? rng.RandfRange(0.0f, 180.0f)
                : rng.RandfRange(0.0f, 5.0f);
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

    private static Provenance CreateProvenance(MoonSpec spec, ParentContext context)
    {
        Dictionary specSnapshot = spec.ToDictionary();
        specSnapshot["context"] = context.ToDictionary();
        return Provenance.CreateCurrent(spec.GenerationSeed, specSnapshot);
    }

    private static string GenerateId(MoonSpec spec, SeededRng rng)
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
        return GeneratorUtils.GenerateIdFromRandomPart("moon", randomPart);
    }
}
