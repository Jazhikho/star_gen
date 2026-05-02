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

## Version v0.10 specifications

### Core direction for v0.10

- Migrate existing UI elements from code into `.tscn` scenes as far as possible.
- Keep scripts behavior-focused; if an element does not require runtime logic, it must be scene-defined.
- Enforce scientific realism across the full generation pipeline, aligned with `Docs/galactic_formation.md` from top-level galaxy generation through downstream object outcomes.
- Remove and hand off concept-heavy scope to MythicWorldGen based on the concept lists under `Concepts/Additions.md`.

### UI migration requirement (`.tscn` first)

- Audit all current UI created or configured in C# and move layout/appearance/static configuration into `.tscn`.
- Reserve scripting for dynamic behavior, state transitions, validation, and event handling only.
- Prefer engine/editor configuration over hardcoded UI construction in scripts.
- Treat this as a release gate: UI that can be authored in scene files should not remain code-built.

### Scientific realism requirement (start to finish)

- All generation stages in StarGen must follow the realism framework and constraints documented in `Docs/galactic_formation.md`.
- Where assumptions are uncertain or contested, use explicit tunable parameters and document defaults/ranges.
- Keep realism consistency across galaxy, stellar, planetary, and life-distribution outputs rather than applying realism to isolated tiers only.
- Any realism-related changes require documentation updates and human verification against reviewed sources before acceptance.

### Active effort: Planetary retrofit and downstream environmental constraints (`0.8.7.0`)

- Retrofit the existing planet-generation spine instead of replacing it, keeping `GalaxyConfig -> SolarSystemSpec -> SystemPlanetGenerator -> PlanetSpec -> PlanetGenerator` intact while threading a shared aggregate planetary-formation profile through the upstream tiers.
- Keep aggregate planetary-formation assumptions at Galaxy Studio and System Studio only, exposing model choices such as mass-radius handling, envelope loss, gas-giant formation, metallicity coupling, rogue-planet allowance, moon-formation bias, and outer-system small-body bias with plain-language help.
- Keep Object Studio limited to direct single-planet controls, including orbit mode, class bias, composition bias, envelope override, volatile richness, hydrosphere tendency, and moon-bundle settings, without surfacing disk- or system-level formation knobs there.
- Derive a deterministic `PlanetarySystemState` once per system so orbit-slot weighting and broad class preconditions can respond to snow-line position, solid/gas budget surrogates, escape pressure, metallicity enrichment, migration strength, and impact stirring without rewriting the generator into a simulation.
- Store enough formation trace and provenance on generated planets to explain why a world became rocky, water-rich, sub-Neptune-like, gas-giant-like, or stripped-core-like, while preserving current save/load compatibility through additive fields and defaults.
- Keep this pass grounded in `Sources/Texts/planets.md` as a deterministic implementation spec, not a mandate to mirror every latent variable or rewrite the whole planetary stack into a full formation simulator.
- Use the aggregate planetary state to drive non-cosmetic downstream consequences, especially atmosphere retention, volatile delivery, moon architecture, outer-belt and comet-leaning small-body placement, and biosphere support.
- Add flux-, habitable-zone-, XUV-, and tidal-heating-aware environment scoring so habitability and native-biology outcomes follow system context rather than only standalone planet readouts.
- Calibrate these constraints against reviewed literature for planet demographics, moon formation, asteroid/comet structure, and habitability windows, and keep the implementation additive instead of rewriting the current generator stack.

### Active effort: End-to-end scientific grounding audit (`0.9.1.0`)

- Promote the internal audit inventory into tracked repo docs and use it as the source of truth for generator-internal numeric constants that materially shape output.
- Close unsupported science claims in staged passes: source acquisition first, bibliography and `Sources/Texts` notes second, inline APA-style code citations third, with human verification required before any claim is treated as fully grounded.
- Keep this effort traceability-focused rather than behavior-focused: unless reviewed literature clearly contradicts a narrow coefficient, runtime generator formulas stay stable and only their grounding comments, audit status, and source trail change.
- First-pass targets are the galaxy bar-strength and elliptical-structure claims, the compact stellar-lifetime exponents, the planetary migration framework reference, and the sulfur-chemistry support window.
- Deliverables for each pass are explicit status updates in `Docs/EndToEndScienceAudit.md`, corresponding bibliography and `Sources/Texts` entries, inline code comments that distinguish sourced framework from StarGen tuning, provenance updates, and a quality regression that checks the tracked audit contract.
- AI-assisted source summaries or inline citations added during this effort must remain marked as pending human verification until a human reviews the underlying source material and accepts the claim.

### Planned effort: RPG compatibility overrides

- Expand the current `Generation Overrides` seam beyond the existing Traveller-leaning clean-room mode into a small set of explicitly supported RPG compatibility profiles.
- Keep this layer compatibility-oriented rather than lore-oriented: it should bias generated outputs toward usable mainworlds, settlement structures, readouts, and constraints for a target game family without copying protected setting material or implying endorsement.
- Use only systems with reviewed source and licensing support in the repo for first-class built-in overrides. Current first-wave candidates are:
  - Cepheus Engine
  - Ironsworn: Starforged
  - Starfinder, but only as a clean-room compatibility profile unless and until more ORC-designated primary material is reviewed
- Keep Traveller support clean-room unless a separate human-reviewed licensing path broadens what can be shipped.
- Avoid shipping unsupported built-in overrides for Stars Without Number or GURPS until a clearer reviewed rights basis exists.
- Build the override layer on top of the existing `GenerationUseCaseSettings` / `RulesetMode` seam so the same controls can apply consistently in Galaxy Studio, System Studio, and Object Studio.
- Override profiles should materially change generation:
  - settlement density and mainworld policy
  - atmosphere and hydrographics permissiveness
  - life and civilization forcing or relaxation
  - starport or hub-world bias
  - debris, frontier, and hazard pressure
  - export and readout mappings
- Do not reproduce rules text or proprietary tables verbatim unless the reviewed source explicitly permits it.

### Recently completed effort: Planetary and minor-body taxonomy expansion (`0.8.5.0`)

- Expanded Object Studio so it behaves as a true single-object authoring surface, showing only the controls that match the currently selected object type instead of exposing unrelated presets and parameters all at once.
- Removed standalone moon generation from the Object Studio top-level picker and moved moon generation under planets, with context-sensitive moon controls that only appear when moon generation is enabled.
- Added comet generation as a real celestial-body path, including generation, rendering, save/load metadata, viewer display, and Object Studio controls.
- Broadened asteroid authoring beyond type-only selection by exposing practical orbit-band, density, and albedo shaping controls that materially change the generated result.
- Expanded planet object controls toward a richer profile-driven surface so users can shape outcomes such as atmosphere, ocean/ice coverage, albedo, volcanism, and moon generation more directly.
- Added deterministic and non-visual integration coverage proving the Object Studio context gating, comet support, expanded asteroid tuning, and planet moon controls behave correctly.

### Recently completed effort: Stellar population expansion (`0.8.4.0`)

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

### Edition strategy and persistence split

- The standard public build is `0.10`, which keeps generation and viewing available but leaves save/load/export disabled.
- The export-enabled paid build also displays `0.10`, and restores the persistence flows needed for authoring and downstream artifact handling.
- Both editions should come from the same codebase and differ through explicit build-channel capability gating rather than hidden buttons alone.
- In the public build, user modifications are intentionally non-persistable.

### Version tag policy for this split

- User-facing labels use the plain approved release version, currently `0.10`.
- Release artifacts identify export-enabled packages with an `-export` artifact label rather than an in-app version suffix.
- Keep docs, release tooling, packaging labels, and user-facing version labels synchronized with each build's actual capabilities.
