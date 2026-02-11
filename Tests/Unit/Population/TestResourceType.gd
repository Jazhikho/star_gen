## Tests for ResourceType enum and utilities.
extends TestCase

const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Tests to_string_name returns correct values.
func test_to_string_name() -> void:
	assert_equal(ResourceType.to_string_name(ResourceType.Type.WATER), "Water")
	assert_equal(ResourceType.to_string_name(ResourceType.Type.METALS), "Metals")
	assert_equal(ResourceType.to_string_name(ResourceType.Type.RARE_ELEMENTS), "Rare Elements")
	assert_equal(ResourceType.to_string_name(ResourceType.Type.ORGANICS), "Organics")


## Tests from_string parses correctly.
func test_from_string() -> void:
	assert_equal(ResourceType.from_string("water"), ResourceType.Type.WATER)
	assert_equal(ResourceType.from_string("WATER"), ResourceType.Type.WATER)
	assert_equal(ResourceType.from_string("Rare Elements"), ResourceType.Type.RARE_ELEMENTS)
	assert_equal(ResourceType.from_string("rare_elements"), ResourceType.Type.RARE_ELEMENTS)


## Tests from_string returns SILICATES for unknown values.
func test_from_string_unknown() -> void:
	assert_equal(ResourceType.from_string("unknown"), ResourceType.Type.SILICATES)
	assert_equal(ResourceType.from_string(""), ResourceType.Type.SILICATES)


## Tests round-trip conversion.
func test_round_trip() -> void:
	for resource_int in range(ResourceType.count()):
		var resource: ResourceType.Type = resource_int as ResourceType.Type
		var name_str: String = ResourceType.to_string_name(resource)
		var restored: ResourceType.Type = ResourceType.from_string(name_str)
		assert_equal(restored, resource, "Round-trip failed for resource %d" % resource_int)


## Tests count returns correct number.
func test_count() -> void:
	assert_equal(ResourceType.count(), 10)
