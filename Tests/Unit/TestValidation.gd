## Tests for validation functions.
extends TestCase


## Tests is_positive_float with various values.
func test_is_positive_float() -> void:
	assert_true(Validation.is_positive_float(0.001))
	assert_true(Validation.is_positive_float(100.0))
	assert_false(Validation.is_positive_float(0.0))
	assert_false(Validation.is_positive_float(-0.001))


## Tests is_non_negative_float with various values.
func test_is_non_negative_float() -> void:
	assert_true(Validation.is_non_negative_float(0.0))
	assert_true(Validation.is_non_negative_float(100.0))
	assert_false(Validation.is_non_negative_float(-0.001))


## Tests is_positive_int with various values.
func test_is_positive_int() -> void:
	assert_true(Validation.is_positive_int(1))
	assert_true(Validation.is_positive_int(100))
	assert_false(Validation.is_positive_int(0))
	assert_false(Validation.is_positive_int(-1))


## Tests is_non_negative_int with various values.
func test_is_non_negative_int() -> void:
	assert_true(Validation.is_non_negative_int(0))
	assert_true(Validation.is_non_negative_int(100))
	assert_false(Validation.is_non_negative_int(-1))


## Tests is_in_range_float with boundary and interior values.
func test_is_in_range_float() -> void:
	assert_true(Validation.is_in_range_float(5.0, 0.0, 10.0))
	assert_true(Validation.is_in_range_float(0.0, 0.0, 10.0))
	assert_true(Validation.is_in_range_float(10.0, 0.0, 10.0))
	assert_false(Validation.is_in_range_float(-0.1, 0.0, 10.0))
	assert_false(Validation.is_in_range_float(10.1, 0.0, 10.0))


## Tests is_in_range_int with boundary and interior values.
func test_is_in_range_int() -> void:
	assert_true(Validation.is_in_range_int(5, 0, 10))
	assert_true(Validation.is_in_range_int(0, 0, 10))
	assert_true(Validation.is_in_range_int(10, 0, 10))
	assert_false(Validation.is_in_range_int(-1, 0, 10))
	assert_false(Validation.is_in_range_int(11, 0, 10))


## Tests is_not_empty_string with various strings.
func test_is_not_empty_string() -> void:
	assert_true(Validation.is_not_empty_string("hello"))
	assert_true(Validation.is_not_empty_string(" "))
	assert_false(Validation.is_not_empty_string(""))


## Tests is_not_empty_array with various arrays.
func test_is_not_empty_array() -> void:
	assert_true(Validation.is_not_empty_array([1, 2, 3]))
	assert_true(Validation.is_not_empty_array([null]))
	assert_false(Validation.is_not_empty_array([]))


## Tests is_valid_enum with mock enum size.
func test_is_valid_enum() -> void:
	var enum_size: int = 5
	assert_true(Validation.is_valid_enum(0, enum_size))
	assert_true(Validation.is_valid_enum(4, enum_size))
	assert_false(Validation.is_valid_enum(-1, enum_size))
	assert_false(Validation.is_valid_enum(5, enum_size))


## Tests is_valid_seed accepts all integers.
func test_is_valid_seed() -> void:
	assert_true(Validation.is_valid_seed(0))
	assert_true(Validation.is_valid_seed(12345))
	assert_true(Validation.is_valid_seed(-12345))
	assert_true(Validation.is_valid_seed(9223372036854775807))
