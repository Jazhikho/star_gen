## Manages the current zoom level and transitions for the galaxy viewer.
## Emits level_changed when the zoom level changes.
class_name ZoomStateMachine
extends RefCounted


## Emitted when the zoom level transitions.
## Parameters: old_level (int), new_level (int) — values from GalaxyCoordinates.ZoomLevel.
signal level_changed(old_level, new_level)


## Current zoom level as a GalaxyCoordinates.ZoomLevel value.
var _current_level: int = GalaxyCoordinates.ZoomLevel.GALAXY


## Returns the current zoom level.
## @return: Current level as GalaxyCoordinates.ZoomLevel int.
func get_current_level() -> int:
	return _current_level


## Transitions directly to a specific zoom level.
## Does nothing if already at that level.
## @param new_level: Target GalaxyCoordinates.ZoomLevel value.
func transition_to(new_level: int) -> void:
	if new_level == _current_level:
		return
	var old_level: int = _current_level
	_current_level = new_level
	level_changed.emit(old_level, new_level)


## Zooms in one level if possible.
## Levels are sequential: GALAXY → QUADRANT → SECTOR → SUBSECTOR.
func zoom_in() -> void:
	if can_zoom_in():
		transition_to(_current_level + 1)


## Zooms out one level if possible.
func zoom_out() -> void:
	if can_zoom_out():
		transition_to(_current_level - 1)


## Returns whether zooming in further is possible.
## @return: True if not already at the deepest level.
func can_zoom_in() -> bool:
	return _current_level < GalaxyCoordinates.ZoomLevel.SUBSECTOR


## Returns whether zooming out further is possible.
## @return: True if not already at the galaxy level.
func can_zoom_out() -> bool:
	return _current_level > GalaxyCoordinates.ZoomLevel.GALAXY
