## Unit tests for StationPlacementContext enum.
extends TestCase


func test_to_string_name_returns_correct_values() -> void:
	assert_equal(StationPlacementContext.to_string_name(StationPlacementContext.Context.BRIDGE_SYSTEM), "Bridge System")
	assert_equal(StationPlacementContext.to_string_name(StationPlacementContext.Context.COLONY_WORLD), "Colony World")
	assert_equal(StationPlacementContext.to_string_name(StationPlacementContext.Context.NATIVE_WORLD), "Native World")
	assert_equal(StationPlacementContext.to_string_name(StationPlacementContext.Context.RESOURCE_SYSTEM), "Resource System")
	assert_equal(StationPlacementContext.to_string_name(StationPlacementContext.Context.STRATEGIC), "Strategic")
	assert_equal(StationPlacementContext.to_string_name(StationPlacementContext.Context.SCIENTIFIC), "Scientific")
	assert_equal(StationPlacementContext.to_string_name(StationPlacementContext.Context.OTHER), "Other")


func test_from_string_parses_correctly() -> void:
	assert_equal(StationPlacementContext.from_string("Bridge System"), StationPlacementContext.Context.BRIDGE_SYSTEM)
	assert_equal(StationPlacementContext.from_string("bridge_system"), StationPlacementContext.Context.BRIDGE_SYSTEM)
	assert_equal(StationPlacementContext.from_string("bridge"), StationPlacementContext.Context.BRIDGE_SYSTEM)
	assert_equal(StationPlacementContext.from_string("Colony World"), StationPlacementContext.Context.COLONY_WORLD)
	assert_equal(StationPlacementContext.from_string("colony"), StationPlacementContext.Context.COLONY_WORLD)
	assert_equal(StationPlacementContext.from_string("Native World"), StationPlacementContext.Context.NATIVE_WORLD)
	assert_equal(StationPlacementContext.from_string("Resource System"), StationPlacementContext.Context.RESOURCE_SYSTEM)
	assert_equal(StationPlacementContext.from_string("Strategic"), StationPlacementContext.Context.STRATEGIC)
	assert_equal(StationPlacementContext.from_string("Scientific"), StationPlacementContext.Context.SCIENTIFIC)
	assert_equal(StationPlacementContext.from_string("science"), StationPlacementContext.Context.SCIENTIFIC)


func test_from_string_is_case_insensitive() -> void:
	assert_equal(StationPlacementContext.from_string("BRIDGE_SYSTEM"), StationPlacementContext.Context.BRIDGE_SYSTEM)
	assert_equal(StationPlacementContext.from_string("colony_world"), StationPlacementContext.Context.COLONY_WORLD)


func test_from_string_returns_default_for_unknown() -> void:
	assert_equal(StationPlacementContext.from_string("unknown"), StationPlacementContext.Context.OTHER)
	assert_equal(StationPlacementContext.from_string(""), StationPlacementContext.Context.OTHER)


func test_favors_utility_stations() -> void:
	assert_true(StationPlacementContext.favors_utility_stations(StationPlacementContext.Context.BRIDGE_SYSTEM))
	assert_false(StationPlacementContext.favors_utility_stations(StationPlacementContext.Context.COLONY_WORLD))
	assert_false(StationPlacementContext.favors_utility_stations(StationPlacementContext.Context.NATIVE_WORLD))
	assert_false(StationPlacementContext.favors_utility_stations(StationPlacementContext.Context.RESOURCE_SYSTEM))


func test_can_support_large_stations() -> void:
	assert_false(StationPlacementContext.can_support_large_stations(StationPlacementContext.Context.BRIDGE_SYSTEM))
	assert_true(StationPlacementContext.can_support_large_stations(StationPlacementContext.Context.COLONY_WORLD))
	assert_true(StationPlacementContext.can_support_large_stations(StationPlacementContext.Context.NATIVE_WORLD))
	assert_true(StationPlacementContext.can_support_large_stations(StationPlacementContext.Context.RESOURCE_SYSTEM))
	assert_false(StationPlacementContext.can_support_large_stations(StationPlacementContext.Context.STRATEGIC))


func test_requires_spacefaring_natives() -> void:
	assert_false(StationPlacementContext.requires_spacefaring_natives(StationPlacementContext.Context.BRIDGE_SYSTEM))
	assert_false(StationPlacementContext.requires_spacefaring_natives(StationPlacementContext.Context.COLONY_WORLD))
	assert_true(StationPlacementContext.requires_spacefaring_natives(StationPlacementContext.Context.NATIVE_WORLD))
	assert_false(StationPlacementContext.requires_spacefaring_natives(StationPlacementContext.Context.RESOURCE_SYSTEM))


func test_count_returns_correct_value() -> void:
	assert_equal(StationPlacementContext.count(), 7)


func test_roundtrip_string_conversion() -> void:
	for i in range(StationPlacementContext.count()):
		var context: StationPlacementContext.Context = i as StationPlacementContext.Context
		var name_str: String = StationPlacementContext.to_string_name(context)
		var parsed: StationPlacementContext.Context = StationPlacementContext.from_string(name_str)
		assert_equal(parsed, context, "Roundtrip failed for context %d" % i)
