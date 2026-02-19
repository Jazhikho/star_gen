## Cryosphere properties for bodies with significant ice features.
## Covers polar caps, permafrost, and subsurface ice.
class_name CryosphereProps
extends RefCounted


## Fraction of surface covered by polar ice caps (0-1).
var polar_cap_coverage: float

## Depth of permafrost layer in meters.
var permafrost_depth_m: float

## Whether a subsurface liquid ocean exists beneath ice.
var has_subsurface_ocean: bool

## Subsurface ocean depth in meters (if present).
var subsurface_ocean_depth_m: float

## Cryovolcanism activity level (0 = none, 1 = highly active).
var cryovolcanism_level: float

## Ice composition type identifier.
var ice_type: String


## Creates a new CryosphereProps instance.
## @param p_polar_cap_coverage: Polar cap coverage fraction (0-1).
## @param p_permafrost_depth_m: Permafrost depth in meters.
## @param p_has_subsurface_ocean: Whether subsurface ocean exists.
## @param p_subsurface_ocean_depth_m: Subsurface ocean depth in meters.
## @param p_cryovolcanism_level: Cryovolcanism activity level (0-1).
## @param p_ice_type: Ice composition type.
func _init(
	p_polar_cap_coverage: float = 0.0,
	p_permafrost_depth_m: float = 0.0,
	p_has_subsurface_ocean: bool = false,
	p_subsurface_ocean_depth_m: float = 0.0,
	p_cryovolcanism_level: float = 0.0,
	p_ice_type: String = "water_ice"
) -> void:
	polar_cap_coverage = p_polar_cap_coverage
	permafrost_depth_m = p_permafrost_depth_m
	has_subsurface_ocean = p_has_subsurface_ocean
	subsurface_ocean_depth_m = p_subsurface_ocean_depth_m
	cryovolcanism_level = p_cryovolcanism_level
	ice_type = p_ice_type


## Returns whether the body has significant ice features.
## @return: True if polar caps or permafrost present.
func has_significant_ice() -> bool:
	return polar_cap_coverage > 0.1 or permafrost_depth_m > 0.0


## Returns whether cryovolcanism is active.
## @return: True if cryovolcanism level is significant.
func is_cryovolcanically_active() -> bool:
	return cryovolcanism_level > 0.1


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"polar_cap_coverage": polar_cap_coverage,
		"permafrost_depth_m": permafrost_depth_m,
		"has_subsurface_ocean": has_subsurface_ocean,
		"subsurface_ocean_depth_m": subsurface_ocean_depth_m,
		"cryovolcanism_level": cryovolcanism_level,
		"ice_type": ice_type,
	}


## Creates a CryosphereProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new CryosphereProps instance.
static func from_dict(data: Dictionary) -> CryosphereProps:
	var script_class: GDScript = load("res://src/domain/celestial/components/CryosphereProps.gd") as GDScript
	return script_class.new(
		data.get("polar_cap_coverage", 0.0) as float,
		data.get("permafrost_depth_m", 0.0) as float,
		data.get("has_subsurface_ocean", false) as bool,
		data.get("subsurface_ocean_depth_m", 0.0) as float,
		data.get("cryovolcanism_level", 0.0) as float,
		data.get("ice_type", "water_ice") as String
	) as CryosphereProps
