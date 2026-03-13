using System;
using System.Collections.Generic;

namespace StarGen.Domain.Population.StationDesign.Classification;

/// <summary>
/// Evaluates a station design against all Traveller classification rules.
/// </summary>
public static class ClassificationEvaluator
{
    private sealed class Requirement
    {
        public string Label = string.Empty;
        public Func<ClassificationContext, bool> Check = static _ => false;
    }

    private sealed class Rule
    {
        public ClassificationId Id;
        public string DisplayName = string.Empty;
        public string Icon = string.Empty;
        public Requirement[] Requirements = Array.Empty<Requirement>();
    }

    private static readonly Dictionary<ClassificationId, Rule> Rules = BuildRules();

    /// <summary>
    /// Evaluates a design result and returns the full classification report.
    /// </summary>
    public static ClassificationReport Evaluate(DesignResult design)
    {
        return Evaluate(ClassificationContext.FromResult(design));
    }

    /// <summary>
    /// Evaluates a prepared context and returns the full classification report.
    /// </summary>
    public static ClassificationReport Evaluate(ClassificationContext context)
    {
        ClassificationReport report = new();

        foreach (KeyValuePair<ClassificationId, Rule> entry in Rules)
        {
            Rule rule = entry.Value;
            ClassificationResult result = new()
            {
                Id = rule.Id,
                DisplayName = rule.DisplayName,
                Icon = rule.Icon,
                TotalCount = rule.Requirements.Length,
            };

            int metCount = 0;
            foreach (Requirement requirement in rule.Requirements)
            {
                bool passed = requirement.Check(context);
                result.Requirements.Add(new RequirementResult
                {
                    Label = requirement.Label,
                    Met = passed,
                });

                if (passed)
                {
                    metCount += 1;
                }
            }

            result.MetCount = metCount;
            result.Earned = metCount == rule.Requirements.Length;
            report.Results[rule.Id] = result;

            if (result.Earned)
            {
                report.Earned.Add(rule.Id);
            }
        }

        return report;
    }

    private static Requirement Q(string label, Func<ClassificationContext, bool> check)
    {
        return new Requirement
        {
            Label = label,
            Check = check,
        };
    }

    private static Rule R(ClassificationId id, string name, string icon, params Requirement[] requirements)
    {
        return new Rule
        {
            Id = id,
            DisplayName = name,
            Icon = icon,
            Requirements = requirements,
        };
    }

    private static Dictionary<ClassificationId, Rule> BuildRules()
    {
        Dictionary<ClassificationId, Rule> rules = new();

        rules[ClassificationId.StarportA] = R(ClassificationId.StarportA, "Class A Starport", "A",
            Q("Shipyard >= 1,000t capacity", c => c.Facilities[FacilityKind.ShipyardMedium] + c.Facilities[FacilityKind.ShipyardLarge] >= 1),
            Q("Fuel refinery >= 1", c => c.Facilities[FacilityKind.FuelRefinery] >= 1),
            Q("Fuel depot >= 1", c => c.Facilities[FacilityKind.FuelDepot] >= 1),
            Q("Large or Capital berth >= 1", c => c.Docking[DockingBerthKind.LargeBerth] + c.Docking[DockingBerthKind.CapitalBerth] >= 1),
            Q("Standard berths >= 4", c => c.Docking[DockingBerthKind.StandardBerth] >= 4),
            Q("Small craft bays >= 2", c => c.Docking[DockingBerthKind.SmallCraftBay] + c.Docking[DockingBerthKind.HangarSmall] >= 2),
            Q("Commercial district >= 2", c => c.Facilities[FacilityKind.Commercial] >= 2),
            Q("Medical bay >= 1", c => c.Facilities[FacilityKind.Medical] >= 1),
            Q("Customs >= 1", c => c.Facilities[FacilityKind.Customs] >= 1),
            Q("Warehouse >= 1", c => c.Facilities[FacilityKind.Warehouse] >= 1));

        rules[ClassificationId.StarportB] = R(ClassificationId.StarportB, "Class B Starport", "B",
            Q("Craft yard or Shipyard >= 1", c => c.Facilities[FacilityKind.ShipyardSmall] + c.Facilities[FacilityKind.ShipyardMedium] + c.Facilities[FacilityKind.ShipyardLarge] >= 1),
            Q("Fuel refinery >= 1", c => c.Facilities[FacilityKind.FuelRefinery] >= 1),
            Q("Standard+ berths >= 3", c => c.Docking[DockingBerthKind.StandardBerth] + c.Docking[DockingBerthKind.LargeBerth] + c.Docking[DockingBerthKind.CapitalBerth] >= 3),
            Q("Small craft bays >= 2", c => c.Docking[DockingBerthKind.SmallCraftBay] + c.Docking[DockingBerthKind.HangarSmall] >= 2),
            Q("Commercial >= 1", c => c.Facilities[FacilityKind.Commercial] >= 1),
            Q("Medical bay >= 1", c => c.Facilities[FacilityKind.Medical] >= 1));

        rules[ClassificationId.StarportC] = R(ClassificationId.StarportC, "Class C Starport", "C",
            Q("Repair bay or yard >= 1", c => c.Facilities[FacilityKind.RepairBay] + c.Facilities[FacilityKind.ShipyardSmall] + c.Facilities[FacilityKind.ShipyardMedium] + c.Facilities[FacilityKind.ShipyardLarge] >= 1),
            Q("Docking berths >= 2", c => c.Docking[DockingBerthKind.StandardBerth] + c.Docking[DockingBerthKind.LargeBerth] + c.Docking[DockingBerthKind.CapitalBerth] >= 2),
            Q("Fuel available", c => c.Facilities[FacilityKind.FuelDepot] + c.Facilities[FacilityKind.FuelRefinery] >= 1));

        rules[ClassificationId.StarportD] = R(ClassificationId.StarportD, "Class D Starport", "D",
            Q("Any docking >= 1", c => c.Docking.Sum() >= 1),
            Q("Fuel depot >= 1", c => c.Facilities[FacilityKind.FuelDepot] >= 1));

        rules[ClassificationId.NavalBase] = R(ClassificationId.NavalBase, "Naval Base", "N",
            Q("Combat Information Center", c => c.Facilities[FacilityKind.CombatInfoCenter] >= 1),
            Q("Armory >= 1", c => c.Facilities[FacilityKind.Armory] >= 1),
            Q("Military command center", c => c.CommandCenter == CommandCenterKind.Military),
            Q("Military+ sensors", c => c.Sensors == SensorGrade.Military || c.Sensors == SensorGrade.Improved || c.Sensors == SensorGrade.Advanced),
            Q("Weapons >= 20% hardpoints", c => c.Hardpoints > 0 && (double)c.HardpointsUsed / c.Hardpoints >= 0.2),
            Q("Docking berths >= 6", c => c.Docking.Sum() >= 6),
            Q("Training facility", c => c.Facilities[FacilityKind.Training] >= 1));

        rules[ClassificationId.ScoutBase] = R(ClassificationId.ScoutBase, "Scout Base", "S",
            Q("Comms array >= 1", c => c.Facilities[FacilityKind.CommsArray] >= 1),
            Q("Laboratory >= 1", c => c.Facilities[FacilityKind.Laboratory] >= 1),
            Q("Improved+ sensors", c => c.Sensors == SensorGrade.Improved || c.Sensors == SensorGrade.Advanced),
            Q("Small craft bays >= 2", c => c.Docking[DockingBerthKind.SmallCraftBay] + c.Docking[DockingBerthKind.HangarSmall] >= 2));

        rules[ClassificationId.ResearchStation] = R(ClassificationId.ResearchStation, "Research Station", "R",
            Q("Laboratories >= 2", c => c.Facilities[FacilityKind.Laboratory] >= 2),
            Q("Computer/15+", c => c.ComputerRating >= 15),
            Q("Comms array >= 1", c => c.Facilities[FacilityKind.CommsArray] >= 1));

        rules[ClassificationId.MiningPlatform] = R(ClassificationId.MiningPlatform, "Mining Platform", "M",
            Q("Ore processing >= 1", c => c.Facilities[FacilityKind.OreProcessing] >= 1),
            Q("Warehouse >= 1", c => c.Facilities[FacilityKind.Warehouse] >= 1),
            Q("Small craft bay >= 1", c => c.Docking[DockingBerthKind.SmallCraftBay] + c.Docking[DockingBerthKind.HangarSmall] >= 1));

        rules[ClassificationId.TradeStation] = R(ClassificationId.TradeStation, "Trade Station", "T",
            Q("Commercial >= 3", c => c.Facilities[FacilityKind.Commercial] >= 3),
            Q("Warehouse >= 2", c => c.Facilities[FacilityKind.Warehouse] >= 2),
            Q("Customs >= 1", c => c.Facilities[FacilityKind.Customs] >= 1),
            Q("Standard+ berths >= 5", c => c.Docking[DockingBerthKind.StandardBerth] + c.Docking[DockingBerthKind.LargeBerth] + c.Docking[DockingBerthKind.CapitalBerth] >= 5));

        rules[ClassificationId.DefensePlatform] = R(ClassificationId.DefensePlatform, "Defense Platform", "X",
            Q("CIC", c => c.Facilities[FacilityKind.CombatInfoCenter] >= 1),
            Q("Advanced sensors", c => c.Sensors == SensorGrade.Advanced),
            Q("Weapons >= 30% hardpoints", c => c.Hardpoints > 0 && (double)c.HardpointsUsed / c.Hardpoints >= 0.3),
            Q("Screens >= 1", c => c.Screens.Sum() >= 1),
            Q("Military command", c => c.CommandCenter == CommandCenterKind.Military));

        rules[ClassificationId.Waystation] = R(ClassificationId.Waystation, "Waystation", "W",
            Q("Fuel depot >= 2", c => c.Facilities[FacilityKind.FuelDepot] >= 2),
            Q("Docking >= 2", c => c.Docking.Sum() >= 2));

        return rules;
    }
}
