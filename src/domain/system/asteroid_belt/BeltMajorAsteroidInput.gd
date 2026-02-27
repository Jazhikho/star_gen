## Input orbital/physical data for a major asteroid.
class_name BeltMajorAsteroidInput
extends RefCounted


## Major asteroid body ID.
var body_id: String = ""

## Semi-major axis in meters.
var semi_major_axis_m: float = 0.0

## Orbital eccentricity.
var eccentricity: float = 0.0

## Inclination in degrees.
var inclination_deg: float = 0.0

## Longitude of ascending node in degrees.
var longitude_ascending_node_deg: float = 0.0

## Argument of periapsis in degrees.
var argument_periapsis_deg: float = 0.0

## Mean anomaly in degrees.
var mean_anomaly_deg: float = 0.0

## Body radius in kilometers.
var body_radius_km: float = 100.0

## Asteroid type enum value (AsteroidType.Type) or -1.
var asteroid_type: int = -1
