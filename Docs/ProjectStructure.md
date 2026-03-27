# StarGen Project Structure

Current structure snapshot for major project folders and key entry files.

Notes:
- Excludes generated/cache metadata such as `.uid`, `.godot/`, `.mono/`, and `.git/`.
- `release/` and `releases/` are local build output folders and are git-ignored.

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
|   |-- TravellerWorldCreation.md
|   `-- galactic_formation.md
|-- NuGet/
|   `-- Migrations/
|-- Sources/
|-- src/
|   |-- app/
|   |   |-- components/
|   |   |-- concepts/
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
|   |   |-- concepts/
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
|       |-- concepts/
|       |-- export/
|       `-- persistence/
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
