## Tests for unit conversion functions.
extends TestCase


## Tests solar mass conversion round-trip.
func test_solar_mass_round_trip() -> void:
	var original: float = 1.5
	var kg: float = Units.solar_masses_to_kg(original)
	var back: float = Units.kg_to_solar_masses(kg)
	assert_float_equal(back, original)


## Tests Earth mass conversion round-trip.
func test_earth_mass_round_trip() -> void:
	var original: float = 317.8
	var kg: float = Units.earth_masses_to_kg(original)
	var back: float = Units.kg_to_earth_masses(kg)
	assert_float_equal(back, original)


## Tests Jupiter mass conversion round-trip.
func test_jupiter_mass_round_trip() -> void:
	var original: float = 2.5
	var kg: float = Units.jupiter_masses_to_kg(original)
	var back: float = Units.kg_to_jupiter_masses(kg)
	assert_float_equal(back, original)


## Tests AU conversion round-trip.
func test_au_round_trip() -> void:
	var original: float = 1.0
	var meters: float = Units.au_to_meters(original)
	var back: float = Units.meters_to_au(meters)
	assert_float_equal(back, original)


## Tests light year conversion round-trip.
func test_light_year_round_trip() -> void:
	var original: float = 4.24
	var meters: float = Units.light_years_to_meters(original)
	var back: float = Units.meters_to_light_years(meters)
	assert_float_equal(back, original, 0.0001)


## Tests parsec conversion round-trip.
func test_parsec_round_trip() -> void:
	var original: float = 1.3
	var meters: float = Units.parsecs_to_meters(original)
	var back: float = Units.meters_to_parsecs(meters)
	assert_float_equal(back, original, 0.0001)


## Tests solar radius conversion round-trip.
func test_solar_radius_round_trip() -> void:
	var original: float = 0.1
	var meters: float = Units.solar_radii_to_meters(original)
	var back: float = Units.meters_to_solar_radii(meters)
	assert_float_equal(back, original)


## Tests Earth radius conversion round-trip.
func test_earth_radius_round_trip() -> void:
	var original: float = 11.2
	var meters: float = Units.earth_radii_to_meters(original)
	var back: float = Units.meters_to_earth_radii(meters)
	assert_float_equal(back, original)


## Tests Celsius to Kelvin conversion.
func test_celsius_to_kelvin() -> void:
	assert_float_equal(Units.celsius_to_kelvin(0.0), 273.15)
	assert_float_equal(Units.celsius_to_kelvin(100.0), 373.15)
	assert_float_equal(Units.celsius_to_kelvin(-273.15), 0.0)


## Tests Kelvin to Celsius conversion.
func test_kelvin_to_celsius() -> void:
	assert_float_equal(Units.kelvin_to_celsius(273.15), 0.0)
	assert_float_equal(Units.kelvin_to_celsius(373.15), 100.0)
	assert_float_equal(Units.kelvin_to_celsius(0.0), -273.15)


## Tests known physical relationship: 1 Jupiter mass ≈ 317.8 Earth masses.
func test_jupiter_earth_mass_relationship() -> void:
	var jupiter_kg: float = Units.jupiter_masses_to_kg(1.0)
	var earth_masses: float = Units.kg_to_earth_masses(jupiter_kg)
	# Jupiter is approximately 317.8 Earth masses
	assert_in_range(earth_masses, 317.0, 319.0)
