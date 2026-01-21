## Dialog for editing celestial body properties.
## Displays editable properties with slider and spinbox controls.
class_name EditDialog
extends Window

const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _body_renderer := preload("res://src/app/rendering/BodyRenderer.gd")
const _property_formatter := preload("res://src/app/viewer/PropertyFormatter.gd")

## Signal emitted when the user confirms edits
signal edits_confirmed(body: CelestialBody)

## Signal emitted when the user cancels edits
signal edits_cancelled()

## The celestial body being edited
var _body: CelestialBody = null

## Original values for revert functionality
var _original_values: Dictionary = {}

## Current working values being edited
var _working_values: Dictionary = {}

## Confirmed values (updated on slider release)
var _confirmed_values: Dictionary = {}

## References to derived value labels for updating
var _derived_labels: Dictionary = {}

## References to derived preview labels (shown during drag)
var _derived_preview_labels: Dictionary = {}

## Whether a slider is currently being dragged
var _is_dragging: bool = false

## Dictionary mapping property paths to their editor controls
var _property_editors: Dictionary = {}

## Content container
@onready var _content: VBoxContainer = $MarginContainer/VBoxContainer/ContentSplit/ScrollContainer/ContentMargin/ContentContainer

## Preview viewport elements
@onready var _preview_viewport: SubViewport = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport
@onready var _preview_camera: Camera3D = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/Camera3D
@onready var _preview_body_renderer: BodyRenderer = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/BodyRenderer
@onready var _preview_light: DirectionalLight3D = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/PreviewEnvironment/DirectionalLight3D

## Buttons
@onready var _revert_button: Button = $MarginContainer/VBoxContainer/ButtonContainer/RevertButton
@onready var _confirm_button: Button = $MarginContainer/VBoxContainer/ButtonContainer/ConfirmButton
@onready var _cancel_button: Button = $MarginContainer/VBoxContainer/ButtonContainer/CancelButton

## Current section content for adding properties
var _current_section_content: VBoxContainer = null

## Color constants
const SECTION_COLOR: Color = Color(0.9, 0.9, 0.9)
const LABEL_COLOR: Color = Color(0.6, 0.6, 0.6)
const DERIVED_COLOR: Color = Color(0.5, 0.6, 0.7)
const DERIVED_PREVIEW_COLOR: Color = Color(0.6, 0.7, 0.4)
const WARNING_COLOR: Color = Color(0.9, 0.6, 0.2)
const LABEL_MIN_WIDTH: float = 120.0

## Density constraints by body type (kg/m³)
## Stars: red giants (~0.0001) to red dwarfs (~100,000)
const STAR_MIN_DENSITY: float = 0.0001
const STAR_MAX_DENSITY: float = 150000.0

## Gas giants: Saturn (~687) to dense gas giants (~3000)
const GAS_GIANT_MIN_DENSITY: float = 300.0
const GAS_GIANT_MAX_DENSITY: float = 3500.0

## Rocky planets: Mars-like (~3900) to super-dense (~8000)
const ROCKY_MIN_DENSITY: float = 2500.0
const ROCKY_MAX_DENSITY: float = 10000.0

## Moons: icy (~1000) to rocky (~5500)
const MOON_MIN_DENSITY: float = 800.0
const MOON_MAX_DENSITY: float = 6000.0

## Asteroids: rubble pile (~1000) to metallic (~8000)
const ASTEROID_MIN_DENSITY: float = 500.0
const ASTEROID_MAX_DENSITY: float = 8000.0

## Spectral class options for stars
const SPECTRAL_CLASSES: Array[String] = [
	"O0", "O5", "O9",
	"B0", "B5", "B9",
	"A0", "A5", "A9",
	"F0", "F5", "F9",
	"G0", "G2", "G5", "G9",
	"K0", "K5", "K9",
	"M0", "M5", "M9"
]

## Luminosity class options
const LUMINOSITY_CLASSES: Array[String] = [
	"Ia", "Ib", "II", "III", "IV", "V", "VI", "VII"
]


func _ready() -> void:
	close_requested.connect(_on_cancel_pressed)
	_revert_button.pressed.connect(_on_revert_pressed)
	_confirm_button.pressed.connect(_on_confirm_pressed)
	_cancel_button.pressed.connect(_on_cancel_pressed)


## Opens the dialog for editing the given body.
## @param body: The celestial body to edit.
func open_for_body(body: CelestialBody) -> void:
	if not body:
		return
	
	_body = body
	title = "Edit: %s" % (body.name if body.name else body.id)
	
	# Extract and store original values
	_extract_values_from_body()
	_original_values = _working_values.duplicate(true)
	_confirmed_values = _working_values.duplicate(true)
	
	# Apply working values to body for initial display
	_apply_values_to_body()
	
	_build_editor_ui()
	_update_preview()
	popup_centered()


## Extracts all editable values from the body into _working_values.
func _extract_values_from_body() -> void:
	_working_values.clear()
	
	if not _body:
		return
	
	# Basic info
	_working_values["name"] = _body.name if _body.name else _body.id
	
	# Physical properties (store in base units)
	_working_values["physical.mass_kg"] = _body.physical.mass_kg
	_working_values["physical.radius_m"] = _body.physical.radius_m
	_working_values["physical.rotation_period_s"] = _body.physical.rotation_period_s
	_working_values["physical.axial_tilt_deg"] = _body.physical.axial_tilt_deg
	_working_values["physical.oblateness"] = _body.physical.oblateness
	
	# Stellar properties
	if _body.has_stellar():
		_working_values["stellar.temperature_k"] = _body.stellar.effective_temperature_k
		_working_values["stellar.luminosity_watts"] = _body.stellar.luminosity_watts
		_working_values["stellar.age_years"] = _body.stellar.age_years
		_working_values["stellar.metallicity"] = _body.stellar.metallicity
		_working_values["stellar.spectral_class"] = _body.stellar.spectral_class
		# Parse spectral class into components for editing
		var parsed: Dictionary = _parse_spectral_class(_body.stellar.spectral_class)
		_working_values["stellar.spectral_type"] = parsed.get("type", "G2")
		_working_values["stellar.luminosity_class"] = parsed.get("luminosity", "V")
	
	# Orbital properties
	if _body.has_orbital():
		_working_values["orbital.semi_major_axis_m"] = _body.orbital.semi_major_axis_m
		_working_values["orbital.eccentricity"] = _body.orbital.eccentricity
		_working_values["orbital.inclination_deg"] = _body.orbital.inclination_deg
	
	# Atmosphere properties
	if _body.has_atmosphere():
		_working_values["atmosphere.surface_pressure_pa"] = _body.atmosphere.surface_pressure_pa
		_working_values["atmosphere.scale_height_m"] = _body.atmosphere.scale_height_m
		_working_values["atmosphere.greenhouse_factor"] = _body.atmosphere.greenhouse_factor
	
	# Surface properties
	if _body.has_surface():
		_working_values["surface.temperature_k"] = _body.surface.temperature_k
		_working_values["surface.albedo"] = _body.surface.albedo
		_working_values["surface.volcanism_level"] = _body.surface.volcanism_level


## Applies working values to the body.
func _apply_values_to_body() -> void:
	if not _body:
		return
	
	# Basic info
	_body.name = _working_values.get("name", _body.name)
	
	# Physical properties
	_body.physical.mass_kg = _working_values.get("physical.mass_kg", _body.physical.mass_kg)
	_body.physical.radius_m = _working_values.get("physical.radius_m", _body.physical.radius_m)
	_body.physical.rotation_period_s = _working_values.get("physical.rotation_period_s", _body.physical.rotation_period_s)
	_body.physical.axial_tilt_deg = _working_values.get("physical.axial_tilt_deg", _body.physical.axial_tilt_deg)
	_body.physical.oblateness = _working_values.get("physical.oblateness", _body.physical.oblateness)
	
	# Stellar properties
	if _body.has_stellar():
		_body.stellar.effective_temperature_k = _working_values.get("stellar.temperature_k", _body.stellar.effective_temperature_k)
		_body.stellar.luminosity_watts = _working_values.get("stellar.luminosity_watts", _body.stellar.luminosity_watts)
		_body.stellar.age_years = _working_values.get("stellar.age_years", _body.stellar.age_years)
		_body.stellar.metallicity = _working_values.get("stellar.metallicity", _body.stellar.metallicity)
		# Combine spectral type and luminosity class
		var spectral_type: String = _working_values.get("stellar.spectral_type", "G2")
		var luminosity_class: String = _working_values.get("stellar.luminosity_class", "V")
		_body.stellar.spectral_class = spectral_type + luminosity_class
	
	# Orbital properties
	if _body.has_orbital():
		_body.orbital.semi_major_axis_m = _working_values.get("orbital.semi_major_axis_m", _body.orbital.semi_major_axis_m)
		_body.orbital.eccentricity = _working_values.get("orbital.eccentricity", _body.orbital.eccentricity)
		_body.orbital.inclination_deg = _working_values.get("orbital.inclination_deg", _body.orbital.inclination_deg)
	
	# Atmosphere properties
	if _body.has_atmosphere():
		_body.atmosphere.surface_pressure_pa = _working_values.get("atmosphere.surface_pressure_pa", _body.atmosphere.surface_pressure_pa)
		_body.atmosphere.scale_height_m = _working_values.get("atmosphere.scale_height_m", _body.atmosphere.scale_height_m)
		_body.atmosphere.greenhouse_factor = _working_values.get("atmosphere.greenhouse_factor", _body.atmosphere.greenhouse_factor)
	
	# Surface properties
	if _body.has_surface():
		_body.surface.temperature_k = _working_values.get("surface.temperature_k", _body.surface.temperature_k)
		_body.surface.albedo = _working_values.get("surface.albedo", _body.surface.albedo)
		_body.surface.volcanism_level = _working_values.get("surface.volcanism_level", _body.surface.volcanism_level)


## Parses a spectral class string into type and luminosity class.
## @param spectral_class: The full spectral class string (e.g., "G2V").
## @return: Dictionary with "type" and "luminosity" keys.
func _parse_spectral_class(spectral_class: String) -> Dictionary:
	var result: Dictionary = {"type": "G2", "luminosity": "V"}
	
	if spectral_class.is_empty():
		return result
	
	# Find where luminosity class starts (Roman numerals)
	var lum_start: int = -1
	for i in range(spectral_class.length()):
		var c: String = spectral_class[i]
		if c == "I" or c == "V":
			# Check if this is part of luminosity class
			if i > 0:
				lum_start = i
				break
	
	if lum_start > 0:
		result["type"] = spectral_class.substr(0, lum_start)
		result["luminosity"] = spectral_class.substr(lum_start)
	else:
		result["type"] = spectral_class
	
	return result


## Gets the density constraints for the current body type.
## @return: Vector2(min_density, max_density) in kg/m³.
func _get_density_constraints() -> Vector2:
	match _body.type:
		CelestialType.Type.STAR:
			return Vector2(STAR_MIN_DENSITY, STAR_MAX_DENSITY)
		CelestialType.Type.PLANET:
			# Check if likely gas giant based on current radius
			var radius_m: float = _working_values.get("physical.radius_m", Units.EARTH_RADIUS_METERS)
			if radius_m > 2.5 * Units.EARTH_RADIUS_METERS:
				return Vector2(GAS_GIANT_MIN_DENSITY, GAS_GIANT_MAX_DENSITY)
			else:
				return Vector2(ROCKY_MIN_DENSITY, ROCKY_MAX_DENSITY)
		CelestialType.Type.MOON:
			return Vector2(MOON_MIN_DENSITY, MOON_MAX_DENSITY)
		CelestialType.Type.ASTEROID:
			return Vector2(ASTEROID_MIN_DENSITY, ASTEROID_MAX_DENSITY)
		_:
			return Vector2(500.0, 10000.0)


## Calculates the valid radius range for current mass based on density limits.
## @return: Vector2(min_radius, max_radius) in display units.
func _get_valid_radius_range_for_current_mass() -> Vector2:
	var mass_kg: float = _working_values.get("physical.mass_kg", 1.0)
	var density_constraints: Vector2 = _get_density_constraints()
	var radius_range: Vector2 = _get_radius_range_for_mass(mass_kg, density_constraints)
	
	# Convert to display units
	match _body.type:
		CelestialType.Type.STAR:
			return Vector2(radius_range.x / Units.SOLAR_RADIUS_METERS, radius_range.y / Units.SOLAR_RADIUS_METERS)
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			return Vector2(radius_range.x / Units.EARTH_RADIUS_METERS, radius_range.y / Units.EARTH_RADIUS_METERS)
		_:
			return Vector2(radius_range.x / 1000.0, radius_range.y / 1000.0)


## Calculates the valid mass range for current radius based on density limits.
## @return: Vector2(min_mass, max_mass) in display units.
func _get_valid_mass_range_for_current_radius() -> Vector2:
	var radius_m: float = _working_values.get("physical.radius_m", 1.0)
	var density_constraints: Vector2 = _get_density_constraints()
	var mass_range: Vector2 = _get_mass_range_for_radius(radius_m, density_constraints)
	
	# Convert to display units
	match _body.type:
		CelestialType.Type.STAR:
			return Vector2(mass_range.x / Units.SOLAR_MASS_KG, mass_range.y / Units.SOLAR_MASS_KG)
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			return Vector2(mass_range.x / Units.EARTH_MASS_KG, mass_range.y / Units.EARTH_MASS_KG)
		_:
			return Vector2(mass_range.x / 1e15, mass_range.y / 1e15)


## Calculates valid radius range for a given mass based on density limits (base units).
## @param mass_kg: The mass in kg.
## @param density_constraints: Vector2(min_density, max_density) in kg/m³
## @return: Vector2 with (min_radius, max_radius) in meters.
func _get_radius_range_for_mass(mass_kg: float, density_constraints: Vector2) -> Vector2:
	if mass_kg <= 0:
		return Vector2(0.0, 1e20)
	var min_volume: float = mass_kg / density_constraints.y  # max density gives min volume
	var max_volume: float = mass_kg / density_constraints.x  # min density gives max volume
	var min_radius: float = pow(3.0 * min_volume / (4.0 * PI), 1.0 / 3.0)
	var max_radius: float = pow(3.0 * max_volume / (4.0 * PI), 1.0 / 3.0)
	return Vector2(min_radius, max_radius)


## Calculates valid mass range for a given radius based on density limits (base units).
## @param radius_m: The radius in meters.
## @param density_constraints: Vector2(min_density, max_density) in kg/m³
## @return: Vector2 with (min_mass, max_mass) in kg.
func _get_mass_range_for_radius(radius_m: float, density_constraints: Vector2) -> Vector2:
	if radius_m <= 0:
		return Vector2(0.0, 1e40)
	var volume: float = (4.0 / 3.0) * PI * pow(radius_m, 3)
	var min_mass: float = volume * density_constraints.x
	var max_mass: float = volume * density_constraints.y
	return Vector2(min_mass, max_mass)


## Checks if current density is within valid range.
## @return: True if density is valid, false otherwise.
func _is_density_valid() -> bool:
	var density: float = _body.physical.get_density_kg_m3()
	var density_constraints: Vector2 = _get_density_constraints()
	return density >= density_constraints.x and density <= density_constraints.y


## Builds the editor UI for the current body.
func _build_editor_ui() -> void:
	_clear_content()
	_derived_labels.clear()
	_derived_preview_labels.clear()
	
	if not _body:
		return
	
	# Basic info
	_add_section("Basic Info")
	_add_string_property("name", "Name", _body.name if _body.name else _body.id)
	_add_derived_property("Type", _format_type(_body))
	_add_derived_property("ID", _body.id)
	
	# Physical properties
	_add_section("Physical Properties")
	_add_physical_properties()
	
	# Stellar properties (stars only)
	if _body.has_stellar():
		_add_section("Stellar Properties")
		_add_stellar_properties()
	
	# Orbital properties
	if _body.has_orbital():
		_add_section("Orbital Properties")
		_add_orbital_properties()
	
	# Atmosphere
	if _body.has_atmosphere():
		_add_section("Atmosphere")
		_add_atmosphere_properties()
	
	# Surface
	if _body.has_surface():
		_add_section("Surface")
		_add_surface_properties()




## Gets the display value for a property (converts from base units).
## @param property_path: The property path.
## @return: Value in display units.
func _get_display_value_for_property(property_path: String) -> float:
	var base_value: float = _working_values.get(property_path, 0.0)
	
	match property_path:
		"physical.mass_kg":
			match _body.type:
				CelestialType.Type.STAR:
					return base_value / Units.SOLAR_MASS_KG
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					return base_value / Units.EARTH_MASS_KG
				_:
					return base_value / 1e15
		"physical.radius_m":
			match _body.type:
				CelestialType.Type.STAR:
					return base_value / Units.SOLAR_RADIUS_METERS
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					return base_value / Units.EARTH_RADIUS_METERS
				_:
					return base_value / 1000.0
		"physical.rotation_period_s":
			return absf(base_value) / 3600.0  # To hours
		"stellar.luminosity_watts":
			return base_value / 3.828e26  # To solar luminosities
		"stellar.age_years":
			return base_value / 1e9  # To Gyr
		"orbital.semi_major_axis_m":
			return base_value / Units.AU_METERS
		"atmosphere.surface_pressure_pa":
			return base_value / 101325.0  # To atm
		"atmosphere.scale_height_m":
			return base_value / 1000.0  # To km
		_:
			return base_value


## Converts a constraint from base units to display units.
## @param property_path: The property path.
## @param constraint: Vector2(min, max) in base units.
## @return: Vector2(min, max) in display units.
func _convert_constraint_to_display(property_path: String, constraint: Vector2) -> Vector2:
	var factor: float = 1.0
	
	match property_path:
		"physical.mass_kg":
			match _body.type:
				CelestialType.Type.STAR:
					factor = Units.SOLAR_MASS_KG
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					factor = Units.EARTH_MASS_KG
				_:
					factor = 1e15
		"physical.radius_m":
			match _body.type:
				CelestialType.Type.STAR:
					factor = Units.SOLAR_RADIUS_METERS
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					factor = Units.EARTH_RADIUS_METERS
				_:
					factor = 1000.0
		"physical.rotation_period_s":
			factor = 3600.0
		"stellar.luminosity_watts":
			factor = 3.828e26
		"stellar.age_years":
			factor = 1e9
		"orbital.semi_major_axis_m":
			factor = Units.AU_METERS
		"atmosphere.surface_pressure_pa":
			factor = 101325.0
		"atmosphere.scale_height_m":
			factor = 1000.0
	
	return Vector2(constraint.x / factor, constraint.y / factor)


## Clears all content from the dialog.
func _clear_content() -> void:
	_property_editors.clear()
	if not _content:
		return
	
	for child in _content.get_children():
		child.queue_free()
	
	_current_section_content = null


## Adds a collapsible section.
## @param title_text: The section title.
func _add_section(title_text: String) -> void:
	var section: VBoxContainer = VBoxContainer.new()
	section.add_theme_constant_override("separation", 5)
	
	var header: Label = Label.new()
	header.text = title_text
	header.add_theme_font_size_override("font_size", 16)
	header.add_theme_color_override("font_color", SECTION_COLOR)
	section.add_child(header)
	
	var separator: HSeparator = HSeparator.new()
	section.add_child(separator)
	
	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 12)
	section.add_child(content)
	
	_content.add_child(section)
	_current_section_content = content


## Adds a numeric property editor with slider and spinbox.
## @param property_path: The property path identifier.
## @param label_text: The display label.
## @param value: The current value.
## @param min_value: Minimum allowed value.
## @param max_value: Maximum allowed value.
## @param step: Step increment for controls.
## @param suffix: Unit suffix to display.
func _add_numeric_property(
	property_path: String,
	label_text: String,
	value: float,
	min_value: float,
	max_value: float,
	step: float = 0.01,
	suffix: String = ""
) -> void:
	if not _current_section_content:
		return
	
	var container: VBoxContainer = VBoxContainer.new()
	container.add_theme_constant_override("separation", 4)
	
	# Label
	var label: Label = Label.new()
	label.text = label_text
	label.add_theme_color_override("font_color", LABEL_COLOR)
	label.add_theme_font_size_override("font_size", 13)
	container.add_child(label)
	
	# Controls row
	var controls: HBoxContainer = HBoxContainer.new()
	controls.add_theme_constant_override("separation", 10)
	
	# Slider
	var slider: HSlider = HSlider.new()
	slider.min_value = min_value
	slider.max_value = max_value
	slider.step = step
	slider.value = value
	slider.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	slider.custom_minimum_size.x = 120
	controls.add_child(slider)
	
	# SpinBox
	var spinbox: SpinBox = SpinBox.new()
	spinbox.min_value = min_value
	spinbox.max_value = max_value
	spinbox.step = step
	spinbox.value = value
	spinbox.suffix = suffix
	spinbox.custom_minimum_size.x = 150
	spinbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	controls.add_child(spinbox)
	
	# Revert button for this property
	var revert_btn: Button = Button.new()
	revert_btn.text = "↺"
	revert_btn.tooltip_text = "Revert to original value"
	revert_btn.custom_minimum_size = Vector2(28, 28)
	revert_btn.pressed.connect(_on_property_revert_pressed.bind(property_path, slider, spinbox))
	controls.add_child(revert_btn)
	
	container.add_child(controls)
	
	# Valid range indicator (for mass/radius)
	var range_label: Label = null
	if property_path == "physical.mass_kg" or property_path == "physical.radius_m":
		range_label = Label.new()
		range_label.add_theme_font_size_override("font_size", 10)
		range_label.add_theme_color_override("font_color", Color(0.5, 0.5, 0.5))
		container.add_child(range_label)
		_update_range_indicator(property_path, range_label)
	
	_current_section_content.add_child(container)
	
	# Connect signals to keep them in sync
	slider.value_changed.connect(_on_slider_value_changed.bind(property_path, spinbox))
	slider.drag_started.connect(_on_slider_drag_started)
	slider.drag_ended.connect(_on_slider_drag_ended.bind(property_path, slider, spinbox))
	spinbox.value_changed.connect(_on_spinbox_value_changed.bind(property_path, slider))
	
	# Store reference
	_property_editors[property_path] = {
		"slider": slider,
		"spinbox": spinbox,
		"revert_btn": revert_btn,
		"range_label": range_label,
		"min": min_value,
		"max": max_value,
		"step": step
	}


## Updates the valid range indicator for a property.
## @param property_path: The property path.
## @param range_label: The label to update.
func _update_range_indicator(property_path: String, range_label: Label) -> void:
	if not range_label:
		return
	
	var valid_range: Vector2
	var is_valid: bool = _is_density_valid()
	
	if property_path == "physical.mass_kg":
		valid_range = _get_valid_mass_range_for_current_radius()
		var suffix: String = _get_mass_suffix()
		range_label.text = "Valid range: %.4f - %.4f%s (for current radius)" % [valid_range.x, valid_range.y, suffix]
	elif property_path == "physical.radius_m":
		valid_range = _get_valid_radius_range_for_current_mass()
		var suffix: String = _get_radius_suffix()
		range_label.text = "Valid range: %.4f - %.4f%s (for current mass)" % [valid_range.x, valid_range.y, suffix]
	
	if is_valid:
		range_label.add_theme_color_override("font_color", Color(0.5, 0.5, 0.5))
	else:
		range_label.add_theme_color_override("font_color", WARNING_COLOR)


## Gets the mass suffix for current body type.
func _get_mass_suffix() -> String:
	match _body.type:
		CelestialType.Type.STAR:
			return " M☉"
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			return " M⊕"
		_:
			return " ×10¹⁵kg"


## Gets the radius suffix for current body type.
func _get_radius_suffix() -> String:
	match _body.type:
		CelestialType.Type.STAR:
			return " R☉"
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			return " R⊕"
		_:
			return " km"


## Adds a string property editor (just line edit).
## @param property_path: The property path identifier.
## @param label_text: The display label.
## @param value: The current value.
func _add_string_property(property_path: String, label_text: String, value: String) -> void:
	if not _current_section_content:
		return
	
	var container: VBoxContainer = VBoxContainer.new()
	container.add_theme_constant_override("separation", 4)
	
	var label: Label = Label.new()
	label.text = label_text
	label.add_theme_color_override("font_color", LABEL_COLOR)
	label.add_theme_font_size_override("font_size", 13)
	container.add_child(label)
	
	var line_edit: LineEdit = LineEdit.new()
	line_edit.text = value
	line_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	line_edit.text_changed.connect(_on_name_changed)
	container.add_child(line_edit)
	
	# Revert button
	var revert_btn: Button = Button.new()
	revert_btn.text = "↺"
	revert_btn.tooltip_text = "Revert to original value"
	revert_btn.custom_minimum_size = Vector2(28, 28)
	revert_btn.pressed.connect(_on_string_property_revert_pressed.bind(property_path, line_edit))
	container.add_child(revert_btn)
	
	_current_section_content.add_child(container)
	
	_property_editors[property_path] = {
		"line_edit": line_edit,
		"revert_btn": revert_btn
	}


## Adds an option button property editor.
## @param property_path: The property path identifier.
## @param label_text: The display label.
## @param current_value: The current selected value.
## @param options: Array of option strings.
func _add_option_property(property_path: String, label_text: String, current_value: String, options: Array) -> void:
	if not _current_section_content:
		return
	
	var container: VBoxContainer = VBoxContainer.new()
	container.add_theme_constant_override("separation", 4)
	
	var label: Label = Label.new()
	label.text = label_text
	label.add_theme_color_override("font_color", LABEL_COLOR)
	label.add_theme_font_size_override("font_size", 13)
	container.add_child(label)
	
	var controls: HBoxContainer = HBoxContainer.new()
	controls.add_theme_constant_override("separation", 10)
	
	var option_button: OptionButton = OptionButton.new()
	option_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	
	var selected_idx: int = 0
	for i in range(options.size()):
		option_button.add_item(options[i])
		if options[i] == current_value:
			selected_idx = i
	option_button.selected = selected_idx
	
	option_button.item_selected.connect(_on_option_selected.bind(property_path, options))
	controls.add_child(option_button)
	
	# Revert button
	var revert_btn: Button = Button.new()
	revert_btn.text = "↺"
	revert_btn.tooltip_text = "Revert to original value"
	revert_btn.custom_minimum_size = Vector2(28, 28)
	revert_btn.pressed.connect(_on_option_revert_pressed.bind(property_path, option_button, options))
	controls.add_child(revert_btn)
	
	container.add_child(controls)
	_current_section_content.add_child(container)
	
	_property_editors[property_path] = {
		"option_button": option_button,
		"revert_btn": revert_btn,
		"options": options
	}


## Called when an option button selection changes.
## @param index: The selected index.
## @param property_path: The property path.
## @param options: The options array.
func _on_option_selected(index: int, property_path: String, options: Array) -> void:
	if index >= 0 and index < options.size():
		_working_values[property_path] = options[index]
		_confirmed_values[property_path] = options[index]
		_apply_values_to_body()
		_update_preview()
		
		# Update title if spectral class changed
		if property_path == "stellar.spectral_type" or property_path == "stellar.luminosity_class":
			_update_type_display()


## Handles per-property revert for option buttons.
## @param property_path: The property to revert.
## @param option_button: The option button control.
## @param options: The options array.
func _on_option_revert_pressed(property_path: String, option_button: OptionButton, options: Array) -> void:
	if not _original_values.has(property_path):
		return
	
	_working_values[property_path] = _original_values[property_path]
	_confirmed_values[property_path] = _original_values[property_path]
	
	# Find index of original value
	var original_value: String = str(_original_values[property_path])
	for i in range(options.size()):
		if options[i] == original_value:
			option_button.selected = i
			break
	
	_apply_values_to_body()
	_update_preview()
	_update_type_display()


## Called when name changes.
func _on_name_changed(new_text: String) -> void:
	_working_values["name"] = new_text
	_apply_values_to_body()


## Called when slider value changes during drag.
## @param value: The new slider value.
## @param property_path: The property being edited.
## @param spinbox: The spinbox to update.
func _on_slider_value_changed(value: float, property_path: String, spinbox: SpinBox) -> void:
	spinbox.set_value_no_signal(value)
	
	# Update working value (proposed, not finalized if dragging)
	_set_working_value_from_display(property_path, value)
	
	# Update body and preview
	_apply_values_to_body()
	_update_preview()
	
	# Show preview derived values during drag
	if _is_dragging:
		_show_derived_previews()
		_update_range_indicators()


## Called when slider drag starts.
func _on_slider_drag_started() -> void:
	_is_dragging = true


## Called when slider drag ends.
## @param property_path: The property that was edited.
## @param slider: The slider control.
## @param spinbox: The spinbox control.
func _on_slider_drag_ended(value_changed: bool, property_path: String, slider: HSlider, spinbox: SpinBox) -> void:
	_is_dragging = false
	
	if value_changed:
		# Finalize the value and cascade constraints
		_finalize_property_change(property_path)
	
	# Hide previews and show confirmed values
	_hide_derived_previews()
	_update_derived_displays()


## Called when spinbox value changes.
## @param value: The new spinbox value.
## @param property_path: The property being edited.
## @param slider: The slider to update.
func _on_spinbox_value_changed(value: float, property_path: String, slider: HSlider) -> void:
	slider.set_value_no_signal(value)
	
	# Update working value
	_set_working_value_from_display(property_path, value)
	
	# Spinbox changes are immediate, so finalize
	_finalize_property_change(property_path)
	_update_derived_displays()
	_update_range_indicators()


## Sets a working value from a display value (converts units).
## @param property_path: The property path.
## @param display_value: The value in display units.
func _set_working_value_from_display(property_path: String, display_value: float) -> void:
	var base_value: float = display_value
	
	match property_path:
		"physical.mass_kg":
			match _body.type:
				CelestialType.Type.STAR:
					base_value = display_value * Units.SOLAR_MASS_KG
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					base_value = display_value * Units.EARTH_MASS_KG
				_:
					base_value = display_value * 1e15
		"physical.radius_m":
			match _body.type:
				CelestialType.Type.STAR:
					base_value = display_value * Units.SOLAR_RADIUS_METERS
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					base_value = display_value * Units.EARTH_RADIUS_METERS
				_:
					base_value = display_value * 1000.0
		"physical.rotation_period_s":
			base_value = display_value * 3600.0  # From hours
		"stellar.luminosity_watts":
			base_value = display_value * 3.828e26  # From solar luminosities
		"stellar.age_years":
			base_value = display_value * 1e9  # From Gyr
		"orbital.semi_major_axis_m":
			base_value = display_value * Units.AU_METERS
		"atmosphere.surface_pressure_pa":
			base_value = display_value * 101325.0  # From atm
		"atmosphere.scale_height_m":
			base_value = display_value * 1000.0  # From km
	
	_working_values[property_path] = base_value


## Finalizes a property change and cascades constraints.
## @param changed_property: The property that was changed.
func _finalize_property_change(changed_property: String) -> void:
	# Update confirmed values
	_confirmed_values = _working_values.duplicate(true)
	
	# Apply to body
	_apply_values_to_body()
	
	_update_preview()
	_update_derived_displays()
	_update_range_indicators()


## Updates all range indicators.
func _update_range_indicators() -> void:
	if _property_editors.has("physical.mass_kg"):
		var editor_data: Dictionary = _property_editors["physical.mass_kg"]
		if editor_data.has("range_label") and editor_data["range_label"]:
			_update_range_indicator("physical.mass_kg", editor_data["range_label"])
	
	if _property_editors.has("physical.radius_m"):
		var editor_data: Dictionary = _property_editors["physical.radius_m"]
		if editor_data.has("range_label") and editor_data["range_label"]:
			_update_range_indicator("physical.radius_m", editor_data["range_label"])


## Shows preview values for derived properties during drag.
func _show_derived_previews() -> void:
	if not _body:
		return
	
	# Physical derived values
	if _derived_preview_labels.has("Density"):
		var preview: Label = _derived_preview_labels["Density"]
		preview.text = " → %.1f kg/m³" % _body.physical.get_density_kg_m3()
		preview.visible = true
	
	if _derived_preview_labels.has("Surface Gravity"):
		var preview: Label = _derived_preview_labels["Surface Gravity"]
		preview.text = " → %.2f m/s²" % _body.physical.get_surface_gravity_m_s2()
		preview.visible = true
	
	if _derived_preview_labels.has("Escape Velocity"):
		var preview: Label = _derived_preview_labels["Escape Velocity"]
		preview.text = " → %.2f km/s" % (_body.physical.get_escape_velocity_m_s() / 1000.0)
		preview.visible = true
	
	# Orbital derived values
	if _body.has_orbital():
		if _derived_preview_labels.has("Periapsis"):
			var preview: Label = _derived_preview_labels["Periapsis"]
			preview.text = " → %s" % PropertyFormatter.format_distance(_body.orbital.get_periapsis_m())
			preview.visible = true
		
		if _derived_preview_labels.has("Apoapsis"):
			var preview: Label = _derived_preview_labels["Apoapsis"]
			preview.text = " → %s" % PropertyFormatter.format_distance(_body.orbital.get_apoapsis_m())
			preview.visible = true


## Hides all derived property previews.
func _hide_derived_previews() -> void:
	for key in _derived_preview_labels.keys():
		var preview: Label = _derived_preview_labels[key]
		if preview:
			preview.visible = false


## Updates all derived value displays.
func _update_derived_displays() -> void:
	if not _body:
		return
	
	# Physical derived values
	if _derived_labels.has("Density"):
		_derived_labels["Density"].text = "%.1f kg/m³" % _body.physical.get_density_kg_m3()
	if _derived_labels.has("Surface Gravity"):
		_derived_labels["Surface Gravity"].text = "%.2f m/s²" % _body.physical.get_surface_gravity_m_s2()
	if _derived_labels.has("Escape Velocity"):
		_derived_labels["Escape Velocity"].text = "%.2f km/s" % (_body.physical.get_escape_velocity_m_s() / 1000.0)
	
	# Orbital derived values
	if _body.has_orbital():
		if _derived_labels.has("Periapsis"):
			_derived_labels["Periapsis"].text = PropertyFormatter.format_distance(_body.orbital.get_periapsis_m())
		if _derived_labels.has("Apoapsis"):
			_derived_labels["Apoapsis"].text = PropertyFormatter.format_distance(_body.orbital.get_apoapsis_m())


## Handles per-property revert button press.
## @param property_path: The property to revert.
## @param slider: The slider control.
## @param spinbox: The spinbox control.
func _on_property_revert_pressed(property_path: String, slider: HSlider, spinbox: SpinBox) -> void:
	if not _original_values.has(property_path):
		return
	
	# Restore original value
	_working_values[property_path] = _original_values[property_path]
	_confirmed_values[property_path] = _original_values[property_path]
	
	# Update controls
	var display_value: float = _get_display_value_for_property(property_path)
	slider.set_value_no_signal(display_value)
	spinbox.set_value_no_signal(display_value)
	
	# Apply and update
	_apply_values_to_body()
	_update_preview()
	_update_derived_displays()
	_update_range_indicators()


## Handles per-property revert button press for string properties.
## @param property_path: The property to revert.
## @param line_edit: The line edit control.
func _on_string_property_revert_pressed(property_path: String, line_edit: LineEdit) -> void:
	if not _original_values.has(property_path):
		return
	
	# Restore original value
	_working_values[property_path] = _original_values[property_path]
	_confirmed_values[property_path] = _original_values[property_path]
	
	# Update control
	line_edit.text = str(_original_values[property_path])
	
	# Apply (name change doesn't need preview update)
	_apply_values_to_body()


## Adds physical property editors.
func _add_physical_properties() -> void:
	var phys: PhysicalProps = _body.physical
	
	# Mass - use appropriate units based on body type
	var mass_value: float
	var mass_min: float
	var mass_max: float
	var mass_suffix: String
	
	match _body.type:
		CelestialType.Type.STAR:
			mass_value = phys.mass_kg / Units.SOLAR_MASS_KG
			mass_min = 0.08  # Brown dwarf limit
			mass_max = 300.0  # Most massive known stars
			mass_suffix = " M☉"
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			mass_value = phys.mass_kg / Units.EARTH_MASS_KG
			mass_min = 0.0001
			mass_max = 5000.0
			mass_suffix = " M⊕"
		_:
			mass_value = phys.mass_kg / 1e15
			mass_min = 0.0001
			mass_max = 1e10
			mass_suffix = " ×10¹⁵kg"
	
	_add_numeric_property("physical.mass_kg", "Mass", mass_value, mass_min, mass_max, 0.0001, mass_suffix)
	
	# Radius
	var radius_value: float
	var radius_min: float
	var radius_max: float
	var radius_suffix: String
	
	match _body.type:
		CelestialType.Type.STAR:
			radius_value = phys.radius_m / Units.SOLAR_RADIUS_METERS
			radius_min = 0.001  # White dwarfs can be tiny
			radius_max = 2000.0  # Red supergiants can be huge
			radius_suffix = " R☉"
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			radius_value = phys.radius_m / Units.EARTH_RADIUS_METERS
			radius_min = 0.01
			radius_max = 30.0  # Allow for inflated hot Jupiters
			radius_suffix = " R⊕"
		_:
			radius_value = phys.radius_m / 1000.0
			radius_min = 0.001
			radius_max = 1000.0
			radius_suffix = " km"
	
	_add_numeric_property("physical.radius_m", "Radius", radius_value, radius_min, radius_max, 0.001, radius_suffix)
	
	# Derived values
	_add_derived_property("Density", "%.1f kg/m³" % phys.get_density_kg_m3(), "Density")
	_add_derived_property("Surface Gravity", "%.2f m/s²" % phys.get_surface_gravity_m_s2(), "Surface Gravity")
	_add_derived_property("Escape Velocity", "%.2f km/s" % (phys.get_escape_velocity_m_s() / 1000.0), "Escape Velocity")
	
	# Rotation period (in hours)
	var rotation_hours: float = absf(_body.physical.rotation_period_s) / 3600.0
	_add_numeric_property("physical.rotation_period_s", "Rotation Period", rotation_hours, 0.1, 10000.0, 0.1, " hrs")
	
	# Axial tilt
	_add_numeric_property("physical.axial_tilt_deg", "Axial Tilt", _body.physical.axial_tilt_deg, 0.0, 180.0, 0.1, "°")
	
	# Oblateness
	_add_numeric_property("physical.oblateness", "Oblateness", _body.physical.oblateness, 0.0, 0.5, 0.001, "")


## Adds stellar property editors.
func _add_stellar_properties() -> void:
	var stellar: StellarProps = _body.stellar
	
	# Spectral type (e.g., G2)
	var current_type: String = _working_values.get("stellar.spectral_type", "G2")
	_add_option_property("stellar.spectral_type", "Spectral Type", current_type, SPECTRAL_CLASSES)
	
	# Luminosity class
	var current_lum: String = _working_values.get("stellar.luminosity_class", "V")
	_add_option_property("stellar.luminosity_class", "Luminosity Class", current_lum, LUMINOSITY_CLASSES)
	
	# Temperature
	_add_numeric_property("stellar.temperature_k", "Temperature", stellar.effective_temperature_k, 2000.0, 50000.0, 10.0, " K")
	
	# Luminosity (in solar luminosities)
	var lum_solar: float = stellar.luminosity_watts / 3.828e26
	_add_numeric_property("stellar.luminosity_watts", "Luminosity", lum_solar, 0.00001, 10000000.0, 0.0001, " L☉")
	
	# Age (in billions of years)
	var age_gyr: float = stellar.age_years / 1e9
	_add_numeric_property("stellar.age_years", "Age", age_gyr, 0.001, 15.0, 0.001, " Gyr")
	
	# Metallicity
	_add_numeric_property("stellar.metallicity", "Metallicity", stellar.metallicity, 0.001, 10.0, 0.001, "")


## Adds orbital property editors.
func _add_orbital_properties() -> void:
	var orbital: OrbitalProps = _body.orbital
	
	# Semi-major axis (in AU)
	var sma_au: float = orbital.semi_major_axis_m / Units.AU_METERS
	_add_numeric_property("orbital.semi_major_axis_m", "Semi-major Axis", sma_au, 0.001, 1000.0, 0.001, " AU")
	
	# Eccentricity
	_add_numeric_property("orbital.eccentricity", "Eccentricity", orbital.eccentricity, 0.0, 0.99, 0.001, "")
	
	# Inclination
	_add_numeric_property("orbital.inclination_deg", "Inclination", orbital.inclination_deg, 0.0, 180.0, 0.1, "°")
	
	# Derived values
	_add_derived_property("Periapsis", PropertyFormatter.format_distance(orbital.get_periapsis_m()), "Periapsis")
	_add_derived_property("Apoapsis", PropertyFormatter.format_distance(orbital.get_apoapsis_m()), "Apoapsis")


## Adds atmosphere property editors.
func _add_atmosphere_properties() -> void:
	var atmo: AtmosphereProps = _body.atmosphere
	
	# Surface pressure (in atm)
	var pressure_atm: float = atmo.surface_pressure_pa / 101325.0
	_add_numeric_property("atmosphere.surface_pressure_pa", "Surface Pressure", pressure_atm, 0.0, 10000.0, 0.001, " atm")
	
	# Scale height (in km)
	var scale_height_km: float = atmo.scale_height_m / 1000.0
	_add_numeric_property("atmosphere.scale_height_m", "Scale Height", scale_height_km, 1.0, 500.0, 0.1, " km")
	
	# Greenhouse factor
	_add_numeric_property("atmosphere.greenhouse_factor", "Greenhouse Factor", atmo.greenhouse_factor, 1.0, 100.0, 0.01, "×")


## Adds surface property editors.
func _add_surface_properties() -> void:
	var surface: SurfaceProps = _body.surface
	
	# Temperature
	_add_numeric_property("surface.temperature_k", "Temperature", surface.temperature_k, 0.0, 5000.0, 1.0, " K")
	
	# Albedo
	_add_numeric_property("surface.albedo", "Albedo", surface.albedo, 0.0, 1.0, 0.001, "")
	
	# Volcanism
	_add_numeric_property("surface.volcanism_level", "Volcanism Level", surface.volcanism_level, 0.0, 1.0, 0.01, "")


## Handles revert button press.
func _on_revert_pressed() -> void:
	# Restore original values
	_working_values = _original_values.duplicate(true)
	_confirmed_values = _original_values.duplicate(true)
	
	# Apply to body
	_apply_values_to_body()
	
	# Rebuild UI
	_build_editor_ui()
	
	# Update preview
	_update_preview()


## Handles confirm button press.
func _on_confirm_pressed() -> void:
	# Values are already applied to body, just emit and close
	edits_confirmed.emit(_body)
	hide()


## Handles cancel button press.
func _on_cancel_pressed() -> void:
	# Restore original values to body
	_working_values = _original_values.duplicate(true)
	_confirmed_values = _original_values.duplicate(true)
	_apply_values_to_body()
	edits_cancelled.emit()
	hide()


## Updates the 3D preview with the current body.
func _update_preview() -> void:
	if not _body or not _preview_body_renderer:
		return
	
	# Calculate display scale
	var scale_factor: float = _calculate_display_scale(_body)
	
	# Render the body
	_preview_body_renderer.render_body(_body, scale_factor)
	
	# Adjust camera distance
	var camera_distance: float = scale_factor * 3.5
	camera_distance = clampf(camera_distance, 2.0, 30.0)
	_preview_camera.transform.origin.z = camera_distance
	
	# Adjust lighting for body type
	_adjust_preview_lighting()


## Calculates display scale for the preview.
## @param body: The celestial body.
## @return: Scale factor for rendering.
func _calculate_display_scale(body: CelestialBody) -> float:
	var radius_m: float = body.physical.radius_m
	
	match body.type:
		CelestialType.Type.STAR:
			return clampf(radius_m / Units.SOLAR_RADIUS_METERS, 0.5, 3.0)
		CelestialType.Type.PLANET:
			return clampf(radius_m / Units.EARTH_RADIUS_METERS, 0.2, 2.5)
		CelestialType.Type.MOON:
			return clampf(radius_m / Units.EARTH_RADIUS_METERS * 2.0, 0.2, 2.0)
		CelestialType.Type.ASTEROID:
			var km: float = radius_m / 1000.0
			if km < 10:
				return 0.5
			elif km < 100:
				return 0.5 + (km - 10) / 90.0
			else:
				return 1.5
		_:
			return 1.0


## Adjusts preview lighting based on body type.
func _adjust_preview_lighting() -> void:
	if not _preview_light:
		return
	
	match _body.type:
		CelestialType.Type.STAR:
			_preview_light.light_energy = 0.1
		CelestialType.Type.ASTEROID:
			_preview_light.light_energy = 1.2
		_:
			_preview_light.light_energy = 0.8


## Formats the type string for display.
## @param body: The celestial body.
## @return: Formatted type string.
func _format_type(body: CelestialBody) -> String:
	var type_str: String = body.get_type_string()
	
	if body.type == CelestialType.Type.STAR and body.has_stellar():
		type_str += " (%s)" % body.stellar.spectral_class
	
	return type_str


## Updates the type display (for stars, includes spectral class).
func _update_type_display() -> void:
	if _derived_labels.has("Type"):
		_derived_labels["Type"].text = _format_type(_body)


## Adds a derived (read-only) property display.
## @param label_text: The property label.
## @param value_text: The property value.
## @param track_key: Optional key for tracking updates.
func _add_derived_property(label_text: String, value_text: String, track_key: String = "") -> void:
	if not _current_section_content:
		return
	
	var container: HBoxContainer = HBoxContainer.new()
	container.add_theme_constant_override("separation", 10)
	
	var label: Label = Label.new()
	label.text = label_text + ":"
	label.custom_minimum_size.x = LABEL_MIN_WIDTH
	label.add_theme_color_override("font_color", LABEL_COLOR)
	label.add_theme_font_size_override("font_size", 12)
	container.add_child(label)
	
	# Current value label
	var value: Label = Label.new()
	value.text = value_text
	value.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	value.add_theme_color_override("font_color", DERIVED_COLOR)
	value.add_theme_font_size_override("font_size", 12)
	container.add_child(value)
	
	# Preview value label (shown during drag)
	var preview: Label = Label.new()
	preview.text = ""
	preview.add_theme_color_override("font_color", DERIVED_PREVIEW_COLOR)
	preview.add_theme_font_size_override("font_size", 12)
	preview.visible = false
	container.add_child(preview)
	
	# Add subtle indicator that this is derived
	var indicator: Label = Label.new()
	indicator.text = "(derived)"
	indicator.add_theme_color_override("font_color", Color(0.4, 0.4, 0.5))
	indicator.add_theme_font_size_override("font_size", 10)
	container.add_child(indicator)
	
	_current_section_content.add_child(container)
	
	# Store reference for updating
	var key: String = track_key if track_key != "" else label_text
	_derived_labels[key] = value
	_derived_preview_labels[key] = preview
