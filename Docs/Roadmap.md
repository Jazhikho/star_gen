# StarGen Roadmap (Object-First to Galaxy)

Godot 4.x - GDScript | DRY + SOLID | Deterministic generation with test gates

## Overview
This roadmap builds StarGen in three layers: (1) editable celestial objects, (2) solar systems, then (3) galactic scale. Each phase ends in a shippable vertical slice with a clear acceptance flow and automated tests.

## Guiding principles
•	One phase at a time. Anything outside the current phase goes to BACKLOG.md.
•	Determinism is non-negotiable: same seed + same inputs must produce identical outputs.
•	Keep domain logic pure (no scene tree / Nodes / file I/O inside domain).
•	Ship tests with features. Golden-master fixtures cover regression for known seeds.
•	Prefer composition over inheritance. Small services and data components over "manager" classes.

## Definition of Done (for each phase)
•	Unit tests pass in headless mode.
•	Determinism checks pass (fixed seeds match fixtures).
•	Minimal UX flow works end-to-end for the phase.
•	Schema/versioning updated if data formats changed.
•	Documentation updated (this roadmap, plus any dev notes needed to run/verify).

## Phase summary
| Phase | Name | Primary output | Acceptance flow |
|-------|------|----------------|-----------------|
| 0 | Foundations and guardrails | Repo structure, RNG wrapper, test runner, schema constants | Clone -> run tests -> green |
| 1 | Celestial object model | Data model + validation + serialization | Create object data -> save -> load -> identical |
| 2 | Object generators v1 | Seed/spec -> star/planet/moon/asteroid objects | Generate by seed -> export JSON -> matches fixture |
| 3 | Object viewer v1 | Viewer scene with inspect panel + save/load | Open app -> generate -> view -> save/load |
| 4 | Object editing v1 | Editable UI with validation + derived recalculation + undo | Edit fields -> derived updates -> undo -> save/load |
| 5 | Object rendering v2 (optional) | Improved shaders/materials + basic LOD | Viewer remains stable and responsive while rendering |
| 6 | Solar system generator + viewer | Random system generation + system viewer | Generate system -> browse bodies -> open object viewer |
| 7 | Solar system constraints (locks) | Constraint-based generation (min/max/exact) | Set constraints -> regenerate -> constraints satisfied |
| 8 | Solar system editing tools | Add/remove bodies, adjust orbits, recalc, undo | Apply edits -> recalc -> undo -> stable results |
| 9 | Galactic map v1 | Galaxy browser + lazy system generation + persistence | Browse galaxy -> open system -> edits persist |
| 10 | Galactic constraints and editing | Region rules + system placement edits | Apply region rules -> regenerate -> edits preserved |

## Phase details

### Phase 0: Foundations and guardrails
**Goal:**
Establish structure, determinism, and automated tests so progress is measurable and regressions are obvious.

**Deliverables:**
•	Project structure: domain / services / app / tests directories.
•	Deterministic RNG wrapper used by all generators (no direct calls to global random APIs).
•	Shared math/validation utilities (units, ranges, clamps).
•	Test framework wired (e.g., GUT or GdUnit4) with CLI run script.
•	Version constants: GENERATOR_VERSION and SCHEMA_VERSION.

**Tests:**
•	Headless test run returns correct exit code.
•	RNG determinism test: known seed produces known sequence.

**Acceptance criteria:**
•	Fresh clone -> one command runs tests successfully.

### Phase 1: Celestial object model
**Goal:**
Define editable celestial objects (stars, planets, moons, asteroids) as validated data with stable serialization.

**Deliverables:**
•	Core data model: CelestialBody with type-specific components (PhysicalProps, SurfaceProps, AtmosphereProps, RingProps).
•	Validation rules and error reporting for invalid values.
•	Serialization: JSON schema with schema_version and generator_version fields.
•	Load/save service for celestial objects.

**Tests:**
•	Validation invariants: mass > 0, radius > 0, optional fields consistent.
•	Serialization round-trip: object -> JSON -> object equals original.

**Acceptance criteria:**
•	Create an object in code, save to JSON, reload, and verify identical content.

### Phase 2: Object generators v1
**Goal:**
Generate celestial objects deterministically from (spec, seed).

**Deliverables:**
•	Generators for Star, Planet, Moon, and Asteroid with minimal, controllable specs.
•	Provenance stored on every object: seed used and spec snapshot.
•	Fixture export tool to write golden-master JSON for selected seeds.

**Tests:**
•	Golden-master regression: known seeds match saved fixtures.
•	Range tests: generated outputs always satisfy validation.

**Acceptance criteria:**
•	Generate each body type from a seed and produce stable JSON outputs.

### Phase 3: Object viewer v1
**Goal:**
View a single celestial object in-app and inspect its properties.

**Deliverables:**
•	ObjectViewer scene: camera controls, read-only inspector panel.
•	Generate/re-roll button (changes seed), and save/load JSON from disk.
•	Basic rendering: sphere mesh with seed-driven material parameters.

**Tests:**
•	Integration smoke test: viewer scene instantiates and runs one frame.
•	Invalid JSON load fails gracefully (no crash, user-readable error).

**Acceptance criteria:**
•	Open app -> generate object -> view -> save -> reload -> same result.

### Phase 4: Object editing v1
**Goal:**
Edit object properties in the program with validation, derived-value recalculation, and undo.

**Deliverables:**
•	Editable inspector controls for core fields (mass, radius, type-specific).
•	Derived values recalc after edits (e.g., density, surface gravity).
•	Field lock toggles (prepares for future constrained generation).
•	Undo/redo stack (command-based) for edits.

**Tests:**
•	Validation tests reject invalid edits and preserve last valid state.
•	Derived-value tests verify correct recalculation.
•	Undo/redo tests restore exact prior state.

**Acceptance criteria:**
•	Edit fields -> derived updates -> undo -> save/load preserves edits.

### Phase 5: Object rendering v2 (optional)
**Goal:**
Improve visuals without expanding simulation scope.

**Deliverables:**
•	Seed-driven material layers (continents/bands/noise) for planets.
•	Simple atmosphere shell and ring rendering (if enabled by props).
•	Basic LOD policy for meshes/material complexity.

**Tests:**
•	Performance budget check for common viewer actions.
•	Guard against NaN/inf material parameters.

**Acceptance criteria:**
•	Viewer remains stable and responsive while showing improved visuals.

### Phase 6: Solar system generator and viewer
**Goal:**
Randomly generate a solar system, display it, and inspect its bodies (no editing tools yet).

**Deliverables:**
•	SolarSystem data model: stars, planets, optional moons and belts (start minimal).
•	Orbital parameters: semi-major axis, eccentricity, inclination (start simple).
•	SolarSystemGenerator.generate(spec, rng) with minimal spec (ranges only).
•	SolarSystemViewer: 2D map or lightweight 3D view; select body opens ObjectViewer.

**Tests:**
•	Determinism: same seed/spec -> identical system layout.
•	Orbital invariants: distances positive; no planet inside star radius; ecc in [0, 1).
•	Golden-master fixtures for a few seeds.

**Acceptance criteria:**
•	Generate system -> browse bodies -> open object viewer reliably.

### Phase 7: Solar system constraints (locks)
**Goal:**
Support generation constraints such as exact/min/max star count and minimum planet count.

**Deliverables:**
•	SystemConstraints model (exact/min/max counts, must-include templates later).
•	Constraint-aware generation with bounded retries and clear failure errors.
•	UI for constraints and regenerate.

**Tests:**
•	Constraint satisfaction tests.
•	Impossible constraints fail fast and provide actionable errors.

**Acceptance criteria:**
•	Set constraints -> regenerate -> constraints are satisfied (or cleanly rejected).

### Phase 8: Solar system editing tools
**Goal:**
Allow edits like adding a star at distance X, adjusting orbits, and recalculating the system.

**Deliverables:**
•	Command-based edit operations: add/remove body, adjust orbit, recalc.
•	Safety rails: prevent orbit overlap or auto-resolve.
•	System-level undo/redo.

**Tests:**
•	Command apply/undo tests.
•	Recalc determinism: same system + same edit ops -> same result.

**Acceptance criteria:**
•	Apply edits -> recalc -> undo works and system remains valid.

### Phase 9: Galactic map v1
**Goal:**
Add a galactic container that browses and lazily generates systems without regenerating edits.

**Deliverables:**
•	Galaxy model: sectors/regions and seeded system placement.
•	Lazy generation (generate on demand, not all at once).
•	Persistence of edits as deltas/patches applied on top of seeded generation.
•	Galaxy viewer: browse region/sector -> open system.

**Tests:**
•	Determinism across lazy generation.
•	Patch application tests for persistence.

**Acceptance criteria:**
•	Browse galaxy -> open system -> edits persist across sessions.

### Phase 10: Galactic constraints and editing
**Goal:**
Add region-level rules and editing tools for system placement and density.

**Deliverables:**
•	Region constraints (e.g., higher binary fraction, denser core).
•	Editing tools: add/remove system, adjust density, regenerate region.
•	Persistence and backward compatibility for saved galaxy data.

**Tests:**
•	Constraint + persistence tests.
•	Backward compatibility test for at least one prior schema version.

**Acceptance criteria:**
•	Apply region rules -> regenerate -> edits remain stable and preserved.

## Backlog discipline
Any feature request that does not directly support the current phase must be recorded in BACKLOG.md and deferred. Each backlog item should include: title, why it matters, target phase, and complexity (S/M/L).
