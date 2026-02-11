## Core data model for a celestial body.
## Composed of optional components based on body type.
class_name CelestialBody
extends RefCounted


## Unique identifier for this body.
var id: String

## Display name of the body.
var name: String

## Type of celestial body (star, planet, moon, asteroid).
var type: CelestialType.Type

## Physical properties (required for all bodies).
var physical: PhysicalProps

## Orbital properties (null for system center / free-floating bodies).
var orbital: OrbitalProps

## Stellar properties (null for non-stars).
var stellar: StellarProps

## Surface properties (null for gas giants without solid surface).
var surface: SurfaceProps

## Atmospheric properties (null for bodies without atmosphere).
var atmosphere: AtmosphereProps

## Ring system properties (null for bodies without ring systems).
var ring_system: RingSystemProps

## Population data (null for bodies without population generation).
var population_data: PlanetPopulationData

## Generation provenance information.
var provenance: Provenance


## Creates a new CelestialBody instance.
## @param p_id: Unique identifier.
## @param p_name: Display name.
## @param p_type: Body type.
## @param p_physical: Physical properties.
## @param p_provenance: Generation provenance.
func _init(
	p_id: String = "",
	p_name: String = "",
	p_type: CelestialType.Type = CelestialType.Type.PLANET,
	p_physical: PhysicalProps = null,
	p_provenance: Provenance = null
) -> void:
	id = p_id
	name = p_name
	type = p_type
	physical = p_physical if p_physical else PhysicalProps.new()
	provenance = p_provenance
	orbital = null
	stellar = null
	surface = null
	atmosphere = null
	ring_system = null
	population_data = null


## Returns true if this body has orbital data.
## @return: True if orbital is not null.
func has_orbital() -> bool:
	return orbital != null


## Returns true if this body has stellar data.
## @return: True if stellar is not null.
func has_stellar() -> bool:
	return stellar != null


## Returns true if this body has surface data.
## @return: True if surface is not null.
func has_surface() -> bool:
	return surface != null


## Returns true if this body has atmospheric data.
## @return: True if atmosphere is not null.
func has_atmosphere() -> bool:
	return atmosphere != null


## Returns true if this body has ring data.
## @return: True if ring_system is not null.
func has_ring_system() -> bool:
	return ring_system != null


## Returns true if this body has population data.
## @return: True if population_data is not null.
func has_population_data() -> bool:
	return population_data != null


## Returns the body type as a string.
## @return: Human-readable type name.
func get_type_string() -> String:
	return CelestialType.type_to_string(type)
