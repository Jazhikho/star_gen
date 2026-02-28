## A set of property constraints keyed by property path.
## Tracks which properties are locked and what ranges are valid.
## Pure domain. No Nodes, no file IO.
class_name ConstraintSet
extends RefCounted


## Map of property_path -> PropertyConstraint.
var _constraints: Dictionary


## Creates an empty ConstraintSet.
func _init() -> void:
	_constraints = {}


## Adds or replaces a constraint.
## @param constraint: The constraint to store.
func set_constraint(constraint: PropertyConstraint) -> void:
	_constraints[constraint.property_path] = constraint


## Gets a constraint by property path.
## @param property_path: The property path to look up.
## @return: The constraint, or null if not present.
func get_constraint(property_path: String) -> PropertyConstraint:
	if _constraints.has(property_path):
		return _constraints[property_path] as PropertyConstraint
	return null


## Returns whether a constraint exists for the given path.
## @param property_path: The property path to check.
## @return: True if present.
func has_constraint(property_path: String) -> bool:
	return _constraints.has(property_path)


## Returns all property paths currently tracked.
## @return: Array of property path strings.
func get_all_paths() -> Array[String]:
	var paths: Array[String] = []
	for key: String in _constraints.keys():
		paths.append(key)
	return paths


## Returns property paths that are locked.
## @return: Array of locked property path strings.
func get_locked_paths() -> Array[String]:
	var paths: Array[String] = []
	for key: String in _constraints.keys():
		var c: PropertyConstraint = _constraints[key] as PropertyConstraint
		if c.is_locked:
			paths.append(key)
	return paths


## Returns locked values as a property_path -> value dictionary.
## This shape matches BaseSpec.overrides so the output can feed
## directly into a spec for constrained regeneration.
## @return: Dictionary of locked path -> current value.
func get_locked_overrides() -> Dictionary:
	var result: Dictionary = {}
	for key: String in _constraints.keys():
		var c: PropertyConstraint = _constraints[key] as PropertyConstraint
		if c.is_locked:
			result[key] = c.current_value
	return result


## Locks a property at its current value.
## No-op if the property isn't tracked.
## @param property_path: The property to lock.
func lock(property_path: String) -> void:
	var c: PropertyConstraint = get_constraint(property_path)
	if c == null:
		return
	_constraints[property_path] = c.with_lock(true)


## Unlocks a property.
## No-op if the property isn't tracked.
## @param property_path: The property to unlock.
func unlock(property_path: String) -> void:
	var c: PropertyConstraint = get_constraint(property_path)
	if c == null:
		return
	_constraints[property_path] = c.with_lock(false)


## Sets a property's current value without changing its lock state.
## Does NOT clamp — callers decide whether to clamp first.
## @param property_path: The property to update.
## @param value: New value (base units).
func set_value(property_path: String, value: float) -> void:
	var c: PropertyConstraint = get_constraint(property_path)
	if c == null:
		return
	_constraints[property_path] = c.with_value(value)


## Returns whether every constraint in the set is satisfiable
## and every current value is within its range.
## @return: True if the whole set is consistent.
func is_consistent() -> bool:
	for key: String in _constraints.keys():
		var c: PropertyConstraint = _constraints[key] as PropertyConstraint
		if not c.is_satisfiable():
			return false
		if not c.is_value_in_range():
			return false
	return true


## Returns property paths whose current value is outside their allowed range.
## @return: Array of violating property paths.
func get_violations() -> Array[String]:
	var violations: Array[String] = []
	for key: String in _constraints.keys():
		var c: PropertyConstraint = _constraints[key] as PropertyConstraint
		if not c.is_satisfiable() or not c.is_value_in_range():
			violations.append(key)
	return violations


## Clamps every property's current value into its allowed range.
## Locked properties are NOT clamped — they are authoritative.
## @return: Array of paths that were modified.
func clamp_unlocked() -> Array[String]:
	var modified: Array[String] = []
	for key: String in _constraints.keys():
		var c: PropertyConstraint = _constraints[key] as PropertyConstraint
		if c.is_locked:
			continue
		if c.is_value_in_range():
			continue
		var clamped: float = c.clamp_value(c.current_value)
		_constraints[key] = c.with_value(clamped)
		modified.append(key)
	return modified


## Returns the number of constraints in the set.
## @return: Constraint count.
func size() -> int:
	return _constraints.size()


## Returns the (min, max) range for a property as a Vector2, or the
## fallback if the property isn't tracked.
## @param property_path: The property to look up.
## @param fallback: Range to return if absent.
## @return: Vector2(min, max) in base units.
func get_range(property_path: String, fallback: Vector2 = Vector2(-INF, INF)) -> Vector2:
	var c: PropertyConstraint = get_constraint(property_path)
	if c == null:
		return fallback
	return Vector2(c.min_value, c.max_value)
