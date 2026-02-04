## Unit tests for SystemInspectorPanel.
## Tests system display, body selection display, generation controls, and signals.
extends TestCase

const _system_inspector_panel: GDScript = preload("res://src/app/system_viewer/SystemInspectorPanel.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _atmosphere_props: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Helper to create a panel in the scene tree.
## @return: SystemInspectorPanel.
func _create_panel() -> SystemInspectorPanel:
	var panel: SystemInspectorPanel = SystemInspectorPanel.new()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	if scene_tree and scene_tree.root:
		scene_tree.root.call_deferred("add_child", panel)
	return panel


## Helper to create a minimal solar system.
## @return: SolarSystem with one star and one planet.
func _make_system() -> SolarSystem:
	var system: SolarSystem = SolarSystem.new("sys_1", "Test System")
	
	# Add a star
	var star_phys: PhysicalProps = PhysicalProps.new(
		Units.SOLAR_MASS_KG, Units.SOLAR_RADIUS_METERS,
		2.16e6, 7.25, 0.0, 0.0, 0.0
	)
	var star: CelestialBody = CelestialBody.new(
		"star_1", "Sol", CelestialType.Type.STAR, star_phys, null
	)
	star.stellar = StellarProps.new(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9)
	system.add_body(star)
	
	# Add a planet
	var planet_phys: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG, Units.EARTH_RADIUS_METERS,
		86400.0, 23.5, 0.003, 8.0e22, 4.7e13
	)
	var planet_orb: OrbitalProps = OrbitalProps.new(
		Units.AU_METERS, 0.017, 0.0, 0.0, 0.0, 45.0, "star_1"
	)
	var planet: CelestialBody = CelestialBody.new(
		"planet_1", "Earth", CelestialType.Type.PLANET, planet_phys, null
	)
	system.add_body(planet)
	
	# Add orbit host
	var host: OrbitHost = OrbitHost.new("star_1", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = Units.SOLAR_MASS_KG
	host.combined_luminosity_watts = 3.828e26
	host.inner_stability_m = 0.1 * Units.AU_METERS
	host.outer_stability_m = 50.0 * Units.AU_METERS
	system.add_orbit_host(host)
	
	return system


## Helper to create a planet body for selection display.
## @return: Planet CelestialBody with all common properties.
func _make_planet() -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		5.972e24, 6.371e6, 86400.0, 23.5, 0.003, 8.0e22, 4.7e13
	)
	var orbital: OrbitalProps = OrbitalProps.new(
		1.496e11, 0.017, 0.0, 0.0, 0.0, 45.0, "star_1"
	)
	var body: CelestialBody = CelestialBody.new(
		"planet_1", "Earth", CelestialType.Type.PLANET, physical, null
	)
	body.atmosphere = AtmosphereProps.new(
		101325.0,
		8500.0,
		{"N2": 0.78, "O2": 0.21, "Ar": 0.01},
		1.0
	)
	return body


# =============================================================================
# INITIALIZATION TESTS
# =============================================================================


## Tests panel creates UI structure on ready.
func test_panel_creates_ui() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# Should have children (separators, overview, body sections)
	assert_true(panel.get_child_count() > 0,
		"Panel should have children after _ready (got %d)" % panel.get_child_count())
	
	panel.queue_free()


# =============================================================================
# SYSTEM DISPLAY TESTS
# =============================================================================


## Tests display_system shows star count.
func test_display_system_star_count() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var system: SolarSystem = _make_system()
	panel.display_system(system)
	
	# Verify by checking that panel has content
	# The overview section should have properties
	assert_true(_section_contains_text(panel, "Stars"),
		"Should display star count label")
	assert_true(_section_contains_text(panel, "1"),
		"Should display '1' for star count")
	
	panel.queue_free()


## Tests display_system shows planet count.
func test_display_system_planet_count() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var system: SolarSystem = _make_system()
	panel.display_system(system)
	
	assert_true(_section_contains_text(panel, "Planets"),
		"Should display planet count label")
	
	panel.queue_free()


## Tests display_system with null system.
func test_display_null_system() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	panel.display_system(null)
	
	assert_true(_section_contains_text(panel, "No system"),
		"Null system should show 'No system' message")
	
	panel.queue_free()


## Tests display_system shows system name.
func test_display_system_name() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var system: SolarSystem = _make_system()
	panel.display_system(system)
	
	assert_true(_section_contains_text(panel, "Test System"),
		"Should display system name")
	
	panel.queue_free()


## Tests display_system shows orbit host info.
func test_display_system_orbit_hosts() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var system: SolarSystem = _make_system()
	panel.display_system(system)
	
	assert_true(_section_contains_text(panel, "Orbit Hosts"),
		"Should display orbit hosts section")
	assert_true(_section_contains_text(panel, "S-type"),
		"Should display host type")
	
	panel.queue_free()


# =============================================================================
# SELECTED BODY DISPLAY TESTS
# =============================================================================


## Tests display_selected_body shows body name.
func test_display_selected_body_name() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var planet: CelestialBody = _make_planet()
	panel.display_selected_body(planet)
	
	assert_true(_section_contains_text(panel, "Earth"),
		"Should display body name")
	
	panel.queue_free()


## Tests display_selected_body shows body type.
func test_display_selected_body_type() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var planet: CelestialBody = _make_planet()
	panel.display_selected_body(planet)
	
	assert_true(_section_contains_text(panel, "Planet"),
		"Should display body type")
	
	panel.queue_free()


## Tests display_selected_body shows physical properties.
func test_display_selected_body_physical() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var planet: CelestialBody = _make_planet()
	panel.display_selected_body(planet)
	
	assert_true(_section_contains_text(panel, "Physical"),
		"Should display Physical section header")
	assert_true(_section_contains_text(panel, "Mass"),
		"Should display mass property")
	assert_true(_section_contains_text(panel, "Radius"),
		"Should display radius property")
	
	panel.queue_free()


## Tests display_selected_body shows orbital properties.
func test_display_selected_body_orbital() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var planet: CelestialBody = _make_planet()
	panel.display_selected_body(planet)
	
	assert_true(_section_contains_text(panel, "Orbital"),
		"Should display Orbital section header")
	assert_true(_section_contains_text(panel, "Semi-major"),
		"Should display semi-major axis")
	assert_true(_section_contains_text(panel, "Eccentricity"),
		"Should display eccentricity")
	
	panel.queue_free()


## Tests display_selected_body shows atmosphere properties.
func test_display_selected_body_atmosphere() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var planet: CelestialBody = _make_planet()
	panel.display_selected_body(planet)
	
	assert_true(_section_contains_text(panel, "Atmosphere"),
		"Should display Atmosphere section header")
	assert_true(_section_contains_text(panel, "Pressure"),
		"Should display pressure")
	
	panel.queue_free()


## Tests display_selected_body with null clears display.
func test_display_null_body() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# First show a body
	panel.display_selected_body(_make_planet())
	
	# Then clear it
	panel.display_selected_body(null)
	
	assert_true(_section_contains_text(panel, "Click a body"),
		"Null body should show 'Click a body to inspect' message")
	
	panel.queue_free()


## Tests display_selected_body shows star stellar properties.
func test_display_star_stellar_section() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# Create a star
	var star_phys: PhysicalProps = PhysicalProps.new(
		Units.SOLAR_MASS_KG, Units.SOLAR_RADIUS_METERS,
		2.16e6, 7.25, 0.0, 0.0, 0.0
	)
	var star: CelestialBody = CelestialBody.new(
		"star_1", "Sol", CelestialType.Type.STAR, star_phys, null
	)
	star.stellar = StellarProps.new(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9)
	
	panel.display_selected_body(star)
	
	assert_true(_section_contains_text(panel, "Stellar"),
		"Should display Stellar section for stars")
	assert_true(_section_contains_text(panel, "G2V"),
		"Should display spectral class")
	
	panel.queue_free()


## Tests display shows open in viewer button.
func test_display_body_shows_open_button() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	panel.display_selected_body(_make_planet())
	
	assert_true(_has_button_with_text(panel, "Open in Object Viewer"),
		"Should show 'Open in Object Viewer' button")
	
	panel.queue_free()


# =============================================================================
# SIGNAL TESTS
# =============================================================================


## Tests open_in_viewer_requested signal emitted on button press.
func test_open_viewer_signal() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame

	var planet: CelestialBody = _make_planet()
	panel.display_selected_body(planet)

	# Use array for mutable capture in lambda
	var result: Array = [false, null]

	panel.open_in_viewer_requested.connect(func(body: CelestialBody) -> void:
		result[0] = true
		result[1] = body
	)

	# Simulate pressing the open viewer button
	if panel._open_viewer_button != null:
		panel._open_viewer_button.emit_signal("pressed")

	assert_true(result[0], "Should receive open_in_viewer_requested signal")
	assert_equal(result[1], planet, "Should receive the correct body")

	panel.queue_free()


# =============================================================================
# CLEAR TESTS
# =============================================================================


## Tests clear resets all sections.
func test_clear() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# Display system and body
	panel.display_system(_make_system())
	panel.display_selected_body(_make_planet())
	
	# Clear
	panel.clear()
	
	assert_true(_section_contains_text(panel, "No system generated"),
		"Clear should show 'No system generated'")
	assert_true(_section_contains_text(panel, "Click a body"),
		"Clear should show 'Click a body to inspect'")
	
	panel.queue_free()


# =============================================================================
# UNIT FORMATTING TESTS
# =============================================================================


## Tests star mass uses solar mass units.
func test_star_mass_solar_units() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	var star_phys: PhysicalProps = PhysicalProps.new(
		Units.SOLAR_MASS_KG, Units.SOLAR_RADIUS_METERS,
		2.16e6, 7.25, 0.0, 0.0, 0.0
	)
	var star: CelestialBody = CelestialBody.new(
		"star_1", "Sol", CelestialType.Type.STAR, star_phys, null
	)
	star.stellar = StellarProps.new(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9)
	
	panel.display_selected_body(star)
	
	assert_true(_section_contains_text(panel, "M☉"),
		"Star mass should use solar mass units (M☉)")
	
	panel.queue_free()


## Tests planet mass uses Earth mass units.
func test_planet_mass_earth_units() -> void:
	var panel: SystemInspectorPanel = _create_panel()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	panel.display_selected_body(_make_planet())
	
	assert_true(_section_contains_text(panel, "M⊕"),
		"Planet mass should use Earth mass units (M⊕)")
	
	panel.queue_free()


# =============================================================================
# HELPER FUNCTIONS
# =============================================================================


## Searches all Label children recursively for text content.
## @param node: Root node to search.
## @param text: Text to search for.
## @return: True if any label contains the text.
func _section_contains_text(node: Node, text: String) -> bool:
	if node is Label:
		var label: Label = node as Label
		if label.text.contains(text):
			return true
	
	for child in node.get_children():
		if _section_contains_text(child, text):
			return true
	
	return false


## Searches for a Button with specific text.
## @param node: Root node to search.
## @param text: Button text to find.
## @return: True if button exists with that text.
func _has_button_with_text(node: Node, text: String) -> bool:
	if node is Button:
		var button: Button = node as Button
		if button.text == text:
			return true
	
	for child in node.get_children():
		if _has_button_with_text(child, text):
			return true
	
	return false
