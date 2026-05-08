#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for TravellerWorldGenerator.
/// </summary>
public static class TestTravellerWorldGenerator
{
    public static void TestGeneratePlanetProfileHonorsRequestedCodes()
    {
        SeededRng rng = new SeededRng(12345);
        TravellerWorldProfile profile = TravellerWorldGenerator.GeneratePlanetProfile(
            requestedSizeCode: 8,
            requestedAtmosphereCode: 6,
            requestedHydrographicsCode: 7,
            requestedPopulationCode: 5,
            OrbitZone.Zone.Temperate,
            rng);

        if (profile.SizeCode != 8)
        {
            throw new System.InvalidOperationException($"Expected size 8, got {profile.SizeCode}");
        }

        if (profile.AtmosphereCode != 6)
        {
            throw new System.InvalidOperationException($"Expected atmosphere 6, got {profile.AtmosphereCode}");
        }

        if (profile.HydrographicsCode != 7)
        {
            throw new System.InvalidOperationException($"Expected hydrographics 7, got {profile.HydrographicsCode}");
        }

        if (profile.PopulationCode != 5)
        {
            throw new System.InvalidOperationException($"Expected population 5, got {profile.PopulationCode}");
        }
    }

    public static void TestGeneratePlanetProfilePopulationZeroClearsDependentCodes()
    {
        SeededRng rng = new SeededRng(98765);
        TravellerWorldProfile profile = TravellerWorldGenerator.GeneratePlanetProfile(
            requestedSizeCode: 4,
            requestedAtmosphereCode: 5,
            requestedHydrographicsCode: 3,
            requestedPopulationCode: 0,
            OrbitZone.Zone.Temperate,
            rng);

        if (profile.GovernmentCode != 0 || profile.LawCode != 0 || profile.TechLevelCode != 0)
        {
            throw new System.InvalidOperationException("Population zero should zero out dependent Traveller codes");
        }

        if (profile.StarportCode != "X")
        {
            throw new System.InvalidOperationException($"Population zero should force starport X, got {profile.StarportCode}");
        }
    }

    public static void TestApplyToPlanetSpecSetsExpectedOverrides()
    {
        PlanetSpec spec = PlanetSpec.Random(12345);
        spec.OrbitZone = (int)OrbitZone.Zone.Cold;
        TravellerWorldProfile profile = new TravellerWorldProfile
        {
            SizeCode = 8,
            AtmosphereCode = 6,
            HydrographicsCode = 7,
            PopulationCode = 5,
        };

        TravellerWorldGenerator.ApplyToPlanetSpec(spec, profile, new SeededRng(12345));

        if (spec.SizeCategory < 0)
        {
            throw new System.InvalidOperationException("Traveller profile should set a concrete size category");
        }

        if (spec.HasAtmosphere.VariantType != Variant.Type.Bool || !(bool)spec.HasAtmosphere)
        {
            throw new System.InvalidOperationException("Traveller atmosphere code 6 should require an atmosphere");
        }

        if (!spec.Overrides.ContainsKey("surface.hydrosphere.ocean_coverage"))
        {
            throw new System.InvalidOperationException("Traveller hydrographics should set hydrosphere coverage");
        }

        if ((double)spec.Overrides["surface.hydrosphere.ocean_coverage"] <= 0.0)
        {
            throw new System.InvalidOperationException("Traveller hydrographics 7 should produce non-zero ocean coverage");
        }
    }

    public static void TestGeneratePlanetProfileAvoidsAllZeroAutoWorld()
    {
        SeededRng rng = new SeededRng(1);
        TravellerWorldProfile profile = TravellerWorldGenerator.GeneratePlanetProfile(
            requestedSizeCode: -1,
            requestedAtmosphereCode: -1,
            requestedHydrographicsCode: -1,
            requestedPopulationCode: -1,
            OrbitZone.Zone.Temperate,
            rng);

        bool allZero = profile.SizeCode == 0
            && profile.AtmosphereCode == 0
            && profile.HydrographicsCode == 0
            && profile.PopulationCode == 0;
        if (allZero)
        {
            throw new System.InvalidOperationException("Auto Traveller world generation should avoid the all-zero blank-world edge case");
        }
    }

    public static void TestGenerateTradeCodesAndTravelZoneFromSupportedUwpElements()
    {
        TravellerWorldProfile profile = new TravellerWorldProfile
        {
            StarportCode = "B",
            SizeCode = 8,
            AtmosphereCode = 10,
            HydrographicsCode = 10,
            PopulationCode = 9,
            GovernmentCode = 10,
            LawCode = 9,
            TechLevelCode = 12,
        };

        TravellerTradeCodeSet tradeCodes = TravellerWorldGenerator.GenerateTradeCodes(profile);
        string travelZone = TravellerWorldGenerator.DetermineTravelZone(profile);
        TravellerRouteProfile routeProfile = TravellerWorldGenerator.BuildRouteProfile(profile, tradeCodes);

        if (!tradeCodes.Contains("Hi"))
        {
            throw new System.InvalidOperationException("High-population profile should produce Hi");
        }

        if (!tradeCodes.Contains("Ht"))
        {
            throw new System.InvalidOperationException("High-tech profile should produce Ht");
        }

        if (!tradeCodes.Contains("Fl"))
        {
            throw new System.InvalidOperationException("Exotic wet atmosphere should produce Fl");
        }

        if (!tradeCodes.Contains("Wa"))
        {
            throw new System.InvalidOperationException("Hydrographics A should produce Wa");
        }

        if (travelZone != "Amber")
        {
            throw new System.InvalidOperationException($"Expected Amber travel zone, got '{travelZone}'");
        }

        if (routeProfile.MaxJumpNumber != 2)
        {
            throw new System.InvalidOperationException($"Expected max jump number 2, got {routeProfile.MaxJumpNumber}");
        }
    }

    public static void TestDeriveFromBodyUsesSentientWorldProfileCodes()
    {
        CelestialBody body = new CelestialBody(
            id: "traveller_profile_world",
            name: "Traveller Profile World",
            type: CelestialType.Type.Planet,
            physical: new PhysicalProps(radiusM: 6_371_000.0));
        PlanetPopulationData populationData = new();
        Colony colony = new();
        colony.Id = "colony_001";
        colony.Name = "Fixture Colony";
        colony.Population = 1_500_000;
        colony.IsActive = true;
        colony.TechLevel = TechnologyLevel.Level.Information;
        colony.Government.Regime = GovernmentType.Regime.Tribal;
        populationData.Colonies.Add(colony);
        populationData.SentientWorldProfile = new SentientWorldProfile
        {
            TotalPopulation = 1_500_000,
            ColonyPopulation = 1_500_000,
            DominantRegime = GovernmentType.Regime.Theocracy,
            EliteCoreTechLevel = 21,
            CoreTechLevel = 19,
            MedianCoreTechLevel = 17,
            LawLevel = 13,
        };
        body.PopulationData = populationData;

        TravellerWorldProfile profile = TravellerWorldGenerator.DeriveFromBody(body);

        if (profile.GovernmentCode != 13)
        {
            throw new System.InvalidOperationException($"Expected sentient profile government code 13, got {profile.GovernmentCode}");
        }

        if (profile.LawCode != 13)
        {
            throw new System.InvalidOperationException($"Expected sentient profile law code 13, got {profile.LawCode}");
        }

        if (profile.TechLevelCode != 15)
        {
            throw new System.InvalidOperationException($"Expected sentient profile tech code 15, got {profile.TechLevelCode}");
        }
    }

    public static void TestTravellerSystemTakeoverIsDeterministic()
    {
        SolarSystemSpec spec = SolarSystemSpec.Binary(24680);
        spec.IncludeAsteroidBelts = true;
        spec.GeneratePopulation = true;
        spec.UseCaseSettings = CreateTravellerSettings();

        SolarSystem? first = SystemFixtureGenerator.GenerateSystem(spec);
        SolarSystem? second = SystemFixtureGenerator.GenerateSystem(SolarSystemSpec.FromDictionary(spec.ToDictionary()));

        if (first == null || second == null)
        {
            throw new System.InvalidOperationException("Traveller fixture systems should generate");
        }

        if (first.TravellerProfile == null || second.TravellerProfile == null)
        {
            throw new System.InvalidOperationException("Traveller mode should produce a typed Traveller system profile");
        }

        if (first.TravellerProfile.MainworldBodyId != second.TravellerProfile.MainworldBodyId)
        {
            throw new System.InvalidOperationException("Traveller mainworld selection should be deterministic");
        }

        if (first.TravellerProfile.GetUwp() != second.TravellerProfile.GetUwp())
        {
            throw new System.InvalidOperationException("Traveller UWP should be deterministic for the same seed");
        }

        if (first.TravellerProfile.TradeCodes.ToDisplayString() != second.TravellerProfile.TradeCodes.ToDisplayString())
        {
            throw new System.InvalidOperationException("Traveller trade codes should be deterministic");
        }

        if (first.TravellerProfile.RouteProfile.ToDictionary().Count != second.TravellerProfile.RouteProfile.ToDictionary().Count)
        {
            throw new System.InvalidOperationException("Traveller route profile shape should be deterministic");
        }

        if (first.TravellerProfile.RouteProfile.MaxJumpNumber != second.TravellerProfile.RouteProfile.MaxJumpNumber)
        {
            throw new System.InvalidOperationException("Traveller route profile should be deterministic");
        }
    }

    private static GenerationUseCaseSettings CreateTravellerSettings()
    {
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Traveller;
        settings.ShowTravellerReadouts = true;
        settings.MainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Require;
        return settings;
    }
}
