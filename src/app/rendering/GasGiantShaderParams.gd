## Derives gas giant shader uniforms from body components.
## Used by MaterialFactory when creating gas giant materials.
class_name GasGiantShaderParams
extends RefCounted


## Returns shader parameters for gas giant rendering.
## @param body: The gas giant CelestialBody.
## @return: Dictionary of shader uniform values.
static func get_gas_giant_shader_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var seed_value: float = 0.0
	if body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0
	params["u_seed"] = seed_value
	params["u_gOblateness"] = body.physical.oblateness
	params["u_axialTilt"] = deg_to_rad(body.physical.axial_tilt_deg)

	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0
	params["u_rotSpeed"] = ShaderParamHelpers.calculate_visual_rotation_speed(rotation_period_s) * 2.0
	params["u_gFlowSpeed"] = 0.2 + (86400.0 / maxf(rotation_period_s, 1.0)) * 0.2

	var temperature_k: float = 150.0
	if body.has_surface():
		temperature_k = body.surface.temperature_k
	var color_params: Dictionary = _get_gas_giant_colors(temperature_k)
	params["u_gColBandLight"] = color_params["band_light"]
	params["u_gColBandDark"] = color_params["band_dark"]
	params["u_gColStorm"] = color_params["storm"]
	params["u_gColPolar"] = color_params["polar"]
	params["u_atmoColor"] = color_params["atmosphere"]

	var rotation_factor: float = clampf(86400.0 / maxf(rotation_period_s, 1.0), 0.5, 3.0)
	params["u_gBandCount"] = 8.0 + rotation_factor * 6.0
	params["u_gBandContrast"] = 0.4 + (temperature_k / 500.0) * 0.2
	params["u_gBandTurb"] = 0.4 + rotation_factor * 0.3
	params["u_gFlowDetail"] = 4 + int(rotation_factor * 2.0)

	var storm_params: Dictionary = _calculate_gas_giant_storm_params(body)
	params["u_gStormIntensity"] = storm_params["intensity"]
	params["u_gStormScale"] = storm_params["scale"]
	params["u_gVortex"] = storm_params["vortex"]

	params["u_atmoDensity"] = 1.2
	params["u_atmoFalloff"] = 2.0
	params["u_scatterStrength"] = 0.7
	params["u_limbDark"] = 0.9
	params["u_terminatorSharp"] = 0.15
	params["u_lightX"] = 0.7
	params["u_lightY"] = 0.3
	params["u_ambient"] = 0.04

	if body.has_ring_system():
		var ring_params: Dictionary = RingShaderParams.get_ring_shader_params(body.ring_system, body.physical.radius_m)
		for key in ring_params:
			params[key] = ring_params[key]
	else:
		params["u_ringType"] = 0

	return params


static func _get_gas_giant_colors(temperature_k: float) -> Dictionary:
	var colors: Dictionary = {}
	if temperature_k > 500.0:
		colors["band_light"] = Vector3(0.95, 0.7, 0.4)
		colors["band_dark"] = Vector3(0.6, 0.3, 0.15)
		colors["storm"] = Vector3(1.0, 0.5, 0.2)
		colors["polar"] = Vector3(0.7, 0.4, 0.3)
		colors["atmosphere"] = Vector3(0.9, 0.5, 0.3)
	elif temperature_k > 200.0:
		colors["band_light"] = Vector3(0.91, 0.835, 0.627)
		colors["band_dark"] = Vector3(0.545, 0.42, 0.227)
		colors["storm"] = Vector3(0.8, 0.4, 0.267)
		colors["polar"] = Vector3(0.4, 0.533, 0.667)
		colors["atmosphere"] = Vector3(0.667, 0.6, 0.5)
	elif temperature_k > 100.0:
		colors["band_light"] = Vector3(0.94, 0.87, 0.63)
		colors["band_dark"] = Vector3(0.77, 0.65, 0.37)
		colors["storm"] = Vector3(0.87, 0.73, 0.47)
		colors["polar"] = Vector3(0.53, 0.6, 0.67)
		colors["atmosphere"] = Vector3(0.8, 0.73, 0.55)
	else:
		colors["band_light"] = Vector3(0.5, 0.7, 0.85)
		colors["band_dark"] = Vector3(0.3, 0.5, 0.7)
		colors["storm"] = Vector3(0.4, 0.65, 0.8)
		colors["polar"] = Vector3(0.35, 0.55, 0.7)
		colors["atmosphere"] = Vector3(0.4, 0.6, 0.8)
	return colors


static func _calculate_gas_giant_storm_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	var heat_factor: float = clampf(body.physical.internal_heat_watts / 1e17, 0.0, 1.0)
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0
	var rotation_factor: float = clampf(86400.0 / rotation_period_s, 0.5, 2.0)
	params["intensity"] = 0.3 + heat_factor * 0.3 + rotation_factor * 0.2
	params["scale"] = 1.5 + heat_factor * 1.5
	params["vortex"] = 0.5 + rotation_factor * 0.5
	return params
