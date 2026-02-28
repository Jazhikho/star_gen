## Dialog for editing celestial body properties.
## Uses PropertyConstraintSolver to keep slider bounds physically consistent.
## Locking a property re-runs the solver and narrows coupled properties.
## The Traveller panel adds an optional UWP size code constraint layer.
##
## Spectral class / luminosity class option editors are deferred to a later slice.
class_name EditDialog
extends Window

const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _body_renderer: GDScript = preload("res://src/app/rendering/BodyRenderer.gd")
const _property_formatter: GDScript = preload("res://src/app/viewer/PropertyFormatter.gd")
const _solver: GDScript = preload("res://src/domain/editing/PropertyConstraintSolver.gd")
const _constraint_set: GDScript = preload("res://src/domain/editing/ConstraintSet.gd")
const _traveller_builder: GDScript = preload("res://src/domain/editing/TravellerConstraintBuilder.gd")
const _edit_regenerator: GDScript = preload("res://src/domain/editing/EditRegenerator.gd")
const _save_data: GDScript = preload("res://src/services/persistence/SaveData.gd")

## Emitted when the user confirms edits.
signal edits_confirmed(body: CelestialBody)

## Emitted when the user cancels edits.
signal edits_cancelled()

## Emitted after a successful constrained regeneration so the parent viewer can swap its displayed body.
signal body_regenerated(new_body: CelestialBody)

## The celestial body being edited.
var _body: CelestialBody = null

## Snapshot of original values in BASE SI UNITS (for revert).
var _original_values: Dictionary = {}

## Working values in BASE SI UNITS. Uses stellar.temperature_k to match solver/override key.
var _working_values: Dictionary = {}

## Set of property paths the user has locked.
var _locked_paths: Array[String] = []

## Currently selected Traveller size code, or null if not applied.
var _traveller_code: Variant = null

## Current solved constraint set (rebuilt on lock/value/traveller changes).
var _constraints: ConstraintSet = null

## Map of property_path -> editor Dictionary { slider, spinbox, lock_btn, range_label }.
var _property_editors: Dictionary = {}

## Map of derived-label key -> Label for live updates.
var _derived_labels: Dictionary = {}

## Traveller UI controls (null for non-planet/moon bodies).
var _traveller_option: OptionButton = null
var _traveller_apply: Button = null
var _traveller_clear: Button = null

## ParentContext to use when regenerating. Set by the caller (ObjectViewer) if the body was generated in a known context; null = use type default.
var regeneration_context: ParentContext = null

## Save file dialog (created lazily).
var _save_dialog: FileDialog = null

## Button references for enable/disable.
var _regenerate_btn: Button = null
var _save_btn: Button = null

@onready var _content: VBoxContainer = $MarginContainer/VBoxContainer/ContentSplit/ScrollContainer/ContentMargin/ContentContainer
@onready var _preview_viewport: SubViewport = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport
@onready var _preview_camera: Camera3D = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/Camera3D
@onready var _preview_body_renderer: BodyRenderer = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/BodyRenderer
@onready var _preview_light: DirectionalLight3D = $MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/PreviewEnvironment/DirectionalLight3D
@onready var _revert_button: Button = $MarginContainer/VBoxContainer/ButtonContainer/RevertButton
@onready var _confirm_button: Button = $MarginContainer/VBoxContainer/ButtonContainer/ConfirmButton
@onready var _cancel_button: Button = $MarginContainer/VBoxContainer/ButtonContainer/CancelButton

## Current section content container for adding rows.
var _current_section_content: VBoxContainer = null

## Colour constants.
const SECTION_COLOR: Color = Color(0.9, 0.9, 0.9)
const LABEL_COLOR: Color = Color(0.6, 0.6, 0.6)
const DERIVED_COLOR: Color = Color(0.5, 0.6, 0.7)
const WARNING_COLOR: Color = Color(0.9, 0.6, 0.2)
const LOCKED_COLOR: Color = Color(0.9, 0.7, 0.3)
const LABEL_MIN_WIDTH: float = 120.0
const DEFAULT_DISPLAY_STEP: float = 0.001

## Display-unit conversion: base * factor = display. 0.0 = type-dependent.
const _DISPLAY_FACTORS: Dictionary = {
	"physical.mass_kg": 0.0,
	"physical.radius_m": 0.0,
	"physical.rotation_period_s": 1.0 / 3600.0,
	"physical.axial_tilt_deg": 1.0,
	"physical.oblateness": 1.0,
	"stellar.temperature_k": 1.0,
	"stellar.luminosity_watts": 1.0 / 3.828e26,
	"stellar.age_years": 1.0e-9,
	"stellar.metallicity": 1.0,
	"orbital.semi_major_axis_m": 1.0 / 1.496e11,
	"orbital.eccentricity": 1.0,
	"orbital.inclination_deg": 1.0,
	"atmosphere.surface_pressure_pa": 1.0 / 101325.0,
	"atmosphere.scale_height_m": 1.0e-3,
	"atmosphere.greenhouse_factor": 1.0,
	"surface.temperature_k": 1.0,
	"surface.albedo": 1.0,
	"surface.volcanism_level": 1.0,
}

const _SUFFIXES: Dictionary = {
	"physical.rotation_period_s": " hrs",
	"physical.axial_tilt_deg": "\u00B0",
	"physical.oblateness": "",
	"stellar.temperature_k": " K",
	"stellar.luminosity_watts": " L\u2609",
	"stellar.age_years": " Gyr",
	"stellar.metallicity": "",
	"orbital.semi_major_axis_m": " AU",
	"orbital.eccentricity": "",
	"orbital.inclination_deg": "\u00B0",
	"atmosphere.surface_pressure_pa": " atm",
	"atmosphere.scale_height_m": " km",
	"atmosphere.greenhouse_factor": "x",
	"surface.temperature_k": " K",
	"surface.albedo": "",
	"surface.volcanism_level": "",
}

const _STEPS: Dictionary = {
	"physical.mass_kg": 0.0001,
	"physical.radius_m": 0.001,
	"physical.rotation_period_s": 0.1,
	"physical.axial_tilt_deg": 0.1,
	"physical.oblateness": 0.001,
	"stellar.temperature_k": 10.0,
	"stellar.luminosity_watts": 0.0001,
	"stellar.age_years": 0.001,
	"stellar.metallicity": 0.001,
	"orbital.semi_major_axis_m": 0.001,
	"orbital.eccentricity": 0.001,
	"orbital.inclination_deg": 0.1,
	"atmosphere.surface_pressure_pa": 0.001,
	"atmosphere.scale_height_m": 0.1,
	"atmosphere.greenhouse_factor": 0.01,
	"surface.temperature_k": 1.0,
	"surface.albedo": 0.001,
	"surface.volcanism_level": 0.01,
}


func _ready() -> void:
	close_requested.connect(_on_cancel_pressed)
	_revert_button.pressed.connect(_on_revert_pressed)
	_confirm_button.pressed.connect(_on_confirm_pressed)
	_cancel_button.pressed.connect(_on_cancel_pressed)
	_setup_extra_buttons()


## Opens the dialog for editing the given body.
## @param body: The celestial body to edit.
func open_for_body(body: CelestialBody) -> void:
	if not body:
		return
	_body = body
	title = "Edit: %s" % (body.name if body.name else body.id)
	_locked_paths.clear()
	_traveller_code = null
	_extract_values_from_body()
	_original_values = _working_values.duplicate(true)
	_resolve()
	_build_editor_ui()
	_update_preview()
	popup_centered()


## Re-runs the solver with current values, locks, and Traveller constraints.
func _resolve() -> void:
	if not _body:
		return
	var extra: Dictionary = {}
	if _traveller_code != null:
		extra = TravellerConstraintBuilder.build_constraints_for_size(_traveller_code)
	_constraints = PropertyConstraintSolver.solve_with_extra_constraints(
		_body.type, _working_values, _locked_paths, extra
	)


## Pulls every editable numeric property from the body into _working_values (base SI).
## Uses stellar.temperature_k as key to match solver and BaseSpec overrides.
func _extract_values_from_body() -> void:
	_working_values.clear()
	if not _body:
		return
	_working_values["name"] = _body.name if _body.name else _body.id
	var p: PhysicalProps = _body.physical
	_working_values["physical.mass_kg"] = p.mass_kg
	_working_values["physical.radius_m"] = p.radius_m
	_working_values["physical.rotation_period_s"] = absf(p.rotation_period_s)
	_working_values["physical.axial_tilt_deg"] = p.axial_tilt_deg
	_working_values["physical.oblateness"] = p.oblateness
	if _body.has_stellar():
		var s: StellarProps = _body.stellar
		_working_values["stellar.temperature_k"] = s.effective_temperature_k
		_working_values["stellar.luminosity_watts"] = s.luminosity_watts
		_working_values["stellar.age_years"] = s.age_years
		_working_values["stellar.metallicity"] = s.metallicity
	if _body.has_orbital():
		var o: OrbitalProps = _body.orbital
		_working_values["orbital.semi_major_axis_m"] = o.semi_major_axis_m
		_working_values["orbital.eccentricity"] = o.eccentricity
		_working_values["orbital.inclination_deg"] = o.inclination_deg
	if _body.has_atmosphere():
		var a: AtmosphereProps = _body.atmosphere
		_working_values["atmosphere.surface_pressure_pa"] = a.surface_pressure_pa
		_working_values["atmosphere.scale_height_m"] = a.scale_height_m
		_working_values["atmosphere.greenhouse_factor"] = a.greenhouse_factor
	if _body.has_surface():
		var sf: SurfaceProps = _body.surface
		_working_values["surface.temperature_k"] = sf.temperature_k
		_working_values["surface.albedo"] = sf.albedo
		_working_values["surface.volcanism_level"] = sf.volcanism_level


## Pushes _working_values back onto the body for preview.
func _apply_values_to_body() -> void:
	if not _body:
		return
	_body.name = _working_values.get("name", _body.name) as String
	var p: PhysicalProps = _body.physical
	p.mass_kg = _wv("physical.mass_kg", p.mass_kg)
	p.radius_m = _wv("physical.radius_m", p.radius_m)
	var retro: float = 1.0
	if p.rotation_period_s < 0.0:
		retro = -1.0
	p.rotation_period_s = _wv("physical.rotation_period_s", absf(p.rotation_period_s)) * retro
	p.axial_tilt_deg = _wv("physical.axial_tilt_deg", p.axial_tilt_deg)
	p.oblateness = _wv("physical.oblateness", p.oblateness)
	if _body.has_stellar():
		var s: StellarProps = _body.stellar
		s.effective_temperature_k = _wv("stellar.temperature_k", s.effective_temperature_k)
		s.luminosity_watts = _wv("stellar.luminosity_watts", s.luminosity_watts)
		s.age_years = _wv("stellar.age_years", s.age_years)
		s.metallicity = _wv("stellar.metallicity", s.metallicity)
	if _body.has_orbital():
		var o: OrbitalProps = _body.orbital
		o.semi_major_axis_m = _wv("orbital.semi_major_axis_m", o.semi_major_axis_m)
		o.eccentricity = _wv("orbital.eccentricity", o.eccentricity)
		o.inclination_deg = _wv("orbital.inclination_deg", o.inclination_deg)
	if _body.has_atmosphere():
		var a: AtmosphereProps = _body.atmosphere
		a.surface_pressure_pa = _wv("atmosphere.surface_pressure_pa", a.surface_pressure_pa)
		a.scale_height_m = _wv("atmosphere.scale_height_m", a.scale_height_m)
		a.greenhouse_factor = _wv("atmosphere.greenhouse_factor", a.greenhouse_factor)
	if _body.has_surface():
		var sf: SurfaceProps = _body.surface
		sf.temperature_k = _wv("surface.temperature_k", sf.temperature_k)
		sf.albedo = _wv("surface.albedo", sf.albedo)
		sf.volcanism_level = _wv("surface.volcanism_level", sf.volcanism_level)


func _wv(path: String, fallback: float) -> float:
	if _working_values.has(path):
		return _working_values[path] as float
	return fallback


func _clear_content() -> void:
	_property_editors.clear()
	_derived_labels.clear()
	if not _content:
		return
	for child: Node in _content.get_children():
		child.queue_free()
	_current_section_content = null
	_traveller_option = null
	_traveller_apply = null
	_traveller_clear = null


func _build_editor_ui() -> void:
	_clear_content()
	if not _body:
		return
	_add_section("Basic Info")
	_add_name_editor()
	_add_derived_row("Type", _body.get_type_string())
	_add_derived_row("ID", _body.id)
	if _body.type == CelestialType.Type.PLANET or _body.type == CelestialType.Type.MOON:
		_add_section("Traveller UWP")
		_add_traveller_panel()
	_add_section("Physical Properties")
	_add_numeric_editor("physical.mass_kg", "Mass")
	_add_numeric_editor("physical.radius_m", "Radius")
	_add_derived_row("Density", _fmt_density(), "Density")
	_add_derived_row("Surface Gravity", _fmt_gravity(), "SurfaceGravity")
	_add_derived_row("Escape Velocity", _fmt_escape(), "EscapeVel")
	_add_numeric_editor("physical.rotation_period_s", "Rotation Period")
	_add_numeric_editor("physical.axial_tilt_deg", "Axial Tilt")
	_add_numeric_editor("physical.oblateness", "Oblateness")
	if _body.has_stellar():
		_add_section("Stellar Properties")
		_add_numeric_editor("stellar.temperature_k", "Temperature")
		_add_numeric_editor("stellar.luminosity_watts", "Luminosity")
		_add_numeric_editor("stellar.age_years", "Age")
		_add_numeric_editor("stellar.metallicity", "Metallicity")
	if _body.has_orbital():
		_add_section("Orbital Properties")
		_add_numeric_editor("orbital.semi_major_axis_m", "Semi-major Axis")
		_add_numeric_editor("orbital.eccentricity", "Eccentricity")
		_add_numeric_editor("orbital.inclination_deg", "Inclination")
		_add_derived_row("Periapsis", _fmt_periapsis(), "Periapsis")
		_add_derived_row("Apoapsis", _fmt_apoapsis(), "Apoapsis")
	if _body.has_atmosphere():
		_add_section("Atmosphere")
		_add_numeric_editor("atmosphere.surface_pressure_pa", "Surface Pressure")
		_add_numeric_editor("atmosphere.scale_height_m", "Scale Height")
		_add_numeric_editor("atmosphere.greenhouse_factor", "Greenhouse Factor")
	if _body.has_surface():
		_add_section("Surface")
		_add_numeric_editor("surface.temperature_k", "Temperature")
		_add_numeric_editor("surface.albedo", "Albedo")
		_add_numeric_editor("surface.volcanism_level", "Volcanism Level")


func _add_section(title_text: String) -> void:
	var section: VBoxContainer = VBoxContainer.new()
	section.add_theme_constant_override("separation", 5)
	var header: Label = Label.new()
	header.text = title_text
	header.add_theme_font_size_override("font_size", 16)
	header.add_theme_color_override("font_color", SECTION_COLOR)
	section.add_child(header)
	section.add_child(HSeparator.new())
	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 12)
	section.add_child(content)
	_content.add_child(section)
	_current_section_content = content


func _add_name_editor() -> void:
	if not _current_section_content:
		return
	var row: HBoxContainer = HBoxContainer.new()
	var lbl: Label = Label.new()
	lbl.text = "Name:"
	lbl.custom_minimum_size.x = LABEL_MIN_WIDTH
	lbl.add_theme_color_override("font_color", LABEL_COLOR)
	row.add_child(lbl)
	var edit: LineEdit = LineEdit.new()
	edit.text = _working_values.get("name", "") as String
	edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	edit.text_changed.connect(func(t: String) -> void:
		_working_values["name"] = t
		_apply_values_to_body()
	)
	row.add_child(edit)
	_current_section_content.add_child(row)


func _add_numeric_editor(property_path: String, label_text: String) -> void:
	if not _current_section_content or not _constraints:
		return
	if not _constraints.has_constraint(property_path):
		return
	var container: VBoxContainer = VBoxContainer.new()
	container.add_theme_constant_override("separation", 4)
	var lbl: Label = Label.new()
	lbl.text = label_text
	lbl.add_theme_color_override("font_color", LABEL_COLOR)
	lbl.add_theme_font_size_override("font_size", 13)
	container.add_child(lbl)
	var controls: HBoxContainer = HBoxContainer.new()
	controls.add_theme_constant_override("separation", 8)
	var lock_btn: CheckButton = CheckButton.new()
	lock_btn.text = "Lock"
	lock_btn.tooltip_text = "Lock this property (constrains dependent properties)"
	lock_btn.custom_minimum_size.x = 50
	lock_btn.button_pressed = _locked_paths.has(property_path)
	lock_btn.toggled.connect(_on_lock_toggled.bind(property_path))
	controls.add_child(lock_btn)
	var range_base: Vector2 = _constraints.get_range(property_path)
	var factor: float = _display_factor_for(property_path)
	var disp_min: float = range_base.x * factor
	var disp_max: float = range_base.y * factor
	var disp_val: float = _wv(property_path, 0.0) * factor
	var step: float = _STEPS.get(property_path, DEFAULT_DISPLAY_STEP) as float
	var suffix: String = _suffix_for(property_path)
	var slider: HSlider = HSlider.new()
	slider.min_value = disp_min
	slider.max_value = disp_max
	slider.step = step
	slider.value = disp_val
	slider.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	slider.custom_minimum_size.x = 100
	controls.add_child(slider)
	var spin: SpinBox = SpinBox.new()
	spin.min_value = disp_min
	spin.max_value = disp_max
	spin.step = step
	spin.value = disp_val
	spin.suffix = suffix
	spin.custom_minimum_size.x = 140
	spin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	controls.add_child(spin)
	container.add_child(controls)
	var range_label: Label = Label.new()
	range_label.add_theme_font_size_override("font_size", 10)
	range_label.add_theme_color_override("font_color", Color(0.5, 0.5, 0.5))
	range_label.text = _fmt_range_label(property_path, range_base)
	container.add_child(range_label)
	_current_section_content.add_child(container)
	slider.value_changed.connect(_on_slider_changed.bind(property_path, spin))
	spin.value_changed.connect(_on_spin_changed.bind(property_path, slider))
	_property_editors[property_path] = {
		"slider": slider,
		"spinbox": spin,
		"lock_btn": lock_btn,
		"range_label": range_label,
	}


func _add_derived_row(label_text: String, value_text: String, track_key: String = "") -> void:
	if not _current_section_content:
		return
	var row: HBoxContainer = HBoxContainer.new()
	var lbl: Label = Label.new()
	lbl.text = label_text + ":"
	lbl.custom_minimum_size.x = LABEL_MIN_WIDTH
	lbl.add_theme_color_override("font_color", LABEL_COLOR)
	lbl.add_theme_font_size_override("font_size", 12)
	row.add_child(lbl)
	var val: Label = Label.new()
	val.text = value_text
	val.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	val.add_theme_color_override("font_color", DERIVED_COLOR)
	val.add_theme_font_size_override("font_size", 12)
	row.add_child(val)
	var tag: Label = Label.new()
	tag.text = "(derived)"
	tag.add_theme_font_size_override("font_size", 10)
	tag.add_theme_color_override("font_color", Color(0.4, 0.4, 0.5))
	row.add_child(tag)
	_current_section_content.add_child(row)
	if not track_key.is_empty():
		_derived_labels[track_key] = val


func _add_traveller_panel() -> void:
	if not _current_section_content:
		return
	var row: HBoxContainer = HBoxContainer.new()
	row.add_theme_constant_override("separation", 8)
	var lbl: Label = Label.new()
	lbl.text = "Size Code:"
	lbl.custom_minimum_size.x = LABEL_MIN_WIDTH
	lbl.add_theme_color_override("font_color", LABEL_COLOR)
	row.add_child(lbl)
	_traveller_option = OptionButton.new()
	_traveller_option.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	var codes: Array = TravellerConstraintBuilder.all_codes()
	var current_code: Variant = TravellerConstraintBuilder.code_for_radius(_wv("physical.radius_m", 6.371e6))
	var sel_idx: int = 0
	for i: int in range(codes.size()):
		var code: Variant = codes[i]
		_traveller_option.add_item(TravellerConstraintBuilder.describe_code(code), i)
		_traveller_option.set_item_metadata(i, code)
		if str(code) == str(current_code):
			sel_idx = i
	_traveller_option.selected = sel_idx
	row.add_child(_traveller_option)
	_traveller_apply = Button.new()
	_traveller_apply.text = "Apply"
	_traveller_apply.tooltip_text = "Constrain radius and mass to this Traveller size code"
	_traveller_apply.pressed.connect(_on_traveller_apply)
	row.add_child(_traveller_apply)
	_traveller_clear = Button.new()
	_traveller_clear.text = "Clear"
	_traveller_clear.tooltip_text = "Remove Traveller constraint"
	_traveller_clear.disabled = _traveller_code == null
	_traveller_clear.pressed.connect(_on_traveller_clear)
	row.add_child(_traveller_clear)
	_current_section_content.add_child(row)
	var status: Label = Label.new()
	status.add_theme_font_size_override("font_size", 10)
	status.add_theme_color_override("font_color", Color(0.5, 0.5, 0.5))
	status.text = _traveller_status_text()
	_current_section_content.add_child(status)
	_derived_labels["TravellerStatus"] = status


func _on_lock_toggled(pressed: bool, property_path: String) -> void:
	if pressed:
		if not _locked_paths.has(property_path):
			_locked_paths.append(property_path)
	else:
		_locked_paths.erase(property_path)
	_resolve()
	var clamped: Array[String] = _constraints.clamp_unlocked()
	for path: String in clamped:
		_working_values[path] = _constraints.get_constraint(path).current_value
	_refresh_all_editor_bounds()
	_apply_values_to_body()
	_update_preview()
	_update_derived_labels()


func _on_slider_changed(disp_value: float, property_path: String, spin: SpinBox) -> void:
	spin.set_value_no_signal(disp_value)
	_commit_display_value(property_path, disp_value)


func _on_spin_changed(disp_value: float, property_path: String, slider: HSlider) -> void:
	slider.set_value_no_signal(disp_value)
	_commit_display_value(property_path, disp_value)


func _commit_display_value(property_path: String, disp_value: float) -> void:
	var factor: float = _display_factor_for(property_path)
	_working_values[property_path] = disp_value / factor
	_resolve()
	_refresh_all_editor_bounds()
	_apply_values_to_body()
	_update_preview()
	_update_derived_labels()


func _on_traveller_apply() -> void:
	if not _traveller_option:
		return
	var idx: int = _traveller_option.selected
	if idx < 0:
		return
	_traveller_code = _traveller_option.get_item_metadata(idx)
	_resolve()
	var clamped: Array[String] = _constraints.clamp_unlocked()
	for path: String in clamped:
		_working_values[path] = _constraints.get_constraint(path).current_value
	_refresh_all_editor_bounds()
	_apply_values_to_body()
	_update_preview()
	_update_derived_labels()
	if _traveller_clear:
		_traveller_clear.disabled = false


func _on_traveller_clear() -> void:
	_traveller_code = null
	_resolve()
	_refresh_all_editor_bounds()
	_update_derived_labels()
	if _traveller_clear:
		_traveller_clear.disabled = true


func _refresh_all_editor_bounds() -> void:
	for path: Variant in _property_editors.keys():
		var path_str: String = path as String
		var ed: Dictionary = _property_editors[path] as Dictionary
		var c: PropertyConstraint = _constraints.get_constraint(path_str)
		if c == null:
			continue
		var factor: float = _display_factor_for(path_str)
		var slider: HSlider = ed["slider"] as HSlider
		var spin: SpinBox = ed["spinbox"] as SpinBox
		var range_label: Label = ed["range_label"] as Label
		var lock_btn: CheckButton = ed["lock_btn"] as CheckButton
		var disp_min: float = c.min_value * factor
		var disp_max: float = c.max_value * factor
		var disp_val: float = _wv(path_str, c.current_value) * factor
		slider.min_value = disp_min
		slider.max_value = disp_max
		slider.set_value_no_signal(disp_val)
		spin.min_value = disp_min
		spin.max_value = disp_max
		spin.set_value_no_signal(disp_val)
		var locked: bool = c.is_locked
		slider.editable = not locked
		spin.editable = not locked
		lock_btn.set_pressed_no_signal(locked)
		if locked:
			lock_btn.add_theme_color_override("font_color", LOCKED_COLOR)
		else:
			lock_btn.remove_theme_color_override("font_color")
		range_label.text = _fmt_range_label(path_str, Vector2(c.min_value, c.max_value))
		if c.is_value_in_range():
			range_label.add_theme_color_override("font_color", Color(0.5, 0.5, 0.5))
		else:
			range_label.add_theme_color_override("font_color", WARNING_COLOR)


func _update_derived_labels() -> void:
	if not _body:
		return
	if _derived_labels.has("Density"):
		_derived_labels["Density"].text = _fmt_density()
	if _derived_labels.has("SurfaceGravity"):
		_derived_labels["SurfaceGravity"].text = _fmt_gravity()
	if _derived_labels.has("EscapeVel"):
		_derived_labels["EscapeVel"].text = _fmt_escape()
	if _body.has_orbital():
		if _derived_labels.has("Periapsis"):
			_derived_labels["Periapsis"].text = _fmt_periapsis()
		if _derived_labels.has("Apoapsis"):
			_derived_labels["Apoapsis"].text = _fmt_apoapsis()
	if _derived_labels.has("TravellerStatus"):
		_derived_labels["TravellerStatus"].text = _traveller_status_text()


func _update_preview() -> void:
	if not _body or not _preview_body_renderer:
		return
	var scale_factor: float = _calculate_display_scale()
	_preview_body_renderer.render_body(_body, scale_factor)
	var cam_dist: float = clampf(scale_factor * 3.5, 2.0, 30.0)
	_preview_camera.transform.origin.z = cam_dist
	_adjust_preview_lighting()


func _calculate_display_scale() -> float:
	var r: float = _body.physical.radius_m
	match _body.type:
		CelestialType.Type.STAR:
			return clampf(r / Units.SOLAR_RADIUS_METERS, 0.5, 3.0)
		CelestialType.Type.PLANET:
			return clampf(r / Units.EARTH_RADIUS_METERS, 0.2, 2.5)
		CelestialType.Type.MOON:
			return clampf(r / Units.EARTH_RADIUS_METERS * 2.0, 0.2, 2.0)
		CelestialType.Type.ASTEROID:
			var km: float = r / 1000.0
			return clampf(0.5 + km / 100.0, 0.5, 1.5)
		_:
			return 1.0


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


func _on_revert_pressed() -> void:
	_working_values = _original_values.duplicate(true)
	_locked_paths.clear()
	_traveller_code = null
	_apply_values_to_body()
	_resolve()
	_build_editor_ui()
	_update_preview()


func _on_confirm_pressed() -> void:
	edits_confirmed.emit(_body)
	hide()


func _on_cancel_pressed() -> void:
	_working_values = _original_values.duplicate(true)
	_apply_values_to_body()
	edits_cancelled.emit()
	hide()


## Creates and wires the Regenerate and Save buttons. Inserted before Revert so confirm/cancel stay rightmost.
func _setup_extra_buttons() -> void:
	var btn_box: HBoxContainer = $MarginContainer/VBoxContainer/ButtonContainer
	if not btn_box:
		return
	_regenerate_btn = Button.new()
	_regenerate_btn.text = "Regenerate Unlocked"
	_regenerate_btn.tooltip_text = "Re-roll unlocked properties; locked ones stay fixed"
	_regenerate_btn.custom_minimum_size = Vector2(160, 35)
	_regenerate_btn.pressed.connect(_on_regenerate_pressed)
	btn_box.add_child(_regenerate_btn)
	btn_box.move_child(_regenerate_btn, 0)
	_save_btn = Button.new()
	_save_btn.text = "Save As\u2026"
	_save_btn.tooltip_text = "Save this edited body to a file"
	_save_btn.custom_minimum_size = Vector2(100, 35)
	_save_btn.pressed.connect(_on_save_pressed)
	btn_box.add_child(_save_btn)
	btn_box.move_child(_save_btn, 1)


## Regenerate handler. Locked properties become spec overrides; everything else is re-rolled with a fresh seed.
func _on_regenerate_pressed() -> void:
	if not _body or not _constraints:
		return
	for path: String in _locked_paths:
		if _working_values.has(path):
			_constraints.set_value(path, _working_values[path] as float)
	var seed_val: int = randi()
	var result: Variant = _edit_regenerator.regenerate(
		_body.type, _constraints, seed_val, regeneration_context
	)
	if not result.success:
		title = "Regeneration failed: %s" % result.error_message
		return
	var preserved_name: String = _working_values.get("name", "") as String
	if not preserved_name.is_empty():
		result.body.name = preserved_name
	var keep_locks: Array[String] = _locked_paths.duplicate()
	var keep_traveller: Variant = _traveller_code
	_body = result.body
	_extract_values_from_body()
	_original_values = _working_values.duplicate(true)
	_locked_paths = keep_locks
	_traveller_code = keep_traveller
	_resolve()
	_build_editor_ui()
	_update_preview()
	title = "Edit: %s (regenerated)" % (_body.name if _body.name else _body.id)
	body_regenerated.emit(_body)


## Save handler. Opens a file dialog; on confirm writes via SaveData.save_edited_body.
func _on_save_pressed() -> void:
	if not _body:
		return
	if _save_dialog == null:
		_save_dialog = FileDialog.new()
		_save_dialog.title = "Save Edited Body"
		_save_dialog.file_mode = FileDialog.FILE_MODE_SAVE_FILE
		_save_dialog.access = FileDialog.ACCESS_FILESYSTEM
		_save_dialog.filters = PackedStringArray([
			"*.sgb ; StarGen Binary",
			"*.json ; JSON",
		])
		_save_dialog.current_dir = OS.get_user_data_dir()
		_save_dialog.file_selected.connect(_on_save_path_selected)
		add_child(_save_dialog)
	var default_name: String = (_body.name if _body.name else "edited_body").replace(" ", "_").to_lower()
	_save_dialog.current_file = default_name + ".sgb"
	_save_dialog.popup_centered(Vector2i(600, 400))


## Writes the body when the user picks a save path.
func _on_save_path_selected(path: String) -> void:
	_apply_values_to_body()
	var compress: bool = path.ends_with(".sgb")
	var err: Error = _save_data.save_edited_body(_body, path, compress)
	if err != OK:
		title = "Save failed: %s" % error_string(err)
	else:
		title = "Saved: %s" % path.get_file()


func _display_factor_for(property_path: String) -> float:
	var f: float = _DISPLAY_FACTORS.get(property_path, 1.0) as float
	if f != 0.0:
		return f
	match property_path:
		"physical.mass_kg":
			match _body.type:
				CelestialType.Type.STAR:
					return 1.0 / Units.SOLAR_MASS_KG
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					return 1.0 / Units.EARTH_MASS_KG
				_:
					return 1.0e-15
		"physical.radius_m":
			match _body.type:
				CelestialType.Type.STAR:
					return 1.0 / Units.SOLAR_RADIUS_METERS
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					return 1.0 / Units.EARTH_RADIUS_METERS
				_:
					return 1.0e-3
	return 1.0


func _suffix_for(property_path: String) -> String:
	if _SUFFIXES.has(property_path):
		return _SUFFIXES[property_path] as String
	match property_path:
		"physical.mass_kg":
			match _body.type:
				CelestialType.Type.STAR:
					return " M\u2609"
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					return " M\u2295"
				_:
					return " x10^15 kg"
		"physical.radius_m":
			match _body.type:
				CelestialType.Type.STAR:
					return " R\u2609"
				CelestialType.Type.PLANET, CelestialType.Type.MOON:
					return " R\u2295"
				_:
					return " km"
	return ""


## Formats range label; uses str() to avoid unsupported %g format in GDScript.
func _fmt_range_label(property_path: String, base_range: Vector2) -> String:
	var factor: float = _display_factor_for(property_path)
	var suffix: String = _suffix_for(property_path)
	var c: PropertyConstraint = _constraints.get_constraint(property_path)
	var reason: String = ""
	if c != null and not c.constraint_reason.is_empty():
		reason = " [" + c.constraint_reason + "]"
	return "Valid: %s - %s%s%s" % [
		str(base_range.x * factor),
		str(base_range.y * factor),
		suffix,
		reason,
	]


func _traveller_status_text() -> String:
	if _traveller_code == null:
		return "No Traveller constraint active."
	var uwp: String = TravellerSizeCode.to_string_uwp(_traveller_code)
	return "Active: size code %s (locks radius and mass windows)" % uwp


func _fmt_density() -> String:
	return "%.1f kg/m^3" % _body.physical.get_density_kg_m3()

func _fmt_gravity() -> String:
	return "%.2f m/s^2" % _body.physical.get_surface_gravity_m_s2()

func _fmt_escape() -> String:
	return "%.2f km/s" % (_body.physical.get_escape_velocity_m_s() / 1000.0)

func _fmt_periapsis() -> String:
	return PropertyFormatter.format_distance(_body.orbital.get_periapsis_m())

func _fmt_apoapsis() -> String:
	return PropertyFormatter.format_distance(_body.orbital.get_apoapsis_m())
