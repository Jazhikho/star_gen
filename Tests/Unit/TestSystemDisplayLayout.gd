## Tests for SystemDisplayLayout.
extends TestCase

const _system_display_layout: GDScript = preload("res://src/app/system_viewer/SystemDisplayLayout.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")


## Creates a test star with given radius in solar radii.
## @param id: Star body ID.
## @param solar_radii: Radius in solar radii.
## @param mass_solar: Mass in solar masses; if negative, derived from solar_radii.
## @return: CelestialBody configured as star.
func _create_test_star(id: String, solar_radii: float, mass_solar: float = -1.0) -> CelestialBody:
	var star: CelestialBody = CelestialBody.new()
	star.id = id
	star.name = "Test Star " + id
	star.type = CelestialType.Type.STAR
	star.physical = PhysicalProps.new()
	star.physical.radius_m = solar_radii * _units.SOLAR_RADIUS_METERS
	if mass_solar < 0:
		star.physical.mass_kg = _units.SOLAR_MASS_KG * pow(solar_radii, 2.0)
	else:
		star.physical.mass_kg = _units.SOLAR_MASS_KG * mass_solar
	return star


## Creates a test planet with given radius in Earth radii.
## @param id: Planet body ID.
## @param earth_radii: Radius in Earth radii.
## @param parent_id: Orbit host node ID.
## @param sma_au: Semi-major axis in AU.
## @return: CelestialBody configured as planet.
func _create_test_planet(id: String, earth_radii: float, parent_id: String, sma_au: float) -> CelestialBody:
	var planet: CelestialBody = CelestialBody.new()
	planet.id = id
	planet.name = "Test Planet " + id
	planet.type = CelestialType.Type.PLANET
	planet.physical = PhysicalProps.new()
	planet.physical.radius_m = earth_radii * _units.EARTH_RADIUS_METERS
	planet.physical.mass_kg = _units.EARTH_MASS_KG * pow(earth_radii, 2.5)
	planet.orbital = OrbitalProps.new()
	planet.orbital.parent_id = parent_id
	planet.orbital.semi_major_axis_m = sma_au * _units.AU_METERS
	planet.orbital.mean_anomaly_deg = 0.0
	return planet


## Creates a test asteroid belt.
func _create_test_belt(id: String, host_id: String, inner_au: float, outer_au: float) -> AsteroidBelt:
	var belt: AsteroidBelt = AsteroidBelt.new(id, "Test Belt")
	belt.orbit_host_id = host_id
	belt.inner_radius_m = inner_au * _units.AU_METERS
	belt.outer_radius_m = outer_au * _units.AU_METERS
	belt.total_mass_kg = 1.0e21
	return belt


## Tests star display radius for sun-like star.
func test_star_display_radius_solar() -> void:
	var radius: float = _system_display_layout.calculate_star_display_radius(_units.SOLAR_RADIUS_METERS)

	assert_float_equal(radius, 3.0, 0.01, "Sun should have display radius of 3 units")


## Tests star display radius for small red dwarf.
func test_star_display_radius_red_dwarf() -> void:
	var radius: float = _system_display_layout.calculate_star_display_radius(0.1 * _units.SOLAR_RADIUS_METERS)

	assert_float_equal(radius, 2.0, 0.01, "0.1 solar radius star should be 2 units")


## Tests star display radius for large star.
func test_star_display_radius_giant() -> void:
	var radius: float = _system_display_layout.calculate_star_display_radius(100.0 * _units.SOLAR_RADIUS_METERS)

	assert_float_equal(radius, 5.0, 0.01, "100 solar radius star should be 5 units")


## Tests star display radius clamping at maximum.
func test_star_display_radius_max_clamp() -> void:
	var radius: float = _system_display_layout.calculate_star_display_radius(1000000.0 * _units.SOLAR_RADIUS_METERS)

	assert_float_equal(radius, 9.0, 0.01, "Very large star should be clamped to 9 units")


## Tests star display radius clamping at minimum.
func test_star_display_radius_min_clamp() -> void:
	var radius: float = _system_display_layout.calculate_star_display_radius(0.001 * _units.SOLAR_RADIUS_METERS)

	assert_float_equal(radius, 1.0, 0.01, "Very small star should be clamped to 1 unit")


## Tests planet display radius for Earth.
func test_planet_display_radius_earth() -> void:
	var radius: float = _system_display_layout.calculate_planet_display_radius(_units.EARTH_RADIUS_METERS)

	assert_greater_than(radius, 0.5)
	assert_less_than(radius, 1.5)


## Tests planet display radius for Jupiter-sized.
func test_planet_display_radius_jupiter() -> void:
	var jupiter_radii: float = 11.2
	var radius: float = _system_display_layout.calculate_planet_display_radius(jupiter_radii * _units.EARTH_RADIUS_METERS)

	assert_greater_than(radius, 1.5)
	assert_true(radius <= 2.0, "Jupiter-sized planet should be at most 2.0 units")


## Tests planet display radius for small dwarf planet.
func test_planet_display_radius_dwarf() -> void:
	var radius: float = _system_display_layout.calculate_planet_display_radius(0.1 * _units.EARTH_RADIUS_METERS)

	assert_float_equal(radius, 0.25, 0.1, "Small planet should be near minimum")


## Tests first orbit radius calculation for solar star (surface gap formula).
func test_first_orbit_radius_solar() -> void:
	var star_display: float = 3.0
	var max_planet_radius: float = 2.0
	var log_adj: float = 0.0
	var orbit: float = _system_display_layout.calculate_first_orbit_radius_for_star(
		star_display, max_planet_radius, log_adj
	)

	# First orbit = star + max_planet + FIRST_ORBIT_SURFACE_GAP (4) + log_adj = 3 + 2 + 4 + 0 = 9
	assert_float_equal(orbit, 9.0, 0.01, "First orbit around sun should be at 9 units")


## Tests first orbit radius for larger star.
func test_first_orbit_radius_giant() -> void:
	var star_display: float = 5.0
	var max_planet_radius: float = 2.0
	var log_adj: float = 2.0
	var orbit: float = _system_display_layout.calculate_first_orbit_radius_for_star(
		star_display, max_planet_radius, log_adj
	)

	# First orbit = 5 + 2 + 4 + 2 = 13
	assert_float_equal(orbit, 13.0, 0.01, "First orbit around giant star should be at 13 units")


## Tests subsequent orbit spacing.
func test_orbit_spacing() -> void:
	var first_orbit: float = 6.0
	var second: float = _system_display_layout.calculate_nth_orbit_radius(first_orbit, 1)
	var third: float = _system_display_layout.calculate_nth_orbit_radius(first_orbit, 2)

	assert_float_equal(second - first_orbit, 6.0, 0.01, "Orbit spacing should be 6 units")
	assert_float_equal(third - second, 6.0, 0.01, "Orbit spacing should be consistent")


## Tests belt layouts are generated with display radii and AU metadata.
func test_belt_layout_generated() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Belt System")
	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)
	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)
	system.add_asteroid_belt(_create_test_belt("belt_0", "node_star_0", 2.0, 3.0))
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var belt_layout: RefCounted = layout.get_belt_layout("belt_0")
	assert_not_null(belt_layout, "Belt layout should be created")
	assert_greater_than(belt_layout.center_display_radius, 0.0, "Display center radius should be positive")
	assert_greater_than(belt_layout.outer_display_radius, belt_layout.inner_display_radius, "Outer display radius > inner")
	assert_float_equal(belt_layout.inner_au, 2.0, 0.001, "Inner AU metadata should be preserved")
	assert_float_equal(belt_layout.outer_au, 3.0, 0.001, "Outer AU metadata should be preserved")


## Tests belt inclination cap scales up with display distance.
func test_belt_inclination_scales_with_distance() -> void:
	var near_value: float = _system_display_layout.calculate_belt_max_inclination_deg(10.0)
	var far_value: float = _system_display_layout.calculate_belt_max_inclination_deg(60.0)
	assert_greater_than(far_value, near_value, "Farther belts should allow greater inclination")


## Tests single star system layout.
func test_single_star_layout() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	assert_not_null(layout)
	assert_true(layout.body_layouts.has("star_0"))

	var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	assert_equal(star_layout.position, Vector3.ZERO, "Single star should be at origin")
	assert_float_equal(star_layout.display_radius, 3.0, 0.01)


## Tests single star with planets layout.
func test_single_star_with_planets_layout() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	var planet1: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	var planet2: CelestialBody = _create_test_planet("planet_1", 5.0, "node_star_0", 5.0)
	system.add_body(planet1)
	system.add_body(planet2)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var p1_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")
	var p2_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_1")

	assert_not_null(p1_layout)
	assert_not_null(p2_layout)

	var extent: SystemDisplayLayout.NodeExtent = layout.get_node_extent("node_star_0")
	var first_orbit: float = extent.first_orbit_radius
	assert_float_equal(p1_layout.orbit_radius, first_orbit, 0.01, "First planet at first orbit")

	# Second planet one ORBIT_SPACING (6) further out
	assert_float_equal(p2_layout.orbit_radius, first_orbit + 6.0, 0.01)

	assert_not_equal(p1_layout.position, Vector3.ZERO)
	assert_equal(p1_layout.orbit_center, Vector3.ZERO, "Orbit center should be at star position")


## Tests star extent with planets.
func test_star_extent_with_planets() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	for i in range(3):
		var planet: CelestialBody = _create_test_planet("planet_%d" % i, 1.0, "node_star_0", float(i + 1))
		system.add_body(planet)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var extent: SystemDisplayLayout.NodeExtent = layout.get_node_extent("node_star_0")
	assert_not_null(extent)
	# Extent = outermost orbit + max_planet_radius (from 3 planets at 1 R⊕ each)
	var outermost_orbit: float = _system_display_layout.calculate_nth_orbit_radius(
		extent.first_orbit_radius, 2
	)
	assert_float_equal(
		extent.extent_radius,
		outermost_orbit + extent.max_planet_radius,
		0.02,
		"Extent = outermost orbit + max planet radius"
	)
	assert_equal(extent.stype_planet_count, 3)


## Tests binary star layout with no planets.
func test_binary_star_layout_no_planets() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary System")

	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 0.5, 0.5)
	system.add_body(star_a)
	system.add_body(star_b)

	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS * 10.0, 0.3
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var star_a_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var star_b_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_1")

	assert_not_null(star_a_layout)
	assert_not_null(star_b_layout)

	assert_not_equal(star_a_layout.position, star_b_layout.position)

	assert_less_than(star_a_layout.position.length(), star_b_layout.position.length(),
		"Heavier star should be closer to barycenter")

	var separation: float = (star_a_layout.position - star_b_layout.position).length()
	var min_separation: float = star_a_layout.display_radius + star_b_layout.display_radius
	assert_greater_than(separation, min_separation, "Stars should not overlap")


## Tests that first orbit is always outside star display radius (solar).
func test_first_orbit_outside_star_solar() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")

	assert_greater_than(
		planet_layout.orbit_radius,
		star_layout.display_radius + 1.0,
		"First orbit should be clearly outside star surface"
	)


## Tests that first orbit is outside star for giant star.
func test_first_orbit_outside_giant_star() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 100.0)
	system.add_body(star)

	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")

	assert_greater_than(
		planet_layout.orbit_radius,
		star_layout.display_radius + 1.0,
		"First orbit should be clearly outside giant star surface"
	)


## Tests that first orbit is outside star for red dwarf.
func test_first_orbit_outside_red_dwarf() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 0.1)
	system.add_body(star)

	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 0.1)
	system.add_body(planet)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")

	assert_greater_than(
		planet_layout.orbit_radius,
		star_layout.display_radius + 1.0,
		"First orbit should be clearly outside red dwarf surface"
	)


## Tests binary star layout with S-type planets - no orbital overlap.
func test_binary_with_stype_planets_no_overlap() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary System")

	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 1.0, 1.0)
	system.add_body(star_a)
	system.add_body(star_b)

	for i in range(3):
		var planet: CelestialBody = _create_test_planet("planet_%d" % i, 1.0, "node_star_0", float(i + 1))
		system.add_body(planet)

	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS * 10.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var star_a_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var star_b_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_1")
	var p2_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_2")

	var planet_orbit_outer_edge_x: float = star_a_layout.position.x + p2_layout.orbit_radius
	var star_b_inner_edge_x: float = star_b_layout.position.x - star_b_layout.display_radius

	assert_less_than(
		planet_orbit_outer_edge_x,
		star_b_inner_edge_x,
		"Planet orbits around star A should not reach star B"
	)


## Tests binary with P-type planets.
func test_binary_with_ptype_planets() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary System")

	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 1.0, 1.0)
	system.add_body(star_a)
	system.add_body(star_b)

	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS * 10.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var ptype_planet: CelestialBody = _create_test_planet("planet_ptype", 5.0, "barycenter_0", 20.0)
	system.add_body(ptype_planet)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var ptype_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_ptype")
	assert_not_null(ptype_layout)

	assert_equal(ptype_layout.orbit_center, Vector3.ZERO)

	var star_a_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var star_b_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_1")

	var max_star_edge: float = maxf(
		star_a_layout.position.length() + star_a_layout.display_radius,
		star_b_layout.position.length() + star_b_layout.display_radius
	)

	assert_greater_than(ptype_layout.orbit_radius, max_star_edge,
		"P-type orbit should be outside both stars")


## Tests quadruple star system (2 binaries).
func test_quadruple_star_layout() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Quadruple System")

	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 0.8, 0.8)
	var star_c: CelestialBody = _create_test_star("star_2", 0.6, 0.6)
	var star_d: CelestialBody = _create_test_star("star_3", 0.5, 0.5)
	system.add_body(star_a)
	system.add_body(star_b)
	system.add_body(star_c)
	system.add_body(star_d)

	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var inner_binary_1: HierarchyNode = HierarchyNode.create_barycenter(
		"inner_binary_1", node_a, node_b, _units.AU_METERS * 5.0, 0.0
	)

	var node_c: HierarchyNode = HierarchyNode.create_star("node_star_2", "star_2")
	var node_d: HierarchyNode = HierarchyNode.create_star("node_star_3", "star_3")
	var inner_binary_2: HierarchyNode = HierarchyNode.create_barycenter(
		"inner_binary_2", node_c, node_d, _units.AU_METERS * 4.0, 0.0
	)

	var outer_binary: HierarchyNode = HierarchyNode.create_barycenter(
		"outer_binary", inner_binary_1, inner_binary_2, _units.AU_METERS * 50.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(outer_binary)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	assert_true(layout.body_layouts.has("star_0"))
	assert_true(layout.body_layouts.has("star_1"))
	assert_true(layout.body_layouts.has("star_2"))
	assert_true(layout.body_layouts.has("star_3"))

	var positions: Array[Vector3] = []
	var radii: Array[float] = []
	for i in range(4):
		var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_%d" % i)
		positions.append(star_layout.position)
		radii.append(star_layout.display_radius)

	for i in range(4):
		for j in range(i + 1, 4):
			var dist: float = (positions[i] - positions[j]).length()
			var min_dist: float = radii[i] + radii[j]
			assert_greater_than(dist, min_dist, "Stars %d and %d should not overlap" % [i, j])


## Tests quadruple with planets around each component.
func test_quadruple_with_planets_no_overlap() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Quadruple System")

	for i in range(4):
		var star: CelestialBody = _create_test_star("star_%d" % i, 1.0, 1.0)
		system.add_body(star)

	for i in range(4):
		for j in range(2):
			var planet: CelestialBody = _create_test_planet(
				"planet_%d_%d" % [i, j], 1.0, "node_star_%d" % i, float(j + 1)
			)
			system.add_body(planet)

	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var inner_binary_1: HierarchyNode = HierarchyNode.create_barycenter(
		"inner_binary_1", node_a, node_b, _units.AU_METERS * 5.0, 0.0
	)

	var node_c: HierarchyNode = HierarchyNode.create_star("node_star_2", "star_2")
	var node_d: HierarchyNode = HierarchyNode.create_star("node_star_3", "star_3")
	var inner_binary_2: HierarchyNode = HierarchyNode.create_barycenter(
		"inner_binary_2", node_c, node_d, _units.AU_METERS * 4.0, 0.0
	)

	var outer_binary: HierarchyNode = HierarchyNode.create_barycenter(
		"outer_binary", inner_binary_1, inner_binary_2, _units.AU_METERS * 50.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(outer_binary)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	for i in range(4):
		var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_%d" % i)
		var star_pos: Vector3 = star_layout.position

		var outer_planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_%d_1" % i)
		var orbit_radius: float = outer_planet_layout.orbit_radius

		for j in range(4):
			if i == j:
				continue
			var other_star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_%d" % j)
			var other_pos: Vector3 = other_star_layout.position

			var dist_to_other: float = (star_pos - other_pos).length()

			assert_greater_than(
				dist_to_other,
				orbit_radius + other_star_layout.display_radius,
				"Planets around star %d should not overlap with star %d" % [i, j]
			)


## Tests total extent calculation for single star.
func test_total_extent_single_star() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	assert_greater_than(layout.total_extent, 0.0)
	assert_true(layout.total_extent >= 3.0, "Total extent should be at least star radius")


## Tests total extent includes all planets.
func test_total_extent_includes_planets() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	for i in range(5):
		var planet: CelestialBody = _create_test_planet("planet_%d" % i, 1.0, "node_star_0", float(i + 1))
		system.add_body(planet)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var outermost_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_4")

	assert_true(layout.total_extent >= outermost_layout.orbit_radius,
		"Total extent should include outermost planet orbit")


## Tests that barycenter host position is recorded.
func test_barycenter_position_recorded() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary System")

	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 1.0, 1.0)
	system.add_body(star_a)
	system.add_body(star_b)

	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS, 0.0
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	assert_true(layout.host_positions.has("barycenter_0"))
	assert_equal(layout.get_host_position("barycenter_0"), Vector3.ZERO)


## Tests host position lookup.
func test_host_position_lookup() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var host_pos: Vector3 = layout.get_host_position("node_star_0")
	assert_equal(host_pos, Vector3.ZERO)


## Tests host position lookup for missing ID.
func test_host_position_lookup_missing() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")

	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)

	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var missing_pos: Vector3 = layout.get_host_position("nonexistent")
	assert_equal(missing_pos, Vector3.ZERO)


## Tests layout with null system.
func test_layout_null_system() -> void:
	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(null)

	assert_not_null(layout)
	assert_true(layout.body_layouts.is_empty())


## Tests layout with empty hierarchy.
func test_layout_empty_hierarchy() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Empty")

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	assert_not_null(layout)
	assert_true(layout.body_layouts.is_empty())


## Tests log adjustment calculation.
func test_log_adjustment() -> void:
	var adj_solar: float = _system_display_layout.get_star_log_adjustment(_units.SOLAR_RADIUS_METERS)
	var adj_small: float = _system_display_layout.get_star_log_adjustment(0.1 * _units.SOLAR_RADIUS_METERS)
	var adj_large: float = _system_display_layout.get_star_log_adjustment(100.0 * _units.SOLAR_RADIUS_METERS)

	assert_float_equal(adj_solar, 0.0, 0.01, "Solar radius should have 0 adjustment")
	assert_float_equal(adj_small, 1.0, 0.01, "0.1 solar radius should have adjustment of 1")
	assert_float_equal(adj_large, 2.0, 0.01, "100 solar radius should have adjustment of 2")


## Tests first orbit uses surface separation (star surface + gap + planet radius).
func test_first_orbit_surface_separation_solar() -> void:
	var star_display: float = 3.0
	var max_planet_radius: float = 2.0
	var log_adj: float = 0.0
	var first: float = _system_display_layout.calculate_first_orbit_radius_for_star(
		star_display, max_planet_radius, log_adj
	)
	# First orbit = star_display + max_planet_radius + FIRST_ORBIT_SURFACE_GAP (4) + log_adj = 9
	assert_float_equal(first, 9.0, 0.01)
	assert_greater_than(first, star_display + max_planet_radius, "Gap must separate star+planet from orbit")


## Tests first orbit accounts for large planet (max_planet_radius).
func test_first_orbit_accounts_for_planet_size() -> void:
	var star_display: float = 3.0
	var log_adj: float = 0.0
	var small_max: float = 0.5
	var large_max: float = 3.0
	var first_small: float = _system_display_layout.calculate_first_orbit_radius_for_star(
		star_display, small_max, log_adj
	)
	var first_large: float = _system_display_layout.calculate_first_orbit_radius_for_star(
		star_display, large_max, log_adj
	)
	assert_greater_than(first_large, first_small, "Larger max planet radius should push first orbit out")


## Tests all planets are clear of star surface.
func test_all_planets_clear_of_star() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")
	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)
	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")

	var min_clear: float = star_layout.display_radius + planet_layout.display_radius + 2.0
	assert_true(
		planet_layout.orbit_radius >= min_clear,
		"Planet orbit must clear star surface + planet radius + gap"
	)


## Tests first orbit for giant star with small planet.
func test_first_orbit_giant_star_small_planet() -> void:
	var star_display: float = 9.0
	var max_planet_radius: float = 0.25
	var log_adj: float = 2.0
	var first: float = _system_display_layout.calculate_first_orbit_radius_for_star(
		star_display, max_planet_radius, log_adj
	)
	# 9 + 0.25 + 4 + 2 = 15.25
	assert_float_equal(first, 15.25, 0.01)


## Tests binary stars have orbit entries (star_orbits).
func test_binary_stars_have_orbits() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary System")
	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 0.5, 0.5)
	system.add_body(star_a)
	system.add_body(star_b)
	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS * 10.0, 0.3
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	assert_true(layout.star_orbits.has("star_0"))
	assert_true(layout.star_orbits.has("star_1"))
	var orbit_a: SystemDisplayLayout.BodyLayout = layout.get_star_orbit("star_0")
	var orbit_b: SystemDisplayLayout.BodyLayout = layout.get_star_orbit("star_1")
	assert_true(orbit_a.is_orbiting)
	assert_true(orbit_b.is_orbiting)
	assert_greater_than(orbit_a.orbit_radius, 0.0)
	assert_greater_than(orbit_b.orbit_radius, 0.0)


## Tests single star has no star orbit (only root star at origin).
func test_single_star_no_orbit() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")
	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	var orbit: SystemDisplayLayout.BodyLayout = layout.get_star_orbit("star_0")
	assert_null(orbit, "Single root star should not have a star_orbits entry")


## Tests orbital periods are calculated for planets and orbiting stars.
func test_orbital_periods_calculated() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")
	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)
	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")

	assert_true(planet_layout.is_orbiting)
	assert_greater_than(planet_layout.orbital_period, 0.0, "Planet should have positive orbital period")


## Tests update_orbits advances positions.
func test_update_orbits_changes_positions() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")
	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)
	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")
	var pos_before: Vector3 = planet_layout.position

	_system_display_layout.update_orbits(layout, 1.0)

	var pos_after: Vector3 = planet_layout.position
	assert_not_equal(pos_before, pos_after, "Position should change after update_orbits")


## Tests orbit radius stays constant during animation.
func test_orbit_radius_constant_during_animation() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")
	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)
	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")
	var radius_before: float = planet_layout.orbit_radius

	_system_display_layout.update_orbits(layout, 10.0)

	assert_float_equal(planet_layout.orbit_radius, radius_before, 0.01, "Orbit radius must not change during animation")


## Tests that planets follow their parent star when the star moves (binary with S-type planet).
func test_planets_follow_orbiting_star() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary with planets")
	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 1.0, 1.0)
	system.add_body(star_a)
	system.add_body(star_b)
	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)
	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS * 10.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var star_initial: Vector3 = layout.get_body_layout("star_0").position
	var planet_initial: Vector3 = layout.get_body_layout("planet_0").position

	for _i in range(50):
		_system_display_layout.update_orbits(layout, 0.1)

	var star_new: Vector3 = layout.get_body_layout("star_0").position
	var planet_new: Vector3 = layout.get_body_layout("planet_0").position

	assert_not_equal(star_initial, star_new, "Star should have moved during animation")
	assert_not_equal(planet_initial, planet_new, "Planet should have moved during animation")

	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")
	var dist_from_center: float = (planet_new - planet_layout.orbit_center).length()
	assert_float_equal(
		dist_from_center,
		planet_layout.orbit_radius,
		0.1,
		"Planet should maintain orbit radius from its moving center"
	)


## Tests that planet orbit_center matches star position after each update.
func test_planet_orbit_center_follows_star() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary with planets")
	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 0.5, 0.5)
	system.add_body(star_a)
	system.add_body(star_b)
	var planet: CelestialBody = _create_test_planet("planet_0", 1.0, "node_star_0", 1.0)
	system.add_body(planet)
	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS * 10.0, 0.3
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	for _i in range(100):
		_system_display_layout.update_orbits(layout, 0.05)
		var star_pos: Vector3 = layout.get_body_layout("star_0").position
		var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")
		assert_float_equal(
			planet_layout.orbit_center.x, star_pos.x, 0.01,
			"Planet orbit center X should match star position"
		)
		assert_float_equal(
			planet_layout.orbit_center.z, star_pos.z, 0.01,
			"Planet orbit center Z should match star position"
		)


## Tests that host_positions is updated during animation for star nodes.
func test_host_positions_updated_during_animation() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Binary")
	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 1.0, 1.0)
	system.add_body(star_a)
	system.add_body(star_b)
	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
		"barycenter_0", node_a, node_b, _units.AU_METERS * 10.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(barycenter)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var initial_host_pos: Vector3 = layout.get_host_position("node_star_0")

	for _i in range(50):
		_system_display_layout.update_orbits(layout, 0.1)

	var new_host_pos: Vector3 = layout.get_host_position("node_star_0")
	assert_not_equal(initial_host_pos, new_host_pos, "Host position should update during animation")

	var star_body_pos: Vector3 = layout.get_body_layout("star_0").position
	assert_equal(new_host_pos, star_body_pos, "Host position should match star body position")


## Tests triple system: planets around different stars maintain distance from their host.
func test_triple_system_planets_follow() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Triple with planets")
	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 0.8, 0.8)
	var star_c: CelestialBody = _create_test_star("star_2", 0.5, 0.5)
	system.add_body(star_a)
	system.add_body(star_b)
	system.add_body(star_c)
	var planet_a: CelestialBody = _create_test_planet("planet_a", 1.0, "node_star_0", 1.0)
	system.add_body(planet_a)
	var planet_c: CelestialBody = _create_test_planet("planet_c", 1.0, "node_star_2", 1.0)
	system.add_body(planet_c)
	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var inner_binary: HierarchyNode = HierarchyNode.create_barycenter(
		"inner_binary", node_a, node_b, _units.AU_METERS * 5.0, 0.0
	)
	var node_c: HierarchyNode = HierarchyNode.create_star("node_star_2", "star_2")
	var outer: HierarchyNode = HierarchyNode.create_barycenter(
		"outer", inner_binary, node_c, _units.AU_METERS * 50.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(outer)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)

	for _i in range(100):
		_system_display_layout.update_orbits(layout, 0.05)
		var star_a_pos: Vector3 = layout.get_body_layout("star_0").position
		var planet_a_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_a")
		var dist_a: float = (planet_a_layout.position - star_a_pos).length()
		assert_float_equal(dist_a, planet_a_layout.orbit_radius, 0.1, "Planet A should maintain distance from star A")
		var star_c_pos: Vector3 = layout.get_body_layout("star_2").position
		var planet_c_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_c")
		var dist_c: float = (planet_c_layout.position - star_c_pos).length()
		assert_float_equal(dist_c, planet_c_layout.orbit_radius, 0.1, "Planet C should maintain distance from star C")


## Tests triple (A+B)+C: after many update_orbits steps, no two bodies overlap (sweep-based separation).
func test_triple_system_no_overlap_after_animation() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Triple no-overlap")
	var star_a: CelestialBody = _create_test_star("star_0", 1.0, 1.0)
	var star_b: CelestialBody = _create_test_star("star_1", 0.8, 0.8)
	var star_c: CelestialBody = _create_test_star("star_2", 0.5, 0.5)
	system.add_body(star_a)
	system.add_body(star_b)
	system.add_body(star_c)
	var planet_a: CelestialBody = _create_test_planet("planet_a", 1.0, "node_star_0", 1.0)
	system.add_body(planet_a)
	var planet_b: CelestialBody = _create_test_planet("planet_b", 1.0, "node_star_1", 1.0)
	system.add_body(planet_b)
	var planet_c: CelestialBody = _create_test_planet("planet_c", 1.0, "node_star_2", 1.0)
	system.add_body(planet_c)
	var node_a: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	var node_b: HierarchyNode = HierarchyNode.create_star("node_star_1", "star_1")
	var inner_binary: HierarchyNode = HierarchyNode.create_barycenter(
		"inner_binary", node_a, node_b, _units.AU_METERS * 5.0, 0.0
	)
	var node_c: HierarchyNode = HierarchyNode.create_star("node_star_2", "star_2")
	var outer: HierarchyNode = HierarchyNode.create_barycenter(
		"outer", inner_binary, node_c, _units.AU_METERS * 50.0, 0.0
	)
	system.hierarchy = SystemHierarchy.new(outer)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	const STEPS: int = 200
	const STEP_DT: float = 0.05
	const MIN_CLEARANCE: float = 0.5

	for _k in range(STEPS):
		_system_display_layout.update_orbits(layout, STEP_DT)
		var body_ids: Array[String] = []
		for bid in layout.body_layouts:
			body_ids.append(bid)
		for i in range(body_ids.size()):
			var layout_a: SystemDisplayLayout.BodyLayout = layout.get_body_layout(body_ids[i])
			if layout_a == null:
				continue
			var pos_a: Vector3 = layout_a.position
			var rad_a: float = layout_a.display_radius
			for j in range(i + 1, body_ids.size()):
				var layout_b: SystemDisplayLayout.BodyLayout = layout.get_body_layout(body_ids[j])
				if layout_b == null:
					continue
				var dist: float = (pos_a - layout_b.position).length()
				var min_dist: float = rad_a + layout_b.display_radius + MIN_CLEARANCE
				assert_true(
					dist >= min_dist - 0.01,
					"At step %d: %s and %s too close (dist=%.2f, min=%.2f)" % [_k + 1, body_ids[i], body_ids[j], dist, min_dist]
				)


## Tests first orbit surface gap is at least FIRST_ORBIT_SURFACE_GAP.
func test_first_orbit_minimum_surface_gap() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test System")
	var star: CelestialBody = _create_test_star("star_0", 1.0)
	system.add_body(star)
	var planet: CelestialBody = _create_test_planet("planet_0", 11.0, "node_star_0", 1.0)
	system.add_body(planet)
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
	system.hierarchy = SystemHierarchy.new(star_node)

	var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
	var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
	var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")

	var planet_inner_surface: float = planet_layout.orbit_radius - planet_layout.display_radius
	var actual_gap: float = planet_inner_surface - star_layout.display_radius

	assert_true(
		actual_gap >= SystemDisplayLayout.FIRST_ORBIT_SURFACE_GAP - 0.1,
		"Surface gap should be at least FIRST_ORBIT_SURFACE_GAP (%.1f)" % SystemDisplayLayout.FIRST_ORBIT_SURFACE_GAP
	)


## Tests planet inner edge stays outside star outer edge for several star/planet size combinations.
func test_first_orbit_visual_clearance() -> void:
	var test_cases: Array = [
		{"star_r": 1.0, "planet_r": 11.0},
		{"star_r": 0.1, "planet_r": 1.0},
		{"star_r": 100.0, "planet_r": 1.0},
		{"star_r": 1.0, "planet_r": 0.5},
	]
	for test_case in test_cases:
		var star_r: float = test_case["star_r"]
		var planet_r: float = test_case["planet_r"]
		var system: SolarSystem = SolarSystem.new("test", "Test")
		var star: CelestialBody = _create_test_star("star_0", star_r)
		system.add_body(star)
		var planet: CelestialBody = _create_test_planet("planet_0", planet_r, "node_star_0", 1.0)
		system.add_body(planet)
		var star_node: HierarchyNode = HierarchyNode.create_star("node_star_0", "star_0")
		system.hierarchy = SystemHierarchy.new(star_node)

		var layout: SystemDisplayLayout.SystemLayout = _system_display_layout.calculate_layout(system)
		var star_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("star_0")
		var planet_layout: SystemDisplayLayout.BodyLayout = layout.get_body_layout("planet_0")

		var planet_inner_edge: float = planet_layout.orbit_radius - planet_layout.display_radius
		var star_outer_edge: float = star_layout.display_radius

		assert_greater_than(
			planet_inner_edge,
			star_outer_edge,
			"Planet (r=%.1f) inner edge should be outside star (r=%.1f) outer edge" % [planet_r, star_r]
		)
