## Resource types available for exploitation on a planet.
## Used for colony suitability and economic potential.
class_name ResourceType
extends RefCounted


## Resource types.
enum Type {
	WATER, ## Liquid water or extractable ice
	SILICATES, ## Common rock/sand materials
	METALS, ## Iron, aluminum, common metals
	RARE_ELEMENTS, ## Rare earth elements, precious metals
	RADIOACTIVES, ## Uranium, thorium, fissile materials
	HYDROCARBONS, ## Fossil fuels, organic compounds
	ORGANICS, ## Biomass, organic materials
	VOLATILES, ## Gases, atmospheric resources
	CRYSTALS, ## Gemstones, crystalline materials
	EXOTICS, ## Unusual or unique materials
}


## Converts a resource type to a display string.
## @param resource: The resource enum value.
## @return: Human-readable string.
static func to_string_name(resource: Type) -> String:
	match resource:
		Type.WATER:
			return "Water"
		Type.SILICATES:
			return "Silicates"
		Type.METALS:
			return "Metals"
		Type.RARE_ELEMENTS:
			return "Rare Elements"
		Type.RADIOACTIVES:
			return "Radioactives"
		Type.HYDROCARBONS:
			return "Hydrocarbons"
		Type.ORGANICS:
			return "Organics"
		Type.VOLATILES:
			return "Volatiles"
		Type.CRYSTALS:
			return "Crystals"
		Type.EXOTICS:
			return "Exotics"
		_:
			return "Unknown"


## Converts a string to a resource type.
## @param name: The string name (case-insensitive).
## @return: The resource type, or SILICATES if not found.
static func from_string(name: String) -> Type:
	match name.to_lower().replace(" ", "_"):
		"water":
			return Type.WATER
		"silicates":
			return Type.SILICATES
		"metals":
			return Type.METALS
		"rare_elements":
			return Type.RARE_ELEMENTS
		"radioactives":
			return Type.RADIOACTIVES
		"hydrocarbons":
			return Type.HYDROCARBONS
		"organics":
			return Type.ORGANICS
		"volatiles":
			return Type.VOLATILES
		"crystals":
			return Type.CRYSTALS
		"exotics":
			return Type.EXOTICS
		_:
			return Type.SILICATES


## Returns the number of resource types.
## @return: Count of resource enum values.
static func count() -> int:
	return 10
