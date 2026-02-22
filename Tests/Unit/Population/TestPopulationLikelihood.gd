## Tests for PopulationLikelihood: likelihood estimation and seed-based population checks.
extends TestCase

const _population_likelihood: GDScript = preload("res://src/domain/population/PopulationLikelihood.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _population_probability: GDScript = preload("res://src/domain/population/PopulationProbability.gd")


## Tests that estimate_native_likelihood matches PopulationProbability.calculate_native_probability.
func test_estimate_native_likelihood_matches_probability() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "test"
	profile.habitability_score = 7
	profile.has_liquid_water = true
	profile.has_breathable_atmosphere = false
	profile.is_tidally_locked = false
	profile.radiation_level = 0.3
	profile.is_moon = false
	profile.tidal_heating_factor = 0.0

	var likelihood: float = PopulationLikelihood.estimate_native_likelihood(profile)
	var prob: float = PopulationProbability.calculate_native_probability(profile)
	assert_float_equal(likelihood, prob, 0.0001, "Likelihood should match probability calculation")


## Tests that derive_roll_value is deterministic for same seed + salt.
func test_derive_roll_value_deterministic() -> void:
	var seed_val: int = 12345
	var salt: int = PopulationLikelihood.NATIVE_ROLL_SALT

	var roll1: float = PopulationLikelihood.derive_roll_value(seed_val, salt)
	var roll2: float = PopulationLikelihood.derive_roll_value(seed_val, salt)
	assert_float_equal(roll1, roll2, 0.0, "Same seed+salt must yield same roll")


## Tests that derive_roll_value returns value in [0, 1).
func test_derive_roll_value_in_range() -> void:
	for i in range(100):
		var seed_val: int = i * 7777
		var roll: float = PopulationLikelihood.derive_roll_value(seed_val, PopulationLikelihood.COLONY_ROLL_SALT)
		assert_true(roll >= 0.0, "Roll must be >= 0 for seed %d" % seed_val)
		assert_true(roll < 1.0, "Roll must be < 1 for seed %d" % seed_val)


## Tests that native and colony use different derived values (different salts).
func test_native_and_colony_rolls_differ() -> void:
	var seed_val: int = 99999
	var native_roll: float = PopulationLikelihood.derive_roll_value(seed_val, PopulationLikelihood.NATIVE_ROLL_SALT)
	var colony_roll: float = PopulationLikelihood.derive_roll_value(seed_val, PopulationLikelihood.COLONY_ROLL_SALT)
	assert_not_equal(native_roll, colony_roll, "Different salts must yield different rolls")


## Tests should_generate_natives is deterministic for same profile + seed.
func test_should_generate_natives_deterministic() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "body1"
	profile.habitability_score = 8
	profile.has_liquid_water = true
	profile.has_breathable_atmosphere = true
	profile.is_tidally_locked = false
	profile.radiation_level = 0.2
	profile.is_moon = false
	profile.tidal_heating_factor = 0.0

	var pop_seed: int = 123456
	var result1: bool = PopulationLikelihood.should_generate_natives(profile, pop_seed)
	var result2: bool = PopulationLikelihood.should_generate_natives(profile, pop_seed)
	assert_equal(result1, result2, "Same profile+seed must yield same natives decision")


## Tests should_generate_natives returns false when habitability too low.
func test_should_generate_natives_zero_when_uninhabitable() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "hostile"
	profile.habitability_score = 1
	profile.has_liquid_water = false
	profile.has_breathable_atmosphere = false
	profile.is_tidally_locked = true
	profile.radiation_level = 1.0

	var result: bool = PopulationLikelihood.should_generate_natives(profile, 99999)
	assert_false(result, "Very low habitability should never produce natives")


## Tests Override enum values.
func test_override_enum_values() -> void:
	assert_equal(PopulationLikelihood.Override.AUTO, 0, "AUTO should be 0")
	assert_equal(PopulationLikelihood.Override.NONE, 1, "NONE should be 1")
	assert_equal(PopulationLikelihood.Override.FORCE_NATIVES, 2, "FORCE_NATIVES should be 2")
	assert_equal(PopulationLikelihood.Override.FORCE_COLONY, 3, "FORCE_COLONY should be 3")
