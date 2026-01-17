## Tests for StellarProps component.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests creation with default values.
func test_default_values() -> void:
	var props: StellarProps = StellarProps.new()
	assert_equal(props.luminosity_watts, 0.0)
	assert_equal(props.effective_temperature_k, 0.0)
	assert_equal(props.spectral_class, "")
	assert_equal(props.stellar_type, "main_sequence")
	assert_equal(props.metallicity, 1.0)
	assert_equal(props.age_years, 0.0)


## Tests creation with Sun-like values.
func test_sun_like_initialization() -> void:
	var props: StellarProps = StellarProps.new(
		3.828e26,  # luminosity_watts
		5778.0,    # effective_temperature_k
		"G2V",     # spectral_class
		"main_sequence",
		1.0,       # metallicity
		4.6e9      # age_years
	)
	assert_equal(props.luminosity_watts, 3.828e26)
	assert_equal(props.effective_temperature_k, 5778.0)
	assert_equal(props.spectral_class, "G2V")


## Tests luminosity solar conversion.
func test_luminosity_solar() -> void:
	var props: StellarProps = StellarProps.new(StellarProps.SOLAR_LUMINOSITY_WATTS)
	assert_float_equal(props.get_luminosity_solar(), 1.0)
	
	props.luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS * 2.0
	assert_float_equal(props.get_luminosity_solar(), 2.0)


## Tests habitable zone calculation for Sun-like star.
func test_habitable_zone_sun_like() -> void:
	var props: StellarProps = StellarProps.new(StellarProps.SOLAR_LUMINOSITY_WATTS)
	
	var inner: float = props.get_habitable_zone_inner_m()
	var outer: float = props.get_habitable_zone_outer_m()
	
	# Inner should be ~0.95 AU
	assert_in_range(inner, 0.9 * Units.AU_METERS, 1.0 * Units.AU_METERS)
	# Outer should be ~1.37 AU
	assert_in_range(outer, 1.3 * Units.AU_METERS, 1.45 * Units.AU_METERS)


## Tests habitable zone scales with luminosity.
func test_habitable_zone_brighter_star() -> void:
	var sun_like: StellarProps = StellarProps.new(StellarProps.SOLAR_LUMINOSITY_WATTS)
	var brighter: StellarProps = StellarProps.new(StellarProps.SOLAR_LUMINOSITY_WATTS * 4.0)
	
	# 4x luminosity -> 2x distance (sqrt relationship)
	var sun_inner: float = sun_like.get_habitable_zone_inner_m()
	var bright_inner: float = brighter.get_habitable_zone_inner_m()
	
	assert_float_equal(bright_inner / sun_inner, 2.0, 0.01)


## Tests frost line calculation.
func test_frost_line() -> void:
	var props: StellarProps = StellarProps.new(StellarProps.SOLAR_LUMINOSITY_WATTS)
	var frost_line: float = props.get_frost_line_m()
	
	# Should be ~2.7 AU for Sun-like star
	assert_in_range(frost_line, 2.5 * Units.AU_METERS, 2.9 * Units.AU_METERS)


## Tests spectral letter extraction.
func test_spectral_letter() -> void:
	var props: StellarProps = StellarProps.new()
	
	props.spectral_class = "G2V"
	assert_equal(props.get_spectral_letter(), "G")
	
	props.spectral_class = "M5V"
	assert_equal(props.get_spectral_letter(), "M")
	
	props.spectral_class = "K0III"
	assert_equal(props.get_spectral_letter(), "K")
	
	props.spectral_class = ""
	assert_equal(props.get_spectral_letter(), "")


## Tests round-trip serialization.
func test_round_trip() -> void:
	var original: StellarProps = StellarProps.new(
		3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9
	)
	var data: Dictionary = original.to_dict()
	var restored: StellarProps = StellarProps.from_dict(data)
	
	assert_float_equal(restored.luminosity_watts, original.luminosity_watts)
	assert_float_equal(restored.effective_temperature_k, original.effective_temperature_k)
	assert_equal(restored.spectral_class, original.spectral_class)
	assert_equal(restored.stellar_type, original.stellar_type)
	assert_float_equal(restored.metallicity, original.metallicity)
	assert_float_equal(restored.age_years, original.age_years)
