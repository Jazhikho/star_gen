## Converts Traveller UWP size codes into property constraint bounds.
## Pure domain logic: feeds into PropertyConstraintSolver as extra narrowing.
##
## Traveller size codes define diameter ranges in km. This class:
##   1. Looks up the diameter range for a code via TravellerSizeCode
##   2. Converts diameter -> radius in metres
##   3. Derives a plausible mass window using category density ranges
##
## Output is a dictionary of property_path -> Vector2(min, max) in base SI units,
## ready to pass to PropertyConstraintSolver.solve_with_extra_constraints().
class_name TravellerConstraintBuilder
extends RefCounted

const _traveller_size: GDScript = preload("res://src/domain/generation/archetypes/TravellerSizeCode.gd")
const _size_table: GDScript = preload("res://src/domain/generation/tables/SizeTable.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Synthetic upper diameter for code E (spec says "120 000 km +"; we pick a
## generous but finite ceiling so the solver can actually intersect ranges).
const _CODE_E_DIAMETER_MAX_KM: float = 300000.0


## Builds property-path -> Vector2(min, max) constraints for a Traveller size code.
## @param size_code: Traveller UWP size code (int 0-9 or String "A"-"E").
## @return: Dict with "physical.radius_m" and "physical.mass_kg" ranges; empty dict if code invalid.
static func build_constraints_for_size(size_code: Variant) -> Dictionary:
	var diam_range: Dictionary = TravellerSizeCode.code_to_diameter_range(size_code)
	if diam_range.is_empty():
		return {}

	var diam_min_km: float = diam_range.get("min", 0.0) as float
	var diam_max_km: float = diam_range.get("max", -1.0) as float
	# Code E reports max = -1 (unbounded); cap it so solver intersection works.
	if diam_max_km < 0.0:
		diam_max_km = _CODE_E_DIAMETER_MAX_KM

	# Diameter (km) -> radius (m): radius_m = (diam_km / 2) * 1000 = diam_km * 500.
	var r_min_m: float = diam_min_km * 500.0
	var r_max_m: float = diam_max_km * 500.0

	# Derive mass window from radius window + density bounds.
	var mass_bounds: Vector2 = _mass_range_for_radius_window(r_min_m, r_max_m)

	return {
		"physical.radius_m": Vector2(r_min_m, r_max_m),
		"physical.mass_kg": mass_bounds,
	}


## Returns the Traveller size code for a given radius in metres.
## Convenience wrapper so callers don't need to do unit conversion.
## @param radius_m: Radius in metres.
## @return: Traveller size code (int 0-9 or String "A"-"E").
static func code_for_radius(radius_m: float) -> Variant:
	var diam_km: float = radius_m * 2.0 / 1000.0
	return TravellerSizeCode.diameter_km_to_code(diam_km)


## Returns all valid size codes as an ordered array.
## @return: Array of Variants (int 0-9 followed by Strings A-E).
static func all_codes() -> Array:
	var codes: Array = []
	for i: int in range(10):
		codes.append(i)
	for s: String in ["A", "B", "C", "D", "E"]:
		codes.append(s)
	return codes


## Formats a code as its UWP single-character digit plus a human diameter range.
## @param code: Size code (int or String).
## @return: Display string like "7 (10 400 - 12 000 km)".
static func describe_code(code: Variant) -> String:
	var uwp: String = TravellerSizeCode.to_string_uwp(code)
	var r: Dictionary = TravellerSizeCode.code_to_diameter_range(code)
	if r.is_empty():
		return uwp
	var lo: float = r.get("min", 0.0) as float
	var hi: float = r.get("max", -1.0) as float
	if hi < 0.0:
		return "%s (%s km +)" % [uwp, _fmt_km(lo)]
	return "%s (%s - %s km)" % [uwp, _fmt_km(lo), _fmt_km(hi)]


## Derives a mass window spanning the full plausible density range
## across the given radius window.
## @param r_min_m: Minimum radius in metres.
## @param r_max_m: Maximum radius in metres.
## @return: Vector2(min_mass_kg, max_mass_kg).
static func _mass_range_for_radius_window(r_min_m: float, r_max_m: float) -> Vector2:
	# Smallest mass: smallest radius at lowest plausible density for that size.
	var cat_min: int = _category_from_radius(r_min_m)
	var dens_min_range: Dictionary = SizeTable.get_density_range(cat_min)
	var d_lo: float = dens_min_range.get("min", 500.0) as float
	var m_min: float = _mass_from_radius_density(r_min_m, d_lo)

	# Largest mass: largest radius at highest plausible density for that size.
	var cat_max: int = _category_from_radius(r_max_m)
	var dens_max_range: Dictionary = SizeTable.get_density_range(cat_max)
	var d_hi: float = dens_max_range.get("max", 8000.0) as float
	var m_max: float = _mass_from_radius_density(r_max_m, d_hi)

	return Vector2(m_min, m_max)


## Resolves a SizeTable category from a radius in metres.
## @param radius_m: Radius in metres.
## @return: SizeCategory.Category enum value.
static func _category_from_radius(radius_m: float) -> int:
	# Estimate mass assuming mid-range rocky density, then classify.
	var guess_density: float = 3500.0
	var guess_mass: float = _mass_from_radius_density(radius_m, guess_density)
	return SizeTable.category_from_mass(guess_mass / Units.EARTH_MASS_KG)


## Computes sphere mass from radius and density.
## @param radius_m: Radius in metres.
## @param density_kg_m3: Density in kg/m^3.
## @return: Mass in kg.
static func _mass_from_radius_density(radius_m: float, density_kg_m3: float) -> float:
	var vol: float = (4.0 / 3.0) * PI * pow(radius_m, 3.0)
	return vol * density_kg_m3


## Formats a km value without trailing decimals.
## @param km: Distance in kilometres.
## @return: Formatted integer-like string.
static func _fmt_km(km: float) -> String:
	return str(int(roundf(km)))
