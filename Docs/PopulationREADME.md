# Population Framework — File Index

All files in the project that relate to the population framework (branch: `population`) and its extensions (e.g. `outposts-and-spacestations`). Use this as a quick map of where everything lives.

---

## Documentation (Docs/)

| File | Purpose |
|------|---------|
| `Docs/PopulationFrameworkPlan.md` | Plan for the population framework: scope, design decisions, stage order, file structure, testing, acceptance criteria. Includes extension: Outposts and Space Stations (branch: `outposts-and-spacestations`). |
| `Docs/RegimeChangeModel.md` | Mermaid diagram and notes: long-run drivers, state sliders, regime forms (R0–R13), baseline paths, crisis-driven shifts. |
| `Docs/PopulationREADME.md` | This index of population-related files. |

Related (reference only): `Docs/Roadmap.md` (branch/phase context), `Docs/CelestialBodyProperties.md` (properties used by ProfileCalculations).

---

## Domain (src/domain/population/)

| File | Purpose |
|------|---------|
| `ClimateZone.gd` | Enum: climate zone classifications (e.g. POLAR, TEMPERATE, TROPICAL, EXTREME). |
| `BiomeType.gd` | Enum: surface biome types (e.g. OCEAN, FOREST, DESERT, BARREN, VOLCANIC). |
| `ResourceType.gd` | Enum: exploitable resource types (e.g. WATER, SILICATES, METALS, ORGANICS). |
| `HabitabilityCategory.gd` | Enum: derived habitability categories (IMPOSSIBLE → IDEAL) from a 0–10 score. |
| `PlanetProfile.gd` | Data model: derived summary of a planet’s habitability-related and surface/atmosphere properties; serialization (to_dict/from_dict). |
| `ProfileCalculations.gd` | Pure calculation functions: habitability score, weather severity, radiation, climate zones, biomes, resources, breathability, moon modifiers (tidal heating, parent radiation, eclipse). |
| `ProfileGenerator.gd` | Builds a `PlanetProfile` from a `CelestialBody` and `ParentContext` (optional parent body for moons). |
| `Outpost.gd` | Small station data model (≤10k population): purpose-driven installations with outpost authority governance. |
| `SpaceStation.gd` | Scalable station data model: supports small (U/O) to city-sized (B/A/S) with appropriate governance (outpost authority or full government). |
| `StationPlacementRules.gd` | Pure functions for station placement logic: evaluates system context (bridge, colony, native, resources) and returns placement recommendations. |
| `StationSpec.gd` | Generation specification: controls station generation parameters (counts, classes, purposes, years, density). |
| `StationGenerator.gd` | Generates stations for a system: creates Outposts and SpaceStations based on context and spec with deterministic RNG. |

---

## Tests (Tests/)

### Unit tests (Tests/Unit/Population/)

| File | Purpose |
|------|---------|
| `TestClimateZone.gd` | Tests for ClimateZone enum and helpers. |
| `TestBiomeType.gd` | Tests for BiomeType enum and helpers. |
| `TestResourceType.gd` | Tests for ResourceType enum and helpers. |
| `TestHabitabilityCategory.gd` | Tests for HabitabilityCategory and score→category mapping. |
| `TestPlanetProfile.gd` | Tests for PlanetProfile: creation, serialization, JSON key handling, helpers. |
| `TestProfileCalculations.gd` | Unit tests for ProfileCalculations (habitability, weather, radiation, climate zones, biomes, resources, breathability, tidal heating, parent radiation, eclipse). |
| `TestProfileGenerator.gd` | Integration tests: ProfileGenerator.generate() with Earth-like, Mars-like, Europa-like bodies; determinism; serialization round-trip; tidal locking; moon modifiers. |
| `TestOutpost.gd` | Tests for Outpost: creation, validation, services, serialization, factory methods. |
| `TestSpaceStation.gd` | Tests for SpaceStation: class transitions, governance models, growth states, serialization. |
| `TestStationPlacementRules.gd` | Tests for placement rules: context determination, station counts, orbital candidates, purposes, resource richness. |
| `TestStationSpec.gd` | Tests for StationSpec: factories, validation, purpose/class filtering, serialization. |
| `TestStationGenerator.gd` | Tests for StationGenerator: context-based generation, determinism, limits, filtering, result helpers. |

### Test support

| File | Purpose |
|------|---------|
| `Tests/PopulationDeps.gd` | Preloads population enums (ClimateZone, BiomeType, ResourceType, HabitabilityCategory) so `class_name` types are in scope for test scripts. Loaded by `RunTestsHeadless.gd` and `TestScene.gd` before population tests run. |

Population test scripts are registered in `Tests/RunTestsHeadless.gd` and `Tests/TestScene.gd` under the “Population framework” sections.

---

## Prototypes (src/app/prototypes/)

| File | Purpose |
|------|---------|
| `StationGeneratorPrototype.gd` | Interactive prototype for StationGenerator: explore system contexts, generation parameters, and generated stations. |
| `StationGeneratorPrototype.tscn` | Scene for the station generator prototype. Run with F6 or: `godot --path . res://src/app/prototypes/StationGeneratorPrototype.tscn` |
