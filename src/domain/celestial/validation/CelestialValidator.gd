## Validates celestial bodies and their components.
## Enforces physical constraints and data consistency.
class_name CelestialValidator
extends RefCounted


## Validates a complete celestial body.
## @param body: The celestial body to validate.
## @return: ValidationResult containing any errors/warnings.
static func validate(body: CelestialBody) -> ValidationResult:
	var result: ValidationResult = ValidationResult.new()
	
	_validate_identity(body, result)
	_validate_physical(body.physical, result)
	
	if body.has_stellar():
		_validate_stellar(body.stellar, body.type, result)
	
	if body.has_orbital():
		_validate_orbital(body.orbital, result)
	
	if body.has_surface():
		_validate_surface(body.surface, result)
	
	if body.has_atmosphere():
		_validate_atmosphere(body.atmosphere, result)
	
	if body.has_ring_system():
		_validate_ring_system(body.ring_system, body.physical, result)
	
	_validate_type_consistency(body, result)
	
	return result


## Validates body identity fields.
## @param body: The body to validate.
## @param result: The result to add errors to.
static func _validate_identity(body: CelestialBody, result: ValidationResult) -> void:
	if body.id.is_empty():
		result.add_error("id", "ID cannot be empty")
	
	if body.name.is_empty():
		result.add_warning("name", "Name is empty")


## Validates physical properties.
## @param physical: The physical props to validate.
## @param result: The result to add errors to.
static func _validate_physical(physical: PhysicalProps, result: ValidationResult) -> void:
	if physical == null:
		result.add_error("physical", "Physical properties are required")
		return
	
	if physical.mass_kg <= 0.0:
		result.add_error("physical.mass_kg", "Mass must be greater than zero")
	
	if physical.radius_m <= 0.0:
		result.add_error("physical.radius_m", "Radius must be greater than zero")
	
	if physical.axial_tilt_deg < 0.0 or physical.axial_tilt_deg > 180.0:
		result.add_warning("physical.axial_tilt_deg", "Axial tilt should be between 0 and 180 degrees")
	
	if physical.oblateness < 0.0 or physical.oblateness >= 1.0:
		result.add_error("physical.oblateness", "Oblateness must be between 0 and 1")
	
	if physical.internal_heat_watts < 0.0:
		result.add_error("physical.internal_heat_watts", "Internal heat cannot be negative")


## Validates stellar properties.
## @param stellar: The stellar props to validate.
## @param body_type: The type of celestial body.
## @param result: The result to add errors to.
static func _validate_stellar(
	stellar: StellarProps,
	body_type: CelestialType.Type,
	result: ValidationResult
) -> void:
	if body_type != CelestialType.Type.STAR:
		result.add_warning("stellar", "Non-star body has stellar properties")
	
	if stellar.luminosity_watts < 0.0:
		result.add_error("stellar.luminosity_watts", "Luminosity cannot be negative")
	
	if stellar.effective_temperature_k < 0.0:
		result.add_error("stellar.effective_temperature_k", "Effective temperature cannot be negative")
	
	if stellar.metallicity < 0.0:
		result.add_error("stellar.metallicity", "Metallicity cannot be negative")
	
	if stellar.age_years < 0.0:
		result.add_error("stellar.age_years", "Age cannot be negative")


## Validates orbital properties.
## @param orbital: The orbital props to validate.
## @param result: The result to add errors to.
static func _validate_orbital(orbital: OrbitalProps, result: ValidationResult) -> void:
	if orbital.semi_major_axis_m <= 0.0:
		result.add_error("orbital.semi_major_axis_m", "Semi-major axis must be greater than zero")
	
	if orbital.eccentricity < 0.0:
		result.add_error("orbital.eccentricity", "Eccentricity cannot be negative")
	
	if orbital.eccentricity >= 1.0:
		result.add_warning("orbital.eccentricity", "Eccentricity >= 1.0 indicates unbound orbit")
	
	if orbital.inclination_deg < 0.0 or orbital.inclination_deg > 180.0:
		result.add_warning("orbital.inclination_deg", "Inclination should be between 0 and 180 degrees")


## Validates surface properties including sub-components.
## @param surface: The surface props to validate.
## @param result: The result to add errors to.
static func _validate_surface(surface: SurfaceProps, result: ValidationResult) -> void:
	if surface.temperature_k < 0.0:
		result.add_error("surface.temperature_k", "Temperature cannot be negative")
	
	if surface.albedo < 0.0 or surface.albedo > 1.0:
		result.add_error("surface.albedo", "Albedo must be between 0 and 1")
	
	if surface.volcanism_level < 0.0 or surface.volcanism_level > 1.0:
		result.add_error("surface.volcanism_level", "Volcanism level must be between 0 and 1")
	
	if surface.has_terrain():
		_validate_terrain(surface.terrain, result)
	
	if surface.has_hydrosphere():
		_validate_hydrosphere(surface.hydrosphere, result)
	
	if surface.has_cryosphere():
		_validate_cryosphere(surface.cryosphere, result)


## Validates terrain properties.
## @param terrain: The terrain props to validate.
## @param result: The result to add errors to.
static func _validate_terrain(terrain: TerrainProps, result: ValidationResult) -> void:
	if terrain.elevation_range_m < 0.0:
		result.add_error("surface.terrain.elevation_range_m", "Elevation range cannot be negative")
	
	if terrain.roughness < 0.0 or terrain.roughness > 1.0:
		result.add_error("surface.terrain.roughness", "Roughness must be between 0 and 1")
	
	if terrain.crater_density < 0.0 or terrain.crater_density > 1.0:
		result.add_error("surface.terrain.crater_density", "Crater density must be between 0 and 1")
	
	if terrain.tectonic_activity < 0.0 or terrain.tectonic_activity > 1.0:
		result.add_error("surface.terrain.tectonic_activity", "Tectonic activity must be between 0 and 1")
	
	if terrain.erosion_level < 0.0 or terrain.erosion_level > 1.0:
		result.add_error("surface.terrain.erosion_level", "Erosion level must be between 0 and 1")


## Validates hydrosphere properties.
## @param hydrosphere: The hydrosphere props to validate.
## @param result: The result to add errors to.
static func _validate_hydrosphere(hydrosphere: HydrosphereProps, result: ValidationResult) -> void:
	if hydrosphere.ocean_coverage < 0.0 or hydrosphere.ocean_coverage > 1.0:
		result.add_error("surface.hydrosphere.ocean_coverage", "Ocean coverage must be between 0 and 1")
	
	if hydrosphere.ocean_depth_m < 0.0:
		result.add_error("surface.hydrosphere.ocean_depth_m", "Ocean depth cannot be negative")
	
	if hydrosphere.ice_coverage < 0.0 or hydrosphere.ice_coverage > 1.0:
		result.add_error("surface.hydrosphere.ice_coverage", "Ice coverage must be between 0 and 1")
	
	if hydrosphere.salinity_ppt < 0.0:
		result.add_error("surface.hydrosphere.salinity_ppt", "Salinity cannot be negative")


## Validates cryosphere properties.
## @param cryosphere: The cryosphere props to validate.
## @param result: The result to add errors to.
static func _validate_cryosphere(cryosphere: CryosphereProps, result: ValidationResult) -> void:
	if cryosphere.polar_cap_coverage < 0.0 or cryosphere.polar_cap_coverage > 1.0:
		result.add_error("surface.cryosphere.polar_cap_coverage", "Polar cap coverage must be between 0 and 1")
	
	if cryosphere.permafrost_depth_m < 0.0:
		result.add_error("surface.cryosphere.permafrost_depth_m", "Permafrost depth cannot be negative")
	
	if cryosphere.subsurface_ocean_depth_m < 0.0:
		result.add_error("surface.cryosphere.subsurface_ocean_depth_m", "Subsurface ocean depth cannot be negative")
	
	if cryosphere.cryovolcanism_level < 0.0 or cryosphere.cryovolcanism_level > 1.0:
		result.add_error("surface.cryosphere.cryovolcanism_level", "Cryovolcanism level must be between 0 and 1")


## Validates atmosphere properties.
## @param atmosphere: The atmosphere props to validate.
## @param result: The result to add errors to.
static func _validate_atmosphere(atmosphere: AtmosphereProps, result: ValidationResult) -> void:
	if atmosphere.surface_pressure_pa < 0.0:
		result.add_error("atmosphere.surface_pressure_pa", "Surface pressure cannot be negative")
	
	if atmosphere.scale_height_m < 0.0:
		result.add_error("atmosphere.scale_height_m", "Scale height cannot be negative")
	
	if atmosphere.greenhouse_factor < 0.0:
		result.add_error("atmosphere.greenhouse_factor", "Greenhouse factor cannot be negative")
	
	var comp_sum: float = atmosphere.get_composition_sum()
	if atmosphere.composition.size() > 0:
		if comp_sum < 0.99 or comp_sum > 1.01:
			result.add_warning(
				"atmosphere.composition",
				"Composition fractions should sum to 1.0 (got %.3f)" % comp_sum
			)


## Validates ring system properties.
## @param ring_system: The ring system props to validate.
## @param physical: The body's physical props for radius comparison.
## @param result: The result to add errors to.
static func _validate_ring_system(
	ring_system: RingSystemProps, 
	physical: PhysicalProps, 
	result: ValidationResult
) -> void:
	if ring_system.total_mass_kg < 0.0:
		result.add_error("ring_system.total_mass_kg", "Total mass cannot be negative")
	
	if ring_system.bands.is_empty():
		result.add_warning("ring_system.bands", "Ring system has no bands")
		return
	
	var prev_outer: float = 0.0
	for i in range(ring_system.bands.size()):
		var band: RingBand = ring_system.bands[i]
		var prefix: String = "ring_system.bands[%d]" % i
		
		if band.inner_radius_m <= 0.0:
			result.add_error(prefix + ".inner_radius_m", "Inner radius must be greater than zero")
		
		if band.outer_radius_m <= 0.0:
			result.add_error(prefix + ".outer_radius_m", "Outer radius must be greater than zero")
		
		if band.inner_radius_m >= band.outer_radius_m:
			result.add_error(prefix, "Inner radius must be less than outer radius")
		
		if band.optical_depth < 0.0:
			result.add_error(prefix + ".optical_depth", "Optical depth cannot be negative")
		
		if band.particle_size_m <= 0.0:
			result.add_error(prefix + ".particle_size_m", "Particle size must be greater than zero")
		
		if physical != null and physical.radius_m > 0.0:
			if band.inner_radius_m < physical.radius_m:
				result.add_error(
					prefix + ".inner_radius_m",
					"Ring inner radius cannot be less than body radius"
				)
		
		if i > 0 and band.inner_radius_m < prev_outer:
			result.add_warning(prefix, "Ring bands overlap")
		
		prev_outer = band.outer_radius_m


## Validates type-specific consistency.
## @param body: The body to validate.
## @param result: The result to add errors to.
static func _validate_type_consistency(body: CelestialBody, result: ValidationResult) -> void:
	match body.type:
		CelestialType.Type.STAR:
			if body.has_surface():
				result.add_warning("surface", "Stars typically don't have surface properties")
			if not body.has_stellar():
				result.add_warning("stellar", "Star should have stellar properties")
		
		CelestialType.Type.ASTEROID:
			if body.has_atmosphere():
				result.add_warning("atmosphere", "Asteroids rarely have atmospheres")
		
		CelestialType.Type.PLANET:
			if body.has_stellar():
				result.add_warning("stellar", "Planets should not have stellar properties")
		
		CelestialType.Type.MOON:
			if body.has_stellar():
				result.add_warning("stellar", "Moons should not have stellar properties")
