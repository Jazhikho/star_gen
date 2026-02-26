## Data for a single asteroid sampled within a belt field.
class_name BeltAsteroidData
extends RefCounted


## True for major/selectable asteroids, false for background visuals.
var is_major: bool = false

## Unique ID for major asteroids; empty for background asteroids.
var body_id: String = ""

## Asteroid type enum value (AsteroidType.Type) or -1.
var asteroid_type: int = -1

## Position in AU relative to belt host center.
var position_au: Vector3 = Vector3.ZERO

## Semi-major axis in AU.
var semi_major_axis_au: float = 0.0

## Eccentricity.
var eccentricity: float = 0.0

## Inclination in radians.
var inclination_rad: float = 0.0

## Longitude of ascending node in radians.
var longitude_ascending_node_rad: float = 0.0

## Argument of periapsis in radians.
var argument_periapsis_rad: float = 0.0

## True anomaly in radians.
var true_anomaly_rad: float = 0.0

## Body radius in kilometers.
var body_radius_km: float = 1.0
