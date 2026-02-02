## Generates surface properties for planets.
## Handles surface type, terrain, hydrosphere, and cryosphere.
class_name PlanetSurfaceGenerator
extends RefCounted

const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _surface_props: GDScript = preload("res://src/domain/celestial/components/SurfaceProps.gd")
const _terrain_props: GDScript = preload("res://src/domain/celestial/components/TerrainProps.gd")
const _hydrosphere_props: GDScript = preload("res://src/domain/celestial/components/HydrosphereProps.gd")
const _cryosphere_props: GDScript = preload("res://src/domain/celestial/components/CryosphereProps.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")

## Water freezing point in Kelvin.
const WATER_FREEZE_K: float = 273.15

## Water boiling point at 1 atm in Kelvin.
const WATER_BOIL_K: float = 373.15


## Generates surface properties for rocky planets.
## @param spec: Planet specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature (with greenhouse).
## @param context: Parent context.
## @param rng: Random number generator.
## @return: SurfaceProps or null.
static func generate_surface(
	spec: PlanetSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	context: ParentContext,
	rng: SeededRng
) -> SurfaceProps:
	# Determine albedo
	var albedo: float = _calculate_albedo(spec, zone, surface_temp_k, rng)
	
	# Determine surface type
	var surface_type: String = _determine_surface_type(size_cat, zone, surface_temp_k, rng)
	
	# Determine volcanism level
	var volcanism_level: float = _calculate_volcanism(
		spec, physical, context.stellar_age_years, rng
	)
	
	# Generate surface composition
	var surface_composition: Dictionary = _generate_surface_composition(
		size_cat, zone, surface_temp_k, rng
	)
	
	# Create surface props
	var surface: SurfaceProps = SurfaceProps.new(
		surface_temp_k,
		albedo,
		surface_type,
		volcanism_level,
		surface_composition
	)
	
	# Generate terrain
	surface.terrain = _generate_terrain(physical, size_cat, volcanism_level, context, rng)
	
	# Generate hydrosphere if conditions allow
	if _can_have_liquid_water(surface_temp_k, physical):
		surface.hydrosphere = _generate_hydrosphere(
			surface_temp_k, size_cat, volcanism_level, rng
		)
	
	# Generate cryosphere if cold enough
	if surface_temp_k < WATER_FREEZE_K or zone == OrbitZone.Zone.COLD:
		surface.cryosphere = _generate_cryosphere(
			surface_temp_k, size_cat, physical, context, rng
		)
	
	return surface


## Calculates surface albedo.
## @param spec: Planet specification.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Albedo (0-1).
static func _calculate_albedo(
	spec: PlanetSpec,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	rng: SeededRng
) -> float:
	var override_albedo: float = spec.get_override_float("surface.albedo", -1.0)
	if override_albedo >= 0.0:
		return clampf(override_albedo, 0.0, 1.0)
	
	# Base albedo depends on surface type
	var base_albedo: float
	
	if surface_temp_k < WATER_FREEZE_K:
		# Ice-covered: high albedo
		base_albedo = rng.randf_range(0.5, 0.8)
	elif zone == OrbitZone.Zone.HOT:
		# Hot rocky: low albedo
		base_albedo = rng.randf_range(0.05, 0.2)
	else:
		# Temperate: varies widely
		base_albedo = rng.randf_range(0.1, 0.5)
	
	return base_albedo


## Determines the surface type classification.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Surface type string.
static func _determine_surface_type(
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	rng: SeededRng
) -> String:
	# Determine primary surface characteristic
	if surface_temp_k > 700.0:
		return "molten"
	elif surface_temp_k > 500.0:
		return "volcanic"
	elif surface_temp_k < 100.0:
		return "frozen"
	elif surface_temp_k < WATER_FREEZE_K:
		if rng.randf() < 0.5:
			return "icy"
		else:
			return "rocky_cold"
	elif zone == OrbitZone.Zone.TEMPERATE:
		var roll: float = rng.randf()
		if roll < 0.2:
			return "oceanic"
		elif roll < 0.5:
			return "continental"
		elif roll < 0.7:
			return "desert"
		else:
			return "rocky"
	else:
		if size_cat == SizeCategory.Category.DWARF:
			return "cratered"
		return "rocky"


## Calculates volcanism level.
## @param spec: Planet specification.
## @param physical: Physical properties.
## @param age_years: System age.
## @param rng: Random number generator.
## @return: Volcanism level (0-1).
static func _calculate_volcanism(
	spec: PlanetSpec,
	physical: PhysicalProps,
	age_years: float,
	rng: SeededRng
) -> float:
	var override_volcanism: float = spec.get_override_float("surface.volcanism_level", -1.0)
	if override_volcanism >= 0.0:
		return clampf(override_volcanism, 0.0, 1.0)
	
	# Volcanism depends on internal heat
	var earth_heat: float = 4.7e13
	var heat_ratio: float = physical.internal_heat_watts / earth_heat
	
	# Scale with internal heat and add variation
	var base_volcanism: float = clampf(heat_ratio * 0.5, 0.0, 1.0)
	
	# Age factor: older planets are less active
	if age_years > 0.0:
		var age_factor: float = pow(0.5, age_years / 5.0e9)
		base_volcanism *= age_factor
	
	# Random variation
	var variation: float = rng.randf_range(0.5, 1.5)
	
	return clampf(base_volcanism * variation, 0.0, 1.0)


## Generates surface material composition.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_surface_composition(
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if surface_temp_k < WATER_FREEZE_K:
		# Cold: ice and rock
		composition["water_ice"] = rng.randf_range(0.2, 0.6)
		composition["silicates"] = rng.randf_range(0.3, 0.6)
		composition["carbon_compounds"] = rng.randf_range(0.05, 0.2)
	elif zone == OrbitZone.Zone.HOT:
		# Hot: silicates and metals
		composition["silicates"] = rng.randf_range(0.5, 0.7)
		composition["iron_oxides"] = rng.randf_range(0.1, 0.3)
		composition["sulfur_compounds"] = rng.randf_range(0.05, 0.15)
	else:
		# Temperate: varied
		composition["silicates"] = rng.randf_range(0.4, 0.6)
		composition["iron_oxides"] = rng.randf_range(0.1, 0.2)
		composition["carbonates"] = rng.randf_range(0.05, 0.15)
		if size_cat >= SizeCategory.Category.TERRESTRIAL:
			composition["water"] = rng.randf_range(0.01, 0.1)
	
	# Normalize
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	if total > 0.0:
		for material in composition.keys():
			composition[material] = (composition[material] as float) / total
	
	return composition


## Generates terrain properties.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param volcanism_level: Volcanism level.
## @param _context: Parent context (reserved for future use).
## @param rng: Random number generator.
## @return: TerrainProps.
static func _generate_terrain(
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	volcanism_level: float,
	_context: ParentContext,
	rng: SeededRng
) -> TerrainProps:
	var gravity: float = physical.get_surface_gravity_m_s2()
	var earth_gravity: float = 9.81
	
	# Elevation range scales inversely with gravity
	# Mars has taller mountains than Earth due to lower gravity
	var gravity_factor: float = earth_gravity / maxf(gravity, 0.1)
	var base_elevation: float = 20000.0  # Earth's max is ~20km from trench to peak
	var elevation_range_m: float = base_elevation * gravity_factor * rng.randf_range(0.3, 1.5)
	elevation_range_m = clampf(elevation_range_m, 1000.0, 100000.0)
	
	# Roughness
	var roughness: float = rng.randf_range(0.2, 0.8)
	
	# Crater density: higher for smaller, less active bodies
	var crater_density: float
	if size_cat == SizeCategory.Category.DWARF:
		crater_density = rng.randf_range(0.6, 0.95)
	elif volcanism_level > 0.3:
		# Active volcanism resurfaces, reducing craters
		crater_density = rng.randf_range(0.0, 0.3)
	else:
		crater_density = rng.randf_range(0.2, 0.7)
	
	# Tectonic activity correlates with volcanism and internal heat
	var tectonic_activity: float = clampf(volcanism_level * rng.randf_range(0.8, 1.2), 0.0, 1.0)
	
	# Erosion depends on atmosphere and water
	var erosion_level: float = rng.randf_range(0.1, 0.5)
	
	# Terrain type
	var terrain_type: String
	if volcanism_level > 0.5:
		terrain_type = "volcanic"
	elif crater_density > 0.6:
		terrain_type = "cratered"
	elif tectonic_activity > 0.4:
		terrain_type = "tectonic"
	else:
		terrain_type = "plains"
	
	return TerrainProps.new(
		elevation_range_m,
		roughness,
		crater_density,
		tectonic_activity,
		erosion_level,
		terrain_type
	)


## Checks if liquid water can exist on the surface.
## @param surface_temp_k: Surface temperature.
## @param physical: Physical properties.
## @return: True if liquid water possible.
static func _can_have_liquid_water(surface_temp_k: float, physical: PhysicalProps) -> bool:
	# Need temperature in liquid water range
	# Pressure affects boiling point, but simplified here
	if surface_temp_k < WATER_FREEZE_K:
		return false
	if surface_temp_k > WATER_BOIL_K * 1.5:  # Allow for high pressure
		return false
	
	# Need sufficient gravity to retain water vapor
	var escape_v: float = physical.get_escape_velocity_m_s()
	return escape_v > 3000.0  # Rough threshold


## Generates hydrosphere properties.
## @param surface_temp_k: Surface temperature.
## @param size_cat: Size category.
## @param volcanism_level: Volcanism level.
## @param rng: Random number generator.
## @return: HydrosphereProps.
static func _generate_hydrosphere(
	surface_temp_k: float,
	size_cat: SizeCategory.Category,
	volcanism_level: float,
	rng: SeededRng
) -> HydrosphereProps:
	# Ocean coverage
	var ocean_coverage: float
	if size_cat >= SizeCategory.Category.TERRESTRIAL:
		ocean_coverage = rng.randf_range(0.1, 0.95)
	else:
		ocean_coverage = rng.randf_range(0.0, 0.3)
	
	# Ocean depth scales with coverage and planet size
	var ocean_depth_m: float = ocean_coverage * rng.randf_range(1000.0, 10000.0)
	
	# Ice coverage based on temperature
	var ice_coverage: float = 0.0
	if surface_temp_k < WATER_FREEZE_K + 20.0:
		# Near freezing: partial ice
		ice_coverage = rng.randf_range(0.3, 0.8)
	elif surface_temp_k < WATER_FREEZE_K + 50.0:
		# Cool: polar ice
		ice_coverage = rng.randf_range(0.05, 0.3)
	
	# Salinity
	var salinity_ppt: float = rng.randf_range(10.0, 50.0)
	
	# Water type based on volcanism
	var water_type: String = "water"
	if volcanism_level > 0.5 and rng.randf() < 0.3:
		water_type = "acidic_water"
	
	return HydrosphereProps.new(
		ocean_coverage,
		ocean_depth_m,
		ice_coverage,
		salinity_ppt,
		water_type
	)


## Generates cryosphere properties.
## @param surface_temp_k: Surface temperature.
## @param _size_cat: Size category (reserved for future use).
## @param physical: Physical properties.
## @param _context: Parent context (reserved for future use).
## @param rng: Random number generator.
## @return: CryosphereProps.
static func _generate_cryosphere(
	surface_temp_k: float,
	_size_cat: SizeCategory.Category,
	physical: PhysicalProps,
	_context: ParentContext,
	rng: SeededRng
) -> CryosphereProps:
	# Polar cap coverage based on temperature
	var polar_cap_coverage: float = 0.0
	if surface_temp_k < WATER_FREEZE_K:
		polar_cap_coverage = rng.randf_range(0.3, 1.0)
	elif surface_temp_k < WATER_FREEZE_K + 30.0:
		polar_cap_coverage = rng.randf_range(0.05, 0.4)
	else:
		polar_cap_coverage = rng.randf_range(0.0, 0.1)
	
	# Permafrost depth
	var permafrost_depth_m: float = 0.0
	if surface_temp_k < WATER_FREEZE_K:
		permafrost_depth_m = rng.randf_range(100.0, 5000.0)
	elif polar_cap_coverage > 0.1:
		permafrost_depth_m = rng.randf_range(10.0, 500.0)
	
	# Subsurface ocean (more likely in cold moons with tidal heating)
	var has_subsurface_ocean: bool = false
	var subsurface_ocean_depth_m: float = 0.0
	
	# Check for subsurface ocean conditions
	if surface_temp_k < WATER_FREEZE_K and physical.internal_heat_watts > 1.0e12:
		if rng.randf() < 0.4:
			has_subsurface_ocean = true
			subsurface_ocean_depth_m = rng.randf_range(10000.0, 100000.0)
	
	# Cryovolcanism
	var cryovolcanism_level: float = 0.0
	if has_subsurface_ocean and physical.internal_heat_watts > 5.0e12:
		cryovolcanism_level = rng.randf_range(0.1, 0.6)
	
	# Ice type based on distance from star
	var ice_type: String = "water_ice"
	if surface_temp_k < 100.0:
		# Very cold: exotic ices
		var roll: float = rng.randf()
		if roll < 0.3:
			ice_type = "nitrogen_ice"
		elif roll < 0.5:
			ice_type = "methane_ice"
		elif roll < 0.7:
			ice_type = "co2_ice"
	
	return CryosphereProps.new(
		polar_cap_coverage,
		permafrost_depth_m,
		has_subsurface_ocean,
		subsurface_ocean_depth_m,
		cryovolcanism_level,
		ice_type
	)
