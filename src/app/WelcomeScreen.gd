## Startup screen for galaxy generation with comprehensive settings.
## Features collapsible sections, presets, and full parameter exposure.
## All randomness for "random seed" goes through the injected SeededRng.
class_name WelcomeScreen
extends Control


## Emitted when the user chooses to start a new galaxy with the given config and seed.
## @param config: GalaxyConfig to use for generation.
## @param seed_value: Galaxy seed (1..999999).
signal start_new_galaxy(config: GalaxyConfig, seed_value: int)

## Emitted when the user chooses to load a galaxy (caller shows file dialog).
signal load_galaxy_requested()

## Emitted when the user chooses to quit the application.
signal quit_requested()


## Preset indices for the preset dropdown.
enum Preset {
	CUSTOM = 0,
	MILKY_WAY = 1,
	ANDROMEDA = 2,
	WHIRLPOOL = 3,
	SOMBRERO = 4,
	LARGE_MAGELLANIC_CLOUD = 5
}


## Arrow indicators for collapsible sections.
const ARROW_COLLAPSED: String = "▶"
const ARROW_EXPANDED: String = "▼"


## Injected RNG for generating random galaxy seed.
var _seeded_rng: RefCounted = null

## Tracks if we're programmatically updating UI to prevent preset reset.
var _is_updating_ui: bool = false

## Section expanded states.
var _type_section_expanded: bool = true
var _structure_section_expanded: bool = false
var _size_section_expanded: bool = false

## UI references - Buttons.
@onready var _start_button: Button = $CenterContainer/MainPanel/MarginContainer/VBox/Buttons/StartButton
@onready var _load_button: Button = $CenterContainer/MainPanel/MarginContainer/VBox/Buttons/LoadButton
@onready var _quit_button: Button = $CenterContainer/MainPanel/MarginContainer/VBox/Buttons/QuitButton
@onready var _randomize_button: Button = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SeedContainer/RandomizeButton

## UI references - Preset.
@onready var _preset_option: OptionButton = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/PresetContainer/PresetOption

## UI references - Type section.
@onready var _type_header: Button = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeHeader
@onready var _type_content: MarginContainer = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent
@onready var _type_option: OptionButton = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/TypeRow/TypeOption
@onready var _arms_row: HBoxContainer = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/ArmsRow
@onready var _arms_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsSlider
@onready var _arms_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsValue

## UI references - Structure section.
@onready var _structure_header: Button = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureHeader
@onready var _structure_content: MarginContainer = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent
@onready var _pitch_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/PitchRow/PitchSlider
@onready var _pitch_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/PitchRow/PitchValue
@onready var _amplitude_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeSlider
@onready var _amplitude_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeValue
@onready var _bulge_intensity_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensitySlider
@onready var _bulge_intensity_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensityValue
@onready var _bulge_radius_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusSlider
@onready var _bulge_radius_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusValue
@onready var _ellipticity_row: HBoxContainer = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/EllipticityRow
@onready var _ellipticity_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticitySlider
@onready var _ellipticity_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticityValue
@onready var _irregularity_row: HBoxContainer = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/IrregularityRow
@onready var _irregularity_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularitySlider
@onready var _irregularity_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularityValue

## UI references - Size section.
@onready var _size_header: Button = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeHeader
@onready var _size_content: MarginContainer = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent
@onready var _radius_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusSlider
@onready var _radius_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusValue
@onready var _disk_length_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthSlider
@onready var _disk_length_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthValue
@onready var _disk_height_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightSlider
@onready var _disk_height_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightValue
@onready var _density_slider: HSlider = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DensityRow/DensitySlider
@onready var _density_value: Label = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DensityRow/DensityValue

## UI references - Seed.
@onready var _seed_spin: SpinBox = $CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SeedContainer/SeedSpin


func _ready() -> void:
	_connect_signals()
	_update_section_visibility()
	_update_type_specific_controls()
	_update_all_value_labels()


## Sets the RNG used for generating random galaxy seed (must be SeededRng-compatible).
## @param rng: SeededRng instance; null to skip random seed generation.
func set_seeded_rng(rng: RefCounted) -> void:
	_seeded_rng = rng


## Returns the current config from UI values.
## @return: GalaxyConfig built from form values.
func get_current_config() -> GalaxyConfig:
	var config: GalaxyConfig = GalaxyConfig.new()
	
	config.galaxy_type = _type_option.selected
	config.num_arms = int(_arms_slider.value)
	config.arm_pitch_angle_deg = _pitch_slider.value
	config.arm_amplitude = _amplitude_slider.value
	config.bulge_intensity = _bulge_intensity_slider.value
	config.bulge_radius_pc = _bulge_radius_slider.value
	config.radius_pc = _radius_slider.value
	config.disk_scale_length_pc = _disk_length_slider.value
	config.disk_scale_height_pc = _disk_height_slider.value
	config.star_density_multiplier = _density_slider.value
	config.ellipticity = _ellipticity_slider.value
	config.irregularity_scale = _irregularity_slider.value
	
	return config


## Refreshes the random seed display (e.g. after set_seeded_rng).
func refresh_random_seed_display() -> void:
	if _seed_spin != null and _seeded_rng != null:
		_seed_spin.value = _generate_random_seed()


## Connects all UI signals.
func _connect_signals() -> void:
	# Buttons
	_start_button.pressed.connect(_on_start_pressed)
	_load_button.pressed.connect(_on_load_pressed)
	_quit_button.pressed.connect(_on_quit_pressed)
	_randomize_button.pressed.connect(_on_randomize_pressed)
	
	# Preset
	_preset_option.item_selected.connect(_on_preset_selected)
	
	# Section headers
	_type_header.pressed.connect(_on_type_header_pressed)
	_structure_header.pressed.connect(_on_structure_header_pressed)
	_size_header.pressed.connect(_on_size_header_pressed)
	
	# Type option
	_type_option.item_selected.connect(_on_type_changed)
	
	# Sliders - connect value_changed to update labels and reset preset to Custom
	_arms_slider.value_changed.connect(_on_arms_changed)
	_pitch_slider.value_changed.connect(_on_pitch_changed)
	_amplitude_slider.value_changed.connect(_on_amplitude_changed)
	_bulge_intensity_slider.value_changed.connect(_on_bulge_intensity_changed)
	_bulge_radius_slider.value_changed.connect(_on_bulge_radius_changed)
	_ellipticity_slider.value_changed.connect(_on_ellipticity_changed)
	_irregularity_slider.value_changed.connect(_on_irregularity_changed)
	_radius_slider.value_changed.connect(_on_radius_changed)
	_disk_length_slider.value_changed.connect(_on_disk_length_changed)
	_disk_height_slider.value_changed.connect(_on_disk_height_changed)
	_density_slider.value_changed.connect(_on_density_changed)


## Updates collapsible section visibility based on expanded states.
func _update_section_visibility() -> void:
	_type_content.visible = _type_section_expanded
	_type_header.text = "%s  Galaxy Type" % (ARROW_EXPANDED if _type_section_expanded else ARROW_COLLAPSED)
	
	_structure_content.visible = _structure_section_expanded
	_structure_header.text = "%s  Structure" % (ARROW_EXPANDED if _structure_section_expanded else ARROW_COLLAPSED)
	
	_size_content.visible = _size_section_expanded
	_size_header.text = "%s  Size & Density" % (ARROW_EXPANDED if _size_section_expanded else ARROW_COLLAPSED)


## Updates which controls are visible based on galaxy type.
func _update_type_specific_controls() -> void:
	var galaxy_type: int = _type_option.selected
	
	# Spiral arms only for spiral
	_arms_row.visible = (galaxy_type == GalaxySpec.GalaxyType.SPIRAL)
	
	# Ellipticity only for elliptical
	_ellipticity_row.visible = (galaxy_type == GalaxySpec.GalaxyType.ELLIPTICAL)
	
	# Irregularity only for irregular
	_irregularity_row.visible = (galaxy_type == GalaxySpec.GalaxyType.IRREGULAR)


## Updates all value labels to match slider values.
func _update_all_value_labels() -> void:
	_arms_value.text = "%d" % int(_arms_slider.value)
	_pitch_value.text = "%.1f°" % _pitch_slider.value
	_amplitude_value.text = "%.2f" % _amplitude_slider.value
	_bulge_intensity_value.text = "%.2f" % _bulge_intensity_slider.value
	_bulge_radius_value.text = "%d pc" % int(_bulge_radius_slider.value)
	_ellipticity_value.text = "%.2f" % _ellipticity_slider.value
	_irregularity_value.text = "%.2f" % _irregularity_slider.value
	_radius_value.text = "%d pc" % int(_radius_slider.value)
	_disk_length_value.text = "%d pc" % int(_disk_length_slider.value)
	_disk_height_value.text = "%d pc" % int(_disk_height_slider.value)
	_density_value.text = "%.1fx" % _density_slider.value


## Applies a preset configuration to all UI controls.
## @param preset: The preset index to apply.
func _apply_preset(preset: int) -> void:
	_is_updating_ui = true
	
	match preset:
		Preset.CUSTOM:
			_apply_default_values()
		Preset.MILKY_WAY:
			_apply_milky_way_preset()
		Preset.ANDROMEDA:
			_apply_andromeda_preset()
		Preset.WHIRLPOOL:
			_apply_whirlpool_preset()
		Preset.SOMBRERO:
			_apply_sombrero_preset()
		Preset.LARGE_MAGELLANIC_CLOUD:
			_apply_lmc_preset()
	
	_update_type_specific_controls()
	_update_all_value_labels()
	_is_updating_ui = false


## Applies default (Custom) values.
func _apply_default_values() -> void:
	_type_option.selected = GalaxySpec.GalaxyType.SPIRAL
	_arms_slider.value = 4.0
	_pitch_slider.value = 14.0
	_amplitude_slider.value = 0.65
	_bulge_intensity_slider.value = 0.8
	_bulge_radius_slider.value = 1500.0
	_ellipticity_slider.value = 0.3
	_irregularity_slider.value = 0.5
	_radius_slider.value = 15000.0
	_disk_length_slider.value = 4000.0
	_disk_height_slider.value = 300.0
	_density_slider.value = 1.0


## Applies Milky Way preset (4-arm barred spiral).
func _apply_milky_way_preset() -> void:
	_type_option.selected = GalaxySpec.GalaxyType.SPIRAL
	_arms_slider.value = 4.0
	_pitch_slider.value = 14.0
	_amplitude_slider.value = 0.65
	_bulge_intensity_slider.value = 0.8
	_bulge_radius_slider.value = 1500.0
	_radius_slider.value = 15000.0
	_disk_length_slider.value = 4000.0
	_disk_height_slider.value = 300.0
	_density_slider.value = 1.0


## Applies Andromeda preset (2-arm with large bulge).
func _apply_andromeda_preset() -> void:
	_type_option.selected = GalaxySpec.GalaxyType.SPIRAL
	_arms_slider.value = 2.0
	_pitch_slider.value = 20.0
	_amplitude_slider.value = 0.55
	_bulge_intensity_slider.value = 1.0
	_bulge_radius_slider.value = 2200.0
	_radius_slider.value = 22000.0
	_disk_length_slider.value = 5500.0
	_disk_height_slider.value = 400.0
	_density_slider.value = 1.2


## Applies Whirlpool preset (2-arm grand design spiral).
func _apply_whirlpool_preset() -> void:
	_type_option.selected = GalaxySpec.GalaxyType.SPIRAL
	_arms_slider.value = 2.0
	_pitch_slider.value = 18.0
	_amplitude_slider.value = 0.85
	_bulge_intensity_slider.value = 0.6
	_bulge_radius_slider.value = 1200.0
	_radius_slider.value = 12000.0
	_disk_length_slider.value = 3500.0
	_disk_height_slider.value = 250.0
	_density_slider.value = 1.0


## Applies Sombrero preset (elliptical/lenticular with huge bulge).
func _apply_sombrero_preset() -> void:
	_type_option.selected = GalaxySpec.GalaxyType.ELLIPTICAL
	_bulge_intensity_slider.value = 1.2
	_bulge_radius_slider.value = 2500.0
	_ellipticity_slider.value = 0.6
	_radius_slider.value = 15000.0
	_disk_length_slider.value = 4000.0
	_disk_height_slider.value = 450.0
	_density_slider.value = 1.3


## Applies Large Magellanic Cloud preset (irregular dwarf).
func _apply_lmc_preset() -> void:
	_type_option.selected = GalaxySpec.GalaxyType.IRREGULAR
	_bulge_intensity_slider.value = 0.4
	_bulge_radius_slider.value = 1000.0
	_irregularity_slider.value = 0.7
	_radius_slider.value = 10000.0
	_disk_length_slider.value = 2500.0
	_disk_height_slider.value = 350.0
	_density_slider.value = 0.7


## Sets preset to Custom when user manually changes a value.
func _mark_as_custom() -> void:
	if not _is_updating_ui and _preset_option.selected != Preset.CUSTOM:
		_preset_option.selected = Preset.CUSTOM


## Generates a seed in 1..999999 using injected RNG.
## @return: Random seed, or 1 if no RNG.
func _generate_random_seed() -> int:
	if _seeded_rng == null:
		return randi_range(1, 999999)
	var raw: int = _seeded_rng.randi()
	var abs_raw: int = absi(raw)
	var capped: int = abs_raw % 1000000
	if capped == 0:
		return 1
	return capped


# Signal handlers

func _on_start_pressed() -> void:
	var config: GalaxyConfig = get_current_config()
	if not config.is_valid():
		push_error("WelcomeScreen: config invalid, using default")
		config = GalaxyConfig.create_default()
	
	var seed_value: int = int(_seed_spin.value)
	if seed_value == 0:
		seed_value = _generate_random_seed()
	
	start_new_galaxy.emit(config, seed_value)


func _on_load_pressed() -> void:
	load_galaxy_requested.emit()


func _on_quit_pressed() -> void:
	quit_requested.emit()


func _on_randomize_pressed() -> void:
	_seed_spin.value = _generate_random_seed()


func _on_preset_selected(index: int) -> void:
	_apply_preset(index)


func _on_type_header_pressed() -> void:
	_type_section_expanded = not _type_section_expanded
	_update_section_visibility()


func _on_structure_header_pressed() -> void:
	_structure_section_expanded = not _structure_section_expanded
	_update_section_visibility()


func _on_size_header_pressed() -> void:
	_size_section_expanded = not _size_section_expanded
	_update_section_visibility()


func _on_type_changed(_index: int) -> void:
	_update_type_specific_controls()
	_mark_as_custom()


func _on_arms_changed(value: float) -> void:
	_arms_value.text = "%d" % int(value)
	_mark_as_custom()


func _on_pitch_changed(value: float) -> void:
	_pitch_value.text = "%.1f°" % value
	_mark_as_custom()


func _on_amplitude_changed(value: float) -> void:
	_amplitude_value.text = "%.2f" % value
	_mark_as_custom()


func _on_bulge_intensity_changed(value: float) -> void:
	_bulge_intensity_value.text = "%.2f" % value
	_mark_as_custom()


func _on_bulge_radius_changed(value: float) -> void:
	_bulge_radius_value.text = "%d pc" % int(value)
	_mark_as_custom()


func _on_ellipticity_changed(value: float) -> void:
	_ellipticity_value.text = "%.2f" % value
	_mark_as_custom()


func _on_irregularity_changed(value: float) -> void:
	_irregularity_value.text = "%.2f" % value
	_mark_as_custom()


func _on_radius_changed(value: float) -> void:
	_radius_value.text = "%d pc" % int(value)
	_mark_as_custom()


func _on_disk_length_changed(value: float) -> void:
	_disk_length_value.text = "%d pc" % int(value)
	_mark_as_custom()


func _on_disk_height_changed(value: float) -> void:
	_disk_height_value.text = "%d pc" % int(value)
	_mark_as_custom()


func _on_density_changed(value: float) -> void:
	_density_value.text = "%.1fx" % value
	_mark_as_custom()
