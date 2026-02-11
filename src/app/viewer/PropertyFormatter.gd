## Utility class for formatting property values for display.
## Provides consistent formatting for mass, radius, distance, population, and other properties.
class_name PropertyFormatter
extends RefCounted

const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _habitability_category: GDScript = preload("res://src/domain/population/HabitabilityCategory.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Formats mass with appropriate units.
## @param mass_kg: Mass in kilograms.
## @param body_type: The body type.
## @return: Formatted mass string.
static func format_mass(mass_kg: float, body_type: CelestialType.Type) -> String:
	match body_type:
		CelestialType.Type.STAR:
			return "%.3f Mâ˜‰" % (mass_kg / Units.SOLAR_MASS_KG)
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			var earth_masses: float = mass_kg / Units.EARTH_MASS_KG
			if earth_masses > 100:
				return "%.2f MJ" % (mass_kg / 1.898e27)
			return "%.4f MâŠ•" % earth_masses
		_:
			return format_scientific(mass_kg, "kg")


## Formats radius with appropriate units.
## @param radius_m: Radius in meters.
## @param body_type: The body type.
## @return: Formatted radius string.
static func format_radius(radius_m: float, body_type: CelestialType.Type) -> String:
	match body_type:
		CelestialType.Type.STAR:
			return "%.3f Râ˜‰" % (radius_m / Units.SOLAR_RADIUS_METERS)
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			return "%.4f RâŠ•" % (radius_m / Units.EARTH_RADIUS_METERS)
		_:
			var km: float = radius_m / 1000.0
			if km < 1.0:
				return "%.1f m" % radius_m
			return "%.2f km" % km


## Formats distance with appropriate units.
## @param distance_m: Distance in meters.
## @return: Formatted distance string.
static func format_distance(distance_m: float) -> String:
	var au: float = distance_m / Units.AU_METERS
	if au > 0.1:
		return "%.4f AU" % au
	var km: float = distance_m / 1000.0
	if km > 1000:
		return "%.0f km" % km
	return "%.1f km" % km


## Formats luminosity.
## @param luminosity_watts: Luminosity in watts.
## @return: Formatted luminosity string.
static func format_luminosity(luminosity_watts: float) -> String:
	var solar: float = luminosity_watts / 3.828e26
	if solar > 0.01:
		return "%.4f Lâ˜‰" % solar
	return format_scientific(luminosity_watts, "W")


## Formats age in years.
## @param years: Age in years.
## @return: Formatted age string.
static func format_age(years: float) -> String:
	if years > 1e9:
		return "%.2f Gyr" % (years / 1e9)
	if years > 1e6:
		return "%.2f Myr" % (years / 1e6)
	return "%.0f years" % years


## Formats pressure.
## @param pressure_pa: Pressure in Pascals.
## @return: Formatted pressure string.
static func format_pressure(pressure_pa: float) -> String:
	var atm: float = pressure_pa / 101325.0
	if atm > 0.01:
		return "%.4f atm" % atm
	if pressure_pa > 1.0:
		return "%.1f Pa" % pressure_pa
	return "%.2e Pa" % pressure_pa


## Formats particle size.
## @param size_m: Size in meters.
## @return: Formatted size string.
static func format_particle_size(size_m: float) -> String:
	if size_m < 0.01:
		return "%.1f mm" % (size_m * 1000.0)
	if size_m < 1.0:
		return "%.1f cm" % (size_m * 100.0)
	return "%.2f m" % size_m


## Formats a number in scientific notation.
## @param value: The value to format.
## @param unit: The unit string.
## @return: Formatted scientific notation string.
static func format_scientific(value: float, unit: String) -> String:
	if value == 0.0:
		return "0 %s" % unit
	
	var exponent: int = int(floor(log(absf(value)) / log(10.0)))
	var mantissa: float = value / pow(10.0, exponent)
	
	var exp_str: String = format_superscript(exponent)
	return "%.2f Ã— 10%s %s" % [mantissa, exp_str, unit]


## Converts an integer to superscript characters.
## @param num: The number to convert.
## @return: Superscript string.
static func format_superscript(num: int) -> String:
	var superscripts: Dictionary = {
		"0": "â°", "1": "Â¹", "2": "Â²", "3": "Â³", "4": "â´",
		"5": "âµ", "6": "â¶", "7": "â·", "8": "â¸", "9": "â¹",
		"-": "â»"
	}
	
	var num_str: String = str(num)
	var result: String = ""
	
	for c in num_str:
		result += superscripts.get(c, c)
	
	return result


## Formats a population count with appropriate units (K, M, B).
## @param count: Population count.
## @return: Formatted population string.
static func format_population(count: int) -> String:
	if count <= 0:
		return "0"
	if count < 1000:
		return str(count)
	if count < 1000000:
		return "%.1fK" % (float(count) / 1000.0)
	if count < 1000000000:
		return "%.2fM" % (float(count) / 1000000.0)
	return "%.2fB" % (float(count) / 1000000000.0)


## Formats a habitability score with category name.
## @param score: Habitability score (0-10).
## @return: Formatted string like "7/10 (Habitable)".
static func format_habitability(score: int) -> String:
	var category_name: String = HabitabilityCategory.to_string_name(
		HabitabilityCategory.from_score(score)
	)
	return "%d/10 (%s)" % [score, category_name]


## Formats a suitability score with category name.
## @param score: Overall suitability score (0-100).
## @return: Formatted string like "75/100 (Favorable)".
static func format_suitability(score: int) -> String:
	var suitability: ColonySuitability = ColonySuitability.new()
	suitability.overall_score = score
	return "%d/100 (%s)" % [score, suitability.get_category_string()]


## Formats a technology level enum value.
## @param level: The technology level.
## @return: Human-readable string.
static func format_tech_level(level: TechnologyLevel.Level) -> String:
	return TechnologyLevel.to_string_name(level)


## Formats a government regime enum value.
## @param regime: The government regime.
## @return: Human-readable string.
static func format_regime(regime: GovernmentType.Regime) -> String:
	return GovernmentType.to_string_name(regime)


## Formats a colony type enum value.
## @param type: The colony type.
## @return: Human-readable string.
static func format_colony_type(type: ColonyType.Type) -> String:
	return ColonyType.to_string_name(type)


## Formats a percentage value (0-1 range).
## @param value: Value between 0.0 and 1.0.
## @return: Formatted percentage string.
static func format_percent(value: float) -> String:
	return "%.1f%%" % (value * 100.0)


## Formats temperature with both Kelvin and Celsius.
## @param temp_k: Temperature in Kelvin.
## @return: Formatted temperature string.
static func format_temperature(temp_k: float) -> String:
	return "%.0f K (%.0f C)" % [temp_k, temp_k - 273.15]


## Formats a political situation string with appropriate capitalization.
## @param situation: Raw situation string from PlanetPopulationData.
## @return: Formatted display string.
static func format_political_situation(situation: String) -> String:
	match situation:
		"uninhabited":
			return "Uninhabited"
		"native_only":
			return "Native Only"
		"colony_only":
			return "Colony Only"
		"coexisting":
			return "Coexisting"
		"conflict":
			return "Conflict"
		_:
			return situation.capitalize()
