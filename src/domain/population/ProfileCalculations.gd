## Pure calculation functions for building PlanetProfile from CelestialBody data.
## All functions are static and deterministic: same inputs → same outputs.
## No file I/O, no Nodes, no global state.
class_name ProfileCalculations
extends RefCounted

# Ensure enums are in scope when this script compiles (e.g. headless runner, editor).
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Earth's surface gravity in m/s².
const EARTH_GRAVITY: float = 9.81

## Earth's atmospheric pressure in Pascals.
const EARTH_PRESSURE_PA: float = 101325.0

## Earth's magnetic moment in A·m² (approximate).
const EARTH_MAGNETIC_MOMENT: float = 8.0e22

## Water freezing point in Kelvin.
const WATER_FREEZE_K: float = 273.15

## Water boiling point at 1 atm in Kelvin.
const WATER_BOIL_K: float = 373.15

## Seconds per hour.
const SECONDS_PER_HOUR: float = 3600.0

## Minimum pressure for "has atmosphere" (in atm).
const MIN_ATMOSPHERE_ATM: float = 0.001


## Calculates habitability score (0-10) from profile data.
## @param temp_k: Average surface temperature in Kelvin.
## @param pressure_atm: Atmospheric pressure in Earth atmospheres.
## @param gravity_g: Surface gravity relative to Earth.
## @param has_liquid_water: Whether liquid water exists.
## @param has_breathable: Whether atmosphere is breathable.
## @param radiation_level: Radiation level (0-1).
## @param ocean_coverage: Fraction of surface covered by ocean.
## @return: Habitability score 0-10.
static func calculate_habitability_score(
	temp_k: float,
	pressure_atm: float,
	gravity_g: float,
	has_liquid_water: bool,
	has_breathable: bool,
	radiation_level: float,
	ocean_coverage: float
) -> int:
	var score: float = 0.0
	var temp_c: float = temp_k - 273.15

	# Temperature (max 3 points)
	# Ideal: 273-303 K (0-30°C)
	if temp_c >= -20.0 and temp_c <= 50.0:
		score += 1.0
	if temp_c >= 0.0 and temp_c <= 40.0:
		score += 1.0
	if temp_c >= 10.0 and temp_c <= 30.0:
		score += 1.0

	# Pressure (max 2 points)
	# Ideal: 0.5-2.0 atm
	if pressure_atm >= 0.1 and pressure_atm <= 5.0:
		score += 1.0
	if pressure_atm >= 0.5 and pressure_atm <= 2.0:
		score += 1.0

	# Water (max 2 points)
	if has_liquid_water:
		score += 1.0
	if ocean_coverage >= 0.1 and ocean_coverage <= 0.9:
		score += 1.0

	# Gravity (max 1 point)
	if gravity_g >= 0.5 and gravity_g <= 1.5:
		score += 1.0

	# Atmosphere breathability (max 1 point)
	if has_breathable:
		score += 1.0

	# Radiation protection (max 1 point)
	if radiation_level < 0.3:
		score += 1.0

	return clampi(roundi(score), 0, 10)


## Calculates weather severity from atmosphere and rotation.
## @param pressure_atm: Atmospheric pressure in Earth atmospheres.
## @param rotation_period_s: Rotation period in seconds (can be negative for retrograde).
## @param has_atmosphere: Whether the body has an atmosphere.
## @return: Weather severity (0-1).
static func calculate_weather_severity(
	pressure_atm: float,
	rotation_period_s: float,
	has_atmosphere: bool
) -> float:
	if not has_atmosphere or pressure_atm < MIN_ATMOSPHERE_ATM:
		return 0.0

	# Atmospheric density contributes to weather
	var atmo_factor: float = clampf(pressure_atm * 0.3, 0.0, 0.5)

	# Fast rotation creates more turbulent weather
	# Earth's day is ~86400 seconds
	var earth_day: float = 86400.0
	var rotation_factor: float = 0.0
	if absf(rotation_period_s) > 0.0:
		var day_ratio: float = earth_day / absf(rotation_period_s)
		rotation_factor = clampf(day_ratio * 0.2, 0.0, 0.4)

	# Very slow rotation (tidally locked) can also cause extreme weather at terminator
	if absf(rotation_period_s) > earth_day * 10.0:
		rotation_factor = 0.3

	return clampf(atmo_factor + rotation_factor, 0.0, 1.0)


## Calculates magnetic field strength normalized to Earth.
## @param magnetic_moment: Magnetic dipole moment in T·m³.
## @return: Normalized magnetic field strength (0-1).
static func calculate_magnetic_strength(magnetic_moment: float) -> float:
	if magnetic_moment <= 0.0:
		return 0.0
	return clampf(magnetic_moment / EARTH_MAGNETIC_MOMENT, 0.0, 1.0)


## Calculates radiation level from magnetic field and atmosphere.
## @param magnetic_moment: Magnetic dipole moment in T·m³.
## @param pressure_atm: Atmospheric pressure in Earth atmospheres.
## @param has_atmosphere: Whether the body has an atmosphere.
## @return: Radiation level at surface (0-1). Higher = more dangerous.
static func calculate_radiation_level(
	magnetic_moment: float,
	pressure_atm: float,
	has_atmosphere: bool
) -> float:
	# Magnetic field protection
	var mag_protection: float = clampf(magnetic_moment / EARTH_MAGNETIC_MOMENT, 0.0, 1.0)

	# Atmospheric protection
	var atmo_protection: float = 0.0
	if has_atmosphere:
		atmo_protection = clampf(pressure_atm * 0.5, 0.0, 0.5)

	# Combined protection reduces radiation
	var total_protection: float = 1.0 - (1.0 - mag_protection) * (1.0 - atmo_protection)

	return clampf(1.0 - total_protection, 0.0, 1.0)


## Estimates continent count from terrain properties and land coverage.
## @param tectonic_activity: Tectonic activity level (0-1).
## @param land_coverage: Fraction of surface that is land (0-1).
## @param has_terrain: Whether terrain data exists.
## @return: Estimated number of continents.
static func estimate_continent_count(
	tectonic_activity: float,
	land_coverage: float,
	has_terrain: bool
) -> int:
	if not has_terrain or land_coverage < 0.01:
		return 0

	# Base count from land coverage
	var base_count: int = 1
	if land_coverage > 0.15:
		base_count = 2
	if land_coverage > 0.30:
		base_count = 3
	if land_coverage > 0.50:
		base_count = 5
	if land_coverage > 0.70:
		base_count = 7

	# Tectonic activity increases fragmentation
	var tectonic_modifier: float = 1.0 + tectonic_activity

	return maxi(1, roundi(base_count * tectonic_modifier))


## Calculates climate zones from axial tilt and temperature.
## @param axial_tilt_deg: Axial tilt in degrees.
## @param avg_temp_k: Average surface temperature in Kelvin.
## @param has_atmosphere: Whether the body has an atmosphere.
## @return: Array of dictionaries with "zone" and "coverage" keys.
static func calculate_climate_zones(
	axial_tilt_deg: float,
	avg_temp_k: float,
	has_atmosphere: bool
) -> Array[Dictionary]:
	var zones: Array[Dictionary] = []

	if not has_atmosphere:
		# No atmosphere = extreme conditions
		zones.append({"zone": ClimateZone.Zone.EXTREME, "coverage": 1.0})
		return zones

	# Extremely cold world
	if avg_temp_k < 200.0:
		zones.append({"zone": ClimateZone.Zone.POLAR, "coverage": 1.0})
		return zones

	# Extremely hot world
	if avg_temp_k > 400.0:
		zones.append({"zone": ClimateZone.Zone.ARID, "coverage": 1.0})
		return zones

	# Normal temperature range - distribute zones based on tilt
	var tilt_factor: float = axial_tilt_deg / 23.5 # Earth-normalized
	tilt_factor = clampf(tilt_factor, 0.5, 2.0)

	# Calculate zone coverage
	var polar: float = clampf(0.1 * tilt_factor, 0.05, 0.25)
	var subpolar: float = clampf(0.1 * tilt_factor, 0.05, 0.15)
	var temperate: float = clampf(0.25 + 0.05 * tilt_factor, 0.15, 0.35)
	var subtropical: float = clampf(0.15, 0.1, 0.2)
	var tropical: float = 1.0 - (polar + subpolar + temperate + subtropical) * 2.0
	tropical = maxf(tropical, 0.1)

	# Adjust for temperature - colder worlds have more polar coverage
	var temp_c: float = avg_temp_k - 273.15
	if temp_c < 0.0:
		var cold_factor: float = clampf(-temp_c / 30.0, 0.0, 1.0)
		polar += cold_factor * 0.2
		tropical -= cold_factor * 0.2
		tropical = maxf(tropical, 0.05)
	elif temp_c > 30.0:
		var hot_factor: float = clampf((temp_c - 30.0) / 30.0, 0.0, 1.0)
		tropical += hot_factor * 0.2
		polar -= hot_factor * 0.1
		polar = maxf(polar, 0.02)

	# Add zones (both hemispheres, so polar/subpolar/temperate/subtropical are doubled)
	zones.append({"zone": ClimateZone.Zone.POLAR, "coverage": polar * 2.0})
	zones.append({"zone": ClimateZone.Zone.SUBPOLAR, "coverage": subpolar * 2.0})
	zones.append({"zone": ClimateZone.Zone.TEMPERATE, "coverage": temperate * 2.0})
	zones.append({"zone": ClimateZone.Zone.SUBTROPICAL, "coverage": subtropical * 2.0})
	zones.append({"zone": ClimateZone.Zone.TROPICAL, "coverage": tropical})

	# Normalize to ensure sum is 1.0
	var total: float = 0.0
	for zone_data in zones:
		total += zone_data["coverage"] as float
	if total > 0.0:
		for zone_data in zones:
			zone_data["coverage"] = (zone_data["coverage"] as float) / total

	return zones


## Calculates biome distribution from climate and surface data.
## @param climate_zones: Array of climate zone dictionaries.
## @param ocean_coverage: Fraction of surface covered by ocean.
## @param ice_coverage: Fraction of surface covered by ice.
## @param volcanism_level: Volcanic activity level (0-1).
## @param has_liquid_water: Whether liquid water exists.
## @param has_atmosphere: Whether the body has an atmosphere.
## @return: Dictionary of BiomeType.Type (as int) -> coverage fraction.
static func calculate_biomes(
	climate_zones: Array[Dictionary],
	ocean_coverage: float,
	ice_coverage: float,
	volcanism_level: float,
	has_liquid_water: bool,
	has_atmosphere: bool
) -> Dictionary:
	var biomes: Dictionary = {} # int (BiomeType.Type) -> float

	# Ocean biome
	if ocean_coverage > 0.0:
		biomes[BiomeType.Type.OCEAN as int] = ocean_coverage

	# Ice sheet biome
	if ice_coverage > 0.0:
		biomes[BiomeType.Type.ICE_SHEET as int] = ice_coverage

	var land: float = maxf(0.0, 1.0 - ocean_coverage - ice_coverage)
	if land <= 0.0:
		return biomes

	# No atmosphere = barren
	if not has_atmosphere:
		biomes[BiomeType.Type.BARREN as int] = land
		return biomes

	# Distribute land biomes by climate zone
	for zone_data in climate_zones:
		var zone: ClimateZone.Zone = zone_data["zone"] as ClimateZone.Zone
		var coverage: float = zone_data["coverage"] as float
		var land_in_zone: float = land * coverage

		if land_in_zone < 0.001:
			continue

		match zone:
			ClimateZone.Zone.POLAR:
				_add_biome(biomes, BiomeType.Type.TUNDRA, land_in_zone)
			ClimateZone.Zone.SUBPOLAR:
				if has_liquid_water:
					_add_biome(biomes, BiomeType.Type.TAIGA, land_in_zone * 0.7)
					_add_biome(biomes, BiomeType.Type.TUNDRA, land_in_zone * 0.3)
				else:
					_add_biome(biomes, BiomeType.Type.TUNDRA, land_in_zone)
			ClimateZone.Zone.TEMPERATE:
				if has_liquid_water:
					_add_biome(biomes, BiomeType.Type.FOREST, land_in_zone * 0.5)
					_add_biome(biomes, BiomeType.Type.GRASSLAND, land_in_zone * 0.4)
					_add_biome(biomes, BiomeType.Type.WETLAND, land_in_zone * 0.1)
				else:
					_add_biome(biomes, BiomeType.Type.DESERT, land_in_zone * 0.6)
					_add_biome(biomes, BiomeType.Type.GRASSLAND, land_in_zone * 0.4)
			ClimateZone.Zone.SUBTROPICAL:
				if has_liquid_water:
					_add_biome(biomes, BiomeType.Type.SAVANNA, land_in_zone * 0.5)
					_add_biome(biomes, BiomeType.Type.FOREST, land_in_zone * 0.3)
					_add_biome(biomes, BiomeType.Type.DESERT, land_in_zone * 0.2)
				else:
					_add_biome(biomes, BiomeType.Type.DESERT, land_in_zone)
			ClimateZone.Zone.TROPICAL:
				if has_liquid_water:
					_add_biome(biomes, BiomeType.Type.JUNGLE, land_in_zone * 0.6)
					_add_biome(biomes, BiomeType.Type.SAVANNA, land_in_zone * 0.3)
					_add_biome(biomes, BiomeType.Type.WETLAND, land_in_zone * 0.1)
				else:
					_add_biome(biomes, BiomeType.Type.DESERT, land_in_zone)
			ClimateZone.Zone.ARID:
				_add_biome(biomes, BiomeType.Type.DESERT, land_in_zone)
			ClimateZone.Zone.EXTREME:
				_add_biome(biomes, BiomeType.Type.BARREN, land_in_zone)

	# Add volcanic regions
	if volcanism_level > 0.3:
		var volcanic_coverage: float = land * volcanism_level * 0.15
		_add_biome(biomes, BiomeType.Type.VOLCANIC, volcanic_coverage)

	# Add mountain regions (assume some percentage of land)
	var mountain_coverage: float = land * 0.1
	_add_biome(biomes, BiomeType.Type.MOUNTAIN, mountain_coverage)

	return biomes


## Helper to add biome coverage, accumulating if already exists.
static func _add_biome(biomes: Dictionary, biome: BiomeType.Type, coverage: float) -> void:
	var key: int = biome as int
	biomes[key] = (biomes.get(key, 0.0) as float) + coverage


## Calculates resource availability from surface composition and biomes.
## @param surface_composition: Material composition dictionary from SurfaceProps.
## @param biomes: Biome distribution dictionary.
## @param volcanism_level: Volcanic activity level (0-1).
## @param has_liquid_water: Whether liquid water exists.
## @param ocean_coverage: Fraction of surface covered by ocean.
## @return: Dictionary of ResourceType.Type (as int) -> abundance (0-1).
static func calculate_resources(
	surface_composition: Dictionary,
	biomes: Dictionary,
	volcanism_level: float,
	has_liquid_water: bool,
	ocean_coverage: float
) -> Dictionary:
	var resources: Dictionary = {} # int (ResourceType.Type) -> float

	# Water resources
	var water_abundance: float = 0.0
	if has_liquid_water:
		water_abundance = 0.5 + ocean_coverage * 0.5
	elif surface_composition.has("water_ice"):
		water_abundance = (surface_composition["water_ice"] as float) * 0.7
	if water_abundance > 0.0:
		resources[ResourceType.Type.WATER as int] = clampf(water_abundance, 0.0, 1.0)

	# Silicates from composition
	if surface_composition.has("silicates"):
		var silicate_val: float = surface_composition["silicates"] as float
		resources[ResourceType.Type.SILICATES as int] = clampf(silicate_val, 0.0, 1.0)
	else:
		# Default: most rocky bodies have silicates
		resources[ResourceType.Type.SILICATES as int] = 0.5

	# Metals from iron oxides and related materials
	var metal_abundance: float = 0.0
	if surface_composition.has("iron_oxides"):
		metal_abundance += (surface_composition["iron_oxides"] as float) * 1.5
	if surface_composition.has("metals"):
		metal_abundance += surface_composition["metals"] as float
	if metal_abundance > 0.0:
		resources[ResourceType.Type.METALS as int] = clampf(metal_abundance, 0.0, 1.0)

	# Rare elements from volcanic activity
	if volcanism_level > 0.2:
		var rare_abundance: float = volcanism_level * 0.5
		resources[ResourceType.Type.RARE_ELEMENTS as int] = clampf(rare_abundance, 0.0, 1.0)

	# Radioactives (often associated with volcanic/tectonic activity)
	if volcanism_level > 0.3:
		var radio_abundance: float = volcanism_level * 0.3
		resources[ResourceType.Type.RADIOACTIVES as int] = clampf(radio_abundance, 0.0, 1.0)

	# Hydrocarbons from carbon compounds
	if surface_composition.has("carbon_compounds"):
		var carbon_val: float = surface_composition["carbon_compounds"] as float
		resources[ResourceType.Type.HYDROCARBONS as int] = clampf(carbon_val * 0.8, 0.0, 1.0)

	# Organics from biomes with life potential
	var organic_abundance: float = 0.0
	var forest_key: int = BiomeType.Type.FOREST as int
	var jungle_key: int = BiomeType.Type.JUNGLE as int
	var wetland_key: int = BiomeType.Type.WETLAND as int
	var ocean_key: int = BiomeType.Type.OCEAN as int

	if biomes.has(forest_key):
		organic_abundance += (biomes[forest_key] as float) * 0.8
	if biomes.has(jungle_key):
		organic_abundance += (biomes[jungle_key] as float) * 1.0
	if biomes.has(wetland_key):
		organic_abundance += (biomes[wetland_key] as float) * 0.6
	if biomes.has(ocean_key) and has_liquid_water:
		organic_abundance += (biomes[ocean_key] as float) * 0.3

	if organic_abundance > 0.0:
		resources[ResourceType.Type.ORGANICS as int] = clampf(organic_abundance, 0.0, 1.0)

	# Volatiles from atmosphere-bearing worlds
	if surface_composition.has("nitrogen_ice") or surface_composition.has("methane_ice"):
		var volatile_val: float = 0.0
		if surface_composition.has("nitrogen_ice"):
			volatile_val += surface_composition["nitrogen_ice"] as float
		if surface_composition.has("methane_ice"):
			volatile_val += surface_composition["methane_ice"] as float
		resources[ResourceType.Type.VOLATILES as int] = clampf(volatile_val, 0.0, 1.0)

	return resources


## Checks if atmosphere is breathable by humans.
## @param composition: Gas composition dictionary from AtmosphereProps.
## @param pressure_atm: Atmospheric pressure in Earth atmospheres.
## @return: True if atmosphere is breathable.
static func check_breathability(composition: Dictionary, pressure_atm: float) -> bool:
	if composition.is_empty():
		return false

	# Need pressure in survivable range
	if pressure_atm < 0.5 or pressure_atm > 3.0:
		return false

	# Check for oxygen
	var o2_fraction: float = 0.0
	if composition.has("O2"):
		o2_fraction = composition["O2"] as float
	elif composition.has("oxygen"):
		o2_fraction = composition["oxygen"] as float

	# Need 18-25% oxygen (Earth is ~21%)
	if o2_fraction < 0.18 or o2_fraction > 0.25:
		return false

	# Check for toxic gases
	var toxic_gases: Array[String] = ["CO2", "CO", "H2S", "SO2", "NH3", "Cl2"]
	for gas in toxic_gases:
		if composition.has(gas):
			var fraction: float = composition[gas] as float
			# CO2 above 5% is dangerous, others above 1%
			var threshold: float = 0.01
			if gas == "CO2":
				threshold = 0.05
			if fraction > threshold:
				return false

	return true


## Calculates moon-specific tidal heating factor.
## @param parent_mass_kg: Mass of parent body in kg.
## @param orbital_distance_m: Orbital semi-major axis in meters.
## @param moon_radius_m: Radius of the moon in meters.
## @param eccentricity: Orbital eccentricity.
## @return: Tidal heating factor (0-1).
static func calculate_tidal_heating(
	parent_mass_kg: float,
	orbital_distance_m: float,
	moon_radius_m: float,
	eccentricity: float
) -> float:
	if parent_mass_kg <= 0.0 or orbital_distance_m <= 0.0 or moon_radius_m <= 0.0:
		return 0.0

	# Tidal heating is proportional to (M^2 * R^5 * e^2) / a^6
	# Simplified and normalized to Io-like conditions
	var io_parent_mass: float = 1.898e27 # Jupiter mass
	var io_distance: float = 4.217e8 # Io orbital distance
	var io_radius: float = 1.821e6 # Io radius
	var io_eccentricity: float = 0.0041

	var mass_factor: float = pow(parent_mass_kg / io_parent_mass, 2.0)
	var radius_factor: float = pow(moon_radius_m / io_radius, 5.0)
	var distance_factor: float = pow(io_distance / orbital_distance_m, 6.0)
	var ecc_factor: float = pow(eccentricity / maxf(io_eccentricity, 0.001), 2.0)

	var heating: float = mass_factor * radius_factor * distance_factor * ecc_factor

	return clampf(heating, 0.0, 1.0)


## Calculates radiation exposure from parent body (for moons of gas giants).
## @param parent_mass_kg: Mass of parent body in kg.
## @param parent_magnetic_moment: Parent's magnetic moment.
## @param orbital_distance_m: Orbital distance from parent.
## @return: Radiation exposure factor (0-1).
static func calculate_parent_radiation(
	parent_mass_kg: float,
	parent_magnetic_moment: float,
	orbital_distance_m: float
) -> float:
	if parent_mass_kg <= 0.0 or orbital_distance_m <= 0.0:
		return 0.0

	# Gas giants with strong magnetic fields trap radiation
	# Jupiter's magnetic moment is ~1.5e20 T·m³
	var jupiter_mass: float = 1.898e27
	var jupiter_magnetic: float = 1.5e20
	var io_distance: float = 4.217e8

	# Only significant for large parent bodies
	if parent_mass_kg < jupiter_mass * 0.01:
		return 0.0

	var magnetic_factor: float = parent_magnetic_moment / jupiter_magnetic
	var distance_factor: float = pow(io_distance / orbital_distance_m, 2.0)

	var radiation: float = magnetic_factor * distance_factor * 0.5

	return clampf(radiation, 0.0, 1.0)


## Calculates eclipse factor for moons (how often the parent blocks the star).
## @param parent_radius_m: Radius of parent body in meters.
## @param orbital_distance_m: Moon's orbital distance from parent.
## @param orbital_period_s: Moon's orbital period in seconds.
## @param parent_orbital_period_s: Parent's orbital period around star.
## @return: Eclipse factor (0-1), representing frequency/impact of eclipses.
static func calculate_eclipse_factor(
	parent_radius_m: float,
	orbital_distance_m: float,
	orbital_period_s: float,
	parent_orbital_period_s: float
) -> float:
	if parent_radius_m <= 0.0 or orbital_distance_m <= 0.0:
		return 0.0
	if orbital_period_s <= 0.0 or parent_orbital_period_s <= 0.0:
		return 0.0

	# Angular size of parent as seen from moon
	var angular_size: float = 2.0 * parent_radius_m / orbital_distance_m

	# Frequency of eclipses per parent orbit
	var eclipses_per_orbit: float = parent_orbital_period_s / orbital_period_s

	# Duration factor (larger parent = longer eclipses)
	var duration_factor: float = clampf(angular_size * 5.0, 0.0, 1.0)

	# Frequency factor (more orbits = more eclipses)
	var frequency_factor: float = clampf(eclipses_per_orbit / 100.0, 0.0, 1.0)

	return clampf(duration_factor * 0.5 + frequency_factor * 0.5, 0.0, 1.0)
