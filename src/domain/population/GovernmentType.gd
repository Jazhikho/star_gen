## Government/regime types based on historical regime forms.
## Used by both NativePopulation and Colony to track political structure.
class_name GovernmentType
extends RefCounted


## Regime forms (matching RegimeChangeModel.md).
enum Regime {
	TRIBAL,              ## Tribal/band governance (custom, councils, chiefs)
	CHIEFDOM,            ## Chiefdom/early hierarchy
	CITY_STATE,          ## City-state oligarchy/republic (elite participation)
	FEUDAL,              ## Feudal fragmentation/lord networks
	PATRIMONIAL_KINGDOM, ## Patrimonial kingdom (personal rule + local delegation)
	BUREAUCRATIC_EMPIRE, ## Bureaucratic empire (officials, records, taxation)
	ABSOLUTE_MONARCHY,   ## Central monarchy/absolutist state (strong executive)
	CONSTITUTIONAL,      ## Constitutional bargain (assemblies, charters, constraints)
	ELITE_REPUBLIC,      ## Elite republic (limited franchise, strong institutions)
	MASS_DEMOCRACY,      ## Mass democracy (broad franchise, parties, elections)
	ONE_PARTY_STATE,     ## One-party state (bureaucratic authoritarian)
	MILITARY_JUNTA,      ## Military junta/emergency rule
	PERSONALIST_DICT,    ## Personalist dictatorship (rule by inner circle)
	FAILED_STATE,        ## Failed state/warlordism (fragmented coercion)
	CORPORATE,           ## Corporate governance (colony-specific)
	THEOCRACY,           ## Religious rule
	TECHNOCRACY,         ## Rule by technical experts
}


## Converts a regime to a display string.
## @param regime: The regime enum value.
## @return: Human-readable string.
static func to_string_name(regime: Regime) -> String:
	match regime:
		Regime.TRIBAL:
			return "Tribal"
		Regime.CHIEFDOM:
			return "Chiefdom"
		Regime.CITY_STATE:
			return "City-State"
		Regime.FEUDAL:
			return "Feudal"
		Regime.PATRIMONIAL_KINGDOM:
			return "Patrimonial Kingdom"
		Regime.BUREAUCRATIC_EMPIRE:
			return "Bureaucratic Empire"
		Regime.ABSOLUTE_MONARCHY:
			return "Absolute Monarchy"
		Regime.CONSTITUTIONAL:
			return "Constitutional Government"
		Regime.ELITE_REPUBLIC:
			return "Elite Republic"
		Regime.MASS_DEMOCRACY:
			return "Mass Democracy"
		Regime.ONE_PARTY_STATE:
			return "One-Party State"
		Regime.MILITARY_JUNTA:
			return "Military Junta"
		Regime.PERSONALIST_DICT:
			return "Personalist Dictatorship"
		Regime.FAILED_STATE:
			return "Failed State"
		Regime.CORPORATE:
			return "Corporate Governance"
		Regime.THEOCRACY:
			return "Theocracy"
		Regime.TECHNOCRACY:
			return "Technocracy"
		_:
			return "Unknown"


## Converts a string to a regime enum.
## @param name: The string name (case-insensitive).
## @return: The regime, or TRIBAL if not found.
static func from_string(name: String) -> Regime:
	match name.to_lower().replace(" ", "_").replace("-", "_"):
		"tribal":
			return Regime.TRIBAL
		"chiefdom":
			return Regime.CHIEFDOM
		"city_state":
			return Regime.CITY_STATE
		"feudal":
			return Regime.FEUDAL
		"patrimonial_kingdom":
			return Regime.PATRIMONIAL_KINGDOM
		"bureaucratic_empire":
			return Regime.BUREAUCRATIC_EMPIRE
		"absolute_monarchy":
			return Regime.ABSOLUTE_MONARCHY
		"constitutional", "constitutional_government":
			return Regime.CONSTITUTIONAL
		"elite_republic":
			return Regime.ELITE_REPUBLIC
		"mass_democracy":
			return Regime.MASS_DEMOCRACY
		"one_party_state":
			return Regime.ONE_PARTY_STATE
		"military_junta":
			return Regime.MILITARY_JUNTA
		"personalist_dict", "personalist_dictatorship":
			return Regime.PERSONALIST_DICT
		"failed_state":
			return Regime.FAILED_STATE
		"corporate", "corporate_governance":
			return Regime.CORPORATE
		"theocracy":
			return Regime.THEOCRACY
		"technocracy":
			return Regime.TECHNOCRACY
		_:
			return Regime.TRIBAL


## Returns typical starting regimes for native populations.
## @return: Array of regimes that natives typically start with.
static func native_starting_regimes() -> Array[Regime]:
	return [Regime.TRIBAL, Regime.CHIEFDOM]


## Returns typical starting regimes for colonies.
## @return: Array of regimes that colonies typically start with.
static func colony_starting_regimes() -> Array[Regime]:
	return [
		Regime.CORPORATE,
		Regime.MILITARY_JUNTA,
		Regime.CONSTITUTIONAL,
		Regime.ONE_PARTY_STATE,
		Regime.TECHNOCRACY,
	]


## Returns the baseline progression path for a regime (non-crisis).
## @param regime: The current regime.
## @return: Array of regimes this can naturally evolve into.
static func baseline_transitions(regime: Regime) -> Array[Regime]:
	match regime:
		Regime.TRIBAL:
			return [Regime.CHIEFDOM]
		Regime.CHIEFDOM:
			return [Regime.CITY_STATE, Regime.FEUDAL, Regime.PATRIMONIAL_KINGDOM]
		Regime.CITY_STATE:
			return [Regime.ELITE_REPUBLIC, Regime.PATRIMONIAL_KINGDOM]
		Regime.FEUDAL:
			return [Regime.ABSOLUTE_MONARCHY, Regime.PATRIMONIAL_KINGDOM]
		Regime.PATRIMONIAL_KINGDOM:
			return [Regime.ABSOLUTE_MONARCHY, Regime.BUREAUCRATIC_EMPIRE]
		Regime.BUREAUCRATIC_EMPIRE:
			return [Regime.ABSOLUTE_MONARCHY]
		Regime.ABSOLUTE_MONARCHY:
			return [Regime.CONSTITUTIONAL, Regime.BUREAUCRATIC_EMPIRE]
		Regime.CONSTITUTIONAL:
			return [Regime.ELITE_REPUBLIC]
		Regime.ELITE_REPUBLIC:
			return [Regime.MASS_DEMOCRACY]
		Regime.MASS_DEMOCRACY:
			return [Regime.CONSTITUTIONAL]
		Regime.ONE_PARTY_STATE:
			return [Regime.CONSTITUTIONAL]
		Regime.MILITARY_JUNTA:
			return [Regime.ONE_PARTY_STATE, Regime.ABSOLUTE_MONARCHY]
		Regime.PERSONALIST_DICT:
			return [Regime.ABSOLUTE_MONARCHY, Regime.BUREAUCRATIC_EMPIRE]
		Regime.FAILED_STATE:
			return [Regime.MILITARY_JUNTA, Regime.PATRIMONIAL_KINGDOM]
		Regime.CORPORATE:
			return [Regime.ELITE_REPUBLIC, Regime.ONE_PARTY_STATE, Regime.CONSTITUTIONAL]
		Regime.THEOCRACY:
			return [Regime.ABSOLUTE_MONARCHY, Regime.CONSTITUTIONAL]
		Regime.TECHNOCRACY:
			return [Regime.ELITE_REPUBLIC, Regime.ONE_PARTY_STATE]
		_:
			return []


## Returns crisis-driven transitions (pressure/collapse scenarios).
## @param regime: The current regime.
## @return: Array of regimes this can collapse/shift into under crisis.
static func crisis_transitions(regime: Regime) -> Array[Regime]:
	var common_crisis: Array[Regime] = [
		Regime.FAILED_STATE,
		Regime.MILITARY_JUNTA,
		Regime.PERSONALIST_DICT,
	]

	match regime:
		Regime.MASS_DEMOCRACY:
			return [Regime.MILITARY_JUNTA, Regime.PERSONALIST_DICT, Regime.ONE_PARTY_STATE]
		Regime.ELITE_REPUBLIC:
			return [Regime.MILITARY_JUNTA, Regime.PERSONALIST_DICT, Regime.MASS_DEMOCRACY]
		Regime.CONSTITUTIONAL:
			return [Regime.MILITARY_JUNTA, Regime.PERSONALIST_DICT, Regime.ABSOLUTE_MONARCHY]
		Regime.BUREAUCRATIC_EMPIRE:
			return [Regime.FEUDAL, Regime.FAILED_STATE, Regime.MILITARY_JUNTA]
		Regime.ABSOLUTE_MONARCHY:
			return [Regime.CONSTITUTIONAL, Regime.FAILED_STATE, Regime.MILITARY_JUNTA]
		_:
			return common_crisis


## Returns whether a regime is considered authoritarian.
## @param regime: The regime to check.
## @return: True if authoritarian.
static func is_authoritarian(regime: Regime) -> bool:
	match regime:
		Regime.ABSOLUTE_MONARCHY, Regime.ONE_PARTY_STATE, Regime.MILITARY_JUNTA, \
		Regime.PERSONALIST_DICT, Regime.THEOCRACY:
			return true
		_:
			return false


## Returns whether a regime is considered democratic/participatory.
## @param regime: The regime to check.
## @return: True if democratic/participatory.
static func is_participatory(regime: Regime) -> bool:
	match regime:
		Regime.TRIBAL, Regime.CITY_STATE, Regime.CONSTITUTIONAL, \
		Regime.ELITE_REPUBLIC, Regime.MASS_DEMOCRACY:
			return true
		_:
			return false


## Returns whether a regime is considered unstable/transitional.
## @param regime: The regime to check.
## @return: True if unstable.
static func is_unstable(regime: Regime) -> bool:
	match regime:
		Regime.FAILED_STATE, Regime.MILITARY_JUNTA:
			return true
		_:
			return false


## Returns the number of regime types.
## @return: Count of Regime enum values.
static func count() -> int:
	return 17
