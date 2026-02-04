## Derived summary of a planet's habitability-related properties.
## All values are computed from CelestialBody data via ProfileCalculations.
## This is a data snapshot used by population and colony logic.
class_name PlanetProfile
extends RefCounted

# Preload dependencies so class_name types are in scope when this script compiles (e.g. in headless test runner).
const _habitability_category: GDScript = preload("res://src/domain/population/HabitabilityCategory.gd")
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Reference to the source celestial body ID.
var body_id: String = ""

## Habitability score (0-10). Primary measure of human survivability.
## 0 = impossible, 10 = Earth-equivalent or better.
var habitability_score: int = 0

## Average surface temperature in Kelvin.
var avg_temperature_k: float = 0.0

## Atmospheric pressure in Earth atmospheres (1.0 = Earth sea level).
var pressure_atm: float = 0.0

## Fraction of surface covered by liquid (0-1).
var ocean_coverage: float = 0.0

## Fraction of surface that is habitable land (0-1).
var land_coverage: float = 0.0

## Fraction of surface covered by ice (0-1).
var ice_coverage: float = 0.0

## Estimated number of distinct continental landmasses.
var continent_count: int = 0

## Maximum elevation range in kilometers.
var max_elevation_km: float = 0.0

## Length of a day in hours.
var day_length_hours: float = 0.0

## Axial tilt in degrees (affects seasons).
var axial_tilt_deg: float = 0.0

## Surface gravity relative to Earth (1.0 = Earth).
var gravity_g: float = 0.0

## Tectonic activity level (0-1).
var tectonic_activity: float = 0.0

## Volcanic activity level (0-1).
var volcanism_level: float = 0.0

## Weather severity (0-1). Higher = more dangerous weather.
var weather_severity: float = 0.0

## Magnetic field strength normalized (0-1). Affects radiation protection.
var magnetic_field_strength: float = 0.0

## Radiation level at surface (0-1). Higher = more dangerous.
var radiation_level: float = 0.0

## Bond albedo (0-1).
var albedo: float = 0.0

## Greenhouse warming factor (1.0 = no effect).
var greenhouse_factor: float = 1.0

## Climate zones with coverage fractions.
## Array of Dictionary: {"zone": ClimateZone.Zone, "coverage": float}
var climate_zones: Array[Dictionary] = []

## Biome distribution as BiomeType.Type -> coverage fraction.
var biomes: Dictionary = {}

## Resource availability as ResourceType.Type -> abundance (0-1).
var resources: Dictionary = {}

## Whether the body is tidally locked to its parent.
var is_tidally_locked: bool = false

## Whether the body has a significant atmosphere.
var has_atmosphere: bool = false

## Whether the body has a protective magnetic field.
var has_magnetic_field: bool = false

## Whether liquid water exists on the surface.
var has_liquid_water: bool = false

## Whether the atmosphere is breathable by humans.
var has_breathable_atmosphere: bool = false

## Whether this body is a moon (affects certain calculations).
var is_moon: bool = false

## Moon-specific: tidal heating factor from parent body (0-1).
var tidal_heating_factor: float = 0.0

## Moon-specific: radiation exposure from parent body (0-1).
var parent_radiation_exposure: float = 0.0

## Moon-specific: eclipse frequency (eclipses per local year, normalized 0-1).
var eclipse_factor: float = 0.0


## Returns the derived habitability category for display/narrative.
## @return: HabitabilityCategory.Category based on habitability_score.
func get_habitability_category() -> HabitabilityCategory.Category:
	return HabitabilityCategory.from_score(habitability_score)


## Returns a human-readable habitability category string.
## @return: Category name string.
func get_habitability_category_string() -> String:
	return HabitabilityCategory.to_string_name(get_habitability_category())


## Returns the average temperature in Celsius.
## @return: Temperature in Celsius.
func get_temperature_celsius() -> float:
	return avg_temperature_k - 273.15


## Returns the total habitable surface fraction (land that isn't ice).
## @return: Fraction of surface that is habitable (0-1).
func get_habitable_surface() -> float:
	return maxf(0.0, land_coverage - ice_coverage * 0.5)


## Returns the dominant biome (highest coverage).
## @return: BiomeType.Type with highest coverage, or BARREN if none.
func get_dominant_biome() -> BiomeType.Type:
	var max_coverage: float = 0.0
	var dominant: BiomeType.Type = BiomeType.Type.BARREN
	for biome_type in biomes.keys():
		var coverage: float = biomes[biome_type] as float
		if coverage > max_coverage:
			max_coverage = coverage
			dominant = biome_type as BiomeType.Type
	return dominant


## Returns the most abundant resource.
## @return: ResourceType.Type with highest abundance, or SILICATES if none.
func get_primary_resource() -> ResourceType.Type:
	var max_abundance: float = 0.0
	var primary: ResourceType.Type = ResourceType.Type.SILICATES
	for resource_type in resources.keys():
		var abundance: float = resources[resource_type] as float
		if abundance > max_abundance:
			max_abundance = abundance
			primary = resource_type as ResourceType.Type
	return primary


## Returns whether the planet can support native life emergence.
## @return: True if conditions allow for life to evolve.
func can_support_native_life() -> bool:
	# Minimum requirements: some water, not too extreme temperature, some atmosphere
	if not has_liquid_water and ocean_coverage < 0.01:
		return false
	if avg_temperature_k < 200.0 or avg_temperature_k > 400.0:
		return false
	if pressure_atm < 0.01:
		return false
	return habitability_score >= 3


## Returns whether the planet is suitable for colonization attempts.
## @return: True if colonization is feasible (even if difficult).
func is_colonizable() -> bool:
	# Any planet with score >= 1 can theoretically be colonized with enough tech
	# Score 0 means truly impossible (e.g., gas giant surface, star)
	return habitability_score >= 1


## Converts this profile to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var climate_zones_data: Array[Dictionary] = []
	for zone_data in climate_zones:
		climate_zones_data.append({
			"zone": zone_data["zone"] as int,
			"coverage": zone_data["coverage"] as float,
		})

	var biomes_data: Dictionary = {}
	for biome_type in biomes.keys():
		biomes_data[biome_type as int] = biomes[biome_type] as float

	var resources_data: Dictionary = {}
	for resource_type in resources.keys():
		resources_data[resource_type as int] = resources[resource_type] as float

	return {
		"body_id": body_id,
		"habitability_score": habitability_score,
		"avg_temperature_k": avg_temperature_k,
		"pressure_atm": pressure_atm,
		"ocean_coverage": ocean_coverage,
		"land_coverage": land_coverage,
		"ice_coverage": ice_coverage,
		"continent_count": continent_count,
		"max_elevation_km": max_elevation_km,
		"day_length_hours": day_length_hours,
		"axial_tilt_deg": axial_tilt_deg,
		"gravity_g": gravity_g,
		"tectonic_activity": tectonic_activity,
		"volcanism_level": volcanism_level,
		"weather_severity": weather_severity,
		"magnetic_field_strength": magnetic_field_strength,
		"radiation_level": radiation_level,
		"albedo": albedo,
		"greenhouse_factor": greenhouse_factor,
		"climate_zones": climate_zones_data,
		"biomes": biomes_data,
		"resources": resources_data,
		"is_tidally_locked": is_tidally_locked,
		"has_atmosphere": has_atmosphere,
		"has_magnetic_field": has_magnetic_field,
		"has_liquid_water": has_liquid_water,
		"has_breathable_atmosphere": has_breathable_atmosphere,
		"is_moon": is_moon,
		"tidal_heating_factor": tidal_heating_factor,
		"parent_radiation_exposure": parent_radiation_exposure,
		"eclipse_factor": eclipse_factor,
	}


## Creates a PlanetProfile from a dictionary.
## Handles JSON-serialized keys (string or int) for biomes and resources.
## @param data: The dictionary to parse.
## @return: A new PlanetProfile instance.
static func from_dict(data: Dictionary) -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()

	profile.body_id = data.get("body_id", "") as String
	profile.habitability_score = data.get("habitability_score", 0) as int
	profile.avg_temperature_k = data.get("avg_temperature_k", 0.0) as float
	profile.pressure_atm = data.get("pressure_atm", 0.0) as float
	profile.ocean_coverage = data.get("ocean_coverage", 0.0) as float
	profile.land_coverage = data.get("land_coverage", 0.0) as float
	profile.ice_coverage = data.get("ice_coverage", 0.0) as float
	profile.continent_count = data.get("continent_count", 0) as int
	profile.max_elevation_km = data.get("max_elevation_km", 0.0) as float
	profile.day_length_hours = data.get("day_length_hours", 0.0) as float
	profile.axial_tilt_deg = data.get("axial_tilt_deg", 0.0) as float
	profile.gravity_g = data.get("gravity_g", 0.0) as float
	profile.tectonic_activity = data.get("tectonic_activity", 0.0) as float
	profile.volcanism_level = data.get("volcanism_level", 0.0) as float
	profile.weather_severity = data.get("weather_severity", 0.0) as float
	profile.magnetic_field_strength = data.get("magnetic_field_strength", 0.0) as float
	profile.radiation_level = data.get("radiation_level", 0.0) as float
	profile.albedo = data.get("albedo", 0.0) as float
	profile.greenhouse_factor = data.get("greenhouse_factor", 1.0) as float
	profile.is_tidally_locked = data.get("is_tidally_locked", false) as bool
	profile.has_atmosphere = data.get("has_atmosphere", false) as bool
	profile.has_magnetic_field = data.get("has_magnetic_field", false) as bool
	profile.has_liquid_water = data.get("has_liquid_water", false) as bool
	profile.has_breathable_atmosphere = data.get("has_breathable_atmosphere", false) as bool
	profile.is_moon = data.get("is_moon", false) as bool
	profile.tidal_heating_factor = data.get("tidal_heating_factor", 0.0) as float
	profile.parent_radiation_exposure = data.get("parent_radiation_exposure", 0.0) as float
	profile.eclipse_factor = data.get("eclipse_factor", 0.0) as float

	# Parse climate zones
	var climate_zones_data: Array = data.get("climate_zones", []) as Array
	for zone_data in climate_zones_data:
		profile.climate_zones.append({
			"zone": zone_data.get("zone", 0) as ClimateZone.Zone,
			"coverage": zone_data.get("coverage", 0.0) as float,
		})

	# Parse biomes: keys may be int or string (e.g. from JSON)
	var biomes_data: Dictionary = data.get("biomes", {}) as Dictionary
	for biome_key in biomes_data.keys():
		var biome_type: BiomeType.Type = _key_to_int(biome_key) as BiomeType.Type
		profile.biomes[biome_type] = biomes_data[biome_key] as float

	# Parse resources: keys may be int or string (e.g. from JSON)
	var resources_data: Dictionary = data.get("resources", {}) as Dictionary
	for resource_key in resources_data.keys():
		var resource_type: ResourceType.Type = _key_to_int(resource_key) as ResourceType.Type
		profile.resources[resource_type] = resources_data[resource_key] as float

	return profile


## Converts a dict key (int or string from JSON) to int for enum use.
## @param key: Key from serialized data (int or String).
## @return: int value for enum lookup.
static func _key_to_int(key: Variant) -> int:
	if key is int:
		return key as int
	if key is String:
		return int(key as String)
	return 0
