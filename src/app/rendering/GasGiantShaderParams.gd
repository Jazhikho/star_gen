## Derives gas giant shader uniforms from body components.
## Maps body properties to shader uniforms by first classifying the body
## into one of six archetypes (hot Jupiter, Jupiter-class, Saturn-class,
## Neptune-class, Uranus-class, Super-Jupiter), then blending archetype
## preset values with continuous physical parameters.
##
## Archetype ranges match the concept prototype presets so the full
## shader parameter space is actually used.
class_name GasGiantShaderParams
extends RefCounted

const _color_utils: GDScript = preload("res://src/app/rendering/ColorUtils.gd")


## Gas giant archetypes, matching the six concept prototype presets.
enum Archetype {
	HOT_JUPITER, ## >700 K, fast rotation — orange/red, high turbulence
	JUPITER_CLASS, ## 300–700 K, fast rotation — tan/amber, moderate storms
	SATURN_CLASS, ## 300–700 K, moderate rotation, low storm — pale gold
	NEPTUNE_CLASS, ## 80–300 K, moderate rotation — blue, high turbulence
	URANUS_CLASS, ## <150 K, slow rotation — pale cyan, minimal storms
	SUPER_JUPITER, ## >200 M⊕, any temp — deep red-brown, extreme storms
}


## Preset parameter tables for each archetype.
## Values tuned to match the concept prototype presets.
## Fields: band_count, band_contrast, band_turb, flow_speed, flow_detail,
##         storm_intensity, storm_scale, vortex, atmo_density, atmo_falloff,
##         scatter, limb_dark, terminator_sharp, ambient,
##         band_light (Color), band_dark (Color), storm (Color), polar (Color),
##         atmo_color (Color)
const _ARCHETYPE_PRESETS: Array[Dictionary] = [
	## HOT_JUPITER — prototype "hotjupiter" preset
	{
		"band_count": 10.0, "band_contrast": 0.70, "band_turb": 1.20,
		"flow_speed": 0.60, "flow_detail": 6,
		"storm_intensity": 0.80, "storm_scale": 2.50, "vortex": 1.20,
		"atmo_density": 1.50, "atmo_falloff": 1.50, "scatter": 0.50,
		"limb_dark": 0.70, "terminator_sharp": 0.08, "ambient": 0.08,
		"band_light": Color(1.00, 0.60, 0.27),
		"band_dark": Color(0.53, 0.13, 0.00),
		"storm": Color(1.00, 0.80, 0.20),
		"polar": Color(1.00, 0.40, 0.13),
		"atmo_color": Color(1.00, 0.33, 0.00),
	},
	## JUPITER_CLASS — prototype "jupiter" preset
	{
		"band_count": 14.0, "band_contrast": 0.50, "band_turb": 0.60,
		"flow_speed": 0.30, "flow_detail": 5,
		"storm_intensity": 0.60, "storm_scale": 2.00, "vortex": 0.80,
		"atmo_density": 1.20, "atmo_falloff": 2.00, "scatter": 0.70,
		"limb_dark": 0.90, "terminator_sharp": 0.15, "ambient": 0.04,
		"band_light": Color(0.91, 0.835, 0.627),
		"band_dark": Color(0.545, 0.420, 0.227),
		"storm": Color(0.80, 0.40, 0.267),
		"polar": Color(0.40, 0.533, 0.667),
		"atmo_color": Color(0.667, 0.733, 0.60),
	},
	## SATURN_CLASS — prototype "saturn" preset
	{
		"band_count": 20.0, "band_contrast": 0.30, "band_turb": 0.40,
		"flow_speed": 0.25, "flow_detail": 5,
		"storm_intensity": 0.20, "storm_scale": 1.50, "vortex": 0.40,
		"atmo_density": 0.90, "atmo_falloff": 2.20, "scatter": 0.60,
		"limb_dark": 0.85, "terminator_sharp": 0.12, "ambient": 0.04,
		"band_light": Color(0.941, 0.867, 0.627),
		"band_dark": Color(0.769, 0.651, 0.376),
		"storm": Color(0.867, 0.733, 0.467),
		"polar": Color(0.533, 0.60, 0.667),
		"atmo_color": Color(0.80, 0.733, 0.533),
	},
	## NEPTUNE_CLASS — prototype "neptune" preset
	{
		"band_count": 8.0, "band_contrast": 0.40, "band_turb": 0.80,
		"flow_speed": 0.35, "flow_detail": 6,
		"storm_intensity": 0.50, "storm_scale": 2.50, "vortex": 1.00,
		"atmo_density": 1.00, "atmo_falloff": 2.50, "scatter": 0.90,
		"limb_dark": 0.95, "terminator_sharp": 0.15, "ambient": 0.03,
		"band_light": Color(0.267, 0.467, 0.80),
		"band_dark": Color(0.133, 0.20, 0.533),
		"storm": Color(0.333, 0.60, 0.867),
		"polar": Color(0.20, 0.333, 0.667),
		"atmo_color": Color(0.20, 0.40, 0.733),
	},
	## URANUS_CLASS — prototype "uranus" preset
	{
		"band_count": 6.0, "band_contrast": 0.15, "band_turb": 0.30,
		"flow_speed": 0.15, "flow_detail": 4,
		"storm_intensity": 0.15, "storm_scale": 1.50, "vortex": 0.30,
		"atmo_density": 0.80, "atmo_falloff": 2.50, "scatter": 0.85,
		"limb_dark": 0.90, "terminator_sharp": 0.12, "ambient": 0.04,
		"band_light": Color(0.533, 0.80, 0.80),
		"band_dark": Color(0.40, 0.667, 0.667),
		"storm": Color(0.467, 0.733, 0.733),
		"polar": Color(0.333, 0.60, 0.667),
		"atmo_color": Color(0.467, 0.733, 0.80),
	},
	## SUPER_JUPITER — prototype "superjupiter" preset
	{
		"band_count": 18.0, "band_contrast": 0.65, "band_turb": 0.90,
		"flow_speed": 0.45, "flow_detail": 6,
		"storm_intensity": 0.70, "storm_scale": 3.00, "vortex": 1.50,
		"atmo_density": 1.40, "atmo_falloff": 1.80, "scatter": 0.60,
		"limb_dark": 0.80, "terminator_sharp": 0.10, "ambient": 0.05,
		"band_light": Color(0.867, 0.733, 0.533),
		"band_dark": Color(0.40, 0.267, 0.133),
		"storm": Color(0.933, 0.533, 0.267),
		"polar": Color(0.333, 0.267, 0.40),
		"atmo_color": Color(0.533, 0.467, 0.333),
	},
]


## Returns shader parameters for gas giant rendering.
## @param body: The gas giant CelestialBody.
## @return: Dictionary of shader uniform values.
static func get_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var seed_value: float = 0.0
	if body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0
	params["u_seed"] = seed_value

	# Classify into archetype, then derive all params from the archetype
	# base blended with continuous physical properties.
	var archetype: Archetype = _classify_archetype(body)
	var preset: Dictionary = _ARCHETYPE_PRESETS[archetype]

	_merge(params, _get_shape_params(body))
	_merge(params, _get_band_params(body, archetype, preset))
	_merge(params, _get_storm_params(body, archetype, preset))
	_merge(params, _get_color_params(body, archetype, preset))
	_merge(params, _get_atmosphere_params(body, archetype, preset))
	_merge(params, _get_lighting_params(body, archetype, preset))
	_merge(params, _get_animation_params(body, preset))

	if body.has_ring_system():
		var ring_params: Dictionary = RingShaderParams.get_ring_shader_params(body.ring_system, body.physical.radius_m)
		for key in ring_params:
			params[key] = ring_params[key]
	else:
		params["u_ringType"] = 0

	return params


## Copies all entries from src into dst.
static func _merge(dst: Dictionary, src: Dictionary) -> void:
	for key in src:
		dst[key] = src[key]


## Classifies a gas giant body into one of six archetypes.
## @param body: The gas giant CelestialBody.
## @return: Archetype enum value.
static func _classify_archetype(body: CelestialBody) -> Archetype:
	var mass_earth: float = body.physical.mass_kg / Units.EARTH_MASS_KG
	var temp_k: float = _get_temperature_k(body)
	var rotation_h: float = absf(body.physical.rotation_period_s) / 3600.0
	if rotation_h < 1.0:
		rotation_h = 10.0

	# Super-Jupiter: very massive regardless of temperature.
	if mass_earth > 500.0:
		return Archetype.SUPER_JUPITER

	# Hot Jupiter: high temperature.
	if temp_k > 700.0:
		return Archetype.HOT_JUPITER

	# Ice giants: cold bodies.
	if temp_k < 150.0:
		# Uranus: slow rotation and very cold; Neptune: faster rotation.
		if rotation_h > 16.0:
			return Archetype.URANUS_CLASS
		return Archetype.NEPTUNE_CLASS

	# Warm gas giants: distinguish Jupiter (fast, stormy) from Saturn (moderate, banded).
	# Saturn has more bands but less storm activity due to slower differential rotation.
	var ch4: float = _get_composition_fraction(body, "CH4")
	if rotation_h < 12.0 and mass_earth < 200.0 and ch4 < 0.01:
		# Fast-rotating, hydrogen-rich, not super-massive — Jupiter-like.
		# Use band contrast to distinguish: Saturn has lower contrast.
		var oblateness: float = body.physical.oblateness
		if oblateness > 0.08:
			return Archetype.SATURN_CLASS
		return Archetype.JUPITER_CLASS

	# Default warm giant to Jupiter class.
	return Archetype.JUPITER_CLASS


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


## Derives band parameters by blending archetype preset with physical properties.
## Rotation rate and composition shift the preset values within their natural range.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved for future use).
## @param preset: Archetype preset dictionary.
## @return: Band shader parameters.
static func _get_band_params(body: CelestialBody, _archetype: Archetype, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0
	# Rotation in hours, clamped to plausible gas giant range.
	var rotation_h: float = clampf(rotation_period_s / 3600.0, 8.0, 30.0)

	# How fast relative to a 10-hour Jupiter-like day: 0=slow, 1=fast.
	var rot_t: float = clampf(1.0 - (rotation_h - 8.0) / 22.0, 0.0, 1.0)

	# Seed-based variation: ±15% perturbation per planet within its archetype.
	var seed_t: float = 0.0
	if body.provenance:
		seed_t = (float(body.provenance.generation_seed % 1000) / 1000.0) - 0.5

	# Band count: preset base ± rotation and seed nudge.
	var band_nudge: float = rot_t * 3.0 + seed_t * 2.0
	params["u_bandCount"] = clampf(preset["band_count"] + band_nudge, 4.0, 28.0)

	# Contrast and turbulence: nudged by internal heat and seed.
	var heat_t: float = clampf(body.physical.internal_heat_watts / 2.0e17, 0.0, 1.0)
	params["u_bandContrast"] = clampf(
		float(preset["band_contrast"]) + heat_t * 0.12 + seed_t * 0.08,
		0.10, 0.95
	)
	params["u_bandTurbulence"] = clampf(
		float(preset["band_turb"]) + heat_t * 0.15 + rot_t * 0.10 + seed_t * 0.10,
		0.15, 1.40
	)
	var flow_extra: int = 0
	if rot_t > 0.5:
		flow_extra = 1
	params["u_flowDetail"] = clampi(int(preset["flow_detail"]) + flow_extra, 3, 8)
	return params


## Derives storm parameters from archetype preset and physical modifiers.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved for future use).
## @param preset: Archetype preset dictionary.
## @return: Storm shader parameters.
static func _get_storm_params(body: CelestialBody, _archetype: Archetype, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}
	var heat_t: float = clampf(body.physical.internal_heat_watts / 2.0e17, 0.0, 1.0)
	var seed_t: float = 0.0
	if body.provenance:
		seed_t = (float(body.provenance.generation_seed % 997) / 997.0) - 0.5

	params["u_stormIntensity"] = clampf(
		float(preset["storm_intensity"]) + heat_t * 0.15 + seed_t * 0.10,
		0.05, 0.95
	)
	params["u_stormScale"] = clampf(
		float(preset["storm_scale"]) + heat_t * 0.50 + seed_t * 0.30,
		0.80, 4.50
	)
	params["u_vortexStrength"] = clampf(
		float(preset["vortex"]) + seed_t * 0.15,
		0.10, 1.60
	)
	return params


## Derives color parameters by applying a seed-based hue shift to archetype colors.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved).
## @param preset: Archetype preset dictionary.
## @return: Color shader parameters.
static func _get_color_params(body: CelestialBody, _archetype: Archetype, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}

	# Seed-based hue shift: gives each planet a unique tint within its class.
	# ±12° hue rotation stays within the archetype palette family.
	var seed_hue_shift: float = 0.0
	if body.provenance:
		seed_hue_shift = float(body.provenance.generation_seed % 1000) / 1000.0
	var hue_shift: float = (seed_hue_shift - 0.5) * 0.067 # ±0.033 = ±12°

	params["u_colBandLight"] = _clamp_color(_shift_hue(preset["band_light"] as Color, hue_shift))
	params["u_colBandDark"] = _clamp_color(_shift_hue(preset["band_dark"] as Color, hue_shift * 0.8))
	params["u_colStorm"] = _clamp_color(_shift_hue(preset["storm"] as Color, hue_shift * 0.5))
	params["u_colPolar"] = _clamp_color(_shift_hue(preset["polar"] as Color, hue_shift * 0.6))
	return params


static func _get_temperature_k(body: CelestialBody) -> float:
	if body.has_surface():
		return body.surface.temperature_k
	if body.has_atmosphere():
		return 150.0 + (body.atmosphere.greenhouse_factor - 1.0) * 200.0
	return 150.0


## Shifts the hue of a color by delta (in [0,1] hue units).
## Preserves saturation and value.
## @param c: Input color.
## @param delta: Hue shift in [0,1] units (wraps around).
## @return: Hue-shifted color.
static func _shift_hue(c: Color, delta: float) -> Color:
	var h: float = c.h + delta
	# Wrap to [0, 1]
	h = h - floorf(h)
	return Color.from_hsv(h, c.s, c.v, c.a)


## Clamps all color channels to [0, 1].
## @param c: Input color.
## @return: Clamped color.
static func _clamp_color(c: Color) -> Color:
	return Color(clampf(c.r, 0.0, 1.0), clampf(c.g, 0.0, 1.0), clampf(c.b, 0.0, 1.0), c.a)


## Returns a composition gas fraction from the atmosphere, or 0.0 if absent.
## @param body: The celestial body.
## @param gas: Gas name key (e.g. "CH4", "NH3", "H2").
## @return: Fraction in [0, 1].
static func _get_composition_fraction(body: CelestialBody, gas: String) -> float:
	if not body.has_atmosphere():
		return 0.0
	return body.atmosphere.composition.get(gas, 0.0) as float


## Derives atmosphere rim parameters from archetype preset and composition.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved).
## @param preset: Archetype preset dictionary.
## @return: Atmosphere shader parameters.
static func _get_atmosphere_params(body: CelestialBody, _archetype: Archetype, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}

	# Start from preset values; nudge density slightly by composition.
	var seed_t: float = 0.0
	if body.provenance:
		seed_t = (float(body.provenance.generation_seed % 991) / 991.0) - 0.5

	params["u_atmoDensity"] = clampf(float(preset["atmo_density"]) + seed_t * 0.10, 0.40, 1.80)
	params["u_atmoFalloff"] = clampf(float(preset["atmo_falloff"]) + seed_t * 0.15, 1.20, 4.50)
	params["u_scatterStrength"] = clampf(float(preset["scatter"]) + seed_t * 0.08, 0.30, 1.40)

	# Atmosphere rim color: blend preset atmo_color with sky color from composition.
	var atmo_color: Color = preset["atmo_color"] as Color
	if body.has_atmosphere():
		var sky_color: Color = _color_utils.atmosphere_to_sky_color(body.atmosphere.composition)
		# 30% blend toward composition-derived sky color for subtle variation.
		atmo_color = atmo_color.lerp(sky_color, 0.30)
	params["u_atmoColor"] = _clamp_color(atmo_color)

	return params


## Derives lighting parameters from archetype preset.
## @param _body: The gas giant body (reserved for future use).
## @param _archetype: Classified archetype (reserved).
## @param preset: Archetype preset dictionary.
## @return: Lighting shader parameters.
static func _get_lighting_params(_body: CelestialBody, _archetype: Archetype, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}
	# Use preset values directly; these are already tuned per archetype.
	params["u_limbDark"] = float(preset["limb_dark"])
	params["u_terminatorSharp"] = float(preset["terminator_sharp"])
	params["u_lightX"] = 0.7
	params["u_lightY"] = 0.3
	params["u_ambient"] = float(preset["ambient"])
	return params


## Derives animation parameters from archetype preset and rotation period.
## @param body: The gas giant body.
## @param preset: Archetype preset dictionary.
## @return: Animation shader parameters.
static func _get_animation_params(body: CelestialBody, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0

	# Visual rotation: use ShaderParamHelpers for consistency, then blend
	# toward the preset value so archetype character is preserved.
	var physics_rot: float = ShaderParamHelpers.calculate_visual_rotation_speed(rotation_period_s) * 2.0
	var preset_rot: float = float(preset["band_count"]) * 0.006 # proxy: more bands = faster visual
	params["u_rotSpeed"] = clampf(lerpf(physics_rot, preset_rot, 0.4), 0.04, 0.22)

	# Flow speed from preset; nudged by rotation.
	var rot_h: float = clampf(rotation_period_s / 3600.0, 8.0, 30.0)
	var rot_t: float = clampf(1.0 - (rot_h - 8.0) / 22.0, 0.0, 1.0)
	params["u_flowSpeed"] = clampf(float(preset["flow_speed"]) + rot_t * 0.08, 0.10, 0.70)

	return params
