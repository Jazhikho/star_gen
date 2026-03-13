using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Complete output of the station design calculator.
/// </summary>
public sealed class DesignResult
{
    public DesignSpec Spec = new();
    public ComponentSelection Selection = new();

    /// <summary>
    /// Preset catalog version used to build this design.
    /// </summary>
    public string CatalogVersion = string.Empty;

    public int StructureHitPoints;
    public int Hardpoints;
    public int HardpointsUsed;
    public int EffectiveArmorPoints;
    public int BerthsAvailable;

    public CrewBreakdown Crew = new();
    public TonnageBreakdown Tonnage = new();
    public CostBreakdown Cost = new();
    public PowerBudget Power = new();

    public List<string> Warnings = new();

    /// <summary>
    /// Serializes the full resolved design result.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<string> warnings = new();
        foreach (string warning in Warnings)
        {
            warnings.Add(warning);
        }

        return new Dictionary
        {
            ["spec"] = Spec.ToDictionary(),
            ["selection"] = Selection.ToDictionary(),
            ["catalog_version"] = CatalogVersion,
            ["structure_hit_points"] = StructureHitPoints,
            ["hardpoints"] = Hardpoints,
            ["hardpoints_used"] = HardpointsUsed,
            ["effective_armor_points"] = EffectiveArmorPoints,
            ["berths_available"] = BerthsAvailable,
            ["crew"] = ToDictionary(Crew),
            ["tonnage"] = ToDictionary(Tonnage),
            ["cost"] = ToDictionary(Cost),
            ["power"] = ToDictionary(Power),
            ["warnings"] = warnings,
        };
    }

    /// <summary>
    /// Rebuilds the full resolved design result from a dictionary payload.
    /// </summary>
    public static DesignResult FromDictionary(Dictionary? data)
    {
        DesignResult result = new();
        if (data == null)
        {
            return result;
        }

        result.Spec = DesignSpec.FromDictionary(GetDictionary(data, "spec"));
        result.Selection = ComponentSelection.FromDictionary(GetDictionary(data, "selection"));
        result.CatalogVersion = GetString(data, "catalog_version", string.Empty);
        result.StructureHitPoints = GetInt(data, "structure_hit_points", 0);
        result.Hardpoints = GetInt(data, "hardpoints", 0);
        result.HardpointsUsed = GetInt(data, "hardpoints_used", 0);
        result.EffectiveArmorPoints = GetInt(data, "effective_armor_points", 0);
        result.BerthsAvailable = GetInt(data, "berths_available", 0);
        result.Crew = FromCrewDictionary(GetDictionary(data, "crew"));
        result.Tonnage = FromTonnageDictionary(GetDictionary(data, "tonnage"));
        result.Cost = FromCostDictionary(GetDictionary(data, "cost"));
        result.Power = FromPowerDictionary(GetDictionary(data, "power"));
        result.Warnings = GetWarnings(data);
        return result;
    }

    /// <summary>
    /// Serializes the compact deterministic representation of the design.
    /// </summary>
    public Dictionary ToCompactDictionary()
    {
        return new Dictionary
        {
            ["catalog_version"] = CatalogVersion,
            ["spec"] = Spec.ToDictionary(),
        };
    }

    /// <summary>
    /// Rebuilds a design from the compact representation by recalculating it.
    /// </summary>
    public static DesignResult FromCompactDictionary(Dictionary? data)
    {
        if (data == null)
        {
            return new DesignResult();
        }

        Dictionary? specData = GetDictionary(data, "spec");
        DesignSpec spec = DesignSpec.FromDictionary(specData);
        DesignResult result = DesignCalculator.Calculate(spec);
        string storedCatalogVersion = GetString(data, "catalog_version", string.Empty);
        if (storedCatalogVersion.Length > 0 && storedCatalogVersion != result.CatalogVersion)
        {
            result.Warnings.Add(
                $"Regenerated from compact design using catalog {result.CatalogVersion}; source catalog was {storedCatalogVersion}.");
        }

        return result;
    }

    private static Dictionary ToDictionary(CrewBreakdown breakdown)
    {
        return new Dictionary
        {
            ["command"] = breakdown.Command,
            ["engineering"] = breakdown.Engineering,
            ["gunnery"] = breakdown.Gunnery,
            ["docking"] = breakdown.Docking,
            ["maintenance"] = breakdown.Maintenance,
            ["medical"] = breakdown.Medical,
            ["security"] = breakdown.Security,
            ["facilities"] = breakdown.Facilities,
            ["admin"] = breakdown.Admin,
            ["total"] = breakdown.Total,
        };
    }

    private static Dictionary ToDictionary(TonnageBreakdown breakdown)
    {
        return new Dictionary
        {
            ["armor"] = breakdown.Armor,
            ["command"] = breakdown.Command,
            ["sensors"] = breakdown.Sensors,
            ["power_plant"] = breakdown.PowerPlant,
            ["fuel"] = breakdown.Fuel,
            ["weapons"] = breakdown.Weapons,
            ["screens"] = breakdown.Screens,
            ["docking"] = breakdown.Docking,
            ["quarters"] = breakdown.Quarters,
            ["facilities"] = breakdown.Facilities,
            ["used"] = breakdown.Used,
            ["cargo"] = breakdown.Cargo,
        };
    }

    private static Dictionary ToDictionary(CostBreakdown breakdown)
    {
        return new Dictionary
        {
            ["hull"] = breakdown.Hull,
            ["armor"] = breakdown.Armor,
            ["command"] = breakdown.Command,
            ["computer"] = breakdown.Computer,
            ["sensors"] = breakdown.Sensors,
            ["power_plant"] = breakdown.PowerPlant,
            ["weapons"] = breakdown.Weapons,
            ["screens"] = breakdown.Screens,
            ["docking"] = breakdown.Docking,
            ["quarters"] = breakdown.Quarters,
            ["facilities"] = breakdown.Facilities,
            ["software"] = breakdown.Software,
            ["total"] = breakdown.Total,
        };
    }

    private static Dictionary ToDictionary(PowerBudget budget)
    {
        return new Dictionary
        {
            ["demand"] = budget.Demand,
            ["output"] = budget.Output,
            ["surplus"] = budget.Surplus,
        };
    }

    private static CrewBreakdown FromCrewDictionary(Dictionary? data)
    {
        CrewBreakdown breakdown = new();
        if (data == null)
        {
            return breakdown;
        }

        breakdown.Command = GetInt(data, "command", 0);
        breakdown.Engineering = GetInt(data, "engineering", 0);
        breakdown.Gunnery = GetInt(data, "gunnery", 0);
        breakdown.Docking = GetInt(data, "docking", 0);
        breakdown.Maintenance = GetInt(data, "maintenance", 0);
        breakdown.Medical = GetInt(data, "medical", 0);
        breakdown.Security = GetInt(data, "security", 0);
        breakdown.Facilities = GetInt(data, "facilities", 0);
        breakdown.Admin = GetInt(data, "admin", 0);
        breakdown.Total = GetInt(data, "total", 0);
        return breakdown;
    }

    private static TonnageBreakdown FromTonnageDictionary(Dictionary? data)
    {
        TonnageBreakdown breakdown = new();
        if (data == null)
        {
            return breakdown;
        }

        breakdown.Armor = GetInt(data, "armor", 0);
        breakdown.Command = GetInt(data, "command", 0);
        breakdown.Sensors = GetInt(data, "sensors", 0);
        breakdown.PowerPlant = GetInt(data, "power_plant", 0);
        breakdown.Fuel = GetInt(data, "fuel", 0);
        breakdown.Weapons = GetInt(data, "weapons", 0);
        breakdown.Screens = GetInt(data, "screens", 0);
        breakdown.Docking = GetInt(data, "docking", 0);
        breakdown.Quarters = GetInt(data, "quarters", 0);
        breakdown.Facilities = GetInt(data, "facilities", 0);
        breakdown.Used = GetInt(data, "used", 0);
        breakdown.Cargo = GetInt(data, "cargo", 0);
        return breakdown;
    }

    private static CostBreakdown FromCostDictionary(Dictionary? data)
    {
        CostBreakdown breakdown = new();
        if (data == null)
        {
            return breakdown;
        }

        breakdown.Hull = GetLong(data, "hull", 0L);
        breakdown.Armor = GetLong(data, "armor", 0L);
        breakdown.Command = GetLong(data, "command", 0L);
        breakdown.Computer = GetLong(data, "computer", 0L);
        breakdown.Sensors = GetLong(data, "sensors", 0L);
        breakdown.PowerPlant = GetLong(data, "power_plant", 0L);
        breakdown.Weapons = GetLong(data, "weapons", 0L);
        breakdown.Screens = GetLong(data, "screens", 0L);
        breakdown.Docking = GetLong(data, "docking", 0L);
        breakdown.Quarters = GetLong(data, "quarters", 0L);
        breakdown.Facilities = GetLong(data, "facilities", 0L);
        breakdown.Software = GetLong(data, "software", 0L);
        breakdown.Total = GetLong(data, "total", 0L);
        return breakdown;
    }

    private static PowerBudget FromPowerDictionary(Dictionary? data)
    {
        PowerBudget budget = new();
        if (data == null)
        {
            return budget;
        }

        budget.Demand = GetInt(data, "demand", 0);
        budget.Output = GetInt(data, "output", 0);
        budget.Surplus = GetInt(data, "surplus", 0);
        return budget;
    }

    private static List<string> GetWarnings(Dictionary data)
    {
        List<string> warnings = new();
        if (!data.ContainsKey("warnings"))
        {
            return warnings;
        }

        Variant value = data["warnings"];
        if (value.VariantType != Variant.Type.Array)
        {
            return warnings;
        }

        foreach (Variant rawWarning in (Array)value)
        {
            if (rawWarning.VariantType == Variant.Type.String)
            {
                warnings.Add(rawWarning.AsString());
            }
        }

        return warnings;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Int)
        {
            return fallback;
        }

        return (int)value;
    }

    private static long GetLong(Dictionary data, string key, long fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Int)
        {
            return fallback;
        }

        return (long)value;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.String)
        {
            return fallback;
        }

        return value.AsString();
    }

    private static Dictionary? GetDictionary(Dictionary data, string key)
    {
        if (!data.ContainsKey(key))
        {
            return null;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return (Dictionary)value;
    }
}
