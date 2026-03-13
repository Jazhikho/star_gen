using System;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Scales down auto-populated module sections when the total estimated
/// tonnage exceeds the hull budget.
/// </summary>
public static class BudgetFitter
{
    public const double BudgetFraction = 0.88;
    public const double MinScaleFactor = 0.25;
    public const double ModuleFloorFraction = 0.3;

    /// <summary>
    /// If estimated tonnage exceeds hull × BudgetFraction, proportionally
    /// scales down only the auto-populated sections.
    /// </summary>
    public static double Fit(
        ComponentSelection sel,
        int hullTons,
        AutoPopulateFlags autoFlags,
        int fixedTons,
        int estPowerPlantTons,
        int estFuelTons,
        int estCrewTons)
    {
        int moduleFacTons = sel.Facilities.SumBy(HullCalculator.FacilityTonnage);
        int moduleDockTons = sel.Docking.SumBy(HullCalculator.DockingTonnage);
        int moduleWpnTons = sel.Turrets.SumBy(HullCalculator.TurretTonnage)
            + sel.Bays.SumBy(HullCalculator.BayWeaponTonnage)
            + sel.Screens.SumBy(HullCalculator.ScreenTonnage);
        int moduleTons = moduleFacTons + moduleDockTons + moduleWpnTons;

        int totalEst = fixedTons + moduleTons + estPowerPlantTons + estFuelTons + estCrewTons;
        double budget = hullTons * BudgetFraction;

        if (totalEst <= budget || moduleTons <= 0)
        {
            return 1.0;
        }

        double target = budget - fixedTons - estPowerPlantTons - estFuelTons - estCrewTons;
        if (target < moduleTons * ModuleFloorFraction)
        {
            target = moduleTons * ModuleFloorFraction;
        }

        double scale = target / moduleTons;
        if (scale < MinScaleFactor)
        {
            scale = MinScaleFactor;
        }

        if (scale >= 1.0)
        {
            return 1.0;
        }

        if ((autoFlags & AutoPopulateFlags.Facilities) != 0)
        {
            sel.Facilities = sel.Facilities.Scale(scale);
        }

        if ((autoFlags & AutoPopulateFlags.Docking) != 0)
        {
            sel.Docking = sel.Docking.Scale(scale);
        }

        if ((autoFlags & AutoPopulateFlags.Defenses) != 0)
        {
            sel.Turrets = sel.Turrets.Scale(scale);
            sel.Bays = sel.Bays.Scale(scale);
            sel.Screens = sel.Screens.Scale(scale);
        }

        return scale;
    }
}
