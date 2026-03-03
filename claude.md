# StarGen (Godot 4.x, C#)

## What we are building
StarGen is a deterministic procedural generator + viewer for:
1) Individual celestial objects (stars, planets, moons, asteroids) — viewable (editing deferred to later)
2) Solar systems — generate + view first, then constraints, then editing tools
3) Galactic scale — data model done (Galaxy, Sector, GalaxyStar, lazy generation); tools/polish deferred

Guiding principles:
- DRY + SOLID
- Determinism and test coverage as non-negotiables
- Finish vertical slices, do not balloon scope
- **Scientific realism is the default:** Generation is driven by scientific documentation and established ranges by default. The eventual goal is to expose the assumption levers that drive generation so users can adjust them to fit their desired outcome (e.g. hard sci-fi vs space opera vs grim frontier).

Recent refactor:
- The core codebase has been refactored to C#; new work should favor C# implementations that fit the existing architecture.
- Any regressions or errors introduced by this refactor are **high-priority fixes** and should be addressed before adding new features.

---

## Roadmap (efforts)

Work is organized by **async efforts** in `Docs/Roadmap.md`. Contributors pick an effort and work against master.

**Efforts:** Solar system constraints; System viewer rendering improvements; Object editing; Object rendering v2; Galactic generator refinement; Solar system tools; Galactic tools; Galactic polish; Jump lanes optimization and polish; Code quality & simplifications; Population detail (civilisation/regime). Galaxy data model & lazy generation is complete. See Docs/Roadmap.md for the full table (name, summary, gates) and Completed efforts.

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
- **When creating a new test script:** Register it so it runs in both headless and in-editor flows. For **C# tests:** add to the appropriate DotNet test suite and ensure they run via the headless harness and test scene. For **GDScript tests:** add to `Tests/RunTestsHeadless.gd` and `Tests/TestScene.gd` (`_test_scripts` in each). If tests depend on domain types that need preloading (e.g. population enums), add the required preload/deps in both runners.
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
- Files: PascalCase (e.g., `CelestialBody.cs`, `StarGenerator.cs`)
- Structural folders under `src/`: snake_case (e.g., `domain/`, `services/`, `app/`)
- Top-level folders: PascalCase (e.g., `Tests/`, `Docs/`, `Resources/`)
- C# methods: PascalCase (e.g., `GenerateStar()`, `ValidateSpec()`)
- C# properties: PascalCase (e.g., `ObjectSeed`, `PhysicalProps`)
- C# private fields: _camelCase when used for backing state (e.g., `_objectSeed`, `_physicalProps`)
- Variables: descriptive names; avoid shadowing engine concepts (`transform`, `position`, `global_position`)
- Constants: PascalCase or UPPER_SNAKE_CASE (e.g., `MaxRadius`, `DefaultTemperature`)

### Typing
- Explicit types everywhere. No `var` for non-obvious types in C#; type all parameters and return values.
- Type all variables. No inference that hides types.

### Documentation
- XML doc blocks above every public method: purpose, parameters, returns.
- For `[Export]` (Godot) or `[SerializeField]`-style members, describe what they are above the declaration.
- Inside methods, comment **why**, not what.

### Script size
- Target ~10 methods per class (soft limit).
- If exceeding 12–15 methods, refactor or split by responsibility. C#: fields, constructors, public methods, then private helpers.

### C# and Cursor rules compliance
- **No ternary operators** in committed code; use explicit `if` blocks.
- **No silent fallbacks;** fail loudly with descriptive errors (e.g. `ArgumentException`, `InvalidOperationException`, Godot `push_error`).
- **Engine-first configuration:** Use Inspector, Input Map, layers/masks, and scene configuration instead of hard-coding where the editor can do it. Prefer `[Export]` and cached node references (`@onready`-style in C# via `GetNode` at ready).
- **Signals/events over polling.** Use Godot signals or C# events; single source of truth for shared data.
- **Explicit typing:** Avoid `var` when the type is not obvious; type all parameters and return values.
- **Warnings as errors:** Code must build cleanly; fix or document any compiler/analyzer warnings.

---

**Object editing** is an outstanding effort in the roadmap. The viewer currently supports generation, viewing, and save/load only.

---

## Prototypes (beyond core scope)

**Core scope (no prototype required):** Galaxy, systems, stars, planets, moons, asteroids — their **data generation** and **object rendering** (appearance: shaders, materials, BodyRenderer, StarShaderParams, GasGiantShaderParams, TerrestrialShaderParams, etc.) are part of the main program. So are system/galaxy generation, population framework, stations, and jump lanes (see Roadmap).

**When a prototype is required:** Any feature or tool that extends functionality **beyond** that core (e.g. ecology simulation, climate model, causality inspector, retcon manager, new RPG export format) must be developed as a **prototype** first — typically under `Concepts/` or a dedicated branch — and must satisfy **migration gates** before being folded into main.

**Prototype lifecycle:**
- Prototypes may start as anything: any language, any stack, any structure. No gate at birth.
- Before code is merged into main, the prototype must meet the gates below. Track status so contributors know where each prototype stands.
- When a prototype meets all migration gates, it can be added as an effort (or part of one) in Docs/Roadmap.md; once folded into main, remove it from Concepts/ and from the concept expansion menu in Concepts/Additions.md.

**Migration gates (all required before fold-in):**
1. **Determinism:** Any generation or simulation uses an injected RNG; same seed + same inputs → identical outputs; provenance (seed, version, spec) is stored where applicable.
2. **Architecture:** Logic fits the layers (domain pure, services for IO/caching, app for UI/rendering). No domain → app/service dependencies.
3. **No global state:** Generators and simulators do not rely on global mutable state; they accept (spec, rng, context) and return data.
4. **Tests:** Non-trivial logic has unit tests; determinism tests where applicable; tests run in the headless harness and are registered (C# suites and/or `RunTestsHeadless.gd` / `TestScene.gd`) if they depend on domain types.
5. **Documentation:** Purpose and integration point are documented; Docs/ProjectStructure.md and Docs/Roadmap.md updated if new modules or efforts are added.
6. **Scientific realism and export/mapping documentation (optional but strongly recommended):** Scientific realism is the default; generation details should be driven by or referenced against scientific documentation (literature, established ranges, physical constraints). Prototypes should document which assumptions drive generation so they can eventually be exposed as user-tunable levers. Because output may be exported to various systems (TTRPGs, other engines, maps), the details required for mapping and export should be considered and at least suggested in the documentation — e.g. which fields or conventions support which export targets, and what mapping or schema notes exist. This step is not a hard gate but is strongly recommended before fold-in.
7. **Fold-in branch:** A branch exists that **attempts to fold** the prototype into main (integrate with existing domain/services/app, same RNG and save/load contracts). The branch need not be perfect, but it must demonstrate that the prototype can be wired in without breaking determinism, architecture, or tests. Review and merge only after gates 1–5 are satisfied (and gate 6 where feasible) and the fold-in branch is deemed mergeable.

**Status annotation:** Where prototypes are listed (e.g. Concepts/Additions.md, Roadmap, or a dedicated prototype index), annotate each with a status such as: **Exploration** (no gates yet), **Gates in progress** (meeting determinism/architecture/tests/docs), **Fold-in branch open** (branch exists, integration in progress), **Merged** (folded into main; prototype can be retired or kept as reference).

**Housekeeping when a prototype is folded in:** Once a prototype has been fully adapted for use in the main program, it is a StarGen tool proper — not a prototype. Then: (1) **Remove it from the prototype location** — delete or relocate the prototype code from `Concepts/` or `src/app/prototypes/` so the repo no longer carries the duplicate. (2) **Remove it from prototype documentation** — drop it from the prototype/concept tables and lists in Concepts/Additions.md, Roadmap, and any other docs that list “Prototype TODO” or concept suggestions; do not list it as a prototype once it lives in main. Keep documentation accurate so “prototype” means “not yet in main.”

**Claude:** When proposing a new feature that goes beyond galaxy/systems/stars/planets (and their current rendering and population/station/jump-lane scope), treat it as a prototype: add it to the concept/tool lists as Prototype TODO, and do not implement it in main until a prototype has met the migration gates and a fold-in branch has been created and reviewed.

---

## Communication protocol
- Before implementing: confirm effort alignment (see Docs/Roadmap.md)
- If feature is out-of-scope: add as a new effort to Docs/Roadmap.md, do not implement
- All code changes: include tests and acceptance criteria
- Breaking changes: document migration path

## Pre-commit discipline
- **Before any commit:** Update all affected documentation. This includes Docs/ProjectStructure.md if files or folders were added, removed, or moved; Docs/Roadmap.md if efforts changed; and any other Docs that the change touches. Keep documentation in sync with the codebase.
