## Unit tests for AtmosphereShaderParams.
extends "res://Tests/Framework/TestCase.gd"

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func get_test_name() -> String:
	return "AtmosphereShaderParams"


## Creates a test body with atmosphere.
func _create_test_body(
	pressure_pa: float = 101325.0,
	scale_height_m: float = 8500.0,
	greenhouse: float = 1.15,
	composition: Dictionary = {}
) -> CelestialBody:
	var body: CelestialBody = CelestialBody.new()
	body.type = CelestialType.Type.PLANET
	body.name = "Test Planet"

	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 5.972e24
	body.physical.radius_m = 6.371e6

	body.atmosphere = AtmosphereProps.new(
		pressure_pa,
		scale_height_m,
		{"N2": 0.78, "O2": 0.21, "Ar": 0.01} if composition.is_empty() else composition,
		greenhouse
	)

	return body


func test_earth_like_params() -> void:
	var body: CelestialBody = _create_test_body()
	var params: Dictionary = AtmosphereShaderParams.get_params(body)

	assert_true(params.has("u_atmosphereColor"), "Should have atmosphere color")
	assert_true(params.has("u_density"), "Should have density")
	assert_true(params.has("u_falloff"), "Should have falloff")
	assert_true(params.has("u_scatterStrength"), "Should have scatter strength")

	var density: float = params["u_density"] as float
	assert_true(density > 0.5 and density < 1.5, "Earth-like density should be moderate")


func test_thin_atmosphere() -> void:
	var body: CelestialBody = _create_test_body(610.0, 11000.0, 1.0)
	var params: Dictionary = AtmosphereShaderParams.get_params(body)

	var density: float = params["u_density"] as float
	assert_true(density < 0.1, "Thin atmosphere should have low density")

	var terminator: float = params["u_terminatorSoftness"] as float
	assert_true(terminator < 0.15, "Thin atmosphere should have sharp terminator")


func test_thick_atmosphere() -> void:
	var body: CelestialBody = _create_test_body(9200000.0, 15000.0, 3.5, {"CO2": 0.96, "N2": 0.03})
	var params: Dictionary = AtmosphereShaderParams.get_params(body)

	var density: float = params["u_density"] as float
	assert_true(density > 1.5, "Thick atmosphere should have high density")

	var greenhouse: float = params["u_greenhouseIntensity"] as float
	assert_true(greenhouse > 0.5, "Strong greenhouse should have high intensity")

	var terminator: float = params["u_terminatorSoftness"] as float
	assert_true(terminator > 0.2, "Thick atmosphere should have soft terminator")


func test_greenhouse_effect() -> void:
	var body_no_gh: CelestialBody = _create_test_body(101325.0, 8500.0, 1.0)
	var params_no_gh: Dictionary = AtmosphereShaderParams.get_params(body_no_gh)

	var body_gh: CelestialBody = _create_test_body(101325.0, 8500.0, 2.5)
	var params_gh: Dictionary = AtmosphereShaderParams.get_params(body_gh)

	var gh_none: float = params_no_gh["u_greenhouseIntensity"] as float
	var gh_strong: float = params_gh["u_greenhouseIntensity"] as float

	assert_true(gh_none < 0.1, "No greenhouse should have low intensity")
	assert_true(gh_strong > 0.5, "Strong greenhouse should have high intensity")


func test_co2_atmosphere_color() -> void:
	var body: CelestialBody = _create_test_body(101325.0, 8500.0, 1.5, {"CO2": 0.95, "N2": 0.05})
	var params: Dictionary = AtmosphereShaderParams.get_params(body)

	var color: Color = params["u_atmosphereColor"] as Color
	assert_true(color.r > color.b, "CO2 atmosphere should have warm tones")

	var gh_color: Color = params["u_greenhouseColor"] as Color
	assert_true(gh_color.r > gh_color.b, "CO2 greenhouse glow should be orange-red")


func test_methane_atmosphere() -> void:
	var body: CelestialBody = _create_test_body(150000.0, 20000.0, 1.2, {"N2": 0.95, "CH4": 0.05})
	var params: Dictionary = AtmosphereShaderParams.get_params(body)

	var gh_color: Color = params["u_greenhouseColor"] as Color
	assert_true(gh_color.g > 0.7, "Methane greenhouse should have yellow tint")


func test_h2_he_atmosphere() -> void:
	var body: CelestialBody = _create_test_body(100000.0, 27000.0, 1.0, {"H2": 0.86, "He": 0.14})
	var params: Dictionary = AtmosphereShaderParams.get_params(body)

	var scatter: float = params["u_scatterStrength"] as float
	assert_true(scatter < 1.0, "H2/He atmosphere should scatter less")


func test_no_atmosphere() -> void:
	var body: CelestialBody = CelestialBody.new()
	body.type = CelestialType.Type.PLANET
	body.physical = PhysicalProps.new()

	var params: Dictionary = AtmosphereShaderParams.get_params(body)

	assert_true(params.has("u_density"), "Should have default density")


func test_should_render_atmosphere_true() -> void:
	var body: CelestialBody = _create_test_body()
	assert_true(AtmosphereShaderParams.should_render_atmosphere(body), "Earth-like should render atmosphere")


func test_should_render_atmosphere_false_no_atmo() -> void:
	var body: CelestialBody = CelestialBody.new()
	body.type = CelestialType.Type.PLANET
	body.physical = PhysicalProps.new()

	assert_false(AtmosphereShaderParams.should_render_atmosphere(body), "No atmosphere should not render")


func test_should_render_atmosphere_false_too_thin() -> void:
	var body: CelestialBody = _create_test_body(50.0, 8500.0, 1.0)
	assert_false(AtmosphereShaderParams.should_render_atmosphere(body), "Very thin atmosphere should not render")


func test_falloff_from_scale_height() -> void:
	var body_large: CelestialBody = _create_test_body(100000.0, 27000.0, 1.0)
	body_large.physical.radius_m = 69911000.0

	var body_small: CelestialBody = _create_test_body(101325.0, 8500.0, 1.0)

	var params_large: Dictionary = AtmosphereShaderParams.get_params(body_large)
	var params_small: Dictionary = AtmosphereShaderParams.get_params(body_small)

	var falloff_large: float = params_large["u_falloff"] as float
	var falloff_small: float = params_small["u_falloff"] as float

	assert_true(falloff_large < falloff_small, "Larger scale height should have lower falloff")


func test_sun_glow_strength() -> void:
	var body_thin: CelestialBody = _create_test_body(5000.0, 8500.0, 1.0)
	var params_thin: Dictionary = AtmosphereShaderParams.get_params(body_thin)

	var body_thick: CelestialBody = _create_test_body(500000.0, 8500.0, 1.0)
	var params_thick: Dictionary = AtmosphereShaderParams.get_params(body_thick)

	var glow_thin: float = params_thin["u_sunGlowStrength"] as float
	var glow_thick: float = params_thick["u_sunGlowStrength"] as float

	assert_true(glow_thick > glow_thin, "Thicker atmosphere should have stronger sun glow")
