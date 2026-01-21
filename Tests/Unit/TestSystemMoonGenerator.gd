## Tests for SystemMoonGenerator.
extends TestCase

const _system_moon_generator := preload("res://src/domain/system/SystemMoonGenerator.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props := preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _orbital_mechanics := preload("res://src/domain/system/OrbitalMechanics.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")
const _star_spec := preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_generator := preload("res://src/domain/generation/generators/StarGenerator.gd")
const _celestial_validator := preload("res://src/domain/celestial/validation/CelestialValidator.gd")
const _validation_result := preload("res://src/domain/celestial/validation/ValidationResult.gd")


## Creates a gas giant planet for testing.
func _create_gas_giant() -> CelestialBody:
	var planet: CelestialBody = CelestialBody.new(
		"gas_giant_1",
		"Gas Giant",
		CelestialType.Type.PLANET
	)
	planet.physical = PhysicalProps.new(
		Units.JUPITER_MASS_KG,
		Units.JUPITER_RADIUS_METERS,
		35730.0,
		3.0,
		0.06,
		1.0e20,
		1.0e17
	)
	planet.orbital = OrbitalProps.new(
		5.2 * Units.AU_METERS,
		0.05,
		1.3,
		0.0,
		0.0,
		0.0,
		"test_star"
	)
	return planet


## Creates a terrestrial planet for testing.
func _create_terrestrial() -> CelestialBody:
	var planet: CelestialBody = CelestialBody.new(
		"terrestrial_1",
		"Terrestrial",
		CelestialType.Type.PLANET
	)
	planet.physical = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0,
		23.5,
		0.003,
		1.0e15,
		1.0e13
	)
	planet.orbital = OrbitalProps.new(
		1.0 * Units.AU_METERS,
		0.017,
		0.0,
		0.0,
		0.0,
		0.0,
		"test_star"
	)
	return planet


## Creates a dwarf planet for testing.
func _create_dwarf_planet() -> CelestialBody:
	var planet: CelestialBody = CelestialBody.new(
		"dwarf_1",
		"Dwarf Planet",
		CelestialType.Type.PLANET
	)
	planet.physical = PhysicalProps.new(
		1.3e22,  # Pluto-like mass
		1.2e6,   # Pluto-like radius
		552960.0,
		120.0,
		0.0,
		0.0,
		0.0
	)
	planet.orbital = OrbitalProps.new(
		40.0 * Units.AU_METERS,
		0.25,
		17.0,
		0.0,
		0.0,
		0.0,
		"test_star"
	)
	return planet


## Creates a test star.
func _create_test_star() -> CelestialBody:
	var spec: StarSpec = StarSpec.sun_like(12345)
	var rng: SeededRng = SeededRng.new(12345)
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	star.id = "test_star"
	return star


## Tests basic moon generation for gas giant.
func test_generate_gas_giant_moons() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(12345)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet],
		[],
		[star],
		rng
	)
	
	assert_true(result.success, "Generation should succeed")
	assert_greater_than(result.moons.size(), 0, "Gas giant should have moons")


## Tests gas giants have more moons than terrestrial planets.
func test_gas_giant_has_more_moons() -> void:
	var gas_giant: CelestialBody = _create_gas_giant()
	var terrestrial: CelestialBody = _create_terrestrial()
	var star: CelestialBody = _create_test_star()
	
	var gas_giant_moon_count: int = 0
	var terrestrial_moon_count: int = 0
	
	# Run multiple times to get average
	for i in range(20):
		var rng1: SeededRng = SeededRng.new(10000 + i)
		var rng2: SeededRng = SeededRng.new(20000 + i)
		
		var result1: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
			[gas_giant], [], [star], rng1
		)
		var result2: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
			[terrestrial], [], [star], rng2
		)
		
		gas_giant_moon_count += result1.moons.size()
		terrestrial_moon_count += result2.moons.size()
	
	assert_greater_than(
		gas_giant_moon_count,
		terrestrial_moon_count,
		"Gas giants should have more moons on average"
	)


## Tests determinism.
func test_determinism() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng1: SeededRng = SeededRng.new(55555)
	var rng2: SeededRng = SeededRng.new(55555)
	
	var result1: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng1
	)
	var result2: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng2
	)
	
	assert_equal(result1.moons.size(), result2.moons.size(), "Same seed should give same count")
	
	for i in range(result1.moons.size()):
		assert_float_equal(
			result1.moons[i].physical.mass_kg,
			result2.moons[i].physical.mass_kg,
			1.0,
			"Same seed should give same moons"
		)


## Tests moons have parent IDs set correctly.
func test_moon_parent_ids() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(66666)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	for moon in result.moons:
		assert_true(moon.has_orbital(), "Moon should have orbital data")
		assert_equal(moon.orbital.parent_id, planet.id, "Moon parent ID should match planet")


## Tests moons are within Hill sphere.
func test_moons_within_hill_sphere() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(77777)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	var hill_radius: float = OrbitalMechanics.calculate_hill_sphere(
		planet.physical.mass_kg,
		star.physical.mass_kg,
		planet.orbital.semi_major_axis_m
	)
	
	for moon in result.moons:
		assert_true(moon.has_orbital(), "Moon should have orbital data")
		assert_less_than(
			moon.orbital.semi_major_axis_m,
			hill_radius,
			"Moon should be within Hill sphere"
		)


## Tests moons are outside planet radius.
func test_moons_outside_planet_surface() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(88888)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	for moon in result.moons:
		assert_greater_than(
			moon.orbital.semi_major_axis_m,
			planet.physical.radius_m,
			"Moon should orbit outside planet surface"
		)


## Tests multiple planets get moons.
func test_multiple_planets() -> void:
	var gas_giant: CelestialBody = _create_gas_giant()
	var terrestrial: CelestialBody = _create_terrestrial()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(99999)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[gas_giant, terrestrial],
		[],
		[star],
		rng
	)
	
	assert_true(result.success)
	
	# Check planet_moon_map
	if result.planet_moon_map.has(gas_giant.id):
		var gas_giant_moons: Array = result.planet_moon_map[gas_giant.id]
		assert_greater_than(gas_giant_moons.size(), 0, "Gas giant should have moon entries")


## Tests get_moons_for_planet.
func test_get_moons_for_planet() -> void:
	var gas_giant: CelestialBody = _create_gas_giant()
	var terrestrial: CelestialBody = _create_terrestrial()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(11111)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[gas_giant, terrestrial],
		[],
		[star],
		rng
	)
	
	var gas_giant_moons: Array[CelestialBody] = SystemMoonGenerator.get_moons_for_planet(
		result.moons,
		gas_giant.id
	)
	var terrestrial_moons: Array[CelestialBody] = SystemMoonGenerator.get_moons_for_planet(
		result.moons,
		terrestrial.id
	)
	
	# All returned moons should have correct parent
	for moon in gas_giant_moons:
		assert_equal(moon.orbital.parent_id, gas_giant.id)
	
	for moon in terrestrial_moons:
		assert_equal(moon.orbital.parent_id, terrestrial.id)


## Tests sort_by_distance.
func test_sort_by_distance() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(22222)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	if result.moons.size() < 2:
		return  # Need at least 2 moons to test sorting
	
	SystemMoonGenerator.sort_by_distance(result.moons)
	
	for i in range(result.moons.size() - 1):
		assert_less_than(
			result.moons[i].orbital.semi_major_axis_m,
			result.moons[i + 1].orbital.semi_major_axis_m,
			"Moons should be sorted by distance"
		)


## Tests get_statistics.
func test_get_statistics() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(33333)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	var stats: Dictionary = SystemMoonGenerator.get_statistics(result.moons)
	
	assert_equal(stats["total"], result.moons.size())
	
	if result.moons.size() > 0:
		assert_greater_than(stats["avg_mass_earth"], 0.0)


## Tests validate_moon_planet_consistency.
func test_validate_consistency() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(44444)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	var is_valid: bool = SystemMoonGenerator.validate_moon_planet_consistency(
		result.moons,
		[planet]
	)
	
	assert_true(is_valid, "Generated moons should be consistent with planets")


## Tests moons pass validation.
func test_moons_pass_validation() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(55555)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	for moon in result.moons:
		var validation: ValidationResult = CelestialValidator.validate(moon)
		assert_true(validation.is_valid(), "Generated moon should pass validation")


## Tests moon IDs are unique.
func test_moon_ids_unique() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(66666)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	var ids: Dictionary = {}
	for moon in result.moons:
		assert_false(ids.has(moon.id), "Moon IDs should be unique")
		ids[moon.id] = true


## Tests moon names are assigned.
func test_moon_names() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(77777)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	for moon in result.moons:
		assert_false(moon.name.is_empty(), "Moon should have a name")
		assert_true(moon.name.contains(planet.name), "Moon name should include planet name")


## Tests dwarf planets can have moons (like Pluto-Charon).
func test_dwarf_planet_can_have_moons() -> void:
	var dwarf: CelestialBody = _create_dwarf_planet()
	var star: CelestialBody = _create_test_star()
	
	# Find a seed that produces a moon for a dwarf planet
	# This tests that the mechanism works, not the probability
	var moon_found: bool = false
	var working_seed: int = -1
	
	for i in range(200):
		var rng: SeededRng = SeededRng.new(80000 + i)
		
		var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
			[dwarf], [], [star], rng
		)
		
		if result.moons.size() > 0:
			moon_found = true
			working_seed = 80000 + i
			
			# Verify the moon is valid
			var moon: CelestialBody = result.moons[0]
			assert_true(moon.has_orbital(), "Moon should have orbital data")
			assert_equal(moon.orbital.parent_id, dwarf.id, "Moon should orbit the dwarf planet")
			break
	
	assert_true(moon_found, "Dwarf planets should be capable of having moons (found at seed %d)" % working_seed)


## Tests captured moons are generated for outer moons.
func test_captured_moons() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	
	var captured_found: bool = false
	
	for i in range(30):
		var rng: SeededRng = SeededRng.new(90000 + i)
		
		var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
			[planet], [], [star], rng
		)
		
		for moon in result.moons:
			if moon.name.contains("captured"):
				captured_found = true
				break
		
		if captured_found:
			break
	
	assert_true(captured_found, "Some outer moons should be captured")


## Tests moon orbital distances increase.
func test_moon_distances_ordered() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(11111)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	var planet_moons: Array[CelestialBody] = SystemMoonGenerator.get_moons_for_planet(
		result.moons,
		planet.id
	)
	
	SystemMoonGenerator.sort_by_distance(planet_moons)
	
	# Check reasonable spacing between moons
	if planet_moons.size() >= 2:
		for i in range(planet_moons.size() - 1):
			var dist_i: float = planet_moons[i].orbital.semi_major_axis_m
			var dist_next: float = planet_moons[i + 1].orbital.semi_major_axis_m
			
			assert_greater_than(dist_next, dist_i * 1.1, "Moons should have some spacing")


## Tests assign_greek_letter_names.
func test_assign_greek_letter_names() -> void:
	var planet: CelestialBody = _create_gas_giant()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(88888)
	
	var result: SystemMoonGenerator.MoonGenerationResult = SystemMoonGenerator.generate(
		[planet], [], [star], rng
	)
	
	if result.moons.is_empty():
		return
	
	SystemMoonGenerator.sort_by_distance(result.moons)
	SystemMoonGenerator.assign_greek_letter_names(result.moons, "Gas Giant")
	
	if result.moons.size() > 0:
		assert_true(result.moons[0].name.contains("Alpha"), "First moon should be Alpha")
	if result.moons.size() > 1:
		assert_true(result.moons[1].name.contains("Beta"), "Second moon should be Beta")
