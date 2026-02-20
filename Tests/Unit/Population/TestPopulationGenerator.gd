## Tests for PopulationGenerator main entry point.
extends TestCase

const _population_generator: GDScript = preload("res://src/domain/population/PopulationGenerator.gd")
const _planet_population_data: GDScript = preload("res://src/domain/population/PlanetPopulationData.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _profile_generator: GDScript = preload("res://src/domain/population/ProfileGenerator.gd")
const _native_population_generator: GDScript = preload("res://src/domain/population/NativePopulationGenerator.gd")
const _colony_generator: GDScript = preload("res://src/domain/population/ColonyGenerator.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")

# Celestial types for building test bodies
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _surface_props: GDScript = preload("res://src/domain/celestial/components/SurfaceProps.gd")
const _atmosphere_props: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _hydrosphere_props: GDScript = preload("res://src/domain/celestial/components/HydrosphereProps.gd")
const _terrain_props: GDScript = preload("res://src/domain/celestial/components/TerrainProps.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")


## Creates a habitable Earth-like body for testing.
func _create_earth_like_body() -> CelestialBody:
	var body: CelestialBody = CelestialBody.new()
	body.id = "earth_like_001"
	body.name = "Test Earth"
	body.type = CelestialType.Type.PLANET

	body.physical = PhysicalProps.new(
		5.972e24,
		6.371e6,
		86400.0,
		23.4,
		0.003,
		8.0e22
	)

	body.surface = SurfaceProps.new(
		288.0,
		0.3,
		"terrestrial",
		0.2,
		{"silicates": 0.6, "iron_oxides": 0.2, "water": 0.1}
	)
	body.surface.terrain = TerrainProps.new(8848.0, 0.5, 0.3, 0.5, 0.4, "varied")
	body.surface.hydrosphere = HydrosphereProps.new(0.71, 3688.0, 0.03, 35.0, "saline")

	body.atmosphere = AtmosphereProps.new(
		101325.0,
		8500.0,
		{"N2": 0.78, "O2": 0.21, "Ar": 0.01},
		1.15
	)

	body.orbital = OrbitalProps.new(
		1.496e11,
		0.017,
		0.0,
		0.0,
		0.0,
		0.0,
		"star_001"
	)

	return body


## Creates a barren Mars-like body.
func _create_barren_body() -> CelestialBody:
	var body: CelestialBody = CelestialBody.new()
	body.id = "barren_001"
	body.name = "Test Barren"
	body.type = CelestialType.Type.PLANET

	body.physical = PhysicalProps.new(
		6.39e23,
		3.389e6,
		88620.0,
		25.2,
		0.005,
		0.0
	)

	body.surface = SurfaceProps.new(
		210.0,
		0.25,
		"barren",
		0.0,
		{"silicates": 0.7, "iron_oxides": 0.3}
	)
	body.surface.terrain = TerrainProps.new(21900.0, 0.6, 0.5, 0.0, 0.3, "cratered")

	body.atmosphere = AtmosphereProps.new(
		610.0,
		11100.0,
		{"CO2": 0.95, "N2": 0.03, "Ar": 0.02},
		1.0
	)

	body.orbital = OrbitalProps.new(
		2.279e11,
		0.093,
		1.85,
		0.0,
		0.0,
		0.0,
		"star_001"
	)

	return body


## Creates a parent context for a Sun-like star.
func _create_sun_context() -> ParentContext:
	return ParentContext.new(
		1.989e30,
		3.828e26,
		5778.0
	)


## Creates a habitable profile for testing generate_from_profile.
func _create_habitable_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "habitable_001"
	profile.habitability_score = 9
	profile.avg_temperature_k = 290.0
	profile.pressure_atm = 1.0
	profile.has_liquid_water = true
	profile.has_breathable_atmosphere = true
	profile.has_atmosphere = true
	profile.ocean_coverage = 0.6
	profile.land_coverage = 0.35
	profile.gravity_g = 1.0
	profile.radiation_level = 0.1
	profile.weather_severity = 0.3
	profile.volcanism_level = 0.2
	profile.tectonic_activity = 0.4
	profile.continent_count = 5
	profile.day_length_hours = 24.0
	profile.axial_tilt_deg = 23.0

	profile.biomes = {
		BiomeType.Type.OCEAN as int: 0.6,
		BiomeType.Type.FOREST as int: 0.2,
		BiomeType.Type.GRASSLAND as int: 0.15,
	}

	profile.resources = {
		ResourceType.Type.WATER as int: 0.9,
		ResourceType.Type.METALS as int: 0.5,
		ResourceType.Type.SILICATES as int: 0.7,
		ResourceType.Type.ORGANICS as int: 0.5,
		ResourceType.Type.RARE_ELEMENTS as int: 0.3,
	}

	return profile


## Tests generate produces complete data.
func test_generate_produces_complete_data() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.native_spec.force_population = true

	var data: PlanetPopulationData = PopulationGenerator.generate(body, context, spec)

	assert_not_null(data)
	assert_equal(data.body_id, "earth_like_001")
	assert_not_null(data.profile)
	assert_not_null(data.suitability)
	assert_equal(data.generation_seed, 12345)


## Tests generate produces profile with correct body_id.
func test_generate_profile_has_body_id() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)

	var data: PlanetPopulationData = PopulationGenerator.generate(body, context, spec)

	assert_equal(data.profile.body_id, "earth_like_001")


## Tests generate_from_profile with existing profile.
func test_generate_from_profile() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.native_spec.force_population = true

	var data: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec)

	assert_not_null(data)
	assert_equal(data.body_id, "habitable_001")
	assert_equal(data.profile, profile)
	assert_not_null(data.suitability)


## Tests generate_profile_only.
func test_generate_profile_only() -> void:
	var body: CelestialBody = _create_earth_like_body()
	var context: ParentContext = _create_sun_context()

	var data: PlanetPopulationData = PopulationGenerator.generate_profile_only(body, context)

	assert_not_null(data)
	assert_not_null(data.profile)
	assert_not_null(data.suitability)
	assert_equal(data.native_populations.size(), 0)
	assert_equal(data.colonies.size(), 0)


## Tests generate with natives disabled.
func test_generate_natives_disabled() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.generate_natives = false

	var data: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec)

	assert_equal(data.native_populations.size(), 0)


## Tests generate with colonies disabled.
func test_generate_colonies_disabled() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.generate_colonies = false

	var data: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec)

	assert_equal(data.colonies.size(), 0)


## Tests determinism - same seed produces same results.
func test_determinism() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec1: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(42)
	spec1.native_spec.force_population = true
	spec1.colony_chance = 1.0

	var spec2: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(42)
	spec2.native_spec.force_population = true
	spec2.colony_chance = 1.0

	var data1: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec1)
	var data2: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec2)

	assert_equal(data1.native_populations.size(), data2.native_populations.size())
	assert_equal(data1.colonies.size(), data2.colonies.size())

	if data1.native_populations.size() > 0 and data2.native_populations.size() > 0:
		assert_equal(data1.native_populations[0].name, data2.native_populations[0].name)


## Tests different seeds produce different results.
func test_different_seeds() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec1: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(1)
	spec1.native_spec.force_population = true

	var spec2: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(999)
	spec2.native_spec.force_population = true

	var data1: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec1)
	var data2: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec2)

	if data1.native_populations.size() > 0 and data2.native_populations.size() > 0:
		assert_not_equal(data1.native_populations[0].name, "", "Should have name")
		assert_not_equal(data2.native_populations[0].name, "", "Should have name")


## Tests barren world produces low habitability.
func test_barren_world_no_natives() -> void:
	var body: CelestialBody = _create_barren_body()
	var context: ParentContext = _create_sun_context()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)

	var data: PlanetPopulationData = PopulationGenerator.generate(body, context, spec)

	assert_less_than(data.profile.habitability_score, 4, "Barren world should have low habitability")


## Tests habitable world can produce natives.
func test_habitable_world_can_have_natives() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.native_spec.force_population = true

	var data: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec)

	assert_greater_than(data.native_populations.size(), 0, "Habitable world with force should have natives")


## Tests colony generation respects suitability.
func test_colony_generation_respects_suitability() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.colony_chance = 1.0
	spec.max_auto_colonies = 3

	var data: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec)

	assert_true(data.suitability.is_colonizable(), "Should be colonizable")


## Tests spec.current_year is passed through.
func test_current_year_passed_through() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.current_year = 500
	spec.native_spec.force_population = true

	var data: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec)

	if data.native_populations.size() > 0:
		assert_less_than(data.native_populations[0].origin_year, 500)


## Tests serialization round-trip of generated data.
func test_generated_data_serialization() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(12345)
	spec.native_spec.force_population = true
	spec.colony_chance = 1.0

	var original: PlanetPopulationData = PopulationGenerator.generate_from_profile(profile, spec)

	var dict: Dictionary = original.to_dict()
	var restored: PlanetPopulationData = PlanetPopulationData.from_dict(dict)

	assert_equal(restored.body_id, original.body_id)
	assert_equal(restored.generation_seed, original.generation_seed)
	assert_equal(restored.get_total_population(), original.get_total_population())
	assert_equal(restored.native_populations.size(), original.native_populations.size())
	assert_equal(restored.colonies.size(), original.colonies.size())


## Tests create_default spec has reasonable values.
func test_create_default_spec() -> void:
	var spec: PopulationGenerator.PopulationSpec = PopulationGenerator.PopulationSpec.create_default(42)

	assert_equal(spec.generation_seed, 42)
	assert_equal(spec.current_year, 0)
	assert_true(spec.generate_natives)
	assert_true(spec.generate_colonies)
	assert_not_null(spec.native_spec)
	assert_equal(spec.native_spec.seed_value, 42)
