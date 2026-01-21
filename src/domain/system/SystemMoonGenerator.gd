## Generates moons for planets in a solar system.
## All planets can potentially have moons, with larger planets having more.
class_name SystemMoonGenerator
extends RefCounted

const _orbit_host := preload("res://src/domain/system/OrbitHost.gd")
const _orbital_mechanics := preload("res://src/domain/system/OrbitalMechanics.gd")
const _size_category := preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _moon_spec := preload("res://src/domain/generation/specs/MoonSpec.gd")
const _moon_generator := preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _parent_context := preload("res://src/domain/generation/ParentContext.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")


## Moon count ranges by planet mass category.
## Format: [min_moons, max_moons, probability_of_having_moons]
const MOON_COUNT_GAS_GIANT: Array = [2, 8, 0.95]      # Jupiter/Saturn-like
const MOON_COUNT_ICE_GIANT: Array = [1, 5, 0.90]      # Neptune/Uranus-like
const MOON_COUNT_SUPER_EARTH: Array = [0, 2, 0.40]    # Large rocky
const MOON_COUNT_TERRESTRIAL: Array = [0, 2, 0.30]    # Earth-like
const MOON_COUNT_SUB_TERRESTRIAL: Array = [0, 1, 0.15] # Mars-like
const MOON_COUNT_DWARF: Array = [0, 1, 0.05]          # Pluto-like

## Minimum Hill sphere fraction for moon orbits.
const MIN_HILL_FRACTION: float = 0.05

## Maximum Hill sphere fraction for regular moons.
const MAX_HILL_FRACTION_REGULAR: float = 0.40

## Maximum Hill sphere fraction for captured moons.
const MAX_HILL_FRACTION_CAPTURED: float = 0.60

## Probability that an outer moon is captured.
const CAPTURE_PROBABILITY: float = 0.30


## Result of moon generation for a system.
class MoonGenerationResult:
	extends RefCounted
	
	## Generated moons.
	var moons: Array[CelestialBody]
	
	## Mapping of planet ID to its moon IDs.
	var planet_moon_map: Dictionary  # String -> Array[String]
	
	## Whether generation succeeded.
	var success: bool
	
	## Error message if failed.
	var error_message: String
	
	func _init() -> void:
		moons = []
		planet_moon_map = {}
		success = false
		error_message = ""


## Generates moons for all planets in a system.
## @param planets: Array of planet bodies.
## @param _orbit_hosts: Array of orbit hosts (reserved for future use).
## @param stars: Array of star bodies.
## @param rng: Random number generator.
## @return: MoonGenerationResult with generated moons.
static func generate(
	planets: Array[CelestialBody],
	_orbit_hosts: Array[OrbitHost],
	stars: Array[CelestialBody],
	rng: SeededRng
) -> MoonGenerationResult:
	var result: MoonGenerationResult = MoonGenerationResult.new()
	
	# Get stellar properties for context
	var stellar_mass_kg: float = Units.SOLAR_MASS_KG
	var stellar_luminosity_watts: float = StellarProps.SOLAR_LUMINOSITY_WATTS
	var stellar_temperature_k: float = 5778.0
	var stellar_age_years: float = 4.6e9
	
	if stars.size() > 0:
		var primary_star: CelestialBody = stars[0]
		stellar_mass_kg = primary_star.physical.mass_kg
		if primary_star.has_stellar():
			stellar_luminosity_watts = primary_star.stellar.luminosity_watts
			stellar_temperature_k = primary_star.stellar.effective_temperature_k
			stellar_age_years = primary_star.stellar.age_years
	
	# Process each planet
	for planet in planets:
		var planet_moons: Array[CelestialBody] = _generate_moons_for_planet(
			planet,
			stellar_mass_kg,
			stellar_luminosity_watts,
			stellar_temperature_k,
			stellar_age_years,
			rng
		)
		
		if planet_moons.size() > 0:
			var moon_ids: Array[String] = []
			for moon in planet_moons:
				result.moons.append(moon)
				moon_ids.append(moon.id)
			result.planet_moon_map[planet.id] = moon_ids
	
	result.success = true
	return result


## Generates moons for a single planet.
## @param planet: The planet body.
## @param stellar_mass_kg: Mass of the star.
## @param stellar_luminosity_watts: Luminosity of the star.
## @param stellar_temperature_k: Temperature of the star.
## @param stellar_age_years: Age of the star/system.
## @param rng: Random number generator.
## @return: Array of generated moons.
static func _generate_moons_for_planet(
	planet: CelestialBody,
	stellar_mass_kg: float,
	stellar_luminosity_watts: float,
	stellar_temperature_k: float,
	stellar_age_years: float,
	rng: SeededRng
) -> Array[CelestialBody]:
	var moons: Array[CelestialBody] = []
	
	# Get planet's orbital distance from star
	var planet_orbital_distance_m: float = Units.AU_METERS
	if planet.has_orbital():
		planet_orbital_distance_m = planet.orbital.semi_major_axis_m
	
	# Determine moon count based on planet mass
	var moon_count: int = _determine_moon_count(planet, rng)
	if moon_count <= 0:
		return moons
	
	# Calculate Hill sphere
	var hill_radius_m: float = OrbitalMechanics.calculate_hill_sphere(
		planet.physical.mass_kg,
		stellar_mass_kg,
		planet_orbital_distance_m
	)
	
	if hill_radius_m <= planet.physical.radius_m * 3.0:
		# Hill sphere too small for stable moons
		return moons
	
	# Generate moon orbital distances
	var moon_distances: Array[float] = _generate_moon_distances(
		planet,
		hill_radius_m,
		moon_count,
		rng
	)
	
	# Generate each moon
	for i in range(moon_distances.size()):
		var moon_distance: float = moon_distances[i]
		var is_captured: bool = _should_be_captured(moon_distance, hill_radius_m, rng)
		
		var moon: CelestialBody = _generate_single_moon(
			planet,
			moon_distance,
			is_captured,
			stellar_mass_kg,
			stellar_luminosity_watts,
			stellar_temperature_k,
			stellar_age_years,
			planet_orbital_distance_m,
			i,
			rng
		)
		
		if moon != null:
			moons.append(moon)
	
	return moons


## Determines how many moons a planet should have.
## @param planet: The planet body.
## @param rng: Random number generator.
## @return: Number of moons.
static func _determine_moon_count(planet: CelestialBody, rng: SeededRng) -> int:
	var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
	
	var moon_params: Array
	
	if mass_earth >= 50.0:
		moon_params = MOON_COUNT_GAS_GIANT
	elif mass_earth >= 10.0:
		moon_params = MOON_COUNT_ICE_GIANT
	elif mass_earth >= 2.0:
		moon_params = MOON_COUNT_SUPER_EARTH
	elif mass_earth >= 0.3:
		moon_params = MOON_COUNT_TERRESTRIAL
	elif mass_earth >= 0.01:
		moon_params = MOON_COUNT_SUB_TERRESTRIAL
	else:
		moon_params = MOON_COUNT_DWARF
	
	# Check if planet has any moons
	var has_moons_probability: float = moon_params[2]
	if rng.randf() > has_moons_probability:
		return 0
	
	# Determine moon count
	var min_moons: int = moon_params[0]
	var max_moons: int = moon_params[1]
	
	if min_moons >= max_moons:
		return min_moons
	
	# Bias toward lower counts
	var raw: float = rng.randf()
	var biased: float = pow(raw, 0.7)  # Slight bias toward lower counts
	return int(lerpf(float(min_moons), float(max_moons) + 0.99, biased))


## Generates orbital distances for moons within Hill sphere.
## @param planet: The planet body.
## @param hill_radius_m: Hill sphere radius.
## @param count: Number of moons to place.
## @param rng: Random number generator.
## @return: Array of orbital distances in meters.
static func _generate_moon_distances(
	planet: CelestialBody,
	hill_radius_m: float,
	count: int,
	rng: SeededRng
) -> Array[float]:
	var distances: Array[float] = []
	
	# Minimum distance (outside Roche limit, at least 2x planet radius)
	var min_distance: float = planet.physical.radius_m * 3.0
	
	# Maximum distance for regular moons
	var max_distance: float = hill_radius_m * MAX_HILL_FRACTION_REGULAR
	
	if min_distance >= max_distance:
		return distances
	
	# Use log-uniform spacing
	var log_min: float = log(min_distance)
	var log_max: float = log(max_distance)
	var log_range: float = log_max - log_min
	
	# Generate distances with some spacing
	for i in range(count):
		var attempts: int = 0
		var distance: float = 0.0
		
		while attempts < 10:
			# Position within the range with some randomness
			var base_fraction: float = (float(i) + 0.5) / float(count)
			var jitter: float = rng.randf_range(-0.3, 0.3) / float(count)
			var fraction: float = clampf(base_fraction + jitter, 0.05, 0.95)
			
			var log_distance: float = log_min + fraction * log_range
			distance = exp(log_distance)
			
			# Check spacing from existing moons
			var valid: bool = true
			for existing in distances:
				var spacing_ratio: float = distance / existing if existing < distance else existing / distance
				if spacing_ratio < 1.3:  # Minimum 30% spacing
					valid = false
					break
			
			if valid:
				break
			
			attempts += 1
		
		if distance > min_distance:
			distances.append(distance)
	
	# Sort by distance
	distances.sort()
	
	return distances


## Determines if a moon should be captured based on distance.
## @param distance_m: Orbital distance.
## @param hill_radius_m: Hill sphere radius.
## @param rng: Random number generator.
## @return: True if moon should be captured.
static func _should_be_captured(distance_m: float, hill_radius_m: float, rng: SeededRng) -> bool:
	# Moons in outer Hill sphere are more likely captured
	var hill_fraction: float = distance_m / hill_radius_m
	
	if hill_fraction > MAX_HILL_FRACTION_REGULAR:
		return true  # Always captured in outer region
	
	if hill_fraction > 0.25:
		return rng.randf() < CAPTURE_PROBABILITY
	
	return false


## Generates a single moon.
## @param planet: Parent planet.
## @param moon_distance: Orbital distance from planet.
## @param is_captured: Whether this is a captured moon.
## @param stellar_mass_kg: Star mass.
## @param stellar_luminosity_watts: Star luminosity.
## @param stellar_temperature_k: Star temperature.
## @param stellar_age_years: System age.
## @param planet_orbital_distance_m: Planet's distance from star.
## @param moon_index: Index of this moon (for naming).
## @param rng: Random number generator.
## @return: Generated moon, or null if failed.
static func _generate_single_moon(
	planet: CelestialBody,
	moon_distance: float,
	is_captured: bool,
	stellar_mass_kg: float,
	stellar_luminosity_watts: float,
	stellar_temperature_k: float,
	stellar_age_years: float,
	planet_orbital_distance_m: float,
	moon_index: int,
	rng: SeededRng
) -> CelestialBody:
	# Create moon spec
	var moon_seed: int = rng.randi()
	var spec: MoonSpec = MoonSpec.new(moon_seed, -1, is_captured)
	
	# Determine moon size based on planet
	var planet_mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
	var size_category: SizeCategory.Category = _determine_moon_size(planet_mass_earth, is_captured, rng)
	spec.size_category = size_category
	
	# Override orbital distance
	spec.set_override("orbital.semi_major_axis_m", moon_distance)
	
	# Create parent context
	var context: ParentContext = ParentContext.for_moon(
		stellar_mass_kg,
		stellar_luminosity_watts,
		stellar_temperature_k,
		stellar_age_years,
		planet_orbital_distance_m,
		planet.physical.mass_kg,
		planet.physical.radius_m,
		moon_distance
	)
	
	# Generate moon
	var moon_rng: SeededRng = SeededRng.new(moon_seed)
	var moon: CelestialBody = MoonGenerator.generate(spec, context, moon_rng)
	
	if moon != null:
		# Assign ID and name
		moon.id = "moon_%s_%d" % [planet.id, moon_index]
		moon.name = _generate_moon_name(planet, moon_index, is_captured)
		
		# Set parent ID in orbital props
		if moon.has_orbital():
			moon.orbital.parent_id = planet.id
	
	return moon


## Determines moon size category based on parent planet.
## @param planet_mass_earth: Planet mass in Earth masses.
## @param is_captured: Whether moon is captured.
## @param rng: Random number generator.
## @return: Size category for moon.
static func _determine_moon_size(
	planet_mass_earth: float,
	is_captured: bool,
	rng: SeededRng
) -> SizeCategory.Category:
	# Captured moons are generally small
	if is_captured:
		var captured_weights: Array[float] = [80.0, 18.0, 2.0, 0.0]
		var categories: Array[int] = [
			SizeCategory.Category.DWARF,
			SizeCategory.Category.SUB_TERRESTRIAL,
			SizeCategory.Category.TERRESTRIAL,
			SizeCategory.Category.SUPER_EARTH,
		]
		return rng.weighted_choice(categories, captured_weights) as SizeCategory.Category
	
	# Regular moons scale with planet size
	if planet_mass_earth >= 50.0:
		# Gas giant moons (can have large moons like Ganymede, Titan)
		var weights: Array[float] = [30.0, 45.0, 20.0, 5.0]
		var categories: Array[int] = [
			SizeCategory.Category.DWARF,
			SizeCategory.Category.SUB_TERRESTRIAL,
			SizeCategory.Category.TERRESTRIAL,
			SizeCategory.Category.SUPER_EARTH,
		]
		return rng.weighted_choice(categories, weights) as SizeCategory.Category
	
	elif planet_mass_earth >= 10.0:
		# Ice giant moons
		var weights: Array[float] = [40.0, 45.0, 14.0, 1.0]
		var categories: Array[int] = [
			SizeCategory.Category.DWARF,
			SizeCategory.Category.SUB_TERRESTRIAL,
			SizeCategory.Category.TERRESTRIAL,
			SizeCategory.Category.SUPER_EARTH,
		]
		return rng.weighted_choice(categories, weights) as SizeCategory.Category
	
	elif planet_mass_earth >= 0.5:
		# Terrestrial planet moons (like Earth's Moon)
		var weights: Array[float] = [50.0, 45.0, 5.0, 0.0]
		var categories: Array[int] = [
			SizeCategory.Category.DWARF,
			SizeCategory.Category.SUB_TERRESTRIAL,
			SizeCategory.Category.TERRESTRIAL,
			SizeCategory.Category.SUPER_EARTH,
		]
		return rng.weighted_choice(categories, weights) as SizeCategory.Category
	
	else:
		# Small planet moons (like Mars's Phobos/Deimos)
		return SizeCategory.Category.DWARF


## Generates a name for a moon.
## @param planet: Parent planet.
## @param moon_index: Index of this moon.
## @param is_captured: Whether moon is captured.
## @return: Moon name.
static func _generate_moon_name(planet: CelestialBody, moon_index: int, is_captured: bool) -> String:
	var numerals: Array[String] = [
		"I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"
	]
	
	var numeral: String = numerals[moon_index] if moon_index < numerals.size() else str(moon_index + 1)
	
	var prefix: String = planet.name if not planet.name.is_empty() else planet.id
	
	if is_captured:
		return "%s %s (captured)" % [prefix, numeral]
	else:
		return "%s %s" % [prefix, numeral]


## Assigns Greek letter names to moons (Alpha, Beta, etc.).
## @param moons: Array of moons (should be sorted by distance).
## @param planet_name: Parent planet name.
static func assign_greek_letter_names(moons: Array[CelestialBody], planet_name: String = "") -> void:
	var letters: Array[String] = [
		"Alpha", "Beta", "Gamma", "Delta", "Epsilon",
		"Zeta", "Eta", "Theta", "Iota", "Kappa"
	]
	
	for i in range(moons.size()):
		var letter: String = letters[i] if i < letters.size() else str(i + 1)
		
		if planet_name.is_empty():
			moons[i].name = letter
		else:
			moons[i].name = "%s %s" % [planet_name, letter]


## Returns moons for a specific planet.
## @param moons: Array of all moons.
## @param planet_id: ID of the planet.
## @return: Array of moons orbiting the planet.
static func get_moons_for_planet(moons: Array[CelestialBody], planet_id: String) -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	
	for moon in moons:
		if moon.has_orbital() and moon.orbital.parent_id == planet_id:
			result.append(moon)
	
	return result


## Sorts moons by distance from planet (innermost first).
## @param moons: Array of moons to sort.
static func sort_by_distance(moons: Array[CelestialBody]) -> void:
	moons.sort_custom(func(a: CelestialBody, b: CelestialBody) -> bool:
		var a_has: bool = a.has_orbital()
		var b_has: bool = b.has_orbital()
		
		# Both have orbital data - compare normally
		if a_has and b_has:
			return a.orbital.semi_major_axis_m < b.orbital.semi_major_axis_m
		
		# Bodies with orbital data come before those without
		if a_has and not b_has:
			return true
		if not a_has and b_has:
			return false
		
		# Neither has orbital - they're equal, maintain stability
		return false
	)


## Calculates statistics about generated moons.
## @param moons: Array of moons.
## @return: Dictionary with statistics.
static func get_statistics(moons: Array[CelestialBody]) -> Dictionary:
	var stats: Dictionary = {
		"total": moons.size(),
		"captured": 0,
		"regular": 0,
		"has_atmosphere": 0,
		"has_subsurface_ocean": 0,
		"min_mass_earth": 0.0,
		"max_mass_earth": 0.0,
		"avg_mass_earth": 0.0,
	}
	
	if moons.is_empty():
		return stats
	
	var mass_sum: float = 0.0
	var min_mass: float = moons[0].physical.mass_kg
	var max_mass: float = moons[0].physical.mass_kg
	
	for moon in moons:
		var mass_earth: float = moon.physical.mass_kg / Units.EARTH_MASS_KG
		mass_sum += mass_earth
		min_mass = minf(min_mass, moon.physical.mass_kg)
		max_mass = maxf(max_mass, moon.physical.mass_kg)
		
		# Check if captured (from name convention)
		if moon.name.contains("captured"):
			stats["captured"] += 1
		else:
			stats["regular"] += 1
		
		if moon.has_atmosphere():
			stats["has_atmosphere"] += 1
		
		if moon.has_surface() and moon.surface.has_cryosphere():
			if moon.surface.cryosphere.has_subsurface_ocean:
				stats["has_subsurface_ocean"] += 1
	
	stats["min_mass_earth"] = min_mass / Units.EARTH_MASS_KG
	stats["max_mass_earth"] = max_mass / Units.EARTH_MASS_KG
	stats["avg_mass_earth"] = mass_sum / float(moons.size())
	
	return stats


## Validates moon orbital consistency.
## @param moons: Array of moons.
## @param planets: Array of planets.
## @return: True if all moons have valid parent references.
static func validate_moon_planet_consistency(
	moons: Array[CelestialBody],
	planets: Array[CelestialBody]
) -> bool:
	# Build planet ID set
	var planet_ids: Dictionary = {}
	for planet in planets:
		planet_ids[planet.id] = true
	
	# Check each moon
	for moon in moons:
		if not moon.has_orbital():
			return false
		
		if moon.orbital.parent_id.is_empty():
			return false
		
		if not planet_ids.has(moon.orbital.parent_id):
			return false
	
	return true
