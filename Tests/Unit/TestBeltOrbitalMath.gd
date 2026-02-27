## Unit tests for belt orbital math helpers.
extends TestCase

const _belt_orbital_math: GDScript = preload("res://src/domain/system/asteroid_belt/BeltOrbitalMath.gd")


## Circular orbit should preserve radius in XZ plane.
func test_orbital_elements_circular_flat_radius() -> void:
	var a: float = 3.0
	var pos: Vector3 = _belt_orbital_math.orbital_elements_to_position(a, 0.0, 0.0, 0.0, 0.0, PI / 3.0)
	var horizontal_radius: float = sqrt(pos.x * pos.x + pos.z * pos.z)
	assert_float_equal(horizontal_radius, a, 0.001, "Circular orbit keeps constant radius")
	assert_float_equal(pos.y, 0.0, 0.001, "Zero inclination keeps Y at zero")


## Mean anomaly should equal true anomaly when eccentricity is zero.
func test_mean_to_true_anomaly_for_circular_orbit() -> void:
	var mean_anomaly: float = 1.2
	var true_anomaly: float = _belt_orbital_math.mean_anomaly_to_true_anomaly(mean_anomaly, 0.0)
	assert_float_equal(true_anomaly, mean_anomaly, 0.0001, "Circular orbit anomaly identity")
