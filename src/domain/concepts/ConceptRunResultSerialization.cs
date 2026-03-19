using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Concepts;

/// <summary>
/// Serialization helpers for persisted concept run results.
/// </summary>
public static class ConceptRunResultSerialization
{
    /// <summary>
    /// Converts a concept result into a dictionary payload.
    /// </summary>
    public static Dictionary ToDictionary(ConceptRunResult result)
    {
        Array<Dictionary> metrics = new();
        foreach (ConceptMetric metric in result.Metrics)
        {
            metrics.Add(new Dictionary
            {
                ["label"] = metric.Label,
                ["value"] = metric.Value,
                ["max_value"] = metric.MaxValue,
                ["display_text"] = metric.DisplayText,
            });
        }

        Array<Dictionary> sections = new();
        foreach (ConceptSection section in result.Sections)
        {
            Array<string> items = new();
            foreach (string item in section.Items)
            {
                items.Add(item);
            }

            sections.Add(new Dictionary
            {
                ["title"] = section.Title,
                ["items"] = items,
            });
        }

        return new Dictionary
        {
            ["title"] = result.Title,
            ["subtitle"] = result.Subtitle,
            ["summary"] = result.Summary,
            ["metrics"] = metrics,
            ["sections"] = sections,
            ["provenance"] = new Dictionary
            {
                ["concept_id"] = result.Provenance.ConceptId,
                ["seed"] = result.Provenance.Seed,
                ["generator_version"] = result.Provenance.GeneratorVersion,
                ["source_context"] = result.Provenance.SourceContext,
            },
        };
    }

    /// <summary>
    /// Creates a concept result from a dictionary payload.
    /// </summary>
    public static ConceptRunResult FromDictionary(Dictionary data)
    {
        ConceptRunResult result = new()
        {
            Title = GetString(data, "title"),
            Subtitle = GetString(data, "subtitle"),
            Summary = GetString(data, "summary"),
        };

        if (data.ContainsKey("metrics") && data["metrics"].VariantType == Variant.Type.Array)
        {
            foreach (Variant metricValue in (Array)data["metrics"])
            {
                if (metricValue.VariantType != Variant.Type.Dictionary)
                {
                    continue;
                }

                Dictionary metricData = (Dictionary)metricValue;
                result.Metrics.Add(new ConceptMetric
                {
                    Label = GetString(metricData, "label"),
                    Value = GetDouble(metricData, "value"),
                    MaxValue = GetDouble(metricData, "max_value", 1.0),
                    DisplayText = GetString(metricData, "display_text"),
                });
            }
        }

        if (data.ContainsKey("sections") && data["sections"].VariantType == Variant.Type.Array)
        {
            foreach (Variant sectionValue in (Array)data["sections"])
            {
                if (sectionValue.VariantType != Variant.Type.Dictionary)
                {
                    continue;
                }

                Dictionary sectionData = (Dictionary)sectionValue;
                ConceptSection section = new()
                {
                    Title = GetString(sectionData, "title"),
                };

                if (sectionData.ContainsKey("items") && sectionData["items"].VariantType == Variant.Type.Array)
                {
                    foreach (Variant itemValue in (Array)sectionData["items"])
                    {
                        if (itemValue.VariantType == Variant.Type.String)
                        {
                            section.Items.Add((string)itemValue);
                        }
                    }
                }

                result.Sections.Add(section);
            }
        }

        if (data.ContainsKey("provenance") && data["provenance"].VariantType == Variant.Type.Dictionary)
        {
            Dictionary provenance = (Dictionary)data["provenance"];
            result.Provenance = new ConceptProvenance
            {
                ConceptId = GetString(provenance, "concept_id"),
                Seed = GetInt(provenance, "seed"),
                GeneratorVersion = GetString(provenance, "generator_version"),
                SourceContext = GetString(provenance, "source_context"),
            };
        }

        return result;
    }

    /// <summary>
    /// Returns a deep clone of a concept result.
    /// </summary>
    public static ConceptRunResult Clone(ConceptRunResult result)
    {
        return FromDictionary(ToDictionary(result));
    }

    private static string GetString(Dictionary data, string key, string fallback = "")
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

    private static int GetInt(Dictionary data, string key, int fallback = 0)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    private static double GetDouble(Dictionary data, string key, double fallback = 0.0)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (double)value,
            _ => fallback,
        };
    }
}
