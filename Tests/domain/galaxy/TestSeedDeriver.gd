## Tests for SeedDeriver — determinism and hierarchy correctness.
class_name TestSeedDeriver
extends TestCase


func test_quadrant_seed_deterministic() -> void:
	var a: int = SeedDeriver.derive_quadrant_seed(42, Vector3i(3, 1, 7))
	var b: int = SeedDeriver.derive_quadrant_seed(42, Vector3i(3, 1, 7))
	assert_equal(a, b, "Same galaxy seed + coords must give same quadrant seed")


func test_quadrant_seed_varies_with_coords() -> void:
	var a: int = SeedDeriver.derive_quadrant_seed(42, Vector3i(0, 0, 0))
	var b: int = SeedDeriver.derive_quadrant_seed(42, Vector3i(1, 0, 0))
	var c: int = SeedDeriver.derive_quadrant_seed(42, Vector3i(0, 1, 0))
	var d: int = SeedDeriver.derive_quadrant_seed(42, Vector3i(0, 0, 1))
	assert_not_equal(a, b, "Different x should give different seed")
	assert_not_equal(a, c, "Different y should give different seed")
	assert_not_equal(a, d, "Different z should give different seed")


func test_quadrant_seed_varies_with_galaxy_seed() -> void:
	var a: int = SeedDeriver.derive_quadrant_seed(1, Vector3i(5, 5, 5))
	var b: int = SeedDeriver.derive_quadrant_seed(2, Vector3i(5, 5, 5))
	assert_not_equal(a, b, "Different galaxy seeds must give different quadrant seeds")


func test_sector_seed_deterministic() -> void:
	var a: int = SeedDeriver.derive_sector_seed(999, Vector3i(4, 5, 6))
	var b: int = SeedDeriver.derive_sector_seed(999, Vector3i(4, 5, 6))
	assert_equal(a, b, "Same quadrant seed + coords must give same sector seed")


func test_sector_seed_varies_with_local_coords() -> void:
	var a: int = SeedDeriver.derive_sector_seed(999, Vector3i(0, 0, 0))
	var b: int = SeedDeriver.derive_sector_seed(999, Vector3i(9, 9, 9))
	assert_not_equal(a, b, "Different sector coords should give different seeds")


func test_subsector_seed_deterministic() -> void:
	var a: int = SeedDeriver.derive_subsector_seed(777, Vector3i(2, 3, 4))
	var b: int = SeedDeriver.derive_subsector_seed(777, Vector3i(2, 3, 4))
	assert_equal(a, b, "Same sector seed + coords must give same subsector seed")


func test_star_seed_deterministic() -> void:
	var a: int = SeedDeriver.derive_star_seed(555, 0)
	var b: int = SeedDeriver.derive_star_seed(555, 0)
	assert_equal(a, b, "Same subsector seed + index must give same star seed")


func test_star_seed_varies_with_index() -> void:
	var a: int = SeedDeriver.derive_star_seed(555, 0)
	var b: int = SeedDeriver.derive_star_seed(555, 1)
	var c: int = SeedDeriver.derive_star_seed(555, 2)
	assert_not_equal(a, b, "Different star indices should give different seeds")
	assert_not_equal(b, c, "Different star indices should give different seeds")


func test_full_chain_deterministic() -> void:
	var a: int = SeedDeriver.derive_sector_seed_full(
		42, Vector3i(3, 0, -2), Vector3i(5, 5, 5)
	)
	var b: int = SeedDeriver.derive_sector_seed_full(
		42, Vector3i(3, 0, -2), Vector3i(5, 5, 5)
	)
	assert_equal(a, b, "Full chain must be deterministic")


func test_full_chain_subsector_deterministic() -> void:
	var a: int = SeedDeriver.derive_subsector_seed_full(
		42, Vector3i(1, 0, 1), Vector3i(3, 3, 3), Vector3i(7, 7, 7)
	)
	var b: int = SeedDeriver.derive_subsector_seed_full(
		42, Vector3i(1, 0, 1), Vector3i(3, 3, 3), Vector3i(7, 7, 7)
	)
	assert_equal(a, b, "Full subsector chain must be deterministic")


func test_full_chain_varies_with_quadrant() -> void:
	var a: int = SeedDeriver.derive_sector_seed_full(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5)
	)
	var b: int = SeedDeriver.derive_sector_seed_full(
		42, Vector3i(1, 0, 0), Vector3i(5, 5, 5)
	)
	assert_not_equal(a, b, "Different quadrants must produce different sector seeds")


func test_hierarchy_independence() -> void:
	# Ensure quadrant seed is independent from sector-level derivation
	var q_seed: int = SeedDeriver.derive_quadrant_seed(42, Vector3i(5, 5, 5))
	var s_seed_a: int = SeedDeriver.derive_sector_seed(q_seed, Vector3i(0, 0, 0))
	var s_seed_b: int = SeedDeriver.derive_sector_seed(q_seed, Vector3i(0, 0, 1))

	# Both sector seeds should differ from each other and from the quadrant seed
	assert_not_equal(q_seed, s_seed_a, "Sector seed should differ from parent quadrant seed")
	assert_not_equal(s_seed_a, s_seed_b, "Adjacent sectors should have different seeds")
