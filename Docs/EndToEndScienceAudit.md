# End-to-End Scientific Grounding Audit

Date: `2026-04-23`

This tracked document supersedes the earlier worktree-only draft under `.claude/worktrees/.../Docs/EndToEndScienceAudit.md` and is now the repository source of truth for the end-to-end science-grounding pass.

Purpose: trace generator-internal numeric claims that materially shape galaxy, stellar, planetary, and life outputs; distinguish literature-backed framework from StarGen tuning; and record which first-pass gaps are still blocked pending human-reviewed source verification.

This audit complements [ScientificParameterAudit.md](ScientificParameterAudit.md), which covers the exposed science-parameter surface. This file goes one layer deeper into internal constants and formulas.

Important: every new source note, bibliography addition, and inline citation added in this first pass is an AI-assisted draft and remains pending human verification. Nothing here should be treated as authoritative until a human reviewer confirms the underlying source text, per [AI-Use-Statement.md](../AI-Use-Statement.md) and [Sources/SourceReviewProcedure.md](../Sources/SourceReviewProcedure.md).

---

## Status meanings

- `blocked on source acquisition`: no reviewed source text has been added yet, so the claim stays open.
- `source reviewed`: a source note has been added to `Sources/Texts`, but the relevant code site is not yet inline-cited.
- `inline-cited (human verification pending)`: the code site now carries an APA-style framework comment plus an explicit tuning note, but a human still needs to verify the source note and bibliography entry.

---

## First-pass scope

This first remediation pass is intentionally limited to the audit's explicit `needs source` items that were called out for immediate closure work:

- Galaxy morphology claims in `GalaxyRealismProfileBuilder`
- Stellar lifetime exponents in `StellarIsochroneApproximator`
- Migration-framework claim in `PlanetarySystemState`
- Sulfur-chemistry claim in `BiologySupportEvaluator`

Broader `citeable` cleanup remains future work.

---

## First-pass gap status

| Gap | File | Current status | First-pass disposition |
|---|---|---|---|
| Bar strength coefficient | `src/domain/galaxy/GalaxyRealismProfileBuilder.cs` | `inline-cited (human verification pending)` | Framework comment now points to Diaz-Garcia et al. (2016); exact `0.35 + 0.60 * BulgeToTotal` scaling is explicitly labeled StarGen tuning. |
| Intermediate elliptical Sersic band | `src/domain/galaxy/GalaxyRealismProfileBuilder.cs` | `inline-cited (human verification pending)` | Framework comment now points to Kormendy et al. (2009); exact `3.2 + rand * 1.2` range is explicitly labeled StarGen tuning. |
| Giant elliptical Sersic band | `src/domain/galaxy/GalaxyRealismProfileBuilder.cs` | `inline-cited (human verification pending)` | Framework comment now points to Kormendy et al. (2009); exact `4.0 + rand * 1.5` range is explicitly labeled StarGen tuning. |
| Stellar lifetime exponents | `src/domain/generation/generators/StellarIsochroneApproximator.cs` | `inline-cited (human verification pending)` | Framework comment now points to Hurley et al. (2000); the piecewise `2.1 / 2.5 / 2.9` exponents are explicitly labeled StarGen tuning. |
| Type-I migration framework surrogate | `src/domain/generation/PlanetarySystemState.cs` | `inline-cited (human verification pending)` | Framework comment now points to Tanaka, Takeuchi, and Ward (2002); the composite migration weights remain explicit StarGen tuning. |
| Sulfur-chemistry support window | `src/domain/population/BiologySupportEvaluator.cs` | `inline-cited (human verification pending)` | Inline comment documents a speculative sulfur branch without a Springer book citation; the sulfur temperature/pressure thresholds and multiplier remain explicit StarGen tuning pending a replaceable peer-reviewed primary source. |
| Exotic chemistry anchor system | `src/domain/population/BiologySupportEvaluator.cs` | `blocked on source acquisition` | No reviewed source note was added in this pass, so the broader exotic-anchor system remains open. |

---

## First-pass artifacts

- Audit queue updates: [Sources/ToReview.md](../Sources/ToReview.md)
- Draft source notes: `Sources/Texts/DiazGarcia2016.txt`, `Hurley2000.txt`, `Kormendy2009.txt`, and `TanakaTakeuchiWard2002.txt`
- Bibliography tracking: [Sources/AnnotatedBibliography.md](../Sources/AnnotatedBibliography.md)
- Inline-code traceability:
  - [GalaxyRealismProfileBuilder.cs](../src/domain/galaxy/GalaxyRealismProfileBuilder.cs)
  - [StellarIsochroneApproximator.cs](../src/domain/generation/generators/StellarIsochroneApproximator.cs)
  - [PlanetarySystemState.cs](../src/domain/generation/PlanetarySystemState.cs)
  - [BiologySupportEvaluator.cs](../src/domain/population/BiologySupportEvaluator.cs)
- Regression guard: [TestEndToEndScienceAudit.cs](../Tests/Quality/TestEndToEndScienceAudit.cs)

---

## Notes for human review

- The new source-note files are deliberately conservative: they support the surrounding scientific framework, not every coefficient.
- Where the reviewed literature supports a qualitative or family-level claim but not the exact numeric weight, the code comments now say so directly with `Tuning:` language.
- If human review determines that any cited framework is too weak or the comment is still overstated, narrow the code comment and this audit wording rather than silently leaving a stronger claim in place.

---

## Second-pass inline citation sweep

The broader source-backed inline-citation sweep now covers additional reviewed-source comments in:

- `src/domain/galaxy/SpiralDensityModel.cs`
- `src/domain/system/StellarConfigGenerator.cs`
- `src/domain/generation/generators/StellarMassSampler.cs`
- `src/domain/generation/PlanetarySystemState.cs`
- `src/domain/system/SystemPlanetGenerator.cs`
- `src/domain/population/BiologySupportEvaluator.cs`

This pass used only sources that already existed in `Sources/Texts` at the start of the sweep or had already been promoted in the first pass. The 2026-04-24 Balbi feedback follow-up changed only the civilization oxygen-support scalar so abiotic oxygen false-positive risk now discounts otherwise breathable atmosphere support.

## Source-acquisition batch for one-pass human review

To avoid forcing two separate human review rounds, the unsupported or not-yet-localized anchors needed for future closure work were gathered into one source batch. These draft notes are now queued together in [Sources/ToReview.md](../Sources/ToReview.md):

- `Sources/Texts/Bains2004.txt`
- `Sources/Texts/Behroozi2019.txt`
- `Sources/Texts/BlandHawthornGerhard2016.txt` (promoted by 2026-04-24 human feedback)
- `Sources/Texts/Chabrier2003.txt`
- `Sources/Texts/Choi2016.txt`
- `Sources/Texts/Conselice2014.txt`
- `Sources/Texts/DucheneKraus2013.txt`
- `Sources/Texts/Hayden2014.txt`
- `Sources/Texts/Kennicutt1998.txt`
- `Sources/Texts/Kroupa2001.txt`
- `Sources/Texts/Raghavan2010.txt`
- `Sources/Texts/WeggGerhard2013.txt`

Except for the Bland-Hawthorn and Gerhard (2016) note promoted by 2026-04-24 human feedback, these are still draft source notes. Human verification is required before any remaining draft notes are treated as reviewed or used to close audit gaps.

## Workstream B demographics intake (2026-04-26)

Draft source notes and local arXiv PDFs were added for TESS / Gaia-era exoplanet demographics and architecture correlation:

- `Sources/Texts/CuiEtAl2026.txt` (+ `CuiEtAl2026.pdf`)
- `Sources/Texts/MentCharbonneau2023.txt` (+ `MentCharbonneau2023.pdf`)
- `Sources/Texts/WanderleyEtAl2025.txt` (+ `WanderleyEtAl2025.pdf`)
- `Sources/Texts/GillisEtAl2026.txt` (+ `GillisEtAl2026.pdf`)
- `Sources/Texts/VanZandtEtAl2025.txt` (+ `VanZandtEtAl2025.pdf`)

The first generation pass now uses only conservative, source-marked demographic scalars from this batch: close-in FGK hot-giant suppression, mid-to-late M close-in small-planet pressure, M-dwarf sub-Neptune/hot-giant suppression, and no automatic M-dwarf HZ rocky surplus. Wanderley and Van Zandt remain follow-up because they require a dedicated M-dwarf radius-gap mode and a conditional outer-giant architecture pass. See also [WorkstreamB_ExoplanetDemographicsSources.md](../Sources/WorkstreamB_ExoplanetDemographicsSources.md) if present in the local source archive.

## Workstreams C through K source intake (2026-04-26)

Draft source notes and local PDFs (arXiv or open-access MDPI) were added for the gap-closure plan workstreams **C** through **K** in [SourceRecencyGapClosurePlan.md](../Sources/SourceRecencyGapClosurePlan.md):

- **C (HZ rocky yield):** `Sources/Texts/KunimotoEtAl2022.txt` (+ `KunimotoEtAl2022.pdf`)
- **D (atmosphere loss / JWST-era framing):** `Sources/Texts/ChatterjeeEtAl2026.txt` (+ `ChatterjeeEtAl2026.pdf`)
- **E (moons):** `Sources/Texts/NakajimaEtAl2022.txt` (+ `NakajimaEtAl2022.pdf`)
- **F (small bodies):** `Sources/Texts/KavelaarsEtAl2023.txt` (+ `KavelaarsEtAl2023.pdf`)
- **G (galaxy morphology):** `Sources/Texts/KhoperskovEtAl2024.txt`, `Sources/Texts/HuntVasiliev2025.txt` (+ matching PDFs)
- **H (orbital stability):** `Sources/Texts/HeEtAl2020.txt`, `Sources/Texts/ObertasTamayo2023.txt` (+ matching PDFs)
- **I (stellar IMF maintenance):** `Sources/Texts/ChabrierLenoble2023.txt` (+ `ChabrierLenoble2023.pdf`)
- **J (planet-conditioned populations — design only):** `Sources/Texts/KarakatsanisMamassis2023.txt` (+ `KarakatsanisMamassis2023.pdf`) — Earth-history energy and carrying-capacity framing; not a direct exoplanet calibration; human audit required before any production population scoring.
- **K (astrobiology / alternative biochemistry):** `Sources/Texts/BainsEtAl2024.txt` (+ `BainsEtAl2024.pdf`); `AnnotatedBibliography.md` now also indexes existing `Sources/Texts/Bains2004.txt` under a dedicated subsection.

### Workstreams C–K second anchor batch (2026-04-26)

Additional **AI-assisted draft** notes and arXiv PDFs bring each letter toward **multiple anchors** and name **competing model families** inside each `Texts/*.txt` *Models vs refinements* block (for example C1/C2/C4, D1/D2/D4, E1–E3, F1–F3, G1–G3, H1–H3, I1–I3, J1/J2, K1–K3):

- **C:** `BergstenEtAl2023.txt`, `LuquePalle2022.txt` (alongside existing `Bryson2021.txt`, `KunimotoEtAl2022.txt`)
- **D:** `VissapragadaEtAl2022.txt`, `BiassoniEtAl2023.txt` (alongside `ChatterjeeEtAl2026.txt` and existing Owen/Ginzburg notes)
- **E:** `BenistyEtAl2021.txt`, `MalamudPerets2019.txt` (alongside `NakajimaEtAl2022.txt` and existing Ronnet/Sasaki/Szulagyi notes)
- **F:** `NapierEtAl2023.txt`, `BernardinelliEtAl2022.txt` (alongside `KavelaarsEtAl2023.txt`)
- **G:** `GarmaOehmichenEtAl2022.txt` (alongside `KhoperskovEtAl2024.txt`, `HuntVasiliev2025.txt`)
- **H:** `FangMargot2013.txt` (alongside `HeEtAl2020.txt`, `ObertasTamayo2023.txt`, AMD draft batch)
- **I:** `StevensonEtAl2023.txt` (alongside `ChabrierLenoble2023.txt` and existing `Li2023.txt`, `Kirkpatrick2024.txt`)
- **J:** `HamiltonEtAl2016.txt` (alongside `KarakatsanisMamassis2023.txt`)
- **K:** `PetkowskiEtAl2020.txt` (alongside `Bains2004.txt`, `BainsEtAl2024.txt`, `KrissansenTotton2018.txt`)

## Human feedback disposition 2026-04-24

| Source | Current implementation match | Remaining gap |
|---|---|---|
| Balbi and Frank (2023), with Wordsworth/Kreidberg, Chatterjee/Pierrehumbert, Luger/Barnes, Kopparapu, Vissapragada, and Biassoni context | `BiologySupportEvaluator` already carries early-desiccation risk, prebiotic-UV adequacy, oxygenation chance, biosignature detectability, abiotic-O2 false-positive risk, and civilization oxygen-bottleneck scoring. `PlanetAtmosphereGenerator` now records source-marked atmosphere-retention probability, secondary-atmosphere escape pressure, retention scalar, pre-main-sequence XUV risk, atmosphere regime, composition family, and oxygen context before generated O2/N2/CO2/H2O/H/He outcomes are snapshotted. | Full hydrodynamic escape, volcanic revival, oxygen-evolution timelines, and planet-mass-dependent HZ correction remain follow-up after human verification. |
| Bland-Hawthorn and Gerhard (2016), local shorthand `Bland2016` / `BlandHawthornGerhard2016` | Partly implemented as the active Milky-Way structural schema anchor. `GalaxyConfig`, `GalaxySpec`, and `GalaxyRealismProfile` now carry thin/thick disk scale lengths and heights, bar half-length, solar-circle radius, circular velocity at the solar circle, and stellar-mass scale; spiral density and bar-region classification use the new disk/bar geometry. | Still not a full Milky-Way simulator: pattern speed/corotation, nuclear cluster/disk, baryon/gas components, rotation-curve decomposition, stellar/dark/hot-gas halo, and local mass-budget calibration remain follow-up. |

## Source grounding cleanup 2026-04-24

This pass tightened source status without broadly retuning generators. Local PDFs remain ignored acquisition artifacts; committed artifacts are text source notes, bibliography/review updates, provenance, and source-quality tests.

| Domain | Status | Follow-up |
|---|---|---|
| Galaxy schema | Bland-Hawthorn and Gerhard (2016) is partly implemented for first-class Milky-Way structural schema fields: thin/thick disk dimensions, bar half-length, solar-circle radius, circular velocity, and stellar mass. | Add nuclear cluster/disk, baryon/gas components, halo decomposition, rotation-curve terms, pattern speed/corotation, local mass-budget calibration, and non-Milky-Way comparison sources before claiming full galaxy-realism closure. |
| Mass-radius | Chen and Kipping (2017) is partly implemented with seeded radius scatter and Terran/Neptunian/Jovian probabilities. Otegi et al. (2020) is partly implemented with the rocky branch capped at the source's approximate 25 Earth-mass endpoint and seeded coefficient/exponent uncertainty sampling. | Replace the conservative Chen-Kipping scatter envelope with full Forecaster posterior sampling; decide whether Otegi's pure-water density separator becomes an explicit diagnostic or branch-selection rule after human review. |
| Atmosphere/envelope loss | Fulton (2017), Owen and Wu (2017), and Ginzburg et al. (2018) are partly implemented. Close-in planets now record radius-valley period, center, loss pressure, mechanism, and Fulton-gap anchor in provenance; photoevaporation and core-powered branches now use opposite period slopes and different loss-pressure drivers. | Replace StarGen-tuned scalar pressure proxies with reviewed escape/population models, including initial H/He envelope fraction, core mass/composition, stellar high-energy history, age evolution, and updated CKS/TESS/JWST-era constraints after human review. |
| Planet occurrence/demographics | Petigura (2013), Bryson (2021), Bergsten et al. (2023), Ment and Charbonneau (2023), Cui et al. (2026), and Gillis et al. (2026) are partly implemented as host demographic scalars in `PlanetarySystemState` and planet formation provenance. The active model distinguishes FGK, early M, mid-to-late M, and outside-reviewed-range hosts; it avoids an M-dwarf HZ rocky surplus while allowing close-in M-dwarf small-planet boosts and sub-Neptune/hot-giant suppression. | Replace scalar hooks with reviewed period-radius occurrence tables; reconcile Kunimoto metadata, Wanderley/Gillis M-dwarf radius-gap claims, and Van Zandt conditional outer-giant architecture before adding those larger process changes. |
| Disk / formation / migration / volatile delivery | Pascucci (2016), Ribas (2015), Izidoro (2017), Fernandes (2019), Raymond and Izidoro (2017), Mordasini (2007), Lambrechts and Johansen (2012), and Tanaka, Takeuchi, and Ward (2002) are partly implemented as active formation-state proxies. Generated planets now record formation source IDs, disk-dust host-mass exponent, adjusted disk lifetime, snow-line giant weight, giant-scattering scalar, volatile-delivery scalar, and Type-I migration timescale/likelihood diagnostics. | Replace deterministic scalars with verified disk-population fits, resonant-chain generation/breakup, a non-isothermal migration branch, and asteroid-belt depletion/composition coupling after human review. |
| Habitable zones / atmospheres / life bottlenecks | Kasting (1993) and Kopparapu (2013) support the active HZ options; Kopparapu (2014) is reviewed but underutilized. Wordsworth and Kreidberg (2022), Chatterjee and Pierrehumbert (2026), Luger and Barnes (2015), and Balbi and Frank (2023) are partly implemented through atmosphere/life-bottleneck provenance and scoring. Vissapragada (2022) and Biassoni et al. (2023) are observational context only, not active default models. | Replace heuristic retention scalars with reviewed escape/outgassing models, add volcanic revival, add planet-mass HZ alignment after mass generation, and keep abiotic-O2 warnings distinct from biological oxygenation. |
| Moon formation | Canup and Ward (2006) was dropped from the annotated bibliography after PDF access was withdrawn. Ronnet (2020), Sasaki (2010), Szulagyi (2018), and Jewitt/Haghighipour (2007) are now partly implemented in `SystemMoonGenerator`: regular CPD moons use a satellite-scale mass budget and source-marked Galilean/Saturnian/ice-giant architecture provenance, while captured moons are separated as irregular outcomes with retrograde-favored orbit style. Benisty (2021) is observational CPD context, Heller/Barnes (2013) is habitability/tidal-heating context, and Malamud/Perets (2019) plus Nakajima et al. (2022) are underutilized terrestrial-impact follow-ups. | Replace StarGen-tuned mass shares with verified source distributions, add explicit resonance/capture-family modeling, and add a terrestrial giant-impact moon branch after human verification. |
| Orbital stability | Partly implemented as a source-marked architecture spacing scaffold rather than a full stability model. Generated slots and planets now record the active `stargen_architecture_mass_proxy_mutual_hill_v2` policy, source IDs, period ratio from the inner slot, mutual-Hill spacing from the inner slot, the threshold used, and the compact/transition/giant architecture mass proxy used before planet assignment. Obertas (2017), Rice (2023), and He et al. (2020) support the broad compact-architecture/mutual-Hill/period-ratio framing; Petit/Laskar/Tamayo AMD/SPOCK sources remain underutilized. | Replace the proxy spacing policy with a reviewed stability-time, AMD, or classifier model after human verification. Decide how to handle resonance proximity, eccentricity/inclination, actual planet-mass uncertainty, multiplicity, and target survival timescale. |
| Technology/governance | Chowdhury, Comin/Mestieri, and Stokey notes are documentation-only anchors for future law/technology models. | Separate legal restriction, enforcement reach, state capacity, invention, diffusion, adoption lag, and adoption intensity in a future model. |

## Next closure targets

- Human verification of the full batched source-note set so the first-pass and second-pass audit additions can be reviewed in one sitting
- Promotion of verified batch sources into precise bibliography coverage where needed
- Follow-up closure of the still-open exotic-chemistry anchor system after the batched source review determines whether the current branch should stay speculative-framework-only or be narrowed further
- Galaxy-realism follow-up for Bland-Hawthorn and Gerhard (2016): the first structural schema fields are active, but dynamics, mass decomposition, local mass budget, and broader barred-spiral comparison sources remain open before full generator-fit closure
