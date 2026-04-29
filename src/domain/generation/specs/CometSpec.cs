using Godot;
using Godot.Collections;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Specification for comet generation.
/// </summary>
public partial class CometSpec : BaseSpec
{
    /// <summary>
    /// Target comet family, or -1 for random.
    /// </summary>
    public int Family { get; set; }

    /// <summary>
    /// Target activity state, or -1 for random.
    /// </summary>
    public int ActivityState { get; set; }

    /// <summary>
    /// Whether the comet should bias toward a larger nucleus.
    /// </summary>
    public bool IsLarge { get; set; }

    /// <summary>
    /// Target comet nucleus model, or -1 for the source-backed default.
    /// </summary>
    public int NucleusModel { get; set; }

    /// <summary>
    /// Target comet activity model, or -1 for the source-backed default.
    /// </summary>
    public int ActivityModel { get; set; }

    /// <summary>
    /// Continuous comet-radius scale applied after the selected nucleus model.
    /// </summary>
    public double SizeScale { get; set; }

    /// <summary>
    /// Minor-body population slope proxy used by size sampling.
    /// </summary>
    public double PopulationSlope { get; set; }

    /// <summary>
    /// Creates a new comet specification.
    /// </summary>
    public CometSpec(
        int generationSeed = 0,
        int family = -1,
        int activityState = -1,
        bool isLarge = false,
        int nucleusModel = -1,
        int activityModel = -1,
        double sizeScale = 1.0,
        double populationSlope = 2.0,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : base(generationSeed, nameHint, overrides, useCaseSettings)
    {
        Family = family;
        ActivityState = activityState;
        IsLarge = isLarge;
        NucleusModel = nucleusModel;
        ActivityModel = activityModel;
        SizeScale = sizeScale;
        PopulationSlope = populationSlope;
    }

    /// <summary>
    /// Compatibility constructor for callers that pass name and overrides after the legacy large-comet flag.
    /// </summary>
    public CometSpec(
        int generationSeed,
        int family,
        int activityState,
        bool isLarge,
        string nameHint,
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : this(
            generationSeed,
            family,
            activityState,
            isLarge,
            -1,
            -1,
            1.0,
            2.0,
            nameHint,
            overrides,
            useCaseSettings)
    {
    }

    /// <summary>
    /// Creates a fully random comet specification.
    /// </summary>
    public static CometSpec Random(int generationSeed) => new(generationSeed);

    /// <summary>
    /// Creates a Jupiter-family comet specification.
    /// </summary>
    public static CometSpec JupiterFamily(int generationSeed)
    {
        return new CometSpec(generationSeed, 0, 0, false);
    }

    /// <summary>
    /// Creates a long-period comet specification.
    /// </summary>
    public static CometSpec LongPeriod(int generationSeed)
    {
        return new CometSpec(generationSeed, 1, 0, false);
    }

    /// <summary>
    /// Returns whether a comet family was specified.
    /// </summary>
    public bool HasFamily() => Family >= 0;

    /// <summary>
    /// Returns whether an activity state was specified.
    /// </summary>
    public bool HasActivityState() => ActivityState >= 0;

    /// <summary>
    /// Returns whether a comet nucleus model was specified.
    /// </summary>
    public bool HasNucleusModel() => NucleusModel >= 0;

    /// <summary>
    /// Returns whether a comet activity model was specified.
    /// </summary>
    public bool HasActivityModel() => ActivityModel >= 0;

    /// <summary>
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "comet";
        data["family"] = Family;
        data["activity_state"] = ActivityState;
        data["is_large"] = IsLarge;
        data["nucleus_model"] = NucleusModel;
        data["activity_model"] = ActivityModel;
        data["size_scale"] = SizeScale;
        data["population_slope"] = PopulationSlope;
        return data;
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static CometSpec FromDictionary(Dictionary data)
    {
        int generationSeed;
        if (data.ContainsKey("generation_seed"))
        {
            generationSeed = (int)data["generation_seed"];
        }
        else
        {
            generationSeed = 0;
        }

        int family;
        if (data.ContainsKey("family"))
        {
            family = (int)data["family"];
        }
        else
        {
            family = -1;
        }

        int activityState;
        if (data.ContainsKey("activity_state"))
        {
            activityState = (int)data["activity_state"];
        }
        else
        {
            activityState = -1;
        }

        string nameHint;
        if (data.ContainsKey("name_hint"))
        {
            nameHint = (string)data["name_hint"];
        }
        else
        {
            nameHint = string.Empty;
        }

        Dictionary? overrides = null;
        if (data.ContainsKey("overrides"))
        {
            overrides = (Dictionary)data["overrides"];
        }

        bool isLarge = data.ContainsKey("is_large") && (bool)data["is_large"];
        int nucleusModel = data.ContainsKey("nucleus_model") ? (int)data["nucleus_model"] : -1;
        int activityModel = data.ContainsKey("activity_model") ? (int)data["activity_model"] : -1;
        double sizeScale = data.ContainsKey("size_scale") ? GetDouble(data["size_scale"], 1.0) : 1.0;
        double populationSlope = data.ContainsKey("population_slope") ? GetDouble(data["population_slope"], 2.0) : 2.0;
        CometSpec spec = new CometSpec(
            generationSeed,
            family,
            activityState,
            isLarge,
            nucleusModel,
            activityModel,
            sizeScale,
            populationSlope,
            nameHint,
            overrides);
        spec.ApplyBaseFromDictionary(data);
        return spec;
    }

    private static double GetDouble(Variant value, double fallback)
    {
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            _ => fallback,
        };
    }
}
