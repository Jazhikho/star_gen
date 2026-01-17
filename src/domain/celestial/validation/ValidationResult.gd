## Contains the results of validating a celestial body.
class_name ValidationResult
extends RefCounted


## List of validation errors found.
var errors: Array[ValidationError]


## Creates a new ValidationResult.
func _init() -> void:
	errors = []


## Returns true if there are no errors (warnings are allowed).
## @return: True if valid, false if any errors exist.
func is_valid() -> bool:
	for error in errors:
		if error.severity == ValidationError.Severity.ERROR:
			return false
	return true


## Returns true if there are no errors or warnings.
## @return: True if completely clean.
func is_clean() -> bool:
	return errors.is_empty()


## Adds an error to the result.
## @param field: The field with the error.
## @param message: The error message.
func add_error(field: String, message: String) -> void:
	errors.append(ValidationError.new(field, message, ValidationError.Severity.ERROR))


## Adds a warning to the result.
## @param field: The field with the warning.
## @param message: The warning message.
func add_warning(field: String, message: String) -> void:
	errors.append(ValidationError.new(field, message, ValidationError.Severity.WARNING))


## Returns all error-level issues.
## @return: Array of error-level ValidationErrors.
func get_errors_only() -> Array[ValidationError]:
	var result: Array[ValidationError] = []
	for error in errors:
		if error.severity == ValidationError.Severity.ERROR:
			result.append(error)
	return result


## Returns all warning-level issues.
## @return: Array of warning-level ValidationErrors.
func get_warnings_only() -> Array[ValidationError]:
	var result: Array[ValidationError] = []
	for error in errors:
		if error.severity == ValidationError.Severity.WARNING:
			result.append(error)
	return result


## Returns the count of errors.
## @return: Number of errors.
func get_error_count() -> int:
	return get_errors_only().size()


## Returns the count of warnings.
## @return: Number of warnings.
func get_warning_count() -> int:
	return get_warnings_only().size()
