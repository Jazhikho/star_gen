## Validation utilities for checking value constraints.
## Used by generators and editors to enforce valid data.
class_name Validation
extends RefCounted

const VALIDATION_BRIDGE_CLASS: StringName = &"CSharpValidationBridge"


## Checks if a float is positive (greater than zero).
## @param value: The value to check.
## @return: True if positive, false otherwise.
static func is_positive_float(value: float) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsPositiveFloat"):
		return bool(bridge.call("IsPositiveFloat", value))
	return value > 0.0


## Checks if a float is non-negative (greater than or equal to zero).
## @param value: The value to check.
## @return: True if non-negative, false otherwise.
static func is_non_negative_float(value: float) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsNonNegativeFloat"):
		return bool(bridge.call("IsNonNegativeFloat", value))
	return value >= 0.0


## Checks if an int is positive (greater than zero).
## @param value: The value to check.
## @return: True if positive, false otherwise.
static func is_positive_int(value: int) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsPositiveInt"):
		return bool(bridge.call("IsPositiveInt", value))
	return value > 0


## Checks if an int is non-negative (greater than or equal to zero).
## @param value: The value to check.
## @return: True if non-negative, false otherwise.
static func is_non_negative_int(value: int) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsNonNegativeInt"):
		return bool(bridge.call("IsNonNegativeInt", value))
	return value >= 0


## Checks if a float is within a valid range (inclusive).
## Delegates to MathUtils to avoid duplication.
## @param value: The value to check.
## @param min_val: The minimum allowed value.
## @param max_val: The maximum allowed value.
## @return: True if in range, false otherwise.
static func is_in_range_float(value: float, min_val: float, max_val: float) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsInRangeFloat"):
		return bool(bridge.call("IsInRangeFloat", value, min_val, max_val))
	return MathUtils.is_in_range_float(value, min_val, max_val)


## Checks if an int is within a valid range (inclusive).
## Delegates to MathUtils to avoid duplication.
## @param value: The value to check.
## @param min_val: The minimum allowed value.
## @param max_val: The maximum allowed value.
## @return: True if in range, false otherwise.
static func is_in_range_int(value: int, min_val: int, max_val: int) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsInRangeInt"):
		return bool(bridge.call("IsInRangeInt", value, min_val, max_val))
	return MathUtils.is_in_range_int(value, min_val, max_val)


## Checks if a string is not empty.
## @param value: The string to check.
## @return: True if not empty, false otherwise.
static func is_not_empty_string(value: String) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsNotEmptyString"):
		return bool(bridge.call("IsNotEmptyString", value))
	return value.length() > 0


## Checks if an array is not empty.
## @param value: The array to check.
## @return: True if not empty, false otherwise.
static func is_not_empty_array(value: Array) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsNotEmptyArray"):
		return bool(bridge.call("IsNotEmptyArray", value))
	return value.size() > 0


## Checks if a value is a valid enum value (within enum range).
## @param value: The int value to check.
## @param enum_size: The number of values in the enum.
## @return: True if valid enum value, false otherwise.
static func is_valid_enum(value: int, enum_size: int) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsValidEnum"):
		return bool(bridge.call("IsValidEnum", value, enum_size))
	return value >= 0 and value < enum_size


## Checks if a seed value is valid (any int is valid, but we may want to normalize).
## @param _value: The seed value (unused, all ints are valid).
## @return: True (all int seeds are valid).
static func is_valid_seed(_value: int) -> bool:
	var bridge: Object = null
	if ClassDB.class_exists(VALIDATION_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VALIDATION_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("IsValidSeed"):
		return bool(bridge.call("IsValidSeed", _value))
	return true
