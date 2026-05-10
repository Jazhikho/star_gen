using Godot;
using Godot.Collections;
using System.Collections.Generic;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Population;

/// <summary>
/// Derives a neutral sentient-world baseline from the active populations on a body.
/// </summary>
public static class SentientWorldProfileBuilder
{
    private const double MaxReferencePopulation = 10_000_000_000.0;
    private const double MaxReferenceCultureYears = 60_000.0;

    /// <summary>
    /// Builds a sentient-world baseline from the active native and colony populations on a body.
    /// </summary>
    public static SentientWorldProfile? Build(PlanetPopulationData data, GenerationUseCaseSettings? settings = null)
    {
        if (!data.IsInhabited())
        {
            return null;
        }

        Accumulator accumulator = AccumulatePopulationMetrics(data);
        if (accumulator.TotalPopulation <= 0.0)
        {
            return null;
        }

        SentientWorldProfile profile = new();
        profile.TotalPopulation = (int)System.Math.Round(accumulator.TotalPopulation);
        profile.NativePopulation = data.GetNativePopulation();
        profile.ColonyPopulation = data.GetColonyPopulation();
        profile.HighestTechLevel = data.GetHighestTechLevel();
        profile.EliteTechLevel = profile.HighestTechLevel;
        profile.MedianTechLevel = profile.HighestTechLevel;
        profile.DominantRegime = accumulator.DominantRegime;

        double habitability = GetHabitabilityFactor(data);
        double suitability = GetSuitabilityFactor(data);
        double resourceRichness = GetResourceRichness(data);
        double resourceDiversity = GetResourceDiversity(data);
        double waterSupport = GetWaterSupport(data);
        double selfSufficiency = accumulator.ColonySelfSufficiency;
        double populationScale = NormalizePopulation(accumulator.TotalPopulation);
        double groupScale = NormalizeGroupCount(data.GetExtantNativeCount() + data.GetActiveColonyCount());
        double averageTech = accumulator.WeightedTechLevel / accumulator.TotalPopulation;
        double averageCoreTech = accumulator.WeightedCoreTechLevel / accumulator.TotalPopulation;
        double cultureAge = NormalizeCultureAge(accumulator.EarliestSocietyYear);
        double colonyShare = accumulator.ColonyPopulation / accumulator.TotalPopulation;
        double nativeShare = accumulator.NativePopulation / accumulator.TotalPopulation;
        double coexistencePressure = GetCoexistencePressure(data, colonyShare, nativeShare);
        double terrainFragmentation = GetTerrainFragmentation(data);
        double frontierPressure = GetFrontierPressure(data, colonyShare, habitability, suitability);
        double administrativeCapacity = accumulator.WeightedAdministrativeCapacity / accumulator.TotalPopulation;
        double coercion = accumulator.WeightedCoercion / accumulator.TotalPopulation;
        double inclusiveness = accumulator.WeightedInclusiveness / accumulator.TotalPopulation;

        profile.SocialScale = Clamp01((populationScale * 0.75) + (groupScale * 0.15) + (habitability * 0.10));
        if (settings?.SentientSocialScaleModel == GenerationUseCaseSettings.SentientSocialScaleModelType.PopulationHierarchyAware)
        {
            double highTechnologyBonus = 0.0;
            if ((int)profile.HighestTechLevel >= (int)TechnologyLevel.Level.Industrial)
            {
                highTechnologyBonus = 0.06;
            }

            profile.SocialScale = Clamp01(
                (populationScale * 0.58)
                + (groupScale * 0.18)
                + (administrativeCapacity * 0.14)
                + highTechnologyBonus
                + (habitability * 0.04));
            profile.HumanAuditRequired = true;
        }

        profile.SurplusBase = Clamp01(
            (habitability * 0.25)
            + (suitability * 0.20)
            + (resourceRichness * 0.15)
            + (resourceDiversity * 0.10)
            + (waterSupport * 0.15)
            + (selfSufficiency * 0.10)
            + (averageTech * 0.05));

        profile.TradeConnectivity = Clamp01(
            (averageTech * 0.35)
            + (colonyShare * 0.25)
            + (NormalizeGroupCount(data.GetActiveColonyCount()) * 0.15)
            + (selfSufficiency * 0.10)
            + (resourceDiversity * 0.15));

        if ((int)profile.HighestTechLevel < (int)TechnologyLevel.Level.Spacefaring && profile.TradeConnectivity > 0.45)
        {
            profile.TradeConnectivity = 0.45;
        }

        profile.ExternalThreat = Clamp01(
            (GetConflictSignal(data) * 0.40)
            + (coexistencePressure * 0.20)
            + (frontierPressure * 0.20)
            + ((1.0 - habitability) * 0.10)
            + (coercion * 0.10));

        profile.StateCapacity = Clamp01(
            (administrativeCapacity * 0.40)
            + (profile.SocialScale * 0.20)
            + (profile.TradeConnectivity * 0.20)
            + (profile.SurplusBase * 0.10)
            + (averageTech * 0.10));

        profile.FiscalContract = Clamp01(
            (inclusiveness * 0.45)
            + (profile.TradeConnectivity * 0.20)
            + (profile.SocialScale * 0.15)
            + (profile.SurplusBase * 0.10)
            + (cultureAge * 0.10));

        profile.LegalCentralization = Clamp01(
            (administrativeCapacity * 0.35)
            + (coercion * 0.25)
            + (profile.SocialScale * 0.15)
            + (profile.TradeConnectivity * 0.15)
            + (groupScale * 0.10));

        profile.LegalReach = Clamp01((profile.StateCapacity * 0.65) + (profile.LegalCentralization * 0.35));
        profile.EnforcementReach = Clamp01(
            (profile.StateCapacity * 0.46)
            + (profile.LegalCentralization * 0.18)
            + (coercion * 0.18)
            + (profile.TradeConnectivity * 0.12)
            + (profile.SurplusBase * 0.06)
            - (terrainFragmentation * 0.14)
            - (frontierPressure * 0.10));
        profile.RestrictionPressure = Clamp01(
            (coercion * 0.55)
            + (profile.ExternalThreat * 0.25)
            + ((1.0 - inclusiveness) * 0.20));

        profile.CulturalAccumulation = Clamp01(
            (cultureAge * 0.40)
            + (profile.SocialScale * 0.25)
            + (profile.TradeConnectivity * 0.20)
            + (profile.SurplusBase * 0.15));

        profile.TechnologyAdoptionCapacity = Clamp01(
            (averageTech * 0.30)
            + (profile.TradeConnectivity * 0.25)
            + (profile.CulturalAccumulation * 0.20)
            + (profile.StateCapacity * 0.15)
            + (profile.SurplusBase * 0.10));
        double preliminaryUrbanizationShare = ResolveUrbanizationShare(profile, data);
        if (settings?.SentientTechnologyDiffusionModel == GenerationUseCaseSettings.SentientTechnologyDiffusionModelType.AccessCostDensityProxy)
        {
            double implementationCostProxy = Clamp01((1.0 - selfSufficiency) * 0.35 + terrainFragmentation * 0.30 + frontierPressure * 0.35);
            double densityProxy = Clamp01((populationScale * 0.55) + (preliminaryUrbanizationShare * 0.45));
            profile.TechnologyAdoptionCapacity = Clamp01(
                (profile.TechnologyAdoptionCapacity * 0.65)
                + (densityProxy * 0.25)
                - (implementationCostProxy * 0.20)
                + (averageTech * 0.10));
            profile.HumanAuditRequired = true;
        }

        profile.EconomicComplexity = Clamp01(
            (resourceDiversity * 0.25)
            + (profile.SurplusBase * 0.25)
            + (profile.TradeConnectivity * 0.20)
            + (profile.TechnologyAdoptionCapacity * 0.20)
            + (profile.StateCapacity * 0.10));
        if (settings?.SentientEconomicComplexityModel == GenerationUseCaseSettings.SentientEconomicComplexityModelType.CapabilityPortfolioProxy)
        {
            double capabilityRelatedness = Clamp01((resourceDiversity * 0.35) + (averageTech * 0.30) + (profile.CulturalAccumulation * 0.35));
            double bindingConstraint = Clamp01((1.0 - selfSufficiency) * 0.45 + frontierPressure * 0.35 + terrainFragmentation * 0.20);
            profile.EconomicComplexity = Clamp01((profile.EconomicComplexity * 0.55) + (capabilityRelatedness * 0.40) - (bindingConstraint * 0.20));
            profile.SurplusBase = Clamp01((profile.SurplusBase * 0.88) + (profile.EconomicComplexity * 0.12));
            profile.HumanAuditRequired = true;
        }

        profile.FactionalFragmentation = Clamp01(
            (groupScale * 0.30)
            + (coexistencePressure * 0.25)
            + (GetConflictSignal(data) * 0.20)
            + ((1.0 - profile.StateCapacity) * 0.15)
            + (terrainFragmentation * 0.10));

        profile.ReligiousCentralization = Clamp01(
            ((1.0 - profile.TradeConnectivity) * 0.30)
            + (profile.LegalCentralization * 0.25)
            + (profile.SocialScale * 0.20)
            + ((1.0 - profile.FactionalFragmentation) * 0.15)
            + (coercion * 0.10));
        profile.InternalLegitimacy = Clamp01(
            (inclusiveness * 0.42)
            + (profile.FiscalContract * 0.24)
            + ((1.0 - profile.RestrictionPressure) * 0.20)
            + ((1.0 - coexistencePressure) * 0.14));
        profile.LogisticsCapacity = ResolveLogisticsCapacity(profile);
        profile.ExternalLegitimacy = Clamp01(
            (profile.TradeConnectivity * 0.35)
            + (profile.StateCapacity * 0.25)
            + GetInterstellarHubLegitimacyBonus(profile)
            + ((1.0 - profile.ExternalThreat) * 0.25));
        if (settings?.SentientLegitimacyModel == GenerationUseCaseSettings.SentientLegitimacyModelType.InternalExternalNormProxy)
        {
            profile.LegalReach = Clamp01((profile.LegalReach * 0.70) + (profile.InternalLegitimacy * 0.18) + (profile.ExternalLegitimacy * 0.12));
            profile.EnforcementReach = Clamp01((profile.EnforcementReach * 0.80) + (profile.ExternalLegitimacy * 0.12) + (profile.InternalLegitimacy * 0.08));
            profile.HumanAuditRequired = true;
        }

        profile.UrbanizationShare = ResolveUrbanizationShare(profile, data);
        profile.SettlementPattern = ResolveSettlementPattern(profile, data, colonyShare);
        profile.PrimarySettlementRank = ResolvePrimarySettlementRank(profile.TotalPopulation, profile.UrbanizationShare);
        ApplyTechnologyAccessDiagnostics(
            profile,
            populationScale,
            averageTech,
            averageCoreTech,
            cultureAge,
            terrainFragmentation,
            frontierPressure,
            selfSufficiency,
            resourceDiversity,
            settings);
        profile.AvailableLifeBiomes = BuildAvailableLifeBiomes(data, settings);
        profile.TechnologyDomains = BuildTechnologyDomains(
            profile,
            data,
            resourceRichness,
            resourceDiversity,
            waterSupport,
            selfSufficiency,
            frontierPressure,
            terrainFragmentation,
            colonyShare,
            nativeShare);
        profile.LawLevel = SentientWorldProfile.DeriveLawLevel(
            profile.LegalReach,
            profile.EnforcementReach,
            profile.LegalCentralization,
            profile.RestrictionPressure,
            profile.StateCapacity,
            profile.ExternalThreat,
            coercion);
        profile.LawInterpretation = SentientWorldProfile.DeriveLawInterpretation(
            profile.LegalReach,
            profile.EnforcementReach,
            profile.LegalCentralization,
            profile.RestrictionPressure,
            profile.StateCapacity);
        profile.JurisdictionPluralism = SentientWorldProfile.DeriveJurisdictionPluralism(
            profile.LegalCentralization,
            profile.FactionalFragmentation,
            coexistencePressure,
            terrainFragmentation,
            groupScale);
        profile.JurisdictionConflict = SentientWorldProfile.DeriveJurisdictionConflict(
            profile.FactionalFragmentation,
            profile.ExternalThreat,
            profile.InternalLegitimacy,
            coexistencePressure,
            profile.RestrictionPressure,
            profile.StateCapacity);
        profile.JurisdictionStructure = SentientWorldProfile.DeriveJurisdictionStructure(
            profile.LegalCentralization,
            profile.StateCapacity,
            profile.TradeConnectivity,
            profile.JurisdictionPluralism,
            profile.JurisdictionConflict,
            colonyShare,
            nativeShare);
        profile.Factions = BuildFactions(profile, data);
        profile.CulturalFeatureTags = BuildCulturalFeatureTags(profile, data);
        profile.ReligionStructure = SentientWorldProfile.DeriveReligionStructure(profile);
        return profile;
    }

    /// <summary>
    /// Separates peak availability, median access, invention pressure, and adoption lag.
    /// </summary>
    private static void ApplyTechnologyAccessDiagnostics(
        SentientWorldProfile profile,
        double populationScale,
        double averageTech,
        double averageCoreTech,
        double cultureAge,
        double terrainFragmentation,
        double frontierPressure,
        double selfSufficiency,
        double resourceDiversity,
        GenerationUseCaseSettings? settings)
    {
        profile.EliteTechLevel = profile.HighestTechLevel;
        profile.InventionCapacity = Clamp01(
            (profile.CulturalAccumulation * 0.24)
            + (profile.EconomicComplexity * 0.24)
            + (profile.UrbanizationShare * 0.18)
            + (profile.SocialScale * 0.14)
            + (profile.SurplusBase * 0.12)
            + (cultureAge * 0.08));

        profile.AdoptionLagPressure = Clamp01(
            ((1.0 - selfSufficiency) * 0.25)
            + (terrainFragmentation * 0.20)
            + (frontierPressure * 0.20)
            + ((1.0 - profile.StateCapacity) * 0.15)
            + ((1.0 - profile.TradeConnectivity) * 0.12)
            + ((1.0 - resourceDiversity) * 0.08)
            - (profile.TechnologyAdoptionCapacity * 0.16));

        profile.TechnologyAccessInequality = Clamp01(
            (populationScale * 0.20)
            + (profile.UrbanizationShare * 0.18)
            + (profile.InventionCapacity * 0.20)
            + (profile.RestrictionPressure * 0.14)
            + (profile.FactionalFragmentation * 0.14)
            + (profile.ExternalThreat * 0.08)
            - (profile.InternalLegitimacy * 0.10)
            - (profile.EconomicComplexity * 0.08));

        double medianAccess = Clamp01(
            averageTech
            + (profile.TechnologyAdoptionCapacity * 0.18)
            + (profile.EconomicComplexity * 0.10)
            - (profile.TechnologyAccessInequality * 0.22)
            - (profile.AdoptionLagPressure * 0.18));
        profile.MedianTechLevel = ResolveTechnologyLevelFromNormalized(medianAccess);
        if ((int)profile.MedianTechLevel > (int)profile.EliteTechLevel)
        {
            profile.MedianTechLevel = profile.EliteTechLevel;
        }

        int eliteBaseCore = TechnologyLevel.EraToRepresentativeCoreLevel(profile.HighestTechLevel);
        int inventionBonus = (int)System.Math.Round(profile.InventionCapacity * 3.0);
        int hubBonus = 0;
        if (profile.LogisticsCapacity == "Interstellar Hub")
        {
            hubBonus = 1;
        }

        profile.EliteCoreTechLevel = TechnologyLevel.ClampCoreLevel(eliteBaseCore + inventionBonus + hubBonus);
        double coreScore = averageCoreTech
            + (profile.TechnologyAdoptionCapacity * 1.45)
            + (profile.EconomicComplexity * 0.90)
            - (profile.AdoptionLagPressure * 1.25);
        profile.CoreTechLevel = TechnologyLevel.ClampCoreLevel((int)System.Math.Round(coreScore));

        double medianCoreScore = profile.CoreTechLevel
            - (profile.TechnologyAccessInequality * 3.0)
            - (profile.AdoptionLagPressure * 1.5)
            + (profile.TechnologyAdoptionCapacity * 0.75);
        profile.MedianCoreTechLevel = TechnologyLevel.ClampCoreLevel((int)System.Math.Round(medianCoreScore));
        if (profile.MedianCoreTechLevel > profile.EliteCoreTechLevel)
        {
            profile.MedianCoreTechLevel = profile.EliteCoreTechLevel;
        }

        if (profile.CoreTechLevel > profile.EliteCoreTechLevel)
        {
            profile.CoreTechLevel = profile.EliteCoreTechLevel;
        }

        profile.HighestTechLevel = TechnologyLevel.CoreLevelToEra(profile.EliteCoreTechLevel);
        profile.EliteTechLevel = TechnologyLevel.CoreLevelToEra(profile.EliteCoreTechLevel);
        profile.MedianTechLevel = TechnologyLevel.CoreLevelToEra(profile.MedianCoreTechLevel);

        bool sourceAlignedTechnologyModel = false;
        if (settings != null)
        {
            if (settings.SentientTechnologyDiffusionModel == GenerationUseCaseSettings.SentientTechnologyDiffusionModelType.AccessCostDensityProxy)
            {
                sourceAlignedTechnologyModel = true;
            }

            if (settings.SentientEconomicComplexityModel == GenerationUseCaseSettings.SentientEconomicComplexityModelType.CapabilityPortfolioProxy)
            {
                sourceAlignedTechnologyModel = true;
            }
        }

        if (sourceAlignedTechnologyModel)
        {
            profile.HumanAuditRequired = true;
        }
    }

    private static Accumulator AccumulatePopulationMetrics(PlanetPopulationData data)
    {
        Accumulator accumulator = new();
        accumulator.EarliestSocietyYear = 0;

        foreach (NativePopulation nativePopulation in data.NativePopulations)
        {
            if (!nativePopulation.IsExtant || nativePopulation.Population <= 0)
            {
                continue;
            }

            double weight = nativePopulation.Population;
            accumulator.TotalPopulation += weight;
            accumulator.NativePopulation += weight;
            accumulator.WeightedTechLevel += weight * NormalizeTechLevel(nativePopulation.TechLevel);
            accumulator.WeightedCoreTechLevel += weight * TechnologyLevel.EraToRepresentativeCoreLevel(nativePopulation.TechLevel);
            accumulator.WeightedAdministrativeCapacity += weight * nativePopulation.Government.AdministrativeCapacity;
            accumulator.WeightedCoercion += weight * nativePopulation.Government.CoercionCentralization;
            accumulator.WeightedInclusiveness += weight * nativePopulation.Government.PoliticalInclusiveness;

            if (accumulator.DominantPopulation < weight)
            {
                accumulator.DominantPopulation = weight;
                accumulator.DominantRegime = nativePopulation.Government.Regime;
            }

            if (nativePopulation.OriginYear < accumulator.EarliestSocietyYear)
            {
                accumulator.EarliestSocietyYear = nativePopulation.OriginYear;
            }
        }

        foreach (Colony colony in data.Colonies)
        {
            if (!colony.IsActive || colony.Population <= 0)
            {
                continue;
            }

            double weight = colony.Population;
            accumulator.TotalPopulation += weight;
            accumulator.ColonyPopulation += weight;
            accumulator.WeightedTechLevel += weight * NormalizeTechLevel(colony.TechLevel);
            accumulator.WeightedCoreTechLevel += weight * TechnologyLevel.EraToRepresentativeCoreLevel(colony.TechLevel);
            accumulator.WeightedAdministrativeCapacity += weight * colony.Government.AdministrativeCapacity;
            accumulator.WeightedCoercion += weight * colony.Government.CoercionCentralization;
            accumulator.WeightedInclusiveness += weight * colony.Government.PoliticalInclusiveness;
            accumulator.WeightedSelfSufficiency += weight * colony.SelfSufficiency;
            accumulator.TotalColonyWeight += weight;

            if (accumulator.DominantPopulation < weight)
            {
                accumulator.DominantPopulation = weight;
                accumulator.DominantRegime = colony.Government.Regime;
            }

            if (colony.FoundingYear < accumulator.EarliestSocietyYear)
            {
                accumulator.EarliestSocietyYear = colony.FoundingYear;
            }
        }

        if (accumulator.TotalColonyWeight > 0.0)
        {
            accumulator.ColonySelfSufficiency = accumulator.WeightedSelfSufficiency / accumulator.TotalColonyWeight;
        }
        else
        {
            accumulator.ColonySelfSufficiency = 0.45 + (GetHabitabilityFactor(data) * 0.35);
            accumulator.ColonySelfSufficiency = Clamp01(accumulator.ColonySelfSufficiency);
        }

        return accumulator;
    }

    private static double ResolveUrbanizationShare(SentientWorldProfile profile, PlanetPopulationData data)
    {
        double rawUrbanization = 0.10
            + (profile.SocialScale * 0.30)
            + (profile.TradeConnectivity * 0.20)
            + (profile.TechnologyAdoptionCapacity * 0.20)
            + (profile.SurplusBase * 0.10)
            - ((1.0 - GetHabitabilityFactor(data)) * 0.10);
        rawUrbanization = Clamp01(rawUrbanization);

        double cap = 0.18;
        if ((int)profile.HighestTechLevel >= (int)TechnologyLevel.Level.Classical)
        {
            cap = 0.30;
        }

        if ((int)profile.HighestTechLevel >= (int)TechnologyLevel.Level.Renaissance)
        {
            cap = 0.55;
        }

        if ((int)profile.HighestTechLevel >= (int)TechnologyLevel.Level.Industrial)
        {
            cap = 0.75;
        }

        if ((int)profile.HighestTechLevel >= (int)TechnologyLevel.Level.Interstellar)
        {
            cap = 0.90;
        }

        if (rawUrbanization > cap)
        {
            return cap;
        }

        return rawUrbanization;
    }

    private static string ResolveSettlementPattern(
        SentientWorldProfile profile,
        PlanetPopulationData data,
        double colonyShare)
    {
        if (data.Profile != null && data.Profile.IsMoon && colonyShare >= 0.35)
        {
            if (data.Suitability != null)
            {
                if (data.Suitability.RequiresLifeSupport || data.Suitability.RequiresPressureSuit)
                {
                    return "Orbital-Heavy";
                }
            }
        }

        if (profile.UrbanizationShare >= 0.72
            && (int)profile.HighestTechLevel >= (int)TechnologyLevel.Level.Information
            && profile.StateCapacity >= 0.70)
        {
            return "Arcology-Heavy";
        }

        if (data.Profile != null)
        {
            if (data.Profile.OceanCoverage >= 0.55 && data.Profile.ContinentCount >= 4)
            {
                return "Archipelago";
            }

            if (data.Profile.ContinentCount >= 5)
            {
                return "Clustered";
            }
        }

        if (profile.TradeConnectivity >= 0.55 && profile.UrbanizationShare >= 0.35)
        {
            return "Corridor";
        }

        if (profile.SocialScale <= 0.25 || profile.TotalPopulation < 100_000)
        {
            return "Dispersed";
        }

        return "Clustered";
    }

    private static string ResolvePrimarySettlementRank(int totalPopulation, double urbanizationShare)
    {
        if (totalPopulation < 5_000)
        {
            return "Outpost";
        }

        if (totalPopulation < 100_000)
        {
            return "Town";
        }

        if (totalPopulation < 5_000_000)
        {
            return "City";
        }

        if (totalPopulation < 100_000_000)
        {
            return "Metroplex";
        }

        if (urbanizationShare >= 0.55)
        {
            return "World-City";
        }

        return "Metroplex";
    }

    private static string ResolveLogisticsCapacity(SentientWorldProfile profile)
    {
        double logisticsScore = (profile.TradeConnectivity * 0.40)
            + (profile.StateCapacity * 0.20)
            + (profile.SocialScale * 0.15)
            + (profile.TechnologyAdoptionCapacity * 0.15)
            + (NormalizeTechLevel(profile.HighestTechLevel) * 0.10);

        if (logisticsScore >= 0.80)
        {
            return "Interstellar Hub";
        }

        if (logisticsScore >= 0.60)
        {
            return "Major Port";
        }

        if (logisticsScore >= 0.40)
        {
            return "Regional Port";
        }

        if (logisticsScore >= 0.20)
        {
            return "Frontier Port";
        }

        return "Isolated";
    }

    private static double GetHabitabilityFactor(PlanetPopulationData data)
    {
        if (data.Profile == null)
        {
            return 0.0;
        }

        return Clamp01(data.Profile.HabitabilityScore / 10.0);
    }

    private static double GetSuitabilityFactor(PlanetPopulationData data)
    {
        if (data.Suitability == null)
        {
            return 0.0;
        }

        return Clamp01(data.Suitability.OverallScore / 100.0);
    }

    private static double GetResourceRichness(PlanetPopulationData data)
    {
        if (data.Profile == null || data.Profile.Resources.Count == 0)
        {
            return 0.0;
        }

        double total = 0.0;
        int count = 0;
        foreach (Variant key in data.Profile.Resources.Keys)
        {
            Variant value = data.Profile.Resources[key];
            if (value.VariantType == Variant.Type.Float)
            {
                total += (double)value;
                count += 1;
            }
            else if (value.VariantType == Variant.Type.Int)
            {
                total += (int)value;
                count += 1;
            }
        }

        if (count <= 0)
        {
            return 0.0;
        }

        return Clamp01(total / count);
    }

    private static double GetResourceDiversity(PlanetPopulationData data)
    {
        if (data.Profile == null)
        {
            return 0.0;
        }

        int count = data.Profile.Resources.Count;
        return Clamp01(count / 6.0);
    }

    private static double GetWaterSupport(PlanetPopulationData data)
    {
        if (data.Profile == null)
        {
            return 0.0;
        }

        if (data.Profile.HasLiquidWater)
        {
            return 1.0;
        }

        if (data.Profile.HasAtmosphere)
        {
            return 0.30;
        }

        return 0.0;
    }

    private static double GetCoexistencePressure(PlanetPopulationData data, double colonyShare, double nativeShare)
    {
        bool hasNatives = data.HasExtantNatives();
        bool hasColonies = data.HasActiveColonies();
        if (hasNatives && hasColonies)
        {
            return Clamp01(0.55 + (colonyShare * 0.25) + (nativeShare * 0.20));
        }

        int activeGroups = data.GetExtantNativeCount() + data.GetActiveColonyCount();
        if (activeGroups > 1)
        {
            return Clamp01((activeGroups - 1) / 4.0);
        }

        return 0.0;
    }

    private static double GetTerrainFragmentation(PlanetPopulationData data)
    {
        if (data.Profile == null)
        {
            return 0.0;
        }

        double fragmentation = 0.0;
        if (data.Profile.OceanCoverage >= 0.55)
        {
            fragmentation += 0.45;
        }

        if (data.Profile.ContinentCount >= 5)
        {
            fragmentation += 0.35;
        }

        if (data.Profile.IsMoon)
        {
            fragmentation += 0.10;
        }

        return Clamp01(fragmentation);
    }

    private static double GetFrontierPressure(
        PlanetPopulationData data,
        double colonyShare,
        double habitability,
        double suitability)
    {
        double frontierPressure = (colonyShare * 0.60)
            + ((1.0 - habitability) * 0.25)
            + ((1.0 - suitability) * 0.15);
        if (data.Profile != null && data.Profile.IsMoon)
        {
            frontierPressure += 0.10;
        }

        return Clamp01(frontierPressure);
    }

    private static double GetConflictSignal(PlanetPopulationData data)
    {
        if (data.HasNativeColonyConflict())
        {
            return 1.0;
        }

        return 0.0;
    }

    private static double NormalizePopulation(double population)
    {
        if (population <= 0.0)
        {
            return 0.0;
        }

        double clampedPopulation = population;
        if (clampedPopulation > MaxReferencePopulation)
        {
            clampedPopulation = MaxReferencePopulation;
        }

        return Clamp01(System.Math.Log10(clampedPopulation + 1.0) / System.Math.Log10(MaxReferencePopulation + 1.0));
    }

    private static double NormalizeGroupCount(int count)
    {
        if (count <= 1)
        {
            return 0.0;
        }

        return Clamp01((count - 1) / 5.0);
    }

    private static double NormalizeTechLevel(TechnologyLevel.Level level)
    {
        int maxLevelIndex = TechnologyLevel.Count() - 1;
        if (maxLevelIndex <= 0)
        {
            return 0.0;
        }

        return Clamp01((int)level / (double)maxLevelIndex);
    }

    private static TechnologyLevel.Level ResolveTechnologyLevelFromNormalized(double normalizedLevel)
    {
        int maxLevelIndex = TechnologyLevel.Count() - 1;
        int levelIndex = (int)System.Math.Round(Clamp01(normalizedLevel) * maxLevelIndex);
        if (levelIndex < 0)
        {
            levelIndex = 0;
        }

        if (levelIndex > maxLevelIndex)
        {
            levelIndex = maxLevelIndex;
        }

        return (TechnologyLevel.Level)levelIndex;
    }

    private static double NormalizeCultureAge(int earliestSocietyYear)
    {
        if (earliestSocietyYear >= 0)
        {
            return 0.0;
        }

        return Clamp01((-earliestSocietyYear) / MaxReferenceCultureYears);
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }

    private static double GetInterstellarHubLegitimacyBonus(SentientWorldProfile profile)
    {
        if (profile.LogisticsCapacity == "Interstellar Hub")
        {
            return 0.15;
        }

        return 0.0;
    }

    private static Array<TechnologyDomainAccessRecord> BuildTechnologyDomains(
        SentientWorldProfile profile,
        PlanetPopulationData data,
        double resourceRichness,
        double resourceDiversity,
        double waterSupport,
        double selfSufficiency,
        double frontierPressure,
        double terrainFragmentation,
        double colonyShare,
        double nativeShare)
    {
        Array<TechnologyDomainAccessRecord> domains = new();
        double lifeBiomeSupport = 0.0;
        if (profile.AvailableLifeBiomes.Count > 0)
        {
            lifeBiomeSupport = System.Math.Min(1.0, profile.AvailableLifeBiomes.Count / 6.0);
        }

        double harshWorldPressure = 0.0;
        if (data.Suitability != null)
        {
            if (data.Suitability.RequiresLifeSupport)
            {
                harshWorldPressure += 0.25;
            }

            if (data.Suitability.RequiresPressureSuit)
            {
                harshWorldPressure += 0.20;
            }
        }

        double biologySignal = Clamp01(waterSupport * 0.45 + lifeBiomeSupport * 0.30 + nativeShare * 0.25);
        AddTechnologyDomain(domains, profile, "Energy", resourceRichness * 0.55 + resourceDiversity * 0.20 + profile.EconomicComplexity * 0.25, 1.0, 0.0, 0.0);
        AddTechnologyDomain(domains, profile, "Materials", resourceDiversity * 0.45 + resourceRichness * 0.25 + profile.EconomicComplexity * 0.30, 0.8, 0.0, 0.0);
        AddTechnologyDomain(domains, profile, "Computing", profile.InventionCapacity * 0.45 + profile.UrbanizationShare * 0.25 + profile.TradeConnectivity * 0.30, 0.7, 0.0, 0.0);
        AddTechnologyDomain(domains, profile, "Communications", profile.TradeConnectivity * 0.50 + profile.StateCapacity * 0.20 + profile.UrbanizationShare * 0.30, 0.6, 0.0, 0.0);
        AddTechnologyDomain(domains, profile, "Medicine", biologySignal * 0.35 + profile.CulturalAccumulation * 0.25 + profile.StateCapacity * 0.20 + profile.SurplusBase * 0.20, 0.4, 0.0, 0.0);
        AddTechnologyDomain(domains, profile, "Biotechnology", biologySignal * 0.45 + profile.InventionCapacity * 0.25 + profile.CulturalAccumulation * 0.20 + waterSupport * 0.10, 0.2, 0.0, 0.0);
        AddTechnologyDomain(domains, profile, "Spaceflight", colonyShare * 0.34 + profile.TradeConnectivity * 0.32 + GetLogisticsCapacityScore(profile) * 0.20 + selfSufficiency * 0.14, 1.4, frontierPressure * 0.18, harshWorldPressure);
        AddTechnologyDomain(domains, profile, "Infrastructure", profile.StateCapacity * 0.32 + profile.SurplusBase * 0.26 + profile.UrbanizationShare * 0.22 + selfSufficiency * 0.20, 0.5, terrainFragmentation * 0.18 + frontierPressure * 0.10, harshWorldPressure);
        return domains;
    }

    private static double GetLogisticsCapacityScore(SentientWorldProfile profile)
    {
        if (profile.LogisticsCapacity == "Interstellar Hub")
        {
            return 1.0;
        }

        if (profile.LogisticsCapacity == "Major Port")
        {
            return 0.78;
        }

        if (profile.LogisticsCapacity == "Regional Port")
        {
            return 0.58;
        }

        if (profile.LogisticsCapacity == "Frontier Port")
        {
            return 0.34;
        }

        return 0.12;
    }

    private static void AddTechnologyDomain(
        Array<TechnologyDomainAccessRecord> domains,
        SentientWorldProfile profile,
        string domain,
        double supportSignal,
        double eliteBias,
        double lagPenalty,
        double adaptationPressure)
    {
        double support = Clamp01(supportSignal);
        double adoption = Clamp01((profile.TechnologyAdoptionCapacity * 0.62) + (support * 0.28) + (profile.TradeConnectivity * 0.10) - lagPenalty);
        double lag = Clamp01((profile.AdoptionLagPressure * 0.70) + ((1.0 - support) * 0.18) + lagPenalty - (profile.StateCapacity * 0.08));
        double inequality = Clamp01((profile.TechnologyAccessInequality * 0.72) + (eliteBias * 0.04) + (adaptationPressure * 0.16) - (adoption * 0.08));

        int coreDelta = (int)System.Math.Round((support - 0.50) * 4.0);
        int coreLevel = TechnologyLevel.ClampCoreLevel(profile.CoreTechLevel + coreDelta);
        int eliteLevel = TechnologyLevel.ClampCoreLevel(coreLevel + (int)System.Math.Round(eliteBias + profile.InventionCapacity));
        if (eliteLevel > profile.EliteCoreTechLevel)
        {
            eliteLevel = profile.EliteCoreTechLevel;
        }

        int medianLevel = TechnologyLevel.ClampCoreLevel(coreLevel - (int)System.Math.Round((inequality * 2.0) + lag));
        if (medianLevel > eliteLevel)
        {
            medianLevel = eliteLevel;
        }

        domains.Add(new TechnologyDomainAccessRecord
        {
            Domain = domain,
            CoreTechLevel = coreLevel,
            EliteCoreTechLevel = eliteLevel,
            MedianCoreTechLevel = medianLevel,
            AdoptionCapacity = adoption,
            LagPressure = lag,
            AccessInequality = inequality,
            SourceSignal = ResolveTechnologyDomainSourceSignal(adoption, lag, inequality, support, adaptationPressure),
        });
    }

    private static string ResolveTechnologyDomainSourceSignal(
        double adoption,
        double lag,
        double inequality,
        double support,
        double adaptationPressure)
    {
        if (adaptationPressure >= 0.25 && adoption >= 0.44)
        {
            return "necessity-driven";
        }

        if (lag >= 0.55)
        {
            return "frontier-lagged";
        }

        if (inequality >= 0.50)
        {
            return "elite-concentrated";
        }

        if (support >= 0.58 && adoption >= 0.55)
        {
            return "local-capability";
        }

        if (adoption >= 0.48)
        {
            return "trade-diffused";
        }

        return "limited-access";
    }

    private static Array<SentientFactionRecord> BuildFactions(SentientWorldProfile profile, PlanetPopulationData data)
    {
        List<SentientFactionRecord> factions = new();
        PopulationGroupInfo dominant = FindDominantPopulation(data);
        double totalPopulation = System.Math.Max(1.0, profile.TotalPopulation);
        double dominantShare = dominant.Population / totalPopulation;
        if (dominant.Population > 0.0)
        {
            factions.Add(CreateFaction(
                "faction_001",
                "Primary Administration",
                "Governing Bloc",
                System.Math.Max(0.30, dominantShare),
                "Aligned",
                Clamp01(profile.ExternalThreat * 0.35 + profile.FactionalFragmentation * 0.25),
                dominant.Id,
                "governance"));
        }

        if (profile.NativePopulation > 0 && !dominant.IsNative)
        {
            double nativeShare = profile.NativePopulation / totalPopulation;
            factions.Add(CreateFaction(
                "faction_002",
                "Native Assembly",
                "Native Bloc",
                System.Math.Max(0.12, nativeShare * 0.90),
                "Autonomous",
                Clamp01(profile.FactionalFragmentation * 0.55 + profile.ExternalThreat * 0.25),
                FindFirstNativeId(data),
                "sovereignty"));
        }

        if (profile.ColonyPopulation > 0 && dominant.IsNative)
        {
            double colonyShare = profile.ColonyPopulation / totalPopulation;
            factions.Add(CreateFaction(
                "faction_003",
                "Colonial Directorate",
                "Colony Bloc",
                System.Math.Max(0.12, colonyShare * 0.90),
                "Contested",
                Clamp01(profile.FactionalFragmentation * 0.45 + profile.TradeConnectivity * 0.20),
                FindFirstColonyId(data),
                "resource access"));
        }

        int activeGroups = data.GetExtantNativeCount() + data.GetActiveColonyCount();
        if (profile.FactionalFragmentation >= 0.30 || activeGroups >= 3)
        {
            factions.Add(CreateFaction(
                "faction_004",
                "Local Autonomy Network",
                "Opposition Network",
                0.18 + (profile.FactionalFragmentation * 0.18),
                "Opposed",
                Clamp01(0.35 + (profile.FactionalFragmentation * 0.45)),
                string.Empty,
                "local autonomy"));
        }

        if (profile.RestrictionPressure >= 0.50 || profile.ExternalThreat >= 0.46)
        {
            factions.Add(CreateFaction(
                "faction_005",
                "Security Compact",
                "Security Bloc",
                0.14 + (profile.RestrictionPressure * 0.16),
                "Aligned",
                Clamp01(0.25 + (profile.ExternalThreat * 0.45)),
                dominant.Id,
                "security"));
        }

        NormalizeFactionInfluence(factions);
        Array<SentientFactionRecord> result = new();
        foreach (SentientFactionRecord faction in factions)
        {
            result.Add(faction);
        }

        return result;
    }

    private static SentientFactionRecord CreateFaction(
        string id,
        string name,
        string type,
        double influenceShare,
        string regimeAlignment,
        double tensionLevel,
        string sourcePopulationId,
        string primaryIssue)
    {
        return new SentientFactionRecord
        {
            Id = id,
            Name = name,
            Type = type,
            InfluenceShare = Clamp01(influenceShare),
            RegimeAlignment = regimeAlignment,
            TensionLevel = Clamp01(tensionLevel),
            SourcePopulationId = sourcePopulationId,
            PrimaryIssue = primaryIssue,
        };
    }

    private static void NormalizeFactionInfluence(List<SentientFactionRecord> factions)
    {
        double total = 0.0;
        foreach (SentientFactionRecord faction in factions)
        {
            total += faction.InfluenceShare;
        }

        if (total <= 0.0)
        {
            return;
        }

        foreach (SentientFactionRecord faction in factions)
        {
            faction.InfluenceShare = Clamp01(faction.InfluenceShare / total);
        }
    }

    private static Array<string> BuildCulturalFeatureTags(SentientWorldProfile profile, PlanetPopulationData data)
    {
        Array<string> tags = new();
        if (profile.TradeConnectivity >= 0.50)
        {
            AddUniqueTag(tags, "trade-connected");
        }

        if (profile.ExternalThreat >= 0.38)
        {
            AddUniqueTag(tags, "frontier-pressure");
        }

        if (profile.FactionalFragmentation >= 0.32)
        {
            AddUniqueTag(tags, "plural-authority");
        }

        if (profile.UrbanizationShare >= 0.45)
        {
            AddUniqueTag(tags, "urban-settlement");
        }

        if (profile.TechnologyAccessInequality >= 0.26)
        {
            AddUniqueTag(tags, "tech-stratified");
        }

        if (data.HasExtantNatives() && data.HasActiveColonies())
        {
            AddUniqueTag(tags, "native-colony-contact");
        }

        if (profile.SettlementPattern == "Archipelago")
        {
            AddUniqueTag(tags, "archipelago-settled");
        }

        if (profile.AvailableLifeBiomes.Contains("Subsurface"))
        {
            AddUniqueTag(tags, "subsurface-adapted");
        }

        foreach (NativePopulation nativePopulation in data.NativePopulations)
        {
            if (!nativePopulation.IsExtant)
            {
                continue;
            }

            foreach (string trait in nativePopulation.CulturalTraits)
            {
                if (tags.Count >= 10)
                {
                    return tags;
                }

                AddUniqueTag(tags, NormalizeTag(trait));
            }
        }

        return tags;
    }

    private static Array<string> BuildAvailableLifeBiomes(PlanetPopulationData data, GenerationUseCaseSettings? settings)
    {
        Array<string> biomes = new();
        if (data.Profile == null)
        {
            return biomes;
        }

        BiologySupportEvaluator.Assessment assessment = BiologySupportEvaluator.Evaluate(data.Profile, settings);
        if (!assessment.IsSupported)
        {
            return biomes;
        }

        bool supportsSubsurfaceBiology = assessment.ProtectedBiosphereChance > 0.0;
        List<int> biomeKeys = new();
        foreach (Variant key in data.Profile.Biomes.Keys)
        {
            biomeKeys.Add((int)key);
        }

        biomeKeys.Sort();
        foreach (int biomeKey in biomeKeys)
        {
            BiomeType.Type biome = (BiomeType.Type)biomeKey;
            if (!BiomeType.CanSupportLife(biome))
            {
                continue;
            }

            if (biome == BiomeType.Type.Subsurface && !supportsSubsurfaceBiology)
            {
                continue;
            }

            biomes.Add(BiomeType.ToStringName(biome));
        }

        return biomes;
    }

    private static void AddUniqueTag(Array<string> tags, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        if (!tags.Contains(tag))
        {
            tags.Add(tag);
        }
    }

    private static string NormalizeTag(string value)
    {
        string normalized = value.Trim().ToLowerInvariant().Replace(" ", "-").Replace("_", "-");
        while (normalized.Contains("--"))
        {
            normalized = normalized.Replace("--", "-");
        }

        return normalized;
    }

    private static PopulationGroupInfo FindDominantPopulation(PlanetPopulationData data)
    {
        PopulationGroupInfo dominant = new();
        foreach (NativePopulation nativePopulation in data.NativePopulations)
        {
            if (!nativePopulation.IsExtant || nativePopulation.Population <= 0)
            {
                continue;
            }

            if (nativePopulation.Population > dominant.Population)
            {
                dominant.Id = nativePopulation.Id;
                dominant.Name = nativePopulation.Name;
                dominant.Population = nativePopulation.Population;
                dominant.IsNative = true;
            }
        }

        foreach (Colony colony in data.Colonies)
        {
            if (!colony.IsActive || colony.Population <= 0)
            {
                continue;
            }

            if (colony.Population > dominant.Population)
            {
                dominant.Id = colony.Id;
                dominant.Name = colony.Name;
                dominant.Population = colony.Population;
                dominant.IsNative = false;
            }
        }

        return dominant;
    }

    private static string FindFirstNativeId(PlanetPopulationData data)
    {
        foreach (NativePopulation nativePopulation in data.NativePopulations)
        {
            if (nativePopulation.IsExtant && nativePopulation.Population > 0)
            {
                return nativePopulation.Id;
            }
        }

        return string.Empty;
    }

    private static string FindFirstColonyId(PlanetPopulationData data)
    {
        foreach (Colony colony in data.Colonies)
        {
            if (colony.IsActive && colony.Population > 0)
            {
                return colony.Id;
            }
        }

        return string.Empty;
    }

    private sealed class Accumulator
    {
        public double TotalPopulation;
        public double NativePopulation;
        public double ColonyPopulation;
        public double WeightedTechLevel;
        public double WeightedCoreTechLevel;
        public double WeightedAdministrativeCapacity;
        public double WeightedCoercion;
        public double WeightedInclusiveness;
        public double WeightedSelfSufficiency;
        public double TotalColonyWeight;
        public double ColonySelfSufficiency;
        public double DominantPopulation;
        public GovernmentType.Regime DominantRegime = GovernmentType.Regime.Tribal;
        public int EarliestSocietyYear;
    }

    private sealed class PopulationGroupInfo
    {
        public string Id = string.Empty;
        public string Name = string.Empty;
        public double Population;
        public bool IsNative;
    }
}
