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
|   `-- TravellerWorldCreation.md
|-- NuGet/
|   `-- Migrations/
|-- Sources/
|-- src/
|   |-- app/
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
