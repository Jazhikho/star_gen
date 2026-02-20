# StarGen — Generation Design Document (GDD)

This document is a **generation-focused reference** intended to support **refinement work**. It describes what StarGen can currently generate, the generation “types” at each level (object → system → galaxy), which knobs/specs exist, and the concrete calculations used.

Scope: **deterministic procedural generation** in `res://src/domain/` (plus how the app layers consume outputs at a high level). Rendering/editor UX is intentionally out of scope except where it affects generation inputs/outputs.

---

## Determinism and provenance (global rules)

### Deterministic randomness

- **Single RNG wrapper**: all generation randomness is expected to go through `SeededRng` (`src/domain/rng/SeededRng.gd`).
  - Constructed from an integer seed.
  - Exposes `randf()`, `randf_range()`, `randi()`, `randi_range()`, `randfn()` and `weighted_choice()`.
  - `fork()` creates a derived RNG using `randi()` (useful when you want a sub-generator not to affect the parent RNG’s sequence).

### Specs and overrides

- **Object-level specs** inherit from `BaseSpec` (`src/domain/generation/specs/BaseSpec.gd`):
  - `generation_seed: int`
  - `name_hint: String`
  - `overrides: Dictionary` of **field-path → locked value**.
    - Examples (field paths are generator-specific): `"physical.mass_kg"`, `"orbital.semi_major_axis_m"`, `"stellar.temperature_k"`.
  - Helper accessors: `has_override()`, `get_override()`, typed variants `get_override_float()`, `get_override_int()`, and mutators `set_override()`, `remove_override()`, `clear_overrides()`.

- **System-level spec**: `SolarSystemSpec` (`src/domain/system/SolarSystemSpec.gd`) has its own `overrides` dictionary (same concept; different spec type).

### Provenance

Generated entities store provenance (seed, generator version, schema version, timestamp, spec snapshot).

- **Celestial bodies**: `CelestialBody.provenance` (`src/domain/celestial/CelestialBody.gd`) is created in generator utilities (see `GeneratorUtils.create_provenance(...)` usage).
- **Solar systems**: `StellarConfigGenerator.generate(...)` explicitly builds system-level `Provenance` from `SolarSystemSpec.to_dict()`.

---

## Data model: what gets generated

### Level 1 output type: `CelestialBody`

`CelestialBody` (`src/domain/celestial/CelestialBody.gd`) is **composition-based** (optional components depending on type):

- **Always present**
  - `id: String`
  - `name: String`
  - `type: CelestialType.Type` (STAR / PLANET / MOON / ASTEROID)
  - `physical: PhysicalProps`
  - `provenance: Provenance`
- **Optional components**
  - `orbital: OrbitalProps` (null if not applicable)
  - `stellar: StellarProps` (stars only)
  - `surface: SurfaceProps` (null for bodies without a modeled surface, e.g. gas giants)
  - `atmosphere: AtmosphereProps` (null if no atmosphere)
  - `ring_system: RingSystemProps` (null unless generated)

### Level 2 output type: `SolarSystem`

`SolarSystem` (`src/domain/system/SolarSystem.gd`) is a container that holds:

- Bodies (`CelestialBody`) in an ID-addressable structure
- Star hierarchy (`SystemHierarchy` / `HierarchyNode`)
- Orbit hosts (`OrbitHost`) with stability zones and HZ/frost line boundaries
- Optional asteroid belts (`AsteroidBelt`) and major asteroids (also `CelestialBody`)
- System-level `provenance`

### Level 3 output type: “stars-in-space” data (galaxy sampling)

Galaxy generation uses the **Galaxy** data model (`src/domain/galaxy/Galaxy.gd`), which provides lazy sector and star generation. The primary output type is `GalaxyStar` (position, star_seed, metallicity, age bias). `Galaxy` and `GalaxySpec` use `galaxy_seed` for the master seed; population/station specs use `generation_seed`. Low-level sampling returns:

- `SubSectorGenerator.SectorStarData` (`src/domain/galaxy/SubSectorGenerator.gd`)
  - `positions: PackedVector3Array` (world-space parsec coordinates)
  - `star_seeds: PackedInt64Array` (deterministic per-star seed to generate systems on demand)

Systems are generated on demand via `GalaxySystemGenerator.generate_system(galaxy_star)`, using `star_seed` as the basis for `SolarSystemSpec.generation_seed`.

---

## Level 1: Individual celestial object generation

Level 1 produces one `CelestialBody` (or one component, in the case of ring systems), given:

- a **spec** (StarSpec / PlanetSpec / MoonSpec / AsteroidSpec / RingSystemSpec)
- a deterministic RNG (`SeededRng`)
- for non-stars, a `ParentContext` (stellar + orbit context; and for moons also parent-planet context)

### Shared dependency: `ParentContext`

`ParentContext` (`src/domain/generation/ParentContext.gd`) is the “no-object-reference” context used by object generators.

It supplies:

- **Stellar context**: `stellar_mass_kg`, `stellar_luminosity_watts`, `stellar_temperature_k`, `stellar_age_years`
- **Orbit context**: `orbital_distance_from_star_m`
- **Parent-body context (moons/rings)**: `parent_body_mass_kg`, `parent_body_radius_m`, `orbital_distance_from_parent_m`

Core calculations:

- **Hill sphere** (for parent body vs star): \(R_H = a \cdot (m/(3M))^{1/3}\)
- **Roche limit**: \(d \approx 2.44 R_p \cdot (\rho_p/\rho_s)^{1/3}\)
- **Equilibrium temperature** (Bond albedo \(A\)):
  - Uses Stefan–Boltzmann with absorbed luminosity:
  - \(T_{eq} = \left(\frac{L(1-A)}{16\pi\sigma a^2}\right)^{1/4}\)

### Generation types at Level 1

#### Stars (`StarGenerator`)

- **Entrypoint**: `StarGenerator.generate(spec, rng)` (`src/domain/generation/generators/StarGenerator.gd`)
- **Spec**: `StarSpec` (`src/domain/generation/specs/StarSpec.gd`) (supports “hints” like spectral class/subclass and optional age/metallicity)
- **Archetypes/tables**
  - `StarClass` enum (`src/domain/generation/archetypes/StarClass.gd`)
  - `StarTable` ranges and relations (`src/domain/generation/tables/StarTable.gd`)

**High-level steps**

1. **Spectral class**
   - If spec provides it, use it; otherwise weighted choice using `SPECTRAL_WEIGHTS` favoring M dwarfs.
2. **Subclass**
   - If spec provides it, clamp to 0–9; otherwise `rng.randi_range(0, 9)`.
3. **Mass (solar masses)**
   - Uses `StarTable.get_mass_range(class)` and `StarTable.interpolate_by_subclass(...)`, then ±5% variation.
   - Override path: `"physical.mass_solar"` (and also `"physical.mass_kg"` when building `PhysicalProps`).
4. **Luminosity (solar luminosities)**
   - `StarTable.luminosity_from_mass(mass_solar)` using \(L \propto M^{3.5}\), then ±10% variation.
   - Override path: `"stellar.luminosity_solar"`.
5. **Temperature (Kelvin)**
   - Interpolated from class/subclass temperature range; ±3% variation.
   - Override path: `"stellar.temperature_k"`.
6. **Radius (solar radii)**
   - Derived to be self-consistent with \(L\) and \(T\) (Stefan–Boltzmann in solar units):
   - \(R/R_\odot = \sqrt{L/L_\odot} \cdot (T_\odot/T)^2\), \(T_\odot=5778\)
   - Override path: `"physical.radius_solar"` (and `"physical.radius_m"` later).
7. **Age and metallicity**
   - Age sampled within `StarTable.get_lifetime_range(class)`, biased younger with `pow(raw, 0.7)`.
   - Metallicity sampled log-normal-ish: `exp(rng.randfn(0, 0.2))`, clamped [0.1, 3.0].
8. **Physical props**
   - Rotation period (10–50 days), axial tilt (0–30°), oblateness (~0–0.001), magnetic moment (1e22–1e26), internal heat (0 for stars; luminosity is modeled separately).
   - Override paths include:
     - `"physical.mass_kg"`, `"physical.radius_m"`, `"physical.rotation_period_s"`, `"physical.axial_tilt_deg"`, `"physical.oblateness"`, `"physical.magnetic_moment"`, `"physical.internal_heat_watts"`.
9. **Assemble `CelestialBody`**
   - Type STAR, `stellar` component set; ID can be overridden via `"id"`.

#### Planets (`PlanetGenerator`)

- **Entrypoint**: `PlanetGenerator.generate(spec, context, rng)` (`src/domain/generation/generators/PlanetGenerator.gd`)
- **Spec**: `PlanetSpec` (`src/domain/generation/specs/PlanetSpec.gd`)
- **Sub-generators**
  - `PlanetPhysicalGenerator` (`src/domain/generation/generators/planet/PlanetPhysicalGenerator.gd`)
  - `PlanetAtmosphereGenerator` (`src/domain/generation/generators/planet/PlanetAtmosphereGenerator.gd`)
  - `PlanetSurfaceGenerator` (`src/domain/generation/generators/planet/PlanetSurfaceGenerator.gd`)
- **Tables**
  - `SizeTable` (`src/domain/generation/tables/SizeTable.gd`)
  - `OrbitTable` (`src/domain/generation/tables/OrbitTable.gd`)
  - `OrbitZone` (`src/domain/generation/archetypes/OrbitZone.gd`)
  - `SizeCategory` (`src/domain/generation/archetypes/SizeCategory.gd`)

**High-level steps**

1. **Determine archetypes**
   - Size category: from spec if provided, else weighted choice (`SIZE_CATEGORY_WEIGHTS`).
   - Orbit zone: from spec if provided, else weighted choice (`ORBIT_ZONE_WEIGHTS`).
2. **Generate orbital elements** (`OrbitalProps`)
   - `semi_major_axis_m`:
     - Override path: `"orbital.semi_major_axis_m"`
     - If not overridden, `OrbitTable.random_distance(zone, stellar_luminosity, rng)` using log-uniform within zone-specific [min,max] scaled by \(\sqrt{L/L_\odot}\).
   - `eccentricity`:
     - Override path: `"orbital.eccentricity"`
     - Otherwise `OrbitTable.random_eccentricity(zone, rng)` (biased toward 0).
   - `inclination_deg`:
     - Override path: `"orbital.inclination_deg"`
     - Otherwise `OrbitTable.random_inclination(rng)` (biased low).
   - Angular elements (LAN/AoP/mean anomaly): random 0–360°, each overrideable via:
     - `"orbital.longitude_of_ascending_node_deg"`, `"orbital.argument_of_periapsis_deg"`, `"orbital.mean_anomaly_deg"`.
3. **Physical generation**
   - Delegated to `PlanetPhysicalGenerator.generate_physical_props(...)`.
   - Size category and orbital distance feed into rotation/tidal-lock checks and related calculations.
4. **Equilibrium temperature**
   - `equilibrium_temp_k = context.get_equilibrium_temperature_k(0.3)`
5. **Atmosphere decision + generation**
   - `PlanetAtmosphereGenerator.should_have_atmosphere(...)`
   - If yes: `PlanetAtmosphereGenerator.generate_atmosphere(...)` returns `AtmosphereProps`.
6. **Surface temperature**
   - `AtmosphereUtils.calculate_surface_temperature(equilibrium_temp_k, atmosphere)`
   - If atmosphere exists: `surface_temp = equilibrium_temp * greenhouse_factor`; otherwise equals equilibrium.
7. **Surface**
   - If `SizeCategory.is_rocky(size_cat)` → generate surface; otherwise surface is null (gas/ice giants treated as no solid surface).
8. **Rings**
   - Planet generator attempts rings if:
     - Spec override `"has_rings"` is set, otherwise only for gaseous categories (`SizeCategory.is_gaseous`).
   - Then `RingSystemGenerator.should_have_rings(physical, context, rng)` gates by mass thresholds and probability.
   - If yes: `RingSystemGenerator.generate(...)` returns `RingSystemProps`.
9. **Assemble `CelestialBody`**
   - Type PLANET; `orbital`, `atmosphere`, `surface`, and optional `ring_system` components set.

#### Moons (`MoonGenerator`)

- **Entrypoint**: `MoonGenerator.generate(spec, context, rng)` (`src/domain/generation/generators/MoonGenerator.gd`)
- **Spec**: `MoonSpec` (`src/domain/generation/specs/MoonSpec.gd`)
- Requires `ParentContext.has_parent_body()` (i.e., must include planet data).

**High-level steps**

1. Select size category (rocky-only categories; weights differ for `spec.is_captured`).
2. Generate orbital properties constrained by parent planet’s Hill sphere and Roche limit:
   - Uses:
     - Hill radius from `context.get_hill_sphere_radius_m()`
     - Roche limit from `context.get_roche_limit_m(estimated_density)`
   - Orbital distance chosen log-uniform between:
     - `min_distance = max(roche*1.5, 2× planet_radius)` (with fallback if invalid)
     - `max_distance = hill_radius × fraction` (0.5 prograde, 0.7 captured/retrograde in object-level generator)
3. Generate physical properties (`MoonPhysicalGenerator.generate_physical_props(...)`).
4. Compute tidal heating (`MoonPhysicalGenerator.calculate_tidal_heating(...)`) and add it to `internal_heat_watts`.
5. Atmosphere decision and optional generation (moons usually don’t retain atmospheres; logic lives in `MoonAtmosphereGenerator`).
6. Surface temperature from equilibrium + greenhouse (same as planets).
7. Surface generation (moons always get a surface component here) via `MoonSurfaceGenerator.generate_surface(...)`.
8. Assemble `CelestialBody` type MOON.

#### Asteroids (`AsteroidGenerator`)

- **Entrypoint**: `AsteroidGenerator.generate(spec, context, rng)` (`src/domain/generation/generators/AsteroidGenerator.gd`)
- **Spec**: `AsteroidSpec` (`src/domain/generation/specs/AsteroidSpec.gd`)

**High-level steps**

1. Asteroid type (C/S/M) via weighted choice, unless spec forces it.
2. Physical props:
   - Density range depends on type.
   - Mass is log-uniform; range depends on `spec.is_large` (Ceres-scale).
   - Radius derived from mass+density (sphere approximation): \(R = (3V/(4\pi))^{1/3}\).
   - Rotation period: size-based ranges, log-uniform; 30% chance to be negative (used here as “retrograde” marker).
   - Axial tilt uniform 0–180°.
   - Oblateness: more irregular for small bodies.
   - Magnetic moment near-zero; internal heat minimal (higher only for large).
3. Orbital props:
   - Default semi-major axis: log-uniform in a “main belt” range (2.1–3.3 AU), unless overridden.
   - Eccentricity biased low with max 0.3; inclination biased low with max 30°.
4. Equilibrium temperature from context, using type’s typical albedo.
5. Surface props:
   - Albedo range depends on type.
   - Terrain is cratered, no tectonics/erosion, roughness high.
6. Assemble `CelestialBody` type ASTEROID, **no atmosphere**.

#### Ring systems (`RingSystemGenerator`)

- **Entrypoint**:
  - `RingSystemGenerator.should_have_rings(planet_physical, context, rng) -> bool`
  - `RingSystemGenerator.generate(spec_or_null, planet_physical, context, rng) -> RingSystemProps`
  - File: `src/domain/generation/generators/RingSystemGenerator.gd`
- **Spec**: `RingSystemSpec` (`src/domain/generation/specs/RingSystemSpec.gd`)
  - If `spec == null`, a random spec is created: `RingSystemSpec.random(rng.randi())`.

**High-level steps**

1. Complexity level (TRACE/SIMPLE/COMPLEX) via weighted choice unless spec forces it.
2. Composition (icy vs rocky):
   - If spec forces, use it.
   - Otherwise compare distance to an adjusted ice line:
     - base ice line 2.7 AU, scaled by \(\sqrt{L/L_\odot}\).
3. Ring limits:
   - Inner: \(\approx 1.1 \times R_p\), and at least \(0.5 \times\) Roche.
   - Roche limit uses typical particle density (icy) and planet density.
   - Outer: \(2.5 \times\) Roche, capped by `0.3 × Hill` if parent-body context exists.
4. Bands:
   - Band count from `RingComplexity.get_band_count_range(complexity)`.
   - Multi-band systems generate resonance-jittered gap positions from `RESONANCE_FRACTIONS`, then fill bands between gaps.
   - Each band has optical depth, a composition dict, and particle size (log-uniform 1mm–10m).
5. Total mass:
   - Scaled from Saturn’s rings using ring area ratio and average optical depth ratio.
   - Multiplied by a composition density ratio if rocky.
   - Random variation factor 0.5–2.0.

---

## Level 2: Solar system generation

Level 2 creates a `SolarSystem` (stars + orbit hosts + bodies + belts) from a `SolarSystemSpec` and `SeededRng`.

### System generation “types”

At this level, the generation is staged. The important “types” are:

1. **Stellar configuration** (star count, star bodies, star hierarchy, orbit hosts)
2. **Orbit slot field** (candidate planet orbits per orbit host)
3. **Planet placement** (fill slots with planets)
4. **Moon placement** (add moons to planets)
5. **Asteroid belt placement** (optional; add belts + major asteroids)
6. **Validation + serialization + golden masters** (regression determinism)

### 2.1 Stellar configuration (`StellarConfigGenerator`)

- **Entrypoint**: `StellarConfigGenerator.generate(spec, rng)` (`src/domain/system/StellarConfigGenerator.gd`)
- **Inputs**
  - `SolarSystemSpec` (seed, star count bounds, spectral class hints, age/metallicity hints, include belts flag)
  - `SeededRng` (constructed from `spec.generation_seed` by the caller/orchestrator)

**Core steps**

1. Determine star count
   - Uses override `"star_count"` if present.
   - Else if `min==max`, use that.
   - Else weighted by exponential decay favoring 1-star systems: weight \(= 1/2^{(n-1)}\).
2. Generate star bodies
   - For each star:
     - `star_seed = rng.randi()`
     - `star_rng = SeededRng.new(star_seed)`
     - Build `StarSpec` with hints if supplied.
     - Call `StarGenerator.generate(star_spec, star_rng)`.
     - Force unique stable IDs: `star.id = "star_%d" % i`.
     - Default names: Primary for single; Greek letter names for multiples.
3. Build hierarchical binaries (`SystemHierarchy`)
   - Repeatedly pick two nodes at random and combine into a barycenter node until one root remains.
   - Binary separation:
     - Category weighted close/moderate/wide; separation is log-uniform within AU ranges.
     - When combining barycenters, enforce a wider outer separation (≥ 3× inner).
   - Binary eccentricity:
     - max eccentricity depends on separation (close/moderate/wide); biased low: `raw*raw*max`.
   - Orbital period:
     - `OrbitalMechanics.calculate_orbital_period(separation_m, combined_mass_kg)`.
4. Compute orbit hosts (`OrbitHost`)
   - For each hierarchy node:
     - Star nodes → **S-type** host: orbits around that star.
     - Barycenter nodes → **P-type** host: orbits around that barycenter.
   - Stability limits:
     - Inner stability:
       - S-type: \(3 \times R_\star\) (simple safety margin).
       - P-type: Holman & Wiegert critical ratio via `OrbitalMechanics.calculate_ptype_stability_limit(...)`.
     - Outer stability:
       - If node has parent barycenter, use `OrbitalMechanics.calculate_stype_stability_limit(...)` (currently reused as an outer bound).
       - Else defaults: 100 AU for S-type, 200 AU for P-type.
   - HZ/frost line:
     - `OrbitHost.calculate_zones()` uses host luminosity to compute:
       - `habitable_zone_inner_m`, `habitable_zone_outer_m`, `frost_line_m`.

### 2.2 Orbit slots (`OrbitSlotGenerator`)

- **Entrypoint**: `OrbitSlotGenerator.generate_all_slots(hosts, stars, hierarchy, rng)` (`src/domain/system/OrbitSlotGenerator.gd`)
- Produces: `Dictionary[node_id -> Array[OrbitSlot]]`

**Core ideas**

- Start just beyond `inner_stability_m` and `STAR_RADIUS_SAFETY_MARGIN × star_radius`.
- Generate outward until `outer_stability_m` or `MAX_SLOTS_PER_HOST`.
- Slot-to-slot spacing is resonance-based:
  - pick a resonance ratio from `OrbitalMechanics.get_common_resonance_ratios()`
  - compute next distance via `OrbitalMechanics.calculate_resonance_spacing(...)`
  - enforce minimum spacing (`MIN_SPACING_FACTOR`).
- Each slot stores:
  - zone classification (HOT / TEMPERATE / COLD) via host HZ and frost line
  - suggested eccentricity (increases with distance; biased low)
  - fill probability (exponential decay in AU, clamped to [0.02, 1.0])
  - stability flag (currently companion arrays are empty in `generate_all_slots`, so this is mostly “true” at the moment)

### 2.3 Planets in slots (`SystemPlanetGenerator`)

- **Entrypoint**: `SystemPlanetGenerator.generate(slots, orbit_hosts, stars, rng)` and `generate_targeted(...)`
  - File: `src/domain/system/SystemPlanetGenerator.gd`

**Core steps**

1. For each available slot:
   - Fill decision is probabilistic: `rng.randf() < slot.fill_probability`.
2. Size category selection by zone:
   - Zone-specific weight dictionaries (HOT/TEMPERATE/COLD) choose `SizeCategory`.
3. Create `PlanetSpec`
   - `planet_seed = rng.randi()`
   - `spec = PlanetSpec.new(planet_seed, size_category, slot.zone)`
   - Force orbital distance override: `"orbital.semi_major_axis_m" = slot.semi_major_axis_m`
   - If suggested eccentricity > 0, override `"orbital.eccentricity"`.
4. Create `ParentContext` for the slot
   - Uses orbit host combined mass/luminosity/temp and a “system age” taken from the first star’s `stellar.age_years` (fallback 4.6e9).
5. Generate planet body
   - `planet_rng = SeededRng.new(planet_seed)`
   - `PlanetGenerator.generate(spec, context, planet_rng)`
6. Post-processing
   - Force deterministic ID: `planet.id = "planet_%s" % slot.id`
   - Set `planet.orbital.parent_id = host.node_id`

### 2.4 Moons (`SystemMoonGenerator`)

- **Entrypoint**: `SystemMoonGenerator.generate(planets, orbit_hosts, stars, rng)` (`src/domain/system/SystemMoonGenerator.gd`)

**Core steps (high-level)**

1. Read primary star properties (mass/luminosity/temp/age) from `stars[0]` (with solar defaults).
2. For each planet:
   - Determine moon count from planet mass category with an overall probability gate.
   - Compute Hill sphere: `OrbitalMechanics.calculate_hill_sphere(planet_mass, stellar_mass, planet_distance)`.
   - Choose moon orbital distances within a fraction of Hill sphere.
   - Mark some outer moons as captured.
   - For each moon, create `MoonSpec` and `ParentContext.for_moon(...)` and call `MoonGenerator.generate(...)`.

### 2.5 Asteroid belts (`SystemAsteroidGenerator`)

- **Entrypoint**: `SystemAsteroidGenerator.generate(orbit_hosts, filled_slots, stars, rng)` (`src/domain/system/SystemAsteroidGenerator.gd`)

**Core steps**

1. For each orbit host:
   - Optionally generate:
     - inner (rocky/metallic) belt near frost line (`INNER_BELT_PROBABILITY`)
     - outer (icy/Kuiper-like) belt beyond outer planets (`OUTER_BELT_PROBABILITY`)
   - Uses filled slot distances to find gaps and avoid overlap.
2. For each belt:
   - Estimate mass (different ranges for inner vs outer).
   - Generate up to `MAX_MAJOR_ASTEROIDS` major asteroids using a power-law size distribution (alpha 2.5) and `AsteroidGenerator`.

---

## Level 3: Galactic scale generation (subsector sampling)

### Overview

Galaxy generation now includes a full data model for lazy generation and caching.

### Data model classes

- **Galaxy** (`src/domain/galaxy/Galaxy.gd`): Top-level container; holds config, spec, density model, lazy sectors, and optional system cache.
- **Sector** (`src/domain/galaxy/Sector.gd`): 100 pc³ region; contains stars grouped by subsector and a flat list.
- **GalaxyStar** (`src/domain/galaxy/GalaxyStar.gd`): Represents a star system entry with position, seed, and derived properties (metallicity, age bias).
- **GalaxySystemGenerator** (`src/domain/galaxy/GalaxySystemGenerator.gd`): Bridges galaxy layer to system layer; generates SolarSystem from GalaxyStar on demand.

### Class relationships

```
Galaxy (top-level container)
├── config: GalaxyConfig
├── spec: GalaxySpec
├── density_model: DensityModelInterface
├── _sectors: Dictionary[key -> Sector]  (lazy-loaded)
└── _systems_cache: Dictionary[star_seed -> SolarSystem]

Sector (100pc³ region)
├── quadrant_coords, sector_local_coords
├── _stars_by_subsector: Dictionary[key -> Array[GalaxyStar]]
└── _all_stars: Array[GalaxyStar]  (flat list)

GalaxyStar (star system entry)
├── position: Vector3 (world-space parsecs)
├── star_seed: int (for deterministic system generation)
├── metallicity: float (derived from position)
└── age_bias: float (derived from position)
```

### Lazy generation flow

1. **Query stars**: Call `Galaxy.get_stars_in_sector()` or `Galaxy.get_stars_in_subsector()`.
2. **Generate system on demand**: Use `GalaxySystemGenerator.generate_system(galaxy_star)` to produce a `SolarSystem`; cache optionally in `Galaxy`.

### Persistence

Galaxy state is persisted via `GalaxySaveData`:
- **Seed and config**: Stored and restored; allows deterministic regeneration.
- **View state**: Zoom level, selections, camera position.
- **Generated systems**: NOT persisted. Systems regenerate deterministically from star seeds on demand.

This keeps save files small while ensuring identical results across sessions.

### Integration with GalaxyViewer

`GalaxyViewer` creates and owns a `Galaxy` instance:
- Replaces direct `_density_model` and `_reference_density` fields
- All density queries go through `_galaxy.density_model`
- `get_galaxy()` accessor allows external access to the data model
- Changing galaxy seed creates a new `Galaxy` instance

### Hierarchy and seed chain

- Seeds are derived by `SeedDeriver` (`src/domain/galaxy/SeedDeriver.gd`) using `StableHash`:
  - `galaxy_seed → quadrant_seed → sector_seed → subsector_seed → star_seed`
- `SubSectorGenerator` uses:
  - `SeedDeriver.derive_sector_seed_full(...)`
  - `SeedDeriver.derive_subsector_seed(...)`
  - `SeedDeriver.derive_star_seed(...)`

### Subsector star placement (`SubSectorGenerator`)

- **Entrypoints**
  - `generate_sector_stars(galaxy_seed, quadrant_coords, sector_local_coords, density_model, reference_density)`
  - `generate_sector_with_border(...)`
  - `generate_single_subsector(galaxy_seed, world_origin, density_model, reference_density)`

**Core steps per subsector**

1. Derive `subsector_seed` from `sector_seed` and local coordinates.
2. Seed a Godot `RandomNumberGenerator` with `subsector_seed`.
3. Compute subsector center and query density:
   - `local_density = density_model.get_density(center)`
   - Normalize: `density_ratio = clamp(local_density / reference_density, 0, 10)`
4. Convert density to expected system count:
   - Uses solar neighborhood reference: ~0.004 systems/pc³.
   - Subsector volume is 10×10×10 = 1000 pc³, so baseline expectation is 4 systems/subsector.
   - `expected_stars = density_ratio × 4.0`
5. Sample actual count via Poisson (inverse transform).
6. For each generated star:
   - Derive `star_seed = SeedDeriver.derive_star_seed(subsector_seed, i)`
   - Place a uniform random position within the 10pc cube
   - Store `(position, star_seed)`

**What’s possible at this level today**

- You can generate a stable set of star positions + seeds for:
  - a single subsector
  - a sector (10×10×10 subsectors)
  - a sector plus a one-subsector-deep border shell (for rendering continuity)
- You can generate a `SolarSystem` deterministically on demand from any `star_seed` by using it as (or to derive) a `SolarSystemSpec.generation_seed`.

---

## What to refine next (generation-focused)

This section is intentionally practical: it lists “refinement levers” suggested by the current code structure.

- **Constraint-based system generation (Phase 8)**:
  - Current slot/planet filling is primarily probabilistic; constraints (exact counts, min/max, must-include templates, resonance locking) can be layered on `OrbitSlotGenerator` + `SystemPlanetGenerator.generate_targeted(...)`.
- **Companion perturbations in slot stability**:
  - `OrbitSlotGenerator.generate_all_slots(...)` currently passes empty companion arrays; feeding real companion masses/positions from `SystemHierarchy` would make `is_orbit_stable(...)` meaningful.
- **Galaxy → star properties coupling**:
  - Galaxy generation currently produces positions + seeds only; metallicity/age gradients can be applied by converting position to a “region” and biasing `SolarSystemSpec.system_age_years` / `system_metallicity`.

