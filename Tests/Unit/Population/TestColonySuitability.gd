## Tests for ColonySuitability data model.
extends TestCase

const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")


## Creates a test suitability with sample data.
func _create_test_suitability() -> ColonySuitability:
	var suitability: ColonySuitability = ColonySuitability.new()
	suitability.body_id = "test_planet_001"
	suitability.overall_score = 72
	suitability.carrying_capacity = 5000000000
	suitability.base_growth_rate = 0.025
	suitability.infrastructure_difficulty = 1.2

	suitability.factor_scores = {
		ColonySuitability.FactorType.TEMPERATURE as int: 85,
		ColonySuitability.FactorType.PRESSURE as int: 90,
		ColonySuitability.FactorType.GRAVITY as int: 95,
		ColonySuitability.FactorType.ATMOSPHERE as int: 100,
		ColonySuitability.FactorType.WATER as int: 88,
		ColonySuitability.FactorType.RADIATION as int: 75,
		ColonySuitability.FactorType.RESOURCES as int: 60,
		ColonySuitability.FactorType.TERRAIN as int: 55,
		ColonySuitability.FactorType.WEATHER as int: 45,
		ColonySuitability.FactorType.DAY_LENGTH as int: 92,
	}

	suitability.limiting_factors = [ColonySuitability.FactorType.WEATHER]
	suitability.advantages = [
		ColonySuitability.FactorType.ATMOSPHERE,
		ColonySuitability.FactorType.GRAVITY,
		ColonySuitability.FactorType.DAY_LENGTH,
	]

	return suitability


## Tests basic suitability creation.
func test_creation() -> void:
	var suitability: ColonySuitability = ColonySuitability.new()
	assert_equal(suitability.body_id, "")
	assert_equal(suitability.overall_score, 0)
	assert_equal(suitability.carrying_capacity, 0)
	assert_float_equal(suitability.base_growth_rate, 0.0, 0.001)
	assert_float_equal(suitability.infrastructure_difficulty, 1.0, 0.001)


## Tests category derivation for all score ranges.
func test_get_category_ranges() -> void:
	var suitability: ColonySuitability = ColonySuitability.new()

	suitability.overall_score = 0
	assert_equal(suitability.get_category(), ColonySuitability.Category.UNSUITABLE)

	suitability.overall_score = 9
	assert_equal(suitability.get_category(), ColonySuitability.Category.UNSUITABLE)

	suitability.overall_score = 10
	assert_equal(suitability.get_category(), ColonySuitability.Category.EXTREME)

	suitability.overall_score = 29
	assert_equal(suitability.get_category(), ColonySuitability.Category.EXTREME)

	suitability.overall_score = 30
	assert_equal(suitability.get_category(), ColonySuitability.Category.DIFFICULT)

	suitability.overall_score = 49
	assert_equal(suitability.get_category(), ColonySuitability.Category.DIFFICULT)

	suitability.overall_score = 50
	assert_equal(suitability.get_category(), ColonySuitability.Category.CHALLENGING)

	suitability.overall_score = 69
	assert_equal(suitability.get_category(), ColonySuitability.Category.CHALLENGING)

	suitability.overall_score = 70
	assert_equal(suitability.get_category(), ColonySuitability.Category.FAVORABLE)

	suitability.overall_score = 89
	assert_equal(suitability.get_category(), ColonySuitability.Category.FAVORABLE)

	suitability.overall_score = 90
	assert_equal(suitability.get_category(), ColonySuitability.Category.OPTIMAL)

	suitability.overall_score = 100
	assert_equal(suitability.get_category(), ColonySuitability.Category.OPTIMAL)


## Tests category string conversion.
func test_get_category_string() -> void:
	var suitability: ColonySuitability = ColonySuitability.new()
	suitability.overall_score = 72
	assert_equal(suitability.get_category_string(), "Favorable")


## Tests category to string static method.
func test_category_to_string() -> void:
	assert_equal(ColonySuitability.category_to_string(ColonySuitability.Category.UNSUITABLE), "Unsuitable")
	assert_equal(ColonySuitability.category_to_string(ColonySuitability.Category.EXTREME), "Extreme")
	assert_equal(ColonySuitability.category_to_string(ColonySuitability.Category.DIFFICULT), "Difficult")
	assert_equal(ColonySuitability.category_to_string(ColonySuitability.Category.CHALLENGING), "Challenging")
	assert_equal(ColonySuitability.category_to_string(ColonySuitability.Category.FAVORABLE), "Favorable")
	assert_equal(ColonySuitability.category_to_string(ColonySuitability.Category.OPTIMAL), "Optimal")


## Tests category from string static method.
func test_category_from_string() -> void:
	assert_equal(ColonySuitability.category_from_string("unsuitable"), ColonySuitability.Category.UNSUITABLE)
	assert_equal(ColonySuitability.category_from_string("Extreme"), ColonySuitability.Category.EXTREME)
	assert_equal(ColonySuitability.category_from_string("DIFFICULT"), ColonySuitability.Category.DIFFICULT)
	assert_equal(ColonySuitability.category_from_string("invalid"), ColonySuitability.Category.UNSUITABLE)


## Tests factor to string static method.
func test_factor_to_string() -> void:
	assert_equal(ColonySuitability.factor_to_string(ColonySuitability.FactorType.TEMPERATURE), "Temperature")
	assert_equal(ColonySuitability.factor_to_string(ColonySuitability.FactorType.WATER), "Water")
	assert_equal(ColonySuitability.factor_to_string(ColonySuitability.FactorType.DAY_LENGTH), "Day Length")


## Tests factor from string static method.
func test_factor_from_string() -> void:
	assert_equal(ColonySuitability.factor_from_string("temperature"), ColonySuitability.FactorType.TEMPERATURE)
	assert_equal(ColonySuitability.factor_from_string("Day Length"), ColonySuitability.FactorType.DAY_LENGTH)
	assert_equal(ColonySuitability.factor_from_string("day_length"), ColonySuitability.FactorType.DAY_LENGTH)


## Tests get_factor_score method.
func test_get_factor_score() -> void:
	var suitability: ColonySuitability = _create_test_suitability()
	assert_equal(suitability.get_factor_score(ColonySuitability.FactorType.TEMPERATURE), 85)
	assert_equal(suitability.get_factor_score(ColonySuitability.FactorType.WEATHER), 45)


## Tests get_factor_score for missing factor.
func test_get_factor_score_missing() -> void:
	var suitability: ColonySuitability = ColonySuitability.new()
	assert_equal(suitability.get_factor_score(ColonySuitability.FactorType.TEMPERATURE), 0)


## Tests is_limiting_factor method.
func test_is_limiting_factor() -> void:
	var suitability: ColonySuitability = _create_test_suitability()
	assert_true(suitability.is_limiting_factor(ColonySuitability.FactorType.WEATHER)) # 45
	assert_false(suitability.is_limiting_factor(ColonySuitability.FactorType.TEMPERATURE)) # 85


## Tests is_advantage method.
func test_is_advantage() -> void:
	var suitability: ColonySuitability = _create_test_suitability()
	assert_true(suitability.is_advantage(ColonySuitability.FactorType.ATMOSPHERE)) # 100
	assert_false(suitability.is_advantage(ColonySuitability.FactorType.WEATHER)) # 45


## Tests get_worst_factor method.
func test_get_worst_factor() -> void:
	var suitability: ColonySuitability = _create_test_suitability()
	assert_equal(suitability.get_worst_factor(), ColonySuitability.FactorType.WEATHER)


## Tests get_best_factor method.
func test_get_best_factor() -> void:
	var suitability: ColonySuitability = _create_test_suitability()
	assert_equal(suitability.get_best_factor(), ColonySuitability.FactorType.ATMOSPHERE)


## Tests is_colonizable method.
func test_is_colonizable() -> void:
	var suitability: ColonySuitability = ColonySuitability.new()

	suitability.overall_score = 0
	assert_false(suitability.is_colonizable())

	suitability.overall_score = 9
	assert_false(suitability.is_colonizable())

	suitability.overall_score = 10
	assert_true(suitability.is_colonizable())

	suitability.overall_score = 100
	assert_true(suitability.is_colonizable())


## Tests category description helper.
func test_get_category_description() -> void:
	var desc: String = ColonySuitability.get_category_description(ColonySuitability.Category.FAVORABLE)
	assert_true(desc.length() > 0)
	assert_true(desc.contains("Good") or desc.contains("moderate"))


## Tests factor_count static method.
func test_factor_count() -> void:
	assert_equal(ColonySuitability.factor_count(), 10)


## Tests category_count static method.
func test_category_count() -> void:
	assert_equal(ColonySuitability.category_count(), 6)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: ColonySuitability = _create_test_suitability()

	var data: Dictionary = original.to_dict()
	var restored: ColonySuitability = ColonySuitability.from_dict(data)

	assert_equal(restored.body_id, original.body_id)
	assert_equal(restored.overall_score, original.overall_score)
	assert_equal(restored.carrying_capacity, original.carrying_capacity)
	assert_float_equal(restored.base_growth_rate, original.base_growth_rate, 0.0001)
	assert_float_equal(restored.infrastructure_difficulty, original.infrastructure_difficulty, 0.0001)


## Tests factor_scores serialization.
func test_factor_scores_serialization() -> void:
	var original: ColonySuitability = _create_test_suitability()

	var data: Dictionary = original.to_dict()
	var restored: ColonySuitability = ColonySuitability.from_dict(data)

	assert_equal(restored.factor_scores.size(), original.factor_scores.size())
	for factor_key in original.factor_scores.keys():
		assert_true(restored.factor_scores.has(factor_key))
		assert_equal(restored.factor_scores[factor_key], original.factor_scores[factor_key])


## Tests limiting_factors serialization.
func test_limiting_factors_serialization() -> void:
	var original: ColonySuitability = _create_test_suitability()

	var data: Dictionary = original.to_dict()
	var restored: ColonySuitability = ColonySuitability.from_dict(data)

	assert_equal(restored.limiting_factors.size(), original.limiting_factors.size())
	for i in range(original.limiting_factors.size()):
		assert_equal(restored.limiting_factors[i], original.limiting_factors[i])


## Tests advantages serialization.
func test_advantages_serialization() -> void:
	var original: ColonySuitability = _create_test_suitability()

	var data: Dictionary = original.to_dict()
	var restored: ColonySuitability = ColonySuitability.from_dict(data)

	assert_equal(restored.advantages.size(), original.advantages.size())
	for i in range(original.advantages.size()):
		assert_equal(restored.advantages[i], original.advantages[i])


## Tests from_dict handles JSON-style string keys.
func test_from_dict_json_string_keys() -> void:
	var original: ColonySuitability = _create_test_suitability()
	var data: Dictionary = original.to_dict()

	# Simulate JSON: keys become strings
	var json_like_factors: Dictionary = {}
	for key in data["factor_scores"].keys():
		json_like_factors[str(key)] = data["factor_scores"][key]
	data["factor_scores"] = json_like_factors

	var restored: ColonySuitability = ColonySuitability.from_dict(data)

	assert_equal(restored.factor_scores.size(), original.factor_scores.size())
	for factor_key in original.factor_scores.keys():
		assert_true(restored.factor_scores.has(factor_key))


## Tests empty suitability serialization.
func test_empty_suitability_serialization() -> void:
	var original: ColonySuitability = ColonySuitability.new()

	var data: Dictionary = original.to_dict()
	var restored: ColonySuitability = ColonySuitability.from_dict(data)

	assert_equal(restored.body_id, "")
	assert_equal(restored.overall_score, 0)
	assert_equal(restored.factor_scores.size(), 0)
	assert_equal(restored.limiting_factors.size(), 0)
	assert_equal(restored.advantages.size(), 0)
