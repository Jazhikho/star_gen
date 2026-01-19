## Factory for creating materials for celestial body rendering.
## Creates appropriate materials based on body type and properties.
## Includes material caching for performance.
class_name MaterialFactory
extends RefCounted

const _color_utils := preload("res://src/app/rendering/ColorUtils.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _star_shader := preload("res://src/app/rendering/shaders/star.gdshader")
const _gas_giant_shader := preload("res://src/app/rendering/shaders/gas_giant.gdshader")

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
				# Round luminosity to avoid precision issues in cache key
				var lum_rounded: int = int(lum / 1e24)  # Round to nearest 1e24
				key_parts.append("temp_%d_lum_%d" % [int(temp), lum_rounded])
			else:
				key_parts.append("default")
		
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			if body.has_surface():
				var surface_type: String = body.surface.surface_type
				var albedo: float = body.surface.albedo
				key_parts.append("%s_albedo_%.2f" % [surface_type, albedo])
			else:
				key_parts.append("default")
			
			# Check if gas giant
			if _is_gas_giant(body):
				var mass_earth: float = body.physical.mass_kg / 1.989e30 * 333000.0  # Rough estimate
				key_parts.append("gas_%d" % int(mass_earth))
		
		CelestialType.Type.ASTEROID:
			if body.has_surface():
				var surface_type: String = body.surface.surface_type
				key_parts.append(surface_type)
			else:
				key_parts.append("default")
	
	return "_".join(key_parts)


## Creates a material for a star.
static func _create_star_material(body: CelestialBody) -> ShaderMaterial:
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _star_shader
	
	# Get temperature for blackbody color
	var temperature_k: float = 5778.0  # Default solar temperature
	var luminosity_w: float = 3.828e26  # Solar luminosity
	
	if body.has_stellar():
		temperature_k = body.stellar.effective_temperature_k
		luminosity_w = body.stellar.luminosity_watts
	
	var star_color: Color = ColorUtils.temperature_to_blackbody_color(temperature_k)
	
	material.set_shader_parameter("star_color", star_color)
	material.set_shader_parameter("temperature", temperature_k)
	
	# Calculate emission intensity based on luminosity
	var luminosity_solar: float = luminosity_w / 3.828e26
	var intensity: float = clampf(luminosity_solar, 0.1, 10.0)
	material.set_shader_parameter("emission_intensity", intensity)
	
	# Add corona parameters
	material.set_shader_parameter("corona_size", 1.2)
	material.set_shader_parameter("corona_intensity", intensity * 0.3)
	
	# Set noise texture for surface variation (if available)
	var noise_texture: Texture2D = _get_noise_texture()
	if noise_texture:
		material.set_shader_parameter("noise_texture", noise_texture)
	
	return material


## Creates a material for a planet.
static func _create_planet_material(body: CelestialBody) -> Material:
	# Check if gas giant
	if _is_gas_giant(body):
		return _create_gas_giant_material(body)
	
	# Rocky planet
	return _create_rocky_material(body)


## Creates a material for a moon.
static func _create_moon_material(body: CelestialBody) -> Material:
	# Similar to planets but often more icy/cratered
	if body.has_surface() and body.surface.has_cryosphere():
		if body.surface.cryosphere.polar_cap_coverage > 0.5:
			return _create_icy_material(body)
	
	return _create_rocky_material(body)


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
		material.roughness = 0.95  # Very rough
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


## Creates a gas giant material with bands.
static func _create_gas_giant_material(body: CelestialBody) -> ShaderMaterial:
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _gas_giant_shader
	
	# Determine gas giant colors based on temperature
	var base_color: Color = Color(0.8, 0.7, 0.5)  # Jupiter-like
	var band_color: Color = Color(0.9, 0.85, 0.7)
	var storm_color: Color = Color(0.6, 0.4, 0.3)
	
	if body.has_surface():
		var temp: float = body.surface.temperature_k
		if temp < 100:  # Very cold - Neptune-like
			base_color = Color(0.3, 0.5, 0.8)
			band_color = Color(0.4, 0.6, 0.9)
		elif temp < 150:  # Cold - Uranus-like
			base_color = Color(0.4, 0.7, 0.8)
			band_color = Color(0.5, 0.8, 0.9)
	
	material.set_shader_parameter("base_color", base_color)
	material.set_shader_parameter("band_color", band_color)
	material.set_shader_parameter("storm_color", storm_color)
	material.set_shader_parameter("band_count", randi_range(8, 20))
	material.set_shader_parameter("turbulence", 0.3)
	material.set_shader_parameter("storm_intensity", randf_range(0.0, 0.5))
	material.set_shader_parameter("rotation_speed", 0.1)
	
	# Set noise texture for turbulence
	var noise_texture: Texture2D = _get_noise_texture()
	if noise_texture:
		material.set_shader_parameter("noise_texture", noise_texture)
	
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
	if not body.has_atmosphere():
		return null
	
	var atmo: AtmosphereProps = body.atmosphere
	
	# Get base sky color from composition
	var sky_color: Color = ColorUtils.atmosphere_to_sky_color(atmo.composition)
	
	# Apply greenhouse effect tint
	# Higher greenhouse = more orange/yellow tint (trapped heat)
	var greenhouse_factor: float = atmo.greenhouse_factor
	if greenhouse_factor > 1.1:
		# Lerp toward orange/yellow for strong greenhouse
		var greenhouse_strength: float = clampf((greenhouse_factor - 1.0) / 2.0, 0.0, 1.0)
		var greenhouse_tint: Color = Color(0.9, 0.7, 0.4)  # Warm orange
		sky_color = sky_color.lerp(greenhouse_tint, greenhouse_strength * 0.4)
	
	# Create shader material for atmosphere rim effect
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = _get_atmosphere_shader()
	
	material.set_shader_parameter("atmosphere_color", sky_color)
	
	# Density affects how visible the atmosphere is
	var density: float = clampf(atmo.surface_pressure_pa / 101325.0, 0.01, 5.0)
	material.set_shader_parameter("atmosphere_density", minf(density, 2.0))
	
	# Scale height affects falloff
	var falloff: float = 3.0  # Default
	if body.physical.radius_m > 0 and atmo.scale_height_m > 0:
		falloff = body.physical.radius_m / atmo.scale_height_m * 0.1
		falloff = clampf(falloff, 1.0, 10.0)
	material.set_shader_parameter("falloff", falloff)
	
	# Pass greenhouse factor for additional visual effect
	var greenhouse_intensity: float = clampf((greenhouse_factor - 1.0) * 0.5, 0.0, 1.0)
	material.set_shader_parameter("greenhouse_intensity", greenhouse_intensity)
	
	return material


## Gets or creates the atmosphere shader.
static func _get_atmosphere_shader() -> Shader:
	# Simple rim-lighting shader for atmosphere effect
	var shader: Shader = Shader.new()
	shader.code = """
shader_type spatial;
render_mode blend_add, depth_draw_opaque, cull_front, unshaded;

uniform vec4 atmosphere_color : source_color = vec4(0.4, 0.6, 0.9, 1.0);
uniform float atmosphere_density : hint_range(0.0, 2.0) = 1.0;
uniform float falloff : hint_range(1.0, 10.0) = 3.0;
uniform float greenhouse_intensity : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	// Rim lighting based on view angle
	float rim = 1.0 - dot(NORMAL, VIEW);
	rim = pow(rim, falloff);
	
	// Base atmosphere color
	vec3 atmo_color = atmosphere_color.rgb;
	
	// Greenhouse effect: add a warm inner glow
	if (greenhouse_intensity > 0.0) {
		// Inner glow (opposite of rim - stronger toward center)
		float inner = dot(NORMAL, VIEW);
		inner = pow(inner, 2.0);
		
		// Warm color for trapped heat
		vec3 heat_color = vec3(1.0, 0.6, 0.3);
		atmo_color = mix(atmo_color, heat_color, inner * greenhouse_intensity * 0.3);
	}
	
	// Apply atmosphere color with density
	ALBEDO = atmo_color;
	ALPHA = rim * atmosphere_color.a * atmosphere_density * 0.5;
}
"""
	return shader


## Creates a ring material.
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
	var mass_earth: float = body.physical.mass_kg / 1.989e30 * 333000.0  # Rough estimate
	return mass_earth > 10.0 and not body.has_surface()


## Gets the noise texture for shader effects.
static func _get_noise_texture() -> Texture2D:
	# Try to load noise texture, return null if not available
	var noise_texture: Texture2D = load("res://src/app/rendering/textures/noise.tres") as Texture2D
	return noise_texture


## Clears the material cache (useful for testing or memory management).
static func clear_cache() -> void:
	_material_cache.clear()
	_cache_key_history.clear()
