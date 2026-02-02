## File I/O service for solar system persistence.
## Supports compressed binary and JSON formats.
class_name SystemPersistence
extends RefCounted

const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _system_serializer: GDScript = preload("res://src/domain/system/SystemSerializer.gd")


## File extension for compressed binary format.
const BINARY_EXTENSION: String = "sgs"

## File extension for JSON debug format.
const JSON_EXTENSION: String = "json"


## Result of a load operation.
class LoadResult:
	extends RefCounted
	
	## The loaded system, or null if failed.
	var system: SolarSystem
	
	## Whether load succeeded.
	var success: bool
	
	## Error message if failed.
	var error_message: String
	
	func _init() -> void:
		system = null
		success = false
		error_message = ""


## Saves a solar system to a file.
## @param system: The system to save.
## @param path: File path.
## @param compress: Whether to compress (for .sgs files).
## @return: Error code (OK on success).
static func save(system: SolarSystem, path: String, compress: bool = true) -> Error:
	if system == null:
		return ERR_INVALID_PARAMETER
	
	var data: Dictionary = SystemSerializer.to_dict(system)
	
	# Determine format from extension
	var extension: String = path.get_extension().to_lower()
	
	if extension == JSON_EXTENSION or not compress:
		return _save_json(data, path)
	else:
		return _save_compressed(data, path)


## Loads a solar system from a file.
## @param path: File path.
## @return: LoadResult with system or error.
static func load(path: String) -> LoadResult:
	var result: LoadResult = LoadResult.new()
	
	if not FileAccess.file_exists(path):
		result.error_message = "File not found: %s" % path
		return result
	
	# Determine format from extension
	var extension: String = path.get_extension().to_lower()
	
	var data: Dictionary
	if extension == JSON_EXTENSION:
		data = _load_json(path, result)
	else:
		data = _load_compressed(path, result)
	
	if data.is_empty():
		if result.error_message.is_empty():
			result.error_message = "Failed to parse file"
		return result
	
	result.system = SystemSerializer.from_dict(data)
	if result.system == null:
		result.error_message = "Failed to deserialize system"
		return result
	
	result.success = true
	return result


## Saves data as JSON.
## @param data: Dictionary to save.
## @param path: File path.
## @return: Error code.
static func _save_json(data: Dictionary, path: String) -> Error:
	var json_string: String = JSON.stringify(data, "\t")
	
	var file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return FileAccess.get_open_error()
	
	file.store_string(json_string)
	file.close()
	
	return OK


## Saves data as compressed binary.
## @param data: Dictionary to save.
## @param path: File path.
## @return: Error code.
static func _save_compressed(data: Dictionary, path: String) -> Error:
	var json_string: String = JSON.stringify(data)
	var bytes: PackedByteArray = json_string.to_utf8_buffer()
	# Use GZIP compression (supported by both compress and decompress_dynamic)
	var compressed: PackedByteArray = bytes.compress(FileAccess.COMPRESSION_GZIP)
	
	var file: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return FileAccess.get_open_error()
	
	file.store_buffer(compressed)
	file.close()
	
	return OK


## Loads data from JSON file.
## @param path: File path.
## @param result: LoadResult to populate on error.
## @return: Parsed dictionary, or empty on failure.
static func _load_json(path: String, result: LoadResult) -> Dictionary:
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		result.error_message = "Cannot open file: %s" % error_string(FileAccess.get_open_error())
		return {}
	
	var json_string: String = file.get_as_text()
	file.close()
	
	var json: JSON = JSON.new()
	var error: Error = json.parse(json_string)
	if error != OK:
		result.error_message = "JSON parse error: %s" % json.get_error_message()
		return {}
	
	if not json.data is Dictionary:
		result.error_message = "Invalid JSON structure"
		return {}
	
	return json.data as Dictionary


## Loads data from compressed file.
## @param path: File path.
## @param result: LoadResult to populate on error.
## @return: Parsed dictionary, or empty on failure.
static func _load_compressed(path: String, result: LoadResult) -> Dictionary:
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		result.error_message = "Cannot open file: %s" % error_string(FileAccess.get_open_error())
		return {}
	
	var compressed: PackedByteArray = file.get_buffer(file.get_length())
	file.close()
	
	# Use GZIP decompression (must match compression mode)
	var decompressed: PackedByteArray = compressed.decompress_dynamic(-1, FileAccess.COMPRESSION_GZIP)
	if decompressed.is_empty():
		result.error_message = "Decompression failed"
		return {}
	
	var json_string: String = decompressed.get_string_from_utf8()
	
	var json: JSON = JSON.new()
	var error: Error = json.parse(json_string)
	if error != OK:
		result.error_message = "JSON parse error after decompression: %s" % json.get_error_message()
		return {}
	
	if not json.data is Dictionary:
		result.error_message = "Invalid JSON structure"
		return {}
	
	return json.data as Dictionary


## Returns file size in bytes.
## @param path: File path.
## @return: Size in bytes, or 0 if file doesn't exist.
static func get_file_size(path: String) -> int:
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file == null:
		return 0
	var size: int = file.get_length()
	file.close()
	return size


## Formats file size for display.
## @param bytes: Size in bytes.
## @return: Human-readable size string.
static func format_file_size(bytes: int) -> String:
	if bytes < 1024:
		return "%d B" % bytes
	elif bytes < 1024 * 1024:
		return "%.1f KB" % (float(bytes) / 1024.0)
	else:
		return "%.1f MB" % (float(bytes) / (1024.0 * 1024.0))
