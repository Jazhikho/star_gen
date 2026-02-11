## Calculates population probability for celestial bodies.
## Determines whether native populations and colonies should be generated
## based on the planet profile's habitability and suitability scores.
## All rolls are deterministic via injected RNG.
class_name PopulationProbability
extends RefCounted

const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _suitability_calculator: GDScript = preload("res://src/domain/population/SuitabilityCalculator.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Minimum habitability score for native life to be possible.
const MIN_HABITABILITY_FOR_NATIVES: int = 3

## Minimum habitability score for any colonization attempt.
const MIN_HABITABILITY_FOR_COLONY: int = 1

## Base probability scaling factor for native life emergence.
## Score of 10 (max) gives roughly 60% chance; score of 3 gives roughly 6%.
const NATIVE_PROBABILITY_SCALE: float = 0.06

## Bonus probability for liquid water presence.
const LIQUID_WATER_BONUS: float = 0.15

## Bonus probability for breathable atmosphere.
const BREATHABLE_ATMOSPHERE_BONUS: float = 0.10

## Penalty for tidal locking (reduces habitability diversity).
const TIDAL_LOCKING_PENALTY: float = 0.10

## Bonus for being a moon with tidal heating (subsurface oceans).
const TIDAL_HEATING_BONUS: float = 0.05

## Minimum age in years for native life to evolve.
const MIN_AGE_FOR_NATIVES_YEARS: float = 1.0e9

## Colony attempt base probability given sufficient suitability.
const COLONY_BASE_PROBABILITY: float = 0.30

## Suitability score threshold for colony attempts.
const MIN_SUITABILITY_FOR_COLONY: int = 10


## Calculates the probability of native life having emerged on a body.
## @param profile: The planet profile with habitability data.
## @return: Probability between 0.0 and 1.0.
static func calculate_native_probability(profile: PlanetProfile) -> float:
	if profile.habitability_score < MIN_HABITABILITY_FOR_NATIVES:
		return 0.0

	# Base probability from habitability score
	var probability: float = float(profile.habitability_score) * NATIVE_PROBABILITY_SCALE

	# Bonuses for favorable conditions
	if profile.has_liquid_water:
		probability += LIQUID_WATER_BONUS

	if profile.has_breathable_atmosphere:
		probability += BREATHABLE_ATMOSPHERE_BONUS

	# Tidal heating on moons can enable subsurface life
	if profile.is_moon and profile.tidal_heating_factor > 0.3:
		probability += TIDAL_HEATING_BONUS

	# Penalties
	if profile.is_tidally_locked:
		probability -= TIDAL_LOCKING_PENALTY

	# High radiation reduces life probability
	if profile.radiation_level > 0.7:
		probability -= 0.15

	return clampf(probability, 0.0, 0.95)


## Determines whether native populations should be generated for a body.
## Uses the profile's habitability to calculate probability, then rolls.
## @param profile: The planet profile.
## @param rng: Seeded RNG for deterministic roll.
## @return: True if natives should be generated.
static func should_generate_natives(profile: PlanetProfile, rng: SeededRng) -> bool:
	var probability: float = calculate_native_probability(profile)
	if probability <= 0.0:
		return false
	return rng.randf() < probability


## Calculates the probability of colonization being attempted on a body.
## @param profile: The planet profile.
## @param suitability: The colony suitability assessment.
## @return: Probability between 0.0 and 1.0.
static func calculate_colony_probability(
	profile: PlanetProfile,
	suitability: ColonySuitability
) -> float:
	if profile.habitability_score < MIN_HABITABILITY_FOR_COLONY:
		return 0.0

	if suitability.overall_score < MIN_SUITABILITY_FOR_COLONY:
		return 0.0

	# Scale base probability by suitability (higher = more attractive)
	var score_factor: float = float(suitability.overall_score) / 50.0
	var probability: float = COLONY_BASE_PROBABILITY * score_factor

	return clampf(probability, 0.0, 0.90)


## Determines whether colony generation should be attempted for a body.
## @param profile: The planet profile.
## @param suitability: The colony suitability assessment.
## @param rng: Seeded RNG for deterministic roll.
## @return: True if colony generation should be attempted.
static func should_generate_colony(
	profile: PlanetProfile,
	suitability: ColonySuitability,
	rng: SeededRng
) -> bool:
	var probability: float = calculate_colony_probability(profile, suitability)
	if probability <= 0.0:
		return false
	return rng.randf() < probability
