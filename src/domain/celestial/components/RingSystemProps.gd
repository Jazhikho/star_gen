## Ring system properties containing multiple bands.
## Models planetary ring systems like Saturn's with gaps and varying densities.
class_name RingSystemProps
extends RefCounted


## Array of ring bands from inner to outer.
var bands: Array[RingBand]

## Total mass of the ring system in kilograms.
var total_mass_kg: float

## Inclination of ring plane relative to equator in degrees.
var inclination_deg: float


## Creates a new RingSystemProps instance.
## @param p_bands: Array of ring bands.
## @param p_total_mass_kg: Total mass in kilograms.
## @param p_inclination_deg: Ring plane inclination in degrees.
func _init(
	p_bands: Array[RingBand] = [],
	p_total_mass_kg: float = 0.0,
	p_inclination_deg: float = 0.0
) -> void:
	bands = p_bands.duplicate()
	total_mass_kg = p_total_mass_kg
	inclination_deg = p_inclination_deg


## Returns the innermost radius of the ring system.
## @return: Inner radius in meters, or 0 if no bands.
func get_inner_radius_m() -> float:
	if bands.is_empty():
		return 0.0
	var min_radius: float = bands[0].inner_radius_m
	for band in bands:
		if band.inner_radius_m < min_radius:
			min_radius = band.inner_radius_m
	return min_radius


## Returns the outermost radius of the ring system.
## @return: Outer radius in meters, or 0 if no bands.
func get_outer_radius_m() -> float:
	if bands.is_empty():
		return 0.0
	var max_radius: float = bands[0].outer_radius_m
	for band in bands:
		if band.outer_radius_m > max_radius:
			max_radius = band.outer_radius_m
	return max_radius


## Returns the total width of the ring system.
## @return: Width in meters.
func get_total_width_m() -> float:
	return get_outer_radius_m() - get_inner_radius_m()


## Returns the number of bands in the system.
## @return: Band count.
func get_band_count() -> int:
	return bands.size()


## Adds a band to the system.
## @param band: The band to add.
func add_band(band: RingBand) -> void:
	bands.append(band)


## Gets a band by index.
## @param index: The band index.
## @return: The band, or null if index out of range.
func get_band(index: int) -> RingBand:
	if index < 0 or index >= bands.size():
		return null
	return bands[index]


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var bands_array: Array = []
	for band in bands:
		bands_array.append(band.to_dict())
	
	return {
		"bands": bands_array,
		"total_mass_kg": total_mass_kg,
		"inclination_deg": inclination_deg,
	}


## Creates a RingSystemProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new RingSystemProps instance.
static func from_dict(data: Dictionary) -> RingSystemProps:
	var bands_data: Array = data.get("bands", []) as Array
	var parsed_bands: Array[RingBand] = []
	for band_data in bands_data:
		parsed_bands.append(RingBand.from_dict(band_data as Dictionary))
	
	var script_class: GDScript = load("res://src/domain/celestial/components/RingSystemProps.gd") as GDScript
	return script_class.new(
		parsed_bands,
		data.get("total_mass_kg", 0.0) as float,
		data.get("inclination_deg", 0.0) as float
	) as RingSystemProps
