## Golden master tests for population generation determinism.
## Verifies that the same seed always produces the same population data
## across regeneration, including profile, suitability, natives, and colonies.
extends TestCase

const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _celestial_serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _population_seeding: GDScript = preload("res://src/domain/population/PopulationSeeding.gd")

## Known seeds for golden master fixtures.
const GOLDEN_SEEDS: Array[int] = [42, 100, 255, 999, 12345]


## Tests that multiple regenerations of the same planet produce identical population data.
func test_planet_population_determinism_across_regenerations() -> void:
	for seed_val in GOLDEN_SEEDS:
		var spec1: PlanetSpec = PlanetSpec.new(seed_val, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
		var spec2: PlanetSpec = PlanetSpec.new(seed_val, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
		var context: ParentContext = ParentContext.sun_like()
		
		var rng1: SeededRng = SeededRng.new(seed_val)
		var rng2: SeededRng = SeededRng.new(seed_val)
		
		var body1: CelestialBody = PlanetGenerator.generate(spec1, context, rng1, true)
		var body2: CelestialBody = PlanetGenerator.generate(spec2, context, rng2, true)
		
		_assert_population_data_equal(body1, body2, "seed %d" % seed_val)


## Tests that population data survives dict serialization round-trip.
func test_population_dict_roundtrip_determinism() -> void:
	for seed_val in GOLDEN_SEEDS:
		var spec: PlanetSpec = PlanetSpec.new(seed_val, SizeCategory.Category.SUPER_EARTH, OrbitZone.Zone.TEMPERATE)
		var context: ParentContext = ParentContext.sun_like()
		var rng: SeededRng = SeededRng.new(seed_val)
		
		var original: CelestialBody = PlanetGenerator.generate(spec, context, rng, true)
		
		var data: Dictionary = CelestialSerializer.to_dict(original)
		var restored: CelestialBody = CelestialSerializer.from_dict(data)
		
		_assert_population_data_equal(original, restored, "dict roundtrip seed %d" % seed_val)


## Tests that population data survives JSON serialization round-trip.
func test_population_json_roundtrip_determinism() -> void:
	for seed_val in GOLDEN_SEEDS:
		var spec: PlanetSpec = PlanetSpec.new(seed_val, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
		var context: ParentContext = ParentContext.sun_like()
		var rng: SeededRng = SeededRng.new(seed_val)
		
		var original: CelestialBody = PlanetGenerator.generate(spec, context, rng, true)
		
		var json_str: String = CelestialSerializer.to_json(original)
		var restored: CelestialBody = CelestialSerializer.from_json(json_str)
		
		_assert_population_data_equal(original, restored, "json roundtrip seed %d" % seed_val)


## Tests population seeding is order-independent.
func test_population_seeding_order_independence() -> void:
	var base_seed: int = 42000
	var body_ids: Array[String] = ["planet_01", "planet_02", "planet_03", "moon_01_01", "moon_02_01"]
	
	# Generate seeds in forward order
	var forward_seeds: Dictionary = {}
	for id in body_ids:
		forward_seeds[id] = PopulationSeeding.generate_population_seed(id, base_seed)
	
	# Generate seeds in reverse order
	var reverse_seeds: Dictionary = {}
	var reversed_ids: Array[String] = body_ids.duplicate()
	reversed_ids.reverse()
	for id in reversed_ids:
		reverse_seeds[id] = PopulationSeeding.generate_population_seed(id, base_seed)
	
	# Verify same results regardless of order
	for id in body_ids:
		assert_equal(
			forward_seeds[id], reverse_seeds[id],
			"Seed for '%s' should be same regardless of generation order" % id
		)


## Tests that enabling population does not change the core body generation.
func test_population_does_not_affect_core_generation() -> void:
	var seed_val: int = 42
	var spec_no_pop: PlanetSpec = PlanetSpec.new(seed_val, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
	var spec_pop: PlanetSpec = PlanetSpec.new(seed_val, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
	var context: ParentContext = ParentContext.sun_like()
	
	var rng_no_pop: SeededRng = SeededRng.new(seed_val)
	var rng_pop: SeededRng = SeededRng.new(seed_val)
	
	var body_no_pop: CelestialBody = PlanetGenerator.generate(spec_no_pop, context, rng_no_pop, false)
	var body_pop: CelestialBody = PlanetGenerator.generate(spec_pop, context, rng_pop, true)
	
	# Core properties should be identical
	assert_equal(body_no_pop.id, body_pop.id, "IDs should match")
	assert_float_equal(body_no_pop.physical.mass_kg, body_pop.physical.mass_kg, 1.0, "Mass should match")
	assert_float_equal(body_no_pop.physical.radius_m, body_pop.physical.radius_m, 1.0, "Radius should match")


## Asserts that two bodies have equal population data.
## @param a: First body.
## @param b: Second body.
## @param context_msg: Context message for failures.
func _assert_population_data_equal(a: CelestialBody, b: CelestialBody, context_msg: String) -> void:
	assert_true(a.has_population_data(), "Body A should have population data (%s)" % context_msg)
	assert_true(b.has_population_data(), "Body B should have population data (%s)" % context_msg)
	
	if not a.has_population_data() or not b.has_population_data():
		return
	
	var pa: PlanetPopulationData = a.population_data
	var pb: PlanetPopulationData = b.population_data
	
	# Profile
	if pa.profile != null and pb.profile != null:
		assert_equal(
			pa.profile.habitability_score, pb.profile.habitability_score,
			"Habitability scores should match (%s)" % context_msg
		)
		assert_float_equal(
			pa.profile.avg_temperature_k, pb.profile.avg_temperature_k, 0.01,
			"Temperatures should match (%s)" % context_msg
		)
		assert_float_equal(
			pa.profile.ocean_coverage, pb.profile.ocean_coverage, 0.001,
			"Ocean coverage should match (%s)" % context_msg
		)
	
	# Suitability
	if pa.suitability != null and pb.suitability != null:
		assert_equal(
			pa.suitability.overall_score, pb.suitability.overall_score,
			"Suitability scores should match (%s)" % context_msg
		)
	
	# Natives
	assert_equal(
		pa.native_populations.size(), pb.native_populations.size(),
		"Native population counts should match (%s)" % context_msg
	)
	for i in range(mini(pa.native_populations.size(), pb.native_populations.size())):
		assert_equal(
			pa.native_populations[i].population, pb.native_populations[i].population,
			"Native population %d count should match (%s)" % [i, context_msg]
		)
	
	# Colonies
	assert_equal(
		pa.colonies.size(), pb.colonies.size(),
		"Colony counts should match (%s)" % context_msg
	)
	for i in range(mini(pa.colonies.size(), pb.colonies.size())):
		assert_equal(
			pa.colonies[i].population, pb.colonies[i].population,
			"Colony %d population should match (%s)" % [i, context_msg]
		)
