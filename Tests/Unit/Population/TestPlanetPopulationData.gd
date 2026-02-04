## Tests for PlanetPopulationData container.
extends TestCase

const _planet_population_data: GDScript = preload("res://src/domain/population/PlanetPopulationData.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _colony: GDScript = preload("res://src/domain/population/Colony.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _native_relation: GDScript = preload("res://src/domain/population/NativeRelation.gd")


## Creates a test profile.
func _create_test_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "planet_001"
	profile.habitability_score = 8
	return profile


## Creates a test suitability.
func _create_test_suitability() -> ColonySuitability:
	var suitability: ColonySuitability = ColonySuitability.new()
	suitability.overall_score = 75
	return suitability


## Creates a test native population.
func _create_test_native(id: String, pop: int, extant: bool = true) -> NativePopulation:
	var native: NativePopulation = NativePopulation.new()
	native.id = id
	native.name = "Native " + id
	native.population = pop
	native.is_extant = extant
	native.tech_level = TechnologyLevel.Level.MEDIEVAL
	return native


## Creates a test colony.
func _create_test_colony(id: String, pop: int, active: bool = true) -> Colony:
	var colony: Colony = Colony.new()
	colony.id = id
	colony.name = "Colony " + id
	colony.population = pop
	colony.is_active = active
	colony.tech_level = TechnologyLevel.Level.INTERSTELLAR
	return colony


## Tests default creation.
func test_creation_default() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_equal(data.body_id, "")
	assert_null(data.profile)
	assert_null(data.suitability)
	assert_equal(data.native_populations.size(), 0)
	assert_equal(data.colonies.size(), 0)
	assert_equal(data.generated_timestamp, 0)


## Tests get_total_population with no populations.
func test_get_total_population_empty() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_equal(data.get_total_population(), 0)


## Tests get_total_population with natives only.
func test_get_total_population_natives_only() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000))
	data.native_populations.append(_create_test_native("n2", 500000))

	assert_equal(data.get_total_population(), 1500000)


## Tests get_total_population with colonies only.
func test_get_total_population_colonies_only() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.colonies.append(_create_test_colony("c1", 200000))
	data.colonies.append(_create_test_colony("c2", 100000))

	assert_equal(data.get_total_population(), 300000)


## Tests get_total_population with mixed populations.
func test_get_total_population_mixed() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000))
	data.colonies.append(_create_test_colony("c1", 200000))

	assert_equal(data.get_total_population(), 1200000)


## Tests get_total_population excludes extinct/abandoned.
func test_get_total_population_excludes_inactive() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000, true))
	data.native_populations.append(_create_test_native("n2", 500000, false))
	data.colonies.append(_create_test_colony("c1", 200000, true))
	data.colonies.append(_create_test_colony("c2", 100000, false))

	assert_equal(data.get_total_population(), 1200000)


## Tests get_native_population.
func test_get_native_population() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000))
	data.colonies.append(_create_test_colony("c1", 200000))

	assert_equal(data.get_native_population(), 1000000)


## Tests get_colony_population.
func test_get_colony_population() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000))
	data.colonies.append(_create_test_colony("c1", 200000))

	assert_equal(data.get_colony_population(), 200000)


## Tests get_dominant_population returns largest.
func test_get_dominant_population() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000))
	data.colonies.append(_create_test_colony("c1", 2000000))

	var dominant: Variant = data.get_dominant_population()
	assert_not_null(dominant)
	assert_true(dominant is Colony)
	assert_equal((dominant as Colony).id, "c1")


## Tests get_dominant_population with no populations.
func test_get_dominant_population_empty() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_null(data.get_dominant_population())


## Tests get_dominant_population_name.
func test_get_dominant_population_name() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_equal(data.get_dominant_population_name(), "Uninhabited")

	data.native_populations.append(_create_test_native("n1", 1000000))
	assert_equal(data.get_dominant_population_name(), "Native n1")


## Tests is_inhabited.
func test_is_inhabited() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_false(data.is_inhabited())

	data.native_populations.append(_create_test_native("n1", 1000000))
	assert_true(data.is_inhabited())


## Tests has_natives.
func test_has_natives() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_false(data.has_natives())

	data.native_populations.append(_create_test_native("n1", 1000000, false))
	assert_true(data.has_natives())


## Tests has_extant_natives.
func test_has_extant_natives() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_false(data.has_extant_natives())

	data.native_populations.append(_create_test_native("n1", 1000000, false))
	assert_false(data.has_extant_natives())

	data.native_populations.append(_create_test_native("n2", 500000, true))
	assert_true(data.has_extant_natives())


## Tests has_colonies.
func test_has_colonies() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_false(data.has_colonies())

	data.colonies.append(_create_test_colony("c1", 100000, false))
	assert_true(data.has_colonies())


## Tests has_active_colonies.
func test_has_active_colonies() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_false(data.has_active_colonies())

	data.colonies.append(_create_test_colony("c1", 100000, false))
	assert_false(data.has_active_colonies())

	data.colonies.append(_create_test_colony("c2", 200000, true))
	assert_true(data.has_active_colonies())


## Tests get_extant_native_count.
func test_get_extant_native_count() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000, true))
	data.native_populations.append(_create_test_native("n2", 500000, false))
	data.native_populations.append(_create_test_native("n3", 300000, true))

	assert_equal(data.get_extant_native_count(), 2)


## Tests get_active_colony_count.
func test_get_active_colony_count() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.colonies.append(_create_test_colony("c1", 200000, true))
	data.colonies.append(_create_test_colony("c2", 100000, false))
	data.colonies.append(_create_test_colony("c3", 50000, true))

	assert_equal(data.get_active_colony_count(), 2)


## Tests get_extant_natives.
func test_get_extant_natives() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000, true))
	data.native_populations.append(_create_test_native("n2", 500000, false))

	var extant: Array[NativePopulation] = data.get_extant_natives()
	assert_equal(extant.size(), 1)
	assert_equal(extant[0].id, "n1")


## Tests get_active_colonies.
func test_get_active_colonies() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.colonies.append(_create_test_colony("c1", 200000, true))
	data.colonies.append(_create_test_colony("c2", 100000, false))

	var active: Array[Colony] = data.get_active_colonies()
	assert_equal(active.size(), 1)
	assert_equal(active[0].id, "c1")


## Tests has_native_colony_conflict.
func test_has_native_colony_conflict() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()

	var colony: Colony = _create_test_colony("c1", 200000)
	var hostile_relation: NativeRelation = NativeRelation.create_first_contact("n1", -100, -80)
	hostile_relation.status = NativeRelation.Status.HOSTILE
	colony.set_native_relation(hostile_relation)

	data.colonies.append(colony)

	assert_true(data.has_native_colony_conflict())


## Tests get_political_situation.
func test_get_political_situation() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_equal(data.get_political_situation(), "uninhabited")

	data.native_populations.append(_create_test_native("n1", 1000000))
	assert_equal(data.get_political_situation(), "native_only")

	data.native_populations.clear()
	data.colonies.append(_create_test_colony("c1", 200000))
	assert_equal(data.get_political_situation(), "colony_only")

	data.native_populations.append(_create_test_native("n1", 1000000))
	assert_equal(data.get_political_situation(), "coexisting")


## Tests get_political_situation with conflict.
func test_get_political_situation_conflict() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000))

	var colony: Colony = _create_test_colony("c1", 200000)
	var hostile_relation: NativeRelation = NativeRelation.create_first_contact("n1", -100, -80)
	hostile_relation.status = NativeRelation.Status.HOSTILE
	colony.set_native_relation(hostile_relation)
	data.colonies.append(colony)

	assert_equal(data.get_political_situation(), "conflict")


## Tests get_highest_tech_level.
func test_get_highest_tech_level() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	assert_equal(data.get_highest_tech_level(), TechnologyLevel.Level.STONE_AGE)

	var native: NativePopulation = _create_test_native("n1", 1000000)
	native.tech_level = TechnologyLevel.Level.MEDIEVAL
	data.native_populations.append(native)
	assert_equal(data.get_highest_tech_level(), TechnologyLevel.Level.MEDIEVAL)

	var colony: Colony = _create_test_colony("c1", 200000)
	colony.tech_level = TechnologyLevel.Level.INTERSTELLAR
	data.colonies.append(colony)
	assert_equal(data.get_highest_tech_level(), TechnologyLevel.Level.INTERSTELLAR)


## Tests get_native_by_id.
func test_get_native_by_id() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.native_populations.append(_create_test_native("n1", 1000000))
	data.native_populations.append(_create_test_native("n2", 500000))

	var found: NativePopulation = data.get_native_by_id("n2")
	assert_not_null(found)
	assert_equal(found.id, "n2")

	assert_null(data.get_native_by_id("nonexistent"))


## Tests get_colony_by_id.
func test_get_colony_by_id() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.colonies.append(_create_test_colony("c1", 200000))
	data.colonies.append(_create_test_colony("c2", 100000))

	var found: Colony = data.get_colony_by_id("c2")
	assert_not_null(found)
	assert_equal(found.id, "c2")

	assert_null(data.get_colony_by_id("nonexistent"))


## Tests get_summary.
func test_get_summary() -> void:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.body_id = "planet_001"
	data.profile = _create_test_profile()
	data.suitability = _create_test_suitability()
	data.native_populations.append(_create_test_native("n1", 1000000))
	data.colonies.append(_create_test_colony("c1", 200000))

	var summary: Dictionary = data.get_summary()

	assert_equal(summary["body_id"], "planet_001")
	assert_equal(summary["total_population"], 1200000)
	assert_equal(summary["extant_native_count"], 1)
	assert_equal(summary["active_colony_count"], 1)
	assert_equal(summary["political_situation"], "coexisting")
	assert_equal(summary["habitability_score"], 8)
	assert_equal(summary["suitability_score"], 75)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: PlanetPopulationData = PlanetPopulationData.new()
	original.body_id = "planet_001"
	original.generation_seed = 12345
	original.profile = _create_test_profile()
	original.suitability = _create_test_suitability()
	original.native_populations.append(_create_test_native("n1", 1000000))
	original.colonies.append(_create_test_colony("c1", 200000))

	var data: Dictionary = original.to_dict()
	var restored: PlanetPopulationData = PlanetPopulationData.from_dict(data)

	assert_equal(restored.body_id, original.body_id)
	assert_equal(restored.generation_seed, original.generation_seed)
	assert_not_null(restored.profile)
	assert_equal(restored.profile.habitability_score, original.profile.habitability_score)
	assert_not_null(restored.suitability)
	assert_equal(restored.suitability.overall_score, original.suitability.overall_score)
	assert_equal(restored.native_populations.size(), 1)
	assert_equal(restored.colonies.size(), 1)


## Tests serialization with no populations.
func test_serialization_empty() -> void:
	var original: PlanetPopulationData = PlanetPopulationData.new()
	original.body_id = "empty_planet"

	var data: Dictionary = original.to_dict()
	var restored: PlanetPopulationData = PlanetPopulationData.from_dict(data)

	assert_equal(restored.body_id, "empty_planet")
	assert_equal(restored.native_populations.size(), 0)
	assert_equal(restored.colonies.size(), 0)
