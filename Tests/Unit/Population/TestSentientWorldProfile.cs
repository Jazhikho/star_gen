#nullable enable annotations
#nullable disable warnings
using StarGen.Domain.Population;
using StarGen.Domain.Generation;
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
        DotNetNativeTestSuite.AssertTrue(profile.InventionCapacity > 0.0, "Invention capacity should be populated");
        DotNetNativeTestSuite.AssertTrue(profile.AdoptionLagPressure >= 0.0, "Adoption lag pressure should be bounded");
        DotNetNativeTestSuite.AssertTrue(profile.TechnologyAccessInequality >= 0.0, "Technology access inequality should be bounded");
        DotNetNativeTestSuite.AssertTrue(
            (int)profile.MedianTechLevel <= (int)profile.EliteTechLevel,
            "Median technology access should not exceed elite technology access");
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
        original.EliteTechLevel = TechnologyLevel.Level.Interstellar;
        original.MedianTechLevel = TechnologyLevel.Level.Information;
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
        original.InventionCapacity = 0.62;
        original.AdoptionLagPressure = 0.23;
        original.TechnologyAccessInequality = 0.34;
        original.FactionalFragmentation = 0.31;
        original.ReligiousCentralization = 0.37;
        original.EconomicComplexity = 0.64;
        original.InternalLegitimacy = 0.53;
        original.ExternalLegitimacy = 0.48;
        original.HumanAuditRequired = true;

        Godot.Collections.Dictionary data = original.ToDictionary();
        SentientWorldProfile restored = SentientWorldProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.TotalPopulation, restored.TotalPopulation, "Total population should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.HighestTechLevel, restored.HighestTechLevel, "Highest tech should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.EliteTechLevel, restored.EliteTechLevel, "Elite tech access should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.MedianTechLevel, restored.MedianTechLevel, "Median tech access should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.DominantRegime, restored.DominantRegime, "Dominant regime should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.SettlementPattern, restored.SettlementPattern, "Settlement pattern should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.TradeConnectivity, restored.TradeConnectivity, 0.0001, "Trade connectivity should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.LegalReach, restored.LegalReach, 0.0001, "Legal reach should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.EconomicComplexity, restored.EconomicComplexity, 0.0001, "Economic complexity should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.InventionCapacity, restored.InventionCapacity, 0.0001, "Invention capacity should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.AdoptionLagPressure, restored.AdoptionLagPressure, 0.0001, "Adoption lag pressure should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.TechnologyAccessInequality, restored.TechnologyAccessInequality, 0.0001, "Technology access inequality should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.InternalLegitimacy, restored.InternalLegitimacy, 0.0001, "Internal legitimacy should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.ExternalLegitimacy, restored.ExternalLegitimacy, 0.0001, "External legitimacy should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.HumanAuditRequired, restored.HumanAuditRequired, "Human-audit flag should round-trip");
    }

    /// <summary>
    /// Tests source-aligned sentient model switches materially change the profile and mark audit status.
    /// </summary>
    public static void TestSourceAlignedModelsChangeProfileAndRequireAudit()
    {
        PlanetPopulationData data = CreateInhabitedWorldData();
        SentientWorldProfile? baseline = SentientWorldProfileBuilder.Build(data);
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.SentientSocialScaleModel = GenerationUseCaseSettings.SentientSocialScaleModelType.PopulationHierarchyAware;
        settings.SentientTechnologyDiffusionModel = GenerationUseCaseSettings.SentientTechnologyDiffusionModelType.AccessCostDensityProxy;
        settings.SentientEconomicComplexityModel = GenerationUseCaseSettings.SentientEconomicComplexityModelType.CapabilityPortfolioProxy;
        settings.SentientLegitimacyModel = GenerationUseCaseSettings.SentientLegitimacyModelType.InternalExternalNormProxy;

        SentientWorldProfile? sourceAligned = SentientWorldProfileBuilder.Build(data, settings);

        DotNetNativeTestSuite.AssertNotNull(baseline, "Baseline profile should exist");
        DotNetNativeTestSuite.AssertNotNull(sourceAligned, "Source-aligned profile should exist");
        DotNetNativeTestSuite.AssertTrue(sourceAligned!.HumanAuditRequired, "Source-aligned social-science proxies should require human audit");
        DotNetNativeTestSuite.AssertTrue(sourceAligned.EconomicComplexity > 0.0, "Economic complexity proxy should be populated");
        DotNetNativeTestSuite.AssertTrue(sourceAligned.InventionCapacity > 0.0, "Source-aligned profile should expose invention capacity");
        DotNetNativeTestSuite.AssertTrue(sourceAligned.AdoptionLagPressure >= 0.0, "Source-aligned profile should expose adoption lag pressure");
        DotNetNativeTestSuite.AssertTrue(sourceAligned.TechnologyAccessInequality >= 0.0, "Source-aligned profile should expose access inequality");
        DotNetNativeTestSuite.AssertTrue(
            System.Math.Abs(sourceAligned.TechnologyAdoptionCapacity - baseline!.TechnologyAdoptionCapacity) > 0.0001
            || System.Math.Abs(sourceAligned.SocialScale - baseline.SocialScale) > 0.0001,
            "Selected sentient models should materially change at least one profile score");
    }

    /// <summary>
    /// Tests technology access diagnostics distinguish frontier lag from connected adoption.
    /// </summary>
    public static void TestTechnologyAccessDiagnosticsSeparatePeakAndMedianAccess()
    {
        PlanetPopulationData connectedData = CreateInhabitedWorldData();
        PlanetPopulationData frontierData = CreateFrontierWorldData();
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.SentientTechnologyDiffusionModel = GenerationUseCaseSettings.SentientTechnologyDiffusionModelType.AccessCostDensityProxy;
        settings.SentientEconomicComplexityModel = GenerationUseCaseSettings.SentientEconomicComplexityModelType.CapabilityPortfolioProxy;

        SentientWorldProfile? connectedProfile = SentientWorldProfileBuilder.Build(connectedData, settings);
        SentientWorldProfile? frontierProfile = SentientWorldProfileBuilder.Build(frontierData, settings);

        DotNetNativeTestSuite.AssertNotNull(connectedProfile, "Connected profile should exist");
        DotNetNativeTestSuite.AssertNotNull(frontierProfile, "Frontier profile should exist");
        DotNetNativeTestSuite.AssertTrue(
            frontierProfile!.AdoptionLagPressure > connectedProfile!.AdoptionLagPressure,
            "Harsh, low-self-sufficiency frontier worlds should carry higher adoption-lag pressure");
        DotNetNativeTestSuite.AssertTrue(
            (int)frontierProfile.MedianTechLevel <= (int)frontierProfile.EliteTechLevel,
            "Frontier median technology access should stay at or below elite access");
        DotNetNativeTestSuite.AssertTrue(
            frontierProfile.TechnologyAccessInequality > 0.0,
            "Frontier profiles should expose a nonzero access-inequality diagnostic");
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

    private static PlanetPopulationData CreateFrontierWorldData()
    {
        PlanetPopulationData data = new();
        data.Profile = new PlanetProfile
        {
            BodyId = "frontier_001",
            HabitabilityScore = 3,
            HasLiquidWater = false,
            HasAtmosphere = false,
            HasBreathableAtmosphere = false,
            OceanCoverage = 0.08,
            LandCoverage = 0.72,
            ContinentCount = 2,
            IsMoon = true,
        };
        data.Profile.Resources[(int)ResourceType.Type.Metals] = 0.8;
        data.Profile.Resources[(int)ResourceType.Type.RareElements] = 0.6;

        data.Suitability = new ColonySuitability
        {
            OverallScore = 28,
            RequiresLifeSupport = true,
            RequiresPressureSuit = true,
        };

        Colony colony = new();
        colony.Id = "frontier_colony_001";
        colony.Name = "Frontier Hold";
        colony.Population = 42000;
        colony.IsActive = true;
        colony.TechLevel = TechnologyLevel.Level.Interstellar;
        colony.FoundingYear = -18;
        colony.SelfSufficiency = 0.18;
        colony.Government.Regime = GovernmentType.Regime.Corporate;
        colony.Government.AdministrativeCapacity = 0.38;
        colony.Government.CoercionCentralization = 0.62;
        colony.Government.PoliticalInclusiveness = 0.24;
        data.Colonies.Add(colony);

        return data;
    }
}
