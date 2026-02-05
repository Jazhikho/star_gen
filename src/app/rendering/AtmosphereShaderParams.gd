## Derives atmosphere shader uniforms from AtmosphereProps.
## Used by MaterialFactory when creating atmosphere materials.
class_name AtmosphereShaderParams
extends RefCounted


## Returns shader parameters for atmosphere rendering.
## @param body: The celestial body with atmosphere.
## @return: Dictionary of shader uniform values.
static func get_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	if not body.has_atmosphere():
		return _get_default_params()

	var atmo: AtmosphereProps = body.atmosphere

	# === Base color from composition ===
	var sky_color: Color = ColorUtils.atmosphere_to_sky_color(atmo.composition)
	params["u_atmosphereColor"] = Color(sky_color.r, sky_color.g, sky_color.b, 1.0)

	# === Density from pressure ===
	var pressure_ratio: float = atmo.surface_pressure_pa / 101325.0
	params["u_density"] = clampf(pressure_ratio * 0.8, 0.05, 2.0)

	# === Falloff from scale height ===
	params["u_falloff"] = _calculate_falloff(body, atmo)

	# === Scattering strength ===
	params["u_scatterStrength"] = _calculate_scatter_strength(atmo)

	# === Greenhouse effect ===
	var greenhouse_intensity: float = clampf((atmo.greenhouse_factor - 1.0) * 0.5, 0.0, 1.0)
	params["u_greenhouseIntensity"] = greenhouse_intensity
	params["u_greenhouseColor"] = _get_greenhouse_color(atmo)

	# === Lighting ===
	params["u_lightDir"] = Vector3(0.7, 0.4, 0.6)
	params["u_sunGlowStrength"] = _calculate_sun_glow(atmo)

	# === Terminator softness ===
	params["u_terminatorSoftness"] = _calculate_terminator_softness(pressure_ratio)

	return params


## Returns default parameters for bodies without atmosphere data.
static func _get_default_params() -> Dictionary:
	return {
		"u_atmosphereColor": Color(0.4, 0.6, 0.9, 1.0),
		"u_density": 0.6,
		"u_falloff": 3.0,
		"u_scatterStrength": 0.8,
		"u_greenhouseIntensity": 0.0,
		"u_greenhouseColor": Color(1.0, 0.6, 0.3),
		"u_lightDir": Vector3(0.7, 0.4, 0.6),
		"u_sunGlowStrength": 0.3,
		"u_terminatorSoftness": 0.15,
	}


## Calculates atmospheric falloff from scale height.
static func _calculate_falloff(body: CelestialBody, atmo: AtmosphereProps) -> float:
	if not body.physical or body.physical.radius_m <= 0:
		return 3.0

	if atmo.scale_height_m <= 0:
		return 3.0

	var scale_ratio: float = atmo.scale_height_m / body.physical.radius_m
	var falloff: float = 0.5 / maxf(scale_ratio, 0.001)

	return clampf(falloff, 1.0, 10.0)


## Calculates scattering strength from composition.
static func _calculate_scatter_strength(atmo: AtmosphereProps) -> float:
	var comp: Dictionary = atmo.composition

	var h2_fraction: float = (comp.get("H2", 0.0) as float) + (comp.get("He", 0.0) as float)
	var n2_o2_fraction: float = (comp.get("N2", 0.0) as float) + (comp.get("O2", 0.0) as float)
	var co2_fraction: float = comp.get("CO2", 0.0) as float

	var scatter: float = 0.5

	if n2_o2_fraction > 0.5:
		scatter = 0.8 + n2_o2_fraction * 0.2
	elif h2_fraction > 0.5:
		scatter = 0.5 + h2_fraction * 0.2
	elif co2_fraction > 0.5:
		scatter = 0.4 + co2_fraction * 0.3

	var pressure_factor: float = clampf(atmo.surface_pressure_pa / 101325.0, 0.1, 2.0)
	scatter *= sqrt(pressure_factor)

	return clampf(scatter, 0.3, 1.5)


## Gets greenhouse glow color based on composition.
static func _get_greenhouse_color(atmo: AtmosphereProps) -> Color:
	var comp: Dictionary = atmo.composition

	if (comp.get("CO2", 0.0) as float) > 0.5:
		return Color(1.0, 0.5, 0.25)

	if (comp.get("CH4", 0.0) as float) > 0.01:
		return Color(0.9, 0.8, 0.4)

	if (comp.get("SO2", 0.0) as float) > 0.001 or (comp.get("H2S", 0.0) as float) > 0.001:
		return Color(1.0, 0.85, 0.4)

	return Color(1.0, 0.6, 0.3)


## Calculates sun glow strength.
static func _calculate_sun_glow(atmo: AtmosphereProps) -> float:
	var pressure_ratio: float = atmo.surface_pressure_pa / 101325.0

	if pressure_ratio > 2.0:
		return 0.5
	elif pressure_ratio > 0.5:
		return 0.3
	else:
		return 0.15


## Calculates terminator softness from pressure.
static func _calculate_terminator_softness(pressure_ratio: float) -> float:
	if pressure_ratio > 5.0:
		return 0.3
	elif pressure_ratio > 1.0:
		return 0.2
	elif pressure_ratio > 0.1:
		return 0.15
	else:
		return 0.08


## Checks if a body should have visible atmosphere rendering.
## @param body: The celestial body to check.
## @return: True if atmosphere should be rendered.
static func should_render_atmosphere(body: CelestialBody) -> bool:
	if not body.has_atmosphere():
		return false

	var min_pressure: float = 100.0
	return body.atmosphere.surface_pressure_pa >= min_pressure
