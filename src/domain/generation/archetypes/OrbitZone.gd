## Orbital zones relative to the habitable zone and frost line.
## Determines temperature regime and likely composition.
class_name OrbitZone
extends RefCounted

const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")


## Orbit zone enumeration.
enum Zone {
	HOT,       ## Inside inner habitable zone edge
	TEMPERATE, ## Within habitable zone
	COLD,      ## Beyond frost line
}


## Returns the string name of an orbit zone.
## @param zone: The zone enum value.
## @return: Human-readable zone name.
static func to_string_name(zone: Zone) -> String:
	match zone:
		Zone.HOT:
			return "Hot"
		Zone.TEMPERATE:
			return "Temperate"
		Zone.COLD:
			return "Cold"
		_:
			return "Unknown"


## Parses a string to an orbit zone.
## @param zone_str: The string to parse.
## @return: The corresponding zone, or -1 if invalid.
static func from_string(zone_str: String) -> int:
	match zone_str.to_lower():
		"hot":
			return Zone.HOT
		"temperate":
			return Zone.TEMPERATE
		"cold":
			return Zone.COLD
		_:
			return -1


## Determines the orbit zone from orbital distance and stellar properties.
## @param orbital_distance_m: Distance from star in meters.
## @param stellar_luminosity_watts: Star's luminosity in watts.
## @return: The orbit zone for this position.
static func from_orbital_distance(
	orbital_distance_m: float,
	stellar_luminosity_watts: float
) -> Zone:
	if stellar_luminosity_watts <= 0.0 or orbital_distance_m <= 0.0:
		return Zone.TEMPERATE
	
	var l_solar: float = stellar_luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	var sqrt_l: float = sqrt(l_solar)
	
	# Habitable zone inner edge ~0.95 AU * sqrt(L)
	var hz_inner_m: float = 0.95 * Units.AU_METERS * sqrt_l
	# Frost line ~2.7 AU * sqrt(L)
	var frost_line_m: float = 2.7 * Units.AU_METERS * sqrt_l
	
	if orbital_distance_m < hz_inner_m:
		return Zone.HOT
	elif orbital_distance_m > frost_line_m:
		return Zone.COLD
	else:
		return Zone.TEMPERATE


## Returns the number of zones.
## @return: Total count of orbit zones.
static func count() -> int:
	return 3
