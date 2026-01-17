## Tests for SizeTable lookups.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests mass range retrieval.
func test_mass_ranges() -> void:
	var dwarf_range: Dictionary = SizeTable.get_mass_range(SizeCategory.Category.DWARF)
	assert_float_equal(dwarf_range["min"], 0.0001)
	assert_float_equal(dwarf_range["max"], 0.01)
	
	var giant_range: Dictionary = SizeTable.get_mass_range(SizeCategory.Category.GAS_GIANT)
	assert_float_equal(giant_range["min"], 80.0)
	assert_float_equal(giant_range["max"], 4000.0)


## Tests category detection from mass.
func test_category_from_mass() -> void:
	assert_equal(SizeTable.category_from_mass(0.001), SizeCategory.Category.DWARF)
	assert_equal(SizeTable.category_from_mass(0.1), SizeCategory.Category.SUB_TERRESTRIAL)
	assert_equal(SizeTable.category_from_mass(1.0), SizeCategory.Category.TERRESTRIAL)
	assert_equal(SizeTable.category_from_mass(5.0), SizeCategory.Category.SUPER_EARTH)
	assert_equal(SizeTable.category_from_mass(15.0), SizeCategory.Category.MINI_NEPTUNE)
	assert_equal(SizeTable.category_from_mass(50.0), SizeCategory.Category.NEPTUNE_CLASS)
	assert_equal(SizeTable.category_from_mass(500.0), SizeCategory.Category.GAS_GIANT)


## Tests random mass generation stays in range.
func test_random_mass_in_range() -> void:
	var rng: SeededRng = SeededRng.new(12345)
	
	for category in SizeCategory.Category.values():
		var range_data: Dictionary = SizeTable.get_mass_range(category)
		for i in range(10):
			var mass: float = SizeTable.random_mass_earth(category, rng)
			assert_in_range(mass, range_data["min"], range_data["max"])


## Tests radius from mass and density.
func test_radius_from_mass_density() -> void:
	# Earth: 5.972e24 kg, 5515 kg/m³ -> ~6371 km
	var radius: float = SizeTable.radius_from_mass_density(5.972e24, 5515.0)
	assert_in_range(radius, 6.0e6, 6.5e6)


## Tests density ranges are realistic.
func test_density_ranges_realistic() -> void:
	# Rocky bodies should have higher density than gas giants
	var rocky_density: Dictionary = SizeTable.get_density_range(SizeCategory.Category.TERRESTRIAL)
	var gas_density: Dictionary = SizeTable.get_density_range(SizeCategory.Category.GAS_GIANT)
	
	assert_greater_than(rocky_density["min"], gas_density["max"])
