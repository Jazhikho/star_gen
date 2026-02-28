## Read-only ensemble-sampling helpers for generator layers.
## Used by distribution tests and by any script that exports binned stats (e.g. CSV/JSON).
## Deterministic: same (seed_base, count) yields same histograms.
class_name GenerationStatsHarness
extends RefCounted


## Samples N random stars and returns counts per spectral letter (O,B,A,F,G,K,M).
## @param seed_base: Base seed; each star uses seed_base + index.
## @param count: Number of stars to generate.
## @return: Dictionary with keys "O","B","A","F","G","K","M" and int values; plus "total".
static func sample_star_spectral_histogram(seed_base: int, count: int) -> Dictionary:
	var result: Dictionary = {
		"O": 0,
		"B": 0,
		"A": 0,
		"F": 0,
		"G": 0,
		"K": 0,
		"M": 0,
		"total": 0,
	}
	for i in range(count):
		var spec: StarSpec = StarSpec.random(seed_base + i)
		var rng: SeededRng = SeededRng.new(spec.generation_seed)
		var star: CelestialBody = StarGenerator.generate(spec, rng)
		var spectral: String = star.stellar.spectral_class
		if spectral.is_empty():
			continue
		var letter: String = spectral.substr(0, 1)
		if result.has(letter):
			result[letter] = result[letter] + 1
			result["total"] = result["total"] + 1
	return result


## Samples N sun-like systems and returns aggregate planet statistics.
## @param seed_base: Base seed; each system uses seed_base + index (and derived seeds for slots/planets).
## @param system_count: Number of systems to generate.
## @return: Dictionary with total_planets, hot_jupiters, inner_total, inner_large, outer_total, outer_large.
static func sample_system_planet_stats(seed_base: int, system_count: int) -> Dictionary:
	var total_planets: int = 0
	var hot_jupiters: int = 0
	var inner_total: int = 0
	var inner_large: int = 0
	var outer_total: int = 0
	var outer_large: int = 0

	for i in range(system_count):
		var system_seed: int = seed_base + i
		var spec: SolarSystemSpec = SolarSystemSpec.sun_like(system_seed)
		var rng_stars: SeededRng = SeededRng.new(system_seed)
		var system: SolarSystem = StellarConfigGenerator.generate(spec, rng_stars)
		if system == null:
			continue
		var stars: Array[CelestialBody] = system.get_stars()
		var rng_slots: SeededRng = SeededRng.new(system_seed + 1)
		var all_slots_map: Dictionary = OrbitSlotGenerator.generate_all_slots(
			system.orbit_hosts,
			stars,
			system.hierarchy,
			rng_slots
		)
		var slots: Array[OrbitSlot] = []
		for host_id in all_slots_map.keys():
			var host_slots: Array[OrbitSlot] = all_slots_map[host_id]
			slots.append_array(host_slots)
		if slots.is_empty():
			continue
		var rng_planets: SeededRng = SeededRng.new(system_seed + 2)
		var planet_result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
			slots,
			system.orbit_hosts,
			stars,
			rng_planets,
			false
		)
		for planet in planet_result.planets:
			if not planet.has_orbital():
				continue
			var distance_au: float = planet.orbital.semi_major_axis_m / Units.AU_METERS
			var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
			var is_large: bool = mass_earth >= 10.0
			total_planets += 1
			if distance_au < 0.1 and mass_earth >= 50.0:
				hot_jupiters += 1
			if distance_au < 1.0:
				inner_total += 1
				if is_large:
					inner_large += 1
			elif distance_au > 5.0:
				outer_total += 1
				if is_large:
					outer_large += 1

	return {
		"total_planets": total_planets,
		"hot_jupiters": hot_jupiters,
		"inner_total": inner_total,
		"inner_large": inner_large,
		"outer_total": outer_total,
		"outer_large": outer_large,
	}
