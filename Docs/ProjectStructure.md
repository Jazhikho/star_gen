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
|   |-- CivilisationEngine/
|   |-- ConlangGenerator/
|   |-- DiseaseSimulator/
|   |-- EcologyGenerator/
|   |-- EvoTechTree/
|   `-- ReligionGenerator/
|-- Docs/
|   |-- Assets.md
|   |-- CelestialBodyProperties.md
|   |-- GDD.md
|   |-- ProjectStructure.md
|   |-- RegimeChangeModel.md
|   |-- Roadmap.md
|   |-- ScientificParameterAudit.md
|   `-- TravellerWorldCreation.md
|-- NuGet/
|   `-- Migrations/
|-- Sources/
|-- Resources/
|   `-- Audio/
|-- src/
|   |-- app/
|   |   |-- audio/                     # shared app-level audio controller and cue library used by splash/menu/UI playback
|   |   |-- components/                # reusable scene-authored UI fragments/templates for shipped viewers and editors
|   |   |-- concepts/                  # parked mainline-inactive concept UI/runtime pending migration work
|   |   |-- galaxy_viewer/
|   |   |-- jumplanes_prototype/
|   |   |-- prototypes/
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
|       `-- persistence/               # retained for export branch compatibility, not shipped in 0.9 mainline UI
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

- `src/app/audio/AppAudioController.cs`: shared app-level audio controller that owns reusable music and UI players so startup/menu/UI playback uses explicit exported resources instead of ad hoc runtime file scans.
- `src/app/audio/AppAudioLibrary.cs`: global-class resource declaring the application's shared music and UI cue streams and their default playback volumes.
- `src/app/audio/AppAudioCueId.cs`: stable cue identifiers used by the controller and callers such as the splash screen.
- `Resources/Audio/MainAudioLibrary.tres`: default shared audio library resource, currently binding the intro music cue so desktop exports include the startup audio as a real dependency.
- `Tests/Integration/TestAppAudio.cs`: non-visual regression coverage proving `MainApp` owns the shared audio controller, that intro music is configured through the shared library rather than a splash-local player, and that the persisted skip-intro preference opens directly to the main menu.
- `Docs/ScientificParameterAudit.md`: tracked audit matrix for the shipped galaxy, stellar, planetary, and life science controls, including source relevance, generator wiring, expected outcome direction, test coverage, and disposition.
- `src/domain/generation/PlanetaryGenerationProfile.cs`: shared serializable aggregate planetary-formation profile used by galaxy and system generation to carry mass-radius, envelope-loss, gas-giant, metallicity-coupling, rogue-planet, moon-bias, and outer-system-bias assumptions.
- `src/domain/generation/tables/PlanetMassRadiusTable.cs`: empirical planet mass-radius resolver that now applies the supported Chen-Kipping and Otegi model families directly to generated planet sizes and densities, with direct planet-facing overrides layered on top.
- `src/domain/generation/PlanetarySystemState.cs`: derived per-system planetary state built once from stellar context plus the shared planetary profile so downstream planet generation can react to snow-line, solid/gas budget, escape-pressure, migration/stirring surrogates, habitable-zone alignment, XUV activity, volatile delivery, and outer-reservoir strength.
- `src/domain/generation/parameters/PlanetaryScienceReferenceCatalog.cs`: source registry and plain-language help/tooltips for the aggregate planetary controls shared by Galaxy Studio and System Studio.
- `src/domain/generation/RpgCompatibilityProfile.cs`: shared clean-room compatibility-profile resolver for `Space Opera`, `Cepheus`, `Starfinder`, and `Starforged`, including the generation-pressure defaults that now drive mainworld targeting, system fill bias, population forcing, and UWP-like behavior where appropriate.
- `src/app/SystemGenerationScreen.Planetary.cs`: system-studio partial controller for aggregate planetary controls, summaries, and profile serialization.
- `src/domain/generation/specs/PlanetSpec.cs`: expanded with direct single-planet controls such as orbit mode, class bias, composition bias, envelope override, volatile richness, hydrosphere tendency, moon-bundle settings, and formation trace metadata.
- `src/domain/system/SystemPlanetGenerator.cs`: retrofit layer that consumes the shared upstream planetary profile without replacing the existing system planet-generation entrypoint.
- `src/domain/generation/generators/planet/PlanetAtmosphereGenerator.cs`: atmosphere-retention layer now responds to hot loss regimes, volatile-delivery context, and habitable-zone-aware formation trace rather than treating all rocky atmospheres as isolated local outcomes.
- `src/domain/system/SystemMoonGenerator.cs`: moon generation now reacts to the shared planetary-system state so giant hosts, snow-line context, and moon-formation bias change regular-vs-captured architecture and count tendencies.
- `src/domain/system/SystemAsteroidGenerator.cs`: asteroid-belt placement and composition now respond to solid budget, outer-reservoir strength, and comet-leaning outer-system bias instead of using flat belt assumptions.
- `src/domain/population/ProfileGenerator.cs`: derived life-support profiles now carry stellar flux, habitable-zone alignment, and XUV exposure for downstream ecology and biosphere decisions.
- `src/domain/population/BiologySupportEvaluator.cs`: summary biology support now uses weighted orbit, XUV, and tidal-heating constraints so biosphere support reflects system context without turning generation into a simulation.
- `Tests/Unit/TestPlanetaryGenerationProfile.cs`: deterministic serialization, propagation, and planetary-help metadata coverage for the shared planetary retrofit.
- `Tests/Unit/Population/TestBiologySupportEvaluator.cs`: population-side regression coverage for XUV penalties and bounded tidal-heating benefits on icy moons.
- `Sources/Texts/planets.md`: implementation-oriented deterministic planet-formation specification used as the design target for the upstream planetary retrofit.
- `Sources/Texts/ChenKipping2017.txt`, `Sources/Texts/Otegi2020.txt`, `Sources/Texts/OwenWu2017.txt`, `Sources/Texts/Ginzburg2018.txt`, `Sources/Texts/Mordasini2007.txt`, `Sources/Texts/LambrechtsJohansen2012.txt`, `Sources/Texts/Mroz2020.txt`: reviewed source notes added for the `0.8.8.0` planetary-priors cleanup so every surfaced planetary-prior model points to an external paper-backed basis instead of legacy internal notes.
- `Sources/Texts/Fulton2017.txt`, `Sources/Texts/FischerValenti2005.txt`, `Sources/Texts/CanupWard2006.txt`, `Sources/Texts/DeMeoCarry2014.txt`, `Sources/Texts/Lamy2004.txt`, `Sources/Texts/Kopparapu2014.txt`, `Sources/Texts/HellerBarnes2013.txt`: reviewed source notes added for the `0.8.7.0` calibration pass across planet demographics, moon formation, small-body placement, and biosphere-support constraints.
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
- `src/domain/generation/generators/CometGenerator.cs`: deterministic comet generator that produces Jupiter-family and long-period comet bodies with comet-specific defaults and metadata.
- `src/app/ObjectGenerationScreen.tscn`: Object Studio scene now behaves as a context-sensitive single-object editor, with moon generation folded under planets, richer asteroid and planet controls, and a dedicated comet section.
- `src/app/ObjectGenerationScreen.EnhancedUi.cs`: scene-first Object Studio controller that hides unrelated controls by object type, keeps Traveller-only rows under generation rules, and binds the new comet, asteroid, planet, and star-specific authoring flow.
- `src/app/SystemGenerationScreen.tscn`: System Studio scene now carries the same left-column life model stack and `Generation Overrides` wording as the audited Galaxy Studio surface.
- `src/app/viewer/ObjectViewer.SaveLoad.cs`: object-viewer generation and preset plumbing now support comets, planet-plus-moon generation requests, and the expanded object taxonomy.
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
- `src/domain/galaxy/GalaxyOriginContext.cs`: serializable galaxy-origin context passed downstream into star and system generation, now including the resolved stellar-generation profile.
- `src/domain/galaxy/GalaxyRealismProfile.cs`: derived galaxy realism profile carrying morphology, GHZ, metallicity, and star-formation priors.
- `src/domain/galaxy/GalaxyRealismProfileBuilder.cs`: deterministic builder that resolves the scientific galaxy profile from user config and seed.
- `src/domain/galaxy/GalaxyScientificFieldEvaluator.cs`: evaluates region, metallicity, age, GHZ, cluster-scaffold context, and local stellar-profile adjustments for generated galaxy stars.
- `src/domain/galaxy/LenticularDensityModel.cs`: galaxy-density evaluator for the new lenticular family.
- `src/domain/generation/StellarGenerationProfile.cs`: shared serializable stellar-generation settings used by both galaxy and system generation flows.
- `src/domain/generation/generators/StellarMassSampler.cs`: deterministic IMF-driven stellar mass sampler with Kroupa/Chabrier support and context-aware variation.
- `src/domain/generation/generators/StellarIsochroneApproximator.cs`: lightweight deterministic stellar-property resolver inspired by MIST/PARSEC model families.
- `src/domain/generation/parameters/GalaxyScienceReferenceCatalog.cs`: source registry and user-facing plain-language science/help content for the galaxy studio.
- `src/domain/generation/parameters/StellarScienceReferenceCatalog.cs`: source registry and plain-language stellar science/help content shared by the galaxy and system studios.
- `Tests/Integration/TestStudioScienceUi.cs`: non-visual integration coverage for the galaxy help popup and the `1..10` stellar controls in the studios.
- `Tests/Unit/TestStellarGenerationProfile.cs`: deterministic unit coverage for stellar-profile serialization and metadata wiring.
