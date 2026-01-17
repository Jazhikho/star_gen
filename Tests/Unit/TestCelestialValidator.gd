## Tests for CelestialValidator.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Creates a valid Earth-like planet for testing.
func _create_valid_planet() -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		5.972e24,
		6.371e6,
		86400.0,
		23.5
	)
	var provenance: Provenance = Provenance.create_current(12345)
	var body: CelestialBody = CelestialBody.new(
		"earth_001",
		"Earth-like",
		CelestialType.Type.PLANET,
		physical,
		provenance
	)
	return body


## Tests validation passes for valid body.
func test_valid_body_passes() -> void:
	var body: CelestialBody = _create_valid_planet()
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_true(result.is_valid(), "Valid body should pass validation")


## Tests empty ID fails validation.
func test_empty_id_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.id = ""
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())
	assert_greater_than(result.get_error_count(), 0)


## Tests zero mass fails validation.
func test_zero_mass_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.physical.mass_kg = 0.0
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests negative mass fails validation.
func test_negative_mass_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.physical.mass_kg = -1.0
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests zero radius fails validation.
func test_zero_radius_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.physical.radius_m = 0.0
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests invalid orbital eccentricity fails validation.
func test_negative_eccentricity_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.orbital = OrbitalProps.new(1.5e11, -0.1)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests unbound orbit eccentricity generates warning.
func test_high_eccentricity_warns() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.orbital = OrbitalProps.new(1.5e11, 1.5)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_true(result.is_valid())
	assert_greater_than(result.get_warning_count(), 0)


## Tests invalid albedo fails validation.
func test_invalid_albedo_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.surface = SurfaceProps.new(288.0, 1.5)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests negative temperature fails validation.
func test_negative_temperature_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.surface = SurfaceProps.new(-10.0, 0.3)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests atmosphere composition warning.
func test_atmosphere_composition_warning() -> void:
	var body: CelestialBody = _create_valid_planet()
	body.atmosphere = AtmosphereProps.new(101325.0, 8500.0, {"N2": 0.5})
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_true(result.is_valid())
	assert_greater_than(result.get_warning_count(), 0)


## Tests ring band inner radius less than outer radius.
func test_ring_band_radius_order() -> void:
	var body: CelestialBody = _create_valid_planet()
	var bad_band: RingBand = RingBand.new(2.0e8, 1.0e8)  # inner > outer
	var bands: Array[RingBand] = [bad_band]
	body.ring_system = RingSystemProps.new(bands)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests ring band inside body fails.
func test_ring_band_inside_body_fails() -> void:
	var body: CelestialBody = _create_valid_planet()
	var bad_band: RingBand = RingBand.new(1.0e6, 2.0e8)  # inner < body radius
	var bands: Array[RingBand] = [bad_band]
	body.ring_system = RingSystemProps.new(bands)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_false(result.is_valid())


## Tests valid ring system passes.
func test_valid_ring_system_passes() -> void:
	var body: CelestialBody = _create_valid_planet()
	var band: RingBand = RingBand.new(1.0e8, 2.0e8, 0.5, {"ice": 1.0}, 1.0, "Main")
	var bands: Array[RingBand] = [band]
	body.ring_system = RingSystemProps.new(bands, 1.0e18)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_true(result.is_valid())


## Tests stellar validation on star.
func test_star_with_stellar_passes() -> void:
	var physical: PhysicalProps = PhysicalProps.new(1.989e30, 6.957e8)
	var body: CelestialBody = CelestialBody.new(
		"sun_001", "Sun", CelestialType.Type.STAR, physical
	)
	body.stellar = StellarProps.new(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_true(result.is_valid())


## Tests star without stellar properties warns.
func test_star_without_stellar_warns() -> void:
	var physical: PhysicalProps = PhysicalProps.new(1.989e30, 6.957e8)
	var body: CelestialBody = CelestialBody.new(
		"sun_001", "Sun", CelestialType.Type.STAR, physical
	)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_greater_than(result.get_warning_count(), 0)


## Tests star with surface property generates warning.
func test_star_with_surface_warns() -> void:
	var physical: PhysicalProps = PhysicalProps.new(1.989e30, 6.957e8)
	var body: CelestialBody = CelestialBody.new(
		"sun_001", "Sun", CelestialType.Type.STAR, physical
	)
	body.surface = SurfaceProps.new(5778.0, 0.0)
	var result: ValidationResult = CelestialValidator.validate(body)
	assert_true(result.is_valid())
	assert_greater_than(result.get_warning_count(), 0)
