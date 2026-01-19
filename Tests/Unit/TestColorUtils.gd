## Unit tests for ColorUtils.
extends TestCase

const _color_utils := preload("res://src/app/rendering/ColorUtils.gd")


func test_blackbody_hot_star_is_blue() -> void:
	# O-class star ~30000K should be bluish
	var color: Color = ColorUtils.temperature_to_blackbody_color(30000.0)
	
	assert_true(color.b > color.r, "Hot star should have more blue than red")
	assert_true(color.b > 0.8, "Hot star should have high blue component")


func test_blackbody_solar_is_yellow_white() -> void:
	# G-class star ~5778K should be yellowish-white
	var color: Color = ColorUtils.temperature_to_blackbody_color(5778.0)
	
	# Solar color should be fairly balanced with slight yellow
	assert_true(color.r > 0.9, "Solar temperature should have high red")
	assert_true(color.g > 0.9, "Solar temperature should have high green")
	assert_true(color.b > 0.7, "Solar temperature should have moderate blue")


func test_blackbody_cool_star_is_red() -> void:
	# M-class star ~3000K should be reddish
	var color: Color = ColorUtils.temperature_to_blackbody_color(3000.0)
	
	assert_true(color.r > color.b, "Cool star should have more red than blue")
	assert_true(color.r > 0.9, "Cool star should have high red component")


func test_blackbody_temperature_gradient() -> void:
	# Colors should form a gradient from red to blue as temperature increases
	var color_3000: Color = ColorUtils.temperature_to_blackbody_color(3000.0)
	var color_6000: Color = ColorUtils.temperature_to_blackbody_color(6000.0)
	var color_10000: Color = ColorUtils.temperature_to_blackbody_color(10000.0)
	var color_20000: Color = ColorUtils.temperature_to_blackbody_color(20000.0)
	
	# Blue component should increase with temperature
	assert_true(color_6000.b > color_3000.b, "6000K should be bluer than 3000K")
	assert_true(color_10000.b > color_6000.b, "10000K should be bluer than 6000K")
	assert_true(color_20000.b >= color_10000.b, "20000K should be at least as blue as 10000K")


func test_spectral_class_colors() -> void:
	# Test that different spectral classes produce different colors
	var o_color: Color = ColorUtils.spectral_class_to_color("O5V")
	var g_color: Color = ColorUtils.spectral_class_to_color("G2V")
	var m_color: Color = ColorUtils.spectral_class_to_color("M5V")
	
	# O should be bluest
	assert_true(o_color.b > g_color.b, "O-class should be bluer than G-class")
	
	# M should be reddest
	assert_true(m_color.r > m_color.b, "M-class should be redder than blue")
	assert_true(m_color.r > g_color.r * 0.9, "M-class should have high red")


func test_atmosphere_nitrogen_is_blue() -> void:
	var composition: Dictionary = {"N2": 0.78, "O2": 0.21, "Ar": 0.01}
	var color: Color = ColorUtils.atmosphere_to_sky_color(composition)
	
	assert_true(color.b > color.r, "N2/O2 atmosphere should be bluish")


func test_atmosphere_co2_is_orange() -> void:
	var composition: Dictionary = {"CO2": 0.96, "N2": 0.03}
	var color: Color = ColorUtils.atmosphere_to_sky_color(composition)
	
	assert_true(color.r > color.b, "CO2 atmosphere should be more orange/red")


func test_atmosphere_methane_is_cyan() -> void:
	var composition: Dictionary = {"N2": 0.95, "CH4": 0.05}
	var color: Color = ColorUtils.atmosphere_to_sky_color(composition)
	
	# Methane scatters blue light, should be bluish
	assert_true(color.b > 0.5, "Methane-containing atmosphere should have blue")


func test_surface_molten_is_red_orange() -> void:
	var color: Color = ColorUtils.surface_to_color("molten", {}, 0.1)
	
	assert_true(color.r > color.b, "Molten surface should be reddish")
	assert_true(color.r > 0.7, "Molten surface should have high red")


func test_surface_icy_is_white_blue() -> void:
	var color: Color = ColorUtils.surface_to_color("icy", {"water_ice": 0.9}, 0.8)
	
	assert_true(color.b > 0.7, "Icy surface should have high blue")
	assert_true(color.r > 0.7, "Icy surface should have high red (white)")


func test_surface_rocky_is_gray_brown() -> void:
	var color: Color = ColorUtils.surface_to_color("rocky", {"silicates": 0.7}, 0.3)
	
	# Rocky should be neutral gray-ish
	var max_diff: float = maxf(absf(color.r - color.g), absf(color.g - color.b))
	assert_true(max_diff < 0.3, "Rocky surface should be fairly neutral in color")


func test_asteroid_carbonaceous_is_dark() -> void:
	var color: Color = ColorUtils.asteroid_to_color("carbonaceous", {"carbon_compounds": 0.3})
	
	# C-type asteroids are very dark
	assert_true(color.r < 0.3, "Carbonaceous asteroid should be dark")
	assert_true(color.g < 0.3, "Carbonaceous asteroid should be dark")
	assert_true(color.b < 0.3, "Carbonaceous asteroid should be dark")


func test_asteroid_metallic_is_gray() -> void:
	var color: Color = ColorUtils.asteroid_to_color("metallic", {"iron": 0.8, "nickel": 0.15})
	
	# M-type asteroids have metallic gray color
	assert_true(color.r > 0.4, "Metallic asteroid should be lighter than carbonaceous")


func test_ring_icy_is_bright() -> void:
	var color: Color = ColorUtils.ring_to_color({"water_ice": 0.95}, 0.5)
	
	assert_true(color.r > 0.8, "Icy ring should be bright")
	assert_true(color.b > 0.8, "Icy ring should be bright with blue tint")
	assert_float_equal(color.a, 0.5, 0.01, "Alpha should match optical depth")


func test_ring_optical_depth_affects_alpha() -> void:
	var thin_ring: Color = ColorUtils.ring_to_color({"water_ice": 0.9}, 0.1)
	var thick_ring: Color = ColorUtils.ring_to_color({"water_ice": 0.9}, 0.8)
	
	assert_true(thick_ring.a > thin_ring.a, "Thicker ring should have higher alpha")
