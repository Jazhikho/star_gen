## Generates star system positions within subsectors.
##
## Uses realistic density scaling: ~0.004 star systems per cubic parsec
## at solar neighborhood density, Poisson-sampled per subsector.
class_name SubSectorGenerator
extends RefCounted


## Real-world average star system density in the solar neighborhood.
## Approximately 0.004 systems per cubic parsec.
const SOLAR_NEIGHBORHOOD_DENSITY: float = 0.004

## Volume of one subsector in cubic parsecs (10 × 10 × 10).
const SUBSECTOR_VOLUME_PC3: float = 1000.0

## Expected systems per subsector at solar-neighborhood density.
## 0.004 × 1000 = 4.0 systems.
const EXPECTED_SYSTEMS_AT_SOLAR: float = SOLAR_NEIGHBORHOOD_DENSITY * SUBSECTOR_VOLUME_PC3

## Subsectors per sector edge (10×10×10).
const SUBSECTORS_PER_EDGE: int = 10

## Border shell extent: one subsector deep around the sector.
const BORDER_EXTENT: int = 1


## Result container for generated stars.
class SectorStarData:
	extends RefCounted

	## World-space positions of all stars.
	var positions: PackedVector3Array = PackedVector3Array()

	## Per-star deterministic seed for downstream generation.
	var star_seeds: PackedInt64Array = PackedInt64Array()

	## Total number of generated stars.
	func get_count() -> int:
		return positions.size()

	## Appends another SectorStarData's contents to this one.
	## @param other: Data to merge in.
	func merge(other: SectorStarData) -> void:
		positions.append_array(other.positions)
		star_seeds.append_array(other.star_seeds)


## Generates all stars within a sector by iterating its subsectors.
## @param galaxy_seed: Galaxy master seed.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector position within the quadrant (each axis 0-9).
## @param density_model: Density model for star count determination.
## @param reference_density: Density at a reference point (e.g. solar neighborhood equivalent).
## @return: SectorStarData with positions and seeds.
static func generate_sector_stars(
	galaxy_seed: int,
	quadrant_coords: Vector3i,
	sector_local_coords: Vector3i,
	density_model: DensityModelInterface,
	reference_density: float
) -> SectorStarData:
	var result: SectorStarData = SectorStarData.new()

	if reference_density <= 0.0:
		return result

	var sector_origin: Vector3 = GalaxyCoordinates.sector_world_origin(
		quadrant_coords, sector_local_coords
	)

	for ssx in range(SUBSECTORS_PER_EDGE):
		for ssy in range(SUBSECTORS_PER_EDGE):
			for ssz in range(SUBSECTORS_PER_EDGE):
				var ss_local: Vector3i = Vector3i(ssx, ssy, ssz)
				var ss_origin: Vector3 = GalaxyCoordinates.subsector_offset_to_world(
					sector_origin, ss_local
				)
				var sector_seed: int = SeedDeriver.derive_sector_seed_full(
					galaxy_seed, quadrant_coords, sector_local_coords
				)
				_generate_subsector_stars(
					sector_seed, ss_local, ss_origin,
					density_model, reference_density,
					result.positions, result.star_seeds
				)

	return result


## Generates stars for a sector plus its one-deep border shell.
## @param galaxy_seed: Galaxy master seed.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector position within the quadrant.
## @param density_model: Density model.
## @param reference_density: Reference density for normalization.
## @return: SectorStarData covering the sector and its border.
static func generate_sector_with_border(
	galaxy_seed: int,
	quadrant_coords: Vector3i,
	sector_local_coords: Vector3i,
	density_model: DensityModelInterface,
	reference_density: float
) -> SectorStarData:
	var result: SectorStarData = SectorStarData.new()

	if reference_density <= 0.0:
		return result

	var sector_origin: Vector3 = GalaxyCoordinates.sector_world_origin(
		quadrant_coords, sector_local_coords
	)
	var min_idx: int = - BORDER_EXTENT
	var max_idx: int = SUBSECTORS_PER_EDGE + BORDER_EXTENT

	for bx in range(min_idx, max_idx):
		for by in range(min_idx, max_idx):
			for bz in range(min_idx, max_idx):
				var ss_offset: Vector3i = Vector3i(bx, by, bz)
				var ss_world_origin: Vector3 = GalaxyCoordinates.subsector_offset_to_world(
					sector_origin, ss_offset
				)
				var ss_center: Vector3 = ss_world_origin + Vector3(
					GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5,
					GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5,
					GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5
				)

				var hierarchy: GalaxyCoordinates.HierarchyCoords = (
					GalaxyCoordinates.parsec_to_hierarchy(ss_center)
				)
				var resolved_sector_seed: int = SeedDeriver.derive_sector_seed_full(
					galaxy_seed, hierarchy.quadrant_coords, hierarchy.sector_local_coords
				)

				_generate_subsector_stars(
					resolved_sector_seed, hierarchy.subsector_local_coords,
					ss_world_origin, density_model, reference_density,
					result.positions, result.star_seeds
				)

	return result


## Generates stars for a single subsector and appends to output arrays.
## Uses realistic density: the ratio of local density to reference density
## scales the solar-neighborhood expected count (4 systems per subsector).
## @param sector_seed: Parent sector seed.
## @param subsector_local: Subsector position within sector (each axis 0-9).
## @param subsector_origin: World-space origin of this subsector.
## @param density_model: Density model.
## @param reference_density: Reference density value.
## @param out_positions: Array to append star positions to.
## @param out_seeds: Array to append star seeds to.
static func _generate_subsector_stars(
	sector_seed: int,
	subsector_local: Vector3i,
	subsector_origin: Vector3,
	density_model: DensityModelInterface,
	reference_density: float,
	out_positions: PackedVector3Array,
	out_seeds: PackedInt64Array
) -> void:
	var subsector_seed: int = SeedDeriver.derive_subsector_seed(sector_seed, subsector_local)
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = subsector_seed

	var center: Vector3 = subsector_origin + Vector3(
		GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5,
		GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5,
		GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5
	)

	var local_density: float = density_model.get_density(center)
	var density_ratio: float = clampf(local_density / reference_density, 0.0, 10.0)

	# Scale the solar-neighborhood expected count by the density ratio.
	# At reference density, expect ~4 systems per subsector.
	# At galactic center (higher density), expect more.
	# At the edge, expect fewer.
	var expected_stars: float = density_ratio * EXPECTED_SYSTEMS_AT_SOLAR

	var star_count: int = _sample_poisson(expected_stars, rng)

	for i in range(star_count):
		var star_seed: int = SeedDeriver.derive_star_seed(subsector_seed, i)
		var pos: Vector3 = subsector_origin + Vector3(
			rng.randf() * GalaxyCoordinates.SUBSECTOR_SIZE_PC,
			rng.randf() * GalaxyCoordinates.SUBSECTOR_SIZE_PC,
			rng.randf() * GalaxyCoordinates.SUBSECTOR_SIZE_PC
		)
		out_positions.append(pos)
		out_seeds.append(star_seed)


## Samples from a Poisson distribution using the inverse transform method.
## For small lambda this is efficient and deterministic.
## @param lambda_val: Expected value (mean) of the distribution.
## @param rng: Seeded RNG.
## @return: Non-negative integer sample.
static func _sample_poisson(lambda_val: float, rng: RandomNumberGenerator) -> int:
	if lambda_val <= 0.0:
		return 0

	var l_thresh: float = exp(-lambda_val)
	var k: int = 0
	var p: float = 1.0

	while true:
		k += 1
		p *= rng.randf()
		if p <= l_thresh:
			break

	return k - 1


## Generates stars for a single subsector at a given world-space origin.
## Resolves the correct hierarchy via world-space lookup for proper seed derivation.
## @param galaxy_seed: Galaxy master seed.
## @param world_origin: World-space origin (min corner) of the subsector.
## @param density_model: Density model for star count.
## @param reference_density: Reference density for normalization.
## @return: SectorStarData for this single subsector.
static func generate_single_subsector(
	galaxy_seed: int,
	world_origin: Vector3,
	density_model: DensityModelInterface,
	reference_density: float
) -> SectorStarData:
	var result: SectorStarData = SectorStarData.new()

	if reference_density <= 0.0:
		return result

	var center: Vector3 = world_origin + Vector3(
		GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5,
		GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5,
		GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5
	)
	var hierarchy: GalaxyCoordinates.HierarchyCoords = (
		GalaxyCoordinates.parsec_to_hierarchy(center)
	)
	var sector_seed: int = SeedDeriver.derive_sector_seed_full(
		galaxy_seed, hierarchy.quadrant_coords, hierarchy.sector_local_coords
	)

	_generate_subsector_stars(
		sector_seed, hierarchy.subsector_local_coords,
		world_origin, density_model, reference_density,
		result.positions, result.star_seeds
	)

	return result
