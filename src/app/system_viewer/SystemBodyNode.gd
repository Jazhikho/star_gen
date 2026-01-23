## Represents a single celestial body in the solar system viewer.
## Manages the 3D mesh, label, and selection state for one body.
class_name SystemBodyNode
extends Node3D

const _material_factory := preload("res://src/app/rendering/MaterialFactory.gd")
const _color_utils := preload("res://src/app/rendering/ColorUtils.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _system_scale_manager := preload("res://src/app/system_viewer/SystemScaleManager.gd")


## Emitted when this body is clicked/selected.
signal body_selected(body_id: String)

## Emitted when this body is hovered.
signal body_hovered(body_id: String)

## Emitted when hover ends.
signal body_unhovered(body_id: String)

## The celestial body data this node represents.
var body: CelestialBody = null

## Unique body ID for selection tracking.
var body_id: String = ""

## Display radius in viewport units.
var display_radius: float = 0.1

## Whether this body is currently selected.
var is_selected: bool = false

## Whether this body is currently hovered.
var is_hovered: bool = false

## The mesh instance for this body.
var _mesh_instance: MeshInstance3D = null

## The light source (for stars only).
var _star_light: OmniLight3D = null

## The selection indicator (ring around selected body).
var _selection_ring: MeshInstance3D = null

## Collision area for mouse picking.
var _click_area: Area3D = null

## Collision shape for the click area.
var _collision_shape: CollisionShape3D = null


## Initializes the body node with celestial body data.
## @param p_body: The celestial body to represent.
## @param p_display_radius: Display radius in viewport units.
## @param p_position: Position in viewport units.
func setup(p_body: CelestialBody, p_display_radius: float, p_position: Vector3) -> void:
	if p_body == null:
		return
	
	body = p_body
	body_id = p_body.id
	display_radius = p_display_radius
	position = p_position
	
	_create_mesh()
	_create_click_area()
	
	if body.type == CelestialType.Type.STAR:
		_create_star_light()
	
	name = "Body_" + body_id


## Sets the selected state.
## @param selected: Whether this body is selected.
func set_selected(selected: bool) -> void:
	is_selected = selected
	_update_selection_visual()


## Sets the hovered state.
## @param hovered: Whether this body is hovered.
func set_hovered(hovered: bool) -> void:
	if hovered == is_hovered:
		return
	
	is_hovered = hovered
	_update_hover_visual()
	
	if hovered:
		body_hovered.emit(body_id)
	else:
		body_unhovered.emit(body_id)


## Gets the body type.
## @return: CelestialType.Type enum.
func get_body_type() -> CelestialType.Type:
	if body == null:
		return CelestialType.Type.PLANET
	return body.type


## Gets the body display name.
## @return: Name string.
func get_display_name() -> String:
	if body == null:
		return "Unknown"
	return body.name


## Gets the parent body ID (from orbital props).
## @return: Parent ID, or empty string if none.
func get_parent_id() -> String:
	if body == null or not body.has_orbital():
		return ""
	return body.orbital.parent_id


## Updates the body's visual representation.
## Call after changing display_radius or position.
func update_visual() -> void:
	if _mesh_instance:
		_mesh_instance.scale = Vector3.ONE * display_radius * 2.0
	
	if _collision_shape:
		var sphere_shape: SphereShape3D = _collision_shape.shape as SphereShape3D
		if sphere_shape:
			# Make click area slightly larger than visual for easier selection
			sphere_shape.radius = display_radius * 1.5
	
	_update_selection_visual()


## Creates the body mesh with appropriate material.
func _create_mesh() -> void:
	_mesh_instance = MeshInstance3D.new()
	_mesh_instance.name = "Mesh"
	
	var sphere: SphereMesh = SphereMesh.new()
	sphere.radius = 1.0
	sphere.height = 2.0
	
	# Detail based on body type
	match body.type:
		CelestialType.Type.STAR:
			sphere.radial_segments = 32
			sphere.rings = 16
		CelestialType.Type.PLANET:
			sphere.radial_segments = 24
			sphere.rings = 12
		CelestialType.Type.MOON:
			sphere.radial_segments = 16
			sphere.rings = 8
		CelestialType.Type.ASTEROID:
			sphere.radial_segments = 12
			sphere.rings = 6
	
	_mesh_instance.mesh = sphere
	_mesh_instance.scale = Vector3.ONE * display_radius * 2.0
	
	# Apply material
	var material: Material = MaterialFactory.create_body_material(body)
	_mesh_instance.material_override = material
	
	# Disable shadows for system view (too many bodies)
	_mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	
	add_child(_mesh_instance)


## Creates the click detection area.
func _create_click_area() -> void:
	_click_area = Area3D.new()
	_click_area.name = "ClickArea"
	
	_collision_shape = CollisionShape3D.new()
	_collision_shape.name = "Shape"
	
	var sphere_shape: SphereShape3D = SphereShape3D.new()
	sphere_shape.radius = display_radius * 1.5
	_collision_shape.shape = sphere_shape
	
	_click_area.add_child(_collision_shape)
	add_child(_click_area)
	
	# Connect signals for hover detection
	_click_area.mouse_entered.connect(_on_mouse_entered)
	_click_area.mouse_exited.connect(_on_mouse_exited)
	_click_area.input_event.connect(_on_input_event)


## Creates a point light for star bodies.
func _create_star_light() -> void:
	_star_light = OmniLight3D.new()
	_star_light.name = "StarLight"
	
	# Get star color from temperature
	var temperature_k: float = 5778.0
	if body.has_stellar():
		temperature_k = body.stellar.effective_temperature_k
	
	_star_light.light_color = ColorUtils.temperature_to_blackbody_color(temperature_k)
	
	# Scale energy with luminosity
	var energy: float = 2.0
	if body.has_stellar():
		var luminosity_solar: float = body.stellar.luminosity_watts / 3.828e26
		energy = 1.0 + log(maxf(luminosity_solar, 0.01)) / log(10.0) * 0.5
		energy = clampf(energy, 0.5, 6.0)
	
	_star_light.light_energy = energy
	_star_light.omni_range = display_radius * 30.0
	_star_light.shadow_enabled = false
	
	add_child(_star_light)


## Updates the selection ring visual.
func _update_selection_visual() -> void:
	if is_selected:
		if _selection_ring == null:
			_create_selection_ring()
		_selection_ring.visible = true
	else:
		if _selection_ring != null:
			_selection_ring.visible = false


## Updates the hover visual (slight scale increase).
func _update_hover_visual() -> void:
	if _mesh_instance == null:
		return
	
	if is_hovered:
		_mesh_instance.scale = Vector3.ONE * display_radius * 2.3
	else:
		_mesh_instance.scale = Vector3.ONE * display_radius * 2.0


## Creates a selection ring around the body.
func _create_selection_ring() -> void:
	_selection_ring = MeshInstance3D.new()
	_selection_ring.name = "SelectionRing"
	
	var torus: TorusMesh = TorusMesh.new()
	torus.inner_radius = display_radius * 2.2
	torus.outer_radius = display_radius * 2.5
	torus.rings = 16
	torus.ring_segments = 32
	
	_selection_ring.mesh = torus
	
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = Color(1.0, 0.9, 0.3, 0.8)
	material.emission_enabled = true
	material.emission = Color(1.0, 0.9, 0.3)
	material.emission_energy_multiplier = 0.5
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	_selection_ring.material_override = material
	
	_selection_ring.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	_selection_ring.visible = false
	
	add_child(_selection_ring)


## Handles mouse enter on click area.
func _on_mouse_entered() -> void:
	set_hovered(true)


## Handles mouse exit from click area.
func _on_mouse_exited() -> void:
	set_hovered(false)


## Handles input events on the click area.
## @param _camera: The camera that detected the event.
## @param event: The input event.
## @param _event_position: Position of the event.
## @param _normal: Normal at the event position.
## @param _shape_idx: Shape index.
func _on_input_event(
	_camera: Node,
	event: InputEvent,
	_event_position: Vector3,
	_normal: Vector3,
	_shape_idx: int
) -> void:
	if event is InputEventMouseButton:
		var mouse_event: InputEventMouseButton = event as InputEventMouseButton
		if mouse_event.pressed and mouse_event.button_index == MOUSE_BUTTON_LEFT:
			body_selected.emit(body_id)


## Cleans up resources.
func cleanup() -> void:
	body = null
	if _click_area:
		if _click_area.mouse_entered.is_connected(_on_mouse_entered):
			_click_area.mouse_entered.disconnect(_on_mouse_entered)
		if _click_area.mouse_exited.is_connected(_on_mouse_exited):
			_click_area.mouse_exited.disconnect(_on_mouse_exited)
		if _click_area.input_event.is_connected(_on_input_event):
			_click_area.input_event.disconnect(_on_input_event)
