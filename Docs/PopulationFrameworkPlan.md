# Population Framework Plan

This document describes the development plan for the **population framework** on the `population` branch. It aligns with the "Branch: population (parallel concept)" section in `Docs/Roadmap.md`. 

---

## Design Decisions

| Question | Decision | Impact |
| -------- | -------- | ------ |
| Time scale | Years from present (negative = past, positive = future) | `founding_year` / `established_year` / `origin_year` are `int`; year 0 = "now". |
| Native populations | Multiple co-existing allowed; all natives are human (simplifies biology) | `Array[NativePopulation]` per planet; no species variation in v1. |
| Colony founding civs | Placeholder unique ID, documented for future replacement | `founding_civilization_id: String` with TODO comment until Civilization model exists. |
| Gas giant moons | All moons colonizable; considered for colonization | Profile generator handles moons; no automatic exclusion. Moon-specific modifiers (tidal heating, parent radiation, eclipse factor) apply when context indicates a moon. |
| Habitability scale | 0–10 (0 = impossible, 10 = Earth-equivalent or better) | `habitability_score: int` 0–10 as source of truth. A **derived** habitability category (IMPOSSIBLE, HOSTILE, HARSH, MARGINAL, CHALLENGING, COMFORTABLE, IDEAL) is available for display/narrative. |

---

## Scope

**In scope:**

- Domain model and data structures for populations (native, colony) and history.
- **Planet Profile**: derived summary of a planet’s physical/surface/atmospheric properties used for suitability and population logic.
- **ProfileCalculations**: pure, testable functions that compute profile fields from existing `CelestialBody` / component data.
- **ColonySuitability**: a first-class concept computed from a Planet Profile (and optionally other inputs), with clear, testable rules.
- **NativePopulation**: indigenous or evolved population — origin, demographics, development, history.
- **Colony**: settled population — founding origin, date, relationship to natives, demographic/political evolution.
- **History**: events, migrations, conflicts, technological shifts; shared structure so both native and colonial timelines can be represented.
- Unit tests for all non-trivial logic; determinism tests where applicable.
- Ability to **run population code and tests separately** so the main test suite stays green and unblocked.

**Out of scope (deferred):**

- UI for population or colony management.
- Integration with galaxy/system viewer (beyond a clear boundary so we can attach later).
- Gameplay or narrative content; this plan is about data model and calculations only.

---

## Extension: Outposts and Space Stations (branch: outposts-and-spacestations)

This branch extends the population framework with **outposts** and **space stations**. Both are orbital or deep-space habitats rather than planetary populations, but they integrate with the population model (population counts, purposes, placement logic).

### Outposts

- **Definition:** Habitats with population **10,000 or less**.
- **Placement:**
  - **Bridge systems:** Frequently placed in small space stations in systems that serve as bridges or waypoints between regions (trade routes, choke points).
  - **Other locales:** Placed more or less randomly depending on the needs of the establishing faction:
    - Mining (resource extraction sites)
    - Science (research stations, observatories)
    - Spy bases (intelligence gathering)
    - Other functional purposes
- Outposts are typically small and purpose-driven.

### Space Stations

- **Definition:** Orbital or deep-space habitats that **can function as outposts** but may grow to **city-sized** populations.
- **Placement and roles:**
  - **Over native-populated planets:** Common over planets with native populations that have at least a **space-faring** level of technology. There may be several stations per planet.
  - **Over colony worlds:** Common over colony worlds, usually offering **functional services** to visiting ships: refuel, repair, and other amenities.
  - **Resource-rich systems without habitable planets:** In systems that are resource-rich but lack habitable planets, space stations are more likely to support **colony-sized** populations (the station becomes the primary settlement).

### Relationship to population framework

- Space stations and outposts use the same population concepts (counts, demographics, history) where applicable.
- Placement logic depends on system context (bridge vs colony vs resource-rich), native/colony population presence, and technology level.
- Determinism and RNG injection apply as for other population generation.

---

### Outposts-and-spacestations stage order

1. **Stage 1: Core enums** ✅
   - StationClass, StationType, StationPurpose, StationService, StationPlacementContext, OutpostAuthority.
   - Unit tests for all enums.
   - See `src/domain/population/Station*.gd`, `OutpostAuthority.gd`.

2. **Stage 2: Outpost and SpaceStation models**
   - Outpost data model (population ≤10k, purpose, placement, authority).
   - SpaceStation data model (can grow to city-sized, services, government, history for larger stations).
   - Serialization (to_dict/from_dict) for both.
   - Unit tests (creation, serialization, validation).

3. **Stage 3: Placement rules**
   - StationPlacementRules: pure functions for placement logic.
   - Bridge system detection (stub if galactic data not yet available).
   - Native/colony context, resource richness, technology level checks.
   - Unit tests for all rule branches.

4. **Stage 4: Station generator** ✅
   - StationGenerator: generates stations/outposts for a system given context and spec.
   - StationSpec for generation parameters.
   - Determinism tests.
   - Prototype: `src/app/prototypes/StationGeneratorPrototype.tscn` for interactive exploration.

5. **Stage 5: Integration**
   - System-level container for stations (SystemPopulationData or extension to SolarSystem).
   - Optional integration with PlanetPopulationData when station orbits a planet.

### Station-related file structure

- **Domain:** `src/domain/population/`
  - `StationClass.gd`, `StationType.gd`, `StationPurpose.gd`, `StationService.gd`, `StationPlacementContext.gd`, `OutpostAuthority.gd` ✅
  - `Outpost.gd`, `SpaceStation.gd` (Stage 2)
  - `StationPlacementRules.gd` (Stage 3) ✅
  - `StationGenerator.gd`, `StationSpec.gd` (Stage 4) ✅
- **Tests:** `Tests/Unit/Population/`
  - `TestStationClass.gd`, `TestStationType.gd`, etc. ✅
  - `TestOutpost.gd`, `TestSpaceStation.gd`, `TestStationPlacementRules.gd`, `TestStationSpec.gd`, `TestStationGenerator.gd` ✅

---

## Key Design Decisions

### 1. Planet Profile

- **Purpose:** A single, derived summary of a planet’s “habitability-related” and surface/atmosphere properties, used by suitability and population logic.
- **Source of truth:** All values are **derived** from existing domain types (e.g. `CelestialBody`, `PhysicalProps`, `SurfaceProps`, `AtmosphereProps`, `StellarProps` for parent star). No duplicate storage of raw physics in the profile.
- **Habitability:** Primary measure is **habitability_score: int** 0–10 (0 = impossible, 10 = Earth-equivalent or better), computed from explicit factors (temperature, pressure, water, gravity, breathability, radiation). A **derived** habitability category (e.g. IMPOSSIBLE, HOSTILE, HARSH, MARGINAL, CHALLENGING, COMFORTABLE, IDEAL) is provided for display and narrative; it is keyed off the 0–10 score, not stored separately.
- **Moons:** When the body is a moon, **moon-specific modifiers** apply: tidal heating factor, parent radiation exposure, and eclipse frequency. These are computed in ProfileCalculations (or a dedicated moon-modifier step) when parent context indicates a moon; they feed into radiation level, habitability, and (later) colony suitability where relevant.
- **Ownership:** Profile is produced by **ProfileCalculations** (see below). The profile is a data snapshot (Resource or class with typed fields), not the calculator.

### 2. ProfileCalculations

- **Purpose:** One place that knows how to build a Planet Profile from celestial/planetary data.
- **Location:** Domain layer (e.g. `src/domain/population/` or equivalent). No Nodes, no file I/O.
- **Signature style:** Functions take explicit inputs (body, parent star context, etc.) and return a Profile (or suitability). Same seed + same inputs → same outputs (determinism).
- **Testability:** All calculation logic is unit-tested against known celestial data; align with existing fixtures or minimal test bodies where helpful.

### 3. ColonySuitability

- **Purpose:** A first-class concept: “how suitable is this planet (or this profile) for colonization?”
- **Inputs:** Planet Profile (and optionally policy or scenario flags later).
- **Carrying-capacity** and **Compound Growth** for colony population over time is calculated against habitability and environmental challenges
- **Output:** A structured result (e.g. score + categories or factors), not a single magic number.
- **Location:** Domain; consumed by colony-related logic and (later) UI. Fully testable without the rest of the app.

### 4. NativePopulation vs Colony

- **NativePopulation:** Represents indigenous or evolved population. Fields: origin, demographics, cultural/technological development, **history** (timeline of events).
- **Colony:** Represents a settled population. Fields: founding origin (world or faction), founding date, **relationship to native populations** (if any), demographics, political evolution, **history** (timeline of events). Colony generation takes **existing_natives** as input so that colony–native relations (e.g. `native_relations` or `relationship_to_natives`: native population ID → relation type or score) are first-class from the start.
- **Shared:** Both use the same **history** framework (events, migrations, conflicts, tech shifts) so we can query “what happened when” for either.

### 5. History Framework

- **Purpose:** Reusable timeline of events for both native and colonial populations.
- **Design:** Ordered list of events with type, date, summary. Use **richer event-type granularity** (e.g. FOUNDING, NATURAL_DISASTER, PLAGUE, WAR, CIVIL_WAR, TECH_ADVANCEMENT, EXPANSION, POLITICAL_CHANGE, MIGRATION, COLLAPSE) so narratives stay coherent. **Event weights** when generating history should be **profile-driven** (e.g. higher disaster weight where volcanism/seismic/weather are high; higher conflict weight at higher tech and population) so timelines feel consistent with the planet.
- **Stage order:** Implement history support **before** or **with** native population, then reuse for colony so colony “history” is consistent.

---

## Stage Order

Suggested implementation order:

1. **Planet Profile + ProfileCalculations**
   - Define Profile data type (fields only).
   - Implement ProfileCalculations that take `CelestialBody` (and parent star context as needed) and return Profile.
   - Unit tests: known body → known profile; align with existing celestial types (see file alignment below).

2. **ColonySuitability**
   - Define ColonySuitability result type (e.g. score + factors).
   - Implement suitability from Profile (and optionally from raw body via Profile).
   - Unit tests: profile → suitability; edge cases (no atmosphere, extreme temp, etc.).

3. **History (minimal)**
   - Event type(s), timeline or ordered list, minimal payload (date, type, description/id).
   - No UI; domain-only. Tests: add events, order, query by time range.

4. **NativePopulation**
   - Data model: origin, demographics, development, history (reference to shared history).
   - No generator yet if preferred; can be “empty native” or “native from spec” in a later stage. Tests: creation, serialization, history attachment.

5. **Colony**
   - Data model: founding origin, date, relation to natives, demographics, evolution, history.
   - Optional: “founding” rule that uses ColonySuitability. Tests: creation, serialization, suitability usage.

6. **Integration boundary** ✅
   - **Typed pipeline result:** **PlanetPopulationData** is the return type of the full pipeline. It holds: body reference (or body_id), profile, native_populations, colonies, generation_seed (optional), and helpers such as `get_total_population()`, `get_dominant_population()`, and serialization (to_dict/from_dict). This gives a clear API for “given a CelestialBody (and context), produce profile + populations” Implemented in PlanetPopulationData.gd and PopulationGenerator.gd; tests in TestPlanetPopulationData.gd and TestPopulationGenerator.gd.

7. **Separate test runner / scene**
   - Population tests and/or a small runner scene that exercises Profile, Suitability, Native, Colony, History so that main suite stays green and population can be run in isolation.

---

## File and Folder Structure

- **Domain**
  - Population-related types live under a single domain subtree, e.g. `src/domain/population/`.
  - Suggested files (names are illustrative; follow project PascalCase):
    - `PlanetProfile.gd` — Profile data type (Resource or class with typed fields); includes habitability_score (int 0–10) and derived category helper.
    - `ProfileCalculations.gd` — Pure functions: body + context → Profile; includes moon-specific modifiers when context indicates a moon.
    - `ColonySuitability.gd` — Suitability result type and calculation from Profile.
    - `NativePopulation.gd` — Native population data model.
    - `Colony.gd` — Colony data model (takes existing_natives when generated; holds native_relations or equivalent).
    - `PopulationHistory.gd` or `HistoryEvent.gd` — Event types and timeline; richer EventType granularity; profile-driven event weights.
    - `PlanetPopulationData.gd` — Container for full pipeline result (profile, suitability, natives, colonies, helpers, serialization). ✅
    - `PopulationGenerator.gd` — Main entry point: body + context + spec → PlanetPopulationData; also generate_from_profile, generate_profile_only. ✅
- **Tests**
  - Unit tests for population in `Tests/Unit/` (e.g. `TestProfileCalculations.gd`, `TestColonySuitability.gd`, `TestNativePopulation.gd`, `TestColony.gd`, `TestPopulationHistory.gd`), or under a dedicated `Tests/Unit/Population/` subfolder if the project prefers.
  - Optional: dedicated runner scene or script that runs only population tests for live/exploratory runs (see Roadmap: “runnable separately”).

---

## Alignment With Existing Code

- **ProfileCalculations** should take the same types the rest of the app uses:
  - `CelestialBody` (and its components: `PhysicalProps`, `SurfaceProps`, `AtmosphereProps`, `StellarProps` for parent star, etc.).
- Refer to **`Docs/CelestialBodyProperties.md`** for the canonical list of properties and component nesting.
- **Validation/serialization:** Use existing validators and serializers where a body is loaded/saved; Profile and Suitability are derived, so they need their own serialization only if we persist them (e.g. saved colony state). No duplicate validation of core celestial fields in the profile layer.
- **Determinism:** All population calculations that depend on celestial data or RNG must use the project’s **injected RNG** and same inputs → same outputs; no global randomness.

---

## Testing Strategy

- **Unit tests:** Every non-trivial function in ProfileCalculations, ColonySuitability, and history/native/colony logic has tests. Prefer small, focused tests with explicit inputs and expected outputs.
- **Determinism:** For any path that uses RNG, add a test: same seed + same inputs → same Profile/Suitability/Population output.
- **Main suite:** The existing test suite (object, system, galaxy, etc.) continues to run unchanged and must stay green. New population tests are additive (and can live in a subfolder or behind a runner that is optional for CI until merge).
- **Separate runs:** Provide a way to run only population-related tests (or a single “population” test scene) so that work on the population branch doesn’t block main CI or local full runs.