## Statistical tests for StarGenerator distributions against astrophysical expectations.
class_name TestStarGeneratorDistributions
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
## Preload so class_name is registered before test body references them.
const _harness: GDScript = preload("res://Tests/GenerationStatsHarness.gd")
const _benchmarks: GDScript = preload("res://Tests/ScientificBenchmarks.gd")


## Samples many random stars and checks the spectral-type mix is broadly realistic.
## Purpose: Ensure M dwarfs dominate and early-type stars remain rare, consistent with local IMF studies.
## Returns: Nothing. Asserts on aggregate fractions using ScientificBenchmarks.
func test_spectral_type_distribution_reasonable() -> void:
	var histogram: Dictionary = _harness.call("sample_star_spectral_histogram", 1000, 10000)
	var total: int = histogram["total"] as int
	assert_greater_than(total, 0, "Sampling must produce at least one classified star")

	var m_fraction: float = float(histogram["M"] as int) / float(total)
	var gk_fraction: float = float((histogram["G"] as int) + (histogram["K"] as int)) / float(total)
	var obaf_fraction: float = float(
		(histogram["O"] as int) + (histogram["B"] as int) + (histogram["A"] as int) + (histogram["F"] as int)
	) / float(total)

	var bench: Dictionary = _benchmarks.get_script_constant_map()
	var m_min: float = bench.get("M_DWARF_FRACTION_MIN", 0.0)
	var m_max: float = bench.get("M_DWARF_FRACTION_MAX", 1.0)
	var gk_min: float = bench.get("GK_FRACTION_MIN", 0.0)
	var gk_max: float = bench.get("GK_FRACTION_MAX", 1.0)
	var obaf_max: float = bench.get("OBAF_FRACTION_MAX", 1.0)
	assert_in_range(m_fraction, m_min, m_max, "M-dwarf fraction should match benchmark range")
	assert_in_range(gk_fraction, gk_min, gk_max, "G+K fraction should match benchmark range")
	assert_in_range(obaf_fraction, 0.0, obaf_max, "OBAF fraction should remain within benchmark max")
