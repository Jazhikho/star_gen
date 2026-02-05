## Factory for creating materials for celestial body rendering.
## Creates appropriate materials based on body type and properties.
## Includes material caching for performance.
class_name MaterialFactory
extends RefCounted

const _color_utils: GDScript = preload("res://src/app/rendering/ColorUtils.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _star_surface_shader: Shader = preload("res://src/app/rendering/shaders/star_surface.gdshader")
const _star_atmosphere_shader: Shader = preload("res://src/app/rendering/shaders/star_atmosphere.gdshader")
const _terrestrial_shader: Shader = preload("res://src/app/rendering/shaders/planet_terrestrial_surface.gdshader")
const _gas_giant_surface_shader: Shader = preload("res://src/app/rendering/shaders/planet_gas_giant_surface.gdshader")
const _ring_system_shader: Shader = preload("res://src/app/rendering/shaders/ring_system.gdshader")
const _atmosphere_rim_shader: Shader = preload("res://src/app/rendering/shaders/atmosphere_rim.gdshader")

## Cache for reusable materials (key: cache key string, value: Material)
static var _material_cache: Dictionary = {}

## Cache key for a body (for reuse of identical materials)
static var _cache_key_history: Dictionary = {}


## Creates a material for a celestial body.
## @param body: The celestial body.
## @return: Appropriate material for rendering.
static func create_body_material(body: CelestialBody) -> Material:
	if not body:
		return _create_default_material()
	
	var cache_key: String = _generate_cache_key(body)
	
	# Check cache first
	if _material_cache.has(cache_key):
		return _material_cache[cache_key]
	
	# Create new material based on type
	var material: Material
	match body.type:
		CelestialType.Type.STAR:
			material = _create_star_material(body)
		CelestialType.Type.PLANET:
			material = _create_planet_material(body)
		CelestialType.Type.MOON:
			material = _create_moon_material(body)
		CelestialType.Type.ASTEROID:
			material = _create_asteroid_material(body)
		_:
			material = _create_default_material()
	
	# Cache the material
	_material_cache[cache_key] = material
	_cache_key_history[body] = cache_key
	
	return material


## Generates a cache key for a body (for material reuse).
static func _generate_cache_key(body: CelestialBody) -> String:
	var key_parts: Array[String] = [CelestialType.type_to_string(body.type)]
	
	match body.type:
		CelestialType.Type.STAR:
			if body.has_stellar():
				var temp: float = body.stellar.effective_temperature_k
				var lum: float = body.stellar.luminosity_watts
				var age: float = body.stellar.age_years
				var lum_rounded: int = int(lum / 1e24)
				var age_bin: int = int(age / 1e9)
				var seed_val: int = 0
				if body.provenance:
					seed_val = body.provenance.generation_seed % 1000
				key_parts.append("temp_%d_lum_%d_age_%d_seed_%d" % [int(temp), lum_rounded, age_bin, seed_val])
			else:
				key_parts.append("default")
		
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			if body.has_surface():
				var surface_type: String = body.surface.surface_type
				var albedo: float = body.surface.albedo
				var seed_val: int = 0
				if body.provenance:
					seed_val = body.provenance.generation_seed % 1000
				key_parts.append("%s_albedo_%.2f_seed_%d" % [surface_type, albedo, seed_val])
			else:
				key_parts.append("default")

			# Check if gas giant
			if _is_gas_giant(body):
				var mass_earth: float = body.physical.mass_kg / 1.989e30 * 333000.0 # Rough estimate
				key_parts.append("gas_%d" % int(mass_earth))
			elif TerrestrialShaderParams.is_terrestrial_suitable(body):
				key_parts.append("terrestrial")

		CelestialType.Type.ASTEROID:
			if body.has_surface():
				var surface_type: String = body.surface.surface_type
				key_parts.append(surface_type)
			else:
				key_parts.append("default")
	
	return "_".join(key_parts)


## Creates a material for a star using the advanced surface shader.
static func _create_star_material(body: CelestialBody) -> ShaderMaterial:
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _star_surface_shader

	var params: Dictionary = StarShaderParams.get_star_shader_params(body)

	material.set_shader_parameter("u_star_color", params.get("u_star_color", Color.WHITE))
	material.set_shader_parameter("u_temperature", params.get("u_temperature", 5778.0))
	material.set_shader_parameter("u_luminosity", params.get("u_luminosity", 1.0))
	material.set_shader_parameter("u_limbDark", params.get("u_limbDark", 0.6))

	material.set_shader_parameter("u_granScale", params.get("u_granScale", 30.0))
	material.set_shader_parameter("u_granContrast", params.get("u_granContrast", 0.35))
	material.set_shader_parameter("u_granTurb", params.get("u_granTurb", 0.4))
	material.set_shader_parameter("u_granFlow", params.get("u_granFlow", 0.08))
	material.set_shader_parameter("u_superGranScale", params.get("u_superGranScale", 6.0))
	material.set_shader_parameter("u_superGranStr", params.get("u_superGranStr", 0.15))

	material.set_shader_parameter("u_spotCount", params.get("u_spotCount", 5.0))
	material.set_shader_parameter("u_spotSize", params.get("u_spotSize", 0.06))
	material.set_shader_parameter("u_penumbra", params.get("u_penumbra", 2.0))
	material.set_shader_parameter("u_spotDark", params.get("u_spotDark", 0.35))

	material.set_shader_parameter("u_chromoThick", params.get("u_chromoThick", 0.015))
	material.set_shader_parameter("u_chromoIntensity", params.get("u_chromoIntensity", 0.8))
	material.set_shader_parameter("u_chromoShift", params.get("u_chromoShift", 0.5))

	material.set_shader_parameter("u_coronaExtent", params.get("u_coronaExtent", 0.3))
	material.set_shader_parameter("u_coronaBright", params.get("u_coronaBright", 0.5))
	material.set_shader_parameter("u_coronaStreams", params.get("u_coronaStreams", 8.0))
	material.set_shader_parameter("u_coronaLength", params.get("u_coronaLength", 0.5))
	material.set_shader_parameter("u_coronaAsym", params.get("u_coronaAsym", 0.3))

	material.set_shader_parameter("u_promCount", params.get("u_promCount", 3.0))
	material.set_shader_parameter("u_promHeight", params.get("u_promHeight", 0.12))
	material.set_shader_parameter("u_promGlow", params.get("u_promGlow", 0.8))
	material.set_shader_parameter("u_flareIntensity", params.get("u_flareIntensity", 0.2))

	material.set_shader_parameter("u_rotSpeed", params.get("u_rotSpeed", 0.05))
	material.set_shader_parameter("u_bloomRadius", params.get("u_bloomRadius", 0.15))
	material.set_shader_parameter("u_bloomIntensity", params.get("u_bloomIntensity", 0.6))
	material.set_shader_parameter("u_spikeCount", params.get("u_spikeCount", 4.0))
	material.set_shader_parameter("u_spikeLength", params.get("u_spikeLength", 0.3))
	material.set_shader_parameter("u_spikeBright", params.get("u_spikeBright", 0.25))
	material.set_shader_parameter("u_seed", params.get("u_seed", 0.0))

	return material


## Creates the star atmosphere material (corona, prominences, etc.).
## @param body: The star celestial body.
## @return: Atmosphere material, or null if not a star.
static func create_star_atmosphere_material(body: CelestialBody) -> ShaderMaterial:
	if not body or body.type != CelestialType.Type.STAR:
		return null

	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _star_atmosphere_shader

	var params: Dictionary = StarShaderParams.get_star_shader_params(body)

	material.set_shader_parameter("u_star_color", params.get("u_star_color", Color.WHITE))
	material.set_shader_parameter("u_luminosity", params.get("u_luminosity", 1.0))

	material.set_shader_parameter("u_chromoThick", params.get("u_chromoThick", 0.015))
	material.set_shader_parameter("u_chromoIntensity", params.get("u_chromoIntensity", 0.8))
	material.set_shader_parameter("u_chromoShift", params.get("u_chromoShift", 0.5))

	material.set_shader_parameter("u_coronaExtent", params.get("u_coronaExtent", 0.3))
	material.set_shader_parameter("u_coronaBright", params.get("u_coronaBright", 0.5))
	material.set_shader_parameter("u_coronaStreams", params.get("u_coronaStreams", 8.0))
	material.set_shader_parameter("u_coronaLength", params.get("u_coronaLength", 0.5))
	material.set_shader_parameter("u_coronaAsym", params.get("u_coronaAsym", 0.3))

	material.set_shader_parameter("u_promCount", params.get("u_promCount", 3.0))
	material.set_shader_parameter("u_promHeight", params.get("u_promHeight", 0.12))
	material.set_shader_parameter("u_promGlow", params.get("u_promGlow", 0.8))
	material.set_shader_parameter("u_flareIntensity", params.get("u_flareIntensity", 0.2))

	material.set_shader_parameter("u_bloomRadius", params.get("u_bloomRadius", 0.15))
	material.set_shader_parameter("u_bloomIntensity", params.get("u_bloomIntensity", 0.6))
	material.set_shader_parameter("u_spikeCount", params.get("u_spikeCount", 4.0))
	material.set_shader_parameter("u_spikeLength", params.get("u_spikeLength", 0.3))
	material.set_shader_parameter("u_spikeBright", params.get("u_spikeBright", 0.25))
	material.set_shader_parameter("u_seed", params.get("u_seed", 0.0))

	material.set_shader_parameter("u_star_radius_ratio", 0.8)

	return material


## Creates a material for a planet.
static func _create_planet_material(body: CelestialBody) -> Material:
	# Check if gas giant
	if _is_gas_giant(body):
		return _create_gas_giant_material(body)

	# Check if suitable for terrestrial shader
	if TerrestrialShaderParams.is_terrestrial_suitable(body):
		return _create_terrestrial_material(body)

	# Fall back to simple rocky material
	return _create_rocky_material(body)


## Creates a material for a moon.
static func _create_moon_material(body: CelestialBody) -> Material:
	# Check if suitable for terrestrial shader (large moons with atmospheres)
	if TerrestrialShaderParams.is_terrestrial_suitable(body):
		return _create_terrestrial_material(body)

	# Icy moons
	if body.has_surface() and body.surface.has_cryosphere():
		if body.surface.cryosphere.polar_cap_coverage > 0.5:
			return _create_icy_material(body)

	return _create_rocky_material(body)


## Creates a terrestrial planet material with full shader features.
static func _create_terrestrial_material(body: CelestialBody) -> ShaderMaterial:
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _terrestrial_shader

	var params: Dictionary = TerrestrialShaderParams.get_params(body)

	for param_name in params:
		material.set_shader_parameter(param_name, params[param_name])

	return material


## Creates a material for an asteroid.
static func _create_asteroid_material(body: CelestialBody) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	
	# Very low albedo for most asteroids
	if body.has_surface():
		var surface_type: String = body.surface.surface_type
		var composition: Dictionary = body.surface.surface_composition
		var albedo: float = body.surface.albedo
		
		var asteroid_color: Color = ColorUtils.asteroid_to_color(surface_type, composition)
		# Adjust for actual albedo
		material.albedo_color = asteroid_color * (albedo * 2.0)
		material.roughness = 0.95 # Very rough
		material.metallic = 0.1
		
		# Metallic asteroids have some metallic sheen
		if surface_type == "metallic":
			material.metallic = 0.4
			material.metallic_specular = 0.5
	else:
		material.albedo_color = Color(0.3, 0.3, 0.3)
		material.roughness = 0.9
	
	# Add normal mapping for surface detail (optional enhancement)
	material.normal_enabled = true
	material.normal_scale = 2.0
	
	return material


## Creates a gas giant material with bands, storms, and atmosphere.
static func _create_gas_giant_material(body: CelestialBody) -> ShaderMaterial:
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _gas_giant_surface_shader

	var params: Dictionary = GasGiantShaderParams.get_params(body)
	for param_name in params:
		material.set_shader_parameter(param_name, params[param_name])

	return material


## Creates a rocky/terrestrial material.
static func _create_rocky_material(body: CelestialBody) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	
	if body.has_surface():
		# Base color from surface type
		var surface_color: Color = ColorUtils.surface_to_color(
			body.surface.surface_type,
			body.surface.surface_composition,
			body.surface.albedo
		)
		
		material.albedo_color = surface_color
		
		# Roughness based on terrain
		if body.surface.has_terrain():
			material.roughness = clampf(0.5 + body.surface.terrain.roughness * 0.4, 0.3, 0.95)
		else:
			material.roughness = 0.7
		
		# Metallic for certain compositions
		material.metallic = 0.0
		if body.surface.surface_composition.has("iron"):
			var iron_amount: float = body.surface.surface_composition["iron"] as float
			material.metallic = clampf(iron_amount * 0.5, 0.0, 0.3)
		
		# Add emission for hot surfaces
		if body.surface.temperature_k > 700:
			material.emission_enabled = true
			var emission_color: Color = ColorUtils.temperature_to_blackbody_color(body.surface.temperature_k)
			material.emission = emission_color * 0.5
			material.emission_energy_multiplier = (body.surface.temperature_k - 700.0) / 1000.0
	else:
		# Default rocky appearance
		material.albedo_color = Color(0.5, 0.45, 0.4)
		material.roughness = 0.8
	
	return material


## Creates an icy material.
static func _create_icy_material(body: CelestialBody) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	
	# Base ice color (slightly blue-white)
	material.albedo_color = Color(0.9, 0.95, 1.0)
	
	if body.has_surface():
		# High albedo for ice
		material.albedo_color *= body.surface.albedo * 1.5
		
		# Smooth ice
		material.roughness = 0.2
		
		# Slight subsurface scattering look
		material.rim_enabled = true
		material.rim = 1.0
		material.rim_tint = 0.5
	else:
		material.roughness = 0.3
	
	# Add some specularity
	material.metallic = 0.1
	
	return material


## Creates a default gray material.
static func _create_default_material() -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = Color(0.5, 0.5, 0.5)
	material.roughness = 0.5
	return material


## Creates an atmosphere rim material.
## @param body: The celestial body with atmosphere.
## @return: Material for atmosphere effect, or null if no atmosphere.
static func create_atmosphere_material(body: CelestialBody) -> ShaderMaterial:
	if not AtmosphereShaderParams.should_render_atmosphere(body):
		return null

	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _atmosphere_rim_shader

	var params: Dictionary = AtmosphereShaderParams.get_params(body)
	for param_name in params:
		material.set_shader_parameter(param_name, params[param_name])

	return material


## Creates a ring material for the entire ring system.
## @param ring_system: The ring system properties.
## @param body: The parent body (for context).
## @return: Material for the ring system.
static func create_ring_system_material(ring_system: RingSystemProps, body: CelestialBody = null) -> ShaderMaterial:
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _ring_system_shader

	var params: Dictionary = RingShaderParams.get_params(ring_system, body)
	for param_name in params:
		material.set_shader_parameter(param_name, params[param_name])

	return material


## Creates a ring material for a single band (legacy support).
## @param band: The ring band.
## @return: Material for the ring band.
## @param band: The ring band.
## @return: Material for the ring band.
static func create_ring_material(band: RingBand) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	
	var ring_color: Color = ColorUtils.ring_to_color(band.composition, band.optical_depth)
	
	material.albedo_color = Color(ring_color.r, ring_color.g, ring_color.b, 1.0)
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	material.albedo_color.a = ring_color.a
	
	# Rings are rough (particulate)
	material.roughness = 0.9
	
	# Cull disabled so rings visible from both sides
	material.cull_mode = BaseMaterial3D.CULL_DISABLED
	
	return material


## Checks if a body is a gas giant.
static func _is_gas_giant(body: CelestialBody) -> bool:
	return GasGiantShaderParams.is_gas_giant(body)


## Gets the noise texture for shader effects.
static func _get_noise_texture() -> Texture2D:
	# Try to load noise texture, return null if not available
	var noise_texture: Texture2D = load("res://src/app/rendering/textures/noise.tres") as Texture2D
	return noise_texture


## Clears the material cache (useful for testing or memory management).
static func clear_cache() -> void:
	_material_cache.clear()
	_cache_key_history.clear()
