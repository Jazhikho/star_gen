## Handles efficient saving and loading of celestial bodies.
## Uses regeneration-based storage to minimize file sizes.
class_name SaveData
extends RefCounted

const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _celestial_serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _asteroid_generator: GDScript = preload("res://src/domain/generation/generators/AsteroidGenerator.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _versions: GDScript = preload("res://src/domain/constants/Versions.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")

## Current save format version
const SAVE_VERSION: int = 1

## Save mode enum
enum SaveMode {
	MINIMAL, ## Only seed + type (for regeneration)
	COMPACT, ## Seed + spec + context (default)
	FULL ## Complete serialized body
}

## Result of a load operation
class LoadResult:
	var success: bool = false
	var body: CelestialBody = null
	var error_message: String = ""
	
	static func ok(p_body: CelestialBody) -> LoadResult:
		var result: LoadResult = LoadResult.new()
		result.success = true
		result.body = p_body
		return result
	
	static func error(message: String) -> LoadResult:
		var result: LoadResult = LoadResult.new()
		result.success = false
		result.error_message = message
		return result


## Saves a celestial body to a file.
## @param body: The body to save.
## @param path: The file path.
## @param mode: Save mode (MINIMAL, COMPACT, or FULL).
## @param compress: Whether to compress the output.
## @return: Error code.
static func save_body(
	body: CelestialBody,
	path: String,
	mode: SaveMode = SaveMode.COMPACT,
	compress: bool = true
) -> Error:
	if not body:
		return ERR_INVALID_PARAMETER
	
	var data: Dictionary = _create_save_data(body, mode)
	
	if compress:
		return _save_compressed(path, data)
	else:
		return _save_json(path, data)


## Saves a body that has been edited (values diverge from generation).
## Uses FULL mode because regeneration cannot reproduce user edits —
## the seed + spec would regenerate the *original* values.
## @param body: The edited body.
## @param path: The file path.
## @param compress: Whether to compress the output.
## @return: Error code.
static func save_edited_body(
	body: CelestialBody,
	path: String,
	compress: bool = true
) -> Error:
	return save_body(body, path, SaveMode.FULL, compress)


## Loads a celestial body from a file.
## @param path: The file path.
## @return: LoadResult with body or error.
static func load_body(path: String) -> LoadResult:
	# Try compressed first, then plain JSON
	var data: Dictionary
	var load_error: String = ""
	
	if path.ends_with(".sgb"):
		var result: Dictionary = _load_compressed(path)
		if result.has("error"):
			load_error = result["error"]
		else:
			data = result
	else:
		var result: Dictionary = _load_json(path)
		if result.has("error"):
			load_error = result["error"]
		else:
			data = result
	
	if not load_error.is_empty():
		return LoadResult.error(load_error)
	
	if data.is_empty():
		return LoadResult.error("File is empty or invalid")
	
	# Validate version
	var version: int = data.get("version", 0)
	if version > SAVE_VERSION:
		return LoadResult.error("Save file version %d is newer than supported version %d" % [version, SAVE_VERSION])
	
	# Determine load method based on save mode
	var save_mode: int = data.get("save_mode", SaveMode.COMPACT)
	
	var body: CelestialBody = null
	match save_mode:
		SaveMode.MINIMAL, SaveMode.COMPACT:
			body = _regenerate_body(data)
		SaveMode.FULL:
			body = _deserialize_body(data)
	
	if not body:
		return LoadResult.error("Failed to reconstruct body from save data")
	
	return LoadResult.ok(body)


## Gets file size in bytes.
## @param path: The file path.
## @return: File size in bytes, or 0 if file doesn't exist.
static func get_file_size(path: String) -> int:
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if not file:
		return 0
	
	var size: int = file.get_length()
	file.close()
	return size


## Gets a human-readable file size string.
## @param bytes: The size in bytes.
## @return: Formatted size string (e.g., "1.5 KB", "2.3 MB").
static func format_file_size(bytes: int) -> String:
	if bytes < 1024:
		return "%d B" % bytes
	elif bytes < 1024 * 1024:
		return "%.1f KB" % (bytes / 1024.0)
	else:
		return "%.2f MB" % (bytes / (1024.0 * 1024.0))


## Creates save data dictionary for a body.
static func _create_save_data(body: CelestialBody, mode: SaveMode) -> Dictionary:
	var data: Dictionary = {
		"version": SAVE_VERSION,
		"save_mode": mode,
		"timestamp": int(Time.get_unix_time_from_system()),
	}
	
	match mode:
		SaveMode.MINIMAL:
			data.merge(_create_minimal_data(body))
		SaveMode.COMPACT:
			data.merge(_create_compact_data(body))
		SaveMode.FULL:
			data.merge(_create_full_data(body))
	
	return data


## Creates minimal save data (just enough to regenerate).
static func _create_minimal_data(body: CelestialBody) -> Dictionary:
	var data: Dictionary = {
		"id": body.id,
		"type": CelestialType.type_to_string(body.type),
	}
	
	if body.provenance:
		data["seed"] = body.provenance.generation_seed
	
	return data


## Creates compact save data (spec + context for perfect regeneration).
static func _create_compact_data(body: CelestialBody) -> Dictionary:
	var data: Dictionary = _create_minimal_data(body)
	
	if body.provenance:
		# Get spec snapshot and extract context if present
		var spec_snapshot: Dictionary = body.provenance.spec_snapshot.duplicate()
		
		# Extract context from spec_snapshot for non-star bodies
		if body.type != CelestialType.Type.STAR and spec_snapshot.has("context"):
			data["context"] = spec_snapshot["context"]
			spec_snapshot.erase("context") # Remove context from spec data
		
		data["spec"] = spec_snapshot
		data["generator_version"] = body.provenance.generator_version
		
		# If context wasn't in spec_snapshot, use default for non-star bodies
		if body.type != CelestialType.Type.STAR and not data.has("context"):
			data["context"] = _get_default_context(body.type).to_dict()
	
	# Store any user modifications
	if body.has_meta("user_modifications"):
		data["modifications"] = body.get_meta("user_modifications")
	
	return data


## Creates full save data (complete serialization).
static func _create_full_data(body: CelestialBody) -> Dictionary:
	var data: Dictionary = {
		"id": body.id,
		"type": CelestialType.type_to_string(body.type),
		"body": CelestialSerializer.to_dict(body)
	}
	
	return data


## Regenerates a body from save data.
static func _regenerate_body(data: Dictionary) -> CelestialBody:
	var body_type_str: String = data.get("type", "planet")
	var body_type: int = CelestialType.string_to_type(body_type_str)
	var seed_val: int = data.get("seed", 0)
	
	var spec_data: Dictionary = data.get("spec", {}).duplicate()
	
	# Extract context from spec_data if it was stored there (from generator)
	var context_data: Dictionary = data.get("context", {})
	if context_data.is_empty() and spec_data.has("context"):
		context_data = spec_data["context"]
		spec_data.erase("context") # Remove context from spec before reconstructing
	
	var rng: SeededRng = SeededRng.new(seed_val)
	var body: CelestialBody = null
	
	match body_type:
		CelestialType.Type.STAR:
			var spec: StarSpec = _reconstruct_star_spec(spec_data, seed_val)
			body = StarGenerator.generate(spec, rng)
		
		CelestialType.Type.PLANET:
			var spec: PlanetSpec = _reconstruct_planet_spec(spec_data, seed_val)
			var context: ParentContext = _reconstruct_context(context_data, CelestialType.Type.PLANET)
			body = PlanetGenerator.generate(spec, context, rng)
		
		CelestialType.Type.MOON:
			var spec: MoonSpec = _reconstruct_moon_spec(spec_data, seed_val)
			var context: ParentContext = _reconstruct_context(context_data, CelestialType.Type.MOON)
			body = MoonGenerator.generate(spec, context, rng)
		
		CelestialType.Type.ASTEROID:
			var spec: AsteroidSpec = _reconstruct_asteroid_spec(spec_data, seed_val)
			var context: ParentContext = _reconstruct_context(context_data, CelestialType.Type.ASTEROID)
			body = AsteroidGenerator.generate(spec, context, rng)
	
	# Restore name if it was customized
	if body and data.has("name") and data["name"] != "":
		body.name = data["name"]
	
	# Apply any user modifications
	if body and data.has("modifications"):
		_apply_modifications(body, data["modifications"])
	
	return body


## Deserializes a fully saved body.
static func _deserialize_body(data: Dictionary) -> CelestialBody:
	if data.has("body"):
		return CelestialSerializer.from_dict(data["body"])
	return null


## Reconstructs a StarSpec from save data.
static func _reconstruct_star_spec(spec_data: Dictionary, seed_val: int) -> StarSpec:
	if spec_data.is_empty():
		return StarSpec.random(seed_val)
	
	return StarSpec.from_dict(spec_data)


## Reconstructs a PlanetSpec from save data.
static func _reconstruct_planet_spec(spec_data: Dictionary, seed_val: int) -> PlanetSpec:
	if spec_data.is_empty():
		return PlanetSpec.random(seed_val)
	
	return PlanetSpec.from_dict(spec_data)


## Reconstructs a MoonSpec from save data.
static func _reconstruct_moon_spec(spec_data: Dictionary, seed_val: int) -> MoonSpec:
	if spec_data.is_empty():
		return MoonSpec.random(seed_val)
	
	return MoonSpec.from_dict(spec_data)


## Reconstructs an AsteroidSpec from save data.
static func _reconstruct_asteroid_spec(spec_data: Dictionary, seed_val: int) -> AsteroidSpec:
	if spec_data.is_empty():
		return AsteroidSpec.random(seed_val)
	
	return AsteroidSpec.from_dict(spec_data)


## Reconstructs a ParentContext from save data.
static func _reconstruct_context(context_data: Dictionary, body_type: CelestialType.Type) -> ParentContext:
	if not context_data.is_empty():
		return ParentContext.from_dict(context_data)
	
	return _get_default_context(body_type)


## Gets the default context for a body type.
static func _get_default_context(body_type: CelestialType.Type) -> ParentContext:
	match body_type:
		CelestialType.Type.PLANET:
			return ParentContext.sun_like()
		
		CelestialType.Type.MOON:
			return ParentContext.for_moon(
				Units.SOLAR_MASS_KG,
				3.828e26,
				5778.0,
				4.6e9,
				5.2 * Units.AU_METERS,
				1.898e27,
				6.9911e7,
				5.0e8
			)
		
		CelestialType.Type.ASTEROID:
			return ParentContext.sun_like(2.7 * Units.AU_METERS)
		
		_:
			return ParentContext.sun_like()


## Applies user modifications to a regenerated body.
static func _apply_modifications(body: CelestialBody, modifications: Dictionary) -> void:
	# For now, just store as metadata
	# Future: apply actual property changes
	body.set_meta("user_modifications", modifications)


## Saves data as compressed binary.
static func _save_compressed(path: String, data: Dictionary) -> Error:
	var json_str: String = JSON.stringify(data)
	var bytes: PackedByteArray = json_str.to_utf8_buffer()
	
	var compressed: PackedByteArray = bytes.compress(FileAccess.COMPRESSION_ZSTD)
	
	# Ensure path has correct extension
	var save_path: String = path
	if not save_path.ends_with(".sgb"):
		save_path = path.get_basename() + ".sgb"
	
	var file: FileAccess = FileAccess.open(save_path, FileAccess.WRITE)
	if not file:
		return FileAccess.get_open_error()
	
	# Write magic header
	file.store_string("SGBD") # StarGen Body Data
	file.store_16(SAVE_VERSION)
	file.store_32(bytes.size()) # Uncompressed size
	file.store_buffer(compressed)
	file.close()
	
	return OK


## Saves data as plain JSON.
static func _save_json(path: String, data: Dictionary) -> Error:
	var save_path: String = path
	if not save_path.ends_with(".json"):
		save_path = path.get_basename() + ".json"
	
	var file: FileAccess = FileAccess.open(save_path, FileAccess.WRITE)
	if not file:
		return FileAccess.get_open_error()
	
	var json_str: String = JSON.stringify(data, "\t")
	file.store_string(json_str)
	file.close()
	
	return OK


## Loads compressed data.
static func _load_compressed(path: String) -> Dictionary:
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if not file:
		return {"error": "Could not open file: %s" % path}
	
	# Read and verify header
	var magic: String = file.get_buffer(4).get_string_from_utf8()
	if magic != "SGBD":
		file.close()
		return {"error": "Invalid file format (not a StarGen save file)"}
	
	var file_version: int = file.get_16()
	if file_version > SAVE_VERSION:
		file.close()
		return {"error": "Save file is from a newer version (%d > %d)" % [file_version, SAVE_VERSION]}
	
	var uncompressed_size: int = file.get_32()
	
	# Read compressed data
	var compressed: PackedByteArray = file.get_buffer(file.get_length() - file.get_position())
	file.close()
	
	# Decompress
	var bytes: PackedByteArray = compressed.decompress(uncompressed_size, FileAccess.COMPRESSION_ZSTD)
	if bytes.is_empty():
		return {"error": "Failed to decompress data"}
	
	var json_str: String = bytes.get_string_from_utf8()
	
	var json: JSON = JSON.new()
	var parse_error: Error = json.parse(json_str)
	if parse_error != OK:
		return {"error": "Invalid JSON in save file: %s" % json.get_error_message()}
	
	return json.data as Dictionary


## Loads plain JSON data.
static func _load_json(path: String) -> Dictionary:
	var file: FileAccess = FileAccess.open(path, FileAccess.READ)
	if not file:
		return {"error": "Could not open file: %s" % path}
	
	var json_str: String = file.get_as_text()
	file.close()
	
	if json_str.is_empty():
		return {"error": "File is empty"}
	
	var json: JSON = JSON.new()
	var parse_error: Error = json.parse(json_str)
	if parse_error != OK:
		return {"error": "Invalid JSON: %s at line %d" % [json.get_error_message(), json.get_error_line()]}
	
	if not json.data is Dictionary:
		return {"error": "Expected JSON object at root"}
	
	return json.data as Dictionary
