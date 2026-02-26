## Generates asteroid belt data from a specification and SeededRng.
## Uses orbital elements for physically-motivated placement:
## - Shaped radial distribution (beta-like profile concentrated at belt center)
## - Rejection sampling to enforce radial gaps
## - Power-law body size distribution
## - Optional von-Mises angular clustering for Trojan-like groups
## - Major asteroid inputs placed at their true orbital positions
class_name AsteroidBeltGeneratorConcept
extends RefCounted


## Generates a complete asteroid belt from a specification and RNG.
## Background asteroids are randomly distributed; major asteroid inputs
## are placed at their orbital-element-derived positions.
## @param spec: The belt generation specification.
## @param rng: SeededRng for deterministic output.
## @return: Fully populated AsteroidBeltDataConcept.
func generate_belt(
	spec: AsteroidBeltSpecConcept,
	rng: SeededRng
) -> AsteroidBeltDataConcept:
	var belt: AsteroidBeltDataConcept = AsteroidBeltDataConcept.new()
	belt.spec = spec
	belt.generation_seed = rng.get_initial_seed()

	# Generate background (visual-only) asteroids
	for _i in range(spec.asteroid_count):
		var asteroid: AsteroidDataConcept = _generate_single_asteroid(spec, rng)
		belt.asteroids.append(asteroid)

	# Process major asteroid inputs into positioned AsteroidDataConcept entries
	var majors: Array[AsteroidDataConcept] = _process_major_asteroids(spec)
	for major in majors:
		belt.asteroids.append(major)

	return belt


## Generates one background asteroid with random orbital elements.
## @param spec: The belt specification.
## @param rng: The seeded RNG.
## @return: A background AsteroidDataConcept with is_major == false.
func _generate_single_asteroid(
	spec: AsteroidBeltSpecConcept,
	rng: SeededRng
) -> AsteroidDataConcept:
	var asteroid: AsteroidDataConcept = AsteroidDataConcept.new()
	asteroid.is_major = false

	asteroid.semi_major_axis_au = _sample_semi_major_axis(spec, rng)
	asteroid.eccentricity = _sample_eccentricity(spec, rng)
	asteroid.inclination_rad = _sample_inclination(spec, rng)
	asteroid.longitude_ascending_node_rad = rng.randf() * TAU

	# Sample effective longitude (spatial angle), then derive true anomaly
	# so angular clustering controls actual position, not just orbit phase.
	asteroid.argument_periapsis_rad = rng.randf() * TAU
	var effective_longitude: float = _sample_effective_longitude(spec, rng)
	var raw_anomaly: float = effective_longitude - asteroid.argument_periapsis_rad
	asteroid.true_anomaly_rad = fmod(fmod(raw_anomaly, TAU) + TAU, TAU)

	asteroid.body_radius_km = _sample_body_size(spec, rng)

	asteroid.position_au = OrbitalMathConcept.orbital_elements_to_position(
		asteroid.semi_major_axis_au,
		asteroid.eccentricity,
		asteroid.inclination_rad,
		asteroid.longitude_ascending_node_rad,
		asteroid.argument_periapsis_rad,
		asteroid.true_anomaly_rad
	)

	return asteroid


## Converts each MajorAsteroidInputConcept into a positioned AsteroidDataConcept.
## Uses the Kepler equation to convert mean anomaly → true anomaly for placement.
## @param spec: The belt specification containing major_asteroid_inputs.
## @return: Array of major AsteroidDataConcept entries.
func _process_major_asteroids(
	spec: AsteroidBeltSpecConcept
) -> Array[AsteroidDataConcept]:
	var result: Array[AsteroidDataConcept] = []

	for input in spec.major_asteroid_inputs:
		var asteroid: AsteroidDataConcept = AsteroidDataConcept.new()
		asteroid.is_major = true
		asteroid.body_id = input.body_id
		asteroid.body_radius_km = input.body_radius_km
		asteroid.asteroid_type = input.asteroid_type

		asteroid.semi_major_axis_au = input.semi_major_axis_m / OrbitalMathConcept.AU_METERS
		asteroid.eccentricity = input.eccentricity
		asteroid.inclination_rad = deg_to_rad(input.inclination_deg)
		asteroid.longitude_ascending_node_rad = deg_to_rad(input.longitude_ascending_node_deg)
		asteroid.argument_periapsis_rad = deg_to_rad(input.argument_periapsis_deg)

		var mean_anomaly_rad: float = deg_to_rad(input.mean_anomaly_deg)
		asteroid.true_anomaly_rad = OrbitalMathConcept.mean_anomaly_to_true_anomaly(
			mean_anomaly_rad, input.eccentricity
		)

		asteroid.position_au = OrbitalMathConcept.orbital_elements_to_position(
			asteroid.semi_major_axis_au,
			asteroid.eccentricity,
			asteroid.inclination_rad,
			asteroid.longitude_ascending_node_rad,
			asteroid.argument_periapsis_rad,
			asteroid.true_anomaly_rad
		)

		result.append(asteroid)

	return result


## Samples semi-major axis using shaped radial density with gap rejection.
## @param spec: The belt specification.
## @param rng: The seeded RNG.
## @return: Semi-major axis in AU within [inner_radius, outer_radius], outside any gaps.
func _sample_semi_major_axis(
	spec: AsteroidBeltSpecConcept,
	rng: SeededRng
) -> float:
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


## Computes radial density at normalized belt position t in [0, 1].
## @param t: Normalized radial position (0 = inner edge, 1 = outer edge).
## @param concentration: Shape exponent. 0 = uniform, higher = more peaked.
## @return: Unnormalized density value.
func _radial_density(t: float, concentration: float) -> float:
	if t <= 0.0 or t >= 1.0:
		return 0.0
	return pow(t, concentration) * pow(1.0 - t, concentration)


## Checks whether a radius falls within any defined resonance gap.
## @param radius_au: The radius to test.
## @param spec: The belt specification containing gap definitions.
## @return: True if the radius is inside a gap.
func _is_in_gap(radius_au: float, spec: AsteroidBeltSpecConcept) -> bool:
	var gap_count: int = mini(spec.gap_centers_au.size(), spec.gap_half_widths_au.size())
	for i in range(gap_count):
		if absf(radius_au - spec.gap_centers_au[i]) < spec.gap_half_widths_au[i]:
			return true
	return false


## Samples orbital eccentricity biased toward low values.
## @param spec: The belt specification.
## @param rng: The seeded RNG.
## @return: Eccentricity in [0, max_eccentricity].
func _sample_eccentricity(
	spec: AsteroidBeltSpecConcept,
	rng: SeededRng
) -> float:
	var u: float = rng.randf()
	return spec.max_eccentricity * u * u


## Samples orbital inclination biased toward low values (thin belt).
## @param spec: The belt specification.
## @param rng: The seeded RNG.
## @return: Inclination in radians in [0, max_inclination].
func _sample_inclination(
	spec: AsteroidBeltSpecConcept,
	rng: SeededRng
) -> float:
	var max_incl_rad: float = deg_to_rad(spec.max_inclination_deg)
	var u: float = rng.randf()
	return max_incl_rad * u * u


## Samples effective longitude with optional angular clustering.
## @param spec: The belt specification.
## @param rng: The seeded RNG.
## @return: Effective longitude in radians [0, TAU).
func _sample_effective_longitude(
	spec: AsteroidBeltSpecConcept,
	rng: SeededRng
) -> float:
	if spec.cluster_count <= 0 or spec.cluster_longitudes_rad.is_empty():
		return rng.randf() * TAU

	if rng.randf() < spec.cluster_fraction:
		var cluster_idx: int = rng.randi() % spec.cluster_longitudes_rad.size()
		var center: float = spec.cluster_longitudes_rad[cluster_idx]
		return _sample_von_mises(center, spec.cluster_concentration, rng)
	else:
		return rng.randf() * TAU


## Approximates von Mises sampling using a wrapped normal distribution.
## @param mu: Center angle in radians.
## @param kappa: Concentration parameter (higher = tighter).
## @param rng: The seeded RNG.
## @return: Sampled angle in [0, TAU).
func _sample_von_mises(
	mu: float,
	kappa: float,
	rng: SeededRng
) -> float:
	if kappa <= 0.0:
		return rng.randf() * TAU
	var std_dev: float = 1.0 / sqrt(kappa)
	var sample: float = rng.randfn(mu, std_dev)
	sample = fmod(fmod(sample, TAU) + TAU, TAU)
	return sample


## Samples asteroid body radius using inverse-CDF power law distribution.
## @param spec: The belt specification.
## @param rng: The seeded RNG.
## @return: Body radius in km within [min_body_radius, max_body_radius].
func _sample_body_size(
	spec: AsteroidBeltSpecConcept,
	rng: SeededRng
) -> float:
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
