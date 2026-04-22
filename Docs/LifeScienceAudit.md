# Life Science Audit

Date: `2026-04-22`

Purpose: audit StarGen's current life-generation model against primary astrobiology and origin-of-life literature, identify where the current implementation is still heuristic, and define a concrete tightening path that stays compatible with StarGen's deterministic generation architecture.

## Scope

This audit is narrower than a general "is there life" review. It focuses on whether StarGen's current life model is scientifically grounded in the areas that most directly affect world generation:

1. abiogenesis opportunity,
2. long-term biosphere persistence,
3. complex-life and oxygenation constraints,
4. host-star context and false positives,
5. nutrient access and ocean or land structure,
6. distinction between life existing and life being remotely detectable.

## Current model strengths

- `LifePotentialModeling` already separates abiogenesis, complex life, sentience, and civilization into distinct stages instead of collapsing everything into one "life potential" roll.
- `BiologySupportEvaluator` already uses a `PlanetEnvironmentProfile`, which means the life model can be tightened without rewriting the upstream planet generator.
- Surface water, tidal heating, radiation, XUV, land coverage, ocean coverage, and breathable atmosphere are already present in the environment inputs, so several missing scientific channels can be added by reweighting and extending existing environmental data rather than inventing a new simulation stack.
- The current source surface already reflects real literature for cautious versus optimistic abiogenesis framing, environmental-window framing, rare-complex-life framing, and oxygen bottlenecks for technospheres.

## Current model weaknesses

### 1. Too much of the model is still generic multiplier logic

The main logic in [LifePotentialModeling.cs](/D:/Game%20Creation/star_gen/src/domain/population/LifePotentialModeling.cs:100) resolves most scientific settings into scalar multipliers. The stage separation is good, but the environmental interpretation remains thin:

- `CalculateStabilityWindow(...)` is a compact weighted sum of habitable-zone alignment, water or land presence, radiation, XUV, weather, volcanism, and breathable atmosphere.
- `ResolveAbiogenesisMultiplier(...)`, `ResolveComplexLifeMultiplier(...)`, `ResolveSentienceMultiplier(...)`, and `ResolveCivilizationMultiplier(...)` then mostly scale chances from that single composite score.

This means distinct scientific ideas are being represented, but not yet through distinct environmental mechanisms.

### 2. Host-star history is under-modeled

The current biology path uses present-ish radiation and XUV, but it does not explicitly model the historical penalties that matter most for low-mass hosts.

This matters because:

- Luger and Barnes (2015) show that habitable-zone rocky planets around many M dwarfs can undergo prolonged early runaway greenhouse phases, lose large water inventories, and build up substantial abiotic oxygen.
- Lingam and Loeb (2018) argue that stellar lifetime alone is not enough; atmospheric erosion and biologically useful UV environment materially affect relative habitability across host-star classes.

Today StarGen can penalize high current XUV, but it does not encode a separate "M-dwarf pre-main-sequence desiccation / abiotic oxygen history" channel.

### 3. Abiogenesis opportunity is treated too much like habitability

The current abiogenesis path in [BiologySupportEvaluator.cs](/D:/Game%20Creation/star_gen/src/domain/population/BiologySupportEvaluator.cs:403) scales from chemistry score, permissiveness, environmental window, and a few broad environmental bonuses. That captures "can life survive here" better than it captures "could prebiotic chemistry plausibly start here."

Rimmer et al. (2018) is especially relevant here: it argues that at least one experimentally motivated prebiotic pathway depends on a minimum UV environment and introduces an "abiogenesis zone" concept that is not identical to the classical liquid-water habitable zone.

StarGen currently has no explicit prebiotic-opportunity channel for:

- host-star UV adequacy,
- transient reducing or impact-enabled feedstock opportunity,
- wet-dry or shoreline opportunity,
- redox disequilibrium as a separate abiogenesis support variable.

### 4. Nutrient supply and ocean structure are mostly absent

The current model gives real weight to ocean coverage, land coverage, and water-land mix, but it does not yet distinguish:

- nutrient-poor global oceans from nutrient-cycling oceans,
- ocean worlds with weak surface nutrient turnover from mixed ocean-land worlds,
- aerial or cloud biosphere chemistry from surface biosphere chemistry.

That gap matters because:

- Olson et al. (2020) argue that ocean circulation and upwelling can materially affect biological productivity and biosignature strength, with slower rotation, higher surface pressure, and some seasonal mixing states enhancing nutrient delivery.
- Herbort et al. (2024) argues that even when water condensates exist, phosphorus and metals can remain limiting for aerial biospheres.

Right now `OceanCoverage` and `LandCoverage` are carrying too much conceptual weight by themselves.

### 5. Complex-life and civilization stages still use Earth-shaped proxies too directly

The sentience and civilization logic in [BiologySupportEvaluator.cs](/D:/Game%20Creation/star_gen/src/domain/population/BiologySupportEvaluator.cs:477) and [BiologySupportEvaluator.cs](/D:/Game%20Creation/star_gen/src/domain/population/BiologySupportEvaluator.cs:504) relies strongly on:

- land coverage,
- breathable atmosphere,
- age windows,
- stability windows.

Those are directionally sensible, but they still conflate three different questions:

1. can a large biosphere persist,
2. can oxygenation or other high-energy metabolisms develop,
3. can a technosphere become remotely detectable.

Balbi and Frank (2023) supports the idea that technospheres may need stronger oxygen-rich conditions than complex life alone, but the current model still treats oxygen primarily as a late multiplier rather than the result of biosphere-climate history.

### 6. Life existence and life detectability are not separated cleanly enough

Krissansen-Totton et al. (2018) and Meadows et al. (2018) both argue that biosignatures need planetary context and that oxygen or methane cannot be interpreted in isolation.

StarGen's current model mostly asks whether life could exist and whether civilizations arise. It does not yet keep a clean distinction between:

- subsurface or weak biospheres,
- surface productive biospheres,
- oxygenated biospheres,
- atmospheres likely to yield remotely detectable disequilibrium.

That is a scientific grounding gap, not just a UI gap.

## Research anchors added in this pass

- Luger, R., & Barnes, R. (2015). *Extreme Water Loss and Abiotic O2 Buildup On Planets Throughout the Habitable Zones of M Dwarfs*. [arXiv:1411.7412](https://arxiv.org/abs/1411.7412)
- Rimmer, P. B., Xu, J., Thompson, S. J., Gillen, E., Sutherland, J. D., & Queloz, D. (2018). *The origin of RNA precursors on exoplanets*. [PubMed 30083602](https://pubmed.ncbi.nlm.nih.gov/30083602/)
- Olson, S. L., Jansen, M., & Abbot, D. S. (2020). *Oceanographic Considerations for Exoplanet Life Detection*. [arXiv:1909.02928](https://arxiv.org/abs/1909.02928)
- Herbort, O., Woitke, P., Helling, C., & Zerkle, A. L. (2024). *Habitability constraints by nutrient availability in atmospheres of rocky exoplanets*. [arXiv:2404.04029](https://arxiv.org/abs/2404.04029)
- Lingam, M., & Loeb, A. (2018). *Is Life Most Likely Around Sun-like Stars?* [arXiv:1710.11134](https://arxiv.org/abs/1710.11134)
- Krissansen-Totton, J., Olson, S., & Catling, D. C. (2018). *Disequilibrium biosignatures over Earth history and implications for detecting exoplanet life*. [arXiv:1801.08211](https://arxiv.org/abs/1801.08211)
- Meadows, V. S. et al. (2018). *Exoplanet Biosignatures: Understanding Oxygen as a Biosignature in the Context of Its Environment*. [PMCID PMC6014580](https://pmc.ncbi.nlm.nih.gov/articles/PMC6014580/)

## Tightening plan

### 1. Replace the single "stability window" with stage-specific channels

Add a deterministic `LifeOpportunityState` derived from `PlanetEnvironmentProfile` and host-star history. Keep it lightweight and generator-friendly.

Recommended channels:

- `AbiogenesisOpportunity`: water availability, redox opportunity, prebiotic UV adequacy, wet-dry or shoreline opportunity, and minimum nutrient access.
- `BiospherePersistence`: long-duration solvent availability, climate stability, radiation and XUV survivability, and atmospheric retention.
- `SurfaceProductivity`: nutrient cycling, mixed land-ocean access, pressure and circulation support, and light availability.
- `OxygenationPotential`: long-lived surface productivity, land fraction, weathering or burial proxy, and time.
- `DetectabilityPotential`: disequilibrium context, atmospheric column mass, false-positive risk, and whether the biosphere is surface-expressive versus hidden.

This is the single highest-value change because it lets the existing stage outputs remain separate for scientifically different reasons.

### 2. Add explicit host-star-history penalties and opportunities

Extend `PlanetEnvironmentProfile` or a life-side derived state with:

- `EarlyDesiccationRisk`
- `AbioticOxygenFalsePositiveRisk`
- `PrebioticUvAdequacy`
- `AtmosphericErosionRisk`

Suggested use:

- M-dwarf habitable-zone worlds with high early desiccation risk should take a strong abiogenesis and long-term surface-habitability penalty even if present-day temperatures look favorable.
- High abiotic oxygen risk should not increase complex-life odds and should instead reduce confidence in oxygen-rich biosphere interpretation.
- K- and G-dwarf contexts should often score better than M dwarfs for exposed, surface-complex biospheres, consistent with Lingam and Loeb (2018) and Arney (2019).

### 3. Split surface life from subsurface or cryptic life

Today the model mostly treats "biology support" as one branch. That is too coarse.

Add at least two deterministic branches:

- `ProtectedBiosphereSupport`
- `SurfaceBiosphereSupport`

Examples:

- icy moons with tidal heating and subsurface oceans can score well on protected biospheres without scoring well on oxygenation or technosphere emergence,
- desiccated or high-radiation worlds may fail surface biospheres while still permitting narrow protected niches,
- aerial-biosphere candidates should use a separate nutrient gate instead of borrowing surface-ocean assumptions.

### 4. Make nutrient access a first-class control in the life model

Add a `NutrientAccessibility` proxy rather than using only ocean or land fractions.

Recommended contributors:

- mixed land-ocean interface,
- ocean circulation or upwelling proxy,
- volcanism or weathering supply,
- atmospheric or cloud nutrient limitations,
- ice-locked nutrient sequestration penalty.

Use Olson et al. (2020) to motivate ocean-mixing and circulation effects and Herbort et al. (2024) to justify phosphorus and metal scarcity penalties for aerial biospheres.

### 5. Treat oxygenation as a late biosphere outcome, not an initial habitability gate

Keep Balbi and Frank (2023) for technospheres, but do not let oxygen stand in for generic biological success.

Suggested model order:

1. abiogenesis,
2. durable biosphere,
3. productive surface biosphere,
4. oxygenation-capable biosphere,
5. complex multicellular biosphere,
6. sentient lineage,
7. technosphere.

This better matches both Earth history and exoplanet biosignature literature than the current shortcut where breathable atmosphere enters several earlier stages directly.

### 6. Add a detectability layer distinct from life probability

Add an explicit `BiosignatureContextAssessment` that can be high, medium, or low confidence.

This should use:

- methane plus carbon dioxide disequilibrium potential,
- oxygen false-positive risk,
- atmospheric CO context where available,
- whether the biosphere is likely subsurface, sparse, or globally productive.

Krissansen-Totton et al. (2018) and Meadows et al. (2018) justify this split. A world can host life but still be a poor remote life-detection target.

## Proposed implementation order

1. Add `LifeOpportunityState` with stage-specific derived fields and tests.
2. Move `LifePotentialModeling.Resolve(...)` from generic multipliers toward channel-aware tuning that consumes those fields.
3. Refactor `BiologySupportEvaluator` to use separate abiogenesis, persistence, oxygenation, and detectability helpers.
4. Add host-star-history fields using already available stellar age, XUV, and spectral-class context.
5. Add nutrient-access proxies and separate surface versus protected biosphere branches.
6. Expand source and tooltip surfaces only after the generator behavior is real.

## Recommendation

Do not replace StarGen's deterministic life model with a grand unified astrobiology simulator. The tighter scientific path is to keep the current staged architecture but replace the broad multipliers with a small number of literature-backed environmental-history channels.

That preserves usability while making the model more defensible:

- less Earth-default by accident,
- less M-dwarf optimism by omission,
- less conflation of life with detectability,
- and more explicit about where StarGen is science-backed versus where it remains a bounded surrogate.
