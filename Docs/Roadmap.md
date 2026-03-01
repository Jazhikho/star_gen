# StarGen Roadmap

Godot 4.x - GDScript (C# refactor in progress) | DRY + SOLID | Deterministic generation with test gates

## Version history

Release notes and version summaries are in the [README](../README.md#version-history). **v0.1** corresponds to commit `90e2636`; **v0.2** is the current patch (asteroid belts in system viewer, scientific calibration, harness and docs).

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
| **C# refactor** | Incremental GDScript → C# migration; domain first, then services, then app; preserve determinism and save format | — | — |
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
| Engine/tool integration | Minimal Unity/Unreal sample importer or plugin for real workflow evaluation | — | — |
| Export function | Clean JSON/CSV export for design/UI iteration and technical wiring | — | — |
| Filters that match game needs | Presets (frontier, dense core, mystery zone, resource rich, dangerous) for missions and worldbuilding | — | — |
| Quick tagging layer | Labels (mining, habitable, hazard, trade hub, pirate risk) on objects/systems | — | — |
| Favorites and notes | Bookmark systems, short descriptions, optional screenshot | — | — |
| Export frames as skybox | 4K cubemap set or equirectangular pano for art team (level/menu placeholder) | System viewer rendering improvements (optional) | — |
| Traveller alignment | Align with Traveller UWP planet size scale; size code layer (0–C, D/E) from diameter; Mini-Neptune and above map to D or E | — | — |
| Traveller Use Case | Generate systems/planets/populations usable in standard Traveller play: UWP codes, Traveller rules where applicable, use-case toggle | — | — |
| Starfinder / other RPG export | Mapper from body + population to Starfinder-style world type, gravity, atmosphere, biomes | — | — |
| Stars Without Number export | World tags and sector data derivation for SWN-style play | — | — |
| Traveller/Cepheus UWP full export | Full UWP and sector file export (TravellerMap-compatible) | Traveller alignment | — |
| Education / outreach mode | Deterministic demos, parameter exploration, seed-based reproducibility for workshops and teaching | — | — |
| Science / extended classification export | CADRS-like or exoplanet-class export for worldbuilding or reference | — | — |

**Note:** Branches `object-view` and `effort/traveller-use-case` have been merged into master. Work on Object editing, Object rendering v2, and Traveller Use Case now proceeds on master.

---

## Effort details

### C# refactor (high priority)

**Goal:** Migrate the codebase from GDScript to C# incrementally, preserving determinism, save/load format, and test coverage. GDScript and C# will coexist during migration; app can call into C# domain/services.

**Strategy (migrate in dependency order):**

1. **Domain (no engine)** — Start with leaf types: `CelestialType`, `Units`, `Versions`; then component props (`PhysicalProps`, `OrbitalProps`, `AtmosphereProps`, etc.); then `CelestialBody`, `Provenance`; then validation and serialization. Replace every `load("res://.../X.gd") as GDScript` / `script_class.new(...)` with `new TypeName(...)`. Keep the same `to_dict` / `from_dict` contract so existing callers (or GDScript wrappers) still see the same dictionary shape.
2. **RNG and generation** — `SeededRng`, then specs and generators (Star, Planet, Moon, Asteroid, Ring, etc.). Preserve algorithms and numeric behaviour so golden-master and determinism tests still pass.
3. **System / galaxy domain** — `SolarSystem`, `SystemHierarchy`, `OrbitSlot`, `Galaxy`, `Sector`, etc.; then persistence that uses these types. Keep save/load schema unchanged.
4. **Services** — Persistence (Celestial, System, Galaxy, SaveData). Prefer one language per layer.
5. **App (Nodes)** — Replace scene-attached scripts last: MainApp, WelcomeScreen, viewers (Object, System, Galaxy), UI, rendering. Update every `.tscn` that references a migrated script to the new `.cs` path; fix signals and connections.
6. **Tests** — Port the headless test runner to C# (or keep a thin GDScript runner that instantiates C# types and asserts). Retire `Phase1Deps.gd` / `PopulationDeps.gd` / `JumpLanesDeps.gd` once all tests run against C# types.

**Compatibility during migration:** Keep save/load and fixture JSON/dict structure unchanged. GDScript may call C# (Godot script API). Run full test suite and determinism checks after each migrated chunk.

**Risk checklist:** Determinism (same seed → same output); save/load backward compatibility; all scenes referencing migrated scripts updated; signals names/signatures preserved; no remaining `load("...").new()` for ported types.

**Files involved in refactor (by layer):**

- **Domain — celestial:** `src/domain/celestial/CelestialType.gd`, `CelestialBody.gd`, `Provenance.gd`; `src/domain/celestial/components/PhysicalProps.gd`, `OrbitalProps.gd`, `AtmosphereProps.gd`, `StellarProps.gd`, `TerrainProps.gd`, `HydrosphereProps.gd`, `CryosphereProps.gd`, `RingBand.gd`, `RingSystemProps.gd`, `SurfaceProps.gd`; `src/domain/celestial/validation/ValidationError.gd`, `ValidationResult.gd`, `CelestialValidator.gd`; `src/domain/celestial/serialization/CelestialSerializer.gd`.
- **Domain — constants/math/rng:** `src/domain/constants/Versions.gd`; `src/domain/math/Units.gd`, `MathUtils.gd`; `src/domain/rng/SeededRng.gd`; `src/domain/validation/Validation.gd`.
- **Domain — generation:** `src/domain/generation/ParentContext.gd`, `GenerationRealismProfile.gd`; `src/domain/generation/archetypes/SizeCategory.gd`, `OrbitZone.gd`, `StarClass.gd`, `AsteroidType.gd`, `RingComplexity.gd`, `TravellerSizeCode.gd`; `src/domain/generation/specs/BaseSpec.gd`, `StarSpec.gd`, `PlanetSpec.gd`, `MoonSpec.gd`, `AsteroidSpec.gd`, `RingSystemSpec.gd`; `src/domain/generation/tables/SizeTable.gd`, `StarTable.gd`, `OrbitTable.gd`; `src/domain/generation/utils/AtmosphereUtils.gd`; `src/domain/generation/generators/GeneratorUtils.gd`, `StarGenerator.gd`, `PlanetGenerator.gd`, `MoonGenerator.gd`, `AsteroidGenerator.gd`, `RingSystemGenerator.gd`; `src/domain/generation/generators/planet/PlanetPhysicalGenerator.gd`, `PlanetAtmosphereGenerator.gd`, `PlanetSurfaceGenerator.gd`; `src/domain/generation/generators/moon/MoonPhysicalGenerator.gd`, `MoonSurfaceGenerator.gd`, `MoonAtmosphereGenerator.gd`; `src/domain/generation/fixtures/FixtureGenerator.gd`.
- **Domain — editing:** `src/domain/editing/ConstraintSet.gd`, `EditRegenerator.gd`, `EditSpecBuilder.gd`, `PropertyConstraint.gd`, `PropertyConstraintSolver.gd`, `TravellerConstraintBuilder.gd`.
- **Domain — system:** `src/domain/system/SolarSystem.gd`, `SolarSystemSpec.gd`, `SystemValidator.gd`, `SystemSerializer.gd`, `SystemCache.gd`, `SystemHierarchy.gd`, `HierarchyNode.gd`, `OrbitSlot.gd`, `OrbitHost.gd`, `OrbitalMechanics.gd`, `AsteroidBelt.gd`, `OrbitSlotGenerator.gd`, `StellarConfigGenerator.gd`, `SystemPlanetGenerator.gd`, `SystemMoonGenerator.gd`, `SystemAsteroidGenerator.gd`; `src/domain/system/asteroid_belt/BeltOrbitalMath.gd`, `BeltFieldSpec.gd`, `BeltFieldData.gd`, `BeltFieldGenerator.gd`, `BeltAsteroidData.gd`, `BeltMajorAsteroidInput.gd`; `src/domain/system/fixtures/SystemFixtureGenerator.gd`.
- **Domain — galaxy:** `src/domain/galaxy/Galaxy.gd`, `GalaxySpec.gd`, `GalaxyConfig.gd`, `GalaxyStar.gd`, `GalaxySample.gd`, `GalaxySaveData.gd`, `GalaxySystemGenerator.gd`, `GalaxyBodyOverrides.gd`, `GalaxyCoordinates.gd`, `Sector.gd`, `HomePosition.gd`, `SeedDeriver.gd`, `StableHash.gd`, `GridCursor.gd`, `StarPicker.gd`, `StarSystemPreview.gd`, `SubSectorGenerator.gd`, `SubSectorNeighborhood.gd`, `DensitySampler.gd`, `DensityModelInterface.gd`, `SpiralDensityModel.gd`, `EllipticalDensityModel.gd`, `IrregularDensityModel.gd`, `RaycastUtils.gd`.
- **Domain — jumplanes:** `src/domain/jumplanes/JumpLaneSystem.gd`, `JumpLaneConnection.gd`, `JumpLaneRegion.gd`, `JumpLaneResult.gd`, `JumpLaneCalculator.gd`, `JumpLaneClusterConnector.gd`.
- **Domain — population:** `src/domain/population/BiomeType.gd`, `ClimateZone.gd`, `ColonyType.gd`, `Colony.gd`, `ColonyGenerator.gd`, `ColonySuitability.gd`, `Government.gd`, `GovernmentType.gd`, `HabitabilityCategory.gd`, `HistoryEvent.gd`, `HistoryGenerator.gd`, `NativePopulation.gd`, `NativePopulationGenerator.gd`, `NativeRelation.gd`, `Outpost.gd`, `OutpostAuthority.gd`, `PlanetProfile.gd`, `PlanetPopulationData.gd`, `PopulationHistory.gd`, `PopulationLikelihood.gd`, `PopulationProbability.gd`, `PopulationSeeding.gd`, `PopulationGenerator.gd`, `ProfileCalculations.gd`, `ProfileGenerator.gd`, `ResourceType.gd`, `SpaceStation.gd`, `StationClass.gd`, `StationGenerator.gd`, `StationPlacementContext.gd`, `StationPlacementRules.gd`, `StationPurpose.gd`, `StationService.gd`, `StationSpec.gd`, `StationType.gd`, `SuitabilityCalculator.gd`, `TechnologyLevel.gd`.
- **Services:** `src/services/persistence/CelestialPersistence.gd`, `SystemPersistence.gd`, `GalaxyPersistence.gd`, `SaveData.gd`.
- **App:** `src/app/MainApp.gd`, `WelcomeScreen.gd`; `src/app/components/CollapsibleSection.gd`; `src/app/viewer/ObjectViewer.gd`, `ObjectViewerMoonSystem.gd`, `InspectorPanel.gd`, `PropertyFormatter.gd`, `CameraController.gd`, `EditDialog.gd`; `src/app/rendering/BodyRenderer.gd`, `MaterialFactory.gd`, `ColorUtils.gd`, `ShaderParamHelpers.gd`, `AtmosphereShaderParams.gd`, `GasGiantShaderParams.gd`, `RingShaderParams.gd`, `StarShaderParams.gd`, `TerrestrialShaderParams.gd`; `src/app/system_viewer/SystemViewer.gd`, `SystemBodyNode.gd`, `SystemDisplayLayout.gd`, `SystemInspectorPanel.gd`, `SystemScaleManager.gd`, `SystemViewerSaveLoad.gd`, `SystemCameraController.gd`, `OrbitRenderer.gd`, `BeltRenderer.gd`; `src/app/galaxy_viewer/GalaxyViewer.gd`, `GalaxyViewerDeps.gd`, `GalaxyViewerSaveLoad.gd`, `GalaxyRenderer.gd`, `GalaxyInspectorPanel.gd`, `SectorRenderer.gd`, `QuadrantRenderer.gd`, `SubSectorRenderer.gd`, `QuadrantSelector.gd`, `NeighborhoodRenderer.gd`, `SectorJumpLaneRenderer.gd`, `SelectionIndicator.gd`, `OrbitCamera.gd`, `StarViewCamera.gd`, `NavigationCompass.gd`, `ZoomStateMachine.gd`; `src/app/jumplanes_prototype/JumpLanesPrototype.gd`, `JumpLaneRenderer.gd`, `MockRegionGenerator.gd`; `src/app/prototypes/StationGeneratorPrototype.gd`.
- **Tests:** `Tests/Phase1Deps.gd`, `PopulationDeps.gd`, `JumpLanesDeps.gd`, `RunTestsHeadless.gd`, `TestScene.gd`, `JumpLanesTestRunner.gd`, `JumpLanesTestScene.gd`, `Framework/TestCase.gd`, `Framework/TestRunner.gd`, `Framework/TestResult.gd`, plus all `Tests/Unit/**/*.gd`, `Tests/Integration/**/*.gd`, `Tests/domain/**/*.gd`, and harnesses (`GenerationStatsHarness.gd`, `ScientificBenchmarks.gd`).

---

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

**Done:** Gas giant variety (archetype + per-planet variation): seven archetypes (Hot Jupiter, Jupiter-class, Saturn-class, Neptune-class, Uranus-class, Super-Jupiter, Mini-Neptune) with structural presets; seeded RNG drives band warp, sharpness, storm count range, dark-spot ratio, haze, detail level, and multi-palette colour choice so same-archetype planets look related but distinct. Shader: `planet_gas_giant_surface.gdshader`; params: `GasGiantShaderParams.gd`. Unit tests in `TestGasGiantShaderParams.gd`.

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

### Engine/tool integration

**Goal:** Provide a minimal Unity/Unreal sample importer or plugin so StarGen output can be evaluated in a real engine workflow.

**Deliverables:**
•	Sample importer or plugin for Unity and/or Unreal (at least one; both ideal long term).
•	Documented API or asset format so external tools can consume StarGen data (objects, systems, sectors).
•	Minimal in-engine sample: load a system or sector and display/use the data (e.g. skybox, spawn points, or simple UI).

**Tests:** Import round-trip or schema validation; sample scene runs without errors.

**Acceptance:** A technical user can pull StarGen data into Unity or Unreal and see it in a minimal sample scene; format is documented for further wiring.

---

### Export function

**Goal:** Provide clean JSON/CSV export so design/UI can iterate on data and technical users can wire StarGen into pipelines.

**Deliverables:**
•	Export UI: choose scope (object, system, sector, galaxy) and format (JSON, CSV).
•	JSON: versioned schema for objects, systems, sectors; stable keys and structure for tooling.
•	CSV: tabular exports (e.g. system list, body list) for spreadsheets and design tools.
•	Documentation: schema version, field meanings, and example exports.

**Tests:** Export round-trip or schema validation; CSV parses and contains expected columns.

**Acceptance:** User can export selected scope as JSON or CSV; output is well-formed and documented for design and technical integration.

---

### Filters that match game needs

**Goal:** Presets (e.g. “frontier,” “dense core,” “mystery zone,” “resource rich,” “dangerous”) so filtered output is immediately useful for missions and worldbuilding.

**Deliverables:**
•	Filter preset definitions: criteria per preset (e.g. density, hazards, resources, population) mapped to domain data.
•	Apply presets to system/region/sector listing or generation; filtered view or filtered export.
•	Determinism preserved when filtering (same seed + same preset → same filtered set).
•	UI: choose preset, see filtered list, optionally export filtered result.

**Tests:** Preset criteria applied correctly; determinism for filtered results; edge cases (empty result, full result).

**Acceptance:** User selects a preset and sees a filtered set of systems/regions suitable for missions and worldbuilding; behavior is deterministic and documentable.

---

### Quick tagging layer

**Goal:** Add a basic tagging layer (e.g. “mining,” “habitable,” “hazard,” “trade hub,” “pirate risk”) on objects and/or systems for design and scripting.

**Deliverables:**
•	Tag model: defined tag set (single source of truth); attach tags to objects and/or systems (and optionally sectors).
•	Assignment: deterministic rules where possible (e.g. from population, hazards, resources); optional manual overrides.
•	UI: view and edit tags in inspector or list; tags visible in system/galaxy view.
•	Export and save: tags included in JSON/CSV and in save data.

**Tests:** Tag assignment rules; serialization/deserialization; determinism when tags are rule-driven.

**Acceptance:** Objects/systems can carry tags; user can see and edit tags; tags are exported and saved; rules are consistent with population/hazard/resource data where used.

---

### Favorites and notes

**Goal:** Let users bookmark favorites and attach short notes and optional screenshots for quick reference.

**Deliverables:**
•	Favorites/bookmarks: mark objects, systems, or locations as favorite; persisted list.
•	Per-item notes: short free-text description; persisted with the bookmark.
•	Optional screenshot: attach a screenshot (or reference to a captured frame) to a bookmark.
•	UI: add/remove favorites; edit notes; view list and jump to bookmarked item; load/save with project or user data.

**Tests:** Persistence round-trip; no crash when screenshot missing or invalid.

**Acceptance:** User can bookmark items, add notes and optional screenshot, and return to bookmarks from a simple UI.

---

### Export frames as skybox

**Goal:** Export the current (or chosen) view as skybox-ready art: 4K cubemap set or equirectangular pano so an art team can drop it into a level or menu background as a placeholder.

**Gates:** System viewer rendering improvements (optional but recommended so captured frames are lit and composed correctly).

**Deliverables:**
•	Export options: cubemap (6 faces) or single equirectangular panorama; resolution options (e.g. 4K).
•	Capture from system viewer: use current camera or a dedicated “skybox capture” camera; document orientation and convention (e.g. face order, up axis).
•	Output: standard image format (PNG/EXR); folder or archive for cubemap set; naming convention for engine import.
•	Docs: how to capture, expected layout, and how to import in common engines (e.g. Unity/Unreal).

**Tests:** Output dimensions and count correct; no invalid or blank frames for valid capture request.

**Acceptance:** User can export a 4K cubemap set or equirectangular pano from the system viewer; art can import it as a skybox/background with minimal hand-work.

---

### Traveller alignment

**Goal:** Align StarGen with the Traveller UWP planet size scale so generated bodies can be mapped to Traveller size codes for export or display.

**Done:** Simplified Traveller size-code layer in domain: `TravellerSizeCode` (`src/domain/generation/archetypes/TravellerSizeCode.gd`) with diameter bounds (0 = very small/asteroid &lt;800 km; 1–9, A, B, C; D = small gas giant 40,000–120,000 km; E = large gas giant 120,000+ km). `diameter_km_to_code(diameter_km)` returns int 0–9 or String "A"–"E"; `code_to_diameter_range(code)` and `to_string_uwp(code)` for validation/display. From `PhysicalProps.radius_m`, compute `diameter_km = 2 * radius_m / 1000.0` and call `diameter_km_to_code`. Mini-Neptune and above (and any body with diameter in gas-giant range) map to D or E; internal `SizeCategory` enum is unchanged. Unit tests in `TestTravellerSizeCode.gd`.

**Remaining (optional):** When UWP or “Traveller export” is implemented, use this module for the size digit. Constraining generation by Traveller code (e.g. “size 5–8 only”) would map code ranges back to diameter/mass; not required for alignment.

**Acceptance:** For any `PhysicalProps.radius_m`, the Traveller size code (0–9, A, B, C, D, E) is uniquely determined and unit-tested; roadmap documents the alignment and how Mini-Neptune plus maps to D/E.

---

## Scientific calibration and realism

**Realism profiles:** `GenerationRealismProfile` (`src/domain/generation/GenerationRealismProfile.gd`) defines three modes: **Calibrated** (tracks literature-derived distributions), **Balanced** (default; visually rich, roughly plausible), and **Stylized** (more rings, habitable worlds, spectacular systems). A **[0, 1] realism slider** maps to these: 0 → Stylized, 0.5 → Balanced, 1 → Calibrated (`from_slider`). Profile choice plus seed fully defines outcomes when generators respect it (generators are not yet wired to the profile).

**Benchmarks and comparison:** Reference ranges from astrophysical literature are in `Tests/ScientificBenchmarks.gd` (e.g. M-dwarf fraction 60–90%, G+K fraction 10–35%, hot Jupiters &lt;5% of planets). **StarGenerator** is compared to spectral-type benchmarks (local IMF / 20 pc census). **SystemPlanetGenerator** is compared to hot-Jupiter rarity and inner-vs-outer large-planet fractions (exoplanet demographics). **SubSectorGenerator** is tested for solar-neighborhood density (see `TestSubSectorGenerator.test_solar_neighborhood_density_realistic`).

**Running the statistical comparison:** Run the distribution tests via the headless runner or test scene: `TestStarGeneratorDistributions` (spectral histogram vs benchmarks), `TestSystemPlanetDistributions` (hot Jupiters, cold-zone large planets). For custom ensemble runs, use `Tests/GenerationStatsHarness.gd`: `sample_star_spectral_histogram(seed_base, count)` and `sample_system_planet_stats(seed_base, system_count)` return aggregated histograms/stats without writing files.

**Known intentional deviations:** Ring frequency and terrain variety are tuned for visual diversity. When the profile is wired into generators, Calibrated mode will use benchmark-tight parameter sets; Stylized may increase ring likelihood and habitable-zone emphasis while staying within physical bounds.

**Outer orbit limits (how far out can a planet be):** Two ceilings apply. (1) **Dynamical (Jacobi/tidal):** where the Galaxy's tidal field competes with the star's gravity; r_J ∝ M_star^(1/3) (~1.70 pc × (M/(2 M_sun))^(1/3)); for 1 M_sun ~2.78×10^5 AU. (2) **Formation (disc):** dust extent R_dust ∝ M_star^0.6 (Taurus/Lupus); most discs tens of AU (e.g. 67% of Lupus &lt; 30 AU). StarGen uses **min**(formation, Jacobi) as outer stability limit (`OrbitalMechanics.calculate_outer_stability_limit_m`); formation is the usual limiter; Jacobi caps the dynamical ceiling. Wide-separation freaks (e.g. 2MASS J2126 ~7000 AU) are likely capture-like, not disc-formed.

**Planet count and orbital stability:** Science does not give a single “max planets per star” number; it depends on definition of “planet,” initial disk mass, and whether orbits remain stable. **Observed:** up to 8 confirmed around one star (Solar System, Kepler-90); TRAPPIST-1 has 7 in tight orbits. **Why no hard limit:** (1) “Planet” is convention-dependent (e.g. IAU “clearing the neighbourhood”); (2) orbital stability is the real constraint — spacing below ~10 mutual Hill radii tends to instability (Chambers 1996); (3) formation (mergers, migration, scattering) typically reduces final counts. **Back-of-envelope** for equal-mass planets at ~10 mutual Hill radii over 0.1–100 AU around a Sun-like star: ~60 Earth-mass, ~25 Neptune-mass, ~12 Jupiter-mass; nature rarely produces such tidy systems. Co-orbital rings (multiple bodies on one orbit) are dynamically possible but rare/natural vs engineered. **StarGen:** Planet count is emergent from orbit slots. Slots are generated with stability zones and spacing (see `OrbitalMechanics.calculate_minimum_planet_spacing`); there is no single “max = 17” cap. Constraint-based min/max planet counts (Solar system constraints effort) are for design/narrative, not physical law.
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

**Traveller-specific galaxy/subsector view and jump routes**
9. **Traveller subsector grid config:** Add a logical “Traveller grid” overlay for subsectors that can be configured per use case (e.g. 8×10, 10×10, 8×8×8). Keep the physical 3D model (10pc subsector cubes, parsec positions) unchanged and derive grid cells from local parsec-space coordinates.
10. **3D→2D projection:** Define a deterministic projection from 3D positions to 2D Traveller map coordinates using the existing convention (XZ = galactic plane, Y = height). Use X/Z for map columns/rows and treat Y as depth (slice index, inclusion band, or visual cue) so that multiple stars in a cell can be handled via primary/secondary world policies.
11. **Traveller subsector export:** For a chosen subsector and grid config, bin stars into grid cells, choose a primary system per cell (e.g. by proximity to midplane or population), and export a 2D subsector view (hex positions, names, UWP, trade codes, bases) suitable for on-screen display and printing.
12. **Traveller jump-route mode:** Add an alternative jump-lane calculator that uses Traveller-style distances and rules: integer parsec distances (from 3D positions or hex distance) with a max jump rating (e.g. J-4 or J-6), connecting nearby systems and building “mains” and spurs based on distance and importance, while still respecting the underlying 3D layout.
13. **2D jump-route rendering:** Project Traveller jump routes to the 2D subsector map (hex-to-hex lines), encoding jump distance in styles or labels (e.g. J-1…J-6), so printed maps show Traveller-flavoured routes that remain consistent with the 3D galaxy.

---

## Effort discipline

Proposed changes that do not fit any existing effort are added as a **new effort** in this roadmap. Each new effort should include: name, summary, gates (if any), deliverables, tests, and acceptance criteria. Do not implement until that effort is explicitly picked by a contributor.

---

## Additional efforts (to be considered later)

The following efforts were identified from research into **other RPGs** and **non-game use cases** beyond Traveller. They are listed in the efforts table above and expanded here for context. Consider when prioritising post–Traveller work.

### Other RPG and game-adjacent use cases

**Traveller-family (UWP-compatible):** **Cepheus Engine** and **Cepheus Light** use the same UWP structure and hex/subsector conventions. A full UWP and sector-file export (Traveller/Cepheus UWP full export) would serve both Traveller and Cepheus; tools like TravellerMap consume standard sector formats.

**Starfinder (Paizo):** “Building Worlds” uses world type (terrestrial, gas giant, irregular, satellite, asteroid, colony ship, station), gravity, atmosphere, biomes, and cultural attributes—no single UWP-style code. A **mapper** from StarGen body + population could produce Starfinder-style stats (Starfinder / other RPG export).

**Stars Without Number (Sine Nomine):** Sector and world generation with **world tags** (e.g. “Abandoned Colony”, “Xenophobes”) and tag-driven content. StarGen’s habitability, population, and atmosphere data can drive **suggested tags** and sector export (Stars Without Number export).

**Star Wars RPG and generic generators:** Template-based or simple-type systems (rocky, hospitable, gas giant) with a few stats. Same underlying data as StarGen; a thin export or preset could target these (covered under Starfinder / other RPG export or ad-hoc mappings).

### Non-game use cases

**Education / outreach:** Tools like **Gaia Sky** use procedural generation for astronomy education; workshops use “procedural playground”–style demos. StarGen’s deterministic, parameter-driven generation (spec + seed → body) fits **reproducible demos** and parameter exploration (Education / outreach mode).

**Science / classification:** The **Orion’s Arm** setting uses a **CADRS-style** classification (matrix of criteria, 0–F). Real exoplanet taxonomy (hot Jupiter, super-Earth, etc.) also exists. StarGen already has the data; an **extended or “science” export** (CADRS-like or exoplanet class) could support worldbuilding or teaching (Science / extended classification export).

**Fiction / worldbuilding:** Demand exists for compact codes (e.g. Star Trek–style class letters) or narrative summaries. UWP is one option; other encodings or prose summaries could be added as export views.

### Summary

| Effort | Use case | Fit |
|--------|----------|-----|
| Traveller/Cepheus UWP full export | Traveller, Cepheus, TravellerMap | Direct; build on Traveller alignment. |
| Starfinder / other RPG export | Starfinder, Star Wars, generic | Mapper from body + population. |
| Stars Without Number export | SWN sectors and tags | Derive tags from habitability/pop/atmosphere. |
| Education / outreach mode | Workshops, teaching, demos | Determinism + spec + seed. |
| Science / extended classification export | CADRS-like, exoplanet class, reference | Extended export from existing data. |

No implementation is implied; these are options for when contributors look beyond the current Traveller-focused work.

---

## Merged frameworks (reference)

**Population framework:** PlanetProfile, PopulationLikelihood, native populations, colonies, history; `src/domain/population/`; unit tests in `Tests/Unit/Population/`.

**Outposts and space stations:** StationGenerator, StationPlacementRules, OutpostAuthority; prototype at `src/app/prototypes/StationGeneratorPrototype.tscn`.

**Jump lanes (domain + prototype + galaxy viewer):** `src/domain/jumplanes/`, `src/app/jumplanes_prototype/`, and galaxy viewer (`SectorJumpLaneRenderer`, Calculate Jump Routes in `GalaxyInspectorPanel`, save/load in `GalaxySaveData`). Remaining work: see "Jump lanes optimization and polish" effort above.

**Civilisation / population detail (concepts):** `Concepts/Integration/` — Tech Tree, Regime Chart, and History sim with shared REGIMES/TRANSITIONS and tech-level–regime validity. `Concepts/HistoryGenerator/` — Culture sim, regime transitions, and map visualization. Reference for Population detail (civilisation/regime) effort. See `Docs/RegimeChangeModel.md`.

