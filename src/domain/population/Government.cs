using Godot;
using Godot.Collections;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Population;

/// <summary>
/// Government and regime state for a population.
/// </summary>
public partial class Government : RefCounted
{
    /// <summary>
    /// Current regime type.
    /// </summary>
    public GovernmentType.Regime Regime = GovernmentType.Regime.Tribal;

    /// <summary>
    /// Coercion centralization in the range [0, 1].
    /// </summary>
    public double CoercionCentralization;

    /// <summary>
    /// Administrative capacity in the range [0, 1].
    /// </summary>
    public double AdministrativeCapacity;

    /// <summary>
    /// Political inclusiveness in the range [0, 1].
    /// </summary>
    public double PoliticalInclusiveness;

    /// <summary>
    /// Legitimacy in the range [0, 1].
    /// </summary>
    public double Legitimacy = 0.5;

    /// <summary>
    /// Year the current regime was established.
    /// </summary>
    public int RegimeEstablishedYear;

    /// <summary>
    /// Optional government name.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Creates a default native government.
    /// </summary>
    public static Government CreateNativeDefault(SeededRng rng)
    {
        Government government = new();
        Array<GovernmentType.Regime> starting = GovernmentType.NativeStartingRegimes();
        government.Regime = starting[rng.RandiRange(0, starting.Count - 1)];
        government.CoercionCentralization = rng.RandfRange(0.05f, 0.2f);
        government.AdministrativeCapacity = rng.RandfRange(0.05f, 0.15f);
        government.PoliticalInclusiveness = rng.RandfRange(0.2f, 0.5f);
        government.Legitimacy = rng.RandfRange(0.6f, 0.9f);
        return government;
    }

    /// <summary>
    /// Creates a default colony government.
    /// </summary>
    public static Government CreateColonyDefault(SeededRng rng, string colonyType = "")
    {
        Government government = new();
        Array<GovernmentType.Regime> starting = GovernmentType.ColonyStartingRegimes();

        government.Regime = colonyType.ToLowerInvariant() switch
        {
            "corporate" => GovernmentType.Regime.Corporate,
            "mining" => GovernmentType.Regime.Corporate,
            "commercial" => GovernmentType.Regime.Corporate,
            "military" => GovernmentType.Regime.MilitaryJunta,
            "outpost" => GovernmentType.Regime.MilitaryJunta,
            "strategic" => GovernmentType.Regime.MilitaryJunta,
            "scientific" => GovernmentType.Regime.Technocracy,
            "research" => GovernmentType.Regime.Technocracy,
            "religious" => GovernmentType.Regime.Theocracy,
            "faith" => GovernmentType.Regime.Theocracy,
            "penal" => GovernmentType.Regime.MilitaryJunta,
            "exile" => GovernmentType.Regime.MilitaryJunta,
            _ => starting[rng.RandiRange(0, starting.Count - 1)],
        };

        government.CoercionCentralization = rng.RandfRange(0.4f, 0.7f);
        government.AdministrativeCapacity = rng.RandfRange(0.3f, 0.6f);
        government.PoliticalInclusiveness = rng.RandfRange(0.1f, 0.4f);
        government.Legitimacy = rng.RandfRange(0.4f, 0.7f);
        return government;
    }

    /// <summary>
    /// Returns whether the government is politically stable.
    /// </summary>
    public bool IsStable()
    {
        if (GovernmentType.IsUnstable(Regime))
        {
            return false;
        }

        return Legitimacy > 0.3;
    }

    /// <summary>
    /// Returns whether regime change is likely.
    /// </summary>
    public bool IsRegimeChangeLikely()
    {
        if (Legitimacy < 0.2)
        {
            return true;
        }

        double mismatch = CalculateSliderMismatch();
        return mismatch > 0.5;
    }

    /// <summary>
    /// Returns a compact summary of the government state.
    /// </summary>
    public Dictionary GetSummary()
    {
        return new Dictionary
        {
            ["regime"] = GovernmentType.ToStringName(Regime),
            ["coercion"] = CoercionCentralization,
            ["admin_capacity"] = AdministrativeCapacity,
            ["inclusiveness"] = PoliticalInclusiveness,
            ["legitimacy"] = Legitimacy,
            ["stable"] = IsStable(),
        };
    }

    /// <summary>
    /// Converts this government to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["regime"] = (int)Regime,
            ["coercion_centralization"] = CoercionCentralization,
            ["administrative_capacity"] = AdministrativeCapacity,
            ["political_inclusiveness"] = PoliticalInclusiveness,
            ["legitimacy"] = Legitimacy,
            ["regime_established_year"] = RegimeEstablishedYear,
            ["name"] = Name,
        };
    }

    /// <summary>
    /// Creates a government from a dictionary payload.
    /// </summary>
    public static Government FromDictionary(Dictionary data)
    {
        Government government = new();
        Variant regimeValue = data.ContainsKey("regime") ? data["regime"] : 0;
        if (regimeValue.VariantType == Variant.Type.String)
        {
            regimeValue = int.Parse((string)regimeValue);
        }

        government.Regime = (GovernmentType.Regime)(int)regimeValue;
        government.CoercionCentralization = System.Math.Clamp(GetDouble(data, "coercion_centralization", 0.0), 0.0, 1.0);
        government.AdministrativeCapacity = System.Math.Clamp(GetDouble(data, "administrative_capacity", 0.0), 0.0, 1.0);
        government.PoliticalInclusiveness = System.Math.Clamp(GetDouble(data, "political_inclusiveness", 0.0), 0.0, 1.0);
        government.Legitimacy = System.Math.Clamp(GetDouble(data, "legitimacy", 0.5), 0.0, 1.0);
        government.RegimeEstablishedYear = GetInt(data, "regime_established_year", 0);
        government.Name = GetString(data, "name", string.Empty);
        return government;
    }

    private double CalculateSliderMismatch()
    {
        Dictionary expected = GetExpectedSliderRanges();
        double mismatch = 0.0;

        mismatch += CalculateMismatch(CoercionCentralization, GetDouble(expected, "coercion_min", 0.0), GetDouble(expected, "coercion_max", 1.0));
        mismatch += CalculateMismatch(AdministrativeCapacity, GetDouble(expected, "admin_min", 0.0), GetDouble(expected, "admin_max", 1.0));
        mismatch += CalculateMismatch(PoliticalInclusiveness, GetDouble(expected, "inclusive_min", 0.0), GetDouble(expected, "inclusive_max", 1.0));

        return System.Math.Clamp(mismatch, 0.0, 1.0);
    }

    private Dictionary GetExpectedSliderRanges()
    {
        return Regime switch
        {
            GovernmentType.Regime.Tribal => BuildRanges(0.0, 0.3, 0.0, 0.2, 0.2, 0.6),
            GovernmentType.Regime.Chiefdom => BuildRanges(0.1, 0.4, 0.1, 0.3, 0.1, 0.4),
            GovernmentType.Regime.Feudal => BuildRanges(0.2, 0.5, 0.1, 0.4, 0.05, 0.2),
            GovernmentType.Regime.AbsoluteMonarchy => BuildRanges(0.5, 0.9, 0.3, 0.7, 0.0, 0.2),
            GovernmentType.Regime.BureaucraticEmpire => BuildRanges(0.6, 1.0, 0.5, 1.0, 0.0, 0.3),
            GovernmentType.Regime.MassDemocracy => BuildRanges(0.4, 0.8, 0.4, 0.9, 0.6, 1.0),
            GovernmentType.Regime.MilitaryJunta => BuildRanges(0.6, 1.0, 0.2, 0.6, 0.0, 0.2),
            GovernmentType.Regime.FailedState => BuildRanges(0.0, 0.3, 0.0, 0.2, 0.0, 0.3),
            _ => BuildRanges(0.0, 1.0, 0.0, 1.0, 0.0, 1.0),
        };
    }

    private static Dictionary BuildRanges(
        double coercionMin,
        double coercionMax,
        double adminMin,
        double adminMax,
        double inclusiveMin,
        double inclusiveMax)
    {
        return new Dictionary
        {
            ["coercion_min"] = coercionMin,
            ["coercion_max"] = coercionMax,
            ["admin_min"] = adminMin,
            ["admin_max"] = adminMax,
            ["inclusive_min"] = inclusiveMin,
            ["inclusive_max"] = inclusiveMax,
        };
    }

    private static double CalculateMismatch(double value, double minimum, double maximum)
    {
        if (value < minimum)
        {
            return minimum - value;
        }

        if (value > maximum)
        {
            return value - maximum;
        }

        return 0.0;
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        return data.ContainsKey(key) ? (double)data[key] : fallback;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        return data.ContainsKey(key) ? (int)data[key] : fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        return data.ContainsKey(key) ? (string)data[key] : fallback;
    }
}
