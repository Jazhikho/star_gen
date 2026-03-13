using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population.StationDesign.Classification;

/// <summary>
/// Per-requirement outcome within a classification.
/// </summary>
public sealed class RequirementResult
{
    public string Label = string.Empty;
    public bool Met;
}

/// <summary>
/// Outcome for a single classification.
/// </summary>
public sealed class ClassificationResult
{
    public ClassificationId Id;
    public string DisplayName = string.Empty;
    public string Icon = string.Empty;
    public List<RequirementResult> Requirements = new();
    public int MetCount;
    public int TotalCount;
    public bool Earned;
}

/// <summary>
/// Full classification report: all results plus the earned subset.
/// </summary>
public sealed class ClassificationReport
{
    public List<ClassificationId> Earned = new();
    public System.Collections.Generic.Dictionary<ClassificationId, ClassificationResult> Results = new();

    /// <summary>
    /// Returns whether the report includes a classification.
    /// </summary>
    public bool IsEarned(ClassificationId id)
    {
        return Earned.Contains(id);
    }

    /// <summary>
    /// Serializes the report to a Godot dictionary.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<int> earned = new();
        foreach (ClassificationId id in Earned)
        {
            earned.Add((int)id);
        }

        Dictionary results = new();
        foreach (KeyValuePair<ClassificationId, ClassificationResult> entry in Results)
        {
            Array reqs = new();
            foreach (RequirementResult requirement in entry.Value.Requirements)
            {
                reqs.Add(new Dictionary
                {
                    ["label"] = requirement.Label,
                    ["met"] = requirement.Met,
                });
            }

            results[(int)entry.Key] = new Dictionary
            {
                ["display_name"] = entry.Value.DisplayName,
                ["icon"] = entry.Value.Icon,
                ["met_count"] = entry.Value.MetCount,
                ["total_count"] = entry.Value.TotalCount,
                ["earned"] = entry.Value.Earned,
                ["requirements"] = reqs,
            };
        }

        return new Dictionary
        {
            ["earned"] = earned,
            ["results"] = results,
        };
    }

    /// <summary>
    /// Rebuilds a report from a Godot dictionary.
    /// </summary>
    public static ClassificationReport FromDictionary(Dictionary? data)
    {
        ClassificationReport report = new();
        if (data == null)
        {
            return report;
        }

        if (data.ContainsKey("earned") && data["earned"].VariantType == Variant.Type.Array)
        {
            foreach (Variant rawId in (Array)data["earned"])
            {
                if (rawId.VariantType == Variant.Type.Int)
                {
                    report.Earned.Add((ClassificationId)(int)rawId);
                }
            }
        }

        if (data.ContainsKey("results") && data["results"].VariantType == Variant.Type.Dictionary)
        {
            Dictionary rawResults = (Dictionary)data["results"];
            foreach (Variant rawKey in rawResults.Keys)
            {
                if (rawKey.VariantType != Variant.Type.Int)
                {
                    continue;
                }

                Variant rawValue = rawResults[rawKey];
                if (rawValue.VariantType != Variant.Type.Dictionary)
                {
                    continue;
                }

                Dictionary resultData = (Dictionary)rawValue;
                ClassificationResult result = new()
                {
                    Id = (ClassificationId)(int)rawKey,
                    DisplayName = GetString(resultData, "display_name", string.Empty),
                    Icon = GetString(resultData, "icon", string.Empty),
                    MetCount = GetInt(resultData, "met_count", 0),
                    TotalCount = GetInt(resultData, "total_count", 0),
                    Earned = GetBool(resultData, "earned", false),
                };

                if (resultData.ContainsKey("requirements") && resultData["requirements"].VariantType == Variant.Type.Array)
                {
                    foreach (Variant rawRequirement in (Array)resultData["requirements"])
                    {
                        if (rawRequirement.VariantType != Variant.Type.Dictionary)
                        {
                            continue;
                        }

                        Dictionary reqData = (Dictionary)rawRequirement;
                        result.Requirements.Add(new RequirementResult
                        {
                            Label = GetString(reqData, "label", string.Empty),
                            Met = GetBool(reqData, "met", false),
                        });
                    }
                }

                report.Results[result.Id] = result;
            }
        }

        return report;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.String)
        {
            return fallback;
        }

        return value.AsString();
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Int)
        {
            return fallback;
        }

        return (int)value;
    }

    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Bool)
        {
            return fallback;
        }

        return (bool)value;
    }
}
