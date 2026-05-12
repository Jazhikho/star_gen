# Source-to-Generator Fit Audit

Date: `2026-04-28`

Purpose: track how closely StarGen's current generators match the source notes being
normalized for v1.0 scientific pipeline closure. This audit is descriptive only: it
does not approve source claims, retune formulas, or replace human review.

Source-acquisition rule: if an already cited source has no local `Sources/Texts`
note, treat that as an acquisition gap rather than a bibliography mistake. Active
science controls must either point to a local note or carry explicit pending/blocked
metadata in `SourceAcquisitionRegistry`.

Implementation follow-up: the v1.0 science registry now covers the exposed galaxy,
stellar, planetary, life, and sentient-world science catalog surface. Registry entries
carry value/range metadata, source IDs, supported claim, consuming generator, public
control, implementation status, and human-audit status. Additional entries document
high-impact internal coefficient families that were already marked with `Tuning:`
comments, including galaxy bar/Sersic/spiral coefficients, stellar lifetime/IMF/
companion coefficients, planetary formation/class/envelope/volatile/slot/moon
coefficients, and life-support/dark-biosphere/oxygen-bottleneck coefficients.
`TestScienceTuningRegistry` now fails if cataloged science controls fall out of sync
with tooltips, help-panel source lists, materiality metadata, registry metadata, or
source-acquisition metadata.

Status meanings:

- `good structural fit`: current generator shape matches the source's qualitative model,
  though exact coefficients may still be StarGen tuning.
- `partial fit`: current generator has some relevant fields or behavior, but important
  source implications are missing or only implicit.
- `weak fit`: current generator does not yet represent the source's core mechanism.
- `planning only`: source is relevant to future human-audited systems but should not
  drive v1.0 runtime behavior without explicit approval.

## Normalized Source Notes In This Pass

These source notes were converted from raw full-text extracts into the same working
layout used by `Sources/Texts/BauerEtAl2017.txt`:

- `Sources/Texts/ChacuaEtAl2024.txt`
- `Sources/Texts/EscuderoEtAl2023.txt`
- `Sources/Texts/HamiltonEtAl2020.txt`
- `Sources/Texts/Knez2023.txt`
- `Sources/Texts/SavvidouEtAl2023.txt`
- `Sources/Texts/VanKleefEtAl2023.txt`

`SavvidouEtAl2023.pdf` is currently not the article PDF; it is an A&A journal leaflet.
The note was normalized from the local HTML extraction and is flagged for PDF reacquisition.

## Initial Generator Fit Findings

| Source | Generator surface inspected | Current fit | Findings | v1.0 action |
|---|---|---|---|---|
| `BauerEtAl2017` | `CometGenerator`, `CometSpec`, `SystemAsteroidGenerator` | `partial fit` | `CometGenerator` now defaults to a Bauer-style Jupiter-family radius prior, a smaller nearly-isotropic comet proxy, low albedo, explicit activity state, active-fraction metadata, source metadata, and an explicit legacy wide-range mode. `SystemAsteroidGenerator` uses Bauer as part of the cold reservoir/comet-supply provenance. Missing details: Damocloid/HTC distinction, calibrated active-fraction distribution, and population-count normalization. | Keep Bauer active and `Lamy2004` replaced. Add Damocloid/HTC and population normalization only after human verification. |
| `DeMeoCarry2014`, `KavelaarsEtAl2023`, `BernardinelliEtAl2022` | `AsteroidGenerator`, `AsteroidBelt`, `SmallBodyReservoir`, `SystemAsteroidGenerator`, `PlanetarySystemState`, `SystemInspectorPanel` | `partial fit` | Asteroids and belts now carry DeMeo/Carry composition provenance; cold belts are labeled as trans-Neptunian reservoirs, placed farther outward, use a lower TNO-scale mass ceiling, and carry Kavelaars/Bernardinelli reservoir, size-distribution, and diagnostic subfamily provenance. Generated systems now persist small-body reservoir records for asteroid, TNO, Centaur, and comet-feeding families instead of only storing that context on asteroid belts, mark native life absent on those reservoir surfaces, and surface neutral station/habitat readiness in the overview, selected-belt readout, and dedicated reservoir-details section. The public minor-body population slope now affects representative major-body sizes, and inspectable large belt objects now use diameter-based semantics with composition-derived mass, selected as the largest eligible candidate-pool bodies using configurable count and minimum-diameter controls. Missing details: actual station/habitat records, separate orbital-family distributions, survey-bias-aware counts, export surfaces, and verified radial taxonomic fractions. | Keep as a source-marked deterministic proxy. Expand neutral station/habitat and export surfaces around `SmallBodyReservoir` before adding calibrated counts or family-specific orbital rendering. Branch again if future work changes planet formation, volatile delivery, or belt-family architecture beyond this narrow retune. |
| `SavvidouEtAl2023` | `PlanetarySystemState`, `SystemPlanetGenerator` | `good structural fit` | Current gas-giant generation already depends on solid budget, gas budget, disk lifetime, metallicity, migration strength, snow-line weighting, and local orbit. This matches the source's main claim that no single parameter makes giants. Missing details: disk radius, embryo injection time, dust-to-gas ratio, fragmentation velocity, and a 5-25 AU origin preference from the paper's grid. | Keep current multi-parameter structure for v1.0, but mark coefficients as tuning. Reacquire the correct PDF before using this source for final citation closure. |
| `EscuderoEtAl2023` | `CryosphereProps`, `BiomeType`, `BiologySupportEvaluator`, `ProfileGenerator` | `partial fit` | StarGen already represents subsurface oceans, a subsurface biome, tidal-heating support, and microbial-life support without requiring atmosphere in the subsurface branch. Missing source implications: rock-fluid interfaces, permeability, radiolytic H2, serpentinization, reduced sulfur/iron energy, and a separate "dark biosphere only" life-stage output. | Keep current subsurface branch as a deterministic proxy; add explicit documentation that it is not a calibrated dark-biosphere model. |
| `HamiltonEtAl2020` | `SentientWorldProfileBuilder`, `NativePopulationGenerator` | `good structural fit` | `SentientWorldProfileBuilder` uses normalized population and group count to drive `SocialScale`, but also mixes habitability, trade, surplus, state capacity, and technology. This matches the paper's warning that population is important but not sufficient. Missing details: jurisdictional hierarchy levels and geographic range/density scaling. | Use as a support source for `SocialScale`; do not claim full sociopolitical hierarchy modeling until hierarchy fields exist. |
| `Knez2023` | `SentientWorldProfile`, `SentientWorldProfileBuilder`, `NativePopulationGenerator` | `partial fit` | StarGen separates `HighestTechLevel` from `TechnologyAdoptionCapacity`, and adoption capacity depends on trade, state capacity, cultural accumulation, surplus, and average tech. Missing source implications: per-technology adoption density, local implementation-cost space, wage/development proxy, and time-series diffusion curves. | Keep the access-vs-adoption split. Defer calibrated technology diffusion unless a future sentient-world effort adds per-technology density. |
| `ChacuaEtAl2024` | `SentientWorldProfileBuilder`, `PopulationGenerator`, civilization concept code | `partial fit` | Current sentient-world fields can express trade connectivity, surplus, cultural accumulation, and technology adoption capacity. They do not yet represent capability portfolios, relatedness, binding constraints, or local production readiness. | Treat as a planning source for economic-complexity capability modeling. Avoid using it as proof that current tech/trade outputs are fully grounded. |
| `VanKleefEtAl2023` | `Government`, `SentientWorldProfile`, `SentientWorldProfileBuilder` | `weak fit` | StarGen has government legitimacy, coercion, legal reach, restriction pressure, and factional fragmentation, but does not separate dominance, prestige, local norms, global norms, internal legitimacy, or external legitimacy. | Planning only for v1.0 unless a human-audited legitimacy/faction layer is explicitly added. |

## Cross-Cutting Observations

- The physical generators are generally closer to the source corpus than the
  civilization generators because the current physical models already expose aggregate
  formation, atmosphere, HZ, and biology controls.
- The sentient-world layer has useful latent fields, but the new social-science sources
  mostly support future decomposition, not direct release claims. Human audit remains a
  release gate for these outputs.
- Several current implementations are structurally plausible but should keep "Tuning:"
  language because the exact coefficients are not sourced fit values.
- Comet generation is the clearest early mismatch found in this pass: the code has the
  right conceptual entities, but its default nucleus-radius ranges do not match the
  BauerEtAl2017 source note.

## Implementation Follow-Up 2026-04-28

- Added `ScienceTuningRegistry` as the first code-facing source/tuning map for active
  science heuristics and public controls.
- Retuned the default Jupiter-family comet nucleus path to a Bauer-style smaller-radius
  prior; the older wide radius range is retained only through `LegacyWideRange`.
- Added additive planetary profile controls for comet nucleus/activity model, comet size
  scale, minor-body slope, disk radius, dust-to-gas scale, fragmentation model, and
  giant-origin band.
- Added additive use-case settings for subsurface dark-biosphere scoring and
  sentient-world social-scale, technology-diffusion, economic-complexity, and legitimacy
  proxy models.
- Updated tooltip/help catalogs and added registry/citation tests. Social-science proxy
  outputs remain human-audit-required release claims, not final accepted cultural or
  governance models.

## Implementation Follow-Up 2026-05-12

- Added `GalaxyMassComponentBudget` as a diagnostic-only galaxy schema surface for halo,
  dark matter, baryonic, stellar disk/spheroid/halo, nuclear stellar, cold gas, hot gas,
  baryon/gas fraction, and local stellar-density fields.
- Added `GalaxyRotationCurveDiagnostic` as a diagnostic-only velocity-decomposition
  surface for inner/reference/outer velocity anchors and disk/spheroid/gas/dark-matter
  reference contributions.
- Added `GalaxyDynamicsDiagnostic` as the final non-behavioral hardening surface for
  bar pattern speed, corotation, local mass-density and surface-density proxies,
  analog calibration mode, and non-Milky-Way comparison caveats.
- The component budget, rotation diagnostic, and dynamics diagnostic serialize through
  `GalaxyRealismProfile` and `GalaxySpec`, but they do not yet drive star placement,
  density evaluation, jump routes, population pressure, gravitational potential, or
  orbit integration.
- Exact component ratios and velocity-contribution weights remain StarGen tuning pending
  human verification of the Milky Way mass-budget and Gaia-era dynamics sources. The
  required behavior work is tracked in `Docs/GalacticScienceBehaviorPlan.md`.

## Next Audit Targets

- `SystemMoonGenerator` against `BenistyEtAl2021`, `Ronnet2020`, `Sasaki2010`, and
  `Szulagyi2018`.
- `SystemPlanetGenerator` radius-gap and atmosphere branches against `Fulton2017`,
  `OwenWu2017`, `Ginzburg2018`, `Berger2020Gaia`, `GillisEtAl2026`, and
  `WordsworthKreidberg2022`.
- `GalaxyRealismProfileBuilder`, `GalaxyMassComponentBudget`, and
  `GalaxyScientificFieldEvaluator` against the galaxy morphology, component-budget,
  environment, metallicity, and GHZ source cluster.
