## Derives ring system shader uniforms from RingSystemProps.
## Used by MaterialFactory and by TerrestrialShaderParams/GasGiantShaderParams when body has rings.
class_name RingShaderParams
extends RefCounted


## Returns shader parameters for the new ring_system spatial shader.
## @param ring_system: The RingSystemProps to derive parameters from.
## @param body: The parent body (optional, for seed and radius).
## @return: Dictionary of shader uniform values for ring_system.gdshader.
static func get_params(ring_system: RingSystemProps, body: CelestialBody = null) -> Dictionary:
	var params: Dictionary = {}

	if ring_system == null or ring_system.get_band_count() == 0:
		params["u_bandCount"] = 0
		params["u_innerRadius"] = 1.4
		params["u_outerRadius"] = 2.6
		params["u_density"] = 0.5
		params["u_gapSize"] = 0.15
		params["u_colorInner"] = Color(0.8, 0.6, 0.4)
		params["u_colorMid"] = Color(0.73, 0.67, 0.53)
		params["u_colorOuter"] = Color(0.53, 0.6, 0.67)
		params["u_lightDir"] = Vector3(0.7, 0.4, 0.6)
		params["u_ambient"] = 0.15
		params["u_seed"] = 0.0
		return params

	# Seed from body provenance or default
	var seed_value: float = 0.0
	if body and body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0
	params["u_seed"] = seed_value

	# === Ring structure ===
	params["u_bandCount"] = ring_system.get_band_count()

	# Calculate inner/outer radius from bands
	var inner_r: float = INF
	var outer_r: float = 0.0
	var body_radius: float = 1.0

	if body and body.physical and body.physical.radius_m > 0:
		body_radius = body.physical.radius_m

	for i: int in range(ring_system.get_band_count()):
		var band: RingBand = ring_system.get_band(i)
		var band_inner: float = band.inner_radius_m / body_radius
		var band_outer: float = band.outer_radius_m / body_radius
		inner_r = minf(inner_r, band_inner)
		outer_r = maxf(outer_r, band_outer)

	# Clamp to reasonable visual range
	if inner_r == INF:
		inner_r = 1.4
	if outer_r < inner_r:
		outer_r = inner_r + 0.5
	params["u_innerRadius"] = clampf(inner_r, 1.1, 2.0)
	params["u_outerRadius"] = clampf(outer_r, params["u_innerRadius"] as float + 0.2, 4.0)

	# === Density and gaps ===
	params["u_density"] = _calculate_average_density(ring_system)
	params["u_gapSize"] = _calculate_gap_size(ring_system)

	# === Colors from composition ===
	var colors: Dictionary = _calculate_ring_colors(ring_system)
	params["u_colorInner"] = colors["inner"]
	params["u_colorMid"] = colors["mid"]
	params["u_colorOuter"] = colors["outer"]

	# === Lighting defaults ===
	params["u_lightDir"] = Vector3(0.7, 0.4, 0.6)
	params["u_ambient"] = 0.15

	return params


## Returns parameters for a single ring band (for multi-mesh rendering).
## @param band: The specific ring band.
## @param body: The parent body (optional).
## @return: Dictionary of shader uniform values for this band.
static func get_band_params(band: RingBand, body: CelestialBody = null) -> Dictionary:
	var params: Dictionary = {}

	var seed_value: float = 0.0
	if body and body.provenance:
		seed_value = float(body.provenance.generation_seed % 1000) / 10.0
	params["u_seed"] = seed_value

	var body_radius: float = 1.0
	if body and body.physical and body.physical.radius_m > 0:
		body_radius = body.physical.radius_m

	params["u_bandCount"] = 1
	params["u_innerRadius"] = clampf(band.inner_radius_m / body_radius, 1.1, 3.0)
	params["u_outerRadius"] = clampf(band.outer_radius_m / body_radius, 1.2, 4.0)
	params["u_density"] = clampf(band.optical_depth, 0.1, 1.0)
	params["u_gapSize"] = 0.05 # Minimal gaps within single band

	# Color from this band's composition
	var ring_color: Color = ColorUtils.ring_to_color(band.composition, band.optical_depth)
	params["u_colorInner"] = ring_color
	params["u_colorMid"] = ring_color
	params["u_colorOuter"] = ring_color

	params["u_lightDir"] = Vector3(0.7, 0.4, 0.6)
	params["u_ambient"] = 0.15

	return params


## Calculates average optical density across all ring bands.
## @param ring_system: The ring system properties.
## @return: Average density (0.1 to 1.0).
static func _calculate_average_density(ring_system: RingSystemProps) -> float:
	if ring_system.get_band_count() == 0:
		return 0.5

	var total_depth: float = 0.0
	var total_width: float = 0.0

	for i: int in range(ring_system.get_band_count()):
		var band: RingBand = ring_system.get_band(i)
		var width: float = band.outer_radius_m - band.inner_radius_m
		total_depth += band.optical_depth * width
		total_width += width

	if total_width <= 0:
		return 0.5

	return clampf(total_depth / total_width, 0.1, 1.0)


## Calculates gap size as fraction of total ring span.
## @param ring_system: The ring system properties.
## @return: Gap fraction (0.05 to 0.5).
static func _calculate_gap_size(ring_system: RingSystemProps) -> float:
	if ring_system.get_band_count() < 2:
		return 0.1

	var total_span: float = ring_system.get_outer_radius_m() - ring_system.get_inner_radius_m()
	var band_coverage: float = 0.0

	for i: int in range(ring_system.get_band_count()):
		var band: RingBand = ring_system.get_band(i)
		band_coverage += band.outer_radius_m - band.inner_radius_m

	if total_span <= 0:
		return 0.15

	var gap_fraction: float = 1.0 - (band_coverage / total_span)
	return clampf(gap_fraction, 0.05, 0.5)


## Calculates ring colors from composition of all bands.
## @param ring_system: The ring system properties.
## @return: Dictionary with inner, mid, outer colors.
static func _calculate_ring_colors(ring_system: RingSystemProps) -> Dictionary:
	# Default Saturn-like colors
	var inner: Color = Color(0.8, 0.6, 0.4)
	var mid: Color = Color(0.73, 0.67, 0.53)
	var outer: Color = Color(0.53, 0.6, 0.67)

	if ring_system.get_band_count() == 0:
		return {"inner": inner, "mid": mid, "outer": outer}

	# Analyze composition of bands
	var ice_content: float = 0.0
	var rock_content: float = 0.0
	var iron_content: float = 0.0
	var carbon_content: float = 0.0
	var band_count: float = float(ring_system.get_band_count())

	for i: int in range(ring_system.get_band_count()):
		var band: RingBand = ring_system.get_band(i)
		var comp: Dictionary = band.composition
		ice_content += (comp.get("water_ice", 0.0) as float)
		ice_content += (comp.get("ice", 0.0) as float)
		rock_content += (comp.get("silicates", 0.0) as float)
		rock_content += (comp.get("rock", 0.0) as float)
		iron_content += (comp.get("iron", 0.0) as float)
		iron_content += (comp.get("iron_oxides", 0.0) as float)
		carbon_content += (comp.get("carbon", 0.0) as float)
		carbon_content += (comp.get("carbon_compounds", 0.0) as float)

	ice_content /= band_count
	rock_content /= band_count
	iron_content /= band_count
	carbon_content /= band_count

	# Ice-rich rings (Saturn-like): bright, slightly blue
	if ice_content > 0.5:
		inner = Color(0.85, 0.8, 0.75)
		mid = Color(0.9, 0.88, 0.85)
		outer = Color(0.8, 0.85, 0.9)
	# Rocky rings: darker, brownish
	elif rock_content > 0.5:
		inner = Color(0.5, 0.45, 0.4)
		mid = Color(0.55, 0.5, 0.45)
		outer = Color(0.45, 0.45, 0.5)
	# Iron-rich: reddish tint
	elif iron_content > 0.2:
		inner = Color(0.7, 0.5, 0.4)
		mid = Color(0.65, 0.55, 0.5)
		outer = Color(0.55, 0.5, 0.55)
	# Carbon-rich: very dark
	elif carbon_content > 0.3:
		inner = Color(0.25, 0.23, 0.2)
		mid = Color(0.3, 0.28, 0.25)
		outer = Color(0.28, 0.28, 0.3)

	return {"inner": inner, "mid": mid, "outer": outer}


## Returns shader parameters for ring system rendering (legacy - for gas giant/terrestrial inline rings).
## @param ring_system: The RingSystemProps.
## @param planet_radius_m: Planet's radius in meters.
## @return: Dictionary of shader uniform values.
static func get_ring_shader_params(ring_system: RingSystemProps, planet_radius_m: float) -> Dictionary:
	var params: Dictionary = {}
	if ring_system == null or ring_system.get_band_count() == 0:
		params["u_ringType"] = 0
		return params

	var band_count: int = ring_system.get_band_count()
	if band_count <= 1:
		params["u_ringType"] = 1
	elif band_count <= 3:
		params["u_ringType"] = 2
	else:
		params["u_ringType"] = 3

	var inner_radius: float = INF
	var outer_radius: float = 0.0
	for i: int in range(band_count):
		var band: RingBand = ring_system.get_band(i)
		inner_radius = minf(inner_radius, band.inner_radius_m)
		outer_radius = maxf(outer_radius, band.outer_radius_m)

	params["u_ringInner"] = inner_radius / planet_radius_m
	params["u_ringOuter"] = outer_radius / planet_radius_m
	params["u_ringBands"] = band_count

	var avg_composition: Dictionary = {}
	for i in range(band_count):
		var band: RingBand = ring_system.get_band(i)
		for key: String in band.composition:
			var existing: float = avg_composition.get(key, 0.0) as float
			avg_composition[key] = existing + (band.composition[key] as float) / float(band_count)

	var ring_color: Color = ColorUtils.ring_to_color(avg_composition, 0.5)
	params["u_ringColor1"] = Vector3(ring_color.r * 1.1, ring_color.g * 1.1, ring_color.b * 1.0)
	params["u_ringColor2"] = Vector3(ring_color.r, ring_color.g, ring_color.b)
	params["u_ringColor3"] = Vector3(ring_color.r * 0.9, ring_color.g * 0.95, ring_color.b * 1.1)

	var avg_optical_depth: float = 0.0
	for i in range(band_count):
		var band: RingBand = ring_system.get_band(i)
		avg_optical_depth += band.optical_depth / float(band_count)
	params["u_ringDensity"] = clampf(avg_optical_depth * 0.8, 0.15, 0.95)

	var total_ring_width: float = outer_radius - inner_radius
	var band_width_sum: float = 0.0
	for i: int in range(band_count):
		var band: RingBand = ring_system.get_band(i)
		band_width_sum += band.outer_radius_m - band.inner_radius_m
	var gap_fraction: float = 1.0 - (band_width_sum / maxf(total_ring_width, 1.0))
	params["u_ringGap"] = clampf(gap_fraction, 0.05, 0.4)

	var incl_rad: float = deg_to_rad(ring_system.inclination_deg)
	params["u_ringNormal"] = Vector3(sin(incl_rad), cos(incl_rad), 0.12)

	return params
