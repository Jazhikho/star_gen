## Version constants for tracking generator and schema compatibility.
## Used for provenance tracking and migration detection.
class_name Versions
extends RefCounted

const VERSIONS_BRIDGE_CLASS: StringName = &"CSharpVersionsBridge"


## Current generator version (semver format).
## Increment when generation logic changes in ways that affect output.
const GENERATOR_VERSION: String = "0.1.0"

## Current schema version (integer).
## Increment when serialization format has breaking changes.
const SCHEMA_VERSION: int = 1


## Returns the current generator version.
## @return: Generator version string.
static func get_generator_version() -> String:
	var bridge: Object = null
	if ClassDB.class_exists(VERSIONS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VERSIONS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("GetGeneratorVersion"):
		return String(bridge.call("GetGeneratorVersion"))
	return GENERATOR_VERSION


## Returns the current schema version.
## @return: Schema version number.
static func get_schema_version() -> int:
	var bridge: Object = null
	if ClassDB.class_exists(VERSIONS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(VERSIONS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("GetSchemaVersion"):
		return int(bridge.call("GetSchemaVersion"))
	return SCHEMA_VERSION
