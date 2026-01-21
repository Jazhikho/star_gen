## Tests for SolarSystem.
extends TestCase

const _star_generator := preload("res://src/domain/generation/generators/StarGenerator.gd")
const _star_spec := preload("res://src/domain/generation/specs/StarSpec.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _orbital_props := preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _units := preload("res://src/domain/math/Units.gd")


## Creates a simple test star.
func _create_test_star(star_id: String, star_name: String, seed_val: int) -> CelestialBody:
	var spec: StarSpec = StarSpec.sun_like(seed_val)
	spec.name_hint = star_name
	spec.set_override("id", star_id)
	var rng: SeededRng = SeededRng.new(seed_val)
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	star.id = star_id  # Ensure ID is set
	return star


## Tests basic construction.
func test_construction() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Test System")
	
	assert_equal(system.id, "sys_1")
	assert_equal(system.name, "Test System")
	assert_false(system.is_valid())
	assert_equal(system.get_body_count(), 0)


## Tests adding a star.
func test_add_star() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Sol System")
	var star: CelestialBody = _create_test_star("sol", "Sol", 12345)
	
	# Set up hierarchy
	var star_node: HierarchyNode = HierarchyNode.create_star("node_sol", "sol")
	system.hierarchy = SystemHierarchy.new(star_node)
	
	system.add_body(star)
	
	assert_equal(system.get_star_count(), 1)
	assert_true(system.star_ids.has("sol"))
	assert_not_null(system.get_body("sol"))


## Tests adding multiple body types.
func test_add_multiple_body_types() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Test")
	
	# Create star
	var star: CelestialBody = _create_test_star("star_1", "Star", 111)
	system.add_body(star)
	
	# Create planet (minimal)
	var planet: CelestialBody = CelestialBody.new("planet_1", "Earth", CelestialType.Type.PLANET)
	system.add_body(planet)
	
	# Create moon (minimal)
	var moon: CelestialBody = CelestialBody.new("moon_1", "Luna", CelestialType.Type.MOON)
	moon.orbital = OrbitalProps.new(3.844e8, 0.05, 5.0, 0.0, 0.0, 0.0, "planet_1")
	system.add_body(moon)
	
	# Create asteroid (minimal)
	var asteroid: CelestialBody = CelestialBody.new("asteroid_1", "Ceres", CelestialType.Type.ASTEROID)
	system.add_body(asteroid)
	
	assert_equal(system.get_star_count(), 1)
	assert_equal(system.get_planet_count(), 1)
	assert_equal(system.get_moon_count(), 1)
	assert_equal(system.get_asteroid_count(), 1)
	assert_equal(system.get_body_count(), 4)


## Tests get_moons_of_planet.
func test_get_moons_of_planet() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Test")
	
	# Create planet
	var planet: CelestialBody = CelestialBody.new("earth", "Earth", CelestialType.Type.PLANET)
	system.add_body(planet)
	
	# Create moons for Earth
	var moon1: CelestialBody = CelestialBody.new("luna", "Luna", CelestialType.Type.MOON)
	moon1.orbital = OrbitalProps.new(3.844e8, 0.05, 5.0, 0.0, 0.0, 0.0, "earth")
	system.add_body(moon1)
	
	# Create another planet with its own moon
	var planet2: CelestialBody = CelestialBody.new("mars", "Mars", CelestialType.Type.PLANET)
	system.add_body(planet2)
	
	var moon2: CelestialBody = CelestialBody.new("phobos", "Phobos", CelestialType.Type.MOON)
	moon2.orbital = OrbitalProps.new(9.376e6, 0.01, 1.0, 0.0, 0.0, 0.0, "mars")
	system.add_body(moon2)
	
	var earth_moons: Array[CelestialBody] = system.get_moons_of_planet("earth")
	var mars_moons: Array[CelestialBody] = system.get_moons_of_planet("mars")
	
	assert_equal(earth_moons.size(), 1)
	assert_equal(earth_moons[0].id, "luna")
	assert_equal(mars_moons.size(), 1)
	assert_equal(mars_moons[0].id, "phobos")


## Tests is_valid.
func test_is_valid() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Test")
	
	# No hierarchy, no stars
	assert_false(system.is_valid())
	
	# Add hierarchy but no stars
	var star_node: HierarchyNode = HierarchyNode.create_star("n1", "star_1")
	system.hierarchy = SystemHierarchy.new(star_node)
	assert_false(system.is_valid())  # Still invalid - no star body
	
	# Add star body
	var star: CelestialBody = _create_test_star("star_1", "Star", 123)
	system.add_body(star)
	assert_true(system.is_valid())


## Tests asteroid belt management.
func test_asteroid_belts() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Test")
	
	var belt: AsteroidBelt = AsteroidBelt.new("main_belt", "Main Belt")
	system.add_asteroid_belt(belt)
	
	assert_equal(system.asteroid_belts.size(), 1)
	assert_equal(system.asteroid_belts[0].name, "Main Belt")


## Tests orbit host management.
func test_orbit_hosts() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Test")
	
	var host: OrbitHost = OrbitHost.new("sol", OrbitHost.HostType.S_TYPE)
	system.add_orbit_host(host)
	
	assert_equal(system.orbit_hosts.size(), 1)
	assert_not_null(system.get_orbit_host("sol"))
	assert_null(system.get_orbit_host("nonexistent"))


## Tests get_summary.
func test_get_summary() -> void:
	var system: SolarSystem = SolarSystem.new("sys_1", "Alpha Centauri")
	
	var star: CelestialBody = _create_test_star("alpha_a", "Alpha A", 111)
	system.add_body(star)
	
	var planet: CelestialBody = CelestialBody.new("planet_1", "Planet b", CelestialType.Type.PLANET)
	system.add_body(planet)
	
	var summary: String = system.get_summary()
	
	assert_true(summary.contains("Alpha Centauri"))
	assert_true(summary.contains("1 stars"))
	assert_true(summary.contains("1 planets"))


## Tests serialization round-trip.
func test_round_trip() -> void:
	var original: SolarSystem = SolarSystem.new("sys_test", "Test System")
	
	# Add hierarchy
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "star_b")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", star_a, star_b, 1e11, 0.3)
	original.hierarchy = SystemHierarchy.new(binary)
	
	# Add stars
	var body_a: CelestialBody = _create_test_star("star_a", "Alpha", 111)
	var body_b: CelestialBody = _create_test_star("star_b", "Beta", 222)
	original.add_body(body_a)
	original.add_body(body_b)
	
	# Add planet
	var planet: CelestialBody = CelestialBody.new("planet_1", "Planet I", CelestialType.Type.PLANET)
	planet.physical = PhysicalProps.new(6e24, 6.4e6, 86400.0, 23.5, 0.003, 1e22, 1e13)
	original.add_body(planet)
	
	# Add belt
	var belt: AsteroidBelt = AsteroidBelt.new("belt_1", "Main Belt")
	belt.inner_radius_m = 2.0 * Units.AU_METERS
	belt.outer_radius_m = 3.5 * Units.AU_METERS
	original.add_asteroid_belt(belt)
	
	# Add orbit host
	var host: OrbitHost = OrbitHost.new("binary", OrbitHost.HostType.P_TYPE)
	host.combined_mass_kg = 3e30
	original.add_orbit_host(host)
	
	# Serialize and restore
	var data: Dictionary = original.to_dict()
	var restored: SolarSystem = SolarSystem.from_dict(data)
	
	# Verify
	assert_equal(restored.id, original.id)
	assert_equal(restored.name, original.name)
	assert_equal(restored.get_star_count(), 2)
	assert_equal(restored.get_planet_count(), 1)
	assert_equal(restored.asteroid_belts.size(), 1)
	assert_equal(restored.orbit_hosts.size(), 1)
	assert_true(restored.hierarchy.is_valid())
	assert_equal(restored.hierarchy.get_star_count(), 2)
	
	# Check body restoration
	var restored_star: CelestialBody = restored.get_body("star_a")
	assert_not_null(restored_star)
	assert_equal(restored_star.name, "Alpha")
