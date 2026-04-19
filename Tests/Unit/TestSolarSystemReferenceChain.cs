#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Staged regression that proves the live generation pipeline can still be
/// steered into a Solar-System-like outcome without adding production hacks.
/// </summary>
public static class TestSolarSystemReferenceChain
{
    private const int SolarReferenceSeed = 8_640_221;
    private const double SolarAgeYears = 4.57e9;
    private const double SolarMetallicity = 1.0;

    private static readonly SolarPlanetExpectation[] SolarPlanets =
    {
        new SolarPlanetExpectation(
            "Mercury",
            0.387,
            0.18,
            SizeCategory.Category.SubTerrestrial,
            PlanetCompositionBias.Rocky,
            PlanetEnvelopeOverride.Stripped,
            PlanetVolatileRichness.Poor,
            PlanetHydrosphereTendency.Dry,
            PlanetClassBias.Rocky,
            0.02,
            0.20,
            0.20,
            0.65,
            false,
            false,
            0.055,
            0.383,
            false,
            0.0,
            0.0,
            0.0),
        new SolarPlanetExpectation(
            "Venus",
            0.723,
            0.22,
            SizeCategory.Category.Terrestrial,
            PlanetCompositionBias.Rocky,
            PlanetEnvelopeOverride.Retained,
            PlanetVolatileRichness.Poor,
            PlanetHydrosphereTendency.Dry,
            PlanetClassBias.Rocky,
            0.30,
            2.00,
            0.60,
            1.50,
            true,
            false,
            0.815,
            0.949,
            true,
            9.2e6,
            0.0,
            0.0),
        new SolarPlanetExpectation(
            "Earth",
            1.000,
            0.22,
            SizeCategory.Category.Terrestrial,
            PlanetCompositionBias.Rocky,
            PlanetEnvelopeOverride.Retained,
            PlanetVolatileRichness.Moderate,
            PlanetHydrosphereTendency.Mixed,
            PlanetClassBias.Rocky,
            0.30,
            2.00,
            0.60,
            1.50,
            true,
            true,
            1.000,
            1.000,
            true,
            1.01325e5,
            0.71,
            0.08),
        new SolarPlanetExpectation(
            "Mars",
            1.524,
            0.70,
            SizeCategory.Category.SubTerrestrial,
            PlanetCompositionBias.Rocky,
            PlanetEnvelopeOverride.Thin,
            PlanetVolatileRichness.Poor,
            PlanetHydrosphereTendency.Dry,
            PlanetClassBias.Rocky,
            0.02,
            0.35,
            0.20,
            0.75,
            false,
            false,
            0.107,
            0.532,
            true,
            610.0,
            0.0,
            0.20),
        new SolarPlanetExpectation(
            "Jupiter",
            5.203,
            1.50,
            SizeCategory.Category.GasGiant,
            PlanetCompositionBias.GasEnvelope,
            PlanetEnvelopeOverride.Retained,
            PlanetVolatileRichness.Rich,
            PlanetHydrosphereTendency.Auto,
            PlanetClassBias.GasGiant,
            80.0,
            4000.0,
            6.0,
            15.0,
            true,
            false,
            317.8,
            11.21,
            true,
            -1.0,
            -1.0,
            -1.0),
        new SolarPlanetExpectation(
            "Saturn",
            9.537,
            2.60,
            SizeCategory.Category.GasGiant,
            PlanetCompositionBias.GasEnvelope,
            PlanetEnvelopeOverride.Retained,
            PlanetVolatileRichness.Rich,
            PlanetHydrosphereTendency.Auto,
            PlanetClassBias.GasGiant,
            80.0,
            4000.0,
            6.0,
            15.0,
            true,
            false,
            95.16,
            9.45,
            true,
            -1.0,
            -1.0,
            -1.0),
        new SolarPlanetExpectation(
            "Uranus",
            19.19,
            6.00,
            SizeCategory.Category.MiniNeptune,
            PlanetCompositionBias.GasEnvelope,
            PlanetEnvelopeOverride.Retained,
            PlanetVolatileRichness.Rich,
            PlanetHydrosphereTendency.Auto,
            PlanetClassBias.SubNeptune,
            10.0,
            25.0,
            2.0,
            4.5,
            true,
            false,
            14.54,
            4.01,
            true,
            -1.0,
            -1.0,
            -1.0),
        new SolarPlanetExpectation(
            "Neptune",
            30.07,
            8.00,
            SizeCategory.Category.MiniNeptune,
            PlanetCompositionBias.GasEnvelope,
            PlanetEnvelopeOverride.Retained,
            PlanetVolatileRichness.Rich,
            PlanetHydrosphereTendency.Auto,
            PlanetClassBias.SubNeptune,
            10.0,
            25.0,
            2.0,
            4.5,
            true,
            false,
            17.15,
            3.88,
            true,
            -1.0,
            -1.0,
            -1.0),
    };

    /// <summary>
    /// Verifies the staged Solar reference chain still resolves through the live generators.
    /// </summary>
    public static void TestSolarReferenceChainBuildsExpectedSystem()
    {
        SolarSystem system = GenerateSolarReferenceSystem();
        ValidationResult validation = SystemValidator.Validate(system);
        if (!validation.IsValid())
        {
            throw new InvalidOperationException($"Solar reference chain failed at final validation: system validator reported {validation.GetErrorCount()} errors.");
        }

        if (system.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Solar reference chain failed at final system stage: expected exactly one star.");
        }

        if (system.GetPlanetCount() != SolarPlanets.Length)
        {
            throw new InvalidOperationException($"Solar reference chain failed at final system stage: expected {SolarPlanets.Length} planets but found {system.GetPlanetCount()}.");
        }

        Array<CelestialBody> planets = system.GetPlanets();
        SystemPlanetGenerator.SortByDistance(planets);
        for (int index = 0; index < SolarPlanets.Length; index += 1)
        {
            CelestialBody planet = planets[index];
            SolarPlanetExpectation expectation = SolarPlanets[index];
            if (planet.Name != expectation.Name)
            {
                throw new InvalidOperationException($"Solar reference chain failed at final order stage for slot {index}: expected {expectation.Name} but found {planet.Name}.");
            }

            if (!planet.HasOrbital())
            {
                throw new InvalidOperationException($"Solar reference chain failed at final orbit stage for {expectation.Name}: locked planet lost its orbital data.");
            }

            double actualAu = planet.Orbital!.SemiMajorAxisM / Units.AuMeters;
            double orbitDifference = System.Math.Abs(actualAu - expectation.TargetSemiMajorAxisAu);
            if (orbitDifference > 1.0e-6)
            {
                throw new InvalidOperationException($"Solar reference chain failed at final orbit stage for {expectation.Name}: expected exact orbit {expectation.TargetSemiMajorAxisAu:0.000} AU but found {actualAu:0.000000} AU.");
            }
        }
    }

    private static SolarSystem GenerateSolarReferenceSystem()
    {
        GenerateCandidateSun();
        CelestialBody lockedSun = GenerateLockedSun();
        OrbitHost solarHost = CreateSolarHost();
        System.Collections.Generic.Dictionary<string, OrbitSlot> matchedSlots = FindSolarSlots(lockedSun.Physical.RadiusM, solarHost);

        SolarSystem system = new SolarSystem("solar_reference_chain", "Solar Reference");
        HierarchyNode starNode = HierarchyNode.CreateStar("sol_host", "sol");
        system.Hierarchy = new SystemHierarchy(starNode);
        system.AddBody(lockedSun);
        system.AddOrbitHost(solarHost);

        for (int index = 0; index < SolarPlanets.Length; index += 1)
        {
            SolarPlanetExpectation expectation = SolarPlanets[index];
            OrbitSlot matchedSlot = matchedSlots[expectation.Name];
            GenerateCandidatePlanet(expectation, matchedSlot, solarHost, index);
            CelestialBody lockedPlanet = GenerateLockedPlanet(expectation, solarHost, index);
            if (!lockedPlanet.HasOrbital())
            {
                throw new InvalidOperationException($"Solar reference chain failed at final lock stage for {expectation.Name}: exact planet lost its orbital data.");
            }

            lockedPlanet.Orbital!.ParentId = solarHost.NodeId;
            system.AddBody(lockedPlanet);
        }

        return system;
    }

    private static CelestialBody GenerateCandidateSun()
    {
        string lastFailure = "no Sun-like candidate was evaluated";
        for (int attempt = 0; attempt < 32; attempt += 1)
        {
            int seed = SolarReferenceSeed + attempt;
            StarSpec spec = StarSpec.SunLike(seed);
            spec.NameHint = "Candidate Sun";
            spec.AgeYears = SolarAgeYears;
            spec.Metallicity = SolarMetallicity;

            CelestialBody star = StarGenerator.Generate(spec, new SeededRng(seed));
            string? failure = GetCandidateSunFailure(star);
            if (failure == null)
            {
                return star;
            }

            lastFailure = failure;
        }

        throw new InvalidOperationException($"Solar reference chain failed at stellar preflight after 32 attempts: {lastFailure}");
    }

    private static CelestialBody GenerateLockedSun()
    {
        StarSpec spec = StarSpec.SunLike(SolarReferenceSeed + 1);
        spec.NameHint = "Sun";
        spec.AgeYears = SolarAgeYears;
        spec.Metallicity = SolarMetallicity;
        spec.SetOverride("id", "sol");
        spec.SetOverride("physical.mass_solar", 1.0);
        spec.SetOverride("physical.radius_solar", 1.0);
        spec.SetOverride("stellar.luminosity_solar", 1.0);
        spec.SetOverride("stellar.temperature_k", 5778.0);

        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(SolarReferenceSeed + 1));
        if (!star.HasStellar())
        {
            throw new InvalidOperationException("Solar reference chain failed at stellar lock stage: exact Sun is missing stellar data.");
        }

        double massDifference = System.Math.Abs((star.Physical.MassKg / Units.SolarMassKg) - 1.0);
        double radiusDifference = System.Math.Abs((star.Physical.RadiusM / Units.SolarRadiusMeters) - 1.0);
        double luminosityDifference = System.Math.Abs(star.Stellar!.GetLuminositySolar() - 1.0);
        if (massDifference > 1.0e-9 || radiusDifference > 1.0e-9 || luminosityDifference > 1.0e-9)
        {
            throw new InvalidOperationException("Solar reference chain failed at stellar lock stage: exact Sun overrides were not honored.");
        }

        return star;
    }

    private static OrbitHost CreateSolarHost()
    {
        OrbitHost host = new OrbitHost("sol_host", OrbitHost.HostType.SType)
        {
            CombinedMassKg = Units.SolarMassKg,
            CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts,
            EffectiveTemperatureK = 5778.0,
            InnerStabilityM = 0.30 * Units.AuMeters,
            OuterStabilityM = 45.0 * Units.AuMeters,
        };
        host.CalculateZones();

        if (!host.HasValidZone())
        {
            throw new InvalidOperationException("Solar reference chain failed at orbital host stage: exact solar host does not expose a valid stable zone.");
        }

        return host;
    }

    private static System.Collections.Generic.Dictionary<string, OrbitSlot> FindSolarSlots(double solarRadiusM, OrbitHost solarHost)
    {
        string lastFailure = "slot generation never produced a Solar-compatible scaffold";
        for (int attempt = 0; attempt < 256; attempt += 1)
        {
            OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
                solarHost,
                solarRadiusM,
                new Array<double>(),
                new Array<double>(),
                new SeededRng(SolarReferenceSeed + 200 + attempt));

            if (!result.Success)
            {
                lastFailure = "orbit slot generation did not succeed";
                continue;
            }

            OrbitSlotGenerator.SortByDistance(result.Slots);
            if (result.Slots.Count < SolarPlanets.Length)
            {
                lastFailure = $"expected at least {SolarPlanets.Length} candidate slots but found {result.Slots.Count}";
                continue;
            }

            System.Collections.Generic.Dictionary<string, OrbitSlot> matchedSlots;
            string failureMessage;
            if (TryMatchSolarSlots(result.Slots, out matchedSlots, out failureMessage))
            {
                return matchedSlots;
            }

            lastFailure = failureMessage;
        }

        throw new InvalidOperationException($"Solar reference chain failed at orbital slot stage after 256 attempts: {lastFailure}");
    }

    private static bool TryMatchSolarSlots(
        Array<OrbitSlot> slots,
        out System.Collections.Generic.Dictionary<string, OrbitSlot> matched,
        out string failureMessage)
    {
        matched = new System.Collections.Generic.Dictionary<string, OrbitSlot>();
        HashSet<string> usedSlotIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (SolarPlanetExpectation expectation in SolarPlanets)
        {
            OrbitSlot? bestSlot = null;
            double bestDifference = double.MaxValue;
            foreach (OrbitSlot slot in slots)
            {
                if (usedSlotIds.Contains(slot.Id))
                {
                    continue;
                }

                double slotAu = slot.SemiMajorAxisM / Units.AuMeters;
                double difference = System.Math.Abs(slotAu - expectation.TargetSemiMajorAxisAu);
                if (difference < bestDifference)
                {
                    bestDifference = difference;
                    bestSlot = slot;
                }
            }

            if (bestSlot == null || bestDifference > expectation.SlotToleranceAu)
            {
                failureMessage = $"for {expectation.Name}, expected a slot within {expectation.SlotToleranceAu:0.00} AU of {expectation.TargetSemiMajorAxisAu:0.000} AU, but nearest was {bestDifference:0.000} AU away. Available slots: {FormatSlotDistances(slots)}";
                matched.Clear();
                return false;
            }

            matched[expectation.Name] = bestSlot;
            usedSlotIds.Add(bestSlot.Id);
        }

        failureMessage = string.Empty;
        return true;
    }

    private static CelestialBody GenerateCandidatePlanet(
        SolarPlanetExpectation expectation,
        OrbitSlot matchedSlot,
        OrbitHost solarHost,
        int index)
    {
        string lastFailure = "no candidate planet was evaluated";
        for (int attempt = 0; attempt < 32; attempt += 1)
        {
            int seed = SolarReferenceSeed + 10 + (index * 100) + attempt;
            ParentContext context = ParentContext.ForPlanet(
                solarHost.CombinedMassKg,
                solarHost.CombinedLuminosityWatts,
                solarHost.EffectiveTemperatureK,
                SolarAgeYears,
                matchedSlot.SemiMajorAxisM);

            PlanetSpec spec = new PlanetSpec(
                seed,
                (int)expectation.CandidateSizeCategory,
                (int)matchedSlot.Zone)
            {
                NameHint = $"{expectation.Name} Candidate",
                CompositionBias = expectation.CompositionBias,
                EnvelopeOverride = expectation.EnvelopeOverride,
                VolatileRichness = expectation.VolatileRichness,
                HydrosphereTendency = expectation.HydrosphereTendency,
                ClassBias = expectation.ClassBias,
            };
            spec.SetOverride("orbital.semi_major_axis_m", matchedSlot.SemiMajorAxisM);

            if (expectation.CandidateRequiresAtmosphere)
            {
                spec.HasAtmosphere = true;
            }
            else
            {
                spec.HasAtmosphere = false;
            }

            CelestialBody candidatePlanet = PlanetGenerator.Generate(spec, context, new SeededRng(seed), enablePopulation: false);
            string? failure = GetCandidatePlanetFailure(expectation, candidatePlanet, matchedSlot);
            if (failure == null)
            {
                return candidatePlanet;
            }

            lastFailure = failure;
        }

        throw new InvalidOperationException($"Solar reference chain failed at {expectation.Name} candidate stage after 32 attempts: {lastFailure}");
    }

    private static CelestialBody GenerateLockedPlanet(SolarPlanetExpectation expectation, OrbitHost solarHost, int index)
    {
        int seed = SolarReferenceSeed + 40 + index;
        double orbitalDistanceM = expectation.TargetSemiMajorAxisAu * Units.AuMeters;
        ParentContext context = ParentContext.ForPlanet(
            solarHost.CombinedMassKg,
            solarHost.CombinedLuminosityWatts,
            solarHost.EffectiveTemperatureK,
            SolarAgeYears,
            orbitalDistanceM);

        OrbitZone.Zone zone;
        if (orbitalDistanceM < solarHost.HabitableZoneInnerM)
        {
            zone = OrbitZone.Zone.Hot;
        }
        else if (orbitalDistanceM > solarHost.FrostLineM)
        {
            zone = OrbitZone.Zone.Cold;
        }
        else
        {
            zone = OrbitZone.Zone.Temperate;
        }

        PlanetSpec spec = new PlanetSpec(
            seed,
            (int)expectation.CandidateSizeCategory,
            (int)zone)
        {
            NameHint = expectation.Name,
            CompositionBias = expectation.CompositionBias,
            EnvelopeOverride = expectation.EnvelopeOverride,
            VolatileRichness = expectation.VolatileRichness,
            HydrosphereTendency = expectation.HydrosphereTendency,
            ClassBias = expectation.ClassBias,
        };
        spec.SetOverride("physical.mass_earth", expectation.ExactMassEarth);
        spec.SetOverride("physical.radius_earth", expectation.ExactRadiusEarth);
        spec.SetOverride("orbital.semi_major_axis_m", orbitalDistanceM);

        if (expectation.ExactHasAtmosphere)
        {
            spec.HasAtmosphere = true;
        }
        else
        {
            spec.HasAtmosphere = false;
            spec.SetOverride("atmosphere.surface_pressure_pa", 0.0);
        }

        if (expectation.ExactPressurePa >= 0.0)
        {
            spec.SetOverride("atmosphere.surface_pressure_pa", expectation.ExactPressurePa);
        }

        if (expectation.ExactOceanCoverage >= 0.0)
        {
            spec.SetOverride("surface.hydrosphere.ocean_coverage", expectation.ExactOceanCoverage);
        }

        if (expectation.ExactIceCoverage >= 0.0)
        {
            spec.SetOverride("surface.hydrosphere.ice_coverage", expectation.ExactIceCoverage);
        }

        CelestialBody planet = PlanetGenerator.Generate(spec, context, new SeededRng(seed), enablePopulation: false);
        ValidateLockedPlanet(expectation, planet);
        return planet;
    }

    private static void ValidateLockedPlanet(SolarPlanetExpectation expectation, CelestialBody planet)
    {
        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        double radiusEarth = planet.Physical.RadiusM / Units.EarthRadiusMeters;
        double massDifference = System.Math.Abs(massEarth - expectation.ExactMassEarth);
        double radiusDifference = System.Math.Abs(radiusEarth - expectation.ExactRadiusEarth);
        if (massDifference > 1.0e-6 || radiusDifference > 1.0e-6)
        {
            throw new InvalidOperationException($"Solar reference chain failed at {expectation.Name} lock stage: exact mass/radius overrides were not honored.");
        }

        if (expectation.ExactHasAtmosphere && !planet.HasAtmosphere())
        {
            throw new InvalidOperationException($"Solar reference chain failed at {expectation.Name} lock stage: exact atmosphere-bearing target lost its atmosphere.");
        }

        if (!expectation.ExactHasAtmosphere && planet.HasAtmosphere())
        {
            throw new InvalidOperationException($"Solar reference chain failed at {expectation.Name} lock stage: exact airless target retained an atmosphere.");
        }

        if (expectation.ExactOceanCoverage >= 0.0)
        {
            if (!planet.HasSurface() || !planet.Surface!.HasHydrosphere())
            {
                throw new InvalidOperationException($"Solar reference chain failed at {expectation.Name} lock stage: exact hydrosphere overrides did not produce hydrosphere data.");
            }

            double oceanDifference = System.Math.Abs(planet.Surface.Hydrosphere!.OceanCoverage - expectation.ExactOceanCoverage);
            if (oceanDifference > 1.0e-6)
            {
                throw new InvalidOperationException($"Solar reference chain failed at {expectation.Name} lock stage: exact ocean coverage override was not honored.");
            }
        }
    }

    private static void RequireRange(string stage, string metric, double value, double minInclusive, double maxInclusive)
    {
        if (value < minInclusive || value > maxInclusive)
        {
            throw new InvalidOperationException($"Solar reference chain failed at {stage}: expected {metric} in range [{minInclusive:0.###}, {maxInclusive:0.###}] but found {value:0.###}.");
        }
    }

    private static string? GetCandidateSunFailure(CelestialBody star)
    {
        if (!star.HasStellar())
        {
            return "generated Sun-like star is missing stellar data";
        }

        double massSolar = star.Physical.MassKg / Units.SolarMassKg;
        if (massSolar < 0.85 || massSolar > 1.15)
        {
            return $"mass {massSolar:0.###} was outside [0.85, 1.15]";
        }

        double radiusSolar = star.Physical.RadiusM / Units.SolarRadiusMeters;
        if (radiusSolar < 0.85 || radiusSolar > 1.20)
        {
            return $"radius {radiusSolar:0.###} was outside [0.85, 1.20]";
        }

        double luminositySolar = star.Stellar!.GetLuminositySolar();
        if (luminositySolar < 0.70 || luminositySolar > 1.40)
        {
            return $"luminosity {luminositySolar:0.###} was outside [0.70, 1.40]";
        }

        double temperatureK = star.Stellar.EffectiveTemperatureK;
        if (temperatureK < 5400.0 || temperatureK > 6200.0)
        {
            return $"temperature {temperatureK:0.###} K was outside [5400, 6200]";
        }

        if (!star.Stellar.SpectralType.StartsWith("G", StringComparison.Ordinal))
        {
            return $"expected a G-type candidate but found '{star.Stellar.SpectralType}'";
        }

        return null;
    }

    private static string? GetCandidatePlanetFailure(
        SolarPlanetExpectation expectation,
        CelestialBody candidatePlanet,
        OrbitSlot matchedSlot)
    {
        double massEarth = candidatePlanet.Physical.MassKg / Units.EarthMassKg;
        if (massEarth < expectation.CandidateMassMinEarth || massEarth > expectation.CandidateMassMaxEarth)
        {
            return $"mass {massEarth:0.###} Earth masses at slot {matchedSlot.SemiMajorAxisM / Units.AuMeters:0.000} AU was outside [{expectation.CandidateMassMinEarth:0.###}, {expectation.CandidateMassMaxEarth:0.###}]";
        }

        double radiusEarth = candidatePlanet.Physical.RadiusM / Units.EarthRadiusMeters;
        if (radiusEarth < expectation.CandidateRadiusMinEarth || radiusEarth > expectation.CandidateRadiusMaxEarth)
        {
            return $"radius {radiusEarth:0.###} Earth radii at slot {matchedSlot.SemiMajorAxisM / Units.AuMeters:0.000} AU was outside [{expectation.CandidateRadiusMinEarth:0.###}, {expectation.CandidateRadiusMaxEarth:0.###}]";
        }

        if (expectation.CandidateRequiresAtmosphere && !candidatePlanet.HasAtmosphere())
        {
            return $"slot {matchedSlot.SemiMajorAxisM / Units.AuMeters:0.000} AU did not resolve an atmosphere-bearing candidate";
        }

        if (!expectation.CandidateRequiresAtmosphere && candidatePlanet.HasAtmosphere())
        {
            return $"slot {matchedSlot.SemiMajorAxisM / Units.AuMeters:0.000} AU resolved an atmosphere when the Solar target should be effectively airless";
        }

        if (expectation.CandidateRequiresHydrosphere)
        {
            if (!candidatePlanet.HasSurface() || !candidatePlanet.Surface!.HasHydrosphere())
            {
                return $"slot {matchedSlot.SemiMajorAxisM / Units.AuMeters:0.000} AU did not resolve a hydrosphere-bearing candidate";
            }

            double liquidCoverage = candidatePlanet.Surface.Hydrosphere!.GetLiquidCoverage();
            if (liquidCoverage < 0.10)
            {
                return $"candidate hydrosphere liquid coverage was only {liquidCoverage:0.000}";
            }
        }

        return null;
    }

    private static string FormatSlotDistances(Array<OrbitSlot> slots)
    {
        List<string> distances = new List<string>();
        foreach (OrbitSlot slot in slots)
        {
            distances.Add($"{slot.SemiMajorAxisM / Units.AuMeters:0.000} AU");
        }

        return string.Join(", ", distances);
    }

    private sealed class SolarPlanetExpectation
    {
        public string Name;
        public double TargetSemiMajorAxisAu;
        public double SlotToleranceAu;
        public SizeCategory.Category CandidateSizeCategory;
        public PlanetCompositionBias CompositionBias;
        public PlanetEnvelopeOverride EnvelopeOverride;
        public PlanetVolatileRichness VolatileRichness;
        public PlanetHydrosphereTendency HydrosphereTendency;
        public PlanetClassBias ClassBias;
        public double CandidateMassMinEarth;
        public double CandidateMassMaxEarth;
        public double CandidateRadiusMinEarth;
        public double CandidateRadiusMaxEarth;
        public bool CandidateRequiresAtmosphere;
        public bool CandidateRequiresHydrosphere;
        public double ExactMassEarth;
        public double ExactRadiusEarth;
        public bool ExactHasAtmosphere;
        public double ExactPressurePa;
        public double ExactOceanCoverage;
        public double ExactIceCoverage;

        public SolarPlanetExpectation(
            string name,
            double targetSemiMajorAxisAu,
            double slotToleranceAu,
            SizeCategory.Category candidateSizeCategory,
            PlanetCompositionBias compositionBias,
            PlanetEnvelopeOverride envelopeOverride,
            PlanetVolatileRichness volatileRichness,
            PlanetHydrosphereTendency hydrosphereTendency,
            PlanetClassBias classBias,
            double candidateMassMinEarth,
            double candidateMassMaxEarth,
            double candidateRadiusMinEarth,
            double candidateRadiusMaxEarth,
            bool candidateRequiresAtmosphere,
            bool candidateRequiresHydrosphere,
            double exactMassEarth,
            double exactRadiusEarth,
            bool exactHasAtmosphere,
            double exactPressurePa,
            double exactOceanCoverage,
            double exactIceCoverage)
        {
            Name = name;
            TargetSemiMajorAxisAu = targetSemiMajorAxisAu;
            SlotToleranceAu = slotToleranceAu;
            CandidateSizeCategory = candidateSizeCategory;
            CompositionBias = compositionBias;
            EnvelopeOverride = envelopeOverride;
            VolatileRichness = volatileRichness;
            HydrosphereTendency = hydrosphereTendency;
            ClassBias = classBias;
            CandidateMassMinEarth = candidateMassMinEarth;
            CandidateMassMaxEarth = candidateMassMaxEarth;
            CandidateRadiusMinEarth = candidateRadiusMinEarth;
            CandidateRadiusMaxEarth = candidateRadiusMaxEarth;
            CandidateRequiresAtmosphere = candidateRequiresAtmosphere;
            CandidateRequiresHydrosphere = candidateRequiresHydrosphere;
            ExactMassEarth = exactMassEarth;
            ExactRadiusEarth = exactRadiusEarth;
            ExactHasAtmosphere = exactHasAtmosphere;
            ExactPressurePa = exactPressurePa;
            ExactOceanCoverage = exactOceanCoverage;
            ExactIceCoverage = exactIceCoverage;
        }
    }
}
