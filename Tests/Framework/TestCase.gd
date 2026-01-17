## Base class for all test cases.
## Extend this class and add methods prefixed with "test_" to create tests.
class_name TestCase
extends RefCounted


## Tracks whether the current test has failed.
var _current_test_failed: bool = false

## The failure message for the current test.
var _current_failure_message: String = ""


## Called before each test method. Override to set up test fixtures.
func before_each() -> void:
	pass


## Called after each test method. Override to clean up test fixtures.
func after_each() -> void:
	pass


## Called once before all tests in this test case. Override for one-time setup.
func before_all() -> void:
	pass


## Called once after all tests in this test case. Override for one-time cleanup.
func after_all() -> void:
	pass


## Resets the failure state for a new test.
func _reset_failure_state() -> void:
	_current_test_failed = false
	_current_failure_message = ""


## Returns whether the current test has failed.
## @return: True if the test failed, false otherwise.
func has_failed() -> bool:
	return _current_test_failed


## Returns the failure message for the current test.
## @return: The failure message, or empty string if not failed.
func get_failure_message() -> String:
	return _current_failure_message


## Asserts that a condition is true.
## @param condition: The condition to check.
## @param message: Optional message to display on failure.
func assert_true(condition: bool, message: String = "") -> void:
	if _current_test_failed:
		return
	if not condition:
		_fail(message if message else "Expected true but got false")


## Asserts that a condition is false.
## @param condition: The condition to check.
## @param message: Optional message to display on failure.
func assert_false(condition: bool, message: String = "") -> void:
	if _current_test_failed:
		return
	if condition:
		_fail(message if message else "Expected false but got true")


## Asserts that two values are equal.
## @param actual: The actual value.
## @param expected: The expected value.
## @param message: Optional message to display on failure.
func assert_equal(actual: Variant, expected: Variant, message: String = "") -> void:
	if _current_test_failed:
		return
	if actual != expected:
		var base_msg: String = "Expected '%s' but got '%s'" % [str(expected), str(actual)]
		_fail((message + ": " + base_msg) if message else base_msg)


## Asserts that two values are not equal.
## @param actual: The actual value.
## @param unexpected: The value that actual should not equal.
## @param message: Optional message to display on failure.
func assert_not_equal(actual: Variant, unexpected: Variant, message: String = "") -> void:
	if _current_test_failed:
		return
	if actual == unexpected:
		var base_msg: String = "Expected value to not equal '%s'" % [str(unexpected)]
		_fail((message + ": " + base_msg) if message else base_msg)


## Asserts that a value is null.
## @param value: The value to check.
## @param message: Optional message to display on failure.
func assert_null(value: Variant, message: String = "") -> void:
	if _current_test_failed:
		return
	if value != null:
		var base_msg: String = "Expected null but got '%s'" % [str(value)]
		_fail((message + ": " + base_msg) if message else base_msg)


## Asserts that a value is not null.
## @param value: The value to check.
## @param message: Optional message to display on failure.
func assert_not_null(value: Variant, message: String = "") -> void:
	if _current_test_failed:
		return
	if value == null:
		_fail(message if message else "Expected non-null value but got null")


## Asserts that two floats are approximately equal within a tolerance.
## @param actual: The actual value.
## @param expected: The expected value.
## @param tolerance: The maximum allowed difference (default 0.00001).
## @param message: Optional message to display on failure.
func assert_float_equal(
	actual: float,
	expected: float,
	tolerance: float = 0.00001,
	message: String = ""
) -> void:
	if _current_test_failed:
		return
	if absf(actual - expected) > tolerance:
		var base_msg: String = "Expected %f to be within %f of %f" % [actual, tolerance, expected]
		_fail((message + ": " + base_msg) if message else base_msg)


## Asserts that a value is greater than another.
## @param actual: The actual value.
## @param expected: The value to compare against.
## @param message: Optional message to display on failure.
func assert_greater_than(actual: Variant, expected: Variant, message: String = "") -> void:
	if _current_test_failed:
		return
	if not (actual > expected):
		var base_msg: String = "Expected '%s' to be greater than '%s'" % [str(actual), str(expected)]
		_fail((message + ": " + base_msg) if message else base_msg)


## Asserts that a value is less than another.
## @param actual: The actual value.
## @param expected: The value to compare against.
## @param message: Optional message to display on failure.
func assert_less_than(actual: Variant, expected: Variant, message: String = "") -> void:
	if _current_test_failed:
		return
	if not (actual < expected):
		var base_msg: String = "Expected '%s' to be less than '%s'" % [str(actual), str(expected)]
		_fail((message + ": " + base_msg) if message else base_msg)


## Asserts that a value is within a range (inclusive).
## @param value: The value to check.
## @param min_val: The minimum allowed value.
## @param max_val: The maximum allowed value.
## @param message: Optional message to display on failure.
func assert_in_range(
	value: Variant,
	min_val: Variant,
	max_val: Variant,
	message: String = ""
) -> void:
	if _current_test_failed:
		return
	if value < min_val or value > max_val:
		var base_msg: String = "Expected '%s' to be in range [%s, %s]" % [
			str(value), str(min_val), str(max_val)
		]
		_fail((message + ": " + base_msg) if message else base_msg)


## Explicitly fails the current test with a message.
## @param message: The failure message.
func fail(message: String = "Test failed") -> void:
	_fail(message)


## Internal method to mark the test as failed.
## @param message: The failure message.
func _fail(message: String) -> void:
	_current_test_failed = true
	_current_failure_message = message
