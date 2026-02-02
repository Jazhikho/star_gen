## Tests for ZoomStateMachine — state transitions and guards.
class_name TestZoomStateMachine
extends TestCase


var _machine: ZoomStateMachine


func before_each() -> void:
	_machine = ZoomStateMachine.new()


func test_initial_level_is_galaxy() -> void:
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.GALAXY,
		"Should start at galaxy level"
	)


func test_zoom_in_from_galaxy() -> void:
	_machine.zoom_in()
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.QUADRANT,
		"Zoom in from galaxy should go to quadrant"
	)


func test_zoom_out_from_galaxy_does_nothing() -> void:
	_machine.zoom_out()
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.GALAXY,
		"Zoom out from galaxy should stay at galaxy"
	)


func test_zoom_in_twice() -> void:
	_machine.zoom_in()
	_machine.zoom_in()
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.SECTOR,
		"Two zoom-ins should reach sector level"
	)


func test_zoom_in_three_times() -> void:
	_machine.zoom_in()
	_machine.zoom_in()
	_machine.zoom_in()
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Three zoom-ins should reach subsector level"
	)


func test_cannot_zoom_past_subsector() -> void:
	_machine.zoom_in()
	_machine.zoom_in()
	_machine.zoom_in()
	_machine.zoom_in()
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Should not zoom past subsector"
	)


func test_zoom_out_from_quadrant() -> void:
	_machine.zoom_in()
	_machine.zoom_out()
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.GALAXY,
		"Zoom out from quadrant should return to galaxy"
	)


func test_can_zoom_in_from_galaxy() -> void:
	assert_true(_machine.can_zoom_in(), "Should be able to zoom in from galaxy")


func test_cannot_zoom_out_from_galaxy() -> void:
	assert_false(_machine.can_zoom_out(), "Should not be able to zoom out from galaxy")


func test_can_zoom_in_from_sector() -> void:
	_machine.zoom_in()
	_machine.zoom_in()
	assert_true(_machine.can_zoom_in(), "Should be able to zoom in from sector")


func test_cannot_zoom_in_from_subsector() -> void:
	_machine.zoom_in()
	_machine.zoom_in()
	_machine.zoom_in()
	assert_false(_machine.can_zoom_in(), "Should not be able to zoom in from subsector")


func test_transition_to_specific_level() -> void:
	_machine.transition_to(GalaxyCoordinates.ZoomLevel.SECTOR)
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.SECTOR,
		"Direct transition should set level"
	)


func test_transition_to_same_level_is_noop() -> void:
	# Track whether signal would fire by checking level stays the same
	_machine.transition_to(GalaxyCoordinates.ZoomLevel.GALAXY)
	assert_equal(
		_machine.get_current_level(),
		GalaxyCoordinates.ZoomLevel.GALAXY,
		"Transition to current level should be a no-op"
	)
