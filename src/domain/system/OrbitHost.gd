## Represents a potential host for planetary orbits.
## Computed from hierarchy nodes with stability zones and physical properties.
class_name OrbitHost
extends RefCounted

const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")


## Type of orbit configuration.
enum HostType {
	S_TYPE,  ## Circumstellar (around single star)
	P_TYPE,  ## Circumbinary (around binary barycenter)
}


## Reference to the hierarchy node this host is derived from.
var node_id: String

## Type of orbit host.
var host_type: HostType

## Combined mass of all bodies in/below this node in kg.
var combined_mass_kg: float

## Combined luminosity of all stars in/below this node in watts.
var combined_luminosity_watts: float

## Combined effective temperature (luminosity-weighted average) in Kelvin.
var effective_temperature_k: float

## Inner edge of stable orbital zone in meters.
var inner_stability_m: float

## Outer edge of stable orbital zone in meters.
var outer_stability_m: float

## Inner edge of habitable zone in meters.
var habitable_zone_inner_m: float

## Outer edge of habitable zone in meters.
var habitable_zone_outer_m: float

## Frost line distance in meters.
var frost_line_m: float


## Creates a new OrbitHost.
## @param p_node_id: The hierarchy node ID.
## @param p_host_type: Type of orbit host.
func _init(
	p_node_id: String = "",
	p_host_type: HostType = HostType.S_TYPE
) -> void:
	node_id = p_node_id
	host_type = p_host_type
	combined_mass_kg = 0.0
	combined_luminosity_watts = 0.0
	effective_temperature_k = 0.0
	inner_stability_m = 0.0
	outer_stability_m = 0.0
	habitable_zone_inner_m = 0.0
	habitable_zone_outer_m = 0.0
	frost_line_m = 0.0


## Returns true if orbits are possible around this host.
## @return: True if stability zone has positive width.
func has_valid_zone() -> bool:
	return outer_stability_m > inner_stability_m and inner_stability_m > 0.0


## Returns the width of the stable orbital zone.
## @return: Zone width in meters.
func get_zone_width_m() -> float:
	return maxf(0.0, outer_stability_m - inner_stability_m)


## Returns the width of the stable zone in AU.
## @return: Zone width in AU.
func get_zone_width_au() -> float:
	return get_zone_width_m() / Units.AU_METERS


## Returns the host type as a string.
## @return: "S-type" or "P-type".
func get_type_string() -> String:
	match host_type:
		HostType.S_TYPE:
			return "S-type"
		HostType.P_TYPE:
			return "P-type"
		_:
			return "Unknown"


## Checks if a given orbital distance is within the stable zone.
## @param distance_m: Distance from host in meters.
## @return: True if within stable zone.
func is_distance_stable(distance_m: float) -> bool:
	return distance_m >= inner_stability_m and distance_m <= outer_stability_m


## Checks if a given orbital distance is within the habitable zone.
## @param distance_m: Distance from host in meters.
## @return: True if within habitable zone.
func is_distance_habitable(distance_m: float) -> bool:
	return distance_m >= habitable_zone_inner_m and distance_m <= habitable_zone_outer_m


## Checks if a given orbital distance is beyond the frost line.
## @param distance_m: Distance from host in meters.
## @return: True if beyond frost line.
func is_beyond_frost_line(distance_m: float) -> bool:
	return distance_m >= frost_line_m


## Calculates zones based on combined luminosity.
## Call this after setting combined_luminosity_watts.
func calculate_zones() -> void:
	if combined_luminosity_watts <= 0.0:
		habitable_zone_inner_m = 0.0
		habitable_zone_outer_m = 0.0
		frost_line_m = 0.0
		return
	
	var l_solar: float = combined_luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	var sqrt_l: float = sqrt(l_solar)
	
	# Habitable zone: 0.95 to 1.37 AU * sqrt(L)
	habitable_zone_inner_m = 0.95 * Units.AU_METERS * sqrt_l
	habitable_zone_outer_m = 1.37 * Units.AU_METERS * sqrt_l
	
	# Frost line: ~2.7 AU * sqrt(L)
	frost_line_m = 2.7 * Units.AU_METERS * sqrt_l


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"node_id": node_id,
		"host_type": "s_type" if host_type == HostType.S_TYPE else "p_type",
		"combined_mass_kg": combined_mass_kg,
		"combined_luminosity_watts": combined_luminosity_watts,
		"effective_temperature_k": effective_temperature_k,
		"inner_stability_m": inner_stability_m,
		"outer_stability_m": outer_stability_m,
		"habitable_zone_inner_m": habitable_zone_inner_m,
		"habitable_zone_outer_m": habitable_zone_outer_m,
		"frost_line_m": frost_line_m,
	}


## Creates an OrbitHost from a dictionary.
## @param data: Dictionary to parse.
## @return: A new OrbitHost.
static func from_dict(data: Dictionary) -> OrbitHost:
	var type_str: String = data.get("host_type", "s_type") as String
	var p_host_type: HostType = HostType.S_TYPE if type_str == "s_type" else HostType.P_TYPE
	
	var host: OrbitHost = OrbitHost.new(
		data.get("node_id", "") as String,
		p_host_type
	)
	host.combined_mass_kg = data.get("combined_mass_kg", 0.0) as float
	host.combined_luminosity_watts = data.get("combined_luminosity_watts", 0.0) as float
	host.effective_temperature_k = data.get("effective_temperature_k", 0.0) as float
	host.inner_stability_m = data.get("inner_stability_m", 0.0) as float
	host.outer_stability_m = data.get("outer_stability_m", 0.0) as float
	host.habitable_zone_inner_m = data.get("habitable_zone_inner_m", 0.0) as float
	host.habitable_zone_outer_m = data.get("habitable_zone_outer_m", 0.0) as float
	host.frost_line_m = data.get("frost_line_m", 0.0) as float
	
	return host
