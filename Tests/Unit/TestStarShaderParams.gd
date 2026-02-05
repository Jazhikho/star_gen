## Unit tests for StarShaderParams.
## Tests temperature-to-parameter mapping, determinism, and edge cases.
extends "res://Tests/Framework/TestCase.gd"

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func get_test_name() -> String:
	return "StarShaderParams"


func test_solar_temperature_color() -> void:
	var body: CelestialBody = _create_test_star(5778.0, 1.0, 4.6e9, 2.16e6, 12345)
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)

	assert_equal(params["u_temperature"], 5778.0, "Temperature should match")

	var color: Color = params["u_star_color"]
	assert_true(color.r > 0.9, "Solar color red should be high")
	assert_true(color.g > 0.8, "Solar color green should be moderately high")
	assert_true(color.b < color.r, "Solar color blue should be less than red")


func test_hot_star_temperature() -> void:
	var body: CelestialBody = _create_test_star(35000.0, 100.0, 1e6, 1e5, 11111)
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)

	var color: Color = params["u_star_color"]
	assert_true(color.b >= color.r, "Hot star should have high blue")
	assert_true(params["u_limbDark"] < 0.4, "Hot star limb darkening should be low")
	assert_true(params["u_granContrast"] < 0.2, "Hot star granulation contrast should be low")


func test_cool_star_temperature() -> void:
	var body: CelestialBody = _create_test_star(3000.0, 0.01, 10e9, 5e6, 22222)
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)

	var color: Color = params["u_star_color"]
	assert_true(color.r > color.b, "Cool star should have more red than blue")
	assert_true(params["u_limbDark"] > 0.7, "Cool star limb darkening should be high")
	assert_true(params["u_granContrast"] > 0.4, "Cool star granulation contrast should be high")


func test_granulation_varies_with_temperature() -> void:
	var hot: CelestialBody = _create_test_star(15000.0, 10.0, 1e8, 1e5, 100)
	var solar: CelestialBody = _create_test_star(5778.0, 1.0, 4.6e9, 2.16e6, 100)
	var cool: CelestialBody = _create_test_star(3500.0, 0.05, 8e9, 5e6, 100)

	var hot_params: Dictionary = StarShaderParams.get_star_shader_params(hot)
	var solar_params: Dictionary = StarShaderParams.get_star_shader_params(solar)
	var cool_params: Dictionary = StarShaderParams.get_star_shader_params(cool)

	assert_true(hot_params["u_granContrast"] < solar_params["u_granContrast"],
		"Hot star should have less granulation contrast than solar")
	assert_true(solar_params["u_granContrast"] < cool_params["u_granContrast"],
		"Solar should have less granulation contrast than cool star")


func test_spot_count_varies_with_activity() -> void:
	var active: CelestialBody = _create_test_star(5778.0, 1.0, 1e8, 1e5, 333)
	var quiet: CelestialBody = _create_test_star(5778.0, 1.0, 9e9, 5e6, 333)

	var active_params: Dictionary = StarShaderParams.get_star_shader_params(active)
	var quiet_params: Dictionary = StarShaderParams.get_star_shader_params(quiet)

	assert_true(active_params["u_spotCount"] > quiet_params["u_spotCount"],
		"Active star should have more spots than quiet star")


func test_limb_darkening_varies_with_temperature() -> void:
	var temps: Array[float] = [35000.0, 10000.0, 7500.0, 6000.0, 5200.0, 4000.0, 3000.0]
	var prev_ld: float = 0.0

	for i in range(temps.size()):
		var body: CelestialBody = _create_test_star(temps[i], 1.0, 4.6e9, 2.16e6, 100)
		var params: Dictionary = StarShaderParams.get_star_shader_params(body)

		if i > 0:
			assert_true(params["u_limbDark"] >= prev_ld,
				"Limb darkening should increase as temperature decreases")
		prev_ld = params["u_limbDark"]


func test_determinism_same_seed() -> void:
	var body1: CelestialBody = _create_test_star(5778.0, 1.0, 4.6e9, 2.16e6, 42)
	var body2: CelestialBody = _create_test_star(5778.0, 1.0, 4.6e9, 2.16e6, 42)

	var params1: Dictionary = StarShaderParams.get_star_shader_params(body1)
	var params2: Dictionary = StarShaderParams.get_star_shader_params(body2)

	assert_equal(params1["u_seed"], params2["u_seed"], "Same seed should produce same u_seed")
	assert_equal(params1["u_spotCount"], params2["u_spotCount"], "Same seed should produce same spot count")


func test_determinism_different_seed() -> void:
	var body1: CelestialBody = _create_test_star(5778.0, 1.0, 4.6e9, 2.16e6, 42)
	var body2: CelestialBody = _create_test_star(5778.0, 1.0, 4.6e9, 2.16e6, 999)

	var params1: Dictionary = StarShaderParams.get_star_shader_params(body1)
	var params2: Dictionary = StarShaderParams.get_star_shader_params(body2)

	assert_not_equal(params1["u_seed"], params2["u_seed"], "Different seeds should produce different u_seed")


func test_corona_scales_with_luminosity() -> void:
	var dim: CelestialBody = _create_test_star(5778.0, 0.1, 4.6e9, 2.16e6, 100)
	var bright: CelestialBody = _create_test_star(5778.0, 10.0, 4.6e9, 2.16e6, 100)

	var dim_params: Dictionary = StarShaderParams.get_star_shader_params(dim)
	var bright_params: Dictionary = StarShaderParams.get_star_shader_params(bright)

	assert_true(bright_params["u_coronaExtent"] >= dim_params["u_coronaExtent"],
		"Brighter star should have larger corona extent")
	assert_true(bright_params["u_bloomIntensity"] >= dim_params["u_bloomIntensity"],
		"Brighter star should have more bloom")


func test_chromosphere_parameters() -> void:
	var body: CelestialBody = _create_test_star(5778.0, 1.0, 4.6e9, 2.16e6, 12345)
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)

	assert_true(params["u_chromoThick"] > 0.0, "Chromosphere thickness should be positive")
	assert_true(params["u_chromoIntensity"] > 0.0, "Chromosphere intensity should be positive")
	assert_true(params["u_chromoShift"] >= 0.0 and params["u_chromoShift"] <= 1.0,
		"Chromosphere shift should be in 0-1 range")


func _create_test_star(
	temp_k: float,
	luminosity_solar: float,
	age_years: float,
	rotation_period_s: float,
	seed_val: int
) -> CelestialBody:
	var body: CelestialBody = CelestialBody.new("test_star", "Test Star", CelestialType.Type.STAR, null, null)
	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 1.989e30
	body.physical.radius_m = 6.96e8
	body.physical.rotation_period_s = rotation_period_s

	body.stellar = StellarProps.new()
	body.stellar.effective_temperature_k = temp_k
	body.stellar.luminosity_watts = luminosity_solar * StellarProps.SOLAR_LUMINOSITY_WATTS
	body.stellar.age_years = age_years
	body.stellar.spectral_class = "G2V"
	body.stellar.stellar_type = "main_sequence"

	body.provenance = Provenance.new(seed_val, "1.0.0", 0, 0, {})

	return body
