## Generates ring systems for planets.
## Ring systems are constrained by the parent planet's Roche limit and Hill sphere.
class_name RingSystemGenerator
extends RefCounted

const _ring_system_spec: GDScript = preload("res://src/domain/generation/specs/RingSystemSpec.gd")
const _ring_complexity: GDScript = preload("res://src/domain/generation/archetypes/RingComplexity.gd")
const _ring_system_props: GDScript = preload("res://src/domain/celestial/components/RingSystemProps.gd")
const _ring_band: GDScript = preload("res://src/domain/celestial/components/RingBand.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Complexity level distribution weights.
const COMPLEXITY_WEIGHTS: Array[float] = [
	40.0,  # TRACE - most common (faint rings)
	35.0,  # SIMPLE - moderate
	25.0,  # COMPLEX - less common (Saturn-like)
]

## Ice line distance in AU (beyond this, rings are icy).
const ICE_LINE_AU: float = 2.7

## Typical ring particle density for mass calculation (kg/mÂ³).
const ICY_PARTICLE_DENSITY: float = 900.0
const ROCKY_PARTICLE_DENSITY: float = 2500.0

## Saturn's ring mass for reference (kg).
const SATURN_RING_MASS_KG: float = 1.5e19

## Minimum gap fraction between bands.
const MIN_GAP_FRACTION: float = 0.02

## Maximum gap fraction between bands.
const MAX_GAP_FRACTION: float = 0.15

## Resonance locations (fraction of outer ring radius where gaps tend to form).
## Based on orbital resonances with moons (simplified).
const RESONANCE_FRACTIONS: Array[float] = [
	0.48,  # 2:1 resonance
	0.63,  # 3:2 resonance
	0.76,  # 5:3 resonance
	0.87,  # 7:4 resonance
]


## Determines if a planet should have rings based on its properties.
## @param planet_physical: Physical properties of the planet.
## @param _context: Parent context with orbital information (reserved for future use).
## @param rng: Random number generator.
## @return: True if rings should be generated.
static func should_have_rings(
	planet_physical: PhysicalProps,
	_context: ParentContext,
	rng: SeededRng
) -> bool:
	var mass_earth: float = planet_physical.mass_kg / Units.EARTH_MASS_KG
	
	# Only larger planets tend to have rings
	# Gas giants: high probability
	# Super-Earths/Mini-Neptunes: low probability
	# Terrestrial and smaller: very rare
	
	var probability: float
	if mass_earth > 50.0:
		# Gas giant - high probability
		probability = 0.7
	elif mass_earth > 10.0:
		# Neptune-class - moderate probability
		probability = 0.4
	elif mass_earth > 5.0:
		# Mini-Neptune - low probability
		probability = 0.15
	elif mass_earth > 2.0:
		# Super-Earth - very low
		probability = 0.05
	else:
		# Terrestrial or smaller - extremely rare
		probability = 0.01
	
	return rng.randf() < probability


## Generates a ring system for a planet.
## @param spec: The ring system specification (can be null for defaults).
## @param planet_physical: Physical properties of the parent planet.
## @param context: Parent context for orbital/stellar information.
## @param rng: Random number generator.
## @return: RingSystemProps, or null if generation fails.
static func generate(
	spec: RingSystemSpec,
	planet_physical: PhysicalProps,
	context: ParentContext,
	rng: SeededRng
) -> RingSystemProps:
	# Use default spec if none provided
	var ring_spec: RingSystemSpec = spec
	if ring_spec == null:
		ring_spec = RingSystemSpec.random(rng.randi())
	
	# Determine complexity
	var complexity: RingComplexity.Level = _determine_complexity(ring_spec, rng)
	
	# Determine composition (icy vs rocky)
	var is_icy: bool = _determine_composition(ring_spec, context)
	
	# Calculate ring extent limits
	var ring_limits: Dictionary = _calculate_ring_limits(planet_physical, context)
	var inner_limit_m: float = ring_limits["inner"]
	var outer_limit_m: float = ring_limits["outer"]
	
	if inner_limit_m >= outer_limit_m:
		# Invalid ring region
		return null
	
	# Generate bands based on complexity
	var bands: Array[RingBand] = _generate_bands(
		complexity, inner_limit_m, outer_limit_m, is_icy, rng
	)
	
	if bands.is_empty():
		return null
	
	# Calculate total mass
	var total_mass_kg: float = _calculate_total_mass(bands, is_icy, rng)
	
	# Ring plane inclination (usually aligned with equator, small variation)
	var inclination_deg: float = ring_spec.get_override_float(
		"inclination_deg",
		rng.randf_range(0.0, 2.0)
	)
	
	return RingSystemProps.new(bands, total_mass_kg, inclination_deg)


## Determines the complexity level from spec or random selection.
## @param spec: The ring system specification.
## @param rng: Random number generator.
## @return: The selected complexity level.
static func _determine_complexity(spec: RingSystemSpec, rng: SeededRng) -> RingComplexity.Level:
	if spec.has_complexity():
		return spec.complexity as RingComplexity.Level
	
	var levels: Array = [
		RingComplexity.Level.TRACE,
		RingComplexity.Level.SIMPLE,
		RingComplexity.Level.COMPLEX,
	]
	
	var selected: Variant = rng.weighted_choice(levels, COMPLEXITY_WEIGHTS)
	return selected as RingComplexity.Level


## Determines if rings should be icy or rocky based on distance from star.
## @param spec: The ring system specification.
## @param context: Parent context.
## @return: True if icy, false if rocky.
static func _determine_composition(spec: RingSystemSpec, context: ParentContext) -> bool:
	if spec.has_composition_preference():
		return spec.is_icy as bool
	
	# Beyond the ice line, rings are predominantly icy
	var distance_au: float = context.orbital_distance_from_star_m / Units.AU_METERS
	
	# Adjust ice line based on stellar luminosity
	var luminosity_solar: float = context.stellar_luminosity_watts / 3.828e26
	var adjusted_ice_line: float = ICE_LINE_AU * sqrt(luminosity_solar)
	
	return distance_au > adjusted_ice_line


## Calculates the valid ring extent based on Roche limit and Hill sphere.
## @param planet_physical: Planet physical properties.
## @param context: Parent context.
## @return: Dictionary with "inner" and "outer" limits in meters.
static func _calculate_ring_limits(
	planet_physical: PhysicalProps,
	context: ParentContext
) -> Dictionary:
	var planet_radius_m: float = planet_physical.radius_m
	
	# Inner limit: just outside the planet (typically 1.1-1.5 planet radii)
	# Rings can't exist inside the planet!
	var inner_limit_m: float = planet_radius_m * 1.1
	
	# Calculate Roche limit for typical ring particle
	# Roche limit â‰ˆ 2.44 * R_planet * (Ï_planet / Ï_particle)^(1/3)
	var planet_density: float = planet_physical.get_density_kg_m3()
	var particle_density: float = ICY_PARTICLE_DENSITY  # Use icy as conservative estimate
	
	var roche_limit_m: float = 2.44 * planet_radius_m * pow(planet_density / particle_density, 1.0 / 3.0)
	
	# Outer limit: typically 2-3 Roche limits, but capped by Hill sphere
	# Most rings exist within ~2.5 Roche limits
	var outer_limit_m: float = roche_limit_m * 2.5
	
	# Cap at fraction of Hill sphere if context has parent body info
	if context.has_parent_body():
		var hill_radius: float = context.get_hill_sphere_radius_m()
		outer_limit_m = minf(outer_limit_m, hill_radius * 0.3)
	
	# Ensure inner limit is at least at Roche limit (rings form from disrupted material)
	inner_limit_m = maxf(inner_limit_m, roche_limit_m * 0.5)
	
	return {
		"inner": inner_limit_m,
		"outer": outer_limit_m,
		"roche": roche_limit_m
	}


## Generates ring bands based on complexity.
## @param complexity: The complexity level.
## @param inner_limit_m: Inner boundary in meters.
## @param outer_limit_m: Outer boundary in meters.
## @param is_icy: Whether composition is icy.
## @param rng: Random number generator.
## @return: Array of RingBand objects.
static func _generate_bands(
	complexity: RingComplexity.Level,
	inner_limit_m: float,
	outer_limit_m: float,
	is_icy: bool,
	rng: SeededRng
) -> Array[RingBand]:
	var bands: Array[RingBand] = []
	
	# Get band count range
	var count_range: Dictionary = RingComplexity.get_band_count_range(complexity)
	var band_count: int = rng.randi_range(count_range["min"], count_range["max"])
	
	# Get optical depth range
	var depth_range: Dictionary = RingComplexity.get_optical_depth_range(complexity)
	
	if band_count == 1:
		# Single band (trace rings)
		var band: RingBand = _create_band(
			inner_limit_m,
			outer_limit_m,
			depth_range,
			is_icy,
			"Main",
			rng
		)
		bands.append(band)
	else:
		# Multiple bands with gaps
		bands = _generate_multi_band_system(
			band_count,
			inner_limit_m,
			outer_limit_m,
			depth_range,
			is_icy,
			rng
		)
	
	return bands


## Generates a multi-band ring system with resonance-based gaps.
## @param band_count: Number of bands to generate.
## @param inner_limit_m: Inner boundary.
## @param outer_limit_m: Outer boundary.
## @param depth_range: Optical depth range.
## @param is_icy: Whether composition is icy.
## @param rng: Random number generator.
## @return: Array of RingBand objects.
static func _generate_multi_band_system(
	band_count: int,
	inner_limit_m: float,
	outer_limit_m: float,
	depth_range: Dictionary,
	is_icy: bool,
	rng: SeededRng
) -> Array[RingBand]:
	var bands: Array[RingBand] = []
	var total_width: float = outer_limit_m - inner_limit_m
	
	# Determine gap positions based on resonances (with some randomness)
	var gap_positions: Array[float] = []
	
	# Use resonance fractions as base, add randomness
	for res_frac in RESONANCE_FRACTIONS:
		if gap_positions.size() >= band_count - 1:
			break
		
		# Add some jitter to resonance position
		var jittered_frac: float = res_frac + rng.randf_range(-0.05, 0.05)
		jittered_frac = clampf(jittered_frac, 0.1, 0.9)
		
		# Convert to absolute position
		var gap_pos: float = inner_limit_m + total_width * jittered_frac
		
		# Check if gap is far enough from other gaps
		var too_close: bool = false
		for existing in gap_positions:
			if absf(gap_pos - existing) < total_width * 0.1:
				too_close = true
				break
		
		if not too_close:
			gap_positions.append(gap_pos)
	
	# Add random gaps if we need more
	while gap_positions.size() < band_count - 1:
		var gap_pos: float = rng.randf_range(
			inner_limit_m + total_width * 0.1,
			outer_limit_m - total_width * 0.1
		)
		
		var too_close: bool = false
		for existing in gap_positions:
			if absf(gap_pos - existing) < total_width * 0.08:
				too_close = true
				break
		
		if not too_close:
			gap_positions.append(gap_pos)
	
	# Sort gap positions
	gap_positions.sort()
	
	# Create bands between gaps
	var band_names: Array[String] = ["A", "B", "C", "D", "E", "F", "G"]
	var current_inner: float = inner_limit_m
	
	for i in range(band_count):
		var band_outer: float
		if i < gap_positions.size():
			# Gap width
			var gap_width: float = total_width * rng.randf_range(MIN_GAP_FRACTION, MAX_GAP_FRACTION)
			band_outer = gap_positions[i] - gap_width * 0.5
		else:
			band_outer = outer_limit_m
		
		# Ensure valid band
		if band_outer > current_inner + 1000.0:  # At least 1km wide
			var band_name: String = band_names[i] if i < band_names.size() else "Band_%d" % i
			var band: RingBand = _create_band(
				current_inner,
				band_outer,
				depth_range,
				is_icy,
				band_name,
				rng
			)
			bands.append(band)
		
		# Move to after the gap
		if i < gap_positions.size():
			var gap_width: float = total_width * rng.randf_range(MIN_GAP_FRACTION, MAX_GAP_FRACTION)
			current_inner = gap_positions[i] + gap_width * 0.5
		else:
			current_inner = band_outer
	
	return bands


## Creates a single ring band.
## @param inner_m: Inner radius.
## @param outer_m: Outer radius.
## @param depth_range: Optical depth range dictionary.
## @param is_icy: Whether icy composition.
## @param band_name: Name for the band.
## @param rng: Random number generator.
## @return: A new RingBand.
static func _create_band(
	inner_m: float,
	outer_m: float,
	depth_range: Dictionary,
	is_icy: bool,
	band_name: String,
	rng: SeededRng
) -> RingBand:
	# Optical depth
	var optical_depth: float = rng.randf_range(depth_range["min"], depth_range["max"])
	
	# Composition
	var composition: Dictionary = _generate_composition(is_icy, rng)
	
	# Particle size (log-uniform, typically cm to m scale)
	var log_size_min: float = log(0.001)  # 1mm
	var log_size_max: float = log(10.0)   # 10m
	var particle_size_m: float = exp(rng.randf_range(log_size_min, log_size_max))
	
	return RingBand.new(
		inner_m,
		outer_m,
		optical_depth,
		composition,
		particle_size_m,
		band_name
	)


## Generates composition dictionary for ring material.
## @param is_icy: Whether predominantly icy.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_composition(is_icy: bool, rng: SeededRng) -> Dictionary:
	var composition: Dictionary = {}
	
	if is_icy:
		# Icy rings (Saturn-like)
		composition["water_ice"] = rng.randf_range(0.85, 0.98)
		composition["silicates"] = rng.randf_range(0.01, 0.10)
		composition["carbon_compounds"] = rng.randf_range(0.001, 0.03)
	else:
		# Rocky/dusty rings
		composition["silicates"] = rng.randf_range(0.60, 0.80)
		composition["iron_oxides"] = rng.randf_range(0.10, 0.25)
		composition["carbon_compounds"] = rng.randf_range(0.05, 0.15)
	
	# Normalize
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	if total > 0.0:
		for material in composition.keys():
			composition[material] = (composition[material] as float) / total
	
	return composition


## Calculates the total mass of the ring system.
## @param bands: Array of ring bands.
## @param is_icy: Whether icy composition.
## @param rng: Random number generator.
## @return: Total mass in kg.
static func _calculate_total_mass(
	bands: Array[RingBand],
	is_icy: bool,
	rng: SeededRng
) -> float:
	# Ring mass is very difficult to determine from first principles
	# Use Saturn's rings as reference and scale
	
	# Total ring area
	var total_area_m2: float = 0.0
	var avg_optical_depth: float = 0.0
	
	for band in bands:
		var inner_r: float = band.inner_radius_m
		var outer_r: float = band.outer_radius_m
		var area: float = PI * (outer_r * outer_r - inner_r * inner_r)
		total_area_m2 += area
		avg_optical_depth += band.optical_depth
	
	if bands.size() > 0:
		avg_optical_depth /= bands.size()
	
	# Saturn's main rings: area â‰ˆ 1.5e17 mÂ², optical depth ~0.5, mass ~1.5e19 kg
	var saturn_area: float = 1.5e17
	var saturn_depth: float = 0.5
	
	var area_ratio: float = total_area_m2 / saturn_area
	var depth_ratio: float = avg_optical_depth / saturn_depth
	
	var base_mass: float = SATURN_RING_MASS_KG * area_ratio * depth_ratio
	
	# Adjust for composition (rocky is denser)
	if not is_icy:
		base_mass *= ROCKY_PARTICLE_DENSITY / ICY_PARTICLE_DENSITY
	
	# Add some variation
	var variation: float = rng.randf_range(0.5, 2.0)
	
	return base_mass * variation
