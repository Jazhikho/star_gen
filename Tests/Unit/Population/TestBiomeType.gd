## Tests for BiomeType enum and utilities.
extends TestCase

const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")


## Tests to_string_name returns correct values.
func test_to_string_name() -> void:
	assert_equal(BiomeType.to_string_name(BiomeType.Type.OCEAN), "Ocean")
	assert_equal(BiomeType.to_string_name(BiomeType.Type.ICE_SHEET), "Ice Sheet")
	assert_equal(BiomeType.to_string_name(BiomeType.Type.FOREST), "Forest")
	assert_equal(BiomeType.to_string_name(BiomeType.Type.DESERT), "Desert")
	assert_equal(BiomeType.to_string_name(BiomeType.Type.VOLCANIC), "Volcanic")
	assert_equal(BiomeType.to_string_name(BiomeType.Type.BARREN), "Barren")


## Tests from_string parses correctly.
func test_from_string() -> void:
	assert_equal(BiomeType.from_string("ocean"), BiomeType.Type.OCEAN)
	assert_equal(BiomeType.from_string("OCEAN"), BiomeType.Type.OCEAN)
	assert_equal(BiomeType.from_string("Ice Sheet"), BiomeType.Type.ICE_SHEET)
	assert_equal(BiomeType.from_string("ice_sheet"), BiomeType.Type.ICE_SHEET)


## Tests from_string returns BARREN for unknown values.
func test_from_string_unknown() -> void:
	assert_equal(BiomeType.from_string("unknown"), BiomeType.Type.BARREN)
	assert_equal(BiomeType.from_string(""), BiomeType.Type.BARREN)


## Tests can_support_life returns expected values.
func test_can_support_life() -> void:
	assert_true(BiomeType.can_support_life(BiomeType.Type.OCEAN))
	assert_true(BiomeType.can_support_life(BiomeType.Type.FOREST))
	assert_true(BiomeType.can_support_life(BiomeType.Type.JUNGLE))
	assert_true(BiomeType.can_support_life(BiomeType.Type.GRASSLAND))
	assert_true(BiomeType.can_support_life(BiomeType.Type.DESERT))
	assert_true(BiomeType.can_support_life(BiomeType.Type.TUNDRA))

	assert_false(BiomeType.can_support_life(BiomeType.Type.BARREN))
	assert_false(BiomeType.can_support_life(BiomeType.Type.VOLCANIC))
	assert_false(BiomeType.can_support_life(BiomeType.Type.ICE_SHEET))


## Tests round-trip conversion.
func test_round_trip() -> void:
	for biome_int in range(BiomeType.count()):
		var biome: BiomeType.Type = biome_int as BiomeType.Type
		var name_str: String = BiomeType.to_string_name(biome)
		var restored: BiomeType.Type = BiomeType.from_string(name_str)
		assert_equal(restored, biome, "Round-trip failed for biome %d" % biome_int)


## Tests count returns correct number.
func test_count() -> void:
	assert_equal(BiomeType.count(), 14)
