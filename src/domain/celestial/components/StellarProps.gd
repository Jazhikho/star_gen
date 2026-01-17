## Stellar properties for star-type celestial bodies.
## Contains luminosity, spectral classification, and evolution data.
class_name StellarProps
extends RefCounted


## Luminosity in watts.
var luminosity_watts: float

## Effective photosphere temperature in Kelvin.
var effective_temperature_k: float

## Spectral classification string (e.g., "G2V", "M5V", "K0III").
var spectral_class: String

## Stellar type category.
var stellar_type: String

## Metallicity relative to solar (Sun = 1.0).
var metallicity: float

## Age in years.
var age_years: float

## Solar luminosity in watts for reference.
const SOLAR_LUMINOSITY_WATTS: float = 3.828e26


## Creates a new StellarProps instance.
## @param p_luminosity_watts: Luminosity in watts.
## @param p_effective_temperature_k: Photosphere temperature in Kelvin.
## @param p_spectral_class: Spectral classification string.
## @param p_stellar_type: Stellar type category.
## @param p_metallicity: Metallicity relative to solar.
## @param p_age_years: Age in years.
func _init(
	p_luminosity_watts: float = 0.0,
	p_effective_temperature_k: float = 0.0,
	p_spectral_class: String = "",
	p_stellar_type: String = "main_sequence",
	p_metallicity: float = 1.0,
	p_age_years: float = 0.0
) -> void:
	luminosity_watts = p_luminosity_watts
	effective_temperature_k = p_effective_temperature_k
	spectral_class = p_spectral_class
	stellar_type = p_stellar_type
	metallicity = p_metallicity
	age_years = p_age_years


## Returns luminosity in solar luminosities.
## @return: Luminosity as multiple of Sun's luminosity.
func get_luminosity_solar() -> float:
	if SOLAR_LUMINOSITY_WATTS <= 0.0:
		return 0.0
	return luminosity_watts / SOLAR_LUMINOSITY_WATTS


## Calculates the habitable zone inner edge in meters.
## Based on luminosity relative to solar.
## @return: Inner habitable zone radius in meters.
func get_habitable_zone_inner_m() -> float:
	var l_solar: float = get_luminosity_solar()
	if l_solar <= 0.0:
		return 0.0
	# Empirical formula: inner edge ~0.95 AU * sqrt(L/L_sun)
	return Units.au_to_meters(0.95 * sqrt(l_solar))


## Calculates the habitable zone outer edge in meters.
## Based on luminosity relative to solar.
## @return: Outer habitable zone radius in meters.
func get_habitable_zone_outer_m() -> float:
	var l_solar: float = get_luminosity_solar()
	if l_solar <= 0.0:
		return 0.0
	# Empirical formula: outer edge ~1.37 AU * sqrt(L/L_sun)
	return Units.au_to_meters(1.37 * sqrt(l_solar))


## Calculates the frost line distance in meters.
## Where water ice becomes stable.
## @return: Frost line radius in meters.
func get_frost_line_m() -> float:
	var l_solar: float = get_luminosity_solar()
	if l_solar <= 0.0:
		return 0.0
	# Empirical formula: frost line ~2.7 AU * sqrt(L/L_sun)
	return Units.au_to_meters(2.7 * sqrt(l_solar))


## Extracts the spectral letter from the spectral class.
## @return: Single letter (O, B, A, F, G, K, M) or empty string.
func get_spectral_letter() -> String:
	if spectral_class.is_empty():
		return ""
	return spectral_class.substr(0, 1).to_upper()


## Extracts the luminosity class from the spectral class.
## @return: Roman numeral (I, II, III, IV, V) or empty string.
func get_luminosity_class() -> String:
	if spectral_class.length() < 3:
		return ""
	# Find where letters end and luminosity class begins
	var idx: int = 2
	while idx < spectral_class.length():
		var c: String = spectral_class.substr(idx, 1)
		if c == "I" or c == "V":
			return spectral_class.substr(idx)
		idx += 1
	return ""


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"luminosity_watts": luminosity_watts,
		"effective_temperature_k": effective_temperature_k,
		"spectral_class": spectral_class,
		"stellar_type": stellar_type,
		"metallicity": metallicity,
		"age_years": age_years,
	}


## Creates a StellarProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new StellarProps instance.
static func from_dict(data: Dictionary) -> StellarProps:
	var script: GDScript = load("res://src/domain/celestial/components/StellarProps.gd") as GDScript
	return script.new(
		data.get("luminosity_watts", 0.0) as float,
		data.get("effective_temperature_k", 0.0) as float,
		data.get("spectral_class", "") as String,
		data.get("stellar_type", "main_sequence") as String,
		data.get("metallicity", 1.0) as float,
		data.get("age_years", 0.0) as float
	) as StellarProps
