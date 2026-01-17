## Terrain properties for solid-surface celestial bodies.
## Describes geological features and activity levels.
class_name TerrainProps
extends RefCounted


## Maximum elevation range in meters (highest peak to lowest valley).
var elevation_range_m: float

## Surface roughness (0 = smooth, 1 = extremely rough).
var roughness: float

## Crater density (0 = no craters, 1 = heavily cratered).
var crater_density: float

## Tectonic activity level (0 = dead, 1 = highly active).
var tectonic_activity: float

## Erosion level (0 = pristine, 1 = heavily eroded).
var erosion_level: float

## Terrain type classification for rendering.
var terrain_type: String


## Creates a new TerrainProps instance.
## @param p_elevation_range_m: Elevation range in meters.
## @param p_roughness: Surface roughness (0-1).
## @param p_crater_density: Crater density (0-1).
## @param p_tectonic_activity: Tectonic activity level (0-1).
## @param p_erosion_level: Erosion level (0-1).
## @param p_terrain_type: Terrain type classification.
func _init(
	p_elevation_range_m: float = 0.0,
	p_roughness: float = 0.5,
	p_crater_density: float = 0.0,
	p_tectonic_activity: float = 0.0,
	p_erosion_level: float = 0.0,
	p_terrain_type: String = ""
) -> void:
	elevation_range_m = p_elevation_range_m
	roughness = p_roughness
	crater_density = p_crater_density
	tectonic_activity = p_tectonic_activity
	erosion_level = p_erosion_level
	terrain_type = p_terrain_type


## Returns whether the surface shows signs of geological activity.
## @return: True if tectonically active.
func is_geologically_active() -> bool:
	return tectonic_activity > 0.1


## Returns whether the surface is heavily cratered.
## @return: True if crater density is high.
func is_heavily_cratered() -> bool:
	return crater_density > 0.5


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"elevation_range_m": elevation_range_m,
		"roughness": roughness,
		"crater_density": crater_density,
		"tectonic_activity": tectonic_activity,
		"erosion_level": erosion_level,
		"terrain_type": terrain_type,
	}


## Creates a TerrainProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new TerrainProps instance.
static func from_dict(data: Dictionary) -> TerrainProps:
	var script: GDScript = load("res://src/domain/celestial/components/TerrainProps.gd") as GDScript
	return script.new(
		data.get("elevation_range_m", 0.0) as float,
		data.get("roughness", 0.5) as float,
		data.get("crater_density", 0.0) as float,
		data.get("tectonic_activity", 0.0) as float,
		data.get("erosion_level", 0.0) as float,
		data.get("terrain_type", "") as String
	) as TerrainProps
