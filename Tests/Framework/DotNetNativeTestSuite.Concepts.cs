#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;
using StarGen.App.Concepts;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Pipeline;
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
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_pipeline_marks_lifeless_world_not_applicable",
            TestConceptPipelineMarksLifelessWorldNotApplicable);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_pipeline_maps_subsurface_biology_without_surface_biome_failure",
            TestConceptPipelineMapsSubsurfaceBiologyWithoutSurfaceBiomeFailure);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_pipeline_keeps_non_sentient_worlds_pre_society",
            TestConceptPipelineKeepsNonSentientWorldsPreSociety);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_pipeline_generates_society_for_sentient_worlds",
            TestConceptPipelineGeneratesSocietyForSentientWorlds);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_manual_runs_respect_applicability",
            TestConceptManualRunsRespectApplicability);
        runner.RunNativeTest(
            "DotNetNativeTestSuite::test_concept_paths_have_no_ternary_operators",
            TestConceptPathsHaveNoTernaryOperators);
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

    private static void TestConceptPipelineMarksLifelessWorldNotApplicable()
    {
        PlanetProfile profile = CreateBarrenProfile();
        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(profile, 40102, generateNatives: true, generateColonies: false);

        AssertNotNull(data.EnvironmentProfile, "Lifeless world should still produce an environment profile");
        AssertNotNull(data.EcologyState, "Lifeless world should still persist ecology applicability");
        AssertEqual(ConceptRunStatus.NotApplicable, data.EcologyState!.Status, "Lifeless world should not generate ecology");
        AssertNotNull(data.SpeciesEvolution, "Lifeless world should still persist species applicability");
        AssertEqual(ConceptRunStatus.NotApplicable, data.SpeciesEvolution!.Status, "Lifeless world should not generate species evolution");
        AssertNotNull(data.SentienceAssessment, "Lifeless world should still persist sentience applicability");
        AssertEqual(ConceptRunStatus.NotApplicable, data.SentienceAssessment!.Status, "Lifeless world should not generate sentience");
        AssertEqual(0, data.NativePopulations.Count, "Lifeless world should not create native populations");

        SolarSystem system = CreateSystemWithBody("body_lifeless", "Cinder", profile, data, 40102);
        ConceptWorldStateGenerator.EnsureSystemConcepts(system);

        CelestialBody body = system.GetBody("body_lifeless");
        AssertTrue(body.ConceptResults.Has(ConceptKind.Ecology), "Body should persist ecology applicability result");
        AssertTrue(body.ConceptResults.Has(ConceptKind.Evolution), "Body should persist evolution applicability result");
        AssertFalse(body.ConceptResults.Has(ConceptKind.Civilization), "Body should not fabricate civilisation for lifeless worlds");
        AssertFalse(system.ConceptResults.Has(ConceptKind.Civilization), "System should not fabricate civilisation without population");
    }

    private static void TestConceptPipelineMapsSubsurfaceBiologyWithoutSurfaceBiomeFailure()
    {
        PlanetProfile profile = new PlanetProfile();
        profile.BodyId = "profile_subsurface";
        profile.HabitabilityScore = 3;
        profile.AvgTemperatureK = 245.0;
        profile.PressureAtm = 0.0;
        profile.OceanCoverage = 0.0;
        profile.LandCoverage = 1.0;
        profile.IceCoverage = 0.8;
        profile.GravityG = 0.15;
        profile.TectonicActivity = 0.22;
        profile.VolcanismLevel = 0.08;
        profile.WeatherSeverity = 0.0;
        profile.MagneticFieldStrength = 0.18;
        profile.RadiationLevel = 0.28;
        profile.HasAtmosphere = false;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = false;
        profile.IsMoon = true;
        profile.Biomes[(int)BiomeType.Type.Barren] = 1.0;

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(profile, 44012, generateNatives: true, generateColonies: false);

        AssertNotNull(data.EcologyState, "Subsurface world should evaluate ecology");
        AssertEqual(ConceptRunStatus.Generated, data.EcologyState!.Status, "Subsurface ocean worlds should map to a non-surface ecology instead of failing");
        AssertNotNull(data.EcologyState.Snapshot, "Generated subsurface ecology should include a snapshot");
    }

    private static void TestConceptPipelineKeepsNonSentientWorldsPreSociety()
    {
        PlanetProfile profile = CreateHabitableProfile();
        int seed = FindSeedForSentience(profile, shouldBeSentient: false);
        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(profile, seed, generateNatives: true, generateColonies: false);

        AssertEqual(ConceptRunStatus.Generated, data.EcologyState!.Status, "Life-bearing world should generate ecology");
        AssertEqual(ConceptRunStatus.Generated, data.SpeciesEvolution!.Status, "Life-bearing world should generate species evolution");
        AssertEqual(ConceptRunStatus.Generated, data.SentienceAssessment!.Status, "Life-bearing world should record sentience evaluation");
        AssertFalse(data.SentienceAssessment.HasSentientLife, "Selected seed should stay below sentience threshold");
        AssertEqual(0, data.NativePopulations.Count, "Non-sentient worlds should not generate society-bearing native populations");

        SolarSystem system = CreateSystemWithBody("body_nonsentient", "Viridia", profile, data, seed);
        ConceptWorldStateGenerator.EnsureSystemConcepts(system);

        CelestialBody body = system.GetBody("body_nonsentient");
        AssertTrue(body.ConceptResults.Has(ConceptKind.Ecology), "Body should retain ecology results");
        AssertTrue(body.ConceptResults.Has(ConceptKind.Evolution), "Body should retain evolution results");
        AssertFalse(body.ConceptResults.Has(ConceptKind.Civilization), "Body should not carry aggregate civilisation without sentient population");
        AssertFalse(system.ConceptResults.Has(ConceptKind.Civilization), "System should not carry aggregate civilisation without population");
    }

    private static void TestConceptPipelineGeneratesSocietyForSentientWorlds()
    {
        PlanetProfile profile = CreateHabitableProfile();
        int seed = FindSeedForSentience(profile, shouldBeSentient: true);
        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(profile, seed, generateNatives: true, generateColonies: false);

        AssertEqual(ConceptRunStatus.Generated, data.EcologyState!.Status, "Sentient world should generate ecology");
        AssertEqual(ConceptRunStatus.Generated, data.SpeciesEvolution!.Status, "Sentient world should generate species evolution");
        AssertEqual(ConceptRunStatus.Generated, data.SentienceAssessment!.Status, "Sentient world should record sentience evaluation");
        AssertTrue(data.SentienceAssessment.HasSentientLife, "Selected seed should produce a sentient lineage");
        AssertTrue(data.NativePopulations.Count > 0, "Sentient world should generate native populations");

        SolarSystem system = CreateSystemWithBody("body_sentient", "Haven", profile, data, seed);
        ConceptWorldStateGenerator.EnsureSystemConcepts(system);

        NativePopulation nativePopulation = data.NativePopulations[0];
        AssertNotNull(nativePopulation.SocietyState, "Sentient native population should persist society state");
        AssertEqual(ConceptRunStatus.Generated, nativePopulation.SocietyState!.Status, "Society state should generate for sentient population");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Civilization), "Native population should store civilisation results");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Religion), "Native population should store religion results");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Language), "Native population should store language results");
        AssertTrue(nativePopulation.ConceptResults.Has(ConceptKind.Disease), "Native population should store disease results");
        AssertTrue(system.ConceptResults.Has(ConceptKind.Civilization), "Inhabited system should carry aggregate civilisation results");
    }

    private static void TestConceptManualRunsRespectApplicability()
    {
        ConceptContextSnapshot context = ConceptContextBuilder.CreateDefault(99881);
        context.BodyName = "Dry Rock";
        context.SourceLabel = "Dry Rock";
        context.DominantBiome = "Barren";
        context.WaterAvailability = 0.0;
        context.HabitabilityScore = 0;
        context.Population = 0;

        ConceptRunResult ecology = ConceptResultFactory.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Ecology,
            Context = context,
        });
        ConceptRunResult evolution = ConceptResultFactory.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Evolution,
            Context = context,
        });
        ConceptRunResult civilization = ConceptResultFactory.Run(new ConceptRunRequest
        {
            Kind = ConceptKind.Civilization,
            Context = context,
        });

        AssertEqual(ConceptRunStatus.NotApplicable, ecology.Status, "Manual ecology should not fabricate a biosphere for barren worlds");
        AssertEqual(ConceptRunStatus.NotApplicable, evolution.Status, "Manual evolution should not fabricate a lineage without ecology");
        AssertEqual(ConceptRunStatus.NotApplicable, civilization.Status, "Manual civilisation should require population context");
    }

    private static void TestConceptPathsHaveNoTernaryOperators()
    {
        List<string> directories = new List<string>
        {
            ProjectSettings.GlobalizePath("res://src/domain/concepts"),
            ProjectSettings.GlobalizePath("res://src/services/concepts"),
            ProjectSettings.GlobalizePath("res://src/app/concepts"),
        };
        Regex ternaryPattern = new Regex(@"\?.*:", RegexOptions.Compiled);

        foreach (string directory in directories)
        {
            string[] files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                for (int index = 0; index < lines.Length; index += 1)
                {
                    if (ternaryPattern.IsMatch(lines[index]))
                    {
                        throw new Exception("Concept-path ternary operator found in " + file + " at line " + (index + 1) + ".");
                    }
                }
            }
        }
    }

    private static PlanetProfile CreateHabitableProfile()
    {
        PlanetProfile profile = new PlanetProfile();
        profile.BodyId = "profile_world";
        profile.HabitabilityScore = 7;
        profile.AvgTemperatureK = 289.0;
        profile.PressureAtm = 1.02;
        profile.OceanCoverage = 0.58;
        profile.LandCoverage = 0.34;
        profile.IceCoverage = 0.08;
        profile.ContinentCount = 4;
        profile.GravityG = 0.98;
        profile.TectonicActivity = 0.44;
        profile.VolcanismLevel = 0.21;
        profile.WeatherSeverity = 0.38;
        profile.MagneticFieldStrength = 0.77;
        profile.RadiationLevel = 0.16;
        profile.HasAtmosphere = true;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;
        profile.Biomes[(int)BiomeType.Type.Forest] = 0.36;
        profile.Biomes[(int)BiomeType.Type.Grassland] = 0.30;
        profile.Biomes[(int)BiomeType.Type.Ocean] = 0.24;
        profile.Biomes[(int)BiomeType.Type.Wetland] = 0.10;
        return profile;
    }

    private static PlanetProfile CreateBarrenProfile()
    {
        PlanetProfile profile = new PlanetProfile();
        profile.BodyId = "profile_barren";
        profile.HabitabilityScore = 1;
        profile.AvgTemperatureK = 150.0;
        profile.PressureAtm = 0.0;
        profile.OceanCoverage = 0.0;
        profile.LandCoverage = 1.0;
        profile.IceCoverage = 0.0;
        profile.GravityG = 0.42;
        profile.TectonicActivity = 0.05;
        profile.VolcanismLevel = 0.01;
        profile.WeatherSeverity = 0.0;
        profile.MagneticFieldStrength = 0.0;
        profile.RadiationLevel = 0.82;
        profile.HasAtmosphere = false;
        profile.HasLiquidWater = false;
        profile.HasBreathableAtmosphere = false;
        profile.Biomes[(int)BiomeType.Type.Barren] = 1.0;
        return profile;
    }

    private static int FindSeedForSentience(PlanetProfile profile, bool shouldBeSentient)
    {
        for (int seed = 1000; seed < 20000; seed += 1)
        {
            PlanetEnvironmentProfile environment = PlanetEnvironmentProfile.FromPlanetProfile(profile, seed, "Search", "Planet");
            ConceptDependencyChainGenerator.PopulatePreSocietyStates(
                environment,
                out EcologyState ecologyState,
                out SpeciesEvolutionState speciesEvolutionState,
                out SentienceAssessment sentienceAssessment);

            if (ecologyState.Status != ConceptRunStatus.Generated)
            {
                continue;
            }

            if (speciesEvolutionState.Status != ConceptRunStatus.Generated)
            {
                continue;
            }

            if (sentienceAssessment.Status != ConceptRunStatus.Generated)
            {
                continue;
            }

            if (sentienceAssessment.HasSentientLife == shouldBeSentient)
            {
                return seed;
            }
        }

        throw new Exception("Could not find deterministic seed for sentience target " + shouldBeSentient + ".");
    }

    private static SolarSystem CreateSystemWithBody(string bodyId, string bodyName, PlanetProfile profile, PlanetPopulationData data, int seed)
    {
        SolarSystem system = new SolarSystem("concept_test_system", "Concept Test System");
        system.Provenance = Provenance.CreateCurrent(seed);

        CelestialBody body = new CelestialBody(bodyId, bodyName, CelestialType.Type.Planet, new PhysicalProps(), Provenance.CreateCurrent(seed));
        body.PopulationData = data;
        body.EnvironmentProfile = data.EnvironmentProfile;
        body.Ecology = data.EcologyState;
        body.SpeciesEvolution = data.SpeciesEvolution;
        body.Sentience = data.SentienceAssessment;
        body.Disease = data.DiseaseState;
        system.AddBody(body);
        return system;
    }
}
