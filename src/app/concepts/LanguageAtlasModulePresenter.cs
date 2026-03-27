using StarGen.Domain.Concepts;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the language concept fold-in.
/// </summary>
public sealed class LanguageAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the language presenter.
    /// </summary>
    public LanguageAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Language,
            DisplayName = "Language",
            Summary = "Generate phonology, grammar, lexicon, and sample utterances for naming and cultural presentation.",
            AcceptedContext = "Civilisation context or manual language profile",
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
