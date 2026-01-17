## Tests for CelestialBody data model.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests creation with minimal values.
func test_minimal_creation() -> void:
	var body: CelestialBody = CelestialBody.new("test_001", "Test Planet")
	assert_equal(body.id, "test_001")
	assert_equal(body.name, "Test Planet")
	assert_equal(body.type, CelestialType.Type.PLANET)
	assert_not_null(body.physical)


## Tests creation with all parameters.
func test_full_creation() -> void:
	var physical: PhysicalProps = PhysicalProps.new(1.0e24, 6.0e6)
	var provenance: Provenance = Provenance.create_current(12345)
	var body: CelestialBody = CelestialBody.new(
		"star_001",
		"Test Star",
		CelestialType.Type.STAR,
		physical,
		provenance
	)

	assert_equal(body.id, "star_001")
	assert_equal(body.name, "Test Star")
	assert_equal(body.type, CelestialType.Type.STAR)
	assert_equal(body.physical.mass_kg, 1.0e24)
	assert_not_null(body.provenance)
	assert_equal(body.provenance.generation_seed, 12345)


## Tests optional component flags.
func test_has_component_flags() -> void:
	var body: CelestialBody = CelestialBody.new("test_001", "Test")

	assert_false(body.has_orbital())
	assert_false(body.has_stellar())
	assert_false(body.has_surface())
	assert_false(body.has_atmosphere())
	assert_false(body.has_ring_system())

	body.orbital = OrbitalProps.new()
	assert_true(body.has_orbital())

	body.stellar = StellarProps.new()
	assert_true(body.has_stellar())

	body.surface = SurfaceProps.new()
	assert_true(body.has_surface())

	body.atmosphere = AtmosphereProps.new()
	assert_true(body.has_atmosphere())

	var band: RingBand = RingBand.new(1.0e8, 2.0e8)
	var bands: Array[RingBand] = [band]
	body.ring_system = RingSystemProps.new(bands)
	assert_true(body.has_ring_system())


## Tests type string conversion.
func test_type_string() -> void:
	var body: CelestialBody = CelestialBody.new("test", "Test", CelestialType.Type.MOON)
	assert_equal(body.get_type_string(), "Moon")


## Tests all celestial types.
func test_all_types() -> void:
	var types: Array = [
		CelestialType.Type.STAR,
		CelestialType.Type.PLANET,
		CelestialType.Type.MOON,
		CelestialType.Type.ASTEROID,
	]

	for t in types:
		var body: CelestialBody = CelestialBody.new("test", "Test", t)
		assert_equal(body.type, t)
		assert_true(body.get_type_string().length() > 0)
