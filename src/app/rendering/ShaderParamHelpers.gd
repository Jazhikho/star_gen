## Shared helpers for shader parameter derivation.
## Used by StarShaderParams, TerrestrialShaderParams, and GasGiantShaderParams.
class_name ShaderParamHelpers
extends RefCounted


## Calculates visual rotation speed for animation.
## Normalized so rotation is visible but not too fast.
## @param rotation_period_s: Rotation period in seconds.
## @return: Normalized speed for shader animation.
static func calculate_visual_rotation_speed(rotation_period_s: float) -> float:
	var period_days: float = rotation_period_s / 86400.0

	# Fast rotators (< 1 day): slower visual speed
	if period_days < 1.0:
		return 0.15
	# Normal rotators (1-30 days): moderate speed
	elif period_days < 30.0:
		return 0.05 + (30.0 - period_days) / 30.0 * 0.1
	# Slow rotators (> 30 days): faster visual speed to show rotation
	else:
		return 0.03
