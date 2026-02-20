## Evaluates star density for irregular galaxies using noise-based distribution.
##
## Irregular galaxies have asymmetric, clumpy, 3D distributions without clear structure.
## Unlike spirals and ellipticals, they don't have a disk - they're amorphous blobs.
## Uses layered noise to create multiple density peaks and asymmetry.
class_name IrregularDensityModel
extends DensityModelInterface


## The galaxy specification.
var _spec: GalaxySpec

## FastNoiseLite for large-scale structure variation.
var _structure_noise: FastNoiseLite

## Secondary noise for clumps (star-forming regions).
var _clump_noise: FastNoiseLite

## Tertiary noise for asymmetry.
var _asymmetry_noise: FastNoiseLite

## Random offset for overall asymmetry.
var _center_offset: Vector3

## Peak density for normalization.
var _peak_density: float

## Scale radius for base falloff.
var _scale_radius: float


## Creates the model from a galaxy specification.
## @param spec: Galaxy parameters.
func _init(spec: GalaxySpec) -> void:
	_spec = spec
	_peak_density = spec.bulge_intensity * 1.5
	
	# Irregular galaxies are typically smaller and more compact
	_scale_radius = spec.radius_pc * 0.5
	
	# Main structure noise - creates the overall blob shape
	_structure_noise = FastNoiseLite.new()
	_structure_noise.seed = spec.galaxy_seed
	_structure_noise.noise_type = FastNoiseLite.TYPE_SIMPLEX_SMOOTH
	_structure_noise.frequency = 0.0002 * spec.irregularity_scale
	_structure_noise.fractal_type = FastNoiseLite.FRACTAL_FBM
	_structure_noise.fractal_octaves = 3
	_structure_noise.fractal_lacunarity = 2.0
	_structure_noise.fractal_gain = 0.5
	
	# Clump noise - creates star-forming regions
	_clump_noise = FastNoiseLite.new()
	_clump_noise.seed = spec.galaxy_seed + 1000
	_clump_noise.noise_type = FastNoiseLite.TYPE_CELLULAR
	_clump_noise.frequency = 0.0005 * spec.irregularity_scale
	_clump_noise.cellular_distance_function = FastNoiseLite.DISTANCE_EUCLIDEAN
	_clump_noise.cellular_return_type = FastNoiseLite.RETURN_DISTANCE2_DIV
	
	# Asymmetry noise - distorts the shape
	_asymmetry_noise = FastNoiseLite.new()
	_asymmetry_noise.seed = spec.galaxy_seed + 2000
	_asymmetry_noise.noise_type = FastNoiseLite.TYPE_PERLIN
	_asymmetry_noise.frequency = 0.00015 * spec.irregularity_scale
	
	# Generate random center offset based on seed
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = spec.galaxy_seed
	var offset_scale: float = spec.radius_pc * 0.15 * spec.irregularity_scale
	_center_offset = Vector3(
		rng.randf_range(-offset_scale, offset_scale),
		rng.randf_range(-offset_scale, offset_scale),
		rng.randf_range(-offset_scale, offset_scale)
	)


## Returns the un-normalised density at a position in parsec-space.
## @param position: Galactic position.
## @return: Density >= 0.
func get_density(position: Vector3) -> float:
	# Apply center offset for asymmetry
	var shifted: Vector3 = position - _center_offset
	
	# Base spherical falloff from shifted center
	var r: float = shifted.length()
	var base_density: float = _compute_base_falloff(r)
	
	# Apply structure noise to create blob-like shape
	var structure_mod: float = _get_structure_modulation(position)
	
	# Apply clump enhancement for star-forming regions
	var clump_boost: float = _get_clump_boost(position)
	
	# Combine: base * structure * (1 + clump_boost)
	var density: float = base_density * structure_mod * (1.0 + clump_boost * 0.5)
	
	return maxf(density, 0.0)


## Returns the maximum density for normalization.
## @return: Peak density estimate.
func get_peak_density() -> float:
	return _peak_density


## Returns the center offset vector.
## @return: Offset vector in parsecs.
func get_center_offset() -> Vector3:
	return _center_offset


## Returns the scale radius.
## @return: Scale radius in parsecs.
func get_scale_radius() -> float:
	return _scale_radius


## Computes the base radial falloff (3D spherical, not disk).
## @param r: Distance from center.
## @return: Base density value.
func _compute_base_falloff(r: float) -> float:
	# Use a soft exponential profile with extended tail
	# This creates a blob-like shape, not a disk
	var falloff: float = exp(-r / _scale_radius)
	
	# Add extended halo component
	var halo: float = 0.2 * exp(-r / (_scale_radius * 2.5))
	
	return _spec.bulge_intensity * (falloff + halo)


## Gets the structure modulation at a position.
## This distorts the spherical shape into an irregular blob.
## @param position: 3D position in parsecs.
## @return: Modulation factor in [0.3, 1.0].
func _get_structure_modulation(position: Vector3) -> float:
	var noise_val: float = _structure_noise.get_noise_3d(position.x, position.y, position.z)
	
	# Add asymmetry distortion
	var asymmetry: float = _asymmetry_noise.get_noise_3d(
		position.x * 0.5, position.y * 0.5, position.z * 0.5
	)
	
	# Combine and remap to [0.3, 1.0]
	var combined: float = (noise_val + asymmetry * 0.3 + 1.0) * 0.5
	return clampf(0.3 + combined * 0.7, 0.3, 1.0)


## Gets the clump boost at a position (star-forming regions).
## @param position: 3D position in parsecs.
## @return: Boost factor in [0, 1].
func _get_clump_boost(position: Vector3) -> float:
	var noise_val: float = _clump_noise.get_noise_3d(position.x, position.y, position.z)
	# Cellular noise returns distance ratios; we want peaks at cell centers
	# The DISTANCE2DIV type gives lower values near cell centers
	var boost: float = 1.0 - clampf(noise_val * 0.5 + 0.5, 0.0, 1.0)
	return boost * boost # Square to make peaks sharper
