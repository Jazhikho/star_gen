## Derives star shader uniforms from StellarProps and PhysicalProps.
## Used by MaterialFactory when creating star materials.
class_name StarShaderParams
extends RefCounted


## Returns shader parameters for star rendering.
## @param body: The star CelestialBody.
## @return: Dictionary of shader uniform values.
static func get_star_shader_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var temperature_k: float = 5778.0
	var luminosity_solar: float = 1.0
	var age_years: float = 4.6e9
	var rotation_period_s: float = 2.16e6

	if body.has_stellar():
		temperature_k = body.stellar.effective_temperature_k
		luminosity_solar = body.stellar.luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
		age_years = body.stellar.age_years

	rotation_period_s = absf(body.physical.rotation_period_s)
	if rotation_period_s < 1.0:
		rotation_period_s = 2.16e6

	var seed_value: float = 0.0
	if body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0

	params["u_temperature"] = temperature_k
	params["u_star_color"] = ColorUtils.temperature_to_blackbody_color(temperature_k)
	params["u_luminosity"] = clampf(sqrt(luminosity_solar), 0.3, 3.0)
	params["u_limbDark"] = _calculate_limb_darkening(temperature_k)

	var gran_params: Dictionary = _calculate_granulation_params(temperature_k)
	params["u_granScale"] = gran_params["scale"]
	params["u_granContrast"] = gran_params["contrast"]
	params["u_granTurb"] = gran_params["turbulence"]
	params["u_granFlow"] = gran_params["flow"]
	params["u_superGranScale"] = gran_params["super_scale"]
	params["u_superGranStr"] = gran_params["super_strength"]

	var spot_params: Dictionary = _calculate_spot_params(temperature_k, age_years, rotation_period_s)
	params["u_spotCount"] = spot_params["count"]
	params["u_spotSize"] = spot_params["size"]
	params["u_penumbra"] = spot_params["penumbra"]
	params["u_spotDark"] = spot_params["darkness"]

	var chromo_params: Dictionary = _calculate_chromosphere_params(temperature_k)
	params["u_chromoThick"] = chromo_params["thickness"]
	params["u_chromoIntensity"] = chromo_params["intensity"]
	params["u_chromoShift"] = chromo_params["shift"]

	var corona_params: Dictionary = _calculate_corona_params(temperature_k, luminosity_solar)
	params["u_coronaExtent"] = corona_params["extent"]
	params["u_coronaBright"] = corona_params["brightness"]
	params["u_coronaStreams"] = corona_params["streams"]
	params["u_coronaLength"] = corona_params["length"]
	params["u_coronaAsym"] = corona_params["asymmetry"]

	var prom_params: Dictionary = _calculate_prominence_params(temperature_k, age_years)
	params["u_promCount"] = prom_params["count"]
	params["u_promHeight"] = prom_params["height"]
	params["u_promGlow"] = prom_params["glow"]
	params["u_flareIntensity"] = prom_params["flare_intensity"]

	params["u_rotSpeed"] = ShaderParamHelpers.calculate_visual_rotation_speed(rotation_period_s)
	params["u_bloomRadius"] = 0.15
	params["u_bloomIntensity"] = clampf(0.4 + luminosity_solar * 0.2, 0.4, 1.5)
	params["u_spikeCount"] = 4.0
	params["u_spikeLength"] = 0.3
	params["u_spikeBright"] = 0.25
	params["u_seed"] = seed_value

	return params


## Calculates limb darkening coefficient based on stellar temperature.
## Hotter stars have less limb darkening; cooler stars darken more at the limb.
## @param temperature_k: Effective temperature in Kelvin.
## @return: Limb darkening coefficient (0-1).
static func _calculate_limb_darkening(temperature_k: float) -> float:
	if temperature_k > 30000.0:
		return 0.2
	elif temperature_k > 10000.0:
		return 0.3
	elif temperature_k > 7500.0:
		return 0.45
	elif temperature_k > 6000.0:
		return 0.55
	elif temperature_k > 5200.0:
		return 0.6
	elif temperature_k > 3700.0:
		return 0.68
	else:
		return 0.8


## Calculates granulation parameters based on stellar temperature.
## Cooler stars have more prominent, larger granulation cells.
## Hotter stars have minimal visible granulation.
## @param temperature_k: Effective temperature in Kelvin.
## @return: Dictionary with scale, contrast, turbulence, flow, super_scale, super_strength.
static func _calculate_granulation_params(temperature_k: float) -> Dictionary:
	var params: Dictionary = {}
	if temperature_k > 10000.0:
		params["scale"] = 10.0
		params["contrast"] = 0.05
		params["turbulence"] = 0.15
		params["flow"] = 0.02
		params["super_scale"] = 3.0
		params["super_strength"] = 0.05
	elif temperature_k > 7500.0:
		params["scale"] = 20.0
		params["contrast"] = 0.15
		params["turbulence"] = 0.25
		params["flow"] = 0.04
		params["super_scale"] = 4.0
		params["super_strength"] = 0.08
	elif temperature_k > 6000.0:
		params["scale"] = 26.0
		params["contrast"] = 0.28
		params["turbulence"] = 0.35
		params["flow"] = 0.06
		params["super_scale"] = 5.0
		params["super_strength"] = 0.12
	elif temperature_k > 5200.0:
		params["scale"] = 30.0
		params["contrast"] = 0.35
		params["turbulence"] = 0.4
		params["flow"] = 0.08
		params["super_scale"] = 6.0
		params["super_strength"] = 0.15
	elif temperature_k > 3700.0:
		params["scale"] = 28.0
		params["contrast"] = 0.42
		params["turbulence"] = 0.45
		params["flow"] = 0.1
		params["super_scale"] = 5.0
		params["super_strength"] = 0.12
	else:
		params["scale"] = 20.0
		params["contrast"] = 0.55
		params["turbulence"] = 0.55
		params["flow"] = 0.12
		params["super_scale"] = 4.0
		params["super_strength"] = 0.1
	return params


## Calculates sunspot parameters based on stellar properties.
## Younger, faster-rotating stars have more spots due to stronger magnetic activity.
## Hot stars (>8000K) have no spots; cooler stars can have many large spots.
## @param temperature_k: Effective temperature in Kelvin.
## @param age_years: Stellar age in years.
## @param rotation_period_s: Rotation period in seconds.
## @return: Dictionary with count, size, penumbra, darkness.
static func _calculate_spot_params(temperature_k: float, age_years: float, rotation_period_s: float) -> Dictionary:
	var params: Dictionary = {}
	var age_factor: float = clampf(1.0 - (age_years / 10.0e9), 0.2, 1.0)
	var solar_rotation: float = 2.16e6
	var rotation_factor: float = clampf(solar_rotation / maxf(rotation_period_s, 1.0), 0.5, 2.0)
	var activity: float = age_factor * rotation_factor

	if temperature_k > 8000.0:
		params["count"] = 0.0
		params["size"] = 0.0
		params["penumbra"] = 2.0
		params["darkness"] = 0.35
	elif temperature_k > 6000.0:
		params["count"] = floorf(activity * 3.0)
		params["size"] = 0.04 + activity * 0.02
		params["penumbra"] = 2.0
		params["darkness"] = 0.4
	elif temperature_k > 5200.0:
		params["count"] = floorf(activity * 5.0)
		params["size"] = 0.05 + activity * 0.02
		params["penumbra"] = 2.0
		params["darkness"] = 0.35
	elif temperature_k > 3700.0:
		params["count"] = floorf(activity * 7.0)
		params["size"] = 0.06 + activity * 0.02
		params["penumbra"] = 1.8
		params["darkness"] = 0.35
	else:
		params["count"] = floorf(activity * 12.0)
		params["size"] = 0.08 + activity * 0.04
		params["penumbra"] = 1.6
		params["darkness"] = 0.3
	return params


## Calculates chromosphere parameters based on stellar temperature.
## Cooler stars have thicker, more intense chromospheres with stronger emission.
## Hotter stars have thin, less visible chromospheres.
## @param temperature_k: Effective temperature in Kelvin.
## @return: Dictionary with thickness, intensity, shift.
static func _calculate_chromosphere_params(temperature_k: float) -> Dictionary:
	var params: Dictionary = {}
	if temperature_k > 10000.0:
		params["thickness"] = 0.008
		params["intensity"] = 0.3
		params["shift"] = 0.2
	elif temperature_k > 6000.0:
		params["thickness"] = 0.012
		params["intensity"] = 0.6
		params["shift"] = 0.4
	elif temperature_k > 5200.0:
		params["thickness"] = 0.015
		params["intensity"] = 0.8
		params["shift"] = 0.5
	elif temperature_k > 3700.0:
		params["thickness"] = 0.018
		params["intensity"] = 0.9
		params["shift"] = 0.55
	else:
		params["thickness"] = 0.02
		params["intensity"] = 1.2
		params["shift"] = 0.6
	return params


## Calculates corona parameters based on stellar temperature and luminosity.
## Hotter, more luminous stars have more extensive coronae with more streamers.
## Cooler stars have smaller, dimmer coronae.
## @param temperature_k: Effective temperature in Kelvin.
## @param luminosity_solar: Luminosity in solar units.
## @return: Dictionary with extent, brightness, streams, length, asymmetry.
static func _calculate_corona_params(temperature_k: float, luminosity_solar: float) -> Dictionary:
	var params: Dictionary = {}
	var lum_factor: float = clampf(sqrt(luminosity_solar), 0.5, 2.0)
	if temperature_k > 10000.0:
		params["extent"] = 0.5 * lum_factor
		params["brightness"] = 0.8
		params["streams"] = 12.0
		params["length"] = 0.6
		params["asymmetry"] = 0.2
	elif temperature_k > 6000.0:
		params["extent"] = 0.35 * lum_factor
		params["brightness"] = 0.55
		params["streams"] = 10.0
		params["length"] = 0.5
		params["asymmetry"] = 0.25
	elif temperature_k > 5200.0:
		params["extent"] = 0.3 * lum_factor
		params["brightness"] = 0.5
		params["streams"] = 8.0
		params["length"] = 0.5
		params["asymmetry"] = 0.3
	elif temperature_k > 3700.0:
		params["extent"] = 0.25 * lum_factor
		params["brightness"] = 0.4
		params["streams"] = 6.0
		params["length"] = 0.4
		params["asymmetry"] = 0.35
	else:
		params["extent"] = 0.2 * lum_factor
		params["brightness"] = 0.3
		params["streams"] = 4.0
		params["length"] = 0.35
		params["asymmetry"] = 0.4
	return params


## Calculates prominence and flare parameters based on stellar properties.
## Younger stars are more magnetically active with stronger, more frequent flares.
## Prominence count and height vary with temperature and activity level.
## @param temperature_k: Effective temperature in Kelvin.
## @param age_years: Stellar age in years.
## @return: Dictionary with count, height, glow, flare_intensity.
static func _calculate_prominence_params(temperature_k: float, age_years: float) -> Dictionary:
	var params: Dictionary = {}
	var age_factor: float = clampf(1.0 - (age_years / 10.0e9), 0.3, 1.0)
	if temperature_k > 10000.0:
		params["count"] = 1.0
		params["height"] = 0.08
		params["glow"] = 0.5
		params["flare_intensity"] = 0.1 * age_factor
	elif temperature_k > 6000.0:
		params["count"] = 2.0
		params["height"] = 0.1
		params["glow"] = 0.7
		params["flare_intensity"] = 0.15 * age_factor
	elif temperature_k > 5200.0:
		params["count"] = 3.0
		params["height"] = 0.12
		params["glow"] = 0.8
		params["flare_intensity"] = 0.2 * age_factor
	elif temperature_k > 3700.0:
		params["count"] = 3.0
		params["height"] = 0.15
		params["glow"] = 0.9
		params["flare_intensity"] = 0.3 * age_factor
	else:
		params["count"] = 2.0
		params["height"] = 0.1
		params["glow"] = 0.7
		params["flare_intensity"] = 0.5 * age_factor
	return params
