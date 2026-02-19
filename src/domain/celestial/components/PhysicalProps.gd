## Physical properties of a celestial body.
## Contains mass, radius, rotation, and derived quantities.
class_name PhysicalProps
extends RefCounted


## Gravitational constant in m^3 kg^-1 s^-2.
const G: float = 6.674e-11


## Mass in kilograms.
var mass_kg: float

## Radius in meters.
var radius_m: float

## Rotation period in seconds. Negative values indicate retrograde rotation.
var rotation_period_s: float

## Axial tilt in degrees (0-180).
var axial_tilt_deg: float

## Oblateness (flattening). 0 = perfect sphere, Jupiter = ~0.065.
var oblateness: float

## Magnetic dipole moment in Tesla * m^3.
var magnetic_moment: float

## Internal heat flow in watts.
var internal_heat_watts: float


## Creates a new PhysicalProps instance.
## @param p_mass_kg: Mass in kilograms.
## @param p_radius_m: Radius in meters.
## @param p_rotation_period_s: Rotation period in seconds.
## @param p_axial_tilt_deg: Axial tilt in degrees.
## @param p_oblateness: Oblateness/flattening factor.
## @param p_magnetic_moment: Magnetic dipole moment.
## @param p_internal_heat_watts: Internal heat generation.
func _init(
	p_mass_kg: float = 0.0,
	p_radius_m: float = 0.0,
	p_rotation_period_s: float = 0.0,
	p_axial_tilt_deg: float = 0.0,
	p_oblateness: float = 0.0,
	p_magnetic_moment: float = 0.0,
	p_internal_heat_watts: float = 0.0
) -> void:
	mass_kg = p_mass_kg
	radius_m = p_radius_m
	rotation_period_s = p_rotation_period_s
	axial_tilt_deg = p_axial_tilt_deg
	oblateness = p_oblateness
	magnetic_moment = p_magnetic_moment
	internal_heat_watts = p_internal_heat_watts


## Calculates the volume in cubic meters.
## @return: Volume in m^3.
func get_volume_m3() -> float:
	if radius_m <= 0.0:
		return 0.0
	return (4.0 / 3.0) * PI * pow(radius_m, 3.0)


## Calculates the mean density in kg/m^3.
## @return: Density in kg/m^3.
func get_density_kg_m3() -> float:
	var volume: float = get_volume_m3()
	if volume <= 0.0:
		return 0.0
	return mass_kg / volume


## Calculates surface gravity in m/s^2.
## @return: Surface gravity in m/s^2.
func get_surface_gravity_m_s2() -> float:
	if radius_m <= 0.0:
		return 0.0
	return G * mass_kg / (radius_m * radius_m)


## Calculates escape velocity in m/s.
## @return: Escape velocity in m/s.
func get_escape_velocity_m_s() -> float:
	if radius_m <= 0.0:
		return 0.0
	return sqrt(2.0 * G * mass_kg / radius_m)


## Returns the equatorial radius accounting for oblateness.
## @return: Equatorial radius in meters.
func get_equatorial_radius_m() -> float:
	if oblateness <= 0.0:
		return radius_m
	return radius_m / (1.0 - oblateness)


## Returns the polar radius accounting for oblateness.
## @return: Polar radius in meters.
func get_polar_radius_m() -> float:
	if oblateness <= 0.0:
		return radius_m
	return radius_m * (1.0 - oblateness)


## Converts this component to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"mass_kg": mass_kg,
		"radius_m": radius_m,
		"rotation_period_s": rotation_period_s,
		"axial_tilt_deg": axial_tilt_deg,
		"oblateness": oblateness,
		"magnetic_moment": magnetic_moment,
		"internal_heat_watts": internal_heat_watts,
	}


## Creates a PhysicalProps from a dictionary.
## @param data: The dictionary to parse.
## @return: A new PhysicalProps instance.
static func from_dict(data: Dictionary) -> PhysicalProps:
	var script_class: GDScript = load("res://src/domain/celestial/components/PhysicalProps.gd") as GDScript
	return script_class.new(
		data.get("mass_kg", 0.0) as float,
		data.get("radius_m", 0.0) as float,
		data.get("rotation_period_s", 0.0) as float,
		data.get("axial_tilt_deg", 0.0) as float,
		data.get("oblateness", 0.0) as float,
		data.get("magnetic_moment", 0.0) as float,
		data.get("internal_heat_watts", 0.0) as float
	) as PhysicalProps
