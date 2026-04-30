using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;

namespace StarGen.Domain.Systems;

/// <summary>
/// Candidate orbital position within an orbit host.
/// </summary>
public partial class OrbitSlot : RefCounted
{
    /// <summary>
    /// Unique slot identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Source orbit-host identifier.
    /// </summary>
    public string OrbitHostId = string.Empty;

    /// <summary>
    /// Semi-major axis in meters.
    /// </summary>
    public double SemiMajorAxisM;

    /// <summary>
    /// Suggested eccentricity.
    /// </summary>
    public double SuggestedEccentricity;

    /// <summary>
    /// Orbit-zone classification.
    /// </summary>
    public OrbitZone.Zone Zone = OrbitZone.Zone.Temperate;

    /// <summary>
    /// Whether the slot is dynamically stable.
    /// </summary>
    public bool IsStable = true;

    /// <summary>
    /// Fill probability in the range [0, 1].
    /// </summary>
    public double FillProbability = 0.5;

    /// <summary>
    /// Stability or spacing policy used when this slot was generated.
    /// </summary>
    public string StabilityPolicyId = OrbitalMechanics.CompactArchitectureSpacingPolicyId;

    /// <summary>
    /// Source-note identifiers used by the spacing policy.
    /// </summary>
    public string StabilitySourceIds = OrbitalMechanics.CompactArchitectureSpacingSourceIds;

    /// <summary>
    /// Adjacent spacing from the inner slot in mutual Hill radii, if applicable.
    /// </summary>
    public double SpacingFromInnerMutualHillRadii;

    /// <summary>
    /// Orbital period ratio relative to the inner slot, if applicable.
    /// </summary>
    public double PeriodRatioFromInner;

    /// <summary>
    /// Minimum spacing threshold used by the policy, in mutual Hill radii.
    /// </summary>
    public double MinimumSpacingMutualHillRadii = OrbitalMechanics.MinimumAdjacentPlanetSpacingMutualHillRadii;

    /// <summary>
    /// Candidate architecture mass proxy used for the spacing scaffold, in Earth masses.
    /// </summary>
    public double SpacingMassProxyEarthMasses;

    /// <summary>
    /// Whether the slot is already filled.
    /// </summary>
    public bool IsFilled;

    /// <summary>
    /// Identifier of the occupying planet.
    /// </summary>
    public string PlanetId = string.Empty;

    /// <summary>
    /// Creates a new orbit slot.
    /// </summary>
    public OrbitSlot(string id = "", string orbitHostId = "", double semiMajorAxisM = 0.0)
    {
        Id = id;
        OrbitHostId = orbitHostId;
        SemiMajorAxisM = semiMajorAxisM;
    }

    /// <summary>
    /// Returns the semi-major axis in AU.
    /// </summary>
    public double GetSemiMajorAxisAu()
    {
        return SemiMajorAxisM / Units.AuMeters;
    }

    /// <summary>
    /// Returns the orbit-zone display string.
    /// </summary>
    public string GetZoneString()
    {
        return OrbitZone.ToStringName(Zone);
    }

    /// <summary>
    /// Marks the slot as filled by a planet.
    /// </summary>
    public void FillWithPlanet(string planetId)
    {
        IsFilled = true;
        PlanetId = planetId;
    }

    /// <summary>
    /// Clears the occupying planet reference.
    /// </summary>
    public void ClearPlanet()
    {
        IsFilled = false;
        PlanetId = string.Empty;
    }

    /// <summary>
    /// Returns whether the slot is currently available.
    /// </summary>
    public bool IsAvailable()
    {
        return IsStable && !IsFilled;
    }

    /// <summary>
    /// Converts the slot to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["id"] = Id,
            ["orbit_host_id"] = OrbitHostId,
            ["semi_major_axis_m"] = SemiMajorAxisM,
            ["suggested_eccentricity"] = SuggestedEccentricity,
            ["zone"] = OrbitZone.ToStringName(Zone).ToLowerInvariant(),
            ["is_stable"] = IsStable,
            ["fill_probability"] = FillProbability,
            ["stability_policy_id"] = StabilityPolicyId,
            ["stability_source_ids"] = StabilitySourceIds,
            ["spacing_from_inner_mutual_hill_radii"] = SpacingFromInnerMutualHillRadii,
            ["period_ratio_from_inner"] = PeriodRatioFromInner,
            ["minimum_spacing_mutual_hill_radii"] = MinimumSpacingMutualHillRadii,
            ["spacing_mass_proxy_earth_masses"] = SpacingMassProxyEarthMasses,
            ["is_filled"] = IsFilled,
            ["planet_id"] = PlanetId,
        };
    }

    /// <summary>
    /// Creates an orbit slot from a dictionary payload.
    /// </summary>
    public static OrbitSlot FromDictionary(Dictionary data)
    {
        OrbitSlot slot = new(
            GetString(data, "id", string.Empty),
            GetString(data, "orbit_host_id", string.Empty),
            GetDouble(data, "semi_major_axis_m", 0.0));
        slot.SuggestedEccentricity = GetDouble(data, "suggested_eccentricity", 0.0);

        string zoneName = GetString(data, "zone", "temperate");
        if (OrbitZone.TryParse(zoneName, out OrbitZone.Zone zone))
        {
            slot.Zone = zone;
        }
        else
        {
            slot.Zone = OrbitZone.Zone.Temperate;
        }

        slot.IsStable = GetBool(data, "is_stable", true);
        slot.FillProbability = GetDouble(data, "fill_probability", 0.5);
        slot.StabilityPolicyId = GetString(data, "stability_policy_id", OrbitalMechanics.CompactArchitectureSpacingPolicyId);
        slot.StabilitySourceIds = GetString(data, "stability_source_ids", OrbitalMechanics.CompactArchitectureSpacingSourceIds);
        slot.SpacingFromInnerMutualHillRadii = GetDouble(data, "spacing_from_inner_mutual_hill_radii", 0.0);
        slot.PeriodRatioFromInner = GetDouble(data, "period_ratio_from_inner", 0.0);
        slot.MinimumSpacingMutualHillRadii = GetDouble(data, "minimum_spacing_mutual_hill_radii", OrbitalMechanics.MinimumAdjacentPlanetSpacingMutualHillRadii);
        slot.SpacingMassProxyEarthMasses = GetDouble(data, "spacing_mass_proxy_earth_masses", 0.0);
        slot.IsFilled = GetBool(data, "is_filled", false);
        slot.PlanetId = GetString(data, "planet_id", string.Empty);
        return slot;
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

    /// <summary>
    /// Reads a boolean value from a dictionary.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool)
        {
            return (bool)data[key];
        }

        return fallback;
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
