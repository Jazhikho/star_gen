## Represents a star system entry in the galaxy.
##
## Contains the star's position, seed, and optionally a cached
## reference to the generated SolarSystem. Also stores derived
## properties like metallicity based on galactic position.
## Pure data — no Nodes, no rendering.
class_name GalaxyStar
extends RefCounted

const _galaxy_coordinates: GDScript = preload("res://src/domain/galaxy/GalaxyCoordinates.gd")


## World-space position of this star in parsecs.
var position: Vector3

## Deterministic seed for generating this star's system.
var star_seed: int

## Galactic metallicity modifier (1.0 = solar, affects system generation).
## Derived from galactic position: higher near core, lower in halo.
var metallicity: float = 1.0

## Age bias factor (1.0 = normal, <1 = younger region, >1 = older region).
## Derived from galactic position.
var age_bias: float = 1.0

## Reference to parent sector (weak reference to avoid cycles).
var _sector_quadrant: Vector3i = Vector3i.ZERO
var _sector_local: Vector3i = Vector3i.ZERO

## Subsector local coordinates within the sector.
var subsector_coords: Vector3i = Vector3i.ZERO


## Creates a new GalaxyStar.
## @param p_position: World-space position in parsecs.
## @param p_seed: Deterministic seed for system generation.
func _init(p_position: Vector3, p_seed: int) -> void:
	position = p_position
	star_seed = p_seed


## Creates a GalaxyStar with metallicity and age derived from position.
## @param p_position: World-space position in parsecs.
## @param p_seed: Deterministic seed for system generation.
## @param galaxy_spec: Galaxy specification for deriving properties.
## @return: New GalaxyStar with derived properties.
static func create_with_derived_properties(
	p_position: Vector3,
	p_seed: int,
	galaxy_spec: GalaxySpec
) -> GalaxyStar:
	var star: GalaxyStar = GalaxyStar.new(p_position, p_seed)
	star._derive_properties_from_position(galaxy_spec)
	return star


## Derives metallicity and age bias from galactic position.
## @param galaxy_spec: Galaxy specification.
func _derive_properties_from_position(galaxy_spec: GalaxySpec) -> void:
	var r: float = sqrt(position.x * position.x + position.z * position.z)
	var h: float = absf(position.y)

	# Metallicity gradient: higher near core, drops with radius
	# Based on observed Milky Way metallicity gradient (~-0.05 dex/kpc)
	# Core: ~1.5 solar, Solar neighborhood (8kpc): 1.0, Outer disk: ~0.5
	var r_normalized: float = r / galaxy_spec.disk_scale_length_pc
	metallicity = _calculate_metallicity(r_normalized, h, galaxy_spec)

	# Age bias: older populations in bulge and halo, younger in disk
	age_bias = _calculate_age_bias(r_normalized, h, galaxy_spec)


## Calculates metallicity based on galactic position.
## @param r_normalized: Radial distance normalized by scale length.
## @param h: Height above disk plane in parsecs.
## @param galaxy_spec: Galaxy specification.
## @return: Metallicity relative to solar (1.0 = solar).
func _calculate_metallicity(r_normalized: float, h: float, galaxy_spec: GalaxySpec) -> float:
	# Radial gradient: exponential falloff from center
	var radial_factor: float = exp(-0.3 * r_normalized) + 0.3

	# Vertical gradient: lower metallicity in halo
	var h_normalized: float = h / galaxy_spec.bulge_height_pc
	var vertical_factor: float = exp(-0.5 * h_normalized)

	# Combine and clamp to reasonable range [0.1, 3.0]
	var raw_metallicity: float = radial_factor * vertical_factor * 1.2
	return clampf(raw_metallicity, 0.1, 3.0)


## Calculates age bias based on galactic position.
## @param r_normalized: Radial distance normalized by scale length.
## @param h: Height above disk plane in parsecs.
## @param galaxy_spec: Galaxy specification.
## @return: Age bias factor (1.0 = normal, >1 = older, <1 = younger).
func _calculate_age_bias(r_normalized: float, h: float, galaxy_spec: GalaxySpec) -> float:
	# Bulge stars are older on average
	var bulge_dist: float = sqrt(
		(position.x * position.x + position.z * position.z) / (galaxy_spec.bulge_radius_pc * galaxy_spec.bulge_radius_pc) +
		(position.y * position.y) / (galaxy_spec.bulge_height_pc * galaxy_spec.bulge_height_pc)
	)
	var bulge_factor: float = exp(-bulge_dist) * 0.5

	# Halo stars are older
	var h_normalized: float = h / galaxy_spec.bulge_height_pc
	var halo_factor: float = 0.3 * (1.0 - exp(-h_normalized))

	# Spiral arms have younger stars (active star formation)
	# Simplified: outer disk slightly younger on average
	var disk_factor: float = -0.2 * (1.0 - exp(-r_normalized * 0.5))

	return clampf(1.0 + bulge_factor + halo_factor + disk_factor, 0.5, 2.0)


## Returns the sector quadrant coords (set by Sector during generation).
## @return: Quadrant coordinates.
func get_sector_quadrant() -> Vector3i:
	return _sector_quadrant


## Returns the sector local coords within quadrant (set by Sector during generation).
## @return: Sector local coordinates.
func get_sector_local() -> Vector3i:
	return _sector_local


## Returns the distance from galactic center in parsecs.
## @return: Distance from origin.
func get_distance_from_center() -> float:
	return position.length()


## Returns the radial distance in the galactic plane.
## @return: Distance from Y axis.
func get_radial_distance() -> float:
	return sqrt(position.x * position.x + position.z * position.z)


## Returns the height above/below the galactic plane.
## @return: Y coordinate (signed).
func get_height() -> float:
	return position.y


## Returns a string representation for debugging.
## @return: Debug string.
func _to_string() -> String:
	return "GalaxyStar(seed=%d, pos=%s, Z=%.2f)" % [star_seed, position, metallicity]
