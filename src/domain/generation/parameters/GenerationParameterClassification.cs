namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Classifies how a surfaced parameter participates in the generation pipeline.
/// </summary>
public enum GenerationParameterClassification
{
    /// <summary>
    /// Science-backed input that materially changes generated outcomes.
    /// </summary>
    GenerationPrior = 0,

    /// <summary>
    /// Intentional non-scientific seam that reshapes downstream generation.
    /// </summary>
    GeneratorOverride = 1,

    /// <summary>
    /// Control that affects whether or when a subsystem runs.
    /// </summary>
    RuntimeOrchestrationControl = 2,

    /// <summary>
    /// UI-only control that affects presentation rather than generation.
    /// </summary>
    PresentationReadoutControl = 3,
}
