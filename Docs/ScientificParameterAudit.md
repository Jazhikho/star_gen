# Scientific Parameter Audit

Date: `2026-04-19`

Purpose: audit every shipped science-facing parameter in mainline against three requirements:

1. the cited source actually supports the claim,
2. the parameter materially changes generation in the claimed direction,
3. tests prove the contract and catch regressions.

Disposition meanings:

- `valid`: source, wiring, and tests all line up.
- `fix source`: the generator effect is real, but source mapping needed correction.
- `fix wiring`: source intent is valid, but generation did not yet follow the claimed behavior.
- `fix tests`: source and wiring are acceptable, but contract coverage was too weak.
- `fix wording`: the setting stays, but help text had to be narrowed to match what the code really does.
- `remove/downgrade`: not strong enough to remain on the science-backed surface as-is.

## Galaxy Priors

| Parameter ID | UI Surface | Intended scientific claim | Source IDs | Consuming generator code | Expected outcome change | Test coverage status | Disposition |
|---|---|---|---|---|---|---|---|
| `galaxy_type` | Galaxy Studio | Broad morphology family changes the structural starting point. | `park2007`, `behroozi2019`, `oohama2009`, `laurikainen2010` | `GalaxyRealismProfileBuilder.Build`, density models | Different family resolves to different subtype, arm availability, bulge/disc balance, and downstream fields. | Covered by config/profile tests. | `valid` |
| `subtype_mode` | Galaxy Studio | Early vs later subtype bias changes bulge strength and resolved subtype. | `oohama2009`, `laurikainen2010` | `GalaxyRealismProfileBuilder.GetSubtypeBias` | Earlier settings push toward smoother, bulge-heavier forms. | Covered by config/profile tests; expanded in audit pass. | `valid` |
| `num_arms` | Galaxy Studio | Spiral arm count changes the arm pattern and arm-region coverage. | `hart2017` | `GalaxyScientificFieldEvaluator.IsNearSpiralArm`, `SpiralDensityModel` | More arms spread arm influence over more azimuthal sectors. | Was under-tested; strengthened. | `fix source`, `fix tests` |
| `arm_pitch_angle_deg` | Galaxy Studio | Pitch angle changes how tightly or loosely the arms wind. | `hart2017`, `lingard2021` | `GalaxyScientificFieldEvaluator.IsNearSpiralArm`, `SpiralDensityModel` | Larger pitch opens the arms and shifts arm-region detection. | Was under-tested; strengthened. | `fix source`, `fix tests` |
| `arm_amplitude` | Galaxy Studio | Stronger arms produce stronger arm contrast and star-forming concentration. | `kennicutt1998` | `SpiralDensityModel`, `GalaxyScientificFieldEvaluator.CalculateClusterProbability` | Larger values increase arm prominence and local star-formation weighting. | Existing coverage plus audit tests. | `valid` |
| `bar_mode` | Galaxy Studio | Bar bias changes whether a barred morphology resolves and how strong it is. | `diazgarcia2016` | `GalaxyRealismProfileBuilder.ResolveBarState` | Preferred bars change inner structure and bar-region outcomes. | Existing profile tests. | `valid` |
| `arm_mechanism_preference` | Galaxy Studio | Different spiral-pattern families change ordered vs broken arm structure. | `hart2017`, `lingard2021` | `GalaxyRealismProfileBuilder.ResolveArmMechanism`, `ResolveArmWidth` | Grand-design, multi-armed, and flocculent modes materially change arm width and region influence. | Was source-light; strengthened. | `fix source`, `fix tests` |
| `halo_mass_log10_solar` | Galaxy Studio | Halo mass shifts family realization toward larger, heavier systems. | `behroozi2019` | `GalaxyRealismProfileBuilder.Build*Profile` | Higher mass favors larger and earlier/heavier structures. | Existing validation/profile tests. | `valid` |
| `environment_density_index` | Galaxy Studio | Crowded environments push toward smoother, gas-poorer populations. | `park2007`, `tanaka2004` | `GalaxyRealismProfileBuilder.GetSubtypeBias`, field evaluator | Higher density favors earlier/smoother resolved outcomes and higher hazard. | Existing profile tests; expanded in audit pass. | `valid` |
| `bulge_intensity` | Galaxy Studio | Bulge strength changes central dominance and density. | `oohama2009`, `laurikainen2010` | density models, local density evaluation | Higher values strengthen the central component. | Existing config/density tests. | `valid` |
| `bulge_radius_pc` | Galaxy Studio | Bulge radius changes how far the central concentration spreads. | `oohama2009`, `laurikainen2010` | density models | Larger bulge influences more of the inner galaxy. | Existing config/density tests. | `valid` |
| `radius_pc` | Galaxy Studio | Galaxy size changes overall radial extent. | `behroozi2019` | density models, coordinates | Larger radius expands occupied structure. | Existing tests. | `valid` |
| `disk_scale_length_pc` | Galaxy Studio | Disk scale length changes radial falloff. | `laurikainen2010` | `SpiralDensityModel`, lenticular handling | Larger values keep the disk important farther out. | Existing tests. | `valid` |
| `disk_scale_height_pc` | Galaxy Studio | Disk scale height changes vertical thickness. | `laurikainen2010` | density models, coordinates | Larger values thicken the disc. | Existing tests. | `valid` |
| `star_density_multiplier` | Galaxy Studio | Global crowding scale changes overall fullness after structure is resolved. | `kennicutt1998` | density normalization and downstream slot counts | Larger values increase total crowding without changing family morphology. | Wording tightened; not treated as a pure literature-calibrated law. | `fix wording` |
| `ellipticity` | Galaxy Studio | Intrinsic flattening changes how stretched the galaxy appears. | `rodriguezpadilla2013` | spiral/elliptical density, coordinates | Higher ellipticity flattens the projected structure. | Was uncited; strengthened. | `fix source`, `fix tests` |
| `irregularity_scale` | Galaxy Studio | Higher irregularity increases asymmetry and clumpiness in dwarf/irregular forms. | `behroozi2019` | `IrregularDensityModel` | Higher values increase broken, lopsided structure. | Existing tests are modest; acceptable for current deterministic proxy. | `fix wording` |
| `ghz_inner_radius_pc` | Galaxy Studio | Inner GHZ edge shifts the crowded hazardous region outward or inward. | `forgan2017`, `spitoni2017` | `GalaxyScientificFieldEvaluator.CalculateGhzWeight` | Moving inward/outward changes GHZ weighting. | Existing tests; expanded in audit pass. | `valid` |
| `ghz_outer_radius_pc` | Galaxy Studio | Outer GHZ edge shifts how far useful chemistry extends. | `forgan2017`, `spitoni2017` | `GalaxyScientificFieldEvaluator.CalculateGhzWeight` | Larger values extend positive GHZ weighting outward. | Existing tests; expanded in audit pass. | `valid` |
| `ghz_transition_width_pc` | Galaxy Studio | Transition width softens GHZ edges. | `forgan2017` | `GalaxyScientificFieldEvaluator.CalculateGhzWeight` | Larger values smooth the GHZ transition. | Existing tests; expanded in audit pass. | `valid` |
| `metallicity_gradient_dex_per_kpc` | Galaxy Studio | Metallicity declines with radius and changes planet-building ingredient availability. | `apogee2024`, `cmetall2024` | `GalaxyScientificFieldEvaluator.CalculateMetallicity` | Steeper negative gradients enrich the inner galaxy more strongly. | Existing tests; expanded in audit pass. | `valid` |
| `star_formation_efficiency` | Galaxy Studio | Higher efficiency raises local young-star and cluster scaffolding rates. | `kennicutt1998` | realism profile, field evaluator | Higher SFE raises local cluster probability and young-region weighting. | Existing tests; expanded in audit pass. | `valid` |

## Stellar Priors

| Parameter ID | UI Surface | Intended scientific claim | Source IDs | Consuming generator code | Expected outcome change | Test coverage status | Disposition |
|---|---|---|---|---|---|---|---|
| `stellar_imf_form` | Galaxy Studio, System Studio | IMF form changes the low-mass vs high-mass object mix. | `kroupa2001`, `chabrier2003`, `kirkpatrick2024` | `StellarMassSampler`, `StarGenerator` | Kroupa and Chabrier produce measurably different mass distributions. | Existing distribution tests plus audit-specific materiality tests. | `valid` |
| `stellar_imf_variation_mode` | Galaxy Studio, System Studio | Metallicity/age modulation shifts the IMF where context exists. | `li2023` | `StellarMassSampler`, `StarGenerator` | Context-rich generation shifts mass sampling; standalone system use is weaker. | Help wording must stay narrow; tests should match current context support. | `fix wording`, `fix tests` |
| `stellar_isochrone_model` | Galaxy Studio, System Studio | Isochrone choice changes derived stellar properties and stage thresholds. | `choi2016`, `bressan2012`, `cummings2018` | `StellarIsochroneApproximator`, `StarGenerator` | Same mass/age/metallicity resolves to different radius/luminosity/temperature. | Existing and expanded. | `valid` |
| `stellar_multiplicity_scale` | Galaxy Studio, System Studio | Companion scale changes multiplicity frequency and hierarchy richness. | `duchene2013`, `raghavan2010`, `tokovinin2021`, `moedistefano2017` | `StellarConfigGenerator.DetermineStarCount`, companion/hierarchy builders | Higher values produce more binaries and richer hierarchies. | Existing and expanded. | `valid` |

## Planetary Priors

| Parameter ID | UI Surface | Intended scientific claim | Source IDs | Consuming generator code | Expected outcome change | Test coverage status | Disposition |
|---|---|---|---|---|---|---|---|
| `planet_mass_radius_model` | Galaxy Studio, System Studio | Mass-radius model changes planet size and density resolution. | `chenkipping2017`, `otegi2020` | `PlanetMassRadiusTable`, `PlanetPhysicalGenerator` | Different models produce different transition-world radii/densities. | Existing and expanded. | `valid` |
| `planet_envelope_loss_model` | Galaxy Studio, System Studio | Gas-loss model changes close-in envelope retention. | `fulton2017`, `owenwu2017`, `ginzburg2018` | `PlanetarySystemState`, `SystemPlanetGenerator` | Photoevaporation favors more stripped hot planets than core-powered loss. | Existing and expanded. | `valid` |
| `planet_habitable_zone_model` | Galaxy Studio, System Studio | Academic HZ model family changes the classical circumstellar liquid-water reference band used for orbit weighting and downstream environment context. | `kasting1993`, `kopparapu2013`, `kopparapu2014` | `OrbitalMechanics.Habitability`, `StellarConfigGenerator`, `PlanetarySystemState`, `ProfileGenerator`, `OrbitZone` | Kasting keeps the tighter legacy classical band; Kopparapu conservative widens it with the updated climate coefficients; Kopparapu optimistic widens it further to Recent Venus and Early Mars. | Added in this pass. | `valid` |
| `planet_gas_giant_formation_model` | Galaxy Studio, System Studio | Giant-growth model changes giant-planet weighting. | `mordasini2007`, `lambrechtsjohansen2012` | `PlanetarySystemState`, `SystemPlanetGenerator` | Pebble-assisted growth should not underperform core-accretion on the same seed and context. | Existing and expanded. | `valid` |
| `planet_metallicity_coupling_strength` | Galaxy Studio, System Studio | Metal-rich systems should produce more giant-friendly architectures. | `fischervalenti2005` | `PlanetarySystemState`, `SystemPlanetGenerator` | Stronger coupling raises giant-planet weighting in metal-rich systems. | Existing and expanded. | `valid` |
| `planet_rogue_planet_allowance` | Galaxy Studio, System Studio | More disruption pressure favors more disturbed/ejected outcomes. | `mroz2020` | `PlanetarySystemState`, `SystemPlanetGenerator` | Setting currently shifts disturbed low-mass outcomes more than it creates an explicit rogue census. | Wording narrowed to match implementation. | `fix wording`, `fix tests` |
| `planet_moon_formation_bias` | Galaxy Studio, System Studio | Different moon-formation channels change regular vs captured moon styles. | `ronnet2020`, `sasaki2010`, `szulagyi2018`, `jewitthaghighipour2007` | `SystemMoonGenerator`, formation traces | Regular-disk and captured-rich settings alter moon family structure. Canup & Ward (2006) was removed from bibliography tracking after PDF withdrawal; Ronnet / Sasaki / Szulagyi source notes are pending human verification. | Source cleanup pass. | `review pending` |
| `planet_minor_body_outer_system_bias` | Galaxy Studio, System Studio | Outer cold leftovers shift between rockier belts and icier reservoirs. | `demeocarry2014`, `baueretal2017`, `kavelaarsetal2023`, `bernardinellietal2022` | `PlanetarySystemState`, `SystemAsteroidGenerator`, volatile-delivery logic | Comet-leaning settings increase volatile delivery, icy trans-Neptunian-reservoir bias, and source-marked cold-belt provenance. | Existing and expanded. | `valid` |

## Life Models

| Parameter ID | UI Surface | Intended scientific claim | Source IDs | Consuming generator code | Expected outcome change | Test coverage status | Disposition |
|---|---|---|---|---|---|---|---|
| `life_framework` | Galaxy Studio | Top-level life preset sets the default stance for the stage-specific life assumptions. | `lineweaverdavis2002`, `spiegelturner2012`, `forganrice2010`, `mills2024`, `balbi2023`, `kopparapu2014` | `LifePotentialModeling.Resolve` | Different frameworks change the default abiogenesis, complex-life, civilization, and window-weighting choices. | Existing and expanded. | `valid` |
| `abiogenesis_model` | Galaxy Studio | Life starting from chemistry can be modeled optimistically or conservatively. | `lineweaverdavis2002`, `spiegelturner2012` | `LifePotentialModeling.ResolveAbiogenesisMultiplier`, `BiologySupportEvaluator.CalculateAbiogenesisChance` | Rapid Start increases biosphere emergence without directly forcing civilization. | Existing and expanded. | `valid` |
| `complex_life_model` | Galaxy Studio | Simple biospheres and complex ecosystems should be modeled separately. | `mills2024`, `forganrice2010`, `lineweaverdavis2002`, `spiegelturner2012` | `LifePotentialModeling.ResolveComplexLifeMultiplier`, `BiologySupportEvaluator.CalculateComplexLifeChance` | Environmental Windows rewards stable worlds; Rare Earth Filters suppress complex life. | Existing and expanded. | `valid` |
| `civilization_model` | Galaxy Studio | Sentient lineages and technological civilizations should be modeled separately. | `forganrice2010`, `balbi2023` | `LifePotentialModeling.ResolveCivilizationMultiplier`, `BiologySupportEvaluator.CalculateCivilizationChance`, concept/population consumers | Rare Civilizations and oxygen bottlenecks suppress civilization later than sentience. | Required wiring fix in audit pass. | `fix wiring`, `fix tests` |
| `environmental_window_weight` | Galaxy Studio | Long stable habitable windows should matter for later biological stages. | `mills2024` | `LifePotentialModeling.ResolveEnvironmentalWindowMultiplier`, `BiologySupportEvaluator` | Higher weighting rewards long-lived stable worlds more strongly. | Existing and expanded. | `valid` |

## Sentient-World Models

| Parameter ID | UI Surface | Intended scientific claim | Source IDs | Consuming generator code | Expected outcome change | Test coverage status | Disposition |
|---|---|---|---|---|---|---|---|
| `sentient_social_scale_model` | Galaxy/System/Object use-case settings | Population should affect social scale without being the only determinant. | `hamiltonetal2020`, `bettencourtetal2007` | `SentientWorldProfileBuilder` | Source-aligned mode gives administration and group structure more weight while retaining population pressure. | Existing and expanded. | `human audit required` |
| `sentient_technology_diffusion_model` | Galaxy/System/Object use-case settings | Highest available technology, adoption capacity, median access, and adoption lag should be separate diagnostics. | `knez2023`, `stokey2020`, `comin2013`, `cominmestieri2013` | `SentientWorldProfileBuilder` | Source-aligned mode exposes median tech access, invention capacity, adoption-lag pressure, and access inequality instead of only highest tech. | Added in this pass. | `human audit required` |
| `sentient_economic_complexity_model` | Galaxy/System/Object use-case settings | Capability breadth and productive relatedness constrain economic complexity and invention pressure. | `chacuaetal2024`, `ballandetal2022`, `bettencourtetal2007`, `arvidssonetal2023` | `SentientWorldProfileBuilder` | Capability-portfolio mode changes economic complexity and now feeds invention/access diagnostics. | Existing and expanded. | `human audit required` |
| `sentient_legitimacy_model` | Galaxy/System/Object use-case settings | Internal acceptance and external recognition should be separate from raw state capacity. | `vankleefetal2023`, `chowdhury2022` | `SentientWorldProfileBuilder` | Internal/external legitimacy mode changes legal reach and keeps state-capacity/regulation claims explicitly audited. | Existing. | `human audit required` |

## Audit outcome

- The main wiring bug in this pass was the life pipeline: sentient-lineage checks were still reading `CivilizationChance` in downstream consumers. This was corrected.
- The main source gap in this pass was galaxy arm and ellipticity coverage. Those controls now point to reviewed observational morphology papers instead of empty source lists.
- The main wording fix in this pass was for controls that already changed generation but were over-claiming what the code currently does, especially `planet_rogue_planet_allowance`, `stellar_imf_variation_mode`, and `star_density_multiplier`.
- System Studio now reuses the audited stellar, planetary, and life parameter contracts instead of keeping a divergent life slider surface.
- Object Studio is intentionally outside the aggregate-science contract for galaxy, planetary-population, and life priors; it now enforces a direct single-object standard and is tested against direct spec persistence instead.
- Follow-up life-model audit work is tracked separately in [LifeScienceAudit.md](/D:/Game%20Creation/star_gen/Docs/LifeScienceAudit.md): the exposed life parameters remain materially wired and source-backed as controls, but the underlying biology model still needs deeper host-star-history, abiogenesis-opportunity, nutrient-access, oxygenation, and detectability channels to claim stronger scientific grounding.
