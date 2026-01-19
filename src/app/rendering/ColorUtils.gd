## Utility functions for color calculations.
## Handles blackbody radiation, composition-based colors, etc.
class_name ColorUtils
extends RefCounted


## Calculates blackbody color from temperature using Planck's law approximation.
## Based on algorithm by Tanner Helland.
## @param temperature_k: Temperature in Kelvin.
## @return: RGB color for the blackbody radiation.
static func temperature_to_blackbody_color(temperature_k: float) -> Color:
	# Clamp to reasonable stellar temperature range
	var temp: float = clampf(temperature_k, 1000.0, 40000.0)
	
	# Scale temperature to useful range (divide by 100)
	temp = temp / 100.0
	
	var red: float
	var green: float
	var blue: float
	
	# Calculate red
	if temp <= 66.0:
		red = 255.0
	else:
		red = temp - 60.0
		red = 329.698727446 * pow(red, -0.1332047592)
		red = clampf(red, 0.0, 255.0)
	
	# Calculate green
	if temp <= 66.0:
		green = temp
		green = 99.4708025861 * log(green) - 161.1195681661
		green = clampf(green, 0.0, 255.0)
	else:
		green = temp - 60.0
		green = 288.1221695283 * pow(green, -0.0755148492)
		green = clampf(green, 0.0, 255.0)
	
	# Calculate blue
	if temp >= 66.0:
		blue = 255.0
	elif temp <= 19.0:
		blue = 0.0
	else:
		blue = temp - 10.0
		blue = 138.5177312231 * log(blue) - 305.0447927307
		blue = clampf(blue, 0.0, 255.0)
	
	return Color(red / 255.0, green / 255.0, blue / 255.0)


## Gets a color representing the star's spectral class.
## @param spectral_type: The spectral type string (e.g., "G2V").
## @return: Representative color for the spectral class.
static func spectral_class_to_color(spectral_type: String) -> Color:
	if spectral_type.is_empty():
		return Color.WHITE
	
	var first_char: String = spectral_type[0].to_upper()
	
	match first_char:
		"O":
			return Color(0.6, 0.7, 1.0)      # Blue
		"B":
			return Color(0.7, 0.8, 1.0)      # Blue-white
		"A":
			return Color(0.9, 0.9, 1.0)      # White
		"F":
			return Color(1.0, 1.0, 0.9)      # Yellow-white
		"G":
			return Color(1.0, 1.0, 0.8)      # Yellow
		"K":
			return Color(1.0, 0.85, 0.6)     # Orange
		"M":
			return Color(1.0, 0.6, 0.4)      # Red
		_:
			return Color.WHITE


## Calculates atmosphere/sky color from composition.
## @param composition: Dictionary of gas -> fraction.
## @return: Sky color based on dominant gases.
static func atmosphere_to_sky_color(composition: Dictionary) -> Color:
	if composition.is_empty():
		return Color(0.5, 0.6, 0.8)  # Default blue-ish
	
	# Base color starts neutral
	var color: Color = Color(0.0, 0.0, 0.0)
	var total_weight: float = 0.0
	
	# Gas colors based on Rayleigh scattering and absorption
	var gas_colors: Dictionary = {
		"N2": Color(0.4, 0.5, 0.9),      # Blue (Earth-like)
		"O2": Color(0.5, 0.6, 0.9),      # Slight blue
		"CO2": Color(0.9, 0.7, 0.5),     # Orange-ish (Venus/Mars)
		"CH4": Color(0.4, 0.6, 0.8),     # Cyan-blue (Titan, Uranus)
		"H2": Color(0.7, 0.7, 0.8),      # Pale
		"He": Color(0.8, 0.8, 0.8),      # Very pale
		"NH3": Color(0.8, 0.7, 0.6),     # Tan
		"H2O": Color(0.6, 0.7, 0.9),     # Blue
		"SO2": Color(0.9, 0.8, 0.5),     # Yellow
		"Ar": Color(0.5, 0.5, 0.6),      # Gray-blue
	}
	
	for gas in composition.keys():
		var fraction: float = composition[gas] as float
		if fraction < 0.01:
			continue
		
		var gas_color: Color = gas_colors.get(gas, Color(0.5, 0.5, 0.5))
		color += gas_color * fraction
		total_weight += fraction
	
	if total_weight > 0.0:
		color = color / total_weight
	else:
		color = Color(0.5, 0.6, 0.8)
	
	return color


## Gets surface color from surface type and composition.
## @param surface_type: The surface type string.
## @param composition: Surface material composition.
## @param albedo: Surface albedo.
## @return: Base surface color.
static func surface_to_color(surface_type: String, composition: Dictionary, albedo: float) -> Color:
	var base_color: Color
	
	match surface_type.to_lower():
		"molten":
			base_color = Color(1.0, 0.3, 0.1)  # Bright orange-red
		"volcanic":
			base_color = Color(0.3, 0.25, 0.2)  # Dark with hints of red
		"frozen", "icy", "icy_smooth":
			base_color = Color(0.85, 0.9, 0.95)  # Ice white-blue
		"icy_cratered", "icy_rocky":
			base_color = Color(0.7, 0.75, 0.8)  # Dirty ice
		"rocky", "rocky_cold":
			base_color = Color(0.5, 0.45, 0.4)  # Gray-brown
		"cratered":
			base_color = Color(0.4, 0.4, 0.4)  # Gray
		"oceanic":
			base_color = Color(0.2, 0.4, 0.6)  # Ocean blue
		"continental":
			base_color = Color(0.4, 0.5, 0.3)  # Green-brown
		"desert":
			base_color = Color(0.8, 0.7, 0.5)  # Sand
		_:
			base_color = Color(0.5, 0.5, 0.5)  # Default gray
	
	# Modify by albedo (higher albedo = lighter)
	base_color = base_color.lerp(Color.WHITE, albedo * 0.3)
	
	# Modify by composition if available
	if not composition.is_empty():
		base_color = _modify_color_by_composition(base_color, composition)
	
	return base_color


## Modifies a color based on surface composition.
static func _modify_color_by_composition(base_color: Color, composition: Dictionary) -> Color:
	var color: Color = base_color
	
	# Iron oxides make things more red/orange
	if composition.has("iron_oxides"):
		var amount: float = composition["iron_oxides"] as float
		color = color.lerp(Color(0.7, 0.3, 0.2), amount * 0.5)
	
	# Water ice makes things whiter/bluer
	if composition.has("water_ice"):
		var amount: float = composition["water_ice"] as float
		color = color.lerp(Color(0.9, 0.95, 1.0), amount * 0.4)
	
	# Silicates are gray
	if composition.has("silicates"):
		var amount: float = composition["silicates"] as float
		color = color.lerp(Color(0.5, 0.5, 0.5), amount * 0.2)
	
	# Carbon compounds are dark
	if composition.has("carbon_compounds"):
		var amount: float = composition["carbon_compounds"] as float
		color = color.lerp(Color(0.2, 0.2, 0.2), amount * 0.3)
	
	return color


## Gets asteroid color from type.
## @param surface_type: The asteroid surface type (carbonaceous, silicaceous, metallic).
## @param composition: Surface composition.
## @return: Asteroid color.
static func asteroid_to_color(surface_type: String, composition: Dictionary) -> Color:
	var base_color: Color
	
	match surface_type.to_lower():
		"carbonaceous":
			base_color = Color(0.15, 0.12, 0.1)  # Very dark
		"silicaceous":
			base_color = Color(0.5, 0.45, 0.4)   # Gray-brown
		"metallic":
			base_color = Color(0.6, 0.6, 0.55)   # Metallic gray
		_:
			base_color = Color(0.4, 0.4, 0.4)
	
	# Modify by composition
	if not composition.is_empty():
		if composition.has("iron"):
			var amount: float = composition.get("iron", 0.0) as float
			base_color = base_color.lerp(Color(0.55, 0.5, 0.45), amount * 0.3)
		if composition.has("nickel"):
			var amount: float = composition.get("nickel", 0.0) as float
			base_color = base_color.lerp(Color(0.6, 0.58, 0.5), amount * 0.2)
	
	return base_color


## Gets ring color from composition.
## @param composition: Ring material composition.
## @param optical_depth: Optical depth of the ring.
## @return: Ring color with appropriate alpha.
static func ring_to_color(composition: Dictionary, optical_depth: float) -> Color:
	var base_color: Color = Color(0.8, 0.8, 0.8)  # Default gray
	
	if composition.has("water_ice"):
		var amount: float = composition["water_ice"] as float
		base_color = base_color.lerp(Color(0.95, 0.97, 1.0), amount)
	
	if composition.has("silicates"):
		var amount: float = composition["silicates"] as float
		base_color = base_color.lerp(Color(0.6, 0.55, 0.5), amount)
	
	if composition.has("iron_oxides"):
		var amount: float = composition["iron_oxides"] as float
		base_color = base_color.lerp(Color(0.7, 0.5, 0.4), amount * 0.5)
	
	# Alpha based on optical depth
	var alpha: float = clampf(optical_depth, 0.1, 0.95)
	base_color.a = alpha
	
	return base_color
