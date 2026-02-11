## Tests for SuitabilityCalculator pure functions.
extends TestCase

const _suitability_calculator: GDScript = preload("res://src/domain/population/SuitabilityCalculator.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")


## Creates an Earth-like profile for testing.
func _create_earth_like_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "earth_like_001"
	profile.habitability_score = 10
	profile.avg_temperature_k = 288.0 # 15°C
	profile.pressure_atm = 1.0
	profile.ocean_coverage = 0.71
	profile.land_coverage = 0.29
	profile.ice_coverage = 0.03
	profile.continent_count = 7
	profile.max_elevation_km = 8.8
	profile.day_length_hours = 24.0
	profile.axial_tilt_deg = 23.4
	profile.gravity_g = 1.0
	profile.tectonic_activity = 0.5
	profile.volcanism_level = 0.2
	profile.weather_severity = 0.3
	profile.magnetic_field_strength = 1.0
	profile.radiation_level = 0.1
	profile.albedo = 0.3
	profile.greenhouse_factor = 1.15
	profile.is_tidally_locked = false
	profile.has_atmosphere = true
	profile.has_magnetic_field = true
	profile.has_liquid_water = true
	profile.has_breathable_atmosphere = true
	profile.is_moon = false

	profile.resources = {
		ResourceType.Type.WATER as int: 0.9,
		ResourceType.Type.METALS as int: 0.5,
		ResourceType.Type.SILICATES as int: 0.8,
		ResourceType.Type.ORGANICS as int: 0.7,
		ResourceType.Type.RARE_ELEMENTS as int: 0.3,
	}

	profile.biomes = {
		BiomeType.Type.OCEAN as int: 0.71,
		BiomeType.Type.FOREST as int: 0.12,
		BiomeType.Type.GRASSLAND as int: 0.08,
		BiomeType.Type.DESERT as int: 0.05,
	}

	return profile


## Creates a Mars-like profile for testing.
func _create_mars_like_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "mars_like_001"
	profile.habitability_score = 2
	profile.avg_temperature_k = 210.0 # -63°C
	profile.pressure_atm = 0.006
	profile.ocean_coverage = 0.0
	profile.land_coverage = 0.95
	profile.ice_coverage = 0.05
	profile.continent_count = 0
	profile.max_elevation_km = 21.9
	profile.day_length_hours = 24.6
	profile.axial_tilt_deg = 25.2
	profile.gravity_g = 0.38
	profile.tectonic_activity = 0.0
	profile.volcanism_level = 0.0
	profile.weather_severity = 0.4 # Dust storms
	profile.magnetic_field_strength = 0.0
	profile.radiation_level = 0.6
	profile.albedo = 0.25
	profile.greenhouse_factor = 1.0
	profile.is_tidally_locked = false
	profile.has_atmosphere = true
	profile.has_magnetic_field = false
	profile.has_liquid_water = false
	profile.has_breathable_atmosphere = false
	profile.is_moon = false

	profile.resources = {
		ResourceType.Type.WATER as int: 0.2, # Ice
		ResourceType.Type.METALS as int: 0.4,
		ResourceType.Type.SILICATES as int: 0.9,
	}

	profile.biomes = {
		BiomeType.Type.BARREN as int: 0.95,
		BiomeType.Type.ICE_SHEET as int: 0.05,
	}

	return profile


## Creates an airless moon profile for testing.
func _create_airless_moon_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "moon_like_001"
	profile.habitability_score = 1
	profile.avg_temperature_k = 250.0 # Average, varies wildly
	profile.pressure_atm = 0.0
	profile.ocean_coverage = 0.0
	profile.land_coverage = 1.0
	profile.ice_coverage = 0.0
	profile.continent_count = 0
	profile.max_elevation_km = 10.0
	profile.day_length_hours = 708.0 # Lunar day
	profile.axial_tilt_deg = 1.5
	profile.gravity_g = 0.16
	profile.tectonic_activity = 0.0
	profile.volcanism_level = 0.0
	profile.weather_severity = 0.0
	profile.magnetic_field_strength = 0.0
	profile.radiation_level = 0.9
	profile.albedo = 0.12
	profile.greenhouse_factor = 1.0
	profile.is_tidally_locked = true
	profile.has_atmosphere = false
	profile.has_magnetic_field = false
	profile.has_liquid_water = false
	profile.has_breathable_atmosphere = false
	profile.is_moon = true

	profile.resources = {
		ResourceType.Type.SILICATES as int: 0.8,
		ResourceType.Type.METALS as int: 0.3,
	}

	profile.biomes = {
		BiomeType.Type.BARREN as int: 1.0,
	}

	return profile


## Creates a Venus-like profile for testing.
func _create_venus_like_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "venus_like_001"
	profile.habitability_score = 0
	profile.avg_temperature_k = 737.0 # 464°C
	profile.pressure_atm = 92.0
	profile.ocean_coverage = 0.0
	profile.land_coverage = 1.0
	profile.ice_coverage = 0.0
	profile.continent_count = 2
	profile.max_elevation_km = 11.0
	profile.day_length_hours = 2802.0 # Very slow retrograde
	profile.axial_tilt_deg = 177.0
	profile.gravity_g = 0.91
	profile.tectonic_activity = 0.3
	profile.volcanism_level = 0.8
	profile.weather_severity = 0.9
	profile.magnetic_field_strength = 0.0
	profile.radiation_level = 0.3
	profile.albedo = 0.77
	profile.greenhouse_factor = 2.5
	profile.is_tidally_locked = false
	profile.has_atmosphere = true
	profile.has_magnetic_field = false
	profile.has_liquid_water = false
	profile.has_breathable_atmosphere = false
	profile.is_moon = false

	profile.resources = {
		ResourceType.Type.SILICATES as int: 0.7,
		ResourceType.Type.VOLATILES as int: 0.9,
	}

	profile.biomes = {
		BiomeType.Type.VOLCANIC as int: 0.3,
		BiomeType.Type.BARREN as int: 0.7,
	}

	return profile


## Tests Earth-like planet scores high.
func test_calculate_earth_like() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_in_range(suitability.overall_score, 80, 100, "Earth-like should score 80-100")
	assert_equal(suitability.get_category(), ColonySuitability.Category.OPTIMAL)
	assert_true(suitability.is_colonizable())


## Tests Mars-like planet scores moderately.
func test_calculate_mars_like() -> void:
	var profile: PlanetProfile = _create_mars_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_in_range(suitability.overall_score, 20, 50, "Mars-like should score 20-50")
	assert_true(suitability.is_colonizable())


## Tests airless moon scores low but colonizable.
func test_calculate_airless_moon() -> void:
	var profile: PlanetProfile = _create_airless_moon_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_in_range(suitability.overall_score, 10, 35, "Airless moon should score 10-35")
	assert_true(suitability.is_colonizable())


## Tests Venus-like planet is unsuitable.
func test_calculate_venus_like() -> void:
	var profile: PlanetProfile = _create_venus_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_less_than(suitability.overall_score, 15, "Venus-like should score < 15")


## Tests body_id is preserved.
func test_body_id_preserved() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_equal(suitability.body_id, "earth_like_001")


## Tests all factor scores are populated.
func test_all_factors_populated() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_equal(suitability.factor_scores.size(), ColonySuitability.factor_count())

	for i in range(ColonySuitability.factor_count()):
		var factor: ColonySuitability.FactorType = i as ColonySuitability.FactorType
		assert_true(suitability.factor_scores.has(factor as int),
			"Missing factor: " + ColonySuitability.factor_to_string(factor))


## Tests factor scores are in valid range.
func test_factor_scores_in_range() -> void:
	var profile: PlanetProfile = _create_mars_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	for factor_key in suitability.factor_scores.keys():
		var score: int = suitability.factor_scores[factor_key] as int
		assert_in_range(score, 0, 100, "Factor score out of range")


## Tests temperature factor ideal range.
func test_temperature_factor_ideal() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.avg_temperature_k = 288.0 # 15°C - ideal

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var temp_score: int = suitability.get_factor_score(ColonySuitability.FactorType.TEMPERATURE)

	assert_equal(temp_score, 100, "Ideal temperature should score 100")


## Tests temperature factor freezing.
func test_temperature_factor_cold() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.avg_temperature_k = 220.0 # -53°C

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var temp_score: int = suitability.get_factor_score(ColonySuitability.FactorType.TEMPERATURE)

	assert_in_range(temp_score, 20, 50, "Cold temperature should score 20-50")


## Tests temperature factor too extreme.
func test_temperature_factor_extreme() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.avg_temperature_k = 150.0 # -123°C

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var temp_score: int = suitability.get_factor_score(ColonySuitability.FactorType.TEMPERATURE)

	assert_equal(temp_score, 0, "Extreme temperature should score 0")


## Tests gravity factor ideal range.
func test_gravity_factor_ideal() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.gravity_g = 1.0

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var gravity_score: int = suitability.get_factor_score(ColonySuitability.FactorType.GRAVITY)

	assert_equal(gravity_score, 100, "Earth gravity should score 100")


## Tests gravity factor too low.
func test_gravity_factor_low() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.gravity_g = 0.16 # Moon-like

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var gravity_score: int = suitability.get_factor_score(ColonySuitability.FactorType.GRAVITY)

	assert_in_range(gravity_score, 5, 30, "Low gravity should score 5-30")


## Tests gravity factor too high.
func test_gravity_factor_too_high() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.gravity_g = 4.0 # Too high

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var gravity_score: int = suitability.get_factor_score(ColonySuitability.FactorType.GRAVITY)

	assert_equal(gravity_score, 0, "Extreme gravity should score 0")


## Tests atmosphere factor breathable.
func test_atmosphere_factor_breathable() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.has_breathable_atmosphere = true
	profile.has_atmosphere = true

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var atmo_score: int = suitability.get_factor_score(ColonySuitability.FactorType.ATMOSPHERE)

	assert_equal(atmo_score, 100, "Breathable atmosphere should score 100")


## Tests atmosphere factor none.
func test_atmosphere_factor_none() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.has_breathable_atmosphere = false
	profile.has_atmosphere = false

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var atmo_score: int = suitability.get_factor_score(ColonySuitability.FactorType.ATMOSPHERE)

	assert_equal(atmo_score, 30, "No atmosphere should score 30")


## Tests water factor with ocean.
func test_water_factor_ocean() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.has_liquid_water = true
	profile.ocean_coverage = 0.7

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var water_score: int = suitability.get_factor_score(ColonySuitability.FactorType.WATER)

	assert_in_range(water_score, 85, 100, "Ocean world should score 85-100 for water")


## Tests water factor with ice only.
func test_water_factor_ice() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.has_liquid_water = false
	profile.ice_coverage = 0.3

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var water_score: int = suitability.get_factor_score(ColonySuitability.FactorType.WATER)

	assert_in_range(water_score, 40, 70, "Ice world should score 40-70 for water")


## Tests water factor with no water.
func test_water_factor_none() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.has_liquid_water = false
	profile.ice_coverage = 0.0
	profile.resources = {}

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var water_score: int = suitability.get_factor_score(ColonySuitability.FactorType.WATER)

	assert_equal(water_score, 5, "No water should score 5")


## Tests radiation factor protected.
func test_radiation_factor_protected() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.radiation_level = 0.1
	profile.has_magnetic_field = true
	profile.has_atmosphere = true
	profile.pressure_atm = 1.0

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var rad_score: int = suitability.get_factor_score(ColonySuitability.FactorType.RADIATION)

	assert_in_range(rad_score, 80, 100, "Protected world should score 80-100 for radiation")


## Tests radiation factor exposed.
func test_radiation_factor_exposed() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.radiation_level = 0.9
	profile.has_magnetic_field = false
	profile.has_atmosphere = false

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var rad_score: int = suitability.get_factor_score(ColonySuitability.FactorType.RADIATION)

	assert_in_range(rad_score, 5, 25, "Exposed world should score 5-25 for radiation")


## Tests day length factor ideal.
func test_day_length_factor_ideal() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.day_length_hours = 24.0
	profile.is_tidally_locked = false

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var day_score: int = suitability.get_factor_score(ColonySuitability.FactorType.DAY_LENGTH)

	assert_equal(day_score, 100, "24-hour day should score 100")


## Tests day length factor tidally locked.
func test_day_length_factor_tidally_locked() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.is_tidally_locked = true

	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var day_score: int = suitability.get_factor_score(ColonySuitability.FactorType.DAY_LENGTH)

	assert_equal(day_score, 40, "Tidally locked should score 40")


## Tests limiting factors are identified.
func test_limiting_factors_identified() -> void:
	var profile: PlanetProfile = _create_mars_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_greater_than(suitability.limiting_factors.size(), 0, "Should have limiting factors")

	# Verify limiting factors actually have low scores
	for factor in suitability.limiting_factors:
		var score: int = suitability.get_factor_score(factor)
		assert_less_than(score, 50, "Limiting factor should have score < 50")


## Tests advantages are identified.
func test_advantages_identified() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_greater_than(suitability.advantages.size(), 0, "Should have advantages")

	# Verify advantages actually have high scores
	for factor in suitability.advantages:
		var score: int = suitability.get_factor_score(factor)
		assert_greater_than(score, 69, "Advantage should have score >= 70")


## Tests limiting factors are sorted by severity.
func test_limiting_factors_sorted() -> void:
	var profile: PlanetProfile = _create_mars_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	if suitability.limiting_factors.size() >= 2:
		var prev_score: int = suitability.get_factor_score(suitability.limiting_factors[0])
		for i in range(1, suitability.limiting_factors.size()):
			var curr_score: int = suitability.get_factor_score(suitability.limiting_factors[i])
			assert_true(curr_score >= prev_score, "Limiting factors should be sorted worst first")
			prev_score = curr_score


## Tests advantages are sorted by strength.
func test_advantages_sorted() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	if suitability.advantages.size() >= 2:
		var prev_score: int = suitability.get_factor_score(suitability.advantages[0])
		for i in range(1, suitability.advantages.size()):
			var curr_score: int = suitability.get_factor_score(suitability.advantages[i])
			assert_true(curr_score <= prev_score, "Advantages should be sorted best first")
			prev_score = curr_score


## Tests carrying capacity for Earth-like.
func test_carrying_capacity_earth_like() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	# Earth-like should have billions capacity
	assert_greater_than(suitability.carrying_capacity, 1000000000, "Earth-like should support > 1 billion")


## Tests carrying capacity for hostile world.
func test_carrying_capacity_hostile() -> void:
	var profile: PlanetProfile = _create_airless_moon_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	# Hostile world should have limited capacity but > 0 if colonizable (less than Earth-like billions)
	if suitability.is_colonizable():
		assert_greater_than(suitability.carrying_capacity, 0, "Colonizable world should have capacity > 0")
		assert_less_than(suitability.carrying_capacity, 1000000000, "Hostile world should have less capacity than Earth-like")


## Tests carrying capacity zero for unsuitable.
func test_carrying_capacity_unsuitable() -> void:
	var profile: PlanetProfile = _create_venus_like_profile()
	profile.avg_temperature_k = 1000.0 # Even more extreme
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	if not suitability.is_colonizable():
		assert_equal(suitability.carrying_capacity, 0, "Unsuitable world should have 0 capacity")


## Tests growth rate for Earth-like.
func test_growth_rate_earth_like() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_greater_than(suitability.base_growth_rate, 0.02, "Earth-like should have > 2% growth")
	assert_less_than(suitability.base_growth_rate, 0.04, "Growth rate should be < 4%")


## Tests growth rate for hostile world.
func test_growth_rate_hostile() -> void:
	var profile: PlanetProfile = _create_mars_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_greater_than(suitability.base_growth_rate, 0.0, "Colonizable world should have > 0 growth")
	assert_less_than(suitability.base_growth_rate, 0.02, "Hostile world should have < 2% growth")


## Tests infrastructure difficulty for Earth-like.
func test_infrastructure_difficulty_earth_like() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_float_equal(suitability.infrastructure_difficulty, 1.0, 0.3,
		"Earth-like should have ~1.0 infrastructure difficulty")


## Tests infrastructure difficulty for hostile world.
func test_infrastructure_difficulty_hostile() -> void:
	var profile: PlanetProfile = _create_airless_moon_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_greater_than(suitability.infrastructure_difficulty, 1.5,
		"Hostile world should have > 1.5 infrastructure difficulty")


## Tests population projection logistic growth.
func test_project_population_growth() -> void:
	var pop: int = SuitabilityCalculator.project_population(1000, 50, 0.03, 1000000)

	assert_greater_than(pop, 1000, "Population should grow over time")
	assert_less_than(pop, 1000000, "Population should not exceed capacity")


## Tests population projection approaches capacity.
func test_project_population_approaches_capacity() -> void:
	var capacity: int = 10000
	var pop: int = SuitabilityCalculator.project_population(1000, 500, 0.05, capacity)

	# After long time with high growth, should be near capacity
	assert_greater_than(pop, capacity * 0.9, "Population should approach capacity")


## Tests population projection with zero growth.
func test_project_population_zero_growth() -> void:
	var pop: int = SuitabilityCalculator.project_population(1000, 100, 0.0, 10000)

	assert_equal(pop, 1000, "Zero growth should maintain initial population")


## Tests population projection at capacity.
func test_project_population_at_capacity() -> void:
	var pop: int = SuitabilityCalculator.project_population(10000, 50, 0.03, 10000)

	assert_equal(pop, 10000, "Population at capacity should stay at capacity")


## Tests population projection with zero years.
func test_project_population_zero_years() -> void:
	var pop: int = SuitabilityCalculator.project_population(5000, 0, 0.03, 100000)

	assert_equal(pop, 5000, "Zero years should return initial population")


## Tests determinism - same inputs give same results.
func test_determinism() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()

	var result1: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var result2: ColonySuitability = SuitabilityCalculator.calculate(profile)

	assert_equal(result1.overall_score, result2.overall_score, "Should be deterministic")
	assert_equal(result1.carrying_capacity, result2.carrying_capacity, "Should be deterministic")
	assert_float_equal(result1.base_growth_rate, result2.base_growth_rate, 0.0001, "Should be deterministic")
