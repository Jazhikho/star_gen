## A single property's constraint: valid range, current value, lock state.
## Immutable after construction — create a new one via with_* to change.
## Pure value type. No Nodes, no file IO.
class_name PropertyConstraint
extends RefCounted

## Self-reference for with_* methods so class_name is not required during parse.
const _script_ref: GDScript = preload("res://src/domain/editing/PropertyConstraint.gd")


## The property path this constraint applies to (e.g. "physical.mass_kg").
var property_path: String

## Minimum allowed value in base (SI) units. -INF if unbounded below.
var min_value: float

## Maximum allowed value in base (SI) units. +INF if unbounded above.
var max_value: float

## Current value in base (SI) units.
var current_value: float

## Whether the user has locked this property.
## Locked properties constrain the valid ranges of dependent properties.
var is_locked: bool

## Free-text reason explaining why this range is what it is.
## Useful for UI tooltips ("constrained by locked mass").
var constraint_reason: String


## Creates a new PropertyConstraint.
## @param p_property_path: Property path (e.g. "physical.mass_kg").
## @param p_min_value: Minimum allowed value (base units). Use -INF for none.
## @param p_max_value: Maximum allowed value (base units). Use +INF for none.
## @param p_current_value: Current value (base units).
## @param p_is_locked: Whether this property is user-locked.
## @param p_constraint_reason: Human-readable explanation for the range.
func _init(
	p_property_path: String = "",
	p_min_value: float = - INF,
	p_max_value: float = INF,
	p_current_value: float = 0.0,
	p_is_locked: bool = false,
	p_constraint_reason: String = ""
) -> void:
	property_path = p_property_path
	min_value = p_min_value
	max_value = p_max_value
	current_value = p_current_value
	is_locked = p_is_locked
	constraint_reason = p_constraint_reason


## Returns whether current_value falls inside [min_value, max_value].
## @return: True if current value is within the allowed range.
func is_value_in_range() -> bool:
	return current_value >= min_value and current_value <= max_value


## Clamps the given value into this constraint's range.
## @param value: The value to clamp (base units).
## @return: Clamped value.
func clamp_value(value: float) -> float:
	return clampf(value, min_value, max_value)


## Returns whether this constraint has any finite bounds at all.
## @return: True if min or max is finite.
func has_bounds() -> bool:
	return is_finite(min_value) or is_finite(max_value)


## Returns a copy with the lock flag changed.
## @param locked: New lock state.
## @return: New PropertyConstraint with updated lock.
func with_lock(locked: bool) -> PropertyConstraint:
	return _script_ref.new(
		property_path, min_value, max_value,
		current_value, locked, constraint_reason
	) as PropertyConstraint


## Returns a copy with a new current value (does NOT clamp).
## @param new_value: New current value (base units).
## @return: New PropertyConstraint with updated value.
func with_value(new_value: float) -> PropertyConstraint:
	return _script_ref.new(
		property_path, min_value, max_value,
		new_value, is_locked, constraint_reason
	) as PropertyConstraint


## Returns a copy with a narrowed range (intersection with given bounds).
## If the intersection is empty, min will exceed max — caller should check.
## @param new_min: New minimum to intersect with.
## @param new_max: New maximum to intersect with.
## @param reason: Reason for the narrowing (appended to existing reason).
## @return: New PropertyConstraint with intersected range.
func intersected_with(
	new_min: float,
	new_max: float,
	reason: String = ""
) -> PropertyConstraint:
	var combined_reason: String = constraint_reason
	# Append reason only when narrowing actually changes something,
	# so the UI tooltip shows the *active* constraints, not noise.
	var narrowed: bool = new_min > min_value or new_max < max_value
	if narrowed and not reason.is_empty():
		if combined_reason.is_empty():
			combined_reason = reason
		else:
			combined_reason = combined_reason + "; " + reason
	return _script_ref.new(
		property_path,
		maxf(min_value, new_min),
		minf(max_value, new_max),
		current_value,
		is_locked,
		combined_reason
	) as PropertyConstraint


## Returns whether this constraint's range is non-empty (min <= max).
## @return: True if the range is satisfiable.
func is_satisfiable() -> bool:
	return min_value <= max_value


## Converts this constraint to a dictionary for snapshotting / debugging.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"property_path": property_path,
		"min_value": min_value,
		"max_value": max_value,
		"current_value": current_value,
		"is_locked": is_locked,
		"constraint_reason": constraint_reason,
	}
