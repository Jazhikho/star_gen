## Colony suitability assessment for a planet.
## Computed from PlanetProfile, provides detailed breakdown of colonization factors.
## This is a result type - calculations are in SuitabilityCalculator.
class_name ColonySuitability
extends RefCounted


## Suitability category derived from overall_score.
enum Category {
	UNSUITABLE, ## 0-9: Cannot be colonized
	EXTREME, ## 10-29: Requires massive investment, high risk
	DIFFICULT, ## 30-49: Significant challenges, expensive
	CHALLENGING, ## 50-69: Notable challenges but viable
	FAVORABLE, ## 70-89: Good conditions, moderate investment
	OPTIMAL, ## 90-100: Excellent conditions, minimal barriers
}


## Factor types that contribute to suitability.
enum FactorType {
	TEMPERATURE, ## Surface temperature livability
	PRESSURE, ## Atmospheric pressure suitability
	GRAVITY, ## Surface gravity comfort/safety
	ATMOSPHERE, ## Breathability and composition
	WATER, ## Water availability
	RADIATION, ## Radiation protection level
	RESOURCES, ## Resource availability for self-sufficiency
	TERRAIN, ## Buildable land availability
	WEATHER, ## Weather hazard level
	DAY_LENGTH, ## Day/night cycle suitability
}


## Overall suitability score (0-100). Higher = more suitable.
var overall_score: int = 0

## Individual factor scores (0-100 each).
## Keys are FactorType enum values (as int), values are int scores.
var factor_scores: Dictionary = {}

## Estimated maximum sustainable population (carrying capacity).
var carrying_capacity: int = 0

## Base annual population growth rate (0.0-1.0, e.g., 0.02 = 2%).
var base_growth_rate: float = 0.0

## Infrastructure difficulty modifier (1.0 = normal, >1.0 = harder, <1.0 = easier).
var infrastructure_difficulty: float = 1.0

## Primary limiting factors (sorted by severity, worst first).
var limiting_factors: Array[FactorType] = []

## Primary advantages (sorted by strength, best first).
var advantages: Array[FactorType] = []

## Reference to source profile body ID.
var body_id: String = ""


## Returns the suitability category based on overall_score.
## @return: Category enum value.
func get_category() -> Category:
	if overall_score < 10:
		return Category.UNSUITABLE
	elif overall_score < 30:
		return Category.EXTREME
	elif overall_score < 50:
		return Category.DIFFICULT
	elif overall_score < 70:
		return Category.CHALLENGING
	elif overall_score < 90:
		return Category.FAVORABLE
	else:
		return Category.OPTIMAL


## Returns the category as a display string.
## @return: Human-readable category name.
func get_category_string() -> String:
	return category_to_string(get_category())


## Converts a category enum to a display string.
## @param category: The category enum value.
## @return: Human-readable string.
static func category_to_string(category: Category) -> String:
	match category:
		Category.UNSUITABLE:
			return "Unsuitable"
		Category.EXTREME:
			return "Extreme"
		Category.DIFFICULT:
			return "Difficult"
		Category.CHALLENGING:
			return "Challenging"
		Category.FAVORABLE:
			return "Favorable"
		Category.OPTIMAL:
			return "Optimal"
		_:
			return "Unknown"


## Converts a string to a category enum.
## @param name: The string name (case-insensitive).
## @return: The category, or UNSUITABLE if not found.
static func category_from_string(name: String) -> Category:
	match name.to_lower():
		"unsuitable":
			return Category.UNSUITABLE
		"extreme":
			return Category.EXTREME
		"difficult":
			return Category.DIFFICULT
		"challenging":
			return Category.CHALLENGING
		"favorable":
			return Category.FAVORABLE
		"optimal":
			return Category.OPTIMAL
		_:
			return Category.UNSUITABLE


## Converts a factor type enum to a display string.
## @param factor: The factor type enum value.
## @return: Human-readable string.
static func factor_to_string(factor: FactorType) -> String:
	match factor:
		FactorType.TEMPERATURE:
			return "Temperature"
		FactorType.PRESSURE:
			return "Pressure"
		FactorType.GRAVITY:
			return "Gravity"
		FactorType.ATMOSPHERE:
			return "Atmosphere"
		FactorType.WATER:
			return "Water"
		FactorType.RADIATION:
			return "Radiation"
		FactorType.RESOURCES:
			return "Resources"
		FactorType.TERRAIN:
			return "Terrain"
		FactorType.WEATHER:
			return "Weather"
		FactorType.DAY_LENGTH:
			return "Day Length"
		_:
			return "Unknown"


## Converts a string to a factor type enum.
## @param name: The string name (case-insensitive).
## @return: The factor type, or TEMPERATURE if not found.
static func factor_from_string(name: String) -> FactorType:
	match name.to_lower().replace(" ", "_"):
		"temperature":
			return FactorType.TEMPERATURE
		"pressure":
			return FactorType.PRESSURE
		"gravity":
			return FactorType.GRAVITY
		"atmosphere":
			return FactorType.ATMOSPHERE
		"water":
			return FactorType.WATER
		"radiation":
			return FactorType.RADIATION
		"resources":
			return FactorType.RESOURCES
		"terrain":
			return FactorType.TERRAIN
		"weather":
			return FactorType.WEATHER
		"day_length":
			return FactorType.DAY_LENGTH
		_:
			return FactorType.TEMPERATURE


## Returns a short description of what the category means for colonization.
## @param category: The category to describe.
## @return: A brief description string.
static func get_category_description(category: Category) -> String:
	match category:
		Category.UNSUITABLE:
			return "Cannot support a colony under any realistic conditions"
		Category.EXTREME:
			return "Requires massive investment and accepts high ongoing risk"
		Category.DIFFICULT:
			return "Significant challenges require substantial resources to overcome"
		Category.CHALLENGING:
			return "Notable challenges but viable with proper preparation"
		Category.FAVORABLE:
			return "Good conditions with moderate infrastructure investment"
		Category.OPTIMAL:
			return "Excellent conditions requiring minimal adaptation"
		_:
			return "Unknown suitability"


## Returns the score for a specific factor.
## @param factor: The factor type to query.
## @return: The factor score (0-100), or 0 if not present.
func get_factor_score(factor: FactorType) -> int:
	var key: int = factor as int
	if factor_scores.has(key):
		return factor_scores[key] as int
	return 0


## Returns whether a factor is a limiting factor (score < 50).
## @param factor: The factor type to check.
## @return: True if this factor is limiting.
func is_limiting_factor(factor: FactorType) -> bool:
	return get_factor_score(factor) < 50


## Returns whether a factor is an advantage (score >= 70).
## @param factor: The factor type to check.
## @return: True if this factor is an advantage.
func is_advantage(factor: FactorType) -> bool:
	return get_factor_score(factor) >= 70


## Returns the worst (lowest scoring) factor.
## @return: The factor type with lowest score, or TEMPERATURE if empty.
func get_worst_factor() -> FactorType:
	if limiting_factors.size() > 0:
		return limiting_factors[0]

	var worst: FactorType = FactorType.TEMPERATURE
	var worst_score: int = 101
	for key in factor_scores.keys():
		var score: int = factor_scores[key] as int
		if score < worst_score:
			worst_score = score
			worst = key as FactorType
	return worst


## Returns the best (highest scoring) factor.
## @return: The factor type with highest score, or TEMPERATURE if empty.
func get_best_factor() -> FactorType:
	if advantages.size() > 0:
		return advantages[0]

	var best: FactorType = FactorType.TEMPERATURE
	var best_score: int = -1
	for key in factor_scores.keys():
		var score: int = factor_scores[key] as int
		if score > best_score:
			best_score = score
			best = key as FactorType
	return best


## Returns whether colonization is possible at all.
## @return: True if overall_score >= 10.
func is_colonizable() -> bool:
	return overall_score >= 10


## Returns the number of factor types.
## @return: Count of FactorType enum values.
static func factor_count() -> int:
	return 10


## Returns the number of category types.
## @return: Count of Category enum values.
static func category_count() -> int:
	return 6


## Converts this suitability to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var factor_scores_data: Dictionary = {}
	for key in factor_scores.keys():
		factor_scores_data[key as int] = factor_scores[key] as int

	var limiting_factors_data: Array[int] = []
	for factor in limiting_factors:
		limiting_factors_data.append(factor as int)

	var advantages_data: Array[int] = []
	for factor in advantages:
		advantages_data.append(factor as int)

	return {
		"body_id": body_id,
		"overall_score": overall_score,
		"factor_scores": factor_scores_data,
		"carrying_capacity": carrying_capacity,
		"base_growth_rate": base_growth_rate,
		"infrastructure_difficulty": infrastructure_difficulty,
		"limiting_factors": limiting_factors_data,
		"advantages": advantages_data,
	}


## Creates a ColonySuitability from a dictionary.
## @param data: The dictionary to parse.
## @return: A new ColonySuitability instance.
static func from_dict(data: Dictionary) -> ColonySuitability:
	var suitability: ColonySuitability = ColonySuitability.new()

	suitability.body_id = data.get("body_id", "") as String
	suitability.overall_score = data.get("overall_score", 0) as int
	suitability.carrying_capacity = data.get("carrying_capacity", 0) as int
	suitability.base_growth_rate = data.get("base_growth_rate", 0.0) as float
	suitability.infrastructure_difficulty = data.get("infrastructure_difficulty", 1.0) as float

	# Parse factor scores (keys may be int or string from JSON)
	var factor_scores_data: Dictionary = data.get("factor_scores", {}) as Dictionary
	for key in factor_scores_data.keys():
		var factor_key: int = _key_to_int(key)
		suitability.factor_scores[factor_key] = factor_scores_data[key] as int

	# Parse limiting factors
	var limiting_factors_data: Array = data.get("limiting_factors", []) as Array
	for factor_int in limiting_factors_data:
		suitability.limiting_factors.append(_key_to_int(factor_int) as FactorType)

	# Parse advantages
	var advantages_data: Array = data.get("advantages", []) as Array
	for factor_int in advantages_data:
		suitability.advantages.append(_key_to_int(factor_int) as FactorType)

	return suitability


## Converts a dict key (int or string from JSON) to int for enum use.
## @param key: Key from serialized data (int or String).
## @return: int value for enum lookup.
static func _key_to_int(key: Variant) -> int:
	if key is int:
		return key as int
	if key is String:
		return int(key as String)
	return 0
