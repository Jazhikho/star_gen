## Derives gas giant shader uniforms from body components.
## Used by MaterialFactory when creating gas giant materials.
class_name GasGiantShaderParams
extends RefCounted

const _color_utils: GDScript = preload("res://src/app/rendering/ColorUtils.gd")


## Returns shader parameters for gas giant rendering.
## @param body: The gas giant CelestialBody.
## @return: Dictionary of shader uniform values.
static func get_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var seed_value: float = 0.0
	if body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0
	params["u_seed"] = seed_value

	for key in _get_shape_params(body):
		params[key] = _get_shape_params(body)[key]
	for key in _get_band_params(body):
		params[key] = _get_band_params(body)[key]
	for key in _get_storm_params(body):
		params[key] = _get_storm_params(body)[key]
	for key in _get_color_params(body):
		params[key] = _get_color_params(body)[key]
	for key in _get_atmosphere_params(body):
		params[key] = _get_atmosphere_params(body)[key]
	for key in _get_lighting_params(body):
		params[key] = _get_lighting_params(body)[key]
	for key in _get_animation_params(body):
		params[key] = _get_animation_params(body)[key]

	if body.has_ring_system():
		var ring_params: Dictionary = RingShaderParams.get_ring_shader_params(body.ring_system, body.physical.radius_m)
		for key in ring_params:
			params[key] = ring_params[key]
	else:
		params["u_ringType"] = 0

	return params


## Returns shader params with legacy u_g* keys for backward compatibility.
## @param body: The gas giant CelestialBody.
## @return: Dictionary with u_gBandCount, u_gColBandLight, etc.
static func get_gas_giant_shader_params(body: CelestialBody) -> Dictionary:
	var p: Dictionary = get_params(body)
	var legacy: Dictionary = {}
	legacy["u_seed"] = p.get("u_seed", 0.0)
	legacy["u_gOblateness"] = p.get("u_oblateness", 0.065)
	legacy["u_axialTilt"] = p.get("u_axialTilt", 0.4)
	legacy["u_rotSpeed"] = p.get("u_rotSpeed", 0.12)
	legacy["u_gFlowSpeed"] = p.get("u_flowSpeed", 0.3)
	legacy["u_gColBandLight"] = _color_to_vec3(p.get("u_colBandLight", Color.WHITE))
	legacy["u_gColBandDark"] = _color_to_vec3(p.get("u_colBandDark", Color.GRAY))
	legacy["u_gColStorm"] = _color_to_vec3(p.get("u_colStorm", Color.ORANGE))
	legacy["u_gColPolar"] = _color_to_vec3(p.get("u_colPolar", Color.CORNFLOWER_BLUE))
	legacy["u_atmoColor"] = _color_to_vec3(p.get("u_atmoColor", Color.WHITE))
	legacy["u_gBandCount"] = p.get("u_bandCount", 14.0)
	legacy["u_gBandContrast"] = p.get("u_bandContrast", 0.5)
	legacy["u_gBandTurb"] = p.get("u_bandTurbulence", 0.6)
	legacy["u_gFlowDetail"] = p.get("u_flowDetail", 5)
	legacy["u_gStormIntensity"] = p.get("u_stormIntensity", 0.5)
	legacy["u_gStormScale"] = p.get("u_stormScale", 2.0)
	legacy["u_gVortex"] = p.get("u_vortexStrength", 0.7)
	legacy["u_atmoDensity"] = p.get("u_atmoDensity", 1.2)
	legacy["u_atmoFalloff"] = p.get("u_atmoFalloff", 2.0)
	legacy["u_scatterStrength"] = p.get("u_scatterStrength", 0.7)
	legacy["u_limbDark"] = p.get("u_limbDark", 0.9)
	legacy["u_terminatorSharp"] = p.get("u_terminatorSharp", 0.15)
	legacy["u_lightX"] = p.get("u_lightX", 0.7)
	legacy["u_lightY"] = p.get("u_lightY", 0.3)
	legacy["u_ambient"] = p.get("u_ambient", 0.04)
	if body.has_ring_system():
		var ring_params: Dictionary = RingShaderParams.get_ring_shader_params(body.ring_system, body.physical.radius_m)
		for key in ring_params:
			legacy[key] = ring_params[key]
	else:
		legacy["u_ringType"] = 0
	return legacy


static func _color_to_vec3(c: Color) -> Vector3:
	return Vector3(c.r, c.g, c.b)


## Returns whether the body is a gas giant suitable for gas giant rendering.
## @param body: The celestial body.
## @return: True if gas giant (mass >= 10 Earth, no terrain, surface type or mass indicates gas giant).
static func is_gas_giant(body: CelestialBody) -> bool:
	if body.surface and body.surface.has_terrain():
		return false
	var mass_earth: float = body.physical.mass_kg / Units.EARTH_MASS_KG
	if mass_earth >= 15.0:
		return true
	if mass_earth < 10.0:
		return false
	if body.surface:
		var st: String = body.surface.surface_type.to_lower()
		return st in ["gaseous", "gas_giant", "ice_giant"]
	return false


static func _get_shape_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	params["u_oblateness"] = body.physical.oblateness
	params["u_axialTilt"] = deg_to_rad(body.physical.axial_tilt_deg)
	return params


static func _get_band_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0
	var rotation_factor: float = clampf(86400.0 / maxf(rotation_period_s, 1.0), 0.5, 3.0)
	var temperature_k: float = _get_temperature_k(body)
	params["u_bandCount"] = 8.0 + rotation_factor * 6.0
	params["u_bandContrast"] = 0.4 + (temperature_k / 500.0) * 0.2
	params["u_bandTurbulence"] = 0.4 + rotation_factor * 0.3
	params["u_flowDetail"] = 4 + int(rotation_factor * 2.0)
	return params


static func _get_storm_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	var heat_factor: float = clampf(body.physical.internal_heat_watts / 1e17, 0.0, 1.0)
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0
	var rotation_factor: float = clampf(86400.0 / rotation_period_s, 0.5, 2.0)
	params["u_stormIntensity"] = 0.3 + heat_factor * 0.3 + rotation_factor * 0.2
	params["u_stormScale"] = 1.5 + heat_factor * 1.5
	params["u_vortexStrength"] = 0.5 + rotation_factor * 0.5
	return params


static func _get_color_params(body: CelestialBody) -> Dictionary:
	var temperature_k: float = _get_temperature_k(body)
	var colors: Dictionary = _get_gas_giant_colors(temperature_k)
	var params: Dictionary = {}
	params["u_colBandLight"] = colors["band_light"]
	params["u_colBandDark"] = colors["band_dark"]
	params["u_colStorm"] = colors["storm"]
	params["u_colPolar"] = colors["polar"]
	return params


static func _get_temperature_k(body: CelestialBody) -> float:
	if body.has_surface():
		return body.surface.temperature_k
	if body.has_atmosphere():
		return 150.0 + (body.atmosphere.greenhouse_factor - 1.0) * 200.0
	return 150.0


static func _get_gas_giant_colors(temperature_k: float) -> Dictionary:
	var colors: Dictionary = {}
	if temperature_k > 500.0:
		colors["band_light"] = Color(0.95, 0.7, 0.4)
		colors["band_dark"] = Color(0.6, 0.3, 0.15)
		colors["storm"] = Color(1.0, 0.5, 0.2)
		colors["polar"] = Color(0.7, 0.4, 0.3)
	elif temperature_k > 200.0:
		colors["band_light"] = Color(0.91, 0.835, 0.627)
		colors["band_dark"] = Color(0.545, 0.42, 0.227)
		colors["storm"] = Color(0.8, 0.4, 0.267)
		colors["polar"] = Color(0.4, 0.533, 0.667)
	elif temperature_k > 100.0:
		colors["band_light"] = Color(0.94, 0.87, 0.63)
		colors["band_dark"] = Color(0.77, 0.65, 0.37)
		colors["storm"] = Color(0.87, 0.73, 0.47)
		colors["polar"] = Color(0.53, 0.6, 0.67)
	else:
		colors["band_light"] = Color(0.5, 0.7, 0.85)
		colors["band_dark"] = Color(0.3, 0.5, 0.7)
		colors["storm"] = Color(0.4, 0.65, 0.8)
		colors["polar"] = Color(0.35, 0.55, 0.7)
	return colors


static func _get_atmosphere_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	params["u_atmoDensity"] = 1.2
	params["u_atmoFalloff"] = 2.0
	params["u_scatterStrength"] = 0.7
	var temp_k: float = _get_temperature_k(body)
	var colors: Dictionary = _get_gas_giant_colors(temp_k)
	var band_light: Color = colors["band_light"] as Color
	if body.has_atmosphere():
		var sky_color: Color = _color_utils.atmosphere_to_sky_color(body.atmosphere.composition)
		params["u_atmoColor"] = band_light.lerp(sky_color, 0.4)
	else:
		params["u_atmoColor"] = Color(band_light.r, band_light.g, band_light.b)
	return params


static func _get_lighting_params(_body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	params["u_limbDark"] = 0.9
	params["u_terminatorSharp"] = 0.15
	params["u_lightX"] = 0.7
	params["u_lightY"] = 0.3
	params["u_ambient"] = 0.04
	return params


static func _get_animation_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0
	params["u_rotSpeed"] = ShaderParamHelpers.calculate_visual_rotation_speed(rotation_period_s) * 2.0
	params["u_flowSpeed"] = 0.2 + (86400.0 / maxf(rotation_period_s, 1.0)) * 0.2
	return params
