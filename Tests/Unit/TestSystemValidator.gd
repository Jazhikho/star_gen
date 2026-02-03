## Tests for SystemValidator.
extends TestCase

const _system_validator: GDScript = preload("res://src/domain/system/SystemValidator.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _validation_result: GDScript = preload("res://src/domain/celestial/validation/ValidationResult.gd")


## Creates a minimal valid system for testing.
func _create_valid_system() -> SolarSystem:
	var system: SolarSystem = SolarSystem.new("test_system", "Test System")
	
	# Create star
	var star_spec: StarSpec = StarSpec.sun_like(12345)
	var star_rng: SeededRng = SeededRng.new(12345)
	var star: CelestialBody = StarGenerator.generate(star_spec, star_rng)
	star.id = "star_1"
	system.add_body(star)
	
	# Create hierarchy
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star", "star_1")
	system.hierarchy = SystemHierarchy.new(star_node)
	
	# Create orbit host
	var host: OrbitHost = OrbitHost.new("node_star", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = star.physical.mass_kg
	host.combined_luminosity_watts = star.stellar.luminosity_watts
	host.inner_stability_m = 0.1 * Units.AU_METERS
	host.outer_stability_m = 50.0 * Units.AU_METERS
	host.calculate_zones()
	system.add_orbit_host(host)
	
	return system


## Tests validation of valid system.
func test_validate_valid_system() -> void:
	var system: SolarSystem = _create_valid_system()
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_true(result.is_valid(), "Valid system should pass validation")


## Tests is_valid quick check.
func test_is_valid() -> void:
	var system: SolarSystem = _create_valid_system()
	
	assert_true(SystemValidator.is_valid(system), "Valid system should pass is_valid")


## Tests null system fails.
func test_is_valid_null() -> void:
	assert_false(SystemValidator.is_valid(null), "Null system should fail")


## Tests empty ID fails.
func test_empty_id_fails() -> void:
	var system: SolarSystem = _create_valid_system()
	system.id = ""
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_false(result.is_valid(), "Empty ID should fail")


## Tests missing hierarchy fails.
func test_missing_hierarchy_fails() -> void:
	var system: SolarSystem = _create_valid_system()
	system.hierarchy = null
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_false(result.is_valid(), "Missing hierarchy should fail")


## Tests empty system (no stars) fails.
func test_no_stars_fails() -> void:
	var system: SolarSystem = SolarSystem.new("test", "Test")
	system.hierarchy = SystemHierarchy.new(HierarchyNode.create_star("n1", "star_1"))
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_false(result.is_valid(), "System with no star bodies should fail")


## Tests hierarchy-body mismatch.
func test_hierarchy_body_mismatch() -> void:
	var system: SolarSystem = _create_valid_system()
	
	# Add a second star to hierarchy but not to bodies
	var star_node: HierarchyNode = HierarchyNode.create_star("node_star", "star_1")
	var ghost_node: HierarchyNode = HierarchyNode.create_star("node_ghost", "ghost_star")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", star_node, ghost_node, 1e11, 0.0)
	system.hierarchy = SystemHierarchy.new(binary)
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_false(result.is_valid(), "Hierarchy referencing non-existent star should fail")


## Tests invalid barycenter eccentricity.
func test_invalid_barycenter_eccentricity() -> void:
	var system: SolarSystem = _create_valid_system()
	
	# Create second star
	var star_spec: StarSpec = StarSpec.random(54321)
	var star_rng: SeededRng = SeededRng.new(54321)
	var star2: CelestialBody = StarGenerator.generate(star_spec, star_rng)
	star2.id = "star_2"
	system.add_body(star2)
	
	# Create binary hierarchy with invalid eccentricity
	var node1: HierarchyNode = HierarchyNode.create_star("n1", "star_1")
	var node2: HierarchyNode = HierarchyNode.create_star("n2", "star_2")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", node1, node2, 1e11, 1.5) # Invalid ecc
	system.hierarchy = SystemHierarchy.new(binary)
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_false(result.is_valid(), "Invalid barycenter eccentricity should fail")


## Tests moon without parent warning.
func test_moon_without_parent() -> void:
	var system: SolarSystem = _create_valid_system()
	
	# Add planet
	var planet: CelestialBody = CelestialBody.new("planet_1", "Planet", CelestialType.Type.PLANET)
	planet.physical = PhysicalProps.new(Units.EARTH_MASS_KG, Units.EARTH_RADIUS_METERS, 86400.0)
	planet.orbital = OrbitalProps.new(Units.AU_METERS, 0.0, 0.0, 0.0, 0.0, 0.0, "node_star")
	system.add_body(planet)
	
	# Add moon without parent_id
	var moon: CelestialBody = CelestialBody.new("moon_1", "Moon", CelestialType.Type.MOON)
	moon.physical = PhysicalProps.new(7.34e22, 1.74e6, 2360592.0)
	moon.orbital = OrbitalProps.new(3.84e8, 0.05, 5.0, 0.0, 0.0, 0.0, "") # Empty parent
	system.add_body(moon)
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	# Should have warning but still be valid
	assert_greater_than(result.get_warning_count(), 0, "Moon without parent should generate warning")


## Tests asteroid belt validation.
func test_invalid_asteroid_belt() -> void:
	var system: SolarSystem = _create_valid_system()
	
	# Add invalid belt (outer < inner)
	var belt: AsteroidBelt = AsteroidBelt.new("belt_1", "Bad Belt")
	belt.inner_radius_m = 5.0 * Units.AU_METERS
	belt.outer_radius_m = 2.0 * Units.AU_METERS # Invalid
	system.add_asteroid_belt(belt)
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_false(result.is_valid(), "Invalid belt should fail validation")


## Tests orbit host validation.
func test_invalid_orbit_host() -> void:
	var system: SolarSystem = _create_valid_system()
	
	# Add invalid host (negative mass)
	var bad_host: OrbitHost = OrbitHost.new("bad_host", OrbitHost.HostType.S_TYPE)
	bad_host.combined_mass_kg = -1.0
	system.add_orbit_host(bad_host)
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_false(result.is_valid(), "Invalid orbit host should fail")


## Tests get_error_count and get_warning_count.
func test_error_counts() -> void:
	var system: SolarSystem = SolarSystem.new("", "") # Empty ID
	system.hierarchy = null # Missing hierarchy
	
	var result: ValidationResult = SystemValidator.validate(system)
	
	assert_greater_than(result.get_error_count(), 0, "Should have errors")
