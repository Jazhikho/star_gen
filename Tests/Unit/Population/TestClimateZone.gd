## Tests for ClimateZone enum and utilities.
extends TestCase

const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")


## Tests to_string_name returns correct values.
func test_to_string_name() -> void:
	assert_equal(ClimateZone.to_string_name(ClimateZone.Zone.POLAR), "Polar")
	assert_equal(ClimateZone.to_string_name(ClimateZone.Zone.SUBPOLAR), "Subpolar")
	assert_equal(ClimateZone.to_string_name(ClimateZone.Zone.TEMPERATE), "Temperate")
	assert_equal(ClimateZone.to_string_name(ClimateZone.Zone.SUBTROPICAL), "Subtropical")
	assert_equal(ClimateZone.to_string_name(ClimateZone.Zone.TROPICAL), "Tropical")
	assert_equal(ClimateZone.to_string_name(ClimateZone.Zone.ARID), "Arid")
	assert_equal(ClimateZone.to_string_name(ClimateZone.Zone.EXTREME), "Extreme")


## Tests from_string parses correctly.
func test_from_string() -> void:
	assert_equal(ClimateZone.from_string("polar"), ClimateZone.Zone.POLAR)
	assert_equal(ClimateZone.from_string("POLAR"), ClimateZone.Zone.POLAR)
	assert_equal(ClimateZone.from_string("Temperate"), ClimateZone.Zone.TEMPERATE)
	assert_equal(ClimateZone.from_string("tropical"), ClimateZone.Zone.TROPICAL)


## Tests from_string returns EXTREME for unknown values.
func test_from_string_unknown() -> void:
	assert_equal(ClimateZone.from_string("unknown"), ClimateZone.Zone.EXTREME)
	assert_equal(ClimateZone.from_string(""), ClimateZone.Zone.EXTREME)


## Tests round-trip conversion.
func test_round_trip() -> void:
	for zone_int in range(ClimateZone.count()):
		var zone: ClimateZone.Zone = zone_int as ClimateZone.Zone
		var name_str: String = ClimateZone.to_string_name(zone)
		var restored: ClimateZone.Zone = ClimateZone.from_string(name_str)
		assert_equal(restored, zone, "Round-trip failed for zone %d" % zone_int)


## Tests count returns correct number.
func test_count() -> void:
	assert_equal(ClimateZone.count(), 7)
