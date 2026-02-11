# Implementation Plan: feature/concepts Branch

**Execution order:** Stage 1.1 → 1.2 → 1.3 → (2.1–2.3, 3.1–3.2 in sequence) → 4 → 5 → 6.1–6.2 (can run in parallel with 2–3) → 6.3–6.7 → 7 → 8.

---

### Phase 1: Shader Foundation & Star Rendering

**Goal:** Shared shader infrastructure and stellar concept shader in the 3D viewer.

**Stage 1.1: Shader utility functions**

- **Create** `src/app/rendering/shaders/noise_lib.gdshaderinc`
  - Shared GLSL: simplex (3D), FBM, voronoi (for granulation/terrain).
  - Include from all body shaders to avoid duplication.
- **Add param derivation** (keep ~10 functions per script; use small modules if ColorUtils would grow too large):
  - Star: `StarShaderParams.gd` or `ColorUtils.get_star_shader_params(stellar, physical) -> Dictionary`
  - Wire: temperature, luminosity, radius, rotation, limb darkening, granulation/spot params, seed from provenance.

**Stage 1.2: Convert stellar concept to spatial**

- **Create** `src/app/rendering/shaders/star_surface.gdshader`
  - Port: blackbody color (wavelength-dependent), limb darkening, granulation (voronoi + flow), sunspots (umbra/penumbra), chromosphere rim, corona streamers, prominences/flares, diffraction spikes, bloom.
  - Uniforms from StellarProps/PhysicalProps via Stage 1.1.
- **Update** `MaterialFactory._create_star_material()` to use new shader and params.

**Stage 1.3: Star shader tests**

- **Create** `Tests/Unit/TestStarShaderParams.gd`: temperature → color, spectral class → granulation, determinism (same seed → same params), edge cases (O-type, M-type).

**Deliverables:** Shared noise lib; star spatial shader; star param tests.

---

### Phase 2: Terrestrial Planet Shader

**Stage 2.1: Terrestrial shader conversion**

- **Create** `src/app/rendering/shaders/planet_terrestrial_surface.gdshader`
  - Port: continental terrain (FBM, coherence), coastal detail, ocean (waves, Fresnel, specular, foam), polar ice caps, clouds with shadows, atmospheric scattering/limb darkening, optional city lights (from population), optional ring shadows (see Phase 5 note).
  - Uniforms from TerrainProps, HydrosphereProps, CryosphereProps, AtmosphereProps, PhysicalProps, SurfaceProps; seed from provenance.
- **Param derivation:** `TerrestrialShaderParams.gd` or `ColorUtils` helpers: terrain scale/height, continent size, sea level, ice cap, atmo density, cloud coverage, surface/ocean colors, axial tilt.
- **Update** `MaterialFactory._create_rocky_material()`: use new shader; fall back to StandardMaterial3D if body lacks required props.

**Stage 2.2: Terrestrial shader tests**

- **Create** `Tests/Unit/TestTerrestrialShaderParams.gd`: ocean world, desert, ice world, Earth-like; determinism.

---

### Phase 3: Gas Giant Shader

**Stage 3.1: Gas giant shader conversion**

- **Create** `src/app/rendering/shaders/planet_gas_giant_surface.gdshader`
  - Port: latitude bands, turbulence, zonal flow, storm systems, polar vortices, oblateness (vertex), atmospheric rim.
  - Uniforms: band count/contrast/turbulence, flow speed, storm intensity, oblateness, rotation, band colors from temperature; seed from provenance.
- **Param derivation:** `GasGiantShaderParams.gd` or ColorUtils: mass/rotation → band count; temperature → palette; age → storm intensity.
- **Update** `MaterialFactory._create_gas_giant_material()`.

**Stage 3.2: Gas giant shader tests**

- **Create** `Tests/Unit/TestGasGiantShaderParams.gd`: param derivation, determinism.

---

### Phase 4: Atmosphere & Ring Enhancements

**Atmosphere**

- Upgrade atmosphere material in MaterialFactory (or dedicated `atmosphere_spatial.gdshader` if needed): composition-based scattering color, pressure-based thickness, greenhouse inner glow, terminator. Document that full Rayleigh/Mie may need multi-pass later.

**Ring system**

- **Create/upgrade** ring shader: band-based density profile with gaps, noise variation, composition-based coloring, optical depth → opacity.
- **Document** ring shadow casting on planet (e.g. ring-plane intersection in planet shader or separate shadow pass); defer implementation.
- **Create** `Tests/Unit/TestRingShaderParams.gd`.

---

### Phase 5: Population Integration (Single Entry Point)

**Goal:** Wire population framework into planet/moon generation with one domain entry point and deterministic, order-independent seeding.

**Stage 5.1: Population probability (single module)**

- **Create** `src/domain/population/PopulationProbability.gd` (or retain name `PopulationIntegration.gd` for alignment with original plan)
  - `calculate_native_probability(profile: PlanetProfile) -> float`: habitability score, liquid water, temperature range, atmosphere, radiation, magnetic field, age, tidal locking penalty, moon bonus, subsurface ocean.
  - `should_generate_natives(profile: PlanetProfile, rng: SeededRng) -> bool`: deterministic roll using above probability.
  - Single entry for “if/how” population generates; extend later for colonies.

**Stage 5.2: Deterministic seeding**

- **Create** `src/domain/population/PopulationSeeding.gd`
  - `generate_population_seed(body_id: String, base_seed: int) -> int`: hash body_id + base_seed so same body + seed gives same population regardless of generation order.

**Stage 5.3: Generator integration**

- **Modify** `PlanetGenerator.gd`: after body generation, build PlanetProfile; call PopulationProbability; if `should_generate_natives`, generate native population with PopulationSeeding seed; attach result to body (see 5.4).
- **Modify** `MoonGenerator.gd`: same pattern; pass parent context; moon-specific factors (tidal heating, parent radiation, subsurface ocean).

**Stage 5.4: Storage and serialization**

- **Body storage:** Add `population_data: PlanetPopulationData` (optional, null if none) to `CelestialBody.gd`; or separate lookup by body_id. Document choice; recommend field for simplicity.
- **Update** `CelestialSerializer.gd`: serialize/deserialize population data; round-trip tests in Phase 8.

**Stage 5.5: System-level wiring**

- **Modify** `SystemPlanetGenerator.gd`: after `PlanetGenerator.generate()`, optionally run population generation; pass body + context; respect population flags from spec.
- **Modify** `SystemMoonGenerator.gd`: same for qualifying moons; use parent planet context.
- **Add population spec to** `SolarSystemSpec`: enable/disable population generation; optional population chance modifier.

**Stage 5.6: Population integration tests**

- **Create** `Tests/Unit/TestPopulationProbability.gd`, `Tests/Unit/TestPopulationSeeding.gd`, `Tests/Integration/TestPopulationGeneration.gd`: probability logic, determinism (same body_id + seed → same population), order independence, moon factors, uninhabitable → 0 probability.

---

### Phase 6: Population Display in Viewer

- **InspectorPanel.gd:** Add “Population” section when body has population data: profile summary (habitability, category), suitability category, native populations list (name, tech level, population, status), political situation; “No population” when uninhabited.
- **PropertyFormatter.gd** (or equivalent): format population numbers, tech levels, habitability/suitability categories.
- **SystemInspectorPanel:** Summary when body selected; “View Details” opens ObjectViewer.

---

### Phase 7: Testing & Polish

- **Unit tests:** All shader param modules (star, terrestrial, gas giant, ring); add new test scripts to `Tests/RunTestsHeadless.gd` and `Tests/TestScene.gd`.
- **Integration:** Population pipeline determinism; serialization round-trip including population data.
- **Golden masters:** Extend or add fixtures with population data; verify determinism across regeneration.

---

### File summary

**New:** `noise_lib.gdshaderinc`, `star_surface.gdshader`, `planet_terrestrial_surface.gdshader`, `planet_gas_giant_surface.gdshader`, ring shader; `StarShaderParams.gd` (or ColorUtils), `TerrestrialShaderParams.gd`, `GasGiantShaderParams.gd`; `PopulationProbability.gd` (or `PopulationIntegration.gd`), `PopulationSeeding.gd`; `TestStarShaderParams.gd`, `TestTerrestrialShaderParams.gd`, `TestGasGiantShaderParams.gd`, `TestRingShaderParams.gd`, `TestPopulationProbability.gd`, `TestPopulationSeeding.gd`, `TestPopulationGeneration.gd`.

**Modified:** `MaterialFactory.gd`, `BodyRenderer.gd`, `ColorUtils.gd` (if used for params); `CelestialBody.gd`, `CelestialSerializer.gd`; `PlanetGenerator.gd`, `MoonGenerator.gd`, `SystemPlanetGenerator.gd`, `SystemMoonGenerator.gd`; `SolarSystemSpec`; `InspectorPanel.gd`, `PropertyFormatter.gd`, SystemInspectorPanel; `RunTestsHeadless.gd`, `TestScene.gd`.
