## Container for a fully generated asteroid belt.
## Holds all asteroid data (both major and background) plus provenance information.
class_name AsteroidBeltDataConcept
extends RefCounted


## All asteroids in this belt (major + background).
var asteroids: Array[AsteroidDataConcept] = []

## The spec used to generate this belt (provenance).
var spec: AsteroidBeltSpecConcept = null

## The seed used for generation (provenance).
var generation_seed: int = 0

## Generator version string for provenance tracking.
var generator_version: String = "concept-1.1"


## Returns only the major (selectable) asteroids.
## @return: Array of AsteroidDataConcept where is_major == true.
func get_major_asteroids() -> Array[AsteroidDataConcept]:
	var result: Array[AsteroidDataConcept] = []
	for a in asteroids:
		if a.is_major:
			result.append(a)
	return result


## Returns only the background (visual-only) asteroids.
## @return: Array of AsteroidDataConcept where is_major == false.
func get_background_asteroids() -> Array[AsteroidDataConcept]:
	var result: Array[AsteroidDataConcept] = []
	for a in asteroids:
		if not a.is_major:
			result.append(a)
	return result


## Returns the count of major asteroids.
## @return: Number of major asteroids.
func get_major_count() -> int:
	var count: int = 0
	for a in asteroids:
		if a.is_major:
			count += 1
	return count


## Returns the count of background asteroids.
## @return: Number of background asteroids.
func get_background_count() -> int:
	var count: int = 0
	for a in asteroids:
		if not a.is_major:
			count += 1
	return count
