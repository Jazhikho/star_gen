## Hydrosphere properties for bodies with liquid water.
## Covers oceans, lakes, and surface water features.
class_name HydrosphereProps
extends RefCounted


## Fraction of surface covered by liquid water (0-1).
var ocean_coverage: float

## Average ocean depth in meters.
var ocean_depth_m: float

## Fraction of water surface covered by ice (0-1).
var ice_coverage: float

## Ocean salinity in parts per thousand (Earth's oceans ~35).
var salinity_ppt: float

## Water composition type identifier.
var water_type: String


## Creates a new HydrosphereProps instance.
## @param p_ocean_coverage: Surface coverage fraction (0-1).
## @param p_ocean_depth_m: Average depth in meters.
## @param p_ice_coverage: Ice coverage fraction (0-1).
## @param p_salinity_ppt: Salinity in parts per thousand.
## @param p_water_type: Water composition type.
func _init(
	p_ocean_coverage: float = 0.0,
	p_ocean_depth_m: float = 0.0,
	p_ice_coverage: float = 0.0,
	p_salinity_ppt: float = 0.0,
	p_water_type: String = "water"
) -> void:
	ocean_coverage = p_ocean_coverage
	ocean_depth_m = p_ocean_depth_m
	ice_coverage = p_ice_coverage
	salinity_ppt = p_salinity_ppt
	water_type = p_water_type


## Returns the fraction of surface that is liquid (not ice).
## @return: Liquid water coverage fraction.
func get_liquid_coverage() -> float:
	return ocean_coverage * (1.0 - ice_coverage)


## Returns whether this body qualifies as an ocean world.
## @return: True if ocean coverage exceeds 90%.
func is_ocean_world() -> bool:
	return ocean_coverage > 0.9


## Returns whether the oceans are mostly frozen.
## @return: True if ice coverage exceeds 80%.
func is_frozen() -> bool:
	return ice_coverage > 0.8


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"ocean_coverage": ocean_coverage,
		"ocean_depth_m": ocean_depth_m,
		"ice_coverage": ice_coverage,
		"salinity_ppt": salinity_ppt,
		"water_type": water_type,
	}


## Creates a HydrosphereProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new HydrosphereProps instance.
static func from_dict(data: Dictionary) -> HydrosphereProps:
	var script: GDScript = load("res://src/domain/celestial/components/HydrosphereProps.gd") as GDScript
	return script.new(
		data.get("ocean_coverage", 0.0) as float,
		data.get("ocean_depth_m", 0.0) as float,
		data.get("ice_coverage", 0.0) as float,
		data.get("salinity_ppt", 0.0) as float,
		data.get("water_type", "water") as String
	) as HydrosphereProps
