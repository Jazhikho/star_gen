using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Fully resolved component selection for a station design.
/// All fields are populated — either from preset auto-application
/// or from manual overrides. Consumed by the design calculators.
/// </summary>
public sealed class ComponentSelection
{
    public PowerPlantKind PowerPlant = PowerPlantKind.Fusion;
    public int PowerRating = 10;
    public int FuelMonths = 6;

    public CommandCenterKind CommandCenter = CommandCenterKind.Standard;
    public int ComputerRating = 10;
    public SensorGrade Sensors = SensorGrade.Basic;
    public List<SoftwarePackage> Software = new();

    public ArmorMaterial ArmorMaterial = ArmorMaterial.None;
    public int ArmorPoints = 0;
    public ComponentCounts<TurretMount> Turrets = new();
    public ComponentCounts<BayWeapon> Bays = new();
    public ComponentCounts<DefensiveScreen> Screens = new();

    public ComponentCounts<DockingBerthKind> Docking = new();
    public ComponentCounts<AccommodationKind> Accommodations = new();
    public ComponentCounts<FacilityKind> Facilities = new();

    /// <summary>
    /// Serializes the resolved selection.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<int> software = new();
        foreach (SoftwarePackage package in Software)
        {
            software.Add((int)package);
        }

        return new Dictionary
        {
            ["power_plant"] = (int)PowerPlant,
            ["power_rating"] = PowerRating,
            ["fuel_months"] = FuelMonths,
            ["command_center"] = (int)CommandCenter,
            ["computer_rating"] = ComputerRating,
            ["sensors"] = (int)Sensors,
            ["software"] = software,
            ["armor_material"] = (int)ArmorMaterial,
            ["armor_points"] = ArmorPoints,
            ["turrets"] = Turrets.ToDictionary(),
            ["bays"] = Bays.ToDictionary(),
            ["screens"] = Screens.ToDictionary(),
            ["docking"] = Docking.ToDictionary(),
            ["accommodations"] = Accommodations.ToDictionary(),
            ["facilities"] = Facilities.ToDictionary(),
        };
    }

    /// <summary>
    /// Rebuilds a resolved selection from a dictionary.
    /// </summary>
    public static ComponentSelection FromDictionary(Dictionary? data)
    {
        ComponentSelection selection = new();
        if (data == null)
        {
            return selection;
        }

        selection.PowerPlant = (PowerPlantKind)GetInt(data, "power_plant", (int)selection.PowerPlant);
        selection.PowerRating = GetInt(data, "power_rating", selection.PowerRating);
        selection.FuelMonths = GetInt(data, "fuel_months", selection.FuelMonths);
        selection.CommandCenter = (CommandCenterKind)GetInt(data, "command_center", (int)selection.CommandCenter);
        selection.ComputerRating = GetInt(data, "computer_rating", selection.ComputerRating);
        selection.Sensors = (SensorGrade)GetInt(data, "sensors", (int)selection.Sensors);
        selection.ArmorMaterial = (ArmorMaterial)GetInt(data, "armor_material", (int)selection.ArmorMaterial);
        selection.ArmorPoints = GetInt(data, "armor_points", selection.ArmorPoints);
        selection.Software = GetSoftware(data);
        selection.Turrets = ComponentCounts<TurretMount>.FromDictionary(GetDictionary(data, "turrets"));
        selection.Bays = ComponentCounts<BayWeapon>.FromDictionary(GetDictionary(data, "bays"));
        selection.Screens = ComponentCounts<DefensiveScreen>.FromDictionary(GetDictionary(data, "screens"));
        selection.Docking = ComponentCounts<DockingBerthKind>.FromDictionary(GetDictionary(data, "docking"));
        selection.Accommodations = ComponentCounts<AccommodationKind>.FromDictionary(GetDictionary(data, "accommodations"));
        selection.Facilities = ComponentCounts<FacilityKind>.FromDictionary(GetDictionary(data, "facilities"));
        return selection;
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

    private static List<SoftwarePackage> GetSoftware(Dictionary data)
    {
        List<SoftwarePackage> software = new();
        if (!data.ContainsKey("software"))
        {
            return software;
        }

        Variant value = data["software"];
        if (value.VariantType != Variant.Type.Array)
        {
            return software;
        }

        foreach (Variant rawPackage in (Array)value)
        {
            if (rawPackage.VariantType == Variant.Type.Int)
            {
                software.Add((SoftwarePackage)(int)rawPackage);
            }
        }

        return software;
    }
}
