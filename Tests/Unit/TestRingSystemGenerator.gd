## Unit tests for RingSystemGenerator.
extends TestCase

const _ring_system_generator: GDScript = preload("res://src/domain/generation/generators/RingSystemGenerator.gd")
const _ring_system_spec: GDScript = preload("res://src/domain/generation/specs/RingSystemSpec.gd")
const _ring_complexity: GDScript = preload("res://src/domain/generation/archetypes/RingComplexity.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Creates Saturn-like physical properties for testing.
func _create_saturn_physical() -> PhysicalProps:
	return PhysicalProps.new(
		5.683e26,     # mass_kg (Saturn)
		5.8232e7,     # radius_m (Saturn)
		38362.4,      # rotation_period_s (~10.7 hours)
		26.73,        # axial_tilt_deg
		0.0687,       # oblateness
		4.6e18,       # magnetic_moment
		8.0e16        # internal_heat_watts
	)


## Creates a Saturn-like context (outer solar system, beyond ice line).
func _create_saturn_context() -> ParentContext:
	return ParentContext.for_planet(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		9.5 * Units.AU_METERS  # Saturn's distance
	)


## Creates an inner solar system context (inside ice line).
func _create_inner_context() -> ParentContext:
	return ParentContext.for_planet(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		1.5 * Units.AU_METERS  # Mars distance
	)


func test_generate_returns_ring_system() -> void:
	var spec: RingSystemSpec = RingSystemSpec.random(12345)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(12345)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should return a RingSystemProps")
	assert_true(rings.get_band_count() > 0, "Should have at least one band")


func test_generate_is_deterministic() -> void:
	var spec1: RingSystemSpec = RingSystemSpec.random(54321)
	var spec2: RingSystemSpec = RingSystemSpec.random(54321)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng1: SeededRng = SeededRng.new(54321)
	var rng2: SeededRng = SeededRng.new(54321)
	
	var rings1: RingSystemProps = RingSystemGenerator.generate(spec1, physical, context, rng1)
	var rings2: RingSystemProps = RingSystemGenerator.generate(spec2, physical, context, rng2)
	
	assert_equal(rings1.get_band_count(), rings2.get_band_count(), "Band count should match")
	assert_equal(rings1.total_mass_kg, rings2.total_mass_kg, "Mass should match")
	assert_equal(rings1.get_inner_radius_m(), rings2.get_inner_radius_m(), "Inner radius should match")


func test_trace_complexity() -> void:
	var spec: RingSystemSpec = RingSystemSpec.trace(11111)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(11111)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate trace rings")
	assert_equal(rings.get_band_count(), 1, "Trace rings should have 1 band")
	# Trace rings have low optical depth
	assert_true(rings.bands[0].optical_depth < 0.2, "Trace rings should have low optical depth")


func test_simple_complexity() -> void:
	var spec: RingSystemSpec = RingSystemSpec.simple(22222)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(22222)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate simple rings")
	assert_true(rings.get_band_count() >= 2, "Simple rings should have at least 2 bands")
	assert_true(rings.get_band_count() <= 3, "Simple rings should have at most 3 bands")


func test_complex_complexity() -> void:
	var spec: RingSystemSpec = RingSystemSpec.complex(33333)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(33333)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate complex rings")
	assert_true(rings.get_band_count() >= 4, "Complex rings should have at least 4 bands")
	assert_true(rings.get_band_count() <= 7, "Complex rings should have at most 7 bands")


func test_icy_composition_beyond_ice_line() -> void:
	var spec: RingSystemSpec = RingSystemSpec.random(44444)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()  # Beyond ice line
	var rng: SeededRng = SeededRng.new(44444)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	# Check composition of first band
	var composition: Dictionary = rings.bands[0].composition
	assert_true(composition.has("water_ice"), "Should have water ice beyond ice line")
	assert_true(composition["water_ice"] > 0.5, "Should be predominantly icy")


func test_rocky_composition_inside_ice_line() -> void:
	var spec: RingSystemSpec = RingSystemSpec.random(55555)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_inner_context()  # Inside ice line
	var rng: SeededRng = SeededRng.new(55555)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	var composition: Dictionary = rings.bands[0].composition
	assert_true(composition.has("silicates"), "Should have silicates inside ice line")
	assert_true(composition["silicates"] > 0.5, "Should be predominantly rocky")


func test_forced_icy_composition() -> void:
	var spec: RingSystemSpec = RingSystemSpec.icy(66666)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_inner_context()  # Inside ice line, but forced icy
	var rng: SeededRng = SeededRng.new(66666)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	var composition: Dictionary = rings.bands[0].composition
	assert_true(composition.has("water_ice"), "Should have water ice when forced")


func test_forced_rocky_composition() -> void:
	var spec: RingSystemSpec = RingSystemSpec.rocky(77777)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()  # Beyond ice line, but forced rocky
	var rng: SeededRng = SeededRng.new(77777)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	var composition: Dictionary = rings.bands[0].composition
	assert_true(composition.has("silicates"), "Should have silicates when forced rocky")


func test_rings_outside_planet_radius() -> void:
	var spec: RingSystemSpec = RingSystemSpec.random(88888)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(88888)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	var inner_radius: float = rings.get_inner_radius_m()
	assert_true(inner_radius > physical.radius_m, "Rings should be outside planet radius")


func test_bands_ordered_by_radius() -> void:
	var spec: RingSystemSpec = RingSystemSpec.complex(99999)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(99999)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	
	# Verify bands don't overlap and are ordered
	for i in range(rings.get_band_count()):
		var band: RingBand = rings.get_band(i)
		assert_true(band.outer_radius_m > band.inner_radius_m, "Band outer > inner")
		
		if i > 0:
			var prev_band: RingBand = rings.get_band(i - 1)
			assert_true(band.inner_radius_m >= prev_band.outer_radius_m, "Bands should not overlap")


func test_band_properties_valid() -> void:
	var spec: RingSystemSpec = RingSystemSpec.complex(10101)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(10101)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	
	for i in range(rings.get_band_count()):
		var band: RingBand = rings.get_band(i)
		assert_true(band.inner_radius_m > 0.0, "Inner radius should be positive")
		assert_true(band.outer_radius_m > 0.0, "Outer radius should be positive")
		assert_true(band.optical_depth > 0.0, "Optical depth should be positive")
		assert_true(band.particle_size_m > 0.0, "Particle size should be positive")
		assert_false(band.composition.is_empty(), "Composition should not be empty")


func test_total_mass_positive() -> void:
	var spec: RingSystemSpec = RingSystemSpec.random(20202)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(20202)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	assert_true(rings.total_mass_kg > 0.0, "Total mass should be positive")


func test_inclination_small() -> void:
	var spec: RingSystemSpec = RingSystemSpec.random(30303)
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	var rng: SeededRng = SeededRng.new(30303)
	
	var rings: RingSystemProps = RingSystemGenerator.generate(spec, physical, context, rng)
	
	assert_not_null(rings, "Should generate rings")
	assert_true(rings.inclination_deg >= 0.0, "Inclination should be non-negative")
	assert_true(rings.inclination_deg < 10.0, "Inclination should be small (aligned with equator)")


func test_should_have_rings_gas_giant() -> void:
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	
	# Test multiple times for probability
	var has_rings_count: int = 0
	for i in range(20):
		var rng: SeededRng = SeededRng.new(40404 + i)
		if RingSystemGenerator.should_have_rings(physical, context, rng):
			has_rings_count += 1
	
	# Gas giants should have rings most of the time (70% probability)
	assert_true(has_rings_count > 5, "Gas giants should often have rings")


func test_should_have_rings_terrestrial_rare() -> void:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0,
		23.5,
		0.003,
		8.0e22,
		4.7e13
	)
	var context: ParentContext = ParentContext.sun_like()
	
	# Test multiple times for probability
	var has_rings_count: int = 0
	for i in range(100):
		var rng: SeededRng = SeededRng.new(50505 + i)
		if RingSystemGenerator.should_have_rings(physical, context, rng):
			has_rings_count += 1
	
	# Terrestrial planets should rarely have rings (1% probability)
	assert_true(has_rings_count < 10, "Terrestrial planets should rarely have rings")


func test_different_seeds_produce_different_rings() -> void:
	var physical: PhysicalProps = _create_saturn_physical()
	var context: ParentContext = _create_saturn_context()
	
	var spec1: RingSystemSpec = RingSystemSpec.random(11111)
	var spec2: RingSystemSpec = RingSystemSpec.random(22222)
	var rng1: SeededRng = SeededRng.new(11111)
	var rng2: SeededRng = SeededRng.new(22222)
	
	var rings1: RingSystemProps = RingSystemGenerator.generate(spec1, physical, context, rng1)
	var rings2: RingSystemProps = RingSystemGenerator.generate(spec2, physical, context, rng2)
	
	assert_not_null(rings1, "Should generate rings 1")
	assert_not_null(rings2, "Should generate rings 2")
	# At least something should differ
	var differ: bool = (
		rings1.get_band_count() != rings2.get_band_count() or
		rings1.total_mass_kg != rings2.total_mass_kg or
		rings1.get_inner_radius_m() != rings2.get_inner_radius_m()
	)
	assert_true(differ, "Different seeds should produce different rings")
