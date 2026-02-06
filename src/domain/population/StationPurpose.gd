## Station primary purpose classification.
## Describes the main function or role of the station.
class_name StationPurpose
extends RefCounted


## Station purposes.
enum Purpose {
	UTILITY, ## General services (refuel, repair) - typical for U-class
	TRADE, ## Commercial hub, trading post
	MILITARY, ## Defense, strategic outpost
	SCIENCE, ## Research station, observatory
	MINING, ## Resource extraction operations
	RESIDENTIAL, ## Primary focus on housing population
	ADMINISTRATIVE, ## Government, bureaucratic center
	INDUSTRIAL, ## Manufacturing, processing
	MEDICAL, ## Hospital, medical facility
	COMMUNICATIONS, ## Relay station, communications hub
}


## Converts a purpose to a display string.
## @param purpose: The purpose enum value.
## @return: Human-readable string.
static func to_string_name(purpose: Purpose) -> String:
	match purpose:
		Purpose.UTILITY:
			return "Utility"
		Purpose.TRADE:
			return "Trade"
		Purpose.MILITARY:
			return "Military"
		Purpose.SCIENCE:
			return "Science"
		Purpose.MINING:
			return "Mining"
		Purpose.RESIDENTIAL:
			return "Residential"
		Purpose.ADMINISTRATIVE:
			return "Administrative"
		Purpose.INDUSTRIAL:
			return "Industrial"
		Purpose.MEDICAL:
			return "Medical"
		Purpose.COMMUNICATIONS:
			return "Communications"
		_:
			return "Unknown"


## Converts a string to a purpose enum.
## @param name: The string name (case-insensitive).
## @return: The purpose, or Purpose.UTILITY if not found.
static func from_string(name: String) -> Purpose:
	match name.to_lower().strip_edges():
		"utility":
			return Purpose.UTILITY
		"trade":
			return Purpose.TRADE
		"military":
			return Purpose.MILITARY
		"science":
			return Purpose.SCIENCE
		"mining":
			return Purpose.MINING
		"residential":
			return Purpose.RESIDENTIAL
		"administrative":
			return Purpose.ADMINISTRATIVE
		"industrial":
			return Purpose.INDUSTRIAL
		"medical":
			return Purpose.MEDICAL
		"communications":
			return Purpose.COMMUNICATIONS
		_:
			return Purpose.UTILITY


## Returns typical purposes for U-class (utility) stations.
## @return: Array of typical purposes.
static func typical_utility_purposes() -> Array[Purpose]:
	return [Purpose.UTILITY, Purpose.TRADE, Purpose.COMMUNICATIONS]


## Returns typical purposes for O-class (outpost) stations.
## @return: Array of typical purposes.
static func typical_outpost_purposes() -> Array[Purpose]:
	return [Purpose.MILITARY, Purpose.SCIENCE, Purpose.MINING, Purpose.COMMUNICATIONS]


## Returns typical purposes for larger stations (B/A/S class).
## @return: Array of typical purposes.
static func typical_settlement_purposes() -> Array[Purpose]:
	return [Purpose.TRADE, Purpose.RESIDENTIAL, Purpose.ADMINISTRATIVE, Purpose.INDUSTRIAL]


## Returns whether a purpose is typical for small stations.
## @param purpose: The purpose to check.
## @return: True if commonly found in U/O class stations.
static func is_small_station_purpose(purpose: Purpose) -> bool:
	return purpose in [Purpose.UTILITY, Purpose.MINING, Purpose.SCIENCE, Purpose.MILITARY, Purpose.COMMUNICATIONS]


## Returns the number of purposes.
## @return: Count of Purpose enum values.
static func count() -> int:
	return 10
