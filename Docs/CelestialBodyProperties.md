# Celestial Body Properties and Locations

This document lists all properties of the celestial body model and where they are defined in the codebase. Use it as a reference when attaching new systems (e.g. population) to planets or when working with body data without opening every component file.

**Root model:** `src/domain/celestial/CelestialBody.gd`

---

## 1. CelestialBody (root)

Top-level container. Components are optional based on body type (star, planet, moon, asteroid).

| Property   | Type                 | Location | Notes |
|-----------|----------------------|----------|--------|
| `id`      | String               | CelestialBody.gd | Unique identifier. |
| `name`    | String               | CelestialBody.gd | Display name. |
| `type`    | CelestialType.Type   | CelestialBody.gd | STAR, PLANET, MOON, ASTEROID. |
| `physical`| PhysicalProps        | CelestialBody.gd | Required for all bodies. |
| `orbital` | OrbitalProps         | CelestialBody.gd | Null for system center / free-floating. |
| `stellar` | StellarProps         | CelestialBody.gd | Null for non-stars. |
| `surface` | SurfaceProps        | CelestialBody.gd | Null for gas giants without solid surface. |
| `atmosphere` | AtmosphereProps  | CelestialBody.gd | Null for no atmosphere. |
| `ring_system` | RingSystemProps  | CelestialBody.gd | Null for no rings. |
| `provenance` | Provenance       | CelestialBody.gd | Generation metadata. |

**File:** `src/domain/celestial/CelestialBody.gd`

---

## 2. CelestialType

Enumeration of body types.

| Value     | Meaning   |
|----------|-----------|
| STAR     | Star      |
| PLANET   | Planet    |
| MOON     | Moon      |
| ASTEROID | Asteroid  |

**File:** `src/domain/celestial/CelestialType.gd`

---

## 3. Provenance

Generation and version metadata for reproducibility and migration.

| Property            | Type    | Location   | Notes |
|--------------------|---------|------------|--------|
| `generation_seed`  | int     | Provenance.gd | Seed used to generate this object. |
| `generator_version`| String  | Provenance.gd | Generator version string. |
| `schema_version`   | int     | Provenance.gd | Serialization schema version. |
| `created_timestamp`| int     | Provenance.gd | Unix timestamp of creation. |
| `spec_snapshot`    | Dictionary | Provenance.gd | Optional spec used for generation. |

**File:** `src/domain/celestial/Provenance.gd`

---

## 4. PhysicalProps

Required for all bodies. Mass, size, rotation, and derived quantities.

| Property              | Type  | Location      | Notes |
|-----------------------|-------|---------------|--------|
| `mass_kg`             | float | PhysicalProps.gd | Mass in kilograms. |
| `radius_m`            | float | PhysicalProps.gd | Radius in meters (nominal). |
| `rotation_period_s`   | float | PhysicalProps.gd | Rotation period in seconds; negative = retrograde. |
| `axial_tilt_deg`      | float | PhysicalProps.gd | Axial tilt in degrees (0–180). |
| `oblateness`          | float | PhysicalProps.gd | Flattening (0 = sphere; e.g. Jupiter ~0.065). |
| `magnetic_moment`     | float | PhysicalProps.gd | Magnetic dipole moment in T·m³. |
| `internal_heat_watts`| float | PhysicalProps.gd | Internal heat flow in watts. |

**Derived (methods):** `get_volume_m3()`, `get_density_kg_m3()`, `get_surface_gravity_m_s2()`, `get_escape_velocity_m_s()`, `get_equatorial_radius_m()`, `get_polar_radius_m()`.

**File:** `src/domain/celestial/components/PhysicalProps.gd`

---

## 5. OrbitalProps

Keplerian orbital elements. Present when the body orbits another.

| Property                          | Type  | Location      | Notes |
|-----------------------------------|-------|---------------|--------|
| `semi_major_axis_m`               | float | OrbitalProps.gd | Semi-major axis in meters. |
| `eccentricity`                    | float | OrbitalProps.gd | 0 = circular, 0–1 = elliptical. |
| `inclination_deg`                 | float | OrbitalProps.gd | Orbital inclination in degrees. |
| `longitude_of_ascending_node_deg` | float | OrbitalProps.gd | Longitude of ascending node. |
| `argument_of_periapsis_deg`       | float | OrbitalProps.gd | Argument of periapsis. |
| `mean_anomaly_deg`                | float | OrbitalProps.gd | Mean anomaly at epoch. |
| `parent_id`                       | String| OrbitalProps.gd | ID of parent body; empty if none. |

**Derived (methods):** `get_periapsis_m()`, `get_apoapsis_m()`, `get_orbital_period_s(parent_mass_kg)`.

**File:** `src/domain/celestial/components/OrbitalProps.gd`

---

## 6. StellarProps

Star-only. Luminosity, spectral class, and evolution.

| Property                  | Type  | Location      | Notes |
|---------------------------|-------|---------------|--------|
| `luminosity_watts`        | float | StellarProps.gd | Luminosity in watts. |
| `effective_temperature_k` | float | StellarProps.gd | Photosphere temperature in Kelvin. |
| `spectral_class`          | String| StellarProps.gd | e.g. "G2V", "M5V", "K0III". |
| `stellar_type`            | String| StellarProps.gd | Category (e.g. main_sequence). |
| `metallicity`             | float | StellarProps.gd | Relative to solar (Sun = 1.0). |
| `age_years`               | float | StellarProps.gd | Age in years. |

**Derived (methods):** `get_luminosity_solar()`, `get_habitable_zone_inner_m()`, `get_habitable_zone_outer_m()`, `get_frost_line_m()`, `get_spectral_letter()`, `get_luminosity_class()`.

**File:** `src/domain/celestial/components/StellarProps.gd`

---

## 7. SurfaceProps

Planets, moons, asteroids. Temperature, albedo, surface type, and nested terrain/hydro/cryo.

| Property               | Type           | Location      | Notes |
|------------------------|----------------|---------------|--------|
| `temperature_k`        | float          | SurfaceProps.gd | Surface temperature in Kelvin. |
| `albedo`               | float          | SurfaceProps.gd | Bond albedo (0–1). |
| `surface_type`         | String         | SurfaceProps.gd | Identifier for rendering/classification. |
| `volcanism_level`      | float          | SurfaceProps.gd | 0 = none, 1 = highly active. |
| `surface_composition`  | Dictionary     | SurfaceProps.gd | Material → mass fraction. |
| `terrain`              | TerrainProps   | SurfaceProps.gd | Null for gas giants/stars. |
| `hydrosphere`          | HydrosphereProps | SurfaceProps.gd | Null if no liquid water. |
| `cryosphere`           | CryosphereProps | SurfaceProps.gd | Null if no significant ice. |

**File:** `src/domain/celestial/components/SurfaceProps.gd`

---

## 8. TerrainProps

Nested under `SurfaceProps.terrain`. Geological and surface features.

| Property           | Type  | Location      | Notes |
|--------------------|-------|---------------|--------|
| `elevation_range_m`| float | TerrainProps.gd | Max elevation range (peak to valley) in meters. |
| `roughness`        | float | TerrainProps.gd | 0 = smooth, 1 = extremely rough. |
| `crater_density`   | float | TerrainProps.gd | 0 = none, 1 = heavily cratered. |
| `tectonic_activity`| float | TerrainProps.gd | 0 = dead, 1 = highly active. |
| `erosion_level`    | float | TerrainProps.gd | 0 = pristine, 1 = heavily eroded. |
| `terrain_type`     | String| TerrainProps.gd | Classification for rendering. |

**File:** `src/domain/celestial/components/TerrainProps.gd`

---

## 9. HydrosphereProps

Nested under `SurfaceProps.hydrosphere`. Liquid water coverage and properties.

| Property         | Type  | Location         | Notes |
|------------------|-------|------------------|--------|
| `ocean_coverage` | float | HydrosphereProps.gd | Fraction of surface covered by liquid water (0–1). |
| `ocean_depth_m`  | float | HydrosphereProps.gd | Average ocean depth in meters. |
| `ice_coverage`   | float | HydrosphereProps.gd | Fraction of water surface covered by ice (0–1). |
| `salinity_ppt`   | float | HydrosphereProps.gd | Salinity in parts per thousand (Earth ~35). |
| `water_type`     | String| HydrosphereProps.gd | Composition type identifier. |

**File:** `src/domain/celestial/components/HydrosphereProps.gd`

---

## 10. CryosphereProps

Nested under `SurfaceProps.cryosphere`. Ice caps, permafrost, subsurface ocean.

| Property                   | Type  | Location         | Notes |
|----------------------------|-------|------------------|--------|
| `polar_cap_coverage`      | float | CryosphereProps.gd | Fraction of surface covered by polar ice (0–1). |
| `permafrost_depth_m`      | float | CryosphereProps.gd | Permafrost layer depth in meters. |
| `has_subsurface_ocean`    | bool  | CryosphereProps.gd | Subsurface liquid ocean present. |
| `subsurface_ocean_depth_m`| float | CryosphereProps.gd | Subsurface ocean depth in meters. |
| `cryovolcanism_level`     | float | CryosphereProps.gd | 0 = none, 1 = highly active. |
| `ice_type`                | String| CryosphereProps.gd | Ice composition type. |

**File:** `src/domain/celestial/components/CryosphereProps.gd`

---

## 11. AtmosphereProps

Optional. Surface pressure, scale height, composition, greenhouse effect.

| Property              | Type      | Location        | Notes |
|-----------------------|-----------|-----------------|--------|
| `surface_pressure_pa` | float     | AtmosphereProps.gd | Surface pressure in Pascals. |
| `scale_height_m`       | float     | AtmosphereProps.gd | Atmospheric scale height in meters. |
| `composition`          | Dictionary| AtmosphereProps.gd | Gas name → fraction (sum ~1.0). |
| `greenhouse_factor`    | float     | AtmosphereProps.gd | 1.0 = no effect, >1.0 = warming. |

**File:** `src/domain/celestial/components/AtmosphereProps.gd`

---

## 12. RingSystemProps

Optional. Multiple bands; each band is a RingBand.

| Property        | Type            | Location        | Notes |
|-----------------|-----------------|-----------------|--------|
| `bands`         | Array[RingBand] | RingSystemProps.gd | Bands from inner to outer. |
| `total_mass_kg` | float           | RingSystemProps.gd | Total ring mass in kilograms. |
| `inclination_deg` | float         | RingSystemProps.gd | Ring plane inclination in degrees. |

**File:** `src/domain/celestial/components/RingSystemProps.gd`

---

## 13. RingBand

Single band within a ring system. Referenced by RingSystemProps.bands.

| Property        | Type      | Location  | Notes |
|-----------------|-----------|-----------|--------|
| `inner_radius_m`| float     | RingBand.gd | Inner radius in meters. |
| `outer_radius_m`| float     | RingBand.gd | Outer radius in meters. |
| `optical_depth` | float     | RingBand.gd | 0 = transparent, >1 = opaque. |
| `composition`   | Dictionary| RingBand.gd | Material name → mass fraction. |
| `particle_size_m` | float  | RingBand.gd | Median particle size in meters. |
| `name`          | String    | RingBand.gd | Optional band name. |

**File:** `src/domain/celestial/components/RingBand.gd`

---

## Component nesting summary

```
CelestialBody
├── physical: PhysicalProps          (required)
├── orbital: OrbitalProps             (optional)
├── stellar: StellarProps             (optional, stars)
├── surface: SurfaceProps             (optional)
│   ├── terrain: TerrainProps         (optional)
│   ├── hydrosphere: HydrosphereProps (optional)
│   └── cryosphere: CryosphereProps   (optional)
├── atmosphere: AtmosphereProps       (optional)
├── ring_system: RingSystemProps     (optional)
│   └── bands: Array[RingBand]
└── provenance: Provenance
```

---

## Validation and serialization

- **Validation:** `src/domain/celestial/validation/CelestialValidator.gd` — validates bodies and components; uses `ValidationResult` and `ValidationError` from the same folder.
- **Serialization:** `src/domain/celestial/serialization/CelestialSerializer.gd` — body/component ↔ JSON; each component has `to_dict()` and `static from_dict(data)`.

All component paths above are under `src/domain/celestial/` or `src/domain/celestial/components/`.
