## Data structure for galaxy save files.
## Contains all information needed to restore a galaxy viewer state.
## Note: Generated systems are not persisted; they are regenerated on demand
## from deterministic seeds when the galaxy is loaded.
class_name GalaxySaveData
extends RefCounted

## Current save format version.
const FORMAT_VERSION: int = 1

## Script reference for self-instantiation (avoids class_name resolution order when loaded as dependency).
const _SCRIPT: GDScript = preload("res://src/domain/galaxy/GalaxySaveData.gd")

## File format version.
var version: int = FORMAT_VERSION

## Galaxy seed.
var galaxy_seed: int = 42

## Timestamp when saved (Unix time).
var saved_at: int = 0

## Current zoom level.
var zoom_level: int = GalaxyCoordinates.ZoomLevel.SUBSECTOR

## Selected quadrant coordinates (null if none).
var selected_quadrant: Variant = null

## Selected sector coordinates (null if none).
var selected_sector: Variant = null

## Star camera position (for subsector view).
var camera_position: Vector3 = Vector3.ZERO

## Star camera rotation (for subsector view).
var camera_rotation: Vector3 = Vector3.ZERO

## Whether a star is selected.
var has_star_selection: bool = false

## Selected star seed (if any).
var selected_star_seed: int = 0

## Selected star position (if any).
var selected_star_position: Vector3 = Vector3.ZERO

## Galaxy configuration dictionary (serialized GalaxyConfig).
var galaxy_config_data: Dictionary = {}

## Number of cached systems at save time (informational only).
## Systems themselves are not persisted; they regenerate deterministically.
var cached_system_count: int = 0

## Serialized JumpLaneRegion (null if no routes calculated).
var jump_lane_region_data: Dictionary = {}

## Serialized JumpLaneResult (null if no routes calculated).
var jump_lane_result_data: Dictionary = {}

## Edited-body overrides: star_seed -> { body_id -> serialized body dict }.
## Stored as a raw Dictionary (GalaxyBodyOverrides.to_dict output). Empty = no overrides.
var body_overrides_data: Dictionary = {}


## Returns this script for self-instantiation (avoids class_name resolution order when loaded as dependency).
static func _script_ref() -> GDScript:
	return _SCRIPT


## Creates a new save data with the given timestamp (caller provides time so domain stays engine-free).
## @param timestamp: Unix time when the save was created.
## @return: New GalaxySaveData instance (RefCounted at type level to avoid load-order issues).
static func create(timestamp: int) -> RefCounted:
	var data: RefCounted = _script_ref().new()
	data.set("saved_at", timestamp)
	return data


## Validates the save data.
## @return: True if data is valid for save/load.
func is_valid() -> bool:
	if galaxy_seed == 0:
		return false
	if zoom_level < GalaxyCoordinates.ZoomLevel.GALAXY or \
	   zoom_level > GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		return false
	return true


## Serializes the save data to a dictionary.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var dict: Dictionary = {
		"version": version,
		"galaxy_seed": galaxy_seed,
		"saved_at": saved_at,
		"zoom_level": zoom_level,
		"camera_position": _vector3_to_array(camera_position),
		"camera_rotation": _vector3_to_array(camera_rotation),
		"has_star_selection": has_star_selection,
		"selected_star_seed": selected_star_seed,
		"selected_star_position": _vector3_to_array(selected_star_position),
	}

	if selected_quadrant != null:
		dict["selected_quadrant"] = _vector3i_to_array(selected_quadrant as Vector3i)
	else:
		dict["selected_quadrant"] = null

	if selected_sector != null:
		dict["selected_sector"] = _vector3i_to_array(selected_sector as Vector3i)
	else:
		dict["selected_sector"] = null

	dict["galaxy_config_data"] = galaxy_config_data
	dict["cached_system_count"] = cached_system_count
	dict["jump_lane_region_data"] = jump_lane_region_data
	dict["jump_lane_result_data"] = jump_lane_result_data
	dict["body_overrides_data"] = body_overrides_data
	return dict


## Deserializes save data from a dictionary.
## @param dict: Dictionary to deserialize from.
## @return: GalaxySaveData instance (RefCounted), or null if invalid.
static func from_dict(dict: Dictionary) -> Variant:
	if not dict.has("version") or not dict.has("galaxy_seed"):
		return null

	var data: RefCounted = _script_ref().new()

	data.version = dict.get("version", FORMAT_VERSION) as int
	data.galaxy_seed = dict.get("galaxy_seed", 42) as int
	data.saved_at = dict.get("saved_at", 0) as int
	data.zoom_level = dict.get("zoom_level", GalaxyCoordinates.ZoomLevel.SUBSECTOR) as int

	var cam_pos: Variant = dict.get("camera_position")
	if cam_pos is Array:
		data.camera_position = _array_to_vector3(cam_pos as Array)

	var cam_rot: Variant = dict.get("camera_rotation")
	if cam_rot is Array:
		data.camera_rotation = _array_to_vector3(cam_rot as Array)

	var quad: Variant = dict.get("selected_quadrant")
	if quad is Array:
		data.selected_quadrant = _array_to_vector3i(quad as Array)
	else:
		data.selected_quadrant = null

	var sect: Variant = dict.get("selected_sector")
	if sect is Array:
		data.selected_sector = _array_to_vector3i(sect as Array)
	else:
		data.selected_sector = null

	data.has_star_selection = dict.get("has_star_selection", false) as bool
	data.selected_star_seed = dict.get("selected_star_seed", 0) as int

	var star_pos: Variant = dict.get("selected_star_position")
	if star_pos is Array:
		data.selected_star_position = _array_to_vector3(star_pos as Array)

	var config_dict: Variant = dict.get("galaxy_config_data")
	if config_dict is Dictionary:
		data.galaxy_config_data = config_dict
	else:
		data.galaxy_config_data = {}
	data.cached_system_count = dict.get("cached_system_count", 0) as int

	var jl_region: Variant = dict.get("jump_lane_region_data")
	data.jump_lane_region_data = jl_region if jl_region is Dictionary else {}

	var jl_result: Variant = dict.get("jump_lane_result_data")
	data.jump_lane_result_data = jl_result if jl_result is Dictionary else {}

	var overrides: Variant = dict.get("body_overrides_data")
	data.body_overrides_data = overrides if overrides is Dictionary else {}

	return data


## Converts Vector3 to array for JSON serialization.
## @param v: Vector3 to convert.
## @return: Array of 3 floats.
static func _vector3_to_array(v: Vector3) -> Array:
	return [v.x, v.y, v.z]


## Converts array to Vector3.
## @param arr: Array of 3 floats.
## @return: Vector3.
static func _array_to_vector3(arr: Array) -> Vector3:
	if arr.size() < 3:
		return Vector3.ZERO
	return Vector3(arr[0] as float, arr[1] as float, arr[2] as float)


## Converts Vector3i to array for JSON serialization.
## @param v: Vector3i to convert.
## @return: Array of 3 ints.
static func _vector3i_to_array(v: Vector3i) -> Array:
	return [v.x, v.y, v.z]


## Converts array to Vector3i.
## @param arr: Array of 3 ints.
## @return: Vector3i.
static func _array_to_vector3i(arr: Array) -> Vector3i:
	if arr.size() < 3:
		return Vector3i.ZERO
	return Vector3i(arr[0] as int, arr[1] as int, arr[2] as int)


## Returns a human-readable summary of the save data (timestamp as number; app may format for display).
## @return: Summary string.
func get_summary() -> String:
	var zoom_name: String = _get_zoom_name(zoom_level)
	return "Seed %d, %s view, saved %d" % [galaxy_seed, zoom_name, saved_at]


## Returns whether this save data has a galaxy config.
## @return: True if config data is present.
func has_config() -> bool:
	return not galaxy_config_data.is_empty()


## Returns the galaxy config from save data.
## @return: GalaxyConfig or null if not present.
func get_config() -> GalaxyConfig:
	if galaxy_config_data.is_empty():
		return null
	return GalaxyConfig.from_dict(galaxy_config_data)


## Sets the galaxy config in save data.
## @param config: GalaxyConfig to store.
func set_config(config: GalaxyConfig) -> void:
	if config != null:
		galaxy_config_data = config.to_dict()
	else:
		galaxy_config_data = {}


## Returns whether this save has any body overrides.
## @return: True if at least one body has been edited.
func has_body_overrides() -> bool:
	return not body_overrides_data.is_empty()


## Returns a GalaxyBodyOverrides view of the stored override data.
## @return: New GalaxyBodyOverrides instance (empty if no overrides). Caller may cast to GalaxyBodyOverrides.
func get_body_overrides() -> RefCounted:
	var ScriptClass: GDScript = load("res://src/domain/galaxy/GalaxyBodyOverrides.gd") as GDScript
	if body_overrides_data.is_empty():
		return ScriptClass.new()
	return ScriptClass.from_dict(body_overrides_data)


## Stores a GalaxyBodyOverrides into the save data.
## @param overrides: Overrides to store (null clears). Pass a GalaxyBodyOverrides instance.
func set_body_overrides(overrides: Variant) -> void:
	if overrides == null:
		body_overrides_data = {}
		return
	if overrides.has_method("is_empty") and overrides.is_empty():
		body_overrides_data = {}
		return
	if overrides.has_method("to_dict"):
		body_overrides_data = overrides.to_dict()
	else:
		body_overrides_data = {}


## Returns zoom level name.
## @param level: Zoom level.
## @return: Name string.
static func _get_zoom_name(level: int) -> String:
	match level:
		GalaxyCoordinates.ZoomLevel.GALAXY:
			return "Galaxy"
		GalaxyCoordinates.ZoomLevel.QUADRANT:
			return "Quadrant"
		GalaxyCoordinates.ZoomLevel.SECTOR:
			return "Sector"
		GalaxyCoordinates.ZoomLevel.SUBSECTOR:
			return "Star Field"
		_:
			return "Unknown"
