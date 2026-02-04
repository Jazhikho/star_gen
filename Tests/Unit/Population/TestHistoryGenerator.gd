## Tests for HistoryGenerator profile-driven event generation.
extends TestCase

const _history_generator: GDScript = preload("res://src/domain/population/HistoryGenerator.gd")
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Creates a stable Earth-like profile.
func _create_earth_like_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "earth_like"
	profile.habitability_score = 10
	profile.volcanism_level = 0.2
	profile.tectonic_activity = 0.5
	profile.weather_severity = 0.3
	profile.radiation_level = 0.1
	profile.has_liquid_water = true
	profile.continent_count = 7
	profile.resources = {
		ResourceType.Type.WATER as int: 0.9,
		ResourceType.Type.METALS as int: 0.5,
		ResourceType.Type.SILICATES as int: 0.8,
		ResourceType.Type.ORGANICS as int: 0.7,
		ResourceType.Type.RARE_ELEMENTS as int: 0.3,
	}
	return profile


## Creates a harsh volcanic profile.
func _create_volcanic_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "volcanic"
	profile.habitability_score = 3
	profile.volcanism_level = 0.9
	profile.tectonic_activity = 0.8
	profile.weather_severity = 0.6
	profile.radiation_level = 0.4
	profile.has_liquid_water = false
	profile.continent_count = 1
	profile.resources = {
		ResourceType.Type.SILICATES as int: 0.7,
		ResourceType.Type.METALS as int: 0.6,
	}
	return profile


## Tests event weights calculation for Earth-like world.
func test_calculate_event_weights_earth_like() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var weights: Dictionary = HistoryGenerator.calculate_event_weights(profile)

	assert_greater_than(weights.size(), 10)

	var golden_age_weight: float = weights.get(HistoryEvent.EventType.GOLDEN_AGE as int, 0.0) as float
	assert_greater_than(golden_age_weight, 0.5, "Earth-like should boost golden age events")


## Tests event weights calculation for volcanic world.
func test_calculate_event_weights_volcanic() -> void:
	var profile: PlanetProfile = _create_volcanic_profile()
	var weights: Dictionary = HistoryGenerator.calculate_event_weights(profile)

	var disaster_weight: float = weights.get(HistoryEvent.EventType.NATURAL_DISASTER as int, 0.0) as float

	var earth_profile: PlanetProfile = _create_earth_like_profile()
	var earth_weights: Dictionary = HistoryGenerator.calculate_event_weights(earth_profile)
	var earth_disaster: float = earth_weights.get(HistoryEvent.EventType.NATURAL_DISASTER as int, 0.0) as float

	assert_greater_than(disaster_weight, earth_disaster, "Volcanic world should have higher disaster weight")


## Tests event weights for low habitability.
func test_calculate_event_weights_low_habitability() -> void:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.habitability_score = 2
	profile.volcanism_level = 0.1
	profile.tectonic_activity = 0.1
	profile.has_liquid_water = false

	var weights: Dictionary = HistoryGenerator.calculate_event_weights(profile)

	var expansion_weight: float = weights.get(HistoryEvent.EventType.EXPANSION as int, 0.0) as float
	var golden_age_weight: float = weights.get(HistoryEvent.EventType.GOLDEN_AGE as int, 0.0) as float

	assert_less_than(expansion_weight, 1.0, "Low habitability should reduce expansion weight")
	assert_less_than(golden_age_weight, 0.5, "Low habitability should reduce golden age weight")


## Tests history generation produces events.
func test_generate_history_produces_events() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var history: PopulationHistory = HistoryGenerator.generate_history(
		profile, -1000, 0, rng, "Test Founding"
	)

	assert_greater_than(history.size(), 0, "Should generate at least one event")


## Tests history generation includes founding event.
func test_generate_history_has_founding() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var history: PopulationHistory = HistoryGenerator.generate_history(
		profile, -1000, 0, rng, "The Beginning"
	)

	var founding: HistoryEvent = history.get_founding_event()
	assert_not_null(founding)
	assert_equal(founding.year, -1000)
	assert_equal(founding.title, "The Beginning")


## Tests history generation respects year range.
func test_generate_history_year_range() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var history: PopulationHistory = HistoryGenerator.generate_history(
		profile, -500, 100, rng
	)

	for event in history.get_all_events():
		assert_in_range(event.year, -500, 100, "Event year should be in range")


## Tests history generation with invalid range.
func test_generate_history_invalid_range() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var history: PopulationHistory = HistoryGenerator.generate_history(
		profile, 100, -100, rng
	)

	assert_true(history.is_empty(), "Invalid range should produce empty history")


## Tests determinism - same seed produces same history.
func test_determinism() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()

	var rng1: SeededRng = SeededRng.new(42)
	var history1: PopulationHistory = HistoryGenerator.generate_history(
		profile, -1000, 0, rng1
	)

	var rng2: SeededRng = SeededRng.new(42)
	var history2: PopulationHistory = HistoryGenerator.generate_history(
		profile, -1000, 0, rng2
	)

	assert_equal(history1.size(), history2.size(), "Same seed should produce same event count")

	var events1: Array[HistoryEvent] = history1.get_all_events()
	var events2: Array[HistoryEvent] = history2.get_all_events()

	for i in range(events1.size()):
		assert_equal(events1[i].year, events2[i].year, "Same seed should produce same years")
		assert_equal(events1[i].event_type, events2[i].event_type, "Same seed should produce same types")


## Tests different seeds produce different histories.
func test_different_seeds() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()

	var rng1: SeededRng = SeededRng.new(1)
	var history1: PopulationHistory = HistoryGenerator.generate_history(
		profile, -1000, 0, rng1
	)

	var rng2: SeededRng = SeededRng.new(999)
	var history2: PopulationHistory = HistoryGenerator.generate_history(
		profile, -1000, 0, rng2
	)

	if history1.size() > 3 and history2.size() > 3:
		var events1: Array[HistoryEvent] = history1.get_all_events()
		var events2: Array[HistoryEvent] = history2.get_all_events()

		var has_difference: bool = false
		for i in range(mini(events1.size(), events2.size())):
			if events1[i].event_type != events2[i].event_type:
				has_difference = true
				break

		assert_true(history1.size() != history2.size() or has_difference,
			"Different seeds should generally produce different histories")


## Tests generated events have valid data.
func test_generated_event_validity() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var history: PopulationHistory = HistoryGenerator.generate_history(
		profile, -1000, 0, rng
	)

	for event in history.get_all_events():
		assert_not_equal(event.title, "", "Event should have a title")
		assert_in_range(event.magnitude, -1.0, 1.0, "Magnitude should be in range")


## Tests profile affects event distribution.
func test_profile_affects_distribution() -> void:
	var volcanic: PlanetProfile = _create_volcanic_profile()
	var earth: PlanetProfile = _create_earth_like_profile()

	var volcanic_disasters: int = 0
	var earth_disasters: int = 0
	var iterations: int = 10

	for i in range(iterations):
		var rng_v: SeededRng = SeededRng.new(i * 100)
		var history_v: PopulationHistory = HistoryGenerator.generate_history(
			volcanic, -1000, 0, rng_v
		)
		volcanic_disasters += history_v.get_events_by_type(HistoryEvent.EventType.NATURAL_DISASTER).size()

		var rng_e: SeededRng = SeededRng.new(i * 100)
		var history_e: PopulationHistory = HistoryGenerator.generate_history(
			earth, -1000, 0, rng_e
		)
		earth_disasters += history_e.get_events_by_type(HistoryEvent.EventType.NATURAL_DISASTER).size()

	assert_true(volcanic_disasters >= earth_disasters * 0.5,
		"Volcanic world should not have drastically fewer disasters than Earth-like")


## Tests short time span produces fewer events.
func test_short_time_span() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var short_history: PopulationHistory = HistoryGenerator.generate_history(
		profile, -50, 0, rng
	)

	assert_less_than(short_history.size(), 10, "Short time span should produce few events")


## Tests long time span produces more events.
func test_long_time_span() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var long_history: PopulationHistory = HistoryGenerator.generate_history(
		profile, -5000, 0, rng
	)

	assert_greater_than(long_history.size(), 20, "Long time span should produce many events")


## Tests events have appropriate magnitudes for their types.
func test_event_magnitude_appropriateness() -> void:
	var profile: PlanetProfile = _create_earth_like_profile()
	var rng: SeededRng = SeededRng.new(12345)

	var history: PopulationHistory = HistoryGenerator.generate_history(
		profile, -2000, 0, rng
	)

	for event in history.get_all_events():
		if HistoryEvent.is_typically_harmful(event.event_type):
			assert_less_than(event.magnitude, 0.5,
				"Harmful event type should not have strongly positive magnitude")
		elif HistoryEvent.is_typically_beneficial(event.event_type):
			assert_greater_than(event.magnitude, -0.5,
				"Beneficial event type should not have strongly negative magnitude")


## Tests resource-poor worlds have more famine.
func test_resource_poor_famine() -> void:
	var poor: PlanetProfile = PlanetProfile.new()
	poor.habitability_score = 4
	poor.has_liquid_water = false
	poor.resources = {ResourceType.Type.SILICATES as int: 0.5}

	var weights: Dictionary = HistoryGenerator.calculate_event_weights(poor)
	var famine_weight: float = weights.get(HistoryEvent.EventType.FAMINE as int, 0.0) as float

	assert_greater_than(famine_weight, 1.0, "Resource-poor world should have elevated famine weight")
