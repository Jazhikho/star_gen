## Tests for StationPlacementRules.
extends TestCase

const _rules: GDScript = preload("res://src/domain/population/StationPlacementRules.gd")
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Creates a basic empty system context.
func _create_empty_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.SystemContext.new()
	ctx.system_id = "test_system"
	return ctx


## Creates a bridge system context.
func _create_bridge_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	ctx.is_bridge_system = true
	return ctx


## Creates a context with spacefaring natives.
func _create_native_spacefaring_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	ctx.native_world_count = 1
	ctx.native_planet_ids = ["planet_001"]
	ctx.highest_native_tech = TechnologyLevel.Level.SPACEFARING
	ctx.has_spacefaring_natives = true
	ctx.planet_ids = ["planet_001"]
	return ctx


## Creates a context with non-spacefaring natives.
func _create_native_primitive_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	ctx.native_world_count = 1
	ctx.native_planet_ids = ["planet_001"]
	ctx.highest_native_tech = TechnologyLevel.Level.INDUSTRIAL
	ctx.has_spacefaring_natives = false
	ctx.planet_ids = ["planet_001"]
	return ctx


## Creates a context with a colony.
func _create_colony_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	ctx.colony_world_count = 1
	ctx.colony_planet_ids = ["planet_001"]
	ctx.habitable_planet_count = 1
	ctx.planet_ids = ["planet_001"]
	return ctx


## Creates a resource-rich system context.
func _create_resource_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	ctx.resource_richness = 0.7
	ctx.habitable_planet_count = 0
	ctx.asteroid_belt_count = 2
	ctx.resource_body_ids = ["asteroid_001", "asteroid_002", "moon_001"]
	return ctx


## Tests empty system evaluation.
func test_evaluate_empty_system() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_false(rec.should_have_stations)
	assert_equal(rec.context, StationPlacementContext.Context.OTHER)


## Tests bridge system evaluation.
func test_evaluate_bridge_system() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_bridge_context()
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_true(rec.should_have_stations)
	assert_equal(rec.context, StationPlacementContext.Context.BRIDGE_SYSTEM)
	assert_greater_than(rec.utility_station_count, 0)
	assert_true(rec.allow_deep_space)
	assert_true(StationPurpose.Purpose.UTILITY in rec.recommended_purposes)


## Tests native spacefaring world evaluation.
func test_evaluate_native_spacefaring() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_native_spacefaring_context()
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_true(rec.should_have_stations)
	assert_equal(rec.context, StationPlacementContext.Context.NATIVE_WORLD)
	assert_greater_than(rec.large_station_count, 0)
	assert_true("planet_001" in rec.orbital_candidates)
	assert_true(StationPurpose.Purpose.TRADE in rec.recommended_purposes)


## Tests native primitive world evaluation (scientific interest).
func test_evaluate_native_primitive() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_native_primitive_context()
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_true(rec.should_have_stations)
	assert_equal(rec.context, StationPlacementContext.Context.SCIENTIFIC)
	assert_greater_than(rec.outpost_count, 0)
	assert_true(StationPurpose.Purpose.SCIENCE in rec.recommended_purposes)


## Tests colony world evaluation.
func test_evaluate_colony_world() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_true(rec.should_have_stations)
	assert_equal(rec.context, StationPlacementContext.Context.COLONY_WORLD)
	assert_greater_than(rec.large_station_count, 0)
	assert_true("planet_001" in rec.orbital_candidates)


## Tests resource-rich system evaluation.
func test_evaluate_resource_system() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_resource_context()
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_true(rec.should_have_stations)
	assert_equal(rec.context, StationPlacementContext.Context.RESOURCE_SYSTEM)
	assert_greater_than(rec.outpost_count, 0)
	assert_true(rec.allow_belt_stations)
	assert_true(StationPurpose.Purpose.MINING in rec.recommended_purposes)


## Tests high-resource system gets large station.
func test_high_resource_gets_large_station() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_resource_context()
	ctx.resource_richness = 0.8 # High threshold
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_greater_than(rec.large_station_count, 0)
	assert_true(StationPurpose.Purpose.RESIDENTIAL in rec.recommended_purposes)


## Tests colony + bridge system gets both utilities and orbital.
func test_bridge_with_colony() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_bridge_context()
	ctx.colony_world_count = 1
	ctx.colony_planet_ids = ["planet_001"]
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_equal(rec.context, StationPlacementContext.Context.BRIDGE_SYSTEM)
	assert_greater_than(rec.utility_station_count, 0)
	assert_greater_than(rec.large_station_count, 0)


## Tests should_have_orbital_stations with colony.
func test_should_have_orbital_colony() -> void:
	var result: bool = StationPlacementRules.should_have_orbital_stations(false, null, true)
	assert_true(result)


## Tests should_have_orbital_stations with spacefaring natives.
func test_should_have_orbital_spacefaring_natives() -> void:
	var result: bool = StationPlacementRules.should_have_orbital_stations(
		true,
		TechnologyLevel.Level.SPACEFARING,
		false
	)
	assert_true(result)


## Tests should_have_orbital_stations with primitive natives.
func test_should_have_orbital_primitive_natives() -> void:
	var result: bool = StationPlacementRules.should_have_orbital_stations(
		true,
		TechnologyLevel.Level.INDUSTRIAL,
		false
	)
	assert_false(result)


## Tests should_have_orbital_stations with no population.
func test_should_have_orbital_none() -> void:
	var result: bool = StationPlacementRules.should_have_orbital_stations(false, null, false)
	assert_false(result)


## Tests estimate_orbital_station_count.
func test_estimate_orbital_station_count() -> void:
	# Non-spacefaring gets 0
	var count: int = StationPlacementRules.estimate_orbital_station_count(
		10_000_000,
		TechnologyLevel.Level.INDUSTRIAL
	)
	assert_equal(count, 0)

	# Small spacefaring population gets 1
	count = StationPlacementRules.estimate_orbital_station_count(
		1_000_000,
		TechnologyLevel.Level.SPACEFARING
	)
	assert_equal(count, 1)

	# Large population gets more
	count = StationPlacementRules.estimate_orbital_station_count(
		50_000_000,
		TechnologyLevel.Level.SPACEFARING
	)
	assert_equal(count, 5)

	# Interstellar tech gets bonus
	count = StationPlacementRules.estimate_orbital_station_count(
		50_000_000,
		TechnologyLevel.Level.INTERSTELLAR
	)
	assert_greater_than(count, 5)


## Tests recommend_station_class for bridge system.
func test_recommend_class_bridge() -> void:
	var cls: StationClass.Class = StationPlacementRules.recommend_station_class(
		StationPlacementContext.Context.BRIDGE_SYSTEM,
		false
	)
	assert_equal(cls, StationClass.Class.U)


## Tests recommend_station_class for scientific outpost.
func test_recommend_class_scientific() -> void:
	var cls: StationClass.Class = StationPlacementRules.recommend_station_class(
		StationPlacementContext.Context.SCIENTIFIC,
		false
	)
	assert_equal(cls, StationClass.Class.O)


## Tests recommend_station_class for colony with large pop.
func test_recommend_class_colony_large() -> void:
	var cls: StationClass.Class = StationPlacementRules.recommend_station_class(
		StationPlacementContext.Context.COLONY_WORLD,
		true
	)
	assert_equal(cls, StationClass.Class.A)


## Tests recommend_station_class for resource system without pop.
func test_recommend_class_resource_small() -> void:
	var cls: StationClass.Class = StationPlacementRules.recommend_station_class(
		StationPlacementContext.Context.RESOURCE_SYSTEM,
		false
	)
	assert_equal(cls, StationClass.Class.O)


## Tests recommend_purposes for utility station.
func test_recommend_purposes_utility() -> void:
	var purposes: Array[StationPurpose.Purpose] = StationPlacementRules.recommend_purposes(
		StationPlacementContext.Context.BRIDGE_SYSTEM,
		true
	)
	assert_true(StationPurpose.Purpose.UTILITY in purposes)


## Tests recommend_purposes for colony.
func test_recommend_purposes_colony() -> void:
	var purposes: Array[StationPurpose.Purpose] = StationPlacementRules.recommend_purposes(
		StationPlacementContext.Context.COLONY_WORLD,
		false
	)
	assert_true(StationPurpose.Purpose.TRADE in purposes)
	assert_true(StationPurpose.Purpose.RESIDENTIAL in purposes)


## Tests recommend_purposes for mining.
func test_recommend_purposes_resource() -> void:
	var purposes: Array[StationPurpose.Purpose] = StationPlacementRules.recommend_purposes(
		StationPlacementContext.Context.RESOURCE_SYSTEM,
		false
	)
	assert_true(StationPurpose.Purpose.MINING in purposes)


## Tests calculate_resource_richness with empty resources.
func test_resource_richness_empty() -> void:
	var richness: float = StationPlacementRules.calculate_resource_richness({})
	assert_float_equal(richness, 0.0, 0.001)


## Tests calculate_resource_richness with common resources.
func test_resource_richness_common() -> void:
	var resources: Dictionary = {
		ResourceType.Type.SILICATES: 0.5,
		ResourceType.Type.METALS: 0.3,
	}
	var richness: float = StationPlacementRules.calculate_resource_richness(resources)
	assert_greater_than(richness, 0.0)
	assert_less_than(richness, 1.0)


## Tests calculate_resource_richness with rare resources.
func test_resource_richness_rare() -> void:
	var common: Dictionary = {
		ResourceType.Type.SILICATES: 0.5,
	}
	var rare: Dictionary = {
		ResourceType.Type.RARE_ELEMENTS: 0.5,
	}
	var common_richness: float = StationPlacementRules.calculate_resource_richness(common)
	var rare_richness: float = StationPlacementRules.calculate_resource_richness(rare)

	# Rare resources should score higher
	assert_greater_than(rare_richness, common_richness)


## Tests create_system_context helper.
func test_create_system_context() -> void:
	var native_data: Array[Dictionary] = [
		{"body_id": "planet_001", "tech_level": TechnologyLevel.Level.SPACEFARING}
	]
	var colony_ids: Array[String] = ["planet_002"]
	var planet_ids: Array[String] = ["planet_001", "planet_002"]
	var resource_ids: Array[String] = ["asteroid_001"]

	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.create_system_context(
		"sys_001",
		planet_ids,
		2,
		native_data,
		colony_ids,
		0.5,
		1,
		resource_ids,
		true
	)

	assert_equal(ctx.system_id, "sys_001")
	assert_true(ctx.is_bridge_system)
	assert_equal(ctx.habitable_planet_count, 2)
	assert_equal(ctx.native_world_count, 1)
	assert_equal(ctx.colony_world_count, 1)
	assert_true(ctx.has_spacefaring_natives)
	assert_equal(ctx.highest_native_tech, TechnologyLevel.Level.SPACEFARING)
	assert_float_equal(ctx.resource_richness, 0.5, 0.001)
	assert_equal(ctx.asteroid_belt_count, 1)


## Tests context priority: bridge takes precedence.
func test_context_priority_bridge() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	ctx.is_bridge_system = true
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_equal(rec.context, StationPlacementContext.Context.BRIDGE_SYSTEM)


## Tests context priority: spacefaring natives over colony.
func test_context_priority_spacefaring_natives() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_native_spacefaring_context()
	ctx.colony_world_count = 1
	ctx.colony_planet_ids = ["planet_002"]
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_equal(rec.context, StationPlacementContext.Context.NATIVE_WORLD)


## Tests context priority: colony over resources.
func test_context_priority_colony_over_resources() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	ctx.resource_richness = 0.8
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_equal(rec.context, StationPlacementContext.Context.COLONY_WORLD)


## Tests strategic context for habitable but uncolonized.
func test_strategic_context() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	ctx.habitable_planet_count = 1
	ctx.planet_ids = ["planet_001"]
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_equal(rec.context, StationPlacementContext.Context.STRATEGIC)
	assert_true(StationPurpose.Purpose.MILITARY in rec.recommended_purposes)


## Tests recommendation to_dict.
func test_recommendation_to_dict() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_colony_context()
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	var data: Dictionary = rec.to_dict()
	assert_true(data.has("context"))
	assert_true(data.has("should_have_stations"))
	assert_true(data.has("utility_station_count"))
	assert_true(data.has("outpost_count"))
	assert_true(data.has("large_station_count"))
	assert_true(data.has("recommended_purposes"))
	assert_true(data.has("reasoning"))


## Tests minor resources trigger small outpost.
func test_minor_resources_outpost() -> void:
	var ctx: StationPlacementRules.SystemContext = _create_empty_context()
	ctx.resource_richness = 0.25 # Above 0.2 threshold
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.evaluate_system(ctx)

	assert_true(rec.should_have_stations)
	assert_greater_than(rec.outpost_count, 0)
