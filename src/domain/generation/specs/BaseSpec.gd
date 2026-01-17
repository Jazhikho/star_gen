## Base class for all generation specifications.
## Contains common fields shared by all body type specs.
class_name BaseSpec
extends RefCounted


## The seed used for deterministic generation.
var generation_seed: int

## Optional name hint for the generated body.
var name_hint: String

## Field overrides that lock specific values during generation.
## Keys are field paths (e.g., "physical.mass_kg"), values are locked values.
var overrides: Dictionary


## Creates a new BaseSpec instance.
## @param p_generation_seed: The generation seed.
## @param p_name_hint: Optional name hint.
## @param p_overrides: Optional field overrides.
func _init(
	p_generation_seed: int = 0,
	p_name_hint: String = "",
	p_overrides: Dictionary = {}
) -> void:
	generation_seed = p_generation_seed
	name_hint = p_name_hint
	overrides = p_overrides.duplicate()


## Returns whether a specific field has an override.
## @param field_path: The field path to check.
## @return: True if the field is overridden.
func has_override(field_path: String) -> bool:
	return overrides.has(field_path)


## Gets the override value for a field, or a default if not overridden.
## @param field_path: The field path to get.
## @param default_value: Value to return if not overridden.
## @return: The override value or default.
func get_override(field_path: String, default_value: Variant) -> Variant:
	if overrides.has(field_path):
		return overrides[field_path]
	return default_value


## Gets the override value as a float, or default if not overridden.
## @param field_path: The field path to get.
## @param default_value: Value to return if not overridden.
## @return: The override value as float or default.
func get_override_float(field_path: String, default_value: float) -> float:
	if overrides.has(field_path):
		return overrides[field_path] as float
	return default_value


## Gets the override value as an int, or default if not overridden.
## @param field_path: The field path to get.
## @param default_value: Value to return if not overridden.
## @return: The override value as int or default.
func get_override_int(field_path: String, default_value: int) -> int:
	if overrides.has(field_path):
		return overrides[field_path] as int
	return default_value


## Sets an override value.
## @param field_path: The field path to set.
## @param value: The value to lock.
func set_override(field_path: String, value: Variant) -> void:
	overrides[field_path] = value


## Removes an override.
## @param field_path: The field path to unlock.
func remove_override(field_path: String) -> void:
	overrides.erase(field_path)


## Clears all overrides.
func clear_overrides() -> void:
	overrides.clear()


## Converts base fields to dictionary for serialization.
## Subclasses should call this and extend the result.
## @return: Dictionary with base fields.
func _base_to_dict() -> Dictionary:
	return {
		"generation_seed": generation_seed,
		"name_hint": name_hint,
		"overrides": overrides.duplicate(),
	}


## Populates base fields from a dictionary.
## Subclasses should call this in their from_dict.
## @param data: The dictionary to read from.
func _base_from_dict(data: Dictionary) -> void:
	generation_seed = data.get("generation_seed", 0) as int
	name_hint = data.get("name_hint", "") as String
	overrides = (data.get("overrides", {}) as Dictionary).duplicate()
