## Generates asteroid belt field data from a specification and deterministic RNG.
class_name BeltFieldGenerator
extends RefCounted

const _belt_field_data: GDScript = preload("res://src/domain/system/asteroid_belt/BeltFieldData.gd")
const _belt_asteroid_data: GDScript = preload("res://src/domain/system/asteroid_belt/BeltAsteroidData.gd")
const _belt_orbital_math: GDScript = preload("res://src/domain/system/asteroid_belt/BeltOrbitalMath.gd")


## Generates one complete belt field.
## @param spec: Belt field specification.
## @param rng: Seeded RNG.
## @return: Generated field data.
func generate_field(spec: RefCounted, rng: SeededRng) -> RefCounted:
	var belt: RefCounted = _belt_field_data.new()
	belt.spec = spec
	belt.generation_seed = rng.get_initial_seed()

	for _i in range(spec.asteroid_count):
		var asteroid: RefCounted = _generate_single_asteroid(spec, rng)
		belt.asteroids.append(asteroid)

	var majors: Array = _process_major_asteroids(spec)
	for major in majors:
		belt.asteroids.append(major)

	return belt


## Generates one background asteroid sample.
func _generate_single_asteroid(
	spec: RefCounted,
	rng: SeededRng
) -> RefCounted:
	var asteroid: RefCounted = _belt_asteroid_data.new()
	asteroid.is_major = false

	asteroid.semi_major_axis_au = _sample_semi_major_axis(spec, rng)
	asteroid.eccentricity = _sample_eccentricity(spec, rng)
	asteroid.inclination_rad = _sample_inclination(spec, rng)
	asteroid.longitude_ascending_node_rad = rng.randf() * TAU
	asteroid.argument_periapsis_rad = rng.randf() * TAU

	var effective_longitude: float = _sample_effective_longitude(spec, rng)
	var raw_anomaly: float = effective_longitude - asteroid.argument_periapsis_rad
	asteroid.true_anomaly_rad = fmod(fmod(raw_anomaly, TAU) + TAU, TAU)
	asteroid.body_radius_km = _sample_body_size(spec, rng)

	asteroid.position_au = _belt_orbital_math.orbital_elements_to_position(
		asteroid.semi_major_axis_au,
		asteroid.eccentricity,
		asteroid.inclination_rad,
		asteroid.longitude_ascending_node_rad,
		asteroid.argument_periapsis_rad,
		asteroid.true_anomaly_rad
	)
	return asteroid


## Converts major inputs to generated asteroid data entries.
func _process_major_asteroids(spec: RefCounted) -> Array:
	var result: Array = []

	for input in spec.major_asteroid_inputs:
		var asteroid: RefCounted = _belt_asteroid_data.new()
		asteroid.is_major = true
		asteroid.body_id = input.body_id
		asteroid.body_radius_km = input.body_radius_km
		asteroid.asteroid_type = input.asteroid_type
		asteroid.semi_major_axis_au = input.semi_major_axis_m / _belt_orbital_math.AU_METERS
		asteroid.eccentricity = input.eccentricity
		asteroid.inclination_rad = deg_to_rad(input.inclination_deg)
		asteroid.longitude_ascending_node_rad = deg_to_rad(input.longitude_ascending_node_deg)
		asteroid.argument_periapsis_rad = deg_to_rad(input.argument_periapsis_deg)

		var mean_anomaly_rad: float = deg_to_rad(input.mean_anomaly_deg)
		asteroid.true_anomaly_rad = _belt_orbital_math.mean_anomaly_to_true_anomaly(
			mean_anomaly_rad, input.eccentricity
		)
		asteroid.position_au = _belt_orbital_math.orbital_elements_to_position(
			asteroid.semi_major_axis_au,
			asteroid.eccentricity,
			asteroid.inclination_rad,
			asteroid.longitude_ascending_node_rad,
			asteroid.argument_periapsis_rad,
			asteroid.true_anomaly_rad
		)
		result.append(asteroid)

	return result


func _sample_semi_major_axis(spec: RefCounted, rng: SeededRng) -> float:
	var belt_width: float = spec.outer_radius_au - spec.inner_radius_au
	var max_density: float = _radial_density(0.5, spec.radial_concentration)
	for _attempt in range(1000):
		var t: float = rng.randf()
		var threshold: float = rng.randf() * max_density
		var density: float = _radial_density(t, spec.radial_concentration)
		if threshold < density:
			var radius: float = spec.inner_radius_au + t * belt_width
			if not _is_in_gap(radius, spec):
				return radius
	return (spec.inner_radius_au + spec.outer_radius_au) * 0.5


func _radial_density(t: float, concentration: float) -> float:
	if t <= 0.0 or t >= 1.0:
		return 0.0
	return pow(t, concentration) * pow(1.0 - t, concentration)


func _is_in_gap(radius_au: float, spec: RefCounted) -> bool:
	var gap_count: int = mini(spec.gap_centers_au.size(), spec.gap_half_widths_au.size())
	for i in range(gap_count):
		if absf(radius_au - spec.gap_centers_au[i]) < spec.gap_half_widths_au[i]:
			return true
	return false


func _sample_eccentricity(spec: RefCounted, rng: SeededRng) -> float:
	var u: float = rng.randf()
	return spec.max_eccentricity * u * u


func _sample_inclination(spec: RefCounted, rng: SeededRng) -> float:
	var max_incl_rad: float = deg_to_rad(spec.max_inclination_deg)
	var u: float = rng.randf()
	return max_incl_rad * u * u


func _sample_effective_longitude(spec: RefCounted, rng: SeededRng) -> float:
	if spec.cluster_count <= 0 or spec.cluster_longitudes_rad.is_empty():
		return rng.randf() * TAU
	if rng.randf() < spec.cluster_fraction:
		var cluster_idx: int = rng.randi() % spec.cluster_longitudes_rad.size()
		var center: float = spec.cluster_longitudes_rad[cluster_idx]
		return _sample_von_mises(center, spec.cluster_concentration, rng)
	return rng.randf() * TAU


func _sample_von_mises(mu: float, kappa: float, rng: SeededRng) -> float:
	if kappa <= 0.0:
		return rng.randf() * TAU
	var std_dev: float = 1.0 / sqrt(kappa)
	var sample: float = rng.randfn(mu, std_dev)
	return fmod(fmod(sample, TAU) + TAU, TAU)


func _sample_body_size(spec: RefCounted, rng: SeededRng) -> float:
	var alpha: float = spec.size_power_law_exponent
	var r_min: float = spec.min_body_radius_km
	var r_max: float = spec.max_body_radius_km
	if absf(alpha - 1.0) < 0.001:
		var log_min: float = log(r_min)
		var log_max: float = log(r_max)
		return exp(log_min + rng.randf() * (log_max - log_min))

	var u: float = rng.randf()
	var exp_val: float = 1.0 - alpha
	var result: float = pow(
		(pow(r_max, exp_val) - pow(r_min, exp_val)) * u + pow(r_min, exp_val),
		1.0 / exp_val
	)
	return clampf(result, r_min, r_max)
