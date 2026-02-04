## Pure calculation functions for colony suitability assessment.
## All functions are static and deterministic: same inputs → same outputs.
## No file I/O, no Nodes, no global state.
class_name SuitabilityCalculator
extends RefCounted

# Preload dependencies for headless/editor compilation.
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Ideal temperature range for colonization (Kelvin).
const IDEAL_TEMP_MIN_K: float = 273.0 # 0°C
const IDEAL_TEMP_MAX_K: float = 303.0 # 30°C

## Survivable temperature range (with infrastructure).
const SURVIVABLE_TEMP_MIN_K: float = 200.0 # -73°C
const SURVIVABLE_TEMP_MAX_K: float = 350.0 # 77°C

## Ideal pressure range (Earth atmospheres).
const IDEAL_PRESSURE_MIN_ATM: float = 0.5
const IDEAL_PRESSURE_MAX_ATM: float = 2.0

## Survivable pressure range (with suits/domes).
const SURVIVABLE_PRESSURE_MIN_ATM: float = 0.001
const SURVIVABLE_PRESSURE_MAX_ATM: float = 10.0

## Ideal gravity range (Earth g).
const IDEAL_GRAVITY_MIN_G: float = 0.7
const IDEAL_GRAVITY_MAX_G: float = 1.3

## Survivable gravity range.
const SURVIVABLE_GRAVITY_MIN_G: float = 0.1
const SURVIVABLE_GRAVITY_MAX_G: float = 3.0

## Ideal day length range (hours).
const IDEAL_DAY_MIN_HOURS: float = 18.0
const IDEAL_DAY_MAX_HOURS: float = 30.0

## Survivable day length range.
const SURVIVABLE_DAY_MIN_HOURS: float = 1.0
const SURVIVABLE_DAY_MAX_HOURS: float = 720.0 # 30 Earth days

## Base carrying capacity for Earth-like world (per km² of habitable surface).
const BASE_DENSITY_PER_KM2: float = 50.0

## Earth's surface area in km².
const EARTH_SURFACE_KM2: float = 510.1e6

## Maximum realistic colony growth rate (3% per year).
const MAX_GROWTH_RATE: float = 0.03

## Minimum viable growth rate for hostile environments.
const MIN_GROWTH_RATE: float = 0.001


## Calculates complete suitability assessment from a PlanetProfile.
## @param profile: The planet profile to assess.
## @return: A fully populated ColonySuitability.
static func calculate(profile: PlanetProfile) -> ColonySuitability:
	var suitability: ColonySuitability = ColonySuitability.new()
	suitability.body_id = profile.body_id

	# Calculate individual factor scores
	suitability.factor_scores[ColonySuitability.FactorType.TEMPERATURE as int] = _calculate_temperature_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.PRESSURE as int] = _calculate_pressure_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.GRAVITY as int] = _calculate_gravity_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.ATMOSPHERE as int] = _calculate_atmosphere_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.WATER as int] = _calculate_water_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.RADIATION as int] = _calculate_radiation_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.RESOURCES as int] = _calculate_resources_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.TERRAIN as int] = _calculate_terrain_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.WEATHER as int] = _calculate_weather_score(profile)
	suitability.factor_scores[ColonySuitability.FactorType.DAY_LENGTH as int] = _calculate_day_length_score(profile)

	# Calculate overall score (weighted average with minimum factor penalty)
	suitability.overall_score = _calculate_overall_score(suitability.factor_scores)

	# Identify limiting factors and advantages
	suitability.limiting_factors = _identify_limiting_factors(suitability.factor_scores)
	suitability.advantages = _identify_advantages(suitability.factor_scores)

	# Calculate carrying capacity and growth rate
	suitability.carrying_capacity = calculate_carrying_capacity(profile, suitability.overall_score)
	suitability.base_growth_rate = calculate_growth_rate(profile, suitability.overall_score)

	# Calculate infrastructure difficulty
	suitability.infrastructure_difficulty = _calculate_infrastructure_difficulty(profile, suitability.factor_scores)

	return suitability


## Calculates temperature factor score (0-100).
## @param profile: The planet profile.
## @return: Temperature suitability score.
static func _calculate_temperature_score(profile: PlanetProfile) -> int:
	var temp_k: float = profile.avg_temperature_k

	# Outside survivable range = 0
	if temp_k < SURVIVABLE_TEMP_MIN_K or temp_k > SURVIVABLE_TEMP_MAX_K:
		return 0

	# Within ideal range = 100
	if temp_k >= IDEAL_TEMP_MIN_K and temp_k <= IDEAL_TEMP_MAX_K:
		return 100

	# Interpolate between survivable and ideal
	if temp_k < IDEAL_TEMP_MIN_K:
		var ratio: float = (temp_k - SURVIVABLE_TEMP_MIN_K) / (IDEAL_TEMP_MIN_K - SURVIVABLE_TEMP_MIN_K)
		return clampi(roundi(ratio * 100.0), 0, 99)
	else:
		var ratio: float = (SURVIVABLE_TEMP_MAX_K - temp_k) / (SURVIVABLE_TEMP_MAX_K - IDEAL_TEMP_MAX_K)
		return clampi(roundi(ratio * 100.0), 0, 99)


## Calculates pressure factor score (0-100).
## @param profile: The planet profile.
## @return: Pressure suitability score.
static func _calculate_pressure_score(profile: PlanetProfile) -> int:
	var pressure: float = profile.pressure_atm

	# No atmosphere can still be colonized (domes), but scores low
	if pressure < SURVIVABLE_PRESSURE_MIN_ATM:
		if profile.has_atmosphere == false:
			return 10
		return 0

	# Too high pressure
	if pressure > SURVIVABLE_PRESSURE_MAX_ATM:
		return 0

	# Within ideal range
	if pressure >= IDEAL_PRESSURE_MIN_ATM and pressure <= IDEAL_PRESSURE_MAX_ATM:
		return 100

	# Interpolate
	if pressure < IDEAL_PRESSURE_MIN_ATM:
		# Low pressure: 0.001 -> 0.5 maps to 10 -> 100
		var ratio: float = (pressure - SURVIVABLE_PRESSURE_MIN_ATM) / (IDEAL_PRESSURE_MIN_ATM - SURVIVABLE_PRESSURE_MIN_ATM)
		return clampi(roundi(10.0 + ratio * 90.0), 10, 99)
	else:
		# High pressure: 2.0 -> 10.0 maps to 100 -> 20
		var ratio: float = (pressure - IDEAL_PRESSURE_MAX_ATM) / (SURVIVABLE_PRESSURE_MAX_ATM - IDEAL_PRESSURE_MAX_ATM)
		return clampi(roundi(100.0 - ratio * 80.0), 20, 99)


## Calculates gravity factor score (0-100).
## @param profile: The planet profile.
## @return: Gravity suitability score.
static func _calculate_gravity_score(profile: PlanetProfile) -> int:
	var gravity: float = profile.gravity_g

	# Outside survivable range
	if gravity < SURVIVABLE_GRAVITY_MIN_G or gravity > SURVIVABLE_GRAVITY_MAX_G:
		return 0

	# Within ideal range
	if gravity >= IDEAL_GRAVITY_MIN_G and gravity <= IDEAL_GRAVITY_MAX_G:
		return 100

	# Interpolate
	if gravity < IDEAL_GRAVITY_MIN_G:
		var ratio: float = (gravity - SURVIVABLE_GRAVITY_MIN_G) / (IDEAL_GRAVITY_MIN_G - SURVIVABLE_GRAVITY_MIN_G)
		return clampi(roundi(ratio * 100.0), 0, 99)
	else:
		var ratio: float = (SURVIVABLE_GRAVITY_MAX_G - gravity) / (SURVIVABLE_GRAVITY_MAX_G - IDEAL_GRAVITY_MAX_G)
		return clampi(roundi(ratio * 100.0), 0, 99)


## Calculates atmosphere factor score (0-100).
## @param profile: The planet profile.
## @return: Atmosphere suitability score.
static func _calculate_atmosphere_score(profile: PlanetProfile) -> int:
	# Breathable atmosphere is ideal
	if profile.has_breathable_atmosphere:
		return 100

	# No atmosphere is workable but challenging
	if not profile.has_atmosphere:
		return 30

	# Non-breathable atmosphere: depends on toxicity (use radiation as proxy for now)
	# Lower radiation implies less hostile atmospheric conditions
	var hostility: float = profile.radiation_level
	return clampi(roundi(60.0 - hostility * 40.0), 20, 60)


## Calculates water factor score (0-100).
## @param profile: The planet profile.
## @return: Water availability score.
static func _calculate_water_score(profile: PlanetProfile) -> int:
	# Check water resource
	var water_key: int = ResourceType.Type.WATER as int
	var water_abundance: float = 0.0
	if profile.resources.has(water_key):
		water_abundance = profile.resources[water_key] as float

	# Liquid water on surface is best
	if profile.has_liquid_water:
		var base_score: float = 70.0 + profile.ocean_coverage * 30.0
		return clampi(roundi(base_score), 70, 100)

	# Ice or extractable water
	if profile.ice_coverage > 0.0:
		return clampi(roundi(40.0 + profile.ice_coverage * 40.0), 40, 80)

	# Water from resources (ice mining, etc.)
	if water_abundance > 0.0:
		return clampi(roundi(water_abundance * 60.0), 10, 60)

	# No water = very difficult
	return 5


## Calculates radiation factor score (0-100).
## @param profile: The planet profile.
## @return: Radiation protection score.
static func _calculate_radiation_score(profile: PlanetProfile) -> int:
	# Lower radiation is better
	# radiation_level 0.0 = fully protected, 1.0 = maximum exposure
	var protection: float = 1.0 - profile.radiation_level

	# Magnetic field bonus
	if profile.has_magnetic_field:
		protection = minf(protection + 0.2, 1.0)

	# Atmosphere bonus
	if profile.has_atmosphere and profile.pressure_atm > 0.1:
		protection = minf(protection + 0.1, 1.0)

	return clampi(roundi(protection * 100.0), 0, 100)


## Calculates resources factor score (0-100).
## @param profile: The planet profile.
## @return: Resource availability score.
static func _calculate_resources_score(profile: PlanetProfile) -> int:
	var score: float = 0.0
	var resource_count: int = 0

	# Weight different resources by importance for self-sufficiency
	var weights: Dictionary = {
		ResourceType.Type.WATER as int: 2.0,
		ResourceType.Type.METALS as int: 1.5,
		ResourceType.Type.SILICATES as int: 1.0,
		ResourceType.Type.RARE_ELEMENTS as int: 1.2,
		ResourceType.Type.ORGANICS as int: 1.3,
		ResourceType.Type.HYDROCARBONS as int: 1.0,
		ResourceType.Type.VOLATILES as int: 0.8,
		ResourceType.Type.RADIOACTIVES as int: 0.7,
	}

	var total_weight: float = 0.0
	for resource_key in profile.resources.keys():
		var abundance: float = profile.resources[resource_key] as float
		var weight: float = weights.get(resource_key, 0.5) as float
		score += abundance * weight * 100.0
		total_weight += weight
		resource_count += 1

	if total_weight > 0.0:
		score = score / total_weight

	# Bonus for resource diversity
	if resource_count >= 5:
		score += 10.0
	elif resource_count >= 3:
		score += 5.0

	return clampi(roundi(score), 0, 100)


## Calculates terrain factor score (0-100).
## @param profile: The planet profile.
## @return: Terrain suitability score.
static func _calculate_terrain_score(profile: PlanetProfile) -> int:
	# Need some buildable land
	var habitable: float = profile.get_habitable_surface()

	# Water world with no land
	if habitable < 0.01:
		# Can still build floating/underwater structures
		if profile.ocean_coverage > 0.5:
			return 30
		# Ice world
		if profile.ice_coverage > 0.5:
			return 25
		# Barren with no land somehow
		return 10

	# More habitable land is better, up to a point
	var land_score: float = minf(habitable / 0.3, 1.0) * 60.0

	# Tectonic activity penalty (unstable ground)
	var tectonic_penalty: float = profile.tectonic_activity * 20.0

	# Volcanism penalty
	var volcanic_penalty: float = profile.volcanism_level * 15.0

	# Continent count bonus (more distributed land)
	var continent_bonus: float = minf(profile.continent_count / 5.0, 1.0) * 15.0

	var score: float = 40.0 + land_score - tectonic_penalty - volcanic_penalty + continent_bonus

	return clampi(roundi(score), 10, 100)


## Calculates weather factor score (0-100).
## @param profile: The planet profile.
## @return: Weather hazard score (higher = safer).
static func _calculate_weather_score(profile: PlanetProfile) -> int:
	# No atmosphere = no weather hazards (but also no benefits)
	if not profile.has_atmosphere:
		return 70 # Neutral - no storms but also harsh

	# Lower severity is better
	var safety: float = 1.0 - profile.weather_severity

	return clampi(roundi(safety * 100.0), 0, 100)


## Calculates day length factor score (0-100).
## @param profile: The planet profile.
## @return: Day length suitability score.
static func _calculate_day_length_score(profile: PlanetProfile) -> int:
	var hours: float = profile.day_length_hours

	# Tidally locked is challenging but workable
	if profile.is_tidally_locked:
		return 40

	# Outside survivable range
	if hours < SURVIVABLE_DAY_MIN_HOURS or hours > SURVIVABLE_DAY_MAX_HOURS:
		return 20

	# Within ideal range
	if hours >= IDEAL_DAY_MIN_HOURS and hours <= IDEAL_DAY_MAX_HOURS:
		return 100

	# Interpolate
	if hours < IDEAL_DAY_MIN_HOURS:
		var ratio: float = (hours - SURVIVABLE_DAY_MIN_HOURS) / (IDEAL_DAY_MIN_HOURS - SURVIVABLE_DAY_MIN_HOURS)
		return clampi(roundi(40.0 + ratio * 60.0), 40, 99)
	else:
		var ratio: float = (SURVIVABLE_DAY_MAX_HOURS - hours) / (SURVIVABLE_DAY_MAX_HOURS - IDEAL_DAY_MAX_HOURS)
		return clampi(roundi(40.0 + ratio * 60.0), 40, 99)


## Calculates overall suitability score from factor scores.
## Uses weighted average with penalty for very low factors.
## @param factor_scores: Dictionary of FactorType -> score.
## @return: Overall score (0-100).
static func _calculate_overall_score(factor_scores: Dictionary) -> int:
	if factor_scores.is_empty():
		return 0

	# Factor weights (some factors matter more than others)
	var weights: Dictionary = {
		ColonySuitability.FactorType.TEMPERATURE as int: 1.5,
		ColonySuitability.FactorType.PRESSURE as int: 1.2,
		ColonySuitability.FactorType.GRAVITY as int: 1.3,
		ColonySuitability.FactorType.ATMOSPHERE as int: 1.4,
		ColonySuitability.FactorType.WATER as int: 1.5,
		ColonySuitability.FactorType.RADIATION as int: 1.3,
		ColonySuitability.FactorType.RESOURCES as int: 1.0,
		ColonySuitability.FactorType.TERRAIN as int: 0.8,
		ColonySuitability.FactorType.WEATHER as int: 0.7,
		ColonySuitability.FactorType.DAY_LENGTH as int: 0.6,
	}

	var weighted_sum: float = 0.0
	var total_weight: float = 0.0
	var min_score: int = 100

	for factor_key in factor_scores.keys():
		var score: int = factor_scores[factor_key] as int
		var weight: float = weights.get(factor_key, 1.0) as float
		weighted_sum += score * weight
		total_weight += weight
		min_score = mini(min_score, score)

	var average: float = weighted_sum / total_weight if total_weight > 0.0 else 0.0

	# Penalty for very low factors (bottleneck effect)
	# If any factor is below 20, it drags down the overall score
	if min_score < 20:
		var penalty: float = (20.0 - min_score) * 1.5
		average = maxf(average - penalty, min_score * 0.5)

	return clampi(roundi(average), 0, 100)


## Identifies limiting factors (score < 50), sorted by severity.
## @param factor_scores: Dictionary of FactorType -> score.
## @return: Array of FactorType sorted worst to best.
static func _identify_limiting_factors(factor_scores: Dictionary) -> Array[ColonySuitability.FactorType]:
	var limiting: Array[Dictionary] = []

	for factor_key in factor_scores.keys():
		var score: int = factor_scores[factor_key] as int
		if score < 50:
			limiting.append({"factor": factor_key as ColonySuitability.FactorType, "score": score})

	# Sort by score ascending (worst first)
	limiting.sort_custom(func(a: Dictionary, b: Dictionary) -> bool:
		return (a["score"] as int) < (b["score"] as int)
	)

	var result: Array[ColonySuitability.FactorType] = []
	for item in limiting:
		result.append(item["factor"] as ColonySuitability.FactorType)
	return result


## Identifies advantages (score >= 70), sorted by strength.
## @param factor_scores: Dictionary of FactorType -> score.
## @return: Array of FactorType sorted best to worst.
static func _identify_advantages(factor_scores: Dictionary) -> Array[ColonySuitability.FactorType]:
	var advantages: Array[Dictionary] = []

	for factor_key in factor_scores.keys():
		var score: int = factor_scores[factor_key] as int
		if score >= 70:
			advantages.append({"factor": factor_key as ColonySuitability.FactorType, "score": score})

	# Sort by score descending (best first)
	advantages.sort_custom(func(a: Dictionary, b: Dictionary) -> bool:
		return (a["score"] as int) > (b["score"] as int)
	)

	var result: Array[ColonySuitability.FactorType] = []
	for item in advantages:
		result.append(item["factor"] as ColonySuitability.FactorType)
	return result


## Calculates carrying capacity based on profile and suitability.
## @param profile: The planet profile.
## @param suitability_score: Overall suitability score (0-100).
## @return: Maximum sustainable population.
static func calculate_carrying_capacity(profile: PlanetProfile, suitability_score: int) -> int:
	if suitability_score < 10:
		return 0

	# Base calculation: habitable surface area * density
	var habitable_fraction: float = profile.get_habitable_surface()

	# For water/ice worlds, some population can exist on water
	if habitable_fraction < 0.05:
		if profile.ocean_coverage > 0.3:
			habitable_fraction = profile.ocean_coverage * 0.1 # 10% of ocean is usable
		elif profile.ice_coverage > 0.3:
			habitable_fraction = profile.ice_coverage * 0.05 # 5% of ice is usable

	# Estimate surface area (Earth-like assumption, scaled by gravity proxy)
	# Higher gravity often means larger planet
	var surface_multiplier: float = maxf(profile.gravity_g, 0.5)
	var estimated_surface_km2: float = EARTH_SURFACE_KM2 * surface_multiplier

	var habitable_area_km2: float = estimated_surface_km2 * habitable_fraction

	# Base density modified by suitability
	var density_modifier: float = suitability_score / 100.0
	var effective_density: float = BASE_DENSITY_PER_KM2 * density_modifier

	# Water availability multiplier
	var water_key: int = ResourceType.Type.WATER as int
	var water_abundance: float = profile.resources.get(water_key, 0.0) as float
	var water_multiplier: float = 0.5 + water_abundance * 0.5

	# Resource diversity multiplier
	var resource_multiplier: float = 0.7 + minf(profile.resources.size() / 10.0, 0.3)

	var capacity: float = habitable_area_km2 * effective_density * water_multiplier * resource_multiplier

	# Minimum viable colony size
	if capacity < 100 and suitability_score >= 10:
		capacity = 100

	# Cap at reasonable maximum (10 billion)
	capacity = minf(capacity, 10e9)

	return roundi(capacity)


## Calculates sustainable growth rate based on conditions.
## @param profile: The planet profile.
## @param suitability_score: Overall suitability score (0-100).
## @return: Annual growth rate (0.0-0.03).
static func calculate_growth_rate(profile: PlanetProfile, suitability_score: int) -> float:
	if suitability_score < 10:
		return 0.0

	# Base growth scales with suitability
	var base_rate: float = MIN_GROWTH_RATE + (MAX_GROWTH_RATE - MIN_GROWTH_RATE) * (suitability_score / 100.0)

	# Breathable atmosphere bonus
	if profile.has_breathable_atmosphere:
		base_rate *= 1.2

	# Radiation penalty
	if profile.radiation_level > 0.5:
		base_rate *= 0.8

	# Water availability bonus
	if profile.has_liquid_water:
		base_rate *= 1.1

	return clampf(base_rate, MIN_GROWTH_RATE, MAX_GROWTH_RATE)


## Calculates infrastructure difficulty modifier.
## @param profile: The planet profile.
## @param factor_scores: Dictionary of factor scores.
## @return: Difficulty modifier (1.0 = normal, >1.0 = harder).
static func _calculate_infrastructure_difficulty(
	profile: PlanetProfile,
	factor_scores: Dictionary
) -> float:
	var difficulty: float = 1.0

	# No breathable atmosphere requires sealed habitats
	if not profile.has_breathable_atmosphere:
		difficulty *= 1.5

	# Extreme temperatures require climate control
	var temp_score: int = factor_scores.get(ColonySuitability.FactorType.TEMPERATURE as int, 50) as int
	if temp_score < 50:
		difficulty *= 1.0 + (50.0 - temp_score) / 100.0

	# High radiation requires shielding
	if profile.radiation_level > 0.3:
		difficulty *= 1.0 + profile.radiation_level * 0.5

	# Low gravity makes construction different
	if profile.gravity_g < 0.5:
		difficulty *= 1.2
	# High gravity makes everything harder
	elif profile.gravity_g > 1.5:
		difficulty *= 1.0 + (profile.gravity_g - 1.5) * 0.3

	# Tectonic activity requires reinforced structures
	if profile.tectonic_activity > 0.5:
		difficulty *= 1.1

	# Severe weather requires hardened structures
	if profile.weather_severity > 0.6:
		difficulty *= 1.1

	return clampf(difficulty, 0.5, 5.0)


## Projects population at a given year from founding using logistic growth.
## @param initial_population: Starting population.
## @param years: Number of years from founding.
## @param growth_rate: Base annual growth rate.
## @param carrying_capacity: Maximum sustainable population.
## @return: Projected population.
static func project_population(
	initial_population: int,
	years: int,
	growth_rate: float,
	carrying_capacity: int
) -> int:
	if initial_population <= 0 or carrying_capacity <= 0 or growth_rate <= 0.0:
		return initial_population

	if years <= 0:
		return initial_population

	# Logistic growth: P(t) = K / (1 + ((K - P0) / P0) * e^(-rt))
	# Where K = carrying capacity, P0 = initial, r = growth rate, t = time
	var k: float = float(carrying_capacity)
	var p0: float = float(initial_population)
	var r: float = growth_rate
	var t: float = float(years)

	# Avoid division issues
	if p0 >= k:
		return carrying_capacity

	var ratio: float = (k - p0) / p0
	var population: float = k / (1.0 + ratio * exp(-r * t))

	return clampi(roundi(population), initial_population, carrying_capacity)
