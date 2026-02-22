## Generates a lightweight solar system preview from a galaxy star seed and position.
##
## Used to populate the inspector panel when a star is clicked in the subsector view,
## and to cache the generated system so opening it avoids redundant generation.
class_name StarSystemPreview
extends RefCounted

const _galaxy_star_script: GDScript = preload("res://src/domain/galaxy/GalaxyStar.gd")
const _galaxy_system_generator: GDScript = preload("res://src/domain/galaxy/GalaxySystemGenerator.gd")


## Summary data extracted from a generated system.
class PreviewData:
	extends RefCounted

	## The star seed this preview was generated for.
	var star_seed: int = 0

	## World position of the star.
	var world_position: Vector3 = Vector3.ZERO

	## Number of stars in the system.
	var star_count: int = 0

	## Spectral class strings for each star (e.g. "G2V").
	var spectral_classes: Array[String] = []

	## Effective temperatures for each star in Kelvin.
	var star_temperatures: Array[float] = []

	## Number of planets.
	var planet_count: int = 0

	## Number of moons.
	var moon_count: int = 0

	## Number of asteroid belts.
	var belt_count: int = 0

	## Metallicity relative to solar (1.0 = solar).
	var metallicity: float = 1.0

	## Total population across all bodies (native + colony).
	## 0 if uninhabited or population generation was skipped.
	var total_population: int = 0

	## Whether any body in the system is inhabited.
	## Convenience flag matching system.is_inhabited().
	var is_inhabited: bool = false

	## The fully generated SolarSystem (cached for reuse when opening the system).
	var system: SolarSystem = null


## Generates a PreviewData for the given star.
##
## Constructs a GalaxyStar from position and seed, derives galactic properties
## from the galaxy spec, runs the full GalaxySystemGenerator pipeline, and
## extracts a summary. The generated SolarSystem is cached in the result so
## callers can pass it directly to the system viewer without regenerating.
##
## @param star_seed: Deterministic seed of the star.
## @param world_position: World-space position in parsecs.
## @param galaxy_spec: Galaxy specification used to derive metallicity/age.
## @return: PreviewData on success, null on failure.
static func generate(
	star_seed: int,
	world_position: Vector3,
	galaxy_spec: GalaxySpec
) -> PreviewData:
	if star_seed == 0 or galaxy_spec == null:
		return null

	# Build a GalaxyStar so GalaxySystemGenerator can apply galactic context.
	var galaxy_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		world_position, star_seed, galaxy_spec
	)

	# Run the full generation pipeline with population enabled.
	var system: SolarSystem = GalaxySystemGenerator.generate_system(galaxy_star, true, true)
	if system == null:
		return null

	var data: PreviewData = PreviewData.new()
	data.star_seed = star_seed
	data.world_position = world_position
	data.metallicity = galaxy_star.metallicity
	data.system = system

	# Extract summary counts.
	data.star_count = system.get_star_count()
	data.planet_count = system.get_planet_count()
	data.moon_count = system.get_moon_count()
	data.belt_count = system.asteroid_belts.size()

	# Population summary.
	data.total_population = system.get_total_population()
	data.is_inhabited = system.is_inhabited()

	# Extract stellar data for each star.
	for star_body in system.get_stars():
		if star_body.has_stellar():
			data.spectral_classes.append(star_body.stellar.spectral_class)
			data.star_temperatures.append(star_body.stellar.effective_temperature_k)
		else:
			data.spectral_classes.append("?")
			data.star_temperatures.append(0.0)

	return data
