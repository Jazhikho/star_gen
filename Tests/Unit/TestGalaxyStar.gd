## Unit tests for GalaxyStar class.
class_name TestGalaxyStar
extends TestCase


func get_test_name() -> String:
	return "TestGalaxyStar"


func test_basic_creation() -> void:
	var star: GalaxyStar = GalaxyStar.new(Vector3(100.0, 50.0, 200.0), 12345)
	assert_equal(star.star_seed, 12345, "Seed should match")
	assert_true(star.position.is_equal_approx(Vector3(100.0, 50.0, 200.0)), "Position should match")
	assert_equal(star.metallicity, 1.0, "Default metallicity should be 1.0")
	assert_equal(star.age_bias, 1.0, "Default age bias should be 1.0")


func test_create_with_derived_properties() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(1000.0, 0.0, 0.0), 99999, spec
	)
	assert_equal(star.star_seed, 99999, "Seed should match")
	# Near center, metallicity should be elevated
	assert_greater_than(star.metallicity, 0.5, "Metallicity should be derived")
	assert_less_than(star.metallicity, 5.0, "Metallicity should be reasonable")


func test_metallicity_gradient_radial() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)

	# Star near galactic center
	var center_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(500.0, 0.0, 0.0), 1, spec
	)

	# Star at solar-neighborhood-equivalent distance (~8kpc)
	var solar_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(8000.0, 0.0, 0.0), 2, spec
	)

	# Star in outer disk
	var outer_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(15000.0, 0.0, 0.0), 3, spec
	)

	# Metallicity should decrease with radius
	assert_greater_than(center_star.metallicity, solar_star.metallicity,
		"Center star should have higher metallicity than solar-distance star")
	assert_greater_than(solar_star.metallicity, outer_star.metallicity,
		"Solar-distance star should have higher metallicity than outer star")


func test_metallicity_gradient_vertical() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)

	# Star in disk plane
	var disk_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(5000.0, 0.0, 0.0), 1, spec
	)

	# Star above disk (in halo)
	var halo_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(5000.0, 2000.0, 0.0), 2, spec
	)

	# Halo stars should have lower metallicity
	assert_greater_than(disk_star.metallicity, halo_star.metallicity,
		"Disk star should have higher metallicity than halo star")


func test_age_bias_bulge() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)

	# Star in bulge
	var bulge_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(500.0, 200.0, 0.0), 1, spec
	)

	# Star in outer disk
	var disk_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(10000.0, 0.0, 0.0), 2, spec
	)

	# Bulge stars should have higher age bias (older populations)
	assert_greater_than(bulge_star.age_bias, disk_star.age_bias,
		"Bulge star should have higher age bias than disk star")


func test_distance_helpers() -> void:
	var star: GalaxyStar = GalaxyStar.new(Vector3(3.0, 4.0, 0.0), 1)
	assert_equal(star.get_distance_from_center(), 5.0, "Distance from center should be 5")
	assert_equal(star.get_radial_distance(), 3.0, "Radial distance should be 3")
	assert_equal(star.get_height(), 4.0, "Height should be 4")


func test_to_string() -> void:
	var star: GalaxyStar = GalaxyStar.new(Vector3(100.0, 0.0, 0.0), 42)
	var s: String = star._to_string()
	assert_true(s.contains("42"), "String should contain seed")
