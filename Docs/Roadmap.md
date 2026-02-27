# StarGen Roadmap

Godot 4.x - GDScript | DRY + SOLID | Deterministic generation with test gates

## Overview

This roadmap builds StarGen in three layers: (1) viewable celestial objects (editing deferred), (2) solar systems, then (3) galactic scale. Work is organized as **efforts** that can be pursued in parallel where dependencies allow.

## Guiding principles

•	Determinism is non-negotiable: same seed + same inputs must produce identical outputs.
•	Keep domain logic pure (no scene tree / Nodes / file I/O inside domain).
•	Ship tests with features. Golden-master fixtures cover regression for known seeds.
•	Prefer composition over inheritance. Small services and data components over "manager" classes.
•	Any feature request that does not directly support an active effort is added as a new effort in this roadmap.

## Definition of Done (per effort)

•	Unit tests pass in headless mode.
•	Determinism checks pass where applicable (fixed seeds match fixtures).
•	Minimal UX flow works end-to-end for the effort.
•	Schema/versioning updated if data formats changed.
•	Documentation updated (this roadmap, plus any dev notes needed to run/verify).

---

## Efforts (remaining work)

Contributors pick an effort and work against master. Efforts can run in parallel unless gated. **Gating:** An effort with a gate should start only after its gate(s) are done; gates exist so the dependent work has the right data model or behaviour to build on. **Branch:** When work on an effort is done in a branch (instead of directly on master), put the branch name in the Branch column so others can find it.

| Name | Summary | Gates | Branch |
|------|---------|-------|--------|
| Solar system constraints | Constraint-based generation (min/max/exact counts, stellar locks, resonances), UI | — | `feature/constraints-belts-rotation` |
| System viewer rendering improvements | Directional lighting, axial tilt, 1 day = 1 s, asteroid belt torus | — | `feature/constraints-belts-rotation` |
| Object editing | Editable inspector, derived-value recalc, undo/redo | — | — |
| Object rendering v2 | Oblateness, aurora, LOD, seed-driven materials | — | — |
| Galactic generator refinement | Region constraints, region rules, constraint-based placement | — | — |
| Solar system tools | Add/remove bodies, adjust orbits, recalc, system-level undo | — | — |
| Galactic tools | System placement edits, region editing, galactic undo | Galactic generator refinement | — |
| Galactic polish | Galaxy save/load UI polish, backward compat, performance | Galactic tools | — |
| Jump lanes optimization and polish | Optimize and polish jump-lane rendering in galaxy viewer; population data; line/orphan visuals | — | — |
| Code quality & simplifications | TODOs, placeholder replacements, simplified formulas to redo | — | — |
| Population detail (civilisation/regime) | Enrich population with tech level, regime type, and transitions; align with Integration and History Generator concepts | — | — |
| Traveller Use Case | Generate systems/planets/populations usable in standard Traveller play: UWP codes, Traveller rules where applicable, use-case toggle | — | `effort/traveller-use-case` |

---

## Effort details

### Solar system constraints

**Goal:** Improve generation quality with constraint-based generation (min/max/exact counts, orbital resonances).

**Deliverables:**
•	SystemConstraints model (exact/min/max counts for planets, moons, belts); must-include templates deferred.
•	Stellar locks: lock stellar age (affects planet composition/surface age); lock binary star separation (constrains orbits).
•	Orbital resonances: force resonant ratios; use `OrbitalMechanics.calculate_resonance_spacing` / `get_common_resonance_ratios`.
•	Constraint-aware generation with bounded retries; clear failure errors when constraints are impossible; determinism preserved.
•	UI: constraints panel and regenerate flow in system viewer.

**Tests:** Constraint satisfaction; stellar age lock affects planetary properties; orbital resonance lock; impossible constraints fail fast with actionable errors.

**Acceptance:** Set constraints → regenerate → constraints are satisfied (or cleanly rejected).

---

### System viewer rendering improvements

**Goal:** Fix system viewer rendering so bodies are lit by the star(s), show correct axial tilt and rotation/orbit timing, and render asteroid belts as torus shapes with major asteroids.

**Deliverables:**
•	**Directional lighting:** Planets and moons lit from the star(s) in the system, not uniformly as in the object viewer.
•	**Axial tilt:** Apply `physical.axial_tilt_deg` so body tilt is visible in the system view.
•	**Rotation and orbit speed:** Scale so that ~1 day = ~1 second for both rotation and orbital motion.
•	**Asteroid belt rendering:** Render belts as a flat torus at the belt’s orbital distance (not a sphere on a path); place major asteroids from the belt data within that torus.

**Tests:** Lighting direction from star position; axial tilt applied; rotation/orbit period scaling; belt torus and asteroid placement.

**Acceptance:** In the system viewer, bodies are lit from the star direction, show correct tilt, animate at ~1 day = 1 s, and asteroid belts appear as torus with asteroids inside.

---

### Object editing

**Goal:** Edit object properties in the program with validation, derived-value recalculation, and undo.

**Note:** EditDialog.gd/.tscn exist in `src/app/viewer/` as a deferred preview; integrate when this effort is picked.

**Deliverables:**
•	Editable inspector controls for core fields (mass, radius, type-specific).
•	Derived values recalc after edits (e.g., density, surface gravity).
•	Recalculate derived stellar properties when mass/radius change (luminosity, spectral class).
•	Validate ring gaps don't overlap; update tidal locking when orbital distance changes.
•	Field lock toggles (prepares for constrained generation).
•	Undo/redo stack (command-based) for edits.

**Tests:** Validation rejects invalid edits; derived-value recalc; stellar recalculation; ring gap validation; undo/redo restores prior state.

**Acceptance:** Edit fields → derived updates → undo → save/load preserves edits.

---

### Object rendering v2

**Goal:** Improve visuals without expanding simulation scope.

**Deliverables:**
•	Seed-driven material layers (continents/bands/noise) for planets.
•	Oblateness rendering: gas giant bulge from rotation/density.
•	Atmospheric scale height → visual thickness rendering.
•	Multiple cloud layers based on atmospheric properties.
•	Aurora effects from magnetic field strength and composition.
•	Ring shadow casting on planet surfaces.
•	Basic LOD policy for meshes/material complexity.

**Tests:** Performance budget check; guard against NaN/inf material params; oblateness; aurora intensity mapping.

**Acceptance:** Viewer remains stable and responsive while showing improved visuals.

---

### Galactic generator refinement

**Goal:** Improve galactic generation with region-level constraints and rules.

**Deliverables:**
•	Region constraints (e.g., higher binary fraction, denser core).
•	Constraint-based system placement.
•	Region-level generation rules.
•	UI for constraints and regenerate.

**Tests:** Constraint satisfaction; region rule application; determinism across constraint changes.

**Acceptance:** Apply region rules → regenerate → constraints are satisfied.

---

### Solar system tools

**Goal:** Add editing tools for modifying systems: add/remove bodies, adjust orbits, recalculate.

**Deliverables:**
•	Command-based edit operations: add/remove body, adjust orbit, recalc.
•	Propagate stellar luminosity changes to all planet temperatures.
•	Revalidate orbits when star mass changes (recalculate orbital periods).
•	Cascade deletion: remove moons when planet deleted, remove planets when star deleted.
•	Safety rails: prevent orbit overlap or auto-resolve.
•	System-level undo/redo.

**Tests:** Command apply/undo; luminosity propagation; mass change recalculation; cascade deletion; recalc determinism.

**Acceptance:** Apply edits → recalc → undo works and system remains valid.

---

### Galactic tools

**Goal:** Add editing tools for modifying galactic structure: add/remove systems, adjust density.

**Gates:** Galactic generator refinement (region rules and data model).

**Deliverables:**
•	Editing tools: add/remove system, adjust density, regenerate region.
•	System placement editing.
•	Region editing (modify constraints, regenerate).
•	Galactic-level undo/redo.

**Tests:** Edit operation tests; persistence for edits; undo/redo tests.

**Acceptance:** Apply edits → recalc → undo works and galaxy remains valid.

---

### Galactic polish

**Goal:** Complete the galactic viewer with save/load polish, backward compatibility, and optimizations.

**Gates:** Galactic tools. Polish (including backward compatibility for saved galaxy data) is most useful once galactic editing exists so the schema and UX are stable.

**Deliverables:**
•	Galaxy save/load UI polish (file dialogs, save/load buttons if not yet present).
•	Backward compatibility for saved galaxy data (at least one prior schema version).
•	Performance optimizations (lazy loading, caching).
•	UI polish (tooltips, keyboard shortcuts, status messages).
•	Final testing and bug fixes.

**Tests:** Save/load round-trip; backward compatibility; performance benchmarks.

**Acceptance:** Save galaxy → load galaxy → identical galaxy displayed. Edits remain stable and preserved.

---

### Jump lanes optimization and polish

**Goal:** Optimize and polish jump-lane display in the galaxy viewer: performance, visual clarity, and UX for range selection (subsector vs sector), line colors (green/yellow/orange/red), and orphan highlighting.

**Deliverables:**
•	Population data wiring: single entry point returning `JumpLaneRegion` for current subsector/sector (placeholder population acceptable until real data).
•	User controls: subsector vs sector range; Run jump-lanes button (or equivalent).
•	Visual polish: draw connections (green/yellow/orange/red); highlight orphans (red); performance and LOD if needed.
•	Integration test: load sector, run tool, assert connection/orphan counts.
•	Docs: where jump-lanes lives (menu/panel/shortcut) and how to use.

**Acceptance:** User can select subsector or sector and run the tool. Lines appear with correct colors; orphans appear in red; rendering is performant and polished.

---

### Code quality & simplifications

**Goal:** Address TODOs in code, replace placeholders, and redo simplified formulas where accuracy or correctness matters.

**Scope (from codebase survey):**
•	**TODOs:** SpaceStation/Colony — civilization reference placeholder (replace when Civilization model exists).
•	**Simplifications to review/redo:** OrbitalMechanics (stability zone simplified check); OrbitSlotGenerator (companion positions, host at origin); OrbitTable (tidal locking formula from Peale 1977); PlanetSurfaceGenerator (pressure/boiling point); PlanetPhysicalGenerator (tidal heating); MoonPhysicalGenerator (tidal heating); RingSystemGenerator (resonance-based gaps); StationPlacementRules (simplified system context); SystemDisplayLayout (logarithmic vs astronomical scaling — document or revise).
•	**General:** Audit for other "simplified" or "placeholder" comments; consolidate or document design decisions.

**Deliverables:**
•	Resolve or document each TODO (either implement or add as a new effort with justification).
•	Audit simplified formulas: either improve accuracy or document why simplified is acceptable.
•	No new TODOs introduced without a corresponding effort or plan.

**Acceptance:** Zero unaddressed TODOs; all simplifications either improved or documented.

---

### Population detail (civilisation/regime)

**Goal:** Enrich the population framework with civilisation detail: tech level, regime type, and regime transitions. Use the Integration concept (`Concepts/Integration/`) and the History Generator concept (`Concepts/HistoryGenerator/`) as reference models so natives, colonies, and history can be driven by or displayed with tech levels and regimes.

**Context:** The population framework (PlanetProfile, PopulationLikelihood, natives, colonies, history) exists in `src/domain/population/`. Code quality & simplifications calls out replacing the "civilization reference placeholder" when a Civilization model exists. The Integration concept app provides a single shared model: LEVELS (tech eras), TECHS (tree), REGIMES (with min/max tech level, coercion/capacity/inclusiveness), TRANSITIONS (allowed regime changes), and simulation logic (validRegimesForLevel, pickRegimeForLevel, history sim). The History Generator concept adds culture sim, regime transitions, and map visualization. This effort brings that model into the domain as population detail.

**Deliverables:**
•	Domain model for civilisation detail: tech level (or level band), regime id, and optional sliders (coercion/capacity/inclusiveness) attachable to PlanetProfile, native population, or colony.
•	Regime and transition data: either adopt Integration’s REGIMES/TRANSITIONS (or a GDScript equivalent) as a single source of truth, or document the mapping from Integration to domain types.
•	Deterministic assignment: given seed and context (e.g. planet type, stellar age, existing population), assign tech level and regime using rules consistent with Integration (e.g. validRegimesForLevel, terrain/climate weighting).
•	History / timeline: where population history is stored, optionally include regime-change and tech-advance events consistent with the transition model.
•	Replace civilisation reference placeholder in SpaceStation/Colony (or inspector) with data from this model.
•	Docs: update Roadmap and any population/civilisation docs; reference `Concepts/Integration/`, `Concepts/HistoryGenerator/`, and `Docs/RegimeChangeModel.md` where relevant.

**Tests:** Determinism for tech/regime assignment; valid regime for given tech level; transition rules respected; serialization/deserialization of new fields.

**Acceptance:** Natives/colonies have tech level and regime; regime is valid for that tech level; placeholder replaced; Integration and History Generator concepts remain the reference for the data shape and rules.

---

### Traveller Use Case

**Goal:** Make StarGen output usable in a standard Traveller tabletop game. When “Traveller use case” is selected, generation produces worlds with valid Universal World Profile (UWP) codes and applies Traveller-compatible rules where they affect generation.

**Context:** Traveller uses a **UWP** string (e.g. `X56789A-7`) for each world: Starport (A–E, X), Size (0–9), Atmosphere (0–15), Hydrographics (0–10), Population (0–15, digit = exponent), Government (0–15), Law Level (0–9), Tech Level (0–15). Optional: bases (Naval, Scout, etc.), trade codes (Ag, Hi, In, etc.). StarGen already has: physical size, atmosphere pressure/composition, hydrographics (ocean coverage), population and government (GovernmentType), tech level (TechnologyLevel), and station classes (U/O/B/A/S). Gaps: explicit Law Level, starport grade (Traveller A–X) vs station class, and numeric UWP digits with Traveller’s tables and trade-code rules.

**Deliverables:**
•	**Use-case toggle:** A generation/spec option (e.g. “Traveller use case”) that enables Traveller-specific outputs and rules. No change to default (non-Traveller) behaviour.
•	**UWP model and assignment:** Domain type(s) for UWP: starport code, size/atmo/hydro/pop/gov/law/tech digits; deterministic derivation from existing CelestialBody, PlanetProfile, and population data (plus any new Law Level source).
•	**Mapping tables:** Size (radius/mass → 0–9); Atmosphere (pressure/composition → 0–15); Hydrographics (ocean coverage → 0–10); Population (count → exponent 0–15); Government (GovernmentType.Regime → Traveller 0–15); Tech Level (TechnologyLevel.Level → Traveller 0–15). Starport: from station presence and class, or world population/importance → A–X.
•	**Law Level:** Define Law Level (0–9) in domain (e.g. on PlanetProfile or population); derive from government, tech level, and/or culture; use in UWP and, where applicable, in Traveller rules.
•	**Traveller rules where applicable:** When use case is on, apply Traveller-compatible rules that affect generation (e.g. tech level modifiers from size/atmosphere/hydrographics; starport from population and location; trade codes from UWP digits). Document which rules are applied and which remain optional/house-rule.
•	**Trade codes (optional):** Derive Traveller trade codes (Ag, Hi, In, etc.) from UWP and attach to world data when use case is on.
•	**Bases (optional):** Optional Naval/Scout/other base flags derivable from government, starport, and population for Traveller play.
•	**Serialization and UI:** UWP (and trade codes/bases if implemented) in save/load and visible in system/planet inspector when Traveller use case is active.
•	**Docs:** Roadmap and, if needed, a short Doc describing Traveller use case, mapping choices, and which Traveller rules are used.

**Tests:** Determinism for UWP derivation (fixed seed → same UWP); mapping boundaries (e.g. size digit 0–9, atmo 0–15); Law Level in range; tech-level modifiers consistent with Traveller when use case on; serialization round-trip for UWP/bases/trade codes.

**Acceptance:** With Traveller use case enabled: generate system → every habitable/world body has a valid UWP; UWP digits and starport follow Traveller conventions; inspector shows UWP (and optional trade codes/bases); same seed produces same UWP; default (Traveller off) behaviour unchanged.

**Outline: what needs to be done to bring the repo to Traveller-usable state**
1. **Spec and toggle:** Add “Traveller use case” (or similar) flag to system/galaxy generation spec and wire it through so generators and UI can branch on it.
2. **UWP domain model:** Introduce UWP data structure (starport, size, atmo, hydro, pop, gov, law, tech) and a deterministic UWP calculator that takes CelestialBody + PlanetProfile + population (natives/colonies) + stations and produces UWP (and optionally bases/trade codes).
3. **Mappings:** Implement and test each digit mapping (size, atmosphere, hydrographics, population exponent, government, tech level) with reference to Traveller tables; add starport logic (world importance + station class → A–X).
4. **Law Level:** Add Law Level to domain (e.g. 0–9); derive from regime, tech, and/or RNG with constraints; persist and use in UWP.
5. **Traveller generation rules:** Where generation choices are affected (e.g. tech level modifiers, starport from population), apply Traveller rules only when use case is on; keep existing logic as default.
6. **Trade codes and bases:** Optional pass that computes trade codes and base presence from UWP and context; expose in data and UI.
7. **Persistence and UI:** Store UWP (and optional fields) in save format; show UWP in system/planet inspector when Traveller use case is active; ensure versioning/schema note if format changes.
8. **Documentation:** Update Roadmap and add minimal Doc describing the use case, mappings, and applied rules.

---

## Effort discipline

Proposed changes that do not fit any existing effort are added as a **new effort** in this roadmap. Each new effort should include: name, summary, gates (if any), deliverables, tests, and acceptance criteria. Do not implement until that effort is explicitly picked by a contributor.

---

## Merged frameworks (reference)

**Population framework:** PlanetProfile, PopulationLikelihood, native populations, colonies, history; `src/domain/population/`; unit tests in `Tests/Unit/Population/`.

**Outposts and space stations:** StationGenerator, StationPlacementRules, OutpostAuthority; prototype at `src/app/prototypes/StationGeneratorPrototype.tscn`.

**Jump lanes (domain + prototype + galaxy viewer):** `src/domain/jumplanes/`, `src/app/jumplanes_prototype/`, and galaxy viewer (`SectorJumpLaneRenderer`, Calculate Jump Routes in `GalaxyInspectorPanel`, save/load in `GalaxySaveData`). Remaining work: see "Jump lanes optimization and polish" effort above.

**Civilisation / population detail (concepts):** `Concepts/Integration/` — Tech Tree, Regime Chart, and History sim with shared REGIMES/TRANSITIONS and tech-level–regime validity. `Concepts/HistoryGenerator/` — Culture sim, regime transitions, and map visualization. Reference for Population detail (civilisation/regime) effort. See `Docs/RegimeChangeModel.md`.

