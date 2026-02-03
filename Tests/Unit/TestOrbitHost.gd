## Tests for OrbitHost.
extends TestCase

const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Tests basic construction.
func test_construction() -> void:
	var host: OrbitHost = OrbitHost.new("node_1", OrbitHost.HostType.S_TYPE)
	
	assert_equal(host.node_id, "node_1")
	assert_equal(host.host_type, OrbitHost.HostType.S_TYPE)
	assert_equal(host.combined_mass_kg, 0.0)


## Tests has_valid_zone.
func test_has_valid_zone() -> void:
	var host: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	
	# No zone set
	assert_false(host.has_valid_zone())
	
	# Set valid zone
	host.inner_stability_m = 1.0e10
	host.outer_stability_m = 1.0e12
	assert_true(host.has_valid_zone())
	
	# Invalid zone (inner >= outer)
	host.inner_stability_m = 1.0e12
	host.outer_stability_m = 1.0e10
	assert_false(host.has_valid_zone())


## Tests zone width calculation.
func test_get_zone_width() -> void:
	var host: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	host.inner_stability_m = 1.0e11  # ~0.67 AU
	host.outer_stability_m = 5.0e12  # ~33 AU
	
	var width: float = host.get_zone_width_m()
	assert_float_equal(width, 4.9e12, 1e10)


## Tests is_distance_stable.
func test_is_distance_stable() -> void:
	var host: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	host.inner_stability_m = 1.0e11
	host.outer_stability_m = 5.0e12
	
	assert_false(host.is_distance_stable(0.5e11))  # Too close
	assert_true(host.is_distance_stable(1.0e11))   # At inner edge
	assert_true(host.is_distance_stable(1.0e12))   # Middle
	assert_true(host.is_distance_stable(5.0e12))   # At outer edge
	assert_false(host.is_distance_stable(6.0e12))  # Too far


## Tests calculate_zones for Sun-like star.
func test_calculate_zones_sun_like() -> void:
	var host: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS
	host.calculate_zones()
	
	# HZ should be roughly 0.95-1.37 AU
	var hz_inner_au: float = host.habitable_zone_inner_m / Units.AU_METERS
	var hz_outer_au: float = host.habitable_zone_outer_m / Units.AU_METERS
	var frost_au: float = host.frost_line_m / Units.AU_METERS
	
	assert_in_range(hz_inner_au, 0.9, 1.0)
	assert_in_range(hz_outer_au, 1.3, 1.5)
	assert_in_range(frost_au, 2.5, 3.0)


## Tests calculate_zones for brighter star.
func test_calculate_zones_bright_star() -> void:
	var host: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS * 4.0  # 4x solar
	host.calculate_zones()
	
	# HZ should be roughly 2x farther (sqrt(4) = 2)
	var hz_inner_au: float = host.habitable_zone_inner_m / Units.AU_METERS
	
	assert_in_range(hz_inner_au, 1.8, 2.1)


## Tests is_distance_habitable.
func test_is_distance_habitable() -> void:
	var host: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS
	host.calculate_zones()
	
	assert_false(host.is_distance_habitable(0.5 * Units.AU_METERS))  # Too hot
	assert_true(host.is_distance_habitable(1.0 * Units.AU_METERS))   # Earth-like
	assert_false(host.is_distance_habitable(5.0 * Units.AU_METERS))  # Too cold


## Tests is_beyond_frost_line.
func test_is_beyond_frost_line() -> void:
	var host: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS
	host.calculate_zones()
	
	assert_false(host.is_beyond_frost_line(1.0 * Units.AU_METERS))
	assert_true(host.is_beyond_frost_line(5.0 * Units.AU_METERS))


## Tests get_type_string.
func test_get_type_string() -> void:
	var s_type: OrbitHost = OrbitHost.new("n1", OrbitHost.HostType.S_TYPE)
	var p_type: OrbitHost = OrbitHost.new("n2", OrbitHost.HostType.P_TYPE)
	
	assert_equal(s_type.get_type_string(), "S-type")
	assert_equal(p_type.get_type_string(), "P-type")


## Tests serialization round-trip.
func test_round_trip() -> void:
	var original: OrbitHost = OrbitHost.new("node_42", OrbitHost.HostType.P_TYPE)
	original.combined_mass_kg = 2.0e30
	original.combined_luminosity_watts = 5.0e26
	original.effective_temperature_k = 5500.0
	original.inner_stability_m = 1.0e11
	original.outer_stability_m = 5.0e12
	original.calculate_zones()
	
	var data: Dictionary = original.to_dict()
	var restored: OrbitHost = OrbitHost.from_dict(data)
	
	assert_equal(restored.node_id, original.node_id)
	assert_equal(restored.host_type, original.host_type)
	assert_float_equal(restored.combined_mass_kg, original.combined_mass_kg)
	assert_float_equal(restored.combined_luminosity_watts, original.combined_luminosity_watts)
	assert_float_equal(restored.inner_stability_m, original.inner_stability_m)
	assert_float_equal(restored.outer_stability_m, original.outer_stability_m)
	assert_float_equal(restored.habitable_zone_inner_m, original.habitable_zone_inner_m)
