## Orbital mechanics calculations for solar system generation.
## Provides utilities for stability limits, resonances, zones, and n-body approximations.
class_name OrbitalMechanics
extends RefCounted

const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


## Gravitational constant in m^3 kg^-1 s^-2.
const G: float = 6.674e-11

## Approximate stability factor for S-type orbits (Holman & Wiegert 1999).
## Planet can orbit star in binary if a_planet < separation * S_CRIT / (1 + ecc_binary).
const S_TYPE_CRITICAL_RATIO: float = 0.4

## Approximate stability factor for P-type orbits (Holman & Wiegert 1999).
## Planet can orbit binary if a_planet > separation * P_CRIT * (1 + ecc_binary).
const P_TYPE_CRITICAL_RATIO: float = 2.5


# =============================================================================
# BASIC ORBITAL MECHANICS
# =============================================================================


## Calculates orbital period using Kepler's third law.
## T = 2Ï€ * sqrt(a^3 / (G * M))
## @param semi_major_axis_m: Semi-major axis in meters.
## @param central_mass_kg: Mass of central body in kg.
## @return: Orbital period in seconds.
static func calculate_orbital_period(
	semi_major_axis_m: float,
	central_mass_kg: float
) -> float:
	if semi_major_axis_m <= 0.0 or central_mass_kg <= 0.0:
		return 0.0
	return 2.0 * PI * sqrt(pow(semi_major_axis_m, 3.0) / (G * central_mass_kg))


## Calculates semi-major axis from orbital period (inverse of Kepler's third law).
## a = (G * M * T^2 / (4Ï€^2))^(1/3)
## @param period_s: Orbital period in seconds.
## @param central_mass_kg: Mass of central body in kg.
## @return: Semi-major axis in meters.
static func calculate_semi_major_axis(
	period_s: float,
	central_mass_kg: float
) -> float:
	if period_s <= 0.0 or central_mass_kg <= 0.0:
		return 0.0
	return pow((G * central_mass_kg * pow(period_s, 2.0)) / (4.0 * pow(PI, 2.0)), 1.0 / 3.0)


## Calculates orbital velocity at a given distance (circular orbit approximation).
## v = sqrt(G * M / r)
## @param distance_m: Distance from central body in meters.
## @param central_mass_kg: Central body mass in kg.
## @return: Orbital velocity in m/s.
static func calculate_orbital_velocity(
	distance_m: float,
	central_mass_kg: float
) -> float:
	if distance_m <= 0.0 or central_mass_kg <= 0.0:
		return 0.0
	return sqrt(G * central_mass_kg / distance_m)


## Calculates escape velocity from a body's surface.
## v_esc = sqrt(2 * G * M / R)
## @param body_mass_kg: Body mass in kg.
## @param body_radius_m: Body radius in meters.
## @return: Escape velocity in m/s.
static func calculate_escape_velocity(
	body_mass_kg: float,
	body_radius_m: float
) -> float:
	if body_mass_kg <= 0.0 or body_radius_m <= 0.0:
		return 0.0
	return sqrt(2.0 * G * body_mass_kg / body_radius_m)


## Calculates mean motion (angular velocity) from semi-major axis and central mass.
## n = sqrt(G * M / a^3)  [radians per second]
## @param semi_major_axis_m: Semi-major axis in meters.
## @param central_mass_kg: Central body mass in kg.
## @return: Mean motion in radians per second.
static func calculate_mean_motion(
	semi_major_axis_m: float,
	central_mass_kg: float
) -> float:
	if semi_major_axis_m <= 0.0 or central_mass_kg <= 0.0:
		return 0.0
	return sqrt(G * central_mass_kg / pow(semi_major_axis_m, 3.0))


# =============================================================================
# GRAVITATIONAL INFLUENCE REGIONS
# =============================================================================


## Calculates the Hill sphere radius for a body orbiting a more massive primary.
## R_Hill = a * (m / (3M))^(1/3)
## @param body_mass_kg: Mass of the orbiting body.
## @param primary_mass_kg: Mass of the primary body.
## @param semi_major_axis_m: Orbital distance from primary.
## @return: Hill sphere radius in meters.
static func calculate_hill_sphere(
	body_mass_kg: float,
	primary_mass_kg: float,
	semi_major_axis_m: float
) -> float:
	if body_mass_kg <= 0.0 or primary_mass_kg <= 0.0 or semi_major_axis_m <= 0.0:
		return 0.0
	var mass_ratio: float = body_mass_kg / (3.0 * primary_mass_kg)
	return semi_major_axis_m * pow(mass_ratio, 1.0 / 3.0)


## Calculates the Roche limit for a fluid body.
## d = 2.44 * R_primary * (Ï_primary / Ï_satellite)^(1/3)
## @param primary_radius_m: Radius of primary body in meters.
## @param primary_density_kg_m3: Density of primary in kg/m^3.
## @param satellite_density_kg_m3: Density of satellite in kg/m^3.
## @return: Roche limit distance in meters.
static func calculate_roche_limit(
	primary_radius_m: float,
	primary_density_kg_m3: float,
	satellite_density_kg_m3: float
) -> float:
	if primary_radius_m <= 0.0 or primary_density_kg_m3 <= 0.0 or satellite_density_kg_m3 <= 0.0:
		return 0.0
	return 2.44 * primary_radius_m * pow(primary_density_kg_m3 / satellite_density_kg_m3, 1.0 / 3.0)


## Calculates the Roche limit from mass and radius.
## Convenience function that computes density internally.
## @param primary_mass_kg: Mass of the primary body in kg.
## @param primary_radius_m: Radius of the primary body in meters.
## @param satellite_density_kg_m3: Density of the satellite in kg/m^3.
## @return: Roche limit in meters.
static func calculate_roche_limit_from_mass(
	primary_mass_kg: float,
	primary_radius_m: float,
	satellite_density_kg_m3: float
) -> float:
	if primary_radius_m <= 0.0:
		return 0.0
	var parent_volume: float = (4.0 / 3.0) * PI * pow(primary_radius_m, 3.0)
	var parent_density: float = primary_mass_kg / parent_volume
	return calculate_roche_limit(primary_radius_m, parent_density, satellite_density_kg_m3)


## Calculates the sphere of influence (SOI) radius for a body.
## R_SOI â‰ˆ a * (m / M)^(2/5)  (Laplace approximation)
## @param body_mass_kg: Mass of the body.
## @param primary_mass_kg: Mass of the primary it orbits.
## @param semi_major_axis_m: Distance from primary.
## @return: Sphere of influence radius in meters.
static func calculate_sphere_of_influence(
	body_mass_kg: float,
	primary_mass_kg: float,
	semi_major_axis_m: float
) -> float:
	if body_mass_kg <= 0.0 or primary_mass_kg <= 0.0 or semi_major_axis_m <= 0.0:
		return 0.0
	return semi_major_axis_m * pow(body_mass_kg / primary_mass_kg, 2.0 / 5.0)


# =============================================================================
# BINARY SYSTEM DYNAMICS
# =============================================================================


## Calculates the barycenter distance from body A in a binary pair.
## d_A = separation * M_B / (M_A + M_B)
## @param mass_a_kg: Mass of body A.
## @param mass_b_kg: Mass of body B.
## @param separation_m: Distance between A and B.
## @return: Distance from A to barycenter in meters.
static func calculate_barycenter_from_a(
	mass_a_kg: float,
	mass_b_kg: float,
	separation_m: float
) -> float:
	if mass_a_kg <= 0.0 or mass_b_kg <= 0.0 or separation_m <= 0.0:
		return 0.0
	return separation_m * mass_b_kg / (mass_a_kg + mass_b_kg)


## Calculates the maximum stable orbital distance for S-type (circumstellar) orbits.
## Based on Holman & Wiegert (1999) empirical formula.
## @param binary_separation_m: Distance between binary stars.
## @param mass_ratio: Ratio of companion mass to primary mass (M_companion / M_primary).
## @param binary_eccentricity: Eccentricity of the binary orbit.
## @return: Maximum stable planet distance from primary star in meters.
static func calculate_stype_stability_limit(
	binary_separation_m: float,
	mass_ratio: float,
	binary_eccentricity: float
) -> float:
	if binary_separation_m <= 0.0:
		return 0.0
	
	# Empirical fit from Holman & Wiegert 1999
	# a_crit â‰ˆ (0.464 - 0.380*Î¼ - 0.631*e + 0.586*Î¼*e + 0.150*e^2 - 0.198*Î¼*e^2) * a_binary
	# where Î¼ = M_companion / (M_primary + M_companion)
	var mu: float = mass_ratio / (1.0 + mass_ratio)
	var e: float = clampf(binary_eccentricity, 0.0, 0.99)
	
	var coeff: float = 0.464 - 0.380 * mu - 0.631 * e + 0.586 * mu * e + 0.150 * e * e - 0.198 * mu * e * e
	
	# Apply safety margin (use 90% of critical value)
	return coeff * binary_separation_m * 0.9


## Calculates the minimum stable orbital distance for P-type (circumbinary) orbits.
## Based on Holman & Wiegert (1999) empirical formula.
## @param binary_separation_m: Distance between binary stars.
## @param mass_ratio: Ratio of secondary mass to primary mass.
## @param binary_eccentricity: Eccentricity of the binary orbit.
## @return: Minimum stable planet distance from barycenter in meters.
static func calculate_ptype_stability_limit(
	binary_separation_m: float,
	mass_ratio: float,
	binary_eccentricity: float
) -> float:
	if binary_separation_m <= 0.0:
		return 0.0
	
	# Empirical fit from Holman & Wiegert 1999
	# a_crit â‰ˆ (1.60 + 5.10*e - 2.22*e^2 + 4.12*Î¼ - 4.27*e*Î¼ - 5.09*Î¼^2 + 4.61*e^2*Î¼^2) * a_binary
	var mu: float = mass_ratio / (1.0 + mass_ratio)
	var e: float = clampf(binary_eccentricity, 0.0, 0.99)
	
	var coeff: float = 1.60 + 5.10 * e - 2.22 * e * e + 4.12 * mu - 4.27 * e * mu - 5.09 * mu * mu + 4.61 * e * e * mu * mu
	
	# Apply safety margin (use 110% of critical value for inner limit)
	return coeff * binary_separation_m * 1.1


# =============================================================================
# OUTER ORBIT LIMITS (TIDAL VS FORMATION)
# =============================================================================


## Jacobi (tidal) radius: where the Galaxy's tidal field competes with the star's gravity.
## Scales as M_star^(1/3). Beyond this, companions are barely bound (Oort-like).
## Solar neighbourhood; r_J ≈ 1.70 pc * (M_tot / (2 M_sun))^(1/3); for a planet M_tot ≈ M_star.
## @param stellar_mass_kg: Mass of the primary (star or binary) in kg.
## @return: Jacobi radius in meters (~1e5–5e5 AU for 0.1–5 M_sun).
static func calculate_jacobi_radius_m(stellar_mass_kg: float) -> float:
	if stellar_mass_kg <= 0.0:
		return 0.0
	var m_solar: float = stellar_mass_kg / Units.SOLAR_MASS_KG
	var r_j_pc: float = 1.70 * pow(m_solar / 2.0, 1.0 / 3.0)
	return r_j_pc * Units.PARSEC_METERS


## Formation-based outer disc limit: typical dust disc extent scales as M_star^0.6 (Taurus/Lupus).
## Most "formed in the disc" planets lie inside tens to a few hundred AU; 67% of Lupus discs have R_dust < 30 AU.
## @param stellar_mass_kg: Mass of the primary in kg.
## @param base_au_at_1_solar: Outer limit in AU at 1 M_sun (default 100 AU).
## @return: Formation ceiling in meters.
static func calculate_formation_outer_limit_m(
	stellar_mass_kg: float,
	base_au_at_1_solar: float = 100.0
) -> float:
	if stellar_mass_kg <= 0.0:
		return 0.0
	var m_solar: float = stellar_mass_kg / Units.SOLAR_MASS_KG
	var r_au: float = base_au_at_1_solar * pow(m_solar, 0.6)
	return r_au * Units.AU_METERS


## Recommended outer stability limit for planet-forming orbits: min(formation, Jacobi).
## Formation is the usual limiter; Jacobi is the dynamical ceiling (beyond which binding is marginal).
## @param stellar_mass_kg: Mass of the primary in kg.
## @param formation_base_au: Formation limit in AU at 1 M_sun (default 100).
## @return: Outer limit in meters.
static func calculate_outer_stability_limit_m(
	stellar_mass_kg: float,
	formation_base_au: float = 100.0
) -> float:
	var formation_m: float = calculate_formation_outer_limit_m(stellar_mass_kg, formation_base_au)
	var jacobi_m: float = calculate_jacobi_radius_m(stellar_mass_kg)
	return minf(formation_m, jacobi_m)


## Calculates the binary orbital period.
## @param separation_m: Binary separation in meters.
## @param mass_a_kg: Mass of primary in kg.
## @param mass_b_kg: Mass of secondary in kg.
## @return: Binary orbital period in seconds.
static func calculate_binary_period(
	separation_m: float,
	mass_a_kg: float,
	mass_b_kg: float
) -> float:
	var total_mass: float = mass_a_kg + mass_b_kg
	return calculate_orbital_period(separation_m, total_mass)


# =============================================================================
# ORBITAL ZONES
# =============================================================================


## Calculates the inner edge of the habitable zone.
## Based on empirical formula: HZ_inner â‰ˆ 0.95 AU * sqrt(L/L_sun)
## @param luminosity_watts: Stellar luminosity in watts.
## @return: Inner HZ edge in meters.
static func calculate_habitable_zone_inner(luminosity_watts: float) -> float:
	if luminosity_watts <= 0.0:
		return 0.0
	var l_solar: float = luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	return 0.95 * Units.AU_METERS * sqrt(l_solar)


## Calculates the outer edge of the habitable zone.
## Based on empirical formula: HZ_outer â‰ˆ 1.37 AU * sqrt(L/L_sun)
## @param luminosity_watts: Stellar luminosity in watts.
## @return: Outer HZ edge in meters.
static func calculate_habitable_zone_outer(luminosity_watts: float) -> float:
	if luminosity_watts <= 0.0:
		return 0.0
	var l_solar: float = luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	return 1.37 * Units.AU_METERS * sqrt(l_solar)


## Calculates the frost line distance.
## Based on empirical formula: frost_line â‰ˆ 2.7 AU * sqrt(L/L_sun)
## @param luminosity_watts: Stellar luminosity in watts.
## @return: Frost line distance in meters.
static func calculate_frost_line(luminosity_watts: float) -> float:
	if luminosity_watts <= 0.0:
		return 0.0
	var l_solar: float = luminosity_watts / StellarProps.SOLAR_LUMINOSITY_WATTS
	return 2.7 * Units.AU_METERS * sqrt(l_solar)


## Determines which orbital zone a distance falls into.
## @param distance_m: Orbital distance in meters.
## @param luminosity_watts: Stellar luminosity in watts.
## @return: OrbitZone.Zone enum value.
static func get_orbital_zone(distance_m: float, luminosity_watts: float) -> OrbitZone.Zone:
	var hz_inner: float = calculate_habitable_zone_inner(luminosity_watts)
	var frost_line: float = calculate_frost_line(luminosity_watts)
	
	if distance_m < hz_inner:
		return OrbitZone.Zone.HOT
	elif distance_m > frost_line:
		return OrbitZone.Zone.COLD
	else:
		return OrbitZone.Zone.TEMPERATE


# =============================================================================
# RESONANCE AND SPACING
# =============================================================================


## Calculates the next orbital distance at a given resonance ratio.
## Uses flexible spacing with variation.
## @param inner_orbit_m: Inner orbit distance in meters.
## @param ratio: Resonance ratio (e.g., 2.0 for 2:1, 1.5 for 3:2).
## @param variation: Fractional variation (0.0 = exact, 0.2 = Â±20%).
## @param rng: Random number generator for variation.
## @return: Next orbit distance in meters.
static func calculate_resonance_spacing(
	inner_orbit_m: float,
	ratio: float,
	variation: float,
	rng: SeededRng
) -> float:
	if inner_orbit_m <= 0.0 or ratio <= 1.0:
		return inner_orbit_m
	
	# a_outer = a_inner * ratio^(2/3)  (from Kepler's 3rd law: P^2 âˆ a^3, so P_ratio = a_ratio^(3/2))
	var base_distance: float = inner_orbit_m * pow(ratio, 2.0 / 3.0)
	
	if variation <= 0.0:
		return base_distance
	
	# Apply random variation
	var var_factor: float = rng.randf_range(1.0 - variation, 1.0 + variation)
	return base_distance * var_factor


## Returns common resonance ratios for planetary systems.
## @return: Array of resonance ratios.
static func get_common_resonance_ratios() -> Array[float]:
	return [
		2.0, # 2:1 (very common)
		1.5, # 3:2 (common, Mercury-like)
		1.67, # 5:3 (Pluto-Neptune)
		1.4, # 7:5
		1.6, # 8:5
		1.25, # 5:4
		1.33, # 4:3
	]


## Calculates the distance ratio for a given period ratio.
## @param period_ratio: Ratio of periods.
## @return: Ratio of semi-major axes.
static func period_ratio_to_distance_ratio(period_ratio: float) -> float:
	if period_ratio <= 0.0:
		return 0.0
	return pow(period_ratio, 2.0 / 3.0)


## Calculates the period ratio for a given distance ratio.
## @param distance_ratio: Ratio of semi-major axes.
## @return: Ratio of periods.
static func distance_ratio_to_period_ratio(distance_ratio: float) -> float:
	if distance_ratio <= 0.0:
		return 0.0
	return pow(distance_ratio, 1.5)


## Estimates minimum safe spacing between adjacent planetary orbits.
## Rule of thumb: ~10 mutual Hill radii separation for long-term stability (Chambers 1996).
## There is no single observed "max planets per star"; count is limited by spacing and formation, not a fixed cap.
## @param inner_planet_mass_kg: Mass of inner planet.
## @param outer_planet_mass_kg: Mass of outer planet.
## @param star_mass_kg: Mass of the central star.
## @param inner_orbit_m: Semi-major axis of inner planet.
## @return: Minimum recommended spacing in meters.
static func calculate_minimum_planet_spacing(
	inner_planet_mass_kg: float,
	outer_planet_mass_kg: float,
	star_mass_kg: float,
	inner_orbit_m: float
) -> float:
	if inner_orbit_m <= 0.0 or star_mass_kg <= 0.0:
		return 0.0
	
	# Calculate mutual Hill radius
	var combined_mass: float = inner_planet_mass_kg + outer_planet_mass_kg
	var hill_radius: float = inner_orbit_m * pow(combined_mass / (3.0 * star_mass_kg), 1.0 / 3.0)
	
	# Return ~10 Hill radii as minimum spacing
	return hill_radius * 10.0


# =============================================================================
# PERTURBATION ANALYSIS
# =============================================================================


## Checks if an orbit is stable given perturbations from companion stars.
## Simplified check: orbit is stable if it's within the stability zone of its host
## and sufficiently far from perturbing companions.
## @param orbit_distance_m: Distance of orbit from its host.
## @param host_mass_kg: Mass of the orbit host.
## @param host_position_m: Position of host (for distance calculation, can be 0 for simplified check).
## @param companion_masses_kg: Array of companion star masses.
## @param companion_positions_m: Array of companion star positions (distances from system barycenter).
## @return: True if orbit appears stable.
static func is_orbit_stable(
	orbit_distance_m: float,
	host_mass_kg: float,
	host_position_m: float,
	companion_masses_kg: Array[float],
	companion_positions_m: Array[float]
) -> bool:
	if orbit_distance_m <= 0.0 or host_mass_kg <= 0.0:
		return false
	
	# If no companions, orbit is stable (assuming within host's Hill sphere if applicable)
	if companion_masses_kg.is_empty():
		return true
	
	# Check each companion
	for i in range(mini(companion_masses_kg.size(), companion_positions_m.size())):
		var comp_mass: float = companion_masses_kg[i]
		var comp_position: float = companion_positions_m[i]
		
		# Distance between host and companion
		var separation: float = absf(comp_position - host_position_m)
		if separation <= 0.0:
			continue
		
		# Approximate: if planet orbit > 50% of separation, likely unstable
		if orbit_distance_m > separation * 0.5:
			return false
		
		# Hill sphere check: planet should be well within host's Hill sphere
		# when considering companion perturbation
		var hill_radius: float = calculate_hill_sphere(host_mass_kg, comp_mass, separation)
		if orbit_distance_m > hill_radius * 0.5:
			return false
	
	return true


## Calculates the perturbation strength from a companion.
## Higher values indicate stronger perturbation.
## @param orbit_distance_m: Distance of the orbit from its host.
## @param companion_distance_m: Distance from host to companion.
## @param companion_mass_kg: Mass of the companion.
## @param host_mass_kg: Mass of the orbit host.
## @return: Perturbation parameter (0 = none, >1 = strong).
static func calculate_perturbation_strength(
	orbit_distance_m: float,
	companion_distance_m: float,
	companion_mass_kg: float,
	host_mass_kg: float
) -> float:
	if companion_distance_m <= 0.0 or host_mass_kg <= 0.0:
		return 0.0
	
	# Tisserand-like parameter
	# Perturbation scales as (m_comp/m_host) * (a_orbit/a_comp)Â³ for a < a_comp
	# and (m_comp/m_host) * (a_comp/a_orbit)Â² for a > a_comp
	
	var mass_ratio: float = companion_mass_kg / host_mass_kg
	
	if orbit_distance_m < companion_distance_m:
		var dist_ratio: float = orbit_distance_m / companion_distance_m
		return mass_ratio * pow(dist_ratio, 3.0)
	else:
		var dist_ratio: float = companion_distance_m / orbit_distance_m
		return mass_ratio * pow(dist_ratio, 2.0)


# =============================================================================
# ORBIT OVERLAP AND INTERACTIONS
# =============================================================================


## Checks if two orbits overlap (periapsis to apoapsis ranges).
## @param a1_m: Semi-major axis of orbit 1.
## @param e1: Eccentricity of orbit 1.
## @param a2_m: Semi-major axis of orbit 2.
## @param e2: Eccentricity of orbit 2.
## @return: True if orbits overlap.
static func do_orbits_overlap(
	a1_m: float,
	e1: float,
	a2_m: float,
	e2: float
) -> bool:
	var peri1: float = a1_m * (1.0 - e1)
	var apo1: float = a1_m * (1.0 + e1)
	var peri2: float = a2_m * (1.0 - e2)
	var apo2: float = a2_m * (1.0 + e2)
	
	# Check if ranges overlap
	return not (apo1 < peri2 or apo2 < peri1)


## Calculates synodic period between two orbiting bodies.
## T_syn = |T1 * T2 / (T1 - T2)|
## @param period1_s: Orbital period of body 1.
## @param period2_s: Orbital period of body 2.
## @return: Synodic period in seconds.
static func calculate_synodic_period(
	period1_s: float,
	period2_s: float
) -> float:
	if period1_s <= 0.0 or period2_s <= 0.0:
		return 0.0
	
	var diff: float = absf(period1_s - period2_s)
	var max_period: float = maxf(period1_s, period2_s)
	
	# Use relative threshold: if periods differ by less than 1 part in 10^10,
	# treat as effectively identical (return very large synodic period)
	if diff < max_period * 1.0e-10:
		return 1.0e20 # Effectively infinite
	
	return absf(period1_s * period2_s / diff)
