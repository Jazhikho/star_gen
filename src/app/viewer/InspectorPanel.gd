## Manages the inspector panel that displays celestial body properties.
## Dynamically creates property rows organized by component sections with collapsible headers.
class_name InspectorPanel
extends VBoxContainer

const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _color_utils := preload("res://src/app/rendering/ColorUtils.gd")

## Container for dynamically created inspector content
@onready var inspector_container: VBoxContainer = get_node("InspectorContainer")

## Current section content container for adding properties
var _current_section_content: VBoxContainer = null

## Color for section headers
const SECTION_COLOR: Color = Color(0.9, 0.9, 0.9)

## Color for property labels
const LABEL_COLOR: Color = Color(0.6, 0.6, 0.6)

## Color for property values
const VALUE_COLOR: Color = Color(0.85, 0.85, 0.85)

## Minimum width for labels
const LABEL_MIN_WIDTH: float = 100.0


## Clears all inspector content.
func clear() -> void:
	if not inspector_container:
		return
	
	for child in inspector_container.get_children():
		child.queue_free()
	
	_current_section_content = null


## Displays properties for a celestial body.
## @param body: The celestial body to inspect.
func display_body(body: CelestialBody) -> void:
	clear()
	
	if not body:
		_add_label("No object loaded")
		return
	
	# Basic info section
	_add_section("Basic Info", true)
	_add_property("Name", body.name if body.name else body.id)
	_add_property("Type", _format_type(body))
	_add_property("ID", body.id)
	
	# Physical properties
	_add_section("Physical Properties", true)
	_add_physical_properties(body)
	
	# Stellar properties (stars only)
	if body.has_stellar():
		_add_section("Stellar Properties", true)
		_add_stellar_properties(body)
	
	# Orbital properties
	if body.has_orbital():
		_add_section("Orbital Properties", true)
		_add_orbital_properties(body)
	
	# Atmosphere properties
	if body.has_atmosphere():
		_add_section("Atmosphere", true)
		_add_atmosphere_properties(body)
	
	# Surface properties
	if body.has_surface():
		_add_section("Surface", true)
		_add_surface_properties(body)
	
	# Ring system
	if body.has_ring_system():
		_add_section("Ring System", true)
		_add_ring_properties(body)


## Adds a collapsible section header with content container.
## @param title: The section title.
## @param expanded: Whether the section starts expanded.
func _add_section(title: String, expanded: bool = true) -> void:
	var section: VBoxContainer = VBoxContainer.new()
	section.add_theme_constant_override("separation", 2)
	
	# Header with collapse button
	var header: Button = Button.new()
	header.text = ("▼ " if expanded else "▶ ") + title
	header.flat = true
	header.alignment = HORIZONTAL_ALIGNMENT_LEFT
	header.add_theme_font_size_override("font_size", 14)
	header.add_theme_color_override("font_color", SECTION_COLOR)
	
	# Content container
	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 2)
	content.visible = expanded
	
	# Connect toggle
	header.pressed.connect(_toggle_section.bind(content, header))
	
	section.add_child(header)
	section.add_child(content)
	
	inspector_container.add_child(section)
	
	# Set as current section for property additions
	_current_section_content = content


## Toggles visibility of a section.
## @param content: The content container to toggle.
## @param header: The header button to update.
func _toggle_section(content: VBoxContainer, header: Button) -> void:
	content.visible = not content.visible
	var arrow: String = "▼ " if content.visible else "▶ "
	var text: String = header.text
	# Replace first 2 characters (arrow + space)
	header.text = arrow + text.substr(2)


## Adds a property row.
## @param label_text: The property label.
## @param value_text: The property value.
func _add_property(label_text: String, value_text: String) -> void:
	if not _current_section_content:
		return
	
	var row: HBoxContainer = HBoxContainer.new()
	
	var label: Label = Label.new()
	label.text = label_text + ":"
	label.custom_minimum_size.x = LABEL_MIN_WIDTH
	label.add_theme_color_override("font_color", LABEL_COLOR)
	label.add_theme_font_size_override("font_size", 12)
	row.add_child(label)
	
	var value: Label = Label.new()
	value.text = value_text
	value.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	value.add_theme_color_override("font_color", VALUE_COLOR)
	value.add_theme_font_size_override("font_size", 12)
	value.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
	row.add_child(value)
	
	_current_section_content.add_child(row)


## Adds a simple label (no property row).
## @param text: The label text.
func _add_label(text: String) -> void:
	var label: Label = Label.new()
	label.text = text
	label.add_theme_color_override("font_color", LABEL_COLOR)
	inspector_container.add_child(label)


## Adds a subsection label.
## @param title: The subsection title.
func _add_subsection(title: String) -> void:
	if not _current_section_content:
		return
	
	var label: Label = Label.new()
	label.text = "  " + title
	label.add_theme_font_size_override("font_size", 12)
	label.add_theme_color_override("font_color", Color(0.75, 0.75, 0.85))
	_current_section_content.add_child(label)


## Formats the type string for display.
## @param body: The celestial body.
## @return: Formatted type string.
func _format_type(body: CelestialBody) -> String:
	var type_str: String = body.get_type_string()
	
	if body.type == CelestialType.Type.STAR and body.has_stellar():
		type_str += " (%s)" % body.stellar.spectral_class
	
	return type_str


## Adds physical properties section content.
## @param body: The celestial body.
func _add_physical_properties(body: CelestialBody) -> void:
	var phys: PhysicalProps = body.physical
	
	_add_property("Mass", _format_mass(phys.mass_kg, body.type))
	_add_property("Radius", _format_radius(phys.radius_m, body.type))
	_add_property("Density", "%.1f kg/m³" % phys.get_density_kg_m3())
	_add_property("Surface Gravity", "%.2f m/s²" % phys.get_surface_gravity_m_s2())
	_add_property("Escape Velocity", "%.2f km/s" % (phys.get_escape_velocity_m_s() / 1000.0))
	
	# Rotation
	var rotation_hours: float = absf(phys.rotation_period_s) / 3600.0
	var rotation_str: String
	if rotation_hours < 24.0:
		rotation_str = "%.2f hours" % rotation_hours
	else:
		rotation_str = "%.2f days" % (rotation_hours / 24.0)
	if phys.rotation_period_s < 0:
		rotation_str += " (retrograde)"
	_add_property("Rotation Period", rotation_str)
	
	_add_property("Axial Tilt", "%.1f°" % phys.axial_tilt_deg)
	
	if phys.oblateness > 0.001:
		_add_property("Oblateness", "%.4f" % phys.oblateness)
	
	if phys.magnetic_moment > 0:
		_add_property("Magnetic Field", _format_scientific(phys.magnetic_moment, "T·m³"))


## Adds stellar properties section content.
## @param body: The celestial body.
func _add_stellar_properties(body: CelestialBody) -> void:
	var stellar: StellarProps = body.stellar
	
	_add_property("Spectral Class", stellar.spectral_class)
	_add_property("Luminosity", _format_luminosity(stellar.luminosity_watts))
	_add_property("Temperature", "%.0f K" % stellar.effective_temperature_k)
	_add_property("Age", _format_age(stellar.age_years))
	_add_property("Metallicity", "[Fe/H] = %.2f" % (log(stellar.metallicity) / log(10.0)))
	_add_property("Stellar Type", stellar.stellar_type.replace("_", " ").capitalize())


## Adds orbital properties section content.
## @param body: The celestial body.
func _add_orbital_properties(body: CelestialBody) -> void:
	var orbital: OrbitalProps = body.orbital
	
	_add_property("Semi-major Axis", _format_distance(orbital.semi_major_axis_m))
	_add_property("Eccentricity", "%.4f" % orbital.eccentricity)
	_add_property("Inclination", "%.2f°" % orbital.inclination_deg)
	_add_property("Periapsis", _format_distance(orbital.get_periapsis_m()))
	_add_property("Apoapsis", _format_distance(orbital.get_apoapsis_m()))
	
	if orbital.parent_id:
		_add_property("Parent Body", orbital.parent_id)


## Adds atmosphere properties section content.
## @param body: The celestial body.
func _add_atmosphere_properties(body: CelestialBody) -> void:
	var atmo: AtmosphereProps = body.atmosphere
	
	_add_property("Surface Pressure", _format_pressure(atmo.surface_pressure_pa))
	_add_property("Scale Height", "%.1f km" % (atmo.scale_height_m / 1000.0))
	
	var greenhouse_desc: String = ColorUtils.get_greenhouse_description(atmo.greenhouse_factor)
	_add_property("Greenhouse Effect", "%s (%.2fx)" % [greenhouse_desc, atmo.greenhouse_factor])
	
	# Composition
	if not atmo.composition.is_empty():
		_add_subsection("Composition:")
		var sorted_gases: Array = _sort_composition(atmo.composition)
		for gas_data in sorted_gases:
			var gas: String = gas_data[0]
			var fraction: float = gas_data[1]
			if fraction > 0.001:  # Only show > 0.1%
				_add_property("  " + gas, "%.2f%%" % (fraction * 100.0))


## Adds surface properties section content.
## @param body: The celestial body.
func _add_surface_properties(body: CelestialBody) -> void:
	var surface: SurfaceProps = body.surface
	
	_add_property("Temperature", "%.1f K (%.1f°C)" % [surface.temperature_k, surface.temperature_k - 273.15])
	_add_property("Albedo", "%.3f" % surface.albedo)
	_add_property("Surface Type", surface.surface_type.capitalize())
	
	if surface.volcanism_level > 0.01:
		_add_property("Volcanism", "%.1f%%" % (surface.volcanism_level * 100.0))
	
	# Terrain
	if surface.has_terrain():
		_add_subsection("Terrain:")
		_add_property("  Type", surface.terrain.terrain_type.capitalize())
		_add_property("  Elevation Range", "%.1f km" % (surface.terrain.elevation_range_m / 1000.0))
		_add_property("  Roughness", "%.2f" % surface.terrain.roughness)
		_add_property("  Crater Density", "%.1f%%" % (surface.terrain.crater_density * 100.0))
	
	# Hydrosphere
	if surface.has_hydrosphere():
		_add_subsection("Hydrosphere:")
		_add_property("  Ocean Coverage", "%.1f%%" % (surface.hydrosphere.ocean_coverage * 100.0))
		_add_property("  Ocean Depth", "%.1f km" % (surface.hydrosphere.ocean_depth_m / 1000.0))
		if surface.hydrosphere.ice_coverage > 0.01:
			_add_property("  Ice Coverage", "%.1f%%" % (surface.hydrosphere.ice_coverage * 100.0))
	
	# Cryosphere
	if surface.has_cryosphere():
		_add_subsection("Cryosphere:")
		_add_property("  Polar Caps", "%.1f%%" % (surface.cryosphere.polar_cap_coverage * 100.0))
		_add_property("  Ice Type", surface.cryosphere.ice_type.replace("_", " ").capitalize())
		if surface.cryosphere.has_subsurface_ocean:
			_add_property("  Subsurface Ocean", "%.1f km deep" % (surface.cryosphere.subsurface_ocean_depth_m / 1000.0))
		if surface.cryosphere.cryovolcanism_level > 0.01:
			_add_property("  Cryovolcanism", "%.1f%%" % (surface.cryosphere.cryovolcanism_level * 100.0))


## Adds ring system properties section content.
## @param body: The celestial body.
func _add_ring_properties(body: CelestialBody) -> void:
	var rings: RingSystemProps = body.ring_system
	
	_add_property("Band Count", str(rings.get_band_count()))
	_add_property("Inner Radius", "%.0f km" % (rings.get_inner_radius_m() / 1000.0))
	_add_property("Outer Radius", "%.0f km" % (rings.get_outer_radius_m() / 1000.0))
	_add_property("Total Width", "%.0f km" % (rings.get_total_width_m() / 1000.0))
	_add_property("Total Mass", _format_scientific(rings.total_mass_kg, "kg"))
	_add_property("Inclination", "%.2f°" % rings.inclination_deg)
	
	# Individual bands
	for i in range(mini(rings.get_band_count(), 5)):  # Limit to first 5
		var band: RingBand = rings.get_band(i)
		var band_name: String = band.name if band.name else "Band %d" % (i + 1)
		_add_subsection(band_name + ":")
		_add_property("  Width", "%.0f km" % (band.get_width_m() / 1000.0))
		_add_property("  Optical Depth", "%.3f" % band.optical_depth)
		_add_property("  Particle Size", _format_particle_size(band.particle_size_m))


## Sorts composition dictionary by fraction descending.
## @param composition: The composition dictionary.
## @return: Sorted array of [gas, fraction] pairs.
func _sort_composition(composition: Dictionary) -> Array:
	var pairs: Array = []
	for key in composition.keys():
		pairs.append([key, composition[key]])
	pairs.sort_custom(func(a, b): return a[1] > b[1])
	return pairs


## Formats mass with appropriate units.
## @param mass_kg: Mass in kilograms.
## @param body_type: The body type.
## @return: Formatted mass string.
func _format_mass(mass_kg: float, body_type: CelestialType.Type) -> String:
	match body_type:
		CelestialType.Type.STAR:
			return "%.3f M☉" % (mass_kg / Units.SOLAR_MASS_KG)
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			var earth_masses: float = mass_kg / Units.EARTH_MASS_KG
			if earth_masses > 100:
				return "%.2f MJ" % (mass_kg / 1.898e27)
			return "%.4f M⊕" % earth_masses
		_:
			return _format_scientific(mass_kg, "kg")


## Formats radius with appropriate units.
## @param radius_m: Radius in meters.
## @param body_type: The body type.
## @return: Formatted radius string.
func _format_radius(radius_m: float, body_type: CelestialType.Type) -> String:
	match body_type:
		CelestialType.Type.STAR:
			return "%.3f R☉" % (radius_m / Units.SOLAR_RADIUS_METERS)
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			return "%.4f R⊕" % (radius_m / Units.EARTH_RADIUS_METERS)
		_:
			var km: float = radius_m / 1000.0
			if km < 1.0:
				return "%.1f m" % radius_m
			return "%.2f km" % km


## Formats distance with appropriate units.
## @param distance_m: Distance in meters.
## @return: Formatted distance string.
func _format_distance(distance_m: float) -> String:
	var au: float = distance_m / Units.AU_METERS
	if au > 0.1:
		return "%.4f AU" % au
	var km: float = distance_m / 1000.0
	if km > 1000:
		return "%.0f km" % km
	return "%.1f km" % km


## Formats luminosity.
## @param luminosity_watts: Luminosity in watts.
## @return: Formatted luminosity string.
func _format_luminosity(luminosity_watts: float) -> String:
	var solar: float = luminosity_watts / 3.828e26
	if solar > 0.01:
		return "%.4f L☉" % solar
	return _format_scientific(luminosity_watts, "W")


## Formats age in years.
## @param years: Age in years.
## @return: Formatted age string.
func _format_age(years: float) -> String:
	if years > 1e9:
		return "%.2f Gyr" % (years / 1e9)
	if years > 1e6:
		return "%.2f Myr" % (years / 1e6)
	return "%.0f years" % years


## Formats pressure.
## @param pressure_pa: Pressure in Pascals.
## @return: Formatted pressure string.
func _format_pressure(pressure_pa: float) -> String:
	var atm: float = pressure_pa / 101325.0
	if atm > 0.01:
		return "%.4f atm" % atm
	if pressure_pa > 1.0:
		return "%.1f Pa" % pressure_pa
	return "%.2e Pa" % pressure_pa


## Formats particle size.
## @param size_m: Size in meters.
## @return: Formatted size string.
func _format_particle_size(size_m: float) -> String:
	if size_m < 0.01:
		return "%.1f mm" % (size_m * 1000.0)
	if size_m < 1.0:
		return "%.1f cm" % (size_m * 100.0)
	return "%.2f m" % size_m


## Formats a number in scientific notation.
## @param value: The value to format.
## @param unit: The unit string.
## @return: Formatted scientific notation string.
func _format_scientific(value: float, unit: String) -> String:
	if value == 0.0:
		return "0 %s" % unit
	
	var exponent: int = int(floor(log(absf(value)) / log(10.0)))
	var mantissa: float = value / pow(10.0, exponent)
	
	var exp_str: String = _format_superscript(exponent)
	return "%.2f × 10%s %s" % [mantissa, exp_str, unit]


## Converts an integer to superscript characters.
## @param num: The number to convert.
## @return: Superscript string.
func _format_superscript(num: int) -> String:
	var superscripts: Dictionary = {
		"0": "⁰", "1": "¹", "2": "²", "3": "³", "4": "⁴",
		"5": "⁵", "6": "⁶", "7": "⁷", "8": "⁸", "9": "⁹",
		"-": "⁻"
	}
	
	var num_str: String = str(num)
	var result: String = ""
	
	for c in num_str:
		result += superscripts.get(c, c)
	
	return result
