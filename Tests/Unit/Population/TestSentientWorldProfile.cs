#nullable enable annotations
#nullable disable warnings
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for the neutral sentient-world baseline.
/// </summary>
public static class TestSentientWorldProfile
{
    /// <summary>
    /// Tests the builder returns null for uninhabited data.
    /// </summary>
    public static void TestBuilderReturnsNullForUninhabitedWorld()
    {
        PlanetPopulationData data = new();
        data.Profile = new PlanetProfile
        {
            BodyId = "world_001",
            HabitabilityScore = 8,
            HasLiquidWater = true,
            HasAtmosphere = true,
            OceanCoverage = 0.6,
            LandCoverage = 0.3,
            ContinentCount = 4,
        };

        SentientWorldProfile? profile = SentientWorldProfileBuilder.Build(data);
        DotNetNativeTestSuite.AssertNull(profile, "Uninhabited worlds should not produce a sentient-world baseline");
    }

    /// <summary>
    /// Tests the builder produces core settlement and governance fields for inhabited worlds.
    /// </summary>
    public static void TestBuilderProducesBaselineForInhabitedWorld()
    {
        PlanetPopulationData data = CreateInhabitedWorldData();

        SentientWorldProfile? profile = SentientWorldProfileBuilder.Build(data);

        DotNetNativeTestSuite.AssertNotNull(profile, "Inhabited worlds should produce a sentient-world baseline");
        DotNetNativeTestSuite.AssertTrue(
            profile!.LogisticsCapacity == "Regional Port" || profile.LogisticsCapacity == "Major Port",
            "Expected a settled logistics baseline above isolated or frontier tiers for the mixed inhabited test world");
        DotNetNativeTestSuite.AssertEqual("City", profile.PrimarySettlementRank, "Expected city-scale settlement rank for the test population");
        DotNetNativeTestSuite.AssertEqual("Archipelago", profile.SettlementPattern, "Mixed-ocean inhabited worlds should resolve to archipelago settlement when geography fragments settlement");
        DotNetNativeTestSuite.AssertTrue(profile.StateCapacity > 0.0, "State capacity should be populated");
        DotNetNativeTestSuite.AssertTrue(profile.TradeConnectivity > 0.0, "Trade connectivity should be populated");
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Constitutional, profile.DominantRegime, "Dominant regime should follow the largest active population");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        SentientWorldProfile original = new();
        original.TotalPopulation = 1500000;
        original.NativePopulation = 500000;
        original.ColonyPopulation = 1000000;
        original.HighestTechLevel = TechnologyLevel.Level.Interstellar;
        original.DominantRegime = GovernmentType.Regime.Constitutional;
        original.SettlementPattern = "Corridor";
        original.UrbanizationShare = 0.52;
        original.PrimarySettlementRank = "Metroplex";
        original.LogisticsCapacity = "Major Port";
        original.SocialScale = 0.63;
        original.SurplusBase = 0.58;
        original.TradeConnectivity = 0.61;
        original.ExternalThreat = 0.22;
        original.StateCapacity = 0.57;
        original.FiscalContract = 0.51;
        original.LegalCentralization = 0.49;
        original.LegalReach = 0.55;
        original.RestrictionPressure = 0.28;
        original.CulturalAccumulation = 0.59;
        original.TechnologyAdoptionCapacity = 0.66;
        original.FactionalFragmentation = 0.31;
        original.ReligiousCentralization = 0.37;

        Godot.Collections.Dictionary data = original.ToDictionary();
        SentientWorldProfile restored = SentientWorldProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.TotalPopulation, restored.TotalPopulation, "Total population should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.HighestTechLevel, restored.HighestTechLevel, "Highest tech should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.DominantRegime, restored.DominantRegime, "Dominant regime should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.SettlementPattern, restored.SettlementPattern, "Settlement pattern should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.TradeConnectivity, restored.TradeConnectivity, 0.0001, "Trade connectivity should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.LegalReach, restored.LegalReach, 0.0001, "Legal reach should round-trip");
    }

    private static PlanetPopulationData CreateInhabitedWorldData()
    {
        PlanetPopulationData data = new();
        data.Profile = new PlanetProfile
        {
            BodyId = "world_001",
            HabitabilityScore = 8,
            HasLiquidWater = true,
            HasAtmosphere = true,
            HasBreathableAtmosphere = true,
            OceanCoverage = 0.62,
            LandCoverage = 0.30,
            ContinentCount = 5,
        };
        data.Profile.Resources[(int)ResourceType.Type.Water] = 0.9;
        data.Profile.Resources[(int)ResourceType.Type.Metals] = 0.7;
        data.Profile.Resources[(int)ResourceType.Type.Organics] = 0.8;
        data.Profile.Resources[(int)ResourceType.Type.RareElements] = 0.4;

        data.Suitability = new ColonySuitability
        {
            OverallScore = 78,
        };

        NativePopulation nativePopulation = new();
        nativePopulation.Id = "native_001";
        nativePopulation.Name = "Natives";
        nativePopulation.Population = 600000;
        nativePopulation.IsExtant = true;
        nativePopulation.TechLevel = TechnologyLevel.Level.Information;
        nativePopulation.OriginYear = -18000;
        nativePopulation.Government.Regime = GovernmentType.Regime.EliteRepublic;
        nativePopulation.Government.AdministrativeCapacity = 0.52;
        nativePopulation.Government.CoercionCentralization = 0.41;
        nativePopulation.Government.PoliticalInclusiveness = 0.48;
        data.NativePopulations.Add(nativePopulation);

        Colony colony = new();
        colony.Id = "colony_001";
        colony.Name = "First Colony";
        colony.Population = 950000;
        colony.IsActive = true;
        colony.TechLevel = TechnologyLevel.Level.Interstellar;
        colony.FoundingYear = -220;
        colony.SelfSufficiency = 0.67;
        colony.Government.Regime = GovernmentType.Regime.Constitutional;
        colony.Government.AdministrativeCapacity = 0.73;
        colony.Government.CoercionCentralization = 0.36;
        colony.Government.PoliticalInclusiveness = 0.69;
        data.Colonies.Add(colony);

        return data;
    }
}
