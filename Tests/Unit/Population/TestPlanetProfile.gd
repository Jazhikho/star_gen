## Tests for PlanetProfile data model.
extends TestCase

const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Creates a minimal test profile.
func _create_test_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "test_planet_001"
	profile.habitability_score = 7
	profile.avg_temperature_k = 288.0
	profile.pressure_atm = 1.0
	profile.ocean_coverage = 0.7
	profile.land_coverage = 0.25
	profile.ice_coverage = 0.05
	profile.continent_count = 5
	profile.max_elevation_km = 8.8
	profile.day_length_hours = 24.0
	profile.axial_tilt_deg = 23.4
	profile.gravity_g = 1.0
	profile.tectonic_activity = 0.5
	profile.volcanism_level = 0.3
	profile.weather_severity = 0.4
	profile.magnetic_field_strength = 0.8
	profile.radiation_level = 0.2
	profile.albedo = 0.3
	profile.greenhouse_factor = 1.15
	profile.is_tidally_locked = false
	profile.has_atmosphere = true
	profile.has_magnetic_field = true
	profile.has_liquid_water = true
	profile.has_breathable_atmosphere = true
	profile.is_moon = false

	# Add climate zones
	profile.climate_zones = [
		{"zone": ClimateZone.Zone.POLAR, "coverage": 0.1},
		{"zone": ClimateZone.Zone.TEMPERATE, "coverage": 0.5},
		{"zone": ClimateZone.Zone.TROPICAL, "coverage": 0.4},
	]

	# Add biomes
	profile.biomes = {
		BiomeType.Type.OCEAN: 0.7,
		BiomeType.Type.FOREST: 0.15,
		BiomeType.Type.GRASSLAND: 0.1,
		BiomeType.Type.ICE_SHEET: 0.05,
	}

	# Add resources
	profile.resources = {
		ResourceType.Type.WATER: 0.9,
		ResourceType.Type.METALS: 0.4,
		ResourceType.Type.ORGANICS: 0.6,
	}

	return profile


## Tests basic profile creation.
func test_creation() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	assert_equal(profile.body_id, "")
	assert_equal(profile.habitability_score, 0)
	assert_equal(profile.avg_temperature_k, 0.0)


## Tests habitability category derivation.
func test_get_habitability_category() -> void:
	var profile: PlanetProfile = PlanetProfile.new()

	profile.habitability_score = 0
	assert_equal(profile.get_habitability_category(), HabitabilityCategory.Category.IMPOSSIBLE)

	profile.habitability_score = 5
	assert_equal(profile.get_habitability_category(), HabitabilityCategory.Category.MARGINAL)

	profile.habitability_score = 10
	assert_equal(profile.get_habitability_category(), HabitabilityCategory.Category.IDEAL)


## Tests habitability category string.
func test_get_habitability_category_string() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 7
	assert_equal(profile.get_habitability_category_string(), "Challenging")


## Tests temperature conversion.
func test_get_temperature_celsius() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.avg_temperature_k = 288.0
	assert_float_equal(profile.get_temperature_celsius(), 14.85, 0.01)

	profile.avg_temperature_k = 273.15
	assert_float_equal(profile.get_temperature_celsius(), 0.0, 0.01)


## Tests habitable surface calculation.
func test_get_habitable_surface() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.land_coverage = 0.3
	profile.ice_coverage = 0.1

	# Habitable = land - (ice * 0.5)
	assert_float_equal(profile.get_habitable_surface(), 0.25, 0.001)


## Tests habitable surface with no ice.
func test_get_habitable_surface_no_ice() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.land_coverage = 0.4
	profile.ice_coverage = 0.0

	assert_float_equal(profile.get_habitable_surface(), 0.4, 0.001)


## Tests dominant biome detection.
func test_get_dominant_biome() -> void:
	var profile: PlanetProfile = _create_test_profile()
	assert_equal(profile.get_dominant_biome(), BiomeType.Type.OCEAN)


## Tests dominant biome with no biomes.
func test_get_dominant_biome_empty() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	assert_equal(profile.get_dominant_biome(), BiomeType.Type.BARREN)


## Tests primary resource detection.
func test_get_primary_resource() -> void:
	var profile: PlanetProfile = _create_test_profile()
	assert_equal(profile.get_primary_resource(), ResourceType.Type.WATER)


## Tests primary resource with no resources.
func test_get_primary_resource_empty() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	assert_equal(profile.get_primary_resource(), ResourceType.Type.SILICATES)


## Tests can_support_native_life for habitable world.
func test_can_support_native_life_habitable() -> void:
	var profile: PlanetProfile = _create_test_profile()
	assert_true(profile.can_support_native_life())


## Tests can_support_native_life for barren world.
func test_can_support_native_life_barren() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 1
	profile.has_liquid_water = false
	profile.ocean_coverage = 0.0
	profile.avg_temperature_k = 150.0
	profile.pressure_atm = 0.0

	assert_false(profile.can_support_native_life())


## Tests can_support_native_life requires minimum habitability.
func test_can_support_native_life_requires_min_hab() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 2
	profile.has_liquid_water = true
	profile.ocean_coverage = 0.5
	profile.avg_temperature_k = 290.0
	profile.pressure_atm = 1.0

	# Score is 2, minimum is 3
	assert_false(profile.can_support_native_life())


## Tests is_colonizable for various scores.
func test_is_colonizable() -> void:
	var profile: PlanetProfile = PlanetProfile.new()

	profile.habitability_score = 0
	assert_false(profile.is_colonizable())

	profile.habitability_score = 1
	assert_true(profile.is_colonizable())

	profile.habitability_score = 5
	assert_true(profile.is_colonizable())

	profile.habitability_score = 10
	assert_true(profile.is_colonizable())


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: PlanetProfile = _create_test_profile()

	var data: Dictionary = original.to_dict()
	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.body_id, original.body_id)
	assert_equal(restored.habitability_score, original.habitability_score)
	assert_float_equal(restored.avg_temperature_k, original.avg_temperature_k, 0.001)
	assert_float_equal(restored.pressure_atm, original.pressure_atm, 0.001)
	assert_float_equal(restored.ocean_coverage, original.ocean_coverage, 0.001)
	assert_float_equal(restored.land_coverage, original.land_coverage, 0.001)
	assert_float_equal(restored.ice_coverage, original.ice_coverage, 0.001)
	assert_equal(restored.continent_count, original.continent_count)
	assert_float_equal(restored.day_length_hours, original.day_length_hours, 0.001)
	assert_float_equal(restored.gravity_g, original.gravity_g, 0.001)
	assert_equal(restored.is_tidally_locked, original.is_tidally_locked)
	assert_equal(restored.has_atmosphere, original.has_atmosphere)
	assert_equal(restored.has_liquid_water, original.has_liquid_water)
	assert_equal(restored.is_moon, original.is_moon)


## Tests climate zones serialization.
func test_climate_zones_serialization() -> void:
	var original: PlanetProfile = _create_test_profile()

	var data: Dictionary = original.to_dict()
	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.climate_zones.size(), original.climate_zones.size())
	for i in range(original.climate_zones.size()):
		assert_equal(restored.climate_zones[i]["zone"], original.climate_zones[i]["zone"])
		assert_float_equal(
			restored.climate_zones[i]["coverage"] as float,
			original.climate_zones[i]["coverage"] as float,
			0.001
		)


## Tests biomes serialization.
func test_biomes_serialization() -> void:
	var original: PlanetProfile = _create_test_profile()

	var data: Dictionary = original.to_dict()
	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.biomes.size(), original.biomes.size())
	for biome in original.biomes.keys():
		assert_true(restored.biomes.has(biome))
		assert_float_equal(
			restored.biomes[biome] as float,
			original.biomes[biome] as float,
			0.001
		)


## Tests resources serialization.
func test_resources_serialization() -> void:
	var original: PlanetProfile = _create_test_profile()

	var data: Dictionary = original.to_dict()
	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.resources.size(), original.resources.size())
	for resource in original.resources.keys():
		assert_true(restored.resources.has(resource))
		assert_float_equal(
			restored.resources[resource] as float,
			original.resources[resource] as float,
			0.001
		)


## Tests moon-specific fields serialization.
func test_moon_fields_serialization() -> void:
	var original: PlanetProfile = PlanetProfile.new()
	original.is_moon = true
	original.tidal_heating_factor = 0.7
	original.parent_radiation_exposure = 0.4
	original.eclipse_factor = 0.2

	var data: Dictionary = original.to_dict()
	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.is_moon, true)
	assert_float_equal(restored.tidal_heating_factor, 0.7, 0.001)
	assert_float_equal(restored.parent_radiation_exposure, 0.4, 0.001)
	assert_float_equal(restored.eclipse_factor, 0.2, 0.001)


## Tests empty profile serialization.
func test_empty_profile_serialization() -> void:
	var original: PlanetProfile = PlanetProfile.new()

	var data: Dictionary = original.to_dict()
	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.body_id, "")
	assert_equal(restored.habitability_score, 0)
	assert_equal(restored.climate_zones.size(), 0)
	assert_equal(restored.biomes.size(), 0)
	assert_equal(restored.resources.size(), 0)


## Tests from_dict handles JSON-style string keys for biomes and resources.
func test_from_dict_json_string_keys() -> void:
	var original: PlanetProfile = _create_test_profile()
	var data: Dictionary = original.to_dict()

	# Simulate JSON: keys become strings
	var json_like_biomes: Dictionary = {}
	for key in data["biomes"].keys():
		json_like_biomes[str(key)] = data["biomes"][key]
	data["biomes"] = json_like_biomes

	var json_like_resources: Dictionary = {}
	for key in data["resources"].keys():
		json_like_resources[str(key)] = data["resources"][key]
	data["resources"] = json_like_resources

	var restored: PlanetProfile = PlanetProfile.from_dict(data)

	assert_equal(restored.biomes.size(), original.biomes.size())
	for biome_type in original.biomes.keys():
		assert_true(restored.biomes.has(biome_type))
		assert_float_equal(
			restored.biomes[biome_type] as float,
			original.biomes[biome_type] as float,
			0.001
		)

	assert_equal(restored.resources.size(), original.resources.size())
	for resource_type in original.resources.keys():
		assert_true(restored.resources.has(resource_type))
		assert_float_equal(
			restored.resources[resource_type] as float,
			original.resources[resource_type] as float,
			0.001
		)
