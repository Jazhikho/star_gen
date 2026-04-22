using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Neutral sentient-world baseline used by compatibility adapters and inspectors.
/// </summary>
public partial class SentientWorldProfile : RefCounted
{
    /// <summary>
    /// Total active population represented by the profile.
    /// </summary>
    public int TotalPopulation;

    /// <summary>
    /// Total extant native population represented by the profile.
    /// </summary>
    public int NativePopulation;

    /// <summary>
    /// Total active colony population represented by the profile.
    /// </summary>
    public int ColonyPopulation;

    /// <summary>
    /// Highest active technology level present on the world.
    /// </summary>
    public TechnologyLevel.Level HighestTechLevel = TechnologyLevel.Level.StoneAge;

    /// <summary>
    /// Dominant regime among the active populations.
    /// </summary>
    public GovernmentType.Regime DominantRegime = GovernmentType.Regime.Tribal;

    /// <summary>
    /// Settlement morphology inferred from geography, trade, and infrastructure.
    /// </summary>
    public string SettlementPattern = string.Empty;

    /// <summary>
    /// Share of the active population living in dense settlements.
    /// </summary>
    public double UrbanizationShare;

    /// <summary>
    /// Rank of the primary settlement tier on the world.
    /// </summary>
    public string PrimarySettlementRank = string.Empty;

    /// <summary>
    /// Neutral logistics or port-capacity baseline for ruleset adapters.
    /// </summary>
    public string LogisticsCapacity = string.Empty;

    /// <summary>
    /// Scale of population and institutions supported on the world.
    /// </summary>
    public double SocialScale;

    /// <summary>
    /// Surplus available for specialization, extraction, and state support.
    /// </summary>
    public double SurplusBase;

    /// <summary>
    /// Connectivity to external exchange, migration, and technology flows.
    /// </summary>
    public double TradeConnectivity;

    /// <summary>
    /// Pressure from frontier instability, conflict, or contested control.
    /// </summary>
    public double ExternalThreat;

    /// <summary>
    /// Administrative and coercive reach of the active institutions.
    /// </summary>
    public double StateCapacity;

    /// <summary>
    /// Degree to which governance depends on bargaining with the governed.
    /// </summary>
    public double FiscalContract;

    /// <summary>
    /// Degree to which dispute resolution is centralized and codified.
    /// </summary>
    public double LegalCentralization;

    /// <summary>
    /// Effective reach of formal law across the world.
    /// </summary>
    public double LegalReach;

    /// <summary>
    /// Pressure toward restrictive enforcement once law is formalized.
    /// </summary>
    public double RestrictionPressure;

    /// <summary>
    /// Capacity to preserve, transmit, and accumulate complex cultural knowledge.
    /// </summary>
    public double CulturalAccumulation;

    /// <summary>
    /// Capacity to adopt and scale new techniques rather than merely invent them locally.
    /// </summary>
    public double TechnologyAdoptionCapacity;

    /// <summary>
    /// Degree of fragmentation among elites, settlements, and active populations.
    /// </summary>
    public double FactionalFragmentation;

    /// <summary>
    /// Structural centralization potential for religious institutions.
    /// </summary>
    public double ReligiousCentralization;

    /// <summary>
    /// Converts this profile to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new();
        data["total_population"] = TotalPopulation;
        data["native_population"] = NativePopulation;
        data["colony_population"] = ColonyPopulation;
        data["highest_tech_level"] = (int)HighestTechLevel;
        data["dominant_regime"] = (int)DominantRegime;
        data["settlement_pattern"] = SettlementPattern;
        data["urbanization_share"] = UrbanizationShare;
        data["primary_settlement_rank"] = PrimarySettlementRank;
        data["logistics_capacity"] = LogisticsCapacity;
        data["social_scale"] = SocialScale;
        data["surplus_base"] = SurplusBase;
        data["trade_connectivity"] = TradeConnectivity;
        data["external_threat"] = ExternalThreat;
        data["state_capacity"] = StateCapacity;
        data["fiscal_contract"] = FiscalContract;
        data["legal_centralization"] = LegalCentralization;
        data["legal_reach"] = LegalReach;
        data["restriction_pressure"] = RestrictionPressure;
        data["cultural_accumulation"] = CulturalAccumulation;
        data["technology_adoption_capacity"] = TechnologyAdoptionCapacity;
        data["factional_fragmentation"] = FactionalFragmentation;
        data["religious_centralization"] = ReligiousCentralization;
        return data;
    }

    /// <summary>
    /// Creates a profile from a dictionary payload.
    /// </summary>
    public static SentientWorldProfile FromDictionary(Dictionary data)
    {
        SentientWorldProfile profile = new();
        profile.TotalPopulation = GetInt(data, "total_population", 0);
        profile.NativePopulation = GetInt(data, "native_population", 0);
        profile.ColonyPopulation = GetInt(data, "colony_population", 0);
        profile.HighestTechLevel = (TechnologyLevel.Level)GetInt(data, "highest_tech_level", 0);
        profile.DominantRegime = (GovernmentType.Regime)GetInt(data, "dominant_regime", 0);
        profile.SettlementPattern = GetString(data, "settlement_pattern", string.Empty);
        profile.UrbanizationShare = Clamp01(GetDouble(data, "urbanization_share", 0.0));
        profile.PrimarySettlementRank = GetString(data, "primary_settlement_rank", string.Empty);
        profile.LogisticsCapacity = GetString(data, "logistics_capacity", string.Empty);
        profile.SocialScale = Clamp01(GetDouble(data, "social_scale", 0.0));
        profile.SurplusBase = Clamp01(GetDouble(data, "surplus_base", 0.0));
        profile.TradeConnectivity = Clamp01(GetDouble(data, "trade_connectivity", 0.0));
        profile.ExternalThreat = Clamp01(GetDouble(data, "external_threat", 0.0));
        profile.StateCapacity = Clamp01(GetDouble(data, "state_capacity", 0.0));
        profile.FiscalContract = Clamp01(GetDouble(data, "fiscal_contract", 0.0));
        profile.LegalCentralization = Clamp01(GetDouble(data, "legal_centralization", 0.0));
        profile.LegalReach = Clamp01(GetDouble(data, "legal_reach", 0.0));
        profile.RestrictionPressure = Clamp01(GetDouble(data, "restriction_pressure", 0.0));
        profile.CulturalAccumulation = Clamp01(GetDouble(data, "cultural_accumulation", 0.0));
        profile.TechnologyAdoptionCapacity = Clamp01(GetDouble(data, "technology_adoption_capacity", 0.0));
        profile.FactionalFragmentation = Clamp01(GetDouble(data, "factional_fragmentation", 0.0));
        profile.ReligiousCentralization = Clamp01(GetDouble(data, "religious_centralization", 0.0));
        return profile;
    }

    /// <summary>
    /// Returns a compact summary dictionary.
    /// </summary>
    public Dictionary GetSummary()
    {
        Dictionary summary = new();
        summary["settlement_pattern"] = SettlementPattern;
        summary["primary_settlement_rank"] = PrimarySettlementRank;
        summary["logistics_capacity"] = LogisticsCapacity;
        summary["highest_tech_level"] = TechnologyLevel.ToStringName(HighestTechLevel);
        summary["dominant_regime"] = GovernmentType.ToStringName(DominantRegime);
        summary["social_scale"] = SocialScale;
        summary["state_capacity"] = StateCapacity;
        summary["trade_connectivity"] = TradeConnectivity;
        return summary;
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (int)(double)value;
        }

        if (value.VariantType == Variant.Type.String)
        {
            if (int.TryParse((string)value, out int parsed))
            {
                return parsed;
            }
        }

        return fallback;
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Float)
        {
            return (double)value;
        }

        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.String)
        {
            if (double.TryParse((string)value, out double parsed))
            {
                return parsed;
            }
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.String)
        {
            return (string)value;
        }

        return fallback;
    }
}
