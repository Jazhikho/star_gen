## Technology level classification for populations.
## Represents overall technological development.
class_name TechnologyLevel
extends RefCounted


## Technology eras/levels.
enum Level {
	STONE_AGE,          ## Pre-metalworking
	BRONZE_AGE,         ## Early metallurgy
	IRON_AGE,           ## Iron working
	CLASSICAL,          ## Classical civilizations
	MEDIEVAL,           ## Medieval period
	RENAISSANCE,        ## Early modern
	INDUSTRIAL,         ## Industrial revolution
	ATOMIC,             ## Nuclear/early computing
	INFORMATION,        ## Digital/information age
	SPACEFARING,        ## Established space presence
	INTERSTELLAR,       ## Interstellar capable
	ADVANCED,           ## Beyond current human tech
}


## Converts a level to a display string.
## @param level: The level enum value.
## @return: Human-readable string.
static func to_string_name(level: Level) -> String:
	match level:
		Level.STONE_AGE:
			return "Stone Age"
		Level.BRONZE_AGE:
			return "Bronze Age"
		Level.IRON_AGE:
			return "Iron Age"
		Level.CLASSICAL:
			return "Classical"
		Level.MEDIEVAL:
			return "Medieval"
		Level.RENAISSANCE:
			return "Renaissance"
		Level.INDUSTRIAL:
			return "Industrial"
		Level.ATOMIC:
			return "Atomic Age"
		Level.INFORMATION:
			return "Information Age"
		Level.SPACEFARING:
			return "Spacefaring"
		Level.INTERSTELLAR:
			return "Interstellar"
		Level.ADVANCED:
			return "Advanced"
		_:
			return "Unknown"


## Converts a string to a level enum.
## @param name: The string name (case-insensitive).
## @return: The level, or STONE_AGE if not found.
static func from_string(name: String) -> Level:
	match name.to_lower().replace(" ", "_"):
		"stone_age":
			return Level.STONE_AGE
		"bronze_age":
			return Level.BRONZE_AGE
		"iron_age":
			return Level.IRON_AGE
		"classical":
			return Level.CLASSICAL
		"medieval":
			return Level.MEDIEVAL
		"renaissance":
			return Level.RENAISSANCE
		"industrial":
			return Level.INDUSTRIAL
		"atomic", "atomic_age":
			return Level.ATOMIC
		"information", "information_age":
			return Level.INFORMATION
		"spacefaring":
			return Level.SPACEFARING
		"interstellar":
			return Level.INTERSTELLAR
		"advanced":
			return Level.ADVANCED
		_:
			return Level.STONE_AGE


## Returns the next technology level (if any).
## @param level: The current level.
## @return: The next level, or same if at max.
static func next_level(level: Level) -> Level:
	var next_val: int = (level as int) + 1
	if next_val >= count():
		return level
	return next_val as Level


## Returns the previous technology level (if any).
## @param level: The current level.
## @return: The previous level, or same if at min.
static func previous_level(level: Level) -> Level:
	var prev_val: int = (level as int) - 1
	if prev_val < 0:
		return level
	return prev_val as Level


## Returns whether a level can achieve spaceflight.
## @param level: The level to check.
## @return: True if spacefaring or above.
static func can_spaceflight(level: Level) -> bool:
	return level >= Level.SPACEFARING


## Returns whether a level can achieve interstellar travel.
## @param level: The level to check.
## @return: True if interstellar or above.
static func can_interstellar(level: Level) -> bool:
	return level >= Level.INTERSTELLAR


## Returns approximate years for a native population to reach this level.
## @param level: The target level.
## @return: Rough estimate in years from emergence.
static func typical_years_to_reach(level: Level) -> int:
	match level:
		Level.STONE_AGE:
			return 0
		Level.BRONZE_AGE:
			return 50000
		Level.IRON_AGE:
			return 55000
		Level.CLASSICAL:
			return 57000
		Level.MEDIEVAL:
			return 58000
		Level.RENAISSANCE:
			return 59000
		Level.INDUSTRIAL:
			return 59500
		Level.ATOMIC:
			return 59600
		Level.INFORMATION:
			return 59650
		Level.SPACEFARING:
			return 59700
		Level.INTERSTELLAR:
			return 60000
		Level.ADVANCED:
			return 65000
		_:
			return 0


## Returns the number of technology levels.
## @return: Count of Level enum values.
static func count() -> int:
	return 12
