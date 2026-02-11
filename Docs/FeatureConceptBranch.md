# Feature Concept Branch: Phase 5 + Population Integration

## Purpose

The **feature/concepts** branch focuses on **Phase 5: Object rendering v2** and integrates conditional population generation into the planet/moon generation pipeline. This branch serves as the staging area for:

1. **Visual upgrades** to the Object Viewer using concept shaders
2. **Planet profile–driven population** with probabilistic success/failure outcomes
3. **Moon support** for native and colony populations where appropriate

---

## Phase 5: Object Rendering v2

### Goal

Improve the visuals of the Object Viewer by adopting shaders and techniques from the standalone concept demos.

### Reference Materials

- **`Concepts/planetgenerator.html`** — Terrestrial and gas giant planet rendering with:
  - Continental terrain (FBM, sigmoid land coherence, coastal detail)
  - Ocean with waves, Fresnel, specular
  - Polar ice caps
  - Clouds with shadows
  - Atmospheric scattering and limb darkening
  - Ring systems (trace/simple/complex)
  - Planet profile stats (continents, habitability, etc.)

- **`Concepts/stargenerator.html`** — Stellar rendering with:
  - Spectral class presets (O/B/A/F/G/K/M)
  - Granulation, supergranulation, sunspots
  - Chromosphere, corona, prominences, flares
  - Black hole / remnant modes (accretion disk, jets, lensing)

### Implementation Approach

1. **Reuse concept shaders** — Port as much GLSL as possible from the HTML demos into Godot `.gdshader` files. The existing `planet_terrestrial.gdshader`, `planet_gas_giant_concept.gdshader`, and `stellar_concept.gdshader` are starting points.
2. **Wire to CelestialBody** — Drive shader uniforms from `PhysicalProps`, `StellarProps`, `TerrainProps`, `HydrosphereProps`, `AtmosphereProps`, etc., so generated objects render correctly.
3. **Maintain determinism** — All visual parameters must derive from deterministic generation (seed + spec); no runtime-only randomization that breaks save/load.

### Deliverables (Phase 5)

- Terrestrial planets: terrain, ocean, ice, clouds, atmosphere, rings
- Gas giants: bands, storms, oblateness, rings
- Stars: temperature-based color, limb darkening, granulation, optional sunspots
- Basic LOD or performance tuning so the viewer remains responsive

---

## Population Integration

### Overview

The planet generator will build a **PlanetProfile** and, when conditions allow, **conditionally activate** the population scripts. Not every body will have population; outcomes depend on habitability and desirability.

### Conditional Activation Rules

| Outcome | Description |
|--------|-------------|
| **No population** | Planet/moon never developed native life or was never colonized. |
| **Native population only** | Indigenous life arose and persists. |
| **Native population failed** | Life arose but went extinct (e.g., catastrophe, runaway climate). |
| **Colony only** | No natives; colonization succeeded. |
| **Colony failed** | Colonization attempted but failed. |
| **Colony after native failure** | Natives died; later colonization attempted (success or failure). |
| **Colony after prior colony failure** | Earlier colony failed; retry attempted (success or failure). |

### Probabilistic Model

- **Habitability** (from `PlanetProfile` / `HabitabilityCategory`) and **desirability** (from `ColonySuitability` / `SuitabilityCalculator`) drive the chances of:
  - Native life arising
  - Native population surviving vs. failing
  - Colonization being attempted
  - Colonization succeeding vs. failing
  - Retry after failure (native or colony)

- The RNG (injected, deterministic) must be used for all rolls.

### Moons as Population Candidates

- **Moons** that meet habitability/suitability thresholds (e.g., subsurface oceans, suitable temperature, atmosphere) should be eligible for:
  - Native population (if life could arise)
  - Colony (if desirability is sufficient)

- Integration points: `MoonGenerator`, `SystemMoonGenerator`, and the population generators must support moon context (parent planet, orbital parameters, tidal effects).

---

## Branch Scope Summary

| Area | Scope |
|------|-------|
| **Rendering** | Port and adapt shaders from `planetgenerator.html` and `stargenerator.html` into Object Viewer. |
| **Planet generator** | Build `PlanetProfile`; gate population scripts on habitability/desirability. |
| **Population** | Conditional activation with probabilities for no-pop, native-only, native-failed, colony-only, colony-failed, retry-after-failure. |
| **Moons** | Extend population eligibility to suitable moons. |

---

## Architecture Notes

- **Domain purity** — Population logic stays in `src/domain/population/`; no Godot Nodes or scene tree in domain.
- **Determinism** — All population outcomes must be reproducible from (seed, spec, parent context).
- **Tests** — Object generation, object viewer, and population tests all run in the main test suite.

---

## Minimal File List for Contributors

To successfully address the branch goals, contributors should read these files:

### Documentation (start here)
- `Docs/FeatureConceptBranch.md` — This document
- `Docs/Roadmap.md` — Phase 5 and population context

### Visual reference (inspiration)
- `Concepts/planetgenerator.html` — Planet shader logic and uniforms
- `Concepts/stargenerator.html` — Star shader logic and uniforms

### Rendering pipeline
- `src/app/rendering/BodyRenderer.gd` — Applies materials to celestial bodies
- `src/app/rendering/MaterialFactory.gd` — Creates materials from body data
- `src/app/rendering/shaders/planet_terrestrial.gdshader`
- `src/app/rendering/shaders/planet_gas_giant_concept.gdshader`
- `src/app/rendering/shaders/stellar_concept.gdshader`
- `src/app/viewer/ObjectViewer.gd` — Object Viewer scene controller

### Planet generation + population integration
- `src/domain/generation/generators/PlanetGenerator.gd` — Build profile; conditionally call population
- `src/domain/system/SystemPlanetGenerator.gd` — System-level planet generation
- `src/domain/system/SystemMoonGenerator.gd` — Moon generation; extend for population candidates
- `src/domain/population/PlanetProfile.gd`
- `src/domain/population/ProfileGenerator.gd`
- `src/domain/population/ProfileCalculations.gd`
- `src/domain/population/ColonySuitability.gd`
- `src/domain/population/SuitabilityCalculator.gd`
- `src/domain/population/PlanetPopulationData.gd`
- `src/domain/population/PopulationGenerator.gd`

### Data model (shader wiring)
- `src/domain/celestial/CelestialBody.gd`
- `src/domain/celestial/components/PhysicalProps.gd`
- `src/domain/celestial/components/StellarProps.gd`
- `src/domain/celestial/components/TerrainProps.gd`
- `src/domain/celestial/components/HydrosphereProps.gd`
- `src/domain/celestial/components/AtmosphereProps.gd`
