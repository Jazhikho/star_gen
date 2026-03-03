using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Specification for station generation.
/// </summary>
public partial class StationSpec : RefCounted
{
    /// <summary>
    /// Seed for deterministic generation.
    /// </summary>
    public int GenerationSeed;

    /// <summary>
    /// Whether to generate stations at all.
    /// </summary>
    public bool GenerateStations = true;

    /// <summary>
    /// Forced placement context, or null to auto-detect.
    /// </summary>
    public StationPlacementContext.Context? ForceContext;

    /// <summary>
    /// Minimum number of stations.
    /// </summary>
    public int MinStations;

    /// <summary>
    /// Maximum number of stations, or 0 for no limit.
    /// </summary>
    public int MaxStations;

    /// <summary>
    /// Whether utility stations are allowed.
    /// </summary>
    public bool AllowUtility = true;

    /// <summary>
    /// Whether outposts are allowed.
    /// </summary>
    public bool AllowOutposts = true;

    /// <summary>
    /// Whether large stations are allowed.
    /// </summary>
    public bool AllowLargeStations = true;

    /// <summary>
    /// Whether deep-space stations are allowed.
    /// </summary>
    public bool AllowDeepSpace = true;

    /// <summary>
    /// Whether belt stations are allowed.
    /// </summary>
    public bool AllowBeltStations = true;

    /// <summary>
    /// Minimum establishment year.
    /// </summary>
    public int MinEstablishedYear = -500;

    /// <summary>
    /// Maximum establishment year.
    /// </summary>
    public int MaxEstablishedYear;

    /// <summary>
    /// Population-density modifier.
    /// </summary>
    public double PopulationDensity = 1.0;

    /// <summary>
    /// Chance for decommissioning.
    /// </summary>
    public double DecommissionChance = 0.05;

    /// <summary>
    /// Required station purposes.
    /// </summary>
    public Array<StationPurpose.Purpose> RequiredPurposes = new();

    /// <summary>
    /// Excluded station purposes.
    /// </summary>
    public Array<StationPurpose.Purpose> ExcludedPurposes = new();

    /// <summary>
    /// ID prefix for generated stations.
    /// </summary>
    public string IdPrefix = "station";

    /// <summary>
    /// Founding civilization identifier.
    /// </summary>
    public string FoundingCivilizationId = string.Empty;

    /// <summary>
    /// Founding civilization display name.
    /// </summary>
    public string FoundingCivilizationName = string.Empty;

    /// <summary>
    /// Creates a minimal spec.
    /// </summary>
    public static StationSpec Minimal()
    {
        StationSpec spec = new();
        spec.AllowLargeStations = false;
        spec.MaxStations = 2;
        spec.PopulationDensity = 0.5;
        return spec;
    }

    /// <summary>
    /// Creates a standard spec.
    /// </summary>
    public static StationSpec Standard()
    {
        return new StationSpec();
    }

    /// <summary>
    /// Creates a dense spec.
    /// </summary>
    public static StationSpec Dense()
    {
        StationSpec spec = new();
        spec.PopulationDensity = 2.0;
        spec.MinStations = 2;
        return spec;
    }

    /// <summary>
    /// Creates a spec for a forced context.
    /// </summary>
    public static StationSpec ForContext(StationPlacementContext.Context context)
    {
        StationSpec spec = new();
        spec.ForceContext = context;
        return spec;
    }

    /// <summary>
    /// Returns whether a purpose is allowed by the spec.
    /// </summary>
    public bool IsPurposeAllowed(StationPurpose.Purpose purpose)
    {
        if (ExcludedPurposes.Contains(purpose))
        {
            return false;
        }

        if (RequiredPurposes.Count == 0)
        {
            return true;
        }

        return RequiredPurposes.Contains(purpose);
    }

    /// <summary>
    /// Returns whether a station class is allowed.
    /// </summary>
    public bool IsClassAllowed(StationClass.Class stationClass)
    {
        return stationClass switch
        {
            StationClass.Class.U => AllowUtility,
            StationClass.Class.O => AllowOutposts,
            StationClass.Class.B => AllowLargeStations,
            StationClass.Class.A => AllowLargeStations,
            StationClass.Class.S => AllowLargeStations,
            _ => true,
        };
    }

    /// <summary>
    /// Validates the spec and returns any errors.
    /// </summary>
    public Array<string> Validate()
    {
        Array<string> errors = new();

        if (MinStations > MaxStations && MaxStations > 0)
        {
            errors.Add($"min_stations ({MinStations}) cannot exceed max_stations ({MaxStations})");
        }

        if (PopulationDensity < 0.0)
        {
            errors.Add("population_density cannot be negative");
        }

        if (DecommissionChance < 0.0 || DecommissionChance > 1.0)
        {
            errors.Add("decommission_chance must be between 0 and 1");
        }

        if (MinEstablishedYear > MaxEstablishedYear)
        {
            errors.Add("min_established_year cannot exceed max_established_year");
        }

        return errors;
    }

    /// <summary>
    /// Returns whether the spec is valid.
    /// </summary>
    public bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Converts the spec to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<int> required = new();
        foreach (StationPurpose.Purpose purpose in RequiredPurposes)
        {
            required.Add((int)purpose);
        }

        Array<int> excluded = new();
        foreach (StationPurpose.Purpose purpose in ExcludedPurposes)
        {
            excluded.Add((int)purpose);
        }

        Dictionary data = new()
        {
            ["seed"] = GenerationSeed,
            ["generate_stations"] = GenerateStations,
            ["min_stations"] = MinStations,
            ["max_stations"] = MaxStations,
            ["allow_utility"] = AllowUtility,
            ["allow_outposts"] = AllowOutposts,
            ["allow_large_stations"] = AllowLargeStations,
            ["allow_deep_space"] = AllowDeepSpace,
            ["allow_belt_stations"] = AllowBeltStations,
            ["min_established_year"] = MinEstablishedYear,
            ["max_established_year"] = MaxEstablishedYear,
            ["population_density"] = PopulationDensity,
            ["decommission_chance"] = DecommissionChance,
            ["required_purposes"] = required,
            ["excluded_purposes"] = excluded,
            ["id_prefix"] = IdPrefix,
            ["founding_civilization_id"] = FoundingCivilizationId,
            ["founding_civilization_name"] = FoundingCivilizationName,
        };

        if (ForceContext.HasValue)
        {
            data["force_context"] = (int)ForceContext.Value;
        }

        return data;
    }

    /// <summary>
    /// Creates a station spec from a dictionary payload.
    /// </summary>
    public static StationSpec FromDictionary(Dictionary data)
    {
        StationSpec spec = new();
        spec.GenerationSeed = GetInt(data, "seed", 0);
        spec.GenerateStations = GetBool(data, "generate_stations", true);
        spec.MinStations = GetInt(data, "min_stations", 0);
        spec.MaxStations = GetInt(data, "max_stations", 0);
        spec.AllowUtility = GetBool(data, "allow_utility", true);
        spec.AllowOutposts = GetBool(data, "allow_outposts", true);
        spec.AllowLargeStations = GetBool(data, "allow_large_stations", true);
        spec.AllowDeepSpace = GetBool(data, "allow_deep_space", true);
        spec.AllowBeltStations = GetBool(data, "allow_belt_stations", true);
        spec.MinEstablishedYear = GetInt(data, "min_established_year", -500);
        spec.MaxEstablishedYear = GetInt(data, "max_established_year", 0);
        spec.PopulationDensity = GetDouble(data, "population_density", 1.0);
        spec.DecommissionChance = GetDouble(data, "decommission_chance", 0.05);
        spec.IdPrefix = GetString(data, "id_prefix", "station");
        spec.FoundingCivilizationId = GetString(data, "founding_civilization_id", string.Empty);
        spec.FoundingCivilizationName = GetString(data, "founding_civilization_name", string.Empty);
        if (data.ContainsKey("force_context"))
        {
            spec.ForceContext = (StationPlacementContext.Context)GetInt(data, "force_context", (int)StationPlacementContext.Context.Other);
        }

        spec.RequiredPurposes = GetPurposeArray(data, "required_purposes");
        spec.ExcludedPurposes = GetPurposeArray(data, "excluded_purposes");
        return spec;
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
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : fallback,
            _ => fallback,
        };
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
            Variant.Type.String => double.TryParse((string)value, out double parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a boolean value from a dictionary.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool ? (bool)data[key] : fallback;
    }

    /// <summary>
    /// Reads a station-purpose array from a dictionary.
    /// </summary>
    private static Array<StationPurpose.Purpose> GetPurposeArray(Dictionary data, string key)
    {
        Array<StationPurpose.Purpose> result = new();
        if (!data.ContainsKey(key) || data[key].VariantType != Variant.Type.Array)
        {
            return result;
        }

        foreach (Variant value in (Array)data[key])
        {
            if (value.VariantType == Variant.Type.Int)
            {
                result.Add((StationPurpose.Purpose)(int)value);
            }
            else if (value.VariantType == Variant.Type.String && int.TryParse((string)value, out int parsed))
            {
                result.Add((StationPurpose.Purpose)parsed);
            }
        }

        return result;
    }
}
