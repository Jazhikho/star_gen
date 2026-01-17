## Represents a single validation error.
class_name ValidationError
extends RefCounted


## Severity levels for validation errors.
enum Severity {
	WARNING,
	ERROR,
}


## The field or path that has the error.
var field: String

## Human-readable error message.
var message: String

## Severity of the error.
var severity: Severity


## Creates a new ValidationError.
## @param p_field: The field with the error.
## @param p_message: The error message.
## @param p_severity: The error severity.
func _init(
	p_field: String = "",
	p_message: String = "",
	p_severity: Severity = Severity.ERROR
) -> void:
	field = p_field
	message = p_message
	severity = p_severity


## Returns a formatted string representation.
## @return: Formatted error string.
func format_error() -> String:
	var severity_str: String = "ERROR" if severity == Severity.ERROR else "WARNING"
	return "[%s] %s: %s" % [severity_str, field, message]
