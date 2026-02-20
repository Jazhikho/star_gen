## Tests for StationSpec.
extends TestCase

const _station_spec: GDScript = preload("res://src/domain/population/StationSpec.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")


## Tests default creation.
func test_creation_default() -> void:
	var spec: StationSpec = StationSpec.new()

	assert_true(spec.generate_stations)
	assert_true(spec.allow_utility)
	assert_true(spec.allow_outposts)
	assert_true(spec.allow_large_stations)
	assert_true(spec.allow_deep_space)
	assert_true(spec.allow_belt_stations)
	assert_equal(spec.min_stations, 0)
	assert_equal(spec.max_stations, 0)
	assert_float_equal(spec.population_density, 1.0, 0.001)
	assert_null(spec.force_context)


## Tests minimal factory.
func test_minimal_factory() -> void:
	var spec: StationSpec = StationSpec.minimal()

	assert_true(spec.generate_stations)
	assert_false(spec.allow_large_stations)
	assert_equal(spec.max_stations, 2)
	assert_less_than(spec.population_density, 1.0)


## Tests standard factory.
func test_standard_factory() -> void:
	var spec: StationSpec = StationSpec.standard()

	assert_true(spec.generate_stations)
	assert_true(spec.allow_utility)
	assert_true(spec.allow_outposts)
	assert_true(spec.allow_large_stations)


## Tests dense factory.
func test_dense_factory() -> void:
	var spec: StationSpec = StationSpec.dense()

	assert_greater_than(spec.population_density, 1.0)
	assert_greater_than(spec.min_stations, 0)


## Tests for_context factory.
func test_for_context_factory() -> void:
	var spec: StationSpec = StationSpec.for_context(StationPlacementContext.Context.BRIDGE_SYSTEM)

	assert_equal(spec.force_context, StationPlacementContext.Context.BRIDGE_SYSTEM)


## Tests is_purpose_allowed with no restrictions.
func test_is_purpose_allowed_unrestricted() -> void:
	var spec: StationSpec = StationSpec.new()

	assert_true(spec.is_purpose_allowed(StationPurpose.Purpose.UTILITY))
	assert_true(spec.is_purpose_allowed(StationPurpose.Purpose.MINING))
	assert_true(spec.is_purpose_allowed(StationPurpose.Purpose.MILITARY))


## Tests is_purpose_allowed with required purposes.
func test_is_purpose_allowed_required() -> void:
	var spec: StationSpec = StationSpec.new()
	spec.required_purposes = [StationPurpose.Purpose.MINING, StationPurpose.Purpose.SCIENCE]

	assert_true(spec.is_purpose_allowed(StationPurpose.Purpose.MINING))
	assert_true(spec.is_purpose_allowed(StationPurpose.Purpose.SCIENCE))
	assert_false(spec.is_purpose_allowed(StationPurpose.Purpose.UTILITY))


## Tests is_purpose_allowed with excluded purposes.
func test_is_purpose_allowed_excluded() -> void:
	var spec: StationSpec = StationSpec.new()
	spec.excluded_purposes = [StationPurpose.Purpose.MILITARY]

	assert_true(spec.is_purpose_allowed(StationPurpose.Purpose.MINING))
	assert_false(spec.is_purpose_allowed(StationPurpose.Purpose.MILITARY))


## Tests is_class_allowed.
func test_is_class_allowed() -> void:
	var spec: StationSpec = StationSpec.new()

	assert_true(spec.is_class_allowed(StationClass.Class.U))
	assert_true(spec.is_class_allowed(StationClass.Class.O))
	assert_true(spec.is_class_allowed(StationClass.Class.B))

	spec.allow_utility = false
	assert_false(spec.is_class_allowed(StationClass.Class.U))

	spec.allow_large_stations = false
	assert_false(spec.is_class_allowed(StationClass.Class.B))
	assert_false(spec.is_class_allowed(StationClass.Class.A))


## Tests validation - valid spec.
func test_validation_valid() -> void:
	var spec: StationSpec = StationSpec.new()
	assert_true(spec.is_valid())
	assert_equal(spec.validate().size(), 0)


## Tests validation - min > max stations.
func test_validation_min_max_stations() -> void:
	var spec: StationSpec = StationSpec.new()
	spec.min_stations = 10
	spec.max_stations = 5

	var errors: Array[String] = spec.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("min_stations" in errors[0])


## Tests validation - negative population density.
func test_validation_negative_density() -> void:
	var spec: StationSpec = StationSpec.new()
	spec.population_density = -1.0

	var errors: Array[String] = spec.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("population_density" in errors[0])


## Tests validation - invalid decommission chance.
func test_validation_invalid_decommission() -> void:
	var spec: StationSpec = StationSpec.new()
	spec.decommission_chance = 1.5

	var errors: Array[String] = spec.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("decommission_chance" in errors[0])


## Tests validation - invalid year range.
func test_validation_invalid_years() -> void:
	var spec: StationSpec = StationSpec.new()
	spec.min_established_year = 100
	spec.max_established_year = -100

	var errors: Array[String] = spec.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("year" in errors[0].to_lower())


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: StationSpec = StationSpec.new()
	original.generation_seed = 12345
	original.generate_stations = true
	original.force_context = StationPlacementContext.Context.COLONY_WORLD
	original.min_stations = 2
	original.max_stations = 10
	original.allow_utility = false
	original.population_density = 1.5
	original.decommission_chance = 0.1
	original.required_purposes = [StationPurpose.Purpose.TRADE]
	original.excluded_purposes = [StationPurpose.Purpose.MILITARY]
	original.id_prefix = "test_station"
	original.founding_civilization_id = "civ_001"

	var data: Dictionary = original.to_dict()
	var restored: StationSpec = StationSpec.from_dict(data)

	assert_equal(restored.generation_seed, original.generation_seed)
	assert_equal(restored.generate_stations, original.generate_stations)
	# force_context restored as int (from serialization); compare as int values
	assert_equal(restored.force_context as int, original.force_context as int)
	assert_equal(restored.min_stations, original.min_stations)
	assert_equal(restored.max_stations, original.max_stations)
	assert_equal(restored.allow_utility, original.allow_utility)
	assert_float_equal(restored.population_density, original.population_density, 0.001)
	assert_float_equal(restored.decommission_chance, original.decommission_chance, 0.001)
	assert_equal(restored.required_purposes.size(), original.required_purposes.size())
	assert_equal(restored.excluded_purposes.size(), original.excluded_purposes.size())
	assert_equal(restored.id_prefix, original.id_prefix)
	assert_equal(restored.founding_civilization_id, original.founding_civilization_id)


## Tests serialization without force_context.
func test_serialization_no_force_context() -> void:
	var original: StationSpec = StationSpec.new()
	original.force_context = null

	var data: Dictionary = original.to_dict()
	var restored: StationSpec = StationSpec.from_dict(data)

	assert_null(restored.force_context)
