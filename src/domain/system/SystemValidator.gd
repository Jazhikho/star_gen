## Validates complete solar systems.
## Checks hierarchy integrity, orbital stability, and body consistency.
class_name SystemValidator
extends RefCounted

const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _asteroid_belt: GDScript = preload("res://src/domain/system/AsteroidBelt.gd")
const _orbital_mechanics: GDScript = preload("res://src/domain/system/OrbitalMechanics.gd")
const _celestial_validator: GDScript = preload("res://src/domain/celestial/validation/CelestialValidator.gd")
const _validation_result: GDScript = preload("res://src/domain/celestial/validation/ValidationResult.gd")
const _validation_error: GDScript = preload("res://src/domain/celestial/validation/ValidationError.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Validates a complete solar system.
## @param system: The solar system to validate.
## @return: ValidationResult containing any errors/warnings.
static func validate(system: SolarSystem) -> ValidationResult:
	var result: ValidationResult = ValidationResult.new()
	
	_validate_identity(system, result)
	_validate_hierarchy(system, result)
	_validate_bodies(system, result)
	_validate_orbit_hosts(system, result)
	_validate_orbital_relationships(system, result)
	_validate_asteroid_belts(system, result)
	
	return result


## Validates system identity fields.
## @param system: The system to validate.
## @param result: The result to add errors to.
static func _validate_identity(system: SolarSystem, result: ValidationResult) -> void:
	if system.id.is_empty():
		result.add_error("id", "System ID cannot be empty")
	
	if system.name.is_empty():
		result.add_warning("name", "System name is empty")


## Validates the stellar hierarchy.
## @param system: The system to validate.
## @param result: The result to add errors to.
static func _validate_hierarchy(system: SolarSystem, result: ValidationResult) -> void:
	if system.hierarchy == null:
		result.add_error("hierarchy", "System hierarchy is null")
		return
	
	if not system.hierarchy.is_valid():
		result.add_error("hierarchy", "System hierarchy is invalid (no root)")
		return
	
	# Check hierarchy star count matches body star count
	var hierarchy_star_count: int = system.hierarchy.get_star_count()
	var body_star_count: int = system.star_ids.size()
	
	if hierarchy_star_count != body_star_count:
		result.add_error(
			"hierarchy",
			"Hierarchy star count (%d) doesn't match body star count (%d)" % [
				hierarchy_star_count,
				body_star_count
			]
		)
	
	# Verify all hierarchy star IDs reference actual bodies
	var hierarchy_star_ids: Array[String] = system.hierarchy.get_all_star_ids()
	for star_id in hierarchy_star_ids:
		if not system.bodies.has(star_id):
			result.add_error(
				"hierarchy",
				"Hierarchy references non-existent star: %s" % star_id
			)
	
	# Validate barycenter properties
	var barycenters: Array[HierarchyNode] = system.hierarchy.get_all_barycenters()
	for barycenter in barycenters:
		if barycenter.separation_m <= 0.0:
			result.add_error(
				"hierarchy.%s" % barycenter.id,
				"Barycenter separation must be positive"
			)
		
		if barycenter.eccentricity < 0.0 or barycenter.eccentricity >= 1.0:
			result.add_error(
				"hierarchy.%s" % barycenter.id,
				"Barycenter eccentricity must be in [0, 1)"
			)
		
		if barycenter.children.size() != 2:
			result.add_error(
				"hierarchy.%s" % barycenter.id,
				"Barycenter must have exactly 2 children"
			)


## Validates all celestial bodies.
## @param system: The system to validate.
## @param result: The result to add errors to.
static func _validate_bodies(system: SolarSystem, result: ValidationResult) -> void:
	if system.bodies.is_empty():
		result.add_error("bodies", "System has no bodies")
		return
	
	if system.star_ids.is_empty():
		result.add_error("stars", "System has no stars")
	
	# Validate each body individually
	for body_id in system.bodies:
		var body: CelestialBody = system.bodies[body_id] as CelestialBody
		var body_result: ValidationResult = CelestialValidator.validate(body)
		
		# Copy errors with prefixed path
		for error in body_result.errors:
			var prefixed_field: String = "bodies.%s.%s" % [body_id, error.field]
			if error.severity == ValidationError.Severity.ERROR:
				result.add_error(prefixed_field, error.message)
			else:
				result.add_warning(prefixed_field, error.message)
	
	# Check ID consistency
	for star_id in system.star_ids:
		if not system.bodies.has(star_id):
			result.add_error("star_ids", "Star ID references non-existent body: %s" % star_id)
		else:
			var body: CelestialBody = system.bodies[star_id]
			if body.type != CelestialType.Type.STAR:
				result.add_error("star_ids", "Star ID references non-star body: %s" % star_id)
	
	for planet_id in system.planet_ids:
		if not system.bodies.has(planet_id):
			result.add_error("planet_ids", "Planet ID references non-existent body: %s" % planet_id)
		else:
			var body: CelestialBody = system.bodies[planet_id]
			if body.type != CelestialType.Type.PLANET:
				result.add_error("planet_ids", "Planet ID references non-planet body: %s" % planet_id)
	
	for moon_id in system.moon_ids:
		if not system.bodies.has(moon_id):
			result.add_error("moon_ids", "Moon ID references non-existent body: %s" % moon_id)
		else:
			var body: CelestialBody = system.bodies[moon_id]
			if body.type != CelestialType.Type.MOON:
				result.add_error("moon_ids", "Moon ID references non-moon body: %s" % moon_id)
	
	for asteroid_id in system.asteroid_ids:
		if not system.bodies.has(asteroid_id):
			result.add_error("asteroid_ids", "Asteroid ID references non-existent body: %s" % asteroid_id)
		else:
			var body: CelestialBody = system.bodies[asteroid_id]
			if body.type != CelestialType.Type.ASTEROID:
				result.add_error("asteroid_ids", "Asteroid ID references non-asteroid body: %s" % asteroid_id)


## Validates orbit hosts.
## @param system: The system to validate.
## @param result: The result to add errors to.
static func _validate_orbit_hosts(system: SolarSystem, result: ValidationResult) -> void:
	for i in range(system.orbit_hosts.size()):
		var host: OrbitHost = system.orbit_hosts[i]
		var prefix: String = "orbit_hosts[%d]" % i
		
		if host.node_id.is_empty():
			result.add_error(prefix + ".node_id", "Orbit host node_id is empty")
		
		if host.combined_mass_kg <= 0.0:
			result.add_error(prefix + ".combined_mass_kg", "Orbit host mass must be positive")
		
		if host.inner_stability_m < 0.0:
			result.add_error(prefix + ".inner_stability_m", "Inner stability limit cannot be negative")
		
		if host.outer_stability_m <= host.inner_stability_m:
			result.add_warning(
				prefix,
				"Orbit host has no valid stable zone (outer <= inner)"
			)


## Validates orbital relationships (parent references, distances).
## @param system: The system to validate.
## @param result: The result to add errors to.
static func _validate_orbital_relationships(system: SolarSystem, result: ValidationResult) -> void:
	# Check moon-planet relationships
	for moon_id in system.moon_ids:
		var moon: CelestialBody = system.get_body(moon_id)
		if moon == null or not moon.has_orbital():
			continue
		
		var parent_id: String = moon.orbital.parent_id
		if parent_id.is_empty():
			result.add_warning(
				"bodies.%s.orbital.parent_id" % moon_id,
				"Moon has no parent ID"
			)
			continue
		
		var parent: CelestialBody = system.get_body(parent_id)
		if parent == null:
			result.add_error(
				"bodies.%s.orbital.parent_id" % moon_id,
				"Moon references non-existent parent: %s" % parent_id
			)
			continue
		
		if parent.type != CelestialType.Type.PLANET:
			result.add_warning(
				"bodies.%s.orbital.parent_id" % moon_id,
				"Moon parent is not a planet: %s" % parent_id
			)
	
	# Check for orbital distance overlaps within same host
	_check_orbital_overlaps(system, result)


## Checks for orbital overlaps between planets orbiting the same host.
## @param system: The system to validate.
## @param result: The result to add errors to.
static func _check_orbital_overlaps(system: SolarSystem, result: ValidationResult) -> void:
	# Group planets by their probable orbit host
	var planets: Array[CelestialBody] = system.get_planets()
	
	if planets.size() < 2:
		return
	
	# Sort by orbital distance
	var sorted_planets: Array[CelestialBody] = planets.duplicate()
	sorted_planets.sort_custom(func(a: CelestialBody, b: CelestialBody) -> bool:
		var dist_a: float = a.orbital.semi_major_axis_m if a.has_orbital() else 0.0
		var dist_b: float = b.orbital.semi_major_axis_m if b.has_orbital() else 0.0
		return dist_a < dist_b
	)
	
	# Check adjacent pairs for potential overlap
	for i in range(sorted_planets.size() - 1):
		var inner: CelestialBody = sorted_planets[i]
		var outer: CelestialBody = sorted_planets[i + 1]
		
		if not inner.has_orbital() or not outer.has_orbital():
			continue
		
		var inner_apo: float = inner.orbital.get_apoapsis_m()
		var outer_peri: float = outer.orbital.get_periapsis_m()
		
		if inner_apo >= outer_peri:
			result.add_warning(
				"orbital_overlap",
				"Potential orbital overlap between %s and %s" % [inner.id, outer.id]
			)


## Validates asteroid belts.
## @param system: The system to validate.
## @param result: The result to add errors to.
static func _validate_asteroid_belts(system: SolarSystem, result: ValidationResult) -> void:
	for i in range(system.asteroid_belts.size()):
		var belt: AsteroidBelt = system.asteroid_belts[i]
		var prefix: String = "asteroid_belts[%d]" % i
		
		if belt.id.is_empty():
			result.add_error(prefix + ".id", "Belt ID is empty")
		
		if belt.inner_radius_m <= 0.0:
			result.add_error(prefix + ".inner_radius_m", "Belt inner radius must be positive")
		
		if belt.outer_radius_m <= belt.inner_radius_m:
			result.add_error(prefix, "Belt outer radius must be greater than inner radius")
		
		if belt.total_mass_kg < 0.0:
			result.add_error(prefix + ".total_mass_kg", "Belt mass cannot be negative")
		
		# Check that referenced asteroids exist
		for asteroid_id in belt.major_asteroid_ids:
			if not system.bodies.has(asteroid_id):
				result.add_error(
					prefix + ".major_asteroid_ids",
					"Belt references non-existent asteroid: %s" % asteroid_id
				)


## Performs a quick validation check (fewer details, faster).
## @param system: The system to validate.
## @return: True if system passes basic validation.
static func is_valid(system: SolarSystem) -> bool:
	if system == null:
		return false
	
	if system.id.is_empty():
		return false
	
	if system.hierarchy == null or not system.hierarchy.is_valid():
		return false
	
	if system.star_ids.is_empty():
		return false
	
	# Check at least one star body exists
	for star_id in system.star_ids:
		if not system.bodies.has(star_id):
			return false
	
	return true
