## Provides deterministic, order-independent seed generation for population data.
## Ensures the same body + base seed always produces the same population
## regardless of the order bodies are generated in.
class_name PopulationSeeding
extends RefCounted


## Prime multiplier for body ID hashing. Chosen to reduce collision risk.
const HASH_PRIME_A: int = 2654435761

## Secondary prime for mixing.
const HASH_PRIME_B: int = 2246822519

## Salt for population seeds to separate from other generation domains.
const POPULATION_SALT: int = 0x504F5055 # "POPU" in ASCII hex


## Generates a deterministic population seed from a body ID and base seed.
## The result is independent of generation order — only the body's identity
## and the universe seed matter.
## @param body_id: Unique identifier of the celestial body.
## @param base_seed: The system or universe base seed.
## @return: A deterministic seed for population generation.
static func generate_population_seed(body_id: String, base_seed: int) -> int:
	# Hash the body ID to a stable integer
	var id_hash: int = _hash_string(body_id)

	# Combine: XOR body hash with base seed, then mix with salt
	var combined: int = id_hash ^ base_seed
	combined = _mix(combined, POPULATION_SALT)

	# Ensure positive (GDScript int is 64-bit signed)
	return absi(combined)


## Generates a native population sub-seed (deterministic from body seed + index).
## @param population_seed: The body's population seed.
## @param native_index: Index of this native population (0-based).
## @return: A deterministic seed for native generation.
static func generate_native_seed(population_seed: int, native_index: int) -> int:
	var mixed: int = _mix(population_seed, native_index + 1)
	return absi(mixed)


## Generates a colony sub-seed (deterministic from body seed + index).
## @param population_seed: The body's population seed.
## @param colony_index: Index of this colony (0-based).
## @return: A deterministic seed for colony generation.
static func generate_colony_seed(population_seed: int, colony_index: int) -> int:
	# Use a different offset from native seeds to avoid overlap
	var mixed: int = _mix(population_seed, (colony_index + 1) * 1000003)
	return absi(mixed)


## Hashes a string to a stable integer using FNV-1a-like algorithm.
## @param s: The string to hash.
## @return: Hash value.
static func _hash_string(s: String) -> int:
	var h: int = 0x811C9DC5 # FNV offset basis (32-bit)
	for i in range(s.length()):
		h = h ^ s.unicode_at(i)
		h = h * 0x01000193 # FNV prime (32-bit)
	return h


## Mixes two integer values using a simple but effective bit mixing function.
## @param a: First value.
## @param b: Second value.
## @return: Mixed result.
static func _mix(a: int, b: int) -> int:
	var result: int = a ^ (b * HASH_PRIME_A)
	# Rotate-like mixing via shifts
	result = result ^ (result >> 16)
	result = result * HASH_PRIME_B
	result = result ^ (result >> 13)
	return result
