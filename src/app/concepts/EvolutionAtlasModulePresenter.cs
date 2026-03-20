using StarGen.Domain.Concepts;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the evolution concept fold-in.
/// </summary>
public sealed class EvolutionAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the evolution presenter.
    /// </summary>
    public EvolutionAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Evolution,
            DisplayName = "Evolution",
            Summary = "Generate trait-line progression and species-facing outcomes from environment and ecological pressure.",
            AcceptedContext = "Ecology/environment context or manual adaptive-pressure profile",
        };
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return ConceptResultFactory.Run(request);
    }
}
