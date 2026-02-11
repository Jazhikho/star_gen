## Prototype scene demonstrating station and outpost generation.
## Allows exploring different system contexts and generation parameters.
extends Control

# Preload dependencies.
const _station_generator: GDScript = preload("res://src/domain/population/StationGenerator.gd")
const _station_spec: GDScript = preload("res://src/domain/population/StationSpec.gd")
const _station_placement_rules: GDScript = preload("res://src/domain/population/StationPlacementRules.gd")
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_service: GDScript = preload("res://src/domain/population/StationService.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _outpost_authority: GDScript = preload("res://src/domain/population/OutpostAuthority.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _outpost: GDScript = preload("res://src/domain/population/Outpost.gd")
const _space_station: GDScript = preload("res://src/domain/population/SpaceStation.gd")

# UI References - Left Panel (Controls)
@onready var context_option: OptionButton = %ContextOption
@onready var seed_spinbox: SpinBox = %SeedSpinBox
@onready var random_seed_button: Button = %RandomSeedButton
@onready var density_slider: HSlider = %DensitySlider
@onready var density_label: Label = %DensityLabel
@onready var min_stations_spinbox: SpinBox = %MinStationsSpinBox
@onready var max_stations_spinbox: SpinBox = %MaxStationsSpinBox
@onready var allow_utility_check: CheckBox = %AllowUtilityCheck
@onready var allow_outposts_check: CheckBox = %AllowOutpostsCheck
@onready var allow_large_check: CheckBox = %AllowLargeCheck
@onready var colony_count_spinbox: SpinBox = %ColonyCountSpinBox
@onready var native_check: CheckBox = %NativeCheck
@onready var native_spacefaring_check: CheckBox = %NativeSpacefaringCheck
@onready var resource_slider: HSlider = %ResourceSlider
@onready var resource_label: Label = %ResourceLabel
@onready var belt_count_spinbox: SpinBox = %BeltCountSpinBox
@onready var generate_button: Button = %GenerateButton

# UI References - Right Panel (Results)
@onready var summary_label: RichTextLabel = %SummaryLabel
@onready var station_list: ItemList = %StationList
@onready var detail_text: RichTextLabel = %DetailText

# Current generation result
var _current_result: StationGenerator.GenerationResult = null
var _all_items: Array = []


func _ready() -> void:
	_setup_ui()
	_connect_signals()
	_generate()


## Sets up UI elements.
func _setup_ui() -> void:
	context_option.clear()
	context_option.add_item("Auto-detect", -1)
	context_option.add_item("Bridge System", StationPlacementContext.Context.BRIDGE_SYSTEM)
	context_option.add_item("Colony World", StationPlacementContext.Context.COLONY_WORLD)
	context_option.add_item("Native World", StationPlacementContext.Context.NATIVE_WORLD)
	context_option.add_item("Resource System", StationPlacementContext.Context.RESOURCE_SYSTEM)
	context_option.add_item("Strategic", StationPlacementContext.Context.STRATEGIC)
	context_option.add_item("Scientific", StationPlacementContext.Context.SCIENTIFIC)
	context_option.add_item("Other", StationPlacementContext.Context.OTHER)
	context_option.select(0)

	seed_spinbox.value = randi() % 100000
	density_slider.value = 1.0
	_update_density_label()
	resource_slider.value = 0.3
	_update_resource_label()

	allow_utility_check.button_pressed = true
	allow_outposts_check.button_pressed = true
	allow_large_check.button_pressed = true
	native_check.button_pressed = false
	native_spacefaring_check.button_pressed = false
	native_spacefaring_check.disabled = true


## Connects UI signals.
func _connect_signals() -> void:
	generate_button.pressed.connect(_generate)
	random_seed_button.pressed.connect(_randomize_seed)
	density_slider.value_changed.connect(_on_density_changed)
	resource_slider.value_changed.connect(_on_resource_changed)
	native_check.toggled.connect(_on_native_toggled)
	station_list.item_selected.connect(_on_station_selected)

	context_option.item_selected.connect(_on_settings_changed)
	colony_count_spinbox.value_changed.connect(_on_settings_changed)
	native_spacefaring_check.toggled.connect(_on_settings_changed)
	belt_count_spinbox.value_changed.connect(_on_settings_changed)


## Randomizes the seed.
func _randomize_seed() -> void:
	seed_spinbox.value = randi() % 100000
	_generate()


## Handles settings change for auto-regenerate.
func _on_settings_changed(_arg: Variant = null) -> void:
	_generate()


## Updates density label.
func _update_density_label() -> void:
	density_label.text = "%.1fx" % density_slider.value


## Updates resource label.
func _update_resource_label() -> void:
	resource_label.text = "%.0f%%" % (resource_slider.value * 100)


func _on_density_changed(_value: float) -> void:
	_update_density_label()


func _on_resource_changed(_value: float) -> void:
	_update_resource_label()


func _on_native_toggled(pressed: bool) -> void:
	native_spacefaring_check.disabled = not pressed
	if not pressed:
		native_spacefaring_check.button_pressed = false
	_generate()


## Generates stations based on current settings.
func _generate() -> void:
	var ctx: StationPlacementRules.SystemContext = _build_context()
	var spec: StationSpec = _build_spec()

	_current_result = StationGenerator.generate(ctx, spec)

	_update_summary()
	_update_station_list()
	_clear_detail()


## Builds system context from UI.
func _build_context() -> StationPlacementRules.SystemContext:
	var ctx: StationPlacementRules.SystemContext = StationPlacementRules.SystemContext.new()
	ctx.system_id = "prototype_system"

	var selected_context: int = context_option.get_selected_id()
	if selected_context == StationPlacementContext.Context.BRIDGE_SYSTEM:
		ctx.is_bridge_system = true

	var colony_count: int = int(colony_count_spinbox.value)
	ctx.colony_world_count = colony_count
	ctx.habitable_planet_count = colony_count
	for i in range(colony_count):
		var planet_id: String = "planet_%03d" % i
		ctx.colony_planet_ids.append(planet_id)
		ctx.planet_ids.append(planet_id)

	if native_check.button_pressed:
		ctx.native_world_count = 1
		var native_planet: String = "native_planet_001"
		ctx.native_planet_ids.append(native_planet)
		if native_planet not in ctx.planet_ids:
			ctx.planet_ids.append(native_planet)

		if native_spacefaring_check.button_pressed:
			ctx.has_spacefaring_natives = true
			ctx.highest_native_tech = TechnologyLevel.Level.SPACEFARING
		else:
			ctx.has_spacefaring_natives = false
			ctx.highest_native_tech = TechnologyLevel.Level.INDUSTRIAL

	ctx.resource_richness = resource_slider.value
	ctx.asteroid_belt_count = int(belt_count_spinbox.value)

	if ctx.resource_richness > 0.2:
		for i in range(3):
			ctx.resource_body_ids.append("asteroid_%03d" % i)

	return ctx


## Builds generation spec from UI.
func _build_spec() -> StationSpec:
	var spec: StationSpec = StationSpec.new()

	spec.seed = int(seed_spinbox.value)
	spec.population_density = density_slider.value
	spec.min_stations = int(min_stations_spinbox.value)
	spec.max_stations = int(max_stations_spinbox.value)
	spec.allow_utility = allow_utility_check.button_pressed
	spec.allow_outposts = allow_outposts_check.button_pressed
	spec.allow_large_stations = allow_large_check.button_pressed

	var selected_context: int = context_option.get_selected_id()
	if selected_context >= 0:
		spec.force_context = selected_context as int

	spec.founding_civilization_id = "proto_civ"
	spec.founding_civilization_name = "Prototype Civilization"

	return spec


## Updates the summary display.
func _update_summary() -> void:
	if _current_result == null:
		summary_label.text = "No generation result"
		return

	var lines: Array[String] = []

	var rec: StationPlacementRules.PlacementRecommendation = _current_result.recommendation
	if rec != null:
		lines.append("[b]Context:[/b] %s" % StationPlacementContext.to_string_name(rec.context))
		lines.append("[b]Should Have Stations:[/b] %s" % ("Yes" if rec.should_have_stations else "No"))

	lines.append("")
	lines.append("[b]Generation Results:[/b]")
	lines.append("Seed: %d" % _current_result.generation_seed)
	lines.append("Total Stations: %d" % _current_result.get_total_count())
	lines.append("  - Outposts: %d" % _current_result.outposts.size())
	lines.append("  - Stations: %d" % _current_result.stations.size())

	var total_pop: int = 0
	for o in _current_result.outposts:
		total_pop += o.population
	for s in _current_result.stations:
		total_pop += s.population
	lines.append("Total Population: %s" % _format_population(total_pop))

	var class_counts: Dictionary = {}
	for o in _current_result.outposts:
		var cls: String = StationClass.to_letter(o.station_class)
		class_counts[cls] = class_counts.get(cls, 0) + 1
	for s in _current_result.stations:
		var cls: String = StationClass.to_letter(s.station_class)
		class_counts[cls] = class_counts.get(cls, 0) + 1

	if not class_counts.is_empty():
		var class_str: String = ""
		for cls in ["U", "O", "B", "A", "S"]:
			if class_counts.has(cls):
				class_str += "%s:%d " % [cls, class_counts[cls]]
		lines.append("By Class: %s" % class_str.strip_edges())

	if not _current_result.warnings.is_empty():
		lines.append("")
		lines.append("[color=yellow][b]Warnings:[/b][/color]")
		for warning in _current_result.warnings:
			lines.append("  - %s" % warning)

	if rec != null and not rec.reasoning.is_empty():
		lines.append("")
		lines.append("[b]Reasoning:[/b]")
		for reason in rec.reasoning:
			lines.append("  • %s" % reason)

	summary_label.text = "\n".join(lines)


## Updates the station list.
func _update_station_list() -> void:
	station_list.clear()
	_all_items.clear()

	if _current_result == null:
		return

	for outpost in _current_result.outposts:
		var status: String = "" if outpost.is_operational else " [DECOMM]"
		var text: String = "[%s] %s - %s (%s)%s" % [
			StationClass.to_letter(outpost.station_class),
			outpost.name,
			StationPurpose.to_string_name(outpost.primary_purpose),
			_format_population(outpost.population),
			status
		]
		station_list.add_item(text)
		_all_items.append({"type": "outpost", "data": outpost})

	for station in _current_result.stations:
		var status: String = "" if station.is_operational else " [DECOMM]"
		var text: String = "[%s] %s - %s (%s)%s" % [
			StationClass.to_letter(station.station_class),
			station.name,
			StationPurpose.to_string_name(station.primary_purpose),
			_format_population(station.population),
			status
		]
		station_list.add_item(text)
		_all_items.append({"type": "station", "data": station})


## Clears the detail panel.
func _clear_detail() -> void:
	detail_text.text = "Select a station to view details."


## Handles station selection.
func _on_station_selected(index: int) -> void:
	if index < 0 or index >= _all_items.size():
		_clear_detail()
		return

	var item: Dictionary = _all_items[index]
	if item["type"] == "outpost":
		_show_outpost_detail(item["data"] as Outpost)
	else:
		_show_station_detail(item["data"] as SpaceStation)


## Shows outpost details.
func _show_outpost_detail(outpost: Outpost) -> void:
	var lines: Array[String] = []

	lines.append("[b][u]%s[/u][/b]" % outpost.name)
	lines.append("")

	lines.append("[b]ID:[/b] %s" % outpost.id)
	lines.append("[b]Class:[/b] %s (%s)" % [
		StationClass.to_letter(outpost.station_class),
		StationClass.to_string_name(outpost.station_class)
	])
	lines.append("[b]Type:[/b] %s" % StationType.to_string_name(outpost.station_type))
	lines.append("[b]Purpose:[/b] %s" % StationPurpose.to_string_name(outpost.primary_purpose))
	lines.append("[b]Context:[/b] %s" % StationPlacementContext.to_string_name(outpost.placement_context))

	lines.append("")
	lines.append("[b]Population:[/b] %s" % _format_population(outpost.population))
	lines.append("[b]Established:[/b] Year %d" % outpost.established_year)
	lines.append("[b]Age:[/b] %d years" % outpost.get_age())

	lines.append("")
	lines.append("[b]System:[/b] %s" % outpost.system_id)
	if not outpost.orbiting_body_id.is_empty():
		lines.append("[b]Orbiting:[/b] %s" % outpost.orbiting_body_id)

	lines.append("")
	lines.append("[b]Authority:[/b] %s" % OutpostAuthority.to_string_name(outpost.authority))
	lines.append("[b]Commander:[/b] %s" % outpost.commander_title)
	if outpost.has_parent_organization():
		lines.append("[b]Organization:[/b] %s" % outpost.parent_organization_name)

	if not outpost.services.is_empty():
		lines.append("")
		lines.append("[b]Services:[/b]")
		for service in outpost.services:
			lines.append("  • %s" % StationService.to_string_name(service))

	if not outpost.is_operational:
		lines.append("")
		lines.append("[color=red][b]DECOMMISSIONED[/b][/color]")
		lines.append("Year: %d" % outpost.decommissioned_year)
		lines.append("Reason: %s" % outpost.decommissioned_reason)

	detail_text.text = "\n".join(lines)


## Shows station details.
func _show_station_detail(station: SpaceStation) -> void:
	var lines: Array[String] = []

	lines.append("[b][u]%s[/u][/b]" % station.name)
	lines.append("")

	lines.append("[b]ID:[/b] %s" % station.id)
	lines.append("[b]Class:[/b] %s (%s)" % [
		StationClass.to_letter(station.station_class),
		StationClass.to_string_name(station.station_class)
	])
	lines.append("[b]Type:[/b] %s" % StationType.to_string_name(station.station_type))
	lines.append("[b]Purpose:[/b] %s" % StationPurpose.to_string_name(station.primary_purpose))
	lines.append("[b]Context:[/b] %s" % StationPlacementContext.to_string_name(station.placement_context))

	lines.append("")
	lines.append("[b]Population:[/b] %s" % _format_population(station.population))
	lines.append("[b]Peak Population:[/b] %s (Year %d)" % [
		_format_population(station.peak_population),
		station.peak_population_year
	])
	lines.append("[b]Growth State:[/b] %s" % station.get_growth_state().capitalize())
	lines.append("[b]Established:[/b] Year %d" % station.established_year)
	lines.append("[b]Age:[/b] %d years" % station.get_age())

	lines.append("")
	lines.append("[b]System:[/b] %s" % station.system_id)
	if not station.orbiting_body_id.is_empty():
		lines.append("[b]Orbiting:[/b] %s" % station.orbiting_body_id)

	lines.append("")
	if station.uses_outpost_government():
		lines.append("[b]Governance:[/b] Outpost Authority")
		lines.append("[b]Authority:[/b] %s" % OutpostAuthority.to_string_name(station.outpost_authority))
		lines.append("[b]Commander:[/b] %s" % station.commander_title)
		if station.has_parent_organization():
			lines.append("[b]Organization:[/b] %s" % station.parent_organization_name)
	else:
		lines.append("[b]Governance:[/b] Colony Government")
		if station.government != null:
			lines.append("[b]Regime:[/b] %s" % GovernmentType.to_string_name(station.government.regime))
			lines.append("[b]Legitimacy:[/b] %.0f%%" % (station.government.legitimacy * 100))
			lines.append("[b]Stable:[/b] %s" % ("Yes" if station.is_politically_stable() else "No"))

	if not station.founding_civilization_name.is_empty():
		lines.append("")
		lines.append("[b]Founded By:[/b] %s" % station.founding_civilization_name)

	if station.is_independent:
		lines.append("[b]Independent:[/b] Yes (Year %d)" % station.independence_year)

	if not station.services.is_empty():
		lines.append("")
		lines.append("[b]Services (%d):[/b]" % station.services.size())
		var service_names: Array[String] = []
		for service in station.services:
			service_names.append(StationService.to_string_name(service))
		var col_size: int = 3
		var i: int = 0
		while i < service_names.size():
			var row: String = "  "
			for j in range(col_size):
				if i + j < service_names.size():
					row += "• %s  " % service_names[i + j]
			lines.append(row)
			i += col_size

	if station.history != null and station.history.size() > 0:
		lines.append("")
		lines.append("[b]History (%d events):[/b]" % station.history.size())
		var events: Array = station.history.get_events_in_range(-10000, 0)
		var show_count: int = mini(events.size(), 5)
		for k in range(show_count):
			var evt: Variant = events[k]
			var event_data: Dictionary = evt.to_dict()
			lines.append("  Year %d: %s" % [event_data.get("year", 0), event_data.get("title", "Unknown")])

	if not station.is_operational:
		lines.append("")
		lines.append("[color=red][b]DECOMMISSIONED[/b][/color]")
		lines.append("Year: %d" % station.decommissioned_year)
		lines.append("Reason: %s" % station.decommissioned_reason)

	detail_text.text = "\n".join(lines)


## Formats population for display.
func _format_population(pop: int) -> String:
	if pop >= 1_000_000:
		return "%.2fM" % (pop / 1_000_000.0)
	elif pop >= 1_000:
		return "%.1fK" % (pop / 1_000.0)
	else:
		return str(pop)
