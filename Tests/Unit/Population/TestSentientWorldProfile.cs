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
        DotNetNativeTestSuite.AssertTrue(profile.EnforcementReach > 0.0, "Enforcement reach should be populated");
        DotNetNativeTestSuite.AssertTrue(profile.EnforcementReach <= profile.LegalReach + 0.20, "Practical enforcement should remain tied to, but distinct from, formal legal reach");
        DotNetNativeTestSuite.AssertTrue(profile.TradeConnectivity > 0.0, "Trade connectivity should be populated");
        DotNetNativeTestSuite.AssertTrue(profile.CoreTechLevel > 0, "Core technology level should be populated");
        DotNetNativeTestSuite.AssertTrue(profile.EliteCoreTechLevel >= profile.CoreTechLevel, "Elite core technology should meet or exceed overall core technology");
        DotNetNativeTestSuite.AssertTrue(profile.MedianCoreTechLevel <= profile.EliteCoreTechLevel, "Median core technology should not exceed elite core technology");
        DotNetNativeTestSuite.AssertTrue(profile.LawLevel >= 0 && profile.LawLevel <= 15, "Law level should be compressed to the 0-15 range");
        DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(profile.LawInterpretation), "Law interpretation should be populated");
        DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(profile.JurisdictionStructure), "Jurisdiction structure should be populated");
        DotNetNativeTestSuite.AssertTrue(profile.JurisdictionPluralism >= 0.0 && profile.JurisdictionPluralism <= 1.0, "Jurisdiction pluralism should be bounded");
        DotNetNativeTestSuite.AssertTrue(profile.JurisdictionConflict >= 0.0 && profile.JurisdictionConflict <= 1.0, "Jurisdiction conflict should be bounded");
        DotNetNativeTestSuite.AssertTrue(profile.Factions.Count > 0, "Faction records should be generated");
        DotNetNativeTestSuite.AssertTrue(profile.CulturalFeatureTags.Count > 0, "Cultural feature tags should be generated");
        DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(profile.ReligionStructure), "Religion structure should be populated");
        DotNetNativeTestSuite.AssertTrue(profile.AvailableLifeBiomes.Count > 0, "Life-capable test worlds should expose life biomes");
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
        original.CoreTechLevel = 12;
        original.EliteCoreTechLevel = 14;
        original.MedianCoreTechLevel = 9;
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
        original.EnforcementReach = 0.47;
        original.RestrictionPressure = 0.28;
        original.LawLevel = 6;
        original.LawInterpretation = "Codified Moderate Reach";
        original.JurisdictionStructure = "Charter/Federal";
        original.JurisdictionPluralism = 0.44;
        original.JurisdictionConflict = 0.23;
        original.CulturalAccumulation = 0.59;
        original.TechnologyAdoptionCapacity = 0.66;
        original.InventionCapacity = 0.62;
        original.AdoptionLagPressure = 0.23;
        original.TechnologyAccessInequality = 0.34;
        original.FactionalFragmentation = 0.31;
        original.ReligiousCentralization = 0.37;
        original.Factions.Add(new SentientFactionRecord
        {
            Id = "faction_001",
            Name = "Primary Administration",
            Type = "Governing Bloc",
            InfluenceShare = 0.62,
            RegimeAlignment = "Aligned",
            TensionLevel = 0.22,
            SourcePopulationId = "colony_001",
            PrimaryIssue = "governance",
        });
        original.CulturalFeatureTags.Add("trade-connected");
        original.CulturalFeatureTags.Add("tech-stratified");
        original.ReligionStructure = "Plural";
        original.AvailableLifeBiomes.Add("Forest");
        original.AvailableLifeBiomes.Add("Ocean");
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
        DotNetNativeTestSuite.AssertEqual(original.CoreTechLevel, restored.CoreTechLevel, "Core tech should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.EliteCoreTechLevel, restored.EliteCoreTechLevel, "Elite core tech should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.MedianCoreTechLevel, restored.MedianCoreTechLevel, "Median core tech should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.DominantRegime, restored.DominantRegime, "Dominant regime should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.SettlementPattern, restored.SettlementPattern, "Settlement pattern should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.TradeConnectivity, restored.TradeConnectivity, 0.0001, "Trade connectivity should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.LegalReach, restored.LegalReach, 0.0001, "Legal reach should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.EnforcementReach, restored.EnforcementReach, 0.0001, "Enforcement reach should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.LawLevel, restored.LawLevel, "Law level should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.LawInterpretation, restored.LawInterpretation, "Law interpretation should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.JurisdictionStructure, restored.JurisdictionStructure, "Jurisdiction structure should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.JurisdictionPluralism, restored.JurisdictionPluralism, 0.0001, "Jurisdiction pluralism should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.JurisdictionConflict, restored.JurisdictionConflict, 0.0001, "Jurisdiction conflict should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.EconomicComplexity, restored.EconomicComplexity, 0.0001, "Economic complexity should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.InventionCapacity, restored.InventionCapacity, 0.0001, "Invention capacity should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.AdoptionLagPressure, restored.AdoptionLagPressure, 0.0001, "Adoption lag pressure should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.TechnologyAccessInequality, restored.TechnologyAccessInequality, 0.0001, "Technology access inequality should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.InternalLegitimacy, restored.InternalLegitimacy, 0.0001, "Internal legitimacy should round-trip");
        DotNetNativeTestSuite.AssertFloatNear(original.ExternalLegitimacy, restored.ExternalLegitimacy, 0.0001, "External legitimacy should round-trip");
        DotNetNativeTestSuite.AssertEqual(1, restored.Factions.Count, "Faction array should round-trip");
        DotNetNativeTestSuite.AssertEqual("faction_001", restored.Factions[0].Id, "Faction id should round-trip");
        DotNetNativeTestSuite.AssertEqual(2, restored.CulturalFeatureTags.Count, "Culture tags should round-trip");
        DotNetNativeTestSuite.AssertEqual("Plural", restored.ReligionStructure, "Religion structure should round-trip");
        DotNetNativeTestSuite.AssertEqual(2, restored.AvailableLifeBiomes.Count, "Life biomes should round-trip");
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
        DotNetNativeTestSuite.AssertTrue(sourceAligned.EnforcementReach > 0.0, "Source-aligned profile should expose practical enforcement reach");
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

    /// <summary>
    /// Tests the neutral law level separates low law reach from low restriction pressure.
    /// </summary>
    public static void TestLawLevelDistinguishesReachFromRestriction()
    {
        int lowReachLaw = SentientWorldProfile.DeriveLawLevel(0.05, 0.02, 0.08, 0.00, 0.08, 0.00, 0.00);
        int broadModerateLaw = SentientWorldProfile.DeriveLawLevel(0.70, 0.62, 0.58, 0.10, 0.66, 0.10, 0.12);
        int broadRestrictiveLaw = SentientWorldProfile.DeriveLawLevel(0.70, 0.62, 0.58, 0.78, 0.66, 0.10, 0.68);
        string lowReachInterpretation = SentientWorldProfile.DeriveLawInterpretation(0.05, 0.02, 0.08, 0.00, 0.08);
        string broadModerateInterpretation = SentientWorldProfile.DeriveLawInterpretation(0.70, 0.62, 0.58, 0.10, 0.66);
        string restrictiveInterpretation = SentientWorldProfile.DeriveLawInterpretation(0.70, 0.62, 0.58, 0.78, 0.66);

        DotNetNativeTestSuite.AssertEqual("No Formal Reach", lowReachInterpretation, "Low legal reach should be interpreted as no formal reach");
        DotNetNativeTestSuite.AssertEqual("High-Capacity Legal Order", broadModerateInterpretation, "Low restriction with broad reach should not be treated as restrictive");
        DotNetNativeTestSuite.AssertEqual("Restrictive High-Enforcement Order", restrictiveInterpretation, "High restriction with broad reach should be restrictive");
        DotNetNativeTestSuite.AssertTrue(broadModerateLaw > lowReachLaw, "Broad formal law should exceed no-reach law");
        DotNetNativeTestSuite.AssertTrue(broadRestrictiveLaw > broadModerateLaw, "Restriction pressure should raise the compressed law code");
    }

    /// <summary>
    /// Tests jurisdiction structure keeps plural authority distinct from centralized capacity.
    /// </summary>
    public static void TestJurisdictionStructureDistinguishesPluralAndCentralizedLaw()
    {
        double pluralism = SentientWorldProfile.DeriveJurisdictionPluralism(0.18, 0.72, 0.80, 0.60, 0.65);
        double conflict = SentientWorldProfile.DeriveJurisdictionConflict(0.72, 0.20, 0.62, 0.80, 0.18, 0.34);
        string pluralStructure = SentientWorldProfile.DeriveJurisdictionStructure(0.18, 0.34, 0.30, pluralism, conflict, 0.0, 1.0);
        string federalStructure = SentientWorldProfile.DeriveJurisdictionStructure(0.48, 0.55, 0.55, 0.35, 0.20, 0.42, 0.36);
        string centralizedStructure = SentientWorldProfile.DeriveJurisdictionStructure(0.78, 0.82, 0.62, 0.10, 0.08, 0.0, 1.0);

        DotNetNativeTestSuite.AssertTrue(pluralism > 0.55, "Fragmented customary inputs should raise jurisdiction pluralism");
        DotNetNativeTestSuite.AssertEqual("Layered Customary", pluralStructure, "Plural legal authority should not collapse to centralized law");
        DotNetNativeTestSuite.AssertEqual("Charter/Federal", federalStructure, "Mixed native-colony worlds with trade should support charter or federal structure");
        DotNetNativeTestSuite.AssertEqual("Centralized Unitary", centralizedStructure, "High centralization and capacity should resolve to centralized unitary structure");
    }

    /// <summary>
    /// Tests faction generation is deterministic, bounded, and stable.
    /// </summary>
    public static void TestFactionGenerationIsDeterministicAndBounded()
    {
        SentientWorldProfile? first = SentientWorldProfileBuilder.Build(CreateInhabitedWorldData());
        SentientWorldProfile? second = SentientWorldProfileBuilder.Build(CreateInhabitedWorldData());

        DotNetNativeTestSuite.AssertNotNull(first, "First profile should exist");
        DotNetNativeTestSuite.AssertNotNull(second, "Second profile should exist");
        DotNetNativeTestSuite.AssertEqual(first!.Factions.Count, second!.Factions.Count, "Faction count should be deterministic");
        double influenceSum = 0.0;
        for (int index = 0; index < first.Factions.Count; index += 1)
        {
            DotNetNativeTestSuite.AssertEqual(first.Factions[index].Id, second.Factions[index].Id, "Faction ids should be stable");
            DotNetNativeTestSuite.AssertTrue(first.Factions[index].InfluenceShare >= 0.0, "Faction influence should be nonnegative");
            DotNetNativeTestSuite.AssertTrue(first.Factions[index].InfluenceShare <= 1.0, "Faction influence should be bounded");
            influenceSum += first.Factions[index].InfluenceShare;
        }

        DotNetNativeTestSuite.AssertTrue(influenceSum > 0.99 && influenceSum < 1.01, "Faction influence should normalize near one");
    }

    /// <summary>
    /// Tests higher fragmentation increases faction pressure.
    /// </summary>
    public static void TestFragmentationIncreasesFactionPressure()
    {
        SentientWorldProfile? lowFragmentation = SentientWorldProfileBuilder.Build(CreateFrontierWorldData());
        SentientWorldProfile? highFragmentation = SentientWorldProfileBuilder.Build(CreateFragmentedWorldData());

        DotNetNativeTestSuite.AssertNotNull(lowFragmentation, "Low-fragmentation profile should exist");
        DotNetNativeTestSuite.AssertNotNull(highFragmentation, "High-fragmentation profile should exist");
        DotNetNativeTestSuite.AssertTrue(
            highFragmentation!.FactionalFragmentation > lowFragmentation!.FactionalFragmentation,
            "Fragmented population inputs should raise the fragmentation axis");
        DotNetNativeTestSuite.AssertTrue(
            highFragmentation.Factions.Count >= lowFragmentation.Factions.Count,
            "Fragmented worlds should not produce fewer first-class factions");
        DotNetNativeTestSuite.AssertTrue(
            highFragmentation.JurisdictionPluralism > lowFragmentation.JurisdictionPluralism,
            "Fragmented worlds should raise jurisdiction pluralism");
    }

    /// <summary>
    /// Tests life biome readouts are gated by biology support and biome supportability.
    /// </summary>
    public static void TestAvailableLifeBiomesAreLifeGated()
    {
        SentientWorldProfile? lifeProfile = SentientWorldProfileBuilder.Build(CreateInhabitedWorldData());
        SentientWorldProfile? barrenProfile = SentientWorldProfileBuilder.Build(CreateFrontierWorldData());

        DotNetNativeTestSuite.AssertNotNull(lifeProfile, "Life-capable profile should exist");
        DotNetNativeTestSuite.AssertNotNull(barrenProfile, "Barren profile should exist");
        DotNetNativeTestSuite.AssertTrue(lifeProfile!.AvailableLifeBiomes.Contains("Forest"), "Supportable forest biome should be exposed");
        DotNetNativeTestSuite.AssertTrue(lifeProfile.AvailableLifeBiomes.Contains("Ocean"), "Supportable ocean biome should be exposed");
        DotNetNativeTestSuite.AssertFalse(lifeProfile.AvailableLifeBiomes.Contains("Barren"), "Barren physical terrain should not be exposed as a life biome");
        DotNetNativeTestSuite.AssertFalse(lifeProfile.AvailableLifeBiomes.Contains("Gas Giant"), "Gas giant physical terrain should not be exposed as a life biome");
        DotNetNativeTestSuite.AssertEqual(0, barrenProfile!.AvailableLifeBiomes.Count, "Non-life-capable worlds should expose no life biomes");
    }

    /// <summary>
    /// Tests Starfinder magic appears only in the Starfinder adapter payload.
    /// </summary>
    public static void TestStarfinderMagicIsAdapterOnly()
    {
        SentientWorldProfile? profile = SentientWorldProfileBuilder.Build(CreateInhabitedWorldData());
        GenerationUseCaseSettings defaultSettings = GenerationUseCaseSettings.CreateDefault();
        GenerationUseCaseSettings starfinderSettings = GenerationUseCaseSettings.CreateDefault();
        starfinderSettings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Starfinder;

        StarfinderWorldbuildingProfile? defaultAdapter = StarfinderWorldbuildingProfile.Build(profile, defaultSettings);
        StarfinderWorldbuildingProfile? starfinderAdapter = StarfinderWorldbuildingProfile.Build(profile, starfinderSettings);

        DotNetNativeTestSuite.AssertNull(defaultAdapter, "Default generation should not emit a Starfinder magic adapter");
        DotNetNativeTestSuite.AssertNotNull(starfinderAdapter, "Starfinder mode should emit its adapter payload");
        DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(starfinderAdapter!.MagicPrevalence), "Starfinder adapter should expose magic prevalence");
        DotNetNativeTestSuite.AssertFalse(profile!.ToDictionary().ContainsKey("magic_prevalence"), "Neutral profile serialization should not contain magic prevalence");
    }

    private static PlanetPopulationData CreateInhabitedWorldData()
    {
        PlanetPopulationData data = new();
        data.Profile = new PlanetProfile
        {
            BodyId = "world_001",
            HabitabilityScore = 8,
            AvgTemperatureK = 288,
            PressureAtm = 1.0,
            HasLiquidWater = true,
            HasAtmosphere = true,
            HasBreathableAtmosphere = true,
            OceanCoverage = 0.62,
            LandCoverage = 0.30,
            IceCoverage = 0.06,
            ContinentCount = 5,
            GravityG = 1.0,
            TectonicActivity = 0.45,
            VolcanismLevel = 0.10,
            WeatherSeverity = 0.18,
            MagneticFieldStrength = 0.55,
            RadiationLevel = 0.05,
            StellarFluxEarth = 1.0,
            HabitableZoneAlignment = 1.0,
            XuvExposure = 0.08,
        };
        data.Profile.Biomes[(int)BiomeType.Type.Forest] = 0.28;
        data.Profile.Biomes[(int)BiomeType.Type.Ocean] = 0.42;
        data.Profile.Biomes[(int)BiomeType.Type.Barren] = 0.08;
        data.Profile.Biomes[(int)BiomeType.Type.GasGiant] = 0.02;
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
        nativePopulation.CulturalTraits.Add("Maritime Commons");
        nativePopulation.CulturalTraits.Add("Ancestor Guilds");
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
            AvgTemperatureK = 120,
            PressureAtm = 0.0,
            HasLiquidWater = false,
            HasAtmosphere = false,
            HasBreathableAtmosphere = false,
            OceanCoverage = 0.08,
            LandCoverage = 0.72,
            IceCoverage = 0.05,
            ContinentCount = 2,
            IsMoon = true,
            GravityG = 0.30,
            RadiationLevel = 0.92,
            XuvExposure = 0.85,
            TidalHeatingFactor = 0.02,
        };
        data.Profile.Biomes[(int)BiomeType.Type.Barren] = 0.70;
        data.Profile.Biomes[(int)BiomeType.Type.IceSheet] = 0.20;
        data.Profile.Biomes[(int)BiomeType.Type.Volcanic] = 0.10;
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

    private static PlanetPopulationData CreateFragmentedWorldData()
    {
        PlanetPopulationData data = CreateInhabitedWorldData();

        NativePopulation secondNative = new();
        secondNative.Id = "native_002";
        secondNative.Name = "Highland Natives";
        secondNative.Population = 220000;
        secondNative.IsExtant = true;
        secondNative.TechLevel = TechnologyLevel.Level.Information;
        secondNative.OriginYear = -14000;
        secondNative.Government.Regime = GovernmentType.Regime.Chiefdom;
        secondNative.Government.AdministrativeCapacity = 0.28;
        secondNative.Government.CoercionCentralization = 0.35;
        secondNative.Government.PoliticalInclusiveness = 0.42;
        data.NativePopulations.Add(secondNative);

        Colony secondColony = new();
        secondColony.Id = "colony_002";
        secondColony.Name = "Resource Enclave";
        secondColony.Population = 130000;
        secondColony.IsActive = true;
        secondColony.TechLevel = TechnologyLevel.Level.Interstellar;
        secondColony.FoundingYear = -90;
        secondColony.SelfSufficiency = 0.36;
        secondColony.Government.Regime = GovernmentType.Regime.Corporate;
        secondColony.Government.AdministrativeCapacity = 0.48;
        secondColony.Government.CoercionCentralization = 0.66;
        secondColony.Government.PoliticalInclusiveness = 0.18;
        data.Colonies.Add(secondColony);

        Colony thirdColony = new();
        thirdColony.Id = "colony_003";
        thirdColony.Name = "Freeport";
        thirdColony.Population = 90000;
        thirdColony.IsActive = true;
        thirdColony.TechLevel = TechnologyLevel.Level.Spacefaring;
        thirdColony.FoundingYear = -55;
        thirdColony.SelfSufficiency = 0.44;
        thirdColony.Government.Regime = GovernmentType.Regime.CityState;
        thirdColony.Government.AdministrativeCapacity = 0.33;
        thirdColony.Government.CoercionCentralization = 0.24;
        thirdColony.Government.PoliticalInclusiveness = 0.62;
        data.Colonies.Add(thirdColony);

        return data;
    }
}
