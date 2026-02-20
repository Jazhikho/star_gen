## Represents a 100pc³ sector of the galaxy.
##
## Lazily generates and caches star data for all subsectors within.
## Provides efficient access to stars by subsector or for the whole sector.
## Pure data — no Nodes, no rendering.
class_name Sector
extends RefCounted

const _galaxy_coordinates: GDScript = preload("res://src/domain/galaxy/GalaxyCoordinates.gd")
const _seed_deriver: GDScript = preload("res://src/domain/galaxy/SeedDeriver.gd")
const _sub_sector_generator: GDScript = preload("res://src/domain/galaxy/SubSectorGenerator.gd")
const _galaxy_star: GDScript = preload("res://src/domain/galaxy/GalaxyStar.gd")


## Subsectors per edge (10x10x10 = 1000 subsectors per sector).
const SUBSECTORS_PER_EDGE: int = 10


## Reference to parent galaxy (for accessing density model and spec).
var _galaxy: WeakRef

## Quadrant grid coordinates.
var quadrant_coords: Vector3i

## Sector-local coordinates within quadrant (0-9 each axis).
var sector_local_coords: Vector3i

## World-space origin of this sector (min corner).
var world_origin: Vector3

## Deterministic seed for this sector.
var sector_seed: int

## Whether star data has been generated.
var _is_generated: bool = false

## All stars in this sector, indexed by subsector key.
## Key: "x,y,z" -> Array[GalaxyStar]
var _stars_by_subsector: Dictionary = {}

## Flat list of all stars (populated after generation).
var _all_stars: Array[GalaxyStar] = []


## Creates a new Sector.
## @param galaxy: Parent Galaxy instance.
## @param p_quadrant_coords: Quadrant grid coordinates.
## @param p_sector_local_coords: Sector position within quadrant.
func _init(galaxy: Galaxy, p_quadrant_coords: Vector3i, p_sector_local_coords: Vector3i) -> void:
	_galaxy = weakref(galaxy)
	quadrant_coords = p_quadrant_coords
	sector_local_coords = p_sector_local_coords
	world_origin = GalaxyCoordinates.sector_world_origin(quadrant_coords, sector_local_coords)
	sector_seed = SeedDeriver.derive_sector_seed_full(
		galaxy.galaxy_seed, quadrant_coords, sector_local_coords
	)


## Returns all stars in this sector, generating if needed.
## @return: Array of GalaxyStar instances.
func get_stars() -> Array[GalaxyStar]:
	_ensure_generated()
	return _all_stars


## Returns stars in a specific subsector.
## @param subsector_local_coords: Subsector position within sector (0-9 each axis).
## @return: Array of GalaxyStar instances in that subsector.
func get_stars_in_subsector(subsector_local_coords: Vector3i) -> Array[GalaxyStar]:
	_ensure_generated()
	var key: String = _subsector_key(subsector_local_coords)
	if _stars_by_subsector.has(key):
		return _stars_by_subsector[key] as Array[GalaxyStar]
	return [] as Array[GalaxyStar]


## Returns the total star count in this sector.
## @return: Number of stars.
func get_star_count() -> int:
	_ensure_generated()
	return _all_stars.size()


## Returns whether the sector has been generated.
## @return: True if generated.
func is_generated() -> bool:
	return _is_generated


## Forces regeneration of sector data (clears cache).
func regenerate() -> void:
	_is_generated = false
	_stars_by_subsector.clear()
	_all_stars.clear()
	_ensure_generated()


## Ensures star data is generated.
func _ensure_generated() -> void:
	if _is_generated:
		return

	var galaxy: Galaxy = _galaxy.get_ref() as Galaxy
	if galaxy == null:
		push_error("Sector: Parent galaxy reference is invalid")
		_is_generated = true
		return

	_generate_all_subsectors(galaxy)
	_is_generated = true


## Generates stars for all subsectors in this sector.
## @param galaxy: Parent galaxy for accessing density model.
func _generate_all_subsectors(galaxy: Galaxy) -> void:
	# Initialize subsector arrays
	for ssx in range(SUBSECTORS_PER_EDGE):
		for ssy in range(SUBSECTORS_PER_EDGE):
			for ssz in range(SUBSECTORS_PER_EDGE):
				var ss_local: Vector3i = Vector3i(ssx, ssy, ssz)
				var key: String = _subsector_key(ss_local)
				_stars_by_subsector[key] = [] as Array[GalaxyStar]

	# Generate using SubSectorGenerator
	var star_data: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		galaxy.galaxy_seed,
		quadrant_coords,
		sector_local_coords,
		galaxy.density_model,
		galaxy.reference_density
	)

	# Convert to GalaxyStar instances and sort into subsectors
	for i in range(star_data.get_count()):
		var pos: Vector3 = star_data.positions[i]
		var seed_val: int = star_data.star_seeds[i]

		var star: GalaxyStar = GalaxyStar.create_with_derived_properties(pos, seed_val, galaxy.spec)
		star._sector_quadrant = quadrant_coords
		star._sector_local = sector_local_coords

		# Determine which subsector this star belongs to
		var local_pos: Vector3 = pos - world_origin
		var ss_coords: Vector3i = Vector3i(
			clampi(int(local_pos.x / GalaxyCoordinates.SUBSECTOR_SIZE_PC), 0, 9),
			clampi(int(local_pos.y / GalaxyCoordinates.SUBSECTOR_SIZE_PC), 0, 9),
			clampi(int(local_pos.z / GalaxyCoordinates.SUBSECTOR_SIZE_PC), 0, 9)
		)
		star.subsector_coords = ss_coords

		var key: String = _subsector_key(ss_coords)
		(_stars_by_subsector[key] as Array[GalaxyStar]).append(star)
		_all_stars.append(star)


## Generates a cache key for a subsector.
## @param coords: Subsector local coordinates.
## @return: Key string.
func _subsector_key(coords: Vector3i) -> String:
	return "%d,%d,%d" % [coords.x, coords.y, coords.z]
