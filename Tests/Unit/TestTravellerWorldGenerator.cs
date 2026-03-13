#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Rng;

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
}
