## Unit tests for GalaxyInspectorPanel.
extends TestCase

const _inspector_script: GDScript = preload("res://src/app/galaxy_viewer/GalaxyInspectorPanel.gd")

var _panel: GalaxyInspectorPanel = null


func get_test_name() -> String:
	return "TestGalaxyInspectorPanel"


func before_each() -> void:
	_panel = _inspector_script.new()
	_panel._ready()


func after_each() -> void:
	if _panel != null:
		_panel.free()
		_panel = null


func test_instantiates() -> void:
	assert_not_null(_panel, "Panel should instantiate")


func test_starts_without_star_selected() -> void:
	assert_false(_panel.has_star_selected(), "Should not have star selected initially")
	assert_equal(_panel.get_selected_star_seed(), 0, "Selected seed should be 0")


func test_display_selected_star_sets_selection() -> void:
	var pos: Vector3 = Vector3(100.0, 50.0, 200.0)
	var seed_val: int = 12345

	_panel.display_selected_star(pos, seed_val)

	assert_true(_panel.has_star_selected(), "Should have star selected")
	assert_equal(_panel.get_selected_star_seed(), seed_val, "Should store seed")
	assert_true(_panel.get_selected_star_position().is_equal_approx(pos), "Should store position")


func test_clear_selection_removes_star() -> void:
	_panel.display_selected_star(Vector3(1, 2, 3), 99999)
	_panel.clear_selection()

	assert_false(_panel.has_star_selected(), "Should not have star after clear")
	assert_equal(_panel.get_selected_star_seed(), 0, "Seed should be 0 after clear")


func test_display_selected_quadrant_clears_star() -> void:
	_panel.display_selected_star(Vector3(1, 2, 3), 99999)
	_panel.display_selected_quadrant(Vector3i(0, 0, 0), 0.5)

	assert_false(_panel.has_star_selected(), "Star should be cleared when quadrant selected")


func test_display_selected_sector_clears_star() -> void:
	_panel.display_selected_star(Vector3(1, 2, 3), 99999)
	_panel.display_selected_sector(Vector3i(0, 0, 0), Vector3i(5, 5, 5), 0.3)

	assert_false(_panel.has_star_selected(), "Star should be cleared when sector selected")


func test_display_galaxy_with_spec() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)

	_panel.display_galaxy(spec, GalaxyCoordinates.ZoomLevel.GALAXY)

	assert_false(_panel.has_star_selected(), "Should not have star selected after galaxy display")


func test_display_galaxy_with_null_spec() -> void:
	_panel.display_galaxy(null, GalaxyCoordinates.ZoomLevel.GALAXY)

	assert_false(_panel.has_star_selected(), "Should handle null spec")


func test_open_system_signal() -> void:
	var received: Array = [0, Vector3.ZERO]

	_panel.open_system_requested.connect(func(s: int, p: Vector3) -> void:
		received[0] = s
		received[1] = p
	)

	var test_seed: int = 55555
	var test_pos: Vector3 = Vector3(100, 200, 300)

	_panel.display_selected_star(test_pos, test_seed)
	_panel._on_open_system_pressed()

	assert_equal(received[0], test_seed, "Signal should emit correct seed")
	assert_true((received[1] as Vector3).is_equal_approx(test_pos), "Signal should emit correct position")


func test_open_system_signal_not_emitted_without_selection() -> void:
	var emitted: Array = [false]

	_panel.open_system_requested.connect(func(_s: int, _p: Vector3) -> void:
		emitted[0] = true
	)

	_panel._on_open_system_pressed()

	assert_false(emitted[0], "Signal should not emit without selection")
