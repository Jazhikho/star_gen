## Defines a region of systems for jump lane calculation.
## Can represent a subsector or sector scope.
class_name JumpLaneRegion
extends RefCounted


## Region scope types.
enum RegionScope {
	SUBSECTOR, ## Single subsector
	SECTOR ## Full sector (multiple subsectors)
}


## The scope of this region.
var scope: RegionScope = RegionScope.SUBSECTOR

## Systems contained in this region.
var systems: Array[JumpLaneSystem] = []

## Optional identifier for the region (e.g. subsector/sector name).
var region_id: String = ""


## Creates a new JumpLaneRegion.
## @param p_scope: The region scope.
## @param p_region_id: Optional region identifier.
func _init(p_scope: RegionScope = RegionScope.SUBSECTOR, p_region_id: String = "") -> void:
	scope = p_scope
	region_id = p_region_id


## Adds a system to the region.
## @param system: The system to add.
func add_system(system: JumpLaneSystem) -> void:
	systems.append(system)


## Removes a system from the region by ID.
## @param system_id: The ID of the system to remove.
## @return: True if a system was removed.
func remove_system(system_id: String) -> bool:
	for i in range(systems.size() - 1, -1, -1):
		if systems[i].id == system_id:
			systems.remove_at(i)
			return true
	return false


## Gets a system by ID.
## @param system_id: The ID to search for.
## @return: The system, or null if not found.
func get_system(system_id: String) -> JumpLaneSystem:
	for system in systems:
		if system.id == system_id:
			return system
	return null


## Returns all populated systems in the region.
## @return: Array of populated systems.
func get_populated_systems() -> Array[JumpLaneSystem]:
	var result: Array[JumpLaneSystem] = []
	for system in systems:
		if system.is_populated():
			result.append(system)
	return result


## Returns all unpopulated systems in the region.
## @return: Array of unpopulated systems.
func get_unpopulated_systems() -> Array[JumpLaneSystem]:
	var result: Array[JumpLaneSystem] = []
	for system in systems:
		if not system.is_populated():
			result.append(system)
	return result


## Returns systems sorted by population (ascending).
## Unpopulated systems are excluded from this list.
## @return: Sorted array of populated systems.
func get_systems_sorted_by_population() -> Array[JumpLaneSystem]:
	var populated: Array[JumpLaneSystem] = get_populated_systems()
	populated.sort_custom(_compare_by_effective_population)
	return populated


## Comparison for sorting by effective population ascending.
func _compare_by_effective_population(a: JumpLaneSystem, b: JumpLaneSystem) -> bool:
	return a.get_effective_population() < b.get_effective_population()


## Returns the total number of systems.
## @return: System count.
func get_system_count() -> int:
	return systems.size()


## Returns the number of populated systems.
## @return: Populated system count.
func get_populated_count() -> int:
	return get_populated_systems().size()


## Clears all systems from the region.
func clear() -> void:
	systems.clear()


## Creates a dictionary representation for serialization.
## @return: Dictionary with region data.
func to_dict() -> Dictionary:
	var system_array: Array[Dictionary] = []
	for system in systems:
		system_array.append(system.to_dict())

	return {
		"scope": scope,
		"region_id": region_id,
		"systems": system_array
	}


## Creates a JumpLaneRegion from a dictionary.
## @param data: Dictionary with region data.
## @return: New JumpLaneRegion instance.
static func from_dict(data: Dictionary) -> RefCounted:
	var region: JumpLaneRegion = JumpLaneRegion.new(
		data.get("scope", RegionScope.SUBSECTOR) as RegionScope,
		data.get("region_id", "")
	)

	var systems_data: Array = data.get("systems", [])
	for system_data in systems_data:
		region.systems.append(JumpLaneSystem.from_dict(system_data as Dictionary))

	return region
