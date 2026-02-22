## Generates solar systems on demand from galaxy star data.
##
## Bridges the galaxy layer (GalaxyStar) with the system layer (SolarSystem).
## Applies galactic context (metallicity, age) to system generation.
class_name GalaxySystemGenerator
extends RefCounted

const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _stellar_config_generator: GDScript = preload("res://src/domain/system/StellarConfigGenerator.gd")
const _system_planet_generator: GDScript = preload("res://src/domain/system/SystemPlanetGenerator.gd")
const _system_moon_generator: GDScript = preload("res://src/domain/system/SystemMoonGenerator.gd")
const _system_asteroid_generator: GDScript = preload("res://src/domain/system/SystemAsteroidGenerator.gd")
const _orbit_slot_generator: GDScript = preload("res://src/domain/system/OrbitSlotGenerator.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _galaxy_star: GDScript = preload("res://src/domain/galaxy/GalaxyStar.gd")


## Generates a complete solar system from a GalaxyStar.
## @param star: The GalaxyStar to generate a system for.
## @param include_asteroids: Whether to generate asteroid belts.
## @param enable_population: Whether to generate population data for planets.
##   Set true when callers need get_total_population() / get_native_population() etc.
##   Defaults to false to keep lazy galaxy generation cheap.
## @return: Generated SolarSystem, or null on failure.
static func generate_system(star: GalaxyStar, include_asteroids: bool = true, enable_population: bool = false) -> SolarSystem:
	if star == null:
		return null

	# Create spec with galactic context applied
	var spec: SolarSystemSpec = _create_spec_from_star(star, include_asteroids)

	# Generate using standard pipeline
	var rng: SeededRng = SeededRng.new(spec.generation_seed)

	# Generate stellar configuration
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	if system == null:
		push_error("GalaxySystemGenerator: Failed to generate stellar config for seed %d" % star.star_seed)
		return null

	# Get stars and orbit hosts from the system
	var stars: Array[CelestialBody] = system.get_stars()
	var hosts: Array[OrbitHost] = system.orbit_hosts

	# Generate orbit slots
	var all_slots_dict: Dictionary = OrbitSlotGenerator.generate_all_slots(
		hosts, stars, system.hierarchy, rng
	)

	# Flatten slots into a single array
	var all_slots: Array[OrbitSlot] = []
	for host_id in all_slots_dict:
		var host_slots: Array = all_slots_dict[host_id]
		for slot in host_slots:
			all_slots.append(slot as OrbitSlot)

	# Generate planets
	var planet_result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		all_slots, hosts, stars, rng, enable_population
	)

	for planet in planet_result.planets:
		system.add_body(planet)

	# Generate moons
	var moon_result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		planet_result.planets, hosts, stars, rng, enable_population
	)

	for moon in moon_result.moons:
		system.add_body(moon)

	# Generate asteroid belts if requested
	if include_asteroids and spec.include_asteroid_belts:
		var belt_result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
			hosts, planet_result.slots, stars, rng
		)

		for belt in belt_result.belts:
			system.add_asteroid_belt(belt)

		for asteroid in belt_result.asteroids:
			system.add_body(asteroid)

	# Update provenance with final spec snapshot
	if system.provenance != null:
		system.provenance.spec_snapshot = spec.to_dict()

	return system


## Creates a SolarSystemSpec from a GalaxyStar, applying galactic context.
## @param star: The GalaxyStar with position-derived properties.
## @param include_asteroids: Whether to include asteroid belts.
## @return: Configured SolarSystemSpec.
static func _create_spec_from_star(star: GalaxyStar, include_asteroids: bool) -> SolarSystemSpec:
	var spec: SolarSystemSpec = SolarSystemSpec.random_small(star.star_seed)
	spec.system_metallicity = star.metallicity
	spec.include_asteroid_belts = include_asteroids
	# Age bias could affect system_age_years if we wanted deterministic ages
	return spec
