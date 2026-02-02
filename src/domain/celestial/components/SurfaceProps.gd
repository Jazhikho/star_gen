## Surface properties of a celestial body.
## Applies to planets, moons, and asteroids.
class_name SurfaceProps
extends RefCounted

## Script reference for from_dict self-instantiation (avoids load() in domain).
const _SCRIPT: GDScript = preload("res://src/domain/celestial/components/SurfaceProps.gd")


## Surface temperature in Kelvin.
var temperature_k: float

## Bond albedo (0-1, fraction of incident light reflected).
var albedo: float

## Surface type identifier for rendering/classification.
var surface_type: String

## Volcanism activity level (0 = none, 1 = highly active).
var volcanism_level: float

## Surface material composition as material -> mass fraction.
var surface_composition: Dictionary

## Terrain properties (null for gas giants/stars).
var terrain: TerrainProps

## Hydrosphere properties (null if no liquid water).
var hydrosphere: HydrosphereProps

## Cryosphere properties (null if no significant ice).
var cryosphere: CryosphereProps


## Creates a new SurfaceProps instance.
## @param p_temperature_k: Surface temperature in Kelvin.
## @param p_albedo: Bond albedo (0-1).
## @param p_surface_type: Surface type identifier.
## @param p_volcanism_level: Volcanism activity level (0-1).
## @param p_surface_composition: Material composition dictionary.
func _init(
	p_temperature_k: float = 0.0,
	p_albedo: float = 0.0,
	p_surface_type: String = "",
	p_volcanism_level: float = 0.0,
	p_surface_composition: Dictionary = {}
) -> void:
	temperature_k = p_temperature_k
	albedo = p_albedo
	surface_type = p_surface_type
	volcanism_level = p_volcanism_level
	surface_composition = p_surface_composition.duplicate()
	terrain = null
	hydrosphere = null
	cryosphere = null


## Returns whether the surface has terrain data.
## @return: True if terrain is not null.
func has_terrain() -> bool:
	return terrain != null


## Returns whether the surface has hydrosphere data.
## @return: True if hydrosphere is not null.
func has_hydrosphere() -> bool:
	return hydrosphere != null


## Returns whether the surface has cryosphere data.
## @return: True if cryosphere is not null.
func has_cryosphere() -> bool:
	return cryosphere != null


## Returns whether the surface is volcanically active.
## @return: True if volcanism level is significant.
func is_volcanically_active() -> bool:
	return volcanism_level > 0.1


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = {
		"temperature_k": temperature_k,
		"albedo": albedo,
		"surface_type": surface_type,
		"volcanism_level": volcanism_level,
		"surface_composition": surface_composition.duplicate(),
	}
	
	if terrain != null:
		data["terrain"] = terrain.to_dict()
	
	if hydrosphere != null:
		data["hydrosphere"] = hydrosphere.to_dict()
	
	if cryosphere != null:
		data["cryosphere"] = cryosphere.to_dict()
	
	return data


## Creates a SurfaceProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new SurfaceProps instance.
static func from_dict(data: Dictionary) -> SurfaceProps:
	var props: SurfaceProps = _SCRIPT.new(
		data.get("temperature_k", 0.0) as float,
		data.get("albedo", 0.0) as float,
		data.get("surface_type", "") as String,
		data.get("volcanism_level", 0.0) as float,
		data.get("surface_composition", {}) as Dictionary
	) as SurfaceProps
	
	if data.has("terrain"):
		props.terrain = TerrainProps.from_dict(data["terrain"])
	
	if data.has("hydrosphere"):
		props.hydrosphere = HydrosphereProps.from_dict(data["hydrosphere"])
	
	if data.has("cryosphere"):
		props.cryosphere = CryosphereProps.from_dict(data["cryosphere"])
	
	return props
