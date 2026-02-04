## Generates atmosphere properties for moons.
## Handles atmospheric retention and composition for large moons.
class_name MoonAtmosphereGenerator
extends RefCounted

const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _atmosphere_props: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _atmosphere_utils: GDScript = preload("res://src/domain/generation/utils/AtmosphereUtils.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")

## Boltzmann constant in J/K.
const BOLTZMANN_K: float = 1.380649e-23

## Nitrogen molecular mass in kg.
const NITROGEN_MASS_KG: float = 4.6518e-26


## Generates atmosphere properties for a moon.
## @param spec: Moon specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param equilibrium_temp_k: Equilibrium temperature.
## @param rng: Random number generator.
## @return: AtmosphereProps or null.
static func generate_atmosphere(
	spec: MoonSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> AtmosphereProps:
	# Moon atmospheres are typically thin and N2/CH4 dominated (Titan-like)
	
	# Surface pressure
	var surface_pressure_pa: float = spec.get_override_float("atmosphere.surface_pressure_pa", -1.0)
	if surface_pressure_pa < 0.0:
		match size_cat:
			SizeCategory.Category.SUB_TERRESTRIAL:
				# Titan: ~1.5 bar
				surface_pressure_pa = rng.randf_range(1000.0, 200000.0)
			SizeCategory.Category.TERRESTRIAL:
				surface_pressure_pa = rng.randf_range(10000.0, 500000.0)
			SizeCategory.Category.SUPER_EARTH:
				surface_pressure_pa = rng.randf_range(50000.0, 1000000.0)
			_:
				surface_pressure_pa = rng.randf_range(100.0, 10000.0)
	
	# Composition - typically N2/CH4 for cold moons
	var composition: Dictionary = {}
	if equilibrium_temp_k < 200.0:
		# Very cold: Titan-like N2/CH4
		composition["N2"] = rng.randf_range(0.90, 0.98)
		composition["CH4"] = rng.randf_range(0.01, 0.06)
		composition["Ar"] = rng.randf_range(0.001, 0.01)
	else:
		# Warmer: more varied
		var roll: float = rng.randf()
		if roll < 0.5:
			composition["N2"] = rng.randf_range(0.70, 0.90)
			composition["CO2"] = rng.randf_range(0.05, 0.20)
		else:
			composition["CO2"] = rng.randf_range(0.85, 0.95)
			composition["N2"] = rng.randf_range(0.03, 0.10)
	
	# Normalize composition
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	if total > 0.0:
		for gas in composition.keys():
			composition[gas] = (composition[gas] as float) / total
	
	# Scale height
	var avg_molecular_mass: float = _atmosphere_utils.get_average_molecular_mass(composition)
	var gravity: float = physical.get_surface_gravity_m_s2()
	var scale_height_m: float = 0.0
	if gravity > 0.0 and avg_molecular_mass > 0.0:
		scale_height_m = BOLTZMANN_K * equilibrium_temp_k / (avg_molecular_mass * gravity)
	
	# Greenhouse factor (modest for thin atmospheres)
	var greenhouse_factor: float = 1.0 + rng.randf_range(0.0, 0.2)
	
	return AtmosphereProps.new(
		surface_pressure_pa,
		scale_height_m,
		composition,
		greenhouse_factor
	)


## Determines whether this moon should have an atmosphere.
## @param spec: The moon specification.
## @param physical: The physical properties.
## @param size_cat: The size category.
## @param context: The parent context.
## @param rng: Random number generator.
## @return: True if atmosphere should be generated.
static func should_have_atmosphere(
	spec: MoonSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	context: ParentContext,
	rng: SeededRng
) -> bool:
	# Check spec preference
	if spec.has_atmosphere_preference():
		return spec.has_atmosphere as bool
	
	# Most moons don't have atmospheres
	# Only large moons (Titan-sized) can retain significant atmospheres
	
	var escape_velocity: float = physical.get_escape_velocity_m_s()
	var equilibrium_temp: float = context.get_equilibrium_temperature_k(0.3)
	
	# Thermal velocity for nitrogen (Titan's main gas)
	var thermal_velocity: float = sqrt(3.0 * BOLTZMANN_K * equilibrium_temp / NITROGEN_MASS_KG)
	
	var jeans_param: float = escape_velocity / thermal_velocity
	
	# Need jeans > 6 for long-term retention
	if jeans_param < 6.0:
		return false
	
	# Size requirement: at least sub-terrestrial
	if size_cat == SizeCategory.Category.DWARF:
		return false
	
	# Probability based on size
	var prob: float
	match size_cat:
		SizeCategory.Category.SUB_TERRESTRIAL:
			prob = 0.1  # Rare (only Titan in our solar system)
		SizeCategory.Category.TERRESTRIAL:
			prob = 0.3
		SizeCategory.Category.SUPER_EARTH:
			prob = 0.5
		_:
			prob = 0.0
	
	return rng.randf() < prob
