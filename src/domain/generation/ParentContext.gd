## Provides parent body context for generation without object references.
## Used to calculate orbital dynamics, tidal effects, atmospheric escape, etc.
class_name ParentContext
extends RefCounted

const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Mass of the parent star in kilograms.
var stellar_mass_kg: float

## Luminosity of the parent star in watts.
var stellar_luminosity_watts: float

## Effective temperature of the parent star in Kelvin.
var stellar_temperature_k: float

## Age of the parent star in years.
var stellar_age_years: float

## Distance from the star in meters (for planets: direct, for moons: parent planet's distance).
var orbital_distance_from_star_m: float

## Mass of the parent body in kilograms (for moons: the planet they orbit).
var parent_body_mass_kg: float

## Radius of the parent body in meters (for Roche limit and Hill sphere calculations).
var parent_body_radius_m: float

## Distance from the parent body in meters (for moons: distance from planet).
var orbital_distance_from_parent_m: float


## Creates a new ParentContext instance.
func _init(
	p_stellar_mass_kg: float = 0.0,
	p_stellar_luminosity_watts: float = 0.0,
	p_stellar_temperature_k: float = 0.0,
	p_stellar_age_years: float = 0.0,
	p_orbital_distance_from_star_m: float = 0.0,
	p_parent_body_mass_kg: float = 0.0,
	p_parent_body_radius_m: float = 0.0,
	p_orbital_distance_from_parent_m: float = 0.0
) -> void:
	stellar_mass_kg = p_stellar_mass_kg
	stellar_luminosity_watts = p_stellar_luminosity_watts
	stellar_temperature_k = p_stellar_temperature_k
	stellar_age_years = p_stellar_age_years
	orbital_distance_from_star_m = p_orbital_distance_from_star_m
	parent_body_mass_kg = p_parent_body_mass_kg
	parent_body_radius_m = p_parent_body_radius_m
	orbital_distance_from_parent_m = p_orbital_distance_from_parent_m


## Creates a context for a planet orbiting a star (no parent body).
## @param stellar_mass_kg: Mass of the star.
## @param stellar_luminosity_watts: Luminosity of the star.
## @param stellar_temperature_k: Temperature of the star.
## @param stellar_age_years: Age of the star.
## @param orbital_distance_m: Distance from the star.
## @return: A new ParentContext for a planet.
static func for_planet(
	p_stellar_mass_kg: float,
	p_stellar_luminosity_watts: float,
	p_stellar_temperature_k: float,
	p_stellar_age_years: float,
	p_orbital_distance_m: float
) -> ParentContext:
	return ParentContext.new(
		p_stellar_mass_kg,
		p_stellar_luminosity_watts,
		p_stellar_temperature_k,
		p_stellar_age_years,
		p_orbital_distance_m,
		0.0,
		0.0,
		0.0
	)


## Creates a context for a moon orbiting a planet.
## @param stellar_mass_kg: Mass of the star.
## @param stellar_luminosity_watts: Luminosity of the star.
## @param stellar_temperature_k: Temperature of the star.
## @param stellar_age_years: Age of the star.
## @param planet_orbital_distance_m: Distance of the planet from the star.
## @param planet_mass_kg: Mass of the parent planet.
## @param planet_radius_m: Radius of the parent planet.
## @param moon_orbital_distance_m: Distance of the moon from the planet.
## @return: A new ParentContext for a moon.
static func for_moon(
	p_stellar_mass_kg: float,
	p_stellar_luminosity_watts: float,
	p_stellar_temperature_k: float,
	p_stellar_age_years: float,
	p_planet_orbital_distance_m: float,
	p_planet_mass_kg: float,
	p_planet_radius_m: float,
	p_moon_orbital_distance_m: float
) -> ParentContext:
	return ParentContext.new(
		p_stellar_mass_kg,
		p_stellar_luminosity_watts,
		p_stellar_temperature_k,
		p_stellar_age_years,
		p_planet_orbital_distance_m,
		p_planet_mass_kg,
		p_planet_radius_m,
		p_moon_orbital_distance_m
	)


## Creates a Sun-like default context for testing or standalone generation.
## @param orbital_distance_m: Distance from the star (defaults to 1 AU).
## @return: A ParentContext with solar values.
static func sun_like(orbital_distance_m: float = Units.AU_METERS) -> ParentContext:
	return for_planet(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		orbital_distance_m
	)


## Returns whether this context has a parent body (is for a moon).
## @return: True if parent body data is present.
func has_parent_body() -> bool:
	return parent_body_mass_kg > 0.0


## Calculates the Hill sphere radius for the parent body.
## The Hill sphere is the region where a body's gravity dominates.
## @return: Hill sphere radius in meters, or 0 if not applicable.
func get_hill_sphere_radius_m() -> float:
	if parent_body_mass_kg <= 0.0 or stellar_mass_kg <= 0.0:
		return 0.0
	if orbital_distance_from_star_m <= 0.0:
		return 0.0
	
	var mass_ratio: float = parent_body_mass_kg / (3.0 * stellar_mass_kg)
	return orbital_distance_from_star_m * pow(mass_ratio, 1.0 / 3.0)


## Calculates the Roche limit for the parent body given a satellite density.
## Inside this limit, a body would be torn apart by tidal forces.
## @param satellite_density_kg_m3: Density of the satellite.
## @return: Roche limit in meters.
func get_roche_limit_m(satellite_density_kg_m3: float) -> float:
	if parent_body_radius_m <= 0.0 or parent_body_mass_kg <= 0.0:
		return 0.0
	if satellite_density_kg_m3 <= 0.0:
		return 0.0
	
	var parent_density: float = parent_body_mass_kg / ((4.0 / 3.0) * PI * pow(parent_body_radius_m, 3.0))
	return 2.44 * parent_body_radius_m * pow(parent_density / satellite_density_kg_m3, 1.0 / 3.0)


## Calculates equilibrium temperature at the orbital distance from the star.
## Assumes no greenhouse effect (bare rock/albedo = 0.3).
## @param albedo: Bond albedo of the body (default 0.3).
## @return: Equilibrium temperature in Kelvin.
func get_equilibrium_temperature_k(albedo: float = 0.3) -> float:
	if stellar_luminosity_watts <= 0.0 or orbital_distance_from_star_m <= 0.0:
		return 0.0
	
	var stefan_boltzmann: float = 5.67e-8
	var absorbed: float = stellar_luminosity_watts * (1.0 - albedo)
	var distance_factor: float = 4.0 * PI * pow(orbital_distance_from_star_m, 2.0)
	
	return pow(absorbed / (4.0 * distance_factor * stefan_boltzmann), 0.25)


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"stellar_mass_kg": stellar_mass_kg,
		"stellar_luminosity_watts": stellar_luminosity_watts,
		"stellar_temperature_k": stellar_temperature_k,
		"stellar_age_years": stellar_age_years,
		"orbital_distance_from_star_m": orbital_distance_from_star_m,
		"parent_body_mass_kg": parent_body_mass_kg,
		"parent_body_radius_m": parent_body_radius_m,
		"orbital_distance_from_parent_m": orbital_distance_from_parent_m,
	}


## Creates a ParentContext from a dictionary.
## @param data: Dictionary to parse.
## @return: A new ParentContext instance.
static func from_dict(data: Dictionary) -> ParentContext:
	return ParentContext.new(
		data.get("stellar_mass_kg", 0.0) as float,
		data.get("stellar_luminosity_watts", 0.0) as float,
		data.get("stellar_temperature_k", 0.0) as float,
		data.get("stellar_age_years", 0.0) as float,
		data.get("orbital_distance_from_star_m", 0.0) as float,
		data.get("parent_body_mass_kg", 0.0) as float,
		data.get("parent_body_radius_m", 0.0) as float,
		data.get("orbital_distance_from_parent_m", 0.0) as float
	)
