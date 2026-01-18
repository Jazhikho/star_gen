## Shared utility functions for all generators.
## Provides common operations like ID generation and name generation.
class_name GeneratorUtils
extends RefCounted

const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Generates a unique ID for a celestial body.
## @param body_type: The type of body (star, planet, moon, asteroid).
## @param rng: The random number generator.
## @return: A unique ID string.
static func generate_id(body_type: String, rng: SeededRng) -> String:
	var random_part: int = rng.randi() % 1000000
	return "%s_%06d" % [body_type, random_part]


## Generates a procedural name for a star.
## @param rng: The random number generator.
## @return: A star name.
static func generate_star_name(rng: SeededRng) -> String:
	var prefixes: Array[String] = [
		"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
		"Kappa", "Lambda", "Sigma", "Tau", "Omega", "Nova", "Vega", "Rigel"
	]
	var suffixes: Array[String] = [
		"Centauri", "Eridani", "Cygni", "Draconis", "Orionis", "Lyrae",
		"Pegasi", "Aquarii", "Tauri", "Leonis", "Scorpii", "Carinae"
	]
	
	var prefix: String = prefixes[rng.randi() % prefixes.size()]
	var suffix: String = suffixes[rng.randi() % suffixes.size()]
	
	# Sometimes add a catalog number
	if rng.randf() < 0.3:
		var catalog_num: int = rng.randi_range(1, 999)
		return "%s %s %d" % [prefix, suffix, catalog_num]
	
	return "%s %s" % [prefix, suffix]


## Generates a procedural name for a planet.
## @param rng: The random number generator.
## @param parent_name: Optional parent star name for naming convention.
## @return: A planet name.
static func generate_planet_name(rng: SeededRng, parent_name: String = "") -> String:
	if parent_name and rng.randf() < 0.6:
		var letter: String = char(ord("b") + rng.randi() % 8)
		return "%s %s" % [parent_name, letter]
	
	var prefixes: Array[String] = [
		"Kepler", "Gliese", "HD", "TRAPPIST", "TOI", "Proxima", "Ross", "Wolf"
	]
	var prefix: String = prefixes[rng.randi() % prefixes.size()]
	var number: int = rng.randi_range(1, 9999)
	var letter: String = "b"
	letter = String.chr(letter.unicode_at(0) + rng.randi() % 6)
	
	return "%s-%d%s" % [prefix, number, letter]


## Generates a procedural name for a moon.
## @param rng: The random number generator.
## @param parent_name: Optional parent planet name.
## @return: A moon name.
static func generate_moon_name(rng: SeededRng, parent_name: String = "") -> String:
	if parent_name and rng.randf() < 0.5:
		var roman: Array[String] = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII"]
		var numeral: String = roman[rng.randi() % roman.size()]
		return "%s %s" % [parent_name, numeral]
	
	var names: Array[String] = [
		"Io", "Europa", "Ganymede", "Callisto", "Titan", "Triton", "Charon",
		"Enceladus", "Mimas", "Rhea", "Dione", "Tethys", "Oberon", "Ariel"
	]
	var base_name: String = names[rng.randi() % names.size()]
	
	if rng.randf() < 0.4:
		var suffix: int = rng.randi_range(1, 99)
		return "%s-%d" % [base_name, suffix]
	
	return base_name


## Generates a procedural name for an asteroid.
## @param rng: The random number generator.
## @return: An asteroid designation.
static func generate_asteroid_name(rng: SeededRng) -> String:
	var year: int = rng.randi_range(1990, 2150)
	var letters: String = ""
	var base_char: String = "A"
	letters += String.chr(base_char.unicode_at(0) + rng.randi() % 26)
	letters += String.chr(base_char.unicode_at(0) + rng.randi() % 26)
	var number: int = rng.randi_range(1, 999)
	
	return "%d %s%d" % [year, letters, number]


## Picks a random value from weighted options.
## @param options: Array of values to choose from.
## @param weights: Array of weights (same length as options).
## @param rng: The random number generator.
## @return: The selected option.
static func weighted_choice(options: Array, weights: Array[float], rng: SeededRng) -> Variant:
	if options.is_empty() or options.size() != weights.size():
		return null
	
	var total_weight: float = 0.0
	for w in weights:
		total_weight += w
	
	var roll: float = rng.randf() * total_weight
	var cumulative: float = 0.0
	
	for i in range(options.size()):
		cumulative += weights[i]
		if roll < cumulative:
			return options[i]
	
	return options[options.size() - 1]
