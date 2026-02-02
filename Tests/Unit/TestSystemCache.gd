## Unit tests for SystemCache.
extends TestCase

const _system_cache_script := preload("res://src/domain/system/SystemCache.gd")
const _solar_system := preload("res://src/domain/system/SolarSystem.gd")


var _cache: RefCounted = null


func get_test_name() -> String:
	return "TestSystemCache"


func before_each() -> void:
	_cache = _system_cache_script.new()


func test_starts_empty() -> void:
	assert_equal(_cache.get_cache_size(), 0, "Cache should start empty")


func test_has_system_returns_false_for_missing() -> void:
	assert_false(_cache.has_system(12345), "Should not have uncached system")


func test_get_system_returns_null_for_missing() -> void:
	assert_null(_cache.get_system(12345), "Should return null for uncached system")


func test_put_and_get_system() -> void:
	var system: SolarSystem = _solar_system.new("test_1", "Test System")

	_cache.put_system(12345, system)

	assert_true(_cache.has_system(12345), "Should have cached system")
	assert_equal(_cache.get_system(12345), system, "Should return cached system")


func test_cache_size_increases() -> void:
	var system1: SolarSystem = _solar_system.new("s1", "System One")
	var system2: SolarSystem = _solar_system.new("s2", "System Two")

	_cache.put_system(111, system1)
	assert_equal(_cache.get_cache_size(), 1, "Size should be 1")

	_cache.put_system(222, system2)
	assert_equal(_cache.get_cache_size(), 2, "Size should be 2")


func test_overwrite_same_key() -> void:
	var system1: SolarSystem = _solar_system.new("first", "First")
	var system2: SolarSystem = _solar_system.new("second", "Second")

	_cache.put_system(12345, system1)
	_cache.put_system(12345, system2)

	assert_equal(_cache.get_cache_size(), 1, "Size should still be 1")
	assert_equal(_cache.get_system(12345).name, "Second", "Should have second system")


func test_clear_empties_cache() -> void:
	var system: SolarSystem = _solar_system.new("c", "Clear Test")
	_cache.put_system(12345, system)
	_cache.put_system(67890, system)

	_cache.clear()

	assert_equal(_cache.get_cache_size(), 0, "Cache should be empty after clear")
	assert_false(_cache.has_system(12345), "Should not have system after clear")


func test_different_seeds_are_independent() -> void:
	var system1: SolarSystem = _solar_system.new("one", "System One")
	var system2: SolarSystem = _solar_system.new("two", "System Two")

	_cache.put_system(111, system1)
	_cache.put_system(222, system2)

	assert_equal(_cache.get_system(111).name, "System One", "First system correct")
	assert_equal(_cache.get_system(222).name, "System Two", "Second system correct")
