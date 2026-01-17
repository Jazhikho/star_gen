## Tests for version constants.
extends TestCase


## Tests that GENERATOR_VERSION is a non-empty string.
func test_generator_version_is_non_empty_string() -> void:
	assert_true(Versions.GENERATOR_VERSION.length() > 0, "GENERATOR_VERSION should not be empty")


## Tests that GENERATOR_VERSION follows semver format (x.y.z).
func test_generator_version_is_semver_format() -> void:
	var parts: PackedStringArray = Versions.GENERATOR_VERSION.split(".")
	assert_equal(parts.size(), 3, "GENERATOR_VERSION should have 3 parts (x.y.z)")
	
	for part in parts:
		assert_true(part.is_valid_int(), "Each semver part should be a valid integer")


## Tests that SCHEMA_VERSION is a positive integer.
func test_schema_version_is_positive() -> void:
	assert_greater_than(Versions.SCHEMA_VERSION, 0, "SCHEMA_VERSION should be positive")
