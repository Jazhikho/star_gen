## Statistical tests for system-scale planet distributions against astrophysical expectations.
class_name TestSystemPlanetDistributions
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _harness: GDScript = preload("res://Tests/GenerationStatsHarness.gd")
const _benchmarks: GDScript = preload("res://Tests/ScientificBenchmarks.gd")


## Samples many sun-like systems and measures the frequency of hot Jupiters.
## Purpose: Ensure close-in gas giants remain rare, broadly matching exoplanet demographics.
## Returns: Nothing. Asserts that the hot-Jupiter fraction is below ScientificBenchmarks.HOT_JUPITER_FRACTION_MAX.
func test_hot_jupiter_fraction_rare() -> void:
	var stats: Dictionary = _harness.call("sample_system_planet_stats", 50000, 200)
	var total_planets: int = stats["total_planets"] as int
	var hot_jupiters: int = stats["hot_jupiters"] as int

	assert_greater_than(total_planets, 0, "Ensemble sampling must produce some planets")

	var fraction: float = float(hot_jupiters) / float(total_planets)
	var hj_max: float = _benchmarks.get_script_constant_map().get("HOT_JUPITER_FRACTION_MAX", 0.05)
	assert_less_than(fraction, hj_max, "Hot Jupiters should be rare (below benchmark max)")


## Compares the fraction of large planets inside and outside the snow line.
## Purpose: Validate that cold outer regions preferentially host massive planets and ice/gas giants.
## Returns: Nothing. Asserts that the outer large-planet fraction exceeds the inner fraction.
func test_cold_zone_prefers_large_planets() -> void:
	var stats: Dictionary = _harness.call("sample_system_planet_stats", 60000, 150)
	var inner_total: int = stats["inner_total"] as int
	var inner_large: int = stats["inner_large"] as int
	var outer_total: int = stats["outer_total"] as int
	var outer_large: int = stats["outer_large"] as int

	assert_greater_than(inner_total, 0, "Inner region should contain some planets")
	assert_greater_than(outer_total, 0, "Outer region should contain some planets")

	var inner_large_fraction: float = float(inner_large) / float(inner_total)
	var outer_large_fraction: float = float(outer_large) / float(outer_total)
	assert_greater_than(
		outer_large_fraction,
		inner_large_fraction,
		"Large planets should be more common in outer orbits than inner orbits"
	)
