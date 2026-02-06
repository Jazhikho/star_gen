## Tests for SpaceStation data model.
extends TestCase

const _space_station: GDScript = preload("res://src/domain/population/SpaceStation.gd")
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_service: GDScript = preload("res://src/domain/population/StationService.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _outpost_authority: GDScript = preload("res://src/domain/population/OutpostAuthority.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")


## Creates a small test station (O-class).
func _create_small_station() -> SpaceStation:
	var station: SpaceStation = SpaceStation.new()
	station.id = "station_001"
	station.name = "Outpost Gamma"
	station.station_class = StationClass.Class.O
	station.station_type = StationType.Type.ORBITAL
	station.primary_purpose = StationPurpose.Purpose.MINING
	station.placement_context = StationPlacementContext.Context.RESOURCE_SYSTEM
	station.outpost_authority = OutpostAuthority.Type.CORPORATE
	station.parent_organization_id = "corp_001"
	station.population = 5000
	station.established_year = -100
	station.system_id = "system_001"
	station.orbiting_body_id = "planet_001"
	station.services = [StationService.Service.REFUEL, StationService.Service.STORAGE]
	return station


## Creates a large test station (A-class).
func _create_large_station() -> SpaceStation:
	var station: SpaceStation = SpaceStation.new()
	station.id = "station_002"
	station.name = "Central Hub"
	station.station_class = StationClass.Class.A
	station.station_type = StationType.Type.ORBITAL
	station.primary_purpose = StationPurpose.Purpose.TRADE
	station.placement_context = StationPlacementContext.Context.COLONY_WORLD
	station.population = 500000
	station.peak_population = 600000
	station.peak_population_year = -20
	station.established_year = -200
	station.system_id = "system_002"
	station.orbiting_body_id = "planet_002"
	station.founding_civilization_id = "civ_001"
	station.founding_civilization_name = "United Colonies"
	station.services = StationService.basic_utility_services()
	station.services.append(StationService.Service.SHIPYARD)
	station.services.append(StationService.Service.BANKING)

	station.government = Government.new()
	station.government.regime = GovernmentType.Regime.CONSTITUTIONAL
	station.government.legitimacy = 0.85

	station.history = PopulationHistory.new()
	station.history.add_new_event(
		HistoryEvent.EventType.FOUNDING,
		-200,
		"Station Founded",
		"Central Hub established as trade station"
	)

	return station


## Tests default creation.
func test_creation_default() -> void:
	var station: SpaceStation = SpaceStation.new()
	assert_equal(station.id, "")
	assert_equal(station.station_class, StationClass.Class.O)
	assert_equal(station.population, 0)
	assert_true(station.is_operational)
	assert_null(station.government)
	assert_null(station.history)


## Tests get_age for operational station.
func test_get_age_operational() -> void:
	var station: SpaceStation = _create_small_station()
	station.established_year = -100

	assert_equal(station.get_age(0), 100)


## Tests get_age for decommissioned station.
func test_get_age_decommissioned() -> void:
	var station: SpaceStation = _create_small_station()
	station.established_year = -100
	station.is_operational = false
	station.decommissioned_year = -30

	assert_equal(station.get_age(0), 70)


## Tests update_class_from_population - staying small.
func test_update_class_small() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.station_class = StationClass.Class.O
	station.population = 5000

	station.update_class_from_population()

	assert_equal(station.station_class, StationClass.Class.O)
	assert_null(station.government)
	assert_null(station.history)


## Tests update_class_from_population - growing to B-class.
func test_update_class_growth_to_b() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.station_class = StationClass.Class.O
	station.population = 50000

	station.update_class_from_population()

	assert_equal(station.station_class, StationClass.Class.B)
	assert_not_null(station.government)
	assert_not_null(station.history)


## Tests update_class_from_population - growing to A-class.
func test_update_class_growth_to_a() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.station_class = StationClass.Class.O
	station.population = 500000

	station.update_class_from_population()

	assert_equal(station.station_class, StationClass.Class.A)
	assert_not_null(station.government)


## Tests update_class_from_population - growing to S-class.
func test_update_class_growth_to_s() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.station_class = StationClass.Class.O
	station.population = 2000000

	station.update_class_from_population()

	assert_equal(station.station_class, StationClass.Class.S)


## Tests update_class_from_population - shrinking preserves data.
func test_update_class_shrink_preserves_data() -> void:
	var station: SpaceStation = _create_large_station()
	assert_not_null(station.government)
	assert_not_null(station.history)

	# Shrink to small station population
	station.population = 5000
	station.update_class_from_population()

	assert_equal(station.station_class, StationClass.Class.O)
	# Government and history should be preserved
	assert_not_null(station.government)
	assert_not_null(station.history)


## Tests uses_outpost_government.
func test_uses_outpost_government() -> void:
	var station: SpaceStation = SpaceStation.new()

	station.station_class = StationClass.Class.U
	assert_true(station.uses_outpost_government())

	station.station_class = StationClass.Class.O
	assert_true(station.uses_outpost_government())

	station.station_class = StationClass.Class.B
	assert_false(station.uses_outpost_government())


## Tests uses_colony_government.
func test_uses_colony_government() -> void:
	var station: SpaceStation = SpaceStation.new()

	station.station_class = StationClass.Class.O
	assert_false(station.uses_colony_government())

	station.station_class = StationClass.Class.B
	assert_true(station.uses_colony_government())

	station.station_class = StationClass.Class.A
	assert_true(station.uses_colony_government())

	station.station_class = StationClass.Class.S
	assert_true(station.uses_colony_government())


## Tests get_regime.
func test_get_regime() -> void:
	var station: SpaceStation = _create_large_station()
	assert_equal(station.get_regime(), GovernmentType.Regime.CONSTITUTIONAL)

	var small: SpaceStation = _create_small_station()
	# Small stations return default regime
	assert_equal(small.get_regime(), GovernmentType.Regime.CONSTITUTIONAL)


## Tests is_politically_stable.
func test_is_politically_stable() -> void:
	var station: SpaceStation = _create_large_station()
	station.government.legitimacy = 0.85
	assert_true(station.is_politically_stable())

	station.government.legitimacy = 0.1
	assert_false(station.is_politically_stable())

	var small: SpaceStation = _create_small_station()
	assert_true(small.is_politically_stable()) # Always stable for small


## Tests service management.
func test_service_management() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.services = []

	assert_false(station.offers_service(StationService.Service.SHIPYARD))

	station.add_service(StationService.Service.SHIPYARD)
	assert_true(station.offers_service(StationService.Service.SHIPYARD))

	station.add_service(StationService.Service.SHIPYARD) # Duplicate
	assert_equal(station.services.size(), 1)

	station.remove_service(StationService.Service.SHIPYARD)
	assert_false(station.offers_service(StationService.Service.SHIPYARD))


## Tests set_population with class update.
func test_set_population() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.station_class = StationClass.Class.O

	station.set_population(5000)
	assert_equal(station.population, 5000)
	assert_equal(station.station_class, StationClass.Class.O)

	station.set_population(200000)
	assert_equal(station.population, 200000)
	assert_equal(station.station_class, StationClass.Class.A)

	station.set_population(-100) # Negative
	assert_equal(station.population, 0)


## Tests update_peak_population.
func test_update_peak_population() -> void:
	var station: SpaceStation = _create_large_station()
	station.population = 700000
	station.peak_population = 600000

	station.update_peak_population(-5)

	assert_equal(station.peak_population, 700000)
	assert_equal(station.peak_population_year, -5)


## Tests get_growth_state.
func test_get_growth_state() -> void:
	var station: SpaceStation = _create_large_station()

	station.population = 600000
	station.peak_population = 600000
	assert_equal(station.get_growth_state(), "growing")

	station.population = 400000
	assert_equal(station.get_growth_state(), "stable")

	station.population = 200000
	assert_equal(station.get_growth_state(), "declining")

	station.is_operational = false
	assert_equal(station.get_growth_state(), "abandoned")


## Tests record_decommissioning.
func test_record_decommissioning() -> void:
	var station: SpaceStation = _create_small_station()
	assert_true(station.is_operational)

	station.record_decommissioning(-10, "Structural failure")

	assert_false(station.is_operational)
	assert_equal(station.decommissioned_year, -10)
	assert_equal(station.decommissioned_reason, "Structural failure")


## Tests record_independence.
func test_record_independence() -> void:
	var station: SpaceStation = _create_large_station()
	assert_false(station.is_independent)

	station.record_independence(-30)

	assert_true(station.is_independent)
	assert_equal(station.independence_year, -30)


## Tests get_summary for small station.
func test_get_summary_small() -> void:
	var station: SpaceStation = _create_small_station()
	var summary: Dictionary = station.get_summary()

	assert_equal(summary["id"], "station_001")
	assert_equal(summary["class"], "O")
	assert_equal(summary["population"], 5000)
	assert_true(summary.has("authority"))
	assert_false(summary.has("regime"))


## Tests get_summary for large station.
func test_get_summary_large() -> void:
	var station: SpaceStation = _create_large_station()
	var summary: Dictionary = station.get_summary()

	assert_equal(summary["id"], "station_002")
	assert_equal(summary["class"], "A")
	assert_true(summary.has("regime"))
	assert_true(summary.has("is_independent"))
	assert_false(summary.has("authority"))


## Tests validation - valid small station.
func test_validation_valid_small() -> void:
	var station: SpaceStation = _create_small_station()
	assert_true(station.is_valid())


## Tests validation - valid large station.
func test_validation_valid_large() -> void:
	var station: SpaceStation = _create_large_station()
	assert_true(station.is_valid())


## Tests validation - missing ID.
func test_validation_missing_id() -> void:
	var station: SpaceStation = _create_small_station()
	station.id = ""

	var errors: Array[String] = station.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("ID is required" in errors[0])


## Tests validation - class mismatch.
func test_validation_class_mismatch() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.id = "test"
	station.station_type = StationType.Type.DEEP_SPACE # Avoid orbital validation
	station.station_class = StationClass.Class.A
	station.population = 5000 # Too small for A-class

	var errors: Array[String] = station.validate()
	assert_greater_than(errors.size(), 0)

	var found_class_error: bool = false
	for error in errors:
		if "does not match" in error:
			found_class_error = true
			break
	assert_true(found_class_error, "Should have class mismatch error")


## Tests validation - large station without government.
func test_validation_large_without_government() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.id = "test"
	station.station_type = StationType.Type.DEEP_SPACE # Avoid orbital validation
	station.station_class = StationClass.Class.B
	station.population = 50000
	station.government = null

	var errors: Array[String] = station.validate()
	assert_greater_than(errors.size(), 0)

	var found_gov_error: bool = false
	for error in errors:
		if "government" in error:
			found_gov_error = true
			break
	assert_true(found_gov_error, "Should have government error")


## Tests serialization round-trip for small station.
func test_serialization_small_station() -> void:
	var original: SpaceStation = _create_small_station()
	original.secondary_purposes = [StationPurpose.Purpose.TRADE]
	original.metadata = {"custom": "value"}

	var data: Dictionary = original.to_dict()
	var restored: SpaceStation = SpaceStation.from_dict(data)

	assert_equal(restored.id, original.id)
	assert_equal(restored.name, original.name)
	assert_equal(restored.station_class, original.station_class)
	assert_equal(restored.station_type, original.station_type)
	assert_equal(restored.primary_purpose, original.primary_purpose)
	assert_equal(restored.secondary_purposes.size(), original.secondary_purposes.size())
	assert_equal(restored.services.size(), original.services.size())
	assert_equal(restored.outpost_authority, original.outpost_authority)
	assert_equal(restored.parent_organization_id, original.parent_organization_id)
	assert_equal(restored.population, original.population)
	assert_equal(restored.established_year, original.established_year)
	assert_equal(restored.metadata["custom"], "value")


## Tests serialization round-trip for large station.
func test_serialization_large_station() -> void:
	var original: SpaceStation = _create_large_station()

	var data: Dictionary = original.to_dict()
	var restored: SpaceStation = SpaceStation.from_dict(data)

	assert_equal(restored.id, original.id)
	assert_equal(restored.station_class, original.station_class)
	assert_equal(restored.population, original.population)
	assert_equal(restored.peak_population, original.peak_population)
	assert_equal(restored.founding_civilization_id, original.founding_civilization_id)

	assert_not_null(restored.government)
	assert_equal(restored.government.regime, original.government.regime)
	assert_float_equal(restored.government.legitimacy, original.government.legitimacy, 0.001)

	assert_not_null(restored.history)
	assert_equal(restored.history.size(), original.history.size())


## Tests serialization of decommissioned station.
func test_serialization_decommissioned() -> void:
	var original: SpaceStation = _create_small_station()
	original.record_decommissioning(-10, "Destroyed")

	var data: Dictionary = original.to_dict()
	var restored: SpaceStation = SpaceStation.from_dict(data)

	assert_false(restored.is_operational)
	assert_equal(restored.decommissioned_year, -10)
	assert_equal(restored.decommissioned_reason, "Destroyed")


## Tests serialization of independent station.
func test_serialization_independent() -> void:
	var original: SpaceStation = _create_large_station()
	original.record_independence(-50)

	var data: Dictionary = original.to_dict()
	var restored: SpaceStation = SpaceStation.from_dict(data)

	assert_true(restored.is_independent)
	assert_equal(restored.independence_year, -50)


## Tests create_orbital factory.
func test_create_orbital() -> void:
	var station: SpaceStation = SpaceStation.create_orbital("orb001", "Orbital One", "sys001", "planet001")

	assert_equal(station.id, "orb001")
	assert_equal(station.name, "Orbital One")
	assert_equal(station.system_id, "sys001")
	assert_equal(station.orbiting_body_id, "planet001")
	assert_equal(station.station_type, StationType.Type.ORBITAL)
	assert_equal(station.placement_context, StationPlacementContext.Context.COLONY_WORLD)
	assert_greater_than(station.services.size(), 0)


## Tests create_deep_space factory.
func test_create_deep_space() -> void:
	var station: SpaceStation = SpaceStation.create_deep_space("ds001", "Deep Station", "sys001")

	assert_equal(station.id, "ds001")
	assert_equal(station.station_type, StationType.Type.DEEP_SPACE)
	assert_equal(station.orbiting_body_id, "")
	assert_equal(station.placement_context, StationPlacementContext.Context.RESOURCE_SYSTEM)


## Tests utility station class selection.
func test_utility_class_selection() -> void:
	var station: SpaceStation = SpaceStation.new()
	station.primary_purpose = StationPurpose.Purpose.UTILITY
	station.population = 5000

	station.update_class_from_population()

	assert_equal(station.station_class, StationClass.Class.U)
