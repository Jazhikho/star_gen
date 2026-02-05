## Derives terrestrial planet shader uniforms from body components.
## Used by MaterialFactory when creating rocky/terrestrial materials.
class_name TerrestrialShaderParams
extends RefCounted


## Returns shader parameters for terrestrial planet rendering.
## @param body: The planet CelestialBody.
## @return: Dictionary of shader uniform values.
static func get_terrestrial_shader_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var seed_value: float = 0.0
	if body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0
	params["u_seed"] = seed_value
	params["u_axialTilt"] = deg_to_rad(body.physical.axial_tilt_deg)
	params["u_rotSpeed"] = ShaderParamHelpers.calculate_visual_rotation_speed(absf(body.physical.rotation_period_s))

	if body.has_surface() and body.surface.has_terrain():
		var terrain: TerrainProps = body.surface.terrain
		params["u_terrainScale"] = 3.0 + terrain.roughness * 4.0
		params["u_terrainHeight"] = clampf(terrain.elevation_range_m / 20000.0, 0.2, 0.8)
		params["u_roughness"] = 0.4 + terrain.roughness * 0.3
		params["u_continentSize"] = 1.0 + (1.0 - terrain.tectonic_activity) * 2.0
		params["u_octaves"] = 5 + int(terrain.roughness * 3.0)
	else:
		params["u_terrainScale"] = 4.0
		params["u_terrainHeight"] = 0.5
		params["u_roughness"] = 0.55
		params["u_continentSize"] = 1.5
		params["u_octaves"] = 6

	if body.has_surface() and body.surface.has_hydrosphere():
		var hydro: HydrosphereProps = body.surface.hydrosphere
		params["u_seaLevel"] = clampf(hydro.ocean_coverage * 0.6 + 0.2, 0.0, 0.7)
		params["u_seaSpecular"] = 0.8
		params["u_fresnelStrength"] = 1.0
		var ocean_colors: Dictionary = _get_ocean_colors(hydro)
		params["u_colSeaShallow"] = ocean_colors["shallow"]
		params["u_colSeaDeep"] = ocean_colors["deep"]
	else:
		params["u_seaLevel"] = 0.0
		params["u_seaSpecular"] = 0.0
		params["u_fresnelStrength"] = 0.0
		params["u_colSeaShallow"] = Vector3(0.133, 0.533, 0.733)
		params["u_colSeaDeep"] = Vector3(0.039, 0.133, 0.267)

	if body.has_surface() and body.surface.has_cryosphere():
		var cryo: CryosphereProps = body.surface.cryosphere
		params["u_iceCap"] = cryo.polar_cap_coverage
		params["u_colIce"] = Vector3(0.867, 0.933, 1.0)
	else:
		params["u_iceCap"] = 0.0
		params["u_colIce"] = Vector3(0.867, 0.933, 1.0)

	var surface_colors: Dictionary = _get_surface_colors(body)
	params["u_colLow"] = surface_colors["low"]
	params["u_colMid"] = surface_colors["mid"]
	params["u_colHigh"] = surface_colors["high"]
	params["u_colPeak"] = surface_colors["peak"]

	if body.has_atmosphere():
		var atmo: AtmosphereProps = body.atmosphere
		var pressure_earth: float = atmo.surface_pressure_pa / 101325.0
		params["u_atmoDensity"] = clampf(pressure_earth, 0.0, 2.0)
		params["u_atmoFalloff"] = 2.0 + (1.0 - clampf(pressure_earth, 0.0, 1.0)) * 1.5
		params["u_scatterStrength"] = clampf(pressure_earth * 0.8, 0.0, 1.2)
		var sky_color: Color = ColorUtils.atmosphere_to_sky_color(atmo.composition)
		params["u_atmoColor"] = Vector3(sky_color.r, sky_color.g, sky_color.b)
		var cloud_params: Dictionary = _calculate_cloud_params(atmo, body.surface)
		params["u_cloudCoverage"] = cloud_params["coverage"]
		params["u_cloudScale"] = cloud_params["scale"]
		params["u_cloudShadow"] = cloud_params["shadow"]
		params["u_cloudColor"] = cloud_params["color"]
		params["u_cloudDrift"] = cloud_params["drift"]
	else:
		params["u_atmoDensity"] = 0.0
		params["u_atmoFalloff"] = 4.0
		params["u_scatterStrength"] = 0.0
		params["u_atmoColor"] = Vector3(0.5, 0.5, 0.5)
		params["u_cloudCoverage"] = 0.0
		params["u_cloudScale"] = 3.5
		params["u_cloudShadow"] = 0.0
		params["u_cloudColor"] = Vector3(1.0, 1.0, 1.0)
		params["u_cloudDrift"] = 0.0

	params["u_lightX"] = 0.7
	params["u_lightY"] = 0.4
	params["u_ambient"] = 0.04
	params["u_limbDark"] = 0.8
	params["u_terminatorSharp"] = 0.12

	if body.has_ring_system():
		var ring_params: Dictionary = RingShaderParams.get_ring_shader_params(body.ring_system, body.physical.radius_m)
		for key in ring_params:
			params[key] = ring_params[key]
	else:
		params["u_ringType"] = 0

	return params


static func _get_ocean_colors(hydro: HydrosphereProps) -> Dictionary:
	var colors: Dictionary = {}
	match hydro.water_type:
		"water":
			colors["shallow"] = Vector3(0.133, 0.533, 0.733)
			colors["deep"] = Vector3(0.039, 0.133, 0.267)
		"ammonia":
			colors["shallow"] = Vector3(0.5, 0.55, 0.6)
			colors["deep"] = Vector3(0.2, 0.25, 0.35)
		"methane":
			colors["shallow"] = Vector3(0.3, 0.5, 0.55)
			colors["deep"] = Vector3(0.1, 0.2, 0.25)
		"hydrocarbon":
			colors["shallow"] = Vector3(0.4, 0.35, 0.25)
			colors["deep"] = Vector3(0.15, 0.12, 0.08)
		_:
			colors["shallow"] = Vector3(0.133, 0.533, 0.733)
			colors["deep"] = Vector3(0.039, 0.133, 0.267)
	if hydro.salinity_ppt > 50.0:
		var salt_factor: float = clampf((hydro.salinity_ppt - 50.0) / 100.0, 0.0, 0.3)
		colors["shallow"] = Vector3(
			colors["shallow"].x + salt_factor * 0.1,
			colors["shallow"].y - salt_factor * 0.05,
			colors["shallow"].z - salt_factor * 0.1
		)
	return colors


static func _get_surface_colors(body: CelestialBody) -> Dictionary:
	var colors: Dictionary = {
		"low": Vector3(0.165, 0.29, 0.102),
		"mid": Vector3(0.353, 0.541, 0.227),
		"high": Vector3(0.541, 0.478, 0.353),
		"peak": Vector3(0.8, 0.733, 0.667),
	}
	if not body.has_surface():
		return colors
	var surface: SurfaceProps = body.surface
	match surface.surface_type:
		"rocky":
			colors["low"] = Vector3(0.35, 0.32, 0.28)
			colors["mid"] = Vector3(0.45, 0.42, 0.38)
			colors["high"] = Vector3(0.55, 0.52, 0.48)
			colors["peak"] = Vector3(0.7, 0.68, 0.65)
		"desert":
			colors["low"] = Vector3(0.6, 0.45, 0.3)
			colors["mid"] = Vector3(0.7, 0.55, 0.35)
			colors["high"] = Vector3(0.75, 0.6, 0.4)
			colors["peak"] = Vector3(0.85, 0.75, 0.6)
		"volcanic":
			colors["low"] = Vector3(0.15, 0.12, 0.1)
			colors["mid"] = Vector3(0.25, 0.2, 0.15)
			colors["high"] = Vector3(0.4, 0.25, 0.15)
			colors["peak"] = Vector3(0.6, 0.35, 0.2)
		"icy":
			colors["low"] = Vector3(0.7, 0.75, 0.8)
			colors["mid"] = Vector3(0.8, 0.85, 0.9)
			colors["high"] = Vector3(0.85, 0.88, 0.92)
			colors["peak"] = Vector3(0.95, 0.97, 1.0)
		"oceanic":
			colors["low"] = Vector3(0.2, 0.35, 0.15)
			colors["mid"] = Vector3(0.25, 0.45, 0.2)
			colors["high"] = Vector3(0.4, 0.45, 0.35)
			colors["peak"] = Vector3(0.6, 0.58, 0.55)
	if surface.temperature_k > 400.0:
		var heat_factor: float = clampf((surface.temperature_k - 400.0) / 300.0, 0.0, 1.0)
		colors["low"] = _lerp_vec3(colors["low"], Vector3(0.5, 0.35, 0.2), heat_factor)
		colors["mid"] = _lerp_vec3(colors["mid"], Vector3(0.6, 0.45, 0.25), heat_factor)
	elif surface.temperature_k < 250.0:
		var cold_factor: float = clampf((250.0 - surface.temperature_k) / 100.0, 0.0, 1.0)
		colors["low"] = _lerp_vec3(colors["low"], Vector3(0.6, 0.65, 0.7), cold_factor)
		colors["mid"] = _lerp_vec3(colors["mid"], Vector3(0.7, 0.75, 0.8), cold_factor)
	return colors


static func _lerp_vec3(a: Vector3, b: Vector3, t: float) -> Vector3:
	return a + (b - a) * t


static func _calculate_cloud_params(atmo: AtmosphereProps, surface: SurfaceProps) -> Dictionary:
	var params: Dictionary = {}
	var pressure_earth: float = atmo.surface_pressure_pa / 101325.0
	var base_coverage: float = clampf(pressure_earth * 0.3, 0.0, 0.6)
	if surface != null and surface.has_hydrosphere():
		base_coverage += surface.hydrosphere.ocean_coverage * 0.3
	params["coverage"] = clampf(base_coverage, 0.0, 0.8)
	params["scale"] = 3.0 + (1.0 - params["coverage"]) * 2.0
	params["shadow"] = 0.2 + params["coverage"] * 0.2
	params["drift"] = 0.02 + pressure_earth * 0.02
	var co2_fraction: float = atmo.composition.get("CO2", 0.0) as float
	var sulfur_fraction: float = atmo.composition.get("SO2", 0.0) as float
	if sulfur_fraction > 0.1:
		params["color"] = Vector3(0.9, 0.85, 0.6)
	elif co2_fraction > 0.5:
		params["color"] = Vector3(0.9, 0.85, 0.75)
	else:
		params["color"] = Vector3(1.0, 1.0, 1.0)
	return params
