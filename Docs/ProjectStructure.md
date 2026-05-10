# StarGen Project Structure

Current structure snapshot for major project folders and key entry files.

Notes:
- Excludes generated/cache metadata such as `.uid`, `.godot/`, `.mono/`, and `.git/`.
- `release/` and `releases/` are local build output folders and are git-ignored.
- `Docs/galactic_formation.md` is optional local reference material and is not tracked (see root `.gitignore`).

```
star_gen/
|-- Concepts/
|   |-- Additions.md
|-- Docs/
|   |-- Assets.md
|   |-- CelestialBodyProperties.md
|   |-- EndToEndScienceAudit.md
|   |-- GDD.md
|   |-- LifeScienceAudit.md
|   |-- ParameterMaterialityAudit.md
|   |-- ProjectStructure.md
|   |-- RpgCompatibilityGenerationAudit.md
|   |-- RegimeChangeModel.md
|   |-- Roadmap.md
|   |-- ScientificParameterAudit.md
|   |-- SentientWorldBaseline.md
|   |-- SourceGeneratorFitAudit.md
|   |-- V0.10AcceptanceChecklist.md
|   |-- V0.10ReleaseChecklist.md
|   |-- V0.9AcceptanceChecklist.md
|   |-- V0.9Plan.md
|   |-- V0.9ReleaseChecklist.md
|   |-- V1.0Checklist.md
|   |-- ViewerSceneOwnershipAudit.md
|   `-- TravellerWorldCreation.md
|-- NuGet/
|   `-- Migrations/
|-- Sources/
|   |-- AnnotatedBibliography.md
|   |-- SourceUtilizationPlan.md
|   `-- Texts/
|-- Resources/
|   `-- Audio/
|-- scripts/
|   `-- CreateReleaseBuild.ps1
|-- src/
|   |-- app/
|   |   |-- audio/                     # shared app-level audio controller and cue library used by splash/menu/UI playback
|   |   |-- components/                # reusable scene-authored UI fragments/templates for shipped viewers and editors
|   |   |-- galaxy_viewer/
|   |   |-- rendering/
|   |   |-- shared/
|   |   |-- system_viewer/
|   |   |-- themes/
|   |   `-- viewer/
|   |-- domain/
|   |   |-- bootstrap/
|   |   |-- celestial/
|   |   |-- colonization/
|   |   |-- concepts/                  # parked concept domain modules not shipped in the 0.9 mainline path
|   |   |-- constants/
|   |   |-- editing/
|   |   |-- galaxy/
|   |   |-- generation/
|   |   |-- jumplanes/
|   |   |-- math/
|   |   |-- population/
|   |   |-- rng/
|   |   |-- system/
|   |   |-- utils/
|   |   `-- validation/
|   `-- services/
|       |-- concepts/                  # parked/migration-facing helpers
|       |-- export/
|       `-- persistence/               # retained for the export-enabled edition and gated off in the public demo UI
|-- Tests/
|   |-- Baselines/
|   |   `-- Artifacts/
|   |-- domain/
|   |-- Fixtures/
|   |-- Framework/
|   |-- Integration/
|   |-- Quality/
|   `-- Unit/
|       |-- JumpLanes/
|       `-- Population/
|-- .gitattributes
|-- .gitignore
|-- AI-Provenance-Log.md
|-- AI-Use-Statement.md
|-- claude.md
|-- export_presets.cfg
|-- LICENSE
|-- project.godot
|-- README.md
|-- StarGen.csproj
|-- StarGen.sln
`-- VERSION.md
```

## Recent Major Additions

- `Sources/SourceUtilizationPlan.md`: source-to-generator implementation plan for using reviewed sources to inform active generation behavior. It records the current completed slices, the per-source workflow, deferred follow-ups, and the next priority queue, currently continuing sentient populations, technology, and governance after the first technology-access diagnostics slice.
- `src/app/audio/AppAudioController.cs`: shared app-level audio controller that owns reusable music and UI players so startup/menu/UI playback uses explicit exported resources instead of ad hoc runtime file scans.
- `src/app/audio/AppAudioLibrary.cs`: global-class resource declaring the application's shared music and UI cue streams and their default playback volumes.
- `src/app/audio/AppAudioCueId.cs`: stable cue identifiers used by the controller and callers such as the splash screen.
- `Resources/Audio/MainAudioLibrary.tres`: default shared audio library resource, currently binding the intro music cue so desktop exports include the startup audio as a real dependency.
- `Tests/Integration/TestAppAudio.cs`: non-visual regression coverage proving `MainApp` owns the shared audio controller, that intro music is configured through the shared library rather than a splash-local player, and that the persisted skip-intro preference opens directly to the main menu.
- `Docs/RpgCompatibilityGenerationAudit.md`: audit of the target RPG systems' world-generation outputs, the current StarGen compatibility coverage gaps, and the proposed science-grounded sentient-world baseline for government, law, technology, settlement structure, and ruleset adapters.
- `Docs/SentientWorldBaseline.md`: field-by-field grounding note for the neutral sentient-world baseline now carried on inhabited planets and moons, including why each inspector-facing population field is shown and which academic sources support its structural use.
- `Docs/ScientificParameterAudit.md`: tracked audit matrix for the shipped galaxy, stellar, planetary, and life science controls, including source relevance, generator wiring, expected outcome direction, test coverage, and disposition.
- `Docs/EndToEndScienceAudit.md`: tracked deep-audit inventory of the generator-internal numeric constants, their scientific grounding status, and the first-pass source-acquisition or inline-citation closure state for unsupported claims.
- `Docs/SourceGeneratorFitAudit.md`: initial v1.0 source-to-generator fit audit, starting with the cleaned raw full-text source notes and recording whether current generator behavior is a good, partial, weak, or planning-only fit to those sources.
- `src/domain/generation/parameters/ScienceTuningRegistry.cs`: machine-readable registry for science-facing generator coefficients and model controls, including value/range metadata, source IDs, public control IDs, consuming generators, implementation status, and human-audit requirements.
- `src/domain/generation/parameters/SourceAcquisitionRegistry.cs`: source-acquisition registry for science-control citations, distinguishing local source notes from pending reacquisition, intentionally blocked sources, and background-only catalog references.
- `src/domain/generation/parameters/SentientScienceReferenceCatalog.cs`: sentient-world source and tooltip registry for social-scale, technology-diffusion, economic-complexity, and legitimacy proxy controls.
- `Tests/Quality/TestScienceTuningRegistry.cs`: quality gate proving new science-tuning entries have source IDs, material public controls, tooltip citation surfaces, and explicit acquisition metadata for the new v1.0 source cluster.
- `Sources/Texts/Bains2004.txt`, `Behroozi2019.txt`, `BlandHawthornGerhard2016.txt`, `Chabrier2003.txt`, `Choi2016.txt`, `Conselice2014.txt`, `DucheneKraus2013.txt`, `Hayden2014.txt`, `Kennicutt1998.txt`, `Kroupa2001.txt`, `Raghavan2010.txt`, and `WeggGerhard2013.txt`: second-pass source-note batch queued together for one-pass human review of the remaining end-to-end science-audit source gaps; `BlandHawthornGerhard2016.txt` is now partly implemented as the active Milky-Way structural schema anchor, with dynamics and mass-decomposition follow-ups still open.
- `Sources/Texts/Laskar2017.txt`, `Petit2018.txt`, `Obertas2017.txt`, `Petit2020.txt`, `Tamayo2020.txt`, `Rice2023.txt`, `Outland2020.txt`, `Ronnet2020.txt`, `Sasaki2010.txt`, `Szulagyi2018.txt`, `Chowdhury2022.txt`, `Comin2013.txt`, `CominMestieri2013.txt`, and `Stokey2020.txt`: source-grounding cleanup notes for orbital stability, moon formation, and technology/governance PDFs; these are draft notes unless their individual status says otherwise and remain subject to human verification.
- `Sources/Texts/KunimotoEtAl2022.txt`, `ChatterjeeEtAl2026.txt`, `NakajimaEtAl2022.txt`, `KavelaarsEtAl2023.txt`, `KhoperskovEtAl2024.txt`, `HuntVasiliev2025.txt`, `HeEtAl2020.txt`, `ObertasTamayo2023.txt`, `ChabrierLenoble2023.txt`, `KarakatsanisMamassis2023.txt`, `BainsEtAl2024.txt` (plus local PDFs): 2026-04-26 Workstreams C through K draft intake per `SourceRecencyGapClosurePlan.md`; human verification required before generator or benchmark retuning.
- `src/domain/generation/PlanetaryGenerationProfile.cs`: shared serializable aggregate planetary-formation profile used by galaxy and system generation to carry mass-radius, envelope-loss, habitable-zone model, gas-giant, metallicity-coupling, rogue-planet, moon-bias, and outer-system-bias assumptions.
- `src/domain/generation/tables/PlanetMassRadiusTable.cs`: empirical planet mass-radius resolver that now applies the supported Chen-Kipping and Otegi model families directly to generated planet sizes and densities, with direct planet-facing overrides layered on top.
- `src/domain/generation/PlanetarySystemState.cs`: derived per-system planetary state built once from stellar context plus the shared planetary profile so downstream planet generation can react to snow-line, solid/gas budget, escape-pressure, migration/stirring surrogates, habitable-zone alignment, XUV activity, volatile delivery, outer-reservoir strength, and source-marked small-body reservoir provenance.
- `src/domain/generation/parameters/PlanetaryScienceReferenceCatalog.cs`: source registry and plain-language help/tooltips for the aggregate planetary controls shared by Galaxy Studio and System Studio.
- `src/domain/generation/RpgCompatibilityProfile.cs`: shared clean-room compatibility-profile resolver for `Space Opera`, `Cepheus`, `Starfinder`, and `Starforged`, including the generation-pressure defaults that now drive mainworld targeting, system fill bias, population forcing, and UWP-like behavior where appropriate.
- `src/domain/generation/StarfinderWorldbuildingProfile.cs`: Starfinder-only adapter payload that derives technology tier, accord signal, religion signal, and magic prevalence from the neutral sentient-world profile without adding magic to normal generation.
- `src/app/galaxy_viewer/GalaxyLocalSpaceCache.cs`: temporary aggregate local-space cache profile for Galaxy Viewer, storing merged nearby-system regions across multiple explicit builds, their covered subsector extents, and the coverage checks used by tool-driven follow-up passes.
- `src/app/galaxy_viewer/GalaxyViewer.LocalSpace.cs`: Galaxy Viewer partial controller for the top-bar `Options` dialog, the `Build Local Space` dialog, exact nearby-star preview counts, and explicit local-space cache construction or append behavior used by jump-route tools.
- `src/app/system_viewer/SystemViewer.Options.cs`: shared System Viewer options-dialog and compact bottom-right camera-panel behavior, including persisted fullscreen, seed-visibility, and intro-skip preferences.
- `src/app/system_viewer/SystemViewer.tscn`: active System Viewer scene, now reduced to viewer-only chrome (top bar, inspector, options dialog, and compact controls panel) after the embedded generation and save/load stack was removed.
- `src/app/system_viewer/SystemViewer.Parameters.cs`: thin spec-handoff helper for the viewer, now limited to reflecting the studio-provided `SolarSystemSpec` and validation state instead of constructing a second parameter editor inside the viewer.
- `src/app/system_viewer/SystemInspectorPanel.cs`: System Viewer overview and selection readout, including source-facing `SmallBodyReservoir` family summaries grouped under their current asteroid-belt anchor and focusable large-object subentries for inspectable belt bodies until a dedicated reservoir viewer/export panel exists.
- `src/app/viewer/ObjectViewer.Options.cs`: shared Object Viewer options-dialog and compact bottom-right `Controls` panel behavior aligned with the Galaxy and System viewers, including collapsed sizing and expanded sizing measured from the actual viewer help content.
- `src/app/viewer/ObjectViewer.tscn`: active Object Viewer scene, now reduced to viewer chrome, file operations, a dedicated scene-owned inspector host, the options dialog, and the compact controls panel instead of embedding a second object-generation editor stack.
- `src/app/themes/icons/CheckboxUnchecked.svg`, `src/app/themes/icons/CheckboxChecked.svg`: explicit white-bordered checkbox glyphs used by the shared dark theme so unchecked boxes remain visible across studios, viewers, and options dialogs.
- `src/app/GalaxyGenerationScreen.cs`: galaxy-studio controller now exposes the concrete `Space Opera` override levers in the center-column override panel, persists those values into `GenerationUseCaseSettings`, and keeps them hidden outside the active `Space Opera` profile.
- `src/app/SystemGenerationScreen.cs`: system-studio controller mirrors the same `Space Opera` override controls and now writes the user-tuned compatibility pressures into `SolarSystemSpec.UseCaseSettings` instead of treating the RPG profile as fixed defaults only.
- `src/app/SystemGenerationScreen.Planetary.cs`: system-studio partial controller for aggregate planetary controls, summaries, and profile serialization.
- `src/domain/generation/specs/PlanetSpec.cs`: expanded with direct single-planet controls such as orbit mode, class bias, composition bias, envelope override, volatile richness, hydrosphere tendency, moon-bundle settings, and formation trace metadata.
- `src/domain/system/SystemPlanetGenerator.cs`: retrofit layer that consumes the shared upstream planetary profile without replacing the existing system planet-generation entrypoint.
- `src/domain/generation/generators/planet/PlanetAtmosphereGenerator.cs`: atmosphere-retention layer now responds to hot loss regimes, volatile-delivery context, and habitable-zone-aware formation trace rather than treating all rocky atmospheres as isolated local outcomes.
- `src/domain/generation/generators/planet/PlanetAtmosphereGenerator.cs`: atmosphere source-utilization pass now records active/context/underutilized atmosphere sources, secondary-atmosphere escape pressure, retention scalar, pre-main-sequence XUV risk, final atmosphere regime, composition family, and oxygen context in planet formation provenance.
- `src/domain/system/SystemMoonGenerator.cs`: moon generation now reacts to the shared planetary-system state so giant hosts, snow-line context, and moon-formation bias change regular-vs-captured architecture and count tendencies.
- `src/domain/system/SystemMoonGenerator.cs`: moon source-utilization pass now records moon-channel provenance, separates regular CPD, captured irregular, and impact-limited terrestrial candidates, applies satellite-scale regular-moon mass budgets, and records active/context/underutilized moon sources on generated moons.
- `src/domain/system/SmallBodyReservoir.cs`: serializable system-level small-body reservoir record for asteroid, TNO, Centaur, and comet-feeding families that should be available to science audits, export, and future UI independently from asteroid-belt render geometry.
- `src/domain/system/AsteroidBelt.cs`: serializable belt model carrying orbit bounds, composition, major-body IDs, and source-backed reservoir/composition/size-distribution/subfamily provenance for generated small-body reservoirs.
- `src/domain/system/SystemAsteroidGenerator.cs`: asteroid-belt placement and composition now respond to solid budget, outer-reservoir strength, comet-leaning bias, TNO-style cold-reservoir placement, diagnostic TNO subfamily proxies, and minor-body population slope instead of using flat belt assumptions; it emits first-class small-body reservoir records linked to the current belt anchor and up to 10 inspectable large objects using diameter-based `>= 500 km` semantics.
- `src/domain/population/ProfileGenerator.cs`: derived life-support profiles now carry stellar flux, habitable-zone alignment, and XUV exposure for downstream ecology and biosphere decisions.
- `src/domain/population/SentientWorldProfile.cs`: neutral inhabited-world baseline that stores settlement pattern, logistics capacity, dominant regime, neutral `0-24` core tech levels, per-domain technology access records, law-facing capacity fields, jurisdiction structure/pluralism/conflict signals, first-class factions, culture tags, institutional religion structure, life-biome availability, technology-access diagnostics, and the structural societal axes used by RPG adapters.
- `src/domain/population/TechnologyDomainAccessRecord.cs`: serializable neutral technology-domain access record carrying core/elite/median access, adoption, lag, inequality, and source-signal fields for worldbuilding and adapter export.
- `src/domain/population/SentientFactionRecord.cs`: serializable deterministic faction record carrying influence, alignment, tension, source population, and primary issue for the neutral sentient-world baseline.
- `src/domain/population/SentientWorldProfileBuilder.cs`: deterministic builder that derives the neutral sentient-world baseline from active native and colony populations plus the body's environmental, suitability, global/domain technology-access, law, jurisdiction, faction, life-biome, and adoption-lag context.
- `src/domain/population/StationPopulationProfileBuilder.cs`: deterministic builder that derives the same neutral sentient population baseline for a single space station from station class, services, construction role, governance, and population.
- `scripts/CreateReleaseBuild.ps1`: Windows-hosted release helper that runs the build and headless verification gates, temporarily stamps the requested `demo` or `export` edition channel, exports the configured release presets, zips platform folders, and prints suggested itch `butler` commands.
- `src/app/shared/ReleaseEditionService.cs`: shared build-channel resolver that maps the configured release channel and export features to persistence capability gates while keeping the approved plain release version visible in-app.
- `Docs/V0.10ReleaseChecklist.md`: concrete `0.10` release-prep procedure covering version sync, verification gates, export flow, artifact review, and itch upload steps.
- `Docs/V0.10AcceptanceChecklist.md`: live manual QA checklist for exported `0.10` artifacts, covering startup, studios, viewers, station generation, mainline scope boundaries, and packaging validation.
- `Docs/V0.9ReleaseChecklist.md`: historical `0.9` release-prep procedure retained for reference.
- `Docs/V0.9AcceptanceChecklist.md`: historical manual QA checklist for exported `0.9` artifacts.
- `Docs/V1.0Checklist.md`: concrete checklist for the remaining scope lock, UI, realism, sentient-world audit, and release-hardening work before a defensible `1.0`.
- `src/domain/population/BiologySupportEvaluator.cs`: summary biology support now uses weighted orbit, XUV, tidal-heating, nutrient-access, and abiotic-oxygen false-positive constraints so biosphere and civilization support reflect system context without turning generation into a simulation.
- `Tests/Unit/TestPlanetaryGenerationProfile.cs`: deterministic serialization, propagation, and planetary-help metadata coverage for the shared planetary retrofit.
- `Tests/Unit/Population/TestBiologySupportEvaluator.cs`: population-side regression coverage for XUV penalties, bounded tidal-heating benefits on icy moons, separate biosignature detectability diagnostics, and civilization discounting when breathable oxygen has high abiotic false-positive risk.
- `Tests/Unit/Population/TestSentientWorldProfile.cs`: deterministic regression coverage for sentient-world baseline derivation and serialization, including settlement/logistics/governance summary fields, global and domain technology-access diagnostics, law interpretation, jurisdiction signals, faction records, life-biome gating, and Starfinder adapter-only magic output.
- `Sources/Texts/planets.md`: implementation-oriented deterministic planet-formation specification used as the design target for the upstream planetary retrofit.
- `Sources/Texts/ChenKipping2017.txt`, `Sources/Texts/Otegi2020.txt`, `Sources/Texts/OwenWu2017.txt`, `Sources/Texts/Ginzburg2018.txt`, `Sources/Texts/Mordasini2007.txt`, `Sources/Texts/LambrechtsJohansen2012.txt`, `Sources/Texts/Mroz2020.txt`: reviewed source notes added for the `0.8.8.0` planetary-priors cleanup so every surfaced planetary-prior model points to an external paper-backed basis instead of legacy internal notes.
- `Sources/Texts/Petigura2013.txt`, `Sources/Texts/Bryson2021.txt`, `Sources/Texts/Ribas2015.txt`, `Sources/Texts/Pascucci2016.txt`, `Sources/Texts/Izidoro2017.txt`, `Sources/Texts/Fernandes2019.txt`, `Sources/Texts/RaymondIzidoro2017.txt`: reviewed source notes added for the current planetary-grounding pass so terrestrial occurrence, habitable-zone rocky yield, disk lifetime, disk mass scaling, migration-shaped compact systems, snow-line giant-planet turnover, and volatile-delivery coupling all point to explicit academic anchors.
- `Sources/Texts/Kasting1993.txt`, `Sources/Texts/Kopparapu2013.txt`: reviewed source notes added for the habitable-zone parameter pass so StarGen's explicit circumstellar habitable-zone model choices point to named academic climate-model families rather than a mixed legacy implementation.
- `Sources/AnnotatedBibliography.md`, `Sources/ToReview.md`, `Sources/Texts/planets.md`: expanded planetary source-audit documents that now map StarGen’s aggregate planetary priors to concrete observational and disk-physics calibration anchors instead of treating the upstream sliders as generic heuristics.
- `Sources/Texts/Fulton2017.txt`, `Sources/Texts/FischerValenti2005.txt`, `Sources/Texts/DeMeoCarry2014.txt`, `Sources/Texts/BauerEtAl2017.txt`, `Sources/Texts/Kopparapu2014.txt`, `Sources/Texts/HellerBarnes2013.txt`: reviewed source notes retained for planet demographics, moon formation, small-body placement, comet grounding, and biosphere-support constraints. `Lamy2004` is tracked only as replaced/blocked active support.
- `Sources/Texts/DiazGarcia2016.txt`, `Sources/Texts/Hurley2000.txt`, `Sources/Texts/Kormendy2009.txt`, `Sources/Texts/TanakaTakeuchiWard2002.txt`: end-to-end science-audit source notes for the first-pass unsupported-claim cleanup, covering bar-strength morphology context, elliptical structural families, compact stellar-lifetime approximations, and the Type-I migration framework. These notes are tracked as AI-assisted drafts pending human verification before they are treated as authoritative.
- `Tests/Quality/TestEndToEndScienceAudit.cs`: audit-contract regression that keeps the tracked end-to-end science audit document, source-note bundle, and required inline citation or `tuning` markers present in the codebase.
- `src/domain/generation/parameters/LifeScienceReferenceCatalog.cs`: source registry and plain-language help content for the galaxy-studio life-model stack, including the documented `Earth-Anchored Composite` synthesis preset and the per-stage abiogenesis, complex-life, civilization, and environmental-window controls.
- `src/domain/generation/parameters/ObjectScienceReferenceCatalog.cs`: source registry and plain-language conflict-note/help content for Object Studio, tying direct-setting tensions such as airless versus ocean-heavy worlds to explicit academic sources.
- `src/domain/population/LifePotentialModeling.cs`: life-model resolver that converts the selected framework and per-stage assumptions into distinct biosphere-support, abiogenesis, complex-life, sentience, civilization, and stability-window tuning.
- `src/app/SystemGenerationScreen.Life.cs`: scene-first System Studio controller for the audited life-model stack so system generation uses the same sourced abiogenesis, complex-life, civilization, and environmental-window settings as Galaxy Studio.
- `src/domain/generation/parameters/ObjectGenerationParameterCatalog.cs`: direct-object parameter catalog that keeps Object Studio tooltips tied to what each single-object control materially changes instead of reusing aggregate generation language.
- `src/app/ObjectGenerationScreen.Help.cs`: scene-first Object Studio help-popup controller for the direct-object workflow, reviewed science notes, and close-safe popup sizing.
- `src/app/ObjectGenerationScreen.Life.cs`: scene-first Object Studio controller for direct planet-local life settings that write through to the generated request without exposing aggregate upstream priors.
- `src/app/shared/HelpDialogLayoutHelper.cs`: shared helper that constrains modal help popups to the current viewport so close controls stay visible across the Galaxy, System, and Object studios.
- `Tests/Unit/TestSolarSystemReferenceChain.cs`: staged Solar-System analogue regression that preflights a Sun-like star, solar-compatible orbit slots, and the eight major planets before locking exact Solar values, with stage-specific failure messages when the live generation chain drifts.
- `Sources/Texts/LineweaverDavis2002.txt`, `Sources/Texts/SpiegelTurner2012.txt`, `Sources/Texts/ForganRice2010.txt`, `Sources/Texts/Mills2024.txt`, `Sources/Texts/Balbi2023.txt`: reviewed source notes used by the `0.8.10.0` life-model expansion for optimistic biogenesis, conservative abiogenesis interpretation, Rare Earth filtering, environmental windows, and oxygen bottlenecks for technospheres.
- `Sources/Texts/WordsworthKreidberg2022.txt`: reviewed source note used by the `0.8.13.0` Object Studio conflict pass for rocky-planet atmosphere loss, airless end states, and hydrosphere tension on stripped worlds.
- `Sources/Texts/Hart2017.txt`, `Sources/Texts/Lingard2021.txt`, `Sources/Texts/RodriguezPadilla2013.txt`: reviewed source notes added for the `0.8.11.0` scientific-parameter audit so Galaxy Studio arm-count, pitch, arm-mechanism, and ellipticity controls remain tied to explicit external sources.
- `Sources/Texts/TravellerLicensing2026.txt`, `Sources/Texts/CepheusEngineSRD2026.txt`, `Sources/Texts/StarforgedLicensing2026.txt`, `Sources/Texts/StarfinderCompatibility2026.txt`: reviewed ruleset/licensing notes added to support the planned expansion of `Generation Overrides` toward clean-room RPG compatibility profiles.
- `src/domain/generation/specs/CometSpec.cs`: serializable comet-generation spec carrying family, activity-state, and large-nucleus controls for object and viewer generation.
- `src/domain/generation/generators/AsteroidGenerator.cs`: deterministic asteroid generator with DeMeo/Carry taxonomy provenance for generated asteroid bodies.
- `src/domain/generation/generators/CometGenerator.cs`: deterministic comet generator that produces Jupiter-family and long-period comet bodies with Bauer-style nucleus/activity defaults, active-fraction metadata, and explicit legacy wide-range compatibility behavior.
- `src/app/ObjectGenerationScreen.tscn`: Object Studio scene now behaves as a context-sensitive single-object editor, with moon generation folded under planets, richer asteroid and planet controls, and a dedicated comet section.
- `src/app/ObjectGenerationScreen.EnhancedUi.cs`: scene-first Object Studio controller that hides unrelated controls by object type, keeps Traveller-only rows under generation rules, and binds the new comet, asteroid, planet, and star-specific authoring flow.
- `src/app/SystemGenerationScreen.tscn`: System Studio scene now carries the same left-column life model stack and `Generation Overrides` wording as the audited Galaxy Studio surface.
- `src/app/viewer/ObjectViewer.SaveLoad.cs`: object-viewer save/load and remaining preset plumbing now guard against the stripped viewer-only scene shape while preserving compatibility with the expanded object taxonomy where those paths are still used.
- `Tests/Unit/TestCometGenerator.cs`: deterministic comet-generation coverage for type identity, preset behavior, and repeatability.
- `Tests/Integration/TestStudioScienceUi.cs`: non-visual integration coverage for Object Studio context gating, comet availability, moon-under-planet controls, and the relocated Traveller rules rows.
- `Sources/Texts/Cummings2018.txt`, `Sources/Texts/Kirkpatrick2000.txt`, `Sources/Texts/Kirkpatrick2011.txt`, `Sources/Texts/Kirkpatrick2024.txt`, `Sources/Texts/MoeDiStefano2017.txt`, `Sources/Texts/Tokovinin2021.txt`: reviewed source notes added for the stellar-population expansion covering white dwarfs, brown dwarfs, and multiplicity.
- `src/domain/generation/archetypes/StarClass.cs`: expanded stellar spectral support to include the practical brown-dwarf classes `L`, `T`, and `Y`.
- `src/domain/generation/tables/StarTable.cs`: widened the stellar lookup tables to cover substellar classes and align the hydrogen-burning boundary with the expanded stellar offerings.
- `src/domain/generation/generators/StarGenerator.cs`: now resolves brown dwarfs, evolved stars, and white dwarfs instead of limiting output to main-sequence stars.
- `src/domain/system/StellarConfigGenerator.cs`: now uses stronger empirical multiplicity shaping, coeval companion handling, and wider hierarchical system layouts for multi-star generation.
- `src/app/GalaxyGenerationScreen.Science.cs`: partial controller for galaxy-studio scientific controls, plain-language tooltips, the header `Help` popup, and scene-bound science/help content.
- `src/app/SystemGenerationScreen.Help.cs`: system-studio help popup controller for plain-language star/system explanations and shared science references.
- `src/app/SystemGenerationScreen.Stellar.cs`: scene-first system-studio controller for stellar science controls and summaries.
- `src/domain/galaxy/GalaxyEnums.cs`: shared galaxy-science enums covering family/subtype/bar and related scientific-choice modes.
- `src/domain/galaxy/GalaxyOriginContext.cs`: serializable galaxy-origin context passed downstream into star and system generation, now including the resolved stellar-generation profile plus Milky-Way solar-circle and circular-velocity context.
- `src/domain/galaxy/GalaxyRealismProfile.cs`: derived galaxy realism profile carrying morphology, GHZ, metallicity, star-formation priors, and the source-marked Milky-Way structural schema fields.
- `src/domain/galaxy/GalaxyRealismProfileBuilder.cs`: deterministic builder that resolves the scientific galaxy profile from user config and seed, including Bland-Hawthorn/Gerhard thin/thick disk, bar, solar-circle, velocity, and stellar-mass fields for Milky-Way analog defaults.
- `src/domain/galaxy/GalaxyScientificFieldEvaluator.cs`: evaluates region, metallicity, age, GHZ, cluster-scaffold context, bar-region geometry, and local stellar-profile adjustments for generated galaxy stars.
- `src/domain/galaxy/LenticularDensityModel.cs`: galaxy-density evaluator for the new lenticular family.
- `src/domain/generation/StellarGenerationProfile.cs`: shared serializable stellar-generation settings used by both galaxy and system generation flows.
- `src/domain/generation/generators/StellarMassSampler.cs`: deterministic IMF-driven stellar mass sampler with Kroupa/Chabrier support and context-aware variation.
- `src/domain/generation/generators/StellarIsochroneApproximator.cs`: lightweight deterministic stellar-property resolver inspired by MIST/PARSEC model families.
- `src/domain/generation/parameters/GalaxyScienceReferenceCatalog.cs`: source registry and user-facing plain-language science/help content for the galaxy studio.
- `src/domain/generation/parameters/StellarScienceReferenceCatalog.cs`: source registry and plain-language stellar science/help content shared by the galaxy and system studios.
- `Tests/Integration/TestStudioScienceUi.cs`: non-visual integration coverage for the galaxy help popup and the `1..10` stellar controls in the studios.
- `Tests/Unit/TestStellarGenerationProfile.cs`: deterministic unit coverage for stellar-profile serialization and metadata wiring.
