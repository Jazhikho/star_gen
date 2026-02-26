## Static orbital mechanics utilities for the asteroid belt concept.
## Provides Kepler equation solving, anomaly conversions, and
## orbital-element-to-Cartesian position transformation (Godot Y-up).
class_name OrbitalMathConcept
extends RefCounted


## One Astronomical Unit in meters. Matches Units.AU_METERS for future merge.
const AU_METERS: float = 1.496e11


## Converts six classical orbital elements to a 3D position vector (Godot Y-up).
## Uses the standard perifocal-to-inertial rotation matrix with axes remapped:
## orbital X,Y → Godot X,Z (horizontal), orbital Z → Godot Y (up).
## @param a: Semi-major axis (in whatever unit the caller wants the result in).
## @param e: Eccentricity.
## @param i: Inclination in radians.
## @param big_omega: Longitude of ascending node in radians.
## @param small_omega: Argument of periapsis in radians.
## @param nu: True anomaly in radians.
## @return: Position vector in the same unit as a.
static func orbital_elements_to_position(
	a: float, e: float, i: float,
	big_omega: float, small_omega: float, nu: float
) -> Vector3:
	# Orbital radius at current true anomaly
	var r: float = a * (1.0 - e * e) / (1.0 + e * cos(nu))

	# Position in perifocal frame (orbit plane, periapsis along x)
	var x_pf: float = r * cos(nu)
	var y_pf: float = r * sin(nu)

	# Precompute trig values
	var cos_O: float = cos(big_omega)
	var sin_O: float = sin(big_omega)
	var cos_w: float = cos(small_omega)
	var sin_w: float = sin(small_omega)
	var cos_i: float = cos(i)
	var sin_i: float = sin(i)

	# Rotation matrix columns: perifocal → inertial reference frame
	var px: float = cos_O * cos_w - sin_O * sin_w * cos_i
	var py: float = sin_O * cos_w + cos_O * sin_w * cos_i
	var pz: float = sin_w * sin_i
	var qx: float = - cos_O * sin_w - sin_O * cos_w * cos_i
	var qy: float = - sin_O * sin_w + cos_O * cos_w * cos_i
	var qz: float = cos_w * sin_i

	# Remap: orbital (X,Y,Z) → Godot (X, Y-up, Z)
	return Vector3(
		x_pf * px + y_pf * qx,
		x_pf * pz + y_pf * qz,
		x_pf * py + y_pf * qy
	)


## Converts mean anomaly to true anomaly via Kepler equation.
## @param mean_anomaly_rad: Mean anomaly in radians.
## @param eccentricity: Orbital eccentricity [0, 1).
## @return: True anomaly in radians.
static func mean_anomaly_to_true_anomaly(
	mean_anomaly_rad: float,
	eccentricity: float
) -> float:
	var eccentric_anomaly: float = solve_kepler_equation(mean_anomaly_rad, eccentricity)
	return eccentric_to_true_anomaly(eccentric_anomaly, eccentricity)


## Converts eccentric anomaly to true anomaly.
## @param eccentric_anomaly_rad: Eccentric anomaly in radians.
## @param eccentricity: Orbital eccentricity [0, 1).
## @return: True anomaly in radians.
static func eccentric_to_true_anomaly(
	eccentric_anomaly_rad: float,
	eccentricity: float
) -> float:
	if eccentricity < 1.0e-10:
		return eccentric_anomaly_rad
	var half_e: float = eccentric_anomaly_rad * 0.5
	return 2.0 * atan2(
		sqrt(1.0 + eccentricity) * sin(half_e),
		sqrt(1.0 - eccentricity) * cos(half_e)
	)


## Solves Kepler's equation M = E − e·sin(E) for eccentric anomaly E
## using Newton-Raphson iteration.
## @param mean_anomaly_rad: Mean anomaly M in radians.
## @param eccentricity: Orbital eccentricity [0, 1).
## @param tolerance: Convergence tolerance (default 1e-10).
## @param max_iterations: Maximum Newton iterations (default 50).
## @return: Eccentric anomaly E in radians.
static func solve_kepler_equation(
	mean_anomaly_rad: float,
	eccentricity: float,
	tolerance: float = 1.0e-10,
	max_iterations: int = 50
) -> float:
	if eccentricity < 1.0e-10:
		return mean_anomaly_rad

	var e_anomaly: float = mean_anomaly_rad
	for _iter in range(max_iterations):
		var delta: float = e_anomaly - eccentricity * sin(e_anomaly) - mean_anomaly_rad
		var derivative: float = 1.0 - eccentricity * cos(e_anomaly)
		if absf(derivative) < 1.0e-12:
			break
		e_anomaly -= delta / derivative
		if absf(delta) < tolerance:
			break
	return e_anomaly
