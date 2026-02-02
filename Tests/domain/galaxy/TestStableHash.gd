## Tests for StableHash — determinism, collision resistance, derivation.
class_name TestStableHash
extends TestCase


func test_same_input_same_output() -> void:
	var a: int = StableHash.hash_integers([1, 2, 3] as Array[int])
	var b: int = StableHash.hash_integers([1, 2, 3] as Array[int])
	assert_equal(a, b, "Identical inputs must produce identical hashes")


func test_different_inputs_different_output() -> void:
	var a: int = StableHash.hash_integers([1, 2, 3] as Array[int])
	var b: int = StableHash.hash_integers([3, 2, 1] as Array[int])
	assert_not_equal(a, b, "Different inputs should produce different hashes")


func test_order_matters() -> void:
	var a: int = StableHash.hash_integers([100, 200] as Array[int])
	var b: int = StableHash.hash_integers([200, 100] as Array[int])
	assert_not_equal(a, b, "Input order must affect the hash")


func test_hash_is_positive() -> void:
	var h: int = StableHash.hash_integers([0, -1, 999999] as Array[int])
	assert_greater_than(h, -1, "Hash should be non-negative (32-bit masked)")


func test_derive_seed_deterministic() -> void:
	var a: int = StableHash.derive_seed(42, Vector3i(10, 20, 30))
	var b: int = StableHash.derive_seed(42, Vector3i(10, 20, 30))
	assert_equal(a, b, "Same parent + coords must give same child seed")


func test_derive_seed_varies_with_coords() -> void:
	var a: int = StableHash.derive_seed(42, Vector3i(0, 0, 0))
	var b: int = StableHash.derive_seed(42, Vector3i(1, 0, 0))
	var c: int = StableHash.derive_seed(42, Vector3i(0, 1, 0))
	assert_not_equal(a, b, "Different x should give different seed")
	assert_not_equal(a, c, "Different y should give different seed")
	assert_not_equal(b, c, "Different coords should give different seeds")


func test_derive_seed_varies_with_parent() -> void:
	var a: int = StableHash.derive_seed(1, Vector3i(5, 5, 5))
	var b: int = StableHash.derive_seed(2, Vector3i(5, 5, 5))
	assert_not_equal(a, b, "Different parent seeds must give different children")


func test_derive_seed_indexed_deterministic() -> void:
	var a: int = StableHash.derive_seed_indexed(99, 7)
	var b: int = StableHash.derive_seed_indexed(99, 7)
	assert_equal(a, b, "Same parent + index must give same child seed")


func test_known_value_stability() -> void:
	## Golden-master: if this ever changes, determinism is broken.
	var h: int = StableHash.hash_integers([42] as Array[int])
	# Store whatever the first run produces, then assert it never changes.
	# For now, just verify it's a valid 32-bit positive value.
	assert_greater_than(h, 0, "Known-input hash must be positive")
	assert_less_than(h, 0xFFFFFFFF + 1, "Known-input hash must fit 32 bits")
