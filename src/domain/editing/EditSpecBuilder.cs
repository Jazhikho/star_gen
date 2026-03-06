using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;

namespace StarGen.Domain.Editing;

/// <summary>
/// Builds generator spec overrides from a ConstraintSet's locked properties.
/// Bridges the editing layer (base SI units, uniform property paths) and the generation layer.
/// </summary>
public static class EditSpecBuilder
{
    private const double FactorUnity = 1.0;

    private static Dictionary AliasTableFor(CelestialType.Type bodyType)
    {
        if (bodyType != CelestialType.Type.Star)
        {
            return new Dictionary();
        }

        Dictionary starAliases = new Dictionary
        {
            ["physical.mass_kg"] = new Godot.Collections.Array
            {
                new Dictionary { ["key"] = "physical.mass_solar", ["factor_name"] = "SOLAR_MASS_KG_INV" },
            },
            ["physical.radius_m"] = new Godot.Collections.Array
            {
                new Dictionary { ["key"] = "physical.radius_solar", ["factor_name"] = "SOLAR_RADIUS_M_INV" },
            },
            ["stellar.luminosity_watts"] = new Godot.Collections.Array
            {
                new Dictionary { ["key"] = "stellar.luminosity_solar", ["factor_name"] = "SOLAR_LUM_W_INV" },
            },
        };
        return starAliases;
    }

    /// <summary>Builds a spec-overrides Dictionary from the locked properties of a ConstraintSet.</summary>
    public static Dictionary BuildOverrides(CelestialType.Type bodyType, ConstraintSet constraints)
    {
        Dictionary overrides = constraints.GetLockedOverrides();
        Dictionary aliases = AliasTableFor(bodyType);

        foreach (Variant pathV in overrides.Keys)
        {
            string pathStr = (string)pathV;
            if (!aliases.ContainsKey(pathStr))
            {
                continue;
            }

            double baseVal = (double)overrides[pathV];
            Godot.Collections.Array aliasList = (Godot.Collections.Array)aliases[pathStr];
            foreach (Variant entryV in aliasList)
            {
                Dictionary e = (Dictionary)entryV;
                string key = (string)e["key"];
                double factor = ResolveFactor((string)e["factor_name"]);
                overrides[key] = baseVal * factor;
            }
        }

        return overrides;
    }

    /// <summary>Populates a spec's overrides from a ConstraintSet. Existing overrides are cleared first.</summary>
    public static void ApplyToSpec(BaseSpec spec, CelestialType.Type bodyType, ConstraintSet constraints)
    {
        spec.ClearOverrides();
        Dictionary overrides = BuildOverrides(bodyType, constraints);
        foreach (Variant keyV in overrides.Keys)
        {
            spec.SetOverride((string)keyV, overrides[keyV]);
        }
    }

    private static double ResolveFactor(string name)
    {
        if (name == "UNITY")
        {
            return FactorUnity;
        }

        if (name == "SOLAR_MASS_KG_INV")
        {
            return 1.0 / Units.SolarMassKg;
        }

        if (name == "SOLAR_RADIUS_M_INV")
        {
            return 1.0 / Units.SolarRadiusMeters;
        }

        if (name == "SOLAR_LUM_W_INV")
        {
            return 1.0 / StellarProps.SolarLuminosityWatts;
        }

        return FactorUnity;
    }

}
