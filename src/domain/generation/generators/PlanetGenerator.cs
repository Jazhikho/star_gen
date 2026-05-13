using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators.Planet;
using StarGen.Domain.Generation.Science;
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
        DetermineResolvedPlanetProfile(spec, context, rng, out SizeCategory.Category sizeCategory, out OrbitZone.Zone zone);
        OrbitalProps orbital = GenerateOrbitalProps(spec, context, zone, rng);
        PhysicalProps physical = PlanetPhysicalGenerator.GeneratePhysicalProps(spec, context, sizeCategory, orbital, rng);
        RecordMassCorrectedHabitableZoneDiagnostic(spec, physical, orbital, context);
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
            RingSystemSpec ringSpec = new RingSystemSpec(spec.GenerationSeed, spec.RingComplexity);
            ringSystem = RingSystemGenerator.Generate(ringSpec, physical, context, rng);
        }

        CelestialBody body = new(
            GenerateId(spec, rng),
            spec.NameHint,
            CelestialType.Type.Planet,
            physical,
            CreateProvenance(spec, context))
        {
            Orbital = spec.OrbitMode == PlanetOrbitMode.Rogue ? null : orbital,
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
                populationOverride,
                null,
                spec.UseCaseSettings);
            CopyConceptPipelineState(body);
        }

        return body;
    }

    private static SizeCategory.Category DetermineSizeCategory(PlanetSpec spec, SeededRng rng)
    {
        if (spec.HasSizeCategory())
        {
            return (SizeCategory.Category)spec.SizeCategory;
        }

        if (spec.ClassBias == PlanetClassBias.GasGiant)
        {
            return SizeCategory.Category.GasGiant;
        }

        if (spec.ClassBias == PlanetClassBias.SubNeptune)
        {
            return SizeCategory.Category.MiniNeptune;
        }

        if (spec.ClassBias == PlanetClassBias.StrippedCore)
        {
            return SizeCategory.Category.SuperEarth;
        }

        if (spec.ClassBias == PlanetClassBias.Rocky)
        {
            return rng.Randf() < 0.6f ? SizeCategory.Category.Terrestrial : SizeCategory.Category.SuperEarth;
        }

        if (spec.ClassBias == PlanetClassBias.WaterRich)
        {
            return rng.Randf() < 0.55f ? SizeCategory.Category.Terrestrial : SizeCategory.Category.NeptuneClass;
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

        if (spec.OrbitMode == PlanetOrbitMode.Rogue)
        {
            return OrbitZone.Zone.Cold;
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
            semiMajorAxisM = spec.OrbitMode == PlanetOrbitMode.Rogue
                ? rng.RandfRange(300.0f, 1500.0f) * Units.AuMeters
                : OrbitTable.RandomDistance(zone, context.StellarLuminosityWatts, rng);
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

    private static void DetermineResolvedPlanetProfile(
        PlanetSpec spec,
        ParentContext context,
        SeededRng rng,
        out SizeCategory.Category sizeCategory,
        out OrbitZone.Zone zone)
    {
        ApplyDirectPlanetOverrides(spec);
        zone = DetermineOrbitZone(spec, rng);
        sizeCategory = DetermineSizeCategory(spec, rng);

        spec.FormationTrace["resolved_zone"] = zone.ToString();
        spec.FormationTrace["resolved_size_category"] = sizeCategory.ToString();
        spec.FormationTrace["orbit_mode"] = spec.OrbitMode.ToString();
        spec.FormationTrace["class_bias"] = spec.ClassBias.ToString();
        spec.FormationTrace["composition_bias"] = spec.CompositionBias.ToString();
        spec.FormationTrace["envelope_override"] = spec.EnvelopeOverride.ToString();
        spec.FormationTrace["volatile_richness"] = spec.VolatileRichness.ToString();
        spec.FormationTrace["hydrosphere_tendency"] = spec.HydrosphereTendency.ToString();
        spec.FormationTrace["context_luminosity_solar"] = context.StellarLuminosityWatts / StellarProps.SolarLuminosityWatts;
    }

    private static void ApplyDirectPlanetOverrides(PlanetSpec spec)
    {
        if (spec.CompositionBias == PlanetCompositionBias.IcyWaterRich)
        {
            if (!spec.HasOverride("surface.hydrosphere.ice_coverage"))
            {
                spec.SetOverride("surface.hydrosphere.ice_coverage", 0.55);
            }

            if (!spec.HasOverride("surface.hydrosphere.ocean_coverage"))
            {
                spec.SetOverride("surface.hydrosphere.ocean_coverage", 0.45);
            }
        }
        else if (spec.CompositionBias == PlanetCompositionBias.Rocky)
        {
            if (!spec.HasOverride("surface.hydrosphere.ocean_coverage"))
            {
                spec.SetOverride("surface.hydrosphere.ocean_coverage", 0.15);
            }
        }
        else if (spec.CompositionBias == PlanetCompositionBias.GasEnvelope)
        {
            spec.HasAtmosphere = true;
        }

        if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Thin && !spec.HasOverride("atmosphere.surface_pressure_pa"))
        {
            spec.SetOverride("atmosphere.surface_pressure_pa", 40000.0);
        }
        else if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Retained && !spec.HasOverride("atmosphere.surface_pressure_pa"))
        {
            spec.SetOverride("atmosphere.surface_pressure_pa", 350000.0);
            spec.HasAtmosphere = true;
        }
        else if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Stripped)
        {
            spec.HasAtmosphere = false;
            spec.SetOverride("atmosphere.surface_pressure_pa", 500.0);
        }

        if (spec.VolatileRichness == PlanetVolatileRichness.Poor)
        {
            if (!spec.HasOverride("surface.hydrosphere.ocean_coverage"))
            {
                spec.SetOverride("surface.hydrosphere.ocean_coverage", 0.05);
            }

            if (!spec.HasOverride("surface.hydrosphere.ice_coverage"))
            {
                spec.SetOverride("surface.hydrosphere.ice_coverage", 0.05);
            }
        }
        else if (spec.VolatileRichness == PlanetVolatileRichness.Rich)
        {
            if (!spec.HasOverride("surface.hydrosphere.ocean_coverage"))
            {
                spec.SetOverride("surface.hydrosphere.ocean_coverage", 0.65);
            }

            if (!spec.HasOverride("surface.hydrosphere.ice_coverage"))
            {
                spec.SetOverride("surface.hydrosphere.ice_coverage", 0.35);
            }
        }

        if (spec.HydrosphereTendency == PlanetHydrosphereTendency.Dry && !spec.HasOverride("surface.hydrosphere.ocean_coverage"))
        {
            spec.SetOverride("surface.hydrosphere.ocean_coverage", 0.02);
        }
        else if (spec.HydrosphereTendency == PlanetHydrosphereTendency.Mixed && !spec.HasOverride("surface.hydrosphere.ocean_coverage"))
        {
            spec.SetOverride("surface.hydrosphere.ocean_coverage", 0.35);
        }
        else if (spec.HydrosphereTendency == PlanetHydrosphereTendency.Oceanic && !spec.HasOverride("surface.hydrosphere.ocean_coverage"))
        {
            spec.SetOverride("surface.hydrosphere.ocean_coverage", 0.8);
        }
    }

    private static Provenance CreateProvenance(PlanetSpec spec, ParentContext context)
    {
        Dictionary specSnapshot = spec.ToDictionary();
        specSnapshot["context"] = context.ToDictionary();
        return Provenance.CreateCurrent(spec.GenerationSeed, specSnapshot);
    }

    private static void RecordMassCorrectedHabitableZoneDiagnostic(
        PlanetSpec spec,
        PhysicalProps physical,
        OrbitalProps orbital,
        ParentContext context)
    {
        if (context.StellarLuminosityWatts <= 0.0 || orbital.SemiMajorAxisM <= 0.0)
        {
            return;
        }

        PlanetHabitableZoneDiagnostic diagnostic = PlanetHabitableZoneDiagnosticEngine.EvaluateMassCorrectedKopparapu2014(
            physical,
            orbital,
            context);
        spec.FormationTrace["kopparapu2014_mass_corrected_hz"] = diagnostic.ToDictionary();
        spec.FormationTrace["kopparapu2014_mass_corrected_hz_inner_au"] = diagnostic.InnerAu;
        spec.FormationTrace["kopparapu2014_mass_corrected_hz_outer_au"] = diagnostic.OuterAu;
        spec.FormationTrace["kopparapu2014_mass_corrected_hz_alignment"] = diagnostic.Alignment;
        spec.FormationTrace["kopparapu2014_mass_corrected_hz_source_ids"] = diagnostic.SourceIds;
        spec.FormationTrace["kopparapu2014_mass_corrected_hz_status"] = diagnostic.Status;
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

    private static void CopyConceptPipelineState(CelestialBody body)
    {
        if (body.PopulationData == null)
        {
            return;
        }

        body.EnvironmentProfile = body.PopulationData.EnvironmentProfile;
        body.Ecology = body.PopulationData.EcologyState;
        body.SpeciesEvolution = body.PopulationData.SpeciesEvolution;
        body.Sentience = body.PopulationData.SentienceAssessment;
        body.Disease = body.PopulationData.DiseaseState;
    }
}
