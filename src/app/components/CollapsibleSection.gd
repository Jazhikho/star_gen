## A collapsible UI section with a header button that toggles content visibility.
## Used for organizing settings into expandable groups.
class_name CollapsibleSection
extends VBoxContainer


## Emitted when the section is expanded or collapsed.
## @param is_expanded: True if now expanded, false if collapsed.
signal toggled(is_expanded: bool)


## The title text displayed in the header.
@export var title: String = "Section":
	set(value):
		title = value
		_update_header_text()


## Whether the section starts expanded.
@export var start_expanded: bool = true


## Arrow characters for collapsed/expanded states.
const ARROW_COLLAPSED: String = "▶"
const ARROW_EXPANDED: String = "▼"


## Internal references.
var _header_button: Button = null
var _content_container: VBoxContainer = null
var _is_expanded: bool = true


func _ready() -> void:
	_is_expanded = start_expanded
	_setup_structure()
	_update_header_text()
	_update_content_visibility()


## Returns whether the section is currently expanded.
## @return: True if expanded, false if collapsed.
func is_expanded() -> bool:
	return _is_expanded


## Expands the section to show content.
func expand() -> void:
	if not _is_expanded:
		_is_expanded = true
		_update_header_text()
		_update_content_visibility()
		toggled.emit(true)


## Collapses the section to hide content.
func collapse() -> void:
	if _is_expanded:
		_is_expanded = false
		_update_header_text()
		_update_content_visibility()
		toggled.emit(false)


## Sets the expanded state.
## @param expanded: True to expand, false to collapse.
func set_expanded(expanded: bool) -> void:
	if expanded:
		expand()
	else:
		collapse()


## Returns the content container for adding child controls.
## @return: The VBoxContainer that holds section content.
func get_content_container() -> VBoxContainer:
	return _content_container


## Adds a control to the content area.
## @param control: The control to add.
func add_content(control: Control) -> void:
	if _content_container != null:
		_content_container.add_child(control)


## Sets up the internal structure with header button and content container.
func _setup_structure() -> void:
	# Create header button
	_header_button = Button.new()
	_header_button.name = "HeaderButton"
	_header_button.flat = false
	_header_button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	_header_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_header_button.pressed.connect(_on_header_pressed)
	add_child(_header_button)
	move_child(_header_button, 0)
	
	# Create content container
	_content_container = VBoxContainer.new()
	_content_container.name = "ContentContainer"
	_content_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	
	# Add left margin for indentation
	var margin: MarginContainer = MarginContainer.new()
	margin.name = "ContentMargin"
	margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	margin.add_theme_constant_override("margin_left", 16)
	margin.add_theme_constant_override("margin_top", 8)
	margin.add_theme_constant_override("margin_bottom", 4)
	margin.add_child(_content_container)
	add_child(margin)
	
	# Move any existing children (added in editor) to content container
	_migrate_existing_children()


## Moves any children that were added before _ready to the content container.
func _migrate_existing_children() -> void:
	var children_to_move: Array[Node] = []
	for child in get_children():
		if child != _header_button and child.name != "ContentMargin":
			children_to_move.append(child)
	
	for child in children_to_move:
		remove_child(child)
		_content_container.add_child(child)


## Updates the header button text with arrow indicator.
func _update_header_text() -> void:
	if _header_button == null:
		return
	
	var arrow: String = ARROW_EXPANDED if _is_expanded else ARROW_COLLAPSED
	_header_button.text = "%s  %s" % [arrow, title]


## Updates the visibility of the content container.
func _update_content_visibility() -> void:
	var margin: Node = get_node_or_null("ContentMargin")
	if margin != null:
		margin.visible = _is_expanded


## Handles header button press to toggle expansion.
func _on_header_pressed() -> void:
	set_expanded(not _is_expanded)
