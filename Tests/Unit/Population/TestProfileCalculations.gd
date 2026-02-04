## Tests for ProfileCalculations pure functions.
extends TestCase

const _profile_calculations: GDScript = preload("res://src/domain/population/ProfileCalculations.gd")
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Tests habitability score for Earth-like conditions.
func test_habitability_score_earth_like() -> void:
	var score: int = ProfileCalculations.calculate_habitability_score(
		288.0, # ~15°C
		1.0, # 1 atm
		1.0, # 1g
		true, # liquid water
		true, # breathable
		0.1, # low radiation
		0.7 # 70% ocean
	)
	# Should score high (8-10)
	assert_in_range(score, 8, 10, "Earth-like should score 8-10")


## Tests habitability score for Mars-like conditions.
func test_habitability_score_mars_like() -> void:
	var score: int = ProfileCalculations.calculate_habitability_score(
		210.0, # ~-63°C
		0.006, # Very thin atmosphere
		0.38, # Low gravity
		false, # No liquid water
		false, # Not breathable
		0.5, # Moderate radiation
		0.0 # No ocean
	)
	# Should score low (0-3)
	assert_in_range(score, 0, 3, "Mars-like should score 0-3")


## Tests habitability score for Venus-like conditions.
func test_habitability_score_venus_like() -> void:
	var score: int = ProfileCalculations.calculate_habitability_score(
		737.0, # ~464°C
		92.0, # 92 atm
		0.9, # Similar to Earth
		false, # No liquid water
		false, # Not breathable
		0.2, # Some radiation
		0.0 # No ocean
	)
	# Should score very low (0-2)
	assert_in_range(score, 0, 2, "Venus-like should score 0-2")


## Tests habitability score clamping.
func test_habitability_score_clamping() -> void:
	# Perfect conditions shouldn't exceed 10
	var score: int = ProfileCalculations.calculate_habitability_score(
		293.0, 1.0, 1.0, true, true, 0.0, 0.5
	)
	assert_in_range(score, 0, 10, "Score should be clamped to 0-10")


## Tests weather severity with no atmosphere.
func test_weather_severity_no_atmosphere() -> void:
	var severity: float = ProfileCalculations.calculate_weather_severity(0.0, 86400.0, false)
	assert_float_equal(severity, 0.0, 0.001, "No atmosphere = no weather")


## Tests weather severity with thick atmosphere.
func test_weather_severity_thick_atmosphere() -> void:
	var severity: float = ProfileCalculations.calculate_weather_severity(2.0, 86400.0, true)
	assert_greater_than(severity, 0.3, "Thick atmosphere should increase weather severity")


## Tests weather severity with fast rotation.
func test_weather_severity_fast_rotation() -> void:
	var normal: float = ProfileCalculations.calculate_weather_severity(1.0, 86400.0, true)
	var fast: float = ProfileCalculations.calculate_weather_severity(1.0, 21600.0, true) # 6 hour day
	assert_greater_than(fast, normal, "Fast rotation should increase weather severity")


## Tests magnetic field strength calculation.
func test_magnetic_strength() -> void:
	# Earth-like magnetic moment
	var earth_strength: float = ProfileCalculations.calculate_magnetic_strength(8.0e22)
	assert_float_equal(earth_strength, 1.0, 0.01, "Earth magnetic moment should give ~1.0")

	# Zero magnetic moment
	var zero_strength: float = ProfileCalculations.calculate_magnetic_strength(0.0)
	assert_float_equal(zero_strength, 0.0, 0.001, "Zero moment should give 0.0")

	# Half Earth
	var half_strength: float = ProfileCalculations.calculate_magnetic_strength(4.0e22)
	assert_float_equal(half_strength, 0.5, 0.01, "Half Earth moment should give ~0.5")


## Tests radiation level with full protection.
func test_radiation_level_protected() -> void:
	var radiation: float = ProfileCalculations.calculate_radiation_level(
		8.0e22, # Earth magnetic moment
		1.0, # 1 atm
		true # Has atmosphere
	)
	assert_less_than(radiation, 0.3, "Protected body should have low radiation")


## Tests radiation level with no protection.
func test_radiation_level_unprotected() -> void:
	var radiation: float = ProfileCalculations.calculate_radiation_level(
		0.0, # No magnetic field
		0.0, # No atmosphere
		false # No atmosphere
	)
	assert_float_equal(radiation, 1.0, 0.01, "Unprotected body should have max radiation")


## Tests continent count estimation.
func test_continent_count_no_land() -> void:
	var count: int = ProfileCalculations.estimate_continent_count(0.5, 0.0, true)
	assert_equal(count, 0, "No land should mean no continents")


## Tests continent count estimation with moderate land.
func test_continent_count_moderate_land() -> void:
	var count: int = ProfileCalculations.estimate_continent_count(0.5, 0.3, true)
	assert_in_range(count, 2, 6, "30% land with tectonics should have 2-6 continents")


## Tests continent count increases with tectonics.
func test_continent_count_tectonic_effect() -> void:
	var low_tec: int = ProfileCalculations.estimate_continent_count(0.1, 0.5, true)
	var high_tec: int = ProfileCalculations.estimate_continent_count(1.0, 0.5, true)
	assert_greater_than(high_tec, low_tec, "Higher tectonics should mean more continents")


## Tests climate zones for Earth-like world.
func test_climate_zones_earth_like() -> void:
	var zones: Array[Dictionary] = ProfileCalculations.calculate_climate_zones(
		23.5, # Earth tilt
		288.0, # Earth-like temp
		true # Has atmosphere
	)

	assert_greater_than(zones.size(), 1, "Should have multiple climate zones")

	# Check coverage sums to approximately 1
	var total: float = 0.0
	for zone_data in zones:
		total += zone_data["coverage"] as float
	assert_float_equal(total, 1.0, 0.01, "Zone coverage should sum to 1.0")


## Tests climate zones for frozen world.
func test_climate_zones_frozen() -> void:
	var zones: Array[Dictionary] = ProfileCalculations.calculate_climate_zones(
		23.5,
		150.0, # Very cold
		true
	)

	assert_equal(zones.size(), 1, "Frozen world should have single zone")
	assert_equal(zones[0]["zone"], ClimateZone.Zone.POLAR, "Should be polar zone")


## Tests climate zones for airless world.
func test_climate_zones_airless() -> void:
	var zones: Array[Dictionary] = ProfileCalculations.calculate_climate_zones(
		0.0,
		300.0,
		false # No atmosphere
	)

	assert_equal(zones.size(), 1, "Airless world should have single zone")
	assert_equal(zones[0]["zone"], ClimateZone.Zone.EXTREME, "Should be extreme zone")


## Tests biome calculation produces ocean biome.
func test_biomes_ocean() -> void:
	var zones: Array[Dictionary] = [ {"zone": ClimateZone.Zone.TEMPERATE, "coverage": 1.0}]
	var biomes: Dictionary = ProfileCalculations.calculate_biomes(
		zones, 0.7, 0.0, 0.0, true, true
	)

	var ocean_key: int = BiomeType.Type.OCEAN as int
	assert_true(biomes.has(ocean_key), "Should have ocean biome")
	assert_float_equal(biomes[ocean_key] as float, 0.7, 0.01, "Ocean should be 70%")


## Tests biome calculation for desert world.
func test_biomes_desert() -> void:
	var zones: Array[Dictionary] = [ {"zone": ClimateZone.Zone.ARID, "coverage": 1.0}]
	var biomes: Dictionary = ProfileCalculations.calculate_biomes(
		zones, 0.0, 0.0, 0.0, false, true
	)

	var desert_key: int = BiomeType.Type.DESERT as int
	assert_true(biomes.has(desert_key), "Should have desert biome")


## Tests biome calculation for volcanic world.
func test_biomes_volcanic() -> void:
	var zones: Array[Dictionary] = [ {"zone": ClimateZone.Zone.TEMPERATE, "coverage": 1.0}]
	var biomes: Dictionary = ProfileCalculations.calculate_biomes(
		zones, 0.0, 0.0, 0.8, false, true
	)

	var volcanic_key: int = BiomeType.Type.VOLCANIC as int
	assert_true(biomes.has(volcanic_key), "High volcanism should create volcanic biome")


## Tests resource calculation water abundance.
func test_resources_water() -> void:
	var biomes: Dictionary = {BiomeType.Type.OCEAN as int: 0.5}
	var resources: Dictionary = ProfileCalculations.calculate_resources(
		{}, biomes, 0.0, true, 0.5
	)

	var water_key: int = ResourceType.Type.WATER as int
	assert_true(resources.has(water_key), "Should have water resource")
	assert_greater_than(resources[water_key] as float, 0.5, "Should have significant water")


## Tests resource calculation metals from composition.
func test_resources_metals() -> void:
	var composition: Dictionary = {"iron_oxides": 0.3}
	var resources: Dictionary = ProfileCalculations.calculate_resources(
		composition, {}, 0.0, false, 0.0
	)

	var metals_key: int = ResourceType.Type.METALS as int
	assert_true(resources.has(metals_key), "Should have metals resource")


## Tests resource calculation rare elements from volcanism.
func test_resources_rare_elements() -> void:
	var resources: Dictionary = ProfileCalculations.calculate_resources(
		{}, {}, 0.7, false, 0.0
	)

	var rare_key: int = ResourceType.Type.RARE_ELEMENTS as int
	assert_true(resources.has(rare_key), "High volcanism should produce rare elements")


## Tests breathability check positive case.
func test_breathability_earth_like() -> void:
	var composition: Dictionary = {"N2": 0.78, "O2": 0.21, "Ar": 0.01}
	var breathable: bool = ProfileCalculations.check_breathability(composition, 1.0)
	assert_true(breathable, "Earth-like atmosphere should be breathable")


## Tests breathability check low oxygen.
func test_breathability_low_oxygen() -> void:
	var composition: Dictionary = {"N2": 0.95, "O2": 0.05}
	var breathable: bool = ProfileCalculations.check_breathability(composition, 1.0)
	assert_false(breathable, "Low oxygen should not be breathable")


## Tests breathability check toxic gas.
func test_breathability_toxic() -> void:
	var composition: Dictionary = {"N2": 0.60, "O2": 0.20, "H2S": 0.05}
	var breathable: bool = ProfileCalculations.check_breathability(composition, 1.0)
	assert_false(breathable, "Toxic gas should make atmosphere unbreathable")


## Tests breathability check low pressure.
func test_breathability_low_pressure() -> void:
	var composition: Dictionary = {"N2": 0.78, "O2": 0.21}
	var breathable: bool = ProfileCalculations.check_breathability(composition, 0.1)
	assert_false(breathable, "Low pressure should not be breathable")


## Tests tidal heating calculation.
func test_tidal_heating_io_like() -> void:
	var heating: float = ProfileCalculations.calculate_tidal_heating(
		1.898e27, # Jupiter mass
		4.217e8, # Io distance
		1.821e6, # Io radius
		0.0041 # Io eccentricity
	)
	# Should be significant (Io is the benchmark)
	assert_in_range(heating, 0.5, 1.0, "Io-like moon should have high tidal heating")


## Tests tidal heating with no eccentricity.
func test_tidal_heating_circular_orbit() -> void:
	var heating: float = ProfileCalculations.calculate_tidal_heating(
		1.898e27,
		4.217e8,
		1.821e6,
		0.0 # Circular orbit
	)
	assert_float_equal(heating, 0.0, 0.01, "Circular orbit should have minimal tidal heating")


## Tests parent radiation for Io-like moon.
func test_parent_radiation_io_like() -> void:
	var radiation: float = ProfileCalculations.calculate_parent_radiation(
		1.898e27, # Jupiter mass
		1.5e20, # Jupiter magnetic moment
		4.217e8 # Io distance
	)
	assert_greater_than(radiation, 0.2, "Io-like moon should have significant parent radiation")


## Tests parent radiation for small parent.
func test_parent_radiation_small_parent() -> void:
	var radiation: float = ProfileCalculations.calculate_parent_radiation(
		5.972e24, # Earth mass
		8.0e22, # Earth magnetic moment
		3.844e8 # Moon distance
	)
	assert_float_equal(radiation, 0.0, 0.01, "Small parent should have no significant radiation")


## Tests eclipse factor calculation.
func test_eclipse_factor() -> void:
	var factor: float = ProfileCalculations.calculate_eclipse_factor(
		6.991e7, # Jupiter radius
		4.217e8, # Io distance
		1.5e5, # Io orbital period (~1.8 days)
		3.74e8 # Jupiter orbital period (~12 years)
	)
	assert_greater_than(factor, 0.0, "Close moon should have eclipse factor")


## Tests eclipse factor with zero values.
func test_eclipse_factor_invalid() -> void:
	var factor: float = ProfileCalculations.calculate_eclipse_factor(0.0, 0.0, 0.0, 0.0)
	assert_float_equal(factor, 0.0, 0.001, "Invalid inputs should return 0")
