## Generates historical events for populations based on planet profile.
## Event weights are profile-driven so timelines feel consistent with the planet.
## All functions are static and deterministic when given a SeededRng.
class_name HistoryGenerator
extends RefCounted

# Preload dependencies.
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Base weights for event types (before profile modification).
const BASE_WEIGHTS: Dictionary = {
	HistoryEvent.EventType.NATURAL_DISASTER: 1.0,
	HistoryEvent.EventType.PLAGUE: 0.8,
	HistoryEvent.EventType.FAMINE: 0.7,
	HistoryEvent.EventType.WAR: 1.0,
	HistoryEvent.EventType.CIVIL_WAR: 0.6,
	HistoryEvent.EventType.TECH_ADVANCEMENT: 1.2,
	HistoryEvent.EventType.EXPANSION: 1.0,
	HistoryEvent.EventType.POLITICAL_CHANGE: 0.9,
	HistoryEvent.EventType.MIGRATION: 0.7,
	HistoryEvent.EventType.COLLAPSE: 0.2,
	HistoryEvent.EventType.GOLDEN_AGE: 0.5,
	HistoryEvent.EventType.CULTURAL_SHIFT: 0.6,
	HistoryEvent.EventType.TREATY: 0.4,
	HistoryEvent.EventType.DISCOVERY: 0.8,
	HistoryEvent.EventType.CONSTRUCTION: 0.7,
	HistoryEvent.EventType.LEADER: 0.9,
}

## Minimum years between major events of the same type.
const MIN_EVENT_SPACING: int = 10

## Average years between events (modified by profile).
const BASE_EVENT_INTERVAL: int = 25


## Calculates event weights modified by planet profile.
## @param profile: The planet profile to use.
## @return: Dictionary of EventType (as int) -> weight (float).
static func calculate_event_weights(profile: PlanetProfile) -> Dictionary:
	var weights: Dictionary = {}

	# Start with base weights
	for type_key in BASE_WEIGHTS.keys():
		weights[type_key as int] = BASE_WEIGHTS[type_key] as float

	# Modify based on profile

	# High volcanism/tectonics = more natural disasters
	var disaster_mod: float = 1.0 + profile.volcanism_level + profile.tectonic_activity
	weights[HistoryEvent.EventType.NATURAL_DISASTER as int] = (weights[HistoryEvent.EventType.NATURAL_DISASTER as int] as float) * disaster_mod

	# Severe weather = more disasters and famines
	if profile.weather_severity > 0.5:
		weights[HistoryEvent.EventType.NATURAL_DISASTER as int] = (weights[HistoryEvent.EventType.NATURAL_DISASTER as int] as float) * (1.0 + profile.weather_severity)
		weights[HistoryEvent.EventType.FAMINE as int] = (weights[HistoryEvent.EventType.FAMINE as int] as float) * (1.0 + profile.weather_severity * 0.5)

	# Low habitability = more hardship events
	if profile.habitability_score < 5:
		var hardship_mod: float = 1.5 - profile.habitability_score * 0.1
		weights[HistoryEvent.EventType.FAMINE as int] = (weights[HistoryEvent.EventType.FAMINE as int] as float) * hardship_mod
		weights[HistoryEvent.EventType.PLAGUE as int] = (weights[HistoryEvent.EventType.PLAGUE as int] as float) * hardship_mod
		weights[HistoryEvent.EventType.COLLAPSE as int] = (weights[HistoryEvent.EventType.COLLAPSE as int] as float) * hardship_mod
		# But fewer expansion events
		weights[HistoryEvent.EventType.EXPANSION as int] = (weights[HistoryEvent.EventType.EXPANSION as int] as float) * 0.5
		weights[HistoryEvent.EventType.GOLDEN_AGE as int] = (weights[HistoryEvent.EventType.GOLDEN_AGE as int] as float) * 0.3

	# High habitability = more positive events
	if profile.habitability_score >= 7:
		var prosperity_mod: float = 1.0 + (profile.habitability_score - 7) * 0.15
		weights[HistoryEvent.EventType.GOLDEN_AGE as int] = (weights[HistoryEvent.EventType.GOLDEN_AGE as int] as float) * prosperity_mod
		weights[HistoryEvent.EventType.EXPANSION as int] = (weights[HistoryEvent.EventType.EXPANSION as int] as float) * prosperity_mod
		weights[HistoryEvent.EventType.TECH_ADVANCEMENT as int] = (weights[HistoryEvent.EventType.TECH_ADVANCEMENT as int] as float) * prosperity_mod

	# Water availability affects agriculture and thus famine
	if not profile.has_liquid_water:
		weights[HistoryEvent.EventType.FAMINE as int] = (weights[HistoryEvent.EventType.FAMINE as int] as float) * 1.5

	# Radiation affects health
	if profile.radiation_level > 0.5:
		weights[HistoryEvent.EventType.PLAGUE as int] = (weights[HistoryEvent.EventType.PLAGUE as int] as float) * (1.0 + profile.radiation_level)

	# Resource richness affects conflict and prosperity
	var resource_count: int = profile.resources.size()
	if resource_count >= 5:
		weights[HistoryEvent.EventType.WAR as int] = (weights[HistoryEvent.EventType.WAR as int] as float) * 1.2
		weights[HistoryEvent.EventType.TECH_ADVANCEMENT as int] = (weights[HistoryEvent.EventType.TECH_ADVANCEMENT as int] as float) * 1.2
	elif resource_count <= 2:
		weights[HistoryEvent.EventType.FAMINE as int] = (weights[HistoryEvent.EventType.FAMINE as int] as float) * 1.3
		weights[HistoryEvent.EventType.MIGRATION as int] = (weights[HistoryEvent.EventType.MIGRATION as int] as float) * 1.5

	# Multiple continents = more diverse political events
	if profile.continent_count >= 3:
		weights[HistoryEvent.EventType.WAR as int] = (weights[HistoryEvent.EventType.WAR as int] as float) * 1.2
		weights[HistoryEvent.EventType.TREATY as int] = (weights[HistoryEvent.EventType.TREATY as int] as float) * 1.3
		weights[HistoryEvent.EventType.CULTURAL_SHIFT as int] = (weights[HistoryEvent.EventType.CULTURAL_SHIFT as int] as float) * 1.2

	return weights


## Generates a history for a population over a time span.
## @param profile: The planet profile (affects event weights).
## @param start_year: The founding year.
## @param end_year: The end of history generation.
## @param rng: Seeded random number generator.
## @param founding_title: Title for the founding event.
## @return: A populated PopulationHistory.
static func generate_history(
	profile: PlanetProfile,
	start_year: int,
	end_year: int,
	rng: SeededRng,
	founding_title: String = "Founding"
) -> PopulationHistory:
	var history: PopulationHistory = PopulationHistory.new()

	if start_year >= end_year:
		return history

	# Add founding event
	var founding: HistoryEvent = HistoryEvent.new(
		HistoryEvent.EventType.FOUNDING,
		start_year,
		founding_title,
		"The beginning of recorded history.",
		0.5
	)
	history.add_event(founding)

	# Calculate profile-modified weights
	var weights: Dictionary = calculate_event_weights(profile)

	# Generate events at intervals
	var current_year: int = start_year + _calculate_next_interval(profile, rng)
	var last_event_years: Dictionary = {}

	while current_year < end_year:
		var event_type: HistoryEvent.EventType = _pick_event_type(weights, last_event_years, current_year, rng)

		var event: HistoryEvent = _generate_event(event_type, current_year, profile, rng)
		history.add_event(event)

		last_event_years[event_type as int] = current_year

		current_year += _calculate_next_interval(profile, rng)

	return history


## Calculates the interval to the next event.
## @param profile: The planet profile.
## @param rng: Seeded random number generator.
## @return: Years until next event.
static func _calculate_next_interval(profile: PlanetProfile, rng: SeededRng) -> int:
	var base: float = BASE_EVENT_INTERVAL

	if profile.habitability_score < 5:
		base *= 0.7
	elif profile.habitability_score >= 8:
		base *= 1.3

	var interval: float = base * rng.randf_range(0.5, 1.5)

	return maxi(5, roundi(interval))


## Picks an event type based on weights, avoiding recent repeats.
## @param weights: Event type weights.
## @param last_years: Dictionary of EventType -> last year.
## @param current_year: The current year.
## @param rng: Seeded random number generator.
## @return: The selected event type.
static func _pick_event_type(
	weights: Dictionary,
	last_years: Dictionary,
	current_year: int,
	rng: SeededRng
) -> HistoryEvent.EventType:
	var eligible_types: Array = []
	var eligible_weights: Array[float] = []

	for type_key in weights.keys():
		var type_int: int = type_key as int
		var weight: float = weights[type_key] as float

		if last_years.has(type_int):
			var years_since: int = current_year - (last_years[type_int] as int)
			if years_since < MIN_EVENT_SPACING:
				weight *= 0.1
			elif years_since < MIN_EVENT_SPACING * 2:
				weight *= 0.5

		if weight > 0.01:
			eligible_types.append(type_int)
			eligible_weights.append(weight)

	if eligible_types.is_empty():
		return HistoryEvent.EventType.POLITICAL_CHANGE

	var result: Variant = rng.weighted_choice(eligible_types, eligible_weights)
	return result as HistoryEvent.EventType


## Generates a single event of the given type.
## @param type: The event type.
## @param year: The year of the event.
## @param profile: The planet profile (for context).
## @param rng: Seeded random number generator.
## @return: The generated event.
static func _generate_event(
	type: HistoryEvent.EventType,
	year: int,
	profile: PlanetProfile,
	rng: SeededRng
) -> HistoryEvent:
	var title: String = _generate_event_title(type, profile, rng)
	var description: String = _generate_event_description(type, profile, rng)
	var magnitude: float = _generate_event_magnitude(type, rng)

	var event: HistoryEvent = HistoryEvent.new(type, year, title, description, magnitude)

	event.population_delta = _estimate_population_delta(type, magnitude, rng)

	return event


## Generates a title for an event.
static func _generate_event_title(
	type: HistoryEvent.EventType,
	profile: PlanetProfile,
	rng: SeededRng
) -> String:
	match type:
		HistoryEvent.EventType.NATURAL_DISASTER:
			var disasters: Array[String] = ["Great Earthquake", "Volcanic Eruption", "Massive Flood", "Terrible Storm", "Meteor Strike"]
			if profile.volcanism_level > 0.5:
				disasters.append("Volcanic Winter")
			if profile.tectonic_activity > 0.5:
				disasters.append("Tectonic Upheaval")
			return disasters[rng.randi_range(0, disasters.size() - 1)]

		HistoryEvent.EventType.PLAGUE:
			var plagues: Array[String] = ["The Great Plague", "Red Death", "Wasting Sickness", "Silent Fever", "The Blight"]
			return plagues[rng.randi_range(0, plagues.size() - 1)]

		HistoryEvent.EventType.FAMINE:
			var famines: Array[String] = ["The Great Famine", "Years of Want", "The Hungry Time", "Crop Failure", "The Withering"]
			return famines[rng.randi_range(0, famines.size() - 1)]

		HistoryEvent.EventType.WAR:
			var wars: Array[String] = ["The Great War", "War of Succession", "Border Conflict", "The Conquest", "War of Independence"]
			return wars[rng.randi_range(0, wars.size() - 1)]

		HistoryEvent.EventType.CIVIL_WAR:
			var civil_wars: Array[String] = ["The Civil War", "The Rebellion", "The Uprising", "War of Brothers", "The Revolution"]
			return civil_wars[rng.randi_range(0, civil_wars.size() - 1)]

		HistoryEvent.EventType.TECH_ADVANCEMENT:
			var techs: Array[String] = ["Age of Innovation", "The Discovery", "Technical Revolution", "Scientific Breakthrough", "New Era"]
			return techs[rng.randi_range(0, techs.size() - 1)]

		HistoryEvent.EventType.EXPANSION:
			var expansions: Array[String] = ["The Expansion", "New Territories", "The Settling", "Frontier Era", "Colonial Period"]
			return expansions[rng.randi_range(0, expansions.size() - 1)]

		HistoryEvent.EventType.POLITICAL_CHANGE:
			var changes: Array[String] = ["The Reform", "Change of Power", "New Order", "The Transition", "Political Upheaval"]
			return changes[rng.randi_range(0, changes.size() - 1)]

		HistoryEvent.EventType.MIGRATION:
			var migrations: Array[String] = ["The Great Migration", "The Exodus", "Mass Movement", "The Resettlement", "Diaspora"]
			return migrations[rng.randi_range(0, migrations.size() - 1)]

		HistoryEvent.EventType.COLLAPSE:
			var collapses: Array[String] = ["The Collapse", "The Fall", "Dark Age Begins", "The Decline", "End of an Era"]
			return collapses[rng.randi_range(0, collapses.size() - 1)]

		HistoryEvent.EventType.GOLDEN_AGE:
			var ages: Array[String] = ["The Golden Age", "Era of Prosperity", "The Renaissance", "Age of Plenty", "The Flowering"]
			return ages[rng.randi_range(0, ages.size() - 1)]

		HistoryEvent.EventType.CULTURAL_SHIFT:
			var shifts: Array[String] = ["Cultural Revolution", "The Awakening", "New Beliefs", "The Reformation", "Age of Reason"]
			return shifts[rng.randi_range(0, shifts.size() - 1)]

		HistoryEvent.EventType.TREATY:
			var treaties: Array[String] = ["The Grand Treaty", "Peace Accord", "The Alliance", "Trade Agreement", "The Pact"]
			return treaties[rng.randi_range(0, treaties.size() - 1)]

		HistoryEvent.EventType.DISCOVERY:
			var discoveries: Array[String] = ["The Great Discovery", "New Horizons", "Revelation", "The Finding", "Breakthrough"]
			return discoveries[rng.randi_range(0, discoveries.size() - 1)]

		HistoryEvent.EventType.CONSTRUCTION:
			var constructions: Array[String] = ["The Great Work", "Monument Rising", "Infrastructure Boom", "The Building", "Grand Project"]
			return constructions[rng.randi_range(0, constructions.size() - 1)]

		HistoryEvent.EventType.LEADER:
			var leaders: Array[String] = ["Rise of a Leader", "The Ruler", "New Dynasty", "The Reformer", "The Tyrant"]
			return leaders[rng.randi_range(0, leaders.size() - 1)]

		_:
			return "Historical Event"


## Generates a description for an event.
static func _generate_event_description(
	type: HistoryEvent.EventType,
	_profile: PlanetProfile,
	_rng: SeededRng
) -> String:
	match type:
		HistoryEvent.EventType.NATURAL_DISASTER:
			return "A devastating natural disaster struck the population."
		HistoryEvent.EventType.PLAGUE:
			return "A deadly disease spread through the population."
		HistoryEvent.EventType.FAMINE:
			return "Food shortages led to widespread hunger and hardship."
		HistoryEvent.EventType.WAR:
			return "Armed conflict erupted, changing the political landscape."
		HistoryEvent.EventType.CIVIL_WAR:
			return "Internal divisions led to violent conflict within the population."
		HistoryEvent.EventType.TECH_ADVANCEMENT:
			return "Significant technological progress transformed society."
		HistoryEvent.EventType.EXPANSION:
			return "The population expanded into new territories."
		HistoryEvent.EventType.POLITICAL_CHANGE:
			return "The political structure underwent significant change."
		HistoryEvent.EventType.MIGRATION:
			return "Large numbers of people moved to new regions."
		HistoryEvent.EventType.COLLAPSE:
			return "Existing social structures collapsed under pressure."
		HistoryEvent.EventType.GOLDEN_AGE:
			return "A period of unprecedented prosperity and cultural achievement began."
		HistoryEvent.EventType.CULTURAL_SHIFT:
			return "Major changes in beliefs, values, or customs swept through society."
		HistoryEvent.EventType.TREATY:
			return "A significant agreement was reached between parties."
		HistoryEvent.EventType.DISCOVERY:
			return "An important discovery changed understanding of the world."
		HistoryEvent.EventType.CONSTRUCTION:
			return "A major construction project was completed."
		HistoryEvent.EventType.LEADER:
			return "A notable leader rose to prominence."
		_:
			return "A significant event occurred."


## Generates magnitude for an event.
static func _generate_event_magnitude(type: HistoryEvent.EventType, rng: SeededRng) -> float:
	var base_magnitude: float = 0.0
	var variance: float = 0.3

	if HistoryEvent.is_typically_harmful(type):
		base_magnitude = -0.5
	elif HistoryEvent.is_typically_beneficial(type):
		base_magnitude = 0.5

	var magnitude: float = base_magnitude + rng.randf_range(-variance, variance)

	return clampf(magnitude, -1.0, 1.0)


## Estimates population delta for an event.
static func _estimate_population_delta(
	type: HistoryEvent.EventType,
	magnitude: float,
	rng: SeededRng
) -> int:
	var base_delta: int = 0

	match type:
		HistoryEvent.EventType.NATURAL_DISASTER:
			base_delta = rng.randi_range(-10000, -1000)
		HistoryEvent.EventType.PLAGUE:
			base_delta = rng.randi_range(-50000, -5000)
		HistoryEvent.EventType.FAMINE:
			base_delta = rng.randi_range(-20000, -2000)
		HistoryEvent.EventType.WAR:
			base_delta = rng.randi_range(-30000, -3000)
		HistoryEvent.EventType.CIVIL_WAR:
			base_delta = rng.randi_range(-20000, -2000)
		HistoryEvent.EventType.COLLAPSE:
			base_delta = rng.randi_range(-100000, -10000)
		HistoryEvent.EventType.EXPANSION:
			base_delta = rng.randi_range(5000, 50000)
		HistoryEvent.EventType.GOLDEN_AGE:
			base_delta = rng.randi_range(10000, 100000)
		HistoryEvent.EventType.MIGRATION:
			base_delta = rng.randi_range(-10000, 10000)
		_:
			base_delta = 0

	var scaled: float = base_delta * absf(magnitude)

	return roundi(scaled)
