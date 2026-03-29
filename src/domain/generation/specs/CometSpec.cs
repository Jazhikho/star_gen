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
    /// Creates a new comet specification.
    /// </summary>
    public CometSpec(
        int generationSeed = 0,
        int family = -1,
        int activityState = -1,
        bool isLarge = false,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : base(generationSeed, nameHint, overrides, useCaseSettings)
    {
        Family = family;
        ActivityState = activityState;
        IsLarge = isLarge;
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
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "comet";
        data["family"] = Family;
        data["activity_state"] = ActivityState;
        data["is_large"] = IsLarge;
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
        CometSpec spec = new CometSpec(generationSeed, family, activityState, isLarge, nameHint, overrides);
        spec.ApplyBaseFromDictionary(data);
        return spec;
    }
}
