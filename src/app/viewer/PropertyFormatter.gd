## Utility class for formatting property values for display.
## Provides consistent formatting for mass, radius, distance, and other properties.
class_name PropertyFormatter
extends RefCounted

const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


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
