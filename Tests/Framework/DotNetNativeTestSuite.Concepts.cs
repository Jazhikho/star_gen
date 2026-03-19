#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.Concepts;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;
using StarGen.Services.Concepts;

namespace StarGen.Tests.Framework;

/// <summary>
/// Concept-atlas specific tests.
/// </summary>
public static partial class DotNetNativeTestSuite
{
    private static void RunConceptTests(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_ecology_atlas_presenter_is_deterministic",
            TestEcologyAtlasPresenterIsDeterministic);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_religion_atlas_presenter_is_deterministic",
            TestReligionAtlasPresenterIsDeterministic);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_civilization_atlas_presenter_is_deterministic",
            TestCivilizationAtlasPresenterIsDeterministic);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_language_atlas_presenter_is_deterministic",
            TestLanguageAtlasPresenterIsDeterministic);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_disease_atlas_presenter_is_deterministic",
            TestDiseaseAtlasPresenterIsDeterministic);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_evolution_atlas_presenter_is_deterministic",
            TestEvolutionAtlasPresenterIsDeterministic);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_result_store_round_trips",
            TestConceptResultStoreRoundTrips);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_world_state_generator_populates_persisted_results",
            TestConceptWorldStateGeneratorPopulatesPersistedResults);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_atlas_prefers_persisted_results_when_context_matches",
            TestConceptAtlasPrefersPersistedResultsWhenContextMatches);
    }

    private static void TestEcologyAtlasPresenterIsDeterministic()
    {
        EcologyAtlasModulePresenter presenter = new();
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(81234);
        context.BodyName = "Kharon";
        context.DominantBiome = "Forest";
        context.Population = 18000000;
        context.HabitabilityScore = 7;

        ConceptRunResult first = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Ecology,
            Context = context,
        });
        ConceptRunResult second = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Ecology,
            Context = context.Clone(),
        });

        AssertEqual(first.Summary, second.Summary, "Ecology summary should be deterministic");
        AssertEqual(first.Metrics.Count, second.Metrics.Count, "Ecology metrics count should be stable");
        AssertEqual(first.Sections.Count, second.Sections.Count, "Ecology sections count should be stable");
        AssertEqual(first.Provenance.Seed, second.Provenance.Seed, "Ecology provenance seed should match");
        AssertEqual(first.Sections[1].Items[0], second.Sections[1].Items[0], "Trophic profile should be deterministic");
    }

    private static void TestReligionAtlasPresenterIsDeterministic()
    {
        ReligionAtlasModulePresenter presenter = new();
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(92011);
        context.BodyName = "Selene";
        context.Population = 42000000;
        context.DominantBiome = "Desert";
        context.Regime = StarGen.Domain.Population.GovernmentType.Regime.Theocracy;
        context.TechnologyLevel = StarGen.Domain.Population.TechnologyLevel.Level.Atomic;

        ConceptRunResult first = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Religion,
            Context = context,
        });
        ConceptRunResult second = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Religion,
            Context = context.Clone(),
        });

        AssertEqual(first.Summary, second.Summary, "Religion summary should be deterministic");
        AssertEqual(first.Sections[0].Items[0], second.Sections[0].Items[0], "Religion structure should be deterministic");
        AssertEqual(first.Provenance.Seed, second.Provenance.Seed, "Religion provenance seed should match");
        AssertFalse(first.Subtitle.Contains("_"), "Religion subtitle should use user-facing sentence case instead of snake_case");
    }

    private static void TestCivilizationAtlasPresenterIsDeterministic()
    {
        CivilizationAtlasModulePresenter presenter = new();
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(78110);
        context.BodyName = "Arden";
        context.Population = 17000000;
        context.DominantBiome = "Forest";
        context.Regime = StarGen.Domain.Population.GovernmentType.Regime.Constitutional;
        context.TechnologyLevel = StarGen.Domain.Population.TechnologyLevel.Level.Information;

        ConceptRunResult first = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Civilization,
            Context = context,
        });
        ConceptRunResult second = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Civilization,
            Context = context.Clone(),
        });

        AssertEqual(first.Title, second.Title, "Civilization title should be deterministic");
        AssertEqual(first.Summary, second.Summary, "Civilization summary should be deterministic");
        AssertEqual(first.Sections[2].Items[0], second.Sections[2].Items[0], "Civilization milestones should be deterministic");
        AssertEqual(first.Provenance.Seed, second.Provenance.Seed, "Civilization provenance seed should match");
    }

    private static void TestLanguageAtlasPresenterIsDeterministic()
    {
        LanguageAtlasModulePresenter presenter = new();
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(11407);
        context.BodyName = "Nysa";
        context.Population = 9000000;
        context.DominantBiome = "Oceanic";

        ConceptRunResult first = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Language,
            Context = context,
        });
        ConceptRunResult second = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Language,
            Context = context.Clone(),
        });

        AssertEqual(first.Title, second.Title, "Language title should be deterministic");
        AssertEqual(first.Summary, second.Summary, "Language summary should be deterministic");
        AssertEqual(first.Sections[2].Items[0], second.Sections[2].Items[0], "Language lexicon sample should be deterministic");
        AssertEqual(first.Provenance.Seed, second.Provenance.Seed, "Language provenance seed should match");
    }

    private static void TestDiseaseAtlasPresenterIsDeterministic()
    {
        DiseaseAtlasModulePresenter presenter = new();
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(30119);
        context.BodyName = "Talos";
        context.Population = 27000000;
        context.WaterAvailability = 0.72;
        context.RadiationLevel = 0.31;
        context.HabitabilityScore = 4;

        ConceptRunResult first = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Disease,
            Context = context,
        });
        ConceptRunResult second = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Disease,
            Context = context.Clone(),
        });

        AssertEqual(first.Summary, second.Summary, "Disease summary should be deterministic");
        AssertEqual(first.Sections[2].Items[0], second.Sections[2].Items[0], "Disease epidemic model should be deterministic");
        AssertEqual(first.Provenance.Seed, second.Provenance.Seed, "Disease provenance seed should match");
    }

    private static void TestEvolutionAtlasPresenterIsDeterministic()
    {
        EvolutionAtlasModulePresenter presenter = new();
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(66081);
        context.BodyName = "Pelagos";
        context.DominantBiome = "Oceanic";
        context.WaterAvailability = 0.88;
        context.RadiationLevel = 0.12;
        context.HabitabilityScore = 8;

        ConceptRunResult first = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Evolution,
            Context = context,
        });
        ConceptRunResult second = presenter.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Evolution,
            Context = context.Clone(),
        });

        AssertEqual(first.Title, second.Title, "Evolution title should be deterministic");
        AssertEqual(first.Summary, second.Summary, "Evolution summary should be deterministic");
        AssertEqual(first.Sections[1].Items[0], second.Sections[1].Items[0], "Evolution traits should be deterministic");
        AssertEqual(first.Provenance.Seed, second.Provenance.Seed, "Evolution provenance seed should match");
    }

    private static void TestConceptResultStoreRoundTrips()
    {
        ConceptRunResult result = new ConceptRunResult
        {
            Title = "Archive Ecology",
            Subtitle = "Forest archive",
            Summary = "Persisted concept result",
            Provenance = new ConceptProvenance
            {
                ConceptId = ConceptKind.Ecology.ToString(),
                Seed = 42,
                GeneratorVersion = "test-version",
                SourceContext = "Test source",
            },
        };
        result.Metrics.Add(new ConceptMetric
        {
            Label = "Stability",
            Value = 82.0,
            MaxValue = 100.0,
            DisplayText = "82%",
        });
        result.Sections.Add(new ConceptSection
        {
            Title = "Highlights",
            Items = new global::System.Collections.Generic.List<string> { "Stored item" },
        });

        ConceptResultStore store = new ConceptResultStore();
        store.Set(ConceptKind.Ecology, result);

        ConceptResultStore restored = ConceptResultStore.FromDictionary(store.ToDictionary());
        ConceptRunResult? restoredResult = restored.Get(ConceptKind.Ecology);
        AssertNotNull(restoredResult, "Restored concept result should exist");
        AssertEqual(result.Title, restoredResult!.Title, "Concept title should round-trip");
        AssertEqual(result.Metrics[0].DisplayText, restoredResult.Metrics[0].DisplayText, "Concept metrics should round-trip");
        AssertEqual(result.Sections[0].Items[0], restoredResult.Sections[0].Items[0], "Concept sections should round-trip");
        AssertEqual(result.Provenance.GeneratorVersion, restoredResult.Provenance.GeneratorVersion, "Concept provenance should round-trip");
    }

    private static void TestConceptWorldStateGeneratorPopulatesPersistedResults()
    {
        SolarSystem system = new SolarSystem("test_system", "Test System");
        system.Provenance = Provenance.CreateCurrent(24680);

        CelestialBody body = new CelestialBody("body_1", "Mire", CelestialType.Type.Planet, new PhysicalProps(), Provenance.CreateCurrent(24680));
        PlanetProfile profile = new PlanetProfile
        {
            BodyId = body.Id,
            HabitabilityScore = 7,
            AvgTemperatureK = 289.0,
            GravityG = 1.0,
            RadiationLevel = 0.12,
            OceanCoverage = 0.62,
            HasBreathableAtmosphere = true,
        };
        profile.Biomes[(int)BiomeType.Type.Forest] = 1.0;

        NativePopulation nativePopulation = new NativePopulation
        {
            Id = "native_1",
            Name = "Mirefolk",
            BodyId = body.Id,
            Population = 8500000,
            PeakPopulation = 8500000,
            TechLevel = TechnologyLevel.Level.Information,
            PrimaryBiome = "Forest",
        };
        nativePopulation.Government.Regime = GovernmentType.Regime.Constitutional;

        PlanetPopulationData populationData = new PlanetPopulationData
        {
            BodyId = body.Id,
            Profile = profile,
            GenerationSeed = 24680,
        };
        populationData.NativePopulations.Add(nativePopulation);

        body.PopulationData = populationData;
        system.AddBody(body);

        ConceptWorldStateGenerator.EnsureSystemConcepts(system);

        AssertTrue(body.ConceptResults.Has(ConceptKind.Ecology), "Body should store ecology results");
        AssertTrue(body.ConceptResults.Has(ConceptKind.Evolution), "Body should store evolution results");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Civilization), "Population should store civilization results");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Religion), "Population should store religion results");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Language), "Population should store language results");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Disease), "Population should store disease results");
        AssertTrue(populationData.ConceptResults.Has(ConceptKind.Civilization), "Aggregate population store should mirror dominant concept results");
        AssertTrue(system.ConceptResults.Has(ConceptKind.Civilization), "System should carry aggregate concept results");
        AssertTrue(nativePopulation.History.GetAllEvents().Count > 0, "Population history should receive concept-derived events");
    }

    private static void TestConceptAtlasPrefersPersistedResultsWhenContextMatches()
    {
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(55123);
        context.BodyName = "Archive";
        context.SourceLabel = "Archive";
        context.PersistedResults.Set(
            ConceptKind.Ecology,
            new ConceptRunResult
            {
                Title = "Stored Ecology",
                Subtitle = "Stored Subtitle",
                Summary = "Persisted summary text",
                Provenance = new ConceptProvenance
                {
                    ConceptId = ConceptKind.Ecology.ToString(),
                    Seed = 55123,
                    GeneratorVersion = "stored-version",
                    SourceContext = "Archive",
                },
            });

        ConceptAtlasScreen screen = new ConceptAtlasScreen();
        screen._Ready();
        screen.SetContext(context, ConceptKind.Ecology);

        RichTextLabel? summary = screen.FindChild("SummaryText", recursive: true, owned: false) as RichTextLabel;
        AssertNotNull(summary, "Concept atlas summary label should exist");
        AssertEqual("Persisted summary text", summary!.Text, "Concept atlas should use persisted results before rerunning");

        screen.QueueFree();
    }
}
