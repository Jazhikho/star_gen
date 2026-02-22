## Loads population domain scripts so class_name types are registered before test scripts.
## Preload this in RunTestsHeadless before population tests reference ClimateZone, BiomeType, etc.
## PlanetProfile is not preloaded here; it loads via test preload and pulls in these enums first.
extends RefCounted

# Population domain (Stage 1: Planet Profile Model) - enums only; PlanetProfile loads via tests
const _climate_zone: GDScript = preload("res://src/domain/population/ClimateZone.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")
const _habitability_category: GDScript = preload("res://src/domain/population/HabitabilityCategory.gd")
# Population framework (Government/Regime, Tech level) - load before NativePopulation so types resolve
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
# Population framework (Stage 4: NativePopulation) - load so class_name is registered before tests
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _native_population_generator: GDScript = preload("res://src/domain/population/NativePopulationGenerator.gd")
# Population framework (Stage 5: Colony)
const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _native_relation: GDScript = preload("res://src/domain/population/NativeRelation.gd")
const _colony: GDScript = preload("res://src/domain/population/Colony.gd")
const _colony_generator: GDScript = preload("res://src/domain/population/ColonyGenerator.gd")
# Population framework (Stage 6: Integration boundary)
const _planet_population_data: GDScript = preload("res://src/domain/population/PlanetPopulationData.gd")
const _population_generator: GDScript = preload("res://src/domain/population/PopulationGenerator.gd")
# Population integration (Phase 5: Population wiring)
const _population_probability: GDScript = preload("res://src/domain/population/PopulationProbability.gd")
const _population_likelihood: GDScript = preload("res://src/domain/population/PopulationLikelihood.gd")
const _population_seeding: GDScript = preload("res://src/domain/population/PopulationSeeding.gd")
# Station framework (outposts, space stations)
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_service: GDScript = preload("res://src/domain/population/StationService.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _outpost_authority: GDScript = preload("res://src/domain/population/OutpostAuthority.gd")
const _outpost: GDScript = preload("res://src/domain/population/Outpost.gd")
const _space_station: GDScript = preload("res://src/domain/population/SpaceStation.gd")
const _station_placement_rules: GDScript = preload("res://src/domain/population/StationPlacementRules.gd")
const _station_spec: GDScript = preload("res://src/domain/population/StationSpec.gd")
const _station_generator: GDScript = preload("res://src/domain/population/StationGenerator.gd")
