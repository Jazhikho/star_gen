## Colony type classification based on founding purpose.
## Affects starting regime, growth patterns, and native relations.
class_name ColonyType
extends RefCounted


## Colony founding purposes.
enum Type {
	SETTLEMENT, ## General colonization, mixed purpose
	CORPORATE, ## Resource extraction, commercial venture
	MILITARY, ## Strategic outpost, defense installation
	SCIENTIFIC, ## Research station, exploration base
	PENAL, ## Prison colony, exile settlement
	RELIGIOUS, ## Faith-based community, missionary
	AGRICULTURAL, ## Food production, farming colony
	INDUSTRIAL, ## Manufacturing, processing facility
	REFUGEE, ## Displaced population, emergency settlement
	SEPARATIST, ## Ideological breakaway, independence movement
}


## Converts a colony type to a display string.
## @param type: The type enum value.
## @return: Human-readable string.
static func to_string_name(type: Type) -> String:
	match type:
		Type.SETTLEMENT:
			return "Settlement"
		Type.CORPORATE:
			return "Corporate"
		Type.MILITARY:
			return "Military"
		Type.SCIENTIFIC:
			return "Scientific"
		Type.PENAL:
			return "Penal"
		Type.RELIGIOUS:
			return "Religious"
		Type.AGRICULTURAL:
			return "Agricultural"
		Type.INDUSTRIAL:
			return "Industrial"
		Type.REFUGEE:
			return "Refugee"
		Type.SEPARATIST:
			return "Separatist"
		_:
			return "Unknown"


## Converts a string to a colony type.
## @param name: The string name (case-insensitive).
## @return: The type, or SETTLEMENT if not found.
static func from_string(name: String) -> Type:
	match name.to_lower():
		"settlement":
			return Type.SETTLEMENT
		"corporate":
			return Type.CORPORATE
		"military":
			return Type.MILITARY
		"scientific":
			return Type.SCIENTIFIC
		"penal":
			return Type.PENAL
		"religious":
			return Type.RELIGIOUS
		"agricultural":
			return Type.AGRICULTURAL
		"industrial":
			return Type.INDUSTRIAL
		"refugee":
			return Type.REFUGEE
		"separatist":
			return Type.SEPARATIST
		_:
			return Type.SETTLEMENT


## Returns the typical starting regime for a colony type.
## @param type: The colony type.
## @return: The expected starting regime.
static func typical_starting_regime(type: Type) -> GovernmentType.Regime:
	match type:
		Type.SETTLEMENT:
			return GovernmentType.Regime.CONSTITUTIONAL
		Type.CORPORATE:
			return GovernmentType.Regime.CORPORATE
		Type.MILITARY:
			return GovernmentType.Regime.MILITARY_JUNTA
		Type.SCIENTIFIC:
			return GovernmentType.Regime.TECHNOCRACY
		Type.PENAL:
			return GovernmentType.Regime.MILITARY_JUNTA
		Type.RELIGIOUS:
			return GovernmentType.Regime.THEOCRACY
		Type.AGRICULTURAL:
			return GovernmentType.Regime.CONSTITUTIONAL
		Type.INDUSTRIAL:
			return GovernmentType.Regime.CORPORATE
		Type.REFUGEE:
			return GovernmentType.Regime.TRIBAL # Often starts disorganized
		Type.SEPARATIST:
			return GovernmentType.Regime.ELITE_REPUBLIC
		_:
			return GovernmentType.Regime.CONSTITUTIONAL


## Returns the typical initial population for a colony type.
## @param type: The colony type.
## @return: Approximate starting population.
static func typical_initial_population(type: Type) -> int:
	match type:
		Type.SETTLEMENT:
			return 10000
		Type.CORPORATE:
			return 5000
		Type.MILITARY:
			return 2000
		Type.SCIENTIFIC:
			return 500
		Type.PENAL:
			return 20000
		Type.RELIGIOUS:
			return 3000
		Type.AGRICULTURAL:
			return 8000
		Type.INDUSTRIAL:
			return 15000
		Type.REFUGEE:
			return 50000
		Type.SEPARATIST:
			return 25000
		_:
			return 10000


## Returns the growth rate modifier for a colony type.
## @param type: The colony type.
## @return: Multiplier for base growth rate.
static func growth_rate_modifier(type: Type) -> float:
	match type:
		Type.SETTLEMENT:
			return 1.0
		Type.CORPORATE:
			return 0.7 # Less family-oriented
		Type.MILITARY:
			return 0.5 # Mostly rotation, not permanent
		Type.SCIENTIFIC:
			return 0.4 # Small, specialized
		Type.PENAL:
			return 0.8 # Controlled population
		Type.RELIGIOUS:
			return 1.3 # Often pro-natalist
		Type.AGRICULTURAL:
			return 1.2 # Family farms
		Type.INDUSTRIAL:
			return 0.9
		Type.REFUGEE:
			return 1.1 # Diverse demographics
		Type.SEPARATIST:
			return 1.0
		_:
			return 1.0


## Returns whether a colony type typically has tense native relations.
## @param type: The colony type.
## @return: True if likely to conflict with natives.
static func tends_toward_native_conflict(type: Type) -> bool:
	match type:
		Type.CORPORATE, Type.MILITARY, Type.PENAL, Type.INDUSTRIAL:
			return true
		_:
			return false


## Returns the number of colony types.
## @return: Count of Type enum values.
static func count() -> int:
	return 10
