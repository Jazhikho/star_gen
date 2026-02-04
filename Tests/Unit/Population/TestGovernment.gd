## Tests for Government data model.
extends TestCase

const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Tests default creation.
func test_creation_default() -> void:
	var gov: Government = Government.new()
	assert_equal(gov.regime, GovernmentType.Regime.TRIBAL)
	assert_float_equal(gov.coercion_centralization, 0.0, 0.001)
	assert_float_equal(gov.administrative_capacity, 0.0, 0.001)
	assert_float_equal(gov.political_inclusiveness, 0.0, 0.001)
	assert_float_equal(gov.legitimacy, 0.5, 0.001)


## Tests create_native_default.
func test_create_native_default() -> void:
	var rng: SeededRng = SeededRng.new(12345)
	var gov: Government = Government.create_native_default(rng)

	var native_regimes: Array[GovernmentType.Regime] = GovernmentType.native_starting_regimes()
	assert_true(gov.regime in native_regimes, "Should have native starting regime")

	assert_less_than(gov.coercion_centralization, 0.3)
	assert_less_than(gov.administrative_capacity, 0.2)


## Tests create_colony_default.
func test_create_colony_default() -> void:
	var rng: SeededRng = SeededRng.new(12345)
	var gov: Government = Government.create_colony_default(rng)

	assert_greater_than(gov.administrative_capacity, 0.2)


## Tests create_colony_default with colony type hint.
func test_create_colony_default_corporate() -> void:
	var rng: SeededRng = SeededRng.new(12345)
	var gov: Government = Government.create_colony_default(rng, "corporate")

	assert_equal(gov.regime, GovernmentType.Regime.CORPORATE)


## Tests create_colony_default with military hint.
func test_create_colony_default_military() -> void:
	var rng: SeededRng = SeededRng.new(12345)
	var gov: Government = Government.create_colony_default(rng, "military")

	assert_equal(gov.regime, GovernmentType.Regime.MILITARY_JUNTA)


## Tests is_stable for stable regime.
func test_is_stable_true() -> void:
	var gov: Government = Government.new()
	gov.regime = GovernmentType.Regime.MASS_DEMOCRACY
	gov.legitimacy = 0.7

	assert_true(gov.is_stable())


## Tests is_stable for unstable regime.
func test_is_stable_false_unstable_regime() -> void:
	var gov: Government = Government.new()
	gov.regime = GovernmentType.Regime.FAILED_STATE
	gov.legitimacy = 0.8

	assert_false(gov.is_stable())


## Tests is_stable with low legitimacy.
func test_is_stable_false_low_legitimacy() -> void:
	var gov: Government = Government.new()
	gov.regime = GovernmentType.Regime.MASS_DEMOCRACY
	gov.legitimacy = 0.2

	assert_false(gov.is_stable())


## Tests is_regime_change_likely with very low legitimacy.
func test_is_regime_change_likely_low_legitimacy() -> void:
	var gov: Government = Government.new()
	gov.legitimacy = 0.1

	assert_true(gov.is_regime_change_likely())


## Tests get_summary.
func test_get_summary() -> void:
	var gov: Government = Government.new()
	gov.regime = GovernmentType.Regime.MASS_DEMOCRACY
	gov.coercion_centralization = 0.5
	gov.administrative_capacity = 0.6
	gov.political_inclusiveness = 0.8
	gov.legitimacy = 0.7

	var summary: Dictionary = gov.get_summary()

	assert_equal(summary["regime"], "Mass Democracy")
	assert_float_equal(summary["coercion"] as float, 0.5, 0.001)
	assert_true(summary["stable"] as bool)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: Government = Government.new()
	original.regime = GovernmentType.Regime.CONSTITUTIONAL
	original.coercion_centralization = 0.5
	original.administrative_capacity = 0.6
	original.political_inclusiveness = 0.7
	original.legitimacy = 0.8
	original.regime_established_year = -100
	original.name = "The Republic"

	var data: Dictionary = original.to_dict()
	var restored: Government = Government.from_dict(data)

	assert_equal(restored.regime, original.regime)
	assert_float_equal(restored.coercion_centralization, original.coercion_centralization, 0.001)
	assert_float_equal(restored.administrative_capacity, original.administrative_capacity, 0.001)
	assert_float_equal(restored.political_inclusiveness, original.political_inclusiveness, 0.001)
	assert_float_equal(restored.legitimacy, original.legitimacy, 0.001)
	assert_equal(restored.regime_established_year, original.regime_established_year)
	assert_equal(restored.name, original.name)


## Tests from_dict handles JSON string regime.
func test_from_dict_string_regime() -> void:
	var data: Dictionary = {
		"regime": "5",
		"legitimacy": 0.6,
	}
	var gov: Government = Government.from_dict(data)
	assert_equal(gov.regime, 5 as GovernmentType.Regime)


## Tests slider clamping in from_dict.
func test_from_dict_clamps_sliders() -> void:
	var data: Dictionary = {
		"coercion_centralization": 2.0,
		"administrative_capacity": -0.5,
		"political_inclusiveness": 0.5,
		"legitimacy": 1.5,
	}
	var gov: Government = Government.from_dict(data)

	assert_float_equal(gov.coercion_centralization, 1.0, 0.001)
	assert_float_equal(gov.administrative_capacity, 0.0, 0.001)
	assert_float_equal(gov.legitimacy, 1.0, 0.001)
