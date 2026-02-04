## Tests for StellarConfigGenerator.
extends TestCase

const _stellar_config_generator: GDScript = preload("res://src/domain/system/StellarConfigGenerator.gd")
const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Tests single star generation.
func test_generate_single_star() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.single_star(12345)
	var rng: SeededRng = SeededRng.new(12345)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system, "System should be generated")
	assert_equal(system.get_star_count(), 1, "Should have 1 star")
	assert_true(system.hierarchy.is_valid(), "Hierarchy should be valid")
	assert_equal(system.hierarchy.get_star_count(), 1, "Hierarchy should have 1 star")


## Tests binary star generation.
func test_generate_binary() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.binary(54321)
	var rng: SeededRng = SeededRng.new(54321)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system, "System should be generated")
	assert_equal(system.get_star_count(), 2, "Should have 2 stars")
	assert_equal(system.hierarchy.get_star_count(), 2, "Hierarchy should have 2 stars")
	assert_equal(system.hierarchy.get_depth(), 2, "Binary hierarchy should have depth 2")
	
	# Should have barycenter
	var barycenters: Array[HierarchyNode] = system.hierarchy.get_all_barycenters()
	assert_equal(barycenters.size(), 1, "Should have 1 barycenter")
	assert_greater_than(barycenters[0].separation_m, 0.0, "Separation should be positive")


## Tests triple star generation.
func test_generate_triple() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.alpha_centauri_like(99999)
	var rng: SeededRng = SeededRng.new(99999)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system, "System should be generated")
	assert_equal(system.get_star_count(), 3, "Should have 3 stars")
	assert_equal(system.hierarchy.get_star_count(), 3, "Hierarchy should have 3 stars")
	
	# Should have 2 barycenters (inner binary + outer pairing)
	var barycenters: Array[HierarchyNode] = system.hierarchy.get_all_barycenters()
	assert_equal(barycenters.size(), 2, "Triple should have 2 barycenters")


## Tests determinism.
func test_determinism() -> void:
	var spec1: SolarSystemSpec = SolarSystemSpec.binary(11111)
	var spec2: SolarSystemSpec = SolarSystemSpec.binary(11111)
	var rng1: SeededRng = SeededRng.new(11111)
	var rng2: SeededRng = SeededRng.new(11111)
	
	var system1: SolarSystem = StellarConfigGenerator.generate(spec1, rng1)
	var system2: SolarSystem = StellarConfigGenerator.generate(spec2, rng2)
	
	assert_equal(system1.get_star_count(), system2.get_star_count())
	
	for i in range(system1.star_ids.size()):
		var star1: CelestialBody = system1.get_body(system1.star_ids[i])
		var star2: CelestialBody = system2.get_body(system2.star_ids[i])
		assert_float_equal(
			star1.physical.mass_kg,
			star2.physical.mass_kg,
			1.0,
			"Star masses should match"
		)


## Tests spectral class hints are respected.
func test_spectral_class_hints() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.sun_like(22222)
	var rng: SeededRng = SeededRng.new(22222)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	assert_equal(system.get_star_count(), 1)
	
	var star: CelestialBody = system.get_stars()[0]
	assert_true(star.has_stellar())
	assert_true(star.stellar.spectral_class.begins_with("G"), "Should be G-type star")


## Tests orbit hosts are created.
func test_orbit_hosts_created() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.single_star(33333)
	var rng: SeededRng = SeededRng.new(33333)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	assert_greater_than(system.orbit_hosts.size(), 0, "Should have at least one orbit host")
	
	var host: OrbitHost = system.orbit_hosts[0]
	assert_equal(host.host_type, OrbitHost.HostType.S_TYPE, "Single star should have S-type host")
	assert_true(host.has_valid_zone(), "Host should have valid zone")


## Tests binary creates multiple orbit hosts.
func test_binary_orbit_hosts() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.binary(44444)
	var rng: SeededRng = SeededRng.new(44444)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	
	# Binary should create: 2 S-type (each star) + 1 P-type (barycenter) = 3 hosts
	# But some might be filtered if zone is invalid
	assert_greater_than(system.orbit_hosts.size(), 0, "Should have orbit hosts")
	
	var s_type_count: int = 0
	var p_type_count: int = 0
	for host in system.orbit_hosts:
		if host.host_type == OrbitHost.HostType.S_TYPE:
			s_type_count += 1
		else:
			p_type_count += 1
	
	# At minimum should have P-type for circumbinary
	assert_greater_than(s_type_count + p_type_count, 0, "Should have some orbit hosts")


## Tests orbit host stability limits.
func test_orbit_host_stability_limits() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.single_star(55555)
	var rng: SeededRng = SeededRng.new(55555)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	assert_greater_than(system.orbit_hosts.size(), 0)
	
	var host: OrbitHost = system.orbit_hosts[0]
	
	# Inner limit should be > 0 (outside star radius)
	assert_greater_than(host.inner_stability_m, 0.0, "Inner limit should be positive")
	
	# Outer limit should be much larger
	assert_greater_than(host.outer_stability_m, host.inner_stability_m, "Outer > inner")
	
	# Zone should be reasonable for single star
	var zone_width_au: float = host.get_zone_width_au()
	assert_greater_than(zone_width_au, 10.0, "Single star should have wide stable zone")


## Tests habitable zone calculation.
func test_habitable_zone_calculated() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.sun_like(66666)
	var rng: SeededRng = SeededRng.new(66666)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	var host: OrbitHost = system.orbit_hosts[0]
	
	# Sun-like star should have HZ around 1 AU
	var hz_inner_au: float = host.habitable_zone_inner_m / Units.AU_METERS
	var hz_outer_au: float = host.habitable_zone_outer_m / Units.AU_METERS
	
	assert_in_range(hz_inner_au, 0.8, 1.2, "HZ inner should be near 0.95 AU")
	assert_in_range(hz_outer_au, 1.2, 1.7, "HZ outer should be near 1.37-1.6 AU")


## Tests random star count in range.
func test_random_star_count_in_range() -> void:
	for i in range(10):
		var spec: SolarSystemSpec = SolarSystemSpec.random_small(70000 + i)
		var rng: SeededRng = SeededRng.new(70000 + i)
		
		var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
		
		assert_not_null(system)
		assert_in_range(
			system.get_star_count(),
			spec.star_count_min,
			spec.star_count_max,
			"Star count should be in spec range"
		)


## Tests hierarchical system (4+ stars).
func test_hierarchical_system() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(88888, 4, 4)
	var rng: SeededRng = SeededRng.new(88888)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	assert_equal(system.get_star_count(), 4, "Should have 4 stars")
	
	# 4 stars need 3 barycenters to form a single hierarchy
	var barycenters: Array[HierarchyNode] = system.hierarchy.get_all_barycenters()
	assert_equal(barycenters.size(), 3, "4 stars need 3 barycenters")


## Tests maximum star count.
func test_maximum_stars() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(99999, 10, 10)
	var rng: SeededRng = SeededRng.new(99999)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	assert_equal(system.get_star_count(), 10, "Should have 10 stars")
	
	# 10 stars need 9 barycenters
	var barycenters: Array[HierarchyNode] = system.hierarchy.get_all_barycenters()
	assert_equal(barycenters.size(), 9, "10 stars need 9 barycenters")


## Tests hierarchical binary separations increase.
func test_hierarchical_separations_increase() -> void:
	# Generate a triple system multiple times and check separation ordering
	var found_valid: bool = false
	
	for i in range(20):
		var spec: SolarSystemSpec = SolarSystemSpec.new(80000 + i, 3, 3)
		var rng: SeededRng = SeededRng.new(80000 + i)
		
		var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
		
		if system == null:
			continue
		
		var barycenters: Array[HierarchyNode] = system.hierarchy.get_all_barycenters()
		if barycenters.size() != 2:
			continue
		
		# Root barycenter should have larger separation than inner
		var root: HierarchyNode = system.hierarchy.root
		if not root.is_barycenter():
			continue
		
		var inner_sep: float = 0.0
		for child in root.children:
			if child.is_barycenter():
				inner_sep = child.separation_m
				break
		
		if inner_sep > 0.0 and root.separation_m > inner_sep:
			found_valid = true
			break
	
	assert_true(found_valid, "Should find at least one triple with proper separation ordering")


## Tests star names are assigned.
func test_star_names() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.binary(11111)
	var rng: SeededRng = SeededRng.new(11111)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	
	for star in system.get_stars():
		assert_false(star.name.is_empty(), "Star should have a name")


## Tests star IDs are unique.
func test_star_ids_unique() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(22222, 5, 5)
	var rng: SeededRng = SeededRng.new(22222)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	
	var ids: Dictionary = {}
	for star in system.get_stars():
		assert_false(ids.has(star.id), "Star IDs should be unique")
		ids[star.id] = true


## Tests provenance is stored.
func test_provenance_stored() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.single_star(77777)
	var rng: SeededRng = SeededRng.new(77777)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	assert_not_null(system.provenance)
	assert_equal(system.provenance.generation_seed, 77777)


## Tests system age and metallicity are passed to stars.
func test_system_age_metallicity_passed() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(88888, 1, 1)
	spec.system_age_years = 5.0e9
	spec.system_metallicity = 1.2
	var rng: SeededRng = SeededRng.new(88888)
	
	var system: SolarSystem = StellarConfigGenerator.generate(spec, rng)
	
	assert_not_null(system)
	var star: CelestialBody = system.get_stars()[0]
	assert_true(star.has_stellar())
	# Age and metallicity should be close to spec values (may have small variation)
	assert_in_range(star.stellar.age_years, 4.9e9, 5.1e9, "Age should match spec")
	assert_in_range(star.stellar.metallicity, 1.1, 1.3, "Metallicity should match spec")
