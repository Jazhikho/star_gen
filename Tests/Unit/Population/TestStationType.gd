## Unit tests for StationType enum.
extends TestCase


func test_to_string_name_returns_correct_values() -> void:
	assert_equal(StationType.to_string_name(StationType.Type.ORBITAL), "Orbital")
	assert_equal(StationType.to_string_name(StationType.Type.DEEP_SPACE), "Deep Space")
	assert_equal(StationType.to_string_name(StationType.Type.LAGRANGE), "Lagrange Point")
	assert_equal(StationType.to_string_name(StationType.Type.ASTEROID_BELT), "Asteroid Belt")


func test_from_string_parses_correctly() -> void:
	assert_equal(StationType.from_string("Orbital"), StationType.Type.ORBITAL)
	assert_equal(StationType.from_string("Deep Space"), StationType.Type.DEEP_SPACE)
	assert_equal(StationType.from_string("deep_space"), StationType.Type.DEEP_SPACE)
	assert_equal(StationType.from_string("Lagrange"), StationType.Type.LAGRANGE)
	assert_equal(StationType.from_string("Lagrange Point"), StationType.Type.LAGRANGE)
	assert_equal(StationType.from_string("Asteroid Belt"), StationType.Type.ASTEROID_BELT)
	assert_equal(StationType.from_string("belt"), StationType.Type.ASTEROID_BELT)


func test_from_string_is_case_insensitive() -> void:
	assert_equal(StationType.from_string("ORBITAL"), StationType.Type.ORBITAL)
	assert_equal(StationType.from_string("orbital"), StationType.Type.ORBITAL)


func test_from_string_returns_default_for_unknown() -> void:
	assert_equal(StationType.from_string("unknown"), StationType.Type.ORBITAL)
	assert_equal(StationType.from_string(""), StationType.Type.ORBITAL)


func test_is_body_associated() -> void:
	assert_true(StationType.is_body_associated(StationType.Type.ORBITAL))
	assert_false(StationType.is_body_associated(StationType.Type.DEEP_SPACE))
	assert_false(StationType.is_body_associated(StationType.Type.LAGRANGE))
	assert_false(StationType.is_body_associated(StationType.Type.ASTEROID_BELT))


func test_is_free_floating() -> void:
	assert_false(StationType.is_free_floating(StationType.Type.ORBITAL))
	assert_true(StationType.is_free_floating(StationType.Type.DEEP_SPACE))
	assert_true(StationType.is_free_floating(StationType.Type.LAGRANGE))
	assert_true(StationType.is_free_floating(StationType.Type.ASTEROID_BELT))


func test_is_body_associated_and_free_floating_are_mutually_exclusive() -> void:
	for i in range(StationType.count()):
		var station_type: StationType.Type = i as StationType.Type
		var is_body: bool = StationType.is_body_associated(station_type)
		var is_free: bool = StationType.is_free_floating(station_type)
		assert_true(is_body != is_free, "Type %d should be either body-associated or free-floating" % i)


func test_count_returns_correct_value() -> void:
	assert_equal(StationType.count(), 4)


func test_roundtrip_string_conversion() -> void:
	for i in range(StationType.count()):
		var station_type: StationType.Type = i as StationType.Type
		var name_str: String = StationType.to_string_name(station_type)
		var parsed: StationType.Type = StationType.from_string(name_str)
		assert_equal(parsed, station_type, "Roundtrip failed for type %d" % i)
