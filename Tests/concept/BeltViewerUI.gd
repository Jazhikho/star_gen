## Builds and manages the control panel UI for the belt viewer concept scene.
## Emits regenerate_requested when the user changes parameters.
## Exposes getters for major body count, label visibility, and gap ring visibility.
class_name BeltViewerUIConcept
extends Control


## Emitted when the user presses Generate.
signal regenerate_requested(spec: AsteroidBeltSpecConcept, seed_val: int)

## Emitted when the user presses Reset Camera.
signal reset_view_requested


## Fractional positions within the belt where resonance gaps are placed.
## Derived from real Kirkwood gaps mapped to the 2.06–3.27 AU main belt:
## 3:1 at ~0.30, 5:2 at ~0.55, 7:3 at ~0.70, 2:1 at ~0.85.
const GAP_FRACTIONS: Array[float] = [0.30, 0.55, 0.70, 0.85]

## Gap half-widths as fractions of belt width, proportional to real gap sizes.
const GAP_WIDTH_FRACTIONS: Array[float] = [0.04, 0.03, 0.025, 0.035]


var _seed_spin: SpinBox = null
var _count_spin: SpinBox = null
var _inner_spin: SpinBox = null
var _outer_spin: SpinBox = null
var _incl_spin: SpinBox = null
var _ecc_spin: SpinBox = null
var _conc_spin: SpinBox = null
var _gap_check: CheckBox = null
var _gap_rings_check: CheckBox = null
var _cluster_check: CheckBox = null
var _major_count_spin: SpinBox = null
var _label_check: CheckBox = null
var _status_label: Label = null


## Builds the full UI panel and connects internal signals.
## @param parent: The node to add the panel to.
func setup(parent: Node) -> void:
	parent.add_child(self )
	_build_panel()


## Returns the desired number of test major asteroids.
## @return: Major body count from UI.
func get_major_count() -> int:
	return int(_major_count_spin.value)


## Returns whether labels should be shown on major asteroids.
## @return: True if label checkbox is checked.
func get_show_labels() -> bool:
	return _label_check.button_pressed


## Returns whether gap boundary rings should be drawn.
## @return: True if gap ring checkbox is checked.
func get_show_gap_rings() -> bool:
	return _gap_rings_check.button_pressed


## Updates the status label after generation.
## @param belt: The just-generated belt.
## @param elapsed_ms: Generation time.
func update_status(belt: AsteroidBeltDataConcept, elapsed_ms: float) -> void:
	_status_label.text = "%d bg + %d major\nSeed: %d\n%.1fms" % [
		belt.get_background_count(),
		belt.get_major_count(),
		belt.generation_seed,
		elapsed_ms
	]


## Constructs the panel layout with all controls.
func _build_panel() -> void:
	var panel: PanelContainer = PanelContainer.new()
	var style: StyleBoxFlat = StyleBoxFlat.new()
	style.bg_color = Color(0.05, 0.05, 0.08, 0.82)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_left = 6
	style.corner_radius_bottom_right = 6
	style.content_margin_left = 12.0
	style.content_margin_right = 12.0
	style.content_margin_top = 10.0
	style.content_margin_bottom = 10.0
	panel.add_theme_stylebox_override("panel", style)
	panel.set_anchors_preset(Control.PRESET_TOP_LEFT)
	add_child(panel)

	var scroll: ScrollContainer = ScrollContainer.new()
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	scroll.custom_minimum_size.x = 220.0
	var viewport_h: float = get_viewport_rect().size.y
	scroll.custom_minimum_size.y = minf(viewport_h - 80.0, 520.0)
	panel.add_child(scroll)

	var vbox: VBoxContainer = VBoxContainer.new()
	vbox.add_theme_constant_override("separation", 6)
	vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	scroll.add_child(vbox)

	_add_title(vbox, "Belt Viewer")
	_add_separator(vbox)

	_seed_spin = _add_spin(vbox, "Seed", 0, 999999, 12345, 1)
	_count_spin = _add_spin(vbox, "Background", 100, 5000, 1000, 100)
	_inner_spin = _add_spin(vbox, "Inner (AU)", 0.5, 10.0, 2.0, 0.1, 2)
	_outer_spin = _add_spin(vbox, "Outer (AU)", 1.0, 20.0, 3.5, 0.1, 2)
	_incl_spin = _add_spin(vbox, "Max Incl (°)", 0.0, 60.0, 20.0, 1.0, 1)
	_ecc_spin = _add_spin(vbox, "Max Ecc", 0.0, 0.9, 0.25, 0.05, 3)
	_conc_spin = _add_spin(vbox, "Concentration", 0.0, 6.0, 2.0, 0.5, 2)

	_add_separator(vbox)
	_gap_check = _add_checkbox(vbox, "Kirkwood Gaps")
	_gap_rings_check = _add_checkbox(vbox, "Show Gap Rings")
	_cluster_check = _add_checkbox(vbox, "Trojan Clusters")

	_add_separator(vbox)
	_add_title(vbox, "Major Bodies")
	_major_count_spin = _add_spin(vbox, "Count", 0, 10, 5, 1)
	_label_check = _add_checkbox(vbox, "Show Labels")
	_label_check.button_pressed = true

	_add_separator(vbox)

	var gen_btn: Button = Button.new()
	gen_btn.text = "Generate"
	gen_btn.pressed.connect(_on_generate_pressed)
	vbox.add_child(gen_btn)

	var reset_btn: Button = Button.new()
	reset_btn.text = "Reset Camera"
	reset_btn.pressed.connect(func() -> void: reset_view_requested.emit())
	vbox.add_child(reset_btn)

	_add_separator(vbox)
	_status_label = Label.new()
	_status_label.text = "Press Generate"
	_status_label.add_theme_font_size_override("font_size", 11)
	_status_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	_status_label.custom_minimum_size = Vector2(180, 0)
	vbox.add_child(_status_label)

	_add_separator(vbox)
	var help: Label = Label.new()
	help.text = "LMB: orbit  RMB: pan\nScroll: zoom"
	help.add_theme_font_size_override("font_size", 10)
	help.modulate = Color(0.7, 0.7, 0.7)
	vbox.add_child(help)

	# Type legend
	_add_separator(vbox)
	_add_color_legend(vbox, "C-Type", Color(0.55, 0.45, 0.35))
	_add_color_legend(vbox, "S-Type", Color(0.90, 0.80, 0.55))
	_add_color_legend(vbox, "M-Type", Color(0.80, 0.82, 0.88))


## Reads UI values, builds spec with relative gap positions, and emits.
func _on_generate_pressed() -> void:
	var spec: AsteroidBeltSpecConcept = AsteroidBeltSpecConcept.new()
	spec.inner_radius_au = _inner_spin.value
	spec.outer_radius_au = _outer_spin.value
	spec.asteroid_count = int(_count_spin.value)
	spec.max_inclination_deg = _incl_spin.value
	spec.max_eccentricity = _ecc_spin.value
	spec.radial_concentration = _conc_spin.value

	if _gap_check.button_pressed:
		_apply_relative_gaps(spec)

	if _cluster_check.button_pressed:
		spec.cluster_count = 2
		spec.cluster_longitudes_rad = [PI / 3.0, PI * 5.0 / 3.0]
		spec.cluster_concentration = 4.0
		spec.cluster_fraction = 0.3

	regenerate_requested.emit(spec, int(_seed_spin.value))


## Computes gap positions as fractions of the current belt width.
## Gaps always fall inside the belt regardless of inner/outer radius settings.
## @param spec: The spec to populate gap arrays on.
func _apply_relative_gaps(spec: AsteroidBeltSpecConcept) -> void:
	var belt_width: float = spec.outer_radius_au - spec.inner_radius_au
	if belt_width <= 0.0:
		return

	spec.gap_centers_au = []
	spec.gap_half_widths_au = []

	for i in range(GAP_FRACTIONS.size()):
		var center: float = spec.inner_radius_au + belt_width * GAP_FRACTIONS[i]
		var half_width: float = belt_width * GAP_WIDTH_FRACTIONS[i]
		spec.gap_centers_au.append(center)
		spec.gap_half_widths_au.append(half_width)


## Adds a bold title label.
func _add_title(parent: Control, text: String) -> void:
	var lbl: Label = Label.new()
	lbl.text = text
	lbl.add_theme_font_size_override("font_size", 14)
	parent.add_child(lbl)


## Adds a horizontal separator.
func _add_separator(parent: Control) -> void:
	parent.add_child(HSeparator.new())


## Adds a labelled SpinBox row and returns the SpinBox.
func _add_spin(
	parent: Control, label: String,
	min_val: float, max_val: float, default_val: float,
	step: float, decimals: int = 0
) -> SpinBox:
	var hbox: HBoxContainer = HBoxContainer.new()
	parent.add_child(hbox)
	var lbl: Label = Label.new()
	lbl.text = label + ":"
	lbl.custom_minimum_size = Vector2(110, 0)
	lbl.add_theme_font_size_override("font_size", 11)
	hbox.add_child(lbl)
	var spin: SpinBox = SpinBox.new()
	spin.min_value = min_val
	spin.max_value = max_val
	spin.value = default_val
	spin.step = step
	spin.custom_minimum_size = Vector2(80, 0)
	hbox.add_child(spin)
	return spin


## Adds a labelled CheckBox and returns it.
func _add_checkbox(parent: Control, label: String) -> CheckBox:
	var cb: CheckBox = CheckBox.new()
	cb.text = label
	cb.add_theme_font_size_override("font_size", 11)
	parent.add_child(cb)
	return cb


## Adds a small color swatch + text label for the type legend.
func _add_color_legend(parent: Control, text: String, color: Color) -> void:
	var hbox: HBoxContainer = HBoxContainer.new()
	parent.add_child(hbox)
	var swatch: ColorRect = ColorRect.new()
	swatch.color = color
	swatch.custom_minimum_size = Vector2(12, 12)
	hbox.add_child(swatch)
	var spacer: Control = Control.new()
	spacer.custom_minimum_size = Vector2(6, 0)
	hbox.add_child(spacer)
	var lbl: Label = Label.new()
	lbl.text = text
	lbl.add_theme_font_size_override("font_size", 10)
	lbl.modulate = Color(0.7, 0.7, 0.7)
	hbox.add_child(lbl)
