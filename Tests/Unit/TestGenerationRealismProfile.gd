## Tests for GenerationRealismProfile: slider mapping, mode enum, determinism of profile choice.
class_name TestGenerationRealismProfile
extends TestCase

const _profile_script: GDScript = preload("res://src/domain/generation/GenerationRealismProfile.gd")


func _mode_stylized() -> int:
	return _profile_script.get_script_constant_map().get("Mode", {}).get("STYLIZED", 2)


func _mode_calibrated() -> int:
	return _profile_script.get_script_constant_map().get("Mode", {}).get("CALIBRATED", 0)


func _mode_balanced() -> int:
	return _profile_script.get_script_constant_map().get("Mode", {}).get("BALANCED", 1)


func test_from_slider_zero_is_stylized() -> void:
	var profile: RefCounted = _profile_script.call("from_slider", 0.0)
	assert_equal(profile.mode, _mode_stylized(), "Slider 0 should be STYLIZED")
	assert_equal(profile.realism_slider, 0.0, "Slider value should be 0")


func test_from_slider_one_is_calibrated() -> void:
	var profile: RefCounted = _profile_script.call("from_slider", 1.0)
	assert_equal(profile.mode, _mode_calibrated(), "Slider 1 should be CALIBRATED")
	assert_equal(profile.realism_slider, 1.0, "Slider value should be 1")


func test_from_slider_mid_is_balanced() -> void:
	var profile: RefCounted = _profile_script.call("from_slider", 0.5)
	assert_equal(profile.mode, _mode_balanced(), "Slider 0.5 should be BALANCED")


func test_from_slider_clamped() -> void:
	var low: RefCounted = _profile_script.call("from_slider", -0.1)
	var high: RefCounted = _profile_script.call("from_slider", 1.5)
	assert_equal(low.realism_slider, 0.0, "Negative slider should clamp to 0")
	assert_equal(high.realism_slider, 1.0, "Slider > 1 should clamp to 1")


func test_static_factories() -> void:
	var cal: RefCounted = _profile_script.call("calibrated")
	var bal: RefCounted = _profile_script.call("balanced")
	var sty: RefCounted = _profile_script.call("stylized")
	assert_equal(cal.mode, _mode_calibrated(), "calibrated() should return CALIBRATED")
	assert_equal(bal.mode, _mode_balanced(), "balanced() should return BALANCED")
	assert_equal(sty.mode, _mode_stylized(), "stylized() should return STYLIZED")


func test_same_slider_same_mode() -> void:
	var a: RefCounted = _profile_script.call("from_slider", 0.2)
	var b: RefCounted = _profile_script.call("from_slider", 0.2)
	assert_equal(a.mode, b.mode, "Same slider should yield same mode (determinism)")
	assert_equal(a.realism_slider, b.realism_slider, "Same slider should yield same value")
