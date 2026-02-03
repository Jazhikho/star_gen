## Specification for a procedurally generated galaxy.
## Pure data — no Nodes, no rendering.
class_name GalaxySpec
extends RefCounted


## Supported galaxy morphologies.
enum GalaxyType { SPIRAL, ELLIPTICAL, IRREGULAR }


## Master seed for the galaxy.
var seed: int = 0

## Morphological type.
var galaxy_type: GalaxyType = GalaxyType.SPIRAL

## Radius of the galactic disk in parsecs.
var radius_pc: float = 15000.0

## Half-height of the full extent (disk + bulge) in parsecs.
var height_pc: float = 1000.0

## Number of spiral arms.
var num_arms: int = 4

## Pitch angle of the logarithmic spiral arms in degrees.
var arm_pitch_angle_deg: float = 14.0

## Angular half-width of each arm in radians (Gaussian sigma).
var arm_width: float = 0.4

## Arm density contrast [0 = uniform disk, 1 = stars only on arms].
var arm_amplitude: float = 0.65

## Radius of the central bulge in parsecs (Gaussian sigma).
var bulge_radius_pc: float = 1500.0

## Half-height of the central bulge in parsecs (Gaussian sigma).
var bulge_height_pc: float = 800.0

## Peak intensity of the bulge relative to disk normalization.
var bulge_intensity: float = 0.8

## Exponential scale length of the disk in parsecs.
var disk_scale_length_pc: float = 4000.0

## Exponential scale height of the disk in parsecs.
var disk_scale_height_pc: float = 300.0


## Creates a Milky-Way-like spiral galaxy spec.
## @param galaxy_seed: Master seed for generation.
## @return: A configured GalaxySpec.
static func create_milky_way(galaxy_seed: int) -> GalaxySpec:
	var spec: GalaxySpec = GalaxySpec.new()
	spec.seed = galaxy_seed
	spec.galaxy_type = GalaxyType.SPIRAL
	spec.radius_pc = 15000.0
	spec.height_pc = 1000.0
	spec.num_arms = 4
	spec.arm_pitch_angle_deg = 14.0
	spec.arm_width = 0.4
	spec.arm_amplitude = 0.65
	spec.bulge_radius_pc = 1500.0
	spec.bulge_height_pc = 800.0
	spec.bulge_intensity = 0.8
	spec.disk_scale_length_pc = 4000.0
	spec.disk_scale_height_pc = 300.0
	return spec


## Creates a galaxy spec from a configuration and seed.
## @param config: GalaxyConfig with customization parameters.
## @param galaxy_seed: Master seed for generation.
## @return: A configured GalaxySpec.
static func create_from_config(config: GalaxyConfig, galaxy_seed: int) -> GalaxySpec:
	var spec: GalaxySpec = GalaxySpec.new()
	spec.seed = galaxy_seed
	spec.galaxy_type = config.galaxy_type as GalaxyType
	spec.radius_pc = config.radius_pc
	spec.height_pc = config.radius_pc / 15.0
	spec.num_arms = config.num_arms
	spec.arm_pitch_angle_deg = config.arm_pitch_angle_deg
	spec.arm_width = 0.4
	spec.arm_amplitude = config.arm_amplitude
	spec.bulge_radius_pc = config.bulge_radius_pc
	spec.bulge_height_pc = config.bulge_radius_pc * 0.53
	spec.bulge_intensity = config.bulge_intensity
	spec.disk_scale_length_pc = config.disk_scale_length_pc
	spec.disk_scale_height_pc = config.disk_scale_height_pc
	return spec
