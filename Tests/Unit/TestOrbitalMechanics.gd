## Tests for OrbitalMechanics utility class.
extends TestCase

const _orbital_mechanics: GDScript = preload("res://src/domain/system/OrbitalMechanics.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")


# =============================================================================
# BASIC ORBITAL MECHANICS TESTS
# =============================================================================


## Tests orbital period calculation for Earth.
func test_calculate_orbital_period_earth() -> void:
	var period_s: float = OrbitalMechanics.calculate_orbital_period(
		Units.AU_METERS,
		Units.SOLAR_MASS_KG
	)
	
	var period_years: float = period_s / (365.25 * 24.0 * 3600.0)
	assert_float_equal(period_years, 1.0, 0.01, "Earth orbital period should be ~1 year")


## Tests orbital period calculation for Jupiter.
func test_calculate_orbital_period_jupiter() -> void:
	var period_s: float = OrbitalMechanics.calculate_orbital_period(
		5.2 * Units.AU_METERS,
		Units.SOLAR_MASS_KG
	)
	
	var period_years: float = period_s / (365.25 * 24.0 * 3600.0)
	assert_float_equal(period_years, 11.86, 0.5, "Jupiter orbital period should be ~11.86 years")


## Tests semi-major axis calculation (inverse of period).
func test_calculate_semi_major_axis() -> void:
	var one_year_s: float = 365.25 * 24.0 * 3600.0
	var a: float = OrbitalMechanics.calculate_semi_major_axis(one_year_s, Units.SOLAR_MASS_KG)
	
	assert_float_equal(a, Units.AU_METERS, Units.AU_METERS * 0.01, "1 year period should give 1 AU")


## Tests orbital velocity at Earth's distance.
func test_calculate_orbital_velocity_earth() -> void:
	var velocity: float = OrbitalMechanics.calculate_orbital_velocity(
		Units.AU_METERS,
		Units.SOLAR_MASS_KG
	)
	
	# Earth's orbital velocity is ~29.78 km/s
	var velocity_km_s: float = velocity / 1000.0
	assert_in_range(velocity_km_s, 29.5, 30.0, "Earth orbital velocity should be ~29.78 km/s")


## Tests escape velocity from Earth.
func test_calculate_escape_velocity_earth() -> void:
	var v_esc: float = OrbitalMechanics.calculate_escape_velocity(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS
	)
	
	# Earth's escape velocity is ~11.2 km/s
	var v_esc_km_s: float = v_esc / 1000.0
	assert_in_range(v_esc_km_s, 11.0, 11.4, "Earth escape velocity should be ~11.2 km/s")


## Tests mean motion calculation.
func test_calculate_mean_motion() -> void:
	var n: float = OrbitalMechanics.calculate_mean_motion(
		Units.AU_METERS,
		Units.SOLAR_MASS_KG
	)
	
	# Earth's mean motion: 2π / (365.25 days) ≈ 1.99e-7 rad/s
	assert_float_equal(n, 1.99e-7, 1e-8, "Earth mean motion")


## Tests edge cases return zero.
func test_orbital_period_edge_cases() -> void:
	assert_equal(OrbitalMechanics.calculate_orbital_period(0.0, Units.SOLAR_MASS_KG), 0.0)
	assert_equal(OrbitalMechanics.calculate_orbital_period(Units.AU_METERS, 0.0), 0.0)
	assert_equal(OrbitalMechanics.calculate_orbital_period(-1.0 * Units.AU_METERS, Units.SOLAR_MASS_KG), 0.0)


# =============================================================================
# GRAVITATIONAL INFLUENCE TESTS
# =============================================================================


## Tests Hill sphere for Earth around Sun.
func test_calculate_hill_sphere_earth() -> void:
	var hill_radius: float = OrbitalMechanics.calculate_hill_sphere(
		Units.EARTH_MASS_KG,
		Units.SOLAR_MASS_KG,
		Units.AU_METERS
	)
	
	# Earth's Hill sphere is ~1.5 million km
	var hill_km: float = hill_radius / 1000.0
	assert_in_range(hill_km, 1.4e6, 1.6e6, "Earth Hill sphere should be ~1.5 million km")


## Tests Hill sphere for Jupiter around Sun.
func test_calculate_hill_sphere_jupiter() -> void:
	var jupiter_mass_kg: float = 1.898e27
	var jupiter_distance_m: float = 5.2 * Units.AU_METERS
	
	var hill_radius: float = OrbitalMechanics.calculate_hill_sphere(
		jupiter_mass_kg,
		Units.SOLAR_MASS_KG,
		jupiter_distance_m
	)
	
	# Jupiter's Hill sphere is ~53 million km (~0.35 AU)
	var hill_au: float = hill_radius / Units.AU_METERS
	assert_in_range(hill_au, 0.33, 0.37, "Jupiter Hill sphere should be ~0.35 AU")


## Tests Roche limit for Earth-density satellite around Earth.
func test_calculate_roche_limit_earth() -> void:
	var earth_density: float = 5515.0 # kg/m³
	var satellite_density: float = 3000.0 # Rocky moon
	
	var roche: float = OrbitalMechanics.calculate_roche_limit(
		Units.EARTH_RADIUS_METERS,
		earth_density,
		satellite_density
	)
	
	# Roche limit should be ~18,000-20,000 km for rocky satellite
	var roche_km: float = roche / 1000.0
	assert_in_range(roche_km, 17000.0, 21000.0, "Roche limit for rocky satellite ~18-20k km")


## Tests Roche limit using mass-based function.
func test_calculate_roche_limit_from_mass() -> void:
	var roche: float = OrbitalMechanics.calculate_roche_limit_from_mass(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		3000.0
	)
	
	# Should give similar result to density-based calculation
	var roche_km: float = roche / 1000.0
	assert_in_range(roche_km, 17000.0, 21000.0, "Roche limit from mass should match")


## Tests sphere of influence.
func test_calculate_sphere_of_influence() -> void:
	var soi: float = OrbitalMechanics.calculate_sphere_of_influence(
		Units.EARTH_MASS_KG,
		Units.SOLAR_MASS_KG,
		Units.AU_METERS
	)
	
	# Earth's SOI is ~925,000 km
	var soi_km: float = soi / 1000.0
	assert_in_range(soi_km, 900000.0, 1000000.0, "Earth SOI ~925,000 km")


# =============================================================================
# BINARY SYSTEM TESTS
# =============================================================================


## Tests barycenter for Earth-Moon system.
func test_calculate_barycenter_earth_moon() -> void:
	var moon_mass_kg: float = 7.342e22
	var earth_moon_distance_m: float = 3.844e8 # 384,400 km
	
	var barycenter_dist: float = OrbitalMechanics.calculate_barycenter_from_a(
		Units.EARTH_MASS_KG,
		moon_mass_kg,
		earth_moon_distance_m
	)
	
	# Earth-Moon barycenter is ~4,670 km from Earth's center
	var dist_km: float = barycenter_dist / 1000.0
	assert_in_range(dist_km, 4500.0, 4800.0, "Earth-Moon barycenter should be ~4,670 km from Earth")


## Tests barycenter for equal masses.
func test_calculate_barycenter_equal_masses() -> void:
	var mass: float = 1e30
	var separation: float = 1e11
	
	var barycenter_dist: float = OrbitalMechanics.calculate_barycenter_from_a(
		mass, mass, separation
	)
	
	# For equal masses, barycenter is at midpoint
	assert_float_equal(barycenter_dist, separation / 2.0, separation * 0.001)


## Tests barycenter for Sun-Jupiter system.
func test_calculate_barycenter_sun_jupiter() -> void:
	var jupiter_mass: float = 1.898e27
	var jupiter_distance: float = 5.2 * Units.AU_METERS
	
	var distance_from_sun: float = OrbitalMechanics.calculate_barycenter_from_a(
		Units.SOLAR_MASS_KG,
		jupiter_mass,
		jupiter_distance
	)
	
	# Sun-Jupiter barycenter is about 1.07 solar radii from Sun's center
	var distance_solar_radii: float = distance_from_sun / Units.SOLAR_RADIUS_METERS
	assert_in_range(distance_solar_radii, 1.0, 1.2, "Sun-Jupiter barycenter ~1.07 solar radii")


## Tests S-type stability limit for circular binary.
func test_calculate_stype_stability_limit_circular() -> void:
	var binary_sep: float = 10.0 * Units.AU_METERS
	var mass_ratio: float = 1.0 # Equal masses
	var ecc: float = 0.0
	
	var limit: float = OrbitalMechanics.calculate_stype_stability_limit(
		binary_sep,
		mass_ratio,
		ecc
	)
	
	# Holman & Wiegert formula gives ~27% for equal mass circular binary
	# With 90% safety margin: ~24.6% of separation
	var limit_au: float = limit / Units.AU_METERS
	assert_in_range(limit_au, 2.0, 3.0, "S-type limit should be ~24-27% of separation for equal masses")


## Tests S-type stability limit for eccentric binary.
func test_calculate_stype_stability_limit_eccentric() -> void:
	var binary_sep: float = 10.0 * Units.AU_METERS
	var mass_ratio: float = 0.5
	var ecc: float = 0.5
	
	var limit: float = OrbitalMechanics.calculate_stype_stability_limit(
		binary_sep,
		mass_ratio,
		ecc
	)
	
	# Eccentric binaries have smaller stable regions
	var limit_au: float = limit / Units.AU_METERS
	assert_less_than(limit_au, 3.0, "Eccentric binary should reduce S-type limit")


## Tests S-type stability for Alpha Centauri-like binary.
func test_calculate_stype_stability_alpha_cen() -> void:
	# Alpha Cen AB: separation ~24 AU, e ~0.52, masses ~1.1 and 0.9 solar
	var separation_m: float = 24.0 * Units.AU_METERS
	var eccentricity: float = 0.52
	var mass_ratio: float = 0.9 / 1.1 # Secondary/primary
	
	var limit: float = OrbitalMechanics.calculate_stype_stability_limit(
		separation_m,
		mass_ratio,
		eccentricity
	)
	
	# Planets around Alpha Cen A should be stable out to ~3-4 AU
	var limit_au: float = limit / Units.AU_METERS
	assert_in_range(limit_au, 2.0, 5.0, "S-type limit for Alpha Cen A should be ~3-4 AU")


## Tests P-type stability limit for circular binary.
func test_calculate_ptype_stability_limit_circular() -> void:
	var binary_sep: float = 1.0 * Units.AU_METERS
	var mass_ratio: float = 1.0
	var ecc: float = 0.0
	
	var limit: float = OrbitalMechanics.calculate_ptype_stability_limit(
		binary_sep,
		mass_ratio,
		ecc
	)
	
	# Should be ~2-3x binary separation with safety margin
	var limit_au: float = limit / Units.AU_METERS
	assert_in_range(limit_au, 2.0, 4.0, "P-type limit should be ~2-3x separation")


## Tests P-type stability limit increases with eccentricity.
func test_calculate_ptype_stability_limit_eccentric() -> void:
	var binary_sep: float = 1.0 * Units.AU_METERS
	var mass_ratio: float = 0.5
	
	var limit_circ: float = OrbitalMechanics.calculate_ptype_stability_limit(binary_sep, mass_ratio, 0.0)
	var limit_ecc: float = OrbitalMechanics.calculate_ptype_stability_limit(binary_sep, mass_ratio, 0.5)
	
	assert_greater_than(limit_ecc, limit_circ, "Eccentric binary should increase P-type limit")


## Tests P-type stability for close binary.
func test_calculate_ptype_stability_close_binary() -> void:
	# Close binary: separation ~0.2 AU, e ~0.1
	var separation_m: float = 0.2 * Units.AU_METERS
	var eccentricity: float = 0.1
	var mass_ratio: float = 0.5
	
	var limit: float = OrbitalMechanics.calculate_ptype_stability_limit(
		separation_m,
		mass_ratio,
		eccentricity
	)
	
	# Circumbinary planets should be stable beyond ~0.5-0.8 AU
	var limit_au: float = limit / Units.AU_METERS
	assert_in_range(limit_au, 0.4, 1.0, "P-type limit for close binary ~0.5-0.8 AU")


# =============================================================================
# OUTER ORBIT LIMITS (JACOBI / FORMATION)
# =============================================================================


## Jacobi radius scales as M_star^(1/3); 1 M_sun ~ 2.78e5 AU (Solar neighbourhood).
func test_calculate_jacobi_radius_scaling() -> void:
	var r_1sun_m: float = OrbitalMechanics.calculate_jacobi_radius_m(Units.SOLAR_MASS_KG)
	var r_1sun_au: float = r_1sun_m / Units.AU_METERS
	assert_in_range(r_1sun_au, 2.5e5, 3.0e5, "Jacobi at 1 M_sun ~ 2.78e5 AU")

	var m_01: float = 0.1 * Units.SOLAR_MASS_KG
	var r_01_m: float = OrbitalMechanics.calculate_jacobi_radius_m(m_01)
	var r_01_au: float = r_01_m / Units.AU_METERS
	assert_in_range(r_01_au, 1.1e5, 1.4e5, "Jacobi at 0.1 M_sun ~ 1.29e5 AU")

	# Scaling: double mass -> 2^(1/3) ~ 1.26
	var m_2: float = 2.0 * Units.SOLAR_MASS_KG
	var r_2_m: float = OrbitalMechanics.calculate_jacobi_radius_m(m_2)
	assert_greater_than(r_2_m, r_1sun_m, "Jacobi increases with stellar mass")


## Formation outer limit scales as M_star^0.6; base 100 AU at 1 M_sun.
func test_calculate_formation_outer_limit() -> void:
	var r_1sun_m: float = OrbitalMechanics.calculate_formation_outer_limit_m(Units.SOLAR_MASS_KG, 100.0)
	var r_1sun_au: float = r_1sun_m / Units.AU_METERS
	assert_float_equal(r_1sun_au, 100.0, 1.0, "Formation at 1 M_sun with base 100 AU")

	var m_01: float = 0.1 * Units.SOLAR_MASS_KG
	var r_01_m: float = OrbitalMechanics.calculate_formation_outer_limit_m(m_01, 100.0)
	var r_01_au: float = r_01_m / Units.AU_METERS
	assert_in_range(r_01_au, 18.0, 28.0, "Formation at 0.1 M_sun ~ 25 AU (100 * 0.1^0.6)")


## Combined outer limit is min(formation, Jacobi); formation dominates for normal stars.
func test_calculate_outer_stability_limit_m() -> void:
	var limit_1sun_m: float = OrbitalMechanics.calculate_outer_stability_limit_m(Units.SOLAR_MASS_KG, 100.0)
	var limit_1sun_au: float = limit_1sun_m / Units.AU_METERS
	assert_float_equal(limit_1sun_au, 100.0, 1.0, "At 1 M_sun formation is smaller than Jacobi")

	var formation_m: float = OrbitalMechanics.calculate_formation_outer_limit_m(Units.SOLAR_MASS_KG, 100.0)
	var jacobi_m: float = OrbitalMechanics.calculate_jacobi_radius_m(Units.SOLAR_MASS_KG)
	assert_less_than(limit_1sun_m, jacobi_m, "Outer limit should be formation-limited for 1 M_sun")
	assert_float_equal(limit_1sun_m, formation_m, 1.0, "Limit should equal formation when formation < Jacobi")


## Tests binary period calculation.
func test_calculate_binary_period() -> void:
	# Two solar-mass stars at 1 AU
	var period_s: float = OrbitalMechanics.calculate_binary_period(
		Units.AU_METERS,
		Units.SOLAR_MASS_KG,
		Units.SOLAR_MASS_KG
	)
	var period_years: float = period_s / (365.25 * 24.0 * 3600.0)
	
	# Period should be ~0.707 years (1/sqrt(2) due to doubled mass)
	assert_in_range(period_years, 0.68, 0.73, "Binary period for 2 solar masses at 1 AU")


# =============================================================================
# ORBITAL ZONE TESTS
# =============================================================================


## Tests habitable zone for Sun-like star.
func test_calculate_habitable_zone_solar() -> void:
	var hz_inner: float = OrbitalMechanics.calculate_habitable_zone_inner(
		StellarProps.SOLAR_LUMINOSITY_WATTS
	)
	var hz_outer: float = OrbitalMechanics.calculate_habitable_zone_outer(
		StellarProps.SOLAR_LUMINOSITY_WATTS
	)
	
	var hz_inner_au: float = hz_inner / Units.AU_METERS
	var hz_outer_au: float = hz_outer / Units.AU_METERS
	
	# HZ for Sun should be ~0.95-1.37 AU
	assert_in_range(hz_inner_au, 0.9, 1.0, "Solar HZ inner edge ~0.95 AU")
	assert_in_range(hz_outer_au, 1.3, 1.5, "Solar HZ outer edge ~1.37 AU")


## Tests habitable zone scales with luminosity.
func test_calculate_habitable_zone_scaling() -> void:
	# 4x solar luminosity
	var luminosity: float = 4.0 * StellarProps.SOLAR_LUMINOSITY_WATTS
	
	var hz_inner: float = OrbitalMechanics.calculate_habitable_zone_inner(luminosity)
	var hz_inner_au: float = hz_inner / Units.AU_METERS
	
	# HZ should be 2x farther (sqrt(4) = 2)
	assert_in_range(hz_inner_au, 1.8, 2.0, "HZ inner for 4x solar should be ~1.9 AU")


## Tests frost line for Sun.
func test_calculate_frost_line_solar() -> void:
	var frost: float = OrbitalMechanics.calculate_frost_line(StellarProps.SOLAR_LUMINOSITY_WATTS)
	var frost_au: float = frost / Units.AU_METERS
	
	# Frost line for Sun is ~2.7 AU
	assert_in_range(frost_au, 2.5, 3.0, "Solar frost line ~2.7 AU")


## Tests orbital zone classification.
func test_get_orbital_zone() -> void:
	var lum: float = StellarProps.SOLAR_LUMINOSITY_WATTS
	
	# Inside HZ -> HOT
	var zone_hot: OrbitZone.Zone = OrbitalMechanics.get_orbital_zone(
		0.5 * Units.AU_METERS, lum
	)
	assert_equal(zone_hot, OrbitZone.Zone.HOT, "0.5 AU should be HOT")
	
	# In HZ -> TEMPERATE
	var zone_temp: OrbitZone.Zone = OrbitalMechanics.get_orbital_zone(
		1.0 * Units.AU_METERS, lum
	)
	assert_equal(zone_temp, OrbitZone.Zone.TEMPERATE, "1.0 AU should be TEMPERATE")
	
	# Beyond frost line -> COLD
	var zone_cold: OrbitZone.Zone = OrbitalMechanics.get_orbital_zone(
		5.0 * Units.AU_METERS, lum
	)
	assert_equal(zone_cold, OrbitZone.Zone.COLD, "5.0 AU should be COLD")


# =============================================================================
# RESONANCE TESTS
# =============================================================================


## Tests resonance spacing 2:1 ratio.
func test_calculate_resonance_spacing_2_1() -> void:
	var inner_orbit: float = 1.0 * Units.AU_METERS
	var rng: SeededRng = SeededRng.new(12345)
	
	var outer_orbit: float = OrbitalMechanics.calculate_resonance_spacing(
		inner_orbit,
		2.0,
		0.0, # No variation
		rng
	)
	
	# 2:1 resonance: outer orbit = inner * 2^(2/3) ≈ 1.587
	var outer_au: float = outer_orbit / Units.AU_METERS
	assert_float_equal(outer_au, 1.587, 0.01, "2:1 resonance spacing")


## Tests resonance spacing 3:2 ratio.
func test_calculate_resonance_spacing_3_2() -> void:
	var inner_orbit: float = 1.0 * Units.AU_METERS
	var rng: SeededRng = SeededRng.new(12345)
	
	var outer_orbit: float = OrbitalMechanics.calculate_resonance_spacing(
		inner_orbit,
		1.5,
		0.0,
		rng
	)
	
	# 3:2 resonance: outer orbit = inner * 1.5^(2/3) ≈ 1.310
	var outer_au: float = outer_orbit / Units.AU_METERS
	assert_float_equal(outer_au, 1.310, 0.01, "3:2 resonance spacing")


## Tests resonance spacing with variation.
func test_calculate_resonance_spacing_with_variation() -> void:
	var inner_orbit: float = 1.0 * Units.AU_METERS
	var rng: SeededRng = SeededRng.new(12345)
	
	var outer_orbit: float = OrbitalMechanics.calculate_resonance_spacing(
		inner_orbit,
		2.0,
		0.2, # 20% variation
		rng
	)
	
	# Should be near 1.587 AU but with variation
	var outer_au: float = outer_orbit / Units.AU_METERS
	assert_in_range(outer_au, 1.27, 1.90, "2:1 resonance with 20% variation")


## Tests get_common_resonance_ratios.
func test_get_common_resonance_ratios() -> void:
	var ratios: Array[float] = OrbitalMechanics.get_common_resonance_ratios()
	
	assert_greater_than(ratios.size(), 5, "Should have multiple resonance ratios")
	assert_true(ratios.has(2.0), "Should include 2:1")
	assert_true(ratios.has(1.5), "Should include 3:2")


## Tests period to distance ratio conversion.
func test_period_ratio_to_distance_ratio() -> void:
	# 2:1 period ratio -> 2^(2/3) ≈ 1.587 distance ratio
	var dist_ratio: float = OrbitalMechanics.period_ratio_to_distance_ratio(2.0)
	assert_in_range(dist_ratio, 1.58, 1.60, "2:1 period ratio -> ~1.587 distance ratio")


## Tests distance to period ratio conversion.
func test_distance_ratio_to_period_ratio() -> void:
	# 2x distance -> 2^1.5 ≈ 2.83 period ratio
	var period_ratio: float = OrbitalMechanics.distance_ratio_to_period_ratio(2.0)
	assert_in_range(period_ratio, 2.8, 2.85, "2x distance -> ~2.83 period ratio")


## Tests minimum planet spacing.
func test_calculate_minimum_planet_spacing() -> void:
	var spacing: float = OrbitalMechanics.calculate_minimum_planet_spacing(
		Units.EARTH_MASS_KG,
		Units.EARTH_MASS_KG,
		Units.SOLAR_MASS_KG,
		1.0 * Units.AU_METERS
	)
	
	# Should be ~10 mutual Hill radii
	var spacing_au: float = spacing / Units.AU_METERS
	assert_greater_than(spacing_au, 0.05, "Minimum spacing should be significant")
	assert_less_than(spacing_au, 0.2, "Minimum spacing should be reasonable")


# =============================================================================
# PERTURBATION TESTS
# =============================================================================


## Tests is_orbit_stable with no companions.
func test_is_orbit_stable_no_companions() -> void:
	var stable: bool = OrbitalMechanics.is_orbit_stable(
		1.0 * Units.AU_METERS,
		Units.SOLAR_MASS_KG,
		0.0,
		[],
		[]
	)
	
	assert_true(stable, "Orbit with no companions should be stable")


## Tests is_orbit_stable too close to companion.
func test_is_orbit_stable_too_close_to_companion() -> void:
	var stable: bool = OrbitalMechanics.is_orbit_stable(
		8.0 * Units.AU_METERS, # Orbit at 8 AU
		Units.SOLAR_MASS_KG,
		0.0,
		[Units.SOLAR_MASS_KG], # Companion of equal mass
		[10.0 * Units.AU_METERS] # Companion at 10 AU
	)
	
	assert_false(stable, "Orbit too close to companion should be unstable")


## Tests is_orbit_stable far from companion.
func test_is_orbit_stable_far_from_companion() -> void:
	var stable: bool = OrbitalMechanics.is_orbit_stable(
		1.0 * Units.AU_METERS, # Orbit at 1 AU
		Units.SOLAR_MASS_KG,
		0.0,
		[Units.SOLAR_MASS_KG * 0.5], # Companion of half solar mass
		[50.0 * Units.AU_METERS] # Companion far away at 50 AU
	)
	
	assert_true(stable, "Orbit far from companion should be stable")


## Tests perturbation strength calculation.
func test_calculate_perturbation_strength() -> void:
	var jupiter_mass_kg: float = 1.898e27
	var jupiter_distance_m: float = 5.2 * Units.AU_METERS
	
	# Mars orbit (1.5 AU) - should have low perturbation
	var mars_orbit: float = 1.5 * Units.AU_METERS
	var strength_mars: float = OrbitalMechanics.calculate_perturbation_strength(
		mars_orbit,
		jupiter_distance_m,
		jupiter_mass_kg,
		Units.SOLAR_MASS_KG
	)
	
	# Asteroid belt (3 AU) - should have moderate perturbation
	var belt_orbit: float = 3.0 * Units.AU_METERS
	var strength_belt: float = OrbitalMechanics.calculate_perturbation_strength(
		belt_orbit,
		jupiter_distance_m,
		jupiter_mass_kg,
		Units.SOLAR_MASS_KG
	)
	
	assert_less_than(strength_mars, strength_belt, "Mars perturbation < asteroid belt perturbation")
	
	# Close orbit should have higher perturbation
	var close_orbit: float = 4.0 * Units.AU_METERS
	var strength_close: float = OrbitalMechanics.calculate_perturbation_strength(
		close_orbit,
		jupiter_distance_m,
		jupiter_mass_kg,
		Units.SOLAR_MASS_KG
	)
	assert_greater_than(strength_close, strength_mars, "Closer orbit should have higher perturbation")


## Tests perturbation strength for orbit inside companion.
func test_calculate_perturbation_strength_inside() -> void:
	# Orbit at 1 AU, companion at 5 AU
	var strength: float = OrbitalMechanics.calculate_perturbation_strength(
		1.0 * Units.AU_METERS,
		5.0 * Units.AU_METERS,
		Units.SOLAR_MASS_KG,
		Units.SOLAR_MASS_KG
	)
	
	# Should use (a_orbit/a_comp)^3 scaling
	assert_greater_than(strength, 0.0, "Perturbation strength should be positive")
	assert_less_than(strength, 0.1, "Perturbation for distant companion should be small")


# =============================================================================
# ORBIT OVERLAP AND INTERACTIONS TESTS
# =============================================================================


## Tests do_orbits_overlap for non-overlapping circular orbits.
func test_do_orbits_overlap_no_overlap() -> void:
	var overlap: bool = OrbitalMechanics.do_orbits_overlap(
		1.0 * Units.AU_METERS,
		0.0,
		2.0 * Units.AU_METERS,
		0.0
	)
	
	assert_false(overlap, "Non-overlapping circular orbits")


## Tests do_orbits_overlap for overlapping eccentric orbits.
func test_do_orbits_overlap_eccentric() -> void:
	var overlap: bool = OrbitalMechanics.do_orbits_overlap(
		1.0 * Units.AU_METERS,
		0.5, # Periapsis 0.5 AU, Apoapsis 1.5 AU
		1.2 * Units.AU_METERS,
		0.3 # Periapsis 0.84 AU, Apoapsis 1.56 AU
	)
	
	assert_true(overlap, "Eccentric orbits with overlapping ranges")


## Tests do_orbits_overlap for just-touching orbits.
func test_do_orbits_overlap_touching() -> void:
	# Orbit 1: 1 AU, e=0.5 -> 0.5 to 1.5 AU
	# Orbit 2: 1.5 AU, e=0.0 -> 1.5 to 1.5 AU (circular)
	# They touch at 1.5 AU, so should overlap
	var overlap: bool = OrbitalMechanics.do_orbits_overlap(
		1.0 * Units.AU_METERS,
		0.5,
		1.5 * Units.AU_METERS,
		0.0
	)
	
	assert_true(overlap, "Orbits that touch should overlap")


## Tests synodic period calculation.
func test_calculate_synodic_period() -> void:
	var earth_period: float = 365.25 * 24.0 * 3600.0
	var mars_period: float = 687.0 * 24.0 * 3600.0
	
	var synodic: float = OrbitalMechanics.calculate_synodic_period(earth_period, mars_period)
	var synodic_days: float = synodic / (24.0 * 3600.0)
	
	# Earth-Mars synodic period ~780 days
	assert_float_equal(synodic_days, 780.0, 10.0, "Earth-Mars synodic period ~780 days")


## Tests synodic period for nearly identical periods.
func test_calculate_synodic_period_near_identical() -> void:
	var period1: float = 365.25 * 24.0 * 3600.0 # 1 year
	var period2: float = period1 * 1.0000000001 # Differ by 1 part in 10 billion
	
	var synodic: float = OrbitalMechanics.calculate_synodic_period(period1, period2)
	
	# Should return very large value (effectively infinite)
	assert_greater_than(synodic, 1.0e15, "Near-identical periods should give very large synodic period")


# =============================================================================
# EDGE CASE TESTS
# =============================================================================


## Tests edge case: zero inputs.
func test_edge_case_zero_inputs() -> void:
	assert_equal(OrbitalMechanics.calculate_orbital_period(0.0, Units.SOLAR_MASS_KG), 0.0)
	assert_equal(OrbitalMechanics.calculate_orbital_period(Units.AU_METERS, 0.0), 0.0)
	assert_equal(OrbitalMechanics.calculate_hill_sphere(0.0, Units.SOLAR_MASS_KG, Units.AU_METERS), 0.0)
	assert_equal(OrbitalMechanics.calculate_barycenter_from_a(0.0, Units.SOLAR_MASS_KG, 1e11), 0.0)
	assert_equal(OrbitalMechanics.calculate_roche_limit(0.0, 5000.0, 3000.0), 0.0)
	assert_equal(OrbitalMechanics.calculate_habitable_zone_inner(0.0), 0.0)
	assert_equal(OrbitalMechanics.calculate_frost_line(0.0), 0.0)


## Tests edge case: negative inputs.
func test_edge_case_negative_inputs() -> void:
	# Negative inputs should be handled gracefully (return 0 or clamp)
	assert_equal(OrbitalMechanics.calculate_orbital_period(-1.0 * Units.AU_METERS, Units.SOLAR_MASS_KG), 0.0)
	assert_equal(OrbitalMechanics.calculate_hill_sphere(-1.0, Units.SOLAR_MASS_KG, Units.AU_METERS), 0.0)


## Tests edge case: invalid resonance spacing.
func test_resonance_spacing_edge_cases() -> void:
	var rng: SeededRng = SeededRng.new(12345)
	
	# Invalid ratio (<= 1.0) should return inner orbit
	var result: float = OrbitalMechanics.calculate_resonance_spacing(
		Units.AU_METERS,
		0.5, # Invalid ratio
		0.0,
		rng
	)
	assert_equal(result, Units.AU_METERS, "Invalid ratio should return inner orbit")
	
	# Zero inner orbit should return zero
	var result2: float = OrbitalMechanics.calculate_resonance_spacing(
		0.0,
		2.0,
		0.0,
		rng
	)
	assert_equal(result2, 0.0, "Zero inner orbit should return zero")
