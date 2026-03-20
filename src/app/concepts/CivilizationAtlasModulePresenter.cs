using StarGen.Domain.Concepts;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the civilisation concept fold-in.
/// </summary>
public sealed class CivilizationAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the civilisation presenter.
    /// </summary>
    public CivilizationAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Civilization,
            DisplayName = "Civilisation",
            Summary = "Trace regime, economy, civic values, and historical trajectory as a population-facing worldbuilding layer.",
            AcceptedContext = "Population history, colony/native context, or manual social profile",
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
