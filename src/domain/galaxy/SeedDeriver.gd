## Deterministic seed derivation for the galaxy hierarchy.
##
## Each level derives its seed from its parent seed and its grid coordinates,
## using StableHash to ensure version-stable results.
## The chain is: galaxy_seed → quadrant_seed → sector_seed → subsector_seed → star_seed.
class_name SeedDeriver
extends RefCounted


## Derives a quadrant seed from the galaxy seed and quadrant grid coordinates.
## @param galaxy_seed: The galaxy's master seed.
## @param quadrant_coords: The quadrant's grid position.
## @return: Deterministic seed for this quadrant.
static func derive_quadrant_seed(galaxy_seed: int, quadrant_coords: Vector3i) -> int:
	return StableHash.derive_seed(galaxy_seed, quadrant_coords)


## Derives a sector seed from a quadrant seed and sector-local grid coordinates.
## Sector coordinates are local to the quadrant: each axis in [0, 9].
## @param quadrant_seed: The parent quadrant's seed.
## @param sector_local_coords: Sector position within the quadrant.
## @return: Deterministic seed for this sector.
static func derive_sector_seed(quadrant_seed: int, sector_local_coords: Vector3i) -> int:
	return StableHash.derive_seed(quadrant_seed, sector_local_coords)


## Derives a subsector seed from a sector seed and subsector-local grid coordinates.
## Subsector coordinates are local to the sector: each axis in [0, 9].
## @param sector_seed: The parent sector's seed.
## @param subsector_local_coords: Subsector position within the sector.
## @return: Deterministic seed for this subsector.
static func derive_subsector_seed(sector_seed: int, subsector_local_coords: Vector3i) -> int:
	return StableHash.derive_seed(sector_seed, subsector_local_coords)


## Derives a star system seed from a subsector seed and star index.
## @param subsector_seed: The parent subsector's seed.
## @param star_index: Index of the star within the subsector.
## @return: Deterministic seed for this star system.
static func derive_star_seed(subsector_seed: int, star_index: int) -> int:
	return StableHash.derive_seed_indexed(subsector_seed, star_index)


## Convenience: derives the full chain from galaxy seed to sector seed.
## @param galaxy_seed: Galaxy master seed.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector position within the quadrant.
## @return: Deterministic sector seed.
static func derive_sector_seed_full(
	galaxy_seed: int,
	quadrant_coords: Vector3i,
	sector_local_coords: Vector3i
) -> int:
	var quadrant_seed: int = derive_quadrant_seed(galaxy_seed, quadrant_coords)
	return derive_sector_seed(quadrant_seed, sector_local_coords)


## Convenience: derives the full chain from galaxy seed to subsector seed.
## @param galaxy_seed: Galaxy master seed.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector position within the quadrant.
## @param subsector_local_coords: Subsector position within the sector.
## @return: Deterministic subsector seed.
static func derive_subsector_seed_full(
	galaxy_seed: int,
	quadrant_coords: Vector3i,
	sector_local_coords: Vector3i,
	subsector_local_coords: Vector3i
) -> int:
	var sector_seed: int = derive_sector_seed_full(
		galaxy_seed, quadrant_coords, sector_local_coords
	)
	return derive_subsector_seed(sector_seed, subsector_local_coords)
