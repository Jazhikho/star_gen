## Shared utility functions for atmosphere-related calculations.
## Provides common operations used across multiple generators.
class_name AtmosphereUtils
extends RefCounted

const _atmosphere_props := preload("res://src/domain/celestial/components/AtmosphereProps.gd")


## Gets average molecular mass for a gas mixture.
## @param composition: Gas composition dictionary (gas name -> fraction).
## @return: Average molecular mass in kg.
static func get_average_molecular_mass(composition: Dictionary) -> float:
	# Molecular masses in kg
	var masses: Dictionary = {
		"H2": 2.016 * 1.6605e-27,
		"He": 4.003 * 1.6605e-27,
		"CH4": 16.04 * 1.6605e-27,
		"NH3": 17.03 * 1.6605e-27,
		"H2O": 18.02 * 1.6605e-27,
		"N2": 28.01 * 1.6605e-27,
		"O2": 32.00 * 1.6605e-27,
		"Ar": 39.95 * 1.6605e-27,
		"CO2": 44.01 * 1.6605e-27,
		"SO2": 64.07 * 1.6605e-27,
	}
	
	var avg_mass: float = 0.0
	var total_fraction: float = 0.0
	
	for gas in composition.keys():
		var fraction: float = composition[gas] as float
		var mass: float = masses.get(gas, 28.0 * 1.6605e-27)  # Default to N2 mass
		avg_mass += fraction * mass
		total_fraction += fraction
	
	if total_fraction > 0.0:
		return avg_mass / total_fraction
	
	return 28.0 * 1.6605e-27  # Default N2


## Calculates surface temperature from equilibrium temperature and atmosphere.
## @param equilibrium_temp_k: Equilibrium temperature in Kelvin (without greenhouse effect).
## @param atmosphere: Atmosphere properties (may be null).
## @return: Surface temperature in Kelvin (with greenhouse effect if atmosphere present).
static func calculate_surface_temperature(
	equilibrium_temp_k: float,
	atmosphere: AtmosphereProps
) -> float:
	if atmosphere != null:
		return equilibrium_temp_k * atmosphere.greenhouse_factor
	return equilibrium_temp_k
