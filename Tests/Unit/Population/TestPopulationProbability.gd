## Tests for PopulationProbability probability calculation and deterministic rolling.
extends TestCase

const _population_probability: GDScript = preload("res://src/domain/population/PopulationProbability.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _suitability_calculator: GDScript = preload("res://src/domain/population/SuitabilityCalculator.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Tests that uninhabitable planets return zero native probability.
func test_zero_probability_for_low_habitability() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 0
	
	var probability: float = PopulationProbability.calculate_native_probability(profile)
	assert_float_equal(probability, 0.0, 0.001, "Score 0 should give 0 probability")
	
	profile.habitability_score = 2
	probability = PopulationProbability.calculate_native_probability(profile)
	assert_float_equal(probability, 0.0, 0.001, "Score 2 should give 0 probability (below threshold)")


## Tests that higher habitability scores give higher probabilities.
func test_probability_increases_with_habitability() -> void:
	var profile_low: PlanetProfile = PlanetProfile.new()
	profile_low.habitability_score = 3
	
	var profile_high: PlanetProfile = PlanetProfile.new()
	profile_high.habitability_score = 8
	
	var prob_low: float = PopulationProbability.calculate_native_probability(profile_low)
	var prob_high: float = PopulationProbability.calculate_native_probability(profile_high)
	
	assert_true(prob_high > prob_low, "Higher habitability should give higher probability")


## Tests that liquid water adds a bonus.
func test_liquid_water_bonus() -> void:
	var profile_dry: PlanetProfile = PlanetProfile.new()
	profile_dry.habitability_score = 5
	profile_dry.has_liquid_water = false
	
	var profile_wet: PlanetProfile = PlanetProfile.new()
	profile_wet.habitability_score = 5
	profile_wet.has_liquid_water = true
	
	var prob_dry: float = PopulationProbability.calculate_native_probability(profile_dry)
	var prob_wet: float = PopulationProbability.calculate_native_probability(profile_wet)
	
	assert_true(prob_wet > prob_dry, "Liquid water should increase probability")


## Tests that breathable atmosphere adds a bonus.
func test_breathable_atmosphere_bonus() -> void:
	var profile_no: PlanetProfile = PlanetProfile.new()
	profile_no.habitability_score = 5
	profile_no.has_breathable_atmosphere = false
	
	var profile_yes: PlanetProfile = PlanetProfile.new()
	profile_yes.habitability_score = 5
	profile_yes.has_breathable_atmosphere = true
	
	var prob_no: float = PopulationProbability.calculate_native_probability(profile_no)
	var prob_yes: float = PopulationProbability.calculate_native_probability(profile_yes)
	
	assert_true(prob_yes > prob_no, "Breathable atmosphere should increase probability")


## Tests that tidal locking reduces probability.
func test_tidal_locking_penalty() -> void:
	var profile_free: PlanetProfile = PlanetProfile.new()
	profile_free.habitability_score = 5
	profile_free.is_tidally_locked = false
	
	var profile_locked: PlanetProfile = PlanetProfile.new()
	profile_locked.habitability_score = 5
	profile_locked.is_tidally_locked = true
	
	var prob_free: float = PopulationProbability.calculate_native_probability(profile_free)
	var prob_locked: float = PopulationProbability.calculate_native_probability(profile_locked)
	
	assert_true(prob_free > prob_locked, "Tidal locking should reduce probability")


## Tests that probability is clamped to [0, 0.95].
func test_probability_clamped() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 10
	profile.has_liquid_water = true
	profile.has_breathable_atmosphere = true
	
	var probability: float = PopulationProbability.calculate_native_probability(profile)
	assert_true(probability <= 0.95, "Probability should not exceed 0.95")
	assert_true(probability >= 0.0, "Probability should not be negative")


## Tests deterministic rolling with same seed gives same result.
func test_should_generate_natives_determinism() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 7
	profile.has_liquid_water = true
	
	var rng1: SeededRng = SeededRng.new(12345)
	var rng2: SeededRng = SeededRng.new(12345)
	
	var result1: bool = PopulationProbability.should_generate_natives(profile, rng1)
	var result2: bool = PopulationProbability.should_generate_natives(profile, rng2)
	
	assert_equal(result1, result2, "Same seed should produce same result")


## Tests colony probability is zero for unsuitable planets.
func test_colony_probability_zero_for_unsuitable() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 0
	
	var suitability: ColonySuitability = ColonySuitability.new()
	suitability.overall_score = 5
	
	var probability: float = PopulationProbability.calculate_colony_probability(profile, suitability)
	assert_float_equal(probability, 0.0, 0.001, "Uninhabitable should give 0 colony probability")


## Tests colony probability increases with suitability score.
func test_colony_probability_scales_with_suitability() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 5
	
	var suit_low: ColonySuitability = ColonySuitability.new()
	suit_low.overall_score = 20
	
	var suit_high: ColonySuitability = ColonySuitability.new()
	suit_high.overall_score = 80
	
	var prob_low: float = PopulationProbability.calculate_colony_probability(profile, suit_low)
	var prob_high: float = PopulationProbability.calculate_colony_probability(profile, suit_high)
	
	assert_true(prob_high > prob_low, "Higher suitability should increase colony probability")
