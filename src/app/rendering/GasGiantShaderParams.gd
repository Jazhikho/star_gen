## Derives gas giant shader uniforms from body components.
##
## Two-layer approach:
##   1. Archetype classification → structural preset (band count, storm count,
##      haze density, detail level, etc.) — defines the *character*.
##   2. Seeded RNG → per-planet variation within archetype ranges (color palette
##      selection + hue shift, band sharpness/warp jitter, storm count jitter).
##
## This makes two Jupiters look related but distinct, and makes a mini-Neptune
## look *fundamentally* different from a Jupiter (hazy, smooth, few bands)
## rather than just a palette swap.
class_name GasGiantShaderParams
extends RefCounted

const _color_utils: GDScript = preload("res://src/app/rendering/ColorUtils.gd")


## Archetypes — extended with MINI_NEPTUNE for the small-gas-envelope case.
enum Archetype {
	HOT_JUPITER, ## >700 K — orange/red, high turbulence, many storms
	JUPITER_CLASS, ## 150–700 K, fast rot — tan/amber, crisp bands, bright storms
	SATURN_CLASS, ## 150–700 K, high oblateness — pale gold, soft bands
	NEPTUNE_CLASS, ## 80–150 K — deep blue, moderate haze, dark spots
	URANUS_CLASS, ## <150 K, slow rot — pale cyan, heavy haze, featureless
	SUPER_JUPITER, ## >500 M⊕ — deep red-brown, extreme storms
	MINI_NEPTUNE, ## 10–25 M⊕ — thick haze, almost no visible structure
}


## Structural preset per archetype.
##
## New fields vs. old:
##   band_sharpness, band_warp, chevron — band shape variety
##   jet_strength                       — per-band flow variation
##   storm_count_lo/hi, dark_spot_ratio — storm variety
##   detail_level, haze_density         — feature-obscuring haze
##   streak                             — fine streak intensity
##   palettes                           — array of {zone_eq, zone_mid, belt_eq, belt_mid, belt_polar, storm, polar, haze, atmo}
##
## band_light / band_dark / polar are kept as *fallback* for legacy API & tests.
const _ARCHETYPE_PRESETS: Array[Dictionary] = [
	## HOT_JUPITER
	{
		"band_count": 10.0, "band_contrast": 0.70, "band_turb": 1.20,
		"band_sharpness": 0.35, "band_warp": 0.35, "chevron": 0.15,
		"flow_speed": 0.60, "flow_detail": 6, "jet_strength": 1.3,
		"storm_intensity": 0.80, "storm_scale": 2.50, "vortex": 1.20,
		"storm_count_lo": 3, "storm_count_hi": 6, "dark_spot_ratio": 0.15,
		"detail_level": 0.75, "haze_density": 0.10, "streak": 0.35,
		"atmo_density": 1.50, "atmo_falloff": 1.50, "scatter": 0.50,
		"limb_dark": 0.70, "terminator_sharp": 0.08, "ambient": 0.08,
		"band_light": Color(1.00, 0.60, 0.27),
		"band_dark": Color(0.53, 0.13, 0.00),
		"storm": Color(1.00, 0.80, 0.20),
		"polar": Color(1.00, 0.40, 0.13),
		"atmo_color": Color(1.00, 0.33, 0.00),
		"palettes": [
			{
				"zone_eq": Color(0.85, 0.50, 0.20), "zone_mid": Color(0.75, 0.40, 0.18),
				"belt_eq": Color(0.55, 0.18, 0.08), "belt_mid": Color(0.40, 0.12, 0.05), "belt_polar": Color(0.30, 0.15, 0.12),
				"storm": Color(0.95, 0.70, 0.20), "polar": Color(0.45, 0.25, 0.18),
				"haze": Color(0.60, 0.35, 0.18), "atmo": Color(0.80, 0.40, 0.15),
			},
			{
				"zone_eq": Color(0.95, 0.80, 0.35), "zone_mid": Color(0.88, 0.70, 0.30),
				"belt_eq": Color(0.70, 0.45, 0.15), "belt_mid": Color(0.55, 0.35, 0.12), "belt_polar": Color(0.42, 0.30, 0.15),
				"storm": Color(1.00, 0.85, 0.30), "polar": Color(0.55, 0.40, 0.22),
				"haze": Color(0.72, 0.55, 0.25), "atmo": Color(0.85, 0.60, 0.20),
			},
			{
				"zone_eq": Color(0.45, 0.35, 0.32), "zone_mid": Color(0.40, 0.30, 0.28),
				"belt_eq": Color(0.30, 0.18, 0.15), "belt_mid": Color(0.22, 0.12, 0.10), "belt_polar": Color(0.20, 0.18, 0.20),
				"storm": Color(0.70, 0.25, 0.15), "polar": Color(0.28, 0.22, 0.25),
				"haze": Color(0.35, 0.28, 0.28), "atmo": Color(0.40, 0.22, 0.18),
			},
		],
	},
	## JUPITER_CLASS
	{
		"band_count": 14.0, "band_contrast": 0.50, "band_turb": 0.60,
		"band_sharpness": 0.55, "band_warp": 0.30, "chevron": 0.30,
		"flow_speed": 0.30, "flow_detail": 5, "jet_strength": 1.2,
		"storm_intensity": 0.60, "storm_scale": 2.00, "vortex": 0.80,
		"storm_count_lo": 2, "storm_count_hi": 6, "dark_spot_ratio": 0.20,
		"detail_level": 0.85, "haze_density": 0.05, "streak": 0.35,
		"atmo_density": 1.20, "atmo_falloff": 2.00, "scatter": 0.70,
		"limb_dark": 0.90, "terminator_sharp": 0.15, "ambient": 0.04,
		"band_light": Color(0.91, 0.835, 0.627),
		"band_dark": Color(0.545, 0.420, 0.227),
		"storm": Color(0.80, 0.40, 0.267),
		"polar": Color(0.40, 0.533, 0.667),
		"atmo_color": Color(0.667, 0.733, 0.60),
		"palettes": [
			{
				"zone_eq": Color(0.91, 0.84, 0.63), "zone_mid": Color(0.85, 0.78, 0.58),
				"belt_eq": Color(0.55, 0.42, 0.23), "belt_mid": Color(0.50, 0.38, 0.22), "belt_polar": Color(0.38, 0.38, 0.42),
				"storm": Color(0.80, 0.40, 0.27), "polar": Color(0.45, 0.50, 0.62),
				"haze": Color(0.60, 0.55, 0.50), "atmo": Color(0.67, 0.63, 0.50),
			},
			{
				"zone_eq": Color(0.78, 0.62, 0.48), "zone_mid": Color(0.72, 0.55, 0.42),
				"belt_eq": Color(0.58, 0.35, 0.22), "belt_mid": Color(0.48, 0.28, 0.18), "belt_polar": Color(0.40, 0.35, 0.32),
				"storm": Color(0.85, 0.50, 0.30), "polar": Color(0.45, 0.40, 0.38),
				"haze": Color(0.58, 0.48, 0.38), "atmo": Color(0.65, 0.50, 0.38),
			},
			{
				"zone_eq": Color(0.82, 0.80, 0.78), "zone_mid": Color(0.75, 0.74, 0.75),
				"belt_eq": Color(0.62, 0.58, 0.55), "belt_mid": Color(0.55, 0.52, 0.50), "belt_polar": Color(0.45, 0.45, 0.48),
				"storm": Color(0.75, 0.65, 0.55), "polar": Color(0.50, 0.52, 0.58),
				"haze": Color(0.65, 0.65, 0.68), "atmo": Color(0.68, 0.68, 0.72),
			},
		],
	},
	## SATURN_CLASS
	{
		"band_count": 20.0, "band_contrast": 0.30, "band_turb": 0.40,
		"band_sharpness": 0.25, "band_warp": 0.45, "chevron": 0.10,
		"flow_speed": 0.25, "flow_detail": 5, "jet_strength": 0.8,
		"storm_intensity": 0.20, "storm_scale": 1.50, "vortex": 0.40,
		"storm_count_lo": 0, "storm_count_hi": 3, "dark_spot_ratio": 0.20,
		"detail_level": 0.60, "haze_density": 0.18, "streak": 0.25,
		"atmo_density": 0.90, "atmo_falloff": 2.20, "scatter": 0.60,
		"limb_dark": 0.85, "terminator_sharp": 0.12, "ambient": 0.04,
		"band_light": Color(0.941, 0.867, 0.627),
		"band_dark": Color(0.769, 0.651, 0.376),
		"storm": Color(0.867, 0.733, 0.467),
		"polar": Color(0.533, 0.60, 0.667),
		"atmo_color": Color(0.80, 0.733, 0.533),
		"palettes": [
			{
				"zone_eq": Color(0.94, 0.87, 0.63), "zone_mid": Color(0.88, 0.82, 0.58),
				"belt_eq": Color(0.76, 0.65, 0.38), "belt_mid": Color(0.68, 0.58, 0.35), "belt_polar": Color(0.50, 0.50, 0.45),
				"storm": Color(0.90, 0.82, 0.50), "polar": Color(0.55, 0.55, 0.52),
				"haze": Color(0.72, 0.68, 0.50), "atmo": Color(0.80, 0.75, 0.52),
			},
			{
				"zone_eq": Color(0.90, 0.82, 0.55), "zone_mid": Color(0.85, 0.78, 0.52),
				"belt_eq": Color(0.72, 0.62, 0.38), "belt_mid": Color(0.65, 0.55, 0.35), "belt_polar": Color(0.52, 0.50, 0.42),
				"storm": Color(0.88, 0.78, 0.48), "polar": Color(0.55, 0.55, 0.52),
				"haze": Color(0.72, 0.68, 0.48), "atmo": Color(0.78, 0.72, 0.50),
			},
		],
	},
	## NEPTUNE_CLASS
	{
		"band_count": 8.0, "band_contrast": 0.40, "band_turb": 0.80,
		"band_sharpness": 0.30, "band_warp": 0.40, "chevron": 0.15,
		"flow_speed": 0.35, "flow_detail": 6, "jet_strength": 1.1,
		"storm_intensity": 0.50, "storm_scale": 2.50, "vortex": 1.00,
		"storm_count_lo": 1, "storm_count_hi": 4, "dark_spot_ratio": 0.65,
		"detail_level": 0.50, "haze_density": 0.30, "streak": 0.35,
		"atmo_density": 1.00, "atmo_falloff": 2.50, "scatter": 0.90,
		"limb_dark": 0.95, "terminator_sharp": 0.15, "ambient": 0.03,
		"band_light": Color(0.267, 0.467, 0.80),
		"band_dark": Color(0.133, 0.20, 0.533),
		"storm": Color(0.333, 0.60, 0.867),
		"polar": Color(0.20, 0.333, 0.667),
		"atmo_color": Color(0.20, 0.40, 0.733),
		"palettes": [
			{
				"zone_eq": Color(0.27, 0.47, 0.80), "zone_mid": Color(0.25, 0.42, 0.72),
				"belt_eq": Color(0.15, 0.30, 0.65), "belt_mid": Color(0.13, 0.25, 0.55), "belt_polar": Color(0.12, 0.22, 0.45),
				"storm": Color(0.35, 0.55, 0.85), "polar": Color(0.20, 0.32, 0.60),
				"haze": Color(0.28, 0.42, 0.70), "atmo": Color(0.22, 0.38, 0.72),
			},
			{
				"zone_eq": Color(0.20, 0.55, 0.65), "zone_mid": Color(0.18, 0.50, 0.60),
				"belt_eq": Color(0.12, 0.42, 0.55), "belt_mid": Color(0.10, 0.35, 0.48), "belt_polar": Color(0.10, 0.30, 0.42),
				"storm": Color(0.30, 0.60, 0.65), "polar": Color(0.15, 0.42, 0.55),
				"haze": Color(0.22, 0.50, 0.58), "atmo": Color(0.18, 0.48, 0.60),
			},
			{
				"zone_eq": Color(0.18, 0.35, 0.72), "zone_mid": Color(0.16, 0.32, 0.65),
				"belt_eq": Color(0.10, 0.22, 0.58), "belt_mid": Color(0.08, 0.18, 0.48), "belt_polar": Color(0.08, 0.15, 0.40),
				"storm": Color(0.25, 0.42, 0.78), "polar": Color(0.12, 0.25, 0.55),
				"haze": Color(0.18, 0.32, 0.62), "atmo": Color(0.15, 0.30, 0.65),
			},
		],
	},
	## URANUS_CLASS
	{
		"band_count": 6.0, "band_contrast": 0.15, "band_turb": 0.30,
		"band_sharpness": 0.10, "band_warp": 0.25, "chevron": 0.03,
		"flow_speed": 0.15, "flow_detail": 4, "jet_strength": 0.6,
		"storm_intensity": 0.15, "storm_scale": 1.50, "vortex": 0.30,
		"storm_count_lo": 0, "storm_count_hi": 2, "dark_spot_ratio": 0.50,
		"detail_level": 0.25, "haze_density": 0.55, "streak": 0.10,
		"atmo_density": 0.80, "atmo_falloff": 2.50, "scatter": 0.85,
		"limb_dark": 0.90, "terminator_sharp": 0.12, "ambient": 0.04,
		"band_light": Color(0.533, 0.80, 0.80),
		"band_dark": Color(0.40, 0.667, 0.667),
		"storm": Color(0.467, 0.733, 0.733),
		"polar": Color(0.333, 0.60, 0.667),
		"atmo_color": Color(0.467, 0.733, 0.80),
		"palettes": [
			{
				"zone_eq": Color(0.53, 0.78, 0.82), "zone_mid": Color(0.48, 0.72, 0.78),
				"belt_eq": Color(0.42, 0.65, 0.72), "belt_mid": Color(0.38, 0.58, 0.65), "belt_polar": Color(0.34, 0.52, 0.58),
				"storm": Color(0.48, 0.72, 0.75), "polar": Color(0.38, 0.58, 0.68),
				"haze": Color(0.48, 0.68, 0.75), "atmo": Color(0.45, 0.68, 0.78),
			},
			{
				"zone_eq": Color(0.52, 0.58, 0.55), "zone_mid": Color(0.48, 0.55, 0.52),
				"belt_eq": Color(0.40, 0.48, 0.46), "belt_mid": Color(0.38, 0.44, 0.42), "belt_polar": Color(0.35, 0.40, 0.40),
				"storm": Color(0.48, 0.55, 0.50), "polar": Color(0.40, 0.46, 0.50),
				"haze": Color(0.48, 0.54, 0.52), "atmo": Color(0.44, 0.52, 0.55),
			},
		],
	},
	## SUPER_JUPITER
	{
		"band_count": 18.0, "band_contrast": 0.65, "band_turb": 0.90,
		"band_sharpness": 0.50, "band_warp": 0.35, "chevron": 0.35,
		"flow_speed": 0.45, "flow_detail": 6, "jet_strength": 1.5,
		"storm_intensity": 0.70, "storm_scale": 3.00, "vortex": 1.50,
		"storm_count_lo": 3, "storm_count_hi": 7, "dark_spot_ratio": 0.25,
		"detail_level": 0.90, "haze_density": 0.05, "streak": 0.40,
		"atmo_density": 1.40, "atmo_falloff": 1.80, "scatter": 0.60,
		"limb_dark": 0.80, "terminator_sharp": 0.10, "ambient": 0.05,
		"band_light": Color(0.867, 0.733, 0.533),
		"band_dark": Color(0.40, 0.267, 0.133),
		"storm": Color(0.933, 0.533, 0.267),
		"polar": Color(0.333, 0.267, 0.40),
		"atmo_color": Color(0.533, 0.467, 0.333),
		"palettes": [
			{
				"zone_eq": Color(0.87, 0.73, 0.53), "zone_mid": Color(0.78, 0.62, 0.45),
				"belt_eq": Color(0.40, 0.27, 0.13), "belt_mid": Color(0.32, 0.20, 0.10), "belt_polar": Color(0.28, 0.22, 0.22),
				"storm": Color(0.93, 0.53, 0.27), "polar": Color(0.33, 0.27, 0.40),
				"haze": Color(0.50, 0.42, 0.32), "atmo": Color(0.53, 0.47, 0.33),
			},
			{
				"zone_eq": Color(0.80, 0.72, 0.55), "zone_mid": Color(0.72, 0.65, 0.50),
				"belt_eq": Color(0.48, 0.38, 0.25), "belt_mid": Color(0.42, 0.32, 0.22), "belt_polar": Color(0.35, 0.35, 0.38),
				"storm": Color(0.72, 0.48, 0.28), "polar": Color(0.42, 0.45, 0.52),
				"haze": Color(0.55, 0.52, 0.45), "atmo": Color(0.58, 0.55, 0.48),
			},
		],
	},
	## MINI_NEPTUNE
	{
		"band_count": 5.0, "band_contrast": 0.12, "band_turb": 0.25,
		"band_sharpness": 0.05, "band_warp": 0.20, "chevron": 0.02,
		"flow_speed": 0.12, "flow_detail": 3, "jet_strength": 0.5,
		"storm_intensity": 0.10, "storm_scale": 1.30, "vortex": 0.25,
		"storm_count_lo": 0, "storm_count_hi": 1, "dark_spot_ratio": 0.50,
		"detail_level": 0.15, "haze_density": 0.65, "streak": 0.05,
		"atmo_density": 1.10, "atmo_falloff": 2.20, "scatter": 0.80,
		"limb_dark": 0.85, "terminator_sharp": 0.12, "ambient": 0.04,
		"band_light": Color(0.55, 0.60, 0.68),
		"band_dark": Color(0.42, 0.48, 0.58),
		"storm": Color(0.60, 0.55, 0.50),
		"polar": Color(0.40, 0.45, 0.55),
		"atmo_color": Color(0.45, 0.52, 0.62),
		"palettes": [
			{
				"zone_eq": Color(0.55, 0.60, 0.68), "zone_mid": Color(0.50, 0.55, 0.65),
				"belt_eq": Color(0.42, 0.48, 0.58), "belt_mid": Color(0.38, 0.42, 0.52), "belt_polar": Color(0.35, 0.38, 0.48),
				"storm": Color(0.60, 0.55, 0.50), "polar": Color(0.40, 0.45, 0.55),
				"haze": Color(0.50, 0.55, 0.62), "atmo": Color(0.45, 0.52, 0.62),
			},
			{
				"zone_eq": Color(0.45, 0.62, 0.60), "zone_mid": Color(0.42, 0.58, 0.57),
				"belt_eq": Color(0.35, 0.50, 0.50), "belt_mid": Color(0.32, 0.45, 0.47), "belt_polar": Color(0.30, 0.40, 0.44),
				"storm": Color(0.50, 0.60, 0.55), "polar": Color(0.35, 0.48, 0.52),
				"haze": Color(0.42, 0.55, 0.55), "atmo": Color(0.40, 0.55, 0.58),
			},
			{
				"zone_eq": Color(0.50, 0.58, 0.72), "zone_mid": Color(0.48, 0.55, 0.68),
				"belt_eq": Color(0.40, 0.48, 0.62), "belt_mid": Color(0.38, 0.44, 0.58), "belt_polar": Color(0.35, 0.40, 0.52),
				"storm": Color(0.55, 0.52, 0.58), "polar": Color(0.38, 0.44, 0.58),
				"haze": Color(0.45, 0.52, 0.65), "atmo": Color(0.42, 0.50, 0.65),
			},
		],
	},
]


## Returns shader parameters for gas giant rendering.
## @param body: The gas giant CelestialBody.
## @return: Dictionary of shader uniform values.
static func get_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var seed_int: int = 0
	if body.provenance:
		seed_int = body.provenance.generation_seed
	var seed_value: float = float(seed_int % 1000) / 10.0
	params["u_seed"] = seed_value

	# Deterministic RNG for per-planet variation within the archetype.
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = seed_int

	var archetype: Archetype = _classify_archetype(body)
	var preset: Dictionary = _ARCHETYPE_PRESETS[archetype]

	_merge(params, _get_shape_params(body))
	_merge(params, _get_band_params(body, archetype, preset, rng))
	_merge(params, _get_storm_params(body, archetype, preset, rng))
	_merge(params, _get_detail_params(body, archetype, preset, rng))
	_merge(params, _get_color_params(body, archetype, preset, rng))
	_merge(params, _get_atmosphere_params(body, archetype, preset, rng))
	_merge(params, _get_lighting_params(body, archetype, preset))
	_merge(params, _get_animation_params(body, preset))

	if body.has_ring_system():
		var ring_params: Dictionary = RingShaderParams.get_ring_shader_params(body.ring_system, body.physical.radius_m)
		for key: String in ring_params:
			params[key] = ring_params[key]
	else:
		params["u_ringType"] = 0

	return params


## Copies all entries from src into dst.
static func _merge(dst: Dictionary, src: Dictionary) -> void:
	for key: String in src:
		dst[key] = src[key]


## Classifies a gas giant body into an archetype.
## @param body: The gas giant CelestialBody.
## @return: Archetype enum value.
static func _classify_archetype(body: CelestialBody) -> Archetype:
	var mass_earth: float = body.physical.mass_kg / Units.EARTH_MASS_KG
	var temp_k: float = _get_temperature_k(body)
	var rotation_h: float = absf(body.physical.rotation_period_s) / 3600.0
	if rotation_h < 1.0:
		rotation_h = 10.0

	# Mini-Neptune: small gas-envelope planet, heavy haze, almost featureless.
	if mass_earth < 25.0:
		return Archetype.MINI_NEPTUNE

	# Super-Jupiter: very massive regardless of temperature.
	if mass_earth > 500.0:
		return Archetype.SUPER_JUPITER

	# Hot Jupiter: high temperature.
	if temp_k > 700.0:
		return Archetype.HOT_JUPITER

	# Ice giants: cold bodies.
	if temp_k < 150.0:
		if rotation_h > 16.0:
			return Archetype.URANUS_CLASS
		return Archetype.NEPTUNE_CLASS

	# Warm gas giants: distinguish Jupiter (fast, stormy) from Saturn (moderate, banded).
	var ch4: float = _get_composition_fraction(body, "CH4")
	if rotation_h < 12.0 and mass_earth < 200.0 and ch4 < 0.01:
		var oblateness: float = body.physical.oblateness
		if oblateness > 0.08:
			return Archetype.SATURN_CLASS
		return Archetype.JUPITER_CLASS

	return Archetype.JUPITER_CLASS


## Legacy u_g* key compatibility for consumers that expect the old uniform names.
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
		for key: String in ring_params:
			legacy[key] = ring_params[key]
	else:
		legacy["u_ringType"] = 0
	return legacy


static func _color_to_vec3(c: Color) -> Vector3:
	return Vector3(c.r, c.g, c.b)


## Returns whether the body should use gas giant rendering.
## @param body: The celestial body.
## @return: True if gas giant (mass >= 10 Earth, no terrain, or surface type indicates gas giant).
static func is_gas_giant(body: CelestialBody) -> bool:
	if body.has_surface() and body.surface.has_terrain():
		return false
	var mass_earth: float = body.physical.mass_kg / Units.EARTH_MASS_KG
	if not body.has_surface() and mass_earth >= 10.0:
		return true
	if mass_earth >= 15.0:
		return true
	if mass_earth < 10.0:
		return false
	if body.has_surface():
		var st: String = body.surface.surface_type.to_lower()
		return st in ["gaseous", "gas_giant", "ice_giant"]
	return false


static func _get_shape_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}
	params["u_oblateness"] = 0.0
	params["u_axialTilt"] = deg_to_rad(body.physical.axial_tilt_deg)
	return params


## Band parameters: preset base ± physical modifiers ± RNG jitter.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved).
## @param preset: Archetype preset dictionary.
## @param rng: Seeded RNG for variation.
## @return: Band shader parameters.
static func _get_band_params(body: CelestialBody, _archetype: Archetype, preset: Dictionary, rng: RandomNumberGenerator) -> Dictionary:
	var params: Dictionary = {}
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0
	var rotation_h: float = clampf(rotation_period_s / 3600.0, 8.0, 30.0)
	var rot_t: float = clampf(1.0 - (rotation_h - 8.0) / 22.0, 0.0, 1.0)
	var heat_t: float = clampf(body.physical.internal_heat_watts / 2.0e17, 0.0, 1.0)

	# Band count: preset ± rotation ± RNG (±2.5)
	var band_nudge: float = rot_t * 3.0 + rng.randf_range(-2.5, 2.5)
	params["u_bandCount"] = clampf(float(preset["band_count"]) + band_nudge, 4.0, 28.0)

	params["u_bandContrast"] = clampf(
		float(preset["band_contrast"]) + heat_t * 0.12 + rng.randf_range(-0.08, 0.08),
		0.05, 0.95
	)

	params["u_bandTurbulence"] = clampf(
		float(preset["band_turb"]) * 0.7 + heat_t * 0.10 + rot_t * 0.08 + rng.randf_range(-0.06, 0.06),
		0.05, 0.95
	)

	# Band shape variety knobs
	params["u_bandSharpness"] = clampf(
		float(preset["band_sharpness"]) + rng.randf_range(-0.15, 0.15),
		0.0, 1.0
	)
	params["u_bandWarp"] = clampf(
		float(preset["band_warp"]) + rng.randf_range(-0.12, 0.12),
		0.0, 1.0
	)
	params["u_chevronStrength"] = clampf(
		float(preset["chevron"]) + rng.randf_range(-0.08, 0.08),
		0.0, 0.6
	)

	params["u_flowDetail"] = clampi(int(preset["flow_detail"]) + rng.randi_range(-1, 1), 2, 8)
	params["u_jetStrength"] = clampf(
		float(preset["jet_strength"]) + rng.randf_range(-0.2, 0.2),
		0.3, 2.0
	)

	return params


## Storm parameters: variable count, dark-spot ratio, size/vortex jitter.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved).
## @param preset: Archetype preset dictionary.
## @param rng: Seeded RNG for variation.
## @return: Storm shader parameters.
static func _get_storm_params(body: CelestialBody, _archetype: Archetype, preset: Dictionary, rng: RandomNumberGenerator) -> Dictionary:
	var params: Dictionary = {}
	var heat_t: float = clampf(body.physical.internal_heat_watts / 2.0e17, 0.0, 1.0)

	params["u_stormIntensity"] = clampf(
		float(preset["storm_intensity"]) * 0.85 + heat_t * 0.12 + rng.randf_range(-0.08, 0.08),
		0.0, 0.95
	)
	params["u_stormScale"] = clampf(
		float(preset["storm_scale"]) + heat_t * 0.50 + rng.randf_range(-0.30, 0.30),
		0.80, 4.50
	)
	params["u_vortexStrength"] = clampf(
		float(preset["vortex"]) * 0.9 + rng.randf_range(-0.12, 0.12),
		0.05, 1.40
	)

	var sc_lo: int = int(preset["storm_count_lo"])
	var sc_hi: int = int(preset["storm_count_hi"])
	params["u_stormCount"] = rng.randi_range(sc_lo, sc_hi)

	params["u_darkSpotRatio"] = clampf(
		float(preset["dark_spot_ratio"]) + rng.randf_range(-0.15, 0.15),
		0.0, 1.0
	)

	return params


## Detail level and haze — makes mini-Neptunes / Uranus look fundamentally different.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved).
## @param preset: Archetype preset dictionary.
## @param rng: Seeded RNG for variation.
## @return: Detail and haze shader parameters.
static func _get_detail_params(_body: CelestialBody, _archetype: Archetype, preset: Dictionary, rng: RandomNumberGenerator) -> Dictionary:
	var params: Dictionary = {}

	params["u_detailLevel"] = clampf(
		float(preset["detail_level"]) + rng.randf_range(-0.10, 0.10),
		0.05, 1.0
	)
	params["u_hazeDensity"] = clampf(
		float(preset["haze_density"]) + rng.randf_range(-0.10, 0.10),
		0.0, 0.90
	)
	params["u_streakIntensity"] = clampf(
		float(preset["streak"]) + rng.randf_range(-0.08, 0.08),
		0.0, 0.60
	)

	return params


## Colors: pick one of several palette variants + hue shift + per-channel jitter.
## @param body: The gas giant body.
## @param _archetype: Classified archetype (reserved).
## @param preset: Archetype preset dictionary.
## @param rng: Seeded RNG for variation.
## @return: Color shader parameters.
static func _get_color_params(body: CelestialBody, _archetype: Archetype, preset: Dictionary, rng: RandomNumberGenerator) -> Dictionary:
	var params: Dictionary = {}

	var hue_shift: float = rng.randf_range(-0.033, 0.033)

	var palettes: Array = preset.get("palettes", []) as Array
	var zone_eq: Color
	var zone_mid: Color
	var belt_eq: Color
	var belt_mid: Color
	var belt_polar: Color
	var storm_c: Color
	var polar_c: Color
	var haze_c: Color
	var atmo_c: Color

	if not palettes.is_empty():
		var idx: int = rng.randi_range(0, palettes.size() - 1)
		var pal: Dictionary = palettes[idx] as Dictionary
		zone_eq = pal["zone_eq"] as Color
		zone_mid = pal["zone_mid"] as Color
		belt_eq = pal["belt_eq"] as Color
		belt_mid = pal["belt_mid"] as Color
		belt_polar = pal["belt_polar"] as Color
		storm_c = pal["storm"] as Color
		polar_c = pal["polar"] as Color
		haze_c = pal["haze"] as Color
		atmo_c = pal["atmo"] as Color
	else:
		var bl: Color = preset["band_light"] as Color
		var bd: Color = preset["band_dark"] as Color
		var pl: Color = preset["polar"] as Color
		zone_eq = bl
		zone_mid = bl.lerp(pl, 0.3)
		belt_eq = bd
		belt_mid = bd.lerp(pl, 0.25)
		belt_polar = bd.lerp(pl, 0.6)
		storm_c = preset["storm"] as Color
		polar_c = pl
		haze_c = bl.lerp(pl, 0.5)
		atmo_c = preset["atmo_color"] as Color

	var jitter: float = 0.04

	params["u_colBandLight"] = _finalize_color(zone_eq, hue_shift, rng, jitter)
	params["u_colZoneMid"] = _finalize_color(zone_mid, hue_shift, rng, jitter)
	params["u_colBandDark"] = _finalize_color(belt_eq, hue_shift * 0.8, rng, jitter)
	params["u_colBeltMid"] = _finalize_color(belt_mid, hue_shift * 0.8, rng, jitter)
	params["u_colBeltPolar"] = _finalize_color(belt_polar, hue_shift * 0.7, rng, jitter)
	params["u_colStorm"] = _finalize_color(storm_c, hue_shift * 0.5, rng, jitter)
	params["u_colPolar"] = _finalize_color(polar_c, hue_shift * 0.6, rng, jitter)
	params["u_hazeColor"] = _finalize_color(haze_c, hue_shift * 0.6, rng, jitter * 0.5)

	var atmo_shifted: Color = _shift_hue(atmo_c, hue_shift * 0.5)
	if body.has_atmosphere():
		var sky_color: Color = _color_utils.atmosphere_to_sky_color(body.atmosphere.composition)
		atmo_shifted = atmo_shifted.lerp(sky_color, 0.30)
	params["u_atmoColor"] = _clamp_color(atmo_shifted)

	return params


## Atmosphere rim: preset ± RNG jitter.
static func _get_atmosphere_params(_body: CelestialBody, _archetype: Archetype, preset: Dictionary, rng: RandomNumberGenerator) -> Dictionary:
	var params: Dictionary = {}
	params["u_atmoDensity"] = clampf(float(preset["atmo_density"]) + rng.randf_range(-0.10, 0.10), 0.40, 1.80)
	params["u_atmoFalloff"] = clampf(float(preset["atmo_falloff"]) + rng.randf_range(-0.15, 0.15), 1.20, 4.50)
	params["u_scatterStrength"] = clampf(float(preset["scatter"]) + rng.randf_range(-0.08, 0.08), 0.30, 1.40)
	return params


static func _get_lighting_params(_body: CelestialBody, _archetype: Archetype, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}
	params["u_limbDark"] = float(preset["limb_dark"])
	params["u_terminatorSharp"] = float(preset["terminator_sharp"])
	params["u_lightX"] = 0.7
	params["u_lightY"] = 0.3
	params["u_ambient"] = float(preset["ambient"])
	return params


static func _get_animation_params(body: CelestialBody, preset: Dictionary) -> Dictionary:
	var params: Dictionary = {}
	var rotation_period_s: float = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 36000.0

	var physics_rot: float = ShaderParamHelpers.calculate_visual_rotation_speed(rotation_period_s) * 2.0
	var preset_rot: float = float(preset["band_count"]) * 0.006
	params["u_rotSpeed"] = clampf(lerpf(physics_rot, preset_rot, 0.4), 0.04, 0.22)

	var rot_h: float = clampf(rotation_period_s / 3600.0, 8.0, 30.0)
	var rot_t: float = clampf(1.0 - (rot_h - 8.0) / 22.0, 0.0, 1.0)
	params["u_flowSpeed"] = clampf(float(preset["flow_speed"]) + rot_t * 0.08, 0.05, 0.70)

	return params


# ----------------------------------------------------------------------------
# Helpers
# ----------------------------------------------------------------------------

static func _get_temperature_k(body: CelestialBody) -> float:
	if body.has_surface():
		return body.surface.temperature_k
	if body.has_atmosphere():
		return 150.0 + (body.atmosphere.greenhouse_factor - 1.0) * 200.0
	return 150.0


static func _get_composition_fraction(body: CelestialBody, gas: String) -> float:
	if not body.has_atmosphere():
		return 0.0
	return body.atmosphere.composition.get(gas, 0.0) as float


static func _shift_hue(c: Color, delta: float) -> Color:
	var h: float = c.h + delta
	h = h - floorf(h)
	return Color.from_hsv(h, c.s, c.v, c.a)


static func _clamp_color(c: Color) -> Color:
	return Color(clampf(c.r, 0.0, 1.0), clampf(c.g, 0.0, 1.0), clampf(c.b, 0.0, 1.0), c.a)


## Applies hue shift, per-channel jitter, and clamping.
## @param c: Base color.
## @param hue_shift: Hue delta in [0,1] units.
## @param rng: Seeded RNG for jitter.
## @param jitter: Max per-channel offset.
## @return: Final color for shader.
static func _finalize_color(c: Color, hue_shift: float, rng: RandomNumberGenerator, jitter: float) -> Color:
	var shifted: Color = _shift_hue(c, hue_shift)
	return _clamp_color(Color(
		shifted.r + rng.randf_range(-jitter, jitter),
		shifted.g + rng.randf_range(-jitter, jitter),
		shifted.b + rng.randf_range(-jitter, jitter),
		shifted.a
	))
