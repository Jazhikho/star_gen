## Biome classification for planetary surfaces.
## Represents distinct ecological or surface regions.
class_name BiomeType
extends RefCounted


## Biome types.
enum Type {
	OCEAN, ## Liquid water bodies
	ICE_SHEET, ## Permanent ice coverage
	TUNDRA, ## Cold, sparse vegetation
	TAIGA, ## Cold coniferous forest
	FOREST, ## Temperate forest
	GRASSLAND, ## Temperate plains/prairie
	SAVANNA, ## Tropical grassland with scattered trees
	JUNGLE, ## Dense tropical forest
	DESERT, ## Arid, minimal precipitation
	WETLAND, ## Swamps, marshes, bogs
	MOUNTAIN, ## High elevation terrain
	VOLCANIC, ## Active volcanic regions
	BARREN, ## Lifeless rock/regolith
	SUBSURFACE, ## Underground habitable zones
}


## Converts a biome type to a display string.
## @param biome: The biome enum value.
## @return: Human-readable string.
static func to_string_name(biome: Type) -> String:
	match biome:
		Type.OCEAN:
			return "Ocean"
		Type.ICE_SHEET:
			return "Ice Sheet"
		Type.TUNDRA:
			return "Tundra"
		Type.TAIGA:
			return "Taiga"
		Type.FOREST:
			return "Forest"
		Type.GRASSLAND:
			return "Grassland"
		Type.SAVANNA:
			return "Savanna"
		Type.JUNGLE:
			return "Jungle"
		Type.DESERT:
			return "Desert"
		Type.WETLAND:
			return "Wetland"
		Type.MOUNTAIN:
			return "Mountain"
		Type.VOLCANIC:
			return "Volcanic"
		Type.BARREN:
			return "Barren"
		Type.SUBSURFACE:
			return "Subsurface"
		_:
			return "Unknown"


## Converts a string to a biome type.
## @param name: The string name (case-insensitive).
## @return: The biome type, or BARREN if not found.
static func from_string(name: String) -> Type:
	match name.to_lower().replace(" ", "_"):
		"ocean":
			return Type.OCEAN
		"ice_sheet":
			return Type.ICE_SHEET
		"tundra":
			return Type.TUNDRA
		"taiga":
			return Type.TAIGA
		"forest":
			return Type.FOREST
		"grassland":
			return Type.GRASSLAND
		"savanna":
			return Type.SAVANNA
		"jungle":
			return Type.JUNGLE
		"desert":
			return Type.DESERT
		"wetland":
			return Type.WETLAND
		"mountain":
			return Type.MOUNTAIN
		"volcanic":
			return Type.VOLCANIC
		"barren":
			return Type.BARREN
		"subsurface":
			return Type.SUBSURFACE
		_:
			return Type.BARREN


## Returns whether a biome can support surface life.
## @param biome: The biome to check.
## @return: True if the biome can support life.
static func can_support_life(biome: Type) -> bool:
	match biome:
		Type.BARREN, Type.VOLCANIC, Type.ICE_SHEET:
			return false
		_:
			return true


## Returns the number of biome types.
## @return: Count of biome enum values.
static func count() -> int:
	return 14
