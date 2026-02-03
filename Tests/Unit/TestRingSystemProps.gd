## Tests for RingBand and RingSystemProps components.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


## Tests RingBand creation with default values.
func test_ring_band_default_values() -> void:
	var band: RingBand = RingBand.new()
	assert_equal(band.inner_radius_m, 0.0)
	assert_equal(band.outer_radius_m, 0.0)
	assert_equal(band.optical_depth, 0.0)
	assert_equal(band.particle_size_m, 1.0)
	assert_equal(band.name, "")


## Tests RingBand width calculation.
func test_ring_band_width() -> void:
	var band: RingBand = RingBand.new(1.0e8, 2.5e8)
	assert_float_equal(band.get_width_m(), 1.5e8)


## Tests RingBand dominant material detection.
func test_ring_band_dominant_material() -> void:
	var comp: Dictionary = {"water_ice": 0.7, "silicates": 0.2, "organics": 0.1}
	var band: RingBand = RingBand.new(1.0e8, 2.0e8, 0.5, comp)
	assert_equal(band.get_dominant_material(), "water_ice")


## Tests RingSystemProps creation with empty bands.
func test_ring_system_empty() -> void:
	var system: RingSystemProps = RingSystemProps.new()
	assert_equal(system.get_band_count(), 0)
	assert_equal(system.get_inner_radius_m(), 0.0)
	assert_equal(system.get_outer_radius_m(), 0.0)


## Tests RingSystemProps with multiple bands.
func test_ring_system_multiple_bands() -> void:
	var band_a: RingBand = RingBand.new(1.0e8, 1.5e8, 0.5, {}, 1.0, "A Ring")
	var band_b: RingBand = RingBand.new(2.0e8, 3.0e8, 1.0, {}, 1.0, "B Ring")
	var bands: Array[RingBand] = [band_a, band_b]
	
	var system: RingSystemProps = RingSystemProps.new(bands, 1.5e19)
	
	assert_equal(system.get_band_count(), 2)
	assert_float_equal(system.get_inner_radius_m(), 1.0e8)
	assert_float_equal(system.get_outer_radius_m(), 3.0e8)
	assert_float_equal(system.get_total_width_m(), 2.0e8)


## Tests adding bands to ring system.
func test_ring_system_add_band() -> void:
	var system: RingSystemProps = RingSystemProps.new()
	assert_equal(system.get_band_count(), 0)
	
	var band: RingBand = RingBand.new(1.0e8, 2.0e8)
	system.add_band(band)
	
	assert_equal(system.get_band_count(), 1)


## Tests getting band by index.
func test_ring_system_get_band() -> void:
	var band: RingBand = RingBand.new(1.0e8, 2.0e8, 0.5, {}, 1.0, "Test")
	var bands: Array[RingBand] = [band]
	var system: RingSystemProps = RingSystemProps.new(bands)
	
	var retrieved: RingBand = system.get_band(0)
	assert_not_null(retrieved)
	assert_equal(retrieved.name, "Test")
	
	var invalid: RingBand = system.get_band(5)
	assert_null(invalid)


## Tests RingBand round-trip serialization.
func test_ring_band_round_trip() -> void:
	var comp: Dictionary = {"water_ice": 0.8, "rock": 0.2}
	var original: RingBand = RingBand.new(1.0e8, 2.0e8, 0.7, comp, 0.5, "Main")
	var data: Dictionary = original.to_dict()
	var restored: RingBand = RingBand.from_dict(data)
	
	assert_float_equal(restored.inner_radius_m, original.inner_radius_m)
	assert_float_equal(restored.outer_radius_m, original.outer_radius_m)
	assert_float_equal(restored.optical_depth, original.optical_depth)
	assert_float_equal(restored.particle_size_m, original.particle_size_m)
	assert_equal(restored.name, original.name)
	assert_float_equal(restored.composition["water_ice"], 0.8)


## Tests RingSystemProps round-trip serialization.
func test_ring_system_round_trip() -> void:
	var band_a: RingBand = RingBand.new(1.0e8, 1.5e8, 0.3, {"ice": 1.0}, 1.0, "A")
	var band_b: RingBand = RingBand.new(2.0e8, 3.0e8, 0.8, {"rock": 1.0}, 2.0, "B")
	var bands: Array[RingBand] = [band_a, band_b]
	var original: RingSystemProps = RingSystemProps.new(bands, 1.0e19, 0.5)
	
	var data: Dictionary = original.to_dict()
	var restored: RingSystemProps = RingSystemProps.from_dict(data)
	
	assert_equal(restored.get_band_count(), 2)
	assert_float_equal(restored.total_mass_kg, original.total_mass_kg)
	assert_float_equal(restored.inclination_deg, original.inclination_deg)
	assert_equal(restored.get_band(0).name, "A")
	assert_equal(restored.get_band(1).name, "B")
