## Estimates the likelihood that a population could ever have grown on a planet,
## then checks this against a derived deterministic seed. If the seed falls
## within the likelihood range, population is created.
##
## Deterministic: same body + base seed always yields the same result.
## No RNG advancement; the "roll" is derived purely from the population seed.
class_name PopulationLikelihood
extends RefCounted

const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _population_probability: GDScript = preload("res://src/domain/population/PopulationProbability.gd")


## Population generation mode: auto (likelihood vs seed) or override.
enum Override {
	AUTO,
	NONE,
	FORCE_NATIVES,
	FORCE_COLONY,
}


## Salt for native roll derivation (separates from colony roll).
const NATIVE_ROLL_SALT: int = 0x4E415449 # "NATI" in ASCII hex

## Salt for colony roll derivation.
const COLONY_ROLL_SALT: int = 0x434F4C4F # "COLO" in ASCII hex

## Denominator for seed-to-float conversion; produces [0, 1) from positive int.
const ROLL_DENOMINATOR: float = 2147483648.0


## Estimates the likelihood (0.0–1.0) that native life could have emerged.
## Uses the same factors as PopulationProbability for consistency.
## @param profile: The planet profile with habitability data.
## @return: Likelihood between 0.0 and 1.0.
static func estimate_native_likelihood(profile: PlanetProfile) -> float:
	return PopulationProbability.calculate_native_probability(profile)


## Estimates the likelihood (0.0–1.0) that a colony could have been established.
## @param profile: The planet profile.
## @param suitability: The colony suitability assessment.
## @return: Likelihood between 0.0 and 1.0.
static func estimate_colony_likelihood(
	profile: PlanetProfile,
	suitability: ColonySuitability
) -> float:
	return PopulationProbability.calculate_colony_probability(profile, suitability)


## Derives a deterministic value in [0, 1) from the population seed and a salt.
## Same seed + salt always yields the same value.
## @param population_seed: The deterministic population seed for this body.
## @param salt: Domain-specific salt (NATIVE_ROLL_SALT or COLONY_ROLL_SALT).
## @return: Value in [0, 1).
static func derive_roll_value(population_seed: int, salt: int) -> float:
	var mixed: int = _mix_seed(population_seed, salt)
	var positive: int = absi(mixed)
	var normalized: float = float(positive % 0x7FFFFFFF) / ROLL_DENOMINATOR
	return clampf(normalized, 0.0, 0.9999999)


## Mixes seed and salt for deterministic roll derivation.
## @param seed_val: The population seed.
## @param salt_val: Domain salt.
## @return: Mixed integer.
static func _mix_seed(seed_val: int, salt_val: int) -> int:
	const PRIME: int = 2654435761
	var h: int = (seed_val ^ salt_val) * PRIME
	h = h ^ (h >> 16)
	h = h * 2246822519
	h = h ^ (h >> 13)
	return h


## Determines whether natives should be generated using likelihood vs derived seed.
## Population is created when derived_roll < likelihood.
## @param profile: The planet profile.
## @param population_seed: The deterministic population seed for this body.
## @return: True if population should be generated.
static func should_generate_natives(profile: PlanetProfile, population_seed: int) -> bool:
	var likelihood: float = estimate_native_likelihood(profile)
	if likelihood <= 0.0:
		return false
	var roll: float = derive_roll_value(population_seed, NATIVE_ROLL_SALT)
	return roll < likelihood


## Determines whether a colony should be generated using likelihood vs derived seed.
## @param profile: The planet profile.
## @param suitability: The colony suitability assessment.
## @param population_seed: The deterministic population seed for this body.
## @return: True if colony generation should be attempted.
static func should_generate_colony(
	profile: PlanetProfile,
	suitability: ColonySuitability,
	population_seed: int
) -> bool:
	var likelihood: float = estimate_colony_likelihood(profile, suitability)
	if likelihood <= 0.0:
		return false
	var roll: float = derive_roll_value(population_seed, COLONY_ROLL_SALT)
	return roll < likelihood
