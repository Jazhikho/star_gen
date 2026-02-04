## Climate zone classification for planetary surfaces.
## Represents latitudinal or temperature-based climate bands.
class_name ClimateZone
extends RefCounted


## Climate zone types.
enum Zone {
	POLAR, ## Cold polar regions with ice/tundra
	SUBPOLAR, ## Transitional cold regions
	TEMPERATE, ## Moderate temperature regions
	SUBTROPICAL, ## Warm transitional regions
	TROPICAL, ## Hot equatorial regions
	ARID, ## Hot dry regions (desert climates)
	EXTREME, ## Extreme conditions (no atmosphere, tidally locked extremes)
}


## Converts a zone enum to a display string.
## @param zone: The zone enum value.
## @return: Human-readable string.
static func to_string_name(zone: Zone) -> String:
	match zone:
		Zone.POLAR:
			return "Polar"
		Zone.SUBPOLAR:
			return "Subpolar"
		Zone.TEMPERATE:
			return "Temperate"
		Zone.SUBTROPICAL:
			return "Subtropical"
		Zone.TROPICAL:
			return "Tropical"
		Zone.ARID:
			return "Arid"
		Zone.EXTREME:
			return "Extreme"
		_:
			return "Unknown"


## Converts a string to a zone enum.
## @param name: The string name (case-insensitive).
## @return: The zone enum value, or EXTREME if not found.
static func from_string(name: String) -> Zone:
	match name.to_lower():
		"polar":
			return Zone.POLAR
		"subpolar":
			return Zone.SUBPOLAR
		"temperate":
			return Zone.TEMPERATE
		"subtropical":
			return Zone.SUBTROPICAL
		"tropical":
			return Zone.TROPICAL
		"arid":
			return Zone.ARID
		"extreme":
			return Zone.EXTREME
		_:
			return Zone.EXTREME


## Returns the number of zone types.
## @return: Count of zone enum values.
static func count() -> int:
	return 7
