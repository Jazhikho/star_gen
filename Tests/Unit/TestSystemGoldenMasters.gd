## Golden master regression tests for solar system generation.
## Ensures generated systems remain deterministic across code changes.
extends TestCase

const _system_fixture_generator := preload("res://src/domain/system/fixtures/SystemFixtureGenerator.gd")
const _system_serializer := preload("res://src/domain/system/SystemSerializer.gd")
const _system_validator := preload("res://src/domain/system/SystemValidator.gd")
const _solar_system_spec := preload("res://src/domain/system/SolarSystemSpec.gd")
const _solar_system := preload("res://src/domain/system/SolarSystem.gd")
const _validation_result := preload("res://src/domain/celestial/validation/ValidationResult.gd")


## Cached fixtures for testing.
var _fixtures: Array[Dictionary] = []


## Loads fixtures before tests.
func before_all() -> void:
	_fixtures = SystemFixtureGenerator.generate_all_fixtures()


## Tests that fixtures are generated.
func test_fixtures_generated() -> void:
	assert_greater_than(_fixtures.size(), 0, "Should generate fixtures")


## Tests single star Sun-like system.
func test_fixture_single_sun_like() -> void:
	var fixture: Dictionary = _find_fixture("system_single_sun_like")
	assert_not_null(fixture, "Fixture should exist")
	
	_verify_fixture_regenerates(fixture)


## Tests single star red dwarf system.
func test_fixture_single_red_dwarf() -> void:
	var fixture: Dictionary = _find_fixture("system_single_red_dwarf")
	assert_not_null(fixture, "Fixture should exist")
	
	_verify_fixture_regenerates(fixture)


## Tests binary equal mass system.
func test_fixture_binary_equal() -> void:
	var fixture: Dictionary = _find_fixture("system_binary_equal")
	assert_not_null(fixture, "Fixture should exist")
	
	_verify_fixture_regenerates(fixture)


## Tests binary unequal mass system.
func test_fixture_binary_unequal() -> void:
	var fixture: Dictionary = _find_fixture("system_binary_unequal")
	assert_not_null(fixture, "Fixture should exist")
	
	_verify_fixture_regenerates(fixture)


## Tests triple hierarchical system.
func test_fixture_triple_hierarchical() -> void:
	var fixture: Dictionary = _find_fixture("system_triple_hierarchical")
	assert_not_null(fixture, "Fixture should exist")
	
	_verify_fixture_regenerates(fixture)


## Tests quadruple system.
func test_fixture_quadruple() -> void:
	var fixture: Dictionary = _find_fixture("system_quadruple")
	assert_not_null(fixture, "Fixture should exist")
	
	_verify_fixture_regenerates(fixture)


## Tests maximum stars system.
func test_fixture_max_stars() -> void:
	var fixture: Dictionary = _find_fixture("system_max_stars")
	assert_not_null(fixture, "Fixture should exist")
	
	_verify_fixture_regenerates(fixture)


## Tests all fixtures pass validation.
func test_all_fixtures_valid() -> void:
	for fixture in _fixtures:
		var system_data: Dictionary = fixture.get("system", {}) as Dictionary
		var system: SolarSystem = SystemSerializer.from_dict(system_data)
		
		assert_not_null(system, "Fixture %s should deserialize" % fixture["name"])
		
		var result: ValidationResult = SystemValidator.validate(system)
		assert_true(result.is_valid(), "Fixture %s should be valid" % fixture["name"])


## Tests fixture star counts match spec.
func test_fixture_star_counts() -> void:
	for fixture in _fixtures:
		var spec_data: Dictionary = fixture.get("spec", {}) as Dictionary
		var system_data: Dictionary = fixture.get("system", {}) as Dictionary
		
		var system: SolarSystem = SystemSerializer.from_dict(system_data)
		if system == null:
			continue
		
		var min_stars: int = spec_data.get("star_count_min", 1) as int
		var max_stars: int = spec_data.get("star_count_max", 1) as int
		var actual_stars: int = system.get_star_count()
		
		assert_in_range(
			actual_stars,
			min_stars,
			max_stars,
			"Fixture %s star count should match spec" % fixture["name"]
		)


## Tests fixture serialization round-trip.
func test_fixture_serialization_roundtrip() -> void:
	for fixture in _fixtures:
		var original_data: Dictionary = fixture.get("system", {}) as Dictionary
		var system: SolarSystem = SystemSerializer.from_dict(original_data)
		
		if system == null:
			continue
		
		var reserialized: Dictionary = SystemSerializer.to_dict(system)
		var restored: SolarSystem = SystemSerializer.from_dict(reserialized)
		
		assert_not_null(restored, "Fixture %s should round-trip" % fixture["name"])
		assert_equal(restored.id, system.id)
		assert_equal(restored.get_star_count(), system.get_star_count())
		assert_equal(restored.get_planet_count(), system.get_planet_count())


## Finds a fixture by name.
## @param name: Fixture name.
## @return: Fixture dictionary, or empty dict if not found.
func _find_fixture(fixture_name: String) -> Dictionary:
	for fixture in _fixtures:
		if fixture.get("name", "") == fixture_name:
			return fixture
	return {}


## Verifies a fixture regenerates identically.
## @param fixture: The fixture dictionary.
func _verify_fixture_regenerates(fixture: Dictionary) -> void:
	var spec_data: Dictionary = fixture.get("spec", {}) as Dictionary
	var original_system_data: Dictionary = fixture.get("system", {}) as Dictionary
	
	# Recreate spec
	var spec: SolarSystemSpec = SolarSystemSpec.from_dict(spec_data)
	
	# Regenerate system
	var regenerated: SolarSystem = SystemFixtureGenerator.generate_system(spec)
	assert_not_null(regenerated, "Should regenerate system")
	
	# Compare key properties
	var original: SolarSystem = SystemSerializer.from_dict(original_system_data)
	assert_not_null(original)
	
	assert_equal(
		regenerated.get_star_count(),
		original.get_star_count(),
		"Star count should match"
	)
	
	assert_equal(
		regenerated.get_planet_count(),
		original.get_planet_count(),
		"Planet count should match"
	)
	
	assert_equal(
		regenerated.get_moon_count(),
		original.get_moon_count(),
		"Moon count should match"
	)
	
	# Compare star masses
	var orig_stars: Array[CelestialBody] = original.get_stars()
	var regen_stars: Array[CelestialBody] = regenerated.get_stars()
	
	for i in range(mini(orig_stars.size(), regen_stars.size())):
		assert_float_equal(
			orig_stars[i].physical.mass_kg,
			regen_stars[i].physical.mass_kg,
			orig_stars[i].physical.mass_kg * 0.001,  # 0.1% tolerance
			"Star %d mass should match" % i
		)
