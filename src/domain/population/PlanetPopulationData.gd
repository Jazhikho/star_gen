## Container for all population-related data for a planet.
## This is the typed pipeline result for the population framework.
## Provides a clear API for generating and accessing population data.
class_name PlanetPopulationData
extends RefCounted

# Preload dependencies.
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _colony: GDScript = preload("res://src/domain/population/Colony.gd")


## ID of the celestial body this data is for.
var body_id: String = ""

## The planet's derived profile.
var profile: PlanetProfile = null

## Colony suitability assessment.
var suitability: ColonySuitability = null

## Native populations on this planet.
var native_populations: Array[NativePopulation] = []

## Colonies on this planet.
var colonies: Array[Colony] = []

## Seed used for generation (for reproducibility).
var generation_seed: int = 0

## Timestamp when this data was generated (0 = not set; set from serialization if needed).
## Not set in _init to keep pipeline output deterministic.
var generated_timestamp: int = 0


## Creates a new PlanetPopulationData.
func _init() -> void:
	pass


## Returns the total population across all natives and colonies.
## @return: Combined population count.
func get_total_population() -> int:
	var total: int = 0

	for native in native_populations:
		if native.is_extant:
			total += native.population

	for colony in colonies:
		if colony.is_active:
			total += colony.population

	return total


## Returns the total native population.
## @return: Combined native population count.
func get_native_population() -> int:
	var total: int = 0
	for native in native_populations:
		if native.is_extant:
			total += native.population
	return total


## Returns the total colony population.
## @return: Combined colony population count.
func get_colony_population() -> int:
	var total: int = 0
	for colony in colonies:
		if colony.is_active:
			total += colony.population
	return total


## Returns the dominant population (largest by population count).
## @return: The NativePopulation or Colony with highest population, or null if none.
func get_dominant_population() -> Variant:
	var dominant: Variant = null
	var max_pop: int = 0

	for native in native_populations:
		if native.is_extant and native.population > max_pop:
			max_pop = native.population
			dominant = native

	for colony in colonies:
		if colony.is_active and colony.population > max_pop:
			max_pop = colony.population
			dominant = colony

	return dominant


## Returns the dominant population's name.
## @return: Name string, or "Uninhabited" if no populations.
func get_dominant_population_name() -> String:
	var dominant: Variant = get_dominant_population()
	if dominant == null:
		return "Uninhabited"

	if dominant is NativePopulation:
		return (dominant as NativePopulation).name
	elif dominant is Colony:
		return (dominant as Colony).name

	return "Unknown"


## Returns whether the planet has any population.
## @return: True if any extant natives or active colonies.
func is_inhabited() -> bool:
	return get_total_population() > 0


## Returns whether the planet has native life.
## @return: True if any native populations exist (extant or extinct).
func has_natives() -> bool:
	return not native_populations.is_empty()


## Returns whether the planet has extant native life.
## @return: True if any native populations are still extant.
func has_extant_natives() -> bool:
	for native in native_populations:
		if native.is_extant:
			return true
	return false


## Returns whether the planet has been colonized.
## @return: True if any colonies exist (active or abandoned).
func has_colonies() -> bool:
	return not colonies.is_empty()


## Returns whether the planet has active colonies.
## @return: True if any colonies are still active.
func has_active_colonies() -> bool:
	for colony in colonies:
		if colony.is_active:
			return true
	return false


## Returns the number of extant native populations.
## @return: Count of extant natives.
func get_extant_native_count() -> int:
	var count: int = 0
	for native in native_populations:
		if native.is_extant:
			count += 1
	return count


## Returns the number of active colonies.
## @return: Count of active colonies.
func get_active_colony_count() -> int:
	var count: int = 0
	for colony in colonies:
		if colony.is_active:
			count += 1
	return count


## Returns all extant native populations.
## @return: Array of extant NativePopulation.
func get_extant_natives() -> Array[NativePopulation]:
	var result: Array[NativePopulation] = []
	for native in native_populations:
		if native.is_extant:
			result.append(native)
	return result


## Returns all active colonies.
## @return: Array of active Colony.
func get_active_colonies() -> Array[Colony]:
	var result: Array[Colony] = []
	for colony in colonies:
		if colony.is_active:
			result.append(colony)
	return result


## Returns whether there are any hostile native-colony relations.
## @return: True if any colony has hostile native relations.
func has_native_colony_conflict() -> bool:
	for colony in colonies:
		if colony.is_active and colony.has_hostile_native_relations():
			return true
	return false


## Returns the overall political situation.
## @return: "uninhabited", "native_only", "colony_only", "coexisting", or "conflict".
func get_political_situation() -> String:
	var has_active_natives: bool = has_extant_natives()
	var has_active_cols: bool = has_active_colonies()

	if not has_active_natives and not has_active_cols:
		return "uninhabited"
	elif has_active_natives and not has_active_cols:
		return "native_only"
	elif not has_active_natives and has_active_cols:
		return "colony_only"
	else:
		if has_native_colony_conflict():
			return "conflict"
		else:
			return "coexisting"


## Returns the highest technology level present on the planet.
## @return: TechnologyLevel.Level, or STONE_AGE if uninhabited.
func get_highest_tech_level() -> TechnologyLevel.Level:
	var highest: TechnologyLevel.Level = TechnologyLevel.Level.STONE_AGE
	var found: bool = false

	for native in native_populations:
		if native.is_extant:
			if not found or (native.tech_level as int) > (highest as int):
				highest = native.tech_level
				found = true

	for colony in colonies:
		if colony.is_active:
			if not found or (colony.tech_level as int) > (highest as int):
				highest = colony.tech_level
				found = true

	return highest


## Returns a native population by ID.
## @param id: The native population ID.
## @return: The NativePopulation, or null if not found.
func get_native_by_id(id: String) -> NativePopulation:
	for native in native_populations:
		if native.id == id:
			return native
	return null


## Returns a colony by ID.
## @param id: The colony ID.
## @return: The Colony, or null if not found.
func get_colony_by_id(id: String) -> Colony:
	for colony in colonies:
		if colony.id == id:
			return colony
	return null


## Returns a summary of this planet's population data.
## @return: Dictionary with key information.
func get_summary() -> Dictionary:
	return {
		"body_id": body_id,
		"total_population": get_total_population(),
		"native_population": get_native_population(),
		"colony_population": get_colony_population(),
		"extant_native_count": get_extant_native_count(),
		"active_colony_count": get_active_colony_count(),
		"dominant_population": get_dominant_population_name(),
		"political_situation": get_political_situation(),
		"highest_tech_level": TechnologyLevel.to_string_name(get_highest_tech_level()),
		"habitability_score": profile.habitability_score if profile else 0,
		"suitability_score": suitability.overall_score if suitability else 0,
	}


## Converts this data to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var natives_data: Array[Dictionary] = []
	for native in native_populations:
		natives_data.append(native.to_dict())

	var colonies_data: Array[Dictionary] = []
	for colony in colonies:
		colonies_data.append(colony.to_dict())

	var data: Dictionary = {
		"body_id": body_id,
		"generation_seed": generation_seed,
		"generated_timestamp": generated_timestamp,
		"native_populations": natives_data,
		"colonies": colonies_data,
	}

	if profile:
		data["profile"] = profile.to_dict()

	if suitability:
		data["suitability"] = suitability.to_dict()

	return data


## Creates a PlanetPopulationData from a dictionary.
## @param data: The dictionary to parse.
## @return: A new PlanetPopulationData instance.
static func from_dict(data: Dictionary) -> PlanetPopulationData:
	var pop_data: PlanetPopulationData = PlanetPopulationData.new()

	pop_data.body_id = data.get("body_id", "") as String
	pop_data.generation_seed = data.get("generation_seed", 0) as int
	pop_data.generated_timestamp = data.get("generated_timestamp", 0) as int

	if data.has("profile"):
		pop_data.profile = PlanetProfile.from_dict(data["profile"] as Dictionary)

	if data.has("suitability"):
		pop_data.suitability = ColonySuitability.from_dict(data["suitability"] as Dictionary)

	var natives_data: Array = data.get("native_populations", []) as Array
	for native_dict in natives_data:
		var native: NativePopulation = NativePopulation.from_dict(native_dict as Dictionary)
		pop_data.native_populations.append(native)

	var colonies_data: Array = data.get("colonies", []) as Array
	for colony_dict in colonies_data:
		var colony: Colony = Colony.from_dict(colony_dict as Dictionary)
		pop_data.colonies.append(colony)

	return pop_data
