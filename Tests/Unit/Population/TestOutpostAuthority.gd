## Unit tests for OutpostAuthority enum.
extends TestCase


func test_to_string_name_returns_correct_values() -> void:
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.CORPORATE), "Corporate")
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.MILITARY), "Military")
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.INDEPENDENT), "Independent")
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.FRANCHISE), "Franchise")
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.COOPERATIVE), "Cooperative")
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.AUTOMATED), "Automated")
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.GOVERNMENT), "Government")
	assert_equal(OutpostAuthority.to_string_name(OutpostAuthority.Type.RELIGIOUS), "Religious")


func test_from_string_parses_correctly() -> void:
	assert_equal(OutpostAuthority.from_string("Corporate"), OutpostAuthority.Type.CORPORATE)
	assert_equal(OutpostAuthority.from_string("Military"), OutpostAuthority.Type.MILITARY)
	assert_equal(OutpostAuthority.from_string("Independent"), OutpostAuthority.Type.INDEPENDENT)
	assert_equal(OutpostAuthority.from_string("Franchise"), OutpostAuthority.Type.FRANCHISE)
	assert_equal(OutpostAuthority.from_string("Cooperative"), OutpostAuthority.Type.COOPERATIVE)
	assert_equal(OutpostAuthority.from_string("Automated"), OutpostAuthority.Type.AUTOMATED)
	assert_equal(OutpostAuthority.from_string("Government"), OutpostAuthority.Type.GOVERNMENT)
	assert_equal(OutpostAuthority.from_string("Religious"), OutpostAuthority.Type.RELIGIOUS)


func test_from_string_is_case_insensitive() -> void:
	assert_equal(OutpostAuthority.from_string("CORPORATE"), OutpostAuthority.Type.CORPORATE)
	assert_equal(OutpostAuthority.from_string("military"), OutpostAuthority.Type.MILITARY)


func test_from_string_returns_default_for_unknown() -> void:
	assert_equal(OutpostAuthority.from_string("unknown"), OutpostAuthority.Type.INDEPENDENT)
	assert_equal(OutpostAuthority.from_string(""), OutpostAuthority.Type.INDEPENDENT)


func test_typical_commander_title_returns_non_empty() -> void:
	for i in range(OutpostAuthority.count()):
		var authority: OutpostAuthority.Type = i as OutpostAuthority.Type
		var title: String = OutpostAuthority.typical_commander_title(authority)
		assert_true(title.length() > 0, "Authority %d should have commander title" % i)


func test_typical_commander_title_varies_by_type() -> void:
	var corporate_title: String = OutpostAuthority.typical_commander_title(OutpostAuthority.Type.CORPORATE)
	var military_title: String = OutpostAuthority.typical_commander_title(OutpostAuthority.Type.MILITARY)
	assert_not_equal(corporate_title, military_title)


func test_has_parent_organization() -> void:
	assert_true(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.CORPORATE))
	assert_true(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.MILITARY))
	assert_true(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.FRANCHISE))
	assert_true(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.GOVERNMENT))
	assert_true(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.RELIGIOUS))
	assert_false(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.INDEPENDENT))
	assert_false(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.COOPERATIVE))
	assert_false(OutpostAuthority.has_parent_organization(OutpostAuthority.Type.AUTOMATED))


func test_typical_for_utility_returns_non_empty() -> void:
	var types: Array[OutpostAuthority.Type] = OutpostAuthority.typical_for_utility()
	assert_true(types.size() > 0)
	assert_true(OutpostAuthority.Type.CORPORATE in types)
	assert_true(OutpostAuthority.Type.FRANCHISE in types)


func test_typical_for_outpost_returns_non_empty() -> void:
	var types: Array[OutpostAuthority.Type] = OutpostAuthority.typical_for_outpost()
	assert_true(types.size() > 0)
	assert_true(OutpostAuthority.Type.MILITARY in types)


func test_count_returns_correct_value() -> void:
	assert_equal(OutpostAuthority.count(), 8)


func test_roundtrip_string_conversion() -> void:
	for i in range(OutpostAuthority.count()):
		var authority: OutpostAuthority.Type = i as OutpostAuthority.Type
		var name_str: String = OutpostAuthority.to_string_name(authority)
		var parsed: OutpostAuthority.Type = OutpostAuthority.from_string(name_str)
		assert_equal(parsed, authority, "Roundtrip failed for authority %d" % i)
