## Unit tests for SolarSystem population aggregation methods.
## Verifies get_total_population, get_native_population, get_colony_population,
## and is_inhabited across a range of body configurations.
extends TestCase


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

## Creates a minimal SolarSystem with a unique id.
func _make_system(sys_id: String = "test_sys") -> SolarSystem:
	return SolarSystem.new(sys_id, "Test System")


## Creates a CelestialBody (PLANET type) with no population data attached.
func _make_bare_planet(body_id: String) -> CelestialBody:
	return CelestialBody.new(
		body_id,
		"Planet " + body_id,
		CelestialType.Type.PLANET,
		PhysicalProps.new()
	)


## Creates a CelestialBody with PlanetPopulationData containing one extant
## native population and one active colony, each with the given counts.
## Pass 0 to omit that kind of population entirely.
func _make_populated_planet(
	body_id: String,
	native_pop: int,
	colony_pop: int
) -> CelestialBody:
	var body: CelestialBody = _make_bare_planet(body_id)
	var pop_data: PlanetPopulationData = PlanetPopulationData.new()
	pop_data.body_id = body_id

	if native_pop > 0:
		var native: NativePopulation = NativePopulation.new()
		native.id = "native_" + body_id
		native.population = native_pop
		native.is_extant = true
		pop_data.native_populations.append(native)

	if colony_pop > 0:
		var colony: Colony = Colony.new()
		colony.id = "colony_" + body_id
		colony.population = colony_pop
		colony.is_active = true
		pop_data.colonies.append(colony)

	body.population_data = pop_data
	return body


## Creates a CelestialBody (MOON type) with PlanetPopulationData attached.
## Used to verify that SolarSystem aggregation counts moons alongside planets.
func _make_populated_moon(
	body_id: String,
	native_pop: int,
	colony_pop: int
) -> CelestialBody:
	var body: CelestialBody = CelestialBody.new(
		body_id,
		"Moon " + body_id,
		CelestialType.Type.MOON,
		PhysicalProps.new()
	)
	var pop_data: PlanetPopulationData = PlanetPopulationData.new()
	pop_data.body_id = body_id
	if native_pop > 0:
		var native: NativePopulation = NativePopulation.new()
		native.id = "native_" + body_id
		native.population = native_pop
		native.is_extant = true
		pop_data.native_populations.append(native)
	if colony_pop > 0:
		var colony: Colony = Colony.new()
		colony.id = "colony_" + body_id
		colony.population = colony_pop
		colony.is_active = true
		pop_data.colonies.append(colony)
	body.population_data = pop_data
	return body


# ---------------------------------------------------------------------------
# Zero / empty cases
# ---------------------------------------------------------------------------

func test_empty_system_returns_zero_totals() -> void:
	var system: SolarSystem = _make_system()

	assert_equal(system.get_total_population(), 0)
	assert_equal(system.get_native_population(), 0)
	assert_equal(system.get_colony_population(), 0)


func test_empty_system_is_not_inhabited() -> void:
	var system: SolarSystem = _make_system()
	assert_false(system.is_inhabited())


func test_body_without_population_data_contributes_zero() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_bare_planet("p1"))
	system.add_body(_make_bare_planet("p2"))

	assert_equal(system.get_total_population(), 0)
	assert_false(system.is_inhabited())


# ---------------------------------------------------------------------------
# Aggregation correctness
# ---------------------------------------------------------------------------

func test_total_population_sums_across_all_bodies() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 1000, 500))
	system.add_body(_make_populated_planet("p2", 2000, 0))
	system.add_body(_make_populated_planet("p3", 0, 3000))

	assert_equal(system.get_total_population(), 6500)


func test_native_population_excludes_colony_counts() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 1000, 500))
	system.add_body(_make_populated_planet("p2", 2000, 300))

	assert_equal(system.get_native_population(), 3000)


func test_colony_population_excludes_native_counts() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 1000, 500))
	system.add_body(_make_populated_planet("p2", 2000, 300))

	assert_equal(system.get_colony_population(), 800)


func test_native_plus_colony_equals_total() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 1500, 750))
	system.add_body(_make_populated_planet("p2", 3000, 250))

	assert_equal(
		system.get_native_population() + system.get_colony_population(),
		system.get_total_population()
	)


func test_is_inhabited_true_when_any_colony_exists() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 0, 100))
	assert_true(system.is_inhabited())


func test_is_inhabited_true_when_any_native_exists() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 50, 0))
	assert_true(system.is_inhabited())


# ---------------------------------------------------------------------------
# Extinct / abandoned are excluded
# ---------------------------------------------------------------------------

func test_extinct_natives_do_not_count() -> void:
	var system: SolarSystem = _make_system()
	var body: CelestialBody = _make_bare_planet("p1")
	var pop_data: PlanetPopulationData = PlanetPopulationData.new()
	var native: NativePopulation = NativePopulation.new()
	native.id = "extinct"
	native.population = 0 # set to 0 on extinction per NativePopulation.record_extinction
	native.is_extant = false
	pop_data.native_populations.append(native)
	body.population_data = pop_data
	system.add_body(body)

	assert_equal(system.get_total_population(), 0)
	assert_false(system.is_inhabited())


func test_abandoned_colonies_do_not_count() -> void:
	var system: SolarSystem = _make_system()
	var body: CelestialBody = _make_bare_planet("p1")
	var pop_data: PlanetPopulationData = PlanetPopulationData.new()
	var colony: Colony = Colony.new()
	colony.id = "abandoned"
	colony.population = 0 # set to 0 on abandonment per Colony.record_abandonment
	colony.is_active = false
	pop_data.colonies.append(colony)
	body.population_data = pop_data
	system.add_body(body)

	assert_equal(system.get_total_population(), 0)
	assert_false(system.is_inhabited())


# ---------------------------------------------------------------------------
# Mixed bodies (some with data, some without)
# ---------------------------------------------------------------------------

func test_mixed_bodies_only_count_those_with_data() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_bare_planet("bare_1"))
	system.add_body(_make_populated_planet("pop_1", 5000, 1000))
	system.add_body(_make_bare_planet("bare_2"))
	system.add_body(_make_populated_planet("pop_2", 0, 2000))

	assert_equal(system.get_total_population(), 8000)
	assert_equal(system.get_native_population(), 5000)
	assert_equal(system.get_colony_population(), 3000)


# ---------------------------------------------------------------------------
# Moon bodies are included in system aggregation
# ---------------------------------------------------------------------------

func test_moon_population_included_in_total() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 10000, 0))
	system.add_body(_make_populated_moon("m1", 500, 200))

	assert_equal(system.get_total_population(), 10700)
	assert_equal(system.get_native_population(), 10500)
	assert_equal(system.get_colony_population(), 200)


func test_native_plus_colony_equals_total_with_moons() -> void:
	var system: SolarSystem = _make_system()
	system.add_body(_make_populated_planet("p1", 3000, 1000))
	system.add_body(_make_populated_moon("m1", 800, 0))
	system.add_body(_make_populated_moon("m2", 0, 400))

	assert_equal(
		system.get_native_population() + system.get_colony_population(),
		system.get_total_population()
	)
	assert_equal(system.get_total_population(), 5200)
