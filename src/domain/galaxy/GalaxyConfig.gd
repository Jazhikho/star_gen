## Configuration for galaxy generation parameters.
## Allows customization of galaxy appearance before generation.
## Pure data — no Nodes, no rendering.
class_name GalaxyConfig
extends RefCounted

## Galaxy morphological type (uses GalaxySpec.GalaxyType as single source of truth).
var galaxy_type: int = GalaxySpec.GalaxyType.SPIRAL

## Number of spiral arms (only applies to spiral galaxies).
var num_arms: int = 4

## Pitch angle of spiral arms in degrees (10-30, lower = tighter).
var arm_pitch_angle_deg: float = 14.0

## Arm density contrast (0.3-0.9, higher = more defined arms).
var arm_amplitude: float = 0.65

## Central bulge intensity (0.3-1.2, higher = brighter core).
var bulge_intensity: float = 0.8

## Disk scale length in parsecs (affects galaxy size, 2000-6000).
var disk_scale_length_pc: float = 4000.0

## Overall star density multiplier (0.5-2.0).
var star_density_multiplier: float = 1.0

## Disk scale height in parsecs (affects thickness, 200-500).
var disk_scale_height_pc: float = 300.0

## Bulge radius in parsecs (1000-2500).
var bulge_radius_pc: float = 1500.0

## Galaxy radius in parsecs (10000-25000).
var radius_pc: float = 15000.0

## Ellipticity for elliptical galaxies (0.0-0.7, 0 = spherical, 0.7 = very flat).
var ellipticity: float = 0.3

## Irregularity scale for irregular galaxies (0.1-1.0, controls noise frequency).
var irregularity_scale: float = 0.5


## Creates a default configuration (Milky Way-like).
## @return: New GalaxyConfig with default values.
static func create_default() -> GalaxyConfig:
	return GalaxyConfig.new()


## Creates a Milky Way-like configuration.
## @return: New GalaxyConfig matching Milky Way parameters.
static func create_milky_way() -> GalaxyConfig:
	var config: GalaxyConfig = GalaxyConfig.new()
	config.galaxy_type = GalaxySpec.GalaxyType.SPIRAL
	config.num_arms = 4
	config.arm_pitch_angle_deg = 14.0
	config.arm_amplitude = 0.65
	config.bulge_intensity = 0.8
	config.disk_scale_length_pc = 4000.0
	config.star_density_multiplier = 1.0
	config.disk_scale_height_pc = 300.0
	config.bulge_radius_pc = 1500.0
	config.radius_pc = 15000.0
	return config


## Applies this configuration to a GalaxySpec.
## @param spec: The GalaxySpec to modify.
func apply_to_spec(spec: GalaxySpec) -> void:
	spec.galaxy_type = galaxy_type as GalaxySpec.GalaxyType
	spec.num_arms = num_arms
	spec.arm_pitch_angle_deg = arm_pitch_angle_deg
	spec.arm_amplitude = arm_amplitude
	spec.bulge_intensity = bulge_intensity
	spec.disk_scale_length_pc = disk_scale_length_pc
	spec.disk_scale_height_pc = disk_scale_height_pc
	spec.bulge_radius_pc = bulge_radius_pc
	spec.radius_pc = radius_pc
	spec.ellipticity = ellipticity
	spec.irregularity_scale = irregularity_scale


## Validates the configuration values are within acceptable ranges.
## @return: True if all values are valid.
func is_valid() -> bool:
	if galaxy_type < GalaxySpec.GalaxyType.SPIRAL or galaxy_type > GalaxySpec.GalaxyType.IRREGULAR:
		return false
	if num_arms < 2 or num_arms > 6:
		return false
	if arm_pitch_angle_deg < 10.0 or arm_pitch_angle_deg > 30.0:
		return false
	if arm_amplitude < 0.3 or arm_amplitude > 0.9:
		return false
	if bulge_intensity < 0.3 or bulge_intensity > 1.2:
		return false
	if disk_scale_length_pc < 2000.0 or disk_scale_length_pc > 6000.0:
		return false
	if star_density_multiplier < 0.5 or star_density_multiplier > 2.0:
		return false
	if disk_scale_height_pc < 200.0 or disk_scale_height_pc > 500.0:
		return false
	if bulge_radius_pc < 1000.0 or bulge_radius_pc > 2500.0:
		return false
	if radius_pc < 10000.0 or radius_pc > 25000.0:
		return false
	if ellipticity < 0.0 or ellipticity > 0.7:
		return false
	if irregularity_scale < 0.1 or irregularity_scale > 1.0:
		return false
	return true


## Serializes the configuration to a dictionary.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"galaxy_type": galaxy_type,
		"num_arms": num_arms,
		"arm_pitch_angle_deg": arm_pitch_angle_deg,
		"arm_amplitude": arm_amplitude,
		"bulge_intensity": bulge_intensity,
		"disk_scale_length_pc": disk_scale_length_pc,
		"star_density_multiplier": star_density_multiplier,
		"disk_scale_height_pc": disk_scale_height_pc,
		"bulge_radius_pc": bulge_radius_pc,
		"radius_pc": radius_pc,
		"ellipticity": ellipticity,
		"irregularity_scale": irregularity_scale,
	}


## Deserializes configuration from a dictionary.
## @param dict: Dictionary to deserialize from.
## @return: GalaxyConfig instance, or null if invalid.
static func from_dict(dict: Dictionary) -> GalaxyConfig:
	if dict.is_empty():
		return null
	var config: GalaxyConfig = GalaxyConfig.new()
	config.galaxy_type = dict.get("galaxy_type", GalaxySpec.GalaxyType.SPIRAL) as int
	config.num_arms = dict.get("num_arms", 4) as int
	config.arm_pitch_angle_deg = dict.get("arm_pitch_angle_deg", 14.0) as float
	config.arm_amplitude = dict.get("arm_amplitude", 0.65) as float
	config.bulge_intensity = dict.get("bulge_intensity", 0.8) as float
	config.disk_scale_length_pc = dict.get("disk_scale_length_pc", 4000.0) as float
	config.star_density_multiplier = dict.get("star_density_multiplier", 1.0) as float
	config.disk_scale_height_pc = dict.get("disk_scale_height_pc", 300.0) as float
	config.bulge_radius_pc = dict.get("bulge_radius_pc", 1500.0) as float
	config.radius_pc = dict.get("radius_pc", 15000.0) as float
	config.ellipticity = dict.get("ellipticity", 0.3) as float
	config.irregularity_scale = dict.get("irregularity_scale", 0.5) as float
	return config


## Returns the galaxy type as a display string.
## @return: Human-readable type name.
func get_type_name() -> String:
	match galaxy_type:
		GalaxySpec.GalaxyType.SPIRAL:
			return "Spiral"
		GalaxySpec.GalaxyType.ELLIPTICAL:
			return "Elliptical"
		GalaxySpec.GalaxyType.IRREGULAR:
			return "Irregular"
		_:
			return "Unknown"
