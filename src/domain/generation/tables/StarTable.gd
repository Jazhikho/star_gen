## Lookup table for stellar properties by spectral class.
## Contains mass, luminosity, temperature, and radius relationships for main sequence stars.
class_name StarTable
extends RefCounted


## Mass ranges in solar masses for each spectral class.
const MASS_RANGES: Dictionary = {
	StarClass.SpectralClass.O: {"min": 16.0, "max": 150.0},
	StarClass.SpectralClass.B: {"min": 2.1, "max": 16.0},
	StarClass.SpectralClass.A: {"min": 1.4, "max": 2.1},
	StarClass.SpectralClass.F: {"min": 1.04, "max": 1.4},
	StarClass.SpectralClass.G: {"min": 0.8, "max": 1.04},
	StarClass.SpectralClass.K: {"min": 0.45, "max": 0.8},
	StarClass.SpectralClass.M: {"min": 0.08, "max": 0.45},
}


## Temperature ranges in Kelvin for each spectral class.
const TEMPERATURE_RANGES: Dictionary = {
	StarClass.SpectralClass.O: {"min": 30000.0, "max": 52000.0},
	StarClass.SpectralClass.B: {"min": 10000.0, "max": 30000.0},
	StarClass.SpectralClass.A: {"min": 7500.0, "max": 10000.0},
	StarClass.SpectralClass.F: {"min": 6000.0, "max": 7500.0},
	StarClass.SpectralClass.G: {"min": 5200.0, "max": 6000.0},
	StarClass.SpectralClass.K: {"min": 3700.0, "max": 5200.0},
	StarClass.SpectralClass.M: {"min": 2400.0, "max": 3700.0},
}


## Luminosity ranges in solar luminosities for each spectral class.
const LUMINOSITY_RANGES: Dictionary = {
	StarClass.SpectralClass.O: {"min": 30000.0, "max": 1000000.0},
	StarClass.SpectralClass.B: {"min": 25.0, "max": 30000.0},
	StarClass.SpectralClass.A: {"min": 5.0, "max": 25.0},
	StarClass.SpectralClass.F: {"min": 1.5, "max": 5.0},
	StarClass.SpectralClass.G: {"min": 0.6, "max": 1.5},
	StarClass.SpectralClass.K: {"min": 0.08, "max": 0.6},
	StarClass.SpectralClass.M: {"min": 0.0001, "max": 0.08},
}


## Radius ranges in solar radii for each spectral class.
const RADIUS_RANGES: Dictionary = {
	StarClass.SpectralClass.O: {"min": 6.6, "max": 20.0},
	StarClass.SpectralClass.B: {"min": 1.8, "max": 6.6},
	StarClass.SpectralClass.A: {"min": 1.4, "max": 1.8},
	StarClass.SpectralClass.F: {"min": 1.15, "max": 1.4},
	StarClass.SpectralClass.G: {"min": 0.96, "max": 1.15},
	StarClass.SpectralClass.K: {"min": 0.7, "max": 0.96},
	StarClass.SpectralClass.M: {"min": 0.1, "max": 0.7},
}


## Main sequence lifetime in years for each spectral class (approximate).
const LIFETIME_RANGES: Dictionary = {
	StarClass.SpectralClass.O: {"min": 1.0e6, "max": 10.0e6},
	StarClass.SpectralClass.B: {"min": 10.0e6, "max": 100.0e6},
	StarClass.SpectralClass.A: {"min": 100.0e6, "max": 1.0e9},
	StarClass.SpectralClass.F: {"min": 1.0e9, "max": 4.0e9},
	StarClass.SpectralClass.G: {"min": 4.0e9, "max": 10.0e9},
	StarClass.SpectralClass.K: {"min": 15.0e9, "max": 50.0e9},
	StarClass.SpectralClass.M: {"min": 50.0e9, "max": 200.0e9},
}


## Gets the mass range for a spectral class.
## @param spectral_class: The spectral class.
## @return: Dictionary with "min" and "max" in solar masses.
static func get_mass_range(spectral_class: StarClass.SpectralClass) -> Dictionary:
	if MASS_RANGES.has(spectral_class):
		return MASS_RANGES[spectral_class]
	return {"min": 0.0, "max": 0.0}


## Gets the temperature range for a spectral class.
## @param spectral_class: The spectral class.
## @return: Dictionary with "min" and "max" in Kelvin.
static func get_temperature_range(spectral_class: StarClass.SpectralClass) -> Dictionary:
	if TEMPERATURE_RANGES.has(spectral_class):
		return TEMPERATURE_RANGES[spectral_class]
	return {"min": 0.0, "max": 0.0}


## Gets the luminosity range for a spectral class.
## @param spectral_class: The spectral class.
## @return: Dictionary with "min" and "max" in solar luminosities.
static func get_luminosity_range(spectral_class: StarClass.SpectralClass) -> Dictionary:
	if LUMINOSITY_RANGES.has(spectral_class):
		return LUMINOSITY_RANGES[spectral_class]
	return {"min": 0.0, "max": 0.0}


## Gets the radius range for a spectral class.
## @param spectral_class: The spectral class.
## @return: Dictionary with "min" and "max" in solar radii.
static func get_radius_range(spectral_class: StarClass.SpectralClass) -> Dictionary:
	if RADIUS_RANGES.has(spectral_class):
		return RADIUS_RANGES[spectral_class]
	return {"min": 0.0, "max": 0.0}


## Gets the main sequence lifetime range for a spectral class.
## @param spectral_class: The spectral class.
## @return: Dictionary with "min" and "max" in years.
static func get_lifetime_range(spectral_class: StarClass.SpectralClass) -> Dictionary:
	if LIFETIME_RANGES.has(spectral_class):
		return LIFETIME_RANGES[spectral_class]
	return {"min": 0.0, "max": 0.0}


## Calculates luminosity from mass using mass-luminosity relation.
## For main sequence: L ∝ M^3.5 (approximate).
## @param mass_solar: Mass in solar masses.
## @return: Luminosity in solar luminosities.
static func luminosity_from_mass(mass_solar: float) -> float:
	if mass_solar <= 0.0:
		return 0.0
	# L ∝ M^3.5 for main sequence stars
	return pow(mass_solar, 3.5)


## Calculates radius from mass using mass-radius relation.
## For main sequence: R ∝ M^0.8 (approximate).
## @param mass_solar: Mass in solar masses.
## @return: Radius in solar radii.
static func radius_from_mass(mass_solar: float) -> float:
	if mass_solar <= 0.0:
		return 0.0
	# R ∝ M^0.8 for main sequence stars
	return pow(mass_solar, 0.8)


## Calculates effective temperature from luminosity and radius.
## Uses Stefan-Boltzmann law: L = 4πR²σT⁴.
## @param luminosity_solar: Luminosity in solar luminosities.
## @param radius_solar: Radius in solar radii.
## @return: Effective temperature in Kelvin.
static func temperature_from_luminosity_radius(
	luminosity_solar: float,
	radius_solar: float
) -> float:
	if luminosity_solar <= 0.0 or radius_solar <= 0.0:
		return 0.0
	# T = T_sun * (L/L_sun)^0.25 / (R/R_sun)^0.5
	# T_sun ≈ 5778 K
	return 5778.0 * pow(luminosity_solar, 0.25) / pow(radius_solar, 0.5)


## Interpolates a value within a spectral class based on subclass.
## @param spectral_class: The spectral class.
## @param subclass: The subclass (0-9).
## @param range_data: The min/max range dictionary.
## @return: Interpolated value.
static func interpolate_by_subclass(
	spectral_class: StarClass.SpectralClass,
	subclass: int,
	range_data: Dictionary
) -> float:
	var t: float = float(subclass) / 9.0
	var min_val: float = range_data["min"]
	var max_val: float = range_data["max"]
	# Subclass 0 is hottest/brightest, 9 is coolest/dimmest within class
	return lerpf(max_val, min_val, t)


## Determines spectral class from temperature.
## @param temperature_k: Temperature in Kelvin.
## @return: The matching spectral class.
static func class_from_temperature(temperature_k: float) -> StarClass.SpectralClass:
	if temperature_k >= 30000.0:
		return StarClass.SpectralClass.O
	elif temperature_k > 10000.0:
		return StarClass.SpectralClass.B
	elif temperature_k > 7500.0:
		return StarClass.SpectralClass.A
	elif temperature_k > 6000.0:
		return StarClass.SpectralClass.F
	elif temperature_k > 5200.0:
		return StarClass.SpectralClass.G
	elif temperature_k > 3700.0:
		return StarClass.SpectralClass.K
	else:
		return StarClass.SpectralClass.M
