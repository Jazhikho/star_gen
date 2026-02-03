## Importance-samples star positions from galaxy density models.
##
## Supports spiral, elliptical, and irregular galaxy types with
## type-specific sampling strategies for efficient generation.
##
## Key differences by type:
## - Spiral: Flat disk + spheroidal bulge with arm modulation
## - Elliptical: 3D ellipsoid (oblate spheroid), no disk
## - Irregular: 3D amorphous blob with noise modulation, no disk
class_name DensitySampler
extends RefCounted


## Safety cap: max rejection iterations per point before giving up.
const MAX_ATTEMPTS_PER_POINT: int = 30


## Samples a galaxy into bulge + disk point populations based on galaxy type.
## @param spec: Galaxy parameters.
## @param num_points: Total number of points to generate.
## @param rng: Seeded RNG for determinism.
## @return: A GalaxySample with separated populations.
static func sample_galaxy(
	spec: GalaxySpec,
	num_points: int,
	rng: RandomNumberGenerator
) -> GalaxySample:
	match spec.galaxy_type:
		GalaxySpec.GalaxyType.SPIRAL:
			return _sample_spiral_galaxy(spec, num_points, rng)
		GalaxySpec.GalaxyType.ELLIPTICAL:
			return _sample_elliptical_galaxy(spec, num_points, rng)
		GalaxySpec.GalaxyType.IRREGULAR:
			return _sample_irregular_galaxy(spec, num_points, rng)
		_:
			push_error("DensitySampler: Unknown galaxy type %d, falling back to spiral" % spec.galaxy_type)
			return _sample_spiral_galaxy(spec, num_points, rng)


## Samples a spiral galaxy with bulge and disk populations.
## Spirals have a flat disk with spiral arms and a central bulge.
static func _sample_spiral_galaxy(
	spec: GalaxySpec,
	num_points: int,
	rng: RandomNumberGenerator
) -> GalaxySample:
	var sample: GalaxySample = GalaxySample.new()
	var density_model: SpiralDensityModel = SpiralDensityModel.new(spec)
	
	var bulge_frac: float = _bulge_fraction_spiral(spec)
	var num_bulge: int = roundi(float(num_points) * bulge_frac)
	var num_disk: int = num_points - num_bulge
	
	sample.bulge_points = _sample_gaussian_bulge(spec, num_bulge, rng)
	sample.disk_points = _sample_spiral_disk(spec, density_model, num_disk, rng)
	
	return sample


## Samples an elliptical galaxy as a 3D oblate spheroid.
## Ellipticals have NO disk - they are pure 3D ellipsoids.
static func _sample_elliptical_galaxy(
	spec: GalaxySpec,
	num_points: int,
	rng: RandomNumberGenerator
) -> GalaxySample:
	var sample: GalaxySample = GalaxySample.new()
	var density_model: EllipticalDensityModel = EllipticalDensityModel.new(spec)
	
	# Elliptical galaxies are ALL bulge - no disk component
	sample.bulge_points = _sample_ellipsoid(spec, density_model, num_points, rng)
	sample.disk_points = PackedVector3Array()
	
	return sample


## Samples an irregular galaxy as a 3D amorphous blob.
## Irregulars have NO disk - they are noise-modulated 3D blobs.
static func _sample_irregular_galaxy(
	spec: GalaxySpec,
	num_points: int,
	rng: RandomNumberGenerator
) -> GalaxySample:
	var sample: GalaxySample = GalaxySample.new()
	var density_model: IrregularDensityModel = IrregularDensityModel.new(spec)
	
	# Split into "core" (warmer colored) and "outer" (bluer) for rendering variety
	# But both are sampled from the same 3D distribution
	var core_frac: float = 0.3
	var num_core: int = roundi(float(num_points) * core_frac)
	var num_outer: int = num_points - num_core
	
	# Core samples from inner region
	sample.bulge_points = _sample_irregular_blob(spec, density_model, num_core, rng, true)
	# Outer samples from full region  
	sample.disk_points = _sample_irregular_blob(spec, density_model, num_outer, rng, false)
	
	return sample


## Estimates the fraction of stars in the bulge for spirals.
static func _bulge_fraction_spiral(spec: GalaxySpec) -> float:
	var bulge_vol: float = (
		spec.bulge_intensity
		* TAU * spec.bulge_radius_pc * spec.bulge_radius_pc
		* sqrt(TAU) * spec.bulge_height_pc
	)
	
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


## Samples from a 3D Gaussian bulge distribution (for spiral centers).
static func _sample_gaussian_bulge(
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


## Samples from the spiral disk with arm rejection.
static func _sample_spiral_disk(
	spec: GalaxySpec,
	density_model: SpiralDensityModel,
	count: int,
	rng: RandomNumberGenerator
) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	points.resize(count)
	var accepted: int = 0
	var max_attempts: int = count * MAX_ATTEMPTS_PER_POINT
	
	var attempt: int = 0
	while accepted < count and attempt < max_attempts:
		attempt += 1
		
		# Sample radius from exponential disk profile
		var r: float = _sample_gamma2(spec.disk_scale_length_pc, rng)
		if r > spec.radius_pc:
			continue
		
		# Uniform angle
		var theta: float = rng.randf() * TAU
		
		# Height from Laplace distribution (thin disk)
		var h: float = _sample_laplace(spec.disk_scale_height_pc, rng)
		if absf(h) > spec.height_pc:
			continue
		
		var x: float = r * cos(theta)
		var z: float = r * sin(theta)
		
		# Reject based on arm modulation
		var arm_factor: float = density_model.get_arm_factor(r, x, z)
		if rng.randf() <= arm_factor:
			points[accepted] = Vector3(x, h, z)
			accepted += 1
	
	if accepted < count:
		points.resize(accepted)
	
	return points


## Samples from a 3D ellipsoid for elliptical galaxies.
## Uses direct Gaussian sampling matching the density model.
static func _sample_ellipsoid(
	_spec: GalaxySpec,
	density_model: EllipticalDensityModel,
	count: int,
	rng: RandomNumberGenerator
) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	points.resize(count)
	
	# Get sigma values from density model to ensure consistency
	var sigma_major: float = density_model.get_effective_radius()
	var sigma_minor: float = sigma_major * density_model.get_axis_ratio()
	
	for i in range(count):
		# Sample from 3D Gaussian with ellipsoidal shape
		var x: float = rng.randfn(0.0, sigma_major)
		var y: float = rng.randfn(0.0, sigma_minor)
		var z: float = rng.randfn(0.0, sigma_major)
		points[i] = Vector3(x, y, z)
	
	return points


## Samples from a 3D irregular blob shape.
## Uses spherical sampling with noise-based rejection.
static func _sample_irregular_blob(
	_spec: GalaxySpec,
	density_model: IrregularDensityModel,
	count: int,
	rng: RandomNumberGenerator,
	is_core: bool
) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	points.resize(count)
	var accepted: int = 0
	var max_attempts: int = count * MAX_ATTEMPTS_PER_POINT
	
	var scale_radius: float = density_model.get_scale_radius()
	var peak_density: float = density_model.get_peak_density()
	var center_offset: Vector3 = density_model.get_center_offset()
	
	# Core samples from smaller region
	var sample_scale: float = scale_radius * (0.6 if is_core else 1.5)
	var max_r: float = sample_scale * 4.0
	
	var attempt: int = 0
	while accepted < count and attempt < max_attempts:
		attempt += 1
		
		# Sample radius from exponential
		var r: float = _sample_gamma2(sample_scale, rng)
		if r > max_r:
			continue
		
		# Sample uniformly on sphere (3D, not disk!)
		var theta: float = rng.randf() * TAU
		var cos_phi: float = 2.0 * rng.randf() - 1.0
		var sin_phi: float = sqrt(1.0 - cos_phi * cos_phi)
		
		# Convert to 3D position
		var x: float = r * sin_phi * cos(theta)
		var y: float = r * cos_phi
		var z: float = r * sin_phi * sin(theta)
		
		# Apply center offset for asymmetry
		var position: Vector3 = Vector3(x, y, z) + center_offset
		
		# Rejection based on actual density
		var density: float = density_model.get_density(position)
		var acceptance_prob: float = clampf(density / peak_density, 0.0, 1.0)
		
		if rng.randf() <= acceptance_prob:
			points[accepted] = position
			accepted += 1
	
	if accepted < count:
		points.resize(accepted)
	
	return points


## Samples from Gamma(shape=2, scale) distribution.
static func _sample_gamma2(scale: float, rng: RandomNumberGenerator) -> float:
	var u1: float = maxf(rng.randf(), 1e-10)
	var u2: float = maxf(rng.randf(), 1e-10)
	return -scale * log(u1 * u2)


## Samples from Laplace(0, scale) distribution.
static func _sample_laplace(scale: float, rng: RandomNumberGenerator) -> float:
	var u: float = rng.randf() - 0.5
	var abs_2u: float = minf(2.0 * absf(u), 1.0 - 1e-10)
	return -scale * signf(u) * log(1.0 - abs_2u)
