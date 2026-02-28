## Unit tests for GasGiantShaderParams (spatial shader param derivation).
extends "res://Tests/Framework/TestCase.gd"

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func get_test_name() -> String:
	return "GasGiantShaderParams"


func _create_gas_giant(
	temperature_k: float,
	oblateness: float,
	rotation_period_s: float,
	mass_earth: float = 318.0,
	seed_val: int = 54321
) -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG * mass_earth,
		Units.EARTH_RADIUS_METERS * 11.2,
		rotation_period_s,
		3.0,
		oblateness,
		1.5e27,
		5.0e17
	)
	var body: CelestialBody = CelestialBody.new(
		"test_gas_giant",
		"Test Gas Giant",
		CelestialType.Type.PLANET,
		physical,
		Provenance.new(seed_val, "1.0.0", 0, 0, {})
	)
	var surface: SurfaceProps = SurfaceProps.new()
	surface.temperature_k = temperature_k
	body.surface = surface
	return body


func test_jupiter_like_params() -> void:
	var body: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	assert_true(params.has("u_bandCount"), "Should have band count")
	assert_true(params["u_bandCount"] >= 10.0, "Jupiter-like should have many bands")
	assert_float_equal(params["u_oblateness"], 0.0, 0.001, "Rendered oblateness should be 0")
	var band_light: Color = params["u_colBandLight"] as Color
	# Jupiter palettes are all warm/neutral — red+green should dominate blue.
	assert_true(
		band_light.r + band_light.g > band_light.b * 1.5,
		"Jupiter should have warm tones"
	)
	assert_true(params.has("u_bandSharpness"), "Should have band sharpness")
	assert_true(params.has("u_bandWarp"), "Should have band warp")
	assert_true(params.has("u_stormCount"), "Should have storm count")
	assert_true(params.has("u_hazeDensity"), "Should have haze density")
	assert_true(params["u_hazeDensity"] < 0.25, "Jupiter should have minimal haze")
	assert_true(params["u_detailLevel"] > 0.6, "Jupiter should have high detail")


func test_neptune_like_params() -> void:
	var body: CelestialBody = _create_gas_giant(72.0, 0.017, 57996.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	var band_light: Color = params["u_colBandLight"] as Color
	assert_true(band_light.b > band_light.r, "Ice giant should have cool/blue tones")


func test_hot_jupiter_params() -> void:
	var body: CelestialBody = _create_gas_giant(1200.0, 0.03, 86400.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	var band_light: Color = params["u_colBandLight"] as Color
	assert_true(band_light.r > 0.8, "Hot Jupiter should have orange/red tones")


func test_super_jupiter_params() -> void:
	var body: CelestialBody = _create_gas_giant(180.0, 0.08, 30000.0, 500.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	assert_true(params["u_bandCount"] >= 12.0, "Super-Jupiter should have many bands")
	assert_true(params["u_stormIntensity"] > 0.3, "Super-Jupiter should have notable storms")


func test_oblateness_passed() -> void:
	var body: CelestialBody = _create_gas_giant(150.0, 0.1, 40000.0)
	body.physical.oblateness = 0.1
	var params: Dictionary = GasGiantShaderParams.get_params(body)
	# Oblateness is stored on body but not rendered visually (always 0.0)
	assert_float_equal(params["u_oblateness"], 0.0, 0.001, "Rendered oblateness should be 0")


func test_methane_blue_tint() -> void:
	var body: CelestialBody = _create_gas_giant(75.0, 0.02, 60000.0)
	body.atmosphere = AtmosphereProps.new(101325.0 * 2.0, 20000.0, {"H2": 0.8, "He": 0.15, "CH4": 0.05}, 1.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	var atmo_color: Color = params["u_atmoColor"] as Color
	assert_true(atmo_color.b > atmo_color.r, "Methane atmosphere should add blue tint")


func test_determinism_same_seed() -> void:
	var body1: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0, 318.0, 12345)
	var body2: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0, 318.0, 12345)
	var params1: Dictionary = GasGiantShaderParams.get_params(body1)
	var params2: Dictionary = GasGiantShaderParams.get_params(body2)

	assert_float_equal(params1["u_seed"], params2["u_seed"], 0.001, "Same seed should produce same u_seed")
	assert_float_equal(params1["u_bandCount"], params2["u_bandCount"], 0.01, "Band count should match")
	assert_float_equal(params1["u_oblateness"], params2["u_oblateness"], 0.001, "Oblateness should match")


func test_determinism_different_seed() -> void:
	var body1: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0, 318.0, 11111)
	var body2: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0, 318.0, 22222)
	var params1: Dictionary = GasGiantShaderParams.get_params(body1)
	var params2: Dictionary = GasGiantShaderParams.get_params(body2)

	assert_not_equal(params1["u_seed"], params2["u_seed"], "Different seed should produce different u_seed")
	var differs: bool = (
		absf(float(params1["u_bandSharpness"]) - float(params2["u_bandSharpness"])) > 0.001
		or absf(float(params1["u_bandWarp"]) - float(params2["u_bandWarp"])) > 0.001
		or int(params1["u_stormCount"]) != int(params2["u_stormCount"])
	)
	assert_true(differs, "Different seed should produce different structural params")


func test_is_gas_giant_true() -> void:
	var body: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0, 318.0)
	assert_true(GasGiantShaderParams.is_gas_giant(body), "Jupiter-mass body without terrain should be gas giant")


func test_is_gas_giant_false_small_mass() -> void:
	var body: CelestialBody = _create_gas_giant(288.0, 0.0, 86400.0, 5.0)
	assert_false(GasGiantShaderParams.is_gas_giant(body), "Earth-mass body should not be gas giant")


func test_is_gas_giant_false_terrain() -> void:
	var body: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0, 318.0)
	body.surface.terrain = TerrainProps.new(8000.0, 0.5, 0.3, 0.4, 0.3, "varied")
	assert_false(GasGiantShaderParams.is_gas_giant(body), "Body with terrain should not be gas giant")


func test_saturn_temperature_colors() -> void:
	# Use 300 K so body is classified SATURN_CLASS (warm band); 134 K would classify as ice giant.
	# Palette + hue/jitter can vary; assert warm golden character (r+g dominate b).
	var body: CelestialBody = _create_gas_giant(300.0, 0.098, 38340.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	var band_light: Color = params["u_colBandLight"] as Color
	assert_true(band_light.r + band_light.g > band_light.b * 1.3, "Saturn-class should have warm golden tones")


func test_uranus_temperature_colors() -> void:
	var body: CelestialBody = _create_gas_giant(76.0, 0.023, 62040.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	var band_light: Color = params["u_colBandLight"] as Color
	assert_true(band_light.b > band_light.r, "Uranus-temperature should have cool/blue tones")


func test_axial_tilt_passed() -> void:
	var body: CelestialBody = _create_gas_giant(150.0, 0.065, 40000.0)
	body.physical.axial_tilt_deg = 45.0
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	var tilt_rad: float = params["u_axialTilt"] as float
	assert_float_equal(tilt_rad, deg_to_rad(45.0), 0.01, "Axial tilt should be passed in radians")


func test_mini_neptune_params() -> void:
	# 15 Earth-mass body with no terrain → MINI_NEPTUNE archetype
	var body: CelestialBody = _create_gas_giant(250.0, 0.02, 50000.0, 15.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	assert_true(params["u_hazeDensity"] > 0.4, "Mini-Neptune should have thick haze")
	assert_true(params["u_detailLevel"] < 0.35, "Mini-Neptune should have low detail")
	assert_true(params["u_bandCount"] < 10.0, "Mini-Neptune should have few bands")
	assert_true(int(params["u_stormCount"]) <= 1, "Mini-Neptune should have ≤1 storm")


func test_archetype_structural_differentiation() -> void:
	# Verify that Jupiter and Uranus produce structurally distinct params, not just color swaps.
	var jup: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0, 318.0, 100)
	var ura: CelestialBody = _create_gas_giant(76.0, 0.023, 62040.0, 80.0, 100)
	var jp: Dictionary = GasGiantShaderParams.get_params(jup)
	var up: Dictionary = GasGiantShaderParams.get_params(ura)

	assert_true(
		float(up["u_hazeDensity"]) > float(jp["u_hazeDensity"]) + 0.2,
		"Uranus should be much hazier than Jupiter"
	)
	assert_true(
		float(up["u_detailLevel"]) < float(jp["u_detailLevel"]) - 0.3,
		"Uranus should have much less visible detail"
	)
	assert_true(
		float(up["u_bandSharpness"]) < float(jp["u_bandSharpness"]),
		"Uranus bands should be softer than Jupiter"
	)


func test_neptune_dark_spots() -> void:
	var body: CelestialBody = _create_gas_giant(72.0, 0.017, 57996.0, 100.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	assert_true(
		float(params["u_darkSpotRatio"]) > 0.35,
		"Neptune should have high dark-spot ratio"
	)


func test_new_color_uniforms_present() -> void:
	var body: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0)
	var params: Dictionary = GasGiantShaderParams.get_params(body)

	assert_true(params.has("u_colZoneMid"), "Should have zone mid color")
	assert_true(params.has("u_colBeltMid"), "Should have belt mid color")
	assert_true(params.has("u_colBeltPolar"), "Should have belt polar color")
	assert_true(params.has("u_hazeColor"), "Should have haze color")
