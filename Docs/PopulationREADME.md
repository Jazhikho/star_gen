# Population Framework — File Index

All files in the project that relate to the population framework (branch: `population`). Use this as a quick map of where everything lives.

---

## Documentation (Docs/)

| File | Purpose |
|------|---------|
| `Docs/PopulationFrameworkPlan.md` | Plan for the population framework: scope, design decisions, stage order, file structure, testing, acceptance criteria. |
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

### Test support

| File | Purpose |
|------|---------|
| `Tests/PopulationDeps.gd` | Preloads population enums (ClimateZone, BiomeType, ResourceType, HabitabilityCategory) so `class_name` types are in scope for test scripts. Loaded by `RunTestsHeadless.gd` and `TestScene.gd` before population tests run. |

Population test scripts are registered in `Tests/RunTestsHeadless.gd` and `Tests/TestScene.gd` under the “Population framework” sections.
