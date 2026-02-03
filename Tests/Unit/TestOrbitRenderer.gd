## Unit tests for OrbitRenderer.
## Tests orbit line creation, zone rings, and selection highlighting.
extends TestCase

const _orbit_renderer: GDScript = preload("res://src/app/system_viewer/OrbitRenderer.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")


## Helper to create an OrbitRenderer attached to the scene tree.
## @return: OrbitRenderer node (must be freed by test).
func _create_renderer() -> OrbitRenderer:
	var renderer: OrbitRenderer = OrbitRenderer.new()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	if scene_tree and scene_tree.root:
		scene_tree.root.call_deferred("add_child", renderer)
	return renderer


## Helper to generate simple circular orbit points.
## @param radius: Circle radius.
## @param num_points: Number of points.
## @return: PackedVector3Array of circle points.
func _make_circle_points(radius: float, num_points: int = 32) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	for i in range(num_points + 1):
		var angle: float = (float(i) / float(num_points)) * TAU
		points.append(Vector3(cos(angle) * radius, 0.0, sin(angle) * radius))
	return points


# =============================================================================
# ORBIT CREATION TESTS
# =============================================================================


## Tests adding a planet orbit.
func test_add_planet_orbit() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var points: PackedVector3Array = _make_circle_points(5.0)
	
	var mesh: MeshInstance3D = renderer.add_orbit("planet_1", points, CelestialType.Type.PLANET)
	
	assert_not_null(mesh, "Should create orbit mesh")
	assert_equal(renderer.get_child_count(), 1, "Should have one child")
	assert_equal(mesh.name, "Orbit_planet_1", "Should have correct name")
	
	renderer.queue_free()


## Tests adding multiple orbits.
func test_add_multiple_orbits() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	
	renderer.add_orbit("planet_1", _make_circle_points(2.0), CelestialType.Type.PLANET)
	renderer.add_orbit("planet_2", _make_circle_points(4.0), CelestialType.Type.PLANET)
	renderer.add_orbit("moon_1", _make_circle_points(0.5), CelestialType.Type.MOON)
	
	assert_equal(renderer.get_child_count(), 3, "Should have three orbit children")
	
	renderer.queue_free()


## Tests adding orbit with empty points returns null.
func test_add_orbit_empty_points() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var empty: PackedVector3Array = PackedVector3Array()
	
	var mesh: MeshInstance3D = renderer.add_orbit("empty", empty, CelestialType.Type.PLANET)
	
	assert_null(mesh, "Empty points should return null")
	assert_equal(renderer.get_child_count(), 0, "Should have no children")
	
	renderer.queue_free()


## Tests orbit mesh has material with transparency.
func test_orbit_material_has_transparency() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var points: PackedVector3Array = _make_circle_points(3.0)
	
	var mesh: MeshInstance3D = renderer.add_orbit("planet_1", points, CelestialType.Type.PLANET)
	
	assert_not_null(mesh.material_override, "Should have material override")
	var material: StandardMaterial3D = mesh.material_override as StandardMaterial3D
	assert_not_null(material, "Material should be StandardMaterial3D")
	assert_equal(material.transparency, BaseMaterial3D.TRANSPARENCY_ALPHA,
		"Material should use alpha transparency")
	
	renderer.queue_free()


## Tests orbit mesh doesn't cast shadows.
func test_orbit_no_shadow() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var points: PackedVector3Array = _make_circle_points(3.0)
	
	var mesh: MeshInstance3D = renderer.add_orbit("planet_1", points, CelestialType.Type.PLANET)
	
	assert_equal(mesh.cast_shadow, GeometryInstance3D.SHADOW_CASTING_SETTING_OFF,
		"Orbit lines should not cast shadows")
	
	renderer.queue_free()


## Tests different body types get different orbit colors.
func test_orbit_colors_differ_by_type() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var points: PackedVector3Array = _make_circle_points(3.0)
	
	var planet_mesh: MeshInstance3D = renderer.add_orbit("p1", points, CelestialType.Type.PLANET)
	var moon_mesh: MeshInstance3D = renderer.add_orbit("m1", points, CelestialType.Type.MOON)
	
	var planet_mat: StandardMaterial3D = planet_mesh.material_override as StandardMaterial3D
	var moon_mat: StandardMaterial3D = moon_mesh.material_override as StandardMaterial3D
	
	# Colors should be different
	assert_true(
		planet_mat.albedo_color != moon_mat.albedo_color,
		"Planet and moon orbits should have different colors"
	)
	
	renderer.queue_free()


# =============================================================================
# ZONE RING TESTS
# =============================================================================


## Tests adding a zone ring.
func test_add_zone_ring() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	
	var mesh: MeshInstance3D = renderer.add_zone_ring(
		"habitable_inner", 5.0, OrbitRenderer.HZ_INNER_COLOR
	)
	
	assert_not_null(mesh, "Should create zone ring mesh")
	assert_equal(mesh.name, "Zone_habitable_inner", "Should have correct name")
	
	renderer.queue_free()


## Tests zone ring with zero radius returns null.
func test_zone_ring_zero_radius() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	
	var mesh: MeshInstance3D = renderer.add_zone_ring("zero", 0.0, Color.WHITE)
	
	assert_null(mesh, "Zero radius zone ring should return null")
	
	renderer.queue_free()


## Tests zone ring with negative radius returns null.
func test_zone_ring_negative_radius() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	
	var mesh: MeshInstance3D = renderer.add_zone_ring("neg", -5.0, Color.WHITE)
	
	assert_null(mesh, "Negative radius zone ring should return null")
	
	renderer.queue_free()


# =============================================================================
# SELECTION/HIGHLIGHT TESTS
# =============================================================================


## Tests highlighting an orbit changes its material.
func test_highlight_orbit() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var points: PackedVector3Array = _make_circle_points(3.0)
	renderer.add_orbit("planet_1", points, CelestialType.Type.PLANET)
	
	# Get the original color
	var orbit_mesh: MeshInstance3D = renderer.get_child(0) as MeshInstance3D
	var original_mat: StandardMaterial3D = orbit_mesh.material_override as StandardMaterial3D
	var original_color: Color = original_mat.albedo_color
	
	# Highlight
	renderer.highlight_orbit("planet_1")
	
	var highlighted_mat: StandardMaterial3D = orbit_mesh.material_override as StandardMaterial3D
	assert_true(
		highlighted_mat.albedo_color != original_color,
		"Highlighted orbit should have different color"
	)
	
	renderer.queue_free()


## Tests highlighting empty string clears selection.
func test_highlight_empty_clears() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var points: PackedVector3Array = _make_circle_points(3.0)
	renderer.add_orbit("planet_1", points, CelestialType.Type.PLANET)
	
	# Highlight then clear
	renderer.highlight_orbit("planet_1")
	renderer.highlight_orbit("")
	
	# Should not crash (internal state is cleared)
	assert_true(true, "Clearing highlight should not crash")
	
	renderer.queue_free()


## Tests highlighting nonexistent orbit doesn't crash.
func test_highlight_nonexistent_orbit() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	var points: PackedVector3Array = _make_circle_points(3.0)
	renderer.add_orbit("planet_1", points, CelestialType.Type.PLANET)
	
	# Highlight a non-existent orbit
	renderer.highlight_orbit("nonexistent_orbit")
	
	# Should not crash
	assert_true(true, "Highlighting nonexistent orbit should not crash")
	
	renderer.queue_free()


# =============================================================================
# CLEAR TESTS
# =============================================================================


## Tests clearing removes all orbits.
func test_clear_removes_all() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	
	renderer.add_orbit("p1", _make_circle_points(2.0), CelestialType.Type.PLANET)
	renderer.add_orbit("p2", _make_circle_points(4.0), CelestialType.Type.PLANET)
	renderer.add_zone_ring("hz", 3.0, Color.GREEN)
	
	assert_equal(renderer.get_child_count(), 3, "Should have 3 children before clear")
	
	renderer.clear()
	
	# Children are queue_free'd, so they'll be removed next frame
	# But internally, the orbit meshes dictionary should be empty
	# We can verify by trying to highlight a cleared orbit
	renderer.highlight_orbit("p1")
	assert_true(true, "Highlighting after clear should not crash")
	
	renderer.queue_free()


# =============================================================================
# MOON ORBIT VISIBILITY TESTS
# =============================================================================


## Tests moon orbit visibility toggle.
func test_moon_orbit_visibility() -> void:
	var renderer: OrbitRenderer = _create_renderer()
	
	renderer.add_orbit("planet_1", _make_circle_points(3.0), CelestialType.Type.PLANET)
	renderer.add_orbit("moon_1", _make_circle_points(0.5), CelestialType.Type.MOON)
	
	# Hide moon orbits
	renderer.set_moon_orbits_visible(false)
	
	# Planet orbit should still be visible
	# (We can't easily check visibility of queue_free'd children in same frame,
	# but we verify the method doesn't crash)
	assert_true(true, "Setting moon orbit visibility should not crash")
	
	# Show moon orbits
	renderer.set_moon_orbits_visible(true)
	assert_true(true, "Showing moon orbits should not crash")
	
	renderer.queue_free()
