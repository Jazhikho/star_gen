## Unit tests for StationPurpose enum.
extends TestCase


func test_to_string_name_returns_correct_values() -> void:
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.UTILITY), "Utility")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.TRADE), "Trade")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.MILITARY), "Military")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.SCIENCE), "Science")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.MINING), "Mining")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.RESIDENTIAL), "Residential")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.ADMINISTRATIVE), "Administrative")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.INDUSTRIAL), "Industrial")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.MEDICAL), "Medical")
	assert_equal(StationPurpose.to_string_name(StationPurpose.Purpose.COMMUNICATIONS), "Communications")


func test_from_string_parses_correctly() -> void:
	assert_equal(StationPurpose.from_string("Utility"), StationPurpose.Purpose.UTILITY)
	assert_equal(StationPurpose.from_string("Trade"), StationPurpose.Purpose.TRADE)
	assert_equal(StationPurpose.from_string("Military"), StationPurpose.Purpose.MILITARY)
	assert_equal(StationPurpose.from_string("Science"), StationPurpose.Purpose.SCIENCE)
	assert_equal(StationPurpose.from_string("Mining"), StationPurpose.Purpose.MINING)


func test_from_string_is_case_insensitive() -> void:
	assert_equal(StationPurpose.from_string("TRADE"), StationPurpose.Purpose.TRADE)
	assert_equal(StationPurpose.from_string("military"), StationPurpose.Purpose.MILITARY)


func test_from_string_returns_default_for_unknown() -> void:
	assert_equal(StationPurpose.from_string("unknown"), StationPurpose.Purpose.UTILITY)
	assert_equal(StationPurpose.from_string(""), StationPurpose.Purpose.UTILITY)


func test_typical_utility_purposes_returns_non_empty() -> void:
	var purposes: Array[StationPurpose.Purpose] = StationPurpose.typical_utility_purposes()
	assert_true(purposes.size() > 0)
	assert_true(StationPurpose.Purpose.UTILITY in purposes)


func test_typical_outpost_purposes_returns_non_empty() -> void:
	var purposes: Array[StationPurpose.Purpose] = StationPurpose.typical_outpost_purposes()
	assert_true(purposes.size() > 0)
	assert_true(StationPurpose.Purpose.MILITARY in purposes)
	assert_true(StationPurpose.Purpose.SCIENCE in purposes)


func test_typical_settlement_purposes_returns_non_empty() -> void:
	var purposes: Array[StationPurpose.Purpose] = StationPurpose.typical_settlement_purposes()
	assert_true(purposes.size() > 0)
	assert_true(StationPurpose.Purpose.RESIDENTIAL in purposes)


func test_is_small_station_purpose() -> void:
	assert_true(StationPurpose.is_small_station_purpose(StationPurpose.Purpose.UTILITY))
	assert_true(StationPurpose.is_small_station_purpose(StationPurpose.Purpose.MINING))
	assert_true(StationPurpose.is_small_station_purpose(StationPurpose.Purpose.SCIENCE))
	assert_true(StationPurpose.is_small_station_purpose(StationPurpose.Purpose.MILITARY))
	assert_false(StationPurpose.is_small_station_purpose(StationPurpose.Purpose.RESIDENTIAL))
	assert_false(StationPurpose.is_small_station_purpose(StationPurpose.Purpose.ADMINISTRATIVE))


func test_count_returns_correct_value() -> void:
	assert_equal(StationPurpose.count(), 10)


func test_roundtrip_string_conversion() -> void:
	for i in range(StationPurpose.count()):
		var purpose: StationPurpose.Purpose = i as StationPurpose.Purpose
		var name_str: String = StationPurpose.to_string_name(purpose)
		var parsed: StationPurpose.Purpose = StationPurpose.from_string(name_str)
		assert_equal(parsed, purpose, "Roundtrip failed for purpose %d" % i)
