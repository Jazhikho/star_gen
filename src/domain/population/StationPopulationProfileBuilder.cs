namespace StarGen.Domain.Population;

/// <summary>
/// Builds a planet-like sentient population profile for a single space station.
/// </summary>
public static class StationPopulationProfileBuilder
{
    /// <summary>
    /// Builds a neutral sentient-world profile for the station population.
    /// </summary>
    public static SentientWorldProfile? Build(SpaceStation station)
    {
        if (station.Population <= 0 || !station.IsOperational)
        {
            return null;
        }

        SentientWorldProfile profile = new();
        profile.TotalPopulation = station.Population;
        profile.NativePopulation = 0;
        profile.ColonyPopulation = station.Population;
        profile.HighestTechLevel = TechnologyLevel.Level.Spacefaring;
        profile.EliteTechLevel = TechnologyLevel.Level.Spacefaring;
        profile.MedianTechLevel = TechnologyLevel.Level.Spacefaring;
        profile.DominantRegime = station.GetRegime();

        double populationScale = NormalizePopulation(station.Population);
        double serviceScale = NormalizeServiceCount(station.Services.Count);
        double stationScale = NormalizeStationClass(station.StationClass);
        double governanceCapacity = ResolveGovernanceCapacity(station);
        double specialization = ResolveSpecialization(station);
        double tradeConnectivity = ResolveTradeConnectivity(station, serviceScale, stationScale);
        double securityPressure = ResolveSecurityPressure(station);

        profile.SocialScale = Clamp01((populationScale * 0.55) + (stationScale * 0.30) + (serviceScale * 0.15));
        profile.SurplusBase = Clamp01((serviceScale * 0.35) + (stationScale * 0.25) + (specialization * 0.25) + 0.15);
        profile.TradeConnectivity = tradeConnectivity;
        profile.ExternalThreat = securityPressure;
        profile.StateCapacity = Clamp01((governanceCapacity * 0.45) + (profile.SocialScale * 0.25) + (tradeConnectivity * 0.20) + (stationScale * 0.10));
        profile.FiscalContract = Clamp01((profile.TradeConnectivity * 0.35) + (profile.StateCapacity * 0.30) + (profile.SurplusBase * 0.20) + 0.10);
        profile.LegalCentralization = Clamp01((profile.StateCapacity * 0.55) + (securityPressure * 0.25) + (stationScale * 0.20));
        profile.LegalReach = Clamp01((profile.StateCapacity * 0.70) + (profile.LegalCentralization * 0.30));
        profile.EnforcementReach = Clamp01((profile.StateCapacity * 0.42) + (profile.LegalCentralization * 0.20) + (securityPressure * 0.22) + (serviceScale * 0.10) + (stationScale * 0.06));
        profile.RestrictionPressure = Clamp01((securityPressure * 0.55) + ((1.0 - profile.FiscalContract) * 0.25) + (stationScale * 0.10));
        profile.CulturalAccumulation = Clamp01((profile.SocialScale * 0.35) + (profile.TradeConnectivity * 0.30) + (profile.SurplusBase * 0.20) + (serviceScale * 0.15));
        profile.TechnologyAdoptionCapacity = Clamp01((profile.TradeConnectivity * 0.35) + (profile.CulturalAccumulation * 0.25) + (profile.StateCapacity * 0.20) + 0.20);
        profile.EconomicComplexity = Clamp01((specialization * 0.35) + (serviceScale * 0.25) + (profile.TradeConnectivity * 0.20) + (profile.TechnologyAdoptionCapacity * 0.20));
        profile.FactionalFragmentation = Clamp01((1.0 - profile.StateCapacity) * 0.35 + stationScale * 0.20 + securityPressure * 0.20);
        profile.ReligiousCentralization = Clamp01((profile.LegalCentralization * 0.35) + ((1.0 - profile.TradeConnectivity) * 0.20) + (profile.SocialScale * 0.20));
        profile.InternalLegitimacy = Clamp01((profile.FiscalContract * 0.35) + ((1.0 - profile.RestrictionPressure) * 0.30) + (governanceCapacity * 0.25));
        profile.LogisticsCapacity = ResolveLogisticsCapacity(profile);
        profile.ExternalLegitimacy = Clamp01((profile.TradeConnectivity * 0.45) + (profile.StateCapacity * 0.25) + ((1.0 - profile.ExternalThreat) * 0.20));
        profile.UrbanizationShare = ResolveUrbanizationShare(station, profile);
        profile.SettlementPattern = ResolveSettlementPattern(station);
        profile.PrimarySettlementRank = ResolvePrimarySettlementRank(station);
        ApplyTechnologyAccessDiagnostics(profile, stationScale, serviceScale, securityPressure);
        profile.HumanAuditRequired = true;
        return profile;
    }

    /// <summary>
    /// Adds station-local technology access diagnostics for inhabited stations.
    /// </summary>
    private static void ApplyTechnologyAccessDiagnostics(
        SentientWorldProfile profile,
        double stationScale,
        double serviceScale,
        double securityPressure)
    {
        profile.InventionCapacity = Clamp01(
            (profile.EconomicComplexity * 0.30)
            + (profile.CulturalAccumulation * 0.24)
            + (profile.TradeConnectivity * 0.18)
            + (serviceScale * 0.16)
            + (stationScale * 0.12));

        profile.AdoptionLagPressure = Clamp01(
            ((1.0 - profile.TradeConnectivity) * 0.28)
            + ((1.0 - profile.StateCapacity) * 0.22)
            + (securityPressure * 0.18)
            + ((1.0 - serviceScale) * 0.18)
            - (profile.TechnologyAdoptionCapacity * 0.20));

        profile.TechnologyAccessInequality = Clamp01(
            (profile.InventionCapacity * 0.24)
            + (profile.RestrictionPressure * 0.20)
            + (profile.FactionalFragmentation * 0.18)
            + (stationScale * 0.16)
            - (profile.InternalLegitimacy * 0.12)
            - (profile.EconomicComplexity * 0.08));

        double medianAccess = 0.82
            + (profile.TechnologyAdoptionCapacity * 0.10)
            - (profile.AdoptionLagPressure * 0.16)
            - (profile.TechnologyAccessInequality * 0.12);
        profile.MedianTechLevel = ResolveTechnologyLevelFromNormalized(medianAccess);
        if ((int)profile.MedianTechLevel > (int)profile.EliteTechLevel)
        {
            profile.MedianTechLevel = profile.EliteTechLevel;
        }
    }

    private static double ResolveGovernanceCapacity(SpaceStation station)
    {
        if (station.Government != null)
        {
            return Clamp01(station.Government.AdministrativeCapacity);
        }

        if (station.OutpostAuthority == OutpostAuthority.Type.Military
            || station.OutpostAuthority == OutpostAuthority.Type.Government)
        {
            return 0.72;
        }

        if (station.OutpostAuthority == OutpostAuthority.Type.Corporate
            || station.OutpostAuthority == OutpostAuthority.Type.Franchise)
        {
            return 0.62;
        }

        return 0.48;
    }

    private static double ResolveSpecialization(SpaceStation station)
    {
        if (station.PrimaryPurpose == StationPurpose.Purpose.Trade
            || station.PrimaryPurpose == StationPurpose.Purpose.Industrial
            || station.PrimaryPurpose == StationPurpose.Purpose.Administrative)
        {
            return 0.85;
        }

        if (station.PrimaryPurpose == StationPurpose.Purpose.Mining
            || station.PrimaryPurpose == StationPurpose.Purpose.Science
            || station.PrimaryPurpose == StationPurpose.Purpose.Military)
        {
            return 0.70;
        }

        if (station.PrimaryPurpose == StationPurpose.Purpose.Residential)
        {
            return 0.60;
        }

        return 0.50;
    }

    private static double ResolveTradeConnectivity(
        SpaceStation station,
        double serviceScale,
        double stationScale)
    {
        double score = 0.25 + (serviceScale * 0.25) + (stationScale * 0.20);
        if (station.OffersService(StationService.Service.Trade))
        {
            score += 0.16;
        }

        if (station.OffersService(StationService.Service.Refuel))
        {
            score += 0.10;
        }

        if (station.OffersService(StationService.Service.Shipyard))
        {
            score += 0.12;
        }

        if (station.StationType == StationType.Type.DeepSpace)
        {
            score -= 0.08;
        }

        return Clamp01(score);
    }

    private static double ResolveSecurityPressure(SpaceStation station)
    {
        double pressure = 0.15;
        if (station.PrimaryPurpose == StationPurpose.Purpose.Military)
        {
            pressure += 0.40;
        }

        if (station.PlacementContext == StationPlacementContext.Context.Strategic)
        {
            pressure += 0.25;
        }

        if (station.StationType == StationType.Type.DeepSpace)
        {
            pressure += 0.10;
        }

        return Clamp01(pressure);
    }

    private static double ResolveUrbanizationShare(SpaceStation station, SentientWorldProfile profile)
    {
        double logisticsBonus = 0.0;
        if (profile.LogisticsCapacity == "Interstellar Hub")
        {
            logisticsBonus = 0.06;
        }

        double share = 0.72 + (profile.SocialScale * 0.16) + logisticsBonus;
        if (station.PrimaryPurpose == StationPurpose.Purpose.Residential)
        {
            share += 0.06;
        }

        return Clamp01(share);
    }

    private static string ResolveSettlementPattern(SpaceStation station)
    {
        if (station.StationClass == StationClass.Class.S)
        {
            return "Arcology-Heavy";
        }

        if (station.PrimaryPurpose == StationPurpose.Purpose.Trade
            || station.PrimaryPurpose == StationPurpose.Purpose.Administrative)
        {
            return "Hub-and-Spoke";
        }

        if (station.PrimaryPurpose == StationPurpose.Purpose.Residential)
        {
            return "Dense Habitat";
        }

        if (station.StationType == StationType.Type.AsteroidBelt)
        {
            return "Industrial Cluster";
        }

        return "Compartmentalized";
    }

    private static string ResolvePrimarySettlementRank(SpaceStation station)
    {
        if (station.StationClass == StationClass.Class.U || station.StationClass == StationClass.Class.O)
        {
            return "Outpost";
        }

        if (station.StationClass == StationClass.Class.B)
        {
            return "Town";
        }

        if (station.StationClass == StationClass.Class.A)
        {
            return "City";
        }

        return "Metroplex";
    }

    private static string ResolveLogisticsCapacity(SentientWorldProfile profile)
    {
        double score = (profile.TradeConnectivity * 0.45)
            + (profile.StateCapacity * 0.20)
            + (profile.SocialScale * 0.15)
            + (profile.TechnologyAdoptionCapacity * 0.20);

        if (score >= 0.80)
        {
            return "Interstellar Hub";
        }

        if (score >= 0.60)
        {
            return "Major Port";
        }

        if (score >= 0.40)
        {
            return "Regional Port";
        }

        if (score >= 0.20)
        {
            return "Frontier Port";
        }

        return "Isolated";
    }

    private static double NormalizePopulation(int population)
    {
        if (population <= 0)
        {
            return 0.0;
        }

        double clampedPopulation = population;
        if (clampedPopulation > 10_000_000.0)
        {
            clampedPopulation = 10_000_000.0;
        }

        return Clamp01(System.Math.Log10(clampedPopulation + 1.0) / System.Math.Log10(10_000_001.0));
    }

    private static double NormalizeServiceCount(int serviceCount)
    {
        return Clamp01(serviceCount / 10.0);
    }

    private static double NormalizeStationClass(StationClass.Class stationClass)
    {
        if (stationClass == StationClass.Class.U)
        {
            return 0.15;
        }

        if (stationClass == StationClass.Class.O)
        {
            return 0.25;
        }

        if (stationClass == StationClass.Class.B)
        {
            return 0.50;
        }

        if (stationClass == StationClass.Class.A)
        {
            return 0.75;
        }

        return 1.0;
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

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
