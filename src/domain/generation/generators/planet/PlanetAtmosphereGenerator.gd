## Generates atmosphere properties for planets.
## Handles atmospheric retention, composition, pressure, and greenhouse effects.
class_name PlanetAtmosphereGenerator
extends RefCounted

const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _atmosphere_props: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _atmosphere_utils: GDScript = preload("res://src/domain/generation/utils/AtmosphereUtils.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")

## Earth's atmospheric pressure in Pascals.
const EARTH_ATMOSPHERE_PA: float = 101325.0

## Boltzmann constant in J/K.
const BOLTZMANN_K: float = 1.380649e-23

## Hydrogen molecular mass in kg.
const HYDROGEN_MASS_KG: float = 1.6735575e-27


## Generates atmosphere properties for a planet.
## @param spec: Planet specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param equilibrium_temp_k: Equilibrium temperature.
## @param rng: Random number generator.
## @return: AtmosphereProps or null.
static func generate_atmosphere(
	spec: PlanetSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> AtmosphereProps:
	# Generate surface pressure
	var surface_pressure_pa: float = _calculate_surface_pressure(
		spec, physical, size_cat, rng
	)
	
	# Generate composition based on size and zone
	var composition: Dictionary = _generate_atmosphere_composition(
		size_cat, zone, equilibrium_temp_k, rng
	)
	
	# Calculate scale height: H = kT/(mg)
	# Use average molecular mass based on composition
	var avg_molecular_mass: float = _atmosphere_utils.get_average_molecular_mass(composition)
	var gravity: float = physical.get_surface_gravity_m_s2()
	var scale_height_m: float = 0.0
	if gravity > 0.0 and avg_molecular_mass > 0.0:
		scale_height_m = BOLTZMANN_K * equilibrium_temp_k / (avg_molecular_mass * gravity)
	
	# Calculate greenhouse factor
	var greenhouse_factor: float = _calculate_greenhouse_factor(
		composition, surface_pressure_pa, rng
	)
	
	return AtmosphereProps.new(
		surface_pressure_pa,
		scale_height_m,
		composition,
		greenhouse_factor
	)


## Determines whether this planet should have an atmosphere.
## @param spec: The planet specification.
## @param physical: The physical properties.
## @param size_cat: The size category.
## @param context: The parent context.
## @param rng: Random number generator.
## @return: True if atmosphere should be generated.
static func should_have_atmosphere(
	spec: PlanetSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	context: ParentContext,
	rng: SeededRng
) -> bool:
	# Check spec preference first
	if spec.has_atmosphere_preference():
		return spec.has_atmosphere as bool
	
	# Gas giants always have atmospheres
	if SizeCategory.is_gaseous(size_cat):
		return true
	
	# Check atmospheric retention capability
	var can_retain: bool = _can_retain_atmosphere(physical, size_cat, context)
	
	if not can_retain:
		return false
	
	# Rocky planets: probability based on size and zone
	var retention_probability: float = _get_atmosphere_probability(size_cat)
	return rng.randf() < retention_probability


## Checks if a planet can retain an atmosphere against thermal escape and stellar wind.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param context: Parent context.
## @return: True if atmosphere can be retained.
static func _can_retain_atmosphere(
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	context: ParentContext
) -> bool:
	var escape_velocity: float = physical.get_escape_velocity_m_s()
	
	# Calculate thermal velocity for hydrogen (lightest gas)
	# v_thermal = sqrt(3kT/m)
	var equilibrium_temp: float = context.get_equilibrium_temperature_k(0.3)
	var thermal_velocity: float = sqrt(3.0 * BOLTZMANN_K * equilibrium_temp / HYDROGEN_MASS_KG)
	
	# Jeans escape parameter: Î» = v_escape / v_thermal
	# Need Î» > 6 to retain gas over geological time
	# For heavier gases (N2, CO2), the threshold is lower
	var jeans_param: float = escape_velocity / thermal_velocity
	
	# Dwarf planets need very high jeans parameter due to small size
	if size_cat == SizeCategory.Category.DWARF:
		return jeans_param > 10.0  # Very hard for dwarfs
	
	# Rocky planets need jeans > 6 for heavy gases
	if SizeCategory.is_rocky(size_cat):
		return jeans_param > 4.0  # Can retain CO2, N2
	
	# Gaseous planets always retain
	return true


## Gets the probability of having an atmosphere based on size category.
## @param size_cat: The size category.
## @return: Probability (0-1).
static func _get_atmosphere_probability(size_cat: SizeCategory.Category) -> float:
	match size_cat:
		SizeCategory.Category.DWARF:
			return 0.1  # Very rare (like Pluto's tenuous atmosphere)
		SizeCategory.Category.SUB_TERRESTRIAL:
			return 0.4  # Moderate (Mars has thin atmosphere)
		SizeCategory.Category.TERRESTRIAL:
			return 0.8  # Common (Earth, Venus)
		SizeCategory.Category.SUPER_EARTH:
			return 0.95  # Almost certain
		_:
			return 1.0  # Gas giants always


## Calculates surface pressure based on planet properties.
## @param spec: Planet specification.
## @param _physical: Physical properties (unused, reserved for future use).
## @param size_cat: Size category.
## @param rng: Random number generator.
## @return: Surface pressure in Pascals.
static func _calculate_surface_pressure(
	spec: PlanetSpec,
	_physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	rng: SeededRng
) -> float:
	var override_pressure: float = spec.get_override_float("atmosphere.surface_pressure_pa", -1.0)
	if override_pressure >= 0.0:
		return override_pressure
	
	match size_cat:
		SizeCategory.Category.DWARF:
			# Trace atmosphere (Pluto: ~1 Pa)
			return rng.randf_range(0.1, 100.0)
		
		SizeCategory.Category.SUB_TERRESTRIAL:
			# Thin atmosphere (Mars: ~600 Pa)
			return rng.randf_range(100.0, 10000.0)
		
		SizeCategory.Category.TERRESTRIAL:
			# Earth-like range (Earth: 101325 Pa, Venus: 9.2e6 Pa)
			var log_pressure: float = rng.randf_range(3.0, 7.0)  # 1000 Pa to 10 MPa
			return pow(10.0, log_pressure)
		
		SizeCategory.Category.SUPER_EARTH:
			# Likely thick atmosphere
			var log_pressure: float = rng.randf_range(4.0, 8.0)
			return pow(10.0, log_pressure)
		
		SizeCategory.Category.MINI_NEPTUNE, SizeCategory.Category.NEPTUNE_CLASS, SizeCategory.Category.GAS_GIANT:
			# Defined at cloud tops, very high deep pressure
			# Use "1 bar" level for gas giants
			return rng.randf_range(0.5e5, 2.0e5)
		
		_:
			return EARTH_ATMOSPHERE_PA


## Generates atmospheric composition based on planet type and conditions.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param equilibrium_temp_k: Temperature in Kelvin.
## @param rng: Random number generator.
## @return: Dictionary of gas name -> fraction.
static func _generate_atmosphere_composition(
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if SizeCategory.is_gaseous(size_cat):
		# Gas/ice giant composition
		composition = _generate_gas_giant_composition(size_cat, rng)
	else:
		# Rocky planet composition
		composition = _generate_rocky_atmosphere_composition(zone, equilibrium_temp_k, rng)
	
	# Normalize to sum to 1.0
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	
	if total > 0.0:
		for gas in composition.keys():
			composition[gas] = (composition[gas] as float) / total
	
	return composition


## Generates gas giant atmosphere composition.
## @param size_cat: Size category.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_gas_giant_composition(
	size_cat: SizeCategory.Category,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if size_cat == SizeCategory.Category.GAS_GIANT:
		# Jupiter/Saturn-like: dominated by H2 and He
		composition["H2"] = rng.randf_range(0.82, 0.92)
		composition["He"] = rng.randf_range(0.06, 0.12)
		composition["CH4"] = rng.randf_range(0.001, 0.005)
		composition["NH3"] = rng.randf_range(0.0001, 0.001)
	else:
		# Neptune-class/Mini-Neptune: more heavy elements
		composition["H2"] = rng.randf_range(0.70, 0.85)
		composition["He"] = rng.randf_range(0.10, 0.20)
		composition["CH4"] = rng.randf_range(0.01, 0.05)
		composition["H2O"] = rng.randf_range(0.001, 0.01)
	
	return composition


## Generates rocky planet atmosphere composition.
## @param zone: Orbit zone.
## @param equilibrium_temp_k: Temperature in Kelvin.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_rocky_atmosphere_composition(
	zone: OrbitZone.Zone,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	# Base composition type based on zone and temperature
	var roll: float = rng.randf()
	
	if zone == OrbitZone.Zone.HOT or equilibrium_temp_k > 500.0:
		# Hot planets: CO2 dominated or stripped
		composition["CO2"] = rng.randf_range(0.80, 0.98)
		composition["N2"] = rng.randf_range(0.01, 0.15)
		composition["SO2"] = rng.randf_range(0.001, 0.05)
	elif zone == OrbitZone.Zone.TEMPERATE:
		if roll < 0.3:
			# Earth-like: N2 dominated with O2
			composition["N2"] = rng.randf_range(0.70, 0.80)
			composition["O2"] = rng.randf_range(0.15, 0.25)
			composition["Ar"] = rng.randf_range(0.005, 0.02)
			composition["CO2"] = rng.randf_range(0.0001, 0.001)
			composition["H2O"] = rng.randf_range(0.001, 0.04)
		elif roll < 0.7:
			# Venus-like: CO2 dominated
			composition["CO2"] = rng.randf_range(0.90, 0.98)
			composition["N2"] = rng.randf_range(0.02, 0.08)
			composition["SO2"] = rng.randf_range(0.0001, 0.001)
		else:
			# Mars-like: thin CO2
			composition["CO2"] = rng.randf_range(0.90, 0.97)
			composition["N2"] = rng.randf_range(0.02, 0.05)
			composition["Ar"] = rng.randf_range(0.01, 0.03)
	else:
		# Cold zone: N2/CH4 possible (Titan-like)
		if roll < 0.4:
			composition["N2"] = rng.randf_range(0.90, 0.98)
			composition["CH4"] = rng.randf_range(0.01, 0.06)
		else:
			composition["CO2"] = rng.randf_range(0.85, 0.95)
			composition["N2"] = rng.randf_range(0.03, 0.10)
	
	return composition


## Calculates greenhouse warming factor.
## @param composition: Atmospheric composition.
## @param surface_pressure_pa: Surface pressure.
## @param rng: Random number generator.
## @return: Greenhouse factor (>=1.0).
static func _calculate_greenhouse_factor(
	composition: Dictionary,
	surface_pressure_pa: float,
	rng: SeededRng
) -> float:
	# Greenhouse gases and their relative effectiveness
	var co2_fraction: float = composition.get("CO2", 0.0) as float
	var ch4_fraction: float = composition.get("CH4", 0.0) as float
	var h2o_fraction: float = composition.get("H2O", 0.0) as float
	
	# Pressure factor (more atmosphere = more greenhouse effect)
	# log10(x) = log(x) / log(10)
	var pressure_ratio: float = maxf(surface_pressure_pa / EARTH_ATMOSPHERE_PA, 0.001)
	var pressure_factor: float = log(pressure_ratio) / log(10.0)
	pressure_factor = clampf(pressure_factor, -2.0, 3.0)
	
	# Base greenhouse contribution
	# Venus: ~500K surface with ~230K equilibrium = factor of ~2.2
	# Earth: ~288K surface with ~255K equilibrium = factor of ~1.13
	var greenhouse: float = 1.0
	
	# CO2 contribution (logarithmic with concentration and pressure)
	if co2_fraction > 0.0:
		# log10(x) = log(x) / log(10)
		var co2_effect: float = 0.1 * (log(co2_fraction * 1e6 + 1) / log(10.0)) * (1.0 + pressure_factor * 0.3)
		greenhouse += co2_effect
	
	# CH4 contribution (more potent per molecule but usually trace)
	if ch4_fraction > 0.0:
		greenhouse += ch4_fraction * 25.0  # CH4 is ~25x more potent than CO2
	
	# H2O contribution
	if h2o_fraction > 0.0:
		greenhouse += h2o_fraction * 2.0
	
	# Add some variation
	var variation: float = rng.randf_range(0.9, 1.1)
	greenhouse *= variation
	
	# Clamp to reasonable range
	return clampf(greenhouse, 1.0, 3.0)
