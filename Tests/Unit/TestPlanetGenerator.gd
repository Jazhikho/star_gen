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


# =============================================================================
# STAGE 4 TESTS (Atmosphere & Surface)
# =============================================================================


## Tests that gas giants always have atmospheres.
func test_gas_giant_has_atmosphere() -> void:
	var spec: PlanetSpec = PlanetSpec.new(
		12345,
		SizeCategory.Category.GAS_GIANT,
		OrbitZone.Zone.COLD
	)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_true(planet.has_atmosphere(), "Gas giant should have atmosphere")
	assert_false(planet.has_surface(), "Gas giant should not have solid surface")


## Tests that gas giants have H2/He dominated atmospheres.
func test_gas_giant_composition() -> void:
	var spec: PlanetSpec = PlanetSpec.new(
		12345,
		SizeCategory.Category.GAS_GIANT,
		OrbitZone.Zone.COLD
	)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_true(planet.has_atmosphere(), "Should have atmosphere")
	var h2_fraction: float = planet.atmosphere.composition.get("H2", 0.0) as float
	var he_fraction: float = planet.atmosphere.composition.get("He", 0.0) as float
	
	assert_greater_than(h2_fraction, 0.5, "Gas giant should be H2 dominated")
	assert_greater_than(he_fraction, 0.05, "Gas giant should have significant He")


## Tests that rocky planets have surface properties.
func test_rocky_planet_has_surface() -> void:
	var spec: PlanetSpec = PlanetSpec.earth_like(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_true(planet.has_surface(), "Rocky planet should have surface")
	assert_not_null(planet.surface.terrain, "Rocky planet should have terrain")


## Tests atmosphere composition sums to approximately 1.
func test_atmosphere_composition_normalized() -> void:
	for seed_val in [100, 200, 300]:
		var spec: PlanetSpec = PlanetSpec.random(seed_val)
		spec.has_atmosphere = true  # Force atmosphere
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		if planet.has_atmosphere():
			var comp_sum: float = planet.atmosphere.get_composition_sum()
			assert_float_equal(comp_sum, 1.0, 0.01, "Composition should sum to 1.0")


## Tests that surface temperature reflects greenhouse effect.
func test_greenhouse_effect() -> void:
	var spec: PlanetSpec = PlanetSpec.earth_like(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	if planet.has_atmosphere() and planet.has_surface():
		# Equilibrium temp without greenhouse
		var equilibrium_temp: float = context.get_equilibrium_temperature_k(0.3)
		
		# Surface temp should be >= equilibrium (greenhouse warms)
		assert_true(
			planet.surface.temperature_k >= equilibrium_temp * 0.9,
			"Surface temp should reflect greenhouse warming"
		)
		
		# Greenhouse factor should be >= 1
		assert_true(
			planet.atmosphere.greenhouse_factor >= 1.0,
			"Greenhouse factor should be >= 1"
		)


## Tests that small cold bodies have minimal/no atmosphere.
func test_small_cold_body_atmosphere() -> void:
	var spec: PlanetSpec = PlanetSpec.dwarf_planet(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	# Dwarf planets may or may not have atmosphere, but if they do it's thin
	if planet.has_atmosphere():
		assert_less_than(
			planet.atmosphere.surface_pressure_pa,
			1000.0,
			"Dwarf planet atmosphere should be thin"
		)


## Tests terrain properties are within valid ranges.
func test_terrain_properties_valid() -> void:
	var spec: PlanetSpec = PlanetSpec.earth_like(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_true(planet.has_surface(), "Should have surface")
	assert_not_null(planet.surface.terrain, "Should have terrain")
	
	var terrain: TerrainProps = planet.surface.terrain
	assert_greater_than(terrain.elevation_range_m, 0.0, "Elevation range should be positive")
	assert_in_range(terrain.roughness, 0.0, 1.0, "Roughness should be 0-1")
	assert_in_range(terrain.crater_density, 0.0, 1.0, "Crater density should be 0-1")
	assert_in_range(terrain.tectonic_activity, 0.0, 1.0, "Tectonic activity should be 0-1")
	assert_in_range(terrain.erosion_level, 0.0, 1.0, "Erosion level should be 0-1")


## Tests temperate zone planets can have hydrosphere.
func test_temperate_planet_hydrosphere() -> void:
	# Test multiple seeds to find one with hydrosphere
	var found_hydrosphere: bool = false
	for seed_val in range(100, 200):
		var spec: PlanetSpec = PlanetSpec.earth_like(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		if planet.has_surface() and planet.surface.has_hydrosphere():
			found_hydrosphere = true
			# Validate hydrosphere properties
			var hydro: HydrosphereProps = planet.surface.hydrosphere
			assert_in_range(hydro.ocean_coverage, 0.0, 1.0, "Ocean coverage should be 0-1")
			assert_in_range(hydro.ice_coverage, 0.0, 1.0, "Ice coverage should be 0-1")
			assert_greater_than(hydro.ocean_depth_m, 0.0, "Ocean depth should be positive")
			break
	
	assert_true(found_hydrosphere, "Should find at least one temperate planet with hydrosphere")


## Tests cold planets have cryosphere.
func test_cold_planet_cryosphere() -> void:
	var spec: PlanetSpec = PlanetSpec.new(
		12345,
		SizeCategory.Category.TERRESTRIAL,
		OrbitZone.Zone.COLD
	)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_true(planet.has_surface(), "Should have surface")
	assert_true(planet.surface.has_cryosphere(), "Cold planet should have cryosphere")
	
	var cryo: CryosphereProps = planet.surface.cryosphere
	assert_in_range(cryo.polar_cap_coverage, 0.0, 1.0, "Polar cap coverage should be 0-1")
	assert_true(cryo.permafrost_depth_m >= 0.0, "Permafrost depth should be non-negative")
	assert_in_range(cryo.cryovolcanism_level, 0.0, 1.0, "Cryovolcanism should be 0-1")


## Tests surface albedo is within valid range.
func test_surface_albedo_valid() -> void:
	for seed_val in [500, 600, 700]:
		var spec: PlanetSpec = PlanetSpec.earth_like(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		if planet.has_surface():
			assert_in_range(
				planet.surface.albedo,
				0.0,
				1.0,
				"Albedo should be 0-1"
			)


## Tests volcanism is within valid range.
func test_volcanism_valid() -> void:
	for seed_val in [800, 900, 1000]:
		var spec: PlanetSpec = PlanetSpec.earth_like(seed_val)
		var context: ParentContext = _create_sun_context()
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		
		if planet.has_surface():
			assert_in_range(
				planet.surface.volcanism_level,
				0.0,
				1.0,
				"Volcanism level should be 0-1"
			)


## Tests atmosphere scale height is positive when atmosphere exists.
func test_atmosphere_scale_height_positive() -> void:
	var spec: PlanetSpec = PlanetSpec.earth_like(12345)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	if planet.has_atmosphere():
		assert_greater_than(
			planet.atmosphere.scale_height_m,
			0.0,
			"Scale height should be positive"
		)


## Tests has_atmosphere spec preference is respected.
func test_atmosphere_preference_respected() -> void:
	# Force no atmosphere on a super-earth
	var spec: PlanetSpec = PlanetSpec.new(
		12345,
		SizeCategory.Category.SUPER_EARTH,
		OrbitZone.Zone.TEMPERATE,
		false  # No atmosphere
	)
	var context: ParentContext = _create_sun_context()
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
	
	assert_false(planet.has_atmosphere(), "Should respect no-atmosphere preference")


## Tests that validation still passes with all new components.
func test_full_planet_validation() -> void:
	# Test several planet types
	var specs: Array = [
		PlanetSpec.earth_like(111),
		PlanetSpec.hot_jupiter(222),
		PlanetSpec.mars_like(333),
		PlanetSpec.ice_giant(444),
		PlanetSpec.dwarf_planet(555),
	]
	
	var context: ParentContext = _create_sun_context()
	
	for spec in specs:
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		var planet: CelestialBody = PlanetGenerator.generate(spec, context, rng)
		var result: ValidationResult = CelestialValidator.validate(planet)
		
		assert_true(result.is_valid(), "Planet should pass validation")


## Tests determinism includes atmosphere and surface.
func test_determinism_includes_atmosphere_surface() -> void:
	var spec: PlanetSpec = PlanetSpec.earth_like(12345)
	var context: ParentContext = _create_sun_context()
	
	var rng1: SeededRng = SeededRng.new(spec.generation_seed)
	var rng2: SeededRng = SeededRng.new(spec.generation_seed)
	
	var planet1: CelestialBody = PlanetGenerator.generate(spec, context, rng1)
	var planet2: CelestialBody = PlanetGenerator.generate(spec, context, rng2)
	
	# Check atmosphere determinism
	assert_equal(planet1.has_atmosphere(), planet2.has_atmosphere(), "Atmosphere presence should match")
	if planet1.has_atmosphere():
		assert_float_equal(
			planet1.atmosphere.surface_pressure_pa,
			planet2.atmosphere.surface_pressure_pa,
			0.001,
			"Atmosphere pressure should match"
		)
		assert_float_equal(
			planet1.atmosphere.greenhouse_factor,
			planet2.atmosphere.greenhouse_factor,
			0.001,
			"Greenhouse factor should match"
		)
	
	# Check surface determinism
	assert_equal(planet1.has_surface(), planet2.has_surface(), "Surface presence should match")
	if planet1.has_surface():
		assert_float_equal(
			planet1.surface.temperature_k,
			planet2.surface.temperature_k,
			0.001,
			"Surface temperature should match"
		)
		assert_equal(
			planet1.surface.surface_type,
			planet2.surface.surface_type,
			"Surface type should match"
		)
