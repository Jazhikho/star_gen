# Source Utilization Plan

This plan tracks the source-to-generator cleanup separately from the annotated bibliography. The goal is to make reviewed sources actively inform StarGen generation where appropriate, while keeping human review as the final authority for scientific claims and release decisions.

## Working rules

- Prioritize sources that touch active generation behavior over documentation-only sources.
- Prefer narrow source-alignment changes over broad retuning.
- Keep each source note as the source of truth for reviewer concerns, AI response, implementation status, and follow-up.
- Do not treat AI summaries or draft PDF extracts as final scientific authority.
- If a source is unsuitable, rejected, or only background context, record that disposition and remove it from active support claims.
- When source review exposes a broken generation process rather than a narrow comment/provenance mismatch, branch the work, retune the process there, and validate it before merging back.

## Per-source workflow

1. Confirm source status in `Sources/Texts/*.txt` and `Sources/AnnotatedBibliography.md`.
2. Locate the generation surface the source should affect: code, parameter catalog, tooltip/help text, audit table, or tests.
3. Compare the source note against current behavior and classify it as:
   - `supports current use`
   - `partly implemented`
   - `underutilized`
   - `requires code change`
   - `documentation-only`
   - `rejected/replaced as active support`
4. Implement only narrow, defensible changes when the reviewed source clearly contradicts or refines current behavior.
5. Add tests for changed behavior and provenance when the source affects generation.
6. Update the source note, bibliography annotation, end-to-end audit, and AI provenance log.
7. Run `dotnet build .\StarGen.sln` and the Godot headless harness for code changes.

## Completed source-to-generator slices

| Slice | Sources | Current status |
|---|---|---|
| Mass-radius baseline | `ChenKipping2017` | Partly implemented with seeded scatter and Terran/Neptunian/Jovian classification probabilities. Full Forecaster posterior sampling remains follow-up. |
| Mass-radius branch refinement | `Otegi2020` | Partly implemented with rocky branch capped near the source endpoint and deterministic uncertainty sampling. Pure-water separator remains follow-up. |
| Radius valley / envelope loss | `Fulton2017`, `OwenWu2017`, `Ginzburg2018` | Partly implemented with radius-valley provenance, period-limited regime membership, and distinct photoevaporation/core-powered slopes and pressure drivers. Full escape/population model remains follow-up. |
| Orbital architecture and stability spacing | `Obertas2017`, `Rice2023`, `HeEtAl2020`, with `Petit2018`, `Petit2020`, `Tamayo2020`, `Laskar2017`, `FangMargot2013`, `ObertasTamayo2023`, `Outland2020` disposition updates | Partly implemented as source-marked slot scaffolding and planet provenance: exact mutual-Hill spacing, active spacing policy, source IDs, period ratio, architecture mass proxy, and threshold are now recorded. AMD/SPOCK/inclination/dynamical-packing models remain follow-up. |
| Planet occurrence and architecture demographics | `Petigura2013`, `Bryson2021`, `BergstenEtAl2023`, `MentCharbonneau2023`, `CuiEtAl2026`, `GillisEtAl2026`, with `KunimotoEtAl2022`, `WanderleyEtAl2025`, `VanZandtEtAl2025` disposition updates | Partly implemented as host demographic scalars in `PlanetarySystemState`: FGK close-in/HZ anchors, no automatic M-dwarf HZ surplus, mid-to-late M close-in small-planet boost, and close-in sub-Neptune/hot-giant suppression. Exact period-radius occurrence tables and conditional outer giants remain follow-up. |
| Disk, formation, migration, giants, and volatile delivery | `Pascucci2016`, `Ribas2015`, `Izidoro2017`, `Fernandes2019`, `RaymondIzidoro2017`, `Mordasini2007`, `LambrechtsJohansen2012`, `TanakaTakeuchiWard2002` | Partly implemented as active formation-state provenance and deterministic generator hooks: host-mass solid-reservoir scaling, adjusted disk lifetime, snow-line giant turnover, pebble/core-accretion branch weighting, giant-scattering volatile delivery, and Type-I migration likelihood diagnostics. Full disk-population fitting, resonant-chain generation/breakup, non-isothermal migration, and asteroid-belt architecture remain follow-up. |
| Habitable zones, atmospheres, and life bottlenecks | `Kasting1993`, `Kopparapu2013`, `Kopparapu2014`, `Balbi2023`, `WordsworthKreidberg2022`, `ChatterjeeEtAl2026`, `BiassoniEtAl2023`, `VissapragadaEtAl2022`, `LugerBarnes2015` | Partly implemented as atmosphere-retention and composition-regime provenance: active secondary-atmosphere source IDs, escape pressure, retention scalar, pre-main-sequence XUV risk, atmosphere regime, composition family, and oxygen context are now recorded. Full hydrodynamic escape, volcanic revival, planet-mass HZ correction, and atmospheric evolution remain follow-up. |
| Moon formation and moon-system architecture | `Ronnet2020`, `Sasaki2010`, `Szulagyi2018`, `JewittHaghighipour2007`, with `BenistyEtAl2021`, `HellerBarnes2013`, `MalamudPerets2019`, `NakajimaEtAl2022` dispositions | Partly implemented as source-marked moon-channel and architecture provenance. Regular CPD moons now use a satellite-scale mass budget and record Ronnet/Sasaki/Szulagyi active sources, CPD/Hill diagnostics, and Galilean/Saturnian/ice-giant architecture modes. Captured moons are separated under Jewitt/Haghighipour irregular-satellite context with retrograde-favored orbit style. Benisty is CPD observational context, Heller/Barnes is habitability/tidal-heating context, and Malamud/Perets plus Nakajima remain underutilized terrestrial-impact follow-ups. |
| Galaxy schema and Milky-Way analog structure | `BlandHawthornGerhard2016`, with `Bovy2017`, `KhoperskovEtAl2024`, `HuntVasiliev2025`, `Hayden2014`, `Kennicutt1998`, `Chabrier2003`, `Kroupa2001` context | Partly implemented as first-class Milky-Way structural schema fields: thin/thick disk scale lengths and heights, bar half-length, solar-circle radius, circular velocity at the solar circle, and stellar-mass scale now serialize through config/spec/profile. Spiral density uses the thin/thick disk fields, bar density and bar-region classification use explicit bar geometry, and source/status metadata is surfaced through source notes and galaxy science references. `GalaxyMassComponentBudget`, `GalaxyRotationCurveDiagnostic`, and `GalaxyDynamicsDiagnostic` now provide the full current diagnostic-only galaxy-hardening surface: mass components, rotation anchors, component velocity contributions, pattern speed, corotation, local mass-budget proxies, and non-Milky-Way comparison caveats. Calibrated gravitational potential, orbit integration, source-fitted active local-mass behavior, and family-specific non-Milky-Way analog behavior remain follow-up. |
| Small bodies and reservoirs | `DeMeoCarry2014`, `BauerEtAl2017`, `KavelaarsEtAl2023`, `BernardinelliEtAl2022`, with `NapierEtAl2023` context and `Lamy2004` replacement | Partly implemented as source-marked asteroid/comet/TNO reservoir provenance. Inner belts and generated asteroids now record DeMeo/Carry taxonomy-composition provenance; cold belts are labeled as trans-Neptunian reservoirs, placed farther outward, given a lower TNO-scale mass ceiling, and record Kavelaars/Bernardinelli/Bauer source IDs. TNO-style belts and representative bodies now carry diagnostic cold classical, hot classical, resonant, scattered, Centaur, and comet-feeding subfamily metadata, generated systems persist those families as first-class `SmallBodyReservoir` records linked to their current belt anchor, and System Viewer now surfaces reservoir-family weights in overview, selected-belt readouts, and a dedicated reservoir-details section with source IDs, radial span, diagnostic status, station/habitat readiness, and native-life absence caveats. Inspectable belt large objects now use diameter-based semantics, default to the largest 10 candidate-pool objects at `>= 500 km`, expose configurable count/threshold controls in System Studio, and are selectable/viewer-eligible body records. The public minor-body population slope now affects representative major-body sizes, and comet generation records Bauer activity-fraction/source metadata. Actual station/habitat records, export panels, separate orbital-family distributions, survey-bias models, and calibrated population counts remain follow-up. |
| Sentient population technology, law, factions, and life-biome baseline | `HamiltonEtAl2020`, `BettencourtEtAl2007`, `ArvidssonEtAl2023`, `Knez2023`, `Stokey2020`, `Comin2013`, `CominMestieri2013`, `BallandEtAl2022`, with `Chowdhury2022` capacity context | Partly implemented as explicit sentient-world diagnostics separating elite/median technology access, invention capacity, adoption-lag pressure, technology-access inequality, domain-specific technology access records, legal reach, practical enforcement reach, neutral `0-24` core tech, compressed law level/interpretation, jurisdiction structure/pluralism/conflict, deterministic faction records, structured culture tags, institutional religion structure, and biology-gated life-biome readouts from older direct RPG-style outputs. These remain human-audit-required social-science proxies; no doctrine, magic, culture prose, or species claims are treated as normal-generation science outputs. Per-domain calibration, per-technology diffusion curves, calibrated inequality distributions, deeper jurisdictional hierarchy, population-facing TNO/comet settlement export surfaces, and human-reviewed state-capacity retuning remain follow-up. |

## Next priority queue

### 1. Sentient populations, technology, and governance

Primary sources: `Chowdhury2022`, `Comin2013`, `CominMestieri2013`, `Stokey2020`, `BettencourtEtAl2007`, `ArvidssonEtAl2023`, `BallandEtAl2022`, `HamiltonEtAl2020`, `Knez2023`.

Expected work:
- Keep cultural, governance, law, and technology outputs human-audit-required.
- Separate invention, diffusion, adoption lag, enforcement reach, state capacity, settlement structure, faction records, institutional religion structure, life-biome availability, and economic complexity before broader generator changes.
- Current v0.11 slices add elite/median technology access, invention capacity, adoption-lag pressure, technology-access inequality diagnostics, per-domain technology access records, practical enforcement reach, neutral `0-24` core technology, compressed law level, jurisdiction structure/pluralism/conflict, first-class factions, structured culture/religion signals, and life-biome gating.
- Remaining work should continue with deeper state-capacity refinement, per-technology diffusion, explicit jurisdictional hierarchy, and export/readout surfaces for population work that intersects small-body/TNO/comet settlement contexts. Do not treat asteroid-belt-only readouts as sufficient for those outer-system populations.

### 2. Remaining small-body refinement

Primary sources: `DeMeoCarry2014`, `BauerEtAl2017`, `KavelaarsEtAl2023`, `BernardinelliEtAl2022`, `NapierEtAl2023`, plus future verified comet/TNO updates.

Expected work:
- Expand the new `SmallBodyReservoir` records into actual station/habitat records, export panels, and later calibrated population diagnostics. System Viewer now has compact overview/selected-belt readouts, a dedicated reservoir-details section, settlement-readiness diagnostics, and focusable large-object subentries, but TNO/Centaur/comet-feeding context still needs generated station/habitat records, export surfaces, separate orbital-family distributions, and population-count caveats before it can support broader mapping.
- Add separate orbital-family distributions and calibrated population-count diagnostics using the reviewed survey completeness and luminosity/size distributions.
- Keep `Lamy2004` out of active support unless a human explicitly reacquires and re-approves it.

## Deferred stability follow-up

Primary sources: `HeEtAl2020`, `FangMargot2013`, `Obertas2017`, `Petit2018`, `Petit2020`, `Tamayo2020`, `Laskar2017`, `Rice2023`, `Outland2020`, `ObertasTamayo2023`.

Why deferred: orbital spacing now uses a source-marked mutual-Hill proxy process rather than the prior all-Jupiter scaffold, but it is still not a full stability-time, AMD, or SPOCK classifier implementation.

Remaining work:
- Replace or supplement proxy spacing with a selectable, source-marked AMD/SPOCK/stability-time policy only where reviewed sources support it.
- Add mutual-inclination sampling after Fang/Margot parameter verification.
- Add tests that distinguish the current heuristic from the reviewed future policy.

## Deferred Kopparapu2014 follow-up

Primary sources: `Kopparapu2013`, `Kopparapu2014`, with mass/radius inputs from `ChenKipping2017` and `Otegi2020`.

Why deferred: StarGen currently computes the circumstellar HZ as a system/star-level band before individual planet mass is known. Kopparapu2014 is specifically about planet-mass-dependent HZ limits, so fully using it requires a second, planet-level HZ/alignment pass after physical properties are generated.

Remaining work:
- Add planet-specific mass-corrected HZ diagnostics after `PlanetPhysicalGenerator` resolves mass and radius.
- Record `kopparapu2014_mass_corrected_hz_inner_au`, `kopparapu2014_mass_corrected_hz_outer_au`, and `kopparapu2014_mass_corrected_hz_alignment` in planet provenance.
- Decide whether mass-corrected HZ alignment should affect only diagnostics at first, or also surface temperature, hydrosphere, atmosphere, and life scoring.
- Add tests comparing low-mass, Earth-mass, and super-Earth rocky planets at the same orbit so the Kopparapu2014 correction is visible and bounded.
- Branch this work if it changes orbit selection, hydrosphere generation, or life scoring rather than remaining provenance-only.

## Deferred moon follow-up

Primary sources: `Ronnet2020`, `Sasaki2010`, `Szulagyi2018`, `JewittHaghighipour2007`, `BenistyEtAl2021`, `HellerBarnes2013`, `MalamudPerets2019`, `NakajimaEtAl2022`.

Current source use:
- `Ronnet2020`: active for regular CPD moon channel, satellite-scale total mass-ratio budget, and CPD/Hill provenance; not yet a pebble-accretion or ablation solver.
- `Sasaki2010`: active for Galilean resonant-chain and Saturnian dominant-moon architecture labels and mass-hierarchy shaping; not yet a gas-infall, cavity-evolution, or resonance-capture model.
- `Szulagyi2018`: active for allowing ice-giant regular icy CPD moon systems and CPD outer-fraction diagnostics; not yet a CPD thermodynamics or population-synthesis implementation.
- `JewittHaghighipour2007`: active for captured/irregular moon channel and retrograde-favored captured-orbit style; not yet a capture-family or irregular size-distribution model.
- `BenistyEtAl2021`: observational context only; use later as a CPD mass/size sanity check for young giant systems.
- `HellerBarnes2013`: habitability/tidal-heating context only for this slice; use later for a circumplanetary habitable-edge diagnostic.
- `MalamudPerets2019` and `NakajimaEtAl2022`: underutilized terrestrial-impact moon sources; use later for a branch that models late impacts, large-Moon mass ratios, stable outward tidal evolution, and composition/obliquity effects.

Remaining work:
- Replace StarGen-tuned regular-moon mass shares with source-extracted distributions after human verification.
- Add explicit resonance spacing/capture diagnostics for Galilean-like chains.
- Add captured-satellite family generation, including prograde/retrograde clusters and small-body size distribution.
- Add a terrestrial giant-impact moon process branch; branch the work if it changes rocky-planet moon frequency, habitability scoring, or planet formation history.

## Deferred galaxy follow-up

Primary sources: `BlandHawthornGerhard2016`, `Bovy2017`, `KhoperskovEtAl2024`, `HuntVasiliev2025`, `Hayden2014`, `Kennicutt1998`, `Chabrier2003`, `Kroupa2001`.

Current source use:
- `BlandHawthornGerhard2016`: active for Milky-Way analog schema fields, thin/thick disk dimensions, bar half-length, solar-circle radius, circular velocity, stellar-mass scale, diagnostic mass-component budgets, diagnostic rotation-curve decomposition, diagnostic dynamics/local-mass/comparison caveats, and structural source status. It is not yet a full Milky-Way dynamical model.
- `Bovy2017`: context for disk population structure and the warning that the disk is not truly two discrete components; current thin/thick split is a generator-facing schema proxy, not a full mono-abundance-population model.
- `KhoperskovEtAl2024` and `HuntVasiliev2025`: context for future bar dynamics and Gaia-era kinematics; not active defaults in this slice because pattern speed, corotation, disequilibrium, and halo kinematics need a separate dynamics branch.
- `Hayden2014`: context for future radial/vertical chemical structure; current metallicity gradient remains a separate simplified field.
- `Kennicutt1998`: active elsewhere for star-formation-efficiency context, not for Milky-Way structural geometry.
- `Chabrier2003` and `Kroupa2001`: active stellar IMF sources, not galaxy-structure schema sources.

Remaining work:
- Replace diagnostic nuclear stellar, baryon/gas/halo/local-density, rotation, pattern-speed, and corotation proxies with calibrated behavior only after source-specific values and downstream effects are reviewed.
- Add selectable Milky-Way analog vs broader barred-spiral/elliptical/lenticular/irregular behavior using non-Milky-Way comparison sources.
- The Galaxy Studio and Galaxy Viewer changes in `Docs/GalacticScienceBehaviorPlan.md` are now implemented at the current behavior-gated level. The default P2P/plan-contract tests protect the current diagnostic-only invariants, and the explicit `f2p` behavior harness now passes across thirteen UI/domain behavior surfaces.
- Branch the work if any dynamics field changes star placement, system priors, or population/route-generation behavior.

## Immediate recommendation

Continue with **sentient populations, technology, governance, and neutral settlement/export surfaces** after human review of the remaining source-note metadata. The new neutral baseline now supplies core technology, law, factions, culture tags, institutional religion, and life-biome availability, but it still needs deeper state-capacity/jurisdiction modeling, per-technology diffusion, faction issue taxonomies, and better non-belt expression for TNO, comet-feeding, and outer-reservoir settlement/population contexts. The small-body slice now has active source provenance, narrow generator retuning, diagnostic TNO subfamily metadata, first-class reservoir records with neutral settlement-readiness diagnostics, diameter-correct inspectable large objects selected as largest eligible candidates with configurable count/threshold controls, compact and dedicated System Viewer reservoir readouts, while actual generated belt-station/habitat records, separate generated TNO population records, export surfaces, and survey-bias modeling remain deferred small-body follow-up.
