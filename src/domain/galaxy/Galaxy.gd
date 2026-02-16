## Top-level container for a procedurally generated galaxy.
##
## Manages sectors, lazy generation of star systems, and provides
## a single entry point for querying systems in a region.
## Pure data — no Nodes, no rendering.
class_name Galaxy
extends RefCounted

const _galaxy_spec: GDScript = preload("res://src/domain/galaxy/GalaxySpec.gd")
const _galaxy_config: GDScript = preload("res://src/domain/galaxy/GalaxyConfig.gd")
const _density_model_interface: GDScript = preload("res://src/domain/galaxy/DensityModelInterface.gd")
const _galaxy_coordinates: GDScript = preload("res://src/domain/galaxy/GalaxyCoordinates.gd")
const _seed_deriver: GDScript = preload("res://src/domain/galaxy/SeedDeriver.gd")
const _sector_class: GDScript = preload("res://src/domain/galaxy/Sector.gd")
const _galaxy_star: GDScript = preload("res://src/domain/galaxy/GalaxyStar.gd")


## The galaxy's master seed.
var seed: int

## The galaxy specification (derived from config + seed).
var spec: GalaxySpec

## The galaxy configuration used for generation.
var config: GalaxyConfig

## The density model for this galaxy type.
var density_model: DensityModelInterface

## Reference density at solar-neighborhood-equivalent position.
var reference_density: float

## Cached sectors (key: "qx,qy,qz:sx,sy,sz" -> Sector).
var _sectors: Dictionary = {}

## Cached generated solar systems (key: star_seed -> SolarSystem).
var _systems_cache: Dictionary = {}


## Creates a new Galaxy from a config and seed.
## @param p_config: Galaxy configuration parameters.
## @param p_seed: Master seed for the galaxy.
func _init(p_config: GalaxyConfig, p_seed: int) -> void:
	seed = p_seed
	config = p_config if p_config != null else GalaxyConfig.create_default()
	spec = GalaxySpec.create_from_config(config, seed)
	density_model = DensityModelInterface.create_for_spec(spec)
	reference_density = _compute_reference_density()


## Creates a Galaxy with default Milky Way-like configuration.
## @param p_seed: Master seed for the galaxy.
## @return: New Galaxy instance.
static func create_default(p_seed: int) -> Galaxy:
	return Galaxy.new(GalaxyConfig.create_milky_way(), p_seed)


## Computes the reference density at solar-neighborhood-equivalent radius.
## @return: Density value at ~8kpc from center.
func _compute_reference_density() -> float:
	var solar_radius_pc: float = 8000.0
	return density_model.get_density(Vector3(solar_radius_pc, 0.0, 0.0))


## Returns the sector at the given coordinates, generating if needed.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector position within quadrant (0-9 each axis).
## @return: The Sector instance.
func get_sector(quadrant_coords: Vector3i, sector_local_coords: Vector3i) -> Sector:
	var key: String = _sector_key(quadrant_coords, sector_local_coords)
	if _sectors.has(key):
		return _sectors[key] as Sector

	var sector: Sector = Sector.new(self, quadrant_coords, sector_local_coords)
	_sectors[key] = sector
	return sector


## Returns the sector containing a world-space position.
## @param position: World-space position in parsecs.
## @return: The Sector instance.
func get_sector_at_position(position: Vector3) -> Sector:
	var hierarchy: GalaxyCoordinates.HierarchyCoords = GalaxyCoordinates.parsec_to_hierarchy(position)
	return get_sector(hierarchy.quadrant_coords, hierarchy.sector_local_coords)


## Returns all stars in a sector (lazy-generates if needed).
## This is the primary entry point for getting systems in a region.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector position within quadrant.
## @return: Array of GalaxyStar instances.
func get_stars_in_sector(quadrant_coords: Vector3i, sector_local_coords: Vector3i) -> Array[GalaxyStar]:
	var sector: Sector = get_sector(quadrant_coords, sector_local_coords)
	return sector.get_stars()


## Returns all stars in a subsector.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector position within quadrant.
## @param subsector_local_coords: Subsector position within sector (0-9 each axis).
## @return: Array of GalaxyStar instances.
func get_stars_in_subsector(
	quadrant_coords: Vector3i,
	sector_local_coords: Vector3i,
	subsector_local_coords: Vector3i
) -> Array[GalaxyStar]:
	var sector: Sector = get_sector(quadrant_coords, sector_local_coords)
	return sector.get_stars_in_subsector(subsector_local_coords)


## Returns all stars within a radius of a position.
## Useful for jump-lane calculations and local queries.
## @param center: Center position in parsecs.
## @param radius_pc: Search radius in parsecs.
## @return: Array of GalaxyStar instances within radius.
func get_stars_in_radius(center: Vector3, radius_pc: float) -> Array[GalaxyStar]:
	var result: Array[GalaxyStar] = []

	# Determine which sectors we need to check
	var min_pos: Vector3 = center - Vector3.ONE * radius_pc
	var max_pos: Vector3 = center + Vector3.ONE * radius_pc

	var min_hierarchy: GalaxyCoordinates.HierarchyCoords = GalaxyCoordinates.parsec_to_hierarchy(min_pos)
	var max_hierarchy: GalaxyCoordinates.HierarchyCoords = GalaxyCoordinates.parsec_to_hierarchy(max_pos)

	# Iterate through potentially affected quadrants and sectors
	for qx in range(min_hierarchy.quadrant_coords.x, max_hierarchy.quadrant_coords.x + 1):
		for qy in range(min_hierarchy.quadrant_coords.y, max_hierarchy.quadrant_coords.y + 1):
			for qz in range(min_hierarchy.quadrant_coords.z, max_hierarchy.quadrant_coords.z + 1):
				var quadrant: Vector3i = Vector3i(qx, qy, qz)

				# Determine sector range within this quadrant
				var s_min: Vector3i = Vector3i.ZERO
				var s_max: Vector3i = Vector3i(9, 9, 9)

				if quadrant == min_hierarchy.quadrant_coords:
					s_min = min_hierarchy.sector_local_coords
				if quadrant == max_hierarchy.quadrant_coords:
					s_max = max_hierarchy.sector_local_coords

				for sx in range(s_min.x, s_max.x + 1):
					for sy in range(s_min.y, s_max.y + 1):
						for sz in range(s_min.z, s_max.z + 1):
							var sector_local: Vector3i = Vector3i(sx, sy, sz)
							var stars: Array[GalaxyStar] = get_stars_in_sector(quadrant, sector_local)

							for star in stars:
								if star.position.distance_to(center) <= radius_pc:
									result.append(star)

	return result


## Caches a generated solar system.
## @param star_seed: The star's seed (used as cache key).
## @param system: The generated SolarSystem.
func cache_system(star_seed: int, system: SolarSystem) -> void:
	_systems_cache[star_seed] = system


## Returns a cached solar system, or null if not cached.
## @param star_seed: The star's seed.
## @return: Cached SolarSystem or null.
func get_cached_system(star_seed: int) -> SolarSystem:
	return _systems_cache.get(star_seed) as SolarSystem


## Returns whether a system is cached.
## @param star_seed: The star's seed.
## @return: True if system is cached.
func has_cached_system(star_seed: int) -> bool:
	return _systems_cache.has(star_seed)


## Clears all cached sectors and systems.
## Useful for memory management or regeneration.
func clear_cache() -> void:
	_sectors.clear()
	_systems_cache.clear()


## Returns the number of cached sectors.
## @return: Sector cache size.
func get_cached_sector_count() -> int:
	return _sectors.size()


## Returns the number of cached systems.
## @return: System cache size.
func get_cached_system_count() -> int:
	return _systems_cache.size()


## Generates a unique key for sector caching.
## @param quadrant_coords: Quadrant coordinates.
## @param sector_local_coords: Sector local coordinates.
## @return: Cache key string.
func _sector_key(quadrant_coords: Vector3i, sector_local_coords: Vector3i) -> String:
	return "%d,%d,%d:%d,%d,%d" % [
		quadrant_coords.x, quadrant_coords.y, quadrant_coords.z,
		sector_local_coords.x, sector_local_coords.y, sector_local_coords.z
	]


## Serializes the galaxy to a dictionary (for persistence).
## Note: Only saves config and seed; cached data is not persisted.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"seed": seed,
		"config": config.to_dict() if config != null else {},
	}


## Creates a Galaxy from a dictionary.
## @param data: Dictionary to parse.
## @return: New Galaxy instance.
static func from_dict(data: Dictionary) -> Galaxy:
	var p_seed: int = data.get("seed", 42) as int
	var config_data: Dictionary = data.get("config", {}) as Dictionary
	var p_config: GalaxyConfig = GalaxyConfig.from_dict(config_data) if not config_data.is_empty() else GalaxyConfig.create_default()
	return Galaxy.new(p_config, p_seed)
