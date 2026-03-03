## Unit tests for MaterialFactory.
extends "res://Tests/Framework/TestCase.gd"

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


## Returns the display name for this test case.
## @return: The test suite name.
func get_test_name() -> String:
	return "MaterialFactory"


## Clears the shared material cache before each test.
func before_each() -> void:
	MaterialFactory.clear_cache()


## Clears the shared material cache after each test.
func after_each() -> void:
	MaterialFactory.clear_cache()


## Creates a basic rocky body suitable for material tests.
## @return: A configured planet body.
func _create_rocky_body() -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0,
		23.5,
		0.0033,
		7.8e22,
		4.4e13
	)
	var body: CelestialBody = CelestialBody.new(
		"test_rocky",
		"Test Rocky",
		CelestialType.Type.PLANET,
		physical,
		Provenance.new(24680, "1.0.0", 0, 0, {})
	)
	body.surface = SurfaceProps.new(288.0, 0.3, "continental", 0.1, {"iron": 0.2})
	return body


## Creates a basic stellar body suitable for material tests.
## @return: A configured star body.
func _create_star_body() -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		1.989e30,
		6.9634e8,
		2.16e6,
		7.25,
		0.0,
		0.0,
		0.0
	)
	var body: CelestialBody = CelestialBody.new(
		"test_star",
		"Test Star",
		CelestialType.Type.STAR,
		physical,
		Provenance.new(13579, "1.0.0", 0, 0, {})
	)
	body.stellar = StellarProps.new(
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		"G2V",
		"main_sequence",
		1.0,
		4.6e9
	)
	return body


## Ensures null bodies fall back to the default material.
func test_null_body_returns_default_material() -> void:
	var material: Material = MaterialFactory.create_body_material(null)

	assert_not_null(material, "Null bodies should still return a material")
	assert_true(material is StandardMaterial3D, "Null bodies should use the default standard material")


## Ensures repeated material requests for the same body reuse the cache.
func test_same_body_reuses_cached_material() -> void:
	var body: CelestialBody = _create_rocky_body()
	var first_material: Material = MaterialFactory.create_body_material(body)
	var second_material: Material = MaterialFactory.create_body_material(body)

	assert_not_null(first_material, "First material should be created")
	assert_equal(first_material, second_material, "Same body should reuse the cached material")


## Ensures star bodies use the star shader material path.
func test_star_body_uses_star_shader_material() -> void:
	var body: CelestialBody = _create_star_body()
	var material: Material = MaterialFactory.create_body_material(body)
	var shader_material: ShaderMaterial = material as ShaderMaterial

	assert_true(material is ShaderMaterial, "Stars should use a shader material")
	assert_not_null(shader_material, "Star material should cast to ShaderMaterial")
	assert_not_null(shader_material.shader, "Star material should have a shader assigned")
	assert_float_equal(
		float(shader_material.get_shader_parameter("u_temperature")),
		5778.0,
		0.01,
		"Star shader should receive the stellar temperature"
	)
