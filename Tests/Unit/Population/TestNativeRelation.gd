## Tests for NativeRelation data model.
extends TestCase

const _native_relation: GDScript = preload("res://src/domain/population/NativeRelation.gd")


## Tests default creation.
func test_creation_default() -> void:
	var relation: NativeRelation = NativeRelation.new()
	assert_equal(relation.native_population_id, "")
	assert_equal(relation.status, NativeRelation.Status.UNKNOWN)
	assert_equal(relation.relation_score, 0)
	assert_false(relation.has_treaty)


## Tests create_first_contact.
func test_create_first_contact() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 20)

	assert_equal(relation.native_population_id, "native_001")
	assert_equal(relation.status, NativeRelation.Status.FIRST_CONTACT)
	assert_equal(relation.first_contact_year, -100)
	assert_equal(relation.relation_score, 20)
	assert_greater_than(relation.relationship_events.size(), 0)


## Tests create_first_contact clamps disposition.
func test_create_first_contact_clamps() -> void:
	var relation1: NativeRelation = NativeRelation.create_first_contact("native_001", 0, 200)
	assert_equal(relation1.relation_score, 100)

	var relation2: NativeRelation = NativeRelation.create_first_contact("native_002", 0, -200)
	assert_equal(relation2.relation_score, -100)


## Tests update_status to peaceful.
func test_update_status_peaceful() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 30)
	relation.update_status()

	assert_equal(relation.status, NativeRelation.Status.PEACEFUL)


## Tests update_status to trading.
func test_update_status_trading() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 50)
	relation.trade_level = 0.5
	relation.update_status()

	assert_equal(relation.status, NativeRelation.Status.TRADING)


## Tests update_status to hostile.
func test_update_status_hostile() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, -30)
	relation.conflict_intensity = 0.7
	relation.update_status()

	assert_equal(relation.status, NativeRelation.Status.HOSTILE)


## Tests update_status to subjugated.
func test_update_status_subjugated() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 0)
	relation.territory_taken = 0.9
	relation.update_status()

	assert_equal(relation.status, NativeRelation.Status.SUBJUGATED)


## Tests update_status to integrated.
func test_update_status_integrated() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 70)
	relation.cultural_exchange = 0.8
	relation.update_status()

	assert_equal(relation.status, NativeRelation.Status.INTEGRATED)


## Tests record_extinction.
func test_record_extinction() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 0)
	relation.record_extinction(-50, "plague")

	assert_equal(relation.status, NativeRelation.Status.EXTINCT)
	assert_greater_than(relation.relationship_events.size(), 1)


## Tests record_treaty.
func test_record_treaty() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 20)
	relation.record_treaty(-50, "Peace treaty")

	assert_true(relation.has_treaty)
	assert_equal(relation.treaty_year, -50)
	assert_equal(relation.relation_score, 40) # +20 from treaty


## Tests record_conflict.
func test_record_conflict() -> void:
	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 20)
	relation.has_treaty = true
	relation.record_conflict(-50, "Border war", 0.6)

	assert_false(relation.has_treaty) # Treaty broken
	assert_float_equal(relation.conflict_intensity, 0.6, 0.01)
	assert_less_than(relation.relation_score, 20) # Decreased


## Tests is_positive.
func test_is_positive() -> void:
	var relation: NativeRelation = NativeRelation.new()

	relation.relation_score = 10
	assert_true(relation.is_positive())

	relation.relation_score = 0
	assert_false(relation.is_positive())

	relation.relation_score = -10
	assert_false(relation.is_positive())


## Tests is_hostile.
func test_is_hostile() -> void:
	var relation: NativeRelation = NativeRelation.new()

	relation.status = NativeRelation.Status.HOSTILE
	assert_true(relation.is_hostile())

	relation.status = NativeRelation.Status.TENSE
	relation.relation_score = -50
	assert_true(relation.is_hostile())

	relation.status = NativeRelation.Status.TENSE
	relation.relation_score = -10
	assert_false(relation.is_hostile())


## Tests has_active_trade.
func test_has_active_trade() -> void:
	var relation: NativeRelation = NativeRelation.new()

	relation.status = NativeRelation.Status.TRADING
	relation.trade_level = 0.5
	assert_true(relation.has_active_trade())

	relation.status = NativeRelation.Status.PEACEFUL
	assert_false(relation.has_active_trade())


## Tests status_to_string.
func test_status_to_string() -> void:
	assert_equal(NativeRelation.status_to_string(NativeRelation.Status.UNKNOWN), "Unknown")
	assert_equal(NativeRelation.status_to_string(NativeRelation.Status.FIRST_CONTACT), "First Contact")
	assert_equal(NativeRelation.status_to_string(NativeRelation.Status.HOSTILE), "Hostile")


## Tests status_from_string.
func test_status_from_string() -> void:
	assert_equal(NativeRelation.status_from_string("unknown"), NativeRelation.Status.UNKNOWN)
	assert_equal(NativeRelation.status_from_string("First Contact"), NativeRelation.Status.FIRST_CONTACT)
	assert_equal(NativeRelation.status_from_string("hostile"), NativeRelation.Status.HOSTILE)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 30)
	original.status = NativeRelation.Status.TRADING
	original.trade_level = 0.5
	original.cultural_exchange = 0.3
	original.territory_taken = 0.1
	original.has_treaty = true
	original.treaty_year = -50

	var data: Dictionary = original.to_dict()
	var restored: NativeRelation = NativeRelation.from_dict(data)

	assert_equal(restored.native_population_id, original.native_population_id)
	assert_equal(restored.status, original.status)
	assert_equal(restored.first_contact_year, original.first_contact_year)
	assert_equal(restored.relation_score, original.relation_score)
	assert_equal(restored.has_treaty, original.has_treaty)
	assert_float_equal(restored.trade_level, original.trade_level, 0.001)
	assert_float_equal(restored.cultural_exchange, original.cultural_exchange, 0.001)


## Tests relationship_events serialization.
func test_relationship_events_serialization() -> void:
	var original: NativeRelation = NativeRelation.create_first_contact("native_001", -100, 0)
	original.record_treaty(-50, "Peace")

	var data: Dictionary = original.to_dict()
	var restored: NativeRelation = NativeRelation.from_dict(data)

	assert_equal(restored.relationship_events.size(), original.relationship_events.size())
