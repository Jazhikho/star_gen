using System;
using System.Collections.Generic;
using StarGen.Domain.Population.StationDesign.Presets;
using StdMath = System.Math;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Main station design calculator. Same spec must always produce the same result.
/// </summary>
public static class DesignCalculator
{
    /// <summary>
    /// Computes a complete station design from a spec.
    /// </summary>
    public static DesignResult Calculate(DesignSpec spec)
    {
        int hullTons = spec.HullTons;
        DesignPreset preset = DesignPresetCatalog.Get(spec.Template);
        ComponentSelection selection = ResolveSelection(spec, preset);

        int armorTons = HullCalculator.ArmorTonnage(
            hullTons,
            selection.ArmorMaterial,
            selection.ArmorPoints,
            spec.Configuration);
        int commandTons = HullCalculator.CommandTonnage(hullTons, selection.CommandCenter);
        int sensorTons = HullCalculator.SensorTonnage(selection.Sensors);
        int fixedTons = armorTons + commandTons + sensorTons;
        int commandPower = HullCalculator.CommandPower(commandTons);

        int estimatedPower = commandPower
            + HullCalculator.SensorPower(selection.Sensors)
            + selection.Facilities.SumBy(HullCalculator.FacilityPower)
            + selection.Turrets.SumBy(HullCalculator.TurretPower)
            + selection.Bays.SumBy(HullCalculator.BayWeaponPower)
            + selection.Screens.SumBy(HullCalculator.ScreenPower);

        if (spec.IsAuto(AutoPopulateFlags.Engineering))
        {
            selection.PowerRating = StdMath.Max(10, (int)StdMath.Ceiling(estimatedPower * preset.PowerMargin));
        }

        int estimatedPowerPlantTons = HullCalculator.PowerPlantTonnage(selection.PowerRating, selection.PowerPlant);
        int estimatedFuelTons = HullCalculator.FuelTonnage(selection.PowerRating, selection.PowerPlant, selection.FuelMonths);
        int estimatedCrewQuartersTons = (int)StdMath.Ceiling(hullTons / 350.0) * 3;

        BudgetFitter.Fit(
            selection,
            hullTons,
            spec.AutoFlags,
            fixedTons,
            estimatedPowerPlantTons,
            estimatedFuelTons,
            estimatedCrewQuartersTons);

        int totalPower = commandPower
            + HullCalculator.SensorPower(selection.Sensors)
            + selection.Facilities.SumBy(HullCalculator.FacilityPower)
            + selection.Turrets.SumBy(HullCalculator.TurretPower)
            + selection.Bays.SumBy(HullCalculator.BayWeaponPower)
            + selection.Screens.SumBy(HullCalculator.ScreenPower);

        if (spec.IsAuto(AutoPopulateFlags.Engineering))
        {
            selection.PowerRating = StdMath.Max(10, (int)StdMath.Ceiling(totalPower * preset.PowerMargin));
        }

        int powerPlantTons = HullCalculator.PowerPlantTonnage(selection.PowerRating, selection.PowerPlant);
        int fuelTons = HullCalculator.FuelTonnage(selection.PowerRating, selection.PowerPlant, selection.FuelMonths);

        CrewBreakdown crew = CrewCalculator.Calculate(
            hullTons,
            powerPlantTons,
            fuelTons,
            selection.Turrets,
            selection.Bays,
            selection.Docking,
            selection.Facilities);

        if (spec.IsAuto(AutoPopulateFlags.Quarters))
        {
            selection.Accommodations = PresetApplicator.ApplyAccommodations(crew.Total, preset.OfficerRatio);
        }

        int weaponsTons = selection.Turrets.SumBy(HullCalculator.TurretTonnage)
            + selection.Bays.SumBy(HullCalculator.BayWeaponTonnage);
        int screensTons = selection.Screens.SumBy(HullCalculator.ScreenTonnage);
        int dockingTons = selection.Docking.SumBy(HullCalculator.DockingTonnage);
        int quartersTons = AccommodationTonnage(selection.Accommodations);
        int facilitiesTons = selection.Facilities.SumBy(HullCalculator.FacilityTonnage);

        int usedTons = armorTons + commandTons + sensorTons + powerPlantTons + fuelTons
            + weaponsTons + screensTons + dockingTons + quartersTons + facilitiesTons;
        int cargoTons = StdMath.Max(0, hullTons - usedTons);

        long hullCost = HullCalculator.HullCost(hullTons, spec.Configuration);
        long armorCost = armorTons * ArmorCostPerTon(selection.ArmorMaterial);
        long commandCost = HullCalculator.CommandCost(commandTons, selection.CommandCenter);
        long computerCost = HullCalculator.ComputerCost(selection.ComputerRating);
        long sensorCost = HullCalculator.SensorCost(selection.Sensors);
        long powerPlantCost = HullCalculator.PowerPlantCost(powerPlantTons, selection.PowerPlant);
        long weaponsCost = selection.Turrets.SumByLong(HullCalculator.TurretCost)
            + selection.Bays.SumByLong(HullCalculator.BayWeaponCost);
        long screensCost = selection.Screens.SumByLong(HullCalculator.ScreenCost);
        long dockingCost = selection.Docking.SumByLong(HullCalculator.DockingCost);
        long accommodationsCost = selection.Accommodations.SumByLong(HullCalculator.AccommodationCost);
        long facilitiesCost = selection.Facilities.SumByLong(HullCalculator.FacilityCost);
        long softwareCost = 0L;
        foreach (SoftwarePackage software in selection.Software)
        {
            softwareCost += HullCalculator.SoftwareCost(software);
        }

        long totalCost = hullCost + armorCost + commandCost + computerCost + sensorCost
            + powerPlantCost + weaponsCost + screensCost + dockingCost + accommodationsCost
            + facilitiesCost + softwareCost;

        int berthsAvailable = 0;
        foreach (KeyValuePair<AccommodationKind, int> entry in selection.Accommodations)
        {
            berthsAvailable += HullCalculator.AccommodationOccupancy(entry.Key) * entry.Value;
        }

        int hardpointsUsed = selection.Turrets.SumBy(HullCalculator.TurretHardpoints);
        List<string> warnings = BuildWarnings(
            hullTons,
            usedTons,
            totalPower,
            selection.PowerRating,
            crew.Total,
            berthsAvailable,
            hardpointsUsed);

        return new DesignResult
        {
            Spec = spec,
            Selection = selection,
            CatalogVersion = DesignPresetCatalog.CatalogVersion,
            StructureHitPoints = HullCalculator.StructureHitPoints(hullTons),
            Hardpoints = HullCalculator.Hardpoints(hullTons),
            HardpointsUsed = hardpointsUsed,
            EffectiveArmorPoints = HullCalculator.EffectiveArmorPoints(
                selection.ArmorMaterial,
                selection.ArmorPoints,
                spec.Configuration),
            BerthsAvailable = berthsAvailable,
            Crew = crew,
            Tonnage = new TonnageBreakdown
            {
                Armor = armorTons,
                Command = commandTons,
                Sensors = sensorTons,
                PowerPlant = powerPlantTons,
                Fuel = fuelTons,
                Weapons = weaponsTons,
                Screens = screensTons,
                Docking = dockingTons,
                Quarters = quartersTons,
                Facilities = facilitiesTons,
                Used = usedTons,
                Cargo = cargoTons,
            },
            Cost = new CostBreakdown
            {
                Hull = hullCost,
                Armor = armorCost,
                Command = commandCost,
                Computer = computerCost,
                Sensors = sensorCost,
                PowerPlant = powerPlantCost,
                Weapons = weaponsCost,
                Screens = screensCost,
                Docking = dockingCost,
                Quarters = accommodationsCost,
                Facilities = facilitiesCost,
                Software = softwareCost,
                Total = totalCost,
            },
            Power = new PowerBudget
            {
                Demand = totalPower,
                Output = selection.PowerRating,
                Surplus = selection.PowerRating - totalPower,
            },
            Warnings = warnings,
        };
    }

    private static ComponentSelection ResolveSelection(DesignSpec spec, DesignPreset preset)
    {
        ComponentSelection selection = new();
        int hullTons = spec.HullTons;

        if (spec.IsAuto(AutoPopulateFlags.Engineering))
        {
            selection.PowerPlant = preset.PowerPlant;
            selection.FuelMonths = preset.FuelMonths;
        }

        if (spec.IsAuto(AutoPopulateFlags.Command))
        {
            selection.CommandCenter = preset.CommandCenter;
            selection.ComputerRating = preset.ComputerRating;
            selection.Sensors = preset.Sensors;
            selection.Software = new List<SoftwarePackage>(preset.Software);
        }

        if (spec.IsAuto(AutoPopulateFlags.Defenses))
        {
            selection.ArmorMaterial = preset.ArmorMaterial;
            selection.ArmorPoints = PresetApplicator.ApplyArmorPoints(preset, hullTons);
            selection.Turrets = PresetApplicator.ApplyTurrets(preset, hullTons);
            selection.Bays = PresetApplicator.ApplyBays(preset, hullTons);
            selection.Screens = PresetApplicator.ApplyScreens(preset, hullTons);
        }

        if (spec.IsAuto(AutoPopulateFlags.Docking))
        {
            selection.Docking = PresetApplicator.ApplyDocking(preset, hullTons);
        }

        if (spec.IsAuto(AutoPopulateFlags.Facilities))
        {
            selection.Facilities = PresetApplicator.ApplyFacilities(preset, hullTons);
        }

        return selection;
    }

    private static int AccommodationTonnage(ComponentCounts<AccommodationKind> accommodations)
    {
        double total = 0.0;
        foreach (KeyValuePair<AccommodationKind, int> entry in accommodations)
        {
            total += HullCalculator.AccommodationTonnage(entry.Key) * entry.Value;
        }

        return (int)StdMath.Ceiling(total);
    }

    private static long ArmorCostPerTon(ArmorMaterial material)
    {
        if (ComponentCatalog.ArmorMaterials.TryGetValue(material, out Components.ArmorMaterialProps props))
        {
            return props.CostPerTon;
        }

        return 0L;
    }

    private static List<string> BuildWarnings(
        int hullTons,
        int usedTons,
        int powerDemand,
        int powerOutput,
        int crewTotal,
        int berthsAvailable,
        int hardpointsUsed)
    {
        List<string> warnings = new();
        if (usedTons > hullTons)
        {
            warnings.Add($"Over tonnage by {usedTons - hullTons}t");
        }

        if (powerDemand > powerOutput)
        {
            warnings.Add($"Power deficit: {powerDemand - powerOutput} PP short");
        }

        if (berthsAvailable < crewTotal)
        {
            warnings.Add($"Need {crewTotal - berthsAvailable} more berths");
        }

        int hardpoints = HullCalculator.Hardpoints(hullTons);
        if (hardpointsUsed > hardpoints)
        {
            warnings.Add($"Hardpoints exceeded: {hardpointsUsed}/{hardpoints}");
        }

        return warnings;
    }
}
