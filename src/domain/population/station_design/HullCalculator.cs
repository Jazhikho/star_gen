using System;
using System.Collections.Generic;
using StarGen.Domain.Population.StationDesign.Components;
using StdMath = System.Math;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Hull-related calculations: cost tiers, structure HP, hardpoints,
/// armor tonnage, and per-component tonnage/cost/power lookups.
/// </summary>
public static class HullCalculator
{
    /// <summary>
    /// Returns the credit cost per displacement ton based on hull size tiers.
    /// </summary>
    public static long CostPerTon(int hullTons)
    {
        return ComponentCatalog.HullCostPerTon(hullTons);
    }

    /// <summary>
    /// Returns the total hull cost including configuration multiplier.
    /// </summary>
    public static long HullCost(int hullTons, HullConfiguration config)
    {
        HullConfigurationProps props = GetHullConfiguration(config);
        return (long)(hullTons * CostPerTon(hullTons) * props.CostMultiplier);
    }

    /// <summary>
    /// Returns the number of structure hit points.
    /// </summary>
    public static int StructureHitPoints(int hullTons)
    {
        return (int)StdMath.Ceiling(hullTons / 50.0);
    }

    /// <summary>
    /// Returns the number of available hardpoints.
    /// </summary>
    public static int Hardpoints(int hullTons)
    {
        return hullTons / 100;
    }

    /// <summary>
    /// Returns the tonnage consumed by armor plating.
    /// </summary>
    public static int ArmorTonnage(int hullTons, ArmorMaterial material, int points, HullConfiguration config)
    {
        if (material == ArmorMaterial.None || points <= 0)
        {
            return 0;
        }

        ArmorMaterialProps armor = GetArmorMaterial(material);
        HullConfigurationProps hull = GetHullConfiguration(config);
        return (int)StdMath.Ceiling(hullTons * armor.TonsPerPointPerHullTon * points / hull.ArmorMultiplier);
    }

    /// <summary>
    /// Returns the effective armor protection points after configuration modifier.
    /// </summary>
    public static int EffectiveArmorPoints(ArmorMaterial material, int points, HullConfiguration config)
    {
        if (material == ArmorMaterial.None)
        {
            return 0;
        }

        return (int)StdMath.Floor(points * GetHullConfiguration(config).ArmorMultiplier);
    }

    /// <summary>
    /// Returns the tonnage of the command center section.
    /// </summary>
    public static int CommandTonnage(int hullTons, CommandCenterKind kind)
    {
        double fraction;
        if (kind == CommandCenterKind.Standard)
        {
            fraction = 0.02;
        }
        else if (kind == CommandCenterKind.Military)
        {
            fraction = 0.025;
        }
        else
        {
            throw new ArgumentException($"Unknown command center kind: {kind}");
        }

        return StdMath.Max(20, (int)StdMath.Ceiling(hullTons * fraction));
    }

    /// <summary>
    /// Returns the power consumed by the command center.
    /// </summary>
    public static int CommandPower(int commandTonnage)
    {
        return (int)StdMath.Ceiling(commandTonnage * 0.2);
    }

    /// <summary>
    /// Returns the cost of the command center.
    /// </summary>
    public static long CommandCost(int commandTonnage, CommandCenterKind kind)
    {
        long costPerTon;
        if (kind == CommandCenterKind.Standard)
        {
            costPerTon = 500_000L;
        }
        else if (kind == CommandCenterKind.Military)
        {
            costPerTon = 750_000L;
        }
        else
        {
            throw new ArgumentException($"Unknown command center kind: {kind}");
        }

        return commandTonnage * costPerTon;
    }

    /// <summary>
    /// Returns power plant tonnage for a given rating and plant type.
    /// </summary>
    public static int PowerPlantTonnage(int rating, PowerPlantKind kind)
    {
        PowerPlantProps props = GetPowerPlant(kind);
        return (int)StdMath.Ceiling(rating * props.TonsPerPowerPoint);
    }

    /// <summary>
    /// Returns power plant cost.
    /// </summary>
    public static long PowerPlantCost(int ppTonnage, PowerPlantKind kind)
    {
        return ppTonnage * GetPowerPlant(kind).CostPerTon;
    }

    /// <summary>
    /// Returns fuel tonnage for a given rating, plant type, and months.
    /// </summary>
    public static int FuelTonnage(int rating, PowerPlantKind kind, int months)
    {
        PowerPlantProps props = GetPowerPlant(kind);
        return (int)StdMath.Ceiling(rating * props.FuelPerPointPerMonth * months);
    }

    /// <summary>
    /// Returns the tonnage of a bay weapon by kind.
    /// </summary>
    public static int BayWeaponTonnage(BayWeapon kind)
    {
        return GetBayWeapon(kind).Tonnage;
    }

    /// <summary>
    /// Returns the tonnage of a sensor package by grade.
    /// </summary>
    public static int SensorTonnage(SensorGrade grade)
    {
        return GetSensorGrade(grade).Tonnage;
    }

    /// <summary>
    /// Returns the power consumed by a sensor package.
    /// </summary>
    public static int SensorPower(SensorGrade grade)
    {
        return GetSensorGrade(grade).Power;
    }

    /// <summary>
    /// Returns the cost of a sensor package.
    /// </summary>
    public static long SensorCost(SensorGrade grade)
    {
        return GetSensorGrade(grade).Cost;
    }

    /// <summary>
    /// Returns turret power consumption.
    /// </summary>
    public static int TurretPower(TurretMount kind)
    {
        return GetTurret(kind).Power;
    }

    /// <summary>
    /// Returns turret tonnage.
    /// </summary>
    public static int TurretTonnage(TurretMount kind)
    {
        return GetTurret(kind).Tonnage;
    }

    /// <summary>
    /// Returns turret cost.
    /// </summary>
    public static long TurretCost(TurretMount kind)
    {
        return GetTurret(kind).Cost;
    }

    /// <summary>
    /// Returns hardpoints consumed by a turret mount.
    /// </summary>
    public static int TurretHardpoints(TurretMount kind)
    {
        return GetTurret(kind).HardpointsConsumed;
    }

    /// <summary>
    /// Returns bay weapon power consumption.
    /// </summary>
    public static int BayWeaponPower(BayWeapon kind)
    {
        return GetBayWeapon(kind).Power;
    }

    /// <summary>
    /// Returns bay weapon cost.
    /// </summary>
    public static long BayWeaponCost(BayWeapon kind)
    {
        return GetBayWeapon(kind).Cost;
    }

    /// <summary>
    /// Returns screen tonnage.
    /// </summary>
    public static int ScreenTonnage(DefensiveScreen kind)
    {
        return GetScreen(kind).Tonnage;
    }

    /// <summary>
    /// Returns screen power consumption.
    /// </summary>
    public static int ScreenPower(DefensiveScreen kind)
    {
        return GetScreen(kind).Power;
    }

    /// <summary>
    /// Returns screen cost.
    /// </summary>
    public static long ScreenCost(DefensiveScreen kind)
    {
        return GetScreen(kind).Cost;
    }

    /// <summary>
    /// Returns docking berth tonnage.
    /// </summary>
    public static int DockingTonnage(DockingBerthKind kind)
    {
        return GetDocking(kind).Tonnage;
    }

    /// <summary>
    /// Returns docking berth cost.
    /// </summary>
    public static long DockingCost(DockingBerthKind kind)
    {
        return GetDocking(kind).Cost;
    }

    /// <summary>
    /// Returns facility tonnage.
    /// </summary>
    public static int FacilityTonnage(FacilityKind kind)
    {
        return GetFacility(kind).Tonnage;
    }

    /// <summary>
    /// Returns facility power consumption.
    /// </summary>
    public static int FacilityPower(FacilityKind kind)
    {
        return GetFacility(kind).Power;
    }

    /// <summary>
    /// Returns facility cost.
    /// </summary>
    public static long FacilityCost(FacilityKind kind)
    {
        return GetFacility(kind).Cost;
    }

    /// <summary>
    /// Returns accommodation tonnage.
    /// </summary>
    public static double AccommodationTonnage(AccommodationKind kind)
    {
        return GetAccommodation(kind).Tonnage;
    }

    /// <summary>
    /// Returns accommodation cost.
    /// </summary>
    public static long AccommodationCost(AccommodationKind kind)
    {
        return GetAccommodation(kind).Cost;
    }

    /// <summary>
    /// Returns accommodation occupancy.
    /// </summary>
    public static int AccommodationOccupancy(AccommodationKind kind)
    {
        return GetAccommodation(kind).Occupancy;
    }

    /// <summary>
    /// Returns software package cost.
    /// </summary>
    public static long SoftwareCost(SoftwarePackage kind)
    {
        return GetSoftware(kind).Cost;
    }

    /// <summary>
    /// Returns computer cost for a rating.
    /// </summary>
    public static long ComputerCost(int rating)
    {
        return ComponentCatalog.GetComputerCost(rating);
    }

    private static HullConfigurationProps GetHullConfiguration(HullConfiguration kind)
    {
        return GetRequired(ComponentCatalog.HullConfigurations, kind, "hull configuration");
    }

    private static ArmorMaterialProps GetArmorMaterial(ArmorMaterial kind)
    {
        return GetRequired(ComponentCatalog.ArmorMaterials, kind, "armor material");
    }

    private static PowerPlantProps GetPowerPlant(PowerPlantKind kind)
    {
        return GetRequired(ComponentCatalog.PowerPlants, kind, "power plant");
    }

    private static SensorGradeProps GetSensorGrade(SensorGrade kind)
    {
        return GetRequired(ComponentCatalog.Sensors, kind, "sensor grade");
    }

    private static TurretMountProps GetTurret(TurretMount kind)
    {
        return GetRequired(ComponentCatalog.Turrets, kind, "turret");
    }

    private static BayWeaponProps GetBayWeapon(BayWeapon kind)
    {
        return GetRequired(ComponentCatalog.Bays, kind, "bay weapon");
    }

    private static DefensiveScreenProps GetScreen(DefensiveScreen kind)
    {
        return GetRequired(ComponentCatalog.Screens, kind, "screen");
    }

    private static DockingBerthProps GetDocking(DockingBerthKind kind)
    {
        return GetRequired(ComponentCatalog.Docking, kind, "docking berth");
    }

    private static FacilityProps GetFacility(FacilityKind kind)
    {
        return GetRequired(ComponentCatalog.Facilities, kind, "facility");
    }

    private static AccommodationProps GetAccommodation(AccommodationKind kind)
    {
        return GetRequired(ComponentCatalog.Accommodations, kind, "accommodation");
    }

    private static SoftwareProps GetSoftware(SoftwarePackage kind)
    {
        return GetRequired(ComponentCatalog.Software, kind, "software package");
    }

    private static TProps GetRequired<TKey, TProps>(
        IReadOnlyDictionary<TKey, TProps> data,
        TKey key,
        string label) where TKey : notnull
    {
        if (data.TryGetValue(key, out TProps? value))
        {
            return value;
        }

        throw new ArgumentException($"Unknown {label}: {key}");
    }
}
