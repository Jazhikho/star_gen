## Tests for CelestialSerializer.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


## Creates a fully populated celestial body for testing.
func _create_full_body() -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(5.972e24, 6.371e6, 86400.0, 23.5)
	var provenance: Provenance = Provenance.create_current(12345, {"preset": "earth"})

	var body: CelestialBody = CelestialBody.new(
		"earth_001",
		"Earth-like",
		CelestialType.Type.PLANET,
		physical,
		provenance
	)

	body.orbital = OrbitalProps.new(1.496e11, 0.017, 0.0, 0.0, 0.0, 0.0, "sun_001")
	body.surface = SurfaceProps.new(288.0, 0.306, "terrestrial")
	body.atmosphere = AtmosphereProps.new(
		101325.0, 8500.0, {"N2": 0.78, "O2": 0.21, "Ar": 0.01}, 1.0
	)
	
	body.surface.terrain = TerrainProps.new(20000.0, 0.4, 0.1, 0.7, 0.5, "continental")
	body.surface.hydrosphere = HydrosphereProps.new(0.71, 3688.0, 0.03, 35.0)
	body.surface.cryosphere = CryosphereProps.new(0.05, 500.0, false, 0.0, 0.0)
	
	return body


## Tests to_dict produces required fields.
func test_to_dict_has_required_fields() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)

	assert_true(data.has("schema_version"))
	assert_true(data.has("id"))
	assert_true(data.has("name"))
	assert_true(data.has("type"))
	assert_true(data.has("physical"))
	assert_true(data.has("provenance"))


## Tests to_dict includes optional components.
func test_to_dict_includes_optional_components() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)

	assert_true(data.has("orbital"))
	assert_true(data.has("surface"))
	assert_true(data.has("atmosphere"))


## Tests to_dict excludes null components.
func test_to_dict_excludes_null_components() -> void:
	var physical: PhysicalProps = PhysicalProps.new(1.0e24, 5.0e6)
	var body: CelestialBody = CelestialBody.new("test", "Test", CelestialType.Type.ASTEROID, physical)
	var data: Dictionary = CelestialSerializer.to_dict(body)

	assert_false(data.has("orbital"))
	assert_false(data.has("surface"))
	assert_false(data.has("atmosphere"))
	assert_false(data.has("ring_system"))
	assert_false(data.has("stellar"))


## Tests from_dict restores identity.
func test_from_dict_restores_identity() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)
	var restored: CelestialBody = CelestialSerializer.from_dict(data)

	assert_equal(restored.id, body.id)
	assert_equal(restored.name, body.name)
	assert_equal(restored.type, body.type)


## Tests from_dict restores physical properties.
func test_from_dict_restores_physical() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)
	var restored: CelestialBody = CelestialSerializer.from_dict(data)

	assert_float_equal(restored.physical.mass_kg, body.physical.mass_kg)
	assert_float_equal(restored.physical.radius_m, body.physical.radius_m)


## Tests from_dict restores orbital properties.
func test_from_dict_restores_orbital() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)
	var restored: CelestialBody = CelestialSerializer.from_dict(data)

	assert_true(restored.has_orbital())
	assert_float_equal(restored.orbital.semi_major_axis_m, body.orbital.semi_major_axis_m)
	assert_equal(restored.orbital.parent_id, body.orbital.parent_id)


## Tests from_dict restores atmosphere.
func test_from_dict_restores_atmosphere() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)
	var restored: CelestialBody = CelestialSerializer.from_dict(data)

	assert_true(restored.has_atmosphere())
	assert_float_equal(restored.atmosphere.surface_pressure_pa, body.atmosphere.surface_pressure_pa)
	assert_true(restored.atmosphere.composition.has("N2"))


## Tests from_dict restores surface sub-components.
func test_from_dict_restores_surface_components() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)
	var restored: CelestialBody = CelestialSerializer.from_dict(data)
	
	assert_true(restored.has_surface())
	assert_true(restored.surface.has_terrain())
	assert_true(restored.surface.has_hydrosphere())
	assert_true(restored.surface.has_cryosphere())
	
	assert_float_equal(restored.surface.terrain.elevation_range_m, 20000.0)
	assert_float_equal(restored.surface.hydrosphere.ocean_coverage, 0.71)
	assert_float_equal(restored.surface.cryosphere.polar_cap_coverage, 0.05)


## Tests stellar properties serialization for stars.
func test_stellar_round_trip() -> void:
	var physical: PhysicalProps = PhysicalProps.new(1.989e30, 6.957e8)
	var provenance: Provenance = Provenance.create_current(42)
	var body: CelestialBody = CelestialBody.new(
		"sun_001", "Sun", CelestialType.Type.STAR, physical, provenance
	)
	body.stellar = StellarProps.new(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9)
	
	var json_string: String = CelestialSerializer.to_json(body)
	var restored: CelestialBody = CelestialSerializer.from_json(json_string)
	
	assert_not_null(restored)
	assert_true(restored.has_stellar())
	assert_float_equal(restored.stellar.luminosity_watts, body.stellar.luminosity_watts)
	assert_equal(restored.stellar.spectral_class, body.stellar.spectral_class)


## Tests from_dict restores provenance.
func test_from_dict_restores_provenance() -> void:
	var body: CelestialBody = _create_full_body()
	var data: Dictionary = CelestialSerializer.to_dict(body)
	var restored: CelestialBody = CelestialSerializer.from_dict(data)

	assert_not_null(restored.provenance)
	assert_equal(restored.provenance.generation_seed, body.provenance.generation_seed)
	assert_equal(restored.provenance.generator_version, body.provenance.generator_version)


## Tests JSON round-trip.
func test_json_round_trip() -> void:
	var body: CelestialBody = _create_full_body()
	var json_string: String = CelestialSerializer.to_json(body)
	var restored: CelestialBody = CelestialSerializer.from_json(json_string)

	assert_not_null(restored)
	assert_equal(restored.id, body.id)
	assert_equal(restored.name, body.name)
	assert_float_equal(restored.physical.mass_kg, body.physical.mass_kg)


## Tests from_dict handles missing optional fields.
func test_from_dict_handles_missing_fields() -> void:
	var data: Dictionary = {
		"id": "minimal_001",
		"name": "Minimal",
		"type": "Planet",
		"physical": {"mass_kg": 1.0e24, "radius_m": 5.0e6}
	}
	var body: CelestialBody = CelestialSerializer.from_dict(data)

	assert_not_null(body)
	assert_equal(body.id, "minimal_001")
	assert_false(body.has_orbital())
	assert_false(body.has_surface())


## Tests from_json handles invalid JSON.
func test_from_json_handles_invalid() -> void:
	var body: CelestialBody = CelestialSerializer.from_json("not valid json")
	assert_null(body)


## Tests from_dict handles empty dictionary.
func test_from_dict_handles_empty() -> void:
	var body: CelestialBody = CelestialSerializer.from_dict({})
	assert_null(body)


## Tests schema_version is included in output.
func test_schema_version_included() -> void:
	var physical: PhysicalProps = PhysicalProps.new(1.0e24, 5.0e6)
	var body: CelestialBody = CelestialBody.new("test", "Test", CelestialType.Type.PLANET, physical)
	var data: Dictionary = CelestialSerializer.to_dict(body)

	assert_equal(data["schema_version"], Versions.SCHEMA_VERSION)
