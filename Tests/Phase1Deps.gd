## Loads all Phase 1 domain and service scripts so class_name types are registered.
## Preload this script in Phase 1 tests before they reference celestial types.
## Order: dependencies before dependents.
extends RefCounted

# Base types and utilities (no dependencies)
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _versions: GDScript = preload("res://src/domain/constants/Versions.gd")

# Component props (leaf nodes, only self-references)
const _physical: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _atmosphere: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _stellar: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _terrain: GDScript = preload("res://src/domain/celestial/components/TerrainProps.gd")
const _hydrosphere: GDScript = preload("res://src/domain/celestial/components/HydrosphereProps.gd")
const _cryosphere: GDScript = preload("res://src/domain/celestial/components/CryosphereProps.gd")
const _ring_band: GDScript = preload("res://src/domain/celestial/components/RingBand.gd")

# Components that depend on other components
const _ring_system: GDScript = preload("res://src/domain/celestial/components/RingSystemProps.gd") # depends on RingBand
const _surface: GDScript = preload("res://src/domain/celestial/components/SurfaceProps.gd") # depends on Terrain, Hydro, Cryo

# Higher-level domain objects
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd") # depends on all components

# Validation (depends on CelestialBody)
const _validation_error: GDScript = preload("res://src/domain/celestial/validation/ValidationError.gd")
const _validation_result: GDScript = preload("res://src/domain/celestial/validation/ValidationResult.gd") # depends on ValidationError
const _validator: GDScript = preload("res://src/domain/celestial/validation/CelestialValidator.gd") # depends on CelestialBody, ValidationResult, all components

# Serialization (depends on CelestialBody, all components)
const _serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")

# Services (depends on CelestialBody, CelestialSerializer)
const _persistence: GDScript = preload("res://src/services/persistence/CelestialPersistence.gd")

# Galaxy save/load (so GalaxyViewer and tests can use GalaxySaveData/GalaxyPersistence)
const _galaxy_viewer_deps: GDScript = preload("res://src/app/galaxy_viewer/GalaxyViewerDeps.gd")

# Phase 2: Generation infrastructure
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")
const _asteroid_type: GDScript = preload("res://src/domain/generation/archetypes/AsteroidType.gd")
const _ring_complexity: GDScript = preload("res://src/domain/generation/archetypes/RingComplexity.gd")
const _base_spec: GDScript = preload("res://src/domain/generation/specs/BaseSpec.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _size_table: GDScript = preload("res://src/domain/generation/tables/SizeTable.gd")
const _star_table: GDScript = preload("res://src/domain/generation/tables/StarTable.gd")
const _orbit_table: GDScript = preload("res://src/domain/generation/tables/OrbitTable.gd")
const _generator_utils: GDScript = preload("res://src/domain/generation/generators/GeneratorUtils.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")