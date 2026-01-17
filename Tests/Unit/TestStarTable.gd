## Tests for StarTable lookups and calculations.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests mass range retrieval.
func test_mass_ranges() -> void:
	var g_range: Dictionary = StarTable.get_mass_range(StarClass.SpectralClass.G)
	assert_in_range(1.0, g_range["min"], g_range["max"])  # Sun should fit
	
	var m_range: Dictionary = StarTable.get_mass_range(StarClass.SpectralClass.M)
	assert_less_than(m_range["max"], g_range["min"])  # M stars smaller than G


## Tests luminosity from mass relationship.
func test_luminosity_from_mass() -> void:
	# Sun (1 solar mass) should give ~1 solar luminosity
	var l_sun: float = StarTable.luminosity_from_mass(1.0)
	assert_float_equal(l_sun, 1.0)
	
	# 2 solar masses -> ~11 solar luminosities (2^3.5)
	var l_2: float = StarTable.luminosity_from_mass(2.0)
	assert_in_range(l_2, 10.0, 12.0)


## Tests radius from mass relationship.
func test_radius_from_mass() -> void:
	# Sun (1 solar mass) should give ~1 solar radius
	var r_sun: float = StarTable.radius_from_mass(1.0)
	assert_float_equal(r_sun, 1.0)


## Tests temperature calculation.
func test_temperature_from_luminosity_radius() -> void:
	# Sun: L=1, R=1 -> T ~5778 K
	var t_sun: float = StarTable.temperature_from_luminosity_radius(1.0, 1.0)
	assert_in_range(t_sun, 5700.0, 5850.0)


## Tests class determination from temperature.
func test_class_from_temperature() -> void:
	assert_equal(StarTable.class_from_temperature(5778.0), StarClass.SpectralClass.G)
	assert_equal(StarTable.class_from_temperature(3500.0), StarClass.SpectralClass.M)
	assert_equal(StarTable.class_from_temperature(10000.0), StarClass.SpectralClass.A)
	assert_equal(StarTable.class_from_temperature(35000.0), StarClass.SpectralClass.O)


## Tests subclass interpolation.
func test_subclass_interpolation() -> void:
	var temp_range: Dictionary = StarTable.get_temperature_range(StarClass.SpectralClass.G)
	
	# Subclass 0 should be near max (hottest in class)
	var t0: float = StarTable.interpolate_by_subclass(StarClass.SpectralClass.G, 0, temp_range)
	assert_float_equal(t0, temp_range["max"], 1.0)
	
	# Subclass 9 should be near min (coolest in class)
	var t9: float = StarTable.interpolate_by_subclass(StarClass.SpectralClass.G, 9, temp_range)
	assert_float_equal(t9, temp_range["min"], 1.0)


## Tests lifetime ranges are physically reasonable.
func test_lifetime_ranges() -> void:
	# O stars should have shorter lifetimes than M stars
	var o_life: Dictionary = StarTable.get_lifetime_range(StarClass.SpectralClass.O)
	var m_life: Dictionary = StarTable.get_lifetime_range(StarClass.SpectralClass.M)
	
	assert_less_than(o_life["max"], m_life["min"])
