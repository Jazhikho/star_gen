## Evaluates star density at any point in a spiral galaxy.
##
## Density is the sum of a Gaussian bulge and an exponential disk
## modulated by logarithmic spiral arms.
## Position convention: XZ = galactic plane, Y = height above disk.
class_name SpiralDensityModel
extends RefCounted


var _spec: GalaxySpec
var _pitch_tan: float
var _arm_offsets: PackedFloat64Array


## Creates the model from a galaxy specification.
## @param spec: Galaxy parameters.
func _init(spec: GalaxySpec) -> void:
	_spec = spec
	_pitch_tan = tan(deg_to_rad(spec.arm_pitch_angle_deg))
	_arm_offsets = PackedFloat64Array()
	_arm_offsets.resize(spec.num_arms)
	for i in range(spec.num_arms):
		_arm_offsets[i] = float(i) * TAU / float(spec.num_arms)


## Returns the un-normalised density at a position in parsec-space.
## @param position: Galactic position (XZ plane, Y height).
## @return: Density >= 0 (not clamped to 1).
func get_density(position: Vector3) -> float:
	var r: float = _radial_distance(position)
	var h: float = position.y

	var bulge: float = _bulge_density(r, h)
	var disk: float = _disk_density(r, h)
	var arm_mod: float = _arm_factor(r, position.x, position.z)

	return maxf(bulge + disk * arm_mod, 0.0)


## Returns just the arm modulation factor at a position.
## Factor is in [1 - arm_amplitude, 1] where 1 = on-arm peak.
## @param r: Radial distance from center in the XZ plane.
## @param x: X coordinate (parsecs).
## @param z_pos: Z coordinate (parsecs, not galactic height).
## @return: Combined arm modulation factor.
func get_arm_factor(r: float, x: float, z_pos: float) -> float:
	return _arm_factor(r, x, z_pos)


## Radial distance in the galactic plane.
func _radial_distance(position: Vector3) -> float:
	return sqrt(position.x * position.x + position.z * position.z)


## Gaussian bulge centered at origin.
func _bulge_density(r: float, h: float) -> float:
	var r_norm: float = r / _spec.bulge_radius_pc
	var h_norm: float = h / _spec.bulge_height_pc
	return _spec.bulge_intensity * exp(-0.5 * (r_norm * r_norm + h_norm * h_norm))


## Exponential disk profile.
func _disk_density(r: float, h: float) -> float:
	return exp(-r / _spec.disk_scale_length_pc) * exp(-absf(h) / _spec.disk_scale_height_pc)


## Combined arm modulation factor including base inter-arm level.
func _arm_factor(r: float, x: float, z_pos: float) -> float:
	var base: float = 1.0 - _spec.arm_amplitude
	if r < 1.0:
		# Near center the arms are undefined; return full density
		return 1.0

	var arm_proximity: float = _peak_arm_proximity(r, x, z_pos)
	return base + _spec.arm_amplitude * arm_proximity


## Peak arm proximity [0, 1] — max over all arms of the Gaussian proximity.
## Returns how close the given point is to any spiral arm.
func _peak_arm_proximity(r: float, x: float, z_pos: float) -> float:
	var theta: float = atan2(z_pos, x)
	var log_r: float = log(r)
	var inv_2w2: float = 0.5 / (_spec.arm_width * _spec.arm_width)

	var best_proximity: float = 0.0
	for i in range(_arm_offsets.size()):
		var arm_theta: float = _arm_offsets[i] + log_r / _pitch_tan
		var delta: float = _wrap_angle(theta - arm_theta)
		var proximity: float = exp(-delta * delta * inv_2w2)
		if proximity > best_proximity:
			best_proximity = proximity
	return best_proximity


## Wraps an angle into [-PI, PI].
func _wrap_angle(angle: float) -> float:
	var a: float = fmod(angle + PI, TAU)
	if a < 0.0:
		a += TAU
	return a - PI
