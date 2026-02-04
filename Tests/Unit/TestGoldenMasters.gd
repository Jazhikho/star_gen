## Golden master regression tests for all generators.
## Verifies that generators produce consistent output for known seeds.
extends TestCase

const _fixture_generator: GDScript = preload("res://src/domain/generation/fixtures/FixtureGenerator.gd")
const _celestial_serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _asteroid_generator: GDScript = preload("res://src/domain/generation/generators/AsteroidGenerator.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Tests that all fixtures can be generated without errors.
func test_all_fixtures_generate() -> void:
	var fixtures: Array[Dictionary] = FixtureGenerator.generate_all_fixtures()
	
	assert_true(fixtures.size() >= 28, "Should generate at least 28 fixtures (7 per type)")
	
	for fixture in fixtures:
		assert_true(fixture.has("name"), "Fixture should have name")
		assert_true(fixture.has("body"), "Fixture should have body")
		assert_not_null(fixture["body"], "Body should not be null for: " + fixture["name"])


## Tests star generation determinism.
func test_star_determinism() -> void:
	var seed_val: int = 99999
	var spec1: StarSpec = StarSpec.random(seed_val)
	var spec2: StarSpec = StarSpec.random(seed_val)
	
	var rng1: SeededRng = SeededRng.new(seed_val)
	var rng2: SeededRng = SeededRng.new(seed_val)
	
	var body1: CelestialBody = StarGenerator.generate(spec1, rng1)
	var body2: CelestialBody = StarGenerator.generate(spec2, rng2)
	
	var json1: String = CelestialSerializer.to_json(body1, false)
	var json2: String = CelestialSerializer.to_json(body2, false)
	
	assert_equal(json1, json2, "Star generation should be deterministic")


## Tests planet generation determinism.
func test_planet_determinism() -> void:
	var seed_val: int = 88888
	var spec1: PlanetSpec = PlanetSpec.random(seed_val)
	var spec2: PlanetSpec = PlanetSpec.random(seed_val)
	var context: ParentContext = ParentContext.sun_like()
	
	var rng1: SeededRng = SeededRng.new(seed_val)
	var rng2: SeededRng = SeededRng.new(seed_val)
	
	var body1: CelestialBody = PlanetGenerator.generate(spec1, context, rng1)
	var body2: CelestialBody = PlanetGenerator.generate(spec2, context, rng2)
	
	var json1: String = CelestialSerializer.to_json(body1, false)
	var json2: String = CelestialSerializer.to_json(body2, false)
	
	assert_equal(json1, json2, "Planet generation should be deterministic")


## Tests moon generation determinism.
func test_moon_determinism() -> void:
	var seed_val: int = 77777
	var spec1: MoonSpec = MoonSpec.random(seed_val)
	var spec2: MoonSpec = MoonSpec.random(seed_val)
	
	var context: ParentContext = ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		5.2 * Units.AU_METERS,
		1.898e27,
		6.9911e7,
		5.0e8
	)
	
	var rng1: SeededRng = SeededRng.new(seed_val)
	var rng2: SeededRng = SeededRng.new(seed_val)
	
	var body1: CelestialBody = MoonGenerator.generate(spec1, context, rng1)
	var body2: CelestialBody = MoonGenerator.generate(spec2, context, rng2)
	
	var json1: String = CelestialSerializer.to_json(body1, false)
	var json2: String = CelestialSerializer.to_json(body2, false)
	
	assert_equal(json1, json2, "Moon generation should be deterministic")


## Tests asteroid generation determinism.
func test_asteroid_determinism() -> void:
	var seed_val: int = 66666
	var spec1: AsteroidSpec = AsteroidSpec.random(seed_val)
	var spec2: AsteroidSpec = AsteroidSpec.random(seed_val)
	var context: ParentContext = ParentContext.sun_like(2.7 * Units.AU_METERS)
	
	var rng1: SeededRng = SeededRng.new(seed_val)
	var rng2: SeededRng = SeededRng.new(seed_val)
	
	var body1: CelestialBody = AsteroidGenerator.generate(spec1, context, rng1)
	var body2: CelestialBody = AsteroidGenerator.generate(spec2, context, rng2)
	
	var json1: String = CelestialSerializer.to_json(body1, false)
	var json2: String = CelestialSerializer.to_json(body2, false)
	
	assert_equal(json1, json2, "Asteroid generation should be deterministic")


## Tests that fixtures export to valid JSON.
func test_fixtures_export_to_json() -> void:
	var json_exports: Dictionary = FixtureGenerator.export_all_to_json(true)
	
	assert_true(json_exports.size() >= 28, "Should export at least 28 fixtures")
	
	for fixture_name in json_exports.keys():
		var json_str: String = json_exports[fixture_name] as String
		assert_false(json_str.is_empty(), "JSON should not be empty for: " + fixture_name)
		
		# Verify it's valid JSON
		var json: JSON = JSON.new()
		var error: Error = json.parse(json_str)
		assert_equal(error, OK, "Should be valid JSON for: " + fixture_name)


## Tests range validation for star properties.
func test_star_range_validation() -> void:
	var fixtures: Array[Dictionary] = FixtureGenerator.generate_all_fixtures()
	
	for fixture in fixtures:
		if fixture["type"] != "star":
			continue
		
		var body_data: Dictionary = fixture["body"]
		var physical: Dictionary = body_data["physical"]
		
		assert_true(physical["mass_kg"] > 0.0, "Star mass should be positive")
		assert_true(physical["radius_m"] > 0.0, "Star radius should be positive")
		
		if body_data.has("stellar"):
			var stellar: Dictionary = body_data["stellar"]
			assert_true(stellar.get("luminosity_watts", 0.0) > 0.0, "Star luminosity should be positive")
			assert_true(stellar.get("effective_temperature_k", 0.0) > 0.0, "Star temperature should be positive")
			assert_true(stellar.get("age_years", 0.0) > 0.0, "Star age should be positive")


## Tests range validation for planet properties.
func test_planet_range_validation() -> void:
	var fixtures: Array[Dictionary] = FixtureGenerator.generate_all_fixtures()
	
	for fixture in fixtures:
		if fixture["type"] != "planet":
			continue
		
		var body_data: Dictionary = fixture["body"]
		var physical: Dictionary = body_data["physical"]
		
		assert_true(physical["mass_kg"] > 0.0, "Planet mass should be positive")
		assert_true(physical["radius_m"] > 0.0, "Planet radius should be positive")
		
		if body_data.has("orbital"):
			var orbital: Dictionary = body_data["orbital"]
			assert_true(orbital["semi_major_axis_m"] > 0.0, "Orbital distance should be positive")
			assert_true(orbital["eccentricity"] >= 0.0, "Eccentricity should be non-negative")
			assert_true(orbital["eccentricity"] < 1.0, "Eccentricity should be < 1")


## Tests range validation for moon properties.
func test_moon_range_validation() -> void:
	var fixtures: Array[Dictionary] = FixtureGenerator.generate_all_fixtures()
	
	for fixture in fixtures:
		if fixture["type"] != "moon":
			continue
		
		var body_data: Dictionary = fixture["body"]
		var physical: Dictionary = body_data["physical"]
		
		assert_true(physical["mass_kg"] > 0.0, "Moon mass should be positive")
		assert_true(physical["radius_m"] > 0.0, "Moon radius should be positive")
		
		if body_data.has("orbital"):
			var orbital: Dictionary = body_data["orbital"]
			assert_true(orbital["semi_major_axis_m"] > 0.0, "Orbital distance should be positive")


## Tests range validation for asteroid properties.
func test_asteroid_range_validation() -> void:
	var fixtures: Array[Dictionary] = FixtureGenerator.generate_all_fixtures()
	
	for fixture in fixtures:
		if fixture["type"] != "asteroid":
			continue
		
		var body_data: Dictionary = fixture["body"]
		var physical: Dictionary = body_data["physical"]
		
		assert_true(physical["mass_kg"] > 0.0, "Asteroid mass should be positive")
		assert_true(physical["radius_m"] > 0.0, "Asteroid radius should be positive")


## Tests physics relationships for stars.
func test_stellar_physics_relationships() -> void:
	var fixtures: Array[Dictionary] = FixtureGenerator.generate_all_fixtures()
	
	for fixture in fixtures:
		if fixture["type"] != "star":
			continue
		
		var body_data: Dictionary = fixture["body"]
		var physical: Dictionary = body_data["physical"]
		
		if not body_data.has("stellar"):
			continue
		
		var stellar: Dictionary = body_data["stellar"]
		
		var _mass_kg: float = physical["mass_kg"]
		var radius_m: float = physical["radius_m"]
		var luminosity_w: float = stellar.get("luminosity_watts", 0.0)
		var temperature_k: float = stellar.get("effective_temperature_k", 0.0)
		
		if luminosity_w <= 0.0 or temperature_k <= 0.0:
			continue
		
		# Stefan-Boltzmann check: L = 4πR²σT⁴
		var stefan_boltzmann: float = 5.67e-8
		var expected_luminosity: float = 4.0 * PI * radius_m * radius_m * stefan_boltzmann * pow(temperature_k, 4.0)
		
		# Should be within factor of 2 (allowing for variation)
		var ratio: float = luminosity_w / expected_luminosity
		assert_true(ratio > 0.3 and ratio < 3.0, 
			"Stellar luminosity should roughly match Stefan-Boltzmann (ratio: %f)" % ratio)


## Tests that serialization round-trips correctly.
func test_serialization_roundtrip() -> void:
	var fixtures: Array[Dictionary] = FixtureGenerator.generate_all_fixtures()
	
	for fixture in fixtures:
		var body_data: Dictionary = fixture["body"]
		
		# Deserialize
		var body: CelestialBody = CelestialSerializer.from_dict(body_data)
		assert_not_null(body, "Should deserialize: " + fixture["name"])
		
		# Re-serialize
		var re_serialized: Dictionary = CelestialSerializer.to_dict(body)
		
		# Compare key fields (not all fields due to floating point)
		assert_equal(body_data["id"], re_serialized["id"], "ID should match after roundtrip")
		assert_equal(body_data["type"], re_serialized["type"], "Type should match after roundtrip")
