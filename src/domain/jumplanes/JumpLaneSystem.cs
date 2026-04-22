using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// System record used for jump-lane calculations.
/// </summary>
public partial class JumpLaneSystem : RefCounted
{
    /// <summary>
    /// Unique system identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Position in parsecs.
    /// </summary>
    public Vector3 Position = Vector3.Zero;

    /// <summary>
    /// Population, where zero means unpopulated.
    /// </summary>
    public int Population;

    /// <summary>
    /// Synthetic population when used as a bridge, or -1 when unset.
    /// </summary>
    public int FalsePopulation = -1;

    /// <summary>
    /// Whether this system is acting as a bridge.
    /// </summary>
    public bool IsBridge;

    /// <summary>
    /// Traveller route and mainworld data when the region was built in Traveller mode.
    /// </summary>
    public TravellerSystemProfile? TravellerProfile;

    /// <summary>
    /// Whether this system can export interstellar colonists in realistic mode.
    /// </summary>
    public bool CanExportColonists;

    /// <summary>
    /// Export pressure in the inclusive range [0, 1].
    /// </summary>
    public double ExportPressure;

    /// <summary>
    /// Best colony-target desirability score in the inclusive range [0, 1].
    /// </summary>
    public double ColonyTargetScore;

    /// <summary>
    /// Best colony-target carrying capacity within the system.
    /// </summary>
    public int ColonyTargetCapacity;

    /// <summary>
    /// Interstellar colonization range available to this system in parsecs.
    /// </summary>
    public double ColonizationRangePc;

    /// <summary>
    /// Highest exporting technology level recorded for this system, or -1 when unavailable.
    /// </summary>
    public int RouteTechnologyLevel = -1;

    /// <summary>
    /// Source body identifier for exports from this system.
    /// </summary>
    public string ExportBodyId = string.Empty;

    /// <summary>
    /// Preferred colonization target body identifier in this system.
    /// </summary>
    public string ColonyTargetBodyId = string.Empty;

    /// <summary>
    /// Exporting civilization identifier for this system.
    /// </summary>
    public string ExportCivilizationId = string.Empty;

    /// <summary>
    /// Exporting civilization display name for this system.
    /// </summary>
    public string ExportCivilizationName = string.Empty;

    /// <summary>
    /// Best-fit human habitability score for the system's strongest focal world, or -1 when unavailable.
    /// </summary>
    public int HabitabilityScore = -1;

    /// <summary>
    /// Best-fit resource score for the system's strongest focal world, or -1 when unavailable.
    /// </summary>
    public int ResourceScore = -1;

    /// <summary>
    /// Display summary of the strongest active government in the system.
    /// </summary>
    public string GovernmentSummary = string.Empty;

    /// <summary>
    /// Display summary of route-relevant trade codes when available.
    /// </summary>
    public string TradeCodesSummary = string.Empty;

    /// <summary>
    /// Creates a new jump-lane system.
    /// </summary>
    public JumpLaneSystem(string id = "", Vector3 position = default, int population = 0)
    {
        Id = id;
        Position = position;
        Population = population;
    }

    /// <summary>
    /// Returns the effective population used by sorting and linking.
    /// </summary>
    public int GetEffectivePopulation()
    {
        if (FalsePopulation >= 0)
        {
            return FalsePopulation;
        }

        return Population;
    }

    /// <summary>
    /// Returns whether the system is populated.
    /// </summary>
    public bool IsPopulated()
    {
        return Population > 0;
    }

    /// <summary>
    /// Calculates distance to another system in parsecs.
    /// </summary>
    public double DistanceTo(JumpLaneSystem other)
    {
        return Position.DistanceTo(other.Position);
    }

    /// <summary>
    /// Marks this system as a bridge with a synthetic population.
    /// </summary>
    public void MakeBridge(int higherPopulation)
    {
        IsBridge = true;
        FalsePopulation = higherPopulation - 10000;
        if (FalsePopulation < 0)
        {
            FalsePopulation = 0;
        }
    }

    /// <summary>
    /// Converts the system to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new Dictionary
        {
            ["id"] = Id,
            ["position"] = new Dictionary
            {
                ["x"] = Position.X,
                ["y"] = Position.Y,
                ["z"] = Position.Z,
            },
            ["population"] = Population,
            ["false_population"] = FalsePopulation,
            ["is_bridge"] = IsBridge,
            ["can_export_colonists"] = CanExportColonists,
            ["export_pressure"] = ExportPressure,
            ["colony_target_score"] = ColonyTargetScore,
            ["colony_target_capacity"] = ColonyTargetCapacity,
            ["colonization_range_pc"] = ColonizationRangePc,
            ["route_technology_level"] = RouteTechnologyLevel,
            ["export_body_id"] = ExportBodyId,
            ["colony_target_body_id"] = ColonyTargetBodyId,
            ["export_civilization_id"] = ExportCivilizationId,
            ["export_civilization_name"] = ExportCivilizationName,
            ["habitability_score"] = HabitabilityScore,
            ["resource_score"] = ResourceScore,
            ["government_summary"] = GovernmentSummary,
            ["trade_codes_summary"] = TradeCodesSummary,
        };

        if (TravellerProfile != null)
        {
            data["traveller_profile"] = TravellerProfile.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Creates a system from a dictionary payload.
    /// </summary>
    public static JumpLaneSystem FromDictionary(Dictionary data)
    {
        Vector3 position = Vector3.Zero;
        if (data.ContainsKey("position") && data["position"].VariantType == Variant.Type.Dictionary)
        {
            Dictionary positionData = (Dictionary)data["position"];
            position = new Vector3(
                (float)DomainDictionaryUtils.GetDouble(positionData, "x", 0.0),
                (float)DomainDictionaryUtils.GetDouble(positionData, "y", 0.0),
                (float)DomainDictionaryUtils.GetDouble(positionData, "z", 0.0));
        }

        JumpLaneSystem system = new(
            DomainDictionaryUtils.GetString(data, "id", string.Empty),
            position,
            DomainDictionaryUtils.GetInt(data, "population", 0));
        system.FalsePopulation = DomainDictionaryUtils.GetInt(data, "false_population", -1);
        system.IsBridge = DomainDictionaryUtils.GetBool(data, "is_bridge", false);
        system.CanExportColonists = DomainDictionaryUtils.GetBool(data, "can_export_colonists", false);
        system.ExportPressure = DomainDictionaryUtils.GetDouble(data, "export_pressure", 0.0);
        system.ColonyTargetScore = DomainDictionaryUtils.GetDouble(data, "colony_target_score", 0.0);
        system.ColonyTargetCapacity = DomainDictionaryUtils.GetInt(data, "colony_target_capacity", 0);
        system.ColonizationRangePc = DomainDictionaryUtils.GetDouble(data, "colonization_range_pc", 0.0);
        system.RouteTechnologyLevel = DomainDictionaryUtils.GetInt(data, "route_technology_level", -1);
        system.ExportBodyId = DomainDictionaryUtils.GetString(data, "export_body_id", string.Empty);
        system.ColonyTargetBodyId = DomainDictionaryUtils.GetString(data, "colony_target_body_id", string.Empty);
        system.ExportCivilizationId = DomainDictionaryUtils.GetString(data, "export_civilization_id", string.Empty);
        system.ExportCivilizationName = DomainDictionaryUtils.GetString(data, "export_civilization_name", string.Empty);
        system.HabitabilityScore = DomainDictionaryUtils.GetInt(data, "habitability_score", -1);
        system.ResourceScore = DomainDictionaryUtils.GetInt(data, "resource_score", -1);
        system.GovernmentSummary = DomainDictionaryUtils.GetString(data, "government_summary", string.Empty);
        system.TradeCodesSummary = DomainDictionaryUtils.GetString(data, "trade_codes_summary", string.Empty);
        if (data.ContainsKey("traveller_profile") && data["traveller_profile"].VariantType == Variant.Type.Dictionary)
        {
            system.TravellerProfile = TravellerSystemProfile.FromDictionary((Dictionary)data["traveller_profile"]);
        }
        return system;
    }

}
