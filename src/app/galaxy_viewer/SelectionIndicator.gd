## Visual indicator rendered at a selected star's position.
## Billboard ring + crosshair that always faces the camera.
class_name SelectionIndicator
extends MeshInstance3D


## Size of the indicator quad relative to star size.
const INDICATOR_SIZE: float = 4.0

## Shader material reference.
var _material: ShaderMaterial


func _ready() -> void:
	_build_mesh()
	visible = false


## Shows the indicator at a world-space position.
## @param world_pos: Position to place the indicator.
func show_at(world_pos: Vector3) -> void:
	position = world_pos
	visible = true


## Hides the indicator.
func hide_indicator() -> void:
	visible = false


## Returns whether the indicator is currently shown.
## @return: True if visible.
func is_shown() -> bool:
	return visible


## Builds the billboard quad mesh with the selection ring shader.
func _build_mesh() -> void:
	var quad: QuadMesh = QuadMesh.new()
	quad.size = Vector2(1.0, 1.0)

	_material = ShaderMaterial.new()
	_material.shader = preload("res://src/app/galaxy_viewer/shaders/selection_ring.gdshader")
	quad.material = _material

	mesh = quad

	var basis_scaled: Basis = Basis.IDENTITY.scaled(
		Vector3(INDICATOR_SIZE, INDICATOR_SIZE, INDICATOR_SIZE)
	)
	transform = Transform3D(basis_scaled, Vector3.ZERO)
