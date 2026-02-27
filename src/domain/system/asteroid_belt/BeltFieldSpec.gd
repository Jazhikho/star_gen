## Specification for rendering/generating a dense asteroid belt field.
class_name BeltFieldSpec
extends RefCounted


## Inner belt edge in AU.
var inner_radius_au: float = 2.0

## Outer belt edge in AU.
var outer_radius_au: float = 3.5

## Number of background asteroids.
var asteroid_count: int = 1000

## Maximum orbital inclination in degrees.
var max_inclination_deg: float = 20.0

## Maximum eccentricity.
var max_eccentricity: float = 0.25

## Minimum body radius in kilometers.
var min_body_radius_km: float = 0.5

## Maximum body radius in kilometers.
var max_body_radius_km: float = 500.0

## Power-law exponent for body size distribution.
var size_power_law_exponent: float = 2.5

## Radial concentration exponent.
var radial_concentration: float = 2.0

## Gap centers in AU.
var gap_centers_au: Array[float] = []

## Gap half widths in AU.
var gap_half_widths_au: Array[float] = []

## Number of angular clusters.
var cluster_count: int = 0

## Cluster longitudes in radians.
var cluster_longitudes_rad: Array[float] = []

## Cluster concentration parameter.
var cluster_concentration: float = 3.0

## Fraction of asteroids in cluster groups.
var cluster_fraction: float = 0.3

## Major asteroid inputs for explicit placement.
var major_asteroid_inputs: Array = []
