## Deterministic RNG wrapper that ensures reproducible random sequences.
## All randomness in StarGen must go through this wrapper.
class_name SeededRng
extends RefCounted


## The initial seed used to create this RNG instance.
var _initial_seed: int

## The underlying Godot RandomNumberGenerator.
var _rng: RandomNumberGenerator


## Creates a new SeededRng with the given seed.
## @param seed_value: The seed for deterministic generation.
func _init(seed_value: int) -> void:
	_initial_seed = seed_value
	_rng = RandomNumberGenerator.new()
	_rng.seed = seed_value


## Returns the initial seed this RNG was created with.
## @return: The initial seed value.
func get_initial_seed() -> int:
	return _initial_seed


## Returns the current internal state for serialization.
## @return: The current RNG state.
func get_state() -> int:
	return _rng.state


## Sets the internal state for deserialization/restoration.
## @param state: The state to restore.
func set_state(state: int) -> void:
	_rng.state = state


## Returns a random float in the range [0.0, 1.0).
## @return: A random float.
func randf() -> float:
	return _rng.randf()


## Returns a random float in the range [from, to].
## @param from: The minimum value (inclusive).
## @param to: The maximum value (inclusive).
## @return: A random float in the specified range.
func randf_range(from: float, to: float) -> float:
	return _rng.randf_range(from, to)


## Returns a random 32-bit unsigned integer.
## @return: A random integer.
func randi() -> int:
	return _rng.randi()


## Returns a random integer in the range [from, to].
## @param from: The minimum value (inclusive).
## @param to: The maximum value (inclusive).
## @return: A random integer in the specified range.
func randi_range(from: int, to: int) -> int:
	return _rng.randi_range(from, to)


## Returns a random float following a normal (Gaussian) distribution.
## @param mean: The mean of the distribution.
## @param deviation: The standard deviation.
## @return: A random float from the normal distribution.
func randfn(mean: float = 0.0, deviation: float = 1.0) -> float:
	return _rng.randfn(mean, deviation)


## Creates a new SeededRng derived from this one.
## Useful for creating sub-generators that don't affect the parent's sequence.
## @return: A new SeededRng with a seed derived from this RNG.
func fork() -> SeededRng:
	var new_seed: int = _rng.randi()
	return SeededRng.new(new_seed)
