## Input data describing a major asteroid to be placed in belt visualization.
## Uses the same units and field names as the project's OrbitalProps and PhysicalProps
## to make future integration with CelestialBody straightforward.
class_name MajorAsteroidInputConcept
extends RefCounted


## Unique identifier for linking back to a CelestialBody.
var body_id: String = ""

## Semi-major axis in meters (matches OrbitalProps).
var semi_major_axis_m: float = 0.0

## Orbital eccentricity [0, 1).
var eccentricity: float = 0.0

## Orbital inclination in degrees (matches OrbitalProps).
var inclination_deg: float = 0.0

## Longitude of ascending node in degrees.
var longitude_ascending_node_deg: float = 0.0

## Argument of periapsis in degrees.
var argument_periapsis_deg: float = 0.0

## Mean anomaly in degrees. Converted to true anomaly by the generator.
var mean_anomaly_deg: float = 0.0

## Body radius in km.
var body_radius_km: float = 100.0

## Asteroid compositional type. Uses AsteroidType.Type values, or -1 for unknown.
var asteroid_type: int = -1


## Factory: creates a MajorAsteroidInputConcept from raw orbital/physical data.
## Designed to map directly from CelestialBody.orbital + CelestialBody.physical.
## @param p_body_id: Unique body identifier.
## @param p_semi_major_axis_m: Semi-major axis in meters.
## @param p_eccentricity: Eccentricity.
## @param p_inclination_deg: Inclination in degrees.
## @param p_longitude_ascending_node_deg: Longitude of ascending node in degrees.
## @param p_argument_periapsis_deg: Argument of periapsis in degrees.
## @param p_mean_anomaly_deg: Mean anomaly in degrees.
## @param p_body_radius_km: Body radius in km.
## @param p_asteroid_type: AsteroidType.Type value or -1.
## @return: A new MajorAsteroidInputConcept.
static func from_orbital_data(
	p_body_id: String,
	p_semi_major_axis_m: float,
	p_eccentricity: float,
	p_inclination_deg: float,
	p_longitude_ascending_node_deg: float,
	p_argument_periapsis_deg: float,
	p_mean_anomaly_deg: float,
	p_body_radius_km: float,
	p_asteroid_type: int = -1
) -> MajorAsteroidInputConcept:
	var input: MajorAsteroidInputConcept = MajorAsteroidInputConcept.new()
	input.body_id = p_body_id
	input.semi_major_axis_m = p_semi_major_axis_m
	input.eccentricity = p_eccentricity
	input.inclination_deg = p_inclination_deg
	input.longitude_ascending_node_deg = p_longitude_ascending_node_deg
	input.argument_periapsis_deg = p_argument_periapsis_deg
	input.mean_anomaly_deg = p_mean_anomaly_deg
	input.body_radius_km = p_body_radius_km
	input.asteroid_type = p_asteroid_type
	return input
