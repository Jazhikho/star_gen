## Generates native populations for planets based on profile and parameters.
## All functions are deterministic when given a SeededRng.
class_name NativePopulationGenerator
extends RefCounted

# Preload dependencies.
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _history_generator: GDScript = preload("res://src/domain/population/HistoryGenerator.gd")
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")


## Minimum habitability score for native life to emerge.
const MIN_HABITABILITY_FOR_LIFE: int = 3

## Minimum habitability score for sentient life.
const MIN_HABITABILITY_FOR_SENTIENCE: int = 4

## Base chance for sentient life to emerge on a habitable world (0-1).
const BASE_SENTIENCE_CHANCE: float = 0.3

## Base years for civilization to emerge after sentience.
const BASE_CIVILIZATION_YEARS: int = 100000


## Specification for generating native populations.
class NativePopulationSpec:
	## Seed for deterministic generation.
	var seed_value: int = 0

	## Maximum number of native populations to generate.
	var max_populations: int = 3

	## Whether to force at least one population (if habitable).
	var force_population: bool = false

	## Target end year for history generation (0 = present).
	var current_year: int = 0

	## Minimum years of history to generate.
	var min_history_years: int = 1000

	## Maximum years of history to generate.
	var max_history_years: int = 100000

	## Chance modifier for life emergence (1.0 = default).
	var life_chance_modifier: float = 1.0


## Generates native populations for a planet.
## @param profile: The planet profile.
## @param spec: Generation specification.
## @param rng: Seeded random number generator.
## @return: Array of generated NativePopulation instances.
static func generate(
	profile: PlanetProfile,
	spec: NativePopulationSpec,
	rng: SeededRng
) -> Array[NativePopulation]:
	var populations: Array[NativePopulation] = []

	if not profile.can_support_native_life():
		return populations

	var num_populations: int = _determine_population_count(profile, spec, rng)

	if num_populations == 0:
		return populations

	for i in range(num_populations):
		var pop: NativePopulation = _generate_single_population(
			profile, spec, rng, i, num_populations
		)
		populations.append(pop)

	return populations


## Determines how many native populations should exist.
## @param profile: The planet profile.
## @param spec: Generation specification.
## @param rng: Seeded random number generator.
## @return: Number of populations to generate.
static func _determine_population_count(
	profile: PlanetProfile,
	spec: NativePopulationSpec,
	rng: SeededRng
) -> int:
	var life_chance: float = _calculate_life_chance(profile) * spec.life_chance_modifier

	# If life is impossible, force_population cannot help
	if life_chance <= 0.0:
		return 0

	# force_population guarantees at least one population (if life is possible)
	if spec.force_population:
		var force_count: int = 1

		# Still roll for additional populations
		var force_multi_chance: float = _calculate_multi_population_chance(profile)
		while force_count < spec.max_populations and rng.randf() < force_multi_chance:
			force_count += 1
			force_multi_chance *= 0.5

		return force_count

	# Normal path: roll for first population
	if rng.randf() > life_chance:
		return 0

	var count: int = 1

	# Multiple populations more likely with certain conditions
	var multi_chance: float = _calculate_multi_population_chance(profile)

	while count < spec.max_populations and rng.randf() < multi_chance:
		count += 1
		multi_chance *= 0.5

	return count


## Calculates chance for multiple populations based on profile.
## @param profile: The planet profile.
## @return: Probability for additional populations (0-1).
static func _calculate_multi_population_chance(profile: PlanetProfile) -> float:
	var multi_chance: float = 0.0

	if profile.continent_count >= 3:
		multi_chance += 0.2
	if profile.continent_count >= 5:
		multi_chance += 0.1
	if profile.habitability_score >= 7:
		multi_chance += 0.2
	if profile.biomes.size() >= 4:
		multi_chance += 0.1

	return multi_chance


## Calculates chance for life to emerge on a planet.
static func _calculate_life_chance(profile: PlanetProfile) -> float:
	if profile.habitability_score < MIN_HABITABILITY_FOR_SENTIENCE:
		return 0.0

	var chance: float = BASE_SENTIENCE_CHANCE
	chance *= (profile.habitability_score - MIN_HABITABILITY_FOR_SENTIENCE + 1) / 7.0

	if profile.has_liquid_water:
		chance *= 1.5

	if profile.biomes.size() >= 4:
		chance *= 1.2

	if profile.volcanism_level < 0.3 and profile.tectonic_activity < 0.5:
		chance *= 1.2

	return clampf(chance, 0.0, 0.95)


## Generates a single native population.
static func _generate_single_population(
	profile: PlanetProfile,
	spec: NativePopulationSpec,
	rng: SeededRng,
	index: int,
	total: int
) -> NativePopulation:
	var pop: NativePopulation = NativePopulation.new()

	pop.id = "native_%s_%d" % [profile.body_id, index]
	pop.name = _generate_population_name(rng, index)
	pop.body_id = profile.body_id

	var history_years: int = rng.randi_range(spec.min_history_years, spec.max_history_years)
	pop.origin_year = spec.current_year - history_years

	pop.tech_level = _determine_tech_level(history_years, profile, rng)
	pop.government = _generate_government(pop.tech_level, history_years, rng)
	pop.government.regime_established_year = spec.current_year - rng.randi_range(10, 500)

	pop.primary_biome = _select_primary_biome(profile, rng)

	if total == 1:
		pop.territorial_control = rng.randf_range(0.3, 0.9)
	else:
		var max_control: float = 0.8 / total
		pop.territorial_control = rng.randf_range(0.1, max_control)

	pop.population = _calculate_population(profile, pop, rng)
	pop.peak_population = roundi(pop.population * rng.randf_range(1.0, 1.5))
	pop.peak_population_year = spec.current_year - rng.randi_range(0, int(history_years / 4.0))

	pop.cultural_traits = _generate_cultural_traits(profile, pop.tech_level, rng)

	pop.history = HistoryGenerator.generate_history(
		profile,
		pop.origin_year,
		spec.current_year,
		rng,
		pop.name + " Emergence"
	)

	if rng.randf() < 0.1 and history_years > 10000:
		var extinction_year: int = spec.current_year - rng.randi_range(100, int(history_years / 2.0))
		var causes: Array[String] = ["climate change", "asteroid impact", "plague", "war", "unknown"]
		pop.record_extinction(extinction_year, causes[rng.randi_range(0, causes.size() - 1)])

	return pop


## Generates a name for a population.
static func _generate_population_name(rng: SeededRng, _index: int) -> String:
	var prefixes: Array[String] = [
		"Ak", "El", "Vor", "Zan", "Kir", "Tor", "Mar", "Sol", "Vel", "Nor",
		"Ar", "Eth", "Om", "Ur", "Ix", "Yl", "Qu", "Thal", "Krath", "Ven"
	]
	var suffixes: Array[String] = [
		"ani", "ari", "oni", "eni", "uri", "ian", "ean", "aan", "iin", "oon",
		"ax", "ex", "ix", "ox", "ux", "al", "el", "il", "ol", "ul"
	]

	var prefix: String = prefixes[rng.randi_range(0, prefixes.size() - 1)]
	var suffix: String = suffixes[rng.randi_range(0, suffixes.size() - 1)]

	return prefix + suffix


## Determines technology level based on civilization age.
static func _determine_tech_level(
	age_years: int,
	profile: PlanetProfile,
	rng: SeededRng
) -> TechnologyLevel.Level:
	var expected: TechnologyLevel.Level = TechnologyLevel.Level.STONE_AGE

	for i in range(TechnologyLevel.count()):
		var level: TechnologyLevel.Level = i as TechnologyLevel.Level
		var typical_years: int = TechnologyLevel.typical_years_to_reach(level)
		if age_years >= typical_years:
			expected = level

	var modifier: int = 0
	if profile.habitability_score < 6:
		modifier -= 1
	if profile.resources.size() >= 5:
		modifier += 1
	modifier += rng.randi_range(-1, 1)

	var final_level: int = clampi((expected as int) + modifier, 0, TechnologyLevel.count() - 1)
	return final_level as TechnologyLevel.Level


## Generates a government appropriate for the tech level and age.
static func _generate_government(
	tech_level: TechnologyLevel.Level,
	_age_years: int,
	rng: SeededRng
) -> Government:
	var gov: Government = Government.new()

	match tech_level:
		TechnologyLevel.Level.STONE_AGE:
			gov.regime = GovernmentType.Regime.TRIBAL
		TechnologyLevel.Level.BRONZE_AGE, TechnologyLevel.Level.IRON_AGE:
			var options: Array[GovernmentType.Regime] = [
				GovernmentType.Regime.CHIEFDOM,
				GovernmentType.Regime.CITY_STATE,
				GovernmentType.Regime.PATRIMONIAL_KINGDOM,
			]
			gov.regime = options[rng.randi_range(0, options.size() - 1)]
		TechnologyLevel.Level.CLASSICAL, TechnologyLevel.Level.MEDIEVAL:
			var options: Array[GovernmentType.Regime] = [
				GovernmentType.Regime.FEUDAL,
				GovernmentType.Regime.PATRIMONIAL_KINGDOM,
				GovernmentType.Regime.BUREAUCRATIC_EMPIRE,
			]
			gov.regime = options[rng.randi_range(0, options.size() - 1)]
		TechnologyLevel.Level.RENAISSANCE, TechnologyLevel.Level.INDUSTRIAL:
			var options: Array[GovernmentType.Regime] = [
				GovernmentType.Regime.ABSOLUTE_MONARCHY,
				GovernmentType.Regime.CONSTITUTIONAL,
				GovernmentType.Regime.ELITE_REPUBLIC,
			]
			gov.regime = options[rng.randi_range(0, options.size() - 1)]
		_:
			var options: Array[GovernmentType.Regime] = [
				GovernmentType.Regime.MASS_DEMOCRACY,
				GovernmentType.Regime.CONSTITUTIONAL,
				GovernmentType.Regime.ONE_PARTY_STATE,
				GovernmentType.Regime.ELITE_REPUBLIC,
			]
			gov.regime = options[rng.randi_range(0, options.size() - 1)]

	var tech_factor: float = (tech_level as int) / float(TechnologyLevel.count())
	gov.coercion_centralization = clampf(rng.randf_range(0.1, 0.4) + tech_factor * 0.5, 0.0, 1.0)
	gov.administrative_capacity = clampf(rng.randf_range(0.1, 0.3) + tech_factor * 0.6, 0.0, 1.0)

	if GovernmentType.is_participatory(gov.regime):
		gov.political_inclusiveness = rng.randf_range(0.3, 0.8)
	else:
		gov.political_inclusiveness = rng.randf_range(0.05, 0.3)

	gov.legitimacy = rng.randf_range(0.4, 0.9)

	return gov


## Selects a primary biome for the population (weighted by coverage, habitable only).
static func _select_primary_biome(profile: PlanetProfile, rng: SeededRng) -> String:
	var candidates: Array[int] = []
	var weights: Array[float] = []

	for biome_key in profile.biomes.keys():
		var biome: BiomeType.Type = biome_key as BiomeType.Type
		if BiomeType.can_support_life(biome):
			candidates.append(biome_key as int)
			weights.append(profile.biomes[biome_key] as float)

	if candidates.is_empty():
		return "Unknown"

	var total: float = 0.0
	for w in weights:
		total += w
	var r: float = rng.randf() * total
	for i in range(candidates.size()):
		r -= weights[i]
		if r <= 0.0:
			return BiomeType.to_string_name(candidates[i] as BiomeType.Type)
	return BiomeType.to_string_name(candidates[candidates.size() - 1] as BiomeType.Type)


## Calculates population based on profile and territorial control.
static func _calculate_population(
	profile: PlanetProfile,
	pop: NativePopulation,
	rng: SeededRng
) -> int:
	var base_density: float = 0.0
	match pop.tech_level:
		TechnologyLevel.Level.STONE_AGE:
			base_density = 0.1
		TechnologyLevel.Level.BRONZE_AGE, TechnologyLevel.Level.IRON_AGE:
			base_density = 1.0
		TechnologyLevel.Level.CLASSICAL, TechnologyLevel.Level.MEDIEVAL:
			base_density = 5.0
		TechnologyLevel.Level.RENAISSANCE:
			base_density = 15.0
		TechnologyLevel.Level.INDUSTRIAL:
			base_density = 50.0
		_:
			base_density = 200.0

	base_density *= profile.habitability_score / 10.0

	var habitable_surface: float = profile.get_habitable_surface()
	var surface_area_km2: float = 510.1e6 * maxf(profile.gravity_g, 0.5)
	var controlled_area: float = surface_area_km2 * habitable_surface * pop.territorial_control

	var pop_count: float = controlled_area * base_density * rng.randf_range(0.5, 1.5)

	return maxi(100, roundi(pop_count))


## Generates cultural traits for a population.
static func _generate_cultural_traits(
	profile: PlanetProfile,
	tech_level: TechnologyLevel.Level,
	rng: SeededRng
) -> Array[String]:
	var traits: Array[String] = []

	if profile.ocean_coverage > 0.5:
		if rng.randf() < 0.7:
			traits.append("seafaring")
	if profile.ice_coverage > 0.2:
		if rng.randf() < 0.6:
			traits.append("cold-adapted")
	if profile.volcanism_level > 0.5:
		if rng.randf() < 0.5:
			traits.append("volcanic-reverent")
	if profile.weather_severity > 0.6:
		if rng.randf() < 0.5:
			traits.append("weather-wise")

	if tech_level >= TechnologyLevel.Level.INDUSTRIAL:
		if rng.randf() < 0.6:
			traits.append("industrious")
	if tech_level >= TechnologyLevel.Level.SPACEFARING:
		if rng.randf() < 0.7:
			traits.append("expansionist")

	var random_traits: Array[String] = [
		"warlike", "peaceful", "mercantile", "artistic", "spiritual",
		"pragmatic", "traditional", "innovative", "isolationist", "diplomatic"
	]

	var num_random: int = rng.randi_range(1, 3)
	for _i in range(num_random):
		var cultural_trait: String = random_traits[rng.randi_range(0, random_traits.size() - 1)]
		if cultural_trait not in traits:
			traits.append(cultural_trait)

	return traits
