## Manages 3D rendering of celestial bodies.
## Creates and updates meshes, materials, and effects for body visualization.
class_name BodyRenderer
extends Node3D

const _material_factory := preload("res://src/app/rendering/MaterialFactory.gd")
const _color_utils := preload("res://src/app/rendering/ColorUtils.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _ring_band := preload("res://src/domain/celestial/components/RingBand.gd")

## The main body mesh
@onready var body_mesh: MeshInstance3D = $BodyMesh

## Atmosphere effect mesh (slightly larger sphere)
@onready var atmosphere_mesh: MeshInstance3D = $AtmosphereMesh

## Light source for stars
@onready var star_light: OmniLight3D = $StarLight

## Ring system node
@onready var ring_system_node: Node3D = $RingSystem

## Current body being rendered
var current_body: CelestialBody = null

## Display scale factor
var display_scale: float = 1.0


func _ready() -> void:
	# Create child nodes if they don't exist
	_ensure_nodes_exist()
	
	# Hide everything initially
	clear()


## Ensures all required child nodes exist.
func _ensure_nodes_exist() -> void:
	if not has_node("BodyMesh"):
		body_mesh = MeshInstance3D.new()
		body_mesh.name = "BodyMesh"
		body_mesh.mesh = SphereMesh.new()
		add_child(body_mesh)
	else:
		body_mesh = $BodyMesh
	
	if not has_node("AtmosphereMesh"):
		atmosphere_mesh = MeshInstance3D.new()
		atmosphere_mesh.name = "AtmosphereMesh"
		var atmo_sphere: SphereMesh = SphereMesh.new()
		atmo_sphere.radial_segments = 32
		atmo_sphere.rings = 16
		atmosphere_mesh.mesh = atmo_sphere
		atmosphere_mesh.visible = false
		add_child(atmosphere_mesh)
	else:
		atmosphere_mesh = $AtmosphereMesh
	
	if not has_node("StarLight"):
		star_light = OmniLight3D.new()
		star_light.name = "StarLight"
		star_light.visible = false
		star_light.shadow_enabled = false
		add_child(star_light)
	else:
		star_light = $StarLight
	
	if not has_node("RingSystem"):
		ring_system_node = Node3D.new()
		ring_system_node.name = "RingSystem"
		add_child(ring_system_node)
	else:
		ring_system_node = $RingSystem


## Renders a celestial body.
## @param body: The body to render.
## @param scale_factor: Display scale factor.
func render_body(body: CelestialBody, scale_factor: float = 1.0) -> void:
	if not body:
		clear()
		return
	
	current_body = body
	display_scale = scale_factor
	
	# Update main body mesh
	_update_body_mesh()
	
	# Update star light if this is a star
	_update_star_light()
	
	# Update atmosphere if present
	_update_atmosphere()
	
	# Update ring system if present
	_update_ring_system()
	
	# Apply axial tilt
	_apply_axial_tilt()


## Clears the renderer.
func clear() -> void:
	current_body = null
	
	if body_mesh:
		body_mesh.visible = false
	
	if atmosphere_mesh:
		atmosphere_mesh.visible = false
	
	if star_light:
		star_light.visible = false
	
	_clear_ring_system()


## Updates the main body mesh.
func _update_body_mesh() -> void:
	if not body_mesh or not current_body:
		return
	
	# Create or update sphere mesh
	var sphere: SphereMesh
	if body_mesh.mesh is SphereMesh:
		sphere = body_mesh.mesh as SphereMesh
	else:
		sphere = SphereMesh.new()
		body_mesh.mesh = sphere
	
	# Configure sphere detail based on body type
	match current_body.type:
		CelestialType.Type.STAR:
			sphere.radial_segments = 64
			sphere.rings = 32
		CelestialType.Type.PLANET:
			sphere.radial_segments = 48
			sphere.rings = 24
		CelestialType.Type.MOON:
			sphere.radial_segments = 32
			sphere.rings = 16
		CelestialType.Type.ASTEROID:
			sphere.radial_segments = 24
			sphere.rings = 12
	
	# Apply oblateness (flattening)
	var oblateness: float = current_body.physical.oblateness
	var scale_y: float = 1.0 - oblateness
	body_mesh.scale = Vector3(display_scale, display_scale * scale_y, display_scale)
	
	# Create and apply material
	var material: Material = MaterialFactory.create_body_material(current_body)
	body_mesh.material_override = material
	
	body_mesh.visible = true


## Updates the star light source.
func _update_star_light() -> void:
	if not star_light or not current_body:
		if star_light:
			star_light.visible = false
		return
	
	# Only stars emit light
	if current_body.type != CelestialType.Type.STAR:
		star_light.visible = false
		return
	
	# Get temperature for color
	var temperature_k: float = 5778.0
	if current_body.has_stellar():
		temperature_k = current_body.stellar.effective_temperature_k
	
	var star_color: Color = ColorUtils.temperature_to_blackbody_color(temperature_k)
	
	# Configure light
	star_light.light_color = star_color
	
	# Energy based on temperature and luminosity (logarithmic scaling)
	var energy: float = 2.0
	if current_body.has_stellar():
		# Scale energy with luminosity (logarithmic)
		var luminosity_solar: float = current_body.stellar.luminosity_watts / 3.828e26
		energy = 1.0 + log(maxf(luminosity_solar, 0.01)) / log(10.0) * 0.5
		energy = clampf(energy, 0.5, 8.0)
	
	star_light.light_energy = energy
	
	# Range based on display scale
	star_light.omni_range = display_scale * 20.0
	
	star_light.visible = true


## Updates the atmosphere effect.
func _update_atmosphere() -> void:
	if not atmosphere_mesh or not current_body:
		atmosphere_mesh.visible = false
		return
	
	# Only planets and large moons can have visible atmospheres
	if not current_body.has_atmosphere():
		atmosphere_mesh.visible = false
		return
	
	# Check if atmosphere is substantial enough to render
	var pressure: float = current_body.atmosphere.surface_pressure_pa
	if pressure < 100.0:  # Less than 100 Pa = too thin to see
		atmosphere_mesh.visible = false
		return
	
	# Create atmosphere material
	var atmo_material: ShaderMaterial = MaterialFactory.create_atmosphere_material(current_body)
	if not atmo_material:
		atmosphere_mesh.visible = false
		return
	
	# Base atmosphere scale slightly larger than body
	var base_atmo_scale: float = 1.02
	
	# Thicker atmosphere = larger visible shell
	var pressure_factor: float = clampf(pressure / 101325.0, 0.1, 3.0)
	base_atmo_scale *= (1.0 + pressure_factor * 0.03)
	
	# Apply oblateness to atmosphere (match body shape)
	var oblateness: float = current_body.physical.oblateness
	var scale_y: float = 1.0 - oblateness
	
	atmosphere_mesh.scale = Vector3(
		display_scale * base_atmo_scale,
		display_scale * base_atmo_scale * scale_y,
		display_scale * base_atmo_scale
	)
	
	atmosphere_mesh.material_override = atmo_material
	atmosphere_mesh.visible = true


## Updates the ring system.
func _update_ring_system() -> void:
	_clear_ring_system()
	
	if not current_body or not current_body.has_ring_system():
		return
	
	var rings: RingSystemProps = current_body.ring_system
	var planet_radius_m: float = current_body.physical.radius_m
	
	if planet_radius_m <= 0.0:
		return
	
	# Reset ring system rotation before adding bands
	ring_system_node.rotation = Vector3.ZERO
	
	# Create a mesh for each ring band
	for i in range(rings.get_band_count()):
		var band: RingBand = rings.get_band(i)
		_create_ring_band_mesh(band, planet_radius_m)
	
	# Rings are in the equatorial plane of the planet
	# The ring system will inherit the body's axial tilt from _apply_axial_tilt()
	# Ring inclination is a small additional tilt relative to the equator
	var ring_inclination: float = rings.inclination_deg
	
	# Apply ring inclination (around X axis, after axial tilt is applied)
	# This is relative to the equatorial plane, so it's added to the base tilt
	var axial_tilt: float = current_body.physical.axial_tilt_deg
	ring_system_node.rotation_degrees = Vector3(
		ring_inclination,  # Ring's own inclination relative to equator
		0.0,
		axial_tilt         # Match body's axial tilt (equatorial plane)
	)


## Creates a mesh for a single ring band.
func _create_ring_band_mesh(band: RingBand, planet_radius_m: float) -> void:
	# Calculate ring dimensions relative to planet
	var inner_ratio: float = band.inner_radius_m / planet_radius_m
	var outer_ratio: float = band.outer_radius_m / planet_radius_m
	
	# Scale to display units
	var inner_radius: float = inner_ratio * display_scale
	var outer_radius: float = outer_ratio * display_scale
	
	# Create a torus-like mesh using an ImmediateMesh or a flattened torus
	# For simplicity, use a custom ring mesh
	var ring_mesh: MeshInstance3D = MeshInstance3D.new()
	ring_mesh.name = "RingBand_" + band.name
	ring_mesh.mesh = _create_ring_mesh(inner_radius, outer_radius)
	ring_mesh.material_override = MaterialFactory.create_ring_material(band)
	
	ring_system_node.add_child(ring_mesh)


## Creates a flat ring mesh (annulus).
func _create_ring_mesh(inner_radius: float, outer_radius: float, segments: int = 64) -> ArrayMesh:
	var mesh: ArrayMesh = ArrayMesh.new()
	var vertices: PackedVector3Array = PackedVector3Array()
	var uvs: PackedVector2Array = PackedVector2Array()
	var indices: PackedInt32Array = PackedInt32Array()
	
	# Create ring vertices
	for i in range(segments + 1):
		var angle: float = (float(i) / float(segments)) * TAU
		var cos_a: float = cos(angle)
		var sin_a: float = sin(angle)
		
		# Inner vertex
		vertices.append(Vector3(cos_a * inner_radius, 0.0, sin_a * inner_radius))
		uvs.append(Vector2(float(i) / float(segments), 0.0))
		
		# Outer vertex
		vertices.append(Vector3(cos_a * outer_radius, 0.0, sin_a * outer_radius))
		uvs.append(Vector2(float(i) / float(segments), 1.0))
	
	# Create triangles
	for i in range(segments):
		var base: int = i * 2
		# First triangle
		indices.append(base)
		indices.append(base + 1)
		indices.append(base + 2)
		# Second triangle
		indices.append(base + 1)
		indices.append(base + 3)
		indices.append(base + 2)
	
	# Build mesh
	var arrays: Array = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_TEX_UV] = uvs
	arrays[Mesh.ARRAY_INDEX] = indices
	
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	
	return mesh


## Clears the ring system.
func _clear_ring_system() -> void:
	if not ring_system_node:
		return
	
	for child in ring_system_node.get_children():
		child.queue_free()


## Applies axial tilt to the body.
## Note: In Godot's coordinate system (Y-up):
## - Rotation around Y = spinning on axis (daily rotation)
## - Rotation around Z = tilting the rotation axis left/right (axial tilt)
## - Rotation around X = tilting forward/back (for viewing angles)
func _apply_axial_tilt() -> void:
	if not current_body:
		return
	
	var tilt: float = current_body.physical.axial_tilt_deg
	
	# Apply axial tilt around Z axis (tilts the rotation axis)
	if body_mesh:
		body_mesh.rotation_degrees.z = tilt
	
	# Atmosphere should match body tilt
	if atmosphere_mesh:
		atmosphere_mesh.rotation_degrees.z = tilt
	
	# Ring system tilt is handled in _update_ring_system() to include ring inclination


## Rotates the body for animation.
## @param delta: Time delta.
## @param speed_multiplier: Speed of rotation (1.0 = realistic would be very slow).
func rotate_body(delta: float, speed_multiplier: float = 1.0) -> void:
	if not current_body or not body_mesh:
		return
	
	# Rotation period in seconds (use a reasonable visual speed)
	var period: float = absf(current_body.physical.rotation_period_s)
	if period < 1.0:
		period = 86400.0  # Default to 1 day if invalid
	
	# Scale to reasonable visual speed (complete rotation in ~10 seconds at 1x)
	var rotation_speed: float = (TAU / 10.0) * speed_multiplier
	
	# Retrograde rotation
	if current_body.physical.rotation_period_s < 0:
		rotation_speed = -rotation_speed
	
	# Apply rotation around the local Y axis (after tilt has been applied)
	# This rotates around the tilted axis, not the world Y axis
	body_mesh.rotate_object_local(Vector3.UP, rotation_speed * delta)
	
	# Atmosphere rotates with body
	if atmosphere_mesh and atmosphere_mesh.visible:
		atmosphere_mesh.rotate_object_local(Vector3.UP, rotation_speed * delta)
	
	# Rings rotate with body (they're in the equatorial plane)
	# Only rotate if we have rings
	if ring_system_node and current_body.has_ring_system():
		ring_system_node.rotate_object_local(Vector3.UP, rotation_speed * delta)
