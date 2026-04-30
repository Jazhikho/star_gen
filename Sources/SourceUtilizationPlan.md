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

## Next priority queue

### 1. Habitable zones, atmospheres, and life bottlenecks

Primary sources: `Kasting1993`, `Kopparapu2013`, `Kopparapu2014`, `Balbi2023`, `WordsworthKreidberg2022`, `ChatterjeeEtAl2026`, `BiassoniEtAl2023`, `VissapragadaEtAl2022`, `LugerBarnes2015`.

Expected work:
- Keep HZ models and atmosphere-loss/life-support fields aligned with reviewed assumptions.
- Add missing atmosphere-regime provenance before O2/N2/CO2/H2O/H/He/tenuous/silicate outcomes.

### 2. Moon formation and moon-system architecture

Primary sources: `Ronnet2020`, `Sasaki2010`, `Szulagyi2018`, `HellerBarnes2013`, `BenistyEtAl2021`, `MalamudPerets2019`, `NakajimaEtAl2022`.

Expected work:
- Replace generic active moon-formation support with the reviewed Ronnet/Sasaki/Szulagyi cluster.
- Separate regular circumplanetary-disk moons, captured moons, and impact/accretion limits in tests and provenance.

### 3. Galaxy schema and Milky-Way analog structure

Primary sources: `BlandHawthornGerhard2016`, `Bovy2017`, `Chabrier2003`, `ChabrierLenoble2023`, `KhoperskovEtAl2024`, `HuntVasiliev2025`, `Hayden2014`, `Kroupa2001`, `Kennicutt1998`.

Expected work:
- Keep `BlandHawthornGerhard2016` marked reviewed but underutilized until schema fields exist for the broader Milky-Way parameter set.
- Decide which long-bar, nuclear, thin/thick disk, baryon, halo, rotation-curve, and local mass-budget fields become first-class.

### 4. Small bodies and reservoirs

Primary sources: `DeMeoCarry2014`, `Lamy2004`, `KavelaarsEtAl2023`, `NapierEtAl2023`, `BernardinelliEtAl2022`, `JewittHaghighipour2022`.

Expected work:
- Tighten asteroid/comet reservoir scaling, belt placement, and volatile-delivery assumptions.
- Ensure object-family generation and system-level reservoirs cite the same accepted notes.

### 5. Sentient populations, technology, and governance

Primary sources: `Chowdhury2022`, `Comin2013`, `CominMestieri2013`, `Stokey2020`, `BettencourtEtAl2007`, `ArvidssonEtAl2023`, `BallandEtAl2022`, `HamiltonEtAl2020`, `Knez2023`.

Expected work:
- Keep cultural, governance, law, and technology outputs human-audit-required.
- Separate invention, diffusion, adoption lag, enforcement reach, state capacity, settlement structure, and economic complexity before generator changes.

## Deferred stability follow-up

Primary sources: `HeEtAl2020`, `FangMargot2013`, `Obertas2017`, `Petit2018`, `Petit2020`, `Tamayo2020`, `Laskar2017`, `Rice2023`, `Outland2020`, `ObertasTamayo2023`.

Why deferred: orbital spacing now uses a source-marked mutual-Hill proxy process rather than the prior all-Jupiter scaffold, but it is still not a full stability-time, AMD, or SPOCK classifier implementation.

Remaining work:
- Replace or supplement proxy spacing with a selectable, source-marked AMD/SPOCK/stability-time policy only where reviewed sources support it.
- Add mutual-inclination sampling after Fang/Margot parameter verification.
- Add tests that distinguish the current heuristic from the reviewed future policy.

## Immediate recommendation

Continue with **Habitable zones, atmospheres, and life bottlenecks**. Formation-budget provenance is now active enough for the current pass, while Balbi/Wordsworth/Chatterjee/Vissapragada/Luger-Barnes still need atmosphere-regime and life-bottleneck provenance before the generated O2/N2/CO2/H2O/H/He/tenuous/silicate outcomes are as traceable as the planet-formation layer.
