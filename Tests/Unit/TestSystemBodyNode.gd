## Unit tests for SystemBodyNode.
## Tests body setup, selection, hover states, and visual updates.
extends TestCase

const _system_body_node: GDScript = preload("res://src/app/system_viewer/SystemBodyNode.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Helper to create a test body.
## @param type: Body type.
## @param id: Body ID.
## @param body_name: Display name.
## @return: CelestialBody.
func _make_body(
	type: CelestialType.Type,
	id: String = "test_body",
	body_name: String = "Test Body"
) -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0, 23.5, 0.003, 8.0e22, 4.7e13
	)
	var body: CelestialBody = CelestialBody.new(id, body_name, type, physical, null)
	# Add orbital props for planets/moons/asteroids
	if type != CelestialType.Type.STAR:
		var orbital: OrbitalProps = OrbitalProps.new(
			Units.AU_METERS, 0.017, 0.0, 0.0, 0.0, 45.0, "star_1"
		)
		body.orbital = orbital
	return body


## Helper to create a star body.
## @return: Star CelestialBody.
func _make_star() -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.SOLAR_MASS_KG,
		Units.SOLAR_RADIUS_METERS,
		2.16e6, 7.25, 0.0, 0.0, 0.0
	)
	var body: CelestialBody = CelestialBody.new(
		"star_1", "Sol", CelestialType.Type.STAR, physical, null
	)
	body.stellar = StellarProps.new(
		3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9
	)
	return body


## Helper to create a SystemBodyNode in the scene tree.
## @return: SystemBodyNode (must be freed by test).
func _create_node() -> SystemBodyNode:
	var node: SystemBodyNode = SystemBodyNode.new()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	if scene_tree and scene_tree.root:
		scene_tree.root.call_deferred("add_child", node)
	return node


# =============================================================================
# SETUP TESTS
# =============================================================================


## Tests basic setup with a planet.
func test_setup_planet() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET, "planet_1", "Earth")
	
	node.setup(body, 0.2, Vector3(5.0, 0.0, 0.0))
	
	assert_equal(node.body_id, "planet_1", "Should set body ID")
	assert_float_equal(node.display_radius, 0.2, 0.001, "Should set display radius")
	assert_equal(node.position, Vector3(5.0, 0.0, 0.0), "Should set position")
	assert_equal(node.name, "Body_planet_1", "Should set node name")
	
	node.queue_free()


## Tests setup with a star creates light.
func test_setup_star_creates_light() -> void:
	var node: SystemBodyNode = _create_node()
	var star: CelestialBody = _make_star()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	node.setup(star, 0.3, Vector3.ZERO)
	
	# Should have a StarLight child
	var light: OmniLight3D = node.get_node_or_null("StarLight") as OmniLight3D
	assert_not_null(light, "Star body should have a light")
	
	node.queue_free()


## Tests setup with planet does NOT create light.
func test_setup_planet_no_light() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	
	node.setup(body, 0.2, Vector3(3.0, 0.0, 0.0))
	
	var light: OmniLight3D = node.get_node_or_null("StarLight") as OmniLight3D
	assert_null(light, "Planet should not have a star light")
	
	node.queue_free()


## Tests setup creates mesh.
func test_setup_creates_mesh() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	
	node.setup(body, 0.15, Vector3.ZERO)
	
	var mesh: MeshInstance3D = node.get_node_or_null("Mesh") as MeshInstance3D
	assert_not_null(mesh, "Should create mesh instance")
	assert_not_null(mesh.mesh, "Mesh should have geometry")
	assert_not_null(mesh.material_override, "Mesh should have material")
	
	node.queue_free()


## Tests setup creates click area.
func test_setup_creates_click_area() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	
	node.setup(body, 0.2, Vector3.ZERO)
	
	var area: Area3D = node.get_node_or_null("ClickArea") as Area3D
	assert_not_null(area, "Should create click area")
	
	var shape: CollisionShape3D = area.get_node_or_null("Shape") as CollisionShape3D
	assert_not_null(shape, "Click area should have collision shape")
	assert_true(shape.shape is SphereShape3D, "Shape should be sphere")
	
	node.queue_free()


## Tests null body setup does nothing.
func test_setup_null_body() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	node.setup(null, 0.2, Vector3.ZERO)
	
	assert_equal(node.body_id, "", "Body ID should remain empty")
	assert_null(node.body, "Body should remain null")
	# Should have no mesh or click area children
	assert_equal(node.get_child_count(), 0, "Should have no children")
	
	node.queue_free()


# =============================================================================
# SELECTION TESTS
# =============================================================================


## Tests selection state change.
func test_set_selected() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	node.setup(body, 0.2, Vector3.ZERO)
	
	assert_false(node.is_selected, "Should not be selected initially")
	
	node.set_selected(true)
	assert_true(node.is_selected, "Should be selected after set_selected(true)")
	
	node.set_selected(false)
	assert_false(node.is_selected, "Should not be selected after set_selected(false)")
	
	node.queue_free()


## Tests selection creates visual indicator.
func test_selection_creates_ring() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	node.setup(body, 0.2, Vector3.ZERO)
	
	node.set_selected(true)
	
	var ring: MeshInstance3D = node.get_node_or_null("SelectionRing") as MeshInstance3D
	assert_not_null(ring, "Selection should create ring")
	assert_true(ring.visible, "Selection ring should be visible")
	
	node.set_selected(false)
	assert_false(ring.visible, "Selection ring should be hidden when deselected")
	
	node.queue_free()


## Tests selection signal emission.
func test_body_selected_signal() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET, "planet_test")
	node.setup(body, 0.2, Vector3.ZERO)
	
	var received_id: String = ""
	node.body_selected.connect(func(id: String) -> void:
		received_id = id
	)
	
	# Simulate selection signal emission
	node.body_selected.emit("planet_test")
	
	assert_equal(received_id, "planet_test", "Should emit body_selected with correct ID")
	
	node.queue_free()


# =============================================================================
# HOVER TESTS
# =============================================================================


## Tests hover state change.
func test_set_hovered() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	node.setup(body, 0.2, Vector3.ZERO)
	
	assert_false(node.is_hovered, "Should not be hovered initially")
	
	node.set_hovered(true)
	assert_true(node.is_hovered, "Should be hovered after set_hovered(true)")
	
	node.set_hovered(false)
	assert_false(node.is_hovered, "Should not be hovered after set_hovered(false)")
	
	node.queue_free()


## Tests hover emits signals.
func test_hover_signals() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET, "hover_test")
	node.setup(body, 0.2, Vector3.ZERO)
	
	var hovered_id: String = ""
	var unhovered_id: String = ""
	
	node.body_hovered.connect(func(id: String) -> void:
		hovered_id = id
	)
	node.body_unhovered.connect(func(id: String) -> void:
		unhovered_id = id
	)
	
	node.set_hovered(true)
	assert_equal(hovered_id, "hover_test", "Should emit body_hovered")
	
	node.set_hovered(false)
	assert_equal(unhovered_id, "hover_test", "Should emit body_unhovered")
	
	node.queue_free()


## Tests hover does not emit duplicate signals.
func test_hover_no_duplicate_signals() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET, "dup_test")
	node.setup(body, 0.2, Vector3.ZERO)
	
	var hover_count: int = 0
	node.body_hovered.connect(func(_id: String) -> void:
		hover_count += 1
	)
	
	node.set_hovered(true)
	node.set_hovered(true) # Duplicate - should not emit
	node.set_hovered(true) # Duplicate - should not emit
	
	assert_equal(hover_count, 1, "Should only emit hover signal once for same state")
	
	node.queue_free()


## Tests hover visual scales up mesh.
func test_hover_scales_mesh() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	node.setup(body, 0.2, Vector3.ZERO)
	
	var mesh: MeshInstance3D = node.get_node_or_null("Mesh") as MeshInstance3D
	var normal_scale: Vector3 = mesh.scale
	
	node.set_hovered(true)
	var hover_scale: Vector3 = mesh.scale
	
	assert_true(hover_scale.x > normal_scale.x,
		"Hovered mesh should be larger (normal=%.2f, hover=%.2f)" % [normal_scale.x, hover_scale.x])
	
	node.set_hovered(false)
	var restored_scale: Vector3 = mesh.scale
	assert_float_equal(restored_scale.x, normal_scale.x, 0.01,
		"Unhovered mesh should return to normal scale")
	
	node.queue_free()


# =============================================================================
# GETTER TESTS
# =============================================================================


## Tests get_body_type returns correct type.
func test_get_body_type() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.MOON, "moon_1", "Luna")
	node.setup(body, 0.1, Vector3.ZERO)
	
	assert_equal(node.get_body_type(), CelestialType.Type.MOON,
		"Should return correct body type")
	
	node.queue_free()


## Tests get_display_name returns body name.
func test_get_display_name() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET, "p1", "Jupiter")
	node.setup(body, 0.2, Vector3.ZERO)
	
	assert_equal(node.get_display_name(), "Jupiter",
		"Should return body display name")
	
	node.queue_free()


## Tests get_display_name with null body.
func test_get_display_name_null_body() -> void:
	var node: SystemBodyNode = SystemBodyNode.new()
	assert_equal(node.get_display_name(), "Unknown",
		"Null body should return 'Unknown'")
	node.free()


## Tests get_parent_id returns orbital parent.
func test_get_parent_id() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET, "p1", "Earth")
	node.setup(body, 0.2, Vector3.ZERO)
	
	assert_equal(node.get_parent_id(), "star_1",
		"Should return parent ID from orbital props")
	
	node.queue_free()


## Tests get_parent_id with no orbital props.
func test_get_parent_id_no_orbital() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var star: CelestialBody = _make_star()
	node.setup(star, 0.3, Vector3.ZERO)
	
	assert_equal(node.get_parent_id(), "",
		"Star without orbital props should return empty parent ID")
	
	node.queue_free()


# =============================================================================
# UPDATE VISUAL TESTS
# =============================================================================


## Tests update_visual updates mesh scale.
func test_update_visual_changes_scale() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	node.setup(body, 0.2, Vector3.ZERO)
	
	var mesh: MeshInstance3D = node.get_node_or_null("Mesh") as MeshInstance3D
	var initial_scale: float = mesh.scale.x
	
	# Change display radius and update
	node.display_radius = 0.4
	node.update_visual()
	
	assert_true(mesh.scale.x > initial_scale,
		"Updated visual should have larger mesh scale")
	assert_float_equal(mesh.scale.x, 0.8, 0.01,
		"Scale should be 2 * display_radius (got %.3f)" % mesh.scale.x)
	
	node.queue_free()


## Tests mesh doesn't cast shadows.
func test_mesh_no_shadows() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	node.setup(body, 0.2, Vector3.ZERO)
	
	var mesh: MeshInstance3D = node.get_node_or_null("Mesh") as MeshInstance3D
	assert_equal(mesh.cast_shadow, GeometryInstance3D.SHADOW_CASTING_SETTING_OFF,
		"Body mesh should not cast shadows in system view")
	
	node.queue_free()


# =============================================================================
# CLEANUP TESTS
# =============================================================================


## Tests cleanup clears references.
func test_cleanup() -> void:
	var node: SystemBodyNode = _create_node()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET)
	node.setup(body, 0.2, Vector3.ZERO)
	
	node.cleanup()
	
	assert_null(node.body, "Body reference should be cleared after cleanup")
	
	node.queue_free()
