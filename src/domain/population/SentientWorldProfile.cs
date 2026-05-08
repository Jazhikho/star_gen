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
    /// Highest elite or institutionally available technology access.
    /// </summary>
    public TechnologyLevel.Level EliteTechLevel = TechnologyLevel.Level.StoneAge;

    /// <summary>
    /// Approximate median technology access experienced by the active population.
    /// </summary>
    public TechnologyLevel.Level MedianTechLevel = TechnologyLevel.Level.StoneAge;

    /// <summary>
    /// Neutral 0-24 overall core technology level used by ruleset adapters.
    /// </summary>
    public int CoreTechLevel;

    /// <summary>
    /// Neutral 0-24 elite or institutionally available technology level.
    /// </summary>
    public int EliteCoreTechLevel;

    /// <summary>
    /// Neutral 0-24 median technology level experienced by the active population.
    /// </summary>
    public int MedianCoreTechLevel;

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
    /// Practical enforcement reach after logistics, terrain, and state capacity constraints.
    /// </summary>
    public double EnforcementReach;

    /// <summary>
    /// Pressure toward restrictive enforcement once law is formalized.
    /// </summary>
    public double RestrictionPressure;

    /// <summary>
    /// Compressed neutral 0-15 law code for ruleset adapters.
    /// </summary>
    public int LawLevel;

    /// <summary>
    /// Compact interpretation of the neutral legal order.
    /// </summary>
    public string LawInterpretation = string.Empty;

    /// <summary>
    /// Compact neutral description of how legal authority is partitioned across the world.
    /// </summary>
    public string JurisdictionStructure = string.Empty;

    /// <summary>
    /// Pressure toward plural or overlapping legal authorities in the inclusive range [0, 1].
    /// </summary>
    public double JurisdictionPluralism;

    /// <summary>
    /// Pressure from contested jurisdictional authority in the inclusive range [0, 1].
    /// </summary>
    public double JurisdictionConflict;

    /// <summary>
    /// Capacity to preserve, transmit, and accumulate complex cultural knowledge.
    /// </summary>
    public double CulturalAccumulation;

    /// <summary>
    /// Capacity to adopt and scale new techniques rather than merely invent them locally.
    /// </summary>
    public double TechnologyAdoptionCapacity;

    /// <summary>
    /// Capacity for local invention and high-complexity production, separate from adoption.
    /// </summary>
    public double InventionCapacity;

    /// <summary>
    /// Pressure delaying broad adoption after a technology is known or imported.
    /// </summary>
    public double AdoptionLagPressure;

    /// <summary>
    /// Gap between elite/peak access and median technology access.
    /// </summary>
    public double TechnologyAccessInequality;

    /// <summary>
    /// Degree of fragmentation among elites, settlements, and active populations.
    /// </summary>
    public double FactionalFragmentation;

    /// <summary>
    /// Structural centralization potential for religious institutions.
    /// </summary>
    public double ReligiousCentralization;

    /// <summary>
    /// First-class neutral factions inferred from population and governance structure.
    /// </summary>
    public Array<SentientFactionRecord> Factions = new();

    /// <summary>
    /// Structured cultural feature tags only; no doctrine or prose culture generation.
    /// </summary>
    public Array<string> CulturalFeatureTags = new();

    /// <summary>
    /// Institutional religion structure only.
    /// </summary>
    public string ReligionStructure = string.Empty;

    /// <summary>
    /// Life-supportable biome readout, only populated for worlds that pass the native-life gate.
    /// </summary>
    public Array<string> AvailableLifeBiomes = new();

    /// <summary>
    /// Proxy for capability breadth, relatedness, and production readiness.
    /// </summary>
    public double EconomicComplexity;

    /// <summary>
    /// Internal legitimacy proxy for acceptance by local populations.
    /// </summary>
    public double InternalLegitimacy;

    /// <summary>
    /// External legitimacy proxy for acceptance by connected outside actors.
    /// </summary>
    public double ExternalLegitimacy;

    /// <summary>
    /// Whether this profile uses social-science proxies that require human audit before release claims.
    /// </summary>
    public bool HumanAuditRequired;

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
        data["elite_tech_level"] = (int)EliteTechLevel;
        data["median_tech_level"] = (int)MedianTechLevel;
        data["core_tech_level"] = TechnologyLevel.ClampCoreLevel(CoreTechLevel);
        data["elite_core_tech_level"] = TechnologyLevel.ClampCoreLevel(EliteCoreTechLevel);
        data["median_core_tech_level"] = TechnologyLevel.ClampCoreLevel(MedianCoreTechLevel);
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
        data["enforcement_reach"] = EnforcementReach;
        data["restriction_pressure"] = RestrictionPressure;
        data["law_level"] = System.Math.Clamp(LawLevel, 0, 15);
        data["law_interpretation"] = LawInterpretation;
        data["jurisdiction_structure"] = JurisdictionStructure;
        data["jurisdiction_pluralism"] = JurisdictionPluralism;
        data["jurisdiction_conflict"] = JurisdictionConflict;
        data["cultural_accumulation"] = CulturalAccumulation;
        data["technology_adoption_capacity"] = TechnologyAdoptionCapacity;
        data["invention_capacity"] = InventionCapacity;
        data["adoption_lag_pressure"] = AdoptionLagPressure;
        data["technology_access_inequality"] = TechnologyAccessInequality;
        data["factional_fragmentation"] = FactionalFragmentation;
        data["religious_centralization"] = ReligiousCentralization;
        Array<Dictionary> factionData = new();
        foreach (SentientFactionRecord faction in Factions)
        {
            factionData.Add(faction.ToDictionary());
        }

        data["factions"] = factionData;
        data["cultural_feature_tags"] = CloneStringArray(CulturalFeatureTags);
        data["religion_structure"] = ReligionStructure;
        data["available_life_biomes"] = CloneStringArray(AvailableLifeBiomes);
        data["economic_complexity"] = EconomicComplexity;
        data["internal_legitimacy"] = InternalLegitimacy;
        data["external_legitimacy"] = ExternalLegitimacy;
        data["human_audit_required"] = HumanAuditRequired;
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
        profile.EliteTechLevel = (TechnologyLevel.Level)GetInt(data, "elite_tech_level", (int)profile.HighestTechLevel);
        profile.MedianTechLevel = (TechnologyLevel.Level)GetInt(data, "median_tech_level", (int)profile.HighestTechLevel);
        profile.EliteCoreTechLevel = TechnologyLevel.ClampCoreLevel(GetInt(
            data,
            "elite_core_tech_level",
            TechnologyLevel.EraToRepresentativeCoreLevel(profile.EliteTechLevel)));
        profile.MedianCoreTechLevel = TechnologyLevel.ClampCoreLevel(GetInt(
            data,
            "median_core_tech_level",
            TechnologyLevel.EraToRepresentativeCoreLevel(profile.MedianTechLevel)));
        profile.CoreTechLevel = TechnologyLevel.ClampCoreLevel(GetInt(
            data,
            "core_tech_level",
            profile.MedianCoreTechLevel));
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
        profile.EnforcementReach = Clamp01(GetDouble(data, "enforcement_reach", profile.LegalReach));
        profile.RestrictionPressure = Clamp01(GetDouble(data, "restriction_pressure", 0.0));
        int derivedLawLevel = DeriveLawLevel(
            profile.LegalReach,
            profile.EnforcementReach,
            profile.LegalCentralization,
            profile.RestrictionPressure,
            profile.StateCapacity,
            profile.ExternalThreat,
            0.0);
        profile.LawLevel = System.Math.Clamp(GetInt(data, "law_level", derivedLawLevel), 0, 15);
        profile.LawInterpretation = GetString(
            data,
            "law_interpretation",
            DeriveLawInterpretation(
                profile.LegalReach,
                profile.EnforcementReach,
                profile.LegalCentralization,
                profile.RestrictionPressure,
                profile.StateCapacity));
        profile.CulturalAccumulation = Clamp01(GetDouble(data, "cultural_accumulation", 0.0));
        profile.TechnologyAdoptionCapacity = Clamp01(GetDouble(data, "technology_adoption_capacity", 0.0));
        profile.InventionCapacity = Clamp01(GetDouble(data, "invention_capacity", 0.0));
        profile.AdoptionLagPressure = Clamp01(GetDouble(data, "adoption_lag_pressure", 0.0));
        profile.TechnologyAccessInequality = Clamp01(GetDouble(data, "technology_access_inequality", 0.0));
        profile.FactionalFragmentation = Clamp01(GetDouble(data, "factional_fragmentation", 0.0));
        profile.ReligiousCentralization = Clamp01(GetDouble(data, "religious_centralization", 0.0));
        profile.Factions = ParseFactionArray(data, "factions");
        profile.CulturalFeatureTags = ParseStringArray(data, "cultural_feature_tags");
        profile.ReligionStructure = GetString(data, "religion_structure", DeriveReligionStructure(profile));
        profile.AvailableLifeBiomes = ParseStringArray(data, "available_life_biomes");
        profile.EconomicComplexity = Clamp01(GetDouble(data, "economic_complexity", 0.0));
        profile.InternalLegitimacy = Clamp01(GetDouble(data, "internal_legitimacy", 0.0));
        profile.ExternalLegitimacy = Clamp01(GetDouble(data, "external_legitimacy", 0.0));
        profile.JurisdictionPluralism = Clamp01(GetDouble(data, "jurisdiction_pluralism", DeriveJurisdictionPluralism(
            profile.LegalCentralization,
            profile.FactionalFragmentation,
            0.0,
            0.0,
            0.0)));
        profile.JurisdictionConflict = Clamp01(GetDouble(data, "jurisdiction_conflict", DeriveJurisdictionConflict(
            profile.FactionalFragmentation,
            profile.ExternalThreat,
            profile.InternalLegitimacy,
            0.0,
            profile.RestrictionPressure,
            profile.StateCapacity)));
        profile.JurisdictionStructure = GetString(
            data,
            "jurisdiction_structure",
            DeriveJurisdictionStructure(
                profile.LegalCentralization,
                profile.StateCapacity,
                profile.TradeConnectivity,
                profile.JurisdictionPluralism,
                profile.JurisdictionConflict,
                0.0,
                0.0));
        profile.HumanAuditRequired = GetBool(data, "human_audit_required", false);
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
        summary["elite_tech_level"] = TechnologyLevel.ToStringName(EliteTechLevel);
        summary["median_tech_level"] = TechnologyLevel.ToStringName(MedianTechLevel);
        summary["core_tech_level"] = CoreTechLevel;
        summary["elite_core_tech_level"] = EliteCoreTechLevel;
        summary["median_core_tech_level"] = MedianCoreTechLevel;
        summary["dominant_regime"] = GovernmentType.ToStringName(DominantRegime);
        summary["law_level"] = LawLevel;
        summary["law_interpretation"] = LawInterpretation;
        summary["jurisdiction_structure"] = JurisdictionStructure;
        summary["jurisdiction_pluralism"] = JurisdictionPluralism;
        summary["jurisdiction_conflict"] = JurisdictionConflict;
        summary["social_scale"] = SocialScale;
        summary["state_capacity"] = StateCapacity;
        summary["enforcement_reach"] = EnforcementReach;
        summary["trade_connectivity"] = TradeConnectivity;
        summary["economic_complexity"] = EconomicComplexity;
        summary["invention_capacity"] = InventionCapacity;
        summary["adoption_lag_pressure"] = AdoptionLagPressure;
        summary["technology_access_inequality"] = TechnologyAccessInequality;
        summary["religion_structure"] = ReligionStructure;
        summary["human_audit_required"] = HumanAuditRequired;
        return summary;
    }

    /// <summary>
    /// Derives the neutral compressed law level from legal and coercive reach.
    /// </summary>
    public static int DeriveLawLevel(
        double legalReach,
        double enforcementReach,
        double legalCentralization,
        double restrictionPressure,
        double stateCapacity,
        double threat,
        double regimeCoercion)
    {
        double score = (Clamp01(legalReach) * 3.2)
            + (Clamp01(enforcementReach) * 3.0)
            + (Clamp01(legalCentralization) * 2.0)
            + (Clamp01(restrictionPressure) * 3.0)
            + (Clamp01(stateCapacity) * 2.0)
            + (Clamp01(threat) * 0.9)
            + (Clamp01(regimeCoercion) * 1.4);
        return System.Math.Clamp((int)System.Math.Round(score), 0, 15);
    }

    /// <summary>
    /// Derives a compact legal-order interpretation.
    /// </summary>
    public static string DeriveLawInterpretation(
        double legalReach,
        double enforcementReach,
        double legalCentralization,
        double restrictionPressure,
        double stateCapacity)
    {
        if (legalReach < 0.12 && stateCapacity < 0.18)
        {
            return "No Formal Reach";
        }

        if (legalCentralization < 0.24 || legalReach < 0.25)
        {
            return "Plural/Customary";
        }

        if (enforcementReach < 0.35)
        {
            return "Patchy Formal Law";
        }

        if (restrictionPressure >= 0.65 && enforcementReach >= 0.55)
        {
            return "Restrictive High-Enforcement Order";
        }

        if (stateCapacity >= 0.65 && legalReach >= 0.60)
        {
            return "High-Capacity Legal Order";
        }

        return "Codified Moderate Reach";
    }

    /// <summary>
    /// Derives plural or overlapping jurisdiction pressure.
    /// </summary>
    public static double DeriveJurisdictionPluralism(
        double legalCentralization,
        double factionalFragmentation,
        double coexistencePressure,
        double terrainFragmentation,
        double groupScale)
    {
        return Clamp01(((1.0 - Clamp01(legalCentralization)) * 0.34)
            + (Clamp01(factionalFragmentation) * 0.24)
            + (Clamp01(coexistencePressure) * 0.20)
            + (Clamp01(terrainFragmentation) * 0.12)
            + (Clamp01(groupScale) * 0.10));
    }

    /// <summary>
    /// Derives contested jurisdiction pressure.
    /// </summary>
    public static double DeriveJurisdictionConflict(
        double factionalFragmentation,
        double externalThreat,
        double internalLegitimacy,
        double coexistencePressure,
        double restrictionPressure,
        double stateCapacity)
    {
        return Clamp01((Clamp01(factionalFragmentation) * 0.30)
            + (Clamp01(externalThreat) * 0.24)
            + ((1.0 - Clamp01(internalLegitimacy)) * 0.18)
            + (Clamp01(coexistencePressure) * 0.16)
            + (Clamp01(restrictionPressure) * 0.12)
            - (Clamp01(stateCapacity) * 0.12));
    }

    /// <summary>
    /// Derives the neutral jurisdiction structure label.
    /// </summary>
    public static string DeriveJurisdictionStructure(
        double legalCentralization,
        double stateCapacity,
        double tradeConnectivity,
        double jurisdictionPluralism,
        double jurisdictionConflict,
        double colonyShare,
        double nativeShare)
    {
        if (stateCapacity < 0.18 && legalCentralization < 0.18)
        {
            return "Informal Local";
        }

        if (jurisdictionConflict >= 0.56)
        {
            return "Contested Jurisdictions";
        }

        if (jurisdictionPluralism >= 0.55 && legalCentralization < 0.46)
        {
            return "Layered Customary";
        }

        if (colonyShare >= 0.10 && nativeShare >= 0.10 && tradeConnectivity >= 0.42)
        {
            return "Charter/Federal";
        }

        if (stateCapacity >= 0.70 && legalCentralization >= 0.66 && tradeConnectivity >= 0.50)
        {
            return "Centralized Unitary";
        }

        if (tradeConnectivity >= 0.62 && stateCapacity >= 0.48)
        {
            return "Extraterritorial/Imperial";
        }

        return "Patchwork Formal";
    }

    /// <summary>
    /// Derives institutional religion structure from neutral profile axes.
    /// </summary>
    public static string DeriveReligionStructure(SentientWorldProfile profile)
    {
        if (profile.ReligiousCentralization < 0.12 && profile.CulturalAccumulation < 0.20)
        {
            return "None";
        }

        if (profile.RestrictionPressure >= 0.72 && profile.ReligiousCentralization < 0.45)
        {
            return "Suppressed";
        }

        if (profile.DominantRegime == GovernmentType.Regime.Theocracy
            || (profile.ReligiousCentralization >= 0.66 && profile.LegalCentralization >= 0.56))
        {
            return "State-Aligned";
        }

        if (profile.ReligiousCentralization >= 0.62)
        {
            return "Centralized";
        }

        if (profile.FactionalFragmentation >= 0.34 || profile.TradeConnectivity >= 0.48)
        {
            return "Plural";
        }

        return "Localized";
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

    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Bool)
        {
            return (bool)value;
        }

        return fallback;
    }

    private static Array<string> CloneStringArray(Array<string> source)
    {
        Array<string> clone = new();
        foreach (string value in source)
        {
            clone.Add(value);
        }

        return clone;
    }

    private static Array<string> ParseStringArray(Dictionary data, string key)
    {
        Array<string> values = new();
        if (!data.ContainsKey(key))
        {
            return values;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Array)
        {
            return values;
        }

        Array array = (Array)value;
        foreach (Variant item in array)
        {
            if (item.VariantType == Variant.Type.String)
            {
                values.Add((string)item);
            }
        }

        return values;
    }

    private static Array<SentientFactionRecord> ParseFactionArray(Dictionary data, string key)
    {
        Array<SentientFactionRecord> values = new();
        if (!data.ContainsKey(key))
        {
            return values;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Array)
        {
            return values;
        }

        Array array = (Array)value;
        foreach (Variant item in array)
        {
            if (item.VariantType == Variant.Type.Dictionary)
            {
                values.Add(SentientFactionRecord.FromDictionary((Dictionary)item));
            }
        }

        return values;
    }
}
