## Tests for _traveller_builder.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _traveller_builder: GDScript = preload("res://src/domain/editing/TravellerConstraintBuilder.gd")


func test_code_8_covers_earth() -> void:
	# Earth diameter ~12 742 km -> Traveller code 8 (12 000 - 13 600 km).
	var cons: Dictionary = _traveller_builder.build_constraints_for_size(8)
	assert_true(cons.has("physical.radius_m"))
	var r: Vector2 = cons["physical.radius_m"] as Vector2
	# 12 000 km diam -> 6.0e6 m radius; 13 600 km -> 6.8e6 m.
	assert_float_equal(r.x, 6.0e6, 1.0)
	assert_float_equal(r.y, 6.8e6, 1.0)
	# Earth's actual radius (6.371e6 m) must fall inside.
	assert_in_range(6.371e6, r.x, r.y, "Earth radius in code-8 window")


func test_code_0_handles_tiny_bodies() -> void:
	var cons: Dictionary = _traveller_builder.build_constraints_for_size(0)
	assert_true(cons.has("physical.radius_m"))
	var r: Vector2 = cons["physical.radius_m"] as Vector2
	assert_float_equal(r.x, 0.0, 1.0, "code 0 min diameter is 0")
	assert_float_equal(r.y, 400000.0, 1.0, "code 0 max diameter 800 km -> 400 000 m radius")


func test_code_e_has_finite_upper_bound() -> void:
	var cons: Dictionary = _traveller_builder.build_constraints_for_size("E")
	var r: Vector2 = cons["physical.radius_m"] as Vector2
	assert_true(is_finite(r.y), "code E should get a synthetic finite cap")
	assert_greater_than(r.y, 60.0e6, "code E cap should cover large gas giants")


func test_mass_window_is_positive_and_ordered() -> void:
	for code: Variant in _traveller_builder.all_codes():
		var cons: Dictionary = _traveller_builder.build_constraints_for_size(code)
		assert_true(cons.has("physical.mass_kg"), "mass present for %s" % str(code))
		var m: Vector2 = cons["physical.mass_kg"] as Vector2
		assert_greater_than(m.y, m.x, "mass max > min for code %s" % str(code))
		assert_true(m.x >= 0.0, "mass min non-negative for code %s" % str(code))


func test_code_for_radius_round_trip_on_earth() -> void:
	var code: Variant = _traveller_builder.code_for_radius(6.371e6)
	assert_equal(code, 8, "Earth radius should map to code 8")


func test_code_for_radius_jupiter_is_d() -> void:
	# Jupiter radius ~69 911 km -> diameter ~139 822 km -> code E (120k+).
	var code: Variant = _traveller_builder.code_for_radius(6.9911e7)
	assert_equal(code, "E", "Jupiter should map to code E")


func test_invalid_code_returns_empty() -> void:
	var bad_int: Dictionary = _traveller_builder.build_constraints_for_size(99)
	var bad_str: Dictionary = _traveller_builder.build_constraints_for_size("Z")
	assert_true(bad_int.is_empty())
	assert_true(bad_str.is_empty())


func test_describe_code_format() -> void:
	var desc: String = _traveller_builder.describe_code(8)
	assert_true(desc.begins_with("8"), "starts with UWP digit")
	assert_true(desc.contains("km"), "contains km unit")


func test_all_codes_order_and_count() -> void:
	var codes: Array = _traveller_builder.all_codes()
	assert_equal(codes.size(), 15, "0-9 plus A-E is 15 codes")
	assert_equal(codes[0], 0)
	assert_equal(codes[9], 9)
	assert_equal(codes[10], "A")
	assert_equal(codes[14], "E")


func test_adjacent_codes_have_non_overlapping_radius_midpoints() -> void:
	# Ranges share boundaries (max of one == min of next). Midpoints should be strictly increasing.
	var codes: Array = _traveller_builder.all_codes()
	var prev_mid: float = -1.0
	for code: Variant in codes:
		var cons: Dictionary = _traveller_builder.build_constraints_for_size(code)
		var r: Vector2 = cons["physical.radius_m"] as Vector2
		var mid: float = (r.x + r.y) * 0.5
		assert_greater_than(mid, prev_mid, "midpoint increases for code %s" % str(code))
		prev_mid = mid
