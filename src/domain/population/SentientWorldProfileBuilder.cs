using Godot;

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
    public static SentientWorldProfile? Build(PlanetPopulationData data)
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

        profile.UrbanizationShare = ResolveUrbanizationShare(profile, data);
        profile.SettlementPattern = ResolveSettlementPattern(profile, data, colonyShare);
        profile.PrimarySettlementRank = ResolvePrimarySettlementRank(profile.TotalPopulation, profile.UrbanizationShare);
        profile.LogisticsCapacity = ResolveLogisticsCapacity(profile);
        return profile;
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

    private sealed class Accumulator
    {
        public double TotalPopulation;
        public double NativePopulation;
        public double ColonyPopulation;
        public double WeightedTechLevel;
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
}
