## Ring system complexity levels.
class_name RingComplexity
extends RefCounted


## Complexity level enumeration.
enum Level {
	TRACE,   ## 1 faint band, low optical depth
	SIMPLE,  ## 2-3 bands with moderate gaps
	COMPLEX, ## 4-7 bands with prominent gaps
}


## Returns the string name of a complexity level.
## @param level: The level enum value.
## @return: Human-readable level name.
static func to_string_name(level: Level) -> String:
	match level:
		Level.TRACE:
			return "Trace"
		Level.SIMPLE:
			return "Simple"
		Level.COMPLEX:
			return "Complex"
		_:
			return "Unknown"


## Parses a string to a complexity level.
## @param level_str: The string to parse.
## @return: The corresponding level, or -1 if invalid.
static func from_string(level_str: String) -> int:
	match level_str.to_lower():
		"trace":
			return Level.TRACE
		"simple":
			return Level.SIMPLE
		"complex":
			return Level.COMPLEX
		_:
			return -1


## Returns the expected band count range for a complexity level.
## @param level: The complexity level.
## @return: Dictionary with "min" and "max" band counts.
static func get_band_count_range(level: Level) -> Dictionary:
	match level:
		Level.TRACE:
			return {"min": 1, "max": 1}
		Level.SIMPLE:
			return {"min": 2, "max": 3}
		Level.COMPLEX:
			return {"min": 4, "max": 7}
		_:
			return {"min": 1, "max": 1}


## Returns typical optical depth range for a complexity level.
## @param level: The complexity level.
## @return: Dictionary with "min" and "max" optical depth values.
static func get_optical_depth_range(level: Level) -> Dictionary:
	match level:
		Level.TRACE:
			return {"min": 0.01, "max": 0.1}
		Level.SIMPLE:
			return {"min": 0.1, "max": 0.5}
		Level.COMPLEX:
			return {"min": 0.3, "max": 2.0}
		_:
			return {"min": 0.01, "max": 0.1}


## Returns the number of levels.
## @return: Total count of complexity levels.
static func count() -> int:
	return 3
