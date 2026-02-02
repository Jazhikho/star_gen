## Manages scale transformations for solar system visualization.
## Converts astronomical distances/sizes to viewport-friendly units.
## Pure utility class with no scene tree dependencies.
class_name SystemScaleManager
extends RefCounted

const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")


## Distance scale: 1 viewport unit = this many meters.
## Default: 1 unit = 1 AU (adjustable via zoom).
var distance_scale_m_per_unit: float = Units.AU_METERS

## Minimum body display radius in viewport units.
const MIN_BODY_DISPLAY_RADIUS: float = 0.05

## Maximum body display radius in viewport units.
const MAX_BODY_DISPLAY_RADIUS: float = 0.5

## Star display radius multiplier (stars are shown larger for visibility).
const STAR_DISPLAY_MULTIPLIER: float = 3.0

## Planet display radius multiplier.
const PLANET_DISPLAY_MULTIPLIER: float = 8.0

## Moon display radius multiplier.
const MOON_DISPLAY_MULTIPLIER: float = 12.0

## Asteroid display radius multiplier.
const ASTEROID_DISPLAY_MULTIPLIER: float = 20.0

## Orbit line width in viewport units.
const ORBIT_LINE_WIDTH: float = 0.01


## Creates a new SystemScaleManager with the given distance scale.
## @param p_distance_scale: Meters per viewport unit.
func _init(p_distance_scale: float = Units.AU_METERS) -> void:
	distance_scale_m_per_unit = maxf(p_distance_scale, 1.0)


## Converts a distance in meters to viewport units.
## @param distance_m: Distance in meters.
## @return: Distance in viewport units.
func distance_to_units(distance_m: float) -> float:
	return distance_m / distance_scale_m_per_unit


## Converts a viewport unit distance back to meters.
## @param units: Distance in viewport units.
## @return: Distance in meters.
func units_to_distance(units: float) -> float:
	return units * distance_scale_m_per_unit


## Calculates the display radius for a body.
## Bodies are exaggerated in size for visibility.
## @param body: The celestial body.
## @return: Display radius in viewport units.
func get_body_display_radius(body: CelestialBody) -> float:
	if body == null:
		return MIN_BODY_DISPLAY_RADIUS
	
	var radius_m: float = body.physical.radius_m
	var base_radius: float = distance_to_units(radius_m)
	
	# Apply type-specific multiplier for visibility
	var multiplier: float = _get_type_multiplier(body.type)
	var display_radius: float = base_radius * multiplier
	
	return clampf(display_radius, MIN_BODY_DISPLAY_RADIUS, MAX_BODY_DISPLAY_RADIUS)


## Calculates the 3D position for a body based on its orbital parameters.
## Uses mean anomaly for initial placement (static snapshot, not animated).
## @param semi_major_axis_m: Semi-major axis in meters.
## @param eccentricity: Orbital eccentricity.
## @param inclination_deg: Orbital inclination in degrees.
## @param longitude_ascending_node_deg: Longitude of ascending node in degrees.
## @param argument_periapsis_deg: Argument of periapsis in degrees.
## @param mean_anomaly_deg: Mean anomaly at epoch in degrees.
## @return: Position in viewport units (Vector3).
func get_orbital_position(
	semi_major_axis_m: float,
	eccentricity: float,
	inclination_deg: float,
	longitude_ascending_node_deg: float,
	argument_periapsis_deg: float,
	mean_anomaly_deg: float
) -> Vector3:
	# Solve Kepler's equation for eccentric anomaly
	var mean_anomaly_rad: float = deg_to_rad(mean_anomaly_deg)
	var eccentric_anomaly: float = _solve_kepler(mean_anomaly_rad, eccentricity)
	
	# Calculate true anomaly from eccentric anomaly
	var true_anomaly: float = _eccentric_to_true_anomaly(eccentric_anomaly, eccentricity)
	
	# Calculate radius at this point in the orbit
	var radius_m: float = semi_major_axis_m * (1.0 - eccentricity * cos(eccentric_anomaly))
	var radius_units: float = distance_to_units(radius_m)
	
	# Calculate position in orbital plane (2D)
	var x_orbital: float = radius_units * cos(true_anomaly)
	var z_orbital: float = radius_units * sin(true_anomaly)
	
	# Apply orbital element rotations
	var omega: float = deg_to_rad(argument_periapsis_deg)
	var big_omega: float = deg_to_rad(longitude_ascending_node_deg)
	var inc: float = deg_to_rad(inclination_deg)
	
	# Rotate by argument of periapsis (in orbital plane)
	var x_rot: float = x_orbital * cos(omega) - z_orbital * sin(omega)
	var z_rot: float = x_orbital * sin(omega) + z_orbital * cos(omega)
	
	# Apply inclination (tilt out of reference plane)
	var y_inclined: float = z_rot * sin(inc)
	var z_inclined: float = z_rot * cos(inc)
	
	# Rotate by longitude of ascending node (around Y axis)
	var x_final: float = x_rot * cos(big_omega) - z_inclined * sin(big_omega)
	var z_final: float = x_rot * sin(big_omega) + z_inclined * cos(big_omega)
	
	return Vector3(x_final, y_inclined, z_final)


## Gets the orbital position for a CelestialBody using its OrbitalProps.
## @param body: The celestial body with orbital properties.
## @return: Position in viewport units.
func get_body_orbital_position(body: CelestialBody) -> Vector3:
	if body == null or not body.has_orbital():
		return Vector3.ZERO
	
	var orbital: OrbitalProps = body.orbital
	return get_orbital_position(
		orbital.semi_major_axis_m,
		orbital.eccentricity,
		orbital.inclination_deg,
		orbital.longitude_of_ascending_node_deg,
		orbital.argument_of_periapsis_deg,
		orbital.mean_anomaly_deg
	)


## Generates points along an orbital ellipse for rendering.
## @param semi_major_axis_m: Semi-major axis in meters.
## @param eccentricity: Orbital eccentricity.
## @param inclination_deg: Inclination in degrees.
## @param longitude_ascending_node_deg: Longitude of ascending node.
## @param argument_periapsis_deg: Argument of periapsis.
## @param num_points: Number of points to generate.
## @return: Array of Vector3 positions in viewport units.
func generate_orbit_points(
	semi_major_axis_m: float,
	eccentricity: float,
	inclination_deg: float,
	longitude_ascending_node_deg: float,
	argument_periapsis_deg: float,
	num_points: int = 128
) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	
	if semi_major_axis_m <= 0.0:
		return points
	
	var omega: float = deg_to_rad(argument_periapsis_deg)
	var big_omega: float = deg_to_rad(longitude_ascending_node_deg)
	var inc: float = deg_to_rad(inclination_deg)
	var a_units: float = distance_to_units(semi_major_axis_m)
	var b_units: float = a_units * sqrt(1.0 - eccentricity * eccentricity)
	
	# Center offset (focus is at origin, center is offset by ae)
	var center_offset: float = a_units * eccentricity
	
	for i in range(num_points + 1):
		var angle: float = (float(i) / float(num_points)) * TAU
		
		# Ellipse in orbital plane (focus at origin)
		var x_orbital: float = a_units * cos(angle) - center_offset
		var z_orbital: float = b_units * sin(angle)
		
		# Rotate by argument of periapsis
		var x_rot: float = x_orbital * cos(omega) - z_orbital * sin(omega)
		var z_rot: float = x_orbital * sin(omega) + z_orbital * cos(omega)
		
		# Apply inclination
		var y_inclined: float = z_rot * sin(inc)
		var z_inclined: float = z_rot * cos(inc)
		
		# Rotate by longitude of ascending node
		var x_final: float = x_rot * cos(big_omega) - z_inclined * sin(big_omega)
		var z_final: float = x_rot * sin(big_omega) + z_inclined * cos(big_omega)
		
		points.append(Vector3(x_final, y_inclined, z_final))
	
	return points


## Solves Kepler's equation M = E - e*sin(E) for E.
## Uses Newton-Raphson iteration.
## @param mean_anomaly_rad: Mean anomaly in radians.
## @param eccentricity: Orbital eccentricity.
## @return: Eccentric anomaly in radians.
func _solve_kepler(mean_anomaly_rad: float, eccentricity: float) -> float:
	if eccentricity < 1e-10:
		return mean_anomaly_rad
	
	# Initial guess
	var e_anomaly: float = mean_anomaly_rad
	if eccentricity > 0.8:
		e_anomaly = PI
	
	# Newton-Raphson iteration
	for _iter in range(20):
		var delta: float = e_anomaly - eccentricity * sin(e_anomaly) - mean_anomaly_rad
		var derivative: float = 1.0 - eccentricity * cos(e_anomaly)
		if absf(derivative) < 1e-15:
			break
		var correction: float = delta / derivative
		e_anomaly -= correction
		if absf(correction) < 1e-12:
			break
	
	return e_anomaly


## Converts eccentric anomaly to true anomaly.
## @param eccentric_anomaly: Eccentric anomaly in radians.
## @param eccentricity: Orbital eccentricity.
## @return: True anomaly in radians.
func _eccentric_to_true_anomaly(eccentric_anomaly: float, eccentricity: float) -> float:
	if eccentricity < 1e-10:
		return eccentric_anomaly
	
	var half_e: float = eccentric_anomaly / 2.0
	var numerator: float = sqrt(1.0 + eccentricity) * sin(half_e)
	var denominator: float = sqrt(1.0 - eccentricity) * cos(half_e)
	
	if absf(denominator) < 1e-15:
		return eccentric_anomaly
	
	return 2.0 * atan2(numerator, denominator)


## Gets the type-specific size multiplier for visibility.
## @param body_type: The celestial body type.
## @return: Multiplier for display radius.
func _get_type_multiplier(body_type: CelestialType.Type) -> float:
	match body_type:
		CelestialType.Type.STAR:
			return STAR_DISPLAY_MULTIPLIER
		CelestialType.Type.PLANET:
			return PLANET_DISPLAY_MULTIPLIER
		CelestialType.Type.MOON:
			return MOON_DISPLAY_MULTIPLIER
		CelestialType.Type.ASTEROID:
			return ASTEROID_DISPLAY_MULTIPLIER
		_:
			return PLANET_DISPLAY_MULTIPLIER
