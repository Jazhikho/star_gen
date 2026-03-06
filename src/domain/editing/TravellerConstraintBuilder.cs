using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;

namespace StarGen.Domain.Editing;

/// <summary>
/// Converts Traveller UWP size codes into property constraint bounds.
/// Output is a dictionary of property_path -> Vector2(min, max) in base SI units.
/// </summary>
public static class TravellerConstraintBuilder
{
    private const double CodeEDiameterMaxKm = 300000.0;

    /// <summary>Builds property-path -> Vector2(min, max) constraints for a Traveller size code.</summary>
    public static Dictionary BuildConstraintsForSize(Variant sizeCode)
    {
        Godot.Collections.Dictionary<string, double> diamRange = TravellerSizeCode.CodeToDiameterRange(sizeCode);
        if (diamRange.Count == 0)
        {
            return new Dictionary();
        }

        double diamMinKm = 0.0;
        if (diamRange.TryGetValue("min", out double dMin))
        {
            diamMinKm = dMin;
        }

        double diamMaxKm = -1.0;
        if (diamRange.TryGetValue("max", out double dMax))
        {
            diamMaxKm = dMax;
        }
        if (diamMaxKm < 0.0)
        {
            diamMaxKm = CodeEDiameterMaxKm;
        }

        double rMinM = diamMinKm * 500.0;
        double rMaxM = diamMaxKm * 500.0;
        Vector2 massBounds = MassRangeForRadiusWindow(rMinM, rMaxM);

        Dictionary result = new Dictionary
        {
            ["physical.radius_m"] = new Vector2((float)rMinM, (float)rMaxM),
            ["physical.mass_kg"] = massBounds,
        };
        return result;
    }

    /// <summary>Returns the Traveller size code for a given radius in metres.</summary>
    public static Variant CodeForRadius(double radiusM)
    {
        double diamKm = radiusM * 2.0 / 1000.0;
        object code = TravellerSizeCode.DiameterKmToCode(diamKm);
        if (code is int intCode)
        {
            return Variant.CreateFrom(intCode);
        }

        if (code is string strCode)
        {
            return Variant.CreateFrom(strCode);
        }

        return default;
    }

    /// <summary>Returns all valid size codes as an ordered list (0-9 then "A"-"E").</summary>
    public static Godot.Collections.Array AllCodes()
    {
        Godot.Collections.Array codes = new Godot.Collections.Array();
        for (int i = 0; i < 10; i += 1)
        {
            codes.Add(i);
        }

        foreach (string s in new[] { "A", "B", "C", "D", "E" })
        {
            codes.Add(s);
        }

        return codes;
    }

    /// <summary>Formats a code as its UWP single-character plus a human diameter range.</summary>
    public static string DescribeCode(Variant code)
    {
        string uwp = TravellerSizeCode.ToStringUwp(code);
        Dictionary<string, double> r = TravellerSizeCode.CodeToDiameterRange(code);
        if (r.Count == 0)
        {
            return uwp;
        }

        double lo = 0.0;
        if (r.TryGetValue("min", out double loVal))
        {
            lo = loVal;
        }

        double hi = -1.0;
        if (r.TryGetValue("max", out double hiVal))
        {
            hi = hiVal;
        }
        if (hi < 0.0)
        {
            return $"{uwp} ({FmtKm(lo)} km +)";
        }

        return $"{uwp} ({FmtKm(lo)} - {FmtKm(hi)} km)";
    }

    private static Vector2 MassRangeForRadiusWindow(double rMinM, double rMaxM)
    {
        SizeCategory.Category catMin = CategoryFromRadius(rMinM);
        (double dLo, double _) = SizeTable.GetDensityRangeTuple(catMin);
        double mMin = MassFromRadiusDensity(rMinM, dLo);

        SizeCategory.Category catMax = CategoryFromRadius(rMaxM);
        (double _, double dHi) = SizeTable.GetDensityRangeTuple(catMax);
        double mMax = MassFromRadiusDensity(rMaxM, dHi);

        return new Vector2((float)mMin, (float)mMax);
    }

    private static SizeCategory.Category CategoryFromRadius(double radiusM)
    {
        double guessDensity = 3500.0;
        double guessMass = MassFromRadiusDensity(radiusM, guessDensity);
        return SizeTable.CategoryFromMass(guessMass / Units.EarthMassKg);
    }

    private static double MassFromRadiusDensity(double radiusM, double densityKgM3)
    {
        double vol = (4.0 / 3.0) * System.Math.PI * System.Math.Pow(radiusM, 3.0);
        return vol * densityKgM3;
    }

    private static string FmtKm(double km)
    {
        return ((int)System.Math.Round(km)).ToString();
    }
}
