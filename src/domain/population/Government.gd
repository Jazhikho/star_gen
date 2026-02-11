## Government/political structure of a population.
## Tracks regime type and the three "sliders" from the regime change model.
class_name Government
extends RefCounted

# Preload for headless compilation.
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Current regime type.
var regime: GovernmentType.Regime = GovernmentType.Regime.TRIBAL

## Coercion centralization (0-1): Who controls organized force?
## 0 = fragmented/local, 1 = fully centralized state monopoly.
var coercion_centralization: float = 0.0

## Administrative/fiscal capacity (0-1): Ability to tax, count, enforce, deliver.
## 0 = no state capacity, 1 = highly capable bureaucracy.
var administrative_capacity: float = 0.0

## Political inclusiveness (0-1): How many people matter politically?
## 0 = tiny elite only, 1 = universal participation.
var political_inclusiveness: float = 0.0

## Legitimacy level (0-1): How accepted is the current regime?
## 0 = no legitimacy, 1 = fully accepted.
var legitimacy: float = 0.5

## Year the current regime was established.
var regime_established_year: int = 0

## Optional name/title for the government (e.g., "The Republic of X").
var name: String = ""


## Creates a new Government with default values.
func _init() -> void:
	pass


## Creates a Government appropriate for a native population starting point.
## @param rng: Seeded random number generator.
## @return: A new Government configured for natives.
static func create_native_default(rng: SeededRng) -> Government:
	var gov: Government = Government.new()

	var starting: Array[GovernmentType.Regime] = GovernmentType.native_starting_regimes()
	gov.regime = starting[rng.randi_range(0, starting.size() - 1)]

	gov.coercion_centralization = rng.randf_range(0.05, 0.2)
	gov.administrative_capacity = rng.randf_range(0.05, 0.15)
	gov.political_inclusiveness = rng.randf_range(0.2, 0.5)
	gov.legitimacy = rng.randf_range(0.6, 0.9)

	return gov


## Creates a Government appropriate for a colony starting point.
## @param rng: Seeded random number generator.
## @param colony_type: Optional hint for colony type (affects starting regime).
## @return: A new Government configured for a colony.
static func create_colony_default(rng: SeededRng, colony_type: String = "") -> Government:
	var gov: Government = Government.new()

	var starting: Array[GovernmentType.Regime] = GovernmentType.colony_starting_regimes()

	match colony_type.to_lower():
		"corporate", "mining", "commercial":
			gov.regime = GovernmentType.Regime.CORPORATE
		"military", "outpost", "strategic":
			gov.regime = GovernmentType.Regime.MILITARY_JUNTA
		"scientific", "research":
			gov.regime = GovernmentType.Regime.TECHNOCRACY
		"religious", "faith":
			gov.regime = GovernmentType.Regime.THEOCRACY
		"penal", "exile":
			gov.regime = GovernmentType.Regime.MILITARY_JUNTA
		_:
			gov.regime = starting[rng.randi_range(0, starting.size() - 1)]

	gov.coercion_centralization = rng.randf_range(0.4, 0.7)
	gov.administrative_capacity = rng.randf_range(0.3, 0.6)
	gov.political_inclusiveness = rng.randf_range(0.1, 0.4)
	gov.legitimacy = rng.randf_range(0.4, 0.7)

	return gov


## Returns whether this government is in a stable state.
## @return: True if legitimacy is adequate and regime is not inherently unstable.
func is_stable() -> bool:
	if GovernmentType.is_unstable(regime):
		return false
	return legitimacy > 0.3


## Returns whether conditions favor a regime change.
## @return: True if legitimacy is low or sliders don't match regime.
func is_regime_change_likely() -> bool:
	if legitimacy < 0.2:
		return true
	var mismatch: float = _calculate_slider_mismatch()
	return mismatch > 0.5


## Calculates how well current sliders match the regime type.
## @return: Mismatch score (0 = perfect fit, 1 = severe mismatch).
func _calculate_slider_mismatch() -> float:
	var expected: Dictionary = _get_expected_slider_ranges()
	var mismatch: float = 0.0

	if coercion_centralization < expected["coercion_min"]:
		mismatch += expected["coercion_min"] - coercion_centralization
	elif coercion_centralization > expected["coercion_max"]:
		mismatch += coercion_centralization - expected["coercion_max"]

	if administrative_capacity < expected["admin_min"]:
		mismatch += expected["admin_min"] - administrative_capacity
	elif administrative_capacity > expected["admin_max"]:
		mismatch += administrative_capacity - expected["admin_max"]

	if political_inclusiveness < expected["inclusive_min"]:
		mismatch += expected["inclusive_min"] - political_inclusiveness
	elif political_inclusiveness > expected["inclusive_max"]:
		mismatch += political_inclusiveness - expected["inclusive_max"]

	return clampf(mismatch, 0.0, 1.0)


## Returns expected slider ranges for current regime.
## @return: Dictionary with min/max for each slider.
func _get_expected_slider_ranges() -> Dictionary:
	var ranges: Dictionary = {
		"coercion_min": 0.0, "coercion_max": 1.0,
		"admin_min": 0.0, "admin_max": 1.0,
		"inclusive_min": 0.0, "inclusive_max": 1.0,
	}

	match regime:
		GovernmentType.Regime.TRIBAL:
			ranges = {"coercion_min": 0.0, "coercion_max": 0.3,
				"admin_min": 0.0, "admin_max": 0.2,
				"inclusive_min": 0.2, "inclusive_max": 0.6}
		GovernmentType.Regime.CHIEFDOM:
			ranges = {"coercion_min": 0.1, "coercion_max": 0.4,
				"admin_min": 0.1, "admin_max": 0.3,
				"inclusive_min": 0.1, "inclusive_max": 0.4}
		GovernmentType.Regime.FEUDAL:
			ranges = {"coercion_min": 0.2, "coercion_max": 0.5,
				"admin_min": 0.1, "admin_max": 0.4,
				"inclusive_min": 0.05, "inclusive_max": 0.2}
		GovernmentType.Regime.ABSOLUTE_MONARCHY:
			ranges = {"coercion_min": 0.5, "coercion_max": 0.9,
				"admin_min": 0.3, "admin_max": 0.7,
				"inclusive_min": 0.0, "inclusive_max": 0.2}
		GovernmentType.Regime.BUREAUCRATIC_EMPIRE:
			ranges = {"coercion_min": 0.6, "coercion_max": 1.0,
				"admin_min": 0.5, "admin_max": 1.0,
				"inclusive_min": 0.0, "inclusive_max": 0.3}
		GovernmentType.Regime.MASS_DEMOCRACY:
			ranges = {"coercion_min": 0.4, "coercion_max": 0.8,
				"admin_min": 0.4, "admin_max": 0.9,
				"inclusive_min": 0.6, "inclusive_max": 1.0}
		GovernmentType.Regime.MILITARY_JUNTA:
			ranges = {"coercion_min": 0.6, "coercion_max": 1.0,
				"admin_min": 0.2, "admin_max": 0.6,
				"inclusive_min": 0.0, "inclusive_max": 0.2}
		GovernmentType.Regime.FAILED_STATE:
			ranges = {"coercion_min": 0.0, "coercion_max": 0.3,
				"admin_min": 0.0, "admin_max": 0.2,
				"inclusive_min": 0.0, "inclusive_max": 0.3}
		_:
			pass

	return ranges


## Returns a summary of the government state.
## @return: Dictionary with regime name and slider values.
func get_summary() -> Dictionary:
	return {
		"regime": GovernmentType.to_string_name(regime),
		"coercion": coercion_centralization,
		"admin_capacity": administrative_capacity,
		"inclusiveness": political_inclusiveness,
		"legitimacy": legitimacy,
		"stable": is_stable(),
	}


## Converts this government to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"regime": regime as int,
		"coercion_centralization": coercion_centralization,
		"administrative_capacity": administrative_capacity,
		"political_inclusiveness": political_inclusiveness,
		"legitimacy": legitimacy,
		"regime_established_year": regime_established_year,
		"name": name,
	}


## Creates a Government from a dictionary.
## @param data: The dictionary to parse.
## @return: A new Government instance.
static func from_dict(data: Dictionary) -> Government:
	var gov: Government = Government.new()

	var regime_val: Variant = data.get("regime", 0)
	if regime_val is String:
		regime_val = int(regime_val as String)
	gov.regime = regime_val as GovernmentType.Regime

	gov.coercion_centralization = clampf(data.get("coercion_centralization", 0.0) as float, 0.0, 1.0)
	gov.administrative_capacity = clampf(data.get("administrative_capacity", 0.0) as float, 0.0, 1.0)
	gov.political_inclusiveness = clampf(data.get("political_inclusiveness", 0.0) as float, 0.0, 1.0)
	gov.legitimacy = clampf(data.get("legitimacy", 0.5) as float, 0.0, 1.0)
	gov.regime_established_year = data.get("regime_established_year", 0) as int
	gov.name = data.get("name", "") as String

	return gov
