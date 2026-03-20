using StarGen.Domain.Concepts;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the disease concept fold-in.
/// </summary>
public sealed class DiseaseAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the disease presenter.
    /// </summary>
    public DiseaseAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Disease,
            DisplayName = "Disease",
            Summary = "Model outbreak traits and population impacts from environment, density, and medical context.",
            AcceptedContext = "Planet environment, population density, or manual epidemiology profile",
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
