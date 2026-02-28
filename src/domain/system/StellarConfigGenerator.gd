## Generates stellar configurations for solar systems.
## Creates stars, builds hierarchies, and calculates orbit host stability zones.
class_name StellarConfigGenerator
extends RefCounted

const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _orbital_mechanics: GDScript = preload("res://src/domain/system/OrbitalMechanics.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _versions: GDScript = preload("res://src/domain/constants/Versions.gd")


## Binary separation category weights (close, moderate, wide).
const SEPARATION_WEIGHTS: Array[float] = [15.0, 50.0, 35.0]

## Close binary separation range in AU.
const CLOSE_BINARY_MIN_AU: float = 0.01
const CLOSE_BINARY_MAX_AU: float = 1.0

## Moderate binary separation range in AU.
const MODERATE_BINARY_MIN_AU: float = 1.0
const MODERATE_BINARY_MAX_AU: float = 50.0

## Wide binary separation range in AU.
const WIDE_BINARY_MIN_AU: float = 50.0
const WIDE_BINARY_MAX_AU: float = 10000.0

## Eccentricity ranges by separation category.
const CLOSE_ECC_MAX: float = 0.6
const MODERATE_ECC_MAX: float = 0.8
const WIDE_ECC_MAX: float = 0.9


## Generates a stellar configuration from a spec.
## @param spec: The system specification.
## @param rng: The random number generator.
## @return: SolarSystem with stars, hierarchy, and orbit hosts.
static func generate(spec: SolarSystemSpec, rng: SeededRng) -> SolarSystem:
	# Determine star count
	var star_count: int = _determine_star_count(spec, rng)
	if star_count < 1 or star_count > 10:
		push_error("Invalid star count: %d" % star_count)
		return null
	
	# Generate system container
	var system: SolarSystem = SolarSystem.new(
		_generate_system_id(rng),
		spec.name_hint if not spec.name_hint.is_empty() else "System-%d" % spec.generation_seed
	)
	
	# Generate all stars
	var stars: Array[CelestialBody] = _generate_stars(spec, star_count, rng)
	if stars.is_empty():
		push_error("Failed to generate stars")
		return null
	
	for star in stars:
		system.add_body(star)
	
	# Build hierarchy
	system.hierarchy = _build_hierarchy(stars, rng)
	if system.hierarchy == null or not system.hierarchy.is_valid():
		push_error("Failed to build hierarchy")
		return null
	
	# Calculate orbit hosts
	_calculate_orbit_hosts(system, stars, rng)
	
	# Create system provenance
	system.provenance = Provenance.new(
		spec.generation_seed,
		Versions.GENERATOR_VERSION,
		Versions.SCHEMA_VERSION,
		int(Time.get_unix_time_from_system()),
		spec.to_dict()
	)
	
	return system


## Determines the number of stars to generate.
## @param spec: The system specification.
## @param rng: The random number generator.
## @return: Star count.
static func _determine_star_count(spec: SolarSystemSpec, rng: SeededRng) -> int:
	# Check for override
	var override_count: Variant = spec.get_override("star_count", null)
	if override_count != null:
		return clampi(override_count as int, 1, 10)
	
	# If min equals max, use that
	if spec.star_count_min == spec.star_count_max:
		return spec.star_count_min
	
	# Weight toward lower star counts (single stars most common)
	# Use a distribution that favors 1, then 2, then rarely more
	var weights: Array[float] = []
	for i in range(spec.star_count_min, spec.star_count_max + 1):
		# Exponential decay: weight = 1 / (2^(n-1))
		weights.append(1.0 / pow(2.0, float(i - 1)))
	
	var options: Array[int] = []
	for i in range(spec.star_count_min, spec.star_count_max + 1):
		options.append(i)
	
	return rng.weighted_choice(options, weights) as int


## Generates all star bodies.
## @param spec: The system specification.
## @param count: Number of stars to generate.
## @param rng: The random number generator.
## @return: Array of star bodies.
static func _generate_stars(spec: SolarSystemSpec, count: int, rng: SeededRng) -> Array[CelestialBody]:
	var stars: Array[CelestialBody] = []
	
	for i in range(count):
		var star_seed: int = rng.randi()
		var star_rng: SeededRng = SeededRng.new(star_seed)
		
		# Create star spec
		var star_spec: StarSpec
		if i < spec.spectral_class_hints.size():
			star_spec = StarSpec.new(
				star_seed,
				spec.spectral_class_hints[i],
				-1, # Random subclass
				spec.system_metallicity,
				spec.system_age_years
			)
		else:
			star_spec = StarSpec.new(
				star_seed,
				-1, # Random spectral class
				-1, # Random subclass
				spec.system_metallicity,
				spec.system_age_years
			)
		
		# Generate star
		var star: CelestialBody = StarGenerator.generate(star_spec, star_rng)
		if star != null:
			# Ensure unique ID
			star.id = "star_%d" % i
			if star.name.is_empty():
				star.name = _generate_star_name(i, count)
			stars.append(star)
	
	return stars


## Generates a default star name based on position.
## @param index: Star index.
## @param total: Total star count.
## @return: Star name.
static func _generate_star_name(index: int, total: int) -> String:
	if total == 1:
		return "Primary"
	
	# Use Greek letters for multiple stars
	var letters: Array[String] = ["Alpha", "Beta", "Gamma", "Delta", "Epsilon",
								   "Zeta", "Eta", "Theta", "Iota", "Kappa"]
	if index < letters.size():
		return letters[index]
	return "Star %d" % (index + 1)


## Builds the stellar hierarchy from stars.
## @param stars: Array of star bodies.
## @param rng: The random number generator.
## @return: SystemHierarchy.
static func _build_hierarchy(stars: Array[CelestialBody], rng: SeededRng) -> SystemHierarchy:
	if stars.is_empty():
		return null
	
	# Create initial star nodes
	var nodes: Array[HierarchyNode] = []
	for i in range(stars.size()):
		var node: HierarchyNode = HierarchyNode.create_star(
			"node_star_%d" % i,
			stars[i].id
		)
		nodes.append(node)
	
	# Single star - simple case
	if nodes.size() == 1:
		return SystemHierarchy.new(nodes[0])
	
	# Multiple stars - build hierarchical structure
	var barycenter_index: int = 0
	
	while nodes.size() > 1:
		# Pick two nodes to combine
		var idx_a: int = rng.randi_range(0, nodes.size() - 1)
		var node_a: HierarchyNode = nodes[idx_a]
		nodes.remove_at(idx_a)
		
		var idx_b: int = rng.randi_range(0, nodes.size() - 1)
		var node_b: HierarchyNode = nodes[idx_b]
		nodes.remove_at(idx_b)
		
		# Generate binary parameters
		var separation_m: float = _generate_binary_separation(node_a, node_b, rng)
		var eccentricity: float = _generate_binary_eccentricity(separation_m, rng)
		
		# Calculate orbital period (need combined mass)
		var mass_a: float = _get_node_mass(node_a, stars)
		var mass_b: float = _get_node_mass(node_b, stars)
		var combined_mass: float = mass_a + mass_b
		var period_s: float = OrbitalMechanics.calculate_orbital_period(separation_m, combined_mass)
		
		# Create barycenter node
		var barycenter: HierarchyNode = HierarchyNode.create_barycenter(
			"node_barycenter_%d" % barycenter_index,
			node_a,
			node_b,
			separation_m,
			eccentricity
		)
		barycenter.orbital_period_s = period_s
		barycenter_index += 1
		
		nodes.append(barycenter)
	
	return SystemHierarchy.new(nodes[0])


## Gets the total mass of a hierarchy node.
## @param node: The hierarchy node.
## @param stars: Array of star bodies.
## @return: Total mass in kg.
static func _get_node_mass(node: HierarchyNode, stars: Array[CelestialBody]) -> float:
	if node.is_star():
		for star in stars:
			if star.id == node.star_id:
				return star.physical.mass_kg
		return Units.SOLAR_MASS_KG # Fallback
	else:
		var total: float = 0.0
		for child in node.children:
			total += _get_node_mass(child, stars)
		return total


## Gets the total luminosity of a hierarchy node.
## @param node: The hierarchy node.
## @param stars: Array of star bodies.
## @return: Total luminosity in watts.
static func _get_node_luminosity(node: HierarchyNode, stars: Array[CelestialBody]) -> float:
	if node.is_star():
		for star in stars:
			if star.id == node.star_id:
				if star.has_stellar():
					return star.stellar.luminosity_watts
		return 3.828e26 # Solar luminosity fallback
	else:
		var total: float = 0.0
		for child in node.children:
			total += _get_node_luminosity(child, stars)
		return total


## Generates binary separation.
## @param node_a: First node.
## @param node_b: Second node.
## @param rng: Random number generator.
## @return: Separation in meters.
static func _generate_binary_separation(
	node_a: HierarchyNode,
	node_b: HierarchyNode,
	rng: SeededRng
) -> float:
	# Choose separation category
	var categories: Array[int] = [0, 1, 2] # close, moderate, wide
	var category: int = rng.weighted_choice(categories, SEPARATION_WEIGHTS) as int
	
	var min_au: float
	var max_au: float
	
	match category:
		0: # Close
			min_au = CLOSE_BINARY_MIN_AU
			max_au = CLOSE_BINARY_MAX_AU
		1: # Moderate
			min_au = MODERATE_BINARY_MIN_AU
			max_au = MODERATE_BINARY_MAX_AU
		_: # Wide
			min_au = WIDE_BINARY_MIN_AU
			max_au = WIDE_BINARY_MAX_AU
	
	# If combining barycenters (hierarchical system), need wider separation
	if node_a.is_barycenter() or node_b.is_barycenter():
		# Outer pair should be at least 3x the inner separation
		var inner_sep_au: float = 0.0
		if node_a.is_barycenter():
			inner_sep_au = maxf(inner_sep_au, node_a.separation_m / Units.AU_METERS)
		if node_b.is_barycenter():
			inner_sep_au = maxf(inner_sep_au, node_b.separation_m / Units.AU_METERS)
		
		min_au = maxf(min_au, inner_sep_au * 3.0)
		max_au = maxf(max_au, min_au * 10.0)
	
	# Log-uniform distribution within range
	var log_min: float = log(min_au)
	var log_max: float = log(max_au)
	var log_sep: float = rng.randf_range(log_min, log_max)
	var sep_au: float = exp(log_sep)
	
	return sep_au * Units.AU_METERS


## Generates binary orbital eccentricity.
## @param separation_m: Binary separation in meters.
## @param rng: Random number generator.
## @return: Eccentricity (0 to <1).
static func _generate_binary_eccentricity(separation_m: float, rng: SeededRng) -> float:
	var sep_au: float = separation_m / Units.AU_METERS
	
	var max_ecc: float
	if sep_au < CLOSE_BINARY_MAX_AU:
		max_ecc = CLOSE_ECC_MAX
	elif sep_au < MODERATE_BINARY_MAX_AU:
		max_ecc = MODERATE_ECC_MAX
	else:
		max_ecc = WIDE_ECC_MAX
	
	# Bias toward lower eccentricities
	var raw: float = rng.randf()
	return raw * raw * max_ecc


## Calculates orbit hosts for all hierarchy nodes.
## @param system: The solar system.
## @param stars: Array of star bodies.
## @param rng: Random number generator.
static func _calculate_orbit_hosts(
	system: SolarSystem,
	stars: Array[CelestialBody],
	_rng: SeededRng
) -> void:
	# Get all nodes
	var all_nodes: Array[HierarchyNode] = system.hierarchy.get_all_nodes()
	
	for node in all_nodes:
		var host: OrbitHost = _create_orbit_host_for_node(node, stars, system.hierarchy)
		if host != null and host.has_valid_zone():
			system.add_orbit_host(host)


## Creates an orbit host for a hierarchy node.
## @param node: The hierarchy node.
## @param stars: Array of star bodies.
## @param hierarchy: The system hierarchy.
## @return: OrbitHost, or null if invalid.
static func _create_orbit_host_for_node(
	node: HierarchyNode,
	stars: Array[CelestialBody],
	hierarchy: SystemHierarchy
) -> OrbitHost:
	var host: OrbitHost
	
	if node.is_star():
		# S-type orbit around this star
		host = OrbitHost.new(node.id, OrbitHost.HostType.S_TYPE)
		
		# Find the star
		var star: CelestialBody = null
		for s in stars:
			if s.id == node.star_id:
				star = s
				break
		
		if star == null:
			return null
		
		host.combined_mass_kg = star.physical.mass_kg
		if star.has_stellar():
			host.combined_luminosity_watts = star.stellar.luminosity_watts
			host.effective_temperature_k = star.stellar.effective_temperature_k
		
		# Calculate stability limits
		# Inner limit: just outside the star
		host.inner_stability_m = star.physical.radius_m * 3.0
		
		# Outer limit depends on whether this star has companions
		var parent_barycenter: HierarchyNode = _find_parent_barycenter(node, hierarchy.root)
		if parent_barycenter != null:
			# Star is part of a binary - use S-type limit
			var sibling_mass: float = _get_sibling_mass(node, parent_barycenter, stars)
			var mass_ratio: float = sibling_mass / host.combined_mass_kg if host.combined_mass_kg > 0.0 else 1.0
			host.outer_stability_m = OrbitalMechanics.calculate_stype_stability_limit(
				parent_barycenter.separation_m,
				mass_ratio,
				parent_barycenter.eccentricity
			)
		else:
			# Single star: outer limit from formation (disc) and Jacobi (tidal) ceiling (see OrbitalMechanics).
			host.outer_stability_m = OrbitalMechanics.calculate_outer_stability_limit_m(
				host.combined_mass_kg,
				100.0
			)
	
	else:
		# P-type orbit around this barycenter
		host = OrbitHost.new(node.id, OrbitHost.HostType.P_TYPE)
		
		# Calculate combined properties
		host.combined_mass_kg = _get_node_mass(node, stars)
		host.combined_luminosity_watts = _get_node_luminosity(node, stars)
		
		# Luminosity-weighted effective temperature
		var total_lum: float = 0.0
		var weighted_temp: float = 0.0
		for star_id in node.get_all_star_ids():
			for star in stars:
				if star.id == star_id and star.has_stellar():
					var lum: float = star.stellar.luminosity_watts
					total_lum += lum
					weighted_temp += lum * star.stellar.effective_temperature_k
		if total_lum > 0.0:
			host.effective_temperature_k = weighted_temp / total_lum
		
		# Inner limit: P-type stability limit
		var child_mass_ratio: float = 1.0 # Assume equal for simplicity
		if node.children.size() >= 2:
			var mass_0: float = _get_node_mass(node.children[0], stars)
			var mass_1: float = _get_node_mass(node.children[1], stars)
			if mass_0 > 0.0:
				child_mass_ratio = mass_1 / mass_0
		
		host.inner_stability_m = OrbitalMechanics.calculate_ptype_stability_limit(
			node.separation_m,
			child_mass_ratio,
			node.eccentricity
		)
		
		# Outer limit: depends on parent barycenter or default
		var parent_barycenter: HierarchyNode = _find_parent_barycenter(node, hierarchy.root)
		if parent_barycenter != null:
			var sibling_mass: float = _get_sibling_mass(node, parent_barycenter, stars)
			var mass_ratio: float = sibling_mass / host.combined_mass_kg if host.combined_mass_kg > 0.0 else 1.0
			host.outer_stability_m = OrbitalMechanics.calculate_stype_stability_limit(
				parent_barycenter.separation_m,
				mass_ratio,
				parent_barycenter.eccentricity
			)
		else:
			# P-type (no parent): formation + Jacobi ceiling; larger base (200 AU at 1 M_sun) for circumbinary discs.
			host.outer_stability_m = OrbitalMechanics.calculate_outer_stability_limit_m(
				host.combined_mass_kg,
				200.0
			)
	
	# Calculate zones based on luminosity
	host.calculate_zones()
	
	return host


## Finds the parent barycenter of a node.
## @param target: The node to find parent for.
## @param current: Current node in search.
## @return: Parent barycenter, or null if not found.
static func _find_parent_barycenter(target: HierarchyNode, current: HierarchyNode) -> HierarchyNode:
	if current == null or current.is_star():
		return null
	
	# Check if target is a direct child
	for child in current.children:
		if child.id == target.id:
			return current
	
	# Recurse into children
	for child in current.children:
		var result: HierarchyNode = _find_parent_barycenter(target, child)
		if result != null:
			return result
	
	return null


## Gets the mass of the sibling node within a barycenter.
## @param node: The node whose sibling we want.
## @param parent: The parent barycenter.
## @param stars: Array of star bodies.
## @return: Sibling mass in kg.
static func _get_sibling_mass(
	node: HierarchyNode,
	parent: HierarchyNode,
	stars: Array[CelestialBody]
) -> float:
	for child in parent.children:
		if child.id != node.id:
			return _get_node_mass(child, stars)
	return Units.SOLAR_MASS_KG # Fallback


## Generates a unique system ID.
## @param rng: Random number generator.
## @return: System ID string.
static func _generate_system_id(rng: SeededRng) -> String:
	return "system_%d" % rng.randi()
