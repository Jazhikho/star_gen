## Evaluates star density for elliptical galaxies using 3D Gaussian profile.
##
## Elliptical galaxies have smooth, symmetric 3D ellipsoidal distributions.
## Uses a Gaussian profile for consistent sampling and visualization.
## The Y axis is the minor axis (flattened direction).
class_name EllipticalDensityModel
extends DensityModelInterface


## The galaxy specification.
var _spec: GalaxySpec

## Scale radius (sigma) for the major axes (X, Z) in parsecs.
var _sigma_major: float

## Scale radius (sigma) for the minor axis (Y) in parsecs.
var _sigma_minor: float

## Axis ratio (b/a) - 1.0 = spherical, 0.3 = very flattened.
var _axis_ratio: float

## Peak density at center for proper normalization.
var _peak_density: float


## Creates the model from a galaxy specification.
## @param spec: Galaxy parameters.
func _init(spec: GalaxySpec) -> void:
	_spec = spec
	
	# Axis ratio: ellipticity = 1 - b/a, so b/a = 1 - ellipticity
	# E0 = spherical (ellipticity = 0), E7 = very flat (ellipticity = 0.7)
	_axis_ratio = 1.0 - spec.ellipticity
	if _axis_ratio < 0.3:
		_axis_ratio = 0.3
	
	# Scale radius based on galaxy size
	# Most stars within ~3 sigma, so sigma ≈ radius/3 gives good coverage
	_sigma_major = spec.radius_pc * 0.35
	_sigma_minor = _sigma_major * _axis_ratio
	
	# Peak density - ellipticals have concentrated cores
	_peak_density = spec.bulge_intensity * 2.0


## Returns the un-normalised density at a position in parsec-space.
## Uses 3D Gaussian with ellipsoidal shape.
## @param position: Galactic position (XZ = major axes, Y = minor axis).
## @return: Density >= 0.
func get_density(position: Vector3) -> float:
	# Compute Gaussian exponent for ellipsoidal shape
	var x_term: float = (position.x * position.x) / (2.0 * _sigma_major * _sigma_major)
	var y_term: float = (position.y * position.y) / (2.0 * _sigma_minor * _sigma_minor)
	var z_term: float = (position.z * position.z) / (2.0 * _sigma_major * _sigma_major)
	
	var exponent: float = - (x_term + y_term + z_term)
	exponent = clampf(exponent, -30.0, 0.0)
	
	return _peak_density * exp(exponent)


## Returns the maximum density (at center) for normalization purposes.
## @return: Peak density value.
func get_peak_density() -> float:
	return _peak_density


## Returns the effective radius (sigma for major axis).
## @return: Effective radius in parsecs.
func get_effective_radius() -> float:
	return _sigma_major


## Returns the axis ratio (b/a).
## @return: Axis ratio.
func get_axis_ratio() -> float:
	return _axis_ratio
