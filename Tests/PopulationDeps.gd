## Loads population domain scripts so class_name types are registered before test scripts.
## Preload this in RunTestsHeadless before population tests reference ClimateZone, BiomeType, etc.
## PlanetProfile is not preloaded here; it loads via test preload and pulls in these enums first.
extends RefCounted

# Population domain (Stage 1: Planet Profile Model) - enums only; PlanetProfile loads via tests
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")
const _habitability_category: GDScript = preload("res://src/domain/population/HabitabilityCategory.gd")
