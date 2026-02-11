## Generates colonies for planets based on profile, suitability, and existing natives.
## All functions are deterministic when given a SeededRng.
class_name ColonyGenerator
extends RefCounted

# Preload dependencies.
const _colony: GDScript = preload("res://src/domain/population/Colony.gd")
const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _native_relation: GDScript = preload("res://src/domain/population/NativeRelation.gd")
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _suitability_calculator: GDScript = preload("res://src/domain/population/SuitabilityCalculator.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _history_generator: GDScript = preload("res://src/domain/population/HistoryGenerator.gd")
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Specification for generating colonies.
class ColonySpec:
	## Seed for deterministic generation.
	var seed: int = 0

	## Colony type (or null to pick randomly based on suitability).
	var colony_type: Variant = null # ColonyType.Type or null

	## Name of the colony (or empty to generate).
	var name: String = ""

	## Founding civilization ID.
	var founding_civilization_id: String = "civ_unknown"

	## Founding civilization name.
	var founding_civilization_name: String = "Unknown Civilization"

	## Target end year for history generation (0 = present).
	var current_year: int = 0

	## Founding year (or 0 to generate based on history length).
	var founding_year: int = 0

	## Minimum years of colony history.
	var min_history_years: int = 50

	## Maximum years of colony history.
	var max_history_years: int = 500

	## Technology level of founding civilization.
	var founding_tech_level: TechnologyLevel.Level = TechnologyLevel.Level.INTERSTELLAR

	## Initial population (0 to use colony type default).
	var initial_population: int = 0

	## Whether to automatically establish native relations.
	var establish_native_relations: bool = true


## Generates a colony for a planet.
## @param profile: The planet profile.
## @param suitability: The colony suitability assessment.
## @param existing_natives: Array of existing native populations (can be empty).
## @param spec: Generation specification.
## @param rng: Seeded random number generator.
## @return: A new Colony, or null if planet is unsuitable.
static func generate(
	profile: PlanetProfile,
	suitability: ColonySuitability,
	existing_natives: Array[NativePopulation],
	spec: ColonySpec,
	rng: SeededRng
) -> Colony:
	# Check if colonization is possible
	if not suitability.is_colonizable():
		return null

	var colony: Colony = Colony.new()

	# Generate ID
	colony.id = "colony_%s_%d" % [profile.body_id, rng.randi()]

	# Determine colony type
	colony.colony_type = _determine_colony_type(profile, suitability, spec, rng)

	# Generate or set name
	if spec.name != "":
		colony.name = spec.name
	else:
		colony.name = _generate_colony_name(colony.colony_type, rng)

	# Link to planet
	colony.body_id = profile.body_id

	# Set founding info
	colony.founding_civilization_id = spec.founding_civilization_id
	colony.founding_civilization_name = spec.founding_civilization_name
	colony.tech_level = spec.founding_tech_level

	# Determine founding year
	if spec.founding_year != 0:
		colony.founding_year = spec.founding_year
	else:
		var history_years: int = rng.randi_range(spec.min_history_years, spec.max_history_years)
		colony.founding_year = spec.current_year - history_years

	# Generate government
	colony.government = _generate_government(colony.colony_type, rng)
	colony.government.regime_established_year = colony.founding_year

	# Calculate initial population
	if spec.initial_population > 0:
		colony.population = spec.initial_population
	else:
		colony.population = _calculate_initial_population(colony.colony_type, rng)

	# Project population growth
	var years_elapsed: int = spec.current_year - colony.founding_year
	colony.population = _project_colony_population(
		colony.population,
		years_elapsed,
		suitability,
		colony.colony_type,
		rng
	)

	colony.peak_population = roundi(colony.population * rng.randf_range(1.0, 1.3))
	colony.peak_population_year = spec.current_year - rng.randi_range(0, years_elapsed / 4)

	# Set territorial control
	colony.territorial_control = _calculate_territorial_control(
		profile, suitability, existing_natives, years_elapsed, rng
	)

	# Set primary industry based on colony type and resources
	colony.primary_industry = _determine_primary_industry(profile, colony.colony_type, rng)

	# Calculate self-sufficiency
	colony.self_sufficiency = _calculate_self_sufficiency(
		profile, suitability, years_elapsed, rng
	)

	# Establish native relations
	if spec.establish_native_relations and not existing_natives.is_empty():
		_establish_native_relations(colony, existing_natives, spec.current_year, rng)

	# Generate history
	colony.history = HistoryGenerator.generate_history(
		profile,
		colony.founding_year,
		spec.current_year,
		rng,
		colony.name + " Founded"
	)

	# Add native-related events to history
	_add_native_events_to_history(colony, spec.current_year, rng)

	# Possibly abandoned
	if rng.randf() < 0.05 and years_elapsed > 100:
		var abandon_year: int = spec.current_year - rng.randi_range(10, years_elapsed / 2)
		var reasons: Array[String] = ["resource depletion", "hostile conditions", "native conflict", "economic collapse", "unknown"]
		colony.record_abandonment(abandon_year, reasons[rng.randi_range(0, reasons.size() - 1)])

	# Possibly independent
	if not colony.is_independent and years_elapsed > 200 and rng.randf() < 0.2:
		var indep_year: int = spec.current_year - rng.randi_range(0, 100)
		colony.record_independence(indep_year)

	return colony


## Determines the colony type based on planet and suitability.
static func _determine_colony_type(
	profile: PlanetProfile,
	suitability: ColonySuitability,
	spec: ColonySpec,
	rng: SeededRng
) -> ColonyType.Type:
	if spec.colony_type != null:
		return spec.colony_type as ColonyType.Type

	# Weight colony types based on conditions
	var weights: Dictionary = {}

	for i in range(ColonyType.count()):
		var type: ColonyType.Type = i as ColonyType.Type
		weights[i] = 1.0

	# High suitability favors settlement
	if suitability.overall_score >= 70:
		weights[ColonyType.Type.SETTLEMENT as int] *= 2.0
		weights[ColonyType.Type.AGRICULTURAL as int] *= 1.5

	# Good resources favor corporate/industrial
	var resource_score: int = suitability.get_factor_score(ColonySuitability.FactorType.RESOURCES)
	if resource_score >= 60:
		weights[ColonyType.Type.CORPORATE as int] *= 1.5
		weights[ColonyType.Type.INDUSTRIAL as int] *= 1.5

	# Low suitability may indicate military/scientific purpose
	if suitability.overall_score < 40:
		weights[ColonyType.Type.MILITARY as int] *= 2.0
		weights[ColonyType.Type.SCIENTIFIC as int] *= 2.0
		weights[ColonyType.Type.SETTLEMENT as int] *= 0.3

	# Water-rich for agricultural
	if profile.has_liquid_water and profile.ocean_coverage < 0.9:
		weights[ColonyType.Type.AGRICULTURAL as int] *= 1.5

	# Build arrays for weighted choice
	var types: Array[int] = []
	var weight_values: Array[float] = []
	for type_int in weights.keys():
		types.append(type_int as int)
		weight_values.append(weights[type_int] as float)

	var selected: Variant = rng.weighted_choice(types, weight_values)
	return selected as ColonyType.Type


## Generates a name for a colony.
static func _generate_colony_name(type: ColonyType.Type, rng: SeededRng) -> String:
	var prefixes: Array[String] = [
		"New", "Nova", "Port", "Fort", "Camp", "Station",
		"Haven", "Point", "Landing", "Base"
	]

	var roots: Array[String] = [
		"Terra", "Sol", "Vega", "Hope", "Dawn", "Unity",
		"Prospect", "Fortune", "Pioneer", "Frontier",
		"Avalon", "Elysium", "Arcadia", "Horizon"
	]

	var suffixes: Array[String] = [
		"", " Prime", " Alpha", " Colony", " Station",
		" Settlement", " Outpost", " Base"
	]

	# Military and scientific more likely to use "Base" or "Station"
	if type == ColonyType.Type.MILITARY or type == ColonyType.Type.SCIENTIFIC:
		prefixes = ["Fort", "Station", "Base", "Outpost", "Camp"]
		suffixes = [" Base", " Station", " Outpost", " Alpha", " Prime"]

	var prefix: String = prefixes[rng.randi_range(0, prefixes.size() - 1)]
	var root: String = roots[rng.randi_range(0, roots.size() - 1)]
	var suffix: String = suffixes[rng.randi_range(0, suffixes.size() - 1)]

	# Sometimes just use root + suffix
	if rng.randf() < 0.3:
		return root + suffix

	return prefix + " " + root + suffix


## Generates government for a colony.
static func _generate_government(type: ColonyType.Type, rng: SeededRng) -> Government:
	var gov: Government = Government.create_colony_default(rng, ColonyType.to_string_name(type))

	# Override regime based on colony type
	gov.regime = ColonyType.typical_starting_regime(type)

	return gov


## Calculates initial population based on colony type.
static func _calculate_initial_population(type: ColonyType.Type, rng: SeededRng) -> int:
	var base: int = ColonyType.typical_initial_population(type)
	var variance: float = rng.randf_range(0.7, 1.3)
	return roundi(base * variance)


## Projects colony population growth over time.
static func _project_colony_population(
	initial: int,
	years: int,
	suitability: ColonySuitability,
	type: ColonyType.Type,
	rng: SeededRng
) -> int:
	var growth_rate: float = suitability.base_growth_rate
	growth_rate *= ColonyType.growth_rate_modifier(type)

	# Add some variance
	growth_rate *= rng.randf_range(0.8, 1.2)

	var capacity: int = suitability.carrying_capacity

	return SuitabilityCalculator.project_population(initial, years, growth_rate, capacity)


## Calculates territorial control.
static func _calculate_territorial_control(
	profile: PlanetProfile,
	suitability: ColonySuitability,
	existing_natives: Array[NativePopulation],
	years: int,
	rng: SeededRng
) -> float:
	# Base expansion rate depends on suitability
	var base_control: float = 0.05 + (suitability.overall_score / 100.0) * 0.1

	# Time factor
	var time_factor: float = minf(years / 200.0, 1.0)
	base_control *= 1.0 + time_factor

	# Native presence limits expansion
	var native_control: float = 0.0
	for native in existing_natives:
		if native.is_extant:
			native_control += native.territorial_control

	# Can't exceed what's available
	var available: float = maxf(0.0, 1.0 - native_control * 0.8)

	var control: float = minf(base_control, available)

	# Add randomness
	control *= rng.randf_range(0.7, 1.3)

	return clampf(control, 0.01, 0.95)


## Determines primary industry based on profile and type.
static func _determine_primary_industry(
	profile: PlanetProfile,
	type: ColonyType.Type,
	rng: SeededRng
) -> String:
	match type:
		ColonyType.Type.CORPORATE, ColonyType.Type.INDUSTRIAL:
			var industries: Array[String] = ["mining", "manufacturing", "processing"]
			if profile.resources.has(ResourceType.Type.RARE_ELEMENTS as int):
				industries.append("rare element extraction")
			if profile.resources.has(ResourceType.Type.HYDROCARBONS as int):
				industries.append("petrochemical")
			return industries[rng.randi_range(0, industries.size() - 1)]

		ColonyType.Type.AGRICULTURAL:
			return "agriculture"

		ColonyType.Type.MILITARY:
			return "defense"

		ColonyType.Type.SCIENTIFIC:
			var focuses: Array[String] = ["research", "exploration", "xenobiology", "terraforming study"]
			return focuses[rng.randi_range(0, focuses.size() - 1)]

		ColonyType.Type.RELIGIOUS:
			return "spiritual community"

		_:
			var general: Array[String] = ["mixed economy", "services", "trade"]
			return general[rng.randi_range(0, general.size() - 1)]


## Calculates self-sufficiency level.
static func _calculate_self_sufficiency(
	profile: PlanetProfile,
	suitability: ColonySuitability,
	years: int,
	rng: SeededRng
) -> float:
	# Base from suitability
	var base: float = suitability.overall_score / 200.0 # 0-0.5 base

	# Time factor (colonies become more self-sufficient over time)
	var time_factor: float = minf(years / 300.0, 0.4) # Up to 0.4 from time

	# Resource factor
	var resource_factor: float = suitability.get_factor_score(ColonySuitability.FactorType.RESOURCES) / 200.0

	var sufficiency: float = base + time_factor + resource_factor

	# Breathable atmosphere is a big boost
	if profile.has_breathable_atmosphere:
		sufficiency += 0.15

	# Liquid water is essential
	if profile.has_liquid_water:
		sufficiency += 0.1

	# Add randomness
	sufficiency *= rng.randf_range(0.9, 1.1)

	return clampf(sufficiency, 0.1, 1.0)


## Establishes relations with existing native populations.
static func _establish_native_relations(
	colony: Colony,
	natives: Array[NativePopulation],
	current_year: int,
	rng: SeededRng
) -> void:
	var colony_age: int = current_year - colony.founding_year

	for native in natives:
		if not native.is_extant:
			# Can still have historical relations with extinct natives
			if native.extinction_year > colony.founding_year:
				var relation: NativeRelation = NativeRelation.create_first_contact(
					native.id,
					colony.founding_year + rng.randi_range(1, 50),
					rng.randi_range(-30, 30)
				)
				relation.record_extinction(native.extinction_year, native.extinction_cause)
				colony.set_native_relation(relation)
			continue

		# Determine first contact year
		var contact_year: int = colony.founding_year + rng.randi_range(0, mini(100, colony_age))

		# Initial disposition based on colony type
		var disposition: int = rng.randi_range(-20, 20)
		if ColonyType.tends_toward_native_conflict(colony.colony_type):
			disposition -= 30
		if colony.colony_type == ColonyType.Type.SCIENTIFIC:
			disposition += 20

		var relation: NativeRelation = NativeRelation.create_first_contact(
			native.id,
			contact_year,
			disposition
		)

		# Evolve the relationship over time
		_evolve_native_relation(relation, colony, native, current_year, rng)

		colony.set_native_relation(relation)


## Evolves a native relation over time.
static func _evolve_native_relation(
	relation: NativeRelation,
	colony: Colony,
	native: NativePopulation,
	current_year: int,
	rng: SeededRng
) -> void:
	var years_of_contact: int = current_year - relation.first_contact_year
	if years_of_contact <= 0:
		return

	# Simulate events over time
	var events_count: int = years_of_contact / 50 # Roughly one significant event per 50 years

	for i in range(events_count):
		var event_type: int = rng.randi_range(0, 10)

		match event_type:
			0, 1: # Conflict
				var intensity: float = rng.randf_range(0.2, 0.8)
				var event_year: int = relation.first_contact_year + rng.randi_range(10, years_of_contact)
				relation.record_conflict(event_year, "Territorial dispute", intensity)
				relation.territory_taken += rng.randf_range(0.05, 0.15)

			2, 3: # Trade development
				relation.trade_level = minf(relation.trade_level + rng.randf_range(0.1, 0.3), 1.0)
				relation.relation_score = mini(relation.relation_score + 10, 100)

			4: # Treaty
				if relation.relation_score > -30:
					var treaty_year: int = relation.first_contact_year + rng.randi_range(20, years_of_contact)
					relation.record_treaty(treaty_year, "Peace and trade agreement")

			5, 6: # Cultural exchange
				relation.cultural_exchange = minf(relation.cultural_exchange + rng.randf_range(0.1, 0.2), 1.0)
				relation.relation_score = mini(relation.relation_score + 5, 100)

			_: # General drift
				relation.relation_score += rng.randi_range(-10, 10)
				relation.relation_score = clampi(relation.relation_score, -100, 100)

	# Clamp territory taken
	relation.territory_taken = clampf(relation.territory_taken, 0.0, 0.9)

	# Update final status
	relation.update_status()


## Adds native-related events to colony history.
static func _add_native_events_to_history(
	colony: Colony,
	current_year: int,
	rng: SeededRng
) -> void:
	for relation in colony.native_relations.values():
		var rel: NativeRelation = relation as NativeRelation

		# Add first contact event
		if rel.first_contact_year > 0:
			colony.history.add_new_event(
				HistoryEvent.EventType.CONTACT,
				rel.first_contact_year,
				"First Contact",
				"Contact established with native population.",
				0.0
			)

		# Add treaty events
		if rel.has_treaty:
			colony.history.add_new_event(
				HistoryEvent.EventType.TREATY,
				rel.treaty_year,
				"Native Treaty",
				"Treaty signed with native population.",
				0.3
			)
