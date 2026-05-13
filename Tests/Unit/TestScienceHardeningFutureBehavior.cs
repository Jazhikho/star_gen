#nullable enable annotations
#nullable disable warnings
using System.IO;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Science;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// F2P tests for the global science-hardening completion plan.
/// </summary>
public static class TestScienceHardeningFutureBehavior
{
    /// <summary>
    /// F2P: the global completion plan should enumerate all hardening families.
    /// </summary>
    public static void TestF2PCompletionPlanCoversRemainingHardeningFamilies()
    {
        string plan = ReadRepoFile(Path.Combine("Docs", "ScienceHardeningCompletionPlan.md"));

        AssertContains(plan, "Planet habitability");
        AssertContains(plan, "Orbital stability");
        AssertContains(plan, "Moon formation");
        AssertContains(plan, "Small bodies");
        AssertContains(plan, "Galaxy dynamics");
        AssertContains(plan, "Sentient population");
        AssertContains(plan, "Tradeoff");
        AssertContains(plan, "human review remains the final authority");
    }

    /// <summary>
    /// F2P: each remaining hardening domain should have a registered model engine family.
    /// </summary>
    public static void TestF2PEngineCatalogDefinesAlternativeFamilies()
    {
        AssertCatalogDomain("planet_habitability");
        AssertCatalogDomain("orbital_stability");
        AssertCatalogDomain("moon_formation");
        AssertCatalogDomain("small_body_population");
        AssertCatalogDomain("galaxy_dynamics");
        AssertCatalogDomain("sentient_population");

        ScienceHardeningEngineDescriptor stability = ScienceHardeningEngineCatalog.GetRequired("mutual_hill_amd_packing_stability_diagnostic");
        DotNetNativeTestSuite.AssertTrue(stability.AlternativeEngineIds.Length >= 3, "F2P expected orbital stability alternatives to be explicit");
        DotNetNativeTestSuite.AssertTrue(stability.DiagnosticOnlyByDefault, "F2P expected stability alternatives to remain diagnostic by default");

        ScienceHardeningEngineDescriptor sentient = ScienceHardeningEngineCatalog.GetRequired("human_audit_required_sentient_population_proxy");
        AssertContains(sentient.GameDesignTradeoff, "human-audit-required");
    }

    /// <summary>
    /// F2P: generated planets should record planet-mass-corrected Kopparapu 2014 HZ diagnostics.
    /// </summary>
    public static void TestF2PPlanetGenerationRecordsMassCorrectedHzDiagnostics()
    {
        PlanetSpec lowMassSpec = PlanetSpec.EarthLike(51013);
        lowMassSpec.SetOverride("physical.mass_earth", 0.20);
        lowMassSpec.SetOverride("orbital.semi_major_axis_m", Units.AuMeters);

        PlanetSpec highMassSpec = PlanetSpec.EarthLike(51014);
        highMassSpec.SetOverride("physical.mass_earth", 5.0);
        highMassSpec.SetOverride("orbital.semi_major_axis_m", Units.AuMeters);

        ParentContext context = ParentContext.SunLike(Units.AuMeters);
        CelestialBody lowMassPlanet = PlanetGenerator.Generate(lowMassSpec, context, new SeededRng(51013), enablePopulation: false);
        CelestialBody highMassPlanet = PlanetGenerator.Generate(highMassSpec, context, new SeededRng(51014), enablePopulation: false);

        Dictionary lowMassTrace = GetFormationTrace(lowMassPlanet);
        Dictionary highMassTrace = GetFormationTrace(highMassPlanet);

        AssertTraceKey(lowMassTrace, "kopparapu2014_mass_corrected_hz_inner_au");
        AssertTraceKey(lowMassTrace, "kopparapu2014_mass_corrected_hz_outer_au");
        AssertTraceKey(lowMassTrace, "kopparapu2014_mass_corrected_hz_alignment");
        AssertTraceKey(lowMassTrace, "kopparapu2014_mass_corrected_hz_source_ids");

        double lowMassInner = lowMassTrace["kopparapu2014_mass_corrected_hz_inner_au"].AsDouble();
        double highMassInner = highMassTrace["kopparapu2014_mass_corrected_hz_inner_au"].AsDouble();
        DotNetNativeTestSuite.AssertTrue(highMassInner < lowMassInner, "F2P expected higher-mass rocky planets to carry a closer diagnostic inner HZ edge");
    }

    /// <summary>
    /// F2P: orbit slots should serialize alternative stability diagnostics.
    /// </summary>
    public static void TestF2POrbitSlotsSerializeAlternativeStabilityDiagnostics()
    {
        OrbitHost host = new("host_primary", OrbitHost.HostType.SType)
        {
            CombinedMassKg = Units.SolarMassKg,
            CombinedLuminosityWatts = StarGen.Domain.Celestial.Components.StellarProps.SolarLuminosityWatts,
            EffectiveTemperatureK = 5778.0,
            InnerStabilityM = 0.20 * Units.AuMeters,
            OuterStabilityM = 5.0 * Units.AuMeters,
            HabitableZoneInnerM = 0.95 * Units.AuMeters,
            HabitableZoneOuterM = 1.37 * Units.AuMeters,
            FrostLineM = 2.7 * Units.AuMeters,
        };

        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            new SeededRng(61101));
        DotNetNativeTestSuite.AssertTrue(result.Slots.Count >= 2, "F2P expected fixture host to generate adjacent slots");

        OrbitSlot secondSlot = result.Slots[1];
        Dictionary data = secondSlot.ToDictionary();
        AssertTraceKey(data, "stability_alternative_engine_ids");
        AssertTraceKey(data, "stability_alternative_source_ids");
        AssertTraceKey(data, "amd_instability_risk");
        AssertTraceKey(data, "dynamical_packing_risk");
        AssertTraceKey(data, "resonance_proximity_score");

        DotNetNativeTestSuite.AssertTrue(secondSlot.StabilityAlternativeEngineIds.Contains("amd_screen_diagnostic"), "F2P expected AMD diagnostic engine to be recorded");
    }

    /// <summary>
    /// F2P: planet formation traces should copy stability diagnostics from their filled slots.
    /// </summary>
    public static void TestF2PPlanetFormationTraceCopiesStabilityDiagnostics()
    {
        Array<OrbitSlot> slots = new();
        OrbitSlot slot = new("slot_fixture_0", "host_primary", Units.AuMeters)
        {
            FillProbability = 1.0,
            Zone = OrbitZone.Zone.Temperate,
            SpacingFromInnerMutualHillRadii = 7.5,
            PeriodRatioFromInner = 1.5,
            AmdInstabilityRisk = 0.25,
            DynamicalPackingRisk = 0.35,
            ResonanceProximityScore = 0.85,
        };
        slots.Add(slot);

        Array<OrbitHost> hosts = new();
        hosts.Add(new OrbitHost("host_primary", OrbitHost.HostType.SType)
        {
            CombinedMassKg = Units.SolarMassKg,
            CombinedLuminosityWatts = StarGen.Domain.Celestial.Components.StellarProps.SolarLuminosityWatts,
            EffectiveTemperatureK = 5778.0,
            HabitableZoneInnerM = 0.95 * Units.AuMeters,
            HabitableZoneOuterM = 1.37 * Units.AuMeters,
            FrostLineM = 2.7 * Units.AuMeters,
            InnerStabilityM = 0.1 * Units.AuMeters,
            OuterStabilityM = 5.0 * Units.AuMeters,
        });

        Array<CelestialBody> stars = new();
        stars.Add(StarGenerator.Generate(StarSpec.SunLike(61102), new SeededRng(61102)));

        PlanetGenerationResult result = SystemPlanetGenerator.GenerateTargeted(
            slots,
            hosts,
            stars,
            1,
            new SeededRng(61103),
            enablePopulation: false);
        DotNetNativeTestSuite.AssertEqual(1, result.Planets.Count, "F2P expected fixture generation to fill one slot");

        Dictionary trace = GetFormationTrace(result.Planets[0]);
        AssertTraceKey(trace, "slot_stability_alternative_engine_ids");
        AssertTraceKey(trace, "slot_amd_instability_risk");
        AssertTraceKey(trace, "slot_dynamical_packing_risk");
        AssertTraceKey(trace, "slot_resonance_proximity_score");
        DotNetNativeTestSuite.AssertEqual(0.25, trace["slot_amd_instability_risk"].AsDouble(), "F2P expected planet trace to preserve slot AMD diagnostic");
    }

    /// <summary>
    /// F2P: terrestrial moon candidates should no longer use an empty source placeholder.
    /// </summary>
    public static void TestF2PMoonTraceMarksTerrestrialImpactChannelSources()
    {
        string source = ReadRepoFile(Path.Combine("src", "domain", "system", "SystemMoonGenerator.cs"));

        AssertContains(source, "terrestrial_giant_impact_candidate");
        AssertContains(source, "MalamudPerets2019;NakajimaEtAl2022");
        AssertContains(source, "diagnostic-only terrestrial impact candidate");
    }

    private static void AssertCatalogDomain(string domain)
    {
        DotNetNativeTestSuite.AssertTrue(ScienceHardeningEngineCatalog.HasDomain(domain), $"F2P expected catalog domain {domain}");
    }

    private static void AssertContains(string text, string fragment)
    {
        DotNetNativeTestSuite.AssertTrue(text.Contains(fragment), $"F2P expected text to contain '{fragment}'");
    }

    private static void AssertTraceKey(Dictionary data, string key)
    {
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey(key), $"F2P expected dictionary to include {key}");
    }

    private static Dictionary GetFormationTrace(CelestialBody body)
    {
        Dictionary specSnapshot = body.Provenance.SpecSnapshot;
        DotNetNativeTestSuite.AssertTrue(specSnapshot.ContainsKey("formation_trace"), "F2P expected provenance spec snapshot to include formation_trace");
        return (Dictionary)specSnapshot["formation_trace"];
    }

    private static string ReadRepoFile(string path)
    {
        DotNetNativeTestSuite.AssertTrue(File.Exists(path), $"F2P expected file to exist: {path}");
        return File.ReadAllText(path);
    }
}
