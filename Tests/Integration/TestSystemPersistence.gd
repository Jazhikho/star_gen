## Integration tests for SystemPersistence.
extends TestCase

const _system_persistence: GDScript = preload("res://src/services/persistence/SystemPersistence.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _asteroid_belt: GDScript = preload("res://src/domain/system/AsteroidBelt.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Test file paths
var _test_json_path: String = "user://test_system.json"
var _test_binary_path: String = "user://test_system.sgs"


## Creates a test system.
func _create_test_system() -> SolarSystem:
	var system: SolarSystem = SolarSystem.new("test_persist", "Persistence Test")
	
	var star_spec: StarSpec = StarSpec.sun_like(99999)
	var star_rng: SeededRng = SeededRng.new(99999)
	var star: CelestialBody = StarGenerator.generate(star_spec, star_rng)
	star.id = "star_1"
	system.add_body(star)
	
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star", "star_1")
	system.hierarchy = SystemHierarchy.new(star_node)
	
	var host: OrbitHost = OrbitHost.new("node_star", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = star.physical.mass_kg
	system.add_orbit_host(host)
	
	return system


## Cleans up test files after each test.
func after_each() -> void:
	if FileAccess.file_exists(_test_json_path):
		DirAccess.remove_absolute(_test_json_path)
	if FileAccess.file_exists(_test_binary_path):
		DirAccess.remove_absolute(_test_binary_path)


## Tests save and load JSON.
func test_save_load_json() -> void:
	var original: SolarSystem = _create_test_system()
	
	var save_error: Error = SystemPersistence.save(original, _test_json_path, false)
	assert_equal(save_error, OK, "Save should succeed")
	
	var result: SystemPersistence.LoadResult = SystemPersistence.load(_test_json_path)
	
	assert_true(result.success, "Load should succeed")
	assert_not_null(result.system)
	assert_equal(result.system.id, original.id)


## Tests save and load compressed binary.
func test_save_load_binary() -> void:
	var original: SolarSystem = _create_test_system()
	
	var save_error: Error = SystemPersistence.save(original, _test_binary_path, true)
	assert_equal(save_error, OK, "Save should succeed")
	
	var result: SystemPersistence.LoadResult = SystemPersistence.load(_test_binary_path)
	
	assert_true(result.success, "Load should succeed")
	assert_not_null(result.system)
	assert_equal(result.system.id, original.id)


## Tests compression reduces file size.
func test_compression_reduces_size() -> void:
	var system: SolarSystem = _create_test_system()
	
	SystemPersistence.save(system, _test_json_path, false)
	SystemPersistence.save(system, _test_binary_path, true)
	
	var json_size: int = SystemPersistence.get_file_size(_test_json_path)
	var binary_size: int = SystemPersistence.get_file_size(_test_binary_path)
	
	assert_greater_than(json_size, 0, "JSON file should have size")
	assert_greater_than(binary_size, 0, "Binary file should have size")
	assert_less_than(binary_size, json_size, "Compressed should be smaller")


## Tests load nonexistent file.
func test_load_nonexistent() -> void:
	var result: SystemPersistence.LoadResult = SystemPersistence.load("user://nonexistent.sgs")
	
	assert_false(result.success)
	assert_true(result.error_message.contains("not found"))


## Tests format_file_size.
func test_format_file_size() -> void:
	assert_equal(SystemPersistence.format_file_size(500), "500 B")
	assert_equal(SystemPersistence.format_file_size(1500), "1.5 KB")
	assert_equal(SystemPersistence.format_file_size(1500000), "1.4 MB")


## Tests round-trip preserves all data.
func test_round_trip_full_data() -> void:
	var original: SolarSystem = _create_test_system()
	
	# Add more data
	var planet: CelestialBody = CelestialBody.new("planet_1", "Test Planet", CelestialType.Type.PLANET)
	planet.physical = PhysicalProps.new(Units.EARTH_MASS_KG, Units.EARTH_RADIUS_METERS, 86400.0)
	planet.orbital = OrbitalProps.new(Units.AU_METERS, 0.0, 0.0, 0.0, 0.0, 0.0, "node_star")
	original.add_body(planet)
	
	var belt: AsteroidBelt = AsteroidBelt.new("belt_1", "Test Belt")
	belt.inner_radius_m = 2.0 * Units.AU_METERS
	belt.outer_radius_m = 3.0 * Units.AU_METERS
	original.add_asteroid_belt(belt)
	
	# Save and load
	SystemPersistence.save(original, _test_binary_path, true)
	var result: SystemPersistence.LoadResult = SystemPersistence.load(_test_binary_path)
	
	assert_true(result.success)
	assert_equal(result.system.planet_ids.size(), 1)
	assert_equal(result.system.asteroid_belts.size(), 1)
