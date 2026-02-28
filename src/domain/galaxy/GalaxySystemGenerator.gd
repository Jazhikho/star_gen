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
const _galaxy_body_overrides: GDScript = preload("res://src/domain/galaxy/GalaxyBodyOverrides.gd")


## Generates a complete solar system from a GalaxyStar.
## @param star: The GalaxyStar to generate a system for.
## @param include_asteroids: Whether to generate asteroid belts.
## @param enable_population: Whether to generate population data for planets.
##   Set true when callers need get_total_population() / get_native_population() etc.
##   Defaults to false to keep lazy galaxy generation cheap.
## @param overrides: Optional edited-body overrides. If non-null and has entries
##   for this star's seed, matching bodies are swapped for the edited versions
##   after generation. Unmatched bodies stay deterministic.
## @return: Generated SolarSystem, or null on failure.
static func generate_system(
	star: GalaxyStar,
	include_asteroids: bool = true,
	enable_population: bool = false,
	overrides: RefCounted = null
) -> SolarSystem:
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

	# Apply edited-body overrides last so they win over deterministic generation.
	if overrides != null and overrides.has_method("has_any_for") and overrides.has_method("apply_to_bodies"):
		if overrides.has_any_for(star.star_seed):
			_apply_overrides_to_system(system, star.star_seed, overrides)

	return system


## Swaps generated bodies for edited versions where ids match.
## @param system: The freshly generated SolarSystem (mutated in place).
## @param star_seed: Star seed used as the override bucket key.
## @param overrides: The override set to consult (RefCounted with GalaxyBodyOverrides API).
static func _apply_overrides_to_system(system: SolarSystem, star_seed: int, overrides: RefCounted) -> void:
	var all_bodies: Array = []
	for s: CelestialBody in system.get_stars():
		all_bodies.append(s)
	for p: CelestialBody in system.get_planets():
		all_bodies.append(p)
	for m: CelestialBody in system.get_moons():
		all_bodies.append(m)
	for a: CelestialBody in system.get_asteroids():
		all_bodies.append(a)

	var replaced: int = overrides.apply_to_bodies(star_seed, all_bodies)
	if replaced == 0:
		return

	for body: CelestialBody in all_bodies:
		if body == null:
			continue
		if overrides.get_override_dict(star_seed, body.id).is_empty():
			continue
		system.add_body(body)


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
