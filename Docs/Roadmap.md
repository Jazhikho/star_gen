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
•	✅ Core data model: CelestialBody with type-specific components (PhysicalProps, SurfaceProps, AtmosphereProps, RingSystemProps).
•	✅ StellarProps component for star-specific properties (luminosity, spectral class, stellar age, habitable zone calculations).
•	✅ RingSystemProps component supporting multi-band rings (RingBand array with individual properties).
•	✅ Detailed surface expansion: TerrainProps, HydrosphereProps, CryosphereProps for geological and surface features.
•	✅ Enhanced PhysicalProps: oblateness, magnetic_moment, internal_heat_watts.
•	✅ Validation rules and error reporting (CelestialValidator with comprehensive type-specific checks).
•	✅ Serialization: JSON schema with schema_version and generator_version fields (CelestialSerializer).
•	✅ Load/save service for celestial objects (CelestialPersistence).
•	✅ Provenance tracking: generation_seed, generator_version, schema_version, created_timestamp.

**Tests:**
•	✅ Validation invariants: mass > 0, radius > 0, optional fields consistent (TestCelestialValidator).
•	✅ Component unit tests: PhysicalProps, OrbitalProps, SurfaceProps, AtmosphereProps, StellarProps, RingSystemProps, TerrainProps, HydrosphereProps, CryosphereProps, Provenance.
•	✅ Serialization round-trip: object -> JSON -> object equals original (TestCelestialSerializer).
•	✅ Persistence integration: save/load round-trip (TestCelestialPersistence).
•	✅ StellarProps validation for star types.
•	✅ RingSystemProps validation for multi-band configurations.

**Acceptance criteria:**
•	Create an object in code, save to JSON, reload, and verify identical content.

### Phase 2: Object generators v1
**Goal:**
Generate celestial objects deterministically from (spec, seed).

**Stage 1: Core Infrastructure** ✅
•	ParentContext data class for orbital/parent context
•	Archetype enums: SizeCategory, OrbitZone, StarClass, AsteroidType, RingComplexity
•	Archetype data tables: SizeTable, StarTable, OrbitTable
•	Base spec classes with override support
•	Spec classes: StarSpec, PlanetSpec, MoonSpec, AsteroidSpec

**Stage 2: Star Generator** ✅
•	✅ StarSpec (archetype: O/B/A/F/G/K/M class hints, optional mass/age overrides)
•	✅ Main sequence relationships: mass → luminosity → temperature → radius
•	✅ Spectral subclass calculation (G2V precision)
•	✅ StarGenerator.generate(spec, rng) → CelestialBody

**Stage 3: Planet Generator - Physical Core** ✅
•	✅ PlanetSpec with size×orbit archetype
•	✅ PlanetGenerator physical properties (mass, radius, density, rotation)
•	✅ Orbital properties from archetype + parent context
•	✅ Tidal locking detection
•	✅ Magnetic field calculation

**Stage 4: Planet Generator - Surface & Atmosphere** ✅
•	✅ Atmospheric retention/escape based on escape velocity + stellar UV
•	✅ Surface type selection based on archetype
•	✅ Terrain generation for rocky bodies
•	✅ Hydrosphere/Cryosphere based on temperature + mass
•	✅ Gas giant atmosphere (no terrain)

**Stage 5: Moon Generator** ✅
•	✅ MoonSpec with archetype
•	✅ Parent planet context for tidal effects
•	✅ Hill sphere / orbital distance constraints
•	✅ Subsurface oceans for icy moons

**Stage 6: Asteroid Generator** ✅
•	✅ AsteroidSpec with C/S/M type
•	✅ Simple physical properties
•	✅ Minimal/no atmosphere
•	✅ Basic terrain (craters, roughness)

**Stage 7: Ring System Generator** ✅
•	✅ RingSystemSpec with complexity level
•	✅ Roche limit calculation from parent
•	✅ Resonance-based gap placement (simplified)
•	✅ Composition based on distance from star (ice vs rock)

**Stage 8: Golden Masters & Tests** ✅
•	✅ Fixture export utility (FixtureGenerator)
•	✅ 28 fixtures (7 per body type)
•	✅ Regression tests against fixtures (TestGoldenMasters)
•	✅ Range validation tests
•	✅ Physics relationship tests

**Tests:** ✅ All passing (286 tests)
•	✅ Golden-master regression: known seeds match saved fixtures.
•	✅ Range tests: generated outputs always satisfy validation.
•	✅ Stellar relationships: spectral class matches temperature/luminosity ranges.
•	✅ Tidal locking: detection correct for close-in bodies.

**Acceptance criteria:** ✅
•	✅ Generate each body type from a seed and produce stable JSON outputs.

**Phase 2 Status: COMPLETE** ✅
All stages implemented and tested. All 286 tests passing. Ready to proceed to Phase 3.

### Phase 3: Object viewer v1
**Goal:**
View a single celestial object in-app and inspect its properties.

**Stage 1: Basic Viewer Infrastructure** ✅
•	✅ ObjectViewer scene with 3D viewport and environment
•	✅ Camera with orbit controls (CameraController)
•	✅ UI panel structure (top bar, side panel)
•	✅ Basic sphere mesh placeholder
•	✅ Integration tests: scene instantiates and runs one frame

**Stage 2: Object Generation & Display** ✅
•	✅ Generate/re-roll button (changes seed)
•	✅ Wire up generators to create random objects
•	✅ Display basic sphere with size from object
•	✅ Show object name and type in UI
•	✅ Object type selector (star/planet/moon/asteroid)
•	✅ Seed input field for reproducibility
•	✅ Type-specific scaling and camera distance
•	✅ Unit formatting (solar masses/radii, Earth masses/radii, etc.)

**Stage 3: Inspector Panel** ✅
•	✅ Read-only property display
•	✅ Organize properties by component (Physical, Orbital, etc.)
•	✅ Format values with appropriate units
•	✅ Collapsible sections for components
•	✅ InspectorPanel class with dynamic property creation
•	✅ All component sections (Physical, Stellar, Orbital, Atmosphere, Surface, Ring System)
•	✅ Proper unit formatting with Unicode preserved (M☉, R☉, M⊕, R⊕, ×, superscripts)

**Stage 4: Save/Load System** ✅
•	✅ Save button → file dialog → compressed/JSON export
•	✅ Load button → file dialog → compressed/JSON import
•	✅ Error display for invalid files (user-readable errors)
•	✅ Confirmation that loaded object matches saved (deterministic regeneration)
•	✅ Status messages for user feedback
•	✅ SaveData service with compression (Zstandard) support
•	✅ Regeneration-based storage (spec + context, not full bodies)
•	✅ File size utilities and formatting
•	✅ Integration tests (12 tests covering save/load, compression, error handling, determinism)

**File Size Optimization Strategy (for Stage 4 and future phases):**
•	Store generation specs + seeds instead of full bodies (~100-200 bytes vs ~2-5 KB per object)
•	Compression support (Zstandard) for JSON files (10:1 to 20:1 reduction typical)
•	Delta storage: only save user modifications/overrides
•	LOD storage: different detail levels based on importance (minimal/basic/detailed/full)
•	For Phase 3: Full JSON for debugging, include spec + body for verification
•	For Phase 6+: Store system seed + specs, regenerate on load
•	For Phase 9+: Binary galaxy metadata + visited systems in JSON, unvisited as seeds only

**Stage 5: Star Rendering** (Pending)
•	Temperature → blackbody color mapping
•	Star material with emission
•	Size-based intensity scaling
•	Add glow/bloom for stars

**Stage 6: Planet/Moon Surface Rendering** (Pending)
•	Surface type → shader selection
•	Basic surface textures/colors
•	Albedo from surface properties
•	Terrain roughness visualization

**Stage 7: Atmosphere Rendering** (Pending)
•	Atmospheric scattering shader
•	Composition → sky color calculation
•	Atmosphere thickness visualization
•	Greenhouse effect visual hints

**Stage 8: Ring System Rendering** (Pending)
•	Ring mesh generation from bands
•	Opacity from optical depth
•	Composition → ring color
•	Multiple band visualization

**Tests:**
•	✅ Integration smoke test: viewer scene instantiates and runs one frame.
•	Invalid JSON load fails gracefully (no crash, user-readable error).
•	Color derivation: temperature maps to correct blackbody colors.
•	Atmospheric scattering: composition produces expected sky colors.

**Acceptance criteria:**
•	Open app -> generate object -> view -> save -> reload -> same result.

### Phase 4: Object editing v1
**Goal:**
Edit object properties in the program with validation, derived-value recalculation, and undo.

**Deliverables:**
•	Editable inspector controls for core fields (mass, radius, type-specific).
•	Derived values recalc after edits (e.g., density, surface gravity).
•	Recalculate derived stellar properties when mass/radius change (luminosity, spectral class).
•	Validate ring gaps don't overlap when editing ring systems.
•	Update tidal locking status when orbital distance changes.
•	Field lock toggles (prepares for future constrained generation).
•	Undo/redo stack (command-based) for edits.

**Tests:**
•	Validation tests reject invalid edits and preserve last valid state.
•	Derived-value tests verify correct recalculation.
•	Stellar recalculation: mass/radius changes update luminosity/spectral class.
•	Ring gap validation: overlapping gaps detected and prevented.
•	Undo/redo tests restore exact prior state.

**Acceptance criteria:**
•	Edit fields -> derived updates -> undo -> save/load preserves edits.

### Phase 5: Object rendering v2 (optional)
**Goal:**
Improve visuals without expanding simulation scope.

**Deliverables:**
•	Seed-driven material layers (continents/bands/noise) for planets.
•	Oblateness rendering: gas giant bulge from rotation/density calculations.
•	Atmospheric scale height → visual thickness rendering.
•	Multiple cloud layers based on atmospheric properties.
•	Aurora effects from magnetic field strength and composition.
•	Ring shadow casting on planet surfaces.
•	Simple atmosphere shell and ring rendering (if enabled by props).
•	Basic LOD policy for meshes/material complexity.

**Tests:**
•	Performance budget check for common viewer actions.
•	Guard against NaN/inf material parameters.
•	Oblateness: correct bulge rendering for fast-rotating bodies.
•	Aurora: magnetic field strength maps to correct visual intensity.

**Acceptance criteria:**
•	Viewer remains stable and responsive while showing improved visuals.

### Phase 6: Solar system generator and viewer
**Goal:**
Randomly generate a solar system, display it, and inspect its bodies (no editing tools yet).

**Deliverables:**
•	SolarSystem data model: stars, planets, optional moons and belts (start minimal).
•	Orbital parameters: semi-major axis, eccentricity, inclination (start simple).
•	Orbital resonances between bodies (detection and visualization).
•	Hill sphere validation: ensure moons are within gravitational influence.
•	Barycenter calculation for binary star systems.
•	Roche limit enforcement for rings and close moons.
•	Habitable zone calculation from star properties (luminosity, temperature).
•	SolarSystemGenerator.generate(spec, rng) with minimal spec (ranges only).
•	SolarSystemViewer: 2D map or lightweight 3D view; select body opens ObjectViewer.

**Tests:**
•	Determinism: same seed/spec -> identical system layout.
•	Orbital invariants: distances positive; no planet inside star radius; ecc in [0, 1).
•	Hill sphere validation: moons properly constrained within parent's influence.
•	Barycenter: binary star systems calculate correct center of mass.
•	Roche limits: rings and close moons respect gravitational stability.
•	Golden-master fixtures for a few seeds.

**Acceptance criteria:**
•	Generate system -> browse bodies -> open object viewer reliably.

### Phase 7: Solar system constraints (locks)
**Goal:**
Support generation constraints such as exact/min/max star count and minimum planet count.

**Deliverables:**
•	SystemConstraints model (exact/min/max counts, must-include templates later).
•	Lock stellar age → affects planet composition and surface age.
•	Lock binary star separation → constrains orbital parameters.
•	Force orbital resonances → snap orbits to resonant ratios.
•	Constraint-aware generation with bounded retries and clear failure errors.
•	UI for constraints and regenerate.

**Tests:**
•	Constraint satisfaction tests.
•	Stellar age lock: affects planetary properties correctly.
•	Orbital resonance lock: bodies maintain specified resonant ratios.
•	Impossible constraints fail fast and provide actionable errors.

**Acceptance criteria:**
•	Set constraints -> regenerate -> constraints are satisfied (or cleanly rejected).

### Phase 8: Solar system editing tools
**Goal:**
Allow edits like adding a star at distance X, adjusting orbits, and recalculating the system.

**Deliverables:**
•	Command-based edit operations: add/remove body, adjust orbit, recalc.
•	Propagate stellar luminosity changes to all planet temperatures.
•	Revalidate orbits when star mass changes (recalculate all orbital periods).
•	Cascade deletion: remove moons when planet deleted, remove planets when star deleted.
•	Safety rails: prevent orbit overlap or auto-resolve.
•	System-level undo/redo.

**Tests:**
•	Command apply/undo tests.
•	Luminosity propagation: star changes update all planet temperatures correctly.
•	Mass change recalculation: orbital periods update correctly.
•	Cascade deletion: dependent bodies removed appropriately.
•	Recalc determinism: same system + same edit ops -> same result.

**Acceptance criteria:**
•	Apply edits -> recalc -> undo works and system remains valid.

### Phase 9: Galactic map v1
**Goal:**
Add a galactic container that browses and lazily generates systems without regenerating edits.

**Deliverables:**
•	Galaxy model: sectors/regions and seeded system placement.
•	Stellar metallicity from galactic position (core vs. spiral arm vs. halo).
•	Star formation rate by region (affects age distribution).
•	Stellar density gradients (higher density in core, lower in halo).
•	Lazy generation (generate on demand, not all at once).
•	Persistence of edits as deltas/patches applied on top of seeded generation.
•	Galaxy viewer: browse region/sector -> open system.

**Tests:**
•	Determinism across lazy generation.
•	Metallicity gradients: correct distribution by galactic position.
•	Star formation rate: regional differences affect system ages.
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
