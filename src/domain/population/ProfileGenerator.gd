## Generates PlanetProfile from CelestialBody and parent context.
## Main entry point for creating population-relevant derived data.
class_name ProfileGenerator
extends RefCounted

# Ensure ProfileCalculations is in scope when this script compiles.
const _profile_calculations: GDScript = preload("res://src/domain/population/ProfileCalculations.gd")


## Generates a PlanetProfile from a CelestialBody.
## @param body: The celestial body to analyze (planet or moon).
## @param context: Parent context (star data for planets, planet data for moons).
## @param parent_body: Optional parent body for moons (to get physical properties).
## @return: A fully populated PlanetProfile.
static func generate(
	body: CelestialBody,
	context: ParentContext,
	parent_body: CelestialBody = null
) -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()

	# Basic identification
	profile.body_id = body.id
	profile.is_moon = (body.type == CelestialType.Type.MOON)

	# Extract physical properties
	_extract_physical_properties(profile, body)

	# Extract atmosphere properties
	_extract_atmosphere_properties(profile, body)

	# Extract surface properties
	_extract_surface_properties(profile, body)

	# Calculate derived properties
	_calculate_derived_properties(profile, body, context)

	# Calculate moon-specific properties
	if profile.is_moon and parent_body != null:
		_calculate_moon_properties(profile, body, parent_body, context)

	# Calculate complex derived data
	profile.climate_zones = ProfileCalculations.calculate_climate_zones(
		profile.axial_tilt_deg,
		profile.avg_temperature_k,
		profile.has_atmosphere
	)

	profile.biomes = ProfileCalculations.calculate_biomes(
		profile.climate_zones,
		profile.ocean_coverage,
		profile.ice_coverage,
		profile.volcanism_level,
		profile.has_liquid_water,
		profile.has_atmosphere
	)

	var surface_composition: Dictionary = {}
	if body.has_surface():
		surface_composition = body.surface.surface_composition

	profile.resources = ProfileCalculations.calculate_resources(
		surface_composition,
		profile.biomes,
		profile.volcanism_level,
		profile.has_liquid_water,
		profile.ocean_coverage
	)

	# Calculate continent count
	profile.continent_count = ProfileCalculations.estimate_continent_count(
		profile.tectonic_activity,
		profile.land_coverage,
		body.has_surface() and body.surface.has_terrain()
	)

	# Final habitability score
	profile.habitability_score = ProfileCalculations.calculate_habitability_score(
		profile.avg_temperature_k,
		profile.pressure_atm,
		profile.gravity_g,
		profile.has_liquid_water,
		profile.has_breathable_atmosphere,
		profile.radiation_level,
		profile.ocean_coverage
	)

	return profile


## Extracts physical properties from the body.
static func _extract_physical_properties(profile: PlanetProfile, body: CelestialBody) -> void:
	profile.gravity_g = body.physical.get_surface_gravity_m_s2() / ProfileCalculations.EARTH_GRAVITY
	profile.day_length_hours = absf(body.physical.rotation_period_s) / ProfileCalculations.SECONDS_PER_HOUR
	profile.axial_tilt_deg = body.physical.axial_tilt_deg

	# Magnetic field
	profile.magnetic_field_strength = ProfileCalculations.calculate_magnetic_strength(
		body.physical.magnetic_moment
	)
	profile.has_magnetic_field = profile.magnetic_field_strength > 0.1


## Extracts atmosphere properties from the body.
static func _extract_atmosphere_properties(profile: PlanetProfile, body: CelestialBody) -> void:
	if not body.has_atmosphere():
		profile.has_atmosphere = false
		profile.pressure_atm = 0.0
		profile.greenhouse_factor = 1.0
		profile.has_breathable_atmosphere = false
		return

	profile.pressure_atm = body.atmosphere.surface_pressure_pa / ProfileCalculations.EARTH_PRESSURE_PA
	profile.has_atmosphere = profile.pressure_atm > ProfileCalculations.MIN_ATMOSPHERE_ATM
	profile.greenhouse_factor = body.atmosphere.greenhouse_factor

	profile.has_breathable_atmosphere = ProfileCalculations.check_breathability(
		body.atmosphere.composition,
		profile.pressure_atm
	)


## Extracts surface properties from the body.
static func _extract_surface_properties(profile: PlanetProfile, body: CelestialBody) -> void:
	if not body.has_surface():
		profile.avg_temperature_k = 0.0
		profile.albedo = 0.0
		profile.volcanism_level = 0.0
		profile.ocean_coverage = 0.0
		profile.land_coverage = 0.0
		profile.ice_coverage = 0.0
		profile.max_elevation_km = 0.0
		profile.tectonic_activity = 0.0
		profile.has_liquid_water = false
		return

	profile.avg_temperature_k = body.surface.temperature_k
	profile.albedo = body.surface.albedo
	profile.volcanism_level = body.surface.volcanism_level

	# Hydrosphere
	if body.surface.has_hydrosphere():
		profile.ocean_coverage = body.surface.hydrosphere.ocean_coverage
		profile.has_liquid_water = profile.ocean_coverage > 0.0
	else:
		profile.ocean_coverage = 0.0
		profile.has_liquid_water = false

	# Cryosphere
	if body.surface.has_cryosphere():
		profile.ice_coverage = body.surface.cryosphere.polar_cap_coverage
		# Subsurface oceans count as liquid water for habitability
		if body.surface.cryosphere.has_subsurface_ocean:
			profile.has_liquid_water = true
	else:
		profile.ice_coverage = 0.0

	# Land coverage
	profile.land_coverage = clampf(1.0 - profile.ocean_coverage - profile.ice_coverage, 0.0, 1.0)

	# Terrain
	if body.surface.has_terrain():
		profile.max_elevation_km = body.surface.terrain.elevation_range_m / 1000.0
		profile.tectonic_activity = body.surface.terrain.tectonic_activity
	else:
		profile.max_elevation_km = 0.0
		profile.tectonic_activity = 0.0


## Calculates derived properties from extracted data.
static func _calculate_derived_properties(
	profile: PlanetProfile,
	body: CelestialBody,
	context: ParentContext
) -> void:
	# Weather severity
	profile.weather_severity = ProfileCalculations.calculate_weather_severity(
		profile.pressure_atm,
		body.physical.rotation_period_s,
		profile.has_atmosphere
	)

	# Radiation level
	profile.radiation_level = ProfileCalculations.calculate_radiation_level(
		body.physical.magnetic_moment,
		profile.pressure_atm,
		profile.has_atmosphere
	)

	# Tidal locking check (planets only; moons are handled in _calculate_moon_properties)
	profile.is_tidally_locked = false
	if body.has_orbital() and context != null and not profile.is_moon:
		var orbital_period: float = body.orbital.get_orbital_period_s(context.stellar_mass_kg)
		var rotation_period: float = absf(body.physical.rotation_period_s)
		if orbital_period > 0.0 and rotation_period > 0.0:
			var difference: float = absf(rotation_period - orbital_period)
			profile.is_tidally_locked = difference < orbital_period * 0.01


## Calculates moon-specific properties.
static func _calculate_moon_properties(
	profile: PlanetProfile,
	body: CelestialBody,
	parent_body: CelestialBody,
	context: ParentContext
) -> void:
	if not body.has_orbital():
		return

	var orbital_distance: float = body.orbital.semi_major_axis_m
	var eccentricity: float = body.orbital.eccentricity
	var moon_radius: float = body.physical.radius_m
	var parent_mass: float = parent_body.physical.mass_kg
	var parent_radius: float = parent_body.physical.radius_m
	var parent_magnetic: float = parent_body.physical.magnetic_moment

	# Moon orbital period (used for tidal lock and eclipse factor)
	var moon_orbital_period: float = body.orbital.get_orbital_period_s(parent_mass)
	var rotation_period: float = absf(body.physical.rotation_period_s)
	if moon_orbital_period > 0.0 and rotation_period > 0.0:
		var difference: float = absf(rotation_period - moon_orbital_period)
		profile.is_tidally_locked = difference < moon_orbital_period * 0.01

	# Tidal heating
	profile.tidal_heating_factor = ProfileCalculations.calculate_tidal_heating(
		parent_mass,
		orbital_distance,
		moon_radius,
		eccentricity
	)

	# Parent radiation (for gas giant moons)
	profile.parent_radiation_exposure = ProfileCalculations.calculate_parent_radiation(
		parent_mass,
		parent_magnetic,
		orbital_distance
	)

	# Add parent radiation to overall radiation level
	profile.radiation_level = clampf(
		profile.radiation_level + profile.parent_radiation_exposure * 0.5,
		0.0, 1.0
	)

	# Eclipse factor
	var parent_orbital_period: float = 0.0
	if parent_body.has_orbital() and context != null:
		parent_orbital_period = parent_body.orbital.get_orbital_period_s(context.stellar_mass_kg)

	profile.eclipse_factor = ProfileCalculations.calculate_eclipse_factor(
		parent_radius,
		orbital_distance,
		moon_orbital_period,
		parent_orbital_period
	)
