## File I/O service for celestial body persistence.
## Handles saving and loading celestial bodies to/from disk.
class_name CelestialPersistence
extends RefCounted


## Default directory for saved celestial bodies.
const DEFAULT_SAVE_DIR: String = "user://celestial_bodies/"


## Saves a celestial body to a JSON file.
## @param body: The celestial body to save.
## @param file_path: The full path to save to.
## @return: OK on success, or an error code.
static func save_body(body: CelestialBody, file_path: String) -> Error:
	var dir_path: String = file_path.get_base_dir()
	var dir: DirAccess = DirAccess.open("user://")
	if dir == null:
		return ERR_CANT_CREATE

	if not DirAccess.dir_exists_absolute(dir_path):
		var err: Error = dir.make_dir_recursive(dir_path.replace("user://", ""))
		if err != OK:
			return err

	var json_string: String = CelestialSerializer.to_json(body, true)

	var file: FileAccess = FileAccess.open(file_path, FileAccess.WRITE)
	if file == null:
		return FileAccess.get_open_error()

	file.store_string(json_string)
	file.close()

	return OK


## Loads a celestial body from a JSON file.
## @param file_path: The full path to load from.
## @return: The loaded CelestialBody, or null if loading fails.
static func load_body(file_path: String) -> CelestialBody:
	if not FileAccess.file_exists(file_path):
		return null

	var file: FileAccess = FileAccess.open(file_path, FileAccess.READ)
	if file == null:
		return null

	var json_string: String = file.get_as_text()
	file.close()

	return CelestialSerializer.from_json(json_string)


## Generates a default file path for a celestial body.
## @param body: The celestial body.
## @return: A suggested file path based on body ID.
static func get_default_path(body: CelestialBody) -> String:
	var filename: String = body.id if not body.id.is_empty() else "unnamed"
	filename = filename.replace(" ", "_").to_lower()
	return DEFAULT_SAVE_DIR + filename + ".json"


## Lists all saved celestial body files in a directory.
## @param dir_path: The directory to scan.
## @return: Array of file paths.
static func list_saved_bodies(dir_path: String = DEFAULT_SAVE_DIR) -> Array[String]:
	var result: Array[String] = []
	
	var dir: DirAccess = DirAccess.open(dir_path)
	if dir == null:
		return result
	
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	while file_name != "":
		if not dir.current_is_dir() and file_name.ends_with(".json"):
			result.append(dir_path + file_name)
		file_name = dir.get_next()
	dir.list_dir_end()
	
	return result


## Deletes a saved celestial body file.
## @param file_path: The path to the file to delete.
## @return: OK on success, or an error code.
static func delete_body(file_path: String) -> Error:
	if not FileAccess.file_exists(file_path):
		return ERR_FILE_NOT_FOUND

	var dir: DirAccess = DirAccess.open(file_path.get_base_dir())
	if dir == null:
		return ERR_CANT_OPEN

	return dir.remove(file_path.get_file())
