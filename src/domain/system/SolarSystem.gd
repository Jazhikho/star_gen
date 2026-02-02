## Main container for a complete solar system.
## Contains all celestial bodies, stellar hierarchy, asteroid belts, and metadata.
class_name SolarSystem
extends RefCounted

const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _asteroid_belt: GDScript = preload("res://src/domain/system/AsteroidBelt.gd")
const _celestial_serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")


## Unique identifier for this system.
var id: String

## Display name of the system.
var name: String

## Stellar hierarchy defining arrangement of stars.
var hierarchy: SystemHierarchy

## All celestial bodies indexed by ID for fast lookup.
var bodies: Dictionary  # String -> CelestialBody

## IDs of all star bodies.
var star_ids: Array[String]

## IDs of all planet bodies.
var planet_ids: Array[String]

## IDs of all moon bodies.
var moon_ids: Array[String]

## IDs of all asteroid bodies (including belt members).
var asteroid_ids: Array[String]

## All asteroid belts in the system.
var asteroid_belts: Array[AsteroidBelt]

## Computed orbit hosts (populated during/after generation).
var orbit_hosts: Array[OrbitHost]

## Generation provenance.
var provenance: Provenance


## Creates a new SolarSystem.
## @param p_id: Unique identifier.
## @param p_name: Display name.
func _init(
	p_id: String = "",
	p_name: String = ""
) -> void:
	id = p_id
	name = p_name
	hierarchy = SystemHierarchy.new()
	bodies = {}
	star_ids = []
	planet_ids = []
	moon_ids = []
	asteroid_ids = []
	asteroid_belts = []
	orbit_hosts = []
	provenance = null


## Adds a celestial body to the system.
## Automatically categorizes by type.
## @param body: The body to add.
func add_body(body: CelestialBody) -> void:
	if body == null or body.id.is_empty():
		return
	
	bodies[body.id] = body
	
	match body.type:
		CelestialType.Type.STAR:
			if not star_ids.has(body.id):
				star_ids.append(body.id)
		CelestialType.Type.PLANET:
			if not planet_ids.has(body.id):
				planet_ids.append(body.id)
		CelestialType.Type.MOON:
			if not moon_ids.has(body.id):
				moon_ids.append(body.id)
		CelestialType.Type.ASTEROID:
			if not asteroid_ids.has(body.id):
				asteroid_ids.append(body.id)


## Gets a body by ID.
## @param body_id: The body ID.
## @return: The body, or null if not found.
func get_body(body_id: String) -> CelestialBody:
	return bodies.get(body_id) as CelestialBody


## Gets all stars.
## @return: Array of star bodies.
func get_stars() -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	for star_id in star_ids:
		var body: CelestialBody = get_body(star_id)
		if body != null:
			result.append(body)
	return result


## Gets all planets.
## @return: Array of planet bodies.
func get_planets() -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	for planet_id in planet_ids:
		var body: CelestialBody = get_body(planet_id)
		if body != null:
			result.append(body)
	return result


## Gets all moons.
## @return: Array of moon bodies.
func get_moons() -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	for moon_id in moon_ids:
		var body: CelestialBody = get_body(moon_id)
		if body != null:
			result.append(body)
	return result


## Gets moons orbiting a specific planet.
## @param planet_id: The parent planet ID.
## @return: Array of moon bodies.
func get_moons_of_planet(planet_id: String) -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	for moon_id in moon_ids:
		var moon: CelestialBody = get_body(moon_id)
		if moon != null and moon.has_orbital():
			if moon.orbital.parent_id == planet_id:
				result.append(moon)
	return result


## Gets all asteroids.
## @return: Array of asteroid bodies.
func get_asteroids() -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	for asteroid_id in asteroid_ids:
		var body: CelestialBody = get_body(asteroid_id)
		if body != null:
			result.append(body)
	return result


## Gets the total body count.
## @return: Number of bodies.
func get_body_count() -> int:
	return bodies.size()


## Gets the star count.
## @return: Number of stars.
func get_star_count() -> int:
	return star_ids.size()


## Gets the planet count.
## @return: Number of planets.
func get_planet_count() -> int:
	return planet_ids.size()


## Gets the moon count.
## @return: Number of moons.
func get_moon_count() -> int:
	return moon_ids.size()


## Gets the asteroid count.
## @return: Number of asteroids.
func get_asteroid_count() -> int:
	return asteroid_ids.size()


## Adds an asteroid belt to the system.
## @param belt: The belt to add.
func add_asteroid_belt(belt: AsteroidBelt) -> void:
	if belt != null:
		asteroid_belts.append(belt)


## Adds an orbit host to the system.
## @param host: The orbit host to add.
func add_orbit_host(host: OrbitHost) -> void:
	if host != null:
		orbit_hosts.append(host)


## Gets an orbit host by node ID.
## @param node_id: The hierarchy node ID.
## @return: The orbit host, or null if not found.
func get_orbit_host(node_id: String) -> OrbitHost:
	for host in orbit_hosts:
		if host.node_id == node_id:
			return host
	return null


## Checks if the system has been initialized.
## @return: True if system has at least one star.
func is_valid() -> bool:
	return hierarchy.is_valid() and star_ids.size() > 0


## Returns a summary string for debugging.
## @return: Summary of system contents.
func get_summary() -> String:
	return "%s: %d stars, %d planets, %d moons, %d asteroids, %d belts" % [
		name if not name.is_empty() else id,
		get_star_count(),
		get_planet_count(),
		get_moon_count(),
		get_asteroid_count(),
		asteroid_belts.size()
	]


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = {
		"id": id,
		"name": name,
		"hierarchy": hierarchy.to_dict() if hierarchy != null else {},
		"star_ids": star_ids.duplicate(),
		"planet_ids": planet_ids.duplicate(),
		"moon_ids": moon_ids.duplicate(),
		"asteroid_ids": asteroid_ids.duplicate(),
	}
	
	# Serialize all bodies
	var bodies_dict: Dictionary = {}
	for body_id in bodies:
		var body: CelestialBody = bodies[body_id]
		bodies_dict[body_id] = CelestialSerializer.to_dict(body)
	data["bodies"] = bodies_dict
	
	# Serialize asteroid belts
	var belts_array: Array[Dictionary] = []
	for belt in asteroid_belts:
		belts_array.append(belt.to_dict())
	data["asteroid_belts"] = belts_array
	
	# Serialize orbit hosts
	var hosts_array: Array[Dictionary] = []
	for host in orbit_hosts:
		hosts_array.append(host.to_dict())
	data["orbit_hosts"] = hosts_array
	
	# Serialize provenance
	if provenance != null:
		data["provenance"] = provenance.to_dict()
	
	return data


## Creates a SolarSystem from a dictionary.
## @param data: Dictionary to parse.
## @return: A new SolarSystem.
static func from_dict(data: Dictionary) -> SolarSystem:
	var system: SolarSystem = SolarSystem.new(
		data.get("id", "") as String,
		data.get("name", "") as String
	)
	
	# Parse hierarchy
	if data.has("hierarchy"):
		system.hierarchy = SystemHierarchy.from_dict(data["hierarchy"] as Dictionary)
	
	# Parse bodies
	var bodies_dict: Dictionary = data.get("bodies", {}) as Dictionary
	for body_id in bodies_dict:
		var body: CelestialBody = CelestialSerializer.from_dict(bodies_dict[body_id] as Dictionary)
		if body != null:
			system.bodies[body_id] = body
	
	# Parse ID lists
	var star_list: Array = data.get("star_ids", []) as Array
	for sid in star_list:
		system.star_ids.append(sid as String)
	
	var planet_list: Array = data.get("planet_ids", []) as Array
	for pid in planet_list:
		system.planet_ids.append(pid as String)
	
	var moon_list: Array = data.get("moon_ids", []) as Array
	for mid in moon_list:
		system.moon_ids.append(mid as String)
	
	var asteroid_list: Array = data.get("asteroid_ids", []) as Array
	for aid in asteroid_list:
		system.asteroid_ids.append(aid as String)
	
	# Parse asteroid belts
	var belts_array: Array = data.get("asteroid_belts", []) as Array
	for belt_data in belts_array:
		var belt: AsteroidBelt = AsteroidBelt.from_dict(belt_data as Dictionary)
		system.asteroid_belts.append(belt)
	
	# Parse orbit hosts
	var hosts_array: Array = data.get("orbit_hosts", []) as Array
	for host_data in hosts_array:
		var host: OrbitHost = OrbitHost.from_dict(host_data as Dictionary)
		system.orbit_hosts.append(host)
	
	# Parse provenance
	if data.has("provenance"):
		system.provenance = Provenance.from_dict(data["provenance"] as Dictionary)
	
	return system
