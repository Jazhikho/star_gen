## Tests for HabitabilityCategory enum and utilities.
extends TestCase

const _habitability_category: GDScript = preload("res://src/domain/population/HabitabilityCategory.gd")


## Tests from_score returns correct categories.
func test_from_score() -> void:
	assert_equal(HabitabilityCategory.from_score(0), HabitabilityCategory.Category.IMPOSSIBLE)
	assert_equal(HabitabilityCategory.from_score(1), HabitabilityCategory.Category.HOSTILE)
	assert_equal(HabitabilityCategory.from_score(2), HabitabilityCategory.Category.HOSTILE)
	assert_equal(HabitabilityCategory.from_score(3), HabitabilityCategory.Category.HARSH)
	assert_equal(HabitabilityCategory.from_score(4), HabitabilityCategory.Category.HARSH)
	assert_equal(HabitabilityCategory.from_score(5), HabitabilityCategory.Category.MARGINAL)
	assert_equal(HabitabilityCategory.from_score(6), HabitabilityCategory.Category.MARGINAL)
	assert_equal(HabitabilityCategory.from_score(7), HabitabilityCategory.Category.CHALLENGING)
	assert_equal(HabitabilityCategory.from_score(8), HabitabilityCategory.Category.COMFORTABLE)
	assert_equal(HabitabilityCategory.from_score(9), HabitabilityCategory.Category.COMFORTABLE)
	assert_equal(HabitabilityCategory.from_score(10), HabitabilityCategory.Category.IDEAL)


## Tests from_score clamps out of range values.
func test_from_score_clamping() -> void:
	assert_equal(HabitabilityCategory.from_score(-5), HabitabilityCategory.Category.IMPOSSIBLE)
	assert_equal(HabitabilityCategory.from_score(15), HabitabilityCategory.Category.IDEAL)


## Tests to_string_name returns correct values.
func test_to_string_name() -> void:
	assert_equal(HabitabilityCategory.to_string_name(HabitabilityCategory.Category.IMPOSSIBLE), "Impossible")
	assert_equal(HabitabilityCategory.to_string_name(HabitabilityCategory.Category.HOSTILE), "Hostile")
	assert_equal(HabitabilityCategory.to_string_name(HabitabilityCategory.Category.HARSH), "Harsh")
	assert_equal(HabitabilityCategory.to_string_name(HabitabilityCategory.Category.MARGINAL), "Marginal")
	assert_equal(HabitabilityCategory.to_string_name(HabitabilityCategory.Category.CHALLENGING), "Challenging")
	assert_equal(HabitabilityCategory.to_string_name(HabitabilityCategory.Category.COMFORTABLE), "Comfortable")
	assert_equal(HabitabilityCategory.to_string_name(HabitabilityCategory.Category.IDEAL), "Ideal")


## Tests from_string parses correctly.
func test_from_string() -> void:
	assert_equal(HabitabilityCategory.from_string("impossible"), HabitabilityCategory.Category.IMPOSSIBLE)
	assert_equal(HabitabilityCategory.from_string("HOSTILE"), HabitabilityCategory.Category.HOSTILE)
	assert_equal(HabitabilityCategory.from_string("Ideal"), HabitabilityCategory.Category.IDEAL)


## Tests from_string returns IMPOSSIBLE for unknown values.
func test_from_string_unknown() -> void:
	assert_equal(HabitabilityCategory.from_string("unknown"), HabitabilityCategory.Category.IMPOSSIBLE)
	assert_equal(HabitabilityCategory.from_string(""), HabitabilityCategory.Category.IMPOSSIBLE)


## Tests allows_unassisted_survival returns expected values.
func test_allows_unassisted_survival() -> void:
	assert_false(HabitabilityCategory.allows_unassisted_survival(HabitabilityCategory.Category.IMPOSSIBLE))
	assert_false(HabitabilityCategory.allows_unassisted_survival(HabitabilityCategory.Category.HOSTILE))
	assert_false(HabitabilityCategory.allows_unassisted_survival(HabitabilityCategory.Category.HARSH))
	assert_true(HabitabilityCategory.allows_unassisted_survival(HabitabilityCategory.Category.MARGINAL))
	assert_true(HabitabilityCategory.allows_unassisted_survival(HabitabilityCategory.Category.CHALLENGING))
	assert_true(HabitabilityCategory.allows_unassisted_survival(HabitabilityCategory.Category.COMFORTABLE))
	assert_true(HabitabilityCategory.allows_unassisted_survival(HabitabilityCategory.Category.IDEAL))


## Tests get_description returns non-empty strings.
func test_get_description() -> void:
	for cat_int in range(HabitabilityCategory.count()):
		var cat: HabitabilityCategory.Category = cat_int as HabitabilityCategory.Category
		var desc: String = HabitabilityCategory.get_description(cat)
		assert_true(desc.length() > 0, "Description should not be empty for category %d" % cat_int)


## Tests count returns correct number.
func test_count() -> void:
	assert_equal(HabitabilityCategory.count(), 7)
