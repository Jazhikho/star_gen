## Loads SeededRng and asteroid belt concept domain scripts so class_name types are registered.
## Preload this in concept test runners before they reference concept types.
extends RefCounted

const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _orbital_math: GDScript = preload("res://src/domain/asteroid_belt_concept/OrbitalMathConcept.gd")
const _major_input: GDScript = preload("res://src/domain/asteroid_belt_concept/MajorAsteroidInputConcept.gd")
const _belt_spec: GDScript = preload("res://src/domain/asteroid_belt_concept/AsteroidBeltSpecConcept.gd")
const _asteroid_data: GDScript = preload("res://src/domain/asteroid_belt_concept/AsteroidDataConcept.gd")
const _belt_data: GDScript = preload("res://src/domain/asteroid_belt_concept/AsteroidBeltDataConcept.gd")
const _belt_generator: GDScript = preload("res://src/domain/asteroid_belt_concept/AsteroidBeltGeneratorConcept.gd")
