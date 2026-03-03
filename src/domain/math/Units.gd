## Physical unit constants and conversion functions for celestial bodies.
## All base units are SI (kg, m, K) unless otherwise noted.
class_name Units
extends RefCounted

const MASS_UNITS_BRIDGE_CLASS: StringName = &"CSharpMassUnitsBridge"
const DISTANCE_UNITS_BRIDGE_CLASS: StringName = &"CSharpDistanceUnitsBridge"
const SCALE_UNITS_BRIDGE_CLASS: StringName = &"CSharpScaleUnitsBridge"


# =============================================================================
# Mass Constants (in kg)
# =============================================================================

## Mass of the Sun in kilograms.
const SOLAR_MASS_KG: float = 1.989e30

## Mass of Earth in kilograms.
const EARTH_MASS_KG: float = 5.972e24

## Mass of Jupiter in kilograms.
const JUPITER_MASS_KG: float = 1.898e27


# =============================================================================
# Distance Constants (in meters)
# =============================================================================

## One Astronomical Unit in meters.
const AU_METERS: float = 1.496e11

## One light year in meters.
const LIGHT_YEAR_METERS: float = 9.461e15

## One parsec in meters.
const PARSEC_METERS: float = 3.086e16


# =============================================================================
# Radius Constants (in meters)
# =============================================================================

## Radius of the Sun in meters.
const SOLAR_RADIUS_METERS: float = 6.957e8

## Radius of Earth in meters.
const EARTH_RADIUS_METERS: float = 6.371e6

## Radius of Jupiter in meters.
const JUPITER_RADIUS_METERS: float = 6.991e7


# =============================================================================
# Temperature Constants
# =============================================================================

## Offset for Celsius to Kelvin conversion.
const CELSIUS_TO_KELVIN_OFFSET: float = 273.15


# =============================================================================
# Mass Conversions
# =============================================================================

## Converts solar masses to kilograms.
## @param solar_masses: Mass in solar masses.
## @return: Mass in kilograms.
static func solar_masses_to_kg(solar_masses: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(MASS_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(MASS_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("SolarMassesToKg"):
		return float(bridge.call("SolarMassesToKg", solar_masses))
	return solar_masses * SOLAR_MASS_KG


## Converts kilograms to solar masses.
## @param kg: Mass in kilograms.
## @return: Mass in solar masses.
static func kg_to_solar_masses(kg: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(MASS_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(MASS_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("KgToSolarMasses"):
		return float(bridge.call("KgToSolarMasses", kg))
	return kg / SOLAR_MASS_KG


## Converts Earth masses to kilograms.
## @param earth_masses: Mass in Earth masses.
## @return: Mass in kilograms.
static func earth_masses_to_kg(earth_masses: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(MASS_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(MASS_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("EarthMassesToKg"):
		return float(bridge.call("EarthMassesToKg", earth_masses))
	return earth_masses * EARTH_MASS_KG


## Converts kilograms to Earth masses.
## @param kg: Mass in kilograms.
## @return: Mass in Earth masses.
static func kg_to_earth_masses(kg: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(MASS_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(MASS_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("KgToEarthMasses"):
		return float(bridge.call("KgToEarthMasses", kg))
	return kg / EARTH_MASS_KG


## Converts Jupiter masses to kilograms.
## @param jupiter_masses: Mass in Jupiter masses.
## @return: Mass in kilograms.
static func jupiter_masses_to_kg(jupiter_masses: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(MASS_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(MASS_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("JupiterMassesToKg"):
		return float(bridge.call("JupiterMassesToKg", jupiter_masses))
	return jupiter_masses * JUPITER_MASS_KG


## Converts kilograms to Jupiter masses.
## @param kg: Mass in kilograms.
## @return: Mass in Jupiter masses.
static func kg_to_jupiter_masses(kg: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(MASS_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(MASS_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("KgToJupiterMasses"):
		return float(bridge.call("KgToJupiterMasses", kg))
	return kg / JUPITER_MASS_KG


# =============================================================================
# Distance Conversions
# =============================================================================

## Converts AU to meters.
## @param au: Distance in astronomical units.
## @return: Distance in meters.
static func au_to_meters(au: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(DISTANCE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(DISTANCE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("AuToMeters"):
		return float(bridge.call("AuToMeters", au))
	return au * AU_METERS


## Converts meters to AU.
## @param meters: Distance in meters.
## @return: Distance in astronomical units.
static func meters_to_au(meters: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(DISTANCE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(DISTANCE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("MetersToAu"):
		return float(bridge.call("MetersToAu", meters))
	return meters / AU_METERS


## Converts light years to meters.
## @param light_years: Distance in light years.
## @return: Distance in meters.
static func light_years_to_meters(light_years: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(DISTANCE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(DISTANCE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("LightYearsToMeters"):
		return float(bridge.call("LightYearsToMeters", light_years))
	return light_years * LIGHT_YEAR_METERS


## Converts meters to light years.
## @param meters: Distance in meters.
## @return: Distance in light years.
static func meters_to_light_years(meters: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(DISTANCE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(DISTANCE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("MetersToLightYears"):
		return float(bridge.call("MetersToLightYears", meters))
	return meters / LIGHT_YEAR_METERS


## Converts parsecs to meters.
## @param parsecs: Distance in parsecs.
## @return: Distance in meters.
static func parsecs_to_meters(parsecs: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(DISTANCE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(DISTANCE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("ParsecsToMeters"):
		return float(bridge.call("ParsecsToMeters", parsecs))
	return parsecs * PARSEC_METERS


## Converts meters to parsecs.
## @param meters: Distance in meters.
## @return: Distance in parsecs.
static func meters_to_parsecs(meters: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(DISTANCE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(DISTANCE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("MetersToParsecs"):
		return float(bridge.call("MetersToParsecs", meters))
	return meters / PARSEC_METERS


# =============================================================================
# Radius Conversions
# =============================================================================

## Converts solar radii to meters.
## @param solar_radii: Radius in solar radii.
## @return: Radius in meters.
static func solar_radii_to_meters(solar_radii: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(SCALE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(SCALE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("SolarRadiiToMeters"):
		return float(bridge.call("SolarRadiiToMeters", solar_radii))
	return solar_radii * SOLAR_RADIUS_METERS


## Converts meters to solar radii.
## @param meters: Radius in meters.
## @return: Radius in solar radii.
static func meters_to_solar_radii(meters: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(SCALE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(SCALE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("MetersToSolarRadii"):
		return float(bridge.call("MetersToSolarRadii", meters))
	return meters / SOLAR_RADIUS_METERS


## Converts Earth radii to meters.
## @param earth_radii: Radius in Earth radii.
## @return: Radius in meters.
static func earth_radii_to_meters(earth_radii: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(SCALE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(SCALE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("EarthRadiiToMeters"):
		return float(bridge.call("EarthRadiiToMeters", earth_radii))
	return earth_radii * EARTH_RADIUS_METERS


## Converts meters to Earth radii.
## @param meters: Radius in meters.
## @return: Radius in Earth radii.
static func meters_to_earth_radii(meters: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(SCALE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(SCALE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("MetersToEarthRadii"):
		return float(bridge.call("MetersToEarthRadii", meters))
	return meters / EARTH_RADIUS_METERS


# =============================================================================
# Temperature Conversions
# =============================================================================

## Converts Celsius to Kelvin.
## @param celsius: Temperature in Celsius.
## @return: Temperature in Kelvin.
static func celsius_to_kelvin(celsius: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(SCALE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(SCALE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("CelsiusToKelvin"):
		return float(bridge.call("CelsiusToKelvin", celsius))
	return celsius + CELSIUS_TO_KELVIN_OFFSET


## Converts Kelvin to Celsius.
## @param kelvin: Temperature in Kelvin.
## @return: Temperature in Celsius.
static func kelvin_to_celsius(kelvin: float) -> float:
	var bridge: Object = null
	if ClassDB.class_exists(SCALE_UNITS_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(SCALE_UNITS_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("KelvinToCelsius"):
		return float(bridge.call("KelvinToCelsius", kelvin))
	return kelvin - CELSIUS_TO_KELVIN_OFFSET
