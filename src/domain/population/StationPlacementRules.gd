## Pure functions for determining station placement in a system.
## Evaluates system context, population presence, and resources to recommend station types.
class_name StationPlacementRules
extends RefCounted

# Preload dependencies.
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")


## Result of evaluating a system for station placement.
## Contains recommendations for what stations to place and where.
class PlacementRecommendation extends RefCounted:
	## Primary placement context for this system.
	var context: StationPlacementContext.Context = StationPlacementContext.Context.OTHER

	## Whether the system should have stations at all.
	var should_have_stations: bool = false

	## Recommended number of utility stations (U-class).
	var utility_station_count: int = 0

	## Recommended number of outposts (O-class).
	var outpost_count: int = 0

	## Recommended number of larger stations (B+ class).
	var large_station_count: int = 0

	## Recommended purposes for stations in this system.
	var recommended_purposes: Array[StationPurpose.Purpose] = []

	## Body IDs that are good candidates for orbital stations.
	var orbital_candidates: Array[String] = []

	## Whether deep space stations are appropriate.
	var allow_deep_space: bool = false

	## Whether asteroid belt stations are appropriate.
	var allow_belt_stations: bool = false

	## Reasoning for the recommendation (for debugging/display).
	var reasoning: Array[String] = []

	## Converts to dictionary for inspection.
	func to_dict() -> Dictionary:
		var purposes_int: Array[int] = []
		for p in recommended_purposes:
			purposes_int.append(p as int)
		return {
			"context": context as int,
			"should_have_stations": should_have_stations,
			"utility_station_count": utility_station_count,
			"outpost_count": outpost_count,
			"large_station_count": large_station_count,
			"recommended_purposes": purposes_int,
			"orbital_candidates": orbital_candidates.duplicate(),
			"allow_deep_space": allow_deep_space,
			"allow_belt_stations": allow_belt_stations,
			"reasoning": reasoning.duplicate(),
		}


## Context about a system for placement evaluation.
## This is a simplified view of system data for placement rules.
class SystemContext extends RefCounted:
	## System ID.
	var system_id: String = ""

	## Whether this system is a bridge/waypoint between regions.
	var is_bridge_system: bool = false

	## Number of habitable planets (habitability >= 5).
	var habitable_planet_count: int = 0

	## Number of planets with native populations.
	var native_world_count: int = 0

	## Number of planets with colonies.
	var colony_world_count: int = 0

	## Highest native tech level in the system (null if no natives).
	var highest_native_tech: Variant = null # TechnologyLevel.Level or null

	## Whether any natives are spacefaring.
	var has_spacefaring_natives: bool = false

	## Total resource richness score (0-1).
	var resource_richness: float = 0.0

	## Number of asteroid belts.
	var asteroid_belt_count: int = 0

	## IDs of planets suitable for orbital stations.
	var planet_ids: Array[String] = []

	## IDs of planets with native populations.
	var native_planet_ids: Array[String] = []

	## IDs of planets with colonies.
	var colony_planet_ids: Array[String] = []

	## IDs of resource-rich bodies (planets, moons, asteroids).
	var resource_body_ids: Array[String] = []


## Thresholds for placement decisions.
const RESOURCE_RICH_THRESHOLD: float = 0.4
const HIGH_RESOURCE_THRESHOLD: float = 0.7
const MIN_HABITABILITY_FOR_COLONY: int = 3


## Evaluates a system and returns placement recommendations.
## @param context: SystemContext describing the system.
## @return: PlacementRecommendation with station suggestions.
static func evaluate_system(context: SystemContext) -> PlacementRecommendation:
	var rec: PlacementRecommendation = PlacementRecommendation.new()

	# Determine primary context
	rec.context = _determine_primary_context(context)
	rec.reasoning.append("Primary context: %s" % StationPlacementContext.to_string_name(rec.context))

	# Evaluate based on context
	match rec.context:
		StationPlacementContext.Context.BRIDGE_SYSTEM:
			_apply_bridge_system_rules(context, rec)
		StationPlacementContext.Context.NATIVE_WORLD:
			_apply_native_world_rules(context, rec)
		StationPlacementContext.Context.COLONY_WORLD:
			_apply_colony_world_rules(context, rec)
		StationPlacementContext.Context.RESOURCE_SYSTEM:
			_apply_resource_system_rules(context, rec)
		StationPlacementContext.Context.STRATEGIC:
			_apply_strategic_rules(context, rec)
		StationPlacementContext.Context.SCIENTIFIC:
			_apply_scientific_rules(context, rec)
		_:
			_apply_default_rules(context, rec)

	return rec


## Determines the primary placement context for a system.
static func _determine_primary_context(context: SystemContext) -> StationPlacementContext.Context:
	# Priority order for context determination
	# Bridge systems are explicitly marked
	if context.is_bridge_system:
		return StationPlacementContext.Context.BRIDGE_SYSTEM

	# Native worlds with spacefaring natives take priority
	if context.has_spacefaring_natives:
		return StationPlacementContext.Context.NATIVE_WORLD

	# Colony worlds
	if context.colony_world_count > 0:
		return StationPlacementContext.Context.COLONY_WORLD

	# Resource-rich systems without habitable worlds
	if context.resource_richness >= RESOURCE_RICH_THRESHOLD and context.habitable_planet_count == 0:
		return StationPlacementContext.Context.RESOURCE_SYSTEM

	# Systems with non-spacefaring natives (scientific interest)
	if context.native_world_count > 0:
		return StationPlacementContext.Context.SCIENTIFIC

	# Habitable but uncolonized (strategic value)
	if context.habitable_planet_count > 0:
		return StationPlacementContext.Context.STRATEGIC

	# Default
	return StationPlacementContext.Context.OTHER


## Applies rules for bridge/waypoint systems.
static func _apply_bridge_system_rules(context: SystemContext, rec: PlacementRecommendation) -> void:
	rec.should_have_stations = true
	rec.reasoning.append("Bridge systems typically have utility stations for travelers")

	# Bridge systems get utility stations
	rec.utility_station_count = 1
	rec.recommended_purposes.append(StationPurpose.Purpose.UTILITY)
	rec.recommended_purposes.append(StationPurpose.Purpose.TRADE)

	# Allow deep space placement for waypoints
	rec.allow_deep_space = true

	# If there are also colonies, add more capacity
	if context.colony_world_count > 0:
		rec.large_station_count = context.colony_world_count
		rec.orbital_candidates.append_array(context.colony_planet_ids)
		rec.reasoning.append("Colony worlds present; adding orbital stations")


## Applies rules for systems with spacefaring natives.
static func _apply_native_world_rules(context: SystemContext, rec: PlacementRecommendation) -> void:
	rec.should_have_stations = true
	rec.reasoning.append("Spacefaring natives can support orbital stations")

	# Multiple stations per native world with spacefaring tech
	rec.large_station_count = context.native_world_count * 2
	rec.orbital_candidates.append_array(context.native_planet_ids)

	rec.recommended_purposes.append(StationPurpose.Purpose.TRADE)
	rec.recommended_purposes.append(StationPurpose.Purpose.RESIDENTIAL)
	rec.recommended_purposes.append(StationPurpose.Purpose.ADMINISTRATIVE)

	# Also support any colonies
	if context.colony_world_count > 0:
		rec.large_station_count += context.colony_world_count
		rec.orbital_candidates.append_array(context.colony_planet_ids)


## Applies rules for systems with colony worlds.
static func _apply_colony_world_rules(context: SystemContext, rec: PlacementRecommendation) -> void:
	rec.should_have_stations = true
	rec.reasoning.append("Colony worlds typically have orbital support stations")

	# At least one station per colony world
	rec.large_station_count = context.colony_world_count
	rec.orbital_candidates.append_array(context.colony_planet_ids)

	rec.recommended_purposes.append(StationPurpose.Purpose.TRADE)
	rec.recommended_purposes.append(StationPurpose.Purpose.UTILITY)

	# Resource-rich systems get additional mining outposts
	if context.resource_richness >= RESOURCE_RICH_THRESHOLD:
		rec.outpost_count = _calculate_mining_outposts(context)
		rec.recommended_purposes.append(StationPurpose.Purpose.MINING)
		rec.allow_belt_stations = context.asteroid_belt_count > 0
		rec.reasoning.append("Resource richness supports mining outposts")


## Applies rules for resource-rich systems without habitable worlds.
static func _apply_resource_system_rules(context: SystemContext, rec: PlacementRecommendation) -> void:
	rec.should_have_stations = true
	rec.reasoning.append("Resource-rich system without habitable worlds; stations become primary settlements")

	# Mining outposts
	rec.outpost_count = _calculate_mining_outposts(context)
	rec.recommended_purposes.append(StationPurpose.Purpose.MINING)

	# High resource systems can support larger populations
	if context.resource_richness >= HIGH_RESOURCE_THRESHOLD:
		rec.large_station_count = 1
		rec.recommended_purposes.append(StationPurpose.Purpose.RESIDENTIAL)
		rec.recommended_purposes.append(StationPurpose.Purpose.INDUSTRIAL)
		rec.reasoning.append("High resources can support colony-sized station")

	rec.allow_deep_space = true
	rec.allow_belt_stations = context.asteroid_belt_count > 0
	rec.orbital_candidates.append_array(context.resource_body_ids)


## Applies rules for strategic systems.
static func _apply_strategic_rules(context: SystemContext, rec: PlacementRecommendation) -> void:
	# Strategic systems may or may not have stations
	rec.should_have_stations = true
	rec.reasoning.append("Strategic location with habitable worlds")

	# Military/strategic outpost
	rec.outpost_count = 1
	rec.recommended_purposes.append(StationPurpose.Purpose.MILITARY)

	# If habitable, might have preliminary station
	if context.habitable_planet_count > 0:
		rec.orbital_candidates.append_array(context.planet_ids)

	rec.allow_deep_space = true


## Applies rules for systems with scientific interest.
static func _apply_scientific_rules(context: SystemContext, rec: PlacementRecommendation) -> void:
	rec.should_have_stations = true
	rec.reasoning.append("Non-spacefaring natives present; scientific observation stations")

	# Science outposts for observation
	rec.outpost_count = context.native_world_count
	rec.recommended_purposes.append(StationPurpose.Purpose.SCIENCE)

	# Orbital observation but at safe distance / hidden
	rec.orbital_candidates.append_array(context.native_planet_ids)
	rec.allow_deep_space = true


## Applies default rules for unremarkable systems.
static func _apply_default_rules(context: SystemContext, rec: PlacementRecommendation) -> void:
	# Most empty systems don't warrant stations
	rec.should_have_stations = false
	rec.reasoning.append("No significant reason for station presence")

	# Exception: some resource presence
	if context.resource_richness > 0.2:
		rec.should_have_stations = true
		rec.outpost_count = 1
		rec.recommended_purposes.append(StationPurpose.Purpose.MINING)
		rec.allow_belt_stations = context.asteroid_belt_count > 0
		rec.reasoning.append("Minor resources may support small mining outpost")


## Calculates recommended number of mining outposts based on resources.
static func _calculate_mining_outposts(context: SystemContext) -> int:
	var base_count: int = context.resource_body_ids.size()

	# Scale by resource richness
	if context.resource_richness >= HIGH_RESOURCE_THRESHOLD:
		base_count = maxi(base_count, 3)
	elif context.resource_richness >= RESOURCE_RICH_THRESHOLD:
		base_count = maxi(base_count, 2)
	else:
		base_count = maxi(base_count, 1)

	# Add for asteroid belts
	base_count += context.asteroid_belt_count

	return mini(base_count, 10) # Cap at reasonable number


## Checks if a planet should have orbital stations based on population.
## @param has_native: Whether the planet has native population.
## @param native_tech: Native tech level (null if no natives).
## @param has_colony: Whether the planet has a colony.
## @return: True if orbital stations are appropriate.
static func should_have_orbital_stations(
	has_native: bool,
	native_tech: Variant,
	has_colony: bool
) -> bool:
	# Colonies always warrant orbital stations
	if has_colony:
		return true

	# Natives need to be spacefaring
	if has_native and native_tech != null:
		var tech: TechnologyLevel.Level = native_tech as TechnologyLevel.Level
		return TechnologyLevel.can_spaceflight(tech)

	return false


## Estimates number of orbital stations for a populated world.
## @param population: Total population of the world.
## @param tech_level: Technology level of the population.
## @return: Recommended number of orbital stations.
static func estimate_orbital_station_count(
	population: int,
	tech_level: TechnologyLevel.Level
) -> int:
	if not TechnologyLevel.can_spaceflight(tech_level):
		return 0

	# Base: 1 station per 10 million population, minimum 1
	var base_count: int = maxi(1, int(population / 10_000_000.0))

	# Advanced tech supports more stations
	if tech_level >= TechnologyLevel.Level.INTERSTELLAR:
		base_count = int(base_count * 1.5)

	# Cap at reasonable number
	return mini(base_count, 20)


## Determines appropriate station class for a context.
## @param context: The placement context.
## @param has_large_population: Whether supporting population is large.
## @return: Recommended station class.
static func recommend_station_class(
	context: StationPlacementContext.Context,
	has_large_population: bool
) -> StationClass.Class:
	# Bridge systems favor small utility stations
	if context == StationPlacementContext.Context.BRIDGE_SYSTEM:
		return StationClass.Class.U

	# Scientific outposts are small
	if context == StationPlacementContext.Context.SCIENTIFIC:
		return StationClass.Class.O

	# Resource systems without population are outposts
	if context == StationPlacementContext.Context.RESOURCE_SYSTEM and not has_large_population:
		return StationClass.Class.O

	# Colony and native worlds with large populations can support bigger stations
	if has_large_population:
		if context == StationPlacementContext.Context.COLONY_WORLD:
			return StationClass.Class.A
		if context == StationPlacementContext.Context.NATIVE_WORLD:
			return StationClass.Class.A

	# Default to base class
	return StationClass.Class.B


## Recommends purposes for a station based on context.
## @param context: The placement context.
## @param is_utility: Whether this is a utility station.
## @return: Array of recommended purposes.
static func recommend_purposes(
	context: StationPlacementContext.Context,
	is_utility: bool
) -> Array[StationPurpose.Purpose]:
	if is_utility:
		return StationPurpose.typical_utility_purposes()

	match context:
		StationPlacementContext.Context.BRIDGE_SYSTEM:
			return [StationPurpose.Purpose.UTILITY, StationPurpose.Purpose.TRADE]
		StationPlacementContext.Context.COLONY_WORLD:
			return [StationPurpose.Purpose.TRADE, StationPurpose.Purpose.RESIDENTIAL, StationPurpose.Purpose.INDUSTRIAL]
		StationPlacementContext.Context.NATIVE_WORLD:
			return [StationPurpose.Purpose.TRADE, StationPurpose.Purpose.ADMINISTRATIVE]
		StationPlacementContext.Context.RESOURCE_SYSTEM:
			return [StationPurpose.Purpose.MINING, StationPurpose.Purpose.INDUSTRIAL]
		StationPlacementContext.Context.STRATEGIC:
			return [StationPurpose.Purpose.MILITARY]
		StationPlacementContext.Context.SCIENTIFIC:
			return [StationPurpose.Purpose.SCIENCE]
		_:
			return [StationPurpose.Purpose.UTILITY]


## Checks if a system qualifies as resource-rich.
## @param resource_abundance: Dictionary of ResourceType.Type -> abundance (0-1).
## @return: Resource richness score (0-1).
static func calculate_resource_richness(resource_abundance: Dictionary) -> float:
	if resource_abundance.is_empty():
		return 0.0

	var total: float = 0.0
	var count: int = 0

	# Weight valuable resources higher
	var weights: Dictionary = {
		ResourceType.Type.WATER: 1.5,
		ResourceType.Type.METALS: 1.2,
		ResourceType.Type.RARE_ELEMENTS: 2.0,
		ResourceType.Type.RADIOACTIVES: 1.8,
		ResourceType.Type.HYDROCARBONS: 1.3,
		ResourceType.Type.EXOTICS: 2.5,
	}

	for resource_type in resource_abundance.keys():
		var abundance: float = resource_abundance[resource_type] as float
		var weight: float = weights.get(resource_type, 1.0) as float
		total += abundance * weight
		count += 1

	if count == 0:
		return 0.0

	# Normalize: assume max weighted score would be ~2.0 per resource
	var normalized: float = total / (count * 2.0)
	return clampf(normalized, 0.0, 1.0)


## Creates a SystemContext from simplified system data.
## Convenience method to bridge the domain models.
## @param system_id: The system ID.
## @param planet_ids: Array of planet body IDs.
## @param habitable_count: Number of habitable planets.
## @param native_data: Array of {body_id, tech_level} for native populations.
## @param colony_ids: Array of body IDs with colonies.
## @param resource_richness: Overall resource score (0-1).
## @param belt_count: Number of asteroid belts.
## @param resource_body_ids: IDs of resource-rich bodies.
## @param is_bridge: Whether this is a bridge system.
## @return: A populated SystemContext.
static func create_system_context(
	system_id: String,
	planet_ids: Array[String],
	habitable_count: int,
	native_data: Array[Dictionary],
	colony_ids: Array[String],
	resource_richness: float,
	belt_count: int,
	resource_body_ids: Array[String],
	is_bridge: bool = false
) -> SystemContext:
	var ctx: SystemContext = SystemContext.new()
	ctx.system_id = system_id
	ctx.is_bridge_system = is_bridge
	ctx.planet_ids = planet_ids
	ctx.habitable_planet_count = habitable_count
	ctx.colony_world_count = colony_ids.size()
	ctx.colony_planet_ids = colony_ids
	ctx.resource_richness = resource_richness
	ctx.asteroid_belt_count = belt_count
	ctx.resource_body_ids = resource_body_ids

	# Process native data
	ctx.native_world_count = native_data.size()
	var highest_tech: int = -1
	for data in native_data:
		var body_id: String = data.get("body_id", "") as String
		if not body_id.is_empty():
			ctx.native_planet_ids.append(body_id)

		var tech: TechnologyLevel.Level = data.get("tech_level", TechnologyLevel.Level.STONE_AGE) as TechnologyLevel.Level
		if tech as int > highest_tech:
			highest_tech = tech as int
			ctx.highest_native_tech = tech

		if TechnologyLevel.can_spaceflight(tech):
			ctx.has_spacefaring_natives = true

	return ctx
