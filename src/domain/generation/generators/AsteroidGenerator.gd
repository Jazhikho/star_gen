## Generates asteroid CelestialBody objects from AsteroidSpec.
## Produces deterministic output based on spec, parent context, and seed.
## Asteroids are simplified bodies with minimal/no atmosphere and basic terrain.
class_name AsteroidGenerator
extends RefCounted

const _asteroid_spec := preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _asteroid_type := preload("res://src/domain/generation/archetypes/AsteroidType.gd")
const _generator_utils := preload("res://src/domain/generation/generators/GeneratorUtils.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props := preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _surface_props := preload("res://src/domain/celestial/components/SurfaceProps.gd")
const _terrain_props := preload("res://src/domain/celestial/components/TerrainProps.gd")
const _provenance := preload("res://src/domain/celestial/Provenance.gd")
const _parent_context := preload("res://src/domain/generation/ParentContext.gd")
const _versions := preload("res://src/domain/constants/Versions.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Asteroid type distribution weights (C-type most common in outer belt).
const TYPE_WEIGHTS: Array[float] = [
	75.0,  # C_TYPE - most common (carbonaceous)
	17.0,  # S_TYPE - second most common (silicaceous)
	8.0,   # M_TYPE - rare (metallic)
]

## Mass ranges in kg for typical asteroids.
## Typical asteroids: ~1e10 to 1e18 kg (1 km to 100 km diameter rough equivalent)
const TYPICAL_MASS_MIN_KG: float = 1.0e10
const TYPICAL_MASS_MAX_KG: float = 1.0e18

## Mass ranges for large asteroids (Ceres-scale).
## Ceres: ~9.4e20 kg, Vesta: ~2.6e20 kg
const LARGE_MASS_MIN_KG: float = 1.0e19
const LARGE_MASS_MAX_KG: float = 1.0e21

## Density ranges by type in kg/m³.
const DENSITY_RANGES: Dictionary = {
	AsteroidType.Type.C_TYPE: {"min": 1100.0, "max": 2500.0},
	AsteroidType.Type.S_TYPE: {"min": 2200.0, "max": 3500.0},
	AsteroidType.Type.M_TYPE: {"min": 4500.0, "max": 7500.0},
}

## Albedo ranges by type.
const ALBEDO_RANGES: Dictionary = {
	AsteroidType.Type.C_TYPE: {"min": 0.03, "max": 0.10},
	AsteroidType.Type.S_TYPE: {"min": 0.10, "max": 0.30},
	AsteroidType.Type.M_TYPE: {"min": 0.10, "max": 0.25},
}

## Main belt orbital distance range in AU.
const MAIN_BELT_INNER_AU: float = 2.1
const MAIN_BELT_OUTER_AU: float = 3.3

## Typical asteroid belt eccentricity range.
const BELT_ECCENTRICITY_MIN: float = 0.0
const BELT_ECCENTRICITY_MAX: float = 0.3

## Typical asteroid belt inclination range in degrees.
const BELT_INCLINATION_MAX_DEG: float = 30.0


## Generates an asteroid from a specification and parent context.
## @param spec: The asteroid specification.
## @param context: The parent star context.
## @param rng: The random number generator (will be advanced).
## @return: A new CelestialBody configured as an asteroid.
static func generate(spec: AsteroidSpec, context: ParentContext, rng: SeededRng) -> CelestialBody:
	# Determine asteroid type
	var ast_type: AsteroidType.Type = _determine_asteroid_type(spec, rng)
	
	# Generate physical properties
	var physical: PhysicalProps = _generate_physical_props(spec, ast_type, rng)
	
	# Generate orbital properties
	var orbital: OrbitalProps = _generate_orbital_props(spec, context, rng)
	
	# Calculate equilibrium temperature
	var equilibrium_temp_k: float = context.get_equilibrium_temperature_k(
		AsteroidType.get_typical_albedo(ast_type)
	)
	
	# Generate surface properties (asteroids always have surfaces)
	var surface: SurfaceProps = _generate_surface(spec, physical, ast_type, equilibrium_temp_k, rng)
	
	# Generate ID
	var body_id: String = _generate_id(spec, rng)
	
	# Create provenance
	var provenance: Provenance = Provenance.new(
		spec.generation_seed,
		Versions.GENERATOR_VERSION,
		Versions.SCHEMA_VERSION,
		int(Time.get_unix_time_from_system()),
		spec.to_dict()
	)
	
	# Assemble the celestial body
	var body: CelestialBody = CelestialBody.new(
		body_id,
		spec.name_hint,
		CelestialType.Type.ASTEROID,
		physical,
		provenance
	)
	body.orbital = orbital
	body.surface = surface
	# Asteroids have no atmosphere (too small to retain gas)
	body.atmosphere = null
	
	return body


## Determines the asteroid type from spec or random selection.
## @param spec: The asteroid specification.
## @param rng: The random number generator.
## @return: The selected asteroid type.
static func _determine_asteroid_type(spec: AsteroidSpec, rng: SeededRng) -> AsteroidType.Type:
	if spec.has_asteroid_type():
		return spec.asteroid_type as AsteroidType.Type
	
	var types: Array = [
		AsteroidType.Type.C_TYPE,
		AsteroidType.Type.S_TYPE,
		AsteroidType.Type.M_TYPE,
	]
	
	var selected: Variant = rng.weighted_choice(types, TYPE_WEIGHTS)
	return selected as AsteroidType.Type


## Generates physical properties for the asteroid.
## @param spec: The asteroid specification.
## @param ast_type: The asteroid type.
## @param rng: The random number generator.
## @return: PhysicalProps for the asteroid.
static func _generate_physical_props(
	spec: AsteroidSpec,
	ast_type: AsteroidType.Type,
	rng: SeededRng
) -> PhysicalProps:
	# Get density range for this type
	var density_range: Dictionary = DENSITY_RANGES[ast_type]
	
	# Generate density
	var density_kg_m3: float = spec.get_override_float("physical.density_kg_m3", -1.0)
	if density_kg_m3 < 0.0:
		density_kg_m3 = rng.randf_range(density_range["min"], density_range["max"])
	
	# Generate mass based on size category
	var mass_kg: float = spec.get_override_float("physical.mass_kg", -1.0)
	if mass_kg < 0.0:
		if spec.is_large:
			# Log-uniform distribution for large asteroids
			var log_min: float = log(LARGE_MASS_MIN_KG)
			var log_max: float = log(LARGE_MASS_MAX_KG)
			mass_kg = exp(rng.randf_range(log_min, log_max))
		else:
			# Log-uniform distribution for typical asteroids
			var log_min: float = log(TYPICAL_MASS_MIN_KG)
			var log_max: float = log(TYPICAL_MASS_MAX_KG)
			mass_kg = exp(rng.randf_range(log_min, log_max))
	
	# Calculate radius from mass and density
	var radius_m: float = spec.get_override_float("physical.radius_m", -1.0)
	if radius_m < 0.0:
		var volume_m3: float = mass_kg / density_kg_m3
		radius_m = pow(volume_m3 * 3.0 / (4.0 * PI), 1.0 / 3.0)
	
	# Rotation period - asteroids can rotate quickly or tumble
	var rotation_period_s: float = spec.get_override_float("physical.rotation_period_s", -1.0)
	if rotation_period_s < 0.0:
		rotation_period_s = _generate_rotation_period(radius_m, rng)
	
	# Axial tilt - asteroids can have any orientation
	var axial_tilt_deg: float = spec.get_override_float("physical.axial_tilt_deg", -1.0)
	if axial_tilt_deg < 0.0:
		# Asteroids have essentially random orientations
		axial_tilt_deg = rng.randf_range(0.0, 180.0)
	
	# Oblateness - irregular shapes, approximate with oblateness
	var oblateness: float = spec.get_override_float("physical.oblateness", -1.0)
	if oblateness < 0.0:
		# Asteroids are often highly irregular
		# Small asteroids are more irregular, large ones more spherical
		if spec.is_large:
			oblateness = rng.randf_range(0.0, 0.1)
		else:
			oblateness = rng.randf_range(0.0, 0.4)
	
	# No significant magnetic field for asteroids
	var magnetic_moment: float = spec.get_override_float("physical.magnetic_moment", 0.0)
	
	# Minimal internal heat
	var internal_heat_watts: float = spec.get_override_float("physical.internal_heat_watts", -1.0)
	if internal_heat_watts < 0.0:
		# Only large asteroids have measurable internal heat
		if spec.is_large:
			internal_heat_watts = rng.randf_range(1.0e6, 1.0e10)
		else:
			internal_heat_watts = rng.randf_range(0.0, 1.0e6)
	
	return PhysicalProps.new(
		mass_kg,
		radius_m,
		rotation_period_s,
		axial_tilt_deg,
		oblateness,
		magnetic_moment,
		internal_heat_watts
	)


## Generates rotation period for an asteroid.
## Asteroids have a wide range of rotation periods.
## @param radius_m: Asteroid radius.
## @param rng: Random number generator.
## @return: Rotation period in seconds.
static func _generate_rotation_period(radius_m: float, rng: SeededRng) -> float:
	# Small asteroids can spin very fast (rubble pile limit ~2.2 hours)
	# Large asteroids spin slower
	# Typical range: 2 hours to 20 hours, some much longer (tumbling)
	
	var radius_km: float = radius_m / 1000.0
	
	var min_hours: float
	var max_hours: float
	
	if radius_km < 1.0:
		# Very small: can spin fast, but some tumble slowly
		min_hours = 0.1
		max_hours = 100.0
	elif radius_km < 10.0:
		# Small: typical fast rotators
		min_hours = 2.0
		max_hours = 24.0
	elif radius_km < 100.0:
		# Medium: moderate rotation
		min_hours = 4.0
		max_hours = 30.0
	else:
		# Large: slower rotation
		min_hours = 5.0
		max_hours = 20.0
	
	# Log-uniform for better distribution
	var log_min: float = log(min_hours)
	var log_max: float = log(max_hours)
	var period_hours: float = exp(rng.randf_range(log_min, log_max))
	
	# Small chance of retrograde rotation
	if rng.randf() < 0.3:
		period_hours = -period_hours
	
	return period_hours * 3600.0


## Generates orbital properties for the asteroid.
## @param spec: The asteroid specification.
## @param _context: The parent context (reserved for future use).
## @param rng: The random number generator.
## @return: OrbitalProps for the asteroid.
static func _generate_orbital_props(
	spec: AsteroidSpec,
	_context: ParentContext,
	rng: SeededRng
) -> OrbitalProps:
	# Semi-major axis (default to main belt if not specified)
	var semi_major_axis_m: float = spec.get_override_float("orbital.semi_major_axis_m", -1.0)
	if semi_major_axis_m < 0.0:
		# Main belt distribution - log-uniform within belt
		var inner_m: float = MAIN_BELT_INNER_AU * Units.AU_METERS
		var outer_m: float = MAIN_BELT_OUTER_AU * Units.AU_METERS
		var log_min: float = log(inner_m)
		var log_max: float = log(outer_m)
		semi_major_axis_m = exp(rng.randf_range(log_min, log_max))
	
	# Eccentricity - asteroids often have moderate eccentricity
	var eccentricity: float = spec.get_override_float("orbital.eccentricity", -1.0)
	if eccentricity < 0.0:
		# Bias toward lower eccentricities but allow moderate values
		var raw: float = rng.randf()
		eccentricity = raw * raw * BELT_ECCENTRICITY_MAX
	
	# Inclination - asteroids can have significant inclination
	var inclination_deg: float = spec.get_override_float("orbital.inclination_deg", -1.0)
	if inclination_deg < 0.0:
		# Rayleigh-like distribution for inclination
		var raw: float = rng.randf()
		inclination_deg = raw * raw * BELT_INCLINATION_MAX_DEG
	
	# Other orbital elements - random
	var longitude_of_ascending_node_deg: float = spec.get_override_float(
		"orbital.longitude_of_ascending_node_deg",
		rng.randf_range(0.0, 360.0)
	)
	
	var argument_of_periapsis_deg: float = spec.get_override_float(
		"orbital.argument_of_periapsis_deg",
		rng.randf_range(0.0, 360.0)
	)
	
	var mean_anomaly_deg: float = spec.get_override_float(
		"orbital.mean_anomaly_deg",
		rng.randf_range(0.0, 360.0)
	)
	
	return OrbitalProps.new(
		semi_major_axis_m,
		eccentricity,
		inclination_deg,
		longitude_of_ascending_node_deg,
		argument_of_periapsis_deg,
		mean_anomaly_deg,
		""  # Parent ID set later by system generator
	)


## Generates surface properties for the asteroid.
## @param spec: The asteroid specification.
## @param physical: Physical properties.
## @param ast_type: Asteroid type.
## @param equilibrium_temp_k: Equilibrium temperature.
## @param rng: Random number generator.
## @return: SurfaceProps.
static func _generate_surface(
	spec: AsteroidSpec,
	physical: PhysicalProps,
	ast_type: AsteroidType.Type,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> SurfaceProps:
	# Albedo based on type
	var albedo_range: Dictionary = ALBEDO_RANGES[ast_type]
	var albedo: float = spec.get_override_float("surface.albedo", -1.0)
	if albedo < 0.0:
		albedo = rng.randf_range(albedo_range["min"], albedo_range["max"])
	
	# Surface type based on composition
	var surface_type: String = _get_surface_type(ast_type)
	
	# No volcanism on asteroids (too small)
	var volcanism_level: float = 0.0
	
	# Surface composition based on type
	var surface_composition: Dictionary = _generate_surface_composition(ast_type, rng)
	
	# Create surface props
	var surface: SurfaceProps = SurfaceProps.new(
		equilibrium_temp_k,
		albedo,
		surface_type,
		volcanism_level,
		surface_composition
	)
	
	# Generate terrain
	surface.terrain = _generate_terrain(physical, spec.is_large, rng)
	
	return surface


## Gets the surface type string for an asteroid type.
## @param ast_type: The asteroid type.
## @return: Surface type string.
static func _get_surface_type(ast_type: AsteroidType.Type) -> String:
	match ast_type:
		AsteroidType.Type.C_TYPE:
			return "carbonaceous"
		AsteroidType.Type.S_TYPE:
			return "silicaceous"
		AsteroidType.Type.M_TYPE:
			return "metallic"
		_:
			return "rocky"


## Generates surface composition based on asteroid type.
## @param ast_type: The asteroid type.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_surface_composition(
	ast_type: AsteroidType.Type,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	match ast_type:
		AsteroidType.Type.C_TYPE:
			# Carbonaceous: carbon compounds, hydrated silicates, organics
			composition["carbon_compounds"] = rng.randf_range(0.15, 0.30)
			composition["hydrated_silicates"] = rng.randf_range(0.30, 0.50)
			composition["organics"] = rng.randf_range(0.05, 0.15)
			composition["water_ice"] = rng.randf_range(0.05, 0.20)
			composition["magnetite"] = rng.randf_range(0.05, 0.15)
		
		AsteroidType.Type.S_TYPE:
			# Silicaceous: silicates, pyroxene, olivine, nickel-iron
			composition["silicates"] = rng.randf_range(0.40, 0.60)
			composition["pyroxene"] = rng.randf_range(0.15, 0.25)
			composition["olivine"] = rng.randf_range(0.10, 0.20)
			composition["nickel_iron"] = rng.randf_range(0.05, 0.15)
		
		AsteroidType.Type.M_TYPE:
			# Metallic: iron-nickel, trace silicates
			composition["iron"] = rng.randf_range(0.70, 0.85)
			composition["nickel"] = rng.randf_range(0.10, 0.20)
			composition["cobalt"] = rng.randf_range(0.01, 0.05)
			composition["silicates"] = rng.randf_range(0.02, 0.10)
	
	# Normalize
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	if total > 0.0:
		for material in composition.keys():
			composition[material] = (composition[material] as float) / total
	
	return composition


## Generates terrain properties for the asteroid.
## @param physical: Physical properties.
## @param is_large: Whether this is a large asteroid.
## @param rng: Random number generator.
## @return: TerrainProps.
static func _generate_terrain(
	physical: PhysicalProps,
	is_large: bool,
	rng: SeededRng
) -> TerrainProps:
	var radius_m: float = physical.radius_m
	
	# Elevation range - relative to size
	# Asteroids can have significant relief relative to their size
	var elevation_fraction: float
	if is_large:
		# Large asteroids are more spherical
		elevation_fraction = rng.randf_range(0.01, 0.05)
	else:
		# Small asteroids can be very irregular
		elevation_fraction = rng.randf_range(0.05, 0.30)
	
	var elevation_range_m: float = radius_m * 2.0 * elevation_fraction
	elevation_range_m = maxf(elevation_range_m, 10.0)  # Minimum 10m
	
	# High roughness for asteroids
	var roughness: float = rng.randf_range(0.5, 1.0)
	
	# Crater density - asteroids are heavily cratered (no erosion)
	var crater_density: float = rng.randf_range(0.6, 0.95)
	
	# No tectonic activity
	var tectonic_activity: float = 0.0
	
	# No erosion (no atmosphere)
	var erosion_level: float = 0.0
	
	# Terrain type
	var terrain_type: String = "cratered"
	
	return TerrainProps.new(
		elevation_range_m,
		roughness,
		crater_density,
		tectonic_activity,
		erosion_level,
		terrain_type
	)


## Generates an ID for the asteroid.
## @param spec: The asteroid specification.
## @param rng: The random number generator.
## @return: The body ID.
static func _generate_id(spec: AsteroidSpec, rng: SeededRng) -> String:
	var override_id: Variant = spec.get_override("id", null)
	if override_id != null and override_id is String and not (override_id as String).is_empty():
		return override_id as String
	return GeneratorUtils.generate_id("asteroid", rng)
