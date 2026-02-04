## Tests for ColonyType enum and helpers.
extends TestCase

const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")


## Tests to_string_name.
func test_to_string_name() -> void:
	assert_equal(ColonyType.to_string_name(ColonyType.Type.SETTLEMENT), "Settlement")
	assert_equal(ColonyType.to_string_name(ColonyType.Type.CORPORATE), "Corporate")
	assert_equal(ColonyType.to_string_name(ColonyType.Type.MILITARY), "Military")
	assert_equal(ColonyType.to_string_name(ColonyType.Type.SCIENTIFIC), "Scientific")


## Tests from_string.
func test_from_string() -> void:
	assert_equal(ColonyType.from_string("settlement"), ColonyType.Type.SETTLEMENT)
	assert_equal(ColonyType.from_string("Corporate"), ColonyType.Type.CORPORATE)
	assert_equal(ColonyType.from_string("MILITARY"), ColonyType.Type.MILITARY)
	assert_equal(ColonyType.from_string("invalid"), ColonyType.Type.SETTLEMENT)


## Tests typical_starting_regime.
func test_typical_starting_regime() -> void:
	assert_equal(ColonyType.typical_starting_regime(ColonyType.Type.CORPORATE), GovernmentType.Regime.CORPORATE)
	assert_equal(ColonyType.typical_starting_regime(ColonyType.Type.MILITARY), GovernmentType.Regime.MILITARY_JUNTA)
	assert_equal(ColonyType.typical_starting_regime(ColonyType.Type.SCIENTIFIC), GovernmentType.Regime.TECHNOCRACY)
	assert_equal(ColonyType.typical_starting_regime(ColonyType.Type.RELIGIOUS), GovernmentType.Regime.THEOCRACY)


## Tests typical_initial_population.
func test_typical_initial_population() -> void:
	var settlement_pop: int = ColonyType.typical_initial_population(ColonyType.Type.SETTLEMENT)
	var scientific_pop: int = ColonyType.typical_initial_population(ColonyType.Type.SCIENTIFIC)
	var refugee_pop: int = ColonyType.typical_initial_population(ColonyType.Type.REFUGEE)

	assert_greater_than(settlement_pop, scientific_pop, "Settlements should have more pop than scientific")
	assert_greater_than(refugee_pop, settlement_pop, "Refugee colonies should have high pop")


## Tests growth_rate_modifier.
func test_growth_rate_modifier() -> void:
	var settlement_mod: float = ColonyType.growth_rate_modifier(ColonyType.Type.SETTLEMENT)
	var military_mod: float = ColonyType.growth_rate_modifier(ColonyType.Type.MILITARY)
	var religious_mod: float = ColonyType.growth_rate_modifier(ColonyType.Type.RELIGIOUS)

	assert_float_equal(settlement_mod, 1.0, 0.01, "Settlement should have base growth")
	assert_less_than(military_mod, settlement_mod, "Military should have lower growth")
	assert_greater_than(religious_mod, settlement_mod, "Religious should have higher growth")


## Tests tends_toward_native_conflict.
func test_tends_toward_native_conflict() -> void:
	assert_true(ColonyType.tends_toward_native_conflict(ColonyType.Type.CORPORATE))
	assert_true(ColonyType.tends_toward_native_conflict(ColonyType.Type.MILITARY))
	assert_false(ColonyType.tends_toward_native_conflict(ColonyType.Type.SCIENTIFIC))
	assert_false(ColonyType.tends_toward_native_conflict(ColonyType.Type.RELIGIOUS))


## Tests count.
func test_count() -> void:
	assert_equal(ColonyType.count(), 10)
