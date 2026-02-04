## Tests for TechnologyLevel enum and helpers.
extends TestCase

const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")


## Tests to_string_name.
func test_to_string_name() -> void:
	assert_equal(TechnologyLevel.to_string_name(TechnologyLevel.Level.STONE_AGE), "Stone Age")
	assert_equal(TechnologyLevel.to_string_name(TechnologyLevel.Level.INDUSTRIAL), "Industrial")
	assert_equal(TechnologyLevel.to_string_name(TechnologyLevel.Level.INTERSTELLAR), "Interstellar")


## Tests from_string.
func test_from_string() -> void:
	assert_equal(TechnologyLevel.from_string("stone_age"), TechnologyLevel.Level.STONE_AGE)
	assert_equal(TechnologyLevel.from_string("Industrial"), TechnologyLevel.Level.INDUSTRIAL)
	assert_equal(TechnologyLevel.from_string("invalid"), TechnologyLevel.Level.STONE_AGE)


## Tests next_level.
func test_next_level() -> void:
	assert_equal(TechnologyLevel.next_level(TechnologyLevel.Level.STONE_AGE), TechnologyLevel.Level.BRONZE_AGE)
	assert_equal(TechnologyLevel.next_level(TechnologyLevel.Level.INDUSTRIAL), TechnologyLevel.Level.ATOMIC)

	assert_equal(TechnologyLevel.next_level(TechnologyLevel.Level.ADVANCED), TechnologyLevel.Level.ADVANCED)


## Tests previous_level.
func test_previous_level() -> void:
	assert_equal(TechnologyLevel.previous_level(TechnologyLevel.Level.BRONZE_AGE), TechnologyLevel.Level.STONE_AGE)

	assert_equal(TechnologyLevel.previous_level(TechnologyLevel.Level.STONE_AGE), TechnologyLevel.Level.STONE_AGE)


## Tests can_spaceflight.
func test_can_spaceflight() -> void:
	assert_false(TechnologyLevel.can_spaceflight(TechnologyLevel.Level.STONE_AGE))
	assert_false(TechnologyLevel.can_spaceflight(TechnologyLevel.Level.INDUSTRIAL))
	assert_false(TechnologyLevel.can_spaceflight(TechnologyLevel.Level.INFORMATION))
	assert_true(TechnologyLevel.can_spaceflight(TechnologyLevel.Level.SPACEFARING))
	assert_true(TechnologyLevel.can_spaceflight(TechnologyLevel.Level.INTERSTELLAR))


## Tests can_interstellar.
func test_can_interstellar() -> void:
	assert_false(TechnologyLevel.can_interstellar(TechnologyLevel.Level.SPACEFARING))
	assert_true(TechnologyLevel.can_interstellar(TechnologyLevel.Level.INTERSTELLAR))
	assert_true(TechnologyLevel.can_interstellar(TechnologyLevel.Level.ADVANCED))


## Tests typical_years_to_reach increases monotonically.
func test_typical_years_to_reach_increases() -> void:
	var prev_years: int = -1
	for i in range(TechnologyLevel.count()):
		var level: TechnologyLevel.Level = i as TechnologyLevel.Level
		var years: int = TechnologyLevel.typical_years_to_reach(level)
		assert_greater_than(years, prev_years - 1, "Years should generally increase")
		prev_years = years


## Tests count.
func test_count() -> void:
	assert_equal(TechnologyLevel.count(), 12)
