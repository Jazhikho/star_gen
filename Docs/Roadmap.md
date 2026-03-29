# StarGen Roadmap

Godot 4.x - C#-first runtime | DRY + SOLID | Deterministic generation with test gates

## Version history

Release notes and version summaries are in the [README](../README.md#version-history). 

## Overview

•	Determinism is non-negotiable: same seed + same inputs must produce identical outputs.
•	Keep domain logic pure (no scene tree / Nodes / file I/O inside domain).
•	UI elements should not exist in scripting if it is possible to build them in .tscn
•	Ship tests with features. Golden-master fixtures cover regression for known seeds.
•	Prefer composition over inheritance. Small services and data components over "manager" classes.
•	Generation sets initial conditions; simulation produces emergent structure. Studio-driven generation should define the starting state top down, while tool-driven simulations should evolve outcomes bottom up from local conditions.
•	Any feature request that does not directly support an active effort is added as a new effort in this roadmap.

## Definition of Done (per effort)

•	Unit tests pass in headless mode.
•	Determinism checks pass where applicable (fixed seeds match fixtures).
•	Minimal UX flow works end-to-end for the effort.
•	Schema/versioning updated if data formats changed.
•	Documentation updated (this roadmap, plus any dev notes needed to run/verify).
•	If work was significantly AI-assisted, disclosure/provenance is updated in `AI-Provenance-Log.md`.
•	AI-assisted scientific, realism, or citation-related claims are human-verified against reviewed sources before acceptance.
•	Culture-, civilisation-, religion-, language-, or species-related outputs receive explicit human audit before merge or release.

---

## Version v0.9 specifications

### Core direction for v0.9

- Migrate existing UI elements from code into `.tscn` scenes as far as possible.
- Keep scripts behavior-focused; if an element does not require runtime logic, it must be scene-defined.
- Enforce scientific realism across the full generation pipeline, aligned with `Docs/galactic_formation.md` from top-level galaxy generation through downstream object outcomes.
- Remove and hand off concept-heavy scope to MythicWorldGen based on the concept lists under `Concepts/Additions.md`.

### UI migration requirement (`.tscn` first)

- Audit all current UI created or configured in C# and move layout/appearance/static configuration into `.tscn`.
- Reserve scripting for dynamic behavior, state transitions, validation, and event handling only.
- Prefer engine/editor configuration over hardcoded UI construction in scripts.
- Treat this as a release gate for v0.9: UI that can be authored in scene files should not remain code-built.

### Scientific realism requirement (start to finish)

- All generation stages in StarGen must follow the realism framework and constraints documented in `Docs/galactic_formation.md`.
- Where assumptions are uncertain or contested, use explicit tunable parameters and document defaults/ranges.
- Keep realism consistency across galaxy, stellar, planetary, and life-distribution outputs rather than applying realism to isolated tiers only.
- Any realism-related changes require documentation updates and human verification against reviewed sources before acceptance.

### Active effort: Stellar population expansion (`0.8.4.0`)

- Expand the stellar generator beyond main-sequence OBAFGKM output so it can also produce practical, research-backed brown dwarfs, evolved stars, white dwarfs, and more realistic multi-star architectures.
- Keep the implementation generation-focused rather than simulation-focused: use deterministic approximations grounded in reviewed IMF, isochrone, white-dwarf, brown-dwarf, and multiplicity literature.
- Carry the expanded stellar offerings through galaxy generation, standalone system generation, viewer readouts, and help surfaces so the same stellar model is used everywhere.
- Where multiple research-backed model families exist, expose the supported choices in Galaxy Studio and System Studio rather than hardcoding one hidden assumption.
- Treat unsupported end states such as neutron stars and black holes as out of scope for this effort unless new reviewed sources and explicit user direction expand the model further.
- Add deterministic and statistical tests that verify the expanded stellar population behaves within the expected scientific bands and that new spectral and stellar-type outputs survive serialization, rendering hints, and system generation.

### Scope split: StarGen vs MythicWorldGen

- StarGen mainline scope narrows to generation functions and generation-time modification workflows.
- Concept systems designated for MythicWorldGen are removed from StarGen scope and tracked as moved in `Concepts/Additions.md`.
- The StarGen release line should avoid carrying deep concept/simulation systems targeted for MythicWorldGen.

### Branch strategy and persistence split

- A dedicated export branch will contain save/load/export conditions for generated objects intended for MythicWorldGen import.
- This export branch is not part of the standard StarGen release build.
- Main branch retains generation capabilities only; save, load, and export functionality is removed from mainline.
- In mainline builds, user modifications are not persistable.

### Version tag policy for this split

- Main branch versions append the suffix `d`.
- Export branch versions append the suffix `e`.
- Keep suffixes, docs, and user-facing version labels synchronized with each branch's actual capabilities.
