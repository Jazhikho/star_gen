using System;
using System.Text;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Pipeline;

namespace StarGen.Services.Concepts;

/// <summary>
/// Shared concept-result builder used by the atlas UI and persisted world-state generation.
/// </summary>
public static partial class ConceptResultFactory
{
    /// <summary>
    /// Runs a concept module against a context snapshot.
    /// </summary>
    public static ConceptRunResult Run(ConceptRunRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Context == null)
        {
            throw new ArgumentNullException(nameof(request.Context));
        }

        if (request.Kind == ConceptKind.Ecology)
        {
            return BuildEcologyResult(request.Context);
        }

        if (request.Kind == ConceptKind.Religion)
        {
            return BuildReligionResult(request.Context);
        }

        if (request.Kind == ConceptKind.Civilization)
        {
            return BuildCivilizationResult(request.Context);
        }

        if (request.Kind == ConceptKind.Language)
        {
            return BuildLanguageResult(request.Context);
        }

        if (request.Kind == ConceptKind.Disease)
        {
            return BuildDiseaseResult(request.Context);
        }

        if (request.Kind == ConceptKind.Evolution)
        {
            return BuildEvolutionResult(request.Context);
        }

        throw new InvalidOperationException("No concept result factory is registered for kind '" + request.Kind + "'.");
    }

    /// <summary>
    /// Converts internal identifiers such as snake_case and PascalCase tokens into sentence-case display text.
    /// </summary>
    private static string FormatIdentifierForDisplay(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.Trim().Replace('_', ' ').Replace('-', ' ');
        StringBuilder builder = new StringBuilder(normalized.Length + 8);
        for (int index = 0; index < normalized.Length; index += 1)
        {
            char current = normalized[index];
            if (index > 0 && char.IsUpper(current))
            {
                char previous = normalized[index - 1];
                if (char.IsLower(previous) || char.IsDigit(previous))
                {
                    builder.Append(' ');
                }
            }

            builder.Append(current);
        }

        string collapsed = builder.ToString();
        string lowered = collapsed.ToLowerInvariant();
        if (lowered.Length == 1)
        {
            return lowered.ToUpperInvariant();
        }

        return char.ToUpperInvariant(lowered[0]) + lowered.Substring(1);
    }

    private static ConceptRunResult BuildNotApplicableResult(
        ConceptKind kind,
        int seed,
        string title,
        string sourceContext,
        string reason,
        string generatorVersion)
    {
        ConceptRunResult result = new ConceptRunResult();
        result.Status = ConceptRunStatus.NotApplicable;
        result.StatusReason = reason;
        result.Title = title;
        result.Subtitle = "Not applicable";
        result.Summary = reason;
        result.Provenance = new ConceptProvenance
        {
            ConceptId = kind.ToString(),
            Seed = seed,
            GeneratorVersion = generatorVersion,
            SourceContext = sourceContext,
        };
        return result;
    }

    private static ConceptRunResult BuildFailedResult(
        ConceptKind kind,
        int seed,
        string title,
        string sourceContext,
        string reason,
        string generatorVersion)
    {
        ConceptRunResult result = new ConceptRunResult();
        result.Status = ConceptRunStatus.Failed;
        result.StatusReason = reason;
        result.Title = title;
        result.Subtitle = "Generation failed";
        result.Summary = reason;
        result.Provenance = new ConceptProvenance
        {
            ConceptId = kind.ToString(),
            Seed = seed,
            GeneratorVersion = generatorVersion,
            SourceContext = sourceContext,
        };
        return result;
    }

    private static string BuildContextTitle(string bodyName, string sandboxTitle, string suffix)
    {
        if (string.IsNullOrEmpty(bodyName))
        {
            return sandboxTitle;
        }

        return bodyName + suffix;
    }
}
