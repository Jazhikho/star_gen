## Handles layout calculations for system viewer display.
## Uses simplified logarithmic scaling for visual clarity rather than astronomical accuracy.
## Properly accounts for orbital extents to prevent overlap in multi-star systems.
class_name SystemDisplayLayout
extends RefCounted

const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")


## Base display radius for a 1 solar radius star (in Godot units).
const STAR_BASE_RADIUS: float = 3.0

## Minimum star display radius.
const STAR_MIN_RADIUS: float = 1.0

## Maximum star display radius.
const STAR_MAX_RADIUS: float = 9.0

## Minimum planet display radius.
const PLANET_MIN_RADIUS: float = 0.25

## Maximum planet display radius.
const PLANET_MAX_RADIUS: float = 2.0

## Minimum gap between star surface and planet surface on first orbit.
const FIRST_ORBIT_SURFACE_GAP: float = 4.0

## Distance between subsequent planet orbits (center to center).
const ORBIT_SPACING: float = 6.0

## Minimum gap between binary components (after accounting for orbital extents and sweep).
const BINARY_BUFFER_GAP: float = 8.0

## Minimum gap between P-type orbit and inner system extent.
const PTYPE_BUFFER_GAP: float = 6.0

## Orbital period for animation (seconds for full orbit at base distance).
const BASE_ORBITAL_PERIOD: float = 20.0

## Period scaling exponent (Kepler-like: period scales with distance; less than 1.5 for visual appeal).
const ORBITAL_PERIOD_EXPONENT: float = 0.8


## Calculated layout data for a body.
class BodyLayout:
	extends RefCounted

	## Body ID.
	var body_id: String

	## Display position in Godot units (updated during animation).
	var position: Vector3

	## Display radius in Godot units.
	var display_radius: float

	## Orbit radius (distance from parent center) in Godot units. 0 for non-orbiting bodies.
	var orbit_radius: float

	## Parent position for orbit visualization (center of orbit).
	var orbit_center: Vector3

	## Parent body/host ID for orbit visualization.
	var orbit_parent_id: String

	## Current orbital angle in radians (for animation).
	var orbital_angle: float

	## Orbital period in seconds.
	var orbital_period: float

	## Whether this body orbits (for animation).
	var is_orbiting: bool

	func _init(p_id: String = "") -> void:
		body_id = p_id
		position = Vector3.ZERO
		display_radius = 1.0
		orbit_radius = 0.0
		orbit_center = Vector3.ZERO
		orbit_parent_id = ""
		orbital_angle = 0.0
		orbital_period = BASE_ORBITAL_PERIOD
		is_orbiting = false

	## Updates position based on current orbital angle.
	func update_position_from_angle() -> void:
		if is_orbiting and orbit_radius > 0:
			position = orbit_center + Vector3(
				cos(orbital_angle) * orbit_radius,
				0.0,
				sin(orbital_angle) * orbit_radius
			)


## Intermediate data for calculating node extents.
class NodeExtent:
	extends RefCounted

	## The hierarchy node ID.
	var node_id: String

	## Radius from this node's center to its outermost content.
	## For a star: star display radius + outermost S-type planet orbit.
	## For a barycenter: distance to farthest child edge + P-type planets.
	var extent_radius: float

	## The "inner" extent before P-type planets (for barycenters).
	## Used to calculate where P-type orbits start.
	var inner_extent_radius: float

	## Position of this node's center (calculated during layout).
	var center_position: Vector3

	## Number of S-type planets around this node.
	var stype_planet_count: int

	## Number of P-type planets around this node (for barycenters).
	var ptype_planet_count: int

	## First orbit radius for planets around this node.
	var first_orbit_radius: float

	## For stars: the display radius.
	var star_display_radius: float

	## Binary separation (for barycenters only).
	var binary_separation: float

	## Largest planet display radius orbiting this host.
	var max_planet_radius: float

	## For star nodes: the associated star body ID (for updating host position during animation).
	var star_body_id: String

	## Maximum radius swept by this node when orbiting its parent (extent + orbit_radius).
	var max_sweep_radius: float

	## Distance from this node's center to its parent barycenter (0 for root or single child).
	var orbit_radius_around_parent: float

	func _init(p_id: String = "") -> void:
		node_id = p_id
		extent_radius = 0.0
		inner_extent_radius = 0.0
		center_position = Vector3.ZERO
		stype_planet_count = 0
		ptype_planet_count = 0
		first_orbit_radius = 0.0
		star_display_radius = 0.0
		binary_separation = 0.0
		max_planet_radius = PLANET_MAX_RADIUS
		star_body_id = ""
		max_sweep_radius = 0.0
		orbit_radius_around_parent = 0.0


## Calculated layout data for the entire system.
class SystemLayout:
	extends RefCounted

	## Body layouts by ID.
	var body_layouts: Dictionary # body_id -> BodyLayout

	## Star orbit data for binary stars (star_id -> BodyLayout with orbit info).
	var star_orbits: Dictionary # star_id -> BodyLayout

	## Node extent data by node ID.
	var node_extents: Dictionary # node_id -> NodeExtent

	## Host positions by node ID (for orbit center lookups); updated during animation.
	var host_positions: Dictionary # node_id -> Vector3

	## Mapping from star body ID to node ID (for updating host position when star moves).
	var star_to_node: Dictionary # star_id -> node_id

	## Total system extent (for camera fitting).
	var total_extent: float

	func _init() -> void:
		body_layouts = {}
		star_orbits = {}
		node_extents = {}
		host_positions = {}
		star_to_node = {}
		total_extent = 0.0

	## Gets layout for a body, or null if not found.
	func get_body_layout(body_id: String) -> BodyLayout:
		return body_layouts.get(body_id) as BodyLayout

	## Gets orbit data for a star (if it orbits a barycenter).
	func get_star_orbit(star_id: String) -> BodyLayout:
		return star_orbits.get(star_id) as BodyLayout

	## Gets extent for a node, or null if not found.
	func get_node_extent(node_id: String) -> NodeExtent:
		return node_extents.get(node_id) as NodeExtent

	## Gets position of an orbit host (star node or barycenter).
	func get_host_position(host_id: String) -> Vector3:
		if host_positions.has(host_id):
			return host_positions[host_id]
		return Vector3.ZERO

	## Gets all orbiting body layouts (for animation).
	func get_all_orbiting_bodies() -> Array[BodyLayout]:
		var result: Array[BodyLayout] = []
		for body_id in body_layouts:
			var layout: BodyLayout = body_layouts[body_id]
			if layout.is_orbiting:
				result.append(layout)
		for star_id in star_orbits:
			var layout: BodyLayout = star_orbits[star_id]
			if layout.is_orbiting:
				result.append(layout)
		return result


## Calculates display radius for a star.
## Uses logarithmic scaling: display = base + log10(solar_radii), clamped.
## @param radius_m: Star radius in meters.
## @return: Display radius in Godot units.
static func calculate_star_display_radius(radius_m: float) -> float:
	var solar_radii: float = radius_m / _units.SOLAR_RADIUS_METERS
	solar_radii = maxf(solar_radii, 0.01)

	var display_radius: float = STAR_BASE_RADIUS + log(solar_radii) / log(10.0)
	return clampf(display_radius, STAR_MIN_RADIUS, STAR_MAX_RADIUS)


## Gets the log10 adjustment magnitude for a star (used in orbit calculations).
## @param radius_m: Star radius in meters.
## @return: Absolute value of log10 adjustment.
static func get_star_log_adjustment(radius_m: float) -> float:
	var solar_radii: float = radius_m / _units.SOLAR_RADIUS_METERS
	solar_radii = maxf(solar_radii, 0.01)
	return absf(log(solar_radii) / log(10.0))


## Calculates display radius for a planet.
## Uses logarithmic scaling mapped to [PLANET_MIN_RADIUS, PLANET_MAX_RADIUS] range.
## @param radius_m: Planet radius in meters.
## @return: Display radius in Godot units.
static func calculate_planet_display_radius(radius_m: float) -> float:
	var earth_radii: float = radius_m / _units.EARTH_RADIUS_METERS
	earth_radii = maxf(earth_radii, 0.01)

	var log_radius: float = log(earth_radii) / log(10.0)
	var t: float = (log_radius + 1.0) / 2.5
	var display_radius: float = lerpf(PLANET_MIN_RADIUS, PLANET_MAX_RADIUS, t)

	return clampf(display_radius, PLANET_MIN_RADIUS, PLANET_MAX_RADIUS)


## Calculates the first orbit radius around a star.
## Accounts for star display radius, maximum planet radius, and gap.
## @param star_display_radius: Star display radius in Godot units.
## @param max_planet_radius: Maximum planet display radius.
## @param log_adjustment: The log10 adjustment for the star.
## @return: First orbit radius in Godot units (from star center).
static func calculate_first_orbit_radius_for_star(
	star_display_radius: float,
	max_planet_radius: float,
	log_adjustment: float
) -> float:
	return star_display_radius + max_planet_radius + FIRST_ORBIT_SURFACE_GAP + log_adjustment


## Calculates orbit radius for the nth planet (0-indexed) around a host.
## @param first_orbit_radius: The first orbit radius for this host.
## @param orbit_index: Planet orbit index (0 = innermost).
## @return: Orbit radius in Godot units.
static func calculate_nth_orbit_radius(first_orbit_radius: float, orbit_index: int) -> float:
	return first_orbit_radius + float(orbit_index) * ORBIT_SPACING


## Calculates orbital period based on orbit radius.
## Uses a modified Kepler-like relationship for visual appeal.
## @param orbit_radius: Orbit radius in Godot units.
## @return: Orbital period in seconds.
static func calculate_orbital_period(orbit_radius: float) -> float:
	if orbit_radius <= 0:
		return BASE_ORBITAL_PERIOD
	var normalized_radius: float = orbit_radius / 10.0
	return BASE_ORBITAL_PERIOD * pow(normalized_radius, ORBITAL_PERIOD_EXPONENT)


## Calculates complete layout for a solar system.
## @param system: The solar system to layout.
## @return: SystemLayout with all positions and sizes.
static func calculate_layout(system: SolarSystem) -> SystemLayout:
	var layout: SystemLayout = SystemLayout.new()

	if system == null or system.hierarchy == null or not system.hierarchy.is_valid():
		return layout

	var planets_by_host: Dictionary = _group_planets_by_host(system)
	var max_planet_radii: Dictionary = _calculate_max_planet_radii(system, planets_by_host)

	_calculate_node_extents(system.hierarchy.root, system, planets_by_host, max_planet_radii, layout)
	_position_hierarchy_node(system.hierarchy.root, Vector3.ZERO, null, system, planets_by_host, layout)
	_position_planets(system, planets_by_host, layout)

	_calculate_total_extent(layout)
	return layout


## Groups planets by their orbit host ID.
## @param system: The solar system.
## @return: Dictionary mapping host_id -> Array of planets, sorted by orbital distance.
static func _group_planets_by_host(system: SolarSystem) -> Dictionary:
	var planets_by_host: Dictionary = {}

	for planet in system.get_planets():
		var host_id: String = _find_planet_host_id(planet, system)
		if not planets_by_host.has(host_id):
			planets_by_host[host_id] = []
		planets_by_host[host_id].append(planet)

	for host_id in planets_by_host:
		var planets: Array = planets_by_host[host_id]
		planets.sort_custom(func(a: CelestialBody, b: CelestialBody) -> bool:
			var dist_a: float = 0.0
			if a.has_orbital():
				dist_a = a.orbital.semi_major_axis_m
			var dist_b: float = 0.0
			if b.has_orbital():
				dist_b = b.orbital.semi_major_axis_m
			return dist_a < dist_b
		)

	return planets_by_host


## Finds the orbit host ID for a planet.
## @param planet: The planet.
## @param system: The solar system.
## @return: Host node ID.
static func _find_planet_host_id(planet: CelestialBody, system: SolarSystem) -> String:
	if planet.has_orbital() and not planet.orbital.parent_id.is_empty():
		return planet.orbital.parent_id

	if not system.orbit_hosts.is_empty():
		return system.orbit_hosts[0].node_id

	if system.hierarchy != null and system.hierarchy.root != null:
		return system.hierarchy.root.id

	return ""


## Calculates the maximum planet display radius for each host.
## @param _system: Unused; kept for API consistency.
## @param planets_by_host: Pre-grouped planets.
## @return: Dictionary mapping host_id -> max planet display radius.
static func _calculate_max_planet_radii(_system: SolarSystem, planets_by_host: Dictionary) -> Dictionary:
	var result: Dictionary = {}
	for host_id in planets_by_host:
		var max_radius: float = PLANET_MIN_RADIUS
		var planets: Array = planets_by_host[host_id]
		for planet in planets:
			var p: CelestialBody = planet as CelestialBody
			var display_r: float = calculate_planet_display_radius(p.physical.radius_m)
			max_radius = maxf(max_radius, display_r)
		result[host_id] = max_radius
	return result


## Recursively calculates the extent (radius) of a hierarchy node.
## @param node: Current hierarchy node.
## @param system: The solar system.
## @param planets_by_host: Pre-grouped planets.
## @param max_planet_radii: Max planet radius per host.
## @param layout: Layout being built.
## @return: The extent radius of this node.
static func _calculate_node_extents(
	node: HierarchyNode,
	system: SolarSystem,
	planets_by_host: Dictionary,
	max_planet_radii: Dictionary,
	layout: SystemLayout
) -> float:
	if node == null:
		return 0.0

	var extent: NodeExtent = NodeExtent.new(node.id)
	extent.max_planet_radius = max_planet_radii.get(node.id, PLANET_MAX_RADIUS) as float

	if node.is_star():
		extent.star_body_id = node.star_id
		var star: CelestialBody = system.get_body(node.star_id)
		if star == null:
			extent.star_display_radius = STAR_BASE_RADIUS
			extent.first_orbit_radius = STAR_BASE_RADIUS + PLANET_MAX_RADIUS + FIRST_ORBIT_SURFACE_GAP
			extent.extent_radius = STAR_BASE_RADIUS
			extent.inner_extent_radius = STAR_BASE_RADIUS
		else:
			var star_display: float = calculate_star_display_radius(star.physical.radius_m)
			var log_adj: float = get_star_log_adjustment(star.physical.radius_m)
			var first_orbit: float = calculate_first_orbit_radius_for_star(
				star_display, extent.max_planet_radius, log_adj
			)

			extent.star_display_radius = star_display
			extent.first_orbit_radius = first_orbit

			var stype_planets: Array = planets_by_host.get(node.id, [])
			extent.stype_planet_count = stype_planets.size()

			if extent.stype_planet_count > 0:
				var outermost_orbit: float = calculate_nth_orbit_radius(
					first_orbit, extent.stype_planet_count - 1
				)
				extent.extent_radius = outermost_orbit + extent.max_planet_radius
				extent.inner_extent_radius = extent.extent_radius
			else:
				extent.extent_radius = star_display
				extent.inner_extent_radius = star_display
		extent.max_sweep_radius = extent.extent_radius
		layout.node_extents[node.id] = extent
	else:
		var child_extents: Array[float] = []
		for child in node.children:
			var child_extent_val: float = _calculate_node_extents(child, system, planets_by_host, max_planet_radii, layout)
			child_extents.append(child_extent_val)

		var child_extent_objects: Array[NodeExtent] = []
		for child in node.children:
			var child_ext: NodeExtent = layout.get_node_extent(child.id)
			if child_ext != null:
				child_extent_objects.append(child_ext)

		var masses: Array[float] = []
		var total_mass: float = 0.0
		for child in node.children:
			var mass: float = _get_node_mass(child, system)
			masses.append(mass)
			total_mass += mass

		var separation: float = BINARY_BUFFER_GAP
		for ce in child_extents:
			separation += ce
		extent.binary_separation = separation

		var max_distance_to_edge: float = 0.0

		if node.children.size() >= 2 and total_mass > 0 and child_extent_objects.size() >= 2:
			var mass_a: float = masses[0]
			var mass_b: float = masses[1]
			var ext_a: NodeExtent = child_extent_objects[0]
			var ext_b: NodeExtent = child_extent_objects[1]

			var orbit_radius_a: float = separation * mass_b / total_mass
			var orbit_radius_b: float = separation * mass_a / total_mass
			ext_a.orbit_radius_around_parent = orbit_radius_a
			ext_a.max_sweep_radius = child_extents[0] + orbit_radius_a
			ext_b.orbit_radius_around_parent = orbit_radius_b
			ext_b.max_sweep_radius = child_extents[1] + orbit_radius_b

			var required_separation: float = ext_a.max_sweep_radius + ext_b.max_sweep_radius + BINARY_BUFFER_GAP
			if required_separation > separation:
				extent.binary_separation = required_separation
				separation = required_separation
				orbit_radius_a = separation * mass_b / total_mass
				orbit_radius_b = separation * mass_a / total_mass
				ext_a.orbit_radius_around_parent = orbit_radius_a
				ext_a.max_sweep_radius = child_extents[0] + orbit_radius_a
				ext_b.orbit_radius_around_parent = orbit_radius_b
				ext_b.max_sweep_radius = child_extents[1] + orbit_radius_b

			var offset_a: float = separation * mass_b / total_mass
			var offset_b: float = separation * mass_a / total_mass
			max_distance_to_edge = maxf(offset_a + ext_a.max_sweep_radius, offset_b + ext_b.max_sweep_radius)
			extent.inner_extent_radius = maxf(ext_a.max_sweep_radius, ext_b.max_sweep_radius)
		elif node.children.size() == 1 and child_extent_objects.size() >= 1:
			var single_ext: NodeExtent = child_extent_objects[0]
			single_ext.orbit_radius_around_parent = 0.0
			single_ext.max_sweep_radius = child_extents[0]
			max_distance_to_edge = child_extents[0]
			extent.inner_extent_radius = max_distance_to_edge
		else:
			for ce in child_extents:
				max_distance_to_edge = maxf(max_distance_to_edge, ce + separation / 2.0)
			extent.inner_extent_radius = max_distance_to_edge

		var ptype_planets: Array = planets_by_host.get(node.id, [])
		extent.ptype_planet_count = ptype_planets.size()

		extent.first_orbit_radius = extent.inner_extent_radius + extent.max_planet_radius + PTYPE_BUFFER_GAP

		if extent.ptype_planet_count > 0:
			var outermost_ptype: float = calculate_nth_orbit_radius(
				extent.first_orbit_radius, extent.ptype_planet_count - 1
			)
			extent.extent_radius = outermost_ptype + extent.max_planet_radius
		else:
			extent.extent_radius = extent.inner_extent_radius

		extent.max_sweep_radius = extent.extent_radius
		layout.node_extents[node.id] = extent
	return extent.extent_radius


## Recursively positions hierarchy nodes (stars and barycenters).
## @param node: Current hierarchy node.
## @param center: Center position for this node.
## @param parent_barycenter_id: ID of parent barycenter (for star orbit tracking), or null.
## @param system: The solar system.
## @param planets_by_host: Pre-grouped planets.
## @param layout: Layout being built.
static func _position_hierarchy_node(
	node: HierarchyNode,
	center: Vector3,
	parent_barycenter_id: Variant,
	system: SolarSystem,
	planets_by_host: Dictionary,
	layout: SystemLayout
) -> void:
	if node == null:
		return

	layout.host_positions[node.id] = center

	var extent: NodeExtent = layout.get_node_extent(node.id)
	if extent != null:
		extent.center_position = center

	if node.is_star():
		var star: CelestialBody = system.get_body(node.star_id)
		if star != null:
			var star_layout: BodyLayout = BodyLayout.new(star.id)
			star_layout.position = center
			star_layout.display_radius = extent.star_display_radius if extent else STAR_BASE_RADIUS
			star_layout.orbit_radius = 0.0
			star_layout.is_orbiting = false
			layout.body_layouts[star.id] = star_layout
			layout.star_to_node[star.id] = node.id

			if parent_barycenter_id != null:
				var parent_center: Vector3 = layout.get_host_position(parent_barycenter_id as String)
				var orbit_radius: float = (center - parent_center).length()
				if orbit_radius > 0.01:
					var star_orbit: BodyLayout = BodyLayout.new(star.id)
					star_orbit.orbit_radius = orbit_radius
					star_orbit.orbit_center = parent_center
					star_orbit.orbit_parent_id = parent_barycenter_id as String
					star_orbit.orbital_angle = atan2(center.z - parent_center.z, center.x - parent_center.x)
					star_orbit.orbital_period = calculate_orbital_period(orbit_radius) * 2.0
					star_orbit.is_orbiting = true
					star_orbit.display_radius = star_layout.display_radius
					star_orbit.position = center
					layout.star_orbits[star.id] = star_orbit
					star_layout.is_orbiting = true
					star_layout.orbit_radius = orbit_radius
					star_layout.orbit_center = parent_center
					star_layout.orbit_parent_id = parent_barycenter_id as String
					star_layout.orbital_angle = star_orbit.orbital_angle
					star_layout.orbital_period = star_orbit.orbital_period
	else:
		if node.children.size() >= 2:
			var child_a: HierarchyNode = node.children[0]
			var child_b: HierarchyNode = node.children[1]

			var separation: float = extent.binary_separation if extent else BINARY_BUFFER_GAP * 2

			var mass_a: float = _get_node_mass(child_a, system)
			var mass_b: float = _get_node_mass(child_b, system)
			var total_mass: float = mass_a + mass_b

			var offset_a: float
			var offset_b: float
			if total_mass > 0:
				offset_a = separation * mass_b / total_mass
				offset_b = separation * mass_a / total_mass
			else:
				offset_a = separation * 0.5
				offset_b = separation * 0.5

			var pos_a: Vector3 = center + Vector3(-offset_a, 0, 0)
			var pos_b: Vector3 = center + Vector3(offset_b, 0, 0)

			_position_hierarchy_node(child_a, pos_a, node.id, system, planets_by_host, layout)
			_position_hierarchy_node(child_b, pos_b, node.id, system, planets_by_host, layout)

		elif node.children.size() == 1:
			_position_hierarchy_node(node.children[0], center, node.id, system, planets_by_host, layout)

		for i in range(2, node.children.size()):
			var angle: float = PI * 0.5 + float(i - 2) * PI / float(node.children.size() - 1)
			var child: HierarchyNode = node.children[i]
			var offset: float = (extent.inner_extent_radius if extent else 10.0) * 0.8
			var pos: Vector3 = center + Vector3(cos(angle) * offset, 0, sin(angle) * offset)
			_position_hierarchy_node(child, pos, node.id, system, planets_by_host, layout)


## Gets total mass of a hierarchy node.
## @param node: The hierarchy node.
## @param system: The solar system.
## @return: Total mass in kg.
static func _get_node_mass(node: HierarchyNode, system: SolarSystem) -> float:
	if node.is_star():
		var star: CelestialBody = system.get_body(node.star_id)
		if star != null:
			return star.physical.mass_kg
		return _units.SOLAR_MASS_KG
	else:
		var total: float = 0.0
		for child in node.children:
			total += _get_node_mass(child, system)
		return total


## Positions all planets around their orbit hosts.
## @param system: The solar system.
## @param planets_by_host: Pre-grouped planets.
## @param layout: Layout being built.
static func _position_planets(
	_system: SolarSystem,
	planets_by_host: Dictionary,
	layout: SystemLayout
) -> void:
	for host_id in planets_by_host:
		var planets: Array = planets_by_host[host_id]
		if planets.is_empty():
			continue

		var host_center: Vector3 = layout.get_host_position(host_id)
		var host_extent: NodeExtent = layout.get_node_extent(host_id)

		if host_extent == null:
			push_warning("No extent found for host: %s" % host_id)
			continue

		var first_orbit_radius: float = host_extent.first_orbit_radius

		for i in range(planets.size()):
			var planet: CelestialBody = planets[i]
			var planet_layout: BodyLayout = BodyLayout.new(planet.id)

			var orbit_radius: float = calculate_nth_orbit_radius(first_orbit_radius, i)
			planet_layout.orbit_radius = orbit_radius
			planet_layout.orbit_center = host_center
			planet_layout.orbit_parent_id = host_id
			planet_layout.display_radius = calculate_planet_display_radius(planet.physical.radius_m)

			var angle: float = 0.0
			if planet.has_orbital():
				angle = deg_to_rad(planet.orbital.mean_anomaly_deg)
			else:
				angle = float(i) * TAU / float(maxi(planets.size(), 1))

			planet_layout.orbital_angle = angle
			planet_layout.orbital_period = calculate_orbital_period(orbit_radius)
			planet_layout.is_orbiting = true
			planet_layout.update_position_from_angle()

			layout.body_layouts[planet.id] = planet_layout


## Calculates the total system extent for camera fitting.
## @param layout: The layout to update.
static func _calculate_total_extent(layout: SystemLayout) -> void:
	var max_extent: float = 0.0

	for body_id in layout.body_layouts:
		var body_layout: BodyLayout = layout.body_layouts[body_id]
		var dist: float = body_layout.position.length() + body_layout.display_radius
		max_extent = maxf(max_extent, dist)
		if body_layout.orbit_radius > 0:
			var orbit_edge: float = body_layout.orbit_center.length() + body_layout.orbit_radius + body_layout.display_radius
			max_extent = maxf(max_extent, orbit_edge)

	for star_id in layout.star_orbits:
		var orbit_layout: BodyLayout = layout.star_orbits[star_id]
		if orbit_layout.orbit_radius > 0:
			var orbit_edge: float = orbit_layout.orbit_center.length() + orbit_layout.orbit_radius + orbit_layout.display_radius
			max_extent = maxf(max_extent, orbit_edge)

	for node_id in layout.node_extents:
		var extent: NodeExtent = layout.node_extents[node_id]
		var sweep: float = extent.max_sweep_radius if extent.max_sweep_radius > 0.0 else extent.extent_radius
		var dist: float = extent.center_position.length() + sweep
		max_extent = maxf(max_extent, dist)

	layout.total_extent = maxf(max_extent, 10.0)


## Updates all orbital positions based on elapsed time.
## Stars are updated first; then host_positions for star nodes; then planets so they follow moving stars.
## @param layout: The layout to update.
## @param delta: Time elapsed in seconds.
static func update_orbits(layout: SystemLayout, delta: float) -> void:
	if layout == null:
		return

	# First: update star orbits and sync host positions for star nodes
	for star_id in layout.star_orbits:
		var orbit_layout: BodyLayout = layout.star_orbits[star_id]
		if orbit_layout.is_orbiting and orbit_layout.orbital_period > 0:
			var angular_velocity: float = TAU / orbit_layout.orbital_period
			orbit_layout.orbital_angle += angular_velocity * delta
			orbit_layout.update_position_from_angle()
			var star_body: BodyLayout = layout.body_layouts.get(star_id) as BodyLayout
			if star_body != null:
				star_body.position = orbit_layout.position
				star_body.orbital_angle = orbit_layout.orbital_angle
			var node_id: String = layout.star_to_node.get(star_id, "") as String
			if not node_id.is_empty():
				layout.host_positions[node_id] = orbit_layout.position
				var extent: NodeExtent = layout.node_extents.get(node_id) as NodeExtent
				if extent != null:
					extent.center_position = orbit_layout.position

	# Then: update planet orbits (orbit_center from current host position, then advance angle)
	for body_id in layout.body_layouts:
		var body_layout: BodyLayout = layout.body_layouts[body_id]
		if layout.star_orbits.has(body_id):
			continue
		if body_layout.is_orbiting and body_layout.orbital_period > 0:
			if not body_layout.orbit_parent_id.is_empty():
				body_layout.orbit_center = layout.get_host_position(body_layout.orbit_parent_id)
			var angular_velocity: float = TAU / body_layout.orbital_period
			body_layout.orbital_angle += angular_velocity * delta
			body_layout.update_position_from_angle()
