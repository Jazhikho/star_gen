using System;

namespace StarGen.Domain.Generation.Science;

/// <summary>
/// Describes a source-backed science engine family and its alternatives.
/// </summary>
public sealed class ScienceHardeningEngineDescriptor
{
    /// <summary>
    /// Stable engine identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Generator domain this engine belongs to.
    /// </summary>
    public string Domain { get; }

    /// <summary>
    /// Semicolon-delimited source IDs that ground the engine family.
    /// </summary>
    public string SourceIds { get; }

    /// <summary>
    /// Why this engine is not a full physical simulation.
    /// </summary>
    public string GameDesignTradeoff { get; }

    /// <summary>
    /// Alternative engine IDs available for the same domain.
    /// </summary>
    public string[] AlternativeEngineIds { get; }

    /// <summary>
    /// Whether this engine is a diagnostic/readout before it can drive generation.
    /// </summary>
    public bool DiagnosticOnlyByDefault { get; }

    /// <summary>
    /// Creates an immutable engine descriptor.
    /// </summary>
    public ScienceHardeningEngineDescriptor(
        string id,
        string domain,
        string sourceIds,
        string gameDesignTradeoff,
        string[] alternativeEngineIds,
        bool diagnosticOnlyByDefault)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("ScienceHardeningEngineDescriptor requires a non-empty id.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentException("ScienceHardeningEngineDescriptor requires a non-empty domain.", nameof(domain));
        }

        Id = id;
        Domain = domain;
        SourceIds = sourceIds;
        GameDesignTradeoff = gameDesignTradeoff;
        AlternativeEngineIds = alternativeEngineIds;
        DiagnosticOnlyByDefault = diagnosticOnlyByDefault;
    }
}
