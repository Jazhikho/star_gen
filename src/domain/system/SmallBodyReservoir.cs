using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems;

/// <summary>
/// Source-backed small-body reservoir record for populations that should not be collapsed into asteroid-belt geometry.
/// </summary>
public partial class SmallBodyReservoir : RefCounted
{
    /// <summary>
    /// Unique reservoir identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Display name for the reservoir.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Identifier of the orbit host associated with this reservoir.
    /// </summary>
    public string OrbitHostId = string.Empty;

    /// <summary>
    /// Identifier of the belt or band that currently anchors this reservoir in the viewer.
    /// </summary>
    public string AnchorBeltId = string.Empty;

    /// <summary>
    /// Broad reservoir family such as main asteroid belt or trans-Neptunian reservoir.
    /// </summary>
    public string ReservoirKind = string.Empty;

    /// <summary>
    /// Specific reservoir family represented by this record.
    /// </summary>
    public string ReservoirFamily = string.Empty;

    /// <summary>
    /// Relative family weight within the source reservoir proxy.
    /// </summary>
    public double RelativeWeight;

    /// <summary>
    /// Inner radial extent in meters.
    /// </summary>
    public double InnerRadiusM;

    /// <summary>
    /// Outer radial extent in meters.
    /// </summary>
    public double OuterRadiusM;

    /// <summary>
    /// Semicolon-delimited source IDs supporting this reservoir record.
    /// </summary>
    public string SourceIds = string.Empty;

    /// <summary>
    /// Generator-facing population model label.
    /// </summary>
    public string PopulationModel = string.Empty;

    /// <summary>
    /// Implementation status for audit and UI/export expansion.
    /// </summary>
    public string RepresentationStatus = "diagnostic_proxy";

    /// <summary>
    /// Creates a new small-body reservoir.
    /// </summary>
    public SmallBodyReservoir(string id = "", string name = "")
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Converts the reservoir to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["orbit_host_id"] = OrbitHostId,
            ["anchor_belt_id"] = AnchorBeltId,
            ["reservoir_kind"] = ReservoirKind,
            ["reservoir_family"] = ReservoirFamily,
            ["relative_weight"] = RelativeWeight,
            ["inner_radius_m"] = InnerRadiusM,
            ["outer_radius_m"] = OuterRadiusM,
            ["source_ids"] = SourceIds,
            ["population_model"] = PopulationModel,
            ["representation_status"] = RepresentationStatus,
        };
    }

    /// <summary>
    /// Creates a reservoir from a dictionary payload.
    /// </summary>
    public static SmallBodyReservoir FromDictionary(Dictionary data)
    {
        SmallBodyReservoir reservoir = new(
            GetString(data, "id", string.Empty),
            GetString(data, "name", string.Empty));
        reservoir.OrbitHostId = GetString(data, "orbit_host_id", string.Empty);
        reservoir.AnchorBeltId = GetString(data, "anchor_belt_id", string.Empty);
        reservoir.ReservoirKind = GetString(data, "reservoir_kind", string.Empty);
        reservoir.ReservoirFamily = GetString(data, "reservoir_family", string.Empty);
        reservoir.RelativeWeight = GetDouble(data, "relative_weight", 0.0);
        reservoir.InnerRadiusM = GetDouble(data, "inner_radius_m", 0.0);
        reservoir.OuterRadiusM = GetDouble(data, "outer_radius_m", 0.0);
        reservoir.SourceIds = GetString(data, "source_ids", string.Empty);
        reservoir.PopulationModel = GetString(data, "population_model", string.Empty);
        reservoir.RepresentationStatus = GetString(data, "representation_status", "diagnostic_proxy");
        return reservoir;
    }

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

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        switch (value.VariantType)
        {
            case Variant.Type.Float:
                return (double)value;
            case Variant.Type.Int:
                return value.AsInt64();
            case Variant.Type.String:
                return TryParseDouble((string)value, fallback);
            default:
                return fallback;
        }
    }

    private static double TryParseDouble(string text, double fallback)
    {
        if (double.TryParse(text, out double parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
