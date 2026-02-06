## Station size classification based on population capacity.
## U and O are small stations (<10k), B/A/S are progressively larger.
class_name StationClass
extends RefCounted


## Station size classes.
enum Class {
	U, ## Utility - <10,000, "gas station" services focus
	O, ## Outpost - <10,000, purpose-driven (mining, science, military)
	B, ## Base - up to 100,000, small permanent settlement
	A, ## Anchor - up to 1,000,000, major regional hub
	S, ## Super - >1,000,000, megastructure city-scale
}


## Population thresholds for each class.
const UTILITY_MAX: int = 10000
const OUTPOST_MAX: int = 10000
const BASE_MAX: int = 100000
const ANCHOR_MAX: int = 1000000
## S-class has no upper limit


## Converts a class to a display string.
## @param station_class: The class enum value.
## @return: Human-readable string.
static func to_string_name(station_class: Class) -> String:
	match station_class:
		Class.U:
			return "Utility"
		Class.O:
			return "Outpost"
		Class.B:
			return "Base"
		Class.A:
			return "Anchor"
		Class.S:
			return "Super"
		_:
			return "Unknown"


## Converts a class to its single-letter designation.
## @param station_class: The class enum value.
## @return: Single letter (U, O, B, A, S).
static func to_letter(station_class: Class) -> String:
	match station_class:
		Class.U:
			return "U"
		Class.O:
			return "O"
		Class.B:
			return "B"
		Class.A:
			return "A"
		Class.S:
			return "S"
		_:
			return "?"


## Converts a string to a class enum.
## @param name: The string name (case-insensitive).
## @return: The class, or Class.O if not found.
static func from_string(name: String) -> Class:
	match name.to_lower().strip_edges():
		"u", "utility":
			return Class.U
		"o", "outpost":
			return Class.O
		"b", "base":
			return Class.B
		"a", "anchor":
			return Class.A
		"s", "super":
			return Class.S
		_:
			return Class.O


## Returns the maximum population capacity for a class.
## @param station_class: The class enum value.
## @return: Maximum population (or -1 for unlimited).
static func get_max_capacity(station_class: Class) -> int:
	match station_class:
		Class.U:
			return UTILITY_MAX
		Class.O:
			return OUTPOST_MAX
		Class.B:
			return BASE_MAX
		Class.A:
			return ANCHOR_MAX
		Class.S:
			return -1 # Unlimited
		_:
			return OUTPOST_MAX


## Returns the minimum population for a class (exclusive lower bound).
## @param station_class: The class enum value.
## @return: Minimum population threshold.
static func get_min_capacity(station_class: Class) -> int:
	match station_class:
		Class.U:
			return 0
		Class.O:
			return 0
		Class.B:
			return 0 # Can start small and grow
		Class.A:
			return BASE_MAX # Must exceed B-class max
		Class.S:
			return ANCHOR_MAX # Must exceed A-class max
		_:
			return 0


## Determines the appropriate class for a given population.
## Note: Cannot distinguish U from O by population alone; defaults to O.
## @param population: The population count.
## @param is_utility: If true and population < 10k, returns U instead of O.
## @return: The appropriate station class.
static func get_class_for_population(population: int, is_utility: bool = false) -> Class:
	if population <= OUTPOST_MAX:
		return Class.U if is_utility else Class.O
	elif population <= BASE_MAX:
		return Class.B
	elif population <= ANCHOR_MAX:
		return Class.A
	else:
		return Class.S


## Returns whether a class uses outpost government (U/O) vs colony government (B/A/S).
## @param station_class: The class enum value.
## @return: True if uses outpost government model.
static func uses_outpost_government(station_class: Class) -> bool:
	return station_class == Class.U or station_class == Class.O


## Returns whether a class uses colony government (B/A/S).
## @param station_class: The class enum value.
## @return: True if uses colony government model.
static func uses_colony_government(station_class: Class) -> bool:
	return station_class == Class.B or station_class == Class.A or station_class == Class.S


## Returns a short description of the class.
## @param station_class: The class enum value.
## @return: Description string.
static func get_description(station_class: Class) -> String:
	match station_class:
		Class.U:
			return "Utility station providing basic services for passing ships"
		Class.O:
			return "Small outpost for specific purposes like mining or research"
		Class.B:
			return "Permanent base with established population"
		Class.A:
			return "Major regional hub and population center"
		Class.S:
			return "Megastructure supporting city-scale or larger population"
		_:
			return "Unknown station class"


## Returns the number of station classes.
## @return: Count of Class enum values.
static func count() -> int:
	return 5
