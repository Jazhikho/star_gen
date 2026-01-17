## Version constants for tracking generator and schema compatibility.
## Used for provenance tracking and migration detection.
class_name Versions
extends RefCounted


## Current generator version (semver format).
## Increment when generation logic changes in ways that affect output.
const GENERATOR_VERSION: String = "0.1.0"

## Current schema version (integer).
## Increment when serialization format has breaking changes.
const SCHEMA_VERSION: int = 1
