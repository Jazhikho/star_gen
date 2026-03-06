#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Verifies that star generation is deterministic for the same spec and RNG seed.
    /// </summary>
    private static void TestStarGeneratorDeterministicSameSeed()
    {
        StarSpec spec = StarSpec.SunLike(101_001);
        CelestialBody first = StarGenerator.Generate(spec, new SeededRng(202_002));
        CelestialBody second = StarGenerator.Generate(spec, new SeededRng(202_002));

        string firstJson = CelestialSerializer.ToJson(first, pretty: false);
        string secondJson = CelestialSerializer.ToJson(second, pretty: false);

        AssertEqual(firstJson, secondJson, "same star spec and seed should serialize identically");
    }

    /// <summary>
    /// Verifies that planet generation is deterministic for the same spec and RNG seed.
    /// </summary>
    private static void TestPlanetGeneratorDeterministicSameSeed()
    {
        PlanetSpec spec = PlanetSpec.EarthLike(212_121);
        ParentContext context = CreateFixturePlanetContext();

        CelestialBody first = PlanetGenerator.Generate(spec, context, new SeededRng(313_131), enablePopulation: false);
        CelestialBody second = PlanetGenerator.Generate(spec, context, new SeededRng(313_131), enablePopulation: false);

        Godot.Collections.Dictionary firstData = CelestialSerializer.ToDictionary(first);
        Godot.Collections.Dictionary secondData = CelestialSerializer.ToDictionary(second);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same planet spec and seed should remain semantically identical");
    }

    /// <summary>
    /// Verifies that moon generation is deterministic for the same spec and RNG seed.
    /// </summary>
    private static void TestMoonGeneratorDeterministicSameSeed()
    {
        MoonSpec spec = MoonSpec.TitanLike(414_141);
        ParentContext context = CreateFixtureMoonContext();

        CelestialBody? first = MoonGenerator.Generate(spec, context, new SeededRng(515_151), enablePopulation: false);
        CelestialBody? second = MoonGenerator.Generate(spec, context, new SeededRng(515_151), enablePopulation: false);

        AssertNotNull(first, "fixture moon spec should generate a moon");
        AssertNotNull(second, "fixture moon spec should generate a moon");

        Godot.Collections.Dictionary firstData = CelestialSerializer.ToDictionary(first!);
        Godot.Collections.Dictionary secondData = CelestialSerializer.ToDictionary(second!);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same moon spec and seed should remain semantically identical");
    }

    /// <summary>
    /// Verifies that C# celestial serialization round-trips a generated star without drift.
    /// </summary>
    private static void TestCelestialSerializerRoundTripPreservesStarPayload()
    {
        StarSpec spec = StarSpec.RedDwarf(303_003);
        CelestialBody original = StarGenerator.Generate(spec, new SeededRng(404_004));
        Godot.Collections.Dictionary originalData = CelestialSerializer.ToDictionary(original);
        string originalJson = Json.Stringify(originalData);

        CelestialBody? rebuilt = CelestialSerializer.FromJson(originalJson);
        AssertNotNull(rebuilt, "round-trip should rebuild a celestial body");

        Godot.Collections.Dictionary rebuiltData = CelestialSerializer.ToDictionary(rebuilt!);
        AssertVariantDeepEqual(
            originalData,
            rebuiltData,
            "round-tripped star payload should remain semantically unchanged");
    }

    /// <summary>
    /// Verifies that ring generation returns a non-null system with at least one band.
    /// </summary>
    private static void TestRingSystemGeneratorReturnsRingSystem()
    {
        RingSystemSpec spec = RingSystemSpec.Random(12345);
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, physical, context, new SeededRng(12345));
        AssertNotNull(rings, "Should return a RingSystemProps");
        AssertTrue(rings!.GetBandCount() > 0, "Should have at least one band");
    }

    /// <summary>
    /// Verifies that ring generation is deterministic for the same inputs and RNG seed.
    /// </summary>
    private static void TestRingSystemGeneratorDeterministicSameSeed()
    {
        RingSystemSpec spec = RingSystemSpec.Complex(505_005);
        PhysicalProps planetPhysical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();

        RingSystemProps? first = RingSystemGenerator.Generate(spec, planetPhysical, context, new SeededRng(606_006));
        RingSystemProps? second = RingSystemGenerator.Generate(spec, planetPhysical, context, new SeededRng(606_006));

        AssertNotNull(first, "ring generation should produce a system for the fixture inputs");
        AssertNotNull(second, "ring generation should produce a system for the fixture inputs");

        string firstJson = Json.Stringify(first!.ToDictionary());
        string secondJson = Json.Stringify(second!.ToDictionary());
        AssertEqual(firstJson, secondJson, "same ring inputs and seed should serialize identically");
    }

    /// <summary>
    /// Verifies that trace complexity produces one band with low optical depth.
    /// </summary>
    private static void TestRingSystemGeneratorTraceComplexity()
    {
        RingSystemSpec spec = RingSystemSpec.Trace(11111);
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(11111));
        AssertNotNull(rings, "Should generate trace rings");
        AssertEqual(rings!.GetBandCount(), 1, "Trace rings should have 1 band");
        RingBand? band = rings.GetBand(0);
        AssertNotNull(band, "first band should exist");
        AssertTrue(band!.OpticalDepth < 0.2, "Trace rings should have low optical depth");
    }

    /// <summary>
    /// Verifies that simple complexity produces 2 to 3 bands.
    /// </summary>
    private static void TestRingSystemGeneratorSimpleComplexity()
    {
        RingSystemSpec spec = RingSystemSpec.Simple(22222);
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(22222));
        AssertNotNull(rings, "Should generate simple rings");
        int count = rings!.GetBandCount();
        AssertTrue(count >= 2, "Simple rings should have at least 2 bands");
        AssertTrue(count <= 3, "Simple rings should have at most 3 bands");
    }

    /// <summary>
    /// Verifies that complex complexity produces 4 to 7 bands.
    /// </summary>
    private static void TestRingSystemGeneratorComplexComplexity()
    {
        RingSystemSpec spec = RingSystemSpec.Complex(33333);
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(33333));
        AssertNotNull(rings, "Should generate complex rings");
        int count = rings!.GetBandCount();
        AssertTrue(count >= 4, "Complex rings should have at least 4 bands");
        AssertTrue(count <= 7, "Complex rings should have at most 7 bands");
    }

    /// <summary>
    /// Verifies that rings beyond the ice line are predominantly icy.
    /// </summary>
    private static void TestRingSystemGeneratorIcyCompositionBeyondIceLine()
    {
        RingSystemSpec spec = RingSystemSpec.Random(44444);
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(44444));
        AssertNotNull(rings, "Should generate rings");
        RingBand? firstBand = rings!.GetBand(0);
        AssertNotNull(firstBand, "first band should exist");
        AssertTrue(firstBand!.Composition.ContainsKey("water_ice"), "Should have water ice beyond ice line");
        double waterIce = (double)firstBand.Composition["water_ice"];
        AssertTrue(waterIce > 0.5, "Should be predominantly icy");
    }

    /// <summary>
    /// Verifies that rings inside the ice line are predominantly rocky.
    /// </summary>
    private static void TestRingSystemGeneratorRockyCompositionInsideIceLine()
    {
        RingSystemSpec spec = RingSystemSpec.Random(55555);
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, CreateSaturnPhysical(), CreateInnerContext(), new SeededRng(55555));
        AssertNotNull(rings, "Should generate rings");
        RingBand? firstBand = rings!.GetBand(0);
        AssertNotNull(firstBand, "first band should exist");
        AssertTrue(firstBand!.Composition.ContainsKey("silicates"), "Should have silicates inside ice line");
        double silicates = (double)firstBand.Composition["silicates"];
        AssertTrue(silicates > 0.5, "Should be predominantly rocky");
    }

    /// <summary>
    /// Verifies that forced icy composition overrides zone.
    /// </summary>
    private static void TestRingSystemGeneratorForcedIcyComposition()
    {
        RingSystemSpec spec = RingSystemSpec.Icy(66666);
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, CreateSaturnPhysical(), CreateInnerContext(), new SeededRng(66666));
        AssertNotNull(rings, "Should generate rings");
        RingBand? firstBand = rings!.GetBand(0);
        AssertNotNull(firstBand, "first band should exist");
        AssertTrue(firstBand!.Composition.ContainsKey("water_ice"), "Should have water ice when forced");
    }

    /// <summary>
    /// Verifies that forced rocky composition overrides zone.
    /// </summary>
    private static void TestRingSystemGeneratorForcedRockyComposition()
    {
        RingSystemSpec spec = RingSystemSpec.Rocky(77777);
        RingSystemProps? rings = RingSystemGenerator.Generate(spec, CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(77777));
        AssertNotNull(rings, "Should generate rings");
        RingBand? firstBand = rings!.GetBand(0);
        AssertNotNull(firstBand, "first band should exist");
        AssertTrue(firstBand!.Composition.ContainsKey("silicates"), "Should have silicates when forced rocky");
    }

    /// <summary>
    /// Verifies that ring inner radius is outside planet radius.
    /// </summary>
    private static void TestRingSystemGeneratorRingsOutsidePlanetRadius()
    {
        PhysicalProps physical = CreateSaturnPhysical();
        RingSystemProps? rings = RingSystemGenerator.Generate(RingSystemSpec.Random(88888), physical, CreateSaturnContext(), new SeededRng(88888));
        AssertNotNull(rings, "Should generate rings");
        double innerRadius = rings!.GetInnerRadiusM();
        AssertTrue(innerRadius > physical.RadiusM, "Rings should be outside planet radius");
    }

    /// <summary>
    /// Verifies that bands are ordered by radius and do not overlap.
    /// </summary>
    private static void TestRingSystemGeneratorBandsOrderedByRadius()
    {
        RingSystemProps? rings = RingSystemGenerator.Generate(RingSystemSpec.Complex(99999), CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(99999));
        AssertNotNull(rings, "Should generate rings");
        for (int i = 0; i < rings!.GetBandCount(); i++)
        {
            RingBand? band = rings.GetBand(i);
            AssertNotNull(band, "band should exist");
            AssertTrue(band!.OuterRadiusM > band.InnerRadiusM, "Band outer > inner");
            if (i > 0)
            {
                RingBand? prevBand = rings.GetBand(i - 1);
                AssertNotNull(prevBand, "previous band should exist");
                AssertTrue(band.InnerRadiusM >= prevBand!.OuterRadiusM, "Bands should not overlap");
            }
        }
    }

    /// <summary>
    /// Verifies that each band has valid positive properties and non-empty composition.
    /// </summary>
    private static void TestRingSystemGeneratorBandPropertiesValid()
    {
        RingSystemProps? rings = RingSystemGenerator.Generate(RingSystemSpec.Complex(10101), CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(10101));
        AssertNotNull(rings, "Should generate rings");
        for (int i = 0; i < rings!.GetBandCount(); i++)
        {
            RingBand? band = rings.GetBand(i);
            AssertNotNull(band, "band should exist");
            AssertTrue(band!.InnerRadiusM > 0.0, "Inner radius should be positive");
            AssertTrue(band.OuterRadiusM > 0.0, "Outer radius should be positive");
            AssertTrue(band.OpticalDepth > 0.0, "Optical depth should be positive");
            AssertTrue(band.ParticleSizeM > 0.0, "Particle size should be positive");
            AssertTrue(band.Composition.Count > 0, "Composition should not be empty");
        }
    }

    /// <summary>
    /// Verifies that total mass is positive.
    /// </summary>
    private static void TestRingSystemGeneratorTotalMassPositive()
    {
        RingSystemProps? rings = RingSystemGenerator.Generate(RingSystemSpec.Random(20202), CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(20202));
        AssertNotNull(rings, "Should generate rings");
        AssertTrue(rings!.TotalMassKg > 0.0, "Total mass should be positive");
    }

    /// <summary>
    /// Verifies that inclination is small and non-negative.
    /// </summary>
    private static void TestRingSystemGeneratorInclinationSmall()
    {
        RingSystemProps? rings = RingSystemGenerator.Generate(RingSystemSpec.Random(30303), CreateSaturnPhysical(), CreateSaturnContext(), new SeededRng(30303));
        AssertNotNull(rings, "Should generate rings");
        AssertTrue(rings!.InclinationDeg >= 0.0, "Inclination should be non-negative");
        AssertTrue(rings.InclinationDeg < 10.0, "Inclination should be small (aligned with equator)");
    }

    /// <summary>
    /// Verifies that gas giants get rings often (probability check over 20 seeds).
    /// </summary>
    private static void TestRingSystemGeneratorShouldHaveRingsGasGiant()
    {
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        int hasRingsCount = 0;
        for (int i = 0; i < 20; i++)
        {
            if (RingSystemGenerator.ShouldHaveRings(physical, context, new SeededRng(40404 + i)))
            {
                hasRingsCount++;
            }
        }
        AssertTrue(hasRingsCount > 5, "Gas giants should often have rings");
    }

    /// <summary>
    /// Verifies that terrestrial planets rarely get rings (probability check over 100 seeds).
    /// </summary>
    private static void TestRingSystemGeneratorShouldHaveRingsTerrestrialRare()
    {
        PhysicalProps physical = new PhysicalProps(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            86400.0,
            23.5,
            0.003,
            8.0e22,
            4.7e13);
        ParentContext context = ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters);
        int hasRingsCount = 0;
        for (int i = 0; i < 100; i++)
        {
            if (RingSystemGenerator.ShouldHaveRings(physical, context, new SeededRng(50505 + i)))
            {
                hasRingsCount++;
            }
        }
        AssertTrue(hasRingsCount < 10, "Terrestrial planets should rarely have rings");
    }

    /// <summary>
    /// Verifies that different seeds produce different ring systems.
    /// </summary>
    private static void TestRingSystemGeneratorDifferentSeedsProduceDifferentRings()
    {
        PhysicalProps physical = CreateSaturnPhysical();
        ParentContext context = CreateSaturnContext();
        RingSystemProps? rings1 = RingSystemGenerator.Generate(RingSystemSpec.Random(11111), physical, context, new SeededRng(11111));
        RingSystemProps? rings2 = RingSystemGenerator.Generate(RingSystemSpec.Random(22222), physical, context, new SeededRng(22222));
        AssertNotNull(rings1, "Should generate rings 1");
        AssertNotNull(rings2, "Should generate rings 2");
        bool differ = rings1!.GetBandCount() != rings2!.GetBandCount()
            || Math.Abs(rings1.TotalMassKg - rings2.TotalMassKg) > 1e-6
            || Math.Abs(rings1.GetInnerRadiusM() - rings2.GetInnerRadiusM()) > 1e-6;
        AssertTrue(differ, "Different seeds should produce different rings");
    }

    /// <summary>
    /// Verifies that asteroid generation is deterministic for the same inputs and RNG seed.
    /// </summary>
    private static void TestAsteroidGeneratorDeterministicSameSeed()
    {
        AsteroidSpec spec = AsteroidSpec.CeresLike(707_007);
        ParentContext context = ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters * 2.7);

        CelestialBody first = AsteroidGenerator.Generate(spec, context, new SeededRng(808_008));
        CelestialBody second = AsteroidGenerator.Generate(spec, context, new SeededRng(808_008));

        Godot.Collections.Dictionary firstData = CelestialSerializer.ToDictionary(first);
        Godot.Collections.Dictionary secondData = CelestialSerializer.ToDictionary(second);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        AssertVariantDeepEqual(
            firstData,
            secondData,
            "same asteroid inputs and seed should remain semantically identical");
    }

    /// <summary>
    /// Verifies that a generated system can round-trip through the C# system serializer.
    /// </summary>
    private static void TestSystemSerializerRoundTripPreservesSystemPayload()
    {
        GalaxyStar star = CreateFixtureGalaxyStar();
        SolarSystem? original = GalaxySystemGenerator.GenerateSystem(star, includeAsteroids: true, enablePopulation: false);
        AssertNotNull(original, "fixture galaxy star should generate a system");

        Godot.Collections.Dictionary originalData = SystemSerializer.ToDictionary(original!);
        NormalizeTransientFields(originalData);
        string originalJson = Json.Stringify(originalData);

        SolarSystem? rebuilt = SystemSerializer.FromJson(originalJson);
        AssertNotNull(rebuilt, "system round-trip should rebuild a solar system");

        Godot.Collections.Dictionary rebuiltData = SystemSerializer.ToDictionary(rebuilt!);
        NormalizeTransientFields(rebuiltData);

        AssertVariantDeepEqual(
            originalData,
            rebuiltData,
            "round-tripped system payload should remain semantically unchanged");
    }
}
