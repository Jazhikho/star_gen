## Derives ring system shader uniforms from RingSystemProps.
## Used by MaterialFactory and by TerrestrialShaderParams/GasGiantShaderParams when body has rings.
class_name RingShaderParams
extends RefCounted


## Returns shader parameters for ring system rendering.
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
	for i in range(band_count):
		var band: RingBand = ring_system.get_band(i)
		inner_radius = minf(inner_radius, band.inner_radius_m)
		outer_radius = maxf(outer_radius, band.outer_radius_m)

	params["u_ringInner"] = inner_radius / planet_radius_m
	params["u_ringOuter"] = outer_radius / planet_radius_m
	params["u_ringBands"] = band_count

	var avg_composition: Dictionary = {}
	for i in range(band_count):
		var band: RingBand = ring_system.get_band(i)
		for key in band.composition:
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
	for i in range(band_count):
		var band: RingBand = ring_system.get_band(i)
		band_width_sum += band.outer_radius_m - band.inner_radius_m
	var gap_fraction: float = 1.0 - (band_width_sum / maxf(total_ring_width, 1.0))
	params["u_ringGap"] = clampf(gap_fraction, 0.05, 0.4)

	var incl_rad: float = deg_to_rad(ring_system.inclination_deg)
	params["u_ringNormal"] = Vector3(sin(incl_rad), cos(incl_rad), 0.12)

	return params
