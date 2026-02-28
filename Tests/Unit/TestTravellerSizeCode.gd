## Tests for TravellerSizeCode: diameter_km_to_code and code_to_diameter_range.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _tsc: GDScript = preload("res://src/domain/generation/archetypes/TravellerSizeCode.gd")


## Known diameters: Luna 3,474 km → 2; Earth 12,742 km → 8; Jupiter ~139,820 km → E.
func test_known_diameters() -> void:
	assert_equal(_tsc.diameter_km_to_code(3474.0), 2)
	assert_equal(_tsc.diameter_km_to_code(12742.0), 8)
	assert_equal(_tsc.diameter_km_to_code(139820.0), "E")


## Boundary at 0/800: &lt;800 → 0, 800 → 1.
func test_boundary_0_800() -> void:
	assert_equal(_tsc.diameter_km_to_code(0.0), 0)
	assert_equal(_tsc.diameter_km_to_code(799.0), 0)
	assert_equal(_tsc.diameter_km_to_code(800.0), 1)
	assert_equal(_tsc.diameter_km_to_code(801.0), 1)


## Boundary at 18,400: just below → 9, at/above → A then C.
func test_boundary_18400() -> void:
	assert_equal(_tsc.diameter_km_to_code(15199.0), 9)
	assert_equal(_tsc.diameter_km_to_code(15200.0), "A")
	assert_equal(_tsc.diameter_km_to_code(18399.0), "B")
	assert_equal(_tsc.diameter_km_to_code(18400.0), "C")
	assert_equal(_tsc.diameter_km_to_code(39999.0), "C")


## Boundary at 40,000 and 120,000: C/D and D/E.
func test_boundary_40000_120000() -> void:
	assert_equal(_tsc.diameter_km_to_code(40000.0), "D")
	assert_equal(_tsc.diameter_km_to_code(119999.0), "D")
	assert_equal(_tsc.diameter_km_to_code(120000.0), "E")
	assert_equal(_tsc.diameter_km_to_code(240000.0), "E")


## Negative diameter maps to 0.
func test_negative_diameter_returns_0() -> void:
	assert_equal(_tsc.diameter_km_to_code(-1.0), 0)


## code_to_diameter_range returns correct min/max for numeric and letter codes.
func test_code_to_diameter_range() -> void:
	var r0: Dictionary = _tsc.code_to_diameter_range(0)
	assert_float_equal(r0["min"], 0.0)
	assert_float_equal(r0["max"], 800.0)

	var r8: Dictionary = _tsc.code_to_diameter_range(8)
	assert_float_equal(r8["min"], 12000.0)
	assert_float_equal(r8["max"], 13600.0)

	var r_d: Dictionary = _tsc.code_to_diameter_range("D")
	assert_float_equal(r_d["min"], 40000.0)
	assert_float_equal(r_d["max"], 120000.0)

	var r_e: Dictionary = _tsc.code_to_diameter_range("E")
	assert_float_equal(r_e["min"], 120000.0)
	assert_float_equal(r_e["max"], -1.0)

	var invalid: Dictionary = _tsc.code_to_diameter_range(99)
	assert_true(invalid.is_empty())
	var invalid_s: Dictionary = _tsc.code_to_diameter_range("X")
	assert_true(invalid_s.is_empty())


## to_string_uwp returns single character for UWP digit.
func test_to_string_uwp() -> void:
	assert_equal(_tsc.to_string_uwp(0), "0")
	assert_equal(_tsc.to_string_uwp(8), "8")
	assert_equal(_tsc.to_string_uwp("A"), "A")
	assert_equal(_tsc.to_string_uwp("E"), "E")
