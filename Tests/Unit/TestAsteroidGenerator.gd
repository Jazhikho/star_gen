## Unit tests for AsteroidGenerator.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")

const _asteroid_generator: GDScript = preload("res://src/domain/generation/generators/AsteroidGenerator.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _asteroid_type: GDScript = preload("res://src/domain/generation/archetypes/AsteroidType.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Creates a standard solar system context for asteroid generation.
func _create_solar_context() -> ParentContext:
	return ParentContext.sun_like(2.7 * Units.AU_METERS)  # Middle of asteroid belt


func test_generate_returns_celestial_body() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(12345)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(12345)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_not_null(asteroid, "Should return a CelestialBody")
	assert_equal(asteroid.type, CelestialType.Type.ASTEROID, "Should be an asteroid type")


func test_generate_is_deterministic() -> void:
	var spec1: AsteroidSpec = AsteroidSpec.random(54321)
	var spec2: AsteroidSpec = AsteroidSpec.random(54321)
	var context: ParentContext = _create_solar_context()
	var rng1: SeededRng = SeededRng.new(54321)
	var rng2: SeededRng = SeededRng.new(54321)
	
	var asteroid1: CelestialBody = AsteroidGenerator.generate(spec1, context, rng1)
	var asteroid2: CelestialBody = AsteroidGenerator.generate(spec2, context, rng2)
	
	assert_equal(asteroid1.id, asteroid2.id, "IDs should match")
	assert_equal(asteroid1.physical.mass_kg, asteroid2.physical.mass_kg, "Mass should match")
	assert_equal(asteroid1.physical.radius_m, asteroid2.physical.radius_m, "Radius should match")
	assert_equal(asteroid1.orbital.semi_major_axis_m, asteroid2.orbital.semi_major_axis_m, "Orbital distance should match")


func test_carbonaceous_spec() -> void:
	var spec: AsteroidSpec = AsteroidSpec.carbonaceous(11111)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(11111)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_not_null(asteroid, "Should generate carbonaceous asteroid")
	assert_equal(asteroid.surface.surface_type, "carbonaceous", "Should have carbonaceous surface")
	# C-type asteroids have low albedo
	assert_true(asteroid.surface.albedo < 0.15, "C-type should have low albedo")


func test_silicaceous_spec() -> void:
	var spec: AsteroidSpec = AsteroidSpec.stony(22222)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(22222)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_not_null(asteroid, "Should generate stony asteroid")
	assert_equal(asteroid.surface.surface_type, "silicaceous", "Should have silicaceous surface")


func test_metallic_spec() -> void:
	var spec: AsteroidSpec = AsteroidSpec.metallic(33333)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(33333)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_not_null(asteroid, "Should generate metallic asteroid")
	assert_equal(asteroid.surface.surface_type, "metallic", "Should have metallic surface")
	# M-type asteroids have high density
	var density: float = asteroid.physical.get_density_kg_m3()
	assert_true(density > 4000.0, "M-type should have high density (got %f)" % density)


func test_ceres_like_spec() -> void:
	var spec: AsteroidSpec = AsteroidSpec.ceres_like(44444)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(44444)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_not_null(asteroid, "Should generate Ceres-like asteroid")
	# Large asteroids should be massive
	assert_true(asteroid.physical.mass_kg > 1.0e19, "Large asteroid should have significant mass")
	# Large asteroids should be more spherical (low oblateness)
	assert_true(asteroid.physical.oblateness < 0.15, "Large asteroid should be more spherical")


func test_no_atmosphere() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(55555)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(55555)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_false(asteroid.has_atmosphere(), "Asteroids should not have atmosphere")


func test_has_surface() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(66666)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(66666)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_true(asteroid.has_surface(), "Asteroid should have surface")
	assert_not_null(asteroid.surface.terrain, "Asteroid should have terrain")


func test_high_crater_density() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(77777)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(77777)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_true(asteroid.surface.terrain.crater_density > 0.5, "Asteroids should be heavily cratered")


func test_no_volcanism() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(88888)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(88888)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_equal(asteroid.surface.volcanism_level, 0.0, "Asteroids should have no volcanism")


func test_no_erosion() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(99999)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(99999)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_equal(asteroid.surface.terrain.erosion_level, 0.0, "Asteroids should have no erosion")


func test_orbital_in_main_belt_by_default() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(10101)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(10101)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	var distance_au: float = asteroid.orbital.semi_major_axis_m / Units.AU_METERS
	assert_true(distance_au >= 2.0, "Should be in or near main belt (got %f AU)" % distance_au)
	assert_true(distance_au <= 3.5, "Should be in or near main belt (got %f AU)" % distance_au)


func test_orbital_override() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(20202)
	spec.set_override("orbital.semi_major_axis_m", 5.0 * Units.AU_METERS)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(20202)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	var distance_au: float = asteroid.orbital.semi_major_axis_m / Units.AU_METERS
	assert_true(absf(distance_au - 5.0) < 0.01, "Should respect orbital override")


func test_physical_properties_valid_ranges() -> void:
	for i in range(10):
		var spec: AsteroidSpec = AsteroidSpec.random(30303 + i)
		var context: ParentContext = _create_solar_context()
		var rng: SeededRng = SeededRng.new(30303 + i)
		
		var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
		
		assert_true(asteroid.physical.mass_kg > 0.0, "Mass should be positive")
		assert_true(asteroid.physical.radius_m > 0.0, "Radius should be positive")
		assert_true(asteroid.physical.get_density_kg_m3() > 0.0, "Density should be positive")
		assert_true(asteroid.physical.axial_tilt_deg >= 0.0, "Axial tilt should be non-negative")
		assert_true(asteroid.physical.axial_tilt_deg <= 180.0, "Axial tilt should be <= 180")


func test_orbital_properties_valid_ranges() -> void:
	for i in range(10):
		var spec: AsteroidSpec = AsteroidSpec.random(40404 + i)
		var context: ParentContext = _create_solar_context()
		var rng: SeededRng = SeededRng.new(40404 + i)
		
		var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
		
		assert_true(asteroid.orbital.semi_major_axis_m > 0.0, "Semi-major axis should be positive")
		assert_true(asteroid.orbital.eccentricity >= 0.0, "Eccentricity should be non-negative")
		assert_true(asteroid.orbital.eccentricity < 1.0, "Eccentricity should be < 1")
		assert_true(asteroid.orbital.inclination_deg >= 0.0, "Inclination should be non-negative")


func test_type_distribution() -> void:
	# Generate many asteroids and check type distribution roughly matches weights
	var c_count: int = 0
	var s_count: int = 0
	var m_count: int = 0
	var total: int = 100
	
	for i in range(total):
		var spec: AsteroidSpec = AsteroidSpec.random(50505 + i)
		var context: ParentContext = _create_solar_context()
		var rng: SeededRng = SeededRng.new(50505 + i)
		
		var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
		
		match asteroid.surface.surface_type:
			"carbonaceous":
				c_count += 1
			"silicaceous":
				s_count += 1
			"metallic":
				m_count += 1
	
	# C-type should be most common (75% weight)
	assert_true(c_count > s_count, "C-type should be more common than S-type")
	assert_true(c_count > m_count, "C-type should be more common than M-type")
	# M-type should be least common (8% weight)
	assert_true(m_count < s_count, "M-type should be less common than S-type")


func test_composition_matches_type() -> void:
	# C-type should have carbon compounds
	var c_spec: AsteroidSpec = AsteroidSpec.carbonaceous(60606)
	var context: ParentContext = _create_solar_context()
	var c_rng: SeededRng = SeededRng.new(60606)
	var c_asteroid: CelestialBody = AsteroidGenerator.generate(c_spec, context, c_rng)
	assert_true(c_asteroid.surface.surface_composition.has("carbon_compounds"), "C-type should have carbon compounds")
	
	# M-type should have iron
	var m_spec: AsteroidSpec = AsteroidSpec.metallic(70707)
	var m_rng: SeededRng = SeededRng.new(70707)
	var m_asteroid: CelestialBody = AsteroidGenerator.generate(m_spec, context, m_rng)
	assert_true(m_asteroid.surface.surface_composition.has("iron"), "M-type should have iron")


func test_provenance_stored() -> void:
	var spec: AsteroidSpec = AsteroidSpec.random(80808)
	var context: ParentContext = _create_solar_context()
	var rng: SeededRng = SeededRng.new(80808)
	
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, rng)
	
	assert_not_null(asteroid.provenance, "Should have provenance")
	assert_equal(asteroid.provenance.generation_seed, 80808, "Provenance should store seed")


func test_different_seeds_produce_different_asteroids() -> void:
	var context: ParentContext = _create_solar_context()
	
	var spec1: AsteroidSpec = AsteroidSpec.random(11111)
	var spec2: AsteroidSpec = AsteroidSpec.random(22222)
	var rng1: SeededRng = SeededRng.new(11111)
	var rng2: SeededRng = SeededRng.new(22222)
	
	var asteroid1: CelestialBody = AsteroidGenerator.generate(spec1, context, rng1)
	var asteroid2: CelestialBody = AsteroidGenerator.generate(spec2, context, rng2)
	
	assert_not_equal(asteroid1.id, asteroid2.id, "Different seeds should produce different IDs")


func test_typical_vs_large_mass_difference() -> void:
	var context: ParentContext = _create_solar_context()
	
	var typical_spec: AsteroidSpec = AsteroidSpec.random(90909)
	var large_spec: AsteroidSpec = AsteroidSpec.ceres_like(90909)
	
	var typical_rng: SeededRng = SeededRng.new(90909)
	var large_rng: SeededRng = SeededRng.new(90909)
	
	var typical: CelestialBody = AsteroidGenerator.generate(typical_spec, context, typical_rng)
	var large: CelestialBody = AsteroidGenerator.generate(large_spec, context, large_rng)
	
	assert_true(large.physical.mass_kg > typical.physical.mass_kg, "Large asteroid should be more massive")


func test_small_asteroids_more_irregular() -> void:
	# Test multiple to account for randomness
	var typical_oblateness_sum: float = 0.0
	var large_oblateness_sum: float = 0.0
	var count: int = 20
	
	var context: ParentContext = _create_solar_context()
	
	for i in range(count):
		var typical_spec: AsteroidSpec = AsteroidSpec.random(10000 + i)
		var large_spec: AsteroidSpec = AsteroidSpec.ceres_like(20000 + i)
		
		var typical_rng: SeededRng = SeededRng.new(10000 + i)
		var large_rng: SeededRng = SeededRng.new(20000 + i)
		
		var typical: CelestialBody = AsteroidGenerator.generate(typical_spec, context, typical_rng)
		var large: CelestialBody = AsteroidGenerator.generate(large_spec, context, large_rng)
		
		typical_oblateness_sum += typical.physical.oblateness
		large_oblateness_sum += large.physical.oblateness
	
	var typical_avg: float = typical_oblateness_sum / count
	var large_avg: float = large_oblateness_sum / count
	
	# On average, typical asteroids should be more irregular (higher oblateness)
	assert_true(typical_avg > large_avg, "Typical asteroids should be more irregular on average")
