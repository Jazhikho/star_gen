## Generates star CelestialBody objects from StarSpec.
## Produces deterministic output based on spec and seed.
class_name StarGenerator
extends RefCounted

const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")
const _star_table: GDScript = preload("res://src/domain/generation/tables/StarTable.gd")
const _generator_utils_script: GDScript = preload("res://src/domain/generation/generators/GeneratorUtils.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _versions: GDScript = preload("res://src/domain/constants/Versions.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Spectral class distribution weights (M stars are most common).
const SPECTRAL_WEIGHTS: Array[float] = [
	0.00003, # O - very rare
	0.13, # B - rare
	0.6, # A - uncommon
	3.0, # F - moderate
	7.6, # G - common (Sun-like)
	12.1, # K - very common
	76.45, # M - most common (red dwarfs)
]


## Generates a star from a specification.
## @param spec: The star specification.
## @param rng: The random number generator (will be advanced).
## @return: A new CelestialBody configured as a star.
static func generate(spec: StarSpec, rng: SeededRng) -> CelestialBody:
	# Determine spectral class
	var spectral_class: StarClass.SpectralClass = _determine_spectral_class(spec, rng)
	
	# Determine subclass (0-9)
	var subclass: int = _determine_subclass(spec, rng)
	
	# Calculate stellar properties based on class and subclass
	var mass_solar: float = _calculate_mass(spec, spectral_class, subclass, rng)
	var luminosity_solar: float = _calculate_luminosity(spec, mass_solar, rng)
	
	# Temperature is interpolated from spectral class (primary observable)
	var temperature_k: float = _calculate_temperature(spec, spectral_class, subclass, rng)
	# Radius derived from luminosity and temperature for self-consistency
	var radius_solar: float = _calculate_radius_from_luminosity_temperature(spec, luminosity_solar, temperature_k, rng)
	
	# Build spectral string
	var spectral_string: String = StarClass.build_spectral_string(spectral_class, subclass, "V")
	
	# Determine age and metallicity
	var age_years: float = _determine_age(spec, spectral_class, rng)
	var metallicity: float = _determine_metallicity(spec, rng)
	
	# Generate physical properties
	var physical: PhysicalProps = _generate_physical_props(
		spec, mass_solar, radius_solar, rng
	)
	
	# Generate stellar properties
	var stellar: StellarProps = StellarProps.new(
		luminosity_solar * StellarProps.SOLAR_LUMINOSITY_WATTS,
		temperature_k,
		spectral_string,
		"main_sequence",
		metallicity,
		age_years
	)
	
	# Generate ID and name
	var body_id: String = _generate_id(spec, rng)
	var body_name: String = _generate_name(spec, rng)
	
	# Create provenance (stars don't have context)
	var provenance: Provenance = GeneratorUtils.create_provenance(spec, null)
	
	# Assemble the celestial body
	var body: CelestialBody = CelestialBody.new(
		body_id,
		body_name,
		CelestialType.Type.STAR,
		physical,
		provenance
	)
	body.stellar = stellar
	
	return body


## Determines the spectral class from spec or random selection.
## @param spec: The star specification.
## @param rng: The random number generator.
## @return: The selected spectral class.
static func _determine_spectral_class(spec: StarSpec, rng: SeededRng) -> StarClass.SpectralClass:
	if spec.has_spectral_class():
		return spec.spectral_class as StarClass.SpectralClass
	
	# Weighted random selection based on stellar population
	var classes: Array = [
		StarClass.SpectralClass.O,
		StarClass.SpectralClass.B,
		StarClass.SpectralClass.A,
		StarClass.SpectralClass.F,
		StarClass.SpectralClass.G,
		StarClass.SpectralClass.K,
		StarClass.SpectralClass.M,
	]
	
	var selected: Variant = rng.weighted_choice(classes, SPECTRAL_WEIGHTS)
	return selected as StarClass.SpectralClass


## Determines the subclass from spec or random selection.
## @param spec: The star specification.
## @param rng: The random number generator.
## @return: The subclass (0-9).
static func _determine_subclass(spec: StarSpec, rng: SeededRng) -> int:
	if spec.has_subclass():
		return clampi(spec.subclass, 0, 9)
	return rng.randi_range(0, 9)


## Calculates stellar mass.
## @param spec: The star specification.
## @param spectral_class: The spectral class.
## @param subclass: The subclass.
## @param rng: The random number generator.
## @return: Mass in solar masses.
static func _calculate_mass(
	spec: StarSpec,
	spectral_class: StarClass.SpectralClass,
	subclass: int,
	rng: SeededRng
) -> float:
	# Check for override
	var override_mass: float = spec.get_override_float("physical.mass_solar", -1.0)
	if override_mass > 0.0:
		return override_mass
	
	var mass_range: Dictionary = StarTable.get_mass_range(spectral_class)
	var base_mass: float = StarTable.interpolate_by_subclass(spectral_class, subclass, mass_range)
	
	# Add small random variation (Â±5%)
	var variation: float = rng.randf_range(0.95, 1.05)
	return base_mass * variation


## Calculates stellar luminosity from mass.
## @param spec: The star specification.
## @param mass_solar: Mass in solar masses.
## @param rng: The random number generator.
## @return: Luminosity in solar luminosities.
static func _calculate_luminosity(
	spec: StarSpec,
	mass_solar: float,
	rng: SeededRng
) -> float:
	# Check for override
	var override_lum: float = spec.get_override_float("stellar.luminosity_solar", -1.0)
	if override_lum > 0.0:
		return override_lum
	
	var base_luminosity: float = StarTable.luminosity_from_mass(mass_solar)
	
	# Add small random variation (Â±10%)
	var variation: float = rng.randf_range(0.90, 1.10)
	return base_luminosity * variation


## Calculates stellar radius from luminosity and temperature using Stefan-Boltzmann.
## @param spec: The star specification.
## @param luminosity_solar: Luminosity in solar luminosities.
## @param temperature_k: Effective temperature in Kelvin.
## @param rng: The random number generator.
## @return: Radius in solar radii.
static func _calculate_radius_from_luminosity_temperature(
	spec: StarSpec,
	luminosity_solar: float,
	temperature_k: float,
	_rng: SeededRng
) -> float:
	# Check for override
	var override_radius: float = spec.get_override_float("physical.radius_solar", -1.0)
	if override_radius > 0.0:
		return override_radius
	
	# Stefan-Boltzmann: L = 4Ï€RÂ²ÏƒTâ´
	# Solving for R: R = sqrt(L / Tâ´) * constant
	# In solar units: R/R_sun = sqrt(L/L_sun) * (T_sun/T)Â²
	var t_sun: float = 5778.0
	var radius_solar: float = sqrt(luminosity_solar) * pow(t_sun / temperature_k, 2.0)
	
	return radius_solar


## Calculates effective temperature from spectral class interpolation.
## @param spec: The star specification.
## @param spectral_class: The spectral class.
## @param subclass: The subclass (0-9).
## @param rng: The random number generator.
## @return: Temperature in Kelvin.
static func _calculate_temperature(
	spec: StarSpec,
	spectral_class: StarClass.SpectralClass,
	subclass: int,
	rng: SeededRng
) -> float:
	# Check for override
	var override_temp: float = spec.get_override_float("stellar.temperature_k", -1.0)
	if override_temp > 0.0:
		return override_temp
	
	var temp_range: Dictionary = StarTable.get_temperature_range(spectral_class)
	var base_temp: float = StarTable.interpolate_by_subclass(spectral_class, subclass, temp_range)
	
	# Add small random variation (Â±3%)
	var variation: float = rng.randf_range(0.97, 1.03)
	return base_temp * variation


## Determines stellar age.
## @param spec: The star specification.
## @param spectral_class: The spectral class.
## @param rng: The random number generator.
## @return: Age in years.
static func _determine_age(
	spec: StarSpec,
	spectral_class: StarClass.SpectralClass,
	rng: SeededRng
) -> float:
	if spec.has_age():
		return spec.age_years
	
	var lifetime_range: Dictionary = StarTable.get_lifetime_range(spectral_class)
	var max_age: float = lifetime_range["max"] * 0.9 # Don't go past main sequence
	var min_age: float = lifetime_range["min"] * 0.1 # Some minimum formation time
	
	# Bias toward younger ages within lifespan
	var raw: float = rng.randf()
	var biased: float = pow(raw, 0.7) # Slight bias toward younger
	
	return lerpf(min_age, max_age, biased)


## Determines stellar metallicity.
## @param spec: The star specification.
## @param rng: The random number generator.
## @return: Metallicity relative to solar.
static func _determine_metallicity(spec: StarSpec, rng: SeededRng) -> float:
	if spec.has_metallicity():
		return spec.metallicity
	
	# Most stars have near-solar metallicity, with spread
	# Log-normal distribution centered on 1.0
	var log_z: float = rng.randfn(0.0, 0.2)
	return clampf(exp(log_z), 0.1, 3.0)


## Generates physical properties for the star.
## @param spec: The star specification.
## @param mass_solar: Mass in solar masses.
## @param radius_solar: Radius in solar radii.
## @param rng: The random number generator.
## @return: PhysicalProps for the star.
static func _generate_physical_props(
	spec: StarSpec,
	mass_solar: float,
	radius_solar: float,
	rng: SeededRng
) -> PhysicalProps:
	var mass_kg: float = spec.get_override_float(
		"physical.mass_kg",
		mass_solar * Units.SOLAR_MASS_KG
	)
	
	var radius_m: float = spec.get_override_float(
		"physical.radius_m",
		radius_solar * Units.SOLAR_RADIUS_METERS
	)
	
	# Stellar rotation period (days -> seconds)
	# Sun rotates in ~25 days at equator
	# Faster rotation for younger/more massive stars
	var rotation_days: float = rng.randf_range(10.0, 50.0)
	var rotation_period_s: float = spec.get_override_float(
		"physical.rotation_period_s",
		rotation_days * 24.0 * 3600.0
	)
	
	# Axial tilt (stars can have any tilt, but usually measured from ecliptic)
	var axial_tilt_deg: float = spec.get_override_float(
		"physical.axial_tilt_deg",
		rng.randf_range(0.0, 30.0)
	)
	
	# Oblateness (very small for most stars)
	var oblateness: float = spec.get_override_float(
		"physical.oblateness",
		rng.randf_range(0.0, 0.001)
	)
	
	# Stars have strong magnetic fields
	var magnetic_moment: float = spec.get_override_float(
		"physical.magnetic_moment",
		rng.randf_range(1.0e22, 1.0e26)
	)
	
	# Internal heat (stars are self-luminous)
	var internal_heat_watts: float = spec.get_override_float(
		"physical.internal_heat_watts",
		0.0 # Handled by luminosity for stars
	)
	
	return PhysicalProps.new(
		mass_kg,
		radius_m,
		rotation_period_s,
		axial_tilt_deg,
		oblateness,
		magnetic_moment,
		internal_heat_watts
	)


## Generates an ID for the star.
## @param spec: The star specification.
## @param rng: The random number generator.
## @return: The body ID.
static func _generate_id(spec: StarSpec, rng: SeededRng) -> String:
	var override_id: Variant = spec.get_override("id", null)
	if override_id != null and override_id is String and not (override_id as String).is_empty():
		return override_id as String
	return _generator_utils_script.generate_id("star", rng)


## Generates a name for the star.
## @param spec: The star specification.
## @param _rng: The random number generator (unused, kept for signature consistency).
## @return: The body name (from name_hint if provided, otherwise empty string).
static func _generate_name(spec: StarSpec, _rng: SeededRng) -> String:
	return spec.name_hint
