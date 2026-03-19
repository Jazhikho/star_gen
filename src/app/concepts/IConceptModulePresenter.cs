using StarGen.Domain.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Concept module presenter contract for the atlas.
/// </summary>
public interface IConceptModulePresenter
{
    /// <summary>
    /// Static descriptor for the module.
    /// </summary>
    ConceptModuleDescriptor Descriptor { get; }

    /// <summary>
    /// Runs the module against the supplied request.
    /// </summary>
    ConceptRunResult Run(ConceptRunRequest request);
}
