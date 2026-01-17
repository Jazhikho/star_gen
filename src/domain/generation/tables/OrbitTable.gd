## Lookup table for orbital zone distances and properties.
class_name OrbitTable
extends RefCounted

const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Gets typical orbital distance range for a zone given stellar luminosity.
## @param zone: The orbit zone.
## @param stellar_luminosity_watts: Star's luminosity in watts.
## @return: Dictionary with "min" and "max" distances in meters.
static func get_distance_range(
	zone: OrbitZone.Zone,
	stellar_luminosity_watts: float
) -> Dictionary:
	var l_solar: float = stellar_luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	if l_solar <= 0.0:
		l_solar = 1.0
	var sqrt_l: float = sqrt(l_solar)
	
	match zone:
		OrbitZone.Zone.HOT:
			return {
				"min": 0.01 * Units.AU_METERS * sqrt_l,
				"max": 0.95 * Units.AU_METERS * sqrt_l,
			}
		OrbitZone.Zone.TEMPERATE:
			return {
				"min": 0.95 * Units.AU_METERS * sqrt_l,
				"max": 2.7 * Units.AU_METERS * sqrt_l,
			}
		OrbitZone.Zone.COLD:
			return {
				"min": 2.7 * Units.AU_METERS * sqrt_l,
				"max": 50.0 * Units.AU_METERS * sqrt_l,
			}
		_:
			return {"min": Units.AU_METERS, "max": Units.AU_METERS}


## Generates a random orbital distance for a zone.
## @param zone: The orbit zone.
## @param stellar_luminosity_watts: Star's luminosity in watts.
## @param rng: The random number generator.
## @return: Orbital distance in meters.
static func random_distance(
	zone: OrbitZone.Zone,
	stellar_luminosity_watts: float,
	rng: SeededRng
) -> float:
	var range_data: Dictionary = get_distance_range(zone, stellar_luminosity_watts)
	# Use log-uniform distribution for more realistic orbital spacing
	var log_min: float = log(range_data["min"])
	var log_max: float = log(range_data["max"])
	var log_val: float = rng.randf_range(log_min, log_max)
	return exp(log_val)


## Gets typical eccentricity range for a zone.
## Hot planets tend to be circularized, cold can be more eccentric.
## @param zone: The orbit zone.
## @return: Dictionary with "min" and "max" eccentricity values.
static func get_eccentricity_range(zone: OrbitZone.Zone) -> Dictionary:
	match zone:
		OrbitZone.Zone.HOT:
			return {"min": 0.0, "max": 0.1}
		OrbitZone.Zone.TEMPERATE:
			return {"min": 0.0, "max": 0.2}
		OrbitZone.Zone.COLD:
			return {"min": 0.0, "max": 0.4}
		_:
			return {"min": 0.0, "max":  0.1}


## Generates a random eccentricity for a zone.
## @param zone: The orbit zone.
## @param rng: The random number generator.
## @return: Orbital eccentricity.
static func random_eccentricity(zone: OrbitZone.Zone, rng: SeededRng) -> float:
	var range_data: Dictionary = get_eccentricity_range(zone)
	# Bias toward lower eccentricities
	var raw: float = rng.randf_range(0.0, 1.0)
	var biased: float = raw * raw  # Square for bias toward 0
	return lerpf(range_data["min"], range_data["max"], biased)


## Generates a random inclination.
## Most planets have low inclinations.
## @param rng: The random number generator.
## @return: Orbital inclination in degrees.
static func random_inclination(rng: SeededRng) -> float:
	# Most planets within ~5 degrees, some up to 10
	var raw: float = rng.randf_range(0.0, 1.0)
	var biased: float = raw * raw  # Bias toward 0
	return biased * 10.0


## Calculates tidal locking timescale for a body.
## @param orbital_distance_m: Distance from star.
## @param body_mass_kg: Mass of the planet.
## @param body_radius_m: Radius of the planet.
## @param stellar_mass_kg: Mass of the star.
## @return: Tidal locking timescale in years.
static func tidal_locking_timescale_years(
	orbital_distance_m: float,
	body_mass_kg: float,
	body_radius_m: float,
	stellar_mass_kg: float
) -> float:
	if body_radius_m <= 0.0 or stellar_mass_kg <= 0.0:
		return 1.0e20
	
	# Simplified formula based on Peale (1977)
	# τ ∝ a^6 * m / (M^2 * R^3)
	# Using Earth as reference: τ_Earth ≈ 10^12 years at 1 AU
	var a_au: float = orbital_distance_m / Units.AU_METERS
	var m_earth: float = body_mass_kg / Units.EARTH_MASS_KG
	var r_earth: float = body_radius_m / Units.EARTH_RADIUS_METERS
	var m_sun: float = stellar_mass_kg / Units.SOLAR_MASS_KG
	
	var tau: float = 1.0e10 * pow(a_au, 6.0) * m_earth / (pow(m_sun, 2.0) * pow(r_earth, 3.0))
	return tau


## Determines if a body would be tidally locked given system age.
## @param orbital_distance_m: Distance from star.
## @param body_mass_kg: Mass of the planet.
## @param body_radius_m: Radius of the planet.
## @param stellar_mass_kg: Mass of the star.
## @param system_age_years: Age of the system in years.
## @return: True if the body would be tidally locked.
static func is_tidally_locked(
	orbital_distance_m: float,
	body_mass_kg: float,
	body_radius_m: float,
	stellar_mass_kg: float,
	system_age_years: float
) -> bool:
	var timescale: float = tidal_locking_timescale_years(
		orbital_distance_m,
		body_mass_kg,
		body_radius_m,
		stellar_mass_kg
	)
	return system_age_years > timescale
