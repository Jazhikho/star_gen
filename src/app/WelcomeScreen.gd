## Startup screen shown before entering the galaxy viewer.
## Offers Start New Galaxy (with optional config), Load Galaxy, and Quit.
## All randomness for "random seed" goes through the injected SeededRng.
class_name WelcomeScreen
extends Control

const _GalaxyConfigRef: GDScript = preload("res://src/domain/galaxy/GalaxyConfig.gd")

## Emitted when the user chooses to start a new galaxy with the given config and seed.
## @param config: GalaxyConfig to use for generation.
## @param seed_value: Galaxy seed (1..999999).
signal start_new_galaxy(config: GalaxyConfig, seed_value: int)

## Emitted when the user chooses to load a galaxy (caller shows file dialog).
signal load_galaxy_requested()

## Emitted when the user chooses to quit the application.
signal quit_requested()

## Injected RNG for generating random galaxy seed (project rule: all randomness through RNG).
var _seeded_rng: RefCounted = null

## UI references (from scene).
var _start_button: Button = null
var _load_button: Button = null
var _quit_button: Button = null
var _type_option: OptionButton = null
var _spiral_container: Control = null
var _num_arms_spin: SpinBox = null
var _seed_spin: SpinBox = null


func _ready() -> void:
	_start_button = _find_button("StartButton")
	_load_button = _find_button("LoadButton")
	_quit_button = _find_button("QuitButton")
	_type_option = _find_option("TypeOption")
	_spiral_container = _find_control("SpiralContainer")
	var opt: Node = get_node_or_null("MarginContainer/VBox/Options")
	if opt:
		var spiral: Node = opt.get_node_or_null("SpiralContainer")
		if spiral:
			_num_arms_spin = spiral.get_node_or_null("NumArmsSpin") as SpinBox
		_seed_spin = opt.get_node_or_null("SeedSpin") as SpinBox

	if _start_button:
		_start_button.pressed.connect(_on_start_pressed)
	if _load_button:
		_load_button.pressed.connect(_on_load_pressed)
	if _quit_button:
		_quit_button.pressed.connect(_on_quit_pressed)
	if _type_option:
		_type_option.item_selected.connect(_on_type_selected)

	_update_spiral_visibility()


## Sets the RNG used for generating random galaxy seed (must be SeededRng-compatible).
## @param rng: SeededRng instance; null to skip random seed generation.
func set_seeded_rng(rng: RefCounted) -> void:
	_seeded_rng = rng


## Returns the current config from UI (or default if no UI).
## @return: GalaxyConfig built from form.
func get_current_config() -> GalaxyConfig:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	if _type_option != null:
		config.galaxy_type = _type_option.selected
	if _num_arms_spin != null and _spiral_container != null and _spiral_container.visible:
		config.num_arms = int(_num_arms_spin.value)
	return config


func _find_button(name_hint: String) -> Button:
	var node: Node = get_node_or_null("MarginContainer/VBox/Buttons/%s" % name_hint)
	if node is Button:
		return node as Button
	return null


func _find_option(name_hint: String) -> OptionButton:
	var node: Node = get_node_or_null("MarginContainer/VBox/Options/%s" % name_hint)
	if node is OptionButton:
		return node as OptionButton
	return null


func _find_control(name_hint: String) -> Control:
	var node: Node = get_node_or_null("MarginContainer/VBox/Options/%s" % name_hint)
	if node is Control:
		return node as Control
	return null


func _on_start_pressed() -> void:
	var config: GalaxyConfig = get_current_config()
	if not config.is_valid():
		push_error("WelcomeScreen: config invalid, using default")
		config = GalaxyConfig.create_default()

	var seed_value: int
	if _seed_spin != null:
		seed_value = int(_seed_spin.value)
	else:
		seed_value = _generate_random_seed()

	if seed_value == 0:
		seed_value = _generate_random_seed()
	if seed_value == 0:
		seed_value = 1

	start_new_galaxy.emit(config, seed_value)


func _on_load_pressed() -> void:
	load_galaxy_requested.emit()


func _on_quit_pressed() -> void:
	quit_requested.emit()


func _on_type_selected(_index: int) -> void:
	_update_spiral_visibility()


func _update_spiral_visibility() -> void:
	if _spiral_container == null or _type_option == null:
		return
	if _type_option.selected == GalaxySpec.GalaxyType.SPIRAL:
		_spiral_container.visible = true
	else:
		_spiral_container.visible = false


## Generates a seed in 1..999999 using injected RNG.
## @return: Random seed, or 1 if no RNG.
func _generate_random_seed() -> int:
	if _seeded_rng == null:
		return 1
	var raw: int = _seeded_rng.randi()
	var abs_raw: int = absi(raw)
	var capped: int = abs_raw % 1000000
	if capped == 0:
		return 1
	return capped


## Refreshes the random seed display (e.g. after set_seeded_rng).
func refresh_random_seed_display() -> void:
	if _seed_spin != null and _seeded_rng != null:
		_seed_spin.value = _generate_random_seed()
