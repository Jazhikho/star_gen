## Service for saving and loading galaxy data.
## Handles file I/O and serialization.
class_name GalaxyPersistence
extends RefCounted


## Default file extension for galaxy saves.
const EXTENSION_BINARY: String = "sgg"

## JSON extension for debug saves.
const EXTENSION_JSON: String = "json"


## Saves galaxy data to a JSON file.
## @param path: File path to save to.
## @param data: GalaxySaveData (RefCounted) to save.
## @return: Empty string on success, error message on failure.
static func save_json(path: String, data: RefCounted) -> String:
	if data == null:
		return "No data to save"
	if not data.is_valid():
		return "Invalid galaxy save data"

	var dict: Dictionary = data.to_dict()
	var json_string: String = JSON.stringify(dict, "  ")

	var file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return "Failed to open file for writing: %s" % path

	file.store_string(json_string)
	file.close()

	return ""


## Loads galaxy data from a JSON file.
## @param path: File path to load from.
## @return: GalaxySaveData instance (Variant) on success, null on failure.
static func load_json(path: String) -> Variant:
	if not FileAccess.file_exists(path):
		push_error("File not found: %s" % path)
		return null

	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("Failed to open file: %s" % path)
		return null

	var json_string: String = file.get_as_text()
	file.close()

	var json: JSON = JSON.new()
	var parse_result: Error = json.parse(json_string)
	if parse_result != OK:
		push_error("Failed to parse JSON: %s" % json.get_error_message())
		return null

	var dict: Variant = json.data
	if not dict is Dictionary:
		push_error("Invalid JSON structure")
		return null

	var ScriptClass: GDScript = load("res://src/domain/galaxy/GalaxySaveData.gd") as GDScript
	return ScriptClass.from_dict(dict as Dictionary)


## Saves galaxy data to a compressed binary file.
## @param path: File path to save to.
## @param data: GalaxySaveData (RefCounted) to save.
## @return: Empty string on success, error message on failure.
static func save_binary(path: String, data: RefCounted) -> String:
	if data == null:
		return "No data to save"
	if not data.is_valid():
		return "Invalid galaxy save data"

	var dict: Dictionary = data.to_dict()
	var json_string: String = JSON.stringify(dict)
	var bytes: PackedByteArray = json_string.to_utf8_buffer()
	var compressed: PackedByteArray = bytes.compress(FileAccess.COMPRESSION_ZSTD)

	var file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return "Failed to open file for writing: %s" % path

	# Write magic header
	file.store_string("SGG1")
	file.store_32(bytes.size())  # Uncompressed size
	file.store_buffer(compressed)
	file.close()

	return ""


## Loads galaxy data from a compressed binary file.
## @param path: File path to load from.
## @return: GalaxySaveData instance (Variant) on success, null on failure.
static func load_binary(path: String) -> Variant:
	if not FileAccess.file_exists(path):
		push_error("File not found: %s" % path)
		return null

	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("Failed to open file: %s" % path)
		return null

	# Read and verify magic header
	var magic: String = file.get_buffer(4).get_string_from_utf8()
	if magic != "SGG1":
		push_error("Invalid file format: %s" % path)
		file.close()
		return null

	var uncompressed_size: int = file.get_32()
	var compressed: PackedByteArray = file.get_buffer(file.get_length() - file.get_position())
	file.close()

	var bytes: PackedByteArray = compressed.decompress(uncompressed_size, FileAccess.COMPRESSION_ZSTD)
	var json_string: String = bytes.get_string_from_utf8()

	var json: JSON = JSON.new()
	var parse_result: Error = json.parse(json_string)
	if parse_result != OK:
		push_error("Failed to parse data: %s" % json.get_error_message())
		return null

	var dict: Variant = json.data
	if not dict is Dictionary:
		push_error("Invalid data structure")
		return null

	var ScriptClass: GDScript = load("res://src/domain/galaxy/GalaxySaveData.gd") as GDScript
	return ScriptClass.from_dict(dict as Dictionary)


## Auto-detects format and loads galaxy data.
## @param path: File path to load from.
## @return: GalaxySaveData instance (Variant) on success, null on failure.
static func load_auto(path: String) -> Variant:
	if path.ends_with(".json"):
		return load_json(path)
	else:
		return load_binary(path)


## Returns the recommended file filter for save dialogs.
## @return: Filter string for FileDialog.
static func get_file_filter() -> String:
	return "*.sgg ; StarGen Galaxy, *.json ; JSON Debug"
