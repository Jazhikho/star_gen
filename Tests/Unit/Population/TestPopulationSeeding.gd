## Tests for PopulationSeeding deterministic seed generation.
extends TestCase

const _population_seeding: GDScript = preload("res://src/domain/population/PopulationSeeding.gd")


## Tests that the same body_id + base_seed always produces the same seed.
func test_determinism_same_inputs_same_output() -> void:
	var seed1: int = PopulationSeeding.generate_population_seed("planet_abc", 42)
	var seed2: int = PopulationSeeding.generate_population_seed("planet_abc", 42)
	assert_equal(seed1, seed2, "Same inputs should produce same seed")


## Tests that different body_ids produce different seeds.
func test_different_body_ids_different_seeds() -> void:
	var seed1: int = PopulationSeeding.generate_population_seed("planet_abc", 42)
	var seed2: int = PopulationSeeding.generate_population_seed("planet_def", 42)
	assert_not_equal(seed1, seed2, "Different body_ids should produce different seeds")


## Tests that different base seeds produce different seeds.
func test_different_base_seeds_different_seeds() -> void:
	var seed1: int = PopulationSeeding.generate_population_seed("planet_abc", 42)
	var seed2: int = PopulationSeeding.generate_population_seed("planet_abc", 43)
	assert_not_equal(seed1, seed2, "Different base seeds should produce different seeds")


## Tests that seeds are positive (non-negative).
func test_seeds_are_positive() -> void:
	for i in range(20):
		var body_id: String = "body_%d" % i
		var seed_val: int = PopulationSeeding.generate_population_seed(body_id, i * 100)
		assert_true(seed_val >= 0, "Seed should be non-negative for body '%s'" % body_id)


## Tests order independence: generating seeds in different order gives same results.
func test_order_independence() -> void:
	var ids: Array[String] = ["planet_01", "planet_02", "planet_03", "moon_01"]
	var base: int = 12345
	
	# Generate in forward order
	var forward_seeds: Array[int] = []
	for id in ids:
		forward_seeds.append(PopulationSeeding.generate_population_seed(id, base))
	
	# Generate in reverse order
	var reverse_seeds: Array[int] = []
	for i in range(ids.size() - 1, -1, -1):
		reverse_seeds.append(PopulationSeeding.generate_population_seed(ids[i], base))
	reverse_seeds.reverse()
	
	for i in range(ids.size()):
		assert_equal(
			forward_seeds[i], reverse_seeds[i],
			"Seed for '%s' should be same regardless of generation order" % ids[i]
		)


## Tests that native sub-seeds are deterministic.
func test_native_seed_determinism() -> void:
	var pop_seed: int = 99999
	var seed1: int = PopulationSeeding.generate_native_seed(pop_seed, 0)
	var seed2: int = PopulationSeeding.generate_native_seed(pop_seed, 0)
	assert_equal(seed1, seed2, "Same population seed + index should give same native seed")


## Tests that different native indices produce different seeds.
func test_native_seeds_differ_by_index() -> void:
	var pop_seed: int = 99999
	var seed0: int = PopulationSeeding.generate_native_seed(pop_seed, 0)
	var seed1: int = PopulationSeeding.generate_native_seed(pop_seed, 1)
	assert_not_equal(seed0, seed1, "Different native indices should produce different seeds")


## Tests that colony sub-seeds differ from native sub-seeds.
func test_colony_seeds_differ_from_native_seeds() -> void:
	var pop_seed: int = 99999
	var native_seed: int = PopulationSeeding.generate_native_seed(pop_seed, 0)
	var colony_seed: int = PopulationSeeding.generate_colony_seed(pop_seed, 0)
	assert_not_equal(native_seed, colony_seed, "Colony and native seeds for same index should differ")
