## Unit tests for shader parameter derivation (StarShaderParams, TerrestrialShaderParams, GasGiantShaderParams, RingShaderParams).
extends "res://Tests/Framework/TestCase.gd"

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func get_test_name() -> String:
	return "ColorUtils Shader Parameters"


# ============================================================================
# STAR SHADER PARAMETER TESTS
# ============================================================================

func test_star_params_sun_like() -> void:
	var body: CelestialBody = _create_star(5778.0, StellarProps.SOLAR_LUMINOSITY_WATTS, 4.6e9)
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)
	assert_true(params.has("u_temperature"), "Should have temperature")
	assert_float_equal(params["u_temperature"], 5778.0, 1.0, "Temperature should be 5778K")
	assert_true(params.has("u_limbDark"), "Should have limb darkening")
	assert_float_equal(params["u_limbDark"], 0.6, 0.05, "G-type limb darkening ~0.6")
	assert_true(params.has("u_granScale"), "Should have granulation scale")
	assert_float_equal(params["u_granScale"], 30.0, 2.0, "G-type granulation scale ~30")
	assert_true(params.has("u_spotCount"), "Should have spot count")
	assert_true(params["u_spotCount"] >= 0.0 and params["u_spotCount"] <= 10.0, "Reasonable spot count")


func test_star_params_hot_o_type() -> void:
	var body: CelestialBody = _create_star(35000.0, StellarProps.SOLAR_LUMINOSITY_WATTS * 100000.0, 1.0e6)
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)
	assert_float_equal(params["u_limbDark"], 0.2, 0.05, "O-type has minimal limb darkening")
	assert_float_equal(params["u_granScale"], 10.0, 2.0, "O-type has small granulation")
	assert_float_equal(params["u_granContrast"], 0.05, 0.02, "O-type has low granulation contrast")
	assert_float_equal(params["u_spotCount"], 0.0, 0.01, "O-type has no spots")


func test_star_params_cool_m_type() -> void:
	var body: CelestialBody = _create_star(3000.0, StellarProps.SOLAR_LUMINOSITY_WATTS * 0.001, 8.0e9)
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)
	assert_float_equal(params["u_limbDark"], 0.8, 0.05, "M-type has strong limb darkening")
	assert_true(params["u_granContrast"] > 0.5, "M-type has high granulation contrast")
	assert_true(params["u_spotCount"] > 0.0, "M-type can have many spots")


func test_star_params_young_active() -> void:
	var body: CelestialBody = _create_star(5778.0, StellarProps.SOLAR_LUMINOSITY_WATTS, 0.5e9)
	body.physical.rotation_period_s = 86400.0 * 5.0
	var params: Dictionary = StarShaderParams.get_star_shader_params(body)
	assert_true(params["u_spotCount"] > 3.0, "Young fast rotator should have more spots")
	assert_true(params["u_flareIntensity"] > 0.15, "Young star should have higher flare intensity")


func test_star_params_deterministic_seed() -> void:
	var body1: CelestialBody = _create_star(5778.0, StellarProps.SOLAR_LUMINOSITY_WATTS, 4.6e9)
	body1.provenance = Provenance.new(12345, "1.0.0", 0, 0, {})
	var body2: CelestialBody = _create_star(5778.0, StellarProps.SOLAR_LUMINOSITY_WATTS, 4.6e9)
	body2.provenance = Provenance.new(12345, "1.0.0", 0, 0, {})
	var params1: Dictionary = StarShaderParams.get_star_shader_params(body1)
	var params2: Dictionary = StarShaderParams.get_star_shader_params(body2)
	assert_float_equal(params1["u_seed"], params2["u_seed"], 0.001, "Same seed should produce same u_seed")


# ============================================================================
# TERRESTRIAL PLANET SHADER PARAMETER TESTS
# ============================================================================

func test_terrestrial_params_earth_like() -> void:
	var body: CelestialBody = _create_terrestrial_planet(288.0, 1.0, 0.7, 0.1)
	var params: Dictionary = TerrestrialShaderParams.get_terrestrial_shader_params(body)
	assert_true(params.has("u_seaLevel"), "Should have sea level")
	assert_true(params["u_seaLevel"] > 0.3, "Should have significant sea level")
	assert_true(params.has("u_iceCap"), "Should have ice cap")
	assert_float_equal(params["u_iceCap"], 0.1, 0.01, "Ice cap should match")
	assert_true(params.has("u_cloudCoverage"), "Should have cloud coverage")
	assert_true(params["u_cloudCoverage"] > 0.2, "Should have clouds with atmosphere")
	assert_true(params.has("u_atmoDensity"), "Should have atmosphere density")
	assert_float_equal(params["u_atmoDensity"], 1.0, 0.1, "Earth-like atmosphere density")


func test_terrestrial_params_desert_world() -> void:
	var body: CelestialBody = _create_terrestrial_planet(350.0, 0.8, 0.05, 0.0)
	body.surface.surface_type = "desert"
	var params: Dictionary = TerrestrialShaderParams.get_terrestrial_shader_params(body)
	assert_true(params["u_seaLevel"] < 0.25, "Desert should have low sea level")
	assert_float_equal(params["u_iceCap"], 0.0, 0.01, "No ice on hot desert")
	var col_mid: Vector3 = params["u_colMid"] as Vector3
	assert_true(col_mid.x > col_mid.z, "Desert should have more red than blue")


func test_terrestrial_params_ice_world() -> void:
	var body: CelestialBody = _create_terrestrial_planet(180.0, 0.5, 0.3, 0.8)
	body.surface.surface_type = "icy"
	var params: Dictionary = TerrestrialShaderParams.get_terrestrial_shader_params(body)
	assert_float_equal(params["u_iceCap"], 0.8, 0.01, "Should have heavy ice coverage")
	var col_mid: Vector3 = params["u_colMid"] as Vector3
	assert_true(col_mid.x > 0.7 and col_mid.y > 0.7, "Ice world should be bright")


func test_terrestrial_params_no_atmosphere() -> void:
	var body: CelestialBody = _create_terrestrial_planet(300.0, 0.0, 0.0, 0.0)
	body.atmosphere = null
	var params: Dictionary = TerrestrialShaderParams.get_terrestrial_shader_params(body)
	assert_float_equal(params["u_atmoDensity"], 0.0, 0.01, "No atmosphere density")
	assert_float_equal(params["u_cloudCoverage"], 0.0, 0.01, "No clouds without atmosphere")
	assert_float_equal(params["u_scatterStrength"], 0.0, 0.01, "No scattering without atmosphere")


# ============================================================================
# GAS GIANT SHADER PARAMETER TESTS
# ============================================================================

func test_gas_giant_params_jupiter_like() -> void:
	var body: CelestialBody = _create_gas_giant(165.0, 0.065, 35730.0)
	var params: Dictionary = GasGiantShaderParams.get_gas_giant_shader_params(body)
	assert_true(params.has("u_gBandCount"), "Should have band count")
	assert_true(params["u_gBandCount"] >= 10.0, "Jupiter-like should have many bands")
	# Oblateness is stored in data but not rendered (always 0.0)
	assert_float_equal(params["u_gOblateness"], 0.0, 0.001, "Rendered oblateness should be 0")
	var band_light: Vector3 = params["u_gColBandLight"] as Vector3
	assert_true(band_light.x > band_light.z, "Jupiter should have warm tones")


func test_gas_giant_params_ice_giant() -> void:
	var body: CelestialBody = _create_gas_giant(72.0, 0.017, 57996.0)
	var params: Dictionary = GasGiantShaderParams.get_gas_giant_shader_params(body)
	var band_light: Vector3 = params["u_gColBandLight"] as Vector3
	assert_true(band_light.z > band_light.x, "Ice giant should have cool/blue tones")


func test_gas_giant_params_hot_jupiter() -> void:
	var body: CelestialBody = _create_gas_giant(1200.0, 0.03, 86400.0)
	var params: Dictionary = GasGiantShaderParams.get_gas_giant_shader_params(body)
	var band_light: Vector3 = params["u_gColBandLight"] as Vector3
	assert_true(band_light.x > 0.8, "Hot Jupiter should have orange/red tones")


# ============================================================================
# RING SHADER PARAMETER TESTS
# ============================================================================

func test_ring_params_simple_ring() -> void:
	var ring_system: RingSystemProps = _create_ring_system(2, 1.5, 2.5)
	var planet_radius: float = 70000000.0
	var params: Dictionary = RingShaderParams.get_ring_shader_params(ring_system, planet_radius)
	assert_true(params.has("u_ringType"), "Should have ring type")
	assert_true(params["u_ringType"] >= 1, "Should have rings enabled")
	assert_true(params.has("u_ringInner"), "Should have inner radius")
	assert_true(params.has("u_ringOuter"), "Should have outer radius")
	assert_true(params["u_ringOuter"] > params["u_ringInner"], "Outer should be greater than inner")


func test_ring_params_no_rings() -> void:
	var params: Dictionary = RingShaderParams.get_ring_shader_params(null, 70000000.0)
	assert_equal(params["u_ringType"], 0, "No rings should be type 0")


func test_ring_params_complex_rings() -> void:
	var ring_system: RingSystemProps = _create_ring_system(7, 1.2, 3.0)
	var planet_radius: float = 60000000.0
	var params: Dictionary = RingShaderParams.get_ring_shader_params(ring_system, planet_radius)
	assert_equal(params["u_ringType"], 3, "7 bands should be complex (type 3)")
	assert_equal(params["u_ringBands"], 7, "Should have 7 bands")


# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

func _create_star(temperature_k: float, luminosity_watts: float, age_years: float) -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.SOLAR_MASS_KG,
		Units.SOLAR_RADIUS_METERS,
		2.16e6,
		7.25,
		0.0,
		0.0,
		0.0
	)
	var stellar: StellarProps = StellarProps.new(
		luminosity_watts,
		temperature_k,
		"G2V",
		"main_sequence",
		1.0,
		age_years
	)
	var body: CelestialBody = CelestialBody.new(
		"test_star",
		"Test Star",
		CelestialType.Type.STAR,
		physical,
		null
	)
	body.stellar = stellar
	return body


func _create_terrestrial_planet(
	temperature_k: float,
	pressure_atm: float,
	ocean_coverage: float,
	ice_coverage: float
) -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0,
		23.5,
		0.003,
		8.0e22,
		4.0e13
	)
	var body: CelestialBody = CelestialBody.new(
		"test_planet",
		"Test Planet",
		CelestialType.Type.PLANET,
		physical,
		Provenance.new(12345, "1.0.0", 0, 0, {})
	)
	var surface: SurfaceProps = SurfaceProps.new()
	surface.temperature_k = temperature_k
	surface.albedo = 0.3
	surface.surface_type = "rocky"
	surface.volcanism_level = 0.2
	var terrain: TerrainProps = TerrainProps.new(8000.0, 0.5, 0.3, 0.4, 0.3, "varied")
	surface.terrain = terrain
	if ocean_coverage > 0.0:
		surface.hydrosphere = HydrosphereProps.new(ocean_coverage, 3700.0, 0.0, 35.0, "water")
	if ice_coverage > 0.0:
		var cryo: CryosphereProps = CryosphereProps.new()
		cryo.polar_cap_coverage = ice_coverage
		surface.cryosphere = cryo
	body.surface = surface
	if pressure_atm > 0.0:
		body.atmosphere = AtmosphereProps.new(
			pressure_atm * 101325.0,
			8500.0,
			{"N2": 0.78, "O2": 0.21, "Ar": 0.01},
			1.0
		)
	return body


func _create_gas_giant(temperature_k: float, oblateness: float, rotation_period_s: float) -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG * 318.0,
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
		Provenance.new(54321, "1.0.0", 0, 0, {})
	)
	var surface: SurfaceProps = SurfaceProps.new()
	surface.temperature_k = temperature_k
	body.surface = surface
	return body


func _create_ring_system(band_count: int, inner_ratio: float, outer_ratio: float) -> RingSystemProps:
	var ring_system: RingSystemProps = RingSystemProps.new()
	ring_system.inclination_deg = 0.0
	var planet_radius: float = 70000000.0
	var total_width: float = (outer_ratio - inner_ratio) * planet_radius
	var band_width: float = total_width / float(band_count) * 0.7
	for i in range(band_count):
		var band_inner: float = planet_radius * inner_ratio + (total_width / float(band_count)) * float(i)
		var band_outer: float = band_inner + band_width
		var band: RingBand = RingBand.new(
			band_inner,
			band_outer,
			0.5,
			{"water_ice": 0.7, "silicates": 0.3},
			1.0,
			"Band %d" % i
		)
		ring_system.add_band(band)
	return ring_system
