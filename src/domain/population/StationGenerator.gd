## Generates stations and outposts for a solar system.
## Uses placement rules and specs to create appropriate stations.
class_name StationGenerator
extends RefCounted

# Preload dependencies.
const _outpost: GDScript = preload("res://src/domain/population/Outpost.gd")
const _space_station: GDScript = preload("res://src/domain/population/SpaceStation.gd")
const _station_spec: GDScript = preload("res://src/domain/population/StationSpec.gd")
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_service: GDScript = preload("res://src/domain/population/StationService.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _station_placement_rules: GDScript = preload("res://src/domain/population/StationPlacementRules.gd")
const _outpost_authority: GDScript = preload("res://src/domain/population/OutpostAuthority.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")


## Result of station generation for a system.
class GenerationResult extends RefCounted:
	## Generated outposts (U/O class).
	var outposts: Array[Outpost] = []

	## Generated space stations (any class, including U/O that may grow).
	var stations: Array[SpaceStation] = []

	## The placement recommendation used.
	var recommendation: StationPlacementRules.PlacementRecommendation = null

	## Seed used for generation.
	var generation_seed: int = 0

	## Any warnings during generation.
	var warnings: Array[String] = []

	## Returns total station count.
	func get_total_count() -> int:
		return outposts.size() + stations.size()

	## Returns all stations as a combined array (SpaceStation only, outposts excluded).
	func get_all_stations() -> Array[SpaceStation]:
		return stations.duplicate()

	## Returns stations orbiting a specific body.
	func get_stations_for_body(body_id: String) -> Array[SpaceStation]:
		var result: Array[SpaceStation] = []
		for station in stations:
			if station.orbiting_body_id == body_id:
				result.append(station)
		return result

	## Returns outposts orbiting a specific body.
	func get_outposts_for_body(body_id: String) -> Array[Outpost]:
		var result: Array[Outpost] = []
		for outpost in outposts:
			if outpost.orbiting_body_id == body_id:
				result.append(outpost)
		return result

	## Converts to dictionary.
	func to_dict() -> Dictionary:
		var outposts_data: Array[Dictionary] = []
		for o in outposts:
			outposts_data.append(o.to_dict())

		var stations_data: Array[Dictionary] = []
		for s in stations:
			stations_data.append(s.to_dict())

		return {
			"outposts": outposts_data,
			"stations": stations_data,
			"generation_seed": generation_seed,
			"warnings": warnings.duplicate(),
			"recommendation": recommendation.to_dict() if recommendation else {},
		}


## Population ranges by station class.
const POP_RANGE_U: Dictionary = {"min": 50, "max": 5000}
const POP_RANGE_O: Dictionary = {"min": 100, "max": 10000}
const POP_RANGE_B: Dictionary = {"min": 10000, "max": 100000}
const POP_RANGE_A: Dictionary = {"min": 100000, "max": 1000000}
const POP_RANGE_S: Dictionary = {"min": 1000000, "max": 10000000}

## Station name prefixes by purpose.
const NAME_PREFIXES: Dictionary = {
	StationPurpose.Purpose.UTILITY: ["Waypoint", "Rest Stop", "Junction", "Crossroads"],
	StationPurpose.Purpose.TRADE: ["Trade Hub", "Market", "Exchange", "Commerce"],
	StationPurpose.Purpose.MILITARY: ["Outpost", "Bastion", "Sentinel", "Watchtower"],
	StationPurpose.Purpose.SCIENCE: ["Observatory", "Research", "Survey", "Lab"],
	StationPurpose.Purpose.MINING: ["Excavation", "Extraction", "Drill", "Mine"],
	StationPurpose.Purpose.RESIDENTIAL: ["Habitat", "Colony", "Settlement", "Haven"],
	StationPurpose.Purpose.ADMINISTRATIVE: ["Central", "Nexus", "Hub", "Authority"],
	StationPurpose.Purpose.INDUSTRIAL: ["Foundry", "Factory", "Works", "Forge"],
	StationPurpose.Purpose.MEDICAL: ["Medical", "Hospital", "Clinic", "Care"],
	StationPurpose.Purpose.COMMUNICATIONS: ["Relay", "Signal", "Beacon", "Comm"],
}

## Greek letters for station naming.
const GREEK_LETTERS: Array[String] = [
	"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
	"Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi"
]


## Generates stations for a system.
## @param system_context: The system context from placement rules.
## @param spec: Generation specification (null = use defaults).
## @param rng: Random number generator (null = create from spec seed).
## @return: GenerationResult with created stations.
static func generate(
	system_context: StationPlacementRules.SystemContext,
	spec: StationSpec = null,
	rng: RandomNumberGenerator = null
) -> GenerationResult:
	var result: GenerationResult = GenerationResult.new()

	# Use default spec if none provided
	if spec == null:
		spec = StationSpec.standard()

	# Validate spec
	if not spec.is_valid():
		result.warnings.append_array(spec.validate())
		return result

	# Check if generation is enabled
	if not spec.generate_stations:
		return result

	# Setup RNG
	if rng == null:
		rng = RandomNumberGenerator.new()
		if spec.generation_seed != 0:
			rng.seed = spec.generation_seed
		else:
			rng.randomize()
	result.generation_seed = rng.seed

	# Get placement recommendation
	var recommendation: StationPlacementRules.PlacementRecommendation
	if spec.force_context != null:
		recommendation = _create_forced_recommendation(spec.force_context as StationPlacementContext.Context, system_context)
	else:
		recommendation = StationPlacementRules.evaluate_system(system_context)
	result.recommendation = recommendation

	# Check if system should have stations
	if not recommendation.should_have_stations and spec.min_stations == 0:
		return result

	# Calculate station counts
	var utility_count: int = _calculate_count(recommendation.utility_station_count, spec, rng)
	var outpost_count: int = _calculate_count(recommendation.outpost_count, spec, rng)
	var large_count: int = _calculate_count(recommendation.large_station_count, spec, rng)

	# Apply spec limits
	var total_count: int = utility_count + outpost_count + large_count
	if spec.max_stations > 0 and total_count > spec.max_stations:
		var scale: float = float(spec.max_stations) / float(total_count)
		utility_count = int(utility_count * scale)
		outpost_count = int(outpost_count * scale)
		large_count = int(large_count * scale)
		if total_count > 0 and utility_count + outpost_count + large_count == 0:
			utility_count = 1

	if spec.min_stations > 0:
		total_count = utility_count + outpost_count + large_count
		if total_count < spec.min_stations:
			var deficit: int = spec.min_stations - total_count
			if recommendation.context == StationPlacementContext.Context.BRIDGE_SYSTEM:
				utility_count += deficit
			elif spec.allow_large_stations:
				large_count += deficit
			else:
				outpost_count += deficit

	# Filter by spec allowances
	if not spec.allow_utility:
		outpost_count += utility_count
		utility_count = 0
	if not spec.allow_outposts and not spec.allow_utility:
		large_count += outpost_count
		outpost_count = 0
	if not spec.allow_large_stations:
		outpost_count += large_count
		large_count = 0

	# Generate utility stations as Outposts
	for i in range(utility_count):
		var outpost: Outpost = _generate_outpost(
			system_context, spec, recommendation, StationClass.Class.U, i, rng
		)
		if outpost != null:
			result.outposts.append(outpost)

	# Generate outposts
	for i in range(outpost_count):
		var outpost: Outpost = _generate_outpost(
			system_context, spec, recommendation, StationClass.Class.O, i + utility_count, rng
		)
		if outpost != null:
			result.outposts.append(outpost)

	# Generate larger stations
	for i in range(large_count):
		var station: SpaceStation = _generate_station(
			system_context, spec, recommendation, i, rng
		)
		if station != null:
			result.stations.append(station)

	return result


## Creates a recommendation for a forced context.
static func _create_forced_recommendation(
	context: StationPlacementContext.Context,
	system_context: StationPlacementRules.SystemContext
) -> StationPlacementRules.PlacementRecommendation:
	var rec: StationPlacementRules.PlacementRecommendation = StationPlacementRules.PlacementRecommendation.new()
	rec.context = context
	rec.should_have_stations = true
	rec.reasoning.append("Forced context: %s" % StationPlacementContext.to_string_name(context))

	match context:
		StationPlacementContext.Context.BRIDGE_SYSTEM:
			rec.utility_station_count = 1
			rec.allow_deep_space = true
			rec.recommended_purposes = [StationPurpose.Purpose.UTILITY, StationPurpose.Purpose.TRADE]
		StationPlacementContext.Context.COLONY_WORLD:
			rec.large_station_count = maxi(1, system_context.colony_world_count)
			rec.orbital_candidates = system_context.colony_planet_ids.duplicate()
			rec.recommended_purposes = [StationPurpose.Purpose.TRADE, StationPurpose.Purpose.RESIDENTIAL]
		StationPlacementContext.Context.NATIVE_WORLD:
			rec.large_station_count = maxi(1, system_context.native_world_count)
			rec.orbital_candidates = system_context.native_planet_ids.duplicate()
			rec.recommended_purposes = [StationPurpose.Purpose.TRADE, StationPurpose.Purpose.ADMINISTRATIVE]
		StationPlacementContext.Context.RESOURCE_SYSTEM:
			rec.outpost_count = 2
			rec.allow_belt_stations = system_context.asteroid_belt_count > 0
			rec.allow_deep_space = true
			rec.orbital_candidates = system_context.resource_body_ids.duplicate()
			rec.recommended_purposes = [StationPurpose.Purpose.MINING, StationPurpose.Purpose.INDUSTRIAL]
		StationPlacementContext.Context.STRATEGIC:
			rec.outpost_count = 1
			rec.allow_deep_space = true
			rec.recommended_purposes = [StationPurpose.Purpose.MILITARY]
		StationPlacementContext.Context.SCIENTIFIC:
			rec.outpost_count = 1
			rec.allow_deep_space = true
			rec.orbital_candidates = system_context.native_planet_ids.duplicate()
			rec.recommended_purposes = [StationPurpose.Purpose.SCIENCE]
		_:
			rec.outpost_count = 1
			rec.allow_deep_space = true
			rec.recommended_purposes = [StationPurpose.Purpose.UTILITY]

	return rec


## Calculates actual count from recommendation with variance.
static func _calculate_count(base_count: int, spec: StationSpec, rng: RandomNumberGenerator) -> int:
	if base_count == 0:
		return 0

	var modified: float = base_count * spec.population_density
	var variance: float = modified * 0.25
	modified += rng.randf_range(-variance, variance)

	return maxi(0, int(round(modified)))


## Generates an outpost.
static func _generate_outpost(
	system_context: StationPlacementRules.SystemContext,
	spec: StationSpec,
	recommendation: StationPlacementRules.PlacementRecommendation,
	station_class: StationClass.Class,
	index: int,
	rng: RandomNumberGenerator
) -> Outpost:
	var outpost: Outpost = Outpost.new()

	outpost.id = "%s_%s_%03d" % [spec.id_prefix, system_context.system_id, index]
	outpost.station_class = station_class
	outpost.primary_purpose = _select_purpose(recommendation, spec, station_class, rng)
	outpost.name = _generate_name(outpost.primary_purpose, index, rng)
	outpost.system_id = system_context.system_id
	outpost.station_type = _select_station_type(recommendation, spec, rng)

	if outpost.station_type == StationType.Type.ORBITAL:
		outpost.orbiting_body_id = _select_orbital_body(recommendation, rng)

	outpost.placement_context = recommendation.context
	outpost.authority = _select_authority(outpost.primary_purpose, rng)
	if OutpostAuthority.has_parent_organization(outpost.authority):
		outpost.parent_organization_id = "org_%s_%03d" % [system_context.system_id, rng.randi() % 1000]
		outpost.parent_organization_name = _generate_org_name(outpost.authority, rng)

	var pop_range: Dictionary = POP_RANGE_U if station_class == StationClass.Class.U else POP_RANGE_O
	var base_pop: int = rng.randi_range(pop_range["min"], pop_range["max"])
	outpost.population = int(base_pop * spec.population_density)
	outpost.population = clampi(outpost.population, pop_range["min"], Outpost.MAX_POPULATION)

	outpost.established_year = rng.randi_range(spec.min_established_year, spec.max_established_year)
	outpost.services = _select_services(outpost.primary_purpose, station_class, rng)
	outpost.update_commander_title()

	if rng.randf() < spec.decommission_chance:
		var decom_year: int = rng.randi_range(outpost.established_year + 10, spec.max_established_year)
		outpost.record_decommissioning(decom_year, _generate_decommission_reason(rng))

	return outpost


## Generates a larger station.
static func _generate_station(
	system_context: StationPlacementRules.SystemContext,
	spec: StationSpec,
	recommendation: StationPlacementRules.PlacementRecommendation,
	index: int,
	rng: RandomNumberGenerator
) -> SpaceStation:
	var station: SpaceStation = SpaceStation.new()

	station.id = "%s_%s_%03d" % [spec.id_prefix, system_context.system_id, index + 100]
	station.primary_purpose = _select_purpose(recommendation, spec, StationClass.Class.B, rng)
	station.name = _generate_name(station.primary_purpose, index, rng)
	station.system_id = system_context.system_id

	if not recommendation.orbital_candidates.is_empty():
		station.station_type = StationType.Type.ORBITAL
		station.orbiting_body_id = _select_orbital_body(recommendation, rng)
	elif recommendation.allow_deep_space:
		station.station_type = StationType.Type.DEEP_SPACE
	else:
		station.station_type = StationType.Type.ORBITAL
		if not system_context.planet_ids.is_empty():
			station.orbiting_body_id = system_context.planet_ids[rng.randi() % system_context.planet_ids.size()]

	station.placement_context = recommendation.context

	var target_class: StationClass.Class = StationPlacementRules.recommend_station_class(
		recommendation.context,
		_is_large_population_context(system_context)
	)

	var pop_range: Dictionary = _get_pop_range(target_class)
	var base_pop: int = rng.randi_range(pop_range["min"], pop_range["max"])
	station.population = int(base_pop * spec.population_density)

	station.update_class_from_population()
	station.established_year = rng.randi_range(spec.min_established_year, spec.max_established_year)
	station.services = _select_services(station.primary_purpose, station.station_class, rng)
	station.founding_civilization_id = spec.founding_civilization_id
	station.founding_civilization_name = spec.founding_civilization_name

	if station.uses_outpost_government():
		station.outpost_authority = _select_authority(station.primary_purpose, rng)
		if OutpostAuthority.has_parent_organization(station.outpost_authority):
			station.parent_organization_id = "org_%s_%03d" % [system_context.system_id, rng.randi() % 1000]
			station.parent_organization_name = _generate_org_name(station.outpost_authority, rng)
		station.update_commander_title()
	else:
		if station.government == null:
			station.government = Government.new()
		station.government.regime = _select_regime(recommendation.context, rng)
		station.government.legitimacy = rng.randf_range(0.5, 0.95)

		if station.history == null:
			station.history = PopulationHistory.new()
		station.history.add_new_event(
			HistoryEvent.EventType.FOUNDING,
			station.established_year,
			"Station Founded",
			"%s established" % station.name
		)

	station.peak_population = station.population
	station.peak_population_year = spec.max_established_year

	if rng.randf() < spec.decommission_chance * 0.5:
		var decom_year: int = rng.randi_range(station.established_year + 50, spec.max_established_year)
		station.record_decommissioning(decom_year, _generate_decommission_reason(rng))

	return station


## Selects a purpose based on recommendation and spec.
static func _select_purpose(
	recommendation: StationPlacementRules.PlacementRecommendation,
	spec: StationSpec,
	station_class: StationClass.Class,
	rng: RandomNumberGenerator
) -> StationPurpose.Purpose:
	var allowed: Array[StationPurpose.Purpose] = []

	for purpose in recommendation.recommended_purposes:
		if spec.is_purpose_allowed(purpose):
			allowed.append(purpose)

	if allowed.is_empty():
		if station_class == StationClass.Class.U:
			allowed = StationPurpose.typical_utility_purposes()
		elif station_class == StationClass.Class.O:
			allowed = StationPurpose.typical_outpost_purposes()
		else:
			allowed = StationPurpose.typical_settlement_purposes()

		var filtered: Array[StationPurpose.Purpose] = []
		for p in allowed:
			if spec.is_purpose_allowed(p):
				filtered.append(p)
		if not filtered.is_empty():
			allowed = filtered

	if allowed.is_empty():
		return StationPurpose.Purpose.UTILITY

	return allowed[rng.randi() % allowed.size()]


## Selects station type based on recommendation.
static func _select_station_type(
	recommendation: StationPlacementRules.PlacementRecommendation,
	spec: StationSpec,
	rng: RandomNumberGenerator
) -> StationType.Type:
	var options: Array[StationType.Type] = []

	if not recommendation.orbital_candidates.is_empty():
		options.append(StationType.Type.ORBITAL)

	if recommendation.allow_deep_space and spec.allow_deep_space:
		options.append(StationType.Type.DEEP_SPACE)

	if recommendation.allow_belt_stations and spec.allow_belt_stations:
		options.append(StationType.Type.ASTEROID_BELT)

	if options.is_empty():
		return StationType.Type.DEEP_SPACE

	return options[rng.randi() % options.size()]


## Selects an orbital body from candidates.
static func _select_orbital_body(
	recommendation: StationPlacementRules.PlacementRecommendation,
	rng: RandomNumberGenerator
) -> String:
	if recommendation.orbital_candidates.is_empty():
		return ""
	return recommendation.orbital_candidates[rng.randi() % recommendation.orbital_candidates.size()]


## Selects authority type based on purpose.
static func _select_authority(
	purpose: StationPurpose.Purpose,
	rng: RandomNumberGenerator
) -> OutpostAuthority.Type:
	var options: Array[OutpostAuthority.Type] = []

	match purpose:
		StationPurpose.Purpose.UTILITY:
			options = [OutpostAuthority.Type.CORPORATE, OutpostAuthority.Type.FRANCHISE, OutpostAuthority.Type.INDEPENDENT]
		StationPurpose.Purpose.MILITARY:
			options = [OutpostAuthority.Type.MILITARY, OutpostAuthority.Type.GOVERNMENT]
		StationPurpose.Purpose.SCIENCE:
			options = [OutpostAuthority.Type.GOVERNMENT, OutpostAuthority.Type.CORPORATE, OutpostAuthority.Type.COOPERATIVE]
		StationPurpose.Purpose.MINING:
			options = [OutpostAuthority.Type.CORPORATE, OutpostAuthority.Type.COOPERATIVE, OutpostAuthority.Type.INDEPENDENT]
		StationPurpose.Purpose.TRADE:
			options = [OutpostAuthority.Type.CORPORATE, OutpostAuthority.Type.FRANCHISE, OutpostAuthority.Type.INDEPENDENT]
		_:
			options = OutpostAuthority.typical_for_outpost()

	return options[rng.randi() % options.size()]


## Selects services based on purpose and class.
static func _select_services(
	purpose: StationPurpose.Purpose,
	station_class: StationClass.Class,
	rng: RandomNumberGenerator
) -> Array[StationService.Service]:
	var services: Array[StationService.Service] = []

	services.append(StationService.Service.COMMUNICATIONS)

	match purpose:
		StationPurpose.Purpose.UTILITY:
			services.append(StationService.Service.REFUEL)
			services.append(StationService.Service.REPAIR)
			services.append(StationService.Service.LODGING)
			if rng.randf() > 0.5:
				services.append(StationService.Service.TRADE)
		StationPurpose.Purpose.TRADE:
			services.append(StationService.Service.TRADE)
			services.append(StationService.Service.STORAGE)
			services.append(StationService.Service.CUSTOMS)
			if rng.randf() > 0.3:
				services.append(StationService.Service.REFUEL)
		StationPurpose.Purpose.MILITARY:
			services.append(StationService.Service.SECURITY)
			services.append(StationService.Service.REPAIR)
			if rng.randf() > 0.5:
				services.append(StationService.Service.MEDICAL)
		StationPurpose.Purpose.SCIENCE:
			services.append(StationService.Service.LODGING)
			if rng.randf() > 0.5:
				services.append(StationService.Service.MEDICAL)
		StationPurpose.Purpose.MINING:
			services.append(StationService.Service.STORAGE)
			services.append(StationService.Service.REFUEL)
			if rng.randf() > 0.5:
				services.append(StationService.Service.REPAIR)
		StationPurpose.Purpose.RESIDENTIAL:
			services.append(StationService.Service.LODGING)
			services.append(StationService.Service.MEDICAL)
			services.append(StationService.Service.ENTERTAINMENT)
		StationPurpose.Purpose.INDUSTRIAL:
			services.append(StationService.Service.STORAGE)
			services.append(StationService.Service.REPAIR)
		_:
			services.append(StationService.Service.REFUEL)

	if StationClass.uses_colony_government(station_class):
		if StationService.Service.MEDICAL not in services:
			services.append(StationService.Service.MEDICAL)
		if rng.randf() > 0.3:
			services.append(StationService.Service.BANKING)
		if rng.randf() > 0.5:
			services.append(StationService.Service.ENTERTAINMENT)
		if station_class >= StationClass.Class.A and rng.randf() > 0.3:
			services.append(StationService.Service.SHIPYARD)

	return services


## Selects a government regime based on context.
static func _select_regime(
	context: StationPlacementContext.Context,
	rng: RandomNumberGenerator
) -> GovernmentType.Regime:
	var options: Array[GovernmentType.Regime] = []

	match context:
		StationPlacementContext.Context.COLONY_WORLD:
			options = [
				GovernmentType.Regime.CONSTITUTIONAL,
				GovernmentType.Regime.CORPORATE,
				GovernmentType.Regime.OLIGARCHIC
			]
		StationPlacementContext.Context.NATIVE_WORLD:
			options = [
				GovernmentType.Regime.CONSTITUTIONAL,
				GovernmentType.Regime.ELITE_REPUBLIC
			]
		StationPlacementContext.Context.RESOURCE_SYSTEM:
			options = [
				GovernmentType.Regime.CORPORATE,
				GovernmentType.Regime.TECHNOCRACY,
				GovernmentType.Regime.OLIGARCHIC
			]
		_:
			options = [
				GovernmentType.Regime.CONSTITUTIONAL,
				GovernmentType.Regime.CORPORATE,
				GovernmentType.Regime.TECHNOCRACY
			]

	return options[rng.randi() % options.size()]


## Generates a station name.
static func _generate_name(purpose: StationPurpose.Purpose, index: int, rng: RandomNumberGenerator) -> String:
	var prefixes: Array = NAME_PREFIXES.get(purpose, ["Station"]) as Array
	var prefix: String = prefixes[rng.randi() % prefixes.size()] as String

	if index < GREEK_LETTERS.size() and rng.randf() > 0.3:
		return "%s %s" % [prefix, GREEK_LETTERS[index]]
	else:
		return "%s %d" % [prefix, index + 1]


## Generates an organization name.
static func _generate_org_name(authority: OutpostAuthority.Type, rng: RandomNumberGenerator) -> String:
	var corp_names: Array[String] = ["Stellar", "Nova", "Cosmos", "Orbital", "Horizon", "Frontier"]
	var suffixes: Array[String] = ["Corp", "Industries", "Enterprises", "Holdings", "Group"]

	match authority:
		OutpostAuthority.Type.CORPORATE:
			return "%s %s" % [corp_names[rng.randi() % corp_names.size()], suffixes[rng.randi() % suffixes.size()]]
		OutpostAuthority.Type.MILITARY:
			return "Defense Command %d" % (rng.randi() % 100 + 1)
		OutpostAuthority.Type.FRANCHISE:
			return "%s Services" % corp_names[rng.randi() % corp_names.size()]
		OutpostAuthority.Type.GOVERNMENT:
			return "Colonial Administration"
		OutpostAuthority.Type.RELIGIOUS:
			return "Order of the Stars"
		_:
			return "Independent Operators"


## Generates a decommission reason.
static func _generate_decommission_reason(rng: RandomNumberGenerator) -> String:
	var reasons: Array[String] = [
		"Resource depletion",
		"Structural failure",
		"Economic downturn",
		"Relocated operations",
		"Political changes",
		"Natural disaster",
		"Abandonment",
		"Consolidation"
	]
	return reasons[rng.randi() % reasons.size()]


## Gets population range for a class.
static func _get_pop_range(station_class: StationClass.Class) -> Dictionary:
	match station_class:
		StationClass.Class.U:
			return POP_RANGE_U
		StationClass.Class.O:
			return POP_RANGE_O
		StationClass.Class.B:
			return POP_RANGE_B
		StationClass.Class.A:
			return POP_RANGE_A
		StationClass.Class.S:
			return POP_RANGE_S
		_:
			return POP_RANGE_O


## Checks if context has large population.
static func _is_large_population_context(context: StationPlacementRules.SystemContext) -> bool:
	return context.colony_world_count > 0 or context.has_spacefaring_natives
