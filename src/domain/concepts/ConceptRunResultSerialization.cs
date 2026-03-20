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
            ["status"] = (int)result.Status,
            ["status_reason"] = result.StatusReason,
            ["title"] = result.Title,
            ["subtitle"] = result.Subtitle,
            ["summary"] = result.Summary,
            ["metrics"] = metrics,
            ["sections"] = sections,
            ["provenance"] = ToProvenanceDictionary(result.Provenance),
        };
    }

    /// <summary>
    /// Creates a concept result from a dictionary payload.
    /// </summary>
    public static ConceptRunResult FromDictionary(Dictionary data)
    {
        ConceptRunResult result = new()
        {
            Status = (ConceptRunStatus)ConceptSerializationUtils.ReadOptionalInt(data, "status", (int)ConceptRunStatus.Generated),
            StatusReason = ConceptSerializationUtils.ReadOptionalString(data, "status_reason"),
            Title = ConceptSerializationUtils.ReadString(data, "title"),
            Subtitle = ConceptSerializationUtils.ReadString(data, "subtitle"),
            Summary = ConceptSerializationUtils.ReadString(data, "summary"),
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
                    Label = ConceptSerializationUtils.ReadString(metricData, "label"),
                    Value = ConceptSerializationUtils.ReadDouble(metricData, "value"),
                    MaxValue = ConceptSerializationUtils.ReadOptionalDouble(metricData, "max_value", 1.0),
                    DisplayText = ConceptSerializationUtils.ReadOptionalString(metricData, "display_text"),
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
                    Title = ConceptSerializationUtils.ReadString(sectionData, "title"),
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
            result.Provenance = FromProvenanceDictionary(provenance);
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

    /// <summary>
    /// Converts provenance into a dictionary payload.
    /// </summary>
    public static Dictionary ToProvenanceDictionary(ConceptProvenance provenance)
    {
        Array<string> dependencies = new();
        foreach (string dependency in provenance.UpstreamDependencies)
        {
            dependencies.Add(dependency);
        }

        return new Dictionary
        {
            ["concept_id"] = provenance.ConceptId,
            ["seed"] = provenance.Seed,
            ["generator_version"] = provenance.GeneratorVersion,
            ["source_context"] = provenance.SourceContext,
            ["input_signature"] = provenance.InputSignature,
            ["upstream_dependencies"] = dependencies,
        };
    }

    /// <summary>
    /// Rehydrates provenance from a dictionary payload.
    /// </summary>
    public static ConceptProvenance FromProvenanceDictionary(Dictionary provenance)
    {
        ConceptProvenance result = new ConceptProvenance
        {
            ConceptId = ConceptSerializationUtils.ReadString(provenance, "concept_id"),
            Seed = ConceptSerializationUtils.ReadInt(provenance, "seed"),
            GeneratorVersion = ConceptSerializationUtils.ReadString(provenance, "generator_version"),
            SourceContext = ConceptSerializationUtils.ReadString(provenance, "source_context"),
            InputSignature = ConceptSerializationUtils.ReadOptionalString(provenance, "input_signature"),
        };

        if (provenance.ContainsKey("upstream_dependencies") && provenance["upstream_dependencies"].VariantType == Variant.Type.Array)
        {
            foreach (Variant dependencyValue in (Array)provenance["upstream_dependencies"])
            {
                if (dependencyValue.VariantType == Variant.Type.String)
                {
                    result.UpstreamDependencies.Add((string)dependencyValue);
                }
            }
        }

        return result;
    }

}
