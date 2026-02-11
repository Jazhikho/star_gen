## Unit tests for StationService enum.
extends TestCase


func test_to_string_name_returns_correct_values() -> void:
	assert_equal(StationService.to_string_name(StationService.Service.REFUEL), "Refuel")
	assert_equal(StationService.to_string_name(StationService.Service.REPAIR), "Repair")
	assert_equal(StationService.to_string_name(StationService.Service.TRADE), "Trade")
	assert_equal(StationService.to_string_name(StationService.Service.MEDICAL), "Medical")
	assert_equal(StationService.to_string_name(StationService.Service.CUSTOMS), "Customs")
	assert_equal(StationService.to_string_name(StationService.Service.ENTERTAINMENT), "Entertainment")
	assert_equal(StationService.to_string_name(StationService.Service.LODGING), "Lodging")
	assert_equal(StationService.to_string_name(StationService.Service.SHIPYARD), "Shipyard")
	assert_equal(StationService.to_string_name(StationService.Service.BANKING), "Banking")
	assert_equal(StationService.to_string_name(StationService.Service.COMMUNICATIONS), "Communications")
	assert_equal(StationService.to_string_name(StationService.Service.STORAGE), "Storage")
	assert_equal(StationService.to_string_name(StationService.Service.SECURITY), "Security")


func test_from_string_parses_correctly() -> void:
	assert_equal(StationService.from_string("Refuel"), StationService.Service.REFUEL)
	assert_equal(StationService.from_string("Repair"), StationService.Service.REPAIR)
	assert_equal(StationService.from_string("Trade"), StationService.Service.TRADE)
	assert_equal(StationService.from_string("Shipyard"), StationService.Service.SHIPYARD)


func test_from_string_is_case_insensitive() -> void:
	assert_equal(StationService.from_string("REFUEL"), StationService.Service.REFUEL)
	assert_equal(StationService.from_string("repair"), StationService.Service.REPAIR)


func test_from_string_returns_default_for_unknown() -> void:
	assert_equal(StationService.from_string("unknown"), StationService.Service.REFUEL)
	assert_equal(StationService.from_string(""), StationService.Service.REFUEL)


func test_basic_utility_services_returns_expected() -> void:
	var services: Array[StationService.Service] = StationService.basic_utility_services()
	assert_true(services.size() >= 3)
	assert_true(StationService.Service.REFUEL in services)
	assert_true(StationService.Service.REPAIR in services)
	assert_true(StationService.Service.TRADE in services)


func test_advanced_services_returns_expected() -> void:
	var services: Array[StationService.Service] = StationService.advanced_services()
	assert_true(services.size() > 0)
	assert_true(StationService.Service.SHIPYARD in services)


func test_common_services_returns_expected() -> void:
	var services: Array[StationService.Service] = StationService.common_services()
	assert_true(services.size() > 0)
	assert_true(StationService.Service.REFUEL in services)


func test_requires_major_infrastructure() -> void:
	assert_true(StationService.requires_major_infrastructure(StationService.Service.SHIPYARD))
	assert_true(StationService.requires_major_infrastructure(StationService.Service.BANKING))
	assert_true(StationService.requires_major_infrastructure(StationService.Service.ENTERTAINMENT))
	assert_false(StationService.requires_major_infrastructure(StationService.Service.REFUEL))
	assert_false(StationService.requires_major_infrastructure(StationService.Service.REPAIR))


func test_count_returns_correct_value() -> void:
	assert_equal(StationService.count(), 12)


func test_roundtrip_string_conversion() -> void:
	for i in range(StationService.count()):
		var service: StationService.Service = i as StationService.Service
		var name_str: String = StationService.to_string_name(service)
		var parsed: StationService.Service = StationService.from_string(name_str)
		assert_equal(parsed, service, "Roundtrip failed for service %d" % i)
