## Unit tests for TerrestrialShaderParams (spatial shader param derivation).
extends "res://Tests/Framework/TestCase.gd"

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func get_test_name() -> String:
	return "TerrestrialShaderParams"


## Creates a test planet with specified properties.
func _create_test_planet(
	surface_type: String = "continental",
	temp_k: float = 288.0,
	has_ocean: bool = true,
	has_ice: bool = true,
	has_atmosphere: bool = true,
	seed_val: int = 12345
) -> CelestialBody:
	var body: CelestialBody = CelestialBody.new()
	body.type = CelestialType.Type.PLANET
	body.name = "Test Planet"

	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 5.972e24
	body.physical.radius_m = 6.371e6
	body.physical.rotation_period_s = 86400.0
	body.physical.axial_tilt_deg = 23.4

	body.surface = SurfaceProps.new()
	body.surface.temperature_k = temp_k
	body.surface.albedo = 0.3
	body.surface.surface_type = surface_type
	body.surface.volcanism_level = 0.2
	body.surface.surface_composition = {"silicates": 0.6, "iron_oxides": 0.2, "water": 0.2}

	body.surface.terrain = TerrainProps.new()
	body.surface.terrain.elevation_range_m = 20000.0
	body.surface.terrain.roughness = 0.5
	body.surface.terrain.crater_density = 0.1
	body.surface.terrain.tectonic_activity = 0.4
	body.surface.terrain.erosion_level = 0.3

	if has_ocean:
		body.surface.hydrosphere = HydrosphereProps.new()
		body.surface.hydrosphere.ocean_coverage = 0.7
		body.surface.hydrosphere.ocean_depth_m = 3700.0
		body.surface.hydrosphere.ice_coverage = 0.1
		body.surface.hydrosphere.water_type = "water"

	if has_ice:
		body.surface.cryosphere = CryosphereProps.new()
		body.surface.cryosphere.polar_cap_coverage = 0.3
		body.surface.cryosphere.ice_type = "water_ice"

	if has_atmosphere:
		body.atmosphere = AtmosphereProps.new()
		body.atmosphere.surface_pressure_pa = 101325.0
		body.atmosphere.scale_height_m = 8500.0
		body.atmosphere.composition = {"N2": 0.78, "O2": 0.21, "H2O": 0.01}
		body.atmosphere.greenhouse_factor = 1.15

	body.provenance = Provenance.new()
	body.provenance.generation_seed = seed_val

	return body


func test_earth_like_params() -> void:
	var body: CelestialBody = _create_test_planet()
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	assert_true(params.has("u_terrainScale"), "Should have terrain scale")
	assert_true(params.has("u_seaLevel"), "Should have sea level")
	assert_true(params.has("u_iceCap"), "Should have ice cap")
	assert_true(params.has("u_atmoDensity"), "Should have atmo density")

	var sea_level: float = params["u_seaLevel"] as float
	assert_true(sea_level > 0.3 and sea_level < 0.8, "Sea level should be moderate for 70% ocean")

	var ice_cap: float = params["u_iceCap"] as float
	assert_almost_equal(ice_cap, 0.3, 0.01, "Ice cap should match cryosphere coverage")


func test_desert_planet_params() -> void:
	var body: CelestialBody = _create_test_planet("desert", 320.0, false, false, true)
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var sea_level: float = params["u_seaLevel"] as float
	assert_true(sea_level < 0.1, "Desert planet should have very low sea level")

	var ice_cap: float = params["u_iceCap"] as float
	assert_true(ice_cap < 0.1, "Hot desert should have minimal ice")

	var col_low: Color = params["u_colLow"] as Color
	assert_true(col_low.r > col_low.g and col_low.g > col_low.b, "Desert color should be warm")


func test_ice_world_params() -> void:
	var body: CelestialBody = _create_test_planet("frozen", 180.0, false, true, false)
	body.surface.cryosphere.polar_cap_coverage = 0.9
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var ice_cap: float = params["u_iceCap"] as float
	assert_true(ice_cap > 0.8, "Ice world should have high ice cap coverage")

	var atmo_density: float = params["u_atmoDensity"] as float
	assert_almost_equal(atmo_density, 0.0, 0.01, "No atmosphere should have zero density")


func test_ocean_world_params() -> void:
	var body: CelestialBody = _create_test_planet()
	body.surface.hydrosphere.ocean_coverage = 0.95
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var sea_level: float = params["u_seaLevel"] as float
	assert_true(sea_level > 0.6, "Ocean world should have high sea level")


func test_venus_like_params() -> void:
	var body: CelestialBody = _create_test_planet("volcanic", 735.0, false, false, true)
	body.atmosphere.surface_pressure_pa = 9200000.0
	body.atmosphere.composition = {"CO2": 0.96, "N2": 0.03, "SO2": 0.01}
	body.atmosphere.greenhouse_factor = 3.5
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var atmo_density: float = params["u_atmoDensity"] as float
	assert_true(atmo_density >= 1.5, "Venus-like should have high atmo density")

	var cloud_coverage: float = params["u_cloudCoverage"] as float
	assert_true(cloud_coverage > 0.6, "Venus-like should have thick clouds")

	var atmo_color: Color = params["u_atmoColor"] as Color
	assert_true(atmo_color.r > atmo_color.b, "Greenhouse atmosphere should be warm-tinted")


func test_mars_like_params() -> void:
	var body: CelestialBody = _create_test_planet("rocky", 210.0, false, true, true)
	body.atmosphere.surface_pressure_pa = 610.0
	body.atmosphere.composition = {"CO2": 0.95, "N2": 0.03}
	body.surface.cryosphere.polar_cap_coverage = 0.15
	body.surface.cryosphere.ice_type = "co2_ice"
	body.surface.surface_composition = {"iron_oxides": 0.5, "silicates": 0.5}
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var atmo_density: float = params["u_atmoDensity"] as float
	assert_true(atmo_density < 0.1, "Mars-like should have very thin atmosphere")

	var cloud_coverage: float = params["u_cloudCoverage"] as float
	assert_true(cloud_coverage < 0.2, "Mars-like should have minimal clouds")

	var col_low: Color = params["u_colLow"] as Color
	assert_true(col_low.r > col_low.g, "Mars-like surface should be reddish")


func test_determinism_same_seed() -> void:
	var body1: CelestialBody = _create_test_planet("continental", 288.0, true, true, true, 42)
	var body2: CelestialBody = _create_test_planet("continental", 288.0, true, true, true, 42)

	var params1: Dictionary = TerrestrialShaderParams.get_params(body1)
	var params2: Dictionary = TerrestrialShaderParams.get_params(body2)

	assert_equal(params1["u_seed"], params2["u_seed"], "Same seed should produce same u_seed")


func test_determinism_different_seed() -> void:
	var body1: CelestialBody = _create_test_planet("continental", 288.0, true, true, true, 42)
	var body2: CelestialBody = _create_test_planet("continental", 288.0, true, true, true, 999)

	var params1: Dictionary = TerrestrialShaderParams.get_params(body1)
	var params2: Dictionary = TerrestrialShaderParams.get_params(body2)

	assert_not_equal(params1["u_seed"], params2["u_seed"], "Different seeds should produce different u_seed")


func test_is_terrestrial_suitable_true() -> void:
	var body: CelestialBody = _create_test_planet()
	assert_true(TerrestrialShaderParams.is_terrestrial_suitable(body), "Earth-like planet should be suitable")


func test_is_terrestrial_suitable_false_no_surface() -> void:
	var body: CelestialBody = CelestialBody.new()
	body.type = CelestialType.Type.PLANET
	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 5.972e24

	assert_false(TerrestrialShaderParams.is_terrestrial_suitable(body), "Planet without surface should not be suitable")


func test_is_terrestrial_suitable_false_gas_giant() -> void:
	var body: CelestialBody = CelestialBody.new()
	body.type = CelestialType.Type.PLANET
	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 1.898e27
	body.surface = SurfaceProps.new()
	body.surface.surface_type = "gaseous"

	assert_false(TerrestrialShaderParams.is_terrestrial_suitable(body), "Gas giant should not be suitable")


func test_methane_ocean_colors() -> void:
	var body: CelestialBody = _create_test_planet()
	body.surface.hydrosphere.water_type = "methane"
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var sea_shallow: Color = params["u_colSeaShallow"] as Color
	var sea_deep: Color = params["u_colSeaDeep"] as Color

	assert_true(sea_shallow.r > sea_shallow.b, "Methane sea should have warm tones")


func test_axial_tilt_passed() -> void:
	var body: CelestialBody = _create_test_planet()
	body.physical.axial_tilt_deg = 45.0
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var tilt: float = params["u_axialTilt"] as float
	assert_almost_equal(tilt, 45.0, 0.1, "Axial tilt should be passed through")


func test_cloud_coverage_no_water() -> void:
	var body: CelestialBody = _create_test_planet("rocky", 288.0, false, false, true)
	body.atmosphere.composition = {"N2": 0.9, "CO2": 0.1}
	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	var cloud_coverage: float = params["u_cloudCoverage"] as float
	assert_true(cloud_coverage < 0.2, "No water vapor should mean minimal clouds")
