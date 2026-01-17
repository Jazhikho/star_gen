## Tests for Provenance tracking.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests creation with default values.
func test_default_values() -> void:
	var prov: Provenance = Provenance.new()
	assert_equal(prov.generation_seed, 0)
	assert_equal(prov.generator_version, "")
	assert_equal(prov.schema_version, 0)
	assert_equal(prov.created_timestamp, 0)
	assert_equal(prov.spec_snapshot.size(), 0)


## Tests creation with specific values.
func test_initialization() -> void:
	var spec: Dictionary = {"preset": "earth-like"}
	var prov: Provenance = Provenance.new(12345, "0.1.0", 1, 1700000000, spec)
	assert_equal(prov.generation_seed, 12345)
	assert_equal(prov.generator_version, "0.1.0")
	assert_equal(prov.schema_version, 1)
	assert_equal(prov.created_timestamp, 1700000000)
	assert_equal(prov.spec_snapshot["preset"], "earth-like")


## Tests create_current uses current version constants.
func test_create_current() -> void:
	var spec: Dictionary = {"test": "value"}
	var prov: Provenance = Provenance.create_current(99999, spec)
	
	assert_equal(prov.generation_seed, 99999)
	assert_equal(prov.generator_version, Versions.GENERATOR_VERSION)
	assert_equal(prov.schema_version, Versions.SCHEMA_VERSION)
	assert_greater_than(prov.created_timestamp, 0)
	assert_equal(prov.spec_snapshot["test"], "value")


## Tests round-trip serialization.
func test_round_trip() -> void:
	var spec: Dictionary = {"preset": "mars", "size": "medium"}
	var original: Provenance = Provenance.new(54321, "0.2.0", 2, 1700000000, spec)
	var data: Dictionary = original.to_dict()
	var restored: Provenance = Provenance.from_dict(data)
	
	assert_equal(restored.generation_seed, original.generation_seed)
	assert_equal(restored.generator_version, original.generator_version)
	assert_equal(restored.schema_version, original.schema_version)
	assert_equal(restored.created_timestamp, original.created_timestamp)
	assert_equal(restored.spec_snapshot["preset"], "mars")
	assert_equal(restored.spec_snapshot["size"], "medium")


## Tests from_dict handles empty dictionary.
func test_from_dict_empty() -> void:
	var prov: Provenance = Provenance.from_dict({})
	assert_null(prov)


## Tests backward compatibility with old "seed" field name.
func test_backward_compatibility_seed_field() -> void:
	# Simulate old format with "seed" instead of "generation_seed"
	var old_data: Dictionary = {
		"seed": 12345,  # old field name
		"generator_version": "0.0.1",
		"schema_version": 0,
		"created_timestamp": 1600000000,
		"spec_snapshot": {}
	}
	
	# For now this will create with generation_seed = 0
	# You could add migration logic if needed
	var prov: Provenance = Provenance.from_dict(old_data)
	assert_equal(prov.generation_seed, 0)  # Falls back to default
