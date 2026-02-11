## Integration tests for population generation wired into PlanetGenerator and MoonGenerator.
## Verifies that population data is generated, deterministic, and properly attached.
extends TestCase

const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _celestial_serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")


## Tests that planet generation with population enabled produces population data.
func test_planet_generation_with_population() -> void:
	var spec: PlanetSpec = PlanetSpec.new(42, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
	var context: ParentContext = ParentContext.sun_like()
	var rng: SeededRng = SeededRng.new(42)
	
	var body: CelestialBody = PlanetGenerator.generate(spec, context, rng, true)
	
	assert_not_null(body, "Planet should be generated")
	assert_true(body.has_population_data(), "Population data should be present when enabled")
	assert_not_null(body.population_data.profile, "Profile should be generated")
	assert_not_null(body.population_data.suitability, "Suitability should be generated")


## Tests that planet generation without population flag does not produce population data.
func test_planet_generation_without_population() -> void:
	var spec: PlanetSpec = PlanetSpec.new(42, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
	var context: ParentContext = ParentContext.sun_like()
	var rng: SeededRng = SeededRng.new(42)
	
	var body: CelestialBody = PlanetGenerator.generate(spec, context, rng, false)
	
	assert_not_null(body, "Planet should be generated")
	assert_false(body.has_population_data(), "Population data should not be present when disabled")


## Tests determinism: same seed produces same population data.
func test_population_determinism() -> void:
	var spec1: PlanetSpec = PlanetSpec.new(100, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
	var spec2: PlanetSpec = PlanetSpec.new(100, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
	var context: ParentContext = ParentContext.sun_like()
	
	var rng1: SeededRng = SeededRng.new(100)
	var rng2: SeededRng = SeededRng.new(100)
	
	var body1: CelestialBody = PlanetGenerator.generate(spec1, context, rng1, true)
	var body2: CelestialBody = PlanetGenerator.generate(spec2, context, rng2, true)
	
	assert_not_null(body1.population_data, "First body should have population data")
	assert_not_null(body2.population_data, "Second body should have population data")
	
	assert_equal(
		body1.population_data.profile.habitability_score,
		body2.population_data.profile.habitability_score,
		"Habitability scores should match"
	)
	assert_equal(
		body1.population_data.suitability.overall_score,
		body2.population_data.suitability.overall_score,
		"Suitability scores should match"
	)
	assert_equal(
		body1.population_data.native_populations.size(),
		body2.population_data.native_populations.size(),
		"Native population count should match"
	)
	assert_equal(
		body1.population_data.colonies.size(),
		body2.population_data.colonies.size(),
		"Colony count should match"
	)


## Tests that moon generation with population enabled produces population data.
func test_moon_generation_with_population() -> void:
	var spec: MoonSpec = MoonSpec.new(42, SizeCategory.Category.SUB_TERRESTRIAL)
	var context: ParentContext = ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		5.2 * Units.AU_METERS,
		1.898e27,
		6.9911e7,
		5.0e8
	)
	var rng: SeededRng = SeededRng.new(42)
	
	var body: CelestialBody = MoonGenerator.generate(spec, context, rng, true)
	
	assert_not_null(body, "Moon should be generated")
	assert_true(body.has_population_data(), "Population data should be present when enabled")


## Tests serialization round-trip of population data.
func test_population_serialization_roundtrip() -> void:
	var spec: PlanetSpec = PlanetSpec.new(55, SizeCategory.Category.TERRESTRIAL, OrbitZone.Zone.TEMPERATE)
	var context: ParentContext = ParentContext.sun_like()
	var rng: SeededRng = SeededRng.new(55)
	
	var body: CelestialBody = PlanetGenerator.generate(spec, context, rng, true)
	assert_true(body.has_population_data(), "Original body should have population data")
	
	# Serialize to dict and back
	var data: Dictionary = CelestialSerializer.to_dict(body)
	assert_true(data.has("population_data"), "Serialized data should include population_data")
	
	var restored: CelestialBody = CelestialSerializer.from_dict(data)
	assert_not_null(restored, "Restored body should not be null")
	assert_true(restored.has_population_data(), "Restored body should have population data")
	
	assert_equal(
		restored.population_data.profile.habitability_score,
		body.population_data.profile.habitability_score,
		"Habitability score should survive round-trip"
	)
	assert_equal(
		restored.population_data.suitability.overall_score,
		body.population_data.suitability.overall_score,
		"Suitability score should survive round-trip"
	)


## Tests JSON serialization round-trip of population data.
func test_population_json_roundtrip() -> void:
	var spec: PlanetSpec = PlanetSpec.new(77, SizeCategory.Category.SUPER_EARTH, OrbitZone.Zone.TEMPERATE)
	var context: ParentContext = ParentContext.sun_like()
	var rng: SeededRng = SeededRng.new(77)
	
	var body: CelestialBody = PlanetGenerator.generate(spec, context, rng, true)
	assert_true(body.has_population_data(), "Original body should have population data")
	
	# JSON round-trip
	var json_str: String = CelestialSerializer.to_json(body)
	assert_false(json_str.is_empty(), "JSON should not be empty")
	
	var restored: CelestialBody = CelestialSerializer.from_json(json_str)
	assert_not_null(restored, "Restored body from JSON should not be null")
	assert_true(restored.has_population_data(), "Restored body from JSON should have population data")
