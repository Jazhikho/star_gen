## Specification for procedurally generating an asteroid belt.
## Controls geometry, density profile, gaps, clustering, size distribution,
## and references to major bodies that should be included in the output.
class_name AsteroidBeltSpecConcept
extends RefCounted


## Inner edge of the asteroid belt in AU.
var inner_radius_au: float = 2.0

## Outer edge of the asteroid belt in AU.
var outer_radius_au: float = 3.5

## Number of background (visual) asteroids to generate.
var asteroid_count: int = 1000

## Maximum orbital inclination in degrees. Controls vertical thickness of belt.
var max_inclination_deg: float = 20.0

## Maximum orbital eccentricity. Controls how elliptical individual orbits can be.
var max_eccentricity: float = 0.25

## Minimum background asteroid body radius in km.
var min_body_radius_km: float = 0.5

## Maximum background asteroid body radius in km.
var max_body_radius_km: float = 500.0

## Power law exponent for size distribution. Higher values produce more small asteroids.
var size_power_law_exponent: float = 2.5

## Radial concentration exponent. Higher values concentrate asteroids toward belt center.
## At 0.0 the radial distribution is uniform; at 2.0+ it peaks sharply at belt midpoint.
var radial_concentration: float = 2.0

## Center positions of radial gaps in AU (e.g., orbital resonance gaps).
var gap_centers_au: Array[float] = []

## Half-widths of each corresponding gap in AU. Must match gap_centers_au length.
var gap_half_widths_au: Array[float] = []

## Number of angular density clusters (e.g., Trojan-like groups). 0 for uniform angular spread.
var cluster_count: int = 0

## Angular positions of each cluster in radians [0, TAU).
var cluster_longitudes_rad: Array[float] = []

## Von Mises concentration parameter for angular clusters. Higher = tighter grouping.
var cluster_concentration: float = 3.0

## Fraction of asteroids allocated to clusters vs uniform background [0.0, 1.0].
var cluster_fraction: float = 0.3

## Major asteroid inputs to include in the belt output as selectable bodies.
## These are positioned using their own orbital elements, not the random distribution.
var major_asteroid_inputs: Array[MajorAsteroidInputConcept] = []
