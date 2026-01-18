## Tests for PlanetGenerator.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Creates a standard sun-like context for testing.
func _create_sun_context() -> ParentContext:
	return ParentContext.sun_like()


## Tests that generation is deterministic.
func test_determinism() -> void:
	var spec: PlanetSpec = PlanetSpec.random(12345)
	var context: ParentContext = _create_sun_context()
	
	var rng1: SeededRng = SeededRng.new(spec.generation_seed)
	var rng2: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet1: CelestialBody = PlanetGenerator.generate(spec, context, rng1)
	var planet2: CelestialBody = PlanetGenerator.generate(spec, context, rng2)
	
	assert_equal(planet1.id, planet2.id, "IDs should match")
	assert_float_equal(planet1.physical.mass_kg, planet2.physical.mass_kg, 0.001, "Mass should match")
	assert_float_equal(planet1.physical.radius_m, planet2.physical.radius_m, 0.001, "Radius should match")
	assert_float_equal(planet1.orbital.semi_major_axis_m, planet2.orbital.semi_major_axis_m, 0.001, "Orbit should match")
	assert_float_equal(planet1.physical.rotation_period_s, planet2.physical.rotation_period_s, 0.001, "Rotation should match")


## Tests that different seeds produce different planets.
func test_different_seeds() -> void:
	var spec1: PlanetSpec = PlanetSpec.random(11111)
	var spec2: PlanetSpec = PlanetSpec.random(22222)
	var context: ParentContext = _create_sun_context()
	
	var rng1: SeededRng = SeededRng.new(spec1.generation_seed)
	var rng2: SeededRng = SeededRng.new(spec2.generation_seed)
	
	var planet1: CelestialBody = PlanetGenerator.generate(spec1, context, rng1)
	var planet2: CelestialBody = PlanetGenerator.generate(spec2, context, rng2)
	
	assert_not_equal(planet1.id, planet2.id, "IDs should differ")


## Tests that generated planet passes validation.
func test_validation_passes() -> void:
	var spec: PlanetSpec = PlanetSpec.random(42)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	var result: ValidationResult = CelestialValidator.validate(planet)
	
	assert_true(result.is_valid(), "Generated planet should pass validation")


## Tests Earth-like preset produces terrestrial temperate planet.
func test_earth_like_preset() -> void:
	var spec: PlanetSpec = PlanetSpec.earth_like(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	# Check mass is in terrestrial range
	var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
	var mass_range: Dictionary = SizeTable.get_mass_range(SizeCategory.Category.TERRESTRIAL)
	assert_in_range(mass_earth, mass_range["min"], mass_range["max"], "Mass should be terrestrial")
	
	# Check orbit is in temperate zone
	var orbit_range: Dictionary = OrbitTable.get_distance_range(
		OrbitZone.Zone.TEMPERATE,
		context.stellar_luminosity_watts
	)
	assert_in_range(
		planet.orbital.semi_major_axis_m,
		orbit_range["min"],
		orbit_range["max"],
		"Orbit should be temperate"
	)


## Tests hot Jupiter preset produces gas giant in hot zone.
func test_hot_jupiter_preset() -> void:
	var spec: PlanetSpec = PlanetSpec.hot_jupiter(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	# Check mass is in gas giant range
	var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
	var mass_range: Dictionary = SizeTable.get_mass_range(SizeCategory.Category.GAS_GIANT)
	assert_in_range(mass_earth, mass_range["min"], mass_range["max"], "Mass should be gas giant")
	
	# Check orbit is in hot zone
	var orbit_range: Dictionary = OrbitTable.get_distance_range(
		OrbitZone.Zone.HOT,
		context.stellar_luminosity_watts
	)
	assert_in_range(
		planet.orbital.semi_major_axis_m,
		orbit_range["min"],
		orbit_range["max"],
		"Orbit should be hot"
	)


## Tests dwarf planet preset produces small cold body.
func test_dwarf_planet_preset() -> void:
	var spec: PlanetSpec = PlanetSpec.dwarf_planet(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	# Check mass is in dwarf range
	var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
	var mass_range: Dictionary = SizeTable.get_mass_range(SizeCategory.Category.DWARF)
	assert_in_range(mass_earth, mass_range["min"], mass_range["max"], "Mass should be dwarf")


## Tests physical properties are positive.
func test_physical_properties_positive() -> void:
	for seed_val in [1, 2, 3, 4, 5]:
		var spec: PlanetSpec = PlanetSpec.random(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		assert_greater_than(planet.physical.mass_kg, 0.0, "Mass should be positive")
		assert_greater_than(planet.physical.radius_m, 0.0, "Radius should be positive")
		# Rotation can be negative (retrograde)
		assert_not_equal(planet.physical.rotation_period_s, 0.0, "Rotation should be non-zero")


## Tests orbital properties are valid.
func test_orbital_properties_valid() -> void:
	for seed_val in [10, 20, 30, 40, 50]:
		var spec: PlanetSpec = PlanetSpec.random(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		assert_greater_than(planet.orbital.semi_major_axis_m, 0.0, "Semi-major axis should be positive")
		assert_in_range(planet.orbital.eccentricity, 0.0, 1.0, "Eccentricity should be 0-1")
		assert_in_range(planet.orbital.inclination_deg, 0.0, 180.0, "Inclination should be 0-180")


## Tests tidal locking for close-in planets.
func test_tidal_locking_close_in() -> void:
	var spec: PlanetSpec = PlanetSpec.new(
		12345,
		SizeCategory.Category.TERRESTRIAL,
		OrbitZone.Zone.HOT
	)
	# Override to very close orbit
	spec.set_override("orbital.semi_major_axis_m", 0.05 * Units.AU_METERS)
	
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	# Calculate expected orbital period
	var orbital_period: float = planet.orbital.get_orbital_period_s(context.stellar_mass_kg)
	
	# For very close orbits around old stars, should be tidally locked
	# Rotation period should equal orbital period (within tolerance)
	assert_float_equal(
		absf(planet.physical.rotation_period_s),
		orbital_period,
		orbital_period * 0.01,  # 1% tolerance
		"Close-in planet should be tidally locked"
	)


## Tests that distant planets are not tidally locked.
func test_not_tidally_locked_distant() -> void:
	var spec: PlanetSpec = PlanetSpec.new(
		12345,
		SizeCategory.Category.GAS_GIANT,
		OrbitZone.Zone.COLD
	)
	# Override to distant orbit
	spec.set_override("orbital.semi_major_axis_m", 10.0 * Units.AU_METERS)
	
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	# Orbital period at 10 AU is very long
	var orbital_period: float = planet.orbital.get_orbital_period_s(context.stellar_mass_kg)
	
	# Rotation should be much shorter than orbital period
	assert_less_than(
		absf(planet.physical.rotation_period_s),
		orbital_period * 0.01,  # Should be less than 1% of orbital period
		"Distant planet should not be tidally locked"
	)


## Tests density is within expected range for size category.
func test_density_matches_category() -> void:
	var categories: Array = [
		SizeCategory.Category.TERRESTRIAL,
		SizeCategory.Category.GAS_GIANT,
		SizeCategory.Category.NEPTUNE_CLASS,
	]
	
	for cat in categories:
		var spec: PlanetSpec = PlanetSpec.new(12345, cat, OrbitZone.Zone.TEMPERATE)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		var density: float = planet.physical.get_density_kg_m3()
		var density_range: Dictionary = SizeTable.get_density_range(cat)
		
		# Allow some margin for mass/radius variation
		var margin: float = (density_range["max"] - density_range["min"]) * 0.3
		assert_in_range(
			density,
			density_range["min"] - margin,
			density_range["max"] + margin,
			"Density should match category: " + SizeCategory.to_string_name(cat)
		)


## Tests provenance is stored correctly.
func test_provenance() -> void:
	var spec: PlanetSpec = PlanetSpec.new(
		99999,
		SizeCategory.Category.TERRESTRIAL,
		OrbitZone.Zone.TEMPERATE,
		null,
		null,
		-1,
		"Test Planet"
	)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_not_null(planet.provenance, "Provenance should exist")
	assert_equal(planet.provenance.generation_seed, 99999, "Seed should match")
	assert_equal(planet.provenance.generator_version, Versions.GENERATOR_VERSION, "Generator version should match")
	assert_equal(planet.provenance.schema_version, Versions.SCHEMA_VERSION, "Schema version should match")
	
	var snapshot: Dictionary = planet.provenance.spec_snapshot
	assert_equal(snapshot["size_category"], SizeCategory.Category.TERRESTRIAL, "Size category should be in snapshot")
	assert_equal(snapshot["orbit_zone"], OrbitZone.Zone.TEMPERATE, "Orbit zone should be in snapshot")


## Tests name hint is used when provided.
func test_name_hint() -> void:
	var spec: PlanetSpec = PlanetSpec.new(12345, -1, -1, null, null, -1, "Kepler-442b")
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_equal(planet.name, "Kepler-442b", "Name hint should be used")


## Tests overrides are respected.
func test_overrides() -> void:
	var spec: PlanetSpec = PlanetSpec.random(12345)
	spec.set_override("physical.mass_earth", 2.0)
	spec.set_override("orbital.semi_major_axis_m", 1.5 * Units.AU_METERS)
	
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
	
	assert_float_equal(mass_earth, 2.0, 0.001, "Mass override should be respected")
	assert_float_equal(
		planet.orbital.semi_major_axis_m,
		1.5 * Units.AU_METERS,
		1000.0,
		"Orbit override should be respected"
	)


## Tests generated planet has correct type.
func test_planet_type() -> void:
	var spec: PlanetSpec = PlanetSpec.random(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_equal(planet.type, CelestialType.Type.PLANET, "Type should be PLANET")
	assert_true(planet.has_orbital(), "Planet should have orbital data")
	assert_false(planet.has_stellar(), "Planet should not have stellar data")


## Tests axial tilt is within valid range.
func test_axial_tilt_range() -> void:
	for seed_val in [100, 200, 300, 400, 500]:
		var spec: PlanetSpec = PlanetSpec.random(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		assert_in_range(
			planet.physical.axial_tilt_deg,
			0.0,
			180.0,
			"Axial tilt should be 0-180 degrees"
		)


## Tests oblateness is reasonable.
func test_oblateness_reasonable() -> void:
	for seed_val in [111, 222, 333]:
		var spec: PlanetSpec = PlanetSpec.random(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		assert_in_range(
			planet.physical.oblateness,
			0.0,
			0.2,  # No planet is more oblate than ~0.15
			"Oblateness should be reasonable"
		)


## Tests magnetic moment is non-negative.
func test_magnetic_moment_non_negative() -> void:
	for seed_val in [1000, 2000, 3000]:
		var spec: PlanetSpec = PlanetSpec.random(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		assert_true(
			planet.physical.magnetic_moment >= 0.0,
			"Magnetic moment should be non-negative"
		)


## Tests internal heat is non-negative.
func test_internal_heat_non_negative() -> void:
	for seed_val in [4000, 5000, 6000]:
		var spec: PlanetSpec = PlanetSpec.random(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		assert_true(
			planet.physical.internal_heat_watts >= 0.0,
			"Internal heat should be non-negative"
		)
