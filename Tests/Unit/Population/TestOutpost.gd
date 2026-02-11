## Tests for Outpost data model.
extends TestCase

const _outpost: GDScript = preload("res://src/domain/population/Outpost.gd")
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_service: GDScript = preload("res://src/domain/population/StationService.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _outpost_authority: GDScript = preload("res://src/domain/population/OutpostAuthority.gd")


## Creates a test outpost.
func _create_test_outpost() -> Outpost:
	var outpost: Outpost = Outpost.new()
	outpost.id = "outpost_001"
	outpost.name = "Waypoint Alpha"
	outpost.station_class = StationClass.Class.U
	outpost.station_type = StationType.Type.DEEP_SPACE
	outpost.primary_purpose = StationPurpose.Purpose.UTILITY
	outpost.placement_context = StationPlacementContext.Context.BRIDGE_SYSTEM
	outpost.authority = OutpostAuthority.Type.FRANCHISE
	outpost.parent_organization_id = "corp_001"
	outpost.parent_organization_name = "StarWay Services"
	outpost.population = 150
	outpost.established_year = -50
	outpost.system_id = "system_001"
	outpost.services = [StationService.Service.REFUEL, StationService.Service.REPAIR, StationService.Service.LODGING]
	return outpost


## Tests default creation.
func test_creation_default() -> void:
	var outpost: Outpost = Outpost.new()
	assert_equal(outpost.id, "")
	assert_equal(outpost.station_class, StationClass.Class.O)
	assert_equal(outpost.station_type, StationType.Type.ORBITAL)
	assert_equal(outpost.primary_purpose, StationPurpose.Purpose.UTILITY)
	assert_equal(outpost.population, 0)
	assert_true(outpost.is_operational)
	assert_not_equal(outpost.commander_title, "")


## Tests get_age for operational outpost.
func test_get_age_operational() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.established_year = -50

	var age: int = outpost.get_age(0)
	assert_equal(age, 50)


## Tests get_age for decommissioned outpost.
func test_get_age_decommissioned() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.established_year = -100
	outpost.is_operational = false
	outpost.decommissioned_year = -20

	var age: int = outpost.get_age(0)
	assert_equal(age, 80)


## Tests is_utility.
func test_is_utility() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.station_class = StationClass.Class.U
	assert_true(outpost.is_utility())

	outpost.station_class = StationClass.Class.O
	assert_false(outpost.is_utility())


## Tests is_body_associated.
func test_is_body_associated() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.station_type = StationType.Type.DEEP_SPACE
	assert_false(outpost.is_body_associated())

	outpost.station_type = StationType.Type.ORBITAL
	outpost.orbiting_body_id = ""
	assert_false(outpost.is_body_associated())

	outpost.orbiting_body_id = "planet_001"
	assert_true(outpost.is_body_associated())


## Tests has_parent_organization.
func test_has_parent_organization() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.authority = OutpostAuthority.Type.CORPORATE
	outpost.parent_organization_id = "corp_001"
	assert_true(outpost.has_parent_organization())

	outpost.authority = OutpostAuthority.Type.INDEPENDENT
	assert_false(outpost.has_parent_organization())

	outpost.authority = OutpostAuthority.Type.CORPORATE
	outpost.parent_organization_id = ""
	assert_false(outpost.has_parent_organization())


## Tests service management.
func test_service_management() -> void:
	var outpost: Outpost = Outpost.new()
	outpost.services = []

	assert_false(outpost.offers_service(StationService.Service.REFUEL))

	outpost.add_service(StationService.Service.REFUEL)
	assert_true(outpost.offers_service(StationService.Service.REFUEL))
	assert_equal(outpost.services.size(), 1)

	# Adding again should not duplicate
	outpost.add_service(StationService.Service.REFUEL)
	assert_equal(outpost.services.size(), 1)

	outpost.remove_service(StationService.Service.REFUEL)
	assert_false(outpost.offers_service(StationService.Service.REFUEL))
	assert_equal(outpost.services.size(), 0)


## Tests set_population clamping.
func test_set_population_clamping() -> void:
	var outpost: Outpost = Outpost.new()

	outpost.set_population(5000)
	assert_equal(outpost.population, 5000)

	outpost.set_population(15000) # Over max
	assert_equal(outpost.population, Outpost.MAX_POPULATION)

	outpost.set_population(-100) # Negative
	assert_equal(outpost.population, 0)


## Tests record_decommissioning.
func test_record_decommissioning() -> void:
	var outpost: Outpost = _create_test_outpost()
	assert_true(outpost.is_operational)

	outpost.record_decommissioning(-10, "Resource depletion")

	assert_false(outpost.is_operational)
	assert_equal(outpost.decommissioned_year, -10)
	assert_equal(outpost.decommissioned_reason, "Resource depletion")


## Tests update_commander_title.
func test_update_commander_title() -> void:
	var outpost: Outpost = Outpost.new()

	outpost.authority = OutpostAuthority.Type.MILITARY
	outpost.update_commander_title()
	assert_equal(outpost.commander_title, "Base Commander")

	outpost.authority = OutpostAuthority.Type.CORPORATE
	outpost.update_commander_title()
	assert_equal(outpost.commander_title, "Station Manager")


## Tests get_summary.
func test_get_summary() -> void:
	var outpost: Outpost = _create_test_outpost()
	var summary: Dictionary = outpost.get_summary()

	assert_equal(summary["id"], "outpost_001")
	assert_equal(summary["name"], "Waypoint Alpha")
	assert_equal(summary["class"], "U")
	assert_equal(summary["population"], 150)
	assert_true(summary["is_operational"] as bool)


## Tests validation - valid outpost.
func test_validation_valid() -> void:
	var outpost: Outpost = _create_test_outpost()
	assert_true(outpost.is_valid())
	assert_equal(outpost.validate().size(), 0)


## Tests validation - missing ID.
func test_validation_missing_id() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.id = ""

	var errors: Array[String] = outpost.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("ID is required" in errors[0])


## Tests validation - population over max.
func test_validation_population_over_max() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.population = 20000 # Direct assignment bypasses clamping

	var errors: Array[String] = outpost.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("exceeds" in errors[0])


## Tests validation - wrong class.
func test_validation_wrong_class() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.station_class = StationClass.Class.A # Too large for outpost

	var errors: Array[String] = outpost.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("U or O class" in errors[0])


## Tests validation - orbital without body.
func test_validation_orbital_without_body() -> void:
	var outpost: Outpost = _create_test_outpost()
	outpost.station_type = StationType.Type.ORBITAL
	outpost.orbiting_body_id = ""

	var errors: Array[String] = outpost.validate()
	assert_greater_than(errors.size(), 0)
	assert_true("orbiting_body_id" in errors[0])


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: Outpost = _create_test_outpost()
	original.secondary_purposes = [StationPurpose.Purpose.TRADE]
	original.metadata = {"custom_key": "custom_value"}

	var data: Dictionary = original.to_dict()
	var restored: Outpost = Outpost.from_dict(data)

	assert_equal(restored.id, original.id)
	assert_equal(restored.name, original.name)
	assert_equal(restored.station_class, original.station_class)
	assert_equal(restored.station_type, original.station_type)
	assert_equal(restored.primary_purpose, original.primary_purpose)
	assert_equal(restored.secondary_purposes.size(), original.secondary_purposes.size())
	assert_equal(restored.services.size(), original.services.size())
	assert_equal(restored.placement_context, original.placement_context)
	assert_equal(restored.authority, original.authority)
	assert_equal(restored.parent_organization_id, original.parent_organization_id)
	assert_equal(restored.population, original.population)
	assert_equal(restored.established_year, original.established_year)
	assert_equal(restored.system_id, original.system_id)
	assert_equal(restored.is_operational, original.is_operational)
	assert_equal(restored.metadata["custom_key"], "custom_value")


## Tests decommissioned outpost serialization.
func test_decommissioned_serialization() -> void:
	var original: Outpost = _create_test_outpost()
	original.record_decommissioning(-10, "Abandoned")

	var data: Dictionary = original.to_dict()
	var restored: Outpost = Outpost.from_dict(data)

	assert_false(restored.is_operational)
	assert_equal(restored.decommissioned_year, -10)
	assert_equal(restored.decommissioned_reason, "Abandoned")


## Tests create_utility factory.
func test_create_utility() -> void:
	var outpost: Outpost = Outpost.create_utility("u001", "Pit Stop", "sys001")

	assert_equal(outpost.id, "u001")
	assert_equal(outpost.name, "Pit Stop")
	assert_equal(outpost.system_id, "sys001")
	assert_equal(outpost.station_class, StationClass.Class.U)
	assert_equal(outpost.primary_purpose, StationPurpose.Purpose.UTILITY)
	assert_equal(outpost.placement_context, StationPlacementContext.Context.BRIDGE_SYSTEM)
	assert_greater_than(outpost.services.size(), 0)


## Tests create_mining factory.
func test_create_mining() -> void:
	var outpost: Outpost = Outpost.create_mining("m001", "Rock Crusher", "sys001", "asteroid001")

	assert_equal(outpost.id, "m001")
	assert_equal(outpost.station_class, StationClass.Class.O)
	assert_equal(outpost.station_type, StationType.Type.ORBITAL)
	assert_equal(outpost.orbiting_body_id, "asteroid001")
	assert_equal(outpost.primary_purpose, StationPurpose.Purpose.MINING)
	assert_equal(outpost.authority, OutpostAuthority.Type.CORPORATE)


## Tests create_science factory.
func test_create_science() -> void:
	var outpost: Outpost = Outpost.create_science("s001", "Deep Space Lab", "sys001")

	assert_equal(outpost.id, "s001")
	assert_equal(outpost.station_class, StationClass.Class.O)
	assert_equal(outpost.station_type, StationType.Type.DEEP_SPACE)
	assert_equal(outpost.primary_purpose, StationPurpose.Purpose.SCIENCE)
	assert_equal(outpost.authority, OutpostAuthority.Type.GOVERNMENT)
