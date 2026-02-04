## Generates surface properties for moons.
## Handles surface type, terrain, and cryosphere (with subsurface ocean logic).
class_name MoonSurfaceGenerator
extends RefCounted

const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _surface_props: GDScript = preload("res://src/domain/celestial/components/SurfaceProps.gd")
const _terrain_props: GDScript = preload("res://src/domain/celestial/components/TerrainProps.gd")
const _cryosphere_props: GDScript = preload("res://src/domain/celestial/components/CryosphereProps.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")

## Water freezing point in Kelvin.
const WATER_FREEZE_K: float = 273.15


## Generates surface properties for the moon.
## @param spec: Moon specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param surface_temp_k: Surface temperature.
## @param tidal_heat_watts: Tidal heating contribution.
## @param _context: Parent context (reserved for future use).
## @param rng: Random number generator.
## @return: SurfaceProps.
static func generate_surface(
	spec: MoonSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	surface_temp_k: float,
	tidal_heat_watts: float,
	_context: ParentContext,
	rng: SeededRng
) -> SurfaceProps:
	# Determine albedo
	var albedo: float = _calculate_albedo(surface_temp_k, rng)
	
	# Determine surface type
	var surface_type: String = _determine_surface_type(surface_temp_k, tidal_heat_watts, rng)
	
	# Volcanism level (can be high for tidally heated moons)
	var volcanism_level: float = _calculate_volcanism(physical, tidal_heat_watts, rng)
	
	# Surface composition
	var surface_composition: Dictionary = _generate_surface_composition(surface_temp_k, rng)
	
	# Create surface props
	var surface: SurfaceProps = SurfaceProps.new(
		surface_temp_k,
		albedo,
		surface_type,
		volcanism_level,
		surface_composition
	)
	
	# Generate terrain
	surface.terrain = _generate_terrain(physical, size_cat, volcanism_level, rng)
	
	# Generate cryosphere for cold moons
	if surface_temp_k < WATER_FREEZE_K:
		surface.cryosphere = _generate_cryosphere(
			spec, surface_temp_k, physical, tidal_heat_watts, rng
		)
	
	return surface


## Calculates surface albedo for moons.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Albedo (0-1).
static func _calculate_albedo(surface_temp_k: float, rng: SeededRng) -> float:
	if surface_temp_k < 150.0:
		# Very cold icy moon: high albedo (Europa: 0.67, Enceladus: 0.99)
		return rng.randf_range(0.4, 0.95)
	elif surface_temp_k < WATER_FREEZE_K:
		# Cold: moderate to high albedo
		return rng.randf_range(0.2, 0.6)
	else:
		# Rocky: low to moderate
		return rng.randf_range(0.05, 0.3)


## Determines the surface type classification.
## @param surface_temp_k: Surface temperature.
## @param tidal_heat_watts: Tidal heating.
## @param rng: Random number generator.
## @return: Surface type string.
static func _determine_surface_type(
	surface_temp_k: float,
	tidal_heat_watts: float,
	rng: SeededRng
) -> String:
	# High tidal heating = volcanic
	if tidal_heat_watts > 1.0e13:
		return "volcanic"
	
	if surface_temp_k < 100.0:
		var roll: float = rng.randf()
		if roll < 0.6:
			return "icy"
		elif roll < 0.8:
			return "icy_cratered"
		else:
			return "icy_smooth"
	elif surface_temp_k < WATER_FREEZE_K:
		if rng.randf() < 0.5:
			return "icy_rocky"
		else:
			return "rocky_cold"
	else:
		return "rocky"


## Calculates volcanism level.
## @param physical: Physical properties.
## @param tidal_heat_watts: Tidal heating contribution.
## @param rng: Random number generator.
## @return: Volcanism level (0-1).
static func _calculate_volcanism(
	physical: PhysicalProps,
	tidal_heat_watts: float,
	rng: SeededRng
) -> float:
	# Tidal heating is primary driver for moon volcanism
	# Io: ~1e14 W, highly volcanic
	
	var total_heat: float = physical.internal_heat_watts + tidal_heat_watts
	var io_heat: float = 1.0e14
	
	var heat_ratio: float = total_heat / io_heat
	var base_volcanism: float = clampf(heat_ratio, 0.0, 1.0)
	
	# Variation
	var variation: float = rng.randf_range(0.7, 1.3)
	
	return clampf(base_volcanism * variation, 0.0, 1.0)


## Generates surface material composition.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_surface_composition(
	surface_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if surface_temp_k < 100.0:
		# Very cold: water ice and exotic ices
		composition["water_ice"] = rng.randf_range(0.5, 0.8)
		composition["silicates"] = rng.randf_range(0.1, 0.3)
		if rng.randf() < 0.5:
			composition["nitrogen_ice"] = rng.randf_range(0.01, 0.1)
		if rng.randf() < 0.3:
			composition["methane_ice"] = rng.randf_range(0.01, 0.05)
	elif surface_temp_k < WATER_FREEZE_K:
		# Cold: water ice and rock
		composition["water_ice"] = rng.randf_range(0.3, 0.6)
		composition["silicates"] = rng.randf_range(0.3, 0.5)
		composition["carbon_compounds"] = rng.randf_range(0.05, 0.15)
	else:
		# Rocky
		composition["silicates"] = rng.randf_range(0.5, 0.7)
		composition["iron_oxides"] = rng.randf_range(0.1, 0.3)
		composition["sulfur_compounds"] = rng.randf_range(0.05, 0.15)
	
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
## @param rng: Random number generator.
## @return: TerrainProps.
static func _generate_terrain(
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	volcanism_level: float,
	rng: SeededRng
) -> TerrainProps:
	var gravity: float = physical.get_surface_gravity_m_s2()
	var earth_gravity: float = 9.81
	
	# Elevation range scales inversely with gravity
	var gravity_factor: float = earth_gravity / maxf(gravity, 0.01)
	var base_elevation: float = 10000.0  # Moons have lower elevation range than planets
	var elevation_range_m: float = base_elevation * gravity_factor * rng.randf_range(0.2, 1.0)
	elevation_range_m = clampf(elevation_range_m, 500.0, 50000.0)
	
	# Roughness
	var roughness: float = rng.randf_range(0.3, 0.9)
	
	# Crater density - moons tend to be heavily cratered (no erosion)
	var crater_density: float
	if volcanism_level > 0.5:
		# Active volcanism resurfaces
		crater_density = rng.randf_range(0.0, 0.2)
	elif size_cat == SizeCategory.Category.DWARF:
		# Small moons: heavily cratered
		crater_density = rng.randf_range(0.7, 0.95)
	else:
		crater_density = rng.randf_range(0.4, 0.8)
	
	# Tectonic activity (low for most moons)
	var tectonic_activity: float = volcanism_level * rng.randf_range(0.5, 1.0)
	
	# Erosion (very low without atmosphere)
	var erosion_level: float = rng.randf_range(0.0, 0.1)
	
	# Terrain type
	var terrain_type: String
	if volcanism_level > 0.5:
		terrain_type = "volcanic"
	elif crater_density > 0.6:
		terrain_type = "cratered"
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


## Generates cryosphere properties for icy moons.
## @param spec: Moon specification.
## @param surface_temp_k: Surface temperature.
## @param physical: Physical properties.
## @param tidal_heat_watts: Tidal heating.
## @param rng: Random number generator.
## @return: CryosphereProps.
static func _generate_cryosphere(
	spec: MoonSpec,
	surface_temp_k: float,
	physical: PhysicalProps,
	tidal_heat_watts: float,
	rng: SeededRng
) -> CryosphereProps:
	# Polar cap coverage (most icy moons are entirely ice-covered)
	var polar_cap_coverage: float
	if surface_temp_k < 150.0:
		polar_cap_coverage = rng.randf_range(0.8, 1.0)
	else:
		polar_cap_coverage = rng.randf_range(0.3, 0.8)
	
	# Permafrost depth
	var permafrost_depth_m: float = rng.randf_range(1000.0, 50000.0)
	
	# Subsurface ocean - key feature for icy moons
	var has_subsurface_ocean: bool = false
	var subsurface_ocean_depth_m: float = 0.0
	
	# Check spec preference first
	if spec.has_ocean_preference():
		has_subsurface_ocean = spec.has_subsurface_ocean as bool
	else:
		# Subsurface ocean requires sufficient internal heat (tidal or radiogenic)
		var total_heat: float = physical.internal_heat_watts + tidal_heat_watts
		# Europa: ~1e12 W tidal + radiogenic
		if total_heat > 5.0e11:
			# High probability with significant heat
			has_subsurface_ocean = rng.randf() < 0.7
		elif total_heat > 1.0e11:
			# Moderate probability
			has_subsurface_ocean = rng.randf() < 0.3
	
	if has_subsurface_ocean:
		# Ocean depth: Europa ~100 km, Enceladus ~10 km
		subsurface_ocean_depth_m = rng.randf_range(5000.0, 150000.0)
	
	# Cryovolcanism (geysers, plumes)
	var cryovolcanism_level: float = 0.0
	if has_subsurface_ocean and tidal_heat_watts > 1.0e11:
		cryovolcanism_level = rng.randf_range(0.1, 0.8)
	
	# Ice type
	var ice_type: String = "water_ice"
	if surface_temp_k < 50.0:
		# Very cold: can have exotic ices
		var roll: float = rng.randf()
		if roll < 0.2:
			ice_type = "nitrogen_ice"
		elif roll < 0.3:
			ice_type = "methane_ice"
	
	return CryosphereProps.new(
		polar_cap_coverage,
		permafrost_depth_m,
		has_subsurface_ocean,
		subsurface_ocean_depth_m,
		cryovolcanism_level,
		ice_type
	)
