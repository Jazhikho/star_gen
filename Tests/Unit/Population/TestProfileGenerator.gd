## Tests for ProfileGenerator.
extends TestCase

const _profile_generator: GDScript = preload("res://src/domain/population/ProfileGenerator.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Creates a minimal Earth-like planet for testing.
func _create_earth_like_body() -> CelestialBody:
	var body: CelestialBody = CelestialBody.new(
		"test_earth",
		"Test Earth",
		CelestialType.Type.PLANET
	)

	# Physical properties
	body.physical = PhysicalProps.new(
		5.972e24, # Earth mass
		6.371e6 # Earth radius
	)
	body.physical.rotation_period_s = 86400.0 # 24 hours
	body.physical.axial_tilt_deg = 23.4
	body.physical.magnetic_moment = 8.0e22
	body.physical.internal_heat_watts = 4.7e13

	# Orbital properties
	body.orbital = OrbitalProps.new()
	body.orbital.semi_major_axis_m = 1.496e11 # 1 AU
	body.orbital.eccentricity = 0.017

	# Atmosphere
	body.atmosphere = AtmosphereProps.new(
		101325.0, # 1 atm
		8500.0, # Scale height
		{"N2": 0.78, "O2": 0.21, "Ar": 0.01},
		1.15 # Greenhouse factor
	)

	# Surface
	body.surface = SurfaceProps.new(
		288.0, # ~15°C
		0.3, # Albedo
		"continental",
		0.3, # Volcanism
		{"silicates": 0.6, "iron_oxides": 0.2, "carbonates": 0.1, "water": 0.1}
	)

	# Terrain
	body.surface.terrain = TerrainProps.new(
		20000.0, # 20km elevation range
		0.5,
		0.2,
		0.5, # Tectonic activity
		0.4,
		"tectonic"
	)

	# Hydrosphere
	body.surface.hydrosphere = HydrosphereProps.new(
		0.71, # 71% ocean
		3800.0, # Average depth
		0.03, # Ice coverage on water
		35.0, # Salinity
		"water"
	)

	# Cryosphere
	body.surface.cryosphere = CryosphereProps.new(
		0.03, # Polar caps
		100.0, # Permafrost depth
		false, # No subsurface ocean
		0.0,
		0.0,
		"water_ice"
	)

	return body


## Creates a Mars-like planet for testing.
func _create_mars_like_body() -> CelestialBody:
	var body: CelestialBody = CelestialBody.new(
		"test_mars",
		"Test Mars",
		CelestialType.Type.PLANET
	)

	# Physical properties
	body.physical = PhysicalProps.new(
		6.39e23, # Mars mass
		3.389e6 # Mars radius
	)
	body.physical.rotation_period_s = 88620.0 # ~24.6 hours
	body.physical.axial_tilt_deg = 25.2
	body.physical.magnetic_moment = 0.0 # No global magnetic field
	body.physical.internal_heat_watts = 1.0e12

	# Orbital
	body.orbital = OrbitalProps.new()
	body.orbital.semi_major_axis_m = 2.279e11 # 1.52 AU
	body.orbital.eccentricity = 0.093

	# Very thin atmosphere
	body.atmosphere = AtmosphereProps.new(
		610.0, # 0.006 atm
		11000.0,
		{"CO2": 0.95, "N2": 0.03, "Ar": 0.02},
		1.02
	)

	# Surface
	body.surface = SurfaceProps.new(
		210.0, # ~-63°C
		0.25,
		"rocky_cold",
		0.1,
		{"silicates": 0.5, "iron_oxides": 0.35, "sulfur_compounds": 0.15}
	)

	body.surface.terrain = TerrainProps.new(
		30000.0, # Olympus Mons!
		0.6,
		0.4,
		0.05, # Low tectonics
		0.2,
		"volcanic"
	)

	body.surface.cryosphere = CryosphereProps.new(
		0.15, # Polar caps
		500.0,
		false,
		0.0,
		0.0,
		"co2_ice"
	)

	return body


## Creates a Europa-like moon for testing.
func _create_europa_like_body() -> CelestialBody:
	var body: CelestialBody = CelestialBody.new(
		"test_europa",
		"Test Europa",
		CelestialType.Type.MOON
	)

	# Physical
	body.physical = PhysicalProps.new(
		4.8e22, # Europa mass
		1.561e6 # Europa radius
	)
	body.physical.rotation_period_s = 306720.0 # Tidally locked
	body.physical.axial_tilt_deg = 0.1
	body.physical.magnetic_moment = 0.0
	body.physical.internal_heat_watts = 1.0e12 # Tidal heating

	# Orbital (around Jupiter)
	body.orbital = OrbitalProps.new()
	body.orbital.semi_major_axis_m = 6.709e8
	body.orbital.eccentricity = 0.009

	# No atmosphere
	body.atmosphere = null

	# Surface
	body.surface = SurfaceProps.new(
		102.0, # Very cold
		0.67, # High albedo (ice)
		"icy",
		0.0,
		{"water_ice": 0.95, "silicates": 0.05}
	)

	body.surface.terrain = TerrainProps.new(
		200.0,
		0.3,
		0.1,
		0.0,
		0.1,
		"icy"
	)

	body.surface.cryosphere = CryosphereProps.new(
		1.0, # All ice
		20000.0, # Thick ice shell
		true, # Subsurface ocean!
		100000.0, # Deep ocean
		0.3, # Cryovolcanism
		"water_ice"
	)

	return body


## Creates a Jupiter-like parent body.
func _create_jupiter_parent() -> CelestialBody:
	var body: CelestialBody = CelestialBody.new(
		"test_jupiter",
		"Test Jupiter",
		CelestialType.Type.PLANET
	)

	body.physical = PhysicalProps.new(
		1.898e27, # Jupiter mass
		6.991e7 # Jupiter radius
	)
	body.physical.magnetic_moment = 1.5e20
	body.physical.rotation_period_s = 35760.0 # ~10 hours

	body.orbital = OrbitalProps.new()
	body.orbital.semi_major_axis_m = 7.785e11 # 5.2 AU

	return body


## Creates a sun-like context.
func _create_sun_context() -> ParentContext:
	return ParentContext.sun_like()


## Tests profile generation for Earth-like planet.
func test_generate_earth_like() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var profile: PlanetProfile = ProfileGenerator.generate(body, context)

	assert_equal(profile.body_id, "test_earth")
	assert_false(profile.is_moon)
	assert_true(profile.has_atmosphere)
	assert_true(profile.has_magnetic_field)
	assert_true(profile.has_liquid_water)
	assert_true(profile.has_breathable_atmosphere)

	# Habitability should be high
	assert_in_range(profile.habitability_score, 8, 10, "Earth-like should score 8-10")

	# Physical properties
	assert_float_equal(profile.gravity_g, 1.0, 0.1, "Gravity should be ~1g")
	assert_float_equal(profile.day_length_hours, 24.0, 0.1, "Day should be ~24h")
	assert_float_equal(profile.pressure_atm, 1.0, 0.01, "Pressure should be ~1 atm")


## Tests profile generation for Mars-like planet.
func test_generate_mars_like() -> void:
	var body: CelestialBody = _create_mars_like_body()
	var context: ParentContext = _create_sun_context()

	var profile: PlanetProfile = ProfileGenerator.generate(body, context)

	assert_equal(profile.body_id, "test_mars")
	assert_true(profile.has_atmosphere) # Very thin but exists
	assert_false(profile.has_magnetic_field)
	assert_false(profile.has_liquid_water)
	assert_false(profile.has_breathable_atmosphere)

	# Habitability should be low
	assert_in_range(profile.habitability_score, 0, 3, "Mars-like should score 0-3")

	# Higher radiation due to no magnetic field
	assert_greater_than(profile.radiation_level, 0.5, "Mars should have high radiation")


## Tests profile generation for Europa-like moon.
func test_generate_europa_like() -> void:
	var body: CelestialBody = _create_europa_like_body()
	var parent: CelestialBody = _create_jupiter_parent()
	var context: ParentContext = _create_sun_context()

	var profile: PlanetProfile = ProfileGenerator.generate(body, context, parent)

	assert_equal(profile.body_id, "test_europa")
	assert_true(profile.is_moon)
	assert_false(profile.has_atmosphere)
	assert_true(profile.has_liquid_water) # Subsurface ocean

	# Moon-specific properties
	assert_greater_than(profile.tidal_heating_factor, 0.0, "Should have tidal heating")
	assert_greater_than(profile.parent_radiation_exposure, 0.0, "Should have parent radiation")


## Tests climate zones are generated.
func test_climate_zones_generated() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var profile: PlanetProfile = ProfileGenerator.generate(body, context)

	assert_greater_than(profile.climate_zones.size(), 0, "Should have climate zones")

	var total: float = 0.0
	for zone_data in profile.climate_zones:
		total += zone_data["coverage"] as float
	assert_float_equal(total, 1.0, 0.01, "Climate zone coverage should sum to 1.0")


## Tests biomes are generated.
func test_biomes_generated() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var profile: PlanetProfile = ProfileGenerator.generate(body, context)

	assert_greater_than(profile.biomes.size(), 0, "Should have biomes")

	# Should have ocean biome for Earth-like
	var ocean_key: int = BiomeType.Type.OCEAN as int
	assert_true(profile.biomes.has(ocean_key), "Should have ocean biome")


## Tests resources are generated.
func test_resources_generated() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var profile: PlanetProfile = ProfileGenerator.generate(body, context)

	assert_greater_than(profile.resources.size(), 0, "Should have resources")

	# Should have water resource for Earth-like
	var water_key: int = ResourceType.Type.WATER as int
	assert_true(profile.resources.has(water_key), "Should have water resource")


## Tests continent count is estimated.
func test_continent_count() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var profile: PlanetProfile = ProfileGenerator.generate(body, context)

	# Earth-like should have multiple continents
	assert_greater_than(profile.continent_count, 0, "Should have continents")


## Tests tidal locking detection.
func test_tidal_locking_detection() -> void:
	var body: CelestialBody = _create_europa_like_body()
	var parent: CelestialBody = _create_jupiter_parent()
	var context: ParentContext = _create_sun_context()

	# Make orbital period match rotation period
	var orbital_period: float = body.orbital.get_orbital_period_s(parent.physical.mass_kg)
	body.physical.rotation_period_s = orbital_period

	var profile: PlanetProfile = ProfileGenerator.generate(body, context, parent)

	assert_true(profile.is_tidally_locked, "Europa-like should be tidally locked")


## Tests determinism - same input gives same output.
func test_determinism() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var profile1: PlanetProfile = ProfileGenerator.generate(body, context)
	var profile2: PlanetProfile = ProfileGenerator.generate(body, context)

	assert_equal(profile1.habitability_score, profile2.habitability_score)
	assert_float_equal(profile1.avg_temperature_k, profile2.avg_temperature_k, 0.001)
	assert_float_equal(profile1.pressure_atm, profile2.pressure_atm, 0.001)
	assert_float_equal(profile1.radiation_level, profile2.radiation_level, 0.001)
	assert_equal(profile1.climate_zones.size(), profile2.climate_zones.size())


## Tests body without surface generates valid profile.
func test_body_without_surface() -> void:
	var body: CelestialBody = CelestialBody.new(
		"test_gas_giant",
		"Test Gas Giant",
		CelestialType.Type.PLANET
	)
	body.physical = PhysicalProps.new(1.898e27, 6.991e7)
	body.physical.rotation_period_s = 35760.0
	body.atmosphere = AtmosphereProps.new(100000.0, 27000.0, {"H2": 0.9, "He": 0.1}, 1.0)

	var context: ParentContext = _create_sun_context()
	var profile: PlanetProfile = ProfileGenerator.generate(body, context)

	# Should not crash and should have low habitability
	assert_in_range(profile.habitability_score, 0, 2, "Gas giant should have very low habitability")
	assert_false(profile.has_liquid_water)


## Tests serialization round-trip preserves generated profile.
func test_serialization_round_trip() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var original: PlanetProfile = ProfileGenerator.generate(body, context)
	var data: Dictionary = original.to_dict()
	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.body_id, original.body_id)
	assert_equal(restored.habitability_score, original.habitability_score)
	assert_float_equal(restored.avg_temperature_k, original.avg_temperature_k, 0.001)
	assert_equal(restored.climate_zones.size(), original.climate_zones.size())
	assert_equal(restored.biomes.size(), original.biomes.size())
	assert_equal(restored.resources.size(), original.resources.size())
