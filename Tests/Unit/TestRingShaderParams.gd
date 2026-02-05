## Unit tests for RingShaderParams.
extends "res://Tests/Framework/TestCase.gd"

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func get_test_name() -> String:
	return "RingShaderParams"


## Creates a test ring system with specified properties.
func _create_test_ring_system(band_count: int = 5, ice_rich: bool = true) -> RingSystemProps:
	var ring_system: RingSystemProps = RingSystemProps.new()

	var body_radius: float = 60268000.0

	for i in range(band_count):
		var band: RingBand = RingBand.new()
		var inner_mult: float = 1.2 + float(i) * 0.25
		var outer_mult: float = inner_mult + 0.2

		band.inner_radius_m = body_radius * inner_mult
		band.outer_radius_m = body_radius * outer_mult
		band.optical_depth = 0.5 + randf() * 0.3

		if ice_rich:
			band.composition = {"water_ice": 0.9, "silicates": 0.1}
		else:
			band.composition = {"silicates": 0.6, "iron": 0.3, "carbon": 0.1}

		ring_system.add_band(band)

	return ring_system


## Creates a test body with ring system.
func _create_test_body_with_rings(ring_system: RingSystemProps, seed_val: int = 12345) -> CelestialBody:
	var body: CelestialBody = CelestialBody.new()
	body.type = CelestialType.Type.PLANET
	body.name = "Test Ringed Planet"

	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 5.683e26
	body.physical.radius_m = 60268000.0

	body.ring_system = ring_system

	body.provenance = Provenance.new()
	body.provenance.generation_seed = seed_val

	return body


func test_basic_params_exist() -> void:
	var ring_system: RingSystemProps = _create_test_ring_system()
	var body: CelestialBody = _create_test_body_with_rings(ring_system)
	var params: Dictionary = RingShaderParams.get_params(ring_system, body)

	assert_true(params.has("u_bandCount"), "Should have band count")
	assert_true(params.has("u_innerRadius"), "Should have inner radius")
	assert_true(params.has("u_outerRadius"), "Should have outer radius")
	assert_true(params.has("u_density"), "Should have density")
	assert_true(params.has("u_colorInner"), "Should have inner color")


func test_band_count_matches() -> void:
	var ring_system: RingSystemProps = _create_test_ring_system(7)
	var params: Dictionary = RingShaderParams.get_params(ring_system, null)

	assert_equal(params["u_bandCount"], 7, "Band count should match")


func test_ice_rich_colors() -> void:
	var ring_system: RingSystemProps = _create_test_ring_system(5, true)
	var params: Dictionary = RingShaderParams.get_params(ring_system, null)

	var inner_color: Color = params["u_colorInner"] as Color
	var outer_color: Color = params["u_colorOuter"] as Color

	assert_true(inner_color.r > 0.7, "Ice-rich inner should be bright")
	assert_true(outer_color.b >= outer_color.r * 0.9, "Ice-rich outer should have blue tint")


func test_rocky_colors() -> void:
	var ring_system: RingSystemProps = _create_test_ring_system(5, false)
	var params: Dictionary = RingShaderParams.get_params(ring_system, null)

	var inner_color: Color = params["u_colorInner"] as Color

	assert_true(inner_color.r < 0.7, "Rocky rings should be darker")


func test_radius_calculation() -> void:
	var ring_system: RingSystemProps = _create_test_ring_system(3)
	var body: CelestialBody = _create_test_body_with_rings(ring_system)
	var params: Dictionary = RingShaderParams.get_params(ring_system, body)

	var inner_r: float = params["u_innerRadius"] as float
	var outer_r: float = params["u_outerRadius"] as float

	assert_true(inner_r >= 1.1, "Inner radius should be > 1.1 body radii")
	assert_true(outer_r > inner_r, "Outer radius should be > inner radius")
	assert_true(outer_r <= 4.0, "Outer radius should be <= 4.0 body radii")


func test_density_from_optical_depth() -> void:
	var ring_system: RingSystemProps = RingSystemProps.new()

	var band: RingBand = RingBand.new(100000.0, 150000.0, 0.9, {"water_ice": 1.0}, 1.0, "")
	ring_system.add_band(band)

	var params: Dictionary = RingShaderParams.get_params(ring_system, null)
	var density: float = params["u_density"] as float

	assert_true(density > 0.5, "Dense ring should have high density param")


func test_gap_size_calculation() -> void:
	var ring_system: RingSystemProps = RingSystemProps.new()

	var band1: RingBand = RingBand.new(100000.0, 120000.0, 0.5, {}, 1.0, "")
	ring_system.add_band(band1)

	var band2: RingBand = RingBand.new(140000.0, 160000.0, 0.5, {}, 1.0, "")
	ring_system.add_band(band2)

	var params: Dictionary = RingShaderParams.get_params(ring_system, null)
	var gap_size: float = params["u_gapSize"] as float

	assert_true(gap_size > 0.1, "Significant gaps should produce gap_size > 0.1")


func test_determinism_same_seed() -> void:
	var ring_system: RingSystemProps = _create_test_ring_system()
	var body1: CelestialBody = _create_test_body_with_rings(ring_system, 42)
	var body2: CelestialBody = _create_test_body_with_rings(ring_system, 42)

	var params1: Dictionary = RingShaderParams.get_params(ring_system, body1)
	var params2: Dictionary = RingShaderParams.get_params(ring_system, body2)

	assert_equal(params1["u_seed"], params2["u_seed"], "Same seed should produce same u_seed")


func test_determinism_different_seed() -> void:
	var ring_system: RingSystemProps = _create_test_ring_system()
	var body1: CelestialBody = _create_test_body_with_rings(ring_system, 42)
	var body2: CelestialBody = _create_test_body_with_rings(ring_system, 999)

	var params1: Dictionary = RingShaderParams.get_params(ring_system, body1)
	var params2: Dictionary = RingShaderParams.get_params(ring_system, body2)

	assert_not_equal(params1["u_seed"], params2["u_seed"], "Different seeds should produce different u_seed")


func test_single_band_params() -> void:
	var band: RingBand = RingBand.new(100000.0, 150000.0, 0.7, {"water_ice": 0.8, "silicates": 0.2}, 1.0, "")

	var body: CelestialBody = CelestialBody.new()
	body.physical = PhysicalProps.new()
	body.physical.radius_m = 60000.0

	var params: Dictionary = RingShaderParams.get_band_params(band, body)

	assert_equal(params["u_bandCount"], 1, "Single band should have band count 1")
	assert_true(params["u_density"] > 0.5, "Band density should reflect optical depth")


func test_empty_ring_system() -> void:
	var ring_system: RingSystemProps = RingSystemProps.new()

	var params: Dictionary = RingShaderParams.get_params(ring_system, null)

	assert_equal(params["u_bandCount"], 0, "Empty ring system should have 0 bands")
	assert_true(params.has("u_density"), "Should still have density param")


func test_carbon_rich_colors() -> void:
	var ring_system: RingSystemProps = RingSystemProps.new()

	var band: RingBand = RingBand.new(100000.0, 150000.0, 0.5, {"carbon": 0.5, "carbon_compounds": 0.3, "silicates": 0.2}, 1.0, "")
	ring_system.add_band(band)

	var params: Dictionary = RingShaderParams.get_params(ring_system, null)

	var inner_color: Color = params["u_colorInner"] as Color

	assert_true(inner_color.r < 0.4, "Carbon-rich rings should be dark")
