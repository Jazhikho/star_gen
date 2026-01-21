## Tests for SystemSerializer.
extends TestCase

const _system_serializer := preload("res://src/domain/system/SystemSerializer.gd")
const _solar_system := preload("res://src/domain/system/SolarSystem.gd")
const _system_hierarchy := preload("res://src/domain/system/SystemHierarchy.gd")
const _hierarchy_node := preload("res://src/domain/system/HierarchyNode.gd")
const _orbit_host := preload("res://src/domain/system/OrbitHost.gd")
const _asteroid_belt := preload("res://src/domain/system/AsteroidBelt.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _star_spec := preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_generator := preload("res://src/domain/generation/generators/StarGenerator.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _provenance := preload("res://src/domain/celestial/Provenance.gd")
const _versions := preload("res://src/domain/constants/Versions.gd")


## Creates a test system with various components.
func _create_test_system() -> SolarSystem:
	var system: SolarSystem = SolarSystem.new("test_sys", "Test System")
	
	# Create star
	var star_spec: StarSpec = StarSpec.sun_like(12345)
	var star_rng: SeededRng = SeededRng.new(12345)
	var star: CelestialBody = StarGenerator.generate(star_spec, star_rng)
	star.id = "star_1"
	system.add_body(star)
	
	# Create hierarchy
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star", "star_1")
	system.hierarchy = SystemHierarchy.new(star_node)
	
	# Create planet
	var planet: CelestialBody = CelestialBody.new("planet_1", "Test Planet", CelestialType.Type.PLANET)
	planet.physical = PhysicalProps.new(Units.EARTH_MASS_KG, Units.EARTH_RADIUS_METERS, 86400.0, 23.5)
	planet.orbital = OrbitalProps.new(Units.AU_METERS, 0.017, 0.0, 0.0, 0.0, 0.0, "node_star")
	system.add_body(planet)
	
	# Create moon
	var moon: CelestialBody = CelestialBody.new("moon_1", "Test Moon", CelestialType.Type.MOON)
	moon.physical = PhysicalProps.new(7.34e22, 1.74e6, 2360592.0)
	moon.orbital = OrbitalProps.new(3.84e8, 0.05, 5.0, 0.0, 0.0, 0.0, "planet_1")
	system.add_body(moon)
	
	# Create orbit host
	var host: OrbitHost = OrbitHost.new("node_star", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = star.physical.mass_kg
	host.inner_stability_m = 0.1 * Units.AU_METERS
	host.outer_stability_m = 50.0 * Units.AU_METERS
	system.add_orbit_host(host)
	
	# Create asteroid belt
	var belt: AsteroidBelt = AsteroidBelt.new("belt_1", "Main Belt")
	belt.orbit_host_id = "node_star"
	belt.inner_radius_m = 2.2 * Units.AU_METERS
	belt.outer_radius_m = 3.2 * Units.AU_METERS
	belt.total_mass_kg = 3.0e21
	belt.composition = AsteroidBelt.Composition.ROCKY
	system.add_asteroid_belt(belt)
	
	# Create provenance
	system.provenance = Provenance.new(12345, "0.1.0", 1, 1234567890, {"test": true})
	
	return system


## Tests serialization round-trip via dictionary.
func test_round_trip_dict() -> void:
	var original: SolarSystem = _create_test_system()
	
	var data: Dictionary = SystemSerializer.to_dict(original)
	var restored: SolarSystem = SystemSerializer.from_dict(data)
	
	assert_not_null(restored, "Should deserialize successfully")
	assert_equal(restored.id, original.id)
	assert_equal(restored.name, original.name)
	assert_equal(restored.star_ids.size(), original.star_ids.size())
	assert_equal(restored.planet_ids.size(), original.planet_ids.size())
	assert_equal(restored.moon_ids.size(), original.moon_ids.size())
	assert_equal(restored.asteroid_belts.size(), original.asteroid_belts.size())
	assert_equal(restored.orbit_hosts.size(), original.orbit_hosts.size())


## Tests serialization round-trip via JSON.
func test_round_trip_json() -> void:
	var original: SolarSystem = _create_test_system()
	
	var json_str: String = SystemSerializer.to_json(original)
	var restored: SolarSystem = SystemSerializer.from_json(json_str)
	
	assert_not_null(restored)
	assert_equal(restored.id, original.id)


## Tests hierarchy is preserved.
func test_hierarchy_preserved() -> void:
	var original: SolarSystem = _create_test_system()
	
	var data: Dictionary = SystemSerializer.to_dict(original)
	var restored: SolarSystem = SystemSerializer.from_dict(data)
	
	assert_not_null(restored.hierarchy)
	assert_true(restored.hierarchy.is_valid())
	assert_equal(restored.hierarchy.get_star_count(), original.hierarchy.get_star_count())


## Tests bodies are preserved.
func test_bodies_preserved() -> void:
	var original: SolarSystem = _create_test_system()
	
	var data: Dictionary = SystemSerializer.to_dict(original)
	var restored: SolarSystem = SystemSerializer.from_dict(data)
	
	for body_id in original.bodies:
		assert_true(restored.bodies.has(body_id), "Body %s should be preserved" % body_id)
		
		var orig_body: CelestialBody = original.bodies[body_id]
		var rest_body: CelestialBody = restored.bodies[body_id]
		
		assert_equal(rest_body.type, orig_body.type)
		assert_float_equal(rest_body.physical.mass_kg, orig_body.physical.mass_kg, 1.0)


## Tests orbit hosts are preserved.
func test_orbit_hosts_preserved() -> void:
	var original: SolarSystem = _create_test_system()
	
	var data: Dictionary = SystemSerializer.to_dict(original)
	var restored: SolarSystem = SystemSerializer.from_dict(data)
	
	assert_equal(restored.orbit_hosts.size(), original.orbit_hosts.size())
	
	for i in range(original.orbit_hosts.size()):
		var orig_host: OrbitHost = original.orbit_hosts[i]
		var rest_host: OrbitHost = restored.orbit_hosts[i]
		
		assert_equal(rest_host.node_id, orig_host.node_id)
		assert_float_equal(rest_host.combined_mass_kg, orig_host.combined_mass_kg, 1.0)


## Tests asteroid belts are preserved.
func test_asteroid_belts_preserved() -> void:
	var original: SolarSystem = _create_test_system()
	
	var data: Dictionary = SystemSerializer.to_dict(original)
	var restored: SolarSystem = SystemSerializer.from_dict(data)
	
	assert_equal(restored.asteroid_belts.size(), original.asteroid_belts.size())
	
	var orig_belt: AsteroidBelt = original.asteroid_belts[0]
	var rest_belt: AsteroidBelt = restored.asteroid_belts[0]
	
	assert_equal(rest_belt.id, orig_belt.id)
	assert_equal(rest_belt.composition, orig_belt.composition)
	assert_float_equal(rest_belt.inner_radius_m, orig_belt.inner_radius_m, 1.0)


## Tests provenance is preserved.
func test_provenance_preserved() -> void:
	var original: SolarSystem = _create_test_system()
	
	var data: Dictionary = SystemSerializer.to_dict(original)
	var restored: SolarSystem = SystemSerializer.from_dict(data)
	
	assert_not_null(restored.provenance)
	assert_equal(restored.provenance.generation_seed, original.provenance.generation_seed)
	assert_equal(restored.provenance.generator_version, original.provenance.generator_version)


## Tests deserialization of invalid data returns null.
func test_invalid_data_returns_null() -> void:
	var result: SolarSystem = SystemSerializer.from_dict({})
	assert_null(result, "Empty dict should return null")


## Tests invalid JSON returns null.
func test_invalid_json_returns_null() -> void:
	var result: SolarSystem = SystemSerializer.from_json("not valid json")
	assert_null(result, "Invalid JSON should return null")


## Tests schema version is included.
func test_schema_version_included() -> void:
	var system: SolarSystem = _create_test_system()
	var data: Dictionary = SystemSerializer.to_dict(system)
	
	assert_true(data.has("schema_version"), "Should include schema version")
	assert_true(data.has("generator_version"), "Should include generator version")
	assert_true(data.has("type"), "Should include type")
	assert_equal(data["type"], "solar_system")
