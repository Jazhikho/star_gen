## Unit tests for GalaxySystemGenerator class.
class_name TestGalaxySystemGenerator
extends TestCase

const _overrides_script: GDScript = preload("res://src/domain/galaxy/GalaxyBodyOverrides.gd")


func get_test_name() -> String:
	return "TestGalaxySystemGenerator"


func test_generate_system_from_star() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(8000.0, 0.0, 0.0), 12345, spec
	)

	var system: SolarSystem = GalaxySystemGenerator.generate_system(star)
	assert_not_null(system, "Should generate system")
	assert_greater_than(system.get_star_count(), 0, "System should have at least one star")


func test_generate_system_deterministic() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	# Use same seed and position for both stars
	var star_a: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(5000.0, 0.0, 0.0), 99999, spec
	)
	var star_b: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(5000.0, 0.0, 0.0), 99999, spec
	)

	var system_a: SolarSystem = GalaxySystemGenerator.generate_system(star_a)
	var system_b: SolarSystem = GalaxySystemGenerator.generate_system(star_b)

	assert_equal(system_a.get_star_count(), system_b.get_star_count(), "Same seed should give same star count")
	assert_equal(system_a.get_planet_count(), system_b.get_planet_count(), "Same seed should give same planet count")
	assert_equal(system_a.get_moon_count(), system_b.get_moon_count(), "Same seed should give same moon count")

	# Verify stars have same properties
	if system_a.get_star_count() > 0 and system_b.get_star_count() > 0:
		var star_a_body: CelestialBody = system_a.get_stars()[0]
		var star_b_body: CelestialBody = system_b.get_stars()[0]
		assert_equal(star_a_body.physical.mass_kg, star_b_body.physical.mass_kg, "Same seed should give same star mass")


func test_generate_system_different_seeds_different_results() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var star_a: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(5000.0, 0.0, 0.0), 111, spec
	)
	var star_b: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(5000.0, 0.0, 0.0), 222, spec
	)

	var system_a: SolarSystem = GalaxySystemGenerator.generate_system(star_a)
	var system_b: SolarSystem = GalaxySystemGenerator.generate_system(star_b)

	# Systems should differ in some way (star count, planet count, or IDs)
	var any_different: bool = (
		system_a.get_star_count() != system_b.get_star_count() or
		system_a.get_planet_count() != system_b.get_planet_count() or
		system_a.id != system_b.id
	)
	assert_true(any_different, "Different seeds should produce different systems")


func test_generate_system_null_star_returns_null() -> void:
	var system: SolarSystem = GalaxySystemGenerator.generate_system(null)
	assert_null(system, "Null star should return null system")


func test_generate_system_without_asteroids() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(8000.0, 0.0, 0.0), 54321, spec
	)

	var system: SolarSystem = GalaxySystemGenerator.generate_system(star, false)
	assert_not_null(system, "Should generate system without asteroids")
	assert_equal(system.asteroid_belts.size(), 0, "Should have no asteroid belts")


func test_metallicity_applied_to_spec() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	# High metallicity star (near center)
	var star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(500.0, 0.0, 0.0), 11111, spec
	)

	var system: SolarSystem = GalaxySystemGenerator.generate_system(star)
	assert_not_null(system, "Should generate system with metallicity context")
	# Verify the system has valid provenance
	assert_not_null(system.provenance, "System should have provenance")
	assert_not_null(system.provenance.spec_snapshot, "Provenance should have spec snapshot")
	assert_true(system.provenance.spec_snapshot.has("system_metallicity"), "Spec snapshot should contain metallicity")


func test_generate_system_has_valid_hierarchy() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(8000.0, 0.0, 0.0), 77777, spec
	)

	var system: SolarSystem = GalaxySystemGenerator.generate_system(star)
	assert_not_null(system, "Should generate system")
	assert_not_null(system.hierarchy, "System should have hierarchy")
	assert_true(system.hierarchy.is_valid(), "Hierarchy should be valid")


func test_generate_system_planets_have_parent_ids() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(8000.0, 0.0, 0.0), 88888, spec
	)

	var system: SolarSystem = GalaxySystemGenerator.generate_system(star)
	assert_not_null(system, "Should generate system")

	var planets: Array[CelestialBody] = system.get_planets()
	for planet in planets:
		assert_true(planet.has_orbital(), "Planet should have orbital properties")
		assert_false(planet.orbital.parent_id.is_empty(), "Planet should have parent_id set")
		# Parent should be a valid orbit host
		var host: OrbitHost = system.get_orbit_host(planet.orbital.parent_id)
		assert_not_null(host, "Planet parent_id should reference valid orbit host")


func test_generate_system_provenance_has_spec_snapshot() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var star: GalaxyStar = GalaxyStar.create_with_derived_properties(
		Vector3(8000.0, 0.0, 0.0), 55555, spec
	)

	var system: SolarSystem = GalaxySystemGenerator.generate_system(star)
	assert_not_null(system, "Should generate system")
	assert_not_null(system.provenance, "System should have provenance")
	assert_false(system.provenance.spec_snapshot.is_empty(), "Provenance should have spec snapshot")
	assert_true(system.provenance.spec_snapshot.has("generation_seed"), "Spec snapshot should have generation_seed")
	assert_equal(system.provenance.spec_snapshot["generation_seed"], star.star_seed, "Spec snapshot seed should match star seed")


func _make_test_star(star_seed: int) -> GalaxyStar:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	return GalaxyStar.create_with_derived_properties(Vector3(8000.0, 0.0, 0.0), star_seed, spec)


func test_generate_system_without_overrides_unchanged() -> void:
	var star: GalaxyStar = _make_test_star(777)
	var sys_a: SolarSystem = GalaxySystemGenerator.generate_system(star, false, false, null)
	var sys_b: SolarSystem = GalaxySystemGenerator.generate_system(star, false, false)
	assert_not_null(sys_a)
	assert_not_null(sys_b)
	var planets_a: Array = sys_a.get_planets()
	var planets_b: Array = sys_b.get_planets()
	assert_equal(planets_a.size(), planets_b.size(), "null overrides must not change body count")


func test_generate_system_applies_planet_override() -> void:
	var star: GalaxyStar = _make_test_star(12345)
	var baseline: SolarSystem = GalaxySystemGenerator.generate_system(star, false, false)
	assert_not_null(baseline)
	var planets: Array = baseline.get_planets()
	if planets.is_empty():
		return
	var target_planet: CelestialBody = planets[0] as CelestialBody
	var target_id: String = target_planet.id

	var edited_mass: float = 7.777e24
	var ov: RefCounted = _overrides_script.new()
	var edited: CelestialBody = CelestialSerializer.from_dict(CelestialSerializer.to_dict(target_planet))
	edited.physical.mass_kg = edited_mass
	edited.name = "Edited-By-Test"
	ov.set_override(star.star_seed, edited)

	var patched: SolarSystem = GalaxySystemGenerator.generate_system(star, false, false, ov)
	assert_not_null(patched)
	var patched_planet: CelestialBody = patched.get_body(target_id)
	assert_not_null(patched_planet, "override body id must still resolve")
	assert_float_equal(patched_planet.physical.mass_kg, edited_mass, 1.0, "override mass must replace deterministic value")
	assert_equal(patched_planet.name, "Edited-By-Test", "override name must survive")


func test_generate_system_ignores_overrides_for_other_seeds() -> void:
	var star: GalaxyStar = _make_test_star(500)
	var baseline: SolarSystem = GalaxySystemGenerator.generate_system(star, false, false)
	assert_not_null(baseline)

	var ov: RefCounted = _overrides_script.new()
	var dummy_phys: PhysicalProps = PhysicalProps.new(1.0, 1.0)
	var dummy: CelestialBody = CelestialBody.new("wont_match", "Nope", CelestialType.Type.PLANET, dummy_phys, null)
	ov.set_override(999999, dummy)

	var patched: SolarSystem = GalaxySystemGenerator.generate_system(star, false, false, ov)
	assert_equal(baseline.get_planets().size(), patched.get_planets().size(), "wrong-seed override must not change structure")
	if not baseline.get_planets().is_empty():
		var a: CelestialBody = baseline.get_planets()[0] as CelestialBody
		var b: CelestialBody = patched.get_planets()[0] as CelestialBody
		assert_float_equal(a.physical.mass_kg, b.physical.mass_kg, 0.0, "wrong-seed override must not affect generation")
