## Orbital parameters of a celestial body.
## Defines the Keplerian orbital elements.
class_name OrbitalProps
extends RefCounted


## Semi-major axis in meters.
var semi_major_axis_m: float

## Orbital eccentricity (0 = circular, 0-1 = elliptical).
var eccentricity: float

## Orbital inclination in degrees.
var inclination_deg: float

## Longitude of ascending node in degrees.
var longitude_of_ascending_node_deg: float

## Argument of periapsis in degrees.
var argument_of_periapsis_deg: float

## Mean anomaly at epoch in degrees.
var mean_anomaly_deg: float

## ID of the parent body this object orbits (empty if none).
var parent_id: String


## Creates a new OrbitalProps instance.
## @param p_semi_major_axis_m: Semi-major axis in meters.
## @param p_eccentricity: Orbital eccentricity.
## @param p_inclination_deg: Inclination in degrees.
## @param p_longitude_of_ascending_node_deg: Longitude of ascending node.
## @param p_argument_of_periapsis_deg: Argument of periapsis.
## @param p_mean_anomaly_deg: Mean anomaly at epoch.
## @param p_parent_id: ID of parent body.
func _init(
	p_semi_major_axis_m: float = 0.0,
	p_eccentricity: float = 0.0,
	p_inclination_deg: float = 0.0,
	p_longitude_of_ascending_node_deg: float = 0.0,
	p_argument_of_periapsis_deg: float = 0.0,
	p_mean_anomaly_deg: float = 0.0,
	p_parent_id: String = ""
) -> void:
	semi_major_axis_m = p_semi_major_axis_m
	eccentricity = p_eccentricity
	inclination_deg = p_inclination_deg
	longitude_of_ascending_node_deg = p_longitude_of_ascending_node_deg
	argument_of_periapsis_deg = p_argument_of_periapsis_deg
	mean_anomaly_deg = p_mean_anomaly_deg
	parent_id = p_parent_id


## Calculates the periapsis distance in meters.
## @return: Periapsis distance in meters.
func get_periapsis_m() -> float:
	return semi_major_axis_m * (1.0 - eccentricity)


## Calculates the apoapsis distance in meters.
## @return: Apoapsis distance in meters.
func get_apoapsis_m() -> float:
	return semi_major_axis_m * (1.0 + eccentricity)


## Calculates the orbital period in seconds given parent mass.
## @param parent_mass_kg: Mass of the parent body in kg.
## @return: Orbital period in seconds.
func get_orbital_period_s(parent_mass_kg: float) -> float:
	if semi_major_axis_m <= 0.0 or parent_mass_kg <= 0.0:
		return 0.0
	const G: float = 6.674e-11
	return 2.0 * PI * sqrt(pow(semi_major_axis_m, 3.0) / (G * parent_mass_kg))


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"semi_major_axis_m": semi_major_axis_m,
		"eccentricity": eccentricity,
		"inclination_deg": inclination_deg,
		"longitude_of_ascending_node_deg": longitude_of_ascending_node_deg,
		"argument_of_periapsis_deg": argument_of_periapsis_deg,
		"mean_anomaly_deg": mean_anomaly_deg,
		"parent_id": parent_id,
	}


## Creates an OrbitalProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new OrbitalProps instance.
static func from_dict(data: Dictionary) -> OrbitalProps:
	var script: GDScript = load("res://src/domain/celestial/components/OrbitalProps.gd") as GDScript
	return script.new(
		data.get("semi_major_axis_m", 0.0) as float,
		data.get("eccentricity", 0.0) as float,
		data.get("inclination_deg", 0.0) as float,
		data.get("longitude_of_ascending_node_deg", 0.0) as float,
		data.get("argument_of_periapsis_deg", 0.0) as float,
		data.get("mean_anomaly_deg", 0.0) as float,
		data.get("parent_id", "") as String
	) as OrbitalProps
