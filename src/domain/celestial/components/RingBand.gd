## A single band within a ring system.
## Ring systems are composed of multiple bands with gaps between them.
class_name RingBand
extends RefCounted


## Inner radius of this band in meters.
var inner_radius_m: float

## Outer radius of this band in meters.
var outer_radius_m: float

## Optical depth (0 = transparent, >1 = opaque).
var optical_depth: float

## Composition as material name -> mass fraction.
var composition: Dictionary

## Median particle size in meters.
var particle_size_m: float

## Optional name/identifier for this band.
var name: String


## Creates a new RingBand instance.
## @param p_inner_radius_m: Inner radius in meters.
## @param p_outer_radius_m: Outer radius in meters.
## @param p_optical_depth: Optical depth.
## @param p_composition: Material composition dictionary.
## @param p_particle_size_m: Median particle size in meters.
## @param p_name: Optional band name.
func _init(
	p_inner_radius_m: float = 0.0,
	p_outer_radius_m: float = 0.0,
	p_optical_depth: float = 0.0,
	p_composition: Dictionary = {},
	p_particle_size_m: float = 1.0,
	p_name: String = ""
) -> void:
	inner_radius_m = p_inner_radius_m
	outer_radius_m = p_outer_radius_m
	optical_depth = p_optical_depth
	composition = p_composition.duplicate()
	particle_size_m = p_particle_size_m
	name = p_name


## Calculates the width of this band in meters.
## @return: Band width in meters.
func get_width_m() -> float:
	return outer_radius_m - inner_radius_m


## Returns the dominant material in the composition.
## @return: Material name with highest fraction, or empty string.
func get_dominant_material() -> String:
	var max_fraction: float = 0.0
	var dominant: String = ""
	for material in composition:
		var fraction: float = composition[material] as float
		if fraction > max_fraction:
			max_fraction = fraction
			dominant = material
	return dominant


## Converts this band to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"inner_radius_m": inner_radius_m,
		"outer_radius_m": outer_radius_m,
		"optical_depth": optical_depth,
		"composition": composition.duplicate(),
		"particle_size_m": particle_size_m,
		"name": name,
	}


## Creates a RingBand from a dictionary.
## @param data: The dictionary to parse.
## @return: A new RingBand instance.
static func from_dict(data: Dictionary) -> RingBand:
	var script: GDScript = load("res://src/domain/celestial/components/RingBand.gd") as GDScript
	return script.new(
		data.get("inner_radius_m", 0.0) as float,
		data.get("outer_radius_m", 0.0) as float,
		data.get("optical_depth", 0.0) as float,
		data.get("composition", {}) as Dictionary,
		data.get("particle_size_m", 1.0) as float,
		data.get("name", "") as String
	) as RingBand
