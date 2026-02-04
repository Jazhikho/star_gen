## Tests for StarGenerator.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


## Tests that generation is deterministic.
func test_determinism() -> void:
	var spec: StarSpec = StarSpec.random(12345)
	
	var rng1: SeededRng = SeededRng.new(spec.generation_seed)
	var rng2: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star1: CelestialBody = StarGenerator.generate(spec, rng1)
	var star2: CelestialBody = StarGenerator.generate(spec, rng2)
	
	assert_equal(star1.id, star2.id)
	assert_equal(star1.name, star2.name)
	assert_float_equal(star1.physical.mass_kg, star2.physical.mass_kg)
	assert_float_equal(star1.physical.radius_m, star2.physical.radius_m)
	assert_float_equal(star1.stellar.luminosity_watts, star2.stellar.luminosity_watts)
	assert_float_equal(star1.stellar.effective_temperature_k, star2.stellar.effective_temperature_k)
	assert_equal(star1.stellar.spectral_class, star2.stellar.spectral_class)


## Tests that different seeds produce different stars.
func test_different_seeds() -> void:
	var spec1: StarSpec = StarSpec.random(11111)
	var spec2: StarSpec = StarSpec.random(22222)
	
	var rng1: SeededRng = SeededRng.new(spec1.generation_seed)
	var rng2: SeededRng = SeededRng.new(spec2.generation_seed)
	
	var star1: CelestialBody = StarGenerator.generate(spec1, rng1)
	var star2: CelestialBody = StarGenerator.generate(spec2, rng2)
	
	# IDs should differ
	assert_not_equal(star1.id, star2.id)


## Tests that generated star passes validation.
func test_validation_passes() -> void:
	var spec: StarSpec = StarSpec.random(42)
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	var result: ValidationResult = CelestialValidator.validate(star)
	
	assert_true(result.is_valid(), "Generated star should pass validation")


## Tests sun-like preset produces G-class star.
func test_sun_like_preset() -> void:
	var spec: StarSpec = StarSpec.sun_like(12345)
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	assert_true(star.stellar.spectral_class.begins_with("G"))
	assert_equal(star.stellar.spectral_class.substr(1, 1), "2")
	assert_equal(star.stellar.stellar_type, "main_sequence")


## Tests red dwarf preset produces M-class star.
func test_red_dwarf_preset() -> void:
	var spec: StarSpec = StarSpec.red_dwarf(12345)
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	assert_true(star.stellar.spectral_class.begins_with("M"))


## Tests hot blue preset produces B-class star.
func test_hot_blue_preset() -> void:
	var spec: StarSpec = StarSpec.hot_blue(12345)
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	assert_true(star.stellar.spectral_class.begins_with("B"))


## Tests temperature matches spectral class.
func test_temperature_matches_class() -> void:
	for seed_val in [100, 200, 300, 400, 500]:
		var spec: StarSpec = StarSpec.random(seed_val)
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var star: CelestialBody = StarGenerator.generate(spec, rng)
		var spectral_letter: String = star.stellar.spectral_class.substr(0, 1)
		var expected_class: StarClass.SpectralClass = StarClass.from_letter(spectral_letter) as StarClass.SpectralClass
		
		var temp_range: Dictionary = StarTable.get_temperature_range(expected_class)
		
		# Allow some margin for temperature variation
		var margin: float = (temp_range["max"] - temp_range["min"]) * 0.2
		assert_in_range(
			star.stellar.effective_temperature_k,
			temp_range["min"] - margin,
			temp_range["max"] + margin
		)


## Tests mass-luminosity relationship is reasonable.
func test_mass_luminosity_relationship() -> void:
	var spec: StarSpec = StarSpec.sun_like(12345)
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	var mass_solar: float = star.physical.mass_kg / Units.SOLAR_MASS_KG
	var lum_solar: float = star.stellar.luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	
	# For main sequence: L ∝ M^3.5, so for 1 solar mass, L should be ~1
	# With variation, check it's in reasonable range
	assert_in_range(mass_solar, 0.8, 1.2)
	assert_in_range(lum_solar, 0.5, 2.0)


## Tests provenance is stored correctly.
func test_provenance() -> void:
	var spec: StarSpec = StarSpec.new(99999, StarClass.SpectralClass.K, 5, 1.2, 5.0e9, "Test Star")
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	assert_not_null(star.provenance)
	assert_equal(star.provenance.generation_seed, 99999)
	assert_equal(star.provenance.generator_version, Versions.GENERATOR_VERSION)
	assert_equal(star.provenance.schema_version, Versions.SCHEMA_VERSION)
	
	# Spec snapshot should contain our spec data
	var snapshot: Dictionary = star.provenance.spec_snapshot
	assert_equal(snapshot["spectral_class"], StarClass.SpectralClass.K)
	assert_equal(snapshot["subclass"], 5)


## Tests name hint is used when provided.
func test_name_hint() -> void:
	var spec: StarSpec = StarSpec.new(12345, -1, -1, -1.0, -1.0, "Sol")
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	assert_equal(star.name, "Sol")


## Tests overrides are respected.
func test_overrides() -> void:
	var spec: StarSpec = StarSpec.random(12345)
	spec.set_override("physical.mass_solar", 2.0)
	spec.set_override("stellar.luminosity_solar", 10.0)
	
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	var mass_solar: float = star.physical.mass_kg / Units.SOLAR_MASS_KG
	var lum_solar: float = star.stellar.luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	
	assert_float_equal(mass_solar, 2.0)
	assert_float_equal(lum_solar, 10.0)


## Tests generated star has correct type.
func test_star_type() -> void:
	var spec: StarSpec = StarSpec.random(12345)
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	assert_equal(star.type, CelestialType.Type.STAR)
	assert_true(star.has_stellar())
	assert_false(star.has_surface())


## Tests physical properties are positive.
func test_physical_properties_positive() -> void:
	for seed_val in [1, 2, 3, 4, 5]:
		var spec: StarSpec = StarSpec.random(seed_val)
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var star: CelestialBody = StarGenerator.generate(spec, rng)
		
		assert_greater_than(star.physical.mass_kg, 0.0)
		assert_greater_than(star.physical.radius_m, 0.0)
		assert_greater_than(star.physical.rotation_period_s, 0.0)


## Tests stellar properties are positive.
func test_stellar_properties_positive() -> void:
	for seed_val in [10, 20, 30, 40, 50]:
		var spec: StarSpec = StarSpec.random(seed_val)
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var star: CelestialBody = StarGenerator.generate(spec, rng)
		
		assert_greater_than(star.stellar.luminosity_watts, 0.0)
		assert_greater_than(star.stellar.effective_temperature_k, 0.0)
		assert_greater_than(star.stellar.metallicity, 0.0)
		assert_greater_than(star.stellar.age_years, 0.0)


## Tests age is within main sequence lifetime.
func test_age_within_lifetime() -> void:
	for seed_val in [111, 222, 333, 444, 555]:
		var spec: StarSpec = StarSpec.random(seed_val)
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		
		var star: CelestialBody = StarGenerator.generate(spec, rng)
		
		var spectral_letter: String = star.stellar.spectral_class.substr(0, 1)
		var spectral_class: StarClass.SpectralClass = StarClass.from_letter(spectral_letter) as StarClass.SpectralClass
		var lifetime_range: Dictionary = StarTable.get_lifetime_range(spectral_class)
		
		assert_less_than(star.stellar.age_years, lifetime_range["max"])


## Tests habitable zone calculation.
func test_habitable_zone() -> void:
	var spec: StarSpec = StarSpec.sun_like(12345)
	var rng: SeededRng = SeededRng.new(spec.generation_seed)
	
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	
	var hz_inner: float = star.stellar.get_habitable_zone_inner_m()
	var hz_outer: float = star.stellar.get_habitable_zone_outer_m()
	
	# For Sun-like star, HZ should be roughly 0.95 - 1.37 AU
	assert_greater_than(hz_inner, 0.8 * Units.AU_METERS)
	assert_less_than(hz_inner, 1.2 * Units.AU_METERS)
	assert_greater_than(hz_outer, 1.2 * Units.AU_METERS)
	assert_less_than(hz_outer, 1.6 * Units.AU_METERS)
