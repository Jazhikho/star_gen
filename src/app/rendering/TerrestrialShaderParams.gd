## Derives terrestrial planet shader uniforms from body components.
## Used by MaterialFactory when creating rocky/terrestrial materials.
## get_params() drives planet_terrestrial_surface.gdshader (spatial 3D).
## get_terrestrial_shader_params() drives canvas_item shaders (legacy).
class_name TerrestrialShaderParams
extends RefCounted


## Returns shader parameters for the spatial terrestrial surface shader.
## @param body: The terrestrial planet CelestialBody.
## @return: Dictionary of shader uniform values for planet_terrestrial_surface.gdshader.
static func get_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var seed_value: float = 0.0
	if body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0
	params["u_seed"] = seed_value

	# === Terrain parameters ===
	params.merge(_get_spatial_terrain_params(body))

	# === Surface colors ===
	params.merge(_get_spatial_surface_color_params(body))

	# === Ocean parameters ===
	params.merge(_get_spatial_ocean_params(body))

	# === Ice cap parameters ===
	params.merge(_get_spatial_ice_params(body))

	# === Atmosphere parameters ===
	params.merge(_get_spatial_atmosphere_params(body))

	# === Cloud parameters ===
	params.merge(_get_spatial_cloud_params(body))

	# === Lighting parameters ===
	params.merge(_get_spatial_lighting_params(body))

	# === Animation parameters ===
	params.merge(_get_spatial_animation_params(body))

	# === City lights (default off, can be enabled by population system) ===
	params["u_cityLightIntensity"] = 0.0
	params["u_cityLightColor"] = Color(1.0, 0.85, 0.5)

	return params


## Extracts terrain parameters for the spatial shader.
static func _get_spatial_terrain_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	# Defaults that produce visible terrain
	params["u_terrainScale"] = 4.0
	params["u_terrainHeight"] = 0.5
	params["u_roughness"] = 0.55
	params["u_continentSize"] = 1.5
	params["u_landCoherence"] = 0.6
	params["u_coastalDetail"] = 0.5
	params["u_octaves"] = 6

	if not body.has_surface():
		# No surface data - use Earth-like defaults
		return params

	var surface: SurfaceProps = body.surface
	var surface_type: String = surface.surface_type.to_lower() if surface.surface_type else ""

	if surface.has_terrain():
		var terrain: TerrainProps = body.surface.terrain

		params["u_roughness"] = clampf(terrain.roughness, 0.3, 0.8)

		var elevation_normalized: float = clampf(terrain.elevation_range_m / 25000.0, 0.1, 1.0)
		params["u_terrainHeight"] = elevation_normalized

		# Continent size: high tectonic activity = more fragmented continents (smaller)
		# Low activity = larger, more coherent landmasses (supercontinent-like)
		# Range: 0.8 (supercontinent) to 3.5 (archipelago)
		var tectonic_factor: float = terrain.tectonic_activity
		params["u_continentSize"] = 0.8 + tectonic_factor * 2.7

		# Land coherence: high erosion = more complex coastlines (lower coherence)
		# High crater density = fragmented terrain (lower coherence)
		# Range: 0.2 (archipelago/fragmented) to 0.9 (solid continental masses)
		var fragmentation: float = maxf(terrain.erosion_level, terrain.crater_density)
		params["u_landCoherence"] = clampf(0.85 - fragmentation * 0.65, 0.2, 0.9)

		# Coastal detail: more erosion = more detailed coastlines
		params["u_coastalDetail"] = clampf(0.3 + terrain.erosion_level * 0.5, 0.2, 0.9)

		if terrain.tectonic_activity > 0.5:
			params["u_terrainScale"] = 5.0 + terrain.tectonic_activity * 3.0
			params["u_octaves"] = 7
		elif terrain.tectonic_activity < 0.2:
			params["u_terrainScale"] = 3.0
			params["u_octaves"] = 5

	# Apply surface-type-based adjustments (whether or not terrain data exists)
	match surface_type:
		"oceanic":
			if not surface.has_terrain():
				params["u_continentSize"] = 3.0
				params["u_landCoherence"] = 0.3
		"continental", "temperate", "earthlike", "habitable":
			if not surface.has_terrain():
				params["u_continentSize"] = 1.5
				params["u_landCoherence"] = 0.6
		"desert", "arid":
			if not surface.has_terrain():
				params["u_continentSize"] = 1.2
				params["u_landCoherence"] = 0.75
		"tundra", "cold", "frozen_continental", "subarctic", "arctic":
			if not surface.has_terrain():
				params["u_continentSize"] = 1.8
				params["u_landCoherence"] = 0.55
		"barren", "rocky", "rocky_cold", "cratered":
			if not surface.has_terrain():
				params["u_continentSize"] = 2.0
				params["u_landCoherence"] = 0.5
		"icy", "frozen", "icy_rocky", "icy_cratered", "ice", "glacial":
			if not surface.has_terrain():
				params["u_continentSize"] = 2.5
				params["u_landCoherence"] = 0.4
		"":
			if not surface.has_terrain():
				var temp_k_fallback: float = surface.temperature_k
				if temp_k_fallback < 200.0:
					params["u_continentSize"] = 2.5
					params["u_landCoherence"] = 0.4
				elif temp_k_fallback < 260.0:
					params["u_continentSize"] = 1.8
					params["u_landCoherence"] = 0.55
				elif temp_k_fallback < 320.0:
					params["u_continentSize"] = 1.5
					params["u_landCoherence"] = 0.6
				else:
					params["u_continentSize"] = 1.2
					params["u_landCoherence"] = 0.7
		_:
			if not surface.has_terrain():
				params["u_continentSize"] = 1.8
				params["u_landCoherence"] = 0.55

	# Temperature-based adjustments (applies to all bodies)
	var temp_k: float = surface.temperature_k
	if temp_k < 250.0:
		# Cold worlds: slightly more coherent landmasses
		params["u_landCoherence"] = minf(params["u_landCoherence"] + 0.1, 0.9)
	elif temp_k > 320.0:
		# Hot worlds: slightly more fragmented
		params["u_landCoherence"] = maxf(params["u_landCoherence"] - 0.1, 0.2)

	return params


## Determines surface colors for the spatial shader (as Color).
static func _get_spatial_surface_color_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	var colors: Dictionary = _get_surface_colors(body)
	params["u_colLow"] = Color(colors["low"].x, colors["low"].y, colors["low"].z)
	params["u_colMid"] = Color(colors["mid"].x, colors["mid"].y, colors["mid"].z)
	params["u_colHigh"] = Color(colors["high"].x, colors["high"].y, colors["high"].z)
	params["u_colPeak"] = Color(colors["peak"].x, colors["peak"].y, colors["peak"].z)

	# Ensure colors are never black - add minimum brightness
	var min_brightness: float = 0.15
	for key: String in ["u_colLow", "u_colMid", "u_colHigh", "u_colPeak"]:
		var col: Color = params[key]
		var brightness: float = maxf(maxf(col.r, col.g), col.b)
		if brightness < min_brightness:
			var boost: float = min_brightness / maxf(brightness, 0.01)
			params[key] = Color(col.r * boost + 0.05, col.g * boost + 0.05, col.b * boost + 0.05)

	if body.has_surface():
		var composition: Dictionary = body.surface.surface_composition
		if composition.has("iron_oxides"):
			var iron: float = composition["iron_oxides"] as float
			var rust_tint: Color = Color(0.7, 0.4, 0.3)
			params["u_colLow"] = params["u_colLow"].lerp(rust_tint, iron * 0.4)
			params["u_colMid"] = params["u_colMid"].lerp(rust_tint, iron * 0.3)

		var temp_k: float = body.surface.temperature_k
		if temp_k > 320.0:
			params["u_colLow"] = params["u_colLow"].lerp(Color(0.6, 0.5, 0.35), 0.3)
			params["u_colMid"] = params["u_colMid"].lerp(Color(0.7, 0.6, 0.4), 0.3)
		elif temp_k < 260.0:
			params["u_colLow"] = params["u_colLow"].lerp(Color(0.4, 0.45, 0.4), 0.3)
			params["u_colMid"] = params["u_colMid"].lerp(Color(0.5, 0.55, 0.5), 0.3)

	return params


## Extracts ocean parameters for the spatial shader.
static func _get_spatial_ocean_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	params["u_seaLevel"] = 0.4
	params["u_seaSpecular"] = 0.8
	params["u_fresnelStrength"] = 1.0
	params["u_colSeaShallow"] = Color(0.133, 0.533, 0.733)
	params["u_colSeaDeep"] = Color(0.039, 0.133, 0.267)

	if body.has_surface() and body.surface.has_hydrosphere():
		var hydro: HydrosphereProps = body.surface.hydrosphere

		params["u_seaLevel"] = clampf(0.3 + hydro.ocean_coverage * 0.4, 0.0, 0.8)

		var depth_factor: float = clampf(hydro.ocean_depth_m / 5000.0, 0.0, 1.0)
		var deep_base: Color = Color(0.039, 0.133, 0.267)
		var very_deep: Color = Color(0.02, 0.06, 0.15)
		params["u_colSeaDeep"] = deep_base.lerp(very_deep, depth_factor)

		var ocean_colors: Dictionary = _get_ocean_colors(hydro)
		params["u_colSeaShallow"] = Color(ocean_colors["shallow"].x, ocean_colors["shallow"].y, ocean_colors["shallow"].z)
		params["u_colSeaDeep"] = Color(ocean_colors["deep"].x, ocean_colors["deep"].y, ocean_colors["deep"].z)

		match hydro.water_type.to_lower():
			"methane":
				params["u_seaSpecular"] = 0.5
			"ammonia":
				params["u_seaSpecular"] = 0.6
			"hydrocarbon":
				params["u_seaSpecular"] = 0.4

		if hydro.ice_coverage > 0.5:
			params["u_seaSpecular"] *= (1.0 - hydro.ice_coverage * 0.5)
	elif body.has_surface():
		var surface_type: String = body.surface.surface_type.to_lower()
		var dry_types: Array[String] = [
			"desert", "rocky", "rocky_cold", "volcanic", "molten",
			"tundra", "arid", "barren", "cratered",
			"frozen", "icy", "icy_rocky", "icy_cratered",
		]
		if surface_type in dry_types:
			params["u_seaLevel"] = 0.05

	return params


## Extracts ice cap parameters for the spatial shader.
static func _get_spatial_ice_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	params["u_iceCap"] = 0.3
	params["u_colIce"] = Color(0.867, 0.933, 1.0)

	if body.has_surface() and body.surface.has_cryosphere():
		var cryo: CryosphereProps = body.surface.cryosphere

		params["u_iceCap"] = clampf(cryo.polar_cap_coverage, 0.0, 1.0)

		match cryo.ice_type.to_lower():
			"water_ice":
				params["u_colIce"] = Color(0.867, 0.933, 1.0)
			"co2_ice", "dry_ice":
				params["u_colIce"] = Color(0.95, 0.95, 0.95)
			"nitrogen_ice":
				params["u_colIce"] = Color(0.85, 0.88, 0.95)
			"methane_ice":
				params["u_colIce"] = Color(0.9, 0.85, 0.8)
	elif body.has_surface():
		var temp_k: float = body.surface.temperature_k
		if temp_k < 200.0:
			params["u_iceCap"] = clampf((250.0 - temp_k) / 100.0, 0.3, 0.9)
		elif temp_k > 300.0:
			params["u_iceCap"] = 0.0

	return params


## Extracts atmosphere parameters for the spatial shader.
static func _get_spatial_atmosphere_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	params["u_atmoDensity"] = 0.6
	params["u_atmoFalloff"] = 2.5
	params["u_scatterStrength"] = 0.8
	params["u_atmoColor"] = Color(0.267, 0.533, 0.8)

	if body.has_atmosphere():
		var atmo: AtmosphereProps = body.atmosphere

		var pressure_ratio: float = atmo.surface_pressure_pa / 101325.0
		params["u_atmoDensity"] = clampf(pressure_ratio * 0.6, 0.0, 2.0)

		if pressure_ratio < 0.1:
			params["u_atmoFalloff"] = 4.0
		elif pressure_ratio > 2.0:
			params["u_atmoFalloff"] = 1.5

		params["u_atmoColor"] = ColorUtils.atmosphere_to_sky_color(atmo.composition)

		if atmo.greenhouse_factor > 1.2:
			var greenhouse_strength: float = clampf((atmo.greenhouse_factor - 1.0) / 2.0, 0.0, 1.0)
			var warm_tint: Color = Color(0.9, 0.7, 0.4)
			params["u_atmoColor"] = params["u_atmoColor"].lerp(warm_tint, greenhouse_strength * 0.3)

		params["u_scatterStrength"] = clampf(0.5 + pressure_ratio * 0.3, 0.3, 1.2)
	else:
		params["u_atmoDensity"] = 0.0
		params["u_scatterStrength"] = 0.0

	return params


## Derives cloud parameters for the spatial shader.
static func _get_spatial_cloud_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	params["u_cloudCoverage"] = 0.4
	params["u_cloudScale"] = 3.5
	params["u_cloudShadow"] = 0.3
	params["u_cloudColor"] = Color(1.0, 1.0, 1.0)

	if body.has_atmosphere():
		var atmo: AtmosphereProps = body.atmosphere

		var has_water: bool = atmo.composition.has("H2O")
		var pressure_ratio: float = atmo.surface_pressure_pa / 101325.0

		if has_water:
			var water_fraction: float = atmo.composition.get("H2O", 0.0) as float
			params["u_cloudCoverage"] = clampf(water_fraction * 10.0 + pressure_ratio * 0.2, 0.0, 0.9)
		else:
			if atmo.composition.has("SO2") or atmo.composition.has("H2SO4"):
				params["u_cloudCoverage"] = clampf(pressure_ratio * 0.5, 0.2, 0.95)
				params["u_cloudColor"] = Color(0.9, 0.85, 0.7)
			elif atmo.composition.has("CO2") and pressure_ratio > 0.5:
				params["u_cloudCoverage"] = clampf(pressure_ratio * 0.1, 0.05, 0.2)
				params["u_cloudColor"] = Color(0.95, 0.9, 0.85)
			else:
				params["u_cloudCoverage"] = 0.1

		if pressure_ratio > 2.0:
			params["u_cloudShadow"] = 0.5

		if atmo.greenhouse_factor > 2.0:
			params["u_cloudCoverage"] = maxf(params["u_cloudCoverage"], 0.7)
			params["u_cloudColor"] = Color(0.95, 0.9, 0.8)
	else:
		params["u_cloudCoverage"] = 0.0

	return params


## Lighting parameters for the spatial shader.
static func _get_spatial_lighting_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	params["u_limbDark"] = 0.8
	params["u_terminatorSharp"] = 0.12
	params["u_ambient"] = 0.04

	if body.has_atmosphere():
		var pressure_ratio: float = body.atmosphere.surface_pressure_pa / 101325.0

		if pressure_ratio > 1.5:
			params["u_terminatorSharp"] = 0.2
			params["u_ambient"] = 0.06
		elif pressure_ratio < 0.1:
			params["u_terminatorSharp"] = 0.08
			params["u_ambient"] = 0.03

	return params


## Animation parameters for the spatial shader.
static func _get_spatial_animation_params(body: CelestialBody) -> Dictionary:
	var params: Dictionary = {}

	params["u_rotSpeed"] = 0.06
	params["u_cloudDrift"] = 0.03
	params["u_axialTilt"] = 23.0

	if body.physical:
		params["u_axialTilt"] = clampf(body.physical.axial_tilt_deg, 0.0, 90.0)
		params["u_rotSpeed"] = ShaderParamHelpers.calculate_visual_rotation_speed(body.physical.rotation_period_s)
		params["u_cloudDrift"] = params["u_rotSpeed"] * 0.5

	return params


## Checks if a body is suitable for the terrestrial surface shader.
## @param body: The celestial body to check.
## @return: True if the body should use the terrestrial surface shader.
static func is_terrestrial_suitable(body: CelestialBody) -> bool:
	if not body:
		return false

	if not body.has_surface():
		return false

	# Gas giants are handled separately
	if GasGiantShaderParams.is_gas_giant(body):
		return false

	# Check for obvious non-terrestrial types
	if body.has_surface():
		var st: String = body.surface.surface_type.to_lower() if body.surface.surface_type else ""
		if st in ["gaseous", "gas_giant", "ice_giant"]:
			return false

	# Any other body with a surface should use the terrestrial shader
	return true


## Returns shader parameters for legacy canvas_item terrestrial shader.
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
		for key: String in ring_params:
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
			colors["shallow"] = Vector3(0.4, 0.35, 0.2)
			colors["deep"] = Vector3(0.2, 0.15, 0.1)
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
	var surface_type_lower: String = surface.surface_type.to_lower() if surface.surface_type else ""

	match surface_type_lower:
		"rocky", "rocky_cold", "cratered":
			colors["low"] = Vector3(0.35, 0.32, 0.28)
			colors["mid"] = Vector3(0.45, 0.42, 0.38)
			colors["high"] = Vector3(0.55, 0.52, 0.48)
			colors["peak"] = Vector3(0.7, 0.68, 0.65)
		"desert":
			colors["low"] = Vector3(0.6, 0.45, 0.3)
			colors["mid"] = Vector3(0.7, 0.55, 0.35)
			colors["high"] = Vector3(0.75, 0.6, 0.4)
			colors["peak"] = Vector3(0.85, 0.75, 0.6)
		"volcanic", "molten":
			colors["low"] = Vector3(0.15, 0.12, 0.1)
			colors["mid"] = Vector3(0.25, 0.2, 0.15)
			colors["high"] = Vector3(0.4, 0.25, 0.15)
			colors["peak"] = Vector3(0.6, 0.35, 0.2)
		"icy", "frozen", "icy_rocky":
			colors["low"] = Vector3(0.7, 0.75, 0.8)
			colors["mid"] = Vector3(0.8, 0.85, 0.9)
			colors["high"] = Vector3(0.85, 0.88, 0.92)
			colors["peak"] = Vector3(0.95, 0.97, 1.0)
		"oceanic", "continental", "temperate", "earthlike", "habitable":
			colors["low"] = Vector3(0.2, 0.35, 0.15)
			colors["mid"] = Vector3(0.25, 0.45, 0.2)
			colors["high"] = Vector3(0.4, 0.45, 0.35)
			colors["peak"] = Vector3(0.6, 0.58, 0.55)
		"tundra", "cold", "frozen_continental", "subarctic", "arctic":
			colors["low"] = Vector3(0.40, 0.45, 0.38)
			colors["mid"] = Vector3(0.50, 0.55, 0.48)
			colors["high"] = Vector3(0.62, 0.66, 0.62)
			colors["peak"] = Vector3(0.82, 0.86, 0.90)
		"arid":
			colors["low"] = Vector3(0.55, 0.42, 0.28)
			colors["mid"] = Vector3(0.65, 0.50, 0.32)
			colors["high"] = Vector3(0.72, 0.58, 0.38)
			colors["peak"] = Vector3(0.82, 0.72, 0.55)
		"barren":
			colors["low"] = Vector3(0.32, 0.30, 0.28)
			colors["mid"] = Vector3(0.42, 0.40, 0.37)
			colors["high"] = Vector3(0.52, 0.50, 0.47)
			colors["peak"] = Vector3(0.65, 0.63, 0.60)
		"", _:
			# Empty or unknown surface type: derive colors from temperature
			var temp_k: float = surface.temperature_k
			var warmth: float = clampf((temp_k - 150.0) / 500.0, 0.0, 1.0)
			colors["low"] = _lerp_vec3(Vector3(0.35, 0.38, 0.42), Vector3(0.50, 0.38, 0.25), warmth)
			colors["mid"] = _lerp_vec3(Vector3(0.45, 0.48, 0.52), Vector3(0.60, 0.48, 0.32), warmth)
			colors["high"] = _lerp_vec3(Vector3(0.55, 0.58, 0.60), Vector3(0.68, 0.56, 0.40), warmth)
			colors["peak"] = _lerp_vec3(Vector3(0.70, 0.72, 0.75), Vector3(0.80, 0.70, 0.55), warmth)

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
