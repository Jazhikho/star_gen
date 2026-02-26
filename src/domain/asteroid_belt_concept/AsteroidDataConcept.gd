## Data for a single asteroid within a belt.
## Stores both the computed 3D position and the orbital elements used to derive it.
## Distinguishes major (selectable) bodies from background (visual-only) rocks.
class_name AsteroidDataConcept
extends RefCounted


## Whether this is a major (top-10, selectable) asteroid vs background visual rock.
var is_major: bool = false

## Unique body identifier. Non-empty for major asteroids, empty for background.
var body_id: String = ""

## Asteroid compositional type. Uses AsteroidType.Type values, or -1 for unknown.
var asteroid_type: int = -1

## 3D position of the asteroid in AU, relative to the parent star (Godot Y-up).
var position_au: Vector3 = Vector3.ZERO

## Semi-major axis of the orbit in AU.
var semi_major_axis_au: float = 0.0

## Orbital eccentricity [0, max_eccentricity].
var eccentricity: float = 0.0

## Orbital inclination in radians [0, max].
var inclination_rad: float = 0.0

## Longitude of ascending node in radians [0, TAU).
var longitude_ascending_node_rad: float = 0.0

## Argument of periapsis in radians [0, TAU).
var argument_periapsis_rad: float = 0.0

## True anomaly in radians [0, TAU). Position along orbit at generation snapshot.
var true_anomaly_rad: float = 0.0

## Body radius in km.
var body_radius_km: float = 1.0
