## Loads all Phase 1 domain and service scripts so class_name types are registered.
## Preload this script in Phase 1 tests before they reference celestial types.
## Order: dependencies before dependents.
extends RefCounted

# Base types and utilities (no dependencies)
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _versions := preload("res://src/domain/constants/Versions.gd")

# Component props (leaf nodes, only self-references)
const _physical := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital := preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _atmosphere := preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _stellar := preload("res://src/domain/celestial/components/StellarProps.gd")
const _terrain := preload("res://src/domain/celestial/components/TerrainProps.gd")
const _hydrosphere := preload("res://src/domain/celestial/components/HydrosphereProps.gd")
const _cryosphere := preload("res://src/domain/celestial/components/CryosphereProps.gd")
const _ring_band := preload("res://src/domain/celestial/components/RingBand.gd")

# Components that depend on other components
const _ring_system := preload("res://src/domain/celestial/components/RingSystemProps.gd")  # depends on RingBand
const _surface := preload("res://src/domain/celestial/components/SurfaceProps.gd")  # depends on Terrain, Hydro, Cryo

# Higher-level domain objects
const _provenance := preload("res://src/domain/celestial/Provenance.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")  # depends on all components

# Validation (depends on CelestialBody)
const _validation_error := preload("res://src/domain/celestial/validation/ValidationError.gd")
const _validation_result := preload("res://src/domain/celestial/validation/ValidationResult.gd")  # depends on ValidationError
const _validator := preload("res://src/domain/celestial/validation/CelestialValidator.gd")  # depends on CelestialBody, ValidationResult, all components

# Serialization (depends on CelestialBody, all components)
const _serializer := preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")

# Services (depends on CelestialBody, CelestialSerializer)
const _persistence := preload("res://src/services/persistence/CelestialPersistence.gd")
