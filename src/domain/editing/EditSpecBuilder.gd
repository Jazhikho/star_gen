## Builds generator spec overrides from a ConstraintSet's locked properties.
## Pure domain logic. Bridges the editing layer (base SI units, uniform
## property paths) and the generation layer (which sometimes uses
## relative units in its override keys, e.g. "physical.mass_solar").
##
## The output is a Dictionary shaped exactly like BaseSpec.overrides,
## so a spec of the right concrete type can be created and have this
## dictionary assigned directly.
##
## Solver and dialog use stellar.temperature_k; StarGenerator already
## checks stellar.temperature_k, so no alias needed for temperature.
class_name EditSpecBuilder
extends RefCounted

const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Maps solver property paths (base SI) to extra override keys that
## generators actually check, with conversion factor name.
## Each entry: solver_path -> Array of { "key": generator_key, "factor_name": ... }.
## The base path is always written; these aliases add generator-specific keys.
const _STAR_ALIASES: Dictionary = {
	"physical.mass_kg": [
		{"key": "physical.mass_solar", "factor_name": "SOLAR_MASS_KG_INV"},
	],
	"physical.radius_m": [
		{"key": "physical.radius_solar", "factor_name": "SOLAR_RADIUS_M_INV"},
	],
	"stellar.luminosity_watts": [
		{"key": "stellar.luminosity_solar", "factor_name": "SOLAR_LUM_W_INV"},
	],
}

const _FACTOR_UNITY: float = 1.0


## Builds a spec-overrides Dictionary from the locked properties of a ConstraintSet.
## Writes every locked property at its base-unit path, plus any
## generator-specific alias paths (e.g. star mass in solar masses).
## @param body_type: CelestialType.Type — picks which alias table applies.
## @param constraints: The ConstraintSet with locked values.
## @return: Dictionary ready to assign to BaseSpec.overrides.
static func build_overrides(body_type: int, constraints: ConstraintSet) -> Dictionary:
	var overrides: Dictionary = constraints.get_locked_overrides()
	var aliases: Dictionary = _alias_table_for(body_type)
	var locked_paths: Array = overrides.keys()
	for path: Variant in locked_paths:
		var path_str: String = path as String
		if not aliases.has(path_str):
			continue
		var base_val: float = overrides[path] as float
		var alias_list: Array = aliases[path_str] as Array
		for entry: Variant in alias_list:
			var e: Dictionary = entry as Dictionary
			var key: String = e["key"] as String
			var factor: float = _resolve_factor(e["factor_name"] as String)
			overrides[key] = base_val * factor
	return overrides


## Resolves a named conversion factor (base -> generator units).
## @param name: Factor name.
## @return: Multiplicative factor.
static func _resolve_factor(name: String) -> float:
	match name:
		"UNITY":
			return _FACTOR_UNITY
		"SOLAR_MASS_KG_INV":
			return 1.0 / Units.SOLAR_MASS_KG
		"SOLAR_RADIUS_M_INV":
			return 1.0 / Units.SOLAR_RADIUS_METERS
		"SOLAR_LUM_W_INV":
			return 1.0 / StellarProps.SOLAR_LUMINOSITY_WATTS
		_:
			return _FACTOR_UNITY


## Picks the alias table for the given body type.
## Planets/moons/asteroids use base-unit paths only.
## @param body_type: CelestialType.Type.
## @return: Alias dictionary (may be empty).
static func _alias_table_for(body_type: int) -> Dictionary:
	if body_type == CelestialType.Type.STAR:
		return _STAR_ALIASES
	return {}


## Populates a spec's overrides from a ConstraintSet in one call.
## Existing overrides are cleared first; locked properties are the source of truth.
## @param spec: The BaseSpec (or subclass) to mutate.
## @param body_type: CelestialType.Type.
## @param constraints: ConstraintSet with locked values.
static func apply_to_spec(spec: BaseSpec, body_type: int, constraints: ConstraintSet) -> void:
	spec.clear_overrides()
	var overrides: Dictionary = build_overrides(body_type, constraints)
	for key: Variant in overrides.keys():
		spec.set_override(key as String, overrides[key])
