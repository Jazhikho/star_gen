using System.Collections.Generic;

namespace StarGen.Domain.Population.StationDesign.Presets;

/// <summary>
/// Static catalog of all design presets, one per DesignTemplate.
/// </summary>
public static class DesignPresetCatalog
{
    /// <summary>
    /// Preset catalog version. Increment when preset behavior changes.
    /// </summary>
    public const string CatalogVersion = "1.1.0";

    /// <summary>
    /// All presets keyed by design template.
    /// </summary>
    public static readonly IReadOnlyDictionary<DesignTemplate, DesignPreset> Presets = BuildAll();

    /// <summary>
    /// Returns the preset for a template.
    /// </summary>
    public static DesignPreset Get(DesignTemplate template)
    {
        if (Presets.TryGetValue(template, out DesignPreset? preset) && preset != null)
        {
            return preset;
        }

        throw new System.ArgumentException($"No design preset defined for template: {template}");
    }

    private static Dictionary<DesignTemplate, DesignPreset> BuildAll()
    {
        Dictionary<DesignTemplate, DesignPreset> all = new()
        {
            [DesignTemplate.HighportA] = BuildHighportA(),
            [DesignTemplate.HighportB] = BuildHighportB(),
            [DesignTemplate.HighportC] = BuildHighportC(),
            [DesignTemplate.HighportD] = BuildHighportD(),
            [DesignTemplate.Naval] = BuildNaval(),
            [DesignTemplate.Scout] = BuildScout(),
            [DesignTemplate.Research] = BuildResearch(),
            [DesignTemplate.Mining] = BuildMining(),
            [DesignTemplate.Trade] = BuildTrade(),
            [DesignTemplate.Waystation] = BuildWaystation(),
            [DesignTemplate.Defense] = BuildDefense(),
            [DesignTemplate.Freeport] = BuildFreeport(),
            [DesignTemplate.Custom] = BuildCustom(),
        };
        return all;
    }

    private static DesignPreset BuildHighportA()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Military,
            ComputerRating = 35,
            Sensors = SensorGrade.Advanced,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.25,
            FuelMonths = 12,
            ArmorMaterial = ArmorMaterial.Crystaliron,
            ArmorRule = new ArmorScalingRule(30000, 3, 12),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.FireControl2, SoftwarePackage.AutoRepair, SoftwarePackage.AntiHijack, SoftwarePackage.BattleNetwork },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.ShipyardLarge] = new(400000, 0),
                [FacilityKind.ShipyardMedium] = new(60000, 1),
                [FacilityKind.FuelRefinery] = new(15000, 2),
                [FacilityKind.FuelDepot] = new(12000, 2),
                [FacilityKind.Commercial] = new(8000, 3),
                [FacilityKind.Medical] = new(25000, 1),
                [FacilityKind.Warehouse] = new(12000, 2),
                [FacilityKind.Customs] = new(40000, 1),
                [FacilityKind.Recreation] = new(10000, 2),
                [FacilityKind.CommsArray] = new(0, 1),
                [FacilityKind.Laboratory] = new(100000, 0),
                [FacilityKind.Hydroponics] = new(15000, 1),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(6000, 3),
                [DockingBerthKind.StandardBerth] = new(4000, 6),
                [DockingBerthKind.LargeBerth] = new(25000, 1),
                [DockingBerthKind.CapitalBerth] = new(500000, 0),
                [DockingBerthKind.HangarSmall] = new(12000, 2),
                [DockingBerthKind.HangarMedium] = new(80000, 0),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.2,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Triple] = 0.5,
                    [TurretMount.Barbette] = 0.3,
                    [TurretMount.Double] = 0.2,
                },
                BayTonnageFraction = 0.01,
                BayMix = new Dictionary<BayWeapon, double>
                {
                    [BayWeapon.Missile50] = 0.4,
                    [BayWeapon.Laser50] = 0.3,
                    [BayWeapon.Particle50] = 0.3,
                },
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>
            {
                [DefensiveScreen.NuclearDamper] = new(120000, 0),
                [DefensiveScreen.MesonScreen] = new(250000, 0),
            },
            OfficerRatio = 0.2,
        };
    }

    private static DesignPreset BuildHighportB()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 25,
            Sensors = SensorGrade.Improved,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.2,
            FuelMonths = 12,
            ArmorMaterial = ArmorMaterial.Crystaliron,
            ArmorRule = new ArmorScalingRule(25000, 2, 8),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.FireControl1, SoftwarePackage.AutoRepair, SoftwarePackage.AntiHijack },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.ShipyardMedium] = new(100000, 0),
                [FacilityKind.ShipyardSmall] = new(30000, 1),
                [FacilityKind.FuelRefinery] = new(18000, 1),
                [FacilityKind.FuelDepot] = new(15000, 1),
                [FacilityKind.Commercial] = new(10000, 2),
                [FacilityKind.Medical] = new(35000, 1),
                [FacilityKind.Warehouse] = new(15000, 1),
                [FacilityKind.Customs] = new(50000, 1),
                [FacilityKind.Recreation] = new(15000, 1),
                [FacilityKind.CommsArray] = new(0, 1),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(7000, 2),
                [DockingBerthKind.StandardBerth] = new(4000, 5),
                [DockingBerthKind.LargeBerth] = new(25000, 1),
                [DockingBerthKind.HangarSmall] = new(15000, 1),
                [DockingBerthKind.HangarMedium] = new(80000, 0),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.12,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Triple] = 0.4,
                    [TurretMount.Double] = 0.4,
                    [TurretMount.Barbette] = 0.2,
                },
                BayTonnageFraction = 0.004,
                BayMix = new Dictionary<BayWeapon, double>
                {
                    [BayWeapon.Missile50] = 0.5,
                    [BayWeapon.Laser50] = 0.5,
                },
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>
            {
                [DefensiveScreen.NuclearDamper] = new(200000, 0),
            },
            OfficerRatio = 0.2,
        };
    }

    private static DesignPreset BuildHighportC()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 15,
            Sensors = SensorGrade.Civilian,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.15,
            FuelMonths = 6,
            ArmorMaterial = ArmorMaterial.TitaniumSteel,
            ArmorRule = new ArmorScalingRule(12000, 1, 4),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.FireControl1 },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.RepairBay] = new(12000, 1),
                [FacilityKind.FuelRefinery] = new(25000, 0),
                [FacilityKind.FuelDepot] = new(10000, 1),
                [FacilityKind.Commercial] = new(15000, 1),
                [FacilityKind.Medical] = new(40000, 0),
                [FacilityKind.Warehouse] = new(20000, 0),
                [FacilityKind.Customs] = new(50000, 0),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(8000, 1),
                [DockingBerthKind.StandardBerth] = new(5000, 2),
                [DockingBerthKind.LargeBerth] = new(40000, 0),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.06,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Double] = 0.6,
                    [TurretMount.Single] = 0.4,
                },
                BayTonnageFraction = 0.0,
                BayMix = new Dictionary<BayWeapon, double>(),
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>(),
            OfficerRatio = 0.15,
        };
    }

    private static DesignPreset BuildHighportD()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 10,
            Sensors = SensorGrade.Basic,
            PowerPlant = PowerPlantKind.Fission,
            PowerMargin = 1.1,
            FuelMonths = 0,
            ArmorMaterial = ArmorMaterial.TitaniumSteel,
            ArmorRule = new ArmorScalingRule(6000, 1, 2),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0 },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.RepairBay] = new(20000, 0),
                [FacilityKind.FuelDepot] = new(5000, 1),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(5000, 1),
                [DockingBerthKind.StandardBerth] = new(3000, 1),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.04,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Single] = 0.7,
                    [TurretMount.Double] = 0.3,
                },
                BayTonnageFraction = 0.0,
                BayMix = new Dictionary<BayWeapon, double>(),
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>(),
            OfficerRatio = 0.1,
        };
    }

    private static DesignPreset BuildNaval()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Military,
            ComputerRating = 35,
            Sensors = SensorGrade.Advanced,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.3,
            FuelMonths = 18,
            ArmorMaterial = ArmorMaterial.BondedSuperdense,
            ArmorRule = new ArmorScalingRule(35000, 4, 15),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.FireControl3, SoftwarePackage.AutoRepair, SoftwarePackage.AntiHijack, SoftwarePackage.BattleNetwork, SoftwarePackage.Evade1 },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.ShipyardLarge] = new(350000, 0),
                [FacilityKind.ShipyardMedium] = new(100000, 1),
                [FacilityKind.FuelRefinery] = new(12000, 2),
                [FacilityKind.FuelDepot] = new(10000, 2),
                [FacilityKind.Medical] = new(20000, 1),
                [FacilityKind.Armory] = new(25000, 1),
                [FacilityKind.CombatInfoCenter] = new(0, 1),
                [FacilityKind.Training] = new(35000, 1),
                [FacilityKind.Warehouse] = new(15000, 1),
                [FacilityKind.Recreation] = new(18000, 1),
                [FacilityKind.Hydroponics] = new(15000, 1),
                [FacilityKind.Brig] = new(150000, 0),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(4000, 4),
                [DockingBerthKind.StandardBerth] = new(3000, 8),
                [DockingBerthKind.LargeBerth] = new(15000, 2),
                [DockingBerthKind.CapitalBerth] = new(100000, 1),
                [DockingBerthKind.HangarSmall] = new(8000, 3),
                [DockingBerthKind.HangarMedium] = new(50000, 1),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.35,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Triple] = 0.35,
                    [TurretMount.Barbette] = 0.45,
                    [TurretMount.Double] = 0.2,
                },
                BayTonnageFraction = 0.02,
                BayMix = new Dictionary<BayWeapon, double>
                {
                    [BayWeapon.Missile50] = 0.3,
                    [BayWeapon.Laser50] = 0.2,
                    [BayWeapon.Particle50] = 0.3,
                    [BayWeapon.Meson50] = 0.2,
                },
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>
            {
                [DefensiveScreen.NuclearDamper] = new(100000, 1),
                [DefensiveScreen.MesonScreen] = new(150000, 0),
            },
            OfficerRatio = 0.25,
        };
    }

    private static DesignPreset BuildScout()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 20,
            Sensors = SensorGrade.Improved,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.2,
            FuelMonths = 12,
            ArmorMaterial = ArmorMaterial.TitaniumSteel,
            ArmorRule = new ArmorScalingRule(18000, 1, 6),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.FireControl1, SoftwarePackage.AutoRepair },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.ShipyardSmall] = new(20000, 0),
                [FacilityKind.RepairBay] = new(12000, 1),
                [FacilityKind.FuelRefinery] = new(18000, 1),
                [FacilityKind.FuelDepot] = new(15000, 1),
                [FacilityKind.Laboratory] = new(8000, 1),
                [FacilityKind.CommsArray] = new(0, 1),
                [FacilityKind.Medical] = new(30000, 0),
                [FacilityKind.Recreation] = new(25000, 0),
                [FacilityKind.Hydroponics] = new(25000, 0),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(5000, 2),
                [DockingBerthKind.StandardBerth] = new(6000, 2),
                [DockingBerthKind.LargeBerth] = new(30000, 0),
                [DockingBerthKind.HangarSmall] = new(10000, 1),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.08,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Double] = 0.5,
                    [TurretMount.Triple] = 0.5,
                },
                BayTonnageFraction = 0.0,
                BayMix = new Dictionary<BayWeapon, double>(),
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>(),
            OfficerRatio = 0.2,
        };
    }

    private static DesignPreset BuildResearch()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 20,
            Sensors = SensorGrade.Improved,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.2,
            FuelMonths = 12,
            ArmorMaterial = ArmorMaterial.TitaniumSteel,
            ArmorRule = new ArmorScalingRule(12000, 1, 4),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.AutoRepair },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.Laboratory] = new(3000, 2),
                [FacilityKind.Medical] = new(10000, 1),
                [FacilityKind.CommsArray] = new(0, 1),
                [FacilityKind.Hydroponics] = new(10000, 1),
                [FacilityKind.Recreation] = new(10000, 0),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(5000, 1),
                [DockingBerthKind.StandardBerth] = new(8000, 1),
                [DockingBerthKind.HangarSmall] = new(12000, 1),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.04,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Double] = 0.6,
                    [TurretMount.Single] = 0.4,
                },
                BayTonnageFraction = 0.0,
                BayMix = new Dictionary<BayWeapon, double>(),
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>(),
            OfficerRatio = 0.25,
        };
    }

    private static DesignPreset BuildMining()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 15,
            Sensors = SensorGrade.Civilian,
            PowerPlant = PowerPlantKind.Fission,
            PowerMargin = 1.2,
            FuelMonths = 0,
            ArmorMaterial = ArmorMaterial.TitaniumSteel,
            ArmorRule = new ArmorScalingRule(18000, 1, 6),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.FireControl1 },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.OreProcessing] = new(10000, 1),
                [FacilityKind.Manufacturing] = new(20000, 0),
                [FacilityKind.RepairBay] = new(15000, 1),
                [FacilityKind.FuelDepot] = new(18000, 1),
                [FacilityKind.Warehouse] = new(8000, 1),
                [FacilityKind.Medical] = new(30000, 0),
                [FacilityKind.Recreation] = new(25000, 0),
                [FacilityKind.Hydroponics] = new(20000, 0),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(6000, 2),
                [DockingBerthKind.StandardBerth] = new(8000, 2),
                [DockingBerthKind.LargeBerth] = new(35000, 0),
                [DockingBerthKind.HangarSmall] = new(12000, 1),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.06,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Single] = 0.4,
                    [TurretMount.Double] = 0.6,
                },
                BayTonnageFraction = 0.0,
                BayMix = new Dictionary<BayWeapon, double>(),
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>(),
            OfficerRatio = 0.15,
        };
    }

    private static DesignPreset BuildTrade()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 20,
            Sensors = SensorGrade.Civilian,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.2,
            FuelMonths = 12,
            ArmorMaterial = ArmorMaterial.Crystaliron,
            ArmorRule = new ArmorScalingRule(25000, 2, 6),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.FireControl1, SoftwarePackage.AntiHijack },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.Commercial] = new(5000, 3),
                [FacilityKind.Warehouse] = new(6000, 2),
                [FacilityKind.Customs] = new(20000, 1),
                [FacilityKind.FuelRefinery] = new(18000, 1),
                [FacilityKind.FuelDepot] = new(10000, 1),
                [FacilityKind.Medical] = new(25000, 1),
                [FacilityKind.Recreation] = new(8000, 2),
                [FacilityKind.Hydroponics] = new(15000, 1),
                [FacilityKind.CommsArray] = new(0, 1),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(6000, 2),
                [DockingBerthKind.StandardBerth] = new(3000, 5),
                [DockingBerthKind.LargeBerth] = new(15000, 2),
                [DockingBerthKind.CapitalBerth] = new(120000, 0),
                [DockingBerthKind.HangarSmall] = new(12000, 1),
                [DockingBerthKind.HangarMedium] = new(60000, 0),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.08,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Double] = 0.5,
                    [TurretMount.Triple] = 0.5,
                },
                BayTonnageFraction = 0.004,
                BayMix = new Dictionary<BayWeapon, double>
                {
                    [BayWeapon.Missile50] = 0.6,
                    [BayWeapon.Laser50] = 0.4,
                },
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>
            {
                [DefensiveScreen.NuclearDamper] = new(200000, 0),
            },
            OfficerRatio = 0.2,
        };
    }

    private static DesignPreset BuildWaystation()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 10,
            Sensors = SensorGrade.Civilian,
            PowerPlant = PowerPlantKind.Solar,
            PowerMargin = 1.15,
            FuelMonths = 0,
            ArmorMaterial = ArmorMaterial.TitaniumSteel,
            ArmorRule = new ArmorScalingRule(6000, 1, 2),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0 },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.FuelDepot] = new(2000, 2),
                [FacilityKind.FuelRefinery] = new(10000, 0),
                [FacilityKind.RepairBay] = new(20000, 0),
                [FacilityKind.Medical] = new(25000, 0),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(5000, 1),
                [DockingBerthKind.StandardBerth] = new(2500, 2),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.04,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Single] = 0.6,
                    [TurretMount.Double] = 0.4,
                },
                BayTonnageFraction = 0.0,
                BayMix = new Dictionary<BayWeapon, double>(),
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>(),
            OfficerRatio = 0.1,
        };
    }

    private static DesignPreset BuildDefense()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Military,
            ComputerRating = 30,
            Sensors = SensorGrade.Advanced,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.3,
            FuelMonths = 18,
            ArmorMaterial = ArmorMaterial.BondedSuperdense,
            ArmorRule = new ArmorScalingRule(12000, 4, 15),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.FireControl3, SoftwarePackage.AutoRepair, SoftwarePackage.AntiHijack, SoftwarePackage.BattleNetwork, SoftwarePackage.Evade1 },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.CombatInfoCenter] = new(0, 1),
                [FacilityKind.Armory] = new(12000, 1),
                [FacilityKind.Medical] = new(18000, 1),
                [FacilityKind.Training] = new(25000, 0),
                [FacilityKind.FuelDepot] = new(12000, 1),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(6000, 1),
                [DockingBerthKind.StandardBerth] = new(10000, 1),
                [DockingBerthKind.HangarSmall] = new(10000, 1),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.45,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Triple] = 0.3,
                    [TurretMount.Barbette] = 0.5,
                    [TurretMount.Double] = 0.2,
                },
                BayTonnageFraction = 0.035,
                BayMix = new Dictionary<BayWeapon, double>
                {
                    [BayWeapon.Missile50] = 0.25,
                    [BayWeapon.Laser50] = 0.25,
                    [BayWeapon.Particle50] = 0.25,
                    [BayWeapon.Meson50] = 0.25,
                },
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>
            {
                [DefensiveScreen.NuclearDamper] = new(40000, 1),
                [DefensiveScreen.MesonScreen] = new(60000, 1),
            },
            OfficerRatio = 0.25,
        };
    }

    private static DesignPreset BuildFreeport()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 20,
            Sensors = SensorGrade.Military,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.2,
            FuelMonths = 12,
            ArmorMaterial = ArmorMaterial.Crystaliron,
            ArmorRule = new ArmorScalingRule(25000, 2, 8),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0, SoftwarePackage.Intellect, SoftwarePackage.FireControl1, SoftwarePackage.AutoRepair, SoftwarePackage.AntiHijack },
            Facilities = new Dictionary<FacilityKind, ScalingRule>
            {
                [FacilityKind.RepairBay] = new(18000, 1),
                [FacilityKind.FuelRefinery] = new(15000, 1),
                [FacilityKind.FuelDepot] = new(10000, 1),
                [FacilityKind.Commercial] = new(5000, 3),
                [FacilityKind.Warehouse] = new(8000, 2),
                [FacilityKind.Medical] = new(25000, 1),
                [FacilityKind.Recreation] = new(8000, 2),
                [FacilityKind.Customs] = new(35000, 1),
                [FacilityKind.Hydroponics] = new(15000, 1),
                [FacilityKind.CommsArray] = new(0, 1),
            },
            Docking = new Dictionary<DockingBerthKind, ScalingRule>
            {
                [DockingBerthKind.SmallCraftBay] = new(5000, 3),
                [DockingBerthKind.StandardBerth] = new(3000, 5),
                [DockingBerthKind.LargeBerth] = new(15000, 1),
                [DockingBerthKind.HangarSmall] = new(10000, 1),
                [DockingBerthKind.HangarMedium] = new(60000, 0),
            },
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.12,
                TurretMix = new Dictionary<TurretMount, double>
                {
                    [TurretMount.Triple] = 0.5,
                    [TurretMount.Barbette] = 0.2,
                    [TurretMount.Double] = 0.3,
                },
                BayTonnageFraction = 0.006,
                BayMix = new Dictionary<BayWeapon, double>
                {
                    [BayWeapon.Missile50] = 0.5,
                    [BayWeapon.Laser50] = 0.5,
                },
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>
            {
                [DefensiveScreen.NuclearDamper] = new(150000, 0),
            },
            OfficerRatio = 0.2,
        };
    }

    private static DesignPreset BuildCustom()
    {
        return new DesignPreset
        {
            CommandCenter = CommandCenterKind.Standard,
            ComputerRating = 10,
            Sensors = SensorGrade.Basic,
            PowerPlant = PowerPlantKind.Fusion,
            PowerMargin = 1.15,
            FuelMonths = 6,
            ArmorMaterial = ArmorMaterial.None,
            ArmorRule = new ArmorScalingRule(1, 0, 0),
            Software = new[] { SoftwarePackage.Library, SoftwarePackage.Maneuver0 },
            Facilities = new Dictionary<FacilityKind, ScalingRule>(),
            Docking = new Dictionary<DockingBerthKind, ScalingRule>(),
            Weapons = new WeaponAllocation
            {
                TurretHardpointFraction = 0.0,
                TurretMix = new Dictionary<TurretMount, double>(),
                BayTonnageFraction = 0.0,
                BayMix = new Dictionary<BayWeapon, double>(),
            },
            Screens = new Dictionary<DefensiveScreen, ScalingRule>(),
            OfficerRatio = 0.15,
        };
    }
}
