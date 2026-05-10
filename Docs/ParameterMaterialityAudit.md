# Parameter Materiality Audit

Date: `2026-04-22`

This audit is the source of truth for StarGen's active parameter surface after the materiality cleanup. The goal is simple:

- retained generation-facing parameters must materially affect downstream output,
- override seams must stay explicit and be labeled as overrides rather than science,
- runtime and presentation controls must stay out of the shared generation catalog,
- derived fields must either drive later generation, remain necessary intermediates, or be treated as trace-only state.

## Startup And Generation Flow

`project.godot`
- launches `res://src/app/MainApp.tscn`

`MainApp`
- mounts `MainMenuScreen`
- opens `GalaxyGenerationScreen`, `SystemGenerationScreen`, or `ObjectGenerationScreen`

Studios
- `GalaxyGenerationScreen` builds `GalaxyConfig`
- `SystemGenerationScreen` builds `SolarSystemSpec`
- `ObjectGenerationScreen` builds direct object requests/specs

Galaxy-to-system path
- `GalaxyConfig` -> `Galaxy.Generate(...)`
- `GalaxyScientificFieldEvaluator.Evaluate(...)` derives local context
- `GalaxySystemGenerator.CreateSpecFromStar(...)` builds `SolarSystemSpec`
- `StellarConfigGenerator.Generate(...)`
- `OrbitSlotGenerator.GenerateAllSlots(...)`
- `SystemPlanetGenerator.Generate(...)`
- `SystemMoonGenerator.Generate(...)`
- optional `SystemAsteroidGenerator.Generate(...)`
- optional `PopulationGenerator.Generate(...)`

System-only path
- `SolarSystemSpec`
- `StellarConfigGenerator.Generate(...)`
- `OrbitSlotGenerator.GenerateAllSlots(...)`
- `SystemPlanetGenerator.Generate(...)`
- `SystemMoonGenerator.Generate(...)`
- optional `SystemAsteroidGenerator.Generate(...)`
- optional `PopulationGenerator.Generate(...)`

Readout and provenance path
- generated bodies and systems persist the selected `UseCaseSettings`, `StellarProfile`, and `PlanetaryProfile`
- viewer summaries and `InspectorPanel` consume those stored settings for presentation-only readouts such as UWP visibility

## Active Generation Priors

| Ids | Class | Surfaced In | Serialized Through | Derived Through | Consumed By | Outputs Changed |
| --- | --- | --- | --- | --- | --- | --- |
| `galaxy_seed` | `Generation prior` | Galaxy Studio seed control | `GalaxyConfig`, `GalaxySpec` | `GalaxyRealismProfileBuilder.Build(...)` | `Galaxy.Generate(...)`, `GalaxyScientificFieldEvaluator.Evaluate(...)` | Repeatable galaxy structure, repeatable local stellar context |
| `galaxy_type`, `subtype_mode`, `bar_mode`, `arm_mechanism_preference` | `Generation prior` | Galaxy Studio type section | `GalaxyConfig` | `GalaxyRealismProfileBuilder.Build(...)`, resolved `GalaxySpec` family fields | galaxy density-model selection, morphology evaluation | family resolution, bar state, arm behavior, regional morphology |
| `num_arms`, `arm_pitch_angle_deg`, `arm_amplitude` | `Generation prior` | Galaxy Studio structure section | `GalaxyConfig` | resolved spiral arm fields | `Galaxy.Generate(...)`, `GalaxyScientificFieldEvaluator.Evaluate(...)` | arm coverage, arm/interarm placement, local region kind |
| `halo_mass_log10_solar`, `environment_density_index`, `star_formation_efficiency` | `Generation prior` | Galaxy Studio scientific priors section | `GalaxyConfig` | `GalaxyRealismProfileBuilder.Build(...)`, `GalaxyScientificFieldEvaluator.Evaluate(...)` | galaxy field evaluation, galaxy-to-system context | stellar density priors, metallicity priors, multiplicity context, habitability context |
| `bulge_intensity`, `bulge_radius_pc`, `radius_pc`, `disk_scale_length_pc`, `disk_scale_height_pc`, `star_density_multiplier`, `ellipticity`, `irregularity_scale` | `Generation prior` | Galaxy Studio structure and size sections | `GalaxyConfig` | resolved morphology and scale fields | `Galaxy.Generate(...)`, galaxy density models | shape, density falloff, global scale, local star counts |
| `ghz_inner_radius_pc`, `ghz_outer_radius_pc`, `ghz_transition_width_pc`, `metallicity_gradient_dex_per_kpc` | `Generation prior` | Galaxy Studio size section | `GalaxyConfig` | `GalaxyScientificFieldEvaluator.Evaluate(...)` | galaxy field evaluation, galaxy-to-system spec creation | galactic habitability weighting, regional metallicity prior, downstream system chemistry context |
| `generation_seed` | `Generation prior` | System Studio seed control | `SolarSystemSpec` | seed-family derivation across system stages | `StellarConfigGenerator`, `SystemPlanetGenerator`, `SystemMoonGenerator`, `PopulationGenerator` | repeatable stars, planets, moons, and populations |
| `star_count_min`, `star_count_max`, `spectral_class_hints`, `system_age_years`, `system_metallicity` | `Generation prior` | System Studio system controls | `SolarSystemSpec` | `StellarConfigGenerator` inputs, `PlanetarySystemState.Build(...)` | stellar scaffold, orbit hosts, planet generation | star architecture, star chemistry and age context, planet weighting |
| `stellar_imf_form`, `stellar_imf_variation_mode`, `stellar_isochrone_model`, `stellar_multiplicity_scale` | `Generation prior` | Galaxy and System Studio stellar sections | `StellarGenerationProfile` | stored on `GalaxyConfig` and `SolarSystemSpec` | `StellarMassSampler`, `StarGenerator`, `StellarIsochroneApproximator`, `StellarConfigGenerator` | stellar mass mix, stellar evolution outputs, companion frequency and layout |
| `planet_mass_radius_model`, `planet_envelope_loss_model`, `planet_habitable_zone_model`, `planet_gas_giant_formation_model`, `planet_metallicity_coupling_strength`, `planet_rogue_planet_allowance`, `planet_moon_formation_bias`, `planet_minor_body_outer_system_bias` | `Generation prior` | Galaxy and System Studio planetary sections | `PlanetaryGenerationProfile` | `PlanetarySystemState.Build(...)`, `OrbitHost.CalculateZones(...)` | `SystemPlanetGenerator`, `SystemMoonGenerator`, `SystemAsteroidGenerator`, `ProfileGenerator` | planet-class mix, moon architecture, small-body behavior, environment and habitability context |
| `life_framework`, `abiogenesis_model`, `complex_life_model`, `civilization_model`, `environmental_window_weight` | `Generation prior` | Galaxy and System Studio life sections, Object Studio life section | `GenerationUseCaseSettings` | `LifePotentialModeling`, `BiologySupportEvaluator`, `PopulationProbability` | `BiologySupportEvaluator`, `PopulationLikelihood`, `PopulationGenerator`, `SentientWorldProfileBuilder` | biosphere support, native-life pressure, sentience and civilization gating, populated-world outcomes |

## Generator Overrides

| Ids | Why It Stays | Consumed By | Concrete Output |
| --- | --- | --- | --- |
| `ruleset_mode` | Intentional compatibility seam for RPG-facing generation pressure | `RpgCompatibilityProfile.Resolve(...)`, `GalaxySystemGenerator.CreateSpecFromStar(...)`, `SystemPlanetGenerator`, `PopulationProbability` | mainworld pressure, settlement pressure, readout defaults |
| `force_life_on_supportable_worlds` | Explicit override for deterministic biology support acceptance | `PopulationLikelihood.ShouldGenerateNatives(...)` | supportable worlds retain native life without stochastic rejection |
| `mainworld_policy` | Compatibility seam for focal-world pressure | compatibility profile resolution, `TravellerSystemGenerator.ApplyTravellerMainworld(...)`, system weighting helpers | mainworld availability and selection pressure |
| `compatibility_temperate_slot_fill_multiplier` | Compatibility seam for slot occupancy pressure | `SystemPlanetGenerator` slot fill weighting | more or fewer temperate worlds survive slot filling |
| `compatibility_harsh_slot_fill_multiplier` | Compatibility seam for harsh-world retention | `SystemPlanetGenerator` slot fill weighting | harsher worlds are pruned or preserved |
| `compatibility_terrestrial_world_weight_multiplier` | Compatibility seam for mainworld class preference | `SystemPlanetGenerator` class weighting | rocky and super-Earth vs mini-Neptune or giant balance near mainworld-favored slots |
| `compatibility_native_life_probability_multiplier` | Compatibility seam for RPG-facing biosphere pressure | `PopulationProbability.CalculateNativeProbability(...)` | supportable worlds keep native life more or less often |
| `compatibility_colony_probability_multiplier` | Compatibility seam for settlement pressure | `PopulationProbability.CalculateColonyProbability(...)` | colony and outpost frequency |

## Runtime And Presentation Controls

These controls are still useful, but they are not part of the shared generation catalog anymore.

| Ids | Class | Why It Is Not A Scientific Parameter | Downstream Effect |
| --- | --- | --- | --- |
| `include_asteroid_belts` | `Runtime/orchestration control` | only decides whether the belt stage runs | asteroid belts are present or absent |
| `major_asteroid_display_count` | `Runtime/orchestration control` | selects how many already-generated eligible belt large objects are promoted to inspectable bodies | largest eligible large-object body entries exposed per belt |
| `major_asteroid_min_diameter_km` | `Runtime/orchestration control` | selects the minimum diameter for promotion to inspectable body records, not the physical size-distribution slope | which belt large objects are eligible for System Viewer/Object Viewer handoff |
| `generate_population` | `Runtime/orchestration control` | only decides whether the population stage runs | natives, colonies, and sentient-world baselines are present or absent |
| `show_traveller_readouts` | `Presentation/readout control` | only affects UI/readout visibility | UWP and Traveller-style sections appear or stay hidden |

## Legacy Compatibility Alias

`GenerationUseCaseSettings.LifePotentialModel`

- status: keep only as a serialization bridge
- reason: older saves still store `life_potential_model`
- active runtime use: none after this cleanup
- active runtime logic now resolves recommended permissiveness from `LifeFramework`, not from the legacy alias

## Derived Planetary State

### Derived Fields That Must Stay Material

| Fields | Why They Stay | Consumed By | Outputs Changed |
| --- | --- | --- | --- |
| `SnowLineAu`, `SnowLineGiantFormationScalar`, `GasGiantWeight` | aggregate giant-formation context | `SystemPlanetGenerator` | giant-planet frequency and radial turnover |
| `SolidBudgetScalar`, `GasBudgetScalar`, `EscapePressureProxy` | aggregate solids, gas, and retention context | `SystemPlanetGenerator`, `SystemAsteroidGenerator` | rocky vs volatile-rich mix, gas retention, belt composition pressure |
| `HabitableZoneInnerAu`, `HabitableZoneOuterAu`, `GetHabitableZoneAlignment(...)` | circumstellar habitability context | `SystemPlanetGenerator`, `ProfileGenerator` | habitable-slot weighting, environment profiles |
| `XuvActivityScalar`, `OuterReservoirScalar`, `VolatileDeliveryScalar`, `BombardmentScalar` | irradiation and delivery context | `SystemPlanetGenerator`, `ProfileGenerator` | volatile delivery, stripping pressure, bombardment pressure |
| `ImpactStirring`, `GiantScatteringScalar` | dynamically active architecture context | `SystemPlanetGenerator`, `SystemMoonGenerator` | stripped worlds, irregular moons, inner delivery boosts |

### Derived Fields That Remain Intermediate, Not Parameter-Like

| Fields | Status | Why |
| --- | --- | --- |
| `HostMassDiskLifetimeScalar`, `HostMassSolidReservoirScalar` | keep as intermediate derived state | used to build later material fields, but should not be treated as user-facing knobs |
| `MetallicityEnrichment` | keep as intermediate derived state | used while building later material fields, but not a surfaced control |
| `Profile` | keep as carry-through state | needed because later generators consume the selected planetary priors directly |

## Studio Surface Rules

- Science panels should only expose `Generation prior` controls.
- Override panels can expose `Generator override` seams.
- Runtime and presentation controls can stay surfaced, but they must use explicit non-science wording.
- Object Studio can keep direct authoring and presentation controls in its own catalog because it is not the shared science surface.

## Current Result

After this cleanup:

- the shared `GenerationParameterCatalog` generation surfaces include only priors and override seams,
- `show_traveller_readouts`, `generate_population`, and `include_asteroid_belts` are split into explicit auxiliary surfaces,
- life permissiveness baselines resolve from the active `LifeFramework`,
- the machine-readable `ParameterMaterialityRegistry` and the regression suite now guard against orphaned catalog parameters.
