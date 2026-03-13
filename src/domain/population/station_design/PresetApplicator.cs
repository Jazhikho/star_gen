using System;
using System.Collections.Generic;
using StarGen.Domain.Population.StationDesign.Presets;
using StdMath = System.Math;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Applies preset scaling rules to generate component counts for a given hull tonnage.
/// </summary>
public static class PresetApplicator
{
    /// <summary>
    /// Applies facility scaling rules from a preset.
    /// </summary>
    public static ComponentCounts<FacilityKind> ApplyFacilities(DesignPreset preset, int hullTons)
    {
        return ApplyRules(preset.Facilities, hullTons);
    }

    /// <summary>
    /// Applies docking scaling rules from a preset.
    /// </summary>
    public static ComponentCounts<DockingBerthKind> ApplyDocking(DesignPreset preset, int hullTons)
    {
        return ApplyRules(preset.Docking, hullTons);
    }

    /// <summary>
    /// Applies defensive screen scaling rules from a preset.
    /// </summary>
    public static ComponentCounts<DefensiveScreen> ApplyScreens(DesignPreset preset, int hullTons)
    {
        return ApplyRules(preset.Screens, hullTons);
    }

    /// <summary>
    /// Applies armor scaling rule from a preset.
    /// </summary>
    public static int ApplyArmorPoints(DesignPreset preset, int hullTons)
    {
        return preset.ArmorRule.Apply(hullTons);
    }

    /// <summary>
    /// Distributes turret mounts across hardpoints using the preset's mix fractions.
    /// </summary>
    public static ComponentCounts<TurretMount> ApplyTurrets(DesignPreset preset, int hullTons)
    {
        int hardpoints = hullTons / 100;
        int total = (int)StdMath.Floor(hardpoints * preset.Weapons.TurretHardpointFraction);
        return DistributeMix(preset.Weapons.TurretMix, total);
    }

    /// <summary>
    /// Distributes bay weapons within a tonnage budget using the preset's mix fractions.
    /// </summary>
    public static ComponentCounts<BayWeapon> ApplyBays(DesignPreset preset, int hullTons)
    {
        int totalBudget = (int)StdMath.Floor(hullTons * preset.Weapons.BayTonnageFraction);
        ComponentCounts<BayWeapon> bays = new();
        int remaining = totalBudget;
        KeyValuePair<BayWeapon, double>[] bayMix = ToArray(preset.Weapons.BayMix);

        for (int i = 0; i < bayMix.Length; i++)
        {
            KeyValuePair<BayWeapon, double> entry = bayMix[i];
            int bayTonnage = HullCalculator.BayWeaponTonnage(entry.Key);
            if (bayTonnage <= 0)
            {
                continue;
            }

            int allocation;
            if (i == bayMix.Length - 1)
            {
                allocation = remaining;
            }
            else
            {
                allocation = (int)StdMath.Floor(totalBudget * entry.Value);
            }

            int count = allocation / bayTonnage;
            bays[entry.Key] = count;
            remaining -= count * bayTonnage;
        }

        return bays;
    }

    /// <summary>
    /// Generates accommodation counts from crew total and officer ratio.
    /// </summary>
    public static ComponentCounts<AccommodationKind> ApplyAccommodations(int crewTotal, double officerRatio)
    {
        ComponentCounts<AccommodationKind> accommodations = new();
        int officers = StdMath.Max(2, (int)StdMath.Ceiling(crewTotal * officerRatio));
        int enlisted = StdMath.Max(0, crewTotal - officers);

        accommodations[AccommodationKind.Stateroom] = officers;
        accommodations[AccommodationKind.Barracks] = (int)StdMath.Ceiling(enlisted / 4.0);
        accommodations[AccommodationKind.EmergencyLow] = StdMath.Max(1, (int)StdMath.Ceiling(crewTotal / 50.0));
        return accommodations;
    }

    private static ComponentCounts<T> ApplyRules<T>(IReadOnlyDictionary<T, ScalingRule> rules, int hullTons)
        where T : struct, Enum
    {
        ComponentCounts<T> counts = new();
        foreach (KeyValuePair<T, ScalingRule> entry in rules)
        {
            int count = entry.Value.Apply(hullTons);
            if (count > 0)
            {
                counts[entry.Key] = count;
            }
        }

        return counts;
    }

    private static ComponentCounts<T> DistributeMix<T>(IReadOnlyDictionary<T, double> mix, int total)
        where T : struct, Enum
    {
        ComponentCounts<T> counts = new();
        KeyValuePair<T, double>[] entries = ToArray(mix);
        int remaining = total;

        for (int i = 0; i < entries.Length; i++)
        {
            int count;
            if (i == entries.Length - 1)
            {
                count = remaining;
            }
            else
            {
                count = (int)StdMath.Floor(total * entries[i].Value);
            }

            counts[entries[i].Key] = count;
            remaining -= count;
        }

        return counts;
    }

    private static KeyValuePair<T, double>[] ToArray<T>(IReadOnlyDictionary<T, double> mix) where T : struct, Enum
    {
        KeyValuePair<T, double>[] entries = new KeyValuePair<T, double>[mix.Count];
        int index = 0;
        foreach (KeyValuePair<T, double> entry in mix)
        {
            entries[index] = entry;
            index += 1;
        }

        return entries;
    }
}
