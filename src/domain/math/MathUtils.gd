## Shared math utilities for generation and validation.
## Provides range checking, remapping, and other common operations.
class_name MathUtils
extends RefCounted


## Checks if a float value is within a range (inclusive).
## @param value: The value to check.
## @param min_val: The minimum allowed value.
## @param max_val: The maximum allowed value.
## @return: True if value is in range, false otherwise.
static func is_in_range_float(value: float, min_val: float, max_val: float) -> bool:
	return value >= min_val and value <= max_val


## Checks if an int value is within a range (inclusive).
## @param value: The value to check.
## @param min_val: The minimum allowed value.
## @param max_val: The maximum allowed value.
## @return: True if value is in range, false otherwise.
static func is_in_range_int(value: int, min_val: int, max_val: int) -> bool:
	return value >= min_val and value <= max_val


## Remaps a value from one range to another.
## @param value: The value to remap.
## @param from_min: The minimum of the source range.
## @param from_max: The maximum of the source range.
## @param to_min: The minimum of the target range.
## @param to_max: The maximum of the target range.
## @return: The remapped value.
static func remap(
	value: float,
	from_min: float,
	from_max: float,
	to_min: float,
	to_max: float
) -> float:
	if from_max == from_min:
		return to_min
	var normalized: float = (value - from_min) / (from_max - from_min)
	return to_min + normalized * (to_max - to_min)


## Remaps a value from one range to another, clamping to the target range.
## @param value: The value to remap.
## @param from_min: The minimum of the source range.
## @param from_max: The maximum of the source range.
## @param to_min: The minimum of the target range.
## @param to_max: The maximum of the target range.
## @return: The remapped and clamped value.
static func remap_clamped(
	value: float,
	from_min: float,
	from_max: float,
	to_min: float,
	to_max: float
) -> float:
	var remapped: float = remap(value, from_min, from_max, to_min, to_max)
	var actual_min: float = minf(to_min, to_max)
	var actual_max: float = maxf(to_min, to_max)
	return clampf(remapped, actual_min, actual_max)


## Calculates inverse lerp (where a value falls within a range as 0-1).
## @param from: The start of the range.
## @param to: The end of the range.
## @param value: The value to check.
## @return: The normalized position (0.0 at from, 1.0 at to).
static func inverse_lerp(from: float, to: float, value: float) -> float:
	if to == from:
		return 0.0
	return (value - from) / (to - from)


## Performs smooth interpolation using smoothstep curve.
## @param from: The start value.
## @param to: The end value.
## @param weight: The interpolation weight (0.0 to 1.0).
## @return: The smoothly interpolated value.
static func smooth_lerp(from: float, to: float, weight: float) -> float:
	var t: float = clampf(weight, 0.0, 1.0)
	t = t * t * (3.0 - 2.0 * t)
	return lerpf(from, to, t)
