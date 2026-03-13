using System;
using System.Collections.Generic;
using StdMath = System.Math;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Calculates crew requirements from resolved component selection.
/// </summary>
public static class CrewCalculator
{
    /// <summary>
    /// Computes the full crew breakdown.
    /// </summary>
    public static CrewBreakdown Calculate(
        int hullTons, int ppTons, int fuelTons,
        ComponentCounts<TurretMount> turrets, ComponentCounts<BayWeapon> bays,
        ComponentCounts<DockingBerthKind> docking, ComponentCounts<FacilityKind> facilities)
    {
        int cmd = CommandCrew(hullTons);
        int eng = EngineeringCrew(ppTons, fuelTons);
        int gun = GunneryCrew(turrets, bays);
        int dock = DockingCrew(docking);
        int maint = MaintenanceCrew(hullTons);
        int med = MedicalCrew(facilities);
        int sec = SecurityCrew(hullTons);
        int fac = FacilitiesCrew(facilities);
        int baseTotal = cmd + eng + gun + dock + maint + med + sec + fac;
        int admin = AdminCrew(baseTotal);
        int total = baseTotal + admin;

        return new CrewBreakdown
        {
            Command = cmd, Engineering = eng, Gunnery = gun, Docking = dock,
            Maintenance = maint, Medical = med, Security = sec, Facilities = fac,
            Admin = admin, Total = total,
        };
    }

    /// <summary>
    /// Returns command crew from hull size.
    /// </summary>
    public static int CommandCrew(int hullTons)
    {
        return StdMath.Max(2, (int)StdMath.Ceiling(hullTons / 5000.0));
    }

    /// <summary>
    /// Returns engineering crew from power plant and fuel tonnage.
    /// </summary>
    public static int EngineeringCrew(int ppTons, int fuelTons)
    {
        return StdMath.Max(1, (int)StdMath.Ceiling(ppTons / 1000.0))
            + StdMath.Max(0, (int)StdMath.Ceiling(fuelTons / 5000.0));
    }

    /// <summary>
    /// Returns gunnery crew from turret and bay counts.
    /// </summary>
    public static int GunneryCrew(ComponentCounts<TurretMount> turrets, ComponentCounts<BayWeapon> bays)
    {
        return turrets.Sum() + bays.Sum() * 4;
    }

    /// <summary>
    /// Returns docking crew from berth counts.
    /// </summary>
    public static int DockingCrew(ComponentCounts<DockingBerthKind> docking)
    {
        int dock = 0;
        foreach (KeyValuePair<DockingBerthKind, int> entry in docking)
        {
            if (entry.Key == DockingBerthKind.SmallCraftBay || entry.Key == DockingBerthKind.HangarSmall)
            {
                dock += entry.Value;
            }
            else
            {
                dock += entry.Value * 2;
            }
        }

        return dock;
    }

    /// <summary>
    /// Returns maintenance crew from hull size.
    /// </summary>
    public static int MaintenanceCrew(int hullTons)
    {
        return StdMath.Max(1, (int)StdMath.Ceiling(hullTons / 1000.0));
    }

    /// <summary>
    /// Returns medical crew from facilities.
    /// </summary>
    public static int MedicalCrew(ComponentCounts<FacilityKind> facilities)
    {
        return facilities[FacilityKind.Medical] * 2;
    }

    /// <summary>
    /// Returns security crew from hull size.
    /// </summary>
    public static int SecurityCrew(int hullTons)
    {
        return StdMath.Max(1, (int)StdMath.Ceiling(hullTons / 2000.0));
    }

    /// <summary>
    /// Returns facility crew from staffed modules.
    /// </summary>
    public static int FacilitiesCrew(ComponentCounts<FacilityKind> facilities)
    {
        int facilitiesCrew = 0;
        foreach (KeyValuePair<FacilityKind, int> entry in facilities)
        {
            facilitiesCrew += entry.Key switch
            {
                FacilityKind.ShipyardSmall => entry.Value * 10,
                FacilityKind.ShipyardMedium => entry.Value * 10,
                FacilityKind.ShipyardLarge => entry.Value * 10,
                FacilityKind.Manufacturing => entry.Value * 5,
                FacilityKind.OreProcessing => entry.Value * 5,
                FacilityKind.Laboratory => entry.Value * 3,
                FacilityKind.Commercial => entry.Value * 3,
                FacilityKind.FuelRefinery => entry.Value * 2,
                FacilityKind.Training => entry.Value * 2,
                _ => 0,
            };
        }

        return facilitiesCrew;
    }

    /// <summary>
    /// Returns admin crew from the base crew total.
    /// </summary>
    public static int AdminCrew(int baseTotal)
    {
        return StdMath.Max(1, (int)StdMath.Ceiling(baseTotal / 20.0));
    }
}
