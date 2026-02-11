## Manages the inspector panel that displays celestial body properties.
## Dynamically creates property rows organized by component sections with collapsible headers.
class_name InspectorPanel
extends VBoxContainer

const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _color_utils: GDScript = preload("res://src/app/rendering/ColorUtils.gd")
const _property_formatter: GDScript = preload("res://src/app/viewer/PropertyFormatter.gd")
const _planet_population_data: GDScript = preload("res://src/domain/population/PlanetPopulationData.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")

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

## Colour used for moon name buttons in the moon list.
const MOON_COLOR: Color = Color(0.7, 0.85, 1.0)

## Colour used to highlight the currently-focused moon.
const FOCUSED_MOON_COLOR: Color = Color(1.0, 0.85, 0.3)

## Emitted when the user clicks a moon in the moon list. Null means "return focus to the planet".
signal moon_selected(moon: CelestialBody)


## Clears all inspector content.
func clear() -> void:
	if not inspector_container:
		return
	
	for child in inspector_container.get_children():
		child.queue_free()
	
	_current_section_content = null


## Displays a primary body with no moon list.
## Convenience wrapper around display_body_with_moons.
## @param body: The celestial body to inspect.
func display_body(body: CelestialBody) -> void:
	display_body_with_moons(body, [])


## Displays a primary body. If moons is non-empty a collapsible moon list is
## prepended so the user can shift focus to any moon from the panel.
## @param body: The primary body.
## @param moons: Moons orbiting the body (may be empty).
func display_body_with_moons(
	body: CelestialBody,
	moons: Array[CelestialBody]
) -> void:
	clear()

	if not body:
		_add_label("No object loaded")
		return

	if not moons.is_empty():
		_add_moon_list_section(moons, null)

	_add_body_sections(body)


## Displays a focused moon view: moon properties, "back to planet" button,
## compact moon switcher, and collapsed planet summary.
## @param moon: The moon currently focused.
## @param planet: The parent planet.
## @param all_moons: All moons (used for the switcher; may equal [moon]).
func display_focused_moon(
	moon: CelestialBody,
	planet: CelestialBody,
	all_moons: Array[CelestialBody]
) -> void:
	clear()

	if not moon:
		return

	_add_back_to_planet_button(planet)

	_add_section("Moon: %s" % moon.name, true)
	_add_property("Type", _format_type(moon))
	_add_property("ID", moon.id)

	_add_section("Physical Properties", true)
	_add_physical_properties(moon)

	if moon.has_orbital():
		_add_section("Orbital Properties", true)
		_add_orbital_properties(moon)

	if moon.has_atmosphere():
		_add_section("Atmosphere", true)
		_add_atmosphere_properties(moon)

	if moon.has_surface():
		_add_section("Surface", true)
		_add_surface_properties(moon)

	if moon.has_population_data():
		_add_section("Planet Profile", true)
		_add_profile_properties(moon)

	if all_moons.size() > 1:
		_add_moon_list_section(all_moons, moon)

	if planet:
		_add_section("Parent: %s" % planet.name, false)
		_add_property("Type", _format_type(planet))
		_add_physical_properties(planet)


## Adds a collapsible "Moons" section with one button row per moon.
## The focused moon (if any) is highlighted in a distinct colour.
## @param moons: Moons to list.
## @param focused_moon: Currently focused moon, or null.
func _add_moon_list_section(
	moons: Array[CelestialBody],
	focused_moon: CelestialBody
) -> void:
	var section: VBoxContainer = VBoxContainer.new()
	section.add_theme_constant_override("separation", 2)

	var header: Button = Button.new()
	header.text = "▼ Moons (%d)" % moons.size()
	header.flat = true
	header.alignment = HORIZONTAL_ALIGNMENT_LEFT
	header.add_theme_font_size_override("font_size", 14)
	header.add_theme_color_override("font_color", SECTION_COLOR)

	var content: VBoxContainer = VBoxContainer.new()
	content.add_theme_constant_override("separation", 3)
	content.visible = true

	header.pressed.connect(_toggle_section.bind(content, header))

	for moon: CelestialBody in moons:
		var row: HBoxContainer = HBoxContainer.new()
		var is_focused: bool = focused_moon != null and moon.id == focused_moon.id

		var btn: Button = Button.new()
		btn.text = moon.name
		btn.flat = true
		btn.alignment = HORIZONTAL_ALIGNMENT_LEFT
		btn.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		btn.add_theme_font_size_override("font_size", 12)
		var btn_color: Color = MOON_COLOR
		if is_focused:
			btn_color = FOCUSED_MOON_COLOR
		btn.add_theme_color_override("font_color", btn_color)
		btn.pressed.connect(moon_selected.emit.bind(moon))
		row.add_child(btn)

		var tag: Label = Label.new()
		tag.text = _brief_moon_tag(moon)
		tag.add_theme_font_size_override("font_size", 11)
		tag.add_theme_color_override("font_color", LABEL_COLOR)
		row.add_child(tag)

		content.add_child(row)

	section.add_child(header)
	section.add_child(content)
	inspector_container.add_child(section)


## Returns a short one-line descriptor: radius and orbital distance.
## @param moon: The moon body.
## @return: Formatted string, e.g. "  0.272 R⊕  384 Mkm".
func _brief_moon_tag(moon: CelestialBody) -> String:
	var parts: Array[String] = []
	if moon.physical:
		var r: float = moon.physical.radius_m / Units.EARTH_RADIUS_METERS
		parts.append("%.3f R⊕" % r)
	if moon.has_orbital():
		var dist_km: float = moon.orbital.semi_major_axis_m / 1000.0
		if dist_km >= 1.0e6:
			parts.append("%.2f Gkm" % (dist_km / 1.0e6))
		elif dist_km >= 1000.0:
			parts.append("%.0f Mkm" % (dist_km / 1000.0))
		else:
			parts.append("%.0f km" % dist_km)
	return "  " + "  ".join(parts)


## Adds a "← Back to [planet name]" button that emits moon_selected(null).
## @param planet: Parent planet (used for the button label).
func _add_back_to_planet_button(planet: CelestialBody) -> void:
	if not inspector_container:
		return
	var back_label: String = "Planet"
	if planet:
		back_label = planet.name
	var btn: Button = Button.new()
	btn.text = "← Back to %s" % back_label
	btn.tooltip_text = "Refocus on the parent planet"
	btn.pressed.connect(moon_selected.emit.bind(null))
	inspector_container.add_child(btn)


## Adds all standard sections for a full body display.
## @param body: The body whose properties to display.
func _add_body_sections(body: CelestialBody) -> void:
	_add_section("Basic Info", true)
	var name_val: String = body.id
	if body.name:
		name_val = body.name
	_add_property("Name", name_val)
	_add_property("Type", _format_type(body))
	_add_property("ID", body.id)

	_add_section("Physical Properties", true)
	_add_physical_properties(body)

	if body.has_stellar():
		_add_section("Stellar Properties", true)
		_add_stellar_properties(body)

	if body.has_orbital():
		_add_section("Orbital Properties", true)
		_add_orbital_properties(body)

	if body.has_atmosphere():
		_add_section("Atmosphere", true)
		_add_atmosphere_properties(body)

	if body.has_surface():
		_add_section("Surface", true)
		_add_surface_properties(body)

	if body.has_ring_system():
		_add_section("Ring System", true)
		_add_ring_properties(body)

	if body.has_population_data():
		_add_section("Planet Profile", true)
		_add_profile_properties(body)

		_add_section("Colony Suitability", true)
		_add_suitability_properties(body)

		if body.population_data.has_natives():
			_add_section("Native Populations", true)
			_add_native_properties(body)

		if body.population_data.has_colonies():
			_add_section("Colonies", true)
			_add_colony_properties(body)


## Adds a collapsible section header with content container.
## @param title: The section title.
## @param expanded: Whether the section starts expanded.
func _add_section(title: String, expanded: bool = true) -> void:
	var section: VBoxContainer = VBoxContainer.new()
	section.add_theme_constant_override("separation", 2)
	
	# Header with collapse button
	var header: Button = Button.new()
	header.text = ("â–¼ " if expanded else "â–¶ ") + title
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
	var arrow: String = "â–¼ " if content.visible else "â–¶ "
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
	
	_add_property("Mass", _property_formatter.format_mass(phys.mass_kg, body.type))
	_add_property("Radius", _property_formatter.format_radius(phys.radius_m, body.type))
	_add_property("Density", "%.1f kg/mÂ³" % phys.get_density_kg_m3())
	_add_property("Surface Gravity", "%.2f m/sÂ²" % phys.get_surface_gravity_m_s2())
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
	
	_add_property("Axial Tilt", "%.1fÂ°" % phys.axial_tilt_deg)
	
	if phys.oblateness > 0.001:
		_add_property("Oblateness", "%.4f" % phys.oblateness)
	
	if phys.magnetic_moment > 0:
		_add_property("Magnetic Field", _property_formatter.format_scientific(phys.magnetic_moment, "TÂ·mÂ³"))


## Adds stellar properties section content.
## @param body: The celestial body.
func _add_stellar_properties(body: CelestialBody) -> void:
	var stellar: StellarProps = body.stellar
	
	_add_property("Spectral Class", stellar.spectral_class)
	_add_property("Luminosity", _property_formatter.format_luminosity(stellar.luminosity_watts))
	_add_property("Temperature", "%.0f K" % stellar.effective_temperature_k)
	_add_property("Age", _property_formatter.format_age(stellar.age_years))
	_add_property("Metallicity", "[Fe/H] = %.2f" % (log(stellar.metallicity) / log(10.0)))
	_add_property("Stellar Type", stellar.stellar_type.replace("_", " ").capitalize())


## Adds orbital properties section content.
## @param body: The celestial body.
func _add_orbital_properties(body: CelestialBody) -> void:
	var orbital: OrbitalProps = body.orbital
	
	_add_property("Semi-major Axis", _property_formatter.format_distance(orbital.semi_major_axis_m))
	_add_property("Eccentricity", "%.4f" % orbital.eccentricity)
	_add_property("Inclination", "%.2fÂ°" % orbital.inclination_deg)
	_add_property("Periapsis", _property_formatter.format_distance(orbital.get_periapsis_m()))
	_add_property("Apoapsis", _property_formatter.format_distance(orbital.get_apoapsis_m()))
	
	if orbital.parent_id:
		_add_property("Parent Body", orbital.parent_id)


## Adds atmosphere properties section content.
## @param body: The celestial body.
func _add_atmosphere_properties(body: CelestialBody) -> void:
	var atmo: AtmosphereProps = body.atmosphere
	
	_add_property("Surface Pressure", _property_formatter.format_pressure(atmo.surface_pressure_pa))
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
			if fraction > 0.001: # Only show > 0.1%
				_add_property("  " + gas, "%.2f%%" % (fraction * 100.0))


## Adds surface properties section content.
## @param body: The celestial body.
func _add_surface_properties(body: CelestialBody) -> void:
	var surface: SurfaceProps = body.surface
	
	_add_property("Temperature", "%.1f K (%.1fÂ°C)" % [surface.temperature_k, surface.temperature_k - 273.15])
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
	_add_property("Total Mass", _property_formatter.format_scientific(rings.total_mass_kg, "kg"))
	_add_property("Inclination", "%.2fÂ°" % rings.inclination_deg)
	
	# Individual bands
	for i in range(mini(rings.get_band_count(), 5)): # Limit to first 5
		var band: RingBand = rings.get_band(i)
		var band_name: String = band.name if band.name else "Band %d" % (i + 1)
		_add_subsection(band_name + ":")
		_add_property("  Width", "%.0f km" % (band.get_width_m() / 1000.0))
		_add_property("  Optical Depth", "%.3f" % band.optical_depth)
		_add_property("  Particle Size", _property_formatter.format_particle_size(band.particle_size_m))


## Adds planet profile section content.
## @param body: The celestial body with population data.
func _add_profile_properties(body: CelestialBody) -> void:
	var profile: PlanetProfile = body.population_data.profile
	if not profile:
		_add_property("Status", "No profile available")
		return
	
	_add_property("Habitability", _property_formatter.format_habitability(profile.habitability_score))
	_add_property("Temperature", _property_formatter.format_temperature(profile.avg_temperature_k))
	_add_property("Pressure", "%.3f atm" % profile.pressure_atm)
	_add_property("Gravity", "%.2f g" % profile.gravity_g)
	_add_property("Ocean Coverage", _property_formatter.format_percent(profile.ocean_coverage))
	_add_property("Land Coverage", _property_formatter.format_percent(profile.land_coverage))
	_add_property("Ice Coverage", _property_formatter.format_percent(profile.ice_coverage))
	
	if profile.continent_count > 0:
		_add_property("Continents", str(profile.continent_count))
	
	_add_property("Day Length", "%.1f hours" % profile.day_length_hours)
	
	# Key boolean flags
	var flags: Array[String] = []
	if profile.has_liquid_water:
		flags.append("Water")
	if profile.has_breathable_atmosphere:
		flags.append("Breathable")
	if profile.has_magnetic_field:
		flags.append("Magnetic Field")
	if profile.is_tidally_locked:
		flags.append("Tidally Locked")
	if not flags.is_empty():
		_add_property("Features", ", ".join(flags))
	
	# Dominant biome
	if not profile.biomes.is_empty():
		var dominant: BiomeType.Type = profile.get_dominant_biome()
		_add_property("Dominant Biome", BiomeType.to_string_name(dominant))


## Adds colony suitability section content.
## @param body: The celestial body with population data.
func _add_suitability_properties(body: CelestialBody) -> void:
	var suitability: ColonySuitability = body.population_data.suitability
	if not suitability:
		_add_property("Status", "No assessment available")
		return
	
	_add_property("Overall Score", _property_formatter.format_suitability(suitability.overall_score))
	_add_property("Carrying Capacity", _property_formatter.format_population(suitability.carrying_capacity))
	_add_property("Growth Rate", "%.1f%%" % (suitability.base_growth_rate * 100.0))
	_add_property("Infrastructure", "%.1fx difficulty" % suitability.infrastructure_difficulty)
	
	# Factor scores
	_add_subsection("Factor Scores:")
	for factor_int in suitability.factor_scores.keys():
		var factor: ColonySuitability.FactorType = factor_int as ColonySuitability.FactorType
		var score: int = suitability.factor_scores[factor_int] as int
		_add_property("  " + ColonySuitability.factor_to_string(factor), "%d/100" % score)
	
	# Limiting factors
	if not suitability.limiting_factors.is_empty():
		var limit_names: Array[String] = []
		for factor in suitability.limiting_factors:
			limit_names.append(ColonySuitability.factor_to_string(factor))
		_add_property("Limiting", ", ".join(limit_names))
	
	# Advantages
	if not suitability.advantages.is_empty():
		var adv_names: Array[String] = []
		for factor in suitability.advantages:
			adv_names.append(ColonySuitability.factor_to_string(factor))
		_add_property("Advantages", ", ".join(adv_names))


## Adds native population section content.
## @param body: The celestial body with population data.
func _add_native_properties(body: CelestialBody) -> void:
	var pop_data: PlanetPopulationData = body.population_data
	
	_add_property("Total Natives", str(pop_data.native_populations.size()))
	_add_property("Extant", str(pop_data.get_extant_native_count()))
	_add_property("Total Pop.", _property_formatter.format_population(pop_data.get_native_population()))
	
	for native in pop_data.native_populations:
		var status_str: String = native.get_growth_state()
		if not native.is_extant:
			status_str = "EXTINCT"
		_add_subsection(native.name + " (%s):" % status_str)
		_add_property("  Population", _property_formatter.format_population(native.population))
		_add_property("  Tech Level", _property_formatter.format_tech_level(native.tech_level))
		_add_property("  Government", _property_formatter.format_regime(native.get_regime()))
		_add_property("  Territory", _property_formatter.format_percent(native.territorial_control))
		if not native.is_extant:
			_add_property("  Cause", native.extinction_cause)


## Adds colony section content.
## @param body: The celestial body with population data.
func _add_colony_properties(body: CelestialBody) -> void:
	var pop_data: PlanetPopulationData = body.population_data
	
	_add_property("Total Colonies", str(pop_data.colonies.size()))
	_add_property("Active", str(pop_data.get_active_colony_count()))
	_add_property("Total Pop.", _property_formatter.format_population(pop_data.get_colony_population()))
	
	for colony in pop_data.colonies:
		var status_str: String = colony.get_growth_state()
		if not colony.is_active:
			status_str = "ABANDONED"
		_add_subsection(colony.name + " (%s):" % status_str)
		_add_property("  Type", _property_formatter.format_colony_type(colony.colony_type))
		_add_property("  Population", _property_formatter.format_population(colony.population))
		_add_property("  Tech Level", _property_formatter.format_tech_level(colony.tech_level))
		_add_property("  Government", _property_formatter.format_regime(colony.get_regime()))
		_add_property("  Territory", _property_formatter.format_percent(colony.territorial_control))
		_add_property("  Self-Sufficiency", _property_formatter.format_percent(colony.self_sufficiency))
		if colony.is_independent:
			_add_property("  Status", "Independent")
		if not colony.is_active:
			_add_property("  Reason", colony.abandonment_reason)
		var native_status: String = colony.get_overall_native_status()
		if native_status != "none":
			_add_property("  Native Relations", native_status.capitalize())
	
	# Overall situation
	_add_subsection("Political Situation:")
	_add_property("  Status", _property_formatter.format_political_situation(pop_data.get_political_situation()))
	_add_property("  Highest Tech", _property_formatter.format_tech_level(pop_data.get_highest_tech_level()))


## Sorts composition dictionary by fraction descending.
## @param composition: The composition dictionary.
## @return: Sorted array of [gas, fraction] pairs.
func _sort_composition(composition: Dictionary) -> Array:
	var pairs: Array = []
	for key in composition.keys():
		pairs.append([key, composition[key]])
	pairs.sort_custom(func(a, b): return a[1] > b[1])
	return pairs
