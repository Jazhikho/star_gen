## Integration tests for CelestialPersistence.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Test file path for persistence tests.
const TEST_FILE_PATH: String = "user://test_celestial_bodies/test_body.json"


## Clean up test file after each test.
func after_each() -> void:
	if FileAccess.file_exists(TEST_FILE_PATH):
		CelestialPersistence.delete_body(TEST_FILE_PATH)


## Creates a test body.
func _create_test_body() -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(5.972e24, 6.371e6, 86400.0, 23.5)
	var provenance: Provenance = Provenance.create_current(42)
	var body: CelestialBody = CelestialBody.new(
		"persistence_test_001",
		"Persistence Test Planet",
		CelestialType.Type.PLANET,
		physical,
		provenance
	)
	body.orbital = OrbitalProps.new(1.5e11, 0.02, 1.5)
	body.surface = SurfaceProps.new(288.0, 0.3, "rocky")
	return body


## Tests save and load round-trip.
func test_save_and_load() -> void:
	var original: CelestialBody = _create_test_body()

	var save_result: Error = CelestialPersistence.save_body(original, TEST_FILE_PATH)
	assert_equal(save_result, OK, "Save should succeed")

	var loaded: CelestialBody = CelestialPersistence.load_body(TEST_FILE_PATH)
	assert_not_null(loaded, "Load should return body")

	assert_equal(loaded.id, original.id)
	assert_equal(loaded.name, original.name)
	assert_equal(loaded.type, original.type)
	assert_float_equal(loaded.physical.mass_kg, original.physical.mass_kg)
	assert_float_equal(loaded.physical.radius_m, original.physical.radius_m)


## Tests load from non-existent file returns null.
func test_load_nonexistent_file() -> void:
	var body: CelestialBody = CelestialPersistence.load_body("user://does_not_exist.json")
	assert_null(body)


## Tests delete removes file.
func test_delete() -> void:
	var body: CelestialBody = _create_test_body()
	CelestialPersistence.save_body(body, TEST_FILE_PATH)

	assert_true(FileAccess.file_exists(TEST_FILE_PATH))

	var delete_result: Error = CelestialPersistence.delete_body(TEST_FILE_PATH)
	assert_equal(delete_result, OK)
	assert_false(FileAccess.file_exists(TEST_FILE_PATH))


## Tests default path generation.
func test_default_path() -> void:
	var body: CelestialBody = _create_test_body()
	var path: String = CelestialPersistence.get_default_path(body)

	assert_true(path.ends_with(".json"))
	assert_true(path.contains(body.id.to_lower()))


## Tests full persistence round-trip verifies all data.
func test_full_round_trip_integrity() -> void:
	var original: CelestialBody = _create_test_body()
	original.atmosphere = AtmosphereProps.new(
		101325.0, 8500.0, {"N2": 0.78, "O2": 0.21}, 1.0
	)
	original.surface.terrain = TerrainProps.new(15000.0, 0.5, 0.2, 0.6, 0.4, "mixed")
	
	var band: RingBand = RingBand.new(1.0e8, 2.0e8, 0.5, {"ice": 1.0}, 1.0, "Main")
	var bands: Array[RingBand] = [band]
	original.ring_system = RingSystemProps.new(bands, 1.0e17)

	CelestialPersistence.save_body(original, TEST_FILE_PATH)
	var loaded: CelestialBody = CelestialPersistence.load_body(TEST_FILE_PATH)

	assert_true(loaded.has_orbital())
	assert_true(loaded.has_surface())
	assert_true(loaded.has_atmosphere())
	assert_true(loaded.has_ring_system())

	assert_float_equal(loaded.orbital.semi_major_axis_m, original.orbital.semi_major_axis_m)
	assert_float_equal(loaded.orbital.eccentricity, original.orbital.eccentricity)

	assert_float_equal(loaded.surface.temperature_k, original.surface.temperature_k)
	assert_float_equal(loaded.surface.albedo, original.surface.albedo)

	assert_float_equal(loaded.atmosphere.surface_pressure_pa, original.atmosphere.surface_pressure_pa)
	assert_true(loaded.atmosphere.composition.has("N2"))
	
	# Verify terrain
	assert_true(loaded.surface.has_terrain())
	assert_float_equal(loaded.surface.terrain.elevation_range_m, original.surface.terrain.elevation_range_m)
	
	# Verify ring system
	assert_equal(loaded.ring_system.get_band_count(), 1)
	assert_equal(loaded.ring_system.get_band(0).name, "Main")

	assert_equal(loaded.provenance.generation_seed, original.provenance.generation_seed)
