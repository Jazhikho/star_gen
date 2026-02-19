## Atmospheric properties of a celestial body.
## Optional component for bodies with atmospheres.
class_name AtmosphereProps
extends RefCounted


## Surface pressure in Pascals.
var surface_pressure_pa: float

## Atmospheric scale height in meters.
var scale_height_m: float

## Composition as gas name -> fraction (should sum to ~1.0).
var composition: Dictionary

## Greenhouse warming factor (1.0 = no effect, >1.0 = warming).
var greenhouse_factor: float


## Creates a new AtmosphereProps instance.
## @param p_surface_pressure_pa: Surface pressure in Pascals.
## @param p_scale_height_m: Scale height in meters.
## @param p_composition: Gas composition dictionary.
## @param p_greenhouse_factor: Greenhouse warming factor.
func _init(
	p_surface_pressure_pa: float = 0.0,
	p_scale_height_m: float = 0.0,
	p_composition: Dictionary = {},
	p_greenhouse_factor: float = 1.0
) -> void:
	surface_pressure_pa = p_surface_pressure_pa
	scale_height_m = p_scale_height_m
	composition = p_composition.duplicate()
	greenhouse_factor = p_greenhouse_factor


## Calculates the sum of all composition fractions.
## @return: Sum of composition fractions.
func get_composition_sum() -> float:
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	return total


## Returns the dominant gas in the atmosphere.
## @return: Name of the gas with highest fraction, or empty string.
func get_dominant_gas() -> String:
	var max_fraction: float = 0.0
	var dominant: String = ""
	for gas in composition:
		var fraction: float = composition[gas] as float
		if fraction > max_fraction:
			max_fraction = fraction
			dominant = gas
	return dominant


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"surface_pressure_pa": surface_pressure_pa,
		"scale_height_m": scale_height_m,
		"composition": composition.duplicate(),
		"greenhouse_factor": greenhouse_factor,
	}


## Creates an AtmosphereProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new AtmosphereProps instance.
static func from_dict(data: Dictionary) -> AtmosphereProps:
	var script_class: GDScript = load("res://src/domain/celestial/components/AtmosphereProps.gd") as GDScript
	return script_class.new(
		data.get("surface_pressure_pa", 0.0) as float,
		data.get("scale_height_m", 0.0) as float,
		data.get("composition", {}) as Dictionary,
		data.get("greenhouse_factor", 1.0) as float
	) as AtmosphereProps
