## Unit tests for StationClass enum.
extends TestCase


func test_to_string_name_returns_correct_values() -> void:
	assert_equal(StationClass.to_string_name(StationClass.Class.U), "Utility")
	assert_equal(StationClass.to_string_name(StationClass.Class.O), "Outpost")
	assert_equal(StationClass.to_string_name(StationClass.Class.B), "Base")
	assert_equal(StationClass.to_string_name(StationClass.Class.A), "Anchor")
	assert_equal(StationClass.to_string_name(StationClass.Class.S), "Super")


func test_to_letter_returns_correct_values() -> void:
	assert_equal(StationClass.to_letter(StationClass.Class.U), "U")
	assert_equal(StationClass.to_letter(StationClass.Class.O), "O")
	assert_equal(StationClass.to_letter(StationClass.Class.B), "B")
	assert_equal(StationClass.to_letter(StationClass.Class.A), "A")
	assert_equal(StationClass.to_letter(StationClass.Class.S), "S")


func test_from_string_parses_full_names() -> void:
	assert_equal(StationClass.from_string("Utility"), StationClass.Class.U)
	assert_equal(StationClass.from_string("Outpost"), StationClass.Class.O)
	assert_equal(StationClass.from_string("Base"), StationClass.Class.B)
	assert_equal(StationClass.from_string("Anchor"), StationClass.Class.A)
	assert_equal(StationClass.from_string("Super"), StationClass.Class.S)


func test_from_string_parses_letters() -> void:
	assert_equal(StationClass.from_string("U"), StationClass.Class.U)
	assert_equal(StationClass.from_string("O"), StationClass.Class.O)
	assert_equal(StationClass.from_string("B"), StationClass.Class.B)
	assert_equal(StationClass.from_string("A"), StationClass.Class.A)
	assert_equal(StationClass.from_string("S"), StationClass.Class.S)


func test_from_string_is_case_insensitive() -> void:
	assert_equal(StationClass.from_string("utility"), StationClass.Class.U)
	assert_equal(StationClass.from_string("OUTPOST"), StationClass.Class.O)
	assert_equal(StationClass.from_string("bAsE"), StationClass.Class.B)


func test_from_string_returns_default_for_unknown() -> void:
	assert_equal(StationClass.from_string("unknown"), StationClass.Class.O)
	assert_equal(StationClass.from_string(""), StationClass.Class.O)


func test_get_max_capacity_returns_correct_values() -> void:
	assert_equal(StationClass.get_max_capacity(StationClass.Class.U), 10000)
	assert_equal(StationClass.get_max_capacity(StationClass.Class.O), 10000)
	assert_equal(StationClass.get_max_capacity(StationClass.Class.B), 100000)
	assert_equal(StationClass.get_max_capacity(StationClass.Class.A), 1000000)
	assert_equal(StationClass.get_max_capacity(StationClass.Class.S), -1) # Unlimited


func test_get_min_capacity_returns_correct_values() -> void:
	assert_equal(StationClass.get_min_capacity(StationClass.Class.U), 0)
	assert_equal(StationClass.get_min_capacity(StationClass.Class.O), 0)
	assert_equal(StationClass.get_min_capacity(StationClass.Class.B), 0)
	assert_equal(StationClass.get_min_capacity(StationClass.Class.A), 100000)
	assert_equal(StationClass.get_min_capacity(StationClass.Class.S), 1000000)


func test_get_class_for_population_small_defaults_to_outpost() -> void:
	assert_equal(StationClass.get_class_for_population(100), StationClass.Class.O)
	assert_equal(StationClass.get_class_for_population(5000), StationClass.Class.O)
	assert_equal(StationClass.get_class_for_population(10000), StationClass.Class.O)


func test_get_class_for_population_small_with_utility_flag() -> void:
	assert_equal(StationClass.get_class_for_population(100, true), StationClass.Class.U)
	assert_equal(StationClass.get_class_for_population(5000, true), StationClass.Class.U)
	assert_equal(StationClass.get_class_for_population(10000, true), StationClass.Class.U)


func test_get_class_for_population_base() -> void:
	assert_equal(StationClass.get_class_for_population(10001), StationClass.Class.B)
	assert_equal(StationClass.get_class_for_population(50000), StationClass.Class.B)
	assert_equal(StationClass.get_class_for_population(100000), StationClass.Class.B)


func test_get_class_for_population_anchor() -> void:
	assert_equal(StationClass.get_class_for_population(100001), StationClass.Class.A)
	assert_equal(StationClass.get_class_for_population(500000), StationClass.Class.A)
	assert_equal(StationClass.get_class_for_population(1000000), StationClass.Class.A)


func test_get_class_for_population_super() -> void:
	assert_equal(StationClass.get_class_for_population(1000001), StationClass.Class.S)
	assert_equal(StationClass.get_class_for_population(10000000), StationClass.Class.S)


func test_uses_outpost_government() -> void:
	assert_true(StationClass.uses_outpost_government(StationClass.Class.U))
	assert_true(StationClass.uses_outpost_government(StationClass.Class.O))
	assert_false(StationClass.uses_outpost_government(StationClass.Class.B))
	assert_false(StationClass.uses_outpost_government(StationClass.Class.A))
	assert_false(StationClass.uses_outpost_government(StationClass.Class.S))


func test_uses_colony_government() -> void:
	assert_false(StationClass.uses_colony_government(StationClass.Class.U))
	assert_false(StationClass.uses_colony_government(StationClass.Class.O))
	assert_true(StationClass.uses_colony_government(StationClass.Class.B))
	assert_true(StationClass.uses_colony_government(StationClass.Class.A))
	assert_true(StationClass.uses_colony_government(StationClass.Class.S))


func test_get_description_returns_non_empty() -> void:
	for i in range(StationClass.count()):
		var station_class: StationClass.Class = i as StationClass.Class
		var description: String = StationClass.get_description(station_class)
		assert_true(description.length() > 0, "Class %d should have description" % i)


func test_count_returns_correct_value() -> void:
	assert_equal(StationClass.count(), 5)


func test_roundtrip_string_conversion() -> void:
	for i in range(StationClass.count()):
		var station_class: StationClass.Class = i as StationClass.Class
		var name_str: String = StationClass.to_string_name(station_class)
		var parsed: StationClass.Class = StationClass.from_string(name_str)
		assert_equal(parsed, station_class, "Roundtrip failed for class %d" % i)
