## Generated asteroid belt field data including background and major objects.
class_name BeltFieldData
extends RefCounted


## All generated asteroids in this field.
var asteroids: Array = []

## Spec used to generate this field.
var spec: RefCounted = null

## Seed used for field generation.
var generation_seed: int = 0

## Generator version string.
var generator_version: String = "belt-field-1.0"


## Returns only major asteroids.
## @return: Array of major asteroids.
func get_major_asteroids() -> Array:
	var result: Array = []
	for asteroid in asteroids:
		if asteroid.is_major:
			result.append(asteroid)
	return result


## Returns only background asteroids.
## @return: Array of background asteroids.
func get_background_asteroids() -> Array:
	var result: Array = []
	for asteroid in asteroids:
		if not asteroid.is_major:
			result.append(asteroid)
	return result
