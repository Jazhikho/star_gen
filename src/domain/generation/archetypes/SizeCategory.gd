## Size categories for planets and moons.
## Determines mass ranges and likely compositions.
class_name SizeCategory
extends RefCounted


## Size category enumeration from smallest to largest.
enum Category {
	DWARF,           ## Ceres to Pluto scale (0.0001 - 0.01 Earth masses)
	SUB_TERRESTRIAL, ## Moon to Mars scale (0.01 - 0.3 Earth masses)
	TERRESTRIAL,     ## Venus to Earth scale (0.3 - 2 Earth masses)
	SUPER_EARTH,     ## Large rocky (2 - 10 Earth masses)
	MINI_NEPTUNE,    ## Transitional with gas envelope (10 - 25 Earth masses)
	NEPTUNE_CLASS,   ## Ice giants (25 - 80 Earth masses)
	GAS_GIANT,       ## Jupiter class (80 - 4000+ Earth masses)
}


## Returns the string name of a size category.
## @param category: The category enum value.
## @return: Human-readable category name.
static func to_string_name(category: Category) -> String:
	match category:
		Category.DWARF:
			return "Dwarf"
		Category.SUB_TERRESTRIAL:
			return "Sub-Terrestrial"
		Category.TERRESTRIAL:
			return "Terrestrial"
		Category.SUPER_EARTH:
			return "Super-Earth"
		Category.MINI_NEPTUNE:
			return "Mini-Neptune"
		Category.NEPTUNE_CLASS:
			return "Neptune-Class"
		Category.GAS_GIANT:
			return "Gas Giant"
		_:
			return "Unknown"


## Parses a string to a size category.
## @param category_str: The string to parse.
## @return: The corresponding category, or -1 if invalid.
static func from_string(category_str: String) -> int:
	match category_str.to_lower().replace("-", "_").replace(" ", "_"):
		"dwarf":
			return Category.DWARF
		"sub_terrestrial":
			return Category.SUB_TERRESTRIAL
		"terrestrial":
			return Category.TERRESTRIAL
		"super_earth":
			return Category.SUPER_EARTH
		"mini_neptune":
			return Category.MINI_NEPTUNE
		"neptune_class":
			return Category.NEPTUNE_CLASS
		"gas_giant":
			return Category.GAS_GIANT
		_:
			return -1


## Returns whether the category is primarily rocky/solid.
## @param category: The category to check.
## @return: True if rocky composition expected.
static func is_rocky(category: Category) -> bool:
	match category:
		Category.DWARF, Category.SUB_TERRESTRIAL, Category.TERRESTRIAL, Category.SUPER_EARTH:
			return true
		_:
			return false


## Returns whether the category is primarily gaseous.
## @param category: The category to check.
## @return: True if gaseous composition expected.
static func is_gaseous(category: Category) -> bool:
	match category:
		Category.MINI_NEPTUNE, Category.NEPTUNE_CLASS, Category.GAS_GIANT:
			return true
		_:
			return false


## Returns the number of categories.
## @return: Total count of size categories.
static func count() -> int:
	return 7
