using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators.Planet;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Generation.Utils;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Generates planet celestial bodies from planet specifications.
/// </summary>
public static class PlanetGenerator
{
    private static readonly float[] SizeCategoryWeights =
    {
        5.0f,
        10.0f,
        15.0f,
        20.0f,
        20.0f,
        15.0f,
        15.0f,
    };

    private static readonly float[] OrbitZoneWeights =
    {
        20.0f,
        30.0f,
        50.0f,
    };

    private static readonly SizeCategory.Category[] PlanetSizes =
    {
        SizeCategory.Category.Dwarf,
        SizeCategory.Category.SubTerrestrial,
        SizeCategory.Category.Terrestrial,
        SizeCategory.Category.SuperEarth,
        SizeCategory.Category.MiniNeptune,
        SizeCategory.Category.NeptuneClass,
        SizeCategory.Category.GasGiant,
    };

    private static readonly OrbitZone.Zone[] PlanetZones =
    {
        OrbitZone.Zone.Hot,
        OrbitZone.Zone.Temperate,
        OrbitZone.Zone.Cold,
    };

    /// <summary>
    /// Generates a planet from a specification and parent context.
    /// </summary>
    public static CelestialBody Generate(
        PlanetSpec spec,
        ParentContext context,
        SeededRng rng,
        bool enablePopulation = false,
        int populationOverride = 0)
    {
        SizeCategory.Category sizeCategory = DetermineSizeCategory(spec, rng);
        OrbitZone.Zone zone = DetermineOrbitZone(spec, rng);
        OrbitalProps orbital = GenerateOrbitalProps(spec, context, zone, rng);
        PhysicalProps physical = PlanetPhysicalGenerator.GeneratePhysicalProps(spec, context, sizeCategory, orbital, rng);
        double equilibriumTempK = context.GetEquilibriumTemperatureK(0.3);

        AtmosphereProps? atmosphere = null;
        if (PlanetAtmosphereGenerator.ShouldHaveAtmosphere(spec, physical, sizeCategory, context, rng))
        {
            atmosphere = PlanetAtmosphereGenerator.GenerateAtmosphere(
                spec,
                physical,
                sizeCategory,
                zone,
                equilibriumTempK,
                rng);
        }

        double surfaceTempK = AtmosphereUtils.CalculateSurfaceTemperature(equilibriumTempK, atmosphere);
        SurfaceProps? surface = null;
        if (SizeCategory.IsRocky(sizeCategory))
        {
            surface = PlanetSurfaceGenerator.GenerateSurface(
                spec,
                physical,
                sizeCategory,
                zone,
                surfaceTempK,
                context,
                rng);
        }

        RingSystemProps? ringSystem = null;
        if (ShouldGenerateRings(spec, sizeCategory) && RingSystemGenerator.ShouldHaveRings(physical, context, rng))
        {
            ringSystem = RingSystemGenerator.Generate(null, physical, context, rng);
        }

        CelestialBody body = new(
            GenerateId(spec, rng),
            spec.NameHint,
            CelestialType.Type.Planet,
            physical,
            CreateProvenance(spec, context))
        {
            Orbital = orbital,
            Atmosphere = atmosphere,
            Surface = surface,
            RingSystem = ringSystem,
        };

        if (enablePopulation)
        {
            body.PopulationData = PopulationGenerator.GenerateAuto(
                body,
                context,
                spec.GenerationSeed,
                populationOverride);
        }

        return body;
    }

    private static SizeCategory.Category DetermineSizeCategory(PlanetSpec spec, SeededRng rng)
    {
        if (spec.HasSizeCategory())
        {
            return (SizeCategory.Category)spec.SizeCategory;
        }

        SizeCategory.Category? selected = rng.WeightedChoice(PlanetSizes, SizeCategoryWeights);
        return selected ?? SizeCategory.Category.Terrestrial;
    }

    private static OrbitZone.Zone DetermineOrbitZone(PlanetSpec spec, SeededRng rng)
    {
        if (spec.HasOrbitZone())
        {
            return (OrbitZone.Zone)spec.OrbitZone;
        }

        OrbitZone.Zone? selected = rng.WeightedChoice(PlanetZones, OrbitZoneWeights);
        return selected ?? OrbitZone.Zone.Temperate;
    }

    private static OrbitalProps GenerateOrbitalProps(
        PlanetSpec spec,
        ParentContext context,
        OrbitZone.Zone zone,
        SeededRng rng)
    {
        double semiMajorAxisM = spec.GetOverrideFloat("orbital.semi_major_axis_m", -1.0);
        if (semiMajorAxisM < 0.0)
        {
            semiMajorAxisM = OrbitTable.RandomDistance(zone, context.StellarLuminosityWatts, rng);
        }

        double eccentricity = spec.GetOverrideFloat("orbital.eccentricity", -1.0);
        if (eccentricity < 0.0)
        {
            eccentricity = OrbitTable.RandomEccentricity(zone, rng);
        }

        double inclinationDeg = spec.GetOverrideFloat("orbital.inclination_deg", -1.0);
        if (inclinationDeg < 0.0)
        {
            inclinationDeg = OrbitTable.RandomInclination(rng);
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

    private static bool ShouldGenerateRings(PlanetSpec spec, SizeCategory.Category sizeCategory)
    {
        if (spec.HasOverride("has_rings"))
        {
            return (bool)spec.GetOverride("has_rings", false);
        }

        return SizeCategory.IsGaseous(sizeCategory);
    }

    private static Provenance CreateProvenance(PlanetSpec spec, ParentContext context)
    {
        Dictionary specSnapshot = spec.ToDictionary();
        specSnapshot["context"] = context.ToDictionary();
        return Provenance.CreateCurrent(spec.GenerationSeed, specSnapshot);
    }

    private static string GenerateId(PlanetSpec spec, SeededRng rng)
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
        return GeneratorUtils.GenerateIdFromRandomPart("planet", randomPart);
    }
}
