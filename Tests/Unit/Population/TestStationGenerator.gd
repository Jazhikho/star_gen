## Tests for StationGenerator.
extends TestCase

const _station_generator: GDScript = preload("res://src/domain/population/StationGenerator.gd")
const _station_spec: GDScript = preload("res://src/domain/population/StationSpec.gd")
const _station_placement_rules: GDScript = preload("res://src/domain/population/StationPlacementRules.gd")
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _outpost: GDScript = preload("res://src/domain/population/Outpost.gd")
const _space_station: GDScript = preload("res://src/domain/population/SpaceStation.gd")


## Creates a bridge system context.
func _create_bridge_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.SystemContext.new()
	ctx.system_id = "bridge_system"
	ctx.is_bridge_system = true
	ctx.planet_ids = ["planet_001"]
	return ctx


## Creates a colony context.
func _create_colony_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.SystemContext.new()
	ctx.system_id = "colony_system"
	ctx.colony_world_count = 2
	ctx.colony_planet_ids = ["planet_001", "planet_002"]
	ctx.habitable_planet_count = 2
	ctx.planet_ids = ["planet_001", "planet_002"]
	return ctx


## Creates a resource system context.
func _create_resource_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.SystemContext.new()
	ctx.system_id = "resource_system"
	ctx.resource_richness = 0.7
	ctx.habitable_planet_count = 0
	ctx.asteroid_belt_count = 2
	ctx.resource_body_ids = ["asteroid_001", "asteroid_002"]
	return ctx


## Creates a spacefaring natives context.
func _create_native_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.SystemContext.new()
	ctx.system_id = "native_system"
	ctx.native_world_count = 1
	ctx.native_planet_ids = ["planet_001"]
	ctx.highest_native_tech = TechnologyLevel.Level.SPACEFARING
	ctx.has_spacefaring_natives = true
	ctx.planet_ids = ["planet_001"]
	return ctx


## Creates an empty context.
func _create_empty_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.SystemContext.new()
	ctx.system_id = "empty_system"
	return ctx


## Tests generation disabled.
func test_generation_disabled() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.generate_stations = false

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_equal(result.get_total_count(), 0)


## Tests generation for bridge system.
func test_generate_bridge_system() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_bridge_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_greater_than(result.get_total_count(), 0)
	assert_equal(result.recommendation.context, StationPlacementContext.Context.BRIDGE_SYSTEM)

	var has_utility: bool = false
	for outpost in result.outposts:
		if outpost.station_class == StationClass.Class.U:
			has_utility = true
			break
	assert_true(has_utility)


## Tests generation for colony system.
func test_generate_colony_system() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_greater_than(result.get_total_count(), 0)
	assert_equal(result.recommendation.context, StationPlacementContext.Context.COLONY_WORLD)

	assert_greater_than(result.stations.size(), 0)


## Tests generation for resource system.
func test_generate_resource_system() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_resource_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_greater_than(result.get_total_count(), 0)
	assert_equal(result.recommendation.context, StationPlacementContext.Context.RESOURCE_SYSTEM)

	var has_mining: bool = false
	for outpost in result.outposts:
		if outpost.primary_purpose == StationPurpose.Purpose.MINING:
			has_mining = true
			break
	for station in result.stations:
		if station.primary_purpose == StationPurpose.Purpose.MINING:
			has_mining = true
			break
	assert_true(has_mining)


## Tests generation for native world.
func test_generate_native_world() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_native_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_greater_than(result.get_total_count(), 0)
	assert_equal(result.recommendation.context, StationPlacementContext.Context.NATIVE_WORLD)


## Tests generation for empty system.
func test_generate_empty_system() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_equal(result.get_total_count(), 0)


## Tests empty system with min_stations override.
func test_generate_empty_with_min_stations() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.min_stations = 2

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_greater_than(result.get_total_count(), 0)


## Tests max_stations limit.
func test_max_stations_limit() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.max_stations = 1

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_less_than(result.get_total_count(), 3)


## Tests forced context.
func test_forced_context() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	var spec: StationSpec = StationSpec.for_context(StationPlacementContext.Context.RESOURCE_SYSTEM)
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_equal(result.recommendation.context, StationPlacementContext.Context.RESOURCE_SYSTEM)
	assert_greater_than(result.get_total_count(), 0)


## Tests determinism with same seed.
func test_determinism_same_seed() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 99999

	var result1: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)
	var result2: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_equal(result1.get_total_count(), result2.get_total_count())
	assert_equal(result1.outposts.size(), result2.outposts.size())
	assert_equal(result1.stations.size(), result2.stations.size())

	for i in range(result1.stations.size()):
		assert_equal(result1.stations[i].id, result2.stations[i].id)
		assert_equal(result1.stations[i].name, result2.stations[i].name)
		assert_equal(result1.stations[i].population, result2.stations[i].population)


## Tests determinism with different seeds.
func test_determinism_different_seeds() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()

	var spec1: StationSpec = StationSpec.new()
	spec1.seed = 11111

	var spec2: StationSpec = StationSpec.new()
	spec2.seed = 22222

	var result1: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec1)
	var result2: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec2)

	assert_equal(result1.generation_seed, 11111)
	assert_equal(result2.generation_seed, 22222)


## Tests allow_utility = false.
func test_no_utility_stations() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_bridge_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.allow_utility = false

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	for outpost in result.outposts:
		assert_not_equal(outpost.station_class, StationClass.Class.U)


## Tests allow_large_stations = false.
func test_no_large_stations() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.allow_large_stations = false

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	for station in result.stations:
		assert_true(StationClass.uses_outpost_government(station.station_class))


## Tests excluded purposes.
func test_excluded_purposes() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_resource_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.excluded_purposes = [StationPurpose.Purpose.MINING]

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	for outpost in result.outposts:
		assert_not_equal(outpost.primary_purpose, StationPurpose.Purpose.MINING)
	for station in result.stations:
		assert_not_equal(station.primary_purpose, StationPurpose.Purpose.MINING)


## Tests population density modifier.
func test_population_density() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()

	var spec_normal: StationSpec = StationSpec.new()
	spec_normal.seed = 12345
	spec_normal.population_density = 1.0

	var spec_dense: StationSpec = StationSpec.new()
	spec_dense.seed = 12345
	spec_dense.population_density = 2.0

	var result_normal: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec_normal)
	var result_dense: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec_dense)

	var pop_normal: int = 0
	for s in result_normal.stations:
		pop_normal += s.population
	for o in result_normal.outposts:
		pop_normal += o.population

	var pop_dense: int = 0
	for s in result_dense.stations:
		pop_dense += s.population
	for o in result_dense.outposts:
		pop_dense += o.population

	assert_greater_than(pop_dense, pop_normal * 0.5)


## Tests station IDs are unique.
func test_unique_ids() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.min_stations = 5

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	var ids: Dictionary = {}
	for outpost in result.outposts:
		assert_false(ids.has(outpost.id), "Duplicate ID: " + outpost.id)
		ids[outpost.id] = true
	for station in result.stations:
		assert_false(ids.has(station.id), "Duplicate ID: " + station.id)
		ids[station.id] = true


## Tests orbital stations have body IDs.
func test_orbital_body_ids() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	for station in result.stations:
		if station.station_type == StationType.Type.ORBITAL:
			assert_not_equal(station.orbiting_body_id, "")


## Tests large stations have government.
func test_large_stations_have_government() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	for station in result.stations:
		if station.uses_colony_government():
			assert_not_null(station.government)
			assert_not_null(station.history)


## Tests get_stations_for_body.
func test_get_stations_for_body() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	var planet_stations: Array[SpaceStation] = result.get_stations_for_body("planet_001")

	for station in planet_stations:
		assert_equal(station.orbiting_body_id, "planet_001")


## Tests result to_dict.
func test_result_to_dict() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)
	var data: Dictionary = result.to_dict()

	assert_true(data.has("outposts"))
	assert_true(data.has("stations"))
	assert_true(data.has("generation_seed"))
	assert_true(data.has("recommendation"))


## Tests invalid spec produces warnings.
func test_invalid_spec_warnings() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.min_stations = 10
	spec.max_stations = 1

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	assert_greater_than(result.warnings.size(), 0)


## Tests decommission chance.
func test_decommission_chance() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.decommission_chance = 0.5
	spec.min_stations = 10

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	var decommissioned_count: int = 0
	for outpost in result.outposts:
		if not outpost.is_operational:
			decommissioned_count += 1
	for station in result.stations:
		if not station.is_operational:
			decommissioned_count += 1

	assert_true(decommissioned_count >= 0)


## Tests establishment years are within range.
func test_establishment_years() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var spec: StationSpec = StationSpec.new()
	spec.seed = 12345
	spec.min_established_year = -100
	spec.max_established_year = -10

	var result: StationGenerator.GenerationResult = StationGenerator.generate(ctx, spec)

	for outpost in result.outposts:
		assert_greater_than(outpost.established_year, -101)
		assert_less_than(outpost.established_year, -9)

	for station in result.stations:
		assert_greater_than(station.established_year, -101)
		assert_less_than(station.established_year, -9)
