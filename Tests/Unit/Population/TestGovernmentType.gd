## Tests for GovernmentType enum and helpers.
extends TestCase

const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")


## Tests to_string_name for all regimes.
func test_to_string_name() -> void:
	assert_equal(GovernmentType.to_string_name(GovernmentType.Regime.TRIBAL), "Tribal")
	assert_equal(GovernmentType.to_string_name(GovernmentType.Regime.MASS_DEMOCRACY), "Mass Democracy")
	assert_equal(GovernmentType.to_string_name(GovernmentType.Regime.MILITARY_JUNTA), "Military Junta")
	assert_equal(GovernmentType.to_string_name(GovernmentType.Regime.CORPORATE), "Corporate Governance")


## Tests from_string conversion.
func test_from_string() -> void:
	assert_equal(GovernmentType.from_string("tribal"), GovernmentType.Regime.TRIBAL)
	assert_equal(GovernmentType.from_string("Mass Democracy"), GovernmentType.Regime.MASS_DEMOCRACY)
	assert_equal(GovernmentType.from_string("military_junta"), GovernmentType.Regime.MILITARY_JUNTA)
	assert_equal(GovernmentType.from_string("invalid"), GovernmentType.Regime.TRIBAL)


## Tests native_starting_regimes.
func test_native_starting_regimes() -> void:
	var regimes: Array[GovernmentType.Regime] = GovernmentType.native_starting_regimes()
	assert_true(GovernmentType.Regime.TRIBAL in regimes)
	assert_true(GovernmentType.Regime.CHIEFDOM in regimes)
	assert_false(GovernmentType.Regime.CORPORATE in regimes)


## Tests colony_starting_regimes.
func test_colony_starting_regimes() -> void:
	var regimes: Array[GovernmentType.Regime] = GovernmentType.colony_starting_regimes()
	assert_true(GovernmentType.Regime.CORPORATE in regimes)
	assert_true(GovernmentType.Regime.MILITARY_JUNTA in regimes)
	assert_false(GovernmentType.Regime.TRIBAL in regimes)


## Tests baseline_transitions returns valid options.
func test_baseline_transitions() -> void:
	var tribal_next: Array[GovernmentType.Regime] = GovernmentType.baseline_transitions(GovernmentType.Regime.TRIBAL)
	assert_true(GovernmentType.Regime.CHIEFDOM in tribal_next)

	var democracy_next: Array[GovernmentType.Regime] = GovernmentType.baseline_transitions(GovernmentType.Regime.MASS_DEMOCRACY)
	assert_greater_than(democracy_next.size(), 0)


## Tests crisis_transitions returns valid options.
func test_crisis_transitions() -> void:
	var democracy_crisis: Array[GovernmentType.Regime] = GovernmentType.crisis_transitions(GovernmentType.Regime.MASS_DEMOCRACY)
	assert_true(GovernmentType.Regime.MILITARY_JUNTA in democracy_crisis)


## Tests is_authoritarian.
func test_is_authoritarian() -> void:
	assert_true(GovernmentType.is_authoritarian(GovernmentType.Regime.ABSOLUTE_MONARCHY))
	assert_true(GovernmentType.is_authoritarian(GovernmentType.Regime.MILITARY_JUNTA))
	assert_false(GovernmentType.is_authoritarian(GovernmentType.Regime.MASS_DEMOCRACY))
	assert_false(GovernmentType.is_authoritarian(GovernmentType.Regime.TRIBAL))


## Tests is_participatory.
func test_is_participatory() -> void:
	assert_true(GovernmentType.is_participatory(GovernmentType.Regime.MASS_DEMOCRACY))
	assert_true(GovernmentType.is_participatory(GovernmentType.Regime.TRIBAL))
	assert_false(GovernmentType.is_participatory(GovernmentType.Regime.ABSOLUTE_MONARCHY))


## Tests is_unstable.
func test_is_unstable() -> void:
	assert_true(GovernmentType.is_unstable(GovernmentType.Regime.FAILED_STATE))
	assert_true(GovernmentType.is_unstable(GovernmentType.Regime.MILITARY_JUNTA))
	assert_false(GovernmentType.is_unstable(GovernmentType.Regime.MASS_DEMOCRACY))


## Tests count.
func test_count() -> void:
	assert_equal(GovernmentType.count(), 18)
