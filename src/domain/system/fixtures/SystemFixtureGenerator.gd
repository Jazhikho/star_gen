## Generates golden master fixtures for solar system regression testing.
## Creates deterministic test cases for various system configurations.
class_name SystemFixtureGenerator
extends RefCounted

const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _stellar_config_generator: GDScript = preload("res://src/domain/system/StellarConfigGenerator.gd")
const _orbit_slot_generator: GDScript = preload("res://src/domain/system/OrbitSlotGenerator.gd")
const _orbit_slot: GDScript = preload("res://src/domain/system/OrbitSlot.gd")
const _system_planet_generator: GDScript = preload("res://src/domain/system/SystemPlanetGenerator.gd")
const _system_moon_generator: GDScript = preload("res://src/domain/system/SystemMoonGenerator.gd")
const _system_asteroid_generator: GDScript = preload("res://src/domain/system/SystemAsteroidGenerator.gd")
const _system_serializer: GDScript = preload("res://src/domain/system/SystemSerializer.gd")
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _versions: GDScript = preload("res://src/domain/constants/Versions.gd")
const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Base seed for fixture generation.
const BASE_SEED: int = 600000


## Fixture configuration structure.
class FixtureConfig:
	extends RefCounted
	
	var name: String
	var seed_value: int
	var star_count_min: int
	var star_count_max: int
	var spectral_hints: Array[int]
	var include_belts: bool
	
	func _init(
		p_name: String,
		p_seed: int,
		p_min: int,
		p_max: int,
		p_hints: Array[int] = [],
		p_belts: bool = true
	) -> void:
		name = p_name
		seed_value = p_seed
		star_count_min = p_min
		star_count_max = p_max
		spectral_hints = p_hints
		include_belts = p_belts


## Generates all fixtures and returns them as an array of dictionaries.
## @return: Array of fixture dictionaries.
static func generate_all_fixtures() -> Array[Dictionary]:
	var fixtures: Array[Dictionary] = []
	
	var configs: Array[FixtureConfig] = _get_fixture_configs()
	
	for config in configs:
		var fixture: Dictionary = _generate_fixture(config)
		if not fixture.is_empty():
			fixtures.append(fixture)
	
	return fixtures


## Returns the list of fixture configurations.
## @return: Array of FixtureConfig.
static func _get_fixture_configs() -> Array[FixtureConfig]:
	var configs: Array[FixtureConfig] = []
	
	# Single star - Sun-like
	configs.append(FixtureConfig.new(
		"system_single_sun_like",
		BASE_SEED + 1,
		1, 1,
		[StarClass.SpectralClass.G],
		true
	))
	
	# Single star - Red dwarf
	configs.append(FixtureConfig.new(
		"system_single_red_dwarf",
		BASE_SEED + 2,
		1, 1,
		[StarClass.SpectralClass.M],
		true
	))
	
	# Single star - Hot blue
	configs.append(FixtureConfig.new(
		"system_single_hot_blue",
		BASE_SEED + 3,
		1, 1,
		[StarClass.SpectralClass.B],
		false # Hot stars may not have stable belts
	))
	
	# Binary - Equal mass
	configs.append(FixtureConfig.new(
		"system_binary_equal",
		BASE_SEED + 10,
		2, 2,
		[StarClass.SpectralClass.G, StarClass.SpectralClass.G],
		true
	))
	
	# Binary - Unequal mass
	configs.append(FixtureConfig.new(
		"system_binary_unequal",
		BASE_SEED + 11,
		2, 2,
		[StarClass.SpectralClass.G, StarClass.SpectralClass.M],
		true
	))
	
	# Triple - Hierarchical
	configs.append(FixtureConfig.new(
		"system_triple_hierarchical",
		BASE_SEED + 20,
		3, 3,
		[StarClass.SpectralClass.G, StarClass.SpectralClass.K, StarClass.SpectralClass.M],
		true
	))
	
	# Quadruple
	configs.append(FixtureConfig.new(
		"system_quadruple",
		BASE_SEED + 30,
		4, 4,
		[],
		true
	))
	
	# Random small (1-3 stars)
	configs.append(FixtureConfig.new(
		"system_random_small",
		BASE_SEED + 40,
		1, 3,
		[],
		true
	))
	
	# Maximum stars
	configs.append(FixtureConfig.new(
		"system_max_stars",
		BASE_SEED + 50,
		10, 10,
		[],
		false # Skip belts for simplicity
	))
	
	# Minimal system (single star, no belts)
	configs.append(FixtureConfig.new(
		"system_minimal",
		BASE_SEED + 60,
		1, 1,
		[StarClass.SpectralClass.K],
		false
	))
	
	return configs


## Generates a single fixture from configuration.
## @param config: The fixture configuration.
## @return: Fixture dictionary.
static func _generate_fixture(config: FixtureConfig) -> Dictionary:
	var spec: SolarSystemSpec = SolarSystemSpec.new(
		config.seed_value,
		config.star_count_min,
		config.star_count_max
	)
	spec.spectral_class_hints = config.spectral_hints
	spec.include_asteroid_belts = config.include_belts
	spec.name_hint = config.name
	
	var system: SolarSystem = generate_system(spec)
	if system == null:
		return {}
	
	return {
		"name": config.name,
		"seed": config.seed_value,
		"spec": spec.to_dict(),
		"system": SystemSerializer.to_dict(system),
	}


## Generates a complete solar system from spec.
## @param spec: System specification.
## @param enable_population: If true, generate population data for planets/moons.
## @return: Generated SolarSystem, or null on failure.
static func generate_system(spec: SolarSystemSpec, enable_population: bool = false) -> SolarSystem:
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	# Generate stellar configuration (returns SolarSystem with stars and hierarchy)
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	if system == null:
		return null
	
	# Get stars and orbit hosts from the system
	var stars: Array[CelestialBody] = system.get_stars()
	var hosts: Array[OrbitHost] = system.orbit_hosts
	
	# Generate orbit slots
	var all_slots_dict: Dictionary = OrbitSlotGenerator.generate_all_slots(hosts, stars, system.hierarchy, rng)
	
	# Flatten slots into a single array
	var all_slots: Array[OrbitSlot] = []
	for host_id in all_slots_dict:
		var host_slots: Array = all_slots_dict[host_id]
		for slot in host_slots:
			all_slots.append(slot as OrbitSlot)

	# Reserve belt slots before planet generation so belts consume orbit capacity.
	var belt_reservation: RefCounted = null
	if spec.include_asteroid_belts:
		belt_reservation = _system_asteroid_generator.reserve_belt_slots(
			hosts,
			all_slots,
			stars,
			rng
		)
		_system_asteroid_generator.mark_reserved_slots(all_slots, belt_reservation.reserved_slot_ids)
	
	# Generate planets
	var planet_result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		all_slots,
		hosts,
		stars,
		rng,
		enable_population
	)
	
	for planet in planet_result.planets:
		system.add_body(planet)

	# Clear reservation placeholders so only real planets stay marked as filled.
	if spec.include_asteroid_belts:
		_system_asteroid_generator.clear_reserved_slot_marks(planet_result.slots)
	
	# Generate moons
	var moon_result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		planet_result.planets,
		hosts,
		stars,
		rng,
		enable_population
	)
	
	for moon in moon_result.moons:
		system.add_body(moon)
	
	# Generate asteroid belts (if enabled)
	if spec.include_asteroid_belts:
		var belt_result: SystemAsteroidGenerator.BeltGenerationResult = _system_asteroid_generator.generate_from_predefined_belts(
			belt_reservation.belts,
			hosts,
			stars,
			rng
		)
		
		for belt in belt_result.belts:
			system.add_asteroid_belt(belt)
		
		for asteroid in belt_result.asteroids:
			system.add_body(asteroid)
	
	# Provenance is already set by StellarConfigGenerator, but update spec snapshot
	if system.provenance != null:
		system.provenance.spec_snapshot = spec.to_dict()
	
	return system


## Exports all fixtures to JSON strings.
## @param pretty: Whether to format with indentation.
## @return: Dictionary mapping fixture name to JSON string.
static func export_all_to_json(pretty: bool = true) -> Dictionary:
	var fixtures: Array[Dictionary] = generate_all_fixtures()
	var result: Dictionary = {}
	
	for fixture in fixtures:
		var json_str: String
		if pretty:
			json_str = JSON.stringify(fixture, "\t")
		else:
			json_str = JSON.stringify(fixture)
		result[fixture["name"]] = json_str
	
	return result
