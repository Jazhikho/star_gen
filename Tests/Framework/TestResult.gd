## Represents the result of a single test execution.
class_name TestResult
extends RefCounted


## The name of the test that was run.
var test_name: String

## Whether the test passed.
var passed: bool

## Failure message if the test failed, empty string if passed.
var message: String

## Time taken to run the test in milliseconds.
var time_ms: float


## Creates a new TestResult.
## @param p_test_name: The name of the test.
## @param p_passed: Whether the test passed.
## @param p_message: The failure message (if any).
## @param p_time_ms: The execution time in milliseconds.
func _init(
	p_test_name: String = "",
	p_passed: bool = true,
	p_message: String = "",
	p_time_ms: float = 0.0
) -> void:
	test_name = p_test_name
	passed = p_passed
	message = p_message
	time_ms = p_time_ms
