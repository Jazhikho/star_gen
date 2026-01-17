## Lookup table for size category properties.
## Provides mass, radius, and density ranges for each size category.
class_name SizeTable
extends RefCounted

const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Mass ranges in Earth masses for each size category.
const MASS_RANGES: Dictionary = {
	SizeCategory.Category.DWARF: {"min": 0.0001, "max": 0.01},
	SizeCategory.Category.SUB_TERRESTRIAL: {"min": 0.01, "max": 0.3},
	SizeCategory.Category.TERRESTRIAL: {"min": 0.3, "max": 2.0},
	SizeCategory.Category.SUPER_EARTH: {"min": 2.0, "max": 10.0},
	SizeCategory.Category.MINI_NEPTUNE: {"min": 10.0, "max": 25.0},
	SizeCategory.Category.NEPTUNE_CLASS: {"min": 25.0, "max": 80.0},
	SizeCategory.Category.GAS_GIANT: {"min": 80.0, "max": 4000.0},
}


## Radius ranges in Earth radii for each size category.
const RADIUS_RANGES: Dictionary = {
	SizeCategory.Category.DWARF: {"min": 0.03, "max": 0.2},
	SizeCategory.Category.SUB_TERRESTRIAL: {"min": 0.2, "max": 0.6},
	SizeCategory.Category.TERRESTRIAL: {"min": 0.6, "max": 1.5},
	SizeCategory.Category.SUPER_EARTH: {"min": 1.2, "max": 2.0},
	SizeCategory.Category.MINI_NEPTUNE: {"min": 2.0, "max": 4.0},
	SizeCategory.Category.NEPTUNE_CLASS: {"min": 3.5, "max": 6.0},
	SizeCategory.Category.GAS_GIANT: {"min": 6.0, "max": 15.0},
}


## Typical density ranges in kg/m³ for each size category.
const DENSITY_RANGES: Dictionary = {
	SizeCategory.Category.DWARF: {"min": 1500.0, "max": 3500.0},
	SizeCategory.Category.SUB_TERRESTRIAL: {"min": 3000.0, "max": 5500.0},
	SizeCategory.Category.TERRESTRIAL: {"min": 4000.0, "max": 6500.0},
	SizeCategory.Category.SUPER_EARTH: {"min": 4500.0, "max": 8000.0},
	SizeCategory.Category.MINI_NEPTUNE: {"min": 1000.0, "max": 3000.0},
	SizeCategory.Category.NEPTUNE_CLASS: {"min": 800.0, "max": 2000.0},
	SizeCategory.Category.GAS_GIANT: {"min": 500.0, "max": 1500.0},
}


## Gets the mass range for a size category.
## @param category: The size category.
## @return: Dictionary with "min" and "max" in Earth masses.
static func get_mass_range(category: SizeCategory.Category) -> Dictionary:
	if MASS_RANGES.has(category):
		return MASS_RANGES[category]
	return {"min": 0.0, "max": 0.0}


## Gets the radius range for a size category.
## @param category: The size category.
## @return: Dictionary with "min" and "max" in Earth radii.
static func get_radius_range(category: SizeCategory.Category) -> Dictionary:
	if RADIUS_RANGES.has(category):
		return RADIUS_RANGES[category]
	return {"min": 0.0, "max": 0.0}


## Gets the density range for a size category.
## @param category: The size category.
## @return: Dictionary with "min" and "max" in kg/m³.
static func get_density_range(category: SizeCategory.Category) -> Dictionary:
	if DENSITY_RANGES.has(category):
		return DENSITY_RANGES[category]
	return {"min": 0.0, "max": 0.0}


## Determines the size category from a mass value.
## @param mass_earth: Mass in Earth masses.
## @return: The matching size category.
static func category_from_mass(mass_earth: float) -> SizeCategory.Category:
	if mass_earth < 0.01:
		return SizeCategory.Category.DWARF
	elif mass_earth < 0.3:
		return SizeCategory.Category.SUB_TERRESTRIAL
	elif mass_earth < 2.0:
		return SizeCategory.Category.TERRESTRIAL
	elif mass_earth < 10.0:
		return SizeCategory.Category.SUPER_EARTH
	elif mass_earth < 25.0:
		return SizeCategory.Category.MINI_NEPTUNE
	elif mass_earth < 80.0:
		return SizeCategory.Category.NEPTUNE_CLASS
	else:
		return SizeCategory.Category.GAS_GIANT


## Generates a random mass within a size category.
## @param category: The size category.
## @param rng: The random number generator.
## @return: Mass in Earth masses.
static func random_mass_earth(category: SizeCategory.Category, rng: SeededRng) -> float:
	var range_data: Dictionary = get_mass_range(category)
	return rng.randf_range(range_data["min"], range_data["max"])


## Generates a random radius within a size category.
## @param category: The size category.
## @param rng: The random number generator.
## @return: Radius in Earth radii.
static func random_radius_earth(category: SizeCategory.Category, rng: SeededRng) -> float:
	var range_data: Dictionary = get_radius_range(category)
	return rng.randf_range(range_data["min"], range_data["max"])


## Generates a random density within a size category.
## @param category: The size category.
## @param rng: The random number generator.
## @return: Density in kg/m³.
static func random_density(category: SizeCategory.Category, rng: SeededRng) -> float:
	var range_data: Dictionary = get_density_range(category)
	return rng.randf_range(range_data["min"], range_data["max"])


## Calculates radius from mass and density.
## @param mass_kg: Mass in kilograms.
## @param density_kg_m3: Density in kg/m³.
## @return: Radius in meters.
static func radius_from_mass_density(mass_kg: float, density_kg_m3: float) -> float:
	if density_kg_m3 <= 0.0:
		return 0.0
	var volume: float = mass_kg / density_kg_m3
	return pow(volume * 3.0 / (4.0 * PI), 1.0 / 3.0)
