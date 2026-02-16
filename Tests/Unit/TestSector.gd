## Unit tests for Sector class.
class_name TestSector
extends TestCase


func get_test_name() -> String:
	return "TestSector"


func test_sector_creation() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = Sector.new(galaxy, Vector3i(1, 0, 2), Vector3i(5, 5, 5))

	assert_equal(sector.quadrant_coords, Vector3i(1, 0, 2), "Quadrant coords should match")
	assert_equal(sector.sector_local_coords, Vector3i(5, 5, 5), "Sector coords should match")
	assert_false(sector.is_generated(), "Should not be generated initially")


func test_sector_world_origin() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = Sector.new(galaxy, Vector3i(0, 0, 0), Vector3i(3, 4, 5))

	var expected: Vector3 = GalaxyCoordinates.sector_world_origin(Vector3i(0, 0, 0), Vector3i(3, 4, 5))
	assert_true(sector.world_origin.is_equal_approx(expected), "World origin should match")


func test_get_stars_triggers_generation() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = Sector.new(galaxy, Vector3i(0, 0, 0), Vector3i(0, 0, 0))

	assert_false(sector.is_generated(), "Should not be generated before get_stars")
	var _stars: Array[GalaxyStar] = sector.get_stars()
	assert_true(sector.is_generated(), "Should be generated after get_stars")


func test_get_stars_deterministic() -> void:
	var galaxy_a: Galaxy = Galaxy.create_default(42)
	var galaxy_b: Galaxy = Galaxy.create_default(42)

	var sector_a: Sector = Sector.new(galaxy_a, Vector3i(0, 0, 0), Vector3i(5, 5, 5))
	var sector_b: Sector = Sector.new(galaxy_b, Vector3i(0, 0, 0), Vector3i(5, 5, 5))

	var stars_a: Array[GalaxyStar] = sector_a.get_stars()
	var stars_b: Array[GalaxyStar] = sector_b.get_stars()

	assert_equal(stars_a.size(), stars_b.size(), "Same inputs should give same star count")
	if stars_a.size() > 0:
		assert_equal(stars_a[0].star_seed, stars_b[0].star_seed, "First star seed should match")


func test_get_star_count() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = Sector.new(galaxy, Vector3i(0, 0, 0), Vector3i(0, 0, 0))

	var stars: Array[GalaxyStar] = sector.get_stars()
	assert_equal(sector.get_star_count(), stars.size(), "Star count should match array size")


func test_get_stars_in_subsector() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = Sector.new(galaxy, Vector3i(0, 0, 0), Vector3i(0, 0, 0))

	var all_stars: Array[GalaxyStar] = sector.get_stars()

	# Sum stars from all subsectors should equal total
	var subsector_total: int = 0
	for ssx in range(10):
		for ssy in range(10):
			for ssz in range(10):
				var ss_stars: Array[GalaxyStar] = sector.get_stars_in_subsector(Vector3i(ssx, ssy, ssz))
				subsector_total += ss_stars.size()

	assert_equal(subsector_total, all_stars.size(), "Sum of subsector stars should equal total")


func test_stars_have_correct_subsector_coords() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = Sector.new(galaxy, Vector3i(0, 0, 0), Vector3i(5, 5, 5))

	var stars: Array[GalaxyStar] = sector.get_stars_in_subsector(Vector3i(3, 3, 3))
	for star in stars:
		assert_equal(star.subsector_coords, Vector3i(3, 3, 3), "Star subsector coords should match")


func test_regenerate_clears_and_regenerates() -> void:
	var galaxy: Galaxy = Galaxy.create_default(42)
	var sector: Sector = Sector.new(galaxy, Vector3i(0, 0, 0), Vector3i(5, 5, 5))

	var stars_before: Array[GalaxyStar] = sector.get_stars()
	var count_before: int = stars_before.size()

	sector.regenerate()

	var stars_after: Array[GalaxyStar] = sector.get_stars()
	assert_equal(stars_after.size(), count_before, "Regeneration should produce same count")


func test_sector_seed_is_deterministic() -> void:
	var galaxy_a: Galaxy = Galaxy.create_default(42)
	var galaxy_b: Galaxy = Galaxy.create_default(42)

	var sector_a: Sector = Sector.new(galaxy_a, Vector3i(1, 2, 3), Vector3i(4, 5, 6))
	var sector_b: Sector = Sector.new(galaxy_b, Vector3i(1, 2, 3), Vector3i(4, 5, 6))

	assert_equal(sector_a.sector_seed, sector_b.sector_seed, "Same inputs should give same sector seed")

	var sector_c: Sector = Sector.new(galaxy_a, Vector3i(1, 2, 3), Vector3i(7, 8, 9))
	assert_not_equal(sector_a.sector_seed, sector_c.sector_seed, "Different coords should give different seed")
