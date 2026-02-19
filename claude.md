# StarGen (Godot 4.x, GDScript)

## What we are building
StarGen is a deterministic procedural generator + viewer for:
1) Individual celestial objects (stars, planets, moons, asteroids) — viewable (editing deferred to later)
2) Solar systems — generate + view first, then constraints, then editing tools
3) Galactic scale — data model done (Galaxy, Sector, GalaxyStar, lazy generation); tools/polish deferred

Guiding principles:
- DRY + SOLID
- Determinism and test coverage as non-negotiables
- Finish vertical slices, do not balloon scope

---

## Roadmap (efforts)

Work is organized by **async efforts** in `Docs/Roadmap.md`. Contributors pick an effort and work against master.

**Efforts:** Solar system constraints; System viewer rendering improvements; Object editing; Object rendering v2; Galactic generator refinement; Solar system tools; Galactic tools; Galactic polish; Jump lanes integration; Code quality & simplifications; Population detail (civilisation/regime). Galaxy data model & lazy generation is complete. See Docs/Roadmap.md for the full table (name, summary, gates) and Completed efforts.

**Claude:** When asked for new features, map them to the relevant effort in the roadmap. If out-of-scope for all existing efforts, add a new effort to the roadmap and do NOT implement until that effort is picked.

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
- **When creating a new test script:** Add it to **both** `Tests/RunTestsHeadless.gd` and `Tests/TestScene.gd` (the `_test_scripts` array in each). The headless runner and the test scene must run the same set of tests. If the new tests depend on domain types that need preloading (e.g. population enums), add the required preload in both runners (e.g. `PopulationDeps.gd` in RunTestsHeadless and TestScene).
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

## Effort discipline
Proposed changes that do not fit any existing effort are added as a **new effort** in Docs/Roadmap.md. Each new effort should include: name, summary, gates (if any), deliverables, tests, and acceptance criteria. Do not implement until that effort is explicitly picked by a contributor.

---

## Population, stations, and jump lanes (on master)
- The **population framework** (planets: native populations, colonies, history), **outposts and space stations**, and **jump lanes** are on master. See Docs/Roadmap.md for scope.
- Follow the same architecture (domain/services/app), typing, and doc conventions. Population and station logic live in `src/domain/population/`; jump lanes in `src/domain/jumplanes/`.

---

## Code style & conventions
### File organization
- Domain logic: `src/domain/`
- Services (IO, caching): `src/services/`
- App layer (scenes, UI): `src/app/` (system viewer includes SystemViewer, SystemDisplayLayout, OrbitRenderer, SystemCameraController, SystemBodyNode, SystemInspectorPanel)
- Shared resources: `Resources/`
- Test scenes: `Tests/`
- Documentation: `Docs/`

### Naming
- Files: PascalCase (e.g., `CelestialObject.gd`, `StarGenerator.gd`)
- Structural folders under `src/`: snake_case (e.g., `domain/`, `services/`, `app/`)
- Top-level folders: PascalCase (e.g., `Tests/`, `Docs/`, `Resources/`)
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

**Object editing** is an outstanding effort in the roadmap. The viewer currently supports generation, viewing, and save/load only.

---

## Communication protocol
- Before implementing: confirm effort alignment (see Docs/Roadmap.md)
- If feature is out-of-scope: add as a new effort to Docs/Roadmap.md, do not implement
- All code changes: include tests and acceptance criteria
- Breaking changes: document migration path

## Pre-commit discipline
- **Before any commit:** Update all affected documentation. This includes Docs/ProjectStructure.md if files or folders were added, removed, or moved; Docs/Roadmap.md if efforts changed; and any other Docs that the change touches. Keep documentation in sync with the codebase.
