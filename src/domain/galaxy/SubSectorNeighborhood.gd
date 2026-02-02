## Pure-logic management of a 7x7x7 subsector neighborhood around a camera position.
## Generates star data for all 343 subsectors and tags each star with its
## Chebyshev shell distance from center for fade control.
class_name SubSectorNeighborhood
extends RefCounted


## Extent of the neighborhood in each direction from center (7x7x7 = extent 3).
const EXTENT: int = 3

## Total subsectors in the neighborhood (7x7x7).
const TOTAL_SUBSECTORS: int = 343


## Container for the combined neighborhood data.
class NeighborhoodData:
	extends RefCounted

	## Combined star positions from all subsectors.
	var star_positions: PackedVector3Array = PackedVector3Array()

	## Combined star seeds from all subsectors.
	var star_seeds: PackedInt64Array = PackedInt64Array()

	## Per-star Chebyshev shell distance from center (0 = center, 1 = inner, 2 = mid, 3 = outer).
	var star_shells: PackedInt32Array = PackedInt32Array()

	## World-space origins of all subsectors.
	var subsector_origins: Array[Vector3] = []

	## Per-subsector Chebyshev shell distance.
	var subsector_shells: PackedInt32Array = PackedInt32Array()

	## World-space origin of the center subsector.
	var center_origin: Vector3 = Vector3.ZERO

	## Total number of stars in the neighborhood.
	## @return: Star count.
	func get_star_count() -> int:
		return star_positions.size()


## Builds the full 7x7x7 neighborhood around a camera position.
## @param camera_position: World-space camera position in parsecs.
## @param galaxy_seed: Galaxy master seed.
## @param density_model: Density model for star counts.
## @param reference_density: Max density for normalization.
## @return: NeighborhoodData with all stars, origins, and shell tags.
static func build(
	camera_position: Vector3,
	galaxy_seed: int,
	density_model: SpiralDensityModel,
	reference_density: float
) -> NeighborhoodData:
	var data: NeighborhoodData = NeighborhoodData.new()
	var center_origin: Vector3 = GalaxyCoordinates.get_subsector_world_origin(camera_position)
	data.center_origin = center_origin

	var ss_size: float = GalaxyCoordinates.SUBSECTOR_SIZE_PC

	for dx in range(-EXTENT, EXTENT + 1):
		for dy in range(-EXTENT, EXTENT + 1):
			for dz in range(-EXTENT, EXTENT + 1):
				var shell: int = _chebyshev_distance(dx, dy, dz)
				var offset_origin: Vector3 = center_origin + Vector3(
					float(dx) * ss_size,
					float(dy) * ss_size,
					float(dz) * ss_size
				)

				data.subsector_origins.append(offset_origin)
				data.subsector_shells.append(shell)

				var subsector_data: SubSectorGenerator.SectorStarData = (
					SubSectorGenerator.generate_single_subsector(
						galaxy_seed, offset_origin, density_model, reference_density
					)
				)

				for i in range(subsector_data.get_count()):
					data.star_positions.append(subsector_data.positions[i])
					data.star_seeds.append(subsector_data.star_seeds[i])
					data.star_shells.append(shell)

	return data


## Returns the center subsector origin for a given position without building data.
## Use this to detect whether a rebuild is needed.
## @param camera_position: World-space camera position.
## @return: Origin of the subsector containing the camera.
static func get_center_origin(camera_position: Vector3) -> Vector3:
	return GalaxyCoordinates.get_subsector_world_origin(camera_position)


## Computes Chebyshev (L-infinity) distance from origin.
## This gives the shell index: 0 = center, 1 = first ring, 2 = second, 3 = outermost.
## @param dx: X offset from center.
## @param dy: Y offset from center.
## @param dz: Z offset from center.
## @return: Shell index.
static func _chebyshev_distance(dx: int, dy: int, dz: int) -> int:
	return maxi(maxi(absi(dx), absi(dy)), absi(dz))
