## Tests for the SeededRng class.
## Verifies determinism and correct behavior of the RNG wrapper.
extends TestCase


## Tests that the same seed produces the same sequence of random numbers.
func test_determinism_same_seed_produces_same_sequence() -> void:
	var rng1: SeededRng = SeededRng.new(12345)
	var rng2: SeededRng = SeededRng.new(12345)
	
	var sequence1: Array[float] = []
	var sequence2: Array[float] = []
	
	for i in range(100):
		sequence1.append(rng1.randf())
		sequence2.append(rng2.randf())
	
	for i in range(100):
		assert_float_equal(sequence1[i], sequence2[i], 0.0, "Sequence mismatch at index %d" % i)


## Tests that different seeds produce different sequences.
func test_different_seeds_produce_different_sequences() -> void:
	var rng1: SeededRng = SeededRng.new(12345)
	var rng2: SeededRng = SeededRng.new(54321)
	
	var same_count: int = 0
	for i in range(10):
		if rng1.randf() == rng2.randf():
			same_count += 1
	
	assert_less_than(same_count, 2, "Different seeds should produce different sequences")


## Tests that randf returns values in the correct range [0.0, 1.0).
func test_randf_returns_values_in_valid_range() -> void:
	var rng: SeededRng = SeededRng.new(42)
	
	for i in range(1000):
		var value: float = rng.randf()
		assert_true(value >= 0.0, "randf should return >= 0.0")
		assert_true(value < 1.0, "randf should return < 1.0")


## Tests that randf_range returns values in the specified range.
func test_randf_range_returns_values_in_specified_range() -> void:
	var rng: SeededRng = SeededRng.new(42)
	var min_val: float = 5.0
	var max_val: float = 10.0
	
	for i in range(1000):
		var value: float = rng.randf_range(min_val, max_val)
		assert_in_range(value, min_val, max_val, "randf_range value out of range")


## Tests that randi_range returns values in the specified range.
func test_randi_range_returns_values_in_specified_range() -> void:
	var rng: SeededRng = SeededRng.new(42)
	var min_val: int = 1
	var max_val: int = 6
	
	for i in range(1000):
		var value: int = rng.randi_range(min_val, max_val)
		assert_in_range(value, min_val, max_val, "randi_range value out of range")


## Tests that get_initial_seed returns the correct seed.
func test_get_initial_seed_returns_correct_value() -> void:
	var seed_value: int = 99999
	var rng: SeededRng = SeededRng.new(seed_value)
	
	assert_equal(rng.get_initial_seed(), seed_value)
	
	# Seed should not change after generating numbers
	for i in range(100):
		rng.randf()
	
	assert_equal(rng.get_initial_seed(), seed_value)


## Tests that state can be saved and restored.
func test_state_save_and_restore() -> void:
	var rng: SeededRng = SeededRng.new(12345)
	
	# Advance the RNG
	for i in range(50):
		rng.randf()
	
	var saved_state: int = rng.get_state()
	
	var values_after_save: Array[float] = []
	for i in range(10):
		values_after_save.append(rng.randf())
	
	# Restore and verify same values
	rng.set_state(saved_state)
	
	for i in range(10):
		assert_float_equal(rng.randf(), values_after_save[i], 0.0, "State restore failed at index %d" % i)


## Tests that fork creates an independent deterministic RNG.
func test_fork_is_deterministic() -> void:
	var parent1: SeededRng = SeededRng.new(12345)
	var parent2: SeededRng = SeededRng.new(12345)
	
	# Advance both parents identically
	for i in range(10):
		parent1.randf()
		parent2.randf()
	
	var child1: SeededRng = parent1.fork()
	var child2: SeededRng = parent2.fork()
	
	# Children should produce identical sequences
	for i in range(100):
		assert_float_equal(child1.randf(), child2.randf(), 0.0, "Forked RNGs should be deterministic")


## Tests that forking does not break parent determinism.
func test_fork_does_not_affect_parent_determinism() -> void:
	var rng1: SeededRng = SeededRng.new(12345)
	var rng2: SeededRng = SeededRng.new(12345)
	
	for i in range(5):
		rng1.randf()
		rng2.randf()
	
	var _child1: SeededRng = rng1.fork()
	var _child2: SeededRng = rng2.fork()
	
	# Parents should still be in sync
	for i in range(50):
		assert_float_equal(rng1.randf(), rng2.randf(), 0.0, "Parent RNGs should remain deterministic after fork")
