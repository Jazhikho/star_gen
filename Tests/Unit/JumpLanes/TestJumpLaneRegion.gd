## Tests for JumpLaneRegion data model.
extends TestCase


func test_init_defaults() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()

	assert_equal(region.scope, JumpLaneRegion.RegionScope.SUBSECTOR)
	assert_equal(region.region_id, "")
	assert_equal(region.systems.size(), 0)


func test_init_with_values() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new(
		JumpLaneRegion.RegionScope.SECTOR,
		"Spinward Marches"
	)

	assert_equal(region.scope, JumpLaneRegion.RegionScope.SECTOR)
	assert_equal(region.region_id, "Spinward Marches")


func test_add_system() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3.ZERO, 1000)

	region.add_system(system)

	assert_equal(region.get_system_count(), 1)


func test_remove_system() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(JumpLaneSystem.new("sys_001", Vector3.ZERO, 1000))
	region.add_system(JumpLaneSystem.new("sys_002", Vector3.ZERO, 2000))

	var removed: bool = region.remove_system("sys_001")

	assert_true(removed)
	assert_equal(region.get_system_count(), 1)
	assert_null(region.get_system("sys_001"))


func test_remove_system_not_found() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()

	var removed: bool = region.remove_system("nonexistent")

	assert_false(removed)


func test_get_system() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3(1, 2, 3), 5000)
	region.add_system(system)

	var found: JumpLaneSystem = region.get_system("sys_001")

	assert_not_null(found)
	assert_equal(found.id, "sys_001")
	assert_equal(found.population, 5000)


func test_get_system_not_found() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()

	var found: JumpLaneSystem = region.get_system("nonexistent")

	assert_null(found)


func test_get_populated_systems() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(JumpLaneSystem.new("pop_1", Vector3.ZERO, 1000))
	region.add_system(JumpLaneSystem.new("unpop", Vector3.ZERO, 0))
	region.add_system(JumpLaneSystem.new("pop_2", Vector3.ZERO, 2000))

	var populated: Array[JumpLaneSystem] = region.get_populated_systems()

	assert_equal(populated.size(), 2)


func test_get_unpopulated_systems() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(JumpLaneSystem.new("pop_1", Vector3.ZERO, 1000))
	region.add_system(JumpLaneSystem.new("unpop_1", Vector3.ZERO, 0))
	region.add_system(JumpLaneSystem.new("unpop_2", Vector3.ZERO, 0))

	var unpopulated: Array[JumpLaneSystem] = region.get_unpopulated_systems()

	assert_equal(unpopulated.size(), 2)


func test_get_systems_sorted_by_population() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(JumpLaneSystem.new("high", Vector3.ZERO, 50000))
	region.add_system(JumpLaneSystem.new("low", Vector3.ZERO, 1000))
	region.add_system(JumpLaneSystem.new("unpop", Vector3.ZERO, 0))
	region.add_system(JumpLaneSystem.new("mid", Vector3.ZERO, 10000))

	var sorted: Array[JumpLaneSystem] = region.get_systems_sorted_by_population()

	assert_equal(sorted.size(), 3)
	assert_equal(sorted[0].id, "low")
	assert_equal(sorted[1].id, "mid")
	assert_equal(sorted[2].id, "high")


func test_get_populated_count() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(JumpLaneSystem.new("pop_1", Vector3.ZERO, 1000))
	region.add_system(JumpLaneSystem.new("unpop", Vector3.ZERO, 0))
	region.add_system(JumpLaneSystem.new("pop_2", Vector3.ZERO, 2000))

	assert_equal(region.get_populated_count(), 2)


func test_clear() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(JumpLaneSystem.new("sys_001", Vector3.ZERO, 1000))
	region.add_system(JumpLaneSystem.new("sys_002", Vector3.ZERO, 2000))

	region.clear()

	assert_equal(region.get_system_count(), 0)


func test_serialization_round_trip() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new(
		JumpLaneRegion.RegionScope.SECTOR,
		"Test Sector"
	)
	region.add_system(JumpLaneSystem.new("sys_001", Vector3(1, 2, 3), 10000))
	region.add_system(JumpLaneSystem.new("sys_002", Vector3(4, 5, 6), 20000))

	var data: Dictionary = region.to_dict()
	var restored: JumpLaneRegion = JumpLaneRegion.from_dict(data)

	assert_equal(restored.scope, JumpLaneRegion.RegionScope.SECTOR)
	assert_equal(restored.region_id, "Test Sector")
	assert_equal(restored.get_system_count(), 2)
	assert_not_null(restored.get_system("sys_001"))
	assert_not_null(restored.get_system("sys_002"))
