## Interface for galaxy density models.
##
## All density models (Spiral, Elliptical, Irregular) implement this interface.
## This allows uniform handling throughout the codebase.
class_name DensityModelInterface
extends RefCounted


## Returns the un-normalised density at a position in parsec-space.
## @param _position: Galactic position (XZ plane, Y height).
## @return: Density >= 0 (not clamped to 1).
func get_density(_position: Vector3) -> float:
	push_error("DensityModelInterface.get_density() must be overridden")
	return 0.0


## Returns an estimate of the maximum density for normalization.
## @return: Peak density estimate.
func get_peak_density() -> float:
	push_error("DensityModelInterface.get_peak_density() must be overridden")
	return 1.0


## Returns the arm modulation factor at a position (only meaningful for spirals).
## For non-spiral galaxies, returns 1.0 (no arm modulation).
## @param _r: Radial distance from center.
## @param _x: X coordinate.
## @param _z_pos: Z coordinate.
## @return: Arm factor in [0, 1].
func get_arm_factor(_r: float, _x: float, _z_pos: float) -> float:
	# Default: no arm modulation for non-spiral types
	return 1.0


## Creates the appropriate density model for a galaxy spec.
## @param spec: Galaxy specification.
## @return: A density model instance appropriate for the galaxy type.
static func create_for_spec(spec: GalaxySpec) -> DensityModelInterface:
	match spec.galaxy_type:
		GalaxySpec.GalaxyType.SPIRAL:
			return SpiralDensityModel.new(spec)
		GalaxySpec.GalaxyType.ELLIPTICAL:
			return EllipticalDensityModel.new(spec)
		GalaxySpec.GalaxyType.IRREGULAR:
			return IrregularDensityModel.new(spec)
		_:
			push_error("DensityModelInterface: Unknown galaxy type %d" % spec.galaxy_type)
			return SpiralDensityModel.new(spec)
