using System.Collections.Generic;
using System.Linq;
using StarGen.Domain.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Static registry for concept-atlas module presenters.
/// </summary>
public static class ConceptAtlasModuleRegistry
{
    private static readonly Dictionary<ConceptKind, IConceptModulePresenter> _modules = CreateModules();

    /// <summary>
    /// Returns all module descriptors in atlas order.
    /// </summary>
    public static IReadOnlyList<ConceptModuleDescriptor> GetDescriptors()
    {
        return _modules.Values
            .Select(module => module.Descriptor)
            .OrderBy(descriptor => (int)descriptor.Kind)
            .ToList();
    }

    /// <summary>
    /// Runs the selected module.
    /// </summary>
    public static ConceptRunResult Run(ConceptRunRequest request)
    {
        if (_modules.TryGetValue(request.Kind, out IConceptModulePresenter? module))
        {
            return module.Run(request);
        }

        return new ConceptRunResult
        {
            Title = request.Kind.ToString(),
            Summary = "No concept presenter is registered for this module.",
            Provenance = new ConceptProvenance
            {
                ConceptId = request.Kind.ToString(),
                Seed = request.Context.Seed,
                GeneratorVersion = "atlas-missing-module",
                SourceContext = request.Context.SourceLabel,
            },
        };
    }

    private static Dictionary<ConceptKind, IConceptModulePresenter> CreateModules()
    {
        return new Dictionary<ConceptKind, IConceptModulePresenter>
        {
            [ConceptKind.Ecology] = new EcologyAtlasModulePresenter(),
            [ConceptKind.Religion] = new ReligionAtlasModulePresenter(),
            [ConceptKind.Civilization] = new CivilizationAtlasModulePresenter(),
            [ConceptKind.Language] = new LanguageAtlasModulePresenter(),
            [ConceptKind.Disease] = new DiseaseAtlasModulePresenter(),
            [ConceptKind.Evolution] = new EvolutionAtlasModulePresenter(),
        };
    }
}
