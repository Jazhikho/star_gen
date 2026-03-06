using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;

namespace StarGen.Domain.Systems;

/// <summary>
/// Potential host for planetary orbits.
/// </summary>
public partial class OrbitHost : RefCounted
{
    /// <summary>
    /// Orbit-host categories.
    /// </summary>
    public enum HostType
    {
        SType,
        PType,
    }

    /// <summary>
    /// Identifier of the source hierarchy node.
    /// </summary>
    public string NodeId = string.Empty;

    /// <summary>
    /// Host type.
    /// </summary>
    public HostType Type = HostType.SType;

    /// <summary>
    /// Legacy alias for host type field.
    /// </summary>
    public HostType HostTypeValue
    {
        get => Type;
        set => Type = value;
    }

    /// <summary>
    /// Combined mass in kilograms.
    /// </summary>
    public double CombinedMassKg;

    /// <summary>
    /// Combined luminosity in watts.
    /// </summary>
    public double CombinedLuminosityWatts;

    /// <summary>
    /// Effective temperature in Kelvin.
    /// </summary>
    public double EffectiveTemperatureK;

    /// <summary>
    /// Inner edge of the stable zone in meters.
    /// </summary>
    public double InnerStabilityM;

    /// <summary>
    /// Outer edge of the stable zone in meters.
    /// </summary>
    public double OuterStabilityM;

    /// <summary>
    /// Inner habitable-zone edge in meters.
    /// </summary>
    public double HabitableZoneInnerM;

    /// <summary>
    /// Outer habitable-zone edge in meters.
    /// </summary>
    public double HabitableZoneOuterM;

    /// <summary>
    /// Frost line in meters.
    /// </summary>
    public double FrostLineM;

    /// <summary>
    /// Creates a new orbit host.
    /// </summary>
    public OrbitHost(string nodeId = "", HostType type = HostType.SType)
    {
        NodeId = nodeId;
        Type = type;
    }

    /// <summary>
    /// Returns whether the stable zone is usable.
    /// </summary>
    public bool HasValidZone()
    {
        return OuterStabilityM > InnerStabilityM && InnerStabilityM > 0.0;
    }

    /// <summary>
    /// Returns the width of the stable zone in meters.
    /// </summary>
    public double GetZoneWidthM()
    {
        return System.Math.Max(0.0, OuterStabilityM - InnerStabilityM);
    }

    /// <summary>
    /// Returns the width of the stable zone in AU.
    /// </summary>
    public double GetZoneWidthAu()
    {
        return GetZoneWidthM() / Units.AuMeters;
    }

    /// <summary>
    /// Returns the host type as a display string.
    /// </summary>
    public string GetTypeString()
    {
        return Type switch
        {
            HostType.SType => "S-type",
            HostType.PType => "P-type",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Returns whether a distance lies in the stable zone.
    /// </summary>
    public bool IsDistanceStable(double distanceM)
    {
        return distanceM >= InnerStabilityM && distanceM <= OuterStabilityM;
    }

    /// <summary>
    /// Returns whether a distance lies in the habitable zone.
    /// </summary>
    public bool IsDistanceHabitable(double distanceM)
    {
        return distanceM >= HabitableZoneInnerM && distanceM <= HabitableZoneOuterM;
    }

    /// <summary>
    /// Returns whether a distance lies beyond the frost line.
    /// </summary>
    public bool IsBeyondFrostLine(double distanceM)
    {
        return distanceM >= FrostLineM;
    }

    /// <summary>
    /// Calculates derived temperature-based zones from luminosity.
    /// </summary>
    public void CalculateZones()
    {
        if (CombinedLuminosityWatts <= 0.0)
        {
            HabitableZoneInnerM = 0.0;
            HabitableZoneOuterM = 0.0;
            FrostLineM = 0.0;
            return;
        }

        double luminositySolar = CombinedLuminosityWatts / StellarProps.SolarLuminosityWatts;
        double sqrtLuminosity = System.Math.Sqrt(luminositySolar);
        HabitableZoneInnerM = 0.95 * Units.AuMeters * sqrtLuminosity;
        HabitableZoneOuterM = 1.37 * Units.AuMeters * sqrtLuminosity;
        FrostLineM = 2.7 * Units.AuMeters * sqrtLuminosity;
    }

    /// <summary>
    /// Converts the host to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        string hostTypeStr;
        if (Type == HostType.SType)
        {
            hostTypeStr = "s_type";
        }
        else
        {
            hostTypeStr = "p_type";
        }

        return new Dictionary
        {
            ["node_id"] = NodeId,
            ["host_type"] = hostTypeStr,
            ["combined_mass_kg"] = CombinedMassKg,
            ["combined_luminosity_watts"] = CombinedLuminosityWatts,
            ["effective_temperature_k"] = EffectiveTemperatureK,
            ["inner_stability_m"] = InnerStabilityM,
            ["outer_stability_m"] = OuterStabilityM,
            ["habitable_zone_inner_m"] = HabitableZoneInnerM,
            ["habitable_zone_outer_m"] = HabitableZoneOuterM,
            ["frost_line_m"] = FrostLineM,
        };
    }

    /// <summary>
    /// Creates an orbit host from a dictionary payload.
    /// </summary>
    public static OrbitHost FromDictionary(Dictionary data)
    {
        string hostType = GetString(data, "host_type", "s_type");
        HostType type;
        if (hostType == "s_type")
        {
            type = HostType.SType;
        }
        else
        {
            type = HostType.PType;
        }

        OrbitHost host = new(
            GetString(data, "node_id", string.Empty),
            type);
        host.CombinedMassKg = GetDouble(data, "combined_mass_kg", 0.0);
        host.CombinedLuminosityWatts = GetDouble(data, "combined_luminosity_watts", 0.0);
        host.EffectiveTemperatureK = GetDouble(data, "effective_temperature_k", 0.0);
        host.InnerStabilityM = GetDouble(data, "inner_stability_m", 0.0);
        host.OuterStabilityM = GetDouble(data, "outer_stability_m", 0.0);
        host.HabitableZoneInnerM = GetDouble(data, "habitable_zone_inner_m", 0.0);
        host.HabitableZoneOuterM = GetDouble(data, "habitable_zone_outer_m", 0.0);
        host.FrostLineM = GetDouble(data, "frost_line_m", 0.0);
        return host;
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.String)
        {
            return (string)value;
        }

        return fallback;
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            Variant.Type.String => TryParseDouble((string)value, fallback),
            _ => fallback,
        };
    }

    private static double TryParseDouble(string s, double fallback)
    {
        if (double.TryParse(s, out double parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
