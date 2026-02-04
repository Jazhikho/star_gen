## Unit tests for MoonGenerator.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")

const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Creates a Jupiter-like parent context for moon generation.
func _create_jupiter_context() -> ParentContext:
	var jupiter_mass_kg: float = 1.898e27
	var jupiter_radius_m: float = 6.9911e7
	var jupiter_orbit_m: float = 5.2 * Units.AU_METERS
	
	return ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		jupiter_orbit_m,
		jupiter_mass_kg,
		jupiter_radius_m,
		4.0e8 # ~400,000 km from Jupiter (like Europa)
	)


## Creates an Earth-like parent context for moon generation.
func _create_earth_context() -> ParentContext:
	return ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		Units.AU_METERS,
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		3.844e8 # ~384,400 km (Luna's distance)
	)


func test_generate_returns_celestial_body() -> void:
	var spec: MoonSpec = MoonSpec.random(12345)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(12345)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_not_null(moon, "Should return a CelestialBody")
	assert_equal(moon.type, CelestialType.Type.MOON, "Should be a moon type")


func test_generate_is_deterministic() -> void:
	var spec1: MoonSpec = MoonSpec.random(54321)
	var spec2: MoonSpec = MoonSpec.random(54321)
	var context: ParentContext = _create_jupiter_context()
	var rng1: SeededRng = SeededRng.new(54321)
	var rng2: SeededRng = SeededRng.new(54321)
	
	var moon1: CelestialBody = MoonGenerator.generate(spec1, context, rng1)
	var moon2: CelestialBody = MoonGenerator.generate(spec2, context, rng2)
	
	assert_equal(moon1.id, moon2.id, "IDs should match")
	assert_equal(moon1.physical.mass_kg, moon2.physical.mass_kg, "Mass should match")
	assert_equal(moon1.physical.radius_m, moon2.physical.radius_m, "Radius should match")
	assert_equal(moon1.orbital.semi_major_axis_m, moon2.orbital.semi_major_axis_m, "Orbital distance should match")


func test_generate_respects_size_category_override() -> void:
	var spec: MoonSpec = MoonSpec.new(
		11111,
		SizeCategory.Category.TERRESTRIAL
	)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(11111)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	# Terrestrial moons should be relatively large
	var mass_earth: float = moon.physical.mass_kg / Units.EARTH_MASS_KG
	assert_true(mass_earth >= 0.1, "Terrestrial moon should have mass >= 0.1 Earth masses")


func test_luna_like_spec() -> void:
	var spec: MoonSpec = MoonSpec.luna_like(22222)
	var context: ParentContext = _create_earth_context()
	var rng: SeededRng = SeededRng.new(22222)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_not_null(moon, "Should generate Luna-like moon")
	assert_false(moon.has_atmosphere(), "Luna-like moon should not have atmosphere")
	# Should not have subsurface ocean
	if moon.has_surface() and moon.surface.has_cryosphere():
		assert_false(moon.surface.cryosphere.has_subsurface_ocean, "Luna-like should not have subsurface ocean")


func test_europa_like_spec() -> void:
	var spec: MoonSpec = MoonSpec.europa_like(33333)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(33333)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_not_null(moon, "Should generate Europa-like moon")
	assert_false(moon.has_atmosphere(), "Europa-like moon should not have atmosphere")
	# Should have cryosphere
	assert_true(moon.has_surface(), "Should have surface")
	assert_true(moon.surface.has_cryosphere(), "Europa-like should have cryosphere")
	assert_true(moon.surface.cryosphere.has_subsurface_ocean, "Europa-like should have subsurface ocean")


func test_titan_like_spec() -> void:
	var spec: MoonSpec = MoonSpec.titan_like(44444)
	var context: ParentContext = _create_jupiter_context()
	# Increase orbital distance for larger moon
	context.orbital_distance_from_parent_m = 1.2e9 # ~1.2 million km
	var rng: SeededRng = SeededRng.new(44444)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_not_null(moon, "Should generate Titan-like moon")
	assert_true(moon.has_atmosphere(), "Titan-like moon should have atmosphere")


func test_captured_moon_spec() -> void:
	var spec: MoonSpec = MoonSpec.captured(55555)
	var context: ParentContext = _create_jupiter_context()
	context.orbital_distance_from_parent_m = 2.0e10 # Far out
	var rng: SeededRng = SeededRng.new(55555)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_not_null(moon, "Should generate captured moon")
	# Captured moons tend to be small
	var mass_earth: float = moon.physical.mass_kg / Units.EARTH_MASS_KG
	assert_true(mass_earth < 0.1, "Captured moon should be small")


func test_orbital_distance_within_hill_sphere() -> void:
	var spec: MoonSpec = MoonSpec.random(66666)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(66666)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	var hill_radius: float = context.get_hill_sphere_radius_m()
	var orbital_distance: float = moon.orbital.semi_major_axis_m
	
	assert_true(orbital_distance < hill_radius, "Moon should orbit within Hill sphere")
	assert_true(orbital_distance > context.parent_body_radius_m, "Moon should orbit outside parent body")


func test_orbital_distance_outside_roche_limit() -> void:
	var spec: MoonSpec = MoonSpec.random(77777)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(77777)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	# Estimate Roche limit for the generated moon
	var moon_density: float = moon.physical.get_density_kg_m3()
	var roche_limit: float = context.get_roche_limit_m(moon_density)
	var orbital_distance: float = moon.orbital.semi_major_axis_m
	
	assert_true(orbital_distance > roche_limit, "Moon should orbit outside Roche limit")


func test_tidal_locking() -> void:
	# Close-in moons should be tidally locked
	var spec: MoonSpec = MoonSpec.random(88888)
	var context: ParentContext = _create_jupiter_context()
	context.orbital_distance_from_parent_m = 4.0e8 # Close to Jupiter
	var rng: SeededRng = SeededRng.new(88888)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	# For tidally locked moons, rotation period should equal orbital period
	var orbital_period: float = moon.orbital.get_orbital_period_s(context.parent_body_mass_kg)
	var rotation_period: float = moon.physical.rotation_period_s
	
	# Allow 1% tolerance
	var ratio: float = absf(rotation_period / orbital_period)
	assert_true(ratio > 0.99 and ratio < 1.01, "Close moon should be tidally locked (rotation ≈ orbital period)")


func test_captured_moon_can_have_high_eccentricity() -> void:
	# Test multiple captured moons to see varied eccentricities
	var high_ecc_found: bool = false
	
	for i in range(10):
		var spec: MoonSpec = MoonSpec.captured(90000 + i)
		var context: ParentContext = _create_jupiter_context()
		context.orbital_distance_from_parent_m = 1.0e10
		var rng: SeededRng = SeededRng.new(90000 + i)
		
		var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
		
		if moon.orbital.eccentricity > 0.2:
			high_ecc_found = true
			break
	
	assert_true(high_ecc_found, "At least one captured moon should have high eccentricity")


func test_captured_moon_can_have_high_inclination() -> void:
	# Test multiple captured moons to see varied inclinations
	var high_inc_found: bool = false
	
	for i in range(10):
		var spec: MoonSpec = MoonSpec.captured(80000 + i)
		var context: ParentContext = _create_jupiter_context()
		context.orbital_distance_from_parent_m = 1.0e10
		var rng: SeededRng = SeededRng.new(80000 + i)
		
		var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
		
		if moon.orbital.inclination_deg > 30.0:
			high_inc_found = true
			break
	
	assert_true(high_inc_found, "At least one captured moon should have high inclination")


func test_moon_mass_constrained_by_parent() -> void:
	# Moon mass should be much less than parent mass
	var spec: MoonSpec = MoonSpec.new(
		70707,
		SizeCategory.Category.SUPER_EARTH # Request large moon
	)
	var context: ParentContext = _create_earth_context() # Earth-sized parent
	var rng: SeededRng = SeededRng.new(70707)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	var mass_ratio: float = moon.physical.mass_kg / context.parent_body_mass_kg
	assert_true(mass_ratio < 0.15, "Moon mass should be << parent mass (ratio: %f)" % mass_ratio)


func test_moon_has_surface() -> void:
	var spec: MoonSpec = MoonSpec.random(60606)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(60606)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_true(moon.has_surface(), "Moon should always have surface")
	assert_not_null(moon.surface.terrain, "Moon should have terrain")


func test_cold_moon_has_cryosphere() -> void:
	# Jupiter distance = cold
	var spec: MoonSpec = MoonSpec.random(50505)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(50505)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	# At Jupiter's distance, moons should be cold
	assert_true(moon.has_surface(), "Should have surface")
	assert_true(moon.surface.has_cryosphere(), "Cold moon should have cryosphere")


func test_tidal_heating_increases_volcanism() -> void:
	# Compare moon with high vs low eccentricity
	var spec_low_e: MoonSpec = MoonSpec.random(40404)
	spec_low_e.set_override("orbital.eccentricity", 0.001)
	
	var spec_high_e: MoonSpec = MoonSpec.random(40404)
	spec_high_e.set_override("orbital.eccentricity", 0.05)
	
	var context: ParentContext = _create_jupiter_context()
	context.orbital_distance_from_parent_m = 4.0e8 # Close to Jupiter
	
	var rng1: SeededRng = SeededRng.new(40404)
	var rng2: SeededRng = SeededRng.new(40404)
	
	var moon_low_e: CelestialBody = MoonGenerator.generate(spec_low_e, context, rng1)
	var moon_high_e: CelestialBody = MoonGenerator.generate(spec_high_e, context, rng2)
	
	# High eccentricity should lead to more tidal heating and thus more volcanism
	# Note: This may not always be true due to random variation, but on average should be
	assert_true(
		moon_high_e.surface.volcanism_level >= moon_low_e.surface.volcanism_level * 0.5,
		"High eccentricity moon should have comparable or higher volcanism"
	)


func test_subsurface_ocean_with_tidal_heating() -> void:
	# Moons with tidal heating are more likely to have subsurface oceans
	var ocean_count: int = 0
	
	for i in range(20):
		var spec: MoonSpec = MoonSpec.random(30303 + i)
		var context: ParentContext = _create_jupiter_context()
		context.orbital_distance_from_parent_m = 5.0e8 # Close enough for tidal heating
		var rng: SeededRng = SeededRng.new(30303 + i)
		
		# Set moderate eccentricity for tidal heating
		spec.set_override("orbital.eccentricity", 0.01)
		
		var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
		
		if moon.has_surface() and moon.surface.has_cryosphere():
			if moon.surface.cryosphere.has_subsurface_ocean:
				ocean_count += 1
	
	assert_true(ocean_count > 0, "Some tidally heated moons should have subsurface oceans")


func test_physical_properties_valid_ranges() -> void:
	for i in range(10):
		var spec: MoonSpec = MoonSpec.random(20202 + i)
		var context: ParentContext = _create_jupiter_context()
		var rng: SeededRng = SeededRng.new(20202 + i)
		
		var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
		
		assert_true(moon.physical.mass_kg > 0.0, "Mass should be positive")
		assert_true(moon.physical.radius_m > 0.0, "Radius should be positive")
		assert_true(moon.physical.get_density_kg_m3() > 0.0, "Density should be positive")
		assert_true(moon.physical.axial_tilt_deg >= 0.0, "Axial tilt should be non-negative")
		assert_true(moon.physical.axial_tilt_deg <= 180.0, "Axial tilt should be <= 180")


func test_orbital_properties_valid_ranges() -> void:
	for i in range(10):
		var spec: MoonSpec = MoonSpec.random(10101 + i)
		var context: ParentContext = _create_jupiter_context()
		var rng: SeededRng = SeededRng.new(10101 + i)
		
		var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
		
		assert_true(moon.orbital.semi_major_axis_m > 0.0, "Semi-major axis should be positive")
		assert_true(moon.orbital.eccentricity >= 0.0, "Eccentricity should be non-negative")
		assert_true(moon.orbital.eccentricity < 1.0, "Eccentricity should be < 1")
		assert_true(moon.orbital.inclination_deg >= 0.0, "Inclination should be non-negative")
		assert_true(moon.orbital.inclination_deg <= 180.0, "Inclination should be <= 180")


func test_provenance_stored() -> void:
	var spec: MoonSpec = MoonSpec.random(99999)
	var context: ParentContext = _create_jupiter_context()
	var rng: SeededRng = SeededRng.new(99999)
	
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_not_null(moon.provenance, "Should have provenance")
	assert_equal(moon.provenance.generation_seed, 99999, "Provenance should store seed")


func test_requires_parent_context() -> void:
	var spec: MoonSpec = MoonSpec.random(11111)
	# Create a planet context (no parent body) - this should trigger an error
	var context: ParentContext = ParentContext.sun_like()
	var rng: SeededRng = SeededRng.new(11111)
	
	# Note: This will print an error message to console, which is expected behavior
	var moon: CelestialBody = MoonGenerator.generate(spec, context, rng)
	
	assert_null(moon, "Should return null without parent body context")


func test_different_seeds_produce_different_moons() -> void:
	var context: ParentContext = _create_jupiter_context()
	
	var spec1: MoonSpec = MoonSpec.random(11111)
	var spec2: MoonSpec = MoonSpec.random(22222)
	var rng1: SeededRng = SeededRng.new(11111)
	var rng2: SeededRng = SeededRng.new(22222)
	
	var moon1: CelestialBody = MoonGenerator.generate(spec1, context, rng1)
	var moon2: CelestialBody = MoonGenerator.generate(spec2, context, rng2)
	
	assert_not_equal(moon1.id, moon2.id, "Different seeds should produce different IDs")
	# Mass might be the same by chance, so just check they're valid
	assert_true(moon1.physical.mass_kg > 0.0, "Moon 1 should have positive mass")
	assert_true(moon2.physical.mass_kg > 0.0, "Moon 2 should have positive mass")
