## Regenerates a celestial body with locked properties held fixed.
## Picks the right spec + generator for the body type, applies
## locked-value overrides via EditSpecBuilder, and runs generation.
##
## Deterministic: same seed + same locked set -> identical output.
## Pure domain; callers supply the RNG seed.
class_name EditRegenerator
extends RefCounted

const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _asteroid_generator: GDScript = preload("res://src/domain/generation/generators/AsteroidGenerator.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _spec_builder: GDScript = preload("res://src/domain/editing/EditSpecBuilder.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Result of a regeneration attempt.
class RegenerateResult:
	var success: bool = false
	var body: CelestialBody = null
	var error_message: String = ""

	static func ok(p_body: CelestialBody) -> RegenerateResult:
		var r: RegenerateResult = RegenerateResult.new()
		r.success = true
		r.body = p_body
		return r

	static func fail(msg: String) -> RegenerateResult:
		var r: RegenerateResult = RegenerateResult.new()
		r.success = false
		r.error_message = msg
		return r


## Regenerates a body of the given type with locked properties fixed.
## Unlocked properties are re-rolled from the RNG.
## @param body_type: CelestialType.Type of the body to generate.
## @param constraints: ConstraintSet whose locked values become overrides.
## @param seed_value: RNG seed for the unlocked properties.
## @param context: ParentContext for planets/moons/asteroids. Ignored for stars. Pass null for type default.
## @return: RegenerateResult with the new body or an error.
static func regenerate(
	body_type: int,
	constraints: ConstraintSet,
	seed_value: int,
	context: ParentContext = null
) -> RegenerateResult:
	var rng: SeededRng = SeededRng.new(seed_value)
	match body_type:
		CelestialType.Type.STAR:
			return _regenerate_star(constraints, seed_value, rng)
		CelestialType.Type.PLANET:
			return _regenerate_planet(constraints, seed_value, rng, context)
		CelestialType.Type.MOON:
			return _regenerate_moon(constraints, seed_value, rng, context)
		CelestialType.Type.ASTEROID:
			return _regenerate_asteroid(constraints, seed_value, rng, context)
		_:
			return RegenerateResult.fail("Unsupported body type: %d" % body_type)


static func _regenerate_star(
	constraints: ConstraintSet,
	seed_value: int,
	rng: SeededRng
) -> RegenerateResult:
	var spec: StarSpec = StarSpec.new(seed_value)
	_spec_builder.apply_to_spec(spec, CelestialType.Type.STAR, constraints)
	var body: CelestialBody = StarGenerator.generate(spec, rng)
	if body == null:
		return RegenerateResult.fail("StarGenerator returned null")
	return RegenerateResult.ok(body)


static func _regenerate_planet(
	constraints: ConstraintSet,
	seed_value: int,
	rng: SeededRng,
	context: ParentContext
) -> RegenerateResult:
	var spec: PlanetSpec = PlanetSpec.new(seed_value)
	_spec_builder.apply_to_spec(spec, CelestialType.Type.PLANET, constraints)
	var ctx: ParentContext = context
	if ctx == null:
		ctx = ParentContext.sun_like()
	var body: CelestialBody = PlanetGenerator.generate(spec, ctx, rng, false)
	if body == null:
		return RegenerateResult.fail("PlanetGenerator returned null")
	return RegenerateResult.ok(body)


static func _regenerate_moon(
	constraints: ConstraintSet,
	seed_value: int,
	rng: SeededRng,
	context: ParentContext
) -> RegenerateResult:
	var spec: MoonSpec = MoonSpec.new(seed_value)
	_spec_builder.apply_to_spec(spec, CelestialType.Type.MOON, constraints)
	var ctx: ParentContext = context
	if ctx == null:
		ctx = ParentContext.for_moon(
			Units.SOLAR_MASS_KG,
			3.828e26,
			5778.0,
			4.6e9,
			5.2 * Units.AU_METERS,
			1.898e27,
			6.9911e7,
			5.0e8
		)
	var body: CelestialBody = MoonGenerator.generate(spec, ctx, rng, false)
	if body == null:
		return RegenerateResult.fail("MoonGenerator returned null")
	return RegenerateResult.ok(body)


static func _regenerate_asteroid(
	constraints: ConstraintSet,
	seed_value: int,
	rng: SeededRng,
	context: ParentContext
) -> RegenerateResult:
	var spec: AsteroidSpec = AsteroidSpec.new(seed_value)
	_spec_builder.apply_to_spec(spec, CelestialType.Type.ASTEROID, constraints)
	var ctx: ParentContext = context
	if ctx == null:
		ctx = ParentContext.sun_like(2.7 * Units.AU_METERS)
	var body: CelestialBody = AsteroidGenerator.generate(spec, ctx, rng)
	if body == null:
		return RegenerateResult.fail("AsteroidGenerator returned null")
	return RegenerateResult.ok(body)
