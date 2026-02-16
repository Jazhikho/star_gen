# StarGen Roadmap

Godot 4.x - GDScript | DRY + SOLID | Deterministic generation with test gates

## Overview

This roadmap builds StarGen in three layers: (1) viewable celestial objects (editing deferred), (2) solar systems, then (3) galactic scale. Work is organized as **efforts** that can be pursued in parallel where dependencies allow.

## Guiding principles

•	Determinism is non-negotiable: same seed + same inputs must produce identical outputs.
•	Keep domain logic pure (no scene tree / Nodes / file I/O inside domain).
•	Ship tests with features. Golden-master fixtures cover regression for known seeds.
•	Prefer composition over inheritance. Small services and data components over "manager" classes.
•	Any feature request that does not directly support an active effort goes to BACKLOG.md.

## Definition of Done (per effort)

•	Unit tests pass in headless mode.
•	Determinism checks pass where applicable (fixed seeds match fixtures).
•	Minimal UX flow works end-to-end for the effort.
•	Schema/versioning updated if data formats changed.
•	Documentation updated (this roadmap, plus any dev notes needed to run/verify).

---

## What is done

Verified against the codebase as of the last roadmap update.

**Object layer:** Celestial object model (validation, serialization, persistence). Object generators (star, planet, moon, asteroid, ring system). Golden masters (28 fixtures). Object viewer (generate, inspect, save/load). Body rendering (stars, planets, gas giants, atmospheres, rings). Population framework (PlanetProfile, natives, colonies, history). Outposts and space stations (StationGenerator, placement rules, prototype).

**Solar system layer:** Data model (SolarSystem, SystemHierarchy, OrbitHost, AsteroidBelt). Orbital mechanics (Kepler, Hill sphere, Roche limit, resonances, stability). Stellar config generator. Orbit slot generator. Planet generation (SystemPlanetGenerator). Moon generation (SystemMoonGenerator). Asteroid belts (SystemAsteroidGenerator). Validation, serialization, persistence. Golden masters (10 system fixtures). System viewer (3D layout, orbit renderer, body nodes, inspector, link to object viewer). System display layout (sweep-based separation, no overlap in multi-star systems). Zone visualization, view toggles, generation UI.

**Galaxy layer:** Welcome screen (Start New, Load, Quit). GalaxyConfig (type, arms, pitch, ellipticity, seed). Density models (spiral, elliptical, irregular). DensitySampler, GalaxyCoordinates. Galaxy viewer (3D, quadrant/sector/subsector zoom, MultiMesh stars). Save/load galaxy state (seed, zoom, camera, selection).

**Jump lanes:** Domain (JumpLaneCalculator, JumpLaneClusterConnector, connection types including extended red). Prototype scene with mock data. Unit tests.

---

## Efforts (remaining work)

Contributors pick an effort and work against master. Efforts can run in parallel unless gated.

| Effort | Name | Summary | Gates |
|--------|------|---------|-------|
| 1 | Solar system constraints | Constraint-based generation (min/max/exact counts, stellar locks, resonances), UI | — |
| 2 | Solar system save/load & polish | Save/load UI for systems, tooltips, shortcuts, optimization | — |
| 3 | Galaxy data model & lazy generation | Galaxy/Sector/GalaxyStar classes, lazy system generation on demand | — |
| 4 | Object editing | Editable inspector, derived-value recalc, undo/redo | — |
| 5 | Object rendering v2 | Oblateness, aurora, LOD, seed-driven materials | — |
| 6 | Galactic generator refinement | Region constraints, region rules, constraint-based placement | Effort 3 |
| 7 | Solar system tools | Add/remove bodies, adjust orbits, recalc, system-level undo | — |
| 8 | Galactic tools | System placement edits, region editing, galactic undo | Effort 3, Effort 6 |
| 9 | Galactic polish | Galaxy save/load UI polish, backward compat, performance | Effort 3 |
| 10 | Jump lanes integration | Wire jump-lanes into galaxy viewer; population data; line/orphan rendering | Effort 3 |
| 11 | Code quality & simplifications | TODOs, placeholder replacements, simplified formulas to redo | — |

---

## Effort details

### Effort 1: Solar system constraints

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

### Effort 2: Solar system save/load & polish

**Goal:** Complete the solar system viewer with save/load UI and final optimizations.

**Deliverables:**
•	Save/load UI for systems (file dialogs, save/load buttons). SystemPersistence service exists; wire into viewer.
•	Performance optimizations (LOD, culling, batch rendering).
•	UI polish (tooltips, keyboard shortcuts, status messages).
•	Final testing and bug fixes.

**Tests:** Save/load round-trip; performance benchmarks; UI interaction tests.

**Acceptance:** Save system → load system → identical system displayed. Viewer remains responsive with large systems (100+ bodies).

---

### Effort 3: Galaxy data model & lazy generation

**Goal:** Add Galaxy, Sector, GalaxyStar classes and lazy system generation so systems are generated on demand.

**Deliverables:**
•	Galaxy data model: Galaxy, Sector, GalaxyStar with serialization.
•	Sector-based lazy generation; density from density model.
•	Stellar metallicity from galactic position (core vs. spiral arm vs. halo).
•	Star formation rate by region (affects age distribution).
•	Lazy generation consistency: same sector generates same systems.
•	Single entry point for "systems in region" (subsector/sector) for jump-lanes integration.

**Tests:** Determinism across lazy generation; metallicity gradients; grid-based sector indexing; lazy generation consistency; persistence round-trip.

**Acceptance:** Browse galaxy → open system → edits persist across sessions. Galaxy viewer remains responsive with 10k+ stars.

**Gates:** Effort 6 (galactic refinement), Effort 8 (galactic tools), Effort 9 (galactic polish), Effort 10 (jump lanes integration) depend on this.

---

### Effort 4: Object editing

**Goal:** Edit object properties in the program with validation, derived-value recalculation, and undo.

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

### Effort 5: Object rendering v2

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

### Effort 6: Galactic generator refinement

**Goal:** Improve galactic generation with region-level constraints and rules.

**Gates:** Effort 3 (galaxy data model). Cannot proceed until lazy generation and Galaxy/Sector/GalaxyStar exist.

**Deliverables:**
•	Region constraints (e.g., higher binary fraction, denser core).
•	Constraint-based system placement.
•	Region-level generation rules.
•	UI for constraints and regenerate.

**Tests:** Constraint satisfaction; region rule application; determinism across constraint changes.

**Acceptance:** Apply region rules → regenerate → constraints are satisfied.

---

### Effort 7: Solar system tools

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

### Effort 8: Galactic tools

**Goal:** Add editing tools for modifying galactic structure: add/remove systems, adjust density.

**Gates:** Effort 3 (galaxy data model), Effort 6 (galactic refinement). Requires region rules and data model.

**Deliverables:**
•	Editing tools: add/remove system, adjust density, regenerate region.
•	System placement editing.
•	Region editing (modify constraints, regenerate).
•	Galactic-level undo/redo.

**Tests:** Edit operation tests; persistence for edits; undo/redo tests.

**Acceptance:** Apply edits → recalc → undo works and galaxy remains valid.

---

### Effort 9: Galactic polish

**Goal:** Complete the galactic viewer with save/load polish, backward compatibility, and optimizations.

**Gates:** Effort 3 (galaxy data model). Save/load of galaxy state already exists; this effort is polish and compatibility.

**Deliverables:**
•	Galaxy save/load UI polish (file dialogs, save/load buttons if not yet present).
•	Backward compatibility for saved galaxy data (at least one prior schema version).
•	Performance optimizations (lazy loading, caching).
•	UI polish (tooltips, keyboard shortcuts, status messages).
•	Final testing and bug fixes.

**Tests:** Save/load round-trip; backward compatibility; performance benchmarks.

**Acceptance:** Save galaxy → load galaxy → identical galaxy displayed. Edits remain stable and preserved.

---

### Effort 10: Jump lanes integration

**Goal:** Expose the jump-lanes tool in the galaxy viewer: user selects range (subsector vs sector), runs the calculation, and sees lines (green/yellow/orange/red) and orphan highlighting.

**Gates:** Effort 3 (galaxy data model). Needs "get systems in region" entry point and galaxy viewer to display results.

**Deliverables:**
•	Population data wiring: single entry point returning `JumpLaneRegion` for current subsector/sector (placeholder population acceptable until real data).
•	User controls: subsector vs sector range; Run jump-lanes button.
•	Visual representation: draw connections (green/yellow/orange/red); highlight orphans (red).
•	Integration test: load sector, run tool, assert connection/orphan counts.
•	Docs: where jump-lanes lives (menu/panel/shortcut) and how to use.

**See:** `Docs/FeatureConceptBranchImplementationPlan.md` for detailed stages.

**Acceptance:** User can select subsector or sector and run the tool. Lines appear with correct colors; orphans appear in red.

---

### Effort 11: Code quality & simplifications

**Goal:** Address TODOs in code, replace placeholders, and redo simplified formulas where accuracy or correctness matters.

**Scope (from codebase survey):**
•	**TODOs:** SystemViewer — Escape key for back navigation; SpaceStation/Colony — civilization reference placeholder (replace when Civilization model exists).
•	**Simplifications to review/redo:** OrbitalMechanics (stability zone simplified check); OrbitSlotGenerator (companion positions, host at origin); OrbitTable (tidal locking formula from Peale 1977); PlanetSurfaceGenerator (pressure/boiling point); PlanetPhysicalGenerator (tidal heating); MoonPhysicalGenerator (tidal heating); RingSystemGenerator (resonance-based gaps); StationPlacementRules (simplified system context); SystemDisplayLayout (logarithmic vs astronomical scaling — document or revise).
•	**General:** Audit for other "simplified" or "placeholder" comments; consolidate or document design decisions.

**Deliverables:**
•	Resolve or document each TODO (either implement or add to BACKLOG with justification).
•	Audit simplified formulas: either improve accuracy or document why simplified is acceptable.
•	No new TODOs introduced without a corresponding BACKLOG entry or plan.

**Acceptance:** Zero unaddressed TODOs; all simplifications either improved or documented.

---

## Backlog discipline

Any feature request that does not directly support an active effort must be recorded in BACKLOG.md and deferred. Each backlog item should include: title, why it matters, target effort (or TBD), and complexity (S/M/L). Do not implement backlog items unless an effort or maintainer explicitly includes them.

---

## Merged frameworks (reference)

**Population framework:** PlanetProfile, native populations, colonies, history; `src/domain/population/`; unit tests in `Tests/Unit/Population/`.

**Outposts and space stations:** StationGenerator, StationPlacementRules, OutpostAuthority; prototype at `src/app/prototypes/StationGeneratorPrototype.tscn`.

**Jump lanes (domain + prototype):** `src/domain/jumplanes/`, `src/app/jumplanes_prototype/`; see `Docs/FeatureConceptBranchImplementationPlan.md` for Phase 2 integration work.
