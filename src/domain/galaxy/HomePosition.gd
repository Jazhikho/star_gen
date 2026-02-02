## Constants and utilities for the player's home position in the galaxy.
## The home position represents Earth's approximate location in a Milky-Way-like galaxy.
class_name HomePosition
extends RefCounted


## Earth's approximate distance from the galactic center in parsecs.
## The Sun is located ~8,000 parsecs (26,000 light-years) from the center.
const SOLAR_DISTANCE_PC: float = 8000.0

## Earth's approximate height above the galactic plane in parsecs.
## The Sun is very close to the plane, roughly 20-25 parsecs above.
const SOLAR_HEIGHT_PC: float = 20.0

## Default angular position in the disk (radians from +X axis).
## This is arbitrary but provides a consistent starting point.
const SOLAR_ANGLE_RAD: float = 0.0


## Returns the default home position in parsec-space.
## Position convention: XZ = galactic plane, Y = height above disk.
## @return: World-space position in parsecs.
static func get_default_position() -> Vector3:
	var x: float = SOLAR_DISTANCE_PC * cos(SOLAR_ANGLE_RAD)
	var z: float = SOLAR_DISTANCE_PC * sin(SOLAR_ANGLE_RAD)
	return Vector3(x, SOLAR_HEIGHT_PC, z)


## Returns the quadrant coordinates containing the home position.
## @return: Quadrant grid coordinates.
static func get_home_quadrant() -> Vector3i:
	return GalaxyCoordinates.parsec_to_quadrant(get_default_position())


## Returns the full hierarchy coordinates for the home position.
## @return: HierarchyCoords with quadrant, sector, and subsector.
static func get_home_hierarchy() -> GalaxyCoordinates.HierarchyCoords:
	return GalaxyCoordinates.parsec_to_hierarchy(get_default_position())


## Returns the world-space origin of the home sector.
## @return: Sector origin in parsecs.
static func get_home_sector_origin() -> Vector3:
	var hierarchy: GalaxyCoordinates.HierarchyCoords = get_home_hierarchy()
	return GalaxyCoordinates.sector_world_origin(
		hierarchy.quadrant_coords,
		hierarchy.sector_local_coords
	)


## Returns the world-space center of the home sector.
## @return: Sector center in parsecs.
static func get_home_sector_center() -> Vector3:
	var origin: Vector3 = get_home_sector_origin()
	return origin + Vector3.ONE * GalaxyCoordinates.SECTOR_SIZE_PC * 0.5


## Returns the world-space center of the home subsector.
## @return: Subsector center in parsecs.
static func get_home_subsector_center() -> Vector3:
	var hierarchy: GalaxyCoordinates.HierarchyCoords = get_home_hierarchy()
	var sector_origin: Vector3 = GalaxyCoordinates.sector_world_origin(
		hierarchy.quadrant_coords,
		hierarchy.sector_local_coords
	)
	var subsector_origin: Vector3 = sector_origin + Vector3(
		float(hierarchy.subsector_local_coords.x) * GalaxyCoordinates.SUBSECTOR_SIZE_PC,
		float(hierarchy.subsector_local_coords.y) * GalaxyCoordinates.SUBSECTOR_SIZE_PC,
		float(hierarchy.subsector_local_coords.z) * GalaxyCoordinates.SUBSECTOR_SIZE_PC
	)
	return subsector_origin + Vector3.ONE * GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5


## Validates that a position is within reasonable galactic bounds.
## @param position: Position to validate in parsecs.
## @param spec: Galaxy specification for bounds checking.
## @return: True if position is within the galaxy's extent.
static func is_within_galaxy(position: Vector3, spec: GalaxySpec) -> bool:
	var radial_distance: float = sqrt(position.x * position.x + position.z * position.z)
	var height: float = absf(position.y)

	return radial_distance <= spec.radius_pc and height <= spec.height_pc
