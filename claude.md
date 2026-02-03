# StarGen (Godot 4.x, GDScript)

## What we are building
StarGen is a deterministic procedural generator + viewer for:
1) Individual celestial objects (stars, planets, moons, asteroids) — viewable (editing deferred to later)
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
Phase 7: Galactic map v1
Phase 8: Solar system generator refinement
Phase 9: Galactic generator refinement
Phase 10: Solar system tools
Phase 11: Galactic tools
Phase 12: Solar system polish
Phase 13: Galactic polish

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

**Phase 4: Object editing v1** (Deferred - moved to later phase)
**Note:** Editing functionality has been deferred. The viewer currently supports generation, viewing, and save/load only. Editing will be implemented in a future phase after core viewing and system generation features are complete.

---

## Communication protocol
- Before implementing: confirm phase alignment
- If feature is out-of-scope: add to BACKLOG.md, do not implement
- All code changes: include tests and acceptance criteria
- Breaking changes: document migration path
