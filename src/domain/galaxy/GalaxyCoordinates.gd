## Conversions between parsec-space positions and hierarchical grid coordinates.
## All grids are axis-aligned and centered on the galaxy origin.
## Position convention: XZ = galactic plane, Y = height above disk.
class_name GalaxyCoordinates
extends RefCounted


## Hierarchical zoom levels for the galaxy viewer.
enum ZoomLevel { GALAXY, QUADRANT, SECTOR, SUBSECTOR }

## Size of one quadrant edge in parsecs.
const QUADRANT_SIZE_PC: float = 1000.0

## Size of one sector edge in parsecs.
const SECTOR_SIZE_PC: float = 100.0

## Size of one sub-sector edge in parsecs.
const SUBSECTOR_SIZE_PC: float = 10.0

## Number of standard deviations to include for bulge extent.
const BULGE_SIGMA_COVERAGE: float = 3.0


## Converts a parsec-space position to quadrant grid coordinates.
## @param position: Position in parsec-space.
## @return: Integer grid coordinates of the containing quadrant.
static func parsec_to_quadrant(position: Vector3) -> Vector3i:
	return Vector3i(
		floori(position.x / QUADRANT_SIZE_PC),
		floori(position.y / QUADRANT_SIZE_PC),
		floori(position.z / QUADRANT_SIZE_PC)
	)


## Converts quadrant grid coordinates to the parsec-space center of that cell.
## @param coords: Quadrant grid coordinates.
## @return: Center position of the quadrant in parsecs.
static func quadrant_to_parsec_center(coords: Vector3i) -> Vector3:
	var half: float = QUADRANT_SIZE_PC * 0.5
	return Vector3(
		float(coords.x) * QUADRANT_SIZE_PC + half,
		float(coords.y) * QUADRANT_SIZE_PC + half,
		float(coords.z) * QUADRANT_SIZE_PC + half
	)


## Computes the effective radial extent of the galaxy in parsecs.
## Takes the larger of the disk radius and the bulge's radial 3-sigma.
## @param spec: Galaxy specification.
## @return: Effective radial extent in parsecs.
static func get_effective_radius(spec: GalaxySpec) -> float:
	var bulge_radial_extent: float = spec.bulge_radius_pc * BULGE_SIGMA_COVERAGE
	return maxf(spec.radius_pc, bulge_radial_extent)


## Computes the effective half-height of the galaxy in parsecs.
## Takes the larger of the disk clipping height and the bulge's vertical 3-sigma.
## This ensures the grid covers the full extent of where stars actually are.
## @param spec: Galaxy specification.
## @return: Effective half-height in parsecs.
static func get_effective_half_height(spec: GalaxySpec) -> float:
	var bulge_vertical_extent: float = spec.bulge_height_pc * BULGE_SIGMA_COVERAGE
	return maxf(spec.height_pc, bulge_vertical_extent)


## Returns the minimum quadrant grid coordinates that cover the galaxy's
## effective bounds, including the full bulge extent.
## @param spec: Galaxy specification defining radius and height.
## @return: Minimum corner of the quadrant grid.
static func get_quadrant_grid_min(spec: GalaxySpec) -> Vector3i:
	var effective_radius: float = get_effective_radius(spec)
	var effective_half_height: float = get_effective_half_height(spec)
	return parsec_to_quadrant(
		Vector3(-effective_radius, -effective_half_height, -effective_radius)
	)


## Returns the maximum quadrant grid coordinates that cover the galaxy's
## effective bounds, including the full bulge extent.
## @param spec: Galaxy specification defining radius and height.
## @return: Maximum corner of the quadrant grid (inclusive).
static func get_quadrant_grid_max(spec: GalaxySpec) -> Vector3i:
	var effective_radius: float = get_effective_radius(spec)
	var effective_half_height: float = get_effective_half_height(spec)
	return parsec_to_quadrant(
		Vector3(effective_radius, effective_half_height, effective_radius)
	)


## Checks whether a parsec position falls within a given quadrant.
## @param position: Position in parsec-space.
## @param quadrant_coords: The quadrant to test against.
## @return: True if the position is inside that quadrant cell.
static func is_position_in_quadrant(position: Vector3, quadrant_coords: Vector3i) -> bool:
	return parsec_to_quadrant(position) == quadrant_coords


## Full hierarchy coordinates for a world-space position.
class HierarchyCoords:
	extends RefCounted

	## Quadrant grid coordinates.
	var quadrant_coords: Vector3i = Vector3i.ZERO

	## Sector-local coordinates within the quadrant (each axis 0-9).
	var sector_local_coords: Vector3i = Vector3i.ZERO

	## Subsector-local coordinates within the sector (each axis 0-9).
	var subsector_local_coords: Vector3i = Vector3i.ZERO


## Converts a world-space parsec position to its full hierarchy coordinates.
## Correctly handles negative positions and cross-boundary cases.
## @param position: World-space position in parsecs.
## @return: HierarchyCoords with quadrant, sector, and subsector components.
static func parsec_to_hierarchy(position: Vector3) -> HierarchyCoords:
	var result: HierarchyCoords = HierarchyCoords.new()
	result.quadrant_coords = parsec_to_quadrant(position)

	var quadrant_origin: Vector3 = Vector3(
		float(result.quadrant_coords.x) * QUADRANT_SIZE_PC,
		float(result.quadrant_coords.y) * QUADRANT_SIZE_PC,
		float(result.quadrant_coords.z) * QUADRANT_SIZE_PC
	)
	var local_in_quadrant: Vector3 = position - quadrant_origin

	result.sector_local_coords = Vector3i(
		clampi(floori(local_in_quadrant.x / SECTOR_SIZE_PC), 0, 9),
		clampi(floori(local_in_quadrant.y / SECTOR_SIZE_PC), 0, 9),
		clampi(floori(local_in_quadrant.z / SECTOR_SIZE_PC), 0, 9)
	)

	var sector_origin: Vector3 = Vector3(
		float(result.sector_local_coords.x) * SECTOR_SIZE_PC,
		float(result.sector_local_coords.y) * SECTOR_SIZE_PC,
		float(result.sector_local_coords.z) * SECTOR_SIZE_PC
	)
	var local_in_sector: Vector3 = local_in_quadrant - sector_origin

	result.subsector_local_coords = Vector3i(
		clampi(floori(local_in_sector.x / SUBSECTOR_SIZE_PC), 0, 9),
		clampi(floori(local_in_sector.y / SUBSECTOR_SIZE_PC), 0, 9),
		clampi(floori(local_in_sector.z / SUBSECTOR_SIZE_PC), 0, 9)
	)

	return result


## Computes the world-space origin of a subsector given its relative offset
## from a reference sector origin.
## @param sector_world_origin: World-space origin of the reference sector.
## @param subsector_offset: Subsector offset in grid units (can be outside 0-9 for border).
## @return: World-space origin of the subsector.
static func subsector_offset_to_world(
	sector_world_origin: Vector3,
	subsector_offset: Vector3i
) -> Vector3:
	return sector_world_origin + Vector3(
		float(subsector_offset.x) * SUBSECTOR_SIZE_PC,
		float(subsector_offset.y) * SUBSECTOR_SIZE_PC,
		float(subsector_offset.z) * SUBSECTOR_SIZE_PC
	)


## Computes the world-space origin of a sector.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector-local coordinates within the quadrant.
## @return: World-space origin (min corner) of the sector.
static func sector_world_origin(
	quadrant_coords: Vector3i,
	sector_local_coords: Vector3i
) -> Vector3:
	return Vector3(
		float(quadrant_coords.x) * QUADRANT_SIZE_PC + float(sector_local_coords.x) * SECTOR_SIZE_PC,
		float(quadrant_coords.y) * QUADRANT_SIZE_PC + float(sector_local_coords.y) * SECTOR_SIZE_PC,
		float(quadrant_coords.z) * QUADRANT_SIZE_PC + float(sector_local_coords.z) * SECTOR_SIZE_PC
	)


## Computes the world-space origin of the subsector containing a position.
## @param position: World-space position in parsecs.
## @return: World-space origin (min corner) of the containing subsector.
static func get_subsector_world_origin(position: Vector3) -> Vector3:
	var hierarchy: HierarchyCoords = parsec_to_hierarchy(position)
	return sector_world_origin(
		hierarchy.quadrant_coords, hierarchy.sector_local_coords
	) + Vector3(
		float(hierarchy.subsector_local_coords.x) * SUBSECTOR_SIZE_PC,
		float(hierarchy.subsector_local_coords.y) * SUBSECTOR_SIZE_PC,
		float(hierarchy.subsector_local_coords.z) * SUBSECTOR_SIZE_PC
	)


## Computes the world-space center of the subsector containing a position.
## @param position: World-space position in parsecs.
## @return: World-space center of the containing subsector.
static func get_subsector_world_center(position: Vector3) -> Vector3:
	var origin: Vector3 = get_subsector_world_origin(position)
	return origin + Vector3.ONE * SUBSECTOR_SIZE_PC * 0.5
