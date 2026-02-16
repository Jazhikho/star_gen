## Unit tests for Galaxy class.
class_name TestGalaxy
extends TestCase


func get_test_name() -> String:
	return "TestGalaxy"


func test_create_default() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	assert_not_null(galaxy, "Should create galaxy")
	assert_equal(galaxy.seed, 42, "Seed should match")
	assert_not_null(galaxy.spec, "Should have spec")
	assert_not_null(galaxy.config, "Should have config")
	assert_not_null(galaxy.density_model, "Should have density model")
	assert_greater_than(galaxy.reference_density, 0.0, "Reference density should be positive")


func test_create_with_config() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_milky_way()
	config.num_arms = 2
	var galaxy: Galaxy = Galaxy.new(config, 123)
	assert_equal(galaxy.seed, 123, "Seed should match")
	assert_equal(galaxy.config.num_arms, 2, "Config should be applied")


func test_get_sector_creates_sector() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = galaxy.get_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	assert_not_null(sector, "Should create sector")
	assert_equal(sector.quadrant_coords, Vector3i(0, 0, 0), "Quadrant coords should match")
	assert_equal(sector.sector_local_coords, Vector3i(5, 5, 5), "Sector coords should match")


func test_get_sector_caches_sector() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector_a: Sector = galaxy.get_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	var sector_b: Sector = galaxy.get_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	assert_equal(sector_a, sector_b, "Should return same cached sector")
	assert_equal(galaxy.get_cached_sector_count(), 1, "Should have one cached sector")


func test_get_sector_different_coords_different_sectors() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector_a: Sector = galaxy.get_sector(Vector3i(0, 0, 0), Vector3i(0, 0, 0))
	var sector_b: Sector = galaxy.get_sector(Vector3i(0, 0, 0), Vector3i(1, 0, 0))
	assert_not_equal(sector_a, sector_b, "Different coords should give different sectors")
	assert_equal(galaxy.get_cached_sector_count(), 2, "Should have two cached sectors")


func test_get_sector_at_position() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	# Position in the middle of sector (0,0,0):(5,5,5) in quadrant (0,0,0)
	var pos: Vector3 = Vector3(550.0, 550.0, 550.0)
	var sector: Sector = galaxy.get_sector_at_position(pos)
	assert_not_null(sector, "Should find sector")
	assert_equal(sector.quadrant_coords, Vector3i(0, 0, 0), "Should be in quadrant 0")
	assert_equal(sector.sector_local_coords, Vector3i(5, 5, 5), "Should be in sector 5,5,5")


func test_get_stars_in_sector() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var stars: Array[GalaxyStar] = galaxy.get_stars_in_sector(Vector3i(0, 0, 0), Vector3i(0, 0, 0))
	# Near galactic center, should have stars
	assert_greater_than(stars.size(), 0, "Should have stars near center")


func test_get_stars_in_sector_deterministic() -> void:
	var galaxy_a: Galaxy = Galaxy.create_default(42)
	var galaxy_b: Galaxy = Galaxy.create_default(42)
	var stars_a: Array[GalaxyStar] = galaxy_a.get_stars_in_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	var stars_b: Array[GalaxyStar] = galaxy_b.get_stars_in_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	assert_equal(stars_a.size(), stars_b.size(), "Same seed should give same star count")
	if stars_a.size() > 0:
		assert_equal(stars_a[0].star_seed, stars_b[0].star_seed, "Same seed should give same star seeds")
		assert_true(stars_a[0].position.is_equal_approx(stars_b[0].position), "Same seed should give same positions")


func test_get_stars_in_subsector() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var stars: Array[GalaxyStar] = galaxy.get_stars_in_subsector(
		Vector3i(0, 0, 0), Vector3i(0, 0, 0), Vector3i(5, 5, 5)
	)
	# May or may not have stars, but should not error
	assert_true(stars != null, "Should return array (possibly empty)")


func test_cache_system() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var mock_system: SolarSystem = SolarSystem.new("test_id", "Test System")
	galaxy.cache_system(12345, mock_system)
	assert_true(galaxy.has_cached_system(12345), "Should have cached system")
	assert_equal(galaxy.get_cached_system(12345), mock_system, "Should return cached system")
	assert_equal(galaxy.get_cached_system_count(), 1, "Should have one cached system")


func test_get_cached_system_returns_null_for_missing() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	assert_null(galaxy.get_cached_system(99999), "Should return null for uncached seed")
	assert_false(galaxy.has_cached_system(99999), "Should report not cached")


func test_clear_cache() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var _sector: Sector = galaxy.get_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	var mock_system: SolarSystem = SolarSystem.new("test_id", "Test System")
	galaxy.cache_system(12345, mock_system)
	assert_equal(galaxy.get_cached_sector_count(), 1, "Should have cached sector")
	assert_equal(galaxy.get_cached_system_count(), 1, "Should have cached system")

	galaxy.clear_cache()
	assert_equal(galaxy.get_cached_sector_count(), 0, "Should clear sectors")
	assert_equal(galaxy.get_cached_system_count(), 0, "Should clear systems")


func test_to_dict_and_from_dict() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_milky_way()
	config.num_arms = 3
	var galaxy: Galaxy = Galaxy.new(config, 999)

	var dict: Dictionary = galaxy.to_dict()
	assert_equal(dict["seed"], 999, "Dict should contain seed")
	assert_true(dict.has("config"), "Dict should contain config")

	var restored: Galaxy = Galaxy.from_dict(dict)
	assert_equal(restored.seed, 999, "Restored seed should match")
	assert_equal(restored.config.num_arms, 3, "Restored config should match")


func test_get_stars_in_radius() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	# Center and radius chosen so the bounding box stays inside one quadrant and one sector
	# (avoids slow multi-sector generation). Sector = 100pc, quadrant = 1000pc; center (50,50,50) radius 30 → one sector.
	var center: Vector3 = Vector3(50.0, 50.0, 50.0)
	var radius_pc: float = 30.0
	var stars: Array[GalaxyStar] = galaxy.get_stars_in_radius(center, radius_pc)
	# All returned stars should be within radius
	for star in stars:
		var dist: float = star.position.distance_to(center)
		assert_less_than(dist, radius_pc + 0.1, "Star should be within radius")


func test_different_galaxy_seeds_different_stars() -> void:
	var galaxy_a: Galaxy = Galaxy.create_default(42)
	var galaxy_b: Galaxy = Galaxy.create_default(999)
	var stars_a: Array[GalaxyStar] = galaxy_a.get_stars_in_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	var stars_b: Array[GalaxyStar] = galaxy_b.get_stars_in_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5))

	# Different seeds should produce different results
	var any_different: bool = stars_a.size() != stars_b.size()
	if not any_different and stars_a.size() > 0:
		any_different = stars_a[0].star_seed != stars_b[0].star_seed
	assert_true(any_different, "Different galaxy seeds should give different stars")


func test_stars_have_metallicity() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var stars: Array[GalaxyStar] = galaxy.get_stars_in_sector(Vector3i(0, 0, 0), Vector3i(0, 0, 0))
	if stars.size() > 0:
		var star: GalaxyStar = stars[0]
		assert_greater_than(star.metallicity, 0.0, "Metallicity should be positive")
		assert_less_than(star.metallicity, 10.0, "Metallicity should be reasonable")
		assert_greater_than(star.age_bias, 0.0, "Age bias should be positive")
