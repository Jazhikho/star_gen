using System;
using System.Collections.Generic;
using StarGen.Domain.Population.StationDesign.Components;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Static catalog of all station component data tables.
/// Direct port of the Traveller extended ship rules data.
/// </summary>
public static class ComponentCatalog
{
    /// <summary>
    /// Catalog version. Increment when any data value changes, so compact saves can
    /// detect preset drift.
    /// </summary>
    public const string Version = "1.1.0";

    /// <summary>
    /// Hull configuration properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<HullConfiguration, HullConfigurationProps> HullConfigurations =
        new Dictionary<HullConfiguration, HullConfigurationProps>
        {
            [HullConfiguration.Sphere] = new("Sphere", 1.0, 1.0, "Most efficient volume-to-surface ratio"),
            [HullConfiguration.Cylinder] = new("Cylinder", 0.9, 0.9, "Spinnable for centrifugal gravity"),
            [HullConfiguration.Ring] = new("Ring/Torus", 1.2, 0.8, "Spin gravity, spacious interior"),
            [HullConfiguration.Modular] = new("Modular", 1.1, 0.7, "Connected pods, easily expandable"),
            [HullConfiguration.Asteroid] = new("Asteroid", 0.6, 1.5, "Hollowed rock, natural armor bonus"),
            [HullConfiguration.Platform] = new("Open Platform", 0.7, 0.5, "Minimal hull, maximum docking access"),
        };

    /// <summary>
    /// Armor material properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<ArmorMaterial, ArmorMaterialProps> ArmorMaterials =
        new Dictionary<ArmorMaterial, ArmorMaterialProps>
        {
            [ArmorMaterial.None] = new("None", 0.0, 0),
            [ArmorMaterial.TitaniumSteel] = new("Titanium Steel", 0.02, 50000),
            [ArmorMaterial.Crystaliron] = new("Crystaliron", 0.0125, 150000),
            [ArmorMaterial.BondedSuperdense] = new("Bonded Superdense", 0.008, 250000),
        };

    /// <summary>
    /// Power plant type properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<PowerPlantKind, PowerPlantProps> PowerPlants =
        new Dictionary<PowerPlantKind, PowerPlantProps>
        {
            [PowerPlantKind.Fusion] = new("Fusion Reactor", 1.0, 2000000, 0.1),
            [PowerPlantKind.Fission] = new("Fission Pile", 1.5, 1000000, 0.0),
            [PowerPlantKind.Solar] = new("Solar Array", 5.0, 500000, 0.0),
        };

    /// <summary>
    /// Sensor grade properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<SensorGrade, SensorGradeProps> Sensors =
        new Dictionary<SensorGrade, SensorGradeProps>
        {
            [SensorGrade.Basic] = new("Basic (-4 DM)", 1, 25000, 0, -4),
            [SensorGrade.Civilian] = new("Civilian (-2 DM)", 3, 100000, 1, -2),
            [SensorGrade.Military] = new("Military (+0 DM)", 5, 1000000, 2, 0),
            [SensorGrade.Improved] = new("Improved (+1 DM)", 7, 4300000, 4, 1),
            [SensorGrade.Advanced] = new("Advanced (+2 DM)", 10, 8600000, 6, 2),
        };

    /// <summary>
    /// Computer cost by rating. Valid ratings: 5, 10, 15, 20, 25, 30, 35.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, long> ComputerCosts =
        new Dictionary<int, long>
        {
            [5] = 30000,
            [10] = 160000,
            [15] = 2000000,
            [20] = 5000000,
            [25] = 10000000,
            [30] = 20000000,
            [35] = 30000000,
        };

    /// <summary>
    /// Computer display names by rating.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string> ComputerNames =
        new Dictionary<int, string>
        {
            [5] = "Computer/5",
            [10] = "Computer/10",
            [15] = "Computer/15",
            [20] = "Computer/20",
            [25] = "Computer/25",
            [30] = "Computer/30",
            [35] = "Computer/35",
        };

    /// <summary>
    /// Turret mount properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<TurretMount, TurretMountProps> Turrets =
        new Dictionary<TurretMount, TurretMountProps>
        {
            [TurretMount.Single] = new("Single Turret", 1, 200000, 1, 1),
            [TurretMount.Double] = new("Double Turret", 1, 500000, 1, 1),
            [TurretMount.Triple] = new("Triple Turret", 1, 1000000, 1, 1),
            [TurretMount.Barbette] = new("Barbette", 5, 3000000, 3, 1),
        };

    /// <summary>
    /// Bay weapon properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<BayWeapon, BayWeaponProps> Bays =
        new Dictionary<BayWeapon, BayWeaponProps>
        {
            [BayWeapon.Missile50] = new("Missile Bay (50t)", 50, 12000000, 5),
            [BayWeapon.Laser50] = new("Beam Laser Bay (50t)", 50, 9000000, 8),
            [BayWeapon.Particle50] = new("Particle Bay (50t)", 50, 20000000, 10),
            [BayWeapon.Meson50] = new("Meson Gun Bay (50t)", 50, 50000000, 15),
            [BayWeapon.Missile100] = new("Missile Bay (100t)", 100, 24000000, 10),
            [BayWeapon.Laser100] = new("Beam Laser Bay (100t)", 100, 18000000, 16),
            [BayWeapon.Particle100] = new("Particle Bay (100t)", 100, 40000000, 20),
            [BayWeapon.Meson100] = new("Meson Gun Bay (100t)", 100, 100000000, 30),
        };

    /// <summary>
    /// Defensive screen properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<DefensiveScreen, DefensiveScreenProps> Screens =
        new Dictionary<DefensiveScreen, DefensiveScreenProps>
        {
            [DefensiveScreen.NuclearDamper] = new("Nuclear Damper", 50, 50000000, 10),
            [DefensiveScreen.MesonScreen] = new("Meson Screen", 50, 60000000, 15),
        };

    /// <summary>
    /// Docking berth properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<DockingBerthKind, DockingBerthProps> Docking =
        new Dictionary<DockingBerthKind, DockingBerthProps>
        {
            [DockingBerthKind.SmallCraftBay] = new("Small Craft Bay", 30, 150000, "1 craft up to 100t"),
            [DockingBerthKind.StandardBerth] = new("Standard Docking Berth", 150, 500000, "1 vessel up to 1,000t"),
            [DockingBerthKind.LargeBerth] = new("Large Docking Berth", 1500, 3000000, "1 vessel up to 5,000t"),
            [DockingBerthKind.CapitalBerth] = new("Capital Ship Berth", 5000, 10000000, "1 vessel up to 50,000t"),
            [DockingBerthKind.HangarSmall] = new("Enclosed Hangar (Small)", 200, 1000000, "1 craft <100t, full repair"),
            [DockingBerthKind.HangarMedium] = new("Enclosed Hangar (Medium)", 2000, 5000000, "1 vessel <1,000t, full repair"),
        };

    /// <summary>
    /// Accommodation type properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<AccommodationKind, AccommodationProps> Accommodations =
        new Dictionary<AccommodationKind, AccommodationProps>
        {
            [AccommodationKind.Stateroom] = new("Stateroom", 4.0, 50000, 1, "Standard crew/passenger quarters"),
            [AccommodationKind.HighStateroom] = new("High Stateroom", 6.0, 100000, 1, "Superior passenger quarters"),
            [AccommodationKind.LuxurySuite] = new("Luxury Suite", 10.0, 250000, 1, "VIP accommodations"),
            [AccommodationKind.LowBerth] = new("Low Berth", 0.5, 50000, 0, "Cryogenic suspension pod"),
            [AccommodationKind.Barracks] = new("Barracks (4-person)", 8.0, 25000, 4, "Shared military quarters"),
            [AccommodationKind.EmergencyLow] = new("Emergency Low (x10)", 5.0, 100000, 0, "Emergency cryogenic pods"),
        };

    /// <summary>
    /// Facility module properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<FacilityKind, FacilityProps> Facilities =
        new Dictionary<FacilityKind, FacilityProps>
        {
            [FacilityKind.ShipyardSmall] = new("Small Craft Yard (100t)", 400, 30000000, 10, "Build & repair craft ≤100t"),
            [FacilityKind.ShipyardMedium] = new("Shipyard (1,000t)", 3000, 500000000, 50, "Build & repair ships ≤1,000t"),
            [FacilityKind.ShipyardLarge] = new("Shipyard (5,000t)", 15000, 2000000000, 150, "Build & repair ships ≤5,000t"),
            [FacilityKind.RepairBay] = new("Repair Bay (100t)", 200, 10000000, 5, "Repairs only, ≤100t"),
            [FacilityKind.FuelRefinery] = new("Fuel Refinery (500t/day)", 50, 5000000, 5, "Refines unprocessed fuel"),
            [FacilityKind.FuelDepot] = new("Fuel Depot (1,000t)", 1000, 1000000, 0, "Bulk fuel storage"),
            [FacilityKind.Commercial] = new("Commercial District", 100, 2000000, 2, "Shops, markets, services"),
            [FacilityKind.Medical] = new("Medical Bay (10 beds)", 40, 5000000, 3, "Hospital facilities"),
            [FacilityKind.Laboratory] = new("Laboratory", 50, 4000000, 3, "Scientific research lab"),
            [FacilityKind.Manufacturing] = new("Manufacturing Plant", 100, 8000000, 8, "Production facility"),
            [FacilityKind.Hydroponics] = new("Hydroponics (feeds 50)", 100, 1000000, 3, "Food & air recycling"),
            [FacilityKind.Recreation] = new("Recreation Facility", 50, 1000000, 1, "Parks, gym, entertainment"),
            [FacilityKind.Armory] = new("Armory", 20, 2000000, 0, "Weapons storage & range"),
            [FacilityKind.Warehouse] = new("Warehouse (1,000t)", 1000, 500000, 0, "Organized cargo storage"),
            [FacilityKind.OreProcessing] = new("Ore Processing", 200, 15000000, 10, "Mineral refining & smelting"),
            [FacilityKind.CommsArray] = new("Comms Array", 20, 10000000, 5, "Enhanced long-range comms"),
            [FacilityKind.CombatInfoCenter] = new("Combat Info Center", 30, 20000000, 5, "Military C&C"),
            [FacilityKind.Brig] = new("Brig / Detention", 50, 3000000, 1, "Secure detention"),
            [FacilityKind.Customs] = new("Customs & Immigration", 30, 1000000, 1, "Entry processing"),
            [FacilityKind.Training] = new("Training Facility", 60, 4000000, 2, "Crew training & simulation"),
        };

    /// <summary>
    /// Software package properties.
    /// </summary>
    public static readonly IReadOnlyDictionary<SoftwarePackage, SoftwareProps> Software =
        new Dictionary<SoftwarePackage, SoftwareProps>
        {
            [SoftwarePackage.Maneuver0] = new("Maneuver/0 (Station-Keeping)", 0),
            [SoftwarePackage.Library] = new("Library", 0),
            [SoftwarePackage.Intellect] = new("Intellect", 1000000),
            [SoftwarePackage.FireControl1] = new("Fire Control/1", 2000000),
            [SoftwarePackage.FireControl2] = new("Fire Control/2", 4000000),
            [SoftwarePackage.FireControl3] = new("Fire Control/3", 6000000),
            [SoftwarePackage.AutoRepair] = new("Auto-Repair/1", 5000000),
            [SoftwarePackage.AntiHijack] = new("Anti-Hijack/2", 8000000),
            [SoftwarePackage.Evade1] = new("Evade/1", 1000000),
            [SoftwarePackage.BattleNetwork] = new("Battle Network/1", 5000000),
        };

    /// <summary>
    /// Design template display names and hull tonnage ranges.
    /// </summary>
    public static readonly IReadOnlyDictionary<DesignTemplate, (string Name, int MinHull, int MaxHull, string Description)> Templates =
        new Dictionary<DesignTemplate, (string, int, int, string)>
        {
            [DesignTemplate.HighportA] = ("Class A Highport", 80000, 500000, "Full shipyard, refined fuel, excellent facilities"),
            [DesignTemplate.HighportB] = ("Class B Highport", 30000, 200000, "Spacecraft construction, refined fuel, good facilities"),
            [DesignTemplate.HighportC] = ("Class C Highport", 5000, 50000, "Limited repair, unrefined fuel, routine quality"),
            [DesignTemplate.HighportD] = ("Class D Highport", 1000, 10000, "Minimal facilities, unrefined fuel"),
            [DesignTemplate.Naval] = ("Naval Base", 50000, 1000000, "Military operations & fleet support"),
            [DesignTemplate.Scout] = ("Scout Base", 5000, 50000, "IISS operations & comms relay"),
            [DesignTemplate.Research] = ("Research Station", 2000, 20000, "Scientific research facility"),
            [DesignTemplate.Mining] = ("Mining Platform", 5000, 100000, "Resource extraction & processing"),
            [DesignTemplate.Trade] = ("Trade Station", 10000, 100000, "Commercial hub & marketplace"),
            [DesignTemplate.Waystation] = ("Waystation", 1000, 10000, "Deep space refueling point"),
            [DesignTemplate.Defense] = ("Defense Platform", 2000, 50000, "System defense monitor"),
            [DesignTemplate.Freeport] = ("Freeport", 10000, 200000, "Independent station, mixed use"),
            [DesignTemplate.Custom] = ("Custom Station", 100, 2000000, "Design from scratch"),
        };

    /// <summary>
    /// Returns the hull cost per ton for a given hull displacement.
    /// Tiered: ≤2000→50k, ≤10000→40k, ≤100000→30k, >100000→25k.
    /// </summary>
    public static long HullCostPerTon(int hullTons)
    {
        if (hullTons <= 2000)
        {
            return 50000;
        }

        if (hullTons <= 10000)
        {
            return 40000;
        }

        if (hullTons <= 100000)
        {
            return 30000;
        }

        return 25000;
    }

    /// <summary>
    /// Returns the computer cost for a given rating, or throws if invalid.
    /// </summary>
    public static long GetComputerCost(int rating)
    {
        if (ComputerCosts.TryGetValue(rating, out long cost))
        {
            return cost;
        }

        throw new ArgumentException($"Invalid computer rating: {rating}. Valid: 5,10,15,20,25,30,35");
    }
}
