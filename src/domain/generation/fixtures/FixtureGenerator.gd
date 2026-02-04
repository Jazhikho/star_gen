## Utility for generating golden master fixtures for regression testing.
## Creates deterministic test cases for all body types.
class_name FixtureGenerator
extends RefCounted

const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _asteroid_generator: GDScript = preload("res://src/domain/generation/generators/AsteroidGenerator.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _asteroid_type: GDScript = preload("res://src/domain/generation/archetypes/AsteroidType.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _celestial_serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Base seed for fixture generation.
const BASE_SEED: int = 42000


## Generates all fixtures and returns them as an array of dictionaries.
## Each dictionary contains: name, spec, context (if applicable), body
## @return: Array of fixture dictionaries.
static func generate_all_fixtures() -> Array[Dictionary]:
	var fixtures: Array[Dictionary] = []
	
	fixtures.append_array(_generate_star_fixtures())
	fixtures.append_array(_generate_planet_fixtures())
	fixtures.append_array(_generate_moon_fixtures())
	fixtures.append_array(_generate_asteroid_fixtures())
	
	return fixtures


## Generates star fixtures (7 fixtures, one per spectral class).
## @return: Array of star fixture dictionaries.
static func _generate_star_fixtures() -> Array[Dictionary]:
	var fixtures: Array[Dictionary] = []
	
	var spectral_classes: Array = [
		{"class": StarClass.SpectralClass.O, "name": "star_o_class"},
		{"class": StarClass.SpectralClass.B, "name": "star_b_class"},
		{"class": StarClass.SpectralClass.A, "name": "star_a_class"},
		{"class": StarClass.SpectralClass.F, "name": "star_f_class"},
		{"class": StarClass.SpectralClass.G, "name": "star_g_class"},
		{"class": StarClass.SpectralClass.K, "name": "star_k_class"},
		{"class": StarClass.SpectralClass.M, "name": "star_m_class"},
	]
	
	for i in range(spectral_classes.size()):
		var spec_info: Dictionary = spectral_classes[i]
		var seed_val: int = BASE_SEED + i
		
		var spec: StarSpec = StarSpec.new(
			seed_val,
			spec_info["class"],
			-1,
			-1.0,
			-1.0,
			"",
			{}
		)
		
		var rng: SeededRng = SeededRng.new(seed_val)
		var body: CelestialBody = StarGenerator.generate(spec, rng)
		
		fixtures.append({
			"name": spec_info["name"],
			"type": "star",
			"seed": seed_val,
			"spec": spec.to_dict(),
			"context": null,
			"body": CelestialSerializer.to_dict(body)
		})
	
	return fixtures


## Generates planet fixtures (7 fixtures covering size categories and zones).
## @return: Array of planet fixture dictionaries.
static func _generate_planet_fixtures() -> Array[Dictionary]:
	var fixtures: Array[Dictionary] = []
	
	var planet_configs: Array = [
		{"size": SizeCategory.Category.DWARF, "zone": OrbitZone.Zone.COLD, "name": "planet_dwarf_cold"},
		{"size": SizeCategory.Category.SUB_TERRESTRIAL, "zone": OrbitZone.Zone.TEMPERATE, "name": "planet_subterrestrial_temperate"},
		{"size": SizeCategory.Category.TERRESTRIAL, "zone": OrbitZone.Zone.TEMPERATE, "name": "planet_terrestrial_temperate"},
		{"size": SizeCategory.Category.SUPER_EARTH, "zone": OrbitZone.Zone.HOT, "name": "planet_superearth_hot"},
		{"size": SizeCategory.Category.MINI_NEPTUNE, "zone": OrbitZone.Zone.TEMPERATE, "name": "planet_minineptune_temperate"},
		{"size": SizeCategory.Category.NEPTUNE_CLASS, "zone": OrbitZone.Zone.COLD, "name": "planet_neptuneclass_cold"},
		{"size": SizeCategory.Category.GAS_GIANT, "zone": OrbitZone.Zone.COLD, "name": "planet_gasgiant_cold"},
	]
	
	# Create a standard stellar context
	var context: ParentContext = ParentContext.sun_like()
	
	for i in range(planet_configs.size()):
		var config: Dictionary = planet_configs[i]
		var seed_val: int = BASE_SEED + 100 + i
		
		var spec: PlanetSpec = PlanetSpec.new(
			seed_val,
			config["size"],
			config["zone"],
			null,
			null,
			-1,
			"",
			{}
		)
		
		var rng: SeededRng = SeededRng.new(seed_val)
		var body: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		fixtures.append({
			"name": config["name"],
			"type": "planet",
			"seed": seed_val,
			"spec": spec.to_dict(),
			"context": context.to_dict(),
			"body": CelestialSerializer.to_dict(body)
		})
	
	return fixtures


## Generates moon fixtures (7 fixtures covering different moon types).
## @return: Array of moon fixture dictionaries.
static func _generate_moon_fixtures() -> Array[Dictionary]:
	var fixtures: Array[Dictionary] = []
	
	var moon_configs: Array = [
		{"type": "luna_like", "name": "moon_luna_like"},
		{"type": "europa_like", "name": "moon_europa_like"},
		{"type": "titan_like", "name": "moon_titan_like"},
		{"type": "captured", "name": "moon_captured"},
		{"type": "dwarf_regular", "name": "moon_dwarf_regular"},
		{"type": "subterrestrial_regular", "name": "moon_subterrestrial_regular"},
		{"type": "terrestrial_regular", "name": "moon_terrestrial_regular"},
	]
	
	# Create a Jupiter-like parent context
	var jupiter_mass_kg: float = 1.898e27
	var jupiter_radius_m: float = 6.9911e7
	var context: ParentContext = ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		5.2 * Units.AU_METERS,
		jupiter_mass_kg,
		jupiter_radius_m,
		5.0e8  # 500,000 km
	)
	
	for i in range(moon_configs.size()):
		var config: Dictionary = moon_configs[i]
		var seed_val: int = BASE_SEED + 200 + i
		
		var spec: MoonSpec
		match config["type"]:
			"luna_like":
				spec = MoonSpec.luna_like(seed_val)
			"europa_like":
				spec = MoonSpec.europa_like(seed_val)
			"titan_like":
				spec = MoonSpec.titan_like(seed_val)
				# Adjust context for Titan-like (larger orbit)
				context.orbital_distance_from_parent_m = 1.2e9
			"captured":
				spec = MoonSpec.captured(seed_val)
				context.orbital_distance_from_parent_m = 2.0e10
			"dwarf_regular":
				spec = MoonSpec.new(seed_val, SizeCategory.Category.DWARF, false)
				context.orbital_distance_from_parent_m = 3.0e8
			"subterrestrial_regular":
				spec = MoonSpec.new(seed_val, SizeCategory.Category.SUB_TERRESTRIAL, false)
				context.orbital_distance_from_parent_m = 4.0e8
			"terrestrial_regular":
				spec = MoonSpec.new(seed_val, SizeCategory.Category.TERRESTRIAL, false)
				context.orbital_distance_from_parent_m = 6.0e8
			_:
				spec = MoonSpec.random(seed_val)
		
		var rng: SeededRng = SeededRng.new(seed_val)
		var body: CelestialBody = MoonGenerator.generate(spec, context, rng)
		
		fixtures.append({
			"name": config["name"],
			"type": "moon",
			"seed": seed_val,
			"spec": spec.to_dict(),
			"context": context.to_dict(),
			"body": CelestialSerializer.to_dict(body)
		})
	
	return fixtures


## Generates asteroid fixtures (7 fixtures covering different asteroid types).
## @return: Array of asteroid fixture dictionaries.
static func _generate_asteroid_fixtures() -> Array[Dictionary]:
	var fixtures: Array[Dictionary] = []
	
	var asteroid_configs: Array = [
		{"type": "c_type", "large": false, "name": "asteroid_c_type"},
		{"type": "s_type", "large": false, "name": "asteroid_s_type"},
		{"type": "m_type", "large": false, "name": "asteroid_m_type"},
		{"type": "ceres_like", "large": true, "name": "asteroid_ceres_like"},
		{"type": "random_1", "large": false, "name": "asteroid_random_1"},
		{"type": "random_2", "large": false, "name": "asteroid_random_2"},
		{"type": "random_3", "large": false, "name": "asteroid_random_3"},
	]
	
	var context: ParentContext = ParentContext.sun_like(2.7 * Units.AU_METERS)
	
	for i in range(asteroid_configs.size()):
		var config: Dictionary = asteroid_configs[i]
		var seed_val: int = BASE_SEED + 300 + i
		
		var spec: AsteroidSpec
		match config["type"]:
			"c_type":
				spec = AsteroidSpec.carbonaceous(seed_val)
			"s_type":
				spec = AsteroidSpec.stony(seed_val)
			"m_type":
				spec = AsteroidSpec.metallic(seed_val)
			"ceres_like":
				spec = AsteroidSpec.ceres_like(seed_val)
			_:
				spec = AsteroidSpec.random(seed_val)
		
		var rng: SeededRng = SeededRng.new(seed_val)
		var body: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
		
		fixtures.append({
			"name": config["name"],
			"type": "asteroid",
			"seed": seed_val,
			"spec": spec.to_dict(),
			"context": context.to_dict(),
			"body": CelestialSerializer.to_dict(body)
		})
	
	return fixtures


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
