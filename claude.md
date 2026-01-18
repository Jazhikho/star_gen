# StarGen (Godot 4.x, GDScript)

## What we are building
StarGen is a deterministic procedural generator + viewer/editor for:
1) Individual celestial objects (stars, planets, moons, asteroids) — viewable + editable
2) Solar systems — generate + view first, then constraints, then editing tools
3) Galactic scale — later, built on top of the above

Guiding principles:
- DRY + SOLID
- Determinism and test coverage as non-negotiables
- Finish vertical slices, do not balloon scope

---

## Current roadmap (phases)
Phase 0: Foundations and guardrails
Phase 1: Celestial object model
Phase 2: Object generators v1
Phase 3: Object viewer v1
Phase 4: Object editing v1
Phase 5: Object rendering v2 (optional)
Phase 6: Solar system generator + viewer
Phase 7: Solar system constraints (locks)
Phase 8: Solar system editing tools
Phase 9: Galactic map v1
Phase 10: Galactic constraints and editing

_See `Docs/Roadmap.md` for detailed phase descriptions, deliverables, tests, and acceptance criteria._

Claude: when asked for new features, first map them to the CURRENT phase.
If out-of-scope, add to BACKLOG.md and do NOT implement.

---

## Architecture (DRY + SOLID)
### Layers
- `res://src/domain/`     Pure logic. No Nodes, no SceneTree, no file IO.
- `res://src/services/`   Persistence/export/caching/adapters. Uses Godot APIs if needed.
- `res://src/app/`        Scenes, UI, input, rendering. Calls domain/services.

Dependency direction:
```
app → services → domain
```
Never domain → services/app.

### Determinism rules
- All randomness goes through a single injected RNG wrapper.
- Generators must accept (spec, rng) and return data without touching global state.
- Every generated entity stores provenance: seed, generator version, spec snapshot.

---

## Data model rules
- Use composition over inheritance.
- Prefer small Resource-like components (PhysicalProps, AtmosphereProps, RingProps).
- Keep editing and generation separate:
  - Generation uses Spec + RNG
  - Editing uses validated commands and recalculates derived values

---

## Testing rules
- All non-trivial logic changes ship with tests.
- Categories:
  - Unit: encode/decode, generation invariants, validation, commands
  - Integration: scenes boot, load/save flows, viewer does not crash
  - Regression: golden-master JSON fixtures for known seeds
- Determinism tests are mandatory for generators.

---

## Deliverable format for code changes
When proposing implementation, include:
1) File list (paths)
2) Code blocks per file (or diffs if small)
3) Tests added/updated
4) Commands to run tests
5) Acceptance checklist (manual verification steps)

---

## Backlog discipline
All out-of-scope ideas go to BACKLOG.md with:
- Title
- Why it matters
- Which phase it belongs to
- Complexity (S/M/L)
No implementing backlog items unless the current phase explicitly includes them.

---

## Code style & conventions
### File organization
- Domain logic: `src/domain/`
- Services (IO, caching): `src/services/`
- App layer (scenes, UI): `src/app/`
- Shared resources: `Resources/`
- Test scenes: `Tests/`
- Documentation: `Docs/`

### Naming
- Files: PascalCase (e.g., `CelestialObject.gd`, `StarGenerator.gd`)
- Folders: PascalCase (e.g., `Scripts/`, `Resources/`)
- GDScript functions: snake_case (e.g., `generate_star()`, `validate_spec()`)
- GDScript variables: snake_case (e.g., `object_seed`, `physical_props`)
- Constants: UPPER_SNAKE_CASE (e.g., `MAX_RADIUS`, `DEFAULT_TEMPERATURE`)

### Typing
- Explicit types everywhere. No `:=` inference.
- Type all function parameters and return values.
- Type all variables (no untyped `var`).

### Documentation
- Doc blocks above every function: purpose, parameters, returns.
- For `@export` variables, describe what they are ABOVE the export using `##` comments.
- Inside functions, comment **why**, not what.

### Script size
- Target ~10 functions per script (soft limit).
- If exceeding 12-15 functions, refactor or split by responsibility.

---

## Phase 0: Foundations and guardrails ✅ COMPLETE
**Focus**: Establish structure, determinism, and automated tests so progress is measurable and regressions are obvious.

**Deliverables**:
- [x] Project structure: domain / services / app / tests directories
- [x] Deterministic RNG wrapper used by all generators (`SeededRng` in `src/domain/rng/`)
- [x] Shared math/validation utilities (`MathUtils`, `Units`, `Validation` in `src/domain/`)
- [x] Test framework wired with CLI run script (`TestCase`, `TestRunner`, `TestResult` in `Tests/Framework/`)
- [x] Version constants: `GENERATOR_VERSION` and `SCHEMA_VERSION` (`Versions` in `src/domain/constants/`)

**Tests**:
- [x] Headless test run returns correct exit code
- [x] RNG determinism test: known seed produces known sequence (9 tests in `TestSeededRng.gd`)
- [x] All utility tests pass (37 total: 9 RNG + 3 Versions + 13 MathUtils + 11 Units + 10 Validation)

**Implementation notes**:
- `SeededRng` wraps Godot's `RandomNumberGenerator` with seed management, state save/restore, and fork support
- Test framework supports headless execution and scene-based testing
- Math utilities provide range checking, remapping, and interpolation functions
- Unit conversion utilities support celestial body calculations (masses, distances, radii, temperatures)
- Validation utilities enforce data constraints with DRY compliance (delegates to MathUtils for range checks)
- All tests pass and verify determinism and utility correctness

**Acceptance criteria**:
- [x] Fresh clone -> one command runs tests successfully

---

## Current phase: Phase 2
**Goal:**
Generate celestial objects deterministically from (spec, seed).

**Phase 1**: ✅ Complete - Celestial object model with validation, serialization, and persistence

**Previous Phase 1 Deliverables** (completed):
- [x] Core data model: CelestialBody with type-specific components (PhysicalProps, SurfaceProps, AtmosphereProps, RingSystemProps)
- [x] StellarProps component for star properties (luminosity, spectral class, habitable zone)
- [x] RingSystemProps with multi-band support (RingBand array)
- [x] Detailed surface: TerrainProps, HydrosphereProps, CryosphereProps
- [x] Enhanced PhysicalProps: oblateness, magnetic_moment, internal_heat_watts
- [x] Validation rules and error reporting (CelestialValidator)
- [x] Serialization: JSON schema with schema_version and generator_version fields (CelestialSerializer)
- [x] Load/save service for celestial objects (CelestialPersistence)
- [x] Provenance tracking (generation_seed, generator_version, schema_version)

**Tests** (completed):
- [x] Validation invariants: mass > 0, radius > 0, optional fields consistent
- [x] Component unit tests: all Props classes, Provenance
- [x] Serialization round-trip: object -> JSON -> object equals original
- [x] Persistence integration: save/load round-trip
- [x] StellarProps and RingSystemProps validation

**Acceptance criteria** (completed):
- [x] Create an object in code, save to JSON, reload, and verify identical content

---

## Current phase: Phase 2
**Goal:**
Generate celestial objects deterministically from (spec, seed).

**Stage 1: Core Infrastructure** ✅
- [x] ParentContext data class for orbital/parent context
- [x] Archetype enums: SizeCategory, OrbitZone, StarClass, AsteroidType, RingComplexity
- [x] Archetype data tables: SizeTable, StarTable, OrbitTable
- [x] Base spec classes with override support
- [x] Spec classes: StarSpec, PlanetSpec, MoonSpec, AsteroidSpec

**Stage 2: Star Generator** ✅
- [x] StarSpec (archetype: O/B/A/F/G/K/M class hints, optional mass/age overrides)
- [x] Main sequence relationships: mass → luminosity → temperature → radius
- [x] Spectral subclass calculation (G2V precision)
- [x] StarGenerator.generate(spec, rng) → CelestialBody

**Stage 3: Planet Generator - Physical Core** ✅
- [x] PlanetSpec with size×orbit archetype
- [x] PlanetGenerator physical properties (mass, radius, density, rotation)
- [x] Orbital properties from archetype + parent context
- [x] Tidal locking detection
- [x] Magnetic field calculation

**Stage 4: Planet Generator - Surface & Atmosphere** (Pending)
- [ ] Atmospheric retention/escape based on escape velocity + stellar UV
- [ ] Surface type selection based on archetype
- [ ] Terrain generation for rocky bodies
- [ ] Hydrosphere/Cryosphere based on temperature + mass
- [ ] Gas giant atmosphere (no terrain)

**Stage 5: Moon Generator** (Pending)
- [ ] MoonSpec with archetype
- [ ] Parent planet context for tidal effects
- [ ] Hill sphere / orbital distance constraints
- [ ] Subsurface oceans for icy moons

**Stage 6: Asteroid Generator** (Pending)
- [ ] AsteroidSpec with C/S/M type
- [ ] Simple physical properties
- [ ] Minimal/no atmosphere
- [ ] Basic terrain (craters, roughness)

**Stage 7: Ring System Generator** (Pending)
- [ ] RingSystemSpec with complexity level
- [ ] Roche limit calculation from parent
- [ ] Resonance-based gap placement (simplified)
- [ ] Composition based on distance from star (ice vs rock)

**Stage 8: Golden Masters & Tests** (Pending)
- [ ] Fixture export utility
- [ ] 28 fixtures (7 per body type)
- [ ] Regression tests against fixtures
- [ ] Range validation tests
- [ ] Physics relationship tests

**Tests**:
- [ ] Golden-master regression: known seeds match saved fixtures
- [ ] Range tests: generated outputs always satisfy validation
- [ ] Stellar relationships: spectral class matches temperature/luminosity ranges
- [ ] Tidal locking: detection correct for close-in bodies

**Acceptance criteria**:
- [ ] Generate each body type from a seed and produce stable JSON outputs

---

## Communication protocol
- Before implementing: confirm phase alignment
- If feature is out-of-scope: add to BACKLOG.md, do not implement
- All code changes: include tests and acceptance criteria
- Breaking changes: document migration path
