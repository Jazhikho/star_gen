using StarGen.Domain.Concepts;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the religion concept fold-in.
/// </summary>
public sealed class ReligionAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the religion presenter.
    /// </summary>
    public ReligionAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Religion,
            DisplayName = "Religion",
            Summary = "Generate belief systems, authority structures, ritual emphasis, and religious landscapes seeded from culture-adjacent context.",
            AcceptedContext = "Population, government, settlement, or manual cultural profile",
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
