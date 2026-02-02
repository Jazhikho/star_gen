## Importance-samples star positions from a spiral galaxy density model.
##
## Uses direct Gaussian sampling for the bulge (no rejection) and
## exponential-profile sampling with arm-only rejection for the disk,
## giving ~50-70 % acceptance on the disk pass.
class_name DensitySampler
extends RefCounted


## Safety cap: max rejection iterations per disk point before giving up.
const MAX_DISK_ATTEMPTS_PER_POINT: int = 20


## Samples a galaxy into bulge + disk point populations.
## @param spec: Galaxy parameters (used for proposal distributions).
## @param density_model: The density model (used for arm rejection).
## @param num_points: Total number of points to generate.
## @param rng: Seeded RNG for determinism.
## @return: A GalaxySample with separated populations.
static func sample_galaxy(
	spec: GalaxySpec,
	density_model: SpiralDensityModel,
	num_points: int,
	rng: RandomNumberGenerator
) -> GalaxySample:
	var sample: GalaxySample = GalaxySample.new()

	var bulge_frac: float = _bulge_fraction(spec)
	var num_bulge: int = roundi(float(num_points) * bulge_frac)
	var num_disk: int = num_points - num_bulge

	sample.bulge_points = _sample_bulge(spec, num_bulge, rng)
	sample.disk_points = _sample_disk(spec, density_model, num_disk, rng)

	return sample


## Estimates the fraction of total luminosity in the bulge.
## Uses analytic integrals of the Gaussian bulge vs exponential disk.
## @param spec: Galaxy parameters.
## @return: Fraction in [0.05, 0.50].
static func _bulge_fraction(spec: GalaxySpec) -> float:
	# Integral of 3D Gaussian: I * (2pi * rb^2) * (sqrt(2pi) * hb)
	var bulge_vol: float = (
		spec.bulge_intensity
		* TAU * spec.bulge_radius_pc * spec.bulge_radius_pc
		* sqrt(TAU) * spec.bulge_height_pc
	)

	# Integral of exponential disk: 2pi * s^2 * 2*sh, scaled by avg arm modulation
	var avg_arm_mod: float = 1.0 - spec.arm_amplitude * 0.4
	var disk_vol: float = (
		avg_arm_mod
		* TAU * spec.disk_scale_length_pc * spec.disk_scale_length_pc
		* 2.0 * spec.disk_scale_height_pc
	)

	var total: float = bulge_vol + disk_vol
	if total <= 0.0:
		return 0.5
	return clampf(bulge_vol / total, 0.05, 0.50)


## Directly samples from the 3D Gaussian bulge (no rejection needed).
## @param spec: Galaxy parameters.
## @param count: Number of points to generate.
## @param rng: Seeded RNG.
## @return: Packed array of positions.
static func _sample_bulge(
	spec: GalaxySpec,
	count: int,
	rng: RandomNumberGenerator
) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	points.resize(count)

	for i in range(count):
		var x: float = rng.randfn(0.0, spec.bulge_radius_pc)
		var y: float = rng.randfn(0.0, spec.bulge_height_pc)
		var z: float = rng.randfn(0.0, spec.bulge_radius_pc)
		points[i] = Vector3(x, y, z)

	return points


## Samples from the exponential disk with arm-rejection.
## Proposal: Gamma(2, scale_length) for r, uniform theta, Laplace for h.
## Rejection: accept based on arm modulation factor.
## @param spec: Galaxy parameters.
## @param density_model: Used for arm factor evaluation.
## @param count: Target number of disk points.
## @param rng: Seeded RNG.
## @return: Packed array of positions.
static func _sample_disk(
	spec: GalaxySpec,
	density_model: SpiralDensityModel,
	count: int,
	rng: RandomNumberGenerator
) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	points.resize(count)
	var accepted: int = 0
	var max_attempts: int = count * MAX_DISK_ATTEMPTS_PER_POINT

	var attempt: int = 0
	while accepted < count and attempt < max_attempts:
		attempt += 1

		# Radius from Gamma(2, scale_length) — matches r*exp(-r/s) profile
		var r: float = _sample_gamma2(spec.disk_scale_length_pc, rng)
		if r > spec.radius_pc:
			continue

		# Angle uniform
		var theta: float = rng.randf() * TAU

		# Height from Laplace(0, scale_height)
		var h: float = _sample_laplace(spec.disk_scale_height_pc, rng)
		if absf(h) > spec.height_pc:
			continue

		# Convert to Cartesian
		var x: float = r * cos(theta)
		var z: float = r * sin(theta)

		# Reject based on arm modulation only (proposal already matches disk profile)
		var arm_factor: float = density_model.get_arm_factor(r, x, z)
		# arm_factor is in [1-amp, 1]; accept with probability arm_factor / 1.0
		if rng.randf() <= arm_factor:
			points[accepted] = Vector3(x, h, z)
			accepted += 1

	# Trim if we didn't fill all slots
	if accepted < count:
		points.resize(accepted)

	return points


## Samples from Gamma(shape=2, scale) distribution.
## Equivalent to the sum of two independent Exp(scale) variates.
## @param scale: Scale parameter.
## @param rng: RNG.
## @return: A positive sample.
static func _sample_gamma2(scale: float, rng: RandomNumberGenerator) -> float:
	var u1: float = maxf(rng.randf(), 1e-10)
	var u2: float = maxf(rng.randf(), 1e-10)
	return -scale * log(u1 * u2)


## Samples from Laplace(0, scale) distribution.
## @param scale: Scale parameter (b in the standard parameterisation).
## @param rng: RNG.
## @return: A sample (can be negative).
static func _sample_laplace(scale: float, rng: RandomNumberGenerator) -> float:
	var u: float = rng.randf() - 0.5
	var abs_2u: float = minf(2.0 * absf(u), 1.0 - 1e-10)
	return -scale * signf(u) * log(1.0 - abs_2u)
