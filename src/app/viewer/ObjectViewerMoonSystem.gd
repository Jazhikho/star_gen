## Manages moon display, orbital animation, and focus for the ObjectViewer.
## Extracted from ObjectViewer to reduce script size per claude.md guidelines.
class_name ObjectViewerMoonSystem
extends RefCounted

const _body_renderer_scene: PackedScene = preload("res://src/app/rendering/BodyRenderer.tscn")

## Visual period for a moon at ORBIT_REFERENCE_SMA_M (seconds per full orbit).
const ORBIT_BASE_PERIOD_S: float = 120.0

## Reference semi-major axis for Kepler period scaling (metres); ~Luna distance.
const ORBIT_REFERENCE_SMA_M: float = 3.844e8

## The primary body that moons orbit.
var _primary_body: CelestialBody = null

## Array of moon bodies currently displayed.
var _moons: Array[CelestialBody] = []

## The moon that currently has focus (null = primary body focused).
var _focused_moon: CelestialBody = null

## BodyRenderer instances for each moon (index matches _moons).
var _moon_renderers: Array[BodyRenderer] = []

## Accumulated visual time driving moon orbital animation (seconds).
var _orbit_visual_time: float = 0.0

## Cached display scale for the primary body.
var _primary_display_scale: float = 1.0

## Rig under body_renderer with rotation matching planet axial tilt.
var _moon_system_rig: Node3D = null

## Container that holds all moon BodyRenderer instances.
var _moon_bodies_node: Node3D = null

## Container that holds all moon orbit line meshes.
var _moon_orbits_node: Node3D = null

## Emitted when focus shifts to a moon (null = back to planet).
signal moon_focused(moon: CelestialBody)


## Sets up the moon system containers under the given body renderer.
## Must be called once during ObjectViewer initialization.
## @param body_renderer: The parent BodyRenderer node to attach containers to.
func setup(body_renderer: Node3D) -> void:
	_moon_system_rig = Node3D.new()
	_moon_system_rig.name = "MoonSystemRig"
	body_renderer.add_child(_moon_system_rig)

	_moon_bodies_node = Node3D.new()
	_moon_bodies_node.name = "MoonBodies"
	_moon_system_rig.add_child(_moon_bodies_node)

	_moon_orbits_node = Node3D.new()
	_moon_orbits_node.name = "MoonOrbits"
	_moon_system_rig.add_child(_moon_orbits_node)


## Sets the primary body and its display scale.
## Call this before build_moon_display.
## @param body: The primary celestial body.
## @param display_scale: The display scale factor for the primary body.
func set_primary_body(body: CelestialBody, display_scale: float) -> void:
	_primary_body = body
	_primary_display_scale = display_scale


## Builds the moon display for the given moons.
## Creates renderers, positions moons, and draws orbit lines.
## @param moons: Array of moon bodies to display.
## @param axial_tilt_deg: The primary body's axial tilt in degrees.
func build_moon_display(moons: Array[CelestialBody], axial_tilt_deg: float) -> void:
	clear()
	_moons = moons
	_orbit_visual_time = 0.0

	if moons.is_empty() or _primary_body == null:
		return

	# Align moon system with planet equator
	_moon_system_rig.basis = Basis(Vector3.FORWARD, deg_to_rad(axial_tilt_deg))

	var moon_scale: float = _get_moon_system_scale()

	for i: int in range(moons.size()):
		var moon: CelestialBody = moons[i]

		var renderer: BodyRenderer = _body_renderer_scene.instantiate() as BodyRenderer
		if not renderer:
			continue
		renderer.name = "MoonRenderer_%d" % i
		var moon_display_radius: float = moon.physical.radius_m * moon_scale
		var live_ma: float = _compute_live_mean_anomaly(moon)
		renderer.position = _get_moon_position_at_ma(moon, live_ma)
		_moon_bodies_node.add_child(renderer)
		renderer.render_body(moon, moon_display_radius)
		_moon_renderers.append(renderer)

		var orbit_line: MeshInstance3D = _create_moon_orbit_line(moon, moon_scale)
		if orbit_line:
			_moon_orbits_node.add_child(orbit_line)


## Clears all moon renderers, orbit lines, and resets state.
func clear() -> void:
	if _moon_bodies_node:
		for child: Node in _moon_bodies_node.get_children():
			child.queue_free()
	if _moon_orbits_node:
		for child: Node in _moon_orbits_node.get_children():
			child.queue_free()
	_moon_renderers.clear()
	_moons.clear()
	_focused_moon = null
	_orbit_visual_time = 0.0


## Updates moon orbital positions. Call each frame with delta time.
## @param delta: Time since last frame in seconds.
func update_orbital_positions(delta: float) -> void:
	if _moons.is_empty():
		return

	_orbit_visual_time += delta

	for i: int in range(_moons.size()):
		if i >= _moon_renderers.size():
			break
		var moon: CelestialBody = _moons[i]
		var renderer: BodyRenderer = _moon_renderers[i]
		if not moon.has_orbital() or not is_instance_valid(renderer):
			continue
		var live_ma: float = _compute_live_mean_anomaly(moon)
		renderer.position = _get_moon_position_at_ma(moon, live_ma)


## Returns the currently focused moon, or null if planet is focused.
## @return: The focused moon body or null.
func get_focused_moon() -> CelestialBody:
	return _focused_moon


## Returns the array of current moons.
## @return: Array of moon bodies.
func get_moons() -> Array[CelestialBody]:
	return _moons


## Returns whether any moons are currently displayed.
## @return: True if moons are displayed.
func has_moons() -> bool:
	return not _moons.is_empty()


## Shifts focus to the given moon.
## @param moon: Moon to focus on (must be in current moons).
## @return: True if focus was changed, false if moon not found.
func focus_on_moon(moon: CelestialBody) -> bool:
	if not _moons.has(moon):
		return false
	_focused_moon = moon
	moon_focused.emit(moon)
	return true


## Returns focus to the primary body.
func focus_on_planet() -> void:
	_focused_moon = null
	moon_focused.emit(null)


## Returns the world-space position of the focused moon for camera following.
## @return: World position, or Vector3.ZERO if no moon focused.
func get_focused_moon_position() -> Vector3:
	if _focused_moon == null or not _moons.has(_focused_moon):
		return Vector3.ZERO
	var idx: int = _moons.find(_focused_moon)
	if idx < 0 or idx >= _moon_renderers.size():
		return Vector3.ZERO
	var renderer: BodyRenderer = _moon_renderers[idx]
	if not is_instance_valid(renderer):
		return Vector3.ZERO
	return renderer.global_position


## Returns the display radius of the focused moon.
## @return: Display radius, or 0.0 if no moon focused.
func get_focused_moon_display_radius() -> float:
	if _focused_moon == null:
		return 0.0
	return _focused_moon.physical.radius_m * _get_moon_system_scale()


## Calculates the camera distance needed to frame the full moon system.
## @return: Camera distance in display units.
func get_framing_distance() -> float:
	if _primary_body == null:
		return 10.0
	var min_frame: float = _primary_display_scale * 3.0
	if _moons.is_empty():
		return maxf(min_frame, _primary_display_scale * 3.0)

	var moon_scale: float = _get_moon_system_scale()
	var farthest: float = 0.0
	for moon: CelestialBody in _moons:
		if moon.has_orbital():
			var apoapsis: float = moon.orbital.semi_major_axis_m \
				* (1.0 + moon.orbital.eccentricity)
			farthest = maxf(farthest, apoapsis * moon_scale)

	if farthest <= 0.0:
		return maxf(min_frame, _primary_display_scale * 3.0)

	return maxf(min_frame, farthest * 1.5)


## Detects if a click intersects any moon mesh via ray-sphere intersection.
## @param camera: The camera for ray projection.
## @param mouse_pos: Mouse position in viewport coordinates.
## @return: The clicked moon, or null if no hit.
func detect_moon_click(camera: Camera3D, mouse_pos: Vector2) -> CelestialBody:
	if _moons.is_empty() or not camera:
		return null

	var ray_origin: Vector3 = camera.project_ray_origin(mouse_pos)
	var ray_dir: Vector3 = camera.project_ray_normal(mouse_pos)

	var best_moon: CelestialBody = null
	var best_t: float = INF

	var moon_scale: float = _get_moon_system_scale()
	for i: int in range(_moons.size()):
		if i >= _moon_renderers.size():
			break
		var moon: CelestialBody = _moons[i]
		var renderer: BodyRenderer = _moon_renderers[i]
		if not is_instance_valid(renderer):
			continue
		var centre: Vector3 = renderer.global_position
		var radius: float = moon.physical.radius_m * moon_scale
		var oc: Vector3 = ray_origin - centre
		var b: float = oc.dot(ray_dir)
		var c: float = oc.dot(oc) - radius * radius
		var disc: float = b * b - c

		if disc >= 0.0:
			var t: float = -b - sqrt(disc)
			if t > 0.0 and t < best_t:
				best_t = t
				best_moon = moon

	return best_moon


## Returns the scale factor that converts physical meters to display units.
## @return: Display units per meter.
func _get_moon_system_scale() -> float:
	if _primary_body == null:
		return 1.0
	var r: float = _primary_body.physical.radius_m
	if r <= 0.0:
		return 1.0
	return _primary_display_scale / r


## Returns the live mean anomaly (radians) for a moon at current orbit time.
## @param moon: The moon body.
## @return: Mean anomaly in radians.
func _compute_live_mean_anomaly(moon: CelestialBody) -> float:
	if not moon.has_orbital():
		return 0.0
	var sma: float = moon.orbital.semi_major_axis_m
	var period_scale: float = pow(sma / ORBIT_REFERENCE_SMA_M, 1.5)
	var visual_period: float = ORBIT_BASE_PERIOD_S * period_scale
	if visual_period < 0.001:
		visual_period = 0.001
	var initial_ma: float = deg_to_rad(moon.orbital.mean_anomaly_deg)
	return initial_ma + (TAU / visual_period) * _orbit_visual_time


## Computes display-space position for a moon at an explicit mean anomaly.
## @param moon: The moon body.
## @param mean_anomaly_rad: Mean anomaly in radians.
## @return: 3D display-space position.
func _get_moon_position_at_ma(moon: CelestialBody, mean_anomaly_rad: float) -> Vector3:
	if not moon.has_orbital():
		return Vector3.ZERO

	var ms: float = _get_moon_system_scale()
	var a: float = moon.orbital.semi_major_axis_m * ms
	var e: float = clampf(moon.orbital.eccentricity, 0.0, 0.99)
	var inc: float = deg_to_rad(moon.orbital.inclination_deg)
	var lan: float = deg_to_rad(moon.orbital.longitude_of_ascending_node_deg)
	var aop: float = deg_to_rad(moon.orbital.argument_of_periapsis_deg)

	var ea: float = _solve_kepler(mean_anomaly_rad, e)
	var ta: float = 2.0 * atan2(
		sqrt(1.0 + e) * sin(ea / 2.0),
		sqrt(1.0 - e) * cos(ea / 2.0)
	)
	var r: float = a * (1.0 - e * cos(ea))
	var px: float = r * cos(ta)
	var py: float = r * sin(ta)

	var c_lan: float = cos(lan)
	var s_lan: float = sin(lan)
	var c_aop: float = cos(aop)
	var s_aop: float = sin(aop)
	var c_inc: float = cos(inc)
	var s_inc: float = sin(inc)

	var x: float = (c_lan * c_aop - s_lan * s_aop * c_inc) * px \
		+ (-c_lan * s_aop - s_lan * c_aop * c_inc) * py
	var z: float = (s_lan * c_aop + c_lan * s_aop * c_inc) * px \
		+ (-s_lan * s_aop + c_lan * c_aop * c_inc) * py
	var y: float = (s_aop * s_inc) * px + (c_aop * s_inc) * py

	return Vector3(x, y, z)


## Solves Kepler's equation for eccentric anomaly using Newton-Raphson.
## @param mean_anomaly: Mean anomaly in radians.
## @param eccentricity: Orbital eccentricity [0, 1).
## @return: Eccentric anomaly in radians.
func _solve_kepler(mean_anomaly: float, eccentricity: float) -> float:
	var ea: float = mean_anomaly
	for _i: int in range(5):
		ea = ea - (ea - eccentricity * sin(ea) - mean_anomaly) \
			/ (1.0 - eccentricity * cos(ea))
	return ea


## Creates a closed elliptical orbit line for a moon.
## @param moon: The moon body (must have orbital properties).
## @param moon_scale: Display units per meter.
## @return: A MeshInstance3D with the line strip, or null if no orbital data.
func _create_moon_orbit_line(moon: CelestialBody, moon_scale: float) -> MeshInstance3D:
	if not moon.has_orbital():
		return null

	var a: float = moon.orbital.semi_major_axis_m * moon_scale
	var e: float = clampf(moon.orbital.eccentricity, 0.0, 0.99)
	var b: float = a * sqrt(1.0 - e * e)
	var inc: float = deg_to_rad(moon.orbital.inclination_deg)
	var lan: float = deg_to_rad(moon.orbital.longitude_of_ascending_node_deg)
	var aop: float = deg_to_rad(moon.orbital.argument_of_periapsis_deg)

	var c_lan: float = cos(lan)
	var s_lan: float = sin(lan)
	var c_aop: float = cos(aop)
	var s_aop: float = sin(aop)
	var c_inc: float = cos(inc)
	var s_inc: float = sin(inc)

	var f: float = a * e

	var orbit_mesh: ImmediateMesh = ImmediateMesh.new()
	orbit_mesh.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)

	const SEGMENTS: int = 128
	for i: int in range(SEGMENTS + 1):
		var angle: float = (float(i) / float(SEGMENTS)) * TAU
		var px: float = a * cos(angle) - f
		var py: float = b * sin(angle)

		var x: float = (c_lan * c_aop - s_lan * s_aop * c_inc) * px \
			+ (-c_lan * s_aop - s_lan * c_aop * c_inc) * py
		var z: float = (s_lan * c_aop + c_lan * s_aop * c_inc) * px \
			+ (-s_lan * s_aop + c_lan * c_aop * c_inc) * py
		var y: float = (s_aop * s_inc) * px + (c_aop * s_inc) * py

		orbit_mesh.surface_add_vertex(Vector3(x, y, z))

	orbit_mesh.surface_end()

	var instance: MeshInstance3D = MeshInstance3D.new()
	instance.mesh = orbit_mesh
	instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF

	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.albedo_color = Color(0.45, 0.65, 0.85, 0.55)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	instance.material_override = mat

	return instance
