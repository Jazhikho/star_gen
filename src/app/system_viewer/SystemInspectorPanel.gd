## Side panel for the solar system viewer.
## Shows system overview information and details for the selected body.
class_name SystemInspectorPanel
extends VBoxContainer

const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _atmosphere_props: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _property_formatter: GDScript = preload("res://src/app/viewer/PropertyFormatter.gd")
const _planet_population_data: GDScript = preload("res://src/domain/population/PlanetPopulationData.gd")


## Emitted when the user requests to open a body in the detail viewer.
signal open_in_viewer_requested(body: CelestialBody)

## The system overview section container.
var _overview_section: VBoxContainer = null

## The selected body section container.
var _body_section: VBoxContainer = null

## Open in viewer button (for selected body).
var _open_viewer_button: Button = null

## Reference to the currently displayed system.
var _current_system: SolarSystem = null

## Reference to the currently selected body.
var _selected_body: CelestialBody = null


func _ready() -> void:
	_build_ui()


## Builds the panel UI structure.
func _build_ui() -> void:
	# System overview
	_overview_section = _create_section("System Overview")
	add_child(_overview_section)
	
	# Separator
	add_child(HSeparator.new())
	
	# Selected body details
	_body_section = _create_section("Selected Body")
	add_child(_body_section)


## Displays system overview information.
## @param system: The solar system to display info for.
func display_system(system: SolarSystem) -> void:
	_current_system = system
	_selected_body = null
	_clear_section_content(_overview_section)
	_clear_section_content(_body_section)
	_add_property(_body_section, "Status", "Click a body to inspect")
	
	if system == null:
		_add_property(_overview_section, "Status", "No system generated")
		return
	
	_add_property(_overview_section, "Name", system.name)
	_add_property(_overview_section, "Stars", str(system.get_star_count()))
	_add_property(_overview_section, "Planets", str(system.get_planet_count()))
	_add_property(_overview_section, "Moons", str(system.get_moon_count()))
	_add_property(_overview_section, "Asteroids", str(system.get_asteroid_count()))
	_add_property(_overview_section, "Asteroid Belts", str(system.asteroid_belts.size()))
	
	# Star details
	var stars: Array[CelestialBody] = system.get_stars()
	if stars.size() > 0:
		_add_separator(_overview_section)
		_add_header(_overview_section, "Stars")
		for star in stars:
			var star_info: String = _format_star_info(star)
			_add_property(_overview_section, star.name, star_info)
	
	# Orbit host info
	if system.orbit_hosts.size() > 0:
		_add_separator(_overview_section)
		_add_header(_overview_section, "Orbit Hosts")
		for host in system.orbit_hosts:
			var host_info: String = "%s (%.2f - %.2f AU)" % [
				host.get_type_string(),
				host.inner_stability_m / Units.AU_METERS,
				host.outer_stability_m / Units.AU_METERS
			]
			_add_property(_overview_section, host.node_id, host_info)


## Displays details for a selected body.
## @param body: The selected celestial body (null to clear).
func display_selected_body(body: CelestialBody) -> void:
	_selected_body = body
	_clear_section_content(_body_section)
	
	if body == null:
		_add_property(_body_section, "Status", "Click a body to inspect")
		_remove_open_viewer_button()
		return
	
	# Basic info
	_add_property(_body_section, "Name", body.name)
	_add_property(_body_section, "Type", _get_type_display(body.type))
	_add_property(_body_section, "ID", body.id)
	
	# Physical properties
	_add_separator(_body_section)
	_add_header(_body_section, "Physical")
	_add_physical_properties(body)
	
	# Orbital properties
	if body.has_orbital():
		_add_separator(_body_section)
		_add_header(_body_section, "Orbital")
		_add_orbital_properties(body)
	
	# Stellar properties
	if body.has_stellar():
		_add_separator(_body_section)
		_add_header(_body_section, "Stellar")
		_add_stellar_properties(body)
	
	# Atmosphere
	if body.has_atmosphere():
		_add_separator(_body_section)
		_add_header(_body_section, "Atmosphere")
		_add_atmosphere_properties(body)
	
	# Population summary
	if body.has_population_data():
		_add_separator(_body_section)
		_add_header(_body_section, "Population")
		_add_population_summary(body)
	
	# Open in viewer button
	_add_open_viewer_button()


## Clears all displayed information.
func clear() -> void:
	_current_system = null
	_selected_body = null
	_clear_section_content(_overview_section)
	_clear_section_content(_body_section)
	_add_property(_overview_section, "Status", "No system generated")
	_add_property(_body_section, "Status", "Click a body to inspect")


# =============================================================================
# PRIVATE HELPERS
# =============================================================================


## Adds physical property rows for a body.
## @param body: The celestial body.
func _add_physical_properties(body: CelestialBody) -> void:
	var phys: PhysicalProps = body.physical
	
	match body.type:
		CelestialType.Type.STAR:
			_add_property(_body_section, "Mass", "%.3f M☉" % (phys.mass_kg / Units.SOLAR_MASS_KG))
			_add_property(_body_section, "Radius", "%.3f R☉" % (phys.radius_m / Units.SOLAR_RADIUS_METERS))
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			_add_property(_body_section, "Mass", "%.4f M⊕" % (phys.mass_kg / Units.EARTH_MASS_KG))
			_add_property(_body_section, "Radius", "%.4f R⊕" % (phys.radius_m / Units.EARTH_RADIUS_METERS))
		CelestialType.Type.ASTEROID:
			_add_property(_body_section, "Mass", "%.3e kg" % phys.mass_kg)
			_add_property(_body_section, "Radius", "%.1f km" % (phys.radius_m / 1000.0))
	
	var density: float = _calculate_density(phys.mass_kg, phys.radius_m)
	_add_property(_body_section, "Density", "%.1f kg/mÂ³" % density)
	
	if phys.rotation_period_s != 0.0:
		var period_hours: float = absf(phys.rotation_period_s) / 3600.0
		var retrograde: String = " (retrograde)" if phys.rotation_period_s < 0 else ""
		_add_property(_body_section, "Rotation", "%.1f hours%s" % [period_hours, retrograde])
	
	if phys.axial_tilt_deg != 0.0:
		_add_property(_body_section, "Axial Tilt", "%.1f°" % phys.axial_tilt_deg)


## Adds orbital property rows for a body.
## @param body: The celestial body.
func _add_orbital_properties(body: CelestialBody) -> void:
	var orb: OrbitalProps = body.orbital
	
	var sma_au: float = orb.semi_major_axis_m / Units.AU_METERS
	if sma_au > 0.01:
		_add_property(_body_section, "Semi-major Axis", "%.4f AU" % sma_au)
	else:
		_add_property(_body_section, "Semi-major Axis", "%.0f km" % (orb.semi_major_axis_m / 1000.0))
	
	_add_property(_body_section, "Eccentricity", "%.4f" % orb.eccentricity)
	_add_property(_body_section, "Inclination", "%.2f°" % orb.inclination_deg)
	
	if not orb.parent_id.is_empty():
		_add_property(_body_section, "Orbits", orb.parent_id)


## Adds stellar property rows for a body.
## @param body: The celestial body.
func _add_stellar_properties(body: CelestialBody) -> void:
	var stellar: StellarProps = body.stellar
	
	_add_property(_body_section, "Spectral Class", stellar.spectral_class)
	_add_property(_body_section, "Temperature", "%d K" % int(stellar.effective_temperature_k))
	
	var lum_solar: float = stellar.luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	_add_property(_body_section, "Luminosity", "%.4f Lâ˜‰" % lum_solar)
	
	if stellar.age_years > 0:
		var age_gyr: float = stellar.age_years / 1.0e9
		_add_property(_body_section, "Age", "%.2f Gyr" % age_gyr)


## Adds atmosphere property rows for a body.
## @param body: The celestial body.
func _add_atmosphere_properties(body: CelestialBody) -> void:
	var atmo: AtmosphereProps = body.atmosphere
	
	var pressure_atm: float = atmo.surface_pressure_pa / 101325.0
	_add_property(_body_section, "Pressure", "%.4f atm" % pressure_atm)
	
	# Temperature is in SurfaceProps, not AtmosphereProps
	if body.has_surface() and body.surface.temperature_k > 0:
		_add_property(_body_section, "Temperature", "%.0f K" % body.surface.temperature_k)
	
	if atmo.greenhouse_factor > 1.0:
		_add_property(_body_section, "Greenhouse", "%.2fÃ—" % atmo.greenhouse_factor)


## Adds population summary for a body.
## @param body: The celestial body.
func _add_population_summary(body: CelestialBody) -> void:
	var pop_data: PlanetPopulationData = body.population_data
	
	if pop_data.profile != null:
		_add_property(_body_section, "Habitability", _property_formatter.format_habitability(pop_data.profile.habitability_score))
	
	if pop_data.suitability != null:
		_add_property(_body_section, "Suitability", _property_formatter.format_suitability(pop_data.suitability.overall_score))
	
	_add_property(_body_section, "Status", _property_formatter.format_political_situation(pop_data.get_political_situation()))
	
	var total_pop: int = pop_data.get_total_population()
	if total_pop > 0:
		_add_property(_body_section, "Total Pop.", _property_formatter.format_population(total_pop))
		_add_property(_body_section, "Dominant", pop_data.get_dominant_population_name())


## Formats star info for overview display.
## @param star: The star body.
## @return: Formatted info string.
func _format_star_info(star: CelestialBody) -> String:
	if star.has_stellar():
		return "%s (%.0f K)" % [star.stellar.spectral_class, star.stellar.effective_temperature_k]
	return "Unknown type"


## Calculates density from mass and radius.
## @param mass_kg: Mass in kg.
## @param radius_m: Radius in meters.
## @return: Density in kg/mÂ³.
func _calculate_density(mass_kg: float, radius_m: float) -> float:
	if radius_m <= 0.0:
		return 0.0
	var volume: float = (4.0 / 3.0) * PI * pow(radius_m, 3.0)
	return mass_kg / volume


## Gets display string for body type.
## @param type: CelestialType.Type.
## @return: Human-readable type name.
func _get_type_display(type: CelestialType.Type) -> String:
	match type:
		CelestialType.Type.STAR:
			return "Star"
		CelestialType.Type.PLANET:
			return "Planet"
		CelestialType.Type.MOON:
			return "Moon"
		CelestialType.Type.ASTEROID:
			return "Asteroid"
		_:
			return "Unknown"


## Creates a section container with a header label.
## @param title: Section title.
## @return: VBoxContainer for the section.
func _create_section(title: String) -> VBoxContainer:
	var section: VBoxContainer = VBoxContainer.new()
	section.add_theme_constant_override("separation", 4)
	
	var header: Label = Label.new()
	header.text = title
	header.add_theme_font_size_override("font_size", 14)
	header.add_theme_color_override("font_color", Color(0.9, 0.9, 0.9))
	section.add_child(header)
	
	return section


## Clears content from a section (preserves the header).
## @param section: The section to clear.
func _clear_section_content(section: VBoxContainer) -> void:
	if section == null:
		return
	
	# Remove all children except the first (header label)
	while section.get_child_count() > 1:
		var child: Node = section.get_child(section.get_child_count() - 1)
		section.remove_child(child)
		child.queue_free()


## Adds a property row (label: value).
## @param section: Section to add to.
## @param label_text: Property name.
## @param value_text: Property value.
func _add_property(section: VBoxContainer, label_text: String, value_text: String) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	
	var label: Label = Label.new()
	label.text = label_text + ":"
	label.custom_minimum_size = Vector2(100, 0)
	label.add_theme_font_size_override("font_size", 12)
	label.add_theme_color_override("font_color", Color(0.7, 0.7, 0.7))
	row.add_child(label)
	
	var value: Label = Label.new()
	value.text = value_text
	value.add_theme_font_size_override("font_size", 12)
	value.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	value.autowrap_mode = TextServer.AUTOWRAP_WORD
	row.add_child(value)
	
	section.add_child(row)


## Adds a sub-header to a section.
## @param section: Section to add to.
## @param text: Header text.
func _add_header(section: VBoxContainer, text: String) -> void:
	var header: Label = Label.new()
	header.text = text
	header.add_theme_font_size_override("font_size", 12)
	header.add_theme_color_override("font_color", Color(0.8, 0.8, 0.5))
	section.add_child(header)


## Adds a separator to a section.
## @param section: Section to add to.
func _add_separator(section: VBoxContainer) -> void:
	var sep: HSeparator = HSeparator.new()
	section.add_child(sep)


## Adds the "Open in Viewer" button.
func _add_open_viewer_button() -> void:
	_remove_open_viewer_button()
	
	var spacer: Control = Control.new()
	spacer.custom_minimum_size = Vector2(0, 5)
	_body_section.add_child(spacer)
	
	_open_viewer_button = Button.new()
	_open_viewer_button.text = "Open in Object Viewer"
	_open_viewer_button.tooltip_text = "View this body in detail"
	_open_viewer_button.pressed.connect(_on_open_viewer_pressed)
	_body_section.add_child(_open_viewer_button)


## Removes the "Open in Viewer" button.
func _remove_open_viewer_button() -> void:
	if _open_viewer_button != null:
		if _open_viewer_button.pressed.is_connected(_on_open_viewer_pressed):
			_open_viewer_button.pressed.disconnect(_on_open_viewer_pressed)
		_open_viewer_button.queue_free()
		_open_viewer_button = null


## Handles open in viewer button press.
func _on_open_viewer_pressed() -> void:
	if _selected_body != null:
		open_in_viewer_requested.emit(_selected_body)
