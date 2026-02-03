## Integration tests for the save/load system.
extends TestCase

const _save_data: GDScript = preload("res://src/services/persistence/SaveData.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _asteroid_generator: GDScript = preload("res://src/domain/generation/generators/AsteroidGenerator.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")

var _test_dir: String = "user://test_saves/"


func _init() -> void:
	# Ensure test directory exists
	DirAccess.make_dir_recursive_absolute(_test_dir)


## Cleans up test files after each test
func _cleanup_test_files() -> void:
	var dir: DirAccess = DirAccess.open(_test_dir)
	if dir:
		dir.list_dir_begin()
		var file_name: String = dir.get_next()
		while file_name != "":
			if not dir.current_is_dir():
				dir.remove(file_name)
			file_name = dir.get_next()
		dir.list_dir_end()


## Creates a test star
func _create_test_star(seed_val: int) -> CelestialBody:
	var spec: StarSpec = StarSpec.random(seed_val)
	var rng: SeededRng = SeededRng.new(seed_val)
	return StarGenerator.generate(spec, rng)


## Creates a test planet
func _create_test_planet(seed_val: int) -> CelestialBody:
	var spec: PlanetSpec = PlanetSpec.random(seed_val)
	var context: ParentContext = ParentContext.sun_like()
	var rng: SeededRng = SeededRng.new(seed_val)
	return PlanetGenerator.generate(spec, context, rng)


## Creates a test moon
func _create_test_moon(seed_val: int) -> CelestialBody:
	var spec: MoonSpec = MoonSpec.random(seed_val)
	var context: ParentContext = ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		3.828e26,  # SOLAR_LUMINOSITY_WATTS
		5778.0,
		4.6e9,
		5.2 * Units.AU_METERS,
		1.898e27,
		6.9911e7,
		5.0e8
	)
	var rng: SeededRng = SeededRng.new(seed_val)
	return MoonGenerator.generate(spec, context, rng)


## Creates a test asteroid
func _create_test_asteroid(seed_val: int) -> CelestialBody:
	var spec: AsteroidSpec = AsteroidSpec.random(seed_val)
	var context: ParentContext = ParentContext.sun_like(2.7 * Units.AU_METERS)
	var rng: SeededRng = SeededRng.new(seed_val)
	return AsteroidGenerator.generate(spec, context, rng)


## Tests saving and loading a star (compressed).
func test_save_and_load_star_compressed() -> void:
	var original: CelestialBody = _create_test_star(12345)
	var path: String = _test_dir + "test_star.sgb"
	
	# Save
	var save_error: Error = SaveData.save_body(original, path, SaveData.SaveMode.COMPACT, true)
	assert_equal(save_error, OK, "Should save without error")
	
	# Load
	var result: SaveData.LoadResult = SaveData.load_body(path)
	assert_true(result.success, "Should load successfully: " + result.error_message)
	assert_not_null(result.body, "Should have loaded body")
	
	# Verify
	assert_equal(result.body.type, CelestialType.Type.STAR, "Should be a star")
	assert_equal(result.body.physical.mass_kg, original.physical.mass_kg, "Mass should match")
	assert_equal(result.body.physical.radius_m, original.physical.radius_m, "Radius should match")
	
	_cleanup_test_files()


## Tests saving and loading a planet (JSON).
func test_save_and_load_planet_json() -> void:
	var original: CelestialBody = _create_test_planet(23456)
	var path: String = _test_dir + "test_planet.json"
	
	# Save as JSON
	var save_error: Error = SaveData.save_body(original, path, SaveData.SaveMode.COMPACT, false)
	assert_equal(save_error, OK, "Should save without error")
	
	# Load
	var result: SaveData.LoadResult = SaveData.load_body(path)
	assert_true(result.success, "Should load successfully: " + result.error_message)
	assert_not_null(result.body, "Should have loaded body")
	
	# Verify
	assert_equal(result.body.type, CelestialType.Type.PLANET, "Should be a planet")
	assert_equal(result.body.physical.mass_kg, original.physical.mass_kg, "Mass should match")
	
	_cleanup_test_files()


## Tests saving and loading a moon.
func test_save_and_load_moon() -> void:
	var original: CelestialBody = _create_test_moon(34567)
	var path: String = _test_dir + "test_moon.sgb"
	
	var save_error: Error = SaveData.save_body(original, path)
	assert_equal(save_error, OK, "Should save without error")
	
	var result: SaveData.LoadResult = SaveData.load_body(path)
	assert_true(result.success, "Should load successfully: " + result.error_message)
	assert_equal(result.body.type, CelestialType.Type.MOON, "Should be a moon")
	assert_equal(result.body.physical.mass_kg, original.physical.mass_kg, "Mass should match")
	
	_cleanup_test_files()


## Tests saving and loading an asteroid.
func test_save_and_load_asteroid() -> void:
	var original: CelestialBody = _create_test_asteroid(45678)
	var path: String = _test_dir + "test_asteroid.sgb"
	
	var save_error: Error = SaveData.save_body(original, path)
	assert_equal(save_error, OK, "Should save without error")
	
	var result: SaveData.LoadResult = SaveData.load_body(path)
	assert_true(result.success, "Should load successfully: " + result.error_message)
	assert_equal(result.body.type, CelestialType.Type.ASTEROID, "Should be an asteroid")
	
	_cleanup_test_files()


## Tests loading invalid file fails gracefully.
func test_load_invalid_file_fails_gracefully() -> void:
	var result: SaveData.LoadResult = SaveData.load_body(_test_dir + "nonexistent.sgb")
	
	assert_false(result.success, "Should fail to load nonexistent file")
	assert_false(result.error_message.is_empty(), "Should have error message")
	assert_null(result.body, "Should not have body")


## Tests loading invalid JSON fails gracefully.
func test_load_invalid_json_fails_gracefully() -> void:
	# Create an invalid JSON file
	var path: String = _test_dir + "invalid.json"
	var file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	file.store_string("{ this is not valid json }")
	file.close()
	
	var result: SaveData.LoadResult = SaveData.load_body(path)
	
	assert_false(result.success, "Should fail to load invalid JSON")
	assert_true(result.error_message.contains("Invalid JSON"), "Error should mention invalid JSON")
	
	_cleanup_test_files()


## Tests loading wrong format fails gracefully.
func test_load_wrong_format_fails_gracefully() -> void:
	# Create a file that's not a StarGen save
	var path: String = _test_dir + "wrong_format.sgb"
	var file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	file.store_string("NOT A STARGEN FILE")
	file.close()
	
	var result: SaveData.LoadResult = SaveData.load_body(path)
	
	assert_false(result.success, "Should fail to load wrong format")
	assert_true(result.error_message.contains("Invalid file format"), "Error should mention format")
	
	_cleanup_test_files()


## Tests compressed file is smaller than JSON.
func test_compressed_file_is_smaller() -> void:
	var body: CelestialBody = _create_test_planet(56789)
	var json_path: String = _test_dir + "size_test.json"
	var compressed_path: String = _test_dir + "size_test.sgb"
	
	SaveData.save_body(body, json_path, SaveData.SaveMode.COMPACT, false)
	SaveData.save_body(body, compressed_path, SaveData.SaveMode.COMPACT, true)
	
	var json_size: int = SaveData.get_file_size(json_path)
	var compressed_size: int = SaveData.get_file_size(compressed_path)
	
	assert_true(compressed_size < json_size, 
		"Compressed (%d bytes) should be smaller than JSON (%d bytes)" % [compressed_size, json_size])
	
	_cleanup_test_files()


## Tests full save mode.
func test_full_save_mode() -> void:
	var original: CelestialBody = _create_test_planet(67890)
	var path: String = _test_dir + "test_full.sgb"
	
	# Save in FULL mode
	var save_error: Error = SaveData.save_body(original, path, SaveData.SaveMode.FULL, true)
	assert_equal(save_error, OK, "Should save without error")
	
	# Load
	var result: SaveData.LoadResult = SaveData.load_body(path)
	assert_true(result.success, "Should load successfully")
	assert_equal(result.body.type, original.type, "Type should match")
	assert_equal(result.body.id, original.id, "ID should match")
	
	_cleanup_test_files()


## Tests roundtrip preserves provenance.
func test_roundtrip_preserves_provenance() -> void:
	var original: CelestialBody = _create_test_star(78901)
	var path: String = _test_dir + "test_provenance.sgb"
	
	SaveData.save_body(original, path)
	var result: SaveData.LoadResult = SaveData.load_body(path)
	
	assert_true(result.success, "Should load successfully")
	assert_not_null(result.body.provenance, "Should have provenance")
	assert_equal(result.body.provenance.generation_seed, original.provenance.generation_seed, 
		"Seed should be preserved")
	
	_cleanup_test_files()


## Tests saving null body fails.
func test_save_null_body_fails() -> void:
	var path: String = _test_dir + "test_null.sgb"
	var error: Error = SaveData.save_body(null, path)
	
	assert_not_equal(error, OK, "Should fail to save null body")


## Tests file size formatting.
func test_file_size_formatting() -> void:
	# Test bytes
	var bytes_str: String = SaveData.format_file_size(512)
	assert_true(bytes_str.contains("512"), "Should format bytes correctly")
	
	# Test KB
	var kb_str: String = SaveData.format_file_size(2048)
	assert_true(kb_str.contains("KB"), "Should format KB correctly")
	
	# Test MB
	var mb_str: String = SaveData.format_file_size(2 * 1024 * 1024)
	assert_true(mb_str.contains("MB"), "Should format MB correctly")
